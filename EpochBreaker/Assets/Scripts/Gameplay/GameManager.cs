using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    public enum GameState
    {
        TitleScreen,
        Loading,
        Playing,
        Paused,
        LevelComplete,
        GameOver,
        Celebration
    }

    public enum GameMode
    {
        FreePlay,   // Single level from code entry or randomize on
        Campaign,   // 10 epochs in order, limited lives, extra life pickups
        Streak      // Unlimited random levels, 10 lives, no extra life pickups
    }

    public enum DeathResult
    {
        Respawn,     // Still have deaths left this level
        LevelFailed, // Used all deaths this level, advance to next
        GameOver     // No lives remaining
    }

    /// <summary>
    /// Stores data about a played level for history display.
    /// </summary>
    [Serializable]
    public class LevelHistoryEntry
    {
        public string Code;
        public int Epoch;
        public string EpochName;
        public int Score;
        public int Stars;
        public long PlayedTimestamp; // Unix timestamp
        public string Source; // Origin: Campaign, Code, Challenge, Daily, Weekly
    }

    /// <summary>
    /// Container for serializing level history to JSON.
    /// </summary>
    [Serializable]
    public class LevelHistoryData
    {
        public List<LevelHistoryEntry> Entries = new List<LevelHistoryEntry>();
    }

    /// <summary>
    /// Saved session data for Continue functionality.
    /// Persisted to PlayerPrefs so the player can resume after quitting.
    /// </summary>
    [Serializable]
    public class SavedSession
    {
        public string LevelCode;
        public int Mode; // 0=FreePlay, 1=Campaign, 2=Streak
        public int CampaignEpoch;
        public int StreakCount;
        public int GlobalLives;
        public int TotalScore;
    }

    /// <summary>
    /// A single entry in the Legends leaderboard (best streak runs).
    /// </summary>
    [Serializable]
    public class LegendsEntry
    {
        public int StreakCount;
        public long Timestamp;
    }

    /// <summary>
    /// Container for serializing Legends data to JSON.
    /// </summary>
    [Serializable]
    public class LegendsData
    {
        public List<LegendsEntry> Entries = new List<LegendsEntry>();
    }

    /// <summary>
    /// Singleton game manager. Controls game state, level progression, and
    /// coordinates all major systems. Auto-creates itself via RuntimeInitializeOnLoadMethod.
    /// Runs before other scripts to ensure input flags are set before consumption.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // Hit-stop: brief time freeze on impactful events
        private Coroutine _hitStopCoroutine;
        private float _savedTimeScale = 1f;

        /// <summary>
        /// Freeze time briefly for impact emphasis. Duration is in real (unscaled) seconds.
        /// Typical values: enemy kill 0.033s, boss phase 0.066s, boss death 0.133s.
        /// </summary>
        public static void HitStop(float duration)
        {
            if (Instance == null) return;
            // Don't hit-stop if already paused or not playing
            if (Instance.CurrentState == GameState.Paused) return;
            if (Instance._hitStopCoroutine != null)
                Instance.StopCoroutine(Instance._hitStopCoroutine);
            Instance._hitStopCoroutine = Instance.StartCoroutine(Instance.DoHitStop(duration));
        }

        private System.Collections.IEnumerator DelayedCameraIntro()
        {
            yield return null; // Wait one frame for physics to settle
            CameraController.Instance?.StartLevelIntro();
        }

        private System.Collections.IEnumerator DoHitStop(float duration)
        {
            _savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            // Restore — but only if not paused during the hit-stop
            if (CurrentState != GameState.Paused)
                Time.timeScale = _savedTimeScale;
            _hitStopCoroutine = null;
        }

        public GameState CurrentState { get; private set; } = GameState.TitleScreen;
        public int CurrentEpoch { get; set; } = 0;
        public const int DEATHS_PER_LEVEL = 2;
        public int LivesRemaining { get; private set; } = DEATHS_PER_LEVEL;
        public int Score { get; set; }
        public int TotalScore { get; set; }
        public int TimeScore { get; private set; }
        public int ItemBonusScore { get; private set; }
        public int EnemyBonusScore { get; private set; }
        public float LevelElapsedTime { get; private set; }
        public int LevelNumber { get; private set; }
        public int EnemiesKilled { get; set; }
        public int RewardsCollected { get; set; }
        public int WeaponsCollected { get; set; }
        public LevelData CurrentLevel { get; set; }
        public LevelID CurrentLevelID { get; set; }

        // Game mode tracking
        public GameMode CurrentGameMode { get; private set; } = GameMode.FreePlay;
        public int CampaignEpoch { get; private set; } = 0;  // 0-9, which epoch in campaign
        public int StreakCount { get; private set; } = 0;     // Levels completed in streak mode
        public int DeathsThisLevel { get; private set; } = 0; // Deaths in current level
        public int GlobalLives { get; private set; } = 2;     // Persistent lives across levels

        // Sprint 8: Social & Retention
        public int FriendChallengeScore { get; private set; } = 0;  // Friend's target score (0 = no challenge)
        public bool IsGhostReplay { get; private set; } = false;    // True if replaying with ghost overlay
        public string LevelSource { get; private set; } = "";       // Origin tracking for history

        // Settings
        private const string PREF_RANDOMIZE = "EpochBreaker_Randomize";
        private const string PREF_LEGENDS_UNLOCKED = "EpochBreaker_LegendsUnlocked";
        private const string LEGENDS_PREFS_KEY = "EpochBreaker_Legends";
        private const int MAX_LEGENDS_ENTRIES = 20;

        public bool RandomizeEnabled
        {
            get => PlayerPrefs.GetInt(PREF_RANDOMIZE, 0) == 1;
            set { PlayerPrefs.SetInt(PREF_RANDOMIZE, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public bool LegendsUnlocked
        {
            get => PlayerPrefs.GetInt(PREF_LEGENDS_UNLOCKED, 0) == 1;
            set { PlayerPrefs.SetInt(PREF_LEGENDS_UNLOCKED, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        // Session save/load
        private const string SESSION_PREFS_KEY = "EpochBreaker_Session";

        /// <summary>
        /// Check if a saved session exists that the player can continue.
        /// </summary>
        public static bool HasSavedSession()
        {
            return !string.IsNullOrEmpty(PlayerPrefs.GetString(SESSION_PREFS_KEY, ""));
        }

        /// <summary>
        /// Load saved session data. Returns null if no session exists.
        /// </summary>
        public static SavedSession LoadSession()
        {
            string json = PlayerPrefs.GetString(SESSION_PREFS_KEY, "");
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                return JsonUtility.FromJson<SavedSession>(json);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Save the current game session so the player can continue later.
        /// </summary>
        public void SaveSession()
        {
            var session = new SavedSession
            {
                LevelCode = CurrentLevelID.ToCode(),
                Mode = (int)CurrentGameMode,
                CampaignEpoch = CampaignEpoch,
                StreakCount = StreakCount,
                GlobalLives = GlobalLives,
                TotalScore = TotalScore
            };
            string json = JsonUtility.ToJson(session);
            PlayerPrefs.SetString(SESSION_PREFS_KEY, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Clear the saved session (called on Game Over, campaign completion, or new game).
        /// </summary>
        public static void ClearSession()
        {
            PlayerPrefs.DeleteKey(SESSION_PREFS_KEY);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Continue from a saved session.
        /// </summary>
        public void ContinueSession()
        {
            var session = LoadSession();
            if (session == null) return;

            if (!LevelID.TryParse(session.LevelCode, out LevelID levelID))
            {
                ClearSession();
                return;
            }

            CurrentLevelID = levelID;
            CurrentEpoch = levelID.Epoch;
            CurrentGameMode = (GameMode)session.Mode;
            CampaignEpoch = session.CampaignEpoch;
            StreakCount = session.StreakCount;
            GlobalLives = session.GlobalLives;
            TotalScore = session.TotalScore;
            Score = 0;
            LevelNumber = 0;
            LevelElapsedTime = 0f;
            _timerRunning = false;
            DeathsThisLevel = 0;

            TransitionTo(GameState.Loading);
        }

        // Per-type collection counts for multiplier bonuses (exposed for UI display)
        public Dictionary<RewardType, int> RewardCounts => _rewardCounts;
        public Dictionary<WeaponTier, int> WeaponCounts => _weaponCounts;
        private Dictionary<RewardType, int> _rewardCounts = new Dictionary<RewardType, int>();
        private Dictionary<WeaponTier, int> _weaponCounts = new Dictionary<WeaponTier, int>();

        // Combat tracking
        public int ShotsFired { get; private set; }

        // Combo system: kills within 2s build a multiplier, reset on damage
        public int ComboCount { get; private set; }
        private float _comboTimer;
        private const float COMBO_WINDOW = 2f;

        // Score popup event: (world position, score value)
        public static event Action<Vector3, int> OnScorePopup;

        /// <summary>
        /// Show a floating score popup at a world position.
        /// </summary>
        public static void ShowScorePopup(Vector3 worldPos, int score)
        {
            OnScorePopup?.Invoke(worldPos, score);
        }

        // Damage direction event: (damage source world position)
        public static event Action<Vector2> OnDamageDirection;

        /// <summary>
        /// Notify UI of damage direction for edge flash indicator.
        /// </summary>
        public static void NotifyDamageDirection(Vector2 damageSourceWorldPos)
        {
            OnDamageDirection?.Invoke(damageSourceWorldPos);
        }

        private bool _timerRunning;

        private LevelLoader _levelLoader;
        private GameObject _titleScreenObj;
        private GameObject _hudObj;
        private GameObject _pauseMenuObj;
        private GameObject _levelCompleteObj;
        private float _levelCompleteTime;
        private const float LevelCompleteMinDelay = 5f;
        private GameObject _gameOverObj;
        private GameObject _touchControlsObj;
        private GameObject _celebrationObj;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoCreate()
        {
            if (Instance != null) return;
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[Singleton] GameManager duplicate detected — destroying new instance.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _levelLoader = gameObject.AddComponent<LevelLoader>();
            gameObject.AddComponent<AudioManager>();
            gameObject.AddComponent<AchievementManager>();
            gameObject.AddComponent<CosmeticManager>();
            gameObject.AddComponent<TutorialManager>();
            gameObject.AddComponent<AccessibilityManager>();
            gameObject.AddComponent<DifficultyManager>();

            // Persistent background camera — prevents "No cameras rendering" when
            // scene cameras are destroyed during transitions back to title screen
            if (Camera.main == null && FindAnyObjectByType<Camera>() == null)
            {
                var camGO = new GameObject("BackgroundCamera");
                camGO.transform.SetParent(transform); // child of GameManager (DontDestroyOnLoad)
                var cam = camGO.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.08f, 0.06f, 0.15f); // deep purple
                cam.depth = -100; // always behind scene cameras
                cam.cullingMask = 0; // render nothing, just clear the screen
            }

            // Ensure EventSystem exists for UI interaction (using new Input System)
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<InputSystemUIInputModule>();
                DontDestroyOnLoad(esGO);
            }
        }

        private void Start()
        {
            TransitionTo(GameState.TitleScreen);
        }

        private void Update()
        {
            // Read input every frame (runs first due to DefaultExecutionOrder)
            InputManager.UpdateInput();

            switch (CurrentState)
            {
                case GameState.TitleScreen:
                    // Start game with Enter, Space, or Jump action
                    // Skip if an InputField or other UI element has focus
                    if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
                        break;
                    if (Keyboard.current != null &&
                        (Keyboard.current.enterKey.wasPressedThisFrame ||
                         Keyboard.current.numpadEnterKey.wasPressedThisFrame ||
                         Keyboard.current.spaceKey.wasPressedThisFrame))
                    {
                        if (HasSavedSession())
                            ContinueSession();
                        else
                            StartGame();
                    }
                    break;
                case GameState.Playing:
                    if (_timerRunning)
                        LevelElapsedTime += Time.deltaTime;
                    // Combo decay
                    if (_comboTimer > 0f)
                    {
                        _comboTimer -= Time.deltaTime;
                        if (_comboTimer <= 0f)
                            ComboCount = 0;
                    }
                    if (InputManager.PausePressed)
                        PauseGame();
                    break;
                case GameState.Paused:
                    if (InputManager.PausePressed)
                        ResumeGame();
                    break;
                case GameState.LevelComplete:
                    if (Time.unscaledTime - _levelCompleteTime >= LevelCompleteMinDelay &&
                        Keyboard.current != null)
                    {
                        if (Keyboard.current.enterKey.wasPressedThisFrame ||
                            Keyboard.current.numpadEnterKey.wasPressedThisFrame ||
                            Keyboard.current.spaceKey.wasPressedThisFrame)
                            NextLevel();
                        else if (Keyboard.current.rKey.wasPressedThisFrame)
                            RestartLevel();
                        else if (InputManager.IsBackPressed())
                            ReturnToTitle();
                    }
                    break;
            }
        }

        private void LateUpdate()
        {
            // Clear single-frame input flags after all Update() methods have read them
            InputManager.LateReset();
        }

        public void TransitionTo(GameState newState)
        {
            // Cleanup previous state UI
            DestroyUI();

            CurrentState = newState;
            InputManager.Clear();

            switch (newState)
            {
                case GameState.TitleScreen:
                    Time.timeScale = 1f;
                    _levelLoader.CleanupLevel();
                    AudioManager.StopAmbient();
                    AudioManager.PlayMusic(PlaceholderAudio.GetTitleMusic());
                    CreateTitleScreen();
                    break;
                case GameState.Loading:
                    StartLevel();
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    _timerRunning = true;
                    AudioManager.PlayGameplayMusicWithVariant(CurrentEpoch);
                    CreateHUD();
                    SaveSession(); // Persist so player can continue later
                    ScreenFlash.Flash(Color.white, 0.4f); // Level start flash
                    StartCoroutine(DelayedCameraIntro());
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    CreatePauseMenu();
                    break;
                case GameState.LevelComplete:
                    Time.timeScale = 1f;
                    _timerRunning = false;
                    _levelCompleteTime = Time.unscaledTime;

                    // Compute 5-component score
                    TimeScore = CalculateScore(LevelElapsedTime);
                    CombatMasteryScore = CalculateCombatMastery();
                    ExplorationScore = CalculateExploration();
                    PreservationScore = CalculatePreservation();
                    Score = TimeScore + ItemBonusScore + EnemyBonusScore
                          + CombatMasteryScore + ExplorationScore + PreservationScore;
                    TotalScore += Score;
                    SaveLevelToHistory();

                    // Record daily challenge score if applicable
                    var todayID = DailyChallengeManager.GetTodayLevelID();
                    if (CurrentLevelID.Seed == todayID.Seed && CurrentLevelID.Epoch == todayID.Epoch)
                    {
                        DailyChallengeManager.RecordScore(Score, GetStarRating(Score), LevelElapsedTime);
                    }

                    // Record weekly challenge score if applicable
                    var weeklyID = DailyChallengeManager.GetWeeklyLevelID();
                    if (CurrentLevelID.Seed == weeklyID.Seed && CurrentLevelID.Epoch == weeklyID.Epoch)
                    {
                        DailyChallengeManager.RecordWeeklyScore(Score, GetStarRating(Score), LevelElapsedTime);
                    }

                    // Save ghost replay data
                    var ghostSystem = FindAnyObjectByType<GhostReplaySystem>();
                    if (ghostSystem != null && ghostSystem.IsRecording)
                    {
                        ghostSystem.StopRecording();
                        ghostSystem.SaveGhost(CurrentLevelID.ToCode(), Score);
                    }

                    // Clear ghost replay flag after level completion
                    IsGhostReplay = false;

                    // Record wall-jump count from player for achievements
                    var playerCtrl = FindAnyObjectByType<PlayerController>();
                    if (playerCtrl != null)
                        AchievementManager.Instance?.RecordWallJumps(playerCtrl.WallJumpsThisLevel);

                    // Record achievement progress
                    int stars = GetStarRating(Score);
                    AchievementManager.Instance?.RecordLevelComplete(
                        CurrentLevelID.Epoch, LevelElapsedTime, Score, stars);

                    AudioManager.StopMusic();
                    AudioManager.PlaySFX(PlaceholderAudio.GetVictoryJingle(stars));
                    ScreenFlash.Flash(new Color(1f, 0.95f, 0.8f), 0.6f); // Gold flash
                    CreateLevelComplete();
                    break;

                case GameState.GameOver:
                    Time.timeScale = 1f;
                    _timerRunning = false;
                    ClearSession(); // No continue after game over
                    AudioManager.StopMusic();
                    AudioManager.PlaySFX(PlaceholderAudio.GetPlayerHurtSFX());
                    CreateGameOverUI();
                    break;

                case GameState.Celebration:
                    Time.timeScale = 1f;
                    _timerRunning = false;
                    _levelLoader.CleanupLevel();
                    ClearSession(); // Campaign completed
                    AudioManager.StopMusic();
                    AudioManager.PlaySFX(PlaceholderAudio.GetLevelCompleteSFX());
                    LegendsUnlocked = true;
                    CreateCelebrationUI();
                    break;
            }

            // Run post-transition validation in development builds
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (newState == GameState.Playing)
                StartCoroutine(ValidatePlayingState());
            #endif
        }

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// Post-transition validation coroutine. Waits 2 frames (after Destroy
        /// and physics settle) then asserts critical references are intact.
        /// Logs errors for any missing singletons or broken references.
        /// Only runs in Editor and Development builds.
        /// </summary>
        private System.Collections.IEnumerator ValidatePlayingState()
        {
            // Wait 2 frames: 1 for deferred Destroy, 1 for physics sync
            yield return null;
            yield return null;

            bool valid = true;

            // Core singletons
            if (Instance == null)
            {
                Debug.LogError("[QC Validation] GameManager.Instance is null during Playing state!");
                valid = false;
            }
            if (AudioManager.Instance == null)
            {
                Debug.LogError("[QC Validation] AudioManager.Instance is null during Playing state!");
                valid = false;
            }
            if (CameraController.Instance == null)
            {
                Debug.LogError("[QC Validation] CameraController.Instance is null during Playing state! " +
                    "This is the bug from build 020 — singleton race condition.");
                valid = false;
            }
            if (CheckpointManager.Instance == null)
            {
                Debug.LogError("[QC Validation] CheckpointManager.Instance is null during Playing state!");
                valid = false;
            }

            // Level data
            if (_levelLoader == null)
            {
                Debug.LogError("[QC Validation] LevelLoader reference is null!");
                valid = false;
            }
            else
            {
                if (_levelLoader.Player == null)
                {
                    Debug.LogError("[QC Validation] LevelLoader.Player is null — player not spawned!");
                    valid = false;
                }
                if (_levelLoader.TilemapRenderer == null)
                {
                    Debug.LogError("[QC Validation] LevelLoader.TilemapRenderer is null — level not rendered!");
                    valid = false;
                }
            }

            // Camera target
            if (CameraController.Instance != null)
            {
                var cam = CameraController.Instance.GetComponent<Camera>();
                if (cam == null)
                {
                    Debug.LogError("[QC Validation] CameraController has no Camera component!");
                    valid = false;
                }
            }

            // Player tag
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[QC Validation] No GameObject with 'Player' tag found!");
                valid = false;
            }

            if (valid)
                Debug.Log($"[QC Validation] Playing state validated — all {8} checks passed.");
        }
        #endif

        public void StartGame()
        {
            ClearSession(); // Starting fresh — clear any saved session
            Score = 0;
            TotalScore = 0;
            LevelNumber = 0;
            LevelElapsedTime = 0f;
            _timerRunning = false;
            StreakCount = 0;
            DeathsThisLevel = 0;
            FriendChallengeScore = 0;
            IsGhostReplay = false;

            LevelSource = "Campaign";

            if (RandomizeEnabled)
            {
                // FreePlay: random epoch, random seed, single levels
                CurrentGameMode = GameMode.FreePlay;
                GlobalLives = DEATHS_PER_LEVEL; // Not used in FreePlay, but set for safety
            }
            else
            {
                // Campaign: 10 epochs in order, start with 2 lives
                CurrentGameMode = GameMode.Campaign;
                CampaignEpoch = 0;
                CurrentEpoch = 0;
                GlobalLives = 2;
                CurrentLevelID = LevelID.GenerateRandom(0);
            }

            TransitionTo(GameState.Loading);
        }

        public void PauseGame()
        {
            if (CurrentState != GameState.Playing) return;
            TransitionTo(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused) return;
            DestroyUI();
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;
            CreateHUD();
        }

        public float LevelCompleteRemainingDelay =>
            Mathf.Max(0f, LevelCompleteMinDelay - (Time.unscaledTime - _levelCompleteTime));

        public void CompleteLevel()
        {
            if (CurrentState != GameState.Playing) return;
            TransitionTo(GameState.LevelComplete);
        }

        public void RestartLevel()
        {
            TransitionTo(GameState.Loading);
        }

        public void ReturnToTitle()
        {
            TransitionTo(GameState.TitleScreen);
        }

        public void NextLevel()
        {
            // Handle tutorial progression
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
            {
                if (!TutorialManager.Instance.AdvanceToNextLevel())
                {
                    // Tutorial complete — continue to Campaign epoch 0
                }
                TransitionTo(GameState.Loading);
                return;
            }

            if (CurrentGameMode == GameMode.Campaign)
            {
                CampaignEpoch++;
                if (CampaignEpoch > LevelID.MAX_EPOCH)
                {
                    // Completed all 10 epochs — show celebration!
                    TransitionTo(GameState.Celebration);
                    return;
                }
                CurrentLevelID = LevelID.GenerateRandom(CampaignEpoch);
                CurrentEpoch = CampaignEpoch;
            }
            else if (CurrentGameMode == GameMode.Streak)
            {
                StreakCount++;
                CurrentLevelID = LevelID.GenerateRandom(UnityEngine.Random.Range(0, LevelID.MAX_EPOCH + 1));
                CurrentEpoch = CurrentLevelID.Epoch;
            }
            else
            {
                // FreePlay: deterministic next from current
                CurrentLevelID = CurrentLevelID.Next();
                CurrentEpoch = CurrentLevelID.Epoch;
            }

            TransitionTo(GameState.Loading);
        }

        /// <summary>
        /// DEBUG: Start a specific epoch for testing. Generates a random level code for that epoch.
        /// </summary>
        public void StartTestLevel(int epoch)
        {
            CurrentEpoch = Mathf.Clamp(epoch, 0, LevelID.MAX_EPOCH);
            CurrentLevelID = LevelID.GenerateRandom(CurrentEpoch);
            CurrentGameMode = GameMode.FreePlay;
            Score = 0;
            TotalScore = 0;
            LevelNumber = 0;
            LevelElapsedTime = 0f;
            _timerRunning = false;
            DeathsThisLevel = 0;
            FriendChallengeScore = 0;
            IsGhostReplay = false;
            GlobalLives = DEATHS_PER_LEVEL;
            TransitionTo(GameState.Loading);
        }

        /// <summary>
        /// Start a level from a specific level code (e.g., "3-K7XM2P9A").
        /// Always FreePlay mode regardless of Randomize setting.
        /// </summary>
        public void StartLevelFromCode(string code)
        {
            if (!LevelID.TryParse(code, out LevelID levelID))
            {
                Debug.LogWarning($"Invalid level code: {code}");
                return;
            }
            CurrentLevelID = levelID;
            CurrentEpoch = levelID.Epoch;
            CurrentGameMode = GameMode.FreePlay;
            Score = 0;
            TotalScore = 0;
            LevelNumber = 0;
            LevelElapsedTime = 0f;
            _timerRunning = false;
            DeathsThisLevel = 0;
            FriendChallengeScore = 0;
            IsGhostReplay = false;
            GlobalLives = DEATHS_PER_LEVEL;
            LevelSource = "Code";
            TransitionTo(GameState.Loading);
        }

        private void StartLevel()
        {
            // Clear static caches and return all pooled objects from previous level
            ObjectPool.ReturnAll();
            Projectile.ClearCachedRefs();
            EnemyBase.ClearRegistry();

            // Don't count tutorial levels toward the level counter
            if (TutorialManager.Instance == null || !TutorialManager.Instance.IsTutorialActive)
                LevelNumber++;
            DeathsThisLevel = 0;
            // Per-level lives: in FreePlay, reset each level; in Campaign/Streak, deducted from GlobalLives
            // Uses DifficultyManager.MaxDeathsPerLevel for difficulty-aware death limits (Sprint 9)
            LivesRemaining = DifficultyManager.Instance != null
                ? DifficultyManager.Instance.MaxDeathsPerLevel
                : DEATHS_PER_LEVEL;

            // Generate level ID based on game mode
            if (CurrentLevelID.Seed == 0)
            {
                if (CurrentGameMode == GameMode.Campaign)
                    CurrentLevelID = LevelID.GenerateRandom(CampaignEpoch);
                else
                    CurrentLevelID = LevelID.GenerateRandom(CurrentEpoch);
            }

            CurrentEpoch = CurrentLevelID.Epoch;

            LevelElapsedTime = 0f;
            EnemiesKilled = 0;
            RewardsCollected = 0;
            WeaponsCollected = 0;
            ShotsFired = 0;
            BlocksDestroyed = 0;
            RelicsDestroyed = 0;
            ItemBonusScore = 0;
            EnemyBonusScore = 0;
            TimeScore = 0;
            CombatMasteryScore = 0;
            ExplorationScore = 0;
            PreservationScore = 0;
            HiddenContentDiscovered = 0;
            TotalHiddenContent = 0;
            SecretAreasFound = 0;
            TotalRelics = 0;
            BossDefeated = false;
            BestNoDamageStreak = 0;
            _noDamageKillStreak = 0;
            ComboCount = 0;
            _comboTimer = 0f;
            _rewardCounts.Clear();
            _weaponCounts.Clear();

            // Load tutorial level or normal level
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
                _levelLoader.LoadTutorialLevel(TutorialManager.Instance.CurrentTutorialLevel);
            else
                _levelLoader.LoadLevel(CurrentLevelID);

            // Set level totals from metadata
            if (CurrentLevel != null)
            {
                TotalHiddenContent = CountHiddenContent(CurrentLevel);
                TotalRelics = CurrentLevel.Metadata.TotalRelics;
            }

            // Initialize achievements for this level
            if (CurrentLevel != null && AchievementManager.Instance != null)
            {
                int totalDestructibles = CurrentLevel.Metadata.TotalDestructibleTiles;
                int totalItems = CurrentLevel.Metadata.TotalRewards + CurrentLevel.Metadata.TotalWeaponDrops;
                int totalHazards = CurrentLevel.Metadata.TotalHazards;
                AchievementManager.Instance.StartLevel(totalDestructibles, totalItems, totalHazards);
            }

            // Reset context-sensitive hint timers for the new level (Sprint 9)
            if (TutorialManager.Instance != null)
                TutorialManager.Instance.ResetContextHints();

            TransitionTo(GameState.Playing);
        }

        /// <summary>
        /// Calculate speed score from elapsed time. Faster = higher score.
        /// Max 5000 (instant), minimum 500. Halved from v1 to balance with other components.
        /// </summary>
        public static int CalculateScore(float elapsed)
        {
            return Mathf.Max(500, 5000 - (int)(elapsed * 25f));
        }

        /// <summary>
        /// Get star rating based on total score.
        /// Theoretical max ~20000. 3 stars >= 68%, 2 stars >= 39%, 1 star = completed.
        /// </summary>
        public static int GetStarRating(int score)
        {
            if (score >= 13500) return 3;  // ~68% of ~20k theoretical max
            if (score >= 7800) return 2;   // ~39% of ~20k theoretical max
            return 1;
        }

        /// <summary>
        /// Calculate combat mastery score (max 5000).
        /// Components: kill efficiency, no-damage streak, boss defeat.
        /// </summary>
        private int CalculateCombatMastery()
        {
            int score = 0;

            // Kill efficiency: kills/shots ratio × 2000, capped at 2000
            if (ShotsFired > 0)
                score += Mathf.Min(2000, (int)((float)EnemiesKilled / ShotsFired * 2000f));
            else if (EnemiesKilled > 0)
                score += 2000; // Stomped all enemies with no shots = perfect efficiency

            // No-damage kill streak: best streak × 150, capped at 1500
            score += Mathf.Min(1500, BestNoDamageStreak * 150);

            // Boss defeat bonus
            if (BossDefeated) score += 1500;

            return Mathf.Min(5000, score);
        }

        /// <summary>
        /// Calculate exploration score (max 4000).
        /// Components: hidden content discovery, secret areas.
        /// </summary>
        private int CalculateExploration()
        {
            int score = 0;

            // Hidden content ratio: discovered/total × 2500
            if (TotalHiddenContent > 0)
                score += (int)((float)HiddenContentDiscovered / TotalHiddenContent * 2500f);

            // Secret areas: 300 each, capped at 1500
            score += Mathf.Min(1500, SecretAreasFound * 300);

            return Mathf.Min(4000, score);
        }

        /// <summary>
        /// Calculate archaeology score (max 2000).
        /// Based on relics recovered (preserved relic tiles).
        /// </summary>
        private int CalculatePreservation()
        {
            int score = 0;

            // Relics recovered: (preserved / total) × 2000
            if (TotalRelics > 0)
            {
                int preserved = TotalRelics - RelicsDestroyed;
                score += (int)((float)Mathf.Max(0, preserved) / TotalRelics * 2000f);
            }

            return Mathf.Min(2000, score);
        }

        /// <summary>
        /// Count tiles with hidden content in a level.
        /// </summary>
        private static int CountHiddenContent(LevelData level)
        {
            if (level.Layout.Destructibles == null) return 0;
            int count = 0;
            for (int i = 0; i < level.Layout.Destructibles.Length; i++)
            {
                if (level.Layout.Destructibles[i].HiddenContent != HiddenContentType.None)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Record a reward pickup. Returns the bonus score awarded (base * multiplier).
        /// </summary>
        public int CollectReward(RewardType type)
        {
            RewardsCollected++;
            _rewardCounts.TryGetValue(type, out int prev);
            int count = prev + 1;
            _rewardCounts[type] = count;

            int baseValue = GetRewardBaseValue(type);
            int multiplier = Mathf.Min(count, 3); // Cap at 3x multiplier
            int bonus = baseValue * multiplier;
            ItemBonusScore += bonus;

            AchievementManager.Instance?.RecordItemCollected();
            return bonus;
        }

        /// <summary>
        /// Record a weapon pickup with full type info. Returns the bonus score awarded.
        /// </summary>
        public int CollectWeapon(WeaponType type, WeaponTier tier)
        {
            WeaponsCollected++;
            _weaponCounts.TryGetValue(tier, out int prev);
            int count = prev + 1;
            _weaponCounts[tier] = count;

            int baseValue = WeaponDatabase.GetPickupScoreValue(type, tier);
            int multiplier = Mathf.Min(count, 3); // Cap at 3x multiplier
            int bonus = baseValue * multiplier;
            ItemBonusScore += bonus;

            AchievementManager.Instance?.RecordItemCollected();
            AchievementManager.Instance?.RecordWeaponAcquired(tier);
            AchievementManager.Instance?.RecordWeaponTypeAcquired(type);
            return bonus;
        }

        /// <summary>
        /// Legacy weapon collect method for backward compatibility.
        /// </summary>
        public int CollectWeapon(WeaponTier tier)
        {
            return CollectWeapon(WeaponType.Bolt, tier);
        }

        /// <summary>
        /// Record a shot fired (for combat efficiency tracking).
        /// </summary>
        public void RecordShotFired()
        {
            ShotsFired++;
        }

        /// <summary>
        /// Record an enemy kill. Returns the bonus score awarded.
        /// Flat 100 per kill (soft cap 2500) + combo bonus (+50 per level, cap +500).
        /// Also tracks no-damage kill streak for combat mastery scoring.
        /// </summary>
        public int RecordEnemyKill()
        {
            EnemiesKilled++;
            _noDamageKillStreak++;
            BestNoDamageStreak = Mathf.Max(BestNoDamageStreak, _noDamageKillStreak);

            // Combo: kills within 2s build multiplier
            if (_comboTimer > 0f)
                ComboCount++;
            else
                ComboCount = 1;
            _comboTimer = COMBO_WINDOW;

            // Flat 100 per kill, soft cap at 2500
            int bonus = 100;
            if (EnemyBonusScore + bonus > 2500)
                bonus = Mathf.Max(0, 2500 - EnemyBonusScore);

            // Combo bonus: +50 per combo level, capped at +500
            int comboBonus = Mathf.Min(ComboCount * 50, 500);
            bonus += comboBonus;

            EnemyBonusScore += bonus;

            AchievementManager.Instance?.RecordEnemyKilled();
            return bonus;
        }

        // Destruction tracking
        public int BlocksDestroyed { get; private set; }
        public int RelicsDestroyed { get; private set; }

        // Sprint 4: Extended scoring — 5-component system
        public int CombatMasteryScore { get; private set; }
        public int ExplorationScore { get; private set; }
        public int PreservationScore { get; private set; }
        public int HiddenContentDiscovered { get; private set; }
        public int TotalHiddenContent { get; private set; }
        public int SecretAreasFound { get; private set; }
        public int TotalRelics { get; private set; }
        public bool BossDefeated { get; private set; }
        public bool IsFirstCompletion { get; private set; }
        public int BestNoDamageStreak { get; private set; }
        public int CurrentNoDamageStreak => _noDamageKillStreak;
        private int _noDamageKillStreak;

        /// <summary>
        /// Record a destructible block being destroyed (for achievements).
        /// </summary>
        public void RecordBlockDestroyed()
        {
            BlocksDestroyed++;
            AchievementManager.Instance?.RecordBlockDestroyed();
        }

        /// <summary>
        /// Record a relic tile being destroyed (lost preservation score).
        /// </summary>
        public void RecordRelicDestroyed()
        {
            RelicsDestroyed++;
        }

        /// <summary>
        /// Record that the player took damage (for achievements and scoring).
        /// Resets no-damage kill streak for combat mastery.
        /// </summary>
        public void RecordPlayerDamage()
        {
            _noDamageKillStreak = 0;
            ComboCount = 0;
            _comboTimer = 0f;
            AchievementManager.Instance?.RecordDamageTaken();
        }

        /// <summary>
        /// Record discovering hidden content in a destructible tile.
        /// </summary>
        public void RecordHiddenContentFound(HiddenContentType type)
        {
            HiddenContentDiscovered++;
            if (type == HiddenContentType.Secret)
                SecretAreasFound++;
        }

        /// <summary>
        /// Record boss defeated (1500 combat mastery bonus).
        /// </summary>
        public void RecordBossDefeated()
        {
            BossDefeated = true;
            AchievementManager.Instance?.RecordBossDefeated();
        }

        /// <summary>
        /// Get the highest multiplier achieved for any single item type this level.
        /// </summary>
        public int GetBestMultiplier()
        {
            int best = 0;
            foreach (var kv in _rewardCounts)
                if (kv.Value > best) best = kv.Value;
            foreach (var kv in _weaponCounts)
                if (kv.Value > best) best = kv.Value;
            if (EnemiesKilled > best) best = EnemiesKilled;
            return best;
        }

        private static int GetRewardBaseValue(RewardType type)
        {
            switch (type)
            {
                case RewardType.HealthSmall: return 50;
                case RewardType.HealthLarge: return 100;
                case RewardType.AttackBoost: return 150;
                case RewardType.SpeedBoost: return 150;
                case RewardType.Shield: return 200;
                case RewardType.Coin: return 25;
                default: return 50;
            }
        }

        /// <summary>
        /// Award boss kill score directly without inflating kill streak.
        /// </summary>
        public void RecordBossKillScore(int bonus)
        {
            EnemyBonusScore += bonus;
        }

        // =====================================================================
        // Level History
        // =====================================================================

        private const string HISTORY_PREFS_KEY = "EpochBreaker_LevelHistory";
        private const int MAX_HISTORY_ENTRIES = 50;

        /// <summary>
        /// Save the current level to history when completed.
        /// </summary>
        private void SaveLevelToHistory()
        {
            var history = LoadLevelHistory();

            // Check if this code already exists (update it if so)
            string code = CurrentLevelID.ToCode();
            int existingIdx = history.Entries.FindIndex(e => e.Code == code);
            IsFirstCompletion = existingIdx < 0;

            var entry = new LevelHistoryEntry
            {
                Code = code,
                Epoch = CurrentLevelID.Epoch,
                EpochName = CurrentLevelID.EpochName,
                Score = Score,
                Stars = GetStarRating(Score),
                PlayedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Source = LevelSource
            };

            if (existingIdx >= 0)
            {
                // Update existing entry if new score is higher
                if (Score > history.Entries[existingIdx].Score)
                {
                    history.Entries[existingIdx] = entry;
                }
                else
                {
                    // Just update timestamp
                    history.Entries[existingIdx].PlayedTimestamp = entry.PlayedTimestamp;
                }
                // Move to front (most recent)
                var existing = history.Entries[existingIdx];
                history.Entries.RemoveAt(existingIdx);
                history.Entries.Insert(0, existing);
            }
            else
            {
                // Add new entry at the front
                history.Entries.Insert(0, entry);

                // Trim to max size
                while (history.Entries.Count > MAX_HISTORY_ENTRIES)
                {
                    history.Entries.RemoveAt(history.Entries.Count - 1);
                }
            }

            SaveLevelHistory(history);
        }

        /// <summary>
        /// Load level history from PlayerPrefs.
        /// </summary>
        public static LevelHistoryData LoadLevelHistory()
        {
            string json = PlayerPrefs.GetString(HISTORY_PREFS_KEY, "");
            if (string.IsNullOrEmpty(json))
            {
                return new LevelHistoryData();
            }

            try
            {
                return JsonUtility.FromJson<LevelHistoryData>(json) ?? new LevelHistoryData();
            }
            catch
            {
                return new LevelHistoryData();
            }
        }

        /// <summary>
        /// Save level history to PlayerPrefs.
        /// </summary>
        private static void SaveLevelHistory(LevelHistoryData history)
        {
            string json = JsonUtility.ToJson(history);
            PlayerPrefs.SetString(HISTORY_PREFS_KEY, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Clear all level history.
        /// </summary>
        public static void ClearLevelHistory()
        {
            PlayerPrefs.DeleteKey(HISTORY_PREFS_KEY);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Copy text to system clipboard.
        /// </summary>
        public static void CopyToClipboard(string text)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLCopyToClipboard(text);
#else
            GUIUtility.systemCopyBuffer = text;
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void WebGLCopyToClipboard(string text);
#endif

        // =====================================================================
        // Edge Cases (Sprint 9): Auto-pause, crash recovery
        // =====================================================================

        private const string CRASH_FLAG_KEY = "EpochBreaker_CrashFlag";

        /// <summary>
        /// Called when the app is paused/resumed (e.g., backgrounding on mobile).
        /// Auto-pauses the game when losing focus mid-level.
        /// </summary>
        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                // App going to background — auto-pause if playing
                if (CurrentState == GameState.Playing)
                {
                    PauseGame();
                    SaveSession(); // Save in case we get killed
                }
                // Set crash flag so we can detect unexpected termination
                PlayerPrefs.SetInt(CRASH_FLAG_KEY, 1);
                PlayerPrefs.Save();
            }
            else
            {
                // App returning to foreground — clear crash flag
                PlayerPrefs.SetInt(CRASH_FLAG_KEY, 0);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Called when the app gains or loses focus (e.g., switching tabs in browser).
        /// Auto-pauses the game when losing focus.
        /// </summary>
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && CurrentState == GameState.Playing)
            {
                PauseGame();
            }
        }

        /// <summary>
        /// Called when Unity receives a low memory warning.
        /// Cleans up cached assets to reduce memory pressure.
        /// </summary>
        private void OnApplicationQuit()
        {
            // Clear crash flag on clean exit
            PlayerPrefs.SetInt(CRASH_FLAG_KEY, 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Check if the previous session ended in a crash (crash flag still set).
        /// If so, and a saved session exists, the player can recover.
        /// </summary>
        public static bool DidCrashLastSession()
        {
            return PlayerPrefs.GetInt(CRASH_FLAG_KEY, 0) == 1 && HasSavedSession();
        }

        /// <summary>
        /// Handle low memory warnings by clearing cached assets.
        /// </summary>
        private void HandleMemoryWarning()
        {
            // Clear any non-essential caches
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        private void OnEnable()
        {
            Application.lowMemory += HandleMemoryWarning;
        }

        private void OnDisable()
        {
            Application.lowMemory -= HandleMemoryWarning;
        }

        private void DestroyUI()
        {
            if (_titleScreenObj) Destroy(_titleScreenObj);
            if (_hudObj) Destroy(_hudObj);
            if (_pauseMenuObj) Destroy(_pauseMenuObj);
            if (_levelCompleteObj) Destroy(_levelCompleteObj);
            if (_touchControlsObj) Destroy(_touchControlsObj);
            if (_gameOverObj) Destroy(_gameOverObj);
            if (_celebrationObj) Destroy(_celebrationObj);
        }

        private void CreateTitleScreen()
        {
            _titleScreenObj = new GameObject("TitleScreenUI");
            AddUI(_titleScreenObj, "EpochBreaker.UI.TitleScreenUI");
        }

        private void CreateHUD()
        {
            _hudObj = new GameObject("GameplayHUD");
            AddUI(_hudObj, "EpochBreaker.UI.GameplayHUD");

            // Tutorial hint overlay (only visible during active tutorial)
            var tutorialHintObj = new GameObject("TutorialHintUI");
            tutorialHintObj.transform.SetParent(_hudObj.transform);
            AddUI(tutorialHintObj, "EpochBreaker.UI.TutorialHintUI");

            _touchControlsObj = new GameObject("TouchControls");
            AddUI(_touchControlsObj, "EpochBreaker.UI.TouchControlsUI");
        }

        private void CreatePauseMenu()
        {
            _pauseMenuObj = new GameObject("PauseMenuUI");
            AddUI(_pauseMenuObj, "EpochBreaker.UI.PauseMenuUI");
        }

        private void CreateLevelComplete()
        {
            _levelCompleteObj = new GameObject("LevelCompleteUI");
            AddUI(_levelCompleteObj, "EpochBreaker.UI.LevelCompleteUI");
        }

        private void CreateGameOverUI()
        {
            _gameOverObj = new GameObject("GameOverUI");
            AddUI(_gameOverObj, "EpochBreaker.UI.GameOverUI");
        }

        private void CreateCelebrationUI()
        {
            _celebrationObj = new GameObject("CelebrationUI");
            AddUI(_celebrationObj, "EpochBreaker.UI.CelebrationUI");
        }

        // =====================================================================
        // Legends Persistence
        // =====================================================================

        /// <summary>
        /// Save the current streak run to the Legends leaderboard.
        /// </summary>
        public void SaveStreakToLegends()
        {
            if (StreakCount <= 0) return;

            var legends = LoadLegendsData();
            legends.Entries.Add(new LegendsEntry
            {
                StreakCount = StreakCount,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            // Sort descending by streak count
            legends.Entries.Sort((a, b) => b.StreakCount.CompareTo(a.StreakCount));

            // Trim to max
            while (legends.Entries.Count > MAX_LEGENDS_ENTRIES)
                legends.Entries.RemoveAt(legends.Entries.Count - 1);

            string json = JsonUtility.ToJson(legends);
            PlayerPrefs.SetString(LEGENDS_PREFS_KEY, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load Legends leaderboard data from PlayerPrefs.
        /// </summary>
        public static LegendsData LoadLegendsData()
        {
            string json = PlayerPrefs.GetString(LEGENDS_PREFS_KEY, "");
            if (string.IsNullOrEmpty(json))
                return new LegendsData();
            try
            {
                return JsonUtility.FromJson<LegendsData>(json) ?? new LegendsData();
            }
            catch
            {
                return new LegendsData();
            }
        }

        /// <summary>
        /// Called when the player dies. Returns the result determining what happens next.
        /// FreePlay: MaxDeathsPerLevel deaths per level, then game over.
        /// Campaign/Streak: each death costs 1 global life. MaxDeathsPerLevel deaths = level failed (advance).
        ///                   0 global lives = game over.
        /// Deaths per level is configurable via DifficultyManager (Sprint 9).
        /// </summary>
        public DeathResult LoseLife()
        {
            DeathsThisLevel++;
            int maxDeaths = DifficultyManager.Instance != null
                ? DifficultyManager.Instance.MaxDeathsPerLevel
                : DEATHS_PER_LEVEL;

            if (CurrentGameMode == GameMode.FreePlay)
            {
                LivesRemaining--;
                if (LivesRemaining <= 0)
                {
                    TransitionTo(GameState.GameOver);
                    return DeathResult.GameOver;
                }
                return DeathResult.Respawn;
            }

            // Campaign or Streak: deduct a global life
            GlobalLives--;

            if (GlobalLives <= 0)
            {
                // No lives left at all — game over
                if (CurrentGameMode == GameMode.Streak)
                    SaveStreakToLegends();
                TransitionTo(GameState.GameOver);
                return DeathResult.GameOver;
            }

            if (DeathsThisLevel >= maxDeaths)
            {
                // Used up deaths for this level — level failed, advance
                return DeathResult.LevelFailed;
            }

            return DeathResult.Respawn;
        }

        /// <summary>
        /// Advance past a failed level (player died DEATHS_PER_LEVEL times but still has global lives).
        /// </summary>
        public void AdvanceAfterFail()
        {
            if (CurrentGameMode == GameMode.Campaign)
            {
                CampaignEpoch++;
                if (CampaignEpoch > LevelID.MAX_EPOCH)
                {
                    // Failed the last epoch — campaign over (only NextLevel triggers Celebration)
                    TransitionTo(GameState.GameOver);
                    return;
                }
                CurrentLevelID = LevelID.GenerateRandom(CampaignEpoch);
                CurrentEpoch = CampaignEpoch;
            }
            else if (CurrentGameMode == GameMode.Streak)
            {
                // Failed level doesn't count toward streak, just move on
                CurrentLevelID = LevelID.GenerateRandom(UnityEngine.Random.Range(0, LevelID.MAX_EPOCH + 1));
                CurrentEpoch = CurrentLevelID.Epoch;
            }

            TransitionTo(GameState.Loading);
        }

        /// <summary>
        /// Set unlimited lives for God Mode (dev menu).
        /// </summary>
        public void SetUnlimitedLives()
        {
            GlobalLives = 999;
            LivesRemaining = 999;
        }

        /// <summary>
        /// Preview the Level Complete screen with dummy data (dev menu).
        /// </summary>
        public void PreviewLevelComplete()
        {
            Score = 12345;
            TotalScore = 50000;
            TimeScore = 3000;
            ItemBonusScore = 2000;
            EnemyBonusScore = 1500;
            CombatMasteryScore = 2000;
            ExplorationScore = 1845;
            PreservationScore = 2000;
            EnemiesKilled = 8;
            CurrentEpoch = 3;
            _levelCompleteTime = Time.unscaledTime;
            TransitionTo(GameState.LevelComplete);
        }

        /// <summary>
        /// Preview the Celebration screen (dev menu).
        /// </summary>
        public void PreviewCelebration()
        {
            TransitionTo(GameState.Celebration);
        }

        /// <summary>
        /// Transition from celebration to streak mode.
        /// </summary>
        public void StartStreakMode()
        {
            CurrentGameMode = GameMode.Streak;
            GlobalLives = 10;
            StreakCount = 0;
            Score = 0;
            TotalScore = 0;
            LevelNumber = 0;
            DeathsThisLevel = 0;
            FriendChallengeScore = 0;
            IsGhostReplay = false;
            LegendsUnlocked = true;
            CurrentLevelID = LevelID.GenerateRandom(UnityEngine.Random.Range(0, LevelID.MAX_EPOCH + 1));
            CurrentEpoch = CurrentLevelID.Epoch;
            TransitionTo(GameState.Loading);
        }

        /// <summary>
        /// Start today's daily challenge.
        /// </summary>
        public void StartDailyChallenge()
        {
            ClearSession();
            CurrentGameMode = GameMode.FreePlay;
            CurrentLevelID = DailyChallengeManager.GetTodayLevelID();
            CurrentEpoch = CurrentLevelID.Epoch;
            Score = 0;
            TotalScore = 0;
            LevelNumber = 0;
            LevelElapsedTime = 0f;
            _timerRunning = false;
            DeathsThisLevel = 0;
            FriendChallengeScore = 0;
            IsGhostReplay = false;
            GlobalLives = DEATHS_PER_LEVEL;
            LevelSource = "Daily";
            TransitionTo(GameState.Loading);
        }

        /// <summary>
        /// Start this week's weekly challenge. Resets every Monday.
        /// More time to optimize than daily challenges.
        /// </summary>
        public void StartWeeklyChallenge()
        {
            ClearSession();
            CurrentGameMode = GameMode.FreePlay;
            CurrentLevelID = DailyChallengeManager.GetWeeklyLevelID();
            CurrentEpoch = CurrentLevelID.Epoch;
            Score = 0;
            TotalScore = 0;
            LevelNumber = 0;
            LevelElapsedTime = 0f;
            _timerRunning = false;
            DeathsThisLevel = 0;
            FriendChallengeScore = 0;
            IsGhostReplay = false;
            GlobalLives = DEATHS_PER_LEVEL;
            LevelSource = "Weekly";
            TransitionTo(GameState.Loading);
        }

        /// <summary>
        /// Start a friend challenge level. Shows the friend's score as a target during gameplay.
        /// </summary>
        public void StartFriendChallenge(string levelCode, int friendScore)
        {
            if (!LevelID.TryParse(levelCode, out LevelID levelID))
            {
                Debug.LogWarning($"Invalid challenge level code: {levelCode}");
                return;
            }
            ClearSession();
            CurrentGameMode = GameMode.FreePlay;
            CurrentLevelID = levelID;
            CurrentEpoch = levelID.Epoch;
            Score = 0;
            TotalScore = 0;
            LevelNumber = 0;
            LevelElapsedTime = 0f;
            _timerRunning = false;
            DeathsThisLevel = 0;
            FriendChallengeScore = friendScore;
            IsGhostReplay = false;
            GlobalLives = DEATHS_PER_LEVEL;
            LevelSource = "Challenge";
            TransitionTo(GameState.Loading);
        }

        /// <summary>
        /// Replay a level with ghost overlay showing the previous best run.
        /// </summary>
        public void StartGhostReplay(string levelCode)
        {
            if (!LevelID.TryParse(levelCode, out LevelID levelID))
            {
                Debug.LogWarning($"Invalid ghost replay level code: {levelCode}");
                return;
            }
            ClearSession();
            CurrentGameMode = GameMode.FreePlay;
            CurrentLevelID = levelID;
            CurrentEpoch = levelID.Epoch;
            Score = 0;
            TotalScore = 0;
            LevelNumber = 0;
            LevelElapsedTime = 0f;
            _timerRunning = false;
            DeathsThisLevel = 0;
            FriendChallengeScore = 0;
            IsGhostReplay = true;
            GlobalLives = DEATHS_PER_LEVEL;
            TransitionTo(GameState.Loading);
        }

        /// <summary>
        /// Collect an extra life pickup (campaign mode only).
        /// </summary>
        public void CollectExtraLife()
        {
            if (CurrentGameMode != GameMode.Campaign) return;
            GlobalLives++;
            AudioManager.PlaySFX(PlaceholderAudio.GetRewardPickupSFX());
        }

        /// <summary>
        /// Add a UI component by type name via reflection.
        /// Avoids compile-time dependency on UI assembly from gameplay assembly.
        /// </summary>
        private static readonly Dictionary<string, System.Type> _uiTypeCache = new Dictionary<string, System.Type>();

        private static void AddUI(GameObject go, string typeName)
        {
            if (!_uiTypeCache.TryGetValue(typeName, out var cachedType))
            {
                foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    cachedType = asm.GetType(typeName);
                    if (cachedType != null) break;
                }
                _uiTypeCache[typeName] = cachedType;
            }
            if (cachedType != null)
                go.AddComponent(cachedType);
            else
                Debug.LogWarning($"UI type not found: {typeName}");
        }
    }
}
