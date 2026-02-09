using System.Collections.Generic;
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
        private GameObject _canvasGO;
        private GameObject _levelSelectorPanel;
        private GameObject _levelHistoryPanel;
        private GameObject _enterCodePanel;
        private GameObject _settingsPanel;
        private GameObject _settingsMenuContent;
        private GameObject _audioSubPanel;
        private GameObject _aboutSubPanel;
        private GameObject _accessibilitySubPanel;
        private GameObject _difficultySubPanel;
        private GameObject _historyContentParent;
        private int _historyPage = 0;
        private Text _historyPageLabel;
        private const int HISTORY_PAGE_SIZE = 10;
        private GameObject _legendsObj;
        private GameObject _achievementsObj;
        private GameObject _challengeEntryObj;
        private GameObject _cosmeticsObj;
        private InputField _codeInputField;
        private Text _codeErrorText;
        private GameObject _confirmNewGamePanel;
        private GameObject _difficultyPromptPanel;
        private GameObject _devPanel;
        private bool _godModeEnabled;
        private GameObject _godModeCheckmark;
        private Image _playerPreviewImg;

        // ── Layout Constants ──
        // Ornate frame border is 32px at 960×540 (PPU 0.5) → ~64px at 1920×1080 ref.
        // FRAME_INSET is the safe margin from screen edges to avoid overlapping the frame.
        private const float FRAME_INSET = 72f;
        private const float ITEM_SPACING = 55f;       // Standard gap between menu items
        private const float SECTION_GAP = 70f;         // Larger gap between distinct sections
        private const float BTN_HEIGHT = 48f;           // Standard menu button height
        private const float SMALL_BTN_H = 36f;          // Small/secondary button height
        private const float DEV_BTN_W = 80f;
        private const float DEV_BTN_H = 36f;

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
                else if (InputManager.IsBackPressed())
                    _enterCodePanel.SetActive(false);
                return;
            }

            // Close overlays with Escape
            if (InputManager.IsBackPressed())
            {
                // Difficulty prompt (highest priority)
                if (_difficultyPromptPanel != null)
                {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    Destroy(_difficultyPromptPanel);
                    _difficultyPromptPanel = null;
                    return;
                }

                // New game confirmation dialog
                if (_confirmNewGamePanel != null)
                {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    Destroy(_confirmNewGamePanel);
                    _confirmNewGamePanel = null;
                    return;
                }

                // Legends overlay
                if (_legendsObj != null)
                    return; // LegendsUI handles its own escape

                // Achievements overlay
                if (_achievementsObj != null)
                    return; // AchievementsUI handles its own escape

                // Challenge entry overlay
                if (_challengeEntryObj != null)
                    return; // ChallengeEntryUI handles its own escape

                // Cosmetics overlay
                if (_cosmeticsObj != null)
                    return; // CosmeticSelectUI handles its own escape

                // About sub-panel → back to settings menu
                if (_aboutSubPanel != null && _aboutSubPanel.activeSelf)
                {
                    _aboutSubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                }
                // Audio sub-panel → back to settings menu
                else if (_audioSubPanel != null && _audioSubPanel.activeSelf)
                {
                    _audioSubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                }
                // Accessibility sub-panel → back to settings menu
                else if (_accessibilitySubPanel != null && _accessibilitySubPanel.activeSelf)
                {
                    _accessibilitySubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                }
                // Difficulty sub-panel → back to settings menu
                else if (_difficultySubPanel != null && _difficultySubPanel.activeSelf)
                {
                    _difficultySubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                }
                else if (_devPanel != null && _devPanel.activeSelf)
                    _devPanel.SetActive(false);
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
            _canvasGO = canvasGO;
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

            // Main menu buttons (center column)
            CreateMainMenu(canvasGO.transform);

            // Challenge cluster (top-right: DAILY, WEEKLY, CHALLENGE with alert badges)
            CreateChallengeCluster(canvasGO.transform);

            // Legend info panel (bottom center)
            CreateLegend(canvasGO.transform);

            // Controls hint
            var hintGO = CreateText(canvasGO.transform,
                "Move: Arrows/WASD | Jump: Space/W | Ground Pound: S (airborne) | Cycle Weapon: X | Pause: Esc/`", 16,
                new Color(0.65f, 0.65f, 0.75f));
            var hintRect = hintGO.GetComponent<RectTransform>();
            hintRect.anchoredPosition = new Vector2(0, -450);
            hintRect.sizeDelta = new Vector2(900, 30);

            // Build version label (bottom-right corner)
            var buildLabelGO = CreateText(canvasGO.transform, BuildInfo.FullBuildID, 14,
                new Color(0.45f, 0.45f, 0.5f));
            var buildLabelRect = buildLabelGO.GetComponent<RectTransform>();
            buildLabelRect.anchorMin = new Vector2(1f, 0f);
            buildLabelRect.anchorMax = new Vector2(1f, 0f);
            buildLabelRect.pivot = new Vector2(1f, 0f);
            buildLabelRect.anchoredPosition = new Vector2(-10, 10);

            // Enter code overlay (initially hidden)
            CreateEnterCodeOverlay(canvasGO.transform);

            // Settings overlay (initially hidden)
            CreateSettingsOverlay(canvasGO.transform);

            // Level selector overlay (initially hidden)
            CreateLevelSelectorOverlay(canvasGO.transform);

            // Level history overlay (initially hidden)
            CreateLevelHistoryOverlay(canvasGO.transform);

            // Dev menu (top-left button + panel)
            CreateDevMenu(canvasGO.transform);
        }

        private void RebuildUI()
        {
            if (_canvasGO != null) Destroy(_canvasGO);
            _legendsObj = null;
            _achievementsObj = null;
            _challengeEntryObj = null;
            _cosmeticsObj = null;
            _confirmNewGamePanel = null;
            _difficultyPromptPanel = null;
            _devPanel = null;
            _godModeCheckmark = null;
            CreateUI();
        }

        private void ShowNewGameConfirmation()
        {
            if (_confirmNewGamePanel != null) return;

            _confirmNewGamePanel = new GameObject("ConfirmNewGame");
            _confirmNewGamePanel.transform.SetParent(_canvasGO.transform, false);

            // Canvas on top of everything
            var panelCanvas = _confirmNewGamePanel.AddComponent<Canvas>();
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingOrder = 150;
            _confirmNewGamePanel.AddComponent<GraphicRaycaster>();

            // Full-screen dim overlay (blocks clicks behind)
            var dimGO = new GameObject("Dim");
            dimGO.transform.SetParent(_confirmNewGamePanel.transform, false);
            var dimImg = dimGO.AddComponent<Image>();
            dimImg.color = new Color(0.02f, 0.02f, 0.05f, 0.85f);
            var dimRect = dimGO.GetComponent<RectTransform>();
            dimRect.anchorMin = Vector2.zero;
            dimRect.anchorMax = Vector2.one;
            dimRect.sizeDelta = Vector2.zero;

            // Dialog box — ornate style
            var boxGO = new GameObject("DialogBox");
            boxGO.transform.SetParent(_confirmNewGamePanel.transform, false);
            var boxImg = boxGO.AddComponent<Image>();
            boxImg.color = new Color(0.85f, 0.65f, 0.12f, 0.9f); // outer gold border
            var boxRect = boxGO.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.sizeDelta = new Vector2(460, 220);
            boxRect.anchoredPosition = Vector2.zero;

            // Dark gap
            var gapGO = new GameObject("Gap");
            gapGO.transform.SetParent(boxGO.transform, false);
            var gapImg = gapGO.AddComponent<Image>();
            gapImg.color = new Color(0.06f, 0.04f, 0.10f);
            var gapRect = gapGO.GetComponent<RectTransform>();
            gapRect.anchorMin = Vector2.zero;
            gapRect.anchorMax = Vector2.one;
            gapRect.sizeDelta = new Vector2(-4, -4);

            // Inner gold border
            var innerGO = new GameObject("InnerBorder");
            innerGO.transform.SetParent(gapGO.transform, false);
            var innerImg = innerGO.AddComponent<Image>();
            innerImg.color = new Color(0.90f, 0.70f, 0.18f, 0.75f);
            var innerRect = innerGO.GetComponent<RectTransform>();
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.sizeDelta = new Vector2(-3, -3);

            // Dark background
            var bgGO = new GameObject("Bg");
            bgGO.transform.SetParent(innerGO.transform, false);
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.10f, 0.08f, 0.16f, 0.98f);
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = new Vector2(-3, -3);

            // Warning title
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(bgGO.transform, false);
            var titleImg = titleGO.AddComponent<Image>();
            titleImg.sprite = PlaceholderAssets.GetPixelTextSprite("NEW GAME", new Color(1f, 0.85f, 0.2f), 3);
            titleImg.preserveAspect = true;
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(300, 30);
            titleRect.anchoredPosition = new Vector2(0, 60);

            // Warning message
            var msgGO = CreateText(bgGO.transform,
                "All current progress will be lost.\nAre you sure?", 18,
                new Color(0.8f, 0.75f, 0.85f));
            var msgRect = msgGO.GetComponent<RectTransform>();
            msgRect.anchorMin = new Vector2(0.5f, 0.5f);
            msgRect.anchorMax = new Vector2(0.5f, 0.5f);
            msgRect.sizeDelta = new Vector2(380, 50);
            msgRect.anchoredPosition = new Vector2(0, 10);

            // CONTINUE button (confirms new game)
            CreateMenuButton(bgGO.transform, "CONTINUE", new Vector2(-90, -55),
                new Vector2(150, 42), new Color(0.55f, 0.25f, 0.2f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    Destroy(_confirmNewGamePanel);
                    _confirmNewGamePanel = null;
                    GameManager.ClearSession();
                    RebuildUI();
                });

            // CANCEL button
            CreateMenuButton(bgGO.transform, "CANCEL", new Vector2(90, -55),
                new Vector2(150, 42), new Color(0.3f, 0.3f, 0.38f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    Destroy(_confirmNewGamePanel);
                    _confirmNewGamePanel = null;
                });
        }

        private void ShowDifficultyPrompt()
        {
            if (_difficultyPromptPanel != null) return;

            _difficultyPromptPanel = new GameObject("DifficultyPrompt");
            _difficultyPromptPanel.transform.SetParent(_canvasGO.transform, false);

            var panelCanvas = _difficultyPromptPanel.AddComponent<Canvas>();
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingOrder = 150;
            _difficultyPromptPanel.AddComponent<GraphicRaycaster>();

            // Dim overlay
            var dimGO = new GameObject("Dim");
            dimGO.transform.SetParent(_difficultyPromptPanel.transform, false);
            var dimImg = dimGO.AddComponent<Image>();
            dimImg.color = new Color(0.02f, 0.02f, 0.05f, 0.85f);
            var dimRect = dimGO.GetComponent<RectTransform>();
            dimRect.anchorMin = Vector2.zero;
            dimRect.anchorMax = Vector2.one;
            dimRect.sizeDelta = Vector2.zero;

            // Dialog box
            var boxGO = new GameObject("DialogBox");
            boxGO.transform.SetParent(_difficultyPromptPanel.transform, false);
            var boxImg = boxGO.AddComponent<Image>();
            boxImg.color = new Color(0.85f, 0.65f, 0.12f, 0.9f);
            var boxRect = boxGO.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.sizeDelta = new Vector2(500, 300);
            boxRect.anchoredPosition = Vector2.zero;

            // Dark gap + inner border + bg (same ornate style)
            var gapGO = new GameObject("Gap");
            gapGO.transform.SetParent(boxGO.transform, false);
            var gapImg = gapGO.AddComponent<Image>();
            gapImg.color = new Color(0.06f, 0.04f, 0.10f);
            var gapRect = gapGO.GetComponent<RectTransform>();
            gapRect.anchorMin = Vector2.zero;
            gapRect.anchorMax = Vector2.one;
            gapRect.sizeDelta = new Vector2(-4, -4);

            var innerGO = new GameObject("InnerBorder");
            innerGO.transform.SetParent(gapGO.transform, false);
            var innerImg = innerGO.AddComponent<Image>();
            innerImg.color = new Color(0.90f, 0.70f, 0.18f, 0.75f);
            var innerRect = innerGO.GetComponent<RectTransform>();
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.sizeDelta = new Vector2(-3, -3);

            var bgGO = new GameObject("Bg");
            bgGO.transform.SetParent(innerGO.transform, false);
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.10f, 0.08f, 0.16f, 0.98f);
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = new Vector2(-3, -3);

            // Title
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(bgGO.transform, false);
            var titleImg = titleGO.AddComponent<Image>();
            titleImg.sprite = PlaceholderAssets.GetPixelTextSprite(
                "CHOOSE YOUR CHALLENGE", new Color(1f, 0.85f, 0.2f), 3);
            titleImg.preserveAspect = true;
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(420, 30);
            titleRect.anchoredPosition = new Vector2(0, 100);

            // Subtitle
            var subGO = CreateText(bgGO.transform,
                "You can change this later in Settings.", 16,
                new Color(0.6f, 0.6f, 0.7f));
            var subRect = subGO.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.5f, 0.5f);
            subRect.anchorMax = new Vector2(0.5f, 0.5f);
            subRect.sizeDelta = new Vector2(400, 30);
            subRect.anchoredPosition = new Vector2(0, 65);

            System.Action<DifficultyLevel> selectDifficulty = (level) => {
                AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                if (DifficultyManager.Instance != null)
                    DifficultyManager.Instance.CurrentDifficulty = level;
                PlayerPrefs.SetInt("EpochBreaker_DifficultyChosen", 1);
                SafePrefs.Save();
                Destroy(_difficultyPromptPanel);
                _difficultyPromptPanel = null;
                GameManager.Instance?.StartGame();
            };

            // EASY button
            CreateMenuButton(bgGO.transform, "EASY", new Vector2(-150, 10),
                new Vector2(130, 50), new Color(0.25f, 0.50f, 0.30f), () => {
                    selectDifficulty(DifficultyLevel.Easy);
                });
            var easyDesc = CreateText(bgGO.transform, "More health\nWeaker enemies", 13,
                new Color(0.5f, 0.7f, 0.5f));
            easyDesc.GetComponent<RectTransform>().anchoredPosition = new Vector2(-150, -35);

            // NORMAL button (recommended)
            CreateMenuButton(bgGO.transform, "NORMAL", new Vector2(0, 10),
                new Vector2(130, 50), new Color(0.35f, 0.40f, 0.55f), () => {
                    selectDifficulty(DifficultyLevel.Normal);
                });
            var normDesc = CreateText(bgGO.transform, "Recommended", 13,
                new Color(0.6f, 0.6f, 0.8f));
            normDesc.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -35);

            // HARD button
            CreateMenuButton(bgGO.transform, "HARD", new Vector2(150, 10),
                new Vector2(130, 50), new Color(0.55f, 0.25f, 0.25f), () => {
                    selectDifficulty(DifficultyLevel.Hard);
                });
            var hardDesc = CreateText(bgGO.transform, "Less health\nStronger enemies", 13,
                new Color(0.8f, 0.5f, 0.5f));
            hardDesc.GetComponent<RectTransform>().anchoredPosition = new Vector2(150, -35);
        }

        private void CreateMainMenu(Transform parent)
        {
            bool hasSavedSession = GameManager.HasSavedSession();

            if (hasSavedSession)
            {
                // CONTINUE button (primary)
                CreateMenuButton(parent, "CONTINUE", new Vector2(0, 110),
                    new Vector2(260, 55), new Color(0.2f, 0.55f, 0.3f), () => {
                        AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                        GameManager.Instance?.ContinueSession();
                    });

                // NEW GAME button (same size as Continue, below it)
                CreateMenuButton(parent, "NEW GAME", new Vector2(0, 45),
                    new Vector2(260, 55), new Color(0.55f, 0.45f, 0.15f), () => {
                        AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                        ShowNewGameConfirmation();
                    });

                // Enter Code button
                CreateMenuButton(parent, "ENTER CODE", new Vector2(0, -20),
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

                // Settings button
                CreateMenuButton(parent, "SETTINGS", new Vector2(0, -75),
                    new Vector2(260, 42), new Color(0.4f, 0.4f, 0.48f), () => {
                        AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                        if (_settingsPanel != null)
                            _settingsPanel.SetActive(true);
                    });
            }
            else
            {
                // Play Game button (primary) — first-play shows difficulty prompt
                CreateMenuButton(parent, "PLAY GAME", new Vector2(0, 110),
                    new Vector2(260, 55), new Color(0.2f, 0.55f, 0.3f), () => {
                        AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                        if (PlayerPrefs.GetInt("EpochBreaker_DifficultyChosen", 0) == 0)
                            ShowDifficultyPrompt();
                        else
                            GameManager.Instance?.StartGame();
                    });

                // Enter Code button
                CreateMenuButton(parent, "ENTER CODE", new Vector2(0, 45),
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

                // Settings button
                CreateMenuButton(parent, "SETTINGS", new Vector2(0, -15),
                    new Vector2(260, 42), new Color(0.4f, 0.4f, 0.48f), () => {
                        AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                        if (_settingsPanel != null)
                            _settingsPanel.SetActive(true);
                    });
            }

            // Player character preview (lower-right of title screen)
            CreatePlayerPreview(parent);
        }

        private void CreatePlayerPreview(Transform parent)
        {
            var previewGO = new GameObject("PlayerPreview");
            previewGO.transform.SetParent(parent, false);
            _playerPreviewImg = previewGO.AddComponent<Image>();

            RefreshPlayerPreviewSprite();

            _playerPreviewImg.preserveAspect = true;
            _playerPreviewImg.raycastTarget = false;

            var rect = previewGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(200, 300);
            rect.anchoredPosition = new Vector2(620, -280);
        }

        private void RefreshPlayerPreviewSprite()
        {
            if (_playerPreviewImg == null) return;
            var cm = CosmeticManager.Instance;
            PlayerSkin skin = cm != null ? cm.SelectedSkin : PlayerSkin.Default;
            _playerPreviewImg.sprite = skin != PlayerSkin.Default
                ? PlaceholderAssets.GetTintedPlayerSprite(skin)
                : PlaceholderAssets.GetPlayerSprite();
        }

        /// <summary>
        /// Creates the horizontal nav row with TROPHIES, LEVEL HISTORY, and LEGENDS buttons.
        /// Replaces the previous ornate corner buttons with a compact center row at Y=10.
        /// </summary>
        private void CreateNavRow(Transform parent)
        {
            CreateMenuButton(parent, "TROPHIES", new Vector2(-145, 10),
                new Vector2(130, 42), new Color(0.45f, 0.38f, 0.2f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    OpenAchievements();
                });

            CreateMenuButton(parent, "LEVEL HISTORY", new Vector2(0, 10),
                new Vector2(140, 42), new Color(0.35f, 0.45f, 0.55f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    if (_levelHistoryPanel != null)
                    {
                        RefreshHistoryContent();
                        _levelHistoryPanel.SetActive(true);
                    }
                });

            bool legendsUnlocked = GameManager.Instance != null && GameManager.Instance.LegendsUnlocked;
            CreateMenuButton(parent, "LEGENDS", new Vector2(145, 10),
                new Vector2(130, 42), legendsUnlocked
                    ? new Color(0.5f, 0.4f, 0.15f)
                    : new Color(0.25f, 0.22f, 0.28f), () => {
                    if (!legendsUnlocked) return;
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    OpenLegends();
                });
        }

        /// <summary>
        /// Creates the top-right challenge cluster containing DAILY, WEEKLY, and CHALLENGE buttons.
        /// Positioned right of the subtitle area so alert badges (NEW) are prominent and
        /// time-limited challenges are visually grouped together.
        /// </summary>
        private void CreateChallengeCluster(Transform parent)
        {
            // Container panel — top-right area, semi-transparent
            var containerGO = CreatePanel(parent, "ChallengeCluster", new Color(0.12f, 0.10f, 0.22f, 0.7f));
            var containerRect = containerGO.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(240, 180);
            // Top edge = 370 + 90 = 460, well within ±540 viewport bounds (leaves room for NEW badges)
            containerRect.anchoredPosition = new Vector2(660, 370);

            // DAILY, WEEKLY, CHALLENGE stacked vertically
            CreateDailyButton(containerGO.transform, new Vector2(0, 50));
            CreateWeeklyButton(containerGO.transform, new Vector2(0, -2));

            // CHALLENGE button (bottom)
            CreateMenuButton(containerGO.transform, "CHALLENGE", new Vector2(0, -54),
                new Vector2(220, 40), new Color(0.5f, 0.35f, 0.55f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    OpenChallengeEntry();
                });
        }

        private void CreateDailyButton(Transform parent, Vector2 position)
        {
            int epoch = DailyChallengeManager.GetTodayEpoch();
            string epochName = LevelID.GetEpochName(epoch);
            string difficulty = DailyChallengeManager.GetTodayDifficulty();
            bool played = DailyChallengeManager.HasPlayedToday();
            int streak = DailyChallengeManager.GetStreakCount();
            Color epochColor = GetEpochColor(epoch);

            // Button background — daily accent color based on epoch
            Color dailyBgColor = new Color(
                Mathf.Lerp(0.15f, epochColor.r, 0.4f),
                Mathf.Lerp(0.10f, epochColor.g, 0.4f),
                Mathf.Lerp(0.30f, epochColor.b, 0.4f));

            var go = new GameObject("DailyBtn");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = dailyBgColor;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => {
                AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                GameManager.Instance?.StartDailyChallenge();
            });

            var dailyColors = btn.colors;
            dailyColors.normalColor = dailyBgColor;
            dailyColors.highlightedColor = new Color(
                Mathf.Min(1f, dailyBgColor.r + 0.15f),
                Mathf.Min(1f, dailyBgColor.g + 0.15f),
                Mathf.Min(1f, dailyBgColor.b + 0.15f), dailyBgColor.a);
            dailyColors.pressedColor = new Color(
                dailyBgColor.r * 0.7f, dailyBgColor.g * 0.7f, dailyBgColor.b * 0.7f, dailyBgColor.a);
            dailyColors.selectedColor = dailyColors.normalColor;
            dailyColors.fadeDuration = 0.1f;
            btn.colors = dailyColors;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(220, 48);
            rect.anchoredPosition = position;

            // "DAILY" label (main text)
            string labelText = "DAILY";
            if (streak > 0)
                labelText += $" x{streak}";

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(labelText, Color.white, 3);
            labelImg.preserveAspect = true;
            labelImg.raycastTarget = false;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(160, 24);
            labelRect.anchoredPosition = new Vector2(0, 4);

            // Epoch/difficulty sub-label
            var subGO = new GameObject("SubLabel");
            subGO.transform.SetParent(go.transform, false);
            var subImg = subGO.AddComponent<Image>();
            subImg.sprite = PlaceholderAssets.GetPixelTextSprite(
                difficulty,
                new Color(0.75f, 0.75f, 0.85f), 2);
            subImg.preserveAspect = true;
            var subRect = subGO.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.5f, 0.5f);
            subRect.anchorMax = new Vector2(0.5f, 0.5f);
            subRect.sizeDelta = new Vector2(130, 14);
            subRect.anchoredPosition = new Vector2(0, -14);

            // "NEW" badge if not yet played today
            if (!played)
            {
                var badgeGO = new GameObject("NewBadge");
                badgeGO.transform.SetParent(go.transform, false);
                var badgeImg = badgeGO.AddComponent<Image>();
                badgeImg.color = new Color(0.9f, 0.3f, 0.2f);
                var badgeRect = badgeGO.GetComponent<RectTransform>();
                badgeRect.anchorMin = new Vector2(1f, 1f);
                badgeRect.anchorMax = new Vector2(1f, 1f);
                badgeRect.pivot = new Vector2(1f, 1f);
                badgeRect.sizeDelta = new Vector2(50, 24);
                badgeRect.anchoredPosition = new Vector2(4, 4);

                var badgeLabelGO = new GameObject("BadgeLabel");
                badgeLabelGO.transform.SetParent(badgeGO.transform, false);
                var badgeLabelImg = badgeLabelGO.AddComponent<Image>();
                badgeLabelImg.sprite = PlaceholderAssets.GetPixelTextSprite("NEW", Color.white, 2);
                badgeLabelImg.preserveAspect = true;
                var badgeLabelRect = badgeLabelGO.GetComponent<RectTransform>();
                badgeLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
                badgeLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
                badgeLabelRect.sizeDelta = new Vector2(42, 16);
                badgeLabelRect.anchoredPosition = Vector2.zero;
            }
        }

        private void CreateWeeklyButton(Transform parent, Vector2 position)
        {
            int epoch = DailyChallengeManager.GetWeeklyEpoch();
            string epochName = LevelID.GetEpochName(epoch);
            bool played = DailyChallengeManager.HasPlayedThisWeek();
            Color epochColor = GetEpochColor(epoch);

            // Button background — weekly accent color
            Color weeklyBgColor = new Color(
                Mathf.Lerp(0.15f, epochColor.r, 0.3f),
                Mathf.Lerp(0.25f, epochColor.g, 0.3f),
                Mathf.Lerp(0.15f, epochColor.b, 0.3f));

            var go = new GameObject("WeeklyBtn");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = weeklyBgColor;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => {
                AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                GameManager.Instance?.StartWeeklyChallenge();
            });

            var weeklyColors = btn.colors;
            weeklyColors.normalColor = weeklyBgColor;
            weeklyColors.highlightedColor = new Color(
                Mathf.Min(1f, weeklyBgColor.r + 0.15f),
                Mathf.Min(1f, weeklyBgColor.g + 0.15f),
                Mathf.Min(1f, weeklyBgColor.b + 0.15f), weeklyBgColor.a);
            weeklyColors.pressedColor = new Color(
                weeklyBgColor.r * 0.7f, weeklyBgColor.g * 0.7f, weeklyBgColor.b * 0.7f, weeklyBgColor.a);
            weeklyColors.selectedColor = weeklyColors.normalColor;
            weeklyColors.fadeDuration = 0.1f;
            btn.colors = weeklyColors;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(220, 48);
            rect.anchoredPosition = position;

            // "WEEKLY" label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite("WEEKLY", Color.white, 3);
            labelImg.preserveAspect = true;
            labelImg.raycastTarget = false;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(160, 24);
            labelRect.anchoredPosition = new Vector2(0, 4);

            // Epoch sub-label
            var subGO = new GameObject("SubLabel");
            subGO.transform.SetParent(go.transform, false);
            var subImg = subGO.AddComponent<Image>();
            subImg.sprite = PlaceholderAssets.GetPixelTextSprite(
                epochName,
                new Color(0.75f, 0.75f, 0.85f), 2);
            subImg.preserveAspect = true;
            var subRect = subGO.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.5f, 0.5f);
            subRect.anchorMax = new Vector2(0.5f, 0.5f);
            subRect.sizeDelta = new Vector2(130, 14);
            subRect.anchoredPosition = new Vector2(0, -14);

            // "NEW" badge if not yet played this week
            if (!played)
            {
                var badgeGO = new GameObject("NewBadge");
                badgeGO.transform.SetParent(go.transform, false);
                var badgeImg = badgeGO.AddComponent<Image>();
                badgeImg.color = new Color(0.3f, 0.7f, 0.3f);
                var badgeRect = badgeGO.GetComponent<RectTransform>();
                badgeRect.anchorMin = new Vector2(1f, 1f);
                badgeRect.anchorMax = new Vector2(1f, 1f);
                badgeRect.pivot = new Vector2(1f, 1f);
                badgeRect.sizeDelta = new Vector2(50, 24);
                badgeRect.anchoredPosition = new Vector2(4, 4);

                var badgeLabelGO = new GameObject("BadgeLabel");
                badgeLabelGO.transform.SetParent(badgeGO.transform, false);
                var badgeLabelImg = badgeLabelGO.AddComponent<Image>();
                badgeLabelImg.sprite = PlaceholderAssets.GetPixelTextSprite("NEW", Color.white, 2);
                badgeLabelImg.preserveAspect = true;
                var badgeLabelRect = badgeLabelGO.GetComponent<RectTransform>();
                badgeLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
                badgeLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
                badgeLabelRect.sizeDelta = new Vector2(42, 16);
                badgeLabelRect.anchoredPosition = Vector2.zero;
            }
        }

        private void OpenChallengeEntry()
        {
            if (_challengeEntryObj != null) return;
            _challengeEntryObj = new GameObject("ChallengeEntryOverlay");
            var entry = _challengeEntryObj.AddComponent<ChallengeEntryUI>();
            entry.Initialize(() => { _challengeEntryObj = null; });
        }

        private void CreateLegend(Transform parent)
        {
            // Legend panel — centered, wide enough for 6 items per row + label
            var panelGO = new GameObject("Legend");
            panelGO.transform.SetParent(parent, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.10f, 0.08f, 0.18f, 0.85f);
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(820, 260);
            panelRect.anchoredPosition = new Vector2(0, -260);

            // Centered layout: label left-aligned, items evenly distributed
            float labelX = -340f;
            float itemStart = -200f;
            float itemSpacing = 105f;

            // Row 1: Rewards (6 items)
            float row1Y = 95f;
            CreateLegendLabel(panelGO.transform, "Rewards:", new Vector2(labelX, row1Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetRewardSprite(RewardType.HealthSmall),
                "Health", new Vector2(itemStart, row1Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetRewardSprite(RewardType.AttackBoost),
                "Attack", new Vector2(itemStart + itemSpacing, row1Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetRewardSprite(RewardType.SpeedBoost),
                "Speed", new Vector2(itemStart + itemSpacing * 2, row1Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetRewardSprite(RewardType.Shield),
                "Shield", new Vector2(itemStart + itemSpacing * 3, row1Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetRewardSprite(RewardType.Coin),
                "Coin", new Vector2(itemStart + itemSpacing * 4, row1Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetExtraLifeSprite(),
                "1UP", new Vector2(itemStart + itemSpacing * 5, row1Y));

            // Row 2: Weapons (5 items — Slower removed)
            float row2Y = 0f;
            CreateLegendLabel(panelGO.transform, "Weapons:", new Vector2(labelX, row2Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetWeaponPickupSprite(WeaponType.Bolt, WeaponTier.Starting),
                "Bolt", new Vector2(itemStart, row2Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetWeaponPickupSprite(WeaponType.Piercer, WeaponTier.Starting),
                "Piercer", new Vector2(itemStart + itemSpacing, row2Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetWeaponPickupSprite(WeaponType.Spreader, WeaponTier.Starting),
                "Spreader", new Vector2(itemStart + itemSpacing * 2, row2Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetWeaponPickupSprite(WeaponType.Chainer, WeaponTier.Starting),
                "Chainer", new Vector2(itemStart + itemSpacing * 3, row2Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetWeaponPickupSprite(WeaponType.Cannon, WeaponTier.Starting),
                "Cannon", new Vector2(itemStart + itemSpacing * 4, row2Y));

            // Row 3: Checkpoints & Goal (4 items, same spacing for alignment)
            float row3Y = -95f;
            CreateLegendLabel(panelGO.transform, "Progress:", new Vector2(labelX, row3Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetCheckpointSprite(CheckpointType.LevelStart),
                "Start", new Vector2(itemStart, row3Y), 48f);
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetCheckpointSprite(CheckpointType.MidLevel),
                "Mid", new Vector2(itemStart + itemSpacing, row3Y), 48f);
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetCheckpointSprite(CheckpointType.PreBoss),
                "Boss", new Vector2(itemStart + itemSpacing * 2, row3Y), 48f);
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetExitPortalSprite(),
                "Exit", new Vector2(itemStart + itemSpacing * 3, row3Y), 48f);

            // ACHIEVEMENTS and LEGENDS — stacked vertically LEFT of the legend panel
            float btnX = -660f;
            CreateOrnateButton(parent, "ACHIEVEMENTS", new Vector2(btnX, -230f),
                new Vector2(180, 50), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    OpenAchievements();
                });

            bool legendsUnlocked = GameManager.Instance != null && GameManager.Instance.LegendsUnlocked;
            CreateOrnateButton(parent, "LEGENDS", new Vector2(btnX, -290f),
                new Vector2(180, 50), () => {
                    if (!legendsUnlocked) return;
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    OpenLegends();
                }, legendsUnlocked ? 1f : 0.4f);
        }

        private void OpenLegends()
        {
            if (_legendsObj != null) return;
            _legendsObj = new GameObject("LegendsOverlay");
            var legends = _legendsObj.AddComponent<LegendsUI>();
            legends.Initialize(() => { _legendsObj = null; });
        }

        private void OpenAchievements()
        {
            if (_achievementsObj != null) return;
            _achievementsObj = new GameObject("AchievementsOverlay");
            var achievements = _achievementsObj.AddComponent<AchievementsUI>();
            achievements.Initialize(() => { _achievementsObj = null; });
        }

        private void OpenCosmetics()
        {
            if (_cosmeticsObj != null) return;
            _cosmeticsObj = new GameObject("CosmeticsOverlay");
            var cosmetics = _cosmeticsObj.AddComponent<CosmeticSelectUI>();
            cosmetics.Initialize(() => {
                _cosmeticsObj = null;
                RefreshPlayerPreviewSprite();
            });
        }

        private void CreateLegendLabel(Transform parent, string text, Vector2 position)
        {
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(parent, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(text.Replace(":", ""), new Color(0.8f, 0.8f, 0.9f), 3);
            labelImg.preserveAspect = true;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(140, 30);
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
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(label, new Color(0.75f, 0.75f, 0.85f), 3);
            labelImg.preserveAspect = true;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0f);
            labelRect.anchorMax = new Vector2(0.5f, 0f);
            labelRect.sizeDelta = new Vector2(110, 24);
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
            var escGO = CreateText(panelGO.transform, "Press Esc/` to close", 14,
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
            panelRect.sizeDelta = new Vector2(520, 620);
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
            titleRect.anchoredPosition = new Vector2(0, 240);

            // Close button
            CreateMenuButton(_settingsMenuContent.transform, "X", new Vector2(230, 240),
                new Vector2(40, 40), new Color(0.6f, 0.25f, 0.25f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _settingsPanel.SetActive(false);
                });

            // Settings items — y-accumulator with consistent spacing
            float sy = 180f; // First item below title

            CreateMenuButton(_settingsMenuContent.transform, "ABOUT", new Vector2(0, sy),
                new Vector2(300, BTN_HEIGHT), new Color(0.35f, 0.40f, 0.45f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _settingsMenuContent.SetActive(false);
                    _aboutSubPanel.SetActive(true);
                });

            sy -= ITEM_SPACING;
            CreateMenuButton(_settingsMenuContent.transform, "DIFFICULTY", new Vector2(0, sy),
                new Vector2(300, BTN_HEIGHT), new Color(0.5f, 0.35f, 0.35f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _settingsMenuContent.SetActive(false);
                    _difficultySubPanel.SetActive(true);
                });

            sy -= ITEM_SPACING;
            CreateMenuButton(_settingsMenuContent.transform, "COSMETICS", new Vector2(0, sy),
                new Vector2(300, BTN_HEIGHT), new Color(0.5f, 0.4f, 0.6f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    OpenCosmetics();
                });

            sy -= ITEM_SPACING;
            CreateMenuButton(_settingsMenuContent.transform, "LEVEL HISTORY", new Vector2(0, sy),
                new Vector2(300, BTN_HEIGHT), new Color(0.35f, 0.45f, 0.55f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _settingsPanel.SetActive(false);
                    if (_levelHistoryPanel != null)
                    {
                        RefreshHistoryContent();
                        _levelHistoryPanel.SetActive(true);
                    }
                });

            sy -= ITEM_SPACING;
            CreateMenuButton(_settingsMenuContent.transform, "AUDIO", new Vector2(0, sy),
                new Vector2(300, BTN_HEIGHT), new Color(0.3f, 0.4f, 0.55f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _settingsMenuContent.SetActive(false);
                    _audioSubPanel.SetActive(true);
                });

            sy -= ITEM_SPACING;
            CreateMenuButton(_settingsMenuContent.transform, "ACCESSIBILITY", new Vector2(0, sy),
                new Vector2(300, BTN_HEIGHT), new Color(0.3f, 0.45f, 0.5f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _settingsMenuContent.SetActive(false);
                    _accessibilitySubPanel.SetActive(true);
                });

            // Campaign toggle — always visible; label changes based on completion status
            sy -= ITEM_SPACING;
            {
                bool campaignCompleted = AchievementManager.Instance != null
                    && AchievementManager.Instance.IsUnlocked(AchievementType.EpochExplorer);
                bool campaignActive = GameManager.Instance != null && GameManager.Instance.CampaignModeEnabled;
                string campaignLabel = campaignCompleted
                    ? (campaignActive ? "REPLAY CAMPAIGN (ON)" : "REPLAY CAMPAIGN")
                    : (campaignActive ? "START CAMPAIGN (ON)" : "START CAMPAIGN");
                Color campaignColor = campaignActive
                    ? new Color(0.7f, 0.55f, 0.2f) : new Color(0.55f, 0.45f, 0.15f);
                CreateMenuButton(_settingsMenuContent.transform, campaignLabel,
                    new Vector2(0, sy), new Vector2(300, BTN_HEIGHT), campaignColor, () => {
                        AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.CampaignModeEnabled = !GameManager.Instance.CampaignModeEnabled;
                            RebuildUI();
                        }
                    });
                if (campaignActive)
                {
                    sy -= 22f;
                    var feedbackGO = CreateText(_settingsMenuContent.transform,
                        "Next PLAY will start Campaign mode", 14, new Color(1f, 0.85f, 0.3f));
                    var feedbackRect = feedbackGO.GetComponent<RectTransform>();
                    feedbackRect.anchoredPosition = new Vector2(0, sy);
                }
            }

            // Tutorial buttons
            sy -= ITEM_SPACING;
            bool tutDone = Gameplay.TutorialManager.IsTutorialCompleted();

            CreateMenuButton(_settingsMenuContent.transform, "REPLAY TUTORIAL", new Vector2(140, sy),
                new Vector2(150, SMALL_BTN_H), new Color(0.35f, 0.45f, 0.4f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.TutorialManager.ResetTutorial();
                    _settingsPanel.SetActive(false);
                    GameManager.Instance?.StartGame();
                });

            if (!tutDone)
            {
                CreateMenuButton(_settingsMenuContent.transform, "SKIP TUTORIAL", new Vector2(-80, sy),
                    new Vector2(150, SMALL_BTN_H), new Color(0.45f, 0.35f, 0.35f), () => {
                        AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                        Gameplay.TutorialManager.SetTutorialCompleted();
                        _settingsPanel.SetActive(false);
                    });
            }

            sy -= ITEM_SPACING;
            var escGO = CreateText(_settingsMenuContent.transform, "Press Esc/` to close", 14,
                new Color(0.5f, 0.5f, 0.6f));
            var escRect = escGO.GetComponent<RectTransform>();
            escRect.anchoredPosition = new Vector2(0, sy);

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
            var audioEscGO = CreateText(_audioSubPanel.transform, "Press Esc/` to go back", 14,
                new Color(0.5f, 0.5f, 0.6f));
            var audioEscRect = audioEscGO.GetComponent<RectTransform>();
            audioEscRect.anchoredPosition = new Vector2(0, -175);

            _audioSubPanel.SetActive(false);

            // ── About sub-panel ──
            _aboutSubPanel = new GameObject("AboutSubPanel");
            _aboutSubPanel.transform.SetParent(panelGO.transform, false);
            var aboutRect = _aboutSubPanel.AddComponent<RectTransform>();
            aboutRect.anchorMin = Vector2.zero;
            aboutRect.anchorMax = Vector2.one;
            aboutRect.sizeDelta = Vector2.zero;

            // About title
            var aboutTitleGO = CreateText(_aboutSubPanel.transform, "ABOUT", 32, new Color(1f, 0.85f, 0.1f));
            var aboutTitleRect = aboutTitleGO.GetComponent<RectTransform>();
            aboutTitleRect.anchoredPosition = new Vector2(0, 200);

            // Back button
            CreateMenuButton(_aboutSubPanel.transform, "BACK", new Vector2(-200, 200),
                new Vector2(80, 40), new Color(0.4f, 0.4f, 0.5f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _aboutSubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                });

            // Close button
            CreateMenuButton(_aboutSubPanel.transform, "X", new Vector2(230, 200),
                new Vector2(40, 40), new Color(0.6f, 0.25f, 0.25f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _aboutSubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                    _settingsPanel.SetActive(false);
                });

            // Divider
            var aboutDivGO = new GameObject("Divider");
            aboutDivGO.transform.SetParent(_aboutSubPanel.transform, false);
            var aboutDivImg = aboutDivGO.AddComponent<Image>();
            aboutDivImg.color = new Color(0.3f, 0.3f, 0.4f);
            var aboutDivRect = aboutDivGO.GetComponent<RectTransform>();
            aboutDivRect.anchorMin = new Vector2(0.5f, 0.5f);
            aboutDivRect.anchorMax = new Vector2(0.5f, 0.5f);
            aboutDivRect.sizeDelta = new Vector2(440, 2);
            aboutDivRect.anchoredPosition = new Vector2(0, 165);

            // Campaign info
            var campaignGO = CreateText(_aboutSubPanel.transform,
                "CAMPAIGN MODE\n" +
                "Play 10 levels through Epochs 0-9 in order.\n" +
                "Infinite lives. Die too many times = level failed, move on.\n" +
                "Deaths per level depend on difficulty setting.", 16,
                new Color(0.75f, 0.75f, 0.85f));
            var campaignRect = campaignGO.GetComponent<RectTransform>();
            campaignRect.anchoredPosition = new Vector2(0, 95);
            campaignRect.sizeDelta = new Vector2(460, 120);
            campaignGO.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Wrap;

            // Streak info
            var streakGO = CreateText(_aboutSubPanel.transform,
                "THE BREACH (default mode)\n" +
                "Random levels, any epoch. Endless exploration.\n" +
                "Start Campaign from Settings to unlock cosmetics.\n" +
                "STREAK: 10 lives, how far can you go?", 16,
                new Color(0.85f, 0.75f, 0.55f));
            var streakRect = streakGO.GetComponent<RectTransform>();
            streakRect.anchoredPosition = new Vector2(0, -10);
            streakRect.sizeDelta = new Vector2(460, 100);
            streakGO.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Wrap;

            // Legends info
            var legendsInfoGO = CreateText(_aboutSubPanel.transform,
                "LEGENDS\n" +
                "Complete Campaign to unlock. Tracks your best\n" +
                "streak runs on a ranked leaderboard.", 16,
                new Color(1f, 0.85f, 0.4f));
            var legendsInfoRect = legendsInfoGO.GetComponent<RectTransform>();
            legendsInfoRect.anchoredPosition = new Vector2(0, -100);
            legendsInfoRect.sizeDelta = new Vector2(460, 80);
            legendsInfoGO.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Wrap;

            // Build ID
            var buildGO = CreateText(_aboutSubPanel.transform, BuildInfo.FullBuildID, 16,
                new Color(0.6f, 0.6f, 0.65f));
            var buildRect = buildGO.GetComponent<RectTransform>();
            buildRect.anchoredPosition = new Vector2(0, -170);

            // Escape hint
            var aboutEscGO = CreateText(_aboutSubPanel.transform, "Press Esc/` to go back", 14,
                new Color(0.5f, 0.5f, 0.6f));
            var aboutEscRect = aboutEscGO.GetComponent<RectTransform>();
            aboutEscRect.anchoredPosition = new Vector2(0, -200);

            _aboutSubPanel.SetActive(false);

            // ── Accessibility sub-panel (Sprint 9) ──
            _accessibilitySubPanel = new GameObject("AccessibilitySubPanel");
            _accessibilitySubPanel.transform.SetParent(panelGO.transform, false);
            var accRect = _accessibilitySubPanel.AddComponent<RectTransform>();
            accRect.anchorMin = Vector2.zero;
            accRect.anchorMax = Vector2.one;
            accRect.sizeDelta = Vector2.zero;

            var accTitleGO = CreateText(_accessibilitySubPanel.transform, "ACCESSIBILITY", 28, new Color(1f, 0.85f, 0.1f));
            var accTitleRect = accTitleGO.GetComponent<RectTransform>();
            accTitleRect.anchoredPosition = new Vector2(0, 200);

            CreateMenuButton(_accessibilitySubPanel.transform, "BACK", new Vector2(-200, 200),
                new Vector2(80, 40), new Color(0.4f, 0.4f, 0.5f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _accessibilitySubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                });

            CreateMenuButton(_accessibilitySubPanel.transform, "X", new Vector2(230, 200),
                new Vector2(40, 40), new Color(0.6f, 0.25f, 0.25f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _accessibilitySubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                    _settingsPanel.SetActive(false);
                });

            // Colorblind mode toggle
            bool cbInitial = Gameplay.AccessibilityManager.Instance != null &&
                Gameplay.AccessibilityManager.Instance.ColorblindMode;
            CreateToggleRow(_accessibilitySubPanel.transform, "COLORBLIND MODE", new Vector2(0, 120), cbInitial,
                (val) => { if (Gameplay.AccessibilityManager.Instance != null) Gameplay.AccessibilityManager.Instance.ColorblindMode = val; });

            // Screen shake intensity slider
            float shakeInitial = Gameplay.AccessibilityManager.Instance != null
                ? Gameplay.AccessibilityManager.Instance.ScreenShakeIntensity : 1f;
            CreateVolumeSlider(_accessibilitySubPanel.transform, "SCREEN SHAKE", new Vector2(0, 55), shakeInitial,
                (val) => { if (Gameplay.AccessibilityManager.Instance != null) Gameplay.AccessibilityManager.Instance.ScreenShakeIntensity = val; });

            // Font size slider (0.8 to 1.5 mapped to 0-1 slider)
            float fontInitial = Gameplay.AccessibilityManager.Instance != null
                ? (Gameplay.AccessibilityManager.Instance.FontSizeScale - 0.8f) / 0.7f : 0.286f;
            CreateVolumeSlider(_accessibilitySubPanel.transform, "FONT SIZE", new Vector2(0, -10), fontInitial,
                (val) => {
                    float scale = 0.8f + val * 0.7f; // Map 0-1 to 0.8-1.5
                    if (Gameplay.AccessibilityManager.Instance != null)
                        Gameplay.AccessibilityManager.Instance.FontSizeScale = scale;
                });

            // High contrast mode toggle
            bool hcInitial = Gameplay.AccessibilityManager.Instance != null &&
                Gameplay.AccessibilityManager.Instance.HighContrastMode;
            CreateToggleRow(_accessibilitySubPanel.transform, "HIGH CONTRAST", new Vector2(0, -85), hcInitial,
                (val) => { if (Gameplay.AccessibilityManager.Instance != null) Gameplay.AccessibilityManager.Instance.HighContrastMode = val; });

            var accEscGO = CreateText(_accessibilitySubPanel.transform, "Press Esc/` to go back", 14,
                new Color(0.5f, 0.5f, 0.6f));
            var accEscRect = accEscGO.GetComponent<RectTransform>();
            accEscRect.anchoredPosition = new Vector2(0, -200);

            _accessibilitySubPanel.SetActive(false);

            // ── Difficulty sub-panel (Sprint 9) ──
            _difficultySubPanel = new GameObject("DifficultySubPanel");
            _difficultySubPanel.transform.SetParent(panelGO.transform, false);
            var diffRect = _difficultySubPanel.AddComponent<RectTransform>();
            diffRect.anchorMin = Vector2.zero;
            diffRect.anchorMax = Vector2.one;
            diffRect.sizeDelta = Vector2.zero;

            var diffTitleGO = CreateText(_difficultySubPanel.transform, "DIFFICULTY", 28, new Color(1f, 0.85f, 0.1f));
            var diffTitleRect = diffTitleGO.GetComponent<RectTransform>();
            diffTitleRect.anchoredPosition = new Vector2(0, 200);

            CreateMenuButton(_difficultySubPanel.transform, "BACK", new Vector2(-200, 200),
                new Vector2(80, 40), new Color(0.4f, 0.4f, 0.5f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _difficultySubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                });

            CreateMenuButton(_difficultySubPanel.transform, "X", new Vector2(230, 200),
                new Vector2(40, 40), new Color(0.6f, 0.25f, 0.25f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _difficultySubPanel.SetActive(false);
                    _settingsMenuContent.SetActive(true);
                    _settingsPanel.SetActive(false);
                });

            // Difficulty description
            var diffDescGO = CreateText(_difficultySubPanel.transform,
                "Changes apply at next level start.", 16,
                new Color(0.7f, 0.7f, 0.8f));
            var diffDescRect = diffDescGO.GetComponent<RectTransform>();
            diffDescRect.anchoredPosition = new Vector2(0, 140);
            diffDescRect.sizeDelta = new Vector2(460, 50);
            diffDescGO.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Wrap;

            // Active indicator (text that shows current selection)
            var diffActiveGO = CreateText(_difficultySubPanel.transform, "", 18, new Color(0.9f, 0.85f, 0.5f));
            var diffActiveRect = diffActiveGO.GetComponent<RectTransform>();
            diffActiveRect.anchoredPosition = new Vector2(0, -160);
            var diffActiveText = diffActiveGO.GetComponent<Text>();

            // Difficulty buttons — store Image refs for highlight
            Color easyBase = new Color(0.25f, 0.50f, 0.30f);
            Color normalBase = new Color(0.35f, 0.40f, 0.55f);
            Color hardBase = new Color(0.55f, 0.25f, 0.25f);
            Color highlightTint = new Color(0.3f, 0.3f, 0.3f, 0f); // Additive brightness for active

            var easyBtnGO = CreateMenuButton(_difficultySubPanel.transform, "EASY", new Vector2(-140, 60),
                new Vector2(130, 55), easyBase, null);
            var normalBtnGO = CreateMenuButton(_difficultySubPanel.transform, "NORMAL", new Vector2(0, 60),
                new Vector2(130, 55), normalBase, null);
            var hardBtnGO = CreateMenuButton(_difficultySubPanel.transform, "HARD", new Vector2(140, 60),
                new Vector2(130, 55), hardBase, null);

            var easyImg = easyBtnGO.GetComponent<Image>();
            var normalImg = normalBtnGO.GetComponent<Image>();
            var hardImg = hardBtnGO.GetComponent<Image>();

            System.Action updateDifficultyLabel = () => {
                if (Gameplay.DifficultyManager.Instance == null) return;
                var d = Gameplay.DifficultyManager.Instance.CurrentDifficulty;
                string desc = d switch {
                    Gameplay.DifficultyLevel.Easy => "EASY: 3 lives/level, fewer enemies, more health",
                    Gameplay.DifficultyLevel.Hard => "HARD: 1 life/level, more enemies, no health",
                    _ => "NORMAL: 2 lives/level, standard enemies and health"
                };
                diffActiveText.text = desc;

                // Highlight active difficulty button
                easyImg.color = d == Gameplay.DifficultyLevel.Easy ? easyBase + highlightTint : easyBase;
                normalImg.color = d == Gameplay.DifficultyLevel.Normal ? normalBase + highlightTint : normalBase;
                hardImg.color = d == Gameplay.DifficultyLevel.Hard ? hardBase + highlightTint : hardBase;
            };
            updateDifficultyLabel();

            // Wire button clicks
            easyBtnGO.GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                if (Gameplay.DifficultyManager.Instance != null)
                    Gameplay.DifficultyManager.Instance.CurrentDifficulty = Gameplay.DifficultyLevel.Easy;
                updateDifficultyLabel();
            });
            normalBtnGO.GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                if (Gameplay.DifficultyManager.Instance != null)
                    Gameplay.DifficultyManager.Instance.CurrentDifficulty = Gameplay.DifficultyLevel.Normal;
                updateDifficultyLabel();
            });
            hardBtnGO.GetComponent<Button>().onClick.AddListener(() => {
                AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                if (Gameplay.DifficultyManager.Instance != null)
                    Gameplay.DifficultyManager.Instance.CurrentDifficulty = Gameplay.DifficultyLevel.Hard;
                updateDifficultyLabel();
            });

            // Descriptions for each mode
            CreateText(_difficultySubPanel.transform, "3 deaths/level\n50% enemies\n1.5x health", 14,
                new Color(0.5f, 0.7f, 0.5f)).GetComponent<RectTransform>().anchoredPosition = new Vector2(-140, -20);
            CreateText(_difficultySubPanel.transform, "2 deaths/level\nStandard\nStandard", 14,
                new Color(0.6f, 0.6f, 0.8f)).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);
            CreateText(_difficultySubPanel.transform, "1 death/level\n1.5x enemies\nNo health", 14,
                new Color(0.8f, 0.5f, 0.5f)).GetComponent<RectTransform>().anchoredPosition = new Vector2(140, -20);

            var diffEscGO = CreateText(_difficultySubPanel.transform, "Press Esc/` to go back", 14,
                new Color(0.5f, 0.5f, 0.6f));
            var diffEscRect = diffEscGO.GetComponent<RectTransform>();
            diffEscRect.anchoredPosition = new Vector2(0, -200);

            _difficultySubPanel.SetActive(false);
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
            var hintGO = CreateText(panelGO.transform, "Press Esc/` to close", 14,
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

            // Pagination controls
            CreateMenuButton(panelGO.transform, "< PREV", new Vector2(-160, -280),
                new Vector2(100, 36), new Color(0.35f, 0.35f, 0.50f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    if (_historyPage > 0) { _historyPage--; RefreshHistoryContent(); }
                });

            var pageLabelGO = CreateText(panelGO.transform, "Page 1 of 1", 16,
                new Color(0.6f, 0.6f, 0.7f));
            var pageLabelRect = pageLabelGO.GetComponent<RectTransform>();
            pageLabelRect.anchoredPosition = new Vector2(0, -280);
            _historyPageLabel = pageLabelGO.GetComponent<Text>();

            CreateMenuButton(panelGO.transform, "NEXT >", new Vector2(160, -280),
                new Vector2(100, 36), new Color(0.35f, 0.35f, 0.50f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    var h = GameManager.LoadLevelHistory();
                    int maxPage = Mathf.Max(0, (h.Entries.Count - 1) / HISTORY_PAGE_SIZE);
                    if (_historyPage < maxPage) { _historyPage++; RefreshHistoryContent(); }
                });

            // Clear history button
            CreateMenuButton(panelGO.transform, "CLEAR HISTORY", new Vector2(0, -320),
                new Vector2(180, 36), new Color(0.5f, 0.3f, 0.3f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    GameManager.ClearLevelHistory();
                    _historyPage = 0;
                    RefreshHistoryContent();
                });

            // Hint text
            var hintGO2 = CreateText(panelGO.transform, "Press Esc/` to close", 14,
                new Color(0.5f, 0.5f, 0.6f));
            var hintRect2 = hintGO2.GetComponent<RectTransform>();
            hintRect2.anchoredPosition = new Vector2(0, -345);

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
                _historyPage = 0;
                if (_historyPageLabel != null) _historyPageLabel.text = "Page 1 of 1";
                var noHistoryGO = CreateText(_historyContentParent.transform, "No levels played yet", 20,
                    new Color(0.5f, 0.5f, 0.6f));
                var noHistoryRect = noHistoryGO.GetComponent<RectTransform>();
                noHistoryRect.anchoredPosition = new Vector2(0, 100);
                return;
            }

            // Pagination
            int totalPages = Mathf.Max(1, (history.Entries.Count + HISTORY_PAGE_SIZE - 1) / HISTORY_PAGE_SIZE);
            _historyPage = Mathf.Clamp(_historyPage, 0, totalPages - 1);
            if (_historyPageLabel != null)
                _historyPageLabel.text = $"Page {_historyPage + 1} of {totalPages}";

            int startIdx = _historyPage * HISTORY_PAGE_SIZE;
            int endIdx = Mathf.Min(startIdx + HISTORY_PAGE_SIZE, history.Entries.Count);

            // Column headers
            float headerY = 220f;
            CreateHistoryHeader(_historyContentParent.transform, "CODE", new Vector2(-260, headerY), 120);
            CreateHistoryHeader(_historyContentParent.transform, "SOURCE", new Vector2(-150, headerY), 80);
            CreateHistoryHeader(_historyContentParent.transform, "EPOCH", new Vector2(-50, headerY), 120);
            CreateHistoryHeader(_historyContentParent.transform, "SCORE", new Vector2(80, headerY), 100);
            CreateHistoryHeader(_historyContentParent.transform, "STARS", new Vector2(180, headerY), 60);
            CreateHistoryHeader(_historyContentParent.transform, "COPY", new Vector2(270, headerY), 60);

            float rowY = 180f;
            float rowHeight = 42f;

            for (int i = startIdx; i < endIdx; i++)
            {
                var entry = history.Entries[i];
                CreateHistoryRow(_historyContentParent.transform, entry, rowY);
                rowY -= rowHeight;
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
            var codeGO = CreateText(rowGO.transform, entry.Code, 16, new Color(0.5f, 0.8f, 1f));
            var codeRect = codeGO.GetComponent<RectTransform>();
            codeRect.anchoredPosition = new Vector2(-260, 0);
            codeRect.sizeDelta = new Vector2(120, 30);

            // Source (color-coded)
            string source = string.IsNullOrEmpty(entry.Source) ? "-" : entry.Source;
            Color sourceColor;
            switch (entry.Source)
            {
                case "Campaign": sourceColor = new Color(0.4f, 0.9f, 0.4f); break;
                case "Challenge": sourceColor = new Color(0.5f, 0.8f, 1f); break;
                case "Daily": sourceColor = new Color(0.7f, 0.5f, 0.9f); break;
                case "Weekly": sourceColor = new Color(0.7f, 0.5f, 0.9f); break;
                case "Code": sourceColor = new Color(0.6f, 0.6f, 0.65f); break;
                default: sourceColor = new Color(0.45f, 0.45f, 0.5f); break;
            }
            var sourceGO = CreateText(rowGO.transform, source, 14, sourceColor);
            var sourceRect = sourceGO.GetComponent<RectTransform>();
            sourceRect.anchoredPosition = new Vector2(-150, 0);
            sourceRect.sizeDelta = new Vector2(80, 30);

            // Epoch name
            string epochShort = entry.EpochName.Length > 10 ? entry.EpochName.Substring(0, 8) + ".." : entry.EpochName;
            var epochGO = CreateText(rowGO.transform, epochShort, 14, GetEpochColor(entry.Epoch) * 2f);
            var epochRect = epochGO.GetComponent<RectTransform>();
            epochRect.anchoredPosition = new Vector2(-50, 0);
            epochRect.sizeDelta = new Vector2(120, 30);

            // Score
            var scoreGO = CreateText(rowGO.transform, entry.Score.ToString("N0"), 14, Color.white);
            var scoreRect = scoreGO.GetComponent<RectTransform>();
            scoreRect.anchoredPosition = new Vector2(80, 0);
            scoreRect.sizeDelta = new Vector2(100, 30);

            // Stars
            string stars = new string('*', entry.Stars) + new string('-', 3 - entry.Stars);
            var starsGO = CreateText(rowGO.transform, stars, 16, new Color(1f, 0.85f, 0.1f));
            var starsRect = starsGO.GetComponent<RectTransform>();
            starsRect.anchoredPosition = new Vector2(180, 0);
            starsRect.sizeDelta = new Vector2(60, 30);

            // Copy button
            string capturedCode = entry.Code;
            CreateSmallButton(rowGO.transform, "COPY", new Vector2(270, 0), new Vector2(55, 28),
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

        // =====================================================================
        // Dev Menu
        // =====================================================================

        private void CreateDevMenu(Transform parent)
        {
            // DEV button — top-left, positioned inside the ornate frame safe area
            var btnGO = new GameObject("DevButton");
            btnGO.transform.SetParent(parent, false);

            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.4f, 0.3f, 0.5f);

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(() => {
                AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                if (_devPanel != null)
                    _devPanel.SetActive(!_devPanel.activeSelf);
            });

            var colors = btn.colors;
            colors.normalColor = btnImg.color;
            colors.highlightedColor = new Color(0.55f, 0.45f, 0.65f);
            colors.pressedColor = new Color(0.3f, 0.2f, 0.35f);
            colors.selectedColor = colors.normalColor;
            colors.fadeDuration = 0.1f;
            btn.colors = colors;

            var btnRect = btnGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0, 1);
            btnRect.anchorMax = new Vector2(0, 1);
            btnRect.pivot = new Vector2(0, 1);
            btnRect.sizeDelta = new Vector2(DEV_BTN_W, DEV_BTN_H);
            btnRect.anchoredPosition = new Vector2(FRAME_INSET, -FRAME_INSET);

            // DEV label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(btnGO.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite("DEV", Color.white, 3);
            labelImg.preserveAspect = true;
            labelImg.raycastTarget = false;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(64, 24);
            labelRect.anchoredPosition = Vector2.zero;

            // Dev panel (initially hidden)
            CreateDevPanel(parent);
        }

        private void CreateDevPanel(Transform parent)
        {
            // Panel positioned below the DEV button, inside frame safe area
            float panelTop = FRAME_INSET + DEV_BTN_H + 6f;
            float panelW = 340f;

            _devPanel = new GameObject("DevPanel");
            _devPanel.transform.SetParent(parent, false);
            _devPanel.SetActive(false);

            var panelImg = _devPanel.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.08f, 0.15f, 0.95f);

            var panelRect = _devPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 1);
            panelRect.anchorMax = new Vector2(0, 1);
            panelRect.pivot = new Vector2(0, 1);
            panelRect.anchoredPosition = new Vector2(FRAME_INSET, -panelTop);

            // Y-accumulator for panel contents (relative to panel top)
            float dy = -20f;

            // Title
            var titleGO = CreateText(_devPanel.transform, "DEVELOPMENT", 20, new Color(1f, 0.85f, 0.1f));
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, dy);

            // Close button (X) top-right
            var closeGO = new GameObject("CloseBtn");
            closeGO.transform.SetParent(_devPanel.transform, false);
            var closeImg = closeGO.AddComponent<Image>();
            closeImg.color = new Color(0.6f, 0.25f, 0.25f);
            var closeBtn = closeGO.AddComponent<Button>();
            closeBtn.targetGraphic = closeImg;
            closeBtn.onClick.AddListener(() => {
                AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                _devPanel.SetActive(false);
            });
            var closeRect = closeGO.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 1);
            closeRect.anchorMax = new Vector2(1, 1);
            closeRect.pivot = new Vector2(1, 1);
            closeRect.sizeDelta = new Vector2(30, 30);
            closeRect.anchoredPosition = new Vector2(-8, -8);

            var xLabel = CreateText(closeGO.transform, "X", 16, Color.white);
            var xRect = xLabel.GetComponent<RectTransform>();
            xRect.anchoredPosition = Vector2.zero;

            dy -= 44f; // Below title

            // ── GOD MODE toggle ──
            var godRow = new GameObject("GodModeRow");
            godRow.transform.SetParent(_devPanel.transform, false);
            var godRowRect = godRow.AddComponent<RectTransform>();
            godRowRect.anchorMin = new Vector2(0, 1);
            godRowRect.anchorMax = new Vector2(1, 1);
            godRowRect.pivot = new Vector2(0.5f, 1);
            godRowRect.sizeDelta = new Vector2(0, 36);
            godRowRect.anchoredPosition = new Vector2(0, dy);

            // Checkbox background
            var checkBG = new GameObject("CheckBG");
            checkBG.transform.SetParent(godRow.transform, false);
            var checkBGImg = checkBG.AddComponent<Image>();
            checkBGImg.color = new Color(0.2f, 0.18f, 0.25f);
            var checkBGRect = checkBG.GetComponent<RectTransform>();
            checkBGRect.anchorMin = new Vector2(0, 0.5f);
            checkBGRect.anchorMax = new Vector2(0, 0.5f);
            checkBGRect.pivot = new Vector2(0, 0.5f);
            checkBGRect.sizeDelta = new Vector2(28, 28);
            checkBGRect.anchoredPosition = new Vector2(16, 0);

            // Checkmark (hidden by default)
            _godModeCheckmark = new GameObject("Checkmark");
            _godModeCheckmark.transform.SetParent(checkBG.transform, false);
            var checkImg = _godModeCheckmark.AddComponent<Image>();
            checkImg.color = new Color(0.3f, 1f, 0.4f);
            var checkRect = _godModeCheckmark.GetComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.15f, 0.15f);
            checkRect.anchorMax = new Vector2(0.85f, 0.85f);
            checkRect.sizeDelta = Vector2.zero;
            _godModeCheckmark.SetActive(false);

            // Clickable area over checkbox + label
            var godBtn = godRow.AddComponent<Button>();
            var godBtnImg = godRow.AddComponent<Image>();
            godBtnImg.color = Color.clear;
            godBtn.targetGraphic = godBtnImg;
            godBtn.onClick.AddListener(() => {
                AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                ToggleGodMode();
            });

            // GOD MODE label
            var godLabel = CreateText(godRow.transform, "GOD MODE", 17, Color.white);
            var godLabelRect = godLabel.GetComponent<RectTransform>();
            godLabelRect.anchorMin = new Vector2(0, 0.5f);
            godLabelRect.anchorMax = new Vector2(1, 0.5f);
            godLabelRect.pivot = new Vector2(0, 0.5f);
            godLabelRect.anchoredPosition = new Vector2(54, 0);
            godLabelRect.sizeDelta = new Vector2(200, 28);
            godLabel.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

            dy -= ITEM_SPACING;

            // ── EPOCH SELECTOR button ──
            var devBtnW = panelW - 40f; // Inset from panel edges
            CreateDevPanelButton(_devPanel.transform, "EPOCH SELECTOR", dy,
                devBtnW, new Color(0.35f, 0.4f, 0.5f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _devPanel.SetActive(false);
                    if (_levelSelectorPanel != null)
                        _levelSelectorPanel.SetActive(true);
                });

            dy -= ITEM_SPACING;

            // ── SHOW LEVEL COMPLETE button ──
            CreateDevPanelButton(_devPanel.transform, "SHOW LEVEL COMPLETE", dy,
                devBtnW, new Color(0.35f, 0.5f, 0.35f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _devPanel.SetActive(false);
                    GameManager.Instance?.PreviewLevelComplete();
                });

            dy -= ITEM_SPACING;

            // ── SHOW CELEBRATION button ──
            CreateDevPanelButton(_devPanel.transform, "SHOW CELEBRATION", dy,
                devBtnW, new Color(0.5f, 0.4f, 0.2f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _devPanel.SetActive(false);
                    GameManager.Instance?.PreviewCelebration();
                });

            dy -= 40f;

            // Escape hint
            var hintGO = CreateText(_devPanel.transform, "Press Esc to close", 13, new Color(0.5f, 0.5f, 0.6f));
            var hintRect = hintGO.GetComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0.5f, 1);
            hintRect.anchorMax = new Vector2(0.5f, 1);
            hintRect.anchoredPosition = new Vector2(0, dy);

            // Size panel to fit contents
            float panelH = Mathf.Abs(dy) + 24f;
            panelRect.sizeDelta = new Vector2(panelW, panelH);
        }

        /// <summary>
        /// Helper: create a button anchored to the top of the dev panel at a given Y offset.
        /// Avoids the re-anchoring boilerplate that CreateMenuButton needs (it defaults to center anchor).
        /// </summary>
        private void CreateDevPanelButton(Transform parent, string text, float yPos,
            float width, Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("DevBtn");
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
            rect.anchorMin = new Vector2(0.5f, 1);
            rect.anchorMax = new Vector2(0.5f, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(width, 42);
            rect.anchoredPosition = new Vector2(0, yPos);

            // Pixel text label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(text, Color.white, 3);
            labelImg.preserveAspect = true;
            labelImg.raycastTarget = false;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(width - 16, 30);
            labelRect.anchoredPosition = Vector2.zero;
        }

        private void ToggleGodMode()
        {
            _godModeEnabled = !_godModeEnabled;
            CosmeticManager.GodMode = _godModeEnabled;

            // Update checkmark visual
            if (_godModeCheckmark != null)
                _godModeCheckmark.SetActive(_godModeEnabled);

            if (_godModeEnabled)
            {
                // Unlock all achievements
                AchievementManager.Instance?.UnlockAll();

                // Set unlimited lives
                GameManager.Instance?.SetUnlimitedLives();
            }
        }

        // =====================================================================
        // Helpers
        // =====================================================================

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

        private GameObject CreateMenuButton(Transform parent, string text, Vector2 position, Vector2 size,
            Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("MenuBtn");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null)
                btn.onClick.AddListener(onClick);

            // Hover/press color transitions for visual feedback
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

            // Pixel text label (raycastTarget off so clicks pass through to button)
            int scale = 3;
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(text, Color.white, scale);
            labelImg.preserveAspect = true;
            labelImg.raycastTarget = false;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(size.x - 16, size.y - 12);
            labelRect.anchoredPosition = Vector2.zero;
            return go;
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

            // Pixel text label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(text, Color.white, 3);
            labelImg.preserveAspect = true;
            labelImg.raycastTarget = false;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(size.x - 8, size.y - 8);
            labelRect.anchoredPosition = Vector2.zero;
        }

        private void CreatePixelLabel(Transform parent, string text, Vector2 position, Color color, int scale = 3)
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

        /// <summary>
        /// Create a labeled toggle row for settings panels (Sprint 9).
        /// </summary>
        private void CreateToggleRow(Transform parent, string label, Vector2 position, bool initialValue,
            UnityEngine.Events.UnityAction<bool> onValueChanged)
        {
            var containerGO = new GameObject("Toggle_" + label);
            containerGO.transform.SetParent(parent, false);
            var containerRect = containerGO.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(440, 40);
            containerRect.anchoredPosition = position;

            // Checkbox background
            var checkBgGO = new GameObject("CheckBg");
            checkBgGO.transform.SetParent(containerGO.transform, false);
            var checkBgImg = checkBgGO.AddComponent<Image>();
            checkBgImg.color = new Color(0.15f, 0.13f, 0.25f);
            var checkBgRect = checkBgGO.GetComponent<RectTransform>();
            checkBgRect.anchorMin = new Vector2(1, 0.5f);
            checkBgRect.anchorMax = new Vector2(1, 0.5f);
            checkBgRect.pivot = new Vector2(1, 0.5f);
            checkBgRect.sizeDelta = new Vector2(32, 32);
            checkBgRect.anchoredPosition = new Vector2(-8, 0);

            // Checkmark
            var checkmarkGO = new GameObject("Checkmark");
            checkmarkGO.transform.SetParent(checkBgGO.transform, false);
            var checkmarkText = checkmarkGO.AddComponent<Text>();
            checkmarkText.text = "X";
            checkmarkText.fontSize = 22;
            checkmarkText.color = new Color(0.4f, 0.8f, 0.4f);
            checkmarkText.alignment = TextAnchor.MiddleCenter;
            checkmarkText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            checkmarkText.fontStyle = FontStyle.Bold;
            var checkmarkRect = checkmarkGO.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = Vector2.zero;
            checkmarkRect.anchorMax = Vector2.one;
            checkmarkRect.sizeDelta = Vector2.zero;
            checkmarkGO.SetActive(initialValue);

            // Toggle component
            var toggle = checkBgGO.AddComponent<Toggle>();
            toggle.isOn = initialValue;
            toggle.targetGraphic = checkBgImg;
            toggle.graphic = checkmarkText;
            toggle.onValueChanged.AddListener((val) => {
                AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                onValueChanged.Invoke(val);
                checkmarkGO.SetActive(val);
            });

            // Label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(containerGO.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(label, new Color(0.85f, 0.85f, 0.95f), 3);
            labelImg.preserveAspect = true;
            labelImg.raycastTarget = false;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0.5f);
            labelRect.anchorMax = new Vector2(0, 0.5f);
            labelRect.pivot = new Vector2(0, 0.5f);
            labelRect.sizeDelta = new Vector2(label.Length * 18 + 12, 28);
            labelRect.anchoredPosition = new Vector2(8, 0);
        }

        private void CreateRandomizeToggle(Transform parent, Vector2 position)
        {
            bool isOn = GameManager.Instance != null && GameManager.Instance.RandomizeEnabled;

            // Container
            var containerGO = new GameObject("RandomizeToggle");
            containerGO.transform.SetParent(parent, false);
            var containerRect = containerGO.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(300, 40);
            containerRect.anchoredPosition = position;

            // Checkbox background
            var checkBgGO = new GameObject("CheckBg");
            checkBgGO.transform.SetParent(containerGO.transform, false);
            var checkBgImg = checkBgGO.AddComponent<Image>();
            checkBgImg.color = new Color(0.15f, 0.13f, 0.25f);
            var checkBgRect = checkBgGO.GetComponent<RectTransform>();
            checkBgRect.anchorMin = new Vector2(0, 0.5f);
            checkBgRect.anchorMax = new Vector2(0, 0.5f);
            checkBgRect.pivot = new Vector2(0, 0.5f);
            checkBgRect.sizeDelta = new Vector2(32, 32);
            checkBgRect.anchoredPosition = new Vector2(0, 0);

            // Checkmark
            var checkmarkGO = new GameObject("Checkmark");
            checkmarkGO.transform.SetParent(checkBgGO.transform, false);
            var checkmarkText = checkmarkGO.AddComponent<Text>();
            checkmarkText.text = "X";
            checkmarkText.fontSize = 22;
            checkmarkText.color = new Color(0.4f, 0.8f, 0.4f);
            checkmarkText.alignment = TextAnchor.MiddleCenter;
            checkmarkText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            checkmarkText.fontStyle = FontStyle.Bold;
            var checkmarkRect = checkmarkGO.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = Vector2.zero;
            checkmarkRect.anchorMax = Vector2.one;
            checkmarkRect.sizeDelta = Vector2.zero;
            checkmarkGO.SetActive(isOn);

            // Toggle component
            var toggle = checkBgGO.AddComponent<Toggle>();
            toggle.isOn = isOn;
            toggle.targetGraphic = checkBgImg;
            toggle.graphic = checkmarkText;
            toggle.onValueChanged.AddListener((val) => {
                AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                if (GameManager.Instance != null)
                    GameManager.Instance.RandomizeEnabled = val;
                checkmarkGO.SetActive(val);
            });

            // Toggle already handles clicks natively — no extra Button needed

            // Label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(containerGO.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite("RANDOMIZE", new Color(0.85f, 0.85f, 0.95f), 3);
            labelImg.preserveAspect = true;
            labelImg.raycastTarget = false;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0.5f);
            labelRect.anchorMax = new Vector2(0, 0.5f);
            labelRect.pivot = new Vector2(0, 0.5f);
            labelRect.sizeDelta = new Vector2(200, 28);
            labelRect.anchoredPosition = new Vector2(42, 0);

            // Description
            var descGO = CreateText(containerGO.transform, "OFF = Campaign, ON = FreePlay", 14,
                new Color(0.6f, 0.6f, 0.7f));
            var descRect = descGO.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0);
            descRect.anchorMax = new Vector2(1, 0);
            descRect.pivot = new Vector2(0.5f, 1);
            descRect.sizeDelta = new Vector2(300, 16);
            descRect.anchoredPosition = new Vector2(0, -2);
        }

        /// <summary>
        /// Creates a button with ornate gold double-border framing.
        /// Structure: OuterBorder (gold) → DarkGap → InnerBorder (gold) → DarkBg → Button + label.
        /// </summary>
        private void CreateOrnateButton(Transform parent, string text, Vector2 position,
            Vector2 size, UnityEngine.Events.UnityAction onClick, float opacity = 1f)
        {
            // Outer gold border
            var outerGO = new GameObject($"OrnateBtn_{text}");
            outerGO.transform.SetParent(parent, false);
            var outerImg = outerGO.AddComponent<Image>();
            outerImg.color = new Color(0.85f, 0.65f, 0.12f, 0.9f * opacity);
            var outerRect = outerGO.GetComponent<RectTransform>();
            outerRect.anchorMin = new Vector2(0.5f, 0.5f);
            outerRect.anchorMax = new Vector2(0.5f, 0.5f);
            outerRect.sizeDelta = size;
            outerRect.anchoredPosition = position;

            // Dark gap
            var gapGO = new GameObject("DarkGap");
            gapGO.transform.SetParent(outerGO.transform, false);
            var gapImg = gapGO.AddComponent<Image>();
            gapImg.color = new Color(0.06f, 0.04f, 0.10f, opacity);
            var gapRect = gapGO.GetComponent<RectTransform>();
            gapRect.anchorMin = Vector2.zero;
            gapRect.anchorMax = Vector2.one;
            gapRect.sizeDelta = new Vector2(-4, -4);

            // Inner gold border
            var innerGO = new GameObject("InnerBorder");
            innerGO.transform.SetParent(gapGO.transform, false);
            var innerImg = innerGO.AddComponent<Image>();
            innerImg.color = new Color(0.90f, 0.70f, 0.18f, 0.75f * opacity);
            var innerRect = innerGO.GetComponent<RectTransform>();
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.sizeDelta = new Vector2(-3, -3);

            // Dark button background
            var bgGO = new GameObject("ButtonBg");
            bgGO.transform.SetParent(innerGO.transform, false);
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.12f, 0.10f, 0.18f, 0.95f * opacity);
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = new Vector2(-3, -3);

            // Clickable button
            var btn = bgGO.AddComponent<Button>();
            btn.targetGraphic = bgImg;
            btn.onClick.AddListener(onClick);

            Color ornateBg = bgImg.color;
            var ornateColors = btn.colors;
            ornateColors.normalColor = ornateBg;
            ornateColors.highlightedColor = new Color(
                Mathf.Min(1f, ornateBg.r + 0.15f),
                Mathf.Min(1f, ornateBg.g + 0.15f),
                Mathf.Min(1f, ornateBg.b + 0.15f), ornateBg.a);
            ornateColors.pressedColor = new Color(
                ornateBg.r * 0.7f, ornateBg.g * 0.7f, ornateBg.b * 0.7f, ornateBg.a);
            ornateColors.selectedColor = ornateColors.normalColor;
            ornateColors.fadeDuration = 0.1f;
            btn.colors = ornateColors;

            // Pixel text label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(bgGO.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(text,
                new Color(1f * opacity, 0.85f * opacity, 0.2f * opacity + 0.2f * (1f - opacity)), 3);
            labelImg.preserveAspect = true;
            labelImg.raycastTarget = false;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = new Vector2(-12, -12);
        }
    }
}
