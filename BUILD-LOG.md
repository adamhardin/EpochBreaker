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

---

## How to Update

1. Increment `BUILD_NUMBER` in `BuildInfo.cs`
2. Update `BUILD_DATE` to today's date
3. Update `VERSION` if appropriate
4. Add a row to the table above
5. Build and deploy (see README.md for steps)
