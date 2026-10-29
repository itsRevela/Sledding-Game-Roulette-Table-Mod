using Il2Cpp;
using UnityEngine;
using GameBet = Il2Cpp.RouletteTable.RouletteBet;

namespace RouletteRevival.ChatBetting
{
    /// <summary>The roulette table as seen by chat betting: spin state, chip unit and the game's own payout rules. Server-only.</summary>
    public static class TableBridge
    {
        /// <summary>Fallback chip unit if the arcade Big Wheel can't be found (its spin cost in the shipped scene).</summary>
        public const long DefaultChargeUnit = 5000;

        public static RouletteTableNetworked FindTable() => Object.FindObjectOfType<RouletteTableNetworked>();

        public static bool IsSpinning()
        {
            var spinner = Object.FindObjectOfType<RouletteSpinner>();
            return spinner != null && spinner.IsSpinning;
        }

        /// <summary>Charges come in Big Wheel spin costs, so bets must be multiples of it.</summary>
        public static long ChargeUnit()
        {
            var wheel = Object.FindObjectOfType<ArcadeWheelSpinInteractable>();
            return wheel != null && wheel.costOfSpin > 0 ? wheel.costOfSpin : DefaultChargeUnit;
        }

        /// <summary>The game's return for a bet (bet × odds + bet) if it wins on <paramref name="result"/>, else 0.</summary>
        public static long ReturnFor(PlacedBet bet, int result)
        {
            var gameBet = ToGameBet(bet);
            return RouletteTable.IsWinningBet(gameBet, result) ? RouletteTable.CalculatePayout(gameBet) : 0;
        }

        private static GameBet ToGameBet(PlacedBet bet) =>
            new GameBet((RouletteBetType)(int)bet.Spot.Kind, bet.Spot.Number, bet.Amount);
    }
}
