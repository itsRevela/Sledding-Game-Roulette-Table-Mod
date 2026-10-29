using System;
using System.Collections.Generic;
using System.Globalization;

namespace RouletteRevival.ChatBetting
{
    /// <summary>Mirrors the game's RouletteBetType values (only the kinds the table offers).</summary>
    public enum BetKind
    {
        Number = 1,
        Red = 2,
        Black = 3,
        Odd = 5,
        Even = 6,
        Dozen1 = 7,
        Dozen2 = 8,
        Dozen3 = 9,
        Column1 = 10,
        Column2 = 11,
        Column3 = 12,
        LowHalf = 13,
        HighHalf = 14,
    }

    /// <summary>A place on the betting board. Outside bets use number -1, matching the table's chip locations.</summary>
    public readonly struct BetSpot : IEquatable<BetSpot>
    {
        public const int OutsideNumber = -1;

        public BetSpot(BetKind kind, int number)
        {
            Kind = kind;
            Number = number;
        }

        public BetKind Kind { get; }
        public int Number { get; }

        public static BetSpot ForNumber(int number) => new BetSpot(BetKind.Number, number);

        public static BetSpot Outside(BetKind kind) => new BetSpot(kind, OutsideNumber);

        public bool Equals(BetSpot other) => Kind == other.Kind && Number == other.Number;

        public override bool Equals(object obj) => obj is BetSpot other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Kind, Number);

        public override string ToString() => Kind switch
        {
            BetKind.Number => Number.ToString(CultureInfo.InvariantCulture),
            BetKind.Red => "red",
            BetKind.Black => "black",
            BetKind.Odd => "odd",
            BetKind.Even => "even",
            BetKind.LowHalf => "1-18",
            BetKind.HighHalf => "19-36",
            BetKind.Dozen1 => "1st 12",
            BetKind.Dozen2 => "2nd 12",
            BetKind.Dozen3 => "3rd 12",
            BetKind.Column1 => "column 1",
            BetKind.Column2 => "column 2",
            BetKind.Column3 => "column 3",
            _ => Kind.ToString(),
        };
    }

    /// <summary>Parses what a player types for the spot in "!bet &lt;spot&gt; &lt;amount&gt;".</summary>
    public static class BetSpotParser
    {
        public const int MaxNumber = 36;

        private static readonly IReadOnlyDictionary<string, BetKind> Names = new Dictionary<string, BetKind>(StringComparer.OrdinalIgnoreCase)
        {
            ["red"] = BetKind.Red,
            ["black"] = BetKind.Black,
            ["odd"] = BetKind.Odd,
            ["even"] = BetKind.Even,
            ["low"] = BetKind.LowHalf,
            ["1-18"] = BetKind.LowHalf,
            ["high"] = BetKind.HighHalf,
            ["19-36"] = BetKind.HighHalf,
            ["dozen1"] = BetKind.Dozen1,
            ["1st12"] = BetKind.Dozen1,
            ["dozen2"] = BetKind.Dozen2,
            ["2nd12"] = BetKind.Dozen2,
            ["dozen3"] = BetKind.Dozen3,
            ["3rd12"] = BetKind.Dozen3,
            ["col1"] = BetKind.Column1,
            ["column1"] = BetKind.Column1,
            ["col2"] = BetKind.Column2,
            ["column2"] = BetKind.Column2,
            ["col3"] = BetKind.Column3,
            ["column3"] = BetKind.Column3,
        };

        public static bool TryParse(string text, out BetSpot spot)
        {
            spot = default;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var trimmed = text.Trim();
            if (int.TryParse(trimmed, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
            {
                // Canonical numbers only: "00" is an American-wheel pocket this European wheel doesn't have.
                if (number > MaxNumber || (trimmed.Length > 1 && trimmed[0] == '0'))
                {
                    return false;
                }

                spot = BetSpot.ForNumber(number);
                return true;
            }

            if (Names.TryGetValue(trimmed, out var kind))
            {
                spot = BetSpot.Outside(kind);
                return true;
            }

            return false;
        }
    }
}
