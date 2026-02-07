using UnityEngine;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Manages unlockable movement abilities: Double Jump and Air Dash.
    /// Abilities are granted via pickups during gameplay and persist
    /// for the current level only (reset on level load).
    /// </summary>
    public class AbilitySystem : MonoBehaviour
    {
        public bool HasDoubleJump { get; private set; }
        public bool HasAirDash { get; private set; }

        private bool _doubleJumpUsed;
        private bool _dashUsed;
        private float _dashTimer;
        private float _dashCooldownTimer;

        private const float DASH_SPEED = 18f;
        private const float DASH_DURATION = 0.12f;
        private const float DASH_COOLDOWN = 0.8f;

        private PlayerController _player;
        private Rigidbody2D _rb;

        public bool IsDashing => _dashTimer > 0f;
        public bool DoubleJumpAvailable => HasDoubleJump && !_doubleJumpUsed;
        public bool AirDashAvailable => HasAirDash && !_dashUsed && _dashCooldownTimer <= 0f;
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

        public void ResetAbilities()
        {
            HasDoubleJump = false;
            HasAirDash = false;
            _doubleJumpUsed = false;
            _dashUsed = false;
            _dashTimer = 0f;
            _dashCooldownTimer = 0f;
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

            // Active dash
            if (_dashTimer > 0f)
            {
                _dashTimer -= Time.deltaTime;
                if (_dashTimer <= 0f)
                {
                    _dashCooldownTimer = DASH_COOLDOWN;
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
    }
}
