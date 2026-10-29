using Il2Cpp;
using Il2CppFishNet;
using Il2CppFishNet.Connection;

namespace RouletteRevival.ChatBetting
{
    /// <summary>Who typed a command: the connection, their player object id (the table's bet key) and display name.</summary>
    public sealed class Bettor
    {
        public Bettor(int clientId, int playerId, string name)
        {
            ClientId = clientId;
            PlayerId = playerId;
            Name = name;
        }

        public int ClientId { get; }
        public int PlayerId { get; }
        public string Name { get; }
    }

    /// <summary>Resolves players from connections and back. Server-only.</summary>
    public static class Bettors
    {
        public static Bettor FromConnection(NetworkConnection connection)
        {
            var first = connection.FirstObject;
            var player = first != null ? first.GetComponent<PlayerControl>() : null;
            if (player == null)
            {
                return null;
            }

            return new Bettor(connection.ClientId, player.ObjectId, UsernameOf(connection, player));
        }

        /// <summary>
        /// The name the game shows in chat for this player. The game's own chat handler takes it from the player registry;
        /// the username controller comes back empty on the host for remote players.
        /// </summary>
        /// <remarks>
        /// Never call PlayerReferenceManager.TryGetPlayer from here: its interop wrapper gives the game an 8-byte slot for
        /// an out PlayerReference (a ~56-byte struct), which corrupts the stack and crashes the runtime. Reading the
        /// registry's synced list returns entries by value, which the interop handles safely.
        /// </remarks>
        private static string UsernameOf(NetworkConnection connection, PlayerControl player)
        {
            var registry = PlayerReferenceManager.Instance;
            var references = registry != null ? registry.sync_PlayerReferences : null;
            if (references != null)
            {
                for (var i = 0; i < references.Count; i++)
                {
                    var reference = references[i];
                    if (reference != null && reference.ConnectionID == connection.ClientId && !string.IsNullOrWhiteSpace(reference.Username))
                    {
                        return reference.Username;
                    }
                }
            }

            var fromController = player.usernameController != null ? player.usernameController.GetUsername() : null;
            return string.IsNullOrWhiteSpace(fromController) ? $"Player {connection.ClientId}" : fromController;
        }

        public static NetworkConnection ConnectionOf(int clientId)
        {
            var server = InstanceFinder.ServerManager;
            return server != null && server.Clients.TryGetValue(clientId, out var connection) ? connection : null;
        }
    }
}
