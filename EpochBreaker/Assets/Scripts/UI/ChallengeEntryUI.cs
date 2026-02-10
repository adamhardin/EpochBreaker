using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using EpochBreaker.Generative;
using EpochBreaker.Gameplay;

namespace EpochBreaker.UI
{
    /// <summary>
    /// UI overlay for entering a friend's challenge code from the title screen.
    /// Parses challenge codes (format: "levelCode:score") to extract the level seed
    /// and friend's target score, then starts the level with that target displayed.
    /// </summary>
    public class ChallengeEntryUI : MonoBehaviour
    {
        private Canvas _canvas;
        private GameObject _panel;
        private InputField _codeInput;
        private Image _errorImg;
        private Sprite _errorSprite;
        private Image _previewImg;
        private Sprite _previewSprite;
        private System.Action _onClose;

        public void Initialize(System.Action onClose)
        {
            _onClose = onClose;
            CreateUI();
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            {
                SubmitChallengeCode();
            }
            else if (InputManager.IsBackPressed())
            {
                Close();
            }
        }

        private void CreateUI()
        {
            // Canvas
            var canvasGO = new GameObject("ChallengeCanvas");
            canvasGO.transform.SetParent(transform);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 120;
            _canvas.pixelPerfect = true;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Full-screen dimmer
            var dimGO = new GameObject("Dimmer");
            dimGO.transform.SetParent(canvasGO.transform, false);
            var dimImg = dimGO.AddComponent<Image>();
            dimImg.color = new Color(0f, 0f, 0f, 0.85f);
            var dimRect = dimGO.GetComponent<RectTransform>();
            dimRect.anchorMin = Vector2.zero;
            dimRect.anchorMax = Vector2.one;
            dimRect.sizeDelta = Vector2.zero;

            // Panel
            _panel = new GameObject("ChallengePanel");
            _panel.transform.SetParent(canvasGO.transform, false);
            var panelImg = _panel.AddComponent<Image>();
            panelImg.color = new Color(0.12f, 0.10f, 0.20f, 0.98f);
            var panelRect = _panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(580, 420);
            panelRect.anchoredPosition = Vector2.zero;

            // Title (fontSize 32 → scale 5)
            CreatePixelLabel(_panel.transform, "CHALLENGE A FRIEND",
                new Color(1f, 0.85f, 0.1f), 5, new Vector2(0, 170));

            // Instructions (fontSize 18 → scale 3)
            CreatePixelLabel(_panel.transform, "Enter a challenge code from a friend",
                new Color(0.7f, 0.7f, 0.8f), 3, new Vector2(0, 130));

            // Format hint (fontSize 14 → scale 2)
            CreatePixelLabel(_panel.transform, "Format: E-XXXXXXXX:SCORE  (e.g. 3-K7XM2P9A:12450)",
                new Color(0.5f, 0.5f, 0.6f), 2, new Vector2(0, 100));
            CreatePixelLabel(_panel.transform, "Or just a level code: E-XXXXXXXX",
                new Color(0.5f, 0.5f, 0.6f), 2, new Vector2(0, 72));

            // Close button
            CreateButton(_panel.transform, "X", new Vector2(240, 170),
                new Vector2(40, 40), new Color(0.6f, 0.25f, 0.25f), Close);

            // Input field background
            var inputBgGO = new GameObject("InputBg");
            inputBgGO.transform.SetParent(_panel.transform, false);
            var inputBgImg = inputBgGO.AddComponent<Image>();
            inputBgImg.color = new Color(0.06f, 0.05f, 0.12f, 1f);
            var inputBgRect = inputBgGO.GetComponent<RectTransform>();
            inputBgRect.anchorMin = new Vector2(0.5f, 0.5f);
            inputBgRect.anchorMax = new Vector2(0.5f, 0.5f);
            inputBgRect.sizeDelta = new Vector2(420, 55);
            inputBgRect.anchoredPosition = new Vector2(0, 25);

            // Input field text (KEEP legacy Text - required by InputField)
            var inputTextGO = new GameObject("Text");
            inputTextGO.transform.SetParent(inputBgGO.transform, false);
            var inputText = inputTextGO.AddComponent<Text>();
            inputText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            inputText.fontSize = 24;
            inputText.color = Color.white;
            inputText.alignment = TextAnchor.MiddleCenter;
            inputText.supportRichText = false;
            var inputTextRect = inputTextGO.GetComponent<RectTransform>();
            inputTextRect.anchorMin = new Vector2(0.05f, 0f);
            inputTextRect.anchorMax = new Vector2(0.95f, 1f);
            inputTextRect.sizeDelta = Vector2.zero;

            // Placeholder (KEEP legacy Text - required by InputField)
            var phGO = new GameObject("Placeholder");
            phGO.transform.SetParent(inputBgGO.transform, false);
            var phText = phGO.AddComponent<Text>();
            phText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            phText.fontSize = 24;
            phText.color = new Color(0.35f, 0.35f, 0.45f);
            phText.alignment = TextAnchor.MiddleCenter;
            phText.fontStyle = FontStyle.Italic;
            phText.text = "3-K7XM2P9A:12450";
            var phRect = phGO.GetComponent<RectTransform>();
            phRect.anchorMin = new Vector2(0.05f, 0f);
            phRect.anchorMax = new Vector2(0.95f, 1f);
            phRect.sizeDelta = Vector2.zero;

            // Input field component
            _codeInput = inputBgGO.AddComponent<InputField>();
            _codeInput.textComponent = inputText;
            _codeInput.placeholder = phText;
            _codeInput.characterLimit = 20;
            _codeInput.contentType = InputField.ContentType.Custom;
            _codeInput.characterValidation = InputField.CharacterValidation.None;
            _codeInput.onValidateInput += (text, index, ch) =>
            {
                ch = char.ToUpper(ch);
                if (char.IsDigit(ch) || (ch >= 'A' && ch <= 'Z') || ch == '-' || ch == ':')
                    return ch;
                return '\0';
            };
            _codeInput.onValueChanged.AddListener(OnInputChanged);

            // Error image (hidden by default, fontSize 18 → scale 3)
            var errorGO = new GameObject("ErrorImg");
            errorGO.transform.SetParent(_panel.transform, false);
            _errorImg = errorGO.AddComponent<Image>();
            _errorSprite = Gameplay.PlaceholderAssets.CreatePixelTextSprite("Invalid challenge code", new Color(1f, 0.3f, 0.3f), 3);
            _errorImg.sprite = _errorSprite;
            _errorImg.preserveAspect = true;
            _errorImg.raycastTarget = false;
            _errorImg.SetNativeSize();
            var errorRect = errorGO.GetComponent<RectTransform>();
            errorRect.anchorMin = new Vector2(0.5f, 0.5f);
            errorRect.anchorMax = new Vector2(0.5f, 0.5f);
            errorRect.pivot = new Vector2(0.5f, 0.5f);
            errorRect.anchoredPosition = new Vector2(0, -20);
            errorGO.SetActive(false);

            // Preview image (shows parsed level info, hidden by default, fontSize 18 → scale 3)
            var previewGO = new GameObject("PreviewImg");
            previewGO.transform.SetParent(_panel.transform, false);
            _previewImg = previewGO.AddComponent<Image>();
            _previewImg.sprite = null;
            _previewImg.preserveAspect = true;
            _previewImg.raycastTarget = false;
            var previewRect = previewGO.GetComponent<RectTransform>();
            previewRect.anchorMin = new Vector2(0.5f, 0.5f);
            previewRect.anchorMax = new Vector2(0.5f, 0.5f);
            previewRect.pivot = new Vector2(0.5f, 0.5f);
            previewRect.anchoredPosition = new Vector2(0, -20);
            previewGO.SetActive(false);

            // Play Challenge button
            CreateButton(_panel.transform, "PLAY CHALLENGE", new Vector2(0, -75),
                new Vector2(280, 55), new Color(0.2f, 0.55f, 0.3f), SubmitChallengeCode);

#if !UNITY_WEBGL || UNITY_EDITOR
            // Paste from clipboard
            CreateButton(_panel.transform, "PASTE", new Vector2(240, 25),
                new Vector2(80, 40), new Color(0.35f, 0.4f, 0.5f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    string clipboard = GUIUtility.systemCopyBuffer;
                    if (!string.IsNullOrEmpty(clipboard))
                    {
                        if (clipboard.Length > 20) clipboard = clipboard.Substring(0, 20);
                        _codeInput.text = clipboard.ToUpper();
                    }
                });
#endif

            // Escape hint (fontSize 14 → scale 2, replace backtick with ESC)
            CreatePixelLabel(_panel.transform, "Press ESC to close",
                new Color(0.5f, 0.5f, 0.6f), 2, new Vector2(0, -155));

            // Focus input field
            _codeInput.ActivateInputField();
        }

        private void OnInputChanged(string text)
        {
            _errorImg.gameObject.SetActive(false);

            if (string.IsNullOrEmpty(text))
            {
                _previewImg.gameObject.SetActive(false);
                return;
            }

            if (LevelShareManager.TryParseChallengeCode(text.Trim().ToUpper(),
                out string levelCode, out int friendScore))
            {
                if (LevelID.TryParse(levelCode, out LevelID id))
                {
                    string epochName = id.EpochName;
                    string preview = $"Level: {levelCode} ({epochName})";
                    if (friendScore > 0)
                        preview += $" | Target: {friendScore:N0}";
                    if (_previewSprite != null) { Destroy(_previewSprite.texture); Destroy(_previewSprite); }
                    _previewSprite = Gameplay.PlaceholderAssets.CreatePixelTextSprite(preview, new Color(0.5f, 0.8f, 1f), 3);
                    _previewImg.sprite = _previewSprite;
                    _previewImg.SetNativeSize();
                    _previewImg.gameObject.SetActive(true);
                }
            }
            else
            {
                _previewImg.gameObject.SetActive(false);
            }
        }

        private void SubmitChallengeCode()
        {
            string code = _codeInput.text.Trim().ToUpper();
            if (string.IsNullOrEmpty(code))
            {
                if (_errorSprite != null) { Destroy(_errorSprite.texture); Destroy(_errorSprite); }
                _errorSprite = Gameplay.PlaceholderAssets.CreatePixelTextSprite("Please enter a challenge code", new Color(1f, 0.3f, 0.3f), 3);
                _errorImg.sprite = _errorSprite;
                _errorImg.SetNativeSize();
                _errorImg.gameObject.SetActive(true);
                _previewImg.gameObject.SetActive(false);
                return;
            }

            if (!LevelShareManager.TryParseChallengeCode(code, out string levelCode, out int friendScore))
            {
                if (_errorSprite != null) { Destroy(_errorSprite.texture); Destroy(_errorSprite); }
                _errorSprite = Gameplay.PlaceholderAssets.CreatePixelTextSprite($"Invalid code: {code}", new Color(1f, 0.3f, 0.3f), 3);
                _errorImg.sprite = _errorSprite;
                _errorImg.SetNativeSize();
                _errorImg.gameObject.SetActive(true);
                _previewImg.gameObject.SetActive(false);
                return;
            }

            AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());

            // Set the friend challenge target in GameManager
            GameManager.Instance?.StartFriendChallenge(levelCode, friendScore);

            Close();
        }

        private void Close()
        {
            AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
            _onClose?.Invoke();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (_errorSprite != null) { Destroy(_errorSprite.texture); Destroy(_errorSprite); }
            if (_previewSprite != null) { Destroy(_previewSprite.texture); Destroy(_previewSprite); }
        }

        private GameObject CreatePixelLabel(Transform parent, string text,
            Color color, int scale, Vector2 position)
        {
            var go = new GameObject("PixelLabel");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.sprite = Gameplay.PlaceholderAssets.GetPixelTextSprite(text, color, scale);
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.SetNativeSize();

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;

            return go;
        }

        private Button CreateButton(Transform parent, string text, Vector2 position,
            Vector2 size, Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
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

            // Button label (fontSize 22 → scale 3)
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = Gameplay.PlaceholderAssets.GetPixelTextSprite(text, Color.white, 3);
            labelImg.preserveAspect = true;
            labelImg.raycastTarget = false;
            labelImg.SetNativeSize();
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;

            return btn;
        }
    }
}
