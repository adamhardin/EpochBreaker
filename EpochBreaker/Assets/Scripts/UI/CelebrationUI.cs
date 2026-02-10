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
            else if (Gameplay.InputManager.IsBackPressed())
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
            canvas.pixelPerfect = true;

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
                "THE TIMELINES FRACTURE ONCE MORE.", 20,
                new Color(0.6f, 0.55f, 0.4f), new Vector2(0, -70));
            CreateText(canvasGO.transform,
                "YOU MUST DEFEND HUMANITY ACROSS INFINITE TIMELINES.", 20,
                new Color(0.6f, 0.55f, 0.4f), new Vector2(0, -100));

            // Streak mode info
            CreateText(canvasGO.transform,
                "STREAK MODE: 10 LIVES. HOW FAR CAN YOU GO?", 22,
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
            CreateText(canvasGO.transform, "ENTER: BREACH | ESC: MENU", 16,
                new Color(0.4f, 0.35f, 0.25f), new Vector2(0, -380));
        }

        private void CreatePixelText(Transform parent, string text, float scale, Color color, Vector2 position)
        {
            var go = new GameObject("PixelTitle");
            go.transform.SetParent(parent, false);

            int intScale = Mathf.Min(6, Mathf.RoundToInt(scale * 3f));
            var img = go.AddComponent<Image>();
            img.sprite = Gameplay.PlaceholderAssets.GetPixelTextSprite(text, color, intScale);
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.SetNativeSize();

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
        }

        private void CreateText(Transform parent, string text, int fontSize, Color color, Vector2 position)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);

            int scale = fontSize >= 72 ? 10 : fontSize >= 38 ? 6 : fontSize >= 30 ? 5 : fontSize >= 24 ? 4 : fontSize >= 16 ? 3 : 2;
            var img = go.AddComponent<Image>();
            img.sprite = Gameplay.PlaceholderAssets.GetPixelTextSprite(text, color, scale);
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.SetNativeSize();

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
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
            var labelImg = textGO.AddComponent<Image>();
            labelImg.sprite = Gameplay.PlaceholderAssets.GetPixelTextSprite(text, Color.white, 3);
            labelImg.preserveAspect = true;
            labelImg.raycastTarget = false;
            labelImg.SetNativeSize();
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
        }
    }
}
