using UnityEngine;
using UnityEngine.Tilemaps;
using EpochBreaker.Generative;
using UnityTilemapRenderer = UnityEngine.Tilemaps.TilemapRenderer;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Converts LevelData byte arrays into Unity Tilemaps.
    /// Creates separate tilemaps for ground (solid) and destructible layers.
    /// Handles Y-axis inversion: LevelData y=0 is top, Unity y increases upward.
    /// </summary>
    public class LevelRenderer : MonoBehaviour
    {
        public Tilemap GroundTilemap { get; private set; }
        public Tilemap DestructibleTilemap { get; private set; }
        public Tilemap PlatformTilemap { get; private set; }

        private Grid _grid;
        private LevelData _levelData;
        private HazardSystem _hazardSystem;
        private bool _geometryDirty;

        // Pre-built Unity Tile assets (one per tile type), cached per epoch
        private Tile[] _tileAssets;
        private static int s_cachedTileEpoch = -1;
        private static Tile[] s_cachedTiles;

        public int LevelWidth => _levelData?.Layout.WidthTiles ?? 0;
        public int LevelHeight => _levelData?.Layout.HeightTiles ?? 0;

        public void SetHazardSystem(HazardSystem hs) => _hazardSystem = hs;

        private void LateUpdate()
        {
            if (_geometryDirty && DestructibleTilemap != null)
            {
                var composite = DestructibleTilemap.GetComponent<CompositeCollider2D>();
                if (composite != null)
                    composite.GenerateGeometry();
                _geometryDirty = false;
            }
        }

        public void RenderLevel(LevelData data)
        {
            _levelData = data;
            BuildTileAssets();
            CreateGrid();
            PopulateTilemaps();
            TintRelicTiles();
        }

        public void Clear()
        {
            if (_grid != null)
                Destroy(_grid.gameObject);
            _levelData = null;
        }

        /// <summary>
        /// Convert level-data coords to Unity world position.
        /// LevelData: y=0 top, y increases down.
        /// Unity: y increases up. Cell (0,0) is bottom-left.
        /// </summary>
        public Vector3 LevelToWorld(int tileX, int tileY)
        {
            int unityY = (_levelData.Layout.HeightTiles - 1) - tileY;
            // Tilemap cell center: add 0.5 offset
            return new Vector3(tileX + 0.5f, unityY + 0.5f, 0f);
        }

        /// <summary>
        /// Convert level-data coords to tilemap cell position.
        /// </summary>
        public Vector3Int LevelToCell(int tileX, int tileY)
        {
            int unityY = (_levelData.Layout.HeightTiles - 1) - tileY;
            return new Vector3Int(tileX, unityY, 0);
        }

        /// <summary>
        /// Remove a destructible tile at level-data coordinates.
        /// Triggers hazard effects if the tile has a hazard assigned.
        /// </summary>
        public void DestroyTile(int tileX, int tileY)
        {
            if (_levelData == null) return;
            int width = _levelData.Layout.WidthTiles;
            int idx = tileY * width + tileX;

            if (idx < 0 || idx >= _levelData.Layout.Tiles.Length) return;

            // Check for hazard, relic, hidden content, and material before clearing data
            var hazard = _levelData.Layout.Destructibles[idx].Hazard;
            bool isRelic = _levelData.Layout.Destructibles[idx].IsRelic;
            var hiddenContent = _levelData.Layout.Destructibles[idx].HiddenContent;
            byte materialClass = _levelData.Layout.Destructibles[idx].MaterialClass;

            // Spawn destruction particles before clearing data (scaled by material)
            SpawnTileParticles(tileX, tileY, _levelData.Layout.Tiles[idx], materialClass);

            // Pitched crunch SFX — heavier materials get lower pitch
            float pitch = 1.1f - materialClass * 0.1f;
            AudioManager.PlaySFXPitched(PlaceholderAudio.GetTileCrunchSFX(), pitch - 0.1f, pitch + 0.1f);

            // Clear tile data
            _levelData.Layout.Tiles[idx] = (byte)TileType.Empty;
            _levelData.Layout.Collision[idx] = (byte)CollisionType.None;
            _levelData.Layout.Destructibles[idx] = default;

            // Remove from tilemap
            var cell = LevelToCell(tileX, tileY);
            DestructibleTilemap.SetTile(cell, null);

            // Mark geometry dirty — batched in LateUpdate
            _geometryDirty = true;

            // Notify achievement system
            GameManager.Instance?.RecordBlockDestroyed();

            // Record relic destruction (lost preservation score)
            if (isRelic)
                GameManager.Instance?.RecordRelicDestroyed();

            // Record hidden content discovery (exploration score)
            if (hiddenContent != Generative.HiddenContentType.None)
                GameManager.Instance?.RecordHiddenContentFound(hiddenContent);

            // Trigger hazard effect
            if (hazard != HazardType.None)
            {
                if (_hazardSystem == null)
                    _hazardSystem = FindAnyObjectByType<HazardSystem>();
                _hazardSystem?.OnTileDestroyed(tileX, tileY, hazard);
            }
        }

        /// <summary>
        /// Remove a tile without triggering hazards. Used by HazardSystem
        /// for debris cascade to prevent infinite loops.
        /// </summary>
        public void DestroyTileRaw(int tileX, int tileY)
        {
            if (_levelData == null) return;
            int width = _levelData.Layout.WidthTiles;
            int idx = tileY * width + tileX;

            if (idx < 0 || idx >= _levelData.Layout.Tiles.Length) return;

            _levelData.Layout.Tiles[idx] = (byte)TileType.Empty;
            _levelData.Layout.Collision[idx] = (byte)CollisionType.None;
            _levelData.Layout.Destructibles[idx] = default;

            var cell = LevelToCell(tileX, tileY);
            DestructibleTilemap.SetTile(cell, null);

            // Mark geometry dirty — batched in LateUpdate
            _geometryDirty = true;

            GameManager.Instance?.RecordBlockDestroyed();
        }

        /// <summary>
        /// Get the collision type at a world position.
        /// </summary>
        public CollisionType GetCollisionAt(Vector3 worldPos)
        {
            if (_levelData == null) return CollisionType.None;
            int tileX = Mathf.FloorToInt(worldPos.x);
            int tileY = (_levelData.Layout.HeightTiles - 1) - Mathf.FloorToInt(worldPos.y);

            if (tileX < 0 || tileX >= _levelData.Layout.WidthTiles) return CollisionType.None;
            if (tileY < 0 || tileY >= _levelData.Layout.HeightTiles) return CollisionType.None;

            int idx = tileY * _levelData.Layout.WidthTiles + tileX;
            return (CollisionType)_levelData.Layout.Collision[idx];
        }

        /// <summary>
        /// Get destructible tile data at level-data coordinates.
        /// </summary>
        public DestructibleTile GetDestructibleAt(int tileX, int tileY)
        {
            if (_levelData == null) return default;
            int idx = tileY * _levelData.Layout.WidthTiles + tileX;
            if (idx < 0 || idx >= _levelData.Layout.Destructibles.Length) return default;
            return _levelData.Layout.Destructibles[idx];
        }

        /// <summary>
        /// Damage an indestructible tile. After enough accumulated damage, it breaks.
        /// - Projectiles deal 2 damage (25 hits = ~10 seconds of sustained fire)
        /// - Head bops/stomps deal 5 damage (10 hits to break)
        /// Returns true if the tile was destroyed.
        /// </summary>
        public bool DamageIndestructibleTile(int tileX, int tileY, int damage = 2)
        {
            if (_levelData == null) return false;
            int width = _levelData.Layout.WidthTiles;
            int idx = tileY * width + tileX;

            if (idx < 0 || idx >= _levelData.Layout.Tiles.Length) return false;

            // Only affect indestructible tiles
            byte tileType = _levelData.Layout.Tiles[idx];
            if (tileType != (byte)TileType.Indestructible) return false;

            // Total HP of 50 means:
            // - 25 projectile hits (damage=2) to break
            // - 10 head bop/stomp hits (damage=5) to break
            const byte INDESTRUCTIBLE_HP = 50;

            ref var destructible = ref _levelData.Layout.Destructibles[idx];

            // Initialize HP if not set
            if (destructible.MaxHitPoints == 0)
            {
                destructible.MaxHitPoints = INDESTRUCTIBLE_HP;
                destructible.HitPoints = INDESTRUCTIBLE_HP;
            }

            // Apply damage
            if (destructible.HitPoints > damage)
                destructible.HitPoints -= (byte)damage;
            else
                destructible.HitPoints = 0;

            // If HP depleted, destroy the tile
            if (destructible.HitPoints <= 0)
            {
                DestroyTile(tileX, tileY);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Convert world position to level-data tile coordinates.
        /// </summary>
        public Vector2Int WorldToLevel(Vector3 worldPos)
        {
            int tileX = Mathf.FloorToInt(worldPos.x);
            int tileY = (_levelData.Layout.HeightTiles - 1) - Mathf.FloorToInt(worldPos.y);
            return new Vector2Int(tileX, tileY);
        }

        private void SpawnTileParticles(int tileX, int tileY, byte tileType, byte materialClass = 0)
        {
            Vector3 worldPos = LevelToWorld(tileX, tileY);
            int epoch = _levelData?.ID.Epoch ?? 0;
            Color tileColor = PlaceholderAssets.GetTileParticleColor(tileType, epoch);

            // Scale particle count and effects by material class
            // Soft=2, Medium=4, Hard=6, Reinforced=8, Indestructible=10
            int count;
            float maxScale;
            float trauma;
            switch ((MaterialClass)materialClass)
            {
                case MaterialClass.Soft:
                    count = Random.Range(2, 3); maxScale = 0.25f; trauma = 0f; break;
                case MaterialClass.Medium:
                    count = Random.Range(3, 5); maxScale = 0.3f; trauma = 0.05f; break;
                case MaterialClass.Hard:
                    count = Random.Range(5, 7); maxScale = 0.35f; trauma = 0.1f; break;
                case MaterialClass.Reinforced:
                    count = Random.Range(7, 9); maxScale = 0.4f; trauma = 0.2f; break;
                case MaterialClass.Indestructible:
                    count = Random.Range(8, 11); maxScale = 0.45f; trauma = 0.3f; break;
                default:
                    count = Random.Range(3, 5); maxScale = 0.3f; trauma = 0.05f; break;
            }

            if (trauma > 0f)
                CameraController.Instance?.AddTrauma(trauma);

            for (int i = 0; i < count; i++)
            {
                var go = ObjectPool.GetParticle();
                go.transform.position = worldPos + new Vector3(
                    Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0f);

                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = PlaceholderAssets.GetParticleSprite();
                sr.color = tileColor;
                sr.sortingOrder = 15;
                go.transform.localScale = Vector3.one * Random.Range(0.15f, maxScale);

                var rb = go.GetComponent<Rigidbody2D>();
                rb.gravityScale = 4f;
                float angle = Random.Range(30f, 150f) * Mathf.Deg2Rad; // Bias upward
                rb.linearVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(2f, 5f);

                go.GetComponent<PoolTimer>().StartTimer(0.6f);
            }

            // Indestructible: extra flash burst
            if ((MaterialClass)materialClass == MaterialClass.Indestructible)
            {
                var flashGO = ObjectPool.GetFlash();
                flashGO.transform.position = worldPos;
                var flashSR = flashGO.GetComponent<SpriteRenderer>();
                flashSR.sprite = PlaceholderAssets.GetParticleSprite();
                flashSR.color = new Color(1f, 1f, 0.9f, 0.8f);
                flashSR.sortingOrder = 16;
                flashGO.transform.localScale = Vector3.one * 1.2f;
                flashGO.GetComponent<PoolTimer>().StartTimer(0.08f);
            }
        }

        private void BuildTileAssets()
        {
            // Reuse cached tiles if same epoch (tiles are deterministic per epoch)
            int epoch = _levelData.ID.Epoch;
            if (s_cachedTileEpoch == epoch && s_cachedTiles != null)
            {
                _tileAssets = s_cachedTiles;
                return;
            }

            // Create one Unity Tile asset per TileType, using epoch-specific visuals
            _tileAssets = new Tile[13]; // TileType enum has values 0-12
            for (int i = 0; i <= 12; i++)
            {
                if (i == (int)TileType.Empty) continue;
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = PlaceholderAssets.GetTileSprite((byte)i, epoch);
                tile.color = Color.white;
                tile.colliderType = Tile.ColliderType.Grid; // Full cell collider, merged into composite
                _tileAssets[i] = tile;
            }
            s_cachedTiles = _tileAssets;
            s_cachedTileEpoch = epoch;
        }

        /// <summary>
        /// Clear cached tile assets. Called on epoch change or session cleanup.
        /// </summary>
        public static void ClearTileCache()
        {
            s_cachedTiles = null;
            s_cachedTileEpoch = -1;
        }

        private void CreateGrid()
        {
            if (_grid != null) Destroy(_grid.gameObject);

            var gridGO = new GameObject("LevelGrid");
            gridGO.transform.SetParent(transform);
            _grid = gridGO.AddComponent<Grid>();
            _grid.cellSize = Vector3.one;

            GroundTilemap = CreateTilemapLayer("Ground", 0, true);
            DestructibleTilemap = CreateTilemapLayer("Destructible", 1, true);
            PlatformTilemap = CreateTilemapLayer("Platforms", 2, false);

            // Set platform layer to use platform effector
            var platformCollider = PlatformTilemap.gameObject.AddComponent<TilemapCollider2D>();
            platformCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            var platformRB = PlatformTilemap.gameObject.AddComponent<Rigidbody2D>();
            platformRB.bodyType = RigidbodyType2D.Static;
            var platformComposite = PlatformTilemap.gameObject.AddComponent<CompositeCollider2D>();
            platformComposite.geometryType = CompositeCollider2D.GeometryType.Polygons;

            // Add platform effector for one-way platforms
            var effector = PlatformTilemap.gameObject.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = 170f;
            platformComposite.usedByEffector = true;
        }

        private Tilemap CreateTilemapLayer(string name, int sortingOrder, bool addCompositeCollider)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_grid.transform);
            go.layer = LayerMask.NameToLayer("Default");

            var tilemap = go.AddComponent<Tilemap>();
            var renderer = go.AddComponent<UnityTilemapRenderer>();
            renderer.sortingOrder = sortingOrder;

            if (addCompositeCollider)
            {
                var collider = go.AddComponent<TilemapCollider2D>();
                collider.compositeOperation = Collider2D.CompositeOperation.Merge;
                var rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;
                var composite = go.AddComponent<CompositeCollider2D>();
                composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            }

            return tilemap;
        }

        private void PopulateTilemaps()
        {
            int width = _levelData.Layout.WidthTiles;
            int height = _levelData.Layout.HeightTiles;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int idx = y * width + x;
                    byte tileType = _levelData.Layout.Tiles[idx];

                    if (tileType == (byte)TileType.Empty) continue;
                    if (tileType >= _tileAssets.Length || _tileAssets[tileType] == null) continue;

                    var cell = LevelToCell(x, y);
                    var tile = _tileAssets[tileType];

                    if (tileType == (byte)TileType.Platform)
                    {
                        PlatformTilemap.SetTile(cell, tile);
                    }
                    else if (tileType >= (byte)TileType.DestructibleSoft &&
                             tileType <= (byte)TileType.Indestructible)
                    {
                        DestructibleTilemap.SetTile(cell, tile);
                    }
                    else
                    {
                        GroundTilemap.SetTile(cell, tile);
                    }
                }
            }
        }

        /// <summary>
        /// Apply a gold/amber tint to relic tiles so players can identify them visually.
        /// Uses Tilemap.SetColor — a per-cell color multiplier on the existing tile.
        /// Auto-clears when the tile is destroyed (SetTile null).
        /// </summary>
        private void TintRelicTiles()
        {
            if (DestructibleTilemap == null || _levelData == null) return;

            int width = _levelData.Layout.WidthTiles;
            int height = _levelData.Layout.HeightTiles;
            var relicTint = new Color(1f, 0.85f, 0.4f, 1f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = y * width + x;
                    if (!_levelData.Layout.Destructibles[idx].IsRelic) continue;

                    var cell = LevelToCell(x, y);
                    DestructibleTilemap.SetColor(cell, relicTint);
                }
            }
        }
    }
}
