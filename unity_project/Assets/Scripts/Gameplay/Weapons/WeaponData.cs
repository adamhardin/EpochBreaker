using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Stats for a weapon type + tier combination.
    /// Looked up via WeaponDatabase.GetStats().
    /// </summary>
    public struct WeaponStats
    {
        public float FireRate;
        public float DamageMultiplier;
        public float Range;
        public float ProjectileSpeed;
        public int ProjectileCount;
        public float SpreadAngle;
        public int PierceCount;
        public int ChainCount;
        public float ChainRange;
        public float SlowDuration;
        public float SlowFactor;
        public float HeatPerShot;
        public bool BreaksAllMaterials;
    }

    /// <summary>
    /// Inventory slot for one weapon type.
    /// </summary>
    public struct WeaponSlotData
    {
        public WeaponType Type;
        public WeaponTier Tier;
        public bool Acquired;
    }

    /// <summary>
    /// Static database of weapon stats per type and tier.
    /// All tuning values are centralized here for easy adjustment.
    /// </summary>
    public static class WeaponDatabase
    {
        public static WeaponStats GetStats(WeaponType type, WeaponTier tier)
        {
            int t = (int)tier;

            return type switch
            {
                WeaponType.Bolt => new WeaponStats
                {
                    FireRate = 0.25f - t * 0.05f,
                    DamageMultiplier = 1f + t,
                    Range = 12f,
                    ProjectileSpeed = 24f,
                    ProjectileCount = 1,
                },
                WeaponType.Piercer => new WeaponStats
                {
                    FireRate = 0.30f - t * 0.04f,
                    DamageMultiplier = (1f + t) * 0.7f,
                    Range = 14f,
                    ProjectileSpeed = 28f,
                    ProjectileCount = 1,
                    PierceCount = 2 + t,
                },
                WeaponType.Spreader => new WeaponStats
                {
                    FireRate = 0.35f - t * 0.05f,
                    DamageMultiplier = (1f + t) * 0.5f,
                    Range = 8f,
                    ProjectileSpeed = 20f,
                    ProjectileCount = 3 + t, // 3 → 4 → 5 per tier
                    SpreadAngle = 15f,
                },
                WeaponType.Chainer => new WeaponStats
                {
                    FireRate = 0.40f - t * 0.05f,
                    DamageMultiplier = (1f + t) * 0.6f,
                    Range = 10f,
                    ProjectileSpeed = 22f,
                    ProjectileCount = 1,
                    ChainCount = 2 + t, // 2 → 3 → 4 per tier
                    ChainRange = 4f + t, // 4 → 5 → 6 per tier
                },
                WeaponType.Slower => new WeaponStats
                {
                    FireRate = 0.50f - t * 0.05f,
                    DamageMultiplier = (1f + t) * 0.5f,
                    Range = 10f,
                    ProjectileSpeed = 18f,
                    ProjectileCount = 1,
                    SlowDuration = 3f + t, // 3s → 4s → 5s per tier
                    SlowFactor = 0.5f - t * 0.1f, // 50% → 60% → 70% slow
                },
                WeaponType.Cannon => new WeaponStats
                {
                    FireRate = 0.60f - t * 0.05f,
                    DamageMultiplier = (1f + t) * 3f,
                    Range = 12f,
                    ProjectileSpeed = 16f,
                    ProjectileCount = 1,
                    HeatPerShot = 10f,
                    BreaksAllMaterials = true,
                },
                _ => new WeaponStats
                {
                    FireRate = 0.25f,
                    DamageMultiplier = 1f,
                    Range = 12f,
                    ProjectileSpeed = 24f,
                    ProjectileCount = 1,
                }
            };
        }

        /// <summary>
        /// Base score value when picking up a weapon type.
        /// </summary>
        public static int GetPickupScoreValue(WeaponType type, WeaponTier tier)
        {
            int tierMult = 1 + (int)tier;
            return type switch
            {
                WeaponType.Bolt => 50 * tierMult,
                WeaponType.Piercer => 100 * tierMult,
                WeaponType.Spreader => 100 * tierMult,
                WeaponType.Chainer => 150 * tierMult,
                WeaponType.Slower => 150 * tierMult,
                WeaponType.Cannon => 200 * tierMult,
                _ => 50 * tierMult
            };
        }
    }

    /// <summary>
    /// Heat management system for the Cannon weapon.
    /// Overheating locks out firing for a cooldown period.
    /// </summary>
    public class HeatSystem
    {
        public float CurrentHeat { get; private set; }
        public float MaxHeat => 40f;
        public bool IsOverheated { get; private set; }

        private const float HEAT_PER_SHOT = 10f;
        private const float COOL_RATE = 8f;
        private const float OVERHEAT_LOCKOUT = 2f;
        private float _overheatTimer;

        public bool CanFire => !IsOverheated;

        public float HeatRatio => CurrentHeat / MaxHeat;

        public void AddHeat()
        {
            CurrentHeat += HEAT_PER_SHOT;
            if (CurrentHeat >= MaxHeat)
            {
                CurrentHeat = MaxHeat;
                IsOverheated = true;
                _overheatTimer = OVERHEAT_LOCKOUT;
                AchievementManager.Instance?.RecordCannonOverheat();
            }
        }

        public void Update(float dt)
        {
            if (IsOverheated)
            {
                _overheatTimer -= dt;
                if (_overheatTimer <= 0f)
                {
                    IsOverheated = false;
                    CurrentHeat = 0f;
                }
            }
            else if (CurrentHeat > 0f)
            {
                CurrentHeat = UnityEngine.Mathf.Max(0f, CurrentHeat - COOL_RATE * dt);
            }
        }

        public void Reset()
        {
            CurrentHeat = 0f;
            IsOverheated = false;
            _overheatTimer = 0f;
        }
    }
}
