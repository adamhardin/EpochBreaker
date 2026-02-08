using UnityEngine;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Level exit portal. Boss-gated: starts locked (dim) when a boss exists in the level.
    /// Activates on boss death with a camera pan cinematic. Triggers level completion on player contact.
    /// </summary>
    public class ExitPortal : MonoBehaviour
    {
        public static ExitPortal Instance { get; private set; }

        private bool _active;
        private bool _triggered;
        private SpriteRenderer _sr;
        private Color _fullColor;

        // Locked visual: desaturated/dim
        private static readonly Color LOCKED_TINT = new Color(0.3f, 0.3f, 0.4f, 0.7f);

        public bool IsActive => _active;

        private void Awake()
        {
            Instance = this;
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null)
                _fullColor = _sr.color;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// Set the portal to locked (dim, non-interactive) state.
        /// Used when the level has a boss that must be defeated first.
        /// </summary>
        public void SetLocked()
        {
            _active = false;
            if (_sr != null)
                _sr.color = _fullColor * LOCKED_TINT;
        }

        /// <summary>
        /// Activate the portal (full brightness, interactive).
        /// Called when no boss exists, or after boss death.
        /// </summary>
        public void Activate()
        {
            _active = true;
            if (_sr != null)
                _sr.color = _fullColor;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_triggered) return;
            if (!_active) return;
            if (!other.CompareTag("Player")) return;

            _triggered = true;
            GameManager.Instance?.CompleteLevel();
        }
    }
}
