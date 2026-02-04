# 16-Bit Mobile Game

A 16-bit inspired side-scrolling mobile game for iOS with procedurally generated levels and deterministic reconstruction via shareable level IDs.

---

## Core Concept

Players navigate side-scrolling levels with authentic 16-bit aesthetics. The game's differentiator is a **generative level system**: an algorithm that creates unique, playable levels from a deterministic seed. Each level has a unique ID that can be shared -- entering the same ID on any device regenerates the identical level.

```
Player generates level -> Gets ID: LVLID_1_3_1_A1B2C3D4E5F6G7H8
Player shares ID with friend -> Friend enters ID -> Identical level regenerated
Both play the same level -> Compare scores
```

This provides infinite replayability without a server, and enables community-driven content sharing.

---

## Technology Stack

| Component | Choice | Rationale |
|-----------|--------|-----------|
| Engine | Unity 2022 LTS | Best balance of 2D support, dev velocity, community resources ([decision](docs/Engine-Selection-Rubric.md)) |
| Language | C# | Unity native |
| PRNG | xorshift64* | Fast, deterministic, no platform-specific behavior |
| Generation | Hybrid (PRNG terrain + grammar pacing + constraint validation) | Natural layouts with guaranteed playability |
| iOS Target | iOS 15+ | Modern APIs, reasonable install base |
| Minimum Device | iPhone 11 | 60 fps baseline target |
| Audio | FMOD Studio | Professional audio integration |

---

## Performance Targets

Canonical targets defined in [Level-Generation-Technical-Spec.md](docs/Level-Generation-Technical-Spec.md):

| Metric | Target | Device |
|--------|--------|--------|
| Frame rate | 60 fps sustained | iPhone 11+ |
| Frame rate | 120 fps | iPhone 14 Pro+ |
| Level generation (avg) | 100 ms | iPhone 11 |
| Level generation (P95) | 150 ms | iPhone 11 |
| Memory (peak) | < 100 MB | All devices |
| Memory (per level) | < 2 MB | Resident |
| App launch to gameplay | < 3 seconds | All devices |

---

## Project Structure

```
README.md                           <- You are here
DOCUMENTATION-INDEX.md              <- Full document navigation
PROJECT-LAUNCH-CHECKLIST.md         <- Phase-by-phase checklist
docs/
  Training-Plan.md                  <- 10-module expert training curriculum
  Development-Roadmap.md            <- 16-week timeline, milestones
  Engine-Selection-Rubric.md        <- Engine evaluation (Unity selected)
  PROJECT-STRUCTURE.md              <- Directory layout, conventions
  Level-Generation-Technical-Spec.md <- Generation architecture (CRITICAL)
  Level-Generation-Research-Guide.md <- Algorithm research & code examples (CRITICAL)
  Validation-QA-Suite.md            <- 22+ test cases for gen system (CRITICAL)
  Module-1-Assessment-Criteria.md   <- 16-Bit Design Fundamentals
  Module-2-Assessment-Criteria.md   <- Mobile Game Development (iOS)
  Module-3-Assessment-Criteria.md   <- Game Design & Gamification
  Module-4-Assessment-Criteria.md   <- Side-Scrolling Mechanics
  Module-5-Assessment-Criteria.md   <- Mobile UX & Accessibility
  Module-6-Assessment-Criteria.md   <- 16-Bit Audio & Aesthetics
```

---

## Training Plan

The team completes a 10-module expert training program before development begins. Each module has hands-on assessments with rubrics and pass thresholds. See [Training-Plan.md](docs/Training-Plan.md) for the full curriculum.

| Module | Topic | Assessments |
|--------|-------|-------------|
| 1 | 16-Bit Design Fundamentals | 5 + capstone |
| 2 | Mobile Game Development (iOS) | 5 + capstone |
| 3 | Game Design & Gamification | 5 + capstone |
| 4 | Side-Scrolling Mechanics | 5 + capstone |
| 5 | Mobile UX & Accessibility | 5 + capstone |
| 6 | 16-Bit Audio & Aesthetics | 5 + capstone |
| 7-10 | iOS Pipeline, Performance, Market, Live Ops | Outlined in Training Plan |

---

## Development Timeline

See [Development-Roadmap.md](docs/Development-Roadmap.md) for full details.

```
Weeks 1-4:   Training & architecture planning
Weeks 5-8:   Core prototype (generation system, gameplay, controls)
Weeks 9-12:  Polish & integration (audio, visual, iOS features)
Weeks 13-15: QA & App Store preparation
Week 16+:    Launch & post-launch support
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

The core technical innovation. Full specification: [Level-Generation-Technical-Spec.md](docs/Level-Generation-Technical-Spec.md)

**Level ID format:** `LVLID_[VERSION]_[DIFFICULTY]_[BIOME]_[SEED64]`

**Pipeline:**
1. Seed derived from Level ID
2. xorshift64* PRNG initialized with seed (all randomness flows from here)
3. Macro layout: zones placed by grammar rules (intro, combat, challenge, boss)
4. Micro layout: terrain, enemies, rewards placed by PRNG within zone constraints
5. Validation: reachability verified, difficulty checked, safety constraints enforced
6. Level is playable -- same seed always produces the same level

**Critical constraint:** No Unity math functions (e.g., `Mathf.PerlinNoise`) are used in the generation pipeline. All randomness comes from the xorshift64* PRNG to guarantee cross-platform determinism.

---

## Status

**Current phase:** Planning (pre-development)
**Last updated:** 2026-02-04
