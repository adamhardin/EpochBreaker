# Assessment 5.5 - Interruption Handling & State Management Specification

**Module**: 5 - Mobile UX & Accessibility
**Version**: 1.0
**Last Updated**: 2026-02-04
**Status**: Active
**Engine**: Unity 2022 LTS
**Orientation**: Landscape only (locked)
**Minimum Device**: iPhone 11
**Target**: 60fps

---

## Table of Contents

1. [Interruption Event Matrix](#1-interruption-event-matrix)
2. [Phone Call Handling](#2-phone-call-handling)
3. [App Backgrounding & Foregrounding](#3-app-backgrounding--foregrounding)
4. [App Termination & Restoration](#4-app-termination--restoration)
5. [Notification Handling](#5-notification-handling)
6. [Auto-Save System](#6-auto-save-system)
7. [iCloud Sync & Conflict Resolution](#7-icloud-sync--conflict-resolution)
8. [Session Lifecycle & Performance Targets](#8-session-lifecycle--performance-targets)
9. [Save Corruption Handling](#9-save-corruption-handling)
10. [Edge Cases & Error Recovery](#10-edge-cases--error-recovery)
11. [Unity Implementation](#11-unity-implementation)

---

## 1. Interruption Event Matrix

### 1.1 Complete Interruption Catalog

| # | Event | iOS Notification | Game Response | Audio | Save | Resume Behavior |
|---|-------|-----------------|---------------|-------|------|-----------------|
| 1 | Incoming phone call (full screen) | `UIApplicationWillResignActive` | Auto-pause, show pause overlay | Mute all | Save state | Resume on return, show pause menu |
| 2 | Incoming phone call (banner) | `UIApplicationWillResignActive` (brief) | Auto-pause | Mute all | Save state | Auto-resume when call banner dismissed (if < 3s) |
| 3 | App sent to background (home button / swipe) | `UIApplicationDidEnterBackground` | Auto-pause, save full state | Mute all | Save full state | Resume from exact state on foreground |
| 4 | App terminated by system (memory pressure) | `UIApplicationWillTerminate` (if time) | Save at last checkpoint | Mute all | Save checkpoint | Restore from last checkpoint on relaunch |
| 5 | App terminated by user (swipe from app switcher) | No callback guaranteed | Rely on background save | N/A | Last background save | Restore from last checkpoint on relaunch |
| 6 | Notification banner (any app) | None (game remains foreground) | Game CONTINUES playing | No change | No save | No interruption |
| 7 | Notification Center pull-down | `UIApplicationWillResignActive` | Auto-pause | Mute all | Save state | Resume on return |
| 8 | Control Center swipe-up/down | `UIApplicationWillResignActive` | Auto-pause | Mute all | Save state | Resume on return |
| 9 | Low battery warning | System alert overlay | Game CONTINUES (alert is small) | No change | No save | No interruption |
| 10 | Siri invocation | `UIApplicationWillResignActive` | Auto-pause | Mute all | Save state | Resume on return |
| 11 | FaceTime / SharePlay invitation | `UIApplicationWillResignActive` | Auto-pause | Mute all | Save state | Resume on return |
| 12 | Screenshot taken | None | Game continues | Brief shutter SFX (system) | No save | No interruption |
| 13 | Screen recording started | None | Game continues | No change | No save | No interruption |
| 14 | Alarm / Timer fires | `UIApplicationWillResignActive` (if full screen) | Auto-pause if full-screen, continue if banner | Depends on alert type | Save state if full-screen | Resume on dismissal |
| 15 | AirDrop incoming | Small overlay | Game continues | No change | No save | No interruption |

### 1.2 Priority Rules

When multiple interruptions overlap:
1. Always save state on ANY `WillResignActive` event
2. Never auto-resume if another pause-causing event is pending
3. On return from background, always show pause menu (never auto-resume into gameplay directly)
4. Audio state is restored only when the game is actually resumed by the player (not on foreground, but on "Resume" tap)

---

## 2. Phone Call Handling

### 2.1 Full-Screen Phone Call

| Phase | Trigger | Action | Timing |
|-------|---------|--------|--------|
| Call incoming | `applicationWillResignActive` | 1. Pause game loop (`Time.timeScale = 0`). 2. Mute all audio. 3. Save current state. 4. Show pause overlay (dimmed). | Immediate (same frame) |
| User answers or declines | App may background or return | If backgrounded: follow background flow. If call dismissed: proceed to return phase. | Variable |
| Return to app | `applicationDidBecomeActive` | 1. Show pause menu (do NOT auto-resume). 2. Player taps "Resume" to continue. 3. Restore audio. 4. Unpause game loop. | < 0.5s to show pause menu |

### 2.2 Banner Phone Call (CallKit Banner)

| Phase | Trigger | Action | Timing |
|-------|---------|--------|--------|
| Banner appears | `applicationWillResignActive` | 1. Pause game. 2. Mute audio. 3. Save state. | Immediate |
| Banner dismissed (declined) | `applicationDidBecomeActive` within ~3s | 1. If pause duration < 3 seconds, auto-resume. 2. If > 3 seconds, show pause menu. 3. Restore audio (with 0.3s fade-in). | < 0.3s |
| Call answered from banner | App backgrounds | Follow background flow (Section 3). | Immediate |

### 2.3 Audio Handling During Calls

- All game audio sessions use `AVAudioSession.Category.ambient` (mixed with system audio, allows phone calls to take priority)
- On pause: Fade audio out over 0.1s (not abrupt cut)
- On resume: Fade audio in over 0.3s
- If the player was wearing headphones and removed them during the call, respect the iOS audio route change and do NOT play audio through speakers unexpectedly

---

## 3. App Backgrounding & Foregrounding

### 3.1 Entering Background

**Trigger**: `applicationDidEnterBackground` (user pressed Home, swiped to another app, etc.)

**Actions (in order, within ~5 seconds allowed by iOS):**

| Step | Action | Max Time | Notes |
|------|--------|----------|-------|
| 1 | Set `Time.timeScale = 0` | < 1ms | Freezes game physics and animations |
| 2 | Mute all audio channels | < 1ms | `AudioListener.pause = true` |
| 3 | Serialize current game state to memory | < 50ms | Position, velocity, health, score, enemies, particles |
| 4 | Write game state to local storage | < 200ms | `Application.persistentDataPath/quicksave.json` |
| 5 | Write checkpoint save to local storage | < 100ms | `Application.persistentDataPath/checkpoint.json` |
| 6 | Queue iCloud sync (if enabled) | < 10ms | Non-blocking; actual sync happens asynchronously |
| 7 | Capture screenshot for app switcher | Automatic | iOS handles this; ensure no sensitive data visible |
| 8 | Release non-essential resources | < 100ms | Unload cached textures, trim audio buffers |

**Total time budget**: < 500ms (well within iOS 5-second limit)

### 3.2 Returning to Foreground

**Trigger**: `applicationWillEnterForeground` followed by `applicationDidBecomeActive`

**Actions (in order):**

| Step | Action | Max Time | Notes |
|------|--------|----------|-------|
| 1 | Load quick-save from local storage | < 100ms | `quicksave.json` |
| 2 | Validate save data integrity (checksum) | < 10ms | See Section 9 |
| 3 | Restore game state from save | < 200ms | Positions, health, score, etc. |
| 4 | Re-acquire audio session | < 50ms | `AVAudioSession.setActive(true)` |
| 5 | Show pause menu overlay | < 50ms | Player must tap Resume to continue |
| 6 | Wait for player input ("Resume") | Indefinite | Game remains paused |
| 7 | On Resume: `Time.timeScale = 1`, fade in audio | < 300ms | 0.3s audio fade-in |

**Total automatic time**: < 500ms to display pause menu. Target: **< 1 second** for the player to see the pause menu after tapping the app icon.

### 3.3 State Serialization Format

The quick-save captures the complete game state for exact restoration:

```json
{
  "version": 2,
  "timestamp": "2026-02-04T14:30:00Z",
  "checksum": "a1b2c3d4e5f6",
  "save_type": "quicksave",
  "level": {
    "level_number": 7,
    "level_id": "K9P2A3X7",
    "biome": "forest",
    "time_elapsed": 45.23
  },
  "player": {
    "position": { "x": 234.5, "y": 48.0 },
    "velocity": { "x": 3.2, "y": 0.0 },
    "health": 3,
    "max_health": 5,
    "score": 12450,
    "facing_direction": "right",
    "active_powerup": "speed",
    "powerup_remaining_time": 4.2,
    "invulnerable": false,
    "is_grounded": true,
    "animation_state": "run"
  },
  "enemies": [
    {
      "type": "slime",
      "id": "enemy_003",
      "position": { "x": 340.0, "y": 32.0 },
      "health": 2,
      "state": "patrol",
      "alive": true
    },
    {
      "type": "goblin",
      "id": "enemy_007",
      "position": { "x": 520.0, "y": 64.0 },
      "health": 0,
      "state": "dead",
      "alive": false
    }
  ],
  "collectibles": [
    {
      "id": "coin_042",
      "collected": true
    },
    {
      "id": "coin_043",
      "collected": false,
      "position": { "x": 400.0, "y": 80.0 }
    }
  ],
  "checkpoints": {
    "last_activated": "checkpoint_02",
    "position": { "x": 200.0, "y": 32.0 }
  },
  "camera": {
    "position": { "x": 234.5, "y": 96.0 }
  },
  "hud": {
    "combo_count": 0,
    "combo_timer": 0
  },
  "settings_snapshot": {
    "control_scheme": "hybrid",
    "difficulty_assists": ["extended_coyote_time"],
    "color_blind_mode": "normal"
  }
}
```

---

## 4. App Termination & Restoration

### 4.1 Graceful Termination

If iOS sends `applicationWillTerminate` (rare, but possible):

| Step | Action | Notes |
|------|--------|-------|
| 1 | Save at last checkpoint | Uses checkpoint data, NOT quick-save (which may have mid-air state) |
| 2 | Write to `checkpoint.json` | Overwrites previous checkpoint save |
| 3 | Write to `backup_checkpoint.json` | Redundant copy for corruption protection |
| 4 | Sync to iCloud (if enabled) | Best-effort; may not complete |

### 4.2 Ungraceful Termination

If the app is killed without `applicationWillTerminate` (user force-quits, system kills for memory):

- Rely on the **most recent background save** (`quicksave.json` written in Step 4 of Section 3.1)
- If `quicksave.json` exists and is valid, restore from it on next launch
- If `quicksave.json` is missing or corrupt, fall back to `checkpoint.json`
- If both are corrupt, fall back to `backup_checkpoint.json`
- If all local saves are corrupt, attempt iCloud restore
- If no valid save exists, start fresh (Level 1)

### 4.3 Restoration on Cold Launch

When the app launches after a termination:

| Step | Action | Timing |
|------|--------|--------|
| 1 | Engine initialization, scene load | 0 - 1.5s |
| 2 | Check for save files (quicksave > checkpoint > backup > iCloud) | < 100ms |
| 3 | Validate save file integrity | < 50ms |
| 4 | If valid save: Show Main Menu with "Continue" highlighted | 1.5 - 2.5s |
| 5 | Player taps "Continue" | Player-initiated |
| 6 | Load level from save, restore at last checkpoint | < 2.0s |
| 7 | Show pause menu (game paused, player taps Resume) | Immediate |

**Player sees**: Launch screen -> Main Menu (with "Continue" option) -> Level loads -> Pause Menu -> Resume -> Gameplay at last checkpoint.

**Position restoration**: The player resumes at the **last activated checkpoint**, NOT at the exact mid-air position. This prevents edge cases (e.g., termination while falling into a pit, being mid-attack, or in an unrecoverable position).

### 4.4 What is Preserved vs. Lost on Termination

| Data | Preserved | Lost |
|------|-----------|------|
| Level number | Yes | - |
| Score at last checkpoint | Yes | Score gained since checkpoint |
| Health at last checkpoint | Yes | Health changes since checkpoint |
| Enemies defeated before checkpoint | Yes | Enemies defeated after checkpoint |
| Collectibles before checkpoint | Yes | Collectibles after checkpoint |
| Player position | Checkpoint position | Exact position since checkpoint |
| Active power-ups | No (expire on checkpoint load) | - |
| Settings & preferences | Yes (separate storage) | - |
| Tutorial progress | Yes | - |

---

## 5. Notification Handling

### 5.1 Notification Banner (Non-Pausing)

When a notification banner appears at the top of the screen while the game is in the foreground:

| Aspect | Behavior |
|--------|----------|
| Game state | CONTINUES PLAYING (no pause) |
| Audio | No change |
| Input | Game continues accepting input; banner does not block touch in gameplay area |
| HUD safety | All critical HUD elements are positioned to avoid the banner area (top 50pt of screen). The HUD already respects safe area + 20pt padding, which keeps elements clear of the notification banner zone. |
| Touch through | If player taps the notification banner, iOS handles it. The game does NOT intercept the tap. |

### 5.2 Critical UI Avoidance

To ensure notification banners never obscure critical gameplay information:

| HUD Element | Position (from top of safe area) | Banner Safe? |
|-------------|----------------------------------|--------------|
| Health Bar | 24pt from safe area top | Yes - notification banner sits above safe area |
| Score | 24pt from safe area top | Yes |
| Pause Button | 24pt from safe area top | Yes |
| Power-Up Slot | 24pt from safe area top | Yes |
| Combo Counter | 48pt from safe area top | Yes |

The iOS notification banner appears in the status bar area, which is OUTSIDE the safe area. Since all HUD elements are within the safe area (and further inset by 20pt), no gameplay-critical UI is ever obscured by notification banners.

### 5.3 Banner-Style Phone Calls (VoIP, FaceTime)

These use the compact call UI (iOS 14+). The compact banner appears at the top of the screen but does NOT cause `applicationWillResignActive` in some cases. The game should still:
- Continue gameplay if the banner is small and non-blocking
- Pause if the player taps the banner (which would background the app)

---

## 6. Auto-Save System

### 6.1 Save Trigger Points

| Trigger | Save Type | File | Notes |
|---------|-----------|------|-------|
| Checkpoint reached | Checkpoint save | `checkpoint.json` + `backup_checkpoint.json` | Full state at checkpoint |
| Level completed | Level save | `checkpoint.json` + `progress.json` | Records level completion, star rating, score |
| App backgrounding | Quick-save | `quicksave.json` | Complete state including mid-action |
| Manual pause | Quick-save | `quicksave.json` | Same as background save |
| Settings changed | Settings save | `settings.json` | Settings only, not game state |
| Tutorial milestone | Tutorial save | `tutorial_state.json` | Tutorial flags and ghost label counts |

### 6.2 Save File Locations

| File | Path | Purpose | Max Size |
|------|------|---------|----------|
| `quicksave.json` | `Application.persistentDataPath/saves/quicksave.json` | Most recent state | ~10 KB |
| `checkpoint.json` | `Application.persistentDataPath/saves/checkpoint.json` | Last checkpoint | ~8 KB |
| `backup_checkpoint.json` | `Application.persistentDataPath/saves/backup_checkpoint.json` | Previous checkpoint (safety net) | ~8 KB |
| `progress.json` | `Application.persistentDataPath/saves/progress.json` | Overall game progress (levels beaten, stars, scores) | ~20 KB |
| `settings.json` | `Application.persistentDataPath/settings.json` | Player preferences | ~2 KB |
| `tutorial_state.json` | `Application.persistentDataPath/saves/tutorial_state.json` | Tutorial progress | ~1 KB |

### 6.3 Save Performance Budget

| Operation | Max Time | Method |
|-----------|----------|--------|
| Serialize state to JSON | < 5ms | `JsonUtility.ToJson()` on pre-structured data |
| Write to disk | < 50ms | `File.WriteAllText()` with `FileStream.Flush()` |
| Compute checksum | < 2ms | CRC-32 on JSON string |
| Total save operation | < 60ms | Must not cause frame skip at 60fps (16.67ms/frame); save runs on background thread except during termination |

### 6.4 Save Threading

- **Normal saves** (checkpoint, pause): Serialization on main thread (< 5ms), disk write on background thread via `Task.Run()`
- **Background/termination saves**: Must complete synchronously on main thread (no time for async operations when iOS is suspending the app)
- **iCloud sync**: Always asynchronous, never blocks gameplay

### 6.5 Auto-Save Indicator

When an auto-save occurs, a small save icon appears in the top-right of the HUD:
- Icon: Floppy disk or circular progress icon (16 x 16 pt)
- Duration: 1.5 seconds, fades in 0.2s, holds 1.0s, fades out 0.3s
- Opacity: 60%
- Does NOT pause or interrupt gameplay
- Does NOT appear during manual pause saves or background saves (only checkpoint saves visible to player)

---

## 7. iCloud Sync & Conflict Resolution

### 7.1 What Syncs to iCloud

| Data | Syncs | Key | Notes |
|------|-------|-----|-------|
| Game progress (levels, stars, scores) | Yes | `icloud_progress` | Primary sync data |
| Checkpoint save | Yes | `icloud_checkpoint` | Latest checkpoint position |
| Settings | Yes | `icloud_settings` | Player preferences sync across devices |
| Tutorial state | Yes | `icloud_tutorial` | Avoids re-tutorial on new device |
| Quick-save | NO | N/A | Too transient, too frequent |
| Backup checkpoint | NO | N/A | Local safety net only |

### 7.2 Sync Frequency

| Trigger | Action |
|---------|--------|
| Level completed | Push progress to iCloud |
| App backgrounding | Push latest checkpoint to iCloud |
| App foregrounding | Pull latest from iCloud, compare |
| Settings changed | Push settings to iCloud |
| Manual "Sync Now" (settings) | Force pull + push |

### 7.3 Conflict Resolution (Newest Wins with Prompt)

When a conflict is detected (local data differs from iCloud data and neither is a subset of the other):

**Detection**: On foreground or on manual sync, compare `timestamp` fields of local and iCloud saves.

**Resolution Flow**:

```
+----------------------------------------------+
|                                                |
|  Save Conflict Detected                        |
|                                                |
|  Your progress differs between this            |
|  device and iCloud.                            |
|                                                |
|  This Device        iCloud                     |
|  Level 12           Level 14                   |
|  Score: 45,200      Score: 52,100              |
|  3-Star Levels: 8   3-Star Levels: 10          |
|  Last Played:        Last Played:              |
|  Feb 3, 10:30 PM     Feb 4, 8:15 AM           |
|                                                |
|  [USE THIS DEVICE]  [USE iCLOUD]               |
|                                                |
|  The newer save (iCloud) is recommended.       |
|                                                |
+----------------------------------------------+
```

| Resolution Choice | Action |
|-------------------|--------|
| Use This Device | Local data kept. Local data pushed to iCloud, overwriting remote. Dismissed save is stored in `conflict_backup.json` for 7 days. |
| Use iCloud | iCloud data downloaded and applied locally. Old local data stored in `conflict_backup.json` for 7 days. |
| Auto-resolve (if user has enabled "Auto Sync" in settings) | Newest timestamp wins automatically. No prompt. Older save backed up. |

**Prompt timing**: The conflict dialog appears on the Main Menu, never during gameplay. If a conflict is detected while the player is in a level, it is deferred until the player returns to the Main Menu.

### 7.4 Merge Strategy (Progress Data)

For `progress.json`, a merge is possible instead of a full overwrite:

```
For each level:
  merged.level[i].completed = local.level[i].completed OR icloud.level[i].completed
  merged.level[i].stars = max(local.level[i].stars, icloud.level[i].stars)
  merged.level[i].best_score = max(local.level[i].best_score, icloud.level[i].best_score)
  merged.level[i].best_time = min(local.level[i].best_time, icloud.level[i].best_time)
```

This merge takes the "best of both" for progress data. It is used when the conflict is only in progress data (not in checkpoint position or settings). The player is informed: "Your progress has been merged. Best scores and completion data from both devices are preserved."

### 7.5 Implementation

```csharp
// CloudSaveManager.cs
// Handles iCloud sync via NSUbiquitousKeyValueStore for small data
// and CloudKit for larger saves.

public class CloudSaveManager : MonoBehaviour
{
    public static CloudSaveManager Instance { get; private set; }

    [SerializeField] private bool iCloudEnabled = true;

    public event System.Action<ConflictData> OnConflictDetected;

    public void PushToCloud(SaveData localData)
    {
        if (!iCloudEnabled) return;

        #if UNITY_IOS && !UNITY_EDITOR
        string json = JsonUtility.ToJson(localData);
        _PushToiCloud("game_save", json, localData.timestamp);
        #endif
    }

    public void PullFromCloud(System.Action<SaveData> onComplete)
    {
        if (!iCloudEnabled)
        {
            onComplete?.Invoke(null);
            return;
        }

        #if UNITY_IOS && !UNITY_EDITOR
        _PullFromiCloud("game_save", (string json) =>
        {
            if (string.IsNullOrEmpty(json))
            {
                onComplete?.Invoke(null);
                return;
            }

            SaveData cloudData = JsonUtility.FromJson<SaveData>(json);
            SaveData localData = SaveManager.Instance.LoadLocalSave();

            if (HasConflict(localData, cloudData))
            {
                // Raise conflict for UI to handle
                OnConflictDetected?.Invoke(new ConflictData
                {
                    local = localData,
                    cloud = cloudData,
                    recommendation = cloudData.timestamp > localData.timestamp
                        ? ConflictResolution.UseCloud
                        : ConflictResolution.UseLocal
                });
            }
            else
            {
                // No conflict, take newer
                onComplete?.Invoke(
                    cloudData.timestamp > localData.timestamp ? cloudData : localData
                );
            }
        });
        #endif
    }

    private bool HasConflict(SaveData local, SaveData cloud)
    {
        if (local == null || cloud == null) return false;

        // Conflict if both have been modified since last sync
        // and they differ in non-mergeable ways (checkpoint position, current level)
        return local.timestamp > local.lastSyncTimestamp
            && cloud.timestamp > local.lastSyncTimestamp
            && local.checkpoint.levelNumber != cloud.checkpoint.levelNumber;
    }

    public SaveData MergeProgress(SaveData local, SaveData cloud)
    {
        SaveData merged = new SaveData();
        merged.timestamp = System.DateTime.UtcNow.ToString("o");

        for (int i = 0; i < Mathf.Max(local.levels.Count, cloud.levels.Count); i++)
        {
            LevelProgress localLevel = i < local.levels.Count ? local.levels[i] : null;
            LevelProgress cloudLevel = i < cloud.levels.Count ? cloud.levels[i] : null;

            merged.levels.Add(new LevelProgress
            {
                completed = (localLevel?.completed ?? false) || (cloudLevel?.completed ?? false),
                stars = Mathf.Max(localLevel?.stars ?? 0, cloudLevel?.stars ?? 0),
                bestScore = Mathf.Max(localLevel?.bestScore ?? 0, cloudLevel?.bestScore ?? 0),
                bestTime = Mathf.Min(
                    localLevel?.bestTime ?? float.MaxValue,
                    cloudLevel?.bestTime ?? float.MaxValue
                )
            });
        }

        return merged;
    }
}
```

---

## 8. Session Lifecycle & Performance Targets

### 8.1 Cold Launch (App Not in Memory)

**Target**: Main Menu visible in < 3 seconds from app icon tap.

| Phase | Duration | Activity |
|-------|----------|----------|
| iOS Launch | 0 - 0.5s | iOS loads the app binary, displays launch screen (static image) |
| Unity Init | 0.5 - 1.5s | Unity engine starts, loads first scene, initializes subsystems |
| Asset Loading | 1.5 - 2.0s | Load main menu assets (logo, buttons, background). Preload Level 1 assets in background. |
| Save Check | 2.0 - 2.1s | Check for existing saves, iCloud pull (non-blocking) |
| Menu Display | 2.1 - 2.5s | Animate main menu in (fade + slide, 0.3s) |
| Interactive | 2.5s | Menu is tappable |

**Optimization strategies**:
- Launch screen matches main menu background (seamless transition)
- Minimal first-scene: Main Menu scene contains only UI, no 3D/2D game world
- Level 1 assets preloaded during main menu idle time via `Addressables.LoadAssetAsync()`
- No network calls blocking launch (iCloud sync is fire-and-forget on launch)

### 8.2 Warm Resume (App in Memory, Returning from Background)

**Target**: Pause menu visible in < 1 second from app tap.

| Phase | Duration | Activity |
|-------|----------|----------|
| iOS Resume | 0 - 0.1s | iOS brings app to foreground |
| Unity Resume | 0.1 - 0.3s | `applicationWillEnterForeground` callback fires |
| State Restore | 0.3 - 0.5s | Validate and restore quick-save state |
| Pause Menu | 0.5 - 0.7s | Show pause overlay |
| Interactive | 0.7s | Pause menu is tappable |

### 8.3 Level Transition

**Target**: New level playable in < 2 seconds from trigger.

| Phase | Duration | Activity |
|-------|----------|----------|
| Trigger | 0s | Player reaches level exit OR taps "Next Level" |
| Save | 0 - 0.1s | Save level completion data |
| Transition Out | 0.1 - 0.5s | Fade/wipe transition animation (or simple fade in reduced motion mode) |
| Unload | 0.3 - 0.7s | Unload previous level assets (async, overlaps with load) |
| Generate | 0.3 - 1.0s | Generate new level from seed (procedural) |
| Load | 0.7 - 1.5s | Load level assets, instantiate tiles, enemies, collectibles |
| Transition In | 1.5 - 1.8s | Fade/wipe in |
| Interactive | 1.8 - 2.0s | Player can move |

**Loading indicator**: During the transition, a simple animated loading indicator is visible:
- Reduced motion OFF: Spinning pixel-art gear icon (24 x 24 pt), centered
- Reduced motion ON: Static gear icon with pulsing dots ("Loading..."), centered
- Background: The transition animation (fade/wipe) itself provides visual feedback
- Never show a blank screen

### 8.4 Pause / Unpause

| Action | Target Time | Notes |
|--------|-------------|-------|
| Pause (player taps "||") | < 1 frame (16.67ms) | `Time.timeScale = 0` on the same frame as input |
| Pause overlay visible | < 2 frames (33ms) | Overlay GameObject activated, alpha animated |
| Resume (player taps "Resume") | < 1 frame | `Time.timeScale = 1` on the same frame |
| Audio fade-in | 0.3s after resume | Gradual audio restoration |

### 8.5 Performance Monitoring

```csharp
// SessionLifecycleMonitor.cs
// Tracks and logs performance metrics for all session transitions.
// Data is used for optimization and regression detection.

public class SessionLifecycleMonitor : MonoBehaviour
{
    private float coldLaunchStartTime;
    private float warmResumeStartTime;
    private float levelTransitionStartTime;

    public void OnColdLaunchStart()
    {
        coldLaunchStartTime = Time.realtimeSinceStartup;
    }

    public void OnColdLaunchComplete()
    {
        float duration = Time.realtimeSinceStartup - coldLaunchStartTime;
        Debug.Log($"[Lifecycle] Cold launch: {duration:F2}s (target: <3.0s)");

        if (duration > 3.0f)
        {
            Debug.LogWarning($"[Lifecycle] Cold launch EXCEEDED target: {duration:F2}s");
        }

        // Analytics (if enabled)
        Analytics.LogEvent("cold_launch", new Dictionary<string, object>
        {
            { "duration_ms", (int)(duration * 1000) },
            { "device", SystemInfo.deviceModel },
            { "os_version", SystemInfo.operatingSystem }
        });
    }

    // Similar methods for warm resume and level transitions...
}
```

---

## 9. Save Corruption Handling

### 9.1 Corruption Detection

Every save file includes a checksum (CRC-32) computed over the JSON content (excluding the checksum field itself).

```csharp
// SaveIntegrity.cs
public static class SaveIntegrity
{
    public static string ComputeChecksum(string jsonWithoutChecksum)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonWithoutChecksum);
        uint crc = Crc32.Compute(bytes);
        return crc.ToString("X8"); // 8-character hex string
    }

    public static bool ValidateSave(string json)
    {
        try
        {
            var save = JsonUtility.FromJson<SaveData>(json);
            if (save == null || string.IsNullOrEmpty(save.checksum)) return false;

            string storedChecksum = save.checksum;
            save.checksum = "";
            string recomputed = ComputeChecksum(JsonUtility.ToJson(save));

            return storedChecksum == recomputed;
        }
        catch
        {
            return false;
        }
    }
}
```

### 9.2 Corruption Recovery Cascade

When a save file fails validation:

```
Attempt 1: Load quicksave.json
  |-- Valid? -> Use it
  |-- Invalid? -> Fall through

Attempt 2: Load checkpoint.json
  |-- Valid? -> Use it (player loses progress since last checkpoint)
  |-- Invalid? -> Fall through

Attempt 3: Load backup_checkpoint.json
  |-- Valid? -> Use it (player loses more progress, but data is safe)
  |-- Invalid? -> Fall through

Attempt 4: Pull from iCloud
  |-- Valid? -> Use it (may be older but uncorrupted)
  |-- Invalid or unavailable? -> Fall through

Attempt 5: Load progress.json only (no checkpoint)
  |-- Valid? -> Start at most recent unlocked level, beginning of level
  |-- Invalid? -> Fall through

Final Fallback: Fresh start
  |-- Delete all corrupt save files
  |-- Show dialog: "We couldn't restore your save data. Starting fresh."
  |-- Begin at Level 1
  |-- Log corruption event for analytics
```

### 9.3 Player Communication

| Scenario | Message | Buttons |
|----------|---------|---------|
| Quicksave corrupt, checkpoint valid | "Your most recent save was damaged. Resuming from last checkpoint." | [OK] |
| All local saves corrupt, iCloud valid | "Local save data was damaged. Restoring from iCloud backup." | [OK] |
| All saves corrupt | "We were unable to restore your save data. We're sorry for the inconvenience." | [Start Fresh] |
| Partial progress loss | "Some recent progress may have been lost. Your game has been restored to the last safe point." | [OK] |

Messages appear as modal dialogs on the Main Menu, never during gameplay.

### 9.4 Corruption Prevention

| Measure | Description |
|---------|-------------|
| Atomic writes | Write to a `.tmp` file, then rename to final filename. Prevents partial writes. |
| Double-write | Critical saves (checkpoint) are written to both `checkpoint.json` and `backup_checkpoint.json` sequentially. |
| Checksum | CRC-32 validation on every read. |
| Version field | Save format version allows migration if the format changes in updates. |
| Size validation | Reject saves smaller than 100 bytes (minimum valid save is ~500 bytes) or larger than 100KB (max expected is ~20KB). |

```csharp
// AtomicFileWriter.cs
public static class AtomicFileWriter
{
    public static void Write(string path, string content)
    {
        string tempPath = path + ".tmp";
        string backupPath = path + ".bak";

        // Write to temp file
        File.WriteAllText(tempPath, content);

        // Verify temp file is readable and valid
        string verification = File.ReadAllText(tempPath);
        if (verification != content)
        {
            throw new System.IO.IOException("Write verification failed");
        }

        // Backup existing file
        if (File.Exists(path))
        {
            if (File.Exists(backupPath)) File.Delete(backupPath);
            File.Move(path, backupPath);
        }

        // Rename temp to final
        File.Move(tempPath, path);

        // Clean up backup (optional, could keep for extra safety)
        // File.Delete(backupPath);
    }
}
```

---

## 10. Edge Cases & Error Recovery

### 10.1 Rapid Background/Foreground Cycling

If the user rapidly switches away and back (e.g., accidentally):

| Scenario | Behavior |
|----------|----------|
| Background + foreground within < 0.5s | Save fires on background. On foreground, quick-save is loaded (redundant but safe). Pause menu shown. |
| Background + foreground within < 0.1s | Save may not have completed writing to disk. In-memory state is still valid. Pause menu shown. Save completes asynchronously. |
| 5+ rapid cycles | Each background saves. No accumulation of state. No memory leaks. Each foreground loads latest valid state. Throttle: skip save if last save was < 0.1s ago. |

### 10.2 Interruption During Level Transition

| Scenario | Behavior |
|----------|----------|
| Background during level load | Level load pauses (Time.timeScale = 0). On resume, load continues. No data loss. |
| Termination during level load | No level save exists yet (old level was unloaded). On relaunch, restore from checkpoint in the PREVIOUS level. Player replays the level transition. |
| Background during save write | Atomic write handles this: if `.tmp` exists but final file does not, the save was incomplete. On resume, re-save. |

### 10.3 Disk Space Exhaustion

| Scenario | Behavior |
|----------|----------|
| Save fails due to disk full | Catch `IOException`. Show non-blocking banner: "Unable to save. Please free up storage." Game continues. Retry save on next trigger. |
| Repeated save failures | After 3 consecutive failures, show modal dialog: "Your progress cannot be saved. Please free up storage to avoid losing progress." [OK] |
| Critical save (background/termination) fails | Log error. On relaunch, player resumes from last successful save. No crash. |

### 10.4 iCloud Unavailable

| Scenario | Behavior |
|----------|----------|
| iCloud not signed in | iCloud features silently disabled. Local saves only. No error shown. |
| iCloud signed in but sync fails (network) | Retry on next trigger. No error shown (sync is opportunistic). |
| iCloud quota exceeded | Log warning. Disable iCloud push. Show in Settings: "iCloud sync paused: storage full." |
| iCloud data deleted by user (Settings > iCloud > Manage Storage) | On next foreground, pull returns empty. No conflict (local wins). Warn: "iCloud save not found. Using local save." |

### 10.5 App Update with Save Format Change

| Step | Action |
|------|--------|
| 1 | New version includes `save_version` increment (e.g., v2 -> v3) |
| 2 | On launch, `SaveMigrationManager` detects version mismatch |
| 3 | Migration code transforms old format to new format |
| 4 | Migrated save is validated and written |
| 5 | Old save backed up as `save_v2_backup.json` (kept for 30 days) |
| 6 | If migration fails: use old save with default values for new fields |

```csharp
// SaveMigrationManager.cs
public class SaveMigrationManager
{
    public SaveData Migrate(string json, int fromVersion, int toVersion)
    {
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (fromVersion < 2 && toVersion >= 2)
        {
            // v1 -> v2: Added difficulty_assists field
            data.settingsSnapshot.difficultyAssists = new List<string>();
        }

        if (fromVersion < 3 && toVersion >= 3)
        {
            // v2 -> v3: Added best_time per level
            foreach (var level in data.levels)
            {
                if (level.bestTime == 0) level.bestTime = float.MaxValue;
            }
        }

        data.version = toVersion;
        data.checksum = SaveIntegrity.ComputeChecksum(
            JsonUtility.ToJson(data)
        );

        return data;
    }
}
```

---

## 11. Unity Implementation

### 11.1 Application Lifecycle Hooks

```csharp
// AppLifecycleManager.cs
// Central manager for all iOS application lifecycle events.
// Coordinates pause, save, audio, and restore operations.

public class AppLifecycleManager : MonoBehaviour
{
    public static AppLifecycleManager Instance { get; private set; }

    // Dependencies
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private PauseMenuController pauseMenu;
    [SerializeField] private CloudSaveManager cloudSave;
    [SerializeField] private SessionLifecycleMonitor lifecycleMonitor;

    private bool isGameplayActive = false; // True when player is in a level
    private bool isPaused = false;
    private float resignActiveTime;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Called when app is about to lose focus (calls, notifications, control center)
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            // Losing focus
            resignActiveTime = Time.realtimeSinceStartup;
            HandleResignActive();
        }
        else
        {
            // Gaining focus
            float pauseDuration = Time.realtimeSinceStartup - resignActiveTime;
            HandleBecomeActive(pauseDuration);
        }
    }

    // Called when app enters/exits background
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Entering background
            HandleEnterBackground();
        }
        else
        {
            // Returning to foreground
            lifecycleMonitor.OnWarmResumeStart();
            HandleEnterForeground();
        }
    }

    private void HandleResignActive()
    {
        if (!isGameplayActive) return;

        // Immediate pause
        Time.timeScale = 0f;
        isPaused = true;

        // Mute audio
        audioManager.MuteAll(fadeDuration: 0.1f);

        // Quick save
        saveManager.QuickSave();
    }

    private void HandleBecomeActive(float pauseDuration)
    {
        if (!isGameplayActive) return;

        if (pauseDuration < 3.0f && !isPaused)
        {
            // Very brief interruption (notification banner dismissed quickly)
            // Auto-resume
            Time.timeScale = 1f;
            audioManager.UnmuteAll(fadeDuration: 0.3f);
        }
        else
        {
            // Show pause menu, wait for player to Resume
            pauseMenu.Show();
            // Audio and timeScale restored when player taps Resume
        }
    }

    private void HandleEnterBackground()
    {
        // Already paused from ResignActive, but ensure everything is saved
        Time.timeScale = 0f;
        audioManager.MuteAll(fadeDuration: 0f); // Immediate mute

        // Full state save (synchronous, must complete before iOS suspends)
        saveManager.FullStateSave();

        // Push to iCloud (async, best-effort)
        cloudSave.PushToCloud(saveManager.GetCurrentSaveData());

        // Release non-essential memory
        Resources.UnloadUnusedAssets();
    }

    private void HandleEnterForeground()
    {
        // Validate and restore state
        if (saveManager.ValidateQuickSave())
        {
            saveManager.RestoreFromQuickSave();
        }
        else
        {
            // Quick save corrupt; fall back to checkpoint
            saveManager.RestoreFromCheckpoint();
        }

        // Show pause menu
        pauseMenu.Show();

        // Check for iCloud conflicts (deferred to main menu if in gameplay)
        cloudSave.PullFromCloud(null); // Non-blocking check

        lifecycleMonitor.OnWarmResumeComplete();
    }

    // Called by PauseMenuController when player taps Resume
    public void ResumeGameplay()
    {
        Time.timeScale = 1f;
        isPaused = false;
        audioManager.UnmuteAll(fadeDuration: 0.3f);
        pauseMenu.Hide();
    }
}
```

### 11.2 Save Manager

```csharp
// SaveManager.cs
// Handles all save/load operations with integrity checking
// and corruption recovery.

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string savePath;
    private const int CURRENT_SAVE_VERSION = 2;

    void Awake()
    {
        Instance = this;
        savePath = System.IO.Path.Combine(Application.persistentDataPath, "saves");

        if (!System.IO.Directory.Exists(savePath))
        {
            System.IO.Directory.CreateDirectory(savePath);
        }
    }

    public void QuickSave()
    {
        SaveData data = CaptureCurrentState();
        data.saveType = "quicksave";
        data.timestamp = System.DateTime.UtcNow.ToString("o");
        WriteSave(System.IO.Path.Combine(savePath, "quicksave.json"), data);
    }

    public void CheckpointSave()
    {
        SaveData data = CaptureCurrentState();
        data.saveType = "checkpoint";
        data.timestamp = System.DateTime.UtcNow.ToString("o");

        // Write to both checkpoint and backup
        string checkpointPath = System.IO.Path.Combine(savePath, "checkpoint.json");
        string backupPath = System.IO.Path.Combine(savePath, "backup_checkpoint.json");

        WriteSave(checkpointPath, data);
        WriteSave(backupPath, data);

        // Show save indicator in HUD
        HUDManager.Instance.ShowSaveIndicator();
    }

    public void FullStateSave()
    {
        // Called during backgrounding; must be synchronous
        QuickSave();
        // Also update checkpoint if we're at a safe position
        if (IsAtSafePosition())
        {
            CheckpointSave();
        }
    }

    private void WriteSave(string path, SaveData data)
    {
        // Compute checksum
        data.checksum = "";
        string jsonWithoutChecksum = JsonUtility.ToJson(data, true);
        data.checksum = SaveIntegrity.ComputeChecksum(jsonWithoutChecksum);

        string finalJson = JsonUtility.ToJson(data, true);

        try
        {
            AtomicFileWriter.Write(path, finalJson);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to write save: {e.Message}");
            // Track failure count for disk-full handling
        }
    }

    public bool ValidateQuickSave()
    {
        string path = System.IO.Path.Combine(savePath, "quicksave.json");
        if (!System.IO.File.Exists(path)) return false;

        string json = System.IO.File.ReadAllText(path);
        return SaveIntegrity.ValidateSave(json);
    }

    public SaveData LoadWithRecovery()
    {
        // Cascade through save files
        string[] paths = {
            System.IO.Path.Combine(savePath, "quicksave.json"),
            System.IO.Path.Combine(savePath, "checkpoint.json"),
            System.IO.Path.Combine(savePath, "backup_checkpoint.json")
        };

        foreach (string path in paths)
        {
            if (!System.IO.File.Exists(path)) continue;

            string json = System.IO.File.ReadAllText(path);
            if (SaveIntegrity.ValidateSave(json))
            {
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                // Check version, migrate if needed
                if (data.version < CURRENT_SAVE_VERSION)
                {
                    data = new SaveMigrationManager().Migrate(
                        json, data.version, CURRENT_SAVE_VERSION
                    );
                }

                return data;
            }
            else
            {
                Debug.LogWarning($"[SaveManager] Corrupt save file: {path}");
            }
        }

        // All local saves failed; try iCloud
        // (This would be async in practice)
        Debug.LogError("[SaveManager] All local saves corrupt. Attempting iCloud restore.");
        return null; // Caller handles fresh start
    }

    private SaveData CaptureCurrentState()
    {
        // Gather state from all game systems
        var player = PlayerController.Instance;
        var level = LevelManager.Instance;
        var hud = HUDManager.Instance;

        return new SaveData
        {
            version = CURRENT_SAVE_VERSION,
            level = new LevelData
            {
                levelNumber = level.CurrentLevelNumber,
                levelId = level.CurrentLevelId,
                biome = level.CurrentBiome,
                timeElapsed = level.ElapsedTime
            },
            player = new PlayerData
            {
                position = player.transform.position,
                velocity = player.Rigidbody.velocity,
                health = player.CurrentHealth,
                maxHealth = player.MaxHealth,
                score = player.Score,
                facingDirection = player.FacingRight ? "right" : "left",
                activePowerup = player.ActivePowerup?.name ?? "",
                powerupRemainingTime = player.PowerupTimeRemaining,
                isGrounded = player.IsGrounded
            },
            enemies = EnemyManager.Instance.SerializeEnemies(),
            collectibles = CollectibleManager.Instance.SerializeCollectibles(),
            checkpoints = CheckpointManager.Instance.SerializeCheckpoints()
        };
    }
}
```

### 11.3 Audio Session Configuration

For proper audio interruption handling on iOS:

```csharp
// iOSAudioSessionSetup.cs
// Configure the iOS audio session for proper interruption behavior.
// Must run before any audio plays.

public class iOSAudioSessionSetup
{
    public static void Configure()
    {
        #if UNITY_IOS && !UNITY_EDITOR
        // Use Ambient category: mixes with other apps, respects silent switch,
        // allows phone calls to interrupt gracefully.
        // If the game needs to play audio over other apps, use SoloAmbient instead.

        // Native plugin call to configure AVAudioSession:
        // Category: .ambient
        // Mode: .default
        // Options: .mixWithOthers

        _ConfigureAudioSession(
            category: 0,  // ambient
            mode: 0,      // default
            options: 1     // mixWithOthers
        );
        #endif
    }

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void _ConfigureAudioSession(int category, int mode, int options);
}
```

---

**Cross-References**:
- HUD element positions (notification-safe placement): See `ui_layout_spec.md` (Assessment 5.1)
- Haptic feedback on checkpoint: See `control_layout_spec.md` (Assessment 5.2)
- Save persistence for accessibility settings: See `accessibility_spec.md` (Assessment 5.3)
- Checkpoint auto-save in tutorial: See `onboarding_flow.md` (Assessment 5.4)

---

Last Updated: 2026-02-04
Status: Active
Version: 1.0
