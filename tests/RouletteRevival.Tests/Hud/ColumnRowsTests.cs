using FluentAssertions;
using RouletteRevival.ChatBetting;
using RouletteRevival.Hud;
using Xunit;

namespace RouletteRevival.Tests.Hud
{
    public sealed class ColumnRowsTests
    {
        [Theory]
        [InlineData(1, BetKind.Column1)]
        [InlineData(34, BetKind.Column1)]
        [InlineData(2, BetKind.Column2)]
        [InlineData(35, BetKind.Column2)]
        [InlineData(3, BetKind.Column3)]
        [InlineData(36, BetKind.Column3)]
        public void ColumnOf_MatchesTheGamesColumns(int number, BetKind expected)
        {
            ColumnRows.ColumnOf(number).Should().Be(expected);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(37)]
        public void ColumnOf_OffTheColumns_IsNull(int number)
        {
            ColumnRows.ColumnOf(number).Should().BeNull();
        }

        [Fact]
        public void ColumnOf_ShippedMenuRows_ReverseTheShippedButtons()
        {
            // Top to bottom, the menu's number columns read 3, 2, 1 while its "2 to 1" buttons read Column1, 2, 3.
            new[] { ColumnRows.ColumnOf(3), ColumnRows.ColumnOf(2), ColumnRows.ColumnOf(1) }
                .Should().Equal(BetKind.Column3, BetKind.Column2, BetKind.Column1);
        }

        [Theory]
        [InlineData(BetKind.Column1, true)]
        [InlineData(BetKind.Column3, true)]
        [InlineData(BetKind.Dozen1, false)]
        [InlineData(BetKind.Number, false)]
        public void IsColumn(BetKind kind, bool expected)
        {
            ColumnRows.IsColumn(kind).Should().Be(expected);
        }
    }
}
