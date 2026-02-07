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
        private Text _weaponSlotsText;
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

        private const int MAX_HEARTS = 5;

        private void Start()
        {
            CreateUI();
            FindPlayerHealth();

            // Subscribe to score popup events
            Gameplay.GameManager.OnScorePopup += HandleScorePopup;

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

            // Weapon slots indicator (top-right, below weapon name)
            var slotsGO = CreateHUDText(canvasGO.transform, "", TextAnchor.UpperRight);
            _weaponSlotsText = slotsGO.GetComponent<Text>();
            _weaponSlotsText.fontSize = 16;
            _weaponSlotsText.color = new Color(0.7f, 0.7f, 0.7f);
            var slotsRect = slotsGO.GetComponent<RectTransform>();
            slotsRect.anchorMin = new Vector2(1, 1);
            slotsRect.anchorMax = new Vector2(1, 1);
            slotsRect.pivot = new Vector2(1, 1);
            slotsRect.anchoredPosition = new Vector2(-20, -46);

            // Heat bar (top-right, below weapon slots — hidden by default)
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
            scoreRect.anchoredPosition = new Vector2(0, -20);

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
            _autoSelectText.fontSize = 14;
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
            if (gm != null && gm.CurrentGameMode != Gameplay.GameMode.FreePlay)
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

            // Boss health bar (hidden by default, shown when boss is active)
            CreateBossBar(canvasGO.transform);

            // Mode info (top-center: epoch or streak)
            if (gm != null && gm.CurrentGameMode != Gameplay.GameMode.FreePlay)
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

            // Show active weapon name + tier with color
            string typeName = ws.ActiveWeaponType.ToString();
            Color typeColor = GetWeaponDisplayColor(ws.ActiveWeaponType);
            string tierSuffix = ws.ActiveWeaponTier != Generative.WeaponTier.Starting
                ? $" [{ws.ActiveWeaponTier}]" : "";
            _weaponText.text = $"{typeName}{tierSuffix}";
            _weaponText.color = typeColor;

            // Show acquired weapon slots as dots
            if (_weaponSlotsText != null)
            {
                var slots = ws.Slots;
                string dots = "";
                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i].Acquired)
                        dots += i == (int)ws.ActiveWeaponType ? "\u25cf " : "\u25cb ";
                }
                _weaponSlotsText.text = dots.TrimEnd();
            }
        }

        private static Color GetWeaponDisplayColor(Generative.WeaponType type)
        {
            return type switch
            {
                Generative.WeaponType.Bolt => Color.white,
                Generative.WeaponType.Piercer => new Color(0.6f, 0.9f, 1f),
                Generative.WeaponType.Spreader => new Color(1f, 0.7f, 0.3f),
                Generative.WeaponType.Chainer => new Color(0.5f, 0.8f, 1f),
                Generative.WeaponType.Slower => new Color(0.6f, 0.4f, 1f),
                Generative.WeaponType.Cannon => new Color(1f, 0.3f, 0.2f),
                _ => Color.white
            };
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
            _relicText.text = $"Relics: {preserved}/{gm.TotalRelics} recovered";

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
