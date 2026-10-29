using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Il2Cpp;
using RouletteRevival.ChatBetting;
using GameBet = Il2Cpp.RouletteTable.RouletteBet;
using GameBetList = Il2CppSystem.Collections.Generic.List<Il2Cpp.RouletteTable.RouletteBet>;

namespace RouletteRevival.BetDisplay
{
    /// <summary>
    /// Host-side owner of the table's shared bet list (the chips every game draws). The game keeps one entry per player and
    /// draws only the last bet per spot, so two players on red showed one stack. Instead, collect board and chat bets here
    /// and publish a single entry holding the per-spot totals. The list is display-only: every game settles its own board
    /// bets from its local copy, and chat bets settle from the chat ledger.
    /// </summary>
    public static class TableBetDisplay
    {
        /// <summary>Key of the combined entry; real keys are player object ids, which are never negative.</summary>
        public const int CombinedKey = -1;

        private static ImmutableDictionary<int, IReadOnlyList<PlacedBet>> _board = ImmutableDictionary<int, IReadOnlyList<PlacedBet>>.Empty;
        private static ImmutableDictionary<int, IReadOnlyList<PlacedBet>> _chat = ImmutableDictionary<int, IReadOnlyList<PlacedBet>>.Empty;
        private static IntPtr _tablePointer;

        /// <summary>
        /// Called after the game stored a player's board bets under their own key: take over every per-player entry and
        /// republish the totals. <paramref name="senderId"/> covers a sender who removed their last bet, whose entry the
        /// game has already deleted again.
        /// </summary>
        public static void AbsorbBoardBets(RouletteTableNetworked table, int? senderId)
        {
            ForgetIfTableChanged(table);
            var synced = table.sync_PlayerBets;
            var playerIds = new List<int>();
            foreach (var entry in synced.Collection)
            {
                if (entry.Key != CombinedKey)
                {
                    playerIds.Add(entry.Key);
                }
            }

            if (senderId.HasValue && !playerIds.Contains(senderId.Value))
            {
                playerIds.Add(senderId.Value);
            }

            foreach (var playerId in playerIds)
            {
                var bets = synced.ContainsKey(playerId) ? FromGame(synced[playerId]) : Array.Empty<PlacedBet>();
                _board = bets.Count > 0 ? _board.SetItem(playerId, bets) : _board.Remove(playerId);
                if (synced.ContainsKey(playerId))
                {
                    synced.Remove(playerId);
                }
            }

            Publish(table);
        }

        public static void SetChatBets(RouletteTableNetworked table, int playerId, IReadOnlyList<PlacedBet> bets)
        {
            ForgetIfTableChanged(table);
            _chat = bets.Count > 0 ? _chat.SetItem(playerId, bets) : _chat.Remove(playerId);
            Publish(table);
        }

        /// <summary>The game clears the shared list when the wheel spins; forget ours too.</summary>
        public static void Clear()
        {
            _board = ImmutableDictionary<int, IReadOnlyList<PlacedBet>>.Empty;
            _chat = ImmutableDictionary<int, IReadOnlyList<PlacedBet>>.Empty;
        }

        private static void Publish(RouletteTableNetworked table)
        {
            var totals = BetAggregation.SumBySpot(_board.Values.Concat(_chat.Values));
            var synced = table.sync_PlayerBets;
            if (totals.Count == 0)
            {
                if (synced.ContainsKey(CombinedKey))
                {
                    synced.Remove(CombinedKey);
                }

                return;
            }

            var list = new GameBetList();
            foreach (var bet in totals)
            {
                list.Add(new GameBet((RouletteBetType)(int)bet.Spot.Kind, bet.Spot.Number, bet.Amount));
            }

            synced[CombinedKey] = list;
        }

        private static IReadOnlyList<PlacedBet> FromGame(GameBetList bets)
        {
            var result = new List<PlacedBet>();
            if (bets == null)
            {
                return result;
            }

            for (var i = 0; i < bets.Count; i++)
            {
                var bet = bets[i];
                result.Add(new PlacedBet(new BetSpot((BetKind)(int)bet.betType, bet.numberBetOn), bet.betAmount));
            }

            return result;
        }

        private static void ForgetIfTableChanged(RouletteTableNetworked table)
        {
            if (_tablePointer != table.Pointer)
            {
                _tablePointer = table.Pointer;
                Clear();
            }
        }
    }
}
