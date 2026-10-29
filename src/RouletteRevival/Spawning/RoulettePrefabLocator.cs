using Il2CppFishNet.Managing;
using Il2CppFishNet.Object;

namespace RouletteRevival.Spawning
{
    /// <summary>Finds the shipped-but-unused "Roulette Setup" prefab in FishNet's spawnable prefab list.</summary>
    public static class RoulettePrefabLocator
    {
        public static NetworkObject Find(NetworkManager networkManager)
        {
            var prefabs = networkManager.SpawnablePrefabs;
            if (prefabs == null)
            {
                return null;
            }

            var count = prefabs.GetObjectCount();
            for (var id = 0; id < count; id++)
            {
                var prefab = prefabs.GetObject(true, id);
                if (prefab != null && PrefabNames.IsRouletteSetup(prefab.gameObject.name))
                {
                    return prefab;
                }
            }

            return null;
        }
    }
}
