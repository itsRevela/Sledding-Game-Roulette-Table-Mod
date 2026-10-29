using Il2Cpp;
using UnityEngine;

namespace RouletteRevival.Hud
{
    /// <summary>
    /// Points the scene's MenuType.RouletteTable entry in <c>UiReferenceController.interactionMenus</c> at a panel.
    /// The entry exists but its panel was nulled when the feature was sunset; OpenMenu dereferences it without a check.
    /// </summary>
    public static class InteractionMenuWriter
    {
        public static void SetRouletteMenu(UiReferenceController ui, GameObject panel, GameObject firstSelectable)
        {
            var menus = ui.interactionMenus;

            // Entries are value types held by the list: build a fresh pair and write it back through the indexer.
            var pair = new UiReferenceController.InteractionMenuPair
            {
                menuType = MenuType.RouletteTable,
                menu = new UiReferenceController.UiToggleableMenu(panel, firstSelectable),
            };

            for (var i = 0; i < menus.Count; i++)
            {
                if (menus[i].menuType == MenuType.RouletteTable)
                {
                    menus[i] = pair;
                    return;
                }
            }

            menus.Add(pair);
        }
    }
}
