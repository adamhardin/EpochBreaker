# Level Generation: Validation & QA Suite

## Overview
This document defines the comprehensive testing framework for the generative level system. All tests must pass before progression to next development phase.

---

## Test Categories

### 1. Determinism Tests (CRITICAL)

#### Test 1.1: Seed Reproducibility
**Objective**: Verify identical seeds produce identical levels
```
Test Case:
  1. Generate level with seed=0xDEADBEEF, difficulty=2, biome=0
  2. Store level as level_a
  3. Generate level with seed=0xDEADBEEF, difficulty=2, biome=0
  4. Store level as level_b
  5. Compare: tilemap, entities, checkpoints
  
Expected Result: 100% match on all fields
Pass Criteria: level_a == level_b (byte-for-byte)
Failure Criteria: Any mismatch in tile placement, enemy position, rewards
```

#### Test 1.2: Cross-Run Determinism (1000 iterations)
**Objective**: Ensure consistency across multiple generation calls
```
Test Case:
  1. Create array of 10 random seeds
  2. For each seed, generate 100 levels
  3. For each seed, verify all 100 generations are identical
  
Expected Result: 100% consistency
Pass Criteria: All 1000 levels match across iterations
Failure Criteria: Any variation in output
Timeout: 30 seconds per 100 generations
```

#### Test 1.3: Cross-Platform Determinism
**Objective**: Ensure same seed produces same level on different devices/platforms
```
Test Case:
  1. On iOS device: generate level with seed=0x123456
  2. On macOS simulator: generate same level
  3. Compare byte-by-byte
  
Expected Result: Exact match
Pass Criteria: iOS output == macOS output
Failure Criteria: Any platform-specific variation
Test Runs: 50 levels per platform
```

#### Test 1.4: PRNG Period Validation
**Objective**: Confirm xorshift64* has proper period and no short cycles
```
Test Case:
  1. Initialize PRNG with seed=1
  2. Generate sequence of 10 million values
  3. Check for premature repetition (first repeat at position > 2^32)
  4. Verify statistical properties (chi-square test)
  
Expected Result: No repetition within 2^32 values
Pass Criteria: Minimum period > 2^40
Failure Criteria: Early repetition or bias in distribution
```

---

### 2. Reachability Tests (CRITICAL)

#### Test 2.1: Start to Goal Path
**Objective**: Verify player can reach goal from start
```
Test Case:
  1. Generate 100 random levels across all difficulties
  2. For each level, run flood-fill from start_position
  3. Check if goal_position is reachable
  
Expected Result: 100% reachable
Pass Criteria: All 100 levels solvable
Failure Criteria: Any unreachable goal
Analysis: Log failed levels for debugging
```

#### Test 2.2: Platform Continuity
**Objective**: Ensure no isolated platforms exist
```
Test Case:
  1. Generate 50 random levels
  2. For each platform, verify it connects to at least one neighbor
  3. Build connectivity graph
  
Expected Result: Single connected component from start to goal
Pass Criteria: No disconnected platform clusters
Failure Criteria: Isolated platforms in any level
```

#### Test 2.3: Jump Feasibility
**Objective**: Ensure all required jumps are within player capabilities
```
Test Case:
  1. Define player_max_jump_horizontal = 5 tiles, player_max_jump_vertical = 4 tiles
  2. For 100 random levels, measure all gap distances
  3. Verify horizontal gap <= 5 tiles, vertical jump <= 4 tiles
  
Expected Result: All gaps passable
Pass Criteria: 100% of gaps within capability
Failure Criteria: Any impossible jump required for progression
Tolerance: ±0.5 tiles
```

---

### 3. Difficulty Validation Tests

#### Test 3.1: Difficulty Rating Accuracy
**Objective**: Verify calculated difficulty matches expected difficulty
```
Test Case:
  1. Generate levels with difficulty=1, 2, 3, 4, 5
  2. Calculate difficulty_score for each level:
     - enemy_count * enemy_type_weight
     - traversal_challenge_score (gap sizes, timing)
     - safe_zone_ratio
  3. Verify calculated_score ≈ expected_difficulty * weight_constant
  
Expected Result: Calculated difficulty within ±1.5 points
Pass Criteria: |calculated - expected| <= 1.5
Failure Criteria: Variance > 2.0
Test Count: 100 levels per difficulty tier (500 total)
```

#### Test 3.2: Enemy Distribution
**Objective**: Verify enemy counts scale appropriately with difficulty
```
Test Case:
  1. Generate 20 levels at each difficulty (1-5)
  2. Count enemies per level
  3. Analyze distribution:
     - difficulty=1: 3-7 enemies (avg 5)
     - difficulty=2: 7-11 enemies (avg 9)
     - difficulty=3: 11-15 enemies (avg 13)
     - difficulty=4: 15-19 enemies (avg 17)
     - difficulty=5: 19-23 enemies (avg 21)
  
Expected Result: Linear relationship between difficulty and enemy count
Pass Criteria: Correlation coefficient > 0.95
Failure Criteria: Skewed distribution (std dev > expected range)
```

#### Test 3.3: Progression Smoothness
**Objective**: Verify difficulty increases gradually (no sudden spikes)
```
Test Case:
  1. Generate 100 sequential levels (same biome, increasing difficulty)
  2. Calculate difficulty gradient: d(difficulty) / d(level_number)
  3. Verify gradient is positive and monotonic
  
Expected Result: Smooth difficulty curve
Pass Criteria: Max gradient spike < 0.3 difficulty_points per zone
Failure Criteria: Sudden difficulty jump > 0.5
Graph Output: Required for visual inspection
```

---

### 4. Performance Tests

#### Test 4.1: Generation Speed Baseline
**Objective**: Measure generation time across device tiers
```
Test Case:
  1. Generate 100 random levels on each device tier
  2. Measure generation_time from seed input to playable level
  3. Record statistics: min, max, avg, p95, p99
  
Expected Results by Device (canonical targets from Level-Generation-Technical-Spec.md):
  | Device | Target (avg) | P95 | P99 |
  |--------|-------------|-----|-----|
  | iPhone 14+ | 50 ms | 80 ms | 120 ms |
  | iPhone 12/13 | 80 ms | 120 ms | 150 ms |
  | iPhone 11 (baseline) | 100 ms | 150 ms | 200 ms |

Note: iPhone XR and older are not supported.
Pass Criteria: All devices meet P95 targets
Failure Criteria: Any device exceeds P99 by >25%
```

#### Test 4.2: Memory Overhead
**Objective**: Measure per-level memory usage
```
Test Case:
  1. Generate 10 random levels
  2. Measure peak RAM usage during generation
  3. Measure resident level data size
  
Expected Results:
  - Generation peak: < 5 MB
  - Resident level data: < 2 MB per level
  - Texture atlas (shared): < 2 MB total
  
Pass Criteria: All metrics within targets
Failure Criteria: Exceeds by >20%
```

#### Test 4.3: Stress Test (1000 Level Generation)
**Objective**: Ensure system remains stable under heavy load
```
Test Case:
  1. Generate 1000 consecutive levels with random seeds
  2. Monitor for memory leaks, performance degradation
  3. Verify last level takes same time as first
  
Expected Results:
  - No memory leak detected (resident memory stable)
  - Consistent performance across all 1000 generations
  - Zero crashes or timeouts
  
Pass Criteria: All 1000 levels generated successfully
Failure Criteria: Memory growth > 10%, crashes, timeouts
```

---

### 5. Design Quality Tests

#### Test 5.1: Visual Coherence
**Objective**: Verify levels look visually correct and aesthetically pleasing
```
Test Case:
  1. Generate 20 levels per biome
  2. Manual inspection: Does level look "correct"?
  3. Check for visual anomalies:
     - Floating tiles
     - Awkward enemy placement
     - Misaligned textures
     - Ugly patterns or repetition
  
Expected Result: 95%+ of levels pass visual inspection
Pass Criteria: Visual issues in < 5% of levels
Failure Criteria: Visual glitches in > 10%
Reviewer: Design lead + 2 QA testers
```

#### Test 5.2: Gameplay Feel
**Objective**: Verify levels are fun and challenging (not frustrating)
```
Test Case:
  1. Play 50 generated levels (10 per difficulty)
  2. Subjective assessment:
     - Is the difficulty curve smooth?
     - Do enemies feel fair?
     - Are rewards well-placed?
     - Is the level engaging?
  3. Rating scale: 1-5 stars
  
Expected Result: Average rating >= 3.5/5
Pass Criteria: No level scores < 2.0
Failure Criteria: Average < 3.0 or > 2 levels below 2.0
Testers: Minimum 3 independent playtesters
```

#### Test 5.3: Biome Authenticity
**Objective**: Verify biome-specific assets and themes are applied correctly
```
Test Case:
  1. Generate 5 levels per biome type
  2. Verify biome-specific assets are used:
     - Forest: green tiles, woodland enemies
     - Cavern: dark stone, underground creatures
     - Sky: light platforms, flying enemies
     - Volcanic: red/orange tiles, lava hazards
  
Expected Result: 100% biome authenticity
Pass Criteria: All biomes correctly themed
Failure Criteria: Wrong assets in any biome
```

---

### 6. Safety & Edge Case Tests

#### Test 6.1: Impossible Geometry Prevention
**Objective**: Prevent generation of unplayable layouts
```
Test Case:
  1. Generate 100 levels
  2. Check for each level:
     - No inverted gravity zones
     - No stacked overlapping entities
     - No invisible walls
     - No boundaries exceeded
  
Expected Result: Zero geometry errors
Pass Criteria: All levels valid
Failure Criteria: Any anomaly detected
```

#### Test 6.2: Enemy Behavior Validation
**Objective**: Ensure enemies have valid patrol paths
```
Test Case:
  1. Generate 100 levels
  2. For each enemy, verify:
     - Patrol path exists and is continuous
     - No enemies spawn in unreachable locations
     - Behavior_type is valid for current zone
  
Expected Result: All enemies have valid behavior
Pass Criteria: 100% valid enemies
Failure Criteria: Invalid behavior in any enemy
```

#### Test 6.3: Reward Placement Sanity
**Objective**: Ensure rewards are reachable and strategic
```
Test Case:
  1. Generate 100 levels
  2. For each reward:
     - Verify it's on an accessible platform
     - Verify it's placed strategically (post-challenge)
     - Verify no unreachable "hidden" rewards
  
Expected Result: All rewards accessible and strategic
Pass Criteria: 100% valid placements
Failure Criteria: Any unreachable reward
```

---

### 7. Integration Tests

#### Test 7.1: Level ID Encoding/Decoding
**Objective**: Verify Level ID can be encoded and decoded correctly
```
Test Case:
  1. Generate 100 random levels
  2. Extract metadata and encode to Level ID
  3. Decode Level ID back to metadata
  4. Regenerate level from seed
  5. Verify new level == original level
  
Expected Result: Perfect round-trip fidelity
Pass Criteria: level_original == level_regenerated
Failure Criteria: Any mismatch in decoded metadata
```

#### Test 7.2: Level Persistence
**Objective**: Verify levels can be saved and loaded
```
Test Case:
  1. Generate level_a
  2. Serialize to JSON
  3. Save to disk
  4. Load from disk
  5. Deserialize from JSON
  6. Compare with level_a
  
Expected Result: Serialization round-trip works
Pass Criteria: Loaded level == original level
Failure Criteria: Data loss or corruption
File Size Targets: < 50 KB per level
```

#### Test 7.3: Engine Integration
**Objective**: Verify levels load and play correctly in-engine
```
Test Case:
  1. Generate 20 random levels
  2. Load each into Unity game scene
  3. Run for 5 seconds gameplay
  4. Verify:
     - No rendering artifacts
     - Collision detection works
     - Player movement responsive
     - Enemies animate correctly
  
Expected Result: Seamless engine integration
Pass Criteria: All levels load and play
Failure Criteria: Any level fails to load or crashes
```

---

## Test Execution Schedule

### Pre-Alpha Testing (Week 1-2)
- Determinism tests (1.1-1.4)
- Reachability tests (2.1-2.3)
- Performance baseline (4.1)

### Alpha Testing (Week 3-4)
- Difficulty validation (3.1-3.3)
- Performance stress (4.2-4.3)
- Design quality (5.1-5.3)

### Beta Testing (Week 5-6)
- Safety & edge cases (6.1-6.3)
- Integration tests (7.1-7.3)
- Gameplay QA with external testers

### Release Readiness (Week 7)
- Final performance optimization
- Cross-device validation
- Documentation finalization

---

## Test Failure Protocol

If any critical test (marked CRITICAL) fails:
1. **Stop** all further development
2. **Log** detailed failure report (see template below)
3. **Analyze** root cause
4. **Fix** underlying issue
5. **Re-run** failed test and 10 related tests to confirm resolution
6. **Resume** development only after re-validation

### Failure Report Template
```
TEST NAME: [e.g., "Test 1.1: Seed Reproducibility"]
STATUS: FAILED
SEVERITY: [CRITICAL | HIGH | MEDIUM | LOW]
SEED(S): [e.g., 0xDEADBEEF]
FAILURE DETAILS:
  - Expected: [what should have happened]
  - Actual: [what actually happened]
  - Diff: [specific differences, if applicable]
ROOT CAUSE ANALYSIS:
  [Investigation summary]
FIX APPLIED:
  [Description of fix]
RE-TEST RESULTS:
  [Results of regression testing]
```

---

## Success Criteria Summary

| Category | Pass Threshold | Test Count |
|----------|-----------------|-----------|
| Determinism | 100% | 4 |
| Reachability | 100% | 3 |
| Difficulty | >90% accuracy | 3 |
| Performance | All devices meet P95 | 3 |
| Quality | >95% pass visual | 3 |
| Safety | 100% no anomalies | 3 |
| Integration | 100% load/play | 3 |

**Overall Pass Criteria**: All categories at or above thresholds before release.
