using System.Globalization;

namespace RouletteRevival.ChatBetting
{
    /// <summary>Parses bet amounts like "5000", "10,000", "5k" or "1m" (engine-free).</summary>
    public static class AmountParser
    {
        public const long MaxAmount = 1_000_000_000;

        public static bool TryParse(string text, out long amount)
        {
            amount = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var trimmed = text.Trim().Replace(",", string.Empty).ToLowerInvariant();
            long multiplier = 1;
            if (trimmed.EndsWith("k"))
            {
                multiplier = 1_000;
                trimmed = trimmed.Substring(0, trimmed.Length - 1);
            }
            else if (trimmed.EndsWith("m"))
            {
                multiplier = 1_000_000;
                trimmed = trimmed.Substring(0, trimmed.Length - 1);
            }

            if (!decimal.TryParse(trimmed, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
            {
                return false;
            }

            var scaled = value * multiplier;
            if (scaled <= 0 || scaled > MaxAmount || scaled != decimal.Truncate(scaled))
            {
                return false;
            }

            amount = (long)scaled;
            return true;
        }
    }
}
