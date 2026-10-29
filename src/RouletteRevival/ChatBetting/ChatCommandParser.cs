using System;
using System.Globalization;

namespace RouletteRevival.ChatBetting
{
    public enum ChatCommandKind
    {
        TestPay,
        TestCharge,
    }

    /// <summary>
    /// A parsed chat command. <see cref="Amount"/> is points for pay, number of charges for charge.
    /// (A class rather than a record: records emit nullable attributes, which clash with the game's interop assemblies.)
    /// </summary>
    public sealed class ChatCommand : IEquatable<ChatCommand>
    {
        public ChatCommand(ChatCommandKind kind, int amount)
        {
            Kind = kind;
            Amount = amount;
        }

        public ChatCommandKind Kind { get; }
        public int Amount { get; }

        public bool Equals(ChatCommand other) => other != null && Kind == other.Kind && Amount == other.Amount;

        public override bool Equals(object obj) => Equals(obj as ChatCommand);

        public override int GetHashCode() => HashCode.Combine(Kind, Amount);

        public override string ToString() => $"{Kind} {Amount}";
    }

    /// <summary>
    /// Parses roulette chat commands typed by any player (engine-free). Current commands are diagnostics for checking
    /// that the host can move points on players who don't have the mod:
    ///   !rtest pay &lt;points&gt;     credit the sender
    ///   !rtest charge [count]     charge the sender one Big Wheel spin cost per count
    /// </summary>
    public static class ChatCommandParser
    {
        public const string TestPrefix = "!rtest";
        public const int MaxTestPayPoints = 1_000_000;
        public const int MaxTestCharges = 20;

        public static bool TryParse(string message, out ChatCommand command)
        {
            command = null;
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            var parts = message.Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (!parts[0].Equals(TestPrefix, StringComparison.OrdinalIgnoreCase) || parts.Length < 2)
            {
                return false;
            }

            var verb = parts[1].ToLowerInvariant();
            if (verb == "pay" && parts.Length == 3 && TryPositive(parts[2], MaxTestPayPoints, out var points))
            {
                command = new ChatCommand(ChatCommandKind.TestPay, points);
                return true;
            }

            if (verb == "charge" && parts.Length <= 3)
            {
                var count = 1;
                if (parts.Length == 3 && !TryPositive(parts[2], MaxTestCharges, out count))
                {
                    return false;
                }

                command = new ChatCommand(ChatCommandKind.TestCharge, count);
                return true;
            }

            return false;
        }

        private static bool TryPositive(string text, int max, out int value) =>
            int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value) && value > 0 && value <= max;
    }
}
