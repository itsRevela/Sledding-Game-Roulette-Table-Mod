using System.Collections.Generic;
using RouletteRevival.ChatBetting;

namespace RouletteRevival.BetDisplay
{
    /// <summary>
    /// Engine-free: the table draws one chip stack per spot and, when several bet lists name the same spot, keeps only the
    /// last one it reads. Summing every player's bets per spot into one list makes it draw the real totals.
    /// (Plain loops: some LINQ shapes make the compiler emit nullable attributes that clash with the game's interop.)
    /// </summary>
    public static class BetAggregation
    {
        public static IReadOnlyList<PlacedBet> SumBySpot(IEnumerable<IReadOnlyList<PlacedBet>> betLists)
        {
            var totals = new Dictionary<BetSpot, long>();
            if (betLists != null)
            {
                foreach (var list in betLists)
                {
                    if (list == null)
                    {
                        continue;
                    }

                    foreach (var bet in list)
                    {
                        if (bet.Amount > 0)
                        {
                            totals[bet.Spot] = (totals.TryGetValue(bet.Spot, out var sum) ? sum : 0) + bet.Amount;
                        }
                    }
                }
            }

            var result = new List<PlacedBet>(totals.Count);
            foreach (var entry in totals)
            {
                result.Add(new PlacedBet(entry.Key, entry.Value));
            }

            result.Sort(CompareSpots);
            return result;
        }

        private static int CompareSpots(PlacedBet a, PlacedBet b)
        {
            var byKind = ((int)a.Spot.Kind).CompareTo((int)b.Spot.Kind);
            return byKind != 0 ? byKind : a.Spot.Number.CompareTo(b.Spot.Number);
        }
    }
}
