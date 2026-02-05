# Era-Based Enemy Archetype Specifications

## Overview

This document defines 5 mechanically distinct enemy archetypes for the retro side-scrolling mobile shooter (iOS). Each archetype is re-skinned and stat-scaled across 10 eras of human civilization progression. The player character uses auto-firing weapon attachments to blast through enemies alongside destructible terrain.

**Core Design Principle:** Every archetype defines a single behavioral pattern (state machine) that remains constant across all 10 eras. Only visuals, stat values, and difficulty weights change per era. This allows designers to build encounters using 5 known behaviors while artists produce era-appropriate sprites independently.

**Design Rules:**

- Every enemy has a readable "tell" before attacking (minimum 12 frames / 0.2 s visual warning at 60 fps).
- Every enemy can be defeated by the player's base weapon attachment (no mandatory upgrades for progression).
- Enemy placement respects destructible terrain geometry -- enemies do not spawn inside breakable segments.
- Enemy density per screen is governed by the era difficulty budget (see Section 7).
- Auto-fire weapons mean the player focuses on positioning and dodging; enemy design must reward spatial play.

**Reference Values:**

| Parameter | Value |
|-----------|-------|
| Tile unit | 16x16 pixels |
| Game frame rate | 60 fps |
| Player walk speed | 7.0 tiles/s |
| Player max run speed | 10.0 tiles/s |
| Screen width | ~20 tiles (320px) |
| Screen height | ~12 tiles (192px) |
| Auto-fire range | 12 tiles horizontal |
| Auto-fire rate (base) | 4 rounds/s |

---

## The 10 Eras

Eras define the visual theme, destructible terrain materials, and enemy re-skins for each stage of the game. Difficulty scales with era progression.

| Era # | Name | Period | Destructible Terrain | Difficulty Tier |
|--------|------|--------|---------------------|-----------------|
| 1 | Stone Age | Prehistory | Rocks, bones, brush | Tier 1 |
| 2 | Bronze Age | ~3300-1200 BCE | Mud brick, clay walls | Tier 1 |
| 3 | Iron Age | ~1200-500 BCE | Timber, iron gates | Tier 2 |
| 4 | Medieval | ~500-1500 CE | Stone walls, wooden barricades | Tier 2 |
| 5 | Renaissance | ~1400-1700 CE | Marble, plaster, glass | Tier 3 |
| 6 | Industrial | ~1760-1840 CE | Brick, steel beams, pipes | Tier 3 |
| 7 | Modern | ~1900-2000 CE | Concrete, rebar, vehicles | Tier 4 |
| 8 | Digital | ~2000-2050 CE | Circuit boards, server racks, glass panels | Tier 4 |
| 9 | Space | ~2050-2200 CE | Hull plating, airlocks, energy shields | Tier 5 |
| 10 | Transcendent | ~2200+ CE | Crystalline structures, void matter, light constructs | Tier 5 |

**Difficulty Tiers** group eras for stat scaling:

| Tier | Eras | HP Multiplier | Damage Multiplier | Speed Multiplier |
|------|------|---------------|-------------------|------------------|
| Tier 1 | Stone, Bronze | 1.0x | 1.0x | 1.0x |
| Tier 2 | Iron, Medieval | 1.3x | 1.2x | 1.1x |
| Tier 3 | Renaissance, Industrial | 1.6x | 1.4x | 1.2x |
| Tier 4 | Modern, Digital | 2.0x | 1.7x | 1.3x |
| Tier 5 | Space, Transcendent | 2.5x | 2.0x | 1.4x |

---

## Archetype 1: Charger (Melee)

### Concept

A ground-based enemy that rushes at the player on detection. Simple, aggressive, and punishable during recovery. The Charger is the most common enemy type and teaches players to position vertically (jump/duck) to avoid linear horizontal threats.

### Era Variants

| Era | Variant Name | Sprite Description |
|-----|-------------|-------------------|
| Stone Age | Wild Boar | Brown bristled quadruped, tusks forward, dust cloud on charge |
| Bronze Age | Bronze Soldier | Humanoid with bronze shield and short sword, sandaled feet |
| Iron Age | War Hound | Armored canine with iron collar and spiked barding |
| Medieval | Armored Knight | Full plate armor, visor down, sword raised during charge |
| Renaissance | Pikeman | Soldier with breastplate and forward-thrust pike |
| Industrial | Steambot | Brass automaton on treads, steam jets on charge, piston arms |
| Modern | Riot Trooper | Armored soldier with ballistic shield, sprinting stance |
| Digital | Combat Drone | Wheeled robot, low profile, extending ram plate |
| Space | Assault Mech | Bipedal mini-mech with boosted charge jets |
| Transcendent | Void Beast | Amorphous dark creature, red-eyed, tentacle lash on contact |

### Sprite Dimensions

- Body: 16x16 pixels (standard tile)
- Charge effect: 8x8 trailing particle (dust/sparks/energy per era)
- Collision box: 14x14 pixels (2px inset on each side for visual forgiveness)

### State Machine (60 fps)

```
PATROL
  Description: Walks back and forth along ground surface.
  Speed: 2.5 tiles/s
  Range: Full width of current ground segment minus 1 tile buffer on each end.
  Reverses direction at edges or walls.
  Transitions:
    -> DETECT (player enters detection range)

DETECT
  Description: Spots the player, brief reaction pause.
  Duration: 12 frames (0.2 s)
  Visual tell: Exclamation icon flash above head, sprite faces player.
  Audio: Era-appropriate alert sound (grunt/clank/beep).
  Transitions:
    -> CHARGE (after detect duration completes)
    -> PATROL (player leaves detection range during detect window)

CHARGE
  Description: Rushes directly toward player's horizontal position at time of charge start.
  Direction: Locked at start of charge (does not track player mid-charge).
  Speed: 9.0 tiles/s (base, before tier multiplier)
  Duration: Maximum 36 frames (0.6 s) or until hitting a wall/obstacle.
  Visual: Era-appropriate speed lines, screen vibration on contact.
  Damage: Charge damage value (see stats table).
  Transitions:
    -> RECOVER (charge completes, hits wall, or reaches max distance)

RECOVER
  Description: Stunned after charge. Vulnerable to all damage. Cannot move or attack.
  Duration: 42 frames (0.7 s)
  Visual tell: Dazed stars/sparks above head, sprite slumped.
  Transitions:
    -> PATROL (after recovery completes)
```

### Combat Stats (Scaling by Difficulty Tier)

| Stat | Tier 1 | Tier 2 | Tier 3 | Tier 4 | Tier 5 |
|------|--------|--------|--------|--------|--------|
| Health (HP) | 3 | 4 | 5 | 6 | 8 |
| Contact damage (patrol) | 1 | 1 | 2 | 2 | 3 |
| Charge damage | 2 | 3 | 3 | 4 | 5 |
| Detection range (tiles, horizontal) | 6 | 7 | 7 | 8 | 9 |
| Detection range (tiles, vertical) | 2 | 2 | 3 | 3 | 3 |
| Patrol speed (tiles/s) | 2.0 | 2.2 | 2.5 | 2.8 | 3.0 |
| Charge speed (tiles/s) | 7.0 | 8.0 | 9.0 | 10.0 | 11.0 |
| Detect duration (frames) | 15 | 14 | 12 | 10 | 8 |
| Recover duration (frames) | 48 | 45 | 42 | 38 | 36 |
| XP reward | 10 | 15 | 20 | 30 | 45 |

### Frame Data (60 fps)

| Action | Start Frame | Active Frame | End Frame | Total Duration |
|--------|-------------|-------------|-----------|----------------|
| Detect tell | 0 | 0-11 | 12 | 12 frames (0.2 s) |
| Charge startup | 12 | 12-13 | 14 | 2 frames (0.033 s) |
| Charge active | 14 | 14-49 | 50 | 36 frames (0.6 s) max |
| Wall impact stagger | 50 | 50-53 | 54 | 4 frames (0.067 s) |
| Recover (vulnerable) | 50 | 50-91 | 92 | 42 frames (0.7 s) |

### Counterplay

- **Tell Recognition**: The 12-frame detect phase with exclamation icon gives 0.2 s warning. The charge direction locks at initiation, so sidestepping (jumping) after the charge begins is reliable.
- **Positioning**: Jump over the charge trajectory. The Charger cannot attack vertically. After it passes beneath, the player's auto-fire hits it during recovery.
- **Bait and Punish**: Stand at detection range edge to trigger the charge, then jump. The 42-frame recovery window allows 2-3 seconds of free auto-fire damage.
- **Terrain Usage**: Lure Chargers into destructible terrain -- their charge breaks destructible blocks, potentially opening new paths or eliminating the obstacle for the player.

### Difficulty Weight

- Base weight: **1.5**
- Per-tier modifier: +0.3 per tier above 1

### Spawn Constraints

| Constraint | Value |
|------------|-------|
| Minimum ground segment width | 8 tiles |
| Minimum distance from level start | 12 tiles |
| Maximum density | 1 per 8 tiles of ground length |
| Minimum spacing between Chargers | 5 tiles |
| Cannot spawn on | Floating/moving platforms, slopes > 30 degrees |
| Cannot spawn within | 3 tiles of a destructible chokepoint |
| Maximum per screen | 3 |

### Animation Frame Budget

| Animation | Frame Count | FPS | Loop |
|-----------|------------|-----|------|
| Idle/Patrol | 4 frames | 10 | Yes |
| Detect (alert) | 2 frames | 15 | No |
| Charge | 3 frames | 15 | Yes (during charge) |
| Wall impact | 2 frames | 15 | No |
| Recover (dazed) | 3 frames | 8 | Yes (during recover) |
| Hit flash | 2 frames | 15 | No |
| Death | 5 frames | 12 | No |
| **Total unique frames** | **21** | | |

---

## Archetype 2: Shooter (Ranged)

### Concept

A stationary or slow-moving enemy that fires projectiles at the player from a distance. The Shooter pressures the player to close distance or find cover behind destructible terrain. Since the player's weapon auto-fires, Shooters create a "dueling" dynamic where the player must dodge incoming fire while their own weapons deal damage.

### Era Variants

| Era | Variant Name | Sprite Description | Projectile Visual |
|-----|-------------|-------------------|-------------------|
| Stone Age | Spear Thrower | Crouched primitive human, arm cocked | 8x4 flint spear |
| Bronze Age | Slinger | Robed figure with leather sling | 4x4 stone bullet |
| Iron Age | Archer | Standing bowman with quiver | 8x2 iron arrow |
| Medieval | Crossbowman | Armored figure with heavy crossbow | 8x2 crossbow bolt |
| Renaissance | Musketeer | Soldier with flintlock musket, smoke cloud | 4x4 musket ball + 8x8 smoke puff |
| Industrial | Steam Cannon | Mounted brass cannon on tripod, pipes | 6x6 riveted cannonball |
| Modern | Machine Gunner | Prone soldier with bipod LMG | 4x2 bullet tracer |
| Digital | Laser Turret | Rotating head on wall/floor mount | 2-pixel-wide red beam (hitscan, 4-frame duration) |
| Space | Plasma Sentry | Floating orb with barrel extension | 6x6 green plasma bolt |
| Transcendent | Particle Emitter | Geometric crystal, rotating faces | 8x8 expanding energy ring |

### Sprite Dimensions

- Body: 16x16 pixels
- Projectile: varies by era (see table above), all within 8x8 max bounding box
- Collision box (body): 14x14 pixels
- Collision box (projectile): matches visual size, no forgiveness padding

### State Machine (60 fps)

```
IDLE
  Description: Stationary or slow patrol. Faces default direction.
  Patrol speed (if mobile variant): 1.0 tiles/s
  Patrol range: 3 tiles centered on spawn point
  Transitions:
    -> ALERT (player enters detection range)

ALERT
  Description: Locks onto player, aims weapon.
  Duration: 18 frames (0.3 s)
  Visual tell: Weapon raises, targeting indicator appears (era-appropriate: red eye glow, scope glint, etc.)
  Transitions:
    -> FIRE (after alert completes and player is still in range)
    -> IDLE (player leaves detection range during alert)

FIRE
  Description: Launches projectile toward player's position at time of fire.
  Windup: 15 frames (0.25 s) -- visible tell (weapon glow, draw back, chamber flash)
  Fire frame: Frame 15 -- projectile spawned
  Projectile speed: 6.0 tiles/s (base, before tier multiplier)
  Projectile trajectory: Linear toward player's center-mass position at fire frame
  Projectile lifetime: 180 frames (3.0 s) or until terrain/player collision
  Projectile interaction: Destroyed by destructible terrain (breaks 1 block on impact)
  Transitions:
    -> COOLDOWN (immediately after fire frame)

COOLDOWN
  Description: Cannot fire. Weapon recharges.
  Duration: 72 frames (1.2 s) at Tier 1, scales down with tier (see stats)
  Visual: Weapon lowered, reload animation plays
  Transitions:
    -> ALERT (player still in range after cooldown)
    -> IDLE (player out of range after cooldown)
```

### Combat Stats (Scaling by Difficulty Tier)

| Stat | Tier 1 | Tier 2 | Tier 3 | Tier 4 | Tier 5 |
|------|--------|--------|--------|--------|--------|
| Health (HP) | 2 | 3 | 4 | 5 | 6 |
| Contact damage | 1 | 1 | 1 | 2 | 2 |
| Projectile damage | 1 | 2 | 2 | 3 | 4 |
| Detection range (tiles) | 8 | 9 | 10 | 11 | 12 |
| Projectile speed (tiles/s) | 5.0 | 5.5 | 6.0 | 7.0 | 8.0 |
| Cooldown (frames) | 72 | 66 | 60 | 54 | 48 |
| Alert duration (frames) | 20 | 18 | 18 | 16 | 14 |
| Projectiles per volley | 1 | 1 | 1 | 2 | 2 |
| Volley spread (degrees, if 2) | - | - | - | 10 | 15 |
| XP reward | 12 | 18 | 25 | 35 | 50 |

**Tier 4-5 Double Projectile Note:** At Tiers 4 and 5 the Shooter fires 2 projectiles in a spread pattern. Both projectiles are aimed at the player's position but offset by the volley spread angle. This forces the player to dodge laterally rather than simply ducking.

### Frame Data (60 fps)

| Action | Start Frame | Active Frame | End Frame | Total Duration |
|--------|-------------|-------------|-----------|----------------|
| Alert (targeting) | 0 | 0-17 | 18 | 18 frames (0.3 s) |
| Fire windup | 18 | 18-32 | 33 | 15 frames (0.25 s) |
| Fire (projectile spawn) | 33 | 33 | 33 | 1 frame |
| Cooldown (reload) | 34 | 34-105 | 106 | 72 frames (1.2 s) at Tier 1 |

### Counterplay

- **Tell Recognition**: The 18-frame alert + 15-frame windup gives 0.55 s total warning. At longer ranges the player has additional time due to projectile travel.
- **Cover Usage**: Destructible terrain blocks projectiles. The player can use existing cover or break terrain strategically to create sight-line blocks. Projectiles also break destructible blocks, so cover degrades over time.
- **Close the Gap**: Shooters have low HP and low contact damage. Rushing them while dodging the initial volley is effective since the cooldown window leaves them defenseless.
- **Vertical Dodge**: Single projectiles target center-mass. Jumping or ducking avoids the shot. At Tier 4-5, the spread requires wider lateral movement.
- **Prioritization**: Auto-fire weapons target the nearest enemy. Closing distance to a Shooter makes it the auto-fire priority, allowing rapid elimination.

### Difficulty Weight

- Base weight: **2.0**
- Per-tier modifier: +0.4 per tier above 1

### Spawn Constraints

| Constraint | Value |
|------------|-------|
| Minimum platform width | 3 tiles (stationary variant) |
| Must have line of sight | To at least 6 tiles of player approach path |
| Maximum density | 1 per 10 tiles horizontal |
| Minimum spacing between Shooters | 6 tiles |
| Elevation preference | Placed on platforms above player path when possible |
| Cannot spawn on | Moving platforms, slopes |
| Minimum clearance above | 2 tiles (projectile arc headroom) |
| Maximum per screen | 2 |
| Cannot spawn within | 4 tiles of another ranged enemy |

### Animation Frame Budget

| Animation | Frame Count | FPS | Loop |
|-----------|------------|-----|------|
| Idle/Patrol | 3 frames | 8 | Yes |
| Alert (aiming) | 2 frames | 15 | No |
| Fire windup | 3 frames | 12 | No |
| Fire (recoil) | 2 frames | 15 | No |
| Cooldown (reload) | 3 frames | 8 | Yes |
| Hit flash | 2 frames | 15 | No |
| Death | 4 frames | 12 | No |
| Projectile travel | 2 frames | 10 | Yes |
| Projectile impact | 3 frames | 15 | No |
| **Total unique frames** | **24** | | |

---

## Archetype 3: Flyer (Aerial)

### Concept

An airborne enemy that hovers above the player in a sine-wave pattern and periodically swoops or drops bombs. The Flyer exploits the horizontal auto-fire by attacking from above, forcing the player to consider vertical positioning and weapon attachment angles. Flyers break up the horizontal rhythm of ground-based combat.

### Era Variants

| Era | Variant Name | Sprite Description | Attack Style |
|-----|-------------|-------------------|--------------|
| Stone Age | Pterodactyl | Leathery winged reptile, jagged beak | Dive swoop |
| Bronze Age | Fire Hawk | Red-feathered raptor with ember trail | Dive swoop |
| Iron Age | War Eagle | Armored eagle with iron talons | Dive swoop |
| Medieval | Dragon | Small wyvern, bat-wings, fire breath sprite | Dive swoop + drop (fireball) |
| Renaissance | Ornithopter | Da Vinci-style flying machine, wooden frame | Drop (bomb) |
| Industrial | Zeppelin Drone | Small dirigible with propeller, exhaust smoke | Drop (bomb) |
| Modern | Attack Helicopter | Miniature helicopter, rotor blur | Drop (missile) |
| Digital | Hover Bot | Anti-gravity disc, blue glow underside | Dive swoop |
| Space | Fighter Pod | Sleek single-seat fighter, engine trail | Dive swoop + drop (laser) |
| Transcendent | Phase Wraith | Translucent spectral form, flickering in/out | Dive swoop (phases through terrain) |

### Sprite Dimensions

- Body: 16x16 pixels
- Drop projectile (where applicable): 8x8 pixels
- Collision box: 12x12 pixels (generous forgiveness for aerial targets)
- Hover shadow: 8x4 pixel dark ellipse rendered on ground below

### State Machine (60 fps)

```
HOVER
  Description: Floats in sine-wave pattern above patrol zone.
  Horizontal amplitude: 4.0 tiles
  Vertical amplitude: 1.5 tiles
  Period: 120 frames (2.0 s) per full sine cycle
  Base altitude: 3-5 tiles above nearest ground surface
  Speed: 2.0 tiles/s horizontal drift
  Transitions:
    -> AGGRO (player enters detection sphere: 7-tile radius)

AGGRO
  Description: Locks onto player, prepares to dive or drop.
  Duration: 15 frames (0.25 s)
  Visual tell: Sprite flashes red/white 3 times, shadow snaps to player position.
  Audio: Rising screech/whine/hum (era-appropriate).
  Attack selection: 70% dive swoop, 30% drop attack (if variant supports drops)
  Transitions:
    -> DIVE (dive swoop selected)
    -> DROP (drop attack selected, if available; otherwise -> DIVE)

DIVE
  Description: Swoops diagonally toward player's position at time of dive start.
  Trajectory: Diagonal line from current position toward player center-mass.
  Speed: 10.0 tiles/s (base, before tier multiplier)
  Duration: Maximum 24 frames (0.4 s)
  Damage: Dive damage on contact.
  Direction: Locked at start of dive (predictive, does not track).
  Transitions:
    -> RETREAT (dive completes, passes player Y-level, or 24 frames elapsed)

DROP
  Description: Releases a projectile downward while maintaining hover altitude.
  Projectile: Falls straight down at 8.0 tiles/s
  Projectile damage: Drop damage value (see stats)
  Projectile lifetime: Until terrain collision (creates 8x8 impact effect)
  The Flyer does not descend during a drop attack.
  Transitions:
    -> RETREAT (after projectile is released)

RETREAT
  Description: Climbs back to hover altitude, moving away from player.
  Speed: 3.5 tiles/s upward and horizontally away
  Duration: Until reaching hover altitude (typically 30-60 frames)
  Transitions:
    -> HOVER (upon reaching hover altitude; resumes sine pattern from new position)
    -> AGGRO (if player still in detection range after 36-frame post-retreat cooldown)
```

### Combat Stats (Scaling by Difficulty Tier)

| Stat | Tier 1 | Tier 2 | Tier 3 | Tier 4 | Tier 5 |
|------|--------|--------|--------|--------|--------|
| Health (HP) | 2 | 3 | 3 | 4 | 5 |
| Contact damage (hover) | 1 | 1 | 1 | 2 | 2 |
| Dive damage | 2 | 2 | 3 | 3 | 4 |
| Drop projectile damage | 1 | 2 | 2 | 3 | 3 |
| Detection radius (tiles) | 6 | 6 | 7 | 8 | 9 |
| Dive speed (tiles/s) | 8.0 | 9.0 | 10.0 | 11.0 | 12.0 |
| Aggro duration (frames) | 18 | 16 | 15 | 12 | 10 |
| Post-retreat cooldown (frames) | 48 | 42 | 36 | 30 | 24 |
| Drop projectile speed (tiles/s) | 6.0 | 7.0 | 8.0 | 9.0 | 10.0 |
| XP reward | 15 | 22 | 30 | 40 | 55 |

### Frame Data (60 fps)

| Action | Start Frame | Active Frame | End Frame | Total Duration |
|--------|-------------|-------------|-----------|----------------|
| Aggro tell | 0 | 0-14 | 15 | 15 frames (0.25 s) |
| Dive active | 15 | 15-38 | 39 | 24 frames (0.4 s) max |
| Drop release | 15 | 15 | 15 | 1 frame (projectile spawns) |
| Retreat climb | 39 | 39-68 | 69 | ~30 frames (0.5 s) typical |
| Post-retreat cooldown | 69 | 69-104 | 105 | 36 frames (0.6 s) at Tier 3 |

### Counterplay

- **Tell Recognition**: The 15-frame aggro flash is the primary cue. The dive direction locks at the start, so a quick lateral dodge after seeing the flash avoids the swoop entirely.
- **Hover Pattern Exploit**: The sine-wave hover brings the Flyer within auto-fire range at the low point of its cycle. Patient players can damage the Flyer without triggering aggro if positioned at detection range edge.
- **Retreat Vulnerability**: The Flyer moves slowly during retreat (3.5 tiles/s upward). This is the best damage window for auto-fire. Weapon attachments with upward-angle capability are highly effective.
- **Terrain Cover**: Drop projectiles are stopped by terrain. Standing under solid platforms or destructible terrain blocks drop attacks.
- **Priority Targeting**: Flyers can distract from ground threats. Players should clear or avoid ground enemies first, then deal with Flyers during hover phases.

### Difficulty Weight

- Base weight: **2.5**
- Per-tier modifier: +0.4 per tier above 1

### Spawn Constraints

| Constraint | Value |
|------------|-------|
| Minimum vertical clearance | 6 tiles above ground (hover room + dive space) |
| Minimum horizontal space | 10 tiles wide (sine-wave patrol) |
| Maximum density | 1 per 14 tiles horizontal |
| Cannot spawn in | Tunnels or corridors < 5 tiles high |
| Maximum per screen | 2 |
| Minimum spacing between Flyers | 8 tiles horizontal |
| Minimum spacing from ceiling | 2 tiles (needs hover headroom) |
| Preferred placement | Open areas, above gaps/pits, before boss arenas |

### Animation Frame Budget

| Animation | Frame Count | FPS | Loop |
|-----------|------------|-----|------|
| Hover (wings/propeller) | 4 frames | 12 | Yes |
| Aggro (flash) | 3 frames | 15 | No |
| Dive | 3 frames | 15 | Yes (during dive) |
| Drop release | 2 frames | 15 | No |
| Retreat (climb) | 3 frames | 10 | Yes |
| Hit flash | 2 frames | 15 | No |
| Death (falling) | 5 frames | 12 | No |
| Drop projectile | 2 frames | 10 | Yes |
| Drop impact | 3 frames | 15 | No |
| **Total unique frames** | **27** | | |

---

## Archetype 4: Tank (Heavy)

### Concept

A massive, slow-moving enemy with high HP that blocks the player's forward path. Tanks absorb many hits and force the player to commit sustained fire while managing other threats. They function as mid-level gate encounters, requiring the player to have adequate weapon attachments to deal with the HP pool in a reasonable time. The Tank rewards aggressive play -- retreating means giving up screen real estate.

### Era Variants

| Era | Variant Name | Sprite Description |
|-----|-------------|-------------------|
| Stone Age | Mammoth | Woolly mammoth, tusks lowered, shaking ground |
| Bronze Age | War Chariot | Pulled by two oxen, armored rider with lance |
| Iron Age | War Elephant | Armored elephant with iron-plated howdah |
| Medieval | Siege Golem | Animated stone construct, glowing rune core |
| Renaissance | Cannon Wagon | Fortified wheeled gun platform, thick oak and iron |
| Industrial | Steam Tank | Riveted iron hull, belching smokestacks, grinding treads |
| Modern | Battle Tank | Low-profile main battle tank, reactive armor panels |
| Digital | Heavy Mech | Bipedal walker, thick armor plates, shoulder turrets |
| Space | Mech Walker | Four-legged assault platform, energy shields |
| Transcendent | Quantum Titan | Massive humanoid of shifting light, reality-warping aura |

### Sprite Dimensions

- Body: 32x24 pixels (double-wide, 1.5x tall -- occupies 2x1.5 tiles)
- Collision box: 28x20 pixels
- Ground shadow: 32x6 pixels
- Screen shake: 1-pixel vertical shake every 30 frames during advance (footstep/tread rumble)

### State Machine (60 fps)

```
ADVANCE
  Description: Moves slowly and steadily toward the player's side of the screen.
  Speed: 1.5 tiles/s (base, before tier multiplier)
  Direction: Always advances toward the player (left-to-right or right-to-left depending on relative position).
  Behavior: Does NOT stop when detecting the player -- always moves forward.
  Destroys destructible terrain on contact (plows through blocks).
  Transitions:
    -> STOMP (player within 3 tiles horizontal)
    -> STUNNED (HP drops below 25%)

STOMP
  Description: Ground-pound area attack.
  Windup: 18 frames (0.3 s) -- rears up / barrel raises / leg lifts
  Active: 6 frames (0.1 s) -- ground impact
  Damage zone: 4 tiles horizontal on both sides, 2 tiles vertical (ground wave)
  Visual: Terrain crack effect, particle debris, screen shake (2px, 12 frames)
  Transitions:
    -> RECOVER (after stomp completes)

RECOVER
  Description: Brief pause after stomp. Continues absorbing damage at normal rate.
  Duration: 30 frames (0.5 s)
  Transitions:
    -> ADVANCE (after recovery)
    -> STOMP (player still within 3 tiles -- chains stomps with recovery gaps)
    -> STUNNED (HP drops below 25%)

STUNNED
  Description: HP threshold reached. Tank is temporarily weakened.
  Trigger: HP drops below 25% of max
  Duration: 60 frames (1.0 s)
  Visual: Armor cracks glow, sparks/smoke/energy leaks, sprite flickers
  Damage taken: 1.5x multiplier during stun
  Occurs only once per Tank instance.
  Transitions:
    -> ADVANCE (after stun, resumes at 1.2x normal speed -- enraged)
```

### Combat Stats (Scaling by Difficulty Tier)

| Stat | Tier 1 | Tier 2 | Tier 3 | Tier 4 | Tier 5 |
|------|--------|--------|--------|--------|--------|
| Health (HP) | 12 | 16 | 20 | 25 | 30 |
| Contact damage (advance) | 2 | 2 | 3 | 3 | 4 |
| Stomp damage | 2 | 3 | 3 | 4 | 5 |
| Stomp range (tiles, each side) | 3 | 3 | 4 | 4 | 5 |
| Advance speed (tiles/s) | 1.2 | 1.3 | 1.5 | 1.7 | 2.0 |
| Enraged speed (tiles/s) | 1.5 | 1.6 | 1.8 | 2.0 | 2.4 |
| Stomp windup (frames) | 24 | 21 | 18 | 15 | 12 |
| Stomp recovery (frames) | 36 | 33 | 30 | 27 | 24 |
| Stun HP threshold | 25% | 25% | 25% | 25% | 25% |
| Stun duration (frames) | 72 | 66 | 60 | 54 | 48 |
| XP reward | 40 | 55 | 75 | 100 | 140 |

### Frame Data (60 fps)

| Action | Start Frame | Active Frame | End Frame | Total Duration |
|--------|-------------|-------------|-----------|----------------|
| Stomp windup | 0 | 0-17 | 18 | 18 frames (0.3 s) at Tier 3 |
| Stomp active (damage) | 18 | 18-23 | 24 | 6 frames (0.1 s) |
| Stomp recovery | 24 | 24-53 | 54 | 30 frames (0.5 s) at Tier 3 |
| Stun (vulnerable) | 0 | 0-59 | 60 | 60 frames (1.0 s) at Tier 3 |
| Terrain break (on advance) | - | Immediate on contact | - | 0 frames (synchronous) |

### Counterplay

- **Sustained Fire**: The Tank has the highest HP of all archetypes. Players need weapon attachments dealing consistent DPS. This is a gear check encounter.
- **Stomp Jump**: The stomp ground wave is avoided by being airborne. Jump during the 18-frame windup (at Tier 3) to completely avoid stomp damage. The windup is visually distinct.
- **Stun Window**: When HP drops below 25%, the 60-frame stun with 1.5x damage multiplier is the burst damage opportunity. Save powerful attachment cooldowns for this window.
- **Terrain Advantage**: Tanks plow through destructible terrain. Players can use this by positioning so the Tank breaks open paths or resource caches.
- **Retreat Risk**: Because the Tank always advances, retreating means being pushed backward into already-cleared terrain. Standing ground and dealing damage is usually optimal.
- **Vertical Escape**: Jump over the Tank to get behind it. The Tank turns slowly (takes 18 frames to reverse), giving a free damage window from behind.

### Difficulty Weight

- Base weight: **5.0**
- Per-tier modifier: +0.8 per tier above 1

### Spawn Constraints

| Constraint | Value |
|------------|-------|
| Minimum ground segment width | 14 tiles (needs room to advance) |
| Minimum distance from level start | 30 tiles |
| Maximum density | 1 per 25 tiles horizontal |
| Maximum per screen | 1 |
| Cannot spawn within | 12 tiles of another Tank |
| Cannot spawn within | 8 tiles of a boss arena |
| Minimum clearance above | 3 tiles (oversized sprite) |
| Spawn position | Always spawns at far edge of screen, advances toward player |
| Preferred placement | Wide open areas, before era transitions, mid-level gates |
| Cannot co-spawn with | More than 2 other enemies on same screen |

### Animation Frame Budget

| Animation | Frame Count | FPS | Loop |
|-----------|------------|-----|------|
| Advance (walk/tread) | 4 frames | 8 | Yes |
| Stomp windup | 3 frames | 10 | No |
| Stomp impact | 2 frames | 15 | No |
| Stomp recovery | 2 frames | 8 | No |
| Stunned | 3 frames | 10 | Yes |
| Enraged advance | 4 frames | 12 | Yes |
| Hit flash | 2 frames | 15 | No |
| Death | 6 frames | 10 | No |
| Terrain break effect | 3 frames | 15 | No |
| **Total unique frames** | **29** | | |

---

## Archetype 5: Swarm (Group)

### Concept

Individually weak enemies that spawn in groups of 3-5. Swarm units are small, fast, and erratic. They overwhelm the player through volume rather than individual threat. The auto-fire weapon handles them well if the player positions to funnel them into a line, but spread-out swarms from multiple directions create panic. Swarms are the "popcorn" enemies that fill space between major threats and provide satisfying moment-to-moment auto-fire feedback.

### Era Variants

| Era | Variant Name | Sprite Description |
|-----|-------------|-------------------|
| Stone Age | Rats | Brown rodents, skittering in packs |
| Bronze Age | Scarab Beetles | Shiny bronze-green beetles, mandibles clicking |
| Iron Age | Hornets | Striped flying insects, stinger visible |
| Medieval | Bats | Dark winged bats, red eyes, erratic flutter |
| Renaissance | Clockwork Mice | Brass wind-up mice, key on back, whirring |
| Industrial | Clockwork Spiders | Multi-legged brass arachnids, steam puffs |
| Modern | Surveillance Drones | Small quadcopter drones, blinking red LED |
| Digital | Nano Drones | Tiny glowing triangles, swarm formation |
| Space | Repair Bots (hostile) | Small spherical bots with sparking welders |
| Transcendent | Void Mites | Flickering dark particles, leaving void trails |

### Sprite Dimensions

- Body: 8x8 pixels per unit (half-tile, small target)
- Collision box: 6x6 pixels per unit
- Group formation: 3-5 units within a 6x4 tile area
- Each unit has independent movement within the formation zone

### State Machine (60 fps) -- Per Individual Unit

```
SCATTER
  Description: Each unit moves in a semi-random pattern within formation zone.
  Speed: 4.0 tiles/s (base, before tier multiplier)
  Movement: Random direction change every 20-40 frames (randomized per unit, seeded)
  Vertical range: +/- 2 tiles from spawn center
  Horizontal range: +/- 3 tiles from spawn center
  Transitions:
    -> CONVERGE (player enters detection range: 8 tiles from group center)

CONVERGE
  Description: All units orient toward the player and begin closing distance.
  Speed: 5.5 tiles/s (base, before tier multiplier)
  Behavior: Each unit pathfinds toward player with slight offset (avoids stacking).
  Offset: Each unit targets player position + random offset of 0-1 tile (prevents perfect overlap).
  Duration: Continuous while player is in detection range.
  Transitions:
    -> ATTACK (any unit reaches within 1 tile of player)
    -> SCATTER (player leaves detection range + 2 tiles hysteresis buffer)

ATTACK
  Description: Unit-level contact damage on reaching the player.
  Damage: Applied on contact. Unit passes through player and continues.
  Post-contact: Unit continues in its current direction for 15 frames, then re-enters CONVERGE.
  No cooldown between attacks -- units can hit repeatedly if player stands still.
  Transitions:
    -> CONVERGE (15 frames after contact, re-orient to player)

SCATTER (post-group-break)
  Description: If group is reduced to 1 unit, surviving unit flees.
  Flee speed: 6.0 tiles/s, directly away from player
  Flee duration: 90 frames (1.5 s), then despawns OR returns to SCATTER if player out of range.
  Transitions:
    -> Despawn (after 90 frames of fleeing)
    -> SCATTER (player out of range, unit persists at spawn zone)
```

### Combat Stats (Scaling by Difficulty Tier)

All stats are per individual swarm unit.

| Stat | Tier 1 | Tier 2 | Tier 3 | Tier 4 | Tier 5 |
|------|--------|--------|--------|--------|--------|
| Health per unit (HP) | 1 | 1 | 1 | 2 | 2 |
| Contact damage per unit | 1 | 1 | 1 | 1 | 2 |
| Group size | 3 | 3 | 4 | 4 | 5 |
| Scatter speed (tiles/s) | 3.5 | 3.8 | 4.0 | 4.5 | 5.0 |
| Converge speed (tiles/s) | 4.5 | 5.0 | 5.5 | 6.0 | 7.0 |
| Detection range (tiles) | 7 | 7 | 8 | 9 | 10 |
| Direction change interval (frames) | 35-50 | 30-45 | 25-40 | 20-35 | 15-30 |
| Flee threshold (remaining units) | 1 | 1 | 1 | 1 | 1 |
| XP reward per unit | 3 | 5 | 7 | 10 | 15 |
| XP bonus (full group kill) | 5 | 8 | 12 | 18 | 25 |

### Frame Data (60 fps)

| Action | Start Frame | Active Frame | End Frame | Total Duration |
|--------|-------------|-------------|-----------|----------------|
| Scatter (direction change) | 0 | Continuous | - | 20-50 frames between changes |
| Converge (orient to player) | 0 | 0-5 | 6 | 6 frames (0.1 s) orientation snap |
| Contact damage | - | Immediate on collision | - | 0 frames (collision-based) |
| Post-contact drift | 0 | 0-14 | 15 | 15 frames (0.25 s) |
| Flee (last survivor) | 0 | 0-89 | 90 | 90 frames (1.5 s) then despawn |

### Counterplay

- **Auto-Fire Efficiency**: Position so the swarm approaches from the direction the weapon faces. Auto-fire can eliminate 1-HP units rapidly if they are in a line.
- **Funnel Points**: Use terrain (destructible or static) to create chokepoints that force swarm units into auto-fire lanes.
- **Area Weapons**: Weapon attachments with area-of-effect (AoE) or spread shots are highly effective against swarms. This is the primary use case for AoE attachments.
- **Keep Moving**: Standing still allows convergence from all directions. Continuous lateral movement keeps the swarm in a trailing cluster behind the player, in the auto-fire zone.
- **Priority Assessment**: Swarms are individually non-threatening (1 damage per unit). They become dangerous only when combined with other archetypes. Clear the high-priority target (Tank, Shooter) first, then mop up the swarm.
- **Last Survivor Flee**: When reduced to 1 unit, it flees. Players can ignore the fleeing unit (it despawns) to focus on other threats.

### Difficulty Weight

- Base weight per group: **2.0** (entire group counts as one spawn)
- Per-tier modifier: +0.3 per tier above 1
- Per extra unit above 3: +0.3 per additional unit

### Spawn Constraints

| Constraint | Value |
|------------|-------|
| Minimum open area | 6x4 tiles (formation zone) |
| Minimum distance from level start | 8 tiles |
| Maximum density | 1 group per 12 tiles horizontal |
| Maximum groups per screen | 2 |
| Minimum spacing between groups | 10 tiles |
| Cannot spawn in | Narrow corridors < 3 tiles wide |
| Cannot spawn within | 6 tiles of a Tank (visual clutter) |
| Preferred placement | Open areas, after destructible terrain sections, near weapon pickups |
| Spawn trigger | Spawns when player enters screen segment (not pre-placed like other archetypes) |

### Animation Frame Budget

Per individual swarm unit:

| Animation | Frame Count | FPS | Loop |
|-----------|------------|-----|------|
| Scatter (idle movement) | 2 frames | 10 | Yes |
| Converge (attack movement) | 2 frames | 15 | Yes |
| Contact flash | 1 frame | - | No |
| Hit | 1 frame | - | No |
| Death (pop) | 3 frames | 15 | No |
| Flee | 2 frames | 12 | Yes |
| **Total unique frames per unit** | **11** | | |
| **Total for group (shared sprites)** | **11** (same sheet, tinted per era) | | |

---

## 6. Archetype Comparison Matrix

### Core Stats at Tier 3 (Mid-Game Reference)

| Property | Charger | Shooter | Flyer | Tank | Swarm |
|----------|---------|---------|-------|------|-------|
| Category | Melee | Ranged | Aerial | Heavy | Group |
| HP | 5 | 4 | 3 | 20 | 1 per unit (x4) |
| Primary Damage | 3 (charge) | 2 (projectile) | 3 (dive) | 3 (stomp) | 1 (contact, x4) |
| Movement Speed | 9.0 t/s (charge) | 0-1.0 t/s | 10.0 t/s (dive) | 1.5 t/s | 5.5 t/s (converge) |
| Detection Range | 7 tiles | 10 tiles | 7 tiles | Always active | 8 tiles |
| Threat Axis | Horizontal | Horizontal (ranged) | Vertical/Diagonal | Horizontal (advance) | Omnidirectional |
| Difficulty Weight | 1.5 | 2.0 | 2.5 | 5.0 | 2.0 (group) |
| Sprite Size | 16x16 | 16x16 | 16x16 | 32x24 | 8x8 (per unit) |
| Vulnerable Window | Recover (42f) | Cooldown (60f) | Retreat (30f) | Stun (60f) | Always (1 HP) |

### Threat Profile Comparison

| Property | Charger | Shooter | Flyer | Tank | Swarm |
|----------|---------|---------|-------|------|-------|
| Danger if ignored | Medium | High | Medium | Very High | Low |
| Time to kill (Tier 3, base weapon) | ~1.5 s | ~1.2 s | ~0.9 s | ~6.0 s | ~0.3 s per unit |
| Player skill required | Low | Medium | Medium | Low (gear check) | Low |
| Terrain interaction | Breakable by charge | Projectiles break blocks | None (airborne) | Plows through terrain | None |
| Best attachment against | Spread shot | Piercing shot | Upward angle | High-ROF / damage boost | AoE / spread shot |

### Encounter Role Matrix

| Role | Primary Archetype | Support Archetype |
|------|-------------------|-------------------|
| Pressure (push player back) | Tank | Charger |
| Zone control (limit safe zones) | Shooter | Flyer |
| Distraction (split attention) | Swarm | Flyer |
| Gear check (test DPS) | Tank | Shooter |
| Spatial puzzle (positioning) | Flyer | Charger |

---

## 7. Difficulty Budget System

### Budget Formula

The level generator assigns an enemy difficulty budget to each screen segment based on the current era and segment type.

```
segment_enemy_budget = base_budget * segment_type_multiplier * tier_multiplier

base_budget = 10.0 difficulty points

segment_type_multiplier:
  intro_segment   = 0.4   (start of era, teaches new skins)
  traversal       = 0.7   (primarily destructible terrain, light enemies)
  combat_segment  = 1.2   (enemy-focused, terrain is backdrop)
  ambush_segment  = 1.5   (surprise spawns, high density)
  gauntlet        = 2.0   (sustained wave-based combat before boss)
  boss_arena      = 0.0   (boss encounter only, no regular enemies)

tier_multiplier:
  Tier 1 (Stone, Bronze)          = 0.7
  Tier 2 (Iron, Medieval)         = 0.85
  Tier 3 (Renaissance, Industrial)= 1.0
  Tier 4 (Modern, Digital)        = 1.2
  Tier 5 (Space, Transcendent)    = 1.5
```

### Enemy Weight Table (Quick Reference)

| Archetype | Base Weight | Tier 1 | Tier 2 | Tier 3 | Tier 4 | Tier 5 |
|-----------|------------|--------|--------|--------|--------|--------|
| Charger | 1.5 | 1.5 | 1.8 | 2.1 | 2.4 | 2.7 |
| Shooter | 2.0 | 2.0 | 2.4 | 2.8 | 3.2 | 3.6 |
| Flyer | 2.5 | 2.5 | 2.9 | 3.3 | 3.7 | 4.1 |
| Tank | 5.0 | 5.0 | 5.8 | 6.6 | 7.4 | 8.2 |
| Swarm (group of 3) | 2.0 | 2.0 | 2.3 | 2.6 | 2.9 | 3.2 |
| Swarm (group of 4) | 2.3 | 2.3 | 2.6 | 2.9 | 3.2 | 3.5 |
| Swarm (group of 5) | 2.6 | 2.6 | 2.9 | 3.2 | 3.5 | 3.8 |

### Example: Combat Segment, Tier 3 (Renaissance Era)

```
budget = 10.0 * 1.2 * 1.0 = 12.0 difficulty points

Possible enemy compositions:

  Composition A (balanced):
    2x Charger (2.1 each)   = 4.2
    1x Shooter (2.8)        = 2.8
    1x Swarm group of 4     = 2.9
    1x Flyer (3.3)          = EXCEEDS -- drop to:
    Total                   = 9.9 (82.5% of budget, valid)

  Composition B (heavy gate):
    1x Tank (6.6)           = 6.6
    1x Swarm group of 3     = 2.6
    1x Charger (2.1)        = 2.1
    Total                   = 11.3 (94.2% of budget, valid)

  Composition C (ranged pressure):
    2x Shooter (2.8 each)   = 5.6
    2x Charger (2.1 each)   = 4.2
    1x Swarm group of 3     = 2.6
    Total                   = 12.4 -- OVER BUDGET, reduce:
    Drop 1 Charger          = 10.3 (85.8% of budget, valid)

  Composition D (aerial assault):
    2x Flyer (3.3 each)     = 6.6
    1x Swarm group of 4     = 2.9
    1x Charger (2.1)        = 2.1
    Total                   = 11.6 (96.7% of budget, valid)
```

### Gauntlet Segment, Tier 5 (Transcendent Era)

```
budget = 10.0 * 2.0 * 1.5 = 30.0 difficulty points

Possible enemy compositions (spawned in waves):

  Wave 1: 2x Swarm group of 5 (3.8 each) + 2x Charger (2.7 each) = 13.0
  Wave 2: 1x Tank (8.2) + 1x Shooter (3.6) = 11.8
  Wave 3: 2x Flyer (4.1 each) = 8.2
  Total across waves: 33.0 -- over budget, reduce Wave 3 to 1 Flyer:
  Adjusted total: 28.9 (96.3% of budget, valid)
```

### Enemy Selection Rules

1. **Variety Minimum**: Each combat segment must contain at least 2 different archetype types. Gauntlets require at least 3.
2. **Tank Limit**: Maximum 1 Tank per segment. Tanks do not appear in intro segments.
3. **Shooter Positioning**: Shooters must be placed with sight-line validation -- at least 6 tiles of clear line-of-sight to player approach path.
4. **Swarm Spacing**: Swarm groups cannot overlap formation zones. Minimum 10 tiles between group centers.
5. **Flyer Clearance**: Flyers only spawn in segments with at least 6 tiles vertical clearance.
6. **Era Introduction Rule**: The first segment of each new era uses only Chargers and Swarms at 50% budget to let the player recognize new visual skins before facing the full roster.
7. **Budget Tolerance**: Actual placement must be within 80-100% of budget. Never over budget. Slight under-budget is acceptable and preferred over over-budget.
8. **Density Cap**: Total enemies per screen segment (including individual swarm units) cannot exceed 12 regardless of remaining budget.

---

## 8. Global Spawn Validation Rules

These rules are evaluated by the level generator after initial enemy placement. Any placement violating these rules is adjusted or removed.

### Fairness Rules

1. **No Unavoidable Damage**: Every enemy encounter must have at least one avoidance path. The player must be able to dodge, jump over, or retreat from any attack with correctly timed input.

2. **Reaction Distance**: No enemy can be placed closer than 5 tiles to any screen edge where the player enters. This guarantees at least 0.5 s of visibility before the player is in detection range.

3. **Auto-Fire Coverage**: At least 80% of enemies in a segment must be reachable by the player's horizontal auto-fire within 2 seconds of entering the segment. Enemies placed at extreme elevations (Flyers) are exempt from this rule.

4. **Damage Overlap Prevention**: No more than 2 simultaneous damage sources can threaten the player at any single ground position. This prevents unavoidable multi-hit situations.

### Spacing Rules

5. **Cluster Spacing**: No more than 3 enemies (counting individual swarm units as 0.5 each) within a 6-tile horizontal span.

6. **Vertical Stacking**: No more than 1 non-Flyer enemy directly above another within 3 tiles vertical distance.

7. **Archetype Separation**: Same-archetype enemies (except Swarm units within a group) must be at least 5 tiles apart.

### Zone Rules

8. **Boss Arena Buffer**: No regular enemies within 10 tiles of a boss arena entrance.

9. **Checkpoint Buffer**: No enemies within 4 tiles of any checkpoint marker.

10. **Era Transition Buffer**: No enemies within 6 tiles before or after an era transition boundary.

11. **Weapon Pickup Proximity**: At least 1 weapon attachment pickup must exist within 20 tiles preceding any Tank encounter.

12. **Health Availability**: At least 1 health restoration item must exist within 25 tiles of any segment with budget usage above 80%.

### Destructible Terrain Interaction Rules

13. **Terrain-Enemy Clearance**: Enemies do not spawn inside or overlapping destructible terrain segments. Minimum 1-tile gap between enemy spawn position and nearest destructible block.

14. **Cover Availability**: In segments containing Shooters, at least one destructible terrain cluster (minimum 3 blocks) must exist between the Shooter and the player's approach path.

15. **Tank Path**: If a Tank is present, the segment must have at least 14 tiles of continuous ground. Destructible terrain in the Tank's path is permitted (the Tank will destroy it).

16. **Swarm Arena**: Segments with Swarm groups must have at least a 6x4 tile open area (or openable via destructible terrain).

---

## 9. Era Variant Visual Guide

### Per-Era Skin Requirements

Each archetype requires a complete sprite sheet per era. The following table defines the minimum unique visual elements per era variant.

| Element | Charger | Shooter | Flyer | Tank | Swarm Unit |
|---------|---------|---------|-------|------|------------|
| Color palette | 4 colors + transparent | 4 colors + transparent | 4 colors + transparent | 6 colors + transparent | 3 colors + transparent |
| Idle/Patrol frames | 4 | 3 | 4 | 4 | 2 |
| Attack frames | 7 | 7 | 8 | 7 | 3 |
| Hit/Death frames | 7 | 6 | 7 | 8 | 4 |
| Unique VFX frames | 3 (charge trail) | 5 (projectile + impact) | 5 (dive trail + drop + impact) | 3 (terrain break) | 1 (pop) |
| **Total frames per era** | **21** | **24** (incl. projectile) | **27** (incl. drop) | **29** | **11** |

### Total Asset Count

| Resource | Per Archetype | x10 Eras | Total |
|----------|--------------|----------|-------|
| Charger sprite frames | 21 | 210 | 210 |
| Shooter sprite frames (incl. projectile) | 24 | 240 | 240 |
| Flyer sprite frames (incl. drop) | 27 | 270 | 270 |
| Tank sprite frames | 29 | 290 | 290 |
| Swarm sprite frames | 11 | 110 | 110 |
| **Grand total sprite frames** | **112** | - | **1,120** |

---

## Appendix A: Complete Animation Frame Budget Summary

All animations target 60 fps game loop. Animation playback rates are specified per animation to balance smoothness with sprite budget.

### Charger (21 frames per era)

| Animation | Frames | Playback FPS | Game Frames per Anim Frame | Loop | Notes |
|-----------|--------|-------------|---------------------------|------|-------|
| Idle/Patrol walk | 4 | 10 | 6 | Yes | Leg cycle |
| Detect alert | 2 | 15 | 4 | No | Exclamation flash |
| Charge forward | 3 | 15 | 4 | Yes | Speed blur |
| Wall impact | 2 | 15 | 4 | No | Recoil |
| Recover daze | 3 | 8 | 7.5 | Yes | Stars/sparks |
| Hit flash | 2 | 15 | 4 | No | White flash |
| Death explode | 5 | 12 | 5 | No | Era-appropriate breakup |

### Shooter (24 frames per era, including projectile)

| Animation | Frames | Playback FPS | Game Frames per Anim Frame | Loop | Notes |
|-----------|--------|-------------|---------------------------|------|-------|
| Idle/Patrol | 3 | 8 | 7.5 | Yes | Subtle sway |
| Alert aim | 2 | 15 | 4 | No | Weapon raise |
| Fire windup | 3 | 12 | 5 | No | Glow/draw/chamber |
| Fire recoil | 2 | 15 | 4 | No | Kick-back |
| Cooldown reload | 3 | 8 | 7.5 | Yes | Reload cycle |
| Hit flash | 2 | 15 | 4 | No | White flash |
| Death | 4 | 12 | 5 | No | Collapse |
| Projectile travel | 2 | 10 | 6 | Yes | Spin/glow |
| Projectile impact | 3 | 15 | 4 | No | Burst/shatter |

### Flyer (27 frames per era, including drop)

| Animation | Frames | Playback FPS | Game Frames per Anim Frame | Loop | Notes |
|-----------|--------|-------------|---------------------------|------|-------|
| Hover wings | 4 | 12 | 5 | Yes | Wing flap/propeller |
| Aggro flash | 3 | 15 | 4 | No | Red/white strobe |
| Dive swoop | 3 | 15 | 4 | Yes | Tuck/streamline |
| Drop release | 2 | 15 | 4 | No | Release pose |
| Retreat climb | 3 | 10 | 6 | Yes | Wings up |
| Hit flash | 2 | 15 | 4 | No | White flash |
| Death fall | 5 | 12 | 5 | No | Spiral down |
| Drop projectile | 2 | 10 | 6 | Yes | Falling object |
| Drop impact | 3 | 15 | 4 | No | Ground explosion |

### Tank (29 frames per era)

| Animation | Frames | Playback FPS | Game Frames per Anim Frame | Loop | Notes |
|-----------|--------|-------------|---------------------------|------|-------|
| Advance walk | 4 | 8 | 7.5 | Yes | Heavy footstep/tread |
| Stomp windup | 3 | 10 | 6 | No | Rear up |
| Stomp impact | 2 | 15 | 4 | No | Ground crack |
| Stomp recovery | 2 | 8 | 7.5 | No | Settle |
| Stunned | 3 | 10 | 6 | Yes | Sparks/smoke |
| Enraged advance | 4 | 12 | 5 | Yes | Faster, glowing |
| Hit flash | 2 | 15 | 4 | No | White flash |
| Death | 6 | 10 | 6 | No | Multi-part explosion |
| Terrain break VFX | 3 | 15 | 4 | No | Block shatter |

### Swarm Unit (11 frames per era)

| Animation | Frames | Playback FPS | Game Frames per Anim Frame | Loop | Notes |
|-----------|--------|-------------|---------------------------|------|-------|
| Scatter move | 2 | 10 | 6 | Yes | Scurry/flutter |
| Converge move | 2 | 15 | 4 | Yes | Faster version |
| Contact flash | 1 | - | - | No | Single-frame impact |
| Hit | 1 | - | - | No | Single-frame stagger |
| Death pop | 3 | 15 | 4 | No | Small burst |
| Flee | 2 | 12 | 5 | Yes | Panicked version of scatter |

---

## Appendix B: Performance Budget

### Maximum Simultaneous Enemies

| Constraint | Limit | Rationale |
|------------|-------|-----------|
| Total enemy entities on screen | 12 | Mobile GPU sprite batch limit |
| Total enemy entities in memory (loaded) | 20 | Off-screen buffer zone enemies |
| Simultaneous projectiles | 8 | Includes Shooter projectiles and Flyer drops |
| Simultaneous particle effects | 16 | Death pops, charge trails, impact effects |
| Active state machines | 12 | CPU budget for AI updates per frame |
| Collision checks per frame | 24 | Enemy-player and enemy-terrain combined |

### Memory Per Archetype (Estimated)

| Archetype | Sprite Sheet (per era) | State Machine Data | Total per Instance |
|-----------|----------------------|-------------------|-------------------|
| Charger | ~5.4 KB (21 frames x 16x16 x 1 byte indexed) | 64 bytes | ~5.5 KB |
| Shooter | ~6.1 KB (24 frames, mixed sizes) | 96 bytes | ~6.2 KB |
| Flyer | ~6.9 KB (27 frames, mixed sizes) | 96 bytes | ~7.0 KB |
| Tank | ~14.9 KB (29 frames x 32x24 x 1 byte indexed) | 128 bytes | ~15.0 KB |
| Swarm (per unit) | ~0.7 KB (11 frames x 8x8 x 1 byte indexed) | 48 bytes | ~0.75 KB |
| Swarm (group of 5) | ~3.5 KB | 240 bytes | ~3.75 KB |

### Total Sprite Memory (All Eras Loaded)

| Archetype | Per Era | x10 Eras | Total |
|-----------|---------|----------|-------|
| Charger | ~5.4 KB | ~54 KB | 54 KB |
| Shooter | ~6.1 KB | ~61 KB | 61 KB |
| Flyer | ~6.9 KB | ~69 KB | 69 KB |
| Tank | ~14.9 KB | ~149 KB | 149 KB |
| Swarm | ~0.7 KB | ~7 KB | 7 KB |
| **Total** | | | **~340 KB** |

**Note**: Only 2 era sprite sets need to be in memory at once (current era + next era for seamless transition). Active memory usage is ~68 KB for enemy sprites.

---

## Appendix C: Era Variant Quick Reference Card

A condensed reference for designers and artists showing all 50 era-archetype combinations.

```
                STONE AGE    BRONZE AGE   IRON AGE     MEDIEVAL     RENAISSANCE
Charger         Wild Boar    Bronze       War Hound    Armored      Pikeman
                             Soldier                   Knight
Shooter         Spear        Slinger      Archer       Crossbowman  Musketeer
                Thrower
Flyer           Pterodactyl  Fire Hawk    War Eagle    Dragon       Ornithopter
Tank            Mammoth      War Chariot  War          Siege Golem  Cannon
                                          Elephant                  Wagon
Swarm           Rats         Scarab       Hornets      Bats         Clockwork
                             Beetles                                Mice

                INDUSTRIAL   MODERN       DIGITAL      SPACE        TRANSCENDENT
Charger         Steambot     Riot         Combat       Assault      Void Beast
                             Trooper      Drone        Mech
Shooter         Steam        Machine      Laser        Plasma       Particle
                Cannon       Gunner       Turret       Sentry       Emitter
Flyer           Zeppelin     Attack       Hover Bot    Fighter      Phase
                Drone        Helicopter                Pod          Wraith
Tank            Steam Tank   Battle       Heavy        Mech         Quantum
                             Tank         Mech         Walker       Titan
Swarm           Clockwork    Surveillance Nano         Repair       Void
                Spiders      Drones       Drones       Bots         Mites
```

---

## Appendix D: Encounter Composition Templates

Pre-designed encounter templates that the level generator can select from. Each template specifies archetype slots relative to the segment layout.

### Template 1: Gauntlet Run

```
Layout: Long horizontal segment (20+ tiles)
Budget: 8-12 points

[START] --- Swarm --- Charger --- [destructible wall] --- Shooter --- Charger --- [END]
                                                              ^
                                                         (elevated platform)

Spawn order: Swarm on entry, Charger at midpoint, Shooter behind destructible wall,
             Charger near exit.
Player experience: Mow through swarm, dodge charger, break wall, duel shooter,
                   dodge final charger.
```

### Template 2: Aerial Assault

```
Layout: Open vertical segment (10+ tiles high)
Budget: 6-10 points

         Flyer ~~~~                Flyer ~~~~
              ~~~~                      ~~~~

[START] --- --- --- --- [platform] --- --- --- [END]
                Charger                Swarm

Spawn order: Flyers hover above, Charger patrols ground, Swarm triggers on approach.
Player experience: Split attention between aerial dives and ground threats.
```

### Template 3: Heavy Gate

```
Layout: Medium horizontal segment (14+ tiles)
Budget: 10-14 points

                                          Shooter
                                        [platform]

[START] --- Swarm --- --- --- TANK >>>>>>>>>>>>>>> [EXIT blocked until Tank dies]

Spawn order: Tank advances from right, Swarm flanks from left, Shooter fires from above.
Player experience: Must defeat Tank to progress. Swarm and Shooter add pressure.
                   Tank destroys destructible terrain as it advances.
```

### Template 4: Ambush Box

```
Layout: Enclosed area with destructible walls (10x8 tiles)
Budget: 8-12 points

[destructible ceiling -- Flyer above]

[wall]  Swarm    Charger    Swarm  [wall]
[wall]                             [wall]

[START/EXIT at bottom]

Spawn order: All enemies activate simultaneously when player enters.
Player experience: Enclosed panic moment. Break walls to create escape routes.
                   Auto-fire must prioritize targets quickly.
```

### Template 5: Sniper Alley

```
Layout: Long corridor with elevated platforms (25+ tiles)
Budget: 8-10 points

Shooter        Shooter         Shooter
[plat]         [plat]          [plat]

[START] --- [cover] --- [cover] --- [cover] --- [END]
        Swarm               Charger

Spawn order: Shooters pre-placed on platforms, Swarm and Charger on ground.
Player experience: Use destructible cover to advance against ranged fire.
                   Ground enemies force player out of cover periodically.
```

---

*Document version: 2.0*
*Last updated: 2026-02-04*
*Replaces: Assessment 4.2 (biome-based enemy archetypes)*
*Compatible with: Era-based level generation pipeline, auto-fire weapon attachment system*
