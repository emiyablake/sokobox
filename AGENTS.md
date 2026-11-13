# AGENTS.md — Sokobox

> Rules for AI agents (and humans) working on this project.
> Last updated: 2026-11-13.

## Context

2D pixel-art Sokoban puzzle. Migration of a web prototype (JS/HTML5 Canvas)
to Unity 6.6 (6000.6.9f1), template 2D (Core). Target platform: itch.io (WebGL).

## Sources of truth

* `GDD-dev.md` — design decisions and architecture. ALWAYS consult before
  implementing mechanics or changing behavior.
* `GDD-presentation.md` — public-facing vision of the game.
* Code and GDD disagreeing is a documentation bug: fix one of them, never
  ignore the mismatch.

Key design decisions already locked in (see GDD-dev):

* Moves counter is a score metric, NOT a limit. No game-over by moves.
* Per-level timer measures player completion time.
* Boxes on targets are locked (cannot be pushed off).
* End-of-level report: time + moves, then "next level or quit" choice.

## Language & communication

* ALL artifacts in English: code comments, commit messages, docs, descriptions.
* Comments should read like a developer explaining their own code to another
  developer — why, not what. No narrating the obvious.

## Code conventions

* C# naming: PascalCase for types and public members, camelCase for locals
  and parameters, `_camelCase` for private serialized fields.
* Game logic MUST be decoupled from UnityEngine where practical
  (pure C# core: GridSystem, LevelParser, game state) so it stays unit-testable.
* MonoBehaviours are thin adapters: input, rendering, UI.
* No dead/commented-out code in committed files. No `#region`.
* No new packages without asking first.
* No Asset Store assets without asking first.

## Project structure (Assets/)

* `Scripts/Core/` — pure game logic (no UnityEngine dependency)
* `Scripts/Unity/` — MonoBehaviours (input, visuals, UI glue)
* `Art/Sprites/`
* `Levels/` — level data (JSON / TextAsset)
* `Scenes/`, `Prefabs/`, `UI/`

## Agent workflow rules

* NEVER touch `Library/`, `Temp/`, `Logs/`, or `.git/` directly.
* Small, clearly-scoped fix or improvement: implement directly, then report.
* New feature or anything contradicting the GDD: propose a short plan first
  and wait for approval.
* Editor commands via Unity CLI: non-destructive operations are allowed
  freely (Play mode, read logs, run tests, screenshots). Anything destructive
  (deleting objects/assets, force-quitting, clean) requires OK.
* After changing code: report exactly what to validate in the Editor.

## Git workflow

* Feature branches named `feature/<slug>`, `refactor/<slug>`, `bug/<slug>`.
  Never commit directly to `main`.
* Commit messages follow Conventional Commits:
  `feat:`, `fix:`, `refactor:`, `art:`, `docs:`, `chore:` + concise description.
* Commits stay small and atomic.
* Agent waits for explicit approval before every push/commit.
  (Provisional rule — revisit once the pair-programming rhythm is proven.)

## Definition of Done

* Compiles with no new warnings.
* Level loads and is playable in the Editor.
* GDD updated if behavior changed.
* Changes approved and then commit.

