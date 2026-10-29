using System;
using HarmonyLib;
using Il2Cpp_Scripts.Systems.Chat;
using Il2CppFishNet;
using MelonLoader;
using RouletteRevival.Spawning;

namespace RouletteRevival.ChatBetting
{
    /// <summary>
    /// The host's own chat never reaches <see cref="ServerChatPatch"/>: ChatManagerBase.SendChatMessage broadcasts it
    /// straight from the server ("Sending chat message as host"). Catch roulette commands here instead and don't send them.
    /// </summary>
    [HarmonyPatch(typeof(ChatManagerBase), "SendChatMessage")]
    internal static class HostChatPatch
    {
        private static MelonLogger.Instance Log => Melon<RouletteRevivalMod>.Logger;

        /// <returns>false to skip sending when the host typed a roulette command.</returns>
        private static bool Prefix(string cleanedString)
        {
            try
            {
                var settings = RouletteRevivalMod.Settings;
                if (settings == null || !settings.PointTestCommands || !RouletteTableSpawner.IsHosting)
                {
                    return true;
                }

                if (!ChatCommandParser.TryParse(cleanedString, out var command))
                {
                    return true;
                }

                var hostConnection = InstanceFinder.ClientManager != null ? InstanceFinder.ClientManager.Connection : null;
                if (hostConnection == null)
                {
                    Log.Warning("[chat] Host command ignored: no local client connection.");
                    return true;
                }

                PointTestCommands.Execute(hostConnection, command);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"Roulette host chat command failed; sending it as normal chat: {ex}");
                return true;
            }
        }
    }
}
