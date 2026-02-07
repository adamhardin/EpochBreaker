using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace EpochBreaker.UI
{
    /// <summary>
    /// Game Over screen. Shows mode-specific info:
    /// - Campaign: epoch reached
    /// - Streak: streak count (auto-saved to legends)
    /// - FreePlay: simple retry
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

            var gm = Gameplay.GameManager.Instance;
            var mode = gm?.CurrentGameMode ?? Gameplay.GameMode.FreePlay;

            // GAME OVER title
            CreateText(canvasGO.transform, "GAME OVER", 72,
                new Color(0.9f, 0.2f, 0.2f), new Vector2(0, 140));

            // Mode-specific info
            float yPos = 60f;

            switch (mode)
            {
                case Gameplay.GameMode.Campaign:
                    int epoch = gm?.CampaignEpoch ?? 0;
                    string epochName = gm != null ? gm.CurrentLevelID.EpochName : "Unknown";
                    CreateText(canvasGO.transform,
                        $"Campaign ended at Epoch {epoch + 1}: {epochName}", 28,
                        new Color(0.8f, 0.6f, 0.6f), new Vector2(0, yPos));
                    yPos -= 40f;
                    int totalScore = gm?.TotalScore ?? 0;
                    CreateText(canvasGO.transform,
                        $"Total Score: {totalScore:N0}", 24,
                        new Color(0.7f, 0.7f, 0.5f), new Vector2(0, yPos));
                    break;

                case Gameplay.GameMode.Streak:
                    int streak = gm?.StreakCount ?? 0;
                    CreateText(canvasGO.transform,
                        $"Streak: {streak} level{(streak != 1 ? "s" : "")} completed", 32,
                        new Color(1f, 0.85f, 0.2f), new Vector2(0, yPos));
                    yPos -= 40f;
                    CreateText(canvasGO.transform,
                        "Saved to Legends!", 22,
                        new Color(0.7f, 0.6f, 0.4f), new Vector2(0, yPos));
                    break;

                default: // FreePlay
                    CreateText(canvasGO.transform, "Out of lives!", 28,
                        new Color(0.8f, 0.6f, 0.6f), new Vector2(0, yPos));
                    break;
            }

            // Retry button
            CreateButton(canvasGO.transform, "RETRY", new Vector2(0, -60),
                new Color(0.3f, 0.5f, 0.3f), () => {
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.GameManager.Instance?.StartGame();
                });

            // Menu button
            CreateButton(canvasGO.transform, "MAIN MENU", new Vector2(0, -140),
                new Color(0.4f, 0.3f, 0.3f), () => {
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.GameManager.Instance?.ReturnToTitle();
                });

            // Keyboard hints
            CreateText(canvasGO.transform, "Enter: Retry | Esc/`: Menu", 16,
                new Color(0.5f, 0.4f, 0.4f), new Vector2(0, -230));
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            {
                Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                Gameplay.GameManager.Instance?.StartGame();
            }
            else if (Gameplay.InputManager.IsBackPressed())
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
            rect.sizeDelta = new Vector2(700, fontSize + 20);
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
