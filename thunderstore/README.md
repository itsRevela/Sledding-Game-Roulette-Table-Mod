# Roulette Revival

Brings back the roulette table that Sledding Game removed. Bet your points, spin the wheel and win (or lose) with your friends.

## Features

- **The real table is back:** the game's own betting board, chips, wheel and sounds, placed in the lodge arcade.
- **Only the host needs the mod.** Friends without it can still play by typing bets in chat.
- **Everyone's chips on one table:** each spot shows the total of all players' bets.
- **Move the table anywhere:** the host presses F8 to put it in front of them, and the spot is remembered.
- Works with mouse and keyboard or a gamepad.

## Installation

Install it with a mod manager such as **r2modman** or **Gale**. MelonLoader is installed for you.

Installing by hand: install [MelonLoader 0.7.3](https://thunderstore.io/c/sledding-game/p/LavaGang/MelonLoader/), run the game once, then copy `RouletteRevival.dll` from this package's `Mods` folder into the game's `Mods` folder.

## How to play

1. **Host** a lobby. The table appears inside the lodge arcade, by the VIP fence and the big prize wheel, about 2 seconds after the lobby starts.
2. **If you have the mod:** walk up to the table and interact with it to open the betting board.
   - Pick a square and press **Place Bet** or **Remove Bet**. The prompts on the board show your buttons.
   - `+` and `−` change the chip size, and **clear** takes your bets back.
   - Interact with the **wheel** to spin it.
3. **If you don't have the mod:** bet in chat (see below). The table and wheel don't open for you, because the game's betting menu is broken without the mod.

Winnings and losses go to each player's own points.

## Betting in chat

Anyone in a modded host's lobby can type these. They never show up in chat; the host's mod answers them.

| Command | What it does |
|---|---|
| `!bet <spot> <amount>` | Places a bet, e.g. `!bet 17 10k` or `!bet red 5000`. Betting the same spot again adds to it. |
| `!bets` | Shows your bets (only you see the reply). |
| `!clear` | Takes your bets back before the spin. |
| `!spin` | Spins the wheel once anyone has bet. |
| `!help` | Shows help in chat (`!help 2`, `!help 3`, `!help 4` for more). |

**Spots:** `0`–`36`, `red`, `black`, `odd`, `even`, `low` (1–18), `high` (19–36), `dozen1`–`dozen3`, `col1`–`col3`.

**Payouts:** a single number pays 35 to 1, a dozen or column 2 to 1, everything else 1 to 1.

**Amounts:** chat bets go in steps of 5,000, up to 500,000 per spin. That's the smallest amount the game lets the host take from another player. If you can't cover a loss, it isn't taken.

## Settings

Change these in `UserData/MelonPreferences.cfg`, under `[RouletteRevival]`. Only the host's settings matter.

| Setting | Default | What it does |
|---|---|---|
| `Enabled` | `true` | Places the table when you host. |
| `RelocateKey` | `F8` | The key that moves the table (`F1`–`F12`). |
| `ChatBetting` | `true` | Lets players bet by chat. |
| `ChatBetMaxPerSpin` | `500000` | The most one player can bet by chat in one spin (up to 10,000,000). |
| `BlockTableWithoutMod` | `true` | Keeps players without the mod out of the broken table menu. |

## Good to know

- The table only appears in lobbies you host.
- After a game update the mod may stop working until it's updated.
- Found a bug? Open an issue on [GitHub](https://github.com/itsRevela/Sledding-Game-Roulette-Table-Mod/issues).
