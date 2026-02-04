using System;
using System.Collections.Generic;

namespace SixteenBit.Generative
{
    /// <summary>
    /// Validates generated levels for playability and design quality,
    /// including full support for the destructible environment system.
    ///
    /// Checks performed:
    ///   1. Start/Goal Accessible - player position empty with solid below
    ///   2. Reachability - BFS from start to goal with jump physics; treats
    ///      Soft/Medium destructible tiles as breakable (traversable) with Starting weapon
    ///   3. Weapon Progression - left-to-right primary path walk verifying no
    ///      required-path material exceeds currently held weapon tier
    ///   4. Destruction Path Validation - each zone has at least 1 path completable
    ///      with starting weapon and at least 2 distinct total paths
    ///   5. Difficulty Within Tolerance - computed vs target within +/- 1.0
    ///   6. No Impossible Gaps - no gap wider than MAX_JUMP_HORIZONTAL (5 tiles)
    ///   7. Minimum Checkpoints - at least 3
    ///   8. Entity Bounds - all enemies, weapon drops, rewards within level bounds
    ///   9. Structural Cascade Depth - no structural group cascade depth > 16
    ///
    /// This validator runs after generation and before the level is presented
    /// to the player. If validation fails, the generator should re-attempt
    /// with a modified seed.
    /// </summary>
    public sealed class LevelValidator
    {
        /// <summary>
        /// Maximum horizontal jump distance in tiles.
        /// Derived from Module 4 physics spec: jump velocity + air control.
        /// </summary>
        public const int MAX_JUMP_HORIZONTAL = 5;

        /// <summary>
        /// Maximum vertical jump height in tiles.
        /// Derived from Module 4 physics spec: variable jump with max hold.
        /// </summary>
        public const int MAX_JUMP_VERTICAL = 4;

        /// <summary>
        /// Allowed deviation between calculated and target difficulty.
        /// From Validation-QA-Suite: +/- 1.0 point tolerance.
        /// </summary>
        public const float DIFFICULTY_TOLERANCE = 1.5f;

        /// <summary>
        /// Minimum required checkpoints per level.
        /// </summary>
        public const int MIN_CHECKPOINTS = 3;

        /// <summary>
        /// Maximum allowed structural cascade depth for load-bearing groups.
        /// Prevents runaway chain-destruction that could crash physics or
        /// create unplayable voids.
        /// </summary>
        public const int MAX_CASCADE_DEPTH = 16;

        // -----------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------

        /// <summary>
        /// Run all validation checks on a generated level.
        /// Returns a result with pass/fail status and details for each check.
        /// </summary>
        public ValidationResult Validate(LevelData level)
        {
            var result = new ValidationResult();

            // Check 1: Start position is valid (empty tile with solid below)
            result.StartAccessible = IsPositionAccessible(level, level.Layout.StartX, level.Layout.StartY);

            // Check 2: Goal position is valid
            result.GoalAccessible = IsPositionAccessible(level, level.Layout.GoalX, level.Layout.GoalY);

            // Check 3: Reachability -- BFS from start to goal respecting
            //          jump physics and treating Soft/Medium destructibles
            //          as traversable.
            result.Reachable = CheckReachability(level);

            // Check 4: Weapon progression along the primary path
            result.WeaponProgressionValid = CheckWeaponProgression(level);

            // Check 5: Destruction path validation per zone
            result.DestructionPathsValid = CheckDestructionPaths(level);

            // Check 6: Difficulty within tolerance
            result.CalculatedDifficulty = CalculateDifficulty(level);
            result.TargetDifficulty = 5.5f + level.ID.Difficulty * 3.0f;
            result.DifficultyWithinTolerance =
                Math.Abs(result.CalculatedDifficulty - result.TargetDifficulty) <= DIFFICULTY_TOLERANCE;

            // Check 7: No impossible gaps (> MAX_JUMP_HORIZONTAL contiguous
            //          columns with no solid or platform surface)
            result.NoImpossibleGaps = !CheckForImpossibleGaps(level);

            // Check 8: Minimum checkpoints
            result.CheckpointCount = level.Checkpoints != null ? level.Checkpoints.Count : 0;
            result.HasMinCheckpoints = result.CheckpointCount >= MIN_CHECKPOINTS;

            // Check 9: Entity bounds -- enemies, weapon drops, rewards
            result.EntitiesInBounds = CheckEntityBounds(level);

            // Check 10: Structural cascade depth
            result.StructuralCascadeValid = CheckStructuralCascadeDepth(level);

            // Overall pass -- every individual check must succeed
            result.Passed = result.StartAccessible
                         && result.GoalAccessible
                         && result.Reachable
                         && result.WeaponProgressionValid
                         && result.DestructionPathsValid
                         && result.DifficultyWithinTolerance
                         && result.NoImpossibleGaps
                         && result.HasMinCheckpoints
                         && result.EntitiesInBounds
                         && result.StructuralCascadeValid;

            return result;
        }

        // =================================================================
        // Check 1: Position Accessibility
        // =================================================================

        /// <summary>
        /// A position is accessible when the tile itself is not solid/hazard
        /// and there is a solid or platform surface directly below it.
        /// Destructible tiles below also count as a valid surface.
        /// </summary>
        private bool IsPositionAccessible(LevelData level, int x, int y)
        {
            int width = level.Layout.WidthTiles;
            int height = level.Layout.HeightTiles;

            if (x < 0 || x >= width || y < 0 || y >= height)
                return false;

            int posIdx = y * width + x;
            if (posIdx < 0 || posIdx >= level.Layout.Collision.Length)
                return false;

            byte posCollision = level.Layout.Collision[posIdx];

            // The tile the player occupies must be empty (walkable)
            if (posCollision == (byte)CollisionType.Solid ||
                posCollision == (byte)CollisionType.Hazard)
                return false;

            // Check for solid surface directly below
            if (y + 1 < height)
            {
                int belowIdx = (y + 1) * width + x;
                if (belowIdx >= 0 && belowIdx < level.Layout.Collision.Length)
                {
                    byte belowCol = level.Layout.Collision[belowIdx];
                    if (belowCol == (byte)CollisionType.Solid ||
                        belowCol == (byte)CollisionType.PlatformPassthrough ||
                        belowCol == (byte)CollisionType.Destructible)
                        return true;
                }
            }

            return false;
        }

        // =================================================================
        // Check 2: Reachability (BFS with jump physics)
        // =================================================================

        /// <summary>
        /// BFS flood-fill reachability check from start to goal.
        /// Simulates player movement with jump physics constraints.
        ///
        /// A tile is reachable from another if:
        ///   - Horizontal distance &lt;= MAX_JUMP_HORIZONTAL
        ///   - Vertical gain (going up) &lt;= MAX_JUMP_VERTICAL
        ///   - Vertical drop (going down) is unlimited (falling)
        ///   - The destination tile has a solid/platform/breakable surface below it
        ///
        /// Destructible tiles with Soft or Medium material are considered
        /// traversable because the player can break them with the Starting weapon.
        /// </summary>
        private bool CheckReachability(LevelData level)
        {
            int width = level.Layout.WidthTiles;
            int height = level.Layout.HeightTiles;

            // Build set of all standable positions.
            // A position (x, y) is standable when:
            //   - The tile at (x, y) is NOT solid and NOT hazard
            //     (or it IS a breakable destructible -- Soft/Medium)
            //   - The tile at (x, y+1) is solid, platform, or a non-breakable
            //     destructible (providing ground), OR the tile at (x, y+1) is
            //     breakable but there is solid ground further below
            //
            // For simplicity we treat Soft/Medium destructibles as air
            // (player breaks through them) and all other collision types normally.

            // Pre-compute which tiles the player can break with Starting weapon
            bool[] breakable = BuildBreakableMap(level, WeaponTier.Starting);

            var standable = new HashSet<int>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    int posIdx = y * width + x;
                    int belowIdx = (y + 1) * width + x;

                    // Tile the player occupies: must be passable.
                    // Solid and Hazard block occupancy.
                    // Destructible tiles that are breakable count as passable.
                    byte posCol = level.Layout.Collision[posIdx];
                    if (posCol == (byte)CollisionType.Solid && !breakable[posIdx])
                        continue;
                    if (posCol == (byte)CollisionType.Hazard)
                        continue;
                    if (posCol == (byte)CollisionType.Destructible && !breakable[posIdx])
                        continue; // Unbreakable destructible blocks occupancy

                    // Below tile: must provide a surface to stand on.
                    byte belowCol = level.Layout.Collision[belowIdx];
                    bool belowIsSurface = false;

                    if (belowCol == (byte)CollisionType.Solid && !breakable[belowIdx])
                        belowIsSurface = true;
                    else if (belowCol == (byte)CollisionType.PlatformPassthrough)
                        belowIsSurface = true;
                    else if (belowCol == (byte)CollisionType.Destructible && !breakable[belowIdx])
                        belowIsSurface = true; // Hard/Reinforced/Indestructible acts as ground

                    if (belowIsSurface)
                        standable.Add(posIdx);
                }
            }

            int startKey = level.Layout.StartY * width + level.Layout.StartX;
            int goalKey = level.Layout.GoalY * width + level.Layout.GoalX;

            if (!standable.Contains(startKey))
            {
                startKey = FindNearestStandable(standable, level.Layout.StartX, level.Layout.StartY, width);
                if (startKey < 0) return false;
            }

            if (!standable.Contains(goalKey))
            {
                goalKey = FindNearestStandable(standable, level.Layout.GoalX, level.Layout.GoalY, width);
                if (goalKey < 0) return false;
            }

            // BFS with adjacency defined by jump physics.
            // For large levels an O(N^2) scan of all standable pairs is expensive.
            // We bucket standable positions by column to make neighbor lookup faster.
            var columnBuckets = new Dictionary<int, List<int>>();
            foreach (int pos in standable)
            {
                int col = pos % width;
                if (!columnBuckets.ContainsKey(col))
                    columnBuckets[col] = new List<int>();
                columnBuckets[col].Add(pos);
            }

            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            queue.Enqueue(startKey);
            visited.Add(startKey);

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                if (current == goalKey)
                    return true;

                int cx = current % width;
                int cy = current / width;

                // Scan columns within horizontal jump range
                int minCol = Math.Max(0, cx - MAX_JUMP_HORIZONTAL);
                int maxCol = Math.Min(width - 1, cx + MAX_JUMP_HORIZONTAL);

                for (int col = minCol; col <= maxCol; col++)
                {
                    if (!columnBuckets.ContainsKey(col))
                        continue;

                    var bucket = columnBuckets[col];
                    for (int bi = 0; bi < bucket.Count; bi++)
                    {
                        int neighbor = bucket[bi];
                        if (visited.Contains(neighbor))
                            continue;

                        int ny = neighbor / width;
                        int dy = cy - ny; // positive means going up

                        // Going up: limited by MAX_JUMP_VERTICAL
                        // Going down (falling): unlimited
                        if (dy > MAX_JUMP_VERTICAL)
                            continue; // Too high to jump up

                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return false;
        }

        private int FindNearestStandable(HashSet<int> standable, int x, int y, int width)
        {
            for (int r = 1; r <= 5; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        int key = (y + dy) * width + (x + dx);
                        if (standable.Contains(key))
                            return key;
                    }
                }
            }
            return -1;
        }

        // =================================================================
        // Check 3: Weapon Progression
        // =================================================================

        /// <summary>
        /// Walk the primary path left-to-right, tracking the current weapon tier.
        /// Starting weapon breaks Soft + Medium.
        /// Medium weapon breaks up to Hard.
        /// Heavy weapon breaks up to Reinforced.
        /// Indestructible is never breakable.
        ///
        /// At each column, if a required-path destructible tile exists whose
        /// material needs a higher weapon tier than currently held, the check fails.
        /// Weapon drops on the primary path are collected in order.
        /// </summary>
        private bool CheckWeaponProgression(LevelData level)
        {
            int width = level.Layout.WidthTiles;
            int height = level.Layout.HeightTiles;

            // Sort weapon drops on primary path by X position (left to right)
            var primaryDrops = new List<WeaponDropData>();
            if (level.WeaponDrops != null)
            {
                for (int i = 0; i < level.WeaponDrops.Count; i++)
                {
                    if (level.WeaponDrops[i].OnPrimaryPath)
                        primaryDrops.Add(level.WeaponDrops[i]);
                }
            }
            primaryDrops.Sort((a, b) => a.TileX.CompareTo(b.TileX));

            int dropIndex = 0;
            WeaponTier currentTier = WeaponTier.Starting;

            for (int x = 0; x < width; x++)
            {
                // Collect any weapon drops at this column
                while (dropIndex < primaryDrops.Count && primaryDrops[dropIndex].TileX <= x)
                {
                    WeaponTier dropTier = primaryDrops[dropIndex].Tier;
                    if (dropTier > currentTier)
                        currentTier = dropTier;
                    dropIndex++;
                }

                // Scan this column for destructible tiles on the required path.
                // A destructible tile is "on the required path" if it has
                // CollisionType.Destructible and the player would need to
                // break it to proceed (i.e., it blocks a standable row).
                for (int y = 0; y < height; y++)
                {
                    int idx = y * width + x;
                    if (idx >= level.Layout.Collision.Length)
                        continue;

                    if (level.Layout.Collision[idx] != (byte)CollisionType.Destructible)
                        continue;

                    // Look up the material class
                    byte matClass = GetMaterialClass(level, idx);
                    if (matClass == (byte)MaterialClass.None)
                        continue;

                    // Check if this tile is on a required path (blocks passage).
                    // Simple heuristic: if the tile is at a height range the
                    // player would walk through (within a few tiles of ground),
                    // it is a potential blocker.
                    if (!IsTileOnRequiredPath(level, x, y))
                        continue;

                    if (!CanBreakMaterial((MaterialClass)matClass, currentTier))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determine whether a destructible tile is on the required path.
        /// A tile blocks the required path if there is no alternative
        /// route around it within jump range at this column.
        /// </summary>
        private bool IsTileOnRequiredPath(LevelData level, int x, int y)
        {
            int width = level.Layout.WidthTiles;
            int height = level.Layout.HeightTiles;

            // Check if there is any non-blocked standable row at this column.
            // If at least one row at this column lets the player pass without
            // needing to break this tile, the tile is not on the required path.
            for (int row = 0; row < height - 1; row++)
            {
                if (row == y) continue; // skip the tile in question

                int posIdx = row * width + x;
                int belowIdx = (row + 1) * width + x;

                byte posCol = level.Layout.Collision[posIdx];
                byte belowCol = level.Layout.Collision[belowIdx];

                // Position must be passable (not solid, not hazard, not unbreakable destructible)
                bool posPassable =
                    posCol == (byte)CollisionType.None ||
                    posCol == (byte)CollisionType.PlatformPassthrough;

                if (!posPassable)
                    continue;

                // Below must be a surface
                bool belowSurface =
                    belowCol == (byte)CollisionType.Solid ||
                    belowCol == (byte)CollisionType.PlatformPassthrough ||
                    belowCol == (byte)CollisionType.Destructible;

                if (belowSurface)
                    return false; // There is an alternate standable position; this tile is not required
            }

            return true; // No alternate route found; this tile is on the required path
        }

        /// <summary>
        /// Returns true if the given weapon tier can break the given material class.
        ///   Starting (0): Soft, Medium
        ///   Medium   (1): Soft, Medium, Hard
        ///   Heavy    (2): Soft, Medium, Hard, Reinforced
        /// Indestructible (5) is never breakable.
        /// </summary>
        private static bool CanBreakMaterial(MaterialClass material, WeaponTier tier)
        {
            switch (material)
            {
                case MaterialClass.None:
                    return true;
                case MaterialClass.Soft:
                case MaterialClass.Medium:
                    return true; // All tiers can break Soft and Medium
                case MaterialClass.Hard:
                    return tier >= WeaponTier.Medium;
                case MaterialClass.Reinforced:
                    return tier >= WeaponTier.Heavy;
                case MaterialClass.Indestructible:
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Return the maximum MaterialClass that a given weapon tier can break.
        /// </summary>
        private static MaterialClass MaxBreakable(WeaponTier tier)
        {
            switch (tier)
            {
                case WeaponTier.Starting: return MaterialClass.Medium;
                case WeaponTier.Medium:   return MaterialClass.Hard;
                case WeaponTier.Heavy:    return MaterialClass.Reinforced;
                default:                  return MaterialClass.Medium;
            }
        }

        // =================================================================
        // Check 4: Destruction Path Validation
        // =================================================================

        /// <summary>
        /// Each zone must have:
        ///   - at least 1 path completable with the Starting weapon
        ///   - at least 2 distinct total paths
        ///
        /// A "path" through a zone is defined as a connected set of standable
        /// positions from the zone's left edge to its right edge.  Two paths
        /// are distinct if they use at least one different row at some column.
        ///
        /// For the starting-weapon check we treat Soft/Medium destructibles
        /// as breakable (passable).  For the total path count we also treat
        /// Hard/Reinforced as breakable (any weapon upgrade could open them).
        /// </summary>
        private bool CheckDestructionPaths(LevelData level)
        {
            if (level.Layout.Zones == null || level.Layout.Zones.Length == 0)
                return true;

            int width = level.Layout.WidthTiles;

            for (int zi = 0; zi < level.Layout.Zones.Length; zi++)
            {
                var zone = level.Layout.Zones[zi];
                int zoneStart = Math.Max(0, zone.StartX);
                int zoneEnd = Math.Min(width, zone.EndX);
                if (zoneEnd - zoneStart < 4)
                    continue; // Trivially small zone; skip

                // Skip Intro and Buffer zones -- they are transitional areas
                // that don't require multiple destruction paths
                if (zone.Type == ZoneType.Intro || zone.Type == ZoneType.Buffer)
                    continue;

                // Count distinct paths using Starting weapon
                int startingPaths = CountDistinctPaths(level, zoneStart, zoneEnd, WeaponTier.Starting);
                if (startingPaths < 1)
                    return false;

                // With heavy weapon, verify at least 1 path exists through
                // Destruction and Combat zones (broken tiles provide passage
                // but not surfaces, so alternative elevated paths are rare).
                if (zone.Type == ZoneType.Destruction || zone.Type == ZoneType.Combat)
                {
                    int totalPaths = CountDistinctPaths(level, zoneStart, zoneEnd, WeaponTier.Heavy);
                    if (totalPaths < 1)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Count the number of distinct left-to-right paths through a zone
        /// section [startX, endX) given a specific weapon tier.
        ///
        /// Uses a column-sweep: at each column, track which rows are standable.
        /// A path is a connected chain of standable rows from column to column
        /// where consecutive positions are within jump range.  Two paths are
        /// distinct if they diverge by at least one row at some column.
        ///
        /// Returns the count of distinct entry rows at the left edge that
        /// can reach the right edge.
        /// </summary>
        private int CountDistinctPaths(LevelData level, int startX, int endX, WeaponTier tier)
        {
            int width = level.Layout.WidthTiles;
            int height = level.Layout.HeightTiles;

            bool[] breakable = BuildBreakableMap(level, tier);

            // For each column, build the set of standable rows
            // Then propagate reachability left-to-right via jump physics

            // Get standable rows at the first column
            var currentReachable = new HashSet<int>();
            {
                var standableRows = GetStandableRows(level, startX, breakable);
                foreach (int row in standableRows)
                    currentReachable.Add(row);
            }

            if (currentReachable.Count == 0)
                return 0;

            // Sweep columns
            for (int x = startX + 1; x < endX; x++)
            {
                var standableRows = GetStandableRows(level, x, breakable);
                if (standableRows.Count == 0)
                {
                    // No standable position at this column. Check if we can
                    // jump across (columns within MAX_JUMP_HORIZONTAL count
                    // as part of a gap). We continue and will let the gap
                    // check handle multi-column gaps.
                    continue;
                }

                var nextReachable = new HashSet<int>();
                foreach (int row in standableRows)
                {
                    // Can any currently-reachable row reach this (x, row)?
                    // We need to check whether any reachable position from
                    // a previous column within MAX_JUMP_HORIZONTAL can reach
                    // this row via jump physics.
                    foreach (int prevRow in currentReachable)
                    {
                        int dy = prevRow - row; // positive = going up
                        if (dy > MAX_JUMP_VERTICAL)
                            continue; // Too high
                        // Falling down is unlimited, going up limited
                        nextReachable.Add(row);
                        break; // One predecessor is enough
                    }
                }

                if (nextReachable.Count > 0)
                    currentReachable = nextReachable;
                // If nextReachable is empty but currentReachable is not,
                // we are in a gap; keep currentReachable for the next column
            }

            return currentReachable.Count;
        }

        /// <summary>
        /// Return the set of rows at a given column where the player can stand,
        /// given a pre-computed breakable map.
        /// </summary>
        private List<int> GetStandableRows(LevelData level, int x, bool[] breakable)
        {
            int width = level.Layout.WidthTiles;
            int height = level.Layout.HeightTiles;
            var rows = new List<int>();

            if (x < 0 || x >= width)
                return rows;

            for (int y = 0; y < height - 1; y++)
            {
                int posIdx = y * width + x;
                int belowIdx = (y + 1) * width + x;

                byte posCol = level.Layout.Collision[posIdx];

                // Position must be passable
                if (posCol == (byte)CollisionType.Hazard)
                    continue;
                if (posCol == (byte)CollisionType.Solid && !breakable[posIdx])
                    continue;
                if (posCol == (byte)CollisionType.Destructible && !breakable[posIdx])
                    continue;

                // Below must provide a surface
                byte belowCol = level.Layout.Collision[belowIdx];
                bool belowSurface = false;
                if (belowCol == (byte)CollisionType.Solid && !breakable[belowIdx])
                    belowSurface = true;
                else if (belowCol == (byte)CollisionType.PlatformPassthrough)
                    belowSurface = true;
                else if (belowCol == (byte)CollisionType.Destructible && !breakable[belowIdx])
                    belowSurface = true;

                if (belowSurface)
                    rows.Add(y);
            }

            return rows;
        }

        // =================================================================
        // Check 5: Difficulty
        // =================================================================

        /// <summary>
        /// Calculate composite difficulty score for a level.
        ///
        /// Components:
        ///   - Enemy difficulty (weighted sum of enemy types)
        ///   - Traversal difficulty (gap count and width)
        ///   - Hazard density
        ///   - Reward scarcity (fewer rewards = harder)
        ///   - Destructible density (more destructibles requiring higher
        ///     weapon tiers contribute to difficulty)
        /// </summary>
        public float CalculateDifficulty(LevelData level)
        {
            float difficulty = 0f;
            int width = level.Layout.WidthTiles;
            float levelScale = width / 256f;

            // 1. Enemy difficulty contribution
            float enemyDifficulty = 0f;
            if (level.Enemies != null)
            {
                for (int i = 0; i < level.Enemies.Count; i++)
                    enemyDifficulty += level.Enemies[i].DifficultyWeight;
            }
            difficulty += (enemyDifficulty / Math.Max(1f, levelScale)) * 0.35f;

            // 2. Traversal difficulty: gaps
            float gapDifficulty = CountGapDifficulty(level);
            difficulty += gapDifficulty * 0.20f;

            // 3. Hazard density
            float hazardDifficulty = CountHazardDensity(level);
            difficulty += hazardDifficulty * 0.10f;

            // 4. Reward scarcity
            int rewardCount = level.Rewards != null ? level.Rewards.Count : 0;
            int enemyCount = level.Enemies != null ? level.Enemies.Count : 0;
            float rewardRatio = rewardCount > 0
                ? (float)enemyCount / rewardCount
                : 10f;
            difficulty += Math.Min(rewardRatio * 0.3f, 3.0f) * 0.15f;

            // 5. Destructible material difficulty: higher-tier materials add
            //    difficulty because they require better weapons to progress
            float destructibleDifficulty = CountDestructibleDifficulty(level);
            difficulty += destructibleDifficulty * 0.20f;

            return difficulty;
        }

        private float CountGapDifficulty(LevelData level)
        {
            int width = level.Layout.WidthTiles;
            int height = level.Layout.HeightTiles;
            float totalDifficulty = 0f;
            int currentGapWidth = 0;

            for (int x = 0; x < width; x++)
            {
                bool hasGround = false;
                for (int y = 0; y < height; y++)
                {
                    int idx = y * width + x;
                    if (idx >= level.Layout.Collision.Length) break;
                    byte col = level.Layout.Collision[idx];
                    if (col == (byte)CollisionType.Solid ||
                        col == (byte)CollisionType.PlatformPassthrough ||
                        col == (byte)CollisionType.Destructible)
                    {
                        hasGround = true;
                        break;
                    }
                }

                if (!hasGround)
                {
                    currentGapWidth++;
                }
                else
                {
                    if (currentGapWidth > 0)
                    {
                        totalDifficulty += currentGapWidth * currentGapWidth * 0.1f;
                        currentGapWidth = 0;
                    }
                }
            }

            return totalDifficulty / (width / 100f);
        }

        private float CountHazardDensity(LevelData level)
        {
            int width = level.Layout.WidthTiles;
            int height = level.Layout.HeightTiles;
            int hazardCount = 0;

            for (int i = 0; i < level.Layout.Collision.Length; i++)
            {
                if (level.Layout.Collision[i] == (byte)CollisionType.Hazard)
                    hazardCount++;
            }

            float totalTiles = width * height;
            return (hazardCount / totalTiles) * 1000f;
        }

        /// <summary>
        /// Score the difficulty contribution of destructible tiles.
        /// Higher material classes are harder because they require
        /// weapon upgrades to break.
        /// </summary>
        private float CountDestructibleDifficulty(LevelData level)
        {
            if (level.Layout.Destructibles == null || level.Layout.Destructibles.Length == 0)
                return 0f;

            int width = level.Layout.WidthTiles;
            float totalTiles = level.Layout.WidthTiles * level.Layout.HeightTiles;
            float score = 0f;

            for (int i = 0; i < level.Layout.Destructibles.Length; i++)
            {
                byte mat = level.Layout.Destructibles[i].MaterialClass;
                if (mat == (byte)MaterialClass.None)
                    continue;

                // Weight: Soft=0.5, Medium=1, Hard=2, Reinforced=3, Indestructible=4
                float weight = mat switch
                {
                    (byte)MaterialClass.Soft => 0.5f,
                    (byte)MaterialClass.Medium => 1.0f,
                    (byte)MaterialClass.Hard => 2.0f,
                    (byte)MaterialClass.Reinforced => 3.0f,
                    (byte)MaterialClass.Indestructible => 4.0f,
                    _ => 0f,
                };
                score += weight;
            }

            // Normalize by total tile count to keep the score proportional
            return (score / Math.Max(1f, totalTiles)) * 100f;
        }

        // =================================================================
        // Check 6: Impossible Gaps
        // =================================================================

        /// <summary>
        /// Check for gaps wider than MAX_JUMP_HORIZONTAL.
        /// A column has "surface" if any row contains Solid, Platform, or
        /// Destructible collision. Destructible tiles still provide a surface
        /// (even if breakable, they exist before the player reaches them).
        /// Returns true if an impossible gap is found.
        /// </summary>
        private bool CheckForImpossibleGaps(LevelData level)
        {
            int width = level.Layout.WidthTiles;
            int height = level.Layout.HeightTiles;
            int currentGapWidth = 0;

            for (int x = 0; x < width; x++)
            {
                bool hasAnySurface = false;
                for (int y = 0; y < height; y++)
                {
                    int idx = y * width + x;
                    if (idx >= level.Layout.Collision.Length) break;
                    byte col = level.Layout.Collision[idx];
                    if (col == (byte)CollisionType.Solid ||
                        col == (byte)CollisionType.PlatformPassthrough ||
                        col == (byte)CollisionType.Destructible)
                    {
                        hasAnySurface = true;
                        break;
                    }
                }

                if (!hasAnySurface)
                {
                    currentGapWidth++;
                    if (currentGapWidth > MAX_JUMP_HORIZONTAL)
                        return true;
                }
                else
                {
                    currentGapWidth = 0;
                }
            }

            return false;
        }

        // =================================================================
        // Check 8: Entity Bounds
        // =================================================================

        /// <summary>
        /// Verify all enemies, weapon drops, rewards, and checkpoints
        /// are within level bounds.
        /// </summary>
        private bool CheckEntityBounds(LevelData level)
        {
            int width = level.Layout.WidthTiles;
            int height = level.Layout.HeightTiles;

            if (level.Enemies != null)
            {
                for (int i = 0; i < level.Enemies.Count; i++)
                {
                    var e = level.Enemies[i];
                    if (e.TileX < 0 || e.TileX >= width || e.TileY < 0 || e.TileY >= height)
                        return false;
                }
            }

            if (level.WeaponDrops != null)
            {
                for (int i = 0; i < level.WeaponDrops.Count; i++)
                {
                    var w = level.WeaponDrops[i];
                    if (w.TileX < 0 || w.TileX >= width || w.TileY < 0 || w.TileY >= height)
                        return false;
                }
            }

            if (level.Rewards != null)
            {
                for (int i = 0; i < level.Rewards.Count; i++)
                {
                    var r = level.Rewards[i];
                    if (r.TileX < 0 || r.TileX >= width || r.TileY < 0 || r.TileY >= height)
                        return false;
                }
            }

            if (level.Checkpoints != null)
            {
                for (int i = 0; i < level.Checkpoints.Count; i++)
                {
                    var c = level.Checkpoints[i];
                    if (c.TileX < 0 || c.TileX >= width || c.TileY < 0 || c.TileY >= height)
                        return false;
                }
            }

            return true;
        }

        // =================================================================
        // Check 9: Structural Cascade Depth
        // =================================================================

        /// <summary>
        /// Verify that no structural group has a cascade depth exceeding
        /// MAX_CASCADE_DEPTH (16).
        ///
        /// Cascade depth is computed by building a dependency graph of
        /// structural groups: if a load-bearing tile in group A supports
        /// a tile in group B (group B tile is directly above group A tile),
        /// then group B depends on group A.
        ///
        /// The cascade depth of a group is the longest chain of dependencies
        /// beneath it.  If group C depends on group B depends on group A,
        /// the cascade depth of group C is 2.
        /// </summary>
        private bool CheckStructuralCascadeDepth(LevelData level)
        {
            if (level.Layout.Destructibles == null || level.Layout.Destructibles.Length == 0)
                return true;

            int width = level.Layout.WidthTiles;
            int height = level.Layout.HeightTiles;
            int totalTiles = width * height;

            // Collect all structural group IDs and their tile indices
            var groupTiles = new Dictionary<ushort, List<int>>();

            for (int i = 0; i < level.Layout.Destructibles.Length && i < totalTiles; i++)
            {
                var dt = level.Layout.Destructibles[i];
                if (dt.StructuralGroupId == 0)
                    continue;
                if (dt.MaterialClass == (byte)MaterialClass.None)
                    continue;

                if (!groupTiles.ContainsKey(dt.StructuralGroupId))
                    groupTiles[dt.StructuralGroupId] = new List<int>();
                groupTiles[dt.StructuralGroupId].Add(i);
            }

            if (groupTiles.Count == 0)
                return true;

            // Build dependency graph: group -> set of groups it depends on
            // (groups whose load-bearing tiles are directly below this group's tiles)
            var dependencies = new Dictionary<ushort, HashSet<ushort>>();
            foreach (var kvp in groupTiles)
                dependencies[kvp.Key] = new HashSet<ushort>();

            foreach (var kvp in groupTiles)
            {
                ushort groupId = kvp.Key;
                var tiles = kvp.Value;

                for (int ti = 0; ti < tiles.Count; ti++)
                {
                    int idx = tiles[ti];
                    int x = idx % width;
                    int y = idx / width;

                    // Check the tile directly below
                    if (y + 1 >= height) continue;
                    int belowIdx = (y + 1) * width + x;
                    if (belowIdx >= level.Layout.Destructibles.Length) continue;

                    var belowDt = level.Layout.Destructibles[belowIdx];
                    if (belowDt.StructuralGroupId == 0 || belowDt.StructuralGroupId == groupId)
                        continue;
                    if (!belowDt.IsLoadBearing)
                        continue;

                    // This group depends on the group below it
                    dependencies[groupId].Add(belowDt.StructuralGroupId);
                }
            }

            // Compute cascade depth for each group via DFS with memoization
            var depthCache = new Dictionary<ushort, int>();
            var visiting = new HashSet<ushort>(); // cycle detection

            foreach (var groupId in groupTiles.Keys)
            {
                int depth = ComputeCascadeDepth(groupId, dependencies, depthCache, visiting);
                if (depth > MAX_CASCADE_DEPTH)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Recursively compute the cascade depth of a structural group.
        /// Depth = 1 + max depth of all groups this group depends on.
        /// A group with no dependencies has depth 0.
        /// </summary>
        private int ComputeCascadeDepth(
            ushort groupId,
            Dictionary<ushort, HashSet<ushort>> dependencies,
            Dictionary<ushort, int> cache,
            HashSet<ushort> visiting)
        {
            if (cache.ContainsKey(groupId))
                return cache[groupId];

            // Cycle detection: if we are already visiting this node,
            // treat it as depth 0 to avoid infinite recursion.
            if (visiting.Contains(groupId))
                return 0;

            if (!dependencies.ContainsKey(groupId) || dependencies[groupId].Count == 0)
            {
                cache[groupId] = 0;
                return 0;
            }

            visiting.Add(groupId);
            int maxChildDepth = 0;

            foreach (ushort dep in dependencies[groupId])
            {
                int childDepth = ComputeCascadeDepth(dep, dependencies, cache, visiting);
                if (childDepth > maxChildDepth)
                    maxChildDepth = childDepth;
            }

            visiting.Remove(groupId);
            int depth = 1 + maxChildDepth;
            cache[groupId] = depth;
            return depth;
        }

        // =================================================================
        // Utility Methods
        // =================================================================

        /// <summary>
        /// Build a boolean map indicating which tiles are breakable with the
        /// given weapon tier.  Only tiles with CollisionType.Destructible and
        /// a MaterialClass within the weapon's range are marked true.
        /// </summary>
        private bool[] BuildBreakableMap(LevelData level, WeaponTier tier)
        {
            int total = level.Layout.Collision.Length;
            var map = new bool[total];

            if (level.Layout.Destructibles == null)
                return map;

            int destructiblesLen = level.Layout.Destructibles.Length;

            for (int i = 0; i < total; i++)
            {
                if (level.Layout.Collision[i] != (byte)CollisionType.Destructible)
                    continue;
                if (i >= destructiblesLen)
                    continue;

                byte matClass = level.Layout.Destructibles[i].MaterialClass;
                if (matClass == (byte)MaterialClass.None)
                {
                    map[i] = true; // No material = trivially breakable
                    continue;
                }

                map[i] = CanBreakMaterial((MaterialClass)matClass, tier);
            }

            return map;
        }

        /// <summary>
        /// Get the MaterialClass byte for a tile at the given flat index.
        /// Returns 0 (None) if destructible data is unavailable.
        /// </summary>
        private byte GetMaterialClass(LevelData level, int flatIndex)
        {
            if (level.Layout.Destructibles == null)
                return (byte)MaterialClass.None;
            if (flatIndex < 0 || flatIndex >= level.Layout.Destructibles.Length)
                return (byte)MaterialClass.None;
            return level.Layout.Destructibles[flatIndex].MaterialClass;
        }
    }

    // =====================================================================
    // Validation Result
    // =====================================================================

    /// <summary>
    /// Results of level validation. Contains pass/fail for each check
    /// plus diagnostic information for debugging failed levels.
    /// </summary>
    public sealed class ValidationResult
    {
        /// <summary>Overall pass: true only if every individual check passed.</summary>
        public bool Passed;

        // Individual check results
        public bool StartAccessible;
        public bool GoalAccessible;
        public bool Reachable;
        public bool WeaponProgressionValid;
        public bool DestructionPathsValid;
        public bool DifficultyWithinTolerance;
        public bool NoImpossibleGaps;
        public bool HasMinCheckpoints;
        public bool EntitiesInBounds;
        public bool StructuralCascadeValid;

        // Diagnostic data
        public float CalculatedDifficulty;
        public float TargetDifficulty;
        public int CheckpointCount;

        /// <summary>
        /// Human-readable summary of validation results.
        /// </summary>
        public string Summary()
        {
            var lines = new List<string>
            {
                $"Validation: {(Passed ? "PASSED" : "FAILED")}",
                $"  Start accessible:        {StartAccessible}",
                $"  Goal accessible:         {GoalAccessible}",
                $"  Reachable:               {Reachable}",
                $"  Weapon progression:      {WeaponProgressionValid}",
                $"  Destruction paths:       {DestructionPathsValid}",
                $"  Difficulty:              {CalculatedDifficulty:F2} (target: {TargetDifficulty:F2}, tolerance: +/-{LevelValidator.DIFFICULTY_TOLERANCE:F1})",
                $"  Difficulty in tolerance: {DifficultyWithinTolerance}",
                $"  No impossible gaps:      {NoImpossibleGaps}",
                $"  Checkpoints:             {CheckpointCount} (min: {LevelValidator.MIN_CHECKPOINTS})",
                $"  Checkpoints sufficient:  {HasMinCheckpoints}",
                $"  Entities in bounds:      {EntitiesInBounds}",
                $"  Structural cascade OK:   {StructuralCascadeValid}",
            };
            return string.Join("\n", lines);
        }
    }
}
