using System;
using System.Globalization;

namespace RouletteRevival.Input
{
    /// <summary>Parses the function-key name used for the relocate hotkey ("F1".."F12"). Engine-free.</summary>
    public static class HotkeyParser
    {
        public const int MinFunctionKey = 1;
        public const int MaxFunctionKey = 12;

        /// <summary>Returns the function-key number (1-12), or <c>false</c> when the text is not F1..F12.</summary>
        public static bool TryParseFunctionKey(string text, out int number)
        {
            number = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var trimmed = text.Trim();
            if (trimmed.Length < 2 || char.ToUpperInvariant(trimmed[0]) != 'F')
            {
                return false;
            }

            if (!int.TryParse(trimmed.AsSpan(1), NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
            {
                return false;
            }

            if (parsed < MinFunctionKey || parsed > MaxFunctionKey)
            {
                return false;
            }

            number = parsed;
            return true;
        }
    }
}
