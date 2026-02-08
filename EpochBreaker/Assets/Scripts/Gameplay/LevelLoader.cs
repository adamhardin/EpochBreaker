using UnityEngine;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Generates a level from a LevelID and creates all gameplay objects:
    /// tilemap, player, enemies, weapon pickups, rewards, checkpoints, exit portal, boss.
    /// </summary>
    public class LevelLoader : MonoBehaviour
    {
        private LevelRenderer _tilemapRenderer;
        private GameObject _levelRoot;
        private GameObject _playerObj;
        private GameObject _mainCamera;
        private GameObject _bossObj;

        // Stored for pillar respawn on boss reset
        private ZoneData? _bossZone;
        private int _bossGroundY;

        public LevelRenderer TilemapRenderer => _tilemapRenderer;
        public PlayerController Player { get; private set; }
        public Boss CurrentBoss { get; private set; }

        /// <summary>
        /// Load a tutorial level by index (0, 1, 2).
        /// Uses TutorialLevelBuilder for deterministic handcrafted layouts.
        /// </summary>
        public void LoadTutorialLevel(int tutorialIndex)
        {
            CleanupLevel();

            var data = TutorialLevelBuilder.BuildTutorial(tutorialIndex);
            GameManager.Instance.CurrentLevel = data;

            BuildLevelFromData(data);
        }

        public void LoadLevel(LevelID id)
        {
            CleanupLevel();

            // Generate level data
            var generator = new LevelGenerator();
            var data = generator.Generate(id);
            GameManager.Instance.CurrentLevel = data;

            BuildLevelFromData(data);
        }

        private void BuildLevelFromData(LevelData data)
        {
            // Create level root
            _levelRoot = new GameObject("LevelRoot");

            // Render tilemap
            var tilemapObj = new GameObject("TilemapRenderer");
            tilemapObj.transform.SetParent(_levelRoot.transform);
            _tilemapRenderer = tilemapObj.AddComponent<LevelRenderer>();
            _tilemapRenderer.RenderLevel(data);

            // Hazard system
            var hazardGO = new GameObject("HazardSystem");
            hazardGO.transform.SetParent(_levelRoot.transform);
            var hazardSystem = hazardGO.AddComponent<HazardSystem>();
            hazardSystem.Initialize(data, _tilemapRenderer);
            _tilemapRenderer.SetHazardSystem(hazardSystem);

            // Spawn player
            SpawnPlayer(data);

            // Sync physics so collider geometry is ready before camera reads player position
            Physics2D.SyncTransforms();

            // Create camera
            CreateCamera();

            // Parallax background
            var parallaxGO = new GameObject("ParallaxBackground");
            parallaxGO.transform.SetParent(_levelRoot.transform);
            var parallax = parallaxGO.AddComponent<ParallaxBackground>();
            parallax.Initialize(_mainCamera.transform, data.ID.Epoch);

            // Spawn entities
            SpawnEnemies(data);
            SpawnWeaponPickups(data);
            SpawnAbilityPickups(data);
            SpawnRewards(data);
            SpawnExtraLives(data);
            SpawnCheckpoints(data);
            SpawnExitPortal(data);
            SpawnBoss(data);

            // Set initial checkpoint
            var checkpointMgr = CheckpointManager.Instance;
            if (checkpointMgr != null)
            {
                Vector3 startPos = _tilemapRenderer.LevelToWorld(data.Layout.StartX, data.Layout.StartY);
                checkpointMgr.SetInitialSpawn(startPos);
            }

            // Final physics sync so CompositeCollider2D geometry includes all spawned entities
            Physics2D.SyncTransforms();
        }

        public void CleanupLevel()
        {
            if (_levelRoot != null)
                Destroy(_levelRoot);
            if (_playerObj != null)
            {
                _playerObj.tag = "Untagged"; // Prevent stale FindWithTag references
                Destroy(_playerObj);
            }
            if (_mainCamera != null)
                Destroy(_mainCamera);
            if (_bossObj != null)
                Destroy(_bossObj);

            _tilemapRenderer = null;
            Player = null;
            CurrentBoss = null;
            EnemyBase.ClearRegistry(); // Clear stale enemy refs on level cleanup
            GameManager.Instance.CurrentLevel = null;
        }

        /// <summary>
        /// Destroy existing arena pillars and respawn fresh ones.
        /// Called on player respawn to restore Phase 3 shelter mechanic.
        /// </summary>
        public void RespawnArenaPillars()
        {
            if (!_bossZone.HasValue || _levelRoot == null) return;

            // Destroy existing pillar parent
            var existing = _levelRoot.transform.Find("ArenaPillars");
            if (existing != null)
                Destroy(existing.gameObject);

            SpawnArenaPillars(_bossZone.Value, _bossGroundY);
        }

        private void SpawnPlayer(LevelData data)
        {
            Vector3 startPos = _tilemapRenderer.LevelToWorld(data.Layout.StartX, data.Layout.StartY);

            _playerObj = new GameObject("Player");
            _playerObj.tag = "Player";
            _playerObj.layer = LayerMask.NameToLayer("Default");

            // Sprite (with cosmetic skin tint applied via renderer color,
            // so all animation frames inherit the tint automatically)
            var sr = _playerObj.AddComponent<SpriteRenderer>();
            var cm = CosmeticManager.Instance;
            PlayerSkin selectedSkin = cm != null ? cm.SelectedSkin : PlayerSkin.Default;
            sr.sprite = PlaceholderAssets.GetPlayerSprite();
            sr.color = CosmeticManager.GetSkinTint(selectedSkin);
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
            _playerObj.AddComponent<SpecialAttackSystem>();

            // Cosmetic trail effect
            TrailEffect selectedTrail = cm != null ? cm.SelectedTrail : TrailEffect.None;
            if (selectedTrail != TrailEffect.None)
            {
                var trail = _playerObj.AddComponent<PlayerTrailEffect>();
                trail.Initialize(selectedTrail);
            }

            // Sprint 8: Ghost replay recording
            var ghostSystem = _playerObj.AddComponent<GhostReplaySystem>();
            ghostSystem.StartRecording();

            // If replaying a ghost, start playback
            var gm = GameManager.Instance;
            if (gm != null && gm.IsGhostReplay)
            {
                string ghostCode = gm.CurrentLevelID.ToCode();
                ghostSystem.StartPlayback(ghostCode);
            }

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

            // AudioListener lives on GameManager (persistent) — not added here to avoid duplicates

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

            // Sprint 9: Apply difficulty enemy count multiplier
            float enemyMultiplier = DifficultyManager.Instance != null
                ? DifficultyManager.Instance.EnemyCountMultiplier : 1f;

            // Use deterministic skip pattern based on level seed for enemy count reduction/increase
            var difficultyRng = new Generative.XORShift64(data.ID.Seed + 99999UL);

            for (int i = 0; i < data.Enemies.Count; i++)
            {
                var enemyData = data.Enemies[i];

                // Skip enemies too close to the start — give player a safe zone
                int dx = Mathf.Abs(enemyData.TileX - data.Layout.StartX);
                if (dx < 8) continue;

                // Difficulty-based spawning: randomly skip enemies on Easy, duplicate on Hard
                if (enemyMultiplier < 1f)
                {
                    // Easy: skip enemies based on multiplier probability
                    float roll = (float)(difficultyRng.Next() % 1000) / 1000f;
                    if (roll >= enemyMultiplier) continue;
                }

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

                // Hard: add an extra enemy nearby (50% chance per enemy to spawn duplicate)
                if (enemyMultiplier > 1f)
                {
                    float roll = (float)(difficultyRng.Next() % 1000) / 1000f;
                    if (roll < (enemyMultiplier - 1f))
                    {
                        // Spawn duplicate enemy offset slightly
                        Vector3 dupePos = pos + new Vector3(1.5f, 0f, 0f);
                        var dupeGo = new GameObject($"Enemy_{i}_dupe_{enemyData.Type}");
                        dupeGo.transform.SetParent(enemyParent.transform);
                        dupeGo.transform.position = dupePos;
                        dupeGo.tag = "Enemy";

                        var dupeSr = dupeGo.AddComponent<SpriteRenderer>();
                        dupeSr.sprite = PlaceholderAssets.GetEnemySprite(enemyData.Type, enemyData.Behavior);
                        dupeSr.sortingOrder = 9;

                        var dupeRb = dupeGo.AddComponent<Rigidbody2D>();
                        dupeRb.gravityScale = (enemyData.Behavior == EnemyBehavior.Flying) ? 0f : 3f;
                        dupeRb.freezeRotation = true;
                        dupeRb.interpolation = RigidbodyInterpolation2D.Interpolate;

                        var dupeCol = dupeGo.AddComponent<BoxCollider2D>();
                        dupeCol.size = new Vector2(1.0f, 1.2f);
                        dupeCol.offset = new Vector2(0f, 0.6f);

                        var dupeEnemy = dupeGo.AddComponent<EnemyBase>();
                        dupeEnemy.Initialize(enemyData, _tilemapRenderer);
                    }
                }
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
                if (drop.Type == WeaponType.Slower) continue; // Slower is deprecated

                int clampedY = ClampAboveGround(data, drop.TileX, drop.TileY);
                Vector3 pos = _tilemapRenderer.LevelToWorld(drop.TileX, clampedY);

                var go = new GameObject($"Weapon_{drop.Type}_{drop.Tier}_{i}");
                go.transform.SetParent(weaponParent.transform);
                go.transform.position = pos;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetWeaponPickupSprite(drop.Type, drop.Tier);
                sr.sortingOrder = 8;

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(0.8f, 0.8f);

                var pickup = go.AddComponent<WeaponPickup>();
                pickup.Tier = drop.Tier;
                pickup.Type = drop.Type;
            }
        }

        private void SpawnAbilityPickups(LevelData data)
        {
            int epoch = data.ID.Epoch;
            // Ability pickups appear from epoch 3+, placed at ~40% through the level
            if (epoch < 3) return;

            var rng = new Generative.XORShift64(data.ID.Seed + 77777UL);

            // --- First pickup: movement ability (Double Jump / Air Dash) ---
            int placeX = data.Layout.WidthTiles * 2 / 5;
            int placeY = data.Layout.StartY;
            placeY = ClampAboveGround(data, placeX, placeY);

            // Alternate between movement abilities based on epoch
            AbilityType movementType = (epoch % 2 == 1) ? AbilityType.DoubleJump : AbilityType.AirDash;
            SpawnSingleAbilityPickup(data, movementType, placeX, placeY);

            // --- Second pickup: combat ability (epoch 5+) ---
            if (epoch >= 5)
            {
                int placeX2 = data.Layout.WidthTiles * 3 / 5;
                int placeY2 = data.Layout.StartY;
                placeY2 = ClampAboveGround(data, placeX2, placeY2);

                AbilityType combatType;
                if (epoch >= 7)
                    combatType = AbilityType.PhaseShift;
                else
                    combatType = AbilityType.GroundSlam;

                SpawnSingleAbilityPickup(data, combatType, placeX2, placeY2);
            }
        }

        private void SpawnSingleAbilityPickup(LevelData data, AbilityType type, int tileX, int tileY)
        {
            Vector3 pos = _tilemapRenderer.LevelToWorld(tileX, tileY);

            var go = new GameObject($"AbilityPickup_{type}");
            go.transform.SetParent(_levelRoot.transform);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderAssets.GetAbilitySprite(type);
            sr.sortingOrder = 9;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var pickup = go.AddComponent<AbilityPickup>();
            pickup.Type = type;
        }

        private void SpawnRewards(LevelData data)
        {
            if (data.Rewards == null || data.Rewards.Count == 0) return;

            var rewardParent = new GameObject("Rewards");
            rewardParent.transform.SetParent(_levelRoot.transform);

            // Sprint 9: Apply difficulty health pickup multiplier
            float healthMultiplier = DifficultyManager.Instance != null
                ? DifficultyManager.Instance.HealthPickupMultiplier : 1f;
            var rewardRng = new Generative.XORShift64(data.ID.Seed + 55555UL);

            for (int i = 0; i < data.Rewards.Count; i++)
            {
                var reward = data.Rewards[i];
                if (reward.Hidden) continue;

                // Apply health pickup multiplier (only affects health-type rewards)
                bool isHealth = reward.Type == RewardType.HealthSmall || reward.Type == RewardType.HealthLarge;
                if (isHealth && healthMultiplier < 1f)
                {
                    // Hard mode: skip health pickups based on multiplier (0 = skip all)
                    float roll = (float)(rewardRng.Next() % 1000) / 1000f;
                    if (roll >= healthMultiplier) continue;
                }

                int clampedY = ClampAboveGround(data, reward.TileX, reward.TileY);
                Vector3 pos = _tilemapRenderer.LevelToWorld(reward.TileX, clampedY);

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

                // Easy mode: spawn extra health pickups (multiplier = 1.5)
                if (isHealth && healthMultiplier > 1f)
                {
                    float roll = (float)(rewardRng.Next() % 1000) / 1000f;
                    if (roll < (healthMultiplier - 1f))
                    {
                        int bonusX = Mathf.Min(reward.TileX + 2, data.Layout.WidthTiles - 1);
                        int bonusY = ClampAboveGround(data, bonusX, reward.TileY);
                        Vector3 bonusPos = _tilemapRenderer.LevelToWorld(bonusX, bonusY);

                        var bonusGo = new GameObject($"Reward_{reward.Type}_{i}_bonus");
                        bonusGo.transform.SetParent(rewardParent.transform);
                        bonusGo.transform.position = bonusPos;

                        var bonusSr = bonusGo.AddComponent<SpriteRenderer>();
                        bonusSr.sprite = PlaceholderAssets.GetRewardSprite(reward.Type);
                        bonusSr.sortingOrder = 7;

                        var bonusCol = bonusGo.AddComponent<BoxCollider2D>();
                        bonusCol.isTrigger = true;
                        bonusCol.size = new Vector2(0.8f, 0.8f);

                        var bonusPickup = bonusGo.AddComponent<RewardPickup>();
                        bonusPickup.Type = reward.Type;
                        bonusPickup.Value = reward.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Spawn extra life pickups in Campaign mode only. Max 2 per level.
        /// Places them among the reward positions (reusing existing reward locations that are far enough apart).
        /// </summary>
        private void SpawnExtraLives(LevelData data)
        {
            // Only spawn in Campaign mode
            if (GameManager.Instance == null || GameManager.Instance.CurrentGameMode != GameMode.Campaign)
                return;

            if (data.Rewards == null || data.Rewards.Count < 3) return;

            var extraLifeParent = new GameObject("ExtraLives");
            extraLifeParent.transform.SetParent(_levelRoot.transform);

            // Use the level's seed to deterministically pick positions
            var rng = new XORShift64(data.ID.Seed + 12345UL);
            int maxLives = 2;
            int placed = 0;

            // Place extra lives at roughly 1/3 and 2/3 through the level
            int levelWidth = data.Layout.WidthTiles;
            int[] targetXPositions = new int[]
            {
                levelWidth / 3,
                (levelWidth * 2) / 3
            };

            for (int t = 0; t < targetXPositions.Length && placed < maxLives; t++)
            {
                int targetX = targetXPositions[t];

                // Find the nearest reward position to use as placement reference
                int bestIdx = -1;
                int bestDist = int.MaxValue;
                for (int i = 0; i < data.Rewards.Count; i++)
                {
                    if (data.Rewards[i].Hidden) continue;
                    int dist = Mathf.Abs(data.Rewards[i].TileX - targetX);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestIdx = i;
                    }
                }

                if (bestIdx < 0) continue;

                var reward = data.Rewards[bestIdx];
                int extraLifeX = Mathf.Min(reward.TileX + 2, data.Layout.WidthTiles - 1);
                int clampedY = ClampAboveGround(data, extraLifeX, reward.TileY);
                Vector3 pos = _tilemapRenderer.LevelToWorld(extraLifeX, clampedY);

                var go = new GameObject($"ExtraLife_{placed}");
                go.transform.SetParent(extraLifeParent.transform);
                go.transform.position = pos;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetExtraLifeSprite();
                sr.sortingOrder = 7;

                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(0.8f, 0.8f);

                go.AddComponent<ExtraLifePickup>();
                placed++;
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

            int w = 384, h = 128;
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
                    if (y >= h - 16)
                        px[idx] = stoneDark; // Bottom edge shadow
                    else if (y <= 15)
                        px[idx] = stoneLight; // Top highlight
                    else if (x <= 15)
                        px[idx] = stoneLight; // Left highlight
                    else if (x >= w - 16)
                        px[idx] = stoneDark; // Right shadow
                    else
                        px[idx] = stone;
                }
            }

            tex.SetPixels(px);
            tex.Apply();
            _checkpointPlatformSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 256f);
            _checkpointPlatformSprite.name = "checkpoint_platform";
            return _checkpointPlatformSprite;
        }

        /// <summary>
        /// Ensure a spawn tile Y is above any solid ground. Scans upward in level-data
        /// coordinates (lower Y = higher in world) until finding an empty tile.
        /// Returns the adjusted tileY.
        /// </summary>
        private int ClampAboveGround(LevelData data, int tileX, int tileY)
        {
            int width = data.Layout.WidthTiles;
            int y = tileY;
            while (y > 0)
            {
                int idx = y * width + tileX;
                if (idx >= 0 && idx < data.Layout.Collision.Length &&
                    data.Layout.Collision[idx] != (byte)CollisionType.Solid)
                    break;
                y--;
            }
            return y;
        }

        private void SpawnExitPortal(LevelData data)
        {
            Vector3 portalPos = _tilemapRenderer.LevelToWorld(data.Layout.ExitPortalX, data.Layout.ExitPortalY);

            var go = new GameObject("ExitPortal");
            go.transform.SetParent(_levelRoot.transform);
            go.transform.position = portalPos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderAssets.GetExitPortalSprite();
            sr.sortingOrder = 6;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2.5f, 3.5f);

            var portal = go.AddComponent<ExitPortal>();

            // Boss-gating: lock portal if level has a boss arena
            bool hasBoss = false;
            if (data.Layout.Zones != null)
            {
                for (int i = 0; i < data.Layout.Zones.Length; i++)
                {
                    if (data.Layout.Zones[i].Type == Generative.ZoneType.BossArena)
                    {
                        hasBoss = true;
                        break;
                    }
                }
            }

            if (hasBoss)
                portal.SetLocked();
            else
                portal.Activate();
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

            _bossZone = bossZone;
            var zone = bossZone.Value;
            int epoch = data.ID.Epoch;
            BossType bossType = (BossType)epoch;

            // Spawn boss in the center of the arena
            int arenaCenterX = (zone.StartX + zone.EndX) / 2;

            // Find ground level in arena by scanning from bottom up (level coords: higher Y = lower in world)
            // We want the topmost solid tile (the floor surface), so scan upward from the bottom
            int arenaGroundY = data.Layout.HeightTiles - 3; // Default to near bottom
            for (int y = data.Layout.HeightTiles - 1; y >= 0; y--)
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

            // Calculate arena bounds in world coordinates (convert from tile to world)
            Vector3 minPos = _tilemapRenderer.LevelToWorld(zone.StartX + 2, 0);
            Vector3 maxPos = _tilemapRenderer.LevelToWorld(zone.EndX - 2, 0);
            float arenaMinX = minPos.x;
            float arenaMaxX = maxPos.x;

            CurrentBoss.Initialize(bossType, _tilemapRenderer, arenaMinX, arenaMaxX);

            // Spawn arena pillars (used in Phase 3)
            _bossGroundY = arenaGroundY;
            SpawnArenaPillars(zone, arenaGroundY);

            // Boss activates when player enters the arena (handled by BossArenaTrigger)
            CreateBossArenaTrigger(zone, arenaGroundY);
        }

        /// <summary>
        /// Spawn 2-3 destructible stone pillars in the boss arena.
        /// Pillars are inert in Phases 1-2; boss shelters behind them in Phase 3.
        /// </summary>
        private void SpawnArenaPillars(ZoneData zone, int groundY)
        {
            int arenaWidth = zone.EndX - zone.StartX;
            int pillarCount = arenaWidth >= 40 ? 3 : 2;

            var pillarParent = new GameObject("ArenaPillars");
            pillarParent.transform.SetParent(_levelRoot.transform);

            for (int i = 0; i < pillarCount; i++)
            {
                // Space pillars evenly across the arena
                float fraction = (i + 1f) / (pillarCount + 1f);
                int pillarX = zone.StartX + (int)(arenaWidth * fraction);
                Vector3 pos = _tilemapRenderer.LevelToWorld(pillarX, groundY - 1);
                pos.y += 1.5f; // Raise above ground

                var go = new GameObject($"ArenaPillar_{i}");
                go.transform.SetParent(pillarParent.transform);
                go.transform.position = pos;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetArenaPillarSprite();
                sr.sortingOrder = 8;
                sr.color = new Color(0.6f, 0.55f, 0.45f);

                // Solid collider (blocks movement)
                var col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1f, 3f);
                col.offset = new Vector2(0f, 0f);

                // Trigger collider (detects projectiles)
                var triggerCol = go.AddComponent<BoxCollider2D>();
                triggerCol.size = new Vector2(1.2f, 3.2f);
                triggerCol.offset = new Vector2(0f, 0f);
                triggerCol.isTrigger = true;

                go.AddComponent<ArenaPillar>();
            }
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
