using UnityEngine;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Manages unlockable movement abilities: Double Jump, Air Dash,
    /// Ground Slam (epoch 5+), and Phase Shift (epoch 7+).
    /// Abilities are granted via pickups during gameplay and persist
    /// for the current level only (reset on level load).
    /// Each ability has an enhanced tier that doubles its effectiveness.
    /// </summary>
    public class AbilitySystem : MonoBehaviour
    {
        public bool HasDoubleJump { get; private set; }
        public bool HasAirDash { get; private set; }
        public bool HasGroundSlam { get; private set; }
        public bool HasPhaseShift { get; private set; }

        private bool _doubleJumpUsed;
        private bool _dashUsed;
        private float _dashTimer;
        private float _dashCooldownTimer;
        private float _phaseShiftCooldown;

        private const float DASH_SPEED = 18f;
        private const float DASH_DURATION = 0.12f;
        private const float DASH_COOLDOWN = 0.8f;
        private const float PHASE_SHIFT_COOLDOWN = 1.5f;
        private const float GROUND_SLAM_RADIUS = 3f;
        private const int GROUND_SLAM_DAMAGE = 2;

        // Upgrade tiers
        public bool DoubleJumpEnhanced { get; private set; }
        public bool AirDashEnhanced { get; private set; }
        public bool GroundSlamEnhanced { get; private set; }
        public bool PhaseShiftEnhanced { get; private set; }

        private PlayerController _player;
        private Rigidbody2D _rb;

        public bool IsDashing => _dashTimer > 0f;
        public bool DoubleJumpAvailable => HasDoubleJump && !_doubleJumpUsed;
        public bool AirDashAvailable => HasAirDash && !_dashUsed && _dashCooldownTimer <= 0f;
        public bool PhaseShiftAvailable => HasPhaseShift && _phaseShiftCooldown <= 0f;
        public float DashCooldownRatio => _dashCooldownTimer > 0f ? _dashCooldownTimer / DASH_COOLDOWN : 0f;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            _rb = GetComponent<Rigidbody2D>();
        }

        public void GrantDoubleJump()
        {
            HasDoubleJump = true;
        }

        public void GrantAirDash()
        {
            HasAirDash = true;
        }

        public void GrantGroundSlam()
        {
            HasGroundSlam = true;
        }

        public void GrantPhaseShift()
        {
            HasPhaseShift = true;
        }

        public void EnhanceAbility(AbilityType type)
        {
            switch (type)
            {
                case AbilityType.DoubleJump: DoubleJumpEnhanced = true; break;
                case AbilityType.AirDash: AirDashEnhanced = true; break;
                case AbilityType.GroundSlam: GroundSlamEnhanced = true; break;
                case AbilityType.PhaseShift: PhaseShiftEnhanced = true; break;
            }
        }

        public void ResetAbilities()
        {
            HasDoubleJump = false;
            HasAirDash = false;
            HasGroundSlam = false;
            HasPhaseShift = false;
            DoubleJumpEnhanced = false;
            AirDashEnhanced = false;
            GroundSlamEnhanced = false;
            PhaseShiftEnhanced = false;
            _doubleJumpUsed = false;
            _dashUsed = false;
            _dashTimer = 0f;
            _dashCooldownTimer = 0f;
            _phaseShiftCooldown = 0f;
        }

        private void Update()
        {
            if (_player == null || !_player.IsAlive) return;
            if (GameManager.Instance?.CurrentState != GameState.Playing) return;

            // Reset double jump and dash when grounded or wall-sliding
            if (_player.IsGrounded || _player.IsWallSliding)
            {
                _doubleJumpUsed = false;
                _dashUsed = false;
            }

            // Dash cooldown
            if (_dashCooldownTimer > 0f)
                _dashCooldownTimer -= Time.deltaTime;

            // Phase shift cooldown
            if (_phaseShiftCooldown > 0f)
                _phaseShiftCooldown -= Time.deltaTime;

            // Active dash
            if (_dashTimer > 0f)
            {
                _dashTimer -= Time.deltaTime;
                if (_dashTimer <= 0f)
                {
                    _dashCooldownTimer = DASH_COOLDOWN;

                    // If touching wall at end of dash, auto-enter wall slide
                    if (_player.IsTouchingWall)
                        _player.ForceWallSlide();
                }
            }
        }

        /// <summary>
        /// Attempt a double jump. Returns true if successful.
        /// Called by PlayerController when jump is pressed in mid-air.
        /// </summary>
        public bool TryDoubleJump()
        {
            if (!HasDoubleJump || _doubleJumpUsed) return false;
            _doubleJumpUsed = true;
            AudioManager.PlaySFX(PlaceholderAudio.GetJumpSFX());
            SpawnDoubleJumpEffect();
            return true;
        }

        /// <summary>
        /// Reset double-jump availability (called on stomp chain bounce).
        /// </summary>
        public void ResetDoubleJump()
        {
            _doubleJumpUsed = false;
        }

        /// <summary>
        /// Attempt an air dash. Returns true if successful.
        /// Called by PlayerController when attack is pressed without weapon cycling.
        /// </summary>
        public bool TryAirDash(bool facingRight)
        {
            if (!HasAirDash || _dashUsed || _dashCooldownTimer > 0f) return false;
            if (_player.IsGrounded) return false;

            _dashUsed = true;
            _dashTimer = DASH_DURATION;

            float dir = facingRight ? 1f : -1f;
            _rb.linearVelocity = new Vector2(dir * DASH_SPEED, 0f);

            SpawnDashEffect(facingRight);
            return true;
        }

        private void SpawnDoubleJumpEffect()
        {
            var go = new GameObject("DoubleJumpFX");
            go.transform.position = transform.position + Vector3.down * 0.5f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderAssets.GetParticleSprite();
            sr.color = new Color(0.8f, 0.9f, 1f, 0.7f);
            sr.sortingOrder = 5;
            go.transform.localScale = Vector3.one * 0.8f;
            Destroy(go, 0.15f);
        }

        private void SpawnDashEffect(bool facingRight)
        {
            // Trail of afterimages
            for (int i = 0; i < 3; i++)
            {
                var go = new GameObject("DashTrail");
                float offset = (facingRight ? -1f : 1f) * (i + 1) * 0.4f;
                go.transform.position = transform.position + new Vector3(offset, 0, 0);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetPlayerSprite();
                sr.color = new Color(0.5f, 0.8f, 1f, 0.3f - i * 0.08f);
                sr.sortingOrder = 4;
                Destroy(go, 0.15f);
            }
        }

        // =================================================================
        // Ground Slam (epoch 5+)
        // =================================================================

        /// <summary>
        /// Enhanced ground slam: 3-tile AoE on landing, destroys soft/medium blocks,
        /// 2 damage to enemies in radius. Visual: shockwave ring.
        /// Called by PlayerController when stomping with the ability.
        /// </summary>
        public void PerformGroundSlam()
        {
            if (!HasGroundSlam) return;

            float radius = GroundSlamEnhanced ? GROUND_SLAM_RADIUS * 2f : GROUND_SLAM_RADIUS;
            int damage = GroundSlamEnhanced ? GROUND_SLAM_DAMAGE * 2 : GROUND_SLAM_DAMAGE;

            AudioManager.PlaySFX(PlaceholderAudio.GetStompSFX());
            CameraController.Instance?.AddTrauma(0.4f);

            // Damage nearby enemies
            var enemies = EnemyBase.ActiveEnemies;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (i >= enemies.Count) continue;
                var enemyGO = enemies[i];
                if (enemyGO == null) continue;

                float dist = Vector2.Distance(transform.position, enemyGO.transform.position);
                if (dist <= radius)
                {
                    var enemy = enemyGO.GetComponent<EnemyBase>();
                    if (enemy != null && !enemy.IsDead)
                        enemy.TakeDamage(damage);
                }
            }

            // Destroy soft/medium blocks in radius
            var levelRenderer = Object.FindAnyObjectByType<LevelRenderer>();
            if (levelRenderer != null)
            {
                Vector2Int center = levelRenderer.WorldToLevel(transform.position);
                int tileRadius = Mathf.CeilToInt(radius);
                for (int dx = -tileRadius; dx <= tileRadius; dx++)
                {
                    for (int dy = -tileRadius; dy <= tileRadius; dy++)
                    {
                        int tx = center.x + dx;
                        int ty = center.y + dy;
                        var tile = levelRenderer.GetDestructibleAt(tx, ty);
                        if (tile.MaterialClass == (byte)MaterialClass.Soft ||
                            tile.MaterialClass == (byte)MaterialClass.Medium)
                        {
                            levelRenderer.DestroyTile(tx, ty);
                        }
                    }
                }
            }

            // Visual: shockwave ring
            SpawnShockwaveEffect();
        }

        private void SpawnShockwaveEffect()
        {
            int count = 12;
            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * Mathf.PI * 2f;
                var go = ObjectPool.GetParticle();
                go.transform.position = transform.position + Vector3.down * 0.5f;
                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetParticleSprite();
                sr.color = new Color(1f, 0.6f, 0.2f, 0.8f);
                sr.sortingOrder = 12;
                go.transform.localScale = Vector3.one * 0.5f;

                var rb = go.GetComponent<Rigidbody2D>();
                rb.gravityScale = 0.5f;
                rb.linearVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.3f) * 6f;
                go.GetComponent<PoolTimer>().StartTimer(0.5f);
            }
        }

        // =================================================================
        // Phase Shift (epoch 7+)
        // =================================================================

        /// <summary>
        /// Phase Shift: dash through thin walls (1-2 tiles thick).
        /// Ghost trail effect during shift. Cannot phase through reinforced/indestructible.
        /// </summary>
        public bool TryPhaseShift(bool facingRight)
        {
            if (!HasPhaseShift || _phaseShiftCooldown > 0f) return false;
            if (_player.IsGrounded) return false; // Only works in air

            float shiftDistance = PhaseShiftEnhanced ? 4f : 2.5f;
            float dir = facingRight ? 1f : -1f;
            Vector3 targetPos = transform.position + new Vector3(dir * shiftDistance, 0f, 0f);

            // Check if there's a thin wall we're phasing through
            var levelRenderer = Object.FindAnyObjectByType<LevelRenderer>();
            if (levelRenderer != null)
            {
                // Check the target area isn't inside reinforced/indestructible
                // Player hitbox spans ~1 tile wide × 2 tiles tall
                Vector2Int targetTile = levelRenderer.WorldToLevel(targetPos);
                for (int dy = 0; dy <= 1; dy++)
                {
                    var tile = levelRenderer.GetDestructibleAt(targetTile.x, targetTile.y + dy);
                    if (tile.MaterialClass >= (byte)MaterialClass.Reinforced)
                        return false; // Can't phase through reinforced/indestructible
                }
            }

            // Perform the shift
            _phaseShiftCooldown = PHASE_SHIFT_COOLDOWN;
            transform.position = targetPos;
            _rb.linearVelocity = new Vector2(dir * 8f, _rb.linearVelocity.y);

            // Ghost trail effect
            SpawnPhaseShiftTrail(facingRight, shiftDistance);
            AudioManager.PlaySFX(PlaceholderAudio.GetChargeWindupSFX());

            return true;
        }

        private void SpawnPhaseShiftTrail(bool facingRight, float distance)
        {
            int trailCount = 5;
            float dir = facingRight ? -1f : 1f;

            for (int i = 0; i < trailCount; i++)
            {
                var go = ObjectPool.GetFlash();
                float offset = dir * (distance / trailCount) * (i + 1);
                go.transform.position = transform.position + new Vector3(offset, 0f, 0f);
                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetPlayerSprite();
                sr.color = new Color(0.3f, 0.9f, 1f, 0.3f - i * 0.05f);
                sr.sortingOrder = 9;
                go.transform.localScale = Vector3.one;
                go.GetComponent<PoolTimer>().StartTimer(0.2f);
            }
        }
    }
}
