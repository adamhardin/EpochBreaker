using UnityEngine;
using UnityEngine.UI;
using SixteenBit.Generative;
using SixteenBit.Gameplay;

namespace SixteenBit.UI
{
    /// <summary>
    /// Title screen with game logo, main menu, legend, and dev-only level selector.
    /// Creates its own Canvas and UI elements programmatically.
    /// </summary>
    public class TitleScreenUI : MonoBehaviour
    {
        private Canvas _canvas;
        private GameObject _levelSelectorPanel;

        private static readonly string[] EraNames = {
            "Stone", "Bronze", "Classical", "Medieval", "Renaissance",
            "Industrial", "Modern", "Digital", "Space", "Transcend"
        };

        private static readonly string[] DiffNames = { "Easy", "Normal", "Hard", "Extreme" };

        private void Start()
        {
            CreateUI();
        }

        private void Update()
        {
            // Close level selector with Escape
            if (_levelSelectorPanel != null && _levelSelectorPanel.activeSelf &&
                Input.GetKeyDown(KeyCode.Escape))
            {
                _levelSelectorPanel.SetActive(false);
            }
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

            // Ornate pixel art frame
            var frameGO = new GameObject("OrnateFrame");
            frameGO.transform.SetParent(canvasGO.transform, false);
            var frameImg = frameGO.AddComponent<Image>();
            frameImg.sprite = PlaceholderAssets.GetOrnateFrameSprite();
            frameImg.preserveAspect = false;
            var frameRect = frameGO.GetComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.sizeDelta = Vector2.zero;

            // Title logo (pixel art sprite - single line, large)
            var titleGO = new GameObject("TitleLogo");
            titleGO.transform.SetParent(canvasGO.transform, false);
            var titleImg = titleGO.AddComponent<Image>();
            titleImg.sprite = PlaceholderAssets.GetTitleLogoSprite();
            titleImg.preserveAspect = true;
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(900, 120);
            titleRect.anchoredPosition = new Vector2(0, 300);

            // Main menu buttons
            CreateMainMenu(canvasGO.transform);

            // Legend panel
            CreateLegend(canvasGO.transform);

            // Controls hint
            var hintGO = CreateText(canvasGO.transform,
                "A/D: Move | Space: Jump | Down: Ground Pound | Esc: Pause", 16,
                new Color(0.5f, 0.5f, 0.6f));
            var hintRect = hintGO.GetComponent<RectTransform>();
            hintRect.anchoredPosition = new Vector2(0, -450);

            // Version
            var verGO = CreateText(canvasGO.transform, "v0.1.0 - Debug Build", 14,
                new Color(0.4f, 0.4f, 0.5f));
            var verRect = verGO.GetComponent<RectTransform>();
            verRect.anchorMin = new Vector2(0, 0);
            verRect.anchorMax = new Vector2(0, 0);
            verRect.pivot = new Vector2(0, 0);
            verRect.anchoredPosition = new Vector2(20, 20);

            // Level selector overlay (initially hidden)
            CreateLevelSelectorOverlay(canvasGO.transform);
        }

        private void CreateMainMenu(Transform parent)
        {
            // Play Game button (primary)
            CreateMenuButton(parent, "PLAY GAME", new Vector2(0, 140),
                new Vector2(300, 70), new Color(0.2f, 0.55f, 0.3f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    GameManager.Instance?.StartGame();
                });

            // Level Selector button (dev)
            CreateMenuButton(parent, "LEVEL SELECT [DEV]", new Vector2(0, 50),
                new Vector2(300, 50), new Color(0.4f, 0.35f, 0.5f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    if (_levelSelectorPanel != null)
                        _levelSelectorPanel.SetActive(true);
                });
        }

        private void CreateLegend(Transform parent)
        {
            // Legend panel
            var panelGO = new GameObject("Legend");
            panelGO.transform.SetParent(parent, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.10f, 0.08f, 0.18f, 0.85f);
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(800, 280);
            panelRect.anchoredPosition = new Vector2(0, -180);

            // Header
            var headerGO = CreateText(panelGO.transform, "GAME ELEMENTS", 20, new Color(0.9f, 0.9f, 1f));
            var headerRect = headerGO.GetComponent<RectTransform>();
            headerRect.anchoredPosition = new Vector2(0, 115);

            // Row 1: Rewards
            float row1Y = 55f;
            CreateLegendLabel(panelGO.transform, "Rewards:", new Vector2(-350, row1Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetRewardSprite(RewardType.HealthSmall),
                "Health", new Vector2(-240, row1Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetRewardSprite(RewardType.AttackBoost),
                "Attack", new Vector2(-130, row1Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetRewardSprite(RewardType.SpeedBoost),
                "Speed", new Vector2(-20, row1Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetRewardSprite(RewardType.Shield),
                "Shield", new Vector2(90, row1Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetRewardSprite(RewardType.Coin),
                "Coin", new Vector2(200, row1Y));

            // Row 2: Weapons
            float row2Y = -15f;
            CreateLegendLabel(panelGO.transform, "Weapons:", new Vector2(-350, row2Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetWeaponPickupSprite(WeaponTier.Starting),
                "Sword", new Vector2(-240, row2Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetWeaponPickupSprite(WeaponTier.Medium),
                "Crossbow", new Vector2(-130, row2Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetWeaponPickupSprite(WeaponTier.Heavy),
                "Cannon", new Vector2(-20, row2Y));

            // Row 3: Checkpoints & Goal
            float row3Y = -85f;
            CreateLegendLabel(panelGO.transform, "Progress:", new Vector2(-350, row3Y));
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetCheckpointSprite(CheckpointType.LevelStart),
                "Start", new Vector2(-240, row3Y), 48f);
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetCheckpointSprite(CheckpointType.MidLevel),
                "Mid", new Vector2(-130, row3Y), 48f);
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetCheckpointSprite(CheckpointType.PreBoss),
                "Boss", new Vector2(-20, row3Y), 48f);
            CreateLegendItem(panelGO.transform, PlaceholderAssets.GetGoalSprite(),
                "Goal", new Vector2(110, row3Y), 48f);
        }

        private void CreateLegendLabel(Transform parent, string text, Vector2 position)
        {
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(parent, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(text.Replace(":", ""), new Color(0.8f, 0.8f, 0.9f), 2);
            labelImg.preserveAspect = true;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(100, 24);
            labelRect.anchoredPosition = position;
        }

        private void CreateLegendItem(Transform parent, Sprite sprite, string label, Vector2 position, float iconSize = 40f)
        {
            // Container
            var containerGO = new GameObject("LegendItem");
            containerGO.transform.SetParent(parent, false);
            var containerRect = containerGO.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(80, 60);
            containerRect.anchoredPosition = position;

            // Icon
            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(containerGO.transform, false);
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.sprite = sprite;
            iconImg.preserveAspect = true;
            var iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(iconSize, iconSize);
            iconRect.anchoredPosition = new Vector2(0, 8);

            // Label (pixel text)
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(containerGO.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(label, new Color(0.75f, 0.75f, 0.85f), 2);
            labelImg.preserveAspect = true;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0f);
            labelRect.anchorMax = new Vector2(0.5f, 0f);
            labelRect.sizeDelta = new Vector2(80, 20);
            labelRect.anchoredPosition = new Vector2(0, 5);
        }

        private void CreateLevelSelectorOverlay(Transform parent)
        {
            // Overlay background (darkens screen)
            _levelSelectorPanel = new GameObject("LevelSelectorOverlay");
            _levelSelectorPanel.transform.SetParent(parent, false);
            var overlayImg = _levelSelectorPanel.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.85f);
            var overlayRect = _levelSelectorPanel.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            // Level selector panel
            var panelGO = new GameObject("LevelSelector");
            panelGO.transform.SetParent(_levelSelectorPanel.transform, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.12f, 0.10f, 0.20f, 0.98f);
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(700, 700);
            panelRect.anchoredPosition = Vector2.zero;

            // Title
            var titleGO = CreateText(panelGO.transform, "LEVEL SELECTOR", 32, new Color(1f, 0.85f, 0.1f));
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 310);

            // Subtitle
            var subGO = CreateText(panelGO.transform, "Development Testing Only", 16,
                new Color(0.6f, 0.5f, 0.7f));
            var subRect = subGO.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(0, 275);

            // Close button
            CreateMenuButton(panelGO.transform, "X", new Vector2(320, 310),
                new Vector2(40, 40), new Color(0.6f, 0.25f, 0.25f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    _levelSelectorPanel.SetActive(false);
                });

            // Grid layout
            float headerY = 230f;
            float startX = -280f;
            float colWidth = 70f;
            float eraLabelWidth = 100f;

            // "Era" label in corner
            var cornerGO = CreateText(panelGO.transform, "ERA", 14, new Color(0.6f, 0.6f, 0.7f));
            var cornerRect = cornerGO.GetComponent<RectTransform>();
            cornerRect.anchoredPosition = new Vector2(startX + eraLabelWidth / 2 - 30, headerY);

            // Difficulty column headers
            for (int d = 0; d < 4; d++)
            {
                var diffGO = CreateText(panelGO.transform, DiffNames[d], 12, GetDifficultyColor(d));
                var diffRect = diffGO.GetComponent<RectTransform>();
                diffRect.anchoredPosition = new Vector2(startX + eraLabelWidth + d * colWidth + colWidth / 2, headerY);
            }

            // Era rows with buttons
            float rowHeight = 46f;
            float gridStartY = headerY - 40f;

            for (int era = 0; era < 10; era++)
            {
                float rowY = gridStartY - era * rowHeight;

                // Era name label
                var eraGO = CreateText(panelGO.transform, EraNames[era], 14, GetEraColor(era));
                var eraRect = eraGO.GetComponent<RectTransform>();
                eraRect.anchoredPosition = new Vector2(startX + eraLabelWidth / 2, rowY);
                eraRect.sizeDelta = new Vector2(eraLabelWidth, 24);

                // Difficulty buttons for this era
                for (int diff = 0; diff < 4; diff++)
                {
                    int capturedEra = era;
                    int capturedDiff = diff;

                    CreateLevelButton(panelGO.transform,
                        $"{era + 1}-{diff + 1}",
                        new Vector2(startX + eraLabelWidth + diff * colWidth + colWidth / 2, rowY),
                        new Vector2(60, 38),
                        GetButtonColor(era, diff),
                        () => {
                            AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                            GameManager.Instance?.StartTestLevel(capturedEra, capturedDiff);
                        });
                }
            }

            // Random level button
            CreateMenuButton(panelGO.transform, "RANDOM", new Vector2(0, -290),
                new Vector2(160, 45), new Color(0.3f, 0.45f, 0.6f), () => {
                    AudioManager.PlaySFX(PlaceholderAudio.GetMenuSelectSFX());
                    int randEra = Random.Range(0, 10);
                    int randDiff = Random.Range(0, 4);
                    GameManager.Instance?.StartTestLevel(randEra, randDiff);
                });

            // Hint text
            var hintGO = CreateText(panelGO.transform, "Press Esc to close", 14,
                new Color(0.5f, 0.5f, 0.6f));
            var hintRect = hintGO.GetComponent<RectTransform>();
            hintRect.anchoredPosition = new Vector2(0, -330);

            _levelSelectorPanel.SetActive(false);
        }

        private Color GetEraColor(int era)
        {
            float t = era / 9f;
            return Color.Lerp(
                new Color(0.9f, 0.7f, 0.4f),
                new Color(0.5f, 0.7f, 1.0f),
                t
            );
        }

        private Color GetDifficultyColor(int diff)
        {
            return diff switch
            {
                0 => new Color(0.4f, 0.8f, 0.4f),
                1 => new Color(0.9f, 0.8f, 0.3f),
                2 => new Color(0.9f, 0.5f, 0.2f),
                3 => new Color(0.9f, 0.3f, 0.3f),
                _ => Color.white
            };
        }

        private Color GetButtonColor(int era, int diff)
        {
            Color eraColor = GetEraColor(era);
            float intensity = 0.3f + diff * 0.15f;
            return new Color(
                eraColor.r * intensity,
                eraColor.g * intensity,
                eraColor.b * intensity,
                1f
            );
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

        private void CreateMenuButton(Transform parent, string text, Vector2 position, Vector2 size,
            Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("MenuBtn");
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

            // Pixel text label
            int scale = size.y > 55 ? 3 : 2;
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(text, Color.white, scale);
            labelImg.preserveAspect = true;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(size.x - 16, size.y - 12);
            labelRect.anchoredPosition = Vector2.zero;
        }

        private void CreateLevelButton(Transform parent, string text, Vector2 position, Vector2 size,
            Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("LevelBtn");
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

            // Pixel text label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelImg = labelGO.AddComponent<Image>();
            labelImg.sprite = PlaceholderAssets.GetPixelTextSprite(text, Color.white, 2);
            labelImg.preserveAspect = true;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(size.x - 8, size.y - 8);
            labelRect.anchoredPosition = Vector2.zero;
        }

        private void CreatePixelLabel(Transform parent, string text, Vector2 position, Color color, int scale = 2)
        {
            var go = new GameObject("PixelLabel");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = PlaceholderAssets.GetPixelTextSprite(text, color, scale);
            img.preserveAspect = true;
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(text.Length * 6 * scale + scale * 4, 7 * scale + scale * 4);
            rect.anchoredPosition = position;
        }
    }
}
