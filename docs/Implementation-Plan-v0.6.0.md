# Implementation Plan: v0.6.0 (Review Cycle 3 — "Make It Fun")

**Date**: 2026-02-07
**Review source**: [Expert-Review-Report-v0.5.0.md](Expert-Review-Report-v0.5.0.md)
**Status**: Awaiting approval
**Goal**: Maximum fun. Close the gap between strong systems and satisfying moment-to-moment experience.

---

## Design Principles for This Cycle

1. **Feel over features.** Every sprint prioritizes making existing things feel better over adding new things.
2. **Show, don't hide.** Every system the player interacts with must have visible feedback.
3. **Earn the boss fight.** The boss encounter is the climax — it must feel like one.
4. **Performance is a feature.** Object pooling before new effects. Smooth 60fps or nothing.

---

## Sprint Plan

### Sprint 0: Critical Bug Fixes

**Goal**: Fix all bugs that actively harm gameplay.

**Changes**:

1. **Boss.cs — Fix slow-effect/phase-speed interaction (B1)**
   - Store phase-adjusted speed as `_baseMoveSpeed` in `OnPhaseChange()`
   - `ApplySlow` and `UpdateSlow` will then restore the correct phase speed

2. **Boss.cs — Add contact damage cooldown (B2)**
   - Add 0.5s cooldown between contact damage applications
   - Prevents cornered player from taking damage every physics frame

3. **Boss.cs — Fix DPS cap float truncation (B3)**
   - Use `Mathf.CeilToInt` instead of `(int)` cast for remaining damage allowance

4. **AbilitySystem integration (B4)**
   - Verify PlayerController discovers AbilitySystem at runtime (lazy GetComponent)
   - If not, add lazy-cached `GetComponent<AbilitySystem>()` in PlayerController

5. **WeaponSystem.cs — Fix FindTarget range (B5)**
   - Move auto-select before FindTarget, or use max possible range for target finding

6. **BossArenaTrigger.cs — Fix null boss soft-lock (B6)**
   - If boss is null on trigger, don't consume the trigger. Re-check next frame.

**Verification**: All 6 bugs confirmed fixed. No regression in existing behavior.

---

### Sprint 1: Object Pooling Foundation

**Goal**: Eliminate runtime instantiation for high-frequency objects. Prerequisite for all juice effects.

**Files to create/modify**:
- NEW: `ObjectPool.cs` — Generic pool with pre-allocation, grow-on-demand, and auto-return
- `Projectile.cs` — Pool return instead of Destroy
- `WeaponSystem.cs` — Pool fetch instead of new GameObject
- `EnemyBase.cs` — Pool for enemy projectiles and death particles
- `TilemapRenderer.cs` — Pool for destruction particles
- `PlayerController.cs` — Pool for wall-slide dust

**Changes**:

1. **Generic ObjectPool<T>**: Pre-allocate N instances, disable on return, enable on fetch. Auto-grow if pool is empty (with warning log). Static registry by prefab type.

2. **Projectile pool**: Pre-allocate 30. On "destroy," disable and return to pool. On fire, fetch from pool and reinitialize.

3. **Particle pool**: Pre-allocate 40. Shared between tile destruction (3-6 per break), enemy death (5-8 per death), wall dust.

4. **Muzzle flash pool**: Pre-allocate 5. Short-lived (2 frames), high reuse rate.

**Performance target**: Zero `new GameObject()` calls during gameplay after level load.

---

### Sprint 2: Juice Pass — Core Game Feel

**Goal**: Make every player action feel satisfying.

**Files to modify**:
- `CameraController.cs` — Screen shake system (trauma-based)
- `GameManager.cs` — Hit-stop system
- `PlayerController.cs` — Landing particles, stomp air-stall
- `TilemapRenderer.cs` — Material-scaled destruction effects
- `EnemyBase.cs` — Hit reaction, death enhancement
- `PlaceholderAudio.cs` — Weapon SFX variation, destruction crunch
- `HealthSystem.cs` — Damage direction flash, death slowdown

**Changes**:

1. **Trauma-based screen shake** (CameraController.cs):
   - Replace linear shake with trauma accumulator: `magnitude = trauma^2`
   - Perlin noise offset (not Random.Range) for smooth shake
   - Framerate-independent: fix Lerp to use `1 - Exp(-speed * dt)`
   - Triggers: player damage (0.4 trauma), Cannon fire (0.3), boss damage (0.15), tile break (0.1), hazard (0.5)

2. **Hit-stop** (GameManager.cs):
   - `public static void HitStop(float duration)` — sets `Time.timeScale = 0` for N frames then restores
   - Triggers: enemy kill (2 frames / 33ms), boss phase transition (4 frames / 66ms), boss death (8 frames / 133ms)
   - Use `Time.unscaledDeltaTime` for all effects during hit-stop

3. **Landing feedback** (PlayerController.cs):
   - Spawn 3-4 small dust particles (from pool) on ground landing
   - Scale particle count by fall speed (0 at walking speed, 4 at max fall)
   - 50ms air-stall on stomp initiation before descending
   - Variable stomp bounce: base 6f + 2f per enemy stomped + 1f per tile height fallen

4. **Material-scaled destruction** (TilemapRenderer.cs):
   - Soft: 2 particles, small, fast fade
   - Medium: 4 particles, medium
   - Hard: 6 particles, larger, screen shake 0.1
   - Reinforced: 8 particles, large, screen shake 0.2
   - Indestructible: 10 particles + flash + screen shake 0.3

5. **Death enhancement** (HealthSystem.cs):
   - Slow spin to 360 deg/s (was 720)
   - 100ms freeze frame before death animation starts
   - Fade to red tint during spin
   - Respawn: brief white flash + 0.3s invuln indicator

6. **Sound variation** (PlaceholderAudio.cs):
   - Weapon fire: ±10% pitch randomization per shot
   - Tile destruction: distinct "crunch" sound (not just debris SFX)
   - Enemy hit: pitch varies by enemy type

---

### Sprint 3: Boss Encounter Overhaul

**Goal**: Make boss fights feel like the climax of every level.

**Files to modify**:
- `Boss.cs` — Phase transitions, telegraphs
- `BossArenaTrigger.cs` — Arena lockdown, intro sequence
- `GameplayHUD.cs` — Boss health bar
- `CameraController.cs` — Arena camera lock
- `PlaceholderAudio.cs` — Boss battle SFX
- `LevelLoader.cs` — Arena door tiles

**Changes**:

1. **Arena lockdown** (BossArenaTrigger + LevelLoader):
   - On boss activation, spawn solid wall tiles at arena entrance (player cannot retreat)
   - Walls break on boss death (liberating exit)

2. **Boss intro sequence** (BossArenaTrigger):
   - 2-second cinematic: camera pans to boss → boss name card appears (e.g., "MAMMOTH CHIEF") → boss roar SFX → camera returns to player → fight begins
   - Player is invulnerable during intro

3. **Boss health bar** (GameplayHUD):
   - 3-segment bar at top of screen (one per phase)
   - Color changes: green → yellow → red per phase
   - Flashes white on hit for feedback
   - Shows boss name text above bar

4. **Phase transition drama** (Boss.cs):
   - Hit-stop (66ms) on phase change
   - Screen flash (white, 0.3s fade)
   - Boss briefly invulnerable (0.5s)
   - Boss roar SFX
   - Speed lines or screen border effect

5. **Charge attack telegraph improvement** (Boss.cs):
   - Increase charge telegraph from 0.2s to 0.6s
   - Add visual: boss pulls back (moves away briefly) then charges
   - Add audio: wind-up growl before charge
   - Add screen border warning glow during wind-up

6. **Camera arena lock** (CameraController):
   - When boss is active, clamp camera X within arena bounds
   - Slight zoom-out (orthographic size 7 → 8) to show full arena

---

### Sprint 4: Visible Systems

**Goal**: Every system the player interacts with must have visible feedback.

**Files to modify**:
- `GameplayHUD.cs` — Running score, ability indicators, DPS feedback
- `LevelCompleteUI.cs` — Personal best, star thresholds
- `WeaponSystem.cs` — Quick Draw visual, auto-select reason
- `Boss.cs` — DPS cap indicator

**Changes**:

1. **Running score on HUD** (GameplayHUD):
   - Score counter at top-center, updates on every scoring event
   - Brief scale-up animation on score change
   - Color pulse on big score events (kill streak, hidden content discovery)

2. **Ability indicators** (GameplayHUD):
   - Small icons below health for Double Jump and Air Dash
   - Icon dims when on cooldown, fills back up
   - Brief glow on ability grant

3. **Quick Draw visual** (WeaponSystem + GameplayHUD):
   - When Quick Draw is active (1.5s after weapon switch), weapon name text glows gold
   - Brief "QUICK DRAW!" text flash at top of screen on activation

4. **DPS cap feedback** (Boss + GameplayHUD):
   - When damage is capped, show "RESISTED" in grey text above boss
   - Projectiles that deal reduced damage flash grey on impact instead of white

5. **Personal best on level complete** (LevelCompleteUI):
   - Track best score per level code in PlayerPrefs
   - Show "NEW RECORD! +X" or "Best: Y" on level complete screen
   - Show star thresholds: "Next star: 13,300 (need 1,200 more)"

6. **Auto-select reason** (GameplayHUD):
   - Small text below weapon name showing context: "→PIERCER (corridor)" or "→SLOWER (boss)"
   - Only shows for 2 seconds after auto-switch

---

### Sprint 5: Era Differentiation

**Goal**: Make each of the 10 eras feel mechanically and aesthetically distinct.

**Files to modify**:
- `ParallaxBackground.cs` — Era-specific silhouettes
- `PlaceholderAudio.cs` — Era music variations
- `Boss.cs` — Per-era boss behavior variations
- `PlaceholderAssets.cs` — Platform sprites per era

**Changes**:

1. **Era-specific parallax silhouettes** (ParallaxBackground):
   - Layer 2 (mid-structures): era-specific landmark silhouettes
     - Era 0 Stone: rocky outcrops, cave entrances
     - Era 1 Bronze: ziggurats, sun disks
     - Era 2 Classical: columns, temples
     - Era 3 Medieval: castle towers, battlements
     - Era 4 Renaissance: domes, arched bridges
     - Era 5 Industrial: smokestacks, gears
     - Era 6 Modern: skyscrapers, cranes
     - Era 7 Digital: server racks, circuit towers
     - Era 8 Space: launch pads, satellite dishes
     - Era 9 Transcendent: crystal spires, floating islands
   - Fix `GetEraBackgroundColors` to actually use the epoch parameter

2. **Era music variations** (PlaceholderAudio):
   - Modify gameplay music generator to accept epoch parameter
   - Era 0-2: slower tempo (110 BPM), triangle wave lead (gentle, ancient)
   - Era 3-5: medium tempo (128 BPM), square wave lead (driving, medieval→industrial)
   - Era 6-8: faster tempo (140 BPM), sawtooth lead (aggressive, modern→digital)
   - Era 9: unique composition (ethereal, reverb-like delay patterns, 120 BPM)
   - All use same chord progression for cohesion but different instruments/tempo

3. **Boss behavior variations** (Boss.cs):
   - Era 0-2 bosses: Heavier charge focus (70% charge, 30% shoot). Slower but harder-hitting.
   - Era 3-5 bosses: Balanced (50/50). Add a shield-bash move (brief charge with knockback but no damage).
   - Era 6-8 bosses: Projectile focus (30% charge, 70% shoot). Faster projectiles. Add spread shot in Phase 1 (not just Phase 3).
   - Era 9 boss: Unique — teleport to random arena position instead of charging. Fires homing projectiles that arc slowly.

4. **Platform sprites** (PlaceholderAssets):
   - Use era-specific palette for platform tiles instead of fixed grey stone.

---

### Sprint 6: Weapon Balance

**Goal**: Every weapon should have a clear tactical identity and no weapon should be strictly dominant.

**Files to modify**:
- `WeaponData.cs` — Rebalance stats
- `WeaponSystem.cs` — Slower damage amp mechanic
- `Projectile.cs` — Scaled specials per tier

**Changes**:

1. **Spreader nerf**: Damage multiplier 0.8x → 0.5x per projectile. Still best for crowds (1.5x total) but Bolt is better for single targets.

2. **Chainer buff**: Starting tier chain count 1 → 2. Chain range 4f → 5f at Medium, 6f at Heavy.

3. **Cannon anti-boss identity**: Cannon ignores 30% of boss DPS cap. Becomes the "boss buster" weapon with risk (heat) and reward.

4. **Slower utility buff**: Slowed enemies take 20% more damage from all sources. Slower becomes a force multiplier, not a DPS loss.

5. **Tier scaling for specials**:
   - Spreader: 3 → 4 → 5 projectiles
   - Piercer: 2 → 3 → 4 pierce count
   - Chainer: 2 → 3 → 4 chain count
   - Slower: 3s → 4s → 5s duration, 50% → 60% → 70% slow

6. **Quick Draw enhancement**: Visual weapon glow (white tint on sprite) + audio pitch shift (+20% on first Quick Draw shot)

---

### Sprint 7: Score Visibility and Gamification

**Goal**: Players should understand and be motivated by the scoring system.

**Files to modify**:
- `GameplayHUD.cs` — Score popups, combo display
- `GameManager.cs` — Combo tracking
- `LevelCompleteUI.cs` — Details expansion, tips
- `AchievementManager.cs` — Toast notification system

**Changes**:

1. **Score popups**: Floating "+100" text on enemy kill, "+25" on coin pickup. From pool, fade over 1s, drift upward.

2. **Combo counter**: Kills within 2 seconds build a multiplier. Display as "x3 COMBO" near player. Gold text, larger with each tier. Reset on damage.

3. **Achievement toast**: On unlock, slide in from top: "[ACHIEVEMENT] First Blood" with icon. Display 3 seconds. Queue if multiple.

4. **Level complete improvements**:
   - First completion of any level: show details expanded by default
   - Show star thresholds
   - Show personal best comparison
   - "Tip:" line based on lowest component ("Tip: Explore more hidden areas to boost your Exploration score")

5. **Fix trivially earned achievements**:
   - HazardDodger: only counts if level had 5+ hazard tiles
   - CoolUnderPressure: only counts if Cannon was fired 3+ times

---

### Sprint 8: Polish Pass

**Goal**: Close remaining feel gaps.

**Changes**:

1. **Enemy telegraphs**: 0.3s flash/glow before shooting. Red tint builds up, then fires.
2. **Enemy knockback on hit**: Small pushback (2 units/sec) when hit by player projectile. Gives weight to hits.
3. **Wall-slide velocity fix**: Don't cap upward velocity on wall grab. Only apply wall-slide speed when player is descending.
4. **Level intro**: 1s camera pan from start to ~30 tiles ahead, then back to player. Orients the player.
5. **Era intro card**: Brief text overlay on level start: "ERA 3: MEDIEVAL" with era-specific accent color. 2s display, fade out.
6. **Boss arena camera zoom**: Slight zoom out when boss activates to show full arena.
7. **Damage direction indicator**: Brief red flash on the screen edge closest to the damage source.

---

## Sprint Dependency Order

```
Sprint 0 (Bug fixes)     — must be first
Sprint 1 (Object pool)   — prerequisite for Sprint 2
Sprint 2 (Juice pass)    — depends on Sprint 1
Sprint 3 (Boss overhaul) — depends on Sprint 2 (uses shake, hit-stop)
Sprint 4 (Visible systems) — independent after Sprint 0
Sprint 5 (Era differentiation) — independent after Sprint 0
Sprint 6 (Weapon balance) — independent after Sprint 0
Sprint 7 (Score/gamification) — depends on Sprint 4 (HUD changes)
Sprint 8 (Polish) — benefits from all others complete
```

**Recommended order**: 0 → 1 → 2 → 3 → 4 → 5 → 6 → 7 → 8

---

## Version Targets

- **Version**: v0.6.0
- **Build**: 012
- **After implementation**: Update BuildInfo.cs, BUILD-LOG.md, GAME-DESIGN-REVIEW.md
- **After build**: Run playtests targeting: boss fights, weapon balance, era identity, juice feel

---

## Verification Checklist (Post-Implementation)

### Bug Fixes (Sprint 0)
- [ ] Boss slow effect does not permanently reduce speed after phase change
- [ ] Boss contact damage has 0.5s cooldown
- [ ] DPS cap does not block fractional damage
- [ ] Abilities work when picked up mid-level
- [ ] Weapon auto-select uses correct range
- [ ] Boss arena trigger re-checks if boss is null

### Object Pooling (Sprint 1)
- [ ] Zero `new GameObject()` calls during gameplay (verify with profiler)
- [ ] No visible difference in behavior from pre-pool version

### Juice (Sprint 2)
- [ ] Screen shake on: player damage, Cannon fire, boss damage, hazard trigger
- [ ] Hit-stop on: enemy kill, boss phase change, boss death
- [ ] Landing dust on every ground landing
- [ ] Material-scaled destruction particles
- [ ] Death freeze frame before spin
- [ ] Weapon fire pitch variation audible

### Boss (Sprint 3)
- [ ] Arena lockdown prevents retreat
- [ ] Boss intro plays with name card and roar
- [ ] Health bar visible with 3 segments
- [ ] Phase transitions have drama (flash, pause, SFX)
- [ ] Charge telegraph is 0.6s and readable

### Visible Systems (Sprint 4)
- [ ] Running score visible on HUD
- [ ] Ability icons visible with cooldown state
- [ ] Quick Draw activation visible
- [ ] Boss "RESISTED" text on DPS cap
- [ ] Personal best on level complete
- [ ] Star thresholds shown

### Era Differentiation (Sprint 5)
- [ ] Each era has distinct parallax silhouettes
- [ ] Music tempo/instrument changes per era group
- [ ] Boss behavior varies by era
- [ ] Platforms use era palette

### Weapon Balance (Sprint 6)
- [ ] Spreader is not strictly dominant DPS
- [ ] Chainer Starting tier is viable
- [ ] Cannon has anti-boss identity
- [ ] Slower amplifies team damage
- [ ] Specials scale with tier

### Score/Gamification (Sprint 7)
- [ ] Score popups visible on kills and pickups
- [ ] Combo counter appears at x2+
- [ ] Achievement toast notification on unlock
- [ ] Star thresholds on level complete
- [ ] Personal best comparison visible
- [ ] Trivially earned achievements fixed

### Polish (Sprint 8)
- [ ] Enemy shooting has 0.3s telegraph
- [ ] Enemies take knockback on hit
- [ ] Wall-grab preserves upward momentum
- [ ] Level intro camera pan plays
- [ ] Era name card displays on level start
- [ ] Damage direction indicator visible

---

*This plan addresses all Critical and High findings from the v0.5.0 expert review. Ready for approval.*
