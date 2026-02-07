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

        // Boss health bar
        private GameObject _bossBarRoot;
        private Image _bossBarBg;
        private Image _bossBarFill;
        private Text _bossNameText;
        private float _bossBarFlashTimer;
        private int _lastBossHealth = -1;

        private const int MAX_HEARTS = 5;

        private void Start()
        {
            CreateUI();
            FindPlayerHealth();
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
        }

        private void FindPlayerHealth()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                _healthSystem = player.GetComponent<Gameplay.HealthSystem>();
                _cachedWeaponSystem = player.GetComponent<Gameplay.WeaponSystem>();
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
