# Assessment 3.1: Progression System Design

## Overview

Complete progression system specification for a 16-bit side-scrolling mobile shooter (iOS) where a small character collects weapon attachments that auto-fire from their body, blasting through destructible environments across 10 eras of human civilization. The system sustains engagement across 15+ hours of gameplay through three nested loops, four progression layers, a weapon attachment growth system, and a 50-level unlock schedule spanning the Stone Age to the Transcendent era. All systems integrate with the deterministic level generation pipeline (Level ID format: `LVLID_[VERSION]_[ERA]_[LEVEL]_[SEED64]`).

**Controls reference**: Left thumb D-pad (8-directional movement), right thumb Jump button + Target Cycle button. Weapon attachments auto-fire toward the nearest or cycled target.

---

## 1. Core Loop Architecture

### 1.1 The 30-Second Core Loop (Moment-to-Moment)

**Cycle**: Move --> Encounter Wall/Enemy --> Blast --> Collect Weapon Drops --> Move

| Phase | Duration | Player Action | System Response |
|-------|----------|--------------|-----------------|
| Move | 3-5s | D-pad directional input + jump | Scrolling terrain, parallax backgrounds of current era, ambient SFX |
| Encounter | 2-3s | Approach destructible wall or enemy cluster | Destruction target highlighted, auto-fire begins when in range |
| Blast | 3-8s | Aim via target cycle, position for optimal fire angles | Debris VFX, chunk destruction physics, enemy hit feedback, screen shake |
| Collect | 1-3s | Move through drop field | Weapon attachment pickups magnetize to player, "+ATK" popups, haptic tap |
| Reset | 1-2s | Resume movement through cleared path | Brief open corridor, dust settling particles, score tally ticking up |

**Key metrics**:
- Average destruction events per minute: 5-7 (Stone Age), 8-10 (Medieval), 10-14 (Modern), 14-18 (Transcendent)
- Time between weapon drops (any type): never exceeds 10 seconds
- Auto-fire rate: 1-3 shots/second (base), scales with attachment count and tier
- Player input frequency: 1-3 inputs/second (movement + jump + target cycle during combat)

**Feedback density**: Every player action and auto-fire hit produces a response within 1 frame (16.67ms at 60fps):
- Jump: sprite squash on takeoff, stretch at apex, dust particles on land
- Wall destruction: chunk particles fly outward based on material type, camera micro-shake (1-2px, 80ms)
- Enemy hit: 2-frame hit-stop, enemy flash white, knockback with sprite spin
- Weapon pickup: attachment sprite orbits player briefly then snaps to slot, slot glow pulse, chime plays
- Damage taken: screen flash red (50ms), haptic vibrate (medium), health bar pulse, brief slowdown (100ms)

### 1.2 The 5-Minute Session Loop (Per-Level)

**Structure**: Enter Level --> Blast Through Zones --> Boss --> Score --> Decide

| Phase | Duration | Description |
|-------|----------|-------------|
| Level Entry | 5-8s | Level ID displayed, era title card with historical silhouette art, brief safe landing zone |
| Zone 1: Approach | 30-45s | Light destructible barriers, 1-2 enemy types, teach era-specific material (10% of level width) |
| Zone 2: Gauntlet | 45-60s | Dense destructible maze, multiple paths through walls, weapon drops concentrated (25% of level) |
| Zone 3: Assault | 60-90s | Heavy enemy presence behind fortified walls, environmental destruction puzzles (30% of level) |
| Zone 4: Breach | 45-60s | Combined destruction + platforming at peak density, chain-destruction sequences (20% of level) |
| Zone 5: Boss Arena | 45-75s | Era boss with destructible arena elements, multi-phase encounter (15% of level) |
| Score Screen | 10-15s | Score breakdown, star rating (1-3), weapon attachment rewards, share button |
| Decision Point | 5s | "Next Level" / "Retry" / "Share Level ID" / "Main Menu" |

**Total session**: 4-6 minutes per level (scales with era and destruction density)

**Session loop reinforcement**:
- Unfinished boss = strong pull to retry (Zeigarnik effect)
- Score just under 3-star threshold = retry motivation
- New weapon attachment visible on score screen but not yet equipped = forward momentum to try it
- "Share Level ID" button visible on score screen = social hook
- "Next Level" button pulsates with era-themed glow = progression pull

### 1.3 The Multi-Session Meta Loop (Cross-Session)

**Cycle**: Set Goal --> Play Sessions --> Earn Progress --> Unlock New Era/Weapons --> Set New Goal

| Layer | Timeframe | Examples |
|-------|-----------|---------|
| Era Progression | Days 1-14 | Advance through Stone Age --> Bronze Age --> ... --> Transcendent |
| Weapon Collection | Days 1-14 | Discover and upgrade 30+ weapon attachment types |
| Attachment Slot Growth | Days 1-10 | Unlock slots 2 through 6, build unique loadouts |
| Achievement Hunting | Days 3-30+ | Complete achievement categories (Destroyer, Collector, Speedrunner) |
| Leaderboard Climbing | Days 7-30+ | Climb global/friends/weekly leaderboards per era |
| Daily Challenges | Daily | Unique seed each day, era-themed modifiers, streak tracking |
| Level Sharing | Ongoing | Share level IDs, compare destruction scores, challenge friends |
| Cosmetic Collection | Days 7-30+ | Unlock character skins, weapon VFX, destruction particle themes |
| Mastery Pursuit | Days 14+ | 100% destruction runs, speedrun targets, no-damage clears |

**Cross-loop integration**:
```
30s Loop --> Better aim/positioning --> Higher destruction % in 5min Loop
5min Loop --> Level completion --> Era advancement in Meta Loop
Meta Loop --> New weapons/eras unlocked --> Fresh 30s destruction encounters
```

---

## 2. Progression Layers

### 2.1 Skill Progression (Player Mastery)

The player improves mechanically through practice. No stats gate this -- pure skill growth.

| Skill | How It Develops | Observable Improvement |
|-------|----------------|----------------------|
| Destruction routing | Reading wall layouts | Faster level completion, higher destruction % |
| Weapon combo awareness | Experimenting with attachment synergies | More efficient kills, chain-destruction streaks |
| Target cycling speed | Combat encounters | Prioritizing dangerous enemies, less damage taken |
| Positional mastery | Boss retries and dense gauntlets | Optimal fire angles, fewer wasted shots |
| Destruction puzzle solving | Environmental puzzles | Finding chain-reaction paths through complex structures |
| Speedrunning | Repeat plays | Optimal blast paths, skip discovery via creative destruction |

**Skill milestones** (tracked but not gated):
- First 100% destruction level clear
- First 3-star rating
- First boss defeated without taking damage
- First level completed under par time
- First 5-chain destruction combo (destroying 5 structures in a single blast sequence)
- First perfect run (no damage, 100% destruction, all secrets, under par time)

### 2.2 Content Progression (Unlockable World Content)

New content unlocks at a steady cadence to prevent staleness. The player always encounters something new within 1-2 levels.

| Unlock Category | Total Items | Unlock Rate |
|----------------|-------------|-------------|
| Eras | 10 (Stone through Transcendent) | 1 per 5 levels |
| Weapon attachment types | 30+ | 1-2 per level across first 30 levels |
| Enemy types | 25 (escalating per era) | 1 per 2 levels |
| Boss types | 10 (1 per era) | 1 per 5 levels |
| Destructible material types | 15+ | 1-2 per era |
| Attachment slots | 6 total (start with 1) | 1 at levels 5, 10, 20, 30, 40 |
| Abilities | 5 major | At levels 10, 15, 25, 35, 45 |

### 2.3 Power Progression (Weapon Loadout & Abilities)

Power grows through two systems: weapon attachment slots and permanent abilities. No consumable upgrades, no stat inflation -- growth comes from combinatorial depth.

**Attachment Slot Progression**:

| Slot | Unlock Point | Effect on Gameplay |
|------|-------------|-------------------|
| Slot 1 | Level 1 (start) | Single weapon fires forward; simple, directed destruction |
| Slot 2 | Level 5 (Era 1 Boss clear) | Two weapons: player begins combining fire angles |
| Slot 3 | Level 10 (Era 2 Boss clear) | Three weapons: meaningful loadout decisions emerge |
| Slot 4 | Level 20 (Era 4 Boss clear) | Four weapons: complex synergies, area denial builds possible |
| Slot 5 | Level 30 (Era 6 Boss clear) | Five weapons: screen coverage builds, crowd control dominance |
| Slot 6 | Level 40 (Era 8 Boss clear) | Six weapons: full loadout, master-tier builds |

**Ability Unlocks**:

| Ability | Unlock Point | Effect |
|---------|-------------|--------|
| Double Jump | Level 10 (Era 2 clear) | Second jump at apex, access higher destruction targets and platforms |
| Dash | Level 15 (Era 3 midpoint) | Horizontal burst movement, i-frames during dash, blast through debris |
| Ground Slam | Level 25 (Era 5 midpoint) | Downward slam that damages enemies and shatters floor materials below |
| Wall Cling | Level 35 (Era 7 midpoint) | Cling to vertical surfaces, aim weapons from walls, access hidden paths |
| Phase Shift | Level 45 (Era 9 midpoint) | Brief intangibility, pass through thin walls, dodge boss attacks |

**Design constraint**: Abilities expand the player's movement vocabulary and open new destruction paths but never trivialize earlier content. Each ability is also a new skill to master.

### 2.4 Narrative Progression (Visual Storytelling Through Eras)

Light environmental storytelling -- no cutscenes, no text dumps. Human civilization's evolution tells the story through the world itself.

| Era | Setting | Narrative Thread |
|-----|---------|-----------------|
| Stone Age (1-5) | Caves, open wilderness | Humanity's first tools -- raw survival against nature |
| Bronze Age (6-10) | Early settlements, rivers | Civilization forms -- walls go up, weapons get sharper |
| Iron Age (11-15) | Fortified towns, forests | Conflict escalates -- armies clash, iron breaks bronze |
| Medieval (16-20) | Castles, cathedrals, villages | Power consolidates -- massive stone fortifications, siege warfare |
| Renaissance (21-25) | Cities, harbors, universities | Knowledge explodes -- gunpowder changes everything |
| Industrial (26-30) | Factories, rail yards, mines | Machines of war -- steam-driven destruction, smoke and metal |
| Modern (31-35) | Urban sprawl, highways, bunkers | Total war -- concrete, ballistics, precision destruction |
| Digital (36-40) | Server farms, smart cities, labs | Information age -- energy barriers, digital defenses, hacking |
| Space Age (41-45) | Orbital stations, alien worlds | Beyond Earth -- exotic materials, energy weapons, zero gravity |
| Transcendent (46-50) | Reality-bending, abstract spaces | Post-physical -- the nature of reality itself is destructible |

**Storytelling methods**:
- Background parallax evolution (open sky --> settlements --> cities --> digital grids --> abstract void)
- Destruction VFX shifts (rock chips --> bronze shards --> iron sparks --> glass fragments --> data particles)
- Enemy design arc (animals --> warriors --> soldiers --> drones --> energy constructs)
- Music progression (drums --> strings --> brass --> orchestral --> electronic --> ethereal)
- Weapon aesthetic evolution (crude --> forged --> machined --> energy-based --> reality-warping)

---

## 3. 10-Era Unlock Schedule (50 Levels)

### Era 1: Stone Age (Levels 1-5) -- Starter Era
**Setting**: Caves, open rocky terrain, forests | **Materials**: Dirt, loose rock, wood | **Target**: First-time players

| Level | New Unlock | Category | Notes |
|-------|-----------|----------|-------|
| 1 | D-pad movement + Jump + Forward Shot | Core | Tutorial: move, jump, blast dirt walls. Auto-fire demonstrated with pre-equipped Rock Launcher (Slot 1) |
| 2 | Bone Shard Thrower (weapon) + Cave Bat (enemy) | Weapon / Enemy | First weapon drop: 3-shot spread. Bat is slow patrol, 1 HP, no projectiles |
| 3 | Slingshot (weapon) + Dirt Walls (destructible) | Weapon / Material | Arcing shots that hit behind cover. Dirt walls crumble in 2 hits |
| 4 | Club Spinner (weapon) + Wolf (enemy) | Weapon / Enemy | Melee-range rotational attachment. Wolf charges, 2 HP, telegraphed lunge |
| 5 | **Slot 2 unlocked** + **Cave Bear (Era Boss)** | Slot / Boss | Boss: 3 phases in collapsing cave arena. Drops Bone Crown cosmetic. Slot 2 enables dual-weapon builds |

### Era 2: Bronze Age (Levels 6-10) -- First Escalation
**Setting**: River settlements, early walls, farms | **Materials**: Mud brick, thatch, bronze plate | **Target**: Committed players

| Level | New Unlock | Category | Notes |
|-------|-----------|----------|-------|
| 6 | Bronze Dart Launcher (weapon) + Mud Walls (material) | Weapon / Material | Faster projectile, penetrates thatch. Mud walls take 3 hits, crumble with dust VFX |
| 7 | Flame Pot (weapon) + Spear Guard (enemy) | Weapon / Enemy | Lobbed fire AoE that burns thatch instantly. Spear Guard has shield (must hit from side/behind), 3 HP |
| 8 | Bronze Boomerang (weapon) + Thatch Roof (material) | Weapon / Material | Returns after thrown, hits twice. Thatch collapses downward when destroyed |
| 9 | Sling Whip (weapon) + Chariot Rider (enemy) | Weapon / Enemy | Continuous sweeping arc. Chariot is fast-moving, 2 HP, drops weapon on defeat |
| 10 | **Slot 3 + Double Jump unlocked** + **Bronze Colossus (Era Boss)** | Slot / Ability / Boss | Boss: 4 phases, throws bronze spears, arena walls crumble to reveal attack paths. Drops Bronze Helm cosmetic |

### Era 3: Iron Age (Levels 11-15) -- Complexity Deepens
**Setting**: Fortified towns, watchtowers, forests | **Materials**: Iron plate, reinforced wood, stone block | **Target**: Engaged players

| Level | New Unlock | Category | Notes |
|-------|-----------|----------|-------|
| 11 | Iron Bolt Repeater (weapon) + Iron Plate (material) | Weapon / Material | Rapid-fire bolts. Iron plate requires 5 hits or explosives; sparks on impact |
| 12 | Catapult Attachment (weapon) + Shield Wall (enemy) | Weapon / Enemy | Lobs heavy projectiles over walls. Shield Wall is a stationary barrier enemy, 6 HP, blocks forward shots |
| 13 | Chain Flail (weapon) + Reinforced Wood (material) | Weapon / Material | Swinging arc with extended range. Reinforced wood splinters spectacularly |
| 14 | Fire Arrow Launcher (weapon) + Berserker (enemy) | Weapon / Enemy | Ignites wooden materials on hit. Berserker is fast, 4 HP, enrages at half health |
| 15 | **Dash ability unlocked** + **Iron Warlord (Era Boss)** | Ability / Boss | Boss: 4 phases, commands waves of soldiers, destroyable fortress arena. Drops Iron Crown cosmetic. Dash enables blast-and-dash combat style |

### Era 4: Medieval (Levels 16-20) -- Fortified Destruction
**Setting**: Castles, cathedral spires, village squares | **Materials**: Cut stone, mortar wall, stained glass, heavy wood | **Target**: Skilled players

| Level | New Unlock | Category | Notes |
|-------|-----------|----------|-------|
| 16 | Crossbow Turret (weapon) + Cut Stone (material) | Weapon / Material | High-damage single bolt, slow fire rate. Cut stone requires 6+ hits, cracks visible at 50% |
| 17 | Tar Bomb (weapon) + Armored Knight (enemy) | Weapon / Enemy | Slows enemies, ignitable by fire weapons for AoE. Knight has full armor, 6 HP, charges with lance |
| 18 | Battering Ram (weapon) + Stained Glass (material) | Weapon / Material | Forward-only, massive impact damage to structures. Stained glass shatters in colorful shards (1 hit) |
| 19 | Holy Water Spray (weapon) + Dark Priest (enemy) | Weapon / Enemy | Continuous spray, effective vs undead enemies. Dark Priest summons skeletons, 5 HP, teleports |
| 20 | **Slot 4 unlocked** + **Dragon (Era Boss)** | Slot / Boss | Boss: 5 phases, breathes fire that destroys arena stone walls, reveals attack paths as arena crumbles. Drops Dragon Scale Armor cosmetic |

### Era 5: Renaissance (Levels 21-25) -- Gunpowder Revolution
**Setting**: Plazas, harbors, university courtyards, ornate buildings | **Materials**: Ornate plaster, marble, timber frame, glass pane | **Target**: Dedicated players

| Level | New Unlock | Category | Notes |
|-------|-----------|----------|-------|
| 21 | Musket Attachment (weapon) + Ornate Plaster (material) | Weapon / Material | Long-range, high damage, slow reload. Plaster crumbles with chunk physics, reveals brick beneath |
| 22 | Cannon Ball (weapon) + Privateer (enemy) | Weapon / Enemy | Heavy arc projectile, massive AoE destruction. Privateer has dual pistols, 5 HP, rolls to dodge |
| 23 | Powder Keg (weapon) + Marble Column (material) | Weapon / Material | Placed explosive with 2s fuse, chain-detonates nearby kegs. Marble cracks and topples |
| 24 | Clockwork Turret (weapon) + Inventor (enemy) | Weapon / Enemy | Placeable auto-turret that persists for 15s. Inventor deploys traps, 4 HP, retreats when hurt |
| 25 | **Ground Slam ability unlocked** + **War Galleon (Era Boss)** | Ability / Boss | Boss: 5 phases, naval cannon barrage, destructible ship hull exposes weak points. Drops Captain's Coat cosmetic. Ground Slam enables floor-shattering shortcuts |

### Era 6: Industrial (Levels 26-30) -- Machine Age
**Setting**: Factories, rail yards, coal mines, steel bridges | **Materials**: Brick, corrugated metal, riveted steel, glass block | **Target**: Experienced players

| Level | New Unlock | Category | Notes |
|-------|-----------|----------|-------|
| 26 | Steam Cannon (weapon) + Brick Wall (material) | Weapon / Material | Charged shot (hold = more power), pierces multiple walls at max charge. Brick crumbles row by row |
| 27 | Rivet Gun (weapon) + Automaton (enemy) | Weapon / Enemy | Rapid-fire with ricochet off metal surfaces. Automaton is slow, armored, 8 HP, explodes on death |
| 28 | Tesla Coil (weapon) + Riveted Steel (material) | Weapon / Material | Chain lightning jumps between metal surfaces and enemies. Riveted steel conducts (takes 2x damage from electric) |
| 29 | Dynamite Launcher (weapon) + Mine Foreman (enemy) | Weapon / Enemy | Arcing explosive, massive destruction radius. Foreman commands automatons, 6 HP, armored helmet |
| 30 | **Slot 5 unlocked** + **Iron Leviathan (Era Boss)** | Slot / Boss | Boss: 6 phases, steam-powered mech with destructible limbs, factory arena with conveyor hazards. Drops Brass Goggles cosmetic |

### Era 7: Modern (Levels 31-35) -- Total War
**Setting**: Urban streets, highways, military bunkers, skyscrapers | **Materials**: Concrete, rebar-reinforced walls, bulletproof glass, asphalt | **Target**: Advanced players

| Level | New Unlock | Category | Notes |
|-------|-----------|----------|-------|
| 31 | Assault Rifle Attachment (weapon) + Concrete (material) | Weapon / Material | High fire rate, moderate damage. Concrete cracks in web pattern, takes 8+ hits, rebar exposed mid-destruction |
| 32 | Rocket Launcher (weapon) + Infantry Squad (enemy) | Weapon / Enemy | Slow, devastating AoE. Infantry operates as a squad of 3, flanking AI, 3 HP each |
| 33 | Grenade Launcher (weapon) + Rebar Wall (material) | Weapon / Material | Bouncing grenades that detonate on 2nd surface contact. Rebar bends and snaps with sparks |
| 34 | Minigun Attachment (weapon) + Tank (enemy) | Weapon / Enemy | Spin-up mechanic: damage ramps up over time. Tank has 12 HP, turret tracks player, destructible treads disable movement |
| 35 | **Wall Cling ability unlocked** + **Mecha Commander (Era Boss)** | Ability / Boss | Boss: 6 phases, calls airstrikes, urban arena where buildings collapse to create new paths. Drops Tactical Vest cosmetic. Wall Cling enables vertical flanking strategies |

### Era 8: Digital (Levels 36-40) -- Information Warfare
**Setting**: Server rooms, smart cities, neon-lit labs, data centers | **Materials**: Digital barriers (energy walls), fiber-optic mesh, holographic panels, smart glass | **Target**: Veteran players

| Level | New Unlock | Category | Notes |
|-------|-----------|----------|-------|
| 36 | Laser Beam (weapon) + Digital Barrier (material) | Weapon / Material | Continuous beam, burns through targets. Digital barriers require sustained damage (3s of beam), flicker before collapse |
| 37 | EMP Pulse (weapon) + Security Drone (enemy) | Weapon / Enemy | AoE that disables digital barriers and stuns electronic enemies for 3s. Drone has 4 HP, fires tracking shots |
| 38 | Plasma Cutter (weapon) + Fiber-Optic Mesh (material) | Weapon / Material | Precision beam that cuts clean lines through materials. Mesh frays and sparks when cut |
| 39 | Hacking Probe (weapon) + AI Sentinel (enemy) | Weapon / Enemy | Converts enemy turrets to fight for you for 10s. Sentinel has 8 HP, deploys energy shields, adapts to player attack patterns |
| 40 | **Slot 6 unlocked** + **Mainframe Core (Era Boss)** | Slot / Boss | Boss: 6 phases, reconfigures arena barriers in real-time, spawns digital copies of previously fought enemies. Drops Neon Visor cosmetic. Full 6-slot loadout enables ultimate build diversity |

### Era 9: Space Age (Levels 41-45) -- Beyond Earth
**Setting**: Orbital stations, alien planets, asteroid fields, zero-g corridors | **Materials**: Alien alloy, energy membrane, crystalline lattice, asteroid rock | **Target**: Elite players

| Level | New Unlock | Category | Notes |
|-------|-----------|----------|-------|
| 41 | Photon Blaster (weapon) + Alien Alloy (material) | Weapon / Material | Light-speed projectile, no travel time. Alien alloy fractures in geometric patterns, requires 10+ hits |
| 42 | Gravity Well (weapon) + Xenomorph (enemy) | Weapon / Enemy | Creates a vortex that pulls debris and enemies inward. Xenomorph is fast, 6 HP, clings to walls and ceilings |
| 43 | Antimatter Lance (weapon) + Crystalline Lattice (material) | Weapon / Material | Pierces all materials but fires very slowly (1 shot / 3s). Crystal shatters into prismatic shards |
| 44 | Swarm Nanobots (weapon) + Alien Commander (enemy) | Weapon / Enemy | Cloud of autonomous bots that eat through materials over time. Commander has 10 HP, personal shield, summons alien troops |
| 45 | **Phase Shift ability unlocked** + **Hive Mother (Era Boss)** | Ability / Boss | Boss: 7 phases, organic arena that regenerates walls, spawns waves of xenomorphs. Drops Stellar Mantle cosmetic. Phase Shift enables passing through thin barriers |

### Era 10: Transcendent (Levels 46-50) -- Reality Unbound
**Setting**: Abstract geometry, reality fractures, impossible architecture, void spaces | **Materials**: Reality fabric, void crystal, thought-form walls, paradox barriers | **Target**: Completionists / mastery seekers

| Level | New Unlock | Category | Notes |
|-------|-----------|----------|-------|
| 46 | Reality Ripper (weapon) + Reality Fabric (material) | Weapon / Material | Tears holes in the environment that enemies fall through. Reality fabric ripples and distorts before breaking |
| 47 | Entropy Wave (weapon) + Thought Guardian (enemy) | Weapon / Enemy | Expanding wave that ages/decays everything it touches. Guardian exists in multiple positions simultaneously, 8 HP per instance |
| 48 | Singularity Projector (weapon) + Void Crystal (material) | Weapon / Material | Creates a black hole that consumes all materials within radius. Void crystal inverts color space when destroyed |
| 49 | All weapons available + Gauntlet of all enemy types | Gauntlet | Every material type, every enemy type, ultimate skill test. No new unlocks -- pure mastery challenge |
| 50 | **The Architect (Final Boss)** | Boss | 8-phase final boss: rewrites level geometry mid-fight, phases reference each era's boss mechanics, ultimate destruction puzzle. Drops "Transcendent" title + Reality Crown cosmetic |

### Unlock Cadence Verification

| Era (Level Range) | New Weapons | New Enemies | New Materials | New Abilities | New Slots | Boss | Total New Content |
|-------------------|-------------|-------------|---------------|---------------|-----------|------|-------------------|
| Stone Age (1-5) | 4 | 2 | 2 | 0 | 1 | 1 | 10 |
| Bronze Age (6-10) | 4 | 2 | 2 | 1 | 1 | 1 | 11 |
| Iron Age (11-15) | 4 | 2 | 2 | 1 | 0 | 1 | 10 |
| Medieval (16-20) | 4 | 2 | 2 | 0 | 1 | 1 | 10 |
| Renaissance (21-25) | 4 | 2 | 2 | 1 | 0 | 1 | 10 |
| Industrial (26-30) | 4 | 2 | 2 | 0 | 1 | 1 | 10 |
| Modern (31-35) | 4 | 2 | 2 | 1 | 0 | 1 | 10 |
| Digital (36-40) | 4 | 2 | 2 | 0 | 1 | 1 | 10 |
| Space Age (41-45) | 4 | 2 | 2 | 1 | 0 | 1 | 10 |
| Transcendent (46-50) | 3 | 1 | 2 | 0 | 0 | 1 | 7 |
| **Totals** | **39** | **19** | **20** | **5** | **5** | **10** | **98** |

**Result**: Every level introduces at least one new element (weapon, enemy, material, ability, slot, or boss). No unlock gaps exceed 1 level. Average of 1.96 new content pieces per level across the full game.

---

## 4. First 15 Minutes Experience (Minute-by-Minute)

### Minute 0:00 - 0:30 | App Launch & Title Screen
- App opens to a pixel-art title screen: scrolling prehistoric landscape parallax, 16-bit chiptune theme with primal drums
- Game title rendered in stylized stone-carved pixel font
- Single button: "TAP TO START" (pulsing gently with warm glow)
- No account creation, no login wall, no permissions popup yet
- Player taps, screen transitions with a rock-crumble pixel-wipe effect

### Minute 0:30 - 1:00 | Character Introduction
- Brief non-interactive scene: tiny hero sprite drops from above into a cave entrance (3 seconds)
- Text box (auto-advance, 2 lines): "You are small. But you can carry anything."
- A crude rock launcher attachment drops from the ceiling and snaps to the character
- The weapon auto-fires once at a dirt wall, blasting a hole -- the core mechanic demonstrated in 1 second
- HUD elements fade in: HP bar (top-left), score counter (top-right), D-pad (bottom-left), Jump + Target Cycle buttons (bottom-right)
- Attachment slot display appears on character sprite: Slot 1 glows with the Rock Launcher equipped
- Prompt: "USE D-PAD to move" (overlay on left side of screen)

### Minute 1:00 - 2:30 | Level 1: Learning Movement & Auto-Fire
- **Level ID generated**: `LVLID_1_1_1_[SEED64]` (Version 1, Era 1, Level 1, unique seed)
- Cave environment, loose rock walls, no enemies -- impossible to die
- Dirt walls block the forward path every 3-4 tiles; Rock Launcher auto-fires when player faces them
- First wall destroyed: satisfying crumble SFX + debris particles fly + "+50 DESTROY" popup
- The player learns: move forward, weapon fires automatically, walls break, path opens
- Coins embedded in some walls -- destroying walls reveals them (15 total coins hidden in walls)
- First jump prompt at a small gap (1 tile): "TAP JUMP" overlay on right button
- Jump is forgiving: large landing platform on other side
- Level end: stone archway portal, "LEVEL COMPLETE" fanfare with cave echo
- **Reward #1**: First destruction (dopamine hit within 15 seconds of gameplay)

### Minute 2:30 - 3:30 | First Score Screen
- Score breakdown animates in:
  - Destruction: 87% -- "GOOD!" rating
  - Time: 40s (par: 60s) -- "FAST!" bonus
  - Coins: 12/15 -- "NICE HAUL!" bonus
  - Damage: 0 -- "UNTOUCHED!" bonus
  - Total: 1,450 points
- Star rating: 2/3 stars (2 stars fill with satisfying chime, third star outline pulses -- inviting retry)
- **Reward #2**: First star rating + achievement unlocked: "First Blast" (toast notification)
- Buttons: "Next Level" (large, stone-textured, pulsing) | "Retry" | "Share"
- Player taps "Next Level"

### Minute 3:30 - 5:30 | Level 2: First Weapon Drop & First Enemy
- Level loads instantly (<100ms generation)
- After blasting through the first dirt wall, a glowing pickup drops: **Bone Shard Thrower**
- Player walks over it: attachment sprite orbits character, snaps to Slot 1 (replaces Rock Launcher)
- Prompt: "WEAPONS AUTO-FIRE! Move to aim."
- Bone Shard Thrower fires 3-shot spread -- noticeably different from Rock Launcher
- Second dirt wall section: first **Cave Bat** patrols behind it
- Bat is slow, 1 HP, flies in sine wave pattern
- Player's auto-fire hits bat: 2-frame hit-stop, bat flashes white, poof particle, "+100 ENEMY" popup
- Bat drops a new Rock Launcher pickup -- player now has a choice: keep Bone Shard or switch back
- **Reward #3**: First weapon pickup + first enemy defeated + haptic feedback (light tap)
- Level continues with 2-3 more bats and another weapon drop (Slingshot preview that can't be kept yet -- only drops in Level 3)
- **Reward #4**: Achievement unlocked: "Armed and Dangerous" (collect first weapon attachment)

### Minute 5:30 - 7:30 | Level 3: Weapon Variety & Destructible Depth
- Slingshot attachment drops early in level: arcing shots that go OVER walls and hit enemies behind them
- Dirt walls plus new element: slightly tougher rock formations requiring 3 hits instead of 2
- Player learns: different weapons work better on different materials
- More bats, plus coins hidden in ceiling structures (must angle shots upward via positioning)
- Environmental puzzle: a wall blocks progression, but a rock column can be destroyed to collapse debris onto it, clearing the path in one chain reaction
- **Reward #5**: First chain-destruction event: "+200 CHAIN BONUS!" popup with screen shake

### Minute 7:30 - 9:30 | Level 4: Weapon Combos & Wolves
- Club Spinner drops: melee-range rotational weapon -- powerful but requires getting close
- **Wolf enemy introduced**: charges when player is within 5 tiles, telegraphed with growl + exclamation mark (0.5s), then lunges
- Player learns target cycling: TAP TARGET CYCLE to switch auto-fire priority between wolf and destructible wall
- Key moment: wolf charges through a thin dirt wall, destroying it -- enemies interact with destructible environment
- Risk/reward: dangerous upper path through crumbling ceiling leads to hidden weapon cache with rare variant pickup (Bronze Dart Launcher preview)
- Score screen: player likely gets 2 stars (learning curve dip is expected)
- **Reward #6**: First wolf defeated, first target-cycle usage

### Minute 9:30 - 12:00 | Level 5: First Boss + Slot 2 Unlock
- Pre-level: "ATTACHMENT SLOT 2 UNLOCKED" screen with pixel art of the character now carrying two weapons
- Second slot glows on character sprite -- player can now equip TWO attachments simultaneously
- Level starts with a weapon drop for the empty second slot: auto-equips, both weapons now fire
- Player sees the power spike: two weapons firing simultaneously shreds walls and enemies faster
- Level builds toward **Cave Bear** boss arena:
  - Phase 1: Bear charges across arena, player must blast stalactites to drop on it
  - Phase 2: Bear enrages, arena walls crumble, new attack angles open
  - Phase 3: Ceiling collapse sequence, bear exposed, player unloads both weapons
- Boss defeated: massive VFX celebration, "+500 BOSS DEFEATED" popup, Bone Crown cosmetic drops
- **Reward #7**: First boss defeated + Bone Crown cosmetic + Slot 2 now permanent
- **Reward #8**: Achievement unlocked: "Stone Age Complete" + era progress bar fills

### Minute 12:00 - 13:30 | Level 6: Bronze Age Begins
- Era transition screen: stone textures dissolve into bronze/mud palette
- "ERA 2: BRONZE AGE" title card with silhouette of early settlement
- Music shifts: primal drums gain metallic percussion, tempo increases
- Immediately noticeable: **Mud Walls** replace dirt -- take 3 hits instead of 2, crumble differently
- Bronze Dart Launcher drops: faster projectile, penetrates thatch (new material) in one shot
- First **Spear Guard** enemy: carries a shield, blocks frontal attacks, must be hit from side or behind
- Player must use positioning (moving above/below) to angle auto-fire around shields
- New destruction puzzle: thatch roof above an enemy can be collapsed onto them for an instant kill
- **Reward #9**: First era transition, new visual/audio experience, new tactical challenge

### Minute 13:30 - 15:00 | Score Screen & Meta-Loop Hook
- Level 6 complete, score screen displays
- Player has now completed 6 levels, accumulated ~6,500 total score, collected 6+ weapon types
- **Era progress bar appears**: "Bronze Age: 1/5 levels complete" -- shows path to Era 2 boss
- **Weapon collection display**: silhouettes of undiscovered weapons in current era (mystery/anticipation)
- **Attachment loadout screen** (first time): player can swap weapons between Slot 1 and Slot 2 from collected inventory
- **Daily Challenge teaser**: "Daily Challenge available -- Stone Age rules!" notification
- **Social hook**: "Share your Level ID and challenge a friend!" button with level seed visible
- Player sees the meta-loop forming:
  - 4 more Bronze Age levels before the Bronze Colossus boss
  - Boss silhouette shown on era progress map (mystery/anticipation)
  - Weapon collection progress: "4/8 Bronze Age weapons found"
  - Slot 3 preview: "Next slot unlocks at Level 10!" -- clear goal

### 15-Minute Summary

| Minute | Content | Reward |
|--------|---------|--------|
| 0:00-0:30 | Title screen | Aesthetic delight, primal atmosphere |
| 0:30-1:00 | Character intro + first auto-fire demo | Mechanical hook: weapon fires, wall explodes |
| 1:00-2:30 | Level 1: Movement + destruction basics | First destruction feedback, coin collection |
| 2:30-3:30 | Score screen | Star rating, "First Blast" achievement |
| 3:30-5:30 | Level 2: Weapon drops + first enemy | First weapon pickup, first enemy kill, "Armed and Dangerous" |
| 5:30-7:30 | Level 3: Weapon variety + chain destruction | Chain-destruction bonus, puzzle-solving satisfaction |
| 7:30-9:30 | Level 4: Weapon combos + wolves | Target cycling mastery, enemy-environment interaction |
| 9:30-12:00 | Level 5: Slot 2 + Cave Bear boss | Boss defeated, cosmetic drop, Slot 2 unlock, power spike |
| 12:00-13:30 | Level 6: Bronze Age begins | New era experience, new materials and enemies |
| 13:30-15:00 | Meta hooks | Progress bar, weapon collection, daily challenge, social sharing |

**Total rewards in first 15 minutes**: 9+ distinct reward moments (exceeds the 3-minimum requirement)

**Emotional arc**: Curiosity --> Understanding --> Experimentation --> Power Spike --> Challenge --> Discovery --> Anticipation

---

## 5. Session Length Support

### Short Session (5 minutes)
- Play 1 level (4-6 minutes including score screen)
- Blast through a new configuration of destructible environments
- Try a new weapon attachment that dropped last session
- Retry for a better destruction percentage or star rating
- Meaningful progress: at least 1 level completed, weapon inventory expanded

### Medium Session (15 minutes)
- Play 2-3 levels
- Potentially reach a new weapon type or enemy encounter
- Experiment with different attachment loadout combinations
- Attempt the daily challenge
- Explore hidden destruction paths missed in previous levels

### Long Session (30+ minutes)
- Play 5-8 levels
- Complete an entire era and face a boss
- Unlock a new attachment slot or ability
- Optimize loadout builds for upcoming era materials
- Grind for 100% destruction achievements
- Share level IDs and compare scores with friends

### Session Boundary Design
- Game auto-saves at every checkpoint and level completion
- Weapon inventory persists immediately upon pickup (no save required)
- No penalty for closing mid-level (resume from last checkpoint with current loadout intact)
- Score screen is a natural stopping point (clear session end)
- Loadout management screen accessible from main menu (tinker between sessions)
- "Come back tomorrow" is only ever communicated through daily challenge refresh -- never through energy gates or timed lockouts

---

## 6. Weapon Attachment Progression System

### 6.1 How Weapon Loadout Grows Over Time

The weapon attachment system is the primary power fantasy. The player starts as a tiny character with one crude weapon auto-firing forward. By endgame, they are a walking arsenal with 6 weapons covering all angles, each specialized for different destruction tasks.

**Growth curve**:

| Game Phase | Slots | Weapon Count Available | Loadout Complexity | Player Fantasy |
|-----------|-------|----------------------|-------------------|---------------|
| Levels 1-5 | 1 | 4 | Single weapon, swap when better found | Scrappy survivor |
| Levels 6-10 | 2 | 8 | Two-weapon combos, angle coverage | Armed scavenger |
| Levels 11-15 | 3 | 12 | Directional coverage, material specialization | Tactical destroyer |
| Levels 16-20 | 4 | 16 | Synergy builds, crowd control vs single target | Siege engine |
| Levels 21-25 | 4 | 20 | Deep build optimization, explosive chains | Walking armory |
| Levels 26-30 | 5 | 24 | Full-screen coverage, build diversity | One-person army |
| Levels 31-35 | 5 | 28 | Min-maxing for specific material types | Precision weapon platform |
| Levels 36-40 | 6 | 32 | Energy + physical combos, exploit material weaknesses | Total destruction system |
| Levels 41-45 | 6 | 36 | Exotic weapon synergies, reality-bending combos | Transcendent force |
| Levels 46-50 | 6 | 39 | Full mastery, any build viable, perfect loadout | The Architect's equal |

### 6.2 Weapon Attachment Categories

All weapons fall into five categories. An ideal loadout blends categories for maximum destruction versatility.

| Category | Fire Pattern | Best Against | Example Weapons |
|----------|-------------|-------------|----------------|
| **Projectile** | Single or burst shots forward | Enemies at range | Rock Launcher, Bronze Dart, Musket, Photon Blaster |
| **Spread** | Multiple shots in a cone | Groups, wide walls | Bone Shard Thrower, Rivet Gun, Assault Rifle |
| **Arcing** | Lobbed shots over obstacles | Enemies behind cover, ceilings | Slingshot, Flame Pot, Grenade Launcher, Gravity Well |
| **Beam** | Continuous stream | Sustained damage on tough materials | Tesla Coil, Laser Beam, Plasma Cutter, Antimatter Lance |
| **Melee** | Close-range rotational/sweeping | Adjacent enemies, thin walls | Club Spinner, Chain Flail, Battering Ram, Sling Whip |

### 6.3 Weapon Tier System

Each weapon exists in 3 tiers. Higher tiers drop in later eras and have enhanced stats.

| Tier | Drop Availability | Visual Indicator | Stat Multiplier |
|------|------------------|-----------------|----------------|
| Tier 1 (Common) | Eras 1-5 | Gray/brown item glow | 1.0x base |
| Tier 2 (Refined) | Eras 4-8 | Blue item glow | 1.5x damage, +1 projectile or effect |
| Tier 3 (Masterwork) | Eras 7-10 | Gold item glow | 2.0x damage, +2 projectiles or enhanced effect |

**Example tier progression for Rock Launcher**:
- Tier 1: Fires 1 rock, 10 damage, 1 shot/second
- Tier 2: Fires 2 rocks in slight spread, 15 damage each, 1.2 shots/second, rocks pierce 1 material layer
- Tier 3: Fires 3 rocks in spread, 20 damage each, 1.5 shots/second, rocks explode on final impact

### 6.4 Loadout Examples by Era

**Early Game (Era 2, 2 slots)**:
```
Slot 1: Bronze Dart Launcher (Projectile) -- handles distant enemies
Slot 2: Club Spinner (Melee) -- handles close threats and thin walls
Coverage: Forward + adjacent. Player must face threats.
```

**Mid Game (Era 5, 4 slots)**:
```
Slot 1: Musket Attachment (Projectile) -- long-range, high damage
Slot 2: Powder Keg (Arcing) -- area destruction, chain reactions
Slot 3: Fire Arrow Launcher (Projectile) -- ignites wood, crowd control
Slot 4: Chain Flail (Melee) -- sweeps close enemies, extended range
Coverage: Full forward arc + melee safety net. Explosive combo potential.
```

**Late Game (Era 9, 6 slots)**:
```
Slot 1: Photon Blaster (Beam) -- instant-hit precision
Slot 2: Gravity Well (Arcing) -- pulls debris and enemies together
Slot 3: Swarm Nanobots (Spread) -- eats materials over time
Slot 4: Antimatter Lance (Beam) -- pierces all materials
Slot 5: Rocket Launcher T3 (Arcing) -- massive AoE
Slot 6: Tesla Coil T3 (Beam) -- chains between metal targets
Coverage: 360-degree destruction. Every material weakness covered.
```

---

## 7. Difficulty Curve Across Eras

### 7.1 Destructible Environment Complexity Scaling

The core difficulty mechanic is the destructible environment. As eras advance, walls become tougher, puzzles more complex, and the relationship between weapons and materials more strategic.

| Era | Wall HP Range | Materials Active | Puzzle Complexity | Environmental Interactivity |
|-----|--------------|-----------------|-------------------|---------------------------|
| 1 - Stone Age | 1-3 | 2 (Dirt, Loose Rock) | None -- blast everything | Minimal: walls just break |
| 2 - Bronze Age | 2-4 | 4 (+Mud Brick, Thatch) | Simple: thatch burns, mud crumbles wet | Roof collapse onto enemies |
| 3 - Iron Age | 3-6 | 6 (+Iron Plate, Reinforced Wood) | Material-specific: explosives for iron, fire for wood | Chain reactions through mixed-material walls |
| 4 - Medieval | 4-8 | 8 (+Cut Stone, Stained Glass, Heavy Wood) | Multi-step: destroy supports to topple structures | Falling structures crush enemies and open paths |
| 5 - Renaissance | 5-10 | 10 (+Ornate Plaster, Marble, Glass Pane) | Chain-detonation puzzles with powder kegs | Placed explosives create timed destruction sequences |
| 6 - Industrial | 6-12 | 12 (+Brick, Corrugated Metal, Riveted Steel) | Electrical conduction: Tesla chains through metal paths | Conveyor systems move debris, steam pressure puzzles |
| 7 - Modern | 8-15 | 14 (+Concrete, Rebar, Bulletproof Glass, Asphalt) | Structural engineering: support columns, load-bearing walls | Building collapse physics, rubble creates new platforms |
| 8 - Digital | 5-20 | 16 (+Digital Barrier, Fiber-Optic, Holographic, Smart Glass) | Logic puzzles: barriers toggle, hack sequences | Enemy hacking, barrier reconfiguration mid-combat |
| 9 - Space Age | 8-25 | 18 (+Alien Alloy, Energy Membrane, Crystal Lattice) | Zero-G physics: debris floats, chain reactions propagate differently | Gravity manipulation, orbital mechanics affect destruction |
| 10 - Transcendent | 10-30 | 20 (+Reality Fabric, Void Crystal, Thought-Form, Paradox) | Reality puzzles: destroy one wall, another appears; paradox loops | Environment actively resists and adapts to player |

### 7.2 Enemy Scaling

| Era | Enemy HP Range | Enemy AI Complexity | Enemy Count per Level | Special Behaviors |
|-----|---------------|--------------------|-----------------------|------------------|
| 1 | 1-2 | Patrol only | 5-8 | None |
| 2 | 2-3 | Patrol + shield blocking | 8-12 | Shield direction |
| 3 | 3-6 | Patrol + charge + ranged | 10-15 | Enrage at low HP |
| 4 | 4-6 | Patrol + charge + summon | 12-18 | Shield + teleport |
| 5 | 4-5 | Ranged + dodge + retreat | 12-18 | Trap placement |
| 6 | 6-8 | Armored + explode on death | 15-20 | Self-destruct radius |
| 7 | 3-12 | Squad tactics + flanking | 15-22 | Squad coordination |
| 8 | 4-8 | Adaptive AI + energy shields | 12-20 | Pattern adaptation |
| 9 | 6-10 | Wall-cling + summon + shield | 12-18 | Environmental interaction |
| 10 | 8+ | Multiple simultaneous positions | 10-15 | Reality manipulation |

### 7.3 Boss Difficulty Progression

| Era Boss | Phases | Arena Destructibility | Unique Mechanic | Estimated Clear Time |
|----------|--------|----------------------|-----------------|---------------------|
| Cave Bear (Era 1) | 3 | Stalactites drop when shot | Environmental damage only | 45-60s |
| Bronze Colossus (Era 2) | 4 | Walls crumble to reveal paths | Must destroy armor plates before damaging | 60-75s |
| Iron Warlord (Era 3) | 4 | Fortress walls provide cover for both sides | Summons soldier waves, destroy barricades | 60-90s |
| Dragon (Era 4) | 5 | Fire breath destroys stone arena walls | Arena shrinks as walls burn, opening new angles | 75-90s |
| War Galleon (Era 5) | 5 | Ship hull sections are destructible | Destroy deck to expose cannon weak points | 75-90s |
| Iron Leviathan (Era 6) | 6 | Mech limbs are individually destructible | Remove limbs to disable attacks, factory hazards | 90-120s |
| Mecha Commander (Era 7) | 6 | Urban buildings collapse around arena | Airstrikes destroy cover, use rubble as weapons | 90-120s |
| Mainframe Core (Era 8) | 6 | Digital barriers reconfigure each phase | Boss spawns copies of previous bosses as holograms | 90-120s |
| Hive Mother (Era 9) | 7 | Organic arena walls regenerate over time | Must destroy faster than regeneration rate | 120-150s |
| The Architect (Era 10) | 8 | Arena geometry rewrites mid-fight | Each phase uses a different era's mechanics | 150-180s |

### 7.4 Difficulty Tuning Parameters

The procedural generator adjusts these parameters per era and per level within each era:

| Parameter | Era 1 Value | Era 5 Value | Era 10 Value |
|-----------|-------------|-------------|--------------|
| Wall density (% of level area) | 15% | 30% | 45% |
| Enemy density (per 100 tiles) | 2 | 5 | 8 |
| Material mix (types per level) | 1-2 | 3-4 | 5-6 |
| Destruction puzzle frequency | 0 per level | 2 per level | 4 per level |
| Required destruction % to progress | 30% | 50% | 60% |
| Hidden path count | 0-1 | 2-3 | 3-5 |
| Checkpoint spacing (tiles) | Every 80 | Every 120 | Every 160 |
| Par time (seconds) | 60 | 90 | 150 |

---

## 8. Procedural Generation Integration

### How Progression Interacts with Level Generation

The unlock schedule gates which enemies, materials, weapons, and mechanics the level generator can use for a given player:

```
Generator Input:
  - Player's current era (1-10)
  - Current level within era (1-5)
  - Unlocked enemy types (list)
  - Unlocked material types (list)
  - Unlocked weapon drop pool (list)
  - Unlocked abilities (list, affects reachability validation)
  - Current attachment slot count (1-6)
  - Difficulty scaling parameters for current era

Generator Constraint:
  - Only spawn enemies the player has unlocked
  - Only use materials the player has encountered
  - Only drop weapons the player can equip (within slot limit)
  - Validate reachability using player's current ability set
  - Destruction puzzles use only known material interactions
  - New unlock levels use curated seeds (hand-selected for good introduction)
  - Boss levels always use curated arena layouts
```

### Curated vs. Generated Levels

| Level Type | Generation | When Used |
|-----------|-----------|-----------|
| Tutorial (Levels 1-3) | Hand-curated seeds | First playthrough only |
| Era Boss (every 5th level) | Curated arena + procedural approach path | Era completion |
| Standard (all others) | Fully procedural within era constraints | Levels 4, 6-9, 11-14, etc. |
| Daily Challenge | Procedural, shared seed, era-themed | Once daily, same for all players |
| Shared Levels | Procedural, player seed | Shared via Level ID |

### Level ID and Progression

When sharing a Level ID (`LVLID_1_3_2_9876543210ABCDEF`), the receiving player can play the level regardless of their progression. However:
- Enemies and materials they haven't unlocked appear with a "NEW!" indicator and preview tooltip
- Weapon drops are limited to what the receiving player has unlocked (no progression skipping via shared levels)
- The level counts toward their era progression only if they are at the appropriate era
- This prevents progression skipping while allowing social play across all eras

---

## References

- Schell, J. (2008). *The Art of Game Design: A Book of Lenses*. Chapters 13-15 (Loops and Flow).
- Hunicke, R., LeBlanc, M., & Zubek, R. (2004). *MDA: A Formal Approach to Game Design*.
- Koster, R. (2004). *A Theory of Fun for Game Design*. Chapter 4 (Flow and Mastery).
- Contra: NES (1987) -- reference for side-scrolling shooter weapon pickups and auto-fire.
- Metal Slug: Neo Geo (1996) -- reference for destructible environments and weapon variety in run-and-gun.
- Enter the Gungeon: Dodge Roll (2016) -- reference for weapon attachment diversity and synergy systems.
- Broforce: Free Lives (2015) -- reference for destructible terrain and escalating destruction spectacle.

---

**Version**: 2.0
**Last Updated**: 2026-02-04
**Status**: Active
**Assessment**: 3.1 - Progression System Design
