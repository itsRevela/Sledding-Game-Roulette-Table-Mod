using RouletteRevival.Placement;
using UnityEngine;

namespace RouletteRevival.Spawning
{
    /// <summary>Drops a pose onto the ground below it so the table never floats or sinks after relocation.</summary>
    public static class GroundSnapper
    {
        private const float CastHeightAbove = 3f;
        private const float CastDistance = 8f;

        public static TablePose Snap(TablePose pose)
        {
            var origin = new Vector3(pose.X, pose.Y + CastHeightAbove, pose.Z);
            return Physics.Raycast(origin, Vector3.down, out var hit, CastDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
                ? pose.WithY(hit.point.y)
                : pose;
        }
    }
}
