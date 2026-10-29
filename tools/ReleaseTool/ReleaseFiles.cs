using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ReleaseTool
{
    /// <summary>The CHANGELOG text after release, plus the notes to publish for that version.</summary>
    public sealed record ChangelogRelease(string Changelog, string Notes);

    /// <summary>Pure text transforms applied to repository files when cutting a release.</summary>
    public static class ReleaseFiles
    {
        public const string UnreleasedHeading = "## Unreleased";

        private static readonly Regex VersionElement = new Regex(@"<Version>[^<]*</Version>", RegexOptions.CultureInvariant);

        public static string SetCsprojVersion(string csproj, SemVer version)
        {
            if (csproj == null)
            {
                throw new ArgumentNullException(nameof(csproj));
            }

            if (VersionElement.Matches(csproj).Count != 1)
            {
                throw new InvalidOperationException("Expected exactly one <Version> element in the project file.");
            }

            return VersionElement.Replace(csproj, $"<Version>{version}</Version>");
        }

        public static string GetCsprojVersion(string csproj)
        {
            var match = VersionElement.Match(csproj ?? string.Empty);
            if (!match.Success)
            {
                throw new InvalidOperationException("No <Version> element in the project file.");
            }

            return match.Value.Substring("<Version>".Length, match.Value.Length - "<Version></Version>".Length);
        }

        /// <summary>
        /// Turns the "## Unreleased" section into "## X.Y.Z - date". Without one, a section listing the commit subjects
        /// is inserted so every release still has an entry.
        /// </summary>
        public static ChangelogRelease ReleaseChangelog(string changelog, SemVer version, string date, IEnumerable<string> commitMessages)
        {
            if (changelog == null)
            {
                throw new ArgumentNullException(nameof(changelog));
            }

            var heading = $"## {version} - {date}";
            var lines = changelog.Replace("\r\n", "\n").Split('\n').ToList();
            var unreleased = lines.FindIndex(l => l.Trim().Equals(UnreleasedHeading, StringComparison.OrdinalIgnoreCase));
            if (unreleased >= 0)
            {
                lines[unreleased] = heading;
                var end = lines.FindIndex(unreleased + 1, l => l.StartsWith("## ", StringComparison.Ordinal));
                var body = lines.Skip(unreleased + 1).Take((end < 0 ? lines.Count : end) - unreleased - 1);
                return new ChangelogRelease(string.Join("\n", lines), string.Join("\n", body).Trim() + "\n");
            }

            var notes = CommitSummary(commitMessages);
            var insertAt = lines.FindIndex(l => l.StartsWith("## ", StringComparison.Ordinal));
            var section = new List<string> { heading, string.Empty, notes.TrimEnd(), string.Empty };
            lines.InsertRange(insertAt < 0 ? lines.Count : insertAt, section);
            return new ChangelogRelease(string.Join("\n", lines), notes);
        }

        /// <summary>Bullet list of releasable commit subjects (release commits and non-conventional noise excluded).</summary>
        public static string CommitSummary(IEnumerable<string> commitMessages)
        {
            var subjects = (commitMessages ?? Enumerable.Empty<string>())
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Select(m => m.Trim().Split('\n')[0].Trim())
                .Where(s => !s.StartsWith(VersionBump.ReleaseCommitPrefix, StringComparison.Ordinal))
                .ToList();

            if (subjects.Count == 0)
            {
                return "- Maintenance release.\n";
            }

            var builder = new StringBuilder();
            foreach (var subject in subjects)
            {
                builder.Append("- ").Append(subject).Append('\n');
            }

            return builder.ToString();
        }
    }
}
