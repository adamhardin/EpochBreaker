using System.Collections.Generic;

namespace EpochBreaker.Generative
{
    /// <summary>
    /// Builds 3 hardcoded tutorial levels that teach mechanics in sequence.
    /// Outputs standard LevelData objects so the existing LevelLoader/LevelRenderer
    /// pipeline works unchanged.
    ///
    /// Level 1 "Move & Jump" (64 tiles): flat terrain, gaps, platforms
    /// Level 2 "Shoot & Smash" (80 tiles): destructibles, targets, stomp, weapon pickup
    /// Level 3 "Climb & Survive" (96 tiles): wall-jump, enemies, mini-boss
    /// </summary>
    public static class TutorialLevelBuilder
    {
        private const int HEIGHT = 16;
        // Ground in level-data coords (y=0 top). Ground at y=12 = Unity y=3.
        private const int GROUND_Y = 12;

        public static LevelData BuildTutorial(int index)
        {
            return index switch
            {
                0 => BuildLevel1_MoveAndJump(),
                1 => BuildLevel2_ShootAndSmash(),
                2 => BuildLevel3_ClimbAndSurvive(),
                _ => BuildLevel1_MoveAndJump()
            };
        }

        /// <summary>
        /// Level 1: Move & Jump — 64 tiles wide.
        /// Flat terrain with gaps of increasing width and platforms.
        /// No enemies, no destructibles. Pure movement teaching.
        /// </summary>
        private static LevelData BuildLevel1_MoveAndJump()
        {
            int width = 64;
            var tiles = new byte[width * HEIGHT];
            var collision = new byte[width * HEIGHT];
            var destructibles = new DestructibleTile[width * HEIGHT];

            // Fill ground from y=GROUND_Y to y=15
            for (int x = 0; x < width; x++)
            {
                for (int y = GROUND_Y; y < HEIGHT; y++)
                {
                    SetSolid(tiles, collision, width, x, y);
                }
            }

            // Gap 1: 2-tile gap at x=16-17
            ClearColumn(tiles, collision, width, 16);
            ClearColumn(tiles, collision, width, 17);

            // Gap 2: 3-tile gap at x=24-26
            ClearColumn(tiles, collision, width, 24);
            ClearColumn(tiles, collision, width, 25);
            ClearColumn(tiles, collision, width, 26);

            // Floating platform in middle of gap 2 (1 tile at y=10)
            SetPlatform(tiles, collision, width, 25, 10);

            // Gap 3: 4-tile gap at x=34-37 with stepping platforms
            for (int gx = 34; gx <= 37; gx++)
                ClearColumn(tiles, collision, width, gx);
            SetPlatform(tiles, collision, width, 35, 10);
            SetPlatform(tiles, collision, width, 36, 10);

            // Staircase section: x=44-50, platforms ascending
            SetPlatform(tiles, collision, width, 44, 10);
            SetPlatform(tiles, collision, width, 45, 10);
            SetPlatform(tiles, collision, width, 47, 8);
            SetPlatform(tiles, collision, width, 48, 8);
            SetPlatform(tiles, collision, width, 50, 6);
            SetPlatform(tiles, collision, width, 51, 6);

            // Drop down section: gap at x=54-56
            for (int gx = 54; gx <= 56; gx++)
                ClearColumn(tiles, collision, width, gx);

            // Goal position
            int startX = 2;
            int startY = GROUND_Y - 1;
            int goalX = width - 4;
            int goalY = GROUND_Y - 1;

            // Rewards: coins along the path as breadcrumbs
            var rewards = new List<RewardData>();
            int[] coinXs = { 10, 16, 20, 25, 30, 35, 40, 45, 50, 55, 58 };
            foreach (int cx in coinXs)
            {
                if (cx < width)
                    rewards.Add(new RewardData { TileX = cx, TileY = GROUND_Y - 2, Type = RewardType.Coin, Value = 25 });
            }

            var zones = new ZoneData[]
            {
                new ZoneData { Type = ZoneType.Intro, StartX = 0, EndX = width - 1,
                    DifficultyMin = 0f, DifficultyMax = 0f, DestructionDensity = 0f }
            };

            var checkpoints = new List<CheckpointData>
            {
                new CheckpointData { TileX = startX, TileY = GROUND_Y, Type = CheckpointType.LevelStart },
                new CheckpointData { TileX = 32, TileY = GROUND_Y, Type = CheckpointType.MidLevel }
            };

            return new LevelData
            {
                ID = LevelID.Create(0, 1), // Tutorial epoch 0, seed 1
                Layout = new LevelLayout
                {
                    WidthTiles = width,
                    HeightTiles = HEIGHT,
                    StartX = startX,
                    StartY = startY,
                    GoalX = goalX,
                    GoalY = goalY,
                    Tiles = tiles,
                    Collision = collision,
                    Destructibles = destructibles,
                    Zones = zones,
                },
                Enemies = new List<EnemyData>(),
                WeaponDrops = new List<WeaponDropData>(),
                Rewards = rewards,
                Checkpoints = checkpoints,
                Metadata = ComputeMetadata(tiles, collision, destructibles, width,
                    new List<EnemyData>(), new List<WeaponDropData>(), rewards, checkpoints),
            };
        }

        /// <summary>
        /// Level 2: Shoot & Smash — 80 tiles wide.
        /// Soft destructible walls, stationary target enemies, stomp section, weapon pickup.
        /// </summary>
        private static LevelData BuildLevel2_ShootAndSmash()
        {
            int width = 80;
            var tiles = new byte[width * HEIGHT];
            var collision = new byte[width * HEIGHT];
            var destructibles = new DestructibleTile[width * HEIGHT];

            // Fill ground
            for (int x = 0; x < width; x++)
            {
                for (int y = GROUND_Y; y < HEIGHT; y++)
                    SetSolid(tiles, collision, width, x, y);
            }

            // Destructible wall section: x=14-17, y=8-11 (soft blocks)
            for (int wx = 14; wx <= 17; wx++)
            {
                for (int wy = 8; wy <= GROUND_Y - 1; wy++)
                {
                    SetDestructible(tiles, collision, destructibles, width, wx, wy,
                        TileType.DestructibleSoft, MaterialClass.Soft, 1);
                }
            }

            // Destructible wall section 2: x=28-30, y=8-11 (medium blocks)
            for (int wx = 28; wx <= 30; wx++)
            {
                for (int wy = 8; wy <= GROUND_Y - 1; wy++)
                {
                    SetDestructible(tiles, collision, destructibles, width, wx, wy,
                        TileType.DestructibleMedium, MaterialClass.Medium, 3);
                }
            }

            // Stomp section: floating platforms at x=38-48
            // Remove ground in this area for pit
            for (int gx = 38; gx <= 48; gx++)
                ClearColumn(tiles, collision, width, gx);
            // Platforms over the pit
            SetPlatform(tiles, collision, width, 39, 10);
            SetPlatform(tiles, collision, width, 40, 10);
            SetPlatform(tiles, collision, width, 43, 10);
            SetPlatform(tiles, collision, width, 44, 10);
            SetPlatform(tiles, collision, width, 47, 10);
            SetPlatform(tiles, collision, width, 48, 10);
            // Restore ground at edges so player can exit pit area
            for (int y = GROUND_Y; y < HEIGHT; y++)
            {
                SetSolid(tiles, collision, width, 38, y);
                SetSolid(tiles, collision, width, 48, y);
            }

            // Another destructible section: x=56-60 (soft, to break through)
            for (int wx = 56; wx <= 60; wx++)
            {
                for (int wy = 9; wy <= GROUND_Y - 1; wy++)
                {
                    SetDestructible(tiles, collision, destructibles, width, wx, wy,
                        TileType.DestructibleSoft, MaterialClass.Soft, 1);
                }
            }

            int startX = 2;
            int startY = GROUND_Y - 1;
            int goalX = width - 4;
            int goalY = GROUND_Y - 1;

            // Enemies: 3 stationary turrets (easy targets)
            var enemies = new List<EnemyData>
            {
                new EnemyData { TileX = 22, TileY = GROUND_Y - 1, Type = EnemyType.Caveman,
                    Behavior = EnemyBehavior.Stationary, PatrolMinX = 22, PatrolMaxX = 22, DifficultyWeight = 0.1f },
                new EnemyData { TileX = 35, TileY = GROUND_Y - 1, Type = EnemyType.Caveman,
                    Behavior = EnemyBehavior.Stationary, PatrolMinX = 35, PatrolMaxX = 35, DifficultyWeight = 0.1f },
                new EnemyData { TileX = 65, TileY = GROUND_Y - 1, Type = EnemyType.Caveman,
                    Behavior = EnemyBehavior.Stationary, PatrolMinX = 65, PatrolMaxX = 65, DifficultyWeight = 0.1f },
            };

            // Weapon pickup: Medium tier Bolt at x=50
            var weaponDrops = new List<WeaponDropData>
            {
                new WeaponDropData { TileX = 50, TileY = GROUND_Y - 1, Tier = WeaponTier.Medium,
                    Type = WeaponType.Bolt, OnPrimaryPath = true }
            };

            // Rewards: health and coins
            var rewards = new List<RewardData>
            {
                new RewardData { TileX = 8, TileY = GROUND_Y - 2, Type = RewardType.Coin, Value = 25 },
                new RewardData { TileX = 20, TileY = GROUND_Y - 2, Type = RewardType.HealthSmall, Value = 50 },
                new RewardData { TileX = 43, TileY = 8, Type = RewardType.Coin, Value = 25 },
                new RewardData { TileX = 70, TileY = GROUND_Y - 2, Type = RewardType.Coin, Value = 25 },
            };

            var zones = new ZoneData[]
            {
                new ZoneData { Type = ZoneType.Intro, StartX = 0, EndX = 12,
                    DifficultyMin = 0f, DifficultyMax = 0f, DestructionDensity = 0f },
                new ZoneData { Type = ZoneType.Destruction, StartX = 13, EndX = 52,
                    DifficultyMin = 0f, DifficultyMax = 0.1f, DestructionDensity = 0.3f },
                new ZoneData { Type = ZoneType.Combat, StartX = 53, EndX = width - 1,
                    DifficultyMin = 0f, DifficultyMax = 0.1f, DestructionDensity = 0.2f },
            };

            var checkpoints = new List<CheckpointData>
            {
                new CheckpointData { TileX = startX, TileY = GROUND_Y, Type = CheckpointType.LevelStart },
                new CheckpointData { TileX = 36, TileY = GROUND_Y, Type = CheckpointType.MidLevel },
            };

            return new LevelData
            {
                ID = LevelID.Create(0, 2),
                Layout = new LevelLayout
                {
                    WidthTiles = width,
                    HeightTiles = HEIGHT,
                    StartX = startX,
                    StartY = startY,
                    GoalX = goalX,
                    GoalY = goalY,
                    Tiles = tiles,
                    Collision = collision,
                    Destructibles = destructibles,
                    Zones = zones,
                },
                Enemies = enemies,
                WeaponDrops = weaponDrops,
                Rewards = rewards,
                Checkpoints = checkpoints,
                Metadata = ComputeMetadata(tiles, collision, destructibles, width,
                    enemies, weaponDrops, rewards, checkpoints),
            };
        }

        /// <summary>
        /// Level 3: Climb & Survive — 96 tiles wide.
        /// Wall-jump section, Hard materials + Heavy weapon, mixed enemies, mini-boss.
        /// </summary>
        private static LevelData BuildLevel3_ClimbAndSurvive()
        {
            int width = 96;
            var tiles = new byte[width * HEIGHT];
            var collision = new byte[width * HEIGHT];
            var destructibles = new DestructibleTile[width * HEIGHT];

            // Fill ground
            for (int x = 0; x < width; x++)
            {
                for (int y = GROUND_Y; y < HEIGHT; y++)
                    SetSolid(tiles, collision, width, x, y);
            }

            // Wall-jump chimney: x=18-19 (left wall) and x=24-25 (right wall), y=2-11
            for (int wy = 2; wy <= GROUND_Y - 1; wy++)
            {
                SetSolid(tiles, collision, width, 18, wy);
                SetSolid(tiles, collision, width, 19, wy);
                SetSolid(tiles, collision, width, 24, wy);
                SetSolid(tiles, collision, width, 25, wy);
            }
            // Platform at top of chimney
            SetPlatform(tiles, collision, width, 20, 3);
            SetPlatform(tiles, collision, width, 21, 3);
            SetPlatform(tiles, collision, width, 22, 3);
            SetPlatform(tiles, collision, width, 23, 3);
            // Reward at top
            // Exit platform to the right
            SetPlatform(tiles, collision, width, 26, 5);
            SetPlatform(tiles, collision, width, 27, 5);
            SetPlatform(tiles, collision, width, 28, 5);

            // Hard destructible wall: x=36-39, y=6-11
            for (int wx = 36; wx <= 39; wx++)
            {
                for (int wy = 6; wy <= GROUND_Y - 1; wy++)
                {
                    SetDestructible(tiles, collision, destructibles, width, wx, wy,
                        TileType.DestructibleHard, MaterialClass.Hard, 5);
                }
            }

            // Combat arena: x=50-70, slightly open
            // Platforms for cover
            SetPlatform(tiles, collision, width, 54, 8);
            SetPlatform(tiles, collision, width, 55, 8);
            SetPlatform(tiles, collision, width, 60, 6);
            SetPlatform(tiles, collision, width, 61, 6);
            SetPlatform(tiles, collision, width, 66, 8);
            SetPlatform(tiles, collision, width, 67, 8);

            // Strategic floor: soft blocks over a gap at x=76-80
            for (int gx = 76; gx <= 80; gx++)
            {
                // Remove ground
                for (int y = GROUND_Y; y < HEIGHT; y++)
                {
                    tiles[y * width + gx] = (byte)TileType.Empty;
                    collision[y * width + gx] = (byte)CollisionType.None;
                }
                // Soft destructible bridge at ground level
                SetDestructible(tiles, collision, destructibles, width, gx, GROUND_Y,
                    TileType.DestructibleSoft, MaterialClass.Soft, 1);
            }

            int startX = 2;
            int startY = GROUND_Y - 1;
            int goalX = width - 4;
            int goalY = GROUND_Y - 1;

            // Enemies: 5 mixed types
            var enemies = new List<EnemyData>
            {
                // Patrol enemy before wall-jump
                new EnemyData { TileX = 12, TileY = GROUND_Y - 1, Type = EnemyType.Caveman,
                    Behavior = EnemyBehavior.Patrol, PatrolMinX = 8, PatrolMaxX = 16, DifficultyWeight = 0.2f },
                // Flying enemy in chimney
                new EnemyData { TileX = 21, TileY = 6, Type = EnemyType.Beast,
                    Behavior = EnemyBehavior.Flying, PatrolMinX = 20, PatrolMaxX = 23, DifficultyWeight = 0.3f },
                // Stationary turret in combat arena
                new EnemyData { TileX = 56, TileY = GROUND_Y - 1, Type = EnemyType.Rockslinger,
                    Behavior = EnemyBehavior.Stationary, PatrolMinX = 56, PatrolMaxX = 56, DifficultyWeight = 0.3f },
                // Chase enemy in arena
                new EnemyData { TileX = 62, TileY = GROUND_Y - 1, Type = EnemyType.Beast,
                    Behavior = EnemyBehavior.Chase, PatrolMinX = 50, PatrolMaxX = 70, DifficultyWeight = 0.4f },
                // Mini-boss (tough enemy, not actual Boss class)
                new EnemyData { TileX = 68, TileY = GROUND_Y - 1, Type = EnemyType.Rockslinger,
                    Behavior = EnemyBehavior.Chase, PatrolMinX = 60, PatrolMaxX = 75, DifficultyWeight = 0.6f },
            };

            // Weapon drops: Heavy Bolt near hard wall, Spreader for combat
            var weaponDrops = new List<WeaponDropData>
            {
                new WeaponDropData { TileX = 32, TileY = GROUND_Y - 1, Tier = WeaponTier.Heavy,
                    Type = WeaponType.Bolt, OnPrimaryPath = true },
                new WeaponDropData { TileX = 48, TileY = GROUND_Y - 1, Tier = WeaponTier.Starting,
                    Type = WeaponType.Spreader, OnPrimaryPath = true },
            };

            // Rewards
            var rewards = new List<RewardData>
            {
                new RewardData { TileX = 21, TileY = 2, Type = RewardType.AttackBoost, Value = 150 },
                new RewardData { TileX = 45, TileY = GROUND_Y - 2, Type = RewardType.HealthSmall, Value = 50 },
                new RewardData { TileX = 58, TileY = GROUND_Y - 2, Type = RewardType.HealthLarge, Value = 100 },
                new RewardData { TileX = 85, TileY = GROUND_Y - 2, Type = RewardType.Coin, Value = 25 },
            };

            var zones = new ZoneData[]
            {
                new ZoneData { Type = ZoneType.Intro, StartX = 0, EndX = 15,
                    DifficultyMin = 0f, DifficultyMax = 0.1f, DestructionDensity = 0f },
                new ZoneData { Type = ZoneType.Traversal, StartX = 16, EndX = 30,
                    DifficultyMin = 0.1f, DifficultyMax = 0.2f, DestructionDensity = 0f },
                new ZoneData { Type = ZoneType.Destruction, StartX = 31, EndX = 48,
                    DifficultyMin = 0.1f, DifficultyMax = 0.2f, DestructionDensity = 0.4f },
                new ZoneData { Type = ZoneType.Combat, StartX = 49, EndX = width - 1,
                    DifficultyMin = 0.2f, DifficultyMax = 0.3f, DestructionDensity = 0.1f },
            };

            var checkpoints = new List<CheckpointData>
            {
                new CheckpointData { TileX = startX, TileY = GROUND_Y, Type = CheckpointType.LevelStart },
                new CheckpointData { TileX = 30, TileY = GROUND_Y, Type = CheckpointType.MidLevel },
                new CheckpointData { TileX = 48, TileY = GROUND_Y, Type = CheckpointType.PreBoss },
            };

            return new LevelData
            {
                ID = LevelID.Create(0, 3),
                Layout = new LevelLayout
                {
                    WidthTiles = width,
                    HeightTiles = HEIGHT,
                    StartX = startX,
                    StartY = startY,
                    GoalX = goalX,
                    GoalY = goalY,
                    Tiles = tiles,
                    Collision = collision,
                    Destructibles = destructibles,
                    Zones = zones,
                },
                Enemies = enemies,
                WeaponDrops = weaponDrops,
                Rewards = rewards,
                Checkpoints = checkpoints,
                Metadata = ComputeMetadata(tiles, collision, destructibles, width,
                    enemies, weaponDrops, rewards, checkpoints),
            };
        }

        // =====================================================================
        // Tile helpers
        // =====================================================================

        private static void SetSolid(byte[] tiles, byte[] collision, int width, int x, int y)
        {
            int idx = y * width + x;
            tiles[idx] = (byte)TileType.Ground;
            collision[idx] = (byte)CollisionType.Solid;
        }

        private static void SetPlatform(byte[] tiles, byte[] collision, int width, int x, int y)
        {
            int idx = y * width + x;
            tiles[idx] = (byte)TileType.Platform;
            collision[idx] = (byte)CollisionType.PlatformPassthrough;
        }

        private static void SetDestructible(byte[] tiles, byte[] collision, DestructibleTile[] destructibles,
            int width, int x, int y, TileType tileType, MaterialClass material, byte hp)
        {
            int idx = y * width + x;
            tiles[idx] = (byte)tileType;
            collision[idx] = (byte)CollisionType.Destructible;
            destructibles[idx] = new DestructibleTile
            {
                MaterialClass = (byte)material,
                HitPoints = hp,
                MaxHitPoints = hp,
            };
        }

        private static void ClearColumn(byte[] tiles, byte[] collision, int width, int x)
        {
            for (int y = GROUND_Y; y < HEIGHT; y++)
            {
                tiles[y * width + x] = (byte)TileType.Empty;
                collision[y * width + x] = (byte)CollisionType.None;
            }
        }

        private static LevelMetadata ComputeMetadata(byte[] tiles, byte[] collision,
            DestructibleTile[] destructibles, int width,
            List<EnemyData> enemies, List<WeaponDropData> weapons,
            List<RewardData> rewards, List<CheckpointData> checkpoints)
        {
            int totalDestructible = 0;
            int totalRelics = 0;
            int totalHazards = 0;

            for (int i = 0; i < destructibles.Length; i++)
            {
                if (destructibles[i].MaterialClass > 0)
                    totalDestructible++;
                if (destructibles[i].IsRelic)
                    totalRelics++;
                if (destructibles[i].Hazard != HazardType.None)
                    totalHazards++;
            }

            return new LevelMetadata
            {
                GenerationTimeMs = 0f,
                CalculatedDifficulty = 0f,
                DestructionRatioTarget = 0f,
                TotalEnemies = enemies.Count,
                TotalWeaponDrops = weapons.Count,
                TotalRewards = rewards.Count,
                TotalCheckpoints = checkpoints.Count,
                TotalDestructibleTiles = totalDestructible,
                TotalRelics = totalRelics,
                TotalHazards = totalHazards,
            };
        }
    }
}
