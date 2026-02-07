# Expert Review Report: Epoch Breaker (v0.2.0 build 007)

This report uses the Expert Review Workflow and the competencies in GAME-DESIGN-REVIEW.md to provide actionable feedback, creative direction, and upgrade recommendations.

## Executive Summary

Epoch Breaker already has a strong core loop and a compelling destruction fantasy. The most significant upgrade opportunities are in (1) sustaining challenge under escalating firepower, (2) making destruction feel more strategic and less purely optimal to smash everything, (3) improving learnability and onboarding, and (4) aligning score incentives with interesting play choices rather than speed-only dominance.

## Findings (Ordered by Impact)

### Critical

1) **Power escalation risks trivializing mid-epoch difficulty.**
- Auto-fire plus tiered weapons can overpower enemy and material thresholds once Medium and Heavy are acquired.
- Recommendation: introduce soft caps and epoch-based resistances so weapon growth preserves challenge.

2) **Destructible environments currently reward maximal destruction over strategic preservation.**
- If the optimal play is always to break everything, the intended puzzle strategy is lost.
- Recommendation: add selective incentives (e.g., resources or traversal penalties) that make preservation sometimes optimal.

3) **Onboarding is missing for a system with multiple intertwined mechanics.**
- The combination of auto-fire, destruction, coyote time, and ground pound needs early teaching.
- Recommendation: a three-level onboarding flow with introduced mechanics in sequence.

### High

4) **Scoring heavily favors speed, reducing combat and exploration choices.**
- The time score floor is low and the time multiplier dominates early.
- Recommendation: rebalance score weights to promote combat mastery and exploration.

5) **Boss phases lack mechanic-specific testing with weapon escalation.**
- Multi-weapon system will likely change phase pacing and readability.
- Recommendation: add boss phase tuning with weapon tier permutations.

6) **Cross-platform parity risks input mismatch.**
- WebGL keyboard and mobile touch need distinct affordances and tutorial cues.
- Recommendation: platform-aware onboarding and input guidance.

### Medium

7) **Enemy behavior variety is limited relative to destruction and weapon systems.**
- More interaction between enemy types and destructible materials would increase depth.
- Recommendation: add material-aware enemy behaviors (e.g., tunneling, shielded, demolition-resistant).

8) **Daily challenge and social loops are not yet implemented but are central to retention.**
- Recommendation: prioritize daily seed sharing and leaderboard cycle design.

### Low

9) **Runtime-generated art is strong but needs an identity audit.**
- Recommendation: define motif and palette rules per epoch to improve visual identity.

10) **Synthesized audio lacks a pacing plan tied to gameplay intensity.**
- Recommendation: define intensity tiers (exploration, combat, boss) with waveform shifts.

---

## Competency-Based Recommendations

### 1) 2D Platformer Game Feel & Tuning

**Observations**
- Current values (coyote time, jump buffer, asymmetric gravity) support a forgiving feel.
- Unintentional wall-grab mechanic increases depth but risks confusion if not taught.

**Recommendations**
- Explicitly decide if wall-grab is a feature. If yes, teach it in onboarding and add a hint.
- Validate micro-latency by building a 3-step feel test (jump edge, short hop, ground pound cancel).

### 2) Destructible Environment Design

**Recommendations**
- Add materials that are easier to destroy but create navigation hazards (falling debris, unstable floor).
- Introduce optional preservation rewards (e.g., intact relics grant score multipliers or unlocks).
- Add destruction costs: breaking certain tiles reduces available cover or spawns hazards.

### 3) Weapon System & Power Fantasy

**Recommendations**
- Use epoch-based resistances or armor modifiers so enemies scale with power fantasy.
- Make some weapons emphasize utility over DPS (slow, pierce, chain).
- Introduce limited ammo or heat for the heaviest tier to prevent infinite dominance.

### 4) Mobile UX & Touch Controls

**Recommendations**
- Confirm touch target sizes and thumb zones for jump, attack, and ground pound.
- Add a one-handed accessibility mode with auto-move or simplified controls.
- Teach ground pound and coyote time as discrete touch gestures.

### 5) Procedural Level Design & Replayability

**Recommendations**
- Add handcrafted-feel signatures per epoch (unique landmarks, signature layouts).
- Use theme-driven constraints: e.g., Stone Age = more vertical terrain, Modern = narrow corridors.
- Validate that EnsureWalkablePath does not remove intended destruction puzzles.

### 6) Difficulty Curve & Boss Encounter Design

**Recommendations**
- Use boss phase triggers tied to time-in-fight as well as HP to avoid burst skipping.
- Add a pre-boss skill gate that aligns with the current weapon tier.
- Add checkpoint placement rules for boss failure loops.

### 7) Scoring, Incentive & Achievement Design

**Recommendations**
- Adjust scoring weights so combat, exploration, and clean runs are viable strategies.
- Add high-score bonuses for low destruction to encourage preservation runs.
- Add achievements that promote creative play (no damage + minimal destruction + low time).

### 8) Ethical Retention & Session Design

**Recommendations**
- Daily challenge seeds with opt-in streaks, no penalties for missing days.
- Add a return hook tied to era progression rather than FOMO.

### 9) Retro / Pixel Art Aesthetic Direction

**Recommendations**
- Define an epoch palette guide with 3 signature hues and contrast rules.
- Use material-specific animation patterns (cracks, crumble) to reinforce identity.

### 10) Chiptune & Synthesized Audio

**Recommendations**
- Define waveforms per biome and per intensity band.
- Add sonic cues for destruction levels (soft vs. reinforced).

### 11) Social/Competitive Mechanics for Single-Player

**Recommendations**
- Highlight shareable seed results after each run with a call-to-action.
- Add daily leaderboard filters for friends and global.

### 12) Cross-Platform Design (Mobile + Browser)

**Recommendations**
- Use platform-specific button labels and gesture prompts.
- Ensure difficulty tuning accounts for keyboard precision vs. touch imprecision.

---

## Creative Direction Ideas

- **Epoch Signature Mechanics**: Each epoch adds a twist to destruction (e.g., Stone Age = falling boulders, Medieval = brittle arches, Industrial = pressure pipes).
- **Destruction as Puzzle**: Introduce hidden routes that only open when selective walls remain intact.
- **Weapon Identity**: Give weapons a visual silhouette and audio signature per epoch.
- **Boss Arena Identity**: Each boss arena should encode an epoch signature mechanic as part of the fight.

---

## Code Recommendations (Non-Implementation)

These are recommendations only; no code changes are provided.

- Add a tuning layer for weapon damage scaling per epoch.
- Add data-driven rules for destruction incentives (reward for preserved structures).
- Add platform-aware input profiles and hint systems.

---

## Proposed Upgrade Roadmap (Short-Term)

1) Onboarding flow (Levels 1-3), teach destruction, auto-fire, ground pound.
2) Weapon escalation tuning with epoch-based resistance.
3) Destruction incentives (preserve vs. destroy) and scoring reweight.
4) Boss phase tuning with weapon tier permutations.
5) Daily seed challenge and leaderboard surfacing.

---

## Evidence Gaps

- No qualitative playtest notes included in the review.
- No touch ergonomics audit or thumb reach maps provided.
- No scoring telemetry data for player choice analysis.

---

## Next Actions

- Provide playtest notes and telemetry to validate the recommendations.
- Run a targeted boss tuning session with weapon escalation.
- Pilot a scoring rebalance in a test build and compare session outcomes.
