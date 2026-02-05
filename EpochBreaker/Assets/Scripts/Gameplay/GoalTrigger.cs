using UnityEngine;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Placed at the level goal. Triggers level completion on player contact.
    /// </summary>
    public class GoalTrigger : MonoBehaviour
    {
        private bool _triggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_triggered) return;
            if (!other.CompareTag("Player")) return;

            _triggered = true;
            GameManager.Instance?.CompleteLevel();
        }
    }
}
