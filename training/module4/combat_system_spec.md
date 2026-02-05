# Assessment 4.4 -- Weapon Attachment & Auto-Fire System Specification

## Overview

This document specifies the weapon attachment system, auto-fire mechanics, weapon catalog (30 weapons across 10 eras), special attack system, and damage calculations for a retro side-scrolling mobile shooter (iOS). All timing values are in frames at 60 fps. The weapon system integrates with the procedural level generation pipeline for deterministic weapon drop placement and destructible terrain gating.

**Design Goal**: The player is a small character who collects weapon attachments that physically mount onto their body sprite. Weapons auto-fire continuously toward the nearest enemy. As the player accumulates attachments, their visual silhouette grows into a bristling arsenal of firepower. The player focuses on movement, positioning, and target cycling while their weapons handle the shooting. Progression is expressed through increasingly powerful and visually spectacular weapons across 10 eras of human civilization.

**Core Fantasy**: Start as a tiny figure throwing rocks. End as a walking singularity engine obliterating everything on screen.

**Tile Unit Reference:** 1 tile = 16x16 pixels. Player walk speed = 6.0 tiles/s. Player max jump height = 4.0 tiles. Screen width = 20 tiles. Screen height = 11.25 tiles.

---

## 1. Weapon Attachment System Overview

### 1.1 Attachment Mount Points

The player character has 6 defined mount points on their body sprite. Each mount point can hold one weapon attachment. Weapons visually render at their mount position and animate independently (idle bob, fire recoil, muzzle flash).

| Mount Point | Position (relative to sprite center) | Unlock Order | Visual Priority | Notes |
|-------------|--------------------------------------|--------------|-----------------|-------|
| Right Arm | +4px right, 0px vertical | Slot 1 (default) | Highest (front layer) | Primary weapon position, always visible |
| Left Arm | -4px left, 0px vertical | Slot 2 | High | Mirrors right arm, fires opposite offset |
| Right Shoulder | +3px right, -5px vertical | Slot 3 | Medium | Elevated position, fires over player head |
| Left Shoulder | -3px left, -5px vertical | Slot 4 | Medium | Mirrors right shoulder |
| Back | 0px horizontal, -6px vertical | Slot 5 | Low (behind sprite) | Tallest mount, projectiles arc upward before tracking |
| Head | 0px horizontal, -8px vertical | Slot 6 | Highest (top layer) | Crown position, most visually prominent |

### 1.2 Mount Point Sprite Integration

- Each weapon attachment is an 8x8 pixel sprite that draws at its mount offset relative to the player's current position.
- Weapons inherit the player's horizontal flip state (facing left/right).
- On fire, each weapon plays a 3-frame recoil animation: 1 frame kickback (2px away from fire direction), 1 frame hold, 1 frame return.
- Muzzle flash is a 2-frame overlay (4x4 pixels) at the weapon's barrel point.
- When the player jumps, all mounted weapons follow with a 1-frame visual delay (creates a satisfying "drag" effect as the arsenal trails the character).

### 1.3 Attachment Slots

| Parameter | Value | Notes |
|-----------|-------|-------|
| Starting slots | 1 | Right Arm only at game start |
| Maximum slots | 6 | All mount points filled |
| Slot unlock method | Era progression milestones | See Section 5 |
| Empty slot visual | Faint outline at mount point | Shows player where next weapon will attach |
| Attachment pickup range | 1.5 tile radius | Auto-magnetizes to player when within range |
| Attachment pickup animation | 6 frames (0.1 s) | Weapon flies to mount point with sparkle trail |

### 1.4 Attachment Behavior

- Weapons attach to the **first available slot** in unlock order (Right Arm, Left Arm, Right Shoulder, etc.).
- If all slots are full, a new pickup triggers the **Weapon Replace UI** (see Section 6).
- Attached weapons persist until the player dies, replaces them, or completes the level.
- On death, all attachments are lost. The player respawns with zero weapons and must re-collect.
- On level completion, all current weapons carry forward to the next level within the same era. Crossing an era boundary resets weapons to zero (thematic reset).

---

## 2. Auto-Fire Mechanics

### 2.1 Target Acquisition

The auto-fire system continuously selects and tracks the nearest valid target. The player never manually aims.

| Parameter | Value | Notes |
|-----------|-------|-------|
| Target acquisition range | 12 tiles (horizontal), 8 tiles (vertical) | Asymmetric: wider than tall to match scrolling |
| Target scan rate | Every 6 frames (10 Hz) | Balances accuracy with CPU cost |
| Target priority | Nearest enemy by Euclidean distance | Recalculates each scan |
| Target lock duration | Minimum 30 frames (0.5 s) | Prevents erratic switching between equidistant targets |
| No-target behavior | Weapons fire forward (facing direction) | Projectiles fly straight until off-screen |
| Destructible terrain targeting | Secondary priority | Only targeted when no enemies in range |
| Target lead compensation | None | Projectiles aim at target's current position, not predicted |

### 2.2 Target Cycling

The player can manually override auto-targeting with a dedicated cycle button.

| Parameter | Value | Notes |
|-----------|-------|-------|
| Cycle button | Bottom-left HUD, below jump | Thumb-reachable on mobile |
| Cycle direction | Next-nearest enemy (clockwise by angle) | Wraps around after furthest |
| Manual lock duration | 120 frames (2.0 s) | After 2 s without pressing cycle, reverts to auto-target |
| Visual indicator | Red reticle on current target | 8x8 animated crosshair sprite, pulses on lock |
| Cycle cooldown | 6 frames (0.1 s) | Prevents accidental double-tap |
| Audio cue | Quiet "click" SFX on cycle | Confirms input registered |

### 2.3 Auto-Fire Behavior

Weapons fire continuously without player input. The fire pattern depends on the number of equipped weapons.

| Weapon Count | Fire Pattern | Description |
|--------------|-------------|-------------|
| 1 | Single stream | Weapon fires at its base fire rate |
| 2 | Alternating | Weapons alternate shots (A-B-A-B), effective rate = 2x single |
| 3 | Staggered rotation | A-B-C-A-B-C, evenly spaced, effective rate = 3x single |
| 4 | Dual pairs | Pair 1 (arms) fires, then Pair 2 (shoulders) fires, 50% offset |
| 5 | Rolling salvo | Each weapon fires sequentially with equal frame gaps |
| 6 | Full barrage | All 6 weapons fire in a rolling cycle; at least 1 weapon fires every 2 frames |

**Stagger formula:**
```
fire_offset(slot_index) = floor((base_fire_interval / equipped_count) * slot_index)

Example: 3 weapons, base interval = 12 frames
  Slot 0 fires at frame 0, 12, 24, ...
  Slot 1 fires at frame 4, 16, 28, ...
  Slot 2 fires at frame 8, 20, 32, ...
```

This stagger ensures a constant stream of projectiles rather than synchronized volleys, which feels more satisfying and provides steadier DPS against moving targets.

### 2.4 Projectile General Properties

| Parameter | Value | Notes |
|-----------|-------|-------|
| Max projectiles on screen | 48 | Object pool, oldest recycled if exceeded |
| Projectile collision layer | Enemies, destructible terrain, boss hitboxes | Does NOT collide with other projectiles |
| Projectile lifespan | 180 frames (3.0 s) OR off-screen | Whichever comes first |
| Projectile spawn point | Weapon's muzzle position | Inherits weapon's facing direction |
| Aim interpolation | Instant snap to target angle | No turn rate; weapons instantly face target |
| Projectile visual size | 4x4 to 8x8 pixels | Varies by weapon tier |

---

## 3. Weapon Tiers

Each of the 10 eras contains 3 weapons, one at each tier. Tiers define rarity, power level, and visual complexity.

### 3.1 Tier Definitions

| Property | Tier 1: Basic | Tier 2: Enhanced | Tier 3: Powerful |
|----------|---------------|------------------|------------------|
| Rarity | Common | Uncommon | Rare |
| Drop rate | 60% of weapon drops | 30% of weapon drops | 10% of weapon drops |
| Base damage | 1-3 (scales by era) | 2-5 (scales by era) | 4-10 (scales by era) |
| Fire rate range | 2-4 shots/s | 2-5 shots/s | 1-6 shots/s |
| Projectile visual | Simple (single color, 4x4 px) | Detailed (two-tone, 6x6 px, trail) | Complex (animated, 8x8 px, particles) |
| Special property | None | 1 special property | 1-2 special properties |
| Muzzle flash | Basic (white spark) | Colored (matches projectile) | Elaborate (multi-frame, particles) |
| SFX | Simple 1-channel | Layered 2-channel | Rich 3-channel with reverb |
| Sprite colors | 2-3 | 4-5 | 6-8 with animation |
| Mount animation | Static with fire recoil | Idle bob + fire recoil | Idle glow + bob + recoil + particles |

### 3.2 Special Properties

Special properties are intrinsic to Tier 2 and Tier 3 weapons. They modify projectile behavior.

| Property | Effect | Visual Indicator |
|----------|--------|-----------------|
| Piercing | Projectile passes through first target, hitting up to 2 additional enemies | Projectile does not fade on hit; brief flash at each target struck |
| Splash | On hit, deals 50% damage to enemies within 1.5 tiles of impact | Circular burst animation (4 frames, 12x12 px) at impact point |
| Ricochet | On hit or wall contact, projectile bounces once at reflected angle | Projectile flashes brighter at bounce point, emits spark |
| Homing | Projectile curves toward target at 90 degrees/s turn rate | Faint trail line showing curved path |
| Burn (DoT) | Target takes 1 damage per second for 3 seconds after hit | Orange flame particles on affected enemy |
| Slow | Target movement speed reduced by 50% for 2 seconds | Blue tint on affected enemy, ice crystal particles |
| Chain | On hit, a secondary bolt jumps to 1 nearby enemy within 3 tiles | Visible arc connecting hit enemy to chain target |
| Gravity | Projectile creates a 1-tile pull field for 1 second, drawing enemies inward | Swirling dark vortex at impact point |
| Phase | Projectile passes through destructible terrain without destroying it, hitting enemies behind cover | Projectile appears semi-transparent, ghostly trail |
| Annihilate | Projectile destroys destructible terrain in a 2-tile radius on impact | Large explosion (8 frames, 16x16 px), screen shake (1px, 6 frames) |

---

## 4. Weapon Catalog by Era (30 Weapons)

All damage values are base damage per projectile hit. Fire rate is in shots per second at the weapon's native rate (before stagger adjustments). Projectile speed is in tiles per second. Mount preference indicates which slot the weapon gravitates toward if multiple slots are open (player can manually reassign).

### Era 1: Stone Age (Levels 1-5)

*Theme: Raw, primitive, organic. Muted earth tones. Rough-hewn sprites.*

| # | Name | Tier | Damage | Fire Rate | Proj. Speed | Projectile Visual | Special | Mount Pref. |
|---|------|------|--------|-----------|-------------|-------------------|---------|-------------|
| 1 | Thrown Rock | T1 | 1 | 2.0 | 8.0 | Grey 4x4 circle, tumbles (2-frame rotation) | None | Right Arm |
| 2 | Bone Dart | T2 | 2 | 3.0 | 10.0 | White 4x6 dart, slight wobble | Piercing (1 extra target) | Left Arm |
| 3 | Sling Stone | T3 | 4 | 2.5 | 14.0 | Brown 5x5 stone with speed lines, impact dust | Splash (1.0 tile radius, 50% dmg) | Right Shoulder |

**Era 1 Projectile Palette**: Brown (#8B6914), grey (#A0A0A0), bone white (#E8DCC8).

### Era 2: Bronze Age (Levels 6-10)

*Theme: Early metallurgy, warm golds and copper. Sharper sprites with metallic sheen.*

| # | Name | Tier | Damage | Fire Rate | Proj. Speed | Projectile Visual | Special | Mount Pref. |
|---|------|------|--------|-----------|-------------|-------------------|---------|-------------|
| 4 | Bronze Javelin | T1 | 2 | 2.0 | 9.0 | Bronze 4x8 spear, glints on rotation | None | Right Arm |
| 5 | Fire Pot | T2 | 3 | 1.5 | 7.0 | Orange 6x6 pot with flame trail (3 particles/frame) | Burn (1 dmg/s, 3 s) | Back |
| 6 | Bronze Arrow | T3 | 5 | 3.5 | 16.0 | Gold 4x6 arrow with bronze trail, flash on impact | Piercing (2 extra targets) | Right Shoulder |

**Era 2 Projectile Palette**: Bronze (#CD7F32), copper (#B87333), flame orange (#FF6600).

### Era 3: Classical Age (Levels 11-15)

*Theme: Greek/Roman engineering. White marble, red accents, geometric precision.*

| # | Name | Tier | Damage | Fire Rate | Proj. Speed | Projectile Visual | Special | Mount Pref. |
|---|------|------|--------|-----------|-------------|-------------------|---------|-------------|
| 7 | Pilum Spike | T1 | 2 | 2.5 | 10.0 | Grey 4x8 iron spike, slight arc trajectory | None | Right Arm |
| 8 | Greek Fire Flask | T2 | 3 | 1.5 | 7.5 | Green 6x6 flask, bursts into 3-tile fire on impact | Burn (2 dmg/s, 2 s) + Splash (1.5 tiles) | Left Arm |
| 9 | Ballista Bolt | T3 | 6 | 1.0 | 20.0 | Dark iron 6x10 bolt with speed lines, screen shake on hit (1px, 3 frames) | Piercing (all targets in line) + Annihilate (1 tile) | Back |

**Era 3 Projectile Palette**: Marble white (#F0EAD6), iron grey (#6B6B6B), crimson (#DC143C).

### Era 4: Medieval (Levels 16-20)

*Theme: Dark steel, fire, heavy impact. Gothic silhouettes.*

| # | Name | Tier | Damage | Fire Rate | Proj. Speed | Projectile Visual | Special | Mount Pref. |
|---|------|------|--------|-----------|-------------|-------------------|---------|-------------|
| 10 | Crossbow Bolt | T1 | 3 | 2.5 | 12.0 | Dark 4x6 bolt, fast and direct, small impact spark | None | Right Arm |
| 11 | Flaming Pitch | T2 | 4 | 1.5 | 8.0 | Orange-black 6x6 glob, fire trail (4 particles/frame), lingers 1 s at impact | Burn (2 dmg/s, 3 s) | Left Shoulder |
| 12 | Catapult Stone | T3 | 8 | 0.8 | 6.0 | Large grey 8x8 boulder, arc trajectory, massive impact dust cloud | Splash (2.0 tile radius, 75% dmg) + Annihilate (2 tiles) | Back |

**Era 4 Projectile Palette**: Dark steel (#434343), pitch black (#1A1A1A), flame (#FF4500).

### Era 5: Gunpowder (Levels 21-25)

*Theme: Smoke, flash, early firearms. Sepia tones with bright muzzle flashes.*

| # | Name | Tier | Damage | Fire Rate | Proj. Speed | Projectile Visual | Special | Mount Pref. |
|---|------|------|--------|-----------|-------------|-------------------|---------|-------------|
| 13 | Musket Ball | T1 | 3 | 2.0 | 14.0 | Grey 4x4 sphere, smoke puff at muzzle (6 frames) | None | Right Arm |
| 14 | Cannon Shot | T2 | 5 | 0.8 | 10.0 | Black 6x6 cannonball, heavy arc, large smoke trail | Splash (2.0 tiles, 60% dmg) | Back |
| 15 | Powder Keg | T3 | 9 | 0.5 | 5.0 | Red-brown 8x8 barrel, tumbles, massive explosion (16x16, 8 frames) | Splash (3.0 tiles, 100% dmg) + Annihilate (3 tiles) | Right Shoulder |

**Era 5 Projectile Palette**: Gunmetal (#828282), smoke (#C0C0C0), flash yellow (#FFD700).

### Era 6: Industrial (Levels 26-30)

*Theme: Steam, gears, brass. Mechanical precision, rapid fire.*

| # | Name | Tier | Damage | Fire Rate | Proj. Speed | Projectile Visual | Special | Mount Pref. |
|---|------|------|--------|-----------|-------------|-------------------|---------|-------------|
| 16 | Steam Rivet | T1 | 2 | 5.0 | 16.0 | Silver 3x3 rivet, steam puff trail, rapid clatter SFX | None | Right Arm |
| 17 | Dynamite Stick | T2 | 5 | 1.0 | 8.0 | Red 5x8 stick with sparking fuse, 6-frame fuse animation | Splash (2.5 tiles, 80% dmg) + Annihilate (2 tiles) | Left Arm |
| 18 | Gatling Round | T3 | 3 | 6.0 | 20.0 | Brass 3x3 bullet with golden trail, continuous muzzle flash | Piercing (1 extra target) | Right Shoulder |

**Era 6 Projectile Palette**: Brass (#B5A642), steam white (#E8E8E8), rivet silver (#C8C8C8).

### Era 7: Modern (Levels 31-35)

*Theme: Precise, military, high-velocity. Olive drab, steel blue, tracer effects.*

| # | Name | Tier | Damage | Fire Rate | Proj. Speed | Projectile Visual | Special | Mount Pref. |
|---|------|------|--------|-----------|-------------|-------------------|---------|-------------|
| 19 | Tracer Round | T1 | 3 | 4.0 | 22.0 | Red-orange 3x3 dot with fading tail (8px trail) | None | Right Arm |
| 20 | Rocket | T2 | 7 | 1.0 | 12.0 | Olive 5x8 cylinder with flame exhaust trail (6 particles/frame) | Splash (2.0 tiles, 70% dmg) + Homing (45 deg/s) | Back |
| 21 | Sniper Slug | T3 | 10 | 0.8 | 30.0 | White 3x10 streak, near-instant, screen-flash on fire | Piercing (all targets in line) | Head |

**Era 7 Projectile Palette**: Tracer red (#FF3300), olive (#556B2F), steel blue (#4682B4).

### Era 8: Digital (Levels 36-40)

*Theme: Neon, data streams, electric. Cyan, magenta, electric blue.*

| # | Name | Tier | Damage | Fire Rate | Proj. Speed | Projectile Visual | Special | Mount Pref. |
|---|------|------|--------|-----------|-------------|-------------------|---------|-------------|
| 22 | Laser Beam | T1 | 3 | 4.0 | 28.0 | Cyan 2x12 beam, no travel time visual (appears as line), flickers | None | Right Arm |
| 23 | EMP Pulse | T2 | 4 | 2.0 | 0.0 (instant AoE) | Expanding cyan ring (8 frames, 0 to 3 tiles radius) | Slow (50% speed, 3 s) + Chain (1 target) | Left Shoulder |
| 24 | Drone Strike | T3 | 8 | 1.5 | 10.0 | Small pixel drone sprite (6x4), flies to target, explodes on arrival | Homing (180 deg/s) + Splash (1.5 tiles, 60% dmg) | Head |

**Era 8 Projectile Palette**: Cyan (#00FFFF), magenta (#FF00FF), electric blue (#0080FF).

### Era 9: Space (Levels 41-45)

*Theme: Cosmic, alien, gravitational. Deep purples, plasma greens, void black.*

| # | Name | Tier | Damage | Fire Rate | Proj. Speed | Projectile Visual | Special | Mount Pref. |
|---|------|------|--------|-----------|-------------|-------------------|---------|-------------|
| 25 | Plasma Bolt | T1 | 4 | 3.5 | 18.0 | Green 5x5 glowing sphere, plasma trail (fading green dots) | None | Right Arm |
| 26 | Gravity Well | T2 | 5 | 1.0 | 8.0 | Purple 6x6 orb with swirling particles, distortion effect at impact | Gravity (1.5 tile pull, 1.5 s) + Slow (30%, 2 s) | Back |
| 27 | Ion Stream | T3 | 6 | 5.0 | 24.0 | Blue-white 3x8 bolt, continuous beam visual between shots, electric arcs | Chain (2 targets) + Piercing (1 extra target) | Right Shoulder |

**Era 9 Projectile Palette**: Plasma green (#39FF14), void purple (#4B0082), ion blue (#87CEEB).

### Era 10: Transcendent (Levels 46-50)

*Theme: Beyond physics. Reality-warping. Iridescent, shifting colors, particle-heavy.*

| # | Name | Tier | Damage | Fire Rate | Proj. Speed | Projectile Visual | Special | Mount Pref. |
|---|------|------|--------|-----------|-------------|-------------------|---------|-------------|
| 28 | Nano Swarm | T1 | 5 | 3.0 | 14.0 | Cloud of 8 tiny dots (2x2 each), swirls toward target, reforms on hit | Homing (120 deg/s) | Right Arm |
| 29 | Antimatter Orb | T2 | 8 | 1.0 | 6.0 | Black 7x7 sphere with iridescent corona (4-frame color cycle), screen distortion | Annihilate (3 tiles) + Splash (2.5 tiles, 100% dmg) | Back |
| 30 | Singularity Beam | T3 | 10 | 2.0 | Instant (hitscan) | White-to-black gradient beam, 2px wide, reality crack particles along path | Piercing (infinite) + Gravity (2 tile pull, 2 s) + Phase | Head |

**Era 10 Projectile Palette**: Iridescent (cycles #FF00FF, #00FFFF, #FFFF00 over 12 frames), void black (#000000), singularity white (#FFFFFF).

### 4.1 Weapon Catalog Summary Table

| # | Weapon | Era | Tier | Dmg | Rate | Speed | Special |
|---|--------|-----|------|-----|------|-------|---------|
| 1 | Thrown Rock | Stone Age | T1 | 1 | 2.0 | 8.0 | -- |
| 2 | Bone Dart | Stone Age | T2 | 2 | 3.0 | 10.0 | Piercing |
| 3 | Sling Stone | Stone Age | T3 | 4 | 2.5 | 14.0 | Splash |
| 4 | Bronze Javelin | Bronze Age | T1 | 2 | 2.0 | 9.0 | -- |
| 5 | Fire Pot | Bronze Age | T2 | 3 | 1.5 | 7.0 | Burn |
| 6 | Bronze Arrow | Bronze Age | T3 | 5 | 3.5 | 16.0 | Piercing |
| 7 | Pilum Spike | Classical | T1 | 2 | 2.5 | 10.0 | -- |
| 8 | Greek Fire Flask | Classical | T2 | 3 | 1.5 | 7.5 | Burn + Splash |
| 9 | Ballista Bolt | Classical | T3 | 6 | 1.0 | 20.0 | Piercing + Annihilate |
| 10 | Crossbow Bolt | Medieval | T1 | 3 | 2.5 | 12.0 | -- |
| 11 | Flaming Pitch | Medieval | T2 | 4 | 1.5 | 8.0 | Burn |
| 12 | Catapult Stone | Medieval | T3 | 8 | 0.8 | 6.0 | Splash + Annihilate |
| 13 | Musket Ball | Gunpowder | T1 | 3 | 2.0 | 14.0 | -- |
| 14 | Cannon Shot | Gunpowder | T2 | 5 | 0.8 | 10.0 | Splash |
| 15 | Powder Keg | Gunpowder | T3 | 9 | 0.5 | 5.0 | Splash + Annihilate |
| 16 | Steam Rivet | Industrial | T1 | 2 | 5.0 | 16.0 | -- |
| 17 | Dynamite Stick | Industrial | T2 | 5 | 1.0 | 8.0 | Splash + Annihilate |
| 18 | Gatling Round | Industrial | T3 | 3 | 6.0 | 20.0 | Piercing |
| 19 | Tracer Round | Modern | T1 | 3 | 4.0 | 22.0 | -- |
| 20 | Rocket | Modern | T2 | 7 | 1.0 | 12.0 | Splash + Homing |
| 21 | Sniper Slug | Modern | T3 | 10 | 0.8 | 30.0 | Piercing |
| 22 | Laser Beam | Digital | T1 | 3 | 4.0 | 28.0 | -- |
| 23 | EMP Pulse | Digital | T2 | 4 | 2.0 | Instant | Slow + Chain |
| 24 | Drone Strike | Digital | T3 | 8 | 1.5 | 10.0 | Homing + Splash |
| 25 | Plasma Bolt | Space | T1 | 4 | 3.5 | 18.0 | -- |
| 26 | Gravity Well | Space | T2 | 5 | 1.0 | 8.0 | Gravity + Slow |
| 27 | Ion Stream | Space | T3 | 6 | 5.0 | 24.0 | Chain + Piercing |
| 28 | Nano Swarm | Transcendent | T1 | 5 | 3.0 | 14.0 | Homing |
| 29 | Antimatter Orb | Transcendent | T2 | 8 | 1.0 | 6.0 | Annihilate + Splash |
| 30 | Singularity Beam | Transcendent | T3 | 10 | 2.0 | Instant | Piercing + Gravity + Phase |

### 4.2 DPS Reference Table (Single Weapon, No Modifiers)

| # | Weapon | Damage x Rate = DPS | Effective DPS (with special) |
|---|--------|---------------------|------------------------------|
| 1 | Thrown Rock | 1 x 2.0 = 2.0 | 2.0 |
| 2 | Bone Dart | 2 x 3.0 = 6.0 | ~9.0 (piercing bonus vs groups) |
| 3 | Sling Stone | 4 x 2.5 = 10.0 | ~13.0 (splash bonus vs groups) |
| 4 | Bronze Javelin | 2 x 2.0 = 4.0 | 4.0 |
| 5 | Fire Pot | 3 x 1.5 = 4.5 | 7.5 (burn adds 3 over 3 s) |
| 6 | Bronze Arrow | 5 x 3.5 = 17.5 | ~24.0 (piercing bonus) |
| 7 | Pilum Spike | 2 x 2.5 = 5.0 | 5.0 |
| 8 | Greek Fire Flask | 3 x 1.5 = 4.5 | ~10.0 (burn + splash) |
| 9 | Ballista Bolt | 6 x 1.0 = 6.0 | ~12.0 (piercing + terrain clear) |
| 10 | Crossbow Bolt | 3 x 2.5 = 7.5 | 7.5 |
| 11 | Flaming Pitch | 4 x 1.5 = 6.0 | 9.0 (burn) |
| 12 | Catapult Stone | 8 x 0.8 = 6.4 | ~14.0 (splash + terrain clear) |
| 13 | Musket Ball | 3 x 2.0 = 6.0 | 6.0 |
| 14 | Cannon Shot | 5 x 0.8 = 4.0 | ~7.0 (splash) |
| 15 | Powder Keg | 9 x 0.5 = 4.5 | ~12.0 (splash + terrain clear) |
| 16 | Steam Rivet | 2 x 5.0 = 10.0 | 10.0 |
| 17 | Dynamite Stick | 5 x 1.0 = 5.0 | ~11.0 (splash + terrain clear) |
| 18 | Gatling Round | 3 x 6.0 = 18.0 | ~22.0 (piercing) |
| 19 | Tracer Round | 3 x 4.0 = 12.0 | 12.0 |
| 20 | Rocket | 7 x 1.0 = 7.0 | ~12.0 (splash + homing) |
| 21 | Sniper Slug | 10 x 0.8 = 8.0 | ~16.0 (piercing all) |
| 22 | Laser Beam | 3 x 4.0 = 12.0 | 12.0 |
| 23 | EMP Pulse | 4 x 2.0 = 8.0 | ~12.0 (slow + chain) |
| 24 | Drone Strike | 8 x 1.5 = 12.0 | ~16.0 (homing + splash) |
| 25 | Plasma Bolt | 4 x 3.5 = 14.0 | 14.0 |
| 26 | Gravity Well | 5 x 1.0 = 5.0 | ~10.0 (gravity clusters enemies) |
| 27 | Ion Stream | 6 x 5.0 = 30.0 | ~42.0 (chain + piercing) |
| 28 | Nano Swarm | 5 x 3.0 = 15.0 | ~18.0 (homing, no misses) |
| 29 | Antimatter Orb | 8 x 1.0 = 8.0 | ~20.0 (annihilate + splash) |
| 30 | Singularity Beam | 10 x 2.0 = 20.0 | ~35.0 (piercing + gravity + phase) |

---

## 5. Attachment Slot Progression

Slots unlock at fixed era milestones. Each unlock is accompanied by a brief tutorial prompt showing the new mount point on the character sprite.

| Slot # | Mount Point | Unlocks At | Era | How Unlocked | Tutorial |
|--------|-------------|------------|-----|--------------|----------|
| 1 | Right Arm | Game start (Level 1) | Stone Age | Default | "Weapons you pick up mount here and fire automatically!" |
| 2 | Left Arm | Level 6 (Era 2 start) | Bronze Age | First level of era | "Second mount point unlocked! Dual weapons!" |
| 3 | Right Shoulder | Level 16 (Era 4 start) | Medieval | First level of era | "Shoulder mount online! Three weapons fire in rotation!" |
| 4 | Left Shoulder | Level 26 (Era 6 start) | Industrial | First level of era | "Full shoulders! Your firepower is escalating!" |
| 5 | Back | Level 36 (Era 8 start) | Digital | First level of era | "Back mount active! Five streams of destruction!" |
| 6 | Head | Level 46 (Era 10 start) | Transcendent | First level of era | "Crown mount achieved! Maximum arsenal!" |

### 5.1 Slot Unlock Pacing Rationale

```
Era 1 (Stone Age):     1 slot  -- Learn the core loop: move, auto-fire, progress
Era 2 (Bronze Age):    2 slots -- Experience staggered dual-fire, feel power growth
Era 3 (Classical):     2 slots -- Master dual weapons before adding more
Era 4 (Medieval):      3 slots -- Noticeable firepower jump, matches rising difficulty
Era 5 (Gunpowder):     3 slots -- Consolidate 3-weapon play
Era 6 (Industrial):    4 slots -- Industrial revolution = industrial firepower
Era 7 (Modern):        4 slots -- Master 4-weapon patterns
Era 8 (Digital):       5 slots -- Digital overload, screen fills with projectiles
Era 9 (Space):         5 slots -- Cosmic arsenal, nearly full
Era 10 (Transcendent): 6 slots -- Fully loaded, ultimate power fantasy
```

### 5.2 Slot Unlock Visual Sequence

1. Screen pauses briefly (15 frames / 0.25 s).
2. Camera zooms to player character (1.5x zoom over 30 frames).
3. New mount point glows with era-colored particles (60 frames / 1.0 s).
4. "NEW MOUNT POINT" text appears above character in era-themed font.
5. If a weapon pickup is nearby, it auto-magnetizes to the new slot as a demonstration.
6. Camera returns to normal (30 frames).
7. Total interruption: ~2.25 seconds.

---

## 6. Weapon Stacking Rules

### 6.1 Duplicate Weapons

If the player picks up a weapon identical (same name) to one already equipped:

| Scenario | Result | Visual |
|----------|--------|--------|
| Duplicate, open slot available | Equip in next open slot (two of the same weapon) | Both fire independently at staggered timing |
| Duplicate, all slots full | Weapon Replace UI triggers (see 6.3) | Swap prompt overlay |
| Triple or more of same weapon | Allowed up to max slots | All copies fire in staggered rotation |

**Duplicate bonus**: Having 2+ of the same weapon grants a **Resonance Bonus** -- +10% fire rate per duplicate beyond the first. This is capped at +30% (4 copies). The bonus incentivizes specialization as a valid strategy alongside diversification.

### 6.2 Weapon Mixing

Different weapons in different slots fire independently. There is no penalty for mixing eras or tiers. This encourages variety and experimentation.

| Mix Scenario | Behavior |
|-------------|----------|
| Different eras in different slots | Each fires its own projectile type at its own rate |
| Different tiers, same era | Higher tiers take visual priority in the silhouette |
| Slow + fast weapons mixed | Stagger algorithm distributes all weapons evenly across time |
| AoE + single-target mixed | AoE weapons target clusters, single-target weapons target the locked target |

### 6.3 Weapon Replace UI

When all slots are full and the player touches a new weapon pickup:

1. Game slows to 25% speed (does not fully pause -- maintains tension).
2. Current loadout displays in a horizontal strip at bottom of screen (6 weapon icons).
3. New weapon displays above with its stats.
4. Player taps the weapon they want to **replace**, or taps a "SKIP" button to ignore the pickup.
5. Decision window: 180 frames at 25% speed (effectively 3.0 real seconds).
6. If no input, pickup is skipped (game resumes full speed).
7. Replaced weapon drops behind the player as a pickup (persists for 300 frames / 5.0 s in case of regret).

**Frame data:**
| Phase | Frames (game time) | Real Time | Description |
|-------|-------------------|-----------|-------------|
| Slow-motion ramp | 12 | 0.2 s | Speed transitions from 100% to 25% |
| Decision window | 180 (at 25% speed) | 3.0 s | Player selects slot or skips |
| Swap animation | 18 | 0.3 s | Old weapon detaches, new weapon attaches |
| Speed ramp up | 12 | 0.2 s | Speed transitions from 25% to 100% |

---

## 7. Special Attack System

### 7.1 Activation

The special attack activates when the player **holds the jump button** while meeting the power threshold. This overloads the jump input with a powerful ability that rewards accumulating weapons.

| Parameter | Value | Notes |
|-----------|-------|-------|
| Activation input | Hold jump button for 30 frames (0.5 s) | Must be grounded; aerial hold does NOT trigger |
| Minimum attachments required | 3 | Prevents early-game access; aligns with Slot 3 unlock |
| Cooldown | 900 frames (15.0 s) | Begins after special attack animation completes |
| Cooldown reduction | -90 frames per attachment beyond 3 | 3 weapons: 15 s, 4: 13.5 s, 5: 12 s, 6: 10.5 s |
| Charge visual | Player sprite vibrates (1px oscillation), glow builds from feet upward | Provides hold-duration feedback |
| Charge audio | Rising tone, pitch increases over 30 frames | Clear audio tell for activation timing |
| Cooldown HUD indicator | Circular meter around special attack icon (bottom-right) | Fills clockwise as cooldown recovers |
| Ready indicator | Icon pulses with era-colored glow | Player always knows when special is available |

### 7.2 Special Attack Effects by Era

Each era has a unique special attack themed to its technology level. Damage scales with the number of equipped attachments.

**Base special damage formula:**
```
special_damage = base_era_damage * (1.0 + (attachment_count - 3) * 0.25)

With 3 weapons: 1.0x base
With 4 weapons: 1.25x base
With 5 weapons: 1.5x base
With 6 weapons: 1.75x base
```

| Era | Special Attack Name | Base Damage | Area | Duration (frames) | Visual Description |
|-----|-------------------|-------------|------|-------------------|-------------------|
| Stone Age | Avalanche | 6 | Full screen width, ground level (2 tiles high) | 60 (1.0 s) | Boulders cascade from top of screen, bouncing along ground |
| Bronze Age | Bronze Tempest | 10 | 360-degree ring, 4 tile radius from player | 48 (0.8 s) | Ring of bronze shards spirals outward from player |
| Classical | Phalanx Barrage | 12 | Cone, 8 tiles forward, 3 tiles wide at max | 54 (0.9 s) | Wall of spears launches forward in tight formation |
| Medieval | Rain of Fire | 15 | Full screen width, random impact points | 90 (1.5 s) | Flaming projectiles rain from sky, 8 impact points |
| Gunpowder | Grand Explosion | 18 | 5 tile radius circle centered on player | 36 (0.6 s) | Massive powder explosion, player is invincible during |
| Industrial | Steam Overload | 20 | Full screen, all enemies | 72 (1.2 s) | Steam vents erupt from ground at each enemy position |
| Modern | Airstrike | 24 | Full screen width, 3 tile high band at target Y | 60 (1.0 s) | Jet silhouette crosses screen, carpet bombs target line |
| Digital | System Crash | 28 | Full screen, all enemies and destructible terrain | 48 (0.8 s) | Screen glitches, all enemies flash and take damage, terrain pixelates and crumbles |
| Space | Graviton Collapse | 32 | 6 tile radius, pulls enemies to center before detonating | 90 (1.5 s) | Black hole appears at screen center, pulls all enemies in, then explodes outward |
| Transcendent | Reality Fracture | 40 | Full screen, all targets, ignores all defenses | 120 (2.0 s) | Screen shatters like glass, reality fragments deal damage to everything visible, then reforms |

### 7.3 Special Attack Frame Data

| Phase | Frames | Description |
|-------|--------|-------------|
| Charge (hold) | 30 | Player holds jump, charge visual builds |
| Startup | 12 | Player sprite enters special pose, invincibility begins |
| Active | Variable (see table above) | Damage hitboxes active, visual effect plays |
| Recovery | 18 | Player returns to normal, invincibility ends |
| **Invincibility window** | **Startup + Active + Recovery** | Player cannot take damage during entire special |

### 7.4 Special Attack vs Destructible Terrain

All special attacks destroy destructible terrain within their area of effect. This is critical for level progression -- some paths are blocked by destructible walls that can be cleared by either sustained auto-fire or a single special attack. The special attack provides a satisfying "blast through" moment.

| Terrain Type | HP | Auto-Fire Time to Destroy (single T1 weapon) | Special Attack |
|-------------|-----|-----------------------------------------------|---------------|
| Cracked Rock | 5 | ~2.5 s | Instant (any era special) |
| Reinforced Wall | 15 | ~7.5 s | Instant (any era special) |
| Armored Gate | 30 | ~15.0 s | 1-2 specials depending on era |
| Boss Barrier | 50 | ~25.0 s | 2-3 specials |

---

## 8. Damage Calculation

### 8.1 Damage Against Enemies

```
hit_damage = weapon_base_damage * tier_modifier * resonance_bonus

tier_modifier:
  T1: 1.0x
  T2: 1.0x (damage already baked into base stats)
  T3: 1.0x (damage already baked into base stats)

resonance_bonus:
  1 copy of weapon: 1.0x
  2 copies: 1.1x (fire rate bonus increases effective DPS)
  3 copies: 1.2x
  4+ copies: 1.3x (cap)

Special property damage is calculated separately:
  Splash: hit_damage * splash_percentage (applied to AoE targets)
  Burn:   burn_dps * burn_duration (applied as DoT, independent of hit)
  Chain:  hit_damage * 0.7 (chain target receives 70% of original hit)
```

### 8.2 Damage Against Destructible Terrain

Destructible terrain has HP but no damage resistance. All projectile damage applies 1:1.

```
terrain_damage = weapon_base_damage

Special interactions:
  Annihilate property: Destroys terrain in radius regardless of remaining HP
  Splash property:     Splash damage applies to adjacent terrain tiles
  Phase property:      Projectile passes THROUGH terrain (no terrain damage)
  Normal projectile:   Deals weapon_base_damage, consumed on hit (does not pierce terrain)
```

### 8.3 Destructible Terrain Types

| Terrain | HP | Visual | Drop on Destroy | Notes |
|---------|-----|--------|----------------|-------|
| Cracked Rock | 5 | Fissure lines, dust particles when hit | Coins (2-5) | Most common, blocks side paths |
| Clay Wall | 8 | Smooth brown surface, chips fly on hit | Nothing or weapon drop (15%) | Used as era-gated walls |
| Reinforced Wall | 15 | Metal bands over stone, sparks on hit | Guaranteed weapon drop | Major progression barriers |
| Armored Gate | 30 | Full metal door, dent marks accumulate | Guaranteed T2+ weapon drop | Pre-boss or pre-secret barriers |
| Boss Barrier | 50 | Glowing barrier with era symbol, cracks spread | Boss arena access | One per boss level, must destroy to proceed |
| Crystal Cluster | 12 | Glowing crystals, shatter into fragments | Health orb + coins | Hidden in walls, reward exploration |

### 8.4 Enemy Damage to Player

Enemy damage to the player follows the existing health and i-frame system. Weapon attachments do NOT provide any defensive benefit -- they are purely offensive.

| Parameter | Value | Notes |
|-----------|-------|-------|
| Player starting HP | 5 | Same as base system |
| Maximum HP | 8 | Achievable with HP Boost pickups |
| I-frame duration | 60 frames (1.0 s) | Standard after taking damage |
| Knockback | 1.5 tiles from damage source | 8-frame control lock |
| Hit stop | 2 frames | Both player and enemy freeze |
| Environmental damage cooldown | 30 frames | Spikes, lava, etc. |

### 8.5 Total Firepower Scaling (Full Loadout DPS Estimates)

Estimated DPS with all slots filled at era-appropriate weapons (mixed tiers, typical loadout):

| Era | Slots | Typical Loadout DPS | Enemy HP Range (era-appropriate) | Time to Kill (single enemy) |
|-----|-------|--------------------|---------------------------------|----------------------------|
| Stone Age | 1 | 2-10 | 2-5 | 0.5-2.5 s |
| Bronze Age | 2 | 8-22 | 4-8 | 0.4-1.0 s |
| Classical | 2 | 10-24 | 6-12 | 0.5-1.2 s |
| Medieval | 3 | 18-42 | 10-18 | 0.4-1.0 s |
| Gunpowder | 3 | 16-38 | 12-22 | 0.6-1.4 s |
| Industrial | 4 | 30-60 | 18-30 | 0.5-1.0 s |
| Modern | 4 | 36-68 | 22-36 | 0.5-1.0 s |
| Digital | 5 | 48-90 | 28-45 | 0.5-1.0 s |
| Space | 5 | 55-110 | 35-55 | 0.5-1.0 s |
| Transcendent | 6 | 70-150 | 45-70 | 0.5-1.0 s |

**Design target**: Time to kill a standard enemy should remain between 0.4 s and 1.4 s across all eras. This keeps combat feeling responsive regardless of power level. The numbers scale up together to preserve feel while delivering the power fantasy.

---

## 9. Weapon Drop System

### 9.1 Drop Sources

| Source | Drop Rate | Tier Distribution | Notes |
|--------|-----------|-------------------|-------|
| Standard enemy kill | 8% per kill | T1: 70%, T2: 25%, T3: 5% | Low individual rate, high volume |
| Elite enemy kill | 40% per kill | T1: 30%, T2: 50%, T3: 20% | Elites are larger, tougher variants |
| Boss defeat | 100% (guaranteed) | T1: 0%, T2: 30%, T3: 70% | Always drops high-tier weapon |
| Destructible terrain (Reinforced+) | 100% (guaranteed) | T1: 40%, T2: 45%, T3: 15% | Reward for clearing barriers |
| Destructible terrain (Cracked Rock) | 5% | T1: 90%, T2: 10%, T3: 0% | Rare bonus from common terrain |
| Hidden alcove | 100% (guaranteed, placed) | T1: 0%, T2: 60%, T3: 40% | Exploration reward |
| Weapon Shrine (special room) | 100% (choice of 3) | Player picks 1 of 3 random weapons | Presented as a selection screen |
| Era transition bonus | 100% (guaranteed) | Era-appropriate T2 weapon | Given free at start of new era |

### 9.2 Drop Mechanics

| Parameter | Value | Notes |
|-----------|-------|-------|
| Drop spawn position | 1 tile above defeated enemy or destroyed terrain | Pops upward with small bounce |
| Drop bounce height | 1.5 tiles | Single bounce, then settles |
| Drop settle time | 36 frames (0.6 s) | Cannot be collected during bounce |
| Drop magnetize range | 1.5 tiles from player center | Auto-collects when player is near |
| Drop persistence | 600 frames (10.0 s) | Flashes at 480 frames (8 s), disappears at 600 |
| Drop flash rate | 4 frames on, 4 frames off | Standard pickup warning |
| Drop visual | Rotating weapon sprite (90 deg/s) + era-colored glow aura | Immediately identifiable as weapon |
| Drop SFX (spawn) | Metallic chime, pitch varies by tier | Higher pitch = higher tier |
| Drop SFX (collect) | Satisfying "equip" clunk + era-themed accent | Confirms attachment |

### 9.3 Weapon Era Restriction

Weapons drop from the **current era** of the level being played. The player cannot find future-era weapons early. This preserves the thematic progression and prevents power spikes.

```
valid_weapon_era = current_level_era

Exception: Weapon Shrines may offer ONE weapon from the immediately
           previous era as a "legacy" option alongside two current-era weapons.
```

### 9.4 Guaranteed Drop Placement (Procedural Integration)

The level generator ensures minimum weapon availability per level:

| Level Length | Minimum Weapon Drops (guaranteed placements) | Placement Rule |
|-------------|----------------------------------------------|---------------|
| Short (128 tiles) | 2 | 1 in first quarter, 1 in third quarter |
| Standard (256 tiles) | 4 | 1 per quarter of level length |
| Long (384 tiles) | 6 | 1 per ~64-tile segment |
| Boss level | Standard + 1 from boss | Boss always drops a weapon |

### 9.5 Drop Placement Algorithm

```
for each level_segment in level.segments:
    // Determine if this segment has a guaranteed weapon drop
    if segment_index % guaranteed_interval == 0:
        // Place guaranteed drop
        location = find_safe_platform(segment.midpoint, rng)
        weapon = select_weapon(current_era, tier_weights, rng)
        place_weapon_drop(weapon, location)

    // Enemy drops are runtime (not pre-placed)
    // Destructible terrain drops are pre-seeded into terrain HP data

    // Hidden alcove weapons are placed during room generation
    if segment.has_hidden_alcove:
        weapon = select_weapon(current_era, alcove_tier_weights, rng)
        place_weapon_in_alcove(weapon, segment.alcove_position)
```

### 9.6 Drop Rate Scaling by Difficulty Tier

| Difficulty Tier | Enemy Drop Rate Modifier | Guaranteed Drop Modifier | Notes |
|-----------------|-------------------------|--------------------------|-------|
| Tier 1 (Easy) | 1.25x (10% effective) | +1 extra guaranteed | More generous for new players |
| Tier 2 (Normal) | 1.0x (8% effective) | Standard | Baseline |
| Tier 3 (Hard) | 0.8x (6.4% effective) | Standard | Fewer drops, more precious |
| Tier 4 (Extreme) | 0.6x (4.8% effective) | -1 guaranteed | Scarcity increases tension |
| Tier 5 (Master) | 0.5x (4% effective) | -1 guaranteed | Every weapon matters |

---

## 10. Integration with Generative Level System

### 10.1 Deterministic Weapon Placement

All weapon drop placements (guaranteed drops, terrain contents, alcove weapons, shrine offerings) are driven by the level's xorshift64* PRNG. Given the same seed, the same weapons appear at the same positions. The placement algorithm consumes RNG calls in a fixed order:

1. Level segment iteration (left to right).
2. Within each segment: guaranteed drop selection, terrain content seeding, alcove weapon selection.
3. Weapon Shrine offerings are generated when the shrine room is placed.
4. No external state influences placement (pure function of seed + era + difficulty).

### 10.2 Validation Rules

After placement, the generator validates:

1. **Weapon Access**: At least 1 guaranteed weapon drop within the first 32 tiles of any level.
2. **No Weapon Starvation**: No 64-tile stretch without at least 1 weapon drop opportunity (guaranteed or destructible terrain).
3. **Terrain Gating**: Destructible walls that block the critical path must be destroyable by T1 weapons within 10 seconds of sustained fire (HP <= 20 for critical path walls).
4. **Boss Preparation**: At least 1 weapon drop within 20 tiles before any boss arena entrance.
5. **Slot Awareness**: Guaranteed drops increase by +1 per newly unlocked slot on the level where the slot unlocks.
6. **Reachability**: Every placed weapon drop or weapon-containing terrain must be on a platform reachable from the critical path.

### 10.3 Weapon Budget per Level

```
total_weapon_opportunities = guaranteed_drops + terrain_drops + enemy_drops_expected

guaranteed_drops: see Section 9.4
terrain_drops: count(destructible_terrain_with_drops) in level
enemy_drops_expected: total_enemies * drop_rate * difficulty_modifier

Example: 256-tile Standard level, Era 5, Tier 2, ~40 enemies:
  Guaranteed:  4
  Terrain:     ~3 (reinforced walls + armored gates)
  Enemy:       40 * 0.08 * 1.0 = ~3.2
  Total:       ~10 weapon opportunities

Player will typically hold 3 weapons (era 5 has 3 slots).
Surplus weapons feed into the replace mechanic (Section 6.3).
```

---

## 11. Combat Feel Checklist

| Criterion | Target | Implementation |
|-----------|--------|----------------|
| Weapons feel automatic | Zero manual aim or fire input | Full auto-fire with smart targeting |
| Target cycling feels snappy | < 6 frames to switch | Instant reticle snap on button press |
| Weapon pickup feels exciting | Distinct visual + audio + mount animation | 6-frame attach, sparkle trail, era-themed SFX |
| Multiple weapons feel powerful | Screen fills with projectiles | Staggered fire, unique projectile visuals per weapon |
| Special attack feels epic | Screen-clearing spectacle | Full-screen VFX, screen shake, invincibility during |
| Destructible terrain feels satisfying | Visible chip damage + collapse | HP-based cracking, debris particles, screen shake on destroy |
| Power progression feels tangible | Each new era is visibly more powerful | Projectile size, particle density, SFX richness scale up |
| No input drops on cycle | Buffer system | Cycle input buffered within 6-frame window |
| DPS feels consistent | 0.4-1.4 s kill time across all eras | Damage and enemy HP scale in lockstep |

---

## Appendix A: Unity Implementation Constants

```csharp
public static class WeaponConstants
{
    // === Attachment Slots ===
    public const int StartingSlots                = 1;
    public const int MaxSlots                     = 6;
    public const float AttachmentPickupRange      = 1.5f;   // tiles
    public const int AttachmentPickupAnimFrames   = 6;      // frames

    // === Mount Point Offsets (pixels from sprite center) ===
    // Right Arm
    public const int MountRightArmX               = 4;
    public const int MountRightArmY               = 0;
    // Left Arm
    public const int MountLeftArmX                = -4;
    public const int MountLeftArmY                = 0;
    // Right Shoulder
    public const int MountRightShoulderX          = 3;
    public const int MountRightShoulderY          = -5;
    // Left Shoulder
    public const int MountLeftShoulderX           = -3;
    public const int MountLeftShoulderY           = -5;
    // Back
    public const int MountBackX                   = 0;
    public const int MountBackY                   = -6;
    // Head
    public const int MountHeadX                   = 0;
    public const int MountHeadY                   = -8;

    // === Slot Unlock Levels ===
    public const int Slot1UnlockLevel             = 1;      // Right Arm (default)
    public const int Slot2UnlockLevel             = 6;      // Left Arm (Bronze Age)
    public const int Slot3UnlockLevel             = 16;     // Right Shoulder (Medieval)
    public const int Slot4UnlockLevel             = 26;     // Left Shoulder (Industrial)
    public const int Slot5UnlockLevel             = 36;     // Back (Digital)
    public const int Slot6UnlockLevel             = 46;     // Head (Transcendent)

    // === Auto-Fire / Targeting ===
    public const float TargetRangeX               = 12.0f;  // tiles (horizontal)
    public const float TargetRangeY               = 8.0f;   // tiles (vertical)
    public const int TargetScanInterval           = 6;      // frames (10 Hz)
    public const int TargetLockMinFrames          = 30;     // frames (0.5 s)
    public const int ManualLockDuration           = 120;    // frames (2.0 s)
    public const int CycleCooldownFrames          = 6;      // frames (0.1 s)
    public const int MaxProjectilesOnScreen       = 48;
    public const int ProjectileLifespan           = 180;    // frames (3.0 s)

    // === Weapon Fire Animation ===
    public const int RecoilFrames                 = 3;      // total recoil cycle
    public const int RecoilPixelOffset            = 2;      // pixels kickback
    public const int MuzzleFlashFrames            = 2;
    public const int MuzzleFlashSize              = 4;      // pixels

    // === Weapon Replace UI ===
    public const float ReplaceSlowMotionScale     = 0.25f;  // 25% game speed
    public const int ReplaceDecisionWindow        = 180;    // frames (at 25% speed)
    public const int ReplaceSlowRampFrames        = 12;     // frames
    public const int ReplaceSwapAnimFrames        = 18;     // frames
    public const int ReplacedWeaponPersist        = 300;    // frames (5.0 s)

    // === Special Attack ===
    public const int SpecialHoldFrames            = 30;     // frames (0.5 s hold)
    public const int SpecialMinAttachments        = 3;
    public const int SpecialBaseCooldown          = 900;    // frames (15.0 s)
    public const int SpecialCooldownPerExtra      = 90;     // frames reduction per attachment > 3
    public const int SpecialStartupFrames         = 12;
    public const int SpecialRecoveryFrames        = 18;
    public const float SpecialDmgScalePerExtra    = 0.25f;  // +25% per attachment beyond 3

    // === Weapon Drop ===
    public const float EnemyDropRate              = 0.08f;  // 8% base
    public const float EliteDropRate              = 0.40f;  // 40%
    public const float CrackedRockDropRate        = 0.05f;  // 5%
    public const float DropBounceHeight           = 1.5f;   // tiles
    public const int DropSettleFrames             = 36;     // frames (0.6 s)
    public const float DropMagnetRange            = 1.5f;   // tiles
    public const int DropPersistFrames            = 600;    // frames (10.0 s)
    public const int DropFlashStartFrame          = 480;    // frames (8.0 s, begin flashing)
    public const int DropFlashCycle               = 8;      // frames (4 on, 4 off)

    // === Tier Drop Weights ===
    public const float StandardDropT1Weight       = 0.60f;
    public const float StandardDropT2Weight       = 0.30f;
    public const float StandardDropT3Weight       = 0.10f;
    public const float EliteDropT1Weight          = 0.30f;
    public const float EliteDropT2Weight          = 0.50f;
    public const float EliteDropT3Weight          = 0.20f;
    public const float BossDropT2Weight           = 0.30f;
    public const float BossDropT3Weight           = 0.70f;

    // === Difficulty Drop Rate Modifiers ===
    public const float DropRateTier1Mod           = 1.25f;  // Easy: more generous
    public const float DropRateTier2Mod           = 1.00f;  // Normal: baseline
    public const float DropRateTier3Mod           = 0.80f;  // Hard
    public const float DropRateTier4Mod           = 0.60f;  // Extreme
    public const float DropRateTier5Mod           = 0.50f;  // Master

    // === Destructible Terrain HP ===
    public const int CrackedRockHP                = 5;
    public const int ClayWallHP                   = 8;
    public const int CrystalClusterHP             = 12;
    public const int ReinforcedWallHP             = 15;
    public const int ArmoredGateHP                = 30;
    public const int BossBarrierHP                = 50;

    // === Resonance Bonus (Duplicate Weapons) ===
    public const float ResonanceBonus2            = 0.10f;  // +10% fire rate for 2 copies
    public const float ResonanceBonus3            = 0.20f;  // +20% for 3 copies
    public const float ResonanceBonusCap          = 0.30f;  // +30% for 4+ copies

    // === Special Property Values ===
    public const int PiercingExtraTargetsT2       = 1;
    public const int PiercingExtraTargetsT3       = 2;      // or infinite for specific weapons
    public const float SplashRadiusDefault        = 1.5f;   // tiles
    public const float SplashDamagePercent        = 0.50f;  // 50% of hit damage
    public const float HomingTurnRate             = 90.0f;  // degrees per second (default)
    public const int BurnDPS                      = 1;      // damage per second
    public const int BurnDuration                 = 180;    // frames (3.0 s)
    public const float SlowPercent                = 0.50f;  // 50% speed reduction
    public const int SlowDuration                 = 120;    // frames (2.0 s)
    public const float ChainDamagePercent         = 0.70f;  // 70% damage to chain target
    public const float ChainRange                 = 3.0f;   // tiles
    public const float GravityPullRadius          = 1.0f;   // tiles
    public const int GravityDuration              = 60;     // frames (1.0 s)
    public const float AnnihilateRadius           = 2.0f;   // tiles (terrain destruction)

    // === Player Health (carried from base system) ===
    public const int StartingHP                   = 5;
    public const int MaxHP                        = 8;
    public const int IFrameDuration               = 60;     // frames (1.0 s)
    public const int KnockbackLockFrames          = 8;
    public const float KnockbackSpeedX            = 6.0f;   // tiles/s
    public const float KnockbackSpeedY            = 4.0f;   // tiles/s
    public const int HitStopFrames                = 2;
    public const int EnvDamageCooldown            = 30;     // frames
}

// === Weapon Data Structure ===
[System.Serializable]
public struct WeaponData
{
    public int id;                    // 1-30
    public string name;
    public int era;                   // 1-10
    public int tier;                  // 1-3
    public int baseDamage;
    public float fireRate;            // shots per second
    public float projectileSpeed;     // tiles per second (0 = instant/hitscan)
    public int mountPreference;       // 0-5 (slot index)
    public SpecialProperty[] specials;
}

public enum SpecialProperty
{
    None,
    Piercing,
    Splash,
    Ricochet,
    Homing,
    Burn,
    Slow,
    Chain,
    Gravity,
    Phase,
    Annihilate
}

public enum MountPoint
{
    RightArm     = 0,
    LeftArm      = 1,
    RightShoulder = 2,
    LeftShoulder = 3,
    Back         = 4,
    Head         = 5
}
```

---

**Version**: 2.0
**Last Updated**: 2026-02-04
**Status**: Active (replaces v1.0 Melee Combat System)
**Assessment**: 4.4 - Weapon Attachment & Auto-Fire System
