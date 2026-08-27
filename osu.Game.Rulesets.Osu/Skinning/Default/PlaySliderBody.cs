// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public abstract partial class PlaySliderBody : SnakingSliderBody
    {
        protected IBindable<float> ScaleBindable { get; private set; } = null!;

        protected IBindable<Color4> AccentColourBindable { get; private set; } = null!;

        private IBindable<int> pathVersion = null!;

        [Resolved(CanBeNull = true)]
        private OsuRulesetConfigManager? config { get; set; }

        [Resolved(CanBeNull = true)]
        private OsuConfigManager? osuConfig { get; set; }

        private readonly Bindable<bool> configSnakingOut = new Bindable<bool>();

        private Bindable<bool> customSliderStylesEnabled = null!;

        private Bindable<bool> useComboColour = null!;

        private Bindable<SliderBodyStyle> bodyStyleBindable = null!;
        private Bindable<SliderBorderStyle> borderStyleBindable = null!;
        private Bindable<Colour4> borderColourBindable = null!;
        private Bindable<float> borderLightnessBindable = null!;
        private Bindable<float> bodyOpacityBindable = null!;
        private Bindable<float> glowWidthBindable = null!;

        /// <summary>
        /// The raw combo colour from the beatmap, before any skin transformations.
        /// </summary>
        private Color4 rawComboColour = Color4.White;

        private ISkinSource skin = null!;
        private readonly List<SkinConfiguration> subscribedSkinConfigurations = new List<SkinConfiguration>();

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, DrawableHitObject drawableObject)
        {
            var drawableSlider = (DrawableSlider)drawableObject;

            this.skin = skin;
            subscribeToSkinConfigurations();

            ScaleBindable = drawableSlider.ScaleBindable.GetBoundCopy();
            ScaleBindable.BindValueChanged(scale => PathRadius = OsuHitObject.OBJECT_RADIUS * scale.NewValue, true);

            pathVersion = drawableSlider.PathVersion.GetBoundCopy();
            pathVersion.BindValueChanged(_ => Scheduler.AddOnce(Refresh));

            AccentColourBindable = drawableObject.AccentColour.GetBoundCopy();
            AccentColourBindable.BindValueChanged(accent =>
            {
                rawComboColour = accent.NewValue;
                pushAllStylesToPath();
            }, true);

            config?.BindWith(OsuRulesetSetting.SnakingInSliders, SnakingIn);
            config?.BindWith(OsuRulesetSetting.SnakingOutSliders, configSnakingOut);

            SnakingOut.BindTo(configSnakingOut);

            // Resolve border colour based on border style.
            Color4 borderColour = GetBorderColour(skin);
            BorderColour = borderColour;

            if (config != null && osuConfig != null)
            {
                customSliderStylesEnabled = osuConfig.GetBindable<bool>(OsuSetting.CustomSliderStyles);
                useComboColour = osuConfig.GetBindable<bool>(OsuSetting.CustomSliderUseComboColour);

                bodyStyleBindable = osuConfig.GetBindable<SliderBodyStyle>(OsuSetting.CustomSliderBodyStyle);
                borderStyleBindable = osuConfig.GetBindable<SliderBorderStyle>(OsuSetting.CustomSliderBorderStyle);
                borderColourBindable = osuConfig.GetBindable<Colour4>(OsuSetting.CustomSliderBorderColour);
                borderLightnessBindable = osuConfig.GetBindable<float>(OsuSetting.CustomSliderBorderLightness);
                bodyOpacityBindable = osuConfig.GetBindable<float>(OsuSetting.CustomSliderBodyOpacity);
                glowWidthBindable = osuConfig.GetBindable<float>(OsuSetting.CustomSliderGlowWidth);

                // Wire all settings directly to the path — no path swapping needed.
                customSliderStylesEnabled.BindValueChanged(_ => pushAllStylesToPath(), true);
                bodyStyleBindable.BindValueChanged(_ => pushAllStylesToPath(), true);
                borderStyleBindable.BindValueChanged(_ => pushAllStylesToPath(), true);
                borderColourBindable.BindValueChanged(_ => pushAllStylesToPath(), true);
                borderLightnessBindable.BindValueChanged(_ => pushAllStylesToPath(), true);
                bodyOpacityBindable.BindValueChanged(_ => pushAllStylesToPath(), true);
                glowWidthBindable.BindValueChanged(_ => pushAllStylesToPath(), true);
                useComboColour.BindValueChanged(_ => pushAllStylesToPath(), true);
            }

            skin.SourceChanged += onSkinSourceChanged;
        }

        private void onSkinSourceChanged()
        {
            unsubscribeFromSkinConfigurations();
            subscribeToSkinConfigurations();
            Scheduler.AddOnce(pushAllStylesToPath);
        }

        private void subscribeToSkinConfigurations()
        {
            foreach (Skin source in skin.AllSources.OfType<Skin>())
            {
                source.Configuration.Changed += onSkinConfigurationChanged;
                subscribedSkinConfigurations.Add(source.Configuration);
            }
        }

        private void unsubscribeFromSkinConfigurations()
        {
            foreach (SkinConfiguration configuration in subscribedSkinConfigurations)
                configuration.Changed -= onSkinConfigurationChanged;

            subscribedSkinConfigurations.Clear();
        }

        private void onSkinConfigurationChanged()
        {
            Scheduler.AddOnce(pushAllStylesToPath);
        }

        protected override void Dispose(bool isDisposing)
        {
            skin.SourceChanged -= onSkinSourceChanged;
            unsubscribeFromSkinConfigurations();
            base.Dispose(isDisposing);
        }

        private bool stylesEnabled => customSliderStylesEnabled?.Value == true
                          || skin?.GetConfig<SliderStyleSetting, bool>(SliderStyleSetting.StylesEnabled)?.Value == true;

        /// <summary>
        /// Push all current style settings directly to the <see cref="DefaultDrawableSliderPath"/>.
        /// No path replacement — the path type stays whatever the skin created.
        /// </summary>
        private void pushAllStylesToPath()
        {
            if (Path is not DrawableSliderPath sliderPath)
                return;

            if (bodyStyleBindable == null)
                return;

            if (stylesEnabled)
            {
                // Resolve accent colour.
                if (getSkinValue(SliderStyleSetting.UseComboColour, useComboColour.Value))
                    AccentColour = rawComboColour;
                else
                    AccentColour = GetBodyAccentColour(skin, rawComboColour);

                // Resolve border colour based on border style.
                SliderBorderStyle borderStyle = getSkinValue(SliderStyleSetting.BorderStyle, borderStyleBindable.Value);
                switch (borderStyle)
                {
                    case SliderBorderStyle.ComboColour:
                        BorderColour = rawComboColour;
                        break;

                    case SliderBorderStyle.CustomColour:
                        BorderColour = getSkinValue(SliderStyleSetting.BorderColour, borderColourBindable.Value);
                        break;

                    default:
                        BorderColour = GetBorderColour(skin);
                        break;
                }

                sliderPath.BodyStyle = getSkinValue(SliderStyleSetting.BodyStyle, bodyStyleBindable.Value);
                sliderPath.BorderStyle = borderStyle;
                sliderPath.BorderLightness = getSkinValue(SliderStyleSetting.BorderLightness, borderLightnessBindable.Value);
                sliderPath.BodyOpacity = getSkinValue(SliderStyleSetting.BodyOpacity, bodyOpacityBindable.Value);
                sliderPath.GlowWidth = getSkinValue(SliderStyleSetting.GlowWidth, glowWidthBindable.Value);
            }
            else
            {
                // Reset to vanilla defaults when styles are disabled.
                AccentColour = GetBodyAccentColour(skin, rawComboColour);
                BorderColour = GetBorderColour(skin);
                sliderPath.BodyStyle = SliderBodyStyle.Default;
                sliderPath.BorderStyle = SliderBorderStyle.Skin;
                sliderPath.BorderLightness = 0f;
                sliderPath.BodyOpacity = 1f;
                sliderPath.GlowWidth = 0.5f;
            }
        }

        private TValue getSkinValue<TValue>(SliderStyleSetting setting, TValue fallback)
            where TValue : notnull
        {
            IBindable<TValue>? value = skin.GetConfig<SliderStyleSetting, TValue>(setting);
            return value == null ? fallback : value.Value;
        }

        public override void RecyclePath()
        {
            base.RecyclePath();

            // After recycling, the path is a fresh instance with default properties.
            // Re-push current style settings to the new path, but only after load() has run (skin is resolved).
            if (skin != null)
                pushAllStylesToPath();
        }

        protected virtual Color4 GetBorderColour(ISkinSource skin) => Color4.White;

        protected virtual Color4 GetBodyAccentColour(ISkinSource skin, Color4 hitObjectAccentColour) => hitObjectAccentColour;
    }
}
