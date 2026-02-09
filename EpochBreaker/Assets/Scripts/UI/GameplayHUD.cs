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
        private Image _timerImg;
        private Sprite _timerSprite;
        private Image _eraImg;
        private Image _livesImg;
        private Sprite _livesSprite;
        private Image _modeInfoImg;
        private Sprite _modeInfoSprite;
        private Image _heatBarBg;
        private Image _heatBarFill;
        private Image _relicImg;
        private Sprite _relicSprite;
        private Gameplay.HealthSystem _healthSystem;
        private Gameplay.WeaponSystem _cachedWeaponSystem;
        private Gameplay.AbilitySystem _cachedAbilitySystem;

        // Sprint 4: Visible systems
        private Image _scoreImg;
        private Sprite _scoreSprite;
        private int _displayedScore;
        private float _scoreScaleTimer;
        private Image _doubleJumpIcon;
        private Image _airDashIcon;
        private Image _quickDrawImg;
        private float _quickDrawFlashTimer;
        private Image _autoSelectImg;
        private Sprite _autoSelectSprite;
        private Image _dpsCapImg;
        private float _dpsCapTimer;

        // Boss health bar
        private GameObject _bossBarRoot;
        private Image _bossBarBg;
        private Image _bossBarFill;
        private Image _bossNameImg;
        private Sprite _bossNameSprite;
        private float _bossBarFlashTimer;
        private int _lastBossHealth = -1;

        // Score popups (pooled)
        private const int POPUP_POOL_SIZE = 6;
        private Image[] _popupImgs;
        private Sprite[] _popupSprites;
        private float[] _popupTimers;
        private Vector3[] _popupWorldPositions;

        // Damage number popups (separate pool)
        private const int DMG_POPUP_POOL_SIZE = 8;
        private Image[] _dmgPopupImgs;
        private Sprite[] _dmgPopupSprites;
        private float[] _dmgPopupTimers;
        private Vector3[] _dmgPopupWorldPositions;
        private Color[] _dmgPopupColors;

        // Achievement toast
        private Image _achievementImg;
        private Sprite _achievementSprite;
        private float _achievementTimer;
        private System.Collections.Generic.Queue<string> _achievementQueue
            = new System.Collections.Generic.Queue<string>();

        // Era intro card
        private Image _eraCardImg;
        private float _eraCardTimer;
        private Color _eraCardColor;
        private Image _modeSubtitleImg;
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
        private Image _friendTargetImg;
        private Sprite _friendTargetSprite;

        // Weapon wheel (dynamic — only shows acquired weapons + Unarmed)
        private GameObject _weaponWheelRoot;
        private CanvasGroup _weaponWheelGroup;
        private RectTransform _wheelRingRect;
        private RectTransform _wheelBgRect;
        private GameObject[] _wheelSlotGOs;
        private Image[] _wheelSlotImages;
        private int[] _wheelSlotWeaponIdx; // -1 = unarmed, 0-5 = weapon type
        private int _wheelVisibleCount;
        private Image _weaponWheelLabelImg;
        private Sprite _weaponWheelLabelSprite;
        private float _weaponWheelTimer;
        private int _lastWheelState = -99; // -1 = unarmed, 0-5 = weapon, -99 = uninitialized
        private int _lastAcquiredCount = -1;
        private const float WHEEL_HIGHLIGHT_DURATION = 1.5f;
        private const float WHEEL_RESTING_ALPHA = 0.9f;
        private const float WHEEL_SLOT_SIZE = 50f;

        // Weapon upgrade notification
        private Image _upgradeNotifyImg;
        private Sprite _upgradeNotifySprite;
        private float _upgradeNotifyTimer;
        private int _lastTierHash = -1;

        // Collected items display (compact row in L-strip)
        private Image _itemsImg;
        private Sprite _itemsSprite;
        private int _lastItemHash = -1;

        private const int MAX_HEARTS = 5;

        private void Start()
        {
            CreateUI();
            FindPlayerHealth();

            // Subscribe to score popup events
            Gameplay.GameManager.OnScorePopup += HandleScorePopup;

            // Subscribe to damage popup events
            Gameplay.GameManager.OnDamagePopup += HandleDamagePopup;

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
            UpdateBossBar();
            UpdateRunningScore();
            UpdateAbilityIcons();
            UpdateQuickDraw();
            UpdateAutoSelectReason();
            UpdateDpsCapFeedback();
            UpdateScorePopups();
            UpdateDamagePopups();
            UpdateAchievementToast();
            UpdateEraCard();
            UpdateModeSubtitle();
            UpdateDamageDirection();
            UpdateSpecialAttackMeter();
            UpdateFriendTarget();
            UpdateUpgradeNotification();
            UpdateCollectedItems();
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
            Gameplay.GameManager.OnDamagePopup -= HandleDamagePopup;
            Gameplay.GameManager.OnDamageDirection -= HandleDamageDirection;
            Gameplay.WeaponSystem.OnWeaponUpgraded -= HandleWeaponUpgrade;
            Gameplay.CosmeticManager.OnCosmeticUnlocked -= HandleCosmeticUnlock;

            if (Gameplay.AchievementManager.Instance != null)
                Gameplay.AchievementManager.Instance.OnAchievementUnlocked -= HandleAchievementUnlock;

            // Clean up non-cached sprites to prevent texture leaks
            CleanupSprite(ref _timerSprite);
            CleanupSprite(ref _livesSprite);
            CleanupSprite(ref _modeInfoSprite);
            CleanupSprite(ref _relicSprite);
            CleanupSprite(ref _scoreSprite);
            CleanupSprite(ref _autoSelectSprite);
            CleanupSprite(ref _bossNameSprite);
            CleanupSprite(ref _achievementSprite);
            CleanupSprite(ref _friendTargetSprite);
            CleanupSprite(ref _weaponWheelLabelSprite);
            CleanupSprite(ref _upgradeNotifySprite);
            CleanupSprite(ref _itemsSprite);
            if (_popupSprites != null)
                for (int i = 0; i < _popupSprites.Length; i++)
                    CleanupSprite(ref _popupSprites[i]);
            if (_dmgPopupSprites != null)
                for (int i = 0; i < _dmgPopupSprites.Length; i++)
                    CleanupSprite(ref _dmgPopupSprites[i]);
        }

        private void CleanupSprite(ref Sprite sprite)
        {
            if (sprite != null)
            {
                if (sprite.texture != null) Destroy(sprite.texture);
                Destroy(sprite);
                sprite = null;
            }
        }

        private void _UpdatePixelText(ref Sprite sprite, Image img, string text, Color color, int scale)
        {
            if (sprite != null && sprite.name == text) return;
            if (sprite != null) { Destroy(sprite.texture); Destroy(sprite); }
            sprite = Gameplay.PlaceholderAssets.CreatePixelTextSprite(text, color, scale);
            sprite.name = text;
            img.sprite = sprite;
            img.SetNativeSize();
        }

        private void CreateUI()
        {
            var canvasGO = new GameObject("HUDCanvas");
            canvasGO.transform.SetParent(transform);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 90;
            _canvas.pixelPerfect = true;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // ─── L-Shaped HUD Strip (bottom-left) with epoch border ───

            var gm = Gameplay.GameManager.Instance;
            int epoch = gm != null ? gm.CurrentLevelID.Epoch : 0;
            var (_, _, _, epochAccent) = Gameplay.PlaceholderAssets.GetEpochTerrainColors(epoch);

            // Epoch-specific border colors (double-line retro style)
            Color borderBright = new Color(
                Mathf.Min(epochAccent.r * 1.3f, 1f),
                Mathf.Min(epochAccent.g * 1.3f, 1f),
                Mathf.Min(epochAccent.b * 1.3f, 1f), 1f);
            Color borderDim = new Color(
                borderBright.r * 0.5f,
                borderBright.g * 0.5f,
                borderBright.b * 0.5f, 1f);

            // ── Outer border (bright epoch accent, 3px) ──

            // Tall column outer border
            var tcOuterGO = new GameObject("HUDTallColOuterBorder");
            tcOuterGO.transform.SetParent(canvasGO.transform, false);
            var tcOuterImg = tcOuterGO.AddComponent<Image>();
            tcOuterImg.color = borderBright;
            tcOuterImg.raycastTarget = false;
            var tcOuterRect = tcOuterGO.GetComponent<RectTransform>();
            tcOuterRect.anchorMin = new Vector2(0, 0);
            tcOuterRect.anchorMax = new Vector2(0, 0);
            tcOuterRect.pivot = new Vector2(0, 0);
            tcOuterRect.sizeDelta = new Vector2(175 + 6, 240 + 6);
            tcOuterRect.anchoredPosition = new Vector2(-3, -3);

            // Bottom strip outer border
            var stripOuterGO = new GameObject("HUDStripOuterBorder");
            stripOuterGO.transform.SetParent(canvasGO.transform, false);
            var stripOuterImg = stripOuterGO.AddComponent<Image>();
            stripOuterImg.color = borderBright;
            stripOuterImg.raycastTarget = false;
            var stripOuterRect = stripOuterGO.GetComponent<RectTransform>();
            stripOuterRect.anchorMin = new Vector2(0, 0);
            stripOuterRect.anchorMax = new Vector2(0.5f, 0);
            stripOuterRect.pivot = new Vector2(0, 0);
            stripOuterRect.sizeDelta = new Vector2(6, 125 + 6);
            stripOuterRect.anchoredPosition = new Vector2(-3, -3);

            // ── Inner border (dim epoch accent, 2px) ──

            // Tall column inner border
            var tcInnerGO = new GameObject("HUDTallColInnerBorder");
            tcInnerGO.transform.SetParent(canvasGO.transform, false);
            var tcInnerImg = tcInnerGO.AddComponent<Image>();
            tcInnerImg.color = borderDim;
            tcInnerImg.raycastTarget = false;
            var tcInnerRect = tcInnerGO.GetComponent<RectTransform>();
            tcInnerRect.anchorMin = new Vector2(0, 0);
            tcInnerRect.anchorMax = new Vector2(0, 0);
            tcInnerRect.pivot = new Vector2(0, 0);
            tcInnerRect.sizeDelta = new Vector2(175 + 2, 240 + 2);
            tcInnerRect.anchoredPosition = new Vector2(-1, -1);

            // Bottom strip inner border
            var stripInnerGO = new GameObject("HUDStripInnerBorder");
            stripInnerGO.transform.SetParent(canvasGO.transform, false);
            var stripInnerImg = stripInnerGO.AddComponent<Image>();
            stripInnerImg.color = borderDim;
            stripInnerImg.raycastTarget = false;
            var stripInnerRect = stripInnerGO.GetComponent<RectTransform>();
            stripInnerRect.anchorMin = new Vector2(0, 0);
            stripInnerRect.anchorMax = new Vector2(0.5f, 0);
            stripInnerRect.pivot = new Vector2(0, 0);
            stripInnerRect.sizeDelta = new Vector2(2, 125 + 2);
            stripInnerRect.anchoredPosition = new Vector2(-1, -1);

            // ── Fill panels ──

            // Tall left column (weapon wheel area — 175px wide, 240px tall)
            var tallColGO = new GameObject("HUDTallCol");
            tallColGO.transform.SetParent(canvasGO.transform, false);
            var tallColImg = tallColGO.AddComponent<Image>();
            tallColImg.color = new Color(0.05f, 0.04f, 0.12f, 0.90f);
            tallColImg.raycastTarget = false;
            var tallColRect = tallColGO.GetComponent<RectTransform>();
            tallColRect.anchorMin = new Vector2(0, 0);
            tallColRect.anchorMax = new Vector2(0, 0);
            tallColRect.pivot = new Vector2(0, 0);
            tallColRect.sizeDelta = new Vector2(175, 240);
            tallColRect.anchoredPosition = Vector2.zero;

            // Bottom strip (info area — from left column edge to 50% width, 125px tall)
            var stripBgGO = new GameObject("HUDStripBg");
            stripBgGO.transform.SetParent(canvasGO.transform, false);
            var stripBgImg = stripBgGO.AddComponent<Image>();
            stripBgImg.color = new Color(0.05f, 0.04f, 0.12f, 0.90f);
            stripBgImg.raycastTarget = false;
            var stripBgRect = stripBgGO.GetComponent<RectTransform>();
            stripBgRect.anchorMin = new Vector2(0, 0);
            stripBgRect.anchorMax = new Vector2(0.5f, 0);
            stripBgRect.pivot = new Vector2(0, 0);
            stripBgRect.sizeDelta = new Vector2(0, 125);
            stripBgRect.anchoredPosition = Vector2.zero;

            // Weapon wheel (tall left column)
            CreateWeaponWheel(canvasGO.transform);

            // ─── Top row: vital stats (y ≈ 88–112) ───

            // Difficulty badge (small label above hearts)
            var dm = Gameplay.DifficultyManager.Instance;
            if (dm != null)
            {
                Color diffColor = dm.CurrentDifficulty switch
                {
                    Gameplay.DifficultyLevel.Easy => new Color(0.4f, 0.8f, 0.4f),
                    Gameplay.DifficultyLevel.Hard => new Color(1f, 0.35f, 0.25f),
                    _ => new Color(0.7f, 0.7f, 0.8f)
                };
                string multStr = dm.ScoreMultiplier != 1f ? $" {dm.ScoreMultiplier:0.0}X" : "";
                string diffLabel = $"{dm.DifficultyName}{multStr}";
                var diffGO = new GameObject("DiffBadge");
                diffGO.transform.SetParent(canvasGO.transform, false);
                var diffImg = diffGO.AddComponent<Image>();
                diffImg.sprite = Gameplay.PlaceholderAssets.GetPixelTextSprite(diffLabel, Color.white, 2);
                diffImg.preserveAspect = true;
                diffImg.raycastTarget = false;
                diffImg.SetNativeSize();
                diffImg.color = diffColor;
                var diffRect = diffGO.GetComponent<RectTransform>();
                diffRect.anchorMin = new Vector2(0, 0);
                diffRect.anchorMax = new Vector2(0, 0);
                diffRect.pivot = new Vector2(0, 0);
                diffRect.anchoredPosition = new Vector2(185, 108);
            }

            // Hearts (bottom strip, top row)
            _hearts = new Image[MAX_HEARTS];
            for (int i = 0; i < MAX_HEARTS; i++)
            {
                var heartGO = new GameObject($"Heart_{i}");
                heartGO.transform.SetParent(canvasGO.transform, false);

                var img = heartGO.AddComponent<Image>();
                img.color = Color.red;

                var rect = heartGO.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(0, 0);
                rect.pivot = new Vector2(0, 0);
                rect.sizeDelta = new Vector2(34, 30);
                rect.anchoredPosition = new Vector2(185 + i * 40, 78);

                _hearts[i] = img;
            }

            // Running score (top row, 25px gap after hearts)
            var scoreGO = CreateHUDImage(canvasGO.transform, new Vector2(0, 0.5f));
            _scoreImg = scoreGO.GetComponent<Image>();
            _scoreImg.color = Color.white;
            _UpdatePixelText(ref _scoreSprite, _scoreImg, "0", Color.white, 4);
            var scoreRect = scoreGO.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0, 0);
            scoreRect.anchorMax = new Vector2(0, 0);
            scoreRect.pivot = new Vector2(0, 0);
            scoreRect.anchoredPosition = new Vector2(410, 80);

            // Timer (top row, 30px gap after score)
            var timerGO = CreateHUDImage(canvasGO.transform, new Vector2(0, 0.5f));
            _timerImg = timerGO.GetComponent<Image>();
            _timerImg.color = Color.white;
            _UpdatePixelText(ref _timerSprite, _timerImg, "0:00.0", Color.white, 3);
            var timerRect = timerGO.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0, 0);
            timerRect.anchorMax = new Vector2(0, 0);
            timerRect.pivot = new Vector2(0, 0);
            timerRect.anchoredPosition = new Vector2(590, 84);

            // Era name + level code (top row, right-aligned at 50% edge)
            var eraGO = CreateHUDImage(canvasGO.transform, new Vector2(1, 0.5f));
            _eraImg = eraGO.GetComponent<Image>();
            var eraRect = eraGO.GetComponent<RectTransform>();
            eraRect.anchorMin = new Vector2(0.5f, 0);
            eraRect.anchorMax = new Vector2(0.5f, 0);
            eraRect.pivot = new Vector2(1, 0);
            eraRect.anchoredPosition = new Vector2(-15, 84);

            if (gm != null && gm.CurrentLevelID.Epoch >= 0)
            {
                _eraImg.sprite = Gameplay.PlaceholderAssets.GetPixelTextSprite(
                    $"{gm.CurrentLevelID.EpochName} [{gm.CurrentLevelID.ToCode()}]", Color.white, 2);
                _eraImg.SetNativeSize();
                _eraImg.color = Color.white;
            }

            // ─── Bottom row: abilities & progress (y ≈ 20–50) ───

            // Heat bar (bottom row — hidden by default, shown for Cannon)
            CreateHeatBar(canvasGO.transform);

            // Ability icons (bottom row)
            CreateAbilityIcons(canvasGO.transform);

            // Special attack meter (below weapon wheel in tall column)
            CreateSpecialAttackBar(canvasGO.transform);

            // Relic counter (bottom row, right-aligned at 50% edge)
            if (gm != null && gm.TotalRelics > 0)
            {
                var relicGO = CreateHUDImage(canvasGO.transform, new Vector2(1, 0.5f));
                _relicImg = relicGO.GetComponent<Image>();
                _relicImg.color = new Color(1f, 0.85f, 0.2f);
                var relicRect = relicGO.GetComponent<RectTransform>();
                relicRect.anchorMin = new Vector2(0.5f, 0);
                relicRect.anchorMax = new Vector2(0.5f, 0);
                relicRect.pivot = new Vector2(1, 0);
                relicRect.sizeDelta = new Vector2(200, 24);
                relicRect.anchoredPosition = new Vector2(-15, 44);
            }

            // Pause button (top-right corner — stays at top)
            CreatePauseButton(canvasGO.transform);

            // Quick Draw flash text (below score, hidden by default)
            var qdGO = CreateHUDImage(canvasGO.transform, new Vector2(0.5f, 1));
            _quickDrawImg = qdGO.GetComponent<Image>();
            _quickDrawImg.sprite = Gameplay.PlaceholderAssets.GetPixelTextSprite("QUICK DRAW!", new Color(1f, 0.85f, 0.2f), 3);
            _quickDrawImg.SetNativeSize();
            _quickDrawImg.color = new Color(1f, 1f, 1f, 0f);
            var qdRect = qdGO.GetComponent<RectTransform>();
            qdRect.anchorMin = new Vector2(0.5f, 1);
            qdRect.anchorMax = new Vector2(0.5f, 1);
            qdRect.pivot = new Vector2(0.5f, 1);
            qdRect.anchoredPosition = new Vector2(0, -50);

            // Auto-select reason (bottom strip, below weapon name)
            var asGO = CreateHUDImage(canvasGO.transform, new Vector2(0, 0.5f));
            _autoSelectImg = asGO.GetComponent<Image>();
            _autoSelectImg.color = new Color(0.7f, 0.8f, 0.5f, 0f);
            var asRect = asGO.GetComponent<RectTransform>();
            asRect.anchorMin = new Vector2(0, 0);
            asRect.anchorMax = new Vector2(0, 0);
            asRect.pivot = new Vector2(0, 0);
            asRect.anchoredPosition = new Vector2(185, 8);

            // DPS cap "RESISTED" text (center, hidden)
            var dpsGO = CreateHUDImage(canvasGO.transform, new Vector2(0.5f, 1));
            _dpsCapImg = dpsGO.GetComponent<Image>();
            _dpsCapImg.sprite = Gameplay.PlaceholderAssets.GetPixelTextSprite("RESISTED", new Color(0.5f, 0.5f, 0.5f), 3);
            _dpsCapImg.SetNativeSize();
            _dpsCapImg.color = new Color(1f, 1f, 1f, 0f);
            var dpsRect = dpsGO.GetComponent<RectTransform>();
            dpsRect.anchorMin = new Vector2(0.5f, 1);
            dpsRect.anchorMax = new Vector2(0.5f, 1);
            dpsRect.pivot = new Vector2(0.5f, 1);
            dpsRect.anchoredPosition = new Vector2(0, -70);

            // Lives counter (bottom strip, bottom row, for Campaign/Streak)
            if (gm != null && gm.CurrentGameMode != Gameplay.GameMode.TheBreach)
            {
                var livesGO = CreateHUDImage(canvasGO.transform, new Vector2(0, 0.5f));
                _livesImg = livesGO.GetComponent<Image>();
                _livesImg.color = new Color(1f, 0.85f, 0.2f);
                var livesRect = livesGO.GetComponent<RectTransform>();
                livesRect.anchorMin = new Vector2(0, 0);
                livesRect.anchorMax = new Vector2(0, 0);
                livesRect.pivot = new Vector2(0, 0);
                livesRect.anchoredPosition = new Vector2(640, 44);
            }

            // Collected items summary (bottom strip, bottom row)
            var itemsGO = CreateHUDImage(canvasGO.transform, new Vector2(0, 0.5f));
            _itemsImg = itemsGO.GetComponent<Image>();
            _itemsImg.color = new Color(0.75f, 0.85f, 0.65f, 0f);
            var itemsRect = itemsGO.GetComponent<RectTransform>();
            itemsRect.anchorMin = new Vector2(0, 0);
            itemsRect.anchorMax = new Vector2(0, 0);
            itemsRect.pivot = new Vector2(0, 0);
            itemsRect.sizeDelta = new Vector2(400, 20);
            itemsRect.anchoredPosition = new Vector2(500, 44);

            // Score popup pool
            _popupImgs = new Image[POPUP_POOL_SIZE];
            _popupSprites = new Sprite[POPUP_POOL_SIZE];
            _popupTimers = new float[POPUP_POOL_SIZE];
            _popupWorldPositions = new Vector3[POPUP_POOL_SIZE];
            for (int i = 0; i < POPUP_POOL_SIZE; i++)
            {
                var popGO = CreateHUDImage(canvasGO.transform, new Vector2(0.5f, 0.5f));
                _popupImgs[i] = popGO.GetComponent<Image>();
                _popupImgs[i].color = new Color(1f, 0.85f, 0.2f, 0f);
            }

            // Damage number popup pool
            _dmgPopupImgs = new Image[DMG_POPUP_POOL_SIZE];
            _dmgPopupSprites = new Sprite[DMG_POPUP_POOL_SIZE];
            _dmgPopupTimers = new float[DMG_POPUP_POOL_SIZE];
            _dmgPopupWorldPositions = new Vector3[DMG_POPUP_POOL_SIZE];
            _dmgPopupColors = new Color[DMG_POPUP_POOL_SIZE];
            for (int i = 0; i < DMG_POPUP_POOL_SIZE; i++)
            {
                var dmgGO = CreateHUDImage(canvasGO.transform, new Vector2(0.5f, 0.5f));
                _dmgPopupImgs[i] = dmgGO.GetComponent<Image>();
                _dmgPopupImgs[i].color = new Color(1f, 1f, 1f, 0f);
            }

            // Achievement toast (slide from top)
            var achGO = CreateHUDImage(canvasGO.transform, new Vector2(0.5f, 0.5f));
            _achievementImg = achGO.GetComponent<Image>();
            _achievementImg.color = new Color(1f, 0.85f, 0.2f, 0f);
            var achRect = achGO.GetComponent<RectTransform>();
            achRect.anchorMin = new Vector2(0.5f, 1);
            achRect.anchorMax = new Vector2(0.5f, 1);
            achRect.pivot = new Vector2(0.5f, 1);
            achRect.anchoredPosition = new Vector2(0, 40); // Starts off-screen above

            // Era intro card (large centered text, fades after 2s)
            var eraCardGO = CreateHUDImage(canvasGO.transform, new Vector2(0.5f, 0.5f));
            _eraCardImg = eraCardGO.GetComponent<Image>();
            _eraCardImg.color = new Color(1f, 1f, 1f, 0f);
            var eraCardRect = eraCardGO.GetComponent<RectTransform>();
            eraCardRect.anchorMin = new Vector2(0.5f, 0.5f);
            eraCardRect.anchorMax = new Vector2(0.5f, 0.5f);
            eraCardRect.pivot = new Vector2(0.5f, 0.5f);
            eraCardRect.anchoredPosition = new Vector2(0, 50);

            if (gm != null)
            {
                int cardEpoch = gm.CurrentEpoch;
                string eraName = gm.CurrentLevelID.EpochName.ToUpper();
                _eraCardColor = GetEraAccentColor(cardEpoch);
                _eraCardImg.sprite = Gameplay.PlaceholderAssets.GetPixelTextSprite(
                    $"ERA {cardEpoch + 1}: {eraName}", _eraCardColor, 6);
                _eraCardImg.SetNativeSize();
                _eraCardImg.color = new Color(1f, 1f, 1f, 0f);
                _eraCardTimer = 2.5f;
            }

            // Mode subtitle (below era card — explains the current game mode)
            var modeSubGO = CreateHUDImage(canvasGO.transform, new Vector2(0.5f, 0.5f));
            _modeSubtitleImg = modeSubGO.GetComponent<Image>();
            _modeSubtitleImg.color = new Color(1f, 1f, 1f, 0f);
            var modeSubRect = modeSubGO.GetComponent<RectTransform>();
            modeSubRect.anchorMin = new Vector2(0.5f, 0.5f);
            modeSubRect.anchorMax = new Vector2(0.5f, 0.5f);
            modeSubRect.pivot = new Vector2(0.5f, 0.5f);
            modeSubRect.anchoredPosition = new Vector2(0, 10);

            if (gm != null)
            {
                string subtitleText = gm.CurrentGameMode switch
                {
                    Gameplay.GameMode.TheBreach => "RANDOMIZED EPOCHS -- SURVIVE AS LONG AS YOU CAN",
                    Gameplay.GameMode.Campaign => "SEQUENTIAL EPOCHS -- UNLOCK COSMETICS",
                    Gameplay.GameMode.Streak => "ENDLESS RANDOM LEVELS -- SURVIVE THE STREAK",
                    _ => ""
                };
                if (!string.IsNullOrEmpty(subtitleText))
                {
                    _modeSubtitleImg.sprite = Gameplay.PlaceholderAssets.GetPixelTextSprite(
                        subtitleText, new Color(0.7f, 0.7f, 0.8f), 3);
                    _modeSubtitleImg.SetNativeSize();
                }
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

                var targetGO = CreateHUDImage(canvasGO.transform, new Vector2(0.5f, 1));
                _friendTargetImg = targetGO.GetComponent<Image>();
                _friendTargetImg.color = new Color(0.5f, 0.8f, 1f);
                _UpdatePixelText(ref _friendTargetSprite, _friendTargetImg, $"TARGET: {gm.FriendChallengeScore:N0}", Color.white, 3);
                var targetRect = targetGO.GetComponent<RectTransform>();
                targetRect.anchorMin = new Vector2(0.5f, 1);
                targetRect.anchorMax = new Vector2(0.5f, 1);
                targetRect.pivot = new Vector2(0.5f, 1);
                targetRect.anchoredPosition = new Vector2(0, -66);
            }

            // Weapon upgrade notification (center-bottom, hidden by default)
            var upgradeGO = CreateHUDImage(canvasGO.transform, new Vector2(0.5f, 0.5f));
            _upgradeNotifyImg = upgradeGO.GetComponent<Image>();
            _upgradeNotifyImg.color = new Color(1f, 0.85f, 0.2f, 0f);
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
                var modeGO = CreateHUDImage(canvasGO.transform, new Vector2(0.5f, 1));
                _modeInfoImg = modeGO.GetComponent<Image>();
                _modeInfoImg.color = new Color(0.9f, 0.85f, 0.7f);
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
            if (_timerImg == null || Gameplay.GameManager.Instance == null) return;
            float elapsed = Gameplay.GameManager.Instance.LevelElapsedTime;
            int minutes = (int)(elapsed / 60f);
            float seconds = elapsed % 60f;
            _UpdatePixelText(ref _timerSprite, _timerImg, $"{minutes}:{seconds:00.0}", Color.white, 3);
        }

        private void UpdateWeaponDisplay()
        {
            if (_cachedWeaponSystem == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                    _cachedWeaponSystem = player.GetComponent<Gameplay.WeaponSystem>();
            }
            var ws = _cachedWeaponSystem;
            if (ws == null) return;

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
            if (_weaponWheelLabelImg != null)
            {
                if (ws.IsUnarmed)
                {
                    _UpdatePixelText(ref _weaponWheelLabelSprite, _weaponWheelLabelImg, "UNARMED", Color.white, 3);
                    _weaponWheelLabelImg.color = Color.gray;
                }
                else
                {
                    _UpdatePixelText(ref _weaponWheelLabelSprite, _weaponWheelLabelImg, ws.ActiveWeaponType.ToString(), Color.white, 3);
                    _weaponWheelLabelImg.color = GetWeaponDisplayColor(ws.ActiveWeaponType);
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
            rootRect.anchoredPosition = new Vector2(88, 150);
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
            var labelGO = CreateHUDImage(_weaponWheelRoot.transform, new Vector2(0.5f, 0.5f));
            _weaponWheelLabelImg = labelGO.GetComponent<Image>();
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchoredPosition = Vector2.zero;

            // Slots are created dynamically by RebuildWheelSlots
        }

        private void CreateHeatBar(Transform parent)
        {
            // Background (bottom strip, bottom row — Cannon heat)
            var bgGO = new GameObject("HeatBarBg");
            bgGO.transform.SetParent(parent, false);
            _heatBarBg = bgGO.AddComponent<Image>();
            _heatBarBg.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0);
            bgRect.anchorMax = new Vector2(0, 0);
            bgRect.pivot = new Vector2(0, 0);
            bgRect.sizeDelta = new Vector2(120, 8);
            bgRect.anchoredPosition = new Vector2(185, 24);

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
            if (_relicImg == null) return;
            var gm = Gameplay.GameManager.Instance;
            if (gm == null) return;

            int preserved = Mathf.Max(0, gm.TotalRelics - gm.RelicsDestroyed);
            _UpdatePixelText(ref _relicSprite, _relicImg, $"RELICS PRESERVED: {preserved}/{gm.TotalRelics}", Color.white, 2);

            // Gold when all preserved, red-tinted when some lost
            _relicImg.color = gm.RelicsDestroyed > 0
                ? new Color(1f, 0.5f, 0.3f)
                : new Color(1f, 0.85f, 0.2f);
        }

        private void UpdateModeInfo()
        {
            var gm = Gameplay.GameManager.Instance;
            if (gm == null) return;

            if (_livesImg != null)
            {
                // Campaign has infinite lives — show deaths/level instead of global lives
                if (gm.CurrentGameMode == Gameplay.GameMode.Campaign)
                    _UpdatePixelText(ref _livesSprite, _livesImg, $"DEATHS: {gm.DeathsThisLevel}", Color.white, 3);
                else
                    _UpdatePixelText(ref _livesSprite, _livesImg, $"LIVES: {gm.GlobalLives}", Color.white, 3);
            }

            if (_modeInfoImg != null)
            {
                switch (gm.CurrentGameMode)
                {
                    case Gameplay.GameMode.Campaign:
                        _UpdatePixelText(ref _modeInfoSprite, _modeInfoImg, $"CAMPAIGN - EPOCH {gm.CampaignEpoch + 1}/10", Color.white, 3);
                        break;
                    case Gameplay.GameMode.Streak:
                        _UpdatePixelText(ref _modeInfoSprite, _modeInfoImg, $"STREAK: {gm.StreakCount}", Color.white, 3);
                        break;
                }
            }
        }

        private void CreateAbilityIcons(Transform parent)
        {
            // Double Jump icon (bottom strip, bottom row)
            var djGO = new GameObject("DoubleJumpIcon");
            djGO.transform.SetParent(parent, false);
            _doubleJumpIcon = djGO.AddComponent<Image>();
            _doubleJumpIcon.color = new Color(0.3f, 0.3f, 0.3f, 0.3f); // Dimmed until acquired
            var djRect = djGO.GetComponent<RectTransform>();
            djRect.anchorMin = new Vector2(0, 0);
            djRect.anchorMax = new Vector2(0, 0);
            djRect.pivot = new Vector2(0, 0);
            djRect.sizeDelta = new Vector2(26, 26);
            djRect.anchoredPosition = new Vector2(410, 44);

            // Air Dash icon (next to double jump)
            var adGO = new GameObject("AirDashIcon");
            adGO.transform.SetParent(parent, false);
            _airDashIcon = adGO.AddComponent<Image>();
            _airDashIcon.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
            var adRect = adGO.GetComponent<RectTransform>();
            adRect.anchorMin = new Vector2(0, 0);
            adRect.anchorMax = new Vector2(0, 0);
            adRect.pivot = new Vector2(0, 0);
            adRect.sizeDelta = new Vector2(26, 26);
            adRect.anchoredPosition = new Vector2(450, 44);
        }

        private void UpdateRunningScore()
        {
            if (_scoreImg == null) return;
            var gm = Gameplay.GameManager.Instance;
            if (gm == null) return;

            int runningScore = gm.ItemBonusScore + gm.EnemyBonusScore;
            if (runningScore != _displayedScore)
            {
                _displayedScore = runningScore;
                _UpdatePixelText(ref _scoreSprite, _scoreImg, _displayedScore.ToString(), Color.white, 4);
                _scoreScaleTimer = 0.2f;
            }

            // Scale-up animation on change
            if (_scoreScaleTimer > 0f)
            {
                _scoreScaleTimer -= Time.deltaTime;
                float scale = 1f + Mathf.Clamp01(_scoreScaleTimer / 0.2f) * 0.3f;
                _scoreImg.transform.localScale = new Vector3(scale, scale, 1f);
                _scoreImg.color = Color.Lerp(Color.white, new Color(1f, 0.85f, 0.2f), _scoreScaleTimer / 0.2f);
            }
            else
            {
                _scoreImg.transform.localScale = Vector3.one;
                _scoreImg.color = Color.white;
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

            // "QUICK DRAW!" flash text
            if (_quickDrawImg != null)
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
                    _quickDrawImg.color = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    _quickDrawImg.color = new Color(1f, 1f, 1f, 0f);
                }
            }
        }

        private void UpdateAutoSelectReason()
        {
            if (_autoSelectImg == null) return;
            var ws = _cachedWeaponSystem;
            if (ws == null) return;

            if (ws.AutoSelectReasonTimer > 0f && !string.IsNullOrEmpty(ws.LastAutoSelectReason))
            {
                _UpdatePixelText(ref _autoSelectSprite, _autoSelectImg, $">>{ws.LastAutoSelectReason}", Color.white, 2);
                float alpha = Mathf.Clamp01(ws.AutoSelectReasonTimer / 0.5f);
                _autoSelectImg.color = new Color(0.7f, 0.8f, 0.5f, alpha);
            }
            else
            {
                _autoSelectImg.color = new Color(0.7f, 0.8f, 0.5f, 0f);
            }
        }

        private void UpdateDpsCapFeedback()
        {
            if (_dpsCapImg == null) return;

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
                _dpsCapImg.color = new Color(1f, 1f, 1f, alpha);
            }
            else
            {
                _dpsCapImg.color = new Color(1f, 1f, 1f, 0f);
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
            _bossNameImg = nameGO.AddComponent<Image>();
            _bossNameImg.preserveAspect = true;
            _bossNameImg.raycastTarget = false;
            _bossNameImg.color = new Color(1f, 0.85f, 0.7f);
            var nameRect = nameGO.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 1);
            nameRect.anchorMax = new Vector2(0.5f, 1);
            nameRect.pivot = new Vector2(0.5f, 1);
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
                _UpdatePixelText(ref _bossNameSprite, _bossNameImg, displayName, Color.white, 3);
                _bossNameImg.color = new Color(1f, 0.85f, 0.7f);
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

        private GameObject CreateHUDImage(Transform parent, Vector2 pivot)
        {
            var go = new GameObject("HUDText");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.sprite = null;
            img.color = new Color(1f, 1f, 1f, 0f);
            img.preserveAspect = true;
            img.raycastTarget = false;

            var rect = go.GetComponent<RectTransform>();
            rect.pivot = pivot;
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
                    _UpdatePixelText(ref _popupSprites[i], _popupImgs[i], $"+{score}", Color.white, 3);
                    _popupImgs[i].color = new Color(1f, 0.85f, 0.2f, 1f);
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
            _UpdatePixelText(ref _popupSprites[oldestIdx], _popupImgs[oldestIdx], $"+{score}", Color.white, 3);
            _popupImgs[oldestIdx].color = new Color(1f, 0.85f, 0.2f, 1f);
            _popupTimers[oldestIdx] = 1f;
            _popupWorldPositions[oldestIdx] = worldPos + new Vector3(0, 0.5f, 0);
        }

        private void UpdateScorePopups()
        {
            if (_popupImgs == null) return;
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

                    var rect = _popupImgs[i].GetComponent<RectTransform>();
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _canvas.GetComponent<RectTransform>(), screenPos, null, out Vector2 localPos);
                    rect.anchoredPosition = localPos;
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);

                    float alpha = Mathf.Clamp01(_popupTimers[i] / 0.4f);
                    float scale = 1f + (1f - Mathf.Clamp01(_popupTimers[i])) * 0.3f;
                    _popupImgs[i].color = new Color(1f, 0.85f, 0.2f, alpha);
                    _popupImgs[i].transform.localScale = new Vector3(scale, scale, 1f);
                }
                else
                {
                    _popupImgs[i].color = new Color(1f, 0.85f, 0.2f, 0f);
                }
            }
        }

        private void HandleDamagePopup(Vector3 worldPos, int damage, Color color)
        {
            if (_dmgPopupImgs == null) return;

            // Slight random X offset so overlapping hits don't stack exactly
            float xOffset = UnityEngine.Random.Range(-0.3f, 0.3f);
            Vector3 pos = worldPos + new Vector3(xOffset, 0.6f, 0);

            // Kill shots (red) get larger text
            bool isKillShot = color.r > 0.9f && color.g < 0.3f;
            int dmgScale = isKillShot ? 4 : 3;

            // Find available slot
            for (int i = 0; i < DMG_POPUP_POOL_SIZE; i++)
            {
                if (_dmgPopupTimers[i] <= 0f)
                {
                    CleanupSprite(ref _dmgPopupSprites[i]);
                    _dmgPopupSprites[i] = Gameplay.PlaceholderAssets.CreatePixelTextSprite(damage.ToString(), Color.white, dmgScale);
                    _dmgPopupImgs[i].sprite = _dmgPopupSprites[i];
                    _dmgPopupImgs[i].SetNativeSize();
                    _dmgPopupColors[i] = color;
                    _dmgPopupImgs[i].color = new Color(color.r, color.g, color.b, 1f);
                    _dmgPopupTimers[i] = 0.6f;
                    _dmgPopupWorldPositions[i] = pos;
                    return;
                }
            }

            // No free slot — reuse oldest
            int oldestIdx = 0;
            float oldestTime = float.MaxValue;
            for (int i = 0; i < DMG_POPUP_POOL_SIZE; i++)
            {
                if (_dmgPopupTimers[i] < oldestTime)
                {
                    oldestTime = _dmgPopupTimers[i];
                    oldestIdx = i;
                }
            }
            CleanupSprite(ref _dmgPopupSprites[oldestIdx]);
            _dmgPopupSprites[oldestIdx] = Gameplay.PlaceholderAssets.CreatePixelTextSprite(damage.ToString(), Color.white, dmgScale);
            _dmgPopupImgs[oldestIdx].sprite = _dmgPopupSprites[oldestIdx];
            _dmgPopupImgs[oldestIdx].SetNativeSize();
            _dmgPopupColors[oldestIdx] = color;
            _dmgPopupImgs[oldestIdx].color = new Color(color.r, color.g, color.b, 1f);
            _dmgPopupTimers[oldestIdx] = 0.6f;
            _dmgPopupWorldPositions[oldestIdx] = pos;
        }

        private void UpdateDamagePopups()
        {
            if (_dmgPopupImgs == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            for (int i = 0; i < DMG_POPUP_POOL_SIZE; i++)
            {
                if (_dmgPopupTimers[i] > 0f)
                {
                    _dmgPopupTimers[i] -= Time.deltaTime;
                    _dmgPopupWorldPositions[i] += new Vector3(0, 2f * Time.deltaTime, 0);

                    Vector3 screenPos = cam.WorldToScreenPoint(_dmgPopupWorldPositions[i]);
                    if (screenPos.z < 0) { _dmgPopupTimers[i] = 0f; continue; }

                    var rect = _dmgPopupImgs[i].GetComponent<RectTransform>();
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        _canvas.GetComponent<RectTransform>(), screenPos, null, out Vector2 localPos);
                    rect.anchoredPosition = localPos;
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);

                    float alpha = Mathf.Clamp01(_dmgPopupTimers[i] / 0.2f);
                    var c = _dmgPopupColors[i];
                    _dmgPopupImgs[i].color = new Color(c.r, c.g, c.b, alpha);
                }
                else
                {
                    _dmgPopupImgs[i].color = new Color(1f, 1f, 1f, 0f);
                }
            }
        }

        private void HandleAchievementUnlock(Gameplay.AchievementType type)
        {
            string name = Gameplay.AchievementManager.GetAchievementName(type);
            _achievementQueue.Enqueue(name);
        }

        private void UpdateAchievementToast()
        {
            if (_achievementImg == null) return;

            if (_achievementTimer > 0f)
            {
                _achievementTimer -= Time.deltaTime;

                // Slide in from top (y: 40 → -10)
                float slideIn = Mathf.Clamp01(1f - (_achievementTimer - 2.5f) / 0.5f);
                float slideOut = Mathf.Clamp01(_achievementTimer / 0.5f);
                float y = Mathf.Lerp(40f, -10f, slideIn);
                float alpha = slideOut;

                var rect = _achievementImg.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(0, y);
                _achievementImg.color = new Color(1f, 0.85f, 0.2f, alpha);
            }
            else if (_achievementQueue.Count > 0)
            {
                string name = _achievementQueue.Dequeue();
                _UpdatePixelText(ref _achievementSprite, _achievementImg, $"ACHIEVEMENT: {name}", Color.white, 3);
                _achievementTimer = 3f;
            }
            else
            {
                _achievementImg.color = new Color(1f, 0.85f, 0.2f, 0f);
            }
        }

        private void UpdateEraCard()
        {
            if (_eraCardImg == null || _eraCardTimer <= 0f) return;
            _eraCardTimer -= Time.deltaTime;
            float fadeIn = Mathf.Clamp01((2.5f - _eraCardTimer) / 0.3f);
            float fadeOut = Mathf.Clamp01(_eraCardTimer / 0.5f);
            float alpha = Mathf.Min(fadeIn, fadeOut);
            _eraCardImg.color = new Color(1f, 1f, 1f, alpha);
        }

        private void UpdateModeSubtitle()
        {
            if (_modeSubtitleImg == null || _modeSubtitleTimer <= 0f) return;
            _modeSubtitleTimer -= Time.deltaTime;
            float fadeIn = Mathf.Clamp01((3f - _modeSubtitleTimer) / 0.5f);
            float fadeOut = Mathf.Clamp01(_modeSubtitleTimer / 0.5f);
            float alpha = Mathf.Min(fadeIn, fadeOut) * 0.8f;
            _modeSubtitleImg.color = new Color(1f, 1f, 1f, alpha);
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
            // Background bar (below weapon wheel in L-strip)
            var bgGO = new GameObject("SpecialBarBg");
            bgGO.transform.SetParent(parent, false);
            _specialBarBg = bgGO.AddComponent<Image>();
            _specialBarBg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            _specialBarBg.raycastTarget = false;
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0);
            bgRect.anchorMax = new Vector2(0, 0);
            bgRect.pivot = new Vector2(0.5f, 0);
            bgRect.sizeDelta = new Vector2(140, 6);
            bgRect.anchoredPosition = new Vector2(88, 6);

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
            if (_friendTargetImg == null) return;
            var gm = Gameplay.GameManager.Instance;
            if (gm == null || gm.FriendChallengeScore <= 0) return;

            int currentRunning = gm.ItemBonusScore + gm.EnemyBonusScore;
            int target = gm.FriendChallengeScore;

            if (currentRunning >= target)
            {
                // Player has beaten the friend's score
                _UpdatePixelText(ref _friendTargetSprite, _friendTargetImg, $"TARGET: {target:N0} - BEATEN!", Color.white, 3);
                _friendTargetImg.color = new Color(0.4f, 1f, 0.4f);
            }
            else
            {
                int remaining = target - currentRunning;
                _UpdatePixelText(ref _friendTargetSprite, _friendTargetImg, $"TARGET: {target:N0} (NEED {remaining:N0})", Color.white, 3);
                _friendTargetImg.color = new Color(0.5f, 0.8f, 1f);
            }
        }

        private void HandleCosmeticUnlock(string cosmeticName)
        {
            _achievementQueue.Enqueue($"NEW: {cosmeticName} unlocked!");
        }

        private void HandleWeaponUpgrade(Generative.WeaponType type, Generative.WeaponTier tier)
        {
            if (_upgradeNotifyImg == null) return;
            _UpdatePixelText(ref _upgradeNotifySprite, _upgradeNotifyImg, $"{type} >> {tier}", Color.white, 4);
            _upgradeNotifyTimer = 2f;
        }

        private void UpdateUpgradeNotification()
        {
            if (_upgradeNotifyImg == null || _upgradeNotifyTimer <= 0f) return;
            _upgradeNotifyTimer -= Time.deltaTime;
            float alpha = Mathf.Clamp01(_upgradeNotifyTimer / 0.5f);
            float scale = 1f + Mathf.Clamp01((_upgradeNotifyTimer - 1.5f) / 0.5f) * 0.3f;
            _upgradeNotifyImg.color = new Color(1f, 0.85f, 0.2f, alpha);
            _upgradeNotifyImg.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private void UpdateCollectedItems()
        {
            if (_itemsImg == null) return;
            var gm = Gameplay.GameManager.Instance;
            if (gm == null) return;

            // Build a hash to detect changes, avoiding string alloc every frame
            int hash = 0;
            foreach (var kvp in gm.RewardCounts)
                hash += (int)kvp.Key * 100 + kvp.Value;
            foreach (var kvp in gm.WeaponCounts)
                hash += ((int)kvp.Key + 10) * 100 + kvp.Value;

            if (hash == _lastItemHash) return;
            _lastItemHash = hash;

            var sb = new System.Text.StringBuilder(64);
            foreach (var kvp in gm.RewardCounts)
            {
                if (kvp.Value <= 0) continue;
                if (sb.Length > 0) sb.Append("  ");
                string label = kvp.Key switch
                {
                    Generative.RewardType.HealthSmall => "HP",
                    Generative.RewardType.HealthLarge => "HP+",
                    Generative.RewardType.AttackBoost => "ATK",
                    Generative.RewardType.SpeedBoost => "SPD",
                    Generative.RewardType.Shield => "SHD",
                    Generative.RewardType.Coin => "$",
                    _ => kvp.Key.ToString()
                };
                sb.Append(label).Append(':').Append(kvp.Value);
            }
            foreach (var kvp in gm.WeaponCounts)
            {
                if (kvp.Value <= 0) continue;
                if (sb.Length > 0) sb.Append("  ");
                sb.Append("WPN:").Append(kvp.Value);
            }

            string itemStr = sb.ToString();
            if (!string.IsNullOrEmpty(itemStr))
            {
                _UpdatePixelText(ref _itemsSprite, _itemsImg, itemStr, Color.white, 2);
                _itemsImg.color = new Color(0.75f, 0.85f, 0.65f);
            }
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
            var pauseImg = textGO.AddComponent<Image>();
            pauseImg.sprite = Gameplay.PlaceholderAssets.GetPixelTextSprite("||", Color.white, 4);
            pauseImg.preserveAspect = true;
            pauseImg.raycastTarget = false;
            pauseImg.SetNativeSize();
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
        }
    }
}
