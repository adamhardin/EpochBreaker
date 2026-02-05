using UnityEngine;
using SixteenBit.Generative;

namespace SixteenBit.Gameplay
{
    /// <summary>
    /// Manages the player's weapon. Auto-fires continuously:
    /// targets nearest enemy in range, or fires in the player's
    /// facing direction when no enemies are nearby.
    /// Weapon tier determines what materials can be broken.
    /// </summary>
    public class WeaponSystem : MonoBehaviour
    {
        public WeaponTier CurrentTier { get; private set; } = WeaponTier.Starting;

        private float _fireRate = 0.25f; // 4 shots per second
        private float _fireTimer;
        private float _range = 12f;
        private float _projectileSpeed = 24f;
        private int _damage = 1;

        private Transform _currentTarget;
        private PlayerController _player;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;
            if (_player != null && !_player.IsAlive) return;

            _fireTimer -= Time.deltaTime;
            FindTarget();

            if (_fireTimer <= 0f)
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
            Vector2 dir;

            if (_currentTarget != null)
            {
                // Aim at nearest enemy
                dir = ((Vector2)_currentTarget.position - (Vector2)transform.position).normalized;
            }
            else
            {
                // No enemy in range — fire in facing direction
                bool facingRight = _player != null ? _player.FacingRight : true;
                dir = facingRight ? Vector2.right : Vector2.left;
            }

            Vector3 spawnPos = transform.position + (Vector3)(dir * 0.6f);
            spawnPos.y += 0.75f; // Offset to chest height (1.5-unit character)

            var projGO = new GameObject("PlayerProjectile");
            projGO.transform.position = spawnPos;

            var sr = projGO.AddComponent<SpriteRenderer>();
            int epoch = GameManager.Instance?.CurrentEpoch ?? 0;
            sr.sprite = PlaceholderAssets.GetProjectileSprite(CurrentTier, epoch);
            sr.sortingOrder = 11;

            // Scale projectile based on tier for visual feedback
            float projScale = CurrentTier switch
            {
                WeaponTier.Heavy => 1.5f,
                WeaponTier.Medium => 1.25f,
                _ => 1.0f
            };
            projGO.transform.localScale = new Vector3(projScale, projScale, 1f);

            var rb = projGO.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            var col = projGO.AddComponent<CircleCollider2D>();
            col.radius = 0.15f;
            col.isTrigger = true;

            var proj = projGO.AddComponent<Projectile>();
            proj.Initialize(dir, _projectileSpeed, _damage, false);
            proj.WeaponTier = CurrentTier;

            AudioManager.PlaySFX(PlaceholderAudio.GetShootSFX(), 0.1f);
        }
    }
}
