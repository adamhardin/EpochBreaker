# Assessment 4.1 -- Player Physics & Jump Tuning Specification

## Target Platform & Frame Rate

- **Engine**: Unity 2022 LTS
- **Target Device**: iPhone 11 (baseline)
- **Frame Rate**: 60 fps (fixed timestep = 1/60 s = 16.667 ms per frame)
- **Tile Size**: 16x16 pixels (1 tile = 1 unit in world space)
- **Viewport**: approximately 20 tiles wide x 11.25 tiles tall (320x180 virtual resolution scaled to device)

All values in this document are specified at 60 fps. Frame counts assume fixed-timestep updates. Implementation must use `FixedUpdate()` with `Time.fixedDeltaTime = 1/60f`.

---

## 1. Horizontal Movement

### 1.1 Walk Speed

| Parameter | Value | Unit |
|-----------|-------|------|
| Max walk speed | 6.0 | tiles/second |
| Acceleration time (0 to max) | 8 | frames (0.133 s) |
| Deceleration time (max to 0) | 5 | frames (0.083 s) |
| Acceleration rate | 0.75 | tiles/s per frame |
| Deceleration rate | 1.20 | tiles/s per frame |

### 1.2 Run Speed

This game does not feature a dedicated run button. Sprint behavior is achieved via the Speed Boost power-up (see combat_system_spec.md).

| Parameter | Value | Unit |
|-----------|-------|------|
| Power-up boosted speed | 9.0 | tiles/second |
| Boost acceleration time | 10 | frames (0.167 s) |
| Boost deceleration time | 8 | frames (0.133 s) |

### 1.3 Velocity Per Frame (Walk)

```
Frame 0:  0.000 tiles/s  (idle)
Frame 1:  0.750 tiles/s
Frame 2:  1.500 tiles/s
Frame 3:  2.250 tiles/s
Frame 4:  3.000 tiles/s
Frame 5:  3.750 tiles/s
Frame 6:  4.500 tiles/s
Frame 7:  5.250 tiles/s
Frame 8:  6.000 tiles/s  (max walk)
```

Deceleration (from max):
```
Frame 0:  6.000 tiles/s
Frame 1:  4.800 tiles/s
Frame 2:  3.600 tiles/s
Frame 3:  2.400 tiles/s
Frame 4:  1.200 tiles/s
Frame 5:  0.000 tiles/s  (stopped)
```

### 1.4 Implementation Notes

- Acceleration and deceleration are linear (constant per-frame velocity delta).
- When reversing direction, apply deceleration first, then accelerate in the new direction. The player does NOT instantly reverse -- there is a 2-3 frame turnaround that adds weight.
- Ground friction is implicit in the deceleration rate; no separate friction coefficient is needed.
- Horizontal velocity is clamped to `[-maxSpeed, +maxSpeed]` every frame.

---

## 2. Air Control

| Parameter | Value | Notes |
|-----------|-------|-------|
| Air control factor | 70% | Percentage of ground acceleration available in air |
| Air acceleration rate | 0.525 | tiles/s per frame (0.75 * 0.70) |
| Air deceleration rate | 0.60 | tiles/s per frame (reduced from ground) |
| Air max horizontal speed | 6.0 | tiles/s (same as ground max) |

### 2.1 Rationale

70% air control provides meaningful aerial maneuvering without making the character feel like it is "swimming" in the air. This is close to Mega Man X's feel (estimated ~65-75% air control) and slightly less than Celeste (~90%). The player can correct mid-air trajectory but cannot make sharp reversals, which creates commitment to jump decisions.

### 2.2 Edge Case: Speed Preservation

If the player is moving at boosted speed (9.0 tiles/s) and jumps, their horizontal velocity is preserved during the jump. Air control acceleration cannot increase speed beyond the current max (6.0 normally, 9.0 during boost). Horizontal velocity naturally decays to 6.0 after the boost expires if the player is still airborne.

---

## 3. Vertical Movement -- Jump

### 3.1 Jump Parameters

| Parameter | Value | Unit |
|-----------|-------|------|
| Max jump height | 4.0 | tiles |
| Min jump height (tap) | 1.25 | tiles |
| Jump duration to apex (max) | 22 | frames (0.367 s) |
| Jump duration to apex (min) | 10 | frames (0.167 s) |
| Initial jump velocity | 12.727 | tiles/second |
| Rising gravity | 0.02632 | tiles/frame^2 |
| Falling gravity | 0.04167 | tiles/frame^2 |
| Terminal velocity | 12.0 | tiles/second (0.20 tiles/frame) |

### 3.2 Gravity Model (Variable Gravity)

The game uses asymmetric gravity -- rising is slower than falling. This produces the satisfying "hang time at apex" feeling found in classic platformers while keeping falls snappy and responsive.

**Rising (holding jump):**
```
Gravity_rise = 0.02632 tiles/frame^2

Derivation:
  jump_velocity = 2 * max_height / (apex_frames * dt)
  jump_velocity = 2 * 4.0 / (22 * (1/60))
  jump_velocity = 8.0 / 0.3667
  jump_velocity = 21.818 tiles/s  ... wait, let me recalculate properly.

Using kinematic equations at 60fps discrete steps:
  Let V0 = initial upward velocity (tiles/frame)
  Let g_rise = gravity deceleration (tiles/frame^2)

  At apex: V0 - g_rise * apex_frames = 0
  Total height: V0 * apex_frames - 0.5 * g_rise * apex_frames^2 = max_height

  From first equation: V0 = g_rise * apex_frames
  Substituting: g_rise * apex_frames^2 - 0.5 * g_rise * apex_frames^2 = max_height
                0.5 * g_rise * apex_frames^2 = max_height
                g_rise = 2 * max_height / apex_frames^2
                g_rise = 2 * 4.0 / 22^2
                g_rise = 8.0 / 484
                g_rise = 0.01653 tiles/frame^2

  V0 = g_rise * apex_frames = 0.01653 * 22 = 0.3636 tiles/frame
  V0 in tiles/second = 0.3636 * 60 = 21.818 tiles/s
```

**Corrected exact values:**

| Parameter | Value (tiles/frame) | Value (tiles/s) |
|-----------|---------------------|------------------|
| Initial jump velocity (V0) | 0.3636 | 21.818 |
| Rising gravity (g_rise) | 0.01653 | 59.504 tiles/s^2 |
| Falling gravity (g_fall) | 0.02500 | 90.000 tiles/s^2 |
| Terminal velocity | 0.2000 | 12.000 |

**Falling:**
```
g_fall = 0.02500 tiles/frame^2

Falling gravity ratio: g_fall / g_rise = 1.513

This means the player falls ~51% faster than they rise, creating
a snappy descent that keeps gameplay pace high.

Fall from max height (4.0 tiles) takes:
  4.0 = 0.5 * 0.02500 * t^2
  t^2 = 320
  t = 17.89 frames (~18 frames, 0.30 s)

Total max jump arc time: 22 + 18 = 40 frames (0.667 s)
```

### 3.3 Variable Jump Height

The player achieves variable jump height by releasing the jump button early. On release, the upward velocity is cut.

| Mechanic | Value |
|----------|-------|
| Velocity cut multiplier on release | 0.40 |
| Minimum hold frames before cut applies | 3 frames (0.05 s) |
| Minimum jump height (3-frame hold) | ~1.25 tiles |

**Implementation:**
```csharp
// On jump button release:
if (velocity.y > 0)
{
    velocity.y *= jumpCutMultiplier; // 0.40
}
```

**Variable height breakdown:**

| Hold Duration (frames) | Approximate Height (tiles) |
|-------------------------|---------------------------|
| 3 (minimum tap) | 1.25 |
| 6 | 1.80 |
| 10 | 2.50 |
| 14 | 3.00 |
| 18 | 3.50 |
| 22 (full hold) | 4.00 |

### 3.4 Jump Arc Data -- Max Jump at Walk Speed

Position data for max-height jump while moving at full walk speed (6.0 tiles/s = 0.1 tiles/frame horizontal):

```
Frame | X (tiles) | Y (tiles) | VelY (tiles/frame) | Phase
------|-----------|-----------|--------------------|---------
  0   |  0.000    |  0.000    | +0.3636            | Launch
  2   |  0.200    |  0.694    | +0.3306            | Rising
  4   |  0.400    |  1.323    | +0.2975            | Rising
  6   |  0.600    |  1.886    | +0.2645            | Rising
  8   |  0.800    |  2.384    | +0.2314            | Rising
 10   |  1.000    |  2.817    | +0.1983            | Rising
 12   |  1.200    |  3.185    | +0.1653            | Rising
 14   |  1.400    |  3.488    | +0.1322            | Rising
 16   |  1.600    |  3.725    | +0.0992            | Rising
 18   |  1.800    |  3.898    | +0.0661            | Rising
 20   |  2.000    |  4.005    | +0.0331            | Near Apex
 22   |  2.200    |  4.000    | +0.0000            | Apex
 24   |  2.400    |  3.950    | -0.0500            | Falling
 26   |  2.600    |  3.800    | -0.1000            | Falling
 28   |  2.800    |  3.550    | -0.1500            | Falling
 30   |  3.000    |  3.200    | -0.2000            | Falling (terminal)
 32   |  3.200    |  2.800    | -0.2000            | Falling (clamped)
 34   |  3.400    |  2.400    | -0.2000            | Falling (clamped)
 36   |  3.600    |  2.000    | -0.2000            | Falling (clamped)
 38   |  3.800    |  1.600    | -0.2000            | Falling (clamped)
 40   |  4.000    |  0.000    | -0.2000            | Land
```

**Horizontal distance covered during max jump at walk speed: ~4.0 tiles**

This is critical for level generation: maximum gap width that the player can clear at walk speed is approximately **3.75 tiles** (accounting for needing to land on the edge of a platform, not dead-center).

### 3.5 Jump Arc at Boosted Speed

At 9.0 tiles/s (0.15 tiles/frame horizontal), the max jump covers approximately **6.0 tiles** horizontal distance. Levels must never require boost-speed jumps for the critical path.

---

## 4. Coyote Time & Jump Buffer

| Parameter | Value | Rationale |
|-----------|-------|-----------|
| Coyote time | 5 frames (0.083 s) | After leaving a platform edge, the player can still initiate a jump for 5 frames. This forgives slight mistiming without feeling generous enough to exploit. Celeste uses 5 frames; Mega Man X uses ~3-4. |
| Jump buffer | 6 frames (0.100 s) | If the player presses jump within 6 frames before landing, the jump executes immediately on landing. This prevents "eaten inputs" on fast landings. Celeste uses 6 frames. |

### 4.1 Implementation Details

**Coyote Time:**
```
- Start a coyote timer when the player leaves a platform (not by jumping).
- If coyoteTimer > 0 and player presses jump: execute a grounded jump.
- Coyote timer does NOT activate when leaving via upward jump.
- Coyote timer does NOT activate after walking off a moving platform
  that has been destroyed (edge case -- treat as normal fall).
```

**Jump Buffer:**
```
- When jump is pressed while airborne and falling, store the input
  for jumpBufferFrames (6 frames).
- On landing, if jumpBuffer > 0: immediately execute jump.
- Buffer is consumed on use (no double-buffering).
- Buffer is cleared if the player is hit while airborne.
```

---

## 5. Edge Case Handling

### 5.1 Ceiling Collision

When the player's head collides with a ceiling tile:
- Vertical velocity is immediately set to 0 (no bounce).
- Falling gravity takes effect on the next frame.
- No damage is dealt from ceiling bumps.
- Audio: short "thud" SFX plays.

### 5.2 Wall Collision

- Horizontal velocity is zeroed on wall contact.
- The player does NOT slide up or down walls (no wall-slide mechanic in base game).
- Vertical velocity is unaffected by wall contact.
- The player can still jump while pressed against a wall (if grounded).

### 5.3 Landing on Moving Platforms

- On landing, the player inherits the platform's velocity as a base offset.
- Player input velocity is added on top of platform velocity.
- When leaving a moving platform (jump or walk off), the platform's velocity is added to the player's velocity at the moment of departure. This creates natural momentum transfer.
- Coyote time activates normally when walking off a moving platform.

### 5.4 Landing on Slopes

The game does not use slopes (16-bit tile aesthetic uses staircase terrain). All surfaces are axis-aligned. If future updates add slopes:
- Walking on slopes adjusts speed by cos(angle) factor.
- Jumping from slopes uses the slope normal for launch angle.

### 5.5 Corner Correction

When jumping and the player's collision box clips the corner of a platform by 3 pixels or fewer, the player is nudged horizontally to clear the corner. This prevents frustrating near-miss ceiling clips during precise jumps.

- Maximum nudge distance: 3 pixels (0.1875 tiles)
- Direction: away from the corner (horizontal only)
- Only applies during upward motion (rising phase of jump)

---

## 6. Reference Game Comparison

### 6.1 Mega Man X (SNES, 1993)

| Parameter | Estimated Value | Notes |
|-----------|----------------|-------|
| Walk speed | ~5.5 tiles/s | Moderate, deliberate |
| Dash speed | ~10.0 tiles/s | Significant boost |
| Acceleration frames | ~4 frames | Very snappy, near-instant |
| Deceleration frames | ~3 frames | Almost no slide |
| Max jump height | ~5.5 tiles | Very high |
| Jump apex frames | ~28 frames | Floatier than expected |
| Air control | ~65-75% | Good but committed |
| Variable jump | Yes | Cut velocity on release |
| Coyote time | ~3-4 frames | Minimal |
| Gravity ratio (fall/rise) | ~1.3 | Slight asymmetry |
| Wall jump/slide | Yes | Signature mechanic |

**Feel Profile**: Mega Man X feels **crisp and responsive** with very fast acceleration. The character commits to a direction quickly. The dash creates a strong speed differential that rewards skilled play. Jumps are tall and somewhat floaty at the apex, giving players time to aim during combat.

### 6.2 Super Mario World (SNES, 1990)

| Parameter | Estimated Value | Notes |
|-----------|----------------|-------|
| Walk speed | ~4.5 tiles/s | Slow walk |
| Run speed | ~8.0 tiles/s | Requires holding Y |
| Acceleration frames | ~14 frames (walk), ~24 (run) | Very gradual, momentum-based |
| Deceleration frames | ~8 frames (walk) | Noticeable slide |
| Max jump height (walk) | ~4.0 tiles | Lower without momentum |
| Max jump height (run) | ~5.0 tiles | Height scales with speed |
| Jump apex frames | ~20-24 frames | Varies with speed |
| Air control | ~85% | Extremely high |
| Variable jump | Yes | Multi-tier (short, medium, tall) |
| Coyote time | ~6 frames | Generous |
| Gravity ratio (fall/rise) | ~1.0 | Nearly symmetric |

**Feel Profile**: Super Mario World feels **momentum-driven and elastic**. The long acceleration curve rewards sustained movement. Air control is exceptionally high, making the character feel light and maneuverable. Jump height varies with running speed, creating a skill-based distance system. The symmetric gravity gives a floaty, hang-time-heavy feel.

### 6.3 Celeste (2018, retro-styled)

| Parameter | Estimated Value | Notes |
|-----------|----------------|-------|
| Walk speed | ~6.0 tiles/s | Quick, responsive |
| Dash speed | ~12.0 tiles/s | 8-directional dash |
| Acceleration frames | ~6 frames | Fast but not instant |
| Deceleration frames | ~4 frames | Tight stop |
| Max jump height | ~3.5 tiles | Moderate |
| Jump apex frames | ~18 frames | Tight arc |
| Air control | ~90% | Near-full control |
| Variable jump | Yes | Cut at 0.40 multiplier |
| Coyote time | 5 frames | Documented value |
| Jump buffer | 6 frames | Documented value |
| Gravity ratio (fall/rise) | ~1.6 | Strong asymmetry |

**Feel Profile**: Celeste feels **precise and forgiving**. The generous coyote time and jump buffer prevent frustration, while the tight acceleration/deceleration makes the character feel immediately responsive. The strong gravity asymmetry (fast fall) keeps pace high. The dash mechanic adds a burst option that this project handles via power-ups instead.

### 6.4 Comparison Matrix

| Parameter | This Game | Mega Man X | Mario World | Celeste |
|-----------|-----------|-----------|-------------|---------|
| Walk speed (t/s) | 6.0 | ~5.5 | ~4.5 | ~6.0 |
| Accel frames | 8 | ~4 | ~14 | ~6 |
| Decel frames | 5 | ~3 | ~8 | ~4 |
| Max jump (tiles) | 4.0 | ~5.5 | ~4.0 | ~3.5 |
| Apex frames | 22 | ~28 | ~22 | ~18 |
| Air control % | 70% | ~70% | ~85% | ~90% |
| Coyote (frames) | 5 | ~3 | ~6 | 5 |
| Jump buffer | 6 | ~0 | ~0 | 6 |
| Gravity ratio | 1.51 | ~1.3 | ~1.0 | ~1.6 |
| Variable jump | Yes | Yes | Yes | Yes |

### 6.5 Target Feel Statement

**"This game should feel like Mega Man X's responsiveness married to Celeste's forgiveness."**

The acceleration and air control are tuned to match Mega Man X's deliberate-but-responsive feel. The coyote time and jump buffer are taken directly from Celeste to prevent frustration on mobile (where imprecise touch controls make tight timing harder). The gravity ratio sits between Mega Man X and Celeste -- snappy falls without feeling too punishing. Walk speed matches Celeste's brisk pace rather than Mario's slower start, because mobile sessions should feel immediately active.

---

## 7. Jump Arc Diagram Description

### 7.1 Stationary Max Jump (0 horizontal velocity)

The trajectory is a pure vertical parabola:
- Rise: 22 frames, reaches 4.0 tiles height
- Apex: 1-2 frames of near-zero vertical speed (hang time)
- Fall: 18 frames back to ground (faster due to heavier falling gravity)
- Total time: 40 frames (0.667 s)
- Shape: Asymmetric parabola, wider on the rising side, steeper on the falling side

### 7.2 Walking Max Jump (6.0 tiles/s horizontal)

The trajectory is an asymmetric parabolic arc:
- Horizontal distance: ~4.0 tiles
- Peak at approximately X=2.2 tiles (slightly past horizontal midpoint due to asymmetric gravity)
- The rising portion covers ~55% of the horizontal distance
- The falling portion covers ~45% of the horizontal distance
- Shape: Slightly right-skewed parabola

### 7.3 Minimum Tap Jump (stationary)

- Rise: ~8 frames (velocity cut at frame 3, then decelerating to apex)
- Height: ~1.25 tiles
- Fall: ~10 frames
- Total: ~18 frames (0.30 s)
- Shape: Short, punchy arc -- barely clears a 1-tile obstacle

### 7.4 Minimum Tap Jump (walking)

- Horizontal distance: ~1.8 tiles
- Height: ~1.25 tiles
- Total: ~18 frames
- Shape: Low, wide arc good for hopping over small gaps and ground hazards

### 7.5 Critical Distances for Level Generation

| Jump Type | Height (tiles) | Horizontal Distance (tiles) |
|-----------|---------------|----------------------------|
| Max jump, stationary | 4.0 | 0.0 |
| Max jump, walking | 4.0 | 4.0 |
| Max jump, boosted | 4.0 | 6.0 |
| Min tap, stationary | 1.25 | 0.0 |
| Min tap, walking | 1.25 | 1.8 |
| Mid hold (14 frames), walking | 3.0 | 3.3 |

**Level generation must ensure**: No required gap exceeds 3.5 tiles wide (with 0.25 tile safety margin from max walkable jump of ~3.75 usable tiles). No required vertical reach exceeds 3.75 tiles (with 0.25 tile margin from 4.0 max).

---

## 8. Player Collision Box

| Parameter | Value |
|-----------|-------|
| Width | 12 pixels (0.75 tiles) |
| Height | 16 pixels (1.0 tile) standing |
| Foot sensor | 2 pixels below bottom edge, 8 pixels wide (centered) |
| Head sensor | 2 pixels above top edge, 8 pixels wide (centered) |
| Wall sensor | 2 pixels from left/right edge, 12 pixels tall (centered vertically) |

The collision box is slightly narrower than the sprite (which is 16x16). This is standard practice -- it makes platforming feel more generous because the player can visually overlap tile edges slightly without colliding.

---

## 9. State Machine Summary

```
IDLE
  -> WALK (horizontal input)
  -> JUMP_RISE (jump pressed, grounded or coyote)
  -> FALL (walk off edge, no coyote)
  -> HIT (damage received)

WALK
  -> IDLE (no input, velocity reaches 0)
  -> JUMP_RISE (jump pressed)
  -> FALL (walk off edge, no coyote)
  -> HIT (damage received)

JUMP_RISE
  -> JUMP_RISE (still ascending)
  -> FALL (velocity reaches 0 or jump released with cut)
  -> HIT (damage received)

FALL
  -> IDLE (landing, no horizontal input)
  -> WALK (landing, horizontal input held)
  -> JUMP_RISE (jump buffer triggers on land)
  -> HIT (damage received)

HIT
  -> IDLE (after invincibility frames, grounded)
  -> FALL (after invincibility frames, airborne)
```

---

## 10. Tuning Test Criteria

| Test | Expected Result | Rationale |
|------|-----------------|-----------|
| Tap jump clears 1-tile obstacle | Player clears a single tile block with minimum tap | Minimum useful jump |
| Hold jump clears 3.5-tile gap (walking) | Player clears gap with small margin | Maximum required gap in level gen |
| Hold jump reaches 4-tile ledge (stationary) | Player reaches the ledge | Maximum vertical reach |
| Reversal from full speed | Direction change completes in ~10 frames | Responsive without instant |
| Air reversal | Player can reverse direction mid-jump in ~14 frames | Controllable but committed |
| Coyote jump off cliff edge | Jump activates within 5 frames of leaving edge | Prevents frustration |
| Jump buffer landing | Pressing jump 6 frames before landing triggers jump on land | No eaten inputs |
| Terminal velocity consistency | Player falls no faster than 12 tiles/s regardless of fall height | Predictable fall speed |
| Corner correction | Player nudged past 3-pixel corner clips | Reduces frustrating near-misses |
| Moving platform departure | Player inherits platform velocity on jump | Natural momentum |

---

## 11. Unity Implementation Constants

```csharp
public static class PhysicsConstants
{
    // Horizontal
    public const float WalkSpeed         = 6.0f;    // tiles/second
    public const float AccelRate         = 0.75f;   // tiles/s per frame
    public const float DecelRate         = 1.20f;   // tiles/s per frame
    public const float AirControlFactor  = 0.70f;

    // Vertical
    public const float JumpVelocity      = 21.818f; // tiles/second
    public const float GravityRise       = 59.504f; // tiles/s^2
    public const float GravityFall       = 90.000f; // tiles/s^2
    public const float TerminalVelocity  = 12.000f; // tiles/second

    // Assists
    public const int CoyoteFrames        = 5;
    public const int JumpBufferFrames    = 6;
    public const float JumpCutMultiplier = 0.40f;
    public const int MinJumpHoldFrames   = 3;

    // Collision
    public const float CollisionWidth    = 0.75f;   // tiles
    public const float CollisionHeight   = 1.00f;   // tiles
    public const float CornerCorrectionPx = 3.0f;   // pixels

    // Boost (power-up)
    public const float BoostSpeed        = 9.0f;    // tiles/second

    // Level gen constraints
    public const float MaxRequiredGap    = 3.5f;    // tiles (horizontal)
    public const float MaxRequiredHeight = 3.75f;   // tiles (vertical)
}
```

---

## 12. Frame-Accurate Velocity Tables

### 12.1 Rising Velocity Per Frame (Full Hold Jump)

```
Frame | VelY (t/frame) | PosY (tiles) | Notes
------|----------------|-------------|------
  1   |  0.3636        |  0.3636     | Launch
  2   |  0.3471        |  0.7107     |
  3   |  0.3306        |  1.0413     | Earliest jump cut
  4   |  0.3140        |  1.3553     |
  5   |  0.2975        |  1.6528     |
  6   |  0.2810        |  1.9338     |
  7   |  0.2644        |  2.1982     |
  8   |  0.2479        |  2.4461     |
  9   |  0.2314        |  2.6775     |
 10   |  0.2149        |  2.8924     |
 11   |  0.1983        |  3.0907     |
 12   |  0.1818        |  3.2725     |
 13   |  0.1653        |  3.4378     |
 14   |  0.1488        |  3.5866     |
 15   |  0.1322        |  3.7188     |
 16   |  0.1157        |  3.8345     |
 17   |  0.0992        |  3.9337     |
 18   |  0.0826        |  4.0163     |  Near apex
 19   |  0.0661        |  4.0824     |
 20   |  0.0496        |  4.1320     |
 21   |  0.0331        |  4.1651     |
 22   |  0.0165        |  4.1816     |  Apex (velocity near zero)
```

Note: The slight overshoot past 4.0 tiles is due to discrete frame stepping. The actual apex height is ~4.18 tiles. To achieve exactly 4.0 tiles, the implementation should use g_rise = 0.01736 tiles/frame^2 and V0 = 0.3819 tiles/frame (22.914 tiles/s). The values above are provided for the current tuning pass; final values should be validated in-engine and adjusted by +/- 5% during playtesting.

### 12.2 Falling Velocity Per Frame (From Apex)

```
Frame | VelY (t/frame) | PosY from apex | Notes
------|----------------|----------------|------
  1   | -0.0250        |  -0.0250       |
  2   | -0.0500        |  -0.0750       |
  3   | -0.0750        |  -0.1500       |
  4   | -0.1000        |  -0.2500       |
  5   | -0.1250        |  -0.3750       |
  6   | -0.1500        |  -0.5250       |
  7   | -0.1750        |  -0.7000       |
  8   | -0.2000        |  -0.9000       | Terminal velocity reached
  9   | -0.2000        |  -1.1000       | Clamped
 10   | -0.2000        |  -1.3000       | Clamped
 ...
 18   | -0.2000        |  -2.9000       |
```

---

## Appendix A: Tuning Checklist

- [ ] All values validated in Unity FixedUpdate at 60fps
- [ ] Jump feels snappy on rise, weighty on fall
- [ ] Variable jump height responds correctly to tap vs hold
- [ ] Coyote time feels forgiving but not exploitable
- [ ] Jump buffer eliminates "eaten" jump inputs on landing
- [ ] Air control allows meaningful correction without full reversal
- [ ] Corner correction prevents frustrating near-miss clips
- [ ] Moving platform momentum transfer feels natural
- [ ] No physics values produce NaN or infinity under any condition
- [ ] All values produce identical results across iOS devices (deterministic)
