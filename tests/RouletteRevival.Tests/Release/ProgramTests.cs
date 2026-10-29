using System;
using System.IO;
using FluentAssertions;
using ReleaseTool;
using Xunit;

namespace RouletteRevival.Tests.Release
{
    public sealed class ProgramTests : IDisposable
    {
        private readonly string _dir = Directory.CreateTempSubdirectory("release-tool-tests").FullName;

        public void Dispose() => Directory.Delete(_dir, recursive: true);

        [Fact]
        public void ParseOptions_PairsNamesWithValues()
        {
            var options = Program.ParseOptions(new[] { "apply", "--version", "1.2.0", "--csproj", "a.csproj" });

            options.Should().ContainKey("version").WhoseValue.Should().Be("1.2.0");
            options.Should().ContainKey("csproj").WhoseValue.Should().Be("a.csproj");
        }

        [Fact]
        public void ParseOptions_EmptyOrMissingValue_MapsToEmpty()
        {
            var options = Program.ParseOptions(new[] { "next-version", "--force", "", "--csproj", "a", "--flag" });

            options["force"].Should().BeEmpty();
            options["csproj"].Should().Be("a");
            options["flag"].Should().BeEmpty();
        }

        [Theory]
        [InlineData("stray")]
        [InlineData("--")]
        public void ParseOptions_RejectsNonOptions(string bad)
        {
            Action act = () => Program.ParseOptions(new[] { "apply", bad });

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Main_Apply_UpdatesFilesAndWritesNotes()
        {
            var csproj = Write("p.csproj", "<Project><PropertyGroup><Version>1.1.0</Version></PropertyGroup></Project>");
            var changelog = Write("CHANGELOG.md", "# Changelog\n\n## Unreleased\n\nFixed it.\n\n## 1.1.0 - 2026-10-29\n");
            var commits = Write("commits.txt", "fix: it\n\u001e");
            var notes = Path.Combine(_dir, "notes.md");

            var exit = Program.Main(new[]
            {
                "apply", "--version", "1.1.1", "--csproj", csproj, "--changelog", changelog,
                "--commits-file", commits, "--date", "2026-10-30", "--notes-out", notes,
            });

            exit.Should().Be(0);
            File.ReadAllText(csproj).Should().Contain("<Version>1.1.1</Version>");
            File.ReadAllText(changelog).Should().StartWith("# Changelog\n\n## 1.1.1 - 2026-10-30\n");
            File.ReadAllText(notes).Should().Be("Fixed it.\n");
        }

        [Fact]
        public void Main_MissingRequiredOption_ReturnsError()
        {
            Program.Main(new[] { "apply", "--version", "1.0.0" }).Should().Be(1);
        }

        private string Write(string name, string content)
        {
            var path = Path.Combine(_dir, name);
            File.WriteAllText(path, content);
            return path;
        }
    }
}
