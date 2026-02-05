# Module 6: retro Audio & Aesthetics - Assessment Criteria

## Module Overview
This module develops expertise in authentic retro visual and audio design, including pixel art animation, tilemap construction, parallax scrolling, chiptune composition, and sound effects design. Learners must demonstrate ability to create assets that are faithful to retro era constraints while meeting modern mobile quality standards.

---

## Learning Objectives

By completion, learners will be able to:
1. Create pixel art animations that adhere to retro hardware constraints
2. Design cohesive tilesets with efficient palette usage across biomes
3. Compose chiptune music with authentic waveform and channel constraints
4. Design sound effects that communicate gameplay information clearly
5. Implement parallax scrolling and visual layering for depth

---

## Assessment 6.1: Pixel Art Animation Production

### Objective
Produce a complete set of player character animations and two enemy animation sets that meet retro era authenticity standards.

### Requirements

**Player Character Animations:**
- [ ] Idle: 2-4 frames, subtle breathing/movement loop
- [ ] Walk: 6-8 frames, clear weight transfer and foot contact
- [ ] Run: 6-8 frames (if distinct from walk), more dynamic pose
- [ ] Jump startup: 2 frames (anticipation, launch)
- [ ] Jump apex: 1-2 frames (hang time pose)
- [ ] Fall: 2-3 frames (arms up, legs trailing)
- [ ] Land: 2 frames (impact, recovery)
- [ ] Attack: 3-5 frames (wind-up, active, recovery)
- [ ] Hurt: 2-3 frames (knockback reaction)
- [ ] Death: 4-6 frames (dramatic, clear game-over signal)

**Technical Constraints:**
- [ ] Sprite size: 32x32 pixels (or 16x32 for slim characters)
- [ ] Maximum 16 colors per sprite (from master palette of 256)
- [ ] No anti-aliasing (hard pixel edges only)
- [ ] No rotation or scaling (only frame-by-frame animation)
- [ ] Sub-pixel animation allowed only through careful color shifting

**Enemy Animation Sets (2 enemies):**
- [ ] Enemy 1 (simple): idle (2 frames), walk (4 frames), attack (3 frames), defeat (3 frames)
- [ ] Enemy 2 (complex): idle (2 frames), walk (6 frames), attack (4 frames), special (3 frames), defeat (4 frames)
- [ ] Each enemy visually distinct in silhouette from player and each other

**Quality Standards:**
- [ ] Animations read clearly at 1x scale (no zoom required)
- [ ] Consistent art style across all characters
- [ ] Clean pixel work (no stray pixels, consistent outlines)
- [ ] Timing feels natural at 60fps playback

### Deliverables
```
sprites/
  player_spritesheet.png         (all player animations on one sheet)
  enemy1_spritesheet.png         (enemy 1 animations)
  enemy2_spritesheet.png         (enemy 2 animations)
animation_spec.json              (frame timing, hitbox data per animation)
palette.png                      (master 256-color palette with used colors marked)
art_style_guide.md               (design rules, constraints, rationale)
```

### Success Criteria
- All animations loop smoothly with no visible pops or hitches
- Silhouettes are readable at 1x scale
- Palette usage stays within 16 colors per sprite
- Art style is consistent across all three characters
- Animations communicate gameplay state clearly (idle vs. attacking vs. hurt)

### Evaluation Rubric
| Criteria | Excellent (5) | Good (4) | Acceptable (3) | Needs Work (2) |
|----------|--------------|---------|----------------|----------------|
| Animation Quality | Smooth, natural, expressive | Good motion | Functional | Stiff or unclear |
| Technical Compliance | All constraints met | 1 minor violation | 2-3 violations | Multiple violations |
| Visual Consistency | Unified art style | Minor inconsistencies | Some style drift | Inconsistent |
| Readability | Crystal clear at 1x | Clear | Adequate | Squint required |
| Palette Efficiency | Masterful use of 16 colors | Good use | Adequate | Wasteful |

**Pass Threshold**: Average >= 4.0/5.0

---

## Assessment 6.2: Tileset & Biome Design

### Objective
Design complete tilesets for two biomes that tile seamlessly, support level generation, and maintain retro authenticity.

### Requirements

**Biome 1: Forest**
- [ ] Ground tiles: grass top, dirt fill, grass edge variants (left, right, inner corner, outer corner)
- [ ] Platform tiles: wooden platform (left cap, middle, right cap)
- [ ] Background elements: trees (2 variants), bushes, flowers, rocks
- [ ] Hazard tiles: spike, thorn vine
- [ ] Decorative tiles: sunlight rays, fallen leaves, mushrooms
- [ ] Minimum 24 unique tiles

**Biome 2: Cavern**
- [ ] Ground tiles: stone top, stone fill, stone edge variants
- [ ] Platform tiles: rock ledge (left cap, middle, right cap)
- [ ] Background elements: stalactites, stalagmites, crystals, cave moss
- [ ] Hazard tiles: lava surface, dripping acid
- [ ] Decorative tiles: glowing mushrooms, bat silhouettes, cave pools
- [ ] Minimum 24 unique tiles

**Technical Constraints:**
- [ ] Tile size: 8x8 pixels (retro standard)
- [ ] Maximum 32 colors per biome tileset (from master 256-color palette)
- [ ] All ground/platform tiles must tile seamlessly (no visible seams)
- [ ] Edge tiles must connect properly in all configurations (auto-tiling rules)
- [ ] Background tiles use a reduced palette (darker/desaturated subset)

**Auto-Tiling Rules:**
- [ ] Define which tile connects to which neighbors (bitmask approach)
- [ ] Document auto-tile rules so the level generator can select correct tiles
- [ ] Test tiling with at least 5 different random level layouts

### Deliverables
```
tilesets/
  forest_tileset.png             (all forest tiles on one sheet)
  cavern_tileset.png             (all cavern tiles on one sheet)
  forest_palette.json            (32-color palette with hex codes)
  cavern_palette.json            (32-color palette with hex codes)
autotile_rules.md                (bitmask definitions, neighbor rules)
tileset_guide.md                 (which tile is which, usage notes)
tiling_test_screenshots/         (5 level layouts per biome)
```

### Success Criteria
- Tiles connect seamlessly in all tested configurations
- Biomes are visually distinct (immediately recognizable)
- Palette stays within 32-color limit per biome
- Auto-tiling rules are complete and unambiguous
- Background tiles create depth without competing with foreground

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Tile Variety | /20 | Sufficient variety for generated levels |
| Seamless Tiling | /25 | No visible seams in any configuration |
| Biome Identity | /20 | Visually distinct, thematically clear |
| Palette Efficiency | /15 | Within constraints, well-chosen colors |
| Auto-Tile Documentation | /20 | Complete rules for generator integration |

**Pass Threshold**: >= 85/100

---

## Assessment 6.3: Chiptune Music Composition

### Objective
Compose two complete music tracks (one level theme, one boss theme) using authentic retro audio constraints.

### Requirements

**Track 1: Level Theme (Forest biome)**
- [ ] Duration: 45-90 seconds before loop
- [ ] Loop point: seamless (no audible gap or click)
- [ ] Channel limit: maximum 8 simultaneous channels (SNES standard)
- [ ] Waveforms: square, sawtooth, triangle, noise (drum/percussion)
- [ ] Tempo: 120-140 BPM (energetic but not frantic)
- [ ] Mood: adventurous, upbeat, exploration-friendly
- [ ] Memorable melody that players will recognize and hum

**Track 2: Boss Theme**
- [ ] Duration: 60-120 seconds before loop
- [ ] Loop point: seamless
- [ ] Channel limit: maximum 8 simultaneous channels
- [ ] Tempo: 140-170 BPM (intense, urgent)
- [ ] Mood: threatening, dramatic, escalating tension
- [ ] Distinct from level theme (different key, tempo, rhythm)
- [ ] Builds intensity across the loop

**Technical Constraints:**
- [ ] Sample rate: 32 kHz (SNES standard) for composition reference
- [ ] Final output: high-quality .ogg for game integration
- [ ] File size: < 500 KB per track (compressed)
- [ ] No modern synthesizer sounds (stick to chip waveforms)
- [ ] Percussion must use noise channel or short samples (no full drum kit)

**Composition Analysis:**
- [ ] Document the musical structure of each track (intro, verse, chorus, bridge)
- [ ] Explain instrument assignments (which channel plays what)
- [ ] Describe how the music enhances gameplay mood
- [ ] Reference 2 classic retro tracks that inspired the composition

### Deliverables
```
audio/
  forest_theme.ogg               (level music)
  boss_theme.ogg                 (boss music)
  forest_theme_sheet.pdf         (notation or tracker screenshot)
  boss_theme_sheet.pdf           (notation or tracker screenshot)
composition_notes.md             (structure, instruments, inspiration)
channel_assignment.md            (which channel plays what, polyphony plan)
```

### Success Criteria
- Both tracks loop seamlessly (no audible gap)
- Channel count stays within 8-channel limit
- Music sounds authentically retro (not modern chiptune with reverb/effects)
- Level theme and boss theme are clearly distinct in mood
- Tracks enhance gameplay rather than distract from it

### Evaluation Rubric
| Criteria | Excellent (5) | Good (4) | Acceptable (3) | Needs Work (2) |
|----------|--------------|---------|----------------|----------------|
| Musical Quality | Memorable, professional | Good composition | Adequate | Unpolished |
| Authenticity | Perfectly retro | Very authentic | Mostly authentic | Modern artifacts |
| Looping | Seamless, invisible | Minor gap | Noticeable seam | Obvious loop point |
| Mood Matching | Perfect for context | Good match | Acceptable | Doesn't fit |
| Technical Compliance | All constraints met | 1 minor violation | 2 violations | Multiple violations |

**Pass Threshold**: Average >= 4.0/5.0

---

## Assessment 6.4: Sound Effects Design

### Objective
Design a complete sound effects library for all gameplay actions, UI interactions, and ambient events.

### Requirements

**Gameplay SFX (15 required):**
- [ ] Player actions:
  1. Jump (short, upward tone)
  2. Land (impact thud)
  3. Attack (slash/swing)
  4. Attack hit (enemy contact)
  5. Player hurt (damage taken)
  6. Player death (dramatic, final)
- [ ] Enemy actions:
  7. Enemy attack (distinct from player attack)
  8. Enemy defeat (satisfying pop/explosion)
  9. Boss roar (intro to boss fight)
  10. Boss phase transition (power-up/warning)
- [ ] Pickups and events:
  11. Health pickup (positive, healing tone)
  12. Power-up collect (dramatic, empowering)
  13. Checkpoint reached (reassuring chime)
  14. Level complete (triumphant fanfare, 2-3 seconds)
  15. Game over (somber, brief)

**UI SFX (5 required):**
- [ ] Menu select (cursor movement)
- [ ] Menu confirm (button press)
- [ ] Menu cancel / back
- [ ] Screen transition (whoosh or wipe)
- [ ] Error / invalid action (subtle negative tone)

**Technical Constraints:**
- [ ] Duration: 0.1-3.0 seconds per effect (level complete can be up to 5 seconds)
- [ ] Waveforms: chip-style only (square, saw, triangle, noise)
- [ ] No reverb, delay, or modern effects processing
- [ ] File format: .wav (uncompressed) for editing, .ogg (compressed) for game
- [ ] Each SFX must be distinct and identifiable with eyes closed
- [ ] SFX must not mask each other when played simultaneously

**Layering Plan:**
- [ ] Define priority system: which SFX plays if multiple trigger at once?
- [ ] Maximum simultaneous SFX: 4
- [ ] Document which sounds can overlap and which interrupt each other

### Deliverables
```
sfx/
  gameplay/
    sfx_jump.wav
    sfx_land.wav
    sfx_attack.wav
    ... (all 15 gameplay SFX)
  ui/
    sfx_menu_select.wav
    sfx_menu_confirm.wav
    ... (all 5 UI SFX)
sfx_spec.md                      (duration, waveform, frequency range per SFX)
priority_system.md               (layering rules, simultaneous limits)
sfx_integration_guide.md         (which game event triggers which SFX)
```

### Success Criteria
- All 20 SFX are complete and functional
- Each SFX is identifiable without visual context
- No two SFX sound confusingly similar
- SFX enhance gameplay feedback without being annoying on repetition
- Layering plan prevents audio mudding when multiple SFX fire

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Sound Quality | /25 | Clean, professional chip sounds |
| Gameplay Communication | /25 | Each SFX clearly communicates its event |
| Authenticity | /20 | Genuinely retro |
| Layering Design | /15 | No mudding, clear priorities |
| Documentation | /15 | Complete specs and integration guide |

**Pass Threshold**: >= 85/100

---

## Assessment 6.5: Parallax Scrolling & Visual Layering

### Objective
Design a complete parallax scrolling system with multiple depth layers that creates a convincing sense of depth using retro techniques.

### Requirements

**Layer Definition (minimum 5 layers):**
- [ ] Layer 1 (farthest): Sky / gradient background -- no scrolling or very slow (0.1x)
- [ ] Layer 2: Distant features (mountains, clouds) -- slow scroll (0.2x-0.3x)
- [ ] Layer 3: Mid-ground features (distant trees, buildings) -- medium scroll (0.4x-0.6x)
- [ ] Layer 4: Near-ground features (close vegetation, rocks) -- fast scroll (0.7x-0.8x)
- [ ] Layer 5 (closest): Foreground / gameplay layer -- 1.0x scroll (camera-locked)
- [ ] Optional Layer 6: Foreground overlay (closest leaves, fog, rain) -- > 1.0x scroll

**Per-Biome Parallax Design:**
- [ ] Forest: sky -> distant hills -> far trees -> near trees -> gameplay -> leaf overlay
- [ ] Cavern: dark gradient -> stalactites -> crystal glow -> rock pillars -> gameplay

**Technical Specification:**
- [ ] Scroll speed per layer (as multiplier of camera movement)
- [ ] Texture width per layer (must tile seamlessly for infinite scroll)
- [ ] Color palette per layer (far layers use desaturated/darker colors for depth)
- [ ] Memory budget per layer (total parallax textures < 1 MB per biome)
- [ ] Reduced motion mode: collapse to 2 layers (background + gameplay)

**Visual Quality:**
- [ ] No visible texture seams during scrolling
- [ ] Consistent art style between layers
- [ ] Depth creates atmosphere without distracting from gameplay
- [ ] Player and enemies are always visually distinct from background

### Deliverables
```
parallax/
  forest/
    layer1_sky.png
    layer2_hills.png
    layer3_far_trees.png
    layer4_near_trees.png
    layer5_foreground.png         (optional overlay)
  cavern/
    layer1_dark.png
    layer2_stalactites.png
    layer3_crystals.png
    layer4_pillars.png
parallax_spec.md                 (scroll speeds, dimensions, memory budget)
layer_palette_comparison.png     (color shift per depth layer)
reduced_motion_spec.md           (simplified version for accessibility)
```

### Success Criteria
- All layers tile seamlessly during horizontal scrolling
- Depth effect is convincing and atmospheric
- Player/enemy sprites are never confused with background elements
- Memory budget stays within 1 MB per biome
- Reduced motion mode is functional and still aesthetically acceptable

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Depth Effect | /25 | Convincing sense of depth |
| Art Quality | /25 | Professional pixel art per layer |
| Technical Compliance | /20 | Seamless tiling, memory budget |
| Biome Consistency | /15 | Layers fit biome theme |
| Accessibility | /15 | Reduced motion mode works |

**Pass Threshold**: >= 85/100

---

## Module 6 Final Assessment: Capstone Project

### Objective
Produce a complete audio-visual asset package for one biome (Forest) that includes all sprites, tiles, parallax layers, music, and sound effects.

### Requirements
- [ ] Player character: all animations (10+ animation states)
- [ ] 2 enemy types: all animations
- [ ] Complete tileset: 24+ tiles with auto-tile rules
- [ ] Parallax background: 5 layers with scroll speeds
- [ ] Level theme: 45-90 second looping track
- [ ] Boss theme: 60-120 second looping track
- [ ] All 20 SFX (15 gameplay + 5 UI)
- [ ] Asset integration guide for Unity import

### Evaluation Process
1. Self-assessment by learner
2. Art review by pixel art specialist (if available) or peer
3. Audio review by composition reviewer or peer
4. Integration test: assets load correctly in Unity test scene
5. Design lead approval

**Pass Threshold**: Approved by design lead + art/audio reviewer

---

## Module Completion Checklist

- [ ] Assessment 6.1: Pixel Art Animation Production (PASS)
- [ ] Assessment 6.2: Tileset & Biome Design (PASS)
- [ ] Assessment 6.3: Chiptune Music Composition (PASS)
- [ ] Assessment 6.4: Sound Effects Design (PASS)
- [ ] Assessment 6.5: Parallax Scrolling & Visual Layering (PASS)
- [ ] Capstone Project (APPROVED)
- [ ] Module 6 Knowledge Check (80%+ score)

**Module Status**: Ready to advance to implementation phase

---

## Resources & References

### Essential Readings
- "Pixel Logic: A Guide to Pixel Art" - Michael Azzi
- "Pixel Art for Game Developers" - Daniel Silber
- "Composing Music for Games" - Chance Thomas (chiptune chapters)

### Reference Games (Art & Audio Study)
- Shovel Knight (modern pixel art that respects NES constraints)
- Owlboy (parallax mastery, pixel art animation)
- Mega Man X (sprite animation, tileset design)
- Sonic the Hedgehog 2 (parallax scrolling, zone-based art)
- Castlevania: Symphony of the Night (sprite detail, atmospheric art)

### Pixel Art Tools
- Aseprite (industry standard pixel art editor)
- GraphicsGale (free alternative)
- Pyxel Edit (tilemap-focused pixel art)
- TexturePacker (sprite sheet packing)

### Audio Tools
- FamiTracker (NES chiptune -- good for learning constraints)
- DefleMask (multi-platform chiptune tracker)
- LMMS (free DAW with chip synths)
- Audacity (SFX editing)
- FMOD Studio (game audio integration)

### Color Resources
- Lospec Palette List: https://lospec.com/palette-list
- SNES palette reference: 15-bit color (32,768 colors, 256 on-screen)
- Colour Contrast Analyser for accessibility testing
