using System;
using System.Globalization;

namespace ReleaseTool
{
    /// <summary>Plain MAJOR.MINOR.PATCH version (no pre-release or build metadata).</summary>
    public readonly struct SemVer : IEquatable<SemVer>
    {
        public SemVer(int major, int minor, int patch)
        {
            if (major < 0 || minor < 0 || patch < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(major), "Version components must be non-negative.");
            }

            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }

        public static SemVer Parse(string text)
        {
            var trimmed = (text ?? throw new ArgumentNullException(nameof(text))).Trim();
            if (trimmed.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed.Substring(1);
            }

            var parts = trimmed.Split('.');
            if (parts.Length != 3
                || !TryComponent(parts[0], out var major)
                || !TryComponent(parts[1], out var minor)
                || !TryComponent(parts[2], out var patch))
            {
                throw new FormatException($"'{text}' is not a MAJOR.MINOR.PATCH version.");
            }

            return new SemVer(major, minor, patch);
        }

        public SemVer Bump(BumpLevel level) => level switch
        {
            BumpLevel.Major => new SemVer(Major + 1, 0, 0),
            BumpLevel.Minor => new SemVer(Major, Minor + 1, 0),
            BumpLevel.Patch => new SemVer(Major, Minor, Patch + 1),
            _ => this,
        };

        public bool Equals(SemVer other) => Major == other.Major && Minor == other.Minor && Patch == other.Patch;

        public override bool Equals(object obj) => obj is SemVer other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch);

        public override string ToString() => $"{Major}.{Minor}.{Patch}";

        private static bool TryComponent(string text, out int value) =>
            int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }
}
