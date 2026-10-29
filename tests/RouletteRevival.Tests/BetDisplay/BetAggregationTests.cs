using System.Collections.Generic;
using FluentAssertions;
using RouletteRevival.BetDisplay;
using RouletteRevival.ChatBetting;
using Xunit;

namespace RouletteRevival.Tests.BetDisplay
{
    public sealed class BetAggregationTests
    {
        private static readonly BetSpot Red = BetSpot.Outside(BetKind.Red);
        private static readonly BetSpot Seventeen = BetSpot.ForNumber(17);

        [Fact]
        public void SumBySpot_AddsDifferentPlayersOnTheSameSpot()
        {
            // The reported case: host 10,000 on red (board) and a friend 5,000 on red (chat).
            var host = new[] { new PlacedBet(Red, 10_000) };
            var friend = new[] { new PlacedBet(Red, 5000) };

            BetAggregation.SumBySpot(new IReadOnlyList<PlacedBet>[] { host, friend })
                .Should().Equal(new PlacedBet(Red, 15_000));
        }

        [Fact]
        public void SumBySpot_KeepsSeparateSpotsSeparate()
        {
            var a = new[] { new PlacedBet(Red, 5000), new PlacedBet(Seventeen, 10) };
            var b = new[] { new PlacedBet(Seventeen, 20) };

            BetAggregation.SumBySpot(new IReadOnlyList<PlacedBet>[] { a, b })
                .Should().Equal(new PlacedBet(Seventeen, 30), new PlacedBet(Red, 5000));
        }

        [Fact]
        public void SumBySpot_IgnoresEmptyNullAndZeroAmounts()
        {
            var lists = new IReadOnlyList<PlacedBet>[] { null, new PlacedBet[0], new[] { new PlacedBet(Red, 0) } };

            BetAggregation.SumBySpot(lists).Should().BeEmpty();
            BetAggregation.SumBySpot(null).Should().BeEmpty();
        }

        [Fact]
        public void SumBySpot_OrderIsStable()
        {
            var lists = new IReadOnlyList<PlacedBet>[] { new[] { new PlacedBet(Red, 1), new PlacedBet(BetSpot.ForNumber(3), 1), new PlacedBet(BetSpot.ForNumber(1), 1) } };

            BetAggregation.SumBySpot(lists).Should().Equal(
                new PlacedBet(BetSpot.ForNumber(1), 1), new PlacedBet(BetSpot.ForNumber(3), 1), new PlacedBet(Red, 1));
        }
    }
}
