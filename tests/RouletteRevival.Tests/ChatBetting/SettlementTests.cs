using System;
using FluentAssertions;
using RouletteRevival.ChatBetting;
using Xunit;

namespace RouletteRevival.Tests.ChatBetting
{
    public sealed class SettlementTests
    {
        private const long Unit = 5000;
        private static readonly PlacedBet RedTen = new PlacedBet(BetSpot.Outside(BetKind.Red), 10_000);
        private static readonly PlacedBet SeventeenFive = new PlacedBet(BetSpot.ForNumber(17), 5000);

        [Fact]
        public void Win_PaysNetProfit()
        {
            // Red pays 1:1, so 10,000 returns 20,000 (stake included).
            var result = Settlement.Settle(new[] { RedTen }, b => b.Amount * 2, Unit);

            result.Net.Should().Be(10_000);
            result.PayAmount.Should().Be(10_000);
            result.ChargeCount.Should().Be(0);
        }

        [Fact]
        public void Loss_ChargesWholeStakeInUnits()
        {
            var result = Settlement.Settle(new[] { RedTen, SeventeenFive }, _ => 0, Unit);

            result.Net.Should().Be(-15_000);
            result.PayAmount.Should().Be(0);
            result.ChargeCount.Should().Be(3);
        }

        [Fact]
        public void MixedResult_UsesNet()
        {
            // 17 wins at 35:1 (5,000 returns 180,000); red loses.
            var result = Settlement.Settle(new[] { RedTen, SeventeenFive }, b => b.Spot.Number == 17 ? b.Amount * 36 : 0, Unit);

            result.Staked.Should().Be(15_000);
            result.Returned.Should().Be(180_000);
            result.PayAmount.Should().Be(165_000);
        }

        [Fact]
        public void BreakEven_DoesNothing()
        {
            var result = Settlement.Settle(new[] { RedTen }, b => b.Amount, Unit);

            result.PayAmount.Should().Be(0);
            result.ChargeCount.Should().Be(0);
        }

        [Fact]
        public void ChargeCount_RoundsUpPartialUnits()
        {
            new SettlementResult(7000, 0, Unit).ChargeCount.Should().Be(2);
        }

        [Fact]
        public void NullArguments_Throw()
        {
            Action noBets = () => Settlement.Settle(null, _ => 0, Unit);
            Action noRule = () => Settlement.Settle(new[] { RedTen }, null, Unit);

            noBets.Should().Throw<ArgumentNullException>();
            noRule.Should().Throw<ArgumentNullException>();
        }
    }
}
