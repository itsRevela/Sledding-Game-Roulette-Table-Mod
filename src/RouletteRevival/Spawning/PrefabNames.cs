using System;

namespace RouletteRevival.Spawning
{
    /// <summary>Names of the game objects the mod looks for. Engine-free.</summary>
    public static class PrefabNames
    {
        /// <summary>Root of the roulette prefab in sharedassets1 (NetworkObject with table + wheel children).</summary>
        public const string RouletteSetup = "Roulette Setup";

        /// <summary>Unity appends "(Clone)" to instantiated copies; both forms identify the setup.</summary>
        public static bool IsRouletteSetup(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            const string cloneSuffix = "(Clone)";
            var trimmed = name.EndsWith(cloneSuffix, StringComparison.Ordinal)
                ? name.Substring(0, name.Length - cloneSuffix.Length)
                : name;
            return string.Equals(trimmed.Trim(), RouletteSetup, StringComparison.Ordinal);
        }
    }
}
