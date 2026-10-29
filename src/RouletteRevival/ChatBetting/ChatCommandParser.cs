using System;
using System.Globalization;

namespace RouletteRevival.ChatBetting
{
    public enum ChatCommandKind
    {
        Bet,
        Clear,
        ListBets,
        Spin,
        Help,
    }

    /// <summary>
    /// A parsed chat command. <see cref="Amount"/> is the stake for bet and the page number for help.
    /// (A class rather than a record: records emit nullable attributes, which clash with the game's interop assemblies.)
    /// </summary>
    public sealed class ChatCommand : IEquatable<ChatCommand>
    {
        public ChatCommand(ChatCommandKind kind, long amount = 0, BetSpot spot = default)
        {
            Kind = kind;
            Amount = amount;
            Spot = spot;
        }

        public ChatCommandKind Kind { get; }
        public long Amount { get; }
        public BetSpot Spot { get; }

        public bool Equals(ChatCommand other) =>
            other != null && Kind == other.Kind && Amount == other.Amount && Spot.Equals(other.Spot);

        public override bool Equals(object obj) => Equals(obj as ChatCommand);

        public override int GetHashCode() => HashCode.Combine(Kind, Amount, Spot);

        public override string ToString() => $"{Kind} {Spot} {Amount}";
    }

    /// <summary>
    /// Parses roulette chat commands typed by any player (engine-free). "!" is used because the game handles "/"
    /// commands locally on each player's PC, so they would never reach the host.
    ///   !bet &lt;spot&gt; &lt;amount&gt;   e.g. !bet red 5000, !bet 17 10k
    ///   !clear                  take back your bets before the spin
    ///   !bets                   list your bets
    ///   !help [page]            list the commands, a page at a time
    ///   !spin                   spin the wheel (players without the mod can't press F on it)
    /// </summary>
    public static class ChatCommandParser
    {
        public const int MaxHelpPage = 99;

        public static bool TryParse(string message, out ChatCommand command)
        {
            command = null;
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            var parts = message.Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            switch (parts[0].ToLowerInvariant())
            {
                case "!bet":
                    return TryParseBet(parts, out command);
                case "!clear":
                    command = parts.Length == 1 ? new ChatCommand(ChatCommandKind.Clear) : null;
                    return command != null;
                case "!bets":
                    command = parts.Length == 1 ? new ChatCommand(ChatCommandKind.ListBets) : null;
                    return command != null;
                case "!spin":
                    command = parts.Length == 1 ? new ChatCommand(ChatCommandKind.Spin) : null;
                    return command != null;
                case "!help":
                    return TryParseHelp(parts, out command);
                default:
                    return false;
            }
        }

        private static bool TryParseBet(string[] parts, out ChatCommand command)
        {
            command = null;
            if (parts.Length != 3 || !BetSpotParser.TryParse(parts[1], out var spot) || !AmountParser.TryParse(parts[2], out var amount))
            {
                return false;
            }

            command = new ChatCommand(ChatCommandKind.Bet, amount, spot);
            return true;
        }

        /// <summary>"!help" is page 1; "!help 2" picks a page (Amount holds the page number).</summary>
        private static bool TryParseHelp(string[] parts, out ChatCommand command)
        {
            command = null;
            var page = 1;
            if (parts.Length > 2 || (parts.Length == 2 && !TryPositive(parts[1], MaxHelpPage, out page)))
            {
                return false;
            }

            command = new ChatCommand(ChatCommandKind.Help, page);
            return true;
        }

        private static bool TryPositive(string text, int max, out int value) =>
            int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value) && value > 0 && value <= max;
    }
}
