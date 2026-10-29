using Il2CppFishNet.Connection;
using RouletteRevival.Spawning;

namespace RouletteRevival.ChatBetting
{
    /// <summary>Decides whether a chat line is a roulette command this host handles; handled commands are not broadcast.</summary>
    public static class ChatCommandRouter
    {
        /// <returns>true when the message was a roulette command and has been handled (so it must not reach chat).</returns>
        public static bool TryHandle(NetworkConnection sender, string message)
        {
            var settings = RouletteRevivalMod.Settings;
            if (settings == null || sender == null || !RouletteTableSpawner.IsHosting)
            {
                return false;
            }

            if (!ChatCommandParser.TryParse(message, out var command))
            {
                return false;
            }

            if (!settings.ChatBetting)
            {
                return false;
            }

            ChatBetService.Instance.Handle(sender, command, settings.ChatBetMaxPerSpin);
            return true;
        }
    }
}
