# Summit
### Make the maths work. Hit the target. Climb higher.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-Game-239120?style=for-the-badge&logo=csharp&logoColor=white)
![MonoGame](https://img.shields.io/badge/MonoGame-3.8-E73C00?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Final%20Submission-success?style=for-the-badge)

## What is it?

**Summit** is a mathematical deck-building roguelike built in C# with MonoGame.

Instead of making poker hands, you build equations. Each card changes your running total, and your goal is to finish the round as close as possible to the target score before you run out of hands.

This repository represents the final submitted version of the project, as sent to examiners for the Computing NEA.

## The Cards

Every suit is an operation:

| Suit | Operation | Example |
| --- | --- | --- |
| **Clubs** | Add | `+ value` |
| **Hearts** | Subtract | `- value` |
| **Spades** | Multiply | `× value` |
| **Diamonds** | Divide | `÷ value` |

Number cards use their value. Face cards are treated as 10 when scored.

## Gameplay

You get a deck, a target score, a limited number of hands, and a few discards.

- Pick up to **5 cards** to play.
- Reorder cards to control the calculation.
- Use discards to cycle bad cards.
- Hit the target score within the allowed range.
- Each round gets tighter as the allowed score range shrinks.

Miss the target when your hands run out, and it is game over.

## Scoring Modes

Summit supports different ways of resolving a hand:

| Mode | Does |
| --- | --- |
| **Left-to-Right** | Resolves cards in the order they are played. |
| **BIDMAS** | Resolves multiply/divide before add/subtract. |
| **Inverted** | Resolves add/subtract before multiply/divide. |

## Features

- Drag-and-drop card play.
- Animated card movement, flipping, and scoring.
- Random target scores and seeded rounds.
- Save/load game state.
- Discards, hands, score limits, and round progression.
- Music, sound effects, shaders, texture atlases, and custom UI.
- Built on **SummitKit**, a small MonoGame framework for input, UI, audio, scenes, physics, and rendering.
- Developer console commands for testing cards, scores, seeds, rounds, and resolve profiles.

## Project Structure

| Path | Contains |
| --- | --- |
| `Summit/` | The actual game. |
| `Summit/Card/` | Cards, suits, deck, hand logic, and card entities. |
| `Summit/Maths/` | Expression resolving and scoring logic. |
| `Summit/State/` | Game state, rounds, saving, scoring, and win/loss checks. |
| `Summit/Scenes/` | Scene setup and game screens. |
| `SummitKit/` | Reusable MonoGame framework code. |

## Running from Source

### Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/)
- Git
- Visual Studio 2022, Rider, or another C# IDE

### Clone and run

```bash
git clone https://github.com/duzos/Summit.git
cd Summit
dotnet restore
dotnet run --project Summit/Summit.csproj
```

The first build may take longer because MonoGame tools and packages need to restore.

## Building

```bash
dotnet build Summit.sln
```

## Submission Note

This is the final state of Summit as submitted for assessment. It is preserved as the completed NEA project rather than an actively developed prototype.

## Credits

Created by **Duzo**.

Built with C#, .NET, and MonoGame.
