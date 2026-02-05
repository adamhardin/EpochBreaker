using UnityEngine;
using SixteenBit.Generative;

namespace SixteenBit.Gameplay
{
    /// <summary>
    /// Manages the player's weapon. Auto-fires at the nearest enemy
    /// within range. Weapon tier determines what materials can be broken.
    /// </summary>
    public class WeaponSystem : MonoBehaviour
    {
        public WeaponTier CurrentTier { get; private set; } = WeaponTier.Starting;

        private float _fireRate = 0.25f; // 4 shots per second
        private float _fireTimer;
        private float _range = 12f;
        private float _projectileSpeed = 14f;
        private int _damage = 1;

        private Transform _currentTarget;

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            _fireTimer -= Time.deltaTime;
            FindTarget();

            if (_currentTarget != null && _fireTimer <= 0f)
            {
                Fire();
                _fireTimer = _fireRate;
            }
        }

        public void UpgradeWeapon(WeaponTier tier)
        {
            if (tier > CurrentTier)
            {
                CurrentTier = tier;

                // Improve stats with tier
                switch (tier)
                {
                    case WeaponTier.Medium:
                        _damage = 2;
                        _fireRate = 0.20f;
                        break;
                    case WeaponTier.Heavy:
                        _damage = 3;
                        _fireRate = 0.15f;
                        break;
                }
            }
        }

        private void FindTarget()
        {
            _currentTarget = null;
            float closestDist = _range;

            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    _currentTarget = enemy.transform;
                }
            }
        }

        private void Fire()
        {
            if (_currentTarget == null) return;

            Vector2 dir = ((Vector2)_currentTarget.position - (Vector2)transform.position).normalized;
            Vector3 spawnPos = transform.position + (Vector3)(dir * 0.6f);
            spawnPos.y += 0.75f; // Offset to chest height (1.5-unit character)

            var projGO = new GameObject("PlayerProjectile");
            projGO.transform.position = spawnPos;

            var sr = projGO.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderAssets.GetProjectileSprite();
            sr.sortingOrder = 11;

            var rb = projGO.AddComponent<Rigidbody2D>();
            rb.isKinematic = true;
            rb.gravityScale = 0f;

            var col = projGO.AddComponent<CircleCollider2D>();
            col.radius = 0.15f;
            col.isTrigger = true;

            var proj = projGO.AddComponent<Projectile>();
            proj.Initialize(dir, _projectileSpeed, _damage, false);
            proj.WeaponTier = CurrentTier;
        }
    }
}
