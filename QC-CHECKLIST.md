# Epoch Breaker -- QC Checklist

**Created**: 2026-02-07 | **Version**: v1.0.0 build 024
**Purpose**: Systematic quality check for every screen, button, system, and feature in the game.
**How to use**: Run through each section after significant changes. Check off items as verified. Note failures with build number and description for tracking.

---

## How to Run a QC Pass

1. **Open the game** in WebGL browser (or Unity Editor)
2. **Clear PlayerPrefs** if testing fresh-install experience (optional: `PlayerPrefs.DeleteAll()` in editor console)
3. **Work through each section** below in order
4. **Record failures** in the Failure Log at the bottom
5. **Retest failures** after fixes are applied

### QC Pass Types

| Type | When | Scope |
|------|------|-------|
| **Full QC** | Before version bump (e.g., v0.9 -> v1.0) | All sections |
| **Screen QC** | After UI changes | Sections 1-7 |
| **Gameplay QC** | After gameplay/balance changes | Sections 8-14 |
| **Audio QC** | After audio changes | Section 15 |
| **Regression QC** | After any bug fix | Related section + Section 1 (title screen) |

### Standing Rules (Check Every Pass)

> **VIEWPORT BOUNDS**: Reference resolution is 1920x1080 with CanvasScaler matchWidthOrHeight=0.5. All UI elements (including badges, overflow text, expanded panels) must stay within **±540 vertical** and **±960 horizontal** from center. Anything beyond these bounds is off-screen. When repositioning or resizing any UI container, always verify: `top_edge = anchoredPosition.Y + height/2 ≤ 480` (leaving 60px safety margin for badges/overflow).
>
> This rule exists because viewport overflow is a recurring issue — it has happened with the challenge cluster (builds 014, 015, 017, 023), level complete screen (build 020), and hint text (build 020).

> **AUTOMATED SMOKE TESTS**: Before every deployment, run the Play Mode smoke test suite in Unity Test Runner (Window > General > Test Runner > Play Mode > Category: Smoke). All 12 tests must pass. These tests exercise multi-level lifecycle, death/respawn, pause/resume, ghost replay, rapid state transitions, and singleton persistence. They catch the class of bugs that manual QC misses (e.g., 2nd-level-load camera null from build 020).
>
> **SINGLETON WARNINGS**: All 9 singleton managers log `[Singleton] ... duplicate detected` warnings when a duplicate is created. If you see these warnings in the console, investigate — they indicate a lifecycle issue.
>
> **POST-TRANSITION VALIDATION**: In Editor and Development builds, GameManager runs a validation coroutine 2 frames after entering Playing state that checks all critical references (CameraController, CheckpointManager, Player, LevelRenderer). Look for `[QC Validation]` log entries — errors mean something broke during the level load.

---

## 1. Title Screen

### 1.1 First Launch (No Save Data)

- [ ] Title screen loads without errors
- [ ] Background camera renders (no black screen or "No cameras rendering")
- [ ] Game title/logo displays correctly
- [ ] "PLAY GAME" button visible and centered (no saved session)
- [ ] PLAY GAME launches tutorial (if not completed) or Campaign
- [ ] Sprite legend displays with correct player/enemy/item sprites
- [ ] ACHIEVEMENTS button visible, properly padded from sprite legend
- [ ] LEGENDS button visible, properly padded from sprite legend, stacked below ACHIEVEMENTS
- [ ] Both ornate buttons have gold double-border frames
- [ ] Build ID accessible via Settings > About

### 1.2 Returning Player (With Save Data)

- [ ] "CONTINUE" and "NEW GAME" buttons visible (saved session exists)
- [ ] CONTINUE resumes saved session at correct level
- [ ] NEW GAME shows confirmation dialog ("All current progress will be lost")
- [ ] Confirmation dialog: CONTINUE button clears session and rebuilds menu
- [ ] Confirmation dialog: CANCEL button dismisses dialog
- [ ] Confirmation dialog: Escape key dismisses dialog
- [ ] After clearing session, menu shows "PLAY GAME" (no session)

### 1.3 Challenge Cluster (Top-Right)

- [ ] DAILY button visible with today's epoch name
- [ ] WEEKLY button visible
- [ ] CHALLENGE button visible
- [ ] Cluster positioned at top-right, not overlapping other elements
- [ ] **VIEWPORT CHECK**: All cluster content (including NEW badges) within ±540 vertical / ±960 horizontal bounds
- [ ] DAILY launches daily challenge level
- [ ] WEEKLY launches weekly challenge level
- [ ] CHALLENGE opens code entry overlay

### 1.4 Navigation Buttons

- [ ] ENTER LEVEL CODE button works, opens input overlay
- [ ] Level code input accepts valid codes (e.g., "3-K7XM2P9A")
- [ ] Level code input rejects invalid codes with error message
- [ ] SETTINGS button opens settings overlay
- [ ] COSMETICS button opens cosmetic selection
- [ ] All buttons have legible text (no truncation, no overlap)
- [ ] Keyboard hints display at bottom of screen

### 1.5 Escape Key Navigation

- [ ] Escape closes confirmation dialog (if open)
- [ ] Escape closes active overlay (settings, cosmetics, achievements, etc.)
- [ ] Escape closes sub-panels within settings
- [ ] Escape does not close title screen itself

---

## 2. Settings Menu

### 2.1 Audio Settings

- [ ] Music volume slider responds to input
- [ ] SFX volume slider responds to input
- [ ] Weapon volume slider responds to input (if present)
- [ ] Volume changes persist after closing and reopening settings
- [ ] Volume changes persist across game sessions (PlayerPrefs)
- [ ] Muting music (slider to 0) silences music and ambient
- [ ] Muting SFX (slider to 0) silences all sound effects

### 2.2 Accessibility Settings

- [ ] Colorblind mode options available
- [ ] Text size options available
- [ ] Screen shake intensity slider works (0-100%)
- [ ] Changes take effect immediately (no restart needed)

### 2.3 Difficulty Settings

- [ ] Death limit configurable (1-5)
- [ ] Changes apply to next level (not mid-level)
- [ ] Settings persist across sessions

### 2.4 About Section

- [ ] Build ID displays correctly (matches BuildInfo.cs)
- [ ] Version number accurate
- [ ] Build date accurate

### 2.5 Level History (in Settings)

- [ ] History table loads without errors
- [ ] Columns visible: CODE, SOURCE, EPOCH, SCORE, STARS, COPY
- [ ] Source column color-coded (Campaign=green, Challenge=cyan, Daily/Weekly=purple, Code=gray)
- [ ] Pagination works (next/previous page)
- [ ] COPY button copies level code to clipboard
- [ ] Empty history shows appropriate message
- [ ] History limited to 50 entries max

---

## 3. Achievements Screen

- [ ] Opens from title screen ACHIEVEMENTS button
- [ ] Gold double-border ornate frame renders correctly
- [ ] Title "ACHIEVEMENTS" displays at top
- [ ] Unlock count shows "X / Y Unlocked"
- [ ] Two-column layout with all achievements visible
- [ ] Scrolling works via mouse wheel
- [ ] Unlocked achievements: green background, gold name, "V" icon
- [ ] Locked achievements: dark background, dim name, "-" icon
- [ ] Progress bars display for cumulative achievements (not yet unlocked)
- [ ] Progress text shows "X/Y" for cumulative achievements
- [ ] SHARE button visible on unlocked achievements
- [ ] SHARE copies achievement text to clipboard
- [ ] CLOSE button works
- [ ] Escape key closes overlay
- [ ] No achievement rows overlap or clip outside scroll area

---

## 4. Cosmetics Screen

- [ ] Opens from title screen COSMETICS button
- [ ] Skins section: all 8 skins listed
- [ ] Unlocked skins selectable with visual feedback
- [ ] Locked skins show requirement text
- [ ] Trails section: all 5 trails listed
- [ ] Trails wrap correctly (4 per row)
- [ ] Frames section: all 4 frames listed (including "None")
- [ ] Frames wrap correctly (4 per row)
- [ ] Preview panel shows selected cosmetic
- [ ] Selection persists after closing
- [ ] Escape closes overlay

---

## 5. Legends Leaderboard

- [ ] LEGENDS button visible on title screen (after Campaign completion)
- [ ] Legends button hidden if Campaign not completed
- [ ] Leaderboard shows up to 20 entries
- [ ] Entries sorted by level count (descending)
- [ ] Each entry shows: rank, level count, total score, timestamp
- [ ] Gold ornate frame renders correctly
- [ ] Escape closes overlay

---

## 6. Challenge Entry

- [ ] CHALLENGE button opens code entry overlay
- [ ] Input field accepts text
- [ ] Valid challenge code (e.g., "3-K7XM2P9A:15000") starts level with target score
- [ ] Invalid code shows error message
- [ ] Level code without score (e.g., "3-K7XM2P9A") treated as plain code entry
- [ ] Escape closes overlay

---

## 7. Level Complete Screen

### 7.1 Layout & Positioning

- [ ] "LEVEL X COMPLETE" title displays correctly
- [ ] Epoch name displays below title
- [ ] Star rating (1-3) displays correctly
- [ ] Total score displays prominently
- [ ] **Summary rows do NOT duplicate with details panel** (known bug: build 020)
- [ ] **CONTINUE button fully visible, not cut off** (known bug: build 020)
- [ ] All buttons within screen bounds (1920x1080 reference)

### 7.2 Score Breakdown

- [ ] Top 2 scoring components shown in summary (collapsed view)
- [ ] Details panel expandable (click to toggle)
- [ ] All 5 components listed in details: Speed, Items & Enemies, Combat Mastery, Exploration, Archaeology
- [ ] Sub-breakdowns visible in expanded details
- [ ] Score values add up to total
- [ ] Star rating matches score thresholds (3-star >= ~68%, 2-star >= ~39%)

### 7.3 Challenge Mode Results

- [ ] If played via Challenge: "CHALLENGE BEATEN!" (green) or "CHALLENGE FAILED" (red)
- [ ] Score comparison: "Your: X vs Target: Y (+/-Z)"
- [ ] If played via plain code entry: no challenge result section
- [ ] If played via Campaign/Streak: no challenge result section

### 7.4 Buttons

- [ ] CONTINUE advances to next level (Campaign/Streak)
- [ ] CONTINUE returns to title (FreePlay modes)
- [ ] REPLAY restarts current level
- [ ] MENU returns to title screen
- [ ] SHARE copies level code to clipboard
- [ ] WATCH REPLAY starts ghost replay (if available)
- [ ] 5-second delay before buttons become interactive
- [ ] All buttons have readable text

### 7.5 Tutorial Levels

- [ ] Level complete displays correctly after tutorial level
- [ ] Details auto-expand on first completion
- [ ] Layout does not break when details auto-expand
- [ ] Progression to next tutorial level works

---

## 8. Gameplay HUD

### 8.1 Core Display

- [ ] Health hearts display (5 max)
- [ ] Hearts flicker during i-frames
- [ ] Score counter updates on item/enemy/relic collection
- [ ] Timer runs during gameplay
- [ ] Current weapon/tier displays
- [ ] Lives remaining display (Campaign/Streak)

### 8.2 Combat HUD

- [ ] Kill streak counter appears on combo
- [ ] Kill streak fades after 2s of no kills
- [ ] Boss health bar appears in boss arena
- [ ] Boss name label displays
- [ ] Damage direction indicators flash on hit
- [ ] Special attack meter fills when ready
- [ ] DPS cap warning displays when weapon overheats

### 8.3 Mode Indicators

- [ ] Campaign: epoch number/name visible
- [ ] Streak: level count visible
- [ ] Challenge: "CHALLENGE MODE" badge visible (gold)
- [ ] Challenge: target score displays
- [ ] Daily/Weekly: appropriate mode indicator

### 8.4 Score Popups

- [ ] Score popups appear at world position of collection
- [ ] Popups animate upward and fade
- [ ] Multiple simultaneous popups don't overlap

### 8.5 Era Card

- [ ] Era name card displays at level start
- [ ] Card fades after brief display

### 8.6 Achievement Toasts

- [ ] Toast notification appears on achievement unlock
- [ ] Toast shows achievement name
- [ ] Toast auto-dismisses after timer

---

## 9. Pause Menu

- [ ] Escape pauses gameplay
- [ ] Time freezes (timeScale = 0)
- [ ] RESUME returns to gameplay
- [ ] SETTINGS opens settings sub-panel
- [ ] RESTART LEVEL reloads current level
- [ ] MAIN MENU returns to title
- [ ] Escape closes pause menu (resumes)
- [ ] Audio continues (or mutes) as expected when paused

---

## 10. Game Over Screen

- [ ] Displays after running out of lives
- [ ] Campaign: shows "Campaign ended at Epoch X: [Name]" + total score
- [ ] Streak: shows "Streak: N levels completed" + "Saved to Legends!"
- [ ] FreePlay: shows "Out of lives!"
- [ ] RETRY starts new game
- [ ] MAIN MENU returns to title
- [ ] Streak score saved to Legends leaderboard

---

## 11. Celebration Screen

- [ ] Triggers after completing all 10 campaign epochs
- [ ] Hero sprite displays with selected cosmetic
- [ ] "EPOCH BREAKER" title earned
- [ ] Total campaign score shown
- [ ] ENTER STREAK MODE button works
- [ ] MAIN MENU button works
- [ ] Legends/Streak mode unlocked after this screen

---

## 12. Camera System

### 12.1 Basic Tracking

- [ ] Camera follows player horizontally
- [ ] Camera tracks player vertically (offset for bottom-third view)
- [ ] Smooth damping (no jerky movement)
- [ ] Look-ahead in player's movement direction
- [ ] Dead zone prevents micro-jitter

### 12.2 Level Transitions

- [ ] **Camera follows player on FIRST level load** (known working)
- [ ] **Camera follows player on SECOND level load** (known bug: build 020)
- [ ] Camera follows player on THIRD+ level loads
- [ ] Level intro pan plays on level start
- [ ] Camera returns to player after intro pan

### 12.3 Boss Arena

- [ ] Camera locks during boss arena
- [ ] Orthographic size expands for arena
- [ ] Camera clamps to arena bounds
- [ ] Camera returns to normal after boss defeat

### 12.4 Screen Effects

- [ ] Screen shake on damage (varies with trauma)
- [ ] Screen shake intensity respects accessibility setting
- [ ] Screen flash on level start (white)
- [ ] Screen flash on victory (gold)

---

## 13. Player Mechanics

### 13.1 Movement

- [ ] Left/right movement responsive
- [ ] Jump works (variable height with button hold)
- [ ] Coyote time works (jump after leaving edge)
- [ ] Jump buffer works (jump input before landing)
- [ ] Wall slide activates on wall contact
- [ ] Wall jump launches correctly
- [ ] Ground pound/stomp works
- [ ] Squash/stretch visual on landing/jump

### 13.2 Combat

- [ ] Weapons fire automatically when acquired
- [ ] Manual weapon cycling via attack button
- [ ] Quick Draw activates on manual cycle
- [ ] All 6 weapon types function (Bolt, Piercer, Slower, Cannon, Chain, Spread)
- [ ] Weapon tier upgrades work (Starting -> Bronze -> Silver -> Gold)
- [ ] Heat system limits Cannon fire rate
- [ ] Special attack charges with 3+ weapons
- [ ] Special attack activates on hold

### 13.3 Abilities

- [ ] Double Jump works when acquired
- [ ] Air Dash works when acquired
- [ ] Ground Slam works when acquired (era 5+)
- [ ] Phase Shift works when acquired (era 7+)
- [ ] Ability cooldowns function correctly

### 13.4 Health & Death

- [ ] Player takes damage from enemies/hazards
- [ ] I-frames prevent rapid damage
- [ ] Knockback on damage
- [ ] Death triggers respawn at checkpoint
- [ ] Fall below kill plane triggers death
- [ ] Lives decrement correctly

---

## 14. Level Systems

### 14.1 Level Generation

- [ ] Levels generate from seed without errors
- [ ] Same seed produces same level
- [ ] Different seeds produce different levels
- [ ] All 10 epochs generate valid levels
- [ ] Tutorial levels load correctly (3 levels)

### 14.2 Level Content

- [ ] Enemies spawn at correct positions
- [ ] Enemy patrol/chase behaviors work
- [ ] Boss spawns in arena
- [ ] Boss phases transition correctly
- [ ] Pickups spawn (health, weapons, abilities)
- [ ] Relics spawn and are collectible
- [ ] Destructible tiles break on damage
- [ ] Hazards activate on tile destruction
- [ ] Goal trigger completes level

### 14.3 Era Differentiation

- [ ] Each era has distinct tile/sprite colors
- [ ] Each era has distinct music
- [ ] Each era has distinct ambient audio
- [ ] Difficulty scales across eras (enemy HP, speed, density)

### 14.4 Checkpoints

- [ ] Checkpoint triggers save position
- [ ] Respawn returns to last checkpoint
- [ ] Multiple checkpoints per level work

---

## 15. Audio

### 15.1 Music

- [ ] Title screen music plays
- [ ] Gameplay music plays (era-appropriate)
- [ ] Music variants rotate (3 per era group)
- [ ] Victory jingle plays on level complete (varies by star rating)
- [ ] Music stops on game over
- [ ] Music volume responds to settings slider

### 15.2 Sound Effects

- [ ] Menu select SFX plays on button press
- [ ] Item pickup SFX plays
- [ ] Enemy hit SFX plays
- [ ] Player hurt SFX plays
- [ ] Weapon fire SFX plays (per weapon type)
- [ ] Jump/land SFX plays
- [ ] **No high-pitched artifacts during gameplay** (known issue: builds 018-019)
- [ ] **No audio crackling or distortion**
- [ ] Concurrent SFX limited (max 4, no overload)
- [ ] Rapid-fire prevention works (same clip within 0.05s)

### 15.3 Ambient Audio

- [ ] Ambient loop plays during gameplay (era-appropriate)
- [ ] Ambient follows music volume setting
- [ ] Ambient stops on level complete/game over

### 15.4 Audio Filtering

- [ ] Low-pass filter active (4000Hz cutoff)
- [ ] No piercing high-frequency sounds
- [ ] Volume normalization ceiling (0.35f) effective

---

## 16. Persistence & Data

### 16.1 Session Save/Load

- [ ] Campaign session saves automatically
- [ ] Session includes: level code, mode, epoch, lives, score
- [ ] CONTINUE loads correct level from saved session
- [ ] NEW GAME properly clears session data
- [ ] Crash recovery restores last session

### 16.2 Achievements

- [ ] Achievements track progress across sessions
- [ ] Cumulative achievements increment correctly
- [ ] Single-level achievements reset per level
- [ ] Unlock timestamp recorded
- [ ] Achievement data survives browser refresh (PlayerPrefs)

### 16.3 Cosmetics

- [ ] Selected skin persists across sessions
- [ ] Selected trail persists across sessions
- [ ] Selected frame persists across sessions
- [ ] Invalid selections clamp to valid range

### 16.4 Daily/Weekly Challenges

- [ ] Daily seed deterministic (same level all day)
- [ ] Weekly seed deterministic (same level all week)
- [ ] Daily scores stored (max 5 per day)
- [ ] Weekly scores stored (max 5 per week)
- [ ] Streak counter increments on consecutive days

### 16.5 Level History

- [ ] Completed levels logged with: code, source, epoch, score, stars, timestamp
- [ ] History capped at 50 entries
- [ ] Source field populated correctly (Campaign/Code/Challenge/Daily/Weekly)
- [ ] Old entries (pre-Source) display gracefully (empty/dim)

### 16.6 Ghost Replay

- [ ] Ghost data recorded during gameplay
- [ ] Ghost saved on level complete (if score improves)
- [ ] Ghost playback shows translucent overlay
- [ ] Ghost data per level code (not global)

---

## 17. Visual Systems

### 17.1 Sprites & Rendering

- [ ] Player sprite renders correctly with selected skin
- [ ] Enemy sprites render (era-appropriate colors)
- [ ] Boss sprite renders with phase-based colors
- [ ] Pickup sprites visible (weapons, health, abilities)
- [ ] Tile sprites render correctly per era
- [ ] No z-fighting or rendering artifacts

### 17.2 Particle Effects

- [ ] Jump dust particles
- [ ] Wall slide dust particles
- [ ] Enemy death particles
- [ ] Block destruction particles
- [ ] Player trail effect (if cosmetic selected)
- [ ] Stomp impact particles

### 17.3 Parallax Background

- [ ] Multi-layer parallax scrolls behind gameplay
- [ ] Era-appropriate background colors
- [ ] No visible seams or tearing

### 17.4 Screen Effects

- [ ] Screen flash on events (damage, victory, level start)
- [ ] Hit-stop freezes on impact
- [ ] Squash/stretch on player landing

---

## 18. Input System

- [ ] WASD / Arrow keys for movement
- [ ] Space for jump
- [ ] S / Down arrow for stomp
- [ ] X / J for weapon cycle
- [ ] Escape for pause
- [ ] Enter/Space to start from title
- [ ] R to restart from level complete
- [ ] All inputs responsive with no lag
- [ ] No input conflicts (e.g., jump + stomp simultaneously)

---

## 19. Tutorial System

- [ ] Tutorial starts on first play (if not completed)
- [ ] Tutorial level 1: movement + jump + wall-jump
- [ ] Tutorial level 2: combat + enemies
- [ ] Tutorial level 3: weapons + special attack
- [ ] Tutorial hints display at correct timing
- [ ] Skip per-hint works
- [ ] Tutorial completion flag saved (doesn't replay on next launch)
- [ ] Context-sensitive hints after tutorial (stuck detection, wall proximity)

---

## 20. Edge Cases & Stress Tests

### 20.1 Rapid Actions

- [ ] Rapidly pressing buttons on title screen doesn't crash
- [ ] Rapidly pausing/unpausing doesn't break state
- [ ] Rapidly cycling weapons doesn't cause audio overload
- [ ] Spamming stomp in air doesn't cause physics issues

### 20.2 Level Transitions

- [ ] Complete 3+ levels in sequence without camera/rendering bugs
- [ ] Tutorial -> Campaign transition smooth
- [ ] Campaign -> Game Over -> Retry flow works
- [ ] Level Complete -> Replay -> Level Complete cycle works
- [ ] FreePlay -> Title -> Campaign transition clean

### 20.3 Data Boundaries

- [ ] History at 50 entries: oldest entries pruned on new add
- [ ] Legends at 20 entries: lowest entries replaced
- [ ] All PlayerPrefs survive browser refresh
- [ ] No NaN or Infinity in score calculations

### 20.4 Fresh Install

- [ ] Game loads with no PlayerPrefs (first time)
- [ ] No errors from missing save data
- [ ] Default values sensible (volumes at 1.0, no cosmetics, no history)

---

## Known Bugs (Build 020)

Track known issues here. Remove entries when verified fixed.

### Critical

| # | Build | Section | File:Lines | Description | Status |
|---|-------|---------|-----------|-------------|--------|
| B1 | 020 | 7.1 | LevelCompleteUI.cs:178-260 | Duplicate score rows: summary shows top 2, details shows all 5. When details auto-expand on first completion, same rows appear twice | Fixed (021) |
| B2 | 020 | 7.1 | LevelCompleteUI.cs:388,570 | Hint text at Y=-550 (collapsed) and Y=-660 (expanded) outside ±540 viewport bounds. Challenge result text at Y=-240/-260 hidden behind expanded details panel | Fixed (021) |
| B3 | 020 | 12.2 | LevelLoader.cs:74,101; CameraController.cs | Camera race condition: created before Physics2D.SyncTransforms(). CameraController.Instance not nulled in CleanupLevel() — stale reference between levels | Fixed (021) |
| B4 | 020 | 15.2 | AudioManager.cs:162-164,191-193 | Pitch reset timing: `src.pitch = 1f` runs immediately after PlayOneShot(), before audio samples the value. Weapon/pitched SFX play at pitch=1.0 instead of randomized | Fixed (021) |

### High

| # | Build | Section | File:Lines | Description | Status |
|---|-------|---------|-----------|-------------|--------|
| H1 | 020 | 9 | PauseMenuUI.cs:30-57 | Escape key does NOT close pause menu from main pause content. Missing else clause to resume game | Fixed (021) |
| H2 | 020 | 7.3 | LevelCompleteUI.cs:285-302 | Challenge result text positioned at fixed Y=-240/-260, falls inside details panel area and gets hidden when details expand | Fixed (021) |

### Medium

| # | Build | Section | File:Lines | Description | Status |
|---|-------|---------|-----------|-------------|--------|
| M1 | 020 | 8.4 | GameplayHUD.cs:990-1003 | Score popup pool (size 6) silently drops popups when >6 triggered in one frame | Fixed (021) |
| M2 | 020 | 14.2 | EnemyBase.cs:16 | Enemy registry ClearRegistry() may not be called during level transitions — stale references | Fixed (022) |
| M3 | 020 | 19 | TutorialManager.cs:196-215 | Stuck detection false positive during wall-sliding (player stationary but intentionally sliding) | Fixed (021) |
| M4 | 020 | 12.2 | CameraController.cs:174 | _target.position accessed in LateUpdate() without guard against mid-frame destruction | Fixed (021) |

### Low

| # | Build | Section | File:Lines | Description | Status |
|---|-------|---------|-----------|-------------|--------|
| L1 | 020 | 8.1 | GameplayHUD.cs:263 vs 182 | Running score text (width ±150px) potentially overlaps health hearts (X 20-212) | Fixed (022) |
| L2 | 020 | 13.4 | HealthSystem.cs:58-59 | I-frame flashing uses Time.time — frame-rate-dependent animation speed | Fixed (022) |
| L3 | 020 | 13.4 | HealthSystem.cs:131-132 | _deathProcessing flag not reset if level unloads during death animation | Fixed (022) |
| L4 | 020 | 13.3 | AbilitySystem.cs:284-314 | Phase Shift checks single tile, not full player hitbox — clip into narrow gaps possible | Fixed (022) |
| L5 | 020 | 16.2 | AchievementManager.cs:150-151 | 2s save debounce could lose data on force-close (mitigated by OnDestroy flush) | Open |
| L6 | 019 | 15.2 | PlaceholderAudio.cs | High-pitched audio artifacts during gameplay | Fixed (023): 2-pass LP (alpha 0.35), norm 0.28, runtime LPF 2500Hz |

---

## Failure Log

Record QC failures here during each pass.

| Date | Build | Section | Item | Description | Fixed In |
|------|-------|---------|------|-------------|----------|
| 2026-02-07 | 020 | 7.1 | Layout | Duplicate score rows on first-completion auto-expand | — |
| 2026-02-07 | 020 | 7.1 | Layout | Hint text off-screen at Y=-550 | — |
| 2026-02-07 | 020 | 7.3 | Challenge | Challenge result hidden behind expanded details | — |
| 2026-02-07 | 020 | 9 | Pause | Escape doesn't close main pause menu | — |
| 2026-02-07 | 020 | 12.2 | Camera | Camera race condition on second level load | — |
| 2026-02-07 | 020 | 15.2 | Audio | Pitch variation not applying to weapon/pitched SFX | — |

---

## QC Pass History

| Date | Build | Type | Tester | Sections Passed | Failures Found |
|------|-------|------|--------|-----------------|----------------|
| 2026-02-07 | 020 | Full (code-level) | Claude | 1-6 (Title, Settings, Achievements, Cosmetics, Legends, Challenge), 8 (HUD partial), 10-11 (GameOver, Celebration), 13 (Player), 14 (Levels), 15.1/15.3/15.4 (Audio partial), 16 (Persistence), 18 (Input), 19 (Tutorial partial) | 4 critical, 2 high, 4 medium, 6 low |
| 2026-02-07 | 021 | Bug fix verification | Claude | B1-B4, H1-H2, M1, M3, M4 fixed. Remaining open: M2, L1-L6 | 9 of 16 resolved |
| 2026-02-07 | 022 | Remaining fixes | Claude | M2, L1-L4 fixed. Remaining: L5 (accept), L6 (monitoring) | 14 of 16 resolved |
