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
        private float _baseMoveSpeed = 2.5f;
        private int _direction = 1;
        private float _arenaMinX;
        private float _arenaMaxX;

        public float ArenaMinX => _arenaMinX;
        public float ArenaMaxX => _arenaMaxX;
        public int CurrentPhase => _currentPhase;
        public float LastDpsCapTime { get; private set; } = -10f;

        // Combat
        private float _attackCooldown = 2.0f;
        private float _baseAttackCooldown = 2.0f;
        private float _attackTimer;
        private float _chargeSpeed = 6f;
        private float _baseChargeSpeed = 6f;
        private float _phaseSpeedMultiplier = 1f;
        private bool _isCharging;
        private float _chargeTimer;
        private const float CHARGE_DURATION = 1.4f;  // 0.6s telegraph + 0.8s charge
        private const float CHARGE_TELEGRAPH = 0.6f;  // wind-up duration

        // Slow effect
        private float _slowFactor = 1f;
        private float _slowTimer;
        private Color _baseColor = Color.white;

        // Phase system (changes behavior at health thresholds)
        private int _currentPhase = 1;

        // DPS cap and phase timing
        private float _phaseTimer; // Time spent in current phase
        private const float MIN_PHASE_DURATION = 5f; // Minimum seconds per phase
        private float _recentDamage; // Damage taken recently, decays continuously
        private const float MAX_DPS = 15f; // Max damage per second

        // Teleport telegraph
        private float _teleportTelegraphTimer;
        private bool _isTeleporting;

        // Arena pillar shelter (Phase 3 mechanic replacing shield)
        private bool _isSheltered; // Boss is behind a pillar, immune to damage
        private ArenaPillar _currentPillar; // Pillar the boss is sheltering behind
        private float _shelterTimer; // Timer until next shelter attempt
        private const float SHELTER_INTERVAL = 6f; // Seconds between shelter attempts

        // Contact damage cooldown
        private float _lastContactDamageTime = -1f;
        private const float CONTACT_DAMAGE_INTERVAL = 0.5f;

        // Phase transition invulnerability
        private float _phaseInvulnTimer;

        // Era-based behavior ratios (set in Initialize)
        private float _shootChance = 0.4f; // Chance to shoot vs charge in Phase 2+

        // Boss variant: era-specific behavior
        private BossVariant _variant;
        private int _maxPhases;
        private float _minionTimer;
        private float _shockwaveTimer;

        public void Initialize(BossType type, LevelRenderer tilemapRenderer, float arenaMinX, float arenaMaxX)
        {
            Type = type;
            _tilemapRenderer = tilemapRenderer;
            _arenaMinX = arenaMinX;
            _arenaMaxX = arenaMaxX;

            // Use DifficultyProfile for epoch-scaled boss HP
            int era = (int)type;
            var diff = Generative.DifficultyProfile.GetParams(era);
            MaxHealth = diff.BossHp;
            Health = MaxHealth;

            // Movement speed increases slightly with era
            _moveSpeed = 2.0f + era * 0.15f;
            _baseMoveSpeed = _moveSpeed;
            _chargeSpeed = 5f + era * 0.3f;
            _baseChargeSpeed = _chargeSpeed;

            _baseAttackCooldown = _attackCooldown;

            // Era-based behavior: early eras charge-heavy, late eras shoot-heavy
            if (era <= 2)
                _shootChance = 0.2f;   // 20% shoot, 80% charge
            else if (era <= 5)
                _shootChance = 0.4f;   // 40% shoot, 60% charge (balanced)
            else if (era <= 8)
                _shootChance = 0.65f;  // 65% shoot, 35% charge
            else
                _shootChance = 0.5f;   // Era 9: balanced (teleport handled separately)

            // Set boss variant based on era
            _variant = (BossVariant)era;
            _maxPhases = era <= 2 ? 2 : (era == 9 ? 4 : 3);

            // Era 0-2: simple bosses (2 phases only)
            if (era == 0) _shootChance = 0f; // charge-only
            if (era <= 2)
            {
                _attackCooldown *= 1.3f; // slower cooldowns for early eras
                _baseAttackCooldown = _attackCooldown;
            }

            _phaseTimer = 0f;
            _recentDamage = 0f;
            _isSheltered = false;
            _currentPillar = null;
            _shelterTimer = SHELTER_INTERVAL;
        }

        /// <summary>
        /// Fully reset boss state (called on player respawn in boss arena).
        /// </summary>
        public void ResetBoss()
        {
            if (IsDead) return;

            Health = MaxHealth;
            _currentPhase = 1;
            _phaseTimer = 0f;
            _recentDamage = 0f;
            _isSheltered = false;
            _currentPillar = null;
            _shelterTimer = SHELTER_INTERVAL;
            _slowTimer = 0f;
            _slowFactor = 1f;
            _phaseSpeedMultiplier = 1f;
            _moveSpeed = _baseMoveSpeed;
            _chargeSpeed = _baseChargeSpeed;
            _attackCooldown = _baseAttackCooldown;
            _lastContactDamageTime = -1f;
            _isCharging = false;
            _chargeTimer = 0f;
            _attackTimer = _baseAttackCooldown; // Full cooldown before first attack
            _minionTimer = 0f;
            _shockwaveTimer = 0f;
            _isTeleporting = false;
            _teleportTelegraphTimer = 0f;
            IsActive = false;

            int era = (int)Type;
            _maxPhases = era <= 2 ? 2 : (era == 9 ? 4 : 3);

            if (_sr != null)
                _sr.color = Color.white;
            _baseColor = Color.white;
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            _playerTransform = GameObject.FindWithTag("Player")?.transform;
            EnemyBase.Register(gameObject);
        }

        private void OnDestroy()
        {
            EnemyBase.Unregister(gameObject);
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

            // Lazy re-find player if reference was lost (e.g., player respawn)
            if (_playerTransform == null)
                _playerTransform = GameObject.FindWithTag("Player")?.transform;

            UpdateSlow();

            if (_minionTimer > 0f)
                _minionTimer -= Time.fixedDeltaTime;
            if (_shockwaveTimer > 0f)
                _shockwaveTimer -= Time.fixedDeltaTime;

            // Phase invulnerability timer
            if (_phaseInvulnTimer > 0f)
                _phaseInvulnTimer -= Time.fixedDeltaTime;

            // Teleport telegraph: shimmer before teleport
            if (_isTeleporting)
            {
                _teleportTelegraphTimer -= Time.fixedDeltaTime;
                if (_sr != null)
                {
                    float alpha = 0.3f + Mathf.PingPong(_teleportTelegraphTimer * 10f, 0.7f);
                    _sr.color = new Color(0.6f, 0.4f, 1f, alpha);
                }
                if (_teleportTelegraphTimer <= 0f)
                {
                    _isTeleporting = false;
                    ExecuteTeleport();
                }
                return; // Skip other behavior during telegraph
            }

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
                case 4:
                    UpdatePhase4();
                    break;
            }

            // Boss breathing: scale pulse, faster in later phases
            if (!IsDead)
            {
                float breathSpeed = 1.5f + (_currentPhase - 1) * 0.5f;
                float breathAmount = 0.02f + (_currentPhase - 1) * 0.01f;
                float breath = 1f + Mathf.Sin(Time.time * breathSpeed) * breathAmount;
                transform.localScale = new Vector3(breath, breath, 1f);
            }
        }

        private void UpdatePhase()
        {
            _phaseTimer += Time.fixedDeltaTime;

            // Continuous DPS decay: damage falls off smoothly over 1 second
            // Prevents burst exploit at fixed-window boundaries
            _recentDamage = Mathf.Max(0f, _recentDamage - MAX_DPS * Time.fixedDeltaTime);

            float healthPercent = (float)Health / MaxHealth;

            // Phase thresholds based on max phases
            if (_maxPhases == 2)
            {
                if (_currentPhase == 1 && healthPercent <= 0.50f && _phaseTimer >= MIN_PHASE_DURATION)
                {
                    _currentPhase = 2;
                    _phaseTimer = 0f;
                    OnPhaseChange(2);
                }
            }
            else if (_maxPhases == 3)
            {
                if (_currentPhase == 1 && healthPercent <= 0.66f && _phaseTimer >= MIN_PHASE_DURATION)
                {
                    _currentPhase = 2;
                    _phaseTimer = 0f;
                    OnPhaseChange(2);
                }
                else if (_currentPhase == 2 && healthPercent <= 0.33f && _phaseTimer >= MIN_PHASE_DURATION)
                {
                    _currentPhase = 3;
                    _phaseTimer = 0f;
                    OnPhaseChange(3);
                }
            }
            else if (_maxPhases == 4)
            {
                if (_currentPhase == 1 && healthPercent <= 0.75f && _phaseTimer >= MIN_PHASE_DURATION)
                {
                    _currentPhase = 2;
                    _phaseTimer = 0f;
                    OnPhaseChange(2);
                }
                else if (_currentPhase == 2 && healthPercent <= 0.50f && _phaseTimer >= MIN_PHASE_DURATION)
                {
                    _currentPhase = 3;
                    _phaseTimer = 0f;
                    OnPhaseChange(3);
                }
                else if (_currentPhase == 3 && healthPercent <= 0.25f && _phaseTimer >= MIN_PHASE_DURATION)
                {
                    _currentPhase = 4;
                    _phaseTimer = 0f;
                    OnPhaseChange(4);
                }
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

            // Hit-stop on boss phase transition (4 frames / ~66ms)
            GameManager.HitStop(0.066f);

            // Screen flash and roar SFX
            ScreenFlash.Flash(Color.white, 0.3f);
            AudioManager.PlaySFX(PlaceholderAudio.GetBossRoarSFX());
            CameraController.Instance?.AddTrauma(0.35f);

            // Brief invulnerability during phase transition (0.5s)
            _phaseInvulnTimer = 0.5f;

            // Speed up in later phases — update multiplier so slow recovery is correct
            _phaseSpeedMultiplier *= 1.2f;
            _moveSpeed = _baseMoveSpeed * _phaseSpeedMultiplier * _slowFactor;
            _chargeSpeed = _baseChargeSpeed * _phaseSpeedMultiplier * _slowFactor;
            _attackCooldown *= 0.8f;

            // Tempo increase per phase (handled by AudioManager era parameterization)

            // Flash white then transition to new color over 0.5s
            StartCoroutine(PhaseTransitionFlash(newPhase));
        }

        private System.Collections.IEnumerator PhaseTransitionFlash(int phase)
        {
            if (_sr == null) yield break;

            Color startColor = Color.white;
            float tint = 1f - (phase - 1) * 0.15f;
            Color endColor = new Color(1f, tint, tint);

            _sr.color = startColor;
            float elapsed = 0f;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                _sr.color = Color.Lerp(startColor, endColor, elapsed / 0.5f);
                yield return null;
            }
            _sr.color = endColor;
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
                    // Era-specific Phase 1 attacks
                    if (_variant == BossVariant.Primitive)
                        StartCharge(); // Era 0: charge-only
                    else if (Random.value < _shootChance * 0.5f)
                        ShootAtPlayer();
                    else
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

            // Attack with projectiles and charges (era-based ratio)
            _attackTimer -= Time.fixedDeltaTime;
            if (_attackTimer <= 0f && _playerTransform != null)
            {
                // Era-specific Phase 2 attacks
                if (_variant == BossVariant.Primitive)
                {
                    // Era 0: gains projectile in Phase 2 (30% shoot, 70% charge)
                    if (Random.value < 0.3f)
                        ShootAtPlayer();
                    else
                        StartCharge();
                }
                else if (_variant == BossVariant.Medieval && Random.value < 0.3f)
                    GroundSlamShockwave();
                else if (_variant == BossVariant.Renaissance && Random.value < 0.25f)
                    TeleportDash();
                else if (Random.value < _shootChance)
                    ShootAtPlayer();
                else
                    StartCharge();
                _attackTimer = _attackCooldown * 0.7f;
            }
        }

        /// <summary>
        /// Phase 3: Desperate, constant attacks. Periodically shelters behind arena pillars.
        /// </summary>
        private void UpdatePhase3()
        {
            if (_rb == null) return;

            if (_isCharging)
            {
                UpdateCharge();
                return;
            }

            // Shelter logic: periodically hide behind nearest pillar
            if (_isSheltered)
            {
                // Defensive: if pillar reference became invalid, break shelter
                if (_currentPillar == null || _currentPillar.IsDestroyed)
                {
                    _isSheltered = false;
                    _currentPillar = null;
                    _shelterTimer = SHELTER_INTERVAL;
                    // Fall through to normal Phase 3 behavior
                }
                else
                {
                    // While sheltered: stationary, still shoots
                    _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);

                    // Shelter tint
                    if (_sr != null)
                        _sr.color = new Color(0.5f, 0.7f, 1f); // Blue shield visual

                    // Still shoot while sheltered
                    _attackTimer -= Time.fixedDeltaTime;
                    if (_attackTimer <= 0f && _playerTransform != null)
                    {
                        ShootAtPlayer();
                        _attackTimer = _attackCooldown * 0.7f;
                    }
                    return;
                }
            }

            // Try to shelter behind a pillar periodically
            _shelterTimer -= Time.fixedDeltaTime;
            if (_shelterTimer <= 0f)
            {
                _shelterTimer = SHELTER_INTERVAL;
                TryShelterBehindPillar();
                if (_isSheltered) return;
            }

            // Normal Phase 3: aggressively chase
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

            // Rapid attacks (era-based ratio: late eras spread-heavy)
            _attackTimer -= Time.fixedDeltaTime;
            if (_attackTimer <= 0f && _playerTransform != null)
            {
                // Era-specific Phase 3 attacks
                if (_variant == BossVariant.Industrial && _minionTimer <= 0f)
                {
                    SummonMinions();
                    _minionTimer = 15f; // 15s between summons
                }
                else if (_variant == BossVariant.Medieval && Random.value < 0.4f)
                    GroundSlamShockwave();
                else if (_variant == BossVariant.Renaissance && Random.value < 0.3f)
                    TeleportDash();
                else if (Random.value < _shootChance)
                    ShootSpread();
                else
                    StartCharge();
                _attackTimer = _attackCooldown * 0.5f;
            }
        }

        /// <summary>
        /// Phase 4: The Architect (Era 9 only). Fast pattern with all mechanics.
        /// </summary>
        private void UpdatePhase4()
        {
            if (_rb == null) return;

            if (_isCharging)
            {
                UpdateCharge();
                return;
            }

            // Very aggressive chase
            if (_playerTransform != null)
            {
                float dir = Mathf.Sign(_playerTransform.position.x - transform.position.x);
                float targetX = transform.position.x + dir * _moveSpeed * Time.fixedDeltaTime;
                if (targetX < _arenaMinX + 2) dir = 1;
                if (targetX > _arenaMaxX - 2) dir = -1;

                _rb.linearVelocity = new Vector2(dir * _moveSpeed * 1.8f, _rb.linearVelocity.y);

                if (_sr != null)
                    _sr.flipX = dir < 0;
            }

            // Rapid mixed attacks
            _attackTimer -= Time.fixedDeltaTime;
            if (_attackTimer <= 0f && _playerTransform != null)
            {
                float roll = Random.value;
                if (roll < 0.2f)
                    GroundSlamShockwave();
                else if (roll < 0.35f)
                    TeleportDash();
                else if (roll < 0.55f)
                    ShootSpread();
                else if (roll < 0.75f)
                    StartCharge();
                else
                    ShootAtPlayer();
                _attackTimer = _attackCooldown * 0.35f;
            }
        }

        /// <summary>
        /// Find and move to the nearest standing arena pillar.
        /// </summary>
        private void TryShelterBehindPillar()
        {
            var pillars = FindObjectsByType<ArenaPillar>(FindObjectsSortMode.None);
            if (pillars == null || pillars.Length == 0) return;

            ArenaPillar nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var pillar in pillars)
            {
                if (pillar.IsDestroyed) continue;
                float dist = Mathf.Abs(pillar.transform.position.x - transform.position.x);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = pillar;
                }
            }

            if (nearest == null) return;

            // Snap to pillar position (behind it relative to player)
            _currentPillar = nearest;
            _isSheltered = true;

            // Flash pillar to draw player attention
            StartCoroutine(FlashPillar(nearest));

            // Show gameplay hint on first shelter
            if (TutorialManager.Instance != null && !TutorialManager.IsHintDismissed("pillarshelter"))
            {
                TutorialManager.Instance.ShowGameplayHintPublic("Destroy the pillar!");
                TutorialManager.DismissHint("pillarshelter");
            }

            float pillarX = nearest.transform.position.x;
            if (_playerTransform != null)
            {
                float playerDir = Mathf.Sign(_playerTransform.position.x - pillarX);
                // Move to opposite side of pillar from player, clamped to arena
                float shelterX = pillarX - playerDir * 1.5f;
                shelterX = Mathf.Clamp(shelterX, _arenaMinX + 2f, _arenaMaxX - 2f);
                transform.position = new Vector3(
                    shelterX,
                    transform.position.y,
                    transform.position.z);
            }
        }

        /// <summary>
        /// Flash the pillar sprite to draw player attention when boss shelters.
        /// </summary>
        private System.Collections.IEnumerator FlashPillar(ArenaPillar pillar)
        {
            var pillarSR = pillar != null ? pillar.GetComponent<SpriteRenderer>() : null;
            if (pillarSR == null) yield break;

            Color origColor = pillarSR.color;
            for (int i = 0; i < 4; i++)
            {
                pillarSR.color = Color.yellow;
                yield return new WaitForSeconds(0.15f);
                if (pillar == null || pillar.IsDestroyed) yield break;
                pillarSR.color = origColor;
                yield return new WaitForSeconds(0.15f);
                if (pillar == null || pillar.IsDestroyed) yield break;
            }
        }

        /// <summary>
        /// Called by ArenaPillar when it is destroyed.
        /// Breaks shelter and resumes normal Phase 3 behavior.
        /// </summary>
        public void OnPillarDestroyed(ArenaPillar pillar)
        {
            if (_currentPillar == pillar)
            {
                _isSheltered = false;
                _currentPillar = null;
                _shelterTimer = SHELTER_INTERVAL;

                // Reset visual
                if (_sr != null)
                {
                    float tint = 1f - (_currentPhase - 1) * 0.15f;
                    _sr.color = new Color(1f, tint, tint);
                }
            }
        }

        private void StartCharge()
        {
            if (_playerTransform == null) return;

            _isCharging = true;
            _chargeTimer = CHARGE_DURATION;
            _direction = _playerTransform.position.x > transform.position.x ? 1 : -1;

            // Visual wind-up: orange tint
            if (_sr != null)
                _sr.color = new Color(1f, 0.8f, 0.5f);

            // Charge wind-up growl SFX
            AudioManager.PlaySFX(PlaceholderAudio.GetChargeWindupSFX());
        }

        private void UpdateCharge()
        {
            if (_rb == null) return;

            _chargeTimer -= Time.fixedDeltaTime;

            // Telegraph phase (first 0.6s): pull back away from player
            if (_chargeTimer > CHARGE_DURATION - CHARGE_TELEGRAPH)
            {
                // Move away from player slowly (pullback telegraph)
                _rb.linearVelocity = new Vector2(-_direction * _moveSpeed * 0.5f, _rb.linearVelocity.y);

                // Flash intensity builds during telegraph
                if (_sr != null)
                {
                    float telegraphProgress = 1f - (_chargeTimer - (CHARGE_DURATION - CHARGE_TELEGRAPH)) / CHARGE_TELEGRAPH;
                    float flash = Mathf.PingPong(telegraphProgress * 6f, 1f);
                    _sr.color = Color.Lerp(new Color(1f, 0.8f, 0.5f), new Color(1f, 0.4f, 0.2f), flash);
                }
            }
            // Charge phase: fast dash toward player
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

        /// <summary>
        /// Era 3: Ground-slam shockwave. Creates a visual shockwave that damages the player.
        /// </summary>
        private void GroundSlamShockwave()
        {
            if (_playerTransform == null) return;

            AudioManager.PlaySFX(PlaceholderAudio.GetStompSFX());
            CameraController.Instance?.AddTrauma(0.4f);

            // Spawn visual shockwave particles expanding outward
            for (int i = -3; i <= 3; i++)
            {
                var go = ObjectPool.GetParticle();
                go.transform.position = new Vector3(
                    transform.position.x + i * 2f,
                    transform.position.y - 1f, 0f);
                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetParticleSprite();
                sr.color = new Color(1f, 0.6f, 0.2f, 0.8f);
                sr.sortingOrder = 12;
                go.transform.localScale = Vector3.one * 0.8f;

                var rb = go.GetComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.linearVelocity = new Vector2(i * 3f, 0.5f);
                go.GetComponent<PoolTimer>().StartTimer(0.6f);
            }

            // Check if player is on the ground nearby
            if (_playerTransform != null)
            {
                float dist = Mathf.Abs(_playerTransform.position.x - transform.position.x);
                if (dist < 8f)
                {
                    var health = _playerTransform.GetComponent<HealthSystem>();
                    if (health != null)
                    {
                        // Only damage if player is on the ground (within 2 units Y)
                        float yDiff = Mathf.Abs(_playerTransform.position.y - transform.position.y);
                        if (yDiff < 2f)
                            health.TakeDamage(1, transform.position);
                    }
                }
            }
        }

        /// <summary>
        /// Era 4: Teleport dash. 0.4s shimmer telegraph, then teleports to opposite side.
        /// </summary>
        private void TeleportDash()
        {
            if (_playerTransform == null) return;

            // Start telegraph: shimmer/fade for 0.4s before teleporting
            _isTeleporting = true;
            _teleportTelegraphTimer = 0.4f;
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y); // Stop moving
            AudioManager.PlaySFX(PlaceholderAudio.GetChargeWindupSFX());
        }

        /// <summary>
        /// Execute the actual teleport after telegraph completes.
        /// </summary>
        private void ExecuteTeleport()
        {
            if (_playerTransform == null) return;

            // Flash effect at old position
            var flashGO = ObjectPool.GetFlash();
            flashGO.transform.position = transform.position;
            var flashSR = flashGO.GetComponent<SpriteRenderer>();
            flashSR.sprite = PlaceholderAssets.GetParticleSprite();
            flashSR.color = new Color(0.6f, 0.4f, 1f, 0.9f);
            flashSR.sortingOrder = 16;
            flashGO.transform.localScale = Vector3.one * 3f;
            flashGO.GetComponent<PoolTimer>().StartTimer(0.15f);

            // Teleport to opposite side of arena from player
            float playerX = _playerTransform.position.x;
            float arenaCenter = (_arenaMinX + _arenaMaxX) / 2f;
            float newX = playerX > arenaCenter
                ? _arenaMinX + 3f
                : _arenaMaxX - 3f;

            transform.position = new Vector3(newX, transform.position.y, transform.position.z);

            // Flash effect at new position
            var flashGO2 = ObjectPool.GetFlash();
            flashGO2.transform.position = transform.position;
            var flashSR2 = flashGO2.GetComponent<SpriteRenderer>();
            flashSR2.sprite = PlaceholderAssets.GetParticleSprite();
            flashSR2.color = new Color(0.6f, 0.4f, 1f, 0.9f);
            flashSR2.sortingOrder = 16;
            flashGO2.transform.localScale = Vector3.one * 3f;
            flashGO2.GetComponent<PoolTimer>().StartTimer(0.15f);

            CameraController.Instance?.AddTrauma(0.2f);

            // Reset color to phase tint
            if (_sr != null)
            {
                float tint = 1f - (_currentPhase - 1) * 0.15f;
                _sr.color = new Color(1f, tint, tint);
            }
        }

        /// <summary>
        /// Era 5: Summon 2 minion enemies in the arena.
        /// </summary>
        private void SummonMinions()
        {
            AudioManager.PlaySFX(PlaceholderAudio.GetBossRoarSFX());
            CameraController.Instance?.AddTrauma(0.3f);

            for (int i = 0; i < 2; i++)
            {
                float spawnX = _arenaMinX + 4f + i * ((_arenaMaxX - _arenaMinX - 8f));
                Vector3 spawnPos = new Vector3(spawnX, transform.position.y, 0f);

                var go = new GameObject($"BossMinion_{i}");
                go.transform.position = spawnPos;
                go.tag = "Enemy";

                var sr = go.AddComponent<SpriteRenderer>();
                int era = (int)Type;
                sr.sprite = PlaceholderAssets.GetEnemySprite((EnemyType)(era * 3), EnemyBehavior.Chase);
                sr.sortingOrder = 9;
                sr.color = new Color(1f, 0.8f, 0.8f); // Slightly red tint
                go.transform.localScale = Vector3.one * 0.7f; // Smaller than normal

                var rb = go.AddComponent<Rigidbody2D>();
                rb.gravityScale = 3f;
                rb.freezeRotation = true;

                var col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.8f, 1f);
                col.offset = new Vector2(0f, 0.5f);

                var enemy = go.AddComponent<EnemyBase>();
                var data = new EnemyData
                {
                    Type = (EnemyType)(era * 3),
                    Behavior = EnemyBehavior.Chase,
                    TileX = (int)spawnX,
                    TileY = 0,
                    PatrolMinX = (int)_arenaMinX + 2,
                    PatrolMaxX = (int)_arenaMaxX - 2,
                    DifficultyWeight = 0.5f // Weaker than normal enemies
                };
                enemy.Initialize(data, _tilemapRenderer);

                // Spawn flash
                var flash = ObjectPool.GetFlash();
                flash.transform.position = spawnPos;
                var fsr = flash.GetComponent<SpriteRenderer>();
                fsr.sprite = PlaceholderAssets.GetParticleSprite();
                fsr.color = new Color(1f, 1f, 0.5f, 0.8f);
                fsr.sortingOrder = 16;
                flash.transform.localScale = Vector3.one * 2f;
                flash.GetComponent<PoolTimer>().StartTimer(0.2f);
            }
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

            var projGO = ObjectPool.GetProjectile();
            projGO.transform.position = spawnPos;

            var sr = projGO.GetComponent<SpriteRenderer>();
            int era = (int)Type;
            sr.sprite = PlaceholderAssets.GetProjectileSprite(WeaponTier.Heavy, era);
            sr.color = new Color(1f, 0.4f, 0.9f); // Purple-pink tint for boss
            sr.sortingOrder = 11;
            projGO.transform.localScale = new Vector3(1.5f, 1.5f, 1f); // Larger projectiles

            var col = projGO.GetComponent<CircleCollider2D>();
            col.radius = 0.3f;

            var proj = projGO.GetComponent<Projectile>();
            proj.Initialize(direction, 5f, 2, true); // 2 damage for boss projectiles
        }

        /// <summary>
        /// Apply a slow effect. Bosses resist slow somewhat (caller should reduce factor).
        /// </summary>
        public void ApplySlow(float factor, float duration)
        {
            _slowFactor = Mathf.Min(_slowFactor, factor);
            _slowTimer = Mathf.Max(_slowTimer, duration);
            _moveSpeed = _baseMoveSpeed * _phaseSpeedMultiplier * _slowFactor;
            _chargeSpeed = _baseChargeSpeed * _phaseSpeedMultiplier * _slowFactor;

            // Blue-purple tint to indicate slowed
            _baseColor = new Color(0.6f, 0.5f, 1f);
        }

        private void UpdateSlow()
        {
            if (_slowTimer <= 0f) return;

            _slowTimer -= Time.fixedDeltaTime;
            if (_slowTimer <= 0f)
            {
                _slowFactor = 1f;
                _moveSpeed = _baseMoveSpeed * _phaseSpeedMultiplier;
                _chargeSpeed = _baseChargeSpeed * _phaseSpeedMultiplier;
                _baseColor = Color.white;
            }
        }

        public void TakeDamage(int amount, float dpsCapBypass = 0f)
        {
            // Boss is immune to damage until activated
            if (IsDead || !IsActive) return;

            // Phase transition invulnerability
            if (_phaseInvulnTimer > 0f) return;

            // Sheltered behind pillar: immune to damage
            if (_isSheltered && _currentPillar != null && !_currentPillar.IsDestroyed)
                return;

            // Slowed bosses take 20% more damage from all sources
            if (_slowTimer > 0f)
                amount = Mathf.CeilToInt(amount * 1.2f);

            // DPS cap: ignore excess damage beyond MAX_DPS per second
            // Cannon bypasses a portion of the cap (dpsCapBypass = 0.3 = 30%)
            float effectiveCap = MAX_DPS + MAX_DPS * dpsCapBypass;
            if (_recentDamage >= effectiveCap)
            {
                LastDpsCapTime = Time.time;
                return;
            }
            float remaining = effectiveCap - _recentDamage;
            amount = Mathf.Min(amount, Mathf.CeilToInt(remaining));
            if (amount <= 0) return;
            _recentDamage += amount;

            Health -= amount;

            // Floating damage number (shows actual capped damage)
            bool isKillShot = Health <= 0;
            Color dmgColor = isKillShot ? new Color(1f, 0.2f, 0.2f)
                           : dpsCapBypass > 0f ? new Color(1f, 0.7f, 0.3f) // Orange for heavy/cannon
                           : Color.white;
            GameManager.ShowDamagePopup(transform.position, amount, dmgColor);

            AudioManager.PlaySFX(PlaceholderAudio.GetEnemyHitSFX());

            // Screen shake scales with damage
            CameraController.Instance?.AddTrauma(0.05f * amount);

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
                Color phaseColor = new Color(1f, tint, tint);
                // Blend phase color with slow tint
                _sr.color = _slowTimer > 0f ? Color.Lerp(phaseColor, _baseColor, 0.5f) : phaseColor;
            }
        }

        private void Die()
        {
            IsDead = true;

            if (GameManager.Instance != null)
            {
                // Record boss defeat for scoring and achievements
                GameManager.Instance.RecordBossDefeated();

                // Boss kill awards score directly (500 bonus) without inflating kill streak
                GameManager.Instance.EnemiesKilled++;
                GameManager.Instance.RecordBossKillScore(500);
            }

            // Epic boss death: strong shake + hit-stop + dramatic time-slow
            AudioManager.PlaySFX(PlaceholderAudio.GetBossDeathSFX());
            CameraController.Instance?.AddTrauma(1.0f);
            GameManager.HitStop(0.133f);
            StartCoroutine(BossDeathTimeSlow());

            // Remove arena wall and unlock camera
            var trigger = FindAnyObjectByType<BossArenaTrigger>();
            trigger?.RemoveArenaWall();

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

            // Activate exit portal after boss death (camera pan + unlock)
            var portal = ExitPortal.Instance;
            if (portal != null && !portal.IsActive)
            {
                var cam = CameraController.Instance;
                if (cam != null)
                {
                    cam.PanToPosition(portal.transform.position, 1.0f, 0.5f, () =>
                    {
                        portal.Activate();
                    });
                }
                else
                {
                    portal.Activate();
                }
            }

            // Death particle burst — scatter particles outward from boss position
            SpawnDeathParticles(originalPos);

            Destroy(gameObject);
        }

        private void SpawnDeathParticles(Vector3 center)
        {
            int count = 12;
            for (int i = 0; i < count; i++)
            {
                var go = ObjectPool.GetParticle();
                go.transform.position = center;

                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetParticleSprite();
                float hue = (float)i / count;
                sr.color = Color.HSVToRGB(hue, 0.5f, 1f);
                sr.sortingOrder = 15;
                go.transform.localScale = Vector3.one * Random.Range(0.5f, 1.2f);

                // Scatter outward
                float angle = (360f * i / count) + Random.Range(-15f, 15f);
                float rad = angle * Mathf.Deg2Rad;
                float speed = Random.Range(4f, 10f);
                var rb = go.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.linearVelocity = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * speed;

                go.GetComponent<PoolTimer>().StartTimer(0.6f);
            }
        }

        private System.Collections.IEnumerator BossDeathTimeSlow()
        {
            Time.timeScale = 0.5f;
            yield return new WaitForSecondsRealtime(0.3f);
            Time.timeScale = 1f;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // No collision effects when dead or inactive
            if (IsDead || !IsActive) return;

            // Boss damages player on contact (with cooldown to prevent rapid re-hits)
            if (collision.gameObject.CompareTag("Player"))
            {
                if (Time.time - _lastContactDamageTime < CONTACT_DAMAGE_INTERVAL) return;
                _lastContactDamageTime = Time.time;

                var health = collision.gameObject.GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.TakeDamage(2, transform.position); // Boss does 2 damage on contact
                }
            }
            // Reverse direction when hitting a wall/obstacle (prevents getting stuck)
            else
            {
                // Don't reverse on pillar contact — boss should walk through/past pillars
                if (collision.gameObject.GetComponent<ArenaPillar>() != null) return;
                _direction = -_direction;
                if (_isCharging) EndCharge();
            }
        }
    }
}
