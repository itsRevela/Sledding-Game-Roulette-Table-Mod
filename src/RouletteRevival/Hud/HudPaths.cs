using System;
using System.Collections.Generic;

namespace RouletteRevival.Hud
{
    /// <summary>
    /// Transform paths (relative to the "Roulette Table" object) of the betting HUD that still ships, inactive, inside the
    /// roulette prefab's world-space canvas. Names are verbatim from the asset, typos included. Engine-free.
    /// </summary>
    public static class HudPaths
    {
        public const string Canvas = "(Canvas) Roulette Betting (on)";
        public const string Panel = Canvas + "/Roulette table menu interactions (off)";
        public const string OddsText = Panel + "/current selected odds";
        public const string MaxWinText = Panel + "/potential earnings/slkjflksdjf (1)";
        public const string ClearBetsButton = Panel + "/(button) clear bets";
        public const string TotalBetText = Panel + "/change betting amount/Text (TMP) (1)";
        public const string BetAmountText = Panel + "/change betting amount/layout gruop/Text (TMP)";
        public const string DecreaseBetButton = Panel + "/change betting amount/layout gruop/(button) decrease bet";
        public const string IncreaseBetButton = Panel + "/change betting amount/layout gruop/(button) increase bet";

        /// <summary>Every path the binder needs, in the order it resolves them.</summary>
        public static IReadOnlyList<string> Required { get; } = new[]
        {
            Canvas, Panel, OddsText, MaxWinText, ClearBetsButton, TotalBetText, BetAmountText, DecreaseBetButton, IncreaseBetButton,
        };

        /// <summary>Returns the required paths that <paramref name="exists"/> reports as absent (empty when the HUD is complete).</summary>
        public static IReadOnlyList<string> FindMissing(Func<string, bool> exists)
        {
            if (exists == null)
            {
                throw new ArgumentNullException(nameof(exists));
            }

            var missing = new List<string>();
            foreach (var path in Required)
            {
                if (!exists(path))
                {
                    missing.Add(path);
                }
            }

            return missing;
        }
    }
}
