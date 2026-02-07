# Expert Feedback Log

Record expert reviews and decisions here for traceability.

## Entry Template

- Date:
- Reviewer:
- Review type:
- Change or milestone:
- Recommendation status:
- Top findings:
- Required actions:
- Owner:
- Target date:
- Evidence links:

---

## 2026-02-06 | Epoch Breaker Design Review

- Date: 2026-02-06
- Reviewer: Expert Reviewer (Process)
- Review type: Milestone review
- Change or milestone: v0.2.0 build 007 design review
- Recommendation status: Proceed with Conditions
- Top findings: Power escalation risks trivializing difficulty; destruction incentives need strategy; onboarding and scoring reweight needed
- Required actions: Add weapon scaling controls; introduce preservation incentives; implement onboarding; rebalance score weights
- Owner: Design Lead
- Target date: TBD
- Evidence links: docs/Expert-Review-Report.md

---

## 2026-02-06 | Epoch Breaker v0.3.0 Design Review

- Date: 2026-02-06
- Reviewer: Expert Reviewer (Process)
- Review type: Milestone review
- Change or milestone: v0.3.0 build 008 design review
- Recommendation status: Proceed with Conditions
- Top findings: Auto-select reduces combat agency; preservation may conflict with destruction fantasy; boss anti-trivialize stack risks padding
- Required actions: Add manual-cycling rewards; reframe preservation as optional mastery; replace one boss hard cap with mechanic counterplay
- Owner: Design Lead
- Target date: TBD
- Evidence links: docs/Expert-Review-Report-v0.3.0.md
	Additional: docs/Expert-Review-Report-v0.3.0-Memory-Addendum.md

---

## 2026-02-06 | Epoch Breaker v0.3.0 Implementation Response

- Date: 2026-02-06
- Reviewer: Implementation Lead
- Review type: Response to expert review
- Change or milestone: Proposed approaches for v0.4.0 (review cycle 2)
- Recommendation status: Awaiting Expert Approval
- Top proposals: Bolt-only auto-select + Quick Draw buff; remove low destruction bonus; replace Phase 3 shield with destructible arena pillars; cap hazard density at 30%; score breakdown tooltips; visual polish pass
- Required actions: Expert approval of proposed approaches before implementation
- Owner: Design Lead
- Target date: TBD
- Evidence links: docs/Expert-Review-Response-v0.3.0.md

---

## 2026-02-06 | Expert Reply to v0.3.0 Response

- Date: 2026-02-06
- Reviewer: Expert Reviewer (Process)
- Review type: Response review
- Change or milestone: v0.4.0 proposed sprint plan
- Recommendation status: Proceed with Conditions
- Top findings: Auto-select should allow limited context; preservation reframe approved; boss padding reduced with Phase 3 pillars only
- Required actions: Adopt limited auto-select exceptions; keep preservation at 2,000 max; cap hazards at 30% before adding scan pulse
- Owner: Design Lead
- Target date: TBD
- Evidence links: docs/Expert-Review-Response-v0.3.0-Expert-Reply.md

---

## 2026-02-06 | v0.4.0 Implementation Plan Finalized

- Date: 2026-02-06
- Reviewer: Implementation Lead
- Review type: Plan finalization
- Change or milestone: v0.4.0 implementation plan locked (8 sprints)
- Recommendation status: Ready for Implementation
- Top decisions: Limited auto-select (Bolt + Piercer + Slower); Archaeology reframe; Phase 3 arena pillars; 30% hazard cap; collapsed score details; minimal visual polish
- Required actions: Implement sprints 1-8 in order: 1 → 2 → 4 → 5 → 3 → 6 → 7 → 8
- Owner: Implementation Lead
- Target date: v0.4.0 build 009
- Evidence links: docs/Implementation-Plan-v0.4.0.md

---

## 2026-02-07 | Epoch Breaker v0.5.0 Comprehensive Review (Cycle 3)

- Date: 2026-02-07
- Reviewer: Expert Reviewer (Deep Review — Code + Design + Fun + Creativity + Gamification)
- Review type: Comprehensive milestone review
- Change or milestone: v0.5.0 build 011 full codebase review
- Recommendation status: Proceed with Conditions
- Top findings: (1) Game lacks juice — no screen shake, hit-stop, or satisfying feedback; (2) Invisible systems frustrate players — DPS cap, Quick Draw, abilities all hidden; (3) Boss encounters are underwhelming — no intro, health bar, or phase drama; (4) No object pooling — GC stutters on WebGL/mobile; (5) 10 eras feel cosmetically identical beyond tile palettes
- Critical bugs: Boss slow-effect permanently reduces speed after phase change; boss contact damage has no cooldown; DPS cap truncates float→int; ability integration may fail; weapon auto-select uses wrong range; boss trigger can soft-lock
- Required actions: Fix all critical/high bugs; implement juice pass; boss encounter overhaul; object pooling; era differentiation; score visibility; weapon balance
- Competency scores: 0 of 12 pass (avg 63.5/100). Highest: H8 Ethical Retention (82). Lowest: C4 Mobile UX (45, unimplemented).
- Owner: Implementation Lead
- Target date: v0.6.0 build 012
- Evidence links: docs/Expert-Review-Report-v0.5.0.md, docs/Implementation-Plan-v0.6.0.md

