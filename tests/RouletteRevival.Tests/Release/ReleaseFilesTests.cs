using System;
using FluentAssertions;
using ReleaseTool;
using Xunit;

namespace RouletteRevival.Tests.Release
{
    public sealed class ReleaseFilesTests
    {
        private const string Csproj = "<Project>\n  <PropertyGroup>\n    <Version>1.0.3</Version>\n  </PropertyGroup>\n</Project>";

        [Fact]
        public void SetCsprojVersion_ReplacesOnlyTheVersion()
        {
            var updated = ReleaseFiles.SetCsprojVersion(Csproj, new SemVer(1, 1, 0));

            updated.Should().Be(Csproj.Replace("1.0.3", "1.1.0"));
        }

        [Fact]
        public void SetCsprojVersion_WithoutVersionElement_Throws()
        {
            Action act = () => ReleaseFiles.SetCsprojVersion("<Project />", new SemVer(1, 0, 0));

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetCsprojVersion_ReadsTheValue()
        {
            ReleaseFiles.GetCsprojVersion(Csproj).Should().Be("1.0.3");
        }

        [Fact]
        public void ReleaseChangelog_RenamesUnreleasedAndReturnsItsBody()
        {
            var changelog = "# Changelog\n\n## Unreleased\n\n**What:** New thing.\n\n## 1.0.3 - 2026-10-29\n\nOld.\n";

            var release = ReleaseFiles.ReleaseChangelog(changelog, new SemVer(1, 1, 0), "2026-11-01", new[] { "feat: x" });

            release.Changelog.Should().Be("# Changelog\n\n## 1.1.0 - 2026-11-01\n\n**What:** New thing.\n\n## 1.0.3 - 2026-10-29\n\nOld.\n");
            release.Notes.Should().Be("**What:** New thing.\n");
        }

        [Fact]
        public void ReleaseChangelog_WithoutUnreleased_InsertsCommitSummaryAboveLatest()
        {
            var changelog = "# Changelog\n\n## 1.0.3 - 2026-10-29\n\nOld.\n";

            var release = ReleaseFiles.ReleaseChangelog(changelog, new SemVer(1, 0, 4), "2026-11-01",
                new[] { "fix: stale bets\n\nlong body", "chore(release): v1.0.3" });

            release.Changelog.Should().Be("# Changelog\n\n## 1.0.4 - 2026-11-01\n\n- fix: stale bets\n\n## 1.0.3 - 2026-10-29\n\nOld.\n");
            release.Notes.Should().Be("- fix: stale bets\n");
        }

        [Fact]
        public void ReleaseChangelog_NormalizesWindowsLineEndings()
        {
            var release = ReleaseFiles.ReleaseChangelog("# Changelog\r\n\r\n## Unreleased\r\n\r\nX\r\n", new SemVer(2, 0, 0), "2026-11-01", Array.Empty<string>());

            release.Changelog.Should().Be("# Changelog\n\n## 2.0.0 - 2026-11-01\n\nX\n");
        }

        [Fact]
        public void CommitSummary_NoReleasableCommits_SaysMaintenance()
        {
            ReleaseFiles.CommitSummary(new[] { "chore(release): v1.0.0" }).Should().Be("- Maintenance release.\n");
        }
    }
}
