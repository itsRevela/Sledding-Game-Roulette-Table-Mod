using System;
using System.Collections.Generic;
using System.IO;

namespace ReleaseTool
{
    /// <summary>
    /// Release helper used by .github/workflows/release.yml.
    ///   next-version --csproj PATH --commits-file PATH [--force major|minor|patch]
    ///       prints the next version, or nothing when no releasable commits exist
    ///   apply --version X.Y.Z --csproj PATH --changelog PATH --commits-file PATH --date YYYY-MM-DD --notes-out PATH
    ///       writes the new version and changelog section, and the release notes
    /// Used by scripts/package-thunderstore.ps1:
    ///   thunderstore --csproj PATH --template PATH --icon PATH --readme PATH --changelog PATH --dll PATH --out-dir DIR
    ///       writes DIR/&lt;name&gt;-&lt;version&gt;.zip and prints its path
    /// </summary>
    public static class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    throw new ArgumentException("Missing command: next-version, apply or thunderstore.");
                }

                var options = ParseOptions(args);
                switch (args[0])
                {
                    case "next-version":
                        NextVersion(options);
                        return 0;
                    case "apply":
                        Apply(options);
                        return 0;
                    case "thunderstore":
                        Console.WriteLine(Thunderstore(options));
                        return 0;
                    default:
                        throw new ArgumentException($"Unknown command '{args[0]}'.");
                }
            }
            catch (Exception ex) when (ex is ArgumentException || ex is FormatException || ex is IOException || ex is InvalidOperationException)
            {
                Console.Error.WriteLine($"release-tool: {ex.Message}");
                return 1;
            }
        }

        private static void NextVersion(IReadOnlyDictionary<string, string> options)
        {
            var current = SemVer.Parse(ReleaseFiles.GetCsprojVersion(File.ReadAllText(Required(options, "csproj"))));
            var commits = VersionBump.SplitGitLog(File.ReadAllText(Required(options, "commits-file")));
            options.TryGetValue("force", out var forced);
            var level = VersionBump.ParseForced(forced) ?? VersionBump.LevelFor(commits);
            if (level != BumpLevel.None)
            {
                Console.WriteLine(current.Bump(level));
            }
        }

        private static void Apply(IReadOnlyDictionary<string, string> options)
        {
            var version = SemVer.Parse(Required(options, "version"));
            var csprojPath = Required(options, "csproj");
            var changelogPath = Required(options, "changelog");
            var commits = VersionBump.SplitGitLog(File.ReadAllText(Required(options, "commits-file")));

            File.WriteAllText(csprojPath, ReleaseFiles.SetCsprojVersion(File.ReadAllText(csprojPath), version));
            var release = ReleaseFiles.ReleaseChangelog(File.ReadAllText(changelogPath), version, Required(options, "date"), commits);
            File.WriteAllText(changelogPath, release.Changelog);
            File.WriteAllText(Required(options, "notes-out"), release.Notes);
        }

        /// <summary>Builds the Thunderstore zip and returns its path.</summary>
        public static string Thunderstore(IReadOnlyDictionary<string, string> options)
        {
            var version = SemVer.Parse(ReleaseFiles.GetCsprojVersion(File.ReadAllText(Required(options, "csproj"))));
            var manifest = ThunderstorePackage.BuildManifest(File.ReadAllText(Required(options, "template")), version);
            var name = System.Text.Json.Nodes.JsonNode.Parse(manifest)["name"].GetValue<string>();
            var changelog = File.ReadAllText(Required(options, "changelog"));
            ThunderstorePackage.EnsureChangelogCovers(changelog, version);
            var dllPath = Required(options, "dll");
            var outDir = Directory.CreateDirectory(Required(options, "out-dir")).FullName;
            var zipPath = Path.Combine(outDir, $"{name}-{version}.zip");

            // Built in memory so a failed check never leaves a broken zip in the output folder.
            using var buffer = new MemoryStream();
            ThunderstorePackage.WriteZip(buffer, manifest, File.ReadAllBytes(Required(options, "icon")),
                File.ReadAllText(Required(options, "readme")), changelog, Path.GetFileName(dllPath), File.ReadAllBytes(dllPath));
            File.WriteAllBytes(zipPath, buffer.ToArray());
            return zipPath;
        }

        /// <summary>Parses "--name value" pairs after the command; a flag with no value (or an empty one) maps to "".</summary>
        public static IReadOnlyDictionary<string, string> ParseOptions(string[] args)
        {
            var options = new Dictionary<string, string>(StringComparer.Ordinal);
            for (var i = 1; i < args.Length; i++)
            {
                var name = args[i];
                if (!name.StartsWith("--", StringComparison.Ordinal) || name.Length == 2)
                {
                    throw new ArgumentException($"Unexpected argument '{name}'.");
                }

                var hasValue = i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal);
                options[name.Substring(2)] = hasValue ? args[++i] : string.Empty;
            }

            return options;
        }

        private static string Required(IReadOnlyDictionary<string, string> options, string name) =>
            options.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : throw new ArgumentException($"Missing --{name}.");
    }
}
