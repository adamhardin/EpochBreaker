using UnityEngine;
using System.Collections.Generic;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Projectile movement and collision. Used by both player weapons
    /// and enemy shooters. Supports pierce, chain, slow, and
    /// breaks-all-materials behaviors for the 6-slot weapon system.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        public WeaponTier WeaponTier { get; set; } = WeaponTier.Starting;

        // Extended weapon behaviors
        public int PierceRemaining { get; set; }
        public int ChainRemaining { get; set; }
        public float ChainRange { get; set; }
        public float SlowDuration { get; set; }
        public float SlowFactor { get; set; } = 1f;
        public bool BreaksAllMaterials { get; set; }
        public bool IsSentinelMissile { get; private set; }

        // Static cached refs shared by all projectiles (cleared on level load)
        private static LevelRenderer s_levelRenderer;
        private static HazardSystem s_hazardSystem;

        private Vector2 _direction;
        private float _speed;
        private int _damage;
        private bool _isEnemyProjectile;
        private float _lifetime = 3f;
        private HashSet<int> _hitEnemies;

        // Homing missile fields
        private bool _isHoming;
        private Transform _homingTarget;
        private float _homingTurnSpeed;

        public static void ClearCachedRefs()
        {
            s_levelRenderer = null;
            s_hazardSystem = null;
        }

        public void Initialize(Vector2 direction, float speed, int damage, bool isEnemyProjectile)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _isEnemyProjectile = isEnemyProjectile;
            _lifetime = 3f;
            _hitEnemies?.Clear();
            PierceRemaining = 0;
            ChainRemaining = 0;
            ChainRange = 0f;
            SlowDuration = 0f;
            SlowFactor = 1f;
            BreaksAllMaterials = false;
            WeaponTier = WeaponTier.Starting;
            _isHoming = false;
            _homingTarget = null;
            _homingTurnSpeed = 0f;
            IsSentinelMissile = false;
        }

        public void InitializeHoming(Transform target, float speed, int damage, float turnSpeed)
        {
            Vector2 dir = target != null
                ? ((Vector2)target.position - (Vector2)transform.position).normalized
                : Vector2.up;
            Initialize(dir, speed, damage, false);
            _isHoming = true;
            _homingTarget = target;
            _homingTurnSpeed = turnSpeed;
            IsSentinelMissile = true;
        }

        private void Update()
        {
            if (_isHoming)
            {
                // Re-acquire target if current died
                if (_homingTarget == null || !_homingTarget.gameObject.activeInHierarchy)
                    _homingTarget = FindNearestEnemy(transform.position);

                if (_homingTarget != null)
                {
                    Vector2 toTarget = ((Vector2)_homingTarget.position - (Vector2)transform.position).normalized;
                    _direction = Vector2.Lerp(_direction, toTarget, _homingTurnSpeed * Time.deltaTime).normalized;
                }
            }

            transform.position += (Vector3)(_direction * _speed * Time.deltaTime);

            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0f)
                ObjectPool.Return(gameObject);
        }

        private static Transform FindNearestEnemy(Vector3 fromPos)
        {
            float closest = float.MaxValue;
            Transform best = null;
            var enemies = EnemyBase.ActiveEnemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] == null) continue;
                var eb = enemies[i].GetComponent<EnemyBase>();
                if (eb != null && eb.IsDead) continue;
                float dist = Vector2.Distance(fromPos, enemies[i].transform.position);
                if (dist < closest) { closest = dist; best = enemies[i].transform; }
            }
            return best;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isEnemyProjectile)
            {
                // Enemy projectile hits player
                if (other.CompareTag("Player"))
                {
                    var health = other.GetComponent<HealthSystem>();
                    if (health != null)
                        health.TakeDamage(_damage, transform.position);
                    ObjectPool.Return(gameObject);
                }
            }
            else
            {
                // Player projectile — ignore the player's own collider
                if (other.CompareTag("Player"))
                    return;

                // Player projectile hits arena pillar
                var pillar = other.GetComponent<ArenaPillar>();
                if (pillar != null)
                {
                    // Pass through already-destroyed pillars
                    if (pillar.IsDestroyed) return;

                    pillar.TakeDamage(_damage, BreaksAllMaterials);

                    // Pierce through pillars if pierce remaining
                    if (PierceRemaining > 0)
                    {
                        PierceRemaining--;
                        _damage = Mathf.Max(1, (int)(_damage * 0.7f));
                        return;
                    }

                    ObjectPool.Return(gameObject);
                    return;
                }

                // Player projectile hits enemy or boss
                if (other.CompareTag("Enemy"))
                {
                    int enemyId = other.gameObject.GetInstanceID();

                    // Don't hit the same enemy twice (for pierce/chain)
                    if (_hitEnemies != null && _hitEnemies.Contains(enemyId))
                        return;

                    _hitEnemies ??= new HashSet<int>();
                    _hitEnemies.Add(enemyId);

                    var enemy = other.GetComponent<EnemyBase>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(_damage);

                        // Apply slow effect
                        if (SlowDuration > 0f && SlowFactor < 1f)
                            enemy.ApplySlow(SlowFactor, SlowDuration);

                        // Chain to nearby enemies
                        if (ChainRemaining > 0)
                            TryChain(other.transform.position, enemyId);

                        // Pierce: continue through enemy
                        if (PierceRemaining > 0)
                        {
                            PierceRemaining--;
                            _damage = Mathf.Max(1, (int)(_damage * 0.7f));
                            return; // Don't destroy
                        }

                        ObjectPool.Return(gameObject);
                        return;
                    }

                    // Check for boss component
                    var boss = other.GetComponent<Boss>();
                    if (boss != null)
                    {
                        // Track non-Cannon boss damage for CannonExpert achievement
                        if (!BreaksAllMaterials)
                            AchievementManager.Instance?.RecordNonCannonBossDamage();

                        // Cannon bypasses 30% of boss DPS cap
                        float dpsCapBypass = BreaksAllMaterials ? 0.3f : 0f;
                        boss.TakeDamage(_damage, dpsCapBypass);

                        // Slow at 75% effectiveness on bosses
                        if (SlowDuration > 0f && SlowFactor < 1f)
                            boss.ApplySlow(Mathf.Lerp(1f, SlowFactor, 0.75f), SlowDuration);

                        if (ChainRemaining > 0)
                            TryChain(other.transform.position, enemyId);

                        if (PierceRemaining > 0)
                        {
                            PierceRemaining--;
                            _damage = Mathf.Max(1, (int)(_damage * 0.7f));
                            return;
                        }

                        ObjectPool.Return(gameObject);
                        return;
                    }

                    ObjectPool.Return(gameObject);
                    return;
                }

                // Hit a non-trigger collider (tile/wall) — try to destroy it
                if (!other.isTrigger)
                {
                    TryDestroyTile();
                    ObjectPool.Return(gameObject);
                }
            }
        }

        private void TryChain(Vector3 fromPos, int excludeId)
        {
            float closestDist = ChainRange;
            GameObject closestEnemy = null;

            var enemies = EnemyBase.ActiveEnemies;
            for (int ei = 0; ei < enemies.Count; ei++)
            {
                var enemy = enemies[ei];
                if (enemy == null) continue;
                int id = enemy.GetInstanceID();
                if (id == excludeId) continue;
                if (_hitEnemies != null && _hitEnemies.Contains(id)) continue;

                float dist = Vector2.Distance(fromPos, enemy.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestEnemy = enemy;
                }
            }

            if (closestEnemy != null)
            {
                // Spawn chain arc projectile
                Vector2 dir = ((Vector2)closestEnemy.transform.position - (Vector2)fromPos).normalized;
                var chainGO = ObjectPool.GetProjectile();
                chainGO.transform.position = fromPos;

                var sr = chainGO.GetComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetProjectileSprite(WeaponTier);
                sr.color = new Color(0.5f, 0.8f, 1f, 0.8f); // Electric blue
                sr.sortingOrder = 11;
                chainGO.transform.localScale = Vector3.one * 0.8f;

                var col = chainGO.GetComponent<CircleCollider2D>();
                col.radius = 0.15f;

                var chainProj = chainGO.GetComponent<Projectile>();
                int chainDamage = Mathf.Max(1, (int)(_damage * 0.5f));
                chainProj.Initialize(dir, _speed * 1.5f, chainDamage, false);
                chainProj.WeaponTier = WeaponTier;
                chainProj.ChainRemaining = ChainRemaining - 1;
                chainProj.ChainRange = ChainRange;
                chainProj.SlowDuration = SlowDuration;
                chainProj.SlowFactor = SlowFactor;
                chainProj._hitEnemies = new HashSet<int>(_hitEnemies ?? new HashSet<int>());
            }
        }

        private void TryDestroyTile()
        {
            if (s_levelRenderer == null)
                s_levelRenderer = FindAnyObjectByType<LevelRenderer>();
            var tilemapRenderer = s_levelRenderer;
            if (tilemapRenderer == null) return;

            Vector2Int levelPos = tilemapRenderer.WorldToLevel(transform.position);
            var destructible = tilemapRenderer.GetDestructibleAt(levelPos.x, levelPos.y);

            // Handle indestructible tiles - they can be worn down with repeated hits
            if (destructible.MaterialClass == (byte)MaterialClass.Indestructible)
            {
                if (BreaksAllMaterials)
                {
                    tilemapRenderer.DestroyTile(levelPos.x, levelPos.y);
                    TriggerCannonDebris(levelPos.x, levelPos.y);
                }
                else
                    tilemapRenderer.DamageIndestructibleTile(levelPos.x, levelPos.y);
                return;
            }

            if (destructible.MaterialClass > 0 &&
                destructible.MaterialClass < (byte)MaterialClass.Indestructible)
            {
                bool canBreak = BreaksAllMaterials || CanBreak((MaterialClass)destructible.MaterialClass, WeaponTier);
                if (canBreak)
                {
                    tilemapRenderer.DestroyTile(levelPos.x, levelPos.y);
                    // Cannon destruction triggers FallingDebris on tiles above
                    if (BreaksAllMaterials)
                        TriggerCannonDebris(levelPos.x, levelPos.y);
                }
            }
        }

        private static void TriggerCannonDebris(int tileX, int tileY)
        {
            if (s_hazardSystem == null)
                s_hazardSystem = FindAnyObjectByType<HazardSystem>();
            s_hazardSystem?.OnCannonDestruction(tileX, tileY);
        }

        private static bool CanBreak(MaterialClass material, WeaponTier tier)
        {
            return material switch
            {
                MaterialClass.Soft => true,
                MaterialClass.Medium => true,
                MaterialClass.Hard => tier >= WeaponTier.Medium,
                MaterialClass.Reinforced => tier >= WeaponTier.Heavy,
                _ => false,
            };
        }
    }
}
