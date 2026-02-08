using UnityEngine;
using UnityEngine.UI;
using EpochBreaker.Generative;
using System.Collections.Generic;

namespace EpochBreaker.UI
{
    /// <summary>
    /// Inter-level summary screen showing full score breakdown, collected items with sprites, and continue button.
    /// </summary>
    public class LevelCompleteUI : MonoBehaviour
    {
        private Button _continueBtn;
        private Button _replayBtn;
        private Button _menuBtn;
        private Text _continueBtnLabel;
        private Text _hintText;

        // Sprint 8: Share and ghost replay
        private Text _copiedFeedbackText;
        private float _copiedFeedbackTimer;

        private void Start()
        {
            CreateUI();
        }

        private void Update()
        {
            var gm = Gameplay.GameManager.Instance;
            if (gm == null) return;

            float remaining = gm.LevelCompleteRemainingDelay;
            bool locked = remaining > 0f;

            if (_continueBtn) _continueBtn.interactable = !locked;
            if (_replayBtn) _replayBtn.interactable = !locked;
            if (_menuBtn) _menuBtn.interactable = !locked;

            if (_continueBtnLabel)
                _continueBtnLabel.text = locked ? $"CONTINUE ({Mathf.CeilToInt(remaining)}s)" : "CONTINUE";

            if (_hintText)
                _hintText.color = locked ? new Color(0.3f, 0.3f, 0.35f) : new Color(0.5f, 0.5f, 0.6f);

            // Fade out "Copied!" feedback
            if (_copiedFeedbackText != null && _copiedFeedbackTimer > 0f)
            {
                _copiedFeedbackTimer -= Time.deltaTime;
                float alpha = Mathf.Clamp01(_copiedFeedbackTimer / 0.5f);
                _copiedFeedbackText.color = new Color(0.4f, 1f, 0.4f, alpha);
            }
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

            // Profile frame border (cosmetic)
            CreateProfileFrame(canvasGO.transform);

            var gm = Gameplay.GameManager.Instance;
            int levelNum = gm?.LevelNumber ?? 1;
            float elapsed = gm?.LevelElapsedTime ?? 0f;
            int score = gm?.Score ?? 0;
            int totalScore = gm?.TotalScore ?? 0;
            int timeScore = gm?.TimeScore ?? 0;
            int itemBonus = gm?.ItemBonusScore ?? 0;
            int enemyBonus = gm?.EnemyBonusScore ?? 0;
            int combatMastery = gm?.CombatMasteryScore ?? 0;
            int exploration = gm?.ExplorationScore ?? 0;
            int preservation = gm?.PreservationScore ?? 0;
            int stars = Gameplay.GameManager.GetStarRating(score);
            int enemies = gm?.EnemiesKilled ?? 0;
            int totalRelics = gm?.TotalRelics ?? 0;
            int relicsDestroyed = gm?.RelicsDestroyed ?? 0;
            int relicsPreserved = Mathf.Max(0, totalRelics - relicsDestroyed);
            string epochName = gm != null ? gm.CurrentLevelID.EpochName : "";
            string levelCode = gm != null ? gm.CurrentLevelID.ToCode() : "";

            // Capture for closures
            int capturedScore = score;
            int capturedStars = stars;
            string capturedCode = levelCode;

            // === HEADER SECTION ===

            CreateText(canvasGO.transform, $"LEVEL {levelNum} COMPLETE", 48,
                new Color(1f, 0.85f, 0.1f), new Vector2(0, 460));

            CreateText(canvasGO.transform, epochName, 22,
                new Color(0.7f, 0.7f, 0.8f), new Vector2(0, 418));

            // Level code + inline COPY button
            CreateText(canvasGO.transform, $"Level Code: {levelCode}", 18,
                new Color(0.5f, 0.8f, 1f), new Vector2(-40, 390));

            var copyBtn = CreateButton(canvasGO.transform, "COPY", new Vector2(160, 390),
                new Color(0.25f, 0.5f, 0.7f), () => {
                    string shareText = Gameplay.LevelShareManager.GenerateChallengeShareText(
                        capturedScore, capturedStars, capturedCode);
                    Gameplay.GameManager.CopyToClipboard(shareText);
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    ShowCopiedFeedback();
                });
            var copyRect = copyBtn.GetComponent<RectTransform>();
            copyRect.sizeDelta = new Vector2(80, 30);
            var copyLabel = copyBtn.GetComponentInChildren<Text>();
            if (copyLabel != null) copyLabel.fontSize = 16;

            // "Copied!" feedback (near the COPY button, hidden until triggered)
            var copiedGO = CreateText(canvasGO.transform, "Copied to clipboard!", 16,
                new Color(0.4f, 1f, 0.4f, 0f), new Vector2(0, 365));
            _copiedFeedbackText = copiedGO.GetComponent<Text>();

            // Stars
            string starDisplay = new string('*', stars) + new string('-', 3 - stars);
            CreateText(canvasGO.transform, starDisplay, 44,
                new Color(1f, 0.85f, 0.1f), new Vector2(0, 340));

            // Personal best comparison
            int bestScore = GetBestScoreForLevel(levelCode);
            if (score > bestScore && bestScore > 0)
            {
                CreateText(canvasGO.transform, $"NEW RECORD! +{score - bestScore}", 22,
                    new Color(1f, 0.5f, 0.2f), new Vector2(0, 305));
            }
            else if (bestScore > 0)
            {
                CreateText(canvasGO.transform, $"Best: {bestScore}", 18,
                    new Color(0.6f, 0.6f, 0.7f), new Vector2(0, 305));
            }

            // Star threshold hint
            if (stars < 3)
            {
                int nextThreshold = stars == 1 ? 7800 : 13500;
                int needed = nextThreshold - score;
                CreateText(canvasGO.transform, $"Next star: {nextThreshold} (need {needed} more)", 16,
                    new Color(0.5f, 0.5f, 0.6f), new Vector2(0, 280));
            }

            // === SCORE PANEL (left, full details always shown) ===

            Color statColor = new Color(0.75f, 0.75f, 0.85f);
            Color greenColor = new Color(0.4f, 0.9f, 0.4f);
            Color cyanColor = new Color(0.3f, 0.85f, 0.85f);
            Color blueColor = new Color(0.5f, 0.6f, 1f);
            Color goldColor = new Color(1f, 0.85f, 0.1f);

            var panelGO = new GameObject("ScorePanel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelImg = panelGO.AddComponent<Image>();
            panelImg.color = new Color(0.08f, 0.06f, 0.14f, 0.95f);
            var panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(420, 430);
            panelRect.anchoredPosition = new Vector2(-230, 20);

            float rowY = 185f;
            float rowSpacing = 30f;

            // Panel header "SCORE"
            var scoreHeaderGO = new GameObject("ScoreHeader");
            scoreHeaderGO.transform.SetParent(panelGO.transform, false);
            var scoreHeaderText = scoreHeaderGO.AddComponent<Text>();
            scoreHeaderText.text = "SCORE";
            scoreHeaderText.fontSize = 24;
            scoreHeaderText.color = new Color(0.9f, 0.9f, 1f);
            scoreHeaderText.alignment = TextAnchor.MiddleCenter;
            scoreHeaderText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var scoreHeaderRect = scoreHeaderGO.GetComponent<RectTransform>();
            scoreHeaderRect.anchorMin = new Vector2(0.5f, 0.5f);
            scoreHeaderRect.anchorMax = new Vector2(0.5f, 0.5f);
            scoreHeaderRect.sizeDelta = new Vector2(380, 30);
            scoreHeaderRect.anchoredPosition = new Vector2(0, rowY);
            rowY -= 38f;

            // Speed
            int minutes = (int)(elapsed / 60f);
            float secs = elapsed % 60f;
            CreateStatRow(panelGO.transform, $"Speed ({minutes}:{secs:00.0})", $"{timeScore}", rowY,
                new Color(1f, 0.9f, 0.4f));
            rowY -= rowSpacing;

            // Items & Enemies
            int itemEnemyTotal = itemBonus + enemyBonus;
            CreateStatRow(panelGO.transform, "Items & Enemies",
                itemEnemyTotal > 0 ? $"+{itemEnemyTotal}" : "0", rowY,
                itemEnemyTotal > 0 ? greenColor : statColor);
            rowY -= rowSpacing;

            // Combat Mastery
            CreateStatRow(panelGO.transform, "Combat Mastery",
                combatMastery > 0 ? $"+{combatMastery}" : "0", rowY,
                combatMastery > 0 ? cyanColor : statColor);
            rowY -= rowSpacing;

            // Combat sub-breakdown
            int shotsFired = gm?.ShotsFired ?? 0;
            int bestStreak = gm?.BestNoDamageStreak ?? 0;
            bool bossWon = gm?.BossDefeated ?? false;
            string effStr = shotsFired > 0
                ? $"  Efficiency: {enemies}/{shotsFired} shots"
                : (enemies > 0 ? "  Stomps only" : "");
            string streakStr = bestStreak > 0 ? $"  No-hit streak: x{bestStreak}" : "";
            string bossStr = bossWon ? "  Boss defeated" : "";
            string subText = (effStr + (streakStr.Length > 0 ? "\n" + streakStr : "")
                + (bossStr.Length > 0 ? "\n" + bossStr : "")).Trim();
            if (subText.Length > 0)
            {
                CreateSmallText(panelGO.transform, subText, rowY, new Color(0.6f, 0.6f, 0.7f));
                rowY -= (subText.Split('\n').Length * 14f) + 8f;
            }
            else
            {
                rowY -= 10f;
            }

            // Exploration
            CreateStatRow(panelGO.transform, "Exploration",
                exploration > 0 ? $"+{exploration}" : "0", rowY,
                exploration > 0 ? blueColor : statColor);
            rowY -= rowSpacing;

            // Archaeology
            string preserveStr = preservation > 0 ? $"+{preservation}" : "0";
            if (totalRelics > 0)
                preserveStr += $" ({relicsPreserved}/{totalRelics})";
            CreateStatRow(panelGO.transform, "Archaeology", preserveStr, rowY,
                preservation > 0 ? goldColor : statColor);
            rowY -= rowSpacing;

            // Enemies
            string enemyStr = enemies > 0 ? $"{enemies} defeated" : "None";
            CreateStatRow(panelGO.transform, "Enemies", enemyStr, rowY,
                enemies > 0 ? statColor : new Color(0.5f, 0.5f, 0.6f));
            rowY -= 18f;

            // Separator line
            var sepGO = new GameObject("Separator");
            sepGO.transform.SetParent(panelGO.transform, false);
            var sepImg = sepGO.AddComponent<Image>();
            sepImg.color = new Color(0.3f, 0.3f, 0.4f, 0.6f);
            var sepRect = sepGO.GetComponent<RectTransform>();
            sepRect.anchorMin = new Vector2(0.5f, 0.5f);
            sepRect.anchorMax = new Vector2(0.5f, 0.5f);
            sepRect.sizeDelta = new Vector2(360, 1);
            sepRect.anchoredPosition = new Vector2(0, rowY);
            rowY -= 18f;

            // Level Score
            CreateStatRow(panelGO.transform, "Level Score", $"{score}", rowY, Color.white);
            rowY -= rowSpacing;

            // Total Score
            CreateStatRow(panelGO.transform, "Total Score", $"{totalScore}", rowY, goldColor);
            rowY -= rowSpacing;

            // Tip based on lowest scoring component
            string tip = GetTipForLowestComponent(timeScore, itemBonus + enemyBonus,
                combatMastery, exploration, preservation);
            if (!string.IsNullOrEmpty(tip))
            {
                CreateSmallText(panelGO.transform, $"Tip: {tip}", rowY,
                    new Color(0.6f, 0.8f, 0.5f));
                rowY -= 30f;
            }

            // Auto-size score panel to fit content
            float panelContentHeight = 185f - rowY + 30f;
            panelRect.sizeDelta = new Vector2(420, Mathf.Max(430f, panelContentHeight));

            // === ITEMS PANEL (right, aligned with score panel) ===
            CreateItemsPanel(canvasGO.transform, gm);

            // === BOTTOM SECTION ===
            // Compute btnY from score panel bottom edge so buttons never overlap content
            float panelBottom = panelRect.anchoredPosition.y - panelRect.sizeDelta.y / 2f;
            float btnY = Mathf.Min(-210f, panelBottom - 30f);

            // Challenge result (if friend challenge active)
            int friendTarget = gm?.FriendChallengeScore ?? 0;
            if (friendTarget > 0)
            {
                bool beaten = score >= friendTarget;
                int diff = score - friendTarget;
                string resultLabel = beaten ? "CHALLENGE BEATEN!" : "CHALLENGE FAILED";
                Color resultColor = beaten
                    ? new Color(0.4f, 1f, 0.4f)
                    : new Color(1f, 0.4f, 0.35f);
                string comparison = beaten
                    ? $"Your: {score:N0} vs Target: {friendTarget:N0}  (+{diff:N0})"
                    : $"Your: {score:N0} vs Target: {friendTarget:N0}  ({diff:N0})";

                CreateText(canvasGO.transform, resultLabel, 20, resultColor, new Vector2(0, btnY));
                btnY -= 22f;
                CreateText(canvasGO.transform, comparison, 16,
                    new Color(0.7f, 0.7f, 0.8f), new Vector2(0, btnY));
                btnY -= 35f;
            }

            // Continue button (primary action)
            _continueBtn = CreateButton(canvasGO.transform, "CONTINUE", new Vector2(0, btnY),
                new Color(0.2f, 0.6f, 0.3f), () => {
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.GameManager.Instance?.NextLevel();
                });
            var continueBtnRect = _continueBtn.GetComponent<RectTransform>();
            continueBtnRect.sizeDelta = new Vector2(280, 55);
            _continueBtnLabel = _continueBtn.GetComponentInChildren<Text>();
            btnY -= 65f;

            // Replay / Menu buttons
            _replayBtn = CreateButton(canvasGO.transform, "REPLAY", new Vector2(-145, btnY),
                new Color(0.3f, 0.4f, 0.6f), () => {
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.GameManager.Instance?.RestartLevel();
                });
            _menuBtn = CreateButton(canvasGO.transform, "MENU", new Vector2(145, btnY),
                new Color(0.4f, 0.3f, 0.3f), () => {
                    Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                    Gameplay.GameManager.Instance?.ReturnToTitle();
                });
            btnY -= 60f;

            // Contextual buttons (Watch Replay, Share Daily/Weekly)
            if (Gameplay.GhostReplaySystem.HasGhost(capturedCode))
            {
                CreateButton(canvasGO.transform, "WATCH REPLAY", new Vector2(0, btnY),
                    new Color(0.5f, 0.4f, 0.6f), () => {
                        Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                        Gameplay.GameManager.Instance?.StartGhostReplay(capturedCode);
                    });
                btnY -= 55f;
            }

            var todayID = Gameplay.DailyChallengeManager.GetTodayLevelID();
            var currentID = gm?.CurrentLevelID ?? default;
            if (currentID.Seed == todayID.Seed && currentID.Epoch == todayID.Epoch)
            {
                CreateButton(canvasGO.transform, "SHARE DAILY", new Vector2(0, btnY),
                    new Color(0.3f, 0.5f, 0.7f), () => {
                        string shareText = Gameplay.DailyChallengeManager.GetShareText(capturedScore, capturedStars);
                        Gameplay.GameManager.CopyToClipboard(shareText);
                        Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                        ShowCopiedFeedback();
                    });
                btnY -= 55f;
            }

            var weeklyID = Gameplay.DailyChallengeManager.GetWeeklyLevelID();
            if (currentID.Seed == weeklyID.Seed && currentID.Epoch == weeklyID.Epoch)
            {
                CreateButton(canvasGO.transform, "SHARE WEEKLY", new Vector2(0, btnY),
                    new Color(0.3f, 0.55f, 0.5f), () => {
                        string shareText = Gameplay.DailyChallengeManager.GetWeeklyShareText(capturedScore, capturedStars);
                        Gameplay.GameManager.CopyToClipboard(shareText);
                        Gameplay.AudioManager.PlaySFX(Gameplay.PlaceholderAudio.GetMenuSelectSFX());
                        ShowCopiedFeedback();
                    });
                btnY -= 55f;
            }

            // Keyboard hint
            var hintGO = CreateText(canvasGO.transform, "Enter: Continue | R: Replay | Esc/`: Menu", 14,
                new Color(0.5f, 0.5f, 0.6f), new Vector2(0, btnY - 10f));
            _hintText = hintGO.GetComponent<Text>();
        }

        private void CreateItemsPanel(Transform parent, Gameplay.GameManager gm)
        {
            var itemsPanelGO = new GameObject("ItemsPanel");
            itemsPanelGO.transform.SetParent(parent, false);
            var itemsPanelImg = itemsPanelGO.AddComponent<Image>();
            itemsPanelImg.color = new Color(0.08f, 0.06f, 0.14f, 0.95f);
            var itemsPanelRect = itemsPanelGO.GetComponent<RectTransform>();
            itemsPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            itemsPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            itemsPanelRect.sizeDelta = new Vector2(420, 430);
            itemsPanelRect.anchoredPosition = new Vector2(230, 20);

            // Panel header
            var headerGO = new GameObject("Header");
            headerGO.transform.SetParent(itemsPanelGO.transform, false);
            var headerText = headerGO.AddComponent<Text>();
            headerText.text = "ITEMS COLLECTED";
            headerText.fontSize = 24;
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

            int columns = 4;
            float iconSize = 56f;
            float spacingX = 20f;
            float spacingY = 24f;
            float startX = -((columns - 1) * (iconSize + spacingX)) / 2f;
            float startY = 80f;

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
            var containerGO = new GameObject("ItemIcon");
            containerGO.transform.SetParent(parent, false);
            var containerRect = containerGO.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(size, size + 24);
            containerRect.anchoredPosition = position;

            var bgGO = new GameObject("IconBg");
            bgGO.transform.SetParent(containerGO.transform, false);
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.12f, 0.10f, 0.22f, 1f);
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 1f);
            bgRect.anchorMax = new Vector2(0.5f, 1f);
            bgRect.sizeDelta = new Vector2(size, size);
            bgRect.anchoredPosition = new Vector2(0, -size / 2f);

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

        private void ShowCopiedFeedback()
        {
            if (_copiedFeedbackText != null)
            {
                _copiedFeedbackText.color = new Color(0.4f, 1f, 0.4f, 1f);
                _copiedFeedbackTimer = 2f;
            }
        }

        private GameObject CreateSmallText(Transform parent, string text, float yPos, Color color)
        {
            var go = new GameObject("SubText");
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = text;
            t.fontSize = 16;
            t.color = color;
            t.alignment = TextAnchor.UpperLeft;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(380, 60);
            rect.anchoredPosition = new Vector2(30, yPos);
            return go;
        }

        private void CreateStatRow(Transform parent, string label, string value, float yPos, Color valueColor)
        {
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

        private GameObject CreateText(Transform parent, string text, int fontSize, Color color, Vector2 position)
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

            return go;
        }

        private Button CreateButton(Transform parent, string text, Vector2 position,
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
            rect.sizeDelta = new Vector2(260, 50);
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

            // Prevent label image from intercepting button clicks
            var labelImg = textGO.GetComponent<Image>();
            if (labelImg != null) labelImg.raycastTarget = false;

            return btn;
        }

        private static string GetTipForLowestComponent(int time, int items, int combat, int exploration, int preservation)
        {
            var scores = new (string tip, int value)[]
            {
                ("Go faster! Speed is a big part of your score.", time),
                ("Collect more items and defeat more enemies.", items),
                ("Aim for kill streaks without taking damage.", combat),
                ("Explore hidden areas to boost your score.", exploration),
                ("Preserve relic tiles by avoiding collateral damage.", preservation),
            };

            int minVal = int.MaxValue;
            string bestTip = "";
            foreach (var s in scores)
            {
                if (s.value < minVal)
                {
                    minVal = s.value;
                    bestTip = s.tip;
                }
            }
            return bestTip;
        }

        private static int GetBestScoreForLevel(string levelCode)
        {
            var history = Gameplay.GameManager.LoadLevelHistory();
            if (history?.Entries == null) return 0;
            foreach (var entry in history.Entries)
            {
                if (entry.Code == levelCode)
                    return entry.Score;
            }
            return 0;
        }

        /// <summary>
        /// Draw a colored border around the level complete screen based on selected profile frame.
        /// </summary>
        private void CreateProfileFrame(Transform parent)
        {
            var cm = Gameplay.CosmeticManager.Instance;
            if (cm == null) return;

            var frame = cm.SelectedFrame;
            if (frame == Gameplay.ProfileFrame.None) return;
            if (!cm.IsFrameUnlocked(frame)) return;

            Color frameColor = Gameplay.CosmeticManager.GetFrameColor(frame);
            float borderWidth = 6f;

            var topGO = new GameObject("FrameTop");
            topGO.transform.SetParent(parent, false);
            var topImg = topGO.AddComponent<Image>();
            topImg.color = frameColor;
            var topRect = topGO.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0, 1);
            topRect.anchorMax = new Vector2(1, 1);
            topRect.pivot = new Vector2(0.5f, 1);
            topRect.sizeDelta = new Vector2(0, borderWidth);
            topRect.anchoredPosition = Vector2.zero;

            var bottomGO = new GameObject("FrameBottom");
            bottomGO.transform.SetParent(parent, false);
            var bottomImg = bottomGO.AddComponent<Image>();
            bottomImg.color = frameColor;
            var bottomRect = bottomGO.GetComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(0, 0);
            bottomRect.anchorMax = new Vector2(1, 0);
            bottomRect.pivot = new Vector2(0.5f, 0);
            bottomRect.sizeDelta = new Vector2(0, borderWidth);
            bottomRect.anchoredPosition = Vector2.zero;

            var leftGO = new GameObject("FrameLeft");
            leftGO.transform.SetParent(parent, false);
            var leftImg = leftGO.AddComponent<Image>();
            leftImg.color = frameColor;
            var leftRect = leftGO.GetComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0, 0);
            leftRect.anchorMax = new Vector2(0, 1);
            leftRect.pivot = new Vector2(0, 0.5f);
            leftRect.sizeDelta = new Vector2(borderWidth, 0);
            leftRect.anchoredPosition = Vector2.zero;

            var rightGO = new GameObject("FrameRight");
            rightGO.transform.SetParent(parent, false);
            var rightImg = rightGO.AddComponent<Image>();
            rightImg.color = frameColor;
            var rightRect = rightGO.GetComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(1, 0);
            rightRect.anchorMax = new Vector2(1, 1);
            rightRect.pivot = new Vector2(1, 0.5f);
            rightRect.sizeDelta = new Vector2(borderWidth, 0);
            rightRect.anchoredPosition = Vector2.zero;

            float glowWidth = 3f;
            Color glowColor = new Color(frameColor.r, frameColor.g, frameColor.b, 0.3f);

            var glowTopGO = new GameObject("GlowTop");
            glowTopGO.transform.SetParent(parent, false);
            var glowTopImg = glowTopGO.AddComponent<Image>();
            glowTopImg.color = glowColor;
            var glowTopRect = glowTopGO.GetComponent<RectTransform>();
            glowTopRect.anchorMin = new Vector2(0, 1);
            glowTopRect.anchorMax = new Vector2(1, 1);
            glowTopRect.pivot = new Vector2(0.5f, 1);
            glowTopRect.sizeDelta = new Vector2(0, glowWidth);
            glowTopRect.anchoredPosition = new Vector2(0, -borderWidth);

            var glowBottomGO = new GameObject("GlowBottom");
            glowBottomGO.transform.SetParent(parent, false);
            var glowBottomImg = glowBottomGO.AddComponent<Image>();
            glowBottomImg.color = glowColor;
            var glowBottomRect = glowBottomGO.GetComponent<RectTransform>();
            glowBottomRect.anchorMin = new Vector2(0, 0);
            glowBottomRect.anchorMax = new Vector2(1, 0);
            glowBottomRect.pivot = new Vector2(0.5f, 0);
            glowBottomRect.sizeDelta = new Vector2(0, glowWidth);
            glowBottomRect.anchoredPosition = new Vector2(0, borderWidth);

            string frameName = Gameplay.CosmeticManager.GetFrameName(frame);
            var labelGO = new GameObject("FrameLabel");
            labelGO.transform.SetParent(parent, false);
            var labelText = labelGO.AddComponent<Text>();
            labelText.text = frameName;
            labelText.fontSize = 16;
            labelText.color = frameColor;
            labelText.alignment = TextAnchor.MiddleRight;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(1, 1);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.pivot = new Vector2(1, 1);
            labelRect.sizeDelta = new Vector2(100, 20);
            labelRect.anchoredPosition = new Vector2(-12, -12);
        }
    }
}
