# Generative Level System - Technical Specification

## Overview

This specification defines the architecture for a deterministic, procedurally generated level system where each level is uniquely identifiable and reconstructible via a stable seed. The system enables infinite replayability while maintaining consistent difficulty and design quality.

---

## Level ID Format & Seed Management

### Level ID Structure
```
LVLID_[VERSION]_[DIFFICULTY]_[BIOME]_[SEED64]

Example: LVLID_1_2_0_9876543210ABCDEF
```

**Components:**
- `VERSION`: Format version for backwards compatibility (1 byte)
- `DIFFICULTY`: 0=Easy, 1=Normal, 2=Hard, 3=Extreme (1 byte)
- `BIOME`: Tile set and aesthetic (0=Forest, 1=Cavern, 2=Sky, 3=Volcanic) (1 byte)
- `SEED64`: 64-bit deterministic seed (8 bytes)

**Total ID Length**: 39 characters (human-readable hex format)

### Seed Generation Algorithm
```
seed = (timestamp_ms XOR user_id) + (play_count % 2^31)
```
- Ensures uniqueness across devices
- Allows player-specific level generation
- Deterministic on replay with same seed

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

## Level Schema

### Core Data Structure
```json
{
  "metadata": {
    "id": "LVLID_1_2_0_9876543210ABCDEF",
    "version": 1,
    "difficulty": 2,
    "biome": 0,
    "seed": "9876543210ABCDEF",
    "generated_at": 1707000000,
    "estimated_duration_seconds": 120,
    "estimated_difficulty_rating": 6.5
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
        "difficulty_max": 3
      },
      {
        "id": "zone_1",
        "type": "combat",
        "x_start": 32,
        "x_end": 96,
        "difficulty_min": 3,
        "difficulty_max": 5
      }
    ]
  },
  "tilemap": {
    "tiles": [/* array of tile IDs */],
    "collision_map": [/* array of collision flags */]
  },
  "entities": {
    "enemies": [
      {
        "id": "enemy_0",
        "type": "slime",
        "x": 40,
        "y": 10,
        "difficulty_weight": 1,
        "behavior": "patrol"
      }
    ],
    "rewards": [
      {
        "id": "reward_0",
        "type": "health_potion",
        "x": 50,
        "y": 12
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
**Input**: Difficulty, Biome, RNG
**Output**: Zone definition and pacing

1. **Determine Level Duration**
   ```
   base_duration = 90 seconds
   scaled_duration = base_duration * (1.0 + difficulty * 0.2)
   ```

2. **Zone Generation**
   - Intro Zone: 10% of level, difficulty 1-2
   - Traversal Zone: 20% of level, difficulty 2-3
   - Combat Zone: 30% of level, difficulty 3-5
   - Climax Zone: 20% of level, difficulty 4-6
   - Boss Arena: 20% of level, difficulty 6-7

3. **Difficulty Curve**
   ```
   zone_difficulty(i) = min_difficulty + (max_difficulty - min_difficulty) * (i / num_zones)
   ```

### Stage 2: Micro Layout (Tile & Entity Placement)

#### Tilemap Generation
1. **Baseline Terrain**
   - Procedural platform placement using deterministic PRNG (anchor-interpolation method)
   - All randomness sourced from xorshift64* -- no Unity math functions for generation
   - Ensure continuous path from start to exit
   - Add gaps scaled to difficulty

2. **Platform Density by Zone**
   ```
   intro_density = 0.8      (safe, well-spaced)
   combat_density = 0.6     (challenging jumps)
   climax_density = 0.4     (extreme difficulty)
   ```

3. **Collision Detection**
   - Validate every platform is reachable from previous
   - Mark dangerous edges (spike zones)
   - Ensure no dead-end platforms

#### Enemy Placement
1. **Enemy Count by Difficulty**
   ```
   enemy_count = 3 + (difficulty * 4)
   ```

2. **Enemy Type Selection**
   ```
   rng_value = rand() % 100
   if rng_value < 40:      enemy = "slime"     (easy)
   else if rng_value < 70: enemy = "goblin"    (medium)
   else if rng_value < 90: enemy = "skeleton"  (hard)
   else:                   enemy = "warlock"   (extreme)
   ```

3. **Pathing & Behavior**
   - Place enemies on platforms they can traverse
   - Patrol range = zone width / 4
   - Avoid clustering (minimum 2 tile separation)

#### Reward Placement
1. **Reward Distribution**
   - 1 health potion every 20 tiles
   - 1 damage boost every 50 tiles
   - Hidden rewards in off-path areas (5% chance)

2. **Strategic Placement**
   - Place after difficult enemy clusters
   - Never on unavoidable damage tiles

### Stage 3: Boss Arena Generation

**Boss Selection**
```
if difficulty < 3:     boss = "giant_spider"
else if difficulty < 5: boss = "fire_elemental"
else:                   boss = "dark_knight"
```

**Boss Arena Layout**
```
arena_width = 80 tiles
arena_height = 12 tiles
platforms = 3 + difficulty
attack_pattern = difficulty_based_pattern()
```

**Boss Attacks**
- Wave 1: Basic projectiles (difficulty 1-2)
- Wave 2: Area denial (difficulty 3-4)
- Wave 3: Chase mechanics (difficulty 5+)

---

## Validation & Constraints

### Reachability Validation
```pseudocode
function validate_reachability(level):
    visited = set()
    queue = [level.start_pos]
    
    while queue not empty:
        pos = queue.pop()
        if pos in visited: continue
        visited.add(pos)
        
        for neighbor in get_adjacent_platforms(pos, level):
            queue.push(neighbor)
    
    if level.goal_pos not in visited:
        return FAILED_UNREACHABLE
    
    return SUCCESS
```

### Difficulty Validation
```pseudocode
function validate_difficulty(level):
    calculated_difficulty = 0
    
    // Count enemy difficulty contributions
    for enemy in level.enemies:
        calculated_difficulty += enemy.difficulty_weight
    
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
- [ ] Boss arena reachable
- [ ] No impossible jumps (max_gap < player_max_jump)
- [ ] Sufficient checkpoints (minimum 3)
- [ ] Reward distribution balanced
- [ ] Enemy spawn count within limits
- [ ] No tile overlapping with entities

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

### Serialization
- Level data to JSON: < 50 KB
- Tile atlas texture: 256x2048 px (2 MB shared)
- Per-level memory overhead: < 2 MB

---

## Reproducibility Guarantee

### Testing Suite
```
Test 1: Same Seed → Same Level
  for i in 1..10000:
    level_a = generate_level(seed=12345, difficulty=2)
    level_b = generate_level(seed=12345, difficulty=2)
    assert level_a == level_b
    
Test 2: Different Seed → Different Level
  level_1 = generate_level(seed=12345, difficulty=2)
  level_2 = generate_level(seed=12346, difficulty=2)
  assert level_1 != level_2  (allow <2% similarity)
  
Test 3: Cross-Platform Determinism
  // Run on iOS device, macOS simulator, Android
  level_ios = generate(seed, difficulty)
  level_mac = generate(seed, difficulty)
  assert level_ios == level_mac
  
Test 4: Performance Under Stress
  for i in 1..100:
    level = generate_level(seed=random(), difficulty=random())
    assert generation_time < PLATFORM_TARGET_MS
```

---

## Implementation Roadmap

### Phase 1: Core PRNG & Seeding (Week 1)
- [ ] Implement xorshift64* RNG
- [ ] Build Level ID parser/encoder
- [ ] Write seed derivation logic
- [ ] Unit tests for determinism

### Phase 2: Tile Generation (Week 2)
- [ ] Implement platform placement algorithm
- [ ] Add collision validation
- [ ] Perlin noise integration
- [ ] Difficulty scaling

### Phase 3: Entity Generation (Week 3)
- [ ] Enemy type selection and placement
- [ ] Reward distribution
- [ ] AI behavior assignment
- [ ] Boss arena generation

### Phase 4: Validation & QA (Week 4)
- [ ] Reachability validator
- [ ] Difficulty estimator
- [ ] Performance profiling
- [ ] Cross-device testing

### Phase 5: Integration (Week 5)
- [ ] Level loader in game engine
- [ ] Level ID sharing (JSON export)
- [ ] Replay system
- [ ] Analytics tracking

---

## Future Enhancements

1. **Biome-Specific Generation**: Unique tile sets and enemy types per biome
2. **Dynamic Difficulty**: Adjust on-the-fly based on player performance
3. **Multiplayer Levels**: Shared level seeds between players
4. **Community Levels**: Integration with backend for level sharing
5. **AI-Enhanced Generation**: Use ML models to guide level quality

---

## References & Research

- xorshift64*: Marsaglia (2003), "Xorshift RNGs"
- Procedural Content Generation: Togelius et al. (2011), "Search-Based Procedural Content Generation"
- Wave Function Collapse: Gumin (2016), constraint-based tile generation
- Game Design Theory: Hunicke, LeBlanc, Zubek (2004), MDA Framework
- "Artificial Intelligence and Games": Yannakakis & Togelius (2018)

**Determinism Note:** Unity's `Mathf.PerlinNoise` is explicitly excluded from the
generation pipeline. All randomness must flow through the xorshift64* PRNG to
guarantee cross-platform, cross-version determinism.
