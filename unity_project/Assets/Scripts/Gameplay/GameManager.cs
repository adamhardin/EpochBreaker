using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SixteenBit.Generative;

namespace SixteenBit.Gameplay
{
    public enum GameState
    {
        TitleScreen,
        Loading,
        Playing,
        Paused,
        LevelComplete
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
        public int CurrentDifficulty { get; set; } = 0;
        public int CurrentEra { get; set; } = 0;
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

            // Ensure EventSystem exists for UI interaction
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<StandaloneInputModule>();
                DontDestroyOnLoad(esGO);
            }
        }

        private void Start()
        {
            TransitionTo(GameState.TitleScreen);
        }

        private void Update()
        {
            // Read keyboard input every frame (runs first due to DefaultExecutionOrder)
            InputManager.UpdateKeyboard();

            switch (CurrentState)
            {
                case GameState.TitleScreen:
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)
                        || Input.GetKeyDown(KeyCode.Space))
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
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)
                        || Input.GetKeyDown(KeyCode.Space))
                        NextLevel();
                    else if (Input.GetKeyDown(KeyCode.R))
                        RestartLevel();
                    else if (Input.GetKeyDown(KeyCode.Escape))
                        ReturnToTitle();
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
                    TimeScore = CalculateScore(LevelElapsedTime);
                    Score = TimeScore + ItemBonusScore + EnemyBonusScore;
                    TotalScore += Score;
                    AudioManager.StopMusic();
                    AudioManager.PlaySFX(PlaceholderAudio.GetLevelCompleteSFX());
                    CreateLevelComplete();
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
            // Advance era every 4 levels, cycle difficulty
            CurrentDifficulty = (CurrentDifficulty + 1) % 4;
            if (CurrentDifficulty == 0)
                CurrentEra = Mathf.Min(CurrentEra + 1, 9);
            TransitionTo(GameState.Loading);
        }

        /// <summary>
        /// DEBUG: Start a specific era/difficulty for testing.
        /// </summary>
        public void StartTestLevel(int era, int difficulty)
        {
            CurrentEra = Mathf.Clamp(era, 0, 9);
            CurrentDifficulty = Mathf.Clamp(difficulty, 0, 3);
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
            CurrentLevelID = LevelID.GenerateNew(CurrentDifficulty, CurrentEra);
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
            return bonus;
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

        private void DestroyUI()
        {
            if (_titleScreenObj) Destroy(_titleScreenObj);
            if (_hudObj) Destroy(_hudObj);
            if (_pauseMenuObj) Destroy(_pauseMenuObj);
            if (_levelCompleteObj) Destroy(_levelCompleteObj);
            if (_touchControlsObj) Destroy(_touchControlsObj);
        }

        private void CreateTitleScreen()
        {
            _titleScreenObj = new GameObject("TitleScreenUI");
            AddUI(_titleScreenObj, "SixteenBit.UI.TitleScreenUI");
        }

        private void CreateHUD()
        {
            _hudObj = new GameObject("GameplayHUD");
            AddUI(_hudObj, "SixteenBit.UI.GameplayHUD");

            _touchControlsObj = new GameObject("TouchControls");
            AddUI(_touchControlsObj, "SixteenBit.UI.TouchControlsUI");
        }

        private void CreatePauseMenu()
        {
            _pauseMenuObj = new GameObject("PauseMenuUI");
            AddUI(_pauseMenuObj, "SixteenBit.UI.PauseMenuUI");
        }

        private void CreateLevelComplete()
        {
            _levelCompleteObj = new GameObject("LevelCompleteUI");
            AddUI(_levelCompleteObj, "SixteenBit.UI.LevelCompleteUI");
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
