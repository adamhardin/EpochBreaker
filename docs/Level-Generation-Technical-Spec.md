# Generative Level System - Technical Specification

## Overview

This specification defines the architecture for a deterministic, procedurally generated level system for a 16-bit side-scrolling mobile shooter on iOS. Each level is uniquely identifiable and reconstructible via a stable seed. The core gameplay loop centers on destructible environments: the player's weapons auto-fire, blasting through walls, rocks, and terrain to reveal hidden pathways, bridges, stairs, and secret areas. Strategic preservation of material creates platforms, cover, and structural aids, rewarding players who plan their destruction carefully. The system enables infinite replayability through shareable Level IDs while maintaining consistent difficulty and design quality across 10 eras of human civilization.

---

## Level ID Format & Seed Management

### Level ID Structure
```
LVLID_[VERSION]_[DIFFICULTY]_[ERA]_[SEED64]

Example: LVLID_1_2_3_9876543210ABCDEF
```

**Components:**
- `VERSION`: Format version for backwards compatibility (1 byte)
- `DIFFICULTY`: 0=Easy, 1=Normal, 2=Hard, 3=Extreme (1 byte)
- `ERA`: Historical era determining tileset, materials, and aesthetics (0-9, 1 byte)
- `SEED64`: 64-bit deterministic seed (8 bytes)

**Era Definitions:**
| Code | Era | Primary Aesthetic |
|------|-----|-------------------|
| 0 | Stone Age | Caves, raw earth, bone structures |
| 1 | Bronze Age | Mud-brick, early stonework, copper |
| 2 | Classical | Marble columns, aqueducts, carved stone |
| 3 | Medieval | Castle walls, timber, iron reinforcement |
| 4 | Renaissance | Ornate masonry, plaster, vaulted ceilings |
| 5 | Industrial | Brick, wrought iron, factory machinery |
| 6 | Modern | Concrete, steel beams, glass |
| 7 | Digital | Circuit-patterned alloys, server banks, fiber optics |
| 8 | Spacefaring | Hull plating, asteroid rock, zero-g debris |
| 9 | Transcendent | Exotic matter, energy barriers, reality-warping elements |

**Total ID Length**: 39 characters (human-readable hex format)

### Seed Generation Algorithm
```
seed = (timestamp_ms XOR user_id) + (play_count % 2^31)
```
- Ensures uniqueness across devices
- Allows player-specific level generation
- Deterministic on replay with same seed
- Shareable: any player can enter another player's Level ID to play the identical level

### Deterministic PRNG
```
Algorithm: xorshift64* (fast, deterministic, minimal state)
Initialization: seed = SEED64
Output: 64-bit unsigned integer per call
```

**Pseudocode:**
```
class XORShift64:
    def __init__(self, seed):
        self.state = seed if seed != 0 else DEFAULT_SEED

    def next(self):
        x = self.state
        x ^= x << 13
        x ^= x >> 7
        x ^= x << 17
        self.state = x
        return x

    def range(self, min, max):
        return min + (self.next() % (max - min))
```

**Properties:**
- Full 64-bit period (except seed=0)
- No correlation between successive outputs
- Zero external state modifications
- Cross-platform deterministic (no floating-point)

---

## Material System

### Material Categories

Every destructible tile has a material type that determines how it interacts with weapons and structural physics.

| Material Class | Hardness | Starting Weapon | Medium Weapon | Heavy Weapon | Example |
|----------------|----------|-----------------|---------------|--------------|---------|
| Soft | 1 | 1 hit | 1 hit | 1 hit | Dirt, mud, thatch, vines |
| Medium | 2 | 3 hits | 1 hit | 1 hit | Wood, sandstone, brick |
| Hard | 3 | Cannot break | 3 hits | 1 hit | Stone, concrete, iron |
| Reinforced | 4 | Cannot break | Cannot break | 3 hits | Steel, marble columns, hull plating |
| Indestructible | -- | Cannot break | Cannot break | Cannot break | Bedrock, load-bearing, boundary walls |

### Structural Integrity System

Destructible tiles participate in a structural integrity simulation:

```
struct DestructibleTile:
    material_class: uint8        // 1-4 (soft to reinforced), 0 = indestructible
    hit_points: uint8            // current HP
    max_hit_points: uint8        // based on material class
    is_load_bearing: bool        // if true, supports tiles above
    structural_group_id: uint16  // group for cascading collapse
    hidden_content: uint8        // 0=none, 1=pathway, 2=bridge, 3=stairs, 4=secret
```

**Cascading Collapse Rules:**
1. When a load-bearing tile is destroyed, all tiles in its structural group above it are checked
2. Unsupported tiles collapse downward one tick later (60 Hz physics step)
3. Collapse propagates upward and laterally within the structural group
4. Maximum cascade depth: 16 tiles (prevents runaway physics)
5. Collapsed tiles become rubble (non-collidable debris particles, cosmetic only)

### Era Material Distribution

Each era defines the probability distribution of material classes for destructible terrain:

```
MATERIAL_DISTRIBUTION[era][material_class] = probability (0.0 - 1.0)

Stone Age (0):    soft=0.60, medium=0.25, hard=0.10, reinforced=0.00, indestructible=0.05
Bronze Age (1):   soft=0.45, medium=0.35, hard=0.15, reinforced=0.00, indestructible=0.05
Classical (2):    soft=0.30, medium=0.30, hard=0.25, reinforced=0.10, indestructible=0.05
Medieval (3):     soft=0.20, medium=0.30, hard=0.30, reinforced=0.15, indestructible=0.05
Renaissance (4):  soft=0.20, medium=0.25, hard=0.30, reinforced=0.20, indestructible=0.05
Industrial (5):   soft=0.15, medium=0.20, hard=0.35, reinforced=0.25, indestructible=0.05
Modern (6):       soft=0.10, medium=0.15, hard=0.35, reinforced=0.35, indestructible=0.05
Digital (7):      soft=0.10, medium=0.15, hard=0.30, reinforced=0.40, indestructible=0.05
Spacefaring (8):  soft=0.10, medium=0.10, hard=0.30, reinforced=0.45, indestructible=0.05
Transcendent (9): soft=0.15, medium=0.15, hard=0.20, reinforced=0.35, indestructible=0.15
```

**Note:** Indestructible tiles are used sparingly for level boundaries and key structural anchors. Transcendent era has elevated indestructible usage for reality-warping geometry and energy barriers that serve as hard constraints.

---

## Level Schema

### Core Data Structure
```json
{
  "metadata": {
    "id": "LVLID_1_2_3_9876543210ABCDEF",
    "version": 1,
    "difficulty": 2,
    "era": 3,
    "seed": "9876543210ABCDEF",
    "generated_at": 1707000000,
    "estimated_duration_seconds": 120,
    "estimated_difficulty_rating": 6.5,
    "destruction_ratio_target": 0.65
  },
  "layout": {
    "width_tiles": 256,
    "height_tiles": 16,
    "start_x": 2,
    "start_y": 14,
    "zones": [
      {
        "id": "zone_0",
        "type": "intro",
        "x_start": 0,
        "x_end": 32,
        "difficulty_min": 1,
        "difficulty_max": 3,
        "destruction_density": 0.3
      },
      {
        "id": "zone_1",
        "type": "destruction",
        "x_start": 32,
        "x_end": 96,
        "difficulty_min": 3,
        "difficulty_max": 5,
        "destruction_density": 0.8
      }
    ]
  },
  "tilemap": {
    "tiles": ["/* array of tile IDs */"],
    "collision_map": ["/* array of collision flags */"],
    "destructible_map": ["/* array of DestructibleTile structs */"],
    "structural_groups": ["/* array of structural group definitions */"]
  },
  "entities": {
    "enemies": [
      {
        "id": "enemy_0",
        "type": "spearman",
        "x": 40,
        "y": 10,
        "difficulty_weight": 1,
        "behavior": "patrol",
        "uses_cover": true,
        "cover_tile_ids": ["tile_38_10", "tile_39_10"]
      }
    ],
    "weapon_drops": [
      {
        "id": "weapon_drop_0",
        "type": "medium_weapon",
        "x": 28,
        "y": 12,
        "hidden_behind_material": "soft",
        "required_before_tile_x": 48
      }
    ],
    "rewards": [
      {
        "id": "reward_0",
        "type": "health_pack",
        "x": 50,
        "y": 12,
        "hidden": false
      }
    ]
  },
  "destruction_paths": {
    "primary_paths": [
      {
        "id": "path_0",
        "waypoints": [
          {"x": 32, "y": 10},
          {"x": 40, "y": 8},
          {"x": 48, "y": 12}
        ],
        "required_weapon": "starting",
        "materials_encountered": ["soft", "soft", "medium"]
      }
    ],
    "alternate_paths": [
      {
        "id": "alt_path_0",
        "waypoints": [
          {"x": 32, "y": 14},
          {"x": 44, "y": 14},
          {"x": 48, "y": 12}
        ],
        "required_weapon": "medium",
        "materials_encountered": ["medium", "hard", "medium"],
        "reward": "secret_area"
      }
    ]
  },
  "checkpoints": [
    {
      "id": "checkpoint_0",
      "x": 0,
      "y": 14,
      "type": "level_start"
    },
    {
      "id": "boss_arena",
      "x": 240,
      "y": 10,
      "type": "boss_encounter"
    }
  ]
}
```

---

## Generation Pipeline

### Stage 1: Macro Layout (Global Structure)
**Input**: Difficulty, Era, RNG
**Output**: Zone definition, pacing, and destruction density map

1. **Determine Level Duration**
   ```
   base_duration = 90 seconds
   scaled_duration = base_duration * (1.0 + difficulty * 0.2)
   ```

2. **Zone Generation**
   - Intro Zone: 10% of level, difficulty 1-2, destruction density 0.3
   - Traversal Zone: 15% of level, difficulty 2-3, destruction density 0.4
   - Destruction Zone: 25% of level, difficulty 3-5, destruction density 0.8
   - Combat Zone: 20% of level, difficulty 4-6, destruction density 0.5
   - Boss Arena: 20% of level, difficulty 6-7, destruction density 0.6
   - Buffer/Transition: 10% of level, difficulty varies, destruction density 0.2

3. **Difficulty Curve**
   ```
   zone_difficulty(i) = min_difficulty + (max_difficulty - min_difficulty) * (i / num_zones)
   ```

4. **Destruction Density Curve**
   ```
   zone_destruction(i) = base_density * (1.0 + (i / num_zones) * 0.4)
   clamped to [0.2, 0.9]
   ```

### Stage 2: Destructible Terrain Placement

#### Terrain Generation
1. **Baseline Solid Mass**
   - Fill zone regions with solid destructible material based on era distribution
   - All randomness sourced from xorshift64* -- no Unity math functions for generation
   - Indestructible boundary walls at top, bottom, and level edges

2. **Embedded Pathway Carving**
   - For each zone, carve at least one primary path through the destructible mass
   - Primary path uses only soft/medium materials (breakable with starting weapon)
   - Alternate paths use harder materials requiring weapon upgrades
   - Pathways are carved but then filled with destructible material (hidden until destroyed)

3. **Structural Group Assignment**
   ```
   for each column of destructible tiles:
       assign structural_group_id based on connectivity
       mark lowest tile in group as load_bearing = true
       validate cascade depth <= 16
   ```

4. **Hidden Content Embedding**
   ```
   hidden_content_chance_by_zone:
       intro    = 0.05  (rare, teach the concept)
       traversal = 0.10
       destruction = 0.20  (frequent discoveries)
       combat   = 0.10
       boss     = 0.15

   hidden_content_type_distribution:
       pathway  = 0.40
       bridge   = 0.20
       stairs   = 0.20
       secret   = 0.20
   ```

5. **Material Density by Zone Type**
   ```
   intro_solid_fill     = 0.4   (sparse walls, easy to read)
   traversal_solid_fill = 0.5   (moderate terrain obstacles)
   destruction_fill     = 0.85  (dense walls to blast through)
   combat_solid_fill    = 0.5   (cover elements, firing lanes)
   boss_solid_fill      = 0.6   (arena elements, destructible hazards)
   ```

### Stage 3: Weapon Drop Placement

Weapon drops must be placed so the player always has sufficient firepower for the materials ahead.

1. **Weapon Tier Definitions**
   ```
   starting_weapon:  breaks soft (1 hit), medium (3 hits)
   medium_weapon:    breaks soft (1 hit), medium (1 hit), hard (3 hits)
   heavy_weapon:     breaks soft (1 hit), medium (1 hit), hard (1 hit), reinforced (3 hits)
   ```

2. **Drop Placement Rules**
   ```
   for each zone boundary:
       scan forward 32 tiles for hardest material on primary path
       if material_class > current_weapon_capability:
           place weapon_drop before zone boundary
           weapon_drop.type = minimum tier to break material
           weapon_drop.required_before_tile_x = first_hard_material_x
   ```

3. **Drop Accessibility**
   - Weapon drops must be reachable with current weapon tier
   - At least one drop per weapon tier must be on the primary path (not hidden)
   - Hidden drops can contain weapon upgrades as bonus (alternate path rewards)
   - Minimum 1 medium weapon drop before first hard material on any required path
   - Minimum 1 heavy weapon drop before first reinforced material on any required path

### Stage 4: Enemy Placement

1. **Enemy Count by Difficulty**
   ```
   enemy_count = 3 + (difficulty * 4)
   ```

2. **Era-Specific Enemy Types**
   ```
   Era 0 (Stone Age):     caveman, beast, rockslinger
   Era 1 (Bronze Age):    spearman, chariot_archer, bull_warrior
   Era 2 (Classical):     legionary, phalanx, war_elephant
   Era 3 (Medieval):      knight, crossbowman, siege_engine
   Era 4 (Renaissance):   musketeer, pikeman, cannon_crew
   Era 5 (Industrial):    rifleman, automaton, ironclad_turret
   Era 6 (Modern):        soldier, drone, tank_turret
   Era 7 (Digital):       firewall_bot, data_worm, logic_bomb
   Era 8 (Spacefaring):   astro_trooper, plasma_sentry, void_beast
   Era 9 (Transcendent):  echo_phantom, chrono_weaver, reality_shard
   ```

3. **Enemy-Terrain Integration**
   - Enemies can use destructible terrain as cover (`uses_cover = true`)
   - Destroying an enemy's cover tile exposes them (tactical advantage)
   - Some enemies are embedded behind destructible walls (ambush placement)
   - Ambush enemies activate when their enclosing wall is destroyed
   ```
   ambush_chance_by_zone:
       intro      = 0.00  (never, too early)
       traversal  = 0.05
       destruction = 0.15
       combat     = 0.20
       boss       = 0.10
   ```

4. **Enemy Type Selection**
   ```
   rng_value = rand() % 100
   if rng_value < 40:      enemy = era_easy_type      (e.g. caveman)
   else if rng_value < 70: enemy = era_medium_type     (e.g. beast)
   else if rng_value < 90: enemy = era_hard_type       (e.g. rockslinger)
   else:                   enemy = era_elite_type      (e.g. boss variant)
   ```

5. **Pathing & Behavior**
   - Place enemies on platforms they can traverse
   - Patrol range = zone width / 4
   - Avoid clustering (minimum 2 tile separation)
   - Cover-using enemies must be assigned to valid destructible tiles in their zone
   - Enemies behind destructible walls must have valid patrol space once revealed

### Stage 5: Reward & Pickup Placement

1. **Reward Distribution**
   - 1 health pack every 20 tiles
   - 1 damage boost every 50 tiles
   - Hidden rewards behind destructible walls (10% chance per eligible wall)

2. **Strategic Placement**
   - Place after difficult enemy clusters
   - Never on unavoidable damage tiles
   - Hidden rewards incentivize thorough destruction
   - Preservation rewards placed in areas where NOT destroying terrain grants tactical advantage

### Stage 6: Boss Arena Generation

**Boss Selection**
```
boss_type = era_boss_table[era]

Era 0: mammoth_chief
Era 1: bronze_colossus
Era 2: war_titan
Era 3: siege_lord
Era 4: gunpowder_king
Era 5: iron_leviathan
Era 6: steel_juggernaut
Era 7: core_sentinel
Era 8: void_dreadnought
Era 9: reality_breaker
```

**Boss Arena Layout**
```
arena_width = 80 tiles
arena_height = 12 tiles
platforms = 3 + difficulty
destructible_pillars = 2 + difficulty    // can be destroyed for tactical advantage
destructible_ceiling = era >= 5          // later eras have collapsible ceilings
attack_pattern = difficulty_based_pattern()
```

**Boss-Destruction Integration:**
- Destructible pillars provide cover from boss attacks
- Destroying pillars may open new attack angles against the boss
- Boss attacks can destroy terrain (boss_destruction_power = hard)
- Collapsing ceiling sections deal area damage to both player and boss
- Strategic preservation: keeping pillars intact provides cover but limits attack angles

**Boss Attack Waves**
- Wave 1: Basic projectiles + terrain-destroying sweeps (difficulty 1-2)
- Wave 2: Area denial + targeted terrain destruction (difficulty 3-4)
- Wave 3: Chase mechanics + environmental collapse (difficulty 5+)

---

## Zone Type Definitions

### Intro Zone
- **Purpose**: Teach destruction mechanics through light engagement
- **Destruction Density**: Low (0.3)
- **Materials**: Predominantly soft, occasional medium
- **Design Rules**:
  - First destructible wall within 4 tiles of spawn (immediate feedback)
  - First hidden pathway within 12 tiles (teach discovery)
  - No ambush enemies
  - No hard or reinforced materials
  - At least one obvious "shoot this wall" visual cue via era-specific cracked texture

### Traversal Zone
- **Purpose**: Platforming with light destruction elements
- **Destruction Density**: Low-Medium (0.4)
- **Materials**: Mix of soft and medium
- **Design Rules**:
  - Gaps require destroying terrain to create bridges or reveal stairs
  - At least one preservation opportunity (keeping a platform intact aids traversal)
  - Enemies are sparse, placed to threaten during platforming
  - No mandatory hard material destruction

### Destruction Zone
- **Purpose**: Heavy walls to blast through, puzzle-like route finding
- **Destruction Density**: High (0.8)
- **Materials**: Full era distribution, dense solid masses
- **Design Rules**:
  - Multiple valid paths through the mass (minimum 2 primary, 1 alternate)
  - Embedded pathways revealed by destruction
  - Structural integrity plays a role (cascade collapses open shortcuts)
  - Weapon drops placed before sections requiring upgraded weapons
  - Hidden areas with bonus rewards accessible via alternate destruction routes
  - Strategic choice: fast path (blast everything) vs. careful path (preserve platforms)

### Combat Zone
- **Purpose**: Enemies using destructible terrain as cover
- **Destruction Density**: Medium (0.5)
- **Materials**: Medium and hard predominant (functional cover)
- **Design Rules**:
  - Enemies positioned behind destructible cover
  - Destroying cover exposes enemies but removes player cover options
  - Ambush enemies behind walls
  - Preservation valuable: keeping walls intact provides defensive positions
  - Multiple engagement angles based on what the player chooses to destroy
  - At least one flanking route through destructible terrain

### Boss Arena
- **Purpose**: Destructible elements integrated into boss encounter
- **Destruction Density**: Medium-High (0.6)
- **Materials**: Era-specific, including reinforced elements
- **Design Rules**:
  - Destructible pillars and terrain features
  - Boss can destroy terrain with attacks
  - Cascade collapses as dynamic hazards
  - Strategic preservation creates cover; strategic destruction opens attack angles
  - Arena must remain completable regardless of destruction state (no soft-locks)

---

## Era-Specific Generation Rules

### Era 0: Stone Age
- **Visual**: Cave interiors, raw earth, bone scaffolding, torchlight
- **Terrain Character**: Organic, uneven, cave-like formations
- **Materials**: 60% soft (dirt, mud), 25% medium (packed earth, bone), 10% hard (dense rock), 5% indestructible (bedrock)
- **Special Features**: Stalactites (collapse when supports destroyed), vine bridges (soft material), bone gates
- **Structural Behavior**: Low structural complexity; mostly independent blocks, minimal cascading

### Era 3: Medieval
- **Visual**: Castle walls, timber supports, iron-banded doors, arrow slits
- **Terrain Character**: Dense, structured, fortress-like with corridors
- **Materials**: 20% soft (thatch, hay), 30% medium (wood, mortar), 30% hard (stone block), 15% reinforced (iron-banded stone), 5% indestructible
- **Special Features**: Portcullis gates (reinforced, rewarding to destroy), arrow slit windows (firing lanes), tower sections with cascading collapse
- **Structural Behavior**: High structural complexity; castle walls have deep structural groups, destroying foundations can collapse entire tower sections

### Era 6: Modern
- **Visual**: Urban concrete, steel I-beams, glass windows, rebar
- **Terrain Character**: Grid-like, angular, dense with hard materials
- **Materials**: 10% soft (drywall, glass), 15% medium (wood framing), 35% hard (concrete), 35% reinforced (steel beam), 5% indestructible
- **Special Features**: Glass windows (soft, satisfying mass destruction), rebar-reinforced concrete (hard outer shell, soft inner), steel beams (load-bearing reinforced)
- **Structural Behavior**: Very high structural complexity; steel beams are key load-bearing elements, destroying them causes multi-story cascade collapses

### Era 9: Transcendent
- **Visual**: Exotic matter, shimmering energy barriers, reality-folded geometry
- **Terrain Character**: Non-linear feel, floating platforms, energy walls
- **Materials**: 15% soft (unstable matter), 15% medium (condensed energy), 20% hard (crystallized void), 35% reinforced (exotic alloy), 15% indestructible (reality anchors)
- **Special Features**: Energy barriers (indestructible but can be deactivated by destroying linked nodes), reality folds (destroying a tile may cause distant tiles to also break), phase-shift walls (alternate between destructible and indestructible on a timer)
- **Structural Behavior**: Unique; structural groups can span non-adjacent tiles via reality links, cascade effects can jump across the level

---

## Validation & Constraints

### Completability Validation
```pseudocode
function validate_completability(level):
    // Simulate minimum-loadout playthrough
    weapon = STARTING_WEAPON
    visited = set()
    queue = [(level.start_pos, weapon)]
    weapon_drops_collected = set()

    while queue not empty:
        (pos, current_weapon) = queue.pop()
        if pos in visited: continue
        visited.add(pos)

        // Collect weapon drops at this position
        for drop in level.weapon_drops_at(pos):
            current_weapon = max(current_weapon, drop.tier)
            weapon_drops_collected.add(drop.id)

        for neighbor in get_reachable_positions(pos, level, current_weapon):
            queue.push((neighbor, current_weapon))

    if level.goal_pos not in visited:
        return FAILED_UNREACHABLE

    return SUCCESS
```

### Destruction Path Validation
```pseudocode
function validate_destruction_paths(level):
    for zone in level.zones:
        paths = find_all_destruction_paths(zone, level)

        // Must have at least one path completable with starting weapon
        starting_weapon_paths = filter(paths, p => p.required_weapon <= STARTING)
        if len(starting_weapon_paths) == 0:
            return FAILED_NO_STARTING_PATH

        // Must have at least 2 distinct paths total (no single solution)
        if len(paths) < 2:
            return FAILED_SINGLE_SOLUTION

    return SUCCESS
```

### Weapon Progression Validation
```pseudocode
function validate_weapon_progression(level):
    // Walk the primary path and track weapon availability
    weapon = STARTING_WEAPON

    for tile_x in range(0, level.width):
        // Check for weapon drops at this x
        for drop in level.weapon_drops_at_x(tile_x):
            if drop.on_primary_path:
                weapon = max(weapon, drop.tier)

        // Check if any required-path material needs a higher weapon
        for material in level.required_path_materials_at_x(tile_x):
            if not can_break(weapon, material):
                return FAILED_INSUFFICIENT_WEAPON(tile_x, material, weapon)

    return SUCCESS
```

### Soft-Lock Validation
```pseudocode
function validate_no_softlocks(level):
    // For every possible destruction state, verify player can progress
    // Approximation: test critical destruction points

    critical_tiles = get_load_bearing_tiles(level)

    for tile in critical_tiles:
        simulated_level = deep_copy(level)
        simulate_destruction(simulated_level, tile)
        apply_cascade(simulated_level)

        if not validate_completability(simulated_level):
            // Check if player can reach this tile; if unreachable, it's fine
            if tile_is_reachable(tile, level):
                return FAILED_SOFTLOCK(tile)

    return SUCCESS
```

### Strategic Preservation Validation
```pseudocode
function validate_preservation_opportunities(level):
    preservation_count = 0

    for zone in level.zones:
        // A preservation opportunity exists when NOT destroying a tile
        // provides a measurable gameplay advantage (cover, platform, shortcut)
        opportunities = find_preservation_benefits(zone, level)
        preservation_count += len(opportunities)

    if preservation_count < level.num_zones:
        return FAILED_INSUFFICIENT_STRATEGY

    return SUCCESS
```

### Difficulty Validation
```pseudocode
function validate_difficulty(level):
    calculated_difficulty = 0

    // Count enemy difficulty contributions
    for enemy in level.enemies:
        calculated_difficulty += enemy.difficulty_weight

    // Add destruction complexity
    for zone in level.zones:
        calculated_difficulty += zone.destruction_density * DESTRUCTION_WEIGHT
        calculated_difficulty += count_hard_materials(zone) * HARD_MATERIAL_WEIGHT

    // Add traversal difficulty
    for gap in level.gaps:
        calculated_difficulty += (gap.width / max_jump_distance)

    expected_difficulty = level.metadata.difficulty_rating

    if abs(calculated_difficulty - expected_difficulty) > TOLERANCE:
        return FAILED_DIFFICULTY_MISMATCH

    return SUCCESS
```

### Safety Checks
- [ ] Start position accessible
- [ ] Boss arena reachable with starting weapon on primary path
- [ ] No impossible jumps (max_gap < player_max_jump)
- [ ] Sufficient checkpoints (minimum 3)
- [ ] Reward distribution balanced
- [ ] Enemy spawn count within limits
- [ ] No tile overlapping with entities
- [ ] Every zone has at least one valid destruction path with starting weapon
- [ ] Weapon drops placed before harder materials on all required paths
- [ ] No soft-locks from any reachable destruction sequence
- [ ] Strategic preservation opportunities in every zone
- [ ] Structural cascade depth never exceeds 16 tiles
- [ ] Boss arena completable regardless of terrain destruction state
- [ ] Hidden areas are truly optional (not required for completion)

---

## Performance Targets

### Generation Performance

The canonical performance targets are defined here. All other documents defer to these values.

| Device | Target (avg) | P95 | P99 | Max Level Size |
|--------|-------------|-----|-----|----------------|
| iPhone 14+ | 50 ms | 80 ms | 120 ms | 512x32 tiles |
| iPhone 12/13 | 80 ms | 120 ms | 150 ms | 400x32 tiles |
| iPhone 11 (baseline) | 100 ms | 150 ms | 200 ms | 256x32 tiles |

Note: iPhone XR and older are not supported. iPhone 11 is the minimum target device.
The commonly cited "< 150ms" target refers to the P95 on the baseline device (iPhone 11).

### Runtime Destruction Performance
| Metric | Target |
|--------|--------|
| Single tile destruction | < 0.5 ms |
| Cascade collapse (max 16 depth) | < 2 ms |
| Structural integrity recalculation | < 1 ms |
| Debris particle spawn (per tile) | < 0.1 ms |

### Serialization
- Level data to JSON: < 80 KB (increased from 50 KB due to destructible map data)
- Tile atlas texture: 256x2048 px (2 MB shared per era)
- Destructible state bitmask: < 4 KB (runtime only, not serialized in level ID)
- Per-level memory overhead: < 3 MB (increased to accommodate structural integrity data)

---

## Reproducibility Guarantee

### Testing Suite
```
Test 1: Same Seed, Same Level
  for i in 1..10000:
    level_a = generate_level(seed=12345, difficulty=2, era=3)
    level_b = generate_level(seed=12345, difficulty=2, era=3)
    assert level_a == level_b

Test 2: Different Seed, Different Level
  level_1 = generate_level(seed=12345, difficulty=2, era=3)
  level_2 = generate_level(seed=12346, difficulty=2, era=3)
  assert level_1 != level_2  (allow <2% similarity)

Test 3: Cross-Platform Determinism
  // Run on iOS device, macOS simulator
  level_ios = generate(seed, difficulty, era)
  level_mac = generate(seed, difficulty, era)
  assert level_ios == level_mac

Test 4: Performance Under Stress
  for i in 1..100:
    level = generate_level(seed=random(), difficulty=random(), era=random())
    assert generation_time < PLATFORM_TARGET_MS

Test 5: Destruction Determinism
  level = generate_level(seed=12345, difficulty=2, era=3)
  // Same sequence of destruction inputs must produce identical state
  state_a = simulate_destruction_sequence(level, inputs=[...])
  state_b = simulate_destruction_sequence(level, inputs=[...])
  assert state_a == state_b

Test 6: Completability Across All Eras
  for era in 0..9:
    for difficulty in 0..3:
      for i in 1..100:
        level = generate_level(seed=random(), difficulty=difficulty, era=era)
        assert validate_completability(level) == SUCCESS
        assert validate_destruction_paths(level) == SUCCESS
        assert validate_weapon_progression(level) == SUCCESS
        assert validate_no_softlocks(level) == SUCCESS
        assert validate_preservation_opportunities(level) == SUCCESS

Test 7: Level ID Shareability
  // Generate on device A, reconstruct on device B from ID only
  id = "LVLID_1_2_3_9876543210ABCDEF"
  level_a = generate_from_id(id)
  level_b = generate_from_id(id)
  assert level_a == level_b
```

---

## Implementation Roadmap

### Phase 1: Core PRNG & Seeding (Week 1)
- [ ] Implement xorshift64* RNG
- [ ] Build Level ID parser/encoder (updated ERA field)
- [ ] Write seed derivation logic
- [ ] Unit tests for determinism

### Phase 2: Material System & Structural Integrity (Week 2)
- [ ] Implement material class definitions and hardness values
- [ ] Build structural group assignment algorithm
- [ ] Implement cascading collapse simulation
- [ ] Cascade depth limiter (max 16)
- [ ] Unit tests for structural integrity

### Phase 3: Terrain Generation (Week 3)
- [ ] Implement solid mass generation with era-specific material distribution
- [ ] Build embedded pathway carving system
- [ ] Hidden content embedding (pathways, bridges, stairs, secrets)
- [ ] Structural group assignment pass
- [ ] Difficulty-scaled destruction density

### Phase 4: Weapon & Enemy Placement (Week 4)
- [ ] Weapon drop placement algorithm with progression validation
- [ ] Era-specific enemy type selection and placement
- [ ] Enemy-terrain integration (cover assignment, ambush placement)
- [ ] Boss arena generation with destructible elements

### Phase 5: Validation Suite (Week 5)
- [ ] Completability validator (minimum weapon loadout simulation)
- [ ] Destruction path validator (multiple paths, no single solution)
- [ ] Weapon progression validator
- [ ] Soft-lock validator (critical tile destruction testing)
- [ ] Strategic preservation validator
- [ ] Difficulty estimator
- [ ] Performance profiling

### Phase 6: Era Content (Weeks 6-7)
- [ ] Era-specific tile sets and visual assets (10 eras)
- [ ] Era-specific enemy definitions and behaviors
- [ ] Era-specific special features (stalactites, portcullis, energy barriers, etc.)
- [ ] Era-specific boss encounters
- [ ] Per-era generation rule tuning

### Phase 7: Integration & Polish (Week 8)
- [ ] Level loader in game engine with runtime destruction state
- [ ] Level ID sharing (JSON export, copy-paste, QR code)
- [ ] Replay system (records destruction sequence for deterministic replay)
- [ ] Analytics tracking (destruction patterns, path choices, completion rates)
- [ ] Cross-device testing across all supported iPhones

---

## Future Enhancements

1. **Dynamic Difficulty via Destruction**: Adjust material hardness on-the-fly based on player weapon usage and destruction patterns
2. **Multiplayer Destruction**: Shared level seeds with cooperative or competitive destruction between players
3. **Community Levels**: Integration with backend for Level ID sharing, leaderboards per seed
4. **AI-Enhanced Generation**: Use ML models to analyze player destruction patterns and generate levels that match preferred playstyles
5. **Era Mashups**: Levels that transition between eras mid-level, blending material types and enemy rosters
6. **Destruction Replays**: Sharable replay files that record and play back a player's destruction path through a level

---

## References & Research

- xorshift64*: Marsaglia (2003), "Xorshift RNGs"
- Procedural Content Generation: Togelius et al. (2011), "Search-Based Procedural Content Generation"
- Wave Function Collapse: Gumin (2016), constraint-based tile generation
- Game Design Theory: Hunicke, LeBlanc, Zubek (2004), MDA Framework
- "Artificial Intelligence and Games": Yannakakis & Togelius (2018)
- Destructible Terrain: Red Faction (2001), Volition Inc., structural mesh destruction
- Structural Simulation: "Real-Time Structural Analysis for Games", GDC 2015

**Determinism Note:** Unity's `Mathf.PerlinNoise` is explicitly excluded from the
generation pipeline. All randomness must flow through the xorshift64* PRNG to
guarantee cross-platform, cross-version determinism. Destruction physics (cascade
collapse) must also be deterministic: integer-only math, fixed-tick simulation at
60 Hz, no floating-point accumulation.
