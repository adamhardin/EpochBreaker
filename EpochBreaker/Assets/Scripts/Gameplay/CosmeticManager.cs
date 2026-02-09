using UnityEngine;

namespace EpochBreaker.Gameplay
{
    // ── Cosmetic enums ──

    public enum PlayerSkin
    {
        Default,        // Original blue armor — always available
        Gold,           // Campaign: Epoch 0 (Stone Age)
        Shadow,         // Campaign: Epoch 2 (Classical)
        Crimson,        // Campaign: Epoch 4 (Renaissance)
        Glacier,        // Campaign: Epoch 6 (Modern)
        Neon,           // Campaign: Epoch 7 (Digital)
        Archaeologist,  // Campaign: Epoch 8 (Spacefaring)
        Rainbow         // Campaign: All 10 epochs completed
    }

    public enum TrailEffect
    {
        None,           // Default — no trail
        Sparks,         // Campaign: Epoch 1 (Bronze Age)
        Frost,          // Campaign: Epoch 3 (Medieval)
        Fire,           // Campaign: Epoch 5 (Industrial)
        Glitch          // Campaign: All 10 epochs completed
    }

    public enum ProfileFrame
    {
        None,           // No frame (default)
        Bronze,         // Campaign: All 10 epochs completed
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
        public static bool GodMode { get; set; }

        /// <summary>Fired when a cosmetic is newly unlocked. Carries display name like "Gold Skin".</summary>
        public static event System.Action<string> OnCosmeticUnlocked;

        public PlayerSkin SelectedSkin { get; private set; } = PlayerSkin.Default;
        public TrailEffect SelectedTrail { get; private set; } = TrailEffect.None;
        public ProfileFrame SelectedFrame { get; private set; } = ProfileFrame.None;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[Singleton] CosmeticManager duplicate detected — destroying new instance.");
                Destroy(this);
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
            SafePrefs.Save();
        }

        public void SetTrail(TrailEffect trail)
        {
            if (!IsTrailUnlocked(trail)) return;
            SelectedTrail = trail;
            PlayerPrefs.SetInt(PREF_TRAIL, (int)trail);
            SafePrefs.Save();
        }

        public void SetFrame(ProfileFrame frame)
        {
            if (!IsFrameUnlocked(frame)) return;
            SelectedFrame = frame;
            PlayerPrefs.SetInt(PREF_FRAME, (int)frame);
            SafePrefs.Save();
        }

        // ── Unlock checks ──

        public bool IsSkinUnlocked(PlayerSkin skin)
        {
            if (GodMode) return true;
            var am = AchievementManager.Instance;
            if (am == null) return skin == PlayerSkin.Default;

            return skin switch
            {
                PlayerSkin.Default      => true,
                PlayerSkin.Gold         => am.IsCampaignEpochCompleted(0),
                PlayerSkin.Shadow       => am.IsCampaignEpochCompleted(2),
                PlayerSkin.Crimson      => am.IsCampaignEpochCompleted(4),
                PlayerSkin.Glacier      => am.IsCampaignEpochCompleted(6),
                PlayerSkin.Neon         => am.IsCampaignEpochCompleted(7),
                PlayerSkin.Archaeologist => am.IsCampaignEpochCompleted(8),
                PlayerSkin.Rainbow      => am.GetCampaignEpochCount() >= 10,
                _ => false
            };
        }

        public bool IsTrailUnlocked(TrailEffect trail)
        {
            if (GodMode) return true;
            var am = AchievementManager.Instance;
            if (am == null) return trail == TrailEffect.None;

            return trail switch
            {
                TrailEffect.None   => true,
                TrailEffect.Sparks => am.IsCampaignEpochCompleted(1),
                TrailEffect.Frost  => am.IsCampaignEpochCompleted(3),
                TrailEffect.Fire   => am.IsCampaignEpochCompleted(5),
                TrailEffect.Glitch => am.GetCampaignEpochCount() >= 10,
                _ => false
            };
        }

        public bool IsFrameUnlocked(ProfileFrame frame)
        {
            if (GodMode) return true;
            var am = AchievementManager.Instance;
            return frame switch
            {
                ProfileFrame.None   => true,
                ProfileFrame.Bronze => am != null && am.GetCampaignEpochCount() >= 10,
                ProfileFrame.Silver => GetBestStreak() >= 10,
                ProfileFrame.Gold   => GetBestStreak() >= 25,
                _ => false
            };
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

        /// <summary>
        /// Check if completing a campaign epoch unlocked any new cosmetics, and fire notification events.
        /// Called by AchievementManager after recording a campaign epoch completion.
        /// </summary>
        public static void NotifyNewUnlocksForEpoch(int epoch)
        {
            // Map epoch → cosmetic reward
            switch (epoch)
            {
                case 0: OnCosmeticUnlocked?.Invoke("Gold Skin"); break;
                case 1: OnCosmeticUnlocked?.Invoke("Sparks Trail"); break;
                case 2: OnCosmeticUnlocked?.Invoke("Shadow Skin"); break;
                case 3: OnCosmeticUnlocked?.Invoke("Frost Trail"); break;
                case 4: OnCosmeticUnlocked?.Invoke("Crimson Skin"); break;
                case 5: OnCosmeticUnlocked?.Invoke("Fire Trail"); break;
                case 6: OnCosmeticUnlocked?.Invoke("Glacier Skin"); break;
                case 7: OnCosmeticUnlocked?.Invoke("Neon Skin"); break;
                case 8: OnCosmeticUnlocked?.Invoke("Archaeologist Skin"); break;
            }
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
                PlayerSkin.Gold         => "Campaign: Stone Age (Epoch 0)",
                PlayerSkin.Shadow       => "Campaign: Classical (Epoch 2)",
                PlayerSkin.Crimson      => "Campaign: Renaissance (Epoch 4)",
                PlayerSkin.Glacier      => "Campaign: Modern (Epoch 6)",
                PlayerSkin.Neon         => "Campaign: Digital (Epoch 7)",
                PlayerSkin.Archaeologist => "Campaign: Spacefaring (Epoch 8)",
                PlayerSkin.Rainbow      => "Campaign: Complete all 10 epochs",
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
                TrailEffect.Sparks => "Campaign: Bronze Age (Epoch 1)",
                TrailEffect.Frost  => "Campaign: Medieval (Epoch 3)",
                TrailEffect.Fire   => "Campaign: Industrial (Epoch 5)",
                TrailEffect.Glitch => "Campaign: Complete all 10 epochs",
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
                ProfileFrame.Bronze => "Campaign: Complete all 10 epochs",
                ProfileFrame.Silver => "Streak of 10+",
                ProfileFrame.Gold   => "Streak of 25+",
                _ => ""
            };
        }
    }
}
