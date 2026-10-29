using Il2Cpp;
using Il2CppFishNet.Connection;
using UnityEngine;

namespace RouletteRevival.ChatBetting
{
    /// <summary>
    /// Moves points on a player's own save from the host, using target RPCs the game already ships, so it works for
    /// players without the mod. Server-only.
    /// </summary>
    public static class RemotePoints
    {
        /// <summary>Placement 1 makes the client show the credit as a "First" combo (ignoring its sled multiplier).</summary>
        private const int PayoutPlacement = 1;

        /// <summary>A spin value of 0 skips the Big Wheel's ticket reward, leaving only the spin-cost deduction.</summary>
        private const float NoTicketReward = 0f;

        /// <summary>Credits <paramref name="points"/> through the hide-and-seek award RPC on the player's own object.</summary>
        public static bool TryPay(NetworkConnection player, int points, out string error)
        {
            var hideSeek = FindHideSeekController(player);
            if (hideSeek == null)
            {
                error = "the player's object has no PlayerHideSeekController";
                return false;
            }

            hideSeek.Target_AddPoints(player, PayoutPlacement, points);
            error = null;
            return true;
        }

        /// <summary>Credits any amount, split into payments the RPC's 32-bit points field can carry.</summary>
        public static bool TryPayTotal(NetworkConnection player, long points, out string error)
        {
            error = null;
            var remaining = points;
            while (remaining > 0)
            {
                var chunk = (int)System.Math.Min(remaining, int.MaxValue);
                if (!TryPay(player, chunk, out error))
                {
                    return false;
                }

                remaining -= chunk;
            }

            return true;
        }

        /// <summary>
        /// Charges <paramref name="times"/> Big Wheel spin costs (5,000 each in the shipped scene) through the arcade wheel's
        /// spend RPC, looking the wheel up once. The client silently skips any charge it can't afford.
        /// </summary>
        public static bool TryChargeSpinCosts(NetworkConnection player, long times, out string error)
        {
            var wheel = Object.FindObjectOfType<ArcadeWheelSpinInteractable>();
            if (wheel == null)
            {
                error = "no arcade Big Wheel in this scene";
                return false;
            }

            for (var i = 0L; i < times; i++)
            {
                wheel.Target_SpendPoints(player, NoTicketReward);
            }

            error = null;
            return true;
        }

        private static PlayerHideSeekController FindHideSeekController(NetworkConnection player)
        {
            var first = player.FirstObject;
            if (first != null)
            {
                var onPlayer = first.GetComponentInChildren<PlayerHideSeekController>(true);
                if (onPlayer != null)
                {
                    return onPlayer;
                }
            }

            foreach (var candidate in Object.FindObjectsOfType<PlayerHideSeekController>())
            {
                if (candidate.Owner != null && candidate.Owner.ClientId == player.ClientId)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
