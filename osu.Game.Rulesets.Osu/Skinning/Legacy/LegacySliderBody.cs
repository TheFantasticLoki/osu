// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Skinning;
using osu.Game.Utils;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public partial class LegacySliderBody : PlaySliderBody
    {
        protected override DrawableSliderPath CreateSliderPath() => new LegacyDrawableSliderPath();

        protected override Color4 GetBorderColour(ISkinSource skin)
            => skin.GetConfig<OsuSkinColour, Color4>(OsuSkinColour.SliderBorder)?.Value ?? Color4.White;

        protected override Color4 GetBodyAccentColour(ISkinSource skin, Color4 hitObjectAccentColour)
            // legacy skins use a constant value for slider track alpha, regardless of the source colour.
            => (skin.GetConfig<OsuSkinColour, Color4>(OsuSkinColour.SliderTrackOverride)?.Value ?? hitObjectAccentColour).Opacity(0.7f);

        private partial class LegacyDrawableSliderPath : DrawableSliderPath
        {
            protected override Color4 ColourAt(float position)
            {
                // https://github.com/peppy/osu-stable-reference/blob/3ea48705eb67172c430371dcfc8a16a002ed0d3d/osu!/Graphics/Renderers/MmSliderRendererGL.cs#L59-L70
                const float shadow_portion = 1 - (OsuLegacySkinTransformer.LEGACY_CIRCLE_RADIUS / OsuHitObject.OBJECT_RADIUS);
                const float border_portion = 0.1875f;

                // Shadow zone (always present in legacy rendering).
                if (position <= shadow_portion)
                {
                    Color4 shadow = new Color4(0, 0, 0, 0.25f);
                    return LegacyUtils.InterpolateNonLinear(position, Color4.Black.Opacity(0f), shadow, 0, shadow_portion);
                }

                // Border zone: respect custom border style/lightness.
                if (position <= border_portion)
                {
                    Color4 borderColour = BorderColour;

                    if (BorderStyle != SliderBorderStyle.Skin)
                    {
                        float l = BorderLightness;
                        borderColour = l >= 0 ? borderColour.Lighten(l) : borderColour.Darken(-l);
                    }

                    return borderColour;
                }

                // Body zone: apply body style and opacity.
                float bodyPosition = (position - border_portion) / (1f - border_portion);

                switch (BodyStyle)
                {
                    case SliderBodyStyle.GlowingEdge:
                    {
                        Color4 edgeColour = AccentColour.Darken(0.1f);
                        float gw = Math.Clamp(GlowWidth, 0.01f, 1f);
                        float t = Math.Clamp(bodyPosition / gw, 0f, 1f);
                        float alpha = (1f - t) * AccentColour.A * BodyOpacity;
                        return new Color4(edgeColour.R, edgeColour.G, edgeColour.B, alpha);
                    }

                    case SliderBodyStyle.SolidFill:
                    {
                        Color4 centreColour = AccentColour.Lighten(0.5f);
                        return new Color4(centreColour.R, centreColour.G, centreColour.B, 0.8f * AccentColour.A * BodyOpacity);
                    }

                    case SliderBodyStyle.InvertedGradient:
                    {
                        Color4 edgeColour = AccentColour.Darken(0.1f);
                        Color4 centreColour = AccentColour.Lighten(0.5f);
                        float alpha = 0.8f * bodyPosition * AccentColour.A * BodyOpacity;
                        return new Color4(
                            edgeColour.R + (centreColour.R - edgeColour.R) * bodyPosition,
                            edgeColour.G + (centreColour.G - edgeColour.G) * bodyPosition,
                            edgeColour.B + (centreColour.B - edgeColour.B) * bodyPosition,
                            alpha);
                    }

                    default:
                    {
                        // Classic legacy gradient, with opacity support.
                        Color4 outerColour = AccentColour.Darken(0.1f);
                        Color4 innerColour = lighten(AccentColour, 0.5f);
                        Color4 result = LegacyUtils.InterpolateNonLinear(position, outerColour, innerColour, border_portion, 1);
                        return new Color4(result.R, result.G, result.B, result.A * BodyOpacity);
                    }
                }
            }

            /// <summary>
            /// Lightens a colour in a way more friendly to dark or strong colours.
            /// </summary>
            private static Color4 lighten(Color4 color, float amount)
            {
                amount *= 0.5f;
                return new Color4(
                    Math.Min(1, color.R * (1 + 0.5f * amount) + 1 * amount),
                    Math.Min(1, color.G * (1 + 0.5f * amount) + 1 * amount),
                    Math.Min(1, color.B * (1 + 0.5f * amount) + 1 * amount),
                    color.A);
            }
        }
    }
}
