using UnityEngine;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Manages tutorial progression across 3 tutorial levels AND context-sensitive
    /// gameplay hints (Sprint 9). Tutorial hints use scripted steps; gameplay hints
    /// monitor player behavior and show contextual reminders.
    /// Per-hint skip tracking persisted in PlayerPrefs.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        private const string PREF_KEY = "EpochBreaker_TutorialCompleted";
        private const string PREF_HINT_DISMISSED_PREFIX = "EpochBreaker_HintDismissed_";

        public static TutorialManager Instance { get; private set; }

        public bool IsTutorialActive { get; private set; }
        public int CurrentTutorialLevel { get; private set; } // 0, 1, 2
        public int CurrentStep { get; private set; }
        public string CurrentHintText { get; private set; } = "";
        public bool HintVisible { get; private set; }

        // Sprint 9: Context-sensitive gameplay hint state
        /// <summary>Text for the context-sensitive gameplay hint (separate from tutorial hints).</summary>
        public string GameplayHintText { get; private set; } = "";
        /// <summary>Whether a context-sensitive gameplay hint is currently visible.</summary>
        public bool GameplayHintVisible { get; private set; }
        private float _gameplayHintFadeTimer;
        private const float GAMEPLAY_HINT_DURATION = 3f;

        // Player behavior tracking for context hints
        private Vector3 _lastPlayerPos;
        private float _stuckTimer;           // Time player hasn't moved significantly
        private float _nearWallTimer;        // Time player has been near a wall without wall-jumping
        private float _noCombatTimer;        // Time without attacking during combat
        private bool _wallJumpHintShown;
        private bool _attackHintShown;
        private bool _stuckHintShown;
        private bool _weaponCycleHintShown;
        private int _lastWallJumpCount;
        private int _lastWeaponsCollected;

        private TutorialStep[] _steps;
        private float _hintTimer;
        private float _stepTimer;
        private bool _waitingForAction;
        private GameObject _cachedPlayer;
        private PlayerController _cachedPlayerController;

        public static bool IsTutorialCompleted()
        {
            return PlayerPrefs.GetInt(PREF_KEY, 0) == 1;
        }

        public static void SetTutorialCompleted()
        {
            PlayerPrefs.SetInt(PREF_KEY, 1);
            PlayerPrefs.Save();
        }

        public static void ResetTutorial()
        {
            PlayerPrefs.DeleteKey(PREF_KEY);
            PlayerPrefs.Save();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[Singleton] TutorialManager duplicate detected — destroying new instance.");
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// Start the tutorial sequence from level 0.
        /// </summary>
        public void StartTutorial()
        {
            IsTutorialActive = true;
            CurrentTutorialLevel = 0;
            CurrentStep = 0;
            LoadTutorialSteps();
        }

        /// <summary>
        /// Advance to the next tutorial level after completing the current one.
        /// Returns true if there's another level, false if tutorial is complete.
        /// </summary>
        public bool AdvanceToNextLevel()
        {
            CurrentTutorialLevel++;
            CurrentStep = 0;

            if (CurrentTutorialLevel > 2)
            {
                CompleteTutorial();
                return false;
            }

            LoadTutorialSteps();
            return true;
        }

        /// <summary>
        /// Skip the tutorial entirely.
        /// </summary>
        public void SkipTutorial()
        {
            CompleteTutorial();
        }

        private void CompleteTutorial()
        {
            IsTutorialActive = false;
            HintVisible = false;
            CurrentHintText = "";
            SetTutorialCompleted();
        }

        private void Update()
        {
            // Fade out gameplay hints
            if (GameplayHintVisible)
            {
                _gameplayHintFadeTimer -= Time.deltaTime;
                if (_gameplayHintFadeTimer <= 0f)
                {
                    GameplayHintVisible = false;
                    GameplayHintText = "";
                }
            }

            // Run context-sensitive gameplay hints (Sprint 9) when playing but not in tutorial
            if (!IsTutorialActive && GameManager.Instance != null &&
                GameManager.Instance.CurrentState == GameState.Playing)
            {
                UpdateContextHints();
            }

            if (!IsTutorialActive || _steps == null || CurrentStep >= _steps.Length)
                return;

            var step = _steps[CurrentStep];
            _stepTimer += Time.deltaTime;

            // Show hint after delay
            if (!HintVisible && _stepTimer >= step.ShowDelay)
            {
                ShowHint(step);
            }

            // Check if step trigger condition is met
            if (CheckStepCondition(step))
            {
                HintVisible = false;
                CurrentStep++;
                _stepTimer = 0f;

                if (CurrentStep >= _steps.Length)
                {
                    // All steps done for this tutorial level
                    HintVisible = false;
                    CurrentHintText = "";
                }
            }
        }

        // =====================================================================
        // Context-Sensitive Gameplay Hints (Sprint 9)
        // =====================================================================

        /// <summary>
        /// Monitor player behavior and show contextual reminders.
        /// </summary>
        private void UpdateContextHints()
        {
            if (_cachedPlayer == null)
            {
                _cachedPlayer = GameObject.FindWithTag("Player");
                if (_cachedPlayer != null)
                    _cachedPlayerController = _cachedPlayer.GetComponent<PlayerController>();
            }
            if (_cachedPlayer == null || _cachedPlayerController == null) return;

            Vector3 currentPos = _cachedPlayer.transform.position;

            // --- Stuck detection: same position for 8s ---
            float posDiff = (currentPos - _lastPlayerPos).sqrMagnitude;
            if (posDiff < 0.5f && !_cachedPlayerController.IsWallSliding) // Less than ~0.7 units moved
            {
                _stuckTimer += Time.deltaTime;
            }
            else
            {
                _stuckTimer = 0f;
                _stuckHintShown = false;
            }

            if (_stuckTimer >= 8f && !_stuckHintShown && !GameplayHintVisible)
            {
                if (!IsHintDismissed("stuck"))
                {
                    ShowGameplayHint("Look for another path!");
                    _stuckHintShown = true;
                }
            }

            // --- Near wall without wall-jumping for 5s ---
            bool isNearWall = _cachedPlayerController.IsWallSliding;
            if (isNearWall)
            {
                _nearWallTimer += Time.deltaTime;
            }
            else
            {
                _nearWallTimer = 0f;
            }

            int currentWallJumps = _cachedPlayerController.WallJumpsThisLevel;
            if (currentWallJumps > _lastWallJumpCount)
            {
                _nearWallTimer = 0f;
                _wallJumpHintShown = true; // Player knows about wall jumping
                _lastWallJumpCount = currentWallJumps;
            }

            if (_nearWallTimer >= 5f && !_wallJumpHintShown && !GameplayHintVisible)
            {
                if (!IsHintDismissed("walljump"))
                {
                    ShowGameplayHint("Try wall-jumping!");
                    _wallJumpHintShown = true;
                }
            }

            // --- No weapon use during combat for 10s ---
            bool enemiesNearby = EnemyBase.ActiveEnemies.Count > 0;
            if (enemiesNearby)
            {
                _noCombatTimer += Time.deltaTime;
                if (GameManager.Instance.ShotsFired > 0 || GameManager.Instance.EnemiesKilled > 0)
                {
                    _noCombatTimer = 0f;
                    _attackHintShown = true;
                }
            }
            else
            {
                _noCombatTimer = 0f;
            }

            if (_noCombatTimer >= 10f && !_attackHintShown && !GameplayHintVisible)
            {
                if (!IsHintDismissed("attack"))
                {
                    ShowGameplayHint("Your weapon fires automatically at enemies!");
                    _attackHintShown = true;
                }
            }

            // --- Weapon cycling hint: on 2nd weapon pickup ---
            if (!_weaponCycleHintShown && !GameplayHintVisible &&
                GameManager.Instance != null && GameManager.Instance.WeaponsCollected >= 2 &&
                GameManager.Instance.WeaponsCollected > _lastWeaponsCollected)
            {
                _lastWeaponsCollected = GameManager.Instance.WeaponsCollected;
                if (!IsHintDismissed("weaponcycle"))
                {
                    bool isMobile = Application.isMobilePlatform;
                    string hint = isMobile
                        ? "Tap R to cycle weapons for a Quick Draw boost!"
                        : "Press R to cycle weapons for a Quick Draw boost!";
                    ShowGameplayHint(hint);
                    _weaponCycleHintShown = true;
                }
            }

            _lastPlayerPos = currentPos;
        }

        /// <summary>
        /// Show a context-sensitive gameplay hint popup (public, for external callers like Boss/HazardSystem).
        /// </summary>
        public void ShowGameplayHintPublic(string text)
        {
            ShowGameplayHint(text);
        }

        /// <summary>
        /// Show a context-sensitive gameplay hint popup. Fades after GAMEPLAY_HINT_DURATION seconds.
        /// </summary>
        private void ShowGameplayHint(string text)
        {
            GameplayHintText = text;
            GameplayHintVisible = true;
            _gameplayHintFadeTimer = GAMEPLAY_HINT_DURATION;
        }

        /// <summary>
        /// Reset context hint state when starting a new level.
        /// </summary>
        public void ResetContextHints()
        {
            _stuckTimer = 0f;
            _nearWallTimer = 0f;
            _noCombatTimer = 0f;
            _wallJumpHintShown = false;
            _attackHintShown = false;
            _stuckHintShown = false;
            _weaponCycleHintShown = false;
            _lastWallJumpCount = 0;
            _lastWeaponsCollected = 0;
            _lastPlayerPos = Vector3.zero;
            _cachedPlayer = null;
            _cachedPlayerController = null;
            GameplayHintVisible = false;
            GameplayHintText = "";
        }

        /// <summary>
        /// Dismiss a specific hint permanently so it won't show again.
        /// </summary>
        public static void DismissHint(string hintId)
        {
            PlayerPrefs.SetInt(PREF_HINT_DISMISSED_PREFIX + hintId, 1);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Check if a specific hint has been permanently dismissed.
        /// </summary>
        public static bool IsHintDismissed(string hintId)
        {
            return PlayerPrefs.GetInt(PREF_HINT_DISMISSED_PREFIX + hintId, 0) == 1;
        }

        /// <summary>
        /// Reset all dismissed hints (for settings menu "Reset Hints" option).
        /// </summary>
        public static void ResetDismissedHints()
        {
            string[] hintIds = { "stuck", "walljump", "attack", "weaponcycle", "hazard", "pillarshelter" };
            foreach (var id in hintIds)
            {
                PlayerPrefs.DeleteKey(PREF_HINT_DISMISSED_PREFIX + id);
            }
            PlayerPrefs.Save();
        }

        private void ShowHint(TutorialStep step)
        {
            HintVisible = true;
            // Detect platform and show appropriate controls
            bool isMobile = Application.isMobilePlatform;
            CurrentHintText = isMobile ? step.TouchHint : step.KeyboardHint;
        }

        private bool CheckStepCondition(TutorialStep step)
        {
            if (_cachedPlayer == null)
            {
                _cachedPlayer = GameObject.FindWithTag("Player");
                if (_cachedPlayer != null)
                    _cachedPlayerController = _cachedPlayer.GetComponent<PlayerController>();
            }
            if (_cachedPlayer == null) return false;

            switch (step.Trigger)
            {
                case TutorialTrigger.PlayerReachedX:
                    return _cachedPlayer.transform.position.x >= step.TriggerValue;

                case TutorialTrigger.EnemyKilled:
                    return GameManager.Instance != null &&
                           GameManager.Instance.EnemiesKilled >= (int)step.TriggerValue;

                case TutorialTrigger.BlockDestroyed:
                    return GameManager.Instance != null &&
                           GameManager.Instance.BlocksDestroyed >= (int)step.TriggerValue;

                case TutorialTrigger.WeaponPickedUp:
                    return GameManager.Instance != null &&
                           GameManager.Instance.WeaponsCollected >= (int)step.TriggerValue;

                case TutorialTrigger.TimeElapsed:
                    return _stepTimer >= step.TriggerValue;

                case TutorialTrigger.WallJumpPerformed:
                    return _cachedPlayerController != null &&
                           _cachedPlayerController.WallJumpsThisLevel >= (int)step.TriggerValue;

                default:
                    return _stepTimer >= 5f; // Fallback: auto-advance after 5s
            }
        }

        private void LoadTutorialSteps()
        {
            _stepTimer = 0f;
            HintVisible = false;

            _steps = CurrentTutorialLevel switch
            {
                0 => GetLevel1Steps(),
                1 => GetLevel2Steps(),
                2 => GetLevel3Steps(),
                _ => new TutorialStep[0]
            };
        }

        private static TutorialStep[] GetLevel1Steps()
        {
            return new TutorialStep[]
            {
                new TutorialStep
                {
                    KeyboardHint = "Use A/D or Arrow Keys to move",
                    TouchHint = "Tap Left/Right buttons to move",
                    Trigger = TutorialTrigger.PlayerReachedX,
                    TriggerValue = 8f,
                    ShowDelay = 0.5f,
                },
                new TutorialStep
                {
                    KeyboardHint = "Press W or Space to jump",
                    TouchHint = "Tap the A button to jump",
                    Trigger = TutorialTrigger.PlayerReachedX,
                    TriggerValue = 18f,
                    ShowDelay = 0.5f,
                },
                new TutorialStep
                {
                    KeyboardHint = "Jump across wider gaps!",
                    TouchHint = "Jump across wider gaps!",
                    Trigger = TutorialTrigger.PlayerReachedX,
                    TriggerValue = 28f,
                    ShowDelay = 0.5f,
                },
                new TutorialStep
                {
                    KeyboardHint = "Use platforms to reach high places",
                    TouchHint = "Use platforms to reach high places",
                    Trigger = TutorialTrigger.PlayerReachedX,
                    TriggerValue = 52f,
                    ShowDelay = 0.5f,
                },
                new TutorialStep
                {
                    KeyboardHint = "Reach the portal to complete the level!",
                    TouchHint = "Reach the portal to complete the level!",
                    Trigger = TutorialTrigger.PlayerReachedX,
                    TriggerValue = 58f,
                    ShowDelay = 0.5f,
                },
            };
        }

        private static TutorialStep[] GetLevel2Steps()
        {
            return new TutorialStep[]
            {
                new TutorialStep
                {
                    KeyboardHint = "Your weapon fires automatically at enemies and blocks!",
                    TouchHint = "Your weapon fires automatically at enemies and blocks!",
                    Trigger = TutorialTrigger.PlayerReachedX,
                    TriggerValue = 12f,
                    ShowDelay = 0.5f,
                },
                new TutorialStep
                {
                    KeyboardHint = "Shoot these blocks to break through!",
                    TouchHint = "Shoot these blocks to break through!",
                    Trigger = TutorialTrigger.BlockDestroyed,
                    TriggerValue = 1f,
                    ShowDelay = 0.5f,
                },
                new TutorialStep
                {
                    KeyboardHint = "Press S while airborne to stomp down!",
                    TouchHint = "Tap the Down button while airborne to stomp!",
                    Trigger = TutorialTrigger.PlayerReachedX,
                    TriggerValue = 38f,
                    ShowDelay = 0.5f,
                },
                new TutorialStep
                {
                    KeyboardHint = "Pick up weapons to upgrade your firepower!",
                    TouchHint = "Pick up weapons to upgrade your firepower!",
                    Trigger = TutorialTrigger.WeaponPickedUp,
                    TriggerValue = 1f,
                    ShowDelay = 0.5f,
                },
                new TutorialStep
                {
                    KeyboardHint = "Upgraded weapons break tougher blocks. Keep going!",
                    TouchHint = "Upgraded weapons break tougher blocks. Keep going!",
                    Trigger = TutorialTrigger.PlayerReachedX,
                    TriggerValue = 72f,
                    ShowDelay = 0.5f,
                },
            };
        }

        private static TutorialStep[] GetLevel3Steps()
        {
            return new TutorialStep[]
            {
                new TutorialStep
                {
                    KeyboardHint = "Watch out for enemies ahead!",
                    TouchHint = "Watch out for enemies ahead!",
                    Trigger = TutorialTrigger.PlayerReachedX,
                    TriggerValue = 15f,
                    ShowDelay = 0.5f,
                },
                // Wall-jump hint: optional side path. Only shows if player lingers (4s delay).
                // Auto-advances when player passes the chimney area regardless.
                new TutorialStep
                {
                    KeyboardHint = "See that ledge? Try jumping into the wall to climb!",
                    TouchHint = "See that ledge? Try jumping into the wall to climb!",
                    Trigger = TutorialTrigger.PlayerReachedX,
                    TriggerValue = 26f,
                    ShowDelay = 4.0f, // Only shows if player lingers
                },
                new TutorialStep
                {
                    KeyboardHint = "Heavy weapons break hard blocks. Grab the pickup!",
                    TouchHint = "Heavy weapons break hard blocks. Grab the pickup!",
                    Trigger = TutorialTrigger.WeaponPickedUp,
                    TriggerValue = 1f,
                    ShowDelay = 0.5f,
                },
                new TutorialStep
                {
                    KeyboardHint = "Watch out! Don't break the floor you're standing on!",
                    TouchHint = "Watch out! Don't break the floor you're standing on!",
                    Trigger = TutorialTrigger.PlayerReachedX,
                    TriggerValue = 75f,
                    ShowDelay = 0.5f,
                },
                new TutorialStep
                {
                    KeyboardHint = "Defeat the enemies and reach the portal!",
                    TouchHint = "Defeat the enemies and reach the portal!",
                    Trigger = TutorialTrigger.PlayerReachedX,
                    TriggerValue = 88f,
                    ShowDelay = 0.5f,
                },
            };
        }
    }

    public enum TutorialTrigger
    {
        PlayerReachedX,
        EnemyKilled,
        BlockDestroyed,
        WeaponPickedUp,
        TimeElapsed,
        WallJumpPerformed,
    }

    public struct TutorialStep
    {
        public string KeyboardHint;
        public string TouchHint;
        public TutorialTrigger Trigger;
        public float TriggerValue;
        public float ShowDelay;
    }
}
