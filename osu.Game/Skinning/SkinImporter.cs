// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Overlays.Notifications;
using Realms;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class SkinImporter : RealmArchiveModelImporter<SkinInfo>
    {
        private const string skin_info_file = "skininfo.json";

        private readonly IStorageResourceProvider skinResources;

        private readonly ModelManager<SkinInfo> modelManager;

        public SkinImporter(Storage storage, RealmAccess realm, IStorageResourceProvider skinResources)
            : base(storage, realm)
        {
            this.skinResources = skinResources;

            modelManager = new ModelManager<SkinInfo>(storage, realm);
        }

        public override IEnumerable<string> HandledExtensions => new[] { ".osk" };

        protected override string[] HashableFileTypes => new[] { ".ini", ".json" };

        protected override bool ShouldDeleteArchive(string path) => string.Equals(Path.GetExtension(path), @".osk", StringComparison.OrdinalIgnoreCase);

        protected override SkinInfo CreateModel(ArchiveReader archive, ImportParameters parameters) => new SkinInfo { Name = archive.Name ?? @"No name" };

        private const string unknown_creator_string = @"Unknown";

        /// <summary>
        /// Update an existing skin with the contents of a path
        /// </summary>
        /// <param name="notification">The progress notification</param>
        /// <param name="task">The <see cref="ImportTask"/> to update the <paramref name="original"/> with</param>
        /// <param name="original">The <see cref="SkinInfo"/> to update</param>
        /// <returns></returns>
        public override async Task<Live<SkinInfo>?> ImportAsUpdate(ProgressNotification notification, ImportTask task, SkinInfo original)
        {
            return await Realm.WriteAsync<Live<SkinInfo>?>(r =>
            {
                var skinInfo = r.Find<SkinInfo>(original.ID)!;
                skinInfo.Files.Clear();

                string[] filesInMountedDirectory = Directory.EnumerateFiles(task.Path, "*.*", SearchOption.AllDirectories).Select(f => Path.GetRelativePath(task.Path, f)).ToArray();

                foreach (string file in filesInMountedDirectory)
                {
                    using var stream = File.OpenRead(Path.Combine(task.Path, file));

                    modelManager.AddFile(skinInfo, stream, file, r);
                }

                string skinIniPath = Path.Combine(task.Path, "skin.ini");

                if (File.Exists(skinIniPath))
                {
                    using (var stream = File.OpenRead(skinIniPath))
                    using (var lineReader = new LineBufferedReader(stream))
                    {
                        var decodedSkinIni = new LegacySkinDecoder().Decode(lineReader);

                        if (!string.IsNullOrEmpty(decodedSkinIni.SkinInfo.Name))
                            skinInfo.Name = decodedSkinIni.SkinInfo.Name;

                        if (!string.IsNullOrEmpty(decodedSkinIni.SkinInfo.Creator))
                            skinInfo.Creator = decodedSkinIni.SkinInfo.Creator;
                    }
                }

                return skinInfo.ToLive(Realm);
            }).ConfigureAwait(false);
        }

        protected override void Populate(SkinInfo model, ArchiveReader? archive, Realm realm, CancellationToken cancellationToken = default)
        {
            var skinInfoFile = model.GetFile(skin_info_file);

            if (skinInfoFile != null)
            {
                try
                {
                    using (var existingStream = Files.Storage.GetStream(skinInfoFile.File.GetStoragePath()))
                    using (var reader = new StreamReader(existingStream))
                    {
                        var deserialisedSkinInfo = JsonConvert.DeserializeObject<SkinInfo>(reader.ReadToEnd());

                        if (deserialisedSkinInfo != null)
                        {
                            // for now we only care about the instantiation info.
                            // eventually we probably want to transfer everything across.
                            model.InstantiationInfo = deserialisedSkinInfo.InstantiationInfo;
                        }
                    }
                }
                catch (Exception e)
                {
                    LogForModel(model, $"Error during {skin_info_file} parsing, falling back to default", e);

                    // Not sure if we should still run the import in the case of failure here, but let's do so for now.
                    model.InstantiationInfo = string.Empty;
                }
            }

            // Always rewrite instantiation info (even after parsing in from the skin json) for sanity.
            model.InstantiationInfo = createInstance(model).GetType().GetInvariantInstantiationInfo();

            checkSkinIniMetadata(model, realm);
        }

        private void checkSkinIniMetadata(SkinInfo item, Realm realm)
        {
            var instance = createInstance(item);

            // This function can be run on fresh import or save. The logic here ensures a skin.ini file is in a good state for both operations.
            // `Skin` will parse the skin.ini and populate `Skin.Configuration` during construction above.
            string skinIniSourcedName = instance.Configuration.SkinInfo.Name;
            string skinIniSourcedCreator = instance.Configuration.SkinInfo.Creator;
            string archiveName = item.Name.Replace(@".osk", string.Empty, StringComparison.OrdinalIgnoreCase);

            bool isImport = !item.IsManaged;

            if (isImport)
            {
                item.Name = !string.IsNullOrEmpty(skinIniSourcedName) ? skinIniSourcedName : archiveName;
                item.Creator = !string.IsNullOrEmpty(skinIniSourcedCreator) ? skinIniSourcedCreator : unknown_creator_string;

                // For imports, we want to use the archive or folder name as part of the metadata, in addition to any existing skin.ini metadata.
                // In an ideal world, skin.ini would be the only source of metadata, but a lot of skin creators and users don't update it when making modifications.
                // In both of these cases, the expectation from the user is that the filename or folder name is displayed somewhere to identify the skin.
                if (archiveName != item.Name
                    // lazer exports use this format
                    // GetValidFilename accounts for skins with non-ASCII characters in the name that have been exported by lazer.
                    && archiveName != item.GetDisplayString().GetValidFilename())
                    item.Name = @$"{item.Name} [{archiveName}]";
            }

            // By this point, the metadata in SkinInfo will be correct.
            // Regardless of whether this is an import or not, let's write the skin.ini if non-existing or non-matching.
            // This is (weirdly) done inside ComputeHash to avoid adding a new method to handle this case. After switching to realm it can be moved into another place.
            if (skinIniSourcedName != item.Name)
                UpdateSkinIniMetadata(item, realm);
        }

        public void UpdateSkinIniMetadata(SkinInfo item, Realm realm)
        {
            var existingFile = item.GetFile(@"skin.ini");

            // create a working instance of the skin.
            // this implicitly decodes all contents of `skin.ini` into `skin.Configuration` and others.
            // of note, `item` and `skin.Configuration.SkinInfo` are not the same object.
            var skin = createInstance(item);

            // `skin.Configuration.SkinInfo` will be in a post-decoding state,
            // which means it will potentially be in a different state than `item`.
            // write changes from `item` to `skin.SkinInfo` to definitively apply them.
            skin.Configuration.SkinInfo.Name = item.Name;
            skin.Configuration.SkinInfo.Creator = item.Creator;

            using var outStream = new MemoryStream();

            // now with the skin having the updated metadata, write out the full `skin.ini`.
            using (var outWriter = new StreamWriter(outStream, Encoding.UTF8, 1024, true))
            {
                var encoder = new LegacySkinEncoder(skin);
                encoder.Encode(outWriter);
            }

            if (existingFile == null)
                modelManager.AddFile(item, outStream, @"skin.ini", realm);
            else
                modelManager.ReplaceFile(existingFile, outStream, realm);

            // The hash is already populated at this point in import.
            // As we have changed files, it needs to be recomputed.
            item.Hash = ComputeHash(item);
        }

        private Skin createInstance(SkinInfo item) => item.CreateInstance(skinResources);

        /// <summary>
        /// Save a skin, serialising any changes to skin layouts to relevant JSON structures.
        /// </summary>
        /// <returns>Whether any change actually occurred.</returns>
        public bool Save(Skin skin)
        {
            bool hadChanges = false;

            skin.SkinInfo.PerformWrite(s =>
            {
                // Update for safety
                s.InstantiationInfo = skin.GetType().GetInvariantInstantiationInfo();

                // Serialise out the SkinInfo itself.
                string skinInfoJson = JsonConvert.SerializeObject(s, new JsonSerializerSettings { Formatting = Formatting.Indented });

                using (var streamContent = new MemoryStream(Encoding.UTF8.GetBytes(skinInfoJson)))
                {
                    modelManager.AddFile(s, streamContent, skin_info_file, s.Realm!);
                }

                // Then serialise each of the drawable component groups into respective files.
                foreach (var drawableInfo in skin.LayoutInfos)
                {
                    string json = JsonConvert.SerializeObject(drawableInfo.Value, new JsonSerializerSettings { Formatting = Formatting.Indented });

                    using (var streamContent = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                    {
                        string filename = @$"{drawableInfo.Key}.json";

                        var oldFile = s.GetFile(filename);

                        if (oldFile != null)
                            modelManager.ReplaceFile(oldFile, streamContent, s.Realm!);
                        else
                            modelManager.AddFile(s, streamContent, filename, s.Realm!);
                    }
                }

                // Persist any slider style settings to skin.ini.
                updateSliderStyleIni(s, skin, s.Realm!);

                // Persist editor-managed cursor/gameplay config keys and combo / custom colours to skin.ini.
                updateGameSettingsIni(s, skin, s.Realm!);

                string newHash = ComputeHash(s);

                hadChanges = newHash != s.Hash;

                s.Hash = newHash;
            });

            return hadChanges;
        }

        /// <summary>
        /// Persist any slider style settings to the skin's `skin.ini` file.
        /// <para>
        /// Only keys managed by <see cref="SliderStyleSetting"/> are touched; all other contents
        /// of the file (including keys unknown to lazer) are preserved as-is. A `skin.ini` is only
        /// created if the skin does not yet have one and slider styles have been configured.
        /// </para>
        /// </summary>
        private void updateSliderStyleIni(SkinInfo s, Skin skin, Realm realm)
        {
            SliderStyleSetting[] settings = Enum.GetValues<SliderStyleSetting>();

            var styleEntries = new List<string>();

            foreach (var setting in settings)
            {
                if (skin.Configuration.ConfigDictionary.TryGetValue(setting.ToString(), out string? value))
                    styleEntries.Add($@"{setting}: {value}");
            }

            // Don't create or modify skin.ini at all if no slider styles have been configured.
            if (styleEntries.Count == 0)
                return;

            var existingFile = s.GetFile(@"skin.ini");

            var lines = new List<string>();

            if (existingFile != null)
            {
                using (var stream = Files.Storage.GetStream(existingFile.File.GetStoragePath()))
                using (var reader = new StreamReader(stream))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                        lines.Add(line);
                }

                // Strip any previously-written slider style lines, wherever they may be.
                lines.RemoveAll(line => settings.Any(setting =>
                    line.TrimStart().StartsWith($@"{setting}:", StringComparison.OrdinalIgnoreCase)));

                // If no [General] section exists, add one so the keys have a home.
                if (!lines.Any(line => line.Trim().Equals(@"[General]", StringComparison.OrdinalIgnoreCase)))
                {
                    lines.Insert(0, @"[General]");
                    lines.Insert(1, string.Empty);
                }

                // Insert the keys directly after the [General] header.
                int insertIndex = lines.FindIndex(line => line.Trim().Equals(@"[General]", StringComparison.OrdinalIgnoreCase)) + 1;
                lines.InsertRange(insertIndex, styleEntries);
            }
            else
            {
                // This skin has no skin.ini (e.g. a default skin). Create a minimal one containing
                // the metadata and the slider style keys only.
                lines.Add(@"[General]");
                lines.Add($@"Name: {skin.Configuration.SkinInfo.Name}");
                lines.Add($@"Author: {skin.Configuration.SkinInfo.Creator}");
                lines.Add(@"Version: latest");
                lines.Add(string.Empty);
                lines.AddRange(styleEntries);
            }

            using (var outStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(outStream, Encoding.UTF8, 1024, leaveOpen: true))
                {
                    foreach (string line in lines)
                        writer.WriteLine(line);
                }

                outStream.Position = 0;

                if (existingFile != null)
                    modelManager.ReplaceFile(existingFile, outStream, realm);
                else
                    modelManager.AddFile(s, outStream, @"skin.ini", realm);
            }
        }
        /// <summary>
        /// Persist editor-managed gameplay/cursor config keys (as <c>[General]</c> lines) and combo /
        /// custom colours (as <c>[Colours]</c> lines) to the skin's <c>skin.ini</c> file.
        /// <para>
        /// Only keys managed by the skin editor are touched; all other contents of the file
        /// (including keys unknown to lazer) are preserved as-is. This never creates a
        /// <c>skin.ini</c> by itself; if none exists it simply does nothing, leaving creation to
        /// whatever other edit operation requires it.
        /// </para>
        /// </summary>
        private void updateGameSettingsIni(SkinInfo s, Skin skin, Realm realm)
        {
            var generalEntries = new List<string>();
            var colourEntries = new List<string>();

            foreach (string key in editor_managed_general_keys)
            {
                if (skin.Configuration.ConfigDictionary.TryGetValue(key, out string? value))
                    generalEntries.Add($@"{key}: {value}");
            }

            // Combo colours (Combo1..N).
            for (int i = 0; i < skin.Configuration.CustomComboColours.Count; ++i)
                colourEntries.Add($@"Combo{i + 1}: {colourToString(skin.Configuration.CustomComboColours[i])}");

            // Custom colours.
            foreach (string key in editor_managed_colour_keys)
            {
                if (skin.Configuration.CustomColours.TryGetValue(key, out Color4 colour))
                    colourEntries.Add($@"{key}: {colourToString(colour)}");
            }

            if (generalEntries.Count == 0 && colourEntries.Count == 0)
                return;

            var existingFile = s.GetFile(@"skin.ini");

            if (existingFile == null)
                return;

            var lines = new List<string>();

            using (var stream = Files.Storage.GetStream(existingFile.File.GetStoragePath()))
            using (var reader = new StreamReader(stream))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                    lines.Add(line);
            }

            // Strip any previously-written managed general keys, wherever they may be.
            foreach (string key in editor_managed_general_keys)
                lines.RemoveAll(line => line.TrimStart().StartsWith($@"{key}:", StringComparison.OrdinalIgnoreCase));

            // Strip combo + custom colour lines so we can reinsert them in their canonical section.
            for (int i = 0; i < LegacyDecoder<SkinConfiguration>.MAX_COMBO_COLOUR_COUNT; ++i)
                lines.RemoveAll(line => line.TrimStart().StartsWith($@"Combo{i + 1}:", StringComparison.OrdinalIgnoreCase));

            foreach (string key in editor_managed_colour_keys)
                lines.RemoveAll(line => line.TrimStart().StartsWith($@"{key}:", StringComparison.OrdinalIgnoreCase));

            if (generalEntries.Count > 0)
            {
                // If no [General] section exists, add one so the keys have a home.
                if (!lines.Any(line => line.Trim().Equals(@"[General]", StringComparison.OrdinalIgnoreCase)))
                {
                    lines.Insert(0, @"[General]");
                    lines.Insert(1, string.Empty);
                }

                // Insert the keys directly after the [General] header.
                int insertIndex = lines.FindIndex(line => line.Trim().Equals(@"[General]", StringComparison.OrdinalIgnoreCase)) + 1;
                lines.InsertRange(insertIndex, generalEntries);
            }

            if (colourEntries.Count > 0)
            {
                // Colours belong in the [Colours] section.
                if (!lines.Any(line => line.Trim().Equals(@"[Colours]", StringComparison.OrdinalIgnoreCase)))
                {
                    lines.Insert(0, @"[Colours]");
                    lines.Insert(1, string.Empty);
                }

                int insertIndex = lines.FindIndex(line => line.Trim().Equals(@"[Colours]", StringComparison.OrdinalIgnoreCase)) + 1;
                lines.InsertRange(insertIndex, colourEntries);
            }

            using (var outStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(outStream, Encoding.UTF8, 1024, leaveOpen: true))
                {
                    foreach (string line in lines)
                        writer.WriteLine(line);
                }

                outStream.Position = 0;

                modelManager.ReplaceFile(existingFile, outStream, realm);
            }
        }
        /// <summary>
        /// Legacy config keys (cursor / gameplay toggles and spinner settings) exposed in the skin
        /// editor, persisted to the skin's <c>skin.ini</c>.
        /// </summary>
        private static readonly string[] editor_managed_general_keys =
        {
            @"CursorCentre",
            @"CursorExpand",
            @"CursorRotate",
            @"CursorTrailRotate",
            @"HitCircleOverlayAboveNumber",
            @"SpinnerFrequencyModulate",
            @"SpinnerNoBlink",
            @"AllowSliderBallTint",
            @"LayeredHitSounds",
            @"AnimationFramerate",
            @"HitCircleOverlap",
            @"ScoreOverlap",
            @"ComboOverlap",
            @"SliderPathRadius",
        };

        /// <summary>
        /// Custom colour keys exposed in the skin editor, persisted to the <c>[Colours]</c> section
        /// of the skin's <c>skin.ini</c>.
        /// </summary>
        private static readonly string[] editor_managed_colour_keys =
        {
            @"SliderTrackOverride",
            @"SliderBorder",
            @"SliderBall",
            @"SpinnerBackground",
            @"StarBreakAdditive",
            @"InputOverlayText",
            @"MenuGlow",
        };

        private static string colourToString(Color4 colour)
            => FormattableString.Invariant($@"{(int)(colour.R * 255)},{(int)(colour.G * 255)},{(int)(colour.B * 255)}");
    }
}
