using UnityEngine;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Generates a level from a LevelID and creates all gameplay objects:
    /// tilemap, player, enemies, weapon pickups, rewards, checkpoints, goal, boss.
    /// </summary>
    public class LevelLoader : MonoBehaviour
    {
        private LevelRenderer _tilemapRenderer;
        private GameObject _levelRoot;
        private GameObject _playerObj;
        private GameObject _mainCamera;
        private GameObject _bossObj;

        public LevelRenderer TilemapRenderer => _tilemapRenderer;
        public PlayerController Player { get; private set; }
        public Boss CurrentBoss { get; private set; }

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
            SpawnBoss(data);

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
            if (_bossObj != null)
                Destroy(_bossObj);

            _tilemapRenderer = null;
            Player = null;
            CurrentBoss = null;
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

            // Capsule collider — rounded edges prevent catching on tile seams
            var col = _playerObj.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.8f, 1.2f);
            col.offset = new Vector2(0f, 0.6f);
            col.direction = CapsuleDirection2D.Vertical;

            // Frictionless material for smooth wall sliding
            var mat = new PhysicsMaterial2D("PlayerFrictionless");
            mat.friction = 0f;
            mat.bounciness = 0f;
            col.sharedMaterial = mat;

            // Components
            Player = _playerObj.AddComponent<PlayerController>();
            _playerObj.AddComponent<HealthSystem>();
            _playerObj.AddComponent<WeaponSystem>();

            // Checkpoint manager
            var checkpointGO = new GameObject("CheckpointManager");
            checkpointGO.transform.SetParent(_levelRoot.transform);
            checkpointGO.AddComponent<CheckpointManager>();

            _playerObj.transform.position = startPos;

            // Grant spawn protection so enemies near start don't instantly kill the player
            var health = _playerObj.GetComponent<HealthSystem>();
            if (health != null)
                health.GrantSpawnProtection();
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
            cam.orthographicSize = 7f;
            cam.backgroundColor = new Color(0.10f, 0.08f, 0.15f); // dark blue-purple sky
            cam.clearFlags = CameraClearFlags.SolidColor;

            _mainCamera.AddComponent<AudioListener>();

            var camController = _mainCamera.AddComponent<CameraController>();
            camController.Initialize(Player.transform, _tilemapRenderer);

            // Position at player with vertical offset (matching CameraController's offset of 2.8)
            Vector3 playerPos = Player.transform.position;
            _mainCamera.transform.position = new Vector3(playerPos.x, playerPos.y + 2.8f, -10f);
        }

        private void SpawnEnemies(LevelData data)
        {
            if (data.Enemies == null || data.Enemies.Count == 0) return;

            var enemyParent = new GameObject("Enemies");
            enemyParent.transform.SetParent(_levelRoot.transform);

            for (int i = 0; i < data.Enemies.Count; i++)
            {
                var enemyData = data.Enemies[i];

                // Skip enemies too close to the start — give player a safe zone
                int dx = Mathf.Abs(enemyData.TileX - data.Layout.StartX);
                if (dx < 8) continue;

                Vector3 pos = _tilemapRenderer.LevelToWorld(enemyData.TileX, enemyData.TileY);

                var go = new GameObject($"Enemy_{i}_{enemyData.Type}");
                go.transform.SetParent(enemyParent.transform);
                go.transform.position = pos;
                go.tag = "Enemy";

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetEnemySprite(enemyData.Type, enemyData.Behavior);
                sr.sortingOrder = 9;

                var rb = go.AddComponent<Rigidbody2D>();
                rb.gravityScale = (enemyData.Behavior == EnemyBehavior.Flying) ? 0f : 3f;
                rb.freezeRotation = true;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;

                var col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1.0f, 1.2f);
                col.offset = new Vector2(0f, 0.6f);

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
                // Raise checkpoint to sit on top of ground tile (sprite pivot is at bottom)
                pos.y += 1f;

                var go = new GameObject($"Checkpoint_{cp.Type}_{i}");
                go.transform.SetParent(checkpointParent.transform);
                go.transform.position = pos;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetCheckpointSprite(cp.Type);
                sr.sortingOrder = 6;

                // Trigger collider for checkpoint activation
                var triggerCol = go.AddComponent<BoxCollider2D>();
                triggerCol.isTrigger = true;
                triggerCol.size = new Vector2(0.8f, 2.5f);
                triggerCol.offset = new Vector2(0f, 1.25f); // Center on 3-unit tall sprite

                var cpScript = go.AddComponent<CheckpointTrigger>();
                // Respawn at ground level (where player stands), not at flag base
                cpScript.CheckpointPosition = new Vector3(pos.x, pos.y, pos.z);

                // Add standable platform at base of checkpoint
                var platformGO = new GameObject($"CheckpointPlatform_{i}");
                platformGO.transform.SetParent(go.transform);
                platformGO.transform.localPosition = new Vector3(0f, 0.25f, 0f);

                var platformCol = platformGO.AddComponent<BoxCollider2D>();
                platformCol.size = new Vector2(1.5f, 0.5f);
                platformCol.offset = Vector2.zero;

                // Add a small visual indicator for the platform (optional ledge sprite)
                var platformSR = platformGO.AddComponent<SpriteRenderer>();
                platformSR.sprite = CreatePlatformSprite();
                platformSR.sortingOrder = 5;
            }
        }

        /// <summary>
        /// Create a simple platform sprite for checkpoint bases.
        /// </summary>
        private static Sprite _checkpointPlatformSprite;
        private Sprite CreatePlatformSprite()
        {
            if (_checkpointPlatformSprite != null) return _checkpointPlatformSprite;

            int w = 96, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[w * h];

            Color stone = new Color(0.45f, 0.40f, 0.35f);
            Color stoneLight = new Color(0.55f, 0.50f, 0.45f);
            Color stoneDark = new Color(0.30f, 0.25f, 0.22f);

            // Fill with stone color
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    if (y >= h - 4)
                        px[idx] = stoneDark; // Bottom edge shadow
                    else if (y <= 3)
                        px[idx] = stoneLight; // Top highlight
                    else if (x <= 3)
                        px[idx] = stoneLight; // Left highlight
                    else if (x >= w - 4)
                        px[idx] = stoneDark; // Right shadow
                    else
                        px[idx] = stone;
                }
            }

            tex.SetPixels(px);
            tex.Apply();
            _checkpointPlatformSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 64f);
            _checkpointPlatformSprite.name = "checkpoint_platform";
            return _checkpointPlatformSprite;
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
            col.size = new Vector2(2.5f, 3.5f);

            go.AddComponent<GoalTrigger>();
        }

        private void SpawnBoss(LevelData data)
        {
            // Find the BossArena zone to determine boss spawn location
            ZoneData? bossZone = null;
            if (data.Layout.Zones != null)
            {
                for (int i = 0; i < data.Layout.Zones.Length; i++)
                {
                    if (data.Layout.Zones[i].Type == ZoneType.BossArena)
                    {
                        bossZone = data.Layout.Zones[i];
                        break;
                    }
                }
            }

            if (!bossZone.HasValue) return;

            var zone = bossZone.Value;
            int epoch = data.ID.Epoch;
            BossType bossType = (BossType)epoch;

            // Spawn boss in the center of the arena
            int arenaCenterX = (zone.StartX + zone.EndX) / 2;

            // Find ground level in arena by scanning from top down (level coords y=0 is top)
            // In level coordinates, higher Y = lower in world
            int arenaGroundY = data.Layout.HeightTiles - 3; // Default to near bottom
            for (int y = 0; y < data.Layout.HeightTiles; y++)
            {
                int idx = y * data.Layout.WidthTiles + arenaCenterX;
                if (idx >= 0 && idx < data.Layout.Collision.Length &&
                    data.Layout.Collision[idx] == (byte)CollisionType.Solid)
                {
                    arenaGroundY = y;
                    break;
                }
            }

            // Convert to world position (boss spawns 2 tiles above ground for its height)
            // Boss sprite is 3 units tall with pivot at bottom, so spawn at ground level
            Vector3 bossPos = _tilemapRenderer.LevelToWorld(arenaCenterX, arenaGroundY - 1);
            bossPos.y += 1.5f; // Raise slightly to ensure boss stands on ground

            _bossObj = new GameObject($"Boss_{bossType}");
            _bossObj.transform.SetParent(_levelRoot.transform);
            _bossObj.transform.position = bossPos;
            _bossObj.tag = "Enemy"; // Uses Enemy tag for collision detection

            // Sprite - bosses are 3x3 units
            var sr = _bossObj.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderAssets.GetBossSprite(bossType);
            sr.sortingOrder = 10;

            // Physics
            var rb = _bossObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.mass = 5f; // Heavier than regular enemies

            // Larger collider for boss (3 units wide, 2.5 units tall)
            var col = _bossObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2.5f, 2.5f);
            col.offset = new Vector2(0f, 1.25f);

            // Boss component
            CurrentBoss = _bossObj.AddComponent<Boss>();

            // Calculate arena bounds in world coordinates
            float arenaMinX = zone.StartX + 2f;
            float arenaMaxX = zone.EndX - 2f;

            CurrentBoss.Initialize(bossType, _tilemapRenderer, arenaMinX, arenaMaxX);

            // Boss activates when player enters the arena (handled by BossArenaTrigger)
            CreateBossArenaTrigger(zone, arenaGroundY);
        }

        /// <summary>
        /// Create an invisible trigger zone that activates the boss when player enters.
        /// </summary>
        private void CreateBossArenaTrigger(ZoneData zone, int groundY)
        {
            var triggerGO = new GameObject("BossArenaTrigger");
            triggerGO.transform.SetParent(_levelRoot.transform);

            // Position at arena entrance
            Vector3 triggerPos = _tilemapRenderer.LevelToWorld(zone.StartX + 5, groundY - 3);
            triggerGO.transform.position = triggerPos;

            var col = triggerGO.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(4f, 8f); // Wide enough to catch player entering

            triggerGO.AddComponent<BossArenaTrigger>();
        }
    }
}
