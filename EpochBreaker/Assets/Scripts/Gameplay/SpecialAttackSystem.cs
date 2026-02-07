using UnityEngine;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Special Attack: Hold jump 0.5s while grounded with 3+ weapons.
    /// Era-specific devastating screen-clearing attack.
    /// </summary>
    public class SpecialAttackSystem : MonoBehaviour
    {
        private PlayerController _player;
        private WeaponSystem _weapons;
        private HealthSystem _health;

        private float _holdTimer;
        private float _cooldownTimer;
        private bool _isCharging;
        private bool _isAttacking;

        private const float HOLD_THRESHOLD = 0.5f;
        private const float BASE_COOLDOWN = 15f;
        private const float COOLDOWN_REDUCTION_PER_WEAPON = 2f;
        private const int MIN_WEAPONS = 3;
        private const float ATTACK_DURATION = 1.0f;

        public bool IsReady => _cooldownTimer <= 0f && !_isAttacking && CanActivate;
        public bool IsCharging => _isCharging;
        public bool IsAttacking => _isAttacking;
        public float CooldownRatio => _cooldownTimer > 0f ? _cooldownTimer / GetCooldown() : 0f;
        public float ChargeRatio => _isCharging ? Mathf.Clamp01(_holdTimer / HOLD_THRESHOLD) : 0f;

        private bool CanActivate
        {
            get
            {
                if (_player == null || _weapons == null) return false;
                return _player.IsGrounded && _weapons.AcquiredWeaponCount >= MIN_WEAPONS;
            }
        }

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            _weapons = GetComponent<WeaponSystem>();
            _health = GetComponent<HealthSystem>();
        }

        private void Update()
        {
            if (_player == null || !_player.IsAlive) return;
            if (GameManager.Instance?.CurrentState != GameState.Playing) return;

            // Cooldown tick
            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;

            if (_isAttacking) return;

            // Charge detection: jump held while grounded with enough weapons
            if (InputManager.JumpHeld && _player.IsGrounded &&
                _cooldownTimer <= 0f && _weapons.AcquiredWeaponCount >= MIN_WEAPONS)
            {
                _holdTimer += Time.deltaTime;
                _isCharging = true;

                if (_holdTimer >= HOLD_THRESHOLD)
                {
                    ActivateSpecialAttack();
                }
            }
            else
            {
                _holdTimer = 0f;
                _isCharging = false;
            }
        }

        private float GetCooldown()
        {
            int extraWeapons = Mathf.Max(0, (_weapons?.AcquiredWeaponCount ?? 0) - MIN_WEAPONS);
            return Mathf.Max(5f, BASE_COOLDOWN - extraWeapons * COOLDOWN_REDUCTION_PER_WEAPON);
        }

        private void ActivateSpecialAttack()
        {
            _isAttacking = true;
            _isCharging = false;
            _holdTimer = 0f;

            // Grant invulnerability
            if (_health != null)
                _health.GrantSpawnProtection();

            int epoch = GameManager.Instance?.CurrentEpoch ?? 0;

            // Screen effects
            Color eraColor = PlaceholderAssets.GetEraAccentColor(epoch);
            ScreenFlash.Flash(eraColor, 0.8f);
            CameraController.Instance?.AddTrauma(0.8f);
            GameManager.HitStop(0.2f);

            // Deal damage to all enemies on screen
            float damage = GetDamage();
            DealAreaDamage(damage);

            // Era-specific visual effect
            SpawnEraEffect(epoch);

            // SFX
            AudioManager.PlaySFX(PlaceholderAudio.GetLevelCompleteSFX());

            // End attack after duration
            Invoke(nameof(EndAttack), ATTACK_DURATION);
        }

        private float GetDamage()
        {
            int weaponCount = _weapons?.AcquiredWeaponCount ?? MIN_WEAPONS;
            float multiplier = 1f + (weaponCount - MIN_WEAPONS) * 0.5f;
            return 10f * multiplier; // Base 10 damage, scaled
        }

        private void DealAreaDamage(float damage)
        {
            int dmg = Mathf.RoundToInt(damage);

            // Damage all active enemies
            var enemies = EnemyBase.ActiveEnemies;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (i >= enemies.Count) continue;
                var enemyGO = enemies[i];
                if (enemyGO == null) continue;

                var enemy = enemyGO.GetComponent<EnemyBase>();
                if (enemy != null && !enemy.IsDead)
                    enemy.TakeDamage(dmg);

                var boss = enemyGO.GetComponent<Boss>();
                if (boss != null && !boss.IsDead)
                    boss.TakeDamage(dmg);
            }
        }

        private void SpawnEraEffect(int epoch)
        {
            Color eraColor = PlaceholderAssets.GetEraAccentColor(epoch);
            Vector3 center = transform.position;

            // Radial burst of particles
            int count = 20;
            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * Mathf.PI * 2f;
                var go = ObjectPool.GetParticle();
                go.transform.position = center;

                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetParticleSprite();
                sr.color = new Color(eraColor.r, eraColor.g, eraColor.b, 0.9f);
                sr.sortingOrder = 20;
                go.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);

                var rb = go.GetComponent<Rigidbody2D>();
                rb.gravityScale = 0.5f;
                float speed = Random.Range(8f, 15f);
                rb.linearVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;

                go.GetComponent<PoolTimer>().StartTimer(0.8f);
            }
        }

        private void EndAttack()
        {
            _isAttacking = false;
            _cooldownTimer = GetCooldown();
        }
    }
}
