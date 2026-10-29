using Il2CppFishNet.Connection;
using MelonLoader;
using RouletteRevival.Spawning;

namespace RouletteRevival.ChatBetting
{
    /// <summary>Executes the !rtest diagnostics: pay or charge the player who typed the command. Host only.</summary>
    public static class PointTestCommands
    {
        private static MelonLogger.Instance Log => Melon<RouletteRevivalMod>.Logger;

        public static void Execute(NetworkConnection sender, ChatCommand command)
        {
            if (!RouletteTableSpawner.IsHosting)
            {
                return;
            }

            var who = $"client {sender.ClientId}";
            switch (command.Kind)
            {
                case ChatCommandKind.TestPay:
                    if (RemotePoints.TryPay(sender, command.Amount, out var payError))
                    {
                        Log.Msg($"[rtest] Sent +{command.Amount} points to {who}.");
                    }
                    else
                    {
                        Log.Warning($"[rtest] Could not pay {who}: {payError}.");
                    }

                    break;

                case ChatCommandKind.TestCharge:
                    for (var i = 0; i < command.Amount; i++)
                    {
                        if (!RemotePoints.TryChargeSpinCost(sender, out var cost, out var chargeError))
                        {
                            Log.Warning($"[rtest] Could not charge {who}: {chargeError}.");
                            return;
                        }

                        Log.Msg($"[rtest] Sent charge {i + 1}/{command.Amount} of {cost} points to {who}.");
                    }

                    break;
            }
        }
    }
}
