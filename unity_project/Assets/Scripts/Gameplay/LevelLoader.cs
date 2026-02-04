using UnityEngine;
using SixteenBit.Generative;

namespace SixteenBit.Gameplay
{
    /// <summary>
    /// Generates a level from a LevelID and creates all gameplay objects:
    /// tilemap, player, enemies, weapon pickups, rewards, checkpoints, goal.
    /// </summary>
    public class LevelLoader : MonoBehaviour
    {
        private LevelRenderer _tilemapRenderer;
        private GameObject _levelRoot;
        private GameObject _playerObj;
        private GameObject _mainCamera;

        public LevelRenderer TilemapRenderer => _tilemapRenderer;
        public PlayerController Player { get; private set; }

        public void LoadLevel(LevelID id)
        {
            CleanupLevel();

            // Generate level data
            var generator = new LevelGenerator();
            var data = generator.Generate(id);
            GameManager.Instance.CurrentLevel = data;

            // Create level root
            _levelRoot = new GameObject("LevelRoot");

            // Render tilemap
            var tilemapObj = new GameObject("TilemapRenderer");
            tilemapObj.transform.SetParent(_levelRoot.transform);
            _tilemapRenderer = tilemapObj.AddComponent<LevelRenderer>();
            _tilemapRenderer.RenderLevel(data);

            // Spawn player
            SpawnPlayer(data);

            // Create camera
            CreateCamera();

            // Spawn entities
            SpawnEnemies(data);
            SpawnWeaponPickups(data);
            SpawnRewards(data);
            SpawnCheckpoints(data);
            SpawnGoal(data);

            // Set initial checkpoint
            var checkpointMgr = CheckpointManager.Instance;
            if (checkpointMgr != null)
            {
                Vector3 startPos = _tilemapRenderer.LevelToWorld(data.Layout.StartX, data.Layout.StartY);
                checkpointMgr.SetInitialSpawn(startPos);
            }
        }

        public void CleanupLevel()
        {
            if (_levelRoot != null)
                Destroy(_levelRoot);
            if (_playerObj != null)
                Destroy(_playerObj);
            if (_mainCamera != null)
                Destroy(_mainCamera);

            _tilemapRenderer = null;
            Player = null;
            GameManager.Instance.CurrentLevel = null;
        }

        private void SpawnPlayer(LevelData data)
        {
            Vector3 startPos = _tilemapRenderer.LevelToWorld(data.Layout.StartX, data.Layout.StartY);

            _playerObj = new GameObject("Player");
            _playerObj.tag = "Player";
            _playerObj.layer = LayerMask.NameToLayer("Default");

            // Sprite
            var sr = _playerObj.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderAssets.GetPlayerSprite();
            sr.sortingOrder = 10;

            // Physics
            var rb = _playerObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // Managed by PlayerController
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Collider (smaller than sprite for forgiving gameplay)
            var col = _playerObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.9f);
            col.offset = new Vector2(0f, 0.45f);

            // Components
            Player = _playerObj.AddComponent<PlayerController>();
            _playerObj.AddComponent<HealthSystem>();
            _playerObj.AddComponent<WeaponSystem>();

            // Checkpoint manager
            var checkpointGO = new GameObject("CheckpointManager");
            checkpointGO.transform.SetParent(_levelRoot.transform);
            checkpointGO.AddComponent<CheckpointManager>();

            _playerObj.transform.position = startPos;
        }

        private void CreateCamera()
        {
            // Destroy existing scene camera to avoid duplicates
            var existingCam = Camera.main;
            if (existingCam != null)
                Destroy(existingCam.gameObject);

            _mainCamera = new GameObject("MainCamera");
            _mainCamera.tag = "MainCamera";

            var cam = _mainCamera.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 9f;
            cam.backgroundColor = new Color(0.10f, 0.08f, 0.15f); // dark blue-purple sky
            cam.clearFlags = CameraClearFlags.SolidColor;

            var camController = _mainCamera.AddComponent<CameraController>();
            camController.Initialize(Player.transform, _tilemapRenderer);

            // Position at player
            Vector3 playerPos = Player.transform.position;
            _mainCamera.transform.position = new Vector3(playerPos.x, playerPos.y, -10f);
        }

        private void SpawnEnemies(LevelData data)
        {
            if (data.Enemies == null || data.Enemies.Count == 0) return;

            var enemyParent = new GameObject("Enemies");
            enemyParent.transform.SetParent(_levelRoot.transform);

            for (int i = 0; i < data.Enemies.Count; i++)
            {
                var enemyData = data.Enemies[i];
                Vector3 pos = _tilemapRenderer.LevelToWorld(enemyData.TileX, enemyData.TileY);

                bool isCharger = enemyData.Behavior == EnemyBehavior.Patrol ||
                                 enemyData.Behavior == EnemyBehavior.Chase;

                var go = new GameObject($"Enemy_{i}_{enemyData.Type}");
                go.transform.SetParent(enemyParent.transform);
                go.transform.position = pos;
                go.tag = "Enemy";

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetEnemySprite(isCharger);
                sr.sortingOrder = 9;

                var rb = go.AddComponent<Rigidbody2D>();
                rb.gravityScale = (enemyData.Behavior == EnemyBehavior.Flying) ? 0f : 3f;
                rb.freezeRotation = true;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;

                var col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.9f, 0.9f);
                col.offset = new Vector2(0f, 0.45f);

                var enemy = go.AddComponent<EnemyBase>();
                enemy.Initialize(enemyData, _tilemapRenderer);
            }
        }

        private void SpawnWeaponPickups(LevelData data)
        {
            if (data.WeaponDrops == null || data.WeaponDrops.Count == 0) return;

            var weaponParent = new GameObject("WeaponPickups");
            weaponParent.transform.SetParent(_levelRoot.transform);

            for (int i = 0; i < data.WeaponDrops.Count; i++)
            {
                var drop = data.WeaponDrops[i];
                if (drop.Hidden) continue; // Skip hidden for now

                Vector3 pos = _tilemapRenderer.LevelToWorld(drop.TileX, drop.TileY);

                var go = new GameObject($"Weapon_{drop.Tier}_{i}");
                go.transform.SetParent(weaponParent.transform);
                go.transform.position = pos;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetWeaponPickupSprite(drop.Tier);
                sr.sortingOrder = 8;

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(0.8f, 0.8f);

                var pickup = go.AddComponent<WeaponPickup>();
                pickup.Tier = drop.Tier;
            }
        }

        private void SpawnRewards(LevelData data)
        {
            if (data.Rewards == null || data.Rewards.Count == 0) return;

            var rewardParent = new GameObject("Rewards");
            rewardParent.transform.SetParent(_levelRoot.transform);

            for (int i = 0; i < data.Rewards.Count; i++)
            {
                var reward = data.Rewards[i];
                if (reward.Hidden) continue;

                Vector3 pos = _tilemapRenderer.LevelToWorld(reward.TileX, reward.TileY);

                var go = new GameObject($"Reward_{reward.Type}_{i}");
                go.transform.SetParent(rewardParent.transform);
                go.transform.position = pos;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetRewardSprite(reward.Type);
                sr.sortingOrder = 7;

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(0.8f, 0.8f);

                var rewardPickup = go.AddComponent<RewardPickup>();
                rewardPickup.Type = reward.Type;
                rewardPickup.Value = reward.Value;
            }
        }

        private void SpawnCheckpoints(LevelData data)
        {
            if (data.Checkpoints == null || data.Checkpoints.Count == 0) return;

            var checkpointParent = new GameObject("Checkpoints");
            checkpointParent.transform.SetParent(_levelRoot.transform);

            for (int i = 0; i < data.Checkpoints.Count; i++)
            {
                var cp = data.Checkpoints[i];
                Vector3 pos = _tilemapRenderer.LevelToWorld(cp.TileX, cp.TileY);

                var go = new GameObject($"Checkpoint_{cp.Type}_{i}");
                go.transform.SetParent(checkpointParent.transform);
                go.transform.position = pos;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetCheckpointSprite();
                sr.sortingOrder = 6;

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(0.8f, 1.5f);

                var cpScript = go.AddComponent<CheckpointTrigger>();
                cpScript.CheckpointPosition = pos;
            }
        }

        private void SpawnGoal(LevelData data)
        {
            Vector3 goalPos = _tilemapRenderer.LevelToWorld(data.Layout.GoalX, data.Layout.GoalY);

            var go = new GameObject("Goal");
            go.transform.SetParent(_levelRoot.transform);
            go.transform.position = goalPos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderAssets.GetGoalSprite();
            sr.sortingOrder = 6;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.5f, 2f);

            go.AddComponent<GoalTrigger>();
        }
    }
}
