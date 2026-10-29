using System;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace RouletteRevival.SpinDispatch
{
    /// <summary>
    /// Games with the mod (host included) undo the host's result delay, so board bets still pay the moment the ball lands.
    /// Runs before <see cref="TableLock.SpinResultPatch"/>, which then sees the undelayed time too.
    /// </summary>
    [HarmonyPatch(typeof(RouletteTable), nameof(RouletteTable.OnSpin))]
    internal static class SpinResultTimingPatch
    {
        private static void Prefix(RouletteTable __instance, ref float spinTime)
        {
            try
            {
                var spinner = __instance.spinner;
                if (spinner != null)
                {
                    spinTime = SpinTiming.LocalResultTime(spinTime, spinner.spinTime);
                }
            }
            catch (Exception ex)
            {
                Melon<RouletteRevivalMod>.Logger.Warning($"Spin result timing check failed; using the received time: {ex.Message}");
            }
        }
    }
}
