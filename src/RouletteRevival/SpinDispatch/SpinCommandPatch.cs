using System;
using Il2CppFishNet;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using RouletteRevival.TableLock;

namespace RouletteRevival.SpinDispatch
{
    /// <summary>
    /// Host: runs the wheel's spin command as the game does, except the table's result goes out
    /// <see cref="SpinTiming.ResultDelaySeconds"/> after the wheel's timer. Covers every F press (each game sends the command
    /// to the host) and chat !spin.
    /// </summary>
    [HarmonyPatch(typeof(RouletteSpinner), nameof(RouletteSpinner.RpcLogic___Cmd_SpinRoulette___2166136261))]
    internal static class SpinCommandPatch
    {
        private const int Pockets = 37; // 0-36, as the game draws it

        private static bool Prefix(RouletteSpinner __instance)
        {
            var started = false;
            try
            {
                var table = __instance.rouletteTableNetworked;
                var timeManager = InstanceFinder.TimeManager;
                if (!__instance.IsServerInitialized || table == null || timeManager == null)
                {
                    return true;
                }

                if (__instance.IsSpinning)
                {
                    return false;
                }

                var result = UnityEngine.Random.Range(0, Pockets);
                var spinTime = __instance.spinTime;
                started = true;
                __instance.sync_LastSpinNetworkTime.Value = timeManager.TicksToTime(timeManager.Tick);
                table.Server_OnSpin(result, SpinTiming.DelayedResultTime(spinTime));
                __instance.Rpc_SpinWheel(spinTime, result, __instance.stopWheelBeforeEndMultiplier);

                // No new spin until the delayed result has reached every game.
                RealSpin.Started(SpinTiming.DelayedResultTime(spinTime));
                return false;
            }
            catch (Exception ex)
            {
                Melon<RouletteRevivalMod>.Logger.Error($"Spin dispatch failed{(started ? "" : "; letting the game spin")}: {ex}");
                return !started;
            }
        }
    }
}
