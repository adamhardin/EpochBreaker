# Assessment 4.3 -- Camera System Specification

## Overview

This document specifies the complete camera system for the retro side-scrolling platformer. The camera is the player's window into the game world and directly impacts gameplay feel, readability, and comfort. The system must handle horizontal scrolling, vertical transitions, boss arena locks, and screen effects -- all at 60 fps on iPhone 11.

**Engine**: Unity 2022 LTS, using `Cinemachine` Virtual Camera or custom camera script on the main `Camera` component.

---

## 1. Viewport Configuration

| Parameter | Value | Notes |
|-----------|-------|-------|
| Virtual resolution | 320 x 180 pixels | 16:9 aspect ratio |
| Tile size | 16 x 16 pixels | |
| Viewport width | 20 tiles | 320 / 16 |
| Viewport height | 11.25 tiles | 180 / 16 |
| Camera orthographic size | 5.625 | Unity ortho size = half viewport height in units |
| Pixels per unit | 16 | 1 tile = 1 Unity unit |
| Pixel-perfect snapping | YES | Camera position rounded to nearest 1/16 unit (1 pixel) |

### 1.1 Aspect Ratio Handling

| Device Aspect | Behavior |
|---------------|----------|
| 16:9 (standard) | Full viewport, no letterboxing |
| 19.5:9 (iPhone X+) | Horizontal pillarboxing with safe-area insets |
| Other | Letterbox/pillarbox to maintain 16:9 gameplay area |

The camera viewport must NEVER reveal tiles beyond the level boundary. If the level is narrower than 20 tiles (rare), the camera centers the level and fills edges with the biome's background color.

---

## 2. Horizontal Tracking

### 2.1 Dead Zone

| Parameter | Value | Notes |
|-----------|-------|-------|
| Dead zone width | 2.0 tiles | Centered horizontally on screen |
| Dead zone horizontal position | Screen center, offset 0 | Player can move 1.0 tile left or right of center without camera movement |

**Behavior**: While the player's world-space X position is within the dead zone (relative to camera center), the camera does not move horizontally. This prevents jittery micro-corrections during small movements and gives a stable frame for combat.

### 2.2 Look-Ahead

| Parameter | Value | Notes |
|-----------|-------|-------|
| Look-ahead distance | 3.0 tiles | Camera leads in movement direction |
| Look-ahead activation speed | 2.0 tiles/s | Player must be moving at least this speed for look-ahead to engage |
| Look-ahead ramp time | 30 frames (0.5 s) | Time to reach full look-ahead offset |
| Look-ahead return time | 45 frames (0.75 s) | Time for look-ahead to return to center when player stops |

**Behavior**: When the player moves horizontally above the activation speed threshold, the camera gradually shifts ahead of the player in the movement direction. This reveals upcoming terrain and enemies before the player reaches them.

```
look_ahead_target = player_facing_direction * look_ahead_distance
                    * clamp01(player_speed / walk_speed)

current_look_ahead = lerp(current_look_ahead, look_ahead_target,
                          ramp_factor_per_frame)

ramp_factor_per_frame = 1.0 / ramp_frames = 1/30 = 0.0333 (per frame, linear interp)
return_factor_per_frame = 1.0 / return_frames = 1/45 = 0.0222 (per frame, linear interp)
```

**Direction Change**: When the player reverses direction, look-ahead does NOT snap. It smoothly transitions through center to the new direction over `ramp_time + return_time` worst case (75 frames / 1.25 s). This prevents jarring camera snaps on direction changes.

### 2.3 Speed Matching

| Parameter | Value | Notes |
|-----------|-------|-------|
| Camera base follow speed | Matches player speed | 1:1 tracking outside dead zone |
| Camera max speed | 12.0 tiles/s | Prevents camera from moving faster than this |
| Speed smoothing factor (lerp) | 0.12 per frame | Applied to camera velocity, not position |

**Behavior**: Outside the dead zone, the camera matches the player's horizontal speed with slight smoothing. The lerp factor of 0.12 per frame means the camera reaches ~95% of target speed within 24 frames (0.4 s). This prevents the camera from feeling "laggy" while avoiding harsh snapping.

```csharp
// Per FixedUpdate:
float targetCamX = player.x + currentLookAhead;
float deltaX = targetCamX - camera.x;

if (Mathf.Abs(deltaX) > deadZoneHalfWidth)
{
    float sign = Mathf.Sign(deltaX);
    float overshoot = Mathf.Abs(deltaX) - deadZoneHalfWidth;
    cameraVelocityX = Mathf.Lerp(cameraVelocityX,
                                  sign * overshoot * followStrength,
                                  speedSmoothFactor);
    camera.x += cameraVelocityX * Time.fixedDeltaTime;
}
```

---

## 3. Vertical Tracking

### 3.1 Ground Lock

| Parameter | Value | Notes |
|-----------|-------|-------|
| Ground lock Y offset | 3.5 tiles from bottom of viewport | Player stands at ~31% up from screen bottom when grounded |
| Ground lock snap speed | 0.15 per frame (lerp) | Camera snaps to ground level when player lands |
| Ground lock activation | On grounded for 3+ frames | Prevents snap during brief ground touches |

**Behavior**: When the player is grounded, the camera smoothly adjusts to position the player at a consistent vertical offset. This offset places the player in the lower third of the screen, revealing more of the sky/above for upcoming platforming challenges.

```
target_camera_y = player_ground_y + ground_lock_offset - (viewport_height * 0.31)
camera_y = lerp(camera_y, target_camera_y, ground_lock_snap_speed)
```

### 3.2 Vertical Dead Zone

| Parameter | Value | Notes |
|-----------|-------|-------|
| Vertical dead zone height | 2.5 tiles | Centered on ground lock position |
| Vertical dead zone top | 1.25 tiles above ground lock | |
| Vertical dead zone bottom | 1.25 tiles below ground lock | |

**Behavior**: Small jumps (up to ~2.5 tiles of vertical displacement from ground lock position) do not move the camera vertically. This is critical -- without this, every jump would cause the camera to bob up and down, which is disorienting on mobile.

The vertical dead zone means:
- Min tap jumps (1.25 tiles) do NOT move the camera at all
- Mid jumps (2.5 tiles) barely start to move the camera
- Max jumps (4.0 tiles) move the camera ~1.5 tiles upward

### 3.3 Upward Tracking

| Parameter | Value | Notes |
|-----------|-------|-------|
| Upward follow speed | 0.08 per frame (lerp) | Slower than horizontal for smooth feel |
| Upward tracking activation | Player exits top of vertical dead zone | |

**Behavior**: When the player jumps above the vertical dead zone, the camera gently tracks upward. The slow lerp (0.08) keeps the upward pan smooth. The camera does NOT try to center the player vertically -- it only ensures the player remains visible within the top 20% of the viewport.

### 3.4 Fall Look-Down

| Parameter | Value | Notes |
|-----------|-------|-------|
| Fall look-down activation | Player falling for 18+ frames (0.3 s) | Short hops don't trigger |
| Fall look-down distance | 2.0 tiles below normal position | Camera pans down to reveal landing zone |
| Fall look-down speed | 0.06 per frame (lerp) | Gradual pan, not jarring |
| Fall look-down return | On landing, returns to ground lock at 0.15/frame | Snaps back when safe |

**Behavior**: When the player is in a sustained fall (more than 18 frames), the camera gradually pans downward to reveal what the player is falling toward. This is essential for procedurally generated levels where the player cannot memorize layouts.

```
if (player.isFalling && fallFrames > 18)
{
    fallLookDownTarget = -2.0f; // tiles below normal
    currentFallOffset = Mathf.Lerp(currentFallOffset,
                                    fallLookDownTarget,
                                    0.06f);
}
else if (player.isGrounded)
{
    currentFallOffset = Mathf.Lerp(currentFallOffset, 0f, 0.15f);
}
```

---

## 4. Bounds Clamping

| Parameter | Value |
|-----------|-------|
| Clamp to level bounds | YES, always |
| Minimum camera X | 0 + viewport_width / 2 = 10.0 tiles |
| Maximum camera X | level_width - viewport_width / 2 |
| Minimum camera Y | 0 + viewport_height / 2 = 5.625 tiles |
| Maximum camera Y | level_height - viewport_height / 2 |

**Behavior**: The camera position is clamped AFTER all tracking calculations. The camera will never show tiles outside the level's tile map bounds. If the level is smaller than the viewport in either dimension, the camera centers on that axis and does not scroll.

**Implementation order per frame:**
1. Calculate horizontal target (dead zone + look-ahead + speed matching)
2. Calculate vertical target (ground lock + dead zone + fall look-down)
3. Apply smoothing/lerp
4. Clamp to level bounds
5. Apply pixel-perfect snapping (round to nearest 1/16 unit)
6. Apply screen shake offset (additive, after clamping)

---

## 5. Zone-Based Overrides

### 5.1 Boss Arena Lock

| Parameter | Value |
|-----------|-------|
| Trigger | Player enters boss arena zone (defined in level data) |
| Transition time | 45 frames (0.75 s) |
| Transition easing | Ease-in-out (cubic) |
| Lock behavior | Camera constrained to arena bounds |
| Arena bounds source | Boss arena layout (see boss_patterns.md) |

**Behavior**: When the player crosses the boss arena entrance trigger:
1. A gate closes behind the player (blocking retreat)
2. The camera smoothly pans to center on the arena over 45 frames
3. Camera tracking switches from player-following to arena-bounded
4. Horizontal and vertical dead zones still apply WITHIN the arena bounds
5. If the arena is smaller than the viewport, camera is fixed at arena center

```
Boss 1 arena: 20 x 12 tiles -> camera fixed horizontally (20 = viewport width),
              can pan vertically within 12-tile height
Boss 2 arena: 24 x 14 tiles -> slight horizontal tracking (4 extra tiles),
              slight vertical tracking (2.75 extra tiles)
Boss 3 arena: 28 x 16 tiles -> moderate tracking (8 extra horizontal,
              4.75 extra vertical)
```

### 5.2 Vertical Section Handling

Some procedurally generated sections feature vertical shafts (climbing or falling). When the level generator creates a vertical section:

| Parameter | Value |
|-----------|-------|
| Vertical section detection | Section height > 2x section width |
| Camera behavior | Switches to vertical-priority tracking |
| Horizontal dead zone | Expands to 4.0 tiles (less horizontal tracking) |
| Vertical dead zone | Shrinks to 1.0 tile (more responsive vertical tracking) |
| Vertical follow speed | Increases to 0.12/frame (from 0.08) |
| Transition | 30-frame smooth transition on section entry/exit |

### 5.3 Cinematic Moments

Reserved for specific events:

| Event | Camera Behavior | Duration |
|-------|-----------------|----------|
| Level start | Camera pans from left to player start position | 60 frames (1.0 s) |
| Boss intro | Camera zooms out to 1.2x, pans to reveal boss, zooms back | 90 frames (1.5 s) |
| Boss defeat | Camera holds on boss death animation, slight zoom in to 0.9x | 60 frames (1.0 s) |
| Level exit | Camera pans toward exit as player approaches | 45 frames (0.75 s) |

Cinematic cameras disable player input during the pan. The player is invincible during cinematic transitions.

---

## 6. Screen Shake

Screen shake is implemented as an additive offset applied to the final camera position AFTER bounds clamping. This means shake can briefly show outside level bounds -- this is acceptable because shake is brief and the pixel offset is small.

### 6.1 Shake Parameters

| Parameter | Value |
|-----------|-------|
| Shake algorithm | Perlin noise offset (2D) |
| Shake frequency | 30 Hz (every 2 frames at 60 fps) |
| Shake decay | Exponential: intensity * e^(-decay * t) |
| Shake X/Y ratio | 1.0 : 0.6 (more horizontal than vertical) |
| Max simultaneous shakes | 3 (stacked additively, capped at max intensity) |
| Max total intensity | 6.0 pixels (0.375 tiles) |

### 6.2 Shake Intensity Presets

| Event | Intensity (pixels) | Duration (frames) | Decay Rate | Notes |
|-------|--------------------|--------------------|------------|-------|
| Player takes damage | 2.0 | 12 (0.2 s) | 8.0 | Brief, small |
| Player lands from high fall | 1.5 | 8 (0.13 s) | 10.0 | Very brief, subtle |
| Enemy killed (melee) | 1.0 | 6 (0.1 s) | 12.0 | Micro-shake, impact feel |
| Boss stomp / hammer | 4.0 | 18 (0.3 s) | 6.0 | Significant, weighty |
| Boss phase transition | 5.0 | 30 (0.5 s) | 4.0 | Major event |
| Boss death | 6.0 | 45 (0.75 s) | 3.0 | Maximum shake, crescendo |
| Explosion (breakable wall) | 3.0 | 15 (0.25 s) | 7.0 | Moderate |
| Ground collapse | 3.5 | 24 (0.4 s) | 5.0 | Rumble feel |

### 6.3 Implementation

```csharp
public struct ShakeInstance
{
    public float intensity;     // peak pixels
    public float duration;      // total frames
    public float decayRate;     // exponential decay constant
    public float elapsed;       // frames elapsed
    public float xRatio;        // default 1.0
    public float yRatio;        // default 0.6
}

// Per frame:
Vector2 totalOffset = Vector2.zero;
foreach (var shake in activeShakes)
{
    float t = shake.elapsed / 60f; // convert to seconds
    float currentIntensity = shake.intensity * Mathf.Exp(-shake.decayRate * t);

    float noiseX = Mathf.PerlinNoise(shake.elapsed * 0.5f, 0f) * 2f - 1f;
    float noiseY = Mathf.PerlinNoise(0f, shake.elapsed * 0.5f) * 2f - 1f;

    totalOffset.x += noiseX * currentIntensity * shake.xRatio;
    totalOffset.y += noiseY * currentIntensity * shake.yRatio;

    shake.elapsed += 1f;
}

// Clamp total
totalOffset.x = Mathf.Clamp(totalOffset.x, -6f, 6f); // pixels
totalOffset.y = Mathf.Clamp(totalOffset.y, -6f, 6f);

camera.position += totalOffset / pixelsPerUnit; // convert to world units
```

### 6.4 Accessibility

Screen shake can be reduced or disabled in accessibility settings:
- **Full**: 100% intensity (default)
- **Reduced**: 30% intensity
- **Off**: 0% intensity (no shake)

---

## 7. Camera Smoothing Summary

All smoothing values collected in one place for tuning:

| Tracking Axis | Behavior | Lerp Factor (per frame) | Equivalent Time to 95% |
|---------------|----------|------------------------|------------------------|
| Horizontal follow | Outside dead zone | 0.12 | ~24 frames (0.40 s) |
| Look-ahead ramp | Direction change | 0.0333 | ~30 frames (0.50 s) |
| Look-ahead return | Stop moving | 0.0222 | ~45 frames (0.75 s) |
| Vertical ground lock | Landing | 0.15 | ~18 frames (0.30 s) |
| Vertical upward | Jumping above dead zone | 0.08 | ~36 frames (0.60 s) |
| Fall look-down | Extended fall | 0.06 | ~48 frames (0.80 s) |
| Fall return | Landing after fall | 0.15 | ~18 frames (0.30 s) |
| Boss arena transition | Arena entry | Cubic ease | 45 frames (fixed) |
| Vertical section switch | Section change | Linear | 30 frames (fixed) |

**Note on lerp vs spring**: This spec uses exponential lerp (`lerp(current, target, factor)`) rather than spring-damper systems. Lerp is simpler to implement, tune, and profile. If more organic motion is desired during boss encounters, a spring-damper can be substituted with:
- Spring constant: 40.0
- Damping ratio: 0.7 (slightly underdamped for minor overshoot)

---

## 8. Unity Implementation Constants

```csharp
public static class CameraConstants
{
    // Viewport
    public const float VirtualWidth        = 320f;  // pixels
    public const float VirtualHeight       = 180f;  // pixels
    public const float OrthoSize           = 5.625f; // Unity ortho camera size
    public const int   PixelsPerUnit       = 16;

    // Horizontal
    public const float DeadZoneWidth       = 2.0f;  // tiles total (1.0 each side)
    public const float LookAheadDistance   = 3.0f;  // tiles
    public const float LookAheadMinSpeed   = 2.0f;  // tiles/s activation threshold
    public const float LookAheadRampLerp   = 0.0333f; // per frame
    public const float LookAheadReturnLerp = 0.0222f; // per frame
    public const float HorizontalFollowLerp = 0.12f;  // per frame
    public const float CameraMaxSpeed      = 12.0f;   // tiles/s

    // Vertical
    public const float GroundLockOffset    = 3.5f;   // tiles from viewport bottom
    public const float GroundLockLerp      = 0.15f;  // per frame
    public const float VerticalDeadZone    = 2.5f;   // tiles total
    public const float UpwardFollowLerp    = 0.08f;  // per frame
    public const float FallLookDownDist    = 2.0f;   // tiles
    public const float FallLookDownLerp    = 0.06f;  // per frame
    public const float FallLookDownDelay   = 18;     // frames before activation
    public const float FallReturnLerp      = 0.15f;  // per frame

    // Screen Shake
    public const float ShakeMaxIntensity   = 6.0f;   // pixels
    public const float ShakeXYRatio        = 0.6f;   // Y = X * ratio
    public const int   MaxSimultaneousShakes = 3;

    // Boss Arena
    public const float BossTransitionFrames = 45f;

    // Vertical Section Override
    public const float VSectionHDeadZone   = 4.0f;   // tiles
    public const float VSectionVDeadZone   = 1.0f;   // tiles
    public const float VSectionVFollowLerp = 0.12f;  // per frame
    public const float VSectionTransFrames = 30f;    // frames
}
```

---

## 9. Edge Cases & Special Handling

### 9.1 Level Start

On level load, the camera is instantly positioned at the player's start position (no smoothing on first frame). The cinematic pan occurs BEFORE the player gains control.

### 9.2 Player Death and Respawn

On death:
1. Camera freezes for 12 frames (death pause)
2. Screen fades to black over 18 frames
3. Camera teleports to checkpoint position (no smooth transition -- black screen covers it)
4. Screen fades in over 18 frames
5. Player regains control

### 9.3 Very Fast Movement

If the player exceeds `CameraMaxSpeed` (e.g., during knockback or dash power-up), the camera caps its follow speed at 12.0 tiles/s. The player may briefly exit the viewport -- the camera catches up when the player decelerates. This is acceptable because ultra-fast movement is always brief (< 30 frames).

### 9.4 Teleportation / Warp Zones

If the player teleports (via switch or warp tile):
1. Camera performs a 15-frame fade-to-black
2. Camera position jumps to new location
3. 15-frame fade-from-black
4. Normal tracking resumes

### 9.5 Multiple Targets

This game is single-player. Multi-target camera logic is not needed.

---

## 10. Performance Considerations

| Metric | Budget |
|--------|--------|
| Camera update CPU time | < 0.1 ms per frame |
| Screen shake calculation | < 0.02 ms per frame |
| Bounds clamping | < 0.01 ms per frame |
| Total camera system | < 0.15 ms per frame |

The camera system is lightweight. All calculations are scalar math (no physics raycasts, no collision queries). The system runs in `LateUpdate()` to ensure it reads final player position after physics and gameplay updates.

---

## Appendix A: Reference Analysis

### Mega Man X Camera
- Horizontal-only scrolling in most sections
- No vertical dead zone (camera tracks jumps immediately)
- Look-ahead tied to facing direction, not movement
- Boss arenas are exactly viewport-sized (no scrolling during boss fights)
- **Takeaway**: Boss arena lock at viewport size works for simple bosses but limits arena design

### Super Mario World Camera
- Dual-rail horizontal system (camera switches rails based on player height)
- Very large dead zone horizontally
- Vertical tracking is slow and laggy
- No look-ahead
- **Takeaway**: The rail system is elegant but overly complex for procedural levels where terrain height varies unpredictably

### Celeste Camera
- Tight horizontal following with minimal dead zone
- Room-based vertical transitions (snap between rooms)
- No look-ahead (rooms are pre-designed, no discovery needed)
- Screen shake on dash, death, impact -- very polished
- **Takeaway**: Room-based snap transitions don't work for continuous procedural levels, but the screen shake polish and tight following are excellent references

### Hollow Knight Camera (additional reference)
- Smooth lerp following in both axes
- Subtle look-ahead in movement direction
- Vertical look-down when falling (exactly what we need)
- Boss arena locks that still allow some camera movement within the arena
- **Takeaway**: Closest to our needs. The arena lock with internal movement is the model for our boss encounters.

**Chosen Approach**: Hollow Knight-style smooth tracking with Celeste-quality screen shake, adapted for procedural levels with Mega Man X-style look-ahead.
