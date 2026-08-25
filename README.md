# Tichu WinForms - Enhanced

Offline Tichu game built with C# Windows Forms and .NET 8.

## Added in this version

- Initial 3-card exchange
  - one card to the player on the left
  - one card to the partner
  - one card to the player on the right
- Grand Tichu
  - separate declaration
  - +200 / -200 scoring
- Mah Jong wish
  - choose rank 2 through Ace
  - players must satisfy the wish when a legal play is available
- Bombs out of turn
  - four-of-a-kind bombs
  - straight-flush bombs
  - human can interrupt using the BOMB button
  - bots can also interrupt strategically
- Improved bot AI
  - preserves bombs and premium cards
  - evaluates cheaper winning combinations
  - tries to shed multiple cards
  - considers partner/opponent positions
  - strategic exchange logic
  - strategic bomb use
  - Grand Tichu hand-strength heuristic
  - Tichu-aware aggressive play
  - Mah Jong wish handling

## Existing features

- 4-player game
- You + Partner vs Bot Left + Bot Right
- 56-card Tichu deck
- Singles, pairs, triples
- Full houses
- Straights
- Consecutive pairs
- Four-of-a-kind bombs
- Straight-flush bombs
- Mah Jong
- Dog
- Phoenix
- Dragon
- Trick taking
- Card points
- Tichu declaration
- Double victory
- Team score
- 1000-point match target

## Controls

1. At the start, select 3 cards.
2. Click `EXCHANGE 3`.
3. Assign one selected card to Left, Partner and Right.
4. After exchange, play normally.
5. Select cards and press `PLAY`.
6. Press `PASS` when allowed.
7. If the selected cards form a bomb, press `BOMB` even when it is not your turn.
8. When playing Mah Jong, choose the wished rank.
9. `Call Tichu` and `Grand Tichu` are available before your first play, subject to the declaration phase.

## Run

Open:

`TichuWinForms.csproj`

in Visual Studio 2022 with the .NET 8 Desktop Development workload, then press F5.
