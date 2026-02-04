# Assessment 6.5: Parallax Scrolling System Specification

## Overview

Complete parallax scrolling specification defining 5 depth layers per biome for Forest and Cavern environments. Each layer has defined scroll speed multipliers, texture dimensions, color palettes, and memory budgets. The system creates a convincing sense of depth using 16-bit era techniques while meeting modern mobile performance requirements on iOS.

---

## Technical Constraints

| Parameter | Value |
|-----------|-------|
| Target Framerate | 60fps (parallax must never cause frame drops) |
| Maximum Layers | 5 per biome (6 with optional foreground overlay) |
| Scroll Direction | Horizontal only (vertical parallax on sky layer optional) |
| Texture Format | PNG, power-of-two widths for seamless tiling |
| Texture Filter Mode | Point (no interpolation -- pixel art integrity) |
| Pixel Art Scale | Integer scaling only (1x, 2x, 3x) -- no fractional scaling |
| Memory Budget | Total parallax textures < 1 MB per biome (uncompressed in VRAM) |
| Color Depth | 8-bit indexed (256 color palette) or 32-bit RGBA (Unity standard) |
| Scroll Precision | Sub-pixel rendering via shader offset (avoid per-pixel jitter) |
| Camera Reference | Gameplay layer (Layer 5) is locked 1:1 to camera. All other layers are multiples. |

---

## Layer Architecture

### Universal Layer Stack (Back to Front)

| Layer | Depth Name | Scroll Speed | Sorting Order | Description |
|-------|-----------|-------------|---------------|-------------|
| 1 | Sky / Deep Background | 0.05x | -400 | Farthest element. Nearly static. Sky gradient, deep cave darkness. |
| 2 | Far Background | 0.20x | -300 | Distant environmental features. Mountains, far stalactites. |
| 3 | Mid Background | 0.40x | -200 | Medium-distance elements. Trees, crystal formations. |
| 4 | Near Background | 0.70x | -100 | Close background detail. Near foliage, rock pillars. |
| 5 | Gameplay | 1.00x | 0 | Primary gameplay layer. Tiles, characters, enemies, pickups. Camera-locked. |
| 6 (optional) | Foreground Overlay | 1.20x | +100 | Closest elements that pass in front of gameplay. Leaves, fog, particles. Use sparingly -- can occlude gameplay. |

### Scroll Speed Formula

```
layer_position_x = camera_position_x * scroll_multiplier
```

Where `scroll_multiplier` < 1.0 makes the layer move slower than the camera (appears farther away), and `scroll_multiplier` > 1.0 makes it move faster (appears closer).

For seamless infinite scrolling, each layer texture wraps horizontally:

```
// UV offset for seamless tiling
float uvOffset = (camera_position_x * scroll_multiplier) / texture_width;
material.SetFloat("_OffsetX", uvOffset % 1.0f);
```

---

## Forest Biome Parallax

### Layer 1: Sky (0.05x)

| Parameter | Value |
|-----------|-------|
| Scroll Speed | 0.05x camera movement |
| Texture Width | 256px |
| Texture Height | 224px (full screen height at target resolution) |
| Memory | 256 x 224 x 4 bytes = ~224 KB (RGBA) |
| Tiling | Horizontal seamless (left edge matches right edge) |
| Vertical Scroll | None (fixed vertical position) |
| Content | Smooth gradient from light blue at top to warm white-blue at horizon. |

**Color Palette (8 colors):**

| Slot | Name | Hex Code | Vertical Position |
|------|------|----------|-------------------|
| 0 | Deep Sky | #4A90D9 | Top 15% of texture |
| 1 | Sky Upper | #6DB3D3 | 15-30% |
| 2 | Sky Mid | #87CEEB | 30-50% |
| 3 | Sky Lower | #A8D8EA | 50-65% |
| 4 | Horizon Warm | #C5E1A5 | 65-75% |
| 5 | Horizon Glow | #F0F4C3 | 75-85% |
| 6 | Cloud White | #FFFFFF | Scattered cloud shapes |
| 7 | Cloud Shadow | #D0E0F0 | Cloud undersides |

**Art Direction:**
- Upper 60%: Smooth vertical color gradient using dithering between sky colors. No hard bands -- use 2x2 dither patterns to blend adjacent colors.
- Lower 40%: Warmer horizon tones suggesting late-afternoon warmth.
- Clouds: 2-3 small, simple cloud shapes (10-15px wide, 4-6px tall) placed in the upper-mid region. Puffy shapes with flat bottoms. White tops, shadow undersides. The 0.05x scroll speed means clouds barely move, creating a serene backdrop.
- No sun/moon (these would be separate game objects if needed).

---

### Layer 2: Distant Hills (0.20x)

| Parameter | Value |
|-----------|-------|
| Scroll Speed | 0.20x camera movement |
| Texture Width | 512px |
| Texture Height | 96px (lower 43% of screen) |
| Memory | 512 x 96 x 4 bytes = ~192 KB |
| Tiling | Horizontal seamless |
| Vertical Position | Anchored to bottom of screen, overlapping lower portion of sky |
| Content | Rolling hills silhouette with distant tree canopy suggestions. |

**Color Palette (6 colors):**

| Slot | Name | Hex Code | Usage |
|------|------|----------|-------|
| 0 | Transparent | N/A | Above hill silhouette |
| 1 | Hill Dark | #3E6B35 | Deepest shadow of hills (desaturated, dark green) |
| 2 | Hill Mid | #558B2F | Main hill body |
| 3 | Hill Light | #7CB342 | Hill highlights, sunlit tops |
| 4 | Tree Silhouette Dark | #2E5128 | Distant tree shapes on hilltops |
| 5 | Tree Silhouette Light | #4A7C3F | Tree highlight edges |

**Art Direction:**
- Two overlapping hill silhouettes at different heights creating a layered rolling landscape. Front hill peaks at ~60px, back hill peaks at ~40px.
- Hill contours: gentle, undulating curves. No sharp angles. Think smooth sine-wave-like terrain.
- Tree silhouettes on hilltops: simple triangular/conical shapes (3-5px wide, 4-6px tall) clustered in groups of 3-7. Not individual trees -- just a mass canopy suggestion.
- All colors are DESATURATED by 30% and DARKENED by 25% compared to gameplay-layer greens. This atmospheric perspective pushes them visually into the distance.
- Bottom edge of texture is fully opaque (hills cover the bottom). Top edge has irregular silhouette against transparent, revealing sky layer behind.

---

### Layer 3: Far Trees (0.40x)

| Parameter | Value |
|-----------|-------|
| Scroll Speed | 0.40x camera movement |
| Texture Width | 512px |
| Texture Height | 160px (lower 71% of screen) |
| Memory | 512 x 160 x 4 bytes = ~320 KB |
| Tiling | Horizontal seamless |
| Vertical Position | Anchored to bottom, overlapping hills |
| Content | Mid-distance tree line with visible trunks and canopy masses. |

**Color Palette (8 colors):**

| Slot | Name | Hex Code | Usage |
|------|------|----------|-------|
| 0 | Transparent | N/A | Above tree line |
| 1 | Trunk Dark | #3E2723 | Tree trunk shadow side |
| 2 | Trunk Mid | #5D4037 | Tree trunk body |
| 3 | Canopy Dark | #2E7D32 | Foliage deep shadow |
| 4 | Canopy Mid | #43A047 | Foliage main mass |
| 5 | Canopy Light | #66BB6A | Foliage sunlit highlights |
| 6 | Undergrowth | #33691E | Low bushes/ground cover between trees |
| 7 | Gap Dark | #1B3A1B | Dark gaps between tree trunks |

**Art Direction:**
- 4-6 distinct tree silhouettes of varying heights (80-140px tall from base to canopy top) spaced across 512px width.
- Tree trunks: 6-10px wide, visible bark texture (1px vertical lines alternating trunk colors). Slight lean/irregularity -- not perfectly vertical.
- Canopy: Large rounded masses (30-60px wide per tree). Overlapping canopies for dense forest feel. Use dithering between canopy colors for soft depth.
- Undergrowth fills gaps between tree bases. Low, horizontal, darker green.
- Colors are DESATURATED by 15% and DARKENED by 15% relative to gameplay foliage. Noticeably behind the player but not as faded as the hills.
- Occasional light gaps between trees show layers behind (transparent pixels between trunks).

---

### Layer 4: Near Trees (0.70x)

| Parameter | Value |
|-----------|-------|
| Scroll Speed | 0.70x camera movement |
| Texture Width | 512px |
| Texture Height | 192px (lower 86% of screen) |
| Memory | 512 x 192 x 4 bytes = ~384 KB |
| Tiling | Horizontal seamless |
| Vertical Position | Anchored to bottom, overlapping far trees |
| Content | Close background trees with detailed trunks and foliage. Partially frames the gameplay area. |

**Color Palette (10 colors):**

| Slot | Name | Hex Code | Usage |
|------|------|----------|-------|
| 0 | Transparent | N/A | Most of the texture (gameplay needs to show through) |
| 1 | Trunk Dark | #3E2723 | Trunk shadow |
| 2 | Trunk Mid | #6D4C41 | Trunk body |
| 3 | Trunk Light | #8D6E63 | Trunk highlight (bark detail) |
| 4 | Canopy Dark | #1B5E20 | Dense foliage shadow |
| 5 | Canopy Mid | #388E3C | Main foliage |
| 6 | Canopy Light | #4CAF50 | Sunlit foliage |
| 7 | Canopy Highlight | #81C784 | Brightest leaf highlights |
| 8 | Root/Moss | #5D4037 | Tree base, roots |
| 9 | Vine | #2E7D32 | Hanging vines from branches |

**Art Direction:**
- 2-3 large tree trunks spanning from bottom to top of texture. These are CLOSE trees, so they are large -- trunks 12-20px wide.
- IMPORTANT: This layer must be mostly transparent. Trees are placed at the LEFT and RIGHT edges of the 512px texture with large open gaps in between. The player and gameplay must be clearly visible. Trees frame the action, they do not obscure it.
- Canopy foliage hangs from above, creating an overhead framing effect. Leaves at the top ~30px of texture.
- Hanging vines: 1-2px wide, dropping 20-40px from canopy. Subtle, decorative.
- Visible roots at tree bases (gnarled, spreading 5-10px outward).
- Colors are at NEAR-FULL saturation. Only ~5% desaturated from gameplay-layer equivalents. This layer should feel close and vibrant.
- Parallax difference between this (0.7x) and gameplay (1.0x) is the most perceptible -- this is where the depth effect is most dramatic.

---

### Layer 5: Gameplay (1.00x)

| Parameter | Value |
|-----------|-------|
| Scroll Speed | 1.00x (locked to camera) |
| Texture | Tilemap (not a single parallax texture) |
| Memory | Part of tileset budget (see tileset_spec.md) |
| Content | All gameplay tiles, characters, enemies, pickups, hazards. |

This layer is not a parallax texture -- it is the Unity Tilemap and game object layer. Included here for completeness and ordering reference.

---

### Layer 6: Foreground Overlay (1.20x, Optional)

| Parameter | Value |
|-----------|-------|
| Scroll Speed | 1.20x camera movement |
| Texture Width | 256px |
| Texture Height | 64px |
| Memory | 256 x 64 x 4 bytes = ~64 KB |
| Tiling | Horizontal seamless |
| Vertical Position | Anchored to top of screen OR bottom of screen |
| Content | Drifting leaves, light dust motes, or subtle fog. |
| Opacity | 30-50% alpha -- must never obscure gameplay |

**Color Palette (4 colors):**

| Slot | Name | Hex Code | Usage |
|------|------|----------|-------|
| 0 | Transparent | N/A | Most of the texture |
| 1 | Leaf Orange | #FF8F00 | Falling leaf shapes (3-4px) |
| 2 | Leaf Brown | #8D6E63 | Alternate leaf color |
| 3 | Dust/Pollen | #FFF9C4 | Tiny 1-2px light particles |

**Art Direction:**
- Very sparse. 5-8 small leaf shapes and 10-15 dust motes scattered across 256x64px.
- All rendered at 30-50% alpha so gameplay is never obscured.
- The 1.2x scroll speed makes these elements pass in front of the camera faster than the gameplay, reinforcing the depth illusion.
- This layer is the FIRST to be disabled in reduced motion mode.

---

### Forest Memory Budget Summary

| Layer | Dimensions | RGBA Size | PNG Compressed (Est.) |
|-------|-----------|-----------|----------------------|
| L1: Sky | 256 x 224 | 224 KB | ~15 KB |
| L2: Hills | 512 x 96 | 192 KB | ~12 KB |
| L3: Far Trees | 512 x 160 | 320 KB | ~25 KB |
| L4: Near Trees | 512 x 192 | 384 KB | ~20 KB |
| L6: Overlay | 256 x 64 | 64 KB | ~4 KB |
| **Total** | -- | **1,184 KB** | **~76 KB** |

VRAM budget is 1,184 KB in RGBA. This is slightly over the 1 MB target. Optimization options:
- Reduce L3 or L4 width to 256px (saves 50% per layer).
- Use indexed-color textures (8-bit) instead of RGBA (75% savings) -- requires custom shader.
- Recommended: Reduce L1 height to 128px (sky does not need full screen coverage) -- saves 112 KB, bringing total to ~1,072 KB. OR reduce L4 width to 256px -- saves 192 KB, bringing total to 992 KB (under budget).

**Chosen Configuration (Under 1 MB):**

| Layer | Optimized Dimensions | RGBA Size |
|-------|---------------------|-----------|
| L1: Sky | 256 x 144 | 144 KB |
| L2: Hills | 512 x 96 | 192 KB |
| L3: Far Trees | 512 x 160 | 320 KB |
| L4: Near Trees | 256 x 192 | 192 KB |
| L6: Overlay | 256 x 64 | 64 KB |
| **Total** | -- | **912 KB** |

Under 1 MB budget. L4 reduced to 256px width (repeats more often, but at 0.7x scroll speed the repetition is less noticeable).

---

## Cavern Biome Parallax

### Layer 1: Dark Gradient (0.05x)

| Parameter | Value |
|-----------|-------|
| Scroll Speed | 0.05x camera movement |
| Texture Width | 128px |
| Texture Height | 144px |
| Memory | 128 x 144 x 4 = ~72 KB |
| Tiling | Horizontal seamless (effectively a solid gradient, tiles trivially) |
| Content | Deep dark gradient suggesting infinite cave depth behind the scene. |

**Color Palette (5 colors):**

| Slot | Name | Hex Code | Position |
|------|------|----------|----------|
| 0 | Void Black | #050510 | Top 25% (ceiling depth) |
| 1 | Deep Cave | #0A0A1E | 25-45% |
| 2 | Mid Cave | #12122E | 45-60% |
| 3 | Lower Cave | #1A1A3D | 60-80% |
| 4 | Ground Glow | #222248 | Bottom 20% (subtle ambient light from below) |

**Art Direction:**
- Smooth vertical gradient from near-black at top to very dark blue-purple at bottom.
- Use 2x2 dithering patterns between color bands to prevent hard color banding.
- Very subtle -- this layer is the "void" behind everything. Should feel like endless cave depth.
- Optional: 2-3 tiny pixel clusters (1-2px) in slightly lighter color scattered randomly, suggesting distant glowing minerals. Very subtle, barely visible.
- The 0.05x scroll means this is essentially static. The player will barely perceive its movement.

---

### Layer 2: Distant Stalactites (0.15x)

| Parameter | Value |
|-----------|-------|
| Scroll Speed | 0.15x camera movement |
| Texture Width | 512px |
| Texture Height | 144px |
| Memory | 512 x 144 x 4 = ~288 KB |
| Tiling | Horizontal seamless |
| Vertical Position | Full height coverage |
| Content | Distant stalactite and stalagmite silhouettes suggesting a vast cave chamber. |

**Color Palette (5 colors):**

| Slot | Name | Hex Code | Usage |
|------|------|----------|-------|
| 0 | Transparent | N/A | Open cave space |
| 1 | Rock Silhouette Dark | #151530 | Darkest rock mass |
| 2 | Rock Silhouette Mid | #1E1E42 | Main rock body |
| 3 | Rock Edge Light | #2A2A55 | Rock surface facing implied light |
| 4 | Mineral Glint | #3A3A6E | Rare tiny highlights suggesting mineral deposits |

**Art Direction:**
- Stalactites hang from the top edge: 3-5 large stalactites (8-15px wide at base, tapering to 2-3px points, 40-70px long). Irregularly spaced.
- Stalagmites rise from bottom edge: 3-4 matching formations. Some nearly meet their stalactite counterpart (pillar suggestion).
- All shapes are SILHOUETTES -- very dark, nearly matching the background gradient. Only subtle edge lighting (1px of slot 3) differentiates them from pure black.
- Extremely desaturated. These are far away in a dark cave. Color is almost entirely absent -- just value differences.
- Occasional mineral glint (1px dot of slot 4) on stalactite surfaces. Very rare -- 3-4 total across 512px.

---

### Layer 3: Crystal Formations (0.35x)

| Parameter | Value |
|-----------|-------|
| Scroll Speed | 0.35x camera movement |
| Texture Width | 512px |
| Texture Height | 144px |
| Memory | 512 x 144 x 4 = ~288 KB |
| Tiling | Horizontal seamless |
| Content | Mid-distance crystal clusters that provide color accent and ambient light. |

**Color Palette (8 colors):**

| Slot | Name | Hex Code | Usage |
|------|------|----------|-------|
| 0 | Transparent | N/A | Most of texture |
| 1 | Rock Base Dark | #1A1A33 | Rock surfaces holding crystals |
| 2 | Rock Base Light | #262650 | Rock highlights |
| 3 | Crystal Blue Deep | #1565C0 | Crystal body, deep blue |
| 4 | Crystal Blue Light | #42A5F5 | Crystal faces, lighter |
| 5 | Crystal Glow | #90CAF9 | Crystal tips, brightest points |
| 6 | Crystal Purple | #7B1FA2 | Purple crystal variant |
| 7 | Crystal Purple Light | #BA68C8 | Purple crystal highlight |

**Art Direction:**
- 3-4 crystal cluster formations placed across 512px. Each cluster is 20-40px wide, growing from cave walls (top or bottom edge) or from small rock outcroppings.
- Crystal shapes: Angular parallelogram/rhombus shapes. Each crystal shard 3-6px wide, 8-20px long. Clusters contain 3-7 individual shards at varying angles.
- Color distribution: 70% blue crystals, 30% purple crystals. Blue and purple do not mix within a single cluster -- each cluster is one color family.
- GLOW EFFECT: 1-2px halo of the lightest crystal color (slots 5 or 7) surrounding each crystal cluster. This simulates bioluminescent light emission.
- These crystals are the primary COLOR source in the otherwise dark cave. They should pop visually as points of interest without being distracting.
- Much of the texture is transparent -- crystals are sparse features in a large dark space.

---

### Layer 4: Rock Pillars (0.60x)

| Parameter | Value |
|-----------|-------|
| Scroll Speed | 0.60x camera movement |
| Texture Width | 256px |
| Texture Height | 192px |
| Memory | 256 x 192 x 4 = ~192 KB |
| Tiling | Horizontal seamless |
| Content | Close background rock formations that frame the gameplay space. |

**Color Palette (8 colors):**

| Slot | Name | Hex Code | Usage |
|------|------|----------|-------|
| 0 | Transparent | N/A | Open space for gameplay visibility |
| 1 | Rock Dark | #1E1E30 | Deep rock shadow |
| 2 | Rock Mid-Dark | #2E2E48 | Rock body, shadow side |
| 3 | Rock Mid | #3E3E5A | Rock body, main |
| 4 | Rock Light | #52527A | Rock surface, lit side |
| 5 | Rock Highlight | #6A6A90 | Rock edge highlights |
| 6 | Moss Dark | #2E5128 | Moss on damp surfaces |
| 7 | Drip Water | #42A5F5 | Water drips/moisture streaks |

**Art Direction:**
- 2-3 large rock pillars or wall formations. These are CLOSE to the player, so they are large and detailed.
- Pillars: 20-40px wide, spanning nearly full height of texture. Rough, irregular surfaces with visible cracks (1px lines of slot 1) and sediment layers (horizontal banding of different rock tones).
- IMPORTANT: Like the Forest Layer 4, this must be MOSTLY TRANSPARENT. Pillars are positioned at texture edges with large gaps for gameplay visibility. The player must never be obscured.
- Moisture detail: Vertical 1px streaks of slot 7 (water blue) running down pillar surfaces. 2-3 per pillar. Suggests dripping cave water.
- Moss: Small patches of slot 6 on pillar bases and crevices where moisture collects.
- Colors are moderately saturated. Not as vivid as gameplay-layer stone, but noticeably more detailed than distant layers. The blue undertone (cooler grays) differentiates these from gameplay warm grays.

---

### Layer 5: Gameplay (1.00x)

Camera-locked tilemap layer. See tileset_spec.md for Cavern tiles. Same as Forest -- this is not a parallax texture.

---

### Cavern Memory Budget Summary

| Layer | Dimensions | RGBA Size | PNG Compressed (Est.) |
|-------|-----------|-----------|----------------------|
| L1: Dark Gradient | 128 x 144 | 72 KB | ~3 KB |
| L2: Stalactites | 512 x 144 | 288 KB | ~15 KB |
| L3: Crystals | 512 x 144 | 288 KB | ~18 KB |
| L4: Pillars | 256 x 192 | 192 KB | ~12 KB |
| **Total** | -- | **840 KB** | **~48 KB** |

Well under the 1 MB budget at 840 KB VRAM. No foreground overlay layer for cavern (the cave ceiling provides natural framing).

---

## Color Depth Progression Chart

### Forest: Saturation & Brightness by Layer

```
Layer 1 (Sky):     ||||||||||||||||||||||||   High brightness, low saturation (sky blue, neutral)
Layer 2 (Hills):   |||||||||||||||            Medium brightness, reduced saturation (-30%)
Layer 3 (Far):     ||||||||||||               Medium brightness, moderate saturation (-15%)
Layer 4 (Near):    |||||||||||||||||||||      High brightness, near-full saturation (-5%)
Layer 5 (Play):    ||||||||||||||||||||||||   Full brightness, full saturation (reference)
Layer 6 (Overlay): ||||||||||                 Variable (transparent, spot color)
```

### Cavern: Brightness by Layer

```
Layer 1 (Void):    |||                        Very low brightness (near-black)
Layer 2 (Stalac):  |||||                      Low brightness, minimal saturation
Layer 3 (Crystal): ||||||||||||               Medium brightness (crystals provide color pop)
Layer 4 (Pillar):  ||||||||||                 Medium brightness, moderate saturation
Layer 5 (Play):    ||||||||||||||||||||||||   Full brightness, full saturation
```

### Design Rule

**The further a layer is from the camera, the darker, less saturated, and cooler its colors should be.** This mimics atmospheric perspective -- even underground, the principle applies. In caves, "further away" means "darker" since there is no ambient sky light.

---

## Reduced Motion Mode (Accessibility)

### Purpose

Some players experience motion sickness or discomfort from parallax scrolling effects. iOS provides a system-level "Reduce Motion" setting (`UIAccessibility.isReduceMotionEnabled`). The game must respect this setting and provide its own in-game toggle.

### Reduced Motion Configuration

When reduced motion is active, the parallax system collapses from 5 layers to 2:

| Layer | Content | Scroll Speed | Notes |
|-------|---------|-------------|-------|
| Background (merged) | Static composite image | 0.0x (no scroll) | Single pre-rendered image combining layers 1-4 into one static backdrop |
| Gameplay | Normal tilemap | 1.0x | Unchanged |

### Merged Background Creation

For each biome, produce a single STATIC background image that combines the visual essence of all parallax layers into one flat illustration:

**Forest Static Background (512 x 224px):**
- Sky gradient fills upper 40%
- Hill silhouette at horizon line
- Tree line fills lower 60% with 3-4 tree shapes
- Near tree framing on edges
- All at reduced opacity (~70%) to ensure gameplay readability
- No scrolling. Fixed position behind gameplay.

**Cavern Static Background (512 x 224px):**
- Dark gradient fills entire canvas
- Stalactite silhouettes at top
- Crystal glow spots scattered (4-6 points of color)
- Rock pillar silhouettes at edges
- Very dark overall to maintain cave atmosphere
- No scrolling. Fixed position.

### Memory Budget (Reduced Motion)

| Asset | Dimensions | RGBA Size |
|-------|-----------|-----------|
| Forest static BG | 512 x 224 | 448 KB |
| Cavern static BG | 512 x 224 | 448 KB |

Both under 500 KB. The static backgrounds replace all parallax textures, so total VRAM is LESS than normal mode.

### Implementation

```csharp
// Check on scene load
bool reduceMotion = UnityEngine.iOS.Device.systemVersion != null
    && UIAccessibility.isReduceMotionEnabled;

// OR check in-game setting
bool reduceMotion = PlayerPrefs.GetInt("ReduceMotion", 0) == 1;

if (reduceMotion) {
    // Disable layers 1-4 and layer 6
    // Enable single static background sprite
    // Set background scroll speed to 0
    parallaxLayers.ForEach(l => l.gameObject.SetActive(false));
    staticBackground.SetActive(true);
}
```

### Visual Quality Requirement

The reduced motion static background must still look good. It should not feel like a broken or missing feature. The static image should be a carefully composed scene that reads as an intentional art choice. Test that gameplay is fully readable against this background with no visual confusion between foreground and background elements.

---

## Parallax Scrolling Implementation

### Unity Setup

**Option A: SpriteRenderer with Material Offset (Recommended)**

Each parallax layer is a `SpriteRenderer` component on a child GameObject of the camera:

```
Main Camera
  +-- ParallaxLayer_1_Sky     (SpriteRenderer, sorting order -400)
  +-- ParallaxLayer_2_Hills   (SpriteRenderer, sorting order -300)
  +-- ParallaxLayer_3_Far     (SpriteRenderer, sorting order -200)
  +-- ParallaxLayer_4_Near    (SpriteRenderer, sorting order -100)
  +-- [Gameplay Layer is separate Tilemap, sorting order 0]
  +-- ParallaxLayer_6_Overlay (SpriteRenderer, sorting order +100)
```

**Scroll Script (per layer):**

```csharp
public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private float scrollMultiplier = 0.5f;
    [SerializeField] private bool infiniteScroll = true;

    private Transform cameraTransform;
    private float textureUnitSizeX;
    private Vector3 lastCameraPosition;
    private float startPosX;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
        startPosX = transform.position.x;

        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        Texture2D texture = sprite.texture;
        textureUnitSizeX = texture.width / sprite.pixelsPerUnit;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3(
            deltaMovement.x * scrollMultiplier,
            0f, // No vertical parallax (optional: deltaMovement.y * scrollMultiplier * 0.5f)
            0f
        );
        lastCameraPosition = cameraTransform.position;

        if (infiniteScroll)
        {
            float relativeX = cameraTransform.position.x - transform.position.x;
            if (relativeX > textureUnitSizeX)
                transform.position += new Vector3(textureUnitSizeX, 0, 0);
            else if (relativeX < -textureUnitSizeX)
                transform.position -= new Vector3(textureUnitSizeX, 0, 0);
        }
    }
}
```

**For seamless infinite scroll**, each layer sprite must be duplicated 3 times side-by-side (left, center, right) so the camera always sees a filled background regardless of scroll position. The script repositions the sprite when it would otherwise leave the visible area.

**Option B: Shader-Based UV Offset (Performance Optimal)**

Instead of moving GameObjects, a custom shader offsets the UV coordinates of a full-screen quad:

```
// Shader pseudocode
float2 uv = input.uv;
uv.x += _CameraPosX * _ScrollMultiplier / _TextureWidth;
uv.x = frac(uv.x); // Wrap for seamless tiling
float4 color = tex2D(_MainTex, uv);
```

This approach uses zero transform updates and minimal CPU, but requires custom shader knowledge. Recommended for final optimization pass.

---

## Performance Considerations

### Draw Calls

| Configuration | Draw Calls for Parallax |
|--------------|------------------------|
| 5 layers (3 quads each for tiling) | 15 draw calls |
| 5 layers (shader UV offset, 1 quad each) | 5 draw calls |
| Reduced motion (1 static BG) | 1 draw call |

Target: Keep parallax under 10 draw calls. The shader UV offset approach (Option B) achieves this with 5 draw calls.

### GPU Fill Rate

Each parallax layer renders a full-screen quad. On iPhone 11 (reference device) at 750x1334 resolution:
- 5 full-screen quads = ~5 million pixels of fill
- iPhone 11 GPU (A13) handles ~10 billion pixels/sec
- Fill cost: negligible (<0.1% of GPU budget)

### CPU Cost

Option A (transform-based): 5 layers x LateUpdate() per frame = minimal. <0.1ms per frame.
Option B (shader-based): 5 uniform updates per frame = effectively zero CPU cost.

---

## Biome Transition (Future: Sky and Volcanic)

The parallax system is designed to accommodate the remaining two biomes (Sky and Volcanic) without architectural changes. Brief notes for future specification:

### Sky Biome (Preview)

| Layer | Content | Scroll Speed |
|-------|---------|-------------|
| 1 | Open sky with sun/stars | 0.02x |
| 2 | Far cloud layer | 0.15x |
| 3 | Mid clouds with gaps | 0.35x |
| 4 | Near clouds, floating islands in distance | 0.65x |
| 5 | Gameplay (cloud platforms, sky tiles) | 1.0x |
| 6 | Wind streaks, birds | 1.3x |

### Volcanic Biome (Preview)

| Layer | Content | Scroll Speed |
|-------|---------|-------------|
| 1 | Smoke/ash sky (dark red gradient) | 0.05x |
| 2 | Distant lava flows, erupting volcano silhouette | 0.20x |
| 3 | Cooling rock formations with lava veins | 0.40x |
| 4 | Near obsidian pillars, heat shimmer | 0.65x |
| 5 | Gameplay (volcanic tiles) | 1.0x |

### Cross-Biome Transition

When the player moves from one biome to another within a level:
1. Current parallax layers fade out over 2 seconds (alpha 1.0 -> 0.0).
2. New biome layers fade in simultaneously (alpha 0.0 -> 1.0).
3. Scroll positions carry over (no positional reset) to prevent visual pop.
4. Gameplay tilemap transitions independently via tile-by-tile biome boundary.

---

## Art Production Checklist

For each parallax layer, the artist must verify:

- [ ] Left edge pixels match right edge pixels exactly (seamless horizontal tiling)
- [ ] No stray opaque pixels in transparent areas
- [ ] Color values match the palette specification (no unauthorized colors)
- [ ] Texture dimensions match specification (power-of-two width)
- [ ] Gameplay layer content is clearly distinguishable from all background layers
- [ ] Player and enemy sprites maintain visual contrast against all background colors
- [ ] Image saved as PNG with indexed color (8-bit) or RGBA (32-bit) per spec
- [ ] File size is within memory budget allocation

### Testing Procedure

1. Load all 5 layers in Unity test scene
2. Set scroll speeds per specification
3. Scroll camera horizontally for 60+ seconds, observing:
   - No visible texture seams at any scroll position
   - No jitter or sub-pixel flickering
   - Depth effect is convincing
   - Gameplay elements remain clearly visible
4. Toggle reduced motion mode and verify static background quality
5. Measure draw calls and frame time to confirm performance budget

---

**Version**: 1.0
**Last Updated**: 2026-02-04
**Status**: Active
**Assessment**: 6.5 - Parallax Scrolling & Visual Layering
