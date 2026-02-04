using UnityEngine;

namespace SixteenBit.Gameplay
{
    /// <summary>
    /// Tracks activated checkpoints and provides respawn position.
    /// </summary>
    public class CheckpointManager : MonoBehaviour
    {
        public static CheckpointManager Instance { get; private set; }
        public Vector3 CurrentRespawnPoint { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void SetInitialSpawn(Vector3 position)
        {
            CurrentRespawnPoint = position;
        }

        public void ActivateCheckpoint(Vector3 position)
        {
            CurrentRespawnPoint = position;
        }
    }
}
