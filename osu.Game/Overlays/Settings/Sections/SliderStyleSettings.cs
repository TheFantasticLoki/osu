// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Skinning;

namespace osu.Game.Overlays.Settings.Sections
{
    public partial class SliderStyleSettings : SettingsSubsection
    {
        protected override LocalisableString Header => @"Slider Style";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            var customStylesEnabled = config.GetBindable<bool>(OsuSetting.CustomSliderStyles);
            var useComboColour = config.GetBindable<bool>(OsuSetting.CustomSliderUseComboColour);
            var bodyStyle = config.GetBindable<SliderBodyStyle>(OsuSetting.CustomSliderBodyStyle);
            var borderStyle = config.GetBindable<SliderBorderStyle>(OsuSetting.CustomSliderBorderStyle);
            var borderColour = config.GetBindable<Colour4>(OsuSetting.CustomSliderBorderColour);
            var borderLightness = config.GetBindable<float>(OsuSetting.CustomSliderBorderLightness);
            var bodyOpacity = config.GetBindable<float>(OsuSetting.CustomSliderBodyOpacity);
            var glowWidth = config.GetBindable<float>(OsuSetting.CustomSliderGlowWidth);

            var useComboColourItem = new SettingsItemV2(new FormCheckBox
            {
                Caption = @"Use combo colour for slider body",
                Current = useComboColour,
            });
            var bodyStyleItem = new SettingsItemV2(new FormEnumDropdown<SliderBodyStyle>
            {
                Caption = @"Body style",
                Current = bodyStyle,
            });
            var bodyOpacityItem = new SettingsItemV2(new FormSliderBar<float>
            {
                Caption = @"Body opacity",
                KeyboardStep = 0.05f,
                TransferValueOnCommit = true,
                Current = bodyOpacity,
            });
            var glowWidthItem = new SettingsItemV2(new FormSliderBar<float>
            {
                Caption = @"Glow width",
                KeyboardStep = 0.05f,
                TransferValueOnCommit = true,
                Current = glowWidth,
            });
            var borderStyleItem = new SettingsItemV2(new FormEnumDropdown<SliderBorderStyle>
            {
                Caption = @"Border style",
                Current = borderStyle,
            });
            var borderColourItem = new SettingsColour
            {
                LabelText = @"Custom border colour",
                Current = borderColour,
            };
            var borderLightnessItem = new SettingsItemV2(new FormSliderBar<float>
            {
                Caption = @"Border lightness",
                KeyboardStep = 0.05f,
                TransferValueOnCommit = true,
                Current = borderLightness,
            });

            Children = new Drawable[]
            {
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = @"Enable custom slider styles",
                    Current = customStylesEnabled,
                }),
                new OsuSpriteText
                {
                    Text = @"Body",
                    Font = OsuFont.Style.Heading2,
                    Padding = SettingsPanel.CONTENT_PADDING,
                    Margin = new MarginPadding { Top = 8 },
                },
                useComboColourItem,
                bodyStyleItem,
                bodyOpacityItem,
                glowWidthItem,
                new OsuSpriteText
                {
                    Text = @"Border",
                    Font = OsuFont.Style.Heading2,
                    Padding = SettingsPanel.CONTENT_PADDING,
                    Margin = new MarginPadding { Top = 8 },
                },
                borderStyleItem,
                borderColourItem,
                borderLightnessItem,
            };

            customStylesEnabled.BindValueChanged(enabled =>
            {
                useComboColourItem.CanBeShown.Value = enabled.NewValue;
                bodyStyleItem.CanBeShown.Value = enabled.NewValue;
                bodyOpacityItem.CanBeShown.Value = enabled.NewValue;
                glowWidthItem.CanBeShown.Value = enabled.NewValue && bodyStyle.Value == SliderBodyStyle.GlowingEdge;
                borderStyleItem.CanBeShown.Value = enabled.NewValue;
                borderColourItem.CanBeShown.Value = enabled.NewValue && borderStyle.Value == SliderBorderStyle.CustomColour;
                borderLightnessItem.CanBeShown.Value = enabled.NewValue && borderStyle.Value != SliderBorderStyle.Skin;
            }, true);

            bodyStyle.BindValueChanged(style =>
                glowWidthItem.CanBeShown.Value = customStylesEnabled.Value && style.NewValue == SliderBodyStyle.GlowingEdge, true);

            borderStyle.BindValueChanged(style =>
            {
                borderColourItem.CanBeShown.Value = customStylesEnabled.Value && style.NewValue == SliderBorderStyle.CustomColour;
                borderLightnessItem.CanBeShown.Value = customStylesEnabled.Value && style.NewValue != SliderBorderStyle.Skin;
            }, true);
        }
    }
}
