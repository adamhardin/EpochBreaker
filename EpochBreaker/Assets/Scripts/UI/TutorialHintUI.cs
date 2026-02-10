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
        private Image _hintImg;
        private Sprite _hintSprite;
        private string _lastHintText;
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
                if (tm.CurrentHintText != _lastHintText)
                {
                    _lastHintText = tm.CurrentHintText;
                    if (_hintSprite != null) { Destroy(_hintSprite.texture); Destroy(_hintSprite); }
                    _hintSprite = Gameplay.PlaceholderAssets.CreatePixelTextSprite(
                        tm.CurrentHintText, new Color(1f, 0.95f, 0.7f), 3);
                    _hintImg.sprite = _hintSprite;
                    _hintImg.SetNativeSize();
                }
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
            _canvas.pixelPerfect = true;

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
            _hintPanel.AddComponent<RectMask2D>();

            var panelRect = _hintPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.sizeDelta = new Vector2(700, 60);
            panelRect.anchoredPosition = new Vector2(0, -120);

            // Hint text (pixel sprite — updated dynamically in Update)
            var textGO = new GameObject("HintText");
            textGO.transform.SetParent(_hintPanel.transform, false);
            _hintImg = textGO.AddComponent<Image>();
            _hintImg.preserveAspect = true;
            _hintImg.raycastTarget = false;

            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;

            // Arrow indicator (pixel sprite "V" below panel — sibling to avoid RectMask2D clip)
            var arrowGO = new GameObject("Arrow");
            arrowGO.transform.SetParent(canvasGO.transform, false);
            var arrowImg = arrowGO.AddComponent<Image>();
            arrowImg.sprite = Gameplay.PlaceholderAssets.GetPixelTextSprite("V", new Color(1f, 0.95f, 0.7f, 0.6f), 2);
            arrowImg.preserveAspect = true;
            arrowImg.raycastTarget = false;
            arrowImg.SetNativeSize();
            var arrowRect = arrowGO.GetComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(0.5f, 1f);
            arrowRect.anchorMax = new Vector2(0.5f, 1f);
            arrowRect.pivot = new Vector2(0.5f, 1f);
            arrowRect.anchoredPosition = new Vector2(0, -180);

            _hintPanel.SetActive(false);
        }
    }
}
