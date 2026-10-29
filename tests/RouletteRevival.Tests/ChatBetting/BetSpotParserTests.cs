using FluentAssertions;
using RouletteRevival.ChatBetting;
using Xunit;

namespace RouletteRevival.Tests.ChatBetting
{
    public sealed class BetSpotParserTests
    {
        [Theory]
        [InlineData("0", 0)]
        [InlineData("17", 17)]
        [InlineData("36", 36)]
        public void TryParse_Numbers(string text, int number)
        {
            BetSpotParser.TryParse(text, out var spot).Should().BeTrue();
            spot.Should().Be(BetSpot.ForNumber(number));
        }

        [Theory]
        [InlineData("red", BetKind.Red)]
        [InlineData("BLACK", BetKind.Black)]
        [InlineData("odd", BetKind.Odd)]
        [InlineData("even", BetKind.Even)]
        [InlineData("low", BetKind.LowHalf)]
        [InlineData("1-18", BetKind.LowHalf)]
        [InlineData("high", BetKind.HighHalf)]
        [InlineData("19-36", BetKind.HighHalf)]
        [InlineData("dozen1", BetKind.Dozen1)]
        [InlineData("2nd12", BetKind.Dozen2)]
        [InlineData("3rd12", BetKind.Dozen3)]
        [InlineData("col1", BetKind.Column1)]
        [InlineData("column2", BetKind.Column2)]
        [InlineData("col3", BetKind.Column3)]
        public void TryParse_OutsideBets_UseNumberMinusOne(string text, BetKind kind)
        {
            BetSpotParser.TryParse(text, out var spot).Should().BeTrue();
            spot.Should().Be(new BetSpot(kind, BetSpot.OutsideNumber));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("37")]
        [InlineData("-1")]
        [InlineData("green")]
        [InlineData("00")]
        [InlineData("dozen4")]
        public void TryParse_RejectsUnknownSpots(string text)
        {
            BetSpotParser.TryParse(text, out _).Should().BeFalse();
        }

        [Fact]
        public void ToString_IsReadable()
        {
            BetSpot.ForNumber(17).ToString().Should().Be("17");
            BetSpot.Outside(BetKind.Dozen2).ToString().Should().Be("2nd 12");
        }
    }
}
