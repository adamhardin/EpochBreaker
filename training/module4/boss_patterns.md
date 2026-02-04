# Era-Based Boss Encounter Specifications

## Overview

This document specifies 10 boss encounters, one per era, for the 16-bit side-scrolling mobile shooter (iOS). Each era spans 5 levels, with the boss appearing at the end of level 5 of that era (levels 5, 10, 15, 20, 25, 30, 35, 40, 45, 50). All bosses integrate the game's core destructible environment mechanic -- players must blast apart arena elements to expose weak points, create platforms, remove cover, or trigger environmental hazards.

**Core Game Concept:**
- Side-scrolling mobile shooter with auto-firing weapon attachments
- Player character moves and dodges; weapons fire automatically at enemies
- Destructible environments are a primary mechanic (walls, floors, ceilings, structures can be blasted apart)
- Boss arenas are camera-locked with arena-specific destructible elements

**Design Principles:**
- Every boss attack has a visual and/or audio tell with a minimum 18-frame (0.3s) warning
- Every attack pattern has a vulnerability window where the player can deal damage
- Destructible arena elements are central to every boss fight -- not optional
- Complexity escalates across eras: Era 1-3 have 2 phases, Era 4-7 have 3 phases, Era 8-9 have 3 phases, Era 10 has 4 phases
- All encounters are completable with base weapon attachment -- no upgrades required
- Boss HP scales to target encounter durations: 45-60s (early eras) to 120-150s (final era)

**Reference Values:**
- Player auto-fire DPS: 8 damage/s (base attachment, single target)
- Player weapon attachment slots: up to 3 equipped simultaneously
- Player movement speed: 6.0 tiles/s horizontal
- Player dash: 10.0 tiles/s, 12 frames duration, 60-frame cooldown
- Player max jump: 4.0 tiles height, 4.5 tiles horizontal distance
- Destructible tile HP: 1-5 hits depending on material (stone=3, metal=4, energy=5)
- 1 tile = 16x16 pixels
- Frame rate: 60 fps
- All frame counts at 60 fps unless otherwise noted

**Damage Notation:**
- Player base DPS: 8/s (auto-fire hits roughly every 7-8 frames for 1 damage each)
- Boss damage to player listed per hit
- Destructible element HP listed in hits (base weapon hits)
- "Vulnerability window" = boss takes full damage; outside these windows, boss has damage reduction or immunity

---

## Boss 1: Cave Beast (Era 1 -- Stone Age)

### Concept

A massive cave bear/beast (64x48 pixel sprite, 4x3 tiles) that charges across a cavern arena. Jagged rock pillars support stalactites overhead. The beast's thick hide makes it resistant to direct fire -- players must blast the rock pillars to drop stalactites on its head, stunning it and exposing a glowing weak point on its skull. This fight teaches the core lesson: use destruction to create opportunities.

**Target Duration:** 45-60 seconds | **Total HP:** 400 | **Phases:** 2

### Arena Layout

```
Width: 24 tiles | Height: 14 tiles (camera-locked)
D = Destructible rock pillar (3 HP each, 4 total)
S = Stalactite (drops when pillar below is destroyed)
~ = Destructible rock wall (2 HP)
P = Platform (indestructible)
. = Open air
# = Solid ground/wall (indestructible)

##S###S####S###S##########
#.........................#
#.........................#
#.........................#   Ceiling zone
#.........................#
#..D......D....D......D...#   Row 9: Stalactites hang above pillars
#..D......D....D......D...#   Row 8: Pillar tops
#..D......D....D......D...#   Row 7: Pillar middles
#.........................#   Row 6: Open
#...PPP........PPP........#   Row 5: Side platforms
#.........................#   Row 4: Open
#~~....................~~..#   Row 3: Destructible wall alcoves (left/right)
#~~....................~~..#   Row 2: Destructible wall alcoves
##########################   Row 1: Floor
##########################   Row 0: Sub-floor (solid)

Pillar positions (tile columns): 3, 10, 15, 22
Stalactite drop zones: 3-tile radius below each stalactite
Platform positions: tiles 4-6 (left), tiles 16-18 (right), both at row 5
Destructible wall alcoves: tiles 1-2 (left), tiles 22-23 (right) -- blast open for safe zones
Boss patrol zone: floor level, full width
Player start: tile 2, row 1
```

### Phase 1: "Territorial Rage" (100% to 50% HP -- 200 HP)

The Cave Beast patrols the arena floor, charging at the player and swiping with massive claws. Direct weapon fire deals only 25% damage (2 DPS effective). Players must destroy rock pillars to drop stalactites.

**Stalactite Mechanic:**
- Each pillar has 3 HP (3 base weapon hits to destroy)
- When a pillar is destroyed, the stalactite above drops after 6 frames
- Stalactite fall speed: 12.0 tiles/s
- Stalactite damage zone: 3 tiles wide at impact point
- If the Cave Beast is in the drop zone: 50 damage + 120-frame stun (2.0s)
- During stun: glowing skull weak point exposed, takes 3x damage (24 DPS effective)
- Stalactite also creates rubble pile (1 tile high) that acts as temporary terrain for 300 frames (5.0s)
- 4 pillars total in Phase 1 = 4 possible stalactite drops = 200 potential stalactite damage (enough for full phase)

**Attack Pattern -- Boss AI State Machine:**

```
PATROL (default state)
  Movement: 3.0 tiles/s, paces back and forth across arena floor
  Direction: reverses at walls, faces player
  Duration: continues until trigger condition met
  -> CHARGE (player on floor within 12 tiles line of sight)
  -> ROAR (player on elevated platform for 120+ frames)

CHARGE
  Tell: 24 frames (0.4s) -- beast lowers head, eyes flash red, dust kicks up from feet
  Active: charges toward player position at 8.0 tiles/s
  Duration: charges until hitting a wall or traveling 16 tiles
  Hitbox: 4x3 tiles (full body), damage 2 on contact
  Wall impact: if beast hits arena wall, 60-frame stun (1.0s), takes normal damage
  Recovery: 36 frames (0.6s) -- shakes head, repositions
  Vulnerability: YES during recovery (normal damage only, skull not exposed)
  Cooldown: 90 frames before next charge

ROAR
  Tell: 18 frames (0.3s) -- beast rears up on hind legs
  Active: roar creates shockwave along floor, 2 tiles high
  Shockwave speed: 10.0 tiles/s both directions from beast
  Damage: 1 (shockwave)
  Duration: shockwave travels full arena width
  Dodge: jump or stand on platform
  Recovery: 30 frames (0.5s)
  Vulnerability: YES during roar and recovery (normal damage, no skull exposure)

STUNNED (triggered by stalactite hit)
  Duration: 120 frames (2.0s)
  Beast collapses, skull weak point glows bright
  Vulnerability: YES, 3x damage multiplier on skull
  Auto-fire DPS during stun: 24 effective (3x multiplier)
  Damage per stun window: ~48 damage
  After stun: beast roars (cosmetic, 18 frames) then returns to PATROL
```

**Destruction Strategy:**
- Lure the beast near a pillar via positioning
- Blast the pillar (3 hits) while dodging attacks
- Stalactite drops on beast = 50 damage + stun
- Attack skull during 2.0s stun for ~48 additional damage
- Total per stalactite cycle: ~98 damage
- Phase requires 2-3 stalactite hits to complete

**Destructible Wall Alcoves:**
- Left and right wall alcoves (2 HP each) can be blasted open
- Alcoves provide safe zones where beast cannot reach (too large to fit)
- Player can shelter in alcoves to recover or reposition
- Teaches that destruction creates tactical options, not just damage

### Phase 2: "Cornered Animal" (50% to 0% HP -- 200 HP)

**Transition:** 60 frames. Beast roars, slams ground, cracks appear in remaining pillars. Two new stalactites grow from ceiling (positions: tiles 7 and 19). Arena floor cracks appear as cosmetic warning.

**Changes from Phase 1:**
- Patrol speed: 4.0 tiles/s (faster)
- Charge speed: 10.0 tiles/s
- Charge tell reduced: 18 frames (0.3s)
- New attack: LEAP SLAM
- Two new destructible stalactite anchors appear at tiles 7 and 19 (2 HP each, smaller pillars)
- Direct damage resistance reduced: takes 50% damage from weapons (4 DPS effective) instead of 25%

```
LEAP SLAM (new attack)
  Trigger: player on platform or player beyond 8 tiles
  Tell: 24 frames (0.4s) -- beast crouches, muscles tense, ground cracks beneath feet
  Active: leaps in arc toward player position, lands after 18 frames
  Landing zone: 4 tiles wide centered on target position
  Damage: 3 (direct hit), 2 (shockwave, 3 tiles from landing point)
  Shockwave: ground-level, 2 tiles high, must be jumped
  Recovery: 48 frames (0.8s) -- stuck in landing crater
  Vulnerability: YES during recovery (normal damage, skull briefly exposed for 1.5x)
  Cooldown: 120 frames
```

**Phase 2 Stalactite Damage:** 50 damage + 90-frame stun (1.5s, shorter than Phase 1)
- DPS during stun: 24 effective, damage per stun: ~36
- Total per stalactite cycle: ~86 damage
- Phase requires 2-3 stalactite hits plus supplemental weapon fire

### Loot on Defeat

| Reward | Value |
|--------|-------|
| Coins | 500 |
| Weapon Part | Stone Spreader Barrel (unlocks spread-shot attachment variant) |
| Era Completion Bonus | 1,000 XP |
| Achievement | "Cavern Conqueror" |
| Health Restore | Full HP |

---

## Boss 2: Bronze Colossus (Era 2 -- Bronze Age)

### Concept

An animated bronze statue (64x64 pixel sprite, 4x4 tiles) standing in a temple courtyard. The Colossus is covered in thick bronze armor plates that must be individually destroyed to expose its glowing molten core. Each destroyed plate reveals a vulnerable segment. The arena contains destructible bronze pillars and offering pedestals that can be blasted for tactical advantage.

**Target Duration:** 50-65 seconds | **Total HP:** 550 | **Phases:** 2

### Arena Layout

```
Width: 26 tiles | Height: 14 tiles (camera-locked)
A = Bronze armor plate (destructible, on boss body -- see below)
B = Destructible bronze pillar (4 HP each)
O = Destructible offering pedestal (2 HP, drops health shard)
P = Stone platform (indestructible)
# = Solid wall/floor

##########################
#........................#
#........................#   Row 12-13: Upper space
#....PPP......PPP........#   Row 11: High platforms (tiles 5-7, 15-17)
#........................#
#..B..............B......#   Row 9: Bronze pillar tops
#..B.....[BOSS]...B......#   Row 8: Boss center (tiles 11-14, rows 6-9)
#..B..............B......#   Row 7: Bronze pillar bases
#........................#   Row 6: Open
#.O..PPP......PPP....O..#   Row 5: Mid platforms (tiles 5-7, 15-17) + offering pedestals
#........................#   Row 4: Open
#...PPPP........PPPP.....#   Row 3: Low platforms (tiles 4-7, 16-19)
#........................#   Row 2: Open
##########################   Row 1: Floor
##########################   Row 0: Sub-floor

Boss position: center (tiles 11-14, rows 6-9), stationary
Bronze pillars: tiles 3 and 21, rows 7-9 (3 tiles tall, 4 HP each)
Offering pedestals: tiles 2 and 23, row 5 (2 HP, drop health shard on destroy)
Player start: tile 2, row 1
```

**Bronze Armor Plate System:**
The Colossus has 6 armor plates mapped to body regions:

```
Armor Plate Layout (on boss body):
     [HEAD PLATE]        -- 8 HP, top of sprite
   [L.ARM] [R.ARM]       -- 6 HP each, left and right sides
     [CHEST PLATE]       -- 10 HP, center (covers core)
   [L.LEG] [R.LEG]       -- 5 HP each, lower left and right

Destruction order freedom: any plate can be targeted in any order
Each destroyed plate reveals a vulnerable segment (takes full damage)
Core exposure: chest plate must be destroyed to access the core (3x damage zone)
Partial exposure: arm/leg/head plates give 1.5x damage segments
```

### Phase 1: "Bronze Sentinel" (100% to 50% HP -- 275 HP)

The Colossus stands at center, attacking with sweeping arms and energy blasts from its eyes. It cannot move but rotates to face the player. Armor plates deflect all damage -- player must focus fire on individual plates to strip them.

**Attack Pattern -- Repeating 360-frame (6.0s) cycle:**

```
Frames 0-60 (1.0s): IDLE
  Colossus rotates to track player
  Player can fire at armor plates freely
  Vulnerability: PLATES ONLY (damage plates, not HP pool)

Frames 60-78 (0.3s): ARM SWEEP TELL
  Active arm glows orange, pulls back
  Attack comes from the arm nearest to the player

Frames 78-108 (0.5s): ARM SWEEP ACTIVE
  Sweeping arm hitbox: 5 tiles horizontal, 3 tiles vertical
  Direction: toward player side
  Damage: 2
  If arm plate is destroyed: sweep is faster (3 frames less startup)
    but arm segment takes full damage during sweep animation
  Dodge: jump over or dash through gap if opposite arm plate is destroyed

Frames 108-138 (0.5s): RECOVERY
  Arms return to default
  All exposed segments vulnerable (full damage to HP pool)
  Vulnerability: YES (1.5x on exposed segments, 3x on exposed core)

Frames 138-156 (0.3s): EYE BEAM TELL
  Eyes glow, targeting lines appear on floor at player position

Frames 156-216 (1.0s): EYE BEAM ACTIVE
  Twin beams sweep across floor from boss position
  Beam width: 1 tile each, sweep speed: 6.0 tiles/s
  Sweep pattern: outward from center, then back inward
  Damage: 2 per beam per hit (can be hit by both)
  Dodge: jump, use platforms, or stand in gap between beams during inward sweep
  Vulnerability: HEAD exposed during beam (if head plate destroyed, 1.5x damage)

Frames 216-264 (0.8s): STOMP TELL
  Colossus lifts one foot, ground rumbles

Frames 264-288 (0.4s): STOMP ACTIVE
  Ground shockwave travels along floor in both directions
  Shockwave speed: 8.0 tiles/s
  Shockwave height: 2 tiles
  Damage: 2
  Dodge: jump or stand on platform
  If leg plate is destroyed: stomp creates rubble (destructible terrain, 2 HP)
    that can be used as stepping stone

Frames 288-360 (1.2s): COOLDOWN
  Colossus vents steam, all exposed segments vulnerable
  Vulnerability: YES (full damage window)
  Bronze pillars can be destroyed during this window for strategic reasons:
    - Pillar debris damages Colossus for 30 damage if pillar is adjacent
    - Pillar destruction opens sight lines for better auto-fire angles
```

**Plate Destruction Strategy:**
- Focus fire on one plate at a time
- Recommended order: one arm (6 HP) -> chest (10 HP) -> core exposed
- Each destroyed plate: small explosion visual, bronze chunks fly off, vulnerability flash
- With chest plate destroyed, core takes 3x damage during vulnerability windows
- Phase 1 target: destroy 2-3 plates, deal ~275 damage through exposed segments

### Phase 2: "Awakened Idol" (50% to 0% HP -- 275 HP)

**Transition:** 48 frames. Remaining armor plates glow red-hot. Colossus breaks free from pedestal, becomes mobile. Moves slowly along floor.

**Changes from Phase 1:**
- Colossus now moves: 2.0 tiles/s patrol speed along floor
- Remaining armor plates take 2x damage (easier to strip)
- All attack tells reduced by 6 frames
- New attack: BRONZE RAIN
- Any already-destroyed plates remain destroyed

```
BRONZE RAIN (new attack)
  Tell: 30 frames (0.5s) -- Colossus raises both arms, chunks of bronze
    break from ceiling
  Active: 6 bronze chunks rain down at random positions across arena
  Chunk fall speed: 8.0 tiles/s
  Each chunk: 1x1 tile, damage 2
  Chunks leave rubble on floor for 180 frames (destructible, 1 HP)
  Rubble provides temporary platforms but also blocks movement
  Dodge: watch shadows on floor (shadow appears 18 frames before impact)
  Vulnerability: YES during rain (Colossus arms raised, core/segments exposed)

ARM SWEEP changes:
  Now sweeps full 180 degrees if both arm plates destroyed
  Creates wider danger zone but also longer recovery (42 frames)

EYE BEAM changes:
  Beams now track player slowly (1.5 tiles/s tracking)
  Must keep moving to avoid
```

**Destruction Integration:**
- Blast remaining plates quickly with Phase 2's 2x plate damage
- Use bronze pillar rubble for positioning advantage
- Offering pedestals drop health shards (1 HP each) when destroyed -- save for Phase 2
- Core exposed = 3x damage, combined with vulnerability windows = fast kill potential

### Loot on Defeat

| Reward | Value |
|--------|-------|
| Coins | 750 |
| Weapon Part | Bronze Piercer Module (shots penetrate one additional target) |
| Era Completion Bonus | 1,500 XP |
| Achievement | "Idol Breaker" |
| Health Restore | Full HP |

---

## Boss 3: Iron Warlord (Era 3 -- Iron Age)

### Concept

A heavily armored warlord (48x64 pixel sprite, 3x4 tiles) commanding from behind a series of iron fortification walls. The arena is divided into segments by destructible iron walls -- the player must blast through walls to advance toward the Warlord while dodging his attacks. The Warlord retreats behind the next wall when the current one falls. This fight teaches aggressive destruction under pressure.

**Target Duration:** 55-70 seconds | **Total HP:** 700 | **Phases:** 2

### Arena Layout

```
Width: 28 tiles | Height: 14 tiles (camera-locked)
W = Destructible iron wall segment (5 HP per segment)
G = Destructible gate (3 HP, weaker than walls)
T = Destructible tower (4 HP, collapses to create platform)
P = Platform (indestructible)
^ = Arrow slit (Warlord fires through these)
# = Solid boundary

############################
#..........................#
#..........................#   Row 12-13: Upper space
#....T.....T.....T.........#   Row 11: Tower tops
#....T.....T.....T.........#   Row 10: Tower bodies
#....T.....T.....T.........#   Row 9: Tower bodies
#.WWWWW.WWWWW.WWWWW...[WL]#   Row 8: Fortification walls (3 walls)
#.WW^WW.WW^WW.WW^WW...[WL]#   Row 7: Arrow slits in walls
#.WWGWW.WWGWW.WWGWW...[WL]#   Row 6: Gates in walls (weaker)
#..........................#   Row 5: Open
#PPP.....PPP.....PPP.......#   Row 4: Low platforms between walls
#..........................#   Row 3: Open
#..........................#   Row 2: Open
############################   Row 1: Floor
############################   Row 0: Sub-floor

Wall 1 (outer): tiles 2-7, rows 6-8 (gate at tile 4)
Wall 2 (middle): tiles 9-14, rows 6-8 (gate at tile 11)
Wall 3 (inner): tiles 16-21, rows 6-8 (gate at tile 18)
Warlord position: starts behind Wall 3 (tiles 23-25), retreats to tile 26
Towers: tiles 5, 12, 19 (rows 9-11), collapse creates rubble platforms at row 8
Player start: tile 2, row 1
```

### Phase 1: "Iron Fortress" (100% to 50% HP -- 350 HP)

The Warlord stands behind the innermost wall and commands ranged attacks through arrow slits and over walls. Player must breach walls to get direct line of fire on the Warlord.

**Wall Breach Mechanic:**
- Each wall segment: 5 HP (5 base weapon hits)
- Gates: 3 HP (weaker, intended breach points)
- Arrow slits: 1-tile gaps the Warlord fires through (player can also fire through)
- Breaching a wall section: explosion visual, debris flies outward (1 damage to anything in 2-tile radius)
- Towers: 4 HP, when destroyed they collapse into rubble platforms at wall height
- Collapsing tower deals 40 damage to Warlord if he is adjacent

**Wall Damage to Warlord:**
- Wall 1 breach: opens line of fire, player deals 50% damage at range (auto-fire loses damage over distance)
- Wall 2 breach: closer range, player deals 75% damage
- Wall 3 breach: full range, 100% damage
- Warlord takes 25 damage per wall section destroyed (debris hits him)

**Attack Pattern -- Warlord attacks from behind fortifications:**

```
JAVELIN VOLLEY
  Tell: 24 frames (0.4s) -- Warlord raises arm, javelins appear
  Active: throws 3 javelins in arc over walls toward player
  Javelin speed: 6.0 tiles/s
  Javelins arc over walls (cannot be blocked by fortifications)
  Damage: 2 each
  Landing zones: telegraphed by shadow markers on floor (18 frames warning)
  Recovery: 36 frames (0.6s) -- Warlord lowers guard
  Vulnerability: YES during recovery (if line of fire exists)
  Cooldown: 120 frames

ARROW SLIT BARRAGE
  Tell: 18 frames (0.3s) -- arrow slit glows red
  Active: rapid-fire bolts through arrow slit, 5 bolts over 30 frames
  Bolt speed: 10.0 tiles/s
  Each bolt: 1 damage
  Direction: horizontal through slit toward player
  Dodge: move above or below slit height, or stand behind intact wall segment
  Vulnerability: NO (Warlord protected behind wall)

SHIELD BASH (only when player is within 3 tiles, wall breached)
  Tell: 18 frames (0.3s) -- Warlord raises shield, charges forward
  Active: rushes 4 tiles forward through breach
  Damage: 3
  Recovery: 42 frames (0.7s) -- exposed, shield lowered
  Vulnerability: YES (full damage, best window in Phase 1)
  After recovery: Warlord retreats behind next wall
```

**Progression Through Fortifications:**
1. Blast gate in Wall 1 (3 HP) -- opens initial path
2. Fire through breach at Wall 2 while dodging javelins
3. Blast gate in Wall 2 -- advance
4. Destroy tower near Wall 3 -- collapses onto wall, dealing 40 damage and creating gap
5. Fire directly at Warlord through final breach

### Phase 2: "Last Stand" (50% to 0% HP -- 350 HP)

**Transition:** 48 frames. Warlord destroys remaining walls himself in rage (explosion, debris flies everywhere -- player must dodge). Arena becomes open with rubble scattered on floor. Warlord draws a second weapon and becomes mobile.

**Arena Changes:**
- All walls destroyed (open arena)
- Rubble piles scattered across floor (1-2 tiles high, destructible, 2 HP each)
- Rubble provides temporary cover for both player and Warlord
- Warlord now mobile: 3.5 tiles/s walk speed, actively pursues player

```
DUAL BLADE COMBO
  Tell: 18 frames (0.3s) -- both blades glow, stance widens
  Active: 3-hit combo, each swing 8 frames apart
  Range: 3 tiles in front
  Damage: 2 per hit (6 total if all connect)
  Recovery: 36 frames (0.6s)
  Vulnerability: YES during recovery

JAVELIN THROW (modified)
  Now throws single aimed javelin at high speed
  Speed: 12.0 tiles/s (much faster)
  Damage: 3
  Javelin embeds in rubble/walls and can be destroyed (1 HP) to prevent
    area denial
  Tell: 18 frames
  Cooldown: 90 frames

WAR CRY
  Tell: 24 frames (0.4s) -- Warlord plants feet, inhales
  Active: radial shockwave, 5-tile radius
  Damage: 2
  Height: 3 tiles (must dash away, cannot simply jump over)
  Recovery: 48 frames (0.8s) -- Warlord exhausted, full vulnerability
  Vulnerability: YES (best Phase 2 damage window)
  Cooldown: 240 frames

RUBBLE TACTICS
  Warlord kicks rubble at player if within 2 tiles of rubble pile
  Tell: 12 frames (fast, 0.2s)
  Rubble projectile speed: 8.0 tiles/s
  Damage: 1
  Destroys the rubble pile used (removes cover)
  Vulnerability: NO (quick attack)
```

**Destruction in Phase 2:**
- Player can destroy rubble piles to deny Warlord cover and kick attacks
- Warlord uses rubble for cover when at range -- destroying it forces him into the open
- Strategic destruction: leave some rubble for player cover, destroy Warlord's cover
- Remaining tower bases can be shot to topple debris on Warlord (30 damage if he is under it)

### Loot on Defeat

| Reward | Value |
|--------|-------|
| Coins | 1,000 |
| Weapon Part | Iron Repeater Core (increases auto-fire rate by 15%) |
| Era Completion Bonus | 2,000 XP |
| Achievement | "Siege Breaker" |
| Health Restore | Full HP |

---

## Boss 4: Siege Dragon (Era 4 -- Medieval)

### Concept

A dragon (80x48 pixel sprite, 5x3 tiles) perched atop a crumbling castle wall. The dragon repositions along the wall as sections are destroyed. The arena is a castle courtyard with destructible battlements, towers, and wall segments. The player fights from below, blasting castle segments to collapse the dragon's perches and force it to lower positions where weapons can hit its underbelly weak point.

**Target Duration:** 65-80 seconds | **Total HP:** 1,000 | **Phases:** 3

### Arena Layout

```
Width: 30 tiles | Height: 16 tiles (camera-locked)
C = Castle wall (destructible, 5 HP per segment)
B = Battlement (destructible, 3 HP, provides dragon perch)
T = Tower (destructible, 6 HP, tall structure)
R = Rubble zone (created when structures collapse)
P = Indestructible platform
D = Dragon (moves between perch points)

##############################
#............................#
#[D perch 4].[D perch 5].....#   Row 14: Highest perch points (towers)
#..TT.............TT.........#   Row 13: Tower tops
#..TT.............TT.........#   Row 12: Tower bodies
#BBCCCCCCBBBCCCCCCBB.........#   Row 11: Battlements + castle wall (dragon perch level)
#CCCCCCCCCCCCCCCCCCCC........#   Row 10: Castle wall upper
#CCCCCCCCCCCCCCCCCCCC........#   Row 9: Castle wall lower
#..[perch1].[perch2].[perch3]#   Row 8: Mid-height perch points (wall top)
#..........................PP#   Row 7: Right platform
#PP..........................#   Row 6: Left platform
#....PPPP........PPPP........#   Row 5: Mid platforms
#..........................PP#   Row 4: Right low platform
#PP..........................#   Row 3: Left low platform
##############################   Row 1-2: Floor + moat edges
##############################   Row 0: Sub-floor

Dragon perch points (left to right):
  Perch 1: tiles 5-9, row 11 (on battlement)
  Perch 2: tiles 12-16, row 11 (on battlement)
  Perch 3: tiles 19-23, row 11 (on battlement)
  Perch 4: tiles 3-4, row 13 (on tower top)
  Perch 5: tiles 17-18, row 13 (on tower top)

Player start: tile 4, row 2
```

### Phase 1: "Dragon's Roost" (100% to 65% HP -- 350 HP)

The dragon perches on the highest points (battlements) and attacks with fire breath and tail sweeps. Direct fire at the dragon deals only 25% damage because of the angle and armor -- underbelly is not exposed. Players must destroy the battlement the dragon is perched on to make it fall to a lower position.

**Perch Destruction Mechanic:**
- Each battlement section: 3 HP (3 hits)
- Destroying a perch: dragon falls, takes 40 damage from fall
- Dragon is stunned on the ground for 90 frames (1.5s) after falling
- During stun: underbelly exposed, takes 2x damage
- After stun: dragon flies to next available perch
- When all battlements destroyed, Phase 2 triggers

**Attack Pattern:**

```
FIRE BREATH
  Tell: 24 frames (0.4s) -- dragon inhales, throat glows orange
  Active: sweeping fire breath across lower arena
  Sweep direction: left-to-right or right-to-left (alternates)
  Sweep speed: 6.0 tiles/s
  Fire hitbox: 3 tiles wide, extends from dragon down to floor
  Damage: 3
  Duration: 48 frames (0.8s)
  Dodge: move ahead of sweep direction, use platforms to get above fire line
  Destructible element interaction: fire destroys destructible rubble and
    wooden elements in path
  Vulnerability: NO (dragon is attacking, underbelly hidden)
  Cooldown: 120 frames

TAIL SWEEP
  Tell: 18 frames (0.3s) -- tail raises, scales rattle
  Active: tail swings in horizontal arc at perch height
  Range: 6 tiles from dragon, at perch row
  Damage: 2
  Dodge: stay below perch height or beyond range
  Recovery: 30 frames (0.5s)
  Vulnerability: YES during recovery (body exposed, but still 25% damage from angle)

DIVE BOMB
  Trigger: player directly below dragon for 90+ frames
  Tell: 30 frames (0.5s) -- dragon screeches, lifts wings
  Active: dives to floor level at player position
  Dive speed: 14.0 tiles/s
  Damage: 4 (direct), 2 (shockwave on landing, 4 tiles wide)
  Recovery: 60 frames (1.0s) -- on floor, underbelly briefly exposed
  Vulnerability: YES, 2x damage on underbelly during recovery
  After recovery: flies back to perch
  Cooldown: 180 frames
```

### Phase 2: "Crumbling Fortress" (65% to 30% HP -- 350 HP)

**Transition:** 36 frames. All remaining battlements collapse. Dragon lands on castle wall mid-section (row 9-10). Castle wall segments now destructible and the dragon clings to them.

**Arena Changes:**
- Dragon perches on castle wall face (rows 9-10)
- Castle wall segments can be destroyed (5 HP each) -- destroying the segment under the dragon forces it to reposition
- Wall destruction creates rubble platforms below (row 6-7 area)
- Dragon is now at mid-height -- easier to hit but more aggressive

**New/Modified Attacks:**

```
FIRE BREATH (modified)
  Now fires in a cone downward from wall position
  Cone: 60-degree spread, 8-tile range
  Creates fire pools on floor (2 damage/s, 120 frames duration)
  Vulnerability: NO

CLAW RAKE
  Tell: 18 frames (0.3s) -- one claw extends from wall
  Active: rakes downward, 4 tiles vertical range
  Damage: 3
  Recovery: 24 frames (0.4s)
  Vulnerability: YES during recovery (wall-clinging position exposes side)

WALL COLLAPSE TRIGGER
  Tell: 24 frames -- dragon slams wall, cracks appear in a 4-tile section
  Active: wall section collapses after 18 frames, raining debris
  Debris: 4-6 chunks, each 1 damage, random positions below
  Creates new rubble platforms
  Strategic: player can also shoot the cracked section to control where it falls
  Vulnerability: YES briefly during slam (dragon braces, 12 frames of exposure)
```

**Destruction Strategy:**
- Shoot wall segments adjacent to dragon to force repositioning
- Each forced reposition: dragon takes 20 damage from impact
- Collapsing wall sections create rubble platforms for reaching higher positions
- Use rubble platforms to get closer and deal full damage during vulnerability windows

### Phase 3: "Grounded Fury" (30% to 0% HP -- 300 HP)

**Transition:** 48 frames. Remaining castle wall collapses entirely. Dragon crashes to arena floor. Wings damaged -- cannot fly. Arena is now open with rubble scattered everywhere.

**Arena Changes:**
- Castle wall fully collapsed -- rubble fills floor area (varied heights 1-3 tiles)
- Some rubble is destructible (2 HP), some is permanent terrain
- Dragon on floor, mobile: 4.0 tiles/s
- Dragon now fully exposed -- all attacks deal 100% damage
- Dragon is more aggressive and desperate

```
FIRE STREAM (replaces Fire Breath)
  Tell: 18 frames (0.3s) -- shorter tell, faster attack
  Active: continuous fire stream aimed at player, tracks slowly
  Track speed: 3.0 tiles/s
  Duration: 60 frames (1.0s)
  Damage: 3 per hit
  Fire destroys destructible rubble in path (clears cover)
  Vulnerability: NO during stream

FRENZY CHARGE
  Tell: 24 frames (0.4s) -- dragon lowers head, digs claws in
  Active: charges across arena, destroying all rubble in path
  Speed: 10.0 tiles/s
  Damage: 4 (direct hit)
  Clears a path through rubble (permanent terrain removal)
  Recovery: 54 frames (0.9s) -- slides to stop, panting, fully exposed
  Vulnerability: YES, best Phase 3 damage window

DEATH THROES (at 10% HP / 100 HP)
  Dragon roars, fire erupts from cracks in floor
  Fire geysers: 6 random positions, each 2 tiles wide, 18-frame warning
  Damage: 3 per geyser
  Duration: 180 frames (3.0s)
  Dragon is stationary during this, taking 1.5x damage
  Vulnerability: YES (high risk, high reward final push)
```

### Loot on Defeat

| Reward | Value |
|--------|-------|
| Coins | 1,500 |
| Weapon Part | Dragon Fang Barrel (adds fire damage over time: 1 damage/s for 3s) |
| Era Completion Bonus | 3,000 XP |
| Achievement | "Dragon Slayer" |
| Health Restore | Full HP |
| Bonus | Unlocks Medieval weapon skin set |

---

## Boss 5: Clockwork Titan (Era 5 -- Renaissance)

### Concept

A giant clockwork automaton (80x80 pixel sprite, 5x5 tiles) in a workshop arena filled with exposed gears, pistons, and mechanisms. The Titan is powered by visible gear assemblies that the player must destroy to jam its systems. Each destroyed gear housing disables one of the Titan's attack systems and slows its movements. The arena itself has clockwork elements -- moving platforms on gear tracks, rotating hazards, and pressurized steam vents.

**Target Duration:** 70-85 seconds | **Total HP:** 1,200 | **Phases:** 3

### Arena Layout

```
Width: 30 tiles | Height: 16 tiles (camera-locked)
G = Destructible gear housing (6 HP each, 4 total on Titan body)
M = Moving platform (on gear track, moves when gear intact)
V = Steam vent (destructible, 3 HP, shoots steam periodically)
C = Clockwork wall panel (destructible, 4 HP)
# = Solid boundary

##############################
#............GGGG............#   Row 15: Upper gear housing (Titan shoulders)
#............................#
#..M>>>>>>>>>>>>>>>>>>M.....#   Row 13: Moving platform track (upper)
#............................#
#..V.....V..[TITAN]..V...V..#   Row 11: Titan center + steam vents
#........GG..body..GG.......#   Row 10: Side gear housings
#............................#   Row 9: Open
#..M>>>>>>>>>>>>>>>>>>M.....#   Row 8: Moving platform track (lower)
#..CCCC..............CCCC...#   Row 7: Clockwork wall panels
#..CCCC..............CCCC...#   Row 6: Clockwork wall panels
#..........................PP#   Row 5: Right platform
#PP..........................#   Row 4: Left platform
#....PPPPPP....PPPPPP........#   Row 3: Floor platforms
##############################   Row 1-2: Floor
##############################   Row 0: Sub-floor

Titan position: center (tiles 13-17, rows 9-14), stationary in Phase 1-2
Gear housings:
  G1 (Left Shoulder): tiles 13-14, row 15 -- controls left arm
  G2 (Right Shoulder): tiles 17-18, row 15 -- controls right arm
  G3 (Left Hip): tiles 12-13, row 10 -- controls left leg/stomp
  G4 (Right Hip): tiles 18-19, row 10 -- controls right leg/stomp
Steam vents: tiles 3, 10, 20, 27 at row 11 (3 HP each)
Moving platforms: track at rows 8 and 13 (move at 2.0 tiles/s when gears intact)
Clockwork wall panels: tiles 3-6 and 21-24, rows 6-7 (4 HP, blast for flanking routes)
Player start: tile 3, row 2
```

### Phase 1: "Full Operation" (100% to 65% HP -- 420 HP)

All gear housings intact. Titan operates at full capacity with all attack systems online. Moving platforms cycle on tracks. Steam vents fire periodically. Player must begin destroying gear housings while surviving the full attack suite.

**Gear Housing Destruction Effects:**
- G1 (Left Shoulder, 6 HP): disables LEFT ARM SLAM, Titan's left attacks become 30% slower
- G2 (Right Shoulder, 6 HP): disables GEAR LAUNCH, Titan's projectile attack offline
- G3 (Left Hip, 6 HP): disables STOMP QUAKE left-side shockwave
- G4 (Right Hip, 6 HP): disables STOMP QUAKE right-side shockwave
- Each destroyed housing: 30 damage to Titan + 60-frame jam stun (1.0s)
- During jam stun: all systems halt, Titan takes 2x damage

**Attack Pattern -- Multiple systems operate semi-independently:**

```
LEFT ARM SLAM (controlled by G1)
  Cycle: every 180 frames (3.0s)
  Tell: 24 frames (0.4s) -- left arm raises, gears whir loudly
  Active: slams floor on left side of arena (tiles 2-12)
  Damage: 3
  Shockwave: 3 tiles from impact, 2 tiles high
  Recovery: 30 frames (0.5s)
  Disabled when G1 destroyed

RIGHT ARM SLAM (controlled by G2 partially)
  Cycle: every 180 frames (3.0s), offset 90 frames from left arm
  Tell: 24 frames (0.4s) -- right arm raises
  Active: slams floor on right side of arena (tiles 18-28)
  Damage: 3
  Recovery: 30 frames (0.5s)
  Note: right arm slam persists even when G2 destroyed (only GEAR LAUNCH disabled)

GEAR LAUNCH (controlled by G2)
  Cycle: every 240 frames (4.0s)
  Tell: 18 frames (0.3s) -- chest panel opens, gear spins visibly
  Active: launches 3 spinning gear projectiles at player
  Gear speed: 7.0 tiles/s
  Gear size: 1x1 tile each
  Pattern: spread (-20, 0, +20 degrees)
  Damage: 2 each
  Gears bounce off walls once before despawning
  Disabled when G2 destroyed

STOMP QUAKE (controlled by G3 and G4)
  Cycle: every 300 frames (5.0s)
  Tell: 30 frames (0.5s) -- Titan crouches, floor cracks appear
  Active: both feet stomp, shockwaves travel left (G3) and right (G4)
  Shockwave speed: 8.0 tiles/s
  Shockwave height: 2 tiles, must jump to dodge
  Damage: 2 per shockwave
  Destroying G3: eliminates leftward shockwave
  Destroying G4: eliminates rightward shockwave
  Both destroyed: stomp still occurs but no shockwave (stomp damage only in 2-tile radius)

STEAM VENT BURST (environmental, not Titan-controlled)
  Cycle: every 150 frames (2.5s), staggered per vent
  Tell: 18 frames (0.3s) -- vent hisses, steam wisps appear
  Active: vertical steam column, 4 tiles high, 1 tile wide
  Damage: 1
  Duration: 30 frames (0.5s)
  Destroying a vent (3 HP): permanently removes that hazard
```

**Vulnerability:**
- Titan's core (center chest behind gear assemblies) is always partially exposed
- Normal damage to core: 50% (4 DPS effective)
- During gear jam stun: 100% damage, 2x multiplier (16 DPS effective)
- During combined recovery windows (both arms just slammed): 75% damage

### Phase 2: "Failing Gears" (65% to 30% HP -- 420 HP)

**Transition:** 36 frames. Titan sparks and shudders. Any remaining gear housings crack (reduced to 3 HP). Moving platforms malfunction -- they stutter and change direction randomly. New attack added.

**Changes:**
- Remaining gear housings weakened to 3 HP
- Moving platforms become erratic (change direction every 60-120 frames randomly)
- All attack cycles shortened by 30% (faster attacks)
- Titan core exposure increased: normal damage = 75%
- New attack: CLOCKWORK BARRAGE

```
CLOCKWORK BARRAGE (new)
  Tell: 30 frames (0.5s) -- chest opens wide, internal mechanisms visible
  Active: rapid-fire stream of small gears, 12 gears over 60 frames
  Each gear: 1x1 tile, 1 damage, speed 8.0 tiles/s
  Pattern: aimed at player with slight spread (+/- 10 degrees random)
  Duration: 60 frames (1.0s)
  Core fully visible during barrage
  Vulnerability: YES (core exposed, normal damage)
  Dodge: continuous lateral movement or use platforms to break line of sight

Clockwork wall panels become relevant:
  Blast panels (4 HP) to open flanking routes
  Flanking routes allow hitting Titan from sides where remaining gear
    housings are more exposed
  Panels regenerate after 300 frames (5.0s) -- Titan's repair systems
```

### Phase 3: "Overclock Meltdown" (30% to 0% HP -- 360 HP)

**Transition:** 60 frames. Titan overclocks -- all remaining systems run at maximum. Visual: sparks fly from every joint, gears spin at visible high speed, red warning lights on body. Titan breaks free and becomes mobile (slowly).

**Arena Changes:**
- Titan now mobile: 1.5 tiles/s (slow but relentless)
- All gear housings destroyed (if any remained, they auto-destroy in transition, dealing 30 damage each)
- Moving platforms locked in place (gears destroyed)
- Steam vents explode (if any remained, they deal area damage then go offline)
- Core fully exposed: 100% damage at all times
- Titan's attacks are faster but it is visibly breaking down

```
OVERCLOCK FRENZY
  Titan attack speed increased 50% (all cooldowns halved)
  All tells reduced by 6 frames (minimum 12 frames)
  Titan sparks and takes 1 self-damage every 300 frames (self-destructing)

PISTON PUNCH (replaces arm slams)
  Tell: 12 frames (0.2s) -- arm retracts, piston charges
  Active: single powerful punch, 6-tile range horizontal
  Damage: 4
  Recovery: 36 frames (0.6s) -- arm extended, gears grinding
  Vulnerability: YES (core wide open during recovery)

GEAR STORM (replaces Gear Launch)
  Tell: 18 frames (0.3s) -- body shakes violently
  Active: gears fly off Titan body in all directions
  8 gears, random trajectories, 6.0 tiles/s
  Each gear: 2 damage
  Duration: 30 frames (0.5s)
  Titan takes 20 self-damage from losing parts
  Vulnerability: YES (Titan is literally falling apart, full damage)

MELTDOWN (at 10% HP / 120 HP)
  Titan freezes, all gears lock
  Tell: 60 frames (1.0s) -- vibration intensifies, screen shakes
  Explosion: radial shockwave, 8-tile radius, 5 damage
  Safe zone: beyond 8 tiles from Titan center or behind any remaining structure
  After explosion: Titan collapses, 120-frame vulnerability (2.0s), 3x damage
  This is the kill window -- unload everything
```

### Loot on Defeat

| Reward | Value |
|--------|-------|
| Coins | 2,000 |
| Weapon Part | Clockwork Autoloader (weapon fires in 3-round bursts with no delay) |
| Era Completion Bonus | 4,000 XP |
| Achievement | "Clockstopper" |
| Health Restore | Full HP |
| Bonus | Unlocks Clockwork weapon skin set |

---

## Boss 6: Steam Leviathan (Era 6 -- Industrial)

### Concept

A massive steam-powered machine (96x64 pixel sprite, 6x4 tiles) -- part locomotive, part factory, part war machine. It fills the right side of the arena and slowly advances. The arena is an industrial facility with boiler vents, pressure pipes, and coal chutes that can all be destroyed. Players must destroy boiler vents to cause overheating, and blast pressure pipes to redirect scalding steam at the machine's exposed joints.

**Target Duration:** 75-90 seconds | **Total HP:** 1,500 | **Phases:** 3

### Arena Layout

```
Width: 32 tiles | Height: 16 tiles (camera-locked)
B = Boiler vent (destructible, 4 HP, 6 total)
P = Pressure pipe (destructible, 3 HP, redirects steam)
K = Coal chute (destructible, 2 HP, drops coal that damages Leviathan)
S = Scaffold platform (destructible, 3 HP)
I = Industrial platform (indestructible)
L = Leviathan body (advances leftward over time)
# = Solid boundary

################################
#..............................#
#..BB.......BB.......BB........#   Row 14: Boiler vents (upper, 3 pairs)
#..............................#
#..SS.SSSS.SS.SSSS.SS.[LLLLLL]#   Row 12: Scaffolding + Leviathan top
#..PP...PP...PP...PP...[LLLLLL]#   Row 11: Pressure pipes + Leviathan
#.....................[LLLLLL]..#   Row 10: Leviathan body
#.II........II........[LLLLLL]..#   Row 9: Platforms + Leviathan body
#..............................#   Row 8: Open
#..KK........KK.......[LLLL]..#   Row 7: Coal chutes + Leviathan front
#.II...II...II...II............#   Row 6: Industrial platforms
#..............................#   Row 5: Open
#..BB.......BB.......BB........#   Row 4: Boiler vents (lower, 3 pairs)
#..............................#   Row 3: Open
################################   Row 1-2: Floor (grated metal)
################################   Row 0: Sub-floor

Leviathan body: tiles 22-31, rows 7-12 (advances left 1 tile every 300 frames)
Boiler vents (upper): tiles 3-4, 12-13, 21-22 at row 14
Boiler vents (lower): tiles 3-4, 12-13, 21-22 at row 4
Pressure pipes: tiles 3-4, 8-9, 13-14, 18-19 at row 11
Coal chutes: tiles 3-4, 13-14 at row 7
Scaffolding: various positions at row 12, destructible (3 HP each)
Player start: tile 3, row 2
```

### Phase 1: "Full Steam" (100% to 65% HP -- 525 HP)

The Leviathan operates at peak efficiency. Its advance is slow but relentless. Direct weapon fire deals 50% damage to its armored hull. Boiler vents and pressure pipes are the key to dealing real damage.

**Overheat Mechanic:**
- Leviathan has an Overheat gauge: 0-100
- Each destroyed boiler vent: +25 Overheat
- At 100 Overheat: Leviathan enters Overheat State for 180 frames (3.0s)
  - All attacks cease
  - Steam vents from every joint (cosmetic + light area damage, 1/s in 2-tile radius around body)
  - All body segments take 2x damage
  - Overheat resets to 0 after state ends
- 6 boiler vents total (4 needed for first Overheat, leaving 2 for Phase 2)
- Boiler vents respawn after 600 frames (10.0s) at 50% HP

**Pressure Pipe Mechanic:**
- Destroying a pressure pipe (3 HP) causes it to spray steam in a direction
- Steam spray: 4-tile range, 1 tile wide, 2 damage/s to anything in path
- If steam spray hits the Leviathan: 5 damage/s for 120 frames (60 total damage)
- Player must position themselves to blast pipes that face the Leviathan
- Pipes can also hurt the player if they are in the spray path

**Attack Pattern:**

```
SMOKESTACK VOLLEY
  Cycle: every 180 frames (3.0s)
  Tell: 24 frames (0.4s) -- smokestacks belch black, sparks inside
  Active: 4 molten slag projectiles fired upward then arc down
  Projectile speed: 5.0 tiles/s at launch, arc physics
  Landing zones: semi-random, telegraphed by red target circles on floor (18 frames warning)
  Damage: 2 each
  Slag creates 1-tile fire pool on landing (1 damage/s, 90 frames duration)
  Vulnerability: partially (smokestacks area exposed, 75% damage)

PISTON RAM
  Trigger: player within 5 tiles of Leviathan front
  Tell: 24 frames (0.4s) -- front piston retracts, hydraulic hiss
  Active: piston extends 6 tiles horizontally
  Damage: 3
  Piston remains extended for 18 frames then retracts
  Recovery: 42 frames (0.7s) -- piston housing exposed
  Vulnerability: YES during recovery (piston housing, 1.5x damage)

STEAM WHISTLE
  Cycle: every 300 frames (5.0s)
  Tell: 18 frames (0.3s) -- whistle glows white
  Active: piercing whistle, radial shockwave 6-tile radius from Leviathan center
  Damage: 1 (shockwave)
  Effect: player pushed 3 tiles away from Leviathan (knockback)
  Duration: 12 frames
  Vulnerability: NO

COAL FEED ACCELERATION
  Every 600 frames (10.0s): Leviathan speeds up advance by 0.5 tiles per interval
  Base advance: 1 tile per 300 frames
  After 2 accelerations: 1 tile per 200 frames
  Destroying coal chutes (2 HP each) prevents acceleration
  If both coal chutes destroyed: advance stops for 300 frames, then resumes at base speed
```

### Phase 2: "Overclocked Engine" (65% to 30% HP -- 525 HP)

**Transition:** 48 frames. Leviathan's engine surges, red lights flash. Front section opens revealing internal furnace.

**Changes:**
- Leviathan advance speed doubled (1 tile per 150 frames)
- Furnace exposed in front (new weak point, 2x damage)
- All attack cooldowns reduced by 25%
- Boiler vents respawn faster (300 frames / 5.0s)
- New attack: FURNACE BLAST
- Scaffolding becomes critical -- destruction creates gaps player can fall through

```
FURNACE BLAST (new)
  Tell: 30 frames (0.5s) -- furnace door opens, interior glows white-hot
  Active: horizontal fire beam from front of Leviathan
  Beam width: 2 tiles high, extends full arena width
  Beam height: rows 7-8
  Damage: 4
  Duration: 36 frames (0.6s)
  Dodge: jump above or drop below beam height
  Scaffolding in beam path is destroyed
  Recovery: 48 frames (0.8s) -- furnace door jammed open, 3x damage to furnace
  Vulnerability: YES (best damage window, furnace 3x)

SMOKESTACK VOLLEY (modified)
  Now fires 6 projectiles instead of 4
  Fire pools last 150 frames (longer)

Pressure pipes become critical:
  New pipes spawn at row 7 when old ones are destroyed (factory auto-repair)
  Pipes facing Leviathan deal 8 damage/s (increased from 5)
  Strategic pipe destruction is the fastest damage method
```

### Phase 3: "Critical Pressure" (30% to 0% HP -- 450 HP)

**Transition:** 60 frames. Leviathan's boiler ruptures. Machine stops advancing (stuck). Massive steam cloud fills lower arena (rows 1-4 obscured, 1 damage/s to player in cloud). Upper arena is clear.

**Arena Changes:**
- Lower arena (rows 1-4) filled with scalding steam cloud (1 damage/s, visibility reduced)
- Leviathan stationary but enters a damage frenzy
- All scaffolding and platforms in lower half destroyed
- Upper platforms and industrial platforms remain
- Leviathan hull cracked: 100% damage from all angles
- Player must fight from upper arena using remaining platforms

```
BOILER ERUPTION
  Cycle: every 120 frames (2.0s)
  Tell: 18 frames (0.3s) -- hull section bulges, glows red
  Active: steam explosion from random hull section
  Explosion radius: 4 tiles
  Damage: 3
  Creates temporary gap in hull (vulnerability point, 2x damage for 60 frames)
  Dodge: watch for bulging hull section, move away

DESPERATION VOLLEY
  Cycle: every 90 frames (1.5s)
  Tell: 12 frames (0.2s) -- minimal warning
  Active: 8 slag projectiles, rapid fire, aimed at upper platforms
  Damage: 2 each
  Forces constant movement between upper platforms
  Vulnerability: smokestacks exposed during volley (1.5x damage)

FINAL EXPLOSION (at 5% HP / 75 HP)
  Leviathan begins chain explosion sequence
  Player has 300 frames (5.0s) to deal remaining damage
  Explosions occur every 60 frames at random hull sections
  Each explosion: 3 tiles radius, 2 damage
  If player depletes HP: Leviathan explodes in controlled cinematic
  If player fails (timer): Leviathan explodes dealing 5 damage to player
    (survivable but punishing)
```

### Loot on Defeat

| Reward | Value |
|--------|-------|
| Coins | 2,500 |
| Weapon Part | Steam Pressure Cannon (charged shot: hold to charge, 3x damage on release) |
| Era Completion Bonus | 5,000 XP |
| Achievement | "Boiler Buster" |
| Health Restore | Full HP |
| Bonus | Unlocks Industrial weapon skin set |

---

## Boss 7: War Machine (Era 7 -- Modern)

### Concept

A military mech/tank hybrid (80x64 pixel sprite, 5x4 tiles) in an urban warfare arena. Concrete barriers, sandbag walls, ruined buildings, and weapon emplacements fill the arena. The War Machine has multiple weapon systems (missile pod, minigun, mortar, energy shield generator) that can be individually targeted and destroyed. The arena's concrete barriers provide cover but can be destroyed by both the War Machine's attacks and the player's weapons.

**Target Duration:** 80-95 seconds | **Total HP:** 1,800 | **Phases:** 3

### Arena Layout

```
Width: 32 tiles | Height: 16 tiles (camera-locked)
C = Concrete barrier (destructible, 4 HP, provides cover)
S = Sandbag wall (destructible, 2 HP, low cover)
R = Ruined building wall (destructible, 5 HP, tall cover)
E = Weapon emplacement (destructible, 3 HP, player can use briefly)
W = War Machine
# = Solid boundary

################################
#..............................#
#..RR..........RR..........RR..#   Row 14: Ruined building tops
#..RR..........RR..........RR..#   Row 13: Ruined building walls
#..RR..........RR..........RR..#   Row 12: Ruined building walls
#..............................#   Row 11: Open (War Machine fires over cover)
#..CCCC....CCCC....CCCC........#   Row 10: Concrete barriers (upper)
#....E..........E..............#   Row 9: Weapon emplacements
#..............................#   Row 8: Open
#..CCCC....CCCC....CCCC........#   Row 7: Concrete barriers (lower)
#..SS..SS..SS..SS..SS..........#   Row 6: Sandbag walls
#..............................#   Row 5: Open
#....CCCC......CCCC............#   Row 4: Forward concrete barriers
#..SS....SS....SS..............#   Row 3: Forward sandbags
################################   Row 1-2: Street level (floor)
################################   Row 0: Sub-floor

War Machine position: tiles 25-29, rows 3-6 (right side, ground level)
  Moves between three positions: tiles 24-28, tiles 22-26, tiles 20-24
Concrete barriers: 9 total, positions as shown (4 HP each)
Sandbag walls: 8 total (2 HP each)
Ruined buildings: 3 structures (5 HP each wall segment, 2 segments per building)
Weapon emplacements: tiles 5 and 16, row 9 (3 HP, player usable for 2x damage for 60 frames)
Player start: tile 3, row 2
```

### Phase 1: "Fire Superiority" (100% to 65% HP -- 630 HP)

The War Machine occupies the right side of the arena and unleashes overwhelming firepower. Its armor is thick -- direct hits deal 50% damage. Cover is essential for survival. The War Machine's own attacks destroy cover over time, creating an attrition dynamic.

**Weapon System Targeting:**
The War Machine has 4 weapon systems, each a targetable component:
- Missile Pod (top-right, 3x1 tiles): 60 HP -- destroy to disable MISSILE BARRAGE
- Minigun (front-left, 2x1 tiles): 50 HP -- destroy to disable SUPPRESSION FIRE
- Mortar (top-center, 2x2 tiles): 50 HP -- destroy to disable MORTAR RAIN
- Shield Generator (rear, 2x1 tiles): 40 HP -- destroy to remove damage reduction

Destroying a weapon system: 50 bonus damage to War Machine + system disabled permanently.

**Attack Pattern:**

```
MISSILE BARRAGE (Missile Pod)
  Cycle: every 240 frames (4.0s)
  Tell: 24 frames (0.4s) -- missile pod opens, targeting lasers sweep arena
  Active: 4 missiles launch, arc toward player and nearby cover
  Missile speed: 6.0 tiles/s
  Missiles track player loosely (1.0 tiles/s tracking)
  Damage: 3 each
  Missiles destroy cover on impact (deal 3 damage to destructible elements)
  Dodge: dash perpendicular to missile path, or position so missiles hit non-critical cover
  Vulnerability: missile pod exposed during reload (30 frames after barrage)

SUPPRESSION FIRE (Minigun)
  Cycle: every 150 frames (2.5s)
  Tell: 18 frames (0.3s) -- minigun barrel spins up
  Active: sustained fire, 30 rounds over 60 frames, aimed at player
  Each round: 1 damage, 8.0 tiles/s
  Spread: +/- 5 degrees random
  Rounds destroy sandbags (1 damage per hit to destructible elements)
  Dodge: stay behind concrete barrier (absorbs hits) or keep moving
  Vulnerability: NO (War Machine fully oriented toward player)
  Note: minigun fire gradually destroys player's cover

MORTAR RAIN (Mortar)
  Cycle: every 300 frames (5.0s)
  Tell: 30 frames (0.5s) -- mortar tube elevates, thump sound
  Active: 3 mortar shells arc high and land at semi-random positions
  Landing zones: telegraphed by red circles on ground (24 frames warning)
  Damage: 3 each (2 damage splash in 2-tile radius)
  Mortar shells destroy all destructible elements in blast radius
  Dodge: move away from red circles
  Vulnerability: mortar tube exposed during aiming (30 frames)

SHIELD GENERATOR
  Passive: while active, War Machine takes 50% reduced damage from all sources
  Generator visible on rear of machine
  Must destroy cover between player and rear to get line of fire on generator
  Generator HP: 40
  When destroyed: War Machine takes full damage, sparks fly from shield emitters
```

**Cover Destruction Dynamic:**
- War Machine attacks destroy player's cover over time
- Player must manage which cover to preserve and which to sacrifice
- Destroying forward cover opens sight lines to War Machine weapon systems
- Weapon emplacements (3 HP) can be activated by player (stand near for 30 frames): fires at War Machine for 2x player damage for 60 frames, then overheats and is destroyed

### Phase 2: "Close Quarters" (65% to 30% HP -- 630 HP)

**Transition:** 48 frames. War Machine advances to mid-arena. Deploys smoke screen (6 frames of screen darkening). When smoke clears, War Machine is at tiles 15-19.

**Changes:**
- War Machine at mid-arena, much closer to player
- Any destroyed weapon systems remain destroyed
- Remaining weapon systems fire 30% faster
- War Machine can now use melee attacks
- New destructible elements: barricade debris from transition

```
HYDRAULIC STOMP (new, melee)
  Trigger: player within 4 tiles
  Tell: 18 frames (0.3s) -- leg raises, hydraulic hiss
  Active: stomp creates shockwave, 4 tiles radius
  Damage: 3 (stomp), 2 (shockwave)
  Destroys all cover in stomp radius
  Recovery: 36 frames (0.6s) -- leg stuck, undercarriage exposed
  Vulnerability: YES (undercarriage is a 2x damage weak point)

RAMMING CHARGE (new)
  Trigger: player at far left of arena (beyond tile 6) for 120+ frames
  Tell: 30 frames (0.5s) -- engine revs, tracks spin
  Active: War Machine charges left at 8.0 tiles/s
  Destroys ALL cover in path
  Damage: 4 (direct hit)
  War Machine stops at tile 5, then slowly reverses
  Recovery: 60 frames (1.0s) during reversal -- rear exposed
  Vulnerability: YES (rear is exposed, shield generator location = 2x damage if generator destroyed)

Ruined buildings become critical:
  Tall cover (5 HP walls) blocks missile tracking
  Player can blast through buildings to create escape routes
  Collapsing a building on the War Machine: 80 damage (if machine is adjacent)
  Building collapse: destroy both wall segments of a building to topple it
```

### Phase 3: "Disabled and Dangerous" (30% to 0% HP -- 540 HP)

**Transition:** 60 frames. War Machine takes critical damage, treads lock up. Machine is immobile but all remaining weapons enter overdrive. Armor cracked: 100% damage from all angles. Final weapon system: SELF-DESTRUCT PROTOCOL.

**Arena Changes:**
- Most cover destroyed from Phase 1-2 combat
- Some concrete barriers regenerated (military auto-deploy, 4 new barriers at random positions)
- War Machine immobile at current position
- All weapon systems still functioning fire at 2x speed
- War Machine hull cracked: 100% damage, no reduction

```
OVERDRIVE (all remaining weapons)
  All weapons fire at 2x speed (half cooldowns)
  Tells reduced by 6 frames each
  War Machine takes 2 self-damage per weapon firing (systems overloading)
  Total self-damage: ~3-5/s depending on remaining weapons

COUNTERMEASURE FLARES (new)
  Cycle: every 180 frames (3.0s)
  Tell: 12 frames (0.2s) -- hatches open on sides
  Active: 8 flares launched in spread pattern, slow-falling
  Flare fall speed: 2.0 tiles/s
  Each flare: 1 damage on contact, creates 1-tile fire pool on landing (90 frames)
  Fills arena with hazard zones
  Dodge: move between flare positions
  Vulnerability: YES (hatches open, internal systems exposed, 1.5x damage)

SELF-DESTRUCT (at 10% HP / 180 HP)
  War Machine initiates 10-second self-destruct countdown
  Visual: red countdown timer on machine, warning klaxon
  During countdown: all weapons fire continuously (maximum aggression)
  Player must destroy remaining HP in 600 frames (10.0s)
  If successful: controlled explosion cinematic
  If time expires: massive explosion, 8 damage to player (likely lethal)
  During countdown: War Machine takes 1.5x damage (desperate systems failure)
  Best strategy: find remaining cover, focus fire on cracked hull
```

### Loot on Defeat

| Reward | Value |
|--------|-------|
| Coins | 3,000 |
| Weapon Part | Targeting Computer (auto-fire gains slight homing: bullets curve 10 degrees toward nearest enemy) |
| Era Completion Bonus | 6,000 XP |
| Achievement | "Arms Dealer" |
| Health Restore | Full HP |
| Bonus | Unlocks Military weapon skin set |

---

## Boss 8: Core Sentinel (Era 8 -- Digital)

### Concept

A holographic/digital entity (64x64 pixel sprite, 4x4 tiles) that exists within a cyberspace arena. The Core Sentinel projects itself through data nodes scattered around the arena. It has a layered shield system powered by these nodes -- destroying nodes weakens its shields and reveals its true core. The arena is a digital landscape with destructible data walls, firewall barriers, and code pillars that can be blasted apart. The Sentinel can teleport between intact data nodes.

**Target Duration:** 85-100 seconds | **Total HP:** 2,000 | **Phases:** 3

### Arena Layout

```
Width: 32 tiles | Height: 16 tiles (camera-locked)
N = Data node (destructible, 5 HP each, 8 total)
F = Firewall barrier (destructible, 4 HP, blocks player movement and shots)
D = Data wall (destructible, 3 HP, semi-transparent, blocks only shots)
C = Code pillar (destructible, 4 HP, collapses into platform)
H = Holographic platform (indestructible, flickers every 180 frames for 30 frames)
# = Solid boundary (rendered as grid/digital border)

################################
#.N.........N........N......N.#   Row 15: Data nodes (upper corners/mid)
#..............................#
#..FFFF........FFFF............#   Row 13: Firewall barriers
#..............................#
#.DD...HHH..DD...HHH.DD.......#   Row 11: Data walls + holographic platforms
#..CC....[SENTINEL]...CC.......#   Row 10: Code pillars + Sentinel
#..CC....................CC....#   Row 9: Code pillar bases
#..............................#   Row 8: Open
#..FFFF........FFFF............#   Row 7: Firewall barriers (lower)
#.DD...HHH..DD...HHH.DD.......#   Row 6: Data walls + holographic platforms
#..............................#   Row 5: Open
#..CC..........CC..............#   Row 4: Code pillars (lower)
#.N.........N........N......N.#   Row 3: Data nodes (lower)
################################   Row 1-2: Floor (digital grid)
################################   Row 0: Sub-floor

Data nodes (8 total):
  Upper: tiles 2, 12, 21, 29 at row 15
  Lower: tiles 2, 12, 21, 29 at row 3
Sentinel position: center (tiles 14-17, rows 9-12), can teleport to any data node
Firewall barriers: tiles 3-6 and 15-18 at rows 7 and 13
Data walls: tiles 2-3, 10-11, 20-21 at rows 6 and 11
Code pillars: tiles 3-4, 23-24 at rows 9-10; tiles 3-4, 15-16 at rows 4-5
Holographic platforms: tiles 7-9 and 17-19 at rows 6 and 11
Player start: tile 3, row 2
```

### Shield System

The Core Sentinel has a layered shield powered by data nodes:

```
Shield Layers:
  Layer 3 (outermost): Active when 6-8 nodes intact. Damage reduction: 75%
  Layer 2: Active when 3-5 nodes intact. Damage reduction: 50%
  Layer 1: Active when 1-2 nodes intact. Damage reduction: 25%
  Core Exposed: All nodes destroyed. Damage reduction: 0%

Each node destroyed:
  - Shield flickers and weakens visibly
  - Sentinel takes 30 damage (node feedback)
  - Sentinel is stunned for 45 frames (0.75s)
  - Sentinel loses one teleport destination
  - Visual: shield layer peels away like digital static
```

### Phase 1: "Firewall Protocol" (100% to 65% HP -- 700 HP)

All 8 data nodes active. Shield at maximum (75% damage reduction). Sentinel teleports frequently. Player must destroy data nodes while navigating digital hazards.

**Attack Pattern:**

```
BINARY BEAM
  Cycle: every 180 frames (3.0s)
  Tell: 24 frames (0.4s) -- targeting reticle appears on player, digital whine
  Active: precision beam from Sentinel to player position at fire time
  Beam speed: instantaneous (hitscan)
  Beam width: 1 tile
  Damage: 3
  Beam passes through data walls but is blocked by firewall barriers
  Beam destroys data walls in path (1 hit = destroyed)
  Dodge: dash or move out of reticle before fire frame, or stand behind firewall
  Recovery: 24 frames (0.4s) -- Sentinel core flickers
  Vulnerability: YES during recovery (reduced by shield layer)

DATA SWARM
  Cycle: every 240 frames (4.0s)
  Tell: 18 frames (0.3s) -- pixel particles coalesce around Sentinel
  Active: swarm of 12 data fragments orbits outward from Sentinel
  Fragments: 1x1 tile each, orbit at 3-tile radius expanding to 8-tile radius
  Speed: 5.0 tiles/s orbital, 2.0 tiles/s expansion
  Damage: 1 per fragment
  Duration: 90 frames (1.5s) then fragments despawn
  Dodge: stay outside expansion radius or find gaps in orbital pattern
  Vulnerability: NO (Sentinel is generating the swarm)

TELEPORT
  Trigger: player gets within 4 tiles of Sentinel, or every 300 frames
  Tell: 18 frames (0.3s) -- Sentinel pixelates, digital scramble visual
  Action: Sentinel teleports to a random intact data node
  Leaves behind a DATA MINE at departure location
  DATA MINE: 2-tile radius, activates after 30 frames, deals 2 damage, lasts 120 frames
  After teleport: 24-frame materialization, Sentinel vulnerable during materialization
  Vulnerability: YES during materialization (12 frames, still reduced by shield)

FIREWALL REFRESH
  Cycle: every 600 frames (10.0s)
  Any destroyed firewall barrier regenerates
  Data walls do NOT regenerate
  Player must re-destroy firewalls for sight lines
```

**Strategy:**
- Destroy data nodes to weaken shields (priority target)
- Use firewall barriers as cover from Binary Beam
- Blast data walls to create sight lines to distant nodes
- Collapse code pillars (4 HP) to create platforms for reaching upper nodes
- Holographic platforms flicker off for 30 frames every 180 frames -- don't stand on them during flicker

### Phase 2: "Virus Injection" (65% to 30% HP -- 700 HP)

**Transition:** 36 frames. Sentinel glitches, arena flickers. Digital "virus" visual effect -- green corruption spreads from destroyed node locations.

**Requirements:** At least 4 nodes must be destroyed to trigger Phase 2. If fewer than 4 are destroyed when HP reaches 65%, boss stays in Phase 1 pattern until 4 nodes are down.

**Changes:**
- Remaining nodes pulse with energy (easier to spot but also harder to reach)
- Firewall barriers no longer regenerate
- Sentinel teleports more frequently (every 180 frames)
- New attack: CORRUPTED ZONE
- Data walls are replaced by Virus Walls (same HP but damage player on contact, 1 damage)

```
CORRUPTED ZONE (new)
  Tell: 24 frames (0.4s) -- floor grid section turns red/green corrupted
  Active: 4x4 tile zone becomes corrupted
  Damage: 2 per second while standing in zone
  Duration: 240 frames (4.0s)
  Max active zones: 3
  Zones placed at player's recent positions (breadcrumb targeting)
  Forces constant movement
  Vulnerability: YES during zone creation (Sentinel focuses on corruption, core exposed)

BINARY BEAM (modified)
  Now fires 3 beams in spread pattern (-15, 0, +15 degrees)
  Each beam: 2 damage
  Tells reduced to 18 frames

DATA SWARM (modified)
  Fragments now home toward player slightly (0.5 tiles/s tracking)
  Fragment count: 16

SYSTEM OVERWRITE (new)
  Trigger: when a data node is destroyed in Phase 2
  Sentinel screams (digital distortion), sends shockwave from destroyed node
  Shockwave: 6-tile radius, 2 damage
  Then: Sentinel is stunned for 60 frames (1.0s) at current position
  Vulnerability: YES, 2x damage during stun (shield reduced from fewer nodes)
```

### Phase 3: "Core Meltdown" (30% to 0% HP -- 600 HP)

**Transition:** 48 frames. All remaining data nodes auto-destruct (each deals 30 damage to Sentinel = potential significant damage). Shield completely gone. Sentinel's true form revealed: a fractured digital core with exposed circuitry.

**Arena Changes:**
- All nodes destroyed (no teleport destinations)
- All firewall barriers and data walls destroyed
- Arena is open digital grid with only indestructible platforms and code pillar rubble
- Sentinel floats at center, immobile, but launches devastating attacks
- Core fully exposed: 0% damage reduction
- New destructible elements: SYSTEM FRAGMENTS orbit the Sentinel

```
SYSTEM FRAGMENTS
  4 fragments orbit Sentinel at 3-tile radius
  Each fragment: 4 HP, 1x1 tile
  Fragments block player shots that would hit core
  Destroying a fragment: 20 damage to Sentinel + fragment explosion (2-tile radius, 2 damage)
  Fragments respawn after 300 frames (5.0s)
  Strategy: destroy fragments for damage bonus and clear shot to core

DIGITAL STORM
  Cycle: every 120 frames (2.0s)
  Tell: 18 frames (0.3s) -- arena edges crackle with energy
  Active: grid squares across arena flash and deal damage in random pattern
  Pattern: 8 random 2x2 tile zones flash (18-frame warning per zone)
  Damage: 2 per zone
  Duration: 36 frames
  Dodge: watch floor grid for warning flashes, move to dark squares
  Vulnerability: YES (Sentinel is channeling, core exposed)

LASER GRID
  Cycle: every 240 frames (4.0s)
  Tell: 24 frames (0.4s) -- laser lines appear on grid, showing pattern
  Active: horizontal and vertical laser lines activate across arena
  Pattern: 3 horizontal + 3 vertical lines, leaving safe squares between them
  Damage: 3 per line
  Duration: 30 frames
  Dodge: position in gaps between laser lines (shown during tell)
  Vulnerability: NO (Sentinel protected during laser generation)
  After lasers: 36-frame cooldown, core exposed
  Vulnerability: YES during cooldown

CORE IMPLOSION (at 10% HP / 200 HP)
  Tell: 60 frames (1.0s) -- core collapses inward, massive energy buildup
  Active: explosion expands outward, 10-tile radius
  Damage: 5
  Safe zone: arena corners (beyond 10 tiles from center)
  After implosion: Sentinel is stunned for 120 frames (2.0s)
  Vulnerability: YES, 3x damage during post-implosion stun
  This is the intended kill window
```

### Loot on Defeat

| Reward | Value |
|--------|-------|
| Coins | 3,500 |
| Weapon Part | Digital Splitter (shots split into 2 on hit, each dealing 60% damage to nearby targets) |
| Era Completion Bonus | 7,000 XP |
| Achievement | "Firewall Breach" |
| Health Restore | Full HP |
| Bonus | Unlocks Digital weapon skin set + holographic trail effect |

---

## Boss 9: Void Emperor (Era 9 -- Space Age)

### Concept

An alien entity (64x80 pixel sprite, 4x5 tiles) encased in layered energy shields, commanding from within an alien structure. The arena is a space station interior with alien architecture -- energy conduits, crystalline walls, gravity generators, and shield pylons. The Void Emperor's shields are powered by alien structures throughout the arena. Players must destroy these structures to strip shields, then redirect energy beams from destroyed conduits to damage the Emperor directly.

**Target Duration:** 95-110 seconds | **Total HP:** 2,500 | **Phases:** 4

### Arena Layout

```
Width: 32 tiles | Height: 18 tiles (camera-locked)
S = Shield pylon (destructible, 6 HP each, 6 total)
E = Energy conduit (destructible, 4 HP, redirects beam when destroyed)
C = Crystalline wall (destructible, 3 HP, shatters into damaging fragments)
G = Gravity pad (destructible, 2 HP, alters jump physics in 3-tile radius)
A = Alien platform (indestructible, organic-looking)
V = Void Emperor
# = Solid boundary (alien hull)

##################################
#................................#
#.S...........S..........S.......#   Row 17: Shield pylons (upper)
#................................#
#..EE...AA....EE....AA...EE.....#   Row 15: Energy conduits + platforms
#................................#
#..CC.......CC...CC.......CC.....#   Row 13: Crystalline walls
#..CC.......CC...CC.......CC.....#   Row 12: Crystalline walls
#...........[VOID]...............#   Row 11: Void Emperor center
#...........EMPEROR..............#   Row 10: Void Emperor
#..GG.......GG...GG.......GG....#   Row 9: Gravity pads
#................................#
#..CC.......CC...CC.......CC.....#   Row 7: Crystalline walls (lower)
#..EE...AA....EE....AA...EE.....#   Row 6: Energy conduits + platforms (lower)
#................................#   Row 5: Open
#.S...........S..........S.......#   Row 4: Shield pylons (lower)
##################################   Row 2-3: Floor (alien metal)
##################################   Row 0-1: Sub-floor

Void Emperor: center (tiles 12-15, rows 9-13)
Shield pylons (6 total):
  Upper: tiles 2, 14, 25 at row 17
  Lower: tiles 2, 14, 25 at row 4
Energy conduits (6 total): tiles 3-4, 14-15, 25-26 at rows 6 and 15
Crystalline walls: tiles 3-4, 12-13, 16-17, 25-26 at rows 7, 12-13
Gravity pads: tiles 3-4, 12-13, 16-17, 25-26 at row 9
Player start: tile 3, row 3
```

### Shield System

```
Void Emperor Shield Layers:
  Full Shield (6 pylons): Immune to all direct damage. Must destroy pylons.
  Shield 3 (4-5 pylons): 80% damage reduction
  Shield 2 (2-3 pylons): 50% damage reduction
  Shield 1 (1 pylon): 25% damage reduction
  No Shield (0 pylons): Full damage

Each pylon destroyed:
  - 40 damage to Emperor (energy feedback)
  - Shield visually degrades (layer peels away with particle effect)
  - Emperor is stunned for 60 frames (1.0s)
  - Pylon explosion: 3-tile radius, 2 damage (player must avoid)
```

### Energy Beam Redirect Mechanic

```
When an energy conduit is destroyed (4 HP):
  - Conduit breaks, releasing an energy beam
  - Beam fires in a fixed direction (toward center of arena)
  - If beam path intersects the Void Emperor: 15 damage/s continuous
  - Beam lasts 180 frames (3.0s) then conduit reforms
  - Player can destroy crystalline walls to clear beam path to Emperor
  - Crystalline wall fragments: scatter on destruction, each fragment deals
    1 damage in 1-tile radius (affects player and Emperor)
  - Strategic: destroy walls in line between conduit and Emperor, then
    destroy conduit to fire beam through cleared path
```

### Phase 1: "Sovereign Shield" (100% to 70% HP -- 750 HP)

Full shields active. Emperor is immune to direct damage. Player must focus on destroying shield pylons while dodging Emperor attacks.

**Attack Pattern:**

```
VOID BOLT
  Cycle: every 150 frames (2.5s)
  Tell: 24 frames (0.4s) -- Emperor's hands glow purple, void energy coalesces
  Active: fires 3 void bolts at player, slight spread
  Bolt speed: 7.0 tiles/s
  Damage: 2 each
  Bolts pass through crystalline walls but are blocked by alien platforms
  Dodge: lateral movement, use platforms for cover
  Vulnerability: N/A (immune, but pylons can be targeted during this)

GRAVITY WELL
  Cycle: every 300 frames (5.0s)
  Tell: 30 frames (0.5s) -- space distorts around a 4x4 area near player
  Active: gravity well pulls player toward center at 3.0 tiles/s
  Duration: 120 frames (2.0s)
  Damage: 0 (pull only, but may pull into hazards or Emperor's attacks)
  Well center: placed 3 tiles ahead of player's movement direction
  Dodge: dash away from well center, or destroy gravity pads to disable
    gravity manipulation in that area
  Vulnerability: YES (Emperor focuses on gravity manipulation, pylons more exposed)

SHIELD PULSE
  Cycle: every 240 frames (4.0s)
  Tell: 18 frames (0.3s) -- shields brighten, energy ripple outward
  Active: expanding ring of energy from Emperor
  Ring radius: expands at 8.0 tiles/s from Emperor center
  Ring height: full arena height (cannot jump over)
  Damage: 2
  Dodge: find gap in ring (ring has 2-tile gap on alternating sides each pulse)
    or dash through ring during gap
  Crystalline walls stop the ring (wall takes 2 damage from ring)
  Strategy: keep crystalline walls intact in your area as ring defense
```

### Phase 2: "Exposed Monarch" (70% to 40% HP -- 750 HP)

**Transition:** 36 frames. At least 4 pylons must be destroyed to enter Phase 2 (if fewer destroyed at 70% HP, boss stays in Phase 1 pattern). Emperor screams, shields crack visibly.

**Changes:**
- 0-2 pylons remaining (25-50% reduction or none)
- Emperor becomes mobile: 2.0 tiles/s, floats between positions
- Energy conduits become primary damage tool (redirect beams)
- New attack: VOID RIFT
- Gravity pads destroyed in transition

```
VOID RIFT (new)
  Tell: 30 frames (0.5s) -- Emperor tears at space, rift outline appears
  Active: rift opens at target location, sucks in debris and player
  Rift size: 3x3 tiles
  Pull strength: 4.0 tiles/s toward rift center
  Damage: 3 per second in rift center, 1 per second in pull zone (5-tile radius)
  Duration: 150 frames (2.5s)
  Rift destroys any destructible elements it touches
  Dodge: dash away, use alien platforms (indestructible) as anchor points
  Vulnerability: YES (Emperor channels rift, core exposed through weakened shield)

VOID BOLT (modified)
  Now fires 5 bolts in rapid succession (6 frames apart)
  Bolts have slight homing (0.5 tiles/s tracking)

SHIELD PULSE (modified)
  Ring now has only a 1-tile gap (harder to dodge)
  Crystalline wall defense becomes critical
  Destroyed crystalline walls do NOT regenerate in Phase 2

Energy Beam Strategy (primary damage method):
  1. Clear crystalline walls between a conduit and Emperor's current position
  2. Destroy the conduit to release beam toward Emperor
  3. Beam deals 15 damage/s for 3.0s = 45 damage per successful redirect
  4. Emperor tries to move away from beam path (must be timed with his attack windows
     when he is stationary)
  5. 6 conduits available, each reforms after 180 frames
```

### Phase 3: "Void Collapse" (40% to 15% HP -- 625 HP)

**Transition:** 48 frames. All remaining pylons destroyed (auto-destruct, 40 damage each). Emperor's physical form destabilizes -- body flickers between solid and void.

**Arena Changes:**
- All shields gone: full damage to Emperor
- Arena begins breaking apart: floor sections collapse over time (1 section every 300 frames)
- Alien platforms remain (primary safe ground)
- Emperor more aggressive, moves at 3.0 tiles/s
- Conduits still available for beam redirects (now deal 20 damage/s due to no shields)

```
VOID BARRAGE (replaces Void Bolt)
  Cycle: every 90 frames (1.5s)
  Tell: 12 frames (0.2s) -- minimal warning
  Active: 8 void bolts in all directions from Emperor
  Bolt speed: 6.0 tiles/s
  Damage: 2 each
  Bolts bounce off arena walls once
  Vulnerability: YES during firing (Emperor opens to release energy)

DIMENSIONAL TEAR
  Cycle: every 180 frames (3.0s)
  Tell: 24 frames (0.4s) -- Emperor slashes at air, tear appears
  Active: tear creates a portal pair
  Portal 1: at Emperor position
  Portal 2: at player position
  Emperor's attacks travel through portals (can hit player from unexpected angles)
  Duration: 120 frames (2.0s)
  Portals can be destroyed (3 HP each) -- destroying a portal deals 20 damage to Emperor
  Vulnerability: NO during tear creation

FLOOR COLLAPSE (environmental)
  Every 300 frames: one 4-tile floor section collapses
  Collapse tell: 60 frames of cracking + dust
  Collapsed sections: bottomless void (instant kill on fall)
  Forces player to use alien platforms and remaining floor
  Player can shoot floor sections near Emperor to try to collapse them under him
    (Emperor floats -- does not fall, but floor destruction releases energy burst: 30 damage)
```

### Phase 4: "Annihilation" (15% to 0% HP -- 375 HP)

**Transition:** 36 frames. Emperor absorbs remaining arena energy. Half the arena floor is gone. Emperor grows in size (5x6 tiles) and becomes semi-transparent.

**Changes:**
- Emperor is large, semi-transparent, slow-moving (1.5 tiles/s)
- Takes full damage from all sources
- Only alien platforms and fragments of floor remain as terrain
- Conduits destroyed (no more beam redirects)
- Emperor's attacks are devastating but have longer tells
- This is a pure damage race

```
VOID ANNIHILATION BEAM
  Cycle: every 180 frames (3.0s)
  Tell: 36 frames (0.6s) -- Emperor charges energy between hands, aiming at player
  Active: massive beam sweeps across arena
  Beam width: 3 tiles
  Sweep speed: 4.0 tiles/s
  Damage: 5
  Duration: 60 frames (1.0s) sweep
  Dodge: get above or below beam path, use vertical positioning
  Recovery: 48 frames (0.8s) -- Emperor exhausted, core wide open
  Vulnerability: YES, 2x damage during recovery (best window)

REALITY COLLAPSE
  Cycle: every 300 frames (5.0s)
  Tell: 30 frames (0.5s) -- remaining floor sections glow
  Active: random floor sections (2-3) phase out for 60 frames then return
  Player falls if standing on phasing section
  Fall into void: 5 damage + respawn on nearest alien platform
  Vulnerability: NO

FINAL FORM (at 5% HP / 125 HP)
  Emperor goes fully transparent, invincible for 120 frames
  Arena flashes white
  Emperor reappears at center, solid, at original size
  Fully vulnerable for 180 frames (3.0s) -- no attacks, no shields
  3x damage multiplier
  This is the kill window
  If player fails to kill: Emperor fires one Annihilation Beam, then repeats FINAL FORM cycle
```

### Loot on Defeat

| Reward | Value |
|--------|-------|
| Coins | 4,000 |
| Weapon Part | Void Siphon Module (weapon drains 1 HP from enemies per second, restoring player HP) |
| Era Completion Bonus | 8,000 XP |
| Achievement | "Emperor's Fall" |
| Health Restore | Full HP |
| Bonus | Unlocks Alien weapon skin set + void particle trail effect |

---

## Boss 10: The Singularity (Era 10 -- Transcendent)

### Concept

The final boss: a reality-warping entity (variable sprite, base 64x64 pixels, 4x4 tiles, but distorts and reshapes) that exists across multiple dimensions simultaneously. The arena itself is unstable -- sections warp, gravity shifts, and the boundaries of reality break down. The player must destroy "reality anchors" to stabilize sections of the arena while fighting a boss that manipulates the environment itself. This is the ultimate test of every mechanic the player has learned.

**Target Duration:** 120-150 seconds | **Total HP:** 3,500 | **Phases:** 4

### Arena Layout (Initial -- changes per phase)

```
Width: 34 tiles | Height: 18 tiles (camera-locked)
R = Reality anchor (destructible, 8 HP each, 8 total)
W = Warping zone (unstable ground, changes per phase)
M = Mirror surface (destructible, 3 HP, reflects player shots)
X = Void crack (forms when reality anchors destroyed)
A = Stable platform (indestructible, anchor-stabilized)
# = Arena boundary (fluctuates visually)

####################################
#..................................#
#.R...........R..........R.......R.#   Row 17: Reality anchors (top)
#..................................#
#..AAAA...MMMMM...AAAA...MMMMM....#   Row 15: Platforms + mirror surfaces
#..................................#
#..WWWW...........WWWW.............#   Row 13: Warping zones
#..WWWW.[SINGULARITY].WWWW........#   Row 12: Boss center + warping
#..WWWW...........WWWW.............#   Row 11: Warping zones
#..................................#
#..AAAA...MMMMM...AAAA...MMMMM....#   Row 9: Platforms + mirror surfaces
#..................................#
#..WWWW...........WWWW.............#   Row 7: Warping zones (lower)
#.R...........R..........R.......R.#   Row 5: Reality anchors (bottom)
#..................................#
####################################   Row 2-3: Floor (unstable)
####################################   Row 0-1: Sub-floor

The Singularity: center (tiles 15-18, rows 11-14), floats
Reality anchors (8 total):
  Upper: tiles 2, 14, 25, 32 at row 17
  Lower: tiles 2, 14, 25, 32 at row 5
Warping zones: tiles 3-6 and 19-22 at rows 7, 11-13
Mirror surfaces: tiles 10-14 and 24-28 at rows 9 and 15
Stable platforms: tiles 3-6 and 19-22 at rows 9 and 15
Player start: tile 3, row 3
```

### Reality Anchor System

```
Reality Anchors (8 total, 8 HP each):
  Each anchor stabilizes a section of the arena
  Anchors must be strategically destroyed to damage the Singularity

When an anchor is destroyed:
  - Reality cracks open at anchor location (cosmetic + 2-tile danger zone, 1 damage/s)
  - The Singularity takes 60 damage (reality feedback)
  - Arena section near anchor warps: platforms may shift, gravity may change
  - Singularity is stunned for 90 frames (1.5s) -- strongest stun in the game
  - Void crack forms: environmental hazard but also damages Singularity
    (1 damage/s continuous if Singularity passes through crack zone)

Anchor destruction thresholds trigger phase transitions:
  Phase 1 -> Phase 2: 3 anchors destroyed (or HP reaches 70%)
  Phase 2 -> Phase 3: 6 anchors destroyed (or HP reaches 40%)
  Phase 3 -> Phase 4: all 8 anchors destroyed (or HP reaches 15%)
```

### Phase 1: "Reality Fractures" (100% to 70% HP -- 1,050 HP)

The Singularity manipulates the arena subtly. Warping zones distort physics (reversed gravity, slowed movement). The boss attacks with reality-bending projectiles and spatial distortions. Players learn anchor destruction as the primary damage method.

**Warping Zone Effects (Phase 1):**
- Reversed gravity (player falls upward in zone)
- Slow field (player movement at 50% in zone)
- Each effect telegraphed by visual distortion: purple = reversed gravity, blue = slow

**Attack Pattern:**

```
REALITY SHARD
  Cycle: every 150 frames (2.5s)
  Tell: 24 frames (0.4s) -- space cracks around Singularity, fragments form
  Active: 4 reality shards fly toward player
  Shard speed: 6.0 tiles/s
  Shard behavior: each shard teleports once during flight (blinks to new position
    2 tiles offset from predicted path)
  Damage: 2 each
  Dodge: watch for teleport flash (6-frame warning at new position), dash through gaps
  Vulnerability: YES after shards fired (Singularity core exposed, 24 frames)

DIMENSIONAL SHIFT
  Cycle: every 300 frames (5.0s)
  Tell: 30 frames (0.5s) -- arena edges glow, specific zone highlighted
  Active: one warping zone changes effect (gravity reversal becomes slow or vice versa)
  Also: zone boundaries shift 2 tiles in random direction
  Damage: 0 (indirect threat through environmental change)
  Forces player to constantly re-evaluate safe positions
  Vulnerability: NO

SPATIAL SLAM
  Trigger: player stationary for 90+ frames
  Tell: 18 frames (0.3s) -- space compresses around player position
  Active: spatial compression at player location, 3x3 tile area
  Damage: 3
  Area visually compresses (visual squash effect)
  Dodge: keep moving (attack never triggers if player stays mobile)
  Recovery: 24 frames
  Vulnerability: YES during recovery

MIRROR MECHANIC
  Mirror surfaces reflect player auto-fire shots
  Reflected shots deal 50% damage but can hit Singularity from unexpected angles
  Destroying mirrors (3 HP): removes reflection but also opens sight lines
  Strategic choice: keep mirrors for angle shots or destroy for direct path
```

### Phase 2: "Broken Symmetry" (70% to 40% HP -- 1,050 HP)

**Transition:** 48 frames. Arena visually shatters and reforms. Warping zones expand. Singularity splits into two overlapping images (original + mirrored).

**Arena Changes:**
- Warping zones expand by 2 tiles in all directions
- 3+ void cracks from destroyed anchors pulse with energy
- Mirror surfaces regenerate (if destroyed) at 50% HP
- Singularity appears as two overlapping images -- must hit the "real" one
- Floor begins showing cracks (cosmetic, telegraphing Phase 3)

**Dual Image Mechanic:**
- Two Singularity images present, one real and one mirror
- Both attack, but only the real one takes damage
- Real one: slightly brighter core glow (subtle visual tell)
- Mirror image has 1 HP -- one hit destroys it, revealing the real one for 120 frames
- Mirror image regenerates 120 frames after destruction
- Both images attack independently (doubles the attack pressure)

```
REALITY SHARD (modified)
  Both images fire shards (8 total, 4 each)
  Shard teleport distance increased to 3 tiles
  Damage: 2 each

VOID ERUPTION (new)
  Tell: 24 frames (0.4s) -- void cracks pulse, energy spirals upward
  Active: energy erupts from each void crack (from destroyed anchors)
  Eruption: 3-tile radius column, full arena height
  Damage: 3 per eruption
  Duration: 30 frames
  More anchors destroyed = more eruption points = more danger
  Trade-off: destroying anchors damages boss but makes arena more hazardous
  Vulnerability: YES (Singularity channels eruptions, core exposed)

GRAVITATIONAL INVERSION (new)
  Tell: 30 frames (0.5s) -- entire arena flickers, gravity arrow icon appears
  Active: arena gravity inverts for 180 frames (3.0s)
  Ceiling becomes floor, floor becomes ceiling
  All platforms function in reverse
  Player must navigate inverted arena while attacking
  Damage: 0 (indirect, disorientation + falling into void cracks)
  Vulnerability: NO during inversion

SPATIAL SLAM (modified)
  Now targets two positions simultaneously
  Tell: 18 frames
  Damage: 3 each
```

### Phase 3: "Dimensional Collapse" (40% to 15% HP -- 875 HP)

**Transition:** 60 frames. Arena violently shakes. All remaining anchors crack (reduced to 4 HP if above that). Warping zones merge into one massive unstable field covering 60% of the arena. Only stable platforms and anchor-adjacent areas are safe ground.

**Arena Changes:**
- 60% of arena floor is now warping zone (varied effects: gravity reversal, slow, damage fields)
- Stable platforms are primary safe ground
- Singularity is singular again (no dual image) but larger (5x5 tiles)
- Remaining anchors can be destroyed for 60 damage + stun
- Singularity moves through warping zones freely at 3.0 tiles/s
- Void cracks from destroyed anchors now continuously damage Singularity (1/s each, stacking)

**New Attacks:**

```
REALITY TEAR
  Cycle: every 120 frames (2.0s)
  Tell: 18 frames (0.3s) -- Singularity reaches out, space rips
  Active: 3 tears open across arena, each a 2x8 tile vertical rip
  Tears pulse for 60 frames, dealing 3 damage to anything touching them
  Tears destroy any destructible elements they touch
  After 60 frames: tears collapse, leaving 1-tile void cracks (permanent hazard)
  Vulnerability: YES during tear creation (Singularity's core visible through spatial distortion)

WARP STORM
  Cycle: every 240 frames (4.0s)
  Tell: 30 frames (0.5s) -- warping zones intensify, spiral pattern visible
  Active: all warping zones pulse outward, creating traveling distortion waves
  Waves: 2 tiles high, travel at 6.0 tiles/s across arena floor
  Wave damage: 2 on contact
  Waves distort player controls for 30 frames if hit (left/right reversed)
  Duration: 60 frames of waves
  Dodge: jump over waves, stay on stable platforms
  Vulnerability: YES after storm subsides (Singularity recharges, 36 frames)

SINGULARITY PULL (signature attack)
  Cycle: every 300 frames (5.0s)
  Tell: 36 frames (0.6s) -- boss glows white, all arena elements vibrate toward it
  Active: pulls everything toward Singularity center
  Pull strength: 5.0 tiles/s toward center
  Duration: 90 frames (1.5s)
  Anything that reaches center: 4 damage per second
  Destructible elements pulled in are destroyed (dealing 10 damage to Singularity per element consumed)
  Strategy: allow some destructible elements to be pulled in for bonus damage
  Dodge: dash away continuously, use platforms as anchor
  Recovery: 60 frames (1.0s) -- Singularity exhausted, core wide open
  Vulnerability: YES, 2x damage during recovery
```

### Phase 4: "The Final Collapse" (15% to 0% HP -- 525 HP)

**Transition:** 60 frames. All anchors destroyed (auto-destruct if any remain, 60 damage each). Arena boundary itself begins collapsing inward. The Singularity reaches its ultimate form -- a pure point of reality distortion.

**Arena Changes:**
- Arena shrinks over time: boundaries close in by 1 tile every 300 frames (5.0s)
- Starting at 34 tiles wide, shrinks toward center
- Only stable platforms remain as terrain (everything else is void or warping)
- Void cracks everywhere deal continuous 1 damage/s in their zones
- Singularity at center, large (6x6 tiles), pulsing between dimensions
- Full damage from all sources, no reduction

**Attack Pattern -- Desperation:**

```
REALITY CASCADE
  Cycle: every 90 frames (1.5s)
  Tell: 12 frames (0.2s) -- rapid flash
  Active: cascade of reality shards from all directions
  12 shards from arena edges, converging on center
  Each shard: 2 damage, 8.0 tiles/s
  Shards teleport once during flight
  Dodge: stay between shard paths, use dash i-frames
  Vulnerability: YES (Singularity channels, core exposed throughout)

DIMENSIONAL ERASURE
  Cycle: every 180 frames (3.0s)
  Tell: 24 frames (0.4s) -- one platform flickers
  Active: target platform ceases to exist for 120 frames (2.0s)
  If player is on platform: falls into void (3 damage + respawn on nearest stable platform)
  Platform returns after 120 frames
  Forces constant platform switching
  Vulnerability: NO (but boss is always vulnerable in Phase 4 to normal hits)

FINAL SINGULARITY (at 5% HP / 175 HP)
  The Singularity begins its final collapse
  Tell: 90 frames (1.5s) -- everything goes white, then black, then reality returns
  Arena stops shrinking
  All void cracks close
  Singularity is at center, small (2x2 tiles), fully exposed, not attacking
  Duration: 300 frames (5.0s) of pure vulnerability
  4x damage multiplier
  This is the final kill window
  If player fails to kill: Reality CASCADE fires from all directions (20 shards,
    near-impossible to fully dodge), then FINAL SINGULARITY repeats
  Second cycle: Singularity has only 120 frames (2.0s) of vulnerability (harder)
  Third cycle (if somehow needed): 60 frames only, designed to still be winnable
    but extremely tight
```

### Loot on Defeat

| Reward | Value |
|--------|-------|
| Coins | 10,000 |
| Weapon Part | Singularity Core (ultimate attachment: creates a micro black hole on hit every 5s, pulling enemies together and dealing 5 damage in 3-tile radius) |
| Era Completion Bonus | 15,000 XP |
| Achievement | "Reality Anchor" (platinum-tier) |
| Health Restore | Full HP |
| Bonus | Unlocks Transcendent weapon skin set + reality warp trail + "The Singularity" player title |

---

## Boss Encounter Summary Table

| Era | Boss | HP | Phases | Duration (s) | Arena Size | Key Destruction Mechanic | Escalation Theme |
|-----|------|----|--------|-------------|------------|--------------------------|------------------|
| 1 (Stone) | Cave Beast | 400 | 2 | 45-60 | 24x14 | Blast pillars, drop stalactites | Intro to destruction-as-damage |
| 2 (Bronze) | Bronze Colossus | 550 | 2 | 50-65 | 26x14 | Destroy armor plates, expose core | Targeted component destruction |
| 3 (Iron) | Iron Warlord | 700 | 2 | 55-70 | 28x14 | Breach fortification walls | Destruction as progression |
| 4 (Medieval) | Siege Dragon | 1,000 | 3 | 65-80 | 30x16 | Destroy perches, collapse castle | Vertical destruction + repositioning |
| 5 (Renaissance) | Clockwork Titan | 1,200 | 3 | 70-85 | 30x16 | Destroy gear housings, jam systems | System-based component destruction |
| 6 (Industrial) | Steam Leviathan | 1,500 | 3 | 75-90 | 32x16 | Destroy vents for overheat, redirect pipes | Environmental redirection |
| 7 (Modern) | War Machine | 1,800 | 3 | 80-95 | 32x16 | Destroy cover + weapon systems | Attrition and cover management |
| 8 (Digital) | Core Sentinel | 2,000 | 3 | 85-100 | 32x16 | Destroy data nodes, weaken shields | Multi-layer shield stripping |
| 9 (Space) | Void Emperor | 2,500 | 4 | 95-110 | 32x18 | Destroy pylons + redirect beams | Energy redirection + shield layers |
| 10 (Transcendent) | The Singularity | 3,500 | 4 | 120-150 | 34x18 | Destroy reality anchors, warp arena | All mechanics combined |

---

## Difficulty Scaling Within Eras

Each era contains 5 levels. The boss appears at level 5. Boss difficulty adjusts based on the player's weapon attachment loadout and upgrade level:

| Player Power Level | Boss HP Modifier | Boss Damage Modifier | Tell Duration Modifier |
|-------------------|-----------------|---------------------|----------------------|
| Base (no upgrades) | 1.0x | 1.0x | 1.0x |
| Moderate (1-2 upgrades) | 1.1x | 1.0x | 1.0x |
| Strong (3-4 upgrades) | 1.2x | 1.1x | 0.9x |
| Maximum (5+ upgrades) | 1.3x | 1.1x | 0.85x |

**Note:** Tell durations never drop below 12 frames (0.2s) regardless of modifiers.

---

## Destruction Mechanic Integration Guide

### Design Rules for Destructible Arena Elements

1. **Every boss fight must have at least one "destruction = damage" path.** Players should always be able to deal significant boss damage through environmental destruction, not just direct fire.

2. **Destructible elements must be visually distinct.** Cracked textures, different coloring, particle effects on hit. The player should never wonder "can I shoot that?"

3. **Destruction creates tactical options, not just damage.** Blasting a wall should open a new path, create cover, form a platform, or redirect a hazard -- not just deal damage.

4. **Destruction is permanent within a phase** (with noted exceptions). Once a wall is down, it stays down. This prevents repetitive destruction loops and creates a sense of progression within the fight.

5. **Bosses interact with destructible elements.** Boss attacks should destroy cover, boss movement should be affected by terrain changes, and bosses should react to arena changes (repositioning, new attacks unlocked by terrain state).

### Destructible Element HP Reference

| Material | HP (base weapon hits) | Found In Eras | Visual Tell |
|----------|----------------------|---------------|-------------|
| Rock/Stone | 2-3 | 1, 4 | Cracks on hit, dust particles |
| Bronze | 4-6 | 2 | Dent marks, metallic sparks |
| Iron/Metal | 4-5 | 3, 5, 6, 7 | Rivet pops, metal fragments |
| Wood | 1-2 | 3, 4 | Splinters, break on second hit |
| Concrete | 3-4 | 7 | Chips fly, rebar exposed |
| Crystal/Energy | 3-5 | 8, 9 | Fracture lines glow, shatter effect |
| Digital/Data | 3-5 | 8 | Pixel scatter, static effect |
| Reality Fabric | 5-8 | 9, 10 | Spatial distortion, void peek-through |

---

## Appendix A: Boss Animation Frame Budgets

| Animation | Eras 1-3 | Eras 4-6 | Eras 7-8 | Eras 9-10 |
|-----------|----------|----------|----------|-----------|
| Idle | 4 frames | 6 frames | 4 frames | 8 frames |
| Movement | 6 frames | 8 frames | 6 frames | 8 frames |
| Attack Tell | 3 frames | 4 frames | 3 frames | 4 frames |
| Attack Active | 4 frames | 6 frames | 4 frames | 6 frames |
| Recovery | 3 frames | 4 frames | 3 frames | 4 frames |
| Stun | 4 frames | 4 frames | 4 frames | 6 frames |
| Phase Transition | 8 frames | 12 frames | 10 frames | 16 frames |
| Hit React | 2 frames | 2 frames | 2 frames | 3 frames |
| Death | 12 frames | 16 frames | 14 frames | 24 frames |

All boss animations run at 12 fps (5 game frames per animation frame at 60 fps).

---

## Appendix B: Audio Cues per Boss

| Boss | Idle Ambient | Attack Tell | Attack Impact | Phase Transition | Vulnerability Window | Death |
|------|-------------|-------------|---------------|-----------------|---------------------|-------|
| Cave Beast | Low growl loop | Roar buildup | Rock crash / claw scrape | Bestial roar | Pained whimper tone | Collapse thud |
| Bronze Colossus | Metal creak loop | Bronze grinding | Metal clang / beam hum | Armor shatter cascade | Core resonance hum | Metal disintegration |
| Iron Warlord | March drum loop | Weapon draw scrape | Steel clash / javelin thunk | Wall explosion rumble | Shield drop clatter | Armor collapse |
| Siege Dragon | Wing leather flap | Inhale hiss | Fire roar / tail whip | Castle crumble rumble | Dragon groan | Crash + death cry |
| Clockwork Titan | Gear ticking loop | Gear acceleration whir | Piston slam / gear crunch | System failure alarm | Gear jam grind | Spring uncoil cascade |
| Steam Leviathan | Steam hiss loop | Pressure buildup whistle | Slag impact / piston crash | Boiler rupture blast | Pressure release hiss | Explosion chain |
| War Machine | Engine idle rumble | Weapon charge whine | Explosion / bullet impacts | Hydraulic advance thump | System alarm beep | Mech shutdown sequence |
| Core Sentinel | Digital hum loop | Data processing chirp | Beam zap / swarm buzz | Digital glitch stutter | Firewall down chime | System crash static |
| Void Emperor | Void ambient drone | Energy coalesce whoosh | Void bolt impact / rift tear | Shield shatter harmonic | Shield flicker tone | Dimensional collapse |
| The Singularity | Reality hum/distortion | Space compression sound | Reality crack / warp slam | Dimensional shift thunder | Anchor break resonance | Reality restoration chord |

Audio cues are essential for accessibility. Every attack must have a distinct audio tell that precedes the visual tell by 2-3 frames. Each boss's audio design should feel era-appropriate while maintaining consistent gameplay communication.

---

## Appendix C: Loot Progression Summary

| Era | Boss | Coins | XP | Weapon Part | Unique Bonus |
|-----|------|-------|----|-------------|-------------|
| 1 | Cave Beast | 500 | 1,000 | Stone Spreader Barrel | -- |
| 2 | Bronze Colossus | 750 | 1,500 | Bronze Piercer Module | -- |
| 3 | Iron Warlord | 1,000 | 2,000 | Iron Repeater Core | -- |
| 4 | Siege Dragon | 1,500 | 3,000 | Dragon Fang Barrel | Medieval skin set |
| 5 | Clockwork Titan | 2,000 | 4,000 | Clockwork Autoloader | Clockwork skin set |
| 6 | Steam Leviathan | 2,500 | 5,000 | Steam Pressure Cannon | Industrial skin set |
| 7 | War Machine | 3,000 | 6,000 | Targeting Computer | Military skin set |
| 8 | Core Sentinel | 3,500 | 7,000 | Digital Splitter | Digital skin set + holo trail |
| 9 | Void Emperor | 4,000 | 8,000 | Void Siphon Module | Alien skin set + void trail |
| 10 | The Singularity | 10,000 | 15,000 | Singularity Core | Transcendent skin set + warp trail + title |

All bosses restore full HP on defeat. A health pickup is guaranteed within 5 tiles before every boss arena entrance.
