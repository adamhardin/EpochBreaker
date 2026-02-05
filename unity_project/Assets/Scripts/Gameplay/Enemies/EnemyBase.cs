using UnityEngine;
using SixteenBit.Generative;

namespace SixteenBit.Gameplay
{
    /// <summary>
    /// Base enemy component. Handles health, behavior AI (patrol, chase,
    /// stationary, flying), and combat interactions.
    /// </summary>
    public class EnemyBase : MonoBehaviour
    {
        public EnemyType Type { get; private set; }
        public EnemyBehavior Behavior { get; private set; }
        public int Health { get; private set; } = 3;
        public bool IsDead { get; private set; }

        private int _patrolMinX;
        private int _patrolMaxX;
        private float _moveSpeed = 2f;
        private int _direction = 1;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private LevelRenderer _tilemapRenderer;
        private Transform _playerTransform;

        // Chase behavior
        private float _chaseRange = 8f;
        private float _chaseSpeed = 3.5f;

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

            // Scale health with difficulty weight
            Health = Mathf.Max(1, Mathf.RoundToInt(data.DifficultyWeight));

            // Shooter enemies are slower but shoot
            if (Behavior == EnemyBehavior.Stationary)
                _moveSpeed = 0f;

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
            _playerTransform = GameObject.FindWithTag("Player")?.transform;
        }

        private void FixedUpdate()
        {
            if (IsDead || GameManager.Instance.CurrentState != GameState.Playing)
            {
                if (_rb != null) _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                return;
            }

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

            // Shoot at player
            _shootTimer -= Time.fixedDeltaTime;
            if (_shootTimer <= 0f && _playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, _playerTransform.position);
                if (dist < _shootRange)
                {
                    ShootAtPlayer();
                    _shootTimer = _shootCooldown;
                }
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
        }

        private void ShootAtPlayer()
        {
            if (_playerTransform == null) return;

            Vector2 dir = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
            Vector3 spawnPos = transform.position + (Vector3)(dir * 1.0f);

            var projGO = new GameObject("EnemyProjectile");
            projGO.transform.position = spawnPos;
            projGO.layer = LayerMask.NameToLayer("Default");

            var sr = projGO.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderAssets.GetProjectileSprite();
            sr.color = Color.red;
            sr.sortingOrder = 11;

            var rb = projGO.AddComponent<Rigidbody2D>();
            rb.isKinematic = true;
            rb.gravityScale = 0f;

            var col = projGO.AddComponent<CircleCollider2D>();
            col.radius = 0.2f;
            col.isTrigger = true;

            var proj = projGO.AddComponent<Projectile>();
            proj.Initialize(dir, 6f, 1, true);
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;

            Health -= amount;

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
                _sr.color = Color.white;
        }

        private void Die()
        {
            IsDead = true;

            // Add score
            if (GameManager.Instance != null)
                GameManager.Instance.Score += 100;

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
    }
}
