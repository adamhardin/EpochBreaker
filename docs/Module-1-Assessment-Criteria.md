# Module 1: retro Design Fundamentals - Assessment Criteria

## Module Overview
This module trains deep expertise in retro era game design, pixel art theory, classic side-scroller mechanics, and authentic audio design. Learners must develop competency in recognizing and applying retro design constraints and patterns.

---

## Learning Objectives

By completion, learners will be able to:
1. Identify authentic retro design patterns vs. modern imitations
2. Analyze and critique level design for flow, pacing, and difficulty curves
3. Design sprite sheets and animations within retro technical constraints
4. Compose and critique chiptune audio and sound effects
5. Understand palette limitations and color theory applications

---

## Assessment 1.1: Sprite Sheet Design

### Objective
Create an authentic retro sprite sheet for the player character that adheres to era-appropriate constraints.

### Requirements
**Technical Constraints:**
- Maximum 256-color palette (retro standard)
- Sprite size: 32x32 pixels (typical for side-scrollers)
- Animation frames: 6-8 per action (walk, jump, fall, attack, hurt)
- Tile-based design (8x8 pixel tiles)

**Design Specifications:**
- [ ] Walk cycle: 6 frames, smooth anticipation and follow-through
- [ ] Jump startup and landing (2 frames each)
- [ ] Fall animation (idle loop during descent)
- [ ] Attack pose (1-2 frame)
- [ ] Hurt reaction (1-2 frames)

### Success Criteria
- ✅ All sprites fit within 32x32 boundary
- ✅ Animation loops smoothly with no pops
- ✅ Palette usage optimized (< 16 colors per pose)
- ✅ Clear silhouette readable at small scale
- ✅ Authentic retro visual style (no anti-aliasing, clean pixels)
- ✅ Frame-by-frame documentation with timing notes

### Deliverables
```
sprite_sheet_player.png          (256x256 px, 32x32 per sprite)
sprite_sheet_player_spec.json    (animation timing and frame data)
design_notes.md                  (design rationale and constraints met)
```

### Evaluation Rubric
| Criteria | Excellent (5) | Good (4) | Acceptable (3) | Needs Work (2) | Poor (1) |
|----------|--------------|---------|----------------|----------------|---------|
| Technical Compliance | All constraints met perfectly | 1 minor constraint violation | 2 violations | 3+ violations | Major violations |
| Animation Quality | Smooth, natural flow | Minor hitch | Acceptable movement | Stiff or unnatural | Poor quality |
| Aesthetic Authenticity | Perfectly retro | Very authentic | Mostly authentic | Some modern artifacts | Doesn't match era |
| Color Optimization | Excellent use of 16 colors | Good optimization | Adequate use | Wasteful palette | Poor management |
| Documentation | Comprehensive notes | Clear documentation | Basic notes | Minimal notes | Missing |

**Pass Threshold**: Score >= 4.0/5.0

---

## Assessment 1.2: Level Design Critique & Reconstruction

### Objective
Analyze three classic retro side-scroller levels and reconstruct one based on your analysis.

### Part A: Analysis (40 minutes)
Select one level from each game:
- **Mega Man X** (Level 1 or 2)
- **Castlevania IV** (Stage 1 or 2)
- **Contra** (Stage 1 or 2)

**For each level, document:**
1. **Flow Analysis**
   - Average platform spacing (tiles)
   - Difficulty progression curve (graph)
   - Player skill assumptions at each checkpoint
   - Pacing (safe zones vs. challenges)

2. **Design Patterns Identified**
   - Enemy placement strategy
   - Reward distribution
   - Tutorial/difficulty ramp progression
   - Secret room locations

3. **Critical Assessment**
   - Why does this level work?
   - Where does difficulty spike?
   - How does audio enhance the experience?
   - What makes replaying it satisfying?

**Deliverable**: 3-4 page analysis document per level

### Part B: Reconstruction (60 minutes)
Choose one of the three analyzed levels and recreate it:

**Requirements:**
- [ ] Reproduce level layout to 90%+ accuracy
- [ ] Maintain original enemy placement and behavior
- [ ] Preserve difficulty pacing and learning curve
- [ ] Use pixel-perfect tilemap recreation
- [ ] Document any deviations and why

**Deliverable**: Tilemap data file + visual screenshot + rationale document

### Success Criteria
**Analysis Section:**
- ✅ Accurate difficulty curve measurement
- ✅ Identifies 3+ distinct design patterns
- ✅ Clear explanation of why level works
- ✅ References specific mechanical interactions

**Reconstruction Section:**
- ✅ Layout accurate to 90%+ of original
- ✅ Enemy positions replicate original strategy
- ✅ Maintains learning curve
- ✅ All critical challenges preserved

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Analysis Depth | /25 | Identifies patterns and design intent |
| Reconstruction Accuracy | /25 | Layout and enemy placement fidelity |
| Critical Thinking | /25 | Why does level work? |
| Documentation | /25 | Clear, detailed write-up |

**Pass Threshold**: >= 80/100

---

## Assessment 1.3: Palette Theory & Color Constraint Challenge

### Objective
Demonstrate mastery of retro palette constraints and color theory by designing a cohesive tileset within strict limits.

### Challenge Parameters
**Constraints:**
- Maximum 32 colors (retro era standard for tileset)
- Tileset size: 8x8 pixels per tile
- Minimum 16 unique tiles (terrain types)
- Must support parallax background layer
- Must be distinct from other provided tilesets

**Design Requirements:**
- [ ] Create primary terrain tiles (grass, dirt, stone, water)
- [ ] Design hazard tiles (spikes, lava, ice)
- [ ] Create decorative elements (trees, rocks, structures)
- [ ] Background parallax layer (reduced detail, same colors)
- [ ] Documentation of palette and color reasoning

### Success Criteria
- ✅ All tiles fit within 8x8 pixel grid
- ✅ Palette usage exactly 32 colors (no wasted slots)
- ✅ Tiles are clearly distinguishable
- ✅ Color transitions smooth and natural
- ✅ Cohesive visual aesthetic
- ✅ Reusable for multiple levels (good variety)

### Deliverables
```
tileset_primary.png          (128x128 px, 8x8 per tile)
tileset_background.png       (128x128 px, parallax version)
palette_definition.json      (32-color palette with hex codes)
tileset_guide.md             (which tile is which, design notes)
```

### Evaluation Rubric
| Criteria | Excellent (5) | Good (4) | Acceptable (3) | Needs Work (2) |
|----------|--------------|---------|----------------|----------------|
| Palette Optimization | Perfect use of all 32 colors | Excellent use, 1-2 unused | Good use, 3-4 unused | Wasteful or over-palette |
| Visual Coherence | Stunning, unified style | Very cohesive | Acceptable cohesion | Inconsistent style |
| Tile Variety | Rich, reusable set | Good variety | Adequate variety | Limited options |
| Authenticity | Perfectly retro | Very authentic | Mostly authentic | Some modern look |
| Documentation | Comprehensive | Clear | Adequate | Minimal |

**Pass Threshold**: Average >= 4.0/5.0

---

## Assessment 1.4: Chiptune Analysis & Sound Design

### Objective
Understand chiptune composition constraints and design a cohesive audio landscape for a single level.

### Part A: Chiptune Analysis (30 minutes)
Analyze three compositions:
1. **Mega Man X** - "Storm Eagle Stage"
2. **Castlevania IV** - "Entry of the Darkness"
3. **Sonic the Hedgehog 2** - "Emerald Hill Zone"

**For each, document:**
- Instrumentation (square waves, sawtooth, drum pattern)
- Tempo and time signature
- Musical structure (intro, loop, variations)
- Leitmotif or memorable melody
- Sound effect integration

### Part B: SFX Design (45 minutes)
Design sound effects for 10 game actions:
1. Player jump
2. Player land
3. Enemy hit
4. Player damage taken
5. Enemy defeat
6. Health pickup
7. Damage boost pickup
8. Boss roar
9. Level complete
10. Game over

**Constraints:**
- 8-bit or retro chip sounds only
- 2-4 second maximum per effect
- Must be immediately identifiable
- Non-intrusive (don't mask other sounds)

**Deliverables:**
- Audio files (.wav) for each SFX
- Specification document (frequency, duration, wave type)
- Level design document showing SFX integration points

### Success Criteria
- ✅ All SFX are distinct and identifiable
- ✅ Consistent audio aesthetic
- ✅ Professional sound quality
- ✅ SFX enhance gameplay without annoying player
- ✅ Proper documentation of technical specs

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Chiptune Analysis | /25 | Identifies composition techniques |
| SFX Design Quality | /40 | Sound quality and creativity |
| Authenticity | /20 | Sounds genuinely retro |
| Documentation | /15 | Technical specs and rationale |

**Pass Threshold**: >= 80/100

---

## Assessment 1.5: Design Pattern Recognition Challenge

### Objective
Demonstrate ability to identify and apply retro design patterns across multiple classic games.

### Challenge
**Part 1: Pattern Identification (30 minutes)**
Given screenshots and level data from 5 different retro side-scrollers:
- Identify the design pattern (e.g., "tutorial ramp," "skill gate," "environmental challenge")
- Explain the mechanical intent
- Estimate skill level requirement
- Predict player difficulty rating

**Part 2: Pattern Application (45 minutes)**
Design a level that successfully combines:
- [ ] Tutorial pattern (teach one mechanic)
- [ ] Skill gate pattern (test learned mechanic)
- [ ] Environmental challenge (platform-based difficulty)
- [ ] Enemy gauntlet pattern (combat challenge)
- [ ] Boss arena pattern (climactic encounter)

All within a 2-3 minute playtime window.

### Success Criteria
- ✅ Identifies 4+ distinct patterns
- ✅ Accurate difficulty rating predictions
- ✅ Designed level includes all 5 pattern types
- ✅ Pacing is smooth and engaging
- ✅ Level is playable and fun

### Deliverables
```
pattern_analysis.md          (identification of patterns)
level_design_document.md     (designed level with specs)
level_layout.tilemap         (actual level data)
difficulty_curve.png         (graph of difficulty progression)
```

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Pattern Recognition Accuracy | /25 | Correct identification |
| Difficulty Prediction | /25 | Within ±0.5 of actual |
| Level Design Quality | /30 | Engaging, balanced level |
| Documentation | /20 | Clear explanation |

**Pass Threshold**: >= 80/100

---

## Module 1 Final Assessment: Capstone Project

### Objective
Synthesize all Module 1 learnings into a complete mini-game level.

### Requirements
**Deliverables:**
1. **Sprite sheet** for player character (all animations)
2. **Tileset** (minimum 16 unique tiles, optimized palette)
3. **Complete level design** (difficulty ramp, enemy placement, rewards)
4. **Audio design** (chiptune loop, 5+ SFX)
5. **Design document** (3-5 pages explaining all design choices)

**Technical Specs:**
- 60-90 seconds of gameplay
- Minimum 2 enemy types
- Minimum 1 boss encounter
- Authentic retro aesthetic
- Smooth difficulty progression

### Success Criteria
All components must meet quality standards:
- Sprite sheet: >= 4.0/5.0
- Tileset: >= 4.0/5.0
- Level design: >= 80/100
- Audio: >= 80/100
- Documentation: Comprehensive and clear

### Evaluation Process
1. **Self-assessment** by learner
2. **Peer review** by 2 other learners
3. **Design lead review** for authenticity and quality
4. **Final approval** for progression to Module 2

**Pass Threshold**: Approved by design lead + both peers

---

## Module Completion Checklist

- [ ] Assessment 1.1: Sprite Sheet Design (PASS)
- [ ] Assessment 1.2: Level Design Critique & Reconstruction (PASS)
- [ ] Assessment 1.3: Palette Theory Challenge (PASS)
- [ ] Assessment 1.4: Chiptune Analysis & SFX Design (PASS)
- [ ] Assessment 1.5: Design Pattern Recognition (PASS)
- [ ] Capstone Project (APPROVED)
- [ ] Module 1 Knowledge Check (80%+ score)

**Module Status**: Ready to advance to Module 2

---

## Resources & References

### Essential Readings
- "Pixel Art: The Manual of Spriting" - Pixel Pete
- "Game Feel: A Game Programmer's Guide" - Steve Swink
- "Game Engine Architecture" - Jason Gregory (Chapters 1-3)

### Reference Games
- Mega Man X (SNES)
- Castlevania IV (SNES)
- Contra (NES)
- Sonic the Hedgehog 2 (Genesis)
- Kirby Super Star (SNES)

### Tools
- Aseprite (sprite animation)
- PICO-8 (palette and constraint reference)
- FamiTracker (chiptune composition)
- Audacity (audio editing)

### Online Resources
- SpriteMate: https://www.spritemate.com/
- Lospec: https://lospec.com/ (palette reference)
- Game Design Document Templates
