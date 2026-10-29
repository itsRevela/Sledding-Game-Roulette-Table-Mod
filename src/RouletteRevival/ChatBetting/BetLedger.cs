using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RouletteRevival.ChatBetting
{
    /// <summary>One player's stake on one spot.</summary>
    public readonly struct PlacedBet : IEquatable<PlacedBet>
    {
        public PlacedBet(BetSpot spot, long amount)
        {
            Spot = spot;
            Amount = amount;
        }

        public BetSpot Spot { get; }
        public long Amount { get; }

        public bool Equals(PlacedBet other) => Spot.Equals(other.Spot) && Amount == other.Amount;

        public override bool Equals(object obj) => obj is PlacedBet other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Spot, Amount);
    }

    public enum BetRejection
    {
        None,
        NotPositive,
        NotMultipleOfUnit,
        OverSpinLimit,
    }

    /// <summary>
    /// Chat bets for the current spin, keyed by the bettor's player object id (engine-free). State is held in an immutable
    /// map that each operation replaces, so snapshots handed out never change underneath their holder.
    /// </summary>
    public sealed class BetLedger
    {
        private ImmutableDictionary<int, ImmutableList<PlacedBet>> _bets = ImmutableDictionary<int, ImmutableList<PlacedBet>>.Empty;

        public bool IsEmpty => _bets.IsEmpty;

        public IReadOnlyList<PlacedBet> BetsOf(int playerId) =>
            _bets.TryGetValue(playerId, out var bets) ? bets : ImmutableList<PlacedBet>.Empty;

        public long TotalOf(int playerId) => BetsOf(playerId).Sum(b => b.Amount);

        /// <summary>Adds to the player's stake on a spot (merging with an existing bet there) if it passes the rules.</summary>
        public BetRejection TryAdd(int playerId, BetSpot spot, long amount, long unit, long maxPerSpin)
        {
            if (amount <= 0)
            {
                return BetRejection.NotPositive;
            }

            if (unit <= 0 || amount % unit != 0)
            {
                return BetRejection.NotMultipleOfUnit;
            }

            if (TotalOf(playerId) + amount > maxPerSpin)
            {
                return BetRejection.OverSpinLimit;
            }

            var current = _bets.TryGetValue(playerId, out var existing) ? existing : ImmutableList<PlacedBet>.Empty;
            var index = current.FindIndex(b => b.Spot.Equals(spot));
            var updated = index >= 0
                ? current.SetItem(index, new PlacedBet(spot, current[index].Amount + amount))
                : current.Add(new PlacedBet(spot, amount));
            _bets = _bets.SetItem(playerId, updated);
            return BetRejection.None;
        }

        /// <summary>Removes all of the player's bets; returns how much was staked.</summary>
        public long Clear(int playerId)
        {
            var total = TotalOf(playerId);
            _bets = _bets.Remove(playerId);
            return total;
        }

        /// <summary>Hands over every bet (for settling a spin) and starts the next spin empty.</summary>
        public IReadOnlyDictionary<int, IReadOnlyList<PlacedBet>> TakeAll()
        {
            var snapshot = _bets.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<PlacedBet>)kv.Value);
            _bets = ImmutableDictionary<int, ImmutableList<PlacedBet>>.Empty;
            return snapshot;
        }

        public void Reset() => _bets = ImmutableDictionary<int, ImmutableList<PlacedBet>>.Empty;
    }
}
