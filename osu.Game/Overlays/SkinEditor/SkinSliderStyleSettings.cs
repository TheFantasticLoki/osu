// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Edit.Components;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinEditor
{
    internal partial class SkinSliderStyleSettings : EditorSidebarSection
    {
        private readonly Skin skin;
        private readonly Action changed;

        private readonly BindableBool stylesEnabled = new BindableBool();
        private readonly BindableBool useComboColour = new BindableBool();
        private readonly Bindable<SliderBodyStyle> bodyStyle = new Bindable<SliderBodyStyle>();
        private readonly Bindable<SliderBorderStyle> borderStyle = new Bindable<SliderBorderStyle>();
        private readonly Bindable<Colour4> borderColour = new Bindable<Colour4>();
        private readonly BindableFloat borderLightness = new BindableFloat
        {
            MinValue = -1,
            MaxValue = 1,
            Precision = 0.01f,
        };
        private readonly BindableFloat bodyOpacity = new BindableFloat(1f)
        {
            MinValue = 0.1f,
            MaxValue = 1,
            Precision = 0.01f,
        };
        private readonly BindableFloat glowWidth = new BindableFloat(0.5f)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.01f,
        };

        private ConditionalContainer useComboColourItem = null!;
        private ConditionalContainer bodyStyleItem = null!;
        private ConditionalContainer bodyOpacityItem = null!;
        private ConditionalContainer glowWidthItem = null!;
        private ConditionalContainer borderStyleItem = null!;
        private ConditionalContainer borderColourItem = null!;
        private ConditionalContainer borderLightnessItem = null!;
        private OsuSpriteText bodyHeader = null!;
        private OsuSpriteText borderHeader = null!;
        private FillFlowContainer settingsContent = null!;

        public SkinSliderStyleSettings(Skin skin, Action changed)
            : base(SkinEditorSettingsStrings.SliderStyleHeader)
        {
            this.skin = skin;
            this.changed = changed;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            initialiseValues();

            Content.Add(settingsContent = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new osuTK.Vector2(0, 10),
            });

            settingsContent.AddRange(new Drawable[]
            {
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = SkinEditorSettingsStrings.EnableCustomSliderStyles,
                    HintText = SkinEditorSettingsStrings.EnableCustomSliderStylesTooltip,
                    Current = stylesEnabled,
                }),
                bodyHeader = new OsuSpriteText
                {
                    Text = SkinEditorSettingsStrings.SliderBodyHeader,
                    Font = OsuFont.Style.Heading2,
                    Padding = SettingsPanel.CONTENT_PADDING,
                },
                useComboColourItem = new ConditionalContainer(new SettingsItemV2(new FormCheckBox
                {
                    Caption = SkinEditorSettingsStrings.UseComboColourBody,
                    HintText = SkinEditorSettingsStrings.UseComboColourBodyTooltip,
                    Current = useComboColour,
                })),
                bodyStyleItem = new ConditionalContainer(new SettingsItemV2(new FormEnumDropdown<SliderBodyStyle>
                {
                    Caption = SkinEditorSettingsStrings.SliderBodyStyle,
                    HintText = SkinEditorSettingsStrings.SliderBodyStyleTooltip,
                    Current = bodyStyle,
                })),
                bodyOpacityItem = new ConditionalContainer(new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = SkinEditorSettingsStrings.SliderBodyOpacity,
                    HintText = SkinEditorSettingsStrings.SliderBodyOpacityTooltip,
                    Current = bodyOpacity,
                    KeyboardStep = 0.05f,
                    TransferValueOnCommit = true,
                })),
                glowWidthItem = new ConditionalContainer(new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = SkinEditorSettingsStrings.SliderGlowWidth,
                    HintText = SkinEditorSettingsStrings.SliderGlowWidthTooltip,
                    Current = glowWidth,
                    KeyboardStep = 0.05f,
                    TransferValueOnCommit = true,
                })),
                borderHeader = new OsuSpriteText
                {
                    Text = SkinEditorSettingsStrings.SliderBorderHeader,
                    Font = OsuFont.Style.Heading2,
                    Padding = SettingsPanel.CONTENT_PADDING,
                },
                borderStyleItem = new ConditionalContainer(new SettingsItemV2(new FormEnumDropdown<SliderBorderStyle>
                {
                    Caption = SkinEditorSettingsStrings.SliderBorderStyle,
                    HintText = SkinEditorSettingsStrings.SliderBorderStyleTooltip,
                    Current = borderStyle,
                })),
                borderColourItem = new ConditionalContainer(new SettingsColour
                {
                    LabelText = SkinEditorSettingsStrings.SliderBorderColour,
                    TooltipText = SkinEditorSettingsStrings.SliderBorderColourTooltip,
                    Current = borderColour,
                }),
                borderLightnessItem = new ConditionalContainer(new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = SkinEditorSettingsStrings.SliderBorderLightness,
                    HintText = SkinEditorSettingsStrings.SliderBorderLightnessTooltip,
                    Current = borderLightness,
                    KeyboardStep = 0.05f,
                    TransferValueOnCommit = true,
                })),
            });

            stylesEnabled.ValueChanged += _ => updateVisibility();
            bodyStyle.ValueChanged += _ => updateVisibility();
            borderStyle.ValueChanged += _ => updateVisibility();

            stylesEnabled.ValueChanged += _ => writeValue(SliderStyleSetting.StylesEnabled, stylesEnabled.Value.ToString());
            useComboColour.ValueChanged += _ => writeValue(SliderStyleSetting.UseComboColour, useComboColour.Value.ToString());
            bodyStyle.ValueChanged += _ => writeValue(SliderStyleSetting.BodyStyle, bodyStyle.Value.ToString());
            borderStyle.ValueChanged += _ => writeValue(SliderStyleSetting.BorderStyle, borderStyle.Value.ToString());
            borderColour.ValueChanged += _ => writeValue(SliderStyleSetting.BorderColour, colourToString(borderColour.Value));
            borderLightness.ValueChanged += _ => writeValue(SliderStyleSetting.BorderLightness, borderLightness.Value.ToString(CultureInfo.InvariantCulture));
            bodyOpacity.ValueChanged += _ => writeValue(SliderStyleSetting.BodyOpacity, bodyOpacity.Value.ToString(CultureInfo.InvariantCulture));
            glowWidth.ValueChanged += _ => writeValue(SliderStyleSetting.GlowWidth, glowWidth.Value.ToString(CultureInfo.InvariantCulture));

            updateVisibility();
        }

        private void initialiseValues()
        {
            stylesEnabled.Value = readValue(SliderStyleSetting.StylesEnabled, false);
            useComboColour.Value = readValue(SliderStyleSetting.UseComboColour, false);
            bodyStyle.Value = readValue(SliderStyleSetting.BodyStyle, SliderBodyStyle.Default);
            borderStyle.Value = readValue(SliderStyleSetting.BorderStyle, SliderBorderStyle.Skin);
            borderColour.Value = readColour((Colour4)Color4.White);
            borderLightness.Value = readValue(SliderStyleSetting.BorderLightness, 0f);
            bodyOpacity.Value = readValue(SliderStyleSetting.BodyOpacity, 1f);
            glowWidth.Value = readValue(SliderStyleSetting.GlowWidth, 0.5f);
        }

        private void updateVisibility()
        {
            bool enabled = stylesEnabled.Value;
            bodyHeader.Alpha = enabled ? 1 : 0;
            borderHeader.Alpha = enabled ? 1 : 0;
            useComboColourItem.Shown = enabled;
            bodyStyleItem.Shown = enabled;
            bodyOpacityItem.Shown = enabled;
            glowWidthItem.Shown = enabled && bodyStyle.Value == SliderBodyStyle.GlowingEdge;
            borderStyleItem.Shown = enabled;
            borderColourItem.Shown = enabled && borderStyle.Value == SliderBorderStyle.CustomColour;
            borderLightnessItem.Shown = enabled && borderStyle.Value != SliderBorderStyle.Skin;
        }

        private TValue readValue<TValue>(SliderStyleSetting setting, TValue fallback)
            where TValue : notnull
        {
            if (!skin.Configuration.ConfigDictionary.TryGetValue(setting.ToString(), out string? value))
                return fallback;

            return SliderStyleSettingParser.TryParse(value, out TValue parsed) ? parsed : fallback;
        }

        private void writeValue(SliderStyleSetting setting, string value)
        {
            // The dictionary is a BindableDictionary, so this change notification propagates
            // to all gameplay components bound to this skin's configuration.
            skin.Configuration.ConfigDictionary[setting.ToString()] = value;
            changed();
        }

        private partial class ConditionalContainer : Container
        {
            private bool shown = true;

            public bool Shown
            {
                get => shown;
                set
                {
                    if (shown == value)
                        return;

                    shown = value;
                    Invalidate(Invalidation.Presence);
                }
            }

            public ConditionalContainer(Drawable child)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Child = child;
            }

            public override bool IsPresent => base.IsPresent && Shown;
        }

        private Colour4 readColour(Colour4 fallback)
        {
            if (!skin.Configuration.ConfigDictionary.TryGetValue(SliderStyleSetting.BorderColour.ToString(), out string? value))
                return fallback;

            string[] components = value.Split(',');
            return components.Length >= 3
                   && byte.TryParse(components[0], out byte red)
                   && byte.TryParse(components[1], out byte green)
                   && byte.TryParse(components[2], out byte blue)
                ? new Colour4(red, green, blue, 255)
                : fallback;
        }

        private static string colourToString(Colour4 colour)
            => FormattableString.Invariant($"{(int)(colour.R * 255)},{(int)(colour.G * 255)},{(int)(colour.B * 255)}");
    }
}
