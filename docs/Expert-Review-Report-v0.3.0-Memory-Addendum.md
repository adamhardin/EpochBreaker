# Expert Review Addendum: Project Memory (v0.3.0)

This addendum reviews MEMORY.md for v0.3.0 and highlights alignment issues, risks, and recommendations that affect design fidelity and developer workflow.

## Findings (Ordered by Impact)

### Critical

1) **Engine version conflict across docs.**
- MEMORY.md states Unity 2022.3 LTS, while GAME-DESIGN-REVIEW.md states Unity 6000.3.6f1.
- Recommendation: align engine target in all docs to avoid build/tooling divergence.

### High

2) **Dual project locations require strict sync discipline.**
- MEMORY.md emphasizes syncing EpochBreaker/ and unity_project/.
- Recommendation: add a documented sync workflow and guardrails (pre-commit checks or a checklist) to prevent drift.

3) **Coordinate system inversion is a high-risk footgun.**
- `LevelData` y=0 is TOP and Unity y=0 is BOTTOM.
- Recommendation: add a one-page visual diagram and a “conversion checklist” referenced in all level tooling docs.

### Medium

4) **Gameplay feel features are now central but depend on missing visual feedback.**
- Wall slide squish and dust exist, but no broader animation/particle support yet.
- Recommendation: prioritize minimal animation frames and destruction particles to match stated feel goals.

### Low

5) **Known remaining gaps list is incomplete.**
- MEMORY.md only mentions parallax and no imported assets; other gaps in GAME-DESIGN-REVIEW.md include animations, particles, screen shake, daily challenges, cosmetics.
- Recommendation: align the “Known Remaining Gaps” list across docs.

---

## Consistency Checks

- **Weapon system**: Memory matches the 6-slot weapons, heat, and epoch resistance in GAME-DESIGN-REVIEW.md.
- **Hazards and relics**: Memory aligns with hazard types and relic preservation scoring.
- **Scoring**: Memory aligns with 5-component scoring and star thresholds.
- **Difficulty**: Memory aligns with DifficultyProfile and boss anti-trivialize mechanics.

---

## Recommendations

1) Add a “Docs Consistency” checklist entry to the review process that verifies engine version, control schemes, and current build ID across MEMORY.md and GAME-DESIGN-REVIEW.md.
2) Add a single source of truth for engine version and build target (suggest: README.md).
3) Add a “Coordinate System” diagram to the Level Generation Technical Spec.
4) Expand MEMORY.md known gaps to match the unbuilt features list in GAME-DESIGN-REVIEW.md.

---

## Evidence Gaps

- No confirmation of actual Unity Editor version used in current builds.
- No written sync procedure between EpochBreaker/ and unity_project/.

---

## Next Actions

- Decide engine version and update all docs accordingly.
- Add sync workflow to PROJECT-STRUCTURE.md or a new Sync Guide doc.
- Add a coordinate conversion diagram to Level-Generation-Technical-Spec.md.
