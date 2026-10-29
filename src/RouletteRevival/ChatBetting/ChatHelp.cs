using System;
using System.Collections.Generic;

namespace RouletteRevival.ChatBetting
{
    /// <summary>
    /// The !help text (engine-free), split into short pages because the chat box is narrow and only shows a few lines.
    /// Winnings are spelled out on an example bet in the same "+amount" form the spin results use, instead of odds jargon.
    /// </summary>
    public static class ChatHelp
    {
        public const int PageCount = 4;
        public const int MaxLinesPerPage = 4;

        /// <summary>Lines for <paramref name="page"/> (1-based); out-of-range pages clamp to the nearest one.</summary>
        public static IReadOnlyList<string> Page(int page, long chipUnit, long maxPerSpin)
        {
            var current = Math.Clamp(page, 1, PageCount);
            return current switch
            {
                1 => new[]
                {
                    $"Help 1/{PageCount}: !bet <spot> <amount>",
                    "e.g. !bet 17 10k or !bet red 5k",
                    $"Steps of {chipUnit:N0}, max {maxPerSpin:N0}/spin. More: !help 2",
                },
                2 => new[]
                {
                    $"Help 2/{PageCount}: spots: 0-36 (one number)",
                    "red, black, odd, even",
                    "low (1-18), high (19-36)",
                    "dozen1-3 (1-12/13-24/25-36), col1-3. !help 3",
                },
                3 => new[]
                {
                    $"Help 3/{PageCount}: you win, on a {chipUnit:N0} bet:",
                    $"a number: +{chipUnit * 35:N0} (35x)",
                    $"a dozen or column: +{chipUnit * 2:N0} (2x)",
                    $"red black odd even low high: +{chipUnit:N0}. !help 4",
                },
                _ => new[]
                {
                    $"Help 4/{PageCount}: !bets shows your bets",
                    "!clear takes your bets back",
                    "!spin spins the wheel (needs a bet)",
                },
            };
        }
    }
}
