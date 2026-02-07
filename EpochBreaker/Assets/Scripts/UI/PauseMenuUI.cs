using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using EpochBreaker.Gameplay;

namespace EpochBreaker.UI
{
    /// <summary>
    /// Pause menu overlay with Resume, Restart, Settings, and Quit buttons.
    /// Settings sub-panel provides audio, accessibility, and difficulty controls.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        private GameObject _mainContent;
        private GameObject _settingsContent;
        private GameObject _settingsMenuContent;
        private GameObject _audioSubPanel;
        private GameObject _accessibilitySubPanel;
        private GameObject _difficultySubPanel;

        private void Start()
        {
            CreateUI();
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (InputManager.IsBackPressed())
            {
                // Navigate back through sub-panel hierarchy
                if (_accessibilitySubPanel != null && _accessibilitySubPanel.activeSelf)
                {
                    _accessibilitySubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                    InputManager.PausePressed = false;
                }
                else if (_difficultySubPanel != null && _difficultySubPanel.activeSelf)
                {
                    _difficultySubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                    InputManager.PausePressed = false;
                }
                else if (_audioSubPanel != null && _audioSubPanel.activeSelf)
                {
                    _audioSubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                    InputManager.PausePressed = false;
                }
                else if (_settingsContent != null && _settingsContent.activeSelf)
                {
                    _settingsContent.SetActive(false);
                    _mainContent.SetActive(true);
                    InputManager.PausePressed = false;
                }
                else
                {
                    // No sub-panel open — resume game
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    GameManager.Instance?.ResumeGame();
                    InputManager.PausePressed = false;
                }
            }
        }

        private void CreateUI()
        {
            var canvasGO = new GameObject("PauseCanvas");
            canvasGO.transform.SetParent(transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Semi-transparent overlay
            var overlayGO = new GameObject("Overlay");
            overlayGO.transform.SetParent(canvasGO.transform, false);
            var overlayImg = overlayGO.AddComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.7f);
            var overlayRect = overlayGO.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            // ── Main pause content ──
            _mainContent = new GameObject("MainContent");
            _mainContent.transform.SetParent(canvasGO.transform, false);
            var mainRect = _mainContent.AddComponent<RectTransform>();
            mainRect.anchorMin = Vector2.zero;
            mainRect.anchorMax = Vector2.one;
            mainRect.sizeDelta = Vector2.zero;

            // Title
            CreateText(_mainContent.transform, "PAUSED", 48, Color.white, new Vector2(0, 160));

            // Resume button
            CreateButton(_mainContent.transform, "RESUME", new Vector2(0, 60),
                new Color(0.2f, 0.5f, 0.3f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    GameManager.Instance?.ResumeGame();
                });

            // Restart button
            CreateButton(_mainContent.transform, "RESTART", new Vector2(0, -20),
                new Color(0.5f, 0.4f, 0.2f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    GameManager.Instance?.RestartLevel();
                });

            // Settings button
            CreateButton(_mainContent.transform, "SETTINGS", new Vector2(0, -100),
                new Color(0.35f, 0.35f, 0.5f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _mainContent.SetActive(false);
                    _settingsContent.SetActive(true);
                });

            // Quit to Menu
            CreateButton(_mainContent.transform, "QUIT TO MENU", new Vector2(0, -180),
                new Color(0.5f, 0.2f, 0.2f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    GameManager.Instance?.ReturnToTitle();
                });

            // ── Settings content ──
            _settingsContent = new GameObject("SettingsContent");
            _settingsContent.transform.SetParent(canvasGO.transform, false);
            var settingsRect = _settingsContent.AddComponent<RectTransform>();
            settingsRect.anchorMin = Vector2.zero;
            settingsRect.anchorMax = Vector2.one;
            settingsRect.sizeDelta = Vector2.zero;

            // Settings panel background
            var panelGO = new GameObject("SettingsPanel");
            panelGO.transform.SetParent(_settingsContent.transform, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.12f, 0.10f, 0.20f, 0.95f);
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(520, 420);
            panelRect.anchoredPosition = Vector2.zero;

            // ── Settings menu content (main page with category buttons) ──
            _settingsMenuContent = new GameObject("SettingsMenuContent");
            _settingsMenuContent.transform.SetParent(panelGO.transform, false);
            var menuRect = _settingsMenuContent.AddComponent<RectTransform>();
            menuRect.anchorMin = Vector2.zero;
            menuRect.anchorMax = Vector2.one;
            menuRect.sizeDelta = Vector2.zero;

            // Settings title
            CreateText(_settingsMenuContent.transform, "SETTINGS", 36, new Color(1f, 0.85f, 0.1f), new Vector2(0, 170));

            // Back button
            CreateButton(_settingsMenuContent.transform, "BACK", new Vector2(-190, 170),
                new Color(0.4f, 0.4f, 0.5f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _settingsContent.SetActive(false);
                    _mainContent.SetActive(true);
                }, new Vector2(100, 40), 20);

            // Audio button
            CreateButton(_settingsMenuContent.transform, "AUDIO", new Vector2(0, 70),
                new Color(0.3f, 0.4f, 0.55f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _settingsMenuContent.SetActive(false);
                    _audioSubPanel.SetActive(true);
                }, new Vector2(300, 55));

            // Accessibility button (Sprint 9)
            CreateButton(_settingsMenuContent.transform, "ACCESSIBILITY", new Vector2(0, 0),
                new Color(0.3f, 0.45f, 0.5f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _settingsMenuContent.SetActive(false);
                    _accessibilitySubPanel.SetActive(true);
                }, new Vector2(300, 48));

            // Difficulty button (Sprint 9)
            CreateButton(_settingsMenuContent.transform, "DIFFICULTY", new Vector2(0, -60),
                new Color(0.5f, 0.35f, 0.35f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _settingsMenuContent.SetActive(false);
                    _difficultySubPanel.SetActive(true);
                }, new Vector2(300, 48));

            // Hint
            CreateText(_settingsMenuContent.transform, "Press Esc/` to go back", 14,
                new Color(0.5f, 0.5f, 0.6f), new Vector2(0, -170));

            // ── Audio sub-panel ──
            _audioSubPanel = new GameObject("AudioSubPanel");
            _audioSubPanel.transform.SetParent(panelGO.transform, false);
            var audioRect = _audioSubPanel.AddComponent<RectTransform>();
            audioRect.anchorMin = Vector2.zero;
            audioRect.anchorMax = Vector2.one;
            audioRect.sizeDelta = Vector2.zero;

            CreateText(_audioSubPanel.transform, "AUDIO", 36, new Color(1f, 0.85f, 0.1f), new Vector2(0, 170));

            CreateButton(_audioSubPanel.transform, "BACK", new Vector2(-190, 170),
                new Color(0.4f, 0.4f, 0.5f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _audioSubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                }, new Vector2(100, 40), 20);

            // Divider
            var divGO = new GameObject("Divider");
            divGO.transform.SetParent(_audioSubPanel.transform, false);
            var divImg = divGO.AddComponent<Image>();
            divImg.color = new Color(0.3f, 0.3f, 0.4f);
            var divRect = divGO.GetComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.5f, 0.5f);
            divRect.anchorMax = new Vector2(0.5f, 0.5f);
            divRect.sizeDelta = new Vector2(440, 2);
            divRect.anchoredPosition = new Vector2(0, 130);

            // Volume sliders
            float musicVol = AudioManager.Instance != null ? AudioManager.Instance.MusicVolume : 1f;
            float sfxVol = AudioManager.Instance != null ? AudioManager.Instance.SFXVolume : 1f;
            float weaponVol = AudioManager.Instance != null ? AudioManager.Instance.WeaponVolume : 1f;

            CreateVolumeSlider(_audioSubPanel.transform, "BACKGROUND MUSIC", new Vector2(0, 75), musicVol,
                (val) => { if (AudioManager.Instance != null) AudioManager.Instance.MusicVolume = val; });

            CreateVolumeSlider(_audioSubPanel.transform, "SOUND EFFECTS", new Vector2(0, 0), sfxVol,
                (val) => { if (AudioManager.Instance != null) AudioManager.Instance.SFXVolume = val; });

            CreateVolumeSlider(_audioSubPanel.transform, "WEAPON FIRE", new Vector2(0, -75), weaponVol,
                (val) => { if (AudioManager.Instance != null) AudioManager.Instance.WeaponVolume = val; });

            CreateText(_audioSubPanel.transform, "Press Esc/` to go back", 14,
                new Color(0.5f, 0.5f, 0.6f), new Vector2(0, -175));

            _audioSubPanel.SetActive(false);

            // ── Accessibility sub-panel (Sprint 9) ──
            _accessibilitySubPanel = new GameObject("AccessibilitySubPanel");
            _accessibilitySubPanel.transform.SetParent(panelGO.transform, false);
            var accRect = _accessibilitySubPanel.AddComponent<RectTransform>();
            accRect.anchorMin = Vector2.zero;
            accRect.anchorMax = Vector2.one;
            accRect.sizeDelta = Vector2.zero;

            CreateText(_accessibilitySubPanel.transform, "ACCESSIBILITY", 28,
                new Color(1f, 0.85f, 0.1f), new Vector2(0, 170));

            CreateButton(_accessibilitySubPanel.transform, "BACK", new Vector2(-190, 170),
                new Color(0.4f, 0.4f, 0.5f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _accessibilitySubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                }, new Vector2(100, 40), 20);

            // Colorblind mode toggle
            bool cbInitial = AccessibilityManager.Instance != null &&
                AccessibilityManager.Instance.ColorblindMode;
            CreateToggleRow(_accessibilitySubPanel.transform, "COLORBLIND MODE", new Vector2(0, 100), cbInitial,
                (val) => { if (AccessibilityManager.Instance != null) AccessibilityManager.Instance.ColorblindMode = val; });

            // Screen shake intensity slider
            float shakeInitial = AccessibilityManager.Instance != null
                ? AccessibilityManager.Instance.ScreenShakeIntensity : 1f;
            CreateVolumeSlider(_accessibilitySubPanel.transform, "SCREEN SHAKE", new Vector2(0, 40), shakeInitial,
                (val) => { if (AccessibilityManager.Instance != null) AccessibilityManager.Instance.ScreenShakeIntensity = val; });

            // Font size slider (0.8 to 1.5 mapped to 0-1 slider)
            float fontInitial = AccessibilityManager.Instance != null
                ? (AccessibilityManager.Instance.FontSizeScale - 0.8f) / 0.7f : 0.286f;
            CreateVolumeSlider(_accessibilitySubPanel.transform, "FONT SIZE", new Vector2(0, -30), fontInitial,
                (val) => {
                    float scale = 0.8f + val * 0.7f; // Map 0-1 to 0.8-1.5
                    if (AccessibilityManager.Instance != null)
                        AccessibilityManager.Instance.FontSizeScale = scale;
                });

            // High contrast mode toggle
            bool hcInitial = AccessibilityManager.Instance != null &&
                AccessibilityManager.Instance.HighContrastMode;
            CreateToggleRow(_accessibilitySubPanel.transform, "HIGH CONTRAST", new Vector2(0, -100), hcInitial,
                (val) => { if (AccessibilityManager.Instance != null) AccessibilityManager.Instance.HighContrastMode = val; });

            CreateText(_accessibilitySubPanel.transform, "Press Esc/` to go back", 14,
                new Color(0.5f, 0.5f, 0.6f), new Vector2(0, -170));

            _accessibilitySubPanel.SetActive(false);

            // ── Difficulty sub-panel (Sprint 9) ──
            _difficultySubPanel = new GameObject("DifficultySubPanel");
            _difficultySubPanel.transform.SetParent(panelGO.transform, false);
            var diffPanelRect = _difficultySubPanel.AddComponent<RectTransform>();
            diffPanelRect.anchorMin = Vector2.zero;
            diffPanelRect.anchorMax = Vector2.one;
            diffPanelRect.sizeDelta = Vector2.zero;

            CreateText(_difficultySubPanel.transform, "DIFFICULTY", 28,
                new Color(1f, 0.85f, 0.1f), new Vector2(0, 170));

            CreateButton(_difficultySubPanel.transform, "BACK", new Vector2(-190, 170),
                new Color(0.4f, 0.4f, 0.5f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _difficultySubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                }, new Vector2(100, 40), 20);

            // Difficulty description
            CreateText(_difficultySubPanel.transform,
                "Changes apply to new games.\nSeparate leaderboards per difficulty.", 16,
                new Color(0.7f, 0.7f, 0.8f), new Vector2(0, 120));

            // Active difficulty indicator
            var diffActiveGO = CreateText(_difficultySubPanel.transform, "", 18,
                new Color(0.9f, 0.85f, 0.5f), new Vector2(0, -140));
            var diffActiveText = diffActiveGO.GetComponent<Text>();
            diffActiveGO.GetComponent<RectTransform>().sizeDelta = new Vector2(460, 50);

            System.Action updateDifficultyLabel = () => {
                if (DifficultyManager.Instance == null) return;
                var d = DifficultyManager.Instance.CurrentDifficulty;
                string desc = d switch {
                    DifficultyLevel.Easy => "EASY: 3 lives/level, fewer enemies, more health",
                    DifficultyLevel.Hard => "HARD: 1 life/level, more enemies, no health",
                    _ => "NORMAL: 2 lives/level, standard enemies and health"
                };
                diffActiveText.text = desc;
            };
            updateDifficultyLabel();

            // Easy button
            CreateButton(_difficultySubPanel.transform, "EASY", new Vector2(-140, 50),
                new Color(0.25f, 0.50f, 0.30f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    if (DifficultyManager.Instance != null)
                        DifficultyManager.Instance.CurrentDifficulty = DifficultyLevel.Easy;
                    updateDifficultyLabel();
                }, new Vector2(130, 55));

            // Normal button
            CreateButton(_difficultySubPanel.transform, "NORMAL", new Vector2(0, 50),
                new Color(0.35f, 0.40f, 0.55f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    if (DifficultyManager.Instance != null)
                        DifficultyManager.Instance.CurrentDifficulty = DifficultyLevel.Normal;
                    updateDifficultyLabel();
                }, new Vector2(130, 55));

            // Hard button
            CreateButton(_difficultySubPanel.transform, "HARD", new Vector2(140, 50),
                new Color(0.55f, 0.25f, 0.25f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    if (DifficultyManager.Instance != null)
                        DifficultyManager.Instance.CurrentDifficulty = DifficultyLevel.Hard;
                    updateDifficultyLabel();
                }, new Vector2(130, 55));

            // Per-mode descriptions
            CreateText(_difficultySubPanel.transform, "3 deaths/level\n50% enemies\n1.5x health", 13,
                new Color(0.5f, 0.7f, 0.5f), new Vector2(-140, -20));
            CreateText(_difficultySubPanel.transform, "2 deaths/level\nStandard\nStandard", 13,
                new Color(0.6f, 0.6f, 0.8f), new Vector2(0, -20));
            CreateText(_difficultySubPanel.transform, "1 death/level\n1.5x enemies\nNo health", 13,
                new Color(0.8f, 0.5f, 0.5f), new Vector2(140, -20));

            CreateText(_difficultySubPanel.transform, "Press Esc/` to go back", 14,
                new Color(0.5f, 0.5f, 0.6f), new Vector2(0, -170));

            _difficultySubPanel.SetActive(false);

            _settingsContent.SetActive(false);
        }

        private void CreateVolumeSlider(Transform parent, string label, Vector2 position, float initialValue,
            UnityEngine.Events.UnityAction<float> onValueChanged)
        {
            // Container
            var containerGO = new GameObject("VolumeSlider_" + label);
            containerGO.transform.SetParent(parent, false);
            var containerRect = containerGO.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(440, 55);
            containerRect.anchoredPosition = position;

            // Label (pixel text sprite for visibility)
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(containerGO.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(label, new Color(0.85f, 0.85f, 0.95f), 3);
            labelImg.preserveAspect = true;
            labelImg.raycastTarget = false;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.sizeDelta = new Vector2(label.Length * 18 + 12, 28);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(8, 12);

            // Slider background (track) — leave right side for percentage
            var trackGO = new GameObject("Track");
            trackGO.transform.SetParent(containerGO.transform, false);
            var trackImg = trackGO.AddComponent<Image>();
            trackImg.color = new Color(0.15f, 0.13f, 0.25f);
            var trackRect = trackGO.GetComponent<RectTransform>();
            trackRect.anchorMin = new Vector2(0.05f, 0f);
            trackRect.anchorMax = new Vector2(0.82f, 0.45f);
            trackRect.sizeDelta = Vector2.zero;

            // Fill area
            var fillAreaGO = new GameObject("FillArea");
            fillAreaGO.transform.SetParent(trackGO.transform, false);
            var fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = new Vector2(-10, 0);
            fillAreaRect.anchoredPosition = Vector2.zero;

            // Fill
            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(fillAreaGO.transform, false);
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.color = new Color(0.3f, 0.5f, 0.8f);
            var fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;

            // Handle slide area
            var handleAreaGO = new GameObject("HandleArea");
            handleAreaGO.transform.SetParent(trackGO.transform, false);
            var handleAreaRect = handleAreaGO.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.sizeDelta = new Vector2(-14, 0);
            handleAreaRect.anchoredPosition = Vector2.zero;

            // Handle
            var handleGO = new GameObject("Handle");
            handleGO.transform.SetParent(handleAreaGO.transform, false);
            var handleImg = handleGO.AddComponent<Image>();
            handleImg.color = new Color(0.9f, 0.9f, 1f);
            var handleRect = handleGO.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(18, 0);
            handleRect.anchorMin = new Vector2(0, 0);
            handleRect.anchorMax = new Vector2(0, 1);

            // Percentage label — right of the track
            var pctGO = new GameObject("Pct");
            pctGO.transform.SetParent(containerGO.transform, false);
            var pctText = pctGO.AddComponent<Text>();
            pctText.text = Mathf.RoundToInt(initialValue * 100) + "%";
            pctText.fontSize = 18;
            pctText.color = new Color(0.9f, 0.9f, 1f);
            pctText.alignment = TextAnchor.MiddleRight;
            pctText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var pctRect = pctGO.GetComponent<RectTransform>();
            pctRect.anchorMin = new Vector2(1f, 0f);
            pctRect.anchorMax = new Vector2(1f, 0.45f);
            pctRect.pivot = new Vector2(1f, 0.5f);
            pctRect.sizeDelta = new Vector2(70, 0);
            pctRect.anchoredPosition = Vector2.zero;

            // Slider component
            var slider = trackGO.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = initialValue;
            slider.wholeNumbers = false;
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImg;
            slider.onValueChanged.AddListener((val) => {
                onValueChanged.Invoke(val);
                pctText.text = Mathf.RoundToInt(val * 100) + "%";
            });
        }

        private GameObject CreateText(Transform parent, string text, int fontSize, Color color, Vector2 position)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);

            var textComp = go.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = fontSize;
            textComp.color = color;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(400, 60);
            rect.anchoredPosition = position;

            return go;
        }

        private void CreateButton(Transform parent, string text, Vector2 position,
            Color bgColor, UnityEngine.Events.UnityAction onClick,
            Vector2? customSize = null, int customFontSize = 26)
        {
            var size = customSize ?? new Vector2(280, 60);

            var go = new GameObject("Button");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = new Color(
                Mathf.Min(1f, bgColor.r + 0.15f),
                Mathf.Min(1f, bgColor.g + 0.15f),
                Mathf.Min(1f, bgColor.b + 0.15f), bgColor.a);
            colors.pressedColor = new Color(
                bgColor.r * 0.7f, bgColor.g * 0.7f, bgColor.b * 0.7f, bgColor.a);
            colors.selectedColor = colors.normalColor;
            colors.fadeDuration = 0.1f;
            btn.colors = colors;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var textGO = new GameObject("Label");
            textGO.transform.SetParent(go.transform, false);
            var textComp = textGO.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = customFontSize;
            textComp.color = Color.white;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }

        /// <summary>
        /// Creates a toggle row with a label and ON/OFF button. Used for boolean settings.
        /// </summary>
        private void CreateToggleRow(Transform parent, string label, Vector2 position, bool initialValue,
            System.Action<bool> onValueChanged)
        {
            var containerGO = new GameObject("Toggle_" + label);
            containerGO.transform.SetParent(parent, false);
            var containerRect = containerGO.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(440, 40);
            containerRect.anchoredPosition = position;

            // Label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(containerGO.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(label, new Color(0.85f, 0.85f, 0.95f), 3);
            labelImg.preserveAspect = true;
            labelImg.raycastTarget = false;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.sizeDelta = new Vector2(label.Length * 18 + 12, 28);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(8, 0);

            // Toggle button
            bool currentValue = initialValue;
            var btnGO = new GameObject("ToggleBtn");
            btnGO.transform.SetParent(containerGO.transform, false);
            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = currentValue ? new Color(0.3f, 0.55f, 0.35f) : new Color(0.4f, 0.3f, 0.3f);
            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            var btnRect = btnGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1f, 0.5f);
            btnRect.anchorMax = new Vector2(1f, 0.5f);
            btnRect.pivot = new Vector2(1f, 0.5f);
            btnRect.sizeDelta = new Vector2(80, 32);
            btnRect.anchoredPosition = new Vector2(-8, 0);

            var btnTextGO = new GameObject("BtnLabel");
            btnTextGO.transform.SetParent(btnGO.transform, false);
            var btnText = btnTextGO.AddComponent<Text>();
            btnText.text = currentValue ? "ON" : "OFF";
            btnText.fontSize = 18;
            btnText.color = Color.white;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var btnTextRect = btnTextGO.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.sizeDelta = Vector2.zero;

            btn.onClick.AddListener(() => {
                currentValue = !currentValue;
                btnText.text = currentValue ? "ON" : "OFF";
                btnImg.color = currentValue ? new Color(0.3f, 0.55f, 0.35f) : new Color(0.4f, 0.3f, 0.3f);
                onValueChanged(currentValue);
            });
        }
    }
}
