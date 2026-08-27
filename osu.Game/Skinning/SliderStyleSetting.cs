// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Configuration values controlling slider rendering for a skin.
    /// Values are resolved through <see cref="ISkinSource"/>, allowing beatmap skins
    /// to override user skin values.
    /// </summary>
    public enum SliderStyleSetting
    {
        StylesEnabled,
        UseComboColour,
        BodyStyle,
        BorderStyle,
        BorderColour,
        BorderLightness,
        BodyOpacity,
        GlowWidth,
    }

    /// <summary>
    /// Utility for parsing <see cref="SliderStyleSetting"/> values stored as strings
    /// in a skin's <see cref="SkinConfiguration.ConfigDictionary"/>.
    /// </summary>
    public static class SliderStyleSettingParser
    {
        /// <summary>
        /// Parse a stored string value into <typeparamref name="TValue"/>.
        /// Returns <c>false</c> if the value cannot be parsed.
        /// </summary>
        public static bool TryParse<TValue>(string value, out TValue parsed)
            where TValue : notnull
        {
            parsed = default!;

            try
            {
                if (typeof(TValue) == typeof(bool))
                {
                    // handles both "True"/"False" and "1"/"0" representations.
                    if (bool.TryParse(value, out bool boolValue))
                        parsed = (TValue)(object)boolValue;
                    else
                        parsed = (TValue)(object)(Convert.ToInt32(value, CultureInfo.InvariantCulture) != 0);

                    return true;
                }

                if (typeof(TValue).IsEnum)
                {
                    parsed = (TValue)Enum.Parse(typeof(TValue), value, ignoreCase: true);
                    return true;
                }

                if (typeof(TValue) == typeof(Colour4) || typeof(TValue) == typeof(Color4))
                {
                    string[] components = value.Split(',');
                    if (components.Length >= 3
                        && byte.TryParse(components[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out byte red)
                        && byte.TryParse(components[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out byte green)
                        && byte.TryParse(components[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out byte blue))
                    {
                        if (typeof(TValue) == typeof(Colour4))
                            parsed = (TValue)(object)new Colour4(red, green, blue, 255);
                        else
                            parsed = (TValue)(object)new Color4(red, green, blue, 255);

                        return true;
                    }

                    return false;
                }

                var bindable = new Bindable<TValue>();
                bindable.Parse(value, CultureInfo.InvariantCulture);
                parsed = bindable.Value;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
