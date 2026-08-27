// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    /// <summary>
    /// The default slider body gradient: opaque edge (0.8 alpha) fading to transparent centre (0.3 alpha).
    /// Supports configurable body styles, border styles, opacity, and glow width
    /// via properties inherited from <see cref="DrawableSliderPath"/>.
    /// </summary>
    public partial class DefaultDrawableSliderPath : DrawableSliderPath
    {
        private const float opacity_at_centre = 0.3f;
        private const float opacity_at_edge = 0.8f;

        protected override Color4 ColourAt(float position)
        {
            if (CalculatedBorderPortion != 0f && position <= CalculatedBorderPortion)
            {
                Color4 borderColour = BorderColour;

                if (BorderStyle != SliderBorderStyle.Skin)
                {
                    float l = BorderLightness;
                    borderColour = l >= 0 ? borderColour.Lighten(l) : borderColour.Darken(-l);
                }

                return borderColour;
            }

            position -= CalculatedBorderPortion;

            switch (BodyStyle)
            {
                case SliderBodyStyle.GlowingEdge:
                    return glowingEdgeAt(position / GRADIENT_PORTION);

                case SliderBodyStyle.SolidFill:
                    return solidFillAt();

                case SliderBodyStyle.InvertedGradient:
                    return invertedGradientAt(position / GRADIENT_PORTION);

                default:
                    return defaultAt(position);
            }
        }

        /// <summary>
        /// Classic gradient: opaque edge (0.8 alpha) fading to transparent centre (0.3 alpha).
        /// </summary>
        private Color4 defaultAt(float bodyPosition)
        {
            return new Color4(
                AccentColour.R,
                AccentColour.G,
                AccentColour.B,
                (opacity_at_edge - (opacity_at_edge - opacity_at_centre) * bodyPosition / GRADIENT_PORTION) * AccentColour.A * BodyOpacity);
        }

        private Color4 glowingEdgeAt(float bodyPosition)
        {
            Color4 edgeColour = AccentColour.Darken(0.1f);
            float gw = Math.Clamp(GlowWidth, 0.01f, 1f);
            float t = Math.Clamp(bodyPosition / gw, 0f, 1f);
            float alpha = (1f - t) * AccentColour.A * BodyOpacity;

            return new Color4(edgeColour.R, edgeColour.G, edgeColour.B, alpha);
        }

        private Color4 solidFillAt()
        {
            Color4 centreColour = AccentColour.Lighten(0.5f);

            return new Color4(centreColour.R, centreColour.G, centreColour.B, 0.8f * AccentColour.A * BodyOpacity);
        }

        private Color4 invertedGradientAt(float bodyPosition)
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
    }
}
