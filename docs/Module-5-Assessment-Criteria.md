# Module 5: Mobile UX & Accessibility - Assessment Criteria

## Module Overview
This module develops expertise in mobile-specific user experience design, touch ergonomics, adaptive UI, and accessibility compliance. Learners must demonstrate ability to create interfaces that work across iPhone screen sizes, accommodate diverse player abilities, and meet Apple's accessibility standards.

---

## Learning Objectives

By completion, learners will be able to:
1. Design adaptive UI layouts that work across all supported iPhone models
2. Apply touch ergonomics principles for comfortable extended play sessions
3. Implement accessibility features meeting WCAG 2.1 Level AA standards
4. Design onboarding flows that teach without patronizing
5. Handle mobile-specific interruptions gracefully (calls, notifications, backgrounding)

---

## Assessment 5.1: Adaptive UI Layout Design

### Objective
Design a complete UI system that adapts to all supported iPhone screen sizes and orientations, respecting safe areas and notch constraints.

### Requirements

**Screen Matrix:**
- [ ] Design UI for each supported device class:
  - iPhone 11 / XR (6.1", 828x1792, notch)
  - iPhone 12/13 (6.1", 1170x2532, notch)
  - iPhone 14 (6.1", 1170x2532, notch)
  - iPhone 14 Pro / 15 Pro (6.1", 1179x2556, Dynamic Island)
  - iPhone 15 Pro Max (6.7", 1290x2796, Dynamic Island)
  - iPhone SE 3rd gen (4.7", 750x1334, no notch) -- if supported
- [ ] Document safe area insets for each device
- [ ] All interactive elements fully within safe area

**UI Components:**
- [ ] Main menu: play, settings, achievements, level sharing
- [ ] HUD (in-game): health bar, score, power-up indicators, pause button
- [ ] Pause menu: resume, restart, settings, quit
- [ ] Level complete screen: score, stars, next level, replay, share ID
- [ ] Settings: controls, audio, accessibility, account
- [ ] Level ID entry/sharing screen

**Design Constraints:**
- [ ] All touch targets minimum 44x44 points (Apple HIG)
- [ ] No interactive elements within 20pt of screen edges
- [ ] Text minimum 11pt (body), 17pt (headers) for readability
- [ ] High contrast mode support (text contrast ratio >= 4.5:1)
- [ ] Landscape orientation only (game locks to landscape)

### Deliverables
```
ui_layout_spec.md                (complete UI specification)
screen_mockups/                  (folder with mockups for each screen)
  main_menu.png
  hud_layout.png
  pause_menu.png
  level_complete.png
  settings.png
  level_id_screen.png
safe_area_matrix.csv             (device -> safe area insets)
touch_target_audit.md            (every interactive element with hit area)
```

### Success Criteria
- UI is fully functional on all supported device classes
- No interactive element overlaps with notch or Dynamic Island
- All touch targets meet 44x44pt minimum
- Text is readable on smallest supported screen
- Layout adapts gracefully (no clipping, overlapping, or misalignment)

### Evaluation Rubric
| Criteria | Excellent (5) | Good (4) | Acceptable (3) | Needs Work (2) |
|----------|--------------|---------|----------------|----------------|
| Device Coverage | All devices tested | Most devices | Key devices only | Gaps |
| Safe Area Compliance | Perfect compliance | Minor issues | Some violations | Significant problems |
| Touch Target Sizing | All >= 44pt | 1-2 undersized | Several undersized | Many undersized |
| Visual Quality | Professional, polished | Clean design | Functional | Rough |
| Documentation | Comprehensive | Clear | Adequate | Incomplete |

**Pass Threshold**: Average >= 4.0/5.0

---

## Assessment 5.2: Touch Ergonomics & Comfort

### Objective
Conduct ergonomic analysis and design touch layouts optimized for comfortable extended play sessions (30+ minutes).

### Requirements

**Ergonomic Analysis:**
- [ ] Document common grip styles for landscape iPhone gaming:
  - Two-thumb grip (most common)
  - Index-finger grip (tablet-style, less common on phones)
  - One-handed (casual, limited control)
- [ ] Map thumb reach zones for each grip style:
  - Easy zone (natural thumb rest position)
  - Stretch zone (reachable with effort)
  - Unreachable zone (requires grip adjustment)
- [ ] Place all high-frequency controls in the easy zone

**Comfort Design:**
- [ ] Button layout minimizes thumb travel for common action sequences
- [ ] No action requires simultaneous button presses from the same thumb
- [ ] Movement controls (left side) and action controls (right side) are balanced
- [ ] Control opacity is adjustable (25%, 50%, 75%, 100%)
- [ ] Control positions are customizable (drag to reposition)
- [ ] Vibration/haptic intensity is adjustable

**Fatigue Prevention:**
- [ ] Analyze repetitive strain potential for core gameplay loop
- [ ] No action requires sustained pressure for > 3 seconds
- [ ] Natural rest points in gameplay (safe zones, cutscenes)
- [ ] Session length reminder option (30 min, 60 min)

### Deliverables
```
ergonomic_analysis.md            (grip styles, reach zones, analysis)
thumb_reach_diagrams.png         (visual reach maps per device)
control_layout_spec.md           (button positions with ergonomic justification)
comfort_features.md              (adjustability, fatigue prevention)
```

### Success Criteria
- All high-frequency actions are within easy thumb reach
- Control layout is comfortable for 30+ minute sessions
- Customization options cover position, size, and opacity
- No repetitive strain risks in core gameplay loop
- Analysis references real ergonomic data or standards

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Ergonomic Analysis Depth | /25 | Grips, reach zones, data |
| Control Layout Quality | /30 | Comfortable, intuitive placement |
| Customization Options | /20 | Position, size, opacity adjustable |
| Fatigue Prevention | /15 | Strain analysis, rest points |
| Documentation | /10 | Clear, referenced |

**Pass Threshold**: >= 85/100

---

## Assessment 5.3: Accessibility Compliance (WCAG 2.1 AA)

### Objective
Design and document accessibility features that meet WCAG 2.1 Level AA standards and Apple's accessibility guidelines.

### Requirements

**Visual Accessibility:**
- [ ] Color-blind safe design:
  - No gameplay information conveyed by color alone
  - Color-blind mode with alternative palettes (protanopia, deuteranopia, tritanopia)
  - Test all UI with color-blind simulation tools
- [ ] High contrast mode:
  - Alternative palette with increased contrast ratios
  - UI elements clearly distinguishable from game elements
  - Text contrast ratio >= 7:1 in high-contrast mode
- [ ] Text sizing:
  - Support Dynamic Type (iOS system text size setting)
  - Minimum body text: 11pt (default), scalable to 24pt
  - All text remains legible and layout stable at large sizes
- [ ] Reduced motion mode:
  - Disable screen shake
  - Reduce particle effects
  - Simplify parallax scrolling
  - Respect iOS "Reduce Motion" system setting

**Motor Accessibility:**
- [ ] One-handed play mode (simplified controls)
- [ ] Adjustable control sensitivity
- [ ] Auto-run option (reduce input frequency)
- [ ] Switch Control compatibility (iOS)
- [ ] Configurable button hold vs. toggle

**Auditory Accessibility:**
- [ ] Visual indicators for all audio cues (enemy attacks, hazards, rewards)
- [ ] Subtitle/caption system for narrative elements
- [ ] Separate volume controls: music, SFX, voice (if any)
- [ ] Visual metronome or rhythm indicator for timing-based challenges

**Cognitive Accessibility:**
- [ ] Difficulty assist options (more health, slower enemies, extended coyote time)
- [ ] Tutorial replay option (revisit tutorial at any time)
- [ ] Pause-accessible move list / control reference
- [ ] Clear, consistent iconography across all UI
- [ ] No time-pressure in menus or UI (only in gameplay, with assist options)

### Deliverables
```
accessibility_spec.md            (complete specification)
color_blind_palettes.png         (alternative palettes for 3 types)
wcag_audit_checklist.md          (item-by-item WCAG 2.1 AA compliance)
accessibility_settings_mockup.png (settings screen layout)
testing_protocol.md              (how to verify each feature)
```

### Success Criteria
- WCAG 2.1 Level AA audit passes on all applicable criteria
- Color-blind mode tested with simulation tools for all 3 types
- Game is playable with assist options enabled (no content gated behind difficulty)
- iOS accessibility features (VoiceOver, Switch Control, Reduce Motion) respected
- All audio cues have visual equivalents

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Visual Accessibility | /25 | Color-blind, contrast, text, motion |
| Motor Accessibility | /25 | One-handed, sensitivity, switch |
| Auditory Accessibility | /20 | Visual indicators, captions |
| Cognitive Accessibility | /15 | Assists, tutorials, clarity |
| Documentation & Testing | /15 | Audit checklist, verification |

**Pass Threshold**: >= 85/100

---

## Assessment 5.4: Onboarding & Tutorial Design

### Objective
Design an onboarding flow that teaches all core mechanics through gameplay, without text-heavy tutorials or forced interruptions.

### Requirements

**Design Philosophy:**
- "Show, don't tell" -- teach through constrained environments, not text popups
- Player should feel clever for learning, not lectured at
- Every tutorial moment should be skippable for experienced players

**Onboarding Sequence:**
- [ ] First launch: minimal UI, immediate gameplay (< 5 seconds to first input)
- [ ] Level 1: Teach movement only (no enemies, safe environment)
  - Left/right movement in constrained corridor
  - Jump required to progress (gap that can only be crossed by jumping)
  - No death possible in first 30 seconds
- [ ] Level 2: Teach combat (single weak enemy, can't avoid)
  - Enemy blocks path forward
  - Visual highlight on attack button (first time only)
  - Enemy has slow attack, player has time to react
- [ ] Level 3: Teach combinations (movement + combat)
  - Enemies on platforms requiring jump-and-attack
  - First checkpoint introduction
  - First power-up introduction (health, placed before challenge)
- [ ] Post-tutorial: Fade out hints, increase challenge

**Returning Player Handling:**
- [ ] Skip tutorial option on subsequent playthroughs
- [ ] Detect if player already knows mechanics (fast completion = skip hints)
- [ ] Settings option to replay tutorial

**Controls Introduction:**
- [ ] First time: ghost buttons appear with labels ("JUMP", "ATTACK")
- [ ] After 3 uses: labels fade, button outlines remain
- [ ] After Level 3: fully transparent controls (player knows layout)
- [ ] Settings: always show labels option for accessibility

### Deliverables
```
onboarding_flow.md               (complete sequence specification)
tutorial_level_specs.md          (level 1-3 detailed layouts)
hint_system_spec.md              (when hints appear, fade, disappear)
returning_player_handling.md     (skip detection, replays)
```

### Success Criteria
- Player can start playing within 5 seconds of launch
- All core mechanics taught within first 3 levels
- No forced text popups or unskippable cutscenes
- Tutorial is invisible to experienced players on replay
- Hints fade naturally as player demonstrates competence

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Flow Design | /30 | Natural, non-intrusive teaching |
| Mechanic Coverage | /25 | All core mechanics introduced |
| Skip/Replay Handling | /20 | Respects experienced players |
| Hint System Design | /15 | Fades appropriately |
| Documentation | /10 | Complete level specs |

**Pass Threshold**: >= 85/100

---

## Assessment 5.5: Interruption & State Management

### Objective
Design robust handling of all mobile-specific interruptions and state transitions.

### Requirements

**Interruption Handling:**
- [ ] Phone call: auto-pause, resume on return, no progress loss
- [ ] Notification banner: game continues (banner doesn't obscure critical UI)
- [ ] App backgrounding: auto-pause, save state, resume on foreground
- [ ] App termination: save progress at last checkpoint, restore on relaunch
- [ ] Low battery warning: no interference with gameplay
- [ ] Control Center swipe: handled without accidental input

**State Persistence:**
- [ ] Auto-save at every checkpoint
- [ ] Save on pause, background, and app termination
- [ ] Resume from exact game state (position, health, score, enemies)
- [ ] Handle save corruption gracefully (fall back to last checkpoint)
- [ ] iCloud sync conflict resolution (newest save wins, with prompt)

**Session Lifecycle:**
- [ ] Cold launch → main menu (< 3 seconds)
- [ ] Warm resume → game state restored (< 1 second)
- [ ] Level transition → loading (< 2 seconds)
- [ ] All transitions have visual feedback (loading indicator or animation)

### Deliverables
```
interruption_handling_spec.md    (every interruption type and response)
state_persistence_spec.md       (save/load system specification)
session_lifecycle.md             (launch, resume, transition flows)
edge_cases.md                    (corruption handling, sync conflicts)
```

### Success Criteria
- No progress loss from any interruption type
- Resume from background restores exact game state
- Save corruption is handled without crashing
- iCloud sync conflicts are resolved without data loss
- All transitions complete within specified time targets

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Interruption Coverage | /25 | All types handled |
| State Persistence | /25 | Reliable save/restore |
| Session Lifecycle | /20 | Fast transitions |
| Edge Case Handling | /20 | Corruption, conflicts |
| Documentation | /10 | Complete specifications |

**Pass Threshold**: >= 85/100

---

## Module 5 Final Assessment: Capstone Project

### Objective
Produce a complete Mobile UX specification document that integrates adaptive UI, touch ergonomics, accessibility, onboarding, and interruption handling.

### Requirements
- [ ] All five assessment areas integrated into one cohesive UX document
- [ ] UI mockups for every screen on at least 3 device sizes
- [ ] Accessibility audit checklist fully completed
- [ ] Onboarding flow tested against reference games
- [ ] Interruption handling covers all iOS-specific scenarios

### Evaluation Process
1. Self-assessment by learner
2. Accessibility audit by independent reviewer
3. UX review against Apple Human Interface Guidelines
4. Design lead approval

**Pass Threshold**: Approved by design lead + accessibility auditor

---

## Module Completion Checklist

- [ ] Assessment 5.1: Adaptive UI Layout Design (PASS)
- [ ] Assessment 5.2: Touch Ergonomics & Comfort (PASS)
- [ ] Assessment 5.3: Accessibility Compliance (PASS)
- [ ] Assessment 5.4: Onboarding & Tutorial Design (PASS)
- [ ] Assessment 5.5: Interruption & State Management (PASS)
- [ ] Capstone Project (APPROVED)
- [ ] Module 5 Knowledge Check (80%+ score)

**Module Status**: Ready to advance to Module 6

---

## Resources & References

### Essential Readings
- Apple Human Interface Guidelines (iOS): https://developer.apple.com/design/human-interface-guidelines/
- WCAG 2.1 Guidelines: https://www.w3.org/WAI/WCAG21/quickref/
- "Designing for Touch" - Josh Clark
- "Inclusive Design Patterns" - Heydon Pickering

### Reference Games (UX Study)
- Celeste (accessibility options as industry gold standard)
- The Last of Us Part II (comprehensive accessibility suite)
- Alto's Odyssey (elegant mobile UX, minimal UI)
- Monument Valley (touch interaction design)

### Tools
- Xcode Accessibility Inspector
- Colour Contrast Analyser (CCA)
- Color Oracle (color-blind simulation)
- Figma / Sketch (UI mockups)
- iOS Simulator (multi-device testing)
