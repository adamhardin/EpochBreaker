using UnityEngine;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Player movement controller with platformer physics.
    /// Spec: 6 tiles/s max speed, 4-tile jump height, coyote time,
    /// jump buffer, asymmetric gravity, wall-slide, wall-jump.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
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

        // Ground pound
        private const float STOMP_SPEED = 30f;
        private const float STOMP_STALL_DURATION = 0.05f; // 50ms air-stall before descent
        private const float STOMP_BOUNCE = 8f;             // normal bounce off solid ground
        private const float STOMP_CHAIN_BOUNCE = 16f;      // boosted bounce on enemy/tile hit

        // Ground detection
        private const float GROUND_CHECK_DISTANCE = 0.1f;

        // Wall interaction
        private const float WALL_CHECK_DISTANCE = 0.15f;
        private const float WALL_SLIDE_SPEED = 4f;
        private const float WALL_SLIDE_GRAVITY = 12f;
        private const float WALL_JUMP_VELOCITY_X = 5f;
        private const float WALL_JUMP_VELOCITY_Y = 12f;
        private const int WALL_JUMP_LOCK_FRAMES = 5;
        private const int WALL_COYOTE_FRAMES = 4;
        private const float WALL_DUST_INTERVAL = 0.08f;

        private Rigidbody2D _rb;
        private CapsuleCollider2D _collider;
        private SpriteRenderer _spriteRenderer;
        private WeaponSystem _weaponSystem;
        private LevelRenderer _levelRenderer;

        // Cached physics query buffers (avoid per-frame allocation)
        private readonly RaycastHit2D[] _groundResults = new RaycastHit2D[5];
        private readonly RaycastHit2D[] _wallHits = new RaycastHit2D[3];
        private ContactFilter2D _physicsFilter;
        private bool _filterInitialized;

        private Vector2 _velocity;
        private bool _isGrounded;
        private int _coyoteTimer;
        private int _jumpBufferTimer;
        private bool _facingRight = true;
        private bool _jumpCut;
        private bool _isStomping;
        private float _stompStallTimer;

        // Wall state
        private bool _isTouchingWallLeft;
        private bool _isTouchingWallRight;
        private bool _isWallSliding;
        private bool _forceWallSlide;     // air dash → wall slide auto-stick
        private int _wallSlideSide;       // -1 = left wall, 1 = right wall, 0 = none
        private int _wallJumpLockTimer;
        private int _wallCoyoteTimer;
        private int _wallJumpsThisLevel;
        private float _wallDustTimer;
        private AudioSource _wallSlideAudioSource;

        // Squash/stretch visual feedback
        private float _squashTimer;
        private Vector3 _targetScale = Vector3.one;

        // Sprite animation
        private SpriteAnimator _animator;

        public bool IsGrounded => _isGrounded;
        public bool FacingRight => _facingRight;
        public bool IsStomping => _isStomping;
        public bool IsWallSliding => _isWallSliding;
        public bool IsTouchingWall => _isTouchingWallLeft || _isTouchingWallRight;
        public bool IsAlive { get; set; } = true;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<CapsuleCollider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _weaponSystem = GetComponent<WeaponSystem>();
            _rb.gravityScale = 0f; // We handle gravity manually

            // Set up sprite animations
            var animator = gameObject.AddComponent<SpriteAnimator>();
            var idle = new Sprite[] { PlaceholderAssets.GetPlayerSprite() };
            var walk = PlaceholderAssets.GetPlayerWalkFrames();
            var jump = PlaceholderAssets.GetPlayerJumpFrame();
            var fall = PlaceholderAssets.GetPlayerFallFrame();
            var wallSlide = PlaceholderAssets.GetPlayerWallSlideFrames();
            animator.SetAnimations(idle, walk, jump, fall, wallSlide);
            _animator = animator;
        }

        private void Update()
        {
            if (!IsAlive || GameManager.Instance.CurrentState != GameState.Playing)
                return;

            CheckGround();
            CheckWalls();
            HandleJumpBuffer();
            HandleStompInput();
        }

        private void FixedUpdate()
        {
            if (!IsAlive || GameManager.Instance.CurrentState != GameState.Playing)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            if (_isStomping)
            {
                // Air-stall phase: brief hover before plummeting
                if (_stompStallTimer > 0f)
                {
                    _stompStallTimer -= Time.fixedDeltaTime;
                    _velocity = Vector2.zero;
                    ApplyVelocity();
                    return;
                }
                // During stomp: override velocity, no horizontal control
                _velocity.x = 0f;
                _velocity.y = -STOMP_SPEED;
                ApplyVelocity();
                return;
            }

            HandleMovement();
            HandleWallSlide();
            HandleJump();
            ApplyGravity();
            ApplyVelocity();
            UpdateWallSlideVisuals();
            UpdateSquashStretch();
            UpdateAnimationState();
        }

        private void EnsureFilterInitialized()
        {
            if (_filterInitialized) return;
            _physicsFilter = new ContactFilter2D();
            _physicsFilter.SetLayerMask(LayerMask.GetMask("Default"));
            _physicsFilter.useLayerMask = true;
            _filterInitialized = true;
        }

        private void CheckGround()
        {
            EnsureFilterInitialized();

            Vector2 origin = (Vector2)transform.position + _collider.offset;
            float castDist = (_collider.size.y * 0.5f) + GROUND_CHECK_DISTANCE;

            int count = Physics2D.BoxCast(
                origin, new Vector2(_collider.size.x * 0.9f, 0.05f), 0f, Vector2.down,
                _physicsFilter, _groundResults, castDist
            );

            bool wasGrounded = _isGrounded;
            _isGrounded = false;
            for (int i = 0; i < count; i++)
            {
                if (_groundResults[i].collider != null && _groundResults[i].collider != _collider &&
                    !_groundResults[i].collider.isTrigger && _groundResults[i].normal.y > 0.5f)
                {
                    _isGrounded = true;
                    break;
                }
            }

            if (_isGrounded)
            {
                _coyoteTimer = COYOTE_FRAMES;
                // Landing squash effect + dust particles
                if (!wasGrounded)
                {
                    _targetScale = new Vector3(1.2f, 0.8f, 1f);
                    _squashTimer = 0.1f;
                    SpawnLandingDust();
                }
                if (_isStomping)
                    _isStomping = false;
            }
            else if (wasGrounded)
            {
                // Jump stretch effect
                _targetScale = new Vector3(0.85f, 1.15f, 1f);
                _squashTimer = 0.08f;
            }
        }

        private void CheckWalls()
        {
            _isTouchingWallLeft = false;
            _isTouchingWallRight = false;

            if (_isStomping) return;

            EnsureFilterInitialized();

            Vector2 origin = (Vector2)transform.position + _collider.offset;
            float halfWidth = _collider.size.x * 0.5f;
            float halfHeight = _collider.size.y * 0.3f;

            // Two raycasts per side (chest and foot height) for reliable detection
            Vector2 chestOrigin = origin + Vector2.up * halfHeight * 0.5f;
            Vector2 footOrigin = origin - Vector2.up * halfHeight * 0.3f;

            // Check right wall
            int countR1 = Physics2D.Raycast(chestOrigin, Vector2.right, _physicsFilter, _wallHits, halfWidth + WALL_CHECK_DISTANCE);
            bool rightChest = HasSolidHit(_wallHits, countR1);
            int countR2 = Physics2D.Raycast(footOrigin, Vector2.right, _physicsFilter, _wallHits, halfWidth + WALL_CHECK_DISTANCE);
            bool rightFoot = HasSolidHit(_wallHits, countR2);
            _isTouchingWallRight = rightChest && rightFoot;

            // Check left wall
            int countL1 = Physics2D.Raycast(chestOrigin, Vector2.left, _physicsFilter, _wallHits, halfWidth + WALL_CHECK_DISTANCE);
            bool leftChest = HasSolidHit(_wallHits, countL1);
            int countL2 = Physics2D.Raycast(footOrigin, Vector2.left, _physicsFilter, _wallHits, halfWidth + WALL_CHECK_DISTANCE);
            bool leftFoot = HasSolidHit(_wallHits, countL2);
            _isTouchingWallLeft = leftChest && leftFoot;
        }

        private bool HasSolidHit(RaycastHit2D[] hits, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (hits[i].collider != null && hits[i].collider != _collider &&
                    !hits[i].collider.isTrigger)
                    return true;
            }
            return false;
        }

        private void HandleWallSlide()
        {
            bool wasWallSliding = _isWallSliding;
            _isWallSliding = false;

            if (_isGrounded || _isStomping)
            {
                _wallSlideSide = 0;
                _wallCoyoteTimer = 0;
                _forceWallSlide = false;
                if (_wallSlideAudioSource != null && _wallSlideAudioSource.isPlaying)
                    _wallSlideAudioSource.Stop();
                return;
            }

            // Wall-jump lock: reduce player horizontal control during lock
            if (_wallJumpLockTimer > 0)
                _wallJumpLockTimer--;

            float input = InputManager.MoveX;

            // Wall-slide activates when: airborne + pressing into wall
            // Allow grabbing at any vertical velocity so player can catch walls after wall-jumps
            bool slidingRight = _isTouchingWallRight && input > 0.3f;
            bool slidingLeft = _isTouchingWallLeft && input < -0.3f;

            // Forced wall slide from air dash: stay on wall until player presses away or jumps
            if (!slidingRight && !slidingLeft && _forceWallSlide)
            {
                bool stillTouching = (_wallSlideSide == 1 && _isTouchingWallRight) ||
                                     (_wallSlideSide == -1 && _isTouchingWallLeft);
                bool pressingAway = (_wallSlideSide == 1 && input < -0.3f) ||
                                    (_wallSlideSide == -1 && input > 0.3f);

                if (stillTouching && !pressingAway)
                {
                    slidingRight = _wallSlideSide == 1;
                    slidingLeft = _wallSlideSide == -1;
                }
                else
                {
                    _forceWallSlide = false;
                }
            }

            if (slidingRight || slidingLeft)
            {
                _isWallSliding = true;
                _wallSlideSide = slidingRight ? 1 : -1;
                _wallCoyoteTimer = WALL_COYOTE_FRAMES;

                // Zero horizontal velocity to stick to wall (prevents physics bounce)
                _velocity.x = 0f;
                // Clear wall-jump lock on new wall-grab (enables chimney climbing)
                _wallJumpLockTimer = 0;

                // Only clamp descending velocity — preserve upward momentum from wall-jumps
                // Clamp fall speed while sliding
                if (_velocity.y < -WALL_SLIDE_SPEED)
                    _velocity.y = -WALL_SLIDE_SPEED;

                // Start wall-slide audio
                if (!wasWallSliding)
                    StartWallSlideAudio();
            }
            else
            {
                // Wall-coyote: brief grace period after leaving wall
                if (wasWallSliding && _wallCoyoteTimer > 0)
                    _wallCoyoteTimer--;
                else
                    _wallSlideSide = 0;

                if (_wallSlideAudioSource != null && _wallSlideAudioSource.isPlaying)
                    _wallSlideAudioSource.Stop();
            }
        }

        private void StartWallSlideAudio()
        {
            if (_wallSlideAudioSource == null)
            {
                _wallSlideAudioSource = gameObject.AddComponent<AudioSource>();
                _wallSlideAudioSource.loop = true;
                _wallSlideAudioSource.volume = AudioManager.Instance != null
                    ? AudioManager.Instance.SFXVolume * 0.3f : 0.1f;
                _wallSlideAudioSource.clip = PlaceholderAudio.GetWallSlideSFX();
            }
            if (!_wallSlideAudioSource.isPlaying)
                _wallSlideAudioSource.Play();
        }

        private void UpdateWallSlideVisuals()
        {
            if (_isWallSliding)
            {
                // Squish sprite for wall-slide pose
                transform.localScale = new Vector3(0.85f, 1.1f, 1f);
                // Face away from wall
                _spriteRenderer.flipX = _wallSlideSide > 0;

                // Spawn dust particles
                _wallDustTimer -= Time.fixedDeltaTime;
                if (_wallDustTimer <= 0f)
                {
                    SpawnWallDust();
                    _wallDustTimer = WALL_DUST_INTERVAL;
                }
            }
            else
            {
                // Non-wall-sliding scale handled by UpdateSquashStretch
                _wallDustTimer = 0f;
            }
        }

        private void UpdateSquashStretch()
        {
            if (_isWallSliding) return; // Wall-slide has its own scale

            if (_squashTimer > 0f)
            {
                _squashTimer -= Time.fixedDeltaTime;
                transform.localScale = Vector3.Lerp(Vector3.one, _targetScale,
                    Mathf.Clamp01(_squashTimer / 0.1f));
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }

        private void UpdateAnimationState()
        {
            if (_animator == null) return;

            if (_isWallSliding)
                _animator.SetState(SpriteAnimator.AnimState.WallSlide);
            else if (!_isGrounded && _velocity.y > 0.5f)
                _animator.SetState(SpriteAnimator.AnimState.Jump);
            else if (!_isGrounded && _velocity.y < -0.5f)
                _animator.SetState(SpriteAnimator.AnimState.Fall);
            else if (_isGrounded && Mathf.Abs(_velocity.x) > 0.5f)
                _animator.SetState(SpriteAnimator.AnimState.Walk);
            else
                _animator.SetState(SpriteAnimator.AnimState.Idle);
        }

        private void SpawnWallDust()
        {
            float wallX = _wallSlideSide > 0
                ? transform.position.x + _collider.size.x * 0.4f
                : transform.position.x - _collider.size.x * 0.4f;
            Vector3 dustPos = new Vector3(wallX, transform.position.y + _collider.offset.y, 0f);

            var dustGO = ObjectPool.GetFlash();
            dustGO.transform.position = dustPos;
            var sr = dustGO.GetComponent<SpriteRenderer>();
            sr.sprite = PlaceholderAssets.GetProjectileSprite(Generative.WeaponTier.Starting);
            sr.color = new Color(0.8f, 0.8f, 0.7f, 0.5f);
            sr.sortingOrder = 5;
            dustGO.transform.localScale = Vector3.one * 0.3f;
            dustGO.GetComponent<PoolTimer>().StartTimer(0.2f);
        }

        private void SpawnLandingDust()
        {
            // Scale particle count by fall speed: 0 at walk speed, up to 4 at max fall
            float fallSpeed = Mathf.Abs(_velocity.y);
            int count = Mathf.Clamp(Mathf.FloorToInt(fallSpeed / MAX_FALL_SPEED * 4f), 1, 4);

            Vector3 basePos = transform.position + Vector3.down * (_collider.size.y * 0.5f);
            for (int i = 0; i < count; i++)
            {
                var go = ObjectPool.GetParticle();
                go.transform.position = basePos + new Vector3(
                    Random.Range(-0.4f, 0.4f), Random.Range(-0.1f, 0.1f), 0f);
                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetParticleSprite();
                sr.color = new Color(0.75f, 0.7f, 0.6f, 0.6f);
                sr.sortingOrder = 5;
                go.transform.localScale = Vector3.one * Random.Range(0.15f, 0.3f);

                var rb = go.GetComponent<Rigidbody2D>();
                rb.gravityScale = 1f;
                float angle = Random.Range(45f, 135f) * Mathf.Deg2Rad;
                rb.linearVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(1f, 3f);

                go.GetComponent<PoolTimer>().StartTimer(0.4f);
            }
        }

        /// <summary>
        /// Reset per-level tracking (e.g. wall-jump counter for achievements).
        /// </summary>
        public void ResetLevelTracking()
        {
            _wallJumpsThisLevel = 0;
        }

        private void HandleMovement()
        {
            float input = InputManager.MoveX;

            // During wall-jump lock, don't allow input to override wall-jump direction
            // But allow input if player is touching a NEW wall (enables fast chimney climbs)
            if (_wallJumpLockTimer > 0 && !_isWallSliding)
                return;

            float targetSpeed = input * MOVE_SPEED;

            float accel = Mathf.Abs(targetSpeed) > 0.01f ? ACCELERATION : DECELERATION;
            _velocity.x = Mathf.MoveTowards(_velocity.x, targetSpeed, accel * Time.fixedDeltaTime);

            // Flip sprite (but not during wall-slide — that's handled in UpdateWallSlideVisuals)
            if (!_isWallSliding)
            {
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

            // Wall-jump: if wall-sliding (or wall-coyote) and jump buffered
            bool canWallJump = (_isWallSliding || _wallCoyoteTimer > 0) && _wallSlideSide != 0
                               && _jumpBufferTimer > 0;

            if (canWallJump)
            {
                // Jump away from wall
                _velocity.x = -_wallSlideSide * WALL_JUMP_VELOCITY_X;
                _velocity.y = WALL_JUMP_VELOCITY_Y;
                _wallJumpLockTimer = WALL_JUMP_LOCK_FRAMES;
                _jumpBufferTimer = 0;
                _wallCoyoteTimer = 0;
                _isWallSliding = false;
                _forceWallSlide = false;
                _jumpCut = false;

                // Face away from wall
                _facingRight = _wallSlideSide < 0;
                _spriteRenderer.flipX = !_facingRight;

                _wallJumpsThisLevel++;
                AudioManager.PlaySFX(PlaceholderAudio.GetJumpSFX());
                return;
            }

            // Normal jump: grounded or within coyote time, and jump buffered
            bool canJump = (_isGrounded || _coyoteTimer > 0) && _jumpBufferTimer > 0;

            if (canJump)
            {
                _velocity.y = JUMP_VELOCITY;
                _coyoteTimer = 0;
                _jumpBufferTimer = 0;
                _jumpCut = false;
                AudioManager.PlaySFX(PlaceholderAudio.GetJumpSFX());
            }
            else if (_jumpBufferTimer > 0 && !_isGrounded && _coyoteTimer <= 0)
            {
                // Double jump: mid-air, no coyote time, jump buffered
                var abilities = GetComponent<AbilitySystem>();
                if (abilities != null && abilities.TryDoubleJump())
                {
                    _velocity.y = JUMP_VELOCITY * 0.85f; // Slightly weaker than ground jump
                    _jumpBufferTimer = 0;
                    _jumpCut = false;
                }
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
            float gravity;
            if (_isWallSliding)
                gravity = WALL_SLIDE_GRAVITY;
            else
                gravity = _velocity.y > 0 ? GRAVITY_UP : GRAVITY_DOWN;

            _velocity.y -= gravity * Time.fixedDeltaTime;

            float maxFall = _isWallSliding ? WALL_SLIDE_SPEED : MAX_FALL_SPEED;
            if (_velocity.y < -maxFall)
                _velocity.y = -maxFall;

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

        private void HandleStompInput()
        {
            if (InputManager.StompPressed && !_isGrounded && !_isStomping && !_isWallSliding)
            {
                _isStomping = true;
                _stompStallTimer = STOMP_STALL_DURATION; // brief air-stall before descending
                _velocity.y = 0f;
                _velocity.x = 0f;
            }
        }

        /// <summary>
        /// Get wall-jump count this level (for achievement tracking).
        /// </summary>
        public int WallJumpsThisLevel => _wallJumpsThisLevel;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsAlive) return;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Head bump: hit something above while jumping
                if (contact.normal.y < -0.5f && _velocity.y > 0f)
                {
                    TryBreakTileAbove(contact.point);
                    _velocity.y = 0f;
                    break;
                }

                // Stomp onto enemy: boosted bounce + damage + reset double-jump
                if (_isStomping && contact.normal.y > 0.5f)
                {
                    var enemy = collision.gameObject.GetComponent<EnemyBase>();
                    var boss = collision.gameObject.GetComponent<Boss>();
                    if (enemy != null || boss != null)
                    {
                        _isStomping = false;
                        if (enemy != null) enemy.TakeDamage(2);
                        if (boss != null) boss.TakeDamage(2);
                        _velocity.y = STOMP_CHAIN_BOUNCE;
                        AudioManager.PlaySFX(PlaceholderAudio.GetStompSFX());
                        CameraController.Instance?.AddTrauma(0.15f);
                        SpawnChainBounceEffect();

                        // Reset double-jump for chain potential
                        var chainAbilities = GetComponent<AbilitySystem>();
                        if (chainAbilities != null) chainAbilities.ResetDoubleJump();

                        break;
                    }
                }

                // Ground pound landing: hit ground below while stomping
                if (contact.normal.y > 0.5f && _isStomping)
                {
                    _isStomping = false;
                    AudioManager.PlaySFX(PlaceholderAudio.GetStompSFX());
                    CameraController.Instance?.AddTrauma(0.25f);
                    bool brokeTiles = TryBreakTilesBelow(contact.point);

                    if (brokeTiles)
                    {
                        _velocity.y = STOMP_CHAIN_BOUNCE; // boosted bounce on tile break
                        SpawnChainBounceEffect();
                        var chainAbilities = GetComponent<AbilitySystem>();
                        if (chainAbilities != null) chainAbilities.ResetDoubleJump();
                    }
                    else
                    {
                        _velocity.y = STOMP_BOUNCE; // normal bounce
                    }

                    // Stomp shockwave: damage flying enemies within 2 tiles
                    StompShockwave();

                    // Enhanced ground slam if ability is available
                    var abilities = GetComponent<AbilitySystem>();
                    if (abilities != null && abilities.HasGroundSlam)
                        abilities.PerformGroundSlam();

                    break;
                }
            }
        }

        /// <summary>
        /// Stomp shockwave: damages flying enemies within 2 tiles of landing point.
        /// Gives the player counterplay against otherwise stomp-immune flyers.
        /// </summary>
        private void StompShockwave()
        {
            const float SHOCKWAVE_RADIUS = 2f;
            Vector2 stompPos = transform.position;

            for (int i = EnemyBase.ActiveEnemies.Count - 1; i >= 0; i--)
            {
                var enemyGO = EnemyBase.ActiveEnemies[i];
                if (enemyGO == null) continue;

                var enemy = enemyGO.GetComponent<EnemyBase>();
                if (enemy == null || enemy.IsDead) continue;

                float dist = Vector2.Distance(stompPos, enemyGO.transform.position);
                if (dist <= SHOCKWAVE_RADIUS)
                {
                    enemy.TakeDamage(2);

                    // Visual feedback: shockwave ring particle
                    var flash = ObjectPool.GetFlash();
                    flash.transform.position = enemyGO.transform.position;
                    var sr = flash.GetComponent<SpriteRenderer>();
                    sr.sprite = PlaceholderAssets.GetParticleSprite();
                    sr.color = new Color(1f, 0.8f, 0.3f, 0.7f);
                    sr.sortingOrder = 15;
                    flash.transform.localScale = Vector3.one * 1.5f;
                    flash.GetComponent<PoolTimer>().StartTimer(0.15f);
                }
            }
        }

        private void TryBreakTileAbove(Vector2 contactPoint)
        {
            if (_levelRenderer == null)
                _levelRenderer = FindAnyObjectByType<LevelRenderer>();
            if (_levelRenderer == null) return;

            // Nudge into the tile we hit (slightly above the contact point)
            Vector2 checkPoint = contactPoint + Vector2.up * 0.1f;
            Vector2Int levelPos = _levelRenderer.WorldToLevel(checkPoint);
            TryBreakAt(_levelRenderer, levelPos.x, levelPos.y);
        }

        private bool TryBreakTilesBelow(Vector2 contactPoint)
        {
            if (_levelRenderer == null)
                _levelRenderer = FindAnyObjectByType<LevelRenderer>();
            if (_levelRenderer == null) return false;

            // Nudge into the tile below the contact point
            Vector2 checkPoint = contactPoint + Vector2.down * 0.1f;
            Vector2Int levelPos = _levelRenderer.WorldToLevel(checkPoint);

            // Break a small area below: center tile and one on each side
            bool anyBroken = false;
            for (int dx = -1; dx <= 1; dx++)
            {
                if (TryBreakAt(_levelRenderer, levelPos.x + dx, levelPos.y))
                    anyBroken = true;
            }
            return anyBroken;
        }

        private bool TryBreakAt(LevelRenderer levelRenderer, int tileX, int tileY)
        {
            var destructible = levelRenderer.GetDestructibleAt(tileX, tileY);

            // Handle indestructible tiles - they can be worn down with repeated hits
            // Head bops and stomps deal 5 damage (10 hits to break at HP=50)
            if (destructible.MaterialClass == (byte)MaterialClass.Indestructible)
            {
                levelRenderer.DamageIndestructibleTile(tileX, tileY, damage: 5);
                return false;
            }

            if (destructible.MaterialClass > 0 &&
                destructible.MaterialClass < (byte)MaterialClass.Indestructible)
            {
                WeaponTier tier = _weaponSystem != null ? _weaponSystem.CurrentTier : WeaponTier.Starting;
                MaterialClass mat = (MaterialClass)destructible.MaterialClass;

                bool canBreak = mat switch
                {
                    MaterialClass.Soft => true,
                    MaterialClass.Medium => true,
                    MaterialClass.Hard => tier >= WeaponTier.Medium,
                    MaterialClass.Reinforced => tier >= WeaponTier.Heavy,
                    _ => false,
                };

                if (canBreak)
                {
                    levelRenderer.DestroyTile(tileX, tileY);
                    return true;
                }
            }
            return false;
        }

        private void SpawnChainBounceEffect()
        {
            var go = ObjectPool.GetFlash();
            go.transform.position = transform.position + Vector3.down * 0.5f;
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = PlaceholderAssets.GetParticleSprite();
            sr.color = new Color(1f, 0.85f, 0.3f, 0.8f); // gold burst
            sr.sortingOrder = 15;
            go.transform.localScale = Vector3.one * 2f;
            go.GetComponent<PoolTimer>().StartTimer(0.2f);
        }

        /// <summary>
        /// Force-enter wall slide state (used by air dash → wall transition).
        /// </summary>
        public void ForceWallSlide()
        {
            if (_isTouchingWallRight)
                _wallSlideSide = 1;
            else if (_isTouchingWallLeft)
                _wallSlideSide = -1;
            else
                return;

            _isWallSliding = true;
            _forceWallSlide = true;
            _velocity.x = 0f;
            _wallCoyoteTimer = WALL_COYOTE_FRAMES;

            // Face away from wall (ready for wall-jump)
            _facingRight = _wallSlideSide < 0;
            _spriteRenderer.flipX = !_facingRight;
        }
    }
}
