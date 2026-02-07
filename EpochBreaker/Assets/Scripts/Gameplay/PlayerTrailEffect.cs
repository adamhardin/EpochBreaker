using UnityEngine;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Spawns trail particles behind the player while moving.
    /// Uses ObjectPool for particle recycling. Different visual
    /// behaviors per TrailEffect type.
    /// </summary>
    public class PlayerTrailEffect : MonoBehaviour
    {
        private TrailEffect _trailType = TrailEffect.None;
        private Rigidbody2D _rb;
        private float _spawnTimer;
        private float _glitchFlickerTimer;

        // Spawn rate and particle settings per trail type
        private const float SPAWN_INTERVAL = 0.05f;
        private const float MIN_SPEED = 0.5f; // Minimum speed to emit particles

        public void Initialize(TrailEffect trailType)
        {
            _trailType = trailType;
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (_trailType == TrailEffect.None || _rb == null) return;

            float speed = _rb.linearVelocity.magnitude;
            if (speed < MIN_SPEED) return;

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnParticle();
                // Faster spawn rate at higher speeds
                _spawnTimer = SPAWN_INTERVAL * Mathf.Clamp(2f / speed, 0.3f, 1f);
            }
        }

        private void SpawnParticle()
        {
            var go = ObjectPool.GetParticle();
            if (go == null) return;

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) return;
            sr.sprite = PlaceholderAssets.GetParticleSprite();
            sr.sortingOrder = 9;

            var rb = go.GetComponent<Rigidbody2D>();
            var timer = go.GetComponent<PoolTimer>();

            // Base position: at player center with small random offset
            Vector3 pos = transform.position;
            pos.y += 0.6f; // Center of player sprite

            switch (_trailType)
            {
                case TrailEffect.Sparks:
                    ConfigureSparks(go, sr, rb, timer, pos);
                    break;
                case TrailEffect.Frost:
                    ConfigureFrost(go, sr, rb, timer, pos);
                    break;
                case TrailEffect.Fire:
                    ConfigureFire(go, sr, rb, timer, pos);
                    break;
                case TrailEffect.Glitch:
                    ConfigureGlitch(go, sr, rb, timer, pos);
                    break;
            }
        }

        private void ConfigureSparks(GameObject go, SpriteRenderer sr, Rigidbody2D rb, PoolTimer timer, Vector3 pos)
        {
            // Orange/yellow, small, short life
            float t = Random.value;
            Color color = Color.Lerp(
                CosmeticManager.GetTrailColor(TrailEffect.Sparks),
                CosmeticManager.GetTrailSecondaryColor(TrailEffect.Sparks), t);
            sr.color = color;
            go.transform.localScale = Vector3.one * Random.Range(0.3f, 0.6f);

            pos.x += Random.Range(-0.3f, 0.3f);
            pos.y += Random.Range(-0.3f, 0.3f);
            go.transform.position = pos;

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-0.5f, 1f));
            }

            if (timer != null) timer.StartTimer(Random.Range(0.1f, 0.25f));
        }

        private void ConfigureFrost(GameObject go, SpriteRenderer sr, Rigidbody2D rb, PoolTimer timer, Vector3 pos)
        {
            // Light blue/white, medium, drift down
            float t = Random.value;
            Color color = Color.Lerp(
                CosmeticManager.GetTrailColor(TrailEffect.Frost),
                CosmeticManager.GetTrailSecondaryColor(TrailEffect.Frost), t);
            sr.color = color;
            go.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);

            pos.x += Random.Range(-0.2f, 0.2f);
            pos.y += Random.Range(-0.2f, 0.2f);
            go.transform.position = pos;

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-1.5f, -0.5f));
            }

            if (timer != null) timer.StartTimer(Random.Range(0.2f, 0.4f));
        }

        private void ConfigureFire(GameObject go, SpriteRenderer sr, Rigidbody2D rb, PoolTimer timer, Vector3 pos)
        {
            // Red/orange, medium, drift up
            float t = Random.value;
            Color color = Color.Lerp(
                CosmeticManager.GetTrailColor(TrailEffect.Fire),
                CosmeticManager.GetTrailSecondaryColor(TrailEffect.Fire), t);
            sr.color = color;
            go.transform.localScale = Vector3.one * Random.Range(0.4f, 0.9f);

            pos.x += Random.Range(-0.3f, 0.3f);
            pos.y += Random.Range(-0.4f, 0f);
            go.transform.position = pos;

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(0.5f, 2f));
            }

            if (timer != null) timer.StartTimer(Random.Range(0.15f, 0.35f));
        }

        private void ConfigureGlitch(GameObject go, SpriteRenderer sr, Rigidbody2D rb, PoolTimer timer, Vector3 pos)
        {
            // Cyan/magenta, rapid flicker, random offset
            _glitchFlickerTimer += Time.deltaTime;
            bool usePrimary = (int)(_glitchFlickerTimer * 12f) % 2 == 0;
            Color color = usePrimary
                ? CosmeticManager.GetTrailColor(TrailEffect.Glitch)
                : CosmeticManager.GetTrailSecondaryColor(TrailEffect.Glitch);
            sr.color = color;
            go.transform.localScale = Vector3.one * Random.Range(0.3f, 0.7f);

            // Glitch: larger random offset for distortion effect
            pos.x += Random.Range(-0.6f, 0.6f);
            pos.y += Random.Range(-0.5f, 0.5f);
            go.transform.position = pos;

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f));
            }

            if (timer != null) timer.StartTimer(Random.Range(0.05f, 0.15f));
        }
    }
}
