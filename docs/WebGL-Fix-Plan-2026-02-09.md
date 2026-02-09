# WebGL Fix Plan + Verification Checklist (2026-02-09)

This document defines a **code‑change plan** and a **targeted verification checklist** for the
WebGL `abort("OOM")` crashes, progression blockers, and browser console warnings. No code changes
are applied here.

**Constraints:**
- **No texture size or format reduction.** All current texture dimensions and `RGBA32` formats are
  preserved. Memory savings come from lifecycle management, cache stability, pooling, and
  configuration.
- **Desktop browsers only** (Chrome, Firefox, Edge). No mobile Safari considerations.

**Implementation guide:** `docs/WebGL-Implementation-Plan-2026-02-09.md` — step‑by‑step
file‑level changes with exact line numbers, before/after code, and test criteria.

---

## Goals

- Eliminate WebGL OOM during level transitions and menu return.
- Restore deterministic progression to level 2 and beyond.
- Keep memory usage stable across level loads (avoid churn spikes).
- Preserve current visual style and gameplay behavior (no quality reduction).
- Eliminate all browser console warnings, errors, and log noise in production builds.

---

## Fix Plan (Comprehensive, Ordered)

### Phase 0) Console hygiene (quick wins, no gameplay changes)

**Problem:** The browser console shows warnings and log noise from multiple sources during normal
gameplay. These are not crashes, but they obscure real issues and look unprofessional.

**Sources identified:**

| Source | Type | Files |
|--------|------|-------|
| 7 singleton managers log duplicate detection | `Debug.LogWarning` | `GameManager.cs:377`, `AudioManager.cs:89`, `DifficultyManager.cs:127`, `AccessibilityManager.cs:89`, `TutorialManager.cs:73`, `AchievementManager.cs:130`, `CosmeticManager.cs:60` |
| 8 QC validation checks fire on Playing state | `Debug.LogError` | `GameManager.cs:637–697` |
| Achievement unlock announcements | `Debug.Log` | `AchievementManager.cs:472` |
| Input resource not found warning | `Debug.LogWarning` | `InputManager.cs:55` |
| UI type reflection fallback | `Debug.LogWarning` | `GameManager.cs:1840` |
| Deprecated clipboard API fallback | Browser deprecation | `Assets/Plugins/WebGL/ClipboardPlugin.jslib` |
| Unguarded localStorage writes | Potential `QuotaExceededError` | Multiple files via `PlayerPrefs` |

**Fixes:**

1. **Strip all `Debug.Log*` from production WebGL builds.**
   - Add a custom `ILogHandler` or use a scripting define (e.g., `STRIP_LOGGING`) to suppress
     all log levels in release builds.
   - This eliminates every singleton warning, QC validation error, achievement log, input
     warning, and UI type warning in one shot.
   - **Alternative:** use `StackTraceLogType.None` + conditional compilation guards.

2. **Replace deprecated clipboard fallback.**
   - Update `ClipboardPlugin.jslib` to remove the `document.execCommand("copy")` path.
   - The modern `navigator.clipboard.writeText` API is supported by all current desktop browsers.
   - The `.catch()` handler should remain silent (already is) or show a user‑facing toast.

3. **Guard `PlayerPrefs` against `QuotaExceededError`.**
   - Wrap `PlayerPrefs.Save()` calls in a try/catch at the C# interop boundary.
   - Unlikely to trigger on desktop, but prevents an unrecoverable crash if localStorage is full.

4. **Fix singleton duplicate cascade bug (confirmed firing).**
   - These warnings ARE firing during gameplay. Root cause identified:
   - All 7 managers live on the **same `GameManager` GameObject** (created in `GameManager.Awake()`
     lines 384–391 via `AddComponent`).
   - **3 managers use `Destroy(gameObject)`** (GameManager:378, AchievementManager:131,
     CosmeticManager:61) — this destroys the **entire shared object**, killing all 7 managers.
   - **4 managers use `Destroy(this)`** (AudioManager:90, TutorialManager:74,
     DifficultyManager:128, AccessibilityManager:90) — this correctly destroys only the component.
   - **The cascade:** If any `Destroy(gameObject)` fires, all managers' `OnDestroy` runs,
     clearing all static `Instance` fields. Subsequent access triggers recreation, producing
     another full round of duplicate warnings.
   - **Fix:** All 6 child managers (not GameManager itself) must use `Destroy(this)`, not
     `Destroy(gameObject)`. Only GameManager should destroy the shared GameObject, and only
     when the GameManager itself is the duplicate.
   - **Additional safeguard:** Consider adding a `_isInitialized` flag to `GameManager.Awake()`
     to prevent re-entrancy if the cascade is triggered by an exception during initialization.

**Expected impact:** Zero warnings or errors in the browser console during normal gameplay.

---

### Phase 1) Progression correctness (logic fixes)

These are small, deterministic fixes that address "stuck on level 1" even when memory is stable.
Ship these first — they are independent of the memory work.

- **Advance or clear saved session after completion**
  - Move `SaveSession()` (or an equivalent update) to the level‑completion flow.
  - If player returns to menu after completing a level, ensure the saved session reflects the next level.
  - Alternatively, clear session on `ReturnToTitle()` if you consider a menu return to be a fresh start.

- **Reset LevelID on new Breach start**
  - Ensure `StartGame()` initializes `CurrentLevelID` for Breach mode.
  - This guarantees a new random level instead of reusing the previous one.

**Expected impact:** prevents replaying the same LevelID after a menu return, and allows level 2 progression even without OOM.

---

### Phase 2) Memory stabilization during transitions (highest OOM impact)

**Core principle:** Stop destroying things you're about to recreate. The OOM is caused by heap
fragmentation from destroy‑then‑reallocate cycles, not by absolute memory usage.

- **Replace per‑level `Resources.UnloadUnusedAssets()` with a defined memory‑pressure policy:**
  - Trigger only when heap watermark crosses a threshold (e.g., > 75–80% of max),
    or after a long idle (e.g., 30–60s on title screen), or after N consecutive levels
    without an epoch change (e.g., 5–10).
  - Prefer running it behind a loading screen or menu pause to avoid hitches.
  - **Do not remove it entirely** — without a replacement policy, memory grows unbounded.

- **Keep caches warm on menu return**
  - `ReturnToTitle()` should NOT call `ClearAllCaches()`.
  - Epoch‑stable sprites (tiles, parallax, player, weapons, UI text) survive the return.
  - Only clear if an epoch change is pending or memory pressure is detected.

- **Keep epoch‑stable assets cached across levels**
  - `PlaceholderAssets` and parallax backgrounds should not be rebuilt unless epoch changes.
  - `ParallaxBackground.ClearSpriteCache()` and `LevelRenderer.ClearTileCache()` should be
    gated on epoch change, not called unconditionally on menu return.
  - Minimize texture destroy/create churn during transitions.

- **Reuse UI panels instead of destroy/rebuild**
  - Keep `TitleScreenUI`, `PauseMenuUI`, `LevelCompleteUI`, `GameplayHUD` GameObjects alive.
  - Toggle `SetActive(true/false)` instead of `Destroy` + recreate.
  - This eliminates the title screen's 8.29 MB ornate frame + 2–4 MB title logo being
    destroyed and reallocated on every menu return.
  - Cache commonly reused labels (static strings) rather than regenerating textures.

**Expected impact:** reduces heap fragmentation and peak allocations (primary OOM driver).

---

### Phase 3) Cache lifetime management (prevent unbounded growth)

Since texture sizes are staying as‑is, cache lifetimes are the primary memory lever beyond
stabilization.

- **Cap pixel text sprite cache (LRU eviction)**
  - Dynamic strings (scores, level codes, dates) can create unbounded cache entries.
  - Implement an LRU eviction policy: keep the N most recent dynamic entries (e.g., 50).
  - Static labels ("PLAY GAME", "CONTINUE", etc.) are exempt from eviction.

- **Epoch‑gate parallax cache clearing**
  - `ParallaxBackground` is already epoch‑cached (good), but `ClearSpriteCache()` is called
    unconditionally on menu return. Remove that call from `ReturnToTitle()`.
  - Parallax textures (~30 MB per epoch) should persist until the epoch actually changes.

- **Epoch‑gate tile cache clearing**
  - Same pattern: `LevelRenderer.ClearTileCache()` on menu return is unnecessary if the next
    level is the same epoch. Gate on epoch change.

- **Spread large allocations across frames**
  - If a cache miss forces regeneration (new epoch), spread texture creation across 2–3 frames
    using a coroutine behind a loading screen, to avoid a single‑frame allocation spike.

**Expected impact:** bounds the worst‑case cache size, prevents unnecessary regeneration of
epoch‑stable assets, and smooths allocation peaks.

---

### Phase 4) Pooling and reuse (medium impact)

- **Pool remaining transient FX** — hazard clouds, spikes, arena pillars, and short‑lived ability
  effects that currently use `new GameObject` + `Destroy`.
- **Pool UI GameObjects** — for dynamic elements that can't be kept alive permanently (e.g.,
  score breakdown rows in `LevelCompleteUI`), use a small pool instead of instantiate/destroy.
- Reuse GameObjects that are created/destroyed every level.

**Expected impact:** reduces GC pressure and allocation spikes, helps with frame hitches and OOM risk.

---

### Phase 5) WebGL memory configuration (desktop‑only tuning)

Since the target is desktop browsers only and texture sizes are unchanged:

- **Raise `maximumMemorySize` to 768 MB or 1 GB.**
  - Current 512 MB cap is tight given ~60–100 MB per level peak usage.
  - Desktop Chrome/Firefox/Edge handle 1 GB heaps reliably.

- **Consider raising `initialMemorySize` to 384–512 MB.**
  - Reduces the number of growth events (each growth event is a fragmentation risk).
  - Starting closer to the expected steady‑state usage avoids early growth failures.

- **Keep `memoryGrowthMode = Geometric`** — appropriate for desktop where growth requests
  generally succeed.

**Expected impact:** gives the heap more headroom, reduces growth‑related fragmentation.
Cannot solve fragmentation alone — must be combined with Phases 2–4.

---

## Phased OOM Mitigation Summary

### Phase A: Stop the bleeding (no visual changes)

- Strip all `Debug.Log*` from production builds (Phase 0).
- Fix progression bugs (Phase 1).
- Disable or defer `Resources.UnloadUnusedAssets()` per the memory‑pressure policy (Phase 2).
- Keep caches warm across levels and menu returns (Phase 2).
- Reuse UI panels and common label sprites (Phase 2).
- **Required:** track texture count + estimated total texture bytes to validate stability.

**Success criteria:** Level 2 loads reliably; menu return works; no OOM in 10 consecutive runs;
zero browser console warnings.

### Phase B: Bound and optimize caches

- Cap pixel text sprite cache with LRU eviction (Phase 3).
- Epoch‑gate parallax and tile cache clearing (Phase 3).
- Spread large allocations across frames on epoch change (Phase 3).

**Success criteria:** Peak memory stable across 5+ levels; no OOM with dynamic UI; pixel text
cache stays bounded.

### Phase C: Harden allocations and configuration

- Pool transient FX and UI objects (Phase 4).
- Raise WebGL heap limits for desktop (Phase 5).
- Spread cleanup across frames if needed.

**Success criteria:** Load spikes reduced; stable framerate; no OOM in soak test (20+ levels).

---

## Targeted Verification Checklist

### A) Console Cleanliness (Phase 0)
- Zero `Debug.Log`, `Debug.LogWarning`, `Debug.LogError` in browser console during normal play.
- No `document.execCommand` deprecation warning from clipboard operations.
- No `QuotaExceededError` from localStorage / PlayerPrefs.
- Singleton duplicate warnings either root‑caused and fixed, or confirmed as dead code.

### B) Progression (Phase 1)
- Start new Breach game, complete level 1, confirm level 2 loads.
- Complete level 1, return to menu, hit `Continue`:
  - Confirms LevelID advances or loads level 2.
- Start new Breach run after returning to menu:
  - Confirms LevelID is different from previous run.

### C) OOM / Memory Stability (Phases 2–5)
- Play 5 consecutive levels in WebGL without menu return:
  - No `abort("OOM")`.
  - No multi‑second stalls on level transition.
- Complete level 1, return to menu, re‑enter game:
  - No OOM; UI loads correctly; memory does not spike unexpectedly.
- Repeat menu return cycle 3 times:
  - Confirm no progressive heap growth or crashes.
- **Soak test (Phase C):** Play 20+ consecutive levels:
  - Memory should plateau (no steady upward climb).
  - No OOM or progressive hitching.

### D) UI / Cache Behavior (Phases 2–3)
- Title screen loads without re‑generating all UI textures every time.
- Common labels ("PLAY GAME", "CONTINUE", "SETTINGS") reuse cached sprites.
- Pixel text cache size stays within the configured LRU limit.
- No missing UI or corrupted text after multiple transitions.

### E) Regression Checks
- Campaign mode progression still increments epochs and wraps after epoch 9.
- Daily/weekly challenges still record scores after level completion.
- Tutorial completion / skip flow still behaves correctly.
- Clipboard copy still works (modern API path, no fallback).

---

## Observability (Required for Phase A)

Instrumentation required before Phase A exit:

- Total sprite count and estimated texture bytes in `PlaceholderAssets` cache.
- Texture allocations during `StartLevel()` and `ReturnToTitle()`.
- WebGL heap size and growth events around transitions.
- Pixel text cache entry count (Phase B — needed to validate LRU policy).

---

## Open Questions

1. ~~**Singleton duplicate warnings:**~~ **RESOLVED** — confirmed firing. Root cause:
   `Destroy(gameObject)` in AchievementManager/CosmeticManager kills the shared object,
   cascading destruction across all 7 managers. Fix documented in Phase 0, item 4.

2. **Memory‑pressure threshold tuning:** The policy says "75–80% of max heap" — the exact
   threshold will need empirical tuning once instrumentation is in place. Start conservative
   (trigger earlier) and relax as data comes in.

3. **UI panel reuse scope:** Some UI panels (e.g., `LevelCompleteUI` score breakdown) contain
   dynamic content that changes every level. Decide whether to update in‑place (`.SetText()`)
   or pool child elements. Updating in‑place is simpler and preferred if feasible.

---

## Key Files

| File | Relevance |
|------|-----------|
| `EpochBreaker/Assets/Scripts/Gameplay/GameManager.cs` | State machine, cache clearing, `UnloadUnusedAssets`, GC, UI teardown/rebuild, QC validation, singleton guard, `SaveSession`, `ReturnToTitle`, `StartGame` |
| `EpochBreaker/Assets/Scripts/Gameplay/PlaceholderAssets.cs` | Procedural texture generation, sprite caching, `ClearCache` / `ClearAllCaches`, pixel text system |
| `EpochBreaker/Assets/Scripts/Gameplay/ParallaxBackground.cs` | Background texture generation (1280x512 x15), epoch caching, `ClearSpriteCache` |
| `EpochBreaker/Assets/Scripts/Gameplay/TilemapRenderer.cs` | Tile ScriptableObject cache, epoch handling, `ClearTileCache` |
| `EpochBreaker/Assets/Scripts/Gameplay/LevelLoader.cs` | Level construction, GameObject creation, entity spawning |
| `EpochBreaker/Assets/Scripts/Gameplay/ObjectPool.cs` | Transient object reuse (projectiles, particles, flash, debris, hazard visuals) |
| `EpochBreaker/Assets/Scripts/Gameplay/AudioManager.cs` | Singleton guard, `AudioLowPassFilter` try/catch |
| `EpochBreaker/Assets/Scripts/Gameplay/InputManager.cs` | Input resource loading warning |
| `EpochBreaker/Assets/Scripts/Gameplay/AchievementManager.cs` | Unlock logging, singleton guard |
| `EpochBreaker/Assets/Scripts/UI/*` | UI panel creation, procedural text generation, large UI hierarchies |
| `EpochBreaker/Assets/Scripts/Editor/WebGLBuildScript.cs` | Heap size, growth mode, exception support settings |
| `EpochBreaker/Assets/Plugins/WebGL/ClipboardPlugin.jslib` | Deprecated `document.execCommand("copy")` fallback |
