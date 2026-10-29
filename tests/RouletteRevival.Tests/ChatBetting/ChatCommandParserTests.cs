using System.Linq;
using FluentAssertions;
using RouletteRevival.ChatBetting;
using Xunit;

namespace RouletteRevival.Tests.ChatBetting
{
    public sealed class ChatCommandParserTests
    {
        [Fact]
        public void TryParse_Bet()
        {
            ChatCommandParser.TryParse("!bet red 5000", out var red).Should().BeTrue();
            red.Should().Be(new ChatCommand(ChatCommandKind.Bet, 5000, BetSpot.Outside(BetKind.Red)));

            ChatCommandParser.TryParse("  !BET 17 10k ", out var number).Should().BeTrue();
            number.Should().Be(new ChatCommand(ChatCommandKind.Bet, 10_000, BetSpot.ForNumber(17)));
        }

        [Fact]
        public void TryParse_ClearAndList()
        {
            ChatCommandParser.TryParse("!clear", out var clear).Should().BeTrue();
            clear.Kind.Should().Be(ChatCommandKind.Clear);

            ChatCommandParser.TryParse("!bets", out var list).Should().BeTrue();
            list.Kind.Should().Be(ChatCommandKind.ListBets);

            ChatCommandParser.TryParse("!SPIN", out var spin).Should().BeTrue();
            spin.Kind.Should().Be(ChatCommandKind.Spin);
            ChatCommandParser.TryParse("!spin now", out _).Should().BeFalse();

            ChatCommandParser.TryParse("!Help", out var help).Should().BeTrue();
            help.Should().Be(new ChatCommand(ChatCommandKind.Help, 1));

            ChatCommandParser.TryParse("!help 2", out var page2).Should().BeTrue();
            page2.Should().Be(new ChatCommand(ChatCommandKind.Help, 2));

            ChatCommandParser.TryParse("!help two", out _).Should().BeFalse();
            ChatCommandParser.TryParse("!help 0", out _).Should().BeFalse();
            ChatCommandParser.TryParse("!help 1 2", out _).Should().BeFalse();
        }

        [Fact]
        public void Help_PagesTogetherMentionEveryCommandAndTheLimits()
        {
            var text = string.Join(" ", Enumerable.Range(1, ChatHelp.PageCount).SelectMany(p => ChatHelp.Page(p, 5000, 500_000)));

            text.Should().ContainAll("!bet", "!bets", "!clear", "!spin", "5,000", "500,000", "dozen1-3", "col1-3", "+175,000", "+10,000", "+5,000", "low (1-18)", "high (19-36)", "25-36");
        }

        [Fact]
        public void Help_PagesAreShortAndPointToTheNext()
        {
            for (var page = 1; page <= ChatHelp.PageCount; page++)
            {
                var lines = ChatHelp.Page(page, 5000, 500_000);

                lines.Should().HaveCountLessThanOrEqualTo(ChatHelp.MaxLinesPerPage);
                lines.Should().OnlyContain(l => l.Length <= 50, "the chat box is narrow");
                lines[0].Should().StartWith($"Help {page}/{ChatHelp.PageCount}");
                if (page < ChatHelp.PageCount)
                {
                    string.Join(" ", lines).Should().Contain($"!help {page + 1}");
                }
            }
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(99, ChatHelp.PageCount)]
        public void Help_OutOfRangePagesClamp(int requested, int shown)
        {
            ChatHelp.Page(requested, 5000, 500_000)[0].Should().StartWith($"Help {shown}/");
        }

        [Theory]
        [InlineData("!bet")]
        [InlineData("!bet red")]
        [InlineData("!bet purple 5000")]
        [InlineData("!bet red five")]
        [InlineData("!bet red 5000 extra")]
        [InlineData("!clear now")]
        [InlineData("!bets please")]
        [InlineData("/bet red 5000")]
        public void TryParse_RejectsMalformedBettingCommands(string message)
        {
            ChatCommandParser.TryParse(message, out _).Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("hello everyone")]
        public void TryParse_RejectsAnythingElse(string message)
        {
            ChatCommandParser.TryParse(message, out var command).Should().BeFalse();
            command.Should().BeNull();
        }
    }
}
