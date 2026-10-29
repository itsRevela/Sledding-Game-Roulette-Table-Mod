using FluentAssertions;
using RouletteRevival.ChatBetting;
using Xunit;

namespace RouletteRevival.Tests.ChatBetting
{
    public sealed class AmountParserTests
    {
        [Theory]
        [InlineData("5000", 5000)]
        [InlineData("10,000", 10000)]
        [InlineData("5k", 5000)]
        [InlineData("15K", 15000)]
        [InlineData("2.5k", 2500)]
        [InlineData("1m", 1000000)]
        public void TryParse_ValidAmounts(string text, long expected)
        {
            AmountParser.TryParse(text, out var amount).Should().BeTrue();
            amount.Should().Be(expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("0")]
        [InlineData("-5000")]
        [InlineData("1.5")]
        [InlineData("abc")]
        [InlineData("5kk")]
        [InlineData("2000m")]
        public void TryParse_RejectsInvalidAmounts(string text)
        {
            AmountParser.TryParse(text, out _).Should().BeFalse();
        }
    }
}
