using System;
using HarmonyLib;
using Il2Cpp_Scripts.Systems.Chat;
using Il2CppFishNet.Connection;
using MelonLoader;

namespace RouletteRevival.ChatBetting
{
    /// <summary>
    /// Server side of chat for players who joined the host: their messages arrive here with the sender's connection,
    /// before the game re-broadcasts them. Roulette commands are handled and swallowed so they never show in chat.
    /// </summary>
    [HarmonyPatch(typeof(ChatManagerBase), "OnServerReceivedChatBroadcastFromClient")]
    internal static class ServerChatPatch
    {
        /// <returns>false to skip the game's handler (no broadcast) when the message was a roulette command.</returns>
        private static bool Prefix(NetworkConnection networkConnection, ChatMessage chatMessage)
        {
            try
            {
                return !ChatCommandRouter.TryHandle(networkConnection, chatMessage?.Message);
            }
            catch (Exception ex)
            {
                Melon<RouletteRevivalMod>.Logger.Error($"Roulette chat command failed; letting the message through: {ex}");
                return true;
            }
        }
    }
}
