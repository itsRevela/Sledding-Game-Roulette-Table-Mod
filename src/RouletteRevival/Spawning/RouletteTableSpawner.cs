using Il2Cpp;
using Il2CppFishNet;
using Il2CppFishNet.Managing;
using Il2CppFishNet.Object;
using MelonLoader;
using RouletteRevival.Placement;
using UnityEngine;

namespace RouletteRevival.Spawning
{
    /// <summary>
    /// Server-side lifecycle of the roulette setup: spawns it once the host's player exists, and re-spawns it when moved.
    /// FishNet replicates the spawn to every client because the prefab is still in the game's spawnable list.
    /// </summary>
    public sealed class RouletteTableSpawner
    {
        /// <summary>Grace period after the host's player appears, so the scene's own spawns settle first.</summary>
        private const float SpawnDelaySeconds = 2f;

        private GameObject _spawned;
        private float _readySince = -1f;
        private bool _gaveUpThisSession;

        private static MelonLogger.Instance Log => Melon<RouletteRevivalMod>.Logger;

        public static bool IsHosting
        {
            get
            {
                var networkManager = InstanceFinder.NetworkManager;
                return networkManager != null && networkManager.IsServerStarted;
            }
        }

        public void Tick(TablePose configuredPose)
        {
            var networkManager = InstanceFinder.NetworkManager;
            if (networkManager == null || !networkManager.IsServerStarted)
            {
                ResetSession();
                return;
            }

            if (_gaveUpThisSession || _spawned != null)
            {
                return;
            }

            if (PlayerControl.LocalPlayerInstance == null)
            {
                _readySince = -1f;
                return;
            }

            if (_readySince < 0f)
            {
                _readySince = Time.unscaledTime;
                return;
            }

            if (Time.unscaledTime - _readySince < SpawnDelaySeconds)
            {
                return;
            }

            if (Object.FindObjectOfType<RouletteTable>() != null)
            {
                Log.Msg("A roulette table already exists in this world; not spawning another.");
                _gaveUpThisSession = true;
                return;
            }

            _spawned = SpawnAt(networkManager, GroundSnapper.Snap(configuredPose));
            _gaveUpThisSession = _spawned == null;
        }

        /// <summary>Moves the table by despawning and re-spawning it (the prefab has no NetworkTransform). Host only.</summary>
        public bool Relocate(TablePose pose)
        {
            var networkManager = InstanceFinder.NetworkManager;
            if (networkManager == null || !networkManager.IsServerStarted)
            {
                return false;
            }

            if (_spawned != null)
            {
                // Il2CppInterop can't marshal the C# default (null) for an Il2CppSystem.Nullable parameter; an empty
                // Nullable is FishNet's "no override", i.e. the object's own default despawn type.
                networkManager.ServerManager.Despawn(_spawned, new Il2CppSystem.Nullable<DespawnType>());
                _spawned = null;
            }

            _spawned = SpawnAt(networkManager, pose);
            return _spawned != null;
        }

        private static GameObject SpawnAt(NetworkManager networkManager, TablePose pose)
        {
            var prefab = RoulettePrefabLocator.Find(networkManager);
            if (prefab == null)
            {
                Log.Error($"Could not find the '{PrefabNames.RouletteSetup}' prefab in FishNet's spawnable prefabs. A game update may have removed it.");
                return null;
            }

            var rotation = Quaternion.Euler(0f, pose.YawDegrees, 0f);
            var instance = Object.Instantiate(prefab.gameObject, new Vector3(pose.X, pose.Y, pose.Z), rotation);
            networkManager.ServerManager.Spawn(instance);
            Log.Msg($"Roulette table spawned at {pose}.");
            return instance;
        }

        private void ResetSession()
        {
            _spawned = null;
            _readySince = -1f;
            _gaveUpThisSession = false;
        }
    }
}
