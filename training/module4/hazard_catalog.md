# Assessment 4.5 -- Destructible Environment & Hazard Catalog

## Overview

This document specifies the destructible environment system for the 16-bit side-scrolling mobile shooter (iOS). The player's weapons auto-fire and blast through destructible terrain -- walls, rocks, floors, and era-specific materials. Destruction reveals pathways, bridges, stairs, and hidden areas. Strategically preserving material creates platforms, cover, and structural supports. Non-destructible environmental hazards are also present and must be navigated around.

The destructible environment system integrates with procedural level generation and must pass validation rules guaranteeing every level remains completable regardless of what the player destroys.

**Core Design Pillars**:
1. **Destruction as Progression** -- blasting through terrain is the primary means of advancing through levels.
2. **Strategic Preservation** -- leaving material intact creates platforms, cover, and structural advantages.
3. **Revelation** -- destruction exposes hidden paths, secret rooms, shortcuts, bridges, and stairs.
4. **Consequence** -- destroying the wrong material can collapse platforms, remove cover, or open routes for enemies.

**Reference Values:**
- Player walk speed: 6.0 tiles/s
- Player max jump: 4.0 tiles height, 4.0 tiles horizontal distance (walk), 6.0 tiles (boosted)
- Player collision box: 0.75 tiles wide, 1.0 tile tall
- Auto-fire rate: 6 rounds/s (10 frames between shots)
- Base weapon damage per shot: 1 HP (upgradeable to 2 at Tier 3+, 3 at Tier 5)
- 1 tile = 16x16 pixels, 60 fps
- Viewport: 20 tiles wide x 11.25 tiles tall

---

## 1. Destructible Material Types

All destructible terrain is tile-based. Each tile has a material type that determines its hit points, visual appearance, destruction behavior, and debris particles. Materials are grouped into four durability categories.

### 1.1 Durability Categories

| Category | HP Range | Shots to Destroy (Base Weapon) | Shots to Destroy (Tier 3 Weapon) | Shots to Destroy (Tier 5 Weapon) | Design Role |
|----------|----------|-------------------------------|----------------------------------|----------------------------------|-------------|
| Soft | 1-3 | 1-3 | 1-2 | 1 | Quickly cleared; encourages aggressive forward movement |
| Medium | 4-8 | 4-8 | 2-4 | 2-3 | Moderate resistance; forces brief pauses, rewards sustained fire |
| Hard | 9-15 | 9-15 | 5-8 | 3-5 | Significant resistance; demands weapon upgrades or alternate routes |
| Indestructible | Infinite | Cannot destroy | Cannot destroy | Cannot destroy | Level boundaries, structural anchors, puzzle constraints |

### 1.2 Soft Materials (1-3 HP)

| Material | HP | Debris Type | Debris Count | Debris Color | Audio Profile | Notes |
|----------|----|-------------|--------------|--------------|---------------|-------|
| Loose Dirt | 1 | Dust cloud + soil chunks | 4-6 | Brown/tan | Soft crumble | Fastest to clear, no structural role |
| Mud | 1 | Wet splatter particles | 3-5 | Dark brown | Wet thud | Sticks to nearby tiles visually for 30 frames |
| Dry Sand | 1 | Sand grain particles | 6-8 | Tan/yellow | Soft hiss | Falls as loose particles (gravity-affected debris) |
| Thin Wood | 2 | Splinter shards | 4-6 | Light brown | Crack/snap | Can catch fire from explosive weapons (see 2.4) |
| Drywall / Plaster | 2 | Powder cloud + panel chunks | 4-5 | White/grey | Crunch | Modern eras only (Industrial+) |
| Bone Pile | 2 | Bone fragment particles | 5-7 | Off-white | Dry rattle | Stone Age and Medieval only |
| Dried Clay | 3 | Clay chunk particles | 4-6 | Terracotta red | Hollow crack | Bronze Age signature material |
| Thatch / Straw | 1 | Loose fiber strands | 6-8 | Golden yellow | Soft rustle | Highly flammable, chain reaction candidate |
| Vine Growth | 2 | Leaf particles + vine segments | 4-6 | Green | Tearing sound | Can regrow over 300 frames (5.0 s) if not fully cleared |

### 1.3 Medium Materials (4-8 HP)

| Material | HP | Debris Type | Debris Count | Debris Color | Audio Profile | Notes |
|----------|----|-------------|--------------|--------------|---------------|-------|
| Cut Stone | 5 | Stone chunk particles | 4-6 | Grey | Heavy crack | Standard building block across many eras |
| Brick | 6 | Brick fragment + mortar dust | 5-7 | Red/brown | Masonry crumble | Industrial era signature |
| Concrete | 7 | Concrete chunks + rebar bits | 5-7 | Dark grey | Deep crunch | Shows rebar at 50% HP (damaged visual) |
| Wooden Beam (structural) | 4 | Large splinter shards | 3-5 | Brown | Heavy crack | Load-bearing; triggers collapse if destroyed (see 2.3) |
| Iron Gate | 6 | Metal shard particles + sparks | 4-6 | Dark grey + orange sparks | Metallic clang | Medieval era; sparks on each hit |
| Mud Brick | 4 | Clay dust + small chunks | 4-6 | Brown/tan | Dull thud | Bronze Age walls and structures |
| Metal Plating | 8 | Metal shard + rivet particles | 4-6 | Silver/grey + sparks | Sharp metallic ping | Industrial+ eras; sparks fly on each hit |
| Sandstone Block | 5 | Sand dust + stone chips | 5-7 | Tan/gold | Gritty crack | Used in Bronze Age and Medieval |
| Glass Panel | 4 | Glass shard particles (sharp) | 8-12 | Transparent/blue tint | Shatter | Digital era; dramatic shatter animation |
| Data Node | 6 | Pixel dissolve particles | 6-10 | Cyan/magenta | Digital glitch | Digital era; emits data corruption visual on death |

### 1.4 Hard Materials (9-15 HP)

| Material | HP | Debris Type | Debris Count | Debris Color | Audio Profile | Notes |
|----------|----|-------------|--------------|--------------|---------------|-------|
| Reinforced Stone | 10 | Heavy stone chunks | 4-6 | Dark grey | Deep rumble | Shows cracks at 60% and 30% HP |
| Steel Beam | 12 | Large metal shards + heavy sparks | 3-5 | Steel grey + bright sparks | Grinding metal | Industrial+ eras; load-bearing |
| Titanium Plating | 14 | Small metal shards (tough material) | 3-4 | Light silver + sparks | High-pitched ring | Space era; projectile ricochet visual until HP < 50% |
| Energy Shield | 9 | Energy dissipation particles | 8-12 | Color of shield (varies) | Electric crackle | Digital/Space eras; regenerates 1 HP per 120 frames if not fully destroyed |
| Alien Alloy | 13 | Glowing fragment particles | 4-6 | Purple/green bioluminescent | Alien hum + crack | Space era; emits light while intact |
| Crystalline Structure | 11 | Crystal shard particles | 6-8 | Prismatic / rainbow | Chime + shatter | Space era; refracts nearby weapon fire (visual only) |
| Reality Shard | 15 | Reality distortion particles | 4-8 | Shifting colors / glitch | Reality tear audio | Transcendent era; warps visuals in 2-tile radius while intact |
| Void Barrier | 12 | Void particles (dark energy) | 5-7 | Black with purple edges | Low void hum | Transcendent era; dims light in 3-tile radius |
| Quantum Lattice | 14 | Entangled particle pairs | 6-10 | White/blue oscillating | Quantum chirp | Transcendent era; destroying one tile damages linked twin tile for same amount |
| Force Field | 10 | Hexagonal grid dissolve | 6-8 | Translucent blue/green | Electric hum + pop | Space/Digital eras; visible hex grid pattern |

### 1.5 Indestructible Materials

| Material | Visual Indicator | Eras Present | Design Role |
|----------|-----------------|--------------|-------------|
| Bedrock | Darkest shade of stone, no cracks, subtle shimmer | All eras | Level floor boundary; prevents falling out of world |
| Level Boundary Wall | Solid dark edge with era-specific trim | All eras | Horizontal and vertical level limits |
| Structural Anchor | Glowing reinforcement brackets (gold/yellow outline) | All eras | Visually marked supports that hold up critical platforms; NEVER destructible |
| Ancient Monolith | Unique per-era monument texture | Era-specific | Story / lore elements embedded in terrain |
| Checkpoint Pillar | Glowing pillar with save icon | All eras | Checkpoint marker; also indestructible terrain |

**Visual Indicator Rule**: All indestructible materials must have a **gold/yellow outline glow** or **pulsing brightness** effect that distinguishes them from destructible materials. Players must NEVER be confused about whether a tile can be destroyed. When the player's auto-fire hits indestructible material, projectiles ricochet with a distinct "clank" sound and spark particle, providing immediate feedback.

---

## 2. Destruction Physics

### 2.1 Tile Destruction State Machine

Every destructible tile progresses through the following states:

```
INTACT (full HP)
  |
  v  [takes damage]
DAMAGED_1 (HP <= 75%)
  |
  v  [takes damage]
DAMAGED_2 (HP <= 40%)
  |
  v  [HP reaches 0]
DESTROYED (tile removed from collision grid)
  |
  v  [after debris animation completes]
CLEARED (empty space, traversable)
```

| State | Visual Change | Collision | Notes |
|-------|---------------|-----------|-------|
| INTACT | Original texture | Solid | Full material appearance |
| DAMAGED_1 | Hairline cracks overlay, 15% darker | Solid | Subtle visual damage |
| DAMAGED_2 | Major cracks, 30% darker, small particle leak | Solid | Obvious damage; player knows a few more shots will clear it |
| DESTROYED | Crumble/shatter animation plays (12 frames) | None (removed) | Debris particles spawn and fall with gravity |
| CLEARED | Empty tile, background visible | None | Tile is gone; pathable space |

### 2.2 Debris Particle System

When a tile enters DESTROYED state, a burst of debris particles spawns:

| Parameter | Value | Notes |
|-----------|-------|-------|
| Spawn origin | Center of destroyed tile | |
| Particle count | Per material table (see Section 1) | 3-12 depending on material |
| Particle size | 2x2 to 4x4 pixels | Randomized per particle |
| Initial velocity | Radial burst: 3.0-6.0 tiles/s outward | Direction biased away from damage source |
| Gravity | 0.015 tiles/frame^2 | Standard falling behavior |
| Rotation | 60-180 degrees/s per particle | Random spin for visual interest |
| Lifetime | 30-60 frames (0.5-1.0 s) | Fade out over last 10 frames |
| Collision with terrain | YES | Particles bounce off solid tiles (coefficient of restitution: 0.3) |
| Damage to player | NO | Debris is cosmetic only |
| Damage to enemies | NO | Debris is cosmetic only |

**Performance Budget**: Maximum 80 active debris particles on screen at any time. If the budget is exceeded, oldest particles are removed first (FIFO).

### 2.3 Structural Integrity and Collapse

Destroying load-bearing material causes unsupported material above to fall. This is a core strategic mechanic.

**Structural Rules:**

1. **Gravity Check**: When a destructible tile is destroyed, all tiles directly above it are checked for support. A tile is "supported" if:
   - It rests on a solid tile (destructible or indestructible) directly below it, OR
   - It is adjacent (left or right) to a tile that is itself supported (bridge rule -- max span of 3 tiles without a vertical support)

2. **Collapse Cascade**: Unsupported tiles fall downward at gravity rate (0.02 tiles/frame^2, terminal velocity 10.0 tiles/s). Falling tiles:
   - Deal 2 damage to the player on contact
   - Deal 3 damage to enemies on contact
   - Shatter on impact with solid ground (enter DESTROYED state, spawn debris)
   - Stack on solid ground if they are indestructible (bedrock)

3. **Collapse Delay**: After a support is removed, there is a **12-frame (0.2 s) delay** before collapse begins. During this delay, the unsupported tiles visually shake (1-pixel oscillation). This gives the player time to react.

4. **Maximum Collapse Column**: A single support removal can cause a maximum collapse of **8 tiles vertically**. Tiles above the 8th row are treated as having independent support (prevents catastrophic full-column collapses that break level geometry).

5. **Load-Bearing Indicators**: Structural tiles that support material above them display a subtle **downward arrow icon** or **weight line** at their base. This visual cue warns players that destroying this tile will cause a collapse.

| Collapse Parameter | Value |
|--------------------|-------|
| Gravity acceleration | 0.02 tiles/frame^2 |
| Terminal velocity | 10.0 tiles/s |
| Pre-collapse shake duration | 12 frames (0.2 s) |
| Shake amplitude | 1 pixel oscillation |
| Falling tile player damage | 2 HP |
| Falling tile enemy damage | 3 HP |
| Max cascade height | 8 tiles |
| Bridge span limit (unsupported horizontal) | 3 tiles |

### 2.4 Chain Reactions

Certain materials trigger chain reactions when destroyed:

| Material | Chain Reaction | Radius | Delay | Damage to Adjacent |
|----------|---------------|--------|-------|--------------------|
| Thin Wood (near fire source) | Catches fire, burns through | Spreads to adjacent wood tiles | 30 frames (0.5 s) per tile | Destroys adjacent wood tiles over 60 frames |
| Thatch / Straw | Ignites instantly if ANY fire-type weapon hits it | Spreads to all connected thatch | 18 frames (0.3 s) per tile | Burns through connected cluster |
| Glass Panel | Shatters; sends shard particles in 3-tile radius | 3 tiles | Instant | 1 damage to enemies in shard radius; no player damage |
| Data Node | Corruption pulse disables adjacent energy shields for 120 frames | 2 tiles | 6 frames (0.1 s) | Adjacent energy shields lose 3 HP |
| Quantum Lattice | Linked twin tile takes identical damage simultaneously | Unlimited range (linked pair) | 0 frames (instant) | Twin tile mirrors damage exactly |
| Explosive Barrel (placed object) | Explosion destroys all Soft tiles and damages Medium tiles in radius | 3-tile radius | 6 frames (0.1 s) | 4 damage to Medium/Hard tiles, instant destroy Soft tiles, 3 damage to player/enemies |

**Chain Reaction Limit**: A single chain reaction sequence can destroy a maximum of **12 tiles total** to prevent runaway destruction that breaks level geometry. After 12 tiles are destroyed in a single chain, the reaction stops.

### 2.5 Destruction Animation Timing

| Animation | Duration | Description |
|-----------|----------|-------------|
| Hit flash | 2 frames | White flash overlay on tile when struck |
| Crack appearance | 1 frame | Crack overlay added to tile sprite instantly on state transition |
| Crumble (Soft materials) | 8 frames | Tile crumbles inward, particles burst outward |
| Shatter (Glass, Crystal) | 6 frames | Tile explodes outward in shards |
| Collapse (Stone, Brick, Concrete) | 10 frames | Tile breaks into 2-3 large chunks that separate |
| Metal tear (Metal, Steel) | 12 frames | Tile warps and tears apart with spark shower |
| Energy dissipate (Shields, Force Fields) | 10 frames | Tile flickers and dissolves in energy particles |
| Digital dissolve (Data Nodes) | 8 frames | Tile pixelates and breaks into data fragments |
| Reality tear (Reality Shards) | 14 frames | Visual distortion ripple expands from tile center |

---

## 3. Era-Specific Material Palettes

Each of the 10 eras draws from the material pool with era-appropriate visual variants. The underlying HP and physics values remain consistent; only the visual skin and audio change.

### 3.1 Era Material Assignment Table

| Era | ID | Soft Materials | Medium Materials | Hard Materials | Signature Material | Era-Unique Mechanic |
|-----|-----|---------------|-----------------|----------------|-------------------|---------------------|
| Stone Age | 0 | Loose Dirt (1), Bone Pile (2), Dry Sand (1) | Cut Stone (5), Sandstone Block (5) | Reinforced Stone (10) | Bone Pile | Bone piles rattle as audio warning when enemies are near |
| Bronze Age | 1 | Dried Clay (3), Mud (1), Thatch (1) | Mud Brick (4), Sandstone Block (5), Wooden Beam (4) | Reinforced Stone (10) | Dried Clay | Clay walls crumble to reveal bronze weapon caches |
| Iron Age | 2 | Thin Wood (2), Loose Dirt (1), Vine Growth (2) | Cut Stone (5), Iron Gate (6), Wooden Beam (4) | Reinforced Stone (10), Steel Beam (12) | Iron Gate | Gates can be opened OR destroyed; opening preserves adjacent walls |
| Medieval | 3 | Thin Wood (2), Thatch (1), Vine Growth (2) | Cut Stone (5), Brick (6), Iron Gate (6), Wooden Beam (4) | Reinforced Stone (10), Steel Beam (12) | Cut Stone (castle variant) | Castle walls have murder holes that enemies fire through |
| Renaissance | 4 | Drywall/Plaster (2), Thin Wood (2), Vine Growth (2) | Brick (6), Cut Stone (5), Wooden Beam (4), Glass Panel (4) | Reinforced Stone (10), Steel Beam (12) | Glass Panel (stained glass) | Stained glass shatters create colored light beams (cosmetic) |
| Industrial | 5 | Drywall/Plaster (2), Thin Wood (2) | Brick (6), Concrete (7), Metal Plating (8), Wooden Beam (4) | Steel Beam (12), Reinforced Stone (10) | Metal Plating | Steam pipes behind walls release steam (push effect) when wall destroyed |
| Modern | 6 | Drywall/Plaster (2), Dry Sand (1) | Concrete (7), Metal Plating (8), Glass Panel (4) | Steel Beam (12), Titanium Plating (14) | Concrete | Rebar in concrete creates partial cover when tile is at DAMAGED_2 state |
| Digital | 7 | Drywall/Plaster (2) | Glass Panel (4), Data Node (6), Metal Plating (8) | Energy Shield (9), Titanium Plating (14), Force Field (10) | Data Node | Data nodes emit corruption pulse on destruction (see 2.4) |
| Space | 8 | Dry Sand (1, lunar variant) | Glass Panel (4), Metal Plating (8) | Alien Alloy (13), Crystalline Structure (11), Force Field (10), Titanium Plating (14) | Alien Alloy | Alien alloy emits bioluminescent light; destroying it darkens the area |
| Transcendent | 9 | None (all materials are Medium+) | Data Node (6), Glass Panel (4) | Reality Shard (15), Void Barrier (12), Quantum Lattice (14), Energy Shield (9) | Reality Shard | Destroying reality shards warps local space: nearby tiles shift position by 1 tile |

### 3.2 Era Progression Design Intent

| Era Range | Destruction Pacing | Strategic Depth | Player Note |
|-----------|-------------------|-----------------|-------------|
| Eras 0-2 (Stone through Iron Age) | Fast; mostly Soft/Medium materials | Low; blast through everything, learn basics | Teaches destruction as forward movement |
| Eras 3-5 (Medieval through Industrial) | Moderate; mix of all categories | Medium; structural collapse puzzles, cover usage | Teaches strategic preservation and structural mechanics |
| Eras 6-7 (Modern and Digital) | Varied; tactical material mix | High; chain reactions, data corruption, energy shields | Teaches chain reaction exploitation and shield timing |
| Eras 8-9 (Space and Transcendent) | Slow on Hard tiles; strategic pathing | Very High; quantum links, reality warps, void darkness | Mastery of all destruction mechanics combined |

---

## 4. Embedded Structures

Destruction reveals embedded structures hidden within the terrain. These are placed during procedural generation and are tagged in the tile map with a `RevealType` property.

### 4.1 Revealable Structure Types

| Structure | Description | Min Size | Max Size | Frequency Per Level | Visual When Revealed |
|-----------|-------------|----------|----------|--------------------|-----------------------|
| Pathway | Cleared horizontal corridor through terrain | 3 tiles long x 2 tiles tall | 12 tiles long x 3 tiles tall | 2-5 | Open passage with floor and ceiling visible |
| Bridge | Horizontal platform spanning a gap or pit below | 4 tiles long x 1 tile thick | 8 tiles long x 1 tile thick | 1-3 | Solid platform with gap visible below; era-appropriate railing texture |
| Stairs | Stepped surface for vertical traversal (ascending or descending) | 3 tiles run x 3 tiles rise | 6 tiles run x 6 tiles rise | 1-2 | Stepped blocks forming a staircase; each step is 1 tile wide x 1 tile tall |
| Hidden Room | Secret enclosed area with bonus content | 4x4 tiles | 8x6 tiles | 0-2 | Fully enclosed room with unique background; contains weapon cache or power-up |
| Shortcut | Alternate route that bypasses a section of the level | 5 tiles long x 2 tiles tall | 15 tiles long x 3 tiles tall | 0-2 | Narrow passage connecting two separate areas of the level |

### 4.2 Revelation Mechanics

When the player destroys the material concealing an embedded structure:

| Phase | Duration | Description |
|-------|----------|-------------|
| Last tile destroyed | Frame 0 | Final concealing tile enters DESTROYED state |
| Reveal flash | 6 frames | Brief white flash outline around the revealed structure |
| Camera hint | 18 frames | Camera pans slightly toward the revealed structure (max 3 tiles pan, then returns) |
| HUD indicator | 60 frames | Directional arrow on HUD points to revealed structure |
| Audio cue | Frame 0 | "Discovery" chime sound (pitch varies by structure type) |

### 4.3 Structure Contents

| Structure Type | Contents | Probability Distribution |
|----------------|----------|--------------------------|
| Pathway | Empty (traversable) | 100% empty |
| Bridge | Empty platform (traversable) | 100% empty |
| Stairs | Empty (climbable) | 100% empty |
| Hidden Room | Weapon upgrade | 35% |
| Hidden Room | Health pack (restores 3 HP) | 25% |
| Hidden Room | Score bonus (1000-5000 points) | 20% |
| Hidden Room | Special ammo cache (30 rounds of upgraded ammo) | 15% |
| Hidden Room | Cosmetic unlock (sprite variant) | 5% |
| Shortcut | Empty (traversable); occasionally 1-2 enemies guarding | 70% empty, 30% guarded |

### 4.4 Structure Concealment Rules

| Rule | Description |
|------|-------------|
| Minimum concealment depth | Structure must be hidden behind at least 2 tiles of destructible material |
| Maximum concealment depth | Structure must be reachable by destroying no more than 6 tiles |
| Concealment material | Must match the surrounding terrain (no obvious "destroy me" patches) |
| Subtle visual hint | 1-2 tiles in the concealing wall have a slight color variation (5-10% lighter) to reward observant players |
| Audio hint | When the player is within 4 tiles of a hidden room, a faint ambient audio cue plays (e.g., echoing wind, glittering chime) |
| Critical path independence | Hidden rooms and shortcuts are NEVER required to reach the level exit |
| Bridge/stairs critical path | Bridges and stairs CAN be on the critical path; the concealing material must be clearly destructible |

---

## 5. Strategic Preservation

Material that the player does NOT destroy serves strategic functions. The game encourages thoughtful shooting, not just holding down the fire button.

### 5.1 Preservation Benefits

| Benefit | Mechanic | Player Impact | Visual Indicator |
|---------|----------|---------------|------------------|
| Platform | Unblasted floor/terrain tiles remain solid and walkable | Player can stand on them to reach higher areas or cross gaps | Normal terrain appearance |
| Cover (horizontal) | Vertical wall tiles block enemy projectiles | Player can duck behind walls to avoid ranged enemy fire | Projectile impact particles on wall face |
| Cover (overhead) | Ceiling/roof tiles block vertical enemy attacks | Protection from above (aerial enemies, falling hazards) | Normal terrain appearance |
| Enemy barrier | Wall tiles block enemy ground movement | Enemies cannot path through intact walls; funnels enemies to openings | Enemies visually bump into wall and turn around |
| Structural support | Load-bearing tiles hold up platforms above | Preserving supports keeps platforms intact for traversal | Downward arrow / weight line icon (see 2.3) |
| Ramp creation | Destroying a support to INTENTIONALLY collapse material creates a debris ramp | Collapsed tiles stack and form a climbable slope | Fallen tiles rest at 45-degree angle if 3+ tiles collapse |

### 5.2 Auto-Fire Implications

Because the player's weapon auto-fires, strategic preservation requires:

| Mechanic | Implementation |
|----------|----------------|
| Aim direction | Player aims in the direction they face; angling is controlled by tilt/swipe |
| Cease fire zone | Player can tap a "hold fire" button to stop shooting temporarily |
| Directional firing | Player can aim up, forward, or diagonally (45 degrees); shooting downward is not possible while standing |
| Fire arc indicator | Subtle trajectory line shows where shots will land (2 tiles ahead only, fades quickly) |
| Friendly geometry warning | When aiming at a structural support, a brief yellow flash highlights the support and all tiles it holds up |

### 5.3 Preservation Scoring

Players are rewarded for strategic preservation at end-of-level scoring:

| Metric | Score Bonus | Condition |
|--------|-------------|-----------|
| Architect Bonus | 500 points | Completed level while preserving 50%+ of destructible tiles |
| Surgeon Bonus | 1000 points | Completed level while preserving 75%+ of destructible tiles |
| Demolition Bonus | 300 points | Destroyed 90%+ of destructible tiles |
| Pathfinder Bonus | 750 points | Found and entered at least 1 hidden room |
| Shortcut Bonus | 250 points | Used a revealed shortcut |

### 5.4 Preservation Puzzle Examples

| Puzzle Type | Description | Era Introduced |
|-------------|-------------|----------------|
| Floor Integrity | Player must cross a bridge made of destructible tiles while auto-fire threatens to destroy them; requires aim control | Era 1 (Bronze Age) |
| Support Dilemma | Destroying a wall to proceed forward will collapse the platform needed to reach a hidden room above | Era 3 (Medieval) |
| Enemy Funnel | Leaving walls intact forces enemies through a chokepoint where the player has a firing advantage | Era 2 (Iron Age) |
| Controlled Collapse | Destroy a specific support tile to collapse material into a gap, creating a makeshift bridge | Era 4 (Renaissance) |
| Shield Battery | An energy shield blocks the path; destroying the adjacent data node disables it, but also opens a wall letting enemies through | Era 7 (Digital) |
| Quantum Bridge | Two quantum lattice tiles are linked; destroying one destroys the other, which may be a platform the player needs later | Era 9 (Transcendent) |

---

## 6. Environmental Hazards (Non-Destructible)

Environmental hazards CANNOT be destroyed by the player's weapons. Projectiles pass through them or are absorbed with no effect. These hazards must be navigated around, jumped over, or timed through.

### 6.1 Era-Specific Hazard Table

| Hazard | Era(s) | Damage | Contact Type | Visual | Audio Tell | Hitbox |
|--------|--------|--------|-------------|--------|------------|--------|
| Lava Pool | 0 (Stone Age), 5 (Industrial furnaces) | 3 per tick | Continuous (15-frame cooldown) | Bright orange/red animated surface, 4-frame loop, glow | Bubbling + hiss | Full tile, surface level |
| Tar Pit | 0 (Stone Age), 1 (Bronze Age) | 1 per tick | Continuous (30-frame cooldown); also slows movement to 2.0 tiles/s | Black viscous surface, slow bubble animation | Thick gurgling | Full tile, surface level |
| Boiling Oil | 3 (Medieval) | 2 per tick | Continuous (20-frame cooldown) | Dark amber bubbling liquid, steam particles | Sizzling + steam hiss | Full tile; also drips from cauldrons (0.5 tile wide drip hitbox) |
| Poison Gas Cloud | 1 (Bronze Age), 3 (Medieval) | 1 per tick | Continuous (30-frame cooldown) | Green translucent cloud, drifting particles | Low hiss + cough SFX on contact | 3x2 tile zone, semi-transparent |
| Spike Pit | All eras (reskinned) | 2 per contact | On-contact, 30-frame cooldown | Pointed protrusions from floor, era-appropriate material | Metal/bone/crystal clink on land | 1 tile wide, 0.5 tiles tall |
| Electric Fence | 5 (Industrial), 6 (Modern) | 2 per contact | On-contact, 20-frame cooldown | Vertical electric arcs between posts, flickering | Buzzing hum (constant), zap on contact | 0.25 tiles wide, 3 tiles tall (between posts) |
| Laser Grid | 7 (Digital), 8 (Space) | 3 per contact | On-contact, instant (i-frames apply) | Thin red/blue beam lines in grid pattern | High-pitched whine (constant) | 1-pixel wide beams in grid; player must pass through gaps |
| Antimatter Pool | 9 (Transcendent) | 5 per contact | On-contact, instant (i-frames apply) | Black void surface with purple energy arcs, distortion effect | Deep vibrating hum + reality crackle | Full tile, surface level; visual distortion in 2-tile radius |
| Steam Vent | 5 (Industrial), 6 (Modern) | 1 per tick | Periodic (see 6.2) | Metal grate with steam column | Hiss before burst | 0.5 tiles wide, 3 tiles tall when active |
| Flame Jet | 3 (Medieval traps), 5 (Industrial) | 2 per tick | Periodic (see 6.2) | Wall-mounted nozzle with fire column | Whoosh before burst | 0.5 tiles wide, 3 tiles long |
| Gravity Anomaly | 9 (Transcendent) | 0 (no damage) | Continuous zone effect | Visual distortion ripple, floating debris particles | Low warbling hum | 4x4 tile zone; reverses player gravity while inside |
| Radiation Zone | 8 (Space) | 1 per 60 frames | Continuous while in zone | Green/yellow pulsing glow overlay | Geiger counter clicks | 4x3 tile zone |

### 6.2 Periodic / Moving Hazard Timing

| Hazard | Active Duration | Inactive Duration | Total Cycle | Tell Duration Before Active | Movement Pattern |
|--------|----------------|-------------------|-------------|-----------------------------|-----------------|
| Steam Vent | 45 frames (0.75 s) | 90 frames (1.5 s) | 135 frames (2.25 s) | 12 frames (grate glows) | Stationary |
| Flame Jet | 60 frames (1.0 s) | 90 frames (1.5 s) | 150 frames (2.5 s) | 18 frames (nozzle glows) | Stationary |
| Laser Grid | Always active | N/A | N/A | N/A | Pattern shifts every 180 frames (3.0 s); gaps reposition |
| Electric Fence | Always active | N/A | N/A | N/A | Stationary; some fences toggle on/off every 120 frames (2.0 s) |
| Poison Gas Cloud | Always present | N/A | N/A | Visible from 6+ tiles | Drifts horizontally at 0.5 tiles/s |
| Gravity Anomaly | Always active | N/A | N/A | Visible distortion from 3 tiles | Stationary |
| Boiling Oil (drip) | 10 frames (0.167 s) per drip | 80 frames (1.33 s) between drips | 90 frames (1.5 s) | Oil gathers on cauldron lip for 20 frames | Stationary (falls vertically) |

### 6.3 Hazard Interaction with Destruction

| Interaction | Behavior |
|-------------|----------|
| Destroying a wall next to lava/tar/antimatter | The liquid does NOT flow into the cleared space; it remains in its original tiles |
| Destroying a ceiling above a steam vent | The vent continues operating; steam visual extends upward into cleared space |
| Destroying a wall holding a flame jet | The flame jet is mounted on indestructible tile (always); surrounding wall can be destroyed |
| Collapsing material into lava/tar/antimatter | Falling material is instantly destroyed on contact with hazard; no stacking |
| Destroying terrain near electric fence posts | Posts are indestructible; fence remains active |
| Destroying tiles inside a gravity anomaly | Debris falls upward (follows reversed gravity in the anomaly zone) |

---

## 7. Density and Placement Rules for Procedural Generation

### 7.1 Destructible Material Density Per Zone Type

Each level is divided into zones. The procedural generator assigns destructible material density based on zone type:

| Zone Type | Destructible Tile % of Total Solid Tiles | Soft % | Medium % | Hard % | Indestructible % | Design Intent |
|-----------|------------------------------------------|--------|----------|--------|------------------|---------------|
| Intro | 40-50% | 60% | 30% | 5% | 5% | Easy entry, teaches destruction |
| Traversal | 55-65% | 40% | 40% | 15% | 5% | Heavy destruction for forward movement |
| Combat | 30-40% | 25% | 40% | 20% | 15% | More cover and barriers for firefights |
| Exploration | 60-75% | 45% | 35% | 15% | 5% | Dense terrain hiding secrets |
| Climax | 45-55% | 20% | 35% | 35% | 10% | Hard materials slow progress, increase tension |
| Boss Arena | 20-30% | 15% | 35% | 30% | 20% | Limited destruction; arena integrity matters |

### 7.2 Density Scaling by Era

As the game progresses through eras, material composition shifts toward harder materials:

| Era | Soft Tile Multiplier | Medium Tile Multiplier | Hard Tile Multiplier | Net Effect |
|-----|---------------------|----------------------|---------------------|------------|
| 0 (Stone Age) | 1.4x | 0.8x | 0.2x | Very fast destruction |
| 1 (Bronze Age) | 1.3x | 0.9x | 0.3x | Fast destruction |
| 2 (Iron Age) | 1.1x | 1.0x | 0.5x | Moderate |
| 3 (Medieval) | 1.0x | 1.0x | 0.7x | Balanced |
| 4 (Renaissance) | 0.9x | 1.1x | 0.8x | Slightly tougher |
| 5 (Industrial) | 0.8x | 1.2x | 0.9x | Tougher |
| 6 (Modern) | 0.7x | 1.1x | 1.0x | Hard material presence |
| 7 (Digital) | 0.6x | 1.0x | 1.2x | Energy shields appear |
| 8 (Space) | 0.5x | 0.9x | 1.4x | Heavily armored terrain |
| 9 (Transcendent) | 0.0x | 0.8x | 1.6x | No Soft materials; all Medium+ |

### 7.3 Embedded Structure Placement Rules

| Rule | Constraint |
|------|-----------|
| Pathway placement | Must connect two reachable points in the level; minimum 3 tiles from level boundaries |
| Bridge placement | Must span a gap of 3-6 tiles; both endpoints must rest on solid (indestructible or Hard) material |
| Stairs placement | Must connect two vertical levels separated by 3-6 tiles of height; run/rise ratio must be 1:1 |
| Hidden room placement | Must be adjacent to (but not on) the critical path; concealed by 2-4 tiles of destructible material |
| Shortcut placement | Must save the player at least 15 tiles of traversal compared to the main path |
| Minimum separation between structures | 8 tiles (prevents clustering) |
| Maximum structures per level | 10 (across all types combined) |
| Critical path structures (pathways/bridges/stairs) | Must be concealed by Soft or Medium materials only (never Hard) |
| Optional structures (hidden rooms/shortcuts) | May be concealed by any material type including Hard |

### 7.4 Environmental Hazard Placement Rules

| Rule | Constraint |
|------|-----------|
| Hazards per level (total) | 8-20 (scales with era: +1 per era beyond Era 0) |
| Hazards per screen width (20 tiles) | Maximum 3 active hazards |
| Minimum distance from level start | 10 tiles (safe introduction zone) |
| Minimum distance from checkpoint | 3 tiles |
| Minimum distance between hazards | 4 tiles (prevents hazard stacking) |
| Hazards on critical path | Must be avoidable by walking/jumping with no weapon required |
| Hazard-destruction exclusion zone | No destructible material within 1 tile of a non-destructible hazard (prevents confusing interactions) |
| Maximum consecutive hazard floor tiles | 3 tiles (lava, tar, spikes) -- must be jumpable |
| Periodic hazards on critical path | Must have safe passage window of at least 60 frames (1.0 s) |

### 7.5 Enemy-Terrain Integration

| Rule | Constraint |
|------|-----------|
| Enemy spawn points | Must be on solid (non-destructible or Hard) tiles; destroying the spawn tile does NOT prevent enemy spawn (enemy spawns on nearest valid tile) |
| Enemy pathfinding | Enemies navigate around intact terrain; destroying walls opens new enemy paths |
| Enemy cover usage | Ranged enemies will use intact destructible tiles as cover (peek and shoot) |
| Enemy-destructible interaction | Enemy projectiles do NOT destroy terrain (only player weapons destroy terrain) |
| Enemy density near destructible terrain | Maximum 3 enemies within 8 tiles of a dense destructible cluster (prevents overwhelming the player while they blast through) |

---

## 8. Validation Rules for Procedural Generation

These rules are enforced by the level generator after terrain and hazard placement. Any rule violation triggers re-generation of the affected section using deterministic fallback logic.

### 8.1 Completability Rules

| Rule ID | Rule | Validation Method |
|---------|------|-------------------|
| V-01 | **Critical Path Exists** | A* pathfinding from level start to level exit must succeed, assuming the player can destroy all destructible tiles on the path |
| V-02 | **Critical Path Destructible Budget** | The critical path must require destroying no more than 60% of the level's total destructible tiles |
| V-03 | **No Indestructible Blockage** | The critical path must never be blocked by indestructible material with no alternate route |
| V-04 | **Structural Collapse Safety** | No collapse triggered by destroying critical-path tiles can block the critical path itself |
| V-05 | **Hazard Avoidability** | Every non-destructible hazard on the critical path must have a movement sequence (walk/jump) that avoids all damage; validated by simulation |
| V-06 | **Reaction Distance** | Every hazard must be visible on-screen for at least 30 frames (0.5 s) before the player can contact it |
| V-07 | **Soft/Medium Critical Path** | All terrain on the critical path must be Soft or Medium material (Hard materials are optional paths only) |
| V-08 | **Bridge/Stairs Integrity** | Revealed bridges and stairs must rest on indestructible supports (player cannot accidentally destroy bridge supports) |
| V-09 | **Hidden Room Accessibility** | Every hidden room must be reachable by destroying surrounding material without triggering a collapse that blocks entry |
| V-10 | **Weapon Progression Feasibility** | In eras with Hard materials on optional paths, the expected weapon tier must be sufficient to destroy them within a reasonable time (under 30 seconds of sustained fire) |

### 8.2 Strategic Integrity Rules

| Rule ID | Rule | Description |
|---------|------|-------------|
| S-01 | **Floor Preservation Warning** | Any destructible floor tile that, if destroyed, would drop the player into a hazard must be a Medium or Hard material (not Soft -- prevents accidental self-harm) |
| S-02 | **Cover Availability** | In combat zones, at least 30% of terrain tiles between the player's path and enemy positions must be intact destructible or indestructible material |
| S-03 | **Support Visibility** | All structural support tiles must be visually marked (see 2.3) and must not be Soft material |
| S-04 | **No Trapped States** | Destroying any combination of tiles must not create a state where the player cannot reach the level exit; validated by exhaustive reachability check on critical supports |
| S-05 | **Enemy Barrier Minimum** | At least 2 intact wall segments per combat zone must block enemy ground movement paths |
| S-06 | **Preservation Reward Feasibility** | The Architect Bonus (50% preservation) must be achievable while still completing the critical path |

### 8.3 Combination Rules

| Combination | Allowed? | Reason |
|-------------|----------|--------|
| Lava Pool + Destructible Floor Above | YES (floor must be Medium+) | Player might accidentally open the floor; Medium+ material gives warning via damage states |
| Spike Pit + Destructible Wall Adjacent | YES | Destroying wall does not affect spikes |
| Chain Reaction + Structural Support | NO | Chain reactions must not propagate to structural support tiles |
| Gravity Anomaly + Destructible Ceiling | YES | Debris falls upward -- interesting but safe |
| Poison Gas + Dense Destructible Cluster | NO | Player needs clear sightlines while under damage pressure |
| Electric Fence + Adjacent Destructible | YES (1-tile buffer) | 1 tile of indestructible material must separate fence posts from destructible terrain |
| Antimatter Pool + ANY Destructible | YES (2-tile buffer) | 2 tiles of indestructible material must buffer antimatter from destructible terrain |
| Laser Grid + Destructible Terrain | NO | Lasers are mounted on indestructible geometry only |
| Multiple Chain Reactions in 10-tile radius | NO | Maximum 1 chain reaction source per 10-tile horizontal span |
| Collapsing Material + Boss Arena | NO | Boss arena terrain must be stable; no collapses allowed |

### 8.4 Global Limits

| Constraint | Maximum |
|------------|---------|
| Total destructible tiles per level (256 tiles wide) | 400 |
| Total indestructible tiles per level | 150 |
| Active debris particles on screen | 80 |
| Chain reaction tiles per event | 12 |
| Structural collapse height | 8 tiles |
| Embedded structures per level | 10 |
| Hidden rooms per level | 2 |
| Shortcuts per level | 2 |
| Environmental hazards per level | 20 |
| Active hazards per screen (20 tiles) | 3 |
| Enemies per combat zone near destructible clusters | 3 within 8 tiles |
| Simultaneous collapses on screen | 2 |
| Energy shield regeneration instances (active) | 3 |

---

## 9. Unity Implementation Constants

```csharp
public static class DestructibleEnvironmentConstants
{
    // ──────────────────────────────────────────────
    // Tile Grid
    // ──────────────────────────────────────────────
    public const int TilePixels               = 16;       // pixels per tile edge
    public const int FrameRate                 = 60;       // fixed timestep target
    public const float FixedDeltaTime          = 1f / 60f; // Time.fixedDeltaTime

    // ──────────────────────────────────────────────
    // Material HP Ranges
    // ──────────────────────────────────────────────
    public const int SoftHPMin                 = 1;
    public const int SoftHPMax                 = 3;
    public const int MediumHPMin               = 4;
    public const int MediumHPMax               = 8;
    public const int HardHPMin                 = 9;
    public const int HardHPMax                 = 15;

    // ──────────────────────────────────────────────
    // Specific Material HP
    // ──────────────────────────────────────────────
    public const int HP_LooseDirt              = 1;
    public const int HP_Mud                    = 1;
    public const int HP_DrySand                = 1;
    public const int HP_Thatch                 = 1;
    public const int HP_ThinWood               = 2;
    public const int HP_Drywall                = 2;
    public const int HP_BonePile               = 2;
    public const int HP_VineGrowth             = 2;
    public const int HP_DriedClay              = 3;
    public const int HP_MudBrick               = 4;
    public const int HP_WoodenBeam             = 4;
    public const int HP_GlassPanel             = 4;
    public const int HP_CutStone               = 5;
    public const int HP_SandstoneBlock         = 5;
    public const int HP_Brick                  = 6;
    public const int HP_IronGate               = 6;
    public const int HP_DataNode               = 6;
    public const int HP_Concrete               = 7;
    public const int HP_MetalPlating           = 8;
    public const int HP_EnergyShield           = 9;
    public const int HP_ReinforcedStone        = 10;
    public const int HP_ForceField             = 10;
    public const int HP_CrystallineStructure   = 11;
    public const int HP_SteelBeam              = 12;
    public const int HP_VoidBarrier            = 12;
    public const int HP_AlienAlloy             = 13;
    public const int HP_TitaniumPlating        = 14;
    public const int HP_QuantumLattice         = 14;
    public const int HP_RealityShard           = 15;

    // ──────────────────────────────────────────────
    // Weapon Damage Per Shot (by tier)
    // ──────────────────────────────────────────────
    public const int WeaponDamageTier1         = 1;
    public const int WeaponDamageTier3         = 2;
    public const int WeaponDamageTier5         = 3;
    public const int AutoFireRateFrames        = 10;  // frames between shots
    public const float AutoFireRatePerSec      = 6f;  // shots per second

    // ──────────────────────────────────────────────
    // Destruction State Thresholds (% HP remaining)
    // ──────────────────────────────────────────────
    public const float Damaged1Threshold       = 0.75f; // 75% HP -> DAMAGED_1
    public const float Damaged2Threshold       = 0.40f; // 40% HP -> DAMAGED_2

    // ──────────────────────────────────────────────
    // Debris Particles
    // ──────────────────────────────────────────────
    public const int DebrisParticleMin         = 3;
    public const int DebrisParticleMax         = 12;
    public const int DebrisPixelSizeMin        = 2;    // pixels
    public const int DebrisPixelSizeMax        = 4;    // pixels
    public const float DebrisBurstSpeedMin     = 3.0f; // tiles/s
    public const float DebrisBurstSpeedMax     = 6.0f; // tiles/s
    public const float DebrisGravity           = 0.015f; // tiles/frame^2
    public const float DebrisRestitution       = 0.3f; // bounce coefficient
    public const int DebrisLifetimeMin         = 30;   // frames
    public const int DebrisLifetimeMax         = 60;   // frames
    public const int DebrisFadeFrames          = 10;   // fade out over last N frames
    public const int MaxDebrisParticles        = 80;   // screen-wide budget

    // ──────────────────────────────────────────────
    // Structural Collapse
    // ──────────────────────────────────────────────
    public const float CollapseGravity         = 0.02f;  // tiles/frame^2
    public const float CollapseTerminalVel     = 10.0f;  // tiles/s
    public const int CollapseDelayFrames       = 12;     // pre-collapse shake
    public const int CollapseShakePixels       = 1;      // oscillation amplitude
    public const int FallingTilePlayerDamage   = 2;
    public const int FallingTileEnemyDamage    = 3;
    public const int MaxCollapseHeight         = 8;      // tiles
    public const int BridgeSpanLimit           = 3;      // unsupported horizontal tiles

    // ──────────────────────────────────────────────
    // Chain Reactions
    // ──────────────────────────────────────────────
    public const int MaxChainReactionTiles     = 12;
    public const int FireSpreadFrames          = 30;     // frames per tile
    public const int ThatchSpreadFrames        = 18;     // frames per tile
    public const int GlassShardRadius          = 3;      // tiles
    public const int GlassShardEnemyDamage     = 1;
    public const int DataNodeCorruptRadius     = 2;      // tiles
    public const int DataNodeShieldDamage      = 3;      // HP to adjacent shields
    public const int DataNodeDisableDuration   = 120;    // frames
    public const int ExplosiveBarrelRadius     = 3;      // tiles
    public const int ExplosiveBarrelDamage     = 4;      // to Medium/Hard tiles
    public const int ExplosiveBarrelEntityDmg  = 3;      // to player/enemies

    // ──────────────────────────────────────────────
    // Energy Shield Regeneration
    // ──────────────────────────────────────────────
    public const int ShieldRegenRate           = 1;      // HP per interval
    public const int ShieldRegenInterval       = 120;    // frames between regen ticks
    public const int MaxActiveShieldRegens     = 3;      // simultaneous regen instances

    // ──────────────────────────────────────────────
    // Vine Regrowth
    // ──────────────────────────────────────────────
    public const int VineRegrowthFrames        = 300;    // 5.0 s

    // ──────────────────────────────────────────────
    // Destruction Animation Durations (frames)
    // ──────────────────────────────────────────────
    public const int AnimHitFlash              = 2;
    public const int AnimCrumble               = 8;      // Soft materials
    public const int AnimShatter               = 6;      // Glass, Crystal
    public const int AnimCollapse              = 10;     // Stone, Brick, Concrete
    public const int AnimMetalTear             = 12;     // Metal, Steel
    public const int AnimEnergyDissipate       = 10;     // Shields, Force Fields
    public const int AnimDigitalDissolve       = 8;      // Data Nodes
    public const int AnimRealityTear           = 14;     // Reality Shards

    // ──────────────────────────────────────────────
    // Revelation (Embedded Structure Discovery)
    // ──────────────────────────────────────────────
    public const int RevealFlashFrames         = 6;
    public const int RevealCameraHintFrames    = 18;
    public const float RevealCameraMaxPan      = 3.0f;   // tiles
    public const int RevealHUDArrowFrames      = 60;

    // ──────────────────────────────────────────────
    // Environmental Hazard Damage
    // ──────────────────────────────────────────────
    public const int DmgLava                   = 3;
    public const int DmgTarPit                 = 1;
    public const int DmgBoilingOil             = 2;
    public const int DmgPoisonGas              = 1;
    public const int DmgSpikePit               = 2;
    public const int DmgElectricFence          = 2;
    public const int DmgLaserGrid              = 3;
    public const int DmgAntimatterPool         = 5;
    public const int DmgSteamVent              = 1;
    public const int DmgFlameJet               = 2;
    public const int DmgRadiationZone          = 1;

    // ──────────────────────────────────────────────
    // Environmental Hazard Cooldowns (frames)
    // ──────────────────────────────────────────────
    public const int CooldownLava              = 15;
    public const int CooldownTarPit            = 30;
    public const int CooldownBoilingOil        = 20;
    public const int CooldownPoisonGas         = 30;
    public const int CooldownSpikePit          = 30;
    public const int CooldownElectricFence     = 20;
    public const int CooldownRadiation         = 60;

    // ──────────────────────────────────────────────
    // Periodic Hazard Timing (frames)
    // ──────────────────────────────────────────────
    public const int SteamVentActive           = 45;
    public const int SteamVentInactive         = 90;
    public const int SteamVentTell             = 12;
    public const int FlameJetActive            = 60;
    public const int FlameJetInactive          = 90;
    public const int FlameJetTell              = 18;
    public const int LaserGridShiftCycle       = 180;
    public const int ElectricFenceToggle       = 120;
    public const int BoilingOilDripActive      = 10;
    public const int BoilingOilDripInactive    = 80;
    public const int BoilingOilDripTell        = 20;

    // ──────────────────────────────────────────────
    // Placement Constraints
    // ──────────────────────────────────────────────
    public const int SafeStartZoneTiles        = 10;     // no hazards in first N tiles
    public const int MinHazardCheckpointDist   = 3;      // tiles from checkpoint
    public const int MinHazardSeparation       = 4;      // tiles between hazards
    public const int MaxHazardsPerScreen       = 3;      // per 20-tile viewport
    public const int MaxHazardsPerLevel        = 20;
    public const int MaxConsecutiveHazardFloor = 3;      // jumpable
    public const int MinPeriodicSafeWindow     = 60;     // frames for passage
    public const int HazardDestructExclusion   = 1;      // tile buffer
    public const int AntimatterBuffer          = 2;      // tile buffer

    // ──────────────────────────────────────────────
    // Density Limits
    // ──────────────────────────────────────────────
    public const int MaxDestructiblePerLevel   = 400;
    public const int MaxIndestructiblePerLevel = 150;
    public const int MaxStructuresPerLevel     = 10;
    public const int MaxHiddenRoomsPerLevel    = 2;
    public const int MaxShortcutsPerLevel      = 2;
    public const int MaxSimultaneousCollapses  = 2;
    public const int MaxEnemiesNearCluster     = 3;      // within 8 tiles
    public const int MinStructureSeparation    = 8;      // tiles

    // ──────────────────────────────────────────────
    // Scoring Bonuses
    // ──────────────────────────────────────────────
    public const int ScoreArchitectBonus       = 500;    // 50%+ tiles preserved
    public const int ScoreSurgeonBonus         = 1000;   // 75%+ tiles preserved
    public const int ScoreDemolitionBonus      = 300;    // 90%+ tiles destroyed
    public const int ScorePathfinderBonus      = 750;    // found hidden room
    public const int ScoreShortcutBonus        = 250;    // used shortcut

    // ──────────────────────────────────────────────
    // Movement Effects (environmental)
    // ──────────────────────────────────────────────
    public const float TarPitMoveSpeed         = 2.0f;   // tiles/s (slowed)
    public const float PoisonGasDriftSpeed     = 0.5f;   // tiles/s
}

// ──────────────────────────────────────────────────
// Enums
// ──────────────────────────────────────────────────

public enum MaterialCategory
{
    Soft,          // 1-3 HP
    Medium,        // 4-8 HP
    Hard,          // 9-15 HP
    Indestructible // infinite HP
}

public enum TileState
{
    Intact,        // full HP
    Damaged1,      // HP <= 75%
    Damaged2,      // HP <= 40%
    Destroyed,     // HP = 0, animation playing
    Cleared        // tile removed
}

public enum MaterialType
{
    // Soft
    LooseDirt, Mud, DrySand, ThinWood, Drywall,
    BonePile, DriedClay, Thatch, VineGrowth,

    // Medium
    CutStone, Brick, Concrete, WoodenBeam, IronGate,
    MudBrick, MetalPlating, SandstoneBlock, GlassPanel, DataNode,

    // Hard
    ReinforcedStone, SteelBeam, TitaniumPlating, EnergyShield,
    AlienAlloy, CrystallineStructure, RealityShard, VoidBarrier,
    QuantumLattice, ForceField,

    // Indestructible
    Bedrock, LevelBoundary, StructuralAnchor, AncientMonolith,
    CheckpointPillar
}

public enum EraID
{
    StoneAge      = 0,
    BronzeAge     = 1,
    IronAge       = 2,
    Medieval      = 3,
    Renaissance   = 4,
    Industrial    = 5,
    Modern        = 6,
    Digital       = 7,
    Space         = 8,
    Transcendent  = 9
}

public enum RevealType
{
    None,
    Pathway,
    Bridge,
    Stairs,
    HiddenRoom,
    Shortcut
}

public enum HazardType
{
    LavaPool, TarPit, BoilingOil, PoisonGasCloud,
    SpikePit, ElectricFence, LaserGrid, AntimatterPool,
    SteamVent, FlameJet, GravityAnomaly, RadiationZone
}

public enum ChainReactionType
{
    None,
    FireSpread,        // wood/thatch ignition
    GlassShatter,      // shard burst
    DataCorruption,    // disables adjacent shields
    QuantumLink,       // twin tile mirror damage
    Explosion          // explosive barrel
}
```

---

## Appendix A: Visual Design Guidelines

All destructible terrain must be immediately readable at game speed (6 tiles/s scroll). The following visual principles apply:

1. **Durability Readability**: Players must be able to estimate material toughness at a glance.
   - Soft materials: lighter colors, visible textures (grain, fiber, cracks)
   - Medium materials: solid, uniform appearance, muted colors
   - Hard materials: darker tones, metallic sheen, glowing elements
   - Indestructible: gold/yellow outline glow, distinctly different from all other materials

2. **Damage Feedback**: Every hit on a destructible tile must produce:
   - 2-frame white flash on the tile
   - Hit spark particles (2-4 particles at impact point)
   - Audio hit sound (material-specific)
   - Screen shake: 0.5 pixels for Soft, 1 pixel for Medium, 1.5 pixels for Hard

3. **State Transitions**: Visual damage states (DAMAGED_1, DAMAGED_2) must be clearly distinct from INTACT. Cracks should follow material-appropriate patterns (organic cracks for stone, dents for metal, fracture lines for glass).

4. **Era Consistency**: All materials within an era must share a consistent color palette and texture style. Cross-era visual coherence is maintained through the 16-bit pixel art style.

5. **Particle Budget**: Maximum 80 active debris particles + 30 hazard particles = 110 total active particles on screen. Particles are pooled and recycled.

6. **Background Layer**: Destroyed tiles reveal the background parallax layer. Each era has a unique background that becomes visible as terrain is destroyed, creating a "window into the world" effect.

## Appendix B: Cross-Reference Index

| Topic | Reference Document |
|-------|-------------------|
| Player movement and jump physics | physics_spec.md (Assessment 4.1) |
| Camera behavior during reveals | camera_system_spec.md (Assessment 4.3) |
| Weapon stats and power-up integration | combat_system_spec.md (Assessment 4.4) |
| Enemy AI interaction with terrain | enemy_archetypes.md (Assessment 4.2) |
| Boss arena terrain rules | boss_patterns.md (Assessment 4.6) |
| Difficulty scaling and tier definitions | difficulty_curve_spec.md (Assessment 3.2) |
| Tileset art specifications | tileset_spec.md (Module 6) |
| Sound effect specifications | sfx_spec.md (Module 6) |
