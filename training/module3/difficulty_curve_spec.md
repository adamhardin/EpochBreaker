# Assessment 3.2: Difficulty Curve Design

## Overview

Complete 20-level difficulty curve specification for the first two eras (Stone Age and Bronze Age) of the 16-bit side-scrolling shooter mobile game. This document defines four difficulty dimensions, assigns per-level composite scores on a 1-10 scale, documents every teaching moment, and specifies the escalation and climax sequences. The curve is designed for the procedural generation system to use as parameter constraints per level.

---

## 1. Difficulty Dimensions

### Definition of Dimensions

Each level's difficulty is decomposed into four orthogonal dimensions. Every dimension is scored independently on a 1-10 scale, then combined into a composite score using weighted averaging.

| Dimension | Code | Weight | Description |
|-----------|------|--------|-------------|
| Traversal | T | 0.30 | Platform gap width, moving platform speed, vertical complexity, fall death risk |
| Destruction | D | 0.35 | Wall density, material hardness, structural puzzle complexity, debris hazards |
| Timing | M | 0.20 | Precision windows for jumps, weapon pickup timing, pattern synchronization |
| Resource | R | 0.15 | Health potion scarcity, weapon attachment availability, checkpoint spacing |

**Composite Difficulty Formula**:
```
D_composite = (T * 0.30) + (D * 0.35) + (M * 0.20) + (R * 0.15)
```

### Dimension Scoring Rubric

**Traversal (T)**:
| Score | Gap Width | Platform Speed | Vertical Layers | Fall Risk |
|-------|-----------|---------------|-----------------|-----------|
| 1 | 0-1 tiles | None | 1 layer (flat) | None |
| 2 | 1-2 tiles | None | 1 layer | Minimal (short falls) |
| 3 | 2-3 tiles | Slow (1 tile/s) | 1-2 layers | Low |
| 4 | 3 tiles | Slow | 2 layers | Moderate |
| 5 | 3-4 tiles | Medium (2 tiles/s) | 2 layers | Moderate |
| 6 | 4 tiles | Medium | 2-3 layers | Significant |
| 7 | 4-5 tiles | Fast (3 tiles/s) | 3 layers | High |
| 8 | 5 tiles | Fast | 3 layers | High |
| 9 | 5-6 tiles (requires dash) | Very fast (4 tiles/s) | 3+ layers | Very high |
| 10 | 6+ tiles (requires dash+ability) | Erratic patterns | 4 layers | Extreme |

**Destruction (D)**:
| Score | Wall Density | Material Hardness | Structural Complexity | Debris Hazards |
|-------|-------------|------------------|----------------------|----------------|
| 1 | 0 walls | N/A | N/A | None |
| 2 | 1-3 walls (soft clay) | 1 hit to destroy | Single wall, clear path behind | None |
| 3 | 4-6 walls (soft clay) | 1-2 hits | Linear walls, obvious destruction targets | Minimal |
| 4 | 6-8 walls (mixed) | 2 hits avg | Some walls hide secrets, basic branching | Light debris |
| 5 | 8-12 walls (mixed clay/stone) | 2-3 hits | Multi-path options, wall chains | Moderate debris |
| 6 | 12-16 walls (stone) | 3 hits avg | Structural puzzles (destroy support = chain collapse) | Falling debris |
| 7 | 16-20 walls (stone/metal) | 3-4 hits | Complex chain reactions, timing-dependent destruction | Significant debris |
| 8 | 20-25 walls (metal) | 4 hits | Multi-layer walls, load-bearing structures | Heavy debris + environmental damage |
| 9 | 25+ walls (reinforced) | 4-5 hits | Puzzle-destruction (specific order required) | Debris as primary hazard |
| 10 | Dense destructible environment | 5+ hits (requires special weapons) | Full structural puzzles, environmental chain reactions | Debris everywhere, constant threat |

**Timing (M)**:
| Score | Jump Windows | Weapon Pickup Windows | Pattern Sync |
|-------|-------------|----------------------|-------------|
| 1 | Unlimited (static platforms) | N/A | None |
| 2 | 3+ seconds | Weapon drops persist 10+ seconds | None |
| 3 | 2-3 seconds | Weapon drops persist 8 seconds | Simple (1 pattern) |
| 4 | 2 seconds | Weapon drops persist 6 seconds | Simple |
| 5 | 1.5 seconds | Weapon drops persist 5 seconds | 2 overlapping patterns |
| 6 | 1 second | Weapon drops persist 4 seconds | 2-3 overlapping |
| 7 | 0.75 seconds | Weapon drops persist 3 seconds | 3 overlapping |
| 8 | 0.5 seconds | Weapon drops persist 2 seconds | Complex multi-pattern |
| 9 | 0.3 seconds | Weapon drops persist 1.5 seconds | Near-simultaneous |
| 10 | Frame-perfect (<0.2s) | Weapon drops persist 1 second | Full chaos |

**Resource (R)** (inverted: higher score = fewer resources):
| Score | Health Potions per Section | Checkpoints | Weapon Attachments Available |
|-------|--------------------------|-------------|---------------------------|
| 1 | Abundant (1 per 10 tiles) | Every 20 tiles | 3+ per level |
| 2 | Frequent (1 per 15 tiles) | Every 25 tiles | 2-3 per level |
| 3 | Regular (1 per 20 tiles) | Every 30 tiles | 2 per level |
| 4 | Moderate (1 per 25 tiles) | Every 35 tiles | 1-2 per level |
| 5 | Sparse (1 per 30 tiles) | Every 40 tiles | 1 per level |
| 6 | Rare (1 per 40 tiles) | Every 50 tiles | 1 per level |
| 7 | Very rare (1 per 50 tiles) | Every 60 tiles | 0-1 per level |
| 8 | Scarce (1 per 60 tiles) | Every 70 tiles | 0 per level |
| 9 | Nearly absent (1-2 total) | Start + midpoint only | 0 per level |
| 10 | None | Start only | 0 per level |

---

## 2. Complete 20-Level Difficulty Breakdown

### Phase 1: Teaching Sequence (Levels 1-5)

#### Level 1: "First Steps" -- Learn to Move
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 1.0 | Flat terrain, single 1-tile gap, no fall deaths |
| Destruction (D) | 1.0 | Zero destructible walls (movement-only tutorial) |
| Timing (M) | 1.0 | No timing requirements, all static |
| Resource (R) | 1.0 | Coins everywhere, impossible to fail |
| **Composite** | **1.0** | Pure movement tutorial |

**Level parameters**:
- Width: 128 tiles (short)
- Gaps: One 1-tile gap at tile 64 (gap has floor below, no death possible)
- Walls: 0 destructible
- Enemies: 0
- Pickups: 15 coins, 2 health potions (even though no damage source exists)
- Checkpoints: Start + end
- Par time: 60 seconds
- New mechanic taught: **Move** (left D-pad) and **Jump** (right thumb button)

**Teaching moment**: On-screen arrow overlay shows D-pad for movement. At the 1-tile gap, a jump prompt appears. Player cannot proceed without jumping. Success plays a celebratory chime. No weapons appear yet -- player learns pure movement first.

---

#### Level 2: "Breaking Ground" -- Learn to Destroy
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 1.5 | Mostly flat, two 1-tile gaps |
| Destruction (D) | 2.0 | 2 soft clay walls blocking the path, 1 hit each |
| Timing (M) | 1.5 | Walls are static, no time pressure |
| Resource (R) | 1.0 | Health potion after each wall section, first weapon drop |
| **Composite** | **1.6** | First destruction introduction |

**Level parameters**:
- Width: 160 tiles
- Gaps: Two 1-tile gaps (tiles 50, 120)
- Walls: 2 soft clay walls (1 HP each, blocking path at tiles 70 and 100)
- Enemies: 0 (destruction-only level)
- Pickups: 20 coins, 3 health potions, 1 weapon attachment drop (basic auto-fire blaster)
- Checkpoints: Start, midpoint (tile 80), end
- Par time: 75 seconds
- New mechanic taught: **Weapon Pickup** (walk over weapon drop) and **Auto-Fire** (weapon fires automatically at nearest target/wall)

**Teaching moment**: First wall blocks the only path forward. A weapon attachment drop sits 5 tiles before the wall, glowing and pulsing. A prompt says "WALK OVER TO EQUIP." Once equipped, the auto-fire blaster immediately targets and destroys the clay wall. Debris crumbles satisfyingly, revealing coins behind. The player learns: find weapon, weapon fires automatically, walls break, rewards appear.

---

#### Level 3: "Target Practice" -- Target Cycling + Destruction
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 2.0 | Two 2-tile gaps, one elevated platform section |
| Destruction (D) | 2.5 | 4 walls, some hiding secrets, first enemy appears behind wall |
| Timing (M) | 2.0 | Must cycle targets between wall and enemy, but window is generous (2s+) |
| Resource (R) | 1.5 | Health potion placed before the multi-target section |
| **Composite** | **2.1** | First combination challenge |

**Level parameters**:
- Width: 180 tiles
- Gaps: Two 2-tile gaps (tiles 60, 130), elevated platform section (tiles 90-110)
- Walls: 4 soft clay walls -- one blocking main path, one hiding secret area, two creating a destructible corridor
- Enemies: 2 Chargers (basic enemy, walks toward player, 1 HP) -- first one appears after a wall is destroyed
- Pickups: 20 coins, 2 health potions (tiles 55 and 125), 1 weapon attachment
- Checkpoints: Start, tile 80, end
- Par time: 90 seconds
- New mechanic taught: **Target Cycle** (right thumb button to switch auto-fire target between walls and enemies)

**Teaching moment**: The critical learning moment is when the player destroys a wall and reveals a Charger enemy behind it. The auto-fire weapon is targeting the next wall, but the Charger approaches. A prompt highlights the Target Cycle button: "TAP TO SWITCH TARGETS." The player learns to cycle between environmental and enemy targets. Health potion placed before ensures the player is at full HP for this first real challenge.

---

#### Level 4: "Shifting Ground" -- Moving Platforms + Weapon Variety
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 3.0 | Moving platforms (horizontal, 1 tile/s), 3-tile gap |
| Destruction (D) | 2.5 | 5 walls, first stone wall (2 HP) appears |
| Timing (M) | 3.0 | Must time jump onto moving platform (2s window) |
| Resource (R) | 2.0 | Moderate potion placement, second weapon type introduced |
| **Composite** | **2.7** | New hazard type + new weapon type introduction |

**Level parameters**:
- Width: 200 tiles
- Gaps: One 3-tile gap with moving platform (horizontal, period: 4 seconds)
- Moving platforms: 2 total -- one over safe ground (practice), one over pit (real)
- Walls: 5 total -- 3 soft clay (1 HP), 2 stone (2 HP, first appearance)
- Enemies: 2 Chargers + 1 Shooter (first appearance, stationary, fires slow projectile, 2 HP)
- Pickups: 18 coins, 2 health potions, 1 Scatter Shot weapon attachment (first appearance)
- Checkpoints: Start, tile 70 (after practice platform), tile 150, end
- Par time: 105 seconds
- New mechanic taught: **Moving Platforms**, **Stone Walls** (harder material), and **Scatter Shot weapon**

**Teaching moment**: First moving platform is placed over solid ground -- if the player mistimes the jump, they land on the floor below (no damage, no death, just walk back and retry). The Scatter Shot weapon is placed before a section with 3 walls in a cluster, showing how it destroys multiple tiles at once. The Shooter enemy appears on flat ground far from any gaps, so the player can focus on learning to cycle targets to it.

---

#### Level 5: "Rite of Passage" -- Skill Gate (Era 1 Test)
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 3.5 | Mixed gaps (2-3 tiles), one moving platform, elevated sections |
| Destruction (D) | 3.0 | 8 walls of mixed materials, first structural chain (destroy support wall to collapse upper section) |
| Timing (M) | 3.0 | Moving platform + target cycling under mild pressure |
| Resource (R) | 2.5 | Reduced potions, checkpoints every 50 tiles |
| **Composite** | **3.0** | Skill gate: tests all Era 1 mechanics |

**Level parameters**:
- Width: 220 tiles
- Gaps: Mix of 2-tile and 3-tile gaps, one requiring newly unlocked Dash
- Moving platforms: 2 (one horizontal, one vertical)
- Walls: 8 total (4 clay, 3 stone, 1 chain-reaction wall that drops a ceiling section)
- Enemies: 3 Chargers + 2 Shooters, including one Shooter behind a destructible wall
- Pickups: 15 coins, 2 health potions, Dash ability unlock at level start, 1 weapon attachment
- Checkpoints: Start, tile 55, tile 110, tile 165, end
- Par time: 120 seconds
- New mechanic taught: **Dash** (unlocked at the start of this level)

**Teaching moment**: Level begins with a dedicated Dash tutorial section. A 4-tile gap (too wide for normal jump) blocks the path. A prompt says "DOUBLE-TAP to DASH." The gap has spikes above it -- when the player dashes through, the i-frames protect them, teaching dash invulnerability. The chain-reaction wall is introduced with visual cues (cracks on the support, ceiling visibly resting on it). The rest of the level tests movement, jump, weapon pickup, target cycling, destruction, and dash in combination, serving as the gateway to Era 2.

---

### Phase 2: Escalation Sequence (Levels 6-15)

#### Level 6: "Mammoth Grounds"
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 3.5 | Vertical exploration added, hidden upper paths through breakable ceilings |
| Destruction (D) | 3.5 | 10 walls, first hidden destruction paths reveal shortcuts |
| Timing (M) | 3.0 | Target cycling windows shrink to 1.5s |
| Resource (R) | 2.5 | Weapon attachments placed on risk/reward paths |
| **Composite** | **3.2** | Post-skill-gate escalation begins |

- New element: **Spike Traps** (static floor hazards, 1 damage on contact)
- Spikes are visually distinct (red, animated shimmer) and placed on wide platforms with clear borders
- Vertical platforming introduces optional upper path with bonus coins accessible by destroying ceiling tiles
- **Rest beat**: Short flat section after the spike introduction with easy Charger enemies

---

#### Level 7: "Bone Corridor"
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 4.0 | Spike traps on platforms, 3-tile gaps standard |
| Destruction (D) | 3.5 | 12 walls, first walls that hide enemy ambushes |
| Timing (M) | 3.5 | Must prioritize targets quickly: wall hiding enemy vs approaching Charger |
| Resource (R) | 3.0 | Weapon drops every 30 tiles, checkpoints every 55 tiles |
| **Composite** | **3.6** | Destruction becomes tactical (choosing what to destroy first) |

- New element: **Piercing Beam weapon** first appears (fires through multiple wall tiles in a line)
- Walls now serve dual purpose: blocking AND protecting (destroying a wall may reveal an enemy ambush)
- Player must choose: destroy wall to find secrets, or leave it as cover from Shooter enemies
- **Rest beat**: Coin-rich corridor with no enemies at tile 120

---

#### Level 8: "Primal Arena"
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 4.0 | 3-4 tile gaps, moving platforms at 1.5 tiles/s |
| Destruction (D) | 4.0 | 15 walls, first multi-enemy encounter behind destructible barrier |
| Timing (M) | 4.0 | Overlapping platform and destruction timing |
| Resource (R) | 3.0 | Standard resource distribution |
| **Composite** | **3.9** | First simultaneous multi-enemy + destruction encounter |

- New element: **Simultaneous multi-target** -- player must cycle targets between 2 enemies and destructible walls simultaneously
- Player must manage auto-fire targeting: prioritize Charger rushing them, or Shooter behind partial wall?
- Dash becomes essential for repositioning during multi-target fights
- **Rest beat**: Secret room discovery (hidden path behind a destructible wall marked with cracks) with coins and a health potion

---

#### Level 9: "Ancient Bones"
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 4.0 | Consistent 3-tile gaps, vertical sections |
| Destruction (D) | 4.0 | 14 walls, open layout encourages exploration of destruction paths |
| Timing (M) | 3.5 | Standard timing, no new pressure |
| Resource (R) | 3.0 | Generous for pre-boss preparation, extra weapon attachment |
| **Composite** | **3.8** | Intentional slight dip before boss (rest point) |

- New element: **Stone Age Secret Room** variant with hidden coin cache behind multi-layer walls
- Difficulty dips slightly from Level 8 -- this is the "rest before the storm" design pattern
- Level layout is more open and exploratory, rewarding curiosity with destruction shortcuts
- Final section is a calm walk to the boss door with a checkpoint, health potion, and fresh weapon drop
- **Purpose**: Let the player feel confident and well-armed before the Era 1 boss

---

#### Level 10: "The Stone Colossus" -- Era 1 Boss
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 3.5 | Boss arena has platforms at 3 heights, falling boulder hazards |
| Destruction (D) | 5.5 | Boss: Stone Colossus (20 HP, 3 phases), destructible arena pillars |
| Timing (M) | 5.0 | Phase-specific dodge windows (1.5s -> 1.0s -> 0.75s) |
| Resource (R) | 3.5 | 2 health potions in arena, 1 weapon attachment, checkpoint at arena entrance |
| **Composite** | **4.6** | First boss encounter -- significant difficulty spike |

- New element: **Stone Colossus boss** (3-phase encounter)
  - Phase 1 (20-14 HP): Colossus hurls boulders horizontally across arena floor, 2s telegraph, jump to dodge. Auto-fire weapons chip at its body.
  - Phase 2 (13-7 HP): Adds ground slam that drops stalactites (destructible -- can be shot before impact). Arena pillars can be destroyed for line-of-sight advantages.
  - Phase 3 (6-0 HP): Colossus charges at player + arena walls become destructible revealing escape routes.
- Boss arena: 80 tiles wide, 12 tiles tall, 3 platform layers
- Health potions placed at platform edges, requiring traversal under pressure
- **Defeat reward**: "Stone Crown" cosmetic + "Colossus Crusher" achievement + era completion fanfare

---

#### Level 11: "Dawn of Bronze" -- Bronze Age Introduction
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 3.5 | New tileset, slightly narrower platforms, bronze-plated surfaces |
| Destruction (D) | 3.5 | Familiar wall types in new era environment, first bronze walls (harder) |
| Timing (M) | 3.5 | Standard timing |
| Resource (R) | 3.0 | Generous resources for new era exploration, new weapon type introduced |
| **Composite** | **3.4** | Intentional dip -- new era is the "novelty reward" |

- New element: **Bronze Age era** (new tileset, warmer palette, metallic accents, ancient city SFX, forge ambient sounds)
- Difficulty drops from the Level 10 boss peak -- reward for boss victory
- Familiar enemy types in unfamiliar setting lets player focus on environmental learning
- Introduces Bronze Age visual language (bronze-plated walls are harder but shinier)
- First appearance of **Homing Missile weapon** attachment
- **Purpose**: Reset the difficulty baseline for Era 2 while era novelty maintains engagement

---

#### Level 12: "Forge Descent"
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 4.5 | Heavy vertical design, wall jump required |
| Destruction (D) | 3.5 | Walls arranged in vertical shafts, must destroy floor tiles to descend |
| Timing (M) | 4.0 | Wall jump timing windows (1.5s per wall contact) |
| Resource (R) | 3.0 | Weapon drops placed at bottom of shafts |
| **Composite** | **3.9** | Wall Jump introduction level |

- New element: **Wall Jump ability** (unlocked at level start)
- Tutorial section: vertical shaft with alternating walls, must wall-jump upward
- Shaft has no enemies -- pure traversal skill learning
- Second shaft adds destructible floor tiles (shoot downward to create descent path)
- **Teaching moment**: Soft walls (mossy texture) indicate wall-jumpable surfaces. Non-jumpable walls have a smooth/bronze texture.

---

#### Level 13: "Bronze Bazaar"
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 4.5 | Wall jump sections + 3-tile horizontal gaps |
| Destruction (D) | 4.5 | 18 walls, first Flyer enemy weaves between destructible columns |
| Timing (M) | 4.5 | Flyer movement creates timing pressure on target cycling |
| Resource (R) | 3.5 | Moderate resources |
| **Composite** | **4.3** | New enemy type with movement-based challenge |

- New element: **Flyer enemy** (aerial, sine-wave patrol amplitude: 3 tiles, frequency: 2s, 1 HP)
- Flyers do not chase the player -- they fly fixed sine-wave patterns
- Player must cycle targets to Flyers while auto-fire is engaged with walls
- First section: Flyer over a flat area (learn the pattern)
- Second section: Flyer weaving between destructible columns (must destroy columns or dodge)
- **Rest beat**: Long horizontal corridor at tile 140 with coins, no enemies

---

#### Level 14: "Crumbling Walls"
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 5.0 | Falling platforms introduced, must keep moving |
| Destruction (D) | 4.5 | 20 walls, structural chain reactions (destroy key wall = cascade collapse) |
| Timing (M) | 5.0 | 0.5s crumble delay on platforms, must react fast |
| Resource (R) | 4.0 | Fewer potions, 40-tile checkpoint spacing |
| **Composite** | **4.7** | High-pressure traversal with destruction chain puzzles |

- New element: **Falling Platforms** (crumble 0.5s after player contact, dust particles warn) + **Chain Destruction** (destroying a support wall causes connected walls to collapse in sequence)
- Falling platforms are visually distinct: cracked stone texture, slight wobble on contact
- First falling platform is over safe ground (learn the mechanic without death risk)
- Second set: chain of 3 falling platforms over a pit (must keep moving forward)
- Chain destruction is introduced with visual cues: cracks propagate from destroyed support wall
- **Teaching moment**: A static platform is always within jump range if the player falls, reducing frustration

---

#### Level 15: "The Bronze Forge"
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 5.0 | All Bronze Age traversal mechanics combined |
| Destruction (D) | 5.0 | Bronze Sentinel introduced (armored, 4 HP, shield blocks frontal auto-fire) |
| Timing (M) | 4.5 | Must use destruction to create flanking angles past Sentinel shield |
| Resource (R) | 4.0 | Moderate-scarce resources |
| **Composite** | **4.7** | Mid-game peak -- combines all Bronze Age mechanics |

- New element: **Bronze Sentinel** (4 HP, frontal shield blocks auto-fire, must destroy walls to create side angles or use wall jump to get above)
- Sentinel is placed on a wide platform with destructible walls on both flanks
- Player must use destruction strategically: blow out walls to create line-of-sight angles around the shield
- Level combines: wall jump, falling platforms, Flyers, Shooters, and Bronze Sentinel
- **Rest beat**: After the Sentinel encounter, a calm section with coins and a health potion at tile 160

---

### Phase 3: Climax Sequence (Levels 16-20)

#### Level 16: "The Foundry"
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 5.5 | Lava pools introduced, platforming over hazards |
| Destruction (D) | 5.5 | 22 walls, molten material walls (3 HP, drop debris on destruction) |
| Timing (M) | 5.0 | Lava bubble timing (safe window: 2s), debris fall timing |
| Resource (R) | 4.5 | Sparse potions, long checkpoint gaps |
| **Composite** | **5.2** | Lava hazard + debris hazard introduction, difficulty ramp begins |

- New element: **Lava Pools** (instant kill, bubbling animation telegraphs safe/danger windows) + **Debris Hazards** (destroying walls near lava drops burning debris)
- Lava is visually dramatic: orange glow, particle embers, heat shimmer shader
- First lava pool has a wide platform above it (easy to avoid)
- Later lava pools require precise jumping between small platforms
- Molten walls near lava produce dangerous falling debris when destroyed -- player must time destruction
- **Teaching moment**: Lava bubbles intensify for 1s before erupting. Debris falls in predictable arc with shadow telegraph.

---

#### Level 17: "Gauntlet Approach"
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 6.0 | Fast moving platforms over lava, wall jump sequences |
| Destruction (D) | 5.5 | 24 walls, Bronze Sentinels on narrow platforms behind wall cover |
| Timing (M) | 5.5 | Overlapping hazards: lava + falling platforms + debris + enemies |
| Resource (R) | 5.0 | Scarce resources, limited checkpoints |
| **Composite** | **5.6** | Difficulty escalation toward boss |

- No new mechanics -- pure escalation of existing mechanics
- "Gauntlet" section at tiles 100-160: continuous sequence of challenges with no breaks
- Falling platforms over lava with Flyers in the air path
- Wall jump shaft with Bronze Sentinel at the top, must destroy wall cover from below
- **Purpose**: Build tension and pressure before the pre-boss gauntlet

---

#### Level 18: "Trial of the Ages" -- Pre-Boss Gauntlet
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 6.5 | Every traversal mechanic: gaps, moving, falling, wall jump, lava |
| Destruction (D) | 6.0 | 28 walls, all Bronze Age wall types, structural puzzles, 2 Bronze Sentinels |
| Timing (M) | 6.0 | Tight timing throughout, 1s or less windows |
| Resource (R) | 5.5 | Very scarce: 2 potions total, 2 checkpoints |
| **Composite** | **6.1** | Pre-boss gauntlet -- tests all skills |

- **Structure**: 4 gauntlet sections, each testing a different dimension:
  1. Traversal gauntlet (tiles 0-60): Platform obstacle course over lava with falling platforms
  2. Destruction gauntlet (tiles 60-120): Dense wall section with chain reactions, must clear path while enemies approach
  3. Timing gauntlet (tiles 120-180): Moving platforms with Flyers, precise jumps, debris timing
  4. Survival section (tiles 180-240): All combined, minimal resources, must manage weapon ammo and health
- Completion of this level signals the player is ready for the Era 2 boss
- **Design intent**: This is the hardest non-boss level. It should feel like an achievement to complete.

---

#### Level 19: "The Bronze Titan" -- Era 2 Boss
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 5.0 | Boss arena with 4 platform layers, lava floor |
| Destruction (D) | 7.0 | Bronze Titan boss (30 HP, 4 phases), destructible arena walls create tactical cover and attack angles |
| Timing (M) | 6.5 | Phase-specific dodge windows (1.5s -> 1.0s -> 0.5s -> 0.5s) |
| Resource (R) | 5.0 | 3 potions in arena at strategic positions, 2 weapon attachment drops |
| **Composite** | **6.1** | Era 2 boss -- multi-phase climax encounter |

- **Bronze Titan Boss** (30 HP, 4 phases):
  - Phase 1 (30-22 HP): Titan hurls bronze spears horizontally (3 aimed at player, 1.5s interval), dodge and let auto-fire chip at exposed joints
  - Phase 2 (22-14 HP): Titan activates forge hammers from ceiling (2 hammers, 2s telegraph shadow), destroys arena walls creating new lines of fire
  - Phase 3 (14-6 HP): Titan becomes mobile (walks between platforms), slams ground causing debris rain from destructible ceiling
  - Phase 4 (6-0 HP): Desperation -- all attacks increase speed by 1.5x, lava floor rises 2 tiles, arena compressed, player must destroy remaining walls for escape routes
- Arena: 80 tiles wide, 14 tiles tall, lava floor, 4 stone platforms at staggered heights
- Destructible walls throughout arena: destroying them creates new firing angles and escape routes
- Health potions at extreme left, center-top, and extreme right of arena
- Checkpoint: Arena entrance only (must complete boss in one life from last checkpoint)
- **Defeat reward**: "Bronze Helm" cosmetic + "Titan Toppler" achievement + Era 2 completion

---

#### Level 20: "New Dawn" -- Victory Lap
| Dimension | Score | Rationale |
|-----------|-------|-----------|
| Traversal (T) | 3.0 | Open, flowing platforming with generous platforms |
| Destruction (D) | 2.5 | Light destructible walls, satisfying to blast through, no challenge |
| Timing (M) | 2.0 | Relaxed timing, wide windows |
| Resource (R) | 1.5 | Abundant coins, potions, weapon attachments everywhere |
| **Composite** | **2.4** | Reward level -- catharsis after boss victory |

- **No new mechanics** -- pure celebration
- Level is coin-rich (50+ coins), enemies are sparse and slow
- Destructible walls are everywhere but soft (1 HP), creating a satisfying demolition run
- Background transitions from bronze forge interior to dawn light as player progresses rightward
- Multiple weapon attachment drops -- player cycles through weapons feeling powerful
- Secret room with massive coin cache (easy to find, reward for exploring)
- Level ends with a panoramic view of the Iron Age era (teaser for Era 3)
- **Purpose**: Emotional release after the boss fight. Player feels triumphant.
- **Design reference**: Mega Man X's intro stage, Celeste's post-summit descent

---

## 3. Composite Difficulty Curve Summary

| Level | Name | T | D | M | R | Composite | Phase | Rest Point? |
|-------|------|---|---|---|---|-----------|-------|-------------|
| 1 | First Steps | 1.0 | 1.0 | 1.0 | 1.0 | **1.0** | Teaching | -- |
| 2 | Breaking Ground | 1.5 | 2.0 | 1.5 | 1.0 | **1.6** | Teaching | -- |
| 3 | Target Practice | 2.0 | 2.5 | 2.0 | 1.5 | **2.1** | Teaching | -- |
| 4 | Shifting Ground | 3.0 | 2.5 | 3.0 | 2.0 | **2.7** | Teaching | -- |
| 5 | Rite of Passage | 3.5 | 3.0 | 3.0 | 2.5 | **3.0** | Teaching | -- |
| 6 | Mammoth Grounds | 3.5 | 3.5 | 3.0 | 2.5 | **3.2** | Escalation | -- |
| 7 | Bone Corridor | 4.0 | 3.5 | 3.5 | 3.0 | **3.6** | Escalation | -- |
| 8 | Primal Arena | 4.0 | 4.0 | 4.0 | 3.0 | **3.9** | Escalation | -- |
| 9 | Ancient Bones | 4.0 | 4.0 | 3.5 | 3.0 | **3.8** | Escalation | Yes (pre-boss dip) |
| 10 | Stone Colossus | 3.5 | 5.5 | 5.0 | 3.5 | **4.6** | Escalation | -- |
| 11 | Dawn of Bronze | 3.5 | 3.5 | 3.5 | 3.0 | **3.4** | Escalation | Yes (new era reset) |
| 12 | Forge Descent | 4.5 | 3.5 | 4.0 | 3.0 | **3.9** | Escalation | -- |
| 13 | Bronze Bazaar | 4.5 | 4.5 | 4.5 | 3.5 | **4.3** | Escalation | -- |
| 14 | Crumbling Walls | 5.0 | 4.5 | 5.0 | 4.0 | **4.7** | Escalation | -- |
| 15 | The Bronze Forge | 5.0 | 5.0 | 4.5 | 4.0 | **4.7** | Escalation | -- |
| 16 | The Foundry | 5.5 | 5.5 | 5.0 | 4.5 | **5.2** | Climax | -- |
| 17 | Gauntlet Approach | 6.0 | 5.5 | 5.5 | 5.0 | **5.6** | Climax | -- |
| 18 | Trial of the Ages | 6.5 | 6.0 | 6.0 | 5.5 | **6.1** | Climax | -- |
| 19 | Bronze Titan | 5.0 | 7.0 | 6.5 | 5.0 | **6.1** | Climax | -- |
| 20 | New Dawn | 3.0 | 2.5 | 2.0 | 1.5 | **2.4** | Climax | Yes (victory lap) |

### Curve Shape Verification

```
Difficulty
  7 |                                          ___
  6 |                                     ____/   \___
  5 |                                ____/            |
  4 |              ___    _____  ___/                 |
  3 |         ____/ | \__/ |   \/                     |
  2 |    ____/      |      |                          |___
  1 | __/           |      |                              \___
  0 |_______________|______|__________________________________|
    1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20
    |-- Teaching --|------ Escalation ------|----- Climax ----|
                   ^       ^                ^                 ^
                   Spike   Rest             Spike             Victory
                   Gate    Point            Boss              Lap
```

**Curve properties**:
- Generally monotonically increasing with intentional dips
- Maximum adjacent-level spike: 1.2 (Level 9 to 10) -- within the 2.0 limit
- Rest points at: Level 9 (pre-boss), Level 11 (era reset), Level 20 (victory lap)
- Rest point spacing: every 5-6 levels (within the 4-6 target range)
- Boss levels (10, 19) represent local difficulty peaks
- Victory lap (Level 20) provides dramatic drop for catharsis

---

## 4. Teaching Moments Documentation

### Teaching Philosophy

Every new mechanic follows the "Safe --> Practice --> Test --> Combine" pattern:

1. **Safe Introduction**: Mechanic presented in zero-risk environment (no death possible)
2. **Practice**: Mechanic used in low-risk setting (death possible but unlikely, checkpoint nearby)
3. **Test**: Mechanic required in moderate-risk setting (death likely if mechanic not understood)
4. **Combine**: Mechanic combined with previously learned mechanics

### Teaching Moment Index

| Level | Mechanic | Introduction Method | Safe? | Practice Section? | Test Section? |
|-------|----------|-------------------|-------|-------------------|---------------|
| 1 | Move (D-pad) | Arrow overlay prompt | Yes (no death) | Full level | N/A (next level) |
| 1 | Jump | Tap prompt at gap | Yes (floor below gap) | 2 more gaps in level | Level 3 |
| 2 | Weapon Pickup | Glowing drop before wall | Yes (wide platform) | Immediate auto-fire on wall | Level 3 |
| 2 | Auto-Fire | Weapon targets nearest wall | Yes (no enemies yet) | 2 more walls in level | Level 3 |
| 3 | Target Cycle | Prompt when enemy appears behind wall | Yes (health potion nearby) | 2 more enemies | Level 4 |
| 4 | Moving Platform | First platform over ground | Yes (floor below) | Second over pit | Level 5 |
| 4 | Stone Walls | Stone wall on wide flat area | Yes (no time pressure) | Level 5+ | Level 6 |
| 4 | Scatter Shot | Placed before wall cluster | Yes (can retry) | 3 wall sections | Level 6+ |
| 5 | Dash | Prompt at wide gap + spikes | Yes (can retry) | 3 dash sections | Level 6+ |
| 5 | Chain Destruction | Visual cracks on support wall | Low risk (checkpoint nearby) | Level 7+ | Level 14 |
| 7 | Piercing Beam | Placed before wall line | Low risk (potion nearby) | Multiple wall lines | Level 8+ |
| 10 | Boss Patterns | Phase 1 is slow + telegraphed | Moderate (checkpoint at door) | Phase 2 | Phase 3 |
| 11 | Homing Missile | Placed before Flyer section | Low risk | Multiple Flyers | Level 13+ |
| 12 | Wall Jump | Vertical shaft, no enemies | Yes (can drop safely) | Second shaft with destruction | Level 13+ |
| 13 | Flyer Patterns | Flyer over flat ground | Low risk | Flyer over gap | Level 14+ |
| 14 | Falling Platforms | First over solid ground | Yes (floor below) | Chain over pit | Level 15+ |
| 14 | Chain Destruction | Cracks propagate from support | Moderate (checkpoint) | Level 16+ | Level 18 gauntlet |
| 15 | Bronze Sentinel | Wide platform, walls on flanks | Moderate (potion nearby) | Level 16+ | Level 18 gauntlet |
| 16 | Lava Pools | Wide platform above lava | Low risk (distance) | Small platforms over lava | Level 17+ |
| 16 | Debris Hazards | First wall near lava | Low risk (shadow telegraph) | Multiple debris sections | Level 17+ |

### Anti-Frustration Design

| Principle | Implementation |
|-----------|---------------|
| Checkpoint generosity | Checkpoints before every new mechanic introduction |
| Instant respawn | Death --> checkpoint in <0.5 seconds, no loading screen |
| Health insurance | Health potion always placed before first encounter with new hazard |
| Weapon insurance | Fresh weapon attachment always placed before sections requiring destruction |
| Retreat option | Player can always walk backward to safe area (no one-way doors in teaching sections) |
| Visual telegraph | Every hazard has a visual warning at least 1 second before damage |
| Audio telegraph | Unique sound cue plays 0.5-1s before hazard activates |
| Difficulty acknowledgment | If player dies 3 times on same section, hint text appears (opt-in) |

---

## 5. Procedural Generation Difficulty Parameters

These values are passed to the level generator as constraints per level:

```json
{
  "level_difficulty_params": {
    "level_1": {
      "traversal": { "max_gap": 1, "moving_platforms": false, "falling_platforms": false, "vertical_layers": 1 },
      "destruction": { "wall_count": 0, "max_hardness": 0, "chain_reactions": false, "debris_hazards": false },
      "timing": { "min_window_ms": 99999, "overlapping_hazards": false },
      "resource": { "potion_density": 0.1, "checkpoint_interval": 40, "weapon_drops": 0 }
    },
    "level_10": {
      "traversal": { "max_gap": 4, "moving_platforms": true, "falling_platforms": false, "vertical_layers": 3 },
      "destruction": { "wall_count": 0, "max_hardness": 0, "chain_reactions": false, "debris_hazards": false, "boss": "stone_colossus" },
      "timing": { "min_window_ms": 750, "overlapping_hazards": true },
      "resource": { "potion_density": 0.025, "checkpoint_interval": 80, "weapon_drops": 1 }
    },
    "level_18": {
      "traversal": { "max_gap": 5, "moving_platforms": true, "falling_platforms": true, "vertical_layers": 3 },
      "destruction": { "wall_count": 28, "max_hardness": 4, "chain_reactions": true, "debris_hazards": true, "allowed_enemies": ["charger","shooter","flyer","bronze_sentinel"] },
      "timing": { "min_window_ms": 1000, "overlapping_hazards": true },
      "resource": { "potion_density": 0.015, "checkpoint_interval": 120, "weapon_drops": 1 }
    },
    "level_20": {
      "traversal": { "max_gap": 3, "moving_platforms": false, "falling_platforms": false, "vertical_layers": 2 },
      "destruction": { "wall_count": 15, "max_hardness": 1, "chain_reactions": false, "debris_hazards": false, "allowed_enemies": ["charger"] },
      "timing": { "min_window_ms": 3000, "overlapping_hazards": false },
      "resource": { "potion_density": 0.08, "checkpoint_interval": 30, "weapon_drops": 3 }
    }
  }
}
```

---

## References

- Swink, S. (2009). *Game Feel: A Game Designer's Guide to Virtual Sensation*. Chapter 7 (Difficulty Curves).
- Mega Man X (1993): Teaching through level design without text prompts.
- Celeste (2018): Difficulty escalation with assist mode and rest points.
- Super Meat Boy (2010): Rapid retry loop and difficulty layering.
- Schell, J. (2008). *The Art of Game Design*. Lens #34: Skill vs. Difficulty.

---

**Version**: 1.1
**Last Updated**: 2026-02-04
**Status**: Active
**Assessment**: 3.2 - Difficulty Curve Design
