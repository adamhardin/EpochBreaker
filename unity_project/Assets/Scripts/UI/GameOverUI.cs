using UnityEngine;
using UnityEngine.UI;

namespace EpochBreaker.UI
{
    /// <summary>
    /// Game Over screen shown when player runs out of lives.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        private void Start()
        {
            CreateUI();
        }

        private void CreateUI()
        {
            var canvasGO = new GameObject("GameOverCanvas");
            canvasGO.transform.SetParent(transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Full-screen dark background
            var overlayGO = new GameObject("Overlay");
            overlayGO.transform.SetParent(canvasGO.transform, false);
            var overlayImg = overlayGO.AddComponent<Image>();
            overlayImg.color = new Color(0.15f, 0.05f, 0.05f, 0.95f);
            var overlayRect = overlayGO.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            // GAME OVER title
            CreateText(canvasGO.transform, "GAME OVER", 72,
                new Color(0.9f, 0.2f, 0.2f), new Vector2(0, 100));

            // Subtitle
            CreateText(canvasGO.transform, "Out of lives!", 28,
                new Color(0.8f, 0.6f, 0.6f), new Vector2(0, 20));

            // Retry button
            CreateButton(canvasGO.transform, "RETRY LEVEL", new Vector2(0, -80),
                new Color(0.3f, 0.5f, 0.3f), () => {
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.GameManager.Instance?.RestartLevel();
                });

            // Menu button
            CreateButton(canvasGO.transform, "MAIN MENU", new Vector2(0, -160),
                new Color(0.4f, 0.3f, 0.3f), () => {
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.GameManager.Instance?.ReturnToTitle();
                });

            // Keyboard hints
            CreateText(canvasGO.transform, "R: Retry | Esc: Menu", 16,
                new Color(0.5f, 0.4f, 0.4f), new Vector2(0, -260));
        }

        private void Update()
        {
            // Keyboard shortcuts
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return))
            {
                Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                Gameplay.GameManager.Instance?.RestartLevel();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                Gameplay.GameManager.Instance?.ReturnToTitle();
            }
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
            rect.sizeDelta = new Vector2(600, fontSize + 20);
            rect.anchoredPosition = position;
        }

        private void CreateButton(Transform parent, string text, Vector2 position,
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
            rect.sizeDelta = new Vector2(280, 60);
            rect.anchoredPosition = position;

            var textGO = new GameObject("Label");
            textGO.transform.SetParent(go.transform, false);
            var textComp = textGO.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = 26;
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
