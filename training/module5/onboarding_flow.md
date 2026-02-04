# Assessment 5.4 - Onboarding Flow Specification

**Module**: 5 - Mobile UX & Accessibility
**Version**: 1.0
**Last Updated**: 2026-02-04
**Status**: Active
**Engine**: Unity 2022 LTS
**Orientation**: Landscape only (locked)
**Minimum Device**: iPhone 11
**Target**: 60fps

---

## Table of Contents

1. [Design Philosophy](#1-design-philosophy)
2. [First Launch Sequence](#2-first-launch-sequence)
3. [Level 1 Specification: Movement](#3-level-1-specification-movement)
4. [Level 2 Specification: Combat](#4-level-2-specification-combat)
5. [Level 3 Specification: Combination](#5-level-3-specification-combination)
6. [Ghost Button Label System](#6-ghost-button-label-system)
7. [Returning Player Detection](#7-returning-player-detection)
8. [Tutorial Replay System](#8-tutorial-replay-system)
9. [Implementation Details](#9-implementation-details)

---

## 1. Design Philosophy

### 1.1 Core Principles

1. **Show, don't tell.** The environment teaches. Text popups are avoided except for ghost button labels. The player learns by doing, not by reading.
2. **Respect the player's intelligence.** Constrained corridors guide without condescending. The player should feel they discovered the mechanic, not that it was explained to them.
3. **Immediate engagement.** The player must be providing input within 5 seconds of the app loading. No splash screens, no unskippable logos, no "tap to start" gates.
4. **Invisible for experienced players.** If a player demonstrates they already know a mechanic, hints evaporate. Returning players skip the tutorial entirely.
5. **No failure in the first 30 seconds.** The player cannot die before they have learned how to move and jump. The first 30 seconds are a safe introduction.

### 1.2 Teaching Progression

| Level | Mechanic Taught | Method | Enemies | Hazards | Power-Ups | Checkpoints |
|-------|----------------|--------|---------|---------|-----------|-------------|
| 1 | Horizontal movement, jumping | Constrained environment, mandatory gap | None | None | None | None |
| 2 | Attack / combat | Enemy blocks path, attack button highlighted | 1 weak enemy (Slime) | None | None | None |
| 3 | Movement + combat combination | Platforming with enemies, checkpoint, power-up | 3-5 enemies (Slimes) | 1 simple pit | 1 health pickup | 1 checkpoint |
| 4+ | Post-tutorial, full game | Normal level generation, all mechanics active | Standard | Standard | Standard | Standard |

---

## 2. First Launch Sequence

### 2.1 Cold Launch Timeline

| Time (seconds) | Event | Player Action |
|----------------|-------|---------------|
| 0.0 | App launches. iOS launch screen (static image matching game background) | None |
| 0.0 - 1.5 | Unity engine initializes, scene loads | None (launch screen visible) |
| 1.5 - 2.0 | Launch screen cross-fades to game scene | None |
| 2.0 - 2.5 | Player character appears, standing in Level 1 start position. Ghost button labels fade in. | None (but controls are active) |
| 2.5 - 3.0 | Subtle arrow particle effect nudges rightward from the player character, suggesting movement | None |
| 3.0 - 5.0 | **Player provides first input** (goal). If no input by 5.0s, a very faint "Tap to move" text fades in at 30% opacity below the character, then fades out after 3 seconds | Move right |

**Target**: < 5 seconds from app icon tap to first player input.

### 2.2 What Does NOT Happen on First Launch

- No publisher logo screen
- No game studio logo screen
- No "Loading..." screen (Level 1 is preloaded with the app)
- No "Tap to Start" screen
- No cutscene before gameplay
- No control scheme selection (defaults to Hybrid; changeable in settings later)
- No account creation / sign-in prompt
- No notification permission prompt (deferred until after Level 3)

### 2.3 Deferred Prompts

The following are intentionally delayed to avoid disrupting the first experience:

| Prompt | When Shown | Context |
|--------|-----------|---------|
| Accessibility options | After Level 1 completion, once | "Accessibility options are available in Settings" as a brief banner (3s) |
| Notification permission | After Level 3 completion, once | "Enable notifications for daily challenges?" standard iOS dialog |
| Rate the app | After Level 10 completion OR 7 days of play, whichever comes first | Standard SKStoreReviewController |
| iCloud sync | After Level 3 completion, if not already syncing | "Sync your progress across devices?" brief dialog |

---

## 3. Level 1 Specification: Movement

### 3.1 Purpose

Teach horizontal movement and jumping. No enemies. No hazards that can damage the player. The player cannot die in this level.

### 3.2 Level Layout (Schematic)

```
Width: ~30 tiles (480pt at 16pt/tile)
Height: 12 tiles (192pt)

Tile size: 16x16pt (pixel art base unit)

    [START]                                    [END/EXIT]
      |                                            |
      v                                            v
  ....................................................
  .                                                .
  .  P-->                          ___             .
  .  ___________        ___       |   |   _______  .
  .             |      |   |     F|   |  |       E .
  . GROUND      |  GAP |   | GAP |   |  | GROUND  .
  .             |______|   |_____|   |__|          .
  .                                                .
  ....................................................

Legend:
  P = Player spawn point
  E = Exit / level end trigger
  F = Flag (visual only, not a checkpoint - no checkpoints in Level 1)
  _ = Ground tiles
  . = Boundary (invisible walls at left/right edges)
  GAP = Space requiring a jump to cross
  --> = Direction of progression
```

### 3.3 Segment Breakdown

| Segment | Tiles | Purpose | Player Action | Failure Possible |
|---------|-------|---------|---------------|------------------|
| Flat run (start) | Tiles 0-8 | Introduce horizontal movement | Move right | No |
| Ledge drop | Tiles 8-10 | Show that height exists; player drops down | Move right (automatic fall) | No |
| First gap (small) | Tiles 10-14 | Force first jump. Gap is 2 tiles wide (32pt). Easy jump. | Jump | No - fail-safe: if player falls, invisible floor catches them 2 tiles below, places them back on left ledge with a gentle upward bounce |
| Middle platform | Tiles 14-18 | Flat area to recover. Flag here (visual landmark only) | Move right | No |
| Second gap (medium) | Tiles 18-23 | Reinforce jumping. Gap is 3 tiles wide (48pt). Requires a running jump. | Run + Jump | No - same fail-safe as first gap; invisible floor catches and resets |
| Elevated platform | Tiles 23-26 | Ascend via small step-up (1 tile high). Introduces vertical platforming. | Jump up 1 tile, move right | No |
| Exit corridor | Tiles 26-30 | Straight path to level end trigger | Move right | No |

### 3.4 Fail-Safe Mechanics (Level 1 Only)

| Mechanic | Normal Behavior | Level 1 Override |
|----------|----------------|------------------|
| Falling into gap | Player loses 1 heart, respawns at checkpoint | Player is caught by invisible floor, bounced back to left ledge. No damage. |
| Health | 5 hearts | Infinite (not displayed; HUD shows 5 hearts but they cannot decrease) |
| Death | Respawn at checkpoint | Impossible |
| Timer | None | None |
| Enemies | Present in later levels | None in Level 1 |
| Hazards (spikes, lava) | Present in later levels | None in Level 1 |

### 3.5 Visual Design

- **Biome**: Forest (the starter biome; bright, welcoming, non-threatening)
- **Background**: 3-layer parallax: distant mountains, midground trees, foreground foliage
- **Lighting**: Warm daylight, slight golden tint (#FFF8E0)
- **Music**: Calm, exploratory melody; 8-bit synth; 100 BPM; loops after 45 seconds
- **Ambient**: Bird chirps (with visual indicator: small birds occasionally fly across background)

### 3.6 Ghost Labels in Level 1

| Label | Appears When | Position | Duration |
|-------|-------------|----------|----------|
| "MOVE" (with left/right arrows) | 0.5s after player character appears on screen | Below the floating joystick / D-pad area, centered on the control | Fades after player moves horizontally for 2 seconds cumulative |
| "JUMP" (with up arrow) | Player reaches first gap (Tile 10) and stops for 0.5s OR after 3 seconds at gap edge | Below/beside the Jump (A) button | Fades after player successfully jumps once |

---

## 4. Level 2 Specification: Combat

### 4.1 Purpose

Teach the attack mechanic. A single weak enemy blocks the path forward. The player must attack to proceed. This is the first level where the player can take damage, but only from the enemy (no environmental hazards).

### 4.2 Level Layout (Schematic)

```
Width: ~35 tiles (560pt)
Height: 12 tiles (192pt)

    [START]                                        [END/EXIT]
      |                                                |
      v                                                v
  ..........................................................
  .                                                        .
  .  P-->                                                  .
  .  ________________                        ____________  .
  .                  |     SLIME             |            E .
  .  GROUND          |  ___[S]_______________| GROUND      .
  .                  |__|                                   .
  .                                                        .
  ..........................................................

Legend:
  P = Player spawn
  S = Slime enemy (weak, slow)
  E = Exit trigger
```

### 4.3 Segment Breakdown

| Segment | Tiles | Purpose | Player Action |
|---------|-------|---------|---------------|
| Flat run (start) | Tiles 0-10 | Review movement (quick flat run) | Move right |
| Small drop | Tiles 10-12 | Drop down into arena area | Move right (fall) |
| Enemy arena | Tiles 12-25 | Flat arena where the Slime patrols. The path forward is BLOCKED by the Slime. A wall at tile 25 prevents passing until the Slime is defeated. Wall lowers when Slime is killed. | Attack the Slime |
| Post-enemy flat | Tiles 25-35 | Celebratory run to exit after defeating the Slime | Move right |

### 4.4 Slime Enemy (Tutorial Version)

| Property | Value | Notes |
|----------|-------|-------|
| Health | 2 hits | Normal Slime has 3; tutorial version is weaker |
| Movement speed | 30% of normal | Very slow patrol, easy to read |
| Attack type | Contact damage | Touches player = 1 heart damage |
| Attack cooldown | 2.0 seconds | After damaging player, invulnerability frames: 1.5s (90 frames at 60fps) |
| Patrol range | 4 tiles (64pt) back and forth | Predictable, easy to learn timing |
| Visual telegraph | Slime bounces higher before lunging (0.8s warning) | Clear visual cue |
| Audio cue | Squelch sound before attack | With visual indicator (see accessibility spec) |

### 4.5 Attack Button Highlight (First Time)

When the player enters the enemy arena (Tile 12-14):
1. The Slime is visible 4 tiles ahead, patrolling
2. The Attack (B) button gets a pulsing golden highlight border:
   - Border color: #FFD700
   - Pulse: Scale 1.0x to 1.15x, 0.8s cycle, ease-in-out
   - Opacity of highlight: 100% (regardless of control opacity setting, for visibility)
3. A ghost label "ATTACK" appears next to the button (see Section 6)
4. The highlight and label persist until the player presses Attack for the first time
5. After first attack press, the highlight fades over 0.5s and does not return

### 4.6 Player Health in Level 2

- Starting health: 5 hearts
- Health is displayed in the HUD for the first time
- If the player loses all health:
  - Brief "Try Again" text (1.5s)
  - Respawns at the start of the enemy arena (Tile 12), NOT the level start
  - Health restored to 5 hearts
  - Slime health resets
  - No lives system (infinite retries in tutorial levels)

---

## 5. Level 3 Specification: Combination

### 5.1 Purpose

Combine movement and combat. Introduce checkpoints and power-ups. This is the transition level: after completing Level 3, the full game begins. Difficulty is slightly above Level 2 but still well below normal game difficulty.

### 5.2 Level Layout (Schematic)

```
Width: ~60 tiles (960pt)
Height: 15 tiles (240pt)

[START]                                                              [END]
  |                                                                    |
  v                                                                    v
...................................................................
.                                                                     .
.  P-->                         CHECKPOINT                            .
.  __________     ___     _____[CP]_______                ____     __ .
.            |   |   |   |               |   [S]    [S]  |    |   |  E.
.  GROUND    | G |   | G |   [S]  [H+]   |___________   _|    |___|  .
.            |___|   |___|               |           | |              .
.                                  PIT   |  PLAT     | | PLAT        .
.                                ######  |           | |              .
.                                        |___________| |______________.
.                                                                     .
...................................................................

Legend:
  P  = Player spawn
  E  = Exit trigger
  S  = Slime enemy (3 total)
  H+ = Health power-up
  CP = Checkpoint flag
  G  = Gap (requires jump)
  ## = Pit (first real hazard - instant respawn to checkpoint, 1 heart damage)
```

### 5.3 Segment Breakdown

| Segment | Tiles | Purpose | New Mechanic | Enemies | Hazards |
|---------|-------|---------|--------------|---------|---------|
| Movement review | 0-8 | Quick flat run, re-establishes movement | None | None | None |
| Gap sequence | 8-16 | Two gaps in succession (2-tile and 3-tile). Reinforces jumping from Level 1. | None | None | None |
| Enemy + power-up | 16-28 | One Slime patrols on a flat section. Health power-up placed BEFORE the Slime (at tile 20). Player picks up health, then fights. | **Health power-up** | 1 Slime | None |
| Checkpoint | 28-30 | First checkpoint flag. Activates on touch. Emits celebratory particle burst and haptic (notification success). Text appears: "Checkpoint!" for 1.5s | **Checkpoint** | None | None |
| Pit hazard | 30-36 | First pit. 3 tiles wide. Falling in = 1 heart damage, respawn at checkpoint. A warning sign (triangle "!") is posted 2 tiles before the pit edge. | **Environmental hazard** | None | 1 pit |
| Platforming + enemies | 36-52 | Multi-height platforms with 2 Slimes. Requires jumping between platforms and fighting enemies. One Slime on a lower platform, one on a higher platform. | **Jump + attack combo** | 2 Slimes | None |
| Exit corridor | 52-60 | Safe flat run to level end. | None | None | None |

### 5.4 Checkpoint Behavior (Introduction)

| Property | Value |
|----------|-------|
| Activation | Player passes through the checkpoint flag's trigger collider (3 tiles wide, full height) |
| Visual feedback | Flag raises from half-mast to full. Circular wave pulse expands from flag. Sparkle particles. |
| Audio feedback | Chime sound (with visual indicator for deaf players) |
| Haptic feedback | Notification success (see control_layout_spec.md) |
| Text | "Checkpoint!" appears above the flag in 17pt white text, fades after 1.5s |
| Save | Current progress auto-saved (position, health, score, enemies defeated). See interruption_handling_spec.md |
| Respawn behavior | On death, player respawns at checkpoint with health restored to max(current_health, 3). Score preserved. Defeated enemies stay defeated. |

### 5.5 Health Power-Up (Introduction)

| Property | Value |
|----------|-------|
| Appearance | Green "+" icon, 16x16pt sprite, bobs up/down 4pt, sparkle particles |
| Trigger | Player walks through it (collision) |
| Effect | Restores 2 hearts (up to max of 5) |
| Visual feedback | Hearts animate filling in HUD, "+" floats up from player position and fades |
| Audio feedback | Ascending chime (with visual indicator) |
| Haptic feedback | Soft impact |
| Ghost label | "POWER-UP" appears the first time a power-up is within 5 tiles of the player, positioned above the power-up sprite. Fades after collection. |

### 5.6 Post-Level 3 Transition

After completing Level 3:
1. Standard Level Complete screen appears (see ui_layout_spec.md)
2. A one-time banner appears at the top: "Tutorial complete! The adventure begins." (3s, fade out)
3. All ghost labels are permanently disabled (unless re-enabled in accessibility settings: "Always show button labels")
4. The `tutorialComplete` flag is set in the save data
5. From Level 4 onward, levels are procedurally generated using the Level ID system
6. Difficulty ramps per normal progression curves

---

## 6. Ghost Button Label System

### 6.1 Label Lifecycle

Ghost labels are semi-transparent text labels that appear near control buttons to identify their function. They follow a progressive fade-out sequence.

| Phase | Trigger | Label Appearance | Duration |
|-------|---------|-----------------|----------|
| Phase 1: Visible | First encounter with action | Full ghost label: button name in 14pt white text, 70% opacity, positioned 8pt outside the button edge | Until action performed 3 times |
| Phase 2: Fading | After 3 uses of the action | Label opacity reduces: 70% -> 40% -> 20% -> 0% over the 4th, 5th, and 6th use | 3 additional uses |
| Phase 3: Gone | After 6 total uses OR after Level 3 completion (whichever comes first) | Label fully removed from render | Permanent |

### 6.2 Label Definitions

| Control | Label Text | Position Relative to Button | First Shown |
|---------|------------|----------------------------|-------------|
| D-Pad / Joystick | "MOVE" | Below center of control, 8pt gap | Level 1, 0.5s after spawn |
| Jump (A) | "JUMP" | Above the button, 8pt gap | Level 1, at first gap |
| Attack (B) | "ATTACK" | Left of the button, 8pt gap | Level 2, entering enemy arena |
| Special (X) | "SPECIAL" | Above the button, 8pt gap | When special ability is first unlocked (post-tutorial) |

### 6.3 Label Rendering

```
Label Style:
  Font: Pixel font (same as UI headers)
  Size: 14pt
  Color: #FFFFFF
  Opacity: 70% initial (Phase 1), fades as described
  Outline: 1px black (#000000 at 80%) for readability over game background
  Background: None (text only)
  Animation: Gentle pulse (opacity oscillates +/- 10%, 2s cycle) during Phase 1
  Shadow: 1px drop shadow (black, 50% opacity, offset 1pt down)
```

### 6.4 Use Counter Tracking

Each action type has an independent use counter stored in `PlayerPrefs`:

```
ghost_label_move_count:   int (incremented each frame of movement input, counted per "use session" = touch down to touch up)
ghost_label_jump_count:   int (incremented per jump press)
ghost_label_attack_count: int (incremented per attack press)
ghost_label_special_count: int (incremented per special press)
```

A "use" is defined as a single press-and-release cycle:
- Movement: One continuous touch on the joystick/d-pad counts as 1 use. Release and re-touch = 2 uses.
- Jump: Each tap of the jump button = 1 use.
- Attack: Each tap of the attack button = 1 use.

### 6.5 Accessibility Override

Setting: Settings > Accessibility > "Always Show Button Labels"
- When enabled, ghost labels remain at Phase 1 (70% opacity) permanently, on all levels
- Default: Off
- VoiceOver users: Labels serve as accessibility labels too (redundant with VoiceOver but harmless)

---

## 7. Returning Player Detection

### 7.1 Detection Methods

The game identifies returning/experienced players through multiple signals:

| Signal | Detection Method | Action |
|--------|-----------------|--------|
| Save data exists | `PlayerPrefs` contains `tutorialComplete == true` | Skip tutorial, go to main menu with "Continue" option |
| iCloud save exists | CloudKit check on first launch | Offer to restore progress, skip tutorial if restored |
| Level 1 fast completion | Player completes Level 1 in < 15 seconds | Flag as experienced; skip Level 2 hints |
| Level 2 fast completion | Player completes Level 2 in < 20 seconds, no damage taken | Flag as expert; skip Level 3 hints |
| Pre-emptive button use | Player presses Attack button before Level 2 (before it's taught) | Flag that action as "already known"; skip its ghost label |
| Jump without prompt | Player jumps before the "JUMP" ghost label would appear | Flag jump as "already known"; suppress ghost label |

### 7.2 Experience Flags

```json
{
  "tutorial_state": {
    "tutorial_complete": false,
    "current_level": 1,
    "movement_known": false,
    "jump_known": false,
    "attack_known": false,
    "special_known": false,
    "checkpoint_known": false,
    "powerup_known": false,
    "experienced_player": false,
    "expert_player": false
  }
}
```

### 7.3 Skip Tutorial Flow

**For players with existing save data (tutorialComplete == true):**

| Step | Action |
|------|--------|
| 1 | App launches directly to Main Menu (not Level 1) |
| 2 | Main Menu shows "Continue" (resume from last checkpoint) and "New Game" |
| 3 | "Continue" loads last saved state |
| 4 | "New Game" asks "Start from Level 1? (Your previous save will be kept.)" [Yes] [No] |

**For new installations with iCloud save:**

| Step | Action |
|------|--------|
| 1 | App detects iCloud save during initialization |
| 2 | Brief dialog: "Save data found. Restore your progress?" [Restore] [Start Fresh] |
| 3 | "Restore" loads iCloud save, skips tutorial |
| 4 | "Start Fresh" begins at Level 1 as a new player |

### 7.4 Pre-emptive Skill Detection

During Level 1 and Level 2, the game monitors for actions the player performs before being taught:

```csharp
// SkillDetectionManager.cs
// Monitors player actions during tutorial levels to detect
// pre-existing knowledge and suppress unnecessary hints.

public class SkillDetectionManager : MonoBehaviour
{
    // If the player presses Attack during Level 1 (before it's taught),
    // mark "attack_known" = true and skip the highlight in Level 2.
    //
    // If the player jumps over the first gap within 1 second of
    // reaching it (before the ghost label appears), mark
    // "jump_known" = true and suppress the JUMP label.

    private Dictionary<string, bool> knownSkills = new Dictionary<string, bool>();

    void OnAttackPressed()
    {
        if (currentLevel == 1)
        {
            knownSkills["attack"] = true;
            // Attack ghost label will not appear in Level 2
        }
    }

    void OnJumpPressed()
    {
        if (currentLevel == 1 && !jumpLabelShown)
        {
            knownSkills["jump"] = true;
            // Jump ghost label will not appear
        }
    }
}
```

---

## 8. Tutorial Replay System

### 8.1 Access

Settings > Game > "Replay Tutorial"

### 8.2 Replay Behavior

| Aspect | Behavior |
|--------|----------|
| Trigger | Player taps "Replay Tutorial" in settings |
| Confirmation | "Replay the tutorial? Your save will not be affected." [Replay] [Cancel] |
| Levels | Loads Level 1, then Level 2, then Level 3 in sequence |
| Ghost labels | Reset to Phase 1 (full visibility) for the duration of the replay |
| Experience flags | NOT reset (the player's main save retains "tutorial_complete" = true) |
| Score | Not tracked during replay (score display shows "--" instead of numbers) |
| Completion | After Level 3, shows "Tutorial Complete!" and returns to Main Menu |
| Save impact | No impact on main save data. Tutorial replay is a sandboxed session. |
| Accessibility | If "Always Show Button Labels" is enabled, labels remain visible as usual |

### 8.3 Settings Integration

The "Replay Tutorial" button in Settings:

| Property | Value |
|----------|-------|
| Label | "Replay Tutorial" |
| Type | Action button (not a toggle) |
| Touch target | 180 x 48 pt |
| Visible when | Always (regardless of tutorial completion status) |
| VoiceOver | Label: "Replay Tutorial", Hint: "Double-tap to replay the tutorial levels" |

---

## 9. Implementation Details

### 9.1 Tutorial State Machine

```csharp
// TutorialManager.cs
// Manages the tutorial state across Levels 1-3.
// Tracks player knowledge, controls ghost labels, and handles
// experience detection.

public class TutorialManager : MonoBehaviour
{
    public enum TutorialState
    {
        NotStarted,
        Level1_Movement,
        Level1_FirstGap,
        Level1_SecondGap,
        Level1_Complete,
        Level2_PreCombat,
        Level2_EnemyEncounter,
        Level2_PostCombat,
        Level2_Complete,
        Level3_Review,
        Level3_PowerUp,
        Level3_Checkpoint,
        Level3_Hazard,
        Level3_Combination,
        Level3_Complete,
        TutorialComplete
    }

    [SerializeField] private TutorialState currentState = TutorialState.NotStarted;

    // Ghost label references
    [SerializeField] private GhostLabel movementLabel;
    [SerializeField] private GhostLabel jumpLabel;
    [SerializeField] private GhostLabel attackLabel;
    [SerializeField] private GhostLabel specialLabel;

    // Skill detection
    private SkillDetectionManager skillDetection;

    // Timing
    private float levelStartTime;
    private const float FAST_LEVEL1_THRESHOLD = 15f;  // seconds
    private const float FAST_LEVEL2_THRESHOLD = 20f;  // seconds

    void Start()
    {
        // Check if tutorial already complete
        if (PlayerPrefs.GetInt("tutorialComplete", 0) == 1)
        {
            currentState = TutorialState.TutorialComplete;
            DisableAllGhostLabels();
            return;
        }

        currentState = TutorialState.Level1_Movement;
        levelStartTime = Time.time;
    }

    public void OnLevelComplete(int levelNumber)
    {
        float completionTime = Time.time - levelStartTime;

        switch (levelNumber)
        {
            case 1:
                if (completionTime < FAST_LEVEL1_THRESHOLD)
                {
                    // Experienced player detected
                    SaveTutorialData("experienced_player", true);
                }
                currentState = TutorialState.Level1_Complete;
                break;

            case 2:
                if (completionTime < FAST_LEVEL2_THRESHOLD)
                {
                    // Expert player detected
                    SaveTutorialData("expert_player", true);
                }
                currentState = TutorialState.Level2_Complete;
                break;

            case 3:
                currentState = TutorialState.TutorialComplete;
                PlayerPrefs.SetInt("tutorialComplete", 1);
                PlayerPrefs.Save();
                DisableAllGhostLabels();
                break;
        }

        levelStartTime = Time.time; // Reset for next level
    }
}
```

### 9.2 Ghost Label Component

```csharp
// GhostLabel.cs
// A semi-transparent label that appears near a control button,
// identifies its function, and fades away after repeated use.

public class GhostLabel : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string labelText = "JUMP";
    [SerializeField] private int usesToFade = 3;           // Phase 1 duration
    [SerializeField] private int usesToDisappear = 6;       // Total uses to fully remove
    [SerializeField] private float initialOpacity = 0.7f;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private CanvasGroup canvasGroup;

    private int useCount = 0;
    private bool isVisible = false;
    private bool isPermanent = false; // True if "Always Show" accessibility setting

    public void Show()
    {
        if (useCount >= usesToDisappear && !isPermanent) return;

        isVisible = true;
        gameObject.SetActive(true);
        UpdateOpacity();
    }

    public void RecordUse()
    {
        useCount++;
        SaveUseCount();
        UpdateOpacity();

        if (useCount >= usesToDisappear && !isPermanent)
        {
            Hide();
        }
    }

    private void UpdateOpacity()
    {
        if (isPermanent)
        {
            canvasGroup.alpha = initialOpacity;
            return;
        }

        if (useCount < usesToFade)
        {
            // Phase 1: Full visibility
            canvasGroup.alpha = initialOpacity;
        }
        else if (useCount < usesToDisappear)
        {
            // Phase 2: Fading
            float fadeProgress = (float)(useCount - usesToFade) / (usesToDisappear - usesToFade);
            canvasGroup.alpha = Mathf.Lerp(initialOpacity, 0f, fadeProgress);
        }
        else
        {
            // Phase 3: Gone
            canvasGroup.alpha = 0f;
        }
    }

    public void ResetForReplay()
    {
        useCount = 0;
        canvasGroup.alpha = initialOpacity;
        SaveUseCount();
    }

    private void SaveUseCount()
    {
        PlayerPrefs.SetInt($"ghost_label_{labelText.ToLower()}_count", useCount);
    }

    public void SetPermanent(bool permanent)
    {
        isPermanent = permanent;
        if (permanent)
        {
            canvasGroup.alpha = initialOpacity;
            gameObject.SetActive(true);
        }
    }

    private void Hide()
    {
        isVisible = false;
        // Animate fade to 0 over 0.3s, then deactivate
        StartCoroutine(FadeOutAndDeactivate(0.3f));
    }

    private System.Collections.IEnumerator FadeOutAndDeactivate(float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
```

### 9.3 Level 1 Fail-Safe Floor

```csharp
// FailSafeFloor.cs
// Invisible floor below gaps in Level 1 that catches the player
// and bounces them back to the ledge, preventing death during tutorial.

public class FailSafeFloor : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;  // Left ledge of the gap
    [SerializeField] private float bounceForce = 8f;
    [SerializeField] private float catchDelay = 0.2f;  // Brief fall before catch

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(CatchAndBounce(other.GetComponent<PlayerController>()));
        }
    }

    private System.Collections.IEnumerator CatchAndBounce(PlayerController player)
    {
        // Brief moment of falling (player sees they're falling)
        yield return new WaitForSeconds(catchDelay);

        // Teleport to respawn point
        player.transform.position = respawnPoint.position;

        // Apply upward bounce
        player.GetComponent<Rigidbody2D>().velocity = Vector2.up * bounceForce;

        // Brief invulnerability flash (visual cue that something happened)
        player.FlashSprite(0.3f);

        // No damage, no death, no penalty
    }

    // Destroy this component when the level is not Level 1
    void Start()
    {
        if (!TutorialManager.Instance.IsLevel1)
        {
            Destroy(gameObject);
        }
    }
}
```

### 9.4 First Launch Startup Sequence

```csharp
// FirstLaunchManager.cs
// Handles the critical first-launch flow:
// App open -> game scene loaded -> player input within 5 seconds.

public class FirstLaunchManager : MonoBehaviour
{
    [SerializeField] private float nudgeArrowDelay = 2.5f;     // seconds after spawn
    [SerializeField] private float tapToMoveDelay = 5.0f;      // fallback hint delay
    [SerializeField] private float tapToMoveFadeDuration = 3f;  // how long hint stays

    [SerializeField] private ParticleSystem nudgeArrow;         // Right-pointing particles
    [SerializeField] private TextMeshProUGUI tapToMoveText;     // "Tap to move" fallback

    private bool playerHasMoved = false;

    void Start()
    {
        if (PlayerPrefs.GetInt("tutorialComplete", 0) == 1)
        {
            // Not first launch, load main menu instead
            SceneManager.LoadScene("MainMenu");
            return;
        }

        if (PlayerPrefs.GetInt("firstLaunchDone", 0) == 1)
        {
            // Returning to tutorial (not first-ever launch but tutorial not complete)
            // Skip the nudge sequence
            return;
        }

        // First ever launch
        tapToMoveText.gameObject.SetActive(false);
        StartCoroutine(FirstLaunchSequence());
    }

    private System.Collections.IEnumerator FirstLaunchSequence()
    {
        // Wait for scene to fully render (1 frame)
        yield return null;

        // Player character is already visible (spawned by level loader)
        // Ghost labels fade in (handled by GhostLabel.Show())

        // Wait, then show nudge arrow
        yield return new WaitForSeconds(nudgeArrowDelay);

        if (!playerHasMoved)
        {
            nudgeArrow.Play(); // Subtle right-pointing particle hint
        }

        // Wait more, then show text fallback
        yield return new WaitForSeconds(tapToMoveDelay - nudgeArrowDelay);

        if (!playerHasMoved)
        {
            tapToMoveText.gameObject.SetActive(true);
            tapToMoveText.alpha = 0f;

            // Fade in to 30% opacity
            float elapsed = 0f;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                tapToMoveText.alpha = Mathf.Lerp(0f, 0.3f, elapsed / 0.5f);
                yield return null;
            }

            // Auto-fade after duration
            yield return new WaitForSeconds(tapToMoveFadeDuration);
            tapToMoveText.gameObject.SetActive(false);
        }

        PlayerPrefs.SetInt("firstLaunchDone", 1);
        PlayerPrefs.Save();
    }

    // Called by PlayerController when any movement input is detected
    public void OnPlayerMoved()
    {
        playerHasMoved = true;
        nudgeArrow.Stop();
        tapToMoveText.gameObject.SetActive(false);
    }
}
```

---

**Cross-References**:
- Screen layouts and transition animations: See `ui_layout_spec.md` (Assessment 5.1)
- Control positions and schemes: See `control_layout_spec.md` (Assessment 5.2)
- Accessibility-specific tutorial features: See `accessibility_spec.md` (Assessment 5.3)
- Auto-save at checkpoints: See `interruption_handling_spec.md` (Assessment 5.5)

---

Last Updated: 2026-02-04
Status: Active
Version: 1.0
