// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.Formats;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Screens.Edit.Components;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Overlays.SkinEditor
{
    /// <summary>
    /// Exposes the skin's combo colours (the <c>Combo1..N</c> entries of a legacy
    /// <c>skin.ini</c> <c>[Colours]</c> section), stored in
    /// <see cref="SkinConfiguration.CustomComboColours"/>.
    /// </summary>
    internal partial class SkinComboColourSettings : EditorSidebarSection
    {
        private readonly Skin skin;
        private readonly Action changed;

        private FormColourPalette palette = null!;
        private bool syncing;

        public SkinComboColourSettings(Skin skin, Action changed)
            : base(SkinEditorSettingsStrings.ComboColoursHeader)
        {
            this.skin = skin;
            this.changed = changed;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Content.Add(palette = new FormColourPalette
            {
                Caption = SkinEditorSettingsStrings.ComboColoursCaption,
                HintText = SkinEditorSettingsStrings.ComboColoursTooltip,
                PaletteHeaderText = string.Empty,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            palette.Colours.AddRange(skin.Configuration.CustomComboColours.Select(c => (Colour4)c));

            if (palette.Colours.Count == 0)
            {
                // No custom combo colours defined; show the defaults purely for visual feedback
                // without immediately writing them back to the skin.
                syncing = true;
                palette.Colours.AddRange(SkinConfiguration.DefaultComboColours.Select(c => (Colour4)c));
                syncing = false;
            }

            palette.Colours.BindCollectionChanged((_, _) =>
            {
                palette.CanAdd.Value = palette.Colours.Count < LegacyDecoder<SkinConfiguration>.MAX_COMBO_COLOUR_COUNT;

                if (syncing)
                    return;

                syncToSkin();
            });
        }

        private void syncToSkin()
        {
            skin.Configuration.CustomComboColours.Clear();
            skin.Configuration.CustomComboColours.AddRange(palette.Colours.Select(c => new Color4(c.R, c.G, c.B, c.A)));
            changed();
        }
    }
}