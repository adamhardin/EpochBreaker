# Assessment 5.1 - Adaptive UI Layout Specification

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

1. [Device Matrix & Safe Areas](#1-device-matrix--safe-areas)
2. [Global Layout Rules](#2-global-layout-rules)
3. [Screen Specifications](#3-screen-specifications)
   - 3.1 Main Menu
   - 3.2 HUD (In-Game)
   - 3.3 Pause Menu
   - 3.4 Level Complete
   - 3.5 Settings
   - 3.6 Level ID Entry/Share
4. [Typography System](#4-typography-system)
5. [Unity Implementation](#5-unity-implementation)
6. [Testing Checklist](#6-testing-checklist)

---

## 1. Device Matrix & Safe Areas

All measurements below are in **points (pt)** in **landscape orientation** (home button / swipe bar on the right).

### 1.1 Supported Devices

| Device | Screen (pt, landscape) | Render Scale | Notch Type | Status Bar |
|--------|----------------------|--------------|------------|------------|
| iPhone 11 | 896 x 414 | @2x | Notch (left) | Hidden |
| iPhone 12 / 13 | 844 x 390 | @3x | Notch (left) | Hidden |
| iPhone 14 | 844 x 390 | @3x | Notch (left) | Hidden |
| iPhone 14 Pro | 852 x 393 | @3x | Dynamic Island (left) | Hidden |
| iPhone 15 Pro | 852 x 393 | @3x | Dynamic Island (left) | Hidden |
| iPhone 15 Pro Max | 932 x 430 | @3x | Dynamic Island (left) | Hidden |

### 1.2 Safe Area Insets (Landscape, points)

When the device is held in landscape-right (notch/Dynamic Island on the LEFT side):

| Device | Left (notch side) | Right (home side) | Top | Bottom |
|--------|-------------------|-------------------|-----|--------|
| iPhone 11 | 48 | 0 | 0 | 21 |
| iPhone 12 / 13 | 47 | 0 | 0 | 21 |
| iPhone 14 | 47 | 0 | 0 | 21 |
| iPhone 14 Pro | 59 | 0 | 0 | 21 |
| iPhone 15 Pro | 59 | 0 | 0 | 21 |
| iPhone 15 Pro Max | 59 | 0 | 0 | 21 |

When held in landscape-left (notch/Dynamic Island on the RIGHT side):

| Device | Left | Right (notch side) | Top | Bottom |
|--------|------|---------------------|-----|--------|
| iPhone 11 | 0 | 48 | 0 | 21 |
| iPhone 12 / 13 | 0 | 47 | 0 | 21 |
| iPhone 14 | 0 | 47 | 0 | 21 |
| iPhone 14 Pro | 0 | 59 | 0 | 21 |
| iPhone 15 Pro | 0 | 59 | 0 | 21 |
| iPhone 15 Pro Max | 0 | 59 | 0 | 21 |

### 1.3 Safe Area Strategy

Since the game supports both landscape orientations (auto-rotate within landscape), insets must be applied symmetrically using the **maximum** of left/right insets:

| Device | Effective Left Inset | Effective Right Inset | Top | Bottom |
|--------|---------------------|-----------------------|-----|--------|
| iPhone 11 | 48 | 48 | 0 | 21 |
| iPhone 14 Pro | 59 | 59 | 0 | 21 |
| iPhone 15 Pro Max | 59 | 59 | 0 | 21 |

**Implementation Rule**: All interactive UI elements must be placed within the safe area, then further inset by an additional **20pt** padding from each edge of the safe area. This ensures comfortable reach and avoids accidental edge gestures.

**Effective interactive zone** (landscape, worst case -- iPhone 15 Pro Max):
- Left boundary: 59 + 20 = 79pt from screen left
- Right boundary: 59 + 20 = 79pt from screen right
- Top boundary: 0 + 20 = 20pt from screen top
- Bottom boundary: 21 + 20 = 41pt from screen bottom
- Usable width: 932 - 79 - 79 = 774pt
- Usable height: 430 - 20 - 41 = 369pt

---

## 2. Global Layout Rules

### 2.1 Orientation Lock

```
Unity Setting: Player Settings > Resolution and Presentation
  Default Orientation: Landscape Left
  Allowed Orientations:
    Portrait: OFF
    Portrait Upside Down: OFF
    Landscape Right: ON
    Landscape Left: ON
```

The game locks to landscape. Both landscape-left and landscape-right are permitted to accommodate left-handed and right-handed players. The UI uses Unity's `Screen.safeArea` to adapt dynamically.

### 2.2 Coordinate System

All positions in this document use the following convention:
- **Origin**: Bottom-left corner of the safe area
- **X**: Increases rightward
- **Y**: Increases upward
- **Percentage values**: Relative to the safe area dimensions, NOT the full screen
- **Point values (pt)**: Absolute sizes that remain constant across devices

### 2.3 Touch Target Rules

| Rule | Value | Source |
|------|-------|--------|
| Minimum touch target | 44 x 44 pt | Apple HIG |
| Recommended touch target | 48 x 48 pt | Internal standard |
| Minimum spacing between targets | 8 pt | Apple HIG |
| Edge exclusion zone | 20 pt from safe area edge | Internal standard |
| Maximum button size (HUD) | 72 x 72 pt | Prevents occlusion |

### 2.4 Z-Ordering (Render Layers)

| Layer | Sort Order | Contents |
|-------|-----------|----------|
| Game World | 0 | Backgrounds, tiles, entities |
| Game Effects | 100 | Particles, screen shake |
| HUD Background | 200 | Health bar background, score background |
| HUD Foreground | 300 | Health icons, score text, power-up icons |
| Touch Controls | 400 | D-pad, action buttons (semi-transparent) |
| Overlay | 500 | Pause menu, level complete, dialogs |
| System | 600 | Loading screens, error dialogs |

---

## 3. Screen Specifications

### 3.1 Main Menu

**Purpose**: Primary entry point. Game logo, navigation to play, settings, and level sharing.

**Background**: Animated 16-bit pixel art scene (parallax, 3 layers). Character idle animation centered.

**Layout** (all positions relative to safe area):

```
+------------------------------------------------------------------+
|                                                                    |
|                     [GAME LOGO]                                    |
|                   center-x, top 15%                                |
|                   320 x 80 pt                                      |
|                                                                    |
|                                                                    |
|                     [PLAY]                                         |
|                   center-x, 45% from top                           |
|                   200 x 56 pt                                      |
|                                                                    |
|                [LEVEL ID]    [SETTINGS]                             |
|              center-x - 110, center-x + 110                        |
|                65% from top                                        |
|                160 x 48 pt each                                    |
|                                                                    |
|                                                                    |
|  [v1.0]                                              [AUDIO ON]   |
|  bottom-left + 20pt                               bottom-right    |
+------------------------------------------------------------------+
```

| Element | Position (from safe area) | Size (pt) | Touch Target (pt) | Notes |
|---------|--------------------------|-----------|-------------------|-------|
| Game Logo | center-x, y = 85% height | 320 x 80 | Non-interactive | Sprite, pixel-perfect scaling |
| Play Button | center-x, y = 55% height | 200 x 56 | 200 x 56 | Primary CTA, pulse animation |
| Level ID Button | center-x - 110, y = 35% height | 160 x 48 | 160 x 48 | Opens level ID entry screen |
| Settings Button | center-x + 110, y = 35% height | 160 x 48 | 160 x 48 | Opens settings screen |
| Version Label | left + 24, y = 24 | auto x 14 | Non-interactive | "v1.0.0", 11pt, 50% opacity |
| Audio Toggle | right - 24, y = 24 | 44 x 44 | 48 x 48 | Speaker icon, toggles mute |

**Interactions**:
- Play: Scale up 1.1x on press, transition to level select or resume
- Level ID / Settings: Highlight border on press
- Audio Toggle: Icon swap (speaker / speaker-muted), haptic light tap

**Transitions**:
- To gameplay: Fade out over 0.3s, level loads behind fade
- To settings: Slide in from right, 0.25s ease-in-out
- To level ID: Slide in from bottom, 0.25s ease-in-out

### 3.2 HUD (In-Game)

**Purpose**: Persistent gameplay overlay showing player status. Must not obstruct the central gameplay area.

**Design Principle**: HUD elements hug the top-left and top-right corners within the safe area. The bottom portion is reserved for touch controls (see Assessment 5.2). The center 70% of the screen remains clear for gameplay visibility.

**Layout**:

```
+------------------------------------------------------------------+
| [HP: *** ]  [SCORE: 00000]                    [POWER-UP]  [||]   |
|  top-left    left + 180pt                     right - 80   right  |
|                                                                    |
|                                                                    |
|                    << GAMEPLAY AREA >>                              |
|                    (center 70% clear)                              |
|                                                                    |
|                                                                    |
|  [D-PAD]                                        [A] [B]           |
|  bottom-left                                  bottom-right         |
|  (touch controls - see 5.2)                   (touch controls)     |
+------------------------------------------------------------------+
```

| Element | Position (from safe area) | Size (pt) | Touch Target (pt) | Notes |
|---------|--------------------------|-----------|-------------------|-------|
| Health Bar | left + 24, top - 24 | 140 x 20 | Non-interactive | 16-bit heart icons (max 5) |
| Score Display | left + 180, top - 24 | 120 x 20 | Non-interactive | "00000" monospaced, right-aligned |
| Power-Up Slot | right - 80, top - 24 | 36 x 36 | Non-interactive | Shows active power-up icon, empty when none |
| Pause Button | right - 24, top - 24 | 36 x 36 | 48 x 48 | Two vertical bars "||" icon |
| Combo Counter | center-x, top - 48 | 80 x 28 | Non-interactive | Appears only during combos, fades after 2s |

**Health Bar Detail**:
- 5 hearts, each 24 x 20 pt, 4pt spacing
- Full heart: filled red (#E04040)
- Half heart: half-filled
- Empty heart: outline only (#666666)
- Total width: (24 * 5) + (4 * 4) = 136pt
- High contrast mode: Full = white, Empty = dark gray outline with X

**Score Display Detail**:
- Font: Pixel monospaced, 17pt
- Color: White (#FFFFFF) with 1px black outline
- Format: 5 digits, zero-padded
- Pop animation on score increase (scale to 1.2x, return over 0.15s)

**Pause Button Detail**:
- Always accessible, never obscured
- Minimum 20pt from any screen edge (after safe area)
- Tap triggers immediate game pause (same frame)
- Haptic: light impact on tap

### 3.3 Pause Menu

**Purpose**: Overlay that appears when the game is paused. Game world is visible but dimmed behind.

**Background**: Semi-transparent black overlay (#000000 at 60% opacity) over frozen game frame.

**Layout**:

```
+------------------------------------------------------------------+
|                                                                    |
|                     [PAUSED]                                       |
|                   center, top 20%                                  |
|                                                                    |
|                    [RESUME]                                        |
|                   center, 40%                                      |
|                   220 x 52 pt                                      |
|                                                                    |
|                    [RESTART]                                       |
|                   center, 55%                                      |
|                   220 x 52 pt                                      |
|                                                                    |
|                   [SETTINGS]                                       |
|                   center, 70%                                      |
|                   220 x 52 pt                                      |
|                                                                    |
|                  [QUIT TO MENU]                                    |
|                   center, 85%                                      |
|                   220 x 52 pt                                      |
|                                                                    |
+------------------------------------------------------------------+
```

| Element | Position (center of safe area) | Size (pt) | Touch Target (pt) | Notes |
|---------|-------------------------------|-----------|-------------------|-------|
| "PAUSED" Title | center-x, y = 80% | auto x 28 | Non-interactive | 28pt pixel font, white |
| Resume Button | center-x, y = 60% | 220 x 52 | 220 x 56 | Primary action, highlighted border |
| Restart Button | center-x, y = 45% | 220 x 52 | 220 x 56 | Confirmation dialog before executing |
| Settings Button | center-x, y = 30% | 220 x 52 | 220 x 56 | Opens settings as sub-overlay |
| Quit to Menu | center-x, y = 15% | 220 x 52 | 220 x 56 | Confirmation dialog, saves progress |

**Button Spacing**: 15% of safe area height between centers = ~55-65pt depending on device. Minimum spacing between touch targets: 8pt (achieved with 52pt height and ~10pt gap).

**Interactions**:
- Resume: Dismiss overlay, unpause game, 0.15s fade out
- Restart: Show confirmation ("Restart level? Progress will be lost." [Yes] [No])
- Settings: Slide settings panel in from right
- Quit: Show confirmation ("Quit to menu? Progress saved at last checkpoint." [Yes] [No])

**Confirmation Dialog**:
- Size: 320 x 160 pt, centered
- Background: Dark panel (#1A1A2E, 95% opacity)
- Text: 14pt, white, centered
- Buttons: [Yes] 120 x 48 pt, [No] 120 x 48 pt, side by side with 16pt gap
- Touch targets: 120 x 52 pt minimum

### 3.4 Level Complete

**Purpose**: Shown after completing a level. Displays score, star rating, and navigation options.

**Background**: Game world frozen, gradual brightness increase (celebration effect). Confetti particle overlay.

**Layout**:

```
+------------------------------------------------------------------+
|                                                                    |
|                  [LEVEL COMPLETE!]                                  |
|                   center, top 15%                                  |
|                                                                    |
|              [*]      [*]      [*]                                 |
|              Star rating - center, 35%                             |
|              48pt each, 24pt spacing                               |
|                                                                    |
|               Score: 12,450                                        |
|               Best:  15,200                                        |
|               Time:  01:23                                         |
|               center, 50-65%                                       |
|                                                                    |
|         [REPLAY]   [SHARE ID]   [NEXT LEVEL]                      |
|          center, 80%                                               |
|         140x48     140x48       180x48                             |
|                                                                    |
+------------------------------------------------------------------+
```

| Element | Position | Size (pt) | Touch Target (pt) | Notes |
|---------|----------|-----------|-------------------|-------|
| "LEVEL COMPLETE!" | center-x, y = 85% | auto x 28 | Non-interactive | 28pt, gold color (#FFD700), bounce-in animation |
| Star 1 | center-x - 72, y = 65% | 48 x 48 | Non-interactive | Animate: fly in from left, 0.3s delay |
| Star 2 | center-x, y = 65% | 48 x 48 | Non-interactive | Animate: fly in from top, 0.5s delay |
| Star 3 | center-x + 72, y = 65% | 48 x 48 | Non-interactive | Animate: fly in from right, 0.7s delay |
| Score Label | center-x, y = 50% | auto x 17 | Non-interactive | "Score: 12,450" 17pt white |
| Best Label | center-x, y = 44% | auto x 14 | Non-interactive | "Best: 15,200" 14pt, gray if not beaten, gold if new best |
| Time Label | center-x, y = 38% | auto x 14 | Non-interactive | "Time: 01:23" 14pt white |
| Replay Button | center-x - 170, y = 18% | 140 x 48 | 144 x 52 | Circular arrow icon + "REPLAY" |
| Share ID Button | center-x, y = 18% | 140 x 48 | 144 x 52 | Share icon + "SHARE ID" |
| Next Level Button | center-x + 170, y = 18% | 180 x 48 | 184 x 52 | Arrow right icon + "NEXT", primary highlight |

**Star Rating Criteria**:
- 1 star: Level completed
- 2 stars: Completed with >= 50% health remaining
- 3 stars: Completed with >= 80% health, under par time, >= 90% enemies defeated

**Animations**:
- Title: Bounce in from top (0.5s, ease-out-back)
- Stars: Sequential fly-in with sparkle effect
- Score: Count-up animation from 0 to final (1.0s)
- Buttons: Fade in after score animation completes (0.3s)

### 3.5 Settings

**Purpose**: Comprehensive settings panel accessible from main menu and pause menu.

**Layout**: Scrollable list within a panel. When accessed from pause, overlays the game. When accessed from main menu, replaces main menu content.

**Panel Size**: 600 x 340 pt, centered in safe area (or full safe area width on smaller devices with 24pt margin).

```
+------------------------------------------------------------------+
|  [<< Back]              SETTINGS                                   |
|  ----------------------------------------------------------------  |
|  CONTROLS                                                          |
|    Control Scheme    [Button] [Gesture] [Hybrid]                   |
|    Control Opacity   [=====>--------] 75%                          |
|    Customize Layout  [EDIT POSITIONS >>]                           |
|  ----------------------------------------------------------------  |
|  AUDIO                                                             |
|    Music Volume      [========>-----] 80%                          |
|    SFX Volume        [===========>--] 95%                          |
|    Haptic Feedback   [ON / OFF]                                    |
|  ----------------------------------------------------------------  |
|  DISPLAY                                                           |
|    Color-Blind Mode  [Off / Protanopia / Deuteranopia / Tritanopia]|
|    High Contrast     [ON / OFF]                                    |
|    Reduced Motion    [System / On / Off]                           |
|    Show FPS          [ON / OFF]                                    |
|  ----------------------------------------------------------------  |
|  ACCESSIBILITY                                                     |
|    Dynamic Type      [System / Manual] [A- A+]                     |
|    One-Handed Mode   [OFF / Left / Right]                          |
|    Difficulty Assist  [OFF / ON >>]                                |
|    VoiceOver Labels  [ON / OFF]                                    |
|  ----------------------------------------------------------------  |
|  GAME                                                              |
|    Tutorial Replay   [REPLAY TUTORIAL]                             |
|    Session Reminder  [Off / 30min / 60min]                         |
|    iCloud Sync       [ON / OFF]                                    |
|    Reset Progress    [RESET...]                                    |
|  ----------------------------------------------------------------  |
|                                              [v1.0.0 build 42]    |
+------------------------------------------------------------------+
```

| Element | Size (pt) | Touch Target (pt) | Notes |
|---------|-----------|-------------------|-------|
| Back Button | 44 x 44 | 48 x 48 | "<< Back" text + chevron icon |
| Section Header | full width x 28 | Non-interactive | 17pt, bold, left-aligned |
| Setting Label | 200 x 20 | Non-interactive | 14pt, left-aligned, left column |
| Toggle Switch | 51 x 31 | 51 x 44 | iOS-style toggle, expanded touch target |
| Slider | 200 x 28 | 200 x 44 | Track: 200 x 4, thumb: 28 x 28 circle |
| Segmented Control | auto x 32 | auto x 44 | Per segment: minimum 60 x 44 |
| Action Button | 180 x 44 | 180 x 48 | "EDIT POSITIONS", "REPLAY TUTORIAL", etc. |
| Destructive Button | 180 x 44 | 180 x 48 | "RESET..." red text, requires confirmation |

**Scroll Behavior**:
- Content height exceeds panel: vertical scroll enabled
- Scroll indicator: thin 3pt bar on right edge
- Overscroll: elastic bounce (0.15s)
- Sections collapse/expand on header tap (optional, default all expanded)

**Row Height**: Each setting row = 52pt (20pt content + 16pt padding top/bottom)
**Section Spacing**: 12pt between sections
**Total Content Height**: ~680pt (scrollable in 340pt viewport)

### 3.6 Level ID Entry / Share

**Purpose**: Enter a level ID to play a specific procedurally generated level, or share the current level's ID.

**Layout**:

```
+------------------------------------------------------------------+
|  [<< Back]            LEVEL ID                                     |
|  ----------------------------------------------------------------  |
|                                                                    |
|         Enter a Level ID to play that exact level:                 |
|                                                                    |
|         +--------------------------------------+                   |
|         |  A 3 X 7 K 9 P 2                     |                   |
|         +--------------------------------------+                   |
|         ID field: 360 x 52 pt, center                              |
|                                                                    |
|              [PASTE]           [PLAY]                               |
|             120x48            140x48                                |
|                                                                    |
|  ----------------------------------------------------------------  |
|                                                                    |
|         Share your current level:                                  |
|                                                                    |
|         Level ID: K9P2A3X7                                         |
|         (displayed in 20pt monospaced, selectable)                 |
|                                                                    |
|         [COPY ID]    [SHARE...]                                    |
|         120x48       120x48                                        |
|                                                                    |
+------------------------------------------------------------------+
```

| Element | Position | Size (pt) | Touch Target (pt) | Notes |
|---------|----------|-----------|-------------------|-------|
| Back Button | top-left + 24 | 44 x 44 | 48 x 48 | Returns to main menu or pause |
| Title | center-x, top + 24 | auto x 22 | Non-interactive | "LEVEL ID", 22pt |
| Input Label | center-x, y = 75% | auto x 14 | Non-interactive | 14pt, description text |
| ID Text Field | center-x, y = 65% | 360 x 52 | 360 x 56 | Monospaced 20pt, max 8 chars, alphanumeric |
| Paste Button | center-x - 75, y = 52% | 120 x 48 | 124 x 52 | Clipboard icon + "PASTE" |
| Play Button | center-x + 75, y = 52% | 140 x 48 | 144 x 52 | Play icon + "PLAY", primary CTA |
| Divider | center-x, y = 45% | 400 x 1 | Non-interactive | 1pt line, 30% opacity white |
| Share Label | center-x, y = 38% | auto x 14 | Non-interactive | "Share your current level:" |
| Level ID Display | center-x, y = 30% | auto x 20 | long-press to select | Monospaced, white, current level ID |
| Copy Button | center-x - 75, y = 20% | 120 x 48 | 124 x 52 | Copy icon + "COPY ID" |
| Share Button | center-x + 75, y = 20% | 120 x 48 | 124 x 52 | Share icon + "SHARE..." (iOS share sheet) |

**ID Format**: 8 alphanumeric characters (A-Z, 0-9), displayed with spaces every 4 characters for readability (e.g., "A3X7 K9P2").

**Keyboard Handling**:
- Custom alphanumeric keyboard is NOT used; the system keyboard is suppressed
- Input via a large, game-styled alphanumeric grid (6 rows x 6 columns) or paste from clipboard
- Alternatively, standard iOS keyboard with `UIKeyboardType.asciiCapable` if native input is preferred
- Keyboard appearance: dark style to match game aesthetic

**Validation**:
- Invalid ID: Field border turns red (#E04040), shake animation (3 cycles, 4pt amplitude, 0.3s)
- Valid ID: Field border turns green (#40E040), 0.2s
- Loading: Spinner replaces Play button text while level generates

---

## 4. Typography System

### 4.1 Font Stack

| Usage | Font | Fallback | Notes |
|-------|------|----------|-------|
| Pixel UI (headers, buttons) | Custom pixel font (e.g., "Press Start 2P" or custom) | System bold | Rendered at integer multiples of base size |
| Body text (descriptions) | SF Pro Text (system) | Helvetica Neue | Dynamic Type compatible |
| Monospaced (scores, IDs) | SF Mono / custom pixel mono | Courier | Fixed-width for alignment |

### 4.2 Size Scale

| Style | Default Size (pt) | Min (pt) | Max (Dynamic Type, pt) | Line Height | Usage |
|-------|--------------------|----------|------------------------|-------------|-------|
| Title Large | 28 | 28 | 36 | 1.2x | Screen titles ("PAUSED", "LEVEL COMPLETE!") |
| Title | 22 | 22 | 30 | 1.2x | Section headers ("SETTINGS") |
| Heading | 17 | 17 | 24 | 1.3x | Setting section labels, button text |
| Body | 14 | 14 | 22 | 1.4x | Descriptions, setting labels |
| Caption | 12 | 11 | 18 | 1.3x | Version numbers, timestamps |
| Minimum | 11 | 11 | 16 | 1.3x | Legal text, build info only |

### 4.3 Color Palette (Text)

| Context | Color (Hex) | Opacity | Contrast Ratio (on #1A1A2E) |
|---------|-------------|---------|----------------------------|
| Primary text | #FFFFFF | 100% | 13.5:1 |
| Secondary text | #B0B0C0 | 100% | 7.2:1 |
| Disabled text | #606070 | 100% | 3.1:1 (decorative only) |
| Accent / CTA | #FFD700 | 100% | 10.8:1 |
| Error | #FF4444 | 100% | 5.6:1 |
| Success | #44FF44 | 100% | 11.2:1 |
| High contrast primary | #FFFFFF | 100% | 13.5:1 |
| High contrast secondary | #E0E0E0 | 100% | 11.9:1 |

All text in standard mode meets WCAG AA (4.5:1 for normal text, 3:1 for large text).
All text in high contrast mode meets WCAG AAA (7:1).

---

## 5. Unity Implementation

### 5.1 Canvas Configuration

```
Canvas:
  Render Mode: Screen Space - Overlay
  UI Scale Mode: Scale With Screen Size
  Reference Resolution: 896 x 414 (iPhone 11 base)
  Screen Match Mode: Match Width Or Height
  Match: 0.5 (balanced)
  Reference Pixels Per Unit: 16 (for pixel-perfect sprites)

Canvas Scaler ensures consistent sizing across devices.
```

### 5.2 Safe Area Implementation

Create a `SafeAreaPanel` component that adjusts a RectTransform to match `Screen.safeArea`:

```csharp
// Attach to a panel that should respect the safe area.
// This panel becomes the parent for all interactive UI elements.
//
// Usage:
// 1. Create a full-screen Canvas
// 2. Add a child Panel with this component
// 3. All UI elements go inside this panel
//
// The panel's RectTransform anchors will be adjusted each frame
// (or on resolution change) to match Screen.safeArea.

public class SafeAreaPanel : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Rect _lastSafeArea;
    private Vector2Int _lastScreenSize;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update()
    {
        if (Screen.safeArea != _lastSafeArea
            || Screen.width != _lastScreenSize.x
            || Screen.height != _lastScreenSize.y)
        {
            ApplySafeArea();
        }
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;
        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;

        _lastSafeArea = safeArea;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
    }
}
```

### 5.3 Padding Panel

An additional inner panel applies the 20pt padding rule:

```
Inner Padding Panel:
  Anchor: Stretch to parent (SafeAreaPanel)
  Left Padding: 20
  Right Padding: 20
  Top Padding: 20
  Bottom Padding: 20
```

### 5.4 UI Hierarchy

```
Canvas (Screen Space - Overlay)
├── SafeAreaPanel (adjusts to Screen.safeArea)
│   ├── PaddedContentPanel (20pt inner padding)
│   │   ├── HUD_TopBar
│   │   │   ├── HealthBar (anchored top-left)
│   │   │   ├── ScoreDisplay (anchored top-left, offset)
│   │   │   ├── PowerUpSlot (anchored top-right, offset)
│   │   │   └── PauseButton (anchored top-right)
│   │   ├── HUD_BottomControls
│   │   │   ├── DPad (anchored bottom-left)
│   │   │   └── ActionButtons (anchored bottom-right)
│   │   └── HUD_Center
│   │       └── ComboCounter (anchored top-center)
│   │
│   ├── OverlayPanel (full safe area, for menus)
│   │   ├── PauseMenu (centered content)
│   │   ├── LevelCompleteScreen (centered content)
│   │   └── ConfirmationDialog (centered, modal)
│   │
│   └── MainMenuPanel (full safe area)
│       ├── Logo
│       ├── MenuButtons
│       └── SettingsPanel (slide-in)
│
└── SystemOverlay (loading screens, errors - NOT safe-area constrained)
    ├── LoadingScreen
    └── FatalErrorDialog
```

### 5.5 Orientation Lock Code

```csharp
// In a startup script or GameManager.Awake():
Screen.orientation = ScreenOrientation.LandscapeLeft;
Screen.autorotateToPortrait = false;
Screen.autorotateToPortraitUpsideDown = false;
Screen.autorotateToLandscapeLeft = true;
Screen.autorotateToLandscapeRight = true;
Screen.orientation = ScreenOrientation.AutoRotation;
```

Also set in `Info.plist` via Unity Player Settings:
```
UISupportedInterfaceOrientations:
  - UIInterfaceOrientationLandscapeLeft
  - UIInterfaceOrientationLandscapeRight
```

---

## 6. Testing Checklist

### 6.1 Device Testing Matrix

| Test | iPhone 11 | iPhone 14 Pro | iPhone 15 Pro Max |
|------|-----------|---------------|-------------------|
| Safe area insets applied correctly | [ ] | [ ] | [ ] |
| No UI behind notch / Dynamic Island | [ ] | [ ] | [ ] |
| 20pt padding from safe area edges | [ ] | [ ] | [ ] |
| All touch targets >= 44x44 pt | [ ] | [ ] | [ ] |
| Text readable (min 11pt rendered) | [ ] | [ ] | [ ] |
| Orientation lock works (landscape only) | [ ] | [ ] | [ ] |
| Auto-rotate between landscape L/R | [ ] | [ ] | [ ] |
| No clipping on any screen | [ ] | [ ] | [ ] |
| Buttons do not overlap | [ ] | [ ] | [ ] |
| Scroll works in settings | [ ] | [ ] | [ ] |

### 6.2 Screen-by-Screen Audit

| Screen | Elements Visible | Touch Targets OK | Transitions Smooth | Accessible |
|--------|-----------------|-------------------|---------------------|------------|
| Main Menu | [ ] | [ ] | [ ] | [ ] |
| HUD | [ ] | [ ] | N/A | [ ] |
| Pause Menu | [ ] | [ ] | [ ] | [ ] |
| Level Complete | [ ] | [ ] | [ ] | [ ] |
| Settings | [ ] | [ ] | [ ] | [ ] |
| Level ID Entry | [ ] | [ ] | [ ] | [ ] |

### 6.3 Edge Cases

- [ ] Rapid orientation changes do not break layout
- [ ] UI survives app background/foreground cycle
- [ ] Dynamic Type changes respected on return from Settings app
- [ ] Accessibility Inspector reports no warnings
- [ ] All buttons reachable with two-thumb landscape grip
- [ ] No interactive element within 20pt of physical screen edge (after safe area)

---

**Cross-References**:
- Touch control positions: See `control_layout_spec.md` (Assessment 5.2)
- Accessibility modes: See `accessibility_spec.md` (Assessment 5.3)
- Onboarding overlays: See `onboarding_flow.md` (Assessment 5.4)
- State preservation: See `interruption_handling_spec.md` (Assessment 5.5)

---

Last Updated: 2026-02-04
Status: Active
Version: 1.0
