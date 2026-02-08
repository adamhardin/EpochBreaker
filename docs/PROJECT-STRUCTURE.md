# Project Directory Structure & File Guide

**Last Updated**: 2026-02-08 | **Version**: 3.0 | **Status**: Active

---

## Overview

Epoch Breaker is a retro side-scrolling shooter built with Unity 6000.3.6f1, deployed as WebGL to GitHub Pages. All visual and audio assets are procedurally generated at runtime — there are no imported sprites, textures, or audio files.

---

## Project Locations

| Location | Purpose |
|----------|---------|
| `EpochBreaker/` | Full Unity project (opened in Unity Editor) |

---

## Current Structure (v1.4.2)

```
16 Bit Mobile Game/
├── README.md
├── DOCUMENTATION-INDEX.md                ← Index of all docs
├── BUILD-LOG.md                          ← WebGL deployment log
├── QC-CHECKLIST.md                       ← Systematic QC checklist (20 sections)
├── ROADMAP.md                            ← Development roadmap
├── AUDIO-ISSUE-REPORT.md                 ← Audio squeal analysis for external review
├── .gitignore
│
├── docs/                                 ← All documentation
│   ├── EXPERT-REVIEW-v1.0.2.md           (Expert review: 12 findings, competency scores)
│   ├── PROJECT-STRUCTURE.md              ← THIS FILE
│   ├── Level-Generation-Technical-Spec.md
│   ├── Level-Generation-Research-Guide.md
│   ├── Validation-QA-Suite.md
│   ├── architecture-decisions/           (ADR-001 through ADR-006)
│   └── archive/                          (Historical docs — .gitignored)
│
├── EpochBreaker/                         ← FULL UNITY PROJECT
│   ├── Assets/
│   │   ├── Scripts/                      ← ALL SOURCE CODE
│   │   │   ├── Generative/              (Pure C#, no Unity deps)
│   │   │   │   ├── LevelData.cs         (Tile types, zone types, level schema)
│   │   │   │   ├── LevelGenerator.cs    (Procedural level generation)
│   │   │   │   ├── LevelID.cs           (Level code system: E-XXXXXXXX)
│   │   │   │   ├── LevelValidator.cs    (Structural validation)
│   │   │   │   ├── XORShift64.cs        (Deterministic PRNG)
│   │   │   │   ├── DifficultyProfile.cs (Epoch-based difficulty scaling)
│   │   │   │   └── TutorialLevelBuilder.cs (3 hardcoded tutorial levels)
│   │   │   │
│   │   │   ├── Gameplay/                (Unity MonoBehaviours)
│   │   │   │   ├── GameManager.cs       (Singleton, game state, scoring)
│   │   │   │   ├── PlayerController.cs  (Movement, wall-slide, stomp)
│   │   │   │   ├── LevelLoader.cs       (Instantiates level from LevelData)
│   │   │   │   ├── TilemapRenderer.cs   (Renders tiles, destruction)
│   │   │   │   ├── CameraController.cs  (Follow cam, ortho size 7)
│   │   │   │   ├── InputManager.cs      (Input System wrapper)
│   │   │   │   ├── HealthSystem.cs      (Player HP, damage, respawn)
│   │   │   │   ├── HazardSystem.cs      (Environmental hazard effects)
│   │   │   │   ├── TutorialManager.cs   (Step-based tutorial progression)
│   │   │   │   ├── CheckpointManager.cs (Checkpoint tracking)
│   │   │   │   ├── CheckpointTrigger.cs (Checkpoint collision)
│   │   │   │   ├── AchievementManager.cs (30 achievements, persistence)
│   │   │   │   ├── AudioManager.cs      (Music + 8-channel SFX pool + 3 weapon sources)
│   │   │   │   ├── PlaceholderAssets.cs (Runtime sprite generation)
│   │   │   │   ├── PlaceholderAudio.cs  (Runtime audio synthesis)
│   │   │   │   ├── RewardPickup.cs      (Collectible items)
│   │   │   │   ├── ExtraLifePickup.cs   (1-up pickup)
│   │   │   │   ├── GoalTrigger.cs       (Level end trigger)
│   │   │   │   └── BuildInfo.cs         (Version/build tracking)
│   │   │   │
│   │   │   ├── Gameplay/Enemies/        (Enemy system)
│   │   │   │   ├── EnemyBase.cs         (4 behaviors, epoch scaling)
│   │   │   │   ├── Boss.cs              (3-phase boss, DPS cap)
│   │   │   │   └── BossArenaTrigger.cs  (Boss activation zone)
│   │   │   │
│   │   │   ├── Gameplay/Weapons/        (Weapon system)
│   │   │   │   ├── WeaponSystem.cs      (5-weapon, auto-select, cycling)
│   │   │   │   ├── WeaponData.cs        (Stats database, heat system)
│   │   │   │   ├── WeaponPickup.cs      (Weapon acquisition)
│   │   │   │   └── Projectile.cs        (Pierce, chain, heat)
│   │   │   │
│   │   │   ├── UI/                      (In Assembly-CSharp)
│   │   │   │   ├── TitleScreenUI.cs     (Main menu, settings, modes)
│   │   │   │   ├── GameplayHUD.cs       (HP, timer, weapon, relics)
│   │   │   │   ├── LevelCompleteUI.cs   (5-component score breakdown)
│   │   │   │   ├── PauseMenuUI.cs
│   │   │   │   ├── GameOverUI.cs
│   │   │   │   ├── TouchControlsUI.cs   (On-screen buttons)
│   │   │   │   ├── TutorialHintUI.cs    (Floating tutorial hints)
│   │   │   │   ├── AchievementsUI.cs    (Achievement display)
│   │   │   │   ├── CelebrationUI.cs     (Unlock celebrations)
│   │   │   │   └── LegendsUI.cs         (Leaderboard/legends)
│   │   │   │
│   │   │   ├── Editor/                  (Editor-only, auto-detected)
│   │   │   │   ├── ProjectSetup.cs      (Menu: Epoch Breaker > Setup)
│   │   │   │   ├── WebGLBuildScript.cs  (Headless WebGL builds)
│   │   │   │   └── QA/
│   │   │   │       ├── EditorQAWindow.cs
│   │   │   │       ├── EditorQASettings.cs
│   │   │   │       └── QAValidationRunner.cs
│   │   │   │
│   │   │   └── Tests/
│   │   │       ├── Editor/              (NUnit bridge wrappers)
│   │   │       │   ├── TestRunner.cs
│   │   │       │   ├── NUnitBridgeBase.cs
│   │   │       │   ├── XORShift64Tests.cs + Bridge
│   │   │       │   ├── LevelIDTests.cs + Bridge
│   │   │       │   └── LevelGeneratorTests.cs + Bridge
│   │   │       │
│   │   │       └── PlayMode/            (Runtime tests)
│   │   │           ├── HealthSystemTests.cs
│   │   │           ├── GameManagerTests.cs
│   │   │           ├── EnemyBaseTests.cs
│   │   │           ├── WeaponSystemTests.cs
│   │   │           ├── IntegrationTests.cs
│   │   │           └── TestHelpers.cs
│   │   │
│   │   ├── Resources/
│   │   │   └── EpochBreakerInput.inputactions (Input System asset)
│   │   │
│   │   ├── Plugins/WebGL/
│   │   │   └── ClipboardPlugin.jslib    (JS clipboard bridge)
│   │   │
│   │   ├── WebGLTemplates/EpochBreaker/
│   │   │   └── index.html               (Custom WebGL template)
│   │   │
│   │   └── Scenes/
│   │       └── Bootstrap.unity           (Single scene, all runtime)
│   │
│   ├── Packages/
│   │   └── manifest.json                (URP, 2D, Input System, TMP, Test Framework)
│   │
│   └── ProjectSettings/                 (Unity project settings)
```

---

## Assembly Structure

| Assembly | Directory | References | Notes |
|----------|-----------|------------|-------|
| `EpochBreaker.Generative` | Scripts/Generative/ | None | `noEngineReferences: true` |
| `EpochBreaker.Gameplay` | Scripts/Gameplay/ | Generative | Unity MonoBehaviours |
| Assembly-CSharp | Scripts/UI/ | Gameplay (auto) | No asmdef, default assembly |
| `EpochBreaker.Tests.Editor` | Scripts/Tests/Editor/ | Generative, NUnit | Editor-only |
| `EpochBreaker.Tests.PlayMode` | Scripts/Tests/PlayMode/ | Gameplay, Generative, TestRunner | Cross-platform |

---

## Coordinate System

LevelData and Unity use **inverted Y axes**. This is the most common source of bugs in level tooling.

```
LevelData (generation):         Unity (rendering):
y=0  ████████ TOP (sky)         y=15 ████████ TOP (sky)
y=1  ████████                   y=14 ████████
 .       .                       .       .
 .       .                       .       .
y=14 ▓▓▓▓▓▓▓▓                  y=1  ▓▓▓▓▓▓▓▓
y=15 ▓▓▓▓▓▓▓▓ BOTTOM (ground)  y=0  ▓▓▓▓▓▓▓▓ BOTTOM (ground)
```

**Conversion formula**: `unityY = (HeightTiles - 1) - dataY`

**Tile array indexing**: `Tiles[dataY * WidthTiles + x]` (row-major, LevelData coordinates)

### Conversion Checklist

When writing code that bridges generation and rendering:

- [ ] Am I reading from `LevelData`? Use `dataY` directly for array access
- [ ] Am I placing a Unity GameObject? Convert: `unityY = (HeightTiles - 1) - dataY`
- [ ] Am I reading a Unity position back to LevelData? Convert: `dataY = (HeightTiles - 1) - unityY`
- [ ] Ground height range: data y=10-14 maps to Unity y=1-5

---

## Key Design Notes

- **Engine**: Unity 6000.3.6f1
- **No imported assets**: All sprites and audio are generated at runtime via `PlaceholderAssets.cs` and `PlaceholderAudio.cs`
- **Single scene**: Everything runs from `Bootstrap.unity`. UI and game objects are created programmatically
- **UI viewport bounds**: Reference resolution 1920x1080, CanvasScaler matchWidthOrHeight=0.5. Viewport limits are **±540 vertical / ±960 horizontal** from center. Safe area (with 60px margin for badges/overflow) is **±480 vertical / ±900 horizontal**. When repositioning any UI container, verify `anchoredPosition.Y + height/2 ≤ 480`. This is a recurring source of bugs.
- **Audio**: All audio is synthesized at runtime via `AudioClip.Create()` using procedural waveforms (PolyBLEP anti-aliased). No FMOD or imported audio files.
- **GameManager reflection**: Uses reflection for UI `AddComponent` calls to avoid circular dependency between Gameplay and UI assemblies
- **WebGL deployment**: Manual build via `WebGLBuildScript.Build`, output pushed to `gh-pages` branch for GitHub Pages

---

**Version**: 3.0
**Last Updated**: 2026-02-08
**Status**: Active
