using UnityEngine;

namespace EpochBreaker.Gameplay
{
    // ── Cosmetic enums ──

    public enum PlayerSkin
    {
        Default,        // Original blue armor
        Gold,           // 3-star any level
        Shadow,         // Untouchable achievement
        Crimson,        // Boss Killer achievement
        Glacier,        // Cool Under Pressure achievement
        Neon,           // Score Legend achievement
        Archaeologist,  // Archaeologist achievement (all relics)
        Rainbow         // All achievements unlocked
    }

    public enum TrailEffect
    {
        None,           // Default — no trail
        Sparks,         // Unlocked at 5 levels completed
        Frost,          // Unlocked at 10 levels completed
        Fire,           // Unlocked at 20 levels completed
        Glitch          // Unlocked at 50 levels completed + all achievements
    }

    public enum ProfileFrame
    {
        None,           // No frame (default)
        Bronze,         // Campaign complete (all 10 epochs)
        Silver,         // Streak 10+
        Gold            // Streak 25+
    }

    /// <summary>
    /// Manages cosmetic unlocks, selection, and persistence.
    /// Zero-monetization cosmetics earned through gameplay.
    /// </summary>
    public class CosmeticManager : MonoBehaviour
    {
        private const string PREF_SKIN = "EpochBreaker_Cosmetic_Skin";
        private const string PREF_TRAIL = "EpochBreaker_Cosmetic_Trail";
        private const string PREF_FRAME = "EpochBreaker_Cosmetic_Frame";

        public static CosmeticManager Instance { get; private set; }

        public PlayerSkin SelectedSkin { get; private set; } = PlayerSkin.Default;
        public TrailEffect SelectedTrail { get; private set; } = TrailEffect.None;
        public ProfileFrame SelectedFrame { get; private set; } = ProfileFrame.None;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[Singleton] CosmeticManager duplicate detected — destroying new instance.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            LoadSelections();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // ── Persistence ──

        private void LoadSelections()
        {
            SelectedSkin = (PlayerSkin)PlayerPrefs.GetInt(PREF_SKIN, 0);
            SelectedTrail = (TrailEffect)PlayerPrefs.GetInt(PREF_TRAIL, 0);
            SelectedFrame = (ProfileFrame)PlayerPrefs.GetInt(PREF_FRAME, 0);

            // Clamp to valid range
            if ((int)SelectedSkin < 0 || (int)SelectedSkin > 7) SelectedSkin = PlayerSkin.Default;
            if ((int)SelectedTrail < 0 || (int)SelectedTrail > 4) SelectedTrail = TrailEffect.None;
            if ((int)SelectedFrame < 0 || (int)SelectedFrame > 3) SelectedFrame = ProfileFrame.None;
        }

        public void SetSkin(PlayerSkin skin)
        {
            if (!IsSkinUnlocked(skin)) return;
            SelectedSkin = skin;
            PlayerPrefs.SetInt(PREF_SKIN, (int)skin);
            PlayerPrefs.Save();
        }

        public void SetTrail(TrailEffect trail)
        {
            if (!IsTrailUnlocked(trail)) return;
            SelectedTrail = trail;
            PlayerPrefs.SetInt(PREF_TRAIL, (int)trail);
            PlayerPrefs.Save();
        }

        public void SetFrame(ProfileFrame frame)
        {
            if (!IsFrameUnlocked(frame)) return;
            SelectedFrame = frame;
            PlayerPrefs.SetInt(PREF_FRAME, (int)frame);
            PlayerPrefs.Save();
        }

        // ── Unlock checks ──

        public bool IsSkinUnlocked(PlayerSkin skin)
        {
            var am = AchievementManager.Instance;
            if (am == null) return skin == PlayerSkin.Default;

            return skin switch
            {
                PlayerSkin.Default      => true,
                PlayerSkin.Gold         => am.IsUnlocked(AchievementType.PerfectRun),
                PlayerSkin.Shadow       => am.IsUnlocked(AchievementType.Untouchable),
                PlayerSkin.Crimson      => am.IsUnlocked(AchievementType.BossKiller),
                PlayerSkin.Glacier      => am.IsUnlocked(AchievementType.CoolUnderPressure),
                PlayerSkin.Neon         => am.IsUnlocked(AchievementType.ScoreLegend),
                PlayerSkin.Archaeologist => am.IsUnlocked(AchievementType.Archaeologist),
                PlayerSkin.Rainbow      => AreAllAchievementsUnlocked(),
                _ => false
            };
        }

        public bool IsTrailUnlocked(TrailEffect trail)
        {
            var am = AchievementManager.Instance;
            int levelsCompleted = am != null ? am.GetTotalLevelsCompleted() : 0;

            return trail switch
            {
                TrailEffect.None   => true,
                TrailEffect.Sparks => levelsCompleted >= 5,
                TrailEffect.Frost  => levelsCompleted >= 10,
                TrailEffect.Fire   => levelsCompleted >= 20,
                TrailEffect.Glitch => levelsCompleted >= 50 && AreAllAchievementsUnlocked(),
                _ => false
            };
        }

        public bool IsFrameUnlocked(ProfileFrame frame)
        {
            return frame switch
            {
                ProfileFrame.None   => true,
                ProfileFrame.Bronze => IsCampaignComplete(),
                ProfileFrame.Silver => GetBestStreak() >= 10,
                ProfileFrame.Gold   => GetBestStreak() >= 25,
                _ => false
            };
        }

        private bool AreAllAchievementsUnlocked()
        {
            var am = AchievementManager.Instance;
            if (am == null) return false;
            var (unlocked, total) = am.GetUnlockCount();
            return total > 0 && unlocked >= total;
        }

        /// <summary>
        /// Check if all 10 campaign epochs have been completed.
        /// Uses AchievementManager's EpochExplorer achievement as proxy.
        /// </summary>
        private bool IsCampaignComplete()
        {
            var am = AchievementManager.Instance;
            return am != null && am.IsUnlocked(AchievementType.EpochExplorer);
        }

        /// <summary>
        /// Get best streak from Legends data.
        /// </summary>
        private int GetBestStreak()
        {
            var legends = GameManager.LoadLegendsData();
            if (legends?.Entries == null || legends.Entries.Count == 0) return 0;

            int best = 0;
            foreach (var entry in legends.Entries)
            {
                if (entry.StreakCount > best)
                    best = entry.StreakCount;
            }
            return best;
        }

        // ── Skin colors ──

        /// <summary>
        /// Get the color tint to apply to the player sprite for a given skin.
        /// Color.white means no tint (original sprite).
        /// </summary>
        public static Color GetSkinTint(PlayerSkin skin)
        {
            return skin switch
            {
                PlayerSkin.Default      => Color.white,
                PlayerSkin.Gold         => new Color(1.0f, 0.85f, 0.2f),
                PlayerSkin.Shadow       => new Color(0.45f, 0.35f, 0.55f),
                PlayerSkin.Crimson      => new Color(1.0f, 0.3f, 0.25f),
                PlayerSkin.Glacier      => new Color(0.6f, 0.85f, 1.0f),
                PlayerSkin.Neon         => new Color(0.3f, 1.0f, 0.7f),
                PlayerSkin.Archaeologist => new Color(0.75f, 0.6f, 0.4f),
                PlayerSkin.Rainbow      => Color.white, // Special — handled via hue shift
                _ => Color.white
            };
        }

        /// <summary>
        /// Get the trail particle primary color for a given trail type.
        /// </summary>
        public static Color GetTrailColor(TrailEffect trail)
        {
            return trail switch
            {
                TrailEffect.Sparks => new Color(1.0f, 0.7f, 0.2f),
                TrailEffect.Frost  => new Color(0.7f, 0.9f, 1.0f),
                TrailEffect.Fire   => new Color(1.0f, 0.4f, 0.15f),
                TrailEffect.Glitch => new Color(0.3f, 1.0f, 0.9f),
                _ => Color.clear
            };
        }

        /// <summary>
        /// Get the trail particle secondary color (for variety).
        /// </summary>
        public static Color GetTrailSecondaryColor(TrailEffect trail)
        {
            return trail switch
            {
                TrailEffect.Sparks => new Color(1.0f, 0.9f, 0.4f),
                TrailEffect.Frost  => new Color(0.9f, 0.95f, 1.0f),
                TrailEffect.Fire   => new Color(1.0f, 0.6f, 0.1f),
                TrailEffect.Glitch => new Color(0.9f, 0.2f, 0.9f),
                _ => Color.clear
            };
        }

        /// <summary>
        /// Get the profile frame border color.
        /// </summary>
        public static Color GetFrameColor(ProfileFrame frame)
        {
            return frame switch
            {
                ProfileFrame.Bronze => new Color(0.72f, 0.52f, 0.15f),
                ProfileFrame.Silver => new Color(0.75f, 0.75f, 0.80f),
                ProfileFrame.Gold   => new Color(1.0f, 0.85f, 0.2f),
                _ => Color.clear
            };
        }

        // ── Display names ──

        public static string GetSkinName(PlayerSkin skin)
        {
            return skin switch
            {
                PlayerSkin.Default      => "DEFAULT",
                PlayerSkin.Gold         => "GOLD",
                PlayerSkin.Shadow       => "SHADOW",
                PlayerSkin.Crimson      => "CRIMSON",
                PlayerSkin.Glacier      => "GLACIER",
                PlayerSkin.Neon         => "NEON",
                PlayerSkin.Archaeologist => "ARCHAEOLOGIST",
                PlayerSkin.Rainbow      => "RAINBOW",
                _ => "UNKNOWN"
            };
        }

        public static string GetSkinRequirement(PlayerSkin skin)
        {
            return skin switch
            {
                PlayerSkin.Default      => "Always available",
                PlayerSkin.Gold         => "3-star any level",
                PlayerSkin.Shadow       => "Untouchable (no damage)",
                PlayerSkin.Crimson      => "Defeat a boss",
                PlayerSkin.Glacier      => "Cool Under Pressure",
                PlayerSkin.Neon         => "Score Legend (25,000+)",
                PlayerSkin.Archaeologist => "Preserve all relics",
                PlayerSkin.Rainbow      => "All achievements",
                _ => ""
            };
        }

        public static string GetTrailName(TrailEffect trail)
        {
            return trail switch
            {
                TrailEffect.None   => "NONE",
                TrailEffect.Sparks => "SPARKS",
                TrailEffect.Frost  => "FROST",
                TrailEffect.Fire   => "FIRE",
                TrailEffect.Glitch => "GLITCH",
                _ => "UNKNOWN"
            };
        }

        public static string GetTrailRequirement(TrailEffect trail)
        {
            return trail switch
            {
                TrailEffect.None   => "Always available",
                TrailEffect.Sparks => "Complete 5 levels",
                TrailEffect.Frost  => "Complete 10 levels",
                TrailEffect.Fire   => "Complete 20 levels",
                TrailEffect.Glitch => "Complete 50 levels + all achievements",
                _ => ""
            };
        }

        public static string GetFrameName(ProfileFrame frame)
        {
            return frame switch
            {
                ProfileFrame.None   => "NONE",
                ProfileFrame.Bronze => "BRONZE",
                ProfileFrame.Silver => "SILVER",
                ProfileFrame.Gold   => "GOLD",
                _ => "UNKNOWN"
            };
        }

        public static string GetFrameRequirement(ProfileFrame frame)
        {
            return frame switch
            {
                ProfileFrame.None   => "Always available",
                ProfileFrame.Bronze => "Complete all 10 epochs",
                ProfileFrame.Silver => "Streak of 10+",
                ProfileFrame.Gold   => "Streak of 25+",
                _ => ""
            };
        }
    }
}
