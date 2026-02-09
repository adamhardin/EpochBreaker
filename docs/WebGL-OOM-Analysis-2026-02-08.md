# WebGL OOM Analysis (2026-02-08)

Issue: Browser console shows `abort("OOM")` with `abortOnCannotGrowMemory` when advancing to level 2 or returning to menu. This indicates the WebAssembly heap could not grow (or could not find a contiguous block), which in Unity WebGL typically means the build hit its configured memory cap or fragmentation prevented further allocation.

This is a static code review only. No code changes were made.

---

## Browser Console Warnings (Non‑OOM)

In addition to the OOM crash, the WebGL build produces console warnings and log output from
multiple sources during normal gameplay:

| Source | Type | Files |
|--------|------|-------|
| 7 singleton managers log duplicate detection | `Debug.LogWarning` | `GameManager.cs:377`, `AudioManager.cs:89`, `DifficultyManager.cs:127`, `AccessibilityManager.cs:89`, `TutorialManager.cs:73`, `AchievementManager.cs:130`, `CosmeticManager.cs:60` |
| 8 QC validation checks fire on Playing state | `Debug.LogError` | `GameManager.cs:637–697` |
| Achievement unlock announcements | `Debug.Log` | `AchievementManager.cs:472` |
| Input resource not found warning | `Debug.LogWarning` | `InputManager.cs:55` |
| UI type reflection fallback | `Debug.LogWarning` | `GameManager.cs:1840` |
| Deprecated clipboard API fallback | Browser deprecation | `Assets/Plugins/WebGL/ClipboardPlugin.jslib` (uses `document.execCommand("copy")`) |
| Unguarded localStorage writes | Potential `QuotaExceededError` | Multiple files via `PlayerPrefs` |

**Singleton duplicate root cause (confirmed firing):**
- All 7 managers live on the **same `GameManager` GameObject** (`GameManager.Awake()` lines
  384–391 adds all 6 child managers via `AddComponent`).
- 3 managers (`GameManager:378`, `AchievementManager:131`, `CosmeticManager:61`) call
  `Destroy(gameObject)` on duplicate detection — this destroys the **entire shared object**,
  killing all 7 managers at once.
- 4 managers (`AudioManager:90`, `TutorialManager:74`, `DifficultyManager:128`,
  `AccessibilityManager:90`) correctly call `Destroy(this)` (component only).
- When `Destroy(gameObject)` fires, all managers' `OnDestroy` clears their static `Instance`
  fields, triggering mass recreation and a cascade of duplicate warnings.
- **Fix:** child managers must use `Destroy(this)`, not `Destroy(gameObject)`.

**Notes:**
- The `AudioLowPassFilter` initialization in `AudioManager.cs:126–138` is already wrapped in a
  try/catch — this does not produce browser warnings.
- Editor‑only scripts (`BuildScript.cs`, `ProjectSetup.cs`, `LevelValidator1000.cs`,
  `QAValidationRunner.cs`) produce `Debug.Log` output during builds but do not run in the
  WebGL runtime.
- The `ExplicitlyThrownExceptionsOnly` exception support setting means most caught exceptions
  do not appear in the console — only explicitly thrown ones.
- The Bootstrap scene is clean — only an `EditorCamera`. No managers in the scene
  (`ProjectSetup.cs:53`). No `SceneManager.LoadScene` calls in runtime code.

For the fix plan addressing these warnings, see `docs/WebGL-Fix-Plan-2026-02-09.md` (Phase 0).

---

## What the Error Means (WebGL Context)

- `abort("OOM")` + `abortOnCannotGrowMemory` is thrown by the Emscripten runtime when the WASM heap cannot be resized to satisfy a new allocation.
- In WebGL, memory growth can fail even if “total memory” looks reasonable because the browser cannot provide a larger contiguous block or because the configured max heap has been reached.
- This often appears during **level transitions** where large amounts of content are destroyed and re-created in rapid succession.

---

## Primary Root Causes in This Codebase

### 1) Procedural textures are created aggressively and frequently

**High-impact drivers:**
- `EpochBreaker/Assets/Scripts/Gameplay/PlaceholderAssets.cs`
  - Generates many `Texture2D` and `Sprite.Create` assets at runtime.
  - Tile size: `TILE_PX = 256` with `TextureFormat.RGBA32`.
  - Large UI/title textures are explicitly sized (multiple `768x768` textures in `PlaceholderAssets.cs`, plus other large UI sprites).
- `EpochBreaker/Assets/Scripts/Gameplay/ParallaxBackground.cs`
  - Generates 15 textures per epoch (5 layers x 3 tiles) at 1280x512 RGBA32. Even with epoch caching, these are large allocations.

**Why this is risky:**
- WebGL heap growth is sensitive to large allocations and fragmentation.
- Repeated create/destroy of large textures increases fragmentation and can trigger heap resize failures.

### 2) Level transitions intentionally clear caches and trigger heavy allocations

**Level start:**
- `EpochBreaker/Assets/Scripts/Gameplay/GameManager.cs`
  - `PlaceholderAssets.ClearCache()` (clears epoch-specific sprites)
  - `Resources.UnloadUnusedAssets()` every level
  - `System.GC.Collect()` on epoch change

**Return to title:**
- `EpochBreaker/Assets/Scripts/Gameplay/GameManager.cs`
  - `PlaceholderAssets.ClearAllCaches()`
  - `PlaceholderAudio.ClearAllCaches()`
  - `ParallaxBackground.ClearSpriteCache()`
  - `LevelRenderer.ClearTileCache()`

**Why this is risky:**
- Clearing caches + immediately regenerating large textures and UI sprites creates allocation spikes.
- `Resources.UnloadUnusedAssets()` forces native asset cleanup but can fragment memory or stall the main thread.
- WebGL heap growth failures often occur right after these cleanup + rebuild phases.

### 3) UI is rebuilt with new GameObjects and procedural text sprites

- Title and menu screens (`TitleScreenUI.cs`, `PauseMenuUI.cs`, `LevelCompleteUI.cs`, etc.) create large hierarchies of GameObjects and call `PlaceholderAssets.GetPixelTextSprite` for labels.
- Each unique label becomes a new texture cached in `PlaceholderAssets`.

**Risk:**
- Returning to menu after clearing caches forces all UI textures to be regenerated and reallocated.
- If labels are dynamic (scores, level names, dates), cache size can grow quickly and unpredictably.

### 4) Large numbers of runtime instantiations and destroys during level build

- `EpochBreaker/Assets/Scripts/Gameplay/LevelLoader.cs` creates numerous GameObjects each level (tilemap grid, player, enemies, pickups, checkpoints, boss arena, etc.).
- Transient effects (hazards, debris, screen flash, ability FX) often use `new GameObject` + `Destroy` rather than pooling.

**Risk:**
- Increases GC and allocation pressure during transitions, which is when the OOM occurs.

### 5) WebGL heap settings may be insufficient or too aggressive

- `EpochBreaker/Assets/Scripts/Editor/WebGLBuildScript.cs`
  - `initialMemorySize = 256` MB, `maximumMemorySize = 512` MB, `memoryGrowthMode = Geometric`

**Risk:**
- Some browsers (especially Safari) fail to grow to large heaps. Even on Chrome, growth can fail due to fragmentation. A 512MB cap may still be too low if procedural textures spike repeatedly.

---

## Secondary Contributors and “Memory Churn” Risks

### Procedural sprite creation hotspots
- `PlaceholderAssets.cs` creates many `Texture2D` instances across different methods (large UI frames, title logos, boss sprites, etc.).
- This is memory-heavy and unavoidable in the current art pipeline, but **cache lifetimes** matter.

### Checkpoint platform sprite
- `EpochBreaker/Assets/Scripts/Gameplay/LevelLoader.cs` creates a 384x128 platform sprite (cached statically). This is minor but contributes to total texture footprint.

### UI regeneration
- `GameManager.DestroyUI()` + `CreateTitleScreen()` / `CreateHUD()` / `CreatePauseMenu()` destroys and rebuilds UI objects as you transition between states.
- If caches were cleared, all UI textures are regenerated from scratch.

### Destroy-heavy effects
- `HazardSystem`, `AbilitySystem`, `ScreenFlash`, `ArenaPillar`, `Boss` spawn temporary objects and destroy them later.
- Not likely the sole cause, but contributes to memory churn during intense sections.

---

## Why It Fails Specifically After Level 1 / On Menu Return

- **Level 1 loads successfully** because the heap starts clean and there is free headroom.
- **Level 2 triggers OOM** because new allocations happen after cache clearing + `UnloadUnusedAssets`/GC, leading to heap growth attempts at a time when memory is fragmented or capped.
- **Menu return is especially risky** because `ClearAllCaches` + UI rebuild forces a large burst of texture allocations (title logo, ornate frame, pixel labels, etc.).

---

## Progression Blockers Observed in Code (Non‑OOM)

These are logic issues that can make progression look stuck even when memory is sufficient.

### 1) Returning to menu preserves a session that points at the just‑completed level

- `GameManager.SaveSession()` is called when entering `Playing` (level start), not after level completion.
- `ReturnToTitle()` does **not** clear or advance the saved session.
- Result: after finishing level 1 and returning to the title screen, `Continue` reloads the same LevelID instead of advancing.

**Files:**
- `EpochBreaker/Assets/Scripts/Gameplay/GameManager.cs` (SaveSession, ReturnToTitle, TransitionTo→Playing)

### 2) Starting a new Breach run doesn’t reset the current LevelID

- In `StartGame()`, the Campaign branch sets `CurrentLevelID`, but the Breach branch does not.
- `StartLevel()` only generates a new level when `CurrentLevelID.Seed == 0`.
- Result: after returning to menu, starting a new Breach run can re‑use the last LevelID, making it look like progression never changed.

**Files:**
- `EpochBreaker/Assets/Scripts/Gameplay/GameManager.cs` (StartGame, StartLevel)

---

## Recommendations (Summary Only — See Fix Plan)

For detailed recommendations, phases, and verification, see:
- `docs/WebGL-Fix-Plan-2026-02-09.md`

---

## Proactive Assessment: Areas to Inspect or Instrument

1. **Track procedural texture count and total bytes**
   - Log total sprites in `PlaceholderAssets` cache and estimated memory.
2. **Measure level load time with memory snapshots**
   - Check memory before and after `StartLevel()` and after UI creation.
3. **Detect cache explosion in UI labels**
   - If a label is generated from user input or dynamic score text, you may be generating many unique textures.

---

## Key Files Related to OOM Risk and Console Warnings

- `EpochBreaker/Assets/Scripts/Gameplay/GameManager.cs`
  - Cache clearing, `Resources.UnloadUnusedAssets()`, GC, UI teardown/rebuild.
  - Singleton duplicate guard (line 377), QC validation logging (lines 637–697).
- `EpochBreaker/Assets/Scripts/Gameplay/PlaceholderAssets.cs`
  - Large runtime texture creation across many features.
- `EpochBreaker/Assets/Scripts/Gameplay/ParallaxBackground.cs`
  - Large background texture generation.
- `EpochBreaker/Assets/Scripts/Gameplay/TilemapRenderer.cs`
  - Epoch tile cache; clearing on menu return.
- `EpochBreaker/Assets/Scripts/Gameplay/AudioManager.cs`
  - Singleton guard (line 89), `AudioLowPassFilter` try/catch (lines 126–138).
- `EpochBreaker/Assets/Scripts/Gameplay/InputManager.cs`
  - Input resource loading warning (line 55).
- `EpochBreaker/Assets/Scripts/Gameplay/AchievementManager.cs`
  - Unlock logging (line 472), singleton guard (line 130).
- `EpochBreaker/Assets/Scripts/UI/*`
  - Procedural text generation and large UI hierarchies.
- `EpochBreaker/Assets/Scripts/Editor/WebGLBuildScript.cs`
  - Heap size and growth settings.
- `EpochBreaker/Assets/Plugins/WebGL/ClipboardPlugin.jslib`
  - Deprecated `document.execCommand("copy")` fallback.

---

## Summary

The OOM is almost certainly driven by repeated procedural texture creation combined with aggressive cache clearing and full UI rebuilds during level/menu transitions. WebGL's memory model is unforgiving of this allocation pattern. The most effective prevention strategy is to **stabilize memory usage**: keep caches warm across levels, avoid frequent full cleanups, and reuse UI/FX objects instead of recreating them. In addition, the browser console shows persistent warnings from singleton guards, QC validation checks, achievement logging, and a deprecated clipboard API — all of which should be addressed to keep the console clean for real diagnostics.

For the comprehensive fix plan, verification checklist, and step‑by‑step implementation guide, see:
- `docs/WebGL-Fix-Plan-2026-02-09.md` — phases, success criteria, verification checklist
- `docs/WebGL-Implementation-Plan-2026-02-09.md` — file‑level changes with line numbers and test steps
