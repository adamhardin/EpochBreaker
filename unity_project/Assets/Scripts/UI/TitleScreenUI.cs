using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using EpochBreaker.Generative;
using EpochBreaker.Gameplay;

namespace EpochBreaker.UI
{
    /// <summary>
    /// Title screen with game logo, main menu, legend, and dev-only level selector.
    /// Creates its own Canvas and UI elements programmatically.
    /// </summary>
    public class TitleScreenUI : MonoBehaviour
    {
        private Canvas _canvas;
        private GameObject _levelSelectorPanel;
        private GameObject _levelHistoryPanel;
        private GameObject _enterCodePanel;
        private GameObject _settingsPanel;
        private GameObject _settingsMenuContent;
        private GameObject _audioSubPanel;
        private GameObject _historyContentParent;
        private InputField _codeInputField;
        private Text _codeErrorText;

        private static readonly string[] EpochNames = {
            "Stone Age", "Bronze Age", "Classical", "Medieval", "Renaissance",
            "Industrial", "Modern", "Digital", "Spacefaring", "Transcendent"
        };

        private void Start()
        {
            CreateUI();
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            // Submit code with Enter when code panel is open
            if (_enterCodePanel != null && _enterCodePanel.activeSelf)
            {
                if (Keyboard.current.enterKey.wasPressedThisFrame ||
                    Keyboard.current.numpadEnterKey.wasPressedThisFrame)
                    SubmitLevelCode();
                else if (Keyboard.current.escapeKey.wasPressedThisFrame)
                    _enterCodePanel.SetActive(false);
                return;
            }

            // Close overlays with Escape
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                // Audio sub-panel → back to settings menu
                if (_audioSubPanel != null && _audioSubPanel.activeSelf)
                {
                    _audioSubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                }
                else if (_settingsPanel != null && _settingsPanel.activeSelf)
                    _settingsPanel.SetActive(false);
                else if (_levelSelectorPanel != null && _levelSelectorPanel.activeSelf)
                    _levelSelectorPanel.SetActive(false);
                else if (_levelHistoryPanel != null && _levelHistoryPanel.activeSelf)
                    _levelHistoryPanel.SetActive(false);
            }
        }

        private void CreateUI()
        {
            // Canvas
            var canvasGO = new GameObject("TitleCanvas");
            canvasGO.transform.SetParent(transform);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Background
            var bgGO = CreatePanel(canvasGO.transform, "Background", new Color(0.08f, 0.06f, 0.15f, 1f));
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // Ornate pixel art frame
            var frameGO = new GameObject("OrnateFrame");
            frameGO.transform.SetParent(canvasGO.transform, false);
            var frameImg = frameGO.AddComponent<Image>();
            frameImg.sprite = PlaceholderAssets.GetOrnateFrameSprite();
            frameImg.preserveAspect = false;
            var frameRect = frameGO.GetComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.sizeDelta = Vector2.zero;

            // Title logo (pixel art sprite - single line, large)
            var titleGO = new GameObject("TitleLogo");
            titleGO.transform.SetParent(canvasGO.transform, false);
            var titleImg = titleGO.AddComponent<Image>();
            titleImg.sprite = PlaceholderAssets.GetTitleLogoSprite();
            titleImg.preserveAspect = true;
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(900, 120);
            titleRect.anchoredPosition = new Vector2(0, 340);

            // Subtitle description (pixel text at scale 3 for clarity)
            var sub1GO = new GameObject("Subtitle1");
            sub1GO.transform.SetParent(canvasGO.transform, false);
            var sub1Img = sub1GO.AddComponent<Image>();
            sub1Img.sprite = PlaceholderAssets.GetPixelTextSprite(
                "A HERO CHARGES THROUGH HUMANITY'S TIMELINE!",
                new Color(0.7f, 0.65f, 0.85f), 3);
            sub1Img.preserveAspect = true;
            var sub1Rect = sub1GO.GetComponent<RectTransform>();
            sub1Rect.anchorMin = new Vector2(0.5f, 0.5f);
            sub1Rect.anchorMax = new Vector2(0.5f, 0.5f);
            sub1Rect.sizeDelta = new Vector2(900, 34);
            sub1Rect.anchoredPosition = new Vector2(0, 228);

            var sub2GO = new GameObject("Subtitle2");
            sub2GO.transform.SetParent(canvasGO.transform, false);
            var sub2Img = sub2GO.AddComponent<Image>();
            sub2Img.sprite = PlaceholderAssets.GetPixelTextSprite(
                "EACH PATHWAY PROCEDURALLY GENERATED BY A SHARABLE CODE.",
                new Color(0.7f, 0.65f, 0.85f), 3);
            sub2Img.preserveAspect = true;
            var sub2Rect = sub2GO.GetComponent<RectTransform>();
            sub2Rect.anchorMin = new Vector2(0.5f, 0.5f);
            sub2Rect.anchorMax = new Vector2(0.5f, 0.5f);
            sub2Rect.sizeDelta = new Vector2(900, 34);
            sub2Rect.anchoredPosition = new Vector2(0, 198);

            // Main menu buttons
            CreateMainMenu(canvasGO.transform);

            // Legend panel
            CreateLegend(canvasGO.transform);

            // Controls hint
            var hintGO = CreateText(canvasGO.transform,
                "A/D: Move | Space: Jump | Down: Ground Pound | Esc: Pause", 16,
                new Color(0.5f, 0.5f, 0.6f));
            var hintRect = hintGO.GetComponent<RectTransform>();
            hintRect.anchoredPosition = new Vector2(0, -450);

            // Version
            var verGO = CreateText(canvasGO.transform, "v0.1.0 - Debug Build", 14,
                new Color(0.4f, 0.4f, 0.5f));
            var verRect = verGO.GetComponent<RectTransform>();
            verRect.anchorMin = new Vector2(0, 0);
            verRect.anchorMax = new Vector2(0, 0);
            verRect.pivot = new Vector2(0, 0);
            verRect.anchoredPosition = new Vector2(20, 20);

            // Enter code overlay (initially hidden)
            CreateEnterCodeOverlay(canvasGO.transform);

            // Settings overlay (initially hidden)
            CreateSettingsOverlay(canvasGO.transform);

            // Level selector overlay (initially hidden)
            CreateLevelSelectorOverlay(canvasGO.transform);

            // Level history overlay (initially hidden)
            CreateLevelHistoryOverlay(canvasGO.transform);
        }

        private void CreateMainMenu(Transform parent)
        {
            // Play Game button (primary)
            CreateMenuButton(parent, "PLAY GAME", new Vector2(0, 120),
                new Vector2(260, 55), new Color(0.2f, 0.55f, 0.3f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    GameManager.Instance?.StartGame();
                });

            // Enter Code button
            CreateMenuButton(parent, "ENTER CODE", new Vector2(0, 58),
                new Vector2(260, 42), new Color(0.4f, 0.5f, 0.55f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    if (_enterCodePanel != null)
                    {
                        _codeInputField.text = "";
                        _codeErrorText.gameObject.SetActive(false);
                        _enterCodePanel.SetActive(true);
                        _codeInputField.ActivateInputField();
                    }
                });

            // Level History button
            CreateMenuButton(parent, "LEVEL HISTORY", new Vector2(0, 8),
                new Vector2(260, 42), new Color(0.35f, 0.45f, 0.55f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    if (_levelHistoryPanel != null)
                    {
                        RefreshHistoryContent();
                        _levelHistoryPanel.SetActive(true);
                    }
                });

            // Settings button
            CreateMenuButton(parent, "SETTINGS", new Vector2(0, -42),
                new Vector2(260, 42), new Color(0.4f, 0.4f, 0.48f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    if (_settingsPanel != null)
                        _settingsPanel.SetActive(true);
                });

        }

        private void CreateLegend(Transform parent)
        {
            // Legend panel
            var panelGO = new GameObject("Legend");
            panelGO.transform.SetParent(parent, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.10f, 0.08f, 0.18f, 0.85f);
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(800, 280);
            panelRect.anchoredPosition = new Vector2(0, -240);

            // Row 1: Rewards
            float row1Y = 95f;
            CreateLegendLabel(panelGO.transform, "Rewards:", new Vector2(-350, row1Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetRewardSprite(RewardType.HealthSmall),
                "Health", new Vector2(-240, row1Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetRewardSprite(RewardType.AttackBoost),
                "Attack", new Vector2(-130, row1Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetRewardSprite(RewardType.SpeedBoost),
                "Speed", new Vector2(-20, row1Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetRewardSprite(RewardType.Shield),
                "Shield", new Vector2(90, row1Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetRewardSprite(RewardType.Coin),
                "Coin", new Vector2(200, row1Y));

            // Row 2: Weapons
            float row2Y = 0f;
            CreateLegendLabel(panelGO.transform, "Weapons:", new Vector2(-350, row2Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetWeaponPickupSprite(WeaponTier.Starting),
                "Sword", new Vector2(-240, row2Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetWeaponPickupSprite(WeaponTier.Medium),
                "Crossbow", new Vector2(-130, row2Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetWeaponPickupSprite(WeaponTier.Heavy),
                "Cannon", new Vector2(-20, row2Y));

            // Row 3: Checkpoints & Goal
            float row3Y = -95f;
            CreateLegendLabel(panelGO.transform, "Progress:", new Vector2(-350, row3Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetCheckpointSprite(CheckpointType.LevelStart),
                "Start", new Vector2(-240, row3Y), 48f);
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetCheckpointSprite(CheckpointType.MidLevel),
                "Mid", new Vector2(-130, row3Y), 48f);
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetCheckpointSprite(CheckpointType.PreBoss),
                "Boss", new Vector2(-20, row3Y), 48f);
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetGoalSprite(),
                "Goal", new Vector2(110, row3Y), 48f);
        }

        private void CreateLegendLabel(Transform parent, string text, Vector2 position)
        {
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(parent, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(text.Replace(":", ""), new Color(0.8f, 0.8f, 0.9f), 2);
            labelImg.preserveAspect = true;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(100, 24);
            labelRect.anchoredPosition = position;
        }

        private void CreateLegendItem(Transform parent, Sprite sprite, string label, Vector2 position, float iconSize = 40f)
        {
            // Container
            var containerGO = new GameObject("LegendItem");
            containerGO.transform.SetParent(parent, false);
            var containerRect = containerGO.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(80, 60);
            containerRect.anchoredPosition = position;

            // Icon
            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(containerGO.transform, false);
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.sprite = sprite;
            iconImg.preserveAspect = true;
            var iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(iconSize, iconSize);
            iconRect.anchoredPosition = new Vector2(0, 8);

            // Label (pixel text)
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(containerGO.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(label, new Color(0.75f, 0.75f, 0.85f), 2);
            labelImg.preserveAspect = true;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0f);
            labelRect.anchorMax = new Vector2(0.5f, 0f);
            labelRect.sizeDelta = new Vector2(80, 20);
            labelRect.anchoredPosition = new Vector2(0, 5);
        }

        private void CreateEnterCodeOverlay(Transform parent)
        {
            // Overlay background
            _enterCodePanel = new GameObject("EnterCodeOverlay");
            _enterCodePanel.transform.SetParent(parent, false);
            var overlayImg = _enterCodePanel.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.85f);
            var overlayRect = _enterCodePanel.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            // Panel
            var panelGO = new GameObject("EnterCodePanel");
            panelGO.transform.SetParent(_enterCodePanel.transform, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.12f, 0.10f, 0.20f, 0.98f);
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(520, 340);
            panelRect.anchoredPosition = Vector2.zero;

            // Title
            var titleGO = CreateText(panelGO.transform, "ENTER LEVEL CODE", 32, new Color(1f, 0.85f, 0.1f));
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 130);

            // Format hint
            var hintGO = CreateText(panelGO.transform, "Format: E-XXXXXXXX  (e.g. 3-K7XM2P9A)", 16,
                new Color(0.6f, 0.6f, 0.7f));
            var hintRect = hintGO.GetComponent<RectTransform>();
            hintRect.anchoredPosition = new Vector2(0, 90);

            // Close button
            CreateMenuButton(panelGO.transform, "X", new Vector2(230, 130),
                new Vector2(40, 40), new Color(0.6f, 0.25f, 0.25f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _enterCodePanel.SetActive(false);
                });

            // InputField background
            var inputBgGO = new GameObject("InputFieldBg");
            inputBgGO.transform.SetParent(panelGO.transform, false);
            var inputBgImg = inputBgGO.AddComponent<Image>();
            inputBgImg.color = new Color(0.06f, 0.05f, 0.12f, 1f);
            var inputBgRect = inputBgGO.GetComponent<RectTransform>();
            inputBgRect.anchorMin = new Vector2(0.5f, 0.5f);
            inputBgRect.anchorMax = new Vector2(0.5f, 0.5f);
            inputBgRect.sizeDelta = new Vector2(360, 55);
            inputBgRect.anchoredPosition = new Vector2(0, 30);

            // InputField text
            var inputTextGO = new GameObject("Text");
            inputTextGO.transform.SetParent(inputBgGO.transform, false);
            var inputText = inputTextGO.AddComponent<Text>();
            inputText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            inputText.fontSize = 28;
            inputText.color = Color.white;
            inputText.alignment = TextAnchor.MiddleCenter;
            inputText.supportRichText = false;
            var inputTextRect = inputTextGO.GetComponent<RectTransform>();
            inputTextRect.anchorMin = new Vector2(0.05f, 0f);
            inputTextRect.anchorMax = new Vector2(0.95f, 1f);
            inputTextRect.sizeDelta = Vector2.zero;

            // Placeholder text
            var placeholderGO = new GameObject("Placeholder");
            placeholderGO.transform.SetParent(inputBgGO.transform, false);
            var placeholderText = placeholderGO.AddComponent<Text>();
            placeholderText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            placeholderText.fontSize = 28;
            placeholderText.color = new Color(0.35f, 0.35f, 0.45f);
            placeholderText.alignment = TextAnchor.MiddleCenter;
            placeholderText.fontStyle = FontStyle.Italic;
            placeholderText.text = "0-00000000";
            var placeholderRect = placeholderGO.GetComponent<RectTransform>();
            placeholderRect.anchorMin = new Vector2(0.05f, 0f);
            placeholderRect.anchorMax = new Vector2(0.95f, 1f);
            placeholderRect.sizeDelta = Vector2.zero;

            // InputField component
            _codeInputField = inputBgGO.AddComponent<InputField>();
            _codeInputField.textComponent = inputText;
            _codeInputField.placeholder = placeholderText;
            _codeInputField.characterLimit = 10;
            _codeInputField.contentType = InputField.ContentType.Custom;
            _codeInputField.characterValidation = InputField.CharacterValidation.None;
            _codeInputField.onValidateInput += (text, index, ch) =>
            {
                // Allow digits, uppercase letters, and hyphen
                ch = char.ToUpper(ch);
                if (char.IsDigit(ch) || (ch >= 'A' && ch <= 'Z') || ch == '-')
                    return ch;
                return '\0';
            };

            // Error text (hidden by default)
            var errorGO = CreateText(panelGO.transform, "Invalid level code", 18, new Color(1f, 0.3f, 0.3f));
            var errorRect = errorGO.GetComponent<RectTransform>();
            errorRect.anchoredPosition = new Vector2(0, -15);
            _codeErrorText = errorGO.GetComponent<Text>();
            errorGO.SetActive(false);

            // Play button
            CreateMenuButton(panelGO.transform, "PLAY", new Vector2(0, -60),
                new Vector2(200, 55), new Color(0.2f, 0.55f, 0.3f), () => {
                    SubmitLevelCode();
                });

#if !UNITY_WEBGL || UNITY_EDITOR
            // Paste from clipboard button (not available on WebGL — browser security)
            CreateMenuButton(panelGO.transform, "PASTE", new Vector2(210, 30),
                new Vector2(80, 40), new Color(0.35f, 0.4f, 0.5f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    string clipboard = GUIUtility.systemCopyBuffer;
                    if (!string.IsNullOrEmpty(clipboard))
                    {
                        // Trim to 10 chars max (level code length)
                        if (clipboard.Length > 10) clipboard = clipboard.Substring(0, 10);
                        _codeInputField.text = clipboard.ToUpper();
                    }
                });
#endif

            // Escape hint
            var escGO = CreateText(panelGO.transform, "Press Esc to close", 14,
                new Color(0.5f, 0.5f, 0.6f));
            var escRect = escGO.GetComponent<RectTransform>();
            escRect.anchoredPosition = new Vector2(0, -130);

            _enterCodePanel.SetActive(false);
        }

        private void SubmitLevelCode()
        {
            string code = _codeInputField.text.Trim().ToUpper();
            if (string.IsNullOrEmpty(code))
            {
                _codeErrorText.text = "Please enter a level code";
                _codeErrorText.gameObject.SetActive(true);
                return;
            }

            if (!LevelID.TryParse(code, out _))
            {
                _codeErrorText.text = $"Invalid code: {code}";
                _codeErrorText.gameObject.SetActive(true);
                return;
            }

            AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
            _enterCodePanel.SetActive(false);
            GameManager.Instance?.StartLevelFromCode(code);
        }

        private void CreateSettingsOverlay(Transform parent)
        {
            // Overlay background
            _settingsPanel = new GameObject("SettingsOverlay");
            _settingsPanel.transform.SetParent(parent, false);
            var overlayImg = _settingsPanel.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.85f);
            var overlayRect = _settingsPanel.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            // Panel
            var panelGO = new GameObject("SettingsPanel");
            panelGO.transform.SetParent(_settingsPanel.transform, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.12f, 0.10f, 0.20f, 0.98f);
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(520, 420);
            panelRect.anchoredPosition = Vector2.zero;

            // ── Settings menu content (main page) ──
            _settingsMenuContent = new GameObject("SettingsMenuContent");
            _settingsMenuContent.transform.SetParent(panelGO.transform, false);
            var menuRect = _settingsMenuContent.AddComponent<RectTransform>();
            menuRect.anchorMin = Vector2.zero;
            menuRect.anchorMax = Vector2.one;
            menuRect.sizeDelta = Vector2.zero;

            // Title
            var titleGO = CreateText(_settingsMenuContent.transform, "SETTINGS", 32, new Color(1f, 0.85f, 0.1f));
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 170);

            // Close button
            CreateMenuButton(_settingsMenuContent.transform, "X", new Vector2(230, 170),
                new Vector2(40, 40), new Color(0.6f, 0.25f, 0.25f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _settingsPanel.SetActive(false);
                });

            // Audio button
            CreateMenuButton(_settingsMenuContent.transform, "AUDIO", new Vector2(0, 40),
                new Vector2(300, 55), new Color(0.3f, 0.4f, 0.55f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _settingsMenuContent.SetActive(false);
                    _audioSubPanel.SetActive(true);
                });

            // Level Select [DEV] button
            CreateMenuButton(_settingsMenuContent.transform, "LEVEL SELECT [DEV]", new Vector2(0, -35),
                new Vector2(300, 55), new Color(0.4f, 0.35f, 0.5f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _settingsPanel.SetActive(false);
                    if (_levelSelectorPanel != null)
                        _levelSelectorPanel.SetActive(true);
                });

            // Escape hint
            var escGO = CreateText(_settingsMenuContent.transform, "Press Esc to close", 14,
                new Color(0.5f, 0.5f, 0.6f));
            var escRect = escGO.GetComponent<RectTransform>();
            escRect.anchoredPosition = new Vector2(0, -175);

            // ── Audio sub-panel ──
            _audioSubPanel = new GameObject("AudioSubPanel");
            _audioSubPanel.transform.SetParent(panelGO.transform, false);
            var audioRect = _audioSubPanel.AddComponent<RectTransform>();
            audioRect.anchorMin = Vector2.zero;
            audioRect.anchorMax = Vector2.one;
            audioRect.sizeDelta = Vector2.zero;

            // Audio title
            var audioTitleGO = CreateText(_audioSubPanel.transform, "AUDIO", 32, new Color(1f, 0.85f, 0.1f));
            var audioTitleRect = audioTitleGO.GetComponent<RectTransform>();
            audioTitleRect.anchoredPosition = new Vector2(0, 170);

            // Back button
            CreateMenuButton(_audioSubPanel.transform, "BACK", new Vector2(-200, 170),
                new Vector2(80, 40), new Color(0.4f, 0.4f, 0.5f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _audioSubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                });

            // Close button
            CreateMenuButton(_audioSubPanel.transform, "X", new Vector2(230, 170),
                new Vector2(40, 40), new Color(0.6f, 0.25f, 0.25f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _audioSubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                    _settingsPanel.SetActive(false);
                });

            // Divider line
            var divGO = new GameObject("Divider");
            divGO.transform.SetParent(_audioSubPanel.transform, false);
            var divImg = divGO.AddComponent<Image>();
            divImg.color = new Color(0.3f, 0.3f, 0.4f);
            var divRect = divGO.GetComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.5f, 0.5f);
            divRect.anchorMax = new Vector2(0.5f, 0.5f);
            divRect.sizeDelta = new Vector2(440, 2);
            divRect.anchoredPosition = new Vector2(0, 130);

            // Get current volumes from AudioManager
            float musicVol = AudioManager.Instance != null ? AudioManager.Instance.MusicVolume : 1f;
            float sfxVol = AudioManager.Instance != null ? AudioManager.Instance.SFXVolume : 1f;
            float weaponVol = AudioManager.Instance != null ? AudioManager.Instance.WeaponVolume : 1f;

            // Background Music slider
            CreateVolumeSlider(_audioSubPanel.transform, "BACKGROUND MUSIC", new Vector2(0, 75), musicVol,
                (val) => { if (AudioManager.Instance != null) AudioManager.Instance.MusicVolume = val; });

            // Sound Effects slider
            CreateVolumeSlider(_audioSubPanel.transform, "SOUND EFFECTS", new Vector2(0, 0), sfxVol,
                (val) => { if (AudioManager.Instance != null) AudioManager.Instance.SFXVolume = val; });

            // Weapon Fire slider
            CreateVolumeSlider(_audioSubPanel.transform, "WEAPON FIRE", new Vector2(0, -75), weaponVol,
                (val) => { if (AudioManager.Instance != null) AudioManager.Instance.WeaponVolume = val; });

            // Escape hint
            var audioEscGO = CreateText(_audioSubPanel.transform, "Press Esc to go back", 14,
                new Color(0.5f, 0.5f, 0.6f));
            var audioEscRect = audioEscGO.GetComponent<RectTransform>();
            audioEscRect.anchoredPosition = new Vector2(0, -175);

            _audioSubPanel.SetActive(false);
            _settingsPanel.SetActive(false);
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
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(label, new Color(0.85f, 0.85f, 0.95f), 2);
            labelImg.preserveAspect = true;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.sizeDelta = new Vector2(label.Length * 12 + 12, 22);
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
            var pctGO = CreateText(containerGO.transform, Mathf.RoundToInt(initialValue * 100) + "%", 18,
                new Color(0.9f, 0.9f, 1f));
            var pctRect = pctGO.GetComponent<RectTransform>();
            pctRect.anchorMin = new Vector2(1f, 0f);
            pctRect.anchorMax = new Vector2(1f, 0.45f);
            pctRect.pivot = new Vector2(1f, 0.5f);
            pctRect.sizeDelta = new Vector2(70, 0);
            pctRect.anchoredPosition = new Vector2(0, 0);
            var pctText = pctGO.GetComponent<Text>();
            pctText.alignment = TextAnchor.MiddleRight;

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

        private void CreateLevelSelectorOverlay(Transform parent)
        {
            // Overlay background (darkens screen)
            _levelSelectorPanel = new GameObject("LevelSelectorOverlay");
            _levelSelectorPanel.transform.SetParent(parent, false);
            var overlayImg = _levelSelectorPanel.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.85f);
            var overlayRect = _levelSelectorPanel.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            // Level selector panel
            var panelGO = new GameObject("LevelSelector");
            panelGO.transform.SetParent(_levelSelectorPanel.transform, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.12f, 0.10f, 0.20f, 0.98f);
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(600, 700);
            panelRect.anchoredPosition = Vector2.zero;

            // Title
            var titleGO = CreateText(panelGO.transform, "EPOCH SELECTOR", 32, new Color(1f, 0.85f, 0.1f));
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 310);

            // Subtitle
            var subGO = CreateText(panelGO.transform, "Select an epoch to generate a random level", 16,
                new Color(0.6f, 0.5f, 0.7f));
            var subRect = subGO.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(0, 275);

            // Close button
            CreateMenuButton(panelGO.transform, "X", new Vector2(270, 310),
                new Vector2(40, 40), new Color(0.6f, 0.25f, 0.25f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _levelSelectorPanel.SetActive(false);
                });

            // Epoch buttons - two columns of 5
            float rowHeight = 50f;
            float gridStartY = 210f;
            float colWidth = 240f;

            for (int epoch = 0; epoch < 10; epoch++)
            {
                int col = epoch / 5;
                int row = epoch % 5;
                float x = (col == 0) ? -130f : 130f;
                float y = gridStartY - row * rowHeight;

                int capturedEpoch = epoch;
                CreateMenuButton(panelGO.transform,
                    $"{epoch}: {EpochNames[epoch]}",
                    new Vector2(x, y),
                    new Vector2(colWidth, 42),
                    GetEpochColor(epoch),
                    () => {
                        AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                        GameManager.Instance?.StartTestLevel(capturedEpoch);
                    });
            }

            // Random level button
            CreateMenuButton(panelGO.transform, "RANDOM EPOCH", new Vector2(0, -80),
                new Vector2(200, 50), new Color(0.3f, 0.45f, 0.6f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    int randEpoch = Random.Range(0, 10);
                    GameManager.Instance?.StartTestLevel(randEpoch);
                });

            // Hint text
            var hintGO = CreateText(panelGO.transform, "Press Esc to close", 14,
                new Color(0.5f, 0.5f, 0.6f));
            var hintRect = hintGO.GetComponent<RectTransform>();
            hintRect.anchoredPosition = new Vector2(0, -310);

            _levelSelectorPanel.SetActive(false);
        }

        private void CreateLevelHistoryOverlay(Transform parent)
        {
            // Overlay background (darkens screen)
            _levelHistoryPanel = new GameObject("LevelHistoryOverlay");
            _levelHistoryPanel.transform.SetParent(parent, false);
            var overlayImg = _levelHistoryPanel.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.85f);
            var overlayRect = _levelHistoryPanel.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            // History panel
            var panelGO = new GameObject("HistoryPanel");
            panelGO.transform.SetParent(_levelHistoryPanel.transform, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.12f, 0.10f, 0.20f, 0.98f);
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(700, 700);
            panelRect.anchoredPosition = Vector2.zero;

            // Title
            var titleGO = CreateText(panelGO.transform, "LEVEL HISTORY", 32, new Color(1f, 0.85f, 0.1f));
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 310);

            // Subtitle
            var subGO = CreateText(panelGO.transform, "Tap code to copy to clipboard", 16,
                new Color(0.6f, 0.5f, 0.7f));
            var subRect = subGO.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(0, 275);

            // Close button
            CreateMenuButton(panelGO.transform, "X", new Vector2(320, 310),
                new Vector2(40, 40), new Color(0.6f, 0.25f, 0.25f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _levelHistoryPanel.SetActive(false);
                });

            // Content area for history entries (scrollable area)
            var contentGO = new GameObject("HistoryContent");
            contentGO.transform.SetParent(panelGO.transform, false);
            var contentRect = contentGO.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax = new Vector2(0.5f, 0.5f);
            contentRect.sizeDelta = new Vector2(650, 480);
            contentRect.anchoredPosition = new Vector2(0, -20);
            _historyContentParent = contentGO;

            // Clear history button
            CreateMenuButton(panelGO.transform, "CLEAR HISTORY", new Vector2(0, -300),
                new Vector2(180, 40), new Color(0.5f, 0.3f, 0.3f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    GameManager.ClearLevelHistory();
                    RefreshHistoryContent();
                });

            // Hint text
            var hintGO = CreateText(panelGO.transform, "Press Esc to close", 14,
                new Color(0.5f, 0.5f, 0.6f));
            var hintRect = hintGO.GetComponent<RectTransform>();
            hintRect.anchoredPosition = new Vector2(0, -330);

            _levelHistoryPanel.SetActive(false);
        }

        private void RefreshHistoryContent()
        {
            if (_historyContentParent == null) return;

            // Clear existing content
            foreach (Transform child in _historyContentParent.transform)
            {
                Destroy(child.gameObject);
            }

            var history = GameManager.LoadLevelHistory();

            if (history.Entries.Count == 0)
            {
                var noHistoryGO = CreateText(_historyContentParent.transform, "No levels played yet", 20,
                    new Color(0.5f, 0.5f, 0.6f));
                var noHistoryRect = noHistoryGO.GetComponent<RectTransform>();
                noHistoryRect.anchoredPosition = new Vector2(0, 100);
                return;
            }

            // Column headers
            float headerY = 220f;
            CreateHistoryHeader(_historyContentParent.transform, "CODE", new Vector2(-240, headerY), 140);
            CreateHistoryHeader(_historyContentParent.transform, "EPOCH", new Vector2(-70, headerY), 160);
            CreateHistoryHeader(_historyContentParent.transform, "SCORE", new Vector2(100, headerY), 100);
            CreateHistoryHeader(_historyContentParent.transform, "STARS", new Vector2(200, headerY), 80);
            CreateHistoryHeader(_historyContentParent.transform, "COPY", new Vector2(280, headerY), 80);

            // Display up to 10 entries (scrolling would require ScrollRect setup)
            float rowY = 180f;
            float rowHeight = 42f;
            int maxDisplay = Mathf.Min(history.Entries.Count, 10);

            for (int i = 0; i < maxDisplay; i++)
            {
                var entry = history.Entries[i];
                CreateHistoryRow(_historyContentParent.transform, entry, rowY);
                rowY -= rowHeight;
            }

            if (history.Entries.Count > 10)
            {
                var moreGO = CreateText(_historyContentParent.transform,
                    $"... and {history.Entries.Count - 10} more", 14,
                    new Color(0.5f, 0.5f, 0.6f));
                var moreRect = moreGO.GetComponent<RectTransform>();
                moreRect.anchoredPosition = new Vector2(0, rowY - 10);
            }
        }

        private void CreateHistoryHeader(Transform parent, string text, Vector2 position, float width)
        {
            var go = CreateText(parent, text, 16, new Color(0.7f, 0.7f, 0.8f));
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(width, 24);
        }

        private void CreateHistoryRow(Transform parent, LevelHistoryEntry entry, float y)
        {
            // Row background
            var rowGO = new GameObject("HistoryRow");
            rowGO.transform.SetParent(parent, false);
            var rowImg = rowGO.AddComponent<Image>();
            rowImg.color = new Color(0.15f, 0.12f, 0.25f, 0.6f);
            var rowRect = rowGO.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 0.5f);
            rowRect.anchorMax = new Vector2(0.5f, 0.5f);
            rowRect.sizeDelta = new Vector2(620, 38);
            rowRect.anchoredPosition = new Vector2(0, y);

            // Code (clickable to copy)
            var codeGO = CreateText(rowGO.transform, entry.Code, 18, new Color(0.5f, 0.8f, 1f));
            var codeRect = codeGO.GetComponent<RectTransform>();
            codeRect.anchoredPosition = new Vector2(-240, 0);
            codeRect.sizeDelta = new Vector2(140, 30);

            // Epoch name
            string epochShort = entry.EpochName.Length > 12 ? entry.EpochName.Substring(0, 10) + ".." : entry.EpochName;
            var epochGO = CreateText(rowGO.transform, epochShort, 16, GetEpochColor(entry.Epoch) * 2f);
            var epochRect = epochGO.GetComponent<RectTransform>();
            epochRect.anchoredPosition = new Vector2(-70, 0);
            epochRect.sizeDelta = new Vector2(160, 30);

            // Score
            var scoreGO = CreateText(rowGO.transform, entry.Score.ToString("N0"), 16, Color.white);
            var scoreRect = scoreGO.GetComponent<RectTransform>();
            scoreRect.anchoredPosition = new Vector2(100, 0);
            scoreRect.sizeDelta = new Vector2(100, 30);

            // Stars
            string stars = new string('*', entry.Stars) + new string('-', 3 - entry.Stars);
            var starsGO = CreateText(rowGO.transform, stars, 18, new Color(1f, 0.85f, 0.1f));
            var starsRect = starsGO.GetComponent<RectTransform>();
            starsRect.anchoredPosition = new Vector2(200, 0);
            starsRect.sizeDelta = new Vector2(80, 30);

            // Copy button
            string capturedCode = entry.Code;
            CreateSmallButton(rowGO.transform, "COPY", new Vector2(280, 0), new Vector2(60, 28),
                new Color(0.3f, 0.5f, 0.4f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    GameManager.CopyToClipboard(capturedCode);
                });
        }

        private void CreateSmallButton(Transform parent, string text, Vector2 position, Vector2 size,
            Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("SmallBtn");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            // Text label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelText = labelGO.AddComponent<Text>();
            labelText.text = text;
            labelText.fontSize = 12;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
        }

        private Color GetEpochColor(int epoch)
        {
            float t = epoch / 9f;
            Color baseColor = Color.Lerp(
                new Color(0.9f, 0.7f, 0.4f),
                new Color(0.5f, 0.7f, 1.0f),
                t
            );
            return new Color(baseColor.r * 0.5f, baseColor.g * 0.5f, baseColor.b * 0.5f, 1f);
        }

        private GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private GameObject CreateText(Transform parent, string text, int fontSize, Color color)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);

            var textComp = go.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = fontSize;
            textComp.color = color;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.horizontalOverflow = HorizontalWrapMode.Overflow;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(800, fontSize + 20);

            return go;
        }

        private void CreateMenuButton(Transform parent, string text, Vector2 position, Vector2 size,
            Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("MenuBtn");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            // Pixel text label
            int scale = size.y > 55 ? 3 : 2;
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(text, Color.white, scale);
            labelImg.preserveAspect = true;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(size.x - 16, size.y - 12);
            labelRect.anchoredPosition = Vector2.zero;
        }

        private void CreateLevelButton(Transform parent, string text, Vector2 position, Vector2 size,
            Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("LevelBtn");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            // Pixel text label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(text, Color.white, 2);
            labelImg.preserveAspect = true;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(size.x - 8, size.y - 8);
            labelRect.anchoredPosition = Vector2.zero;
        }

        private void CreatePixelLabel(Transform parent, string text, Vector2 position, Color color, int scale = 2)
        {
            var go = new GameObject("PixelLabel");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = PlaceholderAssets.GetPixelTextSprite(text, color, scale);
            img.preserveAspect = true;
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(text.Length * 6 * scale + scale * 4, 7 * scale + scale * 4);
            rect.anchoredPosition = position;
        }
    }
}
