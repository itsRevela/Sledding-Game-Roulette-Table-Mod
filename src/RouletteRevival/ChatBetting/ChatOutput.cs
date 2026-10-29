using System;
using Il2Cpp_Scripts.Systems.Chat;
using Il2CppFishNet;
using Il2CppFishNet.Connection;
using Il2CppFishNet.Transporting;
using MelonLoader;

namespace RouletteRevival.ChatBetting
{
    /// <summary>
    /// Roulette messages in the game's chat log, with no speech bubble. Server-only.
    /// Player chat must come from a real lobby player (or every game drops it) and draws a bubble over that player, so
    /// roulette lines go out as a system message instead: those skip both. The game words system messages through
    /// fixed templates ("{Username} has joined the game"), so the line rides in the name field and a zero-size tag hides
    /// the rest of the template. See <see cref="ChatLineFormat"/>.
    /// </summary>
    public static class ChatOutput
    {
        private static MelonLogger.Instance Log => Melon<RouletteRevivalMod>.Logger;

        /// <summary>Table talk everyone should see: bets placed, spins, results.</summary>
        public static void ToEveryone(string text)
        {
            try
            {
                InstanceFinder.ServerManager.Broadcast(Message(text), true, Channel.Reliable);
            }
            catch (Exception ex)
            {
                Log.Warning($"Roulette chat broadcast failed: {ex.Message}");
            }
        }

        /// <summary>Replies only the asking player should see: help, their bet list, errors.</summary>
        public static void ToPlayer(NetworkConnection player, string text)
        {
            if (player.IsLocalClient)
            {
                ChatManager.Instance.SendLocalSystemChatMessage(ChatLineFormat.Line(text));
                return;
            }

            try
            {
                InstanceFinder.ServerManager.Broadcast(player, Message(text), true, Channel.Reliable);
            }
            catch (Exception ex)
            {
                Log.Warning($"Private roulette message to client {player.ClientId} failed: {ex.Message}");
            }
        }

        private static ChatMessage Message(string text)
        {
            var message = new ChatMessage(string.Empty, ChatMessageType.System)
            {
                Username = ChatLineFormat.InSystemTemplate(text),
                UserProductId = string.Empty,
            };
            message.SetSystemMessageType(SystemMessageType.PlayerJoined);
            return message;
        }
    }
}
