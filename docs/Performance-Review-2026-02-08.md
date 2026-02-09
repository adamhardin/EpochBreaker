# EpochBreaker Performance Review (2026-02-08)

Scope: Browser/WebGL build today, with future iOS targets. Review focuses on memory usage, loading behavior, per-frame CPU hotspots, and WebGL/mobile constraints. This document is based on static code review only; no code changes were made.

## Executive Summary

The game relies heavily on runtime procedural generation of sprites, audio, and background textures. This keeps asset size down, but shifts cost into CPU time and heap/GC pressure during level loads. The most significant risk to performance stability (especially on WebGL/mobile) is the repeated creation/destruction of large textures and the explicit `Resources.UnloadUnusedAssets()` + `GC.Collect()` calls on every level transition, which can cause multi‑second hitches and jank. There are also per‑frame loops that scale with enemy count and hazards, and multiple places that instantiate/destroy GameObjects instead of pooling.

If you only do three things for a major performance win:
1. Replace level‑start full cache clear + GC with staged or conditional cleanup and move procedural asset generation into a warm‑up step.
2. Cache or reuse parallax textures and tile `Tile` assets per epoch instead of re‑creating them every level.
3. Replace per‑frame “scan all enemies/hazards” with cheaper data structures (squared distance checks, spatial bins, or timed sampling).

## High‑Impact Findings (Priority Order)

1. **Level transition forces full GC + unloading every time.**
   - Code: `EpochBreaker/Assets/Scripts/Gameplay/GameManager.cs` (StartLevel). It calls `PlaceholderAssets.ClearCache()`, `PlaceholderAudio.ClearCache()`, `Resources.UnloadUnusedAssets()`, and `System.GC.Collect()` every level load.
   - Risk: Large, inconsistent frame hitches in WebGL/mobile. WebGL browsers are sensitive to long main‑thread stalls; GC + UnloadUnusedAssets often triggers multi‑frame freezes.
   - Recommendation: Move to a staged load pipeline.
     - Pre‑warm/calc procedural assets once per epoch and keep them cached for the session.
     - Only run `UnloadUnusedAssets()` on explicit “memory pressure” events or after a menu screen, not per level.
     - If you must unload, do it on a loading screen and use async with frame yielding.

2. **Parallax background generates large textures at runtime, per level.**
   - Code: `EpochBreaker/Assets/Scripts/Gameplay/ParallaxBackground.cs` (GenerateLayerSprite)
   - Behavior: Creates 15 textures per level (5 layers x 3 tiles), each 1280x512 RGBA32, via `Texture2D` and `SetPixels32`.
   - Risk: High memory churn and CPU cost during level load. Texture allocations are large; this is a likely cause of WebGL spikes and memory growth.
   - Recommendation: Cache parallax textures per epoch, or create a smaller sprite atlas and reuse it across levels. If visual variation is needed, randomize transforms/colors rather than regenerating textures.

3. **Tile assets are created per level, not cached.**
   - Code: `EpochBreaker/Assets/Scripts/Gameplay/TilemapRenderer.cs` (BuildTileAssets)
   - Behavior: Creates a new `Tile` ScriptableObject for each tile type on every level.
   - Risk: Extra allocations and GC churn on level load.
   - Recommendation: Cache tiles per epoch or make static `Tile` assets. If runtime generation is required, store in a per‑epoch cache and reuse.

4. **Procedural sprite and audio caches are fully cleared every level.**
   - Code: `EpochBreaker/Assets/Scripts/Gameplay/PlaceholderAssets.cs` and `EpochBreaker/Assets/Scripts/Gameplay/PlaceholderAudio.cs` + `GameManager.StartLevel`.
   - Risk: Regeneration cost each level. This is amplified because UI screens call `GetPixelTextSprite` in many places.
   - Recommendation: Consider a “session cache” for frequently used UI sprites and common SFX (player sprite, UI labels, core pickups, etc.), and only clear large, level‑specific assets.

5. **Enemy targeting scans all enemies every frame.**
   - Code: `EpochBreaker/Assets/Scripts/Gameplay/Weapons/WeaponSystem.cs` (FindTarget)
   - Behavior: Iterates through `EnemyBase.ActiveEnemies` each frame and uses `Vector2.Distance` (square root) per enemy.
   - Risk: O(N) per frame scaling with enemy count; frequent sqrt calls.
   - Recommendation: Use squared distance checks, or scan less frequently (e.g., every 0.1s). For larger enemy counts, consider spatial bins or a simple grid index.

## Memory Usage and GC Observations

- **Procedural asset generation is the dominant memory driver.**
  - `PlaceholderAssets` creates 256x256 textures for each sprite. Many unique keys exist for tiles, enemies, UI text, etc.
  - `ParallaxBackground` creates multiple large textures per level (1280x512).
  - `PlaceholderAudio` creates new AudioClip buffers per clip.

- **Per‑level cache clearing shifts cost from steady memory to GC spikes.**
  - The explicit `Resources.UnloadUnusedAssets()` + `GC.Collect()` in `GameManager.StartLevel` is a red flag for runtime smoothness, even if it avoids OOM on WebGL.

Recommendations:
- Track total procedural texture count and size. Add a debug overlay that reports counts and estimated memory.
- Consider a two‑tier cache: “session cache” (never cleared), “level cache” (cleared on level change). This reduces rebuild cost.
- If WebGL memory pressure is a concern, consider lowering `PlaceholderAssets` texture size (e.g., 128px tiles) and increasing pixel‑art scale in shader/UI.

## Loading Behavior and Level Construction

- **GameObjects are instantiated/destructed heavily during level load.**
  - Code: `EpochBreaker/Assets/Scripts/Gameplay/LevelLoader.cs` and `EpochBreaker/Assets/Scripts/Gameplay/HazardSystem.cs`.
  - Many objects are created each level: tilemap grids, player, camera, enemies, pickups, hazards, etc.
  - Some transient objects (debris, hazard clouds) are always `new GameObject` + `Destroy`, rather than pooled.

Recommendations:
- Expand `ObjectPool` to cover transient effects (debris, hazard clouds, spikes, short‑lived particles). See `ObjectPool` in `EpochBreaker/Assets/Scripts/Gameplay/ObjectPool.cs`.
- Consider pooling for enemies and pickups if level resets are frequent.
- Avoid `new PhysicsMaterial2D` per player spawn in `LevelLoader.SpawnPlayer`. Cache and reuse a shared material.

## Per‑Frame/FixedUpdate Hotspots

- `WeaponSystem.FindTarget` scans every enemy each frame. Use squared distance or sampling intervals.
- `HazardSystem.Update` iterates every active hazard each frame and does distance checks against the player. If hazards can grow large, consider time‑slicing or spatial checks.
- `EnemyBase.FixedUpdate` uses `GameObject.FindWithTag("Player")` when player reference is lost. If the player is frequently destroyed/recreated, this can cause multiple expensive searches across enemies. Consider a centralized player reference on `GameManager`.
- `ParallaxBackground.LateUpdate` updates 5 layers x 3 tiles each frame; fine as-is, but ensure sprites are static and not causing per‑frame material changes.

## WebGL‑Specific Considerations

- Build script sets:
  - `PlayerSettings.WebGL.initialMemorySize = 256` and `maximumMemorySize = 512` (MB), and uses `WebGLMemoryGrowthMode.Geometric`.
  - `PlayerSettings.WebGL.compressionFormat = Brotli` with `decompressionFallback = true`.
  - Code: `EpochBreaker/Assets/Scripts/Editor/WebGLBuildScript.cs`.

Recommendations:
- For iOS Safari WebGL, 512MB max may be too high; Safari can terminate or refuse allocations sooner. Consider dynamic memory scaling options and testing lower max sizes.
- Keep `exceptionSupport` minimal (already set to `ExplicitlyThrownExceptionsOnly`).
- Consider disabling or limiting `Resources.UnloadUnusedAssets()` on WebGL builds unless behind a loading screen.

## Rendering and URP Settings

- WebGL uses `QualitySettings` “Mobile” profile (index 0). Shadows are enabled with distance 40 and cascades 2.
  - Code: `EpochBreaker/ProjectSettings/QualitySettings.asset`.

Recommendations:
- For WebGL/mobile performance, consider disabling real‑time shadows entirely or reducing shadow distance further (especially for 2D). If you don’t need 3D shadowing, turn them off.
- Ensure URP settings are minimal for 2D: no additional render features, no SSAO, no post‑processing.

## UI/UX Performance Notes

- UI uses procedural `GetPixelTextSprite` heavily for labels. Each unique label is a new texture and sprite stored in cache.
- That makes UI resolution sharp, but can balloon memory if labels are dynamic or numerous.

Recommendations:
- Consolidate labels into a bitmap font (TextMeshPro with a custom font atlas) or cache only the most common labels.
- If sticking with `GetPixelTextSprite`, consider adding a maximum cache size and evict least‑recently‑used entries.

## Suggested Instrumentation

To make optimization decisions with real data:
- Add a debug overlay for runtime memory (texture count, total texture bytes, sprite count, audio clip count).
- Track frame time breakdown on WebGL via the Unity Profiler (development build) and browser performance tools.
- Log level load time and time spent in `Resources.UnloadUnusedAssets()` and GC.

## Additional Observations

- `ObjectPool` is a solid start; extending it to hazards and debris will reduce GC and `Destroy()` spikes.
- Using `Vector2.Distance` repeatedly can be replaced with squared distance checks (`(a-b).sqrMagnitude`) to avoid `sqrt` costs in hotspots.

---

If you want, I can also produce a prioritized optimization plan with estimated effort/cost, or a test checklist for validating performance improvements on WebGL and iOS Safari.
