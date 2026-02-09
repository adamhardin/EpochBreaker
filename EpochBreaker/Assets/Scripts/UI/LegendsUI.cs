using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace EpochBreaker.UI
{
    /// <summary>
    /// Legends leaderboard overlay showing best streak runs.
    /// Accessible from the title screen once unlocked.
    /// </summary>
    public class LegendsUI : MonoBehaviour
    {
        private GameObject _canvasGO;
        private System.Action _onClose;

        public void Initialize(System.Action onClose)
        {
            _onClose = onClose;
            CreateUI();
        }

        private void Update()
        {
            if (Keyboard.current == null) return;
            if (Gameplay.InputManager.IsBackPressed())
            {
                Close();
            }
        }

        private void Close()
        {
            Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
            _onClose?.Invoke();
            Destroy(gameObject);
        }

        private void CreateUI()
        {
            _canvasGO = new GameObject("LegendsCanvas");
            _canvasGO.transform.SetParent(transform);
            var canvas = _canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 140;
            canvas.pixelPerfect = true;

            var scaler = _canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            _canvasGO.AddComponent<GraphicRaycaster>();

            // Full-screen dark overlay
            var overlayGO = new GameObject("Overlay");
            overlayGO.transform.SetParent(_canvasGO.transform, false);
            var overlayImg = overlayGO.AddComponent<Image>();
            overlayImg.color = new Color(0.05f, 0.04f, 0.10f, 0.95f);
            var overlayRect = overlayGO.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            // Outer gold border
            var outerBorderGO = new GameObject("OuterBorder");
            outerBorderGO.transform.SetParent(_canvasGO.transform, false);
            var outerBorderImg = outerBorderGO.AddComponent<Image>();
            outerBorderImg.color = new Color(0.85f, 0.65f, 0.12f, 0.9f);
            var outerBorderRect = outerBorderGO.GetComponent<RectTransform>();
            outerBorderRect.anchorMin = new Vector2(0.5f, 0.5f);
            outerBorderRect.anchorMax = new Vector2(0.5f, 0.5f);
            outerBorderRect.sizeDelta = new Vector2(724, 624);
            outerBorderRect.anchoredPosition = Vector2.zero;

            // Dark gap between borders
            var borderGapGO = new GameObject("BorderGap");
            borderGapGO.transform.SetParent(outerBorderGO.transform, false);
            var borderGapImg = borderGapGO.AddComponent<Image>();
            borderGapImg.color = new Color(0.06f, 0.04f, 0.10f);
            var borderGapRect = borderGapGO.GetComponent<RectTransform>();
            borderGapRect.anchorMin = Vector2.zero;
            borderGapRect.anchorMax = Vector2.one;
            borderGapRect.sizeDelta = new Vector2(-6, -6);
            borderGapRect.anchoredPosition = Vector2.zero;

            // Inner gold border
            var innerBorderGO = new GameObject("InnerBorder");
            innerBorderGO.transform.SetParent(borderGapGO.transform, false);
            var innerBorderImg = innerBorderGO.AddComponent<Image>();
            innerBorderImg.color = new Color(0.90f, 0.70f, 0.18f, 0.75f);
            var innerBorderRect = innerBorderGO.GetComponent<RectTransform>();
            innerBorderRect.anchorMin = Vector2.zero;
            innerBorderRect.anchorMax = Vector2.one;
            innerBorderRect.sizeDelta = new Vector2(-4, -4);
            innerBorderRect.anchoredPosition = Vector2.zero;

            // Panel background (inside the ornate border)
            var panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(innerBorderGO.transform, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.10f, 0.08f, 0.16f, 0.98f);
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = new Vector2(-4, -4);
            panelRect.anchoredPosition = Vector2.zero;

            // Title
            CreateText(panelGO.transform, "LEGENDS", 48,
                new Color(1f, 0.85f, 0.2f), new Vector2(0, 250));

            CreateText(panelGO.transform, "Best Streak Runs", 22,
                new Color(0.7f, 0.6f, 0.4f), new Vector2(0, 210));

            // Header row
            CreateText(panelGO.transform, "RANK", 18,
                new Color(0.6f, 0.6f, 0.7f), new Vector2(-240, 170), TextAnchor.MiddleCenter, 80);
            CreateText(panelGO.transform, "STREAK", 18,
                new Color(0.6f, 0.6f, 0.7f), new Vector2(-80, 170), TextAnchor.MiddleCenter, 120);
            CreateText(panelGO.transform, "DATE", 18,
                new Color(0.6f, 0.6f, 0.7f), new Vector2(120, 170), TextAnchor.MiddleCenter, 200);

            // Divider
            var divGO = new GameObject("Divider");
            divGO.transform.SetParent(panelGO.transform, false);
            var divImg = divGO.AddComponent<Image>();
            divImg.color = new Color(0.3f, 0.25f, 0.4f);
            var divRect = divGO.GetComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.5f, 0.5f);
            divRect.anchorMax = new Vector2(0.5f, 0.5f);
            divRect.sizeDelta = new Vector2(600, 2);
            divRect.anchoredPosition = new Vector2(0, 155);

            // Load legends data
            var legends = Gameplay.GameManager.LoadLegendsData();

            if (legends.Entries.Count == 0)
            {
                CreateText(panelGO.transform, "No streak runs yet.\nComplete Campaign to unlock Streak Mode!", 22,
                    new Color(0.5f, 0.45f, 0.55f), new Vector2(0, 0));
            }
            else
            {
                int maxEntries = Mathf.Min(legends.Entries.Count, 10);
                for (int i = 0; i < maxEntries; i++)
                {
                    float y = 120 - i * 35;
                    var entry = legends.Entries[i];

                    // Rank colors: gold, silver, bronze, then gray
                    Color rankColor = i switch
                    {
                        0 => new Color(1f, 0.85f, 0.2f),
                        1 => new Color(0.75f, 0.75f, 0.80f),
                        2 => new Color(0.80f, 0.55f, 0.25f),
                        _ => new Color(0.55f, 0.55f, 0.60f)
                    };

                    CreateText(panelGO.transform, $"#{i + 1}", 20,
                        rankColor, new Vector2(-240, y), TextAnchor.MiddleCenter, 80);
                    CreateText(panelGO.transform, $"{entry.StreakCount} levels", 20,
                        rankColor, new Vector2(-80, y), TextAnchor.MiddleCenter, 120);

                    // Format timestamp
                    var dt = DateTimeOffset.FromUnixTimeSeconds(entry.Timestamp).LocalDateTime;
                    string dateStr = dt.ToString("MMM d, yyyy");
                    CreateText(panelGO.transform, dateStr, 18,
                        new Color(0.5f, 0.5f, 0.55f), new Vector2(120, y), TextAnchor.MiddleCenter, 200);
                }
            }

            // Close button
            CreateButton(panelGO.transform, "CLOSE", new Vector2(0, -260),
                new Color(0.3f, 0.25f, 0.35f), Close);

            // Hint
            CreateText(panelGO.transform, "ESC: Close", 14,
                new Color(0.4f, 0.35f, 0.45f), new Vector2(0, -285));
        }

        private void CreateText(Transform parent, string text, int fontSize, Color color, Vector2 position,
            TextAnchor alignment = TextAnchor.MiddleCenter, float width = 600)
        {
            int scale = FontSizeToScale(fontSize);

            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.sprite = Gameplay.PlaceholderAssets.GetPixelTextSprite(text, color, scale);
            img.preserveAspect = true;
            img.raycastTarget = false;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;

            // Map alignment to pivot
            if (alignment == TextAnchor.MiddleLeft)
                rect.pivot = new Vector2(0f, 0.5f);
            else if (alignment == TextAnchor.MiddleRight)
                rect.pivot = new Vector2(1f, 0.5f);

            img.SetNativeSize();
        }

        private static int FontSizeToScale(int fontSize)
        {
            if (fontSize >= 72) return 10;
            if (fontSize >= 38) return 6;
            if (fontSize >= 30) return 5;
            if (fontSize >= 24) return 4;
            if (fontSize >= 16) return 3;
            return 2;
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
            rect.sizeDelta = new Vector2(200, 50);
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
