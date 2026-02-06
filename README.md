# Epoch Breaker

A retro side-scrolling mobile shooter for iOS where a small character collects weapon attachments that auto-fire from their body, blasting through destructible environments that evolve across 10 eras of human civilization.

---

## Core Concept

Players control a small character who picks up weapon attachments throughout each level. These weapons continuously auto-fire from the character's body, targeting enemies and destructible terrain. The player blasts through walls, rocks, and obstacles to carve pathways, reveal bridges, uncover stairs, and progress through levels. Strategic destruction is key -- leaving certain material intact creates platforms, cover, and structural supports that aid traversal.

```
Pick up weapons -> Weapons auto-fire -> Blast environment -> Reveal paths -> Progress
                                      -> Destroy enemies  -> Earn drops   -> More weapons
```

The game's world evolves through 10 eras of human civilization, from Stone Age caves to transcendent future landscapes. The character, weapons, enemies, and destructible materials all transform to match each era.

**Differentiator:** Procedurally generated levels via deterministic seeds. Each level has a unique ID that can be shared -- entering the same ID on any device regenerates the identical level.

```
Player generates level -> Gets ID: LVLID_1_3_1_A1B2C3D4E5F6G7H8
Player shares ID with friend -> Friend enters ID -> Identical level regenerated
Both play the same level -> Compare scores
```

---

## Gameplay Pillars

### 1. Weapon Attachment System
- Character picks up weapon attachments scattered through levels
- Weapons mount visually onto the character's body and auto-fire continuously
- Auto-aim targets nearest enemy; player can cycle targets with a button
- More attachments = more simultaneous firepower
- Weapons evolve thematically per era (rocks -> arrows -> bullets -> lasers -> nanobots)

### 2. Destructible Environment
- Walls, rocks, rubble, and terrain can be blasted apart
- Destruction reveals pathways, bridges, stairs, and hidden areas
- Strategic element: preserving material creates platforms, cover, and traversal aids
- Puzzle-like level design rewards thinking about what to destroy vs. preserve

### 3. Era Progression (10 Eras)
Character and world evolve through all of human civilization:

| Era | Theme | Setting | Weapon Style |
|-----|-------|---------|-------------|
| 1 | Stone Age | Caves, wilderness | Rocks, clubs, slings |
| 2 | Bronze Age | Early settlements, mud brick | Bronze spears, fire |
| 3 | Iron Age | Fortifications, forges | Iron weapons, siege tools |
| 4 | Medieval | Castles, villages | Crossbows, catapults |
| 5 | Renaissance | Ornate cities, workshops | Gunpowder, early firearms |
| 6 | Industrial | Factories, railways | Steam weapons, explosives |
| 7 | Modern | Urban, military | Ballistics, rockets |
| 8 | Digital | Data centers, networks | Lasers, drones, EMP |
| 9 | Space Age | Orbital, alien worlds | Energy weapons, plasma |
| 10 | Transcendent | Reality-bending, exotic | Nanobots, antimatter, singularity |

### 4. Controls (Two-Thumb Mobile Layout)
- **Left thumb:** Virtual D-pad for movement (left/right)
- **Right thumb:** Jump button + Target cycle button
- **Hold jump when powered up:** Unleashes special attack

---

## Technology Stack

| Component | Choice | Rationale |
|-----------|--------|-----------|
| Engine | Unity 2022 LTS | Best balance of 2D support, dev velocity, community resources |
| Language | C# | Unity native |
| PRNG | xorshift64* | Fast, deterministic, no platform-specific behavior |
| Generation | Hybrid (PRNG terrain + grammar pacing + constraint validation) | Natural layouts with guaranteed playability |
| iOS Target | iOS 15+ | Modern APIs, reasonable install base |
| Minimum Device | iPhone 11 | 60 fps baseline target |
| Audio | FMOD Studio | Professional audio integration |

---

## Performance Targets

| Metric | Target | Device |
|--------|--------|--------|
| Frame rate | 60 fps sustained | iPhone 11+ |
| Frame rate | 120 fps | iPhone 14 Pro+ |
| Level generation (avg) | 100 ms | iPhone 11 |
| Level generation (P95) | 150 ms | iPhone 11 |
| Memory (peak) | < 100 MB | All devices |
| Memory (per level) | < 2 MB | Resident |
| App launch to gameplay | < 3 seconds | All devices |
| Destructible tiles per level | Up to 500 | All devices |
| Simultaneous projectiles | Up to 30 | All devices |

---

## Project Structure

```
README.md                           <- You are here
DOCUMENTATION-INDEX.md              <- Full document navigation
PROJECT-LAUNCH-CHECKLIST.md         <- Phase-by-phase checklist
docs/
  Training-Plan.md                  <- 10-module expert training curriculum
  Development-Roadmap.md            <- Timeline, milestones
  Engine-Selection-Rubric.md        <- Engine evaluation (Unity selected)
  PROJECT-STRUCTURE.md              <- Directory layout, conventions
  Level-Generation-Technical-Spec.md <- Generation architecture (CRITICAL)
  Level-Generation-Research-Guide.md <- Algorithm research & code examples
  Validation-QA-Suite.md            <- Test cases for gen system
training/
  module3/                          <- Game Design & Gamification
    progression_system_design.md    <- 10-era progression system
    monetization_strategy.md
    retention_features_spec.md
    difficulty_curve_spec.md
    reward_system_spec.md
  module4/                          <- Core Mechanics
    combat_system_spec.md           <- Weapon attachment & auto-fire system
    enemy_archetypes.md             <- Era-based enemy designs
    boss_patterns.md                <- Era-based boss encounters
    hazard_catalog.md               <- Destructible environment system
    physics_spec.md
    camera_system_spec.md
  module5/                          <- Mobile UX
    control_layout_spec.md          <- Two-thumb D-pad + buttons layout
    ui_layout_spec.md
    onboarding_flow.md
    accessibility_spec.md
    interruption_handling_spec.md
  module6/                          <- Art & Audio
    tileset_spec.md                 <- Era-based visual themes
    animation_spec.md
    music_spec.md                   <- Era-evolving soundtrack
    sfx_spec.md                     <- Weapon & destruction SFX
    parallax_spec.md
```

---

## Key Design Targets

| Area | Target |
|------|--------|
| Difficulty balance | Calculated vs. target within +/- 1.0 point |
| Reachability | 100% of generated levels are completable |
| Level duration | 2-5 minutes per level |
| Day-1 retention | >= 40% |
| Day-7 retention | >= 20% |
| User rating | >= 4.0 stars |
| Code coverage | >= 80% for critical systems |

---

## Generative Level System

Full specification: [Level-Generation-Technical-Spec.md](docs/Level-Generation-Technical-Spec.md)

**Level ID format:** `LVLID_[VERSION]_[DIFFICULTY]_[ERA]_[SEED64]`

**Pipeline:**
1. Seed derived from Level ID
2. xorshift64* PRNG initialized with seed (all randomness flows from here)
3. Macro layout: zones placed by grammar rules (intro, traversal, combat, puzzle, boss)
4. Destructible terrain: walls, rocks, and breakable material placed with embedded pathways
5. Weapons and enemies placed by PRNG within zone constraints
6. Validation: reachability verified with minimum weapon loadout, difficulty checked
7. Level is playable -- same seed always produces the same level

**Critical constraint:** No Unity math functions (e.g., `Mathf.PerlinNoise`) in the generation pipeline. All randomness comes from xorshift64* PRNG for cross-platform determinism.

---

## Play in Browser

**Live demo:** [https://adamhardin.github.io/EpochBreaker/](https://adamhardin.github.io/EpochBreaker/)

The game runs as a WebGL build hosted on GitHub Pages. Keyboard controls: WASD/arrows to move, Space to jump, Z to stomp, X to attack, Escape to pause.

### How the browser build works

1. The Unity project is built for WebGL locally using a custom build script ([WebGLBuildScript.cs](EpochBreaker/Assets/Scripts/Editor/WebGLBuildScript.cs))
2. The build output is pushed to the `gh-pages` branch
3. A GitHub Actions workflow ([deploy-webgl.yml](.github/workflows/deploy-webgl.yml)) deploys the `gh-pages` branch to GitHub Pages

### Deploying an update

```bash
# 1. Build WebGL from command line (or use Unity > File > Build)
/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath EpochBreaker \
  -executeMethod EpochBreaker.Editor.WebGLBuildScript.Build \
  -logFile -

# 2. Push build output to gh-pages
git checkout gh-pages
cp -r EpochBreaker/build/WebGL/WebGL/* .
git add -A
git commit -m "Update WebGL build"
git push origin gh-pages
git checkout master
```

The GitHub Actions workflow automatically deploys whenever the `gh-pages` branch is updated.

### WebGL-specific adaptations

- **Clipboard:** Uses a JavaScript bridge plugin ([ClipboardPlugin.jslib](EpochBreaker/Assets/Plugins/WebGL/ClipboardPlugin.jslib)) since `GUIUtility.systemCopyBuffer` isn't available on WebGL
- **Paste disabled:** Browser security prevents synchronous clipboard reads, so the paste button is hidden on WebGL
- **Custom template:** A landscape 16:9 template ([index.html](EpochBreaker/Assets/WebGLTemplates/EpochBreaker/index.html)) with dark background, loading bar, and responsive sizing
- **Compression:** Brotli with decompression fallback (GitHub Pages doesn't serve `.br` content-encoding headers)
- **Memory:** 64 MB initial, 512 MB max, geometric growth
- **Audio:** All audio is generated at runtime via `AudioClip.Create()` — no imported audio files to load
- **Persistence:** PlayerPrefs (achievements, level history, saved sessions) are backed by IndexedDB on WebGL

---

## Status

**Current phase:** Playable prototype with 10 eras, 3 game modes, procedural levels
**Last updated:** 2026-02-05
