using System;
using System.Collections.Generic;

namespace EpochBreaker.Generative
{
    /// <summary>
    /// Complete data representation of a generated level.
    /// This is the output of the generation pipeline and the input to the renderer.
    /// All fields are deterministic given the same LevelID.
    /// </summary>
    public sealed class LevelData
    {
        public LevelID ID;
        public LevelMetadata Metadata;
        public LevelLayout Layout;
        public List<EnemyData> Enemies;
        public List<WeaponDropData> WeaponDrops;
        public List<RewardData> Rewards;
        public List<CheckpointData> Checkpoints;

        public ulong ComputeHash()
        {
            ulong hash = 14695981039346656037UL;

            for (int i = 0; i < Layout.Tiles.Length; i++)
            {
                hash ^= Layout.Tiles[i];
                hash *= 1099511628211UL;
            }

            for (int i = 0; i < Layout.Collision.Length; i++)
            {
                hash ^= Layout.Collision[i];
                hash *= 1099511628211UL;
            }

            if (Layout.Destructibles != null)
            {
                for (int i = 0; i < Layout.Destructibles.Length; i++)
                {
                    var d = Layout.Destructibles[i];
                    hash ^= (ulong)d.MaterialClass;
                    hash *= 1099511628211UL;
                    hash ^= (ulong)d.HitPoints;
                    hash *= 1099511628211UL;
                    hash ^= (ulong)d.HiddenContent;
                    hash *= 1099511628211UL;
                    hash ^= d.IsLoadBearing ? 1UL : 0UL;
                    hash *= 1099511628211UL;
                    hash ^= (ulong)d.StructuralGroupId;
                    hash *= 1099511628211UL;
                }
            }

            for (int i = 0; i < Enemies.Count; i++)
            {
                var e = Enemies[i];
                hash ^= (ulong)e.TileX;
                hash *= 1099511628211UL;
                hash ^= (ulong)e.TileY;
                hash *= 1099511628211UL;
                hash ^= (ulong)e.Type;
                hash *= 1099511628211UL;
            }

            for (int i = 0; i < WeaponDrops.Count; i++)
            {
                var w = WeaponDrops[i];
                hash ^= (ulong)w.TileX;
                hash *= 1099511628211UL;
                hash ^= (ulong)w.TileY;
                hash *= 1099511628211UL;
                hash ^= (ulong)w.Tier;
                hash *= 1099511628211UL;
            }

            for (int i = 0; i < Rewards.Count; i++)
            {
                var r = Rewards[i];
                hash ^= (ulong)r.TileX;
                hash *= 1099511628211UL;
                hash ^= (ulong)r.TileY;
                hash *= 1099511628211UL;
                hash ^= (ulong)r.Type;
                hash *= 1099511628211UL;
            }

            return hash;
        }
    }

    public struct LevelMetadata
    {
        public float GenerationTimeMs;
        public float CalculatedDifficulty;
        public float DestructionRatioTarget;
        public int TotalEnemies;
        public int TotalWeaponDrops;
        public int TotalRewards;
        public int TotalCheckpoints;
        public int TotalDestructibleTiles;
    }

    public struct LevelLayout
    {
        public int WidthTiles;
        public int HeightTiles;
        public int StartX;
        public int StartY;
        public int GoalX;
        public int GoalY;

        /// <summary>Flat array of tile IDs, row-major: Tiles[y * WidthTiles + x]</summary>
        public byte[] Tiles;

        /// <summary>Collision flags, same indexing as Tiles.</summary>
        public byte[] Collision;

        /// <summary>Per-tile destructible data, same indexing as Tiles.</summary>
        public DestructibleTile[] Destructibles;

        /// <summary>Zone definitions for pacing and difficulty progression.</summary>
        public ZoneData[] Zones;
    }

    // =====================================================================
    // Destructible Environment System
    // =====================================================================

    public struct DestructibleTile
    {
        /// <summary>1=Soft, 2=Medium, 3=Hard, 4=Reinforced, 0=None/Empty</summary>
        public byte MaterialClass;
        public byte HitPoints;
        public byte MaxHitPoints;
        public bool IsLoadBearing;
        public ushort StructuralGroupId;
        public HiddenContentType HiddenContent;
    }

    public enum MaterialClass : byte
    {
        None = 0,
        Soft = 1,
        Medium = 2,
        Hard = 3,
        Reinforced = 4,
        Indestructible = 5
    }

    public enum HiddenContentType : byte
    {
        None = 0,
        Pathway = 1,
        Bridge = 2,
        Stairs = 3,
        Secret = 4
    }

    // =====================================================================
    // Weapon System
    // =====================================================================

    public enum WeaponTier : byte
    {
        Starting = 0,   // breaks Soft(1 hit), Medium(3 hits)
        Medium = 1,     // breaks Soft(1), Medium(1), Hard(3)
        Heavy = 2       // breaks Soft(1), Medium(1), Hard(1), Reinforced(3)
    }

    public struct WeaponDropData
    {
        public int TileX;
        public int TileY;
        public WeaponTier Tier;
        public bool OnPrimaryPath;
        public bool Hidden;
    }

    // =====================================================================
    // Zone System
    // =====================================================================

    public struct ZoneData
    {
        public ZoneType Type;
        public int StartX;
        public int EndX;
        public float DifficultyMin;
        public float DifficultyMax;
        public float DestructionDensity;
    }

    public enum ZoneType : byte
    {
        Intro = 0,
        Traversal = 1,
        Destruction = 2,
        Combat = 3,
        BossArena = 4,
        Buffer = 5
    }

    // =====================================================================
    // Tile and Collision Types
    // =====================================================================

    public enum TileType : byte
    {
        Empty = 0,
        Ground = 1,
        Platform = 2,
        GroundLeft = 3,
        GroundRight = 4,
        GroundTop = 5,
        Hazard = 6,
        Decorative = 7,
        DestructibleSoft = 8,
        DestructibleMedium = 9,
        DestructibleHard = 10,
        DestructibleReinforced = 11,
        Indestructible = 12
    }

    public enum CollisionType : byte
    {
        None = 0,
        Solid = 1,
        PlatformPassthrough = 2,
        Hazard = 3,
        Destructible = 4
    }

    // =====================================================================
    // Enemy System (Era-Based)
    // =====================================================================

    public struct EnemyData
    {
        public int TileX;
        public int TileY;
        public EnemyType Type;
        public EnemyBehavior Behavior;
        public int PatrolMinX;
        public int PatrolMaxX;
        public float DifficultyWeight;
        public bool UsesCover;
        public bool IsAmbush;
    }

    /// <summary>
    /// Era-specific enemy types. Each era has 3 types (easy, medium, hard).
    /// Encoded as era * 3 + difficulty_tier.
    /// </summary>
    public enum EnemyType : byte
    {
        // Era 0: Stone Age
        Caveman = 0, Beast = 1, Rockslinger = 2,
        // Era 1: Bronze Age
        Spearman = 3, ChariotArcher = 4, BullWarrior = 5,
        // Era 2: Classical
        Legionary = 6, Phalanx = 7, WarElephant = 8,
        // Era 3: Medieval
        Knight = 9, Crossbowman = 10, SiegeEngine = 11,
        // Era 4: Renaissance
        Musketeer = 12, Pikeman = 13, CannonCrew = 14,
        // Era 5: Industrial
        Rifleman = 15, Automaton = 16, IroncladTurret = 17,
        // Era 6: Modern
        Soldier = 18, Drone = 19, TankTurret = 20,
        // Era 7: Digital
        FirewallBot = 21, DataWorm = 22, LogicBomb = 23,
        // Era 8: Spacefaring
        AstroTrooper = 24, PlasmaSentry = 25, VoidBeast = 26,
        // Era 9: Transcendent
        EchoPhantom = 27, ChronoWeaver = 28, RealityShard = 29,
    }

    public enum EnemyBehavior : byte
    {
        Patrol = 0,
        Chase = 1,
        Stationary = 2,
        Flying = 3
    }

    public enum BossType : byte
    {
        MammothChief = 0, BronzeColossus = 1, WarTitan = 2,
        SiegeLord = 3, GunpowderKing = 4, IronLeviathan = 5,
        SteelJuggernaut = 6, CoreSentinel = 7, VoidDreadnought = 8,
        RealityBreaker = 9,
    }

    // =====================================================================
    // Rewards and Checkpoints
    // =====================================================================

    public struct RewardData
    {
        public int TileX;
        public int TileY;
        public RewardType Type;
        public int Value;
        public bool Hidden;
    }

    public enum RewardType : byte
    {
        HealthSmall = 0, HealthLarge = 1, AttackBoost = 2,
        SpeedBoost = 3, Shield = 4, Coin = 5
    }

    public struct CheckpointData
    {
        public int TileX;
        public int TileY;
        public CheckpointType Type;
    }

    public enum CheckpointType : byte
    {
        LevelStart = 0, MidLevel = 1, PreBoss = 2, BossArena = 3
    }
}
