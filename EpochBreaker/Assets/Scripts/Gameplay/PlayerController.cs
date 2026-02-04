using UnityEngine;

namespace SixteenBit.Gameplay
{
    /// <summary>
    /// Player movement controller with platformer physics.
    /// Spec: 6 tiles/s max speed, 4-tile jump height, coyote time,
    /// jump buffer, asymmetric gravity.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        // Movement
        private const float MOVE_SPEED = 6f;
        private const float ACCELERATION = 40f;
        private const float DECELERATION = 40f;

        // Jump physics
        private const float JUMP_VELOCITY = 12f;
        private const float GRAVITY_UP = 28f;       // slower rising
        private const float GRAVITY_DOWN = 42f;      // faster falling
        private const float MAX_FALL_SPEED = 20f;
        private const int COYOTE_FRAMES = 5;
        private const int JUMP_BUFFER_FRAMES = 6;

        // Ground detection
        private const float GROUND_CHECK_DISTANCE = 0.1f;

        private Rigidbody2D _rb;
        private BoxCollider2D _collider;
        private SpriteRenderer _spriteRenderer;

        private Vector2 _velocity;
        private bool _isGrounded;
        private int _coyoteTimer;
        private int _jumpBufferTimer;
        private bool _facingRight = true;
        private bool _jumpCut;

        public bool IsGrounded => _isGrounded;
        public bool IsAlive { get; set; } = true;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rb.gravityScale = 0f; // We handle gravity manually
        }

        private void Update()
        {
            if (!IsAlive || GameManager.Instance.CurrentState != GameState.Playing)
                return;

            CheckGround();
            HandleJumpBuffer();
        }

        private void FixedUpdate()
        {
            if (!IsAlive || GameManager.Instance.CurrentState != GameState.Playing)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            HandleMovement();
            HandleJump();
            ApplyGravity();
            ApplyVelocity();
        }

        private void CheckGround()
        {
            Vector2 origin = (Vector2)transform.position + _collider.offset;
            float castDist = (_collider.size.y * 0.5f) + GROUND_CHECK_DISTANCE;

            // Cast a thin box downward, ignoring our own collider
            var filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("Default"));
            filter.useLayerMask = true;

            RaycastHit2D[] results = new RaycastHit2D[5];
            int count = Physics2D.BoxCast(
                origin, new Vector2(_collider.size.x * 0.9f, 0.05f), 0f, Vector2.down,
                filter, results, castDist
            );

            bool wasGrounded = _isGrounded;
            _isGrounded = false;
            for (int i = 0; i < count; i++)
            {
                if (results[i].collider != null && results[i].collider != _collider &&
                    !results[i].collider.isTrigger && results[i].normal.y > 0.5f)
                {
                    _isGrounded = true;
                    break;
                }
            }

            if (_isGrounded)
            {
                _coyoteTimer = COYOTE_FRAMES;
            }
            else if (wasGrounded)
            {
                // Start coyote time
            }
        }

        private void HandleMovement()
        {
            float input = InputManager.MoveX;
            float targetSpeed = input * MOVE_SPEED;

            float accel = Mathf.Abs(targetSpeed) > 0.01f ? ACCELERATION : DECELERATION;
            _velocity.x = Mathf.MoveTowards(_velocity.x, targetSpeed, accel * Time.fixedDeltaTime);

            // Flip sprite
            if (input > 0.01f && !_facingRight)
            {
                _facingRight = true;
                _spriteRenderer.flipX = false;
            }
            else if (input < -0.01f && _facingRight)
            {
                _facingRight = false;
                _spriteRenderer.flipX = true;
            }
        }

        private void HandleJumpBuffer()
        {
            if (InputManager.JumpPressed)
                _jumpBufferTimer = JUMP_BUFFER_FRAMES;
        }

        private void HandleJump()
        {
            // Coyote timer countdown
            if (!_isGrounded && _coyoteTimer > 0)
                _coyoteTimer--;

            // Jump buffer countdown
            if (_jumpBufferTimer > 0)
                _jumpBufferTimer--;

            // Can jump: grounded or within coyote time, and jump buffered
            bool canJump = (_isGrounded || _coyoteTimer > 0) && _jumpBufferTimer > 0;

            if (canJump)
            {
                _velocity.y = JUMP_VELOCITY;
                _coyoteTimer = 0;
                _jumpBufferTimer = 0;
                _jumpCut = false;
            }

            // Variable jump height: cut jump short ONCE when button released
            if (!InputManager.JumpHeld && _velocity.y > 0 && !_jumpCut)
            {
                _velocity.y *= 0.5f;
                _jumpCut = true;
            }
        }

        private void ApplyGravity()
        {
            float gravity = _velocity.y > 0 ? GRAVITY_UP : GRAVITY_DOWN;
            _velocity.y -= gravity * Time.fixedDeltaTime;

            if (_velocity.y < -MAX_FALL_SPEED)
                _velocity.y = -MAX_FALL_SPEED;

            // Reset Y velocity when grounded and not jumping
            if (_isGrounded && _velocity.y < 0)
                _velocity.y = -1f; // Small downward force to maintain ground contact
        }

        private void ApplyVelocity()
        {
            _rb.linearVelocity = _velocity;
        }

        /// <summary>
        /// Apply knockback force (called by HealthSystem on damage).
        /// </summary>
        public void ApplyKnockback(Vector2 direction, float force = 8f)
        {
            _velocity = direction.normalized * force;
            _velocity.y = Mathf.Max(_velocity.y, 4f); // Always bounce up a bit
        }

        /// <summary>
        /// Teleport player to position (used for respawn).
        /// </summary>
        public void TeleportTo(Vector3 position)
        {
            transform.position = position;
            _velocity = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;
        }
    }
}
