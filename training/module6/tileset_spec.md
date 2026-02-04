# Era-Based Tileset & Visual Theme Specification

## Overview

Complete tileset specifications for a 16-bit side-scrolling mobile shooter set across 10 eras of human civilization. Each era defines 28+ unique 8x8 pixel tiles with collision data, destructible environment support, auto-tiling bitmask rules, and a dedicated 32-color palette. All tiles conform to 16-bit era constraints: no anti-aliasing, hard pixel edges, and seamless tiling in all valid configurations. Destructible environments are a core gameplay mechanic -- walls, rocks, terrain, and structures can be blasted apart by player weapons.

Four representative eras are defined in full detail: Stone Age (Era 1), Medieval (Era 4), Modern (Era 7), and Transcendent (Era 10). The remaining six eras include palette guidelines and destructible material descriptions.

---

## Technical Constraints

| Parameter | Value |
|-----------|-------|
| Tile Size | 8x8 pixels |
| Colors Per Era Tileset | 32 maximum (from master 256-color palette) |
| Anti-Aliasing | Prohibited |
| Tile Sheet Format | PNG, power-of-two dimensions |
| Tile Sheet Size | 256x128 (32 columns x 16 rows = 512 tile slots) |
| Filter Mode (Unity) | Point (no interpolation) |
| Pixels Per Unit (Unity) | 8 |
| Collision Shapes | Rectangle only (aligned to pixel grid) |
| Target Platform | iOS (iPhone / iPad) |
| Max Eras Loaded | 1 era tileset resident in memory at a time |

### Collision Types

| Type | Code | Behavior |
|------|------|----------|
| SOLID | S | Full 8x8 collision. Player, enemies, and projectiles cannot pass through. |
| PLATFORM | P | Solid from above only. Player can jump through from below and land on top. 1px thick collision at top edge. Projectiles pass through freely. |
| HAZARD | H | Triggers damage on contact. No physical collision (player passes through but takes damage). Projectiles pass through. |
| NONE | N | No collision. Purely decorative or background. |
| SLOPE_L | SL | Left-ascending slope. Collision follows diagonal from bottom-left to top-right. |
| SLOPE_R | SR | Right-ascending slope. Collision follows diagonal from bottom-right to top-left. |
| DESTRUCTIBLE_SOFT | DS | Breakable tile, 1 HP. Destroyed by any weapon in a single hit. Produces small particle debris on destruction. |
| DESTRUCTIBLE_MEDIUM | DM | Breakable tile, 3 HP. Requires multiple hits or a heavy weapon. Shows progressive damage states. Produces medium debris cloud. |
| DESTRUCTIBLE_HARD | DH | Breakable tile, 5 HP. Resistant to light weapons. Requires explosives or sustained fire. Shows three damage states. Produces large debris and screen shake on destruction. |

### Destructible Tile Properties

| Property | DS (Soft) | DM (Medium) | DH (Hard) |
|----------|-----------|-------------|-----------|
| Hit Points | 1 | 3 | 5 |
| Visual Damage States | 1 (intact only, instant destroy) | 3 (intact, damaged, critical) | 3 (intact, damaged, critical) |
| Light Weapon Damage | 1 per hit | 1 per hit | 0 per hit (immune) |
| Heavy Weapon Damage | 1 per hit (overkill) | 2 per hit | 1 per hit |
| Explosive Damage | 1 per hit (overkill) | 3 per hit (one-shot) | 3 per hit |
| Debris Particle Count | 3-5 particles | 6-10 particles | 12-18 particles |
| Screen Shake on Destroy | None | Micro (1px, 100ms) | Heavy (3px, 250ms) |

---

## Era 1: Stone Age Tileset

### Stone Age Palette (32 Colors)

**Foreground Tiles (Slots 0-15):**

| Slot | Name | Hex Code | Usage |
|------|------|----------|-------|
| 0 | Transparent | N/A | Background key |
| 1 | Outline Dark | #1A1008 | Tile outlines, deepest shadow |
| 2 | Cave Rock Light | #A89070 | Rock surface highlight |
| 3 | Cave Rock Mid | #8B7355 | Rock main surface |
| 4 | Cave Rock Dark | #6B5540 | Rock shadow |
| 5 | Cave Rock Deep | #4D3B28 | Interior rock, deep fill |
| 6 | Earth Light | #C4A882 | Sandy ground highlight |
| 7 | Earth Mid | #9E8462 | Packed earth, main fill |
| 8 | Earth Dark | #705A3A | Earth shadow, subsoil |
| 9 | Earth Darkest | #3E2E18 | Deep underground |
| 10 | Stalactite Highlight | #B8A890 | Stalactite/stalagmite lit edge |
| 11 | Stalactite Shadow | #5C4A38 | Stalactite dark face |
| 12 | Firelight Bright | #FF9930 | Fire core, warmest glow |
| 13 | Firelight Mid | #CC6A10 | Fire body |
| 14 | Ember Red | #AA3800 | Embers, hot coals |
| 15 | Hazard Bone White | #E8DCC8 | Bone spikes, sharp edges |

**Background/Decorative Tiles (Slots 16-31):**

| Slot | Name | Hex Code | Usage |
|------|------|----------|-------|
| 16 | BG Cave Deepest | #0E0A06 | Deepest cavern void |
| 17 | BG Cave Deep | #1C1408 | Deep background |
| 18 | BG Cave Mid | #2E2418 | Mid-depth background |
| 19 | BG Cave Near | #40342A | Near background detail |
| 20 | Painting Red Ochre | #B03020 | Cave painting pigment |
| 21 | Painting Yellow Ochre | #C89830 | Cave painting pigment |
| 22 | Painting Charcoal | #2A2018 | Cave painting dark lines |
| 23 | Painting White Chalk | #D8D0C0 | Cave painting highlights |
| 24 | Bone Aged | #C8B898 | Bone decorations, fossils |
| 25 | Bone Shadow | #8A7A60 | Bone dark side |
| 26 | Moss Green | #4A6830 | Damp cave moss |
| 27 | Moss Light | #688840 | Moss highlight |
| 28 | Water Pool Dark | #2A4050 | Underground water, deep |
| 29 | Water Pool Light | #3A6878 | Underground water, surface |
| 30 | Fire Glow Ambient | #4A2800 | Warm glow cast on nearby surfaces |
| 31 | Smoke Grey | #605848 | Smoke wisps from fires |

---

### Stone Age Tile Definitions (32 Tiles)

#### Ground Tiles (IDs 0-10)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| SA00 | cave_floor_flat | S | Flat cave floor. Top 2 rows: irregular rocky surface with 1-2px bumps and chips (slots 2,3). Bottom 6 rows: packed earth fill (slots 7,8,9). Scattered pebble detail using slot 4 pixels. Seamless horizontal tiling. |
| SA01 | cave_floor_left_edge | S | Left edge of cave floor. Top-left drops away in a rough diagonal break (jagged, not smooth). Left 2 columns are transparent/air. Crumbled stone detail at edge (1-2 stray pixels, slot 4). Connects to SA00 on right. |
| SA02 | cave_floor_right_edge | S | Right edge of cave floor. Asymmetric mirror of SA01 with different crack pattern. Right 2 columns are air. Connects to SA00 on left. |
| SA03 | earth_fill | S | Solid packed earth interior. Fill with slots 7,8,9 in a compacted sediment pattern. Occasional embedded pebble (1px slot 3). Subtle horizontal banding suggests strata layers. Tiles seamlessly in all directions. |
| SA04 | cave_ceiling | S | Cave ceiling. Top 6 rows: solid rock fill (slots 3,4,5). Bottom 2 rows: rough downward-facing surface with small stalactite nubs (1-2px, slots 10,11). |
| SA05 | cave_ceiling_stalactite | S | Ceiling with prominent stalactite. Rock fill on top 3 rows. Stalactite hangs 5px down, tapers from 3px wide at ceiling to 1px at tip. Slots 10,11,3 with moisture highlight on left face (1px slot 2). |
| SA06 | cave_left_inner_corner | S | Inner corner, left. Where ceiling meets left wall. Erosion-smoothed 2px radius corner. Moisture stain detail with 1px moss (slot 26). |
| SA07 | cave_right_inner_corner | S | Inner corner, right. Mirror concept of SA06 with different moss placement and unique crack detail. |
| SA08 | cave_left_wall | S | Left-facing vertical wall. Left 2 columns are exposed rock face (slots 2,3,4). Vertical crack lines (1px slot 1). Remaining columns solid fill (slots 5,8). |
| SA09 | cave_right_wall | S | Right-facing vertical wall. Asymmetric mirror of SA08 with unique crack pattern and possible embedded fossil detail (1px slot 24). |
| SA10 | rocky_ground_slope_left | SL | Left-ascending slope. Rocky surface follows diagonal from bottom-left to top-right. Below diagonal: earth fill. Above: transparent. Surface texture is rougher than flat ground -- jagged 1px variation along slope line. |

#### Destructible Tiles (IDs 11-16)

| ID | Name | Collision | HP | Description |
|----|------|-----------|-----|-------------|
| SA11 | loose_rock | DS | 1 | Loose rock pile. Cluster of 4-5 rounded stones (slots 2,3,4) loosely stacked. Visible gaps between stones (1px slot 9 or transparent). Crumbles instantly when hit. Debris: small stone chunks fly outward. |
| SA12 | loose_rock_damaged | -- | -- | *Not applicable: soft destructibles have no damaged state. They break on first hit.* |
| SA13 | stone_wall | DM | 3 | Solid hewn stone wall. Dense rock fill (slots 3,4,5) with primitive chisel marks (1px diagonal scratches in slot 1). Mortar-like sediment between irregular block shapes (slot 8). Feels deliberately stacked by early humans. |
| SA14 | stone_wall_damaged | -- | -- | Damage state for SA13. Same base as SA13 but with 2-3 crack lines radiating from center (slot 1). One corner chunk missing (2x2px area becomes transparent or debris-colored). Surface chips visible (shifted highlight pixels). |
| SA15 | stone_wall_critical | -- | -- | Critical state for SA13. Heavy fracturing across entire tile. Multiple cracks form web pattern. 30% of surface shows exposed darker interior (slot 9). Pieces visibly separating. One edge crumbling (1-2px gaps appearing). |
| SA16 | boulder | DH | 5 | Massive boulder. Nearly fills entire 8x8 tile. Rounded shape with flat bottom (sits on ground). Surface slots 2,3 with strong highlight on upper-left (slot 2) and deep shadow on lower-right (slot 5). Dense, heavy appearance. Tiny embedded crystal fleck (1px slot 29). |
| SA17 | boulder_damaged | -- | -- | Damage state for SA16. Boulder shape intact but with 2 major crack lines (slot 1) running diagonally. Surface chips near cracks show lighter interior (slot 6). Impact point visible as a small concavity (2px depression on one face). |
| SA18 | boulder_critical | -- | -- | Critical state for SA16. Boulder fractured into 3-4 visible segments with gaps between them (1px transparent lines). Each segment slightly offset. Dust pixels (slot 31) around fracture lines. Ready to shatter. |

#### Platform Tiles (IDs 19-21)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| SA19 | rock_ledge_left | P | Left cap of natural rock ledge. 3px thick shelf of stone (slots 2,3,4). Left end has rough broken edge, irregular and chipped. Underside shadow (slot 5). Natural rock support column 3px wide descending 3px below. |
| SA20 | rock_ledge_mid | P | Middle section of rock ledge. 3px thick stone shelf spanning full 8px width. Top surface slightly uneven (1px variation). Underside uses slot 5 shadow. Tiles seamlessly with adjacent ledge pieces. |
| SA21 | rock_ledge_right | P | Right cap. Rough broken right edge with unique break pattern distinct from SA19. Support column below. |

#### Hazard Tiles (IDs 22-23)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| SA22 | bone_spikes | H | Upward-pointing bone spike cluster. 2-3 sharpened bone fragments (slots 15,24) protruding upward 5-6px. Tips are 1px. Mounted in a small mound of earth (slots 7,8) at base, 2px tall. Primitive trap appearance. |
| SA23 | fire_pit_hazard | H | Active fire pit. Base: stone ring 2px tall (slots 3,4) forming a U-shape. Interior: fire pixels (slots 12,13,14) flickering upward 4px. Brightest at center (slot 12), orange edges (slot 13), red embers at base (slot 14). Contact causes burn damage. |

#### Background Tiles (IDs 24-27)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| SA24 | bg_cave_wall_deep | N | Deep background cave wall. Full 8x8 fill using slots 16,17,18. Rocky texture with subtle crack lines in slot 16. Much darker than foreground -- reads as deep cavern receding into darkness. |
| SA25 | bg_cave_painting_hunt | N | Cave painting decoration. Background cave wall (slots 17,18) with stick-figure hunting scene painted in ochre (slot 20) and charcoal (slot 22). 2-3 simplified human figures with spears pursuing an animal shape. Total painting area 6x5px. |
| SA26 | bg_cave_painting_hands | N | Cave painting: hand prints. Background wall (slots 17,18) with 2 hand stencil shapes in chalk white (slot 23) and ochre (slot 21). Each hand 3x4px. Primitive, iconic. |
| SA27 | bg_deep_cavern | N | Bottomless cavern background. Gradient from slot 18 at top to slot 16 at bottom. Suggests vast depth below. No detail -- pure atmospheric void. Occasional 1px stalactite silhouette (slot 17) at top edge. |

#### Decorative Tiles (IDs 28-31)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| SA28 | deco_bone_pile | N | Scattered bones on ground. 3-4 bone shapes (slots 24,25) -- a femur (4px long), rib fragments (2px), and a small skull (3x3px with 2 eye dots in slot 1). Arranged casually on tile bottom half. |
| SA29 | deco_primitive_tools | N | Primitive stone tools. A hand axe shape (3x4px, slots 3,4) with a wooden haft (2px, slot 8). A smaller scraper beside it (2x2px). Ground-level decoration. |
| SA30 | deco_campfire_glow | N | Extinguished campfire with residual glow. Stone ring (3 small stones, slots 3,4) at base. Charred wood (2px, slot 1). Warm ambient glow pixels (slot 30) in a 4px radius around center. Overlay decoration. |
| SA31 | deco_moss_patch | N | Damp cave moss. Organic spreading shape (slots 26,27) 5-6px wide, 2-3px tall. Irregular edges. Placed on floor or wall tiles as overlay to suggest moisture and age. |

---

## Era 4: Medieval Tileset

### Medieval Palette (32 Colors)

**Foreground Tiles (Slots 0-15):**

| Slot | Name | Hex Code | Usage |
|------|------|----------|-------|
| 0 | Transparent | N/A | Background key |
| 1 | Outline Dark | #1A1420 | Tile outlines, mortar lines, darkest detail |
| 2 | Castle Stone Light | #B0A898 | Cut stone highlight, weathered surface |
| 3 | Castle Stone Mid | #8E8678 | Cut stone body |
| 4 | Castle Stone Dark | #68604E | Cut stone shadow, crevices |
| 5 | Castle Stone Deep | #4A4238 | Interior stone fill, deep mortar |
| 6 | Cobble Light | #A09888 | Cobblestone highlight |
| 7 | Cobble Dark | #706858 | Cobblestone shadow, gaps |
| 8 | Wood Plank Light | #C8A870 | Wooden floor/door highlight |
| 9 | Wood Plank Mid | #A08448 | Wood body, grain |
| 10 | Wood Plank Dark | #705828 | Wood shadow, deep grain |
| 11 | Wood Plank Darkest | #483818 | Wood knots, deepest shadow |
| 12 | Iron Dark | #3A3A48 | Iron gates, metal fittings |
| 13 | Iron Mid | #585868 | Iron body |
| 14 | Iron Highlight | #787888 | Iron specular, rivets |
| 15 | Torch Flame | #FFB830 | Torch fire, warm light source |

**Background/Decorative Tiles (Slots 16-31):**

| Slot | Name | Hex Code | Usage |
|------|------|----------|-------|
| 16 | BG Sky Dark | #182848 | Night sky, deep background |
| 17 | BG Sky Mid | #283858 | Twilight sky |
| 18 | BG Stone Distant | #484050 | Background castle wall |
| 19 | BG Stone Detail | #585060 | Background stone texture |
| 20 | Banner Red | #C02020 | Royal banners, heraldry |
| 21 | Banner Red Dark | #801818 | Banner shadow |
| 22 | Banner Blue | #2040A0 | Heraldry blue |
| 23 | Banner Blue Dark | #182870 | Banner blue shadow |
| 24 | Stained Glass Gold | #E8C030 | Window decoration |
| 25 | Stained Glass Green | #30A048 | Window decoration |
| 26 | Stained Glass Red | #C03030 | Window decoration |
| 27 | Torch Glow Ambient | #483010 | Warm glow cast on walls |
| 28 | Mortar Light | #988870 | Visible mortar between blocks |
| 29 | Moss Castle | #506830 | Castle wall moss/lichen |
| 30 | Rope Brown | #907040 | Rope, bindings |
| 31 | Shadow Overlay | #181020 | Deep architectural shadow |

---

### Medieval Tile Definitions (32 Tiles)

#### Ground Tiles (IDs 0-10)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| MV00 | castle_floor_flat | S | Cut stone floor. Top 2 rows: flat dressed stone surface (slots 2,3) with faint chisel texture (alternating pixels). Bottom 6 rows: stone fill (slots 4,5) with mortar line (1px slot 28) at row 4 suggesting block joints. Seamless horizontal tiling. |
| MV01 | castle_floor_left_edge | S | Left edge of stone floor. Clean-cut left edge with 1px mortar reveal (slot 28). Left column is air. Stone block pattern visible on face. Connects to MV00 on right. |
| MV02 | castle_floor_right_edge | S | Right edge of stone floor. Mirror concept of MV01. Right column is air. Different mortar joint placement for asymmetry. |
| MV03 | castle_stone_fill | S | Interior castle wall/floor fill. Ashlar block pattern: 2 visible block courses separated by mortar lines (slot 28). Each block 3-4px wide. Slots 3,4,5 with subtle variation between blocks. Tiles seamlessly in all directions. |
| MV04 | cobblestone_floor | S | Cobblestone ground. Irregular rounded stone shapes (slots 6,7) in a mosaic pattern. Each cobble 2-3px across with 1px dark mortar gaps (slot 1). Top surface slightly uneven. Street/courtyard feel. |
| MV05 | wood_floor_flat | S | Wooden plank floor. Horizontal planks (slots 8,9,10) 2px tall each. Grain lines (1px slot 10) running horizontally. Plank gaps (1px slot 11) between courses. Warm interior feel. |
| MV06 | castle_left_inner_corner | S | Inner corner where wall meets floor from left. Stone blocks with precise mortar joints. Corner has a slightly larger keystone block (3x3px). |
| MV07 | castle_right_inner_corner | S | Inner corner from right. Mirror of MV06 with different block sizing. |
| MV08 | castle_left_wall | S | Left-facing castle wall. Left 2 columns: dressed stone face (slots 2,3) with mortar joints. Remaining columns: solid fill (slots 4,5). Vertical mortar line at column 2. |
| MV09 | castle_right_wall | S | Right-facing castle wall. Mirror of MV08 with unique block pattern. |
| MV10 | castle_slope_right | SR | Right-ascending stone ramp/staircase. Diagonal from bottom-right to top-left. Stone step shapes follow the diagonal with 2px treads and 2px risers forming a crude stair silhouette within the diagonal. |

#### Destructible Tiles (IDs 11-18)

| ID | Name | Collision | HP | Description |
|----|------|-----------|-----|-------------|
| MV11 | wooden_door | DS | 1 | Wooden door section. Vertical planks (slots 8,9,10) with horizontal crossbar (slot 10) at center. Iron nail heads (1px slot 12) at crossbar intersections. Splinters on first hit. |
| MV12 | stone_wall_breakable | DM | 3 | Mortared stone wall. 2 courses of cut stone blocks (slots 2,3,4) with visible mortar (slot 28). Resembles MV03 but designated breakable. Load-bearing appearance. |
| MV13 | stone_wall_breakable_damaged | -- | -- | Damage state for MV12. Mortar crumbling (mortar pixels replaced with darker slot 1). One block has a diagonal crack. Corner of one block chipped away (2px transparent). Surface dust (shifted highlight positions). |
| MV14 | stone_wall_breakable_critical | -- | -- | Critical state for MV12. Multiple blocks fractured. Large crack runs full height of tile. Mortar almost entirely gone between upper blocks. 20% of surface missing (transparent gaps). Blocks visibly separating. |
| MV15 | iron_gate | DH | 5 | Iron portcullis gate section. Vertical bars (2px wide, slots 12,13,14) spaced 2px apart. Horizontal crossbar at row 3 (slots 12,13). Rivets at intersections (1px slot 14). Heavy, imposing, dark. |
| MV16 | iron_gate_damaged | -- | -- | Damage state for MV15. One bar bent 1px outward at center. Rivet missing (pixel removed). Surface scratches (slot 14 highlights shifted). Structural integrity compromised but still blocking. |
| MV17 | iron_gate_critical | -- | -- | Critical state for MV15. Two bars bent significantly. One bar broken (gap in middle). Remaining crossbar cracked. Rust pixels appearing (1px slot 10 on metal). Nearly passable. |
| MV18 | wooden_barricade | DS | 1 | Hasty wooden barricade. Diagonal planks (slots 9,10,11) nailed together at angles. Visible nail heads (slot 12). Gaps between planks show background. Crude, battlefield construction. Shatters easily. |

#### Platform Tiles (IDs 19-21)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| MV19 | wood_beam_left | P | Left end of wooden support beam. 3px thick beam (slots 8,9,10) with squared-off left end. Wood grain horizontal. Iron bracket on underside (2px, slot 12). Platform collision on top 2px. |
| MV20 | wood_beam_mid | P | Middle section of beam. 3px thick spanning full 8px. Wood grain detail. Single iron nail on underside (1px slot 12). Tiles seamlessly. |
| MV21 | wood_beam_right | P | Right end of beam. Squared right end with iron bracket. Mirror concept of MV19 with unique grain pattern. |

#### Hazard Tiles (IDs 22-23)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| MV22 | spike_trap | H | Floor spike trap. Metal spikes (slots 12,13,14) protruding 5px upward from a stone base plate (slots 3,4). 3 spikes across the tile, each 1px wide at tip. Mechanical trap feel with visible hinge mechanism at base (2px slot 12). |
| MV23 | boiling_oil_drip | H | Ceiling-mounted oil hazard. Murder hole opening in ceiling stone (slots 3,4). Hot oil dripping down (slots 10,11 dark streaks). Oil droplet at bottom (2px). Damage on contact. |

#### Background Tiles (IDs 24-27)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| MV24 | bg_castle_wall | N | Background castle wall. Full 8x8 fill using BG stone slots 18,19. Faint block pattern visible. Much darker than foreground -- reads as distant interior wall. Torch glow influence (1-2px of slot 27) on right side suggesting off-screen light source. |
| MV25 | bg_castle_tower | N | Background tower silhouette. Vertical tower shape 4px wide (slots 16,18) rising full height. Crenellation at top (2 merlons, 1px gaps). Small window slit (1px slot 16). Distant, atmospheric. |
| MV26 | bg_stained_glass | N | Stained glass window. Arched window shape 6px wide, 7px tall. Lead lines (slot 1) dividing 4-5 panes. Panes filled with slots 24,25,26 (gold, green, red). Bright against dark wall (slots 18,19). Jewel-like. |
| MV27 | bg_banner | N | Hanging banner/tapestry. Rectangular banner shape 4px wide, 6px tall. Red field (slots 20,21) with simple heraldic device (shield shape in slot 24, 3x3px). Hanging from iron rod (1px slot 12) at top. Bottom edge has 2 triangular points. |

#### Decorative Tiles (IDs 28-31)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| MV28 | deco_wall_torch | N | Wall-mounted torch. Iron bracket (slots 12,13) protruding 2px from right side. Wooden torch handle (slot 10) 1px wide, 3px tall. Flame (slots 15,13) 3px tall at top, flickering shape. Glow pixels (slot 27) surrounding flame in 2px radius. |
| MV29 | deco_shield | N | Decorative wall shield. Kite shield shape 5x6px (slots 12,13 rim, slot 20 red field). Simple cross device in slot 24. Mounted on wall (appears to hang from single point). |
| MV30 | deco_tapestry | N | Wall tapestry section. Woven textile pattern (slots 20,21,22,23) in a simple repeating diamond motif. 6px wide, 8px tall (full height). Slight wave at bottom suggesting fabric drape. |
| MV31 | deco_chain | N | Hanging chain. Iron chain links (slots 12,13,14) descending vertically. Each link 2x3px, alternating orientation. 2 complete links visible. Hangs from ceiling attachment point. Dungeon atmosphere. |

---

## Era 7: Modern Tileset

### Modern Palette (32 Colors)

**Foreground Tiles (Slots 0-15):**

| Slot | Name | Hex Code | Usage |
|------|------|----------|-------|
| 0 | Transparent | N/A | Background key |
| 1 | Outline Dark | #101018 | Outlines, deepest shadow |
| 2 | Concrete Light | #B0B0B8 | Concrete surface highlight |
| 3 | Concrete Mid | #888890 | Concrete main fill |
| 4 | Concrete Dark | #606068 | Concrete shadow, cracks |
| 5 | Concrete Deep | #404048 | Deep concrete, structural |
| 6 | Asphalt Light | #585858 | Road surface highlight |
| 7 | Asphalt Dark | #383838 | Road surface shadow |
| 8 | Metal Grate Light | #909098 | Metal grating highlight |
| 9 | Metal Grate Mid | #686870 | Metal grating body |
| 10 | Metal Grate Dark | #484850 | Metal grating shadow |
| 11 | Steel Blue | #708090 | Reinforced steel, beams |
| 12 | Rebar Orange | #C06020 | Exposed rebar, rust |
| 13 | Hazard Yellow | #E8C800 | Warning stripes, caution |
| 14 | Hazard Black | #181818 | Warning stripe dark band |
| 15 | Spark White | #F0F0FF | Electrical spark, weld flash |

**Background/Decorative Tiles (Slots 16-31):**

| Slot | Name | Hex Code | Usage |
|------|------|----------|-------|
| 16 | BG Sky Dusk | #182030 | City sky, dark |
| 17 | BG Sky Mid | #283848 | City sky gradient |
| 18 | BG Building Dark | #282830 | Background building mass |
| 19 | BG Building Detail | #383840 | Background window grid |
| 20 | Neon Pink | #FF2080 | Neon sign accent |
| 21 | Neon Blue | #20A0FF | Neon sign accent |
| 22 | Neon Green | #20FF60 | Neon sign, exit signs |
| 23 | Window Glow | #D0D8E0 | Lit window interior |
| 24 | Pipe Grey | #606060 | Pipe body |
| 25 | Pipe Shadow | #404040 | Pipe underside |
| 26 | Wire Black | #181820 | Cable, wire |
| 27 | Screen Glow | #80C0E0 | Computer/TV screen light |
| 28 | Graffiti Red | #D03030 | Urban decoration |
| 29 | Graffiti Blue | #3060C0 | Urban decoration |
| 30 | Vent Silver | #A0A0A8 | HVAC vent slats |
| 31 | Dirt Urban | #484030 | Urban grime, stains |

---

### Modern Tile Definitions (32 Tiles)

#### Ground Tiles (IDs 0-10)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| MD00 | concrete_floor_flat | S | Flat concrete slab. Top 2 rows: smooth concrete surface (slots 2,3) with subtle expansion joint line (1px slot 4) at column 4. Bottom 6 rows: concrete fill (slots 3,4,5). Hairline crack detail (1px slot 4) in lower portion. Seamless horizontal tiling. |
| MD01 | concrete_floor_left_edge | S | Left edge of concrete platform. Clean break or formed edge. Left column is air. Exposed aggregate visible on cut face (1px dots of slot 6 in slot 5 fill). Rebar stub protruding 1px (slot 12) on exposed face. |
| MD02 | concrete_floor_right_edge | S | Right edge. Mirror concept of MD01. Different crack pattern. Connects to MD00 on left. |
| MD03 | concrete_fill | S | Solid concrete interior. Uniform fill (slots 3,4,5) with minimal texture -- cast concrete appearance. Occasional air bubble void (1px slot 1). Form tie holes suggested by paired dots (slot 4). Tiles seamlessly in all directions. |
| MD04 | asphalt_ground | S | Asphalt road surface. Top 2 rows: smooth dark surface (slots 6,7). Road marking option: 1px yellow line (slot 13) at top edge. Bottom rows: compacted base (slots 7,1). Aggregate texture (alternating pixels). |
| MD05 | metal_grate_floor | S | Industrial metal grating. Grid pattern of metal bars (slots 8,9,10). Bars 1px wide with 1px gaps between them. Grid pattern at 0 and 90 degrees. Beneath grate: dark void (slot 1). Open, industrial feel. Light passes through. |
| MD06 | concrete_left_inner_corner | S | Inner corner. Formed concrete edge where wall meets floor. Sharp 90-degree joint with visible form line (1px slot 4). Clean, manufactured. |
| MD07 | concrete_right_inner_corner | S | Inner corner from right. Mirror of MD06. |
| MD08 | concrete_left_wall | S | Left-facing concrete wall. Left 2 columns: formed wall surface (slots 2,3). Form tie pattern (paired 1px holes, slot 4) at regular intervals. Remaining columns: solid fill. |
| MD09 | concrete_right_wall | S | Right-facing wall. Mirror of MD08 with unique surface pattern. |
| MD10 | concrete_slope_left | SL | Left-ascending concrete ramp. Diagonal surface with smooth formed finish. Yellow safety stripe (1px slot 13) along top edge of slope. Below: solid fill. |

#### Destructible Tiles (IDs 11-18)

| ID | Name | Collision | HP | Description |
|----|------|-----------|-----|-------------|
| MD11 | drywall_panel | DS | 1 | Interior drywall. Smooth light surface (slot 2) with slight texture. Thin profile -- only 6px of the 8px tile is filled (2px air on top or one side). Punches through instantly. Debris: white dust particles and paper-faced chunks. |
| MD12 | concrete_wall_breakable | DM | 3 | Reinforced concrete wall. Dense fill (slots 3,4,5). Surface has poured concrete texture with aggregate visible (1px dots). Embedded rebar suggested by occasional orange pixel (slot 12) visible at edges. Structural. |
| MD13 | concrete_wall_damaged | -- | -- | Damage state for MD12. Spalling concrete: surface chips missing (2-3px areas where slot 2 becomes slot 5). Crack propagating from impact point. Rebar partially exposed (2px of slot 12 visible through surface). |
| MD14 | concrete_wall_critical | -- | -- | Critical state for MD12. Major structural failure. Rebar fully exposed in center (3px of slot 12 with concrete missing around it). Surface 40% destroyed. Chunks hanging by rebar. About to collapse. |
| MD15 | reinforced_steel | DH | 5 | Heavy steel plate. Solid metal surface (slots 11,9,10). Visible bolt heads in corners (1px slot 15 highlight). Welded seam running horizontally at center (1px raised line, slot 8). Industrial, heavy, resistant. |
| MD16 | reinforced_steel_damaged | -- | -- | Damage state for MD15. Dent visible at impact point (2px concavity, shadow shifts). Bolt head sheared off one corner. Weld seam cracked (gap in line). Surface scratches (slot 8 highlight streaks). |
| MD17 | reinforced_steel_critical | -- | -- | Critical state for MD15. Major deformation. Steel plate buckling (surface pixels shifted to suggest warping). Weld failed (2px gap). One edge peeling back (1px lifted corner). Sparks at stress points (1px slot 15). |
| MD18 | glass_panel | DS | 1 | Glass window/wall panel. Nearly transparent fill with visible edges. Frame: 1px border in slot 9. Interior: sparse highlight pixels (slot 15) suggesting reflections. Shatters into sharp particle debris on any hit. |

#### Platform Tiles (IDs 19-21)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| MD19 | i_beam_left | P | Left end of structural I-beam. Steel beam profile (slots 11,9,10): top flange 2px, web 1px centered, bottom flange 2px. Left end has cut/welded termination. Rust stain (1px slot 12) at bottom flange corner. |
| MD20 | i_beam_mid | P | Middle section of I-beam. Standard I-beam cross section spanning full width. Web has punched holes (1px slot 1 at regular intervals) for service access. Tiles seamlessly. |
| MD21 | i_beam_right | P | Right end of I-beam. Cut termination with weld bead (1px slot 8) on end. Different rust pattern from MD19. |

#### Hazard Tiles (IDs 22-23)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| MD22 | electrical_hazard | H | Exposed electrical junction. Metal box (slots 9,10) 6x6px. Open front reveals wires (slot 26) and active sparking (slots 15,13 -- 2-3px spark pixels). Warning chevron (slots 13,14) on box face. Animated concept: spark flickers between 2 frame positions. |
| MD23 | acid_pool_industrial | H | Industrial chemical spill. Neon green-yellow pool (slots 22,13) filling bottom 3 rows. Surface shimmer (1px slot 15). Dark containment floor beneath (slot 7). Hazard stripe border (alternating slots 13,14) on back edge. Toxic. |

#### Background Tiles (IDs 24-27)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| MD24 | bg_city_skyline | N | City skyline silhouette. Building shapes in slots 18,19 against sky (slots 16,17). 2-3 building outlines of different heights. Lit windows: scattered 1px dots of slot 23. Antenna on tallest building (1px). Atmospheric, distant. |
| MD25 | bg_neon_sign | N | Background neon sign. Dark wall (slots 18,19). Neon lettering or shape in slots 20 or 21 -- a simple geometric bar shape or arrow, 5x3px. Glow aura: 1px of slightly lighter background around neon elements. Urban nightlife feel. |
| MD26 | bg_infrastructure | N | Background pipes and ducts. Dark wall (slot 18). Horizontal pipe (slot 24,25) 2px diameter crossing tile. Vertical conduit (slot 26) 1px wide. Junction box where they meet (2x2px slot 19). Industrial depth. |
| MD27 | bg_building_windows | N | Background building facade. Grid of windows (slot 23 lit, slot 19 dark) in a 2x3 arrangement. Building wall (slot 18). Some windows dark (off), some lit -- suggests inhabited building behind gameplay layer. |

#### Decorative Tiles (IDs 28-31)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| MD28 | deco_pipe_horizontal | N | Exposed pipe running horizontally. Cylindrical pipe (slots 24,25) 2px diameter at mid-height. Pipe clamp/bracket (slot 9) every 4px holding it to wall. Condensation drip (1px slot 29) hanging from underside. |
| MD29 | deco_wire_bundle | N | Cable bundle. 3-4 wires (slot 26) running vertically, loosely bundled. Cable tie (1px slot 9) at center holding them together. Wires splay slightly at top and bottom edges. |
| MD30 | deco_computer_screen | N | Small monitor/terminal. Screen body: 5x4px rectangle (slot 10 frame, slot 27 screen glow). 1px stand at bottom (slot 10). Screen displays 2 lines of "text" (1px dots in slot 22). Technological detail. |
| MD31 | deco_graffiti | N | Urban graffiti tag. Abstract spray-painted shape on wall surface. 5x4px area using slots 28,29 in an angular, stylized mark. Drip lines (1px descending from lowest points). Street-level atmosphere. |

---

## Era 10: Transcendent Tileset

### Transcendent Palette (32 Colors)

**Foreground Tiles (Slots 0-15):**

| Slot | Name | Hex Code | Usage |
|------|------|----------|-------|
| 0 | Transparent | N/A | Background key |
| 1 | Void Black | #06000E | Absolute void, deepest absence |
| 2 | Crystal Surface Light | #E0D8FF | Crystalline highlight, near-white |
| 3 | Crystal Surface Mid | #A898E0 | Crystal body |
| 4 | Crystal Surface Dark | #6858A8 | Crystal shadow |
| 5 | Crystal Deep | #382870 | Crystal interior, deep |
| 6 | Energy White | #F8F0FF | Pure energy, brightest |
| 7 | Energy Blue | #60A0FF | Energy flow, active |
| 8 | Energy Cyan | #40E8F0 | Secondary energy accent |
| 9 | Energy Purple | #A040E0 | Tertiary energy, arcane |
| 10 | Void Platform | #281848 | Solid void-matter surface |
| 11 | Void Platform Light | #3C2868 | Void-matter highlight |
| 12 | Void Path | #180830 | Walkable void |
| 13 | Reality Fracture | #FF60A0 | Reality-break highlight, warning |
| 14 | Dimensional Amber | #F0A020 | Cross-dimensional energy |
| 15 | Quantum Lattice | #B0B8FF | Lattice structure, grid lines |

**Background/Decorative Tiles (Slots 16-31):**

| Slot | Name | Hex Code | Usage |
|------|------|----------|-------|
| 16 | BG Cosmic Void | #020008 | Deepest space void |
| 17 | BG Nebula Dark | #100828 | Nebula body, dark |
| 18 | BG Nebula Mid | #281850 | Nebula mid-tone |
| 19 | BG Nebula Light | #4030A0 | Nebula highlight |
| 20 | BG Star White | #F0F0FF | Distant star point |
| 21 | BG Star Warm | #FFE0A0 | Warm star point |
| 22 | Fractal Blue | #3050FF | Fractal geometry accent |
| 23 | Fractal Purple | #8020D0 | Fractal geometry secondary |
| 24 | Rune Glow | #A0F0FF | Floating rune light |
| 25 | Rune Core | #60D0E0 | Rune interior |
| 26 | Stream Energy A | #50FF90 | Energy stream color A |
| 27 | Stream Energy B | #30B0FF | Energy stream color B |
| 28 | Rift Orange | #FF8030 | Dimensional rift warm edge |
| 29 | Rift Red | #E02040 | Dimensional rift hot edge |
| 30 | Particle Glow | #E0E0FF | Ambient floating particles |
| 31 | Void Gradient | #0C0420 | Void gradient mid-tone |

---

### Transcendent Tile Definitions (32 Tiles)

#### Ground Tiles (IDs 0-10)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| TR00 | crystal_floor_flat | S | Crystalline platform surface. Top 2 rows: faceted crystal surface (slots 2,3) with sharp geometric highlights (1px slot 6 at regular intervals suggesting light refraction). Bottom 6 rows: deep crystal structure (slots 4,5) with internal geometry lines (1px slot 15). Seamless horizontal tiling. |
| TR01 | crystal_floor_left_edge | S | Left edge of crystal platform. Shattered crystalline edge -- angular break (not rounded). Left 2 columns are void (transparent). Break face reveals internal lattice structure (1px grid of slot 15). Sharp, geometric. |
| TR02 | crystal_floor_right_edge | S | Right edge. Angular break pattern distinct from TR01 -- different facet angles. Crystal shards at edge (1px protrusions). |
| TR03 | void_platform_fill | S | Solid void-matter interior. Fill with slots 10,11,12. Not natural -- appears manufactured from compressed void. Faint geometric pattern (2px repeating grid of slot 11 in slot 10 field). Unsettling, unnatural solidity. |
| TR04 | energy_platform_surface | S | Platform made of solidified energy. Top 2 rows: bright energy surface (slots 7,8) with animated shimmer concept. Bottom rows: energy fading into structure (gradient from slot 7 to slot 10). Hums with power. Light source feel. |
| TR05 | void_path | S | Walkable path through void space. Narrow path (center 6 columns are solid, outer columns transparent). Surface is slot 10 with edge glow (1px slot 9) on both sides. Floating in nothingness. Surreal. |
| TR06 | crystal_left_inner_corner | S | Inner corner. Crystal facets meet at precise geometric angles. Corner joint shows lattice structure (slot 15). Energy seam at joint (1px slot 7). |
| TR07 | crystal_right_inner_corner | S | Inner corner from right. Different facet angles. Energy seam color variant (slot 8). |
| TR08 | crystal_left_wall | S | Left-facing crystal wall. Left 2 columns: faceted crystal face with multiple angled facets (slots 2,3,4). Light refraction highlights (1px slot 6). Remaining columns: deep fill. |
| TR09 | crystal_right_wall | S | Right-facing crystal wall. Different facet pattern from TR08 -- no two crystal faces are identical. Internal inclusion visible (1px slot 9). |
| TR10 | energy_slope_left | SL | Left-ascending energy ramp. Diagonal energy surface (slots 7,8) from bottom-left to top-right. Energy particles trailing upward from surface (1-2px slot 6 above the line). Below: void fill (slot 12). Gravity-defying. |

#### Destructible Tiles (IDs 11-18)

| ID | Name | Collision | HP | Description |
|----|------|-----------|-----|-------------|
| TR11 | reality_shard | DS | 1 | Fragment of crystallized reality. Irregular geometric shape (slots 2,3,13) filling 6x6px of tile. Semi-transparent feel (some interior pixels show background colors). Shatters into prismatic particles on impact. Beautiful, fragile. |
| TR12 | quantum_lattice_wall | DM | 3 | Quantum lattice structure. Grid of energy lines (slot 15) forming a precise 2px geometric mesh. Nodes at intersections glow (1px slot 6). Spaces between lattice show void (slot 1). Structured, mathematical. |
| TR13 | quantum_lattice_damaged | -- | -- | Damage state for TR12. Lattice lines disrupted -- 2-3 connections broken (gaps in grid). Nodes at broken connections flicker (shifted between slot 6 and slot 9, suggesting instability). Energy sparks at break points (slot 14). |
| TR14 | quantum_lattice_critical | -- | -- | Critical state for TR12. Lattice mostly collapsed. Only 30% of connections remain. Broken nodes emit unstable energy (slot 13 replacing slot 6). Void bleeding through gaps. Geometric structure dissolving into chaos. |
| TR15 | void_barrier | DH | 5 | Barrier of compressed void-matter. Dense, dark fill (slots 1,12,10) with an oppressive solidity. Surface has anti-light property -- highlights are darker than shadows (inverted shading, slot 12 "highlight" on slot 10 body). Energy containment band (1px slot 9) at top and bottom edges. Alien, resistant. |
| TR16 | void_barrier_damaged | -- | -- | Damage state for TR15. Containment bands flickering (alternating pixels). Surface showing stress fractures that glow with leaked energy (1px lines of slot 7 through dark mass). Anti-light effect weakening (normal highlights beginning to appear). |
| TR17 | void_barrier_critical | -- | -- | Critical state for TR15. Containment failed on one edge (band broken, 2px gap). Void-matter destabilizing -- surface pixels shifting between slot 1 and slot 9 suggesting phase instability. Energy bleeding out of every crack. Reality reasserting around edges. |
| TR18 | crystal_pillar | DS | 1 | Thin crystal column. Vertical crystal shaft 2px wide, full height (slots 2,3,4). Faceted surface with highlight (slot 6). Snaps cleanly. Debris: crystal shards with prismatic sparkle. |

#### Platform Tiles (IDs 19-21)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| TR19 | energy_bridge_left | P | Left end of energy bridge. Solid energy surface (slots 7,8) 2px thick. Left termination: energy dissipates into particles (2-3 scattered 1px dots of slot 6 beyond the edge). Containment field below (1px slot 15 line). Platform collision on top 2px only. |
| TR20 | energy_bridge_mid | P | Middle section of energy bridge. 2px thick energy surface spanning full width. Internal energy flow pattern: 1px lighter streaks (slot 6) moving horizontally. Stable, humming. |
| TR21 | energy_bridge_right | P | Right end. Energy dissipation particles on right. Different particle scatter pattern from TR19. |

#### Hazard Tiles (IDs 22-23)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| TR22 | reality_fracture | H | Torn reality hazard. Jagged crack in space (slot 13 hot pink edges) running diagonally across tile. Interior of crack shows impossible colors (slots 28,29 -- orange and red bleeding through from another dimension). Void space (slot 1) visible through crack. Reality distortion around edges (1px slot 14). |
| TR23 | energy_vortex | H | Spinning energy vortex. Circular energy pattern (slots 7,8,9) 6px diameter centered in tile. Spiral arms suggested by pixel arrangement. Center is bright (slot 6). Outer ring darker (slot 9). Animated concept: spiral rotates over 4 frames. Pulls and damages. |

#### Background Tiles (IDs 24-27)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| TR24 | bg_cosmic_void | N | Cosmic void background. Near-black fill (slots 16,31). Scattered star points: 3-4 single pixels of slot 20 (white) and 1 pixel of slot 21 (warm) placed irregularly. Vast, empty, humbling. Deepest background. |
| TR25 | bg_nebula_cloud | N | Nebula cloud formation. Amorphous cloud shapes using slots 17,18,19. Organic, flowing edges. Brighter core (slot 19) fading to dark edges (slot 17). 6x6px cloud mass. Cosmic beauty behind the gameplay. |
| TR26 | bg_fractal_pattern | N | Fractal geometry background. Self-similar geometric pattern using slots 22,23. Triangular or hexagonal repeating shapes that tile to suggest infinite recursive structure. 4 levels of detail visible within 8x8 space. Mathematical, alien, beautiful. |
| TR27 | bg_dimensional_rift | N | Background dimensional rift. Vertical tear in space 2px wide, full height. Edges glow with slots 28,29. Interior shows different-colored void (slot 19 -- brighter than surrounding space). Energy streamers extend 1-2px from edges. Something beyond is visible. |

#### Decorative Tiles (IDs 28-31)

| ID | Name | Collision | Description |
|----|------|-----------|-------------|
| TR28 | deco_floating_rune | N | Floating arcane rune. Geometric symbol (slots 24,25) 4x4px centered in tile. Circular border with interior geometric mark (triangle, spiral, or cross). Glow aura: 1px of slot 30 surrounding the rune. Floats independently of gravity. Animated concept: slow pulse (brightness oscillation over 2 frames). |
| TR29 | deco_energy_stream | N | Vertical energy stream. Flowing energy (slots 26,27) 2px wide descending through tile. Stream splits and rejoins (1px branch at mid-height). Particle sparkles (1px slot 6) alongside stream. Colors shift between stream A and B. |
| TR30 | deco_dimensional_crack | N | Small dimensional crack. Hairline fracture in reality 1px wide, running 5px at slight angle. Edges tinted slot 13. Interior pixel shows slot 28 (another dimension bleeding through). Subtle, unsettling. Not hazardous -- just decorative evidence of reality strain. |
| TR31 | deco_void_particle | N | Ambient void particles. 3-4 scattered single pixels (slot 30) floating in tile space. No structure -- pure scattered light motes. Animated concept: particles drift slowly (position shifts by 1px per frame over 4 frames). Atmospheric, ethereal. |

---

## Destructible Tile Damage State System

### Visual State Definitions

Every destructible tile of type DM or DH has exactly 3 visual states stored as separate tile entries in the tile sheet. DS (soft) tiles have only 1 state -- they are intact until destroyed.

#### State 1: Intact (Full HP)

- Tile appears at its designed, undamaged appearance
- No cracks, chips, or visual indicators of weakness
- May include subtle material-appropriate details (mortar lines, surface texture) that distinguish it from non-destructible SOLID tiles
- **Visual tell**: Destructible tiles should have a subtle visual distinction from indestructible solid tiles. This is era-specific:
  - Stone Age: Looser stone arrangement, visible seams
  - Medieval: Lighter mortar color, slightly different block pattern
  - Modern: Form tie holes, subtle surface variation
  - Transcendent: Energy containment bands, lattice edges

#### State 2: Damaged (Mid HP)

- Visible cracks: 2-3 crack lines (1px wide) radiating from impact point or center
- Surface chips: 2-3 pixels of surface material removed, revealing darker interior
- Material-specific indicators:
  - Stone/Rock: Cracks follow natural fault lines, chips are angular
  - Wood: Splintering along grain direction, nail pops
  - Metal: Dents (shadow shift), paint/coating scratched off
  - Crystal: Fracture lines along geometric planes
  - Energy: Flickering, containment disruption, color instability
- **Pixel budget for damage indicators**: 6-10 pixels changed from intact state

#### State 3: Critical (Low HP, about to break)

- Heavy structural failure visible across entire tile
- 20-40% of surface area shows damage (transparent pixels, color changes, or displaced elements)
- Material-specific indicators:
  - Stone/Rock: Web of cracks, chunks separating, rubble forming
  - Wood: Major splits, pieces hanging, structural failure
  - Metal: Major deformation, buckling, broken welds
  - Crystal: Shattered facets, pieces floating apart
  - Energy: Violent flickering, containment failure, energy bleeding
- **Pixel budget for damage indicators**: 15-25 pixels changed from intact state
- Player should clearly read this tile as "one more hit"

### Damage State Transition Rules

```
When a DESTRUCTIBLE tile takes damage:
  1. Calculate new HP = current_HP - weapon_damage
  2. If new_HP <= 0:
       Play destruction particle effect (era-specific debris)
       Remove tile from tilemap (replace with NONE)
       Apply screen shake if DH type
  3. Else if new_HP <= max_HP * 0.33:
       Swap tile sprite to CRITICAL state
       Play crack/stress sound effect
  4. Else if new_HP < max_HP:
       Swap tile sprite to DAMAGED state
       Play chip/impact sound effect
```

### Debris Particle Colors Per Era

When a destructible tile is destroyed, it spawns debris particles using colors from that era's palette.

| Era | Debris Colors (Palette Slots) | Debris Shape |
|-----|-------------------------------|--------------|
| Stone Age | 2, 3, 4, 7, 8 (rock and earth tones) | Angular chunks, dust cloud |
| Medieval | 2, 3, 8, 9, 12 (stone, wood, iron) | Block fragments, splinters |
| Modern | 2, 3, 5, 12, 15 (concrete, rebar, sparks) | Concrete chunks, rebar pieces, dust |
| Transcendent | 2, 6, 7, 13, 15 (crystal, energy, sparks) | Prismatic shards, energy dissipation |

---

## Auto-Tiling System

### Bitmask Approach: 4-Bit Cardinal Neighbors

The auto-tiling system uses a 4-bit bitmask based on cardinal (N, E, S, W) neighbors. Each bit represents whether an adjacent tile of the same terrain type exists. This applies to both standard solid tiles and destructible tiles.

```
Bit Layout:
  [N]       Bit 0 = North (value 1)
[W][X][E]   Bit 1 = East  (value 2)
  [S]       Bit 2 = South (value 4)
            Bit 3 = West  (value 8)
```

### Bitmask Value Calculation

```
bitmask = 0
if (neighbor_north is same_terrain): bitmask += 1
if (neighbor_east  is same_terrain): bitmask += 2
if (neighbor_south is same_terrain): bitmask += 4
if (neighbor_west  is same_terrain): bitmask += 8
```

This produces values 0-15 (16 possible configurations).

### Era Ground Auto-Tile Mapping (Universal Pattern)

All eras follow the same bitmask-to-tile-variant mapping. The table below uses generic names; substitute era-specific tile IDs (e.g., SA00 for Stone Age, MV00 for Medieval).

| Bitmask | Binary (WSEN) | Configuration | Tile Variant |
|---------|---------------|---------------|--------------|
| 0 | 0000 | Isolated block | floor_flat (standalone) |
| 1 | 0001 | Neighbor N only | ceiling / bottom edge |
| 2 | 0010 | Neighbor E only | floor_left_edge |
| 3 | 0011 | Neighbors N,E | left_wall (variant) |
| 4 | 0100 | Neighbor S only | floor_flat |
| 5 | 0101 | Neighbors N,S | fill (vertical strip) |
| 6 | 0110 | Neighbors E,S | left_inner_corner |
| 7 | 0111 | Neighbors N,E,S | left_wall |
| 8 | 1000 | Neighbor W only | floor_right_edge |
| 9 | 1001 | Neighbors N,W | right_wall (variant) |
| 10 | 1010 | Neighbors E,W | floor_flat (middle) |
| 11 | 1011 | Neighbors N,E,W | fill (under surface) |
| 12 | 1100 | Neighbors S,W | right_inner_corner |
| 13 | 1101 | Neighbors N,S,W | right_wall |
| 14 | 1110 | Neighbors E,S,W | floor_flat (top of mass) |
| 15 | 1111 | All neighbors | fill (fully enclosed) |

### Destructible Tile Auto-Tiling

Destructible tiles participate in auto-tiling with these rules:

1. **Destructible tiles auto-tile with each other** -- adjacent destructible tiles of the same material form connected shapes using the bitmask system.
2. **Destructible tiles treat SOLID tiles as neighbors** -- a destructible tile next to a solid tile will select a connecting variant (e.g., no exposed edge on that side).
3. **When a destructible tile is destroyed**, adjacent destructible tiles must recalculate their bitmask and swap to the appropriate variant. This creates cascading visual updates as sections are blasted away.
4. **Damage states are per-tile** -- destroying one tile does not damage its neighbors (unless using splash damage weapons, handled by the damage system, not the tilemap).

### Extended 8-Bit Bitmask (Diagonal Awareness)

For higher-quality auto-tiling that handles diagonal corners properly, an 8-bit bitmask extends the 4-bit system:

```
Bit Layout:
[NW][N][NE]   Bit 0 = North      (1)
 [W][X][E]    Bit 1 = NorthEast  (2)
[SW][S][SE]   Bit 2 = East       (4)
              Bit 3 = SouthEast  (8)
              Bit 4 = South      (16)
              Bit 5 = SouthWest  (32)
              Bit 6 = West       (64)
              Bit 7 = NorthWest  (128)
```

**Diagonal Rule**: A diagonal neighbor is only considered "present" if BOTH adjacent cardinal neighbors are also present.

```
// Pseudocode for 8-bit bitmask calculation
bitmask = 0
if (N):  bitmask |= 1
if (E):  bitmask |= 4
if (S):  bitmask |= 16
if (W):  bitmask |= 64
if (NE and N and E): bitmask |= 2
if (SE and S and E): bitmask |= 8
if (SW and S and W): bitmask |= 32
if (NW and N and W): bitmask |= 128
```

This produces 47 unique visual configurations. The 8-bit system is recommended for polish phase; the 4-bit system is sufficient for initial implementation.

### Platform Auto-Tiling (Simplified)

Platforms use a simpler 1D horizontal-only system:

| Left Neighbor | Right Neighbor | Tile |
|--------------|----------------|------|
| No | No | platform_mid (single-width, use mid with capped ends) |
| No | Yes | platform_left |
| Yes | No | platform_right |
| Yes | Yes | platform_mid |

---

## Era Palette Progression Guidelines

### Overview

The 10 eras of human civilization follow a deliberate color progression that tells a visual story of humanity's journey. Early eras are warm and organic; middle eras introduce manufactured precision; late eras become increasingly alien and transcendent. Each era's palette should feel distinctly different at a glance while maintaining internal cohesion.

### Era 1: Stone Age (Fully Defined Above)

- **Dominant Tone**: Warm earth tones -- browns, tans, deep oranges
- **Accent Colors**: Firelight orange, bone white, cave painting ochres
- **Shadow Colors**: Deep warm browns, near-black earth
- **Mood**: Primal warmth, firelit intimacy, natural rawness
- **Destructible Materials**: Loose rock (crumbly, natural), stone walls (primitive masonry), boulders (dense natural stone)
- **Debris Style**: Angular stone chunks, dust clouds, earth scatter

### Era 2: Bronze Age

- **Dominant Tone**: Sandy tans, warm desert yellows, terracotta
- **Accent Colors**: Bronze/copper metallic (warm gold-brown), lapis blue, turquoise
- **Shadow Colors**: Deep terracotta, muddy browns
- **Mood**: Ancient civilization dawning, sun-baked monuments, first metals
- **Palette Anchors**: `#C8A050` (bronze highlight), `#886828` (bronze shadow), `#D4A868` (sandstone), `#2060A0` (lapis)
- **Destructible Materials**:
  - Soft: Mud brick (DS) -- sun-dried clay, crumbles to dust and chunks
  - Medium: Sandstone block (DM) -- carved stone, cracks along bedding planes
  - Hard: Bronze-reinforced wall (DH) -- stone with bronze bands, metal shrieks on impact

### Era 3: Iron Age / Classical

- **Dominant Tone**: Marble whites, warm stone, olive tones
- **Accent Colors**: Iron grey, patina green, crimson, gold leaf
- **Shadow Colors**: Cool grey-blue, deep olive
- **Mood**: Classical grandeur, columns and order, disciplined civilization
- **Palette Anchors**: `#E0D8D0` (marble), `#505860` (iron), `#708038` (olive), `#C02828` (crimson)
- **Destructible Materials**:
  - Soft: Plaster wall (DS) -- thin rendering over stone, shatters to white dust
  - Medium: Marble column section (DM) -- carved marble, fractures along veins
  - Hard: Iron-bound gate (DH) -- wooden gate with iron strapping, requires heavy weapons

### Era 4: Medieval (Fully Defined Above)

- **Dominant Tone**: Castle stone greys, earthy neutrals
- **Accent Colors**: Deep heraldic reds and blues, torchlight gold, iron dark
- **Shadow Colors**: Cold dark purples, deep stone
- **Mood**: Fortress solidity, feudal power, torchlit interiors, gothic weight
- **Destructible Materials**: Wooden doors (splinter), stone walls (crack and crumble), iron gates (bend and break)
- **Debris Style**: Stone block fragments, wood splinters, iron sparks

### Era 5: Renaissance / Age of Sail

- **Dominant Tone**: Rich warm woods, cream stone, vibrant textiles
- **Accent Colors**: Vermillion, ultramarine, gold, verdigris green
- **Shadow Colors**: Deep warm browns, navy
- **Mood**: Ornate craftsmanship, exploration, merchant wealth, artistic flourish
- **Palette Anchors**: `#C89848` (gilded wood), `#E8D8C0` (plaster), `#D04020` (vermillion), `#2040B0` (ultramarine)
- **Destructible Materials**:
  - Soft: Wooden crate (DS) -- merchant cargo, bursts into planks and contents
  - Medium: Brick wall (DM) -- fired brick with lime mortar, chips and crumbles
  - Hard: Ship hull plank (DH) -- thick hardwood with iron bolts, splinters dramatically

### Era 6: Industrial Revolution

- **Dominant Tone**: Dark iron, brick red-brown, soot-stained everything
- **Accent Colors**: Furnace orange, steam white, copper, coal black
- **Shadow Colors**: Near-black iron, deep soot
- **Mood**: Machines and smoke, dark satanic mills, progress at a cost, Victorian grit
- **Palette Anchors**: `#383038` (dark iron), `#904028` (brick), `#E87020` (furnace), `#C8C0B0` (steam)
- **Destructible Materials**:
  - Soft: Wooden scaffold (DS) -- rough timber framework, collapses into boards
  - Medium: Brick wall (DM) -- industrial brick, mortar crumbles, bricks tumble
  - Hard: Cast iron plate (DH) -- thick foundry iron, cracks along casting seams, heavy sparks

### Era 7: Modern (Fully Defined Above)

- **Dominant Tone**: Cold concrete greys, asphalt, utilitarian
- **Accent Colors**: Neon pink and blue, hazard yellow, electrical spark white
- **Shadow Colors**: Cold dark blue-greys, near-black
- **Mood**: Urban jungle, infrastructure, neon nights, functional brutality
- **Destructible Materials**: Drywall (fragile), concrete (structural), reinforced steel (industrial fortress)
- **Debris Style**: Concrete chunks with exposed rebar, drywall dust, steel sparks

### Era 8: Digital / Near Future

- **Dominant Tone**: Clean whites, screen-glow blue, minimal surfaces
- **Accent Colors**: Electric blue, data green, holographic shimmer, LED indicators
- **Shadow Colors**: Cool dark slate, deep digital blue
- **Mood**: Sleek technology, sterile environments, information overload, cyber aesthetics
- **Palette Anchors**: `#E0E8F0` (clean surface), `#2080E0` (interface blue), `#20E070` (data green), `#181828` (dark tech)
- **Destructible Materials**:
  - Soft: Glass panel (DS) -- smart glass, shatters into geometric fragments
  - Medium: Composite wall (DM) -- advanced composite material, delaminates in layers
  - Hard: Blast door (DH) -- reinforced alloy, servo mechanisms spark and fail

### Era 9: Space Age

- **Dominant Tone**: Alien purples, cosmic blues, hull metal grey
- **Accent Colors**: Bioluminescent green, plasma orange, starfield white
- **Shadow Colors**: Deep space black, hull shadow
- **Mood**: Alien worlds, space stations, cosmic isolation, extraterrestrial wonder
- **Palette Anchors**: `#5030A0` (alien purple), `#2050C0` (cosmic blue), `#40E060` (bioluminescent), `#C8C8D0` (hull metal)
- **Destructible Materials**:
  - Soft: Alien membrane (DS) -- organic xenobiological wall, tears and dissolves
  - Medium: Hull plating (DM) -- spacecraft hull, buckles and vents atmosphere
  - Hard: Bulkhead door (DH) -- emergency bulkhead, deforms under extreme force

### Era 10: Transcendent (Fully Defined Above)

- **Dominant Tone**: Deep void purples, impossible darks, crystalline whites
- **Accent Colors**: Electric blue energy, cyan flow, hot pink reality fractures, amber dimensional bleed
- **Shadow Colors**: Absolute void black, deep purple
- **Mood**: Beyond human comprehension, reality unraveling, cosmic transcendence, beautiful terror
- **Destructible Materials**: Reality shards (fragile existence), quantum lattice (structured impossibility), void barriers (compressed nothingness)
- **Debris Style**: Prismatic crystal shards, energy dissipation, reality-distortion particles

### Palette Transition Summary

```
Era  1 (Stone Age)     ||||||||||||||||  Warm browns, firelight orange
Era  2 (Bronze Age)    ||||||||||||||||  Sandy tan, bronze metallic
Era  3 (Iron/Classic)  ||||||||||||||||  Marble white, iron grey, olive
Era  4 (Medieval)      ||||||||||||||||  Castle grey, heraldic red/blue
Era  5 (Renaissance)   ||||||||||||||||  Rich wood, vibrant textile
Era  6 (Industrial)    ||||||||||||||||  Dark iron, brick, furnace orange
Era  7 (Modern)        ||||||||||||||||  Cold concrete, neon accents
Era  8 (Digital)       ||||||||||||||||  Clean white, electric blue
Era  9 (Space Age)     ||||||||||||||||  Alien purple, cosmic blue
Era 10 (Transcendent)  ||||||||||||||||  Void black, energy white, crystal
```

**Overall arc**: Organic warmth --> Manufactured precision --> Alien abstraction --> Cosmic transcendence

---

## Background Tile Treatment

### Depth Layering via Color

Background tiles use a separate sub-palette (slots 16-31) that is deliberately:
- **Desaturated**: 30-50% less color saturation than foreground equivalents
- **Darker**: 20-40% lower value/brightness than foreground equivalents
- **Cooler**: Shifted toward blue/cool tones to suggest atmospheric perspective (except in eras where the background is inherently warm, e.g., Stone Age firelight)

### Depth Examples by Era

| Era | FG Element Color | BG Element Color | Treatment |
|-----|-----------------|------------------|-----------|
| Stone Age | Cave rock #8B7355 | BG cave #2E2418 | -60% brightness, warmer shadows (firelight) |
| Medieval | Castle stone #8E8678 | BG stone #484050 | -45% brightness, cool purple shift |
| Modern | Concrete #888890 | BG building #282830 | -65% brightness, cold blue shift |
| Transcendent | Crystal #A898E0 | BG nebula #281850 | -55% brightness, deeper saturation |

---

## Tile Sheet Layout

### Per-Era Tile Sheet (256x128 PNG = 32 columns x 16 rows = 512 tile slots)

Each era uses a single 256x128 tile sheet with the following layout:

```
Row  0:  Ground tiles (IDs 00-10)     [11 tiles] + [5 empty]  [columns 0-15]
Row  1:  Destructible tiles (IDs 11-18) [8 tiles] + [8 empty]  [columns 0-15]
Row  2:  Platforms + Hazards (IDs 19-23) [5 tiles] + [11 empty] [columns 0-15]
Row  3:  BG tiles (IDs 24-27) + Deco (IDs 28-31) [8 tiles] + [8 empty] [columns 0-15]
Row  4:  Destructible DAMAGED states    [columns 0-15]
Row  5:  Destructible CRITICAL states   [columns 0-15]
Row  6:  Animated tile frame 2 variants [columns 0-15]
Row  7:  Animated tile frame 3 variants [columns 0-15]
Row  8:  Animated tile frame 4 variants [columns 0-15]
Rows 9-15: [Reserved for future tiles, alternate textures, seasonal/weather variants]

--- Extended columns 16-31 (right half of sheet) ---
Columns 16-31, Rows 0-3:  Extended 8-bit auto-tile variants (47 configurations)
Columns 16-31, Rows 4-8:  Additional destructible damage variants for auto-tile configs
Columns 16-31, Rows 9-15: [Reserved]
```

Total available: 512 tile slots per era. Currently using approximately 80-120 slots per era (base tiles + damage states + animation frames + auto-tile variants). This provides substantial room for expansion.

### Tile ID Prefix Convention

| Era | ID Prefix | Example |
|-----|-----------|---------|
| Era 1: Stone Age | SA | SA00, SA11, SA28 |
| Era 2: Bronze Age | BA | BA00, BA11 |
| Era 3: Iron/Classical | IC | IC00, IC11 |
| Era 4: Medieval | MV | MV00, MV11 |
| Era 5: Renaissance | RN | RN00, RN11 |
| Era 6: Industrial | IN | IN00, IN11 |
| Era 7: Modern | MD | MD00, MD11 |
| Era 8: Digital | DG | DG00, DG11 |
| Era 9: Space Age | SP | SP00, SP11 |
| Era 10: Transcendent | TR | TR00, TR11 |

---

## Memory Budget

### Per-Era Budget

| Asset | Dimensions | Bit Depth | Size (Uncompressed) | Size (PNG Compressed) |
|-------|-----------|-----------|---------------------|----------------------|
| Era tileset | 256x128 | 8-bit indexed | 32 KB | ~8 KB |
| Era palette | 32 entries | 24-bit RGB | 96 bytes | ~96 bytes |
| Damage state variants | (included in tileset) | -- | -- | -- |
| Animation frame variants | (included in tileset) | -- | -- | -- |
| **Total per era** | -- | -- | ~32 KB | ~8 KB |

### Total Game Budget (All 10 Eras)

| Asset | Count | Per-Unit Size | Total Size |
|-------|-------|---------------|------------|
| Era tilesets | 10 | ~8 KB (compressed) | ~80 KB |
| Era palettes | 10 | 96 bytes | 960 bytes |
| **Grand total (tilesets)** | -- | -- | **~81 KB** |

Well within the 1 MB per era budget and trivial on modern iOS hardware. Only one era is loaded into active memory at a time; others are loaded on demand during era transitions.

### Runtime Memory (Per Era, In GPU Memory)

| Asset | Format | GPU Memory |
|-------|--------|------------|
| Tileset texture (decompressed) | RGBA32 | 256 x 128 x 4 = 128 KB |
| Tile collision data | byte array | 512 bytes |
| Tile HP/state data | byte array | 512 bytes |
| Auto-tile lookup table | int array | ~256 bytes |
| **Total per era (runtime)** | -- | **~129 KB** |

---

## Unity Integration Notes

### Tilemap Setup

- Use Unity `Tilemap` component with `Grid` (cell size 1x1 Unity units with 8 PPU on tile sprites)
- Sorting layers (back to front):
  1. `Background` (BG tiles, parallax layers)
  2. `BackgroundDecor` (BG architectural details, distant elements)
  3. `Gameplay` (ground, platforms, hazards, destructibles)
  4. `ForegroundDecor` (decorative tiles on gameplay layer)
  5. `Foreground` (overlay effects, particles, debris)

### Destructible Tile Integration

```csharp
// Destructible tile data structure concept
struct DestructibleTileData {
    TileBase intactTile;        // Full HP sprite
    TileBase damagedTile;       // Mid HP sprite (null for DS type)
    TileBase criticalTile;      // Low HP sprite (null for DS type)
    int maxHP;                  // 1, 3, or 5
    int currentHP;              // Tracked per-instance in tilemap
    CollisionType collisionType; // DS, DM, or DH
    ParticleProfile debrisProfile; // Era-specific debris colors and shapes
}
```

### Era Palette JSON Format

```json
{
  "era": "stone_age",
  "era_number": 1,
  "version": "2.0",
  "palette_size": 32,
  "colors": [
    { "slot": 0, "name": "Transparent", "hex": null },
    { "slot": 1, "name": "Outline Dark", "hex": "#1A1008" },
    { "slot": 2, "name": "Cave Rock Light", "hex": "#A89070" },
    "..."
  ],
  "tiles": [
    {
      "id": "SA00",
      "name": "cave_floor_flat",
      "collision": "SOLID",
      "destructible": false,
      "animated": false,
      "sheet_position": { "col": 0, "row": 0 },
      "autotile_bitmask_values": [0, 4, 10, 14]
    },
    {
      "id": "SA13",
      "name": "stone_wall",
      "collision": "DESTRUCTIBLE_MEDIUM",
      "destructible": true,
      "max_hp": 3,
      "damage_states": {
        "intact": { "col": 3, "row": 1 },
        "damaged": { "col": 4, "row": 1 },
        "critical": { "col": 5, "row": 1 }
      },
      "debris_palette_slots": [2, 3, 4, 7, 8],
      "animated": false
    },
    "..."
  ]
}
```

### Rule Tile Configuration

Use Unity `RuleTile` or custom `RuleTile` subclass to implement auto-tiling:
- Define tiling rules matching the bitmask tables above
- Each rule specifies neighbor conditions (This, NotThis, Any)
- Output sprite per rule
- Destructible tiles use a custom `DestructibleRuleTile` subclass that:
  - Stores HP and damage state data per tile instance
  - Recalculates bitmask when adjacent tiles are destroyed
  - Swaps sprites between damage state variants while maintaining auto-tile configuration

---

## Animated Tile Specifications

### Universal Animation Frame Storage

Animation frames are stored in rows 6-8 of each era's tile sheet (frame 1 is the base tile in rows 0-3; frames 2-4 are in rows 6-8 at the same column position).

### Per-Era Animated Tiles

#### Stone Age Animations

| Tile | Frames | Frame Duration | Description |
|------|--------|---------------|-------------|
| SA23 (fire_pit_hazard) | 4 | 150ms | Fire flickers: flame pixels shift position and intensity. Frame 1: flame left-leaning. Frame 2: centered. Frame 3: right-leaning. Frame 4: centered tall. |
| SA30 (deco_campfire_glow) | 2 | 400ms | Residual glow pulses: ambient glow pixels (slot 30) alternate between 3px and 4px radius. Subtle warmth oscillation. |

#### Medieval Animations

| Tile | Frames | Frame Duration | Description |
|------|--------|---------------|-------------|
| MV28 (deco_wall_torch) | 4 | 150ms | Torch flame flickers: flame shape shifts through 4 positions. Glow radius contracts/expands by 1px. |
| MV22 (spike_trap) | 2 | 800ms | Spikes retract partially (2px) then extend. Mechanical cycling. |

#### Modern Animations

| Tile | Frames | Frame Duration | Description |
|------|--------|---------------|-------------|
| MD22 (electrical_hazard) | 3 | 200ms | Spark position alternates between 3 locations within junction box. Bright flash frame followed by dim frames. |
| MD30 (deco_computer_screen) | 2 | 500ms | Screen content shifts: "text" dots rearrange, simulating scrolling data. |

#### Transcendent Animations

| Tile | Frames | Frame Duration | Description |
|------|--------|---------------|-------------|
| TR22 (reality_fracture) | 3 | 250ms | Fracture edges pulse: slot 13 pixels shift position by 1px per frame. Interior dimensional bleed colors cycle (slot 28 to 29 to 28). |
| TR23 (energy_vortex) | 4 | 150ms | Vortex rotates: spiral pattern shifts 2 pixels clockwise per frame. Center brightness pulses. |
| TR28 (deco_floating_rune) | 2 | 600ms | Rune glow pulses: aura pixels alternate between visible and transparent. Core symbol remains static. |
| TR31 (deco_void_particle) | 4 | 300ms | Particles drift: each particle shifts 1px in a slow circular pattern over the 4 frames. |
| TR04 (energy_platform_surface) | 2 | 400ms | Energy surface shimmers: highlight pixels (slot 6) shift 1px horizontally, creating a flowing-energy effect. |

---

## Tile Placement Rules for Level Generator

### Universal Rules (All Eras)

1. **Ground tiles** must always appear at the TOP of terrain masses. Never place surface tiles below fill tiles.
2. **Fill tiles** fill the interior of terrain. Minimum terrain thickness: 2 tiles (1 surface + 1 fill) for visual solidity.
3. **Slopes** connect terrain at different heights. Maximum slope run: 3 tiles. Always cap slope top with flat surface tile.
4. **Platforms** float independently. Minimum width: 2 tiles (left + right) or 3 tiles (left + mid + right). Never single-tile platforms.
5. **Hazards** always sit on a solid surface (floor-mounted) or hang from a ceiling (ceiling-mounted). Never floating in mid-air.
6. **Destructible tiles** may be placed anywhere SOLID tiles can be placed. They auto-tile with adjacent solid and destructible tiles.
7. **Destructible density**: Maximum 40% of terrain in any screen-width section should be destructible. Players need some indestructible terrain to stand on.
8. **Background tiles** fill the background layer. No gaps -- every visible background cell should have a BG tile.
9. **Decorative tiles** placed randomly on empty cells adjacent to ground. Maximum density: 1 decorative tile per 4 ground tiles. No decorations within 1 tile of hazards.

### Era-Specific Placement Guidelines

**Stone Age**: Cave environments with ceilings. Both floor and ceiling terrain. Stalactites on ceilings. Fire pits in sheltered areas. Cave paintings on background walls near fire sources.

**Medieval**: Castle interiors and exteriors. Vertical construction -- towers, walls, battlements. Torches at regular intervals (every 6-8 tiles on walls). Banners in great halls. Iron gates at chokepoints.

**Modern**: Urban environments. Street level and building interiors. Infrastructure visible (pipes, wires) in damaged/open areas. Neon signs on exterior backgrounds. Hazard markings near electrical and chemical hazards.

**Transcendent**: Floating structures in void. Terrain can be discontinuous (void gaps between platforms). Energy bridges connect isolated platforms. Reality fractures placed at narrative-significant locations. Runes scattered as environmental storytelling.

---

**Version**: 2.0
**Last Updated**: 2026-02-04
**Status**: Active
**Specification**: Era-Based Tileset & Visual Theme System
