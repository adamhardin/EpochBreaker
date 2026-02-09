using UnityEngine;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Singleton manager for accessibility settings.
    /// Provides colorblind mode, screen shake intensity, font size scaling, and high contrast mode.
    /// All settings are persisted to PlayerPrefs.
    /// </summary>
    public class AccessibilityManager : MonoBehaviour
    {
        public static AccessibilityManager Instance { get; private set; }

        // PlayerPrefs keys
        private const string PREF_COLORBLIND = "EpochBreaker_ColorblindMode";
        private const string PREF_SHAKE_INTENSITY = "EpochBreaker_ShakeIntensity";
        private const string PREF_FONT_SCALE = "EpochBreaker_FontScale";
        private const string PREF_HIGH_CONTRAST = "EpochBreaker_HighContrast";

        // Cached values
        private bool _colorblindMode;
        private float _shakeIntensity = 1f;   // 0 to 1 (maps to 0-100% slider)
        private float _fontSizeScale = 1f;     // 0.8 to 1.5
        private bool _highContrastMode;

        /// <summary>
        /// When true, icons get shape overlays (circle, triangle, square, diamond)
        /// to supplement color-based information.
        /// </summary>
        public bool ColorblindMode
        {
            get => _colorblindMode;
            set
            {
                _colorblindMode = value;
                PlayerPrefs.SetInt(PREF_COLORBLIND, value ? 1 : 0);
                SafePrefs.Save();
            }
        }

        /// <summary>
        /// Multiplier applied to CameraController trauma for screen shake.
        /// 0 = no shake, 1 = full shake. Default 1.
        /// </summary>
        public float ScreenShakeIntensity
        {
            get => _shakeIntensity;
            set
            {
                _shakeIntensity = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(PREF_SHAKE_INTENSITY, _shakeIntensity);
                SafePrefs.Save();
            }
        }

        /// <summary>
        /// Multiplier for HUD text sizes. Disabled (locked to 1.0) until
        /// UI containers are validated at non-default scales.
        /// </summary>
        public float FontSizeScale
        {
            get => _fontSizeScale;
            set
            {
                _fontSizeScale = 1f;
                PlayerPrefs.SetFloat(PREF_FONT_SCALE, _fontSizeScale);
                SafePrefs.Save();
            }
        }

        /// <summary>
        /// When true, hazards get bright colored outlines for better visibility.
        /// </summary>
        public bool HighContrastMode
        {
            get => _highContrastMode;
            set
            {
                _highContrastMode = value;
                PlayerPrefs.SetInt(PREF_HIGH_CONTRAST, value ? 1 : 0);
                SafePrefs.Save();
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[Singleton] AccessibilityManager duplicate detected — destroying new instance.");
                Destroy(this);
                return;
            }
            Instance = this;

            // Load saved settings
            _colorblindMode = PlayerPrefs.GetInt(PREF_COLORBLIND, 0) == 1;
            _shakeIntensity = PlayerPrefs.GetFloat(PREF_SHAKE_INTENSITY, 1f);
            _fontSizeScale = PlayerPrefs.GetFloat(PREF_FONT_SCALE, 1f);
            _highContrastMode = PlayerPrefs.GetInt(PREF_HIGH_CONTRAST, 0) == 1;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// Get the colorblind shape character for a given index (used for icon overlays).
        /// 0=circle, 1=triangle, 2=square, 3=diamond, 4=cross, 5=star.
        /// </summary>
        public static string GetShapeLabel(int index)
        {
            switch (index % 6)
            {
                case 0: return "O";   // Circle
                case 1: return "^";   // Triangle
                case 2: return "#";   // Square
                case 3: return "<>";  // Diamond
                case 4: return "+";   // Cross
                case 5: return "*";   // Star
                default: return "O";
            }
        }

        /// <summary>
        /// Scale a font size by the accessibility font scale setting.
        /// Returns the original size if no AccessibilityManager exists.
        /// </summary>
        public static int ScaledFontSize(int baseSize)
        {
            if (Instance == null) return baseSize;
            return Mathf.RoundToInt(baseSize * Instance._fontSizeScale);
        }
    }
}
