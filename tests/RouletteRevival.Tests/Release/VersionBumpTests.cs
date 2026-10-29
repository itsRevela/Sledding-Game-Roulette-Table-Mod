using System;
using FluentAssertions;
using ReleaseTool;
using Xunit;

namespace RouletteRevival.Tests.Release
{
    public sealed class VersionBumpTests
    {
        [Theory]
        [InlineData("feat: add roulette HUD", BumpLevel.Minor)]
        [InlineData("feat(hud): wire betting menu", BumpLevel.Minor)]
        [InlineData("fix: stop stale tile bets", BumpLevel.Patch)]
        [InlineData("perf(spawn): cache prefab lookup", BumpLevel.Patch)]
        [InlineData("feat!: drop MelonLoader 0.6 support", BumpLevel.Major)]
        [InlineData("fix(api)!: rename config keys", BumpLevel.Major)]
        [InlineData("feat: new keys\n\nBREAKING CHANGE: config section renamed", BumpLevel.Major)]
        [InlineData("docs: update README", BumpLevel.None)]
        [InlineData("chore: tidy", BumpLevel.None)]
        [InlineData("ci: add workflow", BumpLevel.None)]
        [InlineData("test: more cases", BumpLevel.None)]
        [InlineData("refactor: split spawner", BumpLevel.None)]
        [InlineData("chore(release): v1.2.0", BumpLevel.None)]
        [InlineData("Added a thing", BumpLevel.None)]
        [InlineData("feat:missing space", BumpLevel.None)]
        [InlineData("", BumpLevel.None)]
        public void LevelForCommit_FollowsConventionalCommits(string message, BumpLevel expected)
        {
            VersionBump.LevelForCommit(message).Should().Be(expected);
        }

        [Fact]
        public void LevelFor_TakesTheHighestLevel()
        {
            VersionBump.LevelFor(new[] { "docs: x", "fix: y", "feat: z", "chore: w" }).Should().Be(BumpLevel.Minor);
        }

        [Fact]
        public void LevelFor_NoCommits_IsNone()
        {
            VersionBump.LevelFor(Array.Empty<string>()).Should().Be(BumpLevel.None);
        }

        [Theory]
        [InlineData("major", BumpLevel.Major)]
        [InlineData(" Minor ", BumpLevel.Minor)]
        [InlineData("PATCH", BumpLevel.Patch)]
        public void ParseForced_KnownLevels(string text, BumpLevel expected)
        {
            VersionBump.ParseForced(text).Should().Be(expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void ParseForced_Empty_MeansDeriveFromCommits(string text)
        {
            VersionBump.ParseForced(text).Should().BeNull();
        }

        [Fact]
        public void ParseForced_Unknown_Throws()
        {
            Action act = () => VersionBump.ParseForced("huge");

            act.Should().Throw<FormatException>();
        }

        [Fact]
        public void SplitGitLog_SplitsOnRecordSeparatorAndDropsBlanks()
        {
            var log = "feat: a\n\nbody\n\u001e\nfix: b\n\u001e\n";

            VersionBump.SplitGitLog(log).Should().Equal("feat: a\n\nbody", "fix: b");
        }
    }
}
