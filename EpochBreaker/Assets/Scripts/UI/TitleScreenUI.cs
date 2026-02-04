using UnityEngine;
using UnityEngine.UI;

namespace SixteenBit.UI
{
    /// <summary>
    /// Title screen with game logo, Play button, and settings.
    /// Creates its own Canvas and UI elements programmatically.
    /// </summary>
    public class TitleScreenUI : MonoBehaviour
    {
        private Canvas _canvas;

        private void Start()
        {
            CreateUI();
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

            // Title text
            var titleGO = CreateText(canvasGO.transform, "EPOCH BREAKER", 72, Color.white);
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 150);

            // Subtitle
            var subGO = CreateText(canvasGO.transform, "A 16-Bit Adventure Through Time", 24,
                new Color(0.7f, 0.7f, 0.8f));
            var subRect = subGO.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(0, 70);

            // Play button
            CreateButton(canvasGO.transform, "PLAY", new Vector2(0, -40), new Vector2(300, 70),
                new Color(0.2f, 0.6f, 0.3f), () => {
                    Gameplay.GameManager.Instance?.StartGame();
                });

            // Era/Difficulty info
            var infoGO = CreateText(canvasGO.transform,
                $"Era: {GetCurrentEraName()} | Difficulty: {GetCurrentDifficultyName()}", 20,
                new Color(0.6f, 0.6f, 0.7f));
            var infoRect = infoGO.GetComponent<RectTransform>();
            infoRect.anchoredPosition = new Vector2(0, -130);

            // Controls hint
            var hintGO = CreateText(canvasGO.transform,
                "Arrow Keys: Move | Space: Jump | X: Target Cycle | Esc: Pause", 16,
                new Color(0.5f, 0.5f, 0.6f));
            var hintRect = hintGO.GetComponent<RectTransform>();
            hintRect.anchoredPosition = new Vector2(0, -200);

            // Version
            var verGO = CreateText(canvasGO.transform, "v0.1.0 - Prototype", 14,
                new Color(0.4f, 0.4f, 0.5f));
            var verRect = verGO.GetComponent<RectTransform>();
            verRect.anchorMin = new Vector2(0, 0);
            verRect.anchorMax = new Vector2(0, 0);
            verRect.pivot = new Vector2(0, 0);
            verRect.anchoredPosition = new Vector2(20, 20);
        }

        private string GetCurrentEraName()
        {
            var gm = Gameplay.GameManager.Instance;
            if (gm == null) return "Stone Age";
            return gm.CurrentEra switch
            {
                0 => "Stone Age", 1 => "Bronze Age", 2 => "Classical",
                3 => "Medieval", 4 => "Renaissance", 5 => "Industrial",
                6 => "Modern", 7 => "Digital", 8 => "Spacefaring",
                9 => "Transcendent", _ => "Unknown"
            };
        }

        private string GetCurrentDifficultyName()
        {
            var gm = Gameplay.GameManager.Instance;
            if (gm == null) return "Easy";
            return gm.CurrentDifficulty switch
            {
                0 => "Easy", 1 => "Normal", 2 => "Hard", 3 => "Extreme", _ => "Unknown"
            };
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

        private void CreateButton(Transform parent, string text, Vector2 position, Vector2 size,
            Color bgColor, UnityEngine.Events.UnityAction onClick)
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

            // Button text
            var textGO = new GameObject("Label");
            textGO.transform.SetParent(go.transform, false);

            var textComp = textGO.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = 32;
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
