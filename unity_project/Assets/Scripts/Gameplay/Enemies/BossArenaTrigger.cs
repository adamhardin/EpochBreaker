using UnityEngine;

namespace SixteenBit.Gameplay
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

            // Destroy trigger after use
            Destroy(gameObject);
        }
    }
}
