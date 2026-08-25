# Tichu WinForms – Complete README

## Overview

**Tichu WinForms** is an offline implementation of the card game **Tichu**, developed in **C# using Windows Forms and .NET 8**.

The game is designed as a desktop application for Windows and allows one human player to play together with three computer-controlled opponents.

The table consists of four players divided into two teams:

* **You**
* **Partner**
* **Bot Left**
* **Bot Right**

The teams are:

**Team 1**

* You
* Partner

**Team 2**

* Bot Left
* Bot Right

The game includes the main Tichu mechanics, special cards, card combinations, team scoring, Tichu declarations, Grand Tichu, initial card exchange, Mah Jong wishes, bombs and computer-controlled opponents.

---

# Technologies

The project was developed using:

* C#
* .NET 8
* Windows Forms
* Object-Oriented Programming
* Event-driven programming
* Custom UI controls
* Game-state management
* Rule validation algorithms
* Basic Artificial Intelligence for computer players

---

# Requirements

To run the project you need:

* Windows 10 or Windows 11
* Visual Studio 2022 or newer
* .NET 8 SDK
* .NET Desktop Development workload

---

# How to Run

1. Download and extract the project.

2. Open the project folder.

3. Open:

`TichuWinForms.csproj`

4. Visual Studio should automatically restore the required .NET dependencies.

5. Press:

`F5`

or click:

`Start`

The game window will open automatically.

---

# Game Objective

Tichu is a team-based card game for four players.

The objective is to get rid of all the cards in your hand before the other players while helping your teammate.

Players gain points from certain cards collected during tricks.

The match continues through multiple rounds until one of the teams reaches approximately:

`1000 points`

The team with the highest score wins the match.

---

# Players

There are four players.

### You

The human-controlled player.

You select cards manually and decide whether to:

* Play
* Pass
* Use a bomb
* Call Tichu
* Call Grand Tichu
* Choose a Mah Jong wish
* Exchange cards

### Partner

Your computer-controlled teammate.

The Partner attempts to cooperate with you indirectly through the AI logic.

### Bot Left

Computer-controlled opponent.

### Bot Right

Computer-controlled opponent.

---

# The Deck

The Tichu deck contains:

`56 cards`

There are four normal suits.

Each suit contains cards from:

`2 to Ace`

Normal ranks are:

* 2
* 3
* 4
* 5
* 6
* 7
* 8
* 9
* 10
* Jack
* Queen
* King
* Ace

There are also four special cards:

* Mah Jong
* Dog
* Phoenix
* Dragon

---

# Special Cards

## Mah Jong

Mah Jong is the lowest card in the game.

Its value is:

`1`

The player holding Mah Jong begins the first trick of the round.

When Mah Jong is played, the player may make a **wish**.

The player chooses a normal rank between:

`2 and Ace`

For example:

`Mah Jong Wish: 8`

While the wish is active, players who are able to legally play an 8 must include an 8 in their play.

The wish disappears once the requested rank has been legally played.

---

# Dog

The Dog has no numerical value.

It may only be played when the player has the lead.

When played, the current trick ends immediately and the lead is transferred to the player's teammate.

Example:

If **You** play Dog, the next player to lead is **Partner**.

The Dog cannot beat another card.

---

# Phoenix

Phoenix is one of the strongest and most flexible cards in the deck.

It can act as a wildcard in several combinations.

It can participate in combinations such as:

* Pair
* Triple
* Full House
* Straight
* Consecutive Pairs

As a single card, Phoenix receives a dynamic value depending on the previous card.

Phoenix is worth:

`-25 points`

during scoring.

Phoenix cannot be used to create a bomb.

---

# Dragon

Dragon is the highest normal single card.

It beats every other single card except a bomb.

Dragon is worth:

`25 points`

during scoring.

When a player wins a trick containing the Dragon, the trick must be given to an opponent.

In the current implementation, the game automatically chooses an opposing player to receive the Dragon trick.

---

# Initial Card Exchange

At the beginning of every round, each player receives:

`14 cards`

Before normal play starts, every player must exchange three cards.

Each player gives:

* One card to the player on the left
* One card to their partner
* One card to the player on the right

For the human player:

1. Select exactly three cards from your hand.
2. Click:

`EXCHANGE 3`

3. A dialog appears.
4. Assign one card to:

`Bot Left`

5. Assign one card to:

`Partner`

6. Assign one card to:

`Bot Right`

7. Confirm the exchange.

The computer players automatically choose their exchange cards using AI logic.

---

# Starting the Round

After the exchange is complete, the player holding:

`Mah Jong`

starts the round.

The game automatically detects which player owns Mah Jong.

---

# Playing Cards

During your turn:

1. Click one or more cards in your hand.
2. Selected cards move slightly upward.
3. The game displays the detected combination.
4. Click:

`PLAY`

If the combination is legal, it is placed on the table.

If it is not legal, an error message is displayed.

---

# Passing

If another player has already played a combination and you do not want or cannot beat it, click:

`PASS`

You cannot pass when you are leading a new trick.

If a Mah Jong wish is active and you are able to satisfy it, passing is not allowed.

---

# Legal Card Combinations

The game supports the primary Tichu combinations.

---

## Single

One card.

Example:

`10♠`

To beat a single, another player must play a higher single.

---

## Pair

Two cards of the same rank.

Example:

`8♠ 8♥`

A higher pair beats a lower pair.

---

## Triple

Three cards of the same rank.

Example:

`Q♠ Q♥ Q♦`

---

## Full House

A triple plus a pair.

Example:

`10♠ 10♥ 10♦ 7♠ 7♥`

The value of the Full House is determined by the triple.

---

# Straight

Five or more consecutive cards.

Example:

`5 6 7 8 9`

Another straight must contain the same number of cards and have a higher highest card.

---

# Consecutive Pairs

Two or more consecutive pairs.

Example:

`5 5 6 6`

or:

`7 7 8 8 9 9`

The number of cards must match the previous combination.

---

# Bombs

Bombs are special combinations capable of interrupting normal play.

They are stronger than ordinary combinations.

The game supports two bomb types.

---

## Four-of-a-Kind Bomb

Four cards of the same rank.

Example:

`9♠ 9♥ 9♦ 9♣`

---

## Straight Flush Bomb

Five or more consecutive cards of the same suit.

Example:

`6♠ 7♠ 8♠ 9♠ 10♠`

Straight Flush Bombs are stronger than Four-of-a-Kind Bombs.

Longer Straight Flush Bombs are stronger than shorter ones.

---

# Bombs Out of Turn

Unlike normal combinations, bombs can be played outside the normal turn order.

To use a bomb:

1. Select the cards forming the bomb.
2. Click:

`BOMB`

You may use the BOMB button even when another player is currently taking their turn.

If the selected cards do not form a valid bomb, the game displays an error.

Computer players can also use bombs outside their turn.

Their AI evaluates whether using the bomb is strategically worthwhile.

---

# Tichu Declaration

Before playing their first card, a player may declare:

`TICHU`

A successful Tichu means the player must finish first in the round.

If successful:

`+100 points`

are awarded to the player's team.

If unsuccessful:

`-100 points`

are applied to the player's team.

To declare Tichu, click:

`Call Tichu`

The option becomes unavailable after you play your first card.

---

# Grand Tichu

Grand Tichu is a more risky version of Tichu.

It is normally declared before the player receives their complete hand.

In this project, Grand Tichu is available during the initial phase before the card exchange is completed.

A successful Grand Tichu gives:

`+200 points`

An unsuccessful Grand Tichu gives:

`-200 points`

Click:

`Grand Tichu`

to declare it.

Bots evaluate the strength of their initial cards and may automatically declare Grand Tichu.

---

# Finishing a Round

When a player removes all cards from their hand, their finishing position is recorded.

Possible finishing positions are:

* 1st
* 2nd
* 3rd
* 4th

---

# Double Victory

If the first two players to finish belong to the same team, that team receives:

`200 points`

This is known as a double victory.

Normal trick scoring is skipped for that round.

Tichu and Grand Tichu declarations are still evaluated.

---

# Card Points

Certain cards are worth points.

### Five

Each 5 is worth:

`5 points`

There are four 5s.

Total:

`20 points`

### Ten

Each 10 is worth:

`10 points`

There are four 10s.

Total:

`40 points`

### King

Each King is worth:

`10 points`

There are four Kings.

Total:

`40 points`

### Dragon

Dragon is worth:

`25 points`

### Phoenix

Phoenix is worth:

`-25 points`

The total point value available from cards in a round is:

`100 points`

---

# Team Scoring

At the end of a normal round, captured cards are counted.

The points of cards captured by both teammates are combined.

The score is displayed at the top of the application.

Example:

`Your team: 340`

`Opponents: 260`

---

# Match End

The match continues through multiple rounds.

When one team reaches approximately:

`1000 points`

the match ends.

The team with the higher score is declared the winner.

A new match can then be started.

---

# Artificial Intelligence

The computer players use rule-based AI.

They do not simply select random cards.

The AI evaluates several factors before making a decision.

---

## Combination Evaluation

Bots search their hands for legal combinations that can beat the current table combination.

When several combinations are possible, the AI attempts to select the least expensive option.

For example, if the table contains:

`Pair of 7`

and the bot owns:

* Pair of 8
* Pair of Queen

the AI generally prefers:

`Pair of 8`

to preserve stronger cards.

---

# Card Preservation

The AI tries to preserve strategically important cards.

Examples include:

* Dragon
* Phoenix
* Bombs
* Triples
* Strong pairs

It avoids unnecessarily destroying useful combinations.

---

# Multi-Card Strategy

When leading a trick, the AI often prefers combinations that allow it to remove several cards simultaneously.

For example, it may choose a:

* Straight
* Full House
* Triple
* Pair

instead of always playing single cards.

---

# Bomb Strategy

Bots can detect bombs in their hands.

They can play bombs:

* During their own turn
* Outside their turn

Bots are more likely to use a bomb when:

* An opponent has very few cards remaining
* The bot is close to finishing
* The bomb is required to regain control

Bots generally avoid wasting bombs unnecessarily.

---

# Exchange AI

During the initial three-card exchange, bots evaluate their hands.

They attempt to preserve:

* Bombs
* Phoenix
* Dragon
* Strong pairs
* Triples

Weak isolated cards are more likely to be sent to opponents.

The AI may give a stronger or more strategically useful card to its teammate.

---

# Grand Tichu AI

Before the complete hand is dealt, bots estimate their initial hand strength.

The evaluation considers factors such as:

* Dragon
* Phoenix
* Aces
* Kings
* High cards
* Pairs
* Triples

If the estimated hand strength is sufficiently high, the bot may declare:

`Grand Tichu`

---

# Tichu Strategy

When a bot has declared Tichu or Grand Tichu, its strategy becomes more aggressive.

The bot prioritizes getting rid of cards quickly and is more willing to use stronger combinations.

---

# Mah Jong Wish AI

When a bot plays Mah Jong, it automatically selects a requested rank.

The AI considers which ranks are currently absent from its own hand and generally prefers requesting useful medium or high cards.

All players are then required to satisfy the wish whenever legally possible.

---

# User Interface

The application contains several main areas.

---

## Score Area

At the top of the window you can see:

* Your team's score
* Opponents' score
* Tichu buttons
* Grand Tichu button
* New Round button

---

# Opponent Areas

The three computer-controlled players are displayed around the table.

For each player, the UI displays:

* Player name
* Number of remaining cards
* Finishing position
* Tichu status

---

# Table Area

The center of the application displays:

* Current player's turn
* Current combination
* Combination value
* Number of cards
* Active Mah Jong wish
* Cards currently played

---

# Human Hand

Your cards are displayed at the bottom of the application.

Click a card to select it.

Selected cards are visually raised.

Click the card again to deselect it.

---

# Action Buttons

## PLAY

Attempts to play the selected cards.

---

## PASS

Passes the current trick.

---

## BOMB

Attempts to play the selected cards as a bomb.

This button can also be used outside your normal turn.

---

## EXCHANGE 3

Used during the beginning of the round.

It becomes available after exactly three cards have been selected.

---

## Call Tichu

Declares normal Tichu.

---

## Grand Tichu

Declares Grand Tichu.

---

## New Round

Starts the next round after the current round has ended.

---

# Game Log

The right side of the application contains a game log.

The log records important events such as:

* Cards played
* Players passing
* Tricks won
* Tichu declarations
* Grand Tichu declarations
* Bombs
* Mah Jong wishes
* Players finishing
* Round results

Example:

`Bot Left passed.`

`Partner played Pair: 9♠ 9♥`

`You INTERRUPTED with a BOMB!`

`You finished #1.`

---

# Project Structure

The project is divided into several files and folders.

```text
TichuWinForms
│
├── Game
│   ├── Combination.cs
│   ├── CombinationEvaluator.cs
│   ├── DeckFactory.cs
│   └── TichuGame.cs
│
├── Models
│   ├── Card.cs
│   └── Player.cs
│
├── MainForm.cs
├── MainForm.Designer.cs
├── Program.cs
├── TichuWinForms.csproj
└── README.md
```

---

# Main Files

## Program.cs

Entry point of the application.

It initializes Windows Forms and launches:

`MainForm`

---

# MainForm.cs

Contains the main user-interface logic.

Responsibilities include:

* Handling card selection
* Play button
* Pass button
* Bomb button
* Exchange button
* Tichu declaration
* Grand Tichu declaration
* Mah Jong wish dialog
* Bot turn execution
* UI updates
* Game log updates

---

# MainForm.Designer.cs

Contains the Windows Forms UI definitions.

It defines controls such as:

* Panels
* Labels
* Buttons
* FlowLayoutPanels
* GroupBoxes
* ListBox

---

# Card.cs

Represents an individual Tichu card.

Important properties include:

```csharp
Suit
Rank
Special
Points
RankText
SuitSymbol
```

The class also calculates the point value of each card.

---

# Player.cs

Represents a player.

Important properties include:

```csharp
Name
Seat
Team
Hand
Captured
FinishOrder
CalledTichu
CalledGrandTichu
HasPlayedAnyCard
```

---

# DeckFactory.cs

Creates the complete 56-card deck.

It also contains the deck shuffling algorithm.

---

# Combination.cs

Represents a card combination.

Supported combination types include:

```csharp
Single
Pair
Triple
FullHouse
Straight
ConsecutivePairs
FourBomb
StraightFlushBomb
Dog
```

---

# CombinationEvaluator.cs

Responsible for validating combinations.

It determines:

* Whether selected cards create a legal combination
* Combination type
* Combination value
* Whether a combination beats the previous one
* Whether a play is a bomb

---

# TichuGame.cs

Contains the main game engine.

Responsibilities include:

* Dealing cards
* Exchange system
* Player turns
* Validating plays
* Passing
* Trick resolution
* Bomb interruptions
* Mah Jong wish handling
* Tichu
* Grand Tichu
* Player finishing order
* Round scoring
* Team scoring
* Bot decisions

---

# Important Game State

The game engine tracks several important values.

```csharp
CurrentPlayerIndex
TableCombination
TableCards
TeamScores
FinishCounter
RoundOver
ExchangeCompleted
MahJongWishRank
```

---

# Combination Validation

When cards are selected, they are sent to:

```csharp
CombinationEvaluator.Evaluate(...)
```

The evaluator determines whether the cards form a valid Tichu combination.

The resulting object contains information such as:

```csharp
Type
Value
CardCount
Cards
```

The game then compares the new combination with the previous table combination using:

```csharp
CombinationEvaluator.CanBeat(...)
```

---

# Turn System

The game stores the current player in:

```csharp
CurrentPlayerIndex
```

After a valid move, the turn normally moves clockwise to the next active player.

Players who have already finished are skipped.

---

# Trick System

When a player plays cards, the player becomes the current trick leader.

Other players may either:

* Beat the combination
* Pass
* Interrupt with a bomb

When all remaining opponents pass, the trick is awarded to the last player who successfully played cards.

The cards are transferred to that player's captured pile.

That player normally begins the next trick.

---

# Scoring Implementation

At the end of the round, captured cards are evaluated.

The game calculates:

```csharp
Players
    .Where(p => p.Team == team)
    .SelectMany(p => p.Captured)
    .Sum(c => c.Points);
```

Tichu declarations are then applied.

Normal Tichu:

```text
Success: +100
Failure: -100
```

Grand Tichu:

```text
Success: +200
Failure: -200
```

---

# Object-Oriented Design

The project separates different responsibilities.

`Card`

represents game data.

`Player`

represents players.

`CombinationEvaluator`

contains card-rule logic.

`TichuGame`

contains game-state logic.

`MainForm`

contains interface and user interaction logic.

This structure makes the project easier to:

* Understand
* Debug
* Maintain
* Expand

---

# Possible Future Improvements

The project can be expanded further.

Possible additions include:

* Difficulty levels for bots
* Minimax-based AI
* Monte Carlo game simulation
* Online multiplayer
* LAN multiplayer
* Player names
* Match history
* Statistics
* Save/load system
* Sound effects
* Music
* Card animations
* Drag-and-drop card selection
* Improved card graphics
* Settings menu
* Language selection
* Fullscreen mode
* Responsive resizing
* Main menu
* Tutorial mode
* Rules screen
* Replay system
* Undo system for development/debugging
* AI personality profiles

---

# Online Multiplayer

A future version could use:

`ASP.NET Core + SignalR`

to support real-time online games.

Possible architecture:

```text
Windows Forms Client
        |
        |
     SignalR
        |
        |
ASP.NET Core Server
        |
        |
Game Rooms / Match State
```

Players could then:

* Create rooms
* Join rooms
* Invite friends
* Play over the Internet
* Send messages
* Reconnect after disconnection

---

# AI Improvements

The current AI uses heuristics and rule-based decisions.

A more advanced version could calculate:

* Probability of opponents holding certain cards
* Known cards from the exchange
* Remaining bombs
* Remaining high cards
* Partner behavior
* Expected trick value
* Probability of finishing first
* Tichu success probability

More advanced AI approaches could include:

* Minimax
* Monte Carlo Tree Search
* Reinforcement Learning

---

# Educational Purpose

This project demonstrates several important programming concepts.

### Object-Oriented Programming

Using classes such as:

* Card
* Player
* Combination
* TichuGame

### Collections

Using:

```csharp
List<T>
HashSet<T>
Dictionary<TKey, TValue>
```

### LINQ

Used extensively for:

* Filtering cards
* Sorting cards
* Grouping ranks
* Finding combinations
* Calculating scores

### Events

The game engine communicates with the interface through events.

Example:

```csharp
public event Action<string>? Message;
```

### Event-Driven Programming

Windows Forms events are used for:

* Button clicks
* Card selection
* Dialogs
* Player actions

### Asynchronous Programming

Bot moves use asynchronous delays through:

```csharp
async
await
Task.Delay(...)
```

This prevents the interface from freezing while computer players are taking turns.

---

# Known Limitations

This project aims to provide a functional offline Tichu experience, but the AI is still heuristic rather than equivalent to an experienced human Tichu player.

Some rare rule interactions involving Phoenix, wishes, bombs and unusual combinations may require additional rule-specific handling if the project is intended to become a fully competitive or tournament-level Tichu implementation.

The graphical representation also uses custom Windows Forms controls rather than professional card artwork.

---

# Debugging

If the application does not start, verify that:

1. `.NET 8 SDK` is installed.
2. Visual Studio includes the:

`Desktop development with .NET`

workload.

3. The project target is:

```xml
<TargetFramework>net8.0-windows</TargetFramework>
```

4. Windows Forms is enabled:

```xml
<UseWindowsForms>true</UseWindowsForms>
```

---

# Build from Command Line

If the .NET SDK is installed, the project can also be built from a terminal.

Open a terminal inside the project folder and run:

```bash
dotnet restore
```

Then:

```bash
dotnet build
```

To run:

```bash
dotnet run
```

---

# Release Build

To create a Release build:

```bash
dotnet build -c Release
```

The compiled files will normally appear inside:

```text
bin/Release/net8.0-windows/
```

---

# Publishing

A standalone Windows build can be created with:

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

The output will appear inside the project's:

```text
bin/Release/net8.0-windows/win-x64/publish/
```

This can be used to distribute the game to another Windows computer.

---

# Summary

Tichu WinForms is a C# desktop card game implementing the core mechanics of Tichu.

The project includes:

* Four players
* Two teams
* 56-card deck
* Special cards
* Card combinations
* Initial three-card exchange
* Tichu
* Grand Tichu
* Mah Jong wishes
* Bombs
* Out-of-turn bombs
* Trick system
* Team scoring
* 1000-point matches
* Computer-controlled players
* Strategic bot behavior
* Interactive Windows Forms UI

The project can be used both as a playable game and as an educational example of building a larger event-driven application using C# and Windows Forms.
