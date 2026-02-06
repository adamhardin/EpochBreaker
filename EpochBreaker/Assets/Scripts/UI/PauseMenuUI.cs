using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using EpochBreaker.Gameplay;

namespace EpochBreaker.UI
{
    /// <summary>
    /// Pause menu overlay with Resume, Restart, Settings, and Quit buttons.
    /// Settings sub-panel provides audio volume controls.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        private GameObject _mainContent;
        private GameObject _settingsContent;

        private void Start()
        {
            CreateUI();
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                // If settings is open, go back to main pause menu
                // and consume PausePressed so GameManager doesn't also resume
                if (_settingsContent != null && _settingsContent.activeSelf)
                {
                    _settingsContent.SetActive(false);
                    _mainContent.SetActive(true);
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

            // Settings title
            CreateText(panelGO.transform, "AUDIO SETTINGS", 36, new Color(1f, 0.85f, 0.1f), new Vector2(0, 170));

            // Back button
            CreateButton(panelGO.transform, "BACK", new Vector2(-190, 170),
                new Color(0.4f, 0.4f, 0.5f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _settingsContent.SetActive(false);
                    _mainContent.SetActive(true);
                }, new Vector2(100, 40), 20);

            // Divider
            var divGO = new GameObject("Divider");
            divGO.transform.SetParent(panelGO.transform, false);
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

            CreateVolumeSlider(panelGO.transform, "BACKGROUND MUSIC", new Vector2(0, 75), musicVol,
                (val) => { if (AudioManager.Instance != null) AudioManager.Instance.MusicVolume = val; });

            CreateVolumeSlider(panelGO.transform, "SOUND EFFECTS", new Vector2(0, 0), sfxVol,
                (val) => { if (AudioManager.Instance != null) AudioManager.Instance.SFXVolume = val; });

            CreateVolumeSlider(panelGO.transform, "WEAPON FIRE", new Vector2(0, -75), weaponVol,
                (val) => { if (AudioManager.Instance != null) AudioManager.Instance.WeaponVolume = val; });

            // Hint
            CreateText(panelGO.transform, "Press Esc to go back", 14,
                new Color(0.5f, 0.5f, 0.6f), new Vector2(0, -175));

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

        private void CreateText(Transform parent, string text, int fontSize, Color color, Vector2 position)
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
    }
}
