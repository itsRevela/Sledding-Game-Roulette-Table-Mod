using System;

namespace RouletteRevival.Placement
{
    /// <summary>World position (metres) and yaw (degrees) of the roulette setup root. Engine-free so it can be unit tested.</summary>
    public readonly struct TablePose : IEquatable<TablePose>
    {
        /// <summary>Largest coordinate magnitude accepted from config; the map fits well inside this.</summary>
        public const float MaxCoordinate = 10000f;

        public TablePose(float x, float y, float z, float yawDegrees)
        {
            X = x;
            Y = y;
            Z = z;
            YawDegrees = yawDegrees;
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public float YawDegrees { get; }

        public bool IsValid =>
            IsUsable(X) && IsUsable(Y) && IsUsable(Z) && !float.IsNaN(YawDegrees) && !float.IsInfinity(YawDegrees);

        public TablePose WithY(float y) => new TablePose(X, y, Z, YawDegrees);

        public bool Equals(TablePose other) =>
            X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && YawDegrees.Equals(other.YawDegrees);

        public override bool Equals(object obj) => obj is TablePose other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(X, Y, Z, YawDegrees);

        public override string ToString() => $"({X:0.0}, {Y:0.0}, {Z:0.0}) yaw {YawDegrees:0}";

        private static bool IsUsable(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value) && Math.Abs(value) <= MaxCoordinate;
    }
}
