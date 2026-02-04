using UnityEngine;
using UnityEngine.UI;

namespace SixteenBit.UI
{
    /// <summary>
    /// In-game HUD showing health hearts, score, weapon tier, and pause button.
    /// </summary>
    public class GameplayHUD : MonoBehaviour
    {
        private Canvas _canvas;
        private Image[] _hearts;
        private Text _scoreText;
        private Text _weaponText;
        private Text _eraText;
        private Gameplay.HealthSystem _healthSystem;

        private const int MAX_HEARTS = 5;

        private void Start()
        {
            CreateUI();
            FindPlayerHealth();
        }

        private void Update()
        {
            UpdateScore();
            UpdateWeaponDisplay();
        }

        private void FindPlayerHealth()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                _healthSystem = player.GetComponent<Gameplay.HealthSystem>();
                if (_healthSystem != null)
                {
                    _healthSystem.OnHealthChanged += UpdateHearts;
                    UpdateHearts(_healthSystem.CurrentHealth, _healthSystem.MaxHealth);
                }
            }
        }

        private void OnDestroy()
        {
            if (_healthSystem != null)
                _healthSystem.OnHealthChanged -= UpdateHearts;
        }

        private void CreateUI()
        {
            var canvasGO = new GameObject("HUDCanvas");
            canvasGO.transform.SetParent(transform);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 90;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Hearts (top-left)
            _hearts = new Image[MAX_HEARTS];
            for (int i = 0; i < MAX_HEARTS; i++)
            {
                var heartGO = new GameObject($"Heart_{i}");
                heartGO.transform.SetParent(canvasGO.transform, false);

                var img = heartGO.AddComponent<Image>();
                img.color = Color.red;

                var rect = heartGO.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.sizeDelta = new Vector2(40, 36);
                rect.anchoredPosition = new Vector2(20 + i * 48, -20);

                _hearts[i] = img;
            }

            // Score (top-left, below hearts)
            var scoreGO = CreateHUDText(canvasGO.transform, "Score: 0", TextAnchor.UpperLeft);
            _scoreText = scoreGO.GetComponent<Text>();
            var scoreRect = scoreGO.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0, 1);
            scoreRect.anchorMax = new Vector2(0, 1);
            scoreRect.pivot = new Vector2(0, 1);
            scoreRect.anchoredPosition = new Vector2(20, -70);

            // Weapon tier (top-right)
            var weaponGO = CreateHUDText(canvasGO.transform, "Weapon: Starting", TextAnchor.UpperRight);
            _weaponText = weaponGO.GetComponent<Text>();
            var weaponRect = weaponGO.GetComponent<RectTransform>();
            weaponRect.anchorMin = new Vector2(1, 1);
            weaponRect.anchorMax = new Vector2(1, 1);
            weaponRect.pivot = new Vector2(1, 1);
            weaponRect.anchoredPosition = new Vector2(-20, -20);

            // Era name (top-right, below weapon)
            var eraGO = CreateHUDText(canvasGO.transform, "", TextAnchor.UpperRight);
            _eraText = eraGO.GetComponent<Text>();
            _eraText.fontSize = 18;
            var eraRect = eraGO.GetComponent<RectTransform>();
            eraRect.anchorMin = new Vector2(1, 1);
            eraRect.anchorMax = new Vector2(1, 1);
            eraRect.pivot = new Vector2(1, 1);
            eraRect.anchoredPosition = new Vector2(-20, -50);

            var gm = Gameplay.GameManager.Instance;
            if (gm != null && gm.CurrentLevelID.Era >= 0)
                _eraText.text = $"{gm.CurrentLevelID.EraName} - {gm.CurrentLevelID.DifficultyName}";

            // Pause button (top-right corner)
            CreatePauseButton(canvasGO.transform);
        }

        private void UpdateHearts(int current, int max)
        {
            if (_hearts == null) return;
            for (int i = 0; i < MAX_HEARTS; i++)
            {
                if (_hearts[i] != null)
                {
                    _hearts[i].color = i < current ? Color.red : new Color(0.3f, 0.1f, 0.1f);
                }
            }
        }

        private void UpdateScore()
        {
            if (_scoreText != null && Gameplay.GameManager.Instance != null)
                _scoreText.text = $"Score: {Gameplay.GameManager.Instance.Score}";
        }

        private void UpdateWeaponDisplay()
        {
            if (_weaponText == null) return;
            var player = GameObject.FindWithTag("Player");
            if (player == null) return;
            var ws = player.GetComponent<Gameplay.WeaponSystem>();
            if (ws != null)
                _weaponText.text = $"Weapon: {ws.CurrentTier}";
        }

        private GameObject CreateHUDText(Transform parent, string text, TextAnchor alignment)
        {
            var go = new GameObject("HUDText");
            go.transform.SetParent(parent, false);

            var textComp = go.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = 22;
            textComp.color = Color.white;
            textComp.alignment = alignment;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.horizontalOverflow = HorizontalWrapMode.Overflow;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 30);

            return go;
        }

        private void CreatePauseButton(Transform parent)
        {
            var go = new GameObject("PauseButton");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.3f, 0.6f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => {
                Gameplay.GameManager.Instance?.PauseGame();
            });

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.sizeDelta = new Vector2(50, 50);
            rect.anchoredPosition = new Vector2(-80, -20);

            // Pause icon text
            var textGO = new GameObject("PauseLabel");
            textGO.transform.SetParent(go.transform, false);
            var textComp = textGO.AddComponent<Text>();
            textComp.text = "||";
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
