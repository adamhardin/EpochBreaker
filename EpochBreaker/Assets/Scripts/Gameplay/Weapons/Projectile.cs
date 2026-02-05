using UnityEngine;
using SixteenBit.Generative;

namespace SixteenBit.Gameplay
{
    /// <summary>
    /// Projectile movement and collision. Used by both player weapons
    /// and enemy shooters. Destroys enemies, destructible tiles, or player
    /// depending on source.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        public WeaponTier WeaponTier { get; set; } = WeaponTier.Starting;

        private Vector2 _direction;
        private float _speed;
        private int _damage;
        private bool _isEnemyProjectile;
        private float _lifetime = 3f;

        public void Initialize(Vector2 direction, float speed, int damage, bool isEnemyProjectile)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _isEnemyProjectile = isEnemyProjectile;
        }

        private void Update()
        {
            transform.position += (Vector3)(_direction * _speed * Time.deltaTime);

            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0f)
                Destroy(gameObject);
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
                    Destroy(gameObject);
                }
            }
            else
            {
                // Player projectile hits enemy
                if (other.CompareTag("Enemy"))
                {
                    var enemy = other.GetComponent<EnemyBase>();
                    if (enemy != null)
                        enemy.TakeDamage(_damage);
                    Destroy(gameObject);
                    return;
                }

                // Hit a non-trigger collider (tile/wall) — try to destroy it
                if (!other.isTrigger)
                {
                    TryDestroyTile();
                    Destroy(gameObject);
                }
            }
        }

        private void TryDestroyTile()
        {
            var tilemapRenderer = FindAnyObjectByType<LevelRenderer>();
            if (tilemapRenderer == null) return;

            Vector2Int levelPos = tilemapRenderer.WorldToLevel(transform.position);
            var destructible = tilemapRenderer.GetDestructibleAt(levelPos.x, levelPos.y);

            if (destructible.MaterialClass > 0 &&
                destructible.MaterialClass < (byte)MaterialClass.Indestructible)
            {
                bool canBreak = CanBreak((MaterialClass)destructible.MaterialClass, WeaponTier);
                if (canBreak)
                {
                    tilemapRenderer.DestroyTile(levelPos.x, levelPos.y);
                }
            }
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
