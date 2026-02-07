# Expert Reply: v0.3.0 Response Review

**Date**: 2026-02-06
**Response reviewed**: [Expert-Review-Response-v0.3.0.md](Expert-Review-Response-v0.3.0.md)
**Design context**: [GAME-DESIGN-REVIEW.md](../GAME-DESIGN-REVIEW.md)

This reply reviews the proposed approaches for v0.4.0, answers the seven design questions, and flags risks and refinements. No code changes are included.

---

## Verdict Summary

- The proposed v0.4.0 approach is solid and aligned with the core fantasy.
- The biggest risk is over-nerfing auto-select and creating friction for casual play.
- Preservation reframe is correct; the low destruction bonus should be removed.
- Boss padding should be reduced by replacing one hard cap with a mechanic.
- Visual feedback polish should be a minimal, focused pass within v0.4.0.

---

## Responses to Design Questions

1) **Auto-select defaults to Bolt only**

**Answer**: Bolt-only is slightly too aggressive for approachability. Recommend a conservative auto-select that is context-aware but limited: Bolt by default, Piercer only in long corridors, and Slower only when a boss is active. This preserves agency while preventing the AI from solving the game.

2) **2,000 points (10% of max) for relic preservation**

**Answer**: 2,000 is acceptable if Preservation is reframed as optional mastery. If score distribution data shows Preservation dominating, reduce to 1,500. Keep the relic-only component and remove the low destruction bonus.

3) **Arena pillars in all phases or Phase 3 only**

**Answer**: Phase 3 only, to keep earlier phases readable and avoid visual clutter. If you want early-phase arena interaction, make pillars inert until Phase 3 so they function as setup rather than distraction.

4) **Hazard density cap at 30% vs. scan pulse**

**Answer**: Cap at 30% and adjust hazard pool weights first. Add a scan pulse only if late-epoch playtests still show frustration. The scan pulse introduces new cognitive load and input complexity.

5) **Cannon triggers FallingDebris vs. extra heat for reinforced**

**Answer**: FallingDebris is the better thematic balance. Add a small heat premium (+5) on reinforced only if debris alone does not curb overuse. Avoid stacking too many penalties at once.

6) **Score breakdown always visible vs. tap/click**

**Answer**: Collapsed by default with a single “details” toggle. Always-visible breakdowns will clutter the level complete screen and dilute the “win” moment.

7) **Visual polish in this cycle vs. dedicated polish cycle**

**Answer**: Include a minimal feel pass in v0.4.0 (screen shake, hit flash, destruction particles). Defer full animation work to a dedicated polish cycle later.

---

## Review of Proposed Sprints

### Sprint 1: Auto-select nerf

- **Keep**: manual cycling reward.
- **Refine**: limited auto-select to Bolt by default, with narrow exceptions (corridor Piercer, boss Slower).
- **Risk**: Bolt-only may feel like the game ignores interesting weapons for new players.

### Sprint 2: Preservation reframe

- **Approve**: remove low destruction bonus; rename to Archaeology.
- **Refine**: use one UI sentence to explain relics are a bonus, not a penalty.

### Sprint 3: Boss counterplay

- **Approve**: remove Phase 3 shield, reduce min phase to 5s, add pillars.
- **Refine**: pillar break should be telegraphed and avoid instant damage overlap.

### Sprint 4: Hazard balance

- **Approve**: cap density at 30% and increase CoverWall weights.
- **Refine**: ensure hazard placement never removes intended traversal lines.

### Sprint 5: Cannon consequences

- **Approve**: FallingDebris trigger on Cannon destruction.
- **Refine**: add crack pre-telegraph for debris spawn.

### Sprint 6: Score clarity

- **Approve**: breakdown tooltips and kill streak indicator.
- **Refine**: show “top two” contributors before expanding details.

### Sprint 7: Tutorial optional wall path

- **Approve**: optional wall path with reward.
- **Refine**: add a “soft nudge” hint only if player lingers.

### Sprint 8: Visual polish pass

- **Approve**: minimal feel pass only.
- **Refine**: avoid layering too many new effects at once to keep performance stable.

---

## Additional Expert Notes

- **Combat agency**: Ensure manual weapon cycling does something the AI cannot, even if only for short windows.
- **Preservation framing**: Use language like “relics recovered” rather than “blocks not destroyed.”
- **Boss clarity**: Use a consistent visual tell for Phase 3 pillar cover behavior.
- **Hazard density**: The pool shift toward CoverWall is key to keep late epochs fair.

---

## Decision Summary

- Proceed with v0.4.0 sprint plan **with refinements** above.
- Implement minimal visual polish in v0.4.0, defer full animations.

---

## Evidence Needed Before Implementation

- Late-epoch hazard playtest notes (3 sessions).
- Weapon cycling usage metrics (manual vs. auto).
- Score component distribution after a 10-run sample.
