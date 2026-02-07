# Expert Review Process

Consolidated process guide for expert reviews. Covers workflow, roles, scope, quality standards, and scheduling.

## Quickstart

1. Fill out the intake form (see [Expert-Review-Templates.md](Expert-Review-Templates.md))
2. Attach change proposal and evidence (build link, playtest notes)
3. Schedule review timebox per cadence below
4. Receive findings and recommendation
5. Log decision and actions in Expert Feedback Log

## Roles

- **Product/Design Lead**: Submits review intake and makes final decisions
- **Expert Reviewer**: Evaluates and recommends using the competency framework
- **Engineering Lead**: Estimates feasibility and risk

## Cadence & SLAs

| Review Type | When | SLA |
|---|---|---|
| Pre-sprint | 2 days before sprint start | 2 business days |
| Mid-sprint | Midpoint (high-risk changes only) | 1 business day |
| Release gate | 2 days before release candidate | 2 business days |

## When to Request a Review

- New mechanics or core loop changes
- Changes affecting game feel, difficulty, or UX
- Any alteration to procedural generation rules
- New scoring, retention, or social features
- Release gate reviews

## Scope

**In scope**: Game feel, destructible environments, weapon balance, mobile UX, procedural generation fairness, difficulty curve, scoring/incentives, ethical retention, visual/audio feedback, cross-platform alignment.

**Out of scope**: Engine/build tooling, backend infrastructure, monetization policy (beyond ethical checks), marketing/ASO.

## Review Process

### Step 1: Intake
- Complete the intake form with change summary, rationale, and evidence
- Define review type and deadline

### Step 2: Preparation
- Verify evidence completeness
- Identify affected competencies
- Set scope and timebox

### Step 3: Execution
- Evaluate changes against affected competencies
- Note critical risks first
- Draft recommendations with validation steps

### Step 4: Decision
- Issue recommendation status: **Proceed** | **Proceed with Conditions** | **Hold for Revision** | **Reject**
- List conditions for approval if needed
- Identify evidence gaps

### Step 5: Follow-up
- Log review in Expert Feedback Log
- Capture decision in Decision Log
- Assign owners for follow-up actions
- Schedule validation

## Quality Standards

### Minimum Evidence for Review
- Change summary and rationale
- Prototype or build link
- Playtest notes (even small)
- Expected impacts and risks

### Review Output Must Include
- Findings prioritized by severity
- Competency impacts mapped
- Actionable, testable recommendations
- Clear tie to player impact
- Validation steps and evidence gaps

### Competency Areas Evaluated
1. 2D platformer feel
2. Destructible environments
3. Weapon system balance
4. Mobile UX
5. Procedural generation
6. Difficulty and bosses
7. Scoring and incentives
8. Ethical retention
9. Aesthetics and audio
10. Social and cross-platform

## Escalation

Any **Reject** recommendation requires a Design Lead decision and a documented rationale (use the Decision Rationale template).

## Metrics

### Quality
- Percent of reviews with actionable findings
- Average time from review to resolution
- Percentage of recommendations adopted

### Outcome
- Change success rate (meets stated criteria)
- Regression rate after changes

### Process
- SLA adherence rate
- Evidence completeness rate
- Review cycle time

## Glossary

| Term | Definition |
|---|---|
| Critical finding | Likely to harm fun or clarity if unaddressed |
| Condition | Required change before proceeding |
| Evidence gap | Missing artifact needed to evaluate |
| Proceed with Conditions | Changes may proceed after required fixes |
| Hold for Revision | Rework required before proceeding |

## Example Review

**Review type**: Pre-sprint | **Change**: Multi-weapon attachment system | **Status**: Proceed with Conditions

**Findings**:
- Critical: Weapon escalation risks trivializing mid-epoch difficulty. Add DPS ceilings per epoch.
- High: Auto-aim plus multi-weapon could remove positional skill. Add accuracy falloff.
- Medium: Attachment UI may interrupt flow. Use slow-motion only on first pickup.

**Conditions**: Provide DPS cap table per epoch; playtest validating challenge retention; boss phase tuning with multi-weapon enabled.
