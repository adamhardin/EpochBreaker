using UnityEngine;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Invisible trigger that activates the boss when the player enters the arena.
    /// </summary>
    public class BossArenaTrigger : MonoBehaviour
    {
        private bool _triggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_triggered) return;
            if (!other.CompareTag("Player")) return;

            _triggered = true;

            // Find and activate the boss
            var levelLoader = FindAnyObjectByType<LevelLoader>();
            if (levelLoader != null && levelLoader.CurrentBoss != null)
            {
                levelLoader.CurrentBoss.Activate();
            }

            // Disable trigger (don't destroy — needed for respawn reactivation)
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        /// <summary>
        /// Re-enable the trigger so the boss can be reactivated after player respawn.
        /// </summary>
        public void ResetTrigger()
        {
            _triggered = false;
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
        }
    }
}
