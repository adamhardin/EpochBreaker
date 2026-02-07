# Expert Review Change Log

Track changes made as a result of expert reviews.

---

## Review Cycle 1 (v0.3.0 build 008) -- 2026-02-06

**Review reference**: [Expert-Review-Report.md](Expert-Review-Report.md)
**Implementation plan**: `.claude/plans/partitioned-noodling-lemur.md`

### Sprint 1: Wall-Grab Formalization

- **Date**: 2026-02-05
- **Change**: Formalized wall-slide and wall-jump as core mechanics
- **Review recommendation**: "Formalize wall-grab -- wall-slide + wall-jump as a core mechanic"
- **Action taken**:
  - Added horizontal raycasts (chest + foot height) for reliable wall detection
  - New states: `_isWallSliding`, `_wallSlideSide`, `_wallJumpLockTimer`, `_wallCoyoteTimer`
  - Wall-slide: activates when airborne + pressing into wall, clamps fall speed to 4 u/s
  - Wall-jump: force away from wall (5 u/s X, 12 u/s Y), input lock for 5 frames
  - Wall-grab at any vertical velocity (can catch walls while ascending)
  - Horizontal velocity zeroed on wall-grab to prevent physics bounce
  - Wall-jump lock clears on new wall-grab (enables chimney climbing)
  - Visual: sprite squish (0.85x wide, 1.1x tall), dust particles while sliding
  - Audio: wall-slide scraping sound via PlaceholderAudio.GetWallSlideSFX()
  - Achievement: WallJumpMaster (10 wall-jumps in one level)
- **Outcome**: Wall mechanics work reliably for climbing between walls and single-wall re-grabs
- **Files modified**: PlayerController.cs, PlaceholderAudio.cs, AchievementManager.cs

### Sprint 2: 6-Slot Weapon System

- **Date**: 2026-02-05
- **Change**: Replaced single-tier weapon upgrade with 6 weapon types, each with utility identity
- **Review recommendation**: "6-slot weapon system -- utility weapons, not just DPS tiers"
- **Action taken**:
  - 6 weapon types: Bolt, Piercer, Spreader, Chainer, Slower, Cannon
  - Each type has Starting/Medium/Heavy tiers affecting base damage and fire rate
  - Piercer: passes through 2-4 enemies
  - Spreader: 3 projectiles in 30-degree cone
  - Chainer: arcs to 1-3 nearby enemies
  - Slower: 3s slow effect (50% speed) on hit
  - Cannon: highest damage, breaks all materials, heat system (overheat lockout)
  - Auto-select AI picks best weapon for situation; manual cycle via Attack button
  - Epoch-based enemy resistance prevents trivializing content
  - New WeaponData.cs with WeaponDatabase and HeatSystem
  - Weapon slot HUD with dots and heat bar
- **Outcome**: Weapon diversity creates meaningful tactical choices
- **Files modified/created**: WeaponData.cs (new), WeaponSystem.cs, Projectile.cs, WeaponPickup.cs, EnemyBase.cs, Boss.cs, LevelData.cs, LevelGenerator.cs, PlaceholderAssets.cs, PlaceholderAudio.cs, GameplayHUD.cs, GameManager.cs

### Sprint 3: Risky Destruction

- **Date**: 2026-02-05
- **Change**: Added environmental hazards making destruction a strategic decision
- **Review recommendation**: "Risky destruction -- hazards make breaking blocks a strategic choice"
- **Action taken**:
  - 6 hazard types: FallingDebris, UnstableFloor, GasRelease, FireRelease, SpikeTrap, CoverWall
  - Hazard chance scales with epoch (5% at era 0, 40% at era 9)
  - Relic tiles: preserved for Preservation score bonus, golden glow border
  - HazardSystem.cs manages runtime hazard effects (area damage, debris physics, etc.)
  - Safety: hazards never placed on primary walkable corridor
  - Visual overlays on hazard/relic tiles
- **Outcome**: Players must evaluate risk before breaking blocks
- **Files modified/created**: HazardSystem.cs (new), LevelData.cs, LevelGenerator.cs, LevelRenderer.cs, LevelValidator.cs, PlaceholderAssets.cs, PlaceholderAudio.cs

### Sprint 4: Scoring Rebalance + Preservation

- **Date**: 2026-02-06
- **Change**: Replaced 3-component scoring with 5-component system supporting multiple play styles
- **Review recommendation**: "Scoring rebalance -- multiple viable play styles (speed, combat, exploration, preservation)"
- **Action taken**:
  - 5 score components (no single component > 30% of max):
    - Speed: max 5,000 (`max(500, 5000 - elapsed*25)`)
    - Items & Enemies: ~3,000 (existing multiplier system)
    - Combat Mastery: max 5,000 (kill efficiency + no-damage streaks + boss bonus)
    - Exploration: max 4,000 (hidden content + secret areas)
    - Preservation: max 3,000 (relic integrity + low destruction bonus)
  - Star ratings now score-based: 3 stars >= 14,000, 2 stars >= 8,000
  - Relic counter on HUD (gold/orange based on preservation status)
  - Level complete UI shows 5-component breakdown with color coding
  - 8 new achievements: Archaeologist, MinimalFootprint, StructuralEngineer, HazardDodger, WeaponCollector, ChainMaster, CannonExpert, CoolUnderPressure
- **Outcome**: Speed, combat, exploration, and preservation are all viable scoring strategies
- **Files modified**: GameManager.cs, LevelCompleteUI.cs, GameplayHUD.cs, AchievementManager.cs, WeaponData.cs, HazardSystem.cs, TilemapRenderer.cs, Boss.cs

### Sprint 5: Onboarding Tutorial

- **Date**: 2026-02-06
- **Change**: Added 3-level tutorial teaching mechanics in sequence
- **Review recommendation**: "Onboarding tutorial system"
- **Action taken**:
  - TutorialLevelBuilder.cs: 3 hardcoded levels as standard LevelData
    - Level 1 (64 tiles): Move & Jump -- flat terrain, gaps, platforms
    - Level 2 (80 tiles): Shoot & Smash -- destructibles, stomp, weapon pickup
    - Level 3 (96 tiles): Climb & Survive -- wall-jumps, hard blocks, enemies
  - TutorialManager.cs: step-based progression with platform-aware hints
  - TutorialHintUI.cs: floating hint panel with fade animation
  - Tutorial auto-starts on first Campaign, skippable
  - Replayable via Settings > REPLAY TUTORIAL (directly starts tutorial)
  - Persistence via PlayerPrefs
- **Outcome**: New players learn mechanics progressively
- **Files modified/created**: TutorialLevelBuilder.cs (new), TutorialManager.cs (new), TutorialHintUI.cs (new), LevelLoader.cs, GameManager.cs, TitleScreenUI.cs

### Sprint 6: Difficulty Curve + Boss Tuning

- **Date**: 2026-02-06
- **Change**: Added epoch-based difficulty scaling and boss fight tuning
- **Review recommendation**: "Difficulty curve and boss phase tuning"
- **Action taken**:
  - DifficultyProfile.cs: epoch-based scaling with linear interpolation
    - Epoch 0: 0.5x enemies, 0.7x HP, 0.8x speed, 40% shoot, 100 boss HP
    - Epoch 9: 1.0x enemies, 1.5x HP, 1.2x speed, 85% shoot, 280 boss HP
  - Enemy HP/speed now scaled via DifficultyProfile in EnemyBase.Initialize()
  - Boss HP from DifficultyProfile (100-280 based on epoch)
  - Boss DPS cap: max 15 damage/second (rolling 1-second window)
  - Minimum phase duration: 8 seconds (prevents burst-skipping)
  - Phase 3 shield: 1/3 damage for 2 seconds after spread shot
  - ResetBoss() on player respawn (full HP, phase, visual state reset)
  - SkillGate zone type added to LevelData
- **Outcome**: Smooth difficulty progression, boss fights respect weapon diversity
- **Files modified/created**: DifficultyProfile.cs (new), LevelData.cs, EnemyBase.cs, Boss.cs, HealthSystem.cs

### Post-Sprint: Quality of Life

- **Date**: 2026-02-06
- **Changes**:
  - Wall-jump tuning: reduced X velocity (8->5), increased Y velocity (11->12), reduced lock frames (8->5)
  - Wall-grab at any vertical velocity (not just while falling)
  - Horizontal velocity zeroed on wall-grab (prevents bounce)
  - Tutorial replay button in Settings directly starts tutorial
  - Build version system: BuildInfo.cs with version/build/date tracking
  - Build ID displayed on title screen (bottom-left)
- **Files modified**: PlayerController.cs, TitleScreenUI.cs, BuildInfo.cs

---

## Review Cycle 2 (v0.4.0) -- Approved, Ready for Implementation

**Review reference**: [Expert-Review-Report-v0.3.0.md](Expert-Review-Report-v0.3.0.md)
**Response document**: [Expert-Review-Response-v0.3.0.md](Expert-Review-Response-v0.3.0.md)
**Expert reply**: [Expert-Review-Response-v0.3.0-Expert-Reply.md](Expert-Review-Response-v0.3.0-Expert-Reply.md)
**Implementation plan**: [Implementation-Plan-v0.4.0.md](Implementation-Plan-v0.4.0.md)
**Status**: Approved with refinements — implementation pending

### Approved Sprints (with expert refinements)

| Sprint | Finding | Summary | Key Refinement |
|--------|---------|---------|----------------|
| 1 | Auto-select reduces agency | Limited auto-select (Bolt + corridor Piercer + boss Slower), Quick Draw buff on manual switch | Not Bolt-only; allow 2 narrow exceptions |
| 2 | Preservation vs destruction | Remove low destruction bonus, cap at 2000, rename to "Archaeology" | Use "relics recovered" language |
| 3 | Boss anti-trivialize stack | Remove Phase 3 shield, reduce min phase 8s→5s, add destructible arena pillars (Phase 3 only) | Pillars inert in Phase 1-2; telegraph crack before debris |
| 4 | Hazard scaling | Cap density at 30%, increase CoverWall ratio in late epochs | No scan pulse this cycle |
| 5 | Cannon traversal | Cannon destruction triggers FallingDebris on tiles above (40% chance) | Add crack pre-telegraph (0.3s) |
| 6 | Score opacity | Collapsed breakdown with "details" toggle, top 2 preview, kill streak indicator | Collapsed by default to preserve win moment |
| 7 | Tutorial wall-jump | Optional wall-jump side path in Level 3 with reward | Hint triggers only if player lingers (4s) |
| 8 | Visual polish | Screen shake, hit flash, destruction particles, muzzle flash | Minimal pass only; defer full animations |

### Implementation Order

1 → 2 → 4 → 5 → 3 → 6 → 7 → 8

### Documentation Changes (Completed Pre-Sprint)

- Fixed engine version in MEMORY.md and PROJECT-STRUCTURE.md (Unity 6000.3.6f1)
- Added dual-project sync workflow and coordinate system diagram
- Expanded known gaps list to match GAME-DESIGN-REVIEW.md
