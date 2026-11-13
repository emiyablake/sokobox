# Sokobox

A 2D pixel-art Sokoban puzzle built in Unity. Pilot a small ship through
space-station warehouses and push every crate onto its delivery point —
planning every step, because you can only push, never pull.

This project is a migration of a web prototype (vanilla JS + HTML5 Canvas)
to Unity 6, targeting an itch.io release (WebGL).

## How to play

- **Move:** Arrow keys or WASD — one cell per step
- **Goal:** every crate on a delivery target
- **Rules:** push only, no pulling. Crates settled on a target lock in place.

## Tech

- **Unity 6.6** (6000.6 LTS), 2D (Core) template, URP
- **Input System** for keyboard input
- **Unity Pipeline package** for editor automation (CLI-driven development)
- Levels defined as ASCII maps in JSON — no scene-per-level

## Architecture

Game rules live in a pure C# core with no UnityEngine dependency, so the
entire ruleset runs under EditMode unit tests without booting the engine.

```
Assets/
├── Scripts/
│   ├── Core/        # Sokobox.Core asmdef — pure game logic
│   │   ├── GridPosition.cs   # cell coordinates
│   │   ├── Direction.cs      # movement deltas
│   │   ├── LevelData.cs      # immutable level layout
│   │   ├── LevelParser.cs    # ASCII map -> LevelData
│   │   └── SokobanGrid.cs    # movement, pushing, locked crates, win check
│   └── Unity/       # MonoBehaviour adapters (thin)
│       ├── GameController.cs # input, level flow
│       └── BoardView.cs      # sprite rendering, camera framing
├── Tests/EditMode/  # NUnit tests over the core rules
├── Art/Sprites/     # original pixel art (Point filter, 40 PPU)
└── Resources/Levels/levels.json
```

Design documents live in `Docs/` (`GDD-dev.md` for technical/design
decisions, `GDD-presentation.md` for the public-facing vision) and
agent/workflow rules in `AGENTS.md`.

## Development

- Branch naming: `feature/`, `refactor/`, `bug/` — never commit to `main`
- Conventional Commits
- Definition of done: compiles clean, EditMode tests pass, playable in Editor

## Roadmap

- [x] Core rules with locked crates + unit tests
- [x] Sprite board rendering with camera framing
- [ ] HUD (level, moves, timer) and end-of-level report screen
- [ ] Undo and per-level restart
- [ ] More levels
- [ ] WebGL build for itch.io

## Credits

Art, design, and code: built pixel by pixel. Sprites are original work
and not licensed for reuse.
