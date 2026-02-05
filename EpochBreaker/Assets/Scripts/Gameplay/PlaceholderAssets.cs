using System.Collections.Generic;
using UnityEngine;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Generates all game sprites at runtime via Texture2D pixel painting.
    /// No imported art assets needed. Each sprite is cached by key.
    /// Native 64px tile resolution for crisp pixel art on mobile Retina displays.
    /// </summary>
    public static class PlaceholderAssets
    {
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();
        private const int TILE_PX = 64;
        private const float PPU = 64f;

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
            int texH = px.Length / texW;
            if ((uint)x < (uint)texW && (uint)y < (uint)texH)
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
                0 => (new Color(0.55f, 0.40f, 0.25f), new Color(0.70f, 0.55f, 0.35f), new Color(1.0f, 0.9f, 0.7f)),
                1 => (new Color(0.72f, 0.52f, 0.15f), new Color(0.85f, 0.65f, 0.25f), new Color(1.0f, 0.95f, 0.6f)),
                2 => (new Color(0.60f, 0.15f, 0.15f), new Color(0.80f, 0.30f, 0.20f), new Color(1.0f, 1.0f, 0.8f)),
                3 => (new Color(0.35f, 0.35f, 0.40f), new Color(0.55f, 0.55f, 0.60f), new Color(0.9f, 0.2f, 0.2f)),
                4 => (new Color(0.40f, 0.25f, 0.50f), new Color(0.60f, 0.40f, 0.70f), new Color(1.0f, 0.85f, 0.0f)),
                5 => (new Color(0.30f, 0.30f, 0.30f), new Color(0.50f, 0.45f, 0.35f), new Color(1.0f, 0.5f, 0.0f)),
                6 => (new Color(0.25f, 0.35f, 0.20f), new Color(0.40f, 0.50f, 0.30f), new Color(1.0f, 0.0f, 0.0f)),
                7 => (new Color(0.10f, 0.20f, 0.40f), new Color(0.15f, 0.35f, 0.65f), new Color(0.0f, 1.0f, 0.8f)),
                8 => (new Color(0.20f, 0.10f, 0.30f), new Color(0.35f, 0.20f, 0.50f), new Color(0.6f, 0.0f, 1.0f)),
                9 => (new Color(0.80f, 0.80f, 0.85f), new Color(0.60f, 0.55f, 0.70f), new Color(1.0f, 1.0f, 1.0f)),
                _ => (Color.gray, Color.white, Color.yellow),
            };
        }

        /// <summary>
        /// Get epoch-specific terrain colors for ground, surface, blocks, and accent.
        /// Each epoch has a distinct visual theme.
        /// </summary>
        private static (Color ground, Color surface, Color block, Color accent) GetEpochTerrainColors(int epoch)
        {
            return epoch switch
            {
                // Era 0: Stone Age - earthy browns, grass green
                0 => (new Color(0.40f, 0.26f, 0.13f),   // dirt brown
                      new Color(0.30f, 0.60f, 0.20f),   // grass green
                      new Color(0.85f, 0.75f, 0.55f),   // sandstone
                      new Color(0.55f, 0.40f, 0.25f)),  // wood brown

                // Era 1: Bronze Age - sandy, clay, warm tones
                1 => (new Color(0.65f, 0.50f, 0.30f),   // sandy clay
                      new Color(0.75f, 0.60f, 0.35f),   // desert sand
                      new Color(0.72f, 0.52f, 0.15f),   // bronze/copper
                      new Color(0.85f, 0.65f, 0.25f)),  // gold accent

                // Era 2: Classical - white marble, terracotta
                2 => (new Color(0.75f, 0.70f, 0.65f),   // limestone
                      new Color(0.90f, 0.87f, 0.82f),   // white marble
                      new Color(0.70f, 0.45f, 0.35f),   // terracotta
                      new Color(0.60f, 0.15f, 0.15f)),  // roman red

                // Era 3: Medieval - gray stone, cobblestone
                3 => (new Color(0.35f, 0.35f, 0.38f),   // dark stone
                      new Color(0.50f, 0.50f, 0.52f),   // cobblestone
                      new Color(0.45f, 0.42f, 0.40f),   // castle stone
                      new Color(0.55f, 0.55f, 0.60f)),  // iron gray

                // Era 4: Renaissance - ornate brick, burgundy
                4 => (new Color(0.55f, 0.35f, 0.30f),   // dark brick
                      new Color(0.70f, 0.45f, 0.35f),   // terracotta brick
                      new Color(0.50f, 0.30f, 0.45f),   // purple stone
                      new Color(0.80f, 0.65f, 0.20f)),  // gold trim

                // Era 5: Industrial - rusted steel, riveted metal
                5 => (new Color(0.35f, 0.30f, 0.28f),   // industrial gray
                      new Color(0.45f, 0.38f, 0.32f),   // rusted metal
                      new Color(0.50f, 0.45f, 0.35f),   // brass
                      new Color(0.70f, 0.40f, 0.15f)),  // rust orange

                // Era 6: Modern - concrete, asphalt
                6 => (new Color(0.45f, 0.45f, 0.48f),   // concrete
                      new Color(0.25f, 0.25f, 0.28f),   // asphalt
                      new Color(0.55f, 0.55f, 0.58f),   // cement block
                      new Color(0.90f, 0.70f, 0.10f)),  // warning yellow

                // Era 7: Digital - circuit board, neon grid
                7 => (new Color(0.08f, 0.15f, 0.12f),   // dark PCB green
                      new Color(0.12f, 0.25f, 0.18f),   // circuit green
                      new Color(0.15f, 0.35f, 0.65f),   // data blue
                      new Color(0.00f, 1.00f, 0.80f)),  // neon cyan

                // Era 8: Spacefaring - space station, alien metal
                8 => (new Color(0.18f, 0.18f, 0.25f),   // space gray
                      new Color(0.28f, 0.28f, 0.38f),   // hull metal
                      new Color(0.35f, 0.20f, 0.50f),   // alien purple
                      new Color(0.60f, 0.00f, 1.00f)),  // plasma purple

                // Era 9: Transcendent - crystalline, ethereal
                9 => (new Color(0.70f, 0.70f, 0.80f),   // crystal gray
                      new Color(0.85f, 0.85f, 0.95f),   // pure white
                      new Color(0.75f, 0.75f, 0.90f),   // light crystal
                      new Color(1.00f, 1.00f, 1.00f)),  // pure light

                _ => (Color.gray, Color.white, Color.gray, Color.yellow),
            };
        }

        #endregion

        #region Tiles

        /// <summary>
        /// Get tile sprite for default epoch (0 - Stone Age).
        /// </summary>
        public static Sprite GetTileSprite(byte tileType)
        {
            return GetTileSprite(tileType, 0);
        }

        /// <summary>
        /// Get tile sprite with epoch-specific visual theme.
        /// </summary>
        public static Sprite GetTileSprite(byte tileType, int epoch)
        {
            string key = $"tile_{tileType}_e{epoch}";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            int s = TILE_PX;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[s * s];

            PaintTile(px, s, tileType, epoch);

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), PPU);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        private static void PaintTile(Color[] px, int s, byte tileType, int epoch = 0)
        {
            var (ground, surface, block, accent) = GetEpochTerrainColors(epoch);

            switch (tileType)
            {
                case (byte)TileType.Ground:
                {
                    Color dirt = ground;
                    Color d1 = Darken(dirt, 0.60f);
                    Color d2 = Darken(dirt, 0.70f);
                    Color d3 = Darken(dirt, 0.80f);
                    Color d4 = Darken(dirt, 0.90f);
                    Color l1 = Lighten(dirt, 0.10f);
                    Color l2 = Lighten(dirt, 0.20f);
                    Color l3 = Lighten(dirt, 0.30f);
                    // 8-band vertical gradient
                    SetRect(px, s, 0, 0, 64, 8, d1);
                    SetRect(px, s, 0, 8, 64, 8, d2);
                    SetRect(px, s, 0, 16, 64, 8, d3);
                    SetRect(px, s, 0, 24, 64, 8, d4);
                    SetRect(px, s, 0, 32, 64, 8, dirt);
                    SetRect(px, s, 0, 40, 64, 8, l1);
                    SetRect(px, s, 0, 48, 64, 8, l2);
                    SetRect(px, s, 0, 56, 64, 8, l3);
                    // Epoch-specific texture
                    PaintGroundTexture(px, s, epoch, d1, d2, d3, l1, l2);
                    break;
                }
                case (byte)TileType.GroundTop:
                {
                    Color dirt = ground;
                    Color dirtDk = Darken(dirt, 0.70f);
                    Color dirtMd = Darken(dirt, 0.85f);
                    Color dirtLt = Lighten(dirt, 0.10f);
                    Color surf = surface;
                    Color surfLight = Lighten(surf, 0.35f);
                    Color surfDark = Darken(surf, 0.65f);
                    Color surfMid = Darken(surf, 0.85f);
                    Color detail = Darken(surf, 0.50f);
                    // Ground body with gradient
                    SetRect(px, s, 0, 0, 64, 8, dirtDk);
                    SetRect(px, s, 0, 8, 64, 8, dirtMd);
                    SetRect(px, s, 0, 16, 64, 16, dirt);
                    SetRect(px, s, 0, 32, 64, 12, dirtLt);
                    // Texture in ground
                    SetRect(px, s, 18, 14, 3, 2, dirtDk); SetRect(px, s, 42, 8, 2, 2, dirtDk);
                    Px(px, s, 10, 22, dirtDk); Px(px, s, 30, 18, dirtDk); Px(px, s, 54, 12, dirtDk);
                    SetRect(px, s, 50, 26, 3, 2, dirtDk);
                    // Detail tendrils reaching down from surface
                    Px(px, s, 12, 42, detail); Px(px, s, 13, 40, detail); Px(px, s, 14, 38, detail);
                    Px(px, s, 38, 42, detail); Px(px, s, 39, 40, detail);
                    Px(px, s, 56, 41, detail); Px(px, s, 57, 39, detail);
                    // Surface layer (top 20 rows)
                    SetRect(px, s, 0, 44, 64, 20, surf);
                    SetRect(px, s, 0, 44, 64, 4, surfDark); // roots/base zone
                    SetRect(px, s, 0, 48, 64, 4, surfMid);
                    // Epoch-specific surface texture
                    PaintSurfaceTexture(px, s, epoch, surf, surfLight, surfDark);
                    break;
                }
                case (byte)TileType.GroundLeft:
                {
                    Color dirt = Darken(ground, 0.90f);
                    Color edgeDk = Darken(dirt, 0.55f);
                    Color edgeMd = Darken(dirt, 0.70f);
                    Color edgeLt = Darken(dirt, 0.85f);
                    Color inner = Lighten(dirt, 0.10f);
                    Color innerLt = Lighten(dirt, 0.20f);
                    SetRect(px, s, 0, 0, 64, 64, dirt);
                    // Left edge gradient (4 bands)
                    SetRect(px, s, 0, 0, 8, 64, edgeDk);
                    SetRect(px, s, 8, 0, 6, 64, edgeMd);
                    SetRect(px, s, 14, 0, 4, 64, edgeLt);
                    // Right side lighter
                    SetRect(px, s, 52, 0, 12, 64, inner);
                    SetRect(px, s, 58, 0, 6, 64, innerLt);
                    // Texture clusters
                    SetRect(px, s, 22, 14, 4, 3, edgeDk); SetRect(px, s, 26, 15, 3, 2, edgeMd);
                    SetRect(px, s, 36, 38, 3, 3, edgeDk); SetRect(px, s, 39, 39, 2, 2, edgeMd);
                    SetRect(px, s, 28, 50, 3, 2, edgeDk);
                    Px(px, s, 44, 10, inner); Px(px, s, 45, 10, inner); Px(px, s, 46, 11, inner);
                    Px(px, s, 32, 28, inner); Px(px, s, 33, 28, inner);
                    Px(px, s, 48, 44, innerLt);
                    // Crack/seam line
                    for (int i = 0; i < 6; i++) Px(px, s, 20 + i, 32 + i, edgeDk);
                    break;
                }
                case (byte)TileType.GroundRight:
                {
                    Color dirt = Darken(ground, 0.90f);
                    Color edgeDk = Darken(dirt, 0.55f);
                    Color edgeMd = Darken(dirt, 0.70f);
                    Color edgeLt = Darken(dirt, 0.85f);
                    Color inner = Lighten(dirt, 0.10f);
                    Color innerLt = Lighten(dirt, 0.20f);
                    SetRect(px, s, 0, 0, 64, 64, dirt);
                    // Right edge gradient
                    SetRect(px, s, 56, 0, 8, 64, edgeDk);
                    SetRect(px, s, 50, 0, 6, 64, edgeMd);
                    SetRect(px, s, 46, 0, 4, 64, edgeLt);
                    // Left side lighter
                    SetRect(px, s, 0, 0, 12, 64, inner);
                    SetRect(px, s, 0, 0, 6, 64, innerLt);
                    // Texture
                    SetRect(px, s, 34, 20, 4, 3, edgeDk); SetRect(px, s, 38, 21, 3, 2, edgeMd);
                    SetRect(px, s, 22, 44, 3, 3, edgeDk); SetRect(px, s, 25, 45, 2, 2, edgeMd);
                    SetRect(px, s, 30, 10, 3, 2, edgeDk);
                    Px(px, s, 16, 32, inner); Px(px, s, 17, 32, inner); Px(px, s, 18, 33, inner);
                    Px(px, s, 28, 54, inner);
                    // Crack/seam line
                    for (int i = 0; i < 6; i++) Px(px, s, 38 - i, 26 + i, edgeDk);
                    break;
                }
                case (byte)TileType.Platform:
                {
                    Color stone = new Color(0.55f, 0.55f, 0.55f);
                    Color light = Lighten(stone, 0.25f);
                    Color lighter = Lighten(stone, 0.40f);
                    Color dark = Darken(stone, 0.70f);
                    Color mortar = Darken(stone, 0.50f);
                    SetRect(px, s, 0, 0, 64, 64, stone);
                    // Top edge highlight (bevel)
                    SetRect(px, s, 0, 58, 64, 6, light);
                    SetRect(px, s, 0, 62, 64, 2, lighter);
                    // Bottom shadow
                    SetRect(px, s, 0, 0, 64, 2, dark);
                    // Horizontal mortar lines (4 courses)
                    SetRect(px, s, 0, 14, 64, 2, mortar);
                    SetRect(px, s, 0, 30, 64, 2, mortar);
                    SetRect(px, s, 0, 46, 64, 2, mortar);
                    // Vertical joints (offset per course)
                    SetRect(px, s, 18, 0, 2, 14, mortar);
                    SetRect(px, s, 42, 0, 2, 14, mortar);
                    SetRect(px, s, 10, 16, 2, 14, mortar);
                    SetRect(px, s, 32, 16, 2, 14, mortar);
                    SetRect(px, s, 54, 16, 2, 14, mortar);
                    SetRect(px, s, 14, 32, 2, 14, mortar);
                    SetRect(px, s, 38, 32, 2, 14, mortar);
                    SetRect(px, s, 22, 48, 2, 10, mortar);
                    SetRect(px, s, 48, 48, 2, 10, mortar);
                    // Surface scratches and wear
                    Px(px, s, 8, 6, dark); Px(px, s, 9, 7, dark); Px(px, s, 10, 8, dark);
                    Px(px, s, 28, 22, dark); Px(px, s, 29, 23, dark);
                    Px(px, s, 50, 38, dark); Px(px, s, 51, 39, dark);
                    Px(px, s, 46, 8, dark); Px(px, s, 24, 42, dark);
                    // Subtle highlight on some bricks
                    SetRect(px, s, 4, 4, 12, 2, light);
                    SetRect(px, s, 36, 20, 14, 2, light);
                    break;
                }
                case (byte)TileType.Hazard:
                {
                    Color red = new Color(0.90f, 0.10f, 0.10f);
                    Color orange = new Color(1.0f, 0.50f, 0.10f);
                    Color yellow = new Color(1.0f, 0.80f, 0.20f);
                    Color white = new Color(1.0f, 0.95f, 0.70f);
                    Color darkRed = Darken(red, 0.50f);
                    Color midRed = Darken(red, 0.75f);
                    SetRect(px, s, 0, 0, 64, 64, red);
                    SetRect(px, s, 0, 0, 64, 10, darkRed);
                    SetRect(px, s, 0, 10, 64, 6, midRed);
                    // 5 detailed spike triangles
                    for (int spike = 0; spike < 5; spike++)
                    {
                        int bx = 3 + spike * 12;
                        SetRect(px, s, bx, 44, 8, 4, orange);
                        SetRect(px, s, bx + 1, 48, 6, 4, orange);
                        SetRect(px, s, bx + 2, 52, 4, 4, yellow);
                        SetRect(px, s, bx + 2, 56, 4, 4, yellow);
                        SetRect(px, s, bx + 3, 60, 2, 4, white);
                        // Edge shading on spikes
                        Px(px, s, bx, 44, darkRed); Px(px, s, bx + 7, 44, darkRed);
                    }
                    // Warning stripes across middle
                    SetRect(px, s, 0, 24, 64, 3, orange);
                    SetRect(px, s, 0, 34, 64, 3, orange);
                    // Hazard dots
                    for (int i = 0; i < 8; i++) Px(px, s, 4 + i * 8, 30, yellow);
                    break;
                }
                case (byte)TileType.Decorative:
                {
                    Color green = new Color(0.50f, 0.70f, 0.50f);
                    Color vine = new Color(0.35f, 0.55f, 0.30f);
                    Color vineDk = Darken(vine, 0.65f);
                    Color moss = Darken(green, 0.70f);
                    Color mossLt = Darken(green, 0.85f);
                    Color flower1 = new Color(0.85f, 0.70f, 0.90f);
                    Color flower2 = new Color(0.90f, 0.55f, 0.60f);
                    Color petal = new Color(1.0f, 0.90f, 0.95f);
                    SetRect(px, s, 0, 0, 64, 64, green);
                    // Diagonal vine streaks (longer, with branches)
                    for (int i = 0; i < 18; i++)
                    {
                        Px(px, s, 6 + i, 56 - i, vine); Px(px, s, 7 + i, 56 - i, vine);
                        if (i == 8) { Px(px, s, 14, 50, vineDk); Px(px, s, 15, 51, vineDk); Px(px, s, 16, 52, vineDk); }
                    }
                    for (int i = 0; i < 14; i++)
                    {
                        Px(px, s, 36 + i, 60 - i, vine); Px(px, s, 37 + i, 60 - i, vine);
                        if (i == 6) { Px(px, s, 43, 56, vineDk); Px(px, s, 44, 57, vineDk); }
                    }
                    // Moss clusters with variation
                    SetRect(px, s, 4, 10, 6, 4, moss); SetRect(px, s, 5, 11, 4, 2, mossLt);
                    SetRect(px, s, 40, 22, 5, 4, moss); SetRect(px, s, 41, 23, 3, 2, mossLt);
                    SetRect(px, s, 26, 4, 4, 4, moss);
                    SetRect(px, s, 54, 36, 5, 3, moss);
                    // Flowers with petals
                    Px(px, s, 14, 42, flower1); Px(px, s, 15, 43, flower1); Px(px, s, 13, 43, flower1);
                    Px(px, s, 14, 44, flower1); Px(px, s, 14, 43, petal);
                    Px(px, s, 48, 16, flower2); Px(px, s, 49, 17, flower2); Px(px, s, 47, 17, flower2);
                    Px(px, s, 48, 18, flower2); Px(px, s, 48, 17, petal);
                    // Leaf details
                    Px(px, s, 30, 32, vineDk); Px(px, s, 31, 33, vine); Px(px, s, 32, 33, vine);
                    break;
                }
                case (byte)TileType.DestructibleSoft:
                {
                    // Soft destructible uses lighter version of block color
                    Color c = Lighten(block, 0.15f);
                    Color crack = Darken(c, 0.45f);
                    Color crackLt = Darken(c, 0.60f);
                    Color crackFine = Darken(c, 0.75f);
                    SetRect(px, s, 0, 0, 64, 64, c);
                    // Major cracks (longer, branching)
                    for (int i = 0; i < 12; i++) { Px(px, s, 10 + i, 48 - i, crack); Px(px, s, 11 + i, 48 - i, crackLt); }
                    for (int i = 0; i < 10; i++) { Px(px, s, 38 + i, 56 - i, crack); Px(px, s, 39 + i, 56 - i, crackLt); }
                    for (int i = 0; i < 8; i++) { Px(px, s, 6 + i, 20 - i, crack); Px(px, s, 7 + i, 20 - i, crackLt); }
                    for (int i = 0; i < 10; i++) { Px(px, s, 34 + i, 28 - i, crack); Px(px, s, 35 + i, 28 - i, crackLt); }
                    for (int i = 0; i < 6; i++) Px(px, s, 52 + i, 16 - i, crack);
                    for (int i = 0; i < 5; i++) Px(px, s, 26 + i, 8 - i, crack);
                    // Branch cracks
                    for (int i = 0; i < 4; i++) Px(px, s, 18 + i, 42 + i, crackLt);
                    for (int i = 0; i < 3; i++) Px(px, s, 44 - i, 48 + i, crackLt);
                    // Fine surface cracks
                    for (int i = 0; i < 5; i++) Px(px, s, 8 + i, 56 - i, crackFine);
                    for (int i = 0; i < 4; i++) Px(px, s, 50 + i, 36 - i, crackFine);
                    // Missing chunk
                    SetRect(px, s, 3, 30, 5, 5, crack); SetRect(px, s, 4, 31, 3, 3, crackLt);
                    // Spalling
                    SetRect(px, s, 56, 50, 3, 3, crackLt);
                    break;
                }
                case (byte)TileType.DestructibleMedium:
                {
                    // Medium destructible uses block color directly
                    Color c = block;
                    Color crack = Darken(c, 0.45f);
                    Color crackLt = Darken(c, 0.60f);
                    Color crackFine = Darken(c, 0.75f);
                    SetRect(px, s, 0, 0, 64, 64, c);
                    for (int i = 0; i < 10; i++) { Px(px, s, 14 + i, 44 - i, crack); Px(px, s, 15 + i, 44 - i, crackLt); }
                    for (int i = 0; i < 8; i++) { Px(px, s, 42 + i, 52 - i, crack); Px(px, s, 43 + i, 52 - i, crackLt); }
                    for (int i = 0; i < 6; i++) { Px(px, s, 10 + i, 16 - i, crack); Px(px, s, 11 + i, 16 - i, crackLt); }
                    for (int i = 0; i < 7; i++) { Px(px, s, 34 + i, 24 - i, crack); Px(px, s, 35 + i, 24 - i, crackLt); }
                    // Fine cracks
                    for (int i = 0; i < 4; i++) Px(px, s, 54 + i, 36 - i, crackFine);
                    for (int i = 0; i < 4; i++) Px(px, s, 6 + i, 54 - i, crackFine);
                    break;
                }
                case (byte)TileType.DestructibleHard:
                {
                    // Hard destructible uses darker block color
                    Color c = Darken(block, 0.85f);
                    Color crack = Darken(c, 0.45f);
                    Color crackLt = Darken(c, 0.65f);
                    SetRect(px, s, 0, 0, 64, 64, c);
                    for (int i = 0; i < 8; i++) { Px(px, s, 18 + i, 40 - i, crack); Px(px, s, 19 + i, 40 - i, crackLt); }
                    for (int i = 0; i < 6; i++) { Px(px, s, 42 + i, 20 - i, crack); Px(px, s, 43 + i, 20 - i, crackLt); }
                    for (int i = 0; i < 5; i++) Px(px, s, 10 + i, 54 - i, crack);
                    break;
                }
                case (byte)TileType.DestructibleReinforced:
                {
                    // Reinforced uses very dark block color with metallic accents
                    Color c = Darken(block, 0.65f);
                    Color crack = Darken(c, 0.45f);
                    Color rivet = Lighten(accent, 0.20f);
                    Color rivetHi = Lighten(accent, 0.40f);
                    Color plate = Darken(c, 0.90f);
                    SetRect(px, s, 0, 0, 64, 64, c);
                    // Small crack
                    Px(px, s, 30, 28, crack); Px(px, s, 31, 27, crack); Px(px, s, 32, 26, crack); Px(px, s, 33, 25, crack);
                    // Cross-hatch plate pattern
                    SetRect(px, s, 31, 0, 2, 64, plate);
                    SetRect(px, s, 0, 31, 64, 2, plate);
                    // 8 corner/edge rivets (3x3 with highlight)
                    int[] rx = {6, 54, 6, 54, 28, 34, 28, 34};
                    int[] ry = {6, 6, 54, 54, 6, 6, 54, 54};
                    for (int i = 0; i < 8; i++)
                    {
                        SetRect(px, s, rx[i], ry[i], 3, 3, rivet);
                        Px(px, s, rx[i] + 1, ry[i] + 2, rivetHi);
                    }
                    // Border bevel
                    SetRect(px, s, 0, 62, 64, 2, Lighten(c, 0.15f));
                    SetRect(px, s, 0, 0, 64, 2, Darken(c, 0.75f));
                    break;
                }
                case (byte)TileType.Indestructible:
                {
                    // Indestructible uses darkest possible with metallic sheen
                    Color c = Darken(block, 0.40f);
                    Color hi = Lighten(c, 0.25f);
                    Color hiB = Lighten(c, 0.35f);
                    Color dark = Darken(c, 0.65f);
                    Color darkB = Darken(c, 0.50f);
                    Color rivet = Lighten(accent, 0.30f);
                    SetRect(px, s, 0, 0, 64, 64, c);
                    // Metallic gradient: highlight bottom-left corner, shadow top-right
                    SetRect(px, s, 0, 0, 20, 20, hi);
                    SetRect(px, s, 0, 0, 10, 10, hiB);
                    SetRect(px, s, 0, 0, 4, 4, Lighten(c, 0.45f));
                    SetRect(px, s, 44, 44, 20, 20, dark);
                    SetRect(px, s, 54, 54, 10, 10, darkB);
                    SetRect(px, s, 60, 60, 4, 4, Darken(c, 0.40f));
                    // Diagonal blend
                    for (int i = 0; i < 12; i++) { Px(px, s, 20 + i, 20 - i, hi); Px(px, s, 44 + i, 44 - i, dark); }
                    // Panel lines
                    SetRect(px, s, 0, 31, 64, 1, dark);
                    SetRect(px, s, 31, 0, 1, 64, dark);
                    // Rivets (3x3 with highlight pip)
                    SetRect(px, s, 14, 14, 3, 3, rivet); Px(px, s, 15, 16, hiB);
                    SetRect(px, s, 46, 14, 3, 3, rivet); Px(px, s, 47, 16, hiB);
                    SetRect(px, s, 14, 46, 3, 3, rivet); Px(px, s, 15, 48, hiB);
                    SetRect(px, s, 46, 46, 3, 3, rivet); Px(px, s, 47, 48, hiB);
                    break;
                }
            }
        }

        /// <summary>
        /// Paint epoch-specific ground texture (pebbles, cracks, patterns).
        /// </summary>
        private static void PaintGroundTexture(Color[] px, int s, int epoch, Color d1, Color d2, Color d3, Color l1, Color l2)
        {
            switch (epoch)
            {
                case 0: // Stone Age - pebbles and worm holes
                    SetRect(px, s, 10, 18, 3, 2, d1); SetRect(px, s, 13, 19, 2, 2, d2);
                    SetRect(px, s, 40, 30, 3, 2, d1); SetRect(px, s, 43, 31, 2, 2, d2);
                    SetRect(px, s, 26, 12, 3, 2, d1); SetRect(px, s, 52, 42, 3, 2, d1);
                    Px(px, s, 22, 28, d1); Px(px, s, 23, 28, d1); Px(px, s, 48, 14, d1);
                    Px(px, s, 6, 38, l1); Px(px, s, 7, 39, l1); Px(px, s, 36, 52, l2);
                    break;
                case 1: // Bronze Age - sandy, wind-blown texture
                    for (int i = 0; i < 8; i++) SetRect(px, s, 4 + i * 8, 10 + (i % 3) * 4, 4, 2, d2);
                    for (int i = 0; i < 6; i++) Px(px, s, 8 + i * 10, 40 + (i % 2) * 6, d1);
                    SetRect(px, s, 20, 52, 6, 3, d3); SetRect(px, s, 44, 24, 5, 2, d3);
                    break;
                case 2: // Classical - marble veins
                    for (int i = 0; i < 10; i++) { Px(px, s, 8 + i * 2, 20 + i, l1); Px(px, s, 9 + i * 2, 21 + i, l2); }
                    for (int i = 0; i < 8; i++) { Px(px, s, 40 + i, 44 - i, l1); Px(px, s, 41 + i, 45 - i, l2); }
                    SetRect(px, s, 28, 8, 2, 8, l1); SetRect(px, s, 52, 36, 2, 10, l1);
                    break;
                case 3: // Medieval - cobblestone joints
                    SetRect(px, s, 0, 15, 64, 2, d1); SetRect(px, s, 0, 31, 64, 2, d1); SetRect(px, s, 0, 47, 64, 2, d1);
                    SetRect(px, s, 16, 0, 2, 64, d1); SetRect(px, s, 32, 0, 2, 64, d1); SetRect(px, s, 48, 0, 2, 64, d1);
                    break;
                case 4: // Renaissance - ornate brick pattern
                    SetRect(px, s, 0, 15, 64, 1, d1); SetRect(px, s, 0, 31, 64, 1, d1); SetRect(px, s, 0, 47, 64, 1, d1);
                    SetRect(px, s, 20, 0, 1, 15, d1); SetRect(px, s, 44, 0, 1, 15, d1);
                    SetRect(px, s, 8, 16, 1, 15, d1); SetRect(px, s, 32, 16, 1, 15, d1); SetRect(px, s, 56, 16, 1, 15, d1);
                    break;
                case 5: // Industrial - riveted steel plates
                    SetRect(px, s, 0, 31, 64, 2, d1); SetRect(px, s, 31, 0, 2, 64, d1);
                    SetRect(px, s, 6, 6, 4, 4, l1); SetRect(px, s, 54, 6, 4, 4, l1);
                    SetRect(px, s, 6, 54, 4, 4, l1); SetRect(px, s, 54, 54, 4, 4, l1);
                    SetRect(px, s, 28, 28, 8, 8, l2); // Center bolt
                    break;
                case 6: // Modern - concrete texture
                    for (int i = 0; i < 12; i++) { Px(px, s, 5 + i * 5, 12 + (i % 4) * 3, d2); Px(px, s, 8 + i * 5, 44 + (i % 3) * 2, d2); }
                    SetRect(px, s, 0, 30, 64, 4, d1); // Expansion joint
                    break;
                case 7: // Digital - circuit traces
                    SetRect(px, s, 8, 8, 2, 48, l2); SetRect(px, s, 24, 8, 2, 48, l2);
                    SetRect(px, s, 40, 8, 2, 48, l2); SetRect(px, s, 56, 8, 2, 48, l2);
                    SetRect(px, s, 8, 24, 50, 2, l2); SetRect(px, s, 8, 40, 50, 2, l2);
                    Px(px, s, 8, 24, l1); Px(px, s, 24, 24, l1); Px(px, s, 40, 24, l1); Px(px, s, 56, 24, l1);
                    Px(px, s, 8, 40, l1); Px(px, s, 24, 40, l1); Px(px, s, 40, 40, l1); Px(px, s, 56, 40, l1);
                    break;
                case 8: // Spacefaring - hull plating
                    SetRect(px, s, 0, 20, 64, 3, d1); SetRect(px, s, 0, 43, 64, 3, d1);
                    SetRect(px, s, 20, 0, 3, 64, d1); SetRect(px, s, 43, 0, 3, 64, d1);
                    SetRect(px, s, 4, 4, 3, 3, l1); SetRect(px, s, 57, 4, 3, 3, l1);
                    SetRect(px, s, 4, 57, 3, 3, l1); SetRect(px, s, 57, 57, 3, 3, l1);
                    break;
                case 9: // Transcendent - crystalline fractures
                    for (int i = 0; i < 6; i++) { Px(px, s, 10 + i * 3, 10 + i * 5, l2); Px(px, s, 11 + i * 3, 11 + i * 5, l1); }
                    for (int i = 0; i < 6; i++) { Px(px, s, 50 - i * 3, 20 + i * 4, l2); Px(px, s, 51 - i * 3, 21 + i * 4, l1); }
                    SetRect(px, s, 28, 28, 8, 8, l1); // Core glow
                    break;
            }
        }

        /// <summary>
        /// Paint epoch-specific surface texture (grass, sand, marble, etc.).
        /// </summary>
        private static void PaintSurfaceTexture(Color[] px, int s, int epoch, Color surf, Color surfLight, Color surfDark)
        {
            switch (epoch)
            {
                case 0: // Stone Age - grass blades
                    int[] bladeX = {2,3,7,8,12,13,17,21,22,26,27,31,32,36,37,41,42,46,47,51,52,56,57,61,62};
                    foreach (int bx in bladeX)
                    {
                        Px(px, s, bx, 63, surfLight);
                        if (bx % 5 < 2) { Px(px, s, bx, 62, surfLight); Px(px, s, bx, 61, surfLight); }
                        else if (bx % 3 == 0) Px(px, s, bx, 62, surfLight);
                    }
                    Px(px, s, 5, 63, surfLight); Px(px, s, 5, 62, surfLight); Px(px, s, 5, 61, surfLight); Px(px, s, 5, 60, surfLight);
                    Px(px, s, 35, 63, surfLight); Px(px, s, 35, 62, surfLight); Px(px, s, 35, 61, surfLight); Px(px, s, 35, 60, surfLight);
                    break;
                case 1: // Bronze Age - sandy ripples
                    for (int i = 0; i < 8; i++) SetRect(px, s, i * 8, 56 + (i % 2) * 2, 6, 4, surfLight);
                    for (int i = 0; i < 6; i++) Px(px, s, 10 + i * 10, 62, surfDark);
                    break;
                case 2: // Classical - marble polish
                    SetRect(px, s, 0, 58, 64, 6, surfLight);
                    for (int i = 0; i < 4; i++) SetRect(px, s, 8 + i * 16, 60, 8, 2, Lighten(surfLight, 0.2f));
                    break;
                case 3: // Medieval - cobblestone tops
                    SetRect(px, s, 2, 54, 12, 8, surfLight); SetRect(px, s, 18, 54, 12, 8, surfLight);
                    SetRect(px, s, 34, 54, 12, 8, surfLight); SetRect(px, s, 50, 54, 12, 8, surfLight);
                    SetRect(px, s, 0, 52, 64, 2, surfDark);
                    break;
                case 4: // Renaissance - ornate tiles
                    SetRect(px, s, 0, 58, 64, 6, surfLight);
                    SetRect(px, s, 8, 56, 48, 2, Darken(surfLight, 0.9f));
                    for (int i = 0; i < 8; i++) Px(px, s, 4 + i * 8, 62, surfDark);
                    break;
                case 5: // Industrial - grating
                    for (int i = 0; i < 16; i++) { SetRect(px, s, i * 4, 48, 2, 16, surfDark); }
                    SetRect(px, s, 0, 62, 64, 2, surfLight);
                    break;
                case 6: // Modern - road markings
                    SetRect(px, s, 0, 58, 64, 6, surfLight);
                    SetRect(px, s, 16, 56, 8, 4, new Color(0.9f, 0.7f, 0.1f)); // Yellow line
                    SetRect(px, s, 40, 56, 8, 4, new Color(0.9f, 0.7f, 0.1f));
                    break;
                case 7: // Digital - neon grid lines
                    var neon = new Color(0f, 1f, 0.8f);
                    for (int i = 0; i < 8; i++) { Px(px, s, i * 8 + 4, 63, neon); Px(px, s, i * 8 + 4, 62, neon); }
                    SetRect(px, s, 0, 60, 64, 1, Darken(neon, 0.7f));
                    break;
                case 8: // Spacefaring - glowing edge
                    var plasma = new Color(0.6f, 0f, 1f);
                    SetRect(px, s, 0, 60, 64, 4, surfLight);
                    SetRect(px, s, 0, 62, 64, 2, plasma);
                    for (int i = 0; i < 8; i++) Px(px, s, 4 + i * 8, 63, Lighten(plasma, 0.3f));
                    break;
                case 9: // Transcendent - ethereal glow
                    SetRect(px, s, 0, 54, 64, 10, surfLight);
                    SetRect(px, s, 0, 60, 64, 4, Lighten(surfLight, 0.3f));
                    for (int i = 0; i < 16; i++) Px(px, s, 2 + i * 4, 63, new Color(1f, 1f, 1f, 0.8f));
                    break;
            }
        }

        #endregion

        #region Player

        public static Sprite GetPlayerSprite()
        {
            string key = "player";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            int size = 192;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];

            Color armor    = new Color(0.20f, 0.40f, 0.90f);
            Color armorLt  = Lighten(armor, 0.20f);
            Color armorDk  = Darken(armor, 0.70f);
            Color armorMid = Darken(armor, 0.85f);
            Color armorHi  = Lighten(armor, 0.35f);
            Color visor    = new Color(0.40f, 0.75f, 1.00f);
            Color visorHi  = Lighten(visor, 0.35f);
            Color visorDk  = Darken(visor, 0.70f);
            Color skin     = new Color(0.90f, 0.75f, 0.60f);
            Color skinDk   = Darken(skin, 0.75f);
            Color skinLt   = Lighten(skin, 0.20f);
            Color boot     = new Color(0.15f, 0.25f, 0.55f);
            Color bootDk   = Darken(boot, 0.65f);
            Color bootLt   = Lighten(boot, 0.15f);
            Color belt     = new Color(0.65f, 0.55f, 0.15f);
            Color beltHi   = Lighten(belt, 0.30f);
            Color beltDk   = Darken(belt, 0.65f);

            // === Boots (y=0..30) ===
            // Left boot
            SetRect(px, size, 48, 0, 40, 30, boot);
            SetRect(px, size, 48, 0, 40, 8, bootDk);   // soles
            SetRect(px, size, 48, 0, 6, 30, bootDk);    // left shadow
            SetRect(px, size, 82, 0, 6, 30, bootLt);    // right highlight
            SetRect(px, size, 56, 22, 24, 4, armorDk);  // boot strap
            SetRect(px, size, 58, 23, 20, 2, armorMid);
            // Tread detail
            for (int i = 0; i < 6; i++) SetRect(px, size, 52 + i * 6, 2, 3, 2, bootDk);
            // Lace eyelets
            Px(px, size, 66, 16, armorLt); Px(px, size, 66, 12, armorLt);
            // Right boot
            SetRect(px, size, 104, 0, 40, 30, boot);
            SetRect(px, size, 104, 0, 40, 8, bootDk);
            SetRect(px, size, 104, 0, 6, 30, bootLt);
            SetRect(px, size, 138, 0, 6, 30, bootDk);
            SetRect(px, size, 112, 22, 24, 4, armorDk);
            SetRect(px, size, 114, 23, 20, 2, armorMid);
            for (int i = 0; i < 6; i++) SetRect(px, size, 108 + i * 6, 2, 3, 2, bootDk);
            Px(px, size, 122, 16, armorLt); Px(px, size, 122, 12, armorLt);

            // === Legs (y=30..78) ===
            // Left leg
            SetRect(px, size, 56, 30, 28, 48, armor);
            SetRect(px, size, 56, 30, 4, 48, armorLt);
            SetRect(px, size, 80, 30, 4, 48, armorDk);
            // Knee guard
            SetRect(px, size, 52, 58, 36, 8, armorDk);
            SetRect(px, size, 54, 64, 32, 4, armorMid);
            SetRect(px, size, 58, 66, 24, 2, armorHi);
            // Knee rivet
            Px(px, size, 70, 62, armorHi); Px(px, size, 71, 62, armorHi);
            // Right leg
            SetRect(px, size, 108, 30, 28, 48, armor);
            SetRect(px, size, 108, 30, 4, 48, armorLt);
            SetRect(px, size, 132, 30, 4, 48, armorDk);
            SetRect(px, size, 104, 58, 36, 8, armorDk);
            SetRect(px, size, 106, 64, 32, 4, armorMid);
            SetRect(px, size, 110, 66, 24, 2, armorHi);
            Px(px, size, 122, 62, armorHi); Px(px, size, 123, 62, armorHi);

            // === Belt (y=78..94) ===
            SetRect(px, size, 46, 78, 100, 16, belt);
            SetRect(px, size, 46, 90, 100, 4, beltHi);
            SetRect(px, size, 46, 78, 100, 4, beltDk);
            // Buckle
            SetRect(px, size, 82, 82, 28, 8, visor);
            SetRect(px, size, 86, 84, 20, 4, visorHi);
            // Belt pouches (left and right)
            SetRect(px, size, 54, 82, 14, 8, beltDk);
            SetRect(px, size, 56, 84, 10, 4, Darken(belt, 0.50f));
            Px(px, size, 60, 83, beltHi); // pouch button
            SetRect(px, size, 124, 82, 14, 8, beltDk);
            SetRect(px, size, 126, 84, 10, 4, Darken(belt, 0.50f));
            Px(px, size, 130, 83, beltHi);

            // === Torso (y=94..148) ===
            SetRect(px, size, 50, 94, 92, 54, armor);
            SetRect(px, size, 50, 94, 4, 54, armorLt);
            SetRect(px, size, 138, 94, 4, 54, armorDk);
            // Chest plate center division
            SetRect(px, size, 92, 100, 8, 36, armorDk);
            // Horizontal plate lines
            SetRect(px, size, 50, 118, 92, 3, armorMid);
            SetRect(px, size, 50, 134, 92, 3, armorMid);
            // Chest emblem (diamond)
            SetRect(px, size, 90, 126, 12, 10, visor);
            Px(px, size, 95, 130, visorHi); Px(px, size, 96, 130, visorHi);
            SetRect(px, size, 86, 122, 6, 6, visor);
            SetRect(px, size, 100, 122, 6, 6, visor);
            SetRect(px, size, 92, 118, 8, 4, visor);
            SetRect(px, size, 92, 136, 8, 4, visor);
            // Plate rivets
            Px(px, size, 62, 106, armorHi); Px(px, size, 128, 106, armorHi);
            Px(px, size, 62, 138, armorHi); Px(px, size, 128, 138, armorHi);
            Px(px, size, 62, 122, armorHi); Px(px, size, 128, 122, armorHi);

            // === Arms (y=86..136) ===
            // Left arm
            SetRect(px, size, 30, 86, 20, 50, armor);
            SetRect(px, size, 30, 86, 4, 50, armorLt);
            SetRect(px, size, 46, 86, 4, 50, armorDk);
            // Shoulder pad
            SetRect(px, size, 26, 126, 28, 14, armorDk);
            SetRect(px, size, 30, 132, 20, 4, armorMid);
            SetRect(px, size, 34, 134, 12, 2, armorHi);
            // Elbow joint
            SetRect(px, size, 30, 106, 20, 4, armorMid);
            // Gauntlet / Hand
            SetRect(px, size, 30, 86, 20, 12, skin);
            SetRect(px, size, 30, 86, 20, 4, skinDk);   // fingers
            SetRect(px, size, 32, 88, 4, 6, skinLt);     // knuckle highlight
            // Right arm
            SetRect(px, size, 142, 86, 20, 50, armor);
            SetRect(px, size, 142, 86, 4, 50, armorLt);
            SetRect(px, size, 158, 86, 4, 50, armorDk);
            SetRect(px, size, 138, 126, 28, 14, armorDk);
            SetRect(px, size, 142, 132, 20, 4, armorMid);
            SetRect(px, size, 146, 134, 12, 2, armorHi);
            SetRect(px, size, 142, 106, 20, 4, armorMid);
            SetRect(px, size, 142, 86, 20, 12, skin);
            SetRect(px, size, 142, 86, 20, 4, skinDk);
            SetRect(px, size, 156, 88, 4, 6, skinLt);

            // === Chin / Neck (y=148..158) ===
            SetRect(px, size, 74, 148, 44, 10, skin);
            SetRect(px, size, 78, 148, 36, 4, skinDk);  // chin shadow
            SetRect(px, size, 82, 150, 28, 2, skinLt);   // chin highlight

            // === Helmet (y=158..192) ===
            SetRect(px, size, 62, 158, 68, 34, armor);
            SetRect(px, size, 62, 158, 4, 34, armorLt);
            SetRect(px, size, 126, 158, 4, 34, armorDk);
            // Chin guard
            SetRect(px, size, 66, 158, 60, 8, armorDk);
            SetRect(px, size, 68, 160, 56, 4, armorMid);
            // Visor slit (y=166..174)
            SetRect(px, size, 70, 166, 52, 8, visor);
            SetRect(px, size, 74, 168, 44, 4, visorHi);
            // Visor reflection
            SetRect(px, size, 76, 169, 8, 2, Lighten(visor, 0.50f));
            // Nose guard
            SetRect(px, size, 94, 160, 4, 6, armorDk);
            // Helmet ridge lines
            SetRect(px, size, 62, 178, 68, 3, armorMid);
            // Crest on top
            SetRect(px, size, 82, 186, 28, 6, visor);
            SetRect(px, size, 86, 188, 20, 4, armorLt);
            SetRect(px, size, 90, 190, 12, 2, visorHi);
            // Ear guards
            SetRect(px, size, 62, 170, 4, 8, armorDk);
            SetRect(px, size, 126, 170, 4, 8, armorDk);

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0f), 128f);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        #endregion

        #region Enemies

        public static Sprite GetEnemySprite(EnemyType type, EnemyBehavior behavior)
        {
            int era = (int)type / 3;
            string key = $"enemy_{era}_{(int)behavior}";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            int size = 192;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];
            var (body, secondary, eyes) = GetEraColors(era);

            switch (behavior)
            {
                case EnemyBehavior.Patrol:     PaintPatrolEnemy(px, size, body, secondary, eyes, era); break;
                case EnemyBehavior.Chase:      PaintChaseEnemy(px, size, body, secondary, eyes, era); break;
                case EnemyBehavior.Stationary: PaintStationaryEnemy(px, size, body, secondary, eyes, era); break;
                case EnemyBehavior.Flying:     PaintFlyingEnemy(px, size, body, secondary, eyes, era); break;
            }

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0f), 128f);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        private static void PaintPatrolEnemy(Color[] px, int s, Color body, Color sec, Color eyes, int era)
        {
            Color dark = Darken(body, 0.60f);
            Color light = Lighten(sec, 0.20f);
            Color mid = Darken(body, 0.80f);
            Color secDk = Darken(sec, 0.65f);

            // === Boots (y=0..16) ===
            SetRect(px, s, 56, 0, 24, 16, dark);
            SetRect(px, s, 56, 0, 24, 4, Darken(dark, 0.70f));
            for (int i = 0; i < 4; i++) SetRect(px, s, 60 + i * 5, 2, 2, 2, Darken(dark, 0.60f));
            SetRect(px, s, 112, 0, 24, 16, dark);
            SetRect(px, s, 112, 0, 24, 4, Darken(dark, 0.70f));
            for (int i = 0; i < 4; i++) SetRect(px, s, 116 + i * 5, 2, 2, 2, Darken(dark, 0.60f));

            // === Legs (y=16..80) ===
            SetRect(px, s, 58, 16, 24, 64, sec);
            SetRect(px, s, 58, 16, 4, 64, light);
            SetRect(px, s, 78, 16, 4, 64, secDk);
            SetRect(px, s, 114, 16, 24, 64, sec);
            SetRect(px, s, 114, 16, 4, 64, light);
            SetRect(px, s, 134, 16, 4, 64, secDk);
            // Knee plates
            SetRect(px, s, 54, 56, 32, 8, dark);
            SetRect(px, s, 56, 62, 28, 3, mid);
            SetRect(px, s, 110, 56, 32, 8, dark);
            SetRect(px, s, 112, 62, 28, 3, mid);

            // === Belt ===
            SetRect(px, s, 52, 76, 88, 10, dark);
            SetRect(px, s, 52, 84, 88, 2, mid);

            // === Torso (y=86..150) ===
            SetRect(px, s, 52, 86, 88, 64, body);
            SetRect(px, s, 52, 86, 4, 64, Lighten(body, 0.15f));
            SetRect(px, s, 136, 86, 4, 64, dark);
            // Plate lines
            SetRect(px, s, 52, 108, 88, 3, dark);
            SetRect(px, s, 52, 130, 88, 3, dark);
            // Center seam
            SetRect(px, s, 92, 90, 8, 56, mid);

            // === Arms ===
            // Left arm + shield
            SetRect(px, s, 30, 86, 22, 50, body);
            SetRect(px, s, 30, 86, 4, 50, Lighten(body, 0.15f));
            SetRect(px, s, 30, 106, 22, 18, sec);   // shield
            SetRect(px, s, 32, 108, 18, 14, secDk);  // shield inner
            SetRect(px, s, 36, 112, 10, 6, eyes);    // shield emblem
            // Right arm + weapon
            SetRect(px, s, 140, 86, 22, 50, body);
            SetRect(px, s, 158, 86, 4, 50, dark);
            SetRect(px, s, 152, 86, 10, 34, sec);    // weapon blade
            Px(px, s, 157, 120, eyes); Px(px, s, 158, 120, eyes); // blade tip glow
            SetRect(px, s, 154, 118, 6, 4, light);

            // === Head (y=150..192) ===
            SetRect(px, s, 66, 150, 60, 42, sec);
            SetRect(px, s, 66, 150, 60, 6, dark);      // chin guard
            SetRect(px, s, 68, 152, 56, 2, mid);
            SetRect(px, s, 66, 184, 60, 8, light);      // helmet ridge
            SetRect(px, s, 70, 188, 52, 4, Lighten(light, 0.15f));
            // Eyes
            SetRect(px, s, 78, 166, 14, 8, eyes);
            SetRect(px, s, 104, 166, 14, 8, eyes);
            SetRect(px, s, 80, 168, 4, 4, dark);  // pupils
            SetRect(px, s, 110, 168, 4, 4, dark);
            // Helmet crest
            SetRect(px, s, 88, 190, 16, 2, light);
            Px(px, s, 95, 191, eyes); Px(px, s, 96, 191, eyes);
            // Nose guard
            SetRect(px, s, 92, 156, 8, 10, dark);

            // === Epoch-specific details ===
            AddPatrolEpochDetails(px, s, body, sec, eyes, dark, light, era);
        }

        private static void AddPatrolEpochDetails(Color[] px, int s, Color body, Color sec, Color eyes, Color dark, Color light, int era)
        {
            switch (era)
            {
                case 0: // Stone Age - fur trim, bone necklace
                    Color fur = new Color(0.60f, 0.50f, 0.35f);
                    SetRect(px, s, 52, 140, 88, 6, fur); // fur collar
                    for (int i = 0; i < 5; i++) Px(px, s, 60 + i * 14, 144, Darken(fur, 0.70f));
                    // Bone necklace
                    Color bone = new Color(0.95f, 0.92f, 0.85f);
                    for (int i = 0; i < 4; i++) { SetRect(px, s, 72 + i * 12, 148, 4, 2, bone); }
                    break;

                case 1: // Bronze Age - sun disk emblem, bronze bands
                    Color bronze = new Color(0.80f, 0.55f, 0.20f);
                    SetRect(px, s, 88, 115, 16, 16, bronze); // sun disk
                    SetRect(px, s, 92, 119, 8, 8, new Color(1f, 0.85f, 0.30f)); // sun center
                    // Bronze arm bands
                    SetRect(px, s, 30, 130, 22, 3, bronze);
                    SetRect(px, s, 140, 130, 22, 3, bronze);
                    break;

                case 2: // Classical - laurel wreath, toga drape
                    Color laurel = new Color(0.40f, 0.65f, 0.30f);
                    // Laurel wreath on helmet
                    for (int i = 0; i < 6; i++) { Px(px, s, 72 + i * 8, 186, laurel); Px(px, s, 73 + i * 8, 187, laurel); }
                    // Red cape edge
                    Color capeRed = new Color(0.75f, 0.15f, 0.15f);
                    SetRect(px, s, 48, 140, 4, 10, capeRed);
                    SetRect(px, s, 140, 140, 4, 10, capeRed);
                    break;

                case 3: // Medieval - cross emblem, chainmail glimpse
                    Color silver = new Color(0.75f, 0.75f, 0.80f);
                    // Cross on chest
                    SetRect(px, s, 92, 108, 8, 20, silver);
                    SetRect(px, s, 84, 114, 24, 6, silver);
                    // Chainmail at neck
                    for (int i = 0; i < 8; i++)
                        for (int j = 0; j < 2; j++)
                            Px(px, s, 72 + i * 6, 147 + j * 3, Darken(silver, 0.80f));
                    break;

                case 4: // Renaissance - feathered plume, ornate trim
                    Color feather = new Color(0.90f, 0.20f, 0.40f);
                    Color gold = new Color(0.95f, 0.80f, 0.20f);
                    // Feather plume
                    for (int i = 0; i < 8; i++) SetRect(px, s, 98 + i, 188 + i/2, 2, 4 - i/3, feather);
                    // Gold trim on armor
                    SetRect(px, s, 52, 86, 88, 2, gold);
                    SetRect(px, s, 52, 148, 88, 2, gold);
                    break;

                case 5: // Industrial - goggles, brass fittings
                    Color brass = new Color(0.75f, 0.60f, 0.25f);
                    Color lens = new Color(0.40f, 0.30f, 0.20f);
                    // Goggles on forehead
                    SetRect(px, s, 74, 180, 18, 6, brass);
                    SetRect(px, s, 100, 180, 18, 6, brass);
                    SetRect(px, s, 78, 182, 10, 2, lens);
                    SetRect(px, s, 104, 182, 10, 2, lens);
                    // Gear on belt
                    SetRect(px, s, 90, 76, 12, 10, brass);
                    break;

                case 6: // Modern - tactical vest, camo pattern
                    Color camo1 = new Color(0.35f, 0.40f, 0.25f);
                    Color camo2 = new Color(0.25f, 0.30f, 0.20f);
                    // Camo patches
                    for (int i = 0; i < 6; i++)
                    {
                        SetRect(px, s, 56 + i * 12 + (i%2)*4, 90 + (i%3)*15, 8, 6, camo1);
                        SetRect(px, s, 60 + i * 11, 100 + (i%2)*20, 6, 5, camo2);
                    }
                    // Tactical belt pouches
                    SetRect(px, s, 56, 76, 10, 10, camo2);
                    SetRect(px, s, 126, 76, 10, 10, camo2);
                    break;

                case 7: // Digital - circuit lines, LED eyes
                    Color circuit = new Color(0.00f, 0.90f, 0.70f);
                    // Circuit trace lines
                    SetRect(px, s, 70, 100, 2, 40, circuit);
                    SetRect(px, s, 70, 140, 20, 2, circuit);
                    SetRect(px, s, 120, 100, 2, 40, circuit);
                    SetRect(px, s, 102, 140, 20, 2, circuit);
                    // Glowing LED accents
                    Px(px, s, 70, 100, Lighten(circuit, 0.50f));
                    Px(px, s, 120, 100, Lighten(circuit, 0.50f));
                    break;

                case 8: // Spacefaring - visor, alien glow
                    Color visor = new Color(0.20f, 0.10f, 0.40f);
                    Color glow = new Color(0.60f, 0.00f, 1.00f);
                    // Full visor
                    SetRect(px, s, 72, 160, 48, 20, visor);
                    SetRect(px, s, 76, 164, 40, 12, Lighten(visor, 0.30f));
                    // Glow strips
                    SetRect(px, s, 52, 106, 2, 40, glow);
                    SetRect(px, s, 138, 106, 2, 40, glow);
                    break;

                case 9: // Transcendent - ethereal aura, crystal armor
                    Color aura = new Color(1f, 1f, 1f, 0.80f);
                    Color crystal = new Color(0.90f, 0.90f, 1.00f);
                    // Crystal shoulder pads
                    SetRect(px, s, 44, 136, 8, 8, crystal);
                    SetRect(px, s, 140, 136, 8, 8, crystal);
                    Px(px, s, 48, 142, aura); Px(px, s, 144, 142, aura);
                    // Glowing crown
                    for (int i = 0; i < 5; i++) Px(px, s, 84 + i * 6, 190, aura);
                    // Ethereal chest glow
                    SetRect(px, s, 90, 116, 12, 12, Lighten(crystal, 0.30f));
                    break;
            }
        }

        private static void PaintChaseEnemy(Color[] px, int s, Color body, Color sec, Color eyes, int era)
        {
            Color dark = Darken(body, 0.55f);
            Color spike = Lighten(body, 0.30f);
            Color tooth = Lighten(eyes, 0.35f);
            Color belly = Darken(body, 0.75f);

            // === Four legs ===
            int[] legX = {30, 70, 110, 150};
            foreach (int lx in legX)
            {
                SetRect(px, s, lx, 0, 18, 42, sec);
                SetRect(px, s, lx, 0, 4, 42, Lighten(sec, 0.10f));
                SetRect(px, s, lx + 14, 0, 4, 42, Darken(sec, 0.70f));
                // Claws
                SetRect(px, s, lx, 0, 4, 4, dark);
                SetRect(px, s, lx + 14, 0, 4, 4, dark);
                Px(px, s, lx + 1, 0, spike); Px(px, s, lx + 15, 0, spike);
                // Muscle definition
                SetRect(px, s, lx + 6, 20, 6, 3, Darken(sec, 0.85f));
            }

            // === Wide body ===
            SetRect(px, s, 18, 42, 156, 76, body);
            SetRect(px, s, 18, 42, 156, 14, belly);  // belly
            SetRect(px, s, 18, 42, 4, 76, Lighten(body, 0.12f));
            SetRect(px, s, 170, 42, 4, 76, dark);
            // Muscle ridges
            SetRect(px, s, 54, 72, 4, 30, Darken(body, 0.85f));
            SetRect(px, s, 100, 72, 4, 30, Darken(body, 0.85f));
            SetRect(px, s, 130, 72, 4, 30, Darken(body, 0.85f));
            // Scale texture
            for (int i = 0; i < 10; i++) Px(px, s, 34 + i * 14, 86, Darken(body, 0.80f));
            for (int i = 0; i < 8; i++) Px(px, s, 40 + i * 14, 76, Darken(body, 0.80f));

            // === Head ===
            SetRect(px, s, 126, 86, 58, 64, sec);
            SetRect(px, s, 132, 86, 52, 18, dark);  // jaw
            SetRect(px, s, 126, 86, 58, 4, Darken(sec, 0.70f));
            // Teeth (6 teeth with gaps)
            for (int i = 0; i < 6; i++) SetRect(px, s, 140 + i * 7, 104, 4, 6, tooth);
            // Eyes
            SetRect(px, s, 150, 130, 12, 12, eyes);
            SetRect(px, s, 170, 130, 12, 12, eyes);
            SetRect(px, s, 154, 134, 4, 4, dark);  // pupils
            SetRect(px, s, 174, 134, 4, 4, dark);
            // Brow ridge
            SetRect(px, s, 148, 142, 36, 4, Darken(sec, 0.70f));

            // === Back spikes ===
            for (int i = 0; i < 5; i++)
            {
                int sx = 36 + i * 24;
                SetRect(px, s, sx, 118, 6, 8, spike);
                SetRect(px, s, sx + 1, 126, 4, 6, spike);
                SetRect(px, s, sx + 2, 132, 2, 4, spike);
                Px(px, s, sx + 2, 136, Lighten(spike, 0.2f));
            }

            // === Tail ===
            SetRect(px, s, 18, 80, 20, 12, dark);
            SetRect(px, s, 10, 72, 12, 10, dark);
            SetRect(px, s, 6, 66, 8, 8, Darken(dark, 0.80f));
            Px(px, s, 4, 64, dark); Px(px, s, 5, 64, dark);

            // === Epoch-specific details ===
            AddChaseEpochDetails(px, s, body, sec, eyes, dark, spike, era);
        }

        private static void AddChaseEpochDetails(Color[] px, int s, Color body, Color sec, Color eyes, Color dark, Color spike, int era)
        {
            switch (era)
            {
                case 0: // Stone Age - shaggy fur, bone collar
                    Color fur = new Color(0.55f, 0.45f, 0.30f);
                    // Shaggy fur mane
                    for (int i = 0; i < 12; i++)
                    {
                        int fx = 30 + i * 12;
                        SetRect(px, s, fx, 114 + (i % 3) * 2, 6, 8 - (i % 2) * 2, fur);
                    }
                    // Bone collar
                    Color bone = new Color(0.95f, 0.90f, 0.80f);
                    SetRect(px, s, 130, 144, 50, 4, bone);
                    break;

                case 1: // Bronze Age - bronze plating on head
                    Color bronze = new Color(0.80f, 0.55f, 0.20f);
                    SetRect(px, s, 130, 140, 50, 6, bronze);
                    SetRect(px, s, 148, 146, 24, 4, Darken(bronze, 0.70f));
                    break;

                case 2: // Classical - war paint stripes
                    Color redPaint = new Color(0.80f, 0.15f, 0.10f);
                    for (int i = 0; i < 4; i++)
                        SetRect(px, s, 40 + i * 30, 70, 3, 40, redPaint);
                    break;

                case 3: // Medieval - spiked collar, chain
                    Color iron = new Color(0.50f, 0.50f, 0.55f);
                    SetRect(px, s, 126, 140, 60, 5, iron);
                    for (int i = 0; i < 5; i++)
                        SetRect(px, s, 130 + i * 11, 145, 4, 6, Lighten(iron, 0.30f));
                    break;

                case 4: // Renaissance - decorative saddle marks
                    Color gold = new Color(0.90f, 0.75f, 0.20f);
                    SetRect(px, s, 60, 100, 60, 3, gold);
                    SetRect(px, s, 60, 110, 60, 3, gold);
                    // Ornate tail tip
                    SetRect(px, s, 4, 62, 6, 4, gold);
                    break;

                case 5: // Industrial - mechanical leg braces
                    Color brass = new Color(0.70f, 0.55f, 0.25f);
                    int[] blegX = {32, 72, 112, 152};
                    foreach (int lx in blegX)
                        SetRect(px, s, lx, 30, 14, 4, brass);
                    // Steam vents on back
                    SetRect(px, s, 80, 118, 4, 10, Darken(brass, 0.60f));
                    break;

                case 6: // Modern - tactical harness
                    Color tactical = new Color(0.30f, 0.32f, 0.25f);
                    SetRect(px, s, 50, 90, 80, 4, tactical);
                    SetRect(px, s, 86, 56, 4, 38, tactical);
                    // Red laser sight dot
                    SetRect(px, s, 178, 134, 4, 4, new Color(1f, 0f, 0f));
                    break;

                case 7: // Digital - circuit patterns, data lines
                    Color circuit = new Color(0.00f, 0.95f, 0.75f);
                    SetRect(px, s, 40, 80, 2, 30, circuit);
                    SetRect(px, s, 40, 110, 30, 2, circuit);
                    SetRect(px, s, 140, 80, 2, 30, circuit);
                    SetRect(px, s, 112, 110, 30, 2, circuit);
                    // Glowing eye upgrade
                    SetRect(px, s, 148, 128, 16, 16, Lighten(circuit, 0.30f));
                    break;

                case 8: // Spacefaring - alien markings, bioluminescent
                    Color alienGlow = new Color(0.50f, 0.00f, 0.90f);
                    // Bioluminescent spots
                    for (int i = 0; i < 6; i++)
                    {
                        Px(px, s, 45 + i * 20, 95, alienGlow);
                        Px(px, s, 50 + i * 18, 85, Lighten(alienGlow, 0.40f));
                    }
                    // Glowing underbelly
                    SetRect(px, s, 40, 44, 100, 4, alienGlow);
                    break;

                case 9: // Transcendent - ethereal wisps, crystal spikes
                    Color crystal = new Color(0.95f, 0.95f, 1.00f);
                    Color aura = new Color(1f, 1f, 1f, 0.70f);
                    // Crystal back spikes (replace normal spikes)
                    for (int i = 0; i < 5; i++)
                    {
                        int sx = 36 + i * 24;
                        SetRect(px, s, sx, 136, 6, 12, crystal);
                        Px(px, s, sx + 2, 148, aura);
                    }
                    // Ethereal glow outline
                    for (int i = 0; i < 8; i++) Px(px, s, 20 + i * 20, 118, aura);
                    break;
            }
        }

        private static void PaintStationaryEnemy(Color[] px, int s, Color body, Color sec, Color eyes, int era)
        {
            Color dark = Darken(body, 0.65f);
            Color warning = new Color(0.90f, 0.80f, 0.00f);
            Color warningDk = Darken(warning, 0.65f);

            // === Base platform ===
            SetRect(px, s, 36, 0, 120, 54, sec);
            // Warning stripes (alternating chevrons)
            for (int i = 0; i < 12; i++)
            {
                Color sc = (i % 2 == 0) ? warning : warningDk;
                SetRect(px, s, 42 + i * 9, 6, 6, 8, sc);
            }
            // Bolts on base
            int[] boltX = {50, 80, 110, 140};
            foreach (int bx in boltX)
            {
                SetRect(px, s, bx, 22, 4, 4, dark);
                Px(px, s, bx + 1, 24, Lighten(dark, 0.20f));
                SetRect(px, s, bx, 38, 4, 4, dark);
                Px(px, s, bx + 1, 40, Lighten(dark, 0.20f));
            }

            // === Pedestal ===
            SetRect(px, s, 58, 54, 76, 30, body);
            SetRect(px, s, 58, 54, 4, 30, Lighten(body, 0.15f));
            SetRect(px, s, 130, 54, 4, 30, dark);
            // Panel seam
            SetRect(px, s, 58, 68, 76, 2, dark);

            // === Dome ===
            SetRect(px, s, 46, 84, 100, 62, body);
            SetRect(px, s, 46, 84, 4, 62, Lighten(body, 0.20f));
            SetRect(px, s, 142, 84, 4, 62, dark);
            // Dome highlight
            SetRect(px, s, 50, 130, 40, 12, Lighten(body, 0.18f));
            // Sensor array
            SetRect(px, s, 76, 116, 20, 14, eyes);
            SetRect(px, s, 80, 120, 12, 6, Lighten(eyes, 0.30f));
            Px(px, s, 85, 122, Lighten(eyes, 0.50f)); Px(px, s, 86, 122, Lighten(eyes, 0.50f));
            // Panel lines
            SetRect(px, s, 46, 108, 100, 3, dark);
            SetRect(px, s, 46, 130, 100, 2, Darken(body, 0.85f));

            // === Barrel ===
            SetRect(px, s, 146, 100, 46, 26, dark);
            SetRect(px, s, 146, 104, 46, 18, Darken(dark, 0.80f));
            // Barrel rings
            SetRect(px, s, 154, 100, 3, 26, Lighten(dark, 0.20f));
            SetRect(px, s, 166, 100, 3, 26, Lighten(dark, 0.20f));
            SetRect(px, s, 178, 100, 3, 26, Lighten(dark, 0.20f));
            // Muzzle glow
            SetRect(px, s, 182, 106, 10, 14, eyes);
            SetRect(px, s, 184, 108, 8, 10, Lighten(eyes, 0.30f));
            SetRect(px, s, 186, 110, 4, 6, Lighten(eyes, 0.50f));

            // === Epoch-specific details ===
            AddStationaryEpochDetails(px, s, body, sec, eyes, dark, era);
        }

        private static void AddStationaryEpochDetails(Color[] px, int s, Color body, Color sec, Color eyes, Color dark, int era)
        {
            switch (era)
            {
                case 0: // Stone Age - wooden base, stone thrower
                    Color wood = new Color(0.55f, 0.40f, 0.25f);
                    Color stone = new Color(0.65f, 0.60f, 0.55f);
                    // Replace base with wood planks
                    for (int i = 0; i < 6; i++)
                        SetRect(px, s, 40 + i * 18, 2, 16, 50, Darken(wood, 0.90f + (i % 2) * 0.15f));
                    // Stone ammo pile
                    SetRect(px, s, 50, 46, 12, 8, stone);
                    SetRect(px, s, 130, 46, 12, 8, stone);
                    break;

                case 1: // Bronze Age - bronze finish, sun emblem
                    Color bronze = new Color(0.80f, 0.55f, 0.20f);
                    SetRect(px, s, 76, 130, 20, 14, bronze); // sun emblem
                    SetRect(px, s, 82, 134, 8, 6, new Color(1f, 0.85f, 0.30f));
                    // Bronze trim
                    SetRect(px, s, 46, 142, 100, 3, bronze);
                    break;

                case 2: // Classical - column styled pedestal
                    Color marble = new Color(0.92f, 0.90f, 0.85f);
                    // Fluted column lines
                    for (int i = 0; i < 5; i++)
                        SetRect(px, s, 66 + i * 12, 56, 3, 26, marble);
                    // Decorative band
                    SetRect(px, s, 46, 80, 100, 4, new Color(0.70f, 0.20f, 0.20f));
                    break;

                case 3: // Medieval - castle turret style
                    Color castleStone = new Color(0.55f, 0.55f, 0.58f);
                    // Crenellations
                    for (int i = 0; i < 5; i++)
                        SetRect(px, s, 52 + i * 20, 142, 10, 8, castleStone);
                    // Arrow slit
                    SetRect(px, s, 90, 90, 12, 24, Darken(castleStone, 0.50f));
                    SetRect(px, s, 94, 88, 4, 28, Darken(castleStone, 0.30f));
                    break;

                case 4: // Renaissance - ornate gilded details
                    Color gold = new Color(0.95f, 0.80f, 0.20f);
                    // Gilded dome ridges
                    SetRect(px, s, 46, 82, 100, 3, gold);
                    SetRect(px, s, 46, 144, 100, 2, gold);
                    // Decorative scrollwork
                    for (int i = 0; i < 4; i++)
                    {
                        Px(px, s, 56 + i * 24, 94, gold);
                        Px(px, s, 58 + i * 24, 96, gold);
                    }
                    break;

                case 5: // Industrial - rivets, steam pipes
                    Color brass = new Color(0.70f, 0.55f, 0.25f);
                    Color pipe = new Color(0.45f, 0.42f, 0.38f);
                    // Extra rivets
                    for (int i = 0; i < 8; i++)
                    {
                        SetRect(px, s, 50 + i * 12, 90, 3, 3, brass);
                        SetRect(px, s, 50 + i * 12, 136, 3, 3, brass);
                    }
                    // Steam pipe
                    SetRect(px, s, 30, 90, 16, 8, pipe);
                    SetRect(px, s, 26, 86, 8, 16, pipe);
                    break;

                case 6: // Modern - digital display, radar dish
                    Color screen = new Color(0.10f, 0.30f, 0.10f);
                    Color scanline = new Color(0.20f, 0.80f, 0.20f);
                    // Digital display
                    SetRect(px, s, 78, 118, 16, 10, screen);
                    for (int i = 0; i < 3; i++)
                        SetRect(px, s, 80, 120 + i * 3, 12, 1, scanline);
                    // Antenna
                    SetRect(px, s, 94, 146, 4, 12, dark);
                    break;

                case 7: // Digital - holographic projector
                    Color holo = new Color(0.00f, 0.95f, 0.85f);
                    Color holoFade = new Color(0.00f, 0.70f, 0.60f, 0.60f);
                    // Holographic ring
                    SetRect(px, s, 56, 90, 80, 2, holo);
                    SetRect(px, s, 56, 136, 80, 2, holo);
                    // Data streams
                    for (int i = 0; i < 4; i++)
                        SetRect(px, s, 64 + i * 18, 92, 2, 44, holoFade);
                    break;

                case 8: // Spacefaring - plasma coils
                    Color plasma = new Color(0.60f, 0.00f, 1.00f);
                    Color plasmaGlow = new Color(0.80f, 0.40f, 1.00f);
                    // Plasma coils around dome
                    SetRect(px, s, 42, 100, 4, 30, plasma);
                    SetRect(px, s, 146, 100, 4, 30, plasma);
                    Px(px, s, 44, 115, plasmaGlow); Px(px, s, 148, 115, plasmaGlow);
                    // Alien tech glow
                    SetRect(px, s, 80, 56, 32, 4, plasma);
                    break;

                case 9: // Transcendent - crystalline energy
                    Color crystal = new Color(0.95f, 0.95f, 1.00f);
                    Color aura = new Color(1f, 1f, 1f, 0.80f);
                    // Crystal dome
                    SetRect(px, s, 52, 88, 88, 52, crystal);
                    SetRect(px, s, 60, 96, 72, 36, Lighten(crystal, 0.20f));
                    // Ethereal glow points
                    for (int i = 0; i < 6; i++)
                        Px(px, s, 60 + i * 14, 140, aura);
                    // Energy core
                    SetRect(px, s, 84, 110, 24, 16, aura);
                    break;
            }
        }

        private static void PaintFlyingEnemy(Color[] px, int s, Color body, Color sec, Color eyes, int era)
        {
            Color dark = Darken(body, 0.65f);
            Color wingTip = Lighten(sec, 0.25f);
            Color feather = Darken(sec, 0.80f);

            // === Tail ===
            SetRect(px, s, 78, 14, 36, 16, dark);
            SetRect(px, s, 86, 30, 20, 16, dark);
            Px(px, s, 93, 10, dark); Px(px, s, 94, 10, dark); Px(px, s, 95, 11, dark); Px(px, s, 96, 11, dark);
            // Tail tip fork
            SetRect(px, s, 80, 4, 8, 10, Darken(dark, 0.80f));
            SetRect(px, s, 104, 4, 8, 10, Darken(dark, 0.80f));

            // === Body ===
            SetRect(px, s, 66, 46, 60, 106, body);
            SetRect(px, s, 66, 46, 4, 106, Lighten(body, 0.15f));
            SetRect(px, s, 122, 46, 4, 106, dark);
            // Belly scale bands
            for (int i = 0; i < 5; i++)
                SetRect(px, s, 78, 56 + i * 18, 36, 3, Darken(body, 0.85f));
            // Chest highlight
            SetRect(px, s, 72, 120, 48, 16, Lighten(body, 0.10f));

            // === Head ===
            SetRect(px, s, 70, 152, 52, 40, sec);
            SetRect(px, s, 70, 152, 4, 40, Lighten(sec, 0.15f));
            SetRect(px, s, 118, 152, 4, 40, Darken(sec, 0.70f));
            // Eyes
            SetRect(px, s, 78, 168, 14, 12, eyes);
            SetRect(px, s, 104, 168, 14, 12, eyes);
            Px(px, s, 82, 172, dark); Px(px, s, 83, 172, dark); Px(px, s, 84, 173, dark);
            Px(px, s, 108, 172, dark); Px(px, s, 109, 172, dark); Px(px, s, 110, 173, dark);
            // Beak
            SetRect(px, s, 86, 152, 20, 8, dark);
            SetRect(px, s, 90, 148, 12, 4, Darken(dark, 0.80f));
            Px(px, s, 95, 146, dark); Px(px, s, 96, 146, dark);
            // Crown feathers
            SetRect(px, s, 84, 190, 24, 2, wingTip);

            // === Wings (swept shape) ===
            for (int row = 0; row < 48; row++)
            {
                int wingY = 144 - row;
                // Left wing
                int lStart = 14 + row;
                int lEnd = 64;
                if (lStart < lEnd && wingY >= 0)
                {
                    Color wc = row < 8 ? wingTip : sec;
                    SetRect(px, s, lStart, wingY, lEnd - lStart, 4, wc);
                }
                // Right wing
                int rStart = 126;
                int rEnd = 178 - row;
                if (rStart < rEnd && wingY >= 0)
                {
                    Color wc = row < 8 ? wingTip : sec;
                    SetRect(px, s, rStart, wingY, rEnd - rStart, 4, wc);
                }
            }
            // Wing tip glow
            SetRect(px, s, 14, 142, 8, 6, eyes);
            SetRect(px, s, 172, 142, 8, 6, eyes);
            // Feather lines
            for (int i = 0; i < 5; i++)
            {
                Px(px, s, 26 + i * 8, 132 - i * 6, feather);
                Px(px, s, 27 + i * 8, 132 - i * 6, feather);
                Px(px, s, 158 - i * 8, 132 - i * 6, feather);
                Px(px, s, 159 - i * 8, 132 - i * 6, feather);
            }

            // === Epoch-specific details ===
            AddFlyingEpochDetails(px, s, body, sec, eyes, dark, wingTip, era);
        }

        private static void AddFlyingEpochDetails(Color[] px, int s, Color body, Color sec, Color eyes, Color dark, Color wingTip, int era)
        {
            switch (era)
            {
                case 0: // Stone Age - pterodactyl crest, leathery wings
                    Color leather = new Color(0.50f, 0.40f, 0.30f);
                    // Head crest
                    SetRect(px, s, 90, 186, 4, 6, leather);
                    SetRect(px, s, 88, 184, 6, 4, leather);
                    // Wing membrane texture
                    for (int i = 0; i < 4; i++)
                    {
                        SetRect(px, s, 30 + i * 8, 120 - i * 5, 2, 20, Darken(leather, 0.80f));
                        SetRect(px, s, 154 - i * 8, 120 - i * 5, 2, 20, Darken(leather, 0.80f));
                    }
                    break;

                case 1: // Bronze Age - golden wing tips
                    Color gold = new Color(0.90f, 0.75f, 0.20f);
                    SetRect(px, s, 14, 140, 12, 8, gold);
                    SetRect(px, s, 168, 140, 12, 8, gold);
                    // Sun disk necklace
                    SetRect(px, s, 88, 140, 16, 10, gold);
                    break;

                case 2: // Classical - laurel, roman eagle style
                    Color laurel = new Color(0.40f, 0.60f, 0.30f);
                    Color redBand = new Color(0.75f, 0.15f, 0.15f);
                    // Laurel crown
                    for (int i = 0; i < 4; i++) Px(px, s, 82 + i * 8, 188, laurel);
                    // Red chest band
                    SetRect(px, s, 72, 100, 48, 4, redBand);
                    break;

                case 3: // Medieval - armored, dragon-like
                    Color armor = new Color(0.55f, 0.55f, 0.60f);
                    // Chest plate
                    SetRect(px, s, 78, 80, 36, 30, armor);
                    SetRect(px, s, 82, 84, 28, 22, Lighten(armor, 0.15f));
                    // Horned head
                    SetRect(px, s, 72, 186, 4, 8, dark);
                    SetRect(px, s, 116, 186, 4, 8, dark);
                    break;

                case 4: // Renaissance - colorful plumage
                    Color plume1 = new Color(0.90f, 0.25f, 0.45f);
                    Color plume2 = new Color(0.20f, 0.60f, 0.85f);
                    // Colorful tail feathers
                    SetRect(px, s, 82, 6, 6, 8, plume1);
                    SetRect(px, s, 104, 6, 6, 8, plume2);
                    // Decorative chest medallion
                    SetRect(px, s, 86, 110, 20, 14, new Color(0.95f, 0.80f, 0.20f));
                    break;

                case 5: // Industrial - mechanical wings, goggles
                    Color brass = new Color(0.70f, 0.55f, 0.25f);
                    Color steel = new Color(0.50f, 0.50f, 0.55f);
                    // Goggles
                    SetRect(px, s, 76, 174, 16, 8, brass);
                    SetRect(px, s, 102, 174, 16, 8, brass);
                    SetRect(px, s, 80, 176, 8, 4, new Color(0.30f, 0.25f, 0.20f));
                    SetRect(px, s, 106, 176, 8, 4, new Color(0.30f, 0.25f, 0.20f));
                    // Mechanical wing struts
                    SetRect(px, s, 40, 110, 4, 30, steel);
                    SetRect(px, s, 150, 110, 4, 30, steel);
                    break;

                case 6: // Modern - jet pack, tactical
                    Color jetpack = new Color(0.35f, 0.35f, 0.40f);
                    Color flame = new Color(1f, 0.50f, 0.10f);
                    // Jet pack
                    SetRect(px, s, 84, 46, 24, 20, jetpack);
                    // Thrust flames
                    SetRect(px, s, 88, 40, 6, 8, flame);
                    SetRect(px, s, 98, 40, 6, 8, flame);
                    Px(px, s, 90, 38, new Color(1f, 0.90f, 0.40f));
                    Px(px, s, 100, 38, new Color(1f, 0.90f, 0.40f));
                    break;

                case 7: // Digital - data streams, holographic
                    Color data = new Color(0.00f, 0.95f, 0.80f);
                    // Data trail from wings
                    for (int i = 0; i < 6; i++)
                    {
                        Px(px, s, 20 + i * 6, 130 - i * 3, data);
                        Px(px, s, 170 - i * 6, 130 - i * 3, data);
                    }
                    // Circuit pattern on body
                    SetRect(px, s, 90, 70, 2, 60, data);
                    SetRect(px, s, 80, 100, 32, 2, data);
                    break;

                case 8: // Spacefaring - energy wings, alien
                    Color energy = new Color(0.60f, 0.00f, 1.00f);
                    Color energyGlow = new Color(0.80f, 0.40f, 1.00f);
                    // Energy wing glow
                    for (int i = 0; i < 8; i++)
                    {
                        Px(px, s, 18 + i * 5, 138 - i * 2, energyGlow);
                        Px(px, s, 172 - i * 5, 138 - i * 2, energyGlow);
                    }
                    // Alien third eye
                    SetRect(px, s, 90, 180, 12, 8, energy);
                    SetRect(px, s, 94, 182, 4, 4, energyGlow);
                    break;

                case 9: // Transcendent - pure light wings, ethereal
                    Color light = new Color(1f, 1f, 1f, 0.90f);
                    Color aura = new Color(0.95f, 0.95f, 1.00f);
                    // Glowing wing edges
                    SetRect(px, s, 14, 138, 50, 4, light);
                    SetRect(px, s, 130, 138, 50, 4, light);
                    // Halo above head
                    SetRect(px, s, 78, 190, 36, 2, light);
                    // Ethereal body glow
                    SetRect(px, s, 70, 60, 4, 80, aura);
                    SetRect(px, s, 118, 60, 4, 80, aura);
                    break;
            }
        }

        #endregion

        #region Projectiles and Weapons

        public static Sprite GetProjectileSprite()
        {
            return GetProjectileSprite(WeaponTier.Starting, 0);
        }

        public static Sprite GetProjectileSprite(WeaponTier tier)
        {
            return GetProjectileSprite(tier, 0);
        }

        /// <summary>
        /// Get projectile sprite with tier and epoch-specific styling.
        /// </summary>
        public static Sprite GetProjectileSprite(WeaponTier tier, int epoch)
        {
            string key = $"projectile_{tier}_e{epoch}";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];

            // Get epoch-specific colors, blended with tier
            var (epochBright, epochDim, epochGlow) = GetEpochProjectileColors(epoch);

            // Tier-specific color palettes, tinted by epoch
            Color bright, core, dim, glow, outer;
            switch (tier)
            {
                case WeaponTier.Heavy:
                    bright = BlendColors(new Color(1f, 0.30f, 0.15f), epochBright, 0.3f);
                    core   = new Color(1f, 0.85f, 0.70f);
                    dim    = BlendColors(new Color(0.70f, 0.10f, 0.05f), epochDim, 0.3f);
                    glow   = BlendColors(new Color(1f, 0.50f, 0.10f), epochGlow, 0.3f);
                    outer  = Darken(dim, 0.60f);
                    break;
                case WeaponTier.Medium:
                    bright = BlendColors(new Color(0.20f, 1f, 0.50f), epochBright, 0.3f);
                    core   = new Color(0.80f, 1f, 0.90f);
                    dim    = BlendColors(new Color(0.05f, 0.70f, 0.20f), epochDim, 0.3f);
                    glow   = BlendColors(new Color(0.30f, 1f, 0.60f), epochGlow, 0.3f);
                    outer  = Darken(dim, 0.60f);
                    break;
                default: // Starting
                    bright = BlendColors(new Color(1f, 1f, 0.50f), epochBright, 0.3f);
                    core   = new Color(1f, 1f, 0.90f);
                    dim    = BlendColors(new Color(0.80f, 0.70f, 0.10f), epochDim, 0.3f);
                    glow   = BlendColors(new Color(1f, 0.95f, 0.30f), epochGlow, 0.3f);
                    outer  = Darken(dim, 0.60f);
                    break;
            }

            // Paint epoch-specific projectile shape
            PaintEpochProjectile(px, size, epoch, tier, bright, core, dim, glow, outer);

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), PPU);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        private static Color BlendColors(Color a, Color b, float t)
        {
            return new Color(
                a.r + (b.r - a.r) * t,
                a.g + (b.g - a.g) * t,
                a.b + (b.b - a.b) * t,
                a.a);
        }

        private static (Color bright, Color dim, Color glow) GetEpochProjectileColors(int epoch)
        {
            return epoch switch
            {
                0 => (new Color(0.85f, 0.70f, 0.45f), new Color(0.55f, 0.40f, 0.25f), new Color(1f, 0.85f, 0.50f)), // Stone - earthy
                1 => (new Color(0.90f, 0.65f, 0.20f), new Color(0.70f, 0.45f, 0.10f), new Color(1f, 0.80f, 0.30f)), // Bronze - golden
                2 => (new Color(0.85f, 0.25f, 0.20f), new Color(0.60f, 0.15f, 0.10f), new Color(1f, 0.40f, 0.30f)), // Classical - red
                3 => (new Color(0.70f, 0.70f, 0.75f), new Color(0.40f, 0.40f, 0.45f), new Color(0.90f, 0.90f, 1f)),  // Medieval - silver
                4 => (new Color(0.80f, 0.50f, 0.70f), new Color(0.50f, 0.25f, 0.45f), new Color(1f, 0.70f, 0.90f)), // Renaissance - purple
                5 => (new Color(0.80f, 0.55f, 0.20f), new Color(0.50f, 0.35f, 0.15f), new Color(1f, 0.70f, 0.30f)), // Industrial - brass
                6 => (new Color(0.40f, 0.80f, 0.40f), new Color(0.20f, 0.50f, 0.20f), new Color(0.50f, 1f, 0.50f)), // Modern - tracer green
                7 => (new Color(0.00f, 0.90f, 0.80f), new Color(0.00f, 0.50f, 0.45f), new Color(0.30f, 1f, 0.95f)), // Digital - cyan
                8 => (new Color(0.70f, 0.20f, 1.00f), new Color(0.40f, 0.10f, 0.60f), new Color(0.85f, 0.50f, 1f)), // Spacefaring - purple plasma
                9 => (new Color(1.00f, 1.00f, 1.00f), new Color(0.80f, 0.80f, 0.90f), new Color(1f, 1f, 1f)),       // Transcendent - pure white
                _ => (new Color(1f, 1f, 0.50f), new Color(0.80f, 0.70f, 0.10f), new Color(1f, 0.95f, 0.30f)),
            };
        }

        private static void PaintEpochProjectile(Color[] px, int size, int epoch, WeaponTier tier,
            Color bright, Color core, Color dim, Color glow, Color outer)
        {
            switch (epoch)
            {
                case 0: // Stone Age - rough rock shape
                    // Irregular stone shape
                    SetRect(px, size, 12, 4, 8, 4, outer);
                    SetRect(px, size, 8, 8, 16, 4, dim);
                    SetRect(px, size, 6, 12, 20, 8, bright);
                    SetRect(px, size, 8, 20, 16, 4, dim);
                    SetRect(px, size, 12, 24, 8, 4, outer);
                    // Rough edges
                    Px(px, size, 5, 14, dim); Px(px, size, 26, 14, dim);
                    Px(px, size, 7, 10, outer); Px(px, size, 24, 18, outer);
                    // Center
                    SetRect(px, size, 12, 14, 8, 4, core);
                    break;

                case 1: // Bronze Age - sun disk
                    // Circular sun shape
                    SetRect(px, size, 12, 4, 8, 4, glow);
                    SetRect(px, size, 8, 8, 16, 4, bright);
                    SetRect(px, size, 6, 12, 20, 8, bright);
                    SetRect(px, size, 8, 20, 16, 4, bright);
                    SetRect(px, size, 12, 24, 8, 4, glow);
                    // Sun rays
                    Px(px, size, 15, 0, glow); Px(px, size, 16, 0, glow);
                    Px(px, size, 15, 31, glow); Px(px, size, 16, 31, glow);
                    Px(px, size, 0, 15, glow); Px(px, size, 31, 15, glow);
                    // Core
                    SetRect(px, size, 11, 13, 10, 6, core);
                    break;

                case 2: // Classical - arrow/spear point
                    // Sharp spear shape
                    SetRect(px, size, 14, 2, 4, 6, outer);
                    SetRect(px, size, 12, 8, 8, 4, dim);
                    SetRect(px, size, 10, 12, 12, 4, bright);
                    SetRect(px, size, 8, 16, 16, 4, bright);
                    SetRect(px, size, 10, 20, 12, 4, dim);
                    SetRect(px, size, 12, 24, 8, 4, outer);
                    SetRect(px, size, 14, 28, 4, 2, outer);
                    // Point highlight
                    SetRect(px, size, 14, 4, 4, 4, glow);
                    SetRect(px, size, 12, 14, 8, 4, core);
                    break;

                case 3: // Medieval - crossbow bolt
                    // Long thin bolt shape
                    SetRect(px, size, 14, 0, 4, 8, outer);
                    SetRect(px, size, 12, 8, 8, 4, dim);
                    SetRect(px, size, 10, 12, 12, 8, bright);
                    SetRect(px, size, 8, 20, 16, 4, bright);
                    SetRect(px, size, 6, 24, 20, 4, dim);
                    SetRect(px, size, 4, 28, 24, 4, outer);
                    // Fletching
                    Px(px, size, 6, 26, glow); Px(px, size, 25, 26, glow);
                    SetRect(px, size, 13, 14, 6, 4, core);
                    break;

                case 4: // Renaissance - ornate bullet
                    // Rounded ornate shape
                    SetRect(px, size, 13, 4, 6, 4, glow);
                    SetRect(px, size, 10, 8, 12, 4, bright);
                    SetRect(px, size, 8, 12, 16, 8, bright);
                    SetRect(px, size, 10, 20, 12, 4, bright);
                    SetRect(px, size, 13, 24, 6, 4, glow);
                    // Decorative swirl
                    Px(px, size, 9, 14, glow); Px(px, size, 22, 14, glow);
                    Px(px, size, 10, 18, glow); Px(px, size, 21, 18, glow);
                    SetRect(px, size, 12, 13, 8, 6, core);
                    break;

                case 5: // Industrial - cannonball/bullet
                    // Round cannonball shape
                    SetRect(px, size, 12, 6, 8, 4, outer);
                    SetRect(px, size, 8, 10, 16, 4, dim);
                    SetRect(px, size, 6, 14, 20, 4, bright);
                    SetRect(px, size, 8, 18, 16, 4, dim);
                    SetRect(px, size, 12, 22, 8, 4, outer);
                    // Hot glow
                    SetRect(px, size, 11, 13, 10, 6, core);
                    // Spark trail
                    Px(px, size, 8, 24, glow); Px(px, size, 23, 24, glow);
                    break;

                case 6: // Modern - tracer round
                    // Sleek bullet shape
                    SetRect(px, size, 14, 2, 4, 6, glow);
                    SetRect(px, size, 12, 8, 8, 4, bright);
                    SetRect(px, size, 10, 12, 12, 8, bright);
                    SetRect(px, size, 12, 20, 8, 8, dim);
                    SetRect(px, size, 14, 28, 4, 4, outer);
                    // Tracer glow trail
                    SetRect(px, size, 13, 22, 6, 8, glow);
                    SetRect(px, size, 14, 14, 4, 4, core);
                    break;

                case 7: // Digital - data packet
                    // Square data block
                    SetRect(px, size, 8, 8, 16, 16, bright);
                    SetRect(px, size, 10, 10, 12, 12, dim);
                    // Binary pattern
                    Px(px, size, 12, 12, glow); Px(px, size, 18, 12, glow);
                    Px(px, size, 15, 15, core); Px(px, size, 16, 15, core);
                    Px(px, size, 12, 18, glow); Px(px, size, 18, 18, glow);
                    // Data trails
                    SetRect(px, size, 15, 4, 2, 4, glow);
                    SetRect(px, size, 15, 24, 2, 4, glow);
                    SetRect(px, size, 4, 15, 4, 2, glow);
                    SetRect(px, size, 24, 15, 4, 2, glow);
                    break;

                case 8: // Spacefaring - plasma bolt
                    // Elongated plasma shape
                    SetRect(px, size, 14, 2, 4, 4, outer);
                    SetRect(px, size, 12, 6, 8, 4, dim);
                    SetRect(px, size, 10, 10, 12, 6, bright);
                    SetRect(px, size, 8, 16, 16, 4, bright);
                    SetRect(px, size, 10, 20, 12, 4, dim);
                    SetRect(px, size, 12, 24, 8, 4, outer);
                    // Plasma glow
                    SetRect(px, size, 11, 12, 10, 8, glow);
                    SetRect(px, size, 13, 14, 6, 4, core);
                    // Energy corona
                    Px(px, size, 6, 16, glow); Px(px, size, 25, 16, glow);
                    break;

                case 9: // Transcendent - pure light orb
                    // Perfect circle of light
                    SetRect(px, size, 12, 6, 8, 4, bright);
                    SetRect(px, size, 8, 10, 16, 4, bright);
                    SetRect(px, size, 6, 14, 20, 4, core);
                    SetRect(px, size, 8, 18, 16, 4, bright);
                    SetRect(px, size, 12, 22, 8, 4, bright);
                    // Inner pure core
                    SetRect(px, size, 10, 12, 12, 8, core);
                    SetRect(px, size, 12, 10, 8, 12, core);
                    // Outer radiance
                    Px(px, size, 15, 0, glow); Px(px, size, 16, 0, glow);
                    Px(px, size, 15, 31, glow); Px(px, size, 16, 31, glow);
                    Px(px, size, 0, 15, glow); Px(px, size, 0, 16, glow);
                    Px(px, size, 31, 15, glow); Px(px, size, 31, 16, glow);
                    // Diagonal sparkles
                    Px(px, size, 4, 4, glow); Px(px, size, 27, 4, glow);
                    Px(px, size, 4, 27, glow); Px(px, size, 27, 27, glow);
                    break;

                default:
                    // Default diamond bolt
                    SetRect(px, size, 13, 2, 6, 4, outer);
                    SetRect(px, size, 10, 6, 12, 4, dim);
                    SetRect(px, size, 7, 10, 18, 4, bright);
                    SetRect(px, size, 5, 14, 22, 4, bright);
                    SetRect(px, size, 7, 18, 18, 4, bright);
                    SetRect(px, size, 10, 22, 12, 4, dim);
                    SetRect(px, size, 13, 26, 6, 4, outer);
                    SetRect(px, size, 11, 13, 10, 6, core);
                    break;
            }
        }

        public static Sprite GetWeaponPickupSprite(WeaponTier tier)
        {
            string key = $"weapon_{tier}";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            int size = 96;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];

            switch (tier)
            {
                case WeaponTier.Starting: PaintWeaponSword(px, size); break;
                case WeaponTier.Medium:   PaintWeaponCrossbow(px, size); break;
                case WeaponTier.Heavy:    PaintWeaponCannon(px, size); break;
            }

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), PPU);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        private static void PaintWeaponSword(Color[] px, int s)
        {
            Color blade = new Color(0.60f, 0.80f, 0.60f);
            Color guard = Darken(blade, 0.55f);
            Color grip  = Darken(blade, 0.35f);
            Color hi    = Lighten(blade, 0.30f);
            Color edge  = Lighten(blade, 0.50f);
            Color wrap  = Darken(grip, 0.65f);

            // Pommel
            SetRect(px, s, 36, 6, 24, 12, guard);
            SetRect(px, s, 38, 8, 20, 8, Darken(guard, 0.80f));
            Px(px, s, 47, 10, hi); Px(px, s, 48, 10, hi);
            // Grip
            SetRect(px, s, 40, 18, 16, 22, grip);
            // Grip wrapping
            for (int i = 0; i < 5; i++)
                SetRect(px, s, 42, 20 + i * 4, 12, 2, wrap);
            // Crossguard
            SetRect(px, s, 22, 40, 52, 8, guard);
            SetRect(px, s, 26, 42, 44, 4, Lighten(guard, 0.20f));
            SetRect(px, s, 22, 40, 4, 8, Darken(guard, 0.80f));
            SetRect(px, s, 70, 40, 4, 8, Darken(guard, 0.80f));
            // Blade
            SetRect(px, s, 40, 48, 16, 42, blade);
            SetRect(px, s, 40, 48, 4, 42, hi);       // left edge highlight
            SetRect(px, s, 52, 48, 4, 42, Darken(blade, 0.80f)); // right shadow
            // Fuller (groove in blade)
            SetRect(px, s, 46, 52, 4, 30, Darken(blade, 0.85f));
            // Blade tip
            SetRect(px, s, 42, 90, 12, 2, blade);
            SetRect(px, s, 44, 92, 8, 2, edge);
            SetRect(px, s, 46, 94, 4, 2, edge);
            // Edge glint
            Px(px, s, 41, 70, edge); Px(px, s, 41, 80, edge);
        }

        private static void PaintWeaponCrossbow(Color[] px, int s)
        {
            Color wood = new Color(0.50f, 0.35f, 0.20f);
            Color woodDk = Darken(wood, 0.65f);
            Color woodLt = Lighten(wood, 0.15f);
            Color metal = new Color(0.40f, 0.60f, 1.00f);
            Color metalDk = Darken(metal, 0.70f);
            Color str = Lighten(metal, 0.35f);

            // Stock
            SetRect(px, s, 14, 14, 68, 18, wood);
            SetRect(px, s, 18, 18, 60, 10, woodDk);  // stock groove
            SetRect(px, s, 14, 14, 68, 3, woodLt);    // top highlight
            // Wood grain
            for (int i = 0; i < 8; i++) Px(px, s, 20 + i * 8, 20, Darken(wood, 0.80f));
            // Left arm
            SetRect(px, s, 6, 32, 16, 26, metal);
            SetRect(px, s, 8, 34, 12, 22, metalDk);
            SetRect(px, s, 6, 54, 8, 8, metal);  // left tip
            // Right arm
            SetRect(px, s, 74, 32, 16, 26, metal);
            SetRect(px, s, 76, 34, 12, 22, metalDk);
            SetRect(px, s, 82, 54, 8, 8, metal);  // right tip
            // String
            SetRect(px, s, 14, 58, 68, 3, str);
            // Bolt
            SetRect(px, s, 42, 30, 8, 58, metal);
            SetRect(px, s, 44, 32, 4, 54, metalDk);
            // Bolt tip
            SetRect(px, s, 42, 86, 8, 6, str);
            SetRect(px, s, 44, 92, 4, 4, Lighten(str, 0.30f));
            // Trigger guard
            SetRect(px, s, 38, 6, 16, 10, wood);
            SetRect(px, s, 42, 8, 8, 8, woodDk);   // trigger
        }

        private static void PaintWeaponCannon(Color[] px, int s)
        {
            Color barrel = new Color(0.50f, 0.30f, 0.55f);
            Color body = new Color(0.80f, 0.40f, 0.90f);
            Color glow = new Color(1.0f, 0.70f, 1.00f);
            Color dark = Darken(barrel, 0.55f);

            // Handle
            SetRect(px, s, 36, 6, 24, 22, dark);
            SetRect(px, s, 40, 10, 16, 14, Darken(dark, 0.75f));
            // Handle grip lines
            for (int i = 0; i < 3; i++) SetRect(px, s, 42, 12 + i * 5, 12, 2, Darken(dark, 0.60f));
            // Chamber
            SetRect(px, s, 22, 28, 52, 28, body);
            SetRect(px, s, 26, 32, 44, 20, Lighten(body, 0.15f));
            // Chamber energy
            SetRect(px, s, 40, 38, 16, 12, glow);
            SetRect(px, s, 44, 40, 8, 8, Lighten(glow, 0.40f));
            Px(px, s, 47, 43, Color.white); Px(px, s, 48, 44, Color.white);
            // Barrel
            SetRect(px, s, 30, 56, 36, 30, barrel);
            SetRect(px, s, 32, 58, 32, 26, Darken(barrel, 0.85f));
            // Barrel rings
            SetRect(px, s, 26, 60, 44, 4, dark);
            SetRect(px, s, 26, 72, 44, 4, dark);
            SetRect(px, s, 26, 80, 44, 4, dark);
            // Muzzle glow
            SetRect(px, s, 34, 86, 28, 8, glow);
            SetRect(px, s, 38, 88, 20, 4, Lighten(glow, 0.30f));
            SetRect(px, s, 42, 90, 12, 2, Lighten(glow, 0.50f));
        }

        #endregion

        #region Rewards

        public static Sprite GetRewardSprite(RewardType type)
        {
            string key = $"reward_{type}";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            int size = 64;
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

        private static void PaintHealthSmall(Color[] px, int s)
        {
            Color c = new Color(0.20f, 0.90f, 0.20f);
            Color hi = Lighten(c, 0.35f);
            Color dk = Darken(c, 0.65f);
            Color core = Lighten(c, 0.55f);
            // Green cross
            SetRect(px, s, 22, 8, 20, 48, c);    // vertical bar
            SetRect(px, s, 8, 22, 48, 20, c);     // horizontal bar
            // Highlight edges
            SetRect(px, s, 22, 48, 20, 8, hi);    // top
            SetRect(px, s, 8, 34, 14, 8, hi);     // left arm top
            // Shadow edges
            SetRect(px, s, 22, 8, 20, 8, dk);     // bottom
            SetRect(px, s, 42, 22, 14, 8, dk);    // right arm bottom
            // Inner glow
            SetRect(px, s, 28, 28, 8, 8, core);
            // Border
            SetRect(px, s, 22, 6, 20, 2, dk); SetRect(px, s, 22, 56, 20, 2, dk);
            SetRect(px, s, 6, 22, 2, 20, dk); SetRect(px, s, 56, 22, 2, 20, dk);
        }

        private static void PaintHealthLarge(Color[] px, int s)
        {
            Color c = new Color(0.90f, 0.15f, 0.15f);
            Color hi = Lighten(c, 0.30f);
            Color dk = Darken(c, 0.65f);
            Color core = Lighten(c, 0.50f);
            // Heart shape (native 64x64)
            SetRect(px, s, 8, 48, 20, 12, c);     // left bump
            SetRect(px, s, 36, 48, 20, 12, c);     // right bump
            SetRect(px, s, 4, 36, 56, 12, c);      // wide middle
            SetRect(px, s, 8, 26, 48, 10, c);
            SetRect(px, s, 12, 18, 40, 8, c);
            SetRect(px, s, 16, 12, 32, 6, c);
            SetRect(px, s, 20, 6, 24, 6, c);
            SetRect(px, s, 26, 2, 12, 4, c);
            SetRect(px, s, 30, 0, 4, 2, c);         // point tip
            // Highlight on left lobe
            SetRect(px, s, 10, 48, 14, 8, hi);
            SetRect(px, s, 12, 42, 10, 6, hi);
            Px(px, s, 16, 40, hi); Px(px, s, 17, 40, hi);
            // Inner core glow
            SetRect(px, s, 24, 26, 8, 8, core);
            // Bottom shadow
            SetRect(px, s, 28, 2, 8, 2, dk);
        }

        private static void PaintAttackBoost(Color[] px, int s)
        {
            Color blade = new Color(1.00f, 0.50f, 0.10f);
            Color guard = Darken(blade, 0.65f);
            Color grip  = Darken(blade, 0.45f);
            Color hi    = Lighten(blade, 0.30f);
            Color edge  = Lighten(blade, 0.50f);

            // Blade
            SetRect(px, s, 24, 24, 16, 36, blade);
            SetRect(px, s, 24, 24, 4, 36, hi);
            SetRect(px, s, 36, 24, 4, 36, Darken(blade, 0.80f));
            // Tip
            SetRect(px, s, 28, 60, 8, 2, blade);
            SetRect(px, s, 30, 62, 4, 2, edge);
            // Cross guard
            SetRect(px, s, 12, 20, 40, 6, guard);
            SetRect(px, s, 16, 21, 32, 4, Lighten(guard, 0.20f));
            // Grip
            SetRect(px, s, 24, 8, 16, 12, grip);
            SetRect(px, s, 26, 10, 12, 8, Darken(grip, 0.70f));
            // Pommel
            SetRect(px, s, 20, 0, 24, 8, blade);
            SetRect(px, s, 24, 2, 16, 4, hi);
            // Flame effect on blade
            Px(px, s, 26, 44, edge); Px(px, s, 27, 46, edge);
            Px(px, s, 35, 38, edge); Px(px, s, 36, 40, edge);
        }

        private static void PaintSpeedBoost(Color[] px, int s)
        {
            Color c = new Color(0.20f, 0.80f, 1.00f);
            Color hi = Lighten(c, 0.35f);
            Color dk = Darken(c, 0.70f);
            // Arrow shaft
            SetRect(px, s, 4, 22, 34, 20, c);
            SetRect(px, s, 4, 36, 34, 6, hi);
            // Arrowhead
            SetRect(px, s, 38, 16, 12, 32, c);
            SetRect(px, s, 50, 20, 8, 24, c);
            SetRect(px, s, 58, 26, 4, 12, c);
            SetRect(px, s, 62, 30, 2, 4, hi);   // sharp tip
            // Tail fins
            SetRect(px, s, 0, 44, 8, 10, c);
            SetRect(px, s, 0, 10, 8, 10, c);
            SetRect(px, s, 2, 46, 4, 6, dk);
            SetRect(px, s, 2, 12, 4, 6, dk);
            // Speed lines
            SetRect(px, s, 8, 18, 6, 2, hi);
            SetRect(px, s, 10, 44, 6, 2, hi);
            SetRect(px, s, 6, 8, 4, 2, hi);
            SetRect(px, s, 12, 54, 4, 2, hi);
        }

        private static void PaintShield(Color[] px, int s)
        {
            Color c = new Color(0.70f, 0.70f, 1.00f);
            Color dark = Darken(c, 0.55f);
            Color hi = Lighten(c, 0.35f);
            Color emblem = Darken(c, 0.40f);
            // Shield body (heraldic shape)
            SetRect(px, s, 14, 56, 36, 6, c);   // bottom point area
            SetRect(px, s, 10, 40, 44, 16, c);
            SetRect(px, s, 12, 28, 40, 12, c);
            SetRect(px, s, 16, 18, 32, 10, c);
            SetRect(px, s, 20, 10, 24, 8, c);
            SetRect(px, s, 26, 6, 12, 4, c);
            // Top highlight
            SetRect(px, s, 14, 56, 36, 2, hi);
            SetRect(px, s, 10, 48, 12, 8, hi);
            // Bottom shadow
            SetRect(px, s, 38, 40, 16, 6, dark);
            // Emblem (cross)
            SetRect(px, s, 28, 24, 8, 24, emblem);
            SetRect(px, s, 18, 32, 28, 8, emblem);
            // Emblem center
            SetRect(px, s, 30, 34, 4, 4, hi);
            // Border
            SetRect(px, s, 24, 4, 16, 2, dark);
        }

        private static void PaintCoin(Color[] px, int s)
        {
            Color gold = new Color(1.00f, 0.85f, 0.10f);
            Color inner = Darken(gold, 0.65f);
            Color highlight = Lighten(gold, 0.45f);
            Color edge = Darken(gold, 0.50f);

            // Circle (64x64 native)
            SetRect(px, s, 18, 2, 28, 4, gold);
            SetRect(px, s, 12, 6, 40, 4, gold);
            SetRect(px, s, 8, 10, 48, 4, gold);
            SetRect(px, s, 4, 14, 56, 36, gold);
            SetRect(px, s, 8, 50, 48, 4, gold);
            SetRect(px, s, 12, 54, 40, 4, gold);
            SetRect(px, s, 18, 58, 28, 4, gold);
            // Inner ring
            SetRect(px, s, 18, 14, 28, 36, inner);
            // Center
            SetRect(px, s, 22, 22, 20, 20, gold);
            // $ sign (native detail)
            SetRect(px, s, 28, 18, 8, 28, highlight);
            SetRect(px, s, 20, 28, 24, 4, inner);
            SetRect(px, s, 20, 34, 24, 4, inner);
            // Top/bottom serifs
            SetRect(px, s, 22, 22, 20, 4, highlight);
            SetRect(px, s, 22, 38, 20, 4, highlight);
            // Sparkle
            SetRect(px, s, 18, 46, 4, 4, highlight);
            Px(px, s, 22, 44, highlight);
            Px(px, s, 40, 18, highlight);
            // Edge highlight (top-left)
            SetRect(px, s, 14, 8, 8, 2, highlight);
            SetRect(px, s, 6, 16, 2, 8, highlight);
            // Edge shadow (bottom-right)
            SetRect(px, s, 42, 54, 8, 2, edge);
            SetRect(px, s, 54, 38, 2, 8, edge);
        }

        #endregion

        #region Checkpoints

        public static Sprite GetCheckpointSprite(CheckpointType type)
        {
            string key = $"checkpoint_{(int)type}";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            int w = 64, h = 192;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[w * h];

            Color pole = new Color(0.60f, 0.40f, 0.20f);
            Color poleDk = Darken(pole, 0.65f);
            Color poleHi = Lighten(pole, 0.25f);
            Color poleMid = Darken(pole, 0.85f);

            // Stone brick base (48x32 px)
            Color stone = new Color(0.45f, 0.42f, 0.38f);
            Color stoneDk = Darken(stone, 0.70f);
            Color stoneHi = Lighten(stone, 0.20f);
            Color mortar = Darken(stone, 0.55f);

            // Base platform layers (bottom to top)
            SetRect(px, w, 0, 0, 48, 8, stoneDk);    // bottom shadow layer
            SetRect(px, w, 2, 2, 44, 6, stone);      // bottom row bricks
            SetRect(px, w, 0, 8, 48, 16, stone);     // main brick body
            SetRect(px, w, 2, 22, 44, 4, stoneHi);   // top highlight
            SetRect(px, w, 4, 24, 40, 2, Lighten(stoneHi, 0.15f)); // top edge

            // Brick pattern (mortar lines)
            SetRect(px, w, 0, 8, 48, 1, mortar);     // horizontal mortar
            SetRect(px, w, 0, 16, 48, 1, mortar);    // horizontal mortar
            // Vertical mortar lines (staggered)
            for (int bx = 12; bx < 48; bx += 16) { SetRect(px, w, bx, 9, 1, 7, mortar); }
            for (int bx = 4; bx < 48; bx += 16) { SetRect(px, w, bx, 17, 1, 5, mortar); }

            // Brick highlights and shadows
            for (int by = 9; by < 22; by += 8)
            {
                for (int bx = 2; bx < 46; bx += 16)
                {
                    Px(px, w, bx, by + 5, stoneDk);     // shadow
                    Px(px, w, bx + 1, by, stoneHi);     // highlight
                }
            }

            // Pole mounted on base (8px wide, from base top)
            int poleBottom = 26;
            SetRect(px, w, 8, poleBottom, 8, 192 - poleBottom, pole);
            SetRect(px, w, 8, poleBottom, 3, 192 - poleBottom, poleDk);
            SetRect(px, w, 13, poleBottom, 3, 192 - poleBottom, poleHi);
            // Wood grain on pole
            for (int i = 0; i < 10; i++) Px(px, w, 11, poleBottom + 8 + i * 15, poleMid);

            // Pole cap (ornamental ball)
            SetRect(px, w, 6, 184, 12, 8, poleHi);
            SetRect(px, w, 8, 186, 8, 4, Lighten(poleHi, 0.20f));
            Px(px, w, 11, 188, Lighten(poleHi, 0.40f));

            switch (type)
            {
                case CheckpointType.LevelStart:
                {
                    Color flag = new Color(0.30f, 0.85f, 0.30f);
                    Color hi = Lighten(flag, 0.40f);
                    Color dk = Darken(flag, 0.65f);
                    Color stripe = Lighten(flag, 0.25f);
                    SetRect(px, w, 16, 128, 44, 56, flag);
                    SetRect(px, w, 16, 128, 44, 6, dk);   // shadow at attach
                    SetRect(px, w, 16, 134, 44, 4, Darken(flag, 0.85f));
                    // Horizontal stripe
                    SetRect(px, w, 16, 148, 44, 8, stripe);
                    // Star pattern (detailed)
                    SetRect(px, w, 32, 164, 10, 10, hi);
                    Px(px, w, 30, 169, hi); Px(px, w, 31, 169, hi);
                    Px(px, w, 42, 169, hi); Px(px, w, 43, 169, hi);
                    Px(px, w, 36, 162, hi); Px(px, w, 37, 162, hi);
                    Px(px, w, 36, 176, hi); Px(px, w, 37, 176, hi);
                    // Star inner
                    Px(px, w, 36, 168, Lighten(hi, 0.30f)); Px(px, w, 37, 169, Lighten(hi, 0.30f));
                    // Wave at bottom edge of flag
                    for (int i = 0; i < 11; i++)
                    {
                        int fy = 182 + (i % 3 == 0 ? 2 : 0);
                        Px(px, w, 16 + i * 4, fy, flag);
                    }
                    break;
                }
                case CheckpointType.MidLevel:
                {
                    Color flag = new Color(1.0f, 0.85f, 0.00f);
                    Color hi = new Color(1.0f, 1.0f, 0.50f);
                    Color dk = Darken(flag, 0.70f);
                    SetRect(px, w, 16, 128, 44, 56, flag);
                    SetRect(px, w, 16, 128, 44, 6, dk);
                    // Double stripes
                    SetRect(px, w, 16, 144, 44, 6, hi);
                    SetRect(px, w, 16, 166, 44, 6, hi);
                    // Decorative dots between stripes
                    for (int i = 0; i < 5; i++) Px(px, w, 22 + i * 8, 156, dk);
                    break;
                }
                case CheckpointType.PreBoss:
                {
                    Color flag = new Color(0.90f, 0.40f, 0.10f);
                    Color dk = Darken(flag, 0.65f);
                    SetRect(px, w, 16, 128, 44, 56, flag);
                    SetRect(px, w, 16, 128, 44, 6, dk);
                    // Skull (more detailed)
                    SetRect(px, w, 24, 158, 24, 4, Color.white);   // cranium top
                    SetRect(px, w, 22, 162, 28, 12, Color.white);   // cranium
                    SetRect(px, w, 26, 174, 20, 4, Color.white);    // jaw
                    // Eye sockets
                    SetRect(px, w, 26, 164, 8, 8, dk);
                    SetRect(px, w, 38, 164, 8, 8, dk);
                    // Nose
                    SetRect(px, w, 34, 170, 4, 4, dk);
                    // Teeth
                    for (int i = 0; i < 4; i++)
                        Px(px, w, 30 + i * 4, 176, Color.white);
                    break;
                }
                case CheckpointType.BossArena:
                {
                    Color flag = new Color(0.85f, 0.10f, 0.10f);
                    Color dk = Darken(flag, 0.65f);
                    SetRect(px, w, 16, 128, 44, 56, flag);
                    SetRect(px, w, 16, 128, 44, 6, dk);
                    // Exclamation mark (bold)
                    SetRect(px, w, 34, 158, 8, 20, Color.white);
                    SetRect(px, w, 36, 160, 4, 16, Lighten(flag, 0.40f));
                    SetRect(px, w, 34, 146, 8, 8, Color.white);
                    SetRect(px, w, 36, 148, 4, 4, Lighten(flag, 0.40f));
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

        public static Sprite GetGoalSprite()
        {
            string key = "goal";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            int w = 192, h = 256;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[w * h];

            Color frame   = new Color(0.75f, 0.60f, 0.15f);
            Color frameDk = Darken(frame, 0.55f);
            Color frameHi = Lighten(frame, 0.30f);
            Color frameMd = Darken(frame, 0.80f);
            Color portal  = new Color(0.30f, 0.90f, 1.00f);
            Color pDk     = Darken(portal, 0.65f);
            Color hi      = new Color(0.60f, 1.00f, 1.00f);
            Color star    = new Color(1.00f, 1.00f, 0.80f);

            // === Base ===
            SetRect(px, w, 0, 0, 192, 24, frame);
            SetRect(px, w, 0, 0, 192, 6, frameDk);
            SetRect(px, w, 0, 18, 192, 6, frameHi);

            // === Left pillar ===
            SetRect(px, w, 0, 0, 32, 228, frame);
            SetRect(px, w, 0, 0, 8, 228, frameDk);
            SetRect(px, w, 24, 0, 8, 228, frameHi);
            // Ornament bands
            for (int i = 0; i < 4; i++)
            {
                int by = 48 + i * 52;
                SetRect(px, w, 4, by, 24, 8, frameHi);
                SetRect(px, w, 6, by + 2, 20, 4, frameMd);
            }
            // Column fluting
            SetRect(px, w, 12, 24, 2, 204, frameMd);

            // === Right pillar ===
            SetRect(px, w, 160, 0, 32, 228, frame);
            SetRect(px, w, 160, 0, 8, 228, frameHi);
            SetRect(px, w, 184, 0, 8, 228, frameDk);
            for (int i = 0; i < 4; i++)
            {
                int by = 48 + i * 52;
                SetRect(px, w, 164, by, 24, 8, frameHi);
                SetRect(px, w, 166, by + 2, 20, 4, frameMd);
            }
            SetRect(px, w, 178, 24, 2, 204, frameMd);

            // === Arch top ===
            SetRect(px, w, 0, 228, 192, 28, frame);
            SetRect(px, w, 0, 248, 192, 8, frameHi);
            SetRect(px, w, 0, 228, 192, 6, frameDk);
            // Keystone
            SetRect(px, w, 78, 228, 36, 28, frameHi);
            SetRect(px, w, 84, 234, 24, 16, star);
            SetRect(px, w, 88, 238, 16, 8, Lighten(star, 0.30f));
            // Arch decorative dots
            for (int i = 0; i < 6; i++)
            {
                Px(px, w, 40 + i * 8, 240, frameMd);
                Px(px, w, 120 + i * 8, 240, frameMd);
            }

            // === Portal interior ===
            SetRect(px, w, 32, 24, 128, 204, portal);
            SetRect(px, w, 32, 24, 16, 204, pDk);
            SetRect(px, w, 144, 24, 16, 204, pDk);
            SetRect(px, w, 32, 24, 128, 20, pDk);
            // Center glow (radial gradient approximation)
            SetRect(px, w, 56, 72, 80, 100, Lighten(portal, 0.15f));
            SetRect(px, w, 68, 92, 56, 60, hi);
            SetRect(px, w, 80, 108, 32, 28, star);
            SetRect(px, w, 88, 114, 16, 16, Lighten(star, 0.30f));

            // === Sparkles ===
            SetRect(px, w, 44, 188, 4, 4, star);
            SetRect(px, w, 140, 56, 4, 4, star);
            SetRect(px, w, 72, 152, 4, 4, star);
            SetRect(px, w, 116, 168, 4, 4, star);
            SetRect(px, w, 56, 84, 4, 4, star);
            SetRect(px, w, 132, 120, 4, 4, star);
            Px(px, w, 100, 180, star); Px(px, w, 60, 140, star);
            Px(px, w, 124, 76, star); Px(px, w, 80, 200, star);

            // === Glowing base ===
            SetRect(px, w, 40, 24, 112, 10, hi);
            SetRect(px, w, 48, 28, 96, 4, star);

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), PPU);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        #endregion

        #region Bosses

        public static Sprite GetBossSprite(BossType type)
        {
            int era = (int)type;
            string key = $"boss_{era}";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            int size = 384;
            float bossPPU = 128f;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];

            var (body, secondary, eyes) = GetEraColors(era);
            body = Lighten(body, 0.15f);
            secondary = Lighten(secondary, 0.15f);

            Color dark = Darken(body, 0.55f);
            Color light = Lighten(body, 0.20f);
            Color secDk = Darken(secondary, 0.55f);
            Color secLt = Lighten(secondary, 0.15f);

            // === Boots (y=0..32) ===
            SetRect(px, size, 108, 0, 64, 32, secDk);
            for (int i = 0; i < 8; i++) SetRect(px, size, 114 + i * 7, 4, 4, 4, Darken(secDk, 0.70f));
            SetRect(px, size, 212, 0, 64, 32, secDk);
            for (int i = 0; i < 8; i++) SetRect(px, size, 218 + i * 7, 4, 4, 4, Darken(secDk, 0.70f));

            // === Legs (y=32..160) ===
            SetRect(px, size, 112, 32, 64, 128, secondary);
            SetRect(px, size, 112, 32, 8, 128, secLt);
            SetRect(px, size, 168, 32, 8, 128, secDk);
            SetRect(px, size, 104, 112, 80, 16, dark);  // knee guard
            SetRect(px, size, 108, 124, 72, 8, Darken(secondary, 0.80f));

            SetRect(px, size, 208, 32, 64, 128, secondary);
            SetRect(px, size, 208, 32, 8, 128, secLt);
            SetRect(px, size, 264, 32, 8, 128, secDk);
            SetRect(px, size, 200, 112, 80, 16, dark);
            SetRect(px, size, 204, 124, 72, 8, Darken(secondary, 0.80f));

            // === Belt (y=148..172) ===
            SetRect(px, size, 96, 148, 192, 24, dark);
            SetRect(px, size, 96, 168, 192, 4, Darken(dark, 0.80f));
            // Belt buckle
            SetRect(px, size, 168, 152, 48, 16, eyes);
            SetRect(px, size, 176, 156, 32, 8, Lighten(eyes, 0.30f));

            // === Torso (y=172..312) ===
            SetRect(px, size, 96, 172, 192, 140, body);
            SetRect(px, size, 96, 172, 8, 140, light);
            SetRect(px, size, 280, 172, 8, 140, dark);
            // Plate dividers
            SetRect(px, size, 96, 216, 192, 6, dark);
            SetRect(px, size, 96, 264, 192, 6, dark);
            SetRect(px, size, 184, 176, 8, 128, Darken(body, 0.85f));
            // Armor segments
            SetRect(px, size, 180, 176, 4, 128, Darken(body, 0.90f));
            SetRect(px, size, 192, 176, 4, 128, Darken(body, 0.90f));

            // === Shoulder pads ===
            SetRect(px, size, 56, 260, 40, 60, secondary);
            SetRect(px, size, 56, 260, 6, 60, secLt);
            SetRect(px, size, 288, 260, 40, 60, secondary);
            SetRect(px, size, 322, 260, 6, 60, secDk);
            // Shoulder rivets
            SetRect(px, size, 68, 300, 8, 8, eyes);
            SetRect(px, size, 80, 300, 8, 8, eyes);
            SetRect(px, size, 296, 300, 8, 8, eyes);
            SetRect(px, size, 308, 300, 8, 8, eyes);
            // Shoulder spikes
            SetRect(px, size, 60, 316, 8, 12, secLt);
            SetRect(px, size, 62, 326, 4, 6, Lighten(secLt, 0.15f));
            SetRect(px, size, 316, 316, 8, 12, secLt);
            SetRect(px, size, 318, 326, 4, 6, Lighten(secLt, 0.15f));

            // === Arms ===
            SetRect(px, size, 40, 172, 56, 88, body);
            SetRect(px, size, 40, 172, 8, 88, light);
            SetRect(px, size, 288, 172, 56, 88, body);
            SetRect(px, size, 336, 172, 8, 88, dark);
            // Forearm armor bands
            SetRect(px, size, 40, 200, 56, 8, dark);
            SetRect(px, size, 40, 232, 56, 8, dark);
            SetRect(px, size, 288, 200, 56, 8, dark);
            SetRect(px, size, 288, 232, 56, 8, dark);

            // === Clawed hands ===
            SetRect(px, size, 24, 136, 32, 36, eyes);
            SetRect(px, size, 328, 136, 32, 36, eyes);
            for (int i = 0; i < 4; i++)
            {
                SetRect(px, size, 24 + i * 7, 120, 5, 16, eyes);
                SetRect(px, size, 26 + i * 7, 116, 3, 4, Lighten(eyes, 0.25f));
                SetRect(px, size, 328 + i * 7, 120, 5, 16, eyes);
                SetRect(px, size, 330 + i * 7, 116, 3, 4, Lighten(eyes, 0.25f));
            }

            // === Head / Crown (y=312..384) ===
            SetRect(px, size, 136, 312, 112, 72, secondary);
            SetRect(px, size, 136, 312, 8, 72, secLt);
            SetRect(px, size, 240, 312, 8, 72, secDk);
            // Eyes (12x12 with pupils)
            SetRect(px, size, 156, 348, 20, 16, eyes);
            SetRect(px, size, 212, 348, 20, 16, eyes);
            SetRect(px, size, 162, 352, 8, 8, dark);
            SetRect(px, size, 218, 352, 8, 8, dark);
            // Brow ridge
            SetRect(px, size, 148, 364, 88, 4, secDk);
            // Mouth
            SetRect(px, size, 164, 328, 56, 12, dark);
            SetRect(px, size, 168, 330, 48, 8, Darken(dark, 0.80f));
            // Teeth
            for (int i = 0; i < 6; i++)
                SetRect(px, size, 170 + i * 8, 338, 4, 4, Lighten(eyes, 0.40f));
            // Crown points
            for (int i = 0; i < 7; i++)
            {
                int cx = 144 + i * 16;
                SetRect(px, size, cx, 376, 8, 8, eyes);
                SetRect(px, size, cx + 2, 380, 4, 4, Lighten(eyes, 0.30f));
            }

            // === Chest emblem ===
            SetRect(px, size, 152, 228, 80, 44, eyes);
            SetRect(px, size, 164, 236, 56, 28, Darken(eyes, 0.65f));
            SetRect(px, size, 176, 244, 32, 12, eyes);
            SetRect(px, size, 184, 248, 16, 4, Lighten(eyes, 0.35f));

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0f), bossPPU);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        #endregion

        #region UI Elements

        /// <summary>
        /// Creates a pixel art "EPOCH BREAKER" logo using blocky retro-style letters.
        /// Single line, large text for maximum impact.
        /// </summary>
        public static Sprite GetTitleLogoSprite()
        {
            const string key = "title_logo";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            // Logo dimensions: wide for single line, larger blocks
            int blockSize = 10;  // Large blocks for title
            int letterW = 5 * blockSize;
            int letterH = 7 * blockSize;
            int spacing = 8;
            int spaceWidth = blockSize * 3;  // Extra wide space between words

            // "EPOCH BREAKER" = 12 letters + 1 space
            // Total width: 12 letters + spacing + word gap
            int totalW = 12 * letterW + 11 * spacing + spaceWidth + 32;  // padding
            int totalH = letterH + 32;  // padding for shadow

            int w = totalW, h = totalH;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[w * h];

            // Colors - rich gold with depth
            Color gold = new Color(1f, 0.82f, 0.15f);
            Color goldHi = new Color(1f, 0.95f, 0.45f);
            Color goldDk = new Color(0.75f, 0.50f, 0.08f);
            Color shadow = new Color(0.12f, 0.08f, 0.20f);
            Color outline = new Color(0.35f, 0.18f, 0.05f);

            // Font patterns (5x7)
            int[,] fontE = { {1,1,1,1,1}, {1,0,0,0,0}, {1,1,1,1,0}, {1,0,0,0,0}, {1,0,0,0,0}, {1,0,0,0,0}, {1,1,1,1,1} };
            int[,] fontP = { {1,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {1,1,1,1,0}, {1,0,0,0,0}, {1,0,0,0,0}, {1,0,0,0,0} };
            int[,] fontO = { {0,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {0,1,1,1,0} };
            int[,] fontC = { {0,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,0}, {1,0,0,0,0}, {1,0,0,0,0}, {1,0,0,0,1}, {0,1,1,1,0} };
            int[,] fontH = { {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,1,1,1,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1} };
            int[,] fontB = { {1,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {1,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {1,1,1,1,0} };
            int[,] fontR = { {1,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {1,1,1,1,0}, {1,0,1,0,0}, {1,0,0,1,0}, {1,0,0,0,1} };
            int[,] fontA = { {0,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {1,1,1,1,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1} };
            int[,] fontK = { {1,0,0,0,1}, {1,0,0,1,0}, {1,0,1,0,0}, {1,1,0,0,0}, {1,0,1,0,0}, {1,0,0,1,0}, {1,0,0,0,1} };

            // "EPOCH BREAKER" as single line
            int[][,] title = { fontE, fontP, fontO, fontC, fontH, null, fontB, fontR, fontE, fontA, fontK, fontE, fontR };

            int x = 16;  // Left padding
            int y = 12;  // Bottom padding

            for (int i = 0; i < title.Length; i++)
            {
                if (title[i] == null)
                {
                    // Space between words
                    x += spaceWidth;
                }
                else
                {
                    DrawBlockLetter(px, w, h, title[i], x, y, blockSize, gold, goldHi, goldDk, shadow, outline);
                    x += letterW + spacing;
                }
            }

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 1f);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        private static void DrawBlockLetter(Color[] px, int texW, int texH, int[,] pattern,
            int startX, int startY, int blockSize, Color main, Color hi, Color dk, Color shadow, Color outline)
        {
            int fontH = pattern.GetLength(0);
            int fontW = pattern.GetLength(1);

            for (int fy = 0; fy < fontH; fy++)
            {
                for (int fx = 0; fx < fontW; fx++)
                {
                    if (pattern[fy, fx] == 0) continue;

                    int bx = startX + fx * blockSize;
                    int by = startY + (fontH - 1 - fy) * blockSize;  // Flip Y for bottom-up

                    // Draw shadow (offset down-right)
                    SetRect(px, texW, bx + 3, by - 3, blockSize, blockSize, shadow);

                    // Draw outline
                    SetRect(px, texW, bx - 1, by - 1, blockSize + 2, blockSize + 2, outline);

                    // Draw main block with gradient
                    SetRect(px, texW, bx, by, blockSize, blockSize, main);

                    // Top highlight
                    SetRect(px, texW, bx, by + blockSize - 2, blockSize, 2, hi);

                    // Left highlight
                    SetRect(px, texW, bx, by, 2, blockSize, Lighten(main, 0.15f));

                    // Bottom shadow
                    SetRect(px, texW, bx, by, blockSize, 2, dk);

                    // Right shadow
                    SetRect(px, texW, bx + blockSize - 2, by, 2, blockSize, dk);

                    // Inner shine pixel
                    Px(px, texW, bx + 2, by + blockSize - 3, Lighten(hi, 0.3f));
                }
            }
        }

        // 5x7 pixel font patterns for all needed characters
        private static readonly Dictionary<char, int[,]> _pixelFont = new Dictionary<char, int[,]>
        {
            ['A'] = new int[,] { {0,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {1,1,1,1,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1} },
            ['B'] = new int[,] { {1,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {1,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {1,1,1,1,0} },
            ['C'] = new int[,] { {0,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,0}, {1,0,0,0,0}, {1,0,0,0,0}, {1,0,0,0,1}, {0,1,1,1,0} },
            ['D'] = new int[,] { {1,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,1,1,1,0} },
            ['E'] = new int[,] { {1,1,1,1,1}, {1,0,0,0,0}, {1,0,0,0,0}, {1,1,1,1,0}, {1,0,0,0,0}, {1,0,0,0,0}, {1,1,1,1,1} },
            ['F'] = new int[,] { {1,1,1,1,1}, {1,0,0,0,0}, {1,0,0,0,0}, {1,1,1,1,0}, {1,0,0,0,0}, {1,0,0,0,0}, {1,0,0,0,0} },
            ['G'] = new int[,] { {0,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,0}, {1,0,1,1,1}, {1,0,0,0,1}, {1,0,0,0,1}, {0,1,1,1,0} },
            ['H'] = new int[,] { {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,1,1,1,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1} },
            ['I'] = new int[,] { {1,1,1,1,1}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {1,1,1,1,1} },
            ['J'] = new int[,] { {0,0,1,1,1}, {0,0,0,1,0}, {0,0,0,1,0}, {0,0,0,1,0}, {0,0,0,1,0}, {1,0,0,1,0}, {0,1,1,0,0} },
            ['K'] = new int[,] { {1,0,0,0,1}, {1,0,0,1,0}, {1,0,1,0,0}, {1,1,0,0,0}, {1,0,1,0,0}, {1,0,0,1,0}, {1,0,0,0,1} },
            ['L'] = new int[,] { {1,0,0,0,0}, {1,0,0,0,0}, {1,0,0,0,0}, {1,0,0,0,0}, {1,0,0,0,0}, {1,0,0,0,0}, {1,1,1,1,1} },
            ['M'] = new int[,] { {1,0,0,0,1}, {1,1,0,1,1}, {1,0,1,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1} },
            ['N'] = new int[,] { {1,0,0,0,1}, {1,1,0,0,1}, {1,0,1,0,1}, {1,0,0,1,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1} },
            ['O'] = new int[,] { {0,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {0,1,1,1,0} },
            ['P'] = new int[,] { {1,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {1,1,1,1,0}, {1,0,0,0,0}, {1,0,0,0,0}, {1,0,0,0,0} },
            ['Q'] = new int[,] { {0,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,1,0,1}, {1,0,0,1,0}, {0,1,1,0,1} },
            ['R'] = new int[,] { {1,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {1,1,1,1,0}, {1,0,1,0,0}, {1,0,0,1,0}, {1,0,0,0,1} },
            ['S'] = new int[,] { {0,1,1,1,1}, {1,0,0,0,0}, {1,0,0,0,0}, {0,1,1,1,0}, {0,0,0,0,1}, {0,0,0,0,1}, {1,1,1,1,0} },
            ['T'] = new int[,] { {1,1,1,1,1}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0} },
            ['U'] = new int[,] { {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {0,1,1,1,0} },
            ['V'] = new int[,] { {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {0,1,0,1,0}, {0,1,0,1,0}, {0,0,1,0,0} },
            ['W'] = new int[,] { {1,0,0,0,1}, {1,0,0,0,1}, {1,0,0,0,1}, {1,0,1,0,1}, {1,0,1,0,1}, {1,1,0,1,1}, {1,0,0,0,1} },
            ['X'] = new int[,] { {1,0,0,0,1}, {0,1,0,1,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,1,0,1,0}, {1,0,0,0,1} },
            ['Y'] = new int[,] { {1,0,0,0,1}, {0,1,0,1,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0} },
            ['Z'] = new int[,] { {1,1,1,1,1}, {0,0,0,0,1}, {0,0,0,1,0}, {0,0,1,0,0}, {0,1,0,0,0}, {1,0,0,0,0}, {1,1,1,1,1} },
            ['0'] = new int[,] { {0,1,1,1,0}, {1,0,0,1,1}, {1,0,1,0,1}, {1,0,1,0,1}, {1,1,0,0,1}, {1,0,0,0,1}, {0,1,1,1,0} },
            ['1'] = new int[,] { {0,0,1,0,0}, {0,1,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,1,1,1,0} },
            ['2'] = new int[,] { {0,1,1,1,0}, {1,0,0,0,1}, {0,0,0,0,1}, {0,0,1,1,0}, {0,1,0,0,0}, {1,0,0,0,0}, {1,1,1,1,1} },
            ['3'] = new int[,] { {0,1,1,1,0}, {1,0,0,0,1}, {0,0,0,0,1}, {0,0,1,1,0}, {0,0,0,0,1}, {1,0,0,0,1}, {0,1,1,1,0} },
            ['4'] = new int[,] { {0,0,0,1,0}, {0,0,1,1,0}, {0,1,0,1,0}, {1,0,0,1,0}, {1,1,1,1,1}, {0,0,0,1,0}, {0,0,0,1,0} },
            ['5'] = new int[,] { {1,1,1,1,1}, {1,0,0,0,0}, {1,1,1,1,0}, {0,0,0,0,1}, {0,0,0,0,1}, {1,0,0,0,1}, {0,1,1,1,0} },
            ['6'] = new int[,] { {0,0,1,1,0}, {0,1,0,0,0}, {1,0,0,0,0}, {1,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {0,1,1,1,0} },
            ['7'] = new int[,] { {1,1,1,1,1}, {0,0,0,0,1}, {0,0,0,1,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0} },
            ['8'] = new int[,] { {0,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {0,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {0,1,1,1,0} },
            ['9'] = new int[,] { {0,1,1,1,0}, {1,0,0,0,1}, {1,0,0,0,1}, {0,1,1,1,1}, {0,0,0,0,1}, {0,0,0,1,0}, {0,1,1,0,0} },
            [' '] = new int[,] { {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0} },
            [':'] = new int[,] { {0,0,0,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,0,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,0,0,0} },
            ['-'] = new int[,] { {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {1,1,1,1,1}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0} },
            ['/'] = new int[,] { {0,0,0,0,1}, {0,0,0,0,1}, {0,0,0,1,0}, {0,0,1,0,0}, {0,1,0,0,0}, {1,0,0,0,0}, {1,0,0,0,0} },
            ['|'] = new int[,] { {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0} },
            ['['] = new int[,] { {0,1,1,1,0}, {0,1,0,0,0}, {0,1,0,0,0}, {0,1,0,0,0}, {0,1,0,0,0}, {0,1,0,0,0}, {0,1,1,1,0} },
            [']'] = new int[,] { {0,1,1,1,0}, {0,0,0,1,0}, {0,0,0,1,0}, {0,0,0,1,0}, {0,0,0,1,0}, {0,0,0,1,0}, {0,1,1,1,0} },
            ['.'] = new int[,] { {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,0,0,0}, {0,0,1,0,0}, {0,0,1,0,0} },
        };

        /// <summary>
        /// Creates a pixel art text sprite for UI display.
        /// </summary>
        public static Sprite GetPixelTextSprite(string text, Color color, int scale = 2)
        {
            text = text.ToUpper();
            string key = $"pixeltext_{text}_{ColorToHex(color)}_{scale}";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            int charW = 5 * scale;
            int charH = 7 * scale;
            int spacing = scale;
            int padding = scale * 2;

            int totalW = padding * 2 + text.Length * (charW + spacing) - spacing;
            int totalH = padding * 2 + charH;

            var tex = new Texture2D(totalW, totalH, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[totalW * totalH];

            Color hi = Lighten(color, 0.3f);
            Color dk = Darken(color, 0.7f);

            int x = padding;
            foreach (char c in text)
            {
                if (_pixelFont.TryGetValue(c, out var pattern))
                {
                    DrawPixelChar(px, totalW, pattern, x, padding, scale, color, hi, dk);
                }
                x += charW + spacing;
            }

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, totalW, totalH), new Vector2(0.5f, 0.5f), 1f);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        private static void DrawPixelChar(Color[] px, int texW, int[,] pattern, int startX, int startY, int scale,
            Color main, Color hi, Color dk)
        {
            int fontH = pattern.GetLength(0);
            int fontW = pattern.GetLength(1);

            for (int fy = 0; fy < fontH; fy++)
            {
                for (int fx = 0; fx < fontW; fx++)
                {
                    if (pattern[fy, fx] == 0) continue;

                    int bx = startX + fx * scale;
                    int by = startY + (fontH - 1 - fy) * scale;

                    // Draw scaled pixel block
                    SetRect(px, texW, bx, by, scale, scale, main);

                    // Subtle highlight on top edge
                    if (scale >= 2)
                    {
                        SetRect(px, texW, bx, by + scale - 1, scale, 1, hi);
                        SetRect(px, texW, bx, by, scale, 1, dk);
                    }
                }
            }
        }

        private static string ColorToHex(Color c)
        {
            return $"{(int)(c.r * 255):X2}{(int)(c.g * 255):X2}{(int)(c.b * 255):X2}";
        }

        /// <summary>
        /// Creates an ornate pixel art frame for the title screen border.
        /// Uses 9-slice pattern: corners and edges that tile/stretch.
        /// </summary>
        public static Sprite GetOrnateFrameSprite()
        {
            const string key = "ornate_frame";
            if (_cache.TryGetValue(key, out var cached)) return cached;

            // Frame dimensions to match 1920x1080 reference at scale
            int w = 960, h = 540;
            int borderW = 32;  // Width of the ornate border
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[w * h];

            // Frame colors (gold/bronze ornate look)
            Color frameOuter = new Color(0.35f, 0.25f, 0.12f);
            Color frameMid = new Color(0.65f, 0.50f, 0.20f);
            Color frameInner = new Color(0.85f, 0.70f, 0.30f);
            Color frameHi = new Color(1f, 0.90f, 0.55f);
            Color frameDk = new Color(0.25f, 0.18f, 0.08f);
            Color gem = new Color(0.3f, 0.6f, 0.9f);
            Color gemHi = new Color(0.6f, 0.85f, 1f);

            // Draw outer edge (dark border)
            SetRect(px, w, 0, 0, w, borderW, frameOuter);           // bottom
            SetRect(px, w, 0, h - borderW, w, borderW, frameOuter); // top
            SetRect(px, w, 0, 0, borderW, h, frameOuter);           // left
            SetRect(px, w, w - borderW, 0, borderW, h, frameOuter); // right

            // Draw mid frame layer
            int m = 4;
            SetRect(px, w, m, m, w - m * 2, borderW - m * 2, frameMid);
            SetRect(px, w, m, h - borderW + m, w - m * 2, borderW - m * 2, frameMid);
            SetRect(px, w, m, m, borderW - m * 2, h - m * 2, frameMid);
            SetRect(px, w, w - borderW + m, m, borderW - m * 2, h - m * 2, frameMid);

            // Inner highlight line
            int i = 8;
            SetRect(px, w, i, i, w - i * 2, 2, frameHi);
            SetRect(px, w, i, h - i - 2, w - i * 2, 2, frameHi);
            SetRect(px, w, i, i, 2, h - i * 2, frameHi);
            SetRect(px, w, w - i - 2, i, 2, h - i * 2, frameHi);

            // Inner shadow line
            SetRect(px, w, borderW - 4, borderW - 4, w - (borderW - 4) * 2, 2, frameDk);
            SetRect(px, w, borderW - 4, h - borderW + 2, w - (borderW - 4) * 2, 2, frameDk);
            SetRect(px, w, borderW - 4, borderW - 4, 2, h - (borderW - 4) * 2, frameDk);
            SetRect(px, w, w - borderW + 2, borderW - 4, 2, h - (borderW - 4) * 2, frameDk);

            // Corner ornaments (decorative squares with gems)
            DrawCornerOrnament(px, w, h, 0, 0, borderW, frameInner, frameHi, frameDk, gem, gemHi);                    // bottom-left
            DrawCornerOrnament(px, w, h, w - borderW, 0, borderW, frameInner, frameHi, frameDk, gem, gemHi);          // bottom-right
            DrawCornerOrnament(px, w, h, 0, h - borderW, borderW, frameInner, frameHi, frameDk, gem, gemHi);          // top-left
            DrawCornerOrnament(px, w, h, w - borderW, h - borderW, borderW, frameInner, frameHi, frameDk, gem, gemHi);// top-right

            // Edge decorations (repeating pattern along borders)
            int gemSpacing = 80;
            // Top edge gems
            for (int gx = borderW + gemSpacing / 2; gx < w - borderW; gx += gemSpacing)
                DrawEdgeGem(px, w, gx, h - borderW / 2, gem, gemHi, frameDk);
            // Bottom edge gems
            for (int gx = borderW + gemSpacing / 2; gx < w - borderW; gx += gemSpacing)
                DrawEdgeGem(px, w, gx, borderW / 2, gem, gemHi, frameDk);
            // Left edge gems
            for (int gy = borderW + gemSpacing / 2; gy < h - borderW; gy += gemSpacing)
                DrawEdgeGem(px, w, borderW / 2, gy, gem, gemHi, frameDk);
            // Right edge gems
            for (int gy = borderW + gemSpacing / 2; gy < h - borderW; gy += gemSpacing)
                DrawEdgeGem(px, w, w - borderW / 2, gy, gem, gemHi, frameDk);

            // Filigree pattern on frame edges
            for (int fx = borderW + 20; fx < w - borderW - 10; fx += 40)
            {
                DrawFiligree(px, w, fx, 12, frameHi, frameDk);
                DrawFiligree(px, w, fx, h - 20, frameHi, frameDk);
            }
            for (int fy = borderW + 20; fy < h - borderW - 10; fy += 40)
            {
                DrawFiligreeVert(px, w, 12, fy, frameHi, frameDk);
                DrawFiligreeVert(px, w, w - 20, fy, frameHi, frameDk);
            }

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 0.5f);
            sprite.name = key;
            _cache[key] = sprite;
            return sprite;
        }

        private static void DrawCornerOrnament(Color[] px, int texW, int texH, int cx, int cy, int size,
            Color main, Color hi, Color dk, Color gem, Color gemHi)
        {
            // Ornate corner with layered squares and central gem
            int s = size;
            SetRect(px, texW, cx + 2, cy + 2, s - 4, s - 4, main);

            // Diagonal line pattern
            for (int d = 4; d < s - 4; d += 4)
            {
                Px(px, texW, cx + d, cy + d, hi);
                Px(px, texW, cx + s - d - 1, cy + d, hi);
            }

            // Central gem
            int gc = s / 2;
            SetRect(px, texW, cx + gc - 4, cy + gc - 4, 8, 8, gem);
            SetRect(px, texW, cx + gc - 3, cy + gc - 3, 6, 6, gemHi);
            SetRect(px, texW, cx + gc - 2, cy + gc + 1, 4, 2, gem);
            Px(px, texW, cx + gc - 1, cy + gc + 2, Lighten(gemHi, 0.5f));

            // Corner highlight
            SetRect(px, texW, cx + 2, cy + s - 4, s - 4, 2, hi);
            SetRect(px, texW, cx + 2, cy + 2, 2, s - 4, hi);
        }

        private static void DrawEdgeGem(Color[] px, int texW, int x, int y, Color gem, Color hi, Color dk)
        {
            // Small diamond gem
            Px(px, texW, x, y + 2, gem);
            Px(px, texW, x, y - 2, gem);
            Px(px, texW, x - 2, y, gem);
            Px(px, texW, x + 2, y, gem);
            SetRect(px, texW, x - 1, y - 1, 3, 3, gem);
            Px(px, texW, x, y, hi);
            Px(px, texW, x - 1, y + 1, hi);
        }

        private static void DrawFiligree(Color[] px, int texW, int x, int y, Color hi, Color dk)
        {
            // Simple horizontal scroll pattern
            Px(px, texW, x, y, hi);
            Px(px, texW, x + 1, y + 1, hi);
            Px(px, texW, x + 2, y, hi);
            Px(px, texW, x + 3, y - 1, hi);
            Px(px, texW, x + 4, y, hi);
            Px(px, texW, x + 5, y + 1, hi);
            Px(px, texW, x + 6, y, hi);
        }

        private static void DrawFiligreeVert(Color[] px, int texW, int x, int y, Color hi, Color dk)
        {
            // Simple vertical scroll pattern
            Px(px, texW, x, y, hi);
            Px(px, texW, x + 1, y + 1, hi);
            Px(px, texW, x, y + 2, hi);
            Px(px, texW, x - 1, y + 3, hi);
            Px(px, texW, x, y + 4, hi);
            Px(px, texW, x + 1, y + 5, hi);
            Px(px, texW, x, y + 6, hi);
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
