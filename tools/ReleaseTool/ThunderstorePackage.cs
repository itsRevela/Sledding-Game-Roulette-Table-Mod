using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ReleaseTool
{
    /// <summary>
    /// Builds the Thunderstore package: manifest.json, icon.png, README.md, CHANGELOG.md and the mod under Mods/, the folder
    /// mod managers install MelonLoader mods from.
    /// </summary>
    public static class ThunderstorePackage
    {
        public const int MaxDescriptionLength = 250;
        public const int IconSize = 256;

        private static readonly Regex PackageName = new Regex("^[A-Za-z0-9_]+$", RegexOptions.CultureInvariant);
        private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        /// <summary>The template's fields plus <paramref name="version"/>, checked against Thunderstore's upload rules.</summary>
        public static string BuildManifest(string templateJson, SemVer version)
        {
            if (!(ParseJson(templateJson ?? throw new ArgumentNullException(nameof(templateJson))) is JsonObject template))
            {
                throw new FormatException("The manifest template must be a JSON object.");
            }

            var name = RequiredString(template, "name");
            if (!PackageName.IsMatch(name))
            {
                throw new FormatException($"Package name '{name}' may only use letters, digits and underscores.");
            }

            var description = RequiredString(template, "description");
            if (description.Length > MaxDescriptionLength)
            {
                throw new FormatException($"The description is {description.Length} characters; Thunderstore allows {MaxDescriptionLength}.");
            }

            if (!(template["dependencies"] is JsonArray dependencies))
            {
                throw new FormatException("The manifest template needs a \"dependencies\" array.");
            }

            var manifest = new JsonObject
            {
                ["name"] = name,
                ["version_number"] = version.ToString(),
                ["website_url"] = RequiredString(template, "website_url"),
                ["description"] = description,
                ["dependencies"] = dependencies.DeepClone(),
            };
            return manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>Throws unless the player changelog has a "## X.Y.Z" section for <paramref name="version"/>.</summary>
        public static void EnsureChangelogCovers(string changelog, SemVer version)
        {
            var heading = new Regex($@"^## {Regex.Escape(version.ToString())}\s*$", RegexOptions.Multiline | RegexOptions.CultureInvariant);
            if (changelog == null || !heading.IsMatch(changelog))
            {
                throw new FormatException($"The player changelog has no \"## {version}\" section; add this version's notes first.");
            }
        }

        /// <summary>Throws unless <paramref name="png"/> is a PNG of exactly 256×256 pixels, as Thunderstore requires.</summary>
        public static void ValidateIcon(byte[] png)
        {
            const int headerLength = 24; // signature, IHDR length and type, width, height
            if (png == null || png.Length < headerLength || !png.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature))
            {
                throw new FormatException("The icon must be a PNG file.");
            }

            var width = ReadBigEndian(png, 16);
            var height = ReadBigEndian(png, 20);
            if (width != IconSize || height != IconSize)
            {
                throw new FormatException($"The icon is {width}×{height}; Thunderstore requires {IconSize}×{IconSize}.");
            }
        }

        public static void WriteZip(Stream output, string manifestJson, byte[] icon, string readme, string changelog, string dllName, byte[] dll)
        {
            ValidateIcon(icon);
            if (string.IsNullOrWhiteSpace(dllName) || dll == null || dll.Length == 0)
            {
                throw new ArgumentException("The mod DLL is missing.");
            }

            // UTF-8 without a BOM: Thunderstore rejects a manifest that starts with one.
            var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            using var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true);
            Add(zip, "manifest.json", utf8.GetBytes(manifestJson));
            Add(zip, "icon.png", icon);
            Add(zip, "README.md", utf8.GetBytes(readme));
            Add(zip, "CHANGELOG.md", utf8.GetBytes(changelog));
            Add(zip, "Mods/" + dllName, dll);
        }

        private static void Add(ZipArchive zip, string path, byte[] content)
        {
            using var stream = zip.CreateEntry(path, CompressionLevel.Optimal).Open();
            stream.Write(content, 0, content.Length);
        }

        private static JsonNode ParseJson(string json)
        {
            try
            {
                return JsonNode.Parse(json);
            }
            catch (JsonException ex)
            {
                throw new FormatException($"The manifest template isn't valid JSON: {ex.Message}", ex);
            }
        }

        private static string RequiredString(JsonObject template, string key) =>
            template[key] is JsonValue value && value.TryGetValue<string>(out var text) && !string.IsNullOrWhiteSpace(text)
                ? text
                : throw new FormatException($"The manifest template needs a non-empty \"{key}\".");

        private static int ReadBigEndian(byte[] bytes, int offset) =>
            (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
    }
}
