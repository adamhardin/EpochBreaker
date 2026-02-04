# Assessment 5.2 - Touch Ergonomics & Control Layout Specification

**Module**: 5 - Mobile UX & Accessibility
**Version**: 2.0
**Last Updated**: 2026-02-04
**Status**: Active
**Engine**: Unity 2022 LTS
**Orientation**: Landscape only (locked)
**Minimum Device**: iPhone 11
**Target**: 60fps
**Genre**: 16-bit side-scrolling mobile shooter

---

## Table of Contents

1. [Thumb Reach Zone Analysis](#1-thumb-reach-zone-analysis)
2. [Control Scheme](#2-control-scheme)
3. [Button Placement Specifications](#3-button-placement-specifications)
4. [Control Customization System](#4-control-customization-system)
5. [Haptic Feedback Mapping](#5-haptic-feedback-mapping)
6. [Fatigue Prevention](#6-fatigue-prevention)
7. [Unity Implementation](#7-unity-implementation)
8. [Auto-Fire Visual Feedback](#8-auto-fire-visual-feedback)

---

## 1. Thumb Reach Zone Analysis

### 1.1 Landscape Two-Thumb Grip (Primary)

The dominant grip for landscape mobile gaming. The player holds the device with both hands, thumbs on the screen, index/middle fingers supporting the back.

**Left Thumb Reach Zones** (measured from bottom-left corner of safe area):

| Zone | X Range (% of screen width) | Y Range (% of screen height) | Comfort Level |
|------|------|------|------|
| Easy (natural rest) | 0% - 15% | 5% - 45% | Effortless, unlimited use |
| Comfortable | 0% - 22% | 0% - 60% | Low effort, suitable for frequent use |
| Stretch | 15% - 30% | 40% - 80% | Moderate effort, occasional use only |
| Unreachable | > 35% | > 75% | Requires grip shift, avoid for gameplay |

**Right Thumb Reach Zones** (measured from bottom-right corner of safe area):

| Zone | X Range (% from right) | Y Range (% of screen height) | Comfort Level |
|------|------|------|------|
| Easy (natural rest) | 0% - 15% | 5% - 45% | Effortless, unlimited use |
| Comfortable | 0% - 22% | 0% - 60% | Low effort, suitable for frequent use |
| Stretch | 15% - 30% | 40% - 80% | Moderate effort, occasional use only |
| Unreachable | > 35% | > 75% | Requires grip shift, avoid for gameplay |

**Thumb Rest Position**: When the player's thumbs are relaxed on the screen in landscape grip, they naturally rest at approximately:
- Left thumb: X = 8%, Y = 25% (from bottom-left of safe area)
- Right thumb: X = 92%, Y = 25% (from bottom-left of safe area, i.e., 8% from right)

### 1.2 One-Handed Grip (Accessibility Mode)

For one-handed play mode, all controls must be reachable by a single thumb. This requires a fundamentally different layout (see Accessibility Spec).

### 1.3 Device-Specific Thumb Reach

| Device | Screen Width (pt) | 15% Width (pt) | Easy Zone Width | Notes |
|--------|-------------------|-----------------|-----------------|-------|
| iPhone 11 | 896 | 134 | 134pt per side | Largest easy zone |
| iPhone 14 Pro | 852 | 128 | 128pt per side | Slightly narrower |
| iPhone 15 Pro Max | 932 | 140 | 140pt per side | Widest but heavier; fatigue concern |

On iPhone 15 Pro Max, the extra width means the thumb stretch zone starts further out. Place controls no more than 12% from edge to keep within easy zone on this larger device.

---

## 2. Control Scheme

### 2.1 Game Concept Summary

The player controls a small character in a side-scrolling shooter. Weapon attachments are picked up from the environment and auto-attach to the character's body. Once attached, weapons fire automatically at the nearest enemy -- there is no fire button. The player's role is to navigate, jump, dodge, and cycle weapon targeting between on-screen enemies. The player blasts through destructible environments as they progress.

### 2.2 Control Layout Overview

The control scheme uses a two-thumb landscape grip with a fixed D-pad on the left and two action buttons on the right. Weapons auto-fire continuously; the player never needs to press a fire button.

```
+------------------------------------------------------------------------+
|                                                         [PAUSE]        |
|                                                                        |
|                                                                        |
|                                                                        |
|                                                        [B]             |
|       [<] [>]                                             [A]          |
|        D-PAD                                                           |
+------------------------------------------------------------------------+
  LEFT THUMB                                            RIGHT THUMB
  (Movement)                                   (Jump + Target Cycle)
```

**Left Side -- Fixed D-Pad**:
- Position: Bottom-left of safe area, fixed (not floating)
- Type: 2-directional pad (Left, Right only)
- Overall D-pad size: 120 x 60 pt (wide rectangle accommodating left/right arrows)
- Individual direction touch target: 56 x 56 pt (overlapping into center)
- Dead zone radius: 16pt from center
- Up and Down on the D-pad are not mapped to any gameplay action. If the player presses up, nothing happens. Up is NOT jump.
- The D-pad is always visible on screen at the configured opacity

**Right Side -- Action Buttons**:
- Jump button (A): Primary action button, larger size
  - 72 x 72 pt touch target, 56 x 56 pt visual
  - Tap to jump. Tap again in air for double-jump (if unlocked)
  - Hold while powered up to trigger special attack (hold threshold: 0.4s)
  - Blue tint, "A" label
- Target Cycle button (B): Secondary action button
  - 64 x 64 pt touch target, 48 x 48 pt visual
  - Tap to cycle weapon auto-aim to the next enemy on screen
  - Target order cycles through enemies left-to-right by screen position
  - If no enemies on screen, button press does nothing (no haptic)
  - Green tint, "B" label

**No Fire Button**: Weapons auto-fire at all times once the player has at least one weapon attachment. See Section 8 for auto-fire visual feedback.

**Pause Button**:
- Top-right corner of safe area
- 36 x 36 pt visual, 48 x 48 pt touch target
- "||" icon
- Always visible at full opacity regardless of control opacity setting

### 2.3 Input Summary Table

| Input | Control | Thumb | Action |
|-------|---------|-------|--------|
| Move Left | D-pad Left | Left | Character moves left |
| Move Right | D-pad Right | Left | Character moves right |
| Stop | Release D-pad | Left | Character stops (with deceleration) |
| Jump | Tap A button | Right | Character jumps; tap again in air for double-jump |
| Special Attack | Hold A button (powered up) | Right | Triggers special attack if power gauge is full |
| Cycle Target | Tap B button | Right | Switches weapon auto-aim to next enemy |
| Fire Weapons | Automatic | N/A | All attached weapons fire at targeted enemy continuously |
| Pause | Tap Pause | Right (stretch) | Opens pause menu |

---

## 3. Button Placement Specifications

### 3.1 Exact Positions

All positions are percentages of the **safe area** dimensions, measured from the **bottom-left** corner of the safe area.

#### Left Side Controls -- D-Pad

| Element | X (% from left) | Y (% from bottom) | Visual Size (pt) | Touch Target (pt) | Notes |
|---------|------------------|--------------------|-------------------|--------------------|-------|
| D-Pad Center | 10% | 25% | 120 x 60 (full pad) | N/A | Fixed position, always visible |
| D-Pad Left Arrow | 5% | 25% | 40 x 40 arrow | 56 x 56 | Left movement |
| D-Pad Right Arrow | 15% | 25% | 40 x 40 arrow | 56 x 56 | Right movement |

The D-pad uses a horizontal layout with only Left and Right arrows. There are no Up or Down arrows rendered. The visual design is a flat, wide capsule-shaped base with two directional arrows.

#### Right Side Controls -- Action Buttons

| Element | X (% from left) | Y (% from bottom) | Visual Size (pt) | Touch Target (pt) | Notes |
|---------|------------------|--------------------|-------------------|--------------------|-------|
| Jump (A) | 90% | 18% | 56 x 56 | 72 x 72 | Blue tint, "A" label, primary action, largest button |
| Target Cycle (B) | 82% | 36% | 48 x 48 | 64 x 64 | Green tint, "B" label, above-left of Jump |
| Pause | 97% | 93% | 36 x 36 | 48 x 48 | Top-right corner, "||" icon |

**Button Arrangement Rationale**:
- Jump (A) is placed at the natural right-thumb rest position (bottom-right), making it effortless for the most frequent right-thumb action
- Target Cycle (B) is above and to the left of Jump, within the comfortable zone but offset enough to prevent accidental presses
- The diagonal offset between A and B (8% horizontal, 18% vertical) provides clear spatial separation
- Pause is in the top-right corner, deliberately in the stretch zone to prevent accidental taps during gameplay

#### Absolute Position Examples (iPhone 14 Pro, safe area 734 x 372 pt)

| Element | X (pt from safe left) | Y (pt from safe bottom) | Touch Rect (pt) |
|---------|------------------------|--------------------------|-----------------|
| D-Pad Left | 37 | 93 | (9, 65) to (65, 121) |
| D-Pad Right | 110 | 93 | (82, 65) to (138, 121) |
| Jump (A) | 661 | 67 | (625, 31) to (697, 103) |
| Target Cycle (B) | 602 | 134 | (570, 102) to (634, 166) |
| Pause | 712 | 346 | (688, 322) to (736, 370) |

### 3.2 Safe Area Handling

All control positions are relative to the device safe area, not the full screen. On devices with notches or Dynamic Island, the safe area insets ensure controls are never obscured:

| Edge | Typical Inset (pt) | Handling |
|------|---------------------|----------|
| Left (notch side, landscape) | 44-59 | D-pad shifts right of inset |
| Right (notch side, landscape) | 44-59 | Pause button shifts left of inset |
| Top | 0-20 | No gameplay controls in this region |
| Bottom | 0-21 | D-pad and action buttons clear of home indicator |

---

## 4. Control Customization System

### 4.1 Opacity Control

| Setting | Opacity Value | Visual Appearance |
|---------|---------------|-------------------|
| Level 1 | 25% | Ghost outline, minimal distraction |
| Level 2 | 50% | Semi-transparent, visible but unobtrusive |
| Level 3 | 75% | Clearly visible (DEFAULT) |
| Level 4 | 100% | Fully opaque, maximum visibility |

**Implementation**: Set the `CanvasGroup.alpha` on the controls container. All child elements inherit the opacity. The Pause button is excluded from opacity changes and remains at 100% at all times.

**Access**: Settings > Controls > Control Opacity (slider with 4 detents)

### 4.2 Position Customization (Drag to Reposition)

**Entering Edit Mode**:
1. Settings > Controls > Customize Layout > "EDIT POSITIONS"
2. Game scene loads with controls overlay at current positions
3. Each draggable control has a blue highlight border and a drag handle icon
4. Background text: "Drag controls to reposition. Tap DONE when finished."

**Drag Behavior**:

| Rule | Value | Notes |
|------|-------|-------|
| Draggable elements | D-Pad, Jump (A), Target Cycle (B) | Pause button NOT draggable (always top-right) |
| Snap to grid | 8pt grid | Prevents sub-pixel positioning |
| Boundary constraint | Within safe area + 20pt padding | Cannot drag outside interactive zone |
| Overlap prevention | Minimum 8pt between touch targets | Elements push apart if dragged too close |
| Side constraint | D-Pad locked to left 40%; A and B locked to right 40% | Prevents cross-hand placement |
| Reset option | "RESET TO DEFAULT" button | Restores factory positions |

**Persistence**: Custom positions saved to `UserDefaults` (key: `control_positions_v2`) as a JSON dictionary:

```json
{
  "scheme": "dpad_ab",
  "positions": {
    "dpad": { "x_pct": 10.0, "y_pct": 25.0 },
    "button_a": { "x_pct": 90.0, "y_pct": 18.0 },
    "button_b": { "x_pct": 82.0, "y_pct": 36.0 }
  },
  "opacity": 75,
  "button_size": "medium",
  "version": 2
}
```

### 4.3 Button Size Adjustment

| Size Preset | Visual Scale | Touch Target Scale | Label |
|-------------|-------------|-------------------|-------|
| Small | 0.8x | 0.9x (min 44pt) | "Small" |
| Medium | 1.0x | 1.0x | "Medium" (DEFAULT) |
| Large | 1.25x | 1.2x | "Large" |
| Extra Large | 1.5x | 1.4x | "Extra Large" (accessibility) |

Touch targets never fall below 44 x 44 pt regardless of visual size. The D-pad scales uniformly (both arrows and base scale together).

**Access**: Settings > Controls > Button Size (4 preset buttons)

---

## 5. Haptic Feedback Mapping

### 5.1 Haptic Engine

Uses Unity's iOS haptic integration via `UIImpactFeedbackGenerator`, `UINotificationFeedbackGenerator`, and `UISelectionFeedbackGenerator`.

Requires iPhone 8 or later (all supported devices qualify). Powered by the Taptic Engine.

### 5.2 Game Event to Haptic Mapping

| # | Game Event | Haptic Type | Intensity | Duration | UIFeedbackGenerator Type | Notes |
|---|-----------|-------------|-----------|----------|--------------------------|-------|
| 1 | Weapon Pickup | Notification Success | 0.7 | ~0.2s pattern | UINotificationFeedbackGenerator(.success) | Satisfying confirmation when new weapon attaches to character |
| 2 | Weapon Firing (per shot) | Soft Impact | 0.15 | Instant | UIImpactFeedbackGenerator(.soft) | Very subtle pulse; must not cause fatigue during continuous auto-fire. Rate-limited to max 4 pulses/second regardless of weapon fire rate |
| 3 | Destructible Wall Break | Heavy Impact | 0.8 | Instant | UIImpactFeedbackGenerator(.heavy) | Powerful crunch when environment shatters |
| 4 | Enemy Hit (projectile lands) | Rigid Impact | 0.4 | Instant | UIImpactFeedbackGenerator(.rigid) | Brief confirmation of damage dealt. Rate-limited: max 6/second across all weapons |
| 5 | Enemy Defeat (killed) | Medium Impact | 0.7 | Instant | UIImpactFeedbackGenerator(.medium) | Distinct pop when enemy is destroyed |
| 6 | Player Damaged | Notification Error | 1.0 | ~0.3s pattern | UINotificationFeedbackGenerator(.error) | Double-buzz "error" pattern, unmistakable warning |
| 7 | Player Death | Heavy Impact x3 | 1.0 | 0.6s total | 3x UIImpactFeedbackGenerator(.heavy) at 0.0s, 0.2s, 0.4s | Dramatic triple-thud |
| 8 | Player Jump | Light Impact | 0.4 | Instant | UIImpactFeedbackGenerator(.light) | Brief tap on liftoff |
| 9 | Player Land | Medium Impact | 0.6 | Instant | UIImpactFeedbackGenerator(.medium) | Satisfying thud on ground contact |
| 10 | Special Attack (hold A) | Heavy Impact + Notification Success | 1.0 | 0.4s total | UIImpactFeedbackGenerator(.heavy) at 0.0s, UINotificationFeedbackGenerator(.success) at 0.2s | Two-phase feedback: heavy charge release then success confirmation |
| 11 | Boss Appear | Heavy Impact x2 | 0.9 | 0.4s total | 2x UIImpactFeedbackGenerator(.heavy) at 0.0s, 0.2s | Dramatic entrance |
| 12 | Boss Defeated | Notification Success x3 | 1.0 | 0.8s total | 3x UINotificationFeedbackGenerator(.success) at 0.0s, 0.3s, 0.6s | Triumphant triple-buzz |
| 13 | Level Complete | Notification Success x2 | 1.0 | 0.5s total | 2x UINotificationFeedbackGenerator(.success) at 0.0s, 0.3s | Celebratory double-buzz |
| 14 | Target Cycle (B press) | Selection Changed | 0.3 | Instant | UISelectionFeedbackGenerator() | Subtle tick confirming target switch |
| 15 | Power Gauge Full | Notification Warning | 0.6 | ~0.2s pattern | UINotificationFeedbackGenerator(.warning) | Alerts player that special attack is available |
| 16 | Menu Button Press | Selection Changed | 0.3 | Instant | UISelectionFeedbackGenerator() | Subtle tick for UI interaction |

### 5.3 Auto-Fire Haptic Rate Limiting

Because weapons fire automatically and continuously, the weapon firing haptic (event #2) requires special handling to avoid Taptic Engine fatigue and player annoyance:

| Weapon Fire Rate | Haptic Pulse Rate | Behavior |
|------------------|-------------------|----------|
| 1-4 shots/second | 1:1 (every shot) | Each shot produces a subtle pulse |
| 5-8 shots/second | 1:2 (every other shot) | Pulses at half fire rate |
| 9-12 shots/second | Fixed at 4/second | Constant subtle rhythm regardless of fire rate |
| 13+ shots/second | Fixed at 4/second | Same cap; prevents buzzing sensation |

When multiple weapons fire simultaneously, only one haptic pulse fires per interval (the strongest weapon's pulse).

### 5.4 Haptic Settings

| Setting | Options | Default | Location |
|---------|---------|---------|----------|
| Haptic Feedback | On / Off | On | Settings > Audio > Haptic Feedback |
| Haptic Intensity | 0% - 100% slider | 75% | Settings > Audio > Haptic Intensity (visible when Haptics On) |
| Weapon Fire Haptics | On / Off | On | Settings > Audio > Weapon Fire Haptics (separate toggle) |

The weapon fire haptic toggle is independent because some players may want haptics for all other events but find continuous weapon haptics distracting.

**Implementation Note**: Haptic intensity is implemented by selecting lighter feedback types at lower intensity settings:

| User Setting | Heavy -> | Medium -> | Light -> | Rigid -> | Soft -> |
|-------------|----------|-----------|----------|----------|---------|
| 100% | Heavy | Medium | Light | Rigid | Soft |
| 75% | Medium | Light | Light | Light | Soft |
| 50% | Light | Light | Soft | Soft | (skip) |
| 25% | Soft | Soft | (skip) | (skip) | (skip) |
| 0% | (disabled) | (disabled) | (disabled) | (disabled) | (disabled) |

### 5.5 Haptic Cooldown

To prevent haptic fatigue and Taptic Engine throttling:
- Minimum interval between haptic events: 50ms
- If multiple haptics would fire within 50ms, only the highest-priority one fires
- Priority order: Player Death > Player Damaged > Special Attack > Boss Events > Enemy Defeat > Destructible Break > Enemy Hit > Level Complete > Weapon Pickup > all others
- Weapon firing haptics use a separate cooldown track (250ms minimum) so they do not suppress gameplay-critical haptics

---

## 6. Fatigue Prevention

### 6.1 Repetitive Strain Analysis

| Action | Frequency (per minute) | Thumb Used | Strain Risk | Mitigation |
|--------|----------------------|------------|-------------|------------|
| Horizontal movement | Continuous hold | Left | Low (sustained light pressure) | Fixed D-pad requires minimal force; player holds direction naturally |
| Jump | 8-15 taps | Right | Low-Medium | Large button (72pt target), short travel, intermittent taps |
| Target Cycle | 2-6 taps | Right | Very Low | Infrequent, only when new enemies appear |
| Weapon firing | Automatic (0 taps) | None | None | No button press required; auto-fire eliminates rapid-tap fatigue entirely |
| Special Attack | 0-2 holds | Right | Very Low | Brief hold (0.4s), very infrequent |
| Pause | < 1 tap | Right | Negligible | Rare use |

**Key Fatigue Advantage**: The auto-fire system eliminates the most common source of mobile gaming thumb fatigue -- rapid repeated tapping of a fire/attack button. Players never need to mash any button. The left thumb holds a direction (low sustained pressure) and the right thumb taps intermittently to jump or cycle targets.

### 6.2 Sustained Pressure Limits

| Control | Max Sustained Hold | Behavior After Limit | Notes |
|---------|-------------------|---------------------|-------|
| D-Pad (movement) | Unlimited | N/A | Binary digital input; player modulates naturally by releasing |
| Jump (hold for special) | 0.4 seconds | Special attack fires, input released internally | Only functions when power gauge is full; otherwise hold has no effect beyond standard jump |
| Target Cycle | Instant tap only | N/A | No hold behavior, immediate on press |

No action in the game requires sustained pressure beyond 0.4 seconds.

### 6.3 Natural Rest Points

The game provides natural breaks to reduce continuous input demand:

| Rest Point Type | Frequency | Duration | Input Required |
|----------------|-----------|----------|----------------|
| Checkpoint safe zone | Every 45-90 seconds of gameplay | Until player moves forward | None (safe area, no enemies) |
| Level transition screen | Between every level | Until player taps "Next" | Single tap to continue |
| Cutscene/dialog | 2-3 per world (every 5-8 levels) | 10-30 seconds | Tap to advance (optional auto-advance) |
| Level complete screen | After every level | Unlimited | Single tap to continue |
| Weapon attachment animation | On weapon pickup | 1.5 seconds | None (brief cinematic moment) |

### 6.4 Session Length Reminder

| Setting | Behavior |
|---------|----------|
| Off | No reminders (DEFAULT) |
| 30 minutes | Non-intrusive banner at top: "You've been playing for 30 minutes. Consider taking a break." Auto-dismisses after 5 seconds. No gameplay interruption. |
| 60 minutes | Same banner style at 60 minutes |

Banner does not pause the game. It appears in the HUD notification area (top-center, below safe area top). Tapping it dismisses immediately.

---

## 7. Unity Implementation

### 7.1 Input System Setup

Use Unity's new Input System package (com.unity.inputsystem) for all touch handling.

```csharp
// TouchInputManager.cs
// Central manager for all touch-based gameplay input.
// Handles the fixed D-pad for movement, Jump (A) button,
// Target Cycle (B) button, and Pause button.
// Weapons auto-fire and are not controlled by input.

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class TouchInputManager : MonoBehaviour
{
    public static TouchInputManager Instance { get; private set; }

    // Events for gameplay systems to subscribe to
    public event System.Action<float> OnMovementInput;        // -1 (left), 0 (idle), 1 (right)
    public event System.Action OnJumpPressed;
    public event System.Action OnJumpReleased;
    public event System.Action<float> OnJumpHeld;             // sends hold duration for special attack
    public event System.Action OnTargetCyclePressed;
    public event System.Action OnPausePressed;

    // Control positions (customizable, percentages of safe area)
    [System.Serializable]
    public struct ControlPositions
    {
        public Vector2 dpadCenter;          // default (10%, 25%)
        public Vector2 buttonAPosition;     // default (90%, 18%)
        public Vector2 buttonBPosition;     // default (82%, 36%)
    }

    [SerializeField] private ControlPositions positions;
    [SerializeField] [Range(0.25f, 1.0f)] private float controlOpacity = 0.75f;

    [Header("D-Pad Settings")]
    [SerializeField] private float dpadDeadZone = 16f;        // pt from center

    [Header("Jump Hold Settings")]
    [SerializeField] private float specialAttackHoldThreshold = 0.4f;  // seconds

    [Header("References")]
    [SerializeField] private CanvasGroup controlsCanvasGroup;
    [SerializeField] private DPadController dpadController;
    [SerializeField] private ActionButton jumpButton;
    [SerializeField] private ActionButton targetCycleButton;
    [SerializeField] private ActionButton pauseButton;

    private float jumpHoldTimer = 0f;
    private bool isJumpHeld = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        EnhancedTouchSupport.Enable();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        // Process D-Pad input
        float moveDir = dpadController.GetHorizontalInput();
        OnMovementInput?.Invoke(moveDir);

        // Track jump hold duration for special attack
        if (isJumpHeld)
        {
            jumpHoldTimer += Time.deltaTime;
            OnJumpHeld?.Invoke(jumpHoldTimer);
        }
    }

    // Called by Jump (A) button UI event
    public void HandleJumpDown()
    {
        isJumpHeld = true;
        jumpHoldTimer = 0f;
        OnJumpPressed?.Invoke();
        HapticManager.Instance?.TriggerHaptic(
            HapticManager.HapticType.Light,
            HapticManager.HapticPriority.Medium);
    }

    public void HandleJumpUp()
    {
        if (jumpHoldTimer >= specialAttackHoldThreshold)
        {
            // Special attack triggered (if powered up -- gameplay logic handles validation)
        }
        isJumpHeld = false;
        jumpHoldTimer = 0f;
        OnJumpReleased?.Invoke();
    }

    // Called by Target Cycle (B) button UI event
    public void HandleTargetCycle()
    {
        OnTargetCyclePressed?.Invoke();
        HapticManager.Instance?.TriggerHaptic(
            HapticManager.HapticType.SelectionChanged,
            HapticManager.HapticPriority.Low);
    }

    // Called by Pause button UI event
    public void HandlePause()
    {
        OnPausePressed?.Invoke();
        HapticManager.Instance?.TriggerHaptic(
            HapticManager.HapticType.SelectionChanged,
            HapticManager.HapticPriority.Low);
    }

    public void SetOpacity(float opacity)
    {
        controlOpacity = Mathf.Clamp(opacity, 0.25f, 1.0f);
        controlsCanvasGroup.alpha = controlOpacity;
        // Pause button excluded from opacity -- handled separately
    }
}
```

### 7.2 Fixed D-Pad Implementation

```csharp
// DPadController.cs
// Fixed 2-directional D-pad for horizontal movement.
// Left and Right only. Up and Down are not mapped.

using UnityEngine;
using UnityEngine.EventSystems;

public class DPadController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("D-Pad Settings")]
    [SerializeField] private float deadZoneRadius = 16f;     // pt from center
    [SerializeField] private RectTransform dpadRect;         // overall D-pad area
    [SerializeField] private RectTransform leftArrow;
    [SerializeField] private RectTransform rightArrow;

    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.75f);
    [SerializeField] private Color pressedColor = new Color(0.7f, 0.85f, 1f, 1f);

    private float horizontalInput = 0f;
    private bool isTouching = false;

    /// <summary>
    /// Returns -1 (left), 0 (idle), or 1 (right).
    /// </summary>
    public float GetHorizontalInput()
    {
        return horizontalInput;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isTouching = true;
        UpdateInput(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isTouching)
        {
            UpdateInput(eventData.position);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTouching = false;
        horizontalInput = 0f;
        ResetVisuals();
    }

    private void UpdateInput(Vector2 screenPosition)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dpadRect, screenPosition, null, out localPoint);

        // Only process horizontal axis
        if (Mathf.Abs(localPoint.x) < deadZoneRadius)
        {
            horizontalInput = 0f;
            ResetVisuals();
            return;
        }

        if (localPoint.x < -deadZoneRadius)
        {
            horizontalInput = -1f;
            leftArrow.GetComponent<UnityEngine.UI.Image>().color = pressedColor;
            rightArrow.GetComponent<UnityEngine.UI.Image>().color = normalColor;
        }
        else if (localPoint.x > deadZoneRadius)
        {
            horizontalInput = 1f;
            rightArrow.GetComponent<UnityEngine.UI.Image>().color = pressedColor;
            leftArrow.GetComponent<UnityEngine.UI.Image>().color = normalColor;
        }
    }

    private void ResetVisuals()
    {
        leftArrow.GetComponent<UnityEngine.UI.Image>().color = normalColor;
        rightArrow.GetComponent<UnityEngine.UI.Image>().color = normalColor;
    }
}
```

### 7.3 Auto-Fire Weapon System (Input-Independent)

```csharp
// WeaponAutoFireController.cs
// Manages automatic weapon firing. Weapons fire without player input.
// The player only controls which enemy is targeted via the Target Cycle button.

using UnityEngine;
using System.Collections.Generic;

public class WeaponAutoFireController : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private float targetScanInterval = 0.2f;     // seconds between target re-evaluation
    [SerializeField] private LayerMask enemyLayer;

    [Header("Auto-Fire Haptic Settings")]
    [SerializeField] private float hapticMinInterval = 0.25f;     // 4 pulses/second max

    private List<WeaponAttachment> attachedWeapons = new List<WeaponAttachment>();
    private List<Transform> onScreenEnemies = new List<Transform>();
    private int currentTargetIndex = 0;
    private Transform currentTarget;
    private float lastHapticTime;
    private float scanTimer;

    private void OnEnable()
    {
        TouchInputManager.Instance.OnTargetCyclePressed += CycleTarget;
    }

    private void OnDisable()
    {
        if (TouchInputManager.Instance != null)
            TouchInputManager.Instance.OnTargetCyclePressed -= CycleTarget;
    }

    private void Update()
    {
        // Periodically scan for on-screen enemies
        scanTimer += Time.deltaTime;
        if (scanTimer >= targetScanInterval)
        {
            scanTimer = 0f;
            RefreshOnScreenEnemies();
        }

        // Auto-select nearest if current target is lost
        if (currentTarget == null && onScreenEnemies.Count > 0)
        {
            currentTargetIndex = 0;
            currentTarget = onScreenEnemies[0];
        }

        // Fire all attached weapons at current target
        if (currentTarget != null && attachedWeapons.Count > 0)
        {
            foreach (var weapon in attachedWeapons)
            {
                weapon.AimAt(currentTarget.position);
                weapon.TryFire();
            }

            // Subtle haptic feedback for auto-fire (rate-limited)
            if (Time.realtimeSinceStartup - lastHapticTime >= hapticMinInterval)
            {
                HapticManager.Instance?.TriggerWeaponFireHaptic();
                lastHapticTime = Time.realtimeSinceStartup;
            }
        }
    }

    /// <summary>
    /// Called when the player taps the Target Cycle (B) button.
    /// Cycles to the next enemy on screen, sorted left-to-right.
    /// </summary>
    public void CycleTarget()
    {
        if (onScreenEnemies.Count == 0) return;

        currentTargetIndex = (currentTargetIndex + 1) % onScreenEnemies.Count;
        currentTarget = onScreenEnemies[currentTargetIndex];

        // Visual indicator: highlight new target (handled by TargetIndicatorUI)
        TargetIndicatorUI.Instance?.SetTarget(currentTarget);
    }

    public void AttachWeapon(WeaponAttachment weapon)
    {
        attachedWeapons.Add(weapon);
        HapticManager.Instance?.TriggerHaptic(
            HapticManager.HapticType.NotificationSuccess,
            HapticManager.HapticPriority.Medium);
    }

    public void DetachWeapon(WeaponAttachment weapon)
    {
        attachedWeapons.Remove(weapon);
    }

    private void RefreshOnScreenEnemies()
    {
        onScreenEnemies.Clear();
        Camera cam = Camera.main;
        Collider2D[] enemies = Physics2D.OverlapAreaAll(
            cam.ViewportToWorldPoint(Vector3.zero),
            cam.ViewportToWorldPoint(Vector3.one),
            enemyLayer);

        // Sort left-to-right by screen X position
        System.Array.Sort(enemies, (a, b) =>
            cam.WorldToScreenPoint(a.transform.position).x
            .CompareTo(cam.WorldToScreenPoint(b.transform.position).x));

        foreach (var enemy in enemies)
        {
            onScreenEnemies.Add(enemy.transform);
        }

        // Validate current target is still in list
        if (currentTarget != null && !onScreenEnemies.Contains(currentTarget))
        {
            currentTargetIndex = 0;
            currentTarget = onScreenEnemies.Count > 0 ? onScreenEnemies[0] : null;
        }
    }
}
```

### 7.4 Haptic Feedback Implementation

```csharp
// HapticManager.cs
// Centralized haptic feedback system.
// Handles cooldown, intensity scaling, auto-fire rate limiting,
// and user preferences.

using UnityEngine;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance { get; private set; }

    public enum HapticType
    {
        Light,
        Medium,
        Heavy,
        Rigid,
        Soft,
        NotificationSuccess,
        NotificationWarning,
        NotificationError,
        SelectionChanged
    }

    public enum HapticPriority
    {
        Low = 0,        // UI, target cycle, power-up
        Medium = 1,     // Jump, land, weapon pickup, enemy hit
        High = 2,       // Enemy defeat, destructible break, special attack
        Critical = 3    // Player damage, death, boss events, level complete
    }

    [SerializeField] private bool hapticsEnabled = true;
    [SerializeField] private bool weaponFireHapticsEnabled = true;
    [SerializeField] [Range(0f, 1f)] private float intensityMultiplier = 0.75f;
    [SerializeField] private float cooldownMs = 50f;
    [SerializeField] private float weaponFireCooldownMs = 250f;

    private float lastHapticTime;
    private float lastWeaponFireHapticTime;
    private HapticPriority lastPriority;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Trigger a standard haptic event.
    /// </summary>
    public void TriggerHaptic(HapticType type, HapticPriority priority = HapticPriority.Medium)
    {
        if (!hapticsEnabled) return;

        float elapsed = (Time.realtimeSinceStartup - lastHapticTime) * 1000f;
        if (elapsed < cooldownMs && priority <= lastPriority) return;

        HapticType scaledType = ScaleHapticType(type, intensityMultiplier);
        if (scaledType == HapticType.Light && intensityMultiplier < 0.25f) return;

        #if UNITY_IOS && !UNITY_EDITOR
        TriggerIOSHaptic(scaledType);
        #endif

        lastHapticTime = Time.realtimeSinceStartup;
        lastPriority = priority;
    }

    /// <summary>
    /// Trigger the subtle weapon auto-fire haptic on a separate cooldown track.
    /// Does not interfere with gameplay-critical haptics.
    /// </summary>
    public void TriggerWeaponFireHaptic()
    {
        if (!hapticsEnabled || !weaponFireHapticsEnabled) return;

        float elapsed = (Time.realtimeSinceStartup - lastWeaponFireHapticTime) * 1000f;
        if (elapsed < weaponFireCooldownMs) return;

        #if UNITY_IOS && !UNITY_EDITOR
        TriggerIOSHaptic(HapticType.Soft);  // always soft for weapon fire
        #endif

        lastWeaponFireHapticTime = Time.realtimeSinceStartup;
    }

    /// <summary>
    /// Trigger a multi-hit haptic pattern (e.g., death, boss events).
    /// </summary>
    public void TriggerHapticPattern(HapticType type, int count, float intervalSeconds)
    {
        if (!hapticsEnabled) return;
        StartCoroutine(HapticPatternCoroutine(type, count, intervalSeconds));
    }

    private System.Collections.IEnumerator HapticPatternCoroutine(
        HapticType type, int count, float interval)
    {
        for (int i = 0; i < count; i++)
        {
            #if UNITY_IOS && !UNITY_EDITOR
            TriggerIOSHaptic(ScaleHapticType(type, intensityMultiplier));
            #endif
            if (i < count - 1)
                yield return new WaitForSecondsRealtime(interval);
        }
        lastHapticTime = Time.realtimeSinceStartup;
        lastPriority = HapticPriority.Critical;
    }

    private HapticType ScaleHapticType(HapticType original, float intensity)
    {
        if (intensity >= 0.75f) return original;
        if (intensity >= 0.50f)
        {
            return original switch
            {
                HapticType.Heavy => HapticType.Medium,
                HapticType.Medium => HapticType.Light,
                HapticType.Rigid => HapticType.Light,
                _ => original
            };
        }
        return original switch
        {
            HapticType.Heavy => HapticType.Light,
            HapticType.Medium => HapticType.Soft,
            HapticType.Rigid => HapticType.Soft,
            HapticType.Light => HapticType.Soft,
            _ => original
        };
    }

    #if UNITY_IOS && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void _TriggerImpactHaptic(int style);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void _TriggerNotificationHaptic(int type);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void _TriggerSelectionHaptic();

    private void TriggerIOSHaptic(HapticType type)
    {
        switch (type)
        {
            case HapticType.Light:   _TriggerImpactHaptic(0); break;
            case HapticType.Medium:  _TriggerImpactHaptic(1); break;
            case HapticType.Heavy:   _TriggerImpactHaptic(2); break;
            case HapticType.Rigid:   _TriggerImpactHaptic(3); break;
            case HapticType.Soft:    _TriggerImpactHaptic(4); break;
            case HapticType.NotificationSuccess: _TriggerNotificationHaptic(0); break;
            case HapticType.NotificationWarning: _TriggerNotificationHaptic(1); break;
            case HapticType.NotificationError:   _TriggerNotificationHaptic(2); break;
            case HapticType.SelectionChanged:    _TriggerSelectionHaptic(); break;
        }
    }
    #endif
}
```

### 7.5 Native iOS Haptic Plugin

Create an Objective-C plugin file to bridge Unity to iOS haptic APIs:

```
// File: Assets/Plugins/iOS/HapticPlugin.mm

#import <UIKit/UIKit.h>

extern "C" {
    void _TriggerImpactHaptic(int style) {
        UIImpactFeedbackStyle feedbackStyle;
        switch (style) {
            case 0: feedbackStyle = UIImpactFeedbackStyleLight; break;
            case 1: feedbackStyle = UIImpactFeedbackStyleMedium; break;
            case 2: feedbackStyle = UIImpactFeedbackStyleHeavy; break;
            case 3: feedbackStyle = UIImpactFeedbackStyleRigid; break;
            case 4: feedbackStyle = UIImpactFeedbackStyleSoft; break;
            default: feedbackStyle = UIImpactFeedbackStyleMedium; break;
        }
        UIImpactFeedbackGenerator *generator =
            [[UIImpactFeedbackGenerator alloc] initWithStyle:feedbackStyle];
        [generator prepare];
        [generator impactOccurred];
    }

    void _TriggerNotificationHaptic(int type) {
        UINotificationFeedbackType feedbackType;
        switch (type) {
            case 0: feedbackType = UINotificationFeedbackTypeSuccess; break;
            case 1: feedbackType = UINotificationFeedbackTypeWarning; break;
            case 2: feedbackType = UINotificationFeedbackTypeError; break;
            default: feedbackType = UINotificationFeedbackTypeSuccess; break;
        }
        UINotificationFeedbackGenerator *generator =
            [[UINotificationFeedbackGenerator alloc] init];
        [generator prepare];
        [generator notificationOccurred:feedbackType];
    }

    void _TriggerSelectionHaptic() {
        UISelectionFeedbackGenerator *generator =
            [[UISelectionFeedbackGenerator alloc] init];
        [generator prepare];
        [generator selectionChanged];
    }
}
```

### 7.6 Control Customization Editor

```csharp
// ControlCustomizationEditor.cs
// Manages the drag-to-reposition UI for control layout editing.

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ControlCustomizationEditor : MonoBehaviour
{
    [Header("Draggable Controls")]
    [SerializeField] private List<DraggableControl> draggableControls;
    [SerializeField] private float snapGridSize = 8f;  // pt
    [SerializeField] private float minSpacing = 8f;     // pt between touch targets

    [Header("UI")]
    [SerializeField] private GameObject editModeOverlay;
    [SerializeField] private Button doneButton;
    [SerializeField] private Button resetButton;

    [System.Serializable]
    public class DraggableControl
    {
        public string id;                    // "dpad", "button_a", "button_b"
        public RectTransform rectTransform;
        public Vector2 defaultPosition;       // percentage of safe area
        public Vector2 minBounds;             // percentage (side constraint)
        public Vector2 maxBounds;             // percentage (side constraint)
    }

    public void EnterEditMode()
    {
        editModeOverlay.SetActive(true);
        foreach (var control in draggableControls)
        {
            // Add blue highlight border
            // Enable drag handling
            // Show position percentage label
        }
    }

    public void ExitEditMode()
    {
        editModeOverlay.SetActive(false);
        SavePositions();
    }

    public void SavePositions()
    {
        var data = new ControlPositionData
        {
            scheme = "dpad_ab",
            positions = new Dictionary<string, Vector2>(),
            opacity = (int)(TouchInputManager.Instance.controlOpacity * 100f),
            version = 2
        };

        foreach (var control in draggableControls)
        {
            Vector2 pct = RectToSafeAreaPercentage(control.rectTransform);
            data.positions[control.id] = pct;
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("control_positions_v2", json);
        PlayerPrefs.Save();
    }

    public void ResetToDefaults()
    {
        foreach (var control in draggableControls)
        {
            SetPositionFromPercentage(control.rectTransform, control.defaultPosition);
        }
        SavePositions();
    }

    private Vector2 SnapToGrid(Vector2 position)
    {
        return new Vector2(
            Mathf.Round(position.x / snapGridSize) * snapGridSize,
            Mathf.Round(position.y / snapGridSize) * snapGridSize
        );
    }

    private Vector2 RectToSafeAreaPercentage(RectTransform rect)
    {
        // Convert RectTransform position to percentage of safe area
        Rect safeArea = Screen.safeArea;
        Vector2 pos = rect.anchoredPosition;
        return new Vector2(
            (pos.x / safeArea.width) * 100f,
            (pos.y / safeArea.height) * 100f
        );
    }

    private void SetPositionFromPercentage(RectTransform rect, Vector2 percentage)
    {
        Rect safeArea = Screen.safeArea;
        rect.anchoredPosition = new Vector2(
            (percentage.x / 100f) * safeArea.width,
            (percentage.y / 100f) * safeArea.height
        );
    }
}
```

---

## 8. Auto-Fire Visual Feedback

### 8.1 Design Challenge

Because weapons fire automatically without player input, players may feel disconnected from the shooting action. Clear visual, audio, and haptic feedback must communicate that weapons are actively firing, what they are targeting, and when shots connect -- all without requiring the player to press anything.

### 8.2 Weapon Firing Indicators

| Feedback Type | Implementation | Purpose |
|---------------|---------------|---------|
| Muzzle flash | 2-frame pixel-art flash at weapon attachment point, synchronized to fire rate | Confirms weapon is actively firing |
| Projectile trail | 1-2px bright trail on each projectile in weapon's color | Shows projectile path from player to target |
| Weapon glow pulse | Weapon sprite brightens slightly (10-15% white overlay) on each shot | Draws eye to active weapon attachment |
| Fire rate rhythm | Muzzle flashes create a visual rhythm matching the weapon's fire rate | Gives each weapon a unique "feel" without button input |
| Ammo/energy bar | Small radial indicator around each weapon attachment (optional HUD element) | Shows weapon status for weapons with limited ammo |

### 8.3 Target Indicators

| Indicator | Visual | Behavior |
|-----------|--------|----------|
| Active target reticle | Thin animated bracket/crosshair around targeted enemy, in weapon's accent color | Follows targeted enemy; updates immediately on Target Cycle (B) press |
| Target switch flash | Brief white flash on new target when cycled | Confirms target change |
| Off-screen target arrow | Small directional arrow at screen edge pointing to targeted enemy if off-screen | Prevents confusion when target scrolls out of view |
| No-target indicator | Weapon attachment sprites dim to 60% opacity, muzzle flashes stop | Clearly shows weapons are idle when no enemies present |

### 8.4 Hit Confirmation Feedback

| Event | Visual Feedback | Audio Feedback |
|-------|----------------|----------------|
| Projectile hits enemy | White flash on enemy sprite (1 frame), small hit spark particle | Pitched "tink" sound, pitch varies slightly per hit for variety |
| Enemy health depleted | Enemy flash red 3 times rapidly, then defeat animation | Escalating tone on final hits |
| Destructible wall hit | Crack particles at impact point, debris chunks fly | Impact crunch sound, varies by material |
| Destructible wall breaks | Wall shatters into 4-8 pixel debris pieces, dust cloud | Satisfying shatter sound with bass thump |
| Shot misses / hits wall | Small dust puff at impact point | Muted "tik" sound |

### 8.5 Audio Feedback for Auto-Fire

Since the player does not press a fire button, audio cues are critical for communicating weapon activity:

| Audio Element | Description | Volume Relative to Music |
|---------------|-------------|--------------------------|
| Weapon fire sound | Each weapon type has a unique fire sound; plays on each shot | 60-70% (prominent but not overwhelming) |
| Fire rate layering | Multiple weapons create a layered soundscape; each weapon's sound is mixed to avoid mud | Each additional weapon at -3dB relative to first |
| Targeting "lock" chime | Subtle chime when weapons acquire a new auto-target | 40% |
| Target cycle "click" | Mechanical click sound on B button press | 50% |
| Weapon attach jingle | Short 16-bit jingle when new weapon attaches to player | 80% |
| Dry fire / idle hum | When weapons have no target, a quiet idle hum from weapon attachments | 20% (ambient) |

### 8.6 HUD Indicators

| HUD Element | Position | Description |
|-------------|----------|-------------|
| Weapon loadout display | Top-left corner, below safe area top | Row of small weapon icons showing all attached weapons. Active weapons pulse gently. |
| Target enemy health bar | Above targeted enemy sprite | Thin health bar appears above the currently targeted enemy |
| Power gauge | Bottom-center, above D-pad/buttons | Fills as player defeats enemies. When full, glows to indicate special attack is ready (hold A) |
| Kill counter | Top-center | Running count of enemies defeated in current level |

---

**Cross-References**:
- Screen layouts: See `ui_layout_spec.md` (Assessment 5.1)
- Accessibility controls: See `accessibility_spec.md` (Assessment 5.3)
- Ghost button tutorial labels: See `onboarding_flow.md` (Assessment 5.4)
- Pause on app background: See `interruption_handling_spec.md` (Assessment 5.5)

---

Last Updated: 2026-02-04
Status: Active
Version: 2.0
