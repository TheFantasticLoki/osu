// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Edit.Components;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinEditor
{
    /// <summary>
    /// Exposes the per-skin custom colour entries (the <c>[Colours]</c> section of legacy
    /// <c>skin.ini</c> files) that are consulted by gameplay rendering, such as the slider
    /// track / border / ball, spinner background and menu glow colours.
    /// These map to the <c>osu! ruleset colour lookups</c> (e.g. <c>SliderTrackOverride</c>) /
    /// <see cref="GlobalSkinColours"/>
    /// lookups via <see cref="SkinConfiguration.CustomColours"/>.
    /// </summary>
    internal partial class SkinColourSettings : EditorSidebarSection
    {
        private readonly Skin skin;
        private readonly Action changed;

        private FillFlowContainer settingsContent = null!;

        private static readonly (string key, LocalisableString caption, LocalisableString tooltip, Color4 fallback)[] colour_entries =
        {
            (@"SliderTrackOverride", SkinEditorSettingsStrings.SliderTrackOverride, SkinEditorSettingsStrings.SliderTrackOverrideTooltip, new Color4((byte)255, (byte)255, (byte)255, (byte)255)),
            (@"SliderBorder", SkinEditorSettingsStrings.SliderBorderColourLabel, SkinEditorSettingsStrings.SliderBorderColourLabelTooltip, new Color4((byte)255, (byte)255, (byte)255, (byte)255)),
            (@"SliderBall", SkinEditorSettingsStrings.SliderBallColour, SkinEditorSettingsStrings.SliderBallColourTooltip, new Color4((byte)2, (byte)170, (byte)255, (byte)255)),
            (@"SpinnerBackground", SkinEditorSettingsStrings.SpinnerBackgroundColour, SkinEditorSettingsStrings.SpinnerBackgroundColourTooltip, new Color4((byte)0, (byte)0, (byte)0, (byte)255)),
            (@"StarBreakAdditive", SkinEditorSettingsStrings.StarBreakAdditiveColour, SkinEditorSettingsStrings.StarBreakAdditiveColourTooltip, new Color4((byte)255, (byte)255, (byte)0, (byte)255)),
            (@"InputOverlayText", SkinEditorSettingsStrings.InputOverlayTextColour, SkinEditorSettingsStrings.InputOverlayTextColourTooltip, new Color4((byte)0, (byte)0, (byte)0, (byte)255)),
            (@"MenuGlow", SkinEditorSettingsStrings.MenuGlowColour, SkinEditorSettingsStrings.MenuGlowColourTooltip, new Color4((byte)255, (byte)255, (byte)255, (byte)255)),
        };

        public SkinColourSettings(Skin skin, Action changed)
            : base(SkinEditorSettingsStrings.ColoursHeader)
        {
            this.skin = skin;
            this.changed = changed;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Content.Add(settingsContent = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new osuTK.Vector2(0, 10),
            });

            foreach (var (key, caption, tooltip, defaultColour) in colour_entries)
            {
                var bindable = new Bindable<Colour4>(readColour(key, (Colour4)defaultColour));
                bindable.ValueChanged += e => writeColour(key, e.NewValue);

                settingsContent.Add(new SettingsColour
                {
                    LabelText = caption,
                    TooltipText = tooltip,
                    Current = bindable,
                });
            }
        }

        private Colour4 readColour(string key, Colour4 fallback)
        {
            if (!skin.Configuration.CustomColours.TryGetValue(key, out Color4 stored))
                return fallback;

            return new Colour4(stored.R, stored.G, stored.B, stored.A);
        }

        private void writeColour(string key, Colour4 colour)
        {
            skin.Configuration.CustomColours[key] = new Color4(colour.R, colour.G, colour.B, colour.A);
            changed();
        }
    }
}