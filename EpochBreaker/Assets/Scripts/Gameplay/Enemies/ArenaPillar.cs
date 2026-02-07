using UnityEngine;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Destructible arena pillar spawned in boss arenas during Phase 3.
    /// Boss shelters behind pillars; player must destroy them to expose the boss.
    /// Cannon (BreaksAllMaterials) destroys instantly; other weapons need multiple hits.
    /// When destroyed, triggers FallingDebris via HazardSystem.
    /// </summary>
    public class ArenaPillar : MonoBehaviour
    {
        public const int MAX_HP = 10;
        public int Health { get; private set; } = MAX_HP;
        public bool IsDestroyed { get; private set; }

        private SpriteRenderer _sr;
        private Color _baseColor;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null)
                _baseColor = _sr.color;
        }

        /// <summary>
        /// Take damage from a projectile. Returns true if pillar was destroyed.
        /// </summary>
        public bool TakeDamage(int amount, bool breaksAllMaterials)
        {
            if (IsDestroyed) return false;

            // Cannon breaks instantly
            if (breaksAllMaterials)
            {
                Health = 0;
            }
            else
            {
                Health -= amount;
            }

            // Flash white on hit
            if (_sr != null)
                _sr.color = Color.white;
            Invoke(nameof(ResetColor), 0.1f);

            if (Health <= 0)
            {
                DestroyPillar();
                return true;
            }

            // Visual crack: darken as health drops
            float hpRatio = (float)Health / MAX_HP;
            _baseColor = Color.Lerp(new Color(0.4f, 0.3f, 0.25f), new Color(0.6f, 0.55f, 0.45f), hpRatio);

            return false;
        }

        private void ResetColor()
        {
            if (_sr != null)
                _sr.color = _baseColor;
        }

        private void DestroyPillar()
        {
            IsDestroyed = true;

            // Trigger FallingDebris at pillar position
            var hazardSystem = FindAnyObjectByType<HazardSystem>();
            if (hazardSystem != null)
            {
                // Spawn debris objects at pillar location
                SpawnPillarDebris();
            }

            AudioManager.PlaySFX(PlaceholderAudio.GetDebrisSFX());

            // Notify boss that this pillar is destroyed
            var boss = FindAnyObjectByType<Boss>();
            boss?.OnPillarDestroyed(this);

            Destroy(gameObject);
        }

        private void SpawnPillarDebris()
        {
            // Spawn 3-4 debris chunks with manual physics
            for (int i = 0; i < 4; i++)
            {
                var go = new GameObject("PillarDebris");
                go.transform.position = transform.position + new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(0f, 2f),
                    0f);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetDebrisSprite();
                sr.sortingOrder = 15;
                sr.color = new Color(0.55f, 0.50f, 0.40f);

                var rb = go.AddComponent<Rigidbody2D>();
                rb.gravityScale = 2.5f;
                rb.mass = 0.3f;
                rb.linearVelocity = new Vector2(Random.Range(-2f, 2f), Random.Range(2f, 5f));

                // Solid collider for ground collision
                var col = go.AddComponent<CircleCollider2D>();
                col.radius = 0.25f;

                // Trigger collider (slightly larger) for player damage detection
                var triggerCol = go.AddComponent<CircleCollider2D>();
                triggerCol.radius = 0.35f;
                triggerCol.isTrigger = true;

                var dmg = go.AddComponent<HazardDamager>();
                dmg.Damage = 2;

                Object.Destroy(go, 3f);
            }
        }
    }
}
