using UnityEngine;
using UnityEngine.UI;

namespace SixteenBit.UI
{
    /// <summary>
    /// Pause menu overlay with Resume, Restart, and Quit buttons.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        private void Start()
        {
            CreateUI();
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

            // Title
            CreateText(canvasGO.transform, "PAUSED", 48, Color.white, new Vector2(0, 120));

            // Resume button
            CreateButton(canvasGO.transform, "RESUME", new Vector2(0, 30),
                new Color(0.2f, 0.5f, 0.3f), () => {
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.GameManager.Instance?.ResumeGame();
                });

            // Restart button
            CreateButton(canvasGO.transform, "RESTART", new Vector2(0, -50),
                new Color(0.5f, 0.4f, 0.2f), () => {
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.GameManager.Instance?.RestartLevel();
                });

            // Quit to Menu
            CreateButton(canvasGO.transform, "QUIT TO MENU", new Vector2(0, -130),
                new Color(0.5f, 0.2f, 0.2f), () => {
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.GameManager.Instance?.ReturnToTitle();
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
