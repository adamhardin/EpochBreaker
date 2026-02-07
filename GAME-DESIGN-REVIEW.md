# Epoch Breaker -- Game Design Review Document

**Prepared for**: Expert game design review
**Date**: 2026-02-06
**Build**: v0.6.0 build 012
**Play it**: [https://adamhardin.github.io/EpochBreaker/](https://adamhardin.github.io/EpochBreaker/) (WebGL, keyboard: arrows/WASD, Space=jump, S/Down=stomp, X/J=cycle weapon, Esc=pause)

---

## Reviewer Competencies

To maximize the value of feedback on this game, we recommend a reviewer with experience in as many of the following areas as possible. Listed in priority order by impact on fun:

### Critical (directly shapes whether the game is fun)

1. **2D Platformer Game Feel & Tuning** -- Hands-on experience tuning jump arcs, gravity curves, acceleration, coyote time, input buffering, and the micro-second-level responsiveness that separates a "tight" platformer from a sluggish one. References: *Celeste*, *Hollow Knight*, *Dead Cells*, *Super Meat Boy*.

2. **Destructible Environment Design** -- Understanding of how terrain destruction creates emergent gameplay: when breaking things should feel powerful vs. when preservation creates strategic depth. Experience with destruction as both a traversal mechanic and a puzzle mechanic. References: *Spelunky*, *Noita*, *Terraria*, *Broforce*.

3. **Weapon System & Power Fantasy Design** -- Designing multi-weapon, utility-based combat systems where each weapon has a distinct tactical identity. Balancing "power fantasy escalation" against difficulty so the player feels strong without the game becoming trivial. References: *Vampire Survivors*, *Enter the Gungeon*, *Nuclear Throne*, *Binding of Isaac*.

4. **Mobile Game UX & Touch Control Optimization** -- Designing for two-thumb landscape play on phones. Understanding thumb reach zones, touch target sizing, input latency tolerance, and how to make complex actions feel natural on a touchscreen. Experience with iOS Human Interface Guidelines.

### High Value (shapes progression, replayability, and retention)

5. **Procedural Level Design & Replayability** -- Experience with seed-based or algorithmic level generation that produces levels that feel hand-crafted. Understanding how to create variety within constraints, ensure fairness, and make procedural content feel intentional rather than random. References: *Spelunky*, *Dead Cells*, *Hades*, *Rogue Legacy*.

6. **Difficulty Curve & Boss Encounter Design** -- Designing multi-phase boss fights for side-scrollers, pacing difficulty across a campaign, and creating encounters that teach through failure. Understanding the relationship between checkpoint spacing, player frustration, and satisfaction. References: *Mega Man*, *Cuphead*, *Shovel Knight*.

7. **Scoring, Incentive & Achievement System Design** -- Designing scoring systems that create interesting player choices (speed vs. exploration vs. combat vs. preservation). Building achievement systems that guide players toward mastery without feeling like checklists. Creating "one more run" compulsion loops ethically.

8. **Ethical Retention & Session Design** -- Designing daily challenges, streak systems, social sharing, and comeback mechanics that respect the player's time. Experience building retention without dark patterns, loot boxes, or FOMO-driven mechanics. Understanding session length optimization for mobile.

### Valuable (shapes polish, identity, and long-term appeal)

9. **Retro / Pixel Art Aesthetic Direction** -- Understanding the 16-bit visual language: what makes pixel art readable at small sizes, how color palettes communicate gameplay information, and how to create visual identity with constrained art (our sprites are generated via code, not drawn). References: *Shovel Knight*, *Celeste*, *Katana ZERO*.

10. **Chiptune & Synthesized Game Audio** -- Experience with waveform-based audio design (square, triangle, sawtooth, noise). Understanding how SFX feedback loops reinforce game feel, how music pacing matches gameplay intensity, and how to make synthesized audio feel polished rather than placeholder. References: *Shovel Knight*, *Undertale*, *Celeste*.

11. **Social/Competitive Mechanics for Single-Player Games** -- Designing asynchronous competition (leaderboards, ghost data, shared seeds) that makes a single-player game feel social. Understanding what motivates players to share level codes, compare scores, and build community around a non-multiplayer game. References: *Spelunky* daily challenges, *Trackmania*, *Hitman* Elusive Targets.

12. **Cross-Platform Design (Mobile + Browser)** -- Understanding the UX differences between touch and keyboard play, and how to design a game that feels native on both without compromising either. Experience shipping to both iOS and WebGL.

### Ideal Reviewer Profile

The single most valuable reviewer would be someone who has **shipped a 2D action-platformer with procedural elements on mobile** -- ideally one with destructible environments or a growing weapon/ability system. They would understand the intersection of tight platformer feel, mobile constraints, procedural fairness, and power fantasy progression that defines Epoch Breaker's core design challenge.

---

## 1. Game Identity

**Epoch Breaker** is a retro side-scrolling shooter for iOS where a small armored character collects diverse weapon types that auto-fire from their body, blasting through destructible environments that evolve across 10 eras of human civilization.

**Core fantasy**: You're a one-person army carving through walls of stone, brick, steel, and energy shielding -- each era's terrain crumbling differently under your growing arsenal. Strategic destruction reveals paths, platforms, and secrets. Reckless destruction triggers hazards and collapses your footing.

**Elevator pitch**: *Contra meets Spelunky's destructibility, wrapped in a civilization-spanning art theme with shareable procedural levels.*

### 1.1 Core Loop (30-second cycle)

```
Move -> Encounter wall/enemy -> Auto-fire destroys it -> Collect weapon/reward drop
  -> More firepower -> Move forward -> Encounter harder wall/enemy -> Repeat
```

### 1.2 Session Loop (2-5 minutes per level)

```
Enter level -> Traverse intro zone -> Weapon pickups diversify arsenal
  -> Combat gauntlets + destruction puzzles + hazard navigation -> Boss arena -> Score screen
  -> Share level code / Continue
```

### 1.3 Meta Loop (across sessions)

```
Tutorial (3 levels) -> Campaign (10 epochs in order) -> Unlock Streak mode
  -> Chase high scores on Legends leaderboard -> Share level codes with friends
  -> Compete on same seed -> Achievement hunting
```

---

## 2. What's Playable Right Now

The game is a working prototype with all core systems functional. A reviewer can play through full procedural levels across all 10 eras.

### Currently working:
- Full procedural level generation from deterministic seeds (same code = same level, any device)
- 3 game modes: Campaign (10 epochs, 2 lives), Streak (random epochs, 10 lives, endless), FreePlay (single levels)
- 3-level onboarding tutorial (movement, combat, wall-climbing) with platform-aware hints
- Player movement: asymmetric gravity, coyote time, jump buffering, ground pound, wall-slide, wall-jump
- 6-slot weapon system (Bolt, Piercer, Spreader, Chainer, Slower, Cannon) with 3 tiers each
- Auto-fire targeting with auto-select AI + manual weapon cycling
- Cannon heat system with overheat lockout
- Destructible terrain with 5 material hardness levels
- Environmental hazards (FallingDebris, UnstableFloor, GasRelease, FireRelease, SpikeTrap, CoverWall)
- Relic tiles with preservation scoring incentive
- 4 enemy behaviors (Patrol, Chase, Stationary, Flying) across 40 visual variants
- Epoch-based difficulty scaling via DifficultyProfile (enemy count, HP, speed, shoot %, boss HP)
- Boss encounters with 3-phase AI, DPS cap, minimum phase duration, phase 3 damage shield
- Checkpoint/respawn system with 4 checkpoint types
- 6 reward pickup types (Health, Attack, Speed, Shield, Coin, Extra Life)
- 5-component scoring (Speed, Items/Enemies, Combat Mastery, Exploration, Preservation)
- Score-based 3-star ratings
- 30 tracked achievements across 8 categories
- Achievement gallery UI, Legends leaderboard (top 20 streak runs)
- Session persistence (continue from where you left off)
- Level history with copy-to-clipboard level codes
- Settings menu: 3 volume sliders, tutorial replay, randomize toggle, about info
- Build ID display on title screen
- All visuals generated at runtime (pixel art painted via code, no imported sprites)
- All audio synthesized at runtime (waveform synthesis, no imported audio files)

### Not yet implemented:
- Cosmetic unlocks (skins, trails)
- Daily challenge system
- Sprite animations (currently static sprites)

---

## 3. Player Mechanics

### 3.1 Movement Physics

| Parameter | Value | Design intent |
|-----------|-------|---------------|
| Max horizontal speed | 6 tiles/sec | Brisk but controllable |
| Acceleration | 40 units/sec^2 | Snappy start |
| Deceleration | 40 units/sec^2 | Quick stop, no slide |
| Jump initial velocity | 12 units/sec | ~4 tile max height |
| Rising gravity | 28 units/sec^2 | Floaty ascent for aiming |
| Falling gravity | 42 units/sec^2 | Snappy descent for responsiveness |
| Max fall speed | 20 units/sec | Terminal velocity |
| Coyote time | 5 frames (~83ms) | Forgiveness for ledge jumps |
| Jump buffer | 6 frames (~100ms) | Press jump slightly early |
| Variable jump (release) | velocity * 0.5 | Tap = short hop, hold = full arc |
| Ground pound speed | 30 units/sec downward | Fast slam |
| Ground pound bounce | 6 units/sec upward | Small recovery hop |

### 3.2 Wall-Slide & Wall-Jump

Wall-slide and wall-jump are formalized core mechanics, critical for the tutorial Level 3 and for advanced navigation throughout the game.

| Parameter | Value | Design intent |
|-----------|-------|---------------|
| Wall-slide speed | 4 units/sec downward | Slow descent, time to react |
| Wall-slide gravity | 12 units/sec^2 | Much lighter than normal falling |
| Wall-jump X velocity | 5 units/sec | Push off wall, not too far |
| Wall-jump Y velocity | 12 units/sec | Full jump height from wall |
| Wall-jump input lock | 5 frames (~83ms) | Prevent immediate redirect |
| Wall coyote time | 4 frames (~67ms) | Grace period after leaving wall |
| Wall detection | 2 raycasts per side (chest + foot) | Reliable, requires both to hit |

**Wall-slide activation**: Airborne + pressing into wall. Can grab at any vertical velocity (including while ascending after a wall-jump). Horizontal velocity zeroes on grab.

**Wall-jump**: Applies force away from wall + upward. Input lock prevents the player from immediately overriding the push-off, but clears early if the player touches a new wall (enabling fast chimney climbs).

**Chimney climbing**: Between two walls, players can chain wall-jumps rapidly. The wall-grab-at-any-velocity and lock-clear-on-new-wall-grab mechanics make this fluid.

**Visual feedback**: Sprite squishes (0.85x wide, 1.1x tall) while sliding. Dust particles spawn at the contact point. Player faces away from wall during slide.

**Audio**: Looping wall-slide scraping sound plays while sliding.

### 3.3 Health & Damage

| Parameter | Value |
|-----------|-------|
| Max HP | 5 |
| Enemy contact damage | 1 |
| Boss contact damage | 2 |
| Enemy projectile damage | 1 |
| Boss projectile damage | 2 |
| Post-hit invulnerability | 1.0 second |
| Spawn protection | 2.0 seconds |
| I-frame visual | Alpha flicker 1.0-0.3 at 10 Hz |
| Knockback force | 8 units/sec away from source |
| Knockback min Y | 4 units/sec upward (always bounce) |
| Kill plane | y < -5 (instant death from falling) |

### 3.4 Lives & Death

| Mode | Deaths allowed per level | Global lives | On death |
|------|--------------------------|-------------|----------|
| FreePlay | 2 | N/A | Respawn at checkpoint, then game over |
| Campaign | Uses global lives | 2 | Respawn at checkpoint; 0 lives = game over |
| Streak | Uses global lives | 10 | Respawn at checkpoint; 0 lives = game over |

---

## 4. Weapon & Combat System

### 4.1 Six Weapon Types

The player acquires weapons as pickups throughout the level. Each weapon type has a distinct tactical identity. The auto-fire system selects the best weapon for the situation, but the player can manually cycle with the Attack button.

| Type | Identity | Fire Rate (S/M/H) | Damage Mult | Special | Material |
|------|----------|-------------------|-------------|---------|----------|
| Bolt | Reliable default | 0.25/0.20/0.15s | 1.0x | None | Tier-based |
| Piercer | Corridor control | 0.30/0.26/0.22s | 0.7x | Passes through 2-4 enemies | Tier-based |
| Spreader | Crowd control | 0.35/0.30/0.25s | 0.8x | 3 projectiles, 30deg cone, 8-tile range | Tier-based |
| Chainer | Group damage | 0.40/0.35/0.30s | 0.6x | Arcs to 1-3 nearby enemies | Tier-based |
| Slower | Utility/boss tool | 0.50/0.45/0.40s | 0.5x | 3s slow (50% speed) on hit | Tier-based |
| Cannon | Power + risk | 0.60/0.55/0.50s | 3.0x | Breaks ALL materials, heat system | All |

Each weapon type has **Starting/Medium/Heavy** tiers that affect base damage and fire rate.

### 4.2 Auto-Select AI

The system picks the best weapon automatically:
- **Boss in range** -> Slower (debuff priority)
- **3+ clustered enemies** -> Spreader or Chainer
- **Long corridor** -> Piercer
- **Hard/reinforced block** -> Cannon (if available)
- **Default** -> Bolt

**Manual override**: Pressing Attack (X/J on keyboard, B on touch) cycles to the next acquired weapon. Override persists for 10 seconds or until Attack is pressed again.

### 4.3 Cannon Heat System

The Cannon is the most powerful weapon but has a heat mechanic:
- 10 heat per shot, 40 max heat
- Cools at 8 heat/second
- Overheating locks the Cannon for 2 seconds
- Heat bar visible on HUD when Cannon is active

### 4.4 Epoch-Based Enemy Resistance

To prevent high-tier weapons from trivializing content:
- Enemies 2+ epochs ahead: 50% damage reduction
- Enemies 1 epoch ahead: 25% damage reduction
- Boss in epochs 5+: Bolt damage halved (encourages weapon diversity)

### 4.5 Weapon Pickup Placement

The level generator distributes weapons by epoch:
- **Epoch 0-2**: Bolt pickups + one Piercer or Spreader
- **Epoch 3-5**: Add Chainer and Slower to pool
- **Epoch 6-9**: All types including Cannon
- **Boss arenas**: Always include a Slower pickup near entrance

### 4.6 Material Destruction

| Material | Starting | Medium | Heavy | Cannon | HP |
|----------|----------|--------|-------|--------|-----|
| Soft | 1 hit | 1 hit | 1 hit | 1 hit | Low |
| Medium | 3 hits | 1 hit | 1 hit | 1 hit | Medium |
| Hard | Cannot | 3 hits | 1 hit | 1 hit | High |
| Reinforced | Cannot | Cannot | 3 hits | 1 hit | Very high |
| Indestructible | Cannot | Cannot | Cannot | 1 hit | 50 HP |

---

## 5. Enemy System

### 5.1 Enemy Count Per Level

```
baseCount = 8 + (intensity * 60)
scaledCount = baseCount * DifficultyProfile.EnemyCountMultiplier
```

Intensity is derived from the level seed (0.0 to 1.0). DifficultyProfile scales from 0.5x (epoch 0) to 1.0x (epoch 9), so effective enemy count ranges from ~4 to 68 per level.

### 5.2 Behavior Types

| Type | Frequency | Speed | Shooting | Behavior |
|------|-----------|-------|----------|----------|
| Patrol | 30% | 2 u/s | No | Walk back and forth within bounds |
| Chase | 30% | 3.5 u/s | 2.5s cooldown | Pursue player when within 8 tiles |
| Stationary | 30% | 0 | 1.5s cooldown | Fixed turret, aim and fire |
| Flying | 10% | 2 u/s lateral + vertical bob | 2.0s cooldown | Hover and patrol, harder to reach |

- Shoot percentage scaled by DifficultyProfile (40% at epoch 0, 85% at epoch 9)
- All enemies: base 3 HP, scaled by DifficultyProfile.EnemyHpMultiplier (0.7x-1.5x)
- Enemy speed scaled by DifficultyProfile.EnemySpeedMultiplier (0.8x-1.2x)
- Enemy projectiles: 6 u/s, 1 damage
- Visual: 40 variants (4 behaviors x 10 era color palettes), 192x192 px sprites

### 5.3 Boss System

One boss per level, spawned in a dedicated BossArena zone at the end.

| Property | Value | Scaling |
|----------|-------|---------|
| HP | DifficultyProfile | 100 (epoch 0) to 280 (epoch 9) |
| Size | 3 units (384x384 px) | Fixed |
| Contact damage | 2 | Fixed |
| Projectile damage | 2 | Fixed |
| DPS cap | 15 damage/second | Excess damage ignored |
| Min phase duration | 8 seconds | Prevents burst-skipping |
| Phase 3 shield | 1/3 damage for 2s | After spread shot |
| Death reward | 5 enemy kills | For scoring/achievements |

**Three phases**:

| Phase | HP threshold | Behavior | Speed | Cooldown |
|-------|-------------|----------|-------|----------|
| Phase 1 | 100-67% | Patrol + charge attacks | 1.0x | Normal |
| Phase 2 | 67-33% | Chase + shooting (40% shoot, 60% charge) | 1.2x | 0.8x |
| Phase 3 | <33% | Desperate rapid attacks (50% spread shot, 50% charge) + damage shield | 1.5x | 0.5x |

- **Charge attack**: 0.8s wind-up, then dash at high speed, bounces off walls
- **Single shot**: Direct aim at player
- **Spread shot**: 3 projectiles in a +-15 degree cone
- **Phase 3 shield**: After each spread shot, boss takes only 1/3 damage for 2 seconds
- **DPS cap**: Rolling 1-second window, max 15 damage. Prevents high-DPS weapons from trivializing fights
- **Player respawn**: Boss fully resets (HP, phase, visual state) on player death
- **Death animation**: 1.5s of shaking, color flashing, spinning, scaling to zero
- **Slow resistance**: Bosses resist 25% of Slower weapon effect

---

## 6. Environmental Hazards

### 6.1 Hazard Types

Hazards are tied to destructible blocks. Breaking a hazard-marked block triggers the effect.

| Hazard | Effect | Damage | Duration | Visual |
|--------|--------|--------|----------|--------|
| FallingDebris | Tiles above fall as physics objects | 2 in 1.5-tile radius | Instant | Crack overlay |
| UnstableFloor | Floor crumbles after player steps on it | Fall damage | 0.5-1.5s timer | Subtle cracks |
| GasRelease | Green toxic cloud | 1 per 0.5s | 3 seconds | Green tint overlay |
| FireRelease | Fire erupts from broken block | 1 per 0.3s | 4 seconds | Orange tint overlay |
| SpikeTrap | Spikes emerge from adjacent tiles | 2 on contact | 5 seconds | Spike edge overlay |
| CoverWall | Visual only -- breaking exposes player to enemy fire | Indirect | Permanent | Shield outline |

### 6.2 Hazard Distribution

- Hazard chance scales with epoch: 5% at era 0, 40% at era 9
- Zone-specific weights: Combat zones get more CoverWalls, Destruction zones get more FallingDebris
- **Safety**: Hazards are NEVER placed on the primary walkable corridor (4-tile zone above ground)
- LevelValidator checks that no hazard tiles exist on required walkable path

### 6.3 Relic Tiles

- 15% of structural groups get one relic tile (top tile)
- Relics have a golden glow border
- Preserving relics earns Preservation score
- Destroying blocks near relics may destroy them (collateral damage)
- HUD shows relic counter (gold when all preserved, orange when some lost)

---

## 7. Level Generation

### 7.1 Deterministic Pipeline

Every level is generated from a seed embedded in a shareable level code: `E-XXXXXXXX` (epoch digit + 8 base32 characters = 40-bit seed). The same code always produces the identical level on any device.

**PRNG**: xorshift64* (no Unity math functions allowed in the pipeline -- pure C# for cross-platform determinism).

### 7.2 Dimensions & Structure

| Parameter | Value |
|-----------|-------|
| Base width | 256 tiles (scaled by seed-derived length modifier, 0.8x-1.2x) |
| Height | 16 tiles (fixed) |
| Ground height | Bottom 1-5 Unity units (constrained to lower portion) |
| Tile size | 1 Unity unit = 64x64 pixels |
| Camera | Orthographic size 7, shows 14 tiles vertically |

### 7.3 Zone Layout

| Zone | % of level | Purpose | Enemy density | Destruction density |
|------|-----------|---------|---------------|---------------------|
| Intro | 10% | Safe ramp-up | Few | Low (3-4%) |
| Traversal | 15% | Platforming focus | Medium | Medium (4-5%) |
| Buffer | 5% | Breathing room | Sparse | Very low (2-3%) |
| Destruction | 25% | Block-breaking sandbox | Heavy | High (8-8.5%) |
| Combat | 20% | Enemy gauntlet | High (2x weight) | Medium (5%) |
| SkillGate | ~5% | Pre-boss challenge | 2-3 enemies + weapon | Medium |
| Buffer | 5% | Prep for boss | Sparse | Low |
| Boss Arena | 15% | Final challenge | Boss only | Medium (6%) |

**SkillGate** (new in v0.3.0): A locked-door section 20-30 tiles before the BossArena containing 2-3 enemies and a weapon pickup. Tests readiness before the boss fight.

### 7.4 Material Distribution by Era

| Era | Soft | Medium | Hard | Reinforced | Indestructible |
|-----|------|--------|------|------------|----------------|
| 0 Stone Age | 60% | 25% | 10% | 0% | 5% |
| 3 Medieval | 20% | 30% | 30% | 15% | 5% |
| 6 Modern | 10% | 15% | 35% | 35% | 5% |
| 9 Transcendent | 15% | 15% | 20% | 35% | 15% |

### 7.5 Hazard & Relic Placement

Post-generation stages (after base level layout):
1. `AssignHazards()`: Marks destructible tiles with hazard types based on epoch scaling
2. `PlaceRelics()`: Marks 15% of structural groups with a relic tile
3. `EnsureWalkablePath()`: Strips hazards from corridor tiles, guarantees completability

### 7.6 Completability Guarantee

`EnsureWalkablePath` is a post-generation pass that:
- Clears walls where ground rises >2 tiles between columns
- Converts hard/reinforced materials to soft in a 3-tile walking corridor
- Strips hazards from corridor tiles
- Guarantees every level is completable with the starting weapon

### 7.7 Reward & Checkpoint Placement

- **Health packs**: Every 20 tiles (70% small, 30% large)
- **Damage boosts**: Every 50 tiles
- **Hidden rewards**: 10% chance per eligible wall (behind destructibles)
- **Checkpoints**: Every 64 tiles + pre-boss + boss arena entrance

---

## 8. Progression & Game Modes

### 8.1 Three Modes

**Campaign** (default):
- Play 10 levels, one per epoch (Stone Age through Transcendent), in order
- Start with 2 lives
- Collect extra lives during levels
- Die twice in one level = level failed, advance to next
- Lose all lives = game over
- Completing all 10 unlocks Streak mode and Legends leaderboard

**Streak** (unlocked after Campaign):
- Random epochs, 10 lives, no extra life pickups
- Each completed level adds to your streak count
- Game over when all lives are lost
- Top 20 streaks tracked on Legends leaderboard

**FreePlay** (Randomize toggle ON):
- Single levels with random or manually-entered codes
- 2 deaths per level = game over
- Good for practicing or sharing specific levels

### 8.2 Onboarding Tutorial

First-time Campaign players automatically enter a 3-level tutorial sequence:

| Level | Length | Focus | Key Mechanics Taught |
|-------|--------|-------|---------------------|
| 1: Move & Jump | 64 tiles | Movement | Walking, jumping, coyote time, gap crossing, platforms |
| 2: Shoot & Smash | 80 tiles | Combat | Auto-fire, tile breaking, ground pound, weapon upgrades |
| 3: Climb & Survive | 96 tiles | Advanced | Wall-slide, wall-jump, weapon tiers, enemy combat, strategic floor |

- Platform-aware hints: keyboard prompts on desktop, touch prompts on mobile
- Step-based progression: hints appear after delay, advance on trigger conditions
- Skippable via Settings > SKIP TUTORIAL
- Replayable via Settings > REPLAY TUTORIAL (directly starts tutorial)
- Persistence: completion state stored in PlayerPrefs

### 8.3 Scoring (5-Component System)

No single component exceeds 30% of theoretical maximum score (~20,000).

| Component | Max | Calculation |
|-----------|-----|-------------|
| Speed | 5,000 | `max(500, 5000 - elapsed * 25)` |
| Items & Enemies | ~3,000 | Item multipliers + kill streak bonuses |
| Combat Mastery | 5,000 | Kill efficiency (kills/shots * 2000) + no-damage streak (*150) + boss bonus (1500) |
| Exploration | 4,000 | Hidden content ratio (*2500) + secret areas (*300 each) |
| Preservation | 3,000 | Relic integrity ratio (*2000) + low destruction (<30%) bonus (*1000) |

**Item multiplier**: Each item type's base value multiplied by how many of that type collected:

| Item | Base value | 1st | 2nd | 3rd |
|------|-----------|-----|-----|-----|
| Coin | 25 | 25 | 50 | 75 |
| Health Small | 50 | 50 | 100 | 150 |
| Health Large | 100 | 100 | 200 | 300 |
| Attack Boost | 150 | 150 | 300 | 450 |
| Speed Boost | 150 | 150 | 300 | 450 |
| Shield | 200 | 200 | 400 | 600 |

**Kill streak bonus**: `100 * consecutive_kill_count`. Resets on player damage.

### 8.4 Star Ratings

| Stars | Condition |
|-------|-----------|
| 3 | Score >= 14,000 |
| 2 | Score >= 8,000 |
| 1 | Completed |

### 8.5 Level Codes & Social

Level codes (e.g., `3-K7XM2P9A`) can be copied to clipboard and shared. Anyone entering the same code gets the identical level. Level history stores the last 50 played levels with scores and star ratings.

---

## 9. Difficulty Curve

### 9.1 DifficultyProfile (Epoch-Based Scaling)

Smooth interpolation between control points:

| Epoch | Enemy Count | Enemy HP | Enemy Speed | Shoot % | Boss HP |
|-------|------------|----------|-------------|---------|---------|
| 0 | 0.50x | 0.70x | 0.80x | 40% | 100 |
| 3 | 0.80x | 1.00x | 0.95x | 60% | 160 |
| 6 | 0.95x | 1.20x | 1.05x | 70% | 220 |
| 9 | 1.00x | 1.50x | 1.20x | 85% | 280 |

All values linearly interpolated between control points (e.g., epoch 4 interpolates between epoch 3 and epoch 6 rows).

### 9.2 Boss Anti-Trivialize Mechanics

| Mechanic | Purpose |
|----------|---------|
| DPS cap (15/sec) | Prevents burst damage from skipping phases |
| Min phase duration (8s) | Ensures each phase is experienced |
| Phase 3 shield (1/3 damage, 2s) | After spread shot, gives boss breathing room |
| Boss reset on player death | Full HP/phase/state reset, no cheese via attrition |
| Slow resistance (25%) | Slower weapon less dominant against bosses |
| Bolt damage halved (epoch 5+) | Encourages weapon diversity at high epochs |

---

## 10. Achievement System

30 achievements tracked persistently (PlayerPrefs/IndexedDB):

### Destruction (3)
| Achievement | Condition |
|-------------|-----------|
| Demolisher | 100 blocks destroyed (cumulative) |
| Total Demolition | ALL destructible blocks in a single level |
| Chain Reaction | 10 blocks in 5 seconds |

### Combat (6)
| Achievement | Condition |
|-------------|-----------|
| First Blood | Kill your first enemy |
| Enemy Slayer | 50 enemies killed (cumulative) |
| Untouchable | Complete a level without taking damage |
| Boss Killer | Defeat any boss |
| Killing Spree | 5-enemy kill streak without taking damage |
| Rampage | 10-enemy kill streak without taking damage |

### Collection (4)
| Achievement | Condition |
|-------------|-----------|
| Collector | 50 items collected (cumulative) |
| Hoarder | ALL items in a single level |
| Weapon Master | Collect all 3 weapon tiers in a single level |
| Weapon Collector | Acquire all 6 weapon types in one level |

### Speed (3)
| Achievement | Condition |
|-------------|-----------|
| Speed Demon | Complete a level in under 60 seconds |
| Speed Runner | Complete a level in under 45 seconds |
| Lightning Fast | Complete a level in under 30 seconds |

### Completion (5)
| Achievement | Condition |
|-------------|-----------|
| First Victory | Complete your first level |
| Epoch Explorer | Complete a level in each of the 10 epochs |
| Perfect Run | Earn a 3-star rating |
| Veteran | Complete 10 levels (cumulative) |
| Master | Complete 50 levels (cumulative) |

### Score (2)
| Achievement | Condition |
|-------------|-----------|
| High Scorer | Score 10,000+ in a single level |
| Score Legend | Score 25,000+ in a single level |

### Movement (1)
| Achievement | Condition |
|-------------|-----------|
| Wall Jump Master | Perform 10 wall-jumps in a single level |

### Preservation & Mastery (6)
| Achievement | Condition |
|-------------|-----------|
| Archaeologist | Preserve all relic tiles in a level |
| Minimal Footprint | Complete destroying less than 10% of blocks |
| Structural Engineer | Complete with over 80% structures intact |
| Hazard Dodger | Complete without taking hazard damage |
| Chain Master | Kill 3 enemies with one chain lightning |
| Cool Under Pressure | Complete without overheating Cannon |

---

## 11. Audio Design

All audio is synthesized at runtime using `AudioClip.Create()` + waveform math. No imported audio files.

### 11.1 Sound Effects (12+ clips)

| SFX | Trigger | Waveform |
|-----|---------|----------|
| Jump | Player jumps | Square sweep up |
| Stomp | Ground pound lands | Low noise burst |
| Shoot | Weapon fires (varies by type) | Triangle + square |
| Enemy Hit | Enemy takes damage | Square + noise |
| Enemy Die | Enemy killed | Descending sweep |
| Player Hurt | Player takes damage | Buzzy alarm |
| Reward Pickup | Collect item | Ascending chime |
| Weapon Pickup | Collect weapon | Fanfare arpeggio |
| Checkpoint | Activate checkpoint | Warm chime |
| Level Complete | Reach goal | Victory fanfare |
| Menu Select | UI button press | Click |
| Wall Slide | Sliding on wall (looping) | Soft scraping noise |

### 11.2 Music (2 looping tracks)

| Track | BPM | Key | Usage |
|-------|-----|-----|-------|
| Title Theme | 110 | C major (C-Am-F-G) | Title screen |
| Gameplay Theme | 128 | A minor (Am-F-C-G) | During gameplay |

### 11.3 Volume Levels

| Channel | Base volume | User slider range |
|---------|------------|-------------------|
| Music | 0.15 | 0-100% multiplier |
| SFX | 0.25 | 0-100% multiplier |
| Weapon Fire | Separate slider | 0-100% multiplier |

Anti-stacking: Same clip can't play more than once per 50ms.

---

## 12. Visual Design

All sprites are generated at runtime via Texture2D pixel painting. No imported art assets.

| Entity | Size | PPU | World size |
|--------|------|-----|------------|
| Tiles | 64x64 px | 64 | 1x1 unit |
| Player | 192x192 px | 128 | 1.5x1.5 units |
| Enemies | 192x192 px | 128 | 1.5x1.5 units |
| Boss | 384x384 px | 128 | 3x3 units |
| Projectile | 32x32 px | 64 | 0.5x0.5 units |
| Weapon Pickup | 96x96 px | 64 | 1.5x1.5 units |
| Reward Pickup | 64x64 px | 64 | 1x1 unit |
| Checkpoint | 64x192 px | 64 | 1x3 units |
| Goal Portal | 192x256 px | 64 | 3x4 units |

10 era-specific color palettes applied to enemies and bosses. Player is an armored hero with helmet visor, boot treads, and gauntlets. Enemy silhouettes: soldier (patrol), beast (chase), turret (stationary), winged creature (flying).

6 weapon pickup sprites: sword (Bolt), lance (Piercer), trident (Spreader), lightning bolt (Chainer), hourglass (Slower), barrel (Cannon).

Hazard overlays: crack lines (debris), green tint (gas), orange tint (fire), spike edges, shield outline (cover), golden glow (relic).

### Not yet implemented:
- Animations (currently static sprites)

---

## 13. Controls & Camera

### 13.1 Input

| Action | Keyboard | Touch (planned) |
|--------|----------|-----------------|
| Move | A/D or arrows | Left/Right D-pad buttons |
| Jump | Space or W or Up | A button (right side) |
| Stomp (airborne) | S or Down | Down button |
| Cycle weapon | X or J | B button |
| Pause | Escape or P | Pause button (top) |

### 13.2 Camera

| Parameter | Value |
|-----------|-------|
| Orthographic size | 7 (14 tiles visible vertically) |
| Vertical offset | +2.8 units (player at 30% from bottom) |
| Horizontal smooth | 8 u/sec lerp |
| Vertical smooth | 5 u/sec lerp |
| Vertical deadzone | 1.0 unit |
| Look-ahead | +/-1.5 units in facing direction |
| Clamping | Level bounds |

---

## 14. The 10 Eras

The game's world evolves through all of human civilization. Each era affects enemy colors, tile material distribution, difficulty scaling, and boss HP.

| Era | Theme | Soft% | Reinforced% | Boss HP | Enemy Speed |
|-----|-------|-------|-------------|---------|-------------|
| 0 | Stone Age | 60% | 0% | 100 | 0.80x |
| 1 | Bronze Age | 45% | 0% | 120 | 0.85x |
| 2 | Classical | 30% | 10% | 140 | 0.90x |
| 3 | Medieval | 20% | 15% | 160 | 0.95x |
| 4 | Renaissance | 20% | 20% | 180 | 0.98x |
| 5 | Industrial | 15% | 25% | 200 | 1.02x |
| 6 | Modern | 10% | 35% | 220 | 1.05x |
| 7 | Digital | 10% | 40% | 240 | 1.10x |
| 8 | Space Age | 10% | 45% | 260 | 1.15x |
| 9 | Transcendent | 15% | 35% | 280 | 1.20x |

---

## 15. Design Spec Highlights (Planned but Not Built)

### 15.1 Special Attack System
- Hold jump for 0.5s while grounded with 3+ weapons
- Era-specific devastating attack (Avalanche, Tempest, Barrage, Rain of Fire, etc.)
- 15-second cooldown, reduced by extra attachments
- Full invulnerability during attack

### 15.2 Ability Progression
- Double Jump, Dash with i-frames, Ground Slam, Phase Shift through thin walls

### 15.3 Daily Challenge System
- Unique seed per day, difficulty rotates Mon-Thu, era rotates on 10-day cycle
- Global/friends leaderboards, streak tracking

### 15.4 Full Boss Encounters
- 10 unique bosses with destructible arena elements central to each fight
- Era 1-3: 2 phases, Era 4-7: 3 phases, Era 8-10: 4-8 phases
- Boss 10 (The Architect): 800 HP, 8 phases, rewrites arena geometry mid-fight

### 15.5 Cosmetic Progression
- 15 character skins, 8 trail effects, 5 profile frames
- Earned through achievements (Bronze/Silver/Gold tiers)

---

## 16. Key Design Questions for Review

We'd love expert perspective on these areas:

1. **Wall-slide/wall-jump feel**: The wall mechanics are now formalized (see Section 3.2). Do the tuning values (5 u/s push-off, 12 u/s jump, 5-frame lock) create satisfying climbing? Is chimney climbing too easy or too hard?

2. **Weapon diversity vs. complexity**: With 6 weapon types x 3 tiers = 18 total weapons, is the system too complex for a mobile game? Does auto-select make manual cycling feel unnecessary, or is it a good accessibility feature?

3. **Hazard-destruction tension**: Do hazards successfully make destruction feel risky? Is the hazard frequency scaling (5% epoch 0 to 40% epoch 9) appropriate? Are any hazard types unfun?

4. **Scoring balance**: With 5 components (Speed, Items, Combat Mastery, Exploration, Preservation), are multiple play styles actually viable? Does any component dominate? Is the theoretical max (~20,000) achievable?

5. **Boss anti-trivialize mechanics**: The DPS cap (15/sec continuous decay), minimum phase duration (5s), and phase 3 pillar shelter are designed to prevent burst-skipping. Do they make fights feel designed or artificially padded?

6. **Tutorial pacing**: 3 tutorial levels teaching movement -> combat -> wall-climbing. Is this too long? Too short? Should any mechanics be cut from the tutorial?

7. **Difficulty curve**: Does the DifficultyProfile scaling (see Section 9.1) create a satisfying progression from epoch 0 to 9? Is epoch 0 too easy or epoch 9 too hard?

8. **Relic preservation incentive**: Does the Preservation score component create interesting decisions about which blocks to break? Or does it feel like a "don't break things" penalty that conflicts with the game's destructive identity?

9. **Level sharing as social mechanic**: The shareable level code system is our core social feature. What makes people actually share level codes? How do we build community around this?

10. **What's the most impactful unbuilt feature?** Given limited development resources, which planned-but-unbuilt system (special attacks, daily challenges, cosmetics, sprite animations, high score leaderboard) would have the biggest impact on fun? (Note: abilities and parallax backgrounds are now implemented in v0.5.0.)

---

## 17. Technical Context

- **Engine**: Unity 6000.3.6f1 (originally targeting 2022 LTS)
- **Target**: iOS 15+, iPhone 11+ at 60fps
- **Level generation**: < 100ms average, < 150ms P95 on iPhone 11
- **Memory**: < 100 MB peak, < 2 MB per level
- **All art and audio generated at runtime** -- zero imported assets
- **Deterministic PRNG** (xorshift64*) -- no Unity math in generation pipeline
- **Level dimensions**: 256+ width x 16 height tiles
- **Max enemies per level**: 68
- **Max simultaneous projectiles**: Target 30 (current), designed for 48

---

## 18. Changes Since Last Review (v0.2.0 -> v0.3.0)

Summary of all changes made in response to the first expert review:

| Area | What Changed |
|------|-------------|
| Movement | Wall-slide + wall-jump formalized as core mechanic |
| Weapons | Single-tier upgrade replaced with 6-slot utility weapon system |
| Destruction | Environmental hazards + relic tiles add strategic risk to breaking |
| Scoring | 3-component replaced with 5-component system (multiple play styles) |
| Tutorial | 3-level onboarding teaching mechanics progressively |
| Difficulty | Epoch-based DifficultyProfile with smooth interpolation |
| Bosses | DPS cap, min phase duration, phase 3 shield, full reset on death |
| Achievements | 22 -> 30 achievements (preservation, mastery, weapon mastery) |
| Enemies | HP/speed/shoot% now scaled by DifficultyProfile per epoch |
| Level gen | SkillGate zone, hazard/relic placement stages |
| UI | 5-component score breakdown, relic counter, heat bar, weapon slots |
| Settings | Tutorial replay button, build ID display |

**Full change log**: [docs/Expert-Review-Change-Log.md](docs/Expert-Review-Change-Log.md)

---

## 19. Changes Since Last Review (v0.3.0 -> v0.5.0)

v0.4.0 implemented expert review cycle 2 feedback. v0.5.0 added performance, polish, and features.

| Area | What Changed |
|------|-------------|
| Weapons | Auto-select limited to Bolt/Piercer/Slower only; Quick Draw +25% fire rate on manual switch |
| Scoring | "Preservation" renamed "Archaeology", max reduced 3000→2000, star thresholds recalibrated |
| Bosses | Phase 3 shield replaced with destructible ArenaPillar system (HP=10, Cannon insta-break); MIN_PHASE_DURATION 8s→5s; DPS cap changed to continuous decay |
| Hazards | Density capped at 30%; late-epoch CoverWall bias; Cannon triggers FallingDebris (40% per tile above) |
| UI | LevelCompleteUI collapsed scores with DETAILS toggle; GameplayHUD kill streak indicator |
| Tutorial | Wall-jump hints delayed (4s linger); all gates changed to PlayerReachedX |
| Visual | 5-layer parallax backgrounds (procedural per-epoch); screen flash on level start/complete; squash/stretch on jump/land; enhanced death particles |
| Movement | AbilitySystem: double jump (epoch 3+) and air dash (18 u/s, 0.12s, 0.8s cooldown) via collectible AbilityPickup orbs |
| Performance | Cached physics queries, WeaponSystem/LevelRenderer/HazardSystem refs; batched CompositeCollider2D geometry; debounced PlayerPrefs.Save; static EnemyBase registry eliminates FindGameObjectsWithTag; cached reflection type lookups |
| Code Quality | Singleton guards; death processing guard; pause button touch target 50→70px |
| Bugs Fixed | Boss player-ref loss on respawn; DPS cap exploit; boss death streak inflation; boss ground detection; HazardSystem deterministic RNG; ArenaPillar debris fall-through; Quick Draw false activation; slow effect color restore |

---

## 20. Changes Since Last Review (v0.5.0 -> v0.6.0)

v0.6.0 "Make It Fun" pass: 9 sprints focused on game feel, balance, and polish.

| Area | What Changed |
|------|-------------|
| Performance | Object pooling (Projectile/30, Particle/50, Flash/8) eliminates all runtime allocations during gameplay |
| Game Feel | Trauma-based screen shake (Perlin noise), hit-stop on kills/boss phases, landing dust particles, muzzle flash, death particles with era-colored bursts |
| Boss Overhaul | Arena lockdown with destructible pillars, boss intro cinematic with name card, 3-phase camera zoom, phase-transition hit-stop, slow damage amplification (1.2x), Cannon DPS cap bypass (30%) |
| Visible Systems | Running score with scale animation, ability icons (double jump/air dash), Quick Draw weapon glow + "QUICK DRAW!" flash, auto-select reason display, DPS cap "RESISTED" feedback, boss health bar with phase colors |
| Era Identity | Era-specific parallax silhouettes, era-parameterized music (tempo/waveform/volume per era group), era-specific platform tile colors, boss arena uses era tiles |
| Weapon Balance | Spreader damage 0.8x→0.5x with +projectiles per tier, Chainer chain count 1+t→2+t and range 4→4+t, Slower duration 2+t→3+t with stronger slow, Quick Draw +25% fire rate and +20% audio pitch on manual weapon switch |
| Gamification | Score popups (world-to-screen, pooled), combo counter (x2+ with 2s window, reset on damage), achievement toast queue (slide-in animation), first-completion auto-expand details, contextual tips on level complete |
| Polish | Enemy shoot telegraphs (0.3s red tint buildup), enemy knockback on hit (2 units/sec), wall-slide only caps descending velocity, level intro camera pan (1s sweep ahead), era intro card ("ERA N: NAME" with accent color), damage direction indicators (red edge flash) |
| Scoring | Kill combo system builds multiplier within 2s window, resets on damage |
| Achievements | HazardDodger requires 5+ hazard tiles, CoolUnderPressure requires 3+ Cannon shots, RecordCannonShot tracking |

---

*This document represents the game as of v0.6.0 build 012 (2026-02-06). The codebase, design specifications, and this review document are all available for reference.*
