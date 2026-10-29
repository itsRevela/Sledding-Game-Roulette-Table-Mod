using FluentAssertions;
using RouletteRevival.ChatBetting;
using Xunit;

namespace RouletteRevival.Tests.ChatBetting
{
    public sealed class ChatLineFormatTests
    {
        [Fact]
        public void Line_PrefixesTheSender()
        {
            ChatLineFormat.Line("17 black!").Should().Be("Roulette: 17 black!");
        }

        [Fact]
        public void InSystemTemplate_HidesTheTemplateWordingAfterTheName()
        {
            // The game renders "{Username} has joined the game"; everything after our text must be invisible.
            var rendered = "{Username} has joined the game".Replace("{Username}", ChatLineFormat.InSystemTemplate("17 black!"));

            rendered.Should().Be("Roulette: 17 black!<size=0> has joined the game");
            rendered.Substring(0, rendered.IndexOf(ChatLineFormat.HideRest)).Should().Be("Roulette: 17 black!");
        }

        [Fact]
        public void InSystemTemplate_HasNoBracesThatTheTemplateEngineWouldParse()
        {
            ChatLineFormat.InSystemTemplate("Help 1/3: !bet <spot> <amount>").Should().NotContainAny("{", "}");
        }
    }
}
