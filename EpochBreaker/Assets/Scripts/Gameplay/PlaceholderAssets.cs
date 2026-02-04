using System.Collections.Generic;
using UnityEngine;
using SixteenBit.Generative;

namespace SixteenBit.Gameplay
{
    /// <summary>
    /// Generates all game sprites at runtime via Texture2D pixel painting.
    /// No imported art assets needed. Each sprite is cached by key.
    /// Sprites use 16-bit era visual style with recognizable silhouettes.
    /// </summary>
    public static class PlaceholderAssets
    {
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();
        private const int TILE_PX = 8;
        private const float PPU = 8f;

        #region Helpers

        private static void SetRect(Color[] px, int texW, int x0, int y0, int w, int h, Color c)
        {
            int texH = px.Length / texW;
            int xEnd = x0 + w;
            int yEnd = y0 + h;
            for (int y = y0; y < yEnd; y++)
                for (int x = x0; x < xEnd; x++)
                    if ((uint)x < (uint)texW && (uint)y < (uint)texH)
                        px[y * texW + x] = c;
        }

        private static void Px(Color[] px, int texW, int x, int y, Color c)
        {
            if ((uint)x < (uint)texW && (uint)y < (uint)(px.Length / texW))
                px[y * texW + x] = c;
        }

        private static Color Darken(Color c, float f)
            => new Color(c.r * f, c.g * f, c.b * f, c.a);

        private static Color Lighten(Color c, float f)
            => new Color(
                Mathf.Min(1f, c.r + (1f - c.r) * f),
                Mathf.Min(1f, c.g + (1f - c.g) * f),
                Mathf.Min(1f, c.b + (1f - c.b) * f),
                c.a);

        private static (Color body, Color secondary, Color eyes) GetEraColors(int era)
        {
            return era switch
            {
                0 => (new Color(0.55f, 0.40f, 0.25f), new Color(0.70f, 0.55f, 0.35f), new Color(1.0f, 0.9f, 0.7f)),   // Stone Age
                1 => (new Color(0.72f, 0.52f, 0.15f), new Color(0.85f, 0.65f, 0.25f), new Color(1.0f, 0.95f, 0.6f)),  // Bronze Age
                2 => (new Color(0.60f, 0.15f, 0.15f), new Color(0.80f, 0.30f, 0.20f), new Color(1.0f, 1.0f, 0.8f)),   // Classical
                3 => (new Color(0.35f, 0.35f, 0.40f), new Color(0.55f, 0.55f, 0.60f), new Color(0.9f, 0.2f, 0.2f)),   // Medieval
                4 => (new Color(0.40f, 0.25f, 0.50f), new Color(0.60f, 0.40f, 0.70f), new Color(1.0f, 0.85f, 0.0f)),  // Renaissance
                5 => (new Color(0.30f, 0.30f, 0.30f), new Color(0.50f, 0.45f, 0.35f), new Color(1.0f, 0.5f, 0.0f)),   // Industrial
                6 => (new Color(0.25f, 0.35f, 0.20f), new Color(0.40f, 0.50f, 0.30f), new Color(1.0f, 0.0f, 0.0f)),   // Modern
                7 => (new Color(0.10f, 0.20f, 0.40f), new Color(0.15f, 0.35f, 0.65f), new Color(0.0f, 1.0f, 0.8f)),   // Digital
                8 => (new Color(0.20f, 0.10f, 0.30f), new Color(0.35f, 0.20f, 0.50f), new Color(0.6f, 0.0f, 1.0f)),   // Spacefaring
                9 => (new Color(0.80f, 0.80f, 0.85f), new Color(0.60f, 0.55f, 0.70f), new Color(1.0f, 1.0f, 1.0f)),   // Transcendent
                _ => (Color.gray, Color.white, Color.yellow),
            };
        }

        #endregion

        #region Tiles

        public static Sprite GetTileSprite(byte tileType)
        {
            string key = $"tile_{tileType}";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            Color color = tileType switch
            {
                (byte)TileType.Ground      => new Color(0.40f, 0.26f, 0.13f),
                (byte)TileType.GroundTop   => new Color(0.30f, 0.60f, 0.20f),
                (byte)TileType.GroundLeft  => new Color(0.35f, 0.22f, 0.10f),
                (byte)TileType.GroundRight => new Color(0.35f, 0.22f, 0.10f),
                (byte)TileType.Platform    => new Color(0.55f, 0.55f, 0.55f),
                (byte)TileType.Hazard      => new Color(0.90f, 0.10f, 0.10f),
                (byte)TileType.Decorative  => new Color(0.50f, 0.70f, 0.50f),
                (byte)TileType.DestructibleSoft       => new Color(0.85f, 0.75f, 0.55f),
                (byte)TileType.DestructibleMedium     => new Color(0.70f, 0.55f, 0.35f),
                (byte)TileType.DestructibleHard       => new Color(0.50f, 0.50f, 0.50f),
                (byte)TileType.DestructibleReinforced => new Color(0.35f, 0.35f, 0.40f),
                (byte)TileType.Indestructible         => new Color(0.20f, 0.20f, 0.25f),
                _ => Color.clear,
            };

            var sprite = CreateSquareSprite(color, TILE_PX, PPU);
            _cache[key] = sprite;
            return sprite;
        }

        #endregion

        #region Player

        public static Sprite GetPlayerSprite()
        {
            string key = "player";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];

            Color armor = new Color(0.20f, 0.40f, 0.90f);
            Color visor = new Color(0.40f, 0.75f, 1.00f);
            Color skin  = new Color(0.90f, 0.75f, 0.60f);
            Color boot  = new Color(0.15f, 0.25f, 0.55f);
            Color belt  = new Color(0.65f, 0.55f, 0.15f);

            // Boots (y=0..3)
            SetRect(px, size, 8, 0, 6, 4, boot);
            SetRect(px, size, 18, 0, 6, 4, boot);

            // Legs (y=4..12) with gap at x=14..17
            SetRect(px, size, 9, 4, 5, 9, armor);
            SetRect(px, size, 18, 4, 5, 9, armor);

            // Belt (y=13..15)
            SetRect(px, size, 9, 13, 14, 3, belt);
            Px(px, size, 15, 14, visor);
            Px(px, size, 16, 14, visor);

            // Torso (y=16..25)
            SetRect(px, size, 9, 16, 14, 10, armor);

            // Arms (y=14..23)
            SetRect(px, size, 6, 14, 3, 10, armor);
            SetRect(px, size, 23, 14, 3, 10, armor);
            // Hands (skin on lower 2px of arms)
            SetRect(px, size, 6, 14, 3, 2, skin);
            SetRect(px, size, 23, 14, 3, 2, skin);

            // Chin / face (y=25..26)
            SetRect(px, size, 13, 25, 6, 2, skin);

            // Helmet (y=26..31, overlaps chin area top)
            SetRect(px, size, 11, 27, 10, 5, armor);
            // Visor slit
            SetRect(px, size, 12, 28, 8, 2, visor);
            // Helmet crest
            SetRect(px, size, 14, 31, 4, 1, visor);

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0f), size);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        #endregion

        #region Enemies

        /// <summary>
        /// Get enemy sprite with era-specific colors and behavior-specific silhouette.
        /// 10 eras x 4 behaviors = 40 cached variants.
        /// </summary>
        public static Sprite GetEnemySprite(EnemyType type, EnemyBehavior behavior)
        {
            int era = (int)type / 3;
            string key = $"enemy_{era}_{(int)behavior}";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];
            var (body, secondary, eyes) = GetEraColors(era);

            switch (behavior)
            {
                case EnemyBehavior.Patrol:     PaintPatrolEnemy(px, size, body, secondary, eyes); break;
                case EnemyBehavior.Chase:       PaintChaseEnemy(px, size, body, secondary, eyes); break;
                case EnemyBehavior.Stationary:  PaintStationaryEnemy(px, size, body, secondary, eyes); break;
                case EnemyBehavior.Flying:      PaintFlyingEnemy(px, size, body, secondary, eyes); break;
            }

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0f), size);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        /// <summary>Upright humanoid soldier — head, torso, arms, two legs.</summary>
        private static void PaintPatrolEnemy(Color[] px, int s, Color body, Color sec, Color eyes)
        {
            // Head
            SetRect(px, s, 12, 25, 8, 6, sec);
            Px(px, s, 14, 28, eyes);
            Px(px, s, 17, 28, eyes);

            // Torso
            SetRect(px, s, 10, 14, 12, 11, body);

            // Belt
            SetRect(px, s, 10, 13, 12, 2, Darken(body, 0.7f));

            // Arms
            SetRect(px, s, 7, 14, 3, 9, body);
            SetRect(px, s, 22, 14, 3, 9, body);

            // Legs (gap at x=14..17 for readability)
            SetRect(px, s, 10, 0, 4, 14, sec);
            SetRect(px, s, 18, 0, 4, 14, sec);
        }

        /// <summary>Low wide quadruped beast — forward head, wide body, stub legs, back spikes.</summary>
        private static void PaintChaseEnemy(Color[] px, int s, Color body, Color sec, Color eyes)
        {
            // Wide low body
            SetRect(px, s, 4, 8, 24, 13, body);

            // Forward-jutting head
            SetRect(px, s, 22, 16, 9, 9, sec);
            Px(px, s, 25, 22, eyes);
            Px(px, s, 28, 22, eyes);

            // Jaw / mouth
            SetRect(px, s, 24, 16, 6, 3, Darken(body, 0.6f));

            // Four stub legs
            SetRect(px, s, 6, 4, 3, 4, sec);
            SetRect(px, s, 12, 4, 3, 4, sec);
            SetRect(px, s, 20, 4, 3, 4, sec);
            SetRect(px, s, 25, 4, 3, 4, sec);

            // Back spikes
            Color spike = Lighten(body, 0.3f);
            Px(px, s, 10, 21, spike); Px(px, s, 10, 22, spike);
            Px(px, s, 15, 21, spike); Px(px, s, 15, 22, spike); Px(px, s, 15, 23, spike);
            Px(px, s, 20, 21, spike); Px(px, s, 20, 22, spike);
        }

        /// <summary>Turret/cannon — flat base, pedestal, dome, protruding barrel.</summary>
        private static void PaintStationaryEnemy(Color[] px, int s, Color body, Color sec, Color eyes)
        {
            // Wide flat base
            SetRect(px, s, 8, 0, 16, 9, sec);

            // Pedestal
            SetRect(px, s, 12, 9, 8, 6, body);

            // Dome
            SetRect(px, s, 10, 15, 12, 8, body);

            // Barrel
            SetRect(px, s, 22, 17, 9, 4, Darken(body, 0.7f));

            // Barrel tip glow
            Px(px, s, 30, 17, eyes);
            Px(px, s, 30, 18, eyes);
            Px(px, s, 30, 19, eyes);
            Px(px, s, 30, 20, eyes);

            // Sensor eye on dome
            Px(px, s, 14, 19, eyes);
            Px(px, s, 15, 19, eyes);

            // Base bolts
            Px(px, s, 10, 2, Darken(sec, 0.5f));
            Px(px, s, 21, 2, Darken(sec, 0.5f));
        }

        /// <summary>Winged creature — narrow body, spread triangular wings, tail.</summary>
        private static void PaintFlyingEnemy(Color[] px, int s, Color body, Color sec, Color eyes)
        {
            // Narrow body
            SetRect(px, s, 12, 10, 8, 13, body);

            // Head
            SetRect(px, s, 13, 23, 6, 6, sec);
            Px(px, s, 14, 26, eyes);
            Px(px, s, 17, 26, eyes);

            // Left wing (triangle narrowing downward from body)
            for (int row = 0; row < 7; row++)
            {
                int wingY = 22 - row;
                int startX = 4 + row;
                int endX = 11;
                if (startX <= endX)
                    SetRect(px, s, startX, wingY, endX - startX + 1, 1, sec);
            }

            // Right wing (mirror triangle)
            for (int row = 0; row < 7; row++)
            {
                int wingY = 22 - row;
                int startX = 20;
                int endX = 27 - row;
                if (startX <= endX)
                    SetRect(px, s, startX, wingY, endX - startX + 1, 1, sec);
            }

            // Wing tip glow
            Px(px, s, 4, 22, eyes);
            Px(px, s, 27, 22, eyes);

            // Tail
            SetRect(px, s, 14, 6, 4, 4, Darken(body, 0.7f));
        }

        #endregion

        #region Projectiles and Weapons

        public static Sprite GetProjectileSprite()
        {
            string key = "projectile";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            var sprite = CreateSquareSprite(new Color(1f, 1f, 0.2f), 4, PPU);
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

        #endregion

        #region Rewards

        /// <summary>
        /// Reward sprites with recognizable shapes: heart, cross, sword, arrow, shield, coin.
        /// 10x10 pixels at PPU=8 (slightly larger than 1 tile for visibility).
        /// </summary>
        public static Sprite GetRewardSprite(RewardType type)
        {
            string key = $"reward_{type}";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            int size = 10;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];

            switch (type)
            {
                case RewardType.HealthSmall: PaintHealthSmall(px, size); break;
                case RewardType.HealthLarge: PaintHealthLarge(px, size); break;
                case RewardType.AttackBoost: PaintAttackBoost(px, size); break;
                case RewardType.SpeedBoost:  PaintSpeedBoost(px, size); break;
                case RewardType.Shield:      PaintShield(px, size); break;
                case RewardType.Coin:        PaintCoin(px, size); break;
            }

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), PPU);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        /// <summary>Green cross (+) shape.</summary>
        private static void PaintHealthSmall(Color[] px, int s)
        {
            Color c = new Color(0.20f, 0.90f, 0.20f);
            SetRect(px, s, 4, 2, 2, 6, c);  // vertical bar
            SetRect(px, s, 2, 4, 6, 2, c);  // horizontal bar
        }

        /// <summary>Green heart shape.</summary>
        private static void PaintHealthLarge(Color[] px, int s)
        {
            Color c = new Color(0.10f, 1.00f, 0.10f);
            // Top bumps
            SetRect(px, s, 2, 8, 2, 1, c);
            SetRect(px, s, 6, 8, 2, 1, c);
            // Wide upper body
            SetRect(px, s, 1, 6, 8, 2, c);
            // Taper down
            SetRect(px, s, 2, 5, 6, 1, c);
            SetRect(px, s, 3, 4, 4, 1, c);
            SetRect(px, s, 4, 3, 2, 1, c);
        }

        /// <summary>Orange upward sword.</summary>
        private static void PaintAttackBoost(Color[] px, int s)
        {
            Color blade = new Color(1.00f, 0.50f, 0.10f);
            Color guard = Darken(blade, 0.7f);
            Color grip  = Darken(blade, 0.5f);

            SetRect(px, s, 4, 4, 2, 6, blade);     // blade
            SetRect(px, s, 2, 3, 6, 1, guard);      // crossguard
            SetRect(px, s, 4, 1, 2, 2, grip);       // grip
            SetRect(px, s, 3, 0, 4, 1, blade);      // pommel
        }

        /// <summary>Cyan right-pointing arrow.</summary>
        private static void PaintSpeedBoost(Color[] px, int s)
        {
            Color c = new Color(0.20f, 0.80f, 1.00f);
            // Shaft
            SetRect(px, s, 1, 4, 4, 2, c);
            // Arrow head (wider)
            SetRect(px, s, 5, 3, 3, 4, c);
            // Point tip
            SetRect(px, s, 8, 4, 1, 2, c);
            // Tail fins
            Px(px, s, 1, 7, c);
            Px(px, s, 1, 2, c);
        }

        /// <summary>Blue-white shield shape.</summary>
        private static void PaintShield(Color[] px, int s)
        {
            Color c = new Color(0.70f, 0.70f, 1.00f);
            SetRect(px, s, 3, 8, 4, 1, c);          // top edge
            SetRect(px, s, 2, 5, 6, 3, c);          // upper body
            SetRect(px, s, 3, 4, 4, 1, c);          // taper
            SetRect(px, s, 4, 3, 2, 1, c);          // point
            // Emblem (darker center)
            SetRect(px, s, 4, 6, 2, 1, Darken(c, 0.6f));
        }

        /// <summary>Gold circle with inner detail.</summary>
        private static void PaintCoin(Color[] px, int s)
        {
            Color gold = new Color(1.00f, 0.85f, 0.10f);
            Color inner = Darken(gold, 0.7f);
            Color highlight = Lighten(gold, 0.4f);

            // Outer circle
            SetRect(px, s, 3, 1, 4, 1, gold);
            SetRect(px, s, 2, 2, 6, 1, gold);
            SetRect(px, s, 1, 3, 8, 4, gold);
            SetRect(px, s, 2, 7, 6, 1, gold);
            SetRect(px, s, 3, 8, 4, 1, gold);
            // Inner darker ring
            SetRect(px, s, 3, 3, 4, 4, inner);
            // Highlight sparkle
            Px(px, s, 4, 5, highlight);
            Px(px, s, 5, 4, highlight);
        }

        #endregion

        #region Checkpoints

        /// <summary>
        /// Checkpoint flag sprite. Color varies by type:
        /// LevelStart=green, MidLevel=gold, PreBoss=orange, BossArena=red.
        /// </summary>
        public static Sprite GetCheckpointSprite(CheckpointType type)
        {
            string key = $"checkpoint_{(int)type}";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            int w = 8, h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[w * h];

            Color pole = new Color(0.60f, 0.40f, 0.20f);

            // Pole (all types)
            SetRect(px, w, 1, 0, 1, 16, pole);

            switch (type)
            {
                case CheckpointType.LevelStart:
                {
                    Color flag = new Color(0.30f, 0.85f, 0.30f);
                    SetRect(px, w, 2, 10, 5, 6, flag);
                    SetRect(px, w, 2, 12, 5, 1, Lighten(flag, 0.4f));
                    break;
                }
                case CheckpointType.MidLevel:
                {
                    Color flag = new Color(1.0f, 0.85f, 0.0f);
                    SetRect(px, w, 2, 10, 5, 6, flag);
                    SetRect(px, w, 2, 12, 5, 1, new Color(1.0f, 1.0f, 0.5f));
                    break;
                }
                case CheckpointType.PreBoss:
                {
                    Color flag = new Color(0.90f, 0.40f, 0.10f);
                    SetRect(px, w, 2, 10, 5, 6, flag);
                    // Warning dots (skull eyes)
                    Px(px, w, 3, 13, Color.white);
                    Px(px, w, 5, 13, Color.white);
                    break;
                }
                case CheckpointType.BossArena:
                {
                    Color flag = new Color(0.85f, 0.10f, 0.10f);
                    SetRect(px, w, 2, 10, 5, 6, flag);
                    // Exclamation mark (!)
                    Px(px, w, 4, 14, Color.white);
                    Px(px, w, 4, 13, Color.white);
                    Px(px, w, 4, 12, Color.white);
                    Px(px, w, 4, 11, Color.white);
                    break;
                }
            }

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), PPU);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        #endregion

        #region Goal

        /// <summary>
        /// Goal portal: gold archway frame with glowing cyan interior.
        /// 16x16 at PPU=8 = 2 Unity units. Largest standard pickup sprite.
        /// </summary>
        public static Sprite GetGoalSprite()
        {
            string key = "goal";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            int size = 16;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];

            Color frame     = new Color(0.75f, 0.60f, 0.15f);
            Color portal    = new Color(0.30f, 0.90f, 1.00f);
            Color highlight = new Color(0.60f, 1.00f, 1.00f);
            Color star      = new Color(1.00f, 1.00f, 0.80f);

            // Frame: base, pillars, arch
            SetRect(px, size, 0, 0, 16, 2, frame);     // base
            SetRect(px, size, 0, 0, 2, 14, frame);      // left pillar
            SetRect(px, size, 14, 0, 2, 14, frame);     // right pillar
            SetRect(px, size, 0, 14, 16, 2, frame);     // top arch

            // Portal interior
            SetRect(px, size, 2, 2, 12, 12, portal);

            // Center glow
            SetRect(px, size, 5, 5, 6, 6, highlight);

            // Sparkle accents
            Px(px, size, 4, 12, star);
            Px(px, size, 11, 4, star);
            Px(px, size, 7, 10, star);
            Px(px, size, 9, 7, star);

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), PPU);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        #endregion

        #region Bosses

        /// <summary>
        /// Boss sprite: large imposing figure (48x48 at PPU=16 = 3 Unity units).
        /// Colored per era palette. Forward-compatible API for boss spawning.
        /// </summary>
        public static Sprite GetBossSprite(BossType type)
        {
            int era = (int)type;
            string key = $"boss_{era}";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            int size = 48;
            float bossPPU = 16f;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];

            var (body, secondary, eyes) = GetEraColors(era);
            // Bosses get slightly brighter/more saturated colors
            body = Lighten(body, 0.15f);
            secondary = Lighten(secondary, 0.15f);

            // Legs
            SetRect(px, size, 14, 0, 8, 20, secondary);
            SetRect(px, size, 26, 0, 8, 20, secondary);

            // Belt / waist
            SetRect(px, size, 12, 18, 24, 3, Darken(body, 0.6f));

            // Torso
            SetRect(px, size, 12, 20, 24, 20, body);

            // Shoulder pads
            SetRect(px, size, 8, 32, 4, 8, secondary);
            SetRect(px, size, 36, 32, 4, 8, secondary);

            // Arms
            SetRect(px, size, 6, 20, 6, 12, body);
            SetRect(px, size, 36, 20, 6, 12, body);

            // Hands (glowing)
            SetRect(px, size, 4, 16, 4, 4, eyes);
            SetRect(px, size, 40, 16, 4, 4, eyes);

            // Head / Crown
            SetRect(px, size, 18, 40, 12, 8, secondary);
            // Crown points
            Px(px, size, 20, 47, eyes);
            Px(px, size, 24, 47, eyes);
            Px(px, size, 28, 47, eyes);

            // Eyes (2x2 blocks)
            SetRect(px, size, 21, 44, 2, 2, eyes);
            SetRect(px, size, 26, 44, 2, 2, eyes);

            // Chest emblem (glowing)
            SetRect(px, size, 20, 28, 8, 6, eyes);

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0f), bossPPU);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        #endregion

        #region Utility

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

        #endregion
    }
}
