# Implementation Plan: v0.4.0 (Review Cycle 2)

**Date**: 2026-02-06
**Review source**: [Expert-Review-Report-v0.3.0.md](Expert-Review-Report-v0.3.0.md)
**Response**: [Expert-Review-Response-v0.3.0.md](Expert-Review-Response-v0.3.0.md)
**Expert reply**: [Expert-Review-Response-v0.3.0-Expert-Reply.md](Expert-Review-Response-v0.3.0-Expert-Reply.md)
**Status**: Approved with refinements — ready for implementation

---

## Design Decisions (Locked)

These decisions are resolved. No further discussion needed.

| # | Decision | Source |
|---|----------|--------|
| D1 | Auto-select allows Bolt default + Piercer in corridors + Slower on boss. All other weapons manual only. | Expert reply Q1 |
| D2 | Quick Draw buff: +25% fire rate for 1.5s on manual weapon switch. | Response proposal, approved |
| D3 | Remove 10-second manual override timer. Manual selection persists until player switches again. | Response proposal, approved |
| D4 | Auto-switch from Cannon to Bolt on overheat (safety fallback). | Response proposal, approved |
| D5 | Remove low destruction bonus (1,000 points for <30% destruction). Keep relic integrity only. | Expert reply Q2 |
| D6 | Preservation cap reduced from 3,000 to 2,000. Renamed "Archaeology." Language: "relics recovered." | Expert reply Q2 + refinement |
| D7 | Remove Phase 3 shield entirely. | Expert reply Q3 |
| D8 | Reduce minimum phase duration from 8s to 5s. Keep DPS cap at 15/s. | Response proposal, approved |
| D9 | Add destructible arena pillars in Phase 3 only. Inert in Phases 1-2. Spawned by LevelLoader (not LevelGenerator). | Expert reply Q3 + implementation review |
| D10 | Pillar break is telegraphed (crack visual). Debris spawns staggered (not simultaneous with break). | Expert reply refinement |
| D19 | Boss pillar behavior: boss becomes stationary behind pillar (no retreat AI). Player must break pillar to expose boss. Simpler than pathfinding approach. | Implementation review |
| D20 | Destruction particles use manual velocity/gravity (not Rigidbody2D) to avoid WebGL overhead. Match wall-slide dust pattern. | Implementation review |
| D21 | Star rating thresholds set as post-sprint calibration after all scoring changes stabilize. Not baked into Sprint 2. | Implementation review |
| D11 | Cap hazard density at 30% (epoch 7+). No scan pulse this cycle. | Expert reply Q4 |
| D12 | Increase CoverWall ratio in late-epoch hazard pools. | Expert reply refinement |
| D13 | Cannon destruction triggers FallingDebris on tiles above (40% chance). Add crack pre-telegraph. | Expert reply Q5 |
| D14 | No extra Cannon heat for reinforced materials this cycle. | Expert reply Q5 |
| D15 | Score breakdown collapsed by default. Single "details" toggle. Show top 2 contributors as preview. | Expert reply Q6 |
| D16 | In-game kill streak indicator (x3, x4...) near player, fades after 2s. | Response proposal, approved |
| D17 | Tutorial Level 3: optional wall-jump side path with weapon pickup reward. Hint triggers only if player lingers. | Expert reply Q7 + refinement |
| D18 | Minimal visual polish: screen shake, hit flash, destruction particles, muzzle flash. No sprite animations. | Expert reply Q7 |

---

## Sprint Plan

### Sprint 1: Limited Auto-Select + Quick Draw

**Goal**: Make manual weapon cycling rewarding while keeping auto-select approachable for new players.

**Files to modify**:
- `WeaponSystem.cs` — Rewrite `SelectBestWeapon()`, add Quick Draw buff, remove override timer

**Changes**:

1. **Rewrite `SelectBestWeapon()`** (currently lines 152-167):
   - Default: always Bolt
   - Exception 1: If 2+ enemies are in a line (within 8 tiles forward, vertical spread < 2 tiles) AND Piercer is acquired → auto-select Piercer
   - Exception 2: If boss is active AND Slower is acquired → auto-select Slower
   - No other auto-selections. Chainer, Spreader, Cannon never auto-selected.

2. **Add Quick Draw buff**:
   - New field: `_quickDrawTimer` (float, decrements per frame)
   - On `CycleToNextWeapon()`: set `_quickDrawTimer = 1.5f`
   - In fire rate calculation: if `_quickDrawTimer > 0`, multiply fire rate by 0.75 (25% faster)
   - Visual: brief weapon glow or flash on switch (reuse existing sprite tinting)

3. **Remove 10-second manual override timer** (currently line 14, `MANUAL_OVERRIDE_DURATION = 10f`):
   - Delete the timer. Manual selection persists until next cycle.
   - Keep the overheat fallback: if current weapon is Cannon and overheat triggers, auto-switch to Bolt.

**Verification**: Manual cycling should feel snappy. Auto-select should handle only obvious cases (corridor Piercer, boss Slower). Cannon should never be auto-selected.

---

### Sprint 2: Preservation Reframe to Archaeology

**Goal**: Remove the "don't destroy" penalty. Relic preservation becomes an optional discovery bonus.

**Files to modify**:
- `GameManager.cs` — Modify `CalculatePreservation()`
- `LevelCompleteUI.cs` — Rename label, add framing text
- `GameplayHUD.cs` — Update relic counter label

**Changes**:

1. **Modify `CalculatePreservation()`** (currently lines 708-732):
   - Remove low destruction bonus (lines 724-729): delete the `BlocksDestroyed / TotalDestructibleTiles < 0.3f` check and +1,000 bonus
   - Keep relic integrity: `(preserved / total) * 2000`
   - New max: 2,000 (was 3,000)

2. **Rename UI labels**:
   - Level complete screen: "Preservation" → "Archaeology"
   - Color stays gold
   - Add subtitle line: "Relics recovered: X/Y" (replaces generic preservation text)

3. **HUD relic counter**: Change label from "RELICS" to "RELICS RECOVERED" (or icon-based if space is tight)

4. **Star rating adjustment**: With max score dropping from ~20,000 to ~19,000:
   - 3 stars: >= 13,300 (70% of 19,000) — was 14,000
   - 2 stars: >= 7,600 (40% of 19,000) — was 8,000

**Verification**: Destroying everything except relic tiles should feel rewarding, not penalized. Relic counter should read as "bonus found" not "bonus lost."

---

### Sprint 3: Boss Arena Pillars

**Goal**: Replace artificial Phase 3 shield with a destruction-based counterplay mechanic.

**Files to modify**:
- `Boss.cs` — Remove Phase 3 shield, add pillar cover behavior
- `LevelLoader.cs` or `LevelGenerator.cs` — Generate pillar tiles in boss arena
- `PlaceholderAssets.cs` — Pillar sprite (reinforced stone column)
- `PlaceholderAudio.cs` — Pillar crack and collapse SFX
- `HazardSystem.cs` — Pillar debris effect (reuse FallingDebris)

**Changes**:

1. **Remove Phase 3 shield** (Boss.cs lines 396-407, 478):
   - Delete `_shieldTimer`, `_shieldActive` fields
   - Delete the `amount = Mathf.Max(1, amount / 3)` damage reduction in `TakeDamage()`

2. **Reduce min phase duration** (Boss.cs line 50):
   - `MIN_PHASE_DURATION`: 8.0f → 5.0f

3. **Generate arena pillars** (in boss arena zone):
   - Place 2-3 reinforced stone pillars (2 tiles tall, 1 tile wide) in the boss arena zone
   - Pillars are inert in Phases 1-2 (destructible but boss doesn't use them)
   - Pillar positions: evenly spaced across arena width, on ground level

4. **Phase 3 pillar cover behavior** (Boss.cs):
   - New Phase 3 state: `PillarCover`
   - Every 6-8 seconds in Phase 3, boss retreats behind nearest intact pillar for 2 seconds
   - While behind pillar, projectiles hit the pillar instead of the boss
   - Player must destroy pillar to expose boss (Cannon: 1 hit, others: 4-5 hits)
   - Boss resumes attack pattern after pillar is destroyed or 2 seconds elapse

5. **Pillar destruction telegraph** (D10):
   - When pillar takes damage past 50% HP: add crack overlay (visual crack lines on sprite)
   - When pillar is destroyed: 0.3s crack animation, then FallingDebris spawns above (staggered, not instant)
   - Debris deals 2 damage in 1.5-tile radius (existing FallingDebris behavior)

6. **Pillar sprite**: 64x128 reinforced stone column, darker than surrounding arena tiles, with visible structural lines. PPU=64 (1x2 tiles).

**Verification**: Phase 3 should feel like "break the boss's cover" rather than "wait for shield to drop." Cannon users can break pillars instantly. Other weapons take 4-5 shots. Boss should retreat to a different pillar each time if multiple remain.

---

### Sprint 4: Hazard Density Cap + Pool Rebalance

**Goal**: Reduce late-epoch frustration by capping hazard density and shifting toward less punishing hazard types.

**Files to modify**:
- `LevelGenerator.cs` — Modify `AssignHazards()` formula and pool weights

**Changes**:

1. **Cap hazard density at 30%** (LevelGenerator.cs line 1949-1950):
   - Change: `float hazardChance = Mathf.Min(0.30f, 0.05f + epoch * 0.039f);`
   - Effective range: 5% (epoch 0) → 28% (epoch 6) → 30% cap (epoch 7-9)

2. **Increase CoverWall weight in late epochs**:
   - Epoch 0-4: Current pool weights unchanged
   - Epoch 5-6: Add CoverWall to all zone pools at 20% weight
   - Epoch 7-9: CoverWall at 35% weight in all pools
   - This dilutes the proportion of damage hazards (gas, fire, spikes) in late game

3. **Ensure hazard placement respects traversal** (expert refinement):
   - Existing safety: hazards never placed in 4-tile walking corridor above ground
   - Add: hazards never placed on tiles adjacent to checkpoints (2-tile buffer)
   - Add: hazards never placed on tiles adjacent to weapon pickups (1-tile buffer)

**Verification**: Epoch 9 should feel dangerous but navigable. CoverWalls should be the most common hazard type in late epochs, creating strategic cover decisions rather than damage punishment.

---

### Sprint 5: Cannon Triggers FallingDebris

**Goal**: Make Cannon destruction consequential — breaking walls has structural consequences.

**Files to modify**:
- `HazardSystem.cs` — Add Cannon debris trigger
- `TilemapRenderer.cs` — Pass weapon type to `DestroyTile()`
- `PlaceholderAssets.cs` — Crack overlay for pre-telegraph

**Changes**:

1. **Cannon debris trigger** (HazardSystem.cs):
   - New method: `OnCannonDestroy(int tileX, int tileY)`
   - For each tile directly above the destroyed tile (up to 3 tiles):
     - 40% chance to become FallingDebris
     - If triggered: show crack overlay for 0.3 seconds (pre-telegraph), then debris falls
   - Does not apply if tile above is air or already a hazard

2. **Pass weapon context to tile destruction**:
   - `TilemapRenderer.DestroyTile()` gets optional parameter: `bool isCannon = false`
   - When Cannon projectile destroys a tile, pass `isCannon: true`
   - `DestroyTile()` calls `HazardSystem.OnCannonDestroy()` when flag is set

3. **Crack pre-telegraph sprite**: Thin white/grey crack lines overlaid on tile sprite. Reuse existing hazard overlay pattern from PlaceholderAssets (similar to FallingDebris crack lines).

**Verification**: Cannon should still feel powerful (breaks everything), but reckless Cannon use creates cascading debris. Skilled players time their movement to avoid debris. The 0.3s telegraph gives reaction time.

---

### Sprint 6: Score Clarity

**Goal**: Make scoring transparent so players understand what drives each component.

**Files to modify**:
- `LevelCompleteUI.cs` — Add collapsed breakdown with toggle, top 2 preview
- `GameplayHUD.cs` — Add kill streak indicator
- `GameManager.cs` — Track and expose sub-component values

**Changes**:

1. **Level complete breakdown** (LevelCompleteUI.cs):
   - Each score component row gets a "details" state (collapsed by default)
   - Single toggle button: "SHOW DETAILS" / "HIDE DETAILS" at bottom of score section
   - When expanded, each component shows sub-lines:
     - Speed: "Completed in Xs — Y points"
     - Items & Enemies: "X items (×multiplier) + Y kills (best streak: Z)"
     - Combat Mastery: "Kill efficiency: X% (Y/Z shots) + No-damage streak: ×W + Boss: B"
     - Exploration: "Hidden content: X/Y + Secret areas: Z"
     - Archaeology: "Relics recovered: X/Y"
   - **Top 2 preview**: Before expanding, show the two highest-scoring components with a brief label next to the score number (e.g., "Combat Mastery: 4,200 — best component")

2. **Kill streak indicator** (GameplayHUD.cs):
   - When `NoDamageKillStreak >= 3`: show "×N" text near player position
   - Gold text, 24pt, fades out over 2 seconds after each kill
   - Resets (disappears) when player takes damage
   - Position: offset (1, 1.5) from player, so it doesn't overlap the character

3. **Expose sub-component values** (GameManager.cs):
   - Add public properties for breakdown display:
     - `KillEfficiencyPercent` (int): `ShotsFired > 0 ? (EnemiesKilled * 100 / ShotsFired) : 100`
     - `BestNoDamageStreak` (already tracked)
     - `HiddenContentPercent` (int)
     - `RelicsRecoveredCount` / `TotalRelicsCount`

**Verification**: A player who just finished a level should be able to understand "I scored high on Combat Mastery because I hit 85% of my shots and had an 8-kill streak." The collapsed view keeps the win moment clean; details are one tap away.

---

### Sprint 7: Tutorial Optional Wall Path

**Goal**: Reduce Tutorial Level 3 density by making wall-jump section optional.

**Files to modify**:
- `TutorialLevelBuilder.cs` — Restructure Level 3 layout
- `TutorialManager.cs` — Adjust step triggers for optional path

**Changes**:

1. **Restructure Tutorial Level 3** (TutorialLevelBuilder.cs):
   - Main path (required): combat section with enemies + weapon tier pickup + stomp section
   - Side path (optional): branches upward at a visible ledge, requires wall-jumping to reach
   - Side path reward: Heavy weapon pickup + bonus relic tile
   - Side path rejoins main path after ~8 tiles
   - Visual cue: glowing ledge sprite (golden tint) at branch point

2. **Linger hint** (TutorialManager.cs):
   - If player is within 3 tiles of the wall-jump branch point for 4+ seconds without jumping, show hint: "See that ledge? Try jumping into the wall!"
   - Hint only triggers once per attempt
   - If player has already completed Tutorial Level 3 before (replay), skip the linger check

3. **Tutorial step adjustment**:
   - Wall-jump step changes from "required" to "optional"
   - Tutorial completion doesn't require the wall section
   - If player completes the wall section, show celebration hint: "Wall-jump mastered!"

**Verification**: A new player can complete Tutorial Level 3 without ever wall-jumping. Players who explore the side path get a tangible reward and learn the mechanic naturally.

---

### Sprint 8: Minimal Visual Polish

**Goal**: Add the minimum visual feedback needed for "crisp" game feel.

**Files to modify**:
- `CameraController.cs` — Screen shake
- `PlaceholderAssets.cs` — Destruction particle sprites, muzzle flash sprite
- `PlayerController.cs` — Hit flash on damage
- `EnemyBase.cs` — Hit flash on damage
- `Boss.cs` — Hit flash on damage
- `WeaponSystem.cs` — Muzzle flash on fire
- `TilemapRenderer.cs` — Destruction particles on tile break
- `GameManager.cs` — Trigger shake events

**Changes**:

1. **Screen shake** (CameraController.cs):
   - New method: `Shake(float intensity, float duration)`
   - Implementation: random offset added to camera position each frame during shake, decaying over duration
   - Triggers:
     - Player takes damage: intensity 0.15, duration 0.2s
     - Boss takes damage: intensity 0.05, duration 0.1s
     - Cannon fires: intensity 0.1, duration 0.15s
     - Hazard triggers (explosion/debris): intensity 0.2, duration 0.25s
   - Performance: no new objects, just a Vector2 offset applied in LateUpdate

2. **Hit flash** (PlayerController.cs, EnemyBase.cs, Boss.cs):
   - On `TakeDamage()`: set `SpriteRenderer.color = Color.white` for 1 frame (0.02s), then restore
   - Simple coroutine or timer-based approach
   - No new sprites or objects needed

3. **Destruction particles** (TilemapRenderer.cs):
   - On `DestroyTile()`: spawn 4 small square GameObjects (8x8 pixels, PPU=64 = 0.125 units)
   - Color: sampled from the destroyed tile's palette
   - Physics: random velocity (2-5 u/s outward, 3-6 u/s upward), gravity 15 u/s^2
   - Lifetime: 0.5s with alpha fade
   - Pool: pre-allocate 20 particle objects, reuse oldest when pool is full
   - Performance budget: max 20 active particles at once

4. **Muzzle flash** (WeaponSystem.cs):
   - On fire: instantiate a small bright sprite (16x16, white/yellow, PPU=64) at fire point
   - Duration: 2 frames (0.04s), then destroy
   - Per weapon type: Cannon flash is larger (32x32), others are 16x16

**Performance guard**: All effects are object-pooled or frame-limited. No particle systems (Unity ParticleSystem). No shader changes. Max simultaneous: 20 debris particles + 1 muzzle flash + 1 screen shake offset.

**Verification**: Breaking a tile should feel crunchy (particles + shake if Cannon). Getting hit should be immediately visible (white flash). Firing should have visual pop (muzzle flash). None of these should cause frame drops on WebGL.

---

## Sprint Dependency Order

```
Sprint 1 (Auto-select)      — independent
Sprint 2 (Archaeology)      — independent
Sprint 3 (Boss pillars)     — depends on Sprint 5 for FallingDebris integration
Sprint 4 (Hazard cap)       — independent
Sprint 5 (Cannon debris)    — independent, but Sprint 3 reuses its debris system
Sprint 6 (Score clarity)    — depends on Sprint 2 (Archaeology rename)
Sprint 7 (Tutorial)         — independent
Sprint 8 (Visual polish)    — independent, but benefits from all other sprints being complete
```

**Recommended order**: 1 → 2 → 4 → 5 → 3 → 6 → 7 → 8

---

## Verification Checklist (Post-Implementation)

- [ ] Manual weapon cycling feels rewarding (Quick Draw buff visible and impactful)
- [ ] Auto-select only picks Bolt, Piercer (corridor), or Slower (boss)
- [ ] Destroying all non-relic tiles doesn't penalize score
- [ ] Relic counter reads as "recovered" not "preserved"
- [ ] Star thresholds updated for 19,000 max
- [ ] Boss Phase 3 uses pillar cover instead of shield
- [ ] Pillar break has crack telegraph and staggered debris
- [ ] Boss Phase 1-2 have no pillar interaction
- [ ] Epoch 7-9 hazard density does not exceed 30%
- [ ] CoverWall is most common hazard type in epoch 7-9
- [ ] Cannon tile destruction triggers overhead crack + debris
- [ ] Level complete shows top 2 components + expandable details
- [ ] Kill streak counter (x3+) visible during gameplay
- [ ] Tutorial Level 3 completable without wall-jumping
- [ ] Optional wall path has linger hint (4s delay)
- [ ] Screen shake on: player damage, boss damage, Cannon fire, hazard trigger
- [ ] Hit flash on: player damage, enemy damage
- [ ] Destruction particles on: tile break (4 squares, 0.5s fade)
- [ ] Muzzle flash on: weapon fire (2 frames)
- [ ] All effects stay within performance budget (no frame drops on WebGL)

---

## Version Targets

- **Version**: v0.4.0
- **Build**: 009
- **After implementation**: Update BuildInfo.cs, BUILD-LOG.md, GAME-DESIGN-REVIEW.md, MEMORY.md
- **After build**: Run 3 playtests per expert evidence requirements (late-epoch hazards, weapon cycling, score distribution)

---

*This plan incorporates all expert refinements from the reply document. Ready for implementation.*
