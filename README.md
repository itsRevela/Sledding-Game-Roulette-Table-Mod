# Roulette Revival (Sledding Game mod)

A MelonLoader mod that brings back the roulette table Sledding Game sunsetted. You bet your in-game points at the table and spin the wheel, alongside your friends.

The minigame was never deleted. The game still ships the full network prefab ("Roulette Setup": table, wheel, betting board, chips, sounds) and all of its betting and payout code. The sunset removed only two things:

1. the table's placement in the world;
2. the scene's links to the betting HUD. The HUD itself still sits, inactive, inside the prefab.

This mod restores both. It also adds chat betting, so players who don't have the mod can still play at a modded host's table.

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

**Only the host needs the mod.** The host's copy spawns the table, and FishNet replicates it to every player. Players with the mod use the real betting board. Players without it bet by chat (see below).

## Play

- **Host** a lobby. After about 2 seconds the table appears inside the lodge arcade, by the VIP fence and the big prize wheel.
- **With the mod:** interact with the **table** to open the betting board. Point at a square (mouse or gamepad) and press **Place Bet** or **Remove Bet** (likely left/right mouse button; the prompts on the board show your actual bindings). `+`/`-` change the chip size and **clear** returns your bets. Interact with the **wheel** to spin.
- **Without the mod:** bet in chat (next section). The host locks the table and wheel for these players, so pressing F on them does nothing. Without the lock, the game's broken betting menu would trap them in the table camera.
- **Host only: F8** moves the table in front of you and saves that spot for future sessions.

The mod only spawns the table in lobbies you host. In anyone else's lobby it never spawns anything.

Points are client-local in this game, so each player's winnings and losses go to their own save, as they did when the feature was live. Board bets are taken when the wheel starts and paid when it stops. Betting over 100,000 in one spin with the board unlocks the game's Casino Visor hat.

### Chat betting

Anyone can type these in chat. The commands themselves never show in chat; the host's mod handles them.

| Command | What it does |
|---|---|
| `!bet <spot> <amount>` | Bet, e.g. `!bet 17 10k`, `!bet red 5000`. Betting the same spot again adds to it. |
| `!bets` | Lists your bets (only you see the reply). |
| `!clear` | Takes your bets back before the spin. |
| `!spin` | Spins the wheel once anyone has bet. |
| `!help [page]` | Four short help pages. |

Spots: `0`–`36`, `red`, `black`, `odd`, `even`, `low` (1–18), `high` (19–36), `dozen1`–`dozen3`, `col1`–`col3`. Payouts follow the game: a number wins 35×, a dozen or column 2×, everything else 1×.

Chips from every player, board and chat, show as one stack per spot. Results are posted in chat ("Roulette: 17 black! Crunchy +175,000"), and each chat bettor's net result goes to their own points when the ball lands.

**Limits:**
- Chat bets go in steps of 5,000, up to 500,000 per player per spin (configurable). The game only lets a host charge a player in units of the arcade Big Wheel's 5,000 spin cost.
- If a chat bettor can't cover a loss, their game silently skips that charge. The host can't see other players' points.

## Configure

`<game>/UserData/MelonPreferences.cfg`, section `[RouletteRevival]`:

| Key | Default | Meaning |
|---|---|---|
| `Enabled` | `true` | Spawn the table when you host. The HUD fix always runs. |
| `UseCustomPosition` | `false` | Set by F8. Set it to `false` to go back to the default spot. |
| `PositionX/Y/Z`, `Yaw` | arcade spot | Saved table position and heading, in degrees. |
| `RelocateKey` | `F8` | Any of `F1`–`F12`. |
| `ChatBetting` | `true` | Host only: accept `!bet`/`!clear`/`!bets`/`!spin`/`!help`. |
| `ChatBetMaxPerSpin` | `500000` | Host only: the most one player can stake on one spin by chat (capped at 10,000,000). |
| `BlockTableWithoutMod` | `true` | Host only: stop players without the mod from opening the table (and its wheel). |

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
  RouletteRevivalMod.cs            entry point: per-frame tick for the spawner, HUD binder, table lock, chat settlement, hotkey
  Config/RouletteSettings.cs       MelonPreferences entries
  Spawning/                        host-side: find the prefab, spawn via FishNet, ground snap, F8 relocation
  Hud/                             every client: wire the prefab's HUD into UiReferenceController; input fixes
  ChatBetting/                     host-side: chat commands, bet ledger, settlement via the game's point RPCs, chat output
  TableLock/                       host parks the spin timer so unmodded games can't open the table; modded games see through it
  BetDisplay/                      host combines every player's bets into one entry so each spot shows its total
  SpinDispatch/                    host sends the spin result just after the wheel stops; modded games undo the offset
  Placement/, Input/HotkeyParser.cs, Spawning/PrefabNames.cs, Hud/HudPaths.cs, ChatBetting/*Parser.cs,
  ChatBetting/{BetLedger,Settlement,ChatHelp,ChatLineFormat}.cs, TableLock/SpinLockRules.cs, BetDisplay/BetAggregation.cs,
  SpinDispatch/SpinTiming.cs
                                   engine-free logic, compiled into the tests
tools/ReleaseTool/                 version bump and changelog stamping used by the release workflow
tests/RouletteRevival.Tests/       xUnit tests for the engine-free logic and the release tool
.github/workflows/                 CI (tests) and Release
```

## Uninstall

Delete `<game>/Mods/RouletteRevival.dll`. To remove MelonLoader too, delete `version.dll` and the `MelonLoader/` folder.
