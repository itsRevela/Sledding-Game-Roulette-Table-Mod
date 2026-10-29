using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppFishNet.Connection;
using MelonLoader;
using RouletteRevival.Spawning;

namespace RouletteRevival.BetDisplay
{
    /// <summary>
    /// Runs on the host after a player's board bets arrive (the game's handler is merged into this reader, so this is the
    /// hook point). The game has just stored them under the sender's player id; fold them into the combined display.
    /// </summary>
    [HarmonyPatch(typeof(RouletteTableNetworked), nameof(RouletteTableNetworked.RpcReader___Cmd_SetBetList___2449647308))]
    internal static class BoardBetsPatch
    {
        private static void Postfix(RouletteTableNetworked __instance, NetworkConnection conn)
        {
            try
            {
                if (!RouletteTableSpawner.IsHosting || conn == null)
                {
                    return;
                }

                // The game keys the list by the sender's player object id (PlayerControl.LocalPlayerInstance.ObjectId).
                var first = conn.FirstObject;
                var player = first != null ? first.GetComponent<PlayerControl>() : null;
                TableBetDisplay.AbsorbBoardBets(__instance, player != null ? player.ObjectId : (int?)null);
            }
            catch (Exception ex)
            {
                Melon<RouletteRevivalMod>.Logger.Warning($"Combining table chips failed; the game's own display stays: {ex.Message}");
            }
        }
    }
}
