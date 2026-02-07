using UnityEngine;

namespace EpochBreaker.Gameplay
{
    public enum AbilityType
    {
        DoubleJump,
        AirDash,
    }

    /// <summary>
    /// Collectable ability pickup. Grants the player a movement ability.
    /// </summary>
    public class AbilityPickup : MonoBehaviour
    {
        public AbilityType Type { get; set; }

        private Vector3 _basePosition;

        private void Start()
        {
            _basePosition = transform.position;
        }

        private void Update()
        {
            float offset = Mathf.Sin(Time.time * 2.5f) * 0.15f;
            transform.position = _basePosition + Vector3.up * offset;

            float scale = 1f + Mathf.Sin(Time.time * 3f) * 0.08f;
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var abilities = other.GetComponent<AbilitySystem>();
            if (abilities == null)
            {
                abilities = other.gameObject.AddComponent<AbilitySystem>();
            }

            switch (Type)
            {
                case AbilityType.DoubleJump:
                    abilities.GrantDoubleJump();
                    break;
                case AbilityType.AirDash:
                    abilities.GrantAirDash();
                    break;
            }

            AudioManager.PlaySFX(PlaceholderAudio.GetWeaponPickupSFX());
            Destroy(gameObject);
        }
    }
}
