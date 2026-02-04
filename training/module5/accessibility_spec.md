# Assessment 5.3 - Accessibility Specification (WCAG 2.1 AA)

**Module**: 5 - Mobile UX & Accessibility
**Version**: 1.0
**Last Updated**: 2026-02-04
**Status**: Active
**Engine**: Unity 2022 LTS
**Orientation**: Landscape only (locked)
**Minimum Device**: iPhone 11
**Compliance Target**: WCAG 2.1 Level AA + Apple Accessibility Guidelines

---

## Table of Contents

1. [Color-Blind Modes](#1-color-blind-modes)
2. [High Contrast Mode](#2-high-contrast-mode)
3. [Dynamic Type Support](#3-dynamic-type-support)
4. [Reduced Motion Mode](#4-reduced-motion-mode)
5. [Motor Accessibility](#5-motor-accessibility)
6. [Auditory Accessibility](#6-auditory-accessibility)
7. [Cognitive Accessibility (Difficulty Assists)](#7-cognitive-accessibility--difficulty-assists)
8. [VoiceOver Support](#8-voiceover-support)
9. [Switch Control Compatibility](#9-switch-control-compatibility)
10. [Settings UI Specification](#10-settings-ui-specification)
11. [WCAG 2.1 AA Audit Checklist](#11-wcag-21-aa-audit-checklist)
12. [Testing Protocol](#12-testing-protocol)

---

## 1. Color-Blind Modes

### 1.1 Design Principle

No gameplay-critical information is conveyed by color alone. Every color-coded element also has a shape, pattern, icon, or label differentiator. Color-blind modes provide alternative palettes that remap problematic colors while preserving visual hierarchy.

### 1.2 Default Palette (Normal Vision)

| Element | Default Color | Hex | Usage |
|---------|--------------|-----|-------|
| Player Health (full) | Red | #E04040 | Health hearts, damage indicators |
| Player Health (empty) | Dark Gray | #666666 | Empty heart outlines |
| Enemy Damage Flash | Red | #FF3333 | Enemy hit confirmation |
| Collectible Coin | Gold | #FFD700 | Coins, score pickups |
| Power-Up (Health) | Green | #44CC44 | Health restoration pickup |
| Power-Up (Speed) | Blue | #4488FF | Speed boost pickup |
| Power-Up (Shield) | Purple | #AA44FF | Temporary invincibility |
| Hazard Warning | Orange | #FF8800 | Spikes, lava, danger zones |
| Checkpoint Flag | Green | #44CC44 | Active checkpoint |
| Safe Zone | Teal | #44CCAA | Rest areas, tutorial zones |
| UI Accent | Gold | #FFD700 | Buttons, highlights |
| UI Error | Red | #FF4444 | Invalid input, warnings |
| UI Success | Green | #44FF44 | Confirmations, completions |

### 1.3 Protanopia Palette (Red-Blind, ~1% of males)

Reds and greens are indistinguishable. Remap reds to blues/oranges and greens to yellows.

| Element | Default | Protanopia Remap | Hex | Secondary Indicator |
|---------|---------|------------------|-----|---------------------|
| Player Health (full) | Red #E04040 | Bright Blue | #4488DD | Heart shape (unchanged) |
| Player Health (empty) | Gray #666666 | Gray | #666666 | X-mark through heart |
| Enemy Damage Flash | Red #FF3333 | White Flash | #FFFFFF | Star-burst particle overlay |
| Power-Up (Health) | Green #44CC44 | Yellow | #DDCC44 | "+" icon inside pickup |
| Power-Up (Speed) | Blue #4488FF | Blue | #4488FF | Arrow icon (unchanged) |
| Power-Up (Shield) | Purple #AA44FF | Purple | #AA44FF | Shield icon (unchanged) |
| Hazard Warning | Orange #FF8800 | Bright Yellow | #FFDD00 | Triangle "!" overlay |
| Checkpoint Flag | Green #44CC44 | Yellow | #DDCC44 | Flag waves animation |
| UI Error | Red #FF4444 | Orange | #FF8800 | "!" icon prefix |
| UI Success | Green #44FF44 | Cyan | #44FFDD | Checkmark icon prefix |

### 1.4 Deuteranopia Palette (Green-Blind, ~6% of males)

Greens are indistinguishable from reds. Similar remapping strategy to protanopia with slightly different blue shifts.

| Element | Default | Deuteranopia Remap | Hex | Secondary Indicator |
|---------|---------|-------------------|-----|---------------------|
| Player Health (full) | Red #E04040 | Bright Orange | #FF8844 | Heart shape (unchanged) |
| Player Health (empty) | Gray #666666 | Gray | #666666 | X-mark through heart |
| Enemy Damage Flash | Red #FF3333 | Yellow Flash | #FFFF44 | Star-burst particle overlay |
| Power-Up (Health) | Green #44CC44 | Bright Blue | #44AAFF | "+" icon inside pickup |
| Power-Up (Speed) | Blue #4488FF | Blue | #4488FF | Arrow icon (unchanged) |
| Power-Up (Shield) | Purple #AA44FF | Purple | #AA44FF | Shield icon (unchanged) |
| Hazard Warning | Orange #FF8800 | Bright Yellow | #FFEE00 | Triangle "!" overlay |
| Checkpoint Flag | Green #44CC44 | Bright Blue | #44AAFF | Flag waves animation |
| UI Error | Red #FF4444 | Bright Orange | #FF8844 | "!" icon prefix |
| UI Success | Green #44FF44 | Bright Blue | #44AAFF | Checkmark icon prefix |

### 1.5 Tritanopia Palette (Blue-Blind, ~0.01% of population)

Blues and yellows are indistinguishable. Remap blues to reds/pinks and yellows to light grays.

| Element | Default | Tritanopia Remap | Hex | Secondary Indicator |
|---------|---------|-----------------|-----|---------------------|
| Player Health (full) | Red #E04040 | Red | #E04040 | Heart shape (unchanged) |
| Player Health (empty) | Gray #666666 | Gray | #666666 | X-mark through heart |
| Collectible Coin | Gold #FFD700 | Light Pink | #FFAAAA | Sparkle animation |
| Power-Up (Health) | Green #44CC44 | Green | #44CC44 | "+" icon inside pickup |
| Power-Up (Speed) | Blue #4488FF | Pink | #FF6699 | Arrow icon (unchanged) |
| Power-Up (Shield) | Purple #AA44FF | Dark Red | #CC4444 | Shield icon (unchanged) |
| Hazard Warning | Orange #FF8800 | Red-Orange | #FF4400 | Triangle "!" overlay |
| Checkpoint Flag | Green #44CC44 | Green | #44CC44 | Flag waves animation |
| UI Accent | Gold #FFD700 | Light Pink | #FFB0B0 | Underline decoration |

### 1.6 Implementation

```csharp
// ColorBlindFilter.cs
// Applies a color remapping shader based on the selected color-blind mode.
// Attach to the main camera or a post-processing volume.

public enum ColorBlindMode
{
    Normal,       // No remapping
    Protanopia,   // Red-blind
    Deuteranopia, // Green-blind
    Tritanopia    // Blue-blind
}

public class ColorBlindFilter : MonoBehaviour
{
    [SerializeField] private ColorBlindMode mode = ColorBlindMode.Normal;
    [SerializeField] private Material colorBlindMaterial;

    // Shader uses a 3x3 color transformation matrix per mode.
    // Matrices sourced from Machado et al. (2009) simulation.

    private static readonly Matrix4x4 ProtanopiaMatrix = new Matrix4x4(
        new Vector4(0.152f, 0.115f, -0.004f, 0),
        new Vector4(1.053f, 0.786f, -0.048f, 0),
        new Vector4(-0.205f, 0.099f, 1.052f, 0),
        new Vector4(0, 0, 0, 1)
    );

    // Additional matrices for deuteranopia, tritanopia...

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (mode == ColorBlindMode.Normal)
        {
            Graphics.Blit(src, dest);
            return;
        }

        colorBlindMaterial.SetMatrix("_ColorMatrix", GetMatrixForMode(mode));
        Graphics.Blit(src, dest, colorBlindMaterial);
    }
}
```

**UI elements** also remap via a `ColorPalette` ScriptableObject that swaps the active palette based on the setting. All UI color references go through `ColorPalette.Get(ColorRole role)` rather than using hardcoded colors.

### 1.7 Secondary Indicators (Shape/Pattern)

Every color-dependent gameplay element has a non-color secondary indicator:

| Element | Shape/Pattern Indicator |
|---------|------------------------|
| Health Hearts | Shape is a heart; empty hearts have an "X" through them |
| Power-Up (Health) | "+" cross icon overlaid on pickup sprite |
| Power-Up (Speed) | Right-pointing arrow icon overlaid |
| Power-Up (Shield) | Shield icon overlaid |
| Hazard zones | Animated triangle "!" warning icon; diagonal stripe pattern on hazard tiles |
| Checkpoint | Flag pole with waving flag animation (active) vs. still pole (inactive) |
| Enemy damage | Star-burst particle + screen shake (brief, 2-frame, 2pt amplitude) |
| Collectibles | Sparkle/shine animation (3 frames, loops) |

---

## 2. High Contrast Mode

### 2.1 Contrast Ratios

| Element Pair | Standard Ratio | High Contrast Ratio | WCAG Level |
|-------------|----------------|---------------------|------------|
| Primary text on dark BG | 13.5:1 | 17.4:1 | AAA |
| Secondary text on dark BG | 7.2:1 | 14.0:1 | AAA |
| Button text on button BG | 5.8:1 | 12.6:1 | AAA |
| HUD text on game world | 4.5:1 (with outline) | 7.1:1 (with solid BG) | AA / AAA |
| Player sprite on background | 3.2:1 | 7.5:1 | AAA |
| Enemy sprite on background | 3.0:1 | 7.0:1 | AAA |
| Hazard on background | 3.5:1 | 8.2:1 | AAA |
| Interactive vs. non-interactive | 3.0:1 | 5.0:1 | AA |

### 2.2 High Contrast Visual Changes

| Component | Standard Mode | High Contrast Mode |
|-----------|--------------|-------------------|
| Game background | Full-color parallax, 3 layers | Reduced to 1 layer, desaturated 70%, darkened 40% |
| Platform tiles | Textured, colorful | High-contrast outlines (3px white border on dark tile) |
| Player sprite | Normal colors | Bright white outline (2px), increased saturation |
| Enemy sprites | Normal colors | Red/orange outline (2px), increased saturation |
| Hazards | Normal colors | Bright yellow outline (2px), pulsing glow |
| HUD text | White with 1px black outline | White with 3px black outline + semi-transparent black BG pill (#000000 at 70%) |
| Collectibles | Normal colors | Bright white with sparkle, increased size 1.2x |
| HUD backgrounds | Transparent | Semi-transparent dark panels (#000000 at 50%) |
| Button borders | 1px subtle border | 3px white border, filled dark background |

### 2.3 Implementation

High contrast mode applies a combination of:
1. Post-processing shader that adjusts game world contrast and saturation
2. UI theme swap (borders, backgrounds, text outlines become more prominent)
3. Sprite outline shader applied to player, enemies, hazards, collectibles

```csharp
// HighContrastManager.cs
public class HighContrastManager : MonoBehaviour
{
    [SerializeField] private bool highContrastEnabled = false;

    // Shader properties for the outline effect
    [SerializeField] private float outlineWidth = 2f;         // pixels
    [SerializeField] private Color playerOutlineColor = Color.white;
    [SerializeField] private Color enemyOutlineColor = new Color(1f, 0.4f, 0.2f);
    [SerializeField] private Color hazardOutlineColor = Color.yellow;

    // Background desaturation
    [SerializeField] private float bgDesaturation = 0.7f;     // 0 = full color, 1 = grayscale
    [SerializeField] private float bgDarken = 0.4f;           // 0 = normal, 1 = black

    // UI changes
    [SerializeField] private float uiBackgroundOpacity = 0.5f;
    [SerializeField] private float textOutlineWidth = 3f;

    public void SetHighContrast(bool enabled)
    {
        highContrastEnabled = enabled;
        // Apply shader parameters
        // Swap UI theme
        // Persist to PlayerPrefs
    }
}
```

---

## 3. Dynamic Type Support

### 3.1 iOS Integration

The game reads the user's preferred text size from iOS `UIContentSizeCategory` and scales UI text accordingly.

| iOS Content Size | Scale Factor | Body Text Size | Header Text Size |
|-----------------|-------------|----------------|------------------|
| Extra Small | 0.82x | 11pt | 14pt |
| Small | 0.88x | 12pt | 15pt |
| Medium | 0.94x | 13pt | 16pt |
| Large (Default) | 1.00x | 14pt | 17pt |
| Extra Large | 1.12x | 16pt | 19pt |
| Extra Extra Large | 1.24x | 17pt | 21pt |
| Extra Extra Extra Large | 1.35x | 19pt | 23pt |
| Accessibility Medium | 1.50x | 21pt | 26pt |
| Accessibility Large | 1.65x | 23pt | 28pt |
| Accessibility Extra Large | 1.76x | 24pt | 30pt |

### 3.2 Layout Adjustments at Large Sizes

At Accessibility sizes (1.5x and above):
- Settings rows increase height from 52pt to 68pt
- Button text may truncate; use abbreviations or icon-only mode
- Score display switches from 5-digit to 4-digit with "K" suffix at 10,000+
- Level ID characters get slightly more spacing (6pt instead of 4pt)
- Menu buttons increase height from 48pt to 56pt

### 3.3 Manual Override

Players can also set text size independently of the iOS system setting:

| Setting | Options | Notes |
|---------|---------|-------|
| Text Size Source | "System" (default) / "Manual" | "System" reads iOS Dynamic Type |
| Manual Size | Slider: A- to A+ (0.82x to 1.76x in 0.06x steps) | Only visible when "Manual" selected |

### 3.4 Implementation

```csharp
// DynamicTypeManager.cs
public class DynamicTypeManager : MonoBehaviour
{
    public enum TextSizeSource { System, Manual }

    [SerializeField] private TextSizeSource source = TextSizeSource.System;
    [SerializeField] private float manualScale = 1.0f;

    public float CurrentScale { get; private set; } = 1.0f;

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && source == TextSizeSource.System)
        {
            // Re-read iOS content size category
            UpdateFromSystem();
        }
    }

    private void UpdateFromSystem()
    {
        #if UNITY_IOS && !UNITY_EDITOR
        // Read UIContentSizeCategory from iOS
        string category = GetIOSContentSizeCategory(); // native plugin call
        CurrentScale = CategoryToScale(category);
        #else
        CurrentScale = 1.0f;
        #endif

        ApplyToAllText();
    }

    private void ApplyToAllText()
    {
        // Find all TextMeshProUGUI components tagged as "DynamicType"
        // Apply scale factor relative to their base font size
        var texts = FindObjectsOfType<DynamicTypeLabel>();
        foreach (var text in texts)
        {
            text.ApplyScale(CurrentScale);
        }
    }
}
```

All UI text components use a custom `DynamicTypeLabel` wrapper that stores the base font size and applies the scale factor.

---

## 4. Reduced Motion Mode

### 4.1 iOS System Integration

The game reads `UIAccessibility.isReduceMotionEnabled` on launch and when returning from background. Players can also override manually.

| Setting | Options | Default |
|---------|---------|---------|
| Reduced Motion | "Follow System" / "On" / "Off" | "Follow System" |

### 4.2 Motion Reduction Matrix

| Visual Effect | Normal Mode | Reduced Motion Mode |
|--------------|-------------|---------------------|
| Screen shake (damage) | 3-frame shake, 4pt amplitude, 0.2s | DISABLED - replaced with screen edge red flash (0.15s) |
| Screen shake (explosion) | 5-frame shake, 8pt amplitude, 0.4s | DISABLED - replaced with screen edge orange flash (0.2s) |
| Parallax scrolling (3 layers) | Full parallax at 3 different speeds | All layers scroll at same speed (static relative to each other) |
| Enemy death animation | Burst into particles (16 particles, 0.5s) | Simple 3-frame fade-out (0.3s) |
| Power-up collect animation | Spiral particles inward, 0.4s | Sprite scales down to 0, 0.2s |
| Level transition | Wipe effect with particle trail, 0.8s | Simple crossfade, 0.5s |
| Menu transitions | Slide in/out with bounce easing | Instant or simple crossfade, 0.2s |
| Button press feedback | Scale up 1.1x, bounce back | Highlight border, no scale |
| Combo counter animation | Numbers fly up with trail | Numbers appear in place, no motion |
| Background environmental effects | Animated clouds, birds, falling leaves | Static or very slow (10% speed) |
| Star rating (level complete) | Stars fly in from sides with sparkle | Stars fade in sequentially, no motion |
| Health pickup effect | Hearts float up with spiral | Hearts appear in HUD directly |
| Checkpoint activation | Flag raises with wave animation | Flag simply appears raised |
| Score pop animation | Scale to 1.2x and back, 0.15s | No animation, number updates in place |

### 4.3 Implementation

```csharp
// ReducedMotionManager.cs
public class ReducedMotionManager : MonoBehaviour
{
    public enum MotionSetting { FollowSystem, ForceOn, ForceOff }

    [SerializeField] private MotionSetting setting = MotionSetting.FollowSystem;

    public bool IsReducedMotion
    {
        get
        {
            switch (setting)
            {
                case MotionSetting.ForceOn: return true;
                case MotionSetting.ForceOff: return false;
                case MotionSetting.FollowSystem:
                default:
                    #if UNITY_IOS && !UNITY_EDITOR
                    return UnityEngine.iOS.Device.IsReduceMotionEnabled();
                    #else
                    return false;
                    #endif
            }
        }
    }

    // Call this before playing any animation
    public void PlayEffect(MotionEffect normalEffect, MotionEffect reducedEffect)
    {
        if (IsReducedMotion)
            reducedEffect.Play();
        else
            normalEffect.Play();
    }
}
```

All animation and particle systems check `ReducedMotionManager.IsReducedMotion` before playing. Systems use a strategy pattern: each effect has a normal variant and a reduced variant.

---

## 5. Motor Accessibility

### 5.1 One-Handed Play Mode

Enables gameplay with a single thumb. The game automatically simplifies controls and may adjust difficulty.

| Setting | Options | Default |
|---------|---------|---------|
| One-Handed Mode | "Off" / "Left Hand" / "Right Hand" | "Off" |

**Left Hand Mode Layout** (all controls on left 40% of screen):

| Element | Position (% from left of safe area) | Size (pt) | Action |
|---------|--------------------------------------|-----------|--------|
| Movement Zone | 0% - 25% | Full left quarter | Swipe/joystick for movement |
| Jump Button | 28% | 64 x 64 | Tap to jump |
| Attack Button | 36% | 64 x 64 | Tap to attack |
| Pause | 2%, top | 48 x 48 | Tap to pause |

**Right Hand Mode Layout** (mirrored):

| Element | Position (% from right of safe area) | Size (pt) | Action |
|---------|--------------------------------------|-----------|--------|
| Movement Zone | 0% - 25% | Full right quarter | Swipe/joystick for movement |
| Jump Button | 28% | 64 x 64 | Tap to jump |
| Attack Button | 36% | 64 x 64 | Tap to attack |
| Pause | 2%, top | 48 x 48 | Tap to pause |

**Gameplay Adjustments in One-Handed Mode**:
- Auto-run is enabled by default (player moves forward automatically; tap movement zone to stop/reverse)
- Coyote time extended from 6 frames to 12 frames (more forgiving jumps)
- Attack has a wider hitbox (1.5x normal width)
- Enemy attack cooldowns increased by 30%
- These adjustments are separate from Difficulty Assist and stack with them

### 5.2 Adjustable Control Sensitivity

| Control | Parameter | Range | Default | Step |
|---------|-----------|-------|---------|------|
| Joystick dead zone | Radius before input registers | 4pt - 20pt | 8pt | 2pt |
| Joystick sensitivity | How quickly input reaches max | 0.5x - 2.0x | 1.0x | 0.1x |
| Swipe threshold | Minimum swipe distance to register | 10pt - 40pt | 20pt | 5pt |
| Button hold duration | Time before hold action triggers | 0.1s - 0.8s | 0.3s | 0.05s |
| Double-tap window | Max time between taps for double-tap | 0.15s - 0.6s | 0.3s | 0.05s |

**Access**: Settings > Controls > Sensitivity (sub-menu with individual sliders)

### 5.3 Auto-Run Option

| Setting | Behavior |
|---------|----------|
| Off (default) | Player must actively hold movement input |
| On | Player automatically moves right; tap left side to stop/reverse; release to resume auto-run |

Auto-run is independent of one-handed mode and can be enabled in standard two-handed play.

### 5.4 Hold vs. Toggle Configuration

| Action | Default | Toggle Mode |
|--------|---------|-------------|
| Crouch/Duck | Hold down to crouch, release to stand | Tap down to crouch, tap down again to stand |
| Charge Attack | Hold attack to charge, release to fire | Tap attack to start charge, tap again to fire |
| Sprint (if implemented) | Hold to sprint | Tap to toggle sprint on/off |

Setting: Settings > Controls > Button Mode: "Hold" (default) / "Toggle"

---

## 6. Auditory Accessibility

### 6.1 Visual Indicators for Audio Cues

Every sound effect that conveys gameplay information has a visual equivalent:

| Audio Cue | Sound Description | Visual Indicator | Position | Duration |
|-----------|-------------------|-----------------|----------|----------|
| Enemy attack wind-up | Woosh/charging sound | Exclamation mark "!" icon above enemy head + red directional arrow at screen edge pointing toward enemy | Above enemy sprite | 0.5s before attack |
| Off-screen enemy approaching | Footsteps/growl | Yellow arrow at screen edge pointing toward approaching enemy, pulses 3x | Screen edge, direction of enemy | Until enemy on-screen |
| Hazard proximity | Increasing pitch buzz | Pulsing orange glow on nearest hazard + warning triangle icon | On hazard tile | Continuous when within 3 tiles |
| Collectible nearby | Sparkle/chime | Sparkle particle effect on collectible, brighter as player approaches | On collectible sprite | Continuous |
| Low health warning | Heartbeat sound | Screen edges pulse red, health hearts flash 2x/sec | Screen border, HUD | Continuous below 2 hearts |
| Checkpoint reached | Chime/bell | Flag raises + circular wave pulse from checkpoint + brief "Checkpoint!" text | At checkpoint position | 1.5s |
| Power-up available | Humming/glow sound | Power-up sprite bobs up/down 4pt, bright glow outline | On power-up sprite | Continuous |
| Boss entry | Dramatic music sting | Screen border flashes, "WARNING" text slides across screen, ground shakes | Full screen | 2.0s |
| Level timer warning | Ticking clock | Timer digits turn red, flash 1x/sec; clock icon appears | HUD timer position | Continuous in last 30s |
| Death/game over | Sad descending tone | Screen desaturates, shatters outward from player position | Full screen | 1.0s |

### 6.2 Subtitle/Caption System

For any narrative text, dialog, or story elements:

| Setting | Options | Default |
|---------|---------|---------|
| Subtitles | On / Off | On |
| Caption Background | Transparent / Semi-transparent / Opaque | Semi-transparent |
| Caption Text Size | Small (12pt) / Medium (14pt) / Large (18pt) | Medium |
| Caption Position | Bottom / Top | Bottom |
| Speaker Labels | On / Off | On |

**Caption Format**: `[Speaker Name]: Dialog text here`
- Background: Dark panel (#000000 at 70% when semi-transparent)
- Max width: 80% of safe area width
- Text wrap at max width
- Auto-dismiss after audio ends + 2 seconds (or next caption)

### 6.3 Volume Controls

| Channel | Range | Default | Independent |
|---------|-------|---------|-------------|
| Master | 0% - 100% | 100% | Controls all audio |
| Music | 0% - 100% | 80% | Background music |
| Sound Effects | 0% - 100% | 100% | Gameplay SFX |
| UI Sounds | 0% - 100% | 75% | Button taps, menu transitions |

Each channel has its own slider in Settings > Audio.
Muting master also mutes all channels.
The audio toggle on the main menu controls the master volume (mute/unmute).

### 6.4 Visual Rhythm Indicators

For any timing-based challenges (if implemented):
- Visual metronome: pulsing circle that expands/contracts in rhythm
- Beat indicator: series of dots that light up in sequence
- These replace or supplement audio rhythm cues

---

## 7. Cognitive Accessibility / Difficulty Assists

### 7.1 Difficulty Assist Options

Accessible via Settings > Accessibility > Difficulty Assist. When enabled, a sub-menu appears with individual toggles.

**No content is gated behind difficulty.** All levels, all enemies, all features are accessible with any combination of assists enabled. Assists do not affect scoring or star ratings (scores are only compared within the same assist configuration).

| Assist Option | Effect | Default |
|--------------|--------|---------|
| Extra Health | Player starts with 8 hearts instead of 5 | Off |
| Damage Reduction | Player takes 50% damage from all sources | Off |
| Slower Enemies | Enemy movement speed reduced by 30% | Off |
| Enemy Attack Cooldown | Enemy attack frequency reduced by 40% | Off |
| Extended Coyote Time | Coyote time increased from 6 frames to 15 frames (0.25s at 60fps) | Off |
| Extended Jump Buffer | Jump buffer increased from 4 frames to 12 frames (0.2s at 60fps) | Off |
| Forgiving Hitboxes | Player hitbox shrunk by 25%, enemy hitboxes enlarged by 15% | Off |
| Infinite Lives | Player respawns at last checkpoint without limit (no game over) | Off |
| Guide Arrows | Subtle arrows hint at the correct path forward | Off |
| Slow Motion | Game runs at 75% speed | Off |

### 7.2 Assist Presets

For players who do not want to configure individually:

| Preset | Assists Enabled | Recommended For |
|--------|----------------|-----------------|
| Standard | None | Default experience |
| Relaxed | Extra Health, Slower Enemies, Extended Coyote Time, Extended Jump Buffer | Casual players, young children |
| Assisted | All of Relaxed + Damage Reduction, Forgiving Hitboxes, Infinite Lives | Players with motor challenges |
| Full Assist | All options enabled | Maximum accessibility |
| Custom | Player selects individually | Experienced players who want specific assists |

### 7.3 Assist Indicator

When any assist is active, a small icon appears in the top-center of the HUD:
- Icon: Shield with "A" inside, 20 x 20 pt, 40% opacity
- Tapping it shows a tooltip listing active assists
- This icon is for the player's information only, NOT a stigma indicator
- It can be hidden: Settings > Accessibility > Show Assist Icon: On / Off

### 7.4 Pause-Accessible Reference

Pause menu includes "Controls" option that shows:
- Current control scheme diagram
- All available actions and their inputs
- Move list (if combo attacks exist)
- Active assist options

---

## 8. VoiceOver Support

### 8.1 Menu VoiceOver

All menu screens are fully navigable with iOS VoiceOver enabled. Every interactive and informational element has an accessibility label, trait, and hint.

| Screen | Element | Accessibility Label | Trait | Hint |
|--------|---------|---------------------|-------|------|
| Main Menu | Play Button | "Play" | Button | "Double-tap to start the game" |
| Main Menu | Level ID Button | "Level ID" | Button | "Double-tap to enter or share a level ID" |
| Main Menu | Settings Button | "Settings" | Button | "Double-tap to open settings" |
| Main Menu | Audio Toggle | "Sound, currently on" / "Sound, currently muted" | Button | "Double-tap to toggle sound" |
| Main Menu | Version Label | "Version 1.0.0" | Static Text | (none) |
| HUD | Health Bar | "Health: 3 of 5 hearts" | Static Text | (updates dynamically) |
| HUD | Score | "Score: 12,450" | Static Text | (updates dynamically) |
| HUD | Pause Button | "Pause" | Button | "Double-tap to pause the game" |
| Pause | Resume | "Resume" | Button | "Double-tap to return to game" |
| Pause | Restart | "Restart level" | Button | "Double-tap to restart from beginning" |
| Pause | Settings | "Settings" | Button | "Double-tap to open settings" |
| Pause | Quit | "Quit to main menu" | Button | "Double-tap to quit. Progress saved at last checkpoint." |
| Settings | Each toggle | "[Name], currently [on/off]" | Button | "Double-tap to toggle" |
| Settings | Each slider | "[Name], [value]%" | Adjustable | "Swipe up or down to adjust" |
| Level Complete | Star 1 | "Star 1, earned" / "Star 1, not earned" | Image | (none) |
| Level Complete | Score | "Score: 12,450" | Static Text | (none) |
| Level Complete | Next Level | "Next level" | Button | "Double-tap to continue" |
| Level ID | Text Field | "Level ID entry, [current value]" | Text Field | "Double-tap to edit" |
| Level ID | Play | "Play level" | Button | "Double-tap to load this level" |

### 8.2 In-Game VoiceOver Behavior

During active gameplay, VoiceOver is **paused** to avoid interfering with real-time action. When the game is paused, VoiceOver resumes and navigates the pause menu.

The following are announced during gameplay via VoiceOver even when gameplay is active (as brief interruptions):
- "Checkpoint reached" (when checkpoint is activated)
- "Health low, 1 heart remaining" (when health drops to 1)
- "Power-up collected: [type]" (when a power-up is picked up)
- "Level complete" (when the level ends)

These announcements use `UIAccessibility.post(.announcement, ...)` and do not pause the game.

### 8.3 Implementation

```csharp
// AccessibilityLabel.cs
// Attach to any UI element that should be accessible via VoiceOver.

public class AccessibilityLabel : MonoBehaviour
{
    [SerializeField] private string label;
    [SerializeField] private string hint;
    [SerializeField] private bool isButton = false;
    [SerializeField] private bool isAdjustable = false;

    // Dynamic label support
    public System.Func<string> DynamicLabelProvider { get; set; }

    void Start()
    {
        var element = GetComponent<UnityEngine.UI.Selectable>();
        if (element != null)
        {
            // Register with iOS accessibility system
            SetAccessibilityProperties();
        }
    }

    public void SetAccessibilityProperties()
    {
        #if UNITY_IOS && !UNITY_EDITOR
        string currentLabel = DynamicLabelProvider?.Invoke() ?? label;
        // Call native plugin to set accessibility properties
        _SetAccessibilityLabel(gameObject.GetInstanceID(), currentLabel);
        _SetAccessibilityHint(gameObject.GetInstanceID(), hint);
        _SetAccessibilityTraits(gameObject.GetInstanceID(), isButton, isAdjustable);
        #endif
    }
}
```

---

## 9. Switch Control Compatibility

### 9.1 Overview

iOS Switch Control allows users to navigate and interact using external switches, head movements, or screen taps as switches. The game must support this input method for all menu navigation.

### 9.2 Focus Order

All screens define a logical focus order for Switch Control scanning:

**Main Menu Focus Order**:
1. Play Button
2. Level ID Button
3. Settings Button
4. Audio Toggle

**Pause Menu Focus Order**:
1. Resume
2. Restart
3. Settings
4. Quit to Menu

**Settings Focus Order**:
1. Back Button
2. (Each setting in top-to-bottom order within each section)
3. (Section headers are skipped as non-interactive)

**Level Complete Focus Order**:
1. Replay Button
2. Share ID Button
3. Next Level Button

### 9.3 Gameplay with Switch Control

In-game Switch Control support provides an alternative scanning mode:

| Switch Action | Game Action |
|--------------|-------------|
| Primary switch (select) | Attack |
| Secondary switch | Jump |
| Auto-scanning (if enabled) | Auto-run is forced on; direction alternates on primary switch long-press |
| Menu switch | Pause |

**Note**: Full gameplay with Switch Control requires the "Full Assist" difficulty preset to be enjoyable. The game prompts new Switch Control users to enable assists on first launch.

### 9.4 Implementation

```csharp
// SwitchControlManager.cs
public class SwitchControlManager : MonoBehaviour
{
    void Start()
    {
        // Detect if Switch Control is enabled
        #if UNITY_IOS && !UNITY_EDITOR
        bool switchControlActive = UIAccessibility.IsSwitchControlRunning();
        if (switchControlActive)
        {
            EnableSwitchControlMode();
        }
        #endif
    }

    // Listen for Switch Control status changes
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            #if UNITY_IOS && !UNITY_EDITOR
            bool switchControlActive = UIAccessibility.IsSwitchControlRunning();
            if (switchControlActive && !isSwitchControlMode)
            {
                EnableSwitchControlMode();
                // Prompt user to enable difficulty assists
            }
            #endif
        }
    }

    private void EnableSwitchControlMode()
    {
        // Set all UI elements as accessible
        // Define focus groups and order
        // Enable auto-run
        // Map switch inputs to game actions
    }
}
```

---

## 10. Settings UI Specification

### 10.1 Accessibility Settings Screen

Located at Settings > Accessibility. Grouped by category.

```
+------------------------------------------------------------------+
| [<< Back]          ACCESSIBILITY                                   |
|-------------------------------------------------------------------|
|                                                                    |
| VISION                                                             |
|   Color-Blind Mode     [Off | Protanopia | Deuteranopia |         |
|                          Tritanopia]                               |
|   High Contrast        [ON / OFF]                                  |
|   Text Size            [System | Manual]  [A-  ====  A+]          |
|   Reduced Motion       [System | On | Off]                         |
|                                                                    |
|-------------------------------------------------------------------|
| MOTOR                                                              |
|   One-Handed Mode      [Off | Left | Right]                       |
|   Auto-Run             [ON / OFF]                                  |
|   Button Mode          [Hold | Toggle]                             |
|   Control Sensitivity  [CONFIGURE >>]                              |
|                                                                    |
|-------------------------------------------------------------------|
| HEARING                                                            |
|   Visual Indicators    [ON / OFF]                                  |
|   Subtitles            [ON / OFF]                                  |
|   Caption Background   [Transparent | Semi | Opaque]               |
|   Caption Size         [Small | Medium | Large]                    |
|                                                                    |
|-------------------------------------------------------------------|
| GAMEPLAY                                                           |
|   Difficulty Assist    [Off | Relaxed | Assisted |                 |
|                         Full Assist | Custom >>]                   |
|   Show Assist Icon     [ON / OFF]                                  |
|                                                                    |
|-------------------------------------------------------------------|
| SYSTEM                                                             |
|   VoiceOver Labels     [ON / OFF]                                  |
|                                                                    |
+------------------------------------------------------------------+
```

### 10.2 First-Launch Accessibility Prompt

On first launch, before the main menu appears, a brief, non-intrusive prompt appears:

```
+----------------------------------------------+
|                                                |
|  Accessibility Options Available               |
|                                                |
|  This game supports color-blind modes,         |
|  one-handed play, difficulty assists,          |
|  and more.                                     |
|                                                |
|  [CONFIGURE NOW]     [SKIP]                    |
|                                                |
+----------------------------------------------+
```

- "CONFIGURE NOW" opens Settings > Accessibility
- "SKIP" dismisses and proceeds to main menu
- This prompt appears only on first launch, never again
- Access accessibility settings anytime from Settings

---

## 11. WCAG 2.1 AA Audit Checklist

### Principle 1: Perceivable

| Criterion | ID | Requirement | Status | Implementation |
|-----------|-----|------------|--------|----------------|
| Non-text Content | 1.1.1 | All images/icons have text alternatives | [ ] | Accessibility labels on all UI images |
| Audio-only/Video-only | 1.2.1 | Alternatives for time-based media | [ ] | Subtitles for all narrative audio |
| Captions | 1.2.2 | Captions for audio content | [ ] | Caption system with configurable appearance |
| Use of Color | 1.4.1 | Color not sole means of conveying info | [ ] | Shape/pattern secondary indicators for all color-coded elements |
| Contrast (Minimum) | 1.4.3 | 4.5:1 for normal text, 3:1 for large | [ ] | Standard mode meets AA; High contrast meets AAA |
| Resize Text | 1.4.4 | Text resizable to 200% without loss | [ ] | Dynamic Type support up to 1.76x |
| Images of Text | 1.4.5 | Use real text, not images of text | [ ] | All UI text is rendered text, not sprite text (except pixel font which is a real font) |
| Contrast (Enhanced) | 1.4.6 | 7:1 ratio (AAA, optional but met) | [ ] | High contrast mode achieves 7:1+ |
| Text Spacing | 1.4.12 | Adequate text spacing | [ ] | Line height 1.3x-1.4x, letter spacing configurable |

### Principle 2: Operable

| Criterion | ID | Requirement | Status | Implementation |
|-----------|-----|------------|--------|----------------|
| Keyboard Accessible | 2.1.1 | All functionality via keyboard (Switch Control equivalent) | [ ] | Switch Control support for all menus |
| No Keyboard Trap | 2.1.2 | Focus never trapped | [ ] | Back button always available, focus order defined |
| Timing Adjustable | 2.2.1 | Time limits adjustable | [ ] | No time limits in menus; gameplay timers have slow-motion assist |
| Pause, Stop, Hide | 2.2.2 | Moving content pausable | [ ] | Pause button always accessible; reduced motion mode |
| Three Flashes | 2.3.1 | No content flashes > 3 times/sec | [ ] | All flashing effects limited to 2 Hz max |
| Focus Order | 2.4.3 | Logical focus order | [ ] | Defined for all screens (Section 9.2) |
| Link/Button Purpose | 2.4.4 | Purpose clear from text | [ ] | All buttons have descriptive labels |
| Target Size | 2.5.5 | Touch targets >= 44x44pt | [ ] | All interactive elements meet minimum (Section 2.3 of UI Layout Spec) |

### Principle 3: Understandable

| Criterion | ID | Requirement | Status | Implementation |
|-----------|-----|------------|--------|----------------|
| Language | 3.1.1 | Language of page identified | [ ] | App language set in Info.plist |
| On Focus | 3.2.1 | No context change on focus | [ ] | Focus alone never triggers actions |
| On Input | 3.2.2 | No unexpected context change on input | [ ] | All toggles/buttons behave predictably |
| Error Identification | 3.3.1 | Errors identified and described | [ ] | Invalid level ID shows clear error message |
| Labels or Instructions | 3.3.2 | Input fields have labels | [ ] | Level ID field has placeholder and instruction text |
| Consistent Navigation | 3.2.3 | Consistent navigation patterns | [ ] | Back button always top-left, consistent layout |
| Consistent Identification | 3.2.4 | Same function = same label | [ ] | "Play", "Settings", etc. consistent across screens |

### Principle 4: Robust

| Criterion | ID | Requirement | Status | Implementation |
|-----------|-----|------------|--------|----------------|
| Parsing | 4.1.1 | Content parseable by assistive tech | [ ] | Proper UI hierarchy for VoiceOver |
| Name, Role, Value | 4.1.2 | All components have name/role/value | [ ] | AccessibilityLabel component on all elements |
| Status Messages | 4.1.3 | Status messages announced | [ ] | Score changes, checkpoint, health changes announced |

---

## 12. Testing Protocol

### 12.1 Color-Blind Testing

| Test | Tool | Pass Criteria |
|------|------|---------------|
| Protanopia simulation | Color Oracle or Sim Daltonism | All gameplay information distinguishable without color |
| Deuteranopia simulation | Color Oracle or Sim Daltonism | Same as above |
| Tritanopia simulation | Color Oracle or Sim Daltonism | Same as above |
| Play full level in each mode | Device, each mode enabled | Level completable; all power-ups, hazards identifiable |
| Contrast ratios in each mode | Colour Contrast Analyser | All text meets 4.5:1 (AA) in each palette |

### 12.2 Motor Accessibility Testing

| Test | Method | Pass Criteria |
|------|--------|---------------|
| One-handed left mode | Play Level 1-3 with left thumb only | All levels completable |
| One-handed right mode | Play Level 1-3 with right thumb only | All levels completable |
| Switch Control navigation | Enable iOS Switch Control, navigate all menus | All menus fully navigable |
| Switch Control gameplay | Play Level 1 with Switch Control | Level completable with Full Assist |
| Auto-run gameplay | Enable auto-run, play Level 1-5 | All levels completable |
| Maximum sensitivity settings | Set all sensitivity to max, play | Controls responsive but not erratic |
| Minimum sensitivity settings | Set all sensitivity to min, play | Controls still register properly |

### 12.3 VoiceOver Testing

| Test | Method | Pass Criteria |
|------|--------|---------------|
| Main menu navigation | VoiceOver on, navigate main menu | All elements announced correctly |
| Settings navigation | VoiceOver on, navigate all settings | All labels, values, hints read correctly |
| Pause menu | VoiceOver on, pause during gameplay | Resume and all options accessible |
| Level complete | VoiceOver on, complete a level | Score, stars, and buttons announced |
| Dynamic labels | VoiceOver on, take damage | "Health: X of 5 hearts" updates correctly |

### 12.4 Visual Accessibility Testing

| Test | Method | Pass Criteria |
|------|--------|---------------|
| High contrast mode | Enable, play Level 1-3 | Player, enemies, hazards, collectibles all clearly visible |
| Contrast ratios | Colour Contrast Analyser on screenshots | All ratios meet targets from Section 2.1 |
| Dynamic Type - maximum | Set iOS to Accessibility Extra Large, relaunch | All text readable, no clipping, layout stable |
| Dynamic Type - minimum | Set iOS to Extra Small, relaunch | All text >= 11pt rendered |
| Reduced motion | Enable, play Level 1-3 | No screen shake, no complex particles, parallax simplified |
| Flash test | PEAT (Photosensitive Epilepsy Analysis Tool) on gameplay video | No flashing > 3/sec at any luminance |

### 12.5 Xcode Accessibility Inspector Audit

Run the Xcode Accessibility Inspector on every screen and verify:
- [ ] No "Missing accessibility label" warnings
- [ ] No "Touch target too small" warnings
- [ ] Logical focus order matches defined order (Section 9.2)
- [ ] All interactive elements are accessible
- [ ] Contrast warnings resolved

---

**Cross-References**:
- UI element positions and touch targets: See `ui_layout_spec.md` (Assessment 5.1)
- Control layout and haptics: See `control_layout_spec.md` (Assessment 5.2)
- Tutorial accessibility: See `onboarding_flow.md` (Assessment 5.4)
- State preservation for accessibility settings: See `interruption_handling_spec.md` (Assessment 5.5)

---

Last Updated: 2026-02-04
Status: Active
Version: 1.0
