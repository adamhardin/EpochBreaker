using UnityEngine;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Boss enemy component. Larger than normal enemies, higher health,
    /// spawns after the BossArena checkpoint is activated.
    /// Bosses have multi-phase behavior and area attacks.
    /// </summary>
    public class Boss : MonoBehaviour
    {
        public BossType Type { get; private set; }
        public int MaxHealth { get; private set; }
        public int Health { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsActive { get; private set; }

        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private Transform _playerTransform;
        private LevelRenderer _tilemapRenderer;

        // Movement
        private float _moveSpeed = 2.5f;
        private int _direction = 1;
        private float _arenaMinX;
        private float _arenaMaxX;

        // Combat
        private float _attackCooldown = 2.0f;
        private float _attackTimer;
        private float _chargeSpeed = 6f;
        private bool _isCharging;
        private float _chargeTimer;
        private const float CHARGE_DURATION = 0.8f;

        // Phase system (changes behavior at health thresholds)
        private int _currentPhase = 1;

        public void Initialize(BossType type, LevelRenderer tilemapRenderer, float arenaMinX, float arenaMaxX)
        {
            Type = type;
            _tilemapRenderer = tilemapRenderer;
            _arenaMinX = arenaMinX;
            _arenaMaxX = arenaMaxX;

            // Boss health scales with era (type value 0-9)
            // Much higher than regular enemies to make bosses feel like a real fight
            int era = (int)type;
            MaxHealth = 100 + era * 20; // 100-280 HP based on era
            Health = MaxHealth;

            // Movement speed increases slightly with era
            _moveSpeed = 2.0f + era * 0.15f;
            _chargeSpeed = 5f + era * 0.3f;
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            _playerTransform = GameObject.FindWithTag("Player")?.transform;
        }

        /// <summary>
        /// Activate the boss. Called when player enters the boss arena.
        /// </summary>
        public void Activate()
        {
            if (IsActive || IsDead) return;
            IsActive = true;

            // Boss activation announcement could trigger here
            // For now, just start moving
        }

        private void FixedUpdate()
        {
            if (IsDead || !IsActive) return;
            if (GameManager.Instance?.CurrentState != GameState.Playing) return;

            // Check for phase transitions
            UpdatePhase();

            // Update behavior based on phase
            switch (_currentPhase)
            {
                case 1:
                    UpdatePhase1();
                    break;
                case 2:
                    UpdatePhase2();
                    break;
                case 3:
                    UpdatePhase3();
                    break;
            }
        }

        private void UpdatePhase()
        {
            float healthPercent = (float)Health / MaxHealth;

            if (healthPercent <= 0.33f && _currentPhase < 3)
            {
                _currentPhase = 3;
                OnPhaseChange(3);
            }
            else if (healthPercent <= 0.66f && _currentPhase < 2)
            {
                _currentPhase = 2;
                OnPhaseChange(2);
            }
        }

        private void OnPhaseChange(int newPhase)
        {
            // Visual feedback on phase change
            if (_sr != null)
            {
                // Flash and tint slightly based on phase
                float tint = 1f - (newPhase - 1) * 0.15f;
                _sr.color = new Color(1f, tint, tint);
            }

            // Speed up in later phases
            _moveSpeed *= 1.2f;
            _attackCooldown *= 0.8f;
        }

        /// <summary>
        /// Phase 1: Patrol and occasional charge attacks
        /// </summary>
        private void UpdatePhase1()
        {
            if (_rb == null) return;

            if (_isCharging)
            {
                UpdateCharge();
                return;
            }

            // Patrol within arena bounds
            float posX = transform.position.x;
            if (posX <= _arenaMinX + 2 && _direction < 0)
                _direction = 1;
            else if (posX >= _arenaMaxX - 2 && _direction > 0)
                _direction = -1;

            _rb.linearVelocity = new Vector2(_direction * _moveSpeed, _rb.linearVelocity.y);

            if (_sr != null)
                _sr.flipX = _direction < 0;

            // Attack cooldown
            _attackTimer -= Time.fixedDeltaTime;
            if (_attackTimer <= 0f && _playerTransform != null)
            {
                float distToPlayer = Mathf.Abs(_playerTransform.position.x - transform.position.x);
                if (distToPlayer < 12f)
                {
                    StartCharge();
                    _attackTimer = _attackCooldown;
                }
            }
        }

        /// <summary>
        /// Phase 2: More aggressive, shoots projectiles
        /// </summary>
        private void UpdatePhase2()
        {
            if (_rb == null) return;

            if (_isCharging)
            {
                UpdateCharge();
                return;
            }

            // Chase player more actively
            if (_playerTransform != null)
            {
                float dir = Mathf.Sign(_playerTransform.position.x - transform.position.x);

                // Clamp to arena bounds
                float targetX = transform.position.x + dir * _moveSpeed * Time.fixedDeltaTime;
                if (targetX < _arenaMinX + 2) dir = 1;
                if (targetX > _arenaMaxX - 2) dir = -1;

                _rb.linearVelocity = new Vector2(dir * _moveSpeed * 1.3f, _rb.linearVelocity.y);

                if (_sr != null)
                    _sr.flipX = dir < 0;
            }

            // Attack with projectiles and charges
            _attackTimer -= Time.fixedDeltaTime;
            if (_attackTimer <= 0f && _playerTransform != null)
            {
                if (Random.value < 0.4f)
                {
                    ShootAtPlayer();
                }
                else
                {
                    StartCharge();
                }
                _attackTimer = _attackCooldown * 0.7f;
            }
        }

        /// <summary>
        /// Phase 3: Desperate, constant attacks
        /// </summary>
        private void UpdatePhase3()
        {
            if (_rb == null) return;

            if (_isCharging)
            {
                UpdateCharge();
                return;
            }

            // Aggressively chase
            if (_playerTransform != null)
            {
                float dir = Mathf.Sign(_playerTransform.position.x - transform.position.x);

                float targetX = transform.position.x + dir * _moveSpeed * Time.fixedDeltaTime;
                if (targetX < _arenaMinX + 2) dir = 1;
                if (targetX > _arenaMaxX - 2) dir = -1;

                _rb.linearVelocity = new Vector2(dir * _moveSpeed * 1.5f, _rb.linearVelocity.y);

                if (_sr != null)
                    _sr.flipX = dir < 0;
            }

            // Rapid attacks
            _attackTimer -= Time.fixedDeltaTime;
            if (_attackTimer <= 0f && _playerTransform != null)
            {
                // Alternate between charge and multi-shot
                if (Random.value < 0.5f)
                {
                    ShootSpread();
                }
                else
                {
                    StartCharge();
                }
                _attackTimer = _attackCooldown * 0.5f;
            }
        }

        private void StartCharge()
        {
            if (_playerTransform == null) return;

            _isCharging = true;
            _chargeTimer = CHARGE_DURATION;
            _direction = _playerTransform.position.x > transform.position.x ? 1 : -1;

            // Visual wind-up
            if (_sr != null)
                _sr.color = new Color(1f, 0.8f, 0.5f);
        }

        private void UpdateCharge()
        {
            if (_rb == null) return;

            _chargeTimer -= Time.fixedDeltaTime;

            // First 0.2s: wind-up (slow down)
            if (_chargeTimer > CHARGE_DURATION - 0.2f)
            {
                _rb.linearVelocity = new Vector2(_direction * _moveSpeed * 0.3f, _rb.linearVelocity.y);
            }
            // Rest: fast charge
            else
            {
                _rb.linearVelocity = new Vector2(_direction * _chargeSpeed, _rb.linearVelocity.y);

                // Check arena bounds
                float posX = transform.position.x;
                if (posX <= _arenaMinX + 1 || posX >= _arenaMaxX - 1)
                {
                    EndCharge();
                }
            }

            if (_chargeTimer <= 0f)
            {
                EndCharge();
            }

            if (_sr != null)
                _sr.flipX = _direction < 0;
        }

        private void EndCharge()
        {
            _isCharging = false;
            if (_sr != null)
            {
                float tint = 1f - (_currentPhase - 1) * 0.15f;
                _sr.color = new Color(1f, tint, tint);
            }
        }

        private void ShootAtPlayer()
        {
            if (_playerTransform == null) return;

            Vector2 dir = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
            SpawnProjectile(dir);
        }

        private void ShootSpread()
        {
            if (_playerTransform == null) return;

            Vector2 baseDir = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;

            // Shoot 3 projectiles in a spread
            float spreadAngle = 15f * Mathf.Deg2Rad;
            SpawnProjectile(baseDir);
            SpawnProjectile(RotateVector(baseDir, spreadAngle));
            SpawnProjectile(RotateVector(baseDir, -spreadAngle));
        }

        private Vector2 RotateVector(Vector2 v, float angle)
        {
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        private void SpawnProjectile(Vector2 direction)
        {
            Vector3 spawnPos = transform.position + (Vector3)(direction * 1.5f);

            var projGO = new GameObject("BossProjectile");
            projGO.transform.position = spawnPos;
            projGO.layer = LayerMask.NameToLayer("Default");

            var sr = projGO.AddComponent<SpriteRenderer>();
            int era = (int)Type;
            sr.sprite = PlaceholderAssets.GetProjectileSprite(WeaponTier.Heavy, era);
            sr.color = new Color(1f, 0.4f, 0.9f); // Purple-pink tint for boss
            sr.sortingOrder = 11;
            projGO.transform.localScale = new Vector3(1.5f, 1.5f, 1f); // Larger projectiles

            var rb = projGO.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            var col = projGO.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            col.isTrigger = true;

            var proj = projGO.AddComponent<Projectile>();
            proj.Initialize(direction, 5f, 2, true); // 2 damage for boss projectiles
        }

        public void TakeDamage(int amount)
        {
            // Boss is immune to damage until activated
            if (IsDead || !IsActive) return;

            Health -= amount;

            AudioManager.PlaySFX(PlaceholderAudio.GetEnemyHitSFX());

            // Flash white
            if (_sr != null)
                _sr.color = Color.white;
            Invoke(nameof(ResetColor), 0.1f);

            if (Health <= 0)
            {
                Die();
            }
        }

        private void ResetColor()
        {
            if (_sr != null)
            {
                float tint = 1f - (_currentPhase - 1) * 0.15f;
                _sr.color = new Color(1f, tint, tint);
            }
        }

        private void Die()
        {
            IsDead = true;

            if (GameManager.Instance != null)
            {
                // Record boss kill (counts as multiple kills for achievements)
                for (int i = 0; i < 5; i++)
                    GameManager.Instance.RecordEnemyKill();
            }

            // Epic boss death sound
            AudioManager.PlaySFX(PlaceholderAudio.GetBossDeathSFX());

            // Stop movement
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.gravityScale = 0f;
            }

            // Start dramatic death animation
            StartCoroutine(DeathAnimation());
        }

        private System.Collections.IEnumerator DeathAnimation()
        {
            if (_sr == null)
            {
                Destroy(gameObject, 0.1f);
                yield break;
            }

            float duration = 1.5f;
            float elapsed = 0f;
            Vector3 originalScale = transform.localScale;
            Vector3 originalPos = transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Shake violently
                float shake = (1f - t) * 0.3f;
                transform.position = originalPos + new Vector3(
                    Random.Range(-shake, shake),
                    Random.Range(-shake, shake),
                    0f
                );

                // Flash between white and red
                float flash = Mathf.PingPong(elapsed * 15f, 1f);
                _sr.color = Color.Lerp(Color.white, new Color(1f, 0.2f, 0.2f), flash);

                // Grow slightly then shrink
                float scaleT = t < 0.3f ? 1f + t * 0.5f : Mathf.Lerp(1.15f, 0f, (t - 0.3f) / 0.7f);
                transform.localScale = originalScale * scaleT;

                // Spin faster as it dies
                transform.Rotate(0f, 0f, 360f * t * 2f * Time.deltaTime);

                yield return null;
            }

            Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Boss damages player on contact
            if (collision.gameObject.CompareTag("Player"))
            {
                var health = collision.gameObject.GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.TakeDamage(2, transform.position); // Boss does 2 damage on contact
                }
            }
            // Reverse direction when hitting a wall/obstacle (prevents getting stuck)
            else if (!collision.gameObject.CompareTag("Player"))
            {
                _direction = -_direction;
                if (_isCharging) EndCharge();
            }
        }
    }
}
