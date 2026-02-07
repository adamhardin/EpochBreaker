using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace EpochBreaker.UI
{
    /// <summary>
    /// Achievements overlay showing all 22 achievements with unlock status.
    /// Accessible from the title screen.
    /// </summary>
    public class AchievementsUI : MonoBehaviour
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
            _canvasGO = new GameObject("AchievementsCanvas");
            _canvasGO.transform.SetParent(transform);
            var canvas = _canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 140;

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
            outerBorderRect.sizeDelta = new Vector2(824, 724);
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

            // Panel background
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
            CreateText(panelGO.transform, "ACHIEVEMENTS", 42,
                new Color(1f, 0.85f, 0.2f), new Vector2(0, 310));

            // Unlock count
            var am = Gameplay.AchievementManager.Instance;
            int unlocked = 0, total = 0;
            if (am != null)
            {
                var counts = am.GetUnlockCount();
                unlocked = counts.unlocked;
                total = counts.total;
            }

            CreateText(panelGO.transform, $"{unlocked} / {total} Unlocked", 20,
                new Color(0.7f, 0.6f, 0.4f), new Vector2(0, 272));

            // Divider
            var divGO = new GameObject("Divider");
            divGO.transform.SetParent(panelGO.transform, false);
            var divImg = divGO.AddComponent<Image>();
            divImg.color = new Color(0.3f, 0.25f, 0.4f);
            var divRect = divGO.GetComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.5f, 0.5f);
            divRect.anchorMax = new Vector2(0.5f, 0.5f);
            divRect.sizeDelta = new Vector2(720, 2);
            divRect.anchoredPosition = new Vector2(0, 255);

            // ── Scrollable achievement list ──

            // Viewport — clips content to visible area using RectMask2D
            var viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(panelGO.transform, false);
            var viewportRect = viewportGO.AddComponent<RectTransform>();
            viewportRect.anchorMin = new Vector2(0.5f, 0.5f);
            viewportRect.anchorMax = new Vector2(0.5f, 0.5f);
            viewportRect.sizeDelta = new Vector2(740, 500);
            viewportRect.anchoredPosition = new Vector2(0, -15);
            viewportGO.AddComponent<RectMask2D>();

            // Scroll content — holds all achievement rows
            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewportGO.transform, false);
            var contentRect = contentGO.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 1f);
            contentRect.anchorMax = new Vector2(0.5f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);

            // Achievement list — two columns
            var achievementTypes = (Gameplay.AchievementType[])Enum.GetValues(typeof(Gameplay.AchievementType));
            int halfCount = (achievementTypes.Length + 1) / 2;
            float contentHeight = halfCount * 42f + 10f;
            contentRect.sizeDelta = new Vector2(740, contentHeight);

            for (int i = 0; i < achievementTypes.Length; i++)
            {
                var type = achievementTypes[i];
                int col = i < halfCount ? 0 : 1;
                int row = i < halfCount ? i : i - halfCount;
                float x = col == 0 ? -185f : 185f;
                float y = -5f - row * 42f; // relative to content top

                CreateAchievementRow(contentGO.transform, type, am, new Vector2(x, y));
            }

            // ScrollRect component on the viewport's parent
            var scrollGO = viewportGO;
            var scrollRect = scrollGO.AddComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 30f;

            // Close button (below scroll area)
            CreateButton(panelGO.transform, "CLOSE", new Vector2(0, -300),
                new Color(0.3f, 0.25f, 0.35f), Close);

            // Hint
            CreateText(panelGO.transform, "Esc/`: Close  |  Scroll: Mouse Wheel", 14,
                new Color(0.4f, 0.35f, 0.45f), new Vector2(0, -330));
        }

        private void CreateAchievementRow(Transform parent, Gameplay.AchievementType type,
            Gameplay.AchievementManager am, Vector2 position)
        {
            string name = Gameplay.AchievementManager.GetAchievementName(type);
            string desc = Gameplay.AchievementManager.GetAchievementDescription(type);
            bool isUnlocked = am != null && am.IsUnlocked(type);
            var data = am?.GetAchievement(type);

            // Row container
            var rowGO = new GameObject($"Achievement_{type}");
            rowGO.transform.SetParent(parent, false);
            var rowRect = rowGO.AddComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 1f);
            rowRect.anchorMax = new Vector2(0.5f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.sizeDelta = new Vector2(340, 38);
            rowRect.anchoredPosition = position;

            // Row background
            var rowImg = rowGO.AddComponent<Image>();
            rowImg.color = isUnlocked
                ? new Color(0.15f, 0.18f, 0.12f, 0.7f)
                : new Color(0.12f, 0.10f, 0.18f, 0.5f);

            // Status icon
            CreateText(rowGO.transform, isUnlocked ? "V" : "-", 18,
                isUnlocked ? new Color(0.4f, 0.9f, 0.4f) : new Color(0.35f, 0.30f, 0.40f),
                new Vector2(-150, 0), TextAnchor.MiddleCenter, 28);

            // Achievement name
            Color nameColor = isUnlocked
                ? new Color(1f, 0.85f, 0.2f)
                : new Color(0.5f, 0.45f, 0.55f);
            CreateText(rowGO.transform, name, 15,
                nameColor, new Vector2(-20, 6), TextAnchor.MiddleLeft, 240);

            // Description
            Color descColor = isUnlocked
                ? new Color(0.6f, 0.6f, 0.5f)
                : new Color(0.4f, 0.38f, 0.45f);
            CreateText(rowGO.transform, desc, 11,
                descColor, new Vector2(-20, -10), TextAnchor.MiddleLeft, 240);

            // Progress bar for cumulative achievements
            if (data != null && data.ProgressTarget > 1 && !isUnlocked)
            {
                float progress = Mathf.Clamp01((float)data.Progress / data.ProgressTarget);

                // Track bg
                var trackGO = new GameObject("ProgressTrack");
                trackGO.transform.SetParent(rowGO.transform, false);
                var trackImg = trackGO.AddComponent<Image>();
                trackImg.color = new Color(0.15f, 0.13f, 0.25f);
                var trackRect = trackGO.GetComponent<RectTransform>();
                trackRect.anchorMin = new Vector2(0.5f, 0.5f);
                trackRect.anchorMax = new Vector2(0.5f, 0.5f);
                trackRect.sizeDelta = new Vector2(80, 6);
                trackRect.anchoredPosition = new Vector2(120, -10);

                // Fill
                var fillGO = new GameObject("ProgressFill");
                fillGO.transform.SetParent(trackGO.transform, false);
                var fillImg = fillGO.AddComponent<Image>();
                fillImg.color = new Color(0.4f, 0.6f, 0.3f);
                var fillRect = fillGO.GetComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = new Vector2(progress, 1f);
                fillRect.sizeDelta = Vector2.zero;

                // Progress text
                CreateText(rowGO.transform, $"{data.Progress}/{data.ProgressTarget}", 10,
                    new Color(0.5f, 0.5f, 0.55f), new Vector2(120, 2), TextAnchor.MiddleCenter, 80);
            }

            // Sprint 8: Share button for unlocked achievements
            if (isUnlocked)
            {
                string capturedName = name;
                string capturedDesc = desc;

                var shareBtnGO = new GameObject("ShareBtn");
                shareBtnGO.transform.SetParent(rowGO.transform, false);
                var shareBtnImg = shareBtnGO.AddComponent<Image>();
                shareBtnImg.color = new Color(0.25f, 0.45f, 0.6f, 0.8f);
                var shareBtn = shareBtnGO.AddComponent<Button>();
                shareBtn.targetGraphic = shareBtnImg;
                shareBtn.onClick.AddListener(() => {
                    string shareText = Gameplay.LevelShareManager.GenerateAchievementShareText(
                        capturedName, capturedDesc);
                    Gameplay.GameManager.CopyToClipboard(shareText);
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                });
                var shareBtnRect = shareBtnGO.GetComponent<RectTransform>();
                shareBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
                shareBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
                shareBtnRect.sizeDelta = new Vector2(40, 22);
                shareBtnRect.anchoredPosition = new Vector2(148, 0);

                // Share label
                var shareLabelGO = new GameObject("ShareLabel");
                shareLabelGO.transform.SetParent(shareBtnGO.transform, false);
                var shareLabel = shareLabelGO.AddComponent<Text>();
                shareLabel.text = "SHARE";
                shareLabel.fontSize = 12;
                shareLabel.color = Color.white;
                shareLabel.alignment = TextAnchor.MiddleCenter;
                shareLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                var shareLabelRect = shareLabelGO.GetComponent<RectTransform>();
                shareLabelRect.anchorMin = Vector2.zero;
                shareLabelRect.anchorMax = Vector2.one;
                shareLabelRect.sizeDelta = Vector2.zero;
            }
        }

        private void CreateText(Transform parent, string text, int fontSize, Color color, Vector2 position,
            TextAnchor alignment = TextAnchor.MiddleCenter, float width = 600)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);

            var textComp = go.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = fontSize;
            textComp.color = color;
            textComp.alignment = alignment;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, fontSize + 20);
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
            rect.sizeDelta = new Vector2(200, 50);
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
        }
    }
}
