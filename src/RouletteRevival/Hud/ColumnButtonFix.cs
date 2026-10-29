using System;
using System.Collections.Generic;
using Il2Cpp;
using MelonLoader;
using RouletteRevival.ChatBetting;

namespace RouletteRevival.Hud
{
    /// <summary>
    /// Points each "2 to 1" button of a table's betting grid at the column of the number row beside it (see
    /// <see cref="ColumnRows"/>). Reads the row from a number column, never the button's current value, so applying it again
    /// changes nothing.
    /// </summary>
    public static class ColumnButtonFix
    {
        private static MelonLogger.Instance Log => Melon<RouletteRevivalMod>.Logger;

        /// <summary>Never throws: a failure here must not stop the rest of the betting menu from being wired up.</summary>
        public static void Apply(RouletteTable table)
        {
            try
            {
                MatchColumnsToRows(table);
            }
            catch (Exception ex)
            {
                Log.Warning($"Couldn't check the betting menu's '2 to 1' buttons: {ex.Message}");
            }
        }

        private static void MatchColumnsToRows(RouletteTable table)
        {
            var groups = ButtonsByParent(table);
            var columnButtons = FindGroup(groups, IsColumnGroup);
            var numberColumn = FindGroup(groups, g => columnButtons != null && g.Count == columnButtons.Count && IsNumberGroup(g));
            if (columnButtons == null || numberColumn == null)
            {
                Log.Warning("Couldn't find the betting menu's '2 to 1' buttons; column bets may land on the wrong row.");
                return;
            }

            var fixedCount = 0;
            for (var i = 0; i < columnButtons.Count; i++)
            {
                var column = ColumnRows.ColumnOf(numberColumn[i].betNumber);
                if (column.HasValue && (int)columnButtons[i].betType != (int)column.Value)
                {
                    columnButtons[i].betType = (RouletteBetType)(int)column.Value;
                    fixedCount++;
                }
            }

            if (fixedCount > 0)
            {
                Log.Msg($"Matched {fixedCount} '2 to 1' button(s) to the number rows beside them.");
            }
        }

        /// <summary>Grid buttons grouped by their layout column, in on-screen order (top to bottom).</summary>
        private static List<List<RouletteButtonUI>> ButtonsByParent(RouletteTable table)
        {
            var groups = new List<List<RouletteButtonUI>>();
            var parents = new List<IntPtr>();
            foreach (var button in table.GetComponentsInChildren<RouletteButtonUI>(true))
            {
                var parent = button.transform.parent;
                var key = parent != null ? parent.Pointer : IntPtr.Zero;
                var index = parents.IndexOf(key);
                if (index < 0)
                {
                    parents.Add(key);
                    groups.Add(new List<RouletteButtonUI>());
                    index = groups.Count - 1;
                }

                groups[index].Add(button);
            }

            return groups;
        }

        private static List<RouletteButtonUI> FindGroup(List<List<RouletteButtonUI>> groups, Func<List<RouletteButtonUI>, bool> match)
        {
            foreach (var group in groups)
            {
                if (match(group))
                {
                    return group;
                }
            }

            return null;
        }

        private static bool IsColumnGroup(List<RouletteButtonUI> group)
        {
            foreach (var button in group)
            {
                if (!ColumnRows.IsColumn((BetKind)(int)button.betType))
                {
                    return false;
                }
            }

            return group.Count > 0;
        }

        private static bool IsNumberGroup(List<RouletteButtonUI> group)
        {
            foreach (var button in group)
            {
                if (!ColumnRows.ColumnOf(button.betNumber).HasValue || (int)button.betType != (int)BetKind.Number)
                {
                    return false;
                }
            }

            return group.Count > 0;
        }
    }
}
