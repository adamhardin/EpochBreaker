# Project Directory Structure & File Guide

**Last Updated**: 2026-02-06 | **Version**: 2.0 | **Status**: Active

---

## Overview

Epoch Breaker is a retro side-scrolling mobile shooter built with Unity 2022.3 LTS. All visual and audio assets are procedurally generated at runtime — there are no imported sprites, textures, or audio files.

---

## Project Locations

| Location | Purpose |
|----------|---------|
| `EpochBreaker/` | Full Unity project (opened in Unity Editor) |
| `unity_project/` | Source files only (Assets + Packages), no Unity metadata |

Both locations must stay in sync. When editing scripts, update both.

---

## Current Structure (v0.3.0)

```
16 Bit Mobile Game/
├── README.md
├── GAME-DESIGN-REVIEW.md                 ← Expert review document
├── DOCUMENTATION-INDEX.md                ← Index of all docs
├── BUILD-LOG.md                          ← WebGL deployment log
├── PROJECT-LAUNCH-CHECKLIST.md
├── .gitignore
│
├── .github/
│   └── workflows/
│       └── deploy-webgl.yml              (GitHub Actions: WebGL → GitHub Pages)
│
├── docs/                                 ← All documentation
│   ├── Expert-Review-Report.md           (Latest expert review findings)
│   ├── Expert-Review-Change-Log.md       (Changes from review sprints)
│   ├── Development-Review-Workflow.md    (Review cadence and roles)
│   ├── Expert-Review-*.md                (Review process documents)
│   ├── Development-Roadmap.md            (Timeline, milestones)
│   ├── PROJECT-STRUCTURE.md              ← THIS FILE
│   ├── Training-Plan.md                  (Training curriculum)
│   ├── Level-Generation-Technical-Spec.md
│   ├── Level-Generation-Research-Guide.md
│   ├── Validation-QA-Suite.md
│   └── Module-*-Assessment-Criteria.md   (6 training modules)
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
│   │   │   │   ├── AudioManager.cs      (Music + 8-channel SFX pool)
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
│   │   │   │   ├── WeaponSystem.cs      (6-slot, auto-select, cycling)
│   │   │   │   ├── WeaponData.cs        (Stats database, heat system)
│   │   │   │   ├── WeaponPickup.cs      (Weapon acquisition)
│   │   │   │   └── Projectile.cs        (Pierce, chain, slow, heat)
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
│
└── unity_project/                        ← SOURCE FILES MIRROR
    ├── Assets/                           (Same Scripts/ structure as above)
    └── Packages/
        └── manifest.json
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

## Dual-Project Sync Workflow

The project exists in two locations. **Both must stay in sync.**

| Location | Contains | Used For |
|----------|----------|----------|
| `EpochBreaker/` | Full Unity project (Scripts + ProjectSettings + Library) | Opening in Unity Editor, building |
| `unity_project/` | Source files only (Assets/Scripts + Packages) | Git-friendly mirror, no Unity metadata |

### Sync Procedure

After editing any `.cs` file in `EpochBreaker/Assets/Scripts/`:

```bash
# From the project root (16 Bit Mobile Game/)
cp -R EpochBreaker/Assets/Scripts/ unity_project/Assets/Scripts/
```

### Checklist Before Committing

- [ ] All modified `.cs` files exist in both `EpochBreaker/Assets/Scripts/` and `unity_project/Assets/Scripts/`
- [ ] No Unity metadata (`.meta`, `Library/`, `Temp/`) in `unity_project/`
- [ ] `Packages/manifest.json` matches in both locations

### When Sync Is NOT Needed

- Changes to `ProjectSettings/`, `Scenes/`, `Resources/`, `Plugins/`, `WebGLTemplates/` — these only exist in `EpochBreaker/`
- Documentation files at the project root or in `docs/`

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

- **Engine**: Unity 6000.3.6f1 (upgraded from 2022.3 LTS)
- **No imported assets**: All sprites and audio are generated at runtime via `PlaceholderAssets.cs` and `PlaceholderAudio.cs`
- **Single scene**: Everything runs from `Bootstrap.unity`. UI and game objects are created programmatically
- **GameManager reflection**: Uses reflection for UI `AddComponent` calls to avoid circular dependency between Gameplay and UI assemblies
- **WebGL deployment**: GitHub Actions builds and deploys to GitHub Pages automatically

---

**Version**: 2.1
**Last Updated**: 2026-02-06
**Status**: Active
