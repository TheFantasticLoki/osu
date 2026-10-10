// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace osu.Game.Configuration
{
    /// <summary>
    /// Stores the state of this fork's custom per-thread frame limiter.
    /// </summary>
    /// <remarks>
    /// Deliberately isolated in a fork-owned file rather than stored in <c>framework.ini</c> or <c>client.ini</c>:
    /// official builds sharing the same user data location never read or write this file, so they can always start
    /// normally, and they cannot corrupt or erase the limiter's state. This mirrors how ruleset settings are kept out
    /// of the main client config so that rulesets can be added and removed safely.
    /// </remarks>
    public class CustomFrameLimiterConfigManager : IniConfigManager<CustomFrameLimiterSetting>
    {
        public const string FILENAME = @"custom-frame-limiter.ini";

        protected override string Filename => FILENAME;

        public CustomFrameLimiterConfigManager(Storage storage)
            : base(storage)
        {
        }

        protected override void InitialiseDefaults()
        {
            SetDefault(CustomFrameLimiterSetting.Enabled, false);
            SetDefault(CustomFrameLimiterSetting.InputThreadLimit, 1000.0, 0.0, 8000.0, 1.0);
            SetDefault(CustomFrameLimiterSetting.AudioThreadLimit, 1000.0, 0.0, 8000.0, 1.0);
            SetDefault(CustomFrameLimiterSetting.UpdateThreadLimit, 1000.0, 0.0, 8000.0, 1.0);
            SetDefault(CustomFrameLimiterSetting.DrawThreadLimit, 1000.0, 0.0, 8000.0, 1.0);
            SetDefault(CustomFrameLimiterSetting.SyncThreadLimits, false);
        }
    }

    public enum CustomFrameLimiterSetting
    {
        Enabled,
        InputThreadLimit,
        AudioThreadLimit,
        UpdateThreadLimit,
        DrawThreadLimit,
        SyncThreadLimits,
    }
}
