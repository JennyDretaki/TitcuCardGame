# Tichu WinForms - Smooth UI Edition

A cleaner offline Tichu implementation in C# Windows Forms / .NET 8.

## Main changes in this version

### No constant full UI refresh

The previous version rebuilt multiple WinForms controls after almost every action.

This version uses targeted UI updates:

- The human hand is rebuilt only when cards actually enter or leave the hand.
- Clicking a card only changes that specific card's visual state.
- Bot turns do not rebuild the human hand.
- Score labels, player counts and turn information are updated independently.
- Double-buffered custom panels reduce WinForms flickering.

This makes card selection and bot turns feel substantially smoother.

### No move history sidebar

The move-history / log panel has been completely removed.

The game screen now focuses only on:

- the table
- the four players
- current score
- current combination
- Mah Jong wish
- your hand
- action buttons

### Only the latest play is shown on the table

The game engine now separates:

- `trickPile`: all cards played in the current trick, retained internally for scoring
- `CurrentPlayCards`: only the cards from the latest play, shown on screen

When another player plays, the previous cards disappear from the center and are replaced by the new play.

This avoids a long row of cards accumulating across the table.

## Included gameplay

- 4 players: You + Partner vs Bot Left + Bot Right
- 56-card Tichu deck
- initial 3-card exchange
- Tichu
- Grand Tichu
- Mah Jong wish
- Dog
- Phoenix
- Dragon
- singles
- pairs
- triples
- full houses
- straights
- consecutive pairs
- four-of-a-kind bombs
- straight-flush bombs
- bombs outside normal turn order
- bot AI
- team scoring
- 1000-point match target

## Run

Open:

`TichuWinForms_Smooth.csproj`

with Visual Studio 2022 and .NET 8 Desktop Development installed, then press F5.
