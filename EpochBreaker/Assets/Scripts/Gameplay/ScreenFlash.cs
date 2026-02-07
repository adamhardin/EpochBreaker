using UnityEngine;
using UnityEngine.UI;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Full-screen flash effect for level transitions, damage events, etc.
    /// Self-destructs after the flash completes.
    /// </summary>
    public class ScreenFlash : MonoBehaviour
    {
        private Image _flashImage;
        private float _duration;
        private float _elapsed;
        private Color _color;
        private bool _fadeIn; // true = fade in then out, false = fade out only

        /// <summary>
        /// Flash the screen white (or any color). Fades out over duration.
        /// </summary>
        public static void Flash(Color color, float duration)
        {
            var go = new GameObject("ScreenFlash");
            var flash = go.AddComponent<ScreenFlash>();
            flash.Setup(color, duration, false);
        }

        /// <summary>
        /// Flash in then out (for level transitions).
        /// </summary>
        public static void TransitionFlash(float duration)
        {
            var go = new GameObject("ScreenFlash");
            var flash = go.AddComponent<ScreenFlash>();
            flash.Setup(Color.white, duration, true);
        }

        private void Setup(Color color, float duration, bool fadeIn)
        {
            _color = color;
            _duration = duration;
            _fadeIn = fadeIn;
            _elapsed = 0f;

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            _flashImage = new GameObject("FlashOverlay").AddComponent<Image>();
            _flashImage.transform.SetParent(transform, false);
            _flashImage.color = _fadeIn ? new Color(color.r, color.g, color.b, 0f) : color;

            var rect = _flashImage.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            _flashImage.raycastTarget = false;
        }

        private void Update()
        {
            _elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_elapsed / _duration);

            float alpha;
            if (_fadeIn)
            {
                // Fade in for first half, fade out for second half
                alpha = t < 0.5f
                    ? Mathf.Lerp(0f, 1f, t * 2f)
                    : Mathf.Lerp(1f, 0f, (t - 0.5f) * 2f);
            }
            else
            {
                alpha = 1f - t;
            }

            _flashImage.color = new Color(_color.r, _color.g, _color.b, alpha);

            if (t >= 1f)
                Destroy(gameObject);
        }
    }
}
