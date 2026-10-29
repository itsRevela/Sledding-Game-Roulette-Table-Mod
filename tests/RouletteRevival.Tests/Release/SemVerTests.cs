using System;
using FluentAssertions;
using ReleaseTool;
using Xunit;

namespace RouletteRevival.Tests.Release
{
    public sealed class SemVerTests
    {
        [Theory]
        [InlineData("1.0.3", 1, 0, 3)]
        [InlineData("v2.10.0", 2, 10, 0)]
        [InlineData(" 0.0.1 ", 0, 0, 1)]
        public void Parse_ValidVersions(string text, int major, int minor, int patch)
        {
            SemVer.Parse(text).Should().Be(new SemVer(major, minor, patch));
        }

        [Theory]
        [InlineData("1.0")]
        [InlineData("1.0.0.0")]
        [InlineData("1.0.x")]
        [InlineData("1.0.-1")]
        [InlineData("1.0.0-beta")]
        [InlineData("")]
        public void Parse_InvalidVersions_Throws(string text)
        {
            Action act = () => SemVer.Parse(text);

            act.Should().Throw<FormatException>();
        }

        [Theory]
        [InlineData(BumpLevel.Patch, "1.2.4")]
        [InlineData(BumpLevel.Minor, "1.3.0")]
        [InlineData(BumpLevel.Major, "2.0.0")]
        [InlineData(BumpLevel.None, "1.2.3")]
        public void Bump_ResetsLowerComponents(BumpLevel level, string expected)
        {
            new SemVer(1, 2, 3).Bump(level).ToString().Should().Be(expected);
        }
    }
}
