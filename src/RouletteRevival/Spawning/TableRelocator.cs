using System;
using Il2Cpp;
using MelonLoader;
using RouletteRevival.Config;
using RouletteRevival.Placement;
using UnityEngine;

namespace RouletteRevival.Spawning
{
    /// <summary>Host hotkey action: move the table in front of the local player and remember the spot.</summary>
    public sealed class TableRelocator
    {
        private readonly RouletteTableSpawner _spawner;
        private readonly RouletteSettings _settings;

        public TableRelocator(RouletteTableSpawner spawner, RouletteSettings settings)
        {
            _spawner = spawner ?? throw new ArgumentNullException(nameof(spawner));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        private static MelonLogger.Instance Log => Melon<RouletteRevivalMod>.Logger;

        public void RelocateToLocalPlayer()
        {
            var player = PlayerControl.LocalPlayerInstance;
            if (player == null)
            {
                Log.Msg("Join or host a lobby before moving the roulette table.");
                return;
            }

            if (!RouletteTableSpawner.IsHosting)
            {
                Log.Msg("Only the lobby host can move the roulette table.");
                return;
            }

            var position = player.transform.position;
            var target = GroundSnapper.Snap(PlacementMath.InFrontOf(position.x, position.y, position.z, ViewYaw(player)));
            if (!target.IsValid)
            {
                Log.Warning($"Can't place the roulette table at {target}; move somewhere else on the map and try again.");
                return;
            }

            if (_spawner.Relocate(target))
            {
                _settings.SaveCustomPose(target);
                Log.Msg($"Roulette table moved to {target} and saved.");
            }
        }

        /// <summary>Heading the player is looking along: the main camera when present, otherwise the player body.</summary>
        private static float ViewYaw(PlayerControl player)
        {
            var camera = Camera.main;
            var forward = camera != null ? camera.transform.forward : player.transform.forward;
            var flat = new Vector3(forward.x, 0f, forward.z);
            return flat.sqrMagnitude < 0.0001f
                ? player.transform.eulerAngles.y
                : Quaternion.LookRotation(flat).eulerAngles.y;
        }
    }
}
