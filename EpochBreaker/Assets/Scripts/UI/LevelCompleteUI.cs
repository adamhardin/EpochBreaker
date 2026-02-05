using UnityEngine;
using UnityEngine.UI;

namespace SixteenBit.UI
{
    /// <summary>
    /// Inter-level summary screen showing level stats, score, and continue button.
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
            int rewards = gm?.RewardsCollected ?? 0;
            int weapons = gm?.WeaponsCollected ?? 0;
            int bestMultiplier = gm?.GetBestMultiplier() ?? 0;
            string eraName = gm != null ? gm.CurrentLevelID.EraName : "";
            string diffName = gm != null ? gm.CurrentLevelID.DifficultyName : "";

            // Level header
            CreateText(canvasGO.transform, $"LEVEL {levelNum} COMPLETE", 48,
                new Color(1f, 0.85f, 0.1f), new Vector2(0, 300));

            // Era/Difficulty subtitle
            CreateText(canvasGO.transform, $"{eraName} - {diffName}", 22,
                new Color(0.7f, 0.7f, 0.8f), new Vector2(0, 250));

            // Stars
            string starDisplay = new string('*', stars) + new string('-', 3 - stars);
            CreateText(canvasGO.transform, starDisplay, 44,
                new Color(1f, 0.85f, 0.1f), new Vector2(0, 190));

            // Stats panel
            var panelGO = new GameObject("StatsPanel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.08f, 0.18f, 0.8f);
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(520, 330);
            panelRect.anchoredPosition = new Vector2(0, 0);

            Color statColor = new Color(0.75f, 0.75f, 0.85f);
            Color bonusColor = new Color(0.4f, 0.9f, 0.4f);
            Color goldColor = new Color(1f, 0.85f, 0.1f);

            // Time
            int minutes = (int)(elapsed / 60f);
            float seconds = elapsed % 60f;
            CreateStatRow(panelGO.transform, "Time", $"{minutes}:{seconds:00.0}", 130, statColor);

            // Time score
            CreateStatRow(panelGO.transform, "Time Score", $"{timeScore}", 90, statColor);

            // Items collected with bonus
            int totalItems = rewards + weapons;
            string itemStr = totalItems > 0 ? $"{totalItems}  (+{itemBonus})" : "0";
            CreateStatRow(panelGO.transform, "Items Collected", itemStr, 50,
                totalItems > 0 ? bonusColor : statColor);

            // Enemies defeated with bonus
            string enemyStr = enemies > 0 ? $"{enemies}  (+{enemyBonus})" : "0";
            CreateStatRow(panelGO.transform, "Enemies Defeated", enemyStr, 10,
                enemies > 0 ? bonusColor : statColor);

            // Best multiplier
            if (bestMultiplier > 1)
            {
                CreateStatRow(panelGO.transform, "Best Multiplier", $"x{bestMultiplier}", -30, goldColor);
            }

            // Level score (highlighted)
            CreateStatRow(panelGO.transform, "Level Score", $"{score}", -80, Color.white);

            // Total score (highlighted)
            CreateStatRow(panelGO.transform, "Total Score", $"{totalScore}", -130, goldColor);

            // Continue button (primary action)
            CreateButton(canvasGO.transform, "CONTINUE", new Vector2(0, -195),
                new Color(0.2f, 0.6f, 0.3f), () => {
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.GameManager.Instance?.NextLevel();
                });

            // Replay button
            CreateButton(canvasGO.transform, "REPLAY", new Vector2(-140, -270),
                new Color(0.3f, 0.4f, 0.6f), () => {
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.GameManager.Instance?.RestartLevel();
                });

            // Menu button
            CreateButton(canvasGO.transform, "MENU", new Vector2(140, -270),
                new Color(0.4f, 0.3f, 0.3f), () => {
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.GameManager.Instance?.ReturnToTitle();
                });

            // Keyboard hint
            CreateText(canvasGO.transform, "Enter: Continue | R: Replay | Esc: Menu", 14,
                new Color(0.5f, 0.5f, 0.6f), new Vector2(0, -340));
        }

        private void CreateStatRow(Transform parent, string label, string value, float yPos, Color valueColor)
        {
            // Label (left-aligned)
            var labelGO = new GameObject("StatLabel");
            labelGO.transform.SetParent(parent, false);
            var labelText = labelGO.AddComponent<Text>();
            labelText.text = label;
            labelText.fontSize = 24;
            labelText.color = new Color(0.75f, 0.75f, 0.85f);
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(300, 30);
            labelRect.anchoredPosition = new Vector2(-70, yPos);

            // Value (right-aligned)
            var valueGO = new GameObject("StatValue");
            valueGO.transform.SetParent(parent, false);
            var valueText = valueGO.AddComponent<Text>();
            valueText.text = value;
            valueText.fontSize = 26;
            valueText.color = valueColor;
            valueText.alignment = TextAnchor.MiddleRight;
            valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var valueRect = valueGO.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(0.5f, 0.5f);
            valueRect.anchorMax = new Vector2(0.5f, 0.5f);
            valueRect.sizeDelta = new Vector2(200, 30);
            valueRect.anchoredPosition = new Vector2(170, yPos);
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
