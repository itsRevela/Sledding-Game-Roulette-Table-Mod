using System;
using HarmonyLib;
using Il2Cpp_Scripts.Systems.Chat;
using Il2CppFishNet;
using MelonLoader;

namespace RouletteRevival.ChatBetting
{
    /// <summary>
    /// The host's own chat never reaches <see cref="ServerChatPatch"/>: ChatManagerBase.SendChatMessage broadcasts it
    /// straight from the server ("Sending chat message as host"). Catch roulette commands here instead and don't send them.
    /// </summary>
    [HarmonyPatch(typeof(ChatManagerBase), "SendChatMessage")]
    internal static class HostChatPatch
    {
        /// <returns>false to skip sending when the host typed a roulette command.</returns>
        private static bool Prefix(string cleanedString)
        {
            try
            {
                var hostConnection = InstanceFinder.ClientManager != null ? InstanceFinder.ClientManager.Connection : null;
                return !ChatCommandRouter.TryHandle(hostConnection, cleanedString);
            }
            catch (Exception ex)
            {
                Melon<RouletteRevivalMod>.Logger.Error($"Roulette host chat command failed; sending it as normal chat: {ex}");
                return true;
            }
        }
    }
}
