# HUD Design and QC

**Date:** 2026-02-09
**Scope:** HUD layout history, Option C plan, and QA checklist. Includes CSS prototype snippets for quick layout visualization.

---

## 1) History and Current State

**Observed problem (playtest + screenshot review):**
- The HUD is visually crowded and hard to scan. Key information competes for the same horizontal space, and combat vs progression data are mixed in one strip.

**Current implementation (code reality):**
- HUD is built programmatically in CreateUI in [EpochBreaker/Assets/Scripts/UI/GameplayHUD.cs](EpochBreaker/Assets/Scripts/UI/GameplayHUD.cs#L240-L520).
- Layout is a **single bottom-left strip** that extends to 50% screen width, 125px tall, with three rows:
  - Row 1: difficulty badge, hearts, score, timer, era + level code (right-aligned to strip edge)
  - Row 2: ability icons, relic counter (right-aligned), lives/deaths, collected items
  - Row 3: special bar, heat bar, auto-select reason
- Weapon dock (active weapon + slots + label) is anchored into the left of the strip in [EpochBreaker/Assets/Scripts/UI/GameplayHUD.cs](EpochBreaker/Assets/Scripts/UI/GameplayHUD.cs#L930-L1020).
- Transient overlays (Quick Draw, DPS cap, boss HP bar, achievement toast, era intro, mode info) are center or top-center anchored. They do not depend on the strip.

**Primary issue:**
- The single-strip layout mixes combat-critical and progression info, causing crowded alignment and weak visual hierarchy.

**Current screenshot issues (Option C in game):**
- **Top-left panel** is too tall and text stacks unevenly (era, relics, difficulty) which pushes content down and creates inconsistent left-edge alignment.
- **Top-center score panel** is not centered within the safe area and sits too low relative to the pause button.
- **Bottom combat bar** reads as a long slab but internal items are not aligned to a shared baseline; weapon label, slots, and hearts feel misregistered.
- **Vertical rhythm** is inconsistent: top blocks sit too close to the playfield while the bottom bar feels detached from the action.

---

## 2) Decision Summary

**Chosen direction:** Option C (Top Progression + Center Score, Bottom Combat)

**Rationale:**
- Keeps progression info at the top while preserving a clean center strip for score/time.
- Maintains a unified combat bar at the bottom for fast action scanning.
- Avoids corner crowding while still separating combat vs progression.

**Element assignments (final):**
- **Top-left (progression):** era + level code, relic counter, collected items row, lives/deaths, difficulty badge
- **Top-center (performance):** score, timer, auto-select reason text
- **Bottom bar (combat tools):** weapon dock, abilities (double jump / air dash), special bar, heat bar, HP hearts

**Alignment rules (target):**
- Top-left and top-center panels share the same top edge and height.
- Top-left panel uses a fixed internal grid: title line, subline, badges row.
- Top-center panel is centered within the **safe area** (1800px wide), not the full canvas.
- Bottom bar aligns weapon dock, hearts, and meters to a single baseline at y=0 within the bar.
- Panel margins follow safe area offsets (see [QC-CHECKLIST.md](QC-CHECKLIST.md#L20-L33)).

---

## 3) Implementation Plan (Option C)

### Phase 0: Layout target map
Define three panels with consistent padding and alignment. Remove the single strip background and borders.

**Panel layout targets (in Canvas space):**
- **Top-left panel** size: 480x96, anchor (0,1), pivot (0,1), anchoredPosition (24, -24).
- **Top-center panel** size: 240x96, anchor (0.5,1), pivot (0.5,1), anchoredPosition (0, -24).
- **Bottom bar** size: 1200x96, anchor (0.5,0), pivot (0.5,0), anchoredPosition (0, 24).

Panel anchor targets (Canvas space, reference resolution 1920x1080):
- **Top-left (progression):** anchorMin (0,1), anchorMax (0,1), pivot (0,1)
- **Top-center (score/time):** anchorMin (0.5,1), anchorMax (0.5,1), pivot (0.5,1)
- **Bottom bar (combat):** anchorMin (0,0), anchorMax (1,0), pivot (0.5,0)

### Phase 1: Re-anchor the 4 clusters
Move HUD elements into cluster parents with their own RectTransforms:
- Create 3 parent GameObjects: HudTopLeft, HudTopCenter, HudBottomBar.
- Move each HUD element to the appropriate parent and update anchoredPosition values to be local to that parent.

**Specific alignments:**
- Top-left panel content (relative to panel):
  - Era line: (16, -16)
  - Relics line: (16, -40)
  - Difficulty + lives line: (16, -64)
- Top-center panel content (relative to panel):
  - Score line: (0, -18)
  - Timer line: (0, -44)
  - Auto-select line: (0, -70)
- Bottom bar content (relative to bar):
  - Weapon dock root: (24, 10)
  - HP hearts: (360, 12)
  - Abilities icons: (520, 12)
  - Special bar: (24, 60)
  - Heat bar: (220, 60)
  - Optional: place quick draw text above bar at (0, 120) with anchor (0.5,0)

### Phase 2: Update background styling
Decide between:
- **No panel background** (floating HUD) OR
- **Two top panels + one bottom bar** with epoch accent borders to preserve style.

Recommendation: use subtle borders on the top-left and top-center panels, and a single low-profile bottom combat bar.

**Consistency rule:** all three panels share the same border thickness and alpha.

### Phase 3: Safe area handling
Add safe-area offsets for top-left and top-center panels:
- Compute insets from Screen.safeArea and convert to Canvas space.
- Apply extra padding (e.g., +16 to +32 px) to avoid notches and status bars.
- Align top-center relative to the safe-area width (1800px) to keep it visually centered.

### Phase 4: Update hardcoded positions
- Remove references that assume a single bottom strip (y=90, 52, 14 rows).
- Replace with local offsets relative to each new panel parent.

### Phase 5: Regression test pass
Use the QC checklist below to validate layout and visibility.

---

## 4) Layout Notes and Constraints

- **Safe area:** The current code does not explicitly handle notches. This should be added for top panels. See [docs/PROJECT-STRUCTURE.md](docs/PROJECT-STRUCTURE.md#L170-L186) for viewport bounds.
- **Transient overlays:** Quick Draw, DPS cap, boss HP, achievement toast, era intro, mode info remain center/top-center and should avoid overlapping the top-center score panel.
- **Font scale:** Keep scale 3 for primary (score, timer, HP), scale 2 for secondary (relics, lives, difficulty).

---

## 5) CSS Prototype Snippets (for rapid layout mock)

For a single-page comparison view that renders Options A-D in the browser, see
[docs/HUD-Prototype-Review.md](docs/HUD-Prototype-Review.md).

These CSS blocks let you prototype the Option C layout in a browser (e.g., HTML mock). The goal is spatial visualization only.

```css
:root {
  --hud-padding: 24px;
  --panel-bg: rgba(10, 8, 22, 0.75);
  --panel-border: rgba(180, 140, 80, 0.8);
  --panel-shadow: rgba(0, 0, 0, 0.4);
  --text-primary: #ffffff;
  --text-secondary: #cfd2d9;
}

.hud {
  position: absolute;
  inset: 0;
  pointer-events: none;
  font-family: "Press Start 2P", monospace; /* placeholder */
}

.hud-panel {
  position: absolute;
  padding: var(--hud-padding);
  background: var(--panel-bg);
  border: 2px solid var(--panel-border);
  box-shadow: 0 6px 20px var(--panel-shadow);
}

.hud-top-left {
  top: 16px;
  left: 16px;
}

.hud-top-center {
  top: 16px;
  left: 50%;
  transform: translateX(-50%);
  text-align: center;
}

.hud-bottom-bar {
  bottom: 16px;
  left: 16px;
  right: 16px;
}

.hud-primary {
  color: var(--text-primary);
  font-size: 18px;
  line-height: 1.4;
}

.hud-secondary {
  color: var(--text-secondary);
  font-size: 12px;
  line-height: 1.6;
}
```

Example HTML block (for testing):

```html
<div class="hud">
  <div class="hud-panel hud-top-left">
    <div class="hud-primary">Era 1 [E-XXXXXX]</div>
    <div class="hud-secondary">Relics: 24/25</div>
    <div class="hud-secondary">Items: HP:2 ATK:1 | Lives: 2 | Easy 0.5X</div>
  </div>

  <div class="hud-panel hud-top-center">
    <div class="hud-primary">Score: 350</div>
    <div class="hud-primary">Time: 0:12.9</div>
    <div class="hud-secondary">>>AUTO: Best DPS</div>
  </div>

  <div class="hud-panel hud-bottom-bar">
    <div class="hud-primary">Weapon: Bolt | HP: [♥♥♥]</div>
    <div class="hud-secondary">Abilities: DJ | Dash</div>
    <div class="hud-secondary">Special: [===== ] | Heat: [==   ]</div>
  </div>
</div>
```

---

## 6) QC Checklist (Option C)

### Layout and Visibility
- Top-left progression panel and top-center score panel are visible in 16:9 and 4:3 aspect ratios.
- Top-center score panel does not overlap transient overlays (Quick Draw, boss HP, achievement toast, era intro).
- Top panels respect safe area and are not clipped by notches/status bars.
- Bottom combat bar does not overlap touch controls if mobile UI is enabled.

### Functional Verification
- HP hearts update correctly.
- Lives/deaths display updates correctly for Campaign vs Streak.
- Score and timer update correctly.
- Weapon dock updates when weapons are acquired or switched.
- Abilities (double jump / dash) update when acquired or consumed.
- Special meter and heat bar update and show/hide correctly.
- Relic counter updates and color shifts on relic loss.
- Collected items row updates only when counts change.
- Auto-select reason text appears and fades correctly.

### Regression Check
- Boss bar still appears at top center and updates health/phase color.
- Quick Draw and DPS cap overlays still appear and fade correctly.
- Achievement toast still animates from top center without obstruction.
- Era intro card still appears center and fades correctly.

---

## 7) Open Decisions

- Should the top-center score panel always be visible, or hide in non-score modes?
- Should collected items row be persistent in top-left, or appear only when changed?
- Should the bottom combat bar be a single panel or segmented into weapon and HP sub-panels?

---

**End of document**
