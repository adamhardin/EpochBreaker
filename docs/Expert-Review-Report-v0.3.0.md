# Expert Review Report: Epoch Breaker (v0.3.0 build 008)

This report uses the Expert Review Workflow and the competencies in GAME-DESIGN-REVIEW.md for deep game design feedback aimed at maximizing fun. This is a design review only; no code changes are included.

## Executive Summary

Epoch Breaker made strong, targeted improvements since v0.2.0, notably the tutorial, 6-slot weapon system, and strategic destruction via hazards and relic preservation. The biggest remaining risks are (1) too much automation in combat decisions, (2) over-rewarded preservation that conflicts with the destruction fantasy, (3) potential boss padding due to anti-trivialize mechanics stacking, and (4) missing moment-to-moment feedback (animations/particles/screen shake) that will make the game feel flat even when systems are strong.

## Strengths

- Strong core fantasy and identity with era progression and destructible environments.
- The 6-weapon system introduces real tactical diversity and reduces monotony.
- The 5-component scoring system finally supports multiple play styles.
- The tutorial is structured and platform-aware, which will help mobile retention.

## Findings (Ordered by Impact)

### Critical

1) **Auto-select AI + manual cycling may reduce player agency in combat decisions.**
- If the system always chooses correctly, the player stops making interesting weapon choices.
- Recommendation: add decision tradeoffs and short-term costs so manual cycling matters.

2) **Preservation incentives risk conflicting with the destruction fantasy.**
- The Preservation score and relics can feel like “do not destroy” penalties.
- Recommendation: frame preservation as a separate optional mastery lane rather than a dominant score path.

3) **Boss anti-trivialize stack may feel padded.**
- DPS cap + minimum phase duration + Phase 3 shield can read as artificial.
- Recommendation: replace at least one “hard cap” with a mechanic-driven counterplay.

### High

4) **Hazard scaling (5% to 40%) may over-punish late epochs without fresh tools.**
- Hazard density scaling needs to align with new player power and navigation tools.

5) **Weapon diversity is strong, but Cannon breaks all materials, risking trivial traversal.**
- Even with heat, Cannon can flatten the generation’s intended structure.

6) **Combat Mastery score is likely difficult to understand in play.**
- Efficiency and no-damage streaks are opaque; players will not see how to optimize.

### Medium

7) **Wall-slide/wall-jump feel is likely good, but chimney climbing could allow sequence breaking.**
- Decide if sequence breaking is intended or should be constrained by level layout.

8) **Tutorial length is solid but may overteach wall mechanics early.**
- Consider introducing wall jump later for retention rather than in the onboarding block.

### Low

9) **The game still lacks the feedback polish needed for “crisp” feel.**
- Missing animations, destruction particles, and screen shake will dampen fun.

---

## Deep System Feedback

### 1) Game Feel and Movement

- The movement spec is strong and consistent with tight platformers.
- The wall mechanics are now a core feature. This should be reinforced via level design patterns after tutorial to avoid “forgotten mechanic” drift.

**Recommendations**
- Add a post-tutorial “wall skill gate” in early campaign epochs to keep the skill alive.
- Consider a wall fatigue or timing window if chimney climbing breaks intended paths.

### 2) Weapons and Power Fantasy

- The 6-slot system is excellent, but auto-select AI risks reducing player expression.

**Recommendations**
- Add “weapon friction” tradeoffs: ammo/heat for Cannon, limited chain bounce count, or reduced relic preservation with heavy weapons.
- Make manual cycling grant a temporary micro-buff (accuracy or burst) to reward agency.

### 3) Destruction, Hazards, and Preservation

- Hazards create strong tension and are the right direction.
- Preservation bonus is interesting but must not feel like a punishment to the core fantasy.

**Recommendations**
- Use preservation as an alternate mastery lane that unlocks cosmetic rewards, not main score dominance.
- Add “collateral-safe” weapons or abilities that allow precise destruction without relic damage.

### 4) Scoring, Incentives, and Achievements

- The 5-component system is a big improvement. The risk is opacity: players may not understand how to play for a component.

**Recommendations**
- Provide post-level tooltips that explain why the score shifted (e.g., “Preservation bonus: 85% relics intact”).
- Add a “score lens” UI in pause or level complete to show each component’s controllable actions.

### 5) Procedural Generation and Skill Gate

- The SkillGate addition is smart and aligns with boss readiness.

**Recommendations**
- Ensure SkillGate always tests the era’s key mechanic (hazard type or weapon tier). If it does not, it becomes filler.
- Consider era-specific landmarks to improve handcrafted feel.

### 6) Boss Design and Anti-Trivialize Mechanics

- DPS cap and min phase duration solve burst skipping, but can feel artificial.

**Recommendations**
- Replace one hard cap with a mechanic: shield that drops when player destroys arena objects, or a boss “overheat” window tied to weapon choice.
- Ensure phase transitions include a readable tell to avoid surprise shield moments.

### 7) Tutorial and Onboarding

- The 3-step tutorial is solid and should reduce early churn.

**Recommendations**
- Keep wall-jump in Level 3, but consider gating it behind an optional challenge to avoid overloading new players.
- Add a short “practice arena” on tutorial replay to jump directly to advanced mechanics.

### 8) Audio and Visual Identity

- Runtime generation is impressive, but identity needs deeper motif rules.

**Recommendations**
- Define era-specific audio motifs (percussion rhythms, waveform signatures) tied to hazard intensity.
- Add minimal animation frames to player and weapon pickups as a priority for feel.

---

## Responses to Key Design Questions

1) **Wall-slide/wall-jump feel**: Values seem satisfying; chimney climbing is likely too easy unless gated by level layout or stamina-like friction.
2) **Weapon diversity vs. complexity**: 6 types is fine if auto-select is conservative. Manual cycling must feel worthwhile.
3) **Hazard-destruction tension**: The concept is strong; 40% at epoch 9 may be too punishing without stronger player tools.
4) **Scoring balance**: The 5-component system is good, but Combat Mastery may dominate if not visible. Needs clarity tooling.
5) **Boss anti-trivialize mechanics**: The stack risks padding. Replace one hard cap with a mechanic-driven counter.
6) **Tutorial pacing**: Good length, but ensure Level 3 is not too dense for first-time users.
7) **Difficulty curve**: Smooth interpolation is correct. Epoch 0 may be too easy if tutorial already teaches basics; consider a slight bump at epoch 1.
8) **Relic preservation incentive**: Works if treated as optional mastery. Avoid making it the “best” score path.
9) **Level sharing social mechanic**: Add post-run call-to-action and curated “seed of the day” curation to spark sharing.
10) **Most impactful unbuilt feature**: Visual feedback polish (animations + particles) will yield the largest immediate fun gain, followed by daily challenges for retention.

---

## Creative Direction Ideas

- **Era Signature Mechanics**: Each era introduces a hazard archetype that defines its identity.
- **Weapon Archetypes Per Era**: Encourage certain weapons in each era through environment design.
- **Preservation-as-Discovery**: Preserving relics reveals lore or mini-challenges rather than pure points.

---

## Non-Code Recommendations (Design-Level)

- Add an era-specific “identity pass” checklist (palette, hazards, weapon affinity).
- Add a scoring clarity HUD module to explain component changes in real time.
- Add a boss counterplay rule that rewards destruction strategy rather than DPS.

---

## Evidence Gaps

- No qualitative playtest notes for the new tutorial or 6-weapon system.
- No telemetry on score component distribution (which component dominates).
- No mobile touch ergonomics or reach-zone audit.

---

## Next Actions

1) Run 3 playtests targeting tutorial overload and weapon cycling usage.
2) Collect score component telemetry to verify balance.
3) Prototype a boss counterplay mechanic that replaces one hard cap.
4) Prioritize animation/particle polish for game feel.
