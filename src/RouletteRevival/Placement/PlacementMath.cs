using System;

namespace RouletteRevival.Placement
{
    /// <summary>Engine-free placement rules for the roulette setup.</summary>
    public static class PlacementMath
    {
        /// <summary>
        /// Default spot: inside the lodge arcade, by the VIP fence and the big prize wheel. Picked in-game by the host
        /// with the relocate hotkey during the 2026-10-29 playtest.
        /// </summary>
        public static readonly TablePose DefaultPose = new TablePose(2.32f, 31.39f, 125.85f, 327.56f);

        /// <summary>How far in front of the player a relocated table is placed.</summary>
        public const float RelocateDistance = 2.5f;

        /// <summary>
        /// The prefab's wheel sits 3 m along the root's local -X axis; turning the root 90° from the player's
        /// heading lays table and wheel side by side across the player's view.
        /// </summary>
        public const float SideBySideYawOffset = 90f;

        private const float DegToRad = (float)(Math.PI / 180.0);

        /// <summary>Pose for a table placed <paramref name="distance"/> metres ahead of a player standing at (x, y, z) facing <paramref name="playerYawDegrees"/>.</summary>
        public static TablePose InFrontOf(float x, float y, float z, float playerYawDegrees, float distance = RelocateDistance)
        {
            if (distance < 0f || float.IsNaN(distance))
            {
                throw new ArgumentOutOfRangeException(nameof(distance), distance, "Distance must be zero or positive.");
            }

            var radians = playerYawDegrees * DegToRad;
            var targetX = x + (float)Math.Sin(radians) * distance;
            var targetZ = z + (float)Math.Cos(radians) * distance;
            return new TablePose(targetX, y, targetZ, NormalizeYaw(playerYawDegrees + SideBySideYawOffset));
        }

        /// <summary>Wraps any angle into [0, 360).</summary>
        public static float NormalizeYaw(float yawDegrees)
        {
            if (float.IsNaN(yawDegrees) || float.IsInfinity(yawDegrees))
            {
                return 0f;
            }

            var wrapped = yawDegrees % 360f;
            return wrapped < 0f ? wrapped + 360f : wrapped;
        }

        /// <summary>Returns the configured pose when it is usable, otherwise the built-in default.</summary>
        public static TablePose ResolveConfigured(bool useCustom, TablePose custom) =>
            useCustom && custom.IsValid ? custom : DefaultPose;
    }
}
