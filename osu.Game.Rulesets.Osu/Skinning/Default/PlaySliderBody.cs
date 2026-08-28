// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
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

        private readonly Bindable<bool> configSnakingOut = new Bindable<bool>();

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
            foreach (ISkin source in skin.AllSources)
            {
                // Sources may be transformers wrapping the actual skin (e.g. OsuLegacySkinTransformer),
                // so unwrap to find the underlying Skin instance which owns the configuration.
                Skin? skinInstance = source switch
                {
                    Skin s => s,
                    ISkinTransformer transformer => transformer.Skin as Skin,
                    _ => null,
                };

                if (skinInstance == null)
                    continue;

                skinInstance.Configuration.ConfigDictionary.BindCollectionChanged(onConfigDictionaryChanged);
                subscribedSkinConfigurations.Add(skinInstance.Configuration);
            }
        }

        private void unsubscribeFromSkinConfigurations()
        {
            foreach (SkinConfiguration configuration in subscribedSkinConfigurations)
                configuration.ConfigDictionary.CollectionChanged -= onConfigDictionaryChanged;

            subscribedSkinConfigurations.Clear();
        }

        private void onConfigDictionaryChanged(object? sender, NotifyDictionaryChangedEventArgs<string, string> e)
            => Scheduler.AddOnce(pushAllStylesToPath);

        protected override void Dispose(bool isDisposing)
        {
            if (skin != null)
            {
                skin.SourceChanged -= onSkinSourceChanged;
                unsubscribeFromSkinConfigurations();
            }

            base.Dispose(isDisposing);
        }

        private bool stylesEnabled => skin.GetConfig<SliderStyleSetting, bool>(SliderStyleSetting.StylesEnabled)?.Value == true;

        /// <summary>
        /// Push all current style settings directly to the <see cref="DefaultDrawableSliderPath"/>.
        /// No path replacement — the path type stays whatever the skin created.
        /// </summary>
        private void pushAllStylesToPath()
        {
            if (Path is not DrawableSliderPath sliderPath)
                return;

            if (stylesEnabled)
            {
                // Resolve accent colour.
                if (getSkinValue(SliderStyleSetting.UseComboColour, false))
                    AccentColour = rawComboColour;
                else
                    AccentColour = GetBodyAccentColour(skin, rawComboColour);

                // Resolve border colour based on border style.
                SliderBorderStyle borderStyle = getSkinValue(SliderStyleSetting.BorderStyle, SliderBorderStyle.Skin);
                switch (borderStyle)
                {
                    case SliderBorderStyle.ComboColour:
                        BorderColour = rawComboColour;
                        break;

                    case SliderBorderStyle.CustomColour:
                        BorderColour = getSkinValue(SliderStyleSetting.BorderColour, (Colour4)Color4.White);
                        break;

                    default:
                        BorderColour = GetBorderColour(skin);
                        break;
                }

                sliderPath.BodyStyle = getSkinValue(SliderStyleSetting.BodyStyle, SliderBodyStyle.Default);
                sliderPath.BorderStyle = borderStyle;
                sliderPath.BorderLightness = getSkinValue(SliderStyleSetting.BorderLightness, 0f);
                sliderPath.BodyOpacity = getSkinValue(SliderStyleSetting.BodyOpacity, 1f);
                sliderPath.GlowWidth = getSkinValue(SliderStyleSetting.GlowWidth, 0.5f);
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

        private TValue getSkinValue<TValue>(SliderStyleSetting setting, TValue defaultValue)
            where TValue : notnull
        {
            IBindable<TValue>? value = skin.GetConfig<SliderStyleSetting, TValue>(setting);
            return value == null ? defaultValue : value.Value;
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
