# ADR-004: Generation Pipeline Architecture

**Date:** 2026-02-04
**Status:** Accepted

## Context

The game requires procedurally generated side-scrolling levels that satisfy competing constraints:

- **Deterministic.** Given the same level ID, the pipeline must produce an identical level (ADR-002, ADR-003).
- **Playable.** Every generated level must be completable. The player must be able to reach the exit from the start without requiring abilities or items they do not have.
- **Difficulty-controlled.** Levels tagged as difficulty 03 must feel meaningfully different from difficulty 07. Difficulty should affect platforming precision, enemy density, and hazard frequency in a predictable, tunable way.
- **Natural-looking.** Terrain should feel organic, not obviously random. Flat runs, slopes, caverns, and verticality should emerge naturally rather than looking like random tile noise.
- **Fast.** Generation must complete within 150 ms on the target platform (WebGL in modern browsers; originally baselined on iPhone 11 / A13 Bionic) to avoid perceptible loading stalls.

A purely random approach (scatter tiles, place enemies) fails the playability and aesthetics constraints. A purely hand-authored approach fails the variety and sharing constraints. A hybrid pipeline is required.

## Decision

Level generation uses a **three-stage pipeline**, executed sequentially and entirely deterministically from the xorshift64 PRNG state derived from the level ID seed.

### Stage 1: PRNG Terrain Generation

**Input:** Seed, biome, difficulty.
**Output:** Raw tile grid (up to 256 x 16 tiles on baseline device).

1. Initialize the xorshift64 PRNG with the 64-bit seed from the level ID.
2. Generate a 1D heightmap using our custom Perlin noise implementation (2-3 octaves), seeded from the PRNG. The heightmap defines the ground surface profile.
3. Apply biome-specific modifiers: cave carving for underground biomes, platform islands for sky biomes, flat sections for urban biomes.
4. Place sub-surface fill tiles, background tiles, and decorative tiles based on biome rules.
5. All arithmetic uses integer or fixed-point math per ADR-002.

This stage produces terrain that looks natural but has no guarantee of playability.

### Stage 2: Grammar-Based Zone Placement

**Input:** Raw tile grid from Stage 1, difficulty parameter.
**Output:** Tile grid with gameplay zones injected.

This stage overlays **pre-designed micro-patterns** (grammars) onto the raw terrain to ensure interesting gameplay moments:

- **Challenge zones:** Short hand-designed platforming sequences (e.g., "three floating platforms over a pit", "wall-jump chimney") selected from a library and parameterised by difficulty. Higher difficulty selects tighter timings, smaller platforms, and more hazards.
- **Rest zones:** Safe flat areas with optional collectibles, placed at regular intervals to control pacing.
- **Encounter zones:** Enemy placement arenas with cover geometry, scaled by difficulty.
- **Transition zones:** Connective segments that blend between the raw terrain and grammar zones.

Zone selection and placement positions are determined by the PRNG, but the internal structure of each zone is pre-authored and stored as small tilemap prefabs. The difficulty parameter controls which zones are eligible and how their internal parameters (gap widths, platform sizes, enemy counts) are scaled.

### Stage 3: Constraint Validation and Repair

**Input:** Tile grid with zones from Stage 2.
**Output:** Final, validated tile grid ready for instantiation.

This stage performs a **reachability analysis** to guarantee the level is completable:

1. **Pathfinding pass.** A simplified physics simulation (jump arcs, run speed) walks the level from start to exit. Any tile the player can reach is marked as "reachable."
2. **Reachability check.** If the exit is not reachable, the repair sub-system activates.
3. **Repair.** The repair algorithm identifies the first unreachable gap and inserts a minimal bridge (extra platform, shortened gap, or removed wall) to restore connectivity. The repair loop runs until the exit is reachable or a maximum iteration count is hit.
4. **Difficulty bounds check.** The validator counts challenge metrics (total jumps, enemy count, hazard density) and compares them to the target difficulty range. If metrics are out of bounds, minor adjustments are applied (add/remove enemies, widen/narrow gaps).
5. **Final validation.** A second pathfinding pass confirms reachability post-repair.

If the repair loop exhausts its iteration budget without achieving a valid level (expected to be extremely rare given the grammar-zone design), the generator falls back to a known-good template level for that biome and difficulty, stamped with a flag so analytics can track the failure rate.

### Pipeline Diagram

```
Level ID
   |
   v
[Parse ID] --> epoch, seed  (difficulty from DifficultyManager, not encoded in ID)
   |
   v
[Stage 1: PRNG Terrain]  ~30-50ms
   |
   v
[Stage 2: Grammar Zones]  ~40-60ms
   |
   v
[Stage 3: Validation]     ~20-40ms
   |
   v
Final Tile Grid --> Instantiate Unity Tilemap
```

Total target: < 150 ms on A13 Bionic.

## Consequences

### Positive

- **Guaranteed playability.** The constraint validation stage ensures every level is completable, eliminating the most critical failure mode of pure procedural generation.
- **Controlled difficulty.** Grammar zones and the difficulty bounds check provide two layers of difficulty tuning, giving designers precise control over the experience curve.
- **Natural aesthetics.** Stage 1's noise-based terrain provides organic shapes, while Stage 2's authored zones ensure memorable gameplay moments. The combination avoids both "random noise" and "cookie-cutter template" failure modes.
- **Deterministic.** All three stages derive from the same PRNG state chain. No external data, no randomness outside xorshift64, no floating-point layout decisions.
- **Extensible.** New grammar zones can be added without changing the pipeline architecture. New biome modifiers slot into Stage 1. New validation rules slot into Stage 3.

### Negative

- **Three-stage complexity.** The pipeline is more complex than a single-pass generator. Each stage must be understood in the context of the others. Thorough documentation and integration tests are essential.
- **Repair loop risk.** Although rare, the repair sub-system modifying the level post-generation means the final output may not perfectly match the "pure" procedural result. Repair changes are minimal and localised, but they are a deviation from pure PRNG determinism. Importantly, the repair logic itself is deterministic, so the same seed still produces the same repaired level.
- **Grammar zone authoring burden.** Designers must create a library of micro-pattern prefabs for each biome. The initial library needs at least 15-20 zones per biome to avoid visible repetition. This is an up-front content investment.
- **Performance budget is tight.** 150 ms across three stages on a mobile CPU leaves limited headroom. Stage 3's pathfinding is the most expensive operation and must be carefully optimised (A* on a coarse graph, not per-tile).
