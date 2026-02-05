# Module 4: Side-Scrolling Mechanics - Assessment Criteria

## Module Overview
This module develops deep expertise in the physics, controls, enemy design, camera systems, and level mechanics that define a quality side-scrolling platformer. Learners must demonstrate ability to tune game feel to professional standards, referencing classic retro titles as benchmarks.

---

## Learning Objectives

By completion, learners will be able to:
1. Tune jump arcs, gravity, and acceleration curves for precise platforming feel
2. Design enemy archetypes with distinct behaviors, attack patterns, and counterplay
3. Implement and tune camera systems appropriate for side-scrolling gameplay
4. Design boss encounters with multi-phase attack patterns and readable tells
5. Balance power-ups and combat mechanics for fair, engaging gameplay

---

## Assessment 4.1: Player Physics & Jump Tuning

### Objective
Define and tune the complete player physics model for a retro side-scroller, producing a character that feels responsive, weighty, and satisfying to control.

### Requirements

**Physics Parameters (Full Specification):**
- [ ] Horizontal movement:
  - Walk speed (tiles/second)
  - Run speed (if applicable)
  - Acceleration time (0 to max speed, in frames)
  - Deceleration time (max speed to 0, in frames)
  - Air control factor (% of ground speed available in air)
- [ ] Vertical movement:
  - Jump height (tiles)
  - Jump duration (frames from launch to apex)
  - Gravity (tiles/frame^2) -- rising vs. falling (variable gravity)
  - Terminal velocity (max fall speed)
  - Coyote time (frames after leaving platform where jump is still valid)
  - Jump buffer (frames before landing where jump input is queued)
- [ ] Variable jump height:
  - Minimum jump (tap) vs. maximum jump (hold) -- height difference
  - Release behavior (cut vertical velocity on button release)

**Reference Analysis:**
- [ ] Measure and document jump parameters from 3 classic games:
  - Mega Man X, Super Mario World, Castlevania IV
- [ ] Compare each game's "feel" to the parameters that produce it
- [ ] Identify which reference game's feel is closest to the target

**Tuning Deliverables:**
- [ ] Parameter spreadsheet with all values
- [ ] Jump arc diagram (pixel-accurate trajectory at multiple speeds)
- [ ] Test criteria: "A jump should feel [adjective] because [parameter reasoning]"
- [ ] Edge case handling: wall collision, ceiling bump, landing on moving platforms

### Deliverables
```
physics_spec.md                  (complete parameter specification)
physics_parameters.csv           (all tunable values with ranges)
jump_arc_analysis.png            (trajectory diagrams)
reference_comparison.md          (classic game parameter comparison)
test_criteria.md                 (how to verify "feel" is correct)
```

### Success Criteria
- All physics parameters are explicitly defined (no undefined behavior)
- Variable jump height works correctly (tap vs. hold)
- Coyote time and jump buffer are specified and justified
- Jump arc feels satisfying when visualized (smooth parabola, not floaty or snappy)
- Reference analysis demonstrates understanding of why classic games feel good

### Evaluation Rubric
| Criteria | Excellent (5) | Good (4) | Acceptable (3) | Needs Work (2) |
|----------|--------------|---------|----------------|----------------|
| Parameter Completeness | All params defined | 1-2 missing | Several undefined | Many gaps |
| Reference Analysis | Deep, insightful comparison | Good analysis | Adequate | Surface-level |
| Jump Feel | Professional, satisfying | Good feel | Acceptable | Floaty or stiff |
| Edge Cases | All handled | Most handled | Major cases covered | Gaps |
| Documentation | Comprehensive | Clear | Adequate | Incomplete |

**Pass Threshold**: Average >= 4.0/5.0

---

## Assessment 4.2: Enemy Archetype Design

### Objective
Design 5 distinct enemy archetypes and 3 boss patterns with clear behaviors, attack patterns, counterplay, and difficulty scaling.

### Requirements

**5 Enemy Archetypes:**

For each archetype, specify:
- [ ] Name, visual description, and biome affinity
- [ ] Movement behavior (patrol, chase, stationary, flying)
- [ ] Attack pattern (melee, ranged, area denial)
- [ ] Health points (scaling with difficulty tier)
- [ ] Damage dealt to player
- [ ] Counterplay: How does the player defeat this enemy? What's the "tell"?
- [ ] Difficulty weight (contribution to level difficulty score)
- [ ] Spawn constraints: Where can this enemy appear? Minimum platform width?

**Archetype Variety Requirements:**
- At least 1 melee enemy (close-range threat)
- At least 1 ranged enemy (projectile-based)
- At least 1 mobile enemy (flying or fast-moving)
- At least 1 area-denial enemy (creates hazard zones)
- At least 1 "puzzle" enemy (requires specific approach to defeat)

**3 Boss Patterns:**
- [ ] Boss 1 (Difficulty 1-2): Simple, readable pattern, 2 phases
- [ ] Boss 2 (Difficulty 3-4): Complex pattern, 3 phases, arena hazards
- [ ] Boss 3 (Difficulty 5): Multi-phase, combines mechanics, requires mastery
- [ ] Each boss has: health pool, attack timeline, vulnerability windows, tells
- [ ] Arena layout specification for each boss (platform arrangement, hazards)

### Deliverables
```
enemy_archetypes.md              (5 complete archetype specifications)
boss_patterns.md                 (3 boss encounter specifications)
enemy_difficulty_weights.csv     (archetype -> difficulty contribution)
behavior_state_diagrams.png      (state machine for each enemy AI)
counterplay_guide.md             (how player handles each threat)
```

### Success Criteria
- All 5 archetypes are mechanically distinct (different counterplay required)
- Bosses escalate in complexity matching their difficulty tier
- Every enemy has a readable "tell" before attacking
- Difficulty weights are consistent with the level generation system
- Spawn constraints prevent unfair placements (e.g., ranged enemy on tiny platform)

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Archetype Variety | /20 | Mechanically distinct enemies |
| Behavior Specification | /20 | Clear state machines and patterns |
| Boss Design Quality | /25 | Escalating, fair, engaging encounters |
| Counterplay Design | /20 | Every threat has learnable counter |
| Documentation | /15 | Complete, implementable specs |

**Pass Threshold**: >= 85/100

---

## Assessment 4.3: Camera System Design

### Objective
Design and specify a camera system appropriate for a side-scrolling platformer with procedurally generated levels.

### Requirements

**Camera Behaviors:**
- [ ] Horizontal tracking: How does the camera follow the player horizontally?
  - Dead zone (region where player moves without camera movement)
  - Look-ahead (camera leads in the direction of movement)
  - Speed-matching (camera accelerates to match player speed)
- [ ] Vertical tracking: How does the camera follow vertical movement?
  - Grounded lock (camera snaps to ground level when player lands)
  - Vertical dead zone (allows small jumps without camera movement)
  - Fall look-down (camera pans down during long falls)
- [ ] Zone-based overrides:
  - Boss arena lock (camera constrains to arena bounds)
  - Vertical section handling (camera rotates to portrait-style tracking)
  - Cinematic moments (camera pulls out for dramatic reveals)

**Technical Specification:**
- [ ] Camera viewport size in tiles (visible area)
- [ ] Camera smoothing algorithm (lerp factor or spring damping)
- [ ] Bounds clamping (camera cannot show outside level bounds)
- [ ] Screen shake parameters (intensity, duration, frequency for impacts)

**Reference Analysis:**
- [ ] Analyze camera systems in 3 classic side-scrollers
- [ ] Document what works and what doesn't about each
- [ ] Identify the best reference for this project

### Deliverables
```
camera_system_spec.md            (complete specification)
camera_parameters.csv            (all tunable values)
camera_behavior_diagram.png      (visual showing zones, dead areas, look-ahead)
reference_analysis.md            (classic game camera comparison)
```

### Success Criteria
- Camera never shows outside level bounds
- Player is always visible and centered enough for gameplay readability
- Transitions between behaviors are smooth (no jarring snaps)
- Boss arena lock works correctly with fixed camera bounds
- Screen shake enhances impact without causing disorientation

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Behavior Design | /30 | Comprehensive, well-specified |
| Technical Accuracy | /25 | Implementable parameters |
| Reference Analysis | /20 | Insightful comparison |
| Edge Case Handling | /15 | Bounds, transitions, zones |
| Documentation | /10 | Clear and complete |

**Pass Threshold**: >= 85/100

---

## Assessment 4.4: Power-Up & Combat System Design

### Objective
Design a complete power-up and combat system that integrates with the procedural level generation system.

### Requirements

**Player Combat:**
- [ ] Attack types: melee (primary), ranged (if applicable), special (if applicable)
- [ ] Attack frame data: startup frames, active frames, recovery frames
- [ ] Damage values per attack type
- [ ] Invincibility frames after taking damage (duration and visual feedback)
- [ ] Health system: starting HP, max HP, damage scaling

**Power-Up Design (minimum 6 types):**
- [ ] Health restoration (small, large)
- [ ] Temporary invincibility (duration, visual indicator)
- [ ] Attack boost (damage multiplier, duration)
- [ ] Speed boost (movement multiplier, duration)
- [ ] Shield (absorb N hits)
- [ ] Special ability (context-dependent, one per biome)

For each power-up:
- [ ] Visual and audio design (recognizable at a glance)
- [ ] Duration and magnitude
- [ ] Spawn rules (where does it appear in generated levels?)
- [ ] Interaction with difficulty system (harder levels have fewer power-ups)
- [ ] Stacking rules (can effects combine?)

**Integration with Generative System:**
- [ ] How does the level generator decide power-up placement?
- [ ] Placement rules: after hard sections, before bosses, in exploration rewards
- [ ] Density scaling: fewer power-ups at higher difficulty
- [ ] Deterministic placement: same seed = same power-up locations

### Deliverables
```
combat_system_spec.md            (player combat specification)
powerup_catalog.md               (all power-ups with full specs)
frame_data.csv                   (attack timing data)
placement_rules.md               (generative system integration)
```

### Success Criteria
- Combat feels responsive (attack startup < 4 frames at 60 fps)
- Power-ups are visually distinct and immediately understandable
- Placement rules integrate cleanly with the level generation pipeline
- Difficulty scaling affects power-up density appropriately
- No power-up combination creates game-breaking exploits

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Combat Design | /25 | Responsive, satisfying, fair |
| Power-Up Variety | /25 | Distinct, useful, balanced |
| Generative Integration | /25 | Clean rules for procedural placement |
| Balance | /15 | No exploits, fair scaling |
| Documentation | /10 | Complete, implementable |

**Pass Threshold**: >= 85/100

---

## Assessment 4.5: Hazard & Environment Design

### Objective
Design environmental hazards and interactive elements that work within procedurally generated levels.

### Requirements

**Hazard Types (minimum 6):**
- [ ] Static hazards: spikes, lava pits, thorns
- [ ] Moving hazards: swinging pendulums, falling rocks, rising lava
- [ ] Timed hazards: platforms that disappear/reappear on cycles
- [ ] Conditional hazards: ice (slippery), wind (pushes player), darkness (limited visibility)

For each hazard:
- [ ] Damage dealt or effect applied
- [ ] Visual and audio warning ("tell")
- [ ] Placement constraints for procedural generation
- [ ] Biome affinity (which hazards appear in which biomes)

**Interactive Elements:**
- [ ] Moving platforms (horizontal, vertical, circular paths)
- [ ] Breakable walls/floors (require attack to pass)
- [ ] Switches/levers (activate bridges, open gates)
- [ ] Conveyor belts (force player movement direction)

**Generative System Integration:**
- [ ] Placement rules: hazards must not create impossible paths
- [ ] Density scaling with difficulty
- [ ] Biome-specific hazard pools (forest: thorns, cavern: falling rocks, etc.)
- [ ] Validation: all hazards must be avoidable (no unavoidable damage)

### Deliverables
```
hazard_catalog.md                (all hazards with full specs)
interactive_elements_spec.md     (moving platforms, switches, etc.)
biome_hazard_mapping.csv         (biome -> available hazards)
placement_constraints.md         (rules for procedural generation)
```

### Success Criteria
- All hazards have readable visual tells
- No hazard creates an impossible path in generated levels
- Biome-specific hazards enhance thematic consistency
- Interactive elements add gameplay variety without complexity overload
- Hazard density scales appropriately with difficulty

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Hazard Variety | /20 | Mechanically distinct types |
| Visual Communication | /20 | Clear tells, readable at speed |
| Generative Integration | /25 | Clean placement rules |
| Biome Consistency | /20 | Thematically appropriate |
| Documentation | /15 | Complete specifications |

**Pass Threshold**: >= 85/100

---

## Module 4 Final Assessment: Capstone Project

### Objective
Produce a complete mechanics specification document that integrates player physics, enemies, camera, combat, power-ups, and hazards into a coherent design for the retro side-scroller.

### Requirements
- [ ] All five assessment areas integrated into one cohesive mechanics document
- [ ] Physics parameters are finalized and justified
- [ ] Enemy archetypes work with the generative level system
- [ ] Camera system handles all level types (horizontal, vertical, boss arena)
- [ ] Combat and power-ups are balanced across difficulty tiers
- [ ] Hazards integrate with biome system and procedural generation

### Evaluation Process
1. Self-assessment by learner
2. Peer review by 2 other learners
3. Gameplay prototype review (if available)
4. Design lead approval

**Pass Threshold**: Approved by design lead + both peers

---

## Module Completion Checklist

- [ ] Assessment 4.1: Player Physics & Jump Tuning (PASS)
- [ ] Assessment 4.2: Enemy Archetype Design (PASS)
- [ ] Assessment 4.3: Camera System Design (PASS)
- [ ] Assessment 4.4: Power-Up & Combat System Design (PASS)
- [ ] Assessment 4.5: Hazard & Environment Design (PASS)
- [ ] Capstone Project (APPROVED)
- [ ] Module 4 Knowledge Check (80%+ score)

**Module Status**: Ready to advance to Module 5

---

## Resources & References

### Essential Readings
- "Game Feel: A Game Developer's Guide to Virtual Sensation" - Steve Swink
- "Scroll Back: The Theory and Practice of Cameras in Side-Scrollers" - Itay Keren (GDC 2015)
- "Math for Game Programmers: Building a Better Jump" - GDC talk

### Reference Games (Mechanics Study)
- Mega Man X (jump feel, enemy design, boss patterns)
- Celeste (physics precision, coyote time, jump buffer)
- Hollow Knight (combat feel, enemy variety, camera work)
- Super Mario World (variable jump, power-ups, momentum)
- Castlevania IV (deliberate movement, whip combat, hazard design)

### Tools
- Desmos (jump arc visualization)
- Unity 2D Physics Debugger (collision visualization)
- Frame-by-frame video analysis of reference games
