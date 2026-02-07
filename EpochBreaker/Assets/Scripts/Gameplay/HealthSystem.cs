using System;
using UnityEngine;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Player health system. 5-heart max, 60-frame i-frames,
    /// knockback on damage, death triggers respawn.
    /// </summary>
    public class HealthSystem : MonoBehaviour
    {
        public int MaxHealth { get; private set; } = 5;
        public int CurrentHealth { get; private set; }
        public bool IsInvulnerable { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        public event Action<int, int> OnHealthChanged; // (current, max)
        public event Action OnDeath;
        public event Action OnDamage;

        private float _iFrameFlashAccum;

        private float _invulnerabilityTimer;
        private const float INVULNERABILITY_DURATION = 1f; // 60 frames at 60fps
        private const float KNOCKBACK_FORCE = 8f;
        private const float SPAWN_PROTECTION_DURATION = 2f;

        private PlayerController _player;
        private SpriteRenderer _spriteRenderer;
        private bool _deathProcessing;

        private void Awake()
        {
            CurrentHealth = MaxHealth;
            _player = GetComponent<PlayerController>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        private void OnDisable()
        {
            CancelInvoke(); // Cancel pending Respawn/HandleLevelFailed if destroyed mid-death
            _deathProcessing = false;
        }

        private void Update()
        {
            if (IsInvulnerable)
            {
                _invulnerabilityTimer -= Time.deltaTime;
                if (_invulnerabilityTimer <= 0f)
                {
                    IsInvulnerable = false;
                    if (_spriteRenderer != null)
                        _spriteRenderer.color = Color.white;
                }
                else
                {
                    // Flash effect during i-frames (local accumulator for consistent phase)
                    _iFrameFlashAccum += Time.deltaTime * 10f;
                    if (_spriteRenderer != null)
                    {
                        float flash = Mathf.PingPong(_iFrameFlashAccum, 1f);
                        _spriteRenderer.color = new Color(1f, 1f, 1f, flash > 0.5f ? 1f : 0.3f);
                    }
                }
            }

            // Kill plane: respawn if player falls below the level
            if (_player != null && _player.IsAlive && transform.position.y < -5f)
            {
                Die();
            }
        }

        public void TakeDamage(int amount, Vector2 damageSource)
        {
            if (IsInvulnerable || IsDead) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            OnDamage?.Invoke();
            AudioManager.PlaySFX(PlaceholderAudio.GetPlayerHurtSFX());

            // Screen shake on player damage (high trauma)
            CameraController.Instance?.AddTrauma(0.4f);

            // Brief white flash before i-frame flashing
            if (_spriteRenderer != null)
                _spriteRenderer.color = Color.white;

            // Notify achievement system that player took damage
            GameManager.Instance?.RecordPlayerDamage();

            // Notify UI for damage direction indicator
            GameManager.NotifyDamageDirection(damageSource);

            // Knockback away from damage source
            if (_player != null)
            {
                Vector2 knockbackDir = ((Vector2)transform.position - damageSource).normalized;
                if (knockbackDir.sqrMagnitude < 0.01f)
                    knockbackDir = Vector2.up;
                _player.ApplyKnockback(knockbackDir, KNOCKBACK_FORCE);
            }

            // Start i-frames
            IsInvulnerable = true;
            _invulnerabilityTimer = INVULNERABILITY_DURATION;
            _iFrameFlashAccum = 0f;

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void ResetHealth()
        {
            CurrentHealth = MaxHealth;
            IsInvulnerable = false;
            _deathProcessing = false;
            if (_spriteRenderer != null)
                _spriteRenderer.color = Color.white;
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        private void Die()
        {
            if (_deathProcessing) return;
            _deathProcessing = true;

            OnDeath?.Invoke();

            // Play death sound + dramatic feedback
            AudioManager.PlaySFX(PlaceholderAudio.GetPlayerDeathSFX());
            CameraController.Instance?.AddTrauma(0.5f);
            GameManager.HitStop(0.1f); // 100ms freeze frame on death

            if (_player != null)
                _player.IsAlive = false;

            // Start death animation
            StartCoroutine(DeathAnimation());

            // Check death result
            var result = GameManager.Instance?.LoseLife() ?? DeathResult.Respawn;
            switch (result)
            {
                case DeathResult.Respawn:
                    Invoke(nameof(Respawn), 1.5f);
                    break;
                case DeathResult.LevelFailed:
                    // Level failed — advance to next level after a delay
                    Invoke(nameof(HandleLevelFailed), 2f);
                    break;
                case DeathResult.GameOver:
                    // Game over — already handled by GameManager.LoseLife() transitioning state
                    break;
            }
        }

        private void HandleLevelFailed()
        {
            GameManager.Instance?.AdvanceAfterFail();
        }

        private System.Collections.IEnumerator DeathAnimation()
        {
            if (_spriteRenderer == null) yield break;

            float duration = 1.0f;
            float elapsed = 0f;
            Vector3 originalScale = transform.localScale;
            Color originalColor = _spriteRenderer.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Fade out and shrink
                float alpha = Mathf.Lerp(1f, 0f, t);
                float scale = Mathf.Lerp(1f, 0.3f, t * t); // ease-in shrink

                _spriteRenderer.color = new Color(1f, 0.3f, 0.3f, alpha); // Red tint
                transform.localScale = originalScale * scale;

                // Slower spin (360 deg/s, was 720)
                transform.Rotate(0f, 0f, 360f * Time.deltaTime);

                yield return null;
            }

            // Reset for respawn
            _spriteRenderer.color = originalColor;
            transform.localScale = originalScale;
            transform.rotation = Quaternion.identity;
        }

        private void Respawn()
        {
            var checkpoint = CheckpointManager.Instance;
            if (checkpoint != null && _player != null)
            {
                _player.TeleportTo(checkpoint.CurrentRespawnPoint);
                _player.IsAlive = true;
                ResetHealth();
                GrantSpawnProtection();

                // Respawn flash
                ScreenFlash.Flash(Color.white, 0.3f);

                // Reset boss if player respawns in boss arena
                var loader = GameManager.Instance?.GetComponent<LevelLoader>();
                if (loader != null && loader.CurrentBoss != null)
                {
                    loader.CurrentBoss.ResetBoss();
                    loader.RespawnArenaPillars();

                    // Re-enable arena trigger so boss reactivates on re-entry
                    var trigger = FindAnyObjectByType<BossArenaTrigger>();
                    trigger?.ResetTrigger();
                }
            }
        }

        /// <summary>
        /// Grant temporary invulnerability after spawning or respawning.
        /// </summary>
        public void GrantSpawnProtection()
        {
            IsInvulnerable = true;
            _invulnerabilityTimer = SPAWN_PROTECTION_DURATION;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                TakeDamage(1, collision.transform.position);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Enemy") && !IsInvulnerable)
            {
                TakeDamage(1, collision.transform.position);
            }
        }
    }
}
