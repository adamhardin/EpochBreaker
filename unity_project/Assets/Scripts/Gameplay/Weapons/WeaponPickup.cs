using UnityEngine;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Collectable weapon upgrade. Upgrades the player's weapon tier on contact.
    /// Bobs and pulses to attract attention.
    /// </summary>
    public class WeaponPickup : MonoBehaviour
    {
        public WeaponTier Tier { get; set; }

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
                weapon.UpgradeWeapon(Tier);
            }

            if (GameManager.Instance != null)
                GameManager.Instance.CollectWeapon(Tier);

            AudioManager.PlaySFX(PlaceholderAudio.GetWeaponPickupSFX());
            Destroy(gameObject);
        }
    }
}
