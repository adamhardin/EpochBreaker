using UnityEngine;
using System;
using System.Collections.Generic;

namespace SixteenBit.Gameplay
{
    /// <summary>
    /// Achievement types. Each achievement has unique unlock criteria.
    /// </summary>
    public enum AchievementType
    {
        // Destruction achievements
        Demolisher,           // Destroy 100 destructible blocks (cumulative)
        TotalDemolition,      // Clear ALL destructible blocks in a single level
        ChainReaction,        // Destroy 10 blocks in 5 seconds

        // Combat achievements
        FirstBlood,           // Kill your first enemy
        EnemySlayer,          // Kill 50 enemies (cumulative)
        Untouchable,          // Complete a level without taking damage
        BossKiller,           // Defeat a boss
        KillStreak5,          // Kill 5 enemies without taking damage
        KillStreak10,         // Kill 10 enemies without taking damage

        // Collection achievements
        Collector,            // Collect 50 items (cumulative)
        Hoarder,              // Collect all items in a single level
        WeaponMaster,         // Acquire all weapon tiers in one level

        // Speed achievements
        SpeedDemon,           // Complete any level in under 60 seconds
        SpeedRunner,          // Complete any level in under 45 seconds
        LightningFast,        // Complete any level in under 30 seconds

        // Completion achievements
        FirstVictory,         // Complete your first level
        EpochExplorer,        // Complete a level in each epoch (0-9)
        PerfectRun,           // 3-star rating on any level
        Veteran,              // Complete 10 levels (cumulative)
        Master,               // Complete 50 levels (cumulative)

        // Score achievements
        HighScorer,           // Score over 10,000 in a single level
        ScoreLegend,          // Score over 25,000 in a single level
    }

    /// <summary>
    /// Persisted achievement data.
    /// </summary>
    [Serializable]
    public class AchievementData
    {
        public string Id;
        public bool Unlocked;
        public long UnlockTimestamp;
        public int Progress;        // For cumulative achievements
        public int ProgressTarget;  // Total needed to unlock
    }

    /// <summary>
    /// Container for all achievement data, serialized to PlayerPrefs.
    /// </summary>
    [Serializable]
    public class AchievementSaveData
    {
        public List<AchievementData> Achievements = new List<AchievementData>();
        public int TotalBlocksDestroyed;
        public int TotalEnemiesKilled;
        public int TotalItemsCollected;
        public int TotalLevelsCompleted;
        public int EpochsCompleted;  // Bitmask of completed epochs (bits 0-9)
    }

    /// <summary>
    /// Manages achievement tracking, unlocking, and persistence.
    /// Uses PlayerPrefs for cross-session persistence.
    /// </summary>
    public class AchievementManager : MonoBehaviour
    {
        private const string SAVE_KEY = "EpochBreaker_Achievements";

        public static AchievementManager Instance { get; private set; }

        private AchievementSaveData _saveData;
        private Dictionary<AchievementType, AchievementData> _achievementMap;

        // Session tracking (reset per level)
        private int _levelBlocksDestroyed;
        private int _levelTotalBlocks;
        private int _levelEnemiesKilled;
        private int _levelItemsCollected;
        private int _levelTotalItems;
        private int _currentKillStreak;
        private bool _tookDamageThisLevel;
        private float _lastBlockDestroyTime;
        private int _recentBlockDestroyCount;
        private HashSet<Generative.WeaponTier> _weaponsAcquired;

        public event Action<AchievementType> OnAchievementUnlocked;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadAchievements();
            _weaponsAcquired = new HashSet<Generative.WeaponTier>();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// Initialize achievements for a new level.
        /// </summary>
        public void StartLevel(int totalDestructibleBlocks, int totalItems)
        {
            _levelBlocksDestroyed = 0;
            _levelTotalBlocks = totalDestructibleBlocks;
            _levelEnemiesKilled = 0;
            _levelItemsCollected = 0;
            _levelTotalItems = totalItems;
            _currentKillStreak = 0;
            _tookDamageThisLevel = false;
            _lastBlockDestroyTime = 0f;
            _recentBlockDestroyCount = 0;
            _weaponsAcquired.Clear();
            _weaponsAcquired.Add(Generative.WeaponTier.Starting); // Player starts with this
        }

        /// <summary>
        /// Called when a destructible block is destroyed.
        /// </summary>
        public void RecordBlockDestroyed()
        {
            _levelBlocksDestroyed++;
            _saveData.TotalBlocksDestroyed++;

            // Check chain reaction
            float now = Time.time;
            if (now - _lastBlockDestroyTime < 5f)
            {
                _recentBlockDestroyCount++;
                if (_recentBlockDestroyCount >= 10)
                {
                    TryUnlock(AchievementType.ChainReaction);
                }
            }
            else
            {
                _recentBlockDestroyCount = 1;
            }
            _lastBlockDestroyTime = now;

            // Cumulative demolisher
            UpdateProgress(AchievementType.Demolisher, _saveData.TotalBlocksDestroyed, 100);

            SaveAchievements();
        }

        /// <summary>
        /// Called when an enemy is killed.
        /// </summary>
        public void RecordEnemyKilled()
        {
            _levelEnemiesKilled++;
            _currentKillStreak++;
            _saveData.TotalEnemiesKilled++;

            // First kill
            if (_saveData.TotalEnemiesKilled == 1)
                TryUnlock(AchievementType.FirstBlood);

            // Kill streaks
            if (_currentKillStreak >= 5)
                TryUnlock(AchievementType.KillStreak5);
            if (_currentKillStreak >= 10)
                TryUnlock(AchievementType.KillStreak10);

            // Cumulative slayer
            UpdateProgress(AchievementType.EnemySlayer, _saveData.TotalEnemiesKilled, 50);

            SaveAchievements();
        }

        /// <summary>
        /// Called when the player takes damage.
        /// </summary>
        public void RecordDamageTaken()
        {
            _tookDamageThisLevel = true;
            _currentKillStreak = 0;
        }

        /// <summary>
        /// Called when an item is collected.
        /// </summary>
        public void RecordItemCollected()
        {
            _levelItemsCollected++;
            _saveData.TotalItemsCollected++;

            // Cumulative collector
            UpdateProgress(AchievementType.Collector, _saveData.TotalItemsCollected, 50);

            SaveAchievements();
        }

        /// <summary>
        /// Called when a weapon is acquired.
        /// </summary>
        public void RecordWeaponAcquired(Generative.WeaponTier tier)
        {
            _weaponsAcquired.Add(tier);

            // Check if player has all tiers
            if (_weaponsAcquired.Contains(Generative.WeaponTier.Starting) &&
                _weaponsAcquired.Contains(Generative.WeaponTier.Medium) &&
                _weaponsAcquired.Contains(Generative.WeaponTier.Heavy))
            {
                TryUnlock(AchievementType.WeaponMaster);
            }
        }

        /// <summary>
        /// Called when a level is completed.
        /// </summary>
        public void RecordLevelComplete(int epoch, float elapsedTime, int score, int stars)
        {
            _saveData.TotalLevelsCompleted++;

            // First victory
            if (_saveData.TotalLevelsCompleted == 1)
                TryUnlock(AchievementType.FirstVictory);

            // Cumulative completion
            UpdateProgress(AchievementType.Veteran, _saveData.TotalLevelsCompleted, 10);
            UpdateProgress(AchievementType.Master, _saveData.TotalLevelsCompleted, 50);

            // Epoch explorer - mark this epoch as completed
            _saveData.EpochsCompleted |= (1 << epoch);
            if (_saveData.EpochsCompleted == 0x3FF) // All 10 epochs (bits 0-9)
                TryUnlock(AchievementType.EpochExplorer);

            // Perfect run (3 stars)
            if (stars >= 3)
                TryUnlock(AchievementType.PerfectRun);

            // Untouchable (no damage)
            if (!_tookDamageThisLevel)
                TryUnlock(AchievementType.Untouchable);

            // Total demolition (all blocks destroyed)
            if (_levelTotalBlocks > 0 && _levelBlocksDestroyed >= _levelTotalBlocks)
                TryUnlock(AchievementType.TotalDemolition);

            // Hoarder (all items collected)
            if (_levelTotalItems > 0 && _levelItemsCollected >= _levelTotalItems)
                TryUnlock(AchievementType.Hoarder);

            // Speed achievements
            if (elapsedTime < 60f)
                TryUnlock(AchievementType.SpeedDemon);
            if (elapsedTime < 45f)
                TryUnlock(AchievementType.SpeedRunner);
            if (elapsedTime < 30f)
                TryUnlock(AchievementType.LightningFast);

            // Score achievements
            if (score >= 10000)
                TryUnlock(AchievementType.HighScorer);
            if (score >= 25000)
                TryUnlock(AchievementType.ScoreLegend);

            SaveAchievements();
        }

        /// <summary>
        /// Called when a boss is defeated.
        /// </summary>
        public void RecordBossDefeated()
        {
            TryUnlock(AchievementType.BossKiller);
            SaveAchievements();
        }

        /// <summary>
        /// Try to unlock an achievement if not already unlocked.
        /// </summary>
        private void TryUnlock(AchievementType type)
        {
            if (!_achievementMap.TryGetValue(type, out var data))
                return;

            if (data.Unlocked)
                return;

            data.Unlocked = true;
            data.UnlockTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            Debug.Log($"[Achievement] Unlocked: {type}");
            OnAchievementUnlocked?.Invoke(type);

            SaveAchievements();
        }

        /// <summary>
        /// Update progress on a cumulative achievement.
        /// </summary>
        private void UpdateProgress(AchievementType type, int current, int target)
        {
            if (!_achievementMap.TryGetValue(type, out var data))
                return;

            if (data.Unlocked)
                return;

            data.Progress = current;
            data.ProgressTarget = target;

            if (current >= target)
                TryUnlock(type);
        }

        /// <summary>
        /// Check if an achievement is unlocked.
        /// </summary>
        public bool IsUnlocked(AchievementType type)
        {
            return _achievementMap.TryGetValue(type, out var data) && data.Unlocked;
        }

        /// <summary>
        /// Get achievement data for UI display.
        /// </summary>
        public AchievementData GetAchievement(AchievementType type)
        {
            return _achievementMap.TryGetValue(type, out var data) ? data : null;
        }

        /// <summary>
        /// Get all achievements for UI display.
        /// </summary>
        public List<AchievementData> GetAllAchievements()
        {
            return _saveData.Achievements;
        }

        /// <summary>
        /// Get count of unlocked achievements.
        /// </summary>
        public (int unlocked, int total) GetUnlockCount()
        {
            int unlocked = 0;
            foreach (var a in _saveData.Achievements)
            {
                if (a.Unlocked) unlocked++;
            }
            return (unlocked, _saveData.Achievements.Count);
        }

        /// <summary>
        /// Get user-friendly name for an achievement.
        /// </summary>
        public static string GetAchievementName(AchievementType type)
        {
            return type switch
            {
                AchievementType.Demolisher => "Demolisher",
                AchievementType.TotalDemolition => "Total Demolition",
                AchievementType.ChainReaction => "Chain Reaction",
                AchievementType.FirstBlood => "First Blood",
                AchievementType.EnemySlayer => "Enemy Slayer",
                AchievementType.Untouchable => "Untouchable",
                AchievementType.BossKiller => "Boss Killer",
                AchievementType.KillStreak5 => "Killing Spree",
                AchievementType.KillStreak10 => "Rampage",
                AchievementType.Collector => "Collector",
                AchievementType.Hoarder => "Hoarder",
                AchievementType.WeaponMaster => "Weapon Master",
                AchievementType.SpeedDemon => "Speed Demon",
                AchievementType.SpeedRunner => "Speed Runner",
                AchievementType.LightningFast => "Lightning Fast",
                AchievementType.FirstVictory => "First Victory",
                AchievementType.EpochExplorer => "Epoch Explorer",
                AchievementType.PerfectRun => "Perfect Run",
                AchievementType.Veteran => "Veteran",
                AchievementType.Master => "Master",
                AchievementType.HighScorer => "High Scorer",
                AchievementType.ScoreLegend => "Score Legend",
                _ => type.ToString()
            };
        }

        /// <summary>
        /// Get user-friendly description for an achievement.
        /// </summary>
        public static string GetAchievementDescription(AchievementType type)
        {
            return type switch
            {
                AchievementType.Demolisher => "Destroy 100 destructible blocks",
                AchievementType.TotalDemolition => "Clear ALL destructible blocks in a level",
                AchievementType.ChainReaction => "Destroy 10 blocks within 5 seconds",
                AchievementType.FirstBlood => "Kill your first enemy",
                AchievementType.EnemySlayer => "Kill 50 enemies",
                AchievementType.Untouchable => "Complete a level without taking damage",
                AchievementType.BossKiller => "Defeat a boss",
                AchievementType.KillStreak5 => "Kill 5 enemies without taking damage",
                AchievementType.KillStreak10 => "Kill 10 enemies without taking damage",
                AchievementType.Collector => "Collect 50 items",
                AchievementType.Hoarder => "Collect all items in a level",
                AchievementType.WeaponMaster => "Acquire all weapon tiers in one level",
                AchievementType.SpeedDemon => "Complete a level in under 60 seconds",
                AchievementType.SpeedRunner => "Complete a level in under 45 seconds",
                AchievementType.LightningFast => "Complete a level in under 30 seconds",
                AchievementType.FirstVictory => "Complete your first level",
                AchievementType.EpochExplorer => "Complete a level in each epoch",
                AchievementType.PerfectRun => "Achieve a 3-star rating",
                AchievementType.Veteran => "Complete 10 levels",
                AchievementType.Master => "Complete 50 levels",
                AchievementType.HighScorer => "Score over 10,000 in a level",
                AchievementType.ScoreLegend => "Score over 25,000 in a level",
                _ => ""
            };
        }

        private void LoadAchievements()
        {
            _achievementMap = new Dictionary<AchievementType, AchievementData>();

            string json = PlayerPrefs.GetString(SAVE_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    _saveData = JsonUtility.FromJson<AchievementSaveData>(json);
                }
                catch
                {
                    _saveData = null;
                }
            }

            if (_saveData == null)
                _saveData = new AchievementSaveData();

            // Ensure all achievement types exist in save data
            foreach (AchievementType type in Enum.GetValues(typeof(AchievementType)))
            {
                string id = type.ToString();
                AchievementData existing = null;

                foreach (var a in _saveData.Achievements)
                {
                    if (a.Id == id)
                    {
                        existing = a;
                        break;
                    }
                }

                if (existing == null)
                {
                    existing = new AchievementData
                    {
                        Id = id,
                        Unlocked = false,
                        UnlockTimestamp = 0,
                        Progress = 0,
                        ProgressTarget = GetDefaultTarget(type)
                    };
                    _saveData.Achievements.Add(existing);
                }

                _achievementMap[type] = existing;
            }
        }

        private int GetDefaultTarget(AchievementType type)
        {
            return type switch
            {
                AchievementType.Demolisher => 100,
                AchievementType.EnemySlayer => 50,
                AchievementType.Collector => 50,
                AchievementType.Veteran => 10,
                AchievementType.Master => 50,
                _ => 1
            };
        }

        private void SaveAchievements()
        {
            string json = JsonUtility.ToJson(_saveData);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Reset all achievements (for testing).
        /// </summary>
        public void ResetAllAchievements()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            LoadAchievements();
            Debug.Log("[Achievement] All achievements reset");
        }
    }
}
