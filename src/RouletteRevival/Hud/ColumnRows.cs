using RouletteRevival.ChatBetting;

namespace RouletteRevival.Hud
{
    /// <summary>
    /// Engine-free: the game's columns are 1, 4 … 34 (Column1), 2, 5 … 35 (Column2) and 3, 6 … 36 (Column3), and it draws and
    /// pays column bets that way. Its betting menu ships the three "2 to 1" buttons in the reverse order of the number rows
    /// beside them, so the button next to 3-36 bet on 1-34. Each button's column follows from the numbers in its row.
    /// </summary>
    public static class ColumnRows
    {
        /// <summary>The column holding <paramref name="number"/>, or null for 0 and anything off the board.</summary>
        public static BetKind? ColumnOf(int number)
        {
            if (number < 1 || number > BetSpotParser.MaxNumber)
            {
                return null;
            }

            return (number % 3) switch
            {
                1 => BetKind.Column1,
                2 => BetKind.Column2,
                _ => BetKind.Column3,
            };
        }

        public static bool IsColumn(BetKind kind) =>
            kind == BetKind.Column1 || kind == BetKind.Column2 || kind == BetKind.Column3;
    }
}
