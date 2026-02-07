# Expert Review Response: v0.3.0 build 008

**Date**: 2026-02-06
**Review reference**: [Expert-Review-Report-v0.3.0.md](Expert-Review-Report-v0.3.0.md)
**Addendum reference**: [Expert-Review-Report-v0.3.0-Memory-Addendum.md](Expert-Review-Report-v0.3.0-Memory-Addendum.md)

This document captures our analysis of each expert finding, the current implementation context, and proposed approaches for the next implementation cycle. Each finding includes a design decision recommendation for expert review before code changes begin.

---

## Addendum Findings (Documentation)

All five documentation findings from the Memory Addendum have been addressed:

| # | Finding | Severity | Resolution |
|---|---------|----------|------------|
| A1 | Engine version conflict (2022.3 vs 6000.3.6f1) | Critical | Fixed. MEMORY.md and PROJECT-STRUCTURE.md now state Unity 6000.3.6f1. API compat notes retained. |
| A2 | Dual-project sync needs documented workflow | High | Added sync procedure, checklist, and scope rules to PROJECT-STRUCTURE.md and MEMORY.md. |
| A3 | Coordinate system inversion needs diagram | High | Added ASCII diagram and conversion checklist to both PROJECT-STRUCTURE.md and MEMORY.md. |
| A4 | Missing visual feedback is a feel risk | Medium | Acknowledged. Carried forward as Finding #9 in the main review. |
| A5 | Known gaps list incomplete | Low | MEMORY.md expanded to list all 9 unbuilt features from GAME-DESIGN-REVIEW.md. |

---

## Critical Findings

### Finding 1: Auto-Select AI Reduces Player Agency

**Expert says**: If auto-select always chooses correctly, the player stops making interesting weapon choices. Add decision tradeoffs so manual cycling matters.

**Current implementation** (WeaponSystem.cs `SelectBestWeapon()`):
- Priority chain: Boss → Slower, 3+ enemies → Chainer/Spreader, 1-2 enemies → Cannon, 2+ → Piercer, default → Bolt
- Manual override lasts 10 seconds, then reverts to auto
- Auto-select runs every frame — always picks the optimal weapon

**Analysis**: The auto-select is too smart. It functions as a solved game — players who let it run will always get near-optimal results, making manual cycling feel pointless. The 10-second override timer also punishes manual play by reverting the player's choice.

**Proposed approach — "Conservative Auto + Quick Draw Buff"**:

1. **Nerf auto-select to "safe default"**: Auto-select only picks Bolt (the starting weapon). It never auto-switches to specialized weapons. This forces players to discover and choose tactical weapons themselves.

2. **Add Quick Draw buff on manual switch**: When the player manually cycles weapons, grant a 1.5-second window of +25% fire rate. This creates a micro-reward for active weapon management and makes switching feel snappy.

3. **Remove the 10-second override timer**: Manual selection persists until the player switches again. No auto-revert.

4. **Keep auto-select for Cannon overheat only**: If the current weapon is Cannon and it overheats, auto-switch to Bolt (safety fallback). This is the one case where auto-help is justified.

**Alternative considered**: Keep smart auto-select but add ammo/heat costs to all weapons. Rejected because it adds resource management complexity that conflicts with the fast-paced arcade feel.

**Design question for expert**: Is "auto-select defaults to Bolt only" too aggressive a nerf? Should it also auto-switch to Piercer in corridors, or is Bolt-only the right starting point?

---

### Finding 2: Preservation Conflicts with Destruction Fantasy

**Expert says**: Preservation score and relics can feel like "do not destroy" penalties. Frame preservation as a separate optional mastery lane rather than a dominant score path.

**Current implementation** (GameManager.cs `CalculatePreservation()`):
- Max 3,000 points out of ~20,000 total (15% of max score)
- Two sub-components: Relic integrity (max 2,000) + low destruction bonus under 30% (max 1,000)
- The low destruction bonus (1,000 points for destroying < 30% of blocks) directly penalizes the destruction fantasy

**Analysis**: The 3,000 cap is already the smallest component, so preservation doesn't dominate scoring. But the *low destruction bonus* (1,000 for < 30% destruction) is the real problem — it actively punishes players who enjoy breaking things. The relic integrity bonus (2,000 for preserving specific golden tiles) is fine because it creates targeted decisions rather than a blanket "don't break" rule.

**Proposed approach — "Relics Stay, Low Destruction Bonus Goes"**:

1. **Remove the low destruction bonus entirely**: No points for avoiding destruction. This eliminates the "don't break things" penalty.

2. **Keep relic integrity scoring at 2,000 max**: Preserving specific marked relic tiles is a targeted, visible decision. Players can break everything *except* the golden tiles and still score well.

3. **Reduce Preservation cap from 3,000 to 2,000**: With the low destruction bonus gone, the cap naturally drops. Preservation becomes 10% of max score (2,000 / 20,000), firmly an optional mastery lane.

4. **Reframe UI language**: Change "Preservation" label to "Archaeology" on the level complete screen. Shift the framing from "things you didn't break" to "treasures you discovered and protected."

5. **Future consideration (not this cycle)**: The expert's "Preservation-as-Discovery" idea (relics reveal lore or mini-challenges) is excellent but requires new UI and content systems. Flag for v0.5.0+.

**Design question for expert**: Is 2,000 points (10% of max) the right weight for relic preservation, or should it go lower? Should the Archaeologist achievement also be reframed?

---

### Finding 3: Boss Anti-Trivialize Stack Feels Padded

**Expert says**: DPS cap (15/s) + minimum phase duration (8s) + Phase 3 shield all stack to make bosses feel artificial. Replace at least one hard cap with a mechanic-driven counterplay.

**Current implementation** (Boss.cs):
- **DPS cap**: Tracks `_recentDamage` in a 1-second rolling window. Damage beyond 15/s is discarded.
- **Min phase duration**: 8 seconds per phase regardless of damage dealt. Phases only transition when HP threshold AND timer are both met.
- **Phase 3 shield**: After spread shot in Phase 3, boss takes 1/3 damage for 2 seconds.

**Analysis**: Three simultaneous hard caps is excessive. The DPS cap (15/s) alone already prevents burst-skipping — it means even with Heavy Cannon (3 damage, 0.5s fire rate = 6 DPS), the boss takes minimum ~7 seconds per phase at epoch 0 (100 HP / 3 phases = 33 HP per phase / 15 DPS = 2.2s per phase... but that's still fast). The min phase duration adds a hard floor, and the Phase 3 shield adds a third layer. Players feel walled by invisible mechanics.

**Proposed approach — "Destructible Arena Pillars"**:

1. **Remove Phase 3 shield entirely**: No more invisible damage reduction.

2. **Keep DPS cap at 15/s**: This is the invisible-but-fair mechanic. It prevents literally one-shotting phases with stored Cannon bursts, but players don't feel it moment-to-moment because weapon fire rates naturally stay below 15 DPS.

3. **Reduce min phase duration from 8s to 5s**: Still prevents instant phase transitions, but feels less padded.

4. **Add destructible arena pillars (new mechanic)**: Boss arena generates 2-3 stone pillars. During Phase 3, the boss periodically retreats behind a pillar (1.5s), which blocks projectiles. Player must destroy the pillar with weapons (Cannon breaks instantly, others take 3-5 hits) to expose the boss. This turns "wait for shield to drop" into "use Cannon to break cover" — playing into the destruction fantasy.

5. **Arena pillars as FallingDebris hazard**: When a pillar is destroyed, it triggers the existing FallingDebris hazard effect, creating a danger zone. Risk/reward: break the pillar to hit the boss, but dodge the falling debris.

**Alternative considered**: Boss "overheat" window tied to weapon choice (expert suggestion). Rejected because it adds UI complexity and is less thematically connected to the game's destruction identity.

**Design question for expert**: Should arena pillars appear in all boss phases (giving destruction-focused players something to break throughout), or only Phase 3 (to replace the shield specifically)?

---

## High Findings

### Finding 4: Hazard Scaling Punishes Late Epochs

**Expert says**: Hazard density scaling (5% to 40%) may over-punish late epochs without fresh tools to match.

**Current implementation** (LevelGenerator.cs `AssignHazards()`):
- Formula: `0.05f + epoch * 0.039f` — linear from 5% to 40%
- Safety: hazards never placed in 4-tile walking corridor above ground
- Zone-specific pools: Combat → debris/spikes, Destruction → debris/gas/fire

**Analysis**: At epoch 9, nearly half of all non-corridor destructible tiles are hazardous. Players have stronger weapons (Cannon breaks everything) but no new *defensive* or *navigation* tools to handle hazards. The designed-but-unbuilt ability progression (dash, phase shift) would help, but isn't in scope.

**Proposed approach — "Cap at 30% + Hazard Visibility"**:

1. **Cap hazard density at 30%** (epoch 7+): Change formula to `min(0.30, 0.05 + epoch * 0.039)`. This still doubles from early to late game but avoids the punishing 40% ceiling.

2. **Add hazard scan pulse (new mechanic)**: When player holds the Attack button for 0.5s without a target in range, emit a brief visual pulse (expanding ring) that highlights hazard tiles within 5 tiles for 3 seconds. This gives players a tool to deal with hazard density without requiring new movement abilities.

3. **Increase CoverWall frequency in late epochs**: CoverWalls are the least punishing hazard (visual indicator only). Shift late-epoch hazard pools to include more CoverWalls and fewer damage hazards.

**Design question for expert**: Is the hazard scan pulse too complex for the current scope? Would simply capping at 30% and adjusting the pool weights be sufficient?

---

### Finding 5: Cannon Trivializes Traversal

**Expert says**: Even with heat, Cannon can flatten the generation's intended structure.

**Current implementation**:
- Cannon breaks ALL materials (soft, medium, hard, reinforced)
- Heat: 10 per shot, 40 max, cools 8/s, 2s lockout on overheat
- 4 shots before overheat, then 2s lockout + ~5s full cooldown

**Analysis**: The heat system already limits Cannon to 4-shot bursts. But in traversal (not combat), the player can fire 4 Cannon shots, wait 5 seconds, fire 4 more — systematically clearing any wall. The question is whether this is a problem. Breaking walls *is* the game's core fantasy, and Cannon is the late-game reward for that. The hazard system already punishes reckless destruction.

**Proposed approach — "Cannon Triggers Hazards More Aggressively"**:

1. **No change to Cannon material breaking**: Cannon should break everything. That's the power fantasy.

2. **Cannon destruction always triggers FallingDebris check**: When Cannon destroys a tile, any tile directly above it has a 40% chance to become FallingDebris (regardless of whether it was marked as a hazard). This creates risk proportional to Cannon's power — breaking load-bearing walls has consequences.

3. **Visual feedback**: Cannon-destroyed tiles produce a larger dust cloud and screen shake (ties into Finding #9 visual polish).

**Design question for expert**: Does "Cannon triggers extra FallingDebris" adequately balance the traversal concern, or should Cannon also generate extra heat when breaking reinforced materials?

---

### Finding 6: Combat Mastery Score is Opaque

**Expert says**: Kill efficiency and no-damage streaks are opaque; players won't understand how to optimize.

**Current implementation** (GameManager.cs `CalculateCombatMastery()`):
- Kill efficiency: (kills / shots) x 2,000 — never shown to player during gameplay
- No-damage streak: best streak x 150, max 1,500 — never shown to player during gameplay
- Boss bonus: flat 1,500 — player knows boss was defeated, but not the score value
- Level complete screen shows "Combat Mastery: X" as a single number

**Proposed approach — "Score Breakdown Tooltips"**:

1. **Expand level complete screen**: Show sub-components for Combat Mastery:
   - "Kill efficiency: 78% (14/18 shots hit) — 1,560"
   - "No-damage streak: x8 — 1,200"
   - "Boss defeated — 1,500"

2. **Add similar breakdowns for all 5 components**: Each score line expands on tap/click to show how it was calculated.

3. **In-game kill streak indicator**: When the player gets 3+ kills without taking damage, show a small counter near the player ("x3", "x4"...) that fades after 2 seconds. This makes the no-damage streak visible in real time.

**Design question for expert**: Should the breakdown be always visible on the level complete screen, or collapsed behind a tap/click to keep the screen clean?

---

## Medium Findings

### Finding 7: Chimney Climbing Enables Sequence Breaking

**Expert says**: Decide if sequence breaking is intended or should be constrained.

**Analysis**: Chimney climbing (wall-jumping between two adjacent walls to ascend vertically) is intentionally enabled by the wall-jump implementation. It rewards skilled players with shortcuts and exploration routes. The current level generation doesn't create many "exploit" opportunities because walls are rarely adjacent in tight shafts.

**Proposed approach — "Intended Feature, Monitor via SkillGate"**:

1. **Keep chimney climbing as-is**: It's a skill expression that rewards mastery.
2. **Ensure SkillGate zones cannot be bypassed via chimney climbing**: SkillGate sections should have overhangs or wide-open spaces above that prevent vertical bypass.
3. **Add achievement**: "Vertical Limit" — climb 30+ tiles using wall-jumps in a single level. Reward the behavior rather than punish it.

---

### Finding 8: Tutorial May Overteach Wall Mechanics Early

**Expert says**: Consider introducing wall jump later for retention rather than in the onboarding block.

**Analysis**: Wall-jump is currently in Tutorial Level 3, the final tutorial level. Level 3 also introduces combat, hard blocks, and enemies — it's the densest tutorial level. The concern is valid: first-time players face wall-jumping + combat simultaneously.

**Proposed approach — "Make Wall Section Optional in Level 3"**:

1. **Split Tutorial Level 3 into two paths**: A main path that teaches combat + weapon tiers (required), and a side path with the wall-jump section (optional, marked with a visual cue like a glowing ledge).
2. **Wall-jump section rewards**: Place a weapon pickup or bonus reward in the optional wall path to incentivize exploration without requiring it.
3. **Tutorial hint**: "See that ledge? Try jumping into the wall!" — curiosity prompt rather than a blocking requirement.

---

## Low Findings

### Finding 9: Missing Visual Feedback Polish

**Expert says**: Animations, destruction particles, and screen shake will dampen fun. This is the #1 most impactful unbuilt feature.

**Analysis**: Every system (wall-slide, weapons, hazards, destruction) would benefit from visual feedback. This is a large scope item that could be its own implementation cycle.

**Proposed approach — "Minimal Feel Pass (scoped)"**:

Priority order for maximum feel impact with minimum implementation:

1. **Screen shake**: CameraController shake on boss hit, Cannon fire, hazard trigger, player damage. Small implementation (add shake offset to camera position, decay over time).

2. **Destruction particles**: When tiles are destroyed, spawn 3-5 small square sprites matching the tile color that scatter with physics and fade over 0.5s. Reuse existing PlaceholderAssets color system.

3. **Hit flash**: Player and enemies flash white for 1 frame on damage. Single-line SpriteRenderer color change.

4. **Weapon muzzle flash**: Brief bright sprite at fire point for 2 frames.

**Not in scope this cycle**: Sprite animations (idle, walk, jump frames), parallax backgrounds, elaborate particle systems.

**Design question for expert**: Should the visual polish pass be part of this implementation cycle (adds 2-3 sprints), or saved for a dedicated v0.4.0 polish cycle?

---

## Creative Direction Responses

### Era Signature Mechanics
**Expert idea**: Each era introduces a hazard archetype that defines its identity.
**Response**: Interesting long-term direction. Current hazard system already has zone-based pools but not era-based pools. Could be implemented by adding era weights to the hazard type selection in `AssignHazards()`. Flagged for future consideration.

### Weapon Archetypes Per Era
**Expert idea**: Encourage certain weapons in each era through environment design.
**Response**: The epoch-based resistance system already nudges weapon choice (enemies 2+ epochs ahead take 50% reduced damage from old weapons). Could extend this by biasing weapon pickup types per era. Flagged for future consideration.

### Preservation-as-Discovery
**Expert idea**: Preserving relics reveals lore or mini-challenges rather than pure points.
**Response**: Excellent concept, requires new UI and content systems. Flagged for v0.5.0+.

---

## Evidence Gaps (Expert Noted)

| Gap | How to Address |
|-----|---------------|
| No qualitative playtest notes | Run 3 playtests after next implementation cycle, document in playtest log |
| No score component telemetry | Add debug logging of score component distribution to GameManager |
| No mobile touch ergonomics audit | Defer until iOS build available; document reach zones when testing on device |

---

## Summary: Proposed Implementation Sprints (Cycle 2)

Pending expert approval, the following sprints are proposed:

| Sprint | Finding | Scope |
|--------|---------|-------|
| 1 | #1 Auto-select nerf | WeaponSystem.cs: Bolt-only auto, Quick Draw buff, remove override timer |
| 2 | #2 Preservation reframe | GameManager.cs: Remove low destruction bonus, cap to 2000, rename to Archaeology |
| 3 | #3 Boss counterplay | Boss.cs: Remove Phase 3 shield, reduce min phase to 5s, add arena pillars |
| 4 | #4 Hazard balance | LevelGenerator.cs: Cap 30%, add CoverWall bias; optional hazard scan pulse |
| 5 | #5 Cannon consequences | HazardSystem.cs: Cannon triggers FallingDebris on tiles above |
| 6 | #6 Score clarity | LevelCompleteUI.cs, GameplayHUD.cs: Sub-component breakdowns, kill streak indicator |
| 7 | #7-8 Tutorial + wall adjustments | TutorialLevelBuilder.cs: Optional wall path in Level 3 |
| 8 | #9 Visual polish (if in scope) | CameraController.cs, PlaceholderAssets.cs: Screen shake, destruction particles, hit flash |

**Version target**: v0.4.0 build 009

---

*Awaiting expert review of proposed approaches before implementation begins.*
