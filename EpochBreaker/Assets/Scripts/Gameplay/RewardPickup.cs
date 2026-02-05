using UnityEngine;
using SixteenBit.Generative;

namespace SixteenBit.Gameplay
{
    /// <summary>
    /// Collectable reward/pickup. Heals, boosts, or gives coins.
    /// Bobs up and down to draw attention.
    /// </summary>
    public class RewardPickup : MonoBehaviour
    {
        public RewardType Type { get; set; }
        public int Value { get; set; }

        private Vector3 _basePosition;
        private const float BOB_SPEED = 2f;
        private const float BOB_AMOUNT = 0.15f;

        private void Start()
        {
            _basePosition = transform.position;
        }

        private void Update()
        {
            // Bob animation
            float offset = Mathf.Sin(Time.time * BOB_SPEED) * BOB_AMOUNT;
            transform.position = _basePosition + Vector3.up * offset;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var health = other.GetComponent<HealthSystem>();
            if (health != null)
            {
                switch (Type)
                {
                    case RewardType.HealthSmall:
                        health.Heal(1);
                        break;
                    case RewardType.HealthLarge:
                        health.Heal(2);
                        break;
                    case RewardType.AttackBoost:
                    case RewardType.SpeedBoost:
                    case RewardType.Shield:
                        // Placeholder: just heal for now
                        health.Heal(1);
                        break;
                    case RewardType.Coin:
                        if (GameManager.Instance != null)
                            GameManager.Instance.Score += Value;
                        break;
                }
            }

            if (GameManager.Instance != null)
                GameManager.Instance.CollectReward(Type);

            AudioManager.PlaySFX(PlaceholderAudio.GetRewardPickupSFX());
            Destroy(gameObject);
        }
    }
}
