using FluentAssertions;
using RouletteRevival.ChatBetting;
using Xunit;

namespace RouletteRevival.Tests.ChatBetting
{
    public sealed class BetLedgerTests
    {
        private const long Unit = 5000;
        private const long Max = 50_000;
        private static readonly BetSpot Red = BetSpot.Outside(BetKind.Red);
        private static readonly BetSpot Seventeen = BetSpot.ForNumber(17);

        [Fact]
        public void TryAdd_RecordsBetAndTotal()
        {
            var ledger = new BetLedger();

            ledger.TryAdd(1, Red, 10_000, Unit, Max).Should().Be(BetRejection.None);

            ledger.BetsOf(1).Should().Equal(new PlacedBet(Red, 10_000));
            ledger.TotalOf(1).Should().Be(10_000);
        }

        [Fact]
        public void TryAdd_SameSpotMerges()
        {
            var ledger = new BetLedger();
            ledger.TryAdd(1, Red, 5000, Unit, Max);

            ledger.TryAdd(1, Red, 10_000, Unit, Max);

            ledger.BetsOf(1).Should().Equal(new PlacedBet(Red, 15_000));
        }

        [Fact]
        public void TryAdd_PlayersAreSeparate()
        {
            var ledger = new BetLedger();
            ledger.TryAdd(1, Red, 5000, Unit, Max);
            ledger.TryAdd(2, Seventeen, 5000, Unit, Max);

            ledger.BetsOf(1).Should().Equal(new PlacedBet(Red, 5000));
            ledger.BetsOf(2).Should().Equal(new PlacedBet(Seventeen, 5000));
        }

        [Theory]
        [InlineData(0, BetRejection.NotPositive)]
        [InlineData(-5000, BetRejection.NotPositive)]
        [InlineData(2500, BetRejection.NotMultipleOfUnit)]
        [InlineData(55_000, BetRejection.OverSpinLimit)]
        public void TryAdd_RejectsBadBets(long amount, BetRejection expected)
        {
            var ledger = new BetLedger();

            ledger.TryAdd(1, Red, amount, Unit, Max).Should().Be(expected);
            ledger.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void TryAdd_LimitCountsAllSpots()
        {
            var ledger = new BetLedger();
            ledger.TryAdd(1, Red, 45_000, Unit, Max);

            ledger.TryAdd(1, Seventeen, 10_000, Unit, Max).Should().Be(BetRejection.OverSpinLimit);
            ledger.TryAdd(1, Seventeen, 5000, Unit, Max).Should().Be(BetRejection.None);
        }

        [Fact]
        public void Clear_RemovesOnlyThatPlayerAndReturnsStake()
        {
            var ledger = new BetLedger();
            ledger.TryAdd(1, Red, 10_000, Unit, Max);
            ledger.TryAdd(2, Red, 5000, Unit, Max);

            ledger.Clear(1).Should().Be(10_000);

            ledger.BetsOf(1).Should().BeEmpty();
            ledger.TotalOf(2).Should().Be(5000);
        }

        [Fact]
        public void TakeAll_HandsOverSnapshotAndEmpties()
        {
            var ledger = new BetLedger();
            ledger.TryAdd(1, Red, 10_000, Unit, Max);

            var snapshot = ledger.TakeAll();
            ledger.TryAdd(1, Seventeen, 5000, Unit, Max);

            snapshot[1].Should().Equal(new PlacedBet(Red, 10_000));
            ledger.BetsOf(1).Should().Equal(new PlacedBet(Seventeen, 5000));
        }
    }
}
