using UnityEngine;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Collectable weapon pickup. Grants the player a weapon type at a given tier.
    /// Bobs and pulses to attract attention.
    /// </summary>
    public class WeaponPickup : MonoBehaviour
    {
        public WeaponTier Tier { get; set; }
        public WeaponType Type { get; set; } = WeaponType.Bolt;

        private Vector3 _basePosition;

        private void Start()
        {
            _basePosition = transform.position;
        }

        private void Update()
        {
            // Bob animation
            float offset = Mathf.Sin(Time.time * 3f) * 0.1f;
            transform.position = _basePosition + Vector3.up * offset;

            // Pulse scale
            float scale = 1f + Mathf.Sin(Time.time * 4f) * 0.1f;
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var weapon = other.GetComponent<WeaponSystem>();
            if (weapon != null)
            {
                weapon.AcquireWeapon(Type, Tier);
            }

            int bonus = 0;
            if (GameManager.Instance != null)
                bonus = GameManager.Instance.CollectWeapon(Type, Tier);

            if (bonus > 0)
                GameManager.ShowScorePopup(transform.position, bonus);

            AudioManager.PlaySFX(PlaceholderAudio.GetWeaponPickupSFX());
            Destroy(gameObject);
        }
    }
}
