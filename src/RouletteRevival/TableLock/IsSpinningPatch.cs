using System;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace RouletteRevival.TableLock
{
    /// <summary>
    /// On games with the mod (host included), see through the lock: while the synced spin time is parked, "is spinning"
    /// means the real spin state. The table's and wheel's interaction checks and the host's spin handler all read this.
    /// </summary>
    [HarmonyPatch(typeof(RouletteSpinner), nameof(RouletteSpinner.IsSpinning), MethodType.Getter)]
    internal static class IsSpinningPatch
    {
        private static bool _loggedLockSeen;

        private static void Postfix(RouletteSpinner __instance, ref bool __result)
        {
            try
            {
                if (SpinLockRules.IsLocked(__instance.sync_LastSpinNetworkTime.Value))
                {
                    if (!_loggedLockSeen)
                    {
                        // __result here is what a game without the mod computes: it must be true for the lock to hold.
                        _loggedLockSeen = true;
                        Melon<RouletteRevivalMod>.Logger.Msg($"Table lock seen: games without the mod read spinning={__result}; this game uses the real state ({RealSpin.IsSpinning}).");
                    }

                    __result = RealSpin.IsSpinning;
                }
            }
            catch (Exception ex)
            {
                Melon<RouletteRevivalMod>.Logger.Warning($"Spin state check failed; using the game's value: {ex.Message}");
            }
        }
    }
}
