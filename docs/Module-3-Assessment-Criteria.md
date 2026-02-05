# Module 3: Game Design & Gamification - Assessment Criteria

## Module Overview
This module develops expertise in game design theory, reward systems, progression mechanics, difficulty pacing, and ethical gamification. Learners must demonstrate ability to design engaging, balanced gameplay loops that drive retention without exploitative mechanics.

---

## Learning Objectives

By completion, learners will be able to:
1. Design a multi-layered progression system with clear player motivation
2. Construct difficulty curves that teach, challenge, and satisfy
3. Apply the MDA (Mechanics, Dynamics, Aesthetics) framework to design decisions
4. Evaluate and avoid dark patterns in gamification
5. Define and track key performance indicators (KPIs) for engagement and retention

---

## Assessment 3.1: Progression System Design

### Objective
Design a complete progression system for a retro side-scrolling game that sustains player engagement across 10+ hours of gameplay.

### Requirements

**Core Loop Definition:**
- [ ] Identify the 30-second core loop (moment-to-moment gameplay)
- [ ] Identify the 5-minute session loop (level start to level end)
- [ ] Identify the multi-session meta loop (progression across sessions)
- [ ] Document how each loop feeds into the next

**Progression Layers:**
- [ ] Skill progression: How does the player get better mechanically?
- [ ] Content progression: How do new levels/biomes/enemies unlock?
- [ ] Power progression: How do stats, abilities, or upgrades advance?
- [ ] Narrative progression: What story or thematic arc drives forward motion?

**Unlock Schedule:**
- [ ] Create an unlock timeline for 50 levels across 5 difficulty tiers
- [ ] Map which enemies, biomes, power-ups, and abilities appear at each tier
- [ ] Ensure no unlock gaps (player always has something new within 3-5 levels)
- [ ] Document the "first 15 minutes" experience in detail

### Deliverables
```
progression_system_design.md     (complete system specification)
unlock_schedule.csv              (level-by-level unlock map)
core_loop_diagram.png            (visual of nested loops)
first_15_minutes.md              (minute-by-minute new player experience)
```

### Success Criteria
- All three loop layers are clearly defined and interconnected
- Unlock schedule has no dead zones (3+ levels with nothing new)
- First 15 minutes includes at least 3 meaningful rewards
- System supports both short (5 min) and long (30 min) sessions
- No pay-to-win or pay-to-skip mechanics in design

### Evaluation Rubric
| Criteria | Excellent (5) | Good (4) | Acceptable (3) | Needs Work (2) |
|----------|--------------|---------|----------------|----------------|
| Loop Design | All loops clear, interconnected | Well-defined loops | Adequate design | Missing or unclear loops |
| Progression Depth | Multi-layered, satisfying | Good depth | Adequate variety | Shallow or repetitive |
| Pacing | No dead zones, constant motivation | Minor gaps | Some flat spots | Significant gaps |
| Ethical Design | No dark patterns, player-first | Minor concerns | Acceptable | Exploitative elements |
| Documentation | Comprehensive, actionable | Clear | Adequate | Incomplete |

**Pass Threshold**: Average >= 4.0/5.0

---

## Assessment 3.2: Difficulty Curve Design

### Objective
Design and document difficulty curves for a 20-level sequence that teaches mechanics progressively while maintaining challenge.

### Requirements

**Difficulty Framework:**
- [ ] Define difficulty dimensions: traversal, combat, timing, resource management
- [ ] Assign weights to each dimension per level
- [ ] Create a composite difficulty score (1-10 scale) per level
- [ ] Graph the difficulty curve and annotate key moments

**Teaching Sequence (Levels 1-5):**
- [ ] Level 1: Teach movement (walk, jump) -- no enemies, no death risk
- [ ] Level 2: Introduce enemies (1 type, simple patrol) -- low risk
- [ ] Level 3: Combine movement + enemies -- first real challenge
- [ ] Level 4: Introduce new mechanic (e.g., moving platforms) -- controlled environment
- [ ] Level 5: Skill gate -- test all learned mechanics together

**Escalation Sequence (Levels 6-15):**
- [ ] Gradually introduce new enemy types (1 per 2-3 levels)
- [ ] Increase traversal complexity (gap sizes, platform speed)
- [ ] Introduce hazards (spikes, lava, falling platforms)
- [ ] Each level adds ONE new element on top of mastered skills

**Climax Sequence (Levels 16-20):**
- [ ] Combine all mechanics in increasing complexity
- [ ] Level 18: Pre-boss gauntlet (test all skills)
- [ ] Level 19: Boss encounter (multi-phase)
- [ ] Level 20: Victory lap / reward level (catharsis)

### Deliverables
```
difficulty_curve_spec.md         (complete specification)
difficulty_curve_graph.png       (visual curve with annotations)
level_breakdown.csv              (per-level difficulty scores and new mechanics)
teaching_moments.md              (how each mechanic is introduced)
```

### Success Criteria
- Difficulty curve is monotonically increasing (with intentional dips for rest)
- No difficulty spikes greater than 2 points between adjacent levels
- Every mechanic is taught before it's tested
- Boss encounter difficulty matches the culmination of learned skills
- Rest points (lower-difficulty levels) appear every 4-6 levels

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Curve Shape | /25 | Smooth, intentional, well-paced |
| Teaching Design | /25 | Mechanics taught before tested |
| Escalation Quality | /25 | New elements layered well |
| Documentation | /25 | Clear graphs, specs, rationale |

**Pass Threshold**: >= 85/100

---

## Assessment 3.3: Retention Analysis & Design

### Objective
Analyze retention patterns in mobile games and design specific features to improve Day-1, Day-7, and Day-30 retention for a retro side-scroller.

### Requirements

**Research Phase:**
- [ ] Analyze retention benchmarks for mobile platformers (industry data)
- [ ] Identify the top 3 reasons players churn in the first session
- [ ] Identify the top 3 reasons players return after Day 1
- [ ] Document findings with citations

**Design Phase:**
- [ ] Design a "first session" flow that drives Day-1 return
  - What hook keeps the player thinking about the game after closing it?
  - What unfinished business motivates reopening?
- [ ] Design a daily engagement mechanic (not exploitative)
  - Daily challenge levels (unique seed each day, shared globally)
  - Progress tracking that rewards consistency
- [ ] Design a social/sharing feature that drives organic acquisition
  - Level ID sharing with friend comparison
  - Challenge mode: "Beat my level"
- [ ] Design a long-term goal system for Day-30+ retention
  - Achievement system (meaningful, not checkbox)
  - Mastery goals (perfect-run challenges, speedrun targets)

**Anti-Pattern Audit:**
- [ ] Review design for dark patterns (FOMO, artificial scarcity, pay-to-progress)
- [ ] Ensure all engagement mechanics are opt-in, not coercive
- [ ] Document ethical design principles applied

### Deliverables
```
retention_analysis.md            (research and findings)
retention_features_spec.md       (feature specifications)
ethical_design_audit.md          (dark pattern review)
kpi_dashboard_spec.md            (what to measure and thresholds)
```

### Success Criteria
- Research cites at least 3 industry sources
- Retention features are specific and implementable (not vague)
- Ethical audit identifies and avoids all common dark patterns
- KPI spec includes specific thresholds (D1 >= 40%, D7 >= 20%, D30 >= 10%)
- Daily challenge mechanic is fully specified

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Research Quality | /20 | Sourced, accurate analysis |
| Feature Design | /30 | Specific, implementable features |
| Ethical Design | /20 | No dark patterns, player-first |
| KPI Framework | /15 | Measurable, actionable metrics |
| Documentation | /15 | Clear and comprehensive |

**Pass Threshold**: >= 85/100

---

## Assessment 3.4: Reward System Architecture

### Objective
Design a complete reward system that provides clear, consistent feedback for player actions at multiple timescales.

### Requirements

**Immediate Rewards (per-action):**
- [ ] Define visual/audio feedback for: enemy defeat, coin collect, health pickup, power-up
- [ ] Specify haptic feedback patterns for each reward type
- [ ] Document the "juice" elements (screen shake, particle effects, sound stings)

**Level Rewards (per-level):**
- [ ] Score calculation formula (time, enemies defeated, damage taken, secrets found)
- [ ] Star/grade system (1-3 stars based on performance)
- [ ] Bonus rewards for perfect runs, speedruns, no-damage clears
- [ ] Level completion summary screen layout

**Meta Rewards (cross-session):**
- [ ] Achievement system (15+ achievements with tiers: bronze, silver, gold)
- [ ] Leaderboards (global, friends, weekly)
- [ ] Unlockables tied to milestones (cosmetics, not gameplay advantages)
- [ ] Statistics tracking (total enemies defeated, levels completed, time played)

### Deliverables
```
reward_system_spec.md            (complete architecture)
achievement_list.md              (all achievements with unlock conditions)
score_formula.md                 (detailed scoring algorithm)
reward_feedback_matrix.md        (action -> visual/audio/haptic response mapping)
```

### Success Criteria
- Every player action has clear, satisfying feedback
- Score formula is transparent and fair
- Achievement list covers skill, exploration, and persistence
- No rewards are gated behind payment
- Reward pacing has no dead zones

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Immediate Feedback Design | /25 | Clear, satisfying per-action |
| Level Reward System | /25 | Fair scoring, clear grades |
| Meta Reward Depth | /25 | Achievements, stats, unlocks |
| Documentation | /25 | Complete, implementable specs |

**Pass Threshold**: >= 85/100

---

## Assessment 3.5: Monetization Strategy (Ethical)

### Objective
Design a monetization approach that sustains development without exploiting players.

### Requirements

**Model Selection:**
- [ ] Evaluate: Premium (paid upfront), Free-to-play with ads, Freemium, Cosmetic IAP
- [ ] Select and justify a model for this specific game
- [ ] Document revenue projections based on download estimates

**If Premium:**
- [ ] Justify price point ($0.99, $1.99, $2.99, $4.99)
- [ ] Plan for sales, promotions, and price reductions
- [ ] Design the value proposition (what does the player get?)

**If Free-to-Play / Freemium:**
- [ ] Design ad placement (rewarded video only, not interstitial)
- [ ] Define what is free vs. paid
- [ ] Ensure core gameplay is never gated by payment
- [ ] Design IAP catalog (cosmetics only, no gameplay advantage)

**Ethical Compliance:**
- [ ] No loot boxes or randomized purchases
- [ ] No pay-to-win mechanics
- [ ] No energy systems or artificial wait timers
- [ ] No FOMO-based limited-time purchases for gameplay items
- [ ] Children's safety compliance (if applicable)

### Deliverables
```
monetization_strategy.md         (model selection, justification, projections)
iap_catalog.md                   (if applicable: items, prices, descriptions)
ethical_compliance_checklist.md   (audit against dark patterns)
```

### Success Criteria
- Model is clearly justified with market data
- Revenue projections are realistic (not aspirational)
- Ethical audit passes with no violations
- Core gameplay is accessible without payment
- Strategy is implementable within the technical stack

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Model Justification | /25 | Data-driven, realistic |
| Revenue Projections | /20 | Grounded in comparable titles |
| Ethical Design | /30 | No dark patterns or exploitation |
| Implementation Feasibility | /15 | Achievable within tech stack |
| Documentation | /10 | Clear and actionable |

**Pass Threshold**: >= 85/100

---

## Module 3 Final Assessment: Capstone Project

### Objective
Produce a complete Game Design Document (GDD) section covering progression, difficulty, retention, rewards, and monetization for the retro side-scroller.

### Requirements
- [ ] All five assessment areas integrated into a cohesive design
- [ ] Progression system supports the generative level system (infinite levels via seeds)
- [ ] Difficulty curve adapts to procedurally generated content
- [ ] Retention features leverage the unique level ID sharing mechanic
- [ ] Reward system provides clear feedback at all timescales
- [ ] Monetization is ethical and sustainable

### Evaluation Process
1. Self-assessment by learner
2. Peer review by 2 other learners
3. Design lead review for coherence and feasibility
4. Final approval for Module completion

**Pass Threshold**: Approved by design lead + both peers

---

## Module Completion Checklist

- [ ] Assessment 3.1: Progression System Design (PASS)
- [ ] Assessment 3.2: Difficulty Curve Design (PASS)
- [ ] Assessment 3.3: Retention Analysis & Design (PASS)
- [ ] Assessment 3.4: Reward System Architecture (PASS)
- [ ] Assessment 3.5: Monetization Strategy (PASS)
- [ ] Capstone Project (APPROVED)
- [ ] Module 3 Knowledge Check (80%+ score)

**Module Status**: Ready to advance to Module 4

---

## Resources & References

### Essential Readings
- "The Art of Game Design" - Jesse Schell (Chapters on loops and lenses)
- "MDA: A Formal Approach to Game Design" - Hunicke, LeBlanc, Zubek (2004)
- "Hooked: How to Build Habit-Forming Products" - Nir Eyal (with ethical lens)
- "A Theory of Fun for Game Design" - Raph Koster

### Reference Games (Progression Study)
- Mega Man X (skill progression, teaching through level design)
- Celeste (difficulty curve, assist mode as ethical accessibility)
- Shovel Knight (progression pacing, reward distribution)
- Super Meat Boy (difficulty escalation, short session loops)

### Industry Data
- GameAnalytics benchmarks for mobile retention
- Sensor Tower / data.ai for mobile game revenue comparisons
- GDC talks on ethical game design and player psychology

### Tools
- Machinations (game economy modeling)
- Google Sheets / Excel (unlock schedules, difficulty curves)
- Figma / draw.io (loop diagrams, flow charts)
