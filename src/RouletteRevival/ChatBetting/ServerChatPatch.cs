using System;
using HarmonyLib;
using Il2Cpp_Scripts.Systems.Chat;
using Il2CppFishNet.Connection;
using MelonLoader;
using RouletteRevival.Spawning;

namespace RouletteRevival.ChatBetting
{
    /// <summary>
    /// Server side of chat: every player's message (the host's included) arrives here with the sender's connection,
    /// before the game re-broadcasts it. Roulette commands are handled and swallowed here, so they never show in chat.
    /// Only active while this game is the host; other chat passes through untouched.
    /// </summary>
    [HarmonyPatch(typeof(ChatManagerBase), "OnServerReceivedChatBroadcastFromClient")]
    internal static class ServerChatPatch
    {
        private static bool _loggedFirstMessage;

        private static MelonLogger.Instance Log => Melon<RouletteRevivalMod>.Logger;

        /// <returns>false to skip the game's handler (no broadcast) when the message was a roulette command.</returns>
        private static bool Prefix(NetworkConnection networkConnection, ChatMessage chatMessage)
        {
            try
            {
                var settings = RouletteRevivalMod.Settings;
                if (settings == null || !settings.PointTestCommands || networkConnection == null || !RouletteTableSpawner.IsHosting)
                {
                    return true;
                }

                var text = chatMessage?.Message;
                if (!_loggedFirstMessage)
                {
                    _loggedFirstMessage = true;
                    Log.Msg($"[chat] Server chat hook active (first message from client {networkConnection.ClientId}: \"{Truncate(text)}\").");
                }

                if (!ChatCommandParser.TryParse(text, out var command))
                {
                    return true;
                }

                PointTestCommands.Execute(networkConnection, command);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"Roulette chat command failed; letting the message through: {ex}");
                return true;
            }
        }

        private static string Truncate(string text) =>
            text == null ? "<null>" : text.Length <= 40 ? text : text.Substring(0, 40) + "…";
    }
}
