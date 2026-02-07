# Expert Review Report: Epoch Breaker v0.5.0 (Build 011)

**Date**: 2026-02-07
**Review type**: Comprehensive deep review (code + design + fun + creativity + gamification)
**Scope**: Full codebase audit + 12-competency evaluation + beyond-rubric analysis
**Goal**: Maximize fun
**Status**: Review cycle 3

---

## Executive Summary

Epoch Breaker is a technically impressive game with a strong foundation: deterministic procedural generation, 6 distinct weapons, destructible environments, 10 thematic eras, and zero imported assets. The engineering ambition is remarkable.

**However, the game is currently more impressive as a technical achievement than it is fun to play.** The systems are deep, but the moment-to-moment experience lacks the juice, feedback, and player communication that turn good systems into compelling gameplay. The biggest gains will come not from adding new systems, but from making existing systems *feel better*.

**Top 5 findings (ordered by fun impact):**

1. **[CRITICAL] No game feel juice.** No screen shake, no hit-stop, no satisfying destruction feedback, no boss intro, no pickup celebration. The game is mechanically sound but emotionally flat.
2. **[CRITICAL] Invisible systems frustrate instead of engaging.** DPS cap on boss is invisible. Quick Draw buff is invisible. Abilities have no HUD indicator. Score components are hidden until level end. Players cannot optimize what they cannot see.
3. **[CRITICAL] Boss fights are the game's showcase moment, and they're underwhelming.** No intro sequence, no music change, no health bar, no arena lockdown, no phase transition drama. Plus a critical slow-effect bug permanently reduces boss speed after Phase 1.
4. **[HIGH] Object pooling is completely absent.** Every projectile, particle, muzzle flash, and debris piece is instantiated/destroyed. On WebGL and mobile, this will cause GC stutters during the most intense (and most important) moments.
5. **[HIGH] The 10 eras feel cosmetically different but mechanically identical.** Backgrounds, silhouettes, and music don't change per era. The era progression—the game's core identity—is underdelivered.

---

## Competency Scores

### Critical Competencies

| # | Competency | Score | Threshold | Status |
|---|-----------|-------|-----------|--------|
| C1 | 2D Platformer Game Feel & Tuning | 72/100 | 85 | **NEEDS REMEDIATION** |
| C2 | Destructible Environment Design | 68/100 | 85 | **NEEDS REMEDIATION** |
| C3 | Weapon System & Power Fantasy | 60/100 | 85 | **NEEDS REMEDIATION** |
| C4 | Mobile Game UX & Touch Controls | 45/100 | 90 | **NEEDS REMEDIATION** |

### High-Value Competencies

| # | Competency | Score | Threshold | Status |
|---|-----------|-------|-----------|--------|
| H5 | Procedural Level Design & Replayability | 78/100 | 85 | **NEEDS REMEDIATION** |
| H6 | Difficulty Curve & Boss Encounters | 55/100 | 85 | **NEEDS REMEDIATION** |
| H7 | Scoring, Incentive & Achievement Design | 65/100 | 85 | **NEEDS REMEDIATION** |
| H8 | Ethical Retention & Session Design | 82/100 | 90 | **NEEDS REMEDIATION** |

### Valuable Competencies

| # | Competency | Score | Threshold | Status |
|---|-----------|-------|-----------|--------|
| V9 | Retro / Pixel Art Aesthetic Direction | 70/100 | 80 | **NEEDS REMEDIATION** |
| V10 | Chiptune & Synthesized Audio | 62/100 | 80 | **NEEDS REMEDIATION** |
| V11 | Social/Competitive Mechanics | 50/100 | 80 | **NEEDS REMEDIATION** |
| V12 | Cross-Platform Design (Mobile + Browser) | 55/100 | 80 | **NEEDS REMEDIATION** |

**Overall**: 0 of 12 competencies pass. Average score: 63.5/100. The game has strong foundations but does not yet meet expert-quality thresholds in any area.

---

## Detailed Findings

### CRITICAL FINDINGS

#### CF-1: The Game Lacks Juice (Impacts C1, C2, C3, V9, V10)

The single biggest obstacle to fun. "Juice" is the collection of micro-feedback that makes actions feel satisfying: screen shake on impact, hit-stop on enemy kills, particles on destruction, flash on damage, sound variation on repeated actions.

**Current state:**
- No screen shake on any event (designed in v0.4.0 Sprint 8 but CameraController.Shake() is unused)
- No hit-stop (brief time freeze) on enemy kills or boss damage
- Tile destruction particles are functional but use full Rigidbody2D physics objects (3-6 GameObjects per tile break)
- No muzzle flash persistence (designed as 2-frame effect but likely invisible)
- No landing dust when player hits ground
- Death animation spins at 720 deg/sec (comically fast blur instead of impactful death)
- No pickup celebration effect (items just vanish)
- Boss death is mechanical (shake + shrink) with no drama
- Achievement unlocks are completely silent (event fires but no UI subscribes)

**Impact**: The game feels like a tech demo rather than a finished game. Players will subconsciously judge the game as "not polished" within 5 seconds, regardless of how deep the systems are.

**Recommendation**: This is the single highest-ROI improvement. Implement a minimal juice pass:
- Screen shake (already designed): player damage, boss damage, Cannon fire, hazard explosion
- Hit-stop: 30ms time freeze on enemy kill, 60ms on boss phase transition
- Landing particles: 3 small sprites on every ground landing
- Stomp initiation pause: 50ms air-stall before ground pound descent
- Achievement toast: popup notification on unlock
- Boss phase transition: 0.5s dramatic pause + screen flash + SFX

---

#### CF-2: Invisible Systems (Impacts C3, H6, H7)

Players cannot engage with systems they cannot see.

**Invisible system inventory:**
| System | Problem | Player experience |
|--------|---------|-------------------|
| DPS cap (15/sec on boss) | No visual indicator | "Why is my Heavy Bolt doing the same as Starting Bolt?" |
| Quick Draw buff (+25% fire rate) | No visual/audio feedback | Players don't know it exists |
| Ability cooldowns | No HUD indicator | "Why can't I air dash?" (0.8s cooldown invisible) |
| Score components | Only visible at level end | Cannot optimize during gameplay |
| Kill streak multiplier | Appears at x3+ but fades quickly | Easy to miss |
| Weapon auto-select reasoning | No indicator for why weapon changed | "Why did my weapon switch?" |
| Hazard type/damage | Gas vs fire have similar visuals | "What just killed me?" |
| Star rating thresholds | Never shown to player | "How do I get 3 stars?" |
| Enemy resistance | 50%/25% damage reduction per epoch gap | "Why is this enemy so tanky?" |

**Recommendation**: For each invisible system, add one of:
- A HUD element (boss HP bar, ability cooldown ring, score counter)
- A visual effect (weapon glow for Quick Draw, damage numbers showing "RESISTED")
- A tooltip (star thresholds on level complete, score tips)

---

#### CF-3: Boss Encounters Are Underwhelming (Impacts H6, C3)

The boss fight is meant to be the climax of every level. Currently it is:
- **No intro**: Player walks into arena, boss activates silently. No name card, no music change, no camera zoom, no dramatic pause.
- **No health bar**: Player has zero feedback on boss HP or phase thresholds.
- **No arena lockdown**: Player can run past the boss entirely.
- **No phase transition drama**: Speed multiplier changes silently. No visual warning, no telegraph, no "the boss is getting angry" moment.
- **No unique behavior per era**: All 10 bosses (MammothChief through RealityBreaker) use identical AI. The boss type enum exists but is cosmetic only.

**Critical bug found**: The slow-effect/phase-speed interaction is broken. When a boss enters Phase 2 (`_moveSpeed *= 1.2f`), the base speed stored in `_baseMoveSpeed` is never updated. When a slow effect wears off, `UpdateSlow` restores speed to `_baseMoveSpeed` (the Phase 1 value), permanently losing the phase bonus. This means applying Slower to a Phase 2/3 boss makes it permanently slower than intended.

**Additional bugs:**
- Boss contact damage has no cooldown — a cornered player takes damage every physics frame
- DPS cap casts `float` to `int`, truncating fractional damage allowance. At high fire rates with low damage, this blocks valid damage.
- Boss charge telegraph is only 0.2s (boss turns orange). Standard is 0.5-1.0s for readable telegraphs.

**Recommendation**: Boss encounters need a presentation overhaul:
1. Arena lockdown (block exit when boss activates)
2. Boss intro sequence (name card, camera pan, 2s pause)
3. Boss health bar (3-segment for 3 phases, color changes per phase)
4. Phase transition drama (screen flash, brief invuln, speed lines, roar SFX)
5. Fix the slow-effect bug (store phase-adjusted speed as new base)
6. Add per-era boss behaviors (even small variations: MammothChief charges more, CoreSentinel teleports, RealityBreaker spawns minions)

---

#### CF-4: Touch Controls Are Unimplemented (Impacts C4, V12)

The game targets iOS as its primary platform, yet touch controls are entirely disabled. `TouchControlsUI.cs` exists but is not active. The WebGL build is keyboard-only. This means:

- No mobile playtesting has occurred
- No thumb reach zone audit
- No touch latency measurement
- No ergonomic validation
- The entire Mobile UX competency (C4, threshold 90) cannot be evaluated

**Recommendation**: Before the next review cycle, touch controls must be implemented and playtested on at least 2 iOS devices. The touch control layout needs:
- Left thumb: D-pad or virtual joystick
- Right thumb: Jump (A button), Stomp (down), Cycle Weapon (B button)
- Top: Pause button (already exists at 70px touch target, good)
- Ergonomic validation against iOS HIG thumb reach zones

---

### HIGH FINDINGS

#### HF-1: No Object Pooling (Impacts C1, C4, V12)

Every dynamic entity uses `new GameObject()` / `Destroy()`:
- Player projectiles (up to 21/sec with Spreader)
- Enemy projectiles (up to 30+ enemies firing)
- Muzzle flashes (1 per shot)
- Death particles (5-8 per enemy death)
- Tile destruction particles (3-6 per tile break)
- Chain lightning projectiles
- Wall-slide dust (every 0.08s)
- Debris from hazards and pillars
- Boss arena pillars debris (4 per pillar)
- Ability dash afterimages (3 per dash)

**Worst case**: A boss arena battle with Spreader weapon, multiple enemies, and pillar destruction could instantiate 50+ objects per second, each requiring component setup, physics registration, and eventual GC collection.

On WebGL (IL2CPP/Emscripten), GC pauses are more severe than native. On iPhone 11 at 60fps, a 16ms frame budget leaves zero room for GC stutters.

**Recommendation**: Implement a generic object pool for the 3 highest-frequency object types:
1. Projectiles (player + enemy)
2. Particles (death + destruction + dust)
3. Muzzle flashes

This alone would eliminate ~90% of runtime instantiation.

---

#### HF-2: The 10 Eras Don't Feel Distinct (Impacts V9, V10, H5)

The era system is the game's core identity ("Contra meets Spelunky across civilization"). But currently:

**Visuals that DO change per era:**
- Tile colors (palette per epoch) ✓
- Enemy colors (palette per epoch) ✓
- Projectile shapes (per epoch) ✓
- Sky gradient colors ✓

**Visuals that DON'T change per era (but should):**
- Parallax background silhouettes (mountains/buildings/structures) — hardcoded same for all eras
- Parallax fog/structure colors — `GetEraBackgroundColors()` ignores epoch parameter
- Platform sprites — always gray stone regardless of era
- Hazard visuals — same green/orange/grey for all eras
- Boss appearance — cosmetic era names in enum but identical sprites

**Audio that doesn't change per era:**
- Only 2 music tracks (title + gameplay) for all 10 eras
- No era-specific instrument tones or tempos
- No ambient sounds (wind, machinery, cosmic hum)
- No boss battle music

**Mechanics that don't change per era:**
- All bosses have identical AI (charge + shoot + spread)
- Hazard types are the same across eras
- No era-specific enemy behaviors
- No era-specific environmental mechanics

**Recommendation** (prioritized by impact):
1. Era-specific parallax silhouettes (mountains → pyramids → castles → factories → skyscrapers → circuit boards → space stations → crystals)
2. Era-specific music variations (same progression but different instruments/tempo per era, or 3-4 music tracks instead of 1)
3. Boss AI variations per era (even 2-3 variants would help)
4. Platform sprites using era palette

---

#### HF-3: Weapon Balance Is Skewed (Impacts C3)

DPS analysis across weapon types at Starting tier:

| Weapon | Fire Rate | Damage | Raw DPS | Special | Effective DPS |
|--------|-----------|--------|---------|---------|---------------|
| Bolt | 0.25s | 1.0x | 4.0 | None | 4.0 (baseline) |
| Piercer | 0.30s | 0.7x | 2.3 | Pierce 2 | 4.6 (vs 2+ in line) |
| Spreader | 0.35s | 0.8x×3 | 6.9 | 3 projectiles | **6.9 (best)** |
| Chainer | 0.40s | 0.6x | 1.5 | Chain 1 | 3.0 (vs 2 grouped) |
| Slower | 0.50s | 0.5x | 1.0 | 3s slow | 1.0 (utility only) |
| Cannon | 0.60s | 3.0x | 5.0 | Break all, heat | 3.3 (after heat) |

**Issues:**
- **Spreader is strictly dominant** for DPS in almost all situations. There is little reason to use other weapons for damage.
- **Chainer at Starting tier is useless** — 1 chain at 60% damage is worse than Bolt in every scenario.
- **Cannon vs Boss**: The 15 DPS cap means Cannon and Bolt perform identically against bosses, but Cannon has heat management overhead. Cannon is only valuable for its material-breaking ability, but this is a traversal tool not a combat weapon.
- **Slower is punishing to use**: 1.0 DPS means the player's damage output drops 75% while using it. Against non-bosses, there is no reason to slow enemies.
- **Weapon tiers don't scale special abilities**: Spreader is always 3 projectiles. Chainer always has 4-unit range. Slower always 50% slow. Only base stats change. Upgrading a weapon should make its special ability better.

**Recommendation:**
1. Scale special abilities with tier: Spreader 3→4→5 projectiles, Chainer 1→2→3 bounces, Slower 50%→60%→75% slow
2. Reduce Spreader base DPS (0.6x per projectile instead of 0.8x)
3. Give Cannon bonus damage vs bosses (ignore DPS cap partially, or deal "structural damage" that bypasses the cap)
4. Make Chainer Starting tier chain 2 enemies (not 1)
5. Give Slower a secondary effect (slowed enemies take 15% more damage from all sources)

---

#### HF-4: Scoring System Is Opaque (Impacts H7)

The 5-component scoring system is well-designed on paper. But the player experience is:
1. Play level. No real-time score feedback.
2. Reach end. See total score.
3. See star rating. Don't know thresholds.
4. Maybe click "SHOW DETAILS" (collapsed by default). See breakdown.
5. Still don't understand what to do differently.

**Specific issues:**
- No running score on the HUD during gameplay
- Kill streak indicator (x3, x4...) fades too quickly and is placed at center-bottom where combat action occurs, easy to miss
- "Combat Mastery" sub-components (kill efficiency, no-damage streak, boss bonus) are opaque — player cannot see shot accuracy in real time
- Archaeology score rewards preservation but the HUD counter says "RELICS" without context
- Star rating thresholds are never communicated
- No personal best comparison on level complete
- No "you improved by X points" on replay

**Recommendation:**
1. Add a running score counter to the HUD (top-center, updates on every score event)
2. Show kill streak prominently near the player (bigger text, 3s duration, screen-edge flash on x5+)
3. Show star thresholds on level complete ("Next star at 13,300 — you need 1,200 more")
4. Show personal best comparison ("New record! +2,400 over previous best")
5. First-time level complete: show details expanded by default
6. Add mini-tooltips to each score component on the details panel

---

#### HF-5: DifficultyProfile May Be Dead Code (Impacts H6)

The `DifficultyProfile.cs` defines epoch-based difficulty scaling, but there is evidence it may not be fully integrated:

- `LevelGenerator.cs` uses `8 + (intensity * 60)` for enemy count, where `intensity` is seed-derived (0.0-1.0), NOT epoch-based
- Enemy HP/speed/shoot% scaling may be applied in `EnemyBase.Initialize()` and `LevelLoader`, but the generator itself doesn't reference DifficultyProfile for enemy counts
- If the generator ignores the profile, a Stone Age level with high intensity could have MORE enemies than a Transcendent level with low intensity

**Recommendation**: Verify DifficultyProfile integration end-to-end. Ensure epoch is the primary difficulty driver, with seed intensity as secondary variation within an epoch.

---

### MEDIUM FINDINGS

#### MF-1: Camera System Lacks Polish (Impacts C1)

- **Lerp is not framerate-independent**: `Mathf.Lerp(current, target, speed * Time.deltaTime)` produces different behavior at 30fps vs 120fps. Correct formula: `1 - Mathf.Exp(-speed * Time.deltaTime)`.
- **No vertical look-ahead**: Player cannot peek downward before jumping into a pit. This is critical for a platformer with destructible floors.
- **No boss arena camera lock**: Camera follows player past arena walls, showing preceding corridor during boss fights.
- **No look-down/look-up mechanic**: Hold down to pan camera downward for scouting.
- **Shake uses `Random.Range`**: Not deterministic, feels jittery. Perlin noise or sine-based trauma shake would feel more natural.

#### MF-2: Tutorial Doesn't Teach Abilities (Impacts H6)

- Double Jump (epoch 3+) and Air Dash are acquired from pickups but never explained
- No HUD indicator shows the player has an ability or when it's on cooldown
- The `AbilityPickup` dynamically adds an `AbilitySystem` component. If `PlayerController` caches a null reference at Start(), the ability may never function (**potential integration bug**)
- Tutorial does not teach weapon cycling (the most strategically important action)

#### MF-3: Health System Issues (Impacts C1)

- **Damage lock scenario**: Player cornered against enemy → take damage → i-frames expire → immediate damage → repeat. No knockback escape guarantee.
- **No health regeneration or shield mechanic**: Despite `RewardType.Shield` existing in enum, HealthSystem has no shield implementation.
- **Kill plane at y < -5f is hardcoded**: Not relative to level bounds. Levels with low terrain could trigger premature death.
- **Boss reset on respawn**: If boss was already killed and player dies to a leftover hazard, boss respawns alive.
- **Death animation uses `Time.deltaTime`**: Would freeze if `Time.timeScale` is ever set to 0.

#### MF-4: Tile Destruction Lacks Feedback (Impacts C2)

- No damage states for destructible tiles (no cracks at 50% HP)
- No "about to break" visual for indestructible tiles taking multiple hits
- All tile types spawn identical particles regardless of material
- No chain-reaction delay — cascading destructions happen simultaneously instead of creating a satisfying domino effect
- Hard tiles should break more dramatically than soft tiles

#### MF-5: PlayerController Has Wall-Slide Feel Issues (Impacts C1)

- **Wall-grab kills upward momentum**: When jumping past a wall and touching it, `_velocity.y` is capped to 4 (WALL_SLIDE_SPEED). If the player jumps with velocity 12 and brushes a wall, they lose 67% of their upward speed instantly. This feels like hitting an invisible ceiling.
- **No stomp initiation air-stall**: Many platformers pause briefly at ground pound start for aim time.
- **Stomp bounce is fixed**: Should reward higher bounces for stomping enemies from greater heights.
- **No variable stomp damage**: Higher falls should deal more damage.
- **Wall dust uses projectile sprite**: Visual placeholder that breaks aesthetic consistency.

#### MF-6: Achievement Design Gaps (Impacts H7)

- **Achievement unlock is silent**: `OnAchievementUnlocked` event fires but no UI subscribes. Zero dopamine response.
- **HazardDodger trivially earned**: Unlocks if player takes no hazard damage, even on levels with zero hazards.
- **CoolUnderPressure trivially earned**: Unlocks without ever firing the Cannon (just pick it up).
- **No hidden achievements**: All visible from start, removing discovery moments.
- **No tiered cumulative achievements**: Only 2 levels (10/50 completions). Missing intermediate milestones.
- **No post-unlock value**: Achievements don't grant anything (skins, bonuses, titles).

---

### LOW FINDINGS

#### LF-1: Code Architecture Concerns

- **Singleton web**: GameManager, CameraController, CheckpointManager, AudioManager, AchievementManager, TutorialManager — all singletons with cross-references. Testing is impossible.
- **`FindWithTag`/`FindAnyObjectByType` fallbacks**: Used throughout (GameplayHUD, HazardSystem, TutorialManager, PlayerController). These run every frame when the target is temporarily null (e.g., during respawn).
- **PlaceholderAssets uses `Color[]` instead of `Color32[]`**: 4x memory overhead on every sprite creation.
- **LevelData is mutable**: TilemapRenderer modifies LevelData in-place when tiles are destroyed. Shared mutable state between renderer and GameManager.
- **Tile asset leaks**: `ScriptableObject.CreateInstance<Tile>()` called per level load, old instances never destroyed.
- **`Invoke(nameof(...))` used in multiple files**: String-based reflection that silently fails on rename. Coroutines would be safer.
- **Empty catch blocks**: GameManager has 3 empty catch blocks that silently swallow errors.

#### LF-2: Memory Leaks on WebGL

- Tile ScriptableObject instances created per level load, never destroyed
- Sprite Texture2D objects cached forever, never cleared between levels/eras
- ParallaxBackground creates 15 texture objects (5 layers × 3 tiles × 640×256 = ~24.6MB) that persist
- AudioClip objects cached forever

#### LF-3: UI Inconsistencies

- Mixed font approaches: `PlaceholderAssets.GetPixelTextSprite()` for pixel art text on title screen vs Unity `Text` components on HUD/level complete. Visual inconsistency.
- Star ratings displayed as `***` / `**-` / `*--` (asterisks and dashes) instead of actual star graphics.
- No gamepad/controller navigation support in any menu.
- Dev-only "LEVEL SELECT" button is exposed to all users with no build flag guard.
- All UI lacks animation — instant show/hide with no transitions.

---

## Beyond-Rubric Analysis

### Fun Audit: What Creates Joy? What Creates Frustration?

**Joy sources (currently working):**
- Cannon blasting through "indestructible" walls → satisfying power fantasy
- Wall-jump chimney climbing → skilled movement feels fluid
- Finding hidden paths behind destructible walls → exploration reward
- Kill streak building → tension and reward escalation
- Clearing a dense combat zone → relief and accomplishment

**Frustration sources (need fixing):**
- Boss DPS cap making upgraded weapons feel wasted
- Getting hit by invisible/untelegraphed hazards
- Weapon auto-switching without explanation
- Dying to boss and boss fully resetting (no progress retention feel)
- Not knowing why score was low (opaque scoring)
- Abilities disappearing between levels with no explanation
- Enemies shooting with zero telegraph
- Getting stuck in a damage loop against a wall

**Missing joy sources (biggest opportunities):**
- Boss intro drama → anticipation and spectacle
- Chain reaction destruction → cascading tile breaks with timing delay
- "Near death" escape → no close-call celebration
- Combo system → hit streak building
- Discovery moments → hidden achievements, secret areas with unique rewards
- Mastery moments → wall-jump shortcuts that feel clever
- Rivalry → no ghost data, no "friend's score" comparison

### Creativity Audit: Generic vs. Distinctive

**What makes Epoch Breaker distinctive:**
- Zero imported assets (everything generated at runtime) — technically unique
- 10 eras of civilization as a progression theme — narratively distinctive
- Destructible environments as core mechanic (not decoration) — mechanically distinctive
- Deterministic level codes for sharing — socially distinctive

**What feels generic:**
- Enemy behaviors (patrol, chase, turret, flying) — standard platformer archetypes with no era flavor
- Boss fight (charge + shoot + shield) — identical across 10 thematically different eras
- Reward types (health, speed, damage, shield, coin) — standard power-up roster
- Music (2 tracks for entire game) — no era personality in audio
- Level flow (flat left-to-right) — no verticality, no branching paths

**Creative proposals to make the game more memorable:**
1. **Era Signature Mechanics**: Each era introduces one unique mechanic. Stone Age: unstable terrain (more FallingDebris). Medieval: portcullis gates that block paths until destroyed. Industrial: conveyor belt tiles. Digital: glitch tiles that randomize when shot. Transcendent: gravity reversal zones.
2. **Destruction Chain Reactions**: When Cannon destroys a load-bearing tile, tiles above collapse with a domino delay. Skilled players trigger controlled demolitions to clear entire structures.
3. **Boss Arena Destruction**: Boss attacks damage the arena itself. Charge attack breaks floor tiles. Spread shot embeds into walls. The arena degrades over the fight, creating new hazards and new strategies.
4. **Narrative Crumbs**: Short era-intro text cards before each epoch ("1200 BC: The Bronze Age. Walls of copper and tin stand between you and the colossus..."). Zero code, maximum context.
5. **"Overkill" Moments**: When the player vastly over-destroys (Cannon into soft terrain), play a distinctive satisfying crumble sound and slow-mo the destruction briefly.

### Gamification Audit: Reward Loops and Progression

**Current reward loop:**
```
Play level → Score → Stars → Achievement? → Next level
```

**What's missing from the loop:**
- **Short-term rewards (per-action)**: Score popup on kills, combo counter growth, visual feedback on efficiency
- **Medium-term rewards (per-level)**: Personal best indication, "new record!" celebration, unlockable content
- **Long-term rewards (per-session)**: Campaign progress display, lifetime stats, cosmetic unlocks
- **Social rewards**: Friend comparison, shared level leaderboards, daily challenge competition

**Gamification improvements:**
1. **Score popups**: Small floating numbers on enemy kills showing points earned
2. **Combo system**: Successive kills within 2 seconds build a multiplier. x2, x3, x4... Display prominently. Reset on taking damage. Feeds into Combat Mastery score.
3. **Personal best tracking per level code**: Show on level complete + title screen history
4. **"One more run" hook**: After game over, show "Best: Epoch 7. Try again?" with a single tap restart
5. **Progress visualization**: Campaign map showing 10 eras as a timeline with checkmarks
6. **Daily challenge**: Already designed, high priority for retention. Single daily seed, global leaderboard.

### Juice Audit: Sensory Reward Gap Analysis

| Action | Current Feedback | Ideal Feedback |
|--------|-----------------|----------------|
| Kill enemy | Enemy vanishes + particles | Hit-stop (30ms) + particles + score popup + SFX variation |
| Destroy tile | Tile removed + 3-6 particles | Screen shake (if Cannon) + particles scaled by material + crumble sound |
| Take damage | Alpha flicker + knockback | Screen shake + red flash + damage direction indicator + heartbeat SFX |
| Pick up weapon | Weapon added silently | Flash + "Got Spreader!" text + unique weapon SFX |
| Boss enters phase | Speed changes silently | Screen flash + boss roar + brief slow-mo + "Phase 2!" indicator |
| Stomp enemy | Fixed 6f bounce | Variable bounce (height-based) + splat SFX + screen shake |
| Level complete | UI appears instantly | Slow-mo on goal touch + victory fanfare + score tally with sounds |
| Achievement unlock | Nothing | Toast notification + achievement SFX + brief celebration |
| Kill streak x5+ | Small text appears | Screen border flash + "UNSTOPPABLE" text + crowd roar SFX |

### Player Psychology: Flow State Analysis

**Flow barriers (things that break the player's flow state):**
1. Boss fully resetting on death — progress feels lost
2. Auto-weapon-switch mid-combat — jarring and uncontrollable
3. Invisible DPS cap — effort feels wasted
4. No mid-level score feedback — no motivation during gameplay
5. Tutorial Level 3 density — too many new mechanics at once
6. Hazards with no telegraph — feel unfair

**Flow enablers (things that support flow, keep or amplify these):**
1. Auto-fire removes a button from the player's cognitive load — good for mobile
2. Coyote time + jump buffer — forgiving timing
3. Wall-slide as safety net for missed jumps
4. Checkpoint system prevents excessive repetition
5. 2-5 minute level length is perfect for mobile sessions

---

## Evidence Gaps

| Required Evidence | Status | Impact |
|-------------------|--------|--------|
| Mobile playtest notes (any device) | **Missing** | Cannot evaluate C4 |
| Score component distribution telemetry | **Missing** | Cannot verify H7 balance |
| Boss fight completion rate data | **Missing** | Cannot evaluate H6 pacing |
| Touch ergonomic audit | **Missing** | Cannot evaluate C4 |
| 30fps vs 60fps feel comparison | **Missing** | Cannot verify framerate sensitivity |
| Level generation fairness audit (100+ seeds) | **Missing** | Cannot verify H5 fairness |
| Player retention data | **Missing** | Cannot evaluate H8 |

---

## Bugs Found During Review

| # | Severity | Location | Description |
|---|----------|----------|-------------|
| B1 | **Critical** | Boss.cs | Slow effect permanently reduces boss speed after phase transition (base speed not updated on phase change) |
| B2 | **High** | Boss.cs | Contact damage has no cooldown — cornered player takes damage every physics frame |
| B3 | **High** | Boss.cs | DPS cap truncates float→int, blocking fractional damage allowance |
| B4 | **High** | AbilityPickup.cs | Dynamically adds AbilitySystem component — PlayerController may not discover it if reference cached at Start() |
| B5 | **High** | WeaponSystem.cs | FindTarget() uses previous weapon's range for current weapon's selection |
| B6 | **High** | BossArenaTrigger.cs | If boss is null when player enters, trigger is consumed and boss never activates (soft-lock) |
| B7 | **Medium** | HealthSystem.cs | Boss reset on respawn — if boss was already defeated, respawn brings it back |
| B8 | **Medium** | ArenaPillar.cs | Invoke(ResetColor) fires after Destroy(gameObject) — Unity warning spam |
| B9 | **Medium** | PlayerController.cs | Wall-grab caps upward velocity to 4f, killing 67% of jump momentum |
| B10 | **Medium** | CameraController.cs | Lerp is not framerate-independent (different feel at 30fps vs 120fps) |
| B11 | **Medium** | HealthSystem.cs | Kill plane at y < -5f is hardcoded, not relative to level bounds |
| B12 | **Low** | TilemapRenderer.cs | Tile asset ScriptableObjects leak on level reload (never destroyed) |
| B13 | **Low** | LevelGenerator.cs | `IsDestructibleTile` includes Indestructible — semantic naming error |
| B14 | **Low** | TutorialManager.cs | `Destroy(this)` removes component but leaves orphan GameObject |
| B15 | **Low** | WeaponData.cs | HeatPerShot field is dead data — never consumed by heat system |
| B16 | **Low** | AchievementManager.cs | HazardDodger unlocks on levels with zero hazards |
| B17 | **Low** | AchievementManager.cs | CoolUnderPressure unlocks without ever firing Cannon |
| B18 | **Low** | LevelLoader.cs | Unused XORShift64 rng variable in ability placement code |

---

## Recommendation Status

**Proceed with Conditions**

The game has strong technical foundations and a distinctive identity. However, it does not yet pass any competency threshold. The conditions for the next cycle:

1. **Fix all Critical and High severity bugs** (B1-B6)
2. **Implement the juice pass** (CF-1) — this is the single highest-impact change
3. **Make 3 invisible systems visible** (CF-2) — boss HP bar, ability indicators, running score
4. **Boss encounter presentation overhaul** (CF-3) — intro, health bar, phase transitions
5. **Implement object pooling** for projectiles and particles (HF-1)
6. **Differentiate at least 3 eras** beyond palette swaps (HF-2) — background silhouettes + music variations

---

## Next Actions

1. Document this review and implementation plan to `docs/` (preserving history per workflow rule)
2. Fix critical/high bugs first (B1-B6)
3. Implement juice pass (screen shake, hit-stop, particles, achievement toasts)
4. Boss encounter overhaul (health bar, intro, phase transitions)
5. Object pooling for projectiles and particles
6. Era differentiation (backgrounds, music)
7. Score visibility (HUD counter, star thresholds, personal best)
8. Weapon balance adjustment
9. Run 3 playtests targeting: boss fights, weapon switching, and era progression feel
10. Re-evaluate all 12 competencies after implementation

---

*This report covers the game as of v0.5.0 build 011 (2026-02-07). Full codebase was audited. No code changes are included in this review.*
