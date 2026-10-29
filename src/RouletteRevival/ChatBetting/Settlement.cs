using System;
using System.Collections.Generic;
using System.Linq;

namespace RouletteRevival.ChatBetting
{
    /// <summary>One player's result for a spin (engine-free).</summary>
    public readonly struct SettlementResult
    {
        public SettlementResult(long staked, long returned, long chargeUnit)
        {
            Staked = staked;
            Returned = returned;
            ChargeUnit = chargeUnit;
        }

        /// <summary>Total chips the player had on the table.</summary>
        public long Staked { get; }

        /// <summary>What winning bets return, stake included (bet × odds + bet), as the game computes it.</summary>
        public long Returned { get; }

        public long ChargeUnit { get; }

        /// <summary>Net change to the player's points.</summary>
        public long Net => Returned - Staked;

        /// <summary>Points to credit when the player came out ahead.</summary>
        public long PayAmount => Net > 0 ? Net : 0;

        /// <summary>How many unit-sized charges cover a net loss (rounded up, so the house never under-collects).</summary>
        public long ChargeCount => Net < 0 && ChargeUnit > 0 ? (-Net + ChargeUnit - 1) / ChargeUnit : 0;
    }

    public static class Settlement
    {
        /// <param name="returnFor">The game's payout for a bet if it won, otherwise 0.</param>
        public static SettlementResult Settle(IReadOnlyList<PlacedBet> bets, Func<PlacedBet, long> returnFor, long chargeUnit)
        {
            if (bets == null)
            {
                throw new ArgumentNullException(nameof(bets));
            }

            if (returnFor == null)
            {
                throw new ArgumentNullException(nameof(returnFor));
            }

            return new SettlementResult(bets.Sum(b => b.Amount), bets.Sum(returnFor), chargeUnit);
        }
    }
}
