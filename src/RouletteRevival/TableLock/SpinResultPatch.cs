using System;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using RouletteRevival.BetDisplay;
using RouletteRevival.ChatBetting;
using RouletteRevival.Spawning;

namespace RouletteRevival.TableLock
{
    /// <summary>
    /// Every game receives the wheel's result through RouletteTable.OnSpin. Record the real spin (for the lock), and on the
    /// host lock in this spin's chat bets for settlement.
    /// </summary>
    [HarmonyPatch(typeof(RouletteTable), nameof(RouletteTable.OnSpin))]
    internal static class SpinResultPatch
    {
        private static void Postfix(int result, float spinTime)
        {
            try
            {
                RealSpin.Started(spinTime);
                if (RouletteTableSpawner.IsHosting)
                {
                    // The game empties the shared bet list on every spin; keep the combined display in step.
                    TableBetDisplay.Clear();
                }

                var settings = RouletteRevivalMod.Settings;
                if (settings != null && settings.ChatBetting && RouletteTableSpawner.IsHosting)
                {
                    ChatBetService.Instance.OnSpin(result, spinTime);
                }
            }
            catch (Exception ex)
            {
                Melon<RouletteRevivalMod>.Logger.Error($"Spin result handling failed: {ex}");
            }
        }
    }
}
