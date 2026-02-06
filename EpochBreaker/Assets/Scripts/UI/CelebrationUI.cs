using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace EpochBreaker.UI
{
    /// <summary>
    /// Celebration screen shown after completing all 10 campaign epochs.
    /// Hero earns the title "EPOCH BREAKER" and can enter Streak Mode.
    /// </summary>
    public class CelebrationUI : MonoBehaviour
    {
        private void Start()
        {
            CreateUI();
        }

        private void Update()
        {
            if (Keyboard.current == null) return;
            if (Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                EnterStreakMode();
            }
            else if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                Gameplay.GameManager.Instance?.ReturnToTitle();
            }
        }

        private void EnterStreakMode()
        {
            Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
            Gameplay.GameManager.Instance?.StartStreakMode();
        }

        private void CreateUI()
        {
            var canvasGO = new GameObject("CelebrationCanvas");
            canvasGO.transform.SetParent(transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 130;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Full-screen dark background with golden tint
            var overlayGO = new GameObject("Overlay");
            overlayGO.transform.SetParent(canvasGO.transform, false);
            var overlayImg = overlayGO.AddComponent<Image>();
            overlayImg.color = new Color(0.08f, 0.06f, 0.02f, 0.95f);
            var overlayRect = overlayGO.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            // Player sprite (hero portrait)
            var playerGO = new GameObject("PlayerSprite");
            playerGO.transform.SetParent(canvasGO.transform, false);
            var playerImg = playerGO.AddComponent<RawImage>();
            var playerSprite = Gameplay.PlaceholderAssets.GetPlayerSprite();
            playerImg.texture = playerSprite.texture;
            playerImg.color = Color.white;
            var playerRect = playerGO.GetComponent<RectTransform>();
            playerRect.anchorMin = new Vector2(0.5f, 0.5f);
            playerRect.anchorMax = new Vector2(0.5f, 0.5f);
            playerRect.sizeDelta = new Vector2(200, 200);
            playerRect.anchoredPosition = new Vector2(0, 220);

            // Title earned text
            CreatePixelText(canvasGO.transform, "EPOCH BREAKER", 4f,
                new Color(1f, 0.85f, 0.2f), new Vector2(0, 80));

            // Subtitle
            CreateText(canvasGO.transform,
                "You have conquered all ten epochs.", 28,
                new Color(0.9f, 0.8f, 0.5f), new Vector2(0, 20));

            CreateText(canvasGO.transform,
                "Humanity is saved... for now.", 24,
                new Color(0.7f, 0.6f, 0.4f), new Vector2(0, -20));

            // Narrative
            CreateText(canvasGO.transform,
                "The timelines fracture once more. As EPOCH BREAKER,\nyou must now defend humanity across infinite timelines.", 20,
                new Color(0.6f, 0.55f, 0.4f), new Vector2(0, -80));

            // Streak mode info
            CreateText(canvasGO.transform,
                "STREAK MODE: 10 lives. No extra lives. How far can you go?", 22,
                new Color(0.8f, 0.5f, 0.2f), new Vector2(0, -150));

            // Enter the Breach button
            CreateButton(canvasGO.transform, "ENTER THE BREACH", new Vector2(0, -230),
                new Color(0.6f, 0.45f, 0.1f), new Vector2(360, 70), EnterStreakMode);

            // Menu button
            CreateButton(canvasGO.transform, "MAIN MENU", new Vector2(0, -310),
                new Color(0.3f, 0.25f, 0.15f), new Vector2(280, 50), () => {
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.GameManager.Instance?.ReturnToTitle();
                });

            // Keyboard hints
            CreateText(canvasGO.transform, "Enter: Enter the Breach | Esc: Main Menu", 16,
                new Color(0.4f, 0.35f, 0.25f), new Vector2(0, -380));
        }

        private void CreatePixelText(Transform parent, string text, float scale, Color color, Vector2 position)
        {
            var go = new GameObject("PixelTitle");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<RawImage>();
            var sprite = Gameplay.PlaceholderAssets.GetPixelTextSprite(text, color);
            img.texture = sprite.texture;
            img.color = Color.white;

            float w = sprite.rect.width * scale;
            float h = sprite.rect.height * scale;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(w, h);
            rect.anchoredPosition = position;
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
            rect.sizeDelta = new Vector2(800, fontSize * 3 + 20);
            rect.anchoredPosition = position;
        }

        private void CreateButton(Transform parent, string text, Vector2 position,
            Color bgColor, Vector2 size, UnityEngine.Events.UnityAction onClick)
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
            textComp.fontSize = 28;
            textComp.color = Color.white;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontStyle = FontStyle.Bold;
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }
    }
}
