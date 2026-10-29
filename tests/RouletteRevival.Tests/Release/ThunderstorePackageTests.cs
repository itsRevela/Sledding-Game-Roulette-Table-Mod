using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using FluentAssertions;
using ReleaseTool;
using Xunit;

namespace RouletteRevival.Tests.Release
{
    public sealed class ThunderstorePackageTests
    {
        private const string Template = """
            {
              "name": "RouletteRevival",
              "website_url": "https://example.com",
              "description": "A table.",
              "dependencies": ["LavaGang-MelonLoader-0.7.3"]
            }
            """;

        private static readonly SemVer Version = new SemVer(1, 2, 2);

        [Fact]
        public void BuildManifest_AddsTheVersionAndKeepsTheFields()
        {
            var manifest = JsonNode.Parse(ThunderstorePackage.BuildManifest(Template, Version));

            manifest["name"].GetValue<string>().Should().Be("RouletteRevival");
            manifest["version_number"].GetValue<string>().Should().Be("1.2.2");
            manifest["website_url"].GetValue<string>().Should().Be("https://example.com");
            manifest["description"].GetValue<string>().Should().Be("A table.");
            manifest["dependencies"].AsArray().Select(d => d.GetValue<string>()).Should().Equal("LavaGang-MelonLoader-0.7.3");
        }

        [Theory]
        [InlineData("Roulette Revival")]
        [InlineData("Roulette-Revival")]
        public void BuildManifest_RejectsNamesThunderstoreRefuses(string name)
        {
            var template = Template.Replace("\"RouletteRevival\"", $"\"{name}\"");

            Action act = () => ThunderstorePackage.BuildManifest(template, Version);

            act.Should().Throw<FormatException>().WithMessage("*letters, digits and underscores*");
        }

        [Fact]
        public void BuildManifest_RejectsALongDescription()
        {
            var template = Template.Replace("A table.", new string('x', ThunderstorePackage.MaxDescriptionLength + 1));

            Action act = () => ThunderstorePackage.BuildManifest(template, Version);

            act.Should().Throw<FormatException>().WithMessage("*250*");
        }

        [Theory]
        [InlineData("website_url")]
        [InlineData("dependencies")]
        public void BuildManifest_RejectsMissingFields(string field)
        {
            var template = JsonNode.Parse(Template).AsObject();
            template.Remove(field);

            Action act = () => ThunderstorePackage.BuildManifest(template.ToJsonString(), Version);

            act.Should().Throw<FormatException>().WithMessage($"*{field}*");
        }

        [Fact]
        public void BuildManifest_RejectsInvalidJson()
        {
            Action act = () => ThunderstorePackage.BuildManifest("{ \"name\": ", Version);

            act.Should().Throw<FormatException>().WithMessage("*valid JSON*");
        }

        [Fact]
        public void EnsureChangelogCovers_AcceptsAVersionSection()
        {
            Action act = () => ThunderstorePackage.EnsureChangelogCovers("# Changelog\n\n## 1.2.2\n\n- Fixed.\n", Version);

            act.Should().NotThrow();
        }

        [Theory]
        [InlineData("# Changelog\n\n## 1.2.1\n\n- Fixed.\n")]
        [InlineData("# Changelog\n\n## 1.2.22\n\n- Fixed.\n")]
        [InlineData("# Changelog\n\nFixed in 1.2.2.\n")]
        public void EnsureChangelogCovers_RejectsAMissingSection(string changelog)
        {
            Action act = () => ThunderstorePackage.EnsureChangelogCovers(changelog, Version);

            act.Should().Throw<FormatException>().WithMessage("*## 1.2.2*");
        }

        [Fact]
        public void ValidateIcon_AcceptsA256Png()
        {
            Action act = () => ThunderstorePackage.ValidateIcon(Png(256, 256));

            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(512, 512)]
        [InlineData(256, 128)]
        public void ValidateIcon_RejectsOtherSizes(int width, int height)
        {
            Action act = () => ThunderstorePackage.ValidateIcon(Png(width, height));

            act.Should().Throw<FormatException>().WithMessage("*256*");
        }

        [Fact]
        public void ValidateIcon_RejectsNonPng()
        {
            Action act = () => ThunderstorePackage.ValidateIcon(Encoding.ASCII.GetBytes("GIF89a-not-a-png-at-all!"));

            act.Should().Throw<FormatException>().WithMessage("*PNG*");
        }

        [Fact]
        public void WriteZip_LaysOutThePackageForModManagers()
        {
            using var stream = new MemoryStream();

            ThunderstorePackage.WriteZip(stream, "{}", Png(256, 256), "readme", "changes", "RouletteRevival.dll", new byte[] { 1, 2, 3 });

            stream.Position = 0;
            using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
            zip.Entries.Select(e => e.FullName).Should().BeEquivalentTo(
                "manifest.json", "icon.png", "README.md", "CHANGELOG.md", "Mods/RouletteRevival.dll");
        }

        [Fact]
        public void WriteZip_ManifestHasNoByteOrderMark()
        {
            using var stream = new MemoryStream();

            ThunderstorePackage.WriteZip(stream, "{}", Png(256, 256), "r", "c", "Mod.dll", new byte[] { 1 });

            stream.Position = 0;
            using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
            using var manifest = new MemoryStream();
            zip.GetEntry("manifest.json").Open().CopyTo(manifest);
            manifest.ToArray().Should().Equal((byte)'{', (byte)'}');
        }

        [Fact]
        public void WriteZip_RequiresTheDll()
        {
            Action act = () => ThunderstorePackage.WriteZip(new MemoryStream(), "{}", Png(256, 256), "r", "c", "Mod.dll", Array.Empty<byte>());

            act.Should().Throw<ArgumentException>();
        }

        /// <summary>Just the PNG signature and IHDR header, which is all the size check reads.</summary>
        private static byte[] Png(int width, int height)
        {
            var bytes = new byte[33];
            new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 13, (byte)'I', (byte)'H', (byte)'D', (byte)'R' }.CopyTo(bytes, 0);
            BigEndian(width).CopyTo(bytes, 16);
            BigEndian(height).CopyTo(bytes, 20);
            return bytes;
        }

        private static byte[] BigEndian(int value) =>
            new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };
    }
}
