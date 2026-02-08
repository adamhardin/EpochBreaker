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
        private Text _errorText;
        private Text _previewText;
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

            // Title
            CreateText(_panel.transform, "CHALLENGE A FRIEND", 32,
                new Color(1f, 0.85f, 0.1f), new Vector2(0, 170));

            // Instructions
            CreateText(_panel.transform, "Enter a challenge code from a friend", 18,
                new Color(0.7f, 0.7f, 0.8f), new Vector2(0, 130));

            // Format hint
            CreateText(_panel.transform, "Format: E-XXXXXXXX:SCORE  (e.g. 3-K7XM2P9A:12450)", 14,
                new Color(0.5f, 0.5f, 0.6f), new Vector2(0, 100));
            CreateText(_panel.transform, "Or just a level code: E-XXXXXXXX", 14,
                new Color(0.5f, 0.5f, 0.6f), new Vector2(0, 78));

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

            // Input field text
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

            // Placeholder
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

            // Error text (hidden by default)
            var errorGO = CreateText(_panel.transform, "Invalid challenge code", 18,
                new Color(1f, 0.3f, 0.3f), new Vector2(0, -20));
            _errorText = errorGO.GetComponent<Text>();
            errorGO.SetActive(false);

            // Preview text (shows parsed level info, hidden by default)
            var previewGO = CreateText(_panel.transform, "", 18,
                new Color(0.5f, 0.8f, 1f), new Vector2(0, -20));
            _previewText = previewGO.GetComponent<Text>();
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

            // Escape hint
            CreateText(_panel.transform, "Press Esc/` to close", 14,
                new Color(0.5f, 0.5f, 0.6f), new Vector2(0, -155));

            // Focus input field
            _codeInput.ActivateInputField();
        }

        private void OnInputChanged(string text)
        {
            _errorText.gameObject.SetActive(false);

            if (string.IsNullOrEmpty(text))
            {
                _previewText.gameObject.SetActive(false);
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
                    _previewText.text = preview;
                    _previewText.gameObject.SetActive(true);
                }
            }
            else
            {
                _previewText.gameObject.SetActive(false);
            }
        }

        private void SubmitChallengeCode()
        {
            string code = _codeInput.text.Trim().ToUpper();
            if (string.IsNullOrEmpty(code))
            {
                _errorText.text = "Please enter a challenge code";
                _errorText.gameObject.SetActive(true);
                _previewText.gameObject.SetActive(false);
                return;
            }

            if (!LevelShareManager.TryParseChallengeCode(code, out string levelCode, out int friendScore))
            {
                _errorText.text = $"Invalid code: {code}";
                _errorText.gameObject.SetActive(true);
                _previewText.gameObject.SetActive(false);
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

        private GameObject CreateText(Transform parent, string text, int fontSize,
            Color color, Vector2 position)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);

            var textComp = go.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = fontSize;
            textComp.color = color;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.horizontalOverflow = HorizontalWrapMode.Wrap;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(460, fontSize + 20);
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

            var textGO = new GameObject("Label");
            textGO.transform.SetParent(go.transform, false);
            var textComp = textGO.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = 22;
            textComp.color = Color.white;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return btn;
        }
    }
}
