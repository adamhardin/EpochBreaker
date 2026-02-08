using UnityEngine;
using System.Collections.Generic;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Manages hazard effects triggered when destructible tiles are broken.
    /// Attached to the LevelRoot by LevelLoader.
    /// </summary>
    public class HazardSystem : MonoBehaviour
    {
        private LevelData _levelData;
        private LevelRenderer _renderer;
        private List<ActiveHazard> _activeHazards = new List<ActiveHazard>();
        private Generative.XORShift64 _rng;
        private Transform _cachedPlayerTransform;
        private HealthSystem _cachedPlayerHealth;

        private struct ActiveHazard
        {
            public HazardType Type;
            public Vector3 Position;
            public float TimeRemaining;
            public float DamageTimer;
            public GameObject Visual;
        }

        public void Initialize(LevelData data, LevelRenderer renderer)
        {
            _levelData = data;
            _renderer = renderer;
            _rng = new Generative.XORShift64(data.ID.Seed + 99999UL);
        }

        private void Update()
        {
            if (GameManager.Instance?.CurrentState != GameState.Playing) return;
            if (_activeHazards.Count == 0) return;

            // Lazy re-find player (handles respawn)
            if (_cachedPlayerTransform == null)
            {
                _cachedPlayerTransform = GameManager.PlayerTransform;
                if (_cachedPlayerTransform == null) return;
                _cachedPlayerHealth = _cachedPlayerTransform.GetComponent<HealthSystem>();
            }

            for (int i = _activeHazards.Count - 1; i >= 0; i--)
            {
                var h = _activeHazards[i];
                h.TimeRemaining -= Time.deltaTime;
                h.DamageTimer -= Time.deltaTime;

                if (h.TimeRemaining <= 0f)
                {
                    if (h.Visual != null) ObjectPool.Return(h.Visual);
                    _activeHazards.RemoveAt(i);
                    continue;
                }

                // Check player proximity and deal damage
                if (_cachedPlayerHealth != null && h.DamageTimer <= 0f)
                {
                    float distSq = ((Vector2)_cachedPlayerTransform.position - (Vector2)h.Position).sqrMagnitude;
                    float radius = GetHazardRadius(h.Type);
                    float cooldown = GetHazardDamageCooldown(h.Type);
                    int damage = GetHazardDamage(h.Type);

                    if (distSq < radius * radius)
                    {
                        _cachedPlayerHealth.TakeDamage(damage, h.Position, isEnvironmental: true);
                        h.DamageTimer = cooldown;
                        AchievementManager.Instance?.RecordHazardDamage();
                    }
                }

                _activeHazards[i] = h;
            }
        }

        /// <summary>
        /// Called by LevelRenderer.DestroyTile when a tile with a hazard is broken.
        /// </summary>
        public void OnTileDestroyed(int tileX, int tileY, HazardType hazard)
        {
            if (hazard == HazardType.None) return;
            if (_renderer == null) return;

            // Show hazard hint on first trigger
            if (TutorialManager.Instance != null && !TutorialManager.IsHintDismissed("hazard"))
            {
                TutorialManager.Instance.ShowGameplayHintPublic("Caution! Breaking some blocks releases hazards!");
                TutorialManager.DismissHint("hazard");
            }

            Vector3 worldPos = _renderer.LevelToWorld(tileX, tileY);

            switch (hazard)
            {
                case HazardType.FallingDebris:
                    SpawnFallingDebris(tileX, tileY, worldPos);
                    break;
                case HazardType.GasRelease:
                    SpawnHazardCloud(worldPos, HazardType.GasRelease, 3f,
                        new Color(0.2f, 0.8f, 0.2f, 0.5f));
                    AudioManager.PlaySFX(PlaceholderAudio.GetGasHissSFX());
                    break;
                case HazardType.FireRelease:
                    SpawnHazardCloud(worldPos, HazardType.FireRelease, 4f,
                        new Color(1f, 0.5f, 0.1f, 0.6f));
                    AudioManager.PlaySFX(PlaceholderAudio.GetFireSFX());
                    break;
                case HazardType.SpikeTrap:
                    SpawnSpikes(worldPos);
                    AudioManager.PlaySFX(PlaceholderAudio.GetSpikeSFX());
                    break;
                case HazardType.UnstableFloor:
                    // Unstable floor collapses after a delay — spawns falling debris below
                    SpawnFallingDebris(tileX, tileY, worldPos);
                    break;
                case HazardType.CoverWall:
                    // Cover wall is a structural hazard — no active effect on destruction
                    // (its value is as intact cover, so destroying it is the consequence)
                    AudioManager.PlaySFX(PlaceholderAudio.GetDebrisSFX());
                    break;
            }
        }

        /// <summary>
        /// Called when a Cannon projectile destroys a tile.
        /// Each tile directly above has a 40% chance to become FallingDebris,
        /// with a 0.3s crack pre-telegraph before falling.
        /// </summary>
        public void OnCannonDestruction(int tileX, int tileY)
        {
            if (_levelData == null || _renderer == null) return;
            int width = _levelData.Layout.WidthTiles;

            for (int y = tileY - 1; y >= 0; y--)
            {
                int idx = y * width + tileX;
                if (idx < 0 || idx >= _levelData.Layout.Tiles.Length) break;
                if (_levelData.Layout.Destructibles[idx].MaterialClass == 0) break;

                // Skip tiles that already have a hazard assigned
                if (_levelData.Layout.Destructibles[idx].Hazard != HazardType.None) continue;

                // 40% chance to trigger FallingDebris per tile above (deterministic)
                if (_rng != null && (_rng.Next() % 100) < 40)
                {
                    Vector3 debrisPos = _renderer.LevelToWorld(tileX, y);
                    SpawnDebrisObject(debrisPos);
                    _renderer.DestroyTileRaw(tileX, y);
                }
            }

            AudioManager.PlaySFX(PlaceholderAudio.GetDebrisSFX());
        }

        private void SpawnFallingDebris(int tileX, int tileY, Vector3 worldPos)
        {
            if (_levelData == null) return;
            int width = _levelData.Layout.WidthTiles;

            // Drop tiles directly above as physics objects
            for (int y = tileY - 1; y >= 0; y--)
            {
                int idx = y * width + tileX;
                if (idx < 0 || idx >= _levelData.Layout.Tiles.Length) break;
                if (_levelData.Layout.Destructibles[idx].MaterialClass == 0) break;

                Vector3 debrisPos = _renderer.LevelToWorld(tileX, y);
                SpawnDebrisObject(debrisPos);

                // Clear the tile
                _renderer.DestroyTileRaw(tileX, y);
            }

            AudioManager.PlaySFX(PlaceholderAudio.GetDebrisSFX());
        }

        private void SpawnDebrisObject(Vector3 pos)
        {
            var go = ObjectPool.GetDebris();
            go.transform.position = pos;

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = PlaceholderAssets.GetDebrisSprite();
            sr.sortingOrder = 15;
            sr.color = new Color(0.6f, 0.5f, 0.4f);

            var rb = go.GetComponent<Rigidbody2D>();
            rb.gravityScale = 2f;
            rb.mass = 0.5f;
            rb.linearVelocity = new Vector2(Random.Range(-1f, 1f), 0.5f);

            var dmg = go.GetComponent<HazardDamager>();
            dmg.Damage = 2;

            go.GetComponent<PoolTimer>().StartTimer(3f);
        }

        private void SpawnHazardCloud(Vector3 pos, HazardType type, float duration, Color color)
        {
            var go = ObjectPool.GetHazardVisual();
            go.transform.position = pos;

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = PlaceholderAssets.GetHazardCloudSprite();
            sr.sortingOrder = 14;
            sr.color = color;
            go.transform.localScale = Vector3.one * 2f;

            _activeHazards.Add(new ActiveHazard
            {
                Type = type,
                Position = pos,
                TimeRemaining = duration,
                DamageTimer = 0.3f, // Small grace period
                Visual = go,
            });
        }

        private void SpawnSpikes(Vector3 pos)
        {
            var go = ObjectPool.GetHazardVisual();
            go.transform.position = pos;

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = PlaceholderAssets.GetSpikeSprite();
            sr.sortingOrder = 14;
            sr.color = new Color(0.7f, 0.7f, 0.7f);
            go.transform.localScale = new Vector3(1.5f, 1f, 1f);

            _activeHazards.Add(new ActiveHazard
            {
                Type = HazardType.SpikeTrap,
                Position = pos,
                TimeRemaining = 5f,
                DamageTimer = 0f,
                Visual = go,
            });
        }

        private static float GetHazardRadius(HazardType type)
        {
            return type switch
            {
                HazardType.GasRelease => 1.5f,
                HazardType.FireRelease => 1.0f,
                HazardType.SpikeTrap => 1.2f,
                _ => 1.5f
            };
        }

        private static float GetHazardDamageCooldown(HazardType type)
        {
            return type switch
            {
                HazardType.GasRelease => 0.5f,
                HazardType.FireRelease => 0.3f,
                HazardType.SpikeTrap => 0.8f,
                _ => 0.5f
            };
        }

        private static int GetHazardDamage(HazardType type)
        {
            return type switch
            {
                HazardType.SpikeTrap => 2,
                _ => 1
            };
        }
    }

    /// <summary>
    /// Simple component for falling debris that damages player on trigger contact.
    /// </summary>
    public class HazardDamager : MonoBehaviour
    {
        public int Damage = 2;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            var health = other.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(Damage, transform.position, isEnvironmental: true);
                AchievementManager.Instance?.RecordHazardDamage();
            }
            ObjectPool.Return(gameObject);
        }
    }
}
