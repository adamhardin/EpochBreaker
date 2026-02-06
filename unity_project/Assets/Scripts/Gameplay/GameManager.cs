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
        GameOver
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
    /// Singleton game manager. Controls game state, level progression, and
    /// coordinates all major systems. Auto-creates itself via RuntimeInitializeOnLoadMethod.
    /// Runs before other scripts to ensure input flags are set before consumption.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.TitleScreen;
        public int CurrentEpoch { get; set; } = 0;
        public const int MAX_LIVES_PER_LEVEL = 2;
        public int LivesRemaining { get; private set; } = MAX_LIVES_PER_LEVEL;
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

        // Per-type collection counts for multiplier bonuses (exposed for UI display)
        public Dictionary<RewardType, int> RewardCounts => _rewardCounts;
        public Dictionary<WeaponTier, int> WeaponCounts => _weaponCounts;
        private Dictionary<RewardType, int> _rewardCounts = new Dictionary<RewardType, int>();
        private Dictionary<WeaponTier, int> _weaponCounts = new Dictionary<WeaponTier, int>();

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
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _levelLoader = gameObject.AddComponent<LevelLoader>();
            gameObject.AddComponent<AudioManager>();
            gameObject.AddComponent<AchievementManager>();

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
                        StartGame();
                    break;
                case GameState.Playing:
                    if (_timerRunning)
                        LevelElapsedTime += Time.deltaTime;
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
                        else if (Keyboard.current.escapeKey.wasPressedThisFrame)
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
                    AudioManager.PlayMusic(PlaceholderAudio.GetTitleMusic());
                    CreateTitleScreen();
                    break;
                case GameState.Loading:
                    StartLevel();
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    _timerRunning = true;
                    AudioManager.PlayMusic(PlaceholderAudio.GetGameplayMusic());
                    CreateHUD();
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    CreatePauseMenu();
                    break;
                case GameState.LevelComplete:
                    Time.timeScale = 1f;
                    _timerRunning = false;
                    _levelCompleteTime = Time.unscaledTime;
                    TimeScore = CalculateScore(LevelElapsedTime);
                    Score = TimeScore + ItemBonusScore + EnemyBonusScore;
                    TotalScore += Score;
                    SaveLevelToHistory();

                    // Record achievement progress
                    int stars = GetStarRating(LevelElapsedTime);
                    AchievementManager.Instance?.RecordLevelComplete(
                        CurrentLevelID.Epoch, LevelElapsedTime, Score, stars);

                    AudioManager.StopMusic();
                    AudioManager.PlaySFX(PlaceholderAudio.GetLevelCompleteSFX());
                    CreateLevelComplete();
                    break;

                case GameState.GameOver:
                    Time.timeScale = 1f;
                    _timerRunning = false;
                    AudioManager.StopMusic();
                    AudioManager.PlaySFX(PlaceholderAudio.GetPlayerHurtSFX());
                    CreateGameOverUI();
                    break;
            }
        }

        public void StartGame()
        {
            Score = 0;
            TotalScore = 0;
            LevelNumber = 0;
            LevelElapsedTime = 0f;
            _timerRunning = false;
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
            // Derive next level from current (deterministic sequence, advances epoch)
            CurrentLevelID = CurrentLevelID.Next();
            CurrentEpoch = CurrentLevelID.Epoch;
            TransitionTo(GameState.Loading);
        }

        /// <summary>
        /// DEBUG: Start a specific epoch for testing. Generates a random level code for that epoch.
        /// </summary>
        public void StartTestLevel(int epoch)
        {
            CurrentEpoch = Mathf.Clamp(epoch, 0, LevelID.MAX_EPOCH);
            CurrentLevelID = LevelID.GenerateRandom(CurrentEpoch);
            Score = 0;
            TotalScore = 0;
            LevelNumber = 0;
            LevelElapsedTime = 0f;
            _timerRunning = false;
            TransitionTo(GameState.Loading);
        }

        /// <summary>
        /// Start a level from a specific level code (e.g., "3-K7XM2P9A").
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
            Score = 0;
            TotalScore = 0;
            LevelNumber = 0;
            LevelElapsedTime = 0f;
            _timerRunning = false;
            TransitionTo(GameState.Loading);
        }

        private void StartLevel()
        {
            LevelNumber++;
            // Reset lives at the start of each level
            LivesRemaining = MAX_LIVES_PER_LEVEL;
            // If no level ID set yet (first level), generate a random one for current epoch
            if (CurrentLevelID.Seed == 0)
            {
                CurrentLevelID = LevelID.GenerateRandom(CurrentEpoch);
            }
            LevelElapsedTime = 0f;
            EnemiesKilled = 0;
            RewardsCollected = 0;
            WeaponsCollected = 0;
            ItemBonusScore = 0;
            EnemyBonusScore = 0;
            TimeScore = 0;
            _rewardCounts.Clear();
            _weaponCounts.Clear();
            _levelLoader.LoadLevel(CurrentLevelID);

            // Initialize achievements for this level
            if (CurrentLevel != null && AchievementManager.Instance != null)
            {
                int totalDestructibles = CurrentLevel.Metadata.TotalDestructibleTiles;
                int totalItems = CurrentLevel.Metadata.TotalRewards + CurrentLevel.Metadata.TotalWeaponDrops;
                AchievementManager.Instance.StartLevel(totalDestructibles, totalItems);
            }

            TransitionTo(GameState.Playing);
        }

        /// <summary>
        /// Calculate score from elapsed time. Faster = higher score.
        /// 0s = 10000, ~200s = 100 (minimum).
        /// </summary>
        public static int CalculateScore(float elapsed)
        {
            return Mathf.Max(100, 10000 - (int)(elapsed * 50f));
        }

        /// <summary>
        /// Get star rating from elapsed time.
        /// 3 stars: under 60s, 2 stars: under 120s, 1 star: completed.
        /// </summary>
        public static int GetStarRating(float elapsed)
        {
            if (elapsed < 60f) return 3;
            if (elapsed < 120f) return 2;
            return 1;
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
            int bonus = baseValue * count;
            ItemBonusScore += bonus;

            AchievementManager.Instance?.RecordItemCollected();
            return bonus;
        }

        /// <summary>
        /// Record a weapon pickup. Returns the bonus score awarded (base * multiplier).
        /// </summary>
        public int CollectWeapon(WeaponTier tier)
        {
            WeaponsCollected++;
            _weaponCounts.TryGetValue(tier, out int prev);
            int count = prev + 1;
            _weaponCounts[tier] = count;

            int baseValue = GetWeaponBaseValue(tier);
            int bonus = baseValue * count;
            ItemBonusScore += bonus;

            AchievementManager.Instance?.RecordItemCollected();
            AchievementManager.Instance?.RecordWeaponAcquired(tier);
            return bonus;
        }

        /// <summary>
        /// Record an enemy kill. Returns the bonus score awarded (100 * kill count).
        /// </summary>
        public int RecordEnemyKill()
        {
            EnemiesKilled++;
            int bonus = 100 * EnemiesKilled;
            EnemyBonusScore += bonus;

            AchievementManager.Instance?.RecordEnemyKilled();
            return bonus;
        }

        /// <summary>
        /// Record a destructible block being destroyed (for achievements).
        /// </summary>
        public void RecordBlockDestroyed()
        {
            AchievementManager.Instance?.RecordBlockDestroyed();
        }

        /// <summary>
        /// Record that the player took damage (for achievements).
        /// </summary>
        public void RecordPlayerDamage()
        {
            AchievementManager.Instance?.RecordDamageTaken();
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

        private static int GetWeaponBaseValue(WeaponTier tier)
        {
            switch (tier)
            {
                case WeaponTier.Starting: return 100;
                case WeaponTier.Medium: return 200;
                case WeaponTier.Heavy: return 300;
                default: return 100;
            }
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

            var entry = new LevelHistoryEntry
            {
                Code = code,
                Epoch = CurrentLevelID.Epoch,
                EpochName = CurrentLevelID.EpochName,
                Score = Score,
                Stars = GetStarRating(LevelElapsedTime),
                PlayedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
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

        private void DestroyUI()
        {
            if (_titleScreenObj) Destroy(_titleScreenObj);
            if (_hudObj) Destroy(_hudObj);
            if (_pauseMenuObj) Destroy(_pauseMenuObj);
            if (_levelCompleteObj) Destroy(_levelCompleteObj);
            if (_touchControlsObj) Destroy(_touchControlsObj);
            if (_gameOverObj) Destroy(_gameOverObj);
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

        /// <summary>
        /// Called when the player dies. Returns true if respawn allowed, false if game over.
        /// </summary>
        public bool LoseLife()
        {
            LivesRemaining--;
            if (LivesRemaining <= 0)
            {
                TransitionTo(GameState.GameOver);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Add a UI component by type name via reflection.
        /// Avoids compile-time dependency on UI assembly from gameplay assembly.
        /// </summary>
        private static void AddUI(GameObject go, string typeName)
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(typeName);
                if (t != null) { go.AddComponent(t); return; }
            }
            Debug.LogWarning($"UI type not found: {typeName}");
        }
    }
}
