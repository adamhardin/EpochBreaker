# WebGL Implementation Plan (2026-02-09)

Step‑by‑step implementation guide for the fixes described in
`docs/WebGL-Fix-Plan-2026-02-09.md`. Each step is a discrete, testable change.
No step depends on a later step — implement in order, test after each.

**Constraints (inherited from fix plan):**
- No texture size or format reduction.
- Desktop browsers only.
- No code changes in this document — this is the plan.

---

## Step 1 — Fix singleton cascade bug

**Phase:** 0 (Console hygiene)
**Risk:** Low — two one‑line changes
**Files:**
- `AchievementManager.cs:131`
- `CosmeticManager.cs:61`

**What to change:**

Both files currently call `Destroy(gameObject)` in their duplicate‑detection guard.
Since all 7 managers live on the same `GameManager` GameObject, this kills the
entire shared object.

```
// AchievementManager.cs:131 — BEFORE
Destroy(gameObject);

// AchievementManager.cs:131 — AFTER
Destroy(this);
```

```
// CosmeticManager.cs:61 — BEFORE
Destroy(gameObject);

// CosmeticManager.cs:61 — AFTER
Destroy(this);
```

The other 4 child managers (AudioManager:90, TutorialManager:74,
DifficultyManager:128, AccessibilityManager:90) already use `Destroy(this)` — no
change needed. `GameManager:378` correctly uses `Destroy(gameObject)` because it
owns the shared object.

**Test:** Play a full session (title → level 1 → complete → level 2 → menu return →
new game). Browser console should show **zero** `[Singleton]` duplicate warnings.

---

## Step 2 — Strip Debug.Log from production WebGL builds

**Phase:** 0 (Console hygiene)
**Risk:** Low — logging only, no gameplay impact
**Files:**
- `GameManager.cs` — add a `[RuntimeInitializeOnLoadMethod]` log suppressor
- OR create a new file: `Gameplay/ProductionLogStripper.cs`

**What to change:**

Add a static initializer that disables Unity logging in non‑development builds:

```csharp
// Runs before any other Awake() or log call
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
private static void StripLogsInProduction()
{
    #if !UNITY_EDITOR && !DEVELOPMENT_BUILD
    Debug.unityLogger.logEnabled = false;
    #endif
}
```

**Placement options:**
- Add to `GameManager.cs` (simplest — already has a `BeforeSceneLoad` method).
  Place at lines ~365–371, next to `AutoCreate()`.
- OR create a standalone `ProductionLogStripper.cs` for separation of concerns.

**Note:** The QC validation coroutine (`GameManager.cs:613–616`) is already gated
behind `#if UNITY_EDITOR || DEVELOPMENT_BUILD`, so it will not run or log in
production regardless. This step catches everything else.

**Test:** Make a WebGL development build — logs should appear. Make a release build —
browser console should show zero Unity log output.

---

## Step 3 — Replace deprecated clipboard fallback

**Phase:** 0 (Console hygiene)
**Risk:** Low — clipboard is non‑critical functionality
**File:** `Assets/Plugins/WebGL/ClipboardPlugin.jslib`

**Current code (lines 1–18):**
```javascript
mergeInto(LibraryManager.library, {
    WebGLCopyToClipboard: function(textPtr) {
        var text = UTF8ToString(textPtr);
        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(text).catch(function() {
                // Fallback for older browsers
                var textArea = document.createElement("textarea");
                // ... document.execCommand("copy") ...
            });
        }
    }
});
```

**Replace with:**
```javascript
mergeInto(LibraryManager.library, {
    WebGLCopyToClipboard: function(textPtr) {
        var text = UTF8ToString(textPtr);
        if (navigator.clipboard && navigator.clipboard.writeText) {
            navigator.clipboard.writeText(text).catch(function() {});
        }
    }
});
```

The `navigator.clipboard.writeText` API is supported by all target desktop browsers
(Chrome 66+, Firefox 63+, Edge 79+). The `.catch()` remains to silently handle
permission denials — no fallback needed.

**Test:** In WebGL build, use the COPY button on the level complete screen. Confirm
text copies to clipboard. Confirm no `document.execCommand` deprecation warning in
console.

---

## Step 4 — Guard PlayerPrefs against QuotaExceededError

**Phase:** 0 (Console hygiene)
**Risk:** Low — defensive guard, no behavior change
**Files:** `GameManager.cs` (and optionally other files that call `PlayerPrefs.Save()`)

**What to change:**

Option A — wrap at call sites. `GameManager.SaveSession()` (line 271) calls
`PlayerPrefs.Save()` directly. Wrap it:

```csharp
// GameManager.cs:271 — BEFORE
PlayerPrefs.Save();

// GameManager.cs:271 — AFTER
try { PlayerPrefs.Save(); } catch (System.Exception) { /* localStorage full */ }
```

Option B — create a central helper:

```csharp
public static class SafePrefs
{
    public static void Save()
    {
        try { PlayerPrefs.Save(); }
        catch (System.Exception) { /* localStorage quota exceeded — silent */ }
    }
}
```

Then replace all `PlayerPrefs.Save()` calls with `SafePrefs.Save()`. Search the
codebase for all `PlayerPrefs.Save()` sites:
- `GameManager.cs:271` (SaveSession)
- `GameManager.cs` (ClearSession, ClearCrashFlag, SetCrashFlag — check each)
- `DailyChallengeManager.cs`
- `AchievementManager.cs`
- `AudioManager.cs`
- `CosmeticManager.cs`
- `GhostReplaySystem.cs`

**Test:** Normal gameplay — no behavior change. The guard only activates if
localStorage is full, which is unlikely on desktop but prevents a hard crash.

---

## Step 5 — Fix SaveSession timing (progression bug)

**Phase:** 1 (Progression correctness)
**Risk:** Medium — changes save/continue behavior
**File:** `GameManager.cs`

**Current behavior:**
- `SaveSession()` is called at line 526, inside `TransitionTo(GameState.Playing)`.
  This saves the **current** LevelID before the level starts.
- `ReturnToTitle()` (line 765) does NOT clear or advance the saved session.
- Result: "Continue" reloads the same level instead of the next one.

**Fix (Option A chosen) — advance session on level completion:**

In `TransitionTo(GameState.LevelComplete)` (line 534), after score computation and
before `CreateLevelComplete()` (line 587), save the **next** level's session so
"Continue" resumes from the next level:

```csharp
// After line 586 (AudioManager.PlaySFX), before line 587 (CreateLevelComplete):
// Advance saved session to next level so Continue loads the right one
var nextID = CurrentLevelID.Next();
var advancedSession = new SavedSession
{
    LevelCode = nextID.ToCode(),
    Mode = (int)CurrentGameMode,
    CampaignEpoch = CampaignEpoch,
    StreakCount = StreakCount,
    GlobalLives = GlobalLives,
    TotalScore = TotalScore
};
string json = JsonUtility.ToJson(advancedSession);
PlayerPrefs.SetString(SESSION_PREFS_KEY, json);
PlayerPrefs.Save(); // (use SafePrefs.Save() if Step 4 Option B was chosen)
```

**Test:**
- Complete level 1 → return to menu → Continue → should load level 2 (not level 1).
- Complete level 1 → Continue from LevelComplete screen → should load level 2.
- Start new game (not Continue) → should start fresh regardless.

---

## Step 6 — Reset Breach LevelID on new game

**Phase:** 1 (Progression correctness)
**Risk:** Low — one‑line addition
**File:** `GameManager.cs`

**Current behavior (lines 725–731):**
```csharp
else
{
    // The Breach: random epoch, random seed, endless exploration (default)
    CurrentGameMode = GameMode.TheBreach;
    GlobalLives = DEATHS_PER_LEVEL;
    LevelSource = "TheBreach";
}
```

The Breach branch does NOT set `CurrentLevelID`. When `StartLevel()` runs (line 887),
`if (CurrentLevelID.Seed == 0)` may be false if a previous run's LevelID persists,
causing the same level to replay.

**Fix — zero out CurrentLevelID so StartLevel generates a fresh one:**

```csharp
else
{
    // The Breach: random epoch, random seed, endless exploration (default)
    CurrentGameMode = GameMode.TheBreach;
    CurrentLevelID = default; // ← ADD THIS LINE — forces StartLevel to generate new
    GlobalLives = DEATHS_PER_LEVEL;
    LevelSource = "TheBreach";
}
```

`default` for `LevelID` (a struct) zeroes all fields including `Seed`, which triggers
the `if (CurrentLevelID.Seed == 0)` branch in `StartLevel()` at line 887.

**Test:**
- Start Breach → complete level → return to menu → start new Breach game → should
  get a different level (different LevelID displayed in HUD or level complete screen).

---

## Step 7 — Add memory instrumentation

**Phase:** Observability (required before Phase A exit)
**Risk:** Low — read‑only diagnostics
**Files:**
- `PlaceholderAssets.cs` — add cache stats method
- `GameManager.cs` — log stats at transitions
- New file or JS interop — WebGL heap size

**What to add to `PlaceholderAssets.cs`:**

```csharp
/// <summary>
/// Returns (spriteCount, estimatedBytes) for the current cache.
/// estimatedBytes is approximate: width * height * 4 per texture.
/// </summary>
public static (int count, long bytes) GetCacheStats()
{
    int count = 0;
    long bytes = 0;
    foreach (var sprite in _cache.Values)
    {
        if (sprite != null && sprite.texture != null)
        {
            count++;
            bytes += sprite.texture.width * sprite.texture.height * 4;
        }
    }
    return (count, bytes);
}
```

**What to add to `GameManager.cs`:**

At the end of `StartLevel()` (after line 905, after GC block) and at the start of
`ReturnToTitle()` (line 766), log cache stats:

```csharp
#if DEVELOPMENT_BUILD || UNITY_EDITOR
var (count, bytes) = PlaceholderAssets.GetCacheStats();
Debug.Log($"[MemStats] Sprites={count} EstBytes={bytes / 1024}KB Epoch={CurrentEpoch}");
#endif
```

**WebGL heap size (optional JS interop):**

Add to `ClipboardPlugin.jslib` or a new `.jslib`:

```javascript
GetWebGLHeapSize: function() {
    return HEAPU8.length;
}
```

With C# extern:
```csharp
#if UNITY_WEBGL && !UNITY_EDITOR
[System.Runtime.InteropServices.DllImport("__Internal")]
private static extern int GetWebGLHeapSize();
#endif
```

**Test:** Run a development WebGL build. Confirm `[MemStats]` lines appear at each
level transition and menu return. Confirm sprite count and byte estimate are
reasonable (~50–200 sprites, ~60–100 MB).

---

## Step 8 — Replace per‑level UnloadUnusedAssets with memory‑pressure policy

**Phase:** 2 (Memory stabilization) — **highest OOM impact**
**Risk:** Medium — changes memory management behavior
**File:** `GameManager.cs`

**Current behavior (lines 897–898):**
```csharp
// Always unload destroyed native assets (textures, meshes) to reclaim GPU memory
Resources.UnloadUnusedAssets();
```

This runs on **every** level load, even when the epoch hasn't changed and there are
few unused assets to reclaim. It forces heap fragmentation.

**Replace with a memory‑pressure policy.** Add a private field and check:

```csharp
private int _levelsSinceUnload = 0;
private const int UNLOAD_EVERY_N_LEVELS = 5;
```

In `StartLevel()`, replace the unconditional `Resources.UnloadUnusedAssets()` with:

```csharp
_levelsSinceUnload++;

bool epochChanged = CurrentEpoch != _previousEpoch;
bool memoryPressure = _levelsSinceUnload >= UNLOAD_EVERY_N_LEVELS;

if (epochChanged || memoryPressure)
{
    Resources.UnloadUnusedAssets();
    _levelsSinceUnload = 0;

    if (epochChanged)
    {
        System.GC.Collect();
        _previousEpoch = CurrentEpoch;
    }
}
```

**Keep `HandleMemoryWarning()` (line 1453) as emergency fallback** — it already does
a full cleanup + GC. No change needed there.

**Tuning:** The `UNLOAD_EVERY_N_LEVELS` threshold (5) is conservative. Use the
instrumentation from Step 7 to validate. If memory stays stable, raise it.
Once heap watermark instrumentation is in place (Step 7 JS interop), replace the
level‑count heuristic with a heap percentage check (> 75–80% of max).

**Test:**
- Play 5+ levels in same epoch — no OOM, no multi‑second stalls.
- Cross an epoch boundary — confirm cleanup still runs (GC log in development build).
- Compare `[MemStats]` output before/after: heap should grow more slowly.

---

## Step 9 — Keep caches warm on menu return

**Phase:** 2 (Memory stabilization)
**Risk:** Medium — menu return no longer frees memory
**File:** `GameManager.cs`

**Current behavior (`ReturnToTitle()`, lines 765–775):**
```csharp
public void ReturnToTitle()
{
    PlaceholderAssets.ClearAllCaches();
    PlaceholderAudio.ClearAllCaches();
    ParallaxBackground.ClearSpriteCache();
    LevelRenderer.ClearTileCache();
    _previousEpoch = -1;
    GameManager.PlayerTransform = null;
    TransitionTo(GameState.TitleScreen);
}
```

**Replace with epoch‑gated clearing:**

```csharp
public void ReturnToTitle()
{
    // Only clear epoch-specific sprites (tiles, enemies, bosses).
    // Session sprites (player, UI text, weapons) and epoch caches
    // (parallax, tile ScriptableObjects) survive the return.
    PlaceholderAssets.ClearCache();   // epoch-specific only
    PlaceholderAudio.ClearCache();    // no-op (all audio is session-stable)
    // ParallaxBackground: keep cached — epoch hasn't changed
    // LevelRenderer: keep cached — epoch hasn't changed
    // _previousEpoch: do NOT reset — preserves epoch cache validity
    GameManager.PlayerTransform = null;
    TransitionTo(GameState.TitleScreen);
}
```

**Key change:** `ClearAllCaches()` → `ClearCache()` (level‑specific only). Parallax
and tile caches are left warm. `_previousEpoch` is NOT reset to `-1`.

**Why this is safe:** The title screen's UI sprites (ornate frame, title logo, pixel
text labels) are session‑stable keys and survive `ClearCache()`. Parallax and tile
caches are epoch‑gated and only regenerate when the epoch actually changes.

**Test:**
- Complete level 1 → return to menu: title screen loads instantly (no texture
  regeneration visible). `[MemStats]` should show cache size roughly unchanged.
- Start new game → if same epoch, parallax reuses cached textures.
- Repeat menu return 3 times: no OOM, no progressive heap growth.

---

## Step 10 — Reuse UI panels (toggle visibility)

**Phase:** 2 (Memory stabilization)
**Risk:** High — significant refactor of UI lifecycle
**Files:**
- `GameManager.cs` — `DestroyUI()`, `CreateTitleScreen()`, `CreateHUD()`,
  `CreatePauseMenu()`, `CreateLevelComplete()`, `CreateGameOverUI()`,
  `CreateCelebrationUI()`, `TransitionTo()`
- `TitleScreenUI.cs`, `GameplayHUD.cs`, `PauseMenuUI.cs`, `LevelCompleteUI.cs`,
  `GameOverUI.cs`, `CelebrationUI.cs` — each needs a `Refresh()` or `Show()` method

**Current pattern (lines 1475–1528):**
```csharp
private void DestroyUI()
{
    if (_titleScreenObj) Destroy(_titleScreenObj);
    if (_hudObj) Destroy(_hudObj);
    // ... destroys all 7 UI objects
}

private void CreateTitleScreen()
{
    _titleScreenObj = new GameObject("TitleScreenUI");
    AddUI(_titleScreenObj, "EpochBreaker.UI.TitleScreenUI");
}
// ... same pattern for all 6 other UIs
```

**Replace with hide/show pattern:**

```csharp
private void HideAllUI()
{
    if (_titleScreenObj) _titleScreenObj.SetActive(false);
    if (_hudObj) _hudObj.SetActive(false);
    if (_pauseMenuObj) _pauseMenuObj.SetActive(false);
    if (_levelCompleteObj) _levelCompleteObj.SetActive(false);
    if (_touchControlsObj) _touchControlsObj.SetActive(false);
    if (_gameOverObj) _gameOverObj.SetActive(false);
    if (_celebrationObj) _celebrationObj.SetActive(false);
}

private void ShowOrCreateTitleScreen()
{
    if (_titleScreenObj != null)
    {
        _titleScreenObj.SetActive(true);
        // Optionally call Refresh() to update dynamic content
    }
    else
    {
        _titleScreenObj = new GameObject("TitleScreenUI");
        AddUI(_titleScreenObj, "EpochBreaker.UI.TitleScreenUI");
    }
}
// ... same pattern for all other UIs
```

In `TransitionTo()` (line 501), replace `DestroyUI()` with `HideAllUI()`, and
replace each `Create*()` call with `ShowOrCreate*()`.

**Each UI class needs:**
- A `Refresh()` or `UpdateContent()` method for dynamic data (scores, level info).
- `LevelCompleteUI` is the most complex — score breakdown changes every level.
  Its `Refresh()` must update all score labels. If labels use `GetPixelTextSprite`,
  they'll pull from cache (fast) or generate new (only for new score strings).
- `TitleScreenUI` is mostly static — may need no refresh at all.
- `GameplayHUD` needs refresh for lives, score, weapon indicators.

**This is the largest single step.** Consider implementing it incrementally:
1. Start with `TitleScreenUI` only (biggest savings: 8.29 MB ornate frame).
2. Extend to `PauseMenuUI` (small, mostly static).
3. Then `GameplayHUD`, `LevelCompleteUI`, and the rest.

**Test:** For each converted UI panel:
- Transition to that state → verify UI displays correctly.
- Transition away and back → verify UI reappears with correct content.
- Verify no orphaned/duplicate UI objects after 5+ transitions.
- `[MemStats]` should show no texture churn on repeated transitions.

---

## Step 11 — Cap pixel text cache with LRU eviction

**Phase:** 3 (Cache lifetime management)
**Risk:** Medium — changes cache behavior
**File:** `PlaceholderAssets.cs`

**Current behavior:** Every unique text string creates a cached sprite via
`GetPixelTextSprite()`. Dynamic strings (scores: "12450", level codes: "3-K7XM2P9A",
dates) create unbounded entries.

**What to add:**

Add LRU tracking alongside the existing `_cache` dictionary:

```csharp
private static readonly LinkedList<string> _lruOrder = new LinkedList<string>();
private static readonly Dictionary<string, LinkedListNode<string>> _lruNodes
    = new Dictionary<string, LinkedListNode<string>>();
private const int MAX_DYNAMIC_CACHE_ENTRIES = 50;
```

In the pixel text generation method, when a new dynamic entry is added:

```csharp
// After adding to _cache with a pixeltext_* key:
if (key.StartsWith("pixeltext_"))
{
    // Move to front of LRU (most recently used)
    if (_lruNodes.TryGetValue(key, out var node))
        _lruOrder.Remove(node);
    _lruNodes[key] = _lruOrder.AddFirst(key);

    // Evict oldest if over limit — skip static labels
    while (_lruOrder.Count > MAX_DYNAMIC_CACHE_ENTRIES)
    {
        var oldest = _lruOrder.Last.Value;
        _lruOrder.RemoveLast();
        _lruNodes.Remove(oldest);
        if (_cache.TryGetValue(oldest, out var sprite))
        {
            var tex = sprite.texture;
            Object.Destroy(sprite);
            if (tex != null) Object.Destroy(tex);
            _cache.Remove(oldest);
        }
    }
}
```

**Static label exemption:** Labels that match `IsSessionKey` patterns are already
preserved by `ClearCache()`. The LRU only applies to `pixeltext_*` keys. Common
static labels (PLAY GAME, CONTINUE, etc.) are generated once and never evicted
because they're always accessed and stay at the front of the LRU.

**Test:**
- Play 20+ levels with varying scores.
- `[MemStats]` should show pixel text entries plateau at ~50 (not grow unbounded).
- No missing or corrupted text on any UI screen.

---

## Step 12 — Raise WebGL heap limits

**Phase:** 5 (WebGL configuration)
**Risk:** Low — configuration only
**File:** `WebGLBuildScript.cs`

**Current settings (lines 22–24):**
```csharp
PlayerSettings.WebGL.initialMemorySize = 256;
PlayerSettings.WebGL.maximumMemorySize = 512;
PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;
```

**Replace with:**
```csharp
PlayerSettings.WebGL.initialMemorySize = 512;
PlayerSettings.WebGL.maximumMemorySize = 1024;
PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;
```

**Rationale:**
- Per‑level peak usage is ~60–100 MB. With caches warm across levels, steady state
  may reach 200–300 MB.
- Starting at 512 MB avoids early growth events (each growth is a fragmentation risk).
- 1 GB max is conservative for desktop Chrome/Firefox/Edge.
- Geometric growth mode is appropriate for desktop (growth requests succeed reliably).

**Do this step last** — the cache stabilization from Steps 8–11 should reduce the
need for a high max. If memory stays well under 512 MB with warm caches, you may not
need to raise at all.

**Test:** 20+ level soak test in WebGL. Memory should plateau. No OOM.

---

## Step 13 — Pool remaining transient FX (Phase 4, if needed)

**Phase:** 4 (Pooling and reuse)
**Risk:** Medium — touches multiple gameplay systems
**Files:**
- `ObjectPool.cs` — add new pool categories
- `HazardSystem.cs`, `AbilitySystem.cs`, `ArenaPillar.cs`, `Boss.cs`,
  `ScreenFlash.cs` — replace `new GameObject` + `Destroy` with pool Get/Return

**Scope:** The existing pool covers projectiles, particles, flashes, debris, and
hazard visuals (~130 objects). Remaining un‑pooled transients:
- Hazard clouds (HazardSystem)
- Spike GameObjects (HazardSystem)
- Arena pillar GameObjects (ArenaPillar)
- Short‑lived ability effects (AbilitySystem)
- Boss projectile patterns (Boss)

**This step is lower priority.** The allocation spikes from these are small compared
to texture churn (Steps 8–10). Implement only if OOM persists after Steps 1–12 or
if profiling shows significant GC pressure from these objects.

**Test:** Play levels with heavy FX (boss fights, hazards). Frame rate should be
stable. No new OOM. `[MemStats]` GC pressure should be reduced.

---

## Implementation Order Summary

| Order | Step | Phase | Risk | Impact | Est. Scope |
|-------|------|-------|------|--------|------------|
| 1 | Fix singleton cascade | 0 | Low | Console | 2 lines |
| 2 | Strip Debug.Log | 0 | Low | Console | ~10 lines |
| 3 | Replace clipboard fallback | 0 | Low | Console | 8 lines |
| 4 | Guard PlayerPrefs | 0 | Low | Defensive | ~15 lines |
| 5 | Fix SaveSession timing | 1 | Medium | Progression | ~15 lines |
| 6 | Reset Breach LevelID | 1 | Low | Progression | 1 line |
| 7 | Add instrumentation | Obs. | Low | Diagnostics | ~40 lines |
| 8 | Memory-pressure policy | 2 | Medium | **OOM fix** | ~20 lines |
| 9 | Keep caches warm | 2 | Medium | **OOM fix** | ~10 lines |
| 10 | Reuse UI panels | 2 | High | **OOM fix** | ~200+ lines |
| 11 | Cap pixel text cache | 3 | Medium | Memory bound | ~30 lines |
| 12 | Raise heap limits | 5 | Low | Headroom | 2 lines |
| 13 | Pool transient FX | 4 | Medium | GC pressure | ~100 lines |

**Recommended checkpoints:**
- After Step 4: build and verify zero console warnings.
- After Step 6: verify progression works (level 2 loads, Continue advances).
- After Step 9: verify no OOM on 10 consecutive runs + 3 menu returns.
- After Step 12: run 20+ level soak test, confirm memory plateaus.
