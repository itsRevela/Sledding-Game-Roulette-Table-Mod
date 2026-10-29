using FluentAssertions;
using RouletteRevival.TableLock;
using Xunit;

namespace RouletteRevival.Tests.TableLock
{
    public sealed class SpinLockRulesTests
    {
        [Fact]
        public void IsLocked_ParkedValue()
        {
            SpinLockRules.IsLocked(SpinLockRules.LockedSpinTime).Should().BeTrue();
        }

        [Theory]
        [InlineData(0d)]
        [InlineData(1234.5)]
        [InlineData(86_400d * 365 * 100)] // a century of uptime is still a real timestamp
        public void IsLocked_RealTimestampsAreNot(double networkTime)
        {
            SpinLockRules.IsLocked(networkTime).Should().BeFalse();
        }

        [Fact]
        public void LockedValue_KeepsTheGameBelievingItSpins()
        {
            // The game: spinning while lastSpin + spinTime > now. Parked value must beat any realistic "now".
            const double centuryOfUptime = 86_400d * 365 * 100;
            (SpinLockRules.LockedSpinTime + 8d > centuryOfUptime).Should().BeTrue();
        }

        [Theory]
        [InlineData(10f, 9.9f, true)]
        [InlineData(10f, 10f, false)]
        [InlineData(0f, 5f, false)]
        public void IsRealSpinActive(float endsAt, float now, bool expected)
        {
            SpinLockRules.IsRealSpinActive(endsAt, now).Should().Be(expected);
        }
    }
}
