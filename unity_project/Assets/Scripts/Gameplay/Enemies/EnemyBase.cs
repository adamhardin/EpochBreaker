using UnityEngine;
using System.Collections.Generic;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Base enemy component. Handles health, behavior AI (patrol, chase,
    /// stationary, flying), and combat interactions.
    /// </summary>
    public class EnemyBase : MonoBehaviour
    {
        // Static registry of all active enemies (avoids FindGameObjectsWithTag allocations)
        private static readonly List<GameObject> s_activeEnemies = new List<GameObject>();
        public static IReadOnlyList<GameObject> ActiveEnemies => s_activeEnemies;
        public static void ClearRegistry() => s_activeEnemies.Clear();
        public static void Register(GameObject go) => s_activeEnemies.Add(go);
        public static void Unregister(GameObject go) => s_activeEnemies.Remove(go);

        public EnemyType Type { get; private set; }
        public EnemyBehavior Behavior { get; private set; }
        public int Health { get; private set; } = 3;
        public bool IsDead { get; private set; }

        private int _patrolMinX;
        private int _patrolMaxX;
        private float _moveSpeed = 2f;
        private float _baseMoveSpeed = 2f;
        private int _direction = 1;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private LevelRenderer _tilemapRenderer;
        private Transform _playerTransform;
        private Color _baseColor = Color.white;
        private Color _originalColor = Color.white;

        // Slow effect
        private float _slowFactor = 1f;
        private float _slowTimer;

        // Chase behavior
        private float _chaseRange = 8f;
        private float _chaseSpeed = 3.5f;
        private float _baseChaseSpeed = 3.5f;

        // Shooter behavior
        private float _shootCooldown = 1.5f;
        private float _shootTimer;
        private float _shootRange = 10f;

        // Flying behavior
        private float _flyHeight;
        private float _flyBobSpeed = 1.5f;
        private float _flyBobAmount = 0.5f;

        public void Initialize(EnemyData data, LevelRenderer tilemapRenderer)
        {
            Type = data.Type;
            Behavior = data.Behavior;
            _patrolMinX = data.PatrolMinX;
            _patrolMaxX = data.PatrolMaxX;
            _tilemapRenderer = tilemapRenderer;

            // Apply difficulty scaling from epoch profile
            int era = (int)data.Type / 3;
            var diff = Generative.DifficultyProfile.GetParams(era);

            // Scale health with difficulty weight × HP multiplier
            Health = Mathf.Max(1, Mathf.RoundToInt(data.DifficultyWeight * diff.EnemyHpMultiplier * 3f));

            // Scale movement speed
            _moveSpeed *= diff.EnemySpeedMultiplier;
            _chaseSpeed *= diff.EnemySpeedMultiplier;

            // Shooter enemies are slower but shoot
            if (Behavior == EnemyBehavior.Stationary)
                _moveSpeed = 0f;

            _baseMoveSpeed = _moveSpeed;
            _baseChaseSpeed = _chaseSpeed;

            if (Behavior == EnemyBehavior.Flying)
            {
                _flyHeight = transform.position.y;
                _rb = GetComponent<Rigidbody2D>();
                if (_rb != null) _rb.gravityScale = 0f;
            }
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null)
                _originalColor = _sr.color;
            _playerTransform = GameObject.FindWithTag("Player")?.transform;
            Register(gameObject);
        }

        private void OnDestroy()
        {
            Unregister(gameObject);
        }

        private void FixedUpdate()
        {
            // Lazy re-find player if reference was lost (e.g., level reload)
            if (_playerTransform == null)
                _playerTransform = GameObject.FindWithTag("Player")?.transform;

            if (IsDead || GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                if (_rb != null) _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                return;
            }

            // Update slow effect
            UpdateSlow();

            switch (Behavior)
            {
                case EnemyBehavior.Patrol:
                    UpdatePatrol();
                    break;
                case EnemyBehavior.Chase:
                    UpdateChase();
                    break;
                case EnemyBehavior.Stationary:
                    UpdateStationary();
                    break;
                case EnemyBehavior.Flying:
                    UpdateFlying();
                    break;
            }
        }

        private void UpdatePatrol()
        {
            if (_rb == null) return;

            float posX = transform.position.x;

            // Reverse at patrol bounds
            if (posX <= _patrolMinX && _direction < 0)
                _direction = 1;
            else if (posX >= _patrolMaxX && _direction > 0)
                _direction = -1;

            _rb.linearVelocity = new Vector2(_direction * _moveSpeed, _rb.linearVelocity.y);

            // Flip sprite
            if (_sr != null)
                _sr.flipX = _direction < 0;
        }

        private void UpdateChase()
        {
            if (_rb == null || _playerTransform == null) return;

            float distToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

            if (distToPlayer < _chaseRange)
            {
                // Chase player
                float dir = Mathf.Sign(_playerTransform.position.x - transform.position.x);
                _rb.linearVelocity = new Vector2(dir * _chaseSpeed, _rb.linearVelocity.y);

                if (_sr != null)
                    _sr.flipX = dir < 0;

                // Shoot while chasing (longer cooldown than stationary)
                TryShoot(distToPlayer, 2.5f);
            }
            else
            {
                // Fall back to patrol
                UpdatePatrol();
            }
        }

        private void UpdateStationary()
        {
            if (_rb != null)
                _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);

            // Face player
            if (_playerTransform != null && _sr != null)
            {
                _sr.flipX = _playerTransform.position.x < transform.position.x;
            }

            // Shoot at player (fastest fire rate)
            if (_playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, _playerTransform.position);
                TryShoot(dist, _shootCooldown);
            }
        }

        private void UpdateFlying()
        {
            if (_rb == null) return;

            // Bob vertically
            float targetY = _flyHeight + Mathf.Sin(Time.time * _flyBobSpeed) * _flyBobAmount;
            float yVel = (targetY - transform.position.y) * 3f;

            // Patrol horizontally
            float posX = transform.position.x;
            if (posX <= _patrolMinX && _direction < 0)
                _direction = 1;
            else if (posX >= _patrolMaxX && _direction > 0)
                _direction = -1;

            _rb.linearVelocity = new Vector2(_direction * _moveSpeed, yVel);

            if (_sr != null)
                _sr.flipX = _direction < 0;

            // Flying enemies also shoot (longer range, longer cooldown)
            if (_playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, _playerTransform.position);
                TryShoot(dist, 2.0f);
            }
        }

        /// <summary>
        /// Attempt to shoot at the player if within range and cooldown elapsed.
        /// </summary>
        private void TryShoot(float distToPlayer, float cooldown)
        {
            _shootTimer -= Time.fixedDeltaTime;
            if (_shootTimer <= 0f && distToPlayer < _shootRange)
            {
                ShootAtPlayer();
                _shootTimer = cooldown;
            }
        }

        private void ShootAtPlayer()
        {
            if (_playerTransform == null) return;

            Vector2 dir = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
            Vector3 spawnPos = transform.position + (Vector3)(dir * 1.0f);

            var projGO = ObjectPool.GetProjectile();
            projGO.transform.position = spawnPos;

            var sr = projGO.GetComponent<SpriteRenderer>();
            int era = (int)Type / 3;
            sr.sprite = PlaceholderAssets.GetProjectileSprite(WeaponTier.Starting, era);
            sr.color = new Color(1f, 0.5f, 0.5f); // Tinted red to distinguish from player projectiles
            sr.sortingOrder = 11;

            var col = projGO.GetComponent<CircleCollider2D>();
            col.radius = 0.2f;

            var proj = projGO.GetComponent<Projectile>();
            proj.Initialize(dir, 6f, 1, true);
        }

        /// <summary>
        /// Apply a slow effect that reduces movement speed for a duration.
        /// Stacks by refreshing duration; takes the strongest slow factor.
        /// </summary>
        public void ApplySlow(float factor, float duration)
        {
            _slowFactor = Mathf.Min(_slowFactor, factor);
            _slowTimer = Mathf.Max(_slowTimer, duration);
            _moveSpeed = _baseMoveSpeed * _slowFactor;
            _chaseSpeed = _baseChaseSpeed * _slowFactor;

            // Blue tint to indicate slowed
            if (_sr != null)
                _baseColor = new Color(0.5f, 0.7f, 1f);
        }

        private void UpdateSlow()
        {
            if (_slowTimer <= 0f) return;

            _slowTimer -= Time.fixedDeltaTime;
            if (_slowTimer <= 0f)
            {
                _slowFactor = 1f;
                _moveSpeed = _baseMoveSpeed;
                _chaseSpeed = _baseChaseSpeed;
                _baseColor = _originalColor;
                if (_sr != null)
                    _sr.color = _originalColor;
            }
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;

            Health -= amount;

            AudioManager.PlaySFX(PlaceholderAudio.GetEnemyHitSFX());

            // Flash red
            if (_sr != null)
                _sr.color = Color.red;
            Invoke(nameof(ResetColor), 0.1f);

            if (Health <= 0)
            {
                Die();
            }
        }

        private void ResetColor()
        {
            if (_sr != null)
                _sr.color = _baseColor;
        }

        private void Die()
        {
            IsDead = true;

            if (GameManager.Instance != null)
                GameManager.Instance.RecordEnemyKill();

            AudioManager.PlaySFX(PlaceholderAudio.GetEnemyDieSFX());

            // Screen shake on enemy death
            CameraController.Instance?.Shake(0.08f, 0.12f);

            // Spawn death particles
            SpawnDeathParticles();

            // Simple death: destroy after brief delay
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.gravityScale = 0f;
            }

            if (_sr != null)
                _sr.color = new Color(1f, 1f, 1f, 0.3f);

            Destroy(gameObject, 0.3f);
        }

        private void SpawnDeathParticles()
        {
            int era = (int)Type / 3;
            Color particleColor = PlaceholderAssets.GetEraBodyColor(era);

            // Death flash (bright, short-lived)
            var flashGO = ObjectPool.GetFlash();
            flashGO.transform.position = transform.position;
            var flashSR = flashGO.GetComponent<SpriteRenderer>();
            flashSR.sprite = PlaceholderAssets.GetParticleSprite();
            flashSR.color = new Color(1f, 1f, 0.9f, 0.9f);
            flashSR.sortingOrder = 16;
            flashGO.transform.localScale = Vector3.one * 1.5f;
            flashGO.GetComponent<PoolTimer>().StartTimer(0.06f);

            // Burst particles
            int count = Random.Range(5, 8);
            for (int i = 0; i < count; i++)
            {
                var go = ObjectPool.GetParticle();
                go.transform.position = transform.position + new Vector3(
                    Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0f);
                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetParticleSprite();
                sr.color = new Color(particleColor.r, particleColor.g, particleColor.b, 0.8f);
                sr.sortingOrder = 15;
                go.transform.localScale = Vector3.one * Random.Range(0.15f, 0.45f);

                var rb = go.GetComponent<Rigidbody2D>();
                rb.gravityScale = Random.Range(2f, 4f);
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float speed = Random.Range(3f, 7f);
                rb.linearVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;

                go.GetComponent<PoolTimer>().StartTimer(Random.Range(0.4f, 0.7f));
            }
        }
    }
}
