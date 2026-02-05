using UnityEngine;
using EpochBreaker.Gameplay;

namespace EpochBreaker.Tests.PlayMode
{
    /// <summary>
    /// Shared utility methods for creating test GameObjects with
    /// required components. Used across all Play Mode test fixtures.
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Create a minimal player GameObject with required components.
        /// </summary>
        public static GameObject CreatePlayerObject()
        {
            var go = new GameObject("TestPlayer");
            go.tag = "Player";
            go.AddComponent<SpriteRenderer>();
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            go.AddComponent<BoxCollider2D>();
            go.AddComponent<PlayerController>();
            go.AddComponent<HealthSystem>();
            go.AddComponent<WeaponSystem>();
            return go;
        }

        /// <summary>
        /// Create a minimal enemy GameObject for testing.
        /// </summary>
        public static GameObject CreateEnemyObject()
        {
            var go = new GameObject("TestEnemy");
            go.tag = "Enemy";
            go.AddComponent<SpriteRenderer>();
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            go.AddComponent<BoxCollider2D>();
            go.AddComponent<EnemyBase>();
            return go;
        }

        /// <summary>
        /// Create a GameManager singleton for testing.
        /// Destroys any existing instance first.
        /// </summary>
        public static GameObject CreateGameManager()
        {
            if (GameManager.Instance != null)
                Object.DestroyImmediate(GameManager.Instance.gameObject);

            var go = new GameObject("TestGameManager");
            go.AddComponent<GameManager>();
            return go;
        }

        /// <summary>
        /// Create a CheckpointManager singleton for testing.
        /// Destroys any existing instance first.
        /// </summary>
        public static GameObject CreateCheckpointManager()
        {
            if (CheckpointManager.Instance != null)
                Object.DestroyImmediate(CheckpointManager.Instance.gameObject);

            var go = new GameObject("TestCheckpointManager");
            go.AddComponent<CheckpointManager>();
            return go;
        }

        /// <summary>
        /// Clean up all test singletons and reset input state.
        /// </summary>
        public static void CleanupAll()
        {
            if (GameManager.Instance != null)
                Object.DestroyImmediate(GameManager.Instance.gameObject);

            if (CheckpointManager.Instance != null)
                Object.DestroyImmediate(CheckpointManager.Instance.gameObject);

            InputManager.Clear();
        }
    }
}
