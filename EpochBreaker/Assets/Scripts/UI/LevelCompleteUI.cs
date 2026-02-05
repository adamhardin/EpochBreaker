using UnityEngine;
using UnityEngine.UI;
using SixteenBit.Generative;
using System.Collections.Generic;

namespace SixteenBit.UI
{
    /// <summary>
    /// Inter-level summary screen showing level stats, collected items with sprites, and continue button.
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

            // Full-screen background
            var overlayGO = new GameObject("Overlay");
            overlayGO.transform.SetParent(canvasGO.transform, false);
            var overlayImg = overlayGO.AddComponent<Image>();
            overlayImg.color = new Color(0.06f, 0.05f, 0.12f, 0.95f);
            var overlayRect = overlayGO.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            var gm = Gameplay.GameManager.Instance;
            int levelNum = gm?.LevelNumber ?? 1;
            float elapsed = gm?.LevelElapsedTime ?? 0f;
            int score = gm?.Score ?? 0;
            int totalScore = gm?.TotalScore ?? 0;
            int timeScore = gm?.TimeScore ?? 0;
            int itemBonus = gm?.ItemBonusScore ?? 0;
            int enemyBonus = gm?.EnemyBonusScore ?? 0;
            int stars = Gameplay.GameManager.GetStarRating(elapsed);
            int enemies = gm?.EnemiesKilled ?? 0;
            int bestMultiplier = gm?.GetBestMultiplier() ?? 0;
            string epochName = gm != null ? gm.CurrentLevelID.EpochName : "";
            string levelCode = gm != null ? gm.CurrentLevelID.ToCode() : "";

            // Level header
            CreateText(canvasGO.transform, $"LEVEL {levelNum} COMPLETE", 48,
                new Color(1f, 0.85f, 0.1f), new Vector2(0, 420));

            // Epoch name subtitle
            CreateText(canvasGO.transform, epochName, 24,
                new Color(0.7f, 0.7f, 0.8f), new Vector2(0, 375));

            // Level code (shareable)
            CreateText(canvasGO.transform, $"Level Code: {levelCode}", 18,
                new Color(0.5f, 0.8f, 1f), new Vector2(0, 345));

            // Stars
            string starDisplay = new string('*', stars) + new string('-', 3 - stars);
            CreateText(canvasGO.transform, starDisplay, 44,
                new Color(1f, 0.85f, 0.1f), new Vector2(0, 320));

            // Stats panel (left side) - 95% opacity for readability
            var panelGO = new GameObject("StatsPanel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.08f, 0.06f, 0.14f, 0.95f);
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(420, 340);
            panelRect.anchoredPosition = new Vector2(-230, 60);

            // Panel header
            var statHeaderGO = new GameObject("Header");
            statHeaderGO.transform.SetParent(panelGO.transform, false);
            var statHeaderText = statHeaderGO.AddComponent<Text>();
            statHeaderText.text = "SCORE BREAKDOWN";
            statHeaderText.fontSize = 22;
            statHeaderText.color = new Color(0.9f, 0.9f, 1f);
            statHeaderText.alignment = TextAnchor.MiddleCenter;
            statHeaderText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var statHeaderRect = statHeaderGO.GetComponent<RectTransform>();
            statHeaderRect.anchorMin = new Vector2(0.5f, 1f);
            statHeaderRect.anchorMax = new Vector2(0.5f, 1f);
            statHeaderRect.sizeDelta = new Vector2(380, 30);
            statHeaderRect.anchoredPosition = new Vector2(0, -25);

            Color statColor = new Color(0.75f, 0.75f, 0.85f);
            Color bonusColor = new Color(0.4f, 0.9f, 0.4f);
            Color goldColor = new Color(1f, 0.85f, 0.1f);

            // Consistent 38px spacing for all rows, starting from top padding
            float rowY = 105f;
            float rowSpacing = 38f;

            // Time
            int minutes = (int)(elapsed / 60f);
            float seconds = elapsed % 60f;
            CreateStatRow(panelGO.transform, "Time", $"{minutes}:{seconds:00.0}", rowY, statColor);
            rowY -= rowSpacing;

            // Time score
            CreateStatRow(panelGO.transform, "Time Score", $"{timeScore}", rowY, statColor);
            rowY -= rowSpacing;

            // Item bonus (always show, even if 0)
            CreateStatRow(panelGO.transform, "Item Bonus", itemBonus > 0 ? $"+{itemBonus}" : "0", rowY,
                itemBonus > 0 ? bonusColor : statColor);
            rowY -= rowSpacing;

            // Enemies defeated with bonus
            string enemyStr = enemies > 0 ? $"{enemies}  (+{enemyBonus})" : "0";
            CreateStatRow(panelGO.transform, "Enemies Defeated", enemyStr, rowY,
                enemies > 0 ? bonusColor : statColor);
            rowY -= rowSpacing;

            // Best multiplier (always show)
            CreateStatRow(panelGO.transform, "Best Multiplier", bestMultiplier > 1 ? $"x{bestMultiplier}" : "-", rowY,
                bestMultiplier > 1 ? goldColor : statColor);
            rowY -= rowSpacing + 8f; // Extra gap before totals

            // Level score (highlighted)
            CreateStatRow(panelGO.transform, "Level Score", $"{score}", rowY, Color.white);
            rowY -= rowSpacing;

            // Total score (highlighted)
            CreateStatRow(panelGO.transform, "Total Score", $"{totalScore}", rowY, goldColor);

            // Items panel (right side)
            CreateItemsPanel(canvasGO.transform, gm);

            // Continue button (primary action)
            CreateButton(canvasGO.transform, "CONTINUE", new Vector2(0, -280),
                new Color(0.2f, 0.6f, 0.3f), () => {
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.GameManager.Instance?.NextLevel();
                });

            // Replay button
            CreateButton(canvasGO.transform, "REPLAY", new Vector2(-140, -355),
                new Color(0.3f, 0.4f, 0.6f), () => {
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.GameManager.Instance?.RestartLevel();
                });

            // Menu button
            CreateButton(canvasGO.transform, "MENU", new Vector2(140, -355),
                new Color(0.4f, 0.3f, 0.3f), () => {
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.GameManager.Instance?.ReturnToTitle();
                });

            // Keyboard hint
            CreateText(canvasGO.transform, "Enter: Continue | R: Replay | Esc: Menu", 14,
                new Color(0.5f, 0.5f, 0.6f), new Vector2(0, -420));
        }

        private void CreateItemsPanel(Transform parent, Gameplay.GameManager gm)
        {
            // Items collected panel (right side) - 95% opacity for readability
            var itemsPanelGO = new GameObject("ItemsPanel");
            itemsPanelGO.transform.SetParent(parent, false);
            var itemsPanelImg = itemsPanelGO.AddComponent<Image>();
            itemsPanelImg.color = new Color(0.08f, 0.06f, 0.14f, 0.95f);
            var itemsPanelRect = itemsPanelGO.GetComponent<RectTransform>();
            itemsPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            itemsPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            itemsPanelRect.sizeDelta = new Vector2(420, 340);
            itemsPanelRect.anchoredPosition = new Vector2(230, 60);

            // Panel header
            var headerGO = new GameObject("Header");
            headerGO.transform.SetParent(itemsPanelGO.transform, false);
            var headerText = headerGO.AddComponent<Text>();
            headerText.text = "ITEMS COLLECTED";
            headerText.fontSize = 22;
            headerText.color = new Color(0.9f, 0.9f, 1f);
            headerText.alignment = TextAnchor.MiddleCenter;
            headerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var headerRect = headerGO.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.5f, 1f);
            headerRect.anchorMax = new Vector2(0.5f, 1f);
            headerRect.sizeDelta = new Vector2(380, 30);
            headerRect.anchoredPosition = new Vector2(0, -25);

            // Collect all items into a list for display
            var collectedItems = new List<(Sprite sprite, int count, string label)>();

            // Add rewards
            if (gm?.RewardCounts != null)
            {
                foreach (var kv in gm.RewardCounts)
                {
                    if (kv.Value > 0)
                    {
                        var sprite = Gameplay.PlaceholderAssets.GetRewardSprite(kv.Key);
                        collectedItems.Add((sprite, kv.Value, kv.Key.ToString()));
                    }
                }
            }

            // Add weapons
            if (gm?.WeaponCounts != null)
            {
                foreach (var kv in gm.WeaponCounts)
                {
                    if (kv.Value > 0)
                    {
                        var sprite = Gameplay.PlaceholderAssets.GetWeaponPickupSprite(kv.Key);
                        collectedItems.Add((sprite, kv.Value, kv.Key.ToString()));
                    }
                }
            }

            if (collectedItems.Count == 0)
            {
                // No items collected message (centered in panel, below header)
                var noItemsGO = new GameObject("NoItems");
                noItemsGO.transform.SetParent(itemsPanelGO.transform, false);
                var noItemsText = noItemsGO.AddComponent<Text>();
                noItemsText.text = "No items collected";
                noItemsText.fontSize = 20;
                noItemsText.color = new Color(0.5f, 0.5f, 0.6f);
                noItemsText.alignment = TextAnchor.MiddleCenter;
                noItemsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                var noItemsRect = noItemsGO.GetComponent<RectTransform>();
                noItemsRect.anchorMin = new Vector2(0.5f, 0.5f);
                noItemsRect.anchorMax = new Vector2(0.5f, 0.5f);
                noItemsRect.sizeDelta = new Vector2(350, 40);
                noItemsRect.anchoredPosition = new Vector2(0, -20);
                return;
            }

            // Layout items in a grid (4 columns max) with proper padding
            int columns = 4;
            float iconSize = 56f;
            float spacingX = 20f;
            float spacingY = 24f;
            float startX = -((columns - 1) * (iconSize + spacingX)) / 2f;
            float startY = 80f; // Start below header with padding

            for (int i = 0; i < collectedItems.Count; i++)
            {
                int col = i % columns;
                int row = i / columns;
                float x = startX + col * (iconSize + spacingX);
                float y = startY - row * (iconSize + spacingY + 20f);

                CreateItemIcon(itemsPanelGO.transform, collectedItems[i].sprite,
                    collectedItems[i].count, new Vector2(x, y), iconSize);
            }
        }

        private void CreateItemIcon(Transform parent, Sprite sprite, int count, Vector2 position, float size)
        {
            // Container for icon and count
            var containerGO = new GameObject("ItemIcon");
            containerGO.transform.SetParent(parent, false);
            var containerRect = containerGO.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(size, size + 24);
            containerRect.anchoredPosition = position;

            // Icon background (slightly lighter for contrast against panel)
            var bgGO = new GameObject("IconBg");
            bgGO.transform.SetParent(containerGO.transform, false);
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.12f, 0.10f, 0.22f, 1f);
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 1f);
            bgRect.anchorMax = new Vector2(0.5f, 1f);
            bgRect.sizeDelta = new Vector2(size, size);
            bgRect.anchoredPosition = new Vector2(0, -size / 2f);

            // Sprite icon
            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(bgGO.transform, false);
            var iconImg = iconGO.AddComponent<Image>();
            iconImg.sprite = sprite;
            iconImg.preserveAspect = true;
            var iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.1f, 0.1f);
            iconRect.anchorMax = new Vector2(0.9f, 0.9f);
            iconRect.sizeDelta = Vector2.zero;
            iconRect.anchoredPosition = Vector2.zero;

            // Count text below icon
            var countGO = new GameObject("Count");
            countGO.transform.SetParent(containerGO.transform, false);
            var countText = countGO.AddComponent<Text>();
            countText.text = $"x{count}";
            countText.fontSize = 18;
            countText.color = count > 1 ? new Color(1f, 0.85f, 0.1f) : Color.white;
            countText.alignment = TextAnchor.MiddleCenter;
            countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var countRect = countGO.GetComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0.5f, 0f);
            countRect.anchorMax = new Vector2(0.5f, 0f);
            countRect.sizeDelta = new Vector2(size, 24);
            countRect.anchoredPosition = new Vector2(0, 12);
        }

        private void CreateStatRow(Transform parent, string label, string value, float yPos, Color valueColor)
        {
            // Label (left-aligned with padding)
            var labelGO = new GameObject("StatLabel");
            labelGO.transform.SetParent(parent, false);
            var labelText = labelGO.AddComponent<Text>();
            labelText.text = label;
            labelText.fontSize = 22;
            labelText.color = new Color(0.75f, 0.75f, 0.85f);
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.sizeDelta = new Vector2(220, 32);
            labelRect.anchoredPosition = new Vector2(25, yPos);

            // Value (right-aligned with padding)
            var valueGO = new GameObject("StatValue");
            valueGO.transform.SetParent(parent, false);
            var valueText = valueGO.AddComponent<Text>();
            valueText.text = value;
            valueText.fontSize = 24;
            valueText.color = valueColor;
            valueText.alignment = TextAnchor.MiddleRight;
            valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var valueRect = valueGO.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(1f, 0.5f);
            valueRect.anchorMax = new Vector2(1f, 0.5f);
            valueRect.pivot = new Vector2(1f, 0.5f);
            valueRect.sizeDelta = new Vector2(160, 32);
            valueRect.anchoredPosition = new Vector2(-25, yPos);
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
