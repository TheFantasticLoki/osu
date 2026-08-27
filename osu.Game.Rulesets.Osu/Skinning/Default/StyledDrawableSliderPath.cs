// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

// NOTE: This file is kept as reference for the original plugin-style path-swap approach.
// Styling properties now live on DrawableSliderPath (base class) and are pushed
// directly by PlaySliderBody — no path replacement needed.
// This class is unused and can be deleted once the feature is stable.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    /// <summary>
    /// (UNUSED) Former plugin-style path replacement. Kept as reference only.
    /// </summary>
    public partial class StyledDrawableSliderPath : DrawableSliderPath
    {
        protected override Color4 ColourAt(float position)
        {
            if (CalculatedBorderPortion > 0f && position <= CalculatedBorderPortion)
            {
                Color4 borderColour = BorderColour;

                if (BorderStyle != SliderBorderStyle.Skin)
                {
                    float l = BorderLightness;
                    borderColour = l >= 0 ? borderColour.Lighten(l) : borderColour.Darken(-l);
                }

                return borderColour;
            }

            // Use the same normalisation as DefaultDrawableSliderPath for exact visual parity.
            float bodyPosition = position - CalculatedBorderPortion;

            switch (BodyStyle)
            {
                case SliderBodyStyle.GlowingEdge:
                    return glowingEdgeAt(bodyPosition / GRADIENT_PORTION);

                case SliderBodyStyle.SolidFill:
                    return solidFillAt();

                case SliderBodyStyle.InvertedGradient:
                    return invertedGradientAt(bodyPosition / GRADIENT_PORTION);

                default:
                    return defaultAt(bodyPosition);
            }
        }

        private Color4 defaultAt(float bodyPosition)
        {
            const float opacity_at_centre = 0.3f;
            const float opacity_at_edge = 0.8f;

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
            float alpha = (1f - t) * BodyOpacity;

            return new Color4(edgeColour.R, edgeColour.G, edgeColour.B, alpha);
        }

        private Color4 solidFillAt()
        {
            Color4 centreColour = Lighten(AccentColour, 0.5f);

            return new Color4(centreColour.R, centreColour.G, centreColour.B, 0.8f * BodyOpacity);
        }

        private Color4 invertedGradientAt(float bodyPosition)
        {
            Color4 edgeColour = AccentColour.Darken(0.1f);
            Color4 centreColour = Lighten(AccentColour, 0.5f);
            float alpha = 0.8f * bodyPosition * BodyOpacity;

            return new Color4(
                edgeColour.R + (centreColour.R - edgeColour.R) * bodyPosition,
                edgeColour.G + (centreColour.G - edgeColour.G) * bodyPosition,
                edgeColour.B + (centreColour.B - edgeColour.B) * bodyPosition,
                alpha);
        }

        private static Color4 Lighten(Color4 color, float amount)
        {
            amount *= 0.5f;
            return new Color4(
                Math.Min(1, color.R * (1 + 0.5f * amount) + amount),
                Math.Min(1, color.G * (1 + 0.5f * amount) + amount),
                Math.Min(1, color.B * (1 + 0.5f * amount) + amount),
                color.A);
        }
    }
}
