# Roulette Revival (Sledding Game mod)

A MelonLoader mod that brings back the roulette table Sledding Game sunsetted. You bet your in-game points at the table and spin the wheel, alongside your friends.

The minigame was never deleted. The game still ships the full network prefab ("Roulette Setup": table, wheel, betting board, chips, sounds) and all of its betting and payout code. The sunset removed only two things:

1. the table's placement in the world;
2. the scene's links to the betting HUD. The HUD itself still sits, inactive, inside the prefab.

This mod restores both and adds no gameplay of its own.

## Install

1. Download `RouletteRevival-vX.Y.Z.zip` from the [latest release](https://github.com/itsRevela/Sledding-Game-Roulette-Table-Mod/releases/latest) and extract it anywhere.
2. Close Sledding Game and run `install.bat`.

The installer:
- finds the game through Steam (or pass `install.bat -GameDir "D:\path\to\Sledding Game"`);
- installs MelonLoader 0.7.3 if it's missing, checking the download's SHA-256;
- launches the game once so MelonLoader can generate its assemblies;
- uses your .NET SDK 8+, or installs a private copy into `%LOCALAPPDATA%\RouletteRevival` (Microsoft-signed installer, no admin rights);
- builds the mod against your copy of the game and copies it into `<game>\Mods`.

The mod is built on your PC because it compiles against files generated from your copy of the game, which can't be redistributed. **After a game update**, launch the game once, close it, then run `install.bat` again.

**Everyone in the lobby needs the mod to bet.** The host's copy spawns the table, and FishNet replicates it to all clients, so everyone can see it and spin the wheel. Each player's copy wires up the betting HUD. A player without the mod who uses the table gets stuck in the table camera, because the game's own menu code throws on the missing HUD.

## Play

- **Host** a lobby. After about 2 seconds the table appears inside the lodge arcade, by the VIP fence and the big prize wheel.
- Walk up and interact with the **table** to open the betting board. Point at a square (mouse or gamepad) and press **Place Bet** or **Remove Bet** (likely left/right mouse button; the prompts on the board show your actual bindings). `+`/`-` change the chip size and **clear** returns your bets.
- Interact with the **wheel** to spin. Any player can spin, and payouts go to each player's own points.
- **Host only: F8** moves the table in front of you and saves that spot for future sessions.

The mod only spawns the table in lobbies you host. In anyone else's lobby it never spawns anything.

Points are client-local in this game, so each player's winnings and losses go to their own save, as they did when the feature was live. Betting over 100,000 in one spin unlocks the game's Casino Visor hat.

## Configure

`<game>/UserData/MelonPreferences.cfg`, section `[RouletteRevival]`:

| Key | Default | Meaning |
|---|---|---|
| `Enabled` | `true` | Spawn the table when you host. The HUD fix always runs. |
| `UseCustomPosition` | `false` | Set by F8. Set it to `false` to go back to the default spot. |
| `PositionX/Y/Z`, `Yaw` | arcade spot | Saved table position and heading, in degrees. |
| `RelocateKey` | `F8` | Any of `F1`–`F12`. |

## Develop

Requires the .NET SDK 10 and the game with MelonLoader already run once, because the mod references `MelonLoader/Il2CppAssemblies`.

```powershell
dotnet build -c Release                                          # mod, release tool, tests
dotnet test                                                      # unit tests
dotnet build src/RouletteRevival -c Release -p:Deploy=true       # build and copy into <game>/Mods
```

If the game isn't in the default Steam path, pass `-p:GameDir="D:\path\to\Sledding Game"`.

## Releases

Releases are automatic. On every push to `main`, [`.github/workflows/release.yml`](.github/workflows/release.yml) reads the [Conventional Commits](https://www.conventionalcommits.org/) since the last `v*` tag:

| Commits | Bump |
|---|---|
| `feat!:`, `fix!:` or a `BREAKING CHANGE:` footer | major |
| `feat:` | minor |
| `fix:`, `perf:` | patch |
| anything else (`docs:`, `chore:`, `ci:`, `test:`, `refactor:`) | no release |

When a release is due, the workflow:
1. sets `<Version>` in `RouletteRevival.csproj`; the mod's `MelonInfo` version is generated from it;
2. renames the `## Unreleased` section of `CHANGELOG.md` to the new version, or adds a section listing the commit subjects;
3. pushes an SSH-signed `chore(release): vX.Y.Z` commit and a signed `vX.Y.Z` tag;
4. publishes a GitHub release with the installer bundle.

You can force a bump level from the Actions tab (**Release → Run workflow**). Signing uses the `RELEASE_SIGNING_KEY` secret and the `RELEASE_GIT_NAME`/`RELEASE_GIT_EMAIL` repository variables. The key's public half is registered on the maintainer's account as a signing key.

## Layout

```
install.bat, scripts/install.ps1   end-user installer
src/RouletteRevival/
  RouletteRevivalMod.cs            entry point: per-frame tick for the spawner, HUD binder and hotkey
  Config/RouletteSettings.cs       MelonPreferences entries
  Spawning/                        host-side: find the prefab, spawn via FishNet, ground snap, F8 relocation
  Hud/                             every client: wire the prefab's HUD into UiReferenceController; input fixes
  Placement/, Input/HotkeyParser.cs, Spawning/PrefabNames.cs, Hud/HudPaths.cs
                                   engine-free logic, compiled into the tests
tools/ReleaseTool/                 version bump and changelog stamping used by the release workflow
tests/RouletteRevival.Tests/       xUnit tests for the engine-free logic and the release tool
.github/workflows/                 CI (tests) and Release
```

## Uninstall

Delete `<game>/Mods/RouletteRevival.dll`. To remove MelonLoader too, delete `version.dll` and the `MelonLoader/` folder.
