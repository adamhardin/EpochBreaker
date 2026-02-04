using UnityEngine;
using UnityEngine.UI;

namespace SixteenBit.UI
{
    /// <summary>
    /// Level complete screen showing score, era info, and next/replay/menu buttons.
    /// </summary>
    public class LevelCompleteUI : MonoBehaviour
    {
        private void Start()
        {
            CreateUI();
        }

        private void CreateUI()
        {
            var canvasGO = new GameObject("LevelCompleteCanvas");
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
            overlayImg.color = new Color(0, 0, 0, 0.6f);
            var overlayRect = overlayGO.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            // Title
            CreateText(canvasGO.transform, "LEVEL COMPLETE!", 52,
                new Color(1f, 0.85f, 0.1f), new Vector2(0, 180));

            // Score
            int score = Gameplay.GameManager.Instance?.Score ?? 0;
            CreateText(canvasGO.transform, $"Score: {score}", 32,
                Color.white, new Vector2(0, 100));

            // Level ID
            var gm = Gameplay.GameManager.Instance;
            string levelInfo = "";
            if (gm != null)
                levelInfo = $"{gm.CurrentLevelID.EraName} - {gm.CurrentLevelID.DifficultyName}";
            CreateText(canvasGO.transform, levelInfo, 24,
                new Color(0.7f, 0.7f, 0.8f), new Vector2(0, 50));

            // Stars placeholder
            CreateText(canvasGO.transform, "*** ", 40,
                new Color(1f, 0.85f, 0.1f), new Vector2(0, 0));

            // Next Level button
            CreateButton(canvasGO.transform, "NEXT LEVEL", new Vector2(0, -80),
                new Color(0.2f, 0.6f, 0.3f), () => {
                    Gameplay.GameManager.Instance?.NextLevel();
                });

            // Replay button
            CreateButton(canvasGO.transform, "REPLAY", new Vector2(0, -160),
                new Color(0.3f, 0.4f, 0.6f), () => {
                    Gameplay.GameManager.Instance?.RestartLevel();
                });

            // Menu button
            CreateButton(canvasGO.transform, "MENU", new Vector2(0, -240),
                new Color(0.4f, 0.3f, 0.3f), () => {
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
            rect.sizeDelta = new Vector2(500, fontSize + 20);
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
            rect.sizeDelta = new Vector2(260, 55);
            rect.anchoredPosition = position;

            var textGO = new GameObject("Label");
            textGO.transform.SetParent(go.transform, false);
            var textComp = textGO.AddComponent<Text>();
            textComp.text = text;
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
