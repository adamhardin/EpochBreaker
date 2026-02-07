using UnityEngine;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Manages the player's 6-slot weapon inventory. Auto-fires continuously.
    /// Limited auto-select: Bolt default, Piercer in corridors, Slower on boss.
    /// Manual cycling via Attack button with Quick Draw fire rate buff.
    /// </summary>
    public class WeaponSystem : MonoBehaviour
    {
        public const int SLOT_COUNT = 6;
        private const float QUICK_DRAW_DURATION = 1.5f;
        private const float QUICK_DRAW_RATE_MULT = 0.75f; // 25% faster fire rate
        private const float CORRIDOR_VERTICAL_SPREAD = 2f; // Max Y spread for "in a line"
        private const int CORRIDOR_MIN_ENEMIES = 2;
        private const float TARGET_DETECTION_RANGE = 15f; // Weapon-independent scan range

        private WeaponSlotData[] _slots = new WeaponSlotData[SLOT_COUNT];
        private int _activeSlot;
        private bool _manualOverride;
        private float _quickDrawTimer;

        private float _fireTimer;
        private Transform _currentTarget;
        private PlayerController _player;
        private HeatSystem _heatSystem = new HeatSystem();
        private int _nearbyEnemyCount;
        private bool _bossInRange;
        private bool _enemiesInCorridor; // 2+ enemies roughly in a horizontal line

        // Backward-compatible properties
        public WeaponTier CurrentTier => _slots[_activeSlot].Tier;
        public WeaponType ActiveWeaponType => _slots[_activeSlot].Type;
        public WeaponTier ActiveWeaponTier => _slots[_activeSlot].Tier;
        public WeaponSlotData[] Slots => _slots;
        public HeatSystem Heat => _heatSystem;
        public bool IsQuickDrawActive => _quickDrawTimer > 0f;
        public string LastAutoSelectReason { get; private set; } = "";
        public float AutoSelectReasonTimer { get; private set; }

        private void Awake()
        {
            _player = GetComponent<PlayerController>();

            // Initialize all slots
            for (int i = 0; i < SLOT_COUNT; i++)
            {
                _slots[i] = new WeaponSlotData
                {
                    Type = (WeaponType)i,
                    Tier = WeaponTier.Starting,
                    Acquired = false
                };
            }

            // Bolt is always acquired from the start
            _slots[0].Acquired = true;
            _activeSlot = 0;
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;
            if (_player != null && !_player.IsAlive) return;

            _heatSystem.Update(Time.deltaTime);

            // Quick Draw timer countdown
            if (_quickDrawTimer > 0f)
                _quickDrawTimer -= Time.deltaTime;

            // Manual weapon cycle via Attack button
            HandleAttackInput();

            _fireTimer -= Time.deltaTime;
            FindTarget();

            // Auto-select reason timer
            if (AutoSelectReasonTimer > 0f)
                AutoSelectReasonTimer -= Time.deltaTime;

            // Limited auto-select when not in manual override
            // Only picks Bolt, Piercer (corridor), or Slower (boss)
            if (!_manualOverride)
            {
                int prevSlot = _activeSlot;
                int bestSlot = (int)SelectBestWeapon();
                if (_slots[bestSlot].Acquired)
                    _activeSlot = bestSlot;

                // Track reason when auto-select changes weapon
                if (_activeSlot != prevSlot)
                {
                    if (_bossInRange && _activeSlot == (int)WeaponType.Slower)
                        LastAutoSelectReason = "SLOWER (boss)";
                    else if (_enemiesInCorridor && _activeSlot == (int)WeaponType.Piercer)
                        LastAutoSelectReason = "PIERCER (corridor)";
                    else
                        LastAutoSelectReason = "";
                    AutoSelectReasonTimer = 2f;
                }
            }
            else
            {
                // Overheat safety: auto-switch to Bolt if Cannon overheats
                if (ActiveWeaponType == WeaponType.Cannon && _heatSystem.IsOverheated)
                {
                    _activeSlot = (int)WeaponType.Bolt;
                    _manualOverride = false;
                }
            }

            // Get stats for active weapon
            var stats = WeaponDatabase.GetStats(ActiveWeaponType, ActiveWeaponTier);

            if (_fireTimer <= 0f)
            {
                // Check heat for Cannon
                if (ActiveWeaponType == WeaponType.Cannon && !_heatSystem.CanFire)
                {
                    _fireTimer = 0.1f; // Small delay before retrying
                    return;
                }

                Fire(stats);

                // Apply Quick Draw fire rate bonus
                float fireRate = stats.FireRate;
                if (_quickDrawTimer > 0f)
                    fireRate *= QUICK_DRAW_RATE_MULT;

                _fireTimer = fireRate;
            }
        }

        private void HandleAttackInput()
        {
            if (InputManager.AttackPressed)
            {
                int prevSlot = _activeSlot;
                CycleToNextWeapon();
                _manualOverride = true;
                // Only grant Quick Draw if weapon actually changed
                if (_activeSlot != prevSlot)
                    _quickDrawTimer = QUICK_DRAW_DURATION;
            }
        }

        /// <summary>
        /// Acquire a new weapon type at a given tier.
        /// If already acquired at a lower tier, upgrades in place.
        /// </summary>
        public void AcquireWeapon(WeaponType type, WeaponTier tier)
        {
            int slot = (int)type;
            if (slot < 0 || slot >= SLOT_COUNT) return;

            if (!_slots[slot].Acquired || tier > _slots[slot].Tier)
            {
                _slots[slot].Acquired = true;
                _slots[slot].Tier = tier;
            }
        }

        /// <summary>
        /// Legacy upgrade method for backward compatibility.
        /// Upgrades the Bolt weapon tier.
        /// </summary>
        public void UpgradeWeapon(WeaponTier tier)
        {
            AcquireWeapon(WeaponType.Bolt, tier);
        }

        public bool HasWeapon(WeaponType type) => _slots[(int)type].Acquired;

        private void CycleToNextWeapon()
        {
            int startSlot = _activeSlot;
            for (int i = 1; i <= SLOT_COUNT; i++)
            {
                int slot = (startSlot + i) % SLOT_COUNT;
                if (_slots[slot].Acquired)
                {
                    _activeSlot = slot;
                    return;
                }
            }
        }

        /// <summary>
        /// Limited auto-select: only picks from 3 options.
        /// Bolt (default), Piercer (corridor with 2+ aligned enemies), Slower (boss active).
        /// Chainer, Spreader, Cannon are NEVER auto-selected — manual cycling only.
        /// </summary>
        private WeaponType SelectBestWeapon()
        {
            if (_bossInRange && HasWeapon(WeaponType.Slower))
                return WeaponType.Slower;
            if (_enemiesInCorridor && HasWeapon(WeaponType.Piercer))
                return WeaponType.Piercer;
            return WeaponType.Bolt;
        }

        private void FindTarget()
        {
            _currentTarget = null;
            _nearbyEnemyCount = 0;
            _bossInRange = false;
            _enemiesInCorridor = false;
            float range = TARGET_DETECTION_RANGE;
            float closestDist = range;

            float minEnemyY = float.MaxValue;
            float maxEnemyY = float.MinValue;
            int forwardEnemyCount = 0;
            bool facingRight = _player != null ? _player.FacingRight : true;

            var enemies = EnemyBase.ActiveEnemies;
            for (int ei = 0; ei < enemies.Count; ei++)
            {
                var enemy = enemies[ei];
                if (enemy == null) continue;
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < range)
                {
                    _nearbyEnemyCount++;
                    if (enemy.GetComponent<Boss>() != null)
                        _bossInRange = true;

                    // Track enemies ahead of player for corridor detection
                    float dx = enemy.transform.position.x - transform.position.x;
                    bool isAhead = facingRight ? dx > 0 : dx < 0;
                    if (isAhead)
                    {
                        forwardEnemyCount++;
                        float ey = enemy.transform.position.y;
                        if (ey < minEnemyY) minEnemyY = ey;
                        if (ey > maxEnemyY) maxEnemyY = ey;
                    }
                }
                if (dist < closestDist)
                {
                    closestDist = dist;
                    _currentTarget = enemy.transform;
                }
            }

            // Corridor detection: 2+ enemies ahead with small vertical spread
            _enemiesInCorridor = forwardEnemyCount >= CORRIDOR_MIN_ENEMIES
                && (maxEnemyY - minEnemyY) <= CORRIDOR_VERTICAL_SPREAD;
        }

        private void Fire(WeaponStats stats)
        {
            Vector2 baseDir;

            if (_currentTarget != null)
            {
                baseDir = ((Vector2)_currentTarget.position - (Vector2)transform.position).normalized;
            }
            else
            {
                bool facingRight = _player != null ? _player.FacingRight : true;
                baseDir = facingRight ? Vector2.right : Vector2.left;
            }

            int damage = Mathf.Max(1, Mathf.RoundToInt(stats.DamageMultiplier));
            int epoch = GameManager.Instance?.CurrentEpoch ?? 0;

            // Spreader fires multiple projectiles in a cone
            if (stats.ProjectileCount > 1 && stats.SpreadAngle > 0)
            {
                float totalAngle = stats.SpreadAngle * 2f;
                float step = totalAngle / (stats.ProjectileCount - 1);
                float startAngle = -stats.SpreadAngle;

                for (int i = 0; i < stats.ProjectileCount; i++)
                {
                    float angle = startAngle + step * i;
                    Vector2 dir = RotateVector(baseDir, angle);
                    SpawnProjectile(dir, stats, damage, epoch);
                }
            }
            else
            {
                SpawnProjectile(baseDir, stats, damage, epoch);
            }

            // Apply heat for Cannon
            if (ActiveWeaponType == WeaponType.Cannon)
                _heatSystem.AddHeat();

            // Muzzle flash
            SpawnMuzzleFlash(baseDir);

            GameManager.Instance?.RecordShotFired();
            AudioManager.PlayWeaponSFX(PlaceholderAudio.GetWeaponFireSFX(ActiveWeaponType), 0.1f);
        }

        private void SpawnMuzzleFlash(Vector2 dir)
        {
            Vector3 flashPos = transform.position + (Vector3)(dir * 0.7f);
            flashPos.y += 0.75f;

            var go = ObjectPool.GetFlash();
            go.transform.position = flashPos;

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = PlaceholderAssets.GetParticleSprite();
            sr.color = GetWeaponColor(ActiveWeaponType) * 2f; // Bright flash
            sr.sortingOrder = 12;
            go.transform.localScale = Vector3.one * 0.5f;

            go.GetComponent<PoolTimer>().StartTimer(0.06f);
        }

        private void SpawnProjectile(Vector2 dir, WeaponStats stats, int damage, int epoch)
        {
            Vector3 spawnPos = transform.position + (Vector3)(dir * 0.6f);
            spawnPos.y += 0.75f;

            var projGO = ObjectPool.GetProjectile();
            projGO.transform.position = spawnPos;

            var sr = projGO.GetComponent<SpriteRenderer>();
            sr.sprite = PlaceholderAssets.GetProjectileSprite(ActiveWeaponTier, epoch);
            sr.sortingOrder = 11;

            // Scale projectile based on tier
            float projScale = ActiveWeaponTier switch
            {
                WeaponTier.Heavy => 1.5f,
                WeaponTier.Medium => 1.25f,
                _ => 1.0f
            };
            projGO.transform.localScale = new Vector3(projScale, projScale, 1f);

            // Color tint per weapon type
            sr.color = GetWeaponColor(ActiveWeaponType);

            var col = projGO.GetComponent<CircleCollider2D>();
            col.radius = 0.15f;

            var proj = projGO.GetComponent<Projectile>();
            proj.Initialize(dir, stats.ProjectileSpeed, damage, false);
            proj.WeaponTier = ActiveWeaponTier;
            proj.PierceRemaining = stats.PierceCount;
            proj.ChainRemaining = stats.ChainCount;
            proj.ChainRange = stats.ChainRange;
            proj.SlowDuration = stats.SlowDuration;
            proj.SlowFactor = stats.SlowFactor;
            proj.BreaksAllMaterials = stats.BreaksAllMaterials;
        }

        private static Color GetWeaponColor(WeaponType type)
        {
            return type switch
            {
                WeaponType.Bolt => Color.white,
                WeaponType.Piercer => new Color(0.6f, 0.9f, 1f),     // Light blue
                WeaponType.Spreader => new Color(1f, 0.7f, 0.3f),    // Orange
                WeaponType.Chainer => new Color(0.5f, 0.8f, 1f),     // Electric blue
                WeaponType.Slower => new Color(0.6f, 0.4f, 1f),      // Purple
                WeaponType.Cannon => new Color(1f, 0.3f, 0.2f),      // Red
                _ => Color.white
            };
        }

        private static Vector2 RotateVector(Vector2 v, float angleDeg)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }
}
