// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Edit.Components;
using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinEditor
{
    /// <summary>
    /// Exposes the various gameplay-affecting boolean and numeric skin settings that are
    /// stored as raw keys in a skin's <see cref="SkinConfiguration.ConfigDictionary"/>.
    /// These are the values consumed by the ruleset's cursor / gameplay config lookups
    /// and the general legacy config lookups (e.g. <c>CursorCentre</c>, <c>AnimationFramerate</c>).
    /// </summary>
    internal partial class SkinGameplaySettings : EditorSidebarSection
    {
        private readonly Skin skin;
        private readonly Action changed;

        private FillFlowContainer settingsContent = null!;

        private static readonly (string key, LocalisableString caption, LocalisableString tooltip, bool defaultValue)[] boolean_entries =
        {
            (@"CursorCentre", SkinEditorSettingsStrings.CursorCentre, SkinEditorSettingsStrings.CursorCentreTooltip, true),
            (@"CursorExpand", SkinEditorSettingsStrings.CursorExpand, SkinEditorSettingsStrings.CursorExpandTooltip, true),
            (@"CursorRotate", SkinEditorSettingsStrings.CursorRotate, SkinEditorSettingsStrings.CursorRotateTooltip, true),
            (@"CursorTrailRotate", SkinEditorSettingsStrings.CursorTrailRotate, SkinEditorSettingsStrings.CursorTrailRotateTooltip, true),
            (@"HitCircleOverlayAboveNumber", SkinEditorSettingsStrings.HitCircleOverlayAboveNumber, SkinEditorSettingsStrings.HitCircleOverlayAboveNumberTooltip, false),
            (@"SpinnerFrequencyModulate", SkinEditorSettingsStrings.SpinnerFrequencyModulate, SkinEditorSettingsStrings.SpinnerFrequencyModulateTooltip, true),
            (@"SpinnerNoBlink", SkinEditorSettingsStrings.SpinnerNoBlink, SkinEditorSettingsStrings.SpinnerNoBlinkTooltip, false),
            (@"AllowSliderBallTint", SkinEditorSettingsStrings.AllowSliderBallTint, SkinEditorSettingsStrings.AllowSliderBallTintTooltip, true),
            (@"LayeredHitSounds", SkinEditorSettingsStrings.LayeredHitSounds, SkinEditorSettingsStrings.LayeredHitSoundsTooltip, true),
        };

        private static readonly (string key, LocalisableString caption, LocalisableString tooltip, float min, float max, float fallback)[] numeric_entries =
        {
            (@"AnimationFramerate", SkinEditorSettingsStrings.AnimationFramerate, SkinEditorSettingsStrings.AnimationFramerateTooltip, 0f, 60f, 60f),
            (@"HitCircleOverlap", SkinEditorSettingsStrings.HitCircleOverlap, SkinEditorSettingsStrings.HitCircleOverlapTooltip, 0f, 40f, 0f),
            (@"ScoreOverlap", SkinEditorSettingsStrings.ScoreOverlap, SkinEditorSettingsStrings.ScoreOverlapTooltip, 0f, 40f, 0f),
            (@"ComboOverlap", SkinEditorSettingsStrings.ComboOverlap, SkinEditorSettingsStrings.ComboOverlapTooltip, 0f, 40f, 0f),
        };

        public SkinGameplaySettings(Skin skin, Action changed)
            : base(SkinEditorSettingsStrings.GameplayHeader)
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

            foreach (var (key, caption, tooltip, fallbackDefault) in boolean_entries)
            {
                var bindable = new BindableBool(readValue(key, fallbackDefault));
                bindable.ValueChanged += _ => writeValue(key, bindable.Value.ToString());

                settingsContent.Add(new SettingsItemV2(new FormCheckBox
                {
                    Caption = caption,
                    HintText = tooltip,
                    Current = bindable,
                }));
            }

            foreach (var (key, caption, tooltip, min, max, fallbackDefault) in numeric_entries)
            {
                var bindable = new BindableFloat
                {
                    MinValue = min,
                    MaxValue = max,
                    Precision = 0.1f,
                    Value = readValue(key, fallbackDefault),
                };

                bindable.ValueChanged += _ => writeValue(key, bindable.Value.ToString(CultureInfo.InvariantCulture));

                settingsContent.Add(new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = caption,
                    HintText = tooltip,
                    Current = bindable,
                    KeyboardStep = 1f,
                    TransferValueOnCommit = true,
                }));
            }
        }

        private TValue readValue<TValue>(string key, TValue fallback)
            where TValue : notnull
        {
            if (!skin.Configuration.ConfigDictionary.TryGetValue(key, out string? value))
                return fallback;

            return SliderStyleSettingParser.TryParse(value, out TValue parsed) ? parsed : fallback;
        }

        private void writeValue(string key, string value)
        {
            // The dictionary is a BindableDictionary, so this change notification propagates
            // to all gameplay components bounded to this skin's configuration.
            skin.Configuration.ConfigDictionary[key] = value;
            changed();
        }
    }
}