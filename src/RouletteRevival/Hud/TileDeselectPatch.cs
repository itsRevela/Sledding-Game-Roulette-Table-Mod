using System;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace RouletteRevival.Hud
{
    /// <summary>
    /// Bets go to <c>UiReferenceController.currentSelectedRouletteButton</c>, which a tile sets when it becomes selected
    /// (mouse hover or gamepad focus). The game deselects the tile when the mouse leaves it, but the roulette code never
    /// listens for that, so a click anywhere kept betting on the last hovered tile. Clear it when that tile is deselected.
    /// </summary>
    [HarmonyPatch(typeof(AnimationSelectableUi), nameof(AnimationSelectableUi.Deselect))]
    internal static class TileDeselectPatch
    {
        private static void Postfix(AnimationSelectableUi __instance)
        {
            try
            {
                var ui = UiReferenceController.Instance;
                if (ui == null)
                {
                    return;
                }

                var current = ui.currentSelectedRouletteButton;
                if (current == null)
                {
                    return;
                }

                var tileSelectable = current.selectableUi;
                if (tileSelectable != null && tileSelectable.Pointer == __instance.Pointer)
                {
                    ui.currentSelectedRouletteButton = null;
                }
            }
            catch (Exception ex)
            {
                Melon<RouletteRevivalMod>.Logger.Warning($"Could not clear the deselected roulette tile: {ex.Message}");
            }
        }
    }
}
