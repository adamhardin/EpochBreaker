# Build Log

Tracks each WebGL deployment to GitHub Pages. The build number displayed on the title screen (bottom-left) corresponds to entries here.

**Live URL**: [https://adamhardin.github.io/EpochBreaker/](https://adamhardin.github.io/EpochBreaker/)
**Build ID source**: `EpochBreaker/Assets/Scripts/Gameplay/BuildInfo.cs`

---

| Build | Version | Date | Notes |
|-------|---------|------|-------|
| 001 | 0.1.0 | 2026-02-04 | Initial WebGL deployment — title screen, procedural levels, 10 epochs |
| 002 | 0.1.0 | 2026-02-04 | Fix EventSystem InputModule for new Input System |
| 003 | 0.1.0 | 2026-02-04 | Fix input issues, remove touch controls |
| 004 | 0.1.0 | 2026-02-04 | Migrate to new Input System, add Stomp touch button |
| 005 | 0.1.0 | 2026-02-05 | Boss AI fixes, death effects |
| 006 | 0.1.0 | 2026-02-05 | Game modes (Campaign, Streak, FreePlay), achievements UI, continue system, settings menu |
| 007 | 0.2.0 | 2026-02-06 | Audio volume normalization, build ID on title screen |
| 008 | 0.3.0 | 2026-02-06 | Expert review: 5-component scoring, 6-slot weapons, hazard system, tutorial, difficulty curve, boss tuning |
| 009 | 0.4.0 | 2026-02-06 | Expert review cycle 2: limited auto-select, Quick Draw, archaeology scoring, arena pillars, hazard rebalance, tutorial wall-jump optional |
| 010 | 0.5.0 | 2026-02-06 | Performance pass: cached physics/refs, batched geometry, debounced saves, enemy registry. Visual: parallax backgrounds, screen flash, squash/stretch. Features: ability system (double jump, air dash) |
| 011 | 0.5.0 | 2026-02-06 | Documentation cleanup, build ID display fix, compilation error fixes |
| 012 | 0.6.0 | 2026-02-06 | "Make It Fun" pass: object pooling, screen shake/hit-stop, boss overhaul, visible systems, era differentiation, weapon balance, score gamification, polish |
| 013 | 0.9.0 | 2026-02-07 | Full roadmap (v0.7–v0.9): bug fixes, score rebalance, sprite animations, 10 boss variants, special attacks, ground slam/phase shift, daily/weekly challenges, cosmetics (skins/trails/frames), ghost replay, friend challenges, sharing, accessibility, difficulty modes, tutorial hints, crash recovery, 1000-seed level validator |
| 014 | 0.9.1 | 2026-02-07 | Title screen reorganization: DAILY/WEEKLY/CHALLENGE moved to top-right cluster, TROPHIES/LEGENDS moved from corner buttons to center nav row, fixed button overlap with legend panel |
| 015 | 0.9.2 | 2026-02-07 | UX polish: persistent background camera (no more "No cameras rendering"), challenge cluster repositioned, build ID moved to Settings > About, audio low-pass filter + SFX concurrency limiter, level history pagination, achievements scrollable via ScrollRect |
| 016 | 0.9.3 | 2026-02-07 | UI fixes: achievements list visible (RectMask2D), settings menu spacing redistributed, cosmetics trail/frame row wrapping |
| 017 | 0.9.4 | 2026-02-07 | UI reorganization: challenge cluster enlarged with readable sub-labels, Trophies/Legends moved to legend panel, Level History moved to Settings, main menu simplified, text clarity pass (min font sizes, contrast) |
| 018 | 0.9.5 | 2026-02-07 | Fix achievements rendering (row anchor correction), ornate gold-framed Trophies/Legends buttons flanking sprite legend |
| 019 | 0.9.6 | 2026-02-07 | Audio: aggressive LP filter (alpha 0.45) + runtime AudioLowPassFilter + max 4 concurrent SFX. UI: challenge cluster raised, New Game clears session instead of launching, Trophies renamed to Achievements |
| 020 | 0.9.7 | 2026-02-07 | Differentiate Challenge vs Enter Code: Source field in level history, CHALLENGE MODE HUD badge, challenge result on level complete screen, color-coded Source column in history |
| 021 | 0.9.8 | 2026-02-07 | QC bug fix sprint: level complete layout (duplicate rows, off-screen elements, challenge overlap), camera race condition on 2nd load, audio pitch variation, pause escape key, score popup pool, tutorial stuck detection |
| 022 | 0.9.8 | 2026-02-07 | Remaining QC fixes: enemy registry cleanup on level transition, score text width cap, i-frame flash accumulator, death processing OnDisable guard, Phase Shift multi-tile hitbox check |
| 023 | 0.9.9 | 2026-02-07 | Challenge cluster vertical stack, audio artifact fix: 2-pass LP filter (alpha 0.35, -12dB/oct), lower normalization (0.28), runtime LPF cutoff 2500Hz |
| 024 | 1.0.0 | 2026-02-07 | Automated QC: 12 Play Mode smoke tests (multi-level lifecycle, death/respawn, pause/resume, ghost replay, rapid transitions), singleton lifecycle warnings on all 9 managers, post-transition validation coroutine in GameManager (Editor/Dev builds) |
| 025 | 1.0.1 | 2026-02-07 | QC playtest fixes: WebGL AudioLowPassFilter guard, epoch display off-by-one, button label raycastTarget fix (all UI), hover/press color states on all buttons, Escape→Backquote alternative for WebGL fullscreen |
| 026 | 1.0.2 | 2026-02-07 | Playtest feedback: audio squeal fix (3-pass LP filter, capped SFX frequencies), dedicated weapon audio pool (boss fire rate fix), Level Complete redesign (full score details, COPY button, aligned panels), pixel text scale bump for legibility, tutorial off by default, epoch 9 death→GameOver fix |
| 027 | 1.0.3 | 2026-02-07 | Audio quality overhaul (doubled loop crossfade, Sawtooth→Triangle in ambient/music, soft-clip normalization, strengthened LPF), UI layout fixes (ChallengeEntryUI text overflow, LevelCompleteUI dynamic sizing), settings menu reorder, repo cleanup (removed stale unity_project/, archived docs, .gitignore for local-only folders) |
| 028 | 1.1.0 | 2026-02-07 | Expert review "Maximize Fun": weapon cycling hint, hazard hint, boss pillar flash+hint, infinite campaign lives, Era 0 boss projectile in Phase 2, teleport telegraph (0.4s shimmer), combo grace for env damage, LevelComplete delay 5→2.5s, stomp shockwave (flying counterplay), weapon legend expanded to 6 types |
| 029 | 1.1.1 | 2026-02-07 | Audio: all leads→Triangle, high notes capped at E5, hi-hat 1000→700Hz, shimmer 1320→990Hz, 200ms fade-in, 100ms loop crossfade, soft-clip normalization (0.65 target). UI: sprite legend centered with even spacing, Achievements/Legends buttons repositioned below legend. Cosmetics: switched to SpriteRenderer.color tint. **Known issues:** audio squeal persists, cosmetics not applying, difficulty settings UX broken, button placement wrong |
| 030 | 1.2.0 | 2026-02-07 | QC review fixes (3 phases). **Audio:** 3-pass LP filter in MakeClip (alpha 0.25), linear gain normalization (0.50 target) replacing soft saturation, runtime AudioLowPassFilter (4000Hz) re-added with WebGL guard, BASE_SFX_VOLUME 0.25→0.18. **Cosmetics:** fixed WeaponSystem.cs overriding SpriteRenderer.color with Color.white every frame (Quick Draw glow now blends on skin tint). **UI:** Achievements/Legends buttons moved LEFT of sprite legend, controls hint includes weapon cycling (X key), difficulty button visual highlight for active selection, "Changes apply at next level start" text, Campaign mode "Infinite lives" text, HUD shows "Deaths: X" in Campaign instead of "Lives: X". **Polish:** LeaderboardSuffix wired into Legends + Level History (separate per difficulty), ghost replay PlayerPrefs capped at 50 entries. **Docs:** README/ADR-003 Level ID format aligned to E-XXXXXXXX, ADR-006 updated from FMOD to runtime synthesis, PROJECT-STRUCTURE.md version/bounds corrected, build script method name fixed, validation spec tolerances aligned with code (jump=5, difficulty±1.5). **.gitignore:** added .env patterns |
| 031 | 1.3.0 | 2026-02-08 | **2x Resolution upgrade:** all procedural sprites doubled (TILE_PX 128→256, PPU 128→256, SCALE 2→4). Player/enemy 384→768px, boss 768→1536px, projectiles 64→128px, weapons 192→384px, particles 16→32px, title logo blockSize 10→20. All ShiftRegion animation coords doubled. Checkpoint platform doubled in LevelLoader. World-space dimensions unchanged (pixels/PPU ratio preserved). **Weapon wheel HUD:** radial 6-slot weapon indicator replaces dot display, appears on weapon cycle with fade-in/fade-out (1.5s display, 0.15s in, 0.3s out), active slot highlighted at 1.3x scale, acquired slots colored, unacquired grayed. Center label shows active weapon name. |
| 032 | 1.4.0 | 2026-02-08 | **Title screen overhaul:** Cosmetics moved from main menu to Settings (after Difficulty), New Game resized same as Continue and stacked below it, player character preview with cosmetic tint in lower-right. Achievements/Legends buttons moved further left to prevent clipping. **Weapon wheel:** moved from center-right to lower-left, shows actual weapon sprites instead of colored orbs, 5 slots (72° intervals). **Slower weapon removed:** deprecated WeaponType.Slower (enum value kept, never spawned/cycled/displayed), auto-select changed from Slower→Cannon for boss encounters, legend panel shows 5 weapons. **FreePlay→The Breach:** renamed game mode, auto-defaults to The Breach (randomized) after Campaign completion. Randomize checkbox replaced with "Replay Campaign" button in Settings (only shown post-Campaign). |
| 033 | 1.4.1 | 2026-02-08 | Playtest fixes: **Cosmetics preview** now refreshes when closing cosmetics panel (was stale until menu re-entry). **Level fail retry:** Campaign mode now retries same epoch with new seed instead of advancing (must beat level to progress). **Slower sprite safety:** PlaceholderAssets remaps deprecated Slower to Cannon sprite. **UI:** main menu buttons lowered 30px for better spacing. **Audio squeal mitigation:** removed weapon pitch randomization (beating/interference source), tightened LP filter (alpha 0.25→0.20, ~2.5kHz cutoff), lowered normalization (0.50→0.40), reduced runtime LPF cutoff (4000→3000Hz), max concurrent SFX 4→3. **AudioListener:** moved from per-level camera to persistent GameManager object (fixes "no audio listeners" warning on title screen). |
| 034 | 1.4.2 | 2026-02-08 | **Ability pickup sprites:** replaced placeholder particle orbs with proper procedural sprites for DoubleJump (green chevrons), AirDash (blue arrow with motion trails), GroundSlam (orange impact fist), PhaseShift (cyan diamond with ghost afterimage). New `GetAbilitySprite()` method in PlaceholderAssets. |

---

## How to Update

1. Increment `BUILD_NUMBER` in `BuildInfo.cs`
2. Update `BUILD_DATE` to today's date
3. Update `VERSION` if appropriate
4. Add a row to the table above
5. Build and deploy (see README.md for steps)
