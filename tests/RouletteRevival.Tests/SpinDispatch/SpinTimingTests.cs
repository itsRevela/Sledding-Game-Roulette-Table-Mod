using FluentAssertions;
using RouletteRevival.SpinDispatch;
using Xunit;

namespace RouletteRevival.Tests.SpinDispatch
{
    public sealed class SpinTimingTests
    {
        private const float WheelSpinTime = 7.5f;

        [Fact]
        public void DelayedResultTime_AddsTheOffset()
        {
            SpinTiming.DelayedResultTime(WheelSpinTime).Should().Be(WheelSpinTime + SpinTiming.ResultDelaySeconds);
        }

        [Fact]
        public void LocalResultTime_UndoesTheHostsDelay()
        {
            var received = SpinTiming.DelayedResultTime(WheelSpinTime);

            SpinTiming.LocalResultTime(received, WheelSpinTime).Should().Be(WheelSpinTime);
        }

        [Theory]
        [InlineData(7.5f)] // a host without the delay
        [InlineData(9f)]
        [InlineData(7.6f)]
        public void LocalResultTime_KeepsAnyOtherTime(float received)
        {
            SpinTiming.LocalResultTime(received, WheelSpinTime).Should().Be(received);
        }

        [Fact]
        public void Delay_ClearsTheFinishFrameButStaysInsideTheResultBounce()
        {
            // At 10 fps a frame is 0.1 s: the throw must land at least two frames after the wheel stops.
            SpinTiming.ResultDelaySeconds.Should().BeGreaterThanOrEqualTo(0.2f).And.BeLessThan(0.5f);
        }
    }
}
