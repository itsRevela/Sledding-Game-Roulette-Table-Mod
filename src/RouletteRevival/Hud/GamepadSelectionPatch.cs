using System;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace RouletteRevival.Hud
{
    /// <summary>
    /// On gamepad the game snaps the selection back into the top menu's panel every frame. The roulette bet grid is a
    /// sibling of that panel (both live on the table's world canvas), so without this the grid could never be selected.
    /// Selections anywhere on the roulette canvas are accepted while the roulette menu is on top.
    /// </summary>
    [HarmonyPatch(typeof(UiReferenceController), nameof(UiReferenceController.EnsureTopMenuHasValidSelection))]
    internal static class GamepadSelectionPatch
    {
        private static bool Prefix(UiReferenceController __instance)
        {
            var panel = RouletteHudBinder.ActivePanel;
            if (panel == null)
            {
                return true;
            }

            try
            {
                var top = __instance.GetCurrentOpenMenu();
                if (top == null || top.panel == null || top.panel.Pointer != panel.Pointer)
                {
                    return true;
                }

                var eventSystem = __instance.eventSystem;
                var selected = eventSystem != null ? eventSystem.currentSelectedGameObject : null;
                if (selected == null || !selected.activeInHierarchy)
                {
                    return true;
                }

                // Skip the game's correction only for selections on the roulette canvas itself.
                return !selected.transform.IsChildOf(panel.transform.parent);
            }
            catch (Exception ex)
            {
                Melon<RouletteRevivalMod>.Logger.Warning($"Gamepad selection check failed; using the game's default: {ex.Message}");
                return true;
            }
        }
    }
}
