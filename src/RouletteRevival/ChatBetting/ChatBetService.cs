using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Il2Cpp;
using Il2CppFishNet.Connection;
using MelonLoader;
using RouletteRevival.BetDisplay;
using UnityEngine;

namespace RouletteRevival.ChatBetting
{
    /// <summary>
    /// Host-side chat betting for players without the mod: records bets, shows them as chips on the real table, and
    /// settles them through the game's own point RPCs when the real wheel lands.
    /// </summary>
    public sealed class ChatBetService
    {
        /// <summary>Settle a moment after the wheel stops so the result is visible first.</summary>
        private const float SettleDelayAfterSpin = 0.5f;

        private readonly BetLedger _ledger = new BetLedger();
        private ImmutableDictionary<int, Bettor> _bettors = ImmutableDictionary<int, Bettor>.Empty;
        private IntPtr _tablePointer;
        private PendingSpin _pending;

        public static ChatBetService Instance { get; } = new ChatBetService();

        private static MelonLogger.Instance Log => Melon<RouletteRevivalMod>.Logger;

        public void Handle(NetworkConnection connection, ChatCommand command, long maxPerSpin)
        {
            if (command.Kind == ChatCommandKind.Help)
            {
                foreach (var line in ChatHelp.Page((int)command.Amount, TableBridge.ChargeUnit(), maxPerSpin))
                {
                    ChatOutput.ToPlayer(connection, line);
                }

                return;
            }

            var bettor = Bettors.FromConnection(connection);
            var table = TableBridge.FindTable();
            if (bettor == null || table == null)
            {
                Log.Warning($"[bet] Ignored {command}: {(bettor == null ? "sender has no player object" : "no roulette table")}.");
                if (table == null)
                {
                    ChatOutput.ToPlayer(connection, "There's no roulette table right now.");
                }

                return;
            }

            ResetIfTableChanged(table);
            switch (command.Kind)
            {
                case ChatCommandKind.Bet:
                    PlaceBet(connection, table, bettor, command, maxPerSpin);
                    break;
                case ChatCommandKind.Clear:
                    ClearBets(connection, table, bettor);
                    break;
                case ChatCommandKind.ListBets:
                    ChatOutput.ToPlayer(connection, $"Your bets: {Describe(_ledger.BetsOf(bettor.PlayerId))}");
                    break;
                case ChatCommandKind.Spin:
                    SpinWheel(connection, table, bettor);
                    break;
            }
        }

        /// <summary>Called on the host when the wheel's result is broadcast; locks in this spin's chat bets.</summary>
        public void OnSpin(int result, float spinTime)
        {
            if (_ledger.IsEmpty)
            {
                return;
            }

            _pending = new PendingSpin(result, Time.unscaledTime + spinTime + SettleDelayAfterSpin, _ledger.TakeAll(), _bettors);
            Log.Msg($"[bet] Spin locked in {_pending.Bets.Count} chat bettor(s); result {result} settles in {spinTime:0.0}s.");
        }

        public void Tick()
        {
            if (_pending == null || Time.unscaledTime < _pending.DueTime)
            {
                return;
            }

            var spin = _pending;
            _pending = null;

            // If the host moved the table mid-spin the game refunds board bets; void chat bets the same way.
            var table = TableBridge.FindTable();
            if (table == null || table.Pointer != _tablePointer)
            {
                ChatOutput.ToEveryone("The table moved during the spin, so chat bets were voided (nothing was charged).");
                return;
            }

            SettleSpin(spin);
        }

        private void PlaceBet(NetworkConnection connection, RouletteTableNetworked table, Bettor bettor, ChatCommand command, long maxPerSpin)
        {
            if (_pending != null || TableBridge.IsSpinning())
            {
                ChatOutput.ToPlayer(connection, "Bets are closed while the wheel spins.");
                return;
            }

            var unit = TableBridge.ChargeUnit();
            var rejection = _ledger.TryAdd(bettor.PlayerId, command.Spot, command.Amount, unit, maxPerSpin);
            if (rejection != BetRejection.None)
            {
                ChatOutput.ToPlayer(connection, RejectionText(rejection, unit, maxPerSpin));
                return;
            }

            _bettors = _bettors.SetItem(bettor.PlayerId, bettor);
            TableBetDisplay.SetChatBets(table, bettor.PlayerId, _ledger.BetsOf(bettor.PlayerId));
            ChatOutput.ToEveryone($"{bettor.Name} bet {command.Amount:N0} on {command.Spot} (total {_ledger.TotalOf(bettor.PlayerId):N0}).");
        }

        /// <summary>
        /// Spins on the server exactly as the wheel's own F interaction does. Players without the mod need this: the table
        /// lock also stops them pressing F on the wheel.
        /// </summary>
        private void SpinWheel(NetworkConnection connection, RouletteTableNetworked table, Bettor bettor)
        {
            if (_pending != null || TableBridge.IsSpinning())
            {
                ChatOutput.ToPlayer(connection, "The wheel is already spinning.");
                return;
            }

            if (table.sync_PlayerBets.Count == 0)
            {
                ChatOutput.ToPlayer(connection, "Place a bet first (!help for how).");
                return;
            }

            var spinner = UnityEngine.Object.FindObjectOfType<RouletteSpinner>();
            if (spinner == null)
            {
                Log.Warning("[bet] !spin ignored: no roulette wheel.");
                return;
            }

            ChatOutput.ToEveryone($"{bettor.Name} spins the wheel!");
            spinner.RpcLogic___Cmd_SpinRoulette___2166136261();
        }

        private void ClearBets(NetworkConnection connection, RouletteTableNetworked table, Bettor bettor)
        {
            if (_pending != null || TableBridge.IsSpinning())
            {
                ChatOutput.ToPlayer(connection, "Too late, the wheel is spinning.");
                return;
            }

            var total = _ledger.Clear(bettor.PlayerId);
            TableBetDisplay.SetChatBets(table, bettor.PlayerId, Array.Empty<PlacedBet>());
            if (total > 0)
            {
                ChatOutput.ToEveryone($"{bettor.Name} took back {total:N0}.");
            }
            else
            {
                ChatOutput.ToPlayer(connection, "You have no bets.");
            }
        }

        private static void SettleSpin(PendingSpin spin)
        {
            var unit = TableBridge.ChargeUnit();
            var lines = new List<string>();
            foreach (var entry in spin.Bets)
            {
                var bettor = spin.Bettors.TryGetValue(entry.Key, out var known) ? known : null;
                var result = Settlement.Settle(entry.Value, bet => TableBridge.ReturnFor(bet, spin.Result), unit);
                var connection = bettor != null ? Bettors.ConnectionOf(bettor.ClientId) : null;
                if (connection == null)
                {
                    Log.Warning($"[bet] Player {entry.Key} left before settling; net {result.Net:N0} not applied.");
                    continue;
                }

                ApplyResult(connection, result);
                lines.Add($"{bettor.Name} {(result.Net >= 0 ? "+" : "-")}{Math.Abs(result.Net):N0}");
            }

            var color = spin.Result == 0 ? "green" : RouletteTable.IsRed(spin.Result) ? "red" : "black";
            ChatOutput.ToEveryone($"{spin.Result} {color}! {string.Join(", ", lines)}");
        }

        private static void ApplyResult(NetworkConnection connection, SettlementResult result)
        {
            if (result.PayAmount > 0 && !RemotePoints.TryPayTotal(connection, result.PayAmount, out var payError))
            {
                Log.Warning($"[bet] Payout failed: {payError}.");
            }

            if (result.ChargeCount > 0 && !RemotePoints.TryChargeSpinCosts(connection, result.ChargeCount, out var chargeError))
            {
                Log.Warning($"[bet] Charge failed: {chargeError}.");
            }
        }

        private void ResetIfTableChanged(RouletteTableNetworked table)
        {
            if (_tablePointer == table.Pointer)
            {
                return;
            }

            var hadBets = !_ledger.IsEmpty;
            _tablePointer = table.Pointer;
            _ledger.Reset();
            _bettors = ImmutableDictionary<int, Bettor>.Empty;
            _pending = null;
            if (hadBets)
            {
                // Nothing is charged before a spin settles, so dropped bets cost nobody anything.
                ChatOutput.ToEveryone("The table moved, so chat bets were cleared (nothing was charged).");
            }
        }

        private static string Describe(IReadOnlyList<PlacedBet> bets) =>
            bets.Count == 0 ? "no bets" : string.Join(", ", bets.Select(b => $"{b.Amount:N0} on {b.Spot}"));

        private static string RejectionText(BetRejection rejection, long unit, long maxPerSpin) => rejection switch
        {
            BetRejection.NotMultipleOfUnit => $"bets must be multiples of {unit:N0}.",
            BetRejection.OverSpinLimit => $"the limit is {maxPerSpin:N0} per spin.",
            _ => "that bet isn't valid.",
        };

        private sealed class PendingSpin
        {
            public PendingSpin(int result, float dueTime, IReadOnlyDictionary<int, IReadOnlyList<PlacedBet>> bets, ImmutableDictionary<int, Bettor> bettors)
            {
                Result = result;
                DueTime = dueTime;
                Bets = bets;
                Bettors = bettors;
            }

            public int Result { get; }
            public float DueTime { get; }
            public IReadOnlyDictionary<int, IReadOnlyList<PlacedBet>> Bets { get; }
            public ImmutableDictionary<int, Bettor> Bettors { get; }
        }
    }
}
