using UnityEngine;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Placed at checkpoint locations. Activates on player contact.
    /// </summary>
    public class CheckpointTrigger : MonoBehaviour
    {
        public Vector3 CheckpointPosition { get; set; }
        private bool _activated;
        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_activated) return;
            if (!other.CompareTag("Player")) return;

            _activated = true;
            CheckpointManager.Instance?.ActivateCheckpoint(CheckpointPosition);

            // Visual feedback: change color to green
            if (_sr != null)
                _sr.color = new Color(0.7f, 1.0f, 0.7f);
        }
    }
}
