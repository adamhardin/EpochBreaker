using System.Collections.Generic;
using UnityEngine;
using SixteenBit.Generative;

namespace SixteenBit.Gameplay
{
    /// <summary>
    /// Generates placeholder sprites at runtime so the game can run
    /// without any imported art assets. All sprites are simple colored
    /// squares/rectangles created via Texture2D.
    /// </summary>
    public static class PlaceholderAssets
    {
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();
        private const int TILE_PX = 8;
        private const float PPU = 8f;

        public static Sprite GetTileSprite(byte tileType)
        {
            string key = $"tile_{tileType}";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            Color color = tileType switch
            {
                (byte)TileType.Ground      => new Color(0.40f, 0.26f, 0.13f), // brown
                (byte)TileType.GroundTop   => new Color(0.30f, 0.60f, 0.20f), // green
                (byte)TileType.GroundLeft  => new Color(0.35f, 0.22f, 0.10f), // dark brown
                (byte)TileType.GroundRight => new Color(0.35f, 0.22f, 0.10f),
                (byte)TileType.Platform    => new Color(0.55f, 0.55f, 0.55f), // grey
                (byte)TileType.Hazard      => new Color(0.90f, 0.10f, 0.10f), // red
                (byte)TileType.Decorative  => new Color(0.50f, 0.70f, 0.50f), // light green
                (byte)TileType.DestructibleSoft       => new Color(0.85f, 0.75f, 0.55f), // tan
                (byte)TileType.DestructibleMedium     => new Color(0.70f, 0.55f, 0.35f), // clay
                (byte)TileType.DestructibleHard       => new Color(0.50f, 0.50f, 0.50f), // stone grey
                (byte)TileType.DestructibleReinforced => new Color(0.35f, 0.35f, 0.40f), // dark steel
                (byte)TileType.Indestructible         => new Color(0.20f, 0.20f, 0.25f), // near black
                _ => Color.clear,
            };

            var sprite = CreateSquareSprite(color, TILE_PX, PPU);
            _cache[key] = sprite;
            return sprite;
        }

        public static Sprite GetPlayerSprite()
        {
            string key = "player";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            // 32x32 blue character placeholder
            int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Body (blue)
                    if (x >= 8 && x < 24 && y >= 0 && y < 24)
                        pixels[y * size + x] = new Color(0.20f, 0.40f, 0.90f);
                    // Head (lighter blue)
                    else if (x >= 10 && x < 22 && y >= 24 && y < 32)
                        pixels[y * size + x] = new Color(0.40f, 0.60f, 1.00f);
                    // Eyes
                    else if ((x == 13 || x == 18) && y == 28)
                        pixels[y * size + x] = Color.white;
                    else
                        pixels[y * size + x] = Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0f), size);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        public static Sprite GetEnemySprite(bool isCharger)
        {
            string key = isCharger ? "enemy_charger" : "enemy_shooter";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[size * size];
            Color bodyColor = isCharger
                ? new Color(0.85f, 0.20f, 0.20f)  // red charger
                : new Color(0.80f, 0.50f, 0.10f);  // orange shooter

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (x >= 6 && x < 26 && y >= 0 && y < 28)
                        pixels[y * size + x] = bodyColor;
                    // Eyes
                    else if ((x == 11 || x == 20) && y == 24)
                        pixels[y * size + x] = Color.white;
                    else
                        pixels[y * size + x] = Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0f), size);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        public static Sprite GetProjectileSprite()
        {
            string key = "projectile";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            var sprite = CreateSquareSprite(new Color(1f, 1f, 0.2f), 4, PPU); // yellow
            _cache[key] = sprite;
            return sprite;
        }

        public static Sprite GetWeaponPickupSprite(WeaponTier tier)
        {
            string key = $"weapon_{tier}";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            Color color = tier switch
            {
                WeaponTier.Starting => new Color(0.60f, 0.80f, 0.60f),
                WeaponTier.Medium   => new Color(0.40f, 0.60f, 1.00f),
                WeaponTier.Heavy    => new Color(0.80f, 0.40f, 0.90f),
                _ => Color.white,
            };

            int size = 12;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    pixels[y * size + x] = (x >= 2 && x < 10 && y >= 2 && y < 10) ? color : Color.clear;
            tex.SetPixels(pixels);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), PPU);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        public static Sprite GetRewardSprite(RewardType type)
        {
            string key = $"reward_{type}";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            Color color = type switch
            {
                RewardType.HealthSmall  => new Color(0.20f, 0.90f, 0.20f),
                RewardType.HealthLarge  => new Color(0.10f, 1.00f, 0.10f),
                RewardType.AttackBoost  => new Color(1.00f, 0.50f, 0.10f),
                RewardType.SpeedBoost   => new Color(0.20f, 0.80f, 1.00f),
                RewardType.Shield       => new Color(0.70f, 0.70f, 1.00f),
                RewardType.Coin         => new Color(1.00f, 0.85f, 0.10f),
                _ => Color.white,
            };

            var sprite = CreateSquareSprite(color, 8, PPU);
            _cache[key] = sprite;
            return sprite;
        }

        public static Sprite GetCheckpointSprite()
        {
            string key = "checkpoint";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            // Flag shape: 8x16
            int w = 8, h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (x == 1) // pole
                        pixels[y * w + x] = new Color(0.60f, 0.40f, 0.20f);
                    else if (x >= 2 && x < 7 && y >= 10 && y < 16) // flag
                        pixels[y * w + x] = new Color(1.0f, 0.85f, 0.0f);
                    else
                        pixels[y * w + x] = Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), PPU);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        public static Sprite GetGoalSprite()
        {
            string key = "goal";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            var sprite = CreateSquareSprite(new Color(1f, 0.85f, 0f), 16, PPU); // gold
            _cache[key] = sprite;
            return sprite;
        }

        private static Sprite CreateSquareSprite(Color color, int size, float ppu)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), ppu);
            return sprite;
        }
    }
}
