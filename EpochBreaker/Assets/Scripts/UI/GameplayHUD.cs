using UnityEngine;
using UnityEngine.UI;

namespace EpochBreaker.UI
{
    /// <summary>
    /// In-game HUD showing health hearts, score, weapon info, heat bar, and pause button.
    /// </summary>
    public class GameplayHUD : MonoBehaviour
    {
        private Canvas _canvas;
        private Image[] _hearts;
        private Text _timerText;
        private Text _weaponText;
        private Text _eraText;
        private Text _livesText;
        private Text _modeInfoText;
        private Image _heatBarBg;
        private Image _heatBarFill;
        private Text _relicText;
        private Text _killStreakText;
        private float _killStreakFadeTimer;
        private int _lastKnownStreak;
        private Gameplay.HealthSystem _healthSystem;
        private Gameplay.WeaponSystem _cachedWeaponSystem;
        private Gameplay.AbilitySystem _cachedAbilitySystem;

        // Sprint 4: Visible systems
        private Text _scoreText;
        private int _displayedScore;
        private float _scoreScaleTimer;
        private Image _doubleJumpIcon;
        private Image _airDashIcon;
        private Text _quickDrawText;
        private float _quickDrawFlashTimer;
        private Text _autoSelectText;
        private Text _dpsCapText;
        private float _dpsCapTimer;

        // Boss health bar
        private GameObject _bossBarRoot;
        private Image _bossBarBg;
        private Image _bossBarFill;
        private Text _bossNameText;
        private float _bossBarFlashTimer;
        private int _lastBossHealth = -1;

        // Score popups (pooled)
        private const int POPUP_POOL_SIZE = 6;
        private Text[] _popupTexts;
        private float[] _popupTimers;
        private Vector3[] _popupWorldPositions;

        // Combo counter
        private Text _comboText;

        // Achievement toast
        private Text _achievementText;
        private float _achievementTimer;
        private System.Collections.Generic.Queue<string> _achievementQueue
            = new System.Collections.Generic.Queue<string>();

        // Era intro card
        private Text _eraCardText;
        private float _eraCardTimer;
        private Color _eraCardColor;
        private Text _modeSubtitleText;
        private float _modeSubtitleTimer;

        // Special attack meter
        private Gameplay.SpecialAttackSystem _cachedSpecialAttack;
        private Image _specialBarBg;
        private Image _specialBarFill;
        private float _specialFlashTimer;

        // Damage direction indicators (4 edges: left, right, top, bottom)
        private Image[] _dmgDirImages;
        private float _dmgDirTimer;
        private int _dmgDirEdge = -1;

        // Sprint 8: Friend challenge target score display
        private Text _friendTargetText;

        // Weapon wheel (dynamic — only shows acquired weapons + Unarmed)
        private GameObject _weaponWheelRoot;
        private CanvasGroup _weaponWheelGroup;
        private RectTransform _wheelRingRect;
        private RectTransform _wheelBgRect;
        private GameObject[] _wheelSlotGOs;
        private Image[] _wheelSlotImages;
        private int[] _wheelSlotWeaponIdx; // -1 = unarmed, 0-5 = weapon type
        private int _wheelVisibleCount;
        private Text _weaponWheelLabel;
        private float _weaponWheelTimer;
        private int _lastWheelState = -99; // -1 = unarmed, 0-5 = weapon, -99 = uninitialized
        private int _lastAcquiredCount = -1;
        private const float WHEEL_HIGHLIGHT_DURATION = 1.5f;
        private const float WHEEL_RESTING_ALPHA = 0.9f;
        private const float WHEEL_SLOT_SIZE = 50f;

        // Weapon upgrade notification
        private Text _upgradeNotifyText;
        private float _upgradeNotifyTimer;
        private int _lastTierHash = -1;

        private const int MAX_HEARTS = 5;

        private void Start()
        {
            CreateUI();
            FindPlayerHealth();

            // Subscribe to score popup events
            Gameplay.GameManager.OnScorePopup += HandleScorePopup;

            // Subscribe to damage direction events
            Gameplay.GameManager.OnDamageDirection += HandleDamageDirection;

            // Subscribe to weapon upgrade events
            Gameplay.WeaponSystem.OnWeaponUpgraded += HandleWeaponUpgrade;

            // Subscribe to cosmetic unlock events
            Gameplay.CosmeticManager.OnCosmeticUnlocked += HandleCosmeticUnlock;

            // Subscribe to achievement unlock events
            if (Gameplay.AchievementManager.Instance != null)
                Gameplay.AchievementManager.Instance.OnAchievementUnlocked += HandleAchievementUnlock;
        }

        private void Update()
        {
            UpdateTimer();
            UpdateWeaponDisplay();
            UpdateHeatBar();
            UpdateRelicCounter();
            UpdateModeInfo();
            UpdateKillStreak();
            UpdateBossBar();
            UpdateRunningScore();
            UpdateAbilityIcons();
            UpdateQuickDraw();
            UpdateAutoSelectReason();
            UpdateDpsCapFeedback();
            UpdateScorePopups();
            UpdateComboCounter();
            UpdateAchievementToast();
            UpdateEraCard();
            UpdateModeSubtitle();
            UpdateDamageDirection();
            UpdateSpecialAttackMeter();
            UpdateFriendTarget();
            UpdateUpgradeNotification();
        }

        private void FindPlayerHealth()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                _healthSystem = player.GetComponent<Gameplay.HealthSystem>();
                _cachedWeaponSystem = player.GetComponent<Gameplay.WeaponSystem>();
                _cachedAbilitySystem = player.GetComponent<Gameplay.AbilitySystem>();
                if (_healthSystem != null)
                {
                    _healthSystem.OnHealthChanged += UpdateHearts;
                    UpdateHearts(_healthSystem.CurrentHealth, _healthSystem.MaxHealth);
                }
            }
        }

        private void OnDestroy()
        {
            if (_healthSystem != null)
                _healthSystem.OnHealthChanged -= UpdateHearts;

            Gameplay.GameManager.OnScorePopup -= HandleScorePopup;
            Gameplay.GameManager.OnDamageDirection -= HandleDamageDirection;
            Gameplay.WeaponSystem.OnWeaponUpgraded -= HandleWeaponUpgrade;
            Gameplay.CosmeticManager.OnCosmeticUnlocked -= HandleCosmeticUnlock;

            if (Gameplay.AchievementManager.Instance != null)
                Gameplay.AchievementManager.Instance.OnAchievementUnlocked -= HandleAchievementUnlock;
        }

        private void CreateUI()
        {
            var canvasGO = new GameObject("HUDCanvas");
            canvasGO.transform.SetParent(transform);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 90;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Hearts (top-left)
            _hearts = new Image[MAX_HEARTS];
            for (int i = 0; i < MAX_HEARTS; i++)
            {
                var heartGO = new GameObject($"Heart_{i}");
                heartGO.transform.SetParent(canvasGO.transform, false);

                var img = heartGO.AddComponent<Image>();
                img.color = Color.red;

                var rect = heartGO.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.sizeDelta = new Vector2(40, 36);
                rect.anchoredPosition = new Vector2(20 + i * 48, -20);

                _hearts[i] = img;
            }

            // Special attack meter (below hearts)
            CreateSpecialAttackBar(canvasGO.transform);

            // Timer (top-left, below hearts)
            var timerGO = CreateHUDText(canvasGO.transform, "0:00.0", TextAnchor.UpperLeft);
            _timerText = timerGO.GetComponent<Text>();
            _timerText.fontSize = 26;
            var timerRect = timerGO.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0, 1);
            timerRect.anchorMax = new Vector2(0, 1);
            timerRect.pivot = new Vector2(0, 1);
            timerRect.anchoredPosition = new Vector2(20, -70);

            // Weapon type + tier (top-right)
            var weaponGO = CreateHUDText(canvasGO.transform, "Bolt", TextAnchor.UpperRight);
            _weaponText = weaponGO.GetComponent<Text>();
            var weaponRect = weaponGO.GetComponent<RectTransform>();
            weaponRect.anchorMin = new Vector2(1, 1);
            weaponRect.anchorMax = new Vector2(1, 1);
            weaponRect.pivot = new Vector2(1, 1);
            weaponRect.anchoredPosition = new Vector2(-20, -20);

            // Weapon wheel (lower-left, appears on weapon cycle)
            CreateWeaponWheel(canvasGO.transform);

            // Heat bar (top-right, below weapon name — hidden by default)
            CreateHeatBar(canvasGO.transform);

            // Era name (top-right, below heat bar)
            var eraGO = CreateHUDText(canvasGO.transform, "", TextAnchor.UpperRight);
            _eraText = eraGO.GetComponent<Text>();
            _eraText.fontSize = 18;
            var eraRect = eraGO.GetComponent<RectTransform>();
            eraRect.anchorMin = new Vector2(1, 1);
            eraRect.anchorMax = new Vector2(1, 1);
            eraRect.pivot = new Vector2(1, 1);
            eraRect.anchoredPosition = new Vector2(-20, -82);

            var gm = Gameplay.GameManager.Instance;
            if (gm != null && gm.CurrentLevelID.Epoch >= 0)
                _eraText.text = $"{gm.CurrentLevelID.EpochName} [{gm.CurrentLevelID.ToCode()}]";

            // Relic counter (below timer, left side — only shown if level has relics)
            if (gm != null && gm.TotalRelics > 0)
            {
                var relicGO = CreateHUDText(canvasGO.transform, "", TextAnchor.UpperLeft);
                _relicText = relicGO.GetComponent<Text>();
                _relicText.fontSize = 20;
                _relicText.color = new Color(1f, 0.85f, 0.2f);
                var relicRect = relicGO.GetComponent<RectTransform>();
                relicRect.anchorMin = new Vector2(0, 1);
                relicRect.anchorMax = new Vector2(0, 1);
                relicRect.pivot = new Vector2(0, 1);
                relicRect.anchoredPosition = new Vector2(20, -96);
            }

            // Pause button (top-right corner)
            CreatePauseButton(canvasGO.transform);

            // Running score (top-center)
            var scoreGO = CreateHUDText(canvasGO.transform, "0", TextAnchor.UpperCenter);
            _scoreText = scoreGO.GetComponent<Text>();
            _scoreText.fontSize = 28;
            _scoreText.color = Color.white;
            var scoreRect = scoreGO.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0.5f, 1);
            scoreRect.anchorMax = new Vector2(0.5f, 1);
            scoreRect.pivot = new Vector2(0.5f, 1);
            scoreRect.anchoredPosition = new Vector2(0, -46); // Below mode info text at -20
            scoreRect.sizeDelta = new Vector2(600, 40); // Cap width to prevent overlap with hearts/weapon

            // Ability icons (below hearts)
            CreateAbilityIcons(canvasGO.transform);

            // Quick Draw flash text (below score, hidden by default)
            var qdGO = CreateHUDText(canvasGO.transform, "QUICK DRAW!", TextAnchor.UpperCenter);
            _quickDrawText = qdGO.GetComponent<Text>();
            _quickDrawText.fontSize = 20;
            _quickDrawText.color = new Color(1f, 0.85f, 0.2f, 0f);
            var qdRect = qdGO.GetComponent<RectTransform>();
            qdRect.anchorMin = new Vector2(0.5f, 1);
            qdRect.anchorMax = new Vector2(0.5f, 1);
            qdRect.pivot = new Vector2(0.5f, 1);
            qdRect.anchoredPosition = new Vector2(0, -50);

            // Auto-select reason (below weapon name, right side)
            var asGO = CreateHUDText(canvasGO.transform, "", TextAnchor.UpperRight);
            _autoSelectText = asGO.GetComponent<Text>();
            _autoSelectText.fontSize = 16;
            _autoSelectText.color = new Color(0.7f, 0.8f, 0.5f, 0f);
            var asRect = asGO.GetComponent<RectTransform>();
            asRect.anchorMin = new Vector2(1, 1);
            asRect.anchorMax = new Vector2(1, 1);
            asRect.pivot = new Vector2(1, 1);
            asRect.anchoredPosition = new Vector2(-20, -96);

            // DPS cap "RESISTED" text (center, hidden)
            var dpsGO = CreateHUDText(canvasGO.transform, "RESISTED", TextAnchor.MiddleCenter);
            _dpsCapText = dpsGO.GetComponent<Text>();
            _dpsCapText.fontSize = 18;
            _dpsCapText.color = new Color(0.5f, 0.5f, 0.5f, 0f);
            var dpsRect = dpsGO.GetComponent<RectTransform>();
            dpsRect.anchorMin = new Vector2(0.5f, 1);
            dpsRect.anchorMax = new Vector2(0.5f, 1);
            dpsRect.pivot = new Vector2(0.5f, 1);
            dpsRect.anchoredPosition = new Vector2(0, -70);

            // Kill streak indicator (center-bottom area, hidden by default)
            var streakGO = CreateHUDText(canvasGO.transform, "", TextAnchor.MiddleCenter);
            _killStreakText = streakGO.GetComponent<Text>();
            _killStreakText.fontSize = 32;
            _killStreakText.color = new Color(1f, 0.85f, 0.2f, 0f); // Start transparent
            var streakRect = streakGO.GetComponent<RectTransform>();
            streakRect.anchorMin = new Vector2(0.5f, 0.5f);
            streakRect.anchorMax = new Vector2(0.5f, 0.5f);
            streakRect.pivot = new Vector2(0.5f, 0.5f);
            streakRect.anchoredPosition = new Vector2(0, -200);

            // Lives counter (below timer/relic, for Campaign/Streak)
            if (gm != null && gm.CurrentGameMode != Gameplay.GameMode.TheBreach)
            {
                float livesY = (gm.TotalRelics > 0) ? -122f : -100f;
                var livesGO = CreateHUDText(canvasGO.transform, "", TextAnchor.UpperLeft);
                _livesText = livesGO.GetComponent<Text>();
                _livesText.fontSize = 22;
                _livesText.color = new Color(1f, 0.85f, 0.2f);
                var livesRect = livesGO.GetComponent<RectTransform>();
                livesRect.anchorMin = new Vector2(0, 1);
                livesRect.anchorMax = new Vector2(0, 1);
                livesRect.pivot = new Vector2(0, 1);
                livesRect.anchoredPosition = new Vector2(20, livesY);
            }

            // Score popup pool
            _popupTexts = new Text[POPUP_POOL_SIZE];
            _popupTimers = new float[POPUP_POOL_SIZE];
            _popupWorldPositions = new Vector3[POPUP_POOL_SIZE];
            for (int i = 0; i < POPUP_POOL_SIZE; i++)
            {
                var popGO = CreateHUDText(canvasGO.transform, "", TextAnchor.MiddleCenter);
                _popupTexts[i] = popGO.GetComponent<Text>();
                _popupTexts[i].fontSize = 22;
                _popupTexts[i].color = new Color(1f, 0.85f, 0.2f, 0f);
                var popRect = popGO.GetComponent<RectTransform>();
                popRect.sizeDelta = new Vector2(200, 30);
            }

            // Combo counter (center of screen, below crosshair area)
            var comboGO = CreateHUDText(canvasGO.transform, "", TextAnchor.MiddleCenter);
            _comboText = comboGO.GetComponent<Text>();
            _comboText.fontSize = 36;
            _comboText.color = new Color(1f, 0.85f, 0.2f, 0f);
            var comboRect = comboGO.GetComponent<RectTransform>();
            comboRect.anchorMin = new Vector2(0.5f, 0.5f);
            comboRect.anchorMax = new Vector2(0.5f, 0.5f);
            comboRect.pivot = new Vector2(0.5f, 0.5f);
            comboRect.anchoredPosition = new Vector2(0, -150);

            // Achievement toast (slide from top)
            var achGO = CreateHUDText(canvasGO.transform, "", TextAnchor.MiddleCenter);
            _achievementText = achGO.GetComponent<Text>();
            _achievementText.fontSize = 20;
            _achievementText.color = new Color(1f, 0.85f, 0.2f, 0f);
            var achRect = achGO.GetComponent<RectTransform>();
            achRect.anchorMin = new Vector2(0.5f, 1);
            achRect.anchorMax = new Vector2(0.5f, 1);
            achRect.pivot = new Vector2(0.5f, 1);
            achRect.sizeDelta = new Vector2(400, 30);
            achRect.anchoredPosition = new Vector2(0, 40); // Starts off-screen above

            // Era intro card (large centered text, fades after 2s)
            var eraCardGO = CreateHUDText(canvasGO.transform, "", TextAnchor.MiddleCenter);
            _eraCardText = eraCardGO.GetComponent<Text>();
            _eraCardText.fontSize = 42;
            _eraCardText.color = new Color(1f, 1f, 1f, 0f);
            var eraCardRect = eraCardGO.GetComponent<RectTransform>();
            eraCardRect.anchorMin = new Vector2(0.5f, 0.5f);
            eraCardRect.anchorMax = new Vector2(0.5f, 0.5f);
            eraCardRect.pivot = new Vector2(0.5f, 0.5f);
            eraCardRect.sizeDelta = new Vector2(600, 60);
            eraCardRect.anchoredPosition = new Vector2(0, 50);

            if (gm != null)
            {
                int epoch = gm.CurrentEpoch;
                string eraName = gm.CurrentLevelID.EpochName.ToUpper();
                _eraCardText.text = $"ERA {epoch + 1}: {eraName}";
                _eraCardColor = GetEraAccentColor(epoch);
                _eraCardText.color = new Color(_eraCardColor.r, _eraCardColor.g, _eraCardColor.b, 0f);
                _eraCardTimer = 2.5f;
            }

            // Mode subtitle (below era card — explains the current game mode)
            var modeSubGO = CreateHUDText(canvasGO.transform, "", TextAnchor.MiddleCenter);
            _modeSubtitleText = modeSubGO.GetComponent<Text>();
            _modeSubtitleText.fontSize = 20;
            _modeSubtitleText.color = new Color(0.7f, 0.7f, 0.8f, 0f);
            var modeSubRect = modeSubGO.GetComponent<RectTransform>();
            modeSubRect.anchorMin = new Vector2(0.5f, 0.5f);
            modeSubRect.anchorMax = new Vector2(0.5f, 0.5f);
            modeSubRect.pivot = new Vector2(0.5f, 0.5f);
            modeSubRect.sizeDelta = new Vector2(600, 30);
            modeSubRect.anchoredPosition = new Vector2(0, 10);

            if (gm != null)
            {
                _modeSubtitleText.text = gm.CurrentGameMode switch
                {
                    Gameplay.GameMode.TheBreach => "Randomized epochs \u2014 survive as long as you can",
                    Gameplay.GameMode.Campaign => "Sequential epochs \u2014 unlock cosmetics",
                    Gameplay.GameMode.Streak => "Endless random levels \u2014 survive the streak",
                    _ => ""
                };
                _modeSubtitleTimer = 3f;
            }

            // Damage direction indicators (4 screen edges)
            _dmgDirImages = new Image[4];
            for (int i = 0; i < 4; i++)
            {
                var edgeGO = new GameObject($"DmgDir_{i}");
                edgeGO.transform.SetParent(canvasGO.transform, false);
                _dmgDirImages[i] = edgeGO.AddComponent<Image>();
                _dmgDirImages[i].color = new Color(1f, 0f, 0f, 0f);
                _dmgDirImages[i].raycastTarget = false;
                var edgeRect = edgeGO.GetComponent<RectTransform>();
                switch (i)
                {
                    case 0: // Left
                        edgeRect.anchorMin = new Vector2(0, 0);
                        edgeRect.anchorMax = new Vector2(0, 1);
                        edgeRect.pivot = new Vector2(0, 0.5f);
                        edgeRect.sizeDelta = new Vector2(60, 0);
                        edgeRect.anchoredPosition = Vector2.zero;
                        break;
                    case 1: // Right
                        edgeRect.anchorMin = new Vector2(1, 0);
                        edgeRect.anchorMax = new Vector2(1, 1);
                        edgeRect.pivot = new Vector2(1, 0.5f);
                        edgeRect.sizeDelta = new Vector2(60, 0);
                        edgeRect.anchoredPosition = Vector2.zero;
                        break;
                    case 2: // Top
                        edgeRect.anchorMin = new Vector2(0, 1);
                        edgeRect.anchorMax = new Vector2(1, 1);
                        edgeRect.pivot = new Vector2(0.5f, 1);
                        edgeRect.sizeDelta = new Vector2(0, 40);
                        edgeRect.anchoredPosition = Vector2.zero;
                        break;
                    case 3: // Bottom
                        edgeRect.anchorMin = new Vector2(0, 0);
                        edgeRect.anchorMax = new Vector2(1, 0);
                        edgeRect.pivot = new Vector2(0.5f, 0);
                        edgeRect.sizeDelta = new Vector2(0, 40);
                        edgeRect.anchoredPosition = Vector2.zero;
                        break;
                }
            }

            // Sprint 8: Friend challenge target (below running score, hidden unless in challenge mode)
            if (gm != null && gm.FriendChallengeScore > 0)
            {
                // CHALLENGE MODE badge
                var badgeGO = new GameObject("ChallengeBadge");
                badgeGO.transform.SetParent(canvasGO.transform, false);
                var badgeImg = badgeGO.AddComponent<Image>();
                badgeImg.sprite = Gameplay.PlaceholderAssets.GetPixelTextSprite(
                    "CHALLENGE MODE", new Color(1f, 0.85f, 0.2f), 3);
                badgeImg.preserveAspect = true;
                var badgeRect = badgeGO.GetComponent<RectTransform>();
                badgeRect.anchorMin = new Vector2(0.5f, 1);
                badgeRect.anchorMax = new Vector2(0.5f, 1);
                badgeRect.pivot = new Vector2(0.5f, 1);
                badgeRect.sizeDelta = new Vector2(280, 24);
                badgeRect.anchoredPosition = new Vector2(0, -48);

                var targetGO = CreateHUDText(canvasGO.transform, "", TextAnchor.UpperCenter);
                _friendTargetText = targetGO.GetComponent<Text>();
                _friendTargetText.fontSize = 22;
                _friendTargetText.color = new Color(0.5f, 0.8f, 1f);
                var targetRect = targetGO.GetComponent<RectTransform>();
                targetRect.anchorMin = new Vector2(0.5f, 1);
                targetRect.anchorMax = new Vector2(0.5f, 1);
                targetRect.pivot = new Vector2(0.5f, 1);
                targetRect.sizeDelta = new Vector2(300, 26);
                targetRect.anchoredPosition = new Vector2(0, -66);
                _friendTargetText.text = $"Target: {gm.FriendChallengeScore:N0}";
            }

            // Weapon upgrade notification (center-bottom, hidden by default)
            var upgradeGO = CreateHUDText(canvasGO.transform, "", TextAnchor.MiddleCenter);
            _upgradeNotifyText = upgradeGO.GetComponent<Text>();
            _upgradeNotifyText.fontSize = 28;
            _upgradeNotifyText.color = new Color(1f, 0.85f, 0.2f, 0f);
            var upgradeRect = upgradeGO.GetComponent<RectTransform>();
            upgradeRect.anchorMin = new Vector2(0.5f, 0);
            upgradeRect.anchorMax = new Vector2(0.5f, 0);
            upgradeRect.pivot = new Vector2(0.5f, 0);
            upgradeRect.sizeDelta = new Vector2(400, 40);
            upgradeRect.anchoredPosition = new Vector2(0, 120);

            // Boss health bar (hidden by default, shown when boss is active)
            CreateBossBar(canvasGO.transform);

            // Mode info (top-center: epoch or streak)
            if (gm != null && gm.CurrentGameMode != Gameplay.GameMode.TheBreach)
            {
                var modeGO = CreateHUDText(canvasGO.transform, "", TextAnchor.UpperCenter);
                _modeInfoText = modeGO.GetComponent<Text>();
                _modeInfoText.fontSize = 22;
                _modeInfoText.color = new Color(0.9f, 0.85f, 0.7f);
                var modeRect = modeGO.GetComponent<RectTransform>();
                modeRect.anchorMin = new Vector2(0.5f, 1);
                modeRect.anchorMax = new Vector2(0.5f, 1);
                modeRect.pivot = new Vector2(0.5f, 1);
                modeRect.anchoredPosition = new Vector2(0, -20);
            }
        }

        private void UpdateHearts(int current, int max)
        {
            if (_hearts == null) return;
            for (int i = 0; i < MAX_HEARTS; i++)
            {
                if (_hearts[i] != null)
                {
                    _hearts[i].color = i < current ? Color.red : new Color(0.3f, 0.1f, 0.1f);
                }
            }
        }

        private void UpdateTimer()
        {
            if (_timerText == null || Gameplay.GameManager.Instance == null) return;
            float elapsed = Gameplay.GameManager.Instance.LevelElapsedTime;
            int minutes = (int)(elapsed / 60f);
            float seconds = elapsed % 60f;
            _timerText.text = $"{minutes}:{seconds:00.0}";
        }

        private void UpdateWeaponDisplay()
        {
            if (_weaponText == null) return;
            if (_cachedWeaponSystem == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                    _cachedWeaponSystem = player.GetComponent<Gameplay.WeaponSystem>();
            }
            var ws = _cachedWeaponSystem;
            if (ws == null) return;

            // Top-right weapon text
            if (ws.IsUnarmed)
            {
                _weaponText.text = "Unarmed";
                _weaponText.color = Color.gray;
            }
            else
            {
                string typeName = ws.ActiveWeaponType.ToString();
                Color typeColor = GetWeaponDisplayColor(ws.ActiveWeaponType);
                string tierSuffix = ws.ActiveWeaponTier != Generative.WeaponTier.Starting
                    ? $" [{ws.ActiveWeaponTier}]" : "";
                _weaponText.text = $"{typeName}{tierSuffix}";
                _weaponText.color = typeColor;
            }

            // Rebuild wheel when acquired count or tier state changes (weapon pickup/upgrade)
            int tierHash = 0;
            for (int i = 0; i < Gameplay.WeaponSystem.SLOT_COUNT; i++)
                if (ws.Slots[i].Acquired) tierHash += (int)ws.Slots[i].Tier * (i + 1);
            if (ws.AcquiredWeaponCount != _lastAcquiredCount || tierHash != _lastTierHash)
            {
                RebuildWheelSlots(ws);
                UpdateWeaponWheelSlots(ws);
                _lastTierHash = tierHash;
            }

            // Highlight on weapon/unarmed change
            int currentState = ws.IsUnarmed ? -1 : (int)ws.ActiveWeaponType;
            if (currentState != _lastWheelState)
            {
                if (_lastWheelState != -99)
                    _weaponWheelTimer = WHEEL_HIGHLIGHT_DURATION;
                UpdateWeaponWheelSlots(ws);
            }
            _lastWheelState = currentState;

            // Always-visible fade: full alpha on change, settle back to resting
            if (_weaponWheelGroup != null)
            {
                if (_weaponWheelTimer > 0f)
                {
                    _weaponWheelTimer -= Time.deltaTime;
                    float t = Mathf.Clamp01(_weaponWheelTimer / WHEEL_HIGHLIGHT_DURATION);
                    _weaponWheelGroup.alpha = Mathf.Lerp(WHEEL_RESTING_ALPHA, 1f, t);
                }
                else
                {
                    _weaponWheelGroup.alpha = WHEEL_RESTING_ALPHA;
                }
            }
        }

        private void UpdateWeaponWheelSlots(Gameplay.WeaponSystem ws)
        {
            if (_wheelSlotImages == null) return;

            int activeWeaponIdx = ws.IsUnarmed ? -1 : (int)ws.ActiveWeaponType;

            for (int i = 0; i < _wheelVisibleCount; i++)
            {
                if (_wheelSlotImages[i] == null) continue;
                int weaponIdx = _wheelSlotWeaponIdx[i];

                if (weaponIdx == activeWeaponIdx)
                {
                    // Active slot: bright + scaled up
                    Color c = weaponIdx == -1
                        ? new Color(0.7f, 0.7f, 0.7f, 1f)
                        : GetWeaponDisplayColor((Generative.WeaponType)weaponIdx);
                    _wheelSlotImages[i].color = c;
                    _wheelSlotImages[i].rectTransform.localScale = Vector3.one * 1.3f;
                }
                else
                {
                    // Inactive acquired slot: weapon-tinted, slightly dim
                    Color c = weaponIdx == -1
                        ? new Color(0.5f, 0.5f, 0.5f, 0.5f)
                        : GetWeaponDisplayColor((Generative.WeaponType)weaponIdx);
                    c.a = 0.6f;
                    _wheelSlotImages[i].color = c;
                    _wheelSlotImages[i].rectTransform.localScale = Vector3.one;
                }
            }

            // Update center label
            if (_weaponWheelLabel != null)
            {
                if (ws.IsUnarmed)
                {
                    _weaponWheelLabel.text = "Unarmed";
                    _weaponWheelLabel.color = Color.gray;
                }
                else
                {
                    _weaponWheelLabel.text = ws.ActiveWeaponType.ToString();
                    _weaponWheelLabel.color = GetWeaponDisplayColor(ws.ActiveWeaponType);
                }
            }
        }

        private void RebuildWheelSlots(Gameplay.WeaponSystem ws)
        {
            // Destroy old slots
            if (_wheelSlotGOs != null)
            {
                for (int i = 0; i < _wheelSlotGOs.Length; i++)
                {
                    if (_wheelSlotGOs[i] != null)
                        Destroy(_wheelSlotGOs[i]);
                }
            }

            // Build list: unarmed + acquired weapons in order
            var slots = ws.Slots;
            int count = 1; // unarmed always present
            for (int i = 0; i < Gameplay.WeaponSystem.SLOT_COUNT; i++)
            {
                if (i == (int)Generative.WeaponType.Slower) continue;
                if (slots[i].Acquired) count++;
            }

            _wheelVisibleCount = count;
            _wheelSlotGOs = new GameObject[count];
            _wheelSlotImages = new Image[count];
            _wheelSlotWeaponIdx = new int[count];

            // Slot 0 = unarmed, rest = acquired weapons in type order
            _wheelSlotWeaponIdx[0] = -1;
            int idx = 1;
            for (int i = 0; i < Gameplay.WeaponSystem.SLOT_COUNT; i++)
            {
                if (i == (int)Generative.WeaponType.Slower) continue;
                if (slots[i].Acquired)
                {
                    _wheelSlotWeaponIdx[idx] = i;
                    idx++;
                }
            }

            // Size scales with slot count
            float radius;
            float bgSize;
            if (count <= 1)
            {
                radius = 0f;
                bgSize = 80f;
            }
            else
            {
                radius = 30f + count * 12f;
                bgSize = radius * 2f + WHEEL_SLOT_SIZE;
            }

            // Resize ring and bg
            if (_wheelRingRect != null)
                _wheelRingRect.sizeDelta = new Vector2(bgSize + 12f, bgSize + 12f);
            if (_wheelBgRect != null)
                _wheelBgRect.sizeDelta = new Vector2(bgSize, bgSize);

            var rootRect = _weaponWheelRoot.GetComponent<RectTransform>();
            if (rootRect != null)
                rootRect.sizeDelta = new Vector2(bgSize + 12f, bgSize + 12f);

            // Create slot GameObjects
            for (int i = 0; i < count; i++)
            {
                int weaponIdx = _wheelSlotWeaponIdx[i];
                string slotName;
                Sprite slotSprite;

                if (weaponIdx == -1)
                {
                    slotName = "Slot_Unarmed";
                    slotSprite = Gameplay.PlaceholderAssets.GetParticleSprite();
                }
                else
                {
                    var weaponType = (Generative.WeaponType)weaponIdx;
                    slotName = $"Slot_{weaponType}";
                    slotSprite = Gameplay.PlaceholderAssets.GetWeaponPickupSprite(
                        weaponType, slots[weaponIdx].Tier);
                }

                var slotGO = new GameObject(slotName);
                slotGO.transform.SetParent(_weaponWheelRoot.transform, false);
                _wheelSlotGOs[i] = slotGO;
                _wheelSlotImages[i] = slotGO.AddComponent<Image>();
                _wheelSlotImages[i].sprite = slotSprite;
                _wheelSlotImages[i].preserveAspect = true;
                _wheelSlotImages[i].raycastTarget = false;

                var slotRect = slotGO.GetComponent<RectTransform>();
                slotRect.sizeDelta = new Vector2(WHEEL_SLOT_SIZE, WHEEL_SLOT_SIZE);

                if (count <= 1)
                {
                    slotRect.anchoredPosition = Vector2.zero;
                }
                else
                {
                    float angle = 90f - (360f * i / count);
                    float rad = angle * Mathf.Deg2Rad;
                    float x = Mathf.Cos(rad) * radius;
                    float y = Mathf.Sin(rad) * radius;
                    slotRect.anchoredPosition = new Vector2(x, y);
                }

                _wheelSlotImages[i].color = new Color(0.5f, 0.5f, 0.55f, 0.6f);

                // Tier pips: small dots below slot indicating weapon tier (0 for Starting, 1 for Medium, 2 for Heavy)
                if (weaponIdx >= 0)
                {
                    int tier = (int)slots[weaponIdx].Tier;
                    for (int p = 0; p < tier; p++)
                    {
                        var pipGO = new GameObject($"Pip_{i}_{p}");
                        pipGO.transform.SetParent(slotGO.transform, false);
                        var pipImg = pipGO.AddComponent<Image>();
                        pipImg.color = new Color(1f, 0.85f, 0.2f, 0.8f);
                        pipImg.raycastTarget = false;
                        var pipRect = pipGO.GetComponent<RectTransform>();
                        pipRect.sizeDelta = new Vector2(8, 8);
                        float pipX = tier == 1 ? 0f : (p - (tier - 1) * 0.5f) * 12f;
                        pipRect.anchoredPosition = new Vector2(pipX, -WHEEL_SLOT_SIZE * 0.5f - 6f);
                    }
                }
            }

            _lastAcquiredCount = ws.AcquiredWeaponCount;
        }

        private static Color GetWeaponDisplayColor(Generative.WeaponType type)
        {
            return type switch
            {
                Generative.WeaponType.Bolt => Color.white,
                Generative.WeaponType.Piercer => new Color(0.6f, 0.9f, 1f),
                Generative.WeaponType.Spreader => new Color(1f, 0.7f, 0.3f),
                Generative.WeaponType.Chainer => new Color(0.5f, 0.8f, 1f),
                Generative.WeaponType.Cannon => new Color(1f, 0.3f, 0.2f),
                _ => Color.white
            };
        }

        private void CreateWeaponWheel(Transform parent)
        {
            _weaponWheelRoot = new GameObject("WeaponWheel");
            _weaponWheelRoot.transform.SetParent(parent, false);
            _weaponWheelGroup = _weaponWheelRoot.AddComponent<CanvasGroup>();
            _weaponWheelGroup.alpha = WHEEL_RESTING_ALPHA;
            _weaponWheelGroup.blocksRaycasts = false;
            _weaponWheelGroup.interactable = false;

            var rootRect = _weaponWheelRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0, 0);
            rootRect.anchorMax = new Vector2(0, 0);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = new Vector2(130, 220);
            rootRect.sizeDelta = new Vector2(80, 80); // resized by RebuildWheelSlots

            // Outline ring
            var ringGO = new GameObject("WheelRing");
            ringGO.transform.SetParent(_weaponWheelRoot.transform, false);
            var ringImg = ringGO.AddComponent<Image>();
            ringImg.sprite = Gameplay.PlaceholderAssets.GetParticleSprite();
            ringImg.color = new Color(0.3f, 0.25f, 0.5f, 0.8f);
            ringImg.raycastTarget = false;
            _wheelRingRect = ringGO.GetComponent<RectTransform>();
            _wheelRingRect.anchoredPosition = Vector2.zero;

            // Solid dark background
            var bgGO = new GameObject("WheelBg");
            bgGO.transform.SetParent(_weaponWheelRoot.transform, false);
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.sprite = Gameplay.PlaceholderAssets.GetParticleSprite();
            bgImg.color = new Color(0.08f, 0.06f, 0.18f, 0.92f);
            bgImg.raycastTarget = false;
            _wheelBgRect = bgGO.GetComponent<RectTransform>();
            _wheelBgRect.anchoredPosition = Vector2.zero;

            // Center label
            var labelGO = CreateHUDText(_weaponWheelRoot.transform, "", TextAnchor.MiddleCenter);
            _weaponWheelLabel = labelGO.GetComponent<Text>();
            _weaponWheelLabel.fontSize = 18;
            _weaponWheelLabel.raycastTarget = false;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = new Vector2(120, 30);

            // Slots are created dynamically by RebuildWheelSlots
        }

        private void CreateHeatBar(Transform parent)
        {
            // Background
            var bgGO = new GameObject("HeatBarBg");
            bgGO.transform.SetParent(parent, false);
            _heatBarBg = bgGO.AddComponent<Image>();
            _heatBarBg.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(1, 1);
            bgRect.anchorMax = new Vector2(1, 1);
            bgRect.pivot = new Vector2(1, 1);
            bgRect.sizeDelta = new Vector2(120, 10);
            bgRect.anchoredPosition = new Vector2(-20, -66);

            // Fill
            var fillGO = new GameObject("HeatBarFill");
            fillGO.transform.SetParent(bgGO.transform, false);
            _heatBarFill = fillGO.AddComponent<Image>();
            _heatBarFill.color = new Color(1f, 0.5f, 0.1f);
            var fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.sizeDelta = new Vector2(0, 0);
            fillRect.offsetMin = new Vector2(1, 1);
            fillRect.offsetMax = new Vector2(1, -1);

            // Hidden by default
            bgGO.SetActive(false);
        }

        private void UpdateHeatBar()
        {
            if (_heatBarBg == null) return;
            var ws = _cachedWeaponSystem;
            if (ws == null) return;

            bool showHeat = ws.ActiveWeaponType == Generative.WeaponType.Cannon && ws.HasWeapon(Generative.WeaponType.Cannon);
            _heatBarBg.gameObject.SetActive(showHeat);

            if (showHeat && _heatBarFill != null)
            {
                float ratio = ws.Heat.HeatRatio;
                var fillRect = _heatBarFill.GetComponent<RectTransform>();
                float maxWidth = 118f; // 120 - 2 padding
                fillRect.sizeDelta = new Vector2(maxWidth * ratio, 0);
                fillRect.offsetMax = new Vector2(1 + maxWidth * ratio, -1);

                // Color changes: normal orange, overheated red
                _heatBarFill.color = ws.Heat.IsOverheated
                    ? new Color(1f, 0.15f, 0.1f)
                    : Color.Lerp(new Color(1f, 0.6f, 0.1f), new Color(1f, 0.2f, 0.1f), ratio);
            }
        }

        private void UpdateRelicCounter()
        {
            if (_relicText == null) return;
            var gm = Gameplay.GameManager.Instance;
            if (gm == null) return;

            int preserved = Mathf.Max(0, gm.TotalRelics - gm.RelicsDestroyed);
            _relicText.text = $"Relics Preserved: {preserved}/{gm.TotalRelics}";

            // Gold when all preserved, red-tinted when some lost
            _relicText.color = gm.RelicsDestroyed > 0
                ? new Color(1f, 0.5f, 0.3f)
                : new Color(1f, 0.85f, 0.2f);
        }

        private void UpdateKillStreak()
        {
            if (_killStreakText == null) return;
            var gm = Gameplay.GameManager.Instance;
            if (gm == null) return;

            int streak = gm.CurrentNoDamageStreak;

            // Show when streak reaches 3+
            if (streak >= 3 && streak != _lastKnownStreak)
            {
                _lastKnownStreak = streak;
                _killStreakText.text = $"x{streak}";
                _killStreakFadeTimer = 2f;
            }

            // Reset on streak break
            if (streak < 3)
                _lastKnownStreak = 0;

            // Fade out
            if (_killStreakFadeTimer > 0f)
            {
                _killStreakFadeTimer -= Time.deltaTime;
                float alpha = Mathf.Clamp01(_killStreakFadeTimer / 0.5f); // fade over last 0.5s
                _killStreakText.color = new Color(1f, 0.85f, 0.2f, alpha);
            }
            else
            {
                _killStreakText.color = new Color(1f, 0.85f, 0.2f, 0f);
            }
        }

        private void UpdateModeInfo()
        {
            var gm = Gameplay.GameManager.Instance;
            if (gm == null) return;

            if (_livesText != null)
            {
                // Campaign has infinite lives — show deaths/level instead of global lives
                if (gm.CurrentGameMode == Gameplay.GameMode.Campaign)
                    _livesText.text = $"Deaths: {gm.DeathsThisLevel}";
                else
                    _livesText.text = $"Lives: {gm.GlobalLives}";
            }

            if (_modeInfoText != null)
            {
                switch (gm.CurrentGameMode)
                {
                    case Gameplay.GameMode.Campaign:
                        _modeInfoText.text = $"Campaign - Epoch {gm.CampaignEpoch + 1}/10";
                        break;
                    case Gameplay.GameMode.Streak:
                        _modeInfoText.text = $"Streak: {gm.StreakCount}";
                        break;
                }
            }
        }

        private void CreateAbilityIcons(Transform parent)
        {
            // Double Jump icon (below hearts, left)
            var djGO = new GameObject("DoubleJumpIcon");
            djGO.transform.SetParent(parent, false);
            _doubleJumpIcon = djGO.AddComponent<Image>();
            _doubleJumpIcon.color = new Color(0.3f, 0.3f, 0.3f, 0.3f); // Dimmed until acquired
            var djRect = djGO.GetComponent<RectTransform>();
            djRect.anchorMin = new Vector2(0, 1);
            djRect.anchorMax = new Vector2(0, 1);
            djRect.pivot = new Vector2(0, 1);
            djRect.sizeDelta = new Vector2(28, 28);
            djRect.anchoredPosition = new Vector2(20, -100);

            // Air Dash icon (next to double jump)
            var adGO = new GameObject("AirDashIcon");
            adGO.transform.SetParent(parent, false);
            _airDashIcon = adGO.AddComponent<Image>();
            _airDashIcon.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
            var adRect = adGO.GetComponent<RectTransform>();
            adRect.anchorMin = new Vector2(0, 1);
            adRect.anchorMax = new Vector2(0, 1);
            adRect.pivot = new Vector2(0, 1);
            adRect.sizeDelta = new Vector2(28, 28);
            adRect.anchoredPosition = new Vector2(54, -100);
        }

        private void UpdateRunningScore()
        {
            if (_scoreText == null) return;
            var gm = Gameplay.GameManager.Instance;
            if (gm == null) return;

            int runningScore = gm.ItemBonusScore + gm.EnemyBonusScore;
            if (runningScore != _displayedScore)
            {
                _displayedScore = runningScore;
                _scoreText.text = _displayedScore.ToString();
                _scoreScaleTimer = 0.2f;
            }

            // Scale-up animation on change
            if (_scoreScaleTimer > 0f)
            {
                _scoreScaleTimer -= Time.deltaTime;
                float scale = 1f + Mathf.Clamp01(_scoreScaleTimer / 0.2f) * 0.3f;
                _scoreText.transform.localScale = new Vector3(scale, scale, 1f);
                _scoreText.color = Color.Lerp(Color.white, new Color(1f, 0.85f, 0.2f), _scoreScaleTimer / 0.2f);
            }
            else
            {
                _scoreText.transform.localScale = Vector3.one;
                _scoreText.color = Color.white;
            }
        }

        private void UpdateAbilityIcons()
        {
            if (_cachedAbilitySystem == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                    _cachedAbilitySystem = player.GetComponent<Gameplay.AbilitySystem>();
            }
            var abs = _cachedAbilitySystem;
            if (abs == null) return;

            // Double Jump: bright when acquired + available, dim when used, grey when not acquired
            if (_doubleJumpIcon != null)
            {
                if (!abs.HasDoubleJump)
                    _doubleJumpIcon.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
                else if (abs.DoubleJumpAvailable)
                    _doubleJumpIcon.color = new Color(0.8f, 0.9f, 1f, 0.9f);
                else
                    _doubleJumpIcon.color = new Color(0.4f, 0.5f, 0.6f, 0.5f);
            }

            // Air Dash: bright when available, dim with cooldown fill, grey when not acquired
            if (_airDashIcon != null)
            {
                if (!abs.HasAirDash)
                    _airDashIcon.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
                else if (abs.AirDashAvailable)
                    _airDashIcon.color = new Color(0.5f, 0.8f, 1f, 0.9f);
                else
                {
                    float cooldown = abs.DashCooldownRatio;
                    _airDashIcon.color = Color.Lerp(
                        new Color(0.5f, 0.8f, 1f, 0.9f),
                        new Color(0.3f, 0.4f, 0.5f, 0.5f),
                        cooldown);
                }
            }
        }

        private void UpdateQuickDraw()
        {
            var ws = _cachedWeaponSystem;
            if (ws == null) return;

            // Weapon name glow when Quick Draw active
            if (ws.IsQuickDrawActive && _weaponText != null)
            {
                float pulse = Mathf.PingPong(Time.time * 4f, 1f);
                _weaponText.color = Color.Lerp(
                    GetWeaponDisplayColor(ws.ActiveWeaponType),
                    new Color(1f, 0.85f, 0.2f),
                    pulse * 0.5f);
            }

            // "QUICK DRAW!" flash text
            if (_quickDrawText != null)
            {
                if (ws.IsQuickDrawActive && _quickDrawFlashTimer <= 0f)
                {
                    _quickDrawFlashTimer = 1f;
                }
                else if (!ws.IsQuickDrawActive)
                {
                    _quickDrawFlashTimer = 0f;
                }

                if (_quickDrawFlashTimer > 0f)
                {
                    _quickDrawFlashTimer -= Time.deltaTime;
                    float alpha = Mathf.Clamp01(_quickDrawFlashTimer / 0.5f);
                    _quickDrawText.color = new Color(1f, 0.85f, 0.2f, alpha);
                }
                else
                {
                    _quickDrawText.color = new Color(1f, 0.85f, 0.2f, 0f);
                }
            }
        }

        private void UpdateAutoSelectReason()
        {
            if (_autoSelectText == null) return;
            var ws = _cachedWeaponSystem;
            if (ws == null) return;

            if (ws.AutoSelectReasonTimer > 0f && !string.IsNullOrEmpty(ws.LastAutoSelectReason))
            {
                _autoSelectText.text = $"\u2192{ws.LastAutoSelectReason}";
                float alpha = Mathf.Clamp01(ws.AutoSelectReasonTimer / 0.5f);
                _autoSelectText.color = new Color(0.7f, 0.8f, 0.5f, alpha);
            }
            else
            {
                _autoSelectText.color = new Color(0.7f, 0.8f, 0.5f, 0f);
            }
        }

        private void UpdateDpsCapFeedback()
        {
            if (_dpsCapText == null) return;

            var loader = FindAnyObjectByType<Gameplay.LevelLoader>();
            var boss = loader?.CurrentBoss;

            if (boss != null && boss.IsActive && !boss.IsDead)
            {
                float timeSinceCap = Time.time - boss.LastDpsCapTime;
                if (timeSinceCap < 1f && _dpsCapTimer <= 0f)
                    _dpsCapTimer = 0.8f;
            }

            if (_dpsCapTimer > 0f)
            {
                _dpsCapTimer -= Time.deltaTime;
                float alpha = Mathf.Clamp01(_dpsCapTimer / 0.3f);
                _dpsCapText.color = new Color(0.6f, 0.6f, 0.6f, alpha);
            }
            else
            {
                _dpsCapText.color = new Color(0.6f, 0.6f, 0.6f, 0f);
            }
        }

        private void CreateBossBar(Transform parent)
        {
            _bossBarRoot = new GameObject("BossBar");
            _bossBarRoot.transform.SetParent(parent, false);
            var rootRect = _bossBarRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 1);
            rootRect.anchorMax = new Vector2(0.5f, 1);
            rootRect.pivot = new Vector2(0.5f, 1);
            rootRect.sizeDelta = new Vector2(400, 40);
            rootRect.anchoredPosition = new Vector2(0, -50);

            // Boss name text
            var nameGO = new GameObject("BossName");
            nameGO.transform.SetParent(_bossBarRoot.transform, false);
            _bossNameText = nameGO.AddComponent<Text>();
            _bossNameText.fontSize = 18;
            _bossNameText.color = new Color(1f, 0.85f, 0.7f);
            _bossNameText.alignment = TextAnchor.MiddleCenter;
            _bossNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _bossNameText.horizontalOverflow = HorizontalWrapMode.Overflow;
            var nameRect = nameGO.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 1);
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.pivot = new Vector2(0.5f, 1);
            nameRect.sizeDelta = new Vector2(0, 24);
            nameRect.anchoredPosition = new Vector2(0, 0);

            // Bar background
            var bgGO = new GameObject("BossBarBg");
            bgGO.transform.SetParent(_bossBarRoot.transform, false);
            _bossBarBg = bgGO.AddComponent<Image>();
            _bossBarBg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0);
            bgRect.anchorMax = new Vector2(1, 0);
            bgRect.pivot = new Vector2(0.5f, 0);
            bgRect.sizeDelta = new Vector2(0, 14);
            bgRect.anchoredPosition = new Vector2(0, 0);

            // Bar fill
            var fillGO = new GameObject("BossBarFill");
            fillGO.transform.SetParent(bgGO.transform, false);
            _bossBarFill = fillGO.AddComponent<Image>();
            _bossBarFill.color = new Color(0.2f, 0.9f, 0.3f);
            var fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.offsetMin = new Vector2(2, 2);
            fillRect.offsetMax = new Vector2(-2, -2);

            _bossBarRoot.SetActive(false);
        }

        private void UpdateBossBar()
        {
            if (_bossBarRoot == null) return;

            // Find active boss
            var loader = FindAnyObjectByType<Gameplay.LevelLoader>();
            var boss = loader?.CurrentBoss;

            if (boss == null || !boss.IsActive || boss.IsDead)
            {
                if (_bossBarRoot.activeSelf)
                {
                    _bossBarRoot.SetActive(false);
                    _lastBossHealth = -1;
                }
                return;
            }

            if (!_bossBarRoot.activeSelf)
            {
                _bossBarRoot.SetActive(true);
                _lastBossHealth = boss.Health;

                // Set boss name (convert PascalCase enum to spaced uppercase)
                string rawName = boss.Type.ToString();
                string displayName = System.Text.RegularExpressions.Regex.Replace(rawName, "([a-z])([A-Z])", "$1 $2").ToUpper();
                if (displayName.Length > 20) displayName = displayName.Substring(0, 20);
                _bossNameText.text = displayName;
            }

            // Detect health drop → trigger flash
            if (boss.Health < _lastBossHealth)
                _bossBarFlashTimer = 0.15f;
            _lastBossHealth = boss.Health;

            // Update fill amount
            float healthRatio = (float)boss.Health / boss.MaxHealth;
            if (_bossBarFill != null)
            {
                var fillRect = _bossBarFill.GetComponent<RectTransform>();
                fillRect.anchorMax = new Vector2(healthRatio, 1);
                fillRect.offsetMax = new Vector2(-2, -2);
            }

            // Phase-based color: green (phase 1) → yellow (phase 2) → red (phase 3)
            Color barColor = boss.CurrentPhase switch
            {
                1 => new Color(0.2f, 0.9f, 0.3f),
                2 => new Color(0.95f, 0.85f, 0.15f),
                3 => new Color(0.95f, 0.2f, 0.15f),
                _ => Color.white
            };

            // White flash on hit (brief)
            if (_bossBarFlashTimer > 0f)
            {
                _bossBarFlashTimer -= Time.deltaTime;
                float flash = Mathf.Clamp01(_bossBarFlashTimer / 0.15f);
                barColor = Color.Lerp(barColor, Color.white, flash);
            }

            if (_bossBarFill != null)
                _bossBarFill.color = barColor;
        }

        private GameObject CreateHUDText(Transform parent, string text, TextAnchor alignment)
        {
            var go = new GameObject("HUDText");
            go.transform.SetParent(parent, false);

            var textComp = go.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = 22;
            textComp.color = Color.white;
            textComp.alignment = alignment;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.horizontalOverflow = HorizontalWrapMode.Overflow;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 30);

            return go;
        }

        private void HandleScorePopup(Vector3 worldPos, int score)
        {
            // Find an available popup slot
            for (int i = 0; i < POPUP_POOL_SIZE; i++)
            {
                if (_popupTimers[i] <= 0f)
                {
                    _popupTexts[i].text = $"+{score}";
                    _popupTexts[i].color = new Color(1f, 0.85f, 0.2f, 1f);
                    _popupTimers[i] = 1f;
                    _popupWorldPositions[i] = worldPos + new Vector3(0, 0.5f, 0);
                    return;
                }
            }

            // No free slot — reuse the oldest (lowest timer) to avoid dropping popups
            int oldestIdx = 0;
            float oldestTime = float.MaxValue;
            for (int i = 0; i < POPUP_POOL_SIZE; i++)
            {
                if (_popupTimers[i] < oldestTime)
                {
                    oldestTime = _popupTimers[i];
                    oldestIdx = i;
                }
            }
            _popupTexts[oldestIdx].text = $"+{score}";
            _popupTexts[oldestIdx].color = new Color(1f, 0.85f, 0.2f, 1f);
            _popupTimers[oldestIdx] = 1f;
            _popupWorldPositions[oldestIdx] = worldPos + new Vector3(0, 0.5f, 0);
        }

        private void UpdateScorePopups()
        {
            if (_popupTexts == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            for (int i = 0; i < POPUP_POOL_SIZE; i++)
            {
                if (_popupTimers[i] > 0f)
                {
                    _popupTimers[i] -= Time.deltaTime;
                    _popupWorldPositions[i] += new Vector3(0, 1.5f * Time.deltaTime, 0); // Drift up

                    // Convert world → screen → canvas position
                    Vector3 screenPos = cam.WorldToScreenPoint(_popupWorldPositions[i]);
                    if (screenPos.z < 0) { _popupTimers[i] = 0f; continue; }

                    var rect = _popupTexts[i].GetComponent<RectTransform>();
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _canvas.GetComponent<RectTransform>(), screenPos, null, out Vector2 localPos);
                    rect.anchoredPosition = localPos;
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);

                    float alpha = Mathf.Clamp01(_popupTimers[i] / 0.4f);
                    float scale = 1f + (1f - Mathf.Clamp01(_popupTimers[i])) * 0.3f;
                    _popupTexts[i].color = new Color(1f, 0.85f, 0.2f, alpha);
                    _popupTexts[i].transform.localScale = new Vector3(scale, scale, 1f);
                }
                else
                {
                    _popupTexts[i].color = new Color(1f, 0.85f, 0.2f, 0f);
                }
            }
        }

        private void UpdateComboCounter()
        {
            if (_comboText == null) return;
            var gm = Gameplay.GameManager.Instance;
            if (gm == null) return;

            int combo = gm.ComboCount;
            if (combo >= 2)
            {
                _comboText.text = $"x{combo} COMBO";
                int fontSize = Mathf.Min(48, 30 + combo * 3);
                _comboText.fontSize = fontSize;
                _comboText.color = new Color(1f, 0.85f, 0.2f, 0.9f);
            }
            else
            {
                _comboText.color = new Color(1f, 0.85f, 0.2f, 0f);
            }
        }

        private void HandleAchievementUnlock(Gameplay.AchievementType type)
        {
            string name = Gameplay.AchievementManager.GetAchievementName(type);
            _achievementQueue.Enqueue(name);
        }

        private void UpdateAchievementToast()
        {
            if (_achievementText == null) return;

            if (_achievementTimer > 0f)
            {
                _achievementTimer -= Time.deltaTime;

                // Slide in from top (y: 40 → -10)
                float slideIn = Mathf.Clamp01(1f - (_achievementTimer - 2.5f) / 0.5f);
                float slideOut = Mathf.Clamp01(_achievementTimer / 0.5f);
                float y = Mathf.Lerp(40f, -10f, slideIn);
                float alpha = slideOut;

                var rect = _achievementText.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(0, y);
                _achievementText.color = new Color(1f, 0.85f, 0.2f, alpha);
            }
            else if (_achievementQueue.Count > 0)
            {
                string name = _achievementQueue.Dequeue();
                _achievementText.text = $"ACHIEVEMENT: {name}";
                _achievementTimer = 3f;
            }
            else
            {
                _achievementText.color = new Color(1f, 0.85f, 0.2f, 0f);
            }
        }

        private void UpdateEraCard()
        {
            if (_eraCardText == null || _eraCardTimer <= 0f) return;
            _eraCardTimer -= Time.deltaTime;
            float fadeIn = Mathf.Clamp01((2.5f - _eraCardTimer) / 0.3f);
            float fadeOut = Mathf.Clamp01(_eraCardTimer / 0.5f);
            float alpha = Mathf.Min(fadeIn, fadeOut);
            _eraCardText.color = new Color(_eraCardColor.r, _eraCardColor.g, _eraCardColor.b, alpha);
        }

        private void UpdateModeSubtitle()
        {
            if (_modeSubtitleText == null || _modeSubtitleTimer <= 0f) return;
            _modeSubtitleTimer -= Time.deltaTime;
            float fadeIn = Mathf.Clamp01((3f - _modeSubtitleTimer) / 0.5f);
            float fadeOut = Mathf.Clamp01(_modeSubtitleTimer / 0.5f);
            float alpha = Mathf.Min(fadeIn, fadeOut) * 0.8f;
            _modeSubtitleText.color = new Color(0.7f, 0.7f, 0.8f, alpha);
        }

        private static Color GetEraAccentColor(int epoch)
        {
            return epoch switch
            {
                0 => new Color(0.7f, 0.5f, 0.3f),   // Prehistoric - earth
                1 => new Color(0.9f, 0.8f, 0.4f),   // Ancient - sand gold
                2 => new Color(0.5f, 0.7f, 0.9f),   // Classical - marble blue
                3 => new Color(0.6f, 0.6f, 0.7f),   // Medieval - stone grey
                4 => new Color(0.8f, 0.6f, 0.3f),   // Renaissance - bronze
                5 => new Color(0.5f, 0.5f, 0.6f),   // Industrial - steel
                6 => new Color(0.3f, 0.8f, 0.3f),   // Modern - green
                7 => new Color(0.3f, 0.6f, 1f),      // Information - blue
                8 => new Color(0.6f, 0.3f, 0.9f),   // Future - purple
                9 => new Color(0.9f, 0.2f, 0.4f),   // Singularity - crimson
                _ => Color.white
            };
        }

        private void HandleDamageDirection(Vector2 damageWorldPos)
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) return;

            Vector2 dir = damageWorldPos - (Vector2)player.transform.position;
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                _dmgDirEdge = dir.x < 0 ? 0 : 1;
            else
                _dmgDirEdge = dir.y > 0 ? 2 : 3;
            _dmgDirTimer = 0.3f;
        }

        private void UpdateDamageDirection()
        {
            if (_dmgDirImages == null) return;

            if (_dmgDirTimer > 0f)
            {
                _dmgDirTimer -= Time.deltaTime;
                float alpha = Mathf.Clamp01(_dmgDirTimer / 0.15f) * 0.4f;
                for (int i = 0; i < 4; i++)
                    _dmgDirImages[i].color = i == _dmgDirEdge
                        ? new Color(1f, 0f, 0f, alpha)
                        : new Color(1f, 0f, 0f, 0f);
            }
            else
            {
                for (int i = 0; i < 4; i++)
                    _dmgDirImages[i].color = new Color(1f, 0f, 0f, 0f);
            }
        }

        private void CreateSpecialAttackBar(Transform parent)
        {
            // Background bar (below hearts row)
            var bgGO = new GameObject("SpecialBarBg");
            bgGO.transform.SetParent(parent, false);
            _specialBarBg = bgGO.AddComponent<Image>();
            _specialBarBg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            _specialBarBg.raycastTarget = false;
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 1);
            bgRect.anchorMax = new Vector2(0, 1);
            bgRect.pivot = new Vector2(0, 1);
            bgRect.sizeDelta = new Vector2(220, 8);
            bgRect.anchoredPosition = new Vector2(20, -58);

            // Fill bar
            var fillGO = new GameObject("SpecialBarFill");
            fillGO.transform.SetParent(bgGO.transform, false);
            _specialBarFill = fillGO.AddComponent<Image>();
            _specialBarFill.color = new Color(0.4f, 0.4f, 0.4f);
            _specialBarFill.raycastTarget = false;
            var fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.offsetMin = new Vector2(1, 1);
            fillRect.offsetMax = new Vector2(1, -1);

            // Start hidden — shown once player has enough weapons
            bgGO.SetActive(false);
        }

        private void UpdateSpecialAttackMeter()
        {
            if (_specialBarBg == null || _specialBarFill == null) return;

            // Lazy-find the special attack system
            if (_cachedSpecialAttack == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                    _cachedSpecialAttack = player.GetComponent<Gameplay.SpecialAttackSystem>();
            }
            var sa = _cachedSpecialAttack;
            if (sa == null)
            {
                _specialBarBg.gameObject.SetActive(false);
                return;
            }

            // Only show the bar when player has enough weapons to ever use it
            if (_cachedWeaponSystem == null)
            {
                _specialBarBg.gameObject.SetActive(false);
                return;
            }
            bool hasEnoughWeapons = _cachedWeaponSystem.AcquiredWeaponCount >= 3;
            _specialBarBg.gameObject.SetActive(hasEnoughWeapons);
            if (!hasEnoughWeapons) return;

            float fillRatio;
            Color fillColor;

            if (sa.IsAttacking)
            {
                // Flash white during attack
                _specialFlashTimer = 0.3f;
                fillRatio = 1f;
                fillColor = Color.white;
            }
            else if (sa.IsCharging)
            {
                // Pulsing yellow glow while charging
                fillRatio = sa.ChargeRatio;
                float pulse = Mathf.PingPong(Time.time * 6f, 1f);
                fillColor = Color.Lerp(new Color(0.9f, 0.8f, 0.1f), new Color(1f, 1f, 0.5f), pulse);
            }
            else if (sa.CooldownRatio > 0f)
            {
                // Gray fill showing cooldown progress (fills up as cooldown expires)
                fillRatio = 1f - sa.CooldownRatio;
                fillColor = new Color(0.4f, 0.4f, 0.4f);
            }
            else if (sa.IsReady)
            {
                // Solid yellow when ready
                fillRatio = 1f;
                fillColor = new Color(0.9f, 0.8f, 0.1f);
            }
            else
            {
                // Not enough weapons or not grounded — dim
                fillRatio = 1f;
                fillColor = new Color(0.35f, 0.35f, 0.3f);
            }

            // Activation flash decay
            if (_specialFlashTimer > 0f && !sa.IsAttacking)
            {
                _specialFlashTimer -= Time.deltaTime;
                float flash = Mathf.Clamp01(_specialFlashTimer / 0.3f);
                fillColor = Color.Lerp(fillColor, Color.white, flash);
            }

            _specialBarFill.color = fillColor;
            var rect = _specialBarFill.GetComponent<RectTransform>();
            rect.anchorMax = new Vector2(fillRatio, 1);
            rect.offsetMax = new Vector2(-1, -1);
        }

        private void UpdateFriendTarget()
        {
            if (_friendTargetText == null) return;
            var gm = Gameplay.GameManager.Instance;
            if (gm == null || gm.FriendChallengeScore <= 0) return;

            int currentRunning = gm.ItemBonusScore + gm.EnemyBonusScore;
            int target = gm.FriendChallengeScore;

            if (currentRunning >= target)
            {
                // Player has beaten the friend's score
                _friendTargetText.text = $"Target: {target:N0} - BEATEN!";
                _friendTargetText.color = new Color(0.4f, 1f, 0.4f);
            }
            else
            {
                int remaining = target - currentRunning;
                _friendTargetText.text = $"Target: {target:N0} (need {remaining:N0})";
                _friendTargetText.color = new Color(0.5f, 0.8f, 1f);
            }
        }

        private void HandleCosmeticUnlock(string cosmeticName)
        {
            _achievementQueue.Enqueue($"NEW: {cosmeticName} unlocked!");
        }

        private void HandleWeaponUpgrade(Generative.WeaponType type, Generative.WeaponTier tier)
        {
            if (_upgradeNotifyText == null) return;
            _upgradeNotifyText.text = $"{type} \u2192 {tier}";
            _upgradeNotifyTimer = 2f;
        }

        private void UpdateUpgradeNotification()
        {
            if (_upgradeNotifyText == null || _upgradeNotifyTimer <= 0f) return;
            _upgradeNotifyTimer -= Time.deltaTime;
            float alpha = Mathf.Clamp01(_upgradeNotifyTimer / 0.5f);
            float scale = 1f + Mathf.Clamp01((_upgradeNotifyTimer - 1.5f) / 0.5f) * 0.3f;
            _upgradeNotifyText.color = new Color(1f, 0.85f, 0.2f, alpha);
            _upgradeNotifyText.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private void CreatePauseButton(Transform parent)
        {
            var go = new GameObject("PauseButton");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.3f, 0.6f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => {
                Gameplay.GameManager.Instance?.PauseGame();
            });

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.sizeDelta = new Vector2(70, 70);
            rect.anchoredPosition = new Vector2(-70, -16);

            // Pause icon text
            var textGO = new GameObject("PauseLabel");
            textGO.transform.SetParent(go.transform, false);
            var textComp = textGO.AddComponent<Text>();
            textComp.text = "||";
            textComp.fontSize = 24;
            textComp.color = Color.white;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }
    }
}
