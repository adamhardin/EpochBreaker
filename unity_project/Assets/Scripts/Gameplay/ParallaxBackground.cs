using UnityEngine;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// 5-layer parallax scrolling background. Each layer scrolls at
    /// a different rate relative to the camera for depth illusion.
    /// Sprites are procedurally generated per epoch.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        private const int LAYER_COUNT = 5;
        private const float BASE_WIDTH = 40f; // Width of each background tile

        private Transform _cameraTransform;
        private ParallaxLayer[] _layers;
        private float _lastCameraX;

        private struct ParallaxLayer
        {
            public float ScrollFactor; // 0 = static, 1 = moves with camera
            public SpriteRenderer[] Tiles;
            public float TileWidth;
        }

        public void Initialize(Transform cameraTransform, int epoch)
        {
            _cameraTransform = cameraTransform;
            _lastCameraX = cameraTransform.position.x;

            _layers = new ParallaxLayer[LAYER_COUNT];

            // Layer 0: Sky/gradient (near-static)
            // Layer 1: Far mountains/structures (very slow)
            // Layer 2: Mid-ground buildings/ruins (slow)
            // Layer 3: Near buildings/trees (medium)
            // Layer 4: Foreground debris/fog (fast)
            float[] scrollFactors = { 0.02f, 0.05f, 0.15f, 0.3f, 0.5f };
            int[] sortOrders = { -50, -40, -30, -20, -10 };
            float[] scales = { 1.0f, 0.8f, 0.7f, 0.6f, 0.4f };

            var eraColors = GetEraBackgroundColors(epoch);

            for (int i = 0; i < LAYER_COUNT; i++)
            {
                var layerGO = new GameObject($"ParallaxLayer_{i}");
                layerGO.transform.SetParent(transform);
                layerGO.transform.localPosition = Vector3.zero;

                _layers[i] = new ParallaxLayer
                {
                    ScrollFactor = scrollFactors[i],
                    TileWidth = BASE_WIDTH,
                    Tiles = new SpriteRenderer[3] // 3 tiles for seamless scrolling
                };

                for (int t = 0; t < 3; t++)
                {
                    var tileGO = new GameObject($"Tile_{t}");
                    tileGO.transform.SetParent(layerGO.transform);
                    tileGO.transform.localPosition = new Vector3((t - 1) * BASE_WIDTH, 0, 0);

                    var sr = tileGO.AddComponent<SpriteRenderer>();
                    sr.sprite = GenerateLayerSprite(i, epoch, t);
                    sr.sortingOrder = sortOrders[i];
                    sr.color = eraColors[i];
                    tileGO.transform.localScale = new Vector3(1f, scales[i], 1f);

                    // Center vertically at camera height
                    tileGO.transform.localPosition += Vector3.up * (7f + i * 0.5f);

                    _layers[i].Tiles[t] = sr;
                }
            }
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null || _layers == null) return;

            float cameraX = _cameraTransform.position.x;
            float cameraY = _cameraTransform.position.y;

            for (int i = 0; i < LAYER_COUNT; i++)
            {
                ref var layer = ref _layers[i];
                float parallaxX = cameraX * layer.ScrollFactor;

                // Position the layer parent relative to parallax offset
                var parent = layer.Tiles[0].transform.parent;
                var pos = parent.position;
                pos.x = cameraX - parallaxX;
                pos.y = cameraY * (layer.ScrollFactor * 0.3f); // Subtle vertical parallax
                parent.position = pos;

                // Tile wrapping: reposition tiles that scroll off-screen
                for (int t = 0; t < layer.Tiles.Length; t++)
                {
                    var tile = layer.Tiles[t].transform;
                    float relativeX = tile.position.x - cameraX;

                    if (relativeX < -BASE_WIDTH * 1.5f)
                        tile.position += Vector3.right * (BASE_WIDTH * 3);
                    else if (relativeX > BASE_WIDTH * 1.5f)
                        tile.position -= Vector3.right * (BASE_WIDTH * 3);
                }
            }
        }

        private static Sprite GenerateLayerSprite(int layer, int epoch, int variation)
        {
            // Generate a simple procedural background sprite per layer
            int width = 640; // 10 tiles wide at PPU=64
            int height = 256;
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            var pixels = new Color32[width * height];
            var rng = new Generative.XORShift64((ulong)(epoch * 100 + layer * 10 + variation + 1));

            // Fill with transparent
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(0, 0, 0, 0);

            switch (layer)
            {
                case 0: // Sky gradient
                    FillSkyGradient(pixels, width, height, epoch);
                    break;
                case 1: // Far silhouettes (mountains/pyramids/towers)
                    DrawFarSilhouettes(pixels, width, height, epoch, rng);
                    break;
                case 2: // Mid structures
                    DrawMidStructures(pixels, width, height, epoch, rng);
                    break;
                case 3: // Near structures
                    DrawNearStructures(pixels, width, height, epoch, rng);
                    break;
                case 4: // Foreground particles/fog
                    DrawForegroundFog(pixels, width, height, epoch, rng);
                    break;
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 16f);
        }

        private static void FillSkyGradient(Color32[] pixels, int w, int h, int epoch)
        {
            var top = GetSkyTopColor(epoch);
            var bottom = GetSkyBottomColor(epoch);

            for (int y = 0; y < h; y++)
            {
                float t = (float)y / h;
                byte r = (byte)Mathf.Lerp(bottom.r, top.r, t);
                byte g = (byte)Mathf.Lerp(bottom.g, top.g, t);
                byte b = (byte)Mathf.Lerp(bottom.b, top.b, t);
                var color = new Color32(r, g, b, 180);

                for (int x = 0; x < w; x++)
                    pixels[y * w + x] = color;
            }
        }

        private static void DrawFarSilhouettes(Color32[] pixels, int w, int h, int epoch, Generative.XORShift64 rng)
        {
            var color = new Color32(20, 15, 25, 120);
            int peakCount = 4 + (int)(rng.Next() % 4);

            for (int p = 0; p < peakCount; p++)
            {
                int cx = (int)(rng.Next() % (ulong)w);
                int peakH = h / 3 + (int)(rng.Next() % (ulong)(h / 3));
                int halfW = 40 + (int)(rng.Next() % 60);

                for (int x = cx - halfW; x <= cx + halfW; x++)
                {
                    if (x < 0 || x >= w) continue;
                    float dist = Mathf.Abs(x - cx) / (float)halfW;
                    int colH = (int)(peakH * (1f - dist * dist));

                    for (int y = 0; y < colH && y < h; y++)
                        pixels[y * w + x] = color;
                }
            }
        }

        private static void DrawMidStructures(Color32[] pixels, int w, int h, int epoch, Generative.XORShift64 rng)
        {
            var color = new Color32(30, 25, 35, 100);
            int count = 3 + (int)(rng.Next() % 3);

            for (int s = 0; s < count; s++)
            {
                int x0 = (int)(rng.Next() % (ulong)w);
                int bw = 20 + (int)(rng.Next() % 30);
                int bh = h / 4 + (int)(rng.Next() % (ulong)(h / 4));

                for (int x = x0; x < x0 + bw && x < w; x++)
                    for (int y = 0; y < bh && y < h; y++)
                        pixels[y * w + x] = color;
            }
        }

        private static void DrawNearStructures(Color32[] pixels, int w, int h, int epoch, Generative.XORShift64 rng)
        {
            var color = new Color32(40, 35, 45, 80);
            int count = 2 + (int)(rng.Next() % 3);

            for (int s = 0; s < count; s++)
            {
                int x0 = (int)(rng.Next() % (ulong)w);
                int bw = 15 + (int)(rng.Next() % 25);
                int bh = h / 5 + (int)(rng.Next() % (ulong)(h / 5));

                for (int x = x0; x < x0 + bw && x < w; x++)
                    for (int y = 0; y < bh && y < h; y++)
                        pixels[y * w + x] = color;
            }
        }

        private static void DrawForegroundFog(Color32[] pixels, int w, int h, int epoch, Generative.XORShift64 rng)
        {
            // Scattered fog dots
            int dotCount = 30 + (int)(rng.Next() % 20);
            for (int d = 0; d < dotCount; d++)
            {
                int x = (int)(rng.Next() % (ulong)w);
                int y = (int)(rng.Next() % (ulong)(h / 3)); // Bottom third
                int r = 2 + (int)(rng.Next() % 4);
                byte alpha = (byte)(30 + rng.Next() % 40);

                for (int dx = -r; dx <= r; dx++)
                    for (int dy = -r; dy <= r; dy++)
                    {
                        if (dx * dx + dy * dy > r * r) continue;
                        int px = x + dx, py = y + dy;
                        if (px >= 0 && px < w && py >= 0 && py < h)
                            pixels[py * w + px] = new Color32(200, 200, 210, alpha);
                    }
            }
        }

        private static Color32 GetSkyTopColor(int epoch)
        {
            return epoch switch
            {
                0 => new Color32(10, 5, 20, 255),    // Prehistoric: dark sky
                1 => new Color32(25, 15, 10, 255),    // Egyptian: warm dark
                2 => new Color32(20, 20, 40, 255),    // Greek: blue-dark
                3 => new Color32(15, 10, 25, 255),    // Roman: purple-dark
                4 => new Color32(10, 15, 20, 255),    // Medieval: grey-dark
                5 => new Color32(30, 15, 10, 255),    // Renaissance: warm
                6 => new Color32(20, 20, 20, 255),    // Industrial: grey
                7 => new Color32(5, 5, 15, 255),      // Modern: deep blue
                8 => new Color32(10, 5, 20, 255),     // Digital: purple
                9 => new Color32(5, 10, 25, 255),     // Future: cyan-dark
                _ => new Color32(10, 10, 20, 255),
            };
        }

        private static Color32 GetSkyBottomColor(int epoch)
        {
            return epoch switch
            {
                0 => new Color32(40, 25, 15, 255),    // Prehistoric: warm horizon
                1 => new Color32(60, 35, 15, 255),    // Egyptian: sandy horizon
                2 => new Color32(35, 30, 50, 255),    // Greek: blue horizon
                3 => new Color32(35, 20, 40, 255),    // Roman: purple horizon
                4 => new Color32(30, 30, 25, 255),    // Medieval: grey horizon
                5 => new Color32(50, 30, 15, 255),    // Renaissance: golden
                6 => new Color32(35, 30, 25, 255),    // Industrial: smoggy
                7 => new Color32(15, 15, 30, 255),    // Modern: city glow
                8 => new Color32(20, 10, 35, 255),    // Digital: neon
                9 => new Color32(10, 20, 40, 255),    // Future: electric blue
                _ => new Color32(30, 25, 20, 255),
            };
        }

        private static Color[] GetEraBackgroundColors(int epoch)
        {
            // Per-layer tint colors (subtle, low alpha since sprite already has alpha)
            return new Color[]
            {
                Color.white,                                    // Sky: no tint
                new Color(0.8f, 0.8f, 0.9f, 1f),              // Far: slight blue
                new Color(0.85f, 0.85f, 0.9f, 1f),            // Mid: slight blue
                new Color(0.9f, 0.9f, 0.95f, 1f),             // Near: very slight
                new Color(1f, 1f, 1f, 0.6f),                  // Foreground: semi-transparent
            };
        }
    }
}
