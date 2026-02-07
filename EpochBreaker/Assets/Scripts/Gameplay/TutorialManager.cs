using UnityEngine;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Manages tutorial progression across 3 tutorial levels.
    /// Tracks which tutorial step the player is on and triggers hints.
    /// Persists completion state to PlayerPrefs.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        private const string PREF_KEY = "EpochBreaker_TutorialCompleted";

        public static TutorialManager Instance { get; private set; }

        public bool IsTutorialActive { get; private set; }
        public int CurrentTutorialLevel { get; private set; } // 0, 1, 2
        public int CurrentStep { get; private set; }
        public string CurrentHintText { get; private set; } = "";
        public bool HintVisible { get; private set; }

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
