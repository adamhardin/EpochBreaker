using UnityEngine;
using UnityEngine.UI;

namespace EpochBreaker.UI
{
    /// <summary>
    /// Displays tutorial hint text as a floating box near the top of the screen.
    /// Reads from TutorialManager.Instance for the current hint text and visibility.
    /// </summary>
    public class TutorialHintUI : MonoBehaviour
    {
        private Canvas _canvas;
        private GameObject _hintPanel;
        private Text _hintText;
        private Image _panelBg;
        private CanvasGroup _canvasGroup;
        private float _fadeTarget;
        private float _currentAlpha;

        private void Start()
        {
            CreateUI();
        }

        private void Update()
        {
            var tm = Gameplay.TutorialManager.Instance;
            if (tm == null || !tm.IsTutorialActive)
            {
                SetVisible(false);
                return;
            }

            if (tm.HintVisible && !string.IsNullOrEmpty(tm.CurrentHintText))
            {
                _hintText.text = tm.CurrentHintText;
                SetVisible(true);
            }
            else
            {
                SetVisible(false);
            }

            // Smooth fade
            _currentAlpha = Mathf.MoveTowards(_currentAlpha, _fadeTarget, Time.deltaTime * 4f);
            if (_canvasGroup != null)
                _canvasGroup.alpha = _currentAlpha;
        }

        private void SetVisible(bool visible)
        {
            _fadeTarget = visible ? 1f : 0f;
            if (_hintPanel != null)
                _hintPanel.SetActive(visible || _currentAlpha > 0.01f);
        }

        private void CreateUI()
        {
            var canvasGO = new GameObject("TutorialHintCanvas");
            canvasGO.transform.SetParent(transform);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 95; // Above HUD, below menus

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Hint panel (centered, near top)
            _hintPanel = new GameObject("HintPanel");
            _hintPanel.transform.SetParent(canvasGO.transform, false);

            _canvasGroup = _hintPanel.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _currentAlpha = 0f;
            _fadeTarget = 0f;

            _panelBg = _hintPanel.AddComponent<Image>();
            _panelBg.color = new Color(0.05f, 0.05f, 0.15f, 0.85f);

            var panelRect = _hintPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.sizeDelta = new Vector2(700, 60);
            panelRect.anchoredPosition = new Vector2(0, -120);

            // Hint text
            var textGO = new GameObject("HintText");
            textGO.transform.SetParent(_hintPanel.transform, false);
            _hintText = textGO.AddComponent<Text>();
            _hintText.text = "";
            _hintText.fontSize = 26;
            _hintText.color = new Color(1f, 0.95f, 0.7f);
            _hintText.alignment = TextAnchor.MiddleCenter;
            _hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _hintText.horizontalOverflow = HorizontalWrapMode.Wrap;

            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0f);
            textRect.anchorMax = new Vector2(0.95f, 1f);
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            // Arrow indicator (small triangle below panel)
            var arrowGO = new GameObject("Arrow");
            arrowGO.transform.SetParent(_hintPanel.transform, false);
            var arrowText = arrowGO.AddComponent<Text>();
            arrowText.text = "\u25bc"; // Down triangle
            arrowText.fontSize = 20;
            arrowText.color = new Color(1f, 0.95f, 0.7f, 0.6f);
            arrowText.alignment = TextAnchor.MiddleCenter;
            arrowText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var arrowRect = arrowGO.GetComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(0.5f, 0f);
            arrowRect.anchorMax = new Vector2(0.5f, 0f);
            arrowRect.pivot = new Vector2(0.5f, 1f);
            arrowRect.sizeDelta = new Vector2(30, 20);
            arrowRect.anchoredPosition = new Vector2(0, 0);

            _hintPanel.SetActive(false);
        }
    }
}
