using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using EpochBreaker.Gameplay;

namespace EpochBreaker.UI
{
    /// <summary>
    /// Cosmetic selection overlay. Allows the player to browse and equip
    /// unlocked skins, trail effects, and profile frames.
    /// Accessible from the title screen.
    /// </summary>
    public class CosmeticSelectUI : MonoBehaviour
    {
        private GameObject _canvasGO;
        private Action _onClose;

        // Preview elements
        private Image _previewImage;
        private Text _previewLabel;
        private Text _previewRequirement;

        // Selection highlight tracking
        private Image _selectedSkinHighlight;
        private Image _selectedTrailHighlight;
        private Image _selectedFrameHighlight;

        public void Initialize(Action onClose)
        {
            _onClose = onClose;
            CreateUI();
        }

        private void Update()
        {
            if (Keyboard.current == null) return;
            if (InputManager.IsBackPressed())
                Close();
        }

        private void Close()
        {
            AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
            _onClose?.Invoke();
            Destroy(gameObject);
        }

        private void CreateUI()
        {
            _canvasGO = new GameObject("CosmeticsCanvas");
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

            // Main panel
            var panelGO = new GameObject("CosmeticsPanel");
            panelGO.transform.SetParent(_canvasGO.transform, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.10f, 0.08f, 0.18f, 0.98f);
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(900, 680);
            panelRect.anchoredPosition = Vector2.zero;

            // Inner border
            var borderGO = new GameObject("Border");
            borderGO.transform.SetParent(panelGO.transform, false);
            var borderImg = borderGO.AddComponent<Image>();
            borderImg.color = new Color(0.6f, 0.5f, 0.2f, 0.6f);
            var borderRect = borderGO.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.sizeDelta = new Vector2(-4, -4);
            borderRect.anchoredPosition = Vector2.zero;

            var innerGO = new GameObject("Inner");
            innerGO.transform.SetParent(borderGO.transform, false);
            var innerImg = innerGO.AddComponent<Image>();
            innerImg.color = new Color(0.10f, 0.08f, 0.18f, 1f);
            var innerRect = innerGO.GetComponent<RectTransform>();
            innerRect.anchorMin = Vector2.zero;
            innerRect.anchorMax = Vector2.one;
            innerRect.sizeDelta = new Vector2(-4, -4);
            innerRect.anchoredPosition = Vector2.zero;

            // Title
            CreatePixelLabel(panelGO.transform, "COSMETICS", new Color(1f, 0.85f, 0.2f), 3,
                new Vector2(0, 300), new Vector2(300, 36));

            // Close button
            CreateButton(panelGO.transform, "X", new Vector2(410, 300),
                new Vector2(40, 40), new Color(0.6f, 0.25f, 0.25f), Close);

            // Preview area (left side)
            CreatePreviewArea(panelGO.transform);

            // Sections (right side)
            float sectionX = 160f;
            float skinY = 220f;
            CreateSkinSection(panelGO.transform, sectionX, skinY);

            float trailY = 20f;
            CreateTrailSection(panelGO.transform, sectionX, trailY);

            float frameY = -160f;
            CreateFrameSection(panelGO.transform, sectionX, frameY);

            // Escape hint
            CreateLabel(panelGO.transform, "Press Esc/` to close", 14,
                new Color(0.5f, 0.5f, 0.6f), new Vector2(0, -310));
        }

        private void CreatePreviewArea(Transform parent)
        {
            // Preview panel background
            var previewBgGO = new GameObject("PreviewBg");
            previewBgGO.transform.SetParent(parent, false);
            var previewBgImg = previewBgGO.AddComponent<Image>();
            previewBgImg.color = new Color(0.06f, 0.05f, 0.12f, 1f);
            var previewBgRect = previewBgGO.GetComponent<RectTransform>();
            previewBgRect.anchorMin = new Vector2(0.5f, 0.5f);
            previewBgRect.anchorMax = new Vector2(0.5f, 0.5f);
            previewBgRect.sizeDelta = new Vector2(220, 320);
            previewBgRect.anchoredPosition = new Vector2(-300, 50);

            // Preview label
            CreatePixelLabel(previewBgGO.transform, "PREVIEW", new Color(0.7f, 0.7f, 0.8f), 2,
                new Vector2(0, 140), new Vector2(150, 20));

            // Player sprite preview
            var previewGO = new GameObject("PreviewSprite");
            previewGO.transform.SetParent(previewBgGO.transform, false);
            _previewImage = previewGO.AddComponent<Image>();
            _previewImage.preserveAspect = true;
            var previewRect = previewGO.GetComponent<RectTransform>();
            previewRect.anchorMin = new Vector2(0.5f, 0.5f);
            previewRect.anchorMax = new Vector2(0.5f, 0.5f);
            previewRect.sizeDelta = new Vector2(140, 140);
            previewRect.anchoredPosition = new Vector2(0, 20);

            // Name label
            var nameLabelGO = new GameObject("NameLabel");
            nameLabelGO.transform.SetParent(previewBgGO.transform, false);
            _previewLabel = nameLabelGO.AddComponent<Text>();
            _previewLabel.fontSize = 20;
            _previewLabel.color = Color.white;
            _previewLabel.alignment = TextAnchor.MiddleCenter;
            _previewLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var nameLabelRect = nameLabelGO.GetComponent<RectTransform>();
            nameLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
            nameLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
            nameLabelRect.sizeDelta = new Vector2(200, 28);
            nameLabelRect.anchoredPosition = new Vector2(0, -70);

            // Requirement label
            var reqLabelGO = new GameObject("ReqLabel");
            reqLabelGO.transform.SetParent(previewBgGO.transform, false);
            _previewRequirement = reqLabelGO.AddComponent<Text>();
            _previewRequirement.fontSize = 14;
            _previewRequirement.color = new Color(0.7f, 0.7f, 0.8f);
            _previewRequirement.alignment = TextAnchor.MiddleCenter;
            _previewRequirement.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _previewRequirement.horizontalOverflow = HorizontalWrapMode.Wrap;
            var reqLabelRect = reqLabelGO.GetComponent<RectTransform>();
            reqLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
            reqLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
            reqLabelRect.sizeDelta = new Vector2(200, 40);
            reqLabelRect.anchoredPosition = new Vector2(0, -105);

            // Initialize preview with current selection
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            var cm = CosmeticManager.Instance;
            if (cm == null) return;

            var skin = cm.SelectedSkin;
            _previewImage.sprite = PlaceholderAssets.GetTintedPlayerSprite(skin);
            _previewLabel.text = CosmeticManager.GetSkinName(skin);
            _previewLabel.color = skin == PlayerSkin.Default ? Color.white : CosmeticManager.GetSkinTint(skin);
            _previewRequirement.text = CosmeticManager.GetSkinRequirement(skin);
        }

        // ── Skin Section ──

        private void CreateSkinSection(Transform parent, float baseX, float baseY)
        {
            CreatePixelLabel(parent, "SKINS", new Color(0.9f, 0.8f, 0.4f), 2,
                new Vector2(baseX, baseY), new Vector2(100, 22));

            var cm = CosmeticManager.Instance;
            PlayerSkin selected = cm?.SelectedSkin ?? PlayerSkin.Default;

            float itemX = baseX - 140f;
            float itemY = baseY - 40f;
            int col = 0;

            foreach (PlayerSkin skin in Enum.GetValues(typeof(PlayerSkin)))
            {
                bool unlocked = cm != null && cm.IsSkinUnlocked(skin);
                bool isSelected = skin == selected;
                Color tint = unlocked ? CosmeticManager.GetSkinTint(skin) : new Color(0.3f, 0.3f, 0.35f);
                string name = CosmeticManager.GetSkinName(skin);

                float x = itemX + col * 80f;
                var highlight = CreateCosmeticItem(parent, name, tint, unlocked, isSelected,
                    new Vector2(x, itemY), skin, () =>
                    {
                        if (cm != null && cm.IsSkinUnlocked(skin))
                        {
                            cm.SetSkin(skin);
                            AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                            RefreshUI();
                        }
                    });

                if (isSelected) _selectedSkinHighlight = highlight;
                col++;
                if (col >= 4)
                {
                    col = 0;
                    itemY -= 70f;
                }
            }
        }

        // ── Trail Section ──

        private void CreateTrailSection(Transform parent, float baseX, float baseY)
        {
            CreatePixelLabel(parent, "TRAILS", new Color(0.9f, 0.8f, 0.4f), 2,
                new Vector2(baseX, baseY), new Vector2(100, 22));

            var cm = CosmeticManager.Instance;
            TrailEffect selected = cm?.SelectedTrail ?? TrailEffect.None;

            float itemX = baseX - 140f;
            float itemY = baseY - 40f;
            int col = 0;

            foreach (TrailEffect trail in Enum.GetValues(typeof(TrailEffect)))
            {
                bool unlocked = cm != null && cm.IsTrailUnlocked(trail);
                bool isSelected = trail == selected;
                Color tint = trail == TrailEffect.None
                    ? new Color(0.5f, 0.5f, 0.6f)
                    : (unlocked ? CosmeticManager.GetTrailColor(trail) : new Color(0.3f, 0.3f, 0.35f));
                string name = CosmeticManager.GetTrailName(trail);

                float x = itemX + col * 80f;
                var highlight = CreateCosmeticItem(parent, name, tint, unlocked, isSelected,
                    new Vector2(x, itemY), null, () =>
                    {
                        if (cm != null && cm.IsTrailUnlocked(trail))
                        {
                            cm.SetTrail(trail);
                            AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                            RefreshUI();
                        }
                    });

                if (isSelected) _selectedTrailHighlight = highlight;
                col++;
                if (col >= 4) { col = 0; itemY -= 70f; }
            }
        }

        // ── Frame Section ──

        private void CreateFrameSection(Transform parent, float baseX, float baseY)
        {
            CreatePixelLabel(parent, "FRAMES", new Color(0.9f, 0.8f, 0.4f), 2,
                new Vector2(baseX, baseY), new Vector2(100, 22));

            var cm = CosmeticManager.Instance;
            ProfileFrame selected = cm?.SelectedFrame ?? ProfileFrame.None;

            float itemX = baseX - 140f;
            float itemY = baseY - 40f;
            int col = 0;

            foreach (ProfileFrame frame in Enum.GetValues(typeof(ProfileFrame)))
            {
                bool unlocked = cm != null && cm.IsFrameUnlocked(frame);
                bool isSelected = frame == selected;
                Color tint = frame == ProfileFrame.None
                    ? new Color(0.5f, 0.5f, 0.6f)
                    : (unlocked ? CosmeticManager.GetFrameColor(frame) : new Color(0.3f, 0.3f, 0.35f));
                string name = CosmeticManager.GetFrameName(frame);

                float x = itemX + col * 80f;
                var highlight = CreateCosmeticItem(parent, name, tint, unlocked, isSelected,
                    new Vector2(x, itemY), null, () =>
                    {
                        if (cm != null && cm.IsFrameUnlocked(frame))
                        {
                            cm.SetFrame(frame);
                            AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                            RefreshUI();
                        }
                    });

                if (isSelected) _selectedFrameHighlight = highlight;
                col++;
                if (col >= 4) { col = 0; itemY -= 70f; }
            }
        }

        /// <summary>
        /// Create a single cosmetic item button with color swatch, label, and lock status.
        /// Returns the highlight Image (used for selection tracking).
        /// </summary>
        private Image CreateCosmeticItem(Transform parent, string name, Color color,
            bool unlocked, bool isSelected, Vector2 position, object previewData,
            UnityEngine.Events.UnityAction onClick)
        {
            var containerGO = new GameObject($"Item_{name}");
            containerGO.transform.SetParent(parent, false);
            var containerRect = containerGO.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(70, 60);
            containerRect.anchoredPosition = position;

            // Selection highlight border
            var highlightGO = new GameObject("Highlight");
            highlightGO.transform.SetParent(containerGO.transform, false);
            var highlightImg = highlightGO.AddComponent<Image>();
            highlightImg.color = isSelected ? new Color(1f, 0.85f, 0.2f, 0.8f) : Color.clear;
            var highlightRect = highlightGO.GetComponent<RectTransform>();
            highlightRect.anchorMin = Vector2.zero;
            highlightRect.anchorMax = Vector2.one;
            highlightRect.sizeDelta = new Vector2(4, 4);
            highlightRect.anchoredPosition = Vector2.zero;

            // Color swatch background
            var swatchGO = new GameObject("Swatch");
            swatchGO.transform.SetParent(containerGO.transform, false);
            var swatchImg = swatchGO.AddComponent<Image>();
            swatchImg.color = unlocked ? color : new Color(0.15f, 0.13f, 0.2f);
            var swatchRect = swatchGO.GetComponent<RectTransform>();
            swatchRect.anchorMin = new Vector2(0.5f, 1f);
            swatchRect.anchorMax = new Vector2(0.5f, 1f);
            swatchRect.pivot = new Vector2(0.5f, 1f);
            swatchRect.sizeDelta = new Vector2(50, 32);
            swatchRect.anchoredPosition = new Vector2(0, -2);

            // Lock icon or skin preview in swatch
            if (!unlocked)
            {
                var lockGO = new GameObject("Lock");
                lockGO.transform.SetParent(swatchGO.transform, false);
                var lockImg = lockGO.AddComponent<Image>();
                lockImg.sprite = PlaceholderAssets.GetPixelTextSprite("?", new Color(0.4f, 0.35f, 0.45f), 2);
                lockImg.preserveAspect = true;
                var lockRect = lockGO.GetComponent<RectTransform>();
                lockRect.anchorMin = new Vector2(0.5f, 0.5f);
                lockRect.anchorMax = new Vector2(0.5f, 0.5f);
                lockRect.sizeDelta = new Vector2(20, 20);
                lockRect.anchoredPosition = Vector2.zero;
            }
            else if (previewData is PlayerSkin skin)
            {
                // Show tiny player sprite preview for skins
                var miniGO = new GameObject("MiniPreview");
                miniGO.transform.SetParent(swatchGO.transform, false);
                var miniImg = miniGO.AddComponent<Image>();
                miniImg.sprite = PlaceholderAssets.GetTintedPlayerSprite(skin);
                miniImg.preserveAspect = true;
                var miniRect = miniGO.GetComponent<RectTransform>();
                miniRect.anchorMin = new Vector2(0.5f, 0.5f);
                miniRect.anchorMax = new Vector2(0.5f, 0.5f);
                miniRect.sizeDelta = new Vector2(26, 26);
                miniRect.anchoredPosition = Vector2.zero;
            }

            // Name label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(containerGO.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(name,
                unlocked ? new Color(0.85f, 0.85f, 0.9f) : new Color(0.4f, 0.38f, 0.45f), 2);
            labelImg.preserveAspect = true;
            labelImg.raycastTarget = false;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0f);
            labelRect.anchorMax = new Vector2(0.5f, 0f);
            labelRect.sizeDelta = new Vector2(68, 18);
            labelRect.anchoredPosition = new Vector2(0, 8);

            // Button component on container
            var btn = containerGO.AddComponent<Button>();
            btn.targetGraphic = swatchImg;
            btn.interactable = unlocked;
            btn.onClick.AddListener(onClick);

            // Hover handler: update preview on hover for skins
            if (previewData is PlayerSkin hoverSkin)
            {
                var captured = hoverSkin;
                // Use event trigger for pointer enter
                var trigger = containerGO.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
                enterEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                enterEntry.callback.AddListener((_) => ShowSkinPreview(captured));
                trigger.triggers.Add(enterEntry);

                var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
                exitEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                exitEntry.callback.AddListener((_) => UpdatePreview());
                trigger.triggers.Add(exitEntry);
            }

            return highlightImg;
        }

        private void ShowSkinPreview(PlayerSkin skin)
        {
            if (_previewImage == null) return;
            _previewImage.sprite = PlaceholderAssets.GetTintedPlayerSprite(skin);
            if (_previewLabel != null)
            {
                _previewLabel.text = CosmeticManager.GetSkinName(skin);
                _previewLabel.color = skin == PlayerSkin.Default ? Color.white : CosmeticManager.GetSkinTint(skin);
            }
            if (_previewRequirement != null)
                _previewRequirement.text = CosmeticManager.GetSkinRequirement(skin);
        }

        /// <summary>
        /// Full UI refresh — destroy and recreate to reflect changes.
        /// </summary>
        private void RefreshUI()
        {
            if (_canvasGO != null)
                Destroy(_canvasGO);
            CreateUI();
        }

        // ── UI Helpers ──

        private void CreatePixelLabel(Transform parent, string text, Color color, int scale,
            Vector2 position, Vector2 size)
        {
            var go = new GameObject("PixelLabel");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = PlaceholderAssets.GetPixelTextSprite(text, color, scale);
            img.preserveAspect = true;
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private GameObject CreateLabel(Transform parent, string text, int fontSize,
            Color color, Vector2 position)
        {
            var go = new GameObject("Label");
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
            rect.sizeDelta = new Vector2(400, fontSize + 12);
            rect.anchoredPosition = position;
            return go;
        }

        private void CreateButton(Transform parent, string text, Vector2 position, Vector2 size,
            Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Btn");
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
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            int scale = size.y > 55 ? 3 : 2;
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(text, Color.white, scale);
            labelImg.preserveAspect = true;
            labelImg.raycastTarget = false;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(size.x - 8, size.y - 8);
            labelRect.anchoredPosition = Vector2.zero;
        }
    }
}
