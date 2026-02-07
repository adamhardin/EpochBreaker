using UnityEngine;
using System.Collections;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Invisible trigger that activates the boss when the player enters the arena.
    /// Handles intro cinematic sequence and arena lockdown.
    /// </summary>
    public class BossArenaTrigger : MonoBehaviour
    {
        private bool _triggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_triggered) return;
            if (!other.CompareTag("Player")) return;

            // Find and activate the boss — only consume trigger if boss exists
            var levelLoader = FindAnyObjectByType<LevelLoader>();
            if (levelLoader != null && levelLoader.CurrentBoss != null)
            {
                _triggered = true;

                // Disable trigger (don't destroy — needed for respawn reactivation)
                var col = GetComponent<Collider2D>();
                if (col != null) col.enabled = false;

                StartCoroutine(BossIntroSequence(levelLoader));
            }
        }

        /// <summary>
        /// Boss intro cinematic: camera pans to boss, name card, roar, camera returns, fight begins.
        /// </summary>
        private IEnumerator BossIntroSequence(LevelLoader loader)
        {
            var boss = loader.CurrentBoss;
            var player = loader.Player;
            var cam = CameraController.Instance;

            // Grant player invulnerability during intro
            var health = player?.GetComponent<HealthSystem>();
            health?.GrantSpawnProtection();

            // Disable player input during intro
            if (player != null) player.IsAlive = false;

            // Lock camera to arena
            cam?.LockToArena(boss.ArenaMinX, boss.ArenaMaxX);

            // Spawn arena wall at entrance
            SpawnArenaWall(loader);

            // Pan camera to boss (1s)
            cam?.SetTemporaryTarget(boss.transform);
            yield return new WaitForSeconds(0.8f);

            // Boss roar SFX
            AudioManager.PlaySFX(PlaceholderAudio.GetBossRoarSFX());
            CameraController.Instance?.AddTrauma(0.3f);

            // Show boss name (via ScreenFlash for now — Sprint 4 will add proper name card)
            ScreenFlash.Flash(new Color(1f, 0.3f, 0.2f, 0.3f), 0.5f);

            yield return new WaitForSeconds(0.7f);

            // Pan back to player (0.5s)
            cam?.RestorePlayerTarget();
            yield return new WaitForSeconds(0.5f);

            // Re-enable player and activate boss
            if (player != null) player.IsAlive = true;
            boss.Activate();
        }

        /// <summary>
        /// Spawn a solid wall at the arena entrance to prevent retreat.
        /// </summary>
        private void SpawnArenaWall(LevelLoader loader)
        {
            var tilemap = loader.TilemapRenderer;
            if (tilemap == null) return;

            // Place wall at the trigger position (arena entrance)
            Vector2Int levelPos = tilemap.WorldToLevel(transform.position);
            int wallX = levelPos.x;

            // Build a column of solid wall tiles from ground to ceiling (~8 tiles high)
            for (int dy = -4; dy <= 4; dy++)
            {
                int tileY = levelPos.y + dy;
                if (tileY < 0 || tileY >= tilemap.LevelHeight) continue;

                var cell = tilemap.LevelToCell(wallX, tileY);
                var tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
                int epoch = GameManager.Instance != null ? GameManager.Instance.CurrentEpoch : 0;
                tile.sprite = PlaceholderAssets.GetTileSprite((byte)Generative.TileType.Ground, epoch);
                tile.color = new Color(0.4f, 0.35f, 0.3f);
                tile.colliderType = UnityEngine.Tilemaps.Tile.ColliderType.Grid;
                tilemap.GroundTilemap.SetTile(cell, tile);
            }

            // Force geometry rebuild
            var composite = tilemap.GroundTilemap.GetComponent<CompositeCollider2D>();
            if (composite != null) composite.GenerateGeometry();
        }

        /// <summary>
        /// Remove arena wall tiles (called on boss death).
        /// </summary>
        public void RemoveArenaWall()
        {
            var tilemap = FindAnyObjectByType<LevelRenderer>();
            if (tilemap == null) return;

            Vector2Int levelPos = tilemap.WorldToLevel(transform.position);
            int wallX = levelPos.x;

            for (int dy = -4; dy <= 4; dy++)
            {
                int tileY = levelPos.y + dy;
                if (tileY < 0 || tileY >= tilemap.LevelHeight) continue;

                var cell = tilemap.LevelToCell(wallX, tileY);
                tilemap.GroundTilemap.SetTile(cell, null);
            }

            var composite = tilemap.GroundTilemap.GetComponent<CompositeCollider2D>();
            if (composite != null) composite.GenerateGeometry();

            // Unlock camera
            CameraController.Instance?.UnlockArena();
        }

        /// <summary>
        /// Re-enable the trigger so the boss can be reactivated after player respawn.
        /// </summary>
        public void ResetTrigger()
        {
            _triggered = false;
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            // Remove old arena wall and unlock camera
            RemoveArenaWall();
        }
    }
}
