# Epoch Breaker — Development Roadmap

**Created**: 2026-02-06 | **Updated**: 2026-02-08
**Starting from**: v0.6.0 build 012 | **Current**: v1.4.2 build 034

> **Platform note (2026-02-06):** Mobile (iOS) is deprioritized. There is no way to test
> or prepare for mobile at this time. All development targets **Unity Editor** and
> **WebGL browser** with **keyboard controls**. Sprint 7 (Mobile Readiness) is deferred
> to a future release and removed from v0.9.0 scope. Mobile-specific features (haptics,
> safe area insets, touch UX, battery/thermal) are out of scope until a mobile test
> environment is available.

---

## v1.1.0: "Maximize Fun" — Expert Review Improvements

> **Source**: Expert game design review against 12-competency framework (see `docs/EXPERT-REVIEW-v1.0.2.md`).
> Review identified 4 competencies needing remediation: Weapon System (C3), Mobile UX (C4),
> Difficulty & Bosses (H6), Scoring & Achievements (H7). The 12 findings below address all of them.

### Sprint 10: Tutorialization & Onboarding (Critical)

These are the highest fun-per-effort changes. They unlock existing systems that players currently can't discover.

| # | Finding | File(s) | Change |
|---|---------|---------|--------|
| 1 | Weapon cycling never taught | TutorialManager.cs | Add hint in Tutorial Level 2 on first weapon pickup: "Press X to switch weapons!" Show Quick Draw glow as visual incentive |
| 2 | Hazards never introduced | TutorialManager.cs | Add context-sensitive hint on first hazard trigger: "Careful! Breaking some blocks releases hazards." One-time, dismissable |
| 3 | Boss Phase 3 pillar shelter untaught | Boss.cs | Flash shelter pillar with pulsing color when boss hides behind it. Add "Destroy the pillar!" hint on first encounter |
| 4 | No tutorial replay from menu | TitleScreenUI.cs | Add "Replay Tutorial" option in Settings menu |

**Competency impact**: C3 Weapon System (+8), H6 Difficulty & Bosses (+5)

### Sprint 11: Campaign & Difficulty Rebalance

| # | Finding | File(s) | Change |
|---|---------|---------|--------|
| 5 | Campaign too punishing (2 global lives) | GameManager.cs | Remove global lives from Campaign mode. Deaths cost time/score but never cause Game Over. Campaign is the "see everything" mode; Streak remains the challenge mode. Per-level deaths (DEATHS_PER_LEVEL) still apply — dying too many times on a single level skips it |
| 6 | Era 0 boss is boring (charge-only) | Boss.cs | Give Primitive boss a simple telegraphed projectile in Phase 2 (slow speed, long cooldown ~3s). Shorten patrol distance so charges feel more frequent |
| 7 | Teleport Dash has no telegraph | Boss.cs | Add 0.3–0.5s shimmer/fade animation before Renaissance boss teleport. Preserves the visual language of readability |

**Competency impact**: H6 Difficulty & Bosses (+7)

### Sprint 12: Scoring & Combat Polish

| # | Finding | File(s) | Change |
|---|---------|---------|--------|
| 8 | Combo resets on environmental damage | GameManager.cs | Only reset combo on enemy-sourced damage. Hazard chip damage triggers a 1-second grace period instead of immediate reset |
| 9 | Level Complete 5-second mandatory wait | GameManager.cs | Reduce `LevelCompleteMinDelay` from 5.0 to 2.5 seconds |
| 10 | Flying enemies have no satisfying counterplay | PlayerController.cs, EnemyBase.cs | Stomp creates a 2-tile downward shockwave that damages flying enemies. OR: flying enemies briefly pause when hit (0.3s hover), giving auto-aim a better window |

**Competency impact**: H7 Scoring & Achievements (+7), H6 (+3)

### Sprint 13: Weapon Visibility & UI

| # | Finding | File(s) | Change |
|---|---------|---------|--------|
| 11 | Weapon legend shows only 3 of 5 types | TitleScreenUI.cs | Expand legend row to show all 5 weapon type icons with labels (Slower removed in build 032). Players can see what each pickup looks like before playing |
| 12 | README status outdated | README.md | Update status to v1.0.3 build 027, current feature set |

**Competency impact**: C3 Weapon System (+5)

### Sprint 10-13 Dependencies

```
Sprint 10 (Tutorialization)    <- HIGHEST PRIORITY, no dependencies
Sprint 11 (Campaign rebalance) <- Independent, can parallelize with 10
Sprint 12 (Scoring polish)     <- Independent
Sprint 13 (Weapon UI + docs)   <- Independent, lowest effort
```

All sprints are independent and can be implemented in any order or in parallel. Sprint 10 has the highest fun-per-effort ratio.

### Expected Competency Scores After v1.1.0

| # | Competency | Current | Target | Delta |
|---|-----------|---------|--------|-------|
| C3 | Weapon System & Power Fantasy | 72 | 85+ | +13 |
| C4 | Mobile UX & Touch Controls | 82 | 85+ | +3 |
| H6 | Difficulty Curve & Boss Design | 73 | 88+ | +15 |
| H7 | Scoring & Achievement Design | 78 | 85+ | +7 |

Target: 12/12 competencies passing (currently 8/12).

**Status**: All 12 findings implemented in build 028. Subsequently, builds 029-034 added audio fixes, 2x resolution, weapon wheel, title screen overhaul, Slower weapon removal, FreePlay→The Breach rename, and ability pickup sprites.

---

## v1.4.2: Build 034 — Ability Pickup Sprites

- Proper procedural sprites for DoubleJump (green chevrons), AirDash (blue arrow with motion trails), GroundSlam (orange impact fist), PhaseShift (cyan diamond with ghost afterimage)

---

## v1.4.1: Build 033 — Playtest Fixes

- Cosmetics preview refresh on panel close, Campaign retry same epoch on fail, audio squeal mitigation (LP alpha 0.20, normalization 0.40, runtime LPF 3000Hz, max concurrent SFX 3), AudioListener moved to persistent GameManager

---

## v1.4.0: Build 032 — Title Screen & Weapon Overhaul

- Title screen: Cosmetics moved to Settings, New Game resized below Continue, player character preview lower-right, Achievements/Legends repositioned
- Weapon wheel: moved to lower-left, actual weapon sprites, 5 slots (72° intervals)
- Slower weapon deprecated (5 weapons: Bolt, Piercer, Spreader, Chainer, Cannon)
- FreePlay renamed to "The Breach", auto-defaults after Campaign completion
- Randomize checkbox replaced with "Replay Campaign" in Settings

---

## v1.3.0: Build 031 — 2x Resolution

- All procedural sprites doubled (TILE_PX 128→256, PPU 128→256, SCALE 2→4)
- Weapon wheel HUD: radial 6-slot indicator with fade-in/fade-out

---

## v1.2.0: Build 030 — QC Review Fixes

- Audio: 3-pass LP filter (alpha 0.25), linear normalization (0.50), runtime LPF 4000Hz, BASE_SFX_VOLUME 0.18
- Cosmetics: fixed WeaponSystem.cs overriding skin tint
- UI: Achievements/Legends buttons repositioned, difficulty button highlight, Campaign "Infinite lives" text, HUD deaths counter
- Polish: LeaderboardSuffix per difficulty, ghost replay capped at 50 entries

---

## v1.1.0: Build 028 — Maximize Fun (Expert Review)

All 12 findings from expert review implemented:

- **Tutorialization**: Weapon cycling hint on 2nd pickup ("Press X to cycle weapons for Quick Draw boost!"), hazard hint on first trigger, boss pillar flash + "Destroy the pillar!" hint. Tutorial replay already existed in Settings.
- **Campaign rebalance**: Infinite lives in Campaign mode (deaths cost time/score, never Game Over). Era 0 boss gains projectile in Phase 2 (30% shoot, 70% charge). Teleport Dash gets 0.4s shimmer telegraph before teleporting.
- **Scoring polish**: Combo only resets on enemy-sourced damage; environmental hazard damage gets 1s grace period (cancelled by kills). LevelCompleteMinDelay reduced 5.0→2.5s. Stomp shockwave damages all enemies within 2 tiles (flying counterplay).
- **Weapon UI**: Legend expanded from 3 tiers to all weapon types. (Note: Slower was subsequently removed in build 032, leaving 5 weapons: Bolt, Piercer, Spreader, Chainer, Cannon.)

---

## v1.0.3: Build 027 — Audio Quality, UI Layout, Repo Cleanup

- Audio quality overhaul: doubled loop crossfade (50→100ms), replaced all Sawtooth waves in ambient loops and music leads with Triangle, soft-clip normalization (target 0.28→0.20, tanh-like saturation), strengthened LPF (alpha 0.22→0.18, 3→4 passes, runtime cutoff 2500→2000Hz)
- ChallengeEntryUI: panel widened 540→580, text wrap mode fixed, width reduced 500→460
- LevelCompleteUI: combat mastery spacing fixed, score panel auto-sizes, buttons positioned dynamically below content
- Settings menu reordered: About, Difficulty, Level History, Audio, Accessibility
- Repo cleanup: removed stale `unity_project/` duplicate, archived 17 outdated docs, excluded `agents/`, `training/`, `docs/archive/` from GitHub via `.gitignore`

---

## v1.0.2: Build 026 — Playtest Feedback

- Audio squeal fix (3-pass LP filter, capped SFX frequencies)
- Dedicated weapon audio pool (boss fire rate fix)
- Level Complete redesign (full score details, COPY button, aligned panels)
- Pixel text scale bump for legibility
- Tutorial off by default
- Epoch 9 death→GameOver fix

---

## v1.0.1: Build 025 — QC Playtest Fixes

- WebGL AudioLowPassFilter guard
- Epoch display off-by-one
- Button label raycastTarget fix (all UI)
- Hover/press color states on all buttons
- Escape→Backquote alternative for WebGL fullscreen

---

## v1.0.0: Build 024 — Automated QC System

- 12 Play Mode smoke tests covering multi-level lifecycle, death/respawn, pause/resume, ghost replay, rapid transitions, campaign progression, singleton persistence
- Singleton lifecycle warnings on all 9 managers (`[Singleton] ... duplicate detected`)
- Post-transition validation coroutine in GameManager (Editor/Dev builds): checks CameraController, CheckpointManager, Player, LevelRenderer 2 frames after entering Playing state
- QC-CHECKLIST.md updated with Standing Rules for automated testing

---

## v0.9.8: Builds 021-022 — Bug Fixes & QC

> **Status**: Complete (builds 021-022)

### Bug Fix 1: Level Complete Screen Layout (LevelCompleteUI.cs)

**Symptoms**: Duplicate score rows (Speed and Items & Enemies appear twice), CONTINUE button partially cut off at bottom of screen. Most visible on tutorial levels where details auto-expand.

**Root cause**: Summary panel (lines 165-184) shows top 2 scoring components. Details panel (lines 216-227) shows all 5 components including those same top 2. When details auto-expand on first completion (`IsFirstCompletion = true`), both panels are visible simultaneously. Button shift is a fixed 120px (line 562) which pushes CONTINUE to Y=-400, exceeding the 1080 reference resolution.

**Fix**:
1. Hide summary stat rows when details panel is expanded (toggle visibility, not just shift)
2. Calculate button positions dynamically based on whether details are shown (measure content height, position buttons below)
3. Add scroll/clamp safety: ensure no button exceeds Y=-480 (leaving margin within 1080 bounds)

**Files**: `EpochBreaker/Assets/Scripts/UI/LevelCompleteUI.cs`

### Bug Fix 2: Camera Not Following on Second Level Load (LevelLoader.cs, GameManager.cs)

**Symptoms**: On the second level load, the camera shows the level half-rendered with the view not tracking the player. Usually resolves after the intro pan or on third load.

**Root cause**: Race condition in level loading sequence. Camera is created and positioned (LevelLoader line 74) before `Physics2D.SyncTransforms()` runs (line 101). On second load, player physics position hasn't settled before camera's first `LateUpdate()`. Additionally, `CameraController.StartLevelIntro()` is called immediately when state transitions to Playing (GameManager line 463) without waiting for physics to sync.

**Fix**:
1. Move `Physics2D.SyncTransforms()` call in `BuildLevelFromData()` to BEFORE `CreateCamera()` (ensures player's physics position is settled before camera initializes)
2. Delay `StartLevelIntro()` by one frame via coroutine (yield return null before starting intro pan)
3. In `CameraController.Initialize()`, snap camera position to target immediately (no smooth, no wait)
4. In `LevelLoader.CleanupLevel()`, explicitly null out `CameraController.Instance` before destroying old camera

**Files**: `EpochBreaker/Assets/Scripts/Gameplay/LevelLoader.cs`, `EpochBreaker/Assets/Scripts/Gameplay/GameManager.cs`, `EpochBreaker/Assets/Scripts/Gameplay/CameraController.cs`

### Bug Fix 3: Audio Pitch Variation Not Applying (AudioManager.cs)

**Symptoms**: Weapon fire and pitched SFX all play at pitch=1.0 instead of having ±10% randomization for variety.

**Root cause**: In `PlayWeaponSFX()` (line 162-164) and `PlaySFXPitched()` (line 191-193), `src.pitch` is set to a random value, then `PlayOneShot()` is called, then `src.pitch` is immediately reset to 1.0. Since `PlayOneShot()` is asynchronous, the pitch reset happens before the audio engine samples the pitch value.

**Fix**:
1. Remove the immediate `src.pitch = 1f` reset after PlayOneShot
2. Instead, reset pitch at the START of the next PlayOneShot call on the same source (before setting new pitch)
3. Or use a coroutine to delay the reset by one frame

**Files**: `EpochBreaker/Assets/Scripts/Gameplay/AudioManager.cs`

### Bug Fix 4: Pause Menu Escape Key (PauseMenuUI.cs)

**Symptoms**: When the pause menu is open (main content visible, no sub-panel open), pressing Escape does nothing. Players expect Escape to resume the game.

**Root cause**: The escape key handler (lines 30-57) checks for sub-panels and settings menus, but has no `else` clause to handle the main pause menu. When no sub-panel is open, the method falls through without action.

**Fix**: Add an `else` clause at the end of the escape handler that calls `GameManager.Instance?.ResumeGame()` when no sub-panel is active.

**Files**: `EpochBreaker/Assets/Scripts/UI/PauseMenuUI.cs`

### Bug Fix 5: Audio High-Pitch Artifacts (Monitoring)

**Status**: Mitigated in build 019 (LP alpha 0.45, AudioLowPassFilter 4000Hz, MAX_CONCURRENT_SFX 4). Needs verification that artifacts are fully resolved. If still present, further reduce cutoff frequency or limit SFX pool to 6.

**Files**: `EpochBreaker/Assets/Scripts/Gameplay/AudioManager.cs`, `EpochBreaker/Assets/Scripts/Gameplay/PlaceholderAudio.cs`

### Medium Priority Fixes

| # | File | Lines | Description |
|---|------|-------|-------------|
| 1 | GameplayHUD.cs | 990-1003 | Score popup pool (size 6) silently drops >6 simultaneous popups | Fixed (021) |
| 2 | EnemyBase.cs | 16 | Enemy registry ClearRegistry() not called during level transitions | Fixed (022) |
| 3 | TutorialManager.cs | 196-215 | Stuck detection false positive during intentional wall-sliding | Fixed (021) |
| 4 | CameraController.cs | 174 | _target.position null risk if target destroyed mid-frame in LateUpdate() | Fixed (021) |

### QC Pass

- [x] Run full code-level QC pass (2026-02-07, build 020)
- [x] Fix all critical/high bugs (build 021) — 9 of 16 resolved
- [x] Fix remaining medium/low bugs (build 022) — 14 of 16 resolved (L5 accepted, L6 monitoring)
- [ ] Run full runtime QC pass using [QC-CHECKLIST.md](QC-CHECKLIST.md) after bug fixes
- [ ] Record results in QC Pass History table

---

## v0.7.0–v0.9.0 (Implemented)

> Sprints 0-9 were implemented in builds 013-020. The sections below are preserved for historical reference.

## v0.7.0: "Make It Right" — Stability, Score Balance, Visual Polish

### Sprint 0: Critical Bug Fixes (Implemented build 013)

| # | Issue | Root cause | Fix |
|---|-------|-----------|-----|
| 1 | Camera intro pan freezes first load | WaitForSeconds is timeScale-dependent; player/physics may not be initialized | Switch to WaitForSecondsRealtime; yield one frame before pan so Start() and physics settle; add destroyed-target guard |
| 2 | Level complete details overlaps buttons | Details panel height (340px) is fixed but content overflows when combat sub-breakdown has multiple lines + tip | Shift buttons down by 80px when details expanded, or clamp panel content |
| 3 | Physics desync on first spawn | CompositeCollider2D may not rebuild before player's first FixedUpdate | Call Physics2D.SyncTransforms() after tilemap render + player spawn |

### Sprint 1: Score Rebalance

The kill bonus formula `100 * killCount` creates quadratic scaling (27 kills = 37,800 pts). Items & Enemies reached 44,900 in playtesting — 85% of total score. The 5-component system is effectively 1-component.

| # | Change | Before | After | Rationale |
|---|--------|--------|-------|-----------|
| 1 | Kill bonus formula | 100 * EnemiesKilled (quadratic) | 100 flat per kill, soft cap at 2500 | Prevents one component from dominating |
| 2 | Item multiplier cap | Uncapped (nth pickup = n x base) | Cap at 3x multiplier | Keeps items rewarding without runaway |
| 3 | Combo kill bonus | Combo tracked but gives no score | +50 per combo level (x2=+100, x3=+150...) capped at +500 | Rewards streaks without exponential growth |
| 4 | Star thresholds | 3-star >= 13000, 2-star >= 7500 | Recalibrate after rebalance. Target: theoretical max ~20k, 3-star ~68%, 2-star ~39% | Match design doc intent |
| 5 | Verify all 5 components | Speed max 5000, Combat max 5000, Exploration max 4000, Preservation max 2000 | Items & Enemies max ~3000-4000 | No component exceeds ~30% of total |

### Sprint 2: Sprite Animation Pass

Biggest remaining visual gap — all entities are static sprites. All animations generated at runtime via Texture2D frame sheets.

| # | Target | Frames | Trigger |
|---|--------|--------|---------|
| 1 | Player walk cycle | 4 frames, swap every 0.12s | Horizontal velocity > 0.5 |
| 2 | Player jump/fall | 2 poses (ascending, descending) | Based on Y velocity sign |
| 3 | Player wall-slide | Squished sprite + 2px vertical bob | IsWallSliding flag |
| 4 | Enemy patrol/chase walk | 2-frame alternation | Moving enemies |
| 5 | Enemy idle bob | Subtle scale oscillation (0.98-1.02) | Stationary enemies |
| 6 | Boss breathing | Scale pulse per phase (faster in phase 3) | Always while active |
| 7 | Boss phase transition | Flash white -> new color over 0.5s | On phase change |
| 8 | Pickup pulse | Already implemented (WeaponPickup/RewardPickup bob) | Verify working |

---

## v0.8.0: "Make It Deep" — Content Depth & New Systems

### Sprint 3: Unique Boss Encounters

Design doc Section 15.4 plans 10 unique bosses. Current implementation: single generic 3-phase boss. This sprint adds era-specific boss variety.

| # | Feature | Design |
|---|---------|--------|
| 1 | Boss variant data | Each era gets a BossVariant enum affecting attack patterns, movement style, and arena layout |
| 2 | Era 0-2: Simple bosses | 2 phases only. Era 0: charge-only (no ranged). Era 1: charge + single shot. Era 2: all attacks but slower cooldowns |
| 3 | Era 3-5: Standard bosses | 3 phases (current system). Era 3: adds ground-slam shockwave. Era 4: teleport dash. Era 5: summons 2 minions in phase 3 |
| 4 | Era 6-8: Complex bosses | 3 phases with unique mechanics. Era 6: arena hazard activation. Era 7: projectile reflection shield. Era 8: gravity flip arena |
| 5 | Era 9: The Architect | 4 phases, 500+ HP. Rewrites arena geometry mid-fight (destroys/creates platforms). Phase 4: fast pattern with all previous mechanics |
| 6 | Destructible arena elements | Per-era arena themes: pillars (current), breakable floors, ceiling stalactites, energy barriers |
| 7 | Boss-specific music cue | 2-bar intro stinger on boss activation, tempo increase per phase |

### Sprint 4: Special Attack System

Design doc Section 15.1. High-spectacle power fantasy reward for weapon collection.

| # | Feature | Design |
|---|---------|--------|
| 1 | Activation | Hold jump 0.5s while grounded with 3+ weapons acquired. Visual charge-up (glow). Full invulnerability during attack |
| 2 | Era-specific attacks | Epoch 0: Avalanche (rocks fall). 1: Bronze Storm. 2: Phalanx Volley. 3: Siege Barrage. 4: Renaissance Burst. 5: Steam Cannon. 6: Airstrike. 7: Data Storm. 8: Orbital Beam. 9: Singularity Pulse |
| 3 | Scaling | Damage scales with weapon count (3 weapons = 1x, 5 weapons = 2.5x). 15s cooldown, reduced 2s per extra weapon beyond 3 |
| 4 | Screen effect | Full-screen flash in era accent color, heavy screen shake (trauma 0.8), 0.2s hit-stop |
| 5 | HUD indicator | Charge meter below health hearts, fills when ready, depletes on use |

### Sprint 5: Expanded Abilities

Design doc Section 15.2. Currently implemented: Double Jump (epoch 3+), Air Dash. Missing: Ground Slam, Phase Shift.

| # | Ability | Epoch | Design |
|---|---------|-------|--------|
| 1 | Ground Slam (upgrade) | 5+ | Replaces basic ground pound. 3-tile AoE on landing, destroys soft/medium blocks, 2 damage to enemies in radius. Visual: shockwave ring |
| 2 | Phase Shift | 7+ | Tap dash through thin walls (1-2 tiles thick). 1.5s cooldown. Ghost trail effect during shift. Cannot phase through reinforced/indestructible |
| 3 | Ability upgrade tiers | Match weapon tier system | Starting -> Enhanced. Enhanced doubles duration/range/AoE |
| 4 | Ability pickup variety | Visual distinction | Ground Slam = orange orb, Phase Shift = cyan orb (vs current green/blue) |

### Sprint 6: Daily Challenge System

Design doc Section 15.3. Most impactful retention/social feature.

| # | Feature | Design |
|---|---------|--------|
| 1 | Daily seed | YYYYMMDD -> deterministic seed. Fixed epoch = day % 10. Same level worldwide |
| 2 | Difficulty rotation | Mon=Easy (epoch 0-2), Tue=Medium (3-5), Wed=Hard (6-8), Thu=Expert (9), Fri-Sun=Random |
| 3 | Daily leaderboard | Local top-5 attempts per day stored in PlayerPrefs. Shows best score, star rating, time |
| 4 | Title screen integration | "DAILY" button with today's epoch name + accent color. Badge if not yet attempted today |
| 5 | Streak tracking | Consecutive days played counter. Bonus score multiplier after 3+ day streak (1.1x, 1.2x, 1.5x at 7 days) |
| 6 | Share daily result | "Share" button on daily complete: copies formatted text with score + star rating + level code |

---

## v0.9.0: "Make It Ship" — Cosmetics, Social, Final Polish

### Sprint 7: Cosmetic Progression

Design doc Section 15.5. Zero-monetization cosmetics earned through gameplay.

| # | Feature | Design |
|---|---------|--------|
| 1 | Character skins (8) | Recolor player sprite. Unlocked by achievements: Default, Gold (3-star any), Shadow (Untouchable), Crimson (Boss Killer), Glacier (Cool Under Pressure), Neon (Score Legend), Archaeologist (all relics), Rainbow (all achievements) |
| 2 | Trail effects (5) | Particle trail behind player while moving. None (default), Sparks, Frost, Fire, Glitch. Unlocked at 5/10/20/50 levels completed + all achievements |
| 3 | Profile frames (3) | Border on level complete screen. Bronze (Campaign complete), Silver (Streak 10+), Gold (Streak 25+) |
| 4 | Cosmetic select UI | Settings menu section or dedicated screen. Preview animation on selection |
| 5 | Persistence | Selected cosmetics stored in PlayerPrefs. Visible to player only (no multiplayer) |

### Sprint 8: Social & Retention Features

Design doc Section 15.3 + review question 9 (level sharing).

| # | Feature | Design |
|---|---------|--------|
| 1 | Enhanced level sharing | Share button generates formatted text: "I scored X on Epoch Breaker level [code]! Can you beat it?" Copy to clipboard for paste into messages |
| 2 | Ghost replay | After completing a level, optional replay shows ghost of your run. Stored as position samples every 0.2s (compact). Compare against your previous best |
| 3 | Challenge a friend | Enter a friend's level code from title screen. Shows their score as target during gameplay (ghost text: "Friend: 12,450") |
| 4 | Weekly seed | In addition to daily, a weekly challenge with its own leaderboard. Resets Monday. More time to optimize |
| 5 | Achievement sharing | "Share" button in achievement gallery: copies achievement name + description to clipboard |

### Sprint 9: Final Polish Pass

| # | Area | Changes |
|---|------|---------|
| 1 | Audio variety | 3 gameplay music tracks per era group (0-2, 3-5, 6-8, 9) instead of 1 parameterized track. Random selection per level. Victory jingle varies by star rating |
| 2 | Sound design | Unique SFX per weapon type fire (currently shared). Ambient environmental audio (wind for era 0, machinery for era 5, digital hum for era 7). Boss phase-change roar |
| 3 | Accessibility | Colorblind mode (icon shapes supplement colors). Screen shake intensity slider (0-100%). Font size options for HUD. High contrast mode for hazard overlays |
| 4 | Tutorial refinement | Playtest-driven hint timing adjustments. Context-sensitive reminders (e.g., "Try wall-jumping!" if stuck near a wall for 5s). Skip individual hints |
| 5 | Difficulty options | Easy mode: 3 deaths per level, 50% enemy count, 1.5x health pickups. Hard mode: 1 death per level, 1.5x enemy count, no health pickups. Separate leaderboards per difficulty |
| 6 | Edge cases | Handle app backgrounding mid-level (auto-pause). Memory warning cleanup. Crash recovery from saved session |
| 7 | Level generation QA | Automated level validator: run 1000 random seeds, verify all completable, no stuck states, reasonable difficulty distribution |

---

## Deferred: Mobile Readiness (Future Release)

> **Status:** Deferred indefinitely. No mobile test environment available. All testing is
> done in Unity Editor and WebGL browser with keyboard controls.

When mobile becomes viable, this sprint covers iOS 15+, iPhone 11+ at 60fps:

| # | Feature | Design |
|---|---------|--------|
| 1 | Touch control audit | Verify thumb reach zones for landscape play. D-pad left-thumb, action buttons right-thumb. All targets >= 44pt |
| 2 | Safe area insets | Handle notch/Dynamic Island on iPhone 12+. Shift HUD elements inward from screen edges. Use Screen.safeArea |
| 3 | Haptic feedback | Light haptic on jump, medium on enemy kill, heavy on damage taken. Use iOS Taptic Engine via native plugin |
| 4 | Performance profiling | Target: <16ms frame time on iPhone 11. Profile: object pool, tilemap rendering, particle bursts, UI rebuilds. Optimize hot paths |
| 5 | Battery/thermal | Cap frame rate option (30/60fps toggle). Reduce particle count on low-power mode. Thermal throttle detection |
| 6 | Touch-specific UX | Larger pause button, swipe-to-cycle weapon option, hold-to-stomp vs tap-to-stomp setting |

---

## Priority & Dependencies

```
v0.7.0 Sprint 0 (Bug fixes)       <- BLOCKING, game broken for new players
v0.7.0 Sprint 1 (Score rebalance)  <- HIGH, scoring meaningless without this
v0.7.0 Sprint 2 (Animations)       <- HIGH, biggest visual gap

v0.8.0 Sprint 3 (Boss variants)    <- depends on Sprint 2 (animations)
v0.8.0 Sprint 4 (Special attacks)  <- depends on Sprint 2 (visual effects)
v0.8.0 Sprint 5 (Abilities)        <- independent
v0.8.0 Sprint 6 (Daily challenge)  <- independent

v0.9.0 Sprint 7 (Cosmetics)        <- depends on Sprint 2 (skin sprites)
v0.9.0 Sprint 8 (Social)           <- depends on Sprint 6 (daily system)
v0.9.0 Sprint 9 (Final polish)     <- last, benefits from all others

DEFERRED: Mobile Readiness          <- no test environment, deferred to future release
```

## Version Targets

| Version | Build | Focus | Milestone | Status |
|---------|-------|-------|-----------|--------|
| v0.7.0 | 013 | Fix bugs, rebalance scoring, add animations | Playable and fair | Done |
| v0.8.0 | 014 | Boss variety, special attacks, abilities, daily challenges | Content-complete | Done |
| v0.9.0 | 015-020 | Cosmetics, social, polish, UI reorganization | Ship-ready (desktop/web) | Done |
| v0.9.8 | 021-022 | QC bug fix sprint, camera fix, level complete fix | Bug-free baseline | Done |
| v1.0.0 | 024 | Automated QC: 12 smoke tests, singleton warnings, validation | Automated testing | Done |
| v1.0.1 | 025 | QC playtest fixes, WebGL guards, button states | Playtested | Done |
| v1.0.2 | 026 | Playtest feedback: audio, UI legibility, tutorial | Playtest-hardened | Done |
| v1.0.3 | 027 | Audio quality overhaul, UI layout fixes, repo cleanup | Clean codebase | Done |
| v1.1.0 | 028 | Expert review: tutorialization, campaign rebalance, boss fixes, scoring polish | Fun-maximized | Done |
| v1.2.0 | 030 | QC review fixes: audio, cosmetics, UI, polish, difficulty UX | QC-clean | Done |
| v1.3.0 | 031 | 2x resolution sprites, weapon wheel HUD | Visual upgrade | Done |
| v1.4.0 | 032 | Title screen overhaul, remove Slower weapon, rename FreePlay→The Breach | UX overhaul | Done |
| v1.4.1 | 033 | Playtest fixes: cosmetics preview, level fail retry, audio mitigation | Playtest-hardened | Done |
| v1.4.2 | 034 | Ability pickup sprites (DoubleJump, AirDash, GroundSlam, PhaseShift) | Visual completeness | Done |
| Future | TBD | Mobile readiness (touch, haptics, safe area, perf) | Ship-ready (iOS) | Deferred |
