using UnityEngine;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Extra life pickup. Only spawns in Campaign mode.
    /// Adds one global life to the player's pool.
    /// </summary>
    public class ExtraLifePickup : MonoBehaviour
    {
        private Vector3 _basePosition;
        private const float BOB_SPEED = 2f;
        private const float BOB_AMOUNT = 0.15f;

        private void Start()
        {
            _basePosition = transform.position;
        }

        private void Update()
        {
            float offset = Mathf.Sin(Time.time * BOB_SPEED) * BOB_AMOUNT;
            transform.position = _basePosition + Vector3.up * offset;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            GameManager.Instance?.CollectExtraLife();
            Destroy(gameObject);
        }
    }
}
