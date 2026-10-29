# Changelog

## Unreleased

**What:** Players without the mod can play at a modded host's table.
- **Chat betting:** `!bet <spot> <amount>`, `!bets`, `!clear`, `!spin` and a paged `!help`. The host's mod catches the commands before they reach chat (both the server path and the host's own send path, which bypasses the server handler). It records the bets and settles them when the real wheel lands, through point RPCs the game already ships: the hide-and-seek award pays wins, and the arcade Big Wheel's spend RPC charges losses in 5,000 steps.
- **Table lock:** the host parks the spinner's synced "last spin" time far in the future, so games without the mod can't open the table and get trapped by its broken menu. A postfix on `RouletteSpinner.IsSpinning` lets modded games and the host use the real spin state.
- **Combined chips:** the game draws only the last bet per spot, so two players on red showed one stack. The host now folds every player's board and chat bets into a single entry holding the per-spot totals.
- **Chat output:** roulette lines are sent as system messages, which appear in the chat log with no speech bubble. Private replies (`!help`, `!bets`, errors) go only to the player who asked.
- **New settings:** `ChatBetting`, `ChatBetMaxPerSpin` and `BlockTableWithoutMod`.

**Why:** The betting menu is broken in every unmodded game's scene data and can't be fixed from the host. Chat betting plus host-side settlement is the only way to give those players real stakes. The table lock prevents the camera trap that used to strand them.

**Files:** `src/RouletteRevival/ChatBetting/**`, `src/RouletteRevival/TableLock/**`, `src/RouletteRevival/BetDisplay/**`, `src/RouletteRevival/Config/RouletteSettings.cs`, `src/RouletteRevival/RouletteRevivalMod.cs`, `src/RouletteRevival/RouletteRevival.csproj`, `tests/RouletteRevival.Tests/{ChatBetting,TableLock,BetDisplay}/**`, `tests/RouletteRevival.Tests/RouletteRevival.Tests.csproj`, `README.md`, `CHANGELOG.md`.

## 1.1.1 - 2026-10-29

**What:**
- The release bundle now ships `install.bat` and `scripts/install.ps1` with CRLF line endings.
- The installer's final "Done! Installed …" line shows only the installed path. The build log no longer gets folded into it.

**Why:** `git archive` writes the repository's LF endings regardless of `.gitattributes`, and `cmd.exe` batch files are only fully reliable with CRLF; the v1.1.0 asset was re-uploaded with the fix. PowerShell returns everything a function writes to the output stream, so the `dotnet build` output was being returned along with the path.

**Files:** `.github/workflows/release.yml`, `scripts/install.ps1`, `CHANGELOG.md`.

## 1.1.0 - 2026-10-29

**What:** First public release, on GitHub.
- `install.bat` / `scripts/install.ps1`: finds the game through Steam, installs MelonLoader 0.7.3 (SHA-256 pinned), generates the game assemblies, uses or privately installs a .NET SDK (Microsoft-signed installer only), then builds the mod against the player's own game and deploys it.
- Release automation: CI runs the tests. On push to `main`, the Release workflow bumps the version from Conventional Commits, stamps this changelog, pushes an SSH-signed release commit and tag, and publishes the installer bundle.
- The mod version now comes from `<Version>` in the csproj; `MelonInfo` reads a generated constant.
- The reverse-engineering notes are no longer published (gitignored).

**Why:** The mod compiles against files generated from each player's own game, which can't be redistributed or built on GitHub's runners. Building locally through the installer avoids both problems. Automated, signed releases keep versioning consistent.

**Files:** `install.bat`, `scripts/install.ps1`, `tools/ReleaseTool/**`, `tests/RouletteRevival.Tests/Release/**`, `tests/RouletteRevival.Tests/RouletteRevival.Tests.csproj`, `.github/workflows/{ci,release}.yml`, `src/RouletteRevival/RouletteRevival.csproj`, `src/RouletteRevival/RouletteRevivalMod.cs`, `SleddingGameRoulette.slnx`, `README.md`, `.gitignore`, `.gitattributes`, `CHANGELOG.md`.

## 1.0.3 - 2026-10-29

**What:** The default table spot is now inside the lodge arcade, by the VIP fence and the big prize wheel, at (2.32, 31.39, 125.85), yaw 327.56. It replaces the open ground beside the spawn platform.

**Why:** The host picked this spot in-game with F8 during the playtest and wants it as the default for every install. Players who already saved their own spot with F8 keep it.

**Files:** `src/RouletteRevival/Placement/PlacementMath.cs`, `README.md`, `src/RouletteRevival/RouletteRevivalMod.cs` and `RouletteRevival.csproj` (version 1.0.3), `CHANGELOG.md`.

## 1.0.2 - 2026-10-29

**What:** Fixed F8 relocation, which failed with a NullReferenceException and left the table where it was.

**Why:** `ServerManager.Despawn` was called with its optional `Nullable<DespawnType>` parameter left at the C# default `null`. Il2CppInterop represents that as an `Il2CppSystem.Nullable` object and throws on null. It now passes an empty `Il2CppSystem.Nullable<DespawnType>()`, which is FishNet's "use the object's default despawn type".

**Files:** `src/RouletteRevival/Spawning/RouletteTableSpawner.cs`, `src/RouletteRevival/RouletteRevivalMod.cs` and `RouletteRevival.csproj` (version 1.0.2), `CHANGELOG.md`.

## 1.0.1 - 2026-10-29

**What:** Fixed clicks placing bets on the last hovered tile when the mouse was no longer over any tile, including clicks on the +/−/clear buttons.

**Why:** Bets target `UiReferenceController.currentSelectedRouletteButton`, which a tile sets when it's selected. On mouse exit the game deselects the tile (`AnimationSelectableUi.Deselect`), but the roulette code never listened for deselect, so the reference stayed. A Harmony postfix on `Deselect` now clears it when it's that tile. Gamepad focus changes go through the same path, so a focus move away from the grid also clears it.

**Files:** `src/RouletteRevival/Hud/TileDeselectPatch.cs` (new), `src/RouletteRevival/RouletteRevivalMod.cs` and `RouletteRevival.csproj` (version 1.0.1), `CHANGELOG.md`.

## 1.0.0 - 2026-10-29

**What:** First release. Restores the sunsetted roulette minigame by reusing the content the game still ships.
- The host spawns the shipped "Roulette Setup" FishNet prefab beside the lodge spawn platform; FishNet replicates it to all clients. Host F8 moves it in front of the host and saves the spot.
- Every client wires the prefab's own betting HUD back into `UiReferenceController`: HUD fields, the `MenuType.RouletteTable` interaction-menu entry, and `Roulette_Setup()`. It unwires on despawn and prunes destroyed grid buttons.
- A Harmony prefix on `EnsureTopMenuHasValidSelection` lets gamepad players select the bet grid.
- MelonPreferences settings: enabled flag, saved position, relocate key.

**Why:** Asset and IL2CPP analysis showed no code kill switch. The feature was disabled purely in data: the prefab was not placed, and the scene's HUD references were nulled. With those nulls, interacting would throw in `OpenMenu` and leave the player stuck in the table camera.

**Files:** `src/RouletteRevival/**`, `tests/RouletteRevival.Tests/**`, `Directory.Build.props`, `SleddingGameRoulette.slnx`, `README.md`, `.gitignore`, `documentation/research/2026-10-28-roulette-reverse-engineering.md`.
