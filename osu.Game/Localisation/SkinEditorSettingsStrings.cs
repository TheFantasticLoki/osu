// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    /// <summary>
    /// Localisation strings for the skin-level settings exposed in the skin layout editor.
    /// Includes all categories exposed on the "Skin" tab of <see cref="osu.Game.Overlays.SkinEditor.SkinEditorSettingsSidebar"/>.
    /// </summary>
    public static class SkinEditorSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.SkinEditorSettings";

        #region Slider Style

        /// <summary>
        /// "Slider Style"
        /// </summary>
        public static LocalisableString SliderStyleHeader => new TranslatableString(getKey(@"slider_style_header"), @"Slider Style");

        /// <summary>
        /// "Enable custom slider styles"
        /// </summary>
        public static LocalisableString EnableCustomSliderStyles => new TranslatableString(getKey(@"enable_custom_slider_styles"), @"Enable custom slider styles");

        /// <summary>
        /// "Turns on custom slider rendering for this skin. When off, sliders use the default skin styling."
        /// </summary>
        public static LocalisableString EnableCustomSliderStylesTooltip => new TranslatableString(getKey(@"enable_custom_slider_styles_tooltip"), @"Turns on custom slider rendering for this skin. When off, sliders use the default skin styling.");

        /// <summary>
        /// "Body"
        /// </summary>
        public static LocalisableString SliderBodyHeader => new TranslatableString(getKey(@"slider_body_header"), @"Body");

        /// <summary>
        /// "Use combo colour for slider body"
        /// </summary>
/// <summary>
        /// "Body opacity"
        /// </summary>
        public static LocalisableString SliderBodyOpacity => new TranslatableString(getKey(@"slider_body_opacity"), @"Body opacity");

        /// <summary>
        /// "How opaque the slider body is. Lower values make the body more see-through."
        /// </summary>
        public static LocalisableString SliderBodyOpacityTooltip => new TranslatableString(getKey(@"slider_body_opacity_tooltip"), @"How opaque the slider body is. Lower values make the body more see-through.");

        /// <summary>
        /// "Glow width"
        /// </summary>
        public static LocalisableString SliderGlowWidth => new TranslatableString(getKey(@"slider_glow_width"), @"Glow width");

        /// <summary>
        /// "The width of the glowing edge around the slider body, used with the glowing edge body style."
        /// </summary>
        public static LocalisableString SliderGlowWidthTooltip => new TranslatableString(getKey(@"slider_glow_width_tooltip"), @"The width of the glowing edge around the slider body, used with the glowing edge body style.");

        /// <summary>
        /// "Border"
        /// </summary>
        public static LocalisableString SliderBorderHeader => new TranslatableString(getKey(@"slider_border_header"), @"Border");

        /// <summary>
        /// "Border style"
        /// </summary>
        public static LocalisableString SliderBorderStyle => new TranslatableString(getKey(@"slider_border_style"), @"Border style");

        /// <summary>
        /// "Controls how the slider border is coloured. You can use the skin's own border colour, the combo colour, or a custom colour."
        /// </summary>
        public static LocalisableString SliderBorderStyleTooltip => new TranslatableString(getKey(@"slider_border_style_tooltip"), @"Controls how the slider border is coloured. You can use the skin's own border colour, the combo colour, or a custom colour.");

        /// <summary>
        /// "Custom border colour"
        /// </summary>
        public static LocalisableString SliderBorderColour => new TranslatableString(getKey(@"slider_border_colour"), @"Custom border colour");

        /// <summary>
        /// "The colour used for the slider border when the custom border style is selected."
        /// </summary>
        public static LocalisableString SliderBorderColourTooltip => new TranslatableString(getKey(@"slider_border_colour_tooltip"), @"The colour used for the slider border when the custom border style is selected.");

        /// <summary>
        /// "Border lightness"
        /// </summary>
        public static LocalisableString SliderBorderLightness => new TranslatableString(getKey(@"slider_border_lightness"), @"Border lightness");

        /// <summary>
        /// "Lightens or darkens the slider border. Negative values darken the border, positive values lighten it."
        /// </summary>
        public static LocalisableString SliderBorderLightnessTooltip => new TranslatableString(getKey(@"slider_border_lightness_tooltip"), @"Lightens or darkens the slider border. Negative values darken the border, positive values lighten it.");

        /// <summary>
        /// "Use combo colour for slider body"
        /// </summary>
        public static LocalisableString UseComboColourBody => new TranslatableString(getKey(@"use_combo_colour_body"), @"Use combo colour for slider body");

        /// <summary>
        /// "Forces the slider body to use the current combo colour instead of the skin's slider track override colour."
        /// </summary>
        public static LocalisableString UseComboColourBodyTooltip => new TranslatableString(getKey(@"use_combo_colour_body_tooltip"), @"Forces the slider body to use the current combo colour instead of the skin's slider track override colour.");

        /// <summary>
        /// "Body style"
        /// </summary>
        public static LocalisableString SliderBodyStyle => new TranslatableString(getKey(@"slider_body_style"), @"Body style");

        /// <summary>
        /// "Chooses how the inside of the slider is filled."
        /// </summary>
        public static LocalisableString SliderBodyStyleTooltip => new TranslatableString(getKey(@"slider_body_style_tooltip"), @"Chooses how the inside of the slider is filled.");

        #endregion
        #region Colours

        /// <summary>
        /// "Colours"
        /// </summary>
        public static LocalisableString ColoursHeader => new TranslatableString(getKey(@"colours_header"), @"Colours");

        /// <summary>
        /// "Slider track (override)"
        /// </summary>
        public static LocalisableString SliderTrackOverride => new TranslatableString(getKey(@"slider_track_override"), @"Slider track (override)");

        /// <summary>
        /// "The colour drawn under the slider body. Most skins set a dark colour here so the slider body doesn't show the bright combo colour."
        /// </summary>
        public static LocalisableString SliderTrackOverrideTooltip => new TranslatableString(getKey(@"slider_track_override_tooltip"), @"The colour drawn under the slider body. Most skins set a dark colour here so the slider body doesn't show the bright combo colour.");

        /// <summary>
        /// "Slider border"
        /// </summary>
        public static LocalisableString SliderBorderColourLabel => new TranslatableString(getKey(@"slider_border_colour_label"), @"Slider border");

        /// <summary>
        /// "The colour of the border around the slider body."
        /// </summary>
        public static LocalisableString SliderBorderColourLabelTooltip => new TranslatableString(getKey(@"slider_border_colour_label_tooltip"), @"The colour of the border around the slider body.");

        /// <summary>
        /// "Slider ball"
        /// </summary>
        public static LocalisableString SliderBallColour => new TranslatableString(getKey(@"slider_ball_colour"), @"Slider ball");

        /// <summary>
        /// "The colour of the slider ball that travels along the slider."
        /// </summary>
        public static LocalisableString SliderBallColourTooltip => new TranslatableString(getKey(@"slider_ball_colour_tooltip"), @"The colour of the slider ball that travels along the slider.");

        /// <summary>
        /// "Spinner background"
        /// </summary>
        public static LocalisableString SpinnerBackgroundColour => new TranslatableString(getKey(@"spinner_background_colour"), @"Spinner background");

        /// <summary>
        /// "The background colour behind the spinner."
        /// </summary>
        public static LocalisableString SpinnerBackgroundColourTooltip => new TranslatableString(getKey(@"spinner_background_colour_tooltip"), @"The background colour behind the spinner.");

        /// <summary>
        /// "Star break additive"
        /// </summary>
        public static LocalisableString StarBreakAdditiveColour => new TranslatableString(getKey(@"star_break_additive_colour"), @"Star break additive");

        /// <summary>
        /// "The additive glow colour of the star burst that appears at the end of a slider."
        /// </summary>
        public static LocalisableString StarBreakAdditiveColourTooltip => new TranslatableString(getKey(@"star_break_additive_colour_tooltip"), @"The additive glow colour of the star burst that appears at the end of a slider.");

        /// <summary>
        /// "Input overlay text"
        /// </summary>
        public static LocalisableString InputOverlayTextColour => new TranslatableString(getKey(@"input_overlay_text_colour"), @"Input overlay text");

        /// <summary>
        /// "The colour of the keys shown in the input overlay during gameplay and replays."
        /// </summary>
        public static LocalisableString InputOverlayTextColourTooltip => new TranslatableString(getKey(@"input_overlay_text_colour_tooltip"), @"The colour of the keys shown in the input overlay during gameplay and replays.");

        /// <summary>
        /// "Menu glow"
        /// </summary>
        public static LocalisableString MenuGlowColour => new TranslatableString(getKey(@"menu_glow_colour"), @"Menu glow");

        /// <summary>
        /// "The colour of the glow used on the main menu."
        /// </summary>
        public static LocalisableString MenuGlowColourTooltip => new TranslatableString(getKey(@"menu_glow_colour_tooltip"), @"The colour of the glow used on the main menu.");

        #endregion

        #region Combo Colours

        /// <summary>
        /// "Combo Colours"
        /// </summary>
        public static LocalisableString ComboColoursHeader => new TranslatableString(getKey(@"combo_colours_header"), @"Combo Colours");

        /// <summary>
        /// "Hit circle &amp; slider combo colours"
        /// </summary>
        public static LocalisableString ComboColoursCaption => new TranslatableString(getKey(@"combo_colours_caption"), @"Hit circle & slider combo colours");

        /// <summary>
        /// "The colours used to cycle through hit circles and slider bodies as the combo increases. Add more colours for more variety."
        /// </summary>
        public static LocalisableString ComboColoursTooltip => new TranslatableString(getKey(@"combo_colours_tooltip"), @"The colours used to cycle through hit circles and slider bodies as the combo increases. Add more colours for more variety.");

        #endregion

        #region Gameplay Settings

        /// <summary>
        /// "Gameplay Settings"
        /// </summary>
        public static LocalisableString GameplayHeader => new TranslatableString(getKey(@"gameplay_header"), @"Gameplay Settings");

        /// <summary>
        /// "Centre cursor"
        /// </summary>
        public static LocalisableString CursorCentre => new TranslatableString(getKey(@"cursor_centre"), @"Centre cursor");

        /// <summary>
        /// "Whether the gameplay cursor is centred on the playfield."
        /// </summary>
        public static LocalisableString CursorCentreTooltip => new TranslatableString(getKey(@"cursor_centre_tooltip"), @"Whether the gameplay cursor is centred on the playfield.");

        /// <summary>
        /// "Expand cursor on click"
        /// </summary>
        public static LocalisableString CursorExpand => new TranslatableString(getKey(@"cursor_expand"), @"Expand cursor on click");

        /// <summary>
        /// "Whether the cursor scales up slightly when you click."
        /// </summary>
        public static LocalisableString CursorExpandTooltip => new TranslatableString(getKey(@"cursor_expand_tooltip"), @"Whether the cursor scales up slightly when you click.");

        /// <summary>
        /// "Rotate cursor sprite"
        /// </summary>
        public static LocalisableString CursorRotate => new TranslatableString(getKey(@"cursor_rotate"), @"Rotate cursor sprite");

        /// <summary>
        /// "Whether the cursor sprite rotates as it moves."
        /// </summary>
        public static LocalisableString CursorRotateTooltip => new TranslatableString(getKey(@"cursor_rotate_tooltip"), @"Whether the cursor sprite rotates as it moves.");

        /// <summary>
        /// "Rotate cursor trail"
        /// </summary>
        public static LocalisableString CursorTrailRotate => new TranslatableString(getKey(@"cursor_trail_rotate"), @"Rotate cursor trail");

        /// <summary>
        /// "Whether the cursor trail rotates with the cursor."
        /// </summary>
        public static LocalisableString CursorTrailRotateTooltip => new TranslatableString(getKey(@"cursor_trail_rotate_tooltip"), @"Whether the cursor trail rotates with the cursor.");

        /// <summary>
        /// "Hit circle overlay above number"
        /// </summary>
        public static LocalisableString HitCircleOverlayAboveNumber => new TranslatableString(getKey(@"hit_circle_overlay_above_number"), @"Hit circle overlay above number");

        /// <summary>
        /// "Whether the hit circle overlay is drawn above the combo number instead of below it."
        /// </summary>
        public static LocalisableString HitCircleOverlayAboveNumberTooltip => new TranslatableString(getKey(@"hit_circle_overlay_above_number_tooltip"), @"Whether the hit circle overlay is drawn above the combo number instead of below it.");

        /// <summary>
        /// "Spinner frequency modulate"
        /// </summary>
        public static LocalisableString SpinnerFrequencyModulate => new TranslatableString(getKey(@"spinner_frequency_modulate"), @"Spinner frequency modulate");

        /// <summary>
        /// "Whether the spinner's rotation speed is modulated by the beatmap's BPM."
        /// </summary>
        public static LocalisableString SpinnerFrequencyModulateTooltip => new TranslatableString(getKey(@"spinner_frequency_modulate_tooltip"), @"Whether the spinner's rotation speed is modulated by the beatmap's BPM.");
        /// <summary>
        /// "Disable spinner blink"
        /// </summary>
        public static LocalisableString SpinnerNoBlink => new TranslatableString(getKey(@"spinner_no_blink"), @"Disable spinner blink");

        /// <summary>
        /// "Prevents the spinner from blinking while it is rotating."
        /// </summary>
        public static LocalisableString SpinnerNoBlinkTooltip => new TranslatableString(getKey(@"spinner_no_blink_tooltip"), @"Prevents the spinner from blinking while it is rotating.");

        /// <summary>
        /// "Tint slider ball with combo colour"
        /// </summary>
        public static LocalisableString AllowSliderBallTint => new TranslatableString(getKey(@"allow_slider_ball_tint"), @"Tint slider ball with combo colour");

        /// <summary>
        /// "Whether the slider ball is tinted with the current combo colour."
        /// </summary>
        public static LocalisableString AllowSliderBallTintTooltip => new TranslatableString(getKey(@"allow_slider_ball_tint_tooltip"), @"Whether the slider ball is tinted with the current combo colour.");

        /// <summary>
        /// "Use layered hit sounds"
        /// </summary>
        public static LocalisableString LayeredHitSounds => new TranslatableString(getKey(@"layered_hit_sounds"), @"Use layered hit sounds");

        /// <summary>
        /// "Whether hit sounds layer, allowing multiple samples to play at once rather than replacing each other."
        /// </summary>
        public static LocalisableString LayeredHitSoundsTooltip => new TranslatableString(getKey(@"layered_hit_sounds_tooltip"), @"Whether hit sounds layer, allowing multiple samples to play at once rather than replacing each other.");

        /// <summary>
        /// "Animation framerate"
        /// </summary>
        public static LocalisableString AnimationFramerate => new TranslatableString(getKey(@"animation_framerate"), @"Animation framerate");

        /// <summary>
        /// "The framerate at which animated skin elements are played."
        /// </summary>
        public static LocalisableString AnimationFramerateTooltip => new TranslatableString(getKey(@"animation_framerate_tooltip"), @"The framerate at which animated skin elements are played.");

        /// <summary>
        /// "Hit circle overlap"
        /// </summary>
        public static LocalisableString HitCircleOverlap => new TranslatableString(getKey(@"hit_circle_overlap"), @"Hit circle overlap");

        /// <summary>
        /// "The overlap of the hit circle's surrounding elements, such as the approach circle and number."
        /// </summary>
        public static LocalisableString HitCircleOverlapTooltip => new TranslatableString(getKey(@"hit_circle_overlap_tooltip"), @"The overlap of the hit circle's surrounding elements, such as the approach circle and number.");

        /// <summary>
        /// "Score overlap"
        /// </summary>
        public static LocalisableString ScoreOverlap => new TranslatableString(getKey(@"score_overlap"), @"Score overlap");

        /// <summary>
        /// "The vertical overlap between the digits of the score counter."
        /// </summary>
        public static LocalisableString ScoreOverlapTooltip => new TranslatableString(getKey(@"score_overlap_tooltip"), @"The vertical overlap between the digits of the score counter.");

        /// <summary>
        /// "Combo overlap"
        /// </summary>
        public static LocalisableString ComboOverlap => new TranslatableString(getKey(@"combo_overlap"), @"Combo overlap");

        /// <summary>
        /// "The vertical overlap between the digits of the combo counter."
        /// </summary>
        public static LocalisableString ComboOverlapTooltip => new TranslatableString(getKey(@"combo_overlap_tooltip"), @"The vertical overlap between the digits of the combo counter.");

        #endregion
        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}