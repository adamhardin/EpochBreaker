using UnityEngine;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Difficulty level options (Sprint 9).
    /// </summary>
    public enum DifficultyLevel
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }

    /// <summary>
    /// Singleton manager for difficulty settings.
    /// Configures deaths per level, enemy count, and health pickup multipliers.
    /// Persisted to PlayerPrefs. Separate leaderboard keys per difficulty.
    /// </summary>
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }

        private const string PREF_DIFFICULTY = "EpochBreaker_Difficulty";

        private DifficultyLevel _currentDifficulty = DifficultyLevel.Normal;

        public DifficultyLevel CurrentDifficulty
        {
            get => _currentDifficulty;
            set
            {
                _currentDifficulty = value;
                PlayerPrefs.SetInt(PREF_DIFFICULTY, (int)value);
                SafePrefs.Save();
            }
        }

        /// <summary>
        /// Maximum deaths allowed per level before level failure.
        /// Easy: 3, Normal: 2 (game default DEATHS_PER_LEVEL), Hard: 1.
        /// </summary>
        public int MaxDeathsPerLevel
        {
            get
            {
                switch (_currentDifficulty)
                {
                    case DifficultyLevel.Easy: return 3;
                    case DifficultyLevel.Hard: return 1;
                    default: return GameManager.DEATHS_PER_LEVEL;
                }
            }
        }

        /// <summary>
        /// Multiplier for enemy count during level spawning.
        /// Easy: 0.5 (half enemies), Normal: 1.0, Hard: 1.5.
        /// </summary>
        public float EnemyCountMultiplier
        {
            get
            {
                switch (_currentDifficulty)
                {
                    case DifficultyLevel.Easy: return 0.5f;
                    case DifficultyLevel.Hard: return 1.5f;
                    default: return 1.0f;
                }
            }
        }

        /// <summary>
        /// Multiplier for health pickup spawning.
        /// Easy: 1.5 (50% more), Normal: 1.0, Hard: 0.0 (no health pickups).
        /// </summary>
        public float HealthPickupMultiplier
        {
            get
            {
                switch (_currentDifficulty)
                {
                    case DifficultyLevel.Easy: return 1.5f;
                    case DifficultyLevel.Hard: return 0.0f;
                    default: return 1.0f;
                }
            }
        }

        /// <summary>
        /// Get a difficulty-specific suffix for leaderboard/history keys.
        /// This allows separate leaderboards per difficulty level.
        /// </summary>
        public string LeaderboardSuffix
        {
            get
            {
                switch (_currentDifficulty)
                {
                    case DifficultyLevel.Easy: return "_easy";
                    case DifficultyLevel.Hard: return "_hard";
                    default: return "";
                }
            }
        }

        /// <summary>
        /// Score multiplier applied to final level score.
        /// Easy: 0.5x (halved), Normal: 1.0x, Hard: 2.0x (doubled).
        /// </summary>
        public float ScoreMultiplier
        {
            get
            {
                switch (_currentDifficulty)
                {
                    case DifficultyLevel.Easy: return 0.5f;
                    case DifficultyLevel.Hard: return 2.0f;
                    default: return 1.0f;
                }
            }
        }

        /// <summary>
        /// Display name for the current difficulty.
        /// </summary>
        public string DifficultyName
        {
            get
            {
                switch (_currentDifficulty)
                {
                    case DifficultyLevel.Easy: return "Easy";
                    case DifficultyLevel.Hard: return "Hard";
                    default: return "Normal";
                }
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[Singleton] DifficultyManager duplicate detected — destroying new instance.");
                Destroy(this);
                return;
            }
            Instance = this;

            // Load saved difficulty
            int saved = PlayerPrefs.GetInt(PREF_DIFFICULTY, (int)DifficultyLevel.Normal);
            _currentDifficulty = (DifficultyLevel)Mathf.Clamp(saved, 0, 2);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
