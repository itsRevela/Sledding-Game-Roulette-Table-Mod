using FluentAssertions;
using RouletteRevival.ChatBetting;
using Xunit;

namespace RouletteRevival.Tests.ChatBetting
{
    public sealed class ChatCommandParserTests
    {
        [Theory]
        [InlineData("!rtest pay 5000", 5000)]
        [InlineData("  !RTEST   Pay   1 ", 1)]
        [InlineData("!rtest pay 1000000", 1_000_000)]
        public void TryParse_Pay(string message, int points)
        {
            ChatCommandParser.TryParse(message, out var command).Should().BeTrue();
            command.Should().Be(new ChatCommand(ChatCommandKind.TestPay, points));
        }

        [Theory]
        [InlineData("!rtest charge", 1)]
        [InlineData("!rtest charge 3", 3)]
        [InlineData("!rtest CHARGE 20", 20)]
        public void TryParse_Charge(string message, int count)
        {
            ChatCommandParser.TryParse(message, out var command).Should().BeTrue();
            command.Should().Be(new ChatCommand(ChatCommandKind.TestCharge, count));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("hello everyone")]
        [InlineData("!rtest")]
        [InlineData("!rtest pay")]
        [InlineData("!rtest pay 0")]
        [InlineData("!rtest pay -5")]
        [InlineData("!rtest pay 1000001")]
        [InlineData("!rtest pay 5k")]
        [InlineData("!rtest pay 5000 extra")]
        [InlineData("!rtest charge 0")]
        [InlineData("!rtest charge 21")]
        [InlineData("!rtest charge 2 3")]
        [InlineData("!rtest refund 5")]
        [InlineData("!rtestpay 5000")]
        [InlineData("rtest pay 5000")]
        public void TryParse_RejectsAnythingElse(string message)
        {
            ChatCommandParser.TryParse(message, out var command).Should().BeFalse();
            command.Should().BeNull();
        }
    }
}
