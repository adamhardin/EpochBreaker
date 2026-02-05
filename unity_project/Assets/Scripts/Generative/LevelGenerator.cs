using System;
using System.Collections.Generic;

namespace SixteenBit.Generative
{
    /// <summary>
    /// Core level generation pipeline. Produces a deterministic LevelData from a LevelID.
    ///
    /// Pipeline stages:
    ///   1. Macro layout: zone placement, pacing, difficulty curve
    ///   2. Destructible terrain: solid fill, material assignment, path carving, structural groups
    ///   3. Weapon drop placement: ensure player always has sufficient firepower
    ///   4. Enemy placement: era-specific types, cover/ambush integration
    ///   5. Reward and pickup placement: health, boosts, hidden rewards
    ///   6. Boss arena generation: platforms, pillars, boss type
    ///
    /// All randomness flows through XORShift64. No Unity math functions are used.
    /// Cross-platform deterministic for identical LevelID.
    /// </summary>
    public sealed class LevelGenerator
    {
        // =====================================================================
        // Constants
        // =====================================================================

        // --- Level dimension constants ---
        private const int BASE_WIDTH = 256;
        private const int LEVEL_HEIGHT = 16;
        private const int ANCHOR_INTERVAL = 8;
        private const int MIN_GROUND_HEIGHT = 2;
        private const int MAX_GROUND_HEIGHT = 12;

        // --- Zone proportions (spec: intro=10%, traversal=15%, destruction=25%,
        //     combat=20%, boss=20%, buffer=10%) ---
        private static readonly float[] ZONE_PROPORTIONS =
        {
            0.10f, // Intro
            0.15f, // Traversal
            0.25f, // Destruction
            0.20f, // Combat
            0.20f, // BossArena
            0.10f, // Buffer
        };

        private static readonly ZoneType[] ZONE_ORDER =
        {
            ZoneType.Intro,
            ZoneType.Traversal,
            ZoneType.Destruction,
            ZoneType.Combat,
            ZoneType.BossArena,
            ZoneType.Buffer,
        };

        // --- Destruction density per zone type ---
        private static readonly float[] ZONE_DESTRUCTION_DENSITY =
        {
            0.03f, // Intro
            0.04f, // Traversal
            0.08f, // Destruction
            0.05f, // Combat
            0.06f, // BossArena
            0.02f, // Buffer
        };

        // --- Solid fill density per zone type ---
        private static readonly float[] ZONE_SOLID_FILL =
        {
            0.04f, // Intro
            0.05f, // Traversal
            0.085f, // Destruction
            0.05f, // Combat
            0.06f, // BossArena
            0.03f, // Buffer
        };

        // --- Era material distributions: [era][5] = {soft, medium, hard, reinforced, indestructible} ---
        private static readonly float[][] ERA_MATERIAL_DISTRIBUTIONS =
        {
            new float[] { 0.60f, 0.25f, 0.10f, 0.00f, 0.05f }, // Era 0: Stone Age
            new float[] { 0.45f, 0.35f, 0.15f, 0.00f, 0.05f }, // Era 1: Bronze Age
            new float[] { 0.30f, 0.30f, 0.25f, 0.10f, 0.05f }, // Era 2: Classical
            new float[] { 0.20f, 0.30f, 0.30f, 0.15f, 0.05f }, // Era 3: Medieval
            new float[] { 0.20f, 0.25f, 0.30f, 0.20f, 0.05f }, // Era 4: Renaissance
            new float[] { 0.15f, 0.20f, 0.35f, 0.25f, 0.05f }, // Era 5: Industrial
            new float[] { 0.10f, 0.15f, 0.35f, 0.35f, 0.05f }, // Era 6: Modern
            new float[] { 0.10f, 0.15f, 0.30f, 0.40f, 0.05f }, // Era 7: Digital
            new float[] { 0.10f, 0.10f, 0.30f, 0.45f, 0.05f }, // Era 8: Spacefaring
            new float[] { 0.15f, 0.15f, 0.20f, 0.35f, 0.15f }, // Era 9: Transcendent
        };

        // --- Material HP: Soft=1, Medium=2, Hard=3, Reinforced=4 ---
        private static readonly byte[] MATERIAL_HP = { 0, 1, 2, 3, 4, 0 };

        // --- Hidden content chance by zone type ---
        private static readonly float[] HIDDEN_CONTENT_CHANCE =
        {
            0.05f, // Intro
            0.10f, // Traversal
            0.20f, // Destruction
            0.10f, // Combat
            0.15f, // BossArena
            0.05f, // Buffer
        };

        // --- Hidden content type weights: pathway=0.40, bridge=0.20, stairs=0.20, secret=0.20 ---
        private static readonly float[] HIDDEN_CONTENT_WEIGHTS = { 0.40f, 0.20f, 0.20f, 0.20f };

        // --- Ambush chance by zone type ---
        private static readonly float[] AMBUSH_CHANCE =
        {
            0.00f, // Intro
            0.05f, // Traversal
            0.15f, // Destruction
            0.20f, // Combat
            0.10f, // BossArena
            0.02f, // Buffer
        };

        // --- Entity placement constants ---
        private const int MIN_ENEMY_SEPARATION = 2;
        private const int HEALTH_PACK_INTERVAL = 20;
        private const int DAMAGE_BOOST_INTERVAL = 50;
        private const int CHECKPOINT_INTERVAL_TILES = 64;
        private const int BOSS_ARENA_WIDTH = 80;

        // --- Enemy type selection weights: easy=0.40, medium=0.30, hard=0.30 ---
        private static readonly float[] ENEMY_TIER_WEIGHTS = { 0.40f, 0.30f, 0.30f };

        /// <summary>
        /// Generate a complete level from a LevelID. This is the main entry point.
        /// The result is fully deterministic for a given LevelID.
        /// </summary>
        public LevelData Generate(LevelID id)
        {
            var rng = new XORShift64(id.Seed);

            // Level width scales with difficulty: base 256
            int width = BASE_WIDTH + id.Difficulty * 16;
            int height = LEVEL_HEIGHT;
            int totalTiles = width * height;

            var data = new LevelData
            {
                ID = id,
                Layout = new LevelLayout
                {
                    WidthTiles = width,
                    HeightTiles = height,
                    Tiles = new byte[totalTiles],
                    Collision = new byte[totalTiles],
                    Destructibles = new DestructibleTile[totalTiles],
                },
                Enemies = new List<EnemyData>(),
                WeaponDrops = new List<WeaponDropData>(),
                Rewards = new List<RewardData>(),
                Checkpoints = new List<CheckpointData>(),
            };

            // ---- Stage 1: Macro Layout ----
            data.Layout.Zones = GenerateZones(width, id.Difficulty, rng);

            // ---- Stage 2: Destructible Terrain Placement ----
            GenerateTerrain(data, id.Era, id.Difficulty, rng);

            // Set start position (Intro zone - unaffected by boss arena)
            data.Layout.StartX = 2;
            data.Layout.StartY = FindGroundLevel(data, 2) - 1;
            if (data.Layout.StartY < 0) data.Layout.StartY = 1;

            // Clear spawn area so player doesn't start inside blocks
            ClearSpawnArea(data, width, height);

            // ---- Stage 3: Weapon Drop Placement ----
            PlaceWeaponDrops(data, id.Era, rng);

            // ---- Stage 4: Enemy Placement ----
            PlaceEnemies(data, id.Difficulty, id.Era, rng);

            // ---- Stage 5: Reward & Pickup Placement ----
            PlaceRewards(data, id.Difficulty, rng);

            // ---- Stage 6: Boss Arena ----
            GenerateBossArena(data, id.Era, id.Difficulty, rng);

            // Set goal position AFTER boss arena generation, since the arena
            // flattens terrain at the end of the level and changes ground level
            data.Layout.GoalX = width - 3;
            data.Layout.GoalY = FindGroundLevel(data, width - 3) - 1;
            if (data.Layout.GoalY < 0) data.Layout.GoalY = 1;

            // Place checkpoints
            PlaceCheckpoints(data);

            // Compute metadata
            data.Metadata = ComputeMetadata(data, id.Difficulty);

            return data;
        }

        // =====================================================================
        // Stage 1: Macro Layout (Zone Placement)
        // =====================================================================

        private ZoneData[] GenerateZones(int levelWidth, int difficulty, XORShift64 rng)
        {
            // Zone order: Intro, Buffer, Traversal, Destruction, Buffer, Combat, BossArena
            // Simplified: we place all 6 zone types in the canonical order from the spec,
            // but interleave buffer zones. The spec says buffer=10%, so we split buffer
            // into two 5% segments placed between major zones.

            // Build zone sequence: Intro, Traversal, Buffer, Destruction, Combat, Buffer, BossArena
            ZoneType[] sequence =
            {
                ZoneType.Intro,
                ZoneType.Traversal,
                ZoneType.Buffer,
                ZoneType.Destruction,
                ZoneType.Combat,
                ZoneType.Buffer,
                ZoneType.BossArena,
            };

            float[] proportions =
            {
                0.10f,  // Intro
                0.15f,  // Traversal
                0.05f,  // Buffer (first half)
                0.25f,  // Destruction
                0.20f,  // Combat
                0.05f,  // Buffer (second half)
                0.20f,  // BossArena
            };

            // Add slight randomization to non-boss proportions
            for (int i = 0; i < proportions.Length; i++)
            {
                if (sequence[i] != ZoneType.BossArena)
                {
                    proportions[i] += rng.RangeFloat(-0.02f, 0.02f);
                    if (proportions[i] < 0.03f) proportions[i] = 0.03f;
                }
            }

            // Normalize proportions
            float total = 0f;
            for (int i = 0; i < proportions.Length; i++)
                total += proportions[i];
            for (int i = 0; i < proportions.Length; i++)
                proportions[i] /= total;

            var zones = new ZoneData[sequence.Length];
            int currentX = 0;

            // Difficulty progression across zones
            float baseDifficulty = difficulty * 1.5f;

            for (int i = 0; i < zones.Length; i++)
            {
                int zoneWidth;
                if (i == zones.Length - 1)
                {
                    zoneWidth = levelWidth - currentX;
                }
                else
                {
                    zoneWidth = (int)(levelWidth * proportions[i]);
                    if (zoneWidth < 4) zoneWidth = 4;
                }

                float zoneProgress = (float)i / Math.Max(1, zones.Length - 1);
                float diffMin = baseDifficulty + zoneProgress * 2.0f;
                float diffMax = diffMin + 1.5f;

                int zoneTypeIdx = GetZoneTypeIndex(sequence[i]);
                float destructionDensity = ZONE_DESTRUCTION_DENSITY[zoneTypeIdx];

                zones[i] = new ZoneData
                {
                    Type = sequence[i],
                    StartX = currentX,
                    EndX = currentX + zoneWidth,
                    DifficultyMin = diffMin,
                    DifficultyMax = diffMax,
                    DestructionDensity = destructionDensity,
                };

                currentX += zoneWidth;
            }

            return zones;
        }

        /// <summary>
        /// Map ZoneType enum to its index in the constant arrays (matching enum order).
        /// </summary>
        private int GetZoneTypeIndex(ZoneType type)
        {
            return type switch
            {
                ZoneType.Intro => 0,
                ZoneType.Traversal => 1,
                ZoneType.Destruction => 2,
                ZoneType.Combat => 3,
                ZoneType.BossArena => 4,
                ZoneType.Buffer => 5,
                _ => 5,
            };
        }

        // =====================================================================
        // Stage 2: Destructible Terrain Placement
        // =====================================================================

        private void GenerateTerrain(LevelData data, int era, int difficulty, XORShift64 rng)
        {
            int width = data.Layout.WidthTiles;
            int height = data.Layout.HeightTiles;

            // Step 1: Generate ground heights using anchor-interpolation
            float[] groundHeights = GenerateGroundHeights(width, rng);

            // Step 2: Fill base ground layer (indestructible floor)
            FillBaseGround(data, groundHeights);

            // Step 3: Fill zones with destructible material based on era distribution
            FillDestructibleTerrain(data, era, rng);

            // Step 4: Carve primary paths through destructible mass
            CarvePrimaryPaths(data, groundHeights, rng);

            // Step 5: Assign structural groups and mark load-bearing tiles
            AssignStructuralGroups(data);

            // Step 6: Embed hidden content behind destructible tiles
            EmbedHiddenContent(data, rng);

            // Step 7: Place platforms in traversal areas
            PlacePlatforms(data, rng);

            // Step 8: Apply edge tiles for visual polish
            ApplyEdgeTiles(data);
        }

        /// <summary>
        /// Deterministic terrain height generation using anchor-interpolation.
        /// Places random height anchors at fixed intervals and linearly interpolates
        /// between them, then applies smoothing passes. No Perlin noise.
        /// </summary>
        private float[] GenerateGroundHeights(int width, XORShift64 rng)
        {
            float[] heights = new float[width];
            int anchorCount = (width / ANCHOR_INTERVAL) + 2;
            float[] anchors = new float[anchorCount];

            // Generate random anchor heights
            for (int i = 0; i < anchors.Length; i++)
            {
                anchors[i] = rng.RangeFloat(MIN_GROUND_HEIGHT + 1, MAX_GROUND_HEIGHT - 1);
            }

            // Ensure start and end are at comfortable heights
            anchors[0] = rng.RangeFloat(8, 11);
            anchors[anchors.Length - 1] = rng.RangeFloat(8, 11);

            // Linear interpolation between anchors
            for (int x = 0; x < width; x++)
            {
                int anchorIdx = x / ANCHOR_INTERVAL;
                float t = (x % ANCHOR_INTERVAL) / (float)ANCHOR_INTERVAL;
                int nextIdx = Math.Min(anchorIdx + 1, anchors.Length - 1);
                heights[x] = anchors[anchorIdx] + (anchors[nextIdx] - anchors[anchorIdx]) * t;
            }

            // Two smoothing passes (weighted average with neighbors)
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 1; i < width - 1; i++)
                {
                    heights[i] = (heights[i - 1] + heights[i] * 2 + heights[i + 1]) / 4f;
                }
            }

            return heights;
        }

        /// <summary>
        /// Fill the base ground layer. Ground tiles below the ground height line
        /// are solid indestructible, and the top surface gets GroundTop tile type.
        /// </summary>
        private void FillBaseGround(LevelData data, float[] groundHeights)
        {
            int width = data.Layout.WidthTiles;
            int height = data.Layout.HeightTiles;

            for (int x = 0; x < width; x++)
            {
                int groundLevel = ClampInt((int)groundHeights[x], MIN_GROUND_HEIGHT, MAX_GROUND_HEIGHT);

                for (int y = 0; y < height; y++)
                {
                    int idx = y * width + x;

                    if (y >= groundLevel)
                    {
                        if (y == groundLevel)
                        {
                            data.Layout.Tiles[idx] = (byte)TileType.GroundTop;
                            data.Layout.Collision[idx] = (byte)CollisionType.Solid;
                        }
                        else
                        {
                            data.Layout.Tiles[idx] = (byte)TileType.Ground;
                            data.Layout.Collision[idx] = (byte)CollisionType.Solid;
                        }
                    }
                    else
                    {
                        data.Layout.Tiles[idx] = (byte)TileType.Empty;
                        data.Layout.Collision[idx] = (byte)CollisionType.None;
                    }

                    // Initialize all destructible data to empty
                    data.Layout.Destructibles[idx] = default;
                }
            }
        }

        /// <summary>
        /// Fill zones with destructible material based on era-specific distribution
        /// and zone-specific solid fill density. Material is placed in the airspace
        /// above the ground within each zone.
        /// </summary>
        private void FillDestructibleTerrain(LevelData data, int era, XORShift64 rng)
        {
            int width = data.Layout.WidthTiles;
            int height = data.Layout.HeightTiles;
            float[] materialWeights = ERA_MATERIAL_DISTRIBUTIONS[era];

            foreach (var zone in data.Layout.Zones)
            {
                int zoneTypeIdx = GetZoneTypeIndex(zone.Type);
                float solidFill = ZONE_SOLID_FILL[zoneTypeIdx];

                XORShift64 zoneRng = rng.Fork();

                for (int x = zone.StartX; x < zone.EndX && x < width; x++)
                {
                    // Find ground level at this column
                    int groundY = FindGroundLevel(data, x);

                    // Determine the fill region: from row 1 (leave row 0 open) down to
                    // just above the ground surface
                    int fillTop = 1;
                    int fillBottom = groundY - 1;
                    if (fillBottom <= fillTop) continue;

                    int columnHeight = fillBottom - fillTop;

                    for (int y = fillTop; y <= fillBottom; y++)
                    {
                        int idx = y * width + x;

                        // Only fill if currently empty
                        if (data.Layout.Tiles[idx] != (byte)TileType.Empty)
                            continue;

                        // Use solid fill density to decide whether to place material
                        if (!zoneRng.NextBool(solidFill))
                            continue;

                        // Select material class using era distribution weights
                        int matChoice = zoneRng.WeightedChoice(materialWeights);
                        MaterialClass matClass = (MaterialClass)(matChoice + 1);
                        // matChoice: 0=soft, 1=medium, 2=hard, 3=reinforced, 4=indestructible

                        TileType tileType;
                        CollisionType collisionType;
                        byte hp;

                        switch (matClass)
                        {
                            case MaterialClass.Soft:
                                tileType = TileType.DestructibleSoft;
                                collisionType = CollisionType.Destructible;
                                hp = 1;
                                break;
                            case MaterialClass.Medium:
                                tileType = TileType.DestructibleMedium;
                                collisionType = CollisionType.Destructible;
                                hp = 2;
                                break;
                            case MaterialClass.Hard:
                                tileType = TileType.DestructibleHard;
                                collisionType = CollisionType.Destructible;
                                hp = 3;
                                break;
                            case MaterialClass.Reinforced:
                                tileType = TileType.DestructibleReinforced;
                                collisionType = CollisionType.Destructible;
                                hp = 4;
                                break;
                            default: // Indestructible
                                tileType = TileType.Indestructible;
                                collisionType = CollisionType.Solid;
                                hp = 0;
                                matClass = MaterialClass.Indestructible;
                                break;
                        }

                        data.Layout.Tiles[idx] = (byte)tileType;
                        data.Layout.Collision[idx] = (byte)collisionType;
                        data.Layout.Destructibles[idx] = new DestructibleTile
                        {
                            MaterialClass = (byte)matClass,
                            HitPoints = hp,
                            MaxHitPoints = hp,
                            IsLoadBearing = false,
                            StructuralGroupId = 0,
                            HiddenContent = HiddenContentType.None,
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Carve primary paths through the destructible mass.
        /// Primary paths use only soft/medium materials so they are breakable
        /// with the starting weapon. The path follows the ground height contour
        /// with some vertical variation.
        /// </summary>
        private void CarvePrimaryPaths(LevelData data, float[] groundHeights, XORShift64 rng)
        {
            int width = data.Layout.WidthTiles;
            int height = data.Layout.HeightTiles;

            foreach (var zone in data.Layout.Zones)
            {
                // Skip intro (mostly open) and buffer (low density)
                if (zone.Type == ZoneType.Intro || zone.Type == ZoneType.Buffer)
                    continue;

                XORShift64 pathRng = rng.Fork();

                // Determine path Y -- roughly mid-way between top and ground
                int zoneStartX = Math.Max(zone.StartX, 0);
                int zoneEndX = Math.Min(zone.EndX, width);

                // Pick a starting path height
                int groundAtStart = FindGroundLevel(data, zoneStartX);
                int pathY = Math.Max(2, groundAtStart - 3 - pathRng.Range(0, 3));

                // Path width (vertical thickness): 2-3 tiles high
                int pathHeight = pathRng.Range(2, 4);

                for (int x = zoneStartX; x < zoneEndX; x++)
                {
                    int groundY = FindGroundLevel(data, x);

                    // Gradually vary path height
                    if (pathRng.NextBool(0.15f))
                    {
                        pathY += pathRng.Range(-1, 2);
                    }

                    // Clamp path within valid range
                    pathY = ClampInt(pathY, 1, groundY - pathHeight - 1);
                    if (pathY < 1) pathY = 1;

                    for (int dy = 0; dy < pathHeight; dy++)
                    {
                        int py = pathY + dy;
                        if (py < 0 || py >= height) continue;

                        int idx = py * width + x;
                        byte currentTile = data.Layout.Tiles[idx];

                        // If this is a destructible tile, replace it with soft or medium
                        // so the starting weapon can break through
                        if (IsDestructibleTile(currentTile))
                        {
                            bool useSoft = pathRng.NextBool(0.6f);
                            if (useSoft)
                            {
                                data.Layout.Tiles[idx] = (byte)TileType.DestructibleSoft;
                                data.Layout.Destructibles[idx].MaterialClass = (byte)MaterialClass.Soft;
                                data.Layout.Destructibles[idx].HitPoints = 1;
                                data.Layout.Destructibles[idx].MaxHitPoints = 1;
                            }
                            else
                            {
                                data.Layout.Tiles[idx] = (byte)TileType.DestructibleMedium;
                                data.Layout.Destructibles[idx].MaterialClass = (byte)MaterialClass.Medium;
                                data.Layout.Destructibles[idx].HitPoints = 2;
                                data.Layout.Destructibles[idx].MaxHitPoints = 2;
                            }
                            data.Layout.Collision[idx] = (byte)CollisionType.Destructible;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Assign structural groups to destructible tiles. Groups are column-based:
        /// each contiguous vertical run of destructible tiles in a column forms a group.
        /// The lowest tile in each group is marked as load-bearing.
        /// </summary>
        private void AssignStructuralGroups(LevelData data)
        {
            int width = data.Layout.WidthTiles;
            int height = data.Layout.HeightTiles;
            ushort groupId = 1;

            for (int x = 0; x < width; x++)
            {
                bool inGroup = false;
                int groupStartY = -1;

                for (int y = 0; y < height; y++)
                {
                    int idx = y * width + x;
                    bool isDestructible = IsDestructibleTile(data.Layout.Tiles[idx]);

                    if (isDestructible)
                    {
                        if (!inGroup)
                        {
                            inGroup = true;
                            groupStartY = y;
                        }

                        data.Layout.Destructibles[idx].StructuralGroupId = groupId;
                        data.Layout.Destructibles[idx].IsLoadBearing = false;
                    }
                    else
                    {
                        if (inGroup)
                        {
                            // End of group. Mark the bottom-most tile (y-1) as load-bearing
                            int bottomIdx = (y - 1) * width + x;
                            data.Layout.Destructibles[bottomIdx].IsLoadBearing = true;

                            groupId++;
                            if (groupId == 0) groupId = 1; // Avoid overflow to zero
                            inGroup = false;
                        }
                    }
                }

                // Handle group extending to bottom of level
                if (inGroup)
                {
                    int bottomIdx = (height - 1) * width + x;
                    data.Layout.Destructibles[bottomIdx].IsLoadBearing = true;
                    groupId++;
                    if (groupId == 0) groupId = 1;
                }
            }
        }

        /// <summary>
        /// Embed hidden content (pathway, bridge, stairs, secret) behind destructible tiles.
        /// Hidden content chance varies by zone type. Content type is selected by weighted
        /// distribution: pathway=40%, bridge=20%, stairs=20%, secret=20%.
        /// </summary>
        private void EmbedHiddenContent(LevelData data, XORShift64 rng)
        {
            int width = data.Layout.WidthTiles;
            int height = data.Layout.HeightTiles;

            foreach (var zone in data.Layout.Zones)
            {
                int zoneTypeIdx = GetZoneTypeIndex(zone.Type);
                float hiddenChance = HIDDEN_CONTENT_CHANCE[zoneTypeIdx];

                XORShift64 hiddenRng = rng.Fork();

                int zoneStartX = Math.Max(zone.StartX, 0);
                int zoneEndX = Math.Min(zone.EndX, width);

                for (int x = zoneStartX; x < zoneEndX; x++)
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        int idx = y * width + x;

                        if (!IsDestructibleTile(data.Layout.Tiles[idx]))
                            continue;

                        // Check if this tile should get hidden content
                        if (!hiddenRng.NextBool(hiddenChance))
                            continue;

                        // Verify there is empty space adjacent (the content needs to be
                        // behind/within the destructible mass, adjacent to open space)
                        bool adjacentEmpty = false;
                        if (x > 0 && data.Layout.Tiles[y * width + (x - 1)] == (byte)TileType.Empty)
                            adjacentEmpty = true;
                        if (x < width - 1 && data.Layout.Tiles[y * width + (x + 1)] == (byte)TileType.Empty)
                            adjacentEmpty = true;
                        if (y > 0 && data.Layout.Tiles[(y - 1) * width + x] == (byte)TileType.Empty)
                            adjacentEmpty = true;
                        if (y < height - 1 && data.Layout.Tiles[(y + 1) * width + x] == (byte)TileType.Empty)
                            adjacentEmpty = true;

                        if (!adjacentEmpty)
                            continue;

                        // Select hidden content type
                        int contentChoice = hiddenRng.WeightedChoice(HIDDEN_CONTENT_WEIGHTS);
                        HiddenContentType contentType = contentChoice switch
                        {
                            0 => HiddenContentType.Pathway,
                            1 => HiddenContentType.Bridge,
                            2 => HiddenContentType.Stairs,
                            3 => HiddenContentType.Secret,
                            _ => HiddenContentType.Pathway,
                        };

                        data.Layout.Destructibles[idx].HiddenContent = contentType;
                    }
                }
            }
        }

        /// <summary>
        /// Place platforms above ground in traversal and combat zones.
        /// </summary>
        private void PlacePlatforms(LevelData data, XORShift64 rng)
        {
            int width = data.Layout.WidthTiles;
            int height = data.Layout.HeightTiles;

            foreach (var zone in data.Layout.Zones)
            {
                float density = zone.Type switch
                {
                    ZoneType.Intro => 0.08f,
                    ZoneType.Traversal => 0.12f,
                    ZoneType.Destruction => 0.04f,
                    ZoneType.Combat => 0.06f,
                    ZoneType.BossArena => 0.04f,
                    ZoneType.Buffer => 0.06f,
                    _ => 0.06f,
                };

                int zoneWidth = zone.EndX - zone.StartX;
                int platformCount = Math.Max(1, (int)(zoneWidth * density));
                XORShift64 platRng = rng.Fork();

                for (int p = 0; p < platformCount; p++)
                {
                    int px = platRng.Range(
                        Math.Max(zone.StartX + 2, 0),
                        Math.Max(zone.StartX + 3, Math.Min(zone.EndX - 2, width))
                    );
                    if (px >= width) continue;

                    int groundY = FindGroundLevel(data, px);
                    int py = platRng.Range(Math.Max(1, groundY - 8), Math.Max(2, groundY - 3));
                    if (py < 1 || py >= height) continue;

                    int platformWidth = platRng.Range(3, 7);

                    for (int dx = 0; dx < platformWidth; dx++)
                    {
                        int tx = px + dx;
                        if (tx >= width || tx >= zone.EndX) break;

                        int idx = py * width + tx;
                        if (idx < 0 || idx >= data.Layout.Tiles.Length) continue;

                        if (data.Layout.Tiles[idx] == (byte)TileType.Empty)
                        {
                            data.Layout.Tiles[idx] = (byte)TileType.Platform;
                            data.Layout.Collision[idx] = (byte)CollisionType.PlatformPassthrough;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Apply edge tile variants (GroundLeft, GroundRight) for visual polish.
        /// </summary>
        private void ApplyEdgeTiles(LevelData data)
        {
            int width = data.Layout.WidthTiles;
            int height = data.Layout.HeightTiles;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int idx = y * width + x;
                    if (data.Layout.Tiles[idx] != (byte)TileType.GroundTop)
                        continue;

                    // Check left edge: is the tile to the left empty at this row?
                    if (x > 0)
                    {
                        int leftIdx = y * width + (x - 1);
                        if (data.Layout.Tiles[leftIdx] == (byte)TileType.Empty)
                        {
                            for (int dy = y + 1; dy < height; dy++)
                            {
                                int belowIdx = dy * width + x;
                                if (data.Layout.Tiles[belowIdx] == (byte)TileType.Ground)
                                    data.Layout.Tiles[belowIdx] = (byte)TileType.GroundLeft;
                                else
                                    break;
                            }
                        }
                    }

                    // Check right edge
                    if (x < width - 1)
                    {
                        int rightIdx = y * width + (x + 1);
                        if (data.Layout.Tiles[rightIdx] == (byte)TileType.Empty)
                        {
                            for (int dy = y + 1; dy < height; dy++)
                            {
                                int belowIdx = dy * width + x;
                                if (data.Layout.Tiles[belowIdx] == (byte)TileType.Ground)
                                    data.Layout.Tiles[belowIdx] = (byte)TileType.GroundRight;
                                else
                                    break;
                            }
                        }
                    }
                }
            }
        }

        // =====================================================================
        // Stage 3: Weapon Drop Placement
        // =====================================================================

        /// <summary>
        /// Place weapon drops so the player always has sufficient firepower for
        /// the materials ahead on the required path.
        ///
        /// Rules:
        ///   - Starting weapon breaks Soft(1 hit), Medium(3 hits)
        ///   - Medium weapon breaks Soft(1), Medium(1), Hard(3)
        ///   - Heavy weapon breaks Soft(1), Medium(1), Hard(1), Reinforced(3)
        ///   - At least 1 medium weapon before first hard material on required path
        ///   - At least 1 heavy weapon before first reinforced material on required path
        ///   - Weapon drops placed BEFORE zones that require them
        /// </summary>
        private void PlaceWeaponDrops(LevelData data, int era, XORShift64 rng)
        {
            int width = data.Layout.WidthTiles;
            int height = data.Layout.HeightTiles;

            // Scan left-to-right for the first X position where hard and reinforced
            // materials appear on the primary path region (middle vertical band).
            int firstHardX = -1;
            int firstReinforcedX = -1;

            for (int x = 0; x < width; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    int idx = y * width + x;
                    byte matClass = data.Layout.Destructibles[idx].MaterialClass;

                    if (matClass == (byte)MaterialClass.Hard && firstHardX < 0)
                    {
                        firstHardX = x;
                    }
                    if (matClass == (byte)MaterialClass.Reinforced && firstReinforcedX < 0)
                    {
                        firstReinforcedX = x;
                    }
                }

                if (firstHardX >= 0 && firstReinforcedX >= 0)
                    break;
            }

            XORShift64 weaponRng = rng.Fork();

            // Place medium weapon drop before first hard material
            if (firstHardX > 0)
            {
                int dropX = Math.Max(2, firstHardX - weaponRng.Range(5, 15));
                dropX = ClampInt(dropX, 2, width - 2);
                int groundY = FindGroundLevel(data, dropX);
                int dropY = Math.Max(1, groundY - 1);

                data.WeaponDrops.Add(new WeaponDropData
                {
                    TileX = dropX,
                    TileY = dropY,
                    Tier = WeaponTier.Medium,
                    OnPrimaryPath = true,
                    Hidden = false,
                });
            }

            // Place heavy weapon drop before first reinforced material
            if (firstReinforcedX > 0)
            {
                int dropX = Math.Max(2, firstReinforcedX - weaponRng.Range(5, 15));
                dropX = ClampInt(dropX, 2, width - 2);

                // Ensure heavy drop is after medium drop if both exist
                if (firstHardX > 0 && data.WeaponDrops.Count > 0 && dropX <= data.WeaponDrops[0].TileX)
                {
                    dropX = data.WeaponDrops[0].TileX + weaponRng.Range(3, 8);
                    dropX = ClampInt(dropX, 2, width - 2);
                }

                int groundY = FindGroundLevel(data, dropX);
                int dropY = Math.Max(1, groundY - 1);

                data.WeaponDrops.Add(new WeaponDropData
                {
                    TileX = dropX,
                    TileY = dropY,
                    Tier = WeaponTier.Heavy,
                    OnPrimaryPath = true,
                    Hidden = false,
                });
            }

            // Place additional weapon drops at zone boundaries where material
            // hardness increases. Scan each zone transition.
            WeaponTier currentBestWeapon = WeaponTier.Starting;

            foreach (var zone in data.Layout.Zones)
            {
                if (zone.Type == ZoneType.Intro || zone.Type == ZoneType.Buffer)
                    continue;

                // Scan forward in this zone for the hardest material on primary path
                MaterialClass hardestInZone = MaterialClass.None;
                int zoneStartX = Math.Max(zone.StartX, 0);
                int zoneEndX = Math.Min(zone.EndX, width);

                for (int x = zoneStartX; x < zoneEndX; x++)
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        int idx = y * width + x;
                        byte mc = data.Layout.Destructibles[idx].MaterialClass;
                        if (mc > (byte)hardestInZone && mc < (byte)MaterialClass.Indestructible)
                        {
                            hardestInZone = (MaterialClass)mc;
                        }
                    }
                }

                // Determine minimum weapon tier needed for this zone
                WeaponTier neededTier = WeaponTier.Starting;
                if (hardestInZone >= MaterialClass.Hard)
                    neededTier = WeaponTier.Medium;
                if (hardestInZone >= MaterialClass.Reinforced)
                    neededTier = WeaponTier.Heavy;

                // If player needs a better weapon than they have, place a drop before this zone
                if (neededTier > currentBestWeapon)
                {
                    int dropX = Math.Max(2, zoneStartX - weaponRng.Range(3, 10));
                    dropX = ClampInt(dropX, 2, width - 2);

                    // Check that we are not duplicating a drop at roughly the same location
                    bool tooClose = false;
                    for (int w = 0; w < data.WeaponDrops.Count; w++)
                    {
                        if (Math.Abs(data.WeaponDrops[w].TileX - dropX) < 5 &&
                            data.WeaponDrops[w].Tier >= neededTier)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (!tooClose)
                    {
                        int groundY = FindGroundLevel(data, dropX);
                        int dropY = Math.Max(1, groundY - 1);

                        data.WeaponDrops.Add(new WeaponDropData
                        {
                            TileX = dropX,
                            TileY = dropY,
                            Tier = neededTier,
                            OnPrimaryPath = true,
                            Hidden = false,
                        });
                    }

                    currentBestWeapon = neededTier;
                }
            }

            // Place optional hidden weapon drops in destruction zones for exploration reward
            foreach (var zone in data.Layout.Zones)
            {
                if (zone.Type != ZoneType.Destruction && zone.Type != ZoneType.Combat)
                    continue;

                if (!weaponRng.NextBool(0.4f))
                    continue;

                int zoneStartX = Math.Max(zone.StartX, 0);
                int zoneEndX = Math.Min(zone.EndX, width);
                int zoneWidth = zoneEndX - zoneStartX;
                if (zoneWidth < 8) continue;

                int hx = weaponRng.Range(zoneStartX + 2, zoneEndX - 2);
                int groundY = FindGroundLevel(data, hx);
                int hy = Math.Max(1, groundY - weaponRng.Range(2, 5));

                // Tier of hidden drop is one tier above what is strictly needed
                WeaponTier hiddenTier = zone.Type == ZoneType.Destruction
                    ? WeaponTier.Medium
                    : WeaponTier.Heavy;

                data.WeaponDrops.Add(new WeaponDropData
                {
                    TileX = hx,
                    TileY = hy,
                    Tier = hiddenTier,
                    OnPrimaryPath = false,
                    Hidden = true,
                });
            }
        }

        // =====================================================================
        // Stage 4: Enemy Placement
        // =====================================================================

        /// <summary>
        /// Place enemies throughout the level. Enemy count = 3 + (difficulty * 4).
        /// Era-specific types selected by weighted RNG. No enemies in intro zone.
        /// Minimum 2 tile separation between enemies.
        /// </summary>
        private void PlaceEnemies(LevelData data, int difficulty, int era, XORShift64 rng)
        {
            int width = data.Layout.WidthTiles;
            int height = data.Layout.HeightTiles;
            int totalEnemyCount = 3 + (difficulty * 4);

            // Enemy base index for this era
            int eraBase = era * 3;

            XORShift64 enemyRng = rng.Fork();

            // Build list of valid zones (skip intro)
            var validZones = new List<ZoneData>();
            foreach (var zone in data.Layout.Zones)
            {
                if (zone.Type != ZoneType.Intro)
                    validZones.Add(zone);
            }

            if (validZones.Count == 0) return;

            // Distribute enemies across valid zones proportionally to zone width,
            // with combat and boss zones getting extra weight
            float totalWeight = 0f;
            float[] zoneWeights = new float[validZones.Count];
            for (int i = 0; i < validZones.Count; i++)
            {
                float zoneWidth = validZones[i].EndX - validZones[i].StartX;
                float typeMultiplier = validZones[i].Type switch
                {
                    ZoneType.Combat => 2.0f,
                    ZoneType.Destruction => 1.2f,
                    ZoneType.BossArena => 0.5f, // Boss zone gets few regular enemies
                    ZoneType.Buffer => 0.3f,
                    _ => 1.0f,
                };
                zoneWeights[i] = zoneWidth * typeMultiplier;
                totalWeight += zoneWeights[i];
            }

            // Track placed enemy X positions for separation check
            var placedEnemyPositions = new List<int>();

            int enemiesPlaced = 0;
            int attempts = 0;
            int maxAttempts = totalEnemyCount * 10;

            while (enemiesPlaced < totalEnemyCount && attempts < maxAttempts)
            {
                attempts++;

                // Pick a zone weighted by distribution
                int zoneIdx = enemyRng.WeightedChoice(zoneWeights);
                var zone = validZones[zoneIdx];
                int zoneTypeIdx = GetZoneTypeIndex(zone.Type);

                int zoneStartX = Math.Max(zone.StartX + 1, 0);
                int zoneEndX = Math.Min(zone.EndX - 1, width);
                if (zoneEndX <= zoneStartX) continue;

                int ex = enemyRng.Range(zoneStartX, zoneEndX);
                if (ex < 0 || ex >= width) continue;

                // Check minimum separation from all placed enemies
                bool tooClose = false;
                for (int p = 0; p < placedEnemyPositions.Count; p++)
                {
                    if (Math.Abs(placedEnemyPositions[p] - ex) < MIN_ENEMY_SEPARATION)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                int groundY = FindGroundLevel(data, ex);
                if (groundY <= 1 || groundY >= height) continue;

                // Select enemy type using era base + weighted tier choice
                int tierChoice = enemyRng.WeightedChoice(ENEMY_TIER_WEIGHTS);
                int enemyTypeIndex = eraBase + tierChoice;
                if (enemyTypeIndex > 29) enemyTypeIndex = 29;
                EnemyType enemyType = (EnemyType)enemyTypeIndex;

                // Determine behavior
                EnemyBehavior behavior;
                float behaviorRoll = enemyRng.RangeFloat(0f, 1f);
                if (behaviorRoll < 0.1f)
                    behavior = EnemyBehavior.Flying;
                else if (behaviorRoll < 0.25f)
                    behavior = EnemyBehavior.Stationary;
                else if (behaviorRoll < 0.6f)
                    behavior = EnemyBehavior.Patrol;
                else
                    behavior = EnemyBehavior.Chase;

                // Position: flying enemies are above ground, others on ground
                int ey;
                if (behavior == EnemyBehavior.Flying)
                {
                    ey = enemyRng.Range(Math.Max(1, groundY - 7), Math.Max(2, groundY - 2));
                }
                else
                {
                    ey = groundY - 1;
                }
                if (ey < 0) ey = 1;
                if (ey >= height) ey = height - 2;

                // Patrol bounds
                int zoneWidth = zoneEndX - zoneStartX;
                int patrolRadius = enemyRng.Range(3, Math.Max(4, zoneWidth / 4));
                int patrolMin = Math.Max(zoneStartX, ex - patrolRadius);
                int patrolMax = Math.Min(zoneEndX - 1, ex + patrolRadius);

                // Cover and ambush flags
                float ambushChance = AMBUSH_CHANCE[zoneTypeIdx];
                bool isAmbush = enemyRng.NextBool(ambushChance);

                // UsesCover: check if there are destructible tiles adjacent
                bool usesCover = false;
                if (!isAmbush)
                {
                    // Check tiles around enemy position for destructible cover
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        int cx = ex + dx;
                        if (cx < 0 || cx >= width) continue;
                        int coverIdx = ey * width + cx;
                        if (coverIdx >= 0 && coverIdx < data.Layout.Tiles.Length &&
                            IsDestructibleTile(data.Layout.Tiles[coverIdx]))
                        {
                            usesCover = true;
                            break;
                        }
                    }
                }

                // Difficulty weight: scales with enemy tier and zone difficulty
                float diffWeight = (tierChoice + 1) * 0.5f +
                    (zone.DifficultyMin + zone.DifficultyMax) * 0.1f;

                data.Enemies.Add(new EnemyData
                {
                    TileX = ex,
                    TileY = ey,
                    Type = enemyType,
                    Behavior = behavior,
                    PatrolMinX = patrolMin,
                    PatrolMaxX = patrolMax,
                    DifficultyWeight = diffWeight,
                    UsesCover = usesCover,
                    IsAmbush = isAmbush,
                });

                placedEnemyPositions.Add(ex);
                enemiesPlaced++;
            }
        }

        // =====================================================================
        // Stage 5: Reward & Pickup Placement
        // =====================================================================

        /// <summary>
        /// Place rewards and pickups:
        ///   - Health pack every 20 tiles
        ///   - Damage boost every 50 tiles
        ///   - Hidden rewards behind destructible walls (10% chance)
        ///   - Post-combat rewards after enemy clusters
        /// </summary>
        private void PlaceRewards(LevelData data, int difficulty, XORShift64 rng)
        {
            int width = data.Layout.WidthTiles;
            int height = data.Layout.HeightTiles;

            XORShift64 rewardRng = rng.Fork();

            // Place health packs every HEALTH_PACK_INTERVAL tiles
            for (int x = HEALTH_PACK_INTERVAL; x < width - 4; x += HEALTH_PACK_INTERVAL)
            {
                int rx = x + rewardRng.Range(-3, 4);
                rx = ClampInt(rx, 2, width - 2);

                int groundY = FindGroundLevel(data, rx);
                if (groundY <= 1) continue;

                int ry = groundY - 1;

                // Check for platform above -- place there sometimes
                for (int py = Math.Max(0, ry - 6); py < ry; py++)
                {
                    int pidx = py * width + rx;
                    if (pidx >= 0 && pidx < data.Layout.Tiles.Length &&
                        data.Layout.Tiles[pidx] == (byte)TileType.Platform)
                    {
                        if (rewardRng.NextBool(0.3f))
                            ry = py - 1;
                        break;
                    }
                }

                if (ry < 0) ry = 0;

                // Alternate between small and large health packs based on difficulty
                RewardType healthType = rewardRng.NextBool(0.7f)
                    ? RewardType.HealthSmall
                    : RewardType.HealthLarge;

                int healthValue = healthType == RewardType.HealthSmall ? 10 : 25;

                data.Rewards.Add(new RewardData
                {
                    TileX = rx,
                    TileY = ry,
                    Type = healthType,
                    Value = healthValue,
                    Hidden = false,
                });
            }

            // Place damage boosts every DAMAGE_BOOST_INTERVAL tiles
            for (int x = DAMAGE_BOOST_INTERVAL; x < width - 4; x += DAMAGE_BOOST_INTERVAL)
            {
                int rx = x + rewardRng.Range(-4, 5);
                rx = ClampInt(rx, 2, width - 2);

                int groundY = FindGroundLevel(data, rx);
                if (groundY <= 1) continue;

                int ry = groundY - 1;
                if (ry < 0) ry = 0;

                data.Rewards.Add(new RewardData
                {
                    TileX = rx,
                    TileY = ry,
                    Type = RewardType.AttackBoost,
                    Value = 1,
                    Hidden = false,
                });
            }

            // Hidden rewards behind destructible walls (10% chance per eligible wall)
            PlaceHiddenRewards(data, rewardRng);

            // Post-combat rewards after enemy clusters
            PlacePostCombatRewards(data, rewardRng);
        }

        /// <summary>
        /// Scan destructible tiles and place hidden rewards behind them
        /// with a 10% chance per eligible position.
        /// </summary>
        private void PlaceHiddenRewards(LevelData data, XORShift64 rng)
        {
            int width = data.Layout.WidthTiles;
            int height = data.Layout.HeightTiles;

            // Sample positions to avoid checking every single tile (performance)
            // Check every 8th column, every 3rd row
            for (int x = 4; x < width - 4; x += 8)
            {
                for (int y = 2; y < height - 2; y += 3)
                {
                    int idx = y * width + x;
                    if (!IsDestructibleTile(data.Layout.Tiles[idx]))
                        continue;

                    // Check if there is empty space behind/below this tile
                    bool hasEmptyNeighbor = false;
                    if (x + 1 < width && data.Layout.Tiles[y * width + (x + 1)] == (byte)TileType.Empty)
                        hasEmptyNeighbor = true;
                    if (x - 1 >= 0 && data.Layout.Tiles[y * width + (x - 1)] == (byte)TileType.Empty)
                        hasEmptyNeighbor = true;

                    if (!hasEmptyNeighbor) continue;

                    // 10% chance for hidden reward
                    if (!rng.NextBool(0.10f))
                        continue;

                    // Select reward type
                    float[] hiddenRewardWeights = { 0.25f, 0.10f, 0.15f, 0.10f, 0.10f, 0.30f };
                    RewardType rewardType = (RewardType)rng.WeightedChoice(hiddenRewardWeights);

                    int value = rewardType switch
                    {
                        RewardType.HealthSmall => 10,
                        RewardType.HealthLarge => 25,
                        RewardType.AttackBoost => 1,
                        RewardType.SpeedBoost => 1,
                        RewardType.Shield => 1,
                        RewardType.Coin => rng.Range(1, 6),
                        _ => 1,
                    };

                    data.Rewards.Add(new RewardData
                    {
                        TileX = x,
                        TileY = y,
                        Type = rewardType,
                        Value = value,
                        Hidden = true,
                    });
                }
            }
        }

        /// <summary>
        /// Place health rewards after enemy clusters (3+ enemies within 10 tiles).
        /// </summary>
        private void PlacePostCombatRewards(LevelData data, XORShift64 rng)
        {
            if (data.Enemies.Count < 3) return;

            int width = data.Layout.WidthTiles;

            // Sort enemies by X position for cluster detection
            var sortedEnemies = new List<EnemyData>(data.Enemies);
            sortedEnemies.Sort((a, b) => a.TileX.CompareTo(b.TileX));

            int i = 0;
            while (i < sortedEnemies.Count - 2)
            {
                int clusterStart = sortedEnemies[i].TileX;
                int clusterEnd = clusterStart;
                int count = 1;

                for (int j = i + 1; j < sortedEnemies.Count; j++)
                {
                    if (sortedEnemies[j].TileX - clusterStart <= 10)
                    {
                        clusterEnd = sortedEnemies[j].TileX;
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (count >= 3)
                {
                    int rx = Math.Min(clusterEnd + rng.Range(2, 5), width - 2);
                    int groundY = FindGroundLevel(data, rx);
                    if (groundY > 1)
                    {
                        data.Rewards.Add(new RewardData
                        {
                            TileX = rx,
                            TileY = Math.Max(0, groundY - 1),
                            Type = RewardType.HealthSmall,
                            Value = 10,
                            Hidden = false,
                        });
                    }
                    i += count;
                }
                else
                {
                    i++;
                }
            }
        }

        // =====================================================================
        // Stage 6: Boss Arena
        // =====================================================================

        /// <summary>
        /// Generate the boss arena within the BossArena zone.
        /// Arena width = 80 tiles, platforms = 3 + difficulty,
        /// destructible pillars = 2 + difficulty.
        /// Boss type = BossType matching the era.
        /// </summary>
        private void GenerateBossArena(LevelData data, int era, int difficulty, XORShift64 rng)
        {
            int width = data.Layout.WidthTiles;
            int height = data.Layout.HeightTiles;

            // Find the boss arena zone
            ZoneData? bossZoneOpt = FindZone(data, ZoneType.BossArena);
            if (!bossZoneOpt.HasValue) return;

            ZoneData bossZone = bossZoneOpt.Value;
            XORShift64 bossRng = rng.Fork();

            int arenaStartX = bossZone.StartX;
            int arenaEndX = Math.Min(bossZone.EndX, width);
            int arenaWidth = arenaEndX - arenaStartX;

            // Ensure arena is at least a minimum width
            if (arenaWidth < 20) return;

            // Clear the arena floor area to create a flat fighting space
            // Find average ground height in the arena
            int totalGroundY = 0;
            int groundSamples = 0;
            for (int x = arenaStartX; x < arenaEndX; x += 4)
            {
                int gy = FindGroundLevel(data, Math.Min(x, width - 1));
                if (gy > 0 && gy < height)
                {
                    totalGroundY += gy;
                    groundSamples++;
                }
            }
            int arenaGroundY = groundSamples > 0 ? totalGroundY / groundSamples : height - 3;
            arenaGroundY = ClampInt(arenaGroundY, height - 5, height - 2);

            // Flatten the arena floor
            for (int x = arenaStartX; x < arenaEndX && x < width; x++)
            {
                // Set ground at arena floor level
                for (int y = 0; y < height; y++)
                {
                    int idx = y * width + x;
                    if (y < arenaGroundY)
                    {
                        // Clear space above arena floor (remove destructibles)
                        if (y > 0)
                        {
                            data.Layout.Tiles[idx] = (byte)TileType.Empty;
                            data.Layout.Collision[idx] = (byte)CollisionType.None;
                            data.Layout.Destructibles[idx] = default;
                        }
                    }
                    else if (y == arenaGroundY)
                    {
                        data.Layout.Tiles[idx] = (byte)TileType.GroundTop;
                        data.Layout.Collision[idx] = (byte)CollisionType.Solid;
                        data.Layout.Destructibles[idx] = default;
                    }
                    else
                    {
                        data.Layout.Tiles[idx] = (byte)TileType.Ground;
                        data.Layout.Collision[idx] = (byte)CollisionType.Solid;
                        data.Layout.Destructibles[idx] = default;
                    }
                }
            }

            // Place platforms: 3 + difficulty
            int platformCount = 3 + difficulty;
            for (int p = 0; p < platformCount; p++)
            {
                int platSpacing = arenaWidth / (platformCount + 1);
                int px = arenaStartX + platSpacing * (p + 1) + bossRng.Range(-2, 3);
                px = ClampInt(px, arenaStartX + 2, arenaEndX - 4);

                int py = arenaGroundY - bossRng.Range(3, 6);
                py = ClampInt(py, 2, arenaGroundY - 2);

                int platWidth = bossRng.Range(3, 6);

                for (int dx = 0; dx < platWidth; dx++)
                {
                    int tx = px + dx;
                    if (tx >= arenaEndX || tx >= width) break;

                    int idx = py * width + tx;
                    if (idx >= 0 && idx < data.Layout.Tiles.Length)
                    {
                        data.Layout.Tiles[idx] = (byte)TileType.Platform;
                        data.Layout.Collision[idx] = (byte)CollisionType.PlatformPassthrough;
                    }
                }
            }

            // Place destructible pillars: 2 + difficulty
            int pillarCount = 2 + difficulty;
            float[] eraMaterials = ERA_MATERIAL_DISTRIBUTIONS[era];

            for (int p = 0; p < pillarCount; p++)
            {
                int pillarSpacing = arenaWidth / (pillarCount + 1);
                int px = arenaStartX + pillarSpacing * (p + 1) + bossRng.Range(-3, 4);
                px = ClampInt(px, arenaStartX + 3, arenaEndX - 3);

                int pillarHeight = bossRng.Range(3, 6);
                int pillarBottom = arenaGroundY - 1;
                int pillarTop = Math.Max(2, pillarBottom - pillarHeight);

                // Select pillar material (use era distribution but bias toward harder materials)
                float[] pillarWeights = new float[5];
                for (int m = 0; m < 5; m++)
                {
                    pillarWeights[m] = eraMaterials[m];
                }
                // Bias toward harder materials for pillars
                pillarWeights[2] += 0.15f; // hard
                pillarWeights[3] += 0.10f; // reinforced

                int matChoice = bossRng.WeightedChoice(pillarWeights);
                MaterialClass pillarMat = (MaterialClass)(matChoice + 1);
                byte pillarHP = MATERIAL_HP[(int)pillarMat];
                TileType pillarTile = MaterialToTileType(pillarMat);

                for (int y = pillarTop; y <= pillarBottom; y++)
                {
                    int idx = y * width + px;
                    if (idx < 0 || idx >= data.Layout.Tiles.Length) continue;

                    data.Layout.Tiles[idx] = (byte)pillarTile;
                    data.Layout.Collision[idx] = (byte)CollisionType.Destructible;
                    data.Layout.Destructibles[idx] = new DestructibleTile
                    {
                        MaterialClass = (byte)pillarMat,
                        HitPoints = pillarHP,
                        MaxHitPoints = pillarHP,
                        IsLoadBearing = (y == pillarBottom),
                        StructuralGroupId = (ushort)(60000 + p),
                        HiddenContent = HiddenContentType.None,
                    };
                }
            }

            // Place boss entry point checkpoint
            // (Boss itself is not an EnemyData - it's handled by the boss system,
            // but we record the arena zone and type in metadata via the checkpoint.)

            // Add a boss arena checkpoint at arena entrance
            int bossCheckpointX = arenaStartX + 3;
            if (bossCheckpointX < width)
            {
                data.Checkpoints.Add(new CheckpointData
                {
                    TileX = bossCheckpointX,
                    TileY = Math.Max(0, arenaGroundY - 1),
                    Type = CheckpointType.BossArena,
                });
            }
        }

        // =====================================================================
        // Checkpoint Placement
        // =====================================================================

        private void PlaceCheckpoints(LevelData data)
        {
            int width = data.Layout.WidthTiles;
            int height = data.Layout.HeightTiles;

            // Level start checkpoint
            data.Checkpoints.Insert(0, new CheckpointData
            {
                TileX = data.Layout.StartX,
                TileY = data.Layout.StartY,
                Type = CheckpointType.LevelStart,
            });

            // Mid-level checkpoints at regular intervals
            for (int x = CHECKPOINT_INTERVAL_TILES; x < width - BOSS_ARENA_WIDTH - 10; x += CHECKPOINT_INTERVAL_TILES)
            {
                int groundY = FindGroundLevel(data, x);
                if (groundY > 1 && groundY < height)
                {
                    data.Checkpoints.Add(new CheckpointData
                    {
                        TileX = x,
                        TileY = groundY - 1,
                        Type = CheckpointType.MidLevel,
                    });
                }
            }

            // Pre-boss checkpoint
            ZoneData? bossZone = FindZone(data, ZoneType.BossArena);
            if (bossZone.HasValue)
            {
                int preBossX = bossZone.Value.StartX - 3;
                if (preBossX > 0 && preBossX < width)
                {
                    int groundY = FindGroundLevel(data, preBossX);
                    if (groundY > 1 && groundY < height)
                    {
                        data.Checkpoints.Add(new CheckpointData
                        {
                            TileX = preBossX,
                            TileY = groundY - 1,
                            Type = CheckpointType.PreBoss,
                        });
                    }
                }
            }
        }

        // =====================================================================
        // Metadata
        // =====================================================================

        private LevelMetadata ComputeMetadata(LevelData data, int difficulty)
        {
            float calculatedDifficulty = 0f;

            // Sum enemy difficulty weights
            for (int i = 0; i < data.Enemies.Count; i++)
                calculatedDifficulty += data.Enemies[i].DifficultyWeight;

            // Normalize by level width
            calculatedDifficulty /= (data.Layout.WidthTiles / 100f);

            // Count destructible tiles and compute destruction ratio
            int destructibleCount = 0;
            float totalDestructionDensity = 0f;
            int zoneCount = data.Layout.Zones != null ? data.Layout.Zones.Length : 0;

            int totalTiles = data.Layout.WidthTiles * data.Layout.HeightTiles;
            for (int i = 0; i < totalTiles; i++)
            {
                if (IsDestructibleTile(data.Layout.Tiles[i]))
                    destructibleCount++;
            }

            for (int i = 0; i < zoneCount; i++)
            {
                totalDestructionDensity += data.Layout.Zones[i].DestructionDensity;
            }
            float avgDestructionDensity = zoneCount > 0
                ? totalDestructionDensity / zoneCount
                : 0.5f;

            return new LevelMetadata
            {
                GenerationTimeMs = 0f, // Set by caller via stopwatch
                CalculatedDifficulty = calculatedDifficulty,
                DestructionRatioTarget = avgDestructionDensity,
                TotalEnemies = data.Enemies.Count,
                TotalWeaponDrops = data.WeaponDrops.Count,
                TotalRewards = data.Rewards.Count,
                TotalCheckpoints = data.Checkpoints.Count,
                TotalDestructibleTiles = destructibleCount,
            };
        }

        // =====================================================================
        // Utility Methods
        // =====================================================================

        /// <summary>
        /// Clear blocks around the player spawn point so they don't start trapped.
        /// Clears a 5-wide × 3-tall area (in level-space) above the ground at spawn.
        /// </summary>
        private void ClearSpawnArea(LevelData data, int width, int height)
        {
            int sx = data.Layout.StartX;
            int sy = data.Layout.StartY;

            // Clear from sx-1 to sx+3 (5 wide), sy-2 to sy (3 tall above ground)
            // In level-space, up = decreasing y
            for (int dx = -1; dx <= 3; dx++)
            {
                for (int dy = 0; dy >= -2; dy--)
                {
                    int cx = sx + dx;
                    int cy = sy + dy;
                    if (cx < 0 || cx >= width || cy < 0 || cy >= height) continue;

                    int idx = cy * width + cx;
                    byte tileType = data.Layout.Tiles[idx];

                    // Don't remove ground tiles
                    if (tileType == (byte)TileType.GroundTop || tileType == (byte)TileType.Ground)
                        continue;

                    data.Layout.Tiles[idx] = (byte)TileType.Empty;
                    data.Layout.Collision[idx] = (byte)CollisionType.None;
                    data.Layout.Destructibles[idx] = default;
                }
            }
        }

        /// <summary>
        /// Find the Y coordinate of the topmost solid ground at column x.
        /// Scans from top (y=0) downward. Returns LEVEL_HEIGHT if no ground found.
        /// </summary>
        private int FindGroundLevel(LevelData data, int x)
        {
            int width = data.Layout.WidthTiles;
            int height = data.Layout.HeightTiles;
            if (x < 0 || x >= width) return height;

            for (int y = 0; y < height; y++)
            {
                int idx = y * width + x;
                if (idx >= data.Layout.Collision.Length) break;

                if (data.Layout.Collision[idx] == (byte)CollisionType.Solid)
                    return y;
            }

            return height;
        }

        /// <summary>
        /// Find the first zone of the given type.
        /// </summary>
        private ZoneData? FindZone(LevelData data, ZoneType type)
        {
            if (data.Layout.Zones == null) return null;
            for (int i = 0; i < data.Layout.Zones.Length; i++)
            {
                if (data.Layout.Zones[i].Type == type)
                    return data.Layout.Zones[i];
            }
            return null;
        }

        /// <summary>
        /// Check if a tile type byte represents a destructible tile.
        /// </summary>
        private bool IsDestructibleTile(byte tileType)
        {
            return tileType == (byte)TileType.DestructibleSoft ||
                   tileType == (byte)TileType.DestructibleMedium ||
                   tileType == (byte)TileType.DestructibleHard ||
                   tileType == (byte)TileType.DestructibleReinforced ||
                   tileType == (byte)TileType.Indestructible;
        }

        /// <summary>
        /// Map a MaterialClass to its corresponding TileType.
        /// </summary>
        private TileType MaterialToTileType(MaterialClass mat)
        {
            return mat switch
            {
                MaterialClass.Soft => TileType.DestructibleSoft,
                MaterialClass.Medium => TileType.DestructibleMedium,
                MaterialClass.Hard => TileType.DestructibleHard,
                MaterialClass.Reinforced => TileType.DestructibleReinforced,
                MaterialClass.Indestructible => TileType.Indestructible,
                _ => TileType.DestructibleSoft,
            };
        }

        /// <summary>
        /// Integer clamp without System.Math dependency on Math.Clamp (which
        /// requires .NET Core 2.0+). Keeps compatibility with older Unity runtimes.
        /// </summary>
        private int ClampInt(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
