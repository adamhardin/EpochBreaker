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

        // Pre-built Unity Tile assets (one per tile type)
        private Tile[] _tileAssets;

        public int LevelWidth => _levelData?.Layout.WidthTiles ?? 0;
        public int LevelHeight => _levelData?.Layout.HeightTiles ?? 0;

        public void RenderLevel(LevelData data)
        {
            _levelData = data;
            BuildTileAssets();
            CreateGrid();
            PopulateTilemaps();
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
        /// Called when a projectile or weapon destroys a tile.
        /// </summary>
        public void DestroyTile(int tileX, int tileY)
        {
            if (_levelData == null) return;
            int width = _levelData.Layout.WidthTiles;
            int idx = tileY * width + tileX;

            if (idx < 0 || idx >= _levelData.Layout.Tiles.Length) return;

            // Clear tile data
            _levelData.Layout.Tiles[idx] = (byte)TileType.Empty;
            _levelData.Layout.Collision[idx] = (byte)CollisionType.None;
            _levelData.Layout.Destructibles[idx] = default;

            // Remove from tilemap
            var cell = LevelToCell(tileX, tileY);
            DestructibleTilemap.SetTile(cell, null);

            // Refresh the composite collider after tile removal
            var composite = DestructibleTilemap.GetComponent<CompositeCollider2D>();
            if (composite != null)
                composite.GenerateGeometry();

            // Notify achievement system
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

        private void BuildTileAssets()
        {
            // Create one Unity Tile asset per TileType, using epoch-specific visuals
            int epoch = _levelData.ID.Epoch;
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
    }
}
