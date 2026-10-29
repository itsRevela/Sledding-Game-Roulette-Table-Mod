using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ReleaseTool
{
    public enum BumpLevel
    {
        None = 0,
        Patch = 1,
        Minor = 2,
        Major = 3,
    }

    /// <summary>Conventional-commit rules: breaking → major, feat → minor, fix/perf → patch, anything else → no release.</summary>
    public static class VersionBump
    {
        public const string ReleaseCommitPrefix = "chore(release):";

        private static readonly Regex Header = new Regex(@"^(?<type>[a-z]+)(\([^)]*\))?(?<breaking>!)?:\s", RegexOptions.CultureInvariant);

        public static BumpLevel LevelFor(IEnumerable<string> commitMessages)
        {
            if (commitMessages == null)
            {
                throw new ArgumentNullException(nameof(commitMessages));
            }

            return commitMessages.Select(LevelForCommit).DefaultIfEmpty(BumpLevel.None).Max();
        }

        public static BumpLevel LevelForCommit(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return BumpLevel.None;
            }

            var trimmed = message.Trim();
            if (trimmed.StartsWith(ReleaseCommitPrefix, StringComparison.Ordinal))
            {
                return BumpLevel.None;
            }

            var header = Header.Match(trimmed);
            if (!header.Success)
            {
                return BumpLevel.None;
            }

            if (header.Groups["breaking"].Success || Regex.IsMatch(trimmed, @"^BREAKING[ -]CHANGE:", RegexOptions.Multiline))
            {
                return BumpLevel.Major;
            }

            return header.Groups["type"].Value switch
            {
                "feat" => BumpLevel.Minor,
                "fix" => BumpLevel.Patch,
                "perf" => BumpLevel.Patch,
                _ => BumpLevel.None,
            };
        }

        /// <summary>Parses "major"/"minor"/"patch" (case-insensitive); empty or null means "derive from commits".</summary>
        public static BumpLevel? ParseForced(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return text.Trim().ToLowerInvariant() switch
            {
                "major" => BumpLevel.Major,
                "minor" => BumpLevel.Minor,
                "patch" => BumpLevel.Patch,
                _ => throw new FormatException($"Unknown bump '{text}'. Use major, minor or patch."),
            };
        }

        /// <summary>Splits <c>git log --format=%B%x1e</c> output into individual commit messages.</summary>
        public static IReadOnlyList<string> SplitGitLog(string log) =>
            (log ?? string.Empty)
                .Split('\u001e')
                .Select(m => m.Trim())
                .Where(m => m.Length > 0)
                .ToList();
    }
}
