// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;
using osuTK;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public partial class RendererSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GraphicsSettingsStrings.RendererHeader;

        private bool automaticRendererInUse;

        private FillFlowContainer otherThreadsContainer = null!;
        private FillFlowContainer customLimiterContainer = null!;
        private SettingsItemV2 syncCheckboxItem = null!;

        private readonly Bindable<SettingsNote.Data?> customLimiterNote = new Bindable<SettingsNote.Data?>();

        private Bindable<FrameSync> frameSync = null!;
        private Bindable<ExecutionMode> executionMode = null!;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config, OsuConfigManager osuConfig, IDialogOverlay? dialogOverlay, OsuGame? game, GameHost host)
        {
            var renderer = config.GetBindable<RendererType>(FrameworkSetting.Renderer);
            automaticRendererInUse = renderer.Value == RendererType.Automatic;

            IEnumerable<RendererType> availableRenderers = host.GetPreferredRenderersForCurrentPlatform().Order();

            // Vulkan renderers are pretty broken to the point it may result in a startup crash at worst.
            // If a user isn't already using it let's hide it until we can fix.
            if (renderer.Value != RendererType.Deferred_Vulkan)
                availableRenderers = availableRenderers.Where(t => t != RendererType.Deferred_Vulkan);
            if (renderer.Value != RendererType.Vulkan)
                availableRenderers = availableRenderers.Where(t => t != RendererType.Vulkan);

            frameSync = config.GetBindable<FrameSync>(FrameworkSetting.FrameSync);
            executionMode = config.GetBindable<ExecutionMode>(FrameworkSetting.ExecutionMode);

            Children = new Drawable[]
            {
                new SettingsItemV2(new RendererDropdown
                {
                    Caption = GraphicsSettingsStrings.Renderer,
                    Current = renderer,
                    Items = availableRenderers,
                })
                {
                    Keywords = new[] { @"compatibility", @"directx" },
                },
                // TODO: this needs to be a custom dropdown at some point
                new SettingsItemV2(new FormEnumDropdown<FrameSync>
                {
                    Caption = GraphicsSettingsStrings.FrameLimiter,
                    Current = frameSync,
                })
                {
                    Keywords = new[] { @"fps", @"framerate" },
                },
                customLimiterContainer = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5),
                    LayoutDuration = 200,
                    LayoutEasing = Easing.OutQuint,
                    Children = new Drawable[]
                    {
                        new SettingsNote
                        {
                            RelativeSizeAxes = Axes.X,
                            Current = { Value = new SettingsNote.Data(GraphicsSettingsStrings.CustomLimiterUseAtOwnRisk, SettingsNote.Type.Critical) },
                        },
                        syncCheckboxItem = new SettingsItemV2(new FormCheckBox
                        {
                            Caption = GraphicsSettingsStrings.SyncAllThreads,
                            HintText = GraphicsSettingsStrings.SyncAllThreadsHint,
                            Current = host.SyncThreadLimits,
                        })
                        {
                            Keywords = new[] { @"sync", @"threads", @"limit", @"all" },
                            Note = { BindTarget = customLimiterNote },
                        },
                        new SettingsItemV2(createLimiterSlider(GraphicsSettingsStrings.InputThread, GraphicsSettingsStrings.InputThreadHint, host.CustomInputThreadLimit))
                        {
                            Keywords = new[] { @"input", @"thread", @"fps", @"limit", @"custom" },
                        },
                        otherThreadsContainer = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 5),
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new SettingsItemV2(createLimiterSlider(GraphicsSettingsStrings.AudioThread, GraphicsSettingsStrings.AudioThreadHint, host.CustomAudioThreadLimit))
                                {
                                    Keywords = new[] { @"audio", @"thread", @"fps", @"limit", @"custom" },
                                },
                                new SettingsItemV2(createLimiterSlider(GraphicsSettingsStrings.UpdateThread, GraphicsSettingsStrings.UpdateThreadHint, host.CustomUpdateThreadLimit))
                                {
                                    Keywords = new[] { @"update", @"thread", @"fps", @"limit", @"custom" },
                                },
                                new SettingsItemV2(createLimiterSlider(GraphicsSettingsStrings.DrawThread, GraphicsSettingsStrings.DrawThreadHint, host.CustomDrawThreadLimit))
                                {
                                    Keywords = new[] { @"draw", @"thread", @"fps", @"limit", @"custom" },
                                },
                            }
                        },
                    }
                },
                new SettingsItemV2(new FormEnumDropdown<ExecutionMode>
                {
                    Caption = GraphicsSettingsStrings.ThreadingMode,
                    Current = executionMode,
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = GraphicsSettingsStrings.ShowFPS,
                    Current = osuConfig.GetBindable<bool>(OsuSetting.ShowFpsDisplay),
                })
                {
                    Keywords = new[] { @"framerate", @"counter" },
                },
            };

            frameSync.BindValueChanged(_ => updateCustomLimiterVisibility());
            executionMode.BindValueChanged(_ => updateCustomLimiterVisibility());
            host.SyncThreadLimits.BindValueChanged(_ => updateCustomLimiterVisibility());
            updateCustomLimiterVisibility();
        }

        private void updateCustomLimiterVisibility()
        {
            bool isCustom = frameSync.Value is FrameSync.Custom;
            bool singleThreaded = executionMode.Value is ExecutionMode.SingleThread;
            bool syncAll = Host.SyncThreadLimits.Value;

            customLimiterContainer.Alpha = isCustom ? 1 : 0;

            if (isCustom)
                customLimiterContainer.AutoSizeAxes |= Axes.Y;
            else
            {
                customLimiterContainer.AutoSizeAxes &= ~Axes.Y;
                customLimiterContainer.Height = 0;
            }

            if (syncAll || singleThreaded)
            {
                otherThreadsContainer.AutoSizeAxes &= ~Axes.Y;
                otherThreadsContainer.Height = 0;
            }
            else
                otherThreadsContainer.AutoSizeAxes |= Axes.Y;

            Host.CustomInputThreadLimit.Disabled = !isCustom;
            Host.CustomAudioThreadLimit.Disabled = !isCustom;
            Host.CustomUpdateThreadLimit.Disabled = !isCustom;
            Host.CustomDrawThreadLimit.Disabled = !isCustom;

            if (!isCustom)
                customLimiterNote.Value = new SettingsNote.Data(GraphicsSettingsStrings.CustomLimiterRequiresCustomMode, SettingsNote.Type.Warning);
            else if (syncAll)
                customLimiterNote.Value = new SettingsNote.Data(GraphicsSettingsStrings.ThreadsSyncedWithInput, SettingsNote.Type.Informational);
            else
                customLimiterNote.Value = null;
        }

        [Resolved]
        private GameHost Host { get; set; } = null!;

        private static FormSliderBar<double> createLimiterSlider(LocalisableString caption, LocalisableString hintText, Bindable<double> bindable)
        {
            return new FormSliderBar<double>
            {
                Caption = caption,
                HintText = hintText,
                KeyboardStep = 50,
                TooltipFormat = v => v == 0 ? "Unlimited" : $"{v:0} FPS",
                TransferValueOnCommit = true,
                Current = { BindTarget = bindable },
            };
        }

        private partial class RendererDropdown : FormEnumDropdown<RendererType>
        {
            private RendererType hostResolvedRenderer;
            private bool automaticRendererInUse;

            [BackgroundDependencyLoader]
            private void load(FrameworkConfigManager config, GameHost host)
            {
                var renderer = config.GetBindable<RendererType>(FrameworkSetting.Renderer);
                automaticRendererInUse = renderer.Value == RendererType.Automatic;
                hostResolvedRenderer = host.ResolvedRenderer;
            }

            protected override LocalisableString GenerateItemText(RendererType item)
            {
                if (item == RendererType.Automatic && automaticRendererInUse)
                    return LocalisableString.Interpolate($"{base.GenerateItemText(item)} ({hostResolvedRenderer.GetDescription()})");

                return base.GenerateItemText(item);
            }
        }
    }
}
