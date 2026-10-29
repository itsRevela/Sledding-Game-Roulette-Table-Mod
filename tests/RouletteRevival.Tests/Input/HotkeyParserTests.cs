using FluentAssertions;
using RouletteRevival.Input;
using Xunit;

namespace RouletteRevival.Tests.Input
{
    public sealed class HotkeyParserTests
    {
        [Theory]
        [InlineData("F1", 1)]
        [InlineData("F8", 8)]
        [InlineData("f12", 12)]
        [InlineData("  F5 ", 5)]
        public void TryParseFunctionKey_ValidNames_ReturnsNumber(string text, int expected)
        {
            HotkeyParser.TryParseFunctionKey(text, out var number).Should().BeTrue();
            number.Should().Be(expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("F")]
        [InlineData("F0")]
        [InlineData("F13")]
        [InlineData("G8")]
        [InlineData("F-1")]
        [InlineData("F+8")]
        [InlineData("Space")]
        public void TryParseFunctionKey_InvalidNames_ReturnsFalse(string text)
        {
            HotkeyParser.TryParseFunctionKey(text, out var number).Should().BeFalse();
            number.Should().Be(0);
        }
    }
}
