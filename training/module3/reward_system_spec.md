# Assessment 3.4: Reward System Architecture

## Overview

Complete reward system specification for the retro side-scrolling shooter mobile game (iOS). This document defines three reward timescales (immediate, level, meta), specifies exact visual/audio/haptic feedback for every player action, details the score formula, defines the star/grade system, lists 20 achievements with tiers, and provides a comprehensive reward feedback matrix. All feedback is designed for the retro pixel art aesthetic running at 60fps on iPhone 11+.

---

## 1. Immediate Rewards (Per-Action Feedback)

### 1.1 Design Principle: "Juice"

Every player action must feel satisfying through the combination of visual, audio, and haptic feedback. The term "juice" refers to the cumulative effect of these micro-rewards. The player should never perform an action and feel nothing happen.

**Feedback latency budget**: All feedback must begin within 1 frame (16.67ms) of the triggering action. Audio may have up to 3 frames (50ms) of latency for the synthesis pipeline.

### 1.2 Movement Feedback

| Action | Visual Feedback | Audio Feedback | Haptic Feedback |
|--------|----------------|----------------|-----------------|
| Walk (start) | Foot dust particles (2-3 small white puffs) | Soft footstep loop (8-bit, 4 steps/s) | None |
| Walk (stop) | Sprite snaps to idle, small brake dust | Footstep loop ends with subtle slide SFX | None |
| Jump (takeoff) | Sprite squash (80% height, 120% width for 2 frames), dust burst at feet | "Boing" SFX (8-bit, pitch 440Hz, 80ms) | UIImpactFeedbackGenerator.light |
| Jump (apex) | Sprite stretch (120% height, 80% width for 2 frames) | None (silent apex builds tension) | None |
| Jump (land) | Sprite squash (85% height, 115% width for 3 frames), dust cloud (6 particles) | "Thud" SFX (low 8-bit thump, 120ms) | UIImpactFeedbackGenerator.medium |
| Dash (activate) | 3-frame afterimage trail (semi-transparent copies), speed lines | "Whoosh" SFX (rising pitch sweep, 200ms) | UIImpactFeedbackGenerator.rigid |
| Dash (end) | Afterimages fade over 5 frames, small air burst at destination | Subtle deceleration pop | None |
| Wall jump | Wall dust burst (4 particles from wall surface), sprite rotation 15 degrees briefly | "Kick" SFX (sharp 8-bit snap, 60ms) | UIImpactFeedbackGenerator.light |
| Wall slide | Friction particle trail down wall (continuous small sparks) | Scratchy slide loop (8-bit static, low volume) | None |
| Ground slam | Shockwave ring expanding from impact point (8 pixel radius, 12 frames) | Heavy impact boom (deep 8-bit, 200ms) + screen shake (4px, 150ms) | UIImpactFeedbackGenerator.heavy |

### 1.3 Weapon Auto-Fire Feedback

| Action | Visual Feedback | Audio Feedback | Haptic Feedback |
|--------|----------------|----------------|-----------------|
| Auto-fire (basic blaster) | Muzzle flash at weapon mount point (2 frames), projectile sprite travels toward target | "Pew" SFX (8-bit laser, pitch 880Hz, 60ms) per shot, 3 shots/s rhythm | UIImpactFeedbackGenerator.light (per shot) |
| Auto-fire (scatter shot) | Wide muzzle cone flash (3 frames), 3 projectile sprites fan outward | "Blam" SFX (8-bit shotgun burst, 100ms), 2 shots/s rhythm | UIImpactFeedbackGenerator.medium |
| Auto-fire (piercing beam) | Continuous beam sprite from mount to target, pulse glow at hit point | "Zzzap" SFX (8-bit sustained beam, continuous loop while firing) | Continuous light haptic pulse (6Hz) |
| Auto-fire (homing missile) | Missile sprite with smoke trail, curves toward target | "Fwoosh" SFX (ascending 8-bit whoosh, 200ms) per missile, 1.5/s rhythm | UIImpactFeedbackGenerator.rigid |
| Weapon hit (enemy) | 2-frame hit-stop (game freezes 33ms), enemy flashes white for 3 frames, 4 hit-spark particles, damage number popup (+50) | "Clang" SFX (metallic 8-bit hit, 150ms) + enemy grunt | UIImpactFeedbackGenerator.medium |
| Enemy defeat | Enemy sprite flashes 5 times rapidly (100ms total), then explodes into 8 pixel fragments that fall with gravity, coin burst (3-5 coins scatter upward) | "Poof" SFX (8-bit explosion, 200ms) + coin scatter jingle | UINotificationFeedbackGenerator.success |
| Target cycle | Brief highlight ring flashes around new target (4 frames), current target gets crosshair overlay | "Click" SFX (sharp 8-bit switch, 40ms) | UIImpactFeedbackGenerator.light |
| Special attack (hold jump when powered up) | Screen flash white (2 frames), large radial blast wave from player (16px radius), all targets in range take damage, dramatic weapon-specific effect | "BOOM" SFX (deep 8-bit explosion + rising chime, 400ms) + screen shake (6px, 200ms) | UIImpactFeedbackGenerator.heavy + UINotificationFeedbackGenerator.success |

### 1.4 Destruction Feedback

| Action | Visual Feedback | Audio Feedback | Haptic Feedback |
|--------|----------------|----------------|-----------------|
| Wall hit (not destroyed) | Crack sprites appear on wall tile (progressive: 1 crack -> 2 cracks -> spiderweb), dust puff at impact point | "Thunk" SFX (dull 8-bit impact, pitch lowers with each hit, 80ms) | UIImpactFeedbackGenerator.light |
| Wall destroyed (single) | Wall tile shatters into 6-8 pixel fragments, dust cloud (8 particles), fragments fall with gravity, hidden path/coins revealed behind | "Crumble" SFX (8-bit rocks falling, 300ms) | UIImpactFeedbackGenerator.heavy |
| Chain reaction (multi-wall collapse) | Sequential destruction cascade: each connected wall shatters 0.1s after previous, screen shake builds (2px -> 4px -> 6px), dust cloud intensifies, camera pulls back slightly to show full destruction | "Rumble" SFX (escalating 8-bit cascade, 500-1000ms based on chain length) + individual crumble per wall | UIImpactFeedbackGenerator.heavy + continuous medium pulse during cascade |
| Debris falling (hazard) | Dark shadow on ground grows for 0.5s, debris sprite falls with acceleration, impact dust cloud on landing | "Whistle" SFX (descending pitch, 300ms) + "Crash" on impact (200ms) | UIImpactFeedbackGenerator.medium on nearby impact |
| Path revealed | Hidden area brightens with light sweep (left to right, 12 frames), "SECRET!" text with star burst if secret room, coin sparkle particles in revealed area | "Discovery" fanfare (8-bit mystery resolved chord, 500ms) | UINotificationFeedbackGenerator.success |
| 100% destruction achieved | Screen flash gold (3 frames), "TOTAL DESTRUCTION!" text with explosion border, all remaining debris clears with particle burst | "Achievement" fanfare (triumphant 8-bit brass, 800ms) + crowd roar | UINotificationFeedbackGenerator.success + triple heavy impact |

### 1.5 Weapon Pickup Feedback

| Action | Visual Feedback | Audio Feedback | Haptic Feedback |
|--------|----------------|----------------|-----------------|
| Weapon drop appears | Weapon crate drops from above with parachute sprite (4 frames), lands with bounce, glowing pulse aura (2Hz), beacon light beam upward | "Incoming" SFX (8-bit descending whistle, 200ms) + "Thud" on landing | UIImpactFeedbackGenerator.medium (on landing) |
| Weapon pickup (mounting) | 0.3s dramatic pause: player sprite stops, weapon flies from ground to mount point on character body with arc tween, mounting flash (white burst at mount point, 3 frames), weapon-specific color glow on character for 1s | "Power up" SFX (ascending 8-bit fanfare, 400ms) + weapon-specific activation sound | UIImpactFeedbackGenerator.rigid + UINotificationFeedbackGenerator.success |
| Weapon swap (replacing current) | Old weapon flies off player (opposite direction) and fades, new weapon mounts with same animation as pickup | "Swap" SFX (quick descending then ascending tone, 200ms) | UIImpactFeedbackGenerator.medium |

### 1.6 Damage Feedback (Player Takes Hit)

| Action | Visual Feedback | Audio Feedback | Haptic Feedback |
|--------|----------------|----------------|-----------------|
| Take damage (enemy) | Screen flash red (50ms, 30% opacity), player sprite blinks (invincibility frames, 1.5s), HP bar segment pulses red then drains | "Hurt" SFX (low buzzy 8-bit, 150ms) + HP drain sound | UINotificationFeedbackGenerator.error |
| Take damage (hazard) | Same as enemy + hazard-specific particle (fire sparks for lava, spike particles for spikes, debris dust for falling debris) | Hazard-specific SFX (sizzle for lava, crunch for spikes, crash for debris) | UINotificationFeedbackGenerator.error |
| Near-death (1 HP) | HP bar flashes urgently (1Hz pulse, red), heartbeat sound loop begins, subtle vignette on screen edges | Heartbeat SFX loop (8-bit, 1 beat/s, low volume) | Rhythmic light haptic (1Hz, matching heartbeat) |
| Death | 4-frame death animation (sprite shatters into pixels), screen fades to 60% black, "RETRY?" prompt | Death jingle (8-bit descending notes, 500ms) | UINotificationFeedbackGenerator.error (strong) |
| Respawn | Sprite materializes at checkpoint with sparkle effect (8 frames), brief invincibility flash | Respawn chime (ascending 8-bit arpeggio, 300ms) | UIImpactFeedbackGenerator.light |

### 1.7 Environmental Feedback

| Interaction | Visual Feedback | Audio Feedback | Haptic Feedback |
|------------|----------------|----------------|-----------------|
| Checkpoint activated | Crystal on pedestal lights up (color change: grey -> era color), light beam shoots upward (10 frames), "CHECKPOINT" text | Activation chime (warm 8-bit tone, 300ms) | UINotificationFeedbackGenerator.success |
| Secret room found | Hidden wall shimmers then dissolves (pixel-by-pixel, 20 frames), "SECRET!" text with star burst | Discovery fanfare (8-bit mystery resolved chord, 500ms) | UINotificationFeedbackGenerator.success |
| Door/portal entry | Sprite walks into portal, swirl animation, screen wipe transition | Portal hum SFX (ethereal 8-bit, 400ms) | UIImpactFeedbackGenerator.medium |

---

## 2. Level Rewards (Per-Level Scoring)

### 2.1 Score Formula

The score formula rewards fast completion, thorough destruction, weapon mastery, and survivability.

**Base Score Calculation**:
```
BASE_SCORE = TIME_SCORE + DESTRUCTION_SCORE + WEAPON_SCORE + EXPLORATION_SCORE + SURVIVAL_SCORE

TIME_SCORE:
  par_time = level-specific par time (seconds)
  completion_time = player's actual time (seconds)
  if completion_time <= par_time:
    TIME_SCORE = 4000 * (par_time / completion_time)
    // Faster = higher score. At par time: 4000. At half par: 8000.
  else:
    TIME_SCORE = 4000 * max(0.5, 1.0 - (completion_time - par_time) / par_time)
    // Slower than par: score decreases but floors at 2000

DESTRUCTION_SCORE:
  walls_destroyed = count of walls destroyed
  total_walls = total destructible walls in level
  DESTRUCTION_SCORE = (walls_destroyed / total_walls) * 3000
  // All walls destroyed: 3000. Half: 1500.
  // Bonus: +100 per chain reaction triggered (multi-wall collapse)

WEAPON_SCORE:
  weapons_collected = count of unique weapon types collected in level
  total_weapon_drops = total weapon drops in level
  WEAPON_SCORE = (weapons_collected / total_weapon_drops) * 1500
  // All weapons collected: 1500. Bonus: +200 per weapon type used to destroy walls

EXPLORATION_SCORE:
  coins_collected = count of coins collected
  total_coins = total coins in level
  secrets_found = count of secret rooms found
  total_secrets = total secret rooms in level
  EXPLORATION_SCORE = (coins_collected / total_coins) * 1500
                    + (secrets_found / total_secrets) * 1000
  // All coins: 1500. All secrets: 1000.

SURVIVAL_SCORE:
  deaths = number of deaths during level
  damage_taken = total HP lost
  max_hp = player's max HP (typically 5)
  SURVIVAL_SCORE = max(0, 2000 - (deaths * 400) - (damage_taken * 100))
  // No deaths, no damage: 2000. 5 deaths: 0.
```

**Multipliers**:
```
FINAL_SCORE = BASE_SCORE * DESTRUCTION_MULTIPLIER * DIFFICULTY_MULTIPLIER

DESTRUCTION_MULTIPLIER:
  destruction_percentage = walls_destroyed / total_walls * 100
  if destruction_percentage >= 100: DESTRUCTION_MULTIPLIER = 1.5
  else if destruction_percentage >= 80: DESTRUCTION_MULTIPLIER = 1.3
  else if destruction_percentage >= 50: DESTRUCTION_MULTIPLIER = 1.1
  else: DESTRUCTION_MULTIPLIER = 1.0

DIFFICULTY_MULTIPLIER:
  difficulty_tier = 1 to 10 (based on era)
  DIFFICULTY_MULTIPLIER = 0.8 + (difficulty_tier * 0.1)
  // Era 1: 0.9x, Era 2: 1.0x, Era 3: 1.1x, ... Era 10: 1.8x
```

**Score example (Level 10, Era 1 Boss)**:
```
par_time = 180s, completion_time = 160s
TIME_SCORE = 4000 * (180/160) = 4,500

walls_destroyed = 12/15, 2 chain reactions
DESTRUCTION_SCORE = (12/15) * 3000 + (2 * 100) = 2,600

weapons = 2/2 types, both used on walls
WEAPON_SCORE = (2/2) * 1500 + (2 * 200) = 1,900

coins = 25/30, secrets = 1/1
EXPLORATION_SCORE = (25/30) * 1500 + (1/1) * 1000 = 2,250

deaths = 1, damage_taken = 3
SURVIVAL_SCORE = max(0, 2000 - 400 - 300) = 1,300

BASE_SCORE = 4,500 + 2,600 + 1,900 + 2,250 + 1,300 = 12,550

destruction_percentage = 80% -> DESTRUCTION_MULTIPLIER = 1.3
difficulty_tier = 1 -> DIFFICULTY_MULTIPLIER = 0.9

FINAL_SCORE = 12,550 * 1.3 * 0.9 = 14,684
```

### 2.2 Star Rating System (1-3 Stars)

Stars are awarded based on the percentage of maximum theoretical score achieved:

```
max_theoretical_score = TIME_SCORE(at blitz time) + DESTRUCTION_SCORE(all walls, max chains)
                      + WEAPON_SCORE(all weapons, all used on walls)
                      + EXPLORATION_SCORE(all coins, all secrets)
                      + SURVIVAL_SCORE(no deaths, no damage)
                      * max DESTRUCTION_MULTIPLIER * DIFFICULTY_MULTIPLIER

percentage = FINAL_SCORE / max_theoretical_score

Star thresholds:
  1 Star: percentage >= 30% (level completed, basic performance)
  2 Stars: percentage >= 55% (good performance, some optimization)
  3 Stars: percentage >= 80% (excellent performance, near-optimal play)
```

**Star display animation**:
- 1 Star: Single star fills in with yellow glow (0.3s), small chime
- 2 Stars: Two stars fill sequentially (0.3s each, 0.1s gap), warmer chime
- 3 Stars: Three stars fill with escalating celebration, final star triggers burst of particles, triumphant fanfare, screen glows briefly

**Bonus indicators on score screen**:
| Bonus | Condition | Display |
|-------|-----------|---------|
| FAST! | completion_time <= par_time | Blue clock icon + "FAST!" text |
| BLITZ! | completion_time <= par_time * 0.50 | Gold clock icon + "BLITZ!" text |
| TOTAL DESTRUCTION! | destruction_percentage == 100% | Red explosion icon + "TOTAL DESTRUCTION!" text |
| FLAWLESS! | damage_taken == 0 | Green shield icon + "FLAWLESS!" text |
| DEATHLESS! | deaths == 0 | Purple skull icon (crossed out) + "DEATHLESS!" text |
| SECRET HUNTER! | secrets_found == total_secrets | Star map icon + "SECRET HUNTER!" text |
| FULLY ARMED! | all weapon types collected | Gold gun icon + "FULLY ARMED!" text |
| CHAIN MASTER! | 3+ chain reactions triggered | Chain link icon + "CHAIN MASTER!" text |

### 2.3 Level Completion Summary Screen Layout

```
+--------------------------------------------------+
|              LEVEL COMPLETE!                      |
|                                                   |
|         [Star 1]  [Star 2]  [Star 3]             |
|                                                   |
|  TIME       1:42    par: 2:00       [FAST!]      |
|  DESTROYED  12/15   (80%)           [80%!]        |
|  WEAPONS    2/2 types               [ARMED!]      |
|  COINS      28/30                                 |
|  SECRETS    1/1                     [FOUND!]      |
|  DAMAGE     1 hit                                 |
|  DEATHS     0                       [DEATHLESS!]  |
|  CHAINS     2 reactions                           |
|                                                   |
|  ─────────────────────────────────                |
|  SCORE:              14,684                       |
|  BEST:               16,203                       |
|                                                   |
|  [NEXT LEVEL]    [RETRY]    [SHARE]              |
+--------------------------------------------------+
```

- Score number counts up from 0 to final value (1.5s animation, accelerating)
- If new personal best: "NEW BEST!" banner with confetti particles
- "NEXT LEVEL" button is largest and primary-colored (green)
- "SHARE" button generates Level ID text + destruction % + weapon count for iOS Share Sheet
- Level ID displayed at bottom in small monospace font for manual sharing

---

## 3. Meta Rewards (Cross-Session)

### 3.1 Achievement System (20 Achievements, 3 Tiers Each)

Full achievement list with unlock conditions, organized by category. Each achievement awards coins and, at Silver/Gold tiers, cosmetic unlocks.

#### Destruction Achievements

| # | Name | Bronze (50 coins) | Silver (150 coins) | Gold (500 coins) | Cosmetic Unlock |
|---|------|-------------------|-------------------|------------------|-----------------|
| 1 | Demolisher | Destroy 100 walls | Destroy 1,000 walls | Destroy 10,000 walls | Silver: "Breaker" sprite border; Gold: "Wrecking Ball" character skin |
| 2 | Untouchable | Complete 1 level no-damage | 5 levels no-damage | 20 levels no-damage | Silver: Dodge afterimage color (blue); Gold: "Ghost" character skin |
| 3 | Boss Slayer | Defeat 1 boss | Defeat 5 bosses | Defeat all 10 era bosses | Silver: Boss trophy collection display; Gold: "Apex Predator" title |
| 4 | Chain Reactor | Trigger 5 chain reactions | 25 chain reactions | 100 chain reactions | Silver: Chain explosion trail effect; Gold: "Demolition Expert" title |
| 5 | Total Annihilation | 100% destruction on 1 level | 100% on 5 levels | 100% on 20 levels | Silver: Destruction counter flames; Gold: "Annihilator" character aura |

#### Exploration Achievements

| # | Name | Bronze (50 coins) | Silver (150 coins) | Gold (500 coins) | Cosmetic Unlock |
|---|------|-------------------|-------------------|------------------|-----------------|
| 6 | Secret Keeper | Find 5 secret rooms | Find 15 secret rooms | Find all 6 variants | Silver: Secret room radar ping; Gold: "Seeker" character skin |
| 7 | Coin Collector | 1,000 coins lifetime | 10,000 coins | 50,000 coins | Silver: Golden coin trail; Gold: "Midas" character skin |
| 8 | Era Explorer | 1 level each era | 3 levels each era | 5 levels each era | Silver: Era badge set; Gold: "Time Traveler" title + temporal trail |
| 9 | Path Breaker | Find 3 hidden destruction paths | Find 10 paths | Find 25 paths | Silver: Footprint trail effect; Gold: "Pathfinder" title |
| 10 | Completionist | 100% one level | 100% ten levels | 100% twenty-five levels | Silver: Platinum star border; Gold: "Perfectionist" character skin |

#### Speedrunner Achievements

| # | Name | Bronze (50 coins) | Silver (150 coins) | Gold (500 coins) | Cosmetic Unlock |
|---|------|-------------------|-------------------|------------------|-----------------|
| 11 | Speed Demon | 1 level under par time | 10 levels under par | 25 levels under par | Silver: Speed lines always visible; Gold: "Flash" character skin |
| 12 | Sub-Minute | 1 level under 60 seconds | 5 levels under 60s | 10 levels under 60s | Silver: Stopwatch trail; Gold: "Chronos" title |
| 13 | Blitz | 1 level under 30 seconds | 3 levels under 30s | 5 levels under 30s | Silver: Lightning trail; Gold: "Lightning" character skin |

#### Social Achievements

| # | Name | Bronze (50 coins) | Silver (150 coins) | Gold (500 coins) | Cosmetic Unlock |
|---|------|-------------------|-------------------|------------------|-----------------|
| 14 | Challenger | Share 1 Level ID | Share 10 Level IDs | Share 25 Level IDs | Silver: Envelope trail effect; Gold: "Ambassador" title |
| 15 | Rival | Beat 1 friend's score | Beat 10 friend scores | Beat 50 friend scores | Silver: Crown icon on leaderboard; Gold: "Champion" character skin |
| 16 | Daily Devotee | 7 daily challenges | 30 daily challenges | 100 daily challenges | Silver: Calendar badge; Gold: "Eternal" title + cosmic trail |

#### Mastery Achievements

| # | Name | Bronze (50 coins) | Silver (150 coins) | Gold (500 coins) | Cosmetic Unlock |
|---|------|-------------------|-------------------|------------------|-----------------|
| 17 | Perfect Run | 1 perfect level (3 stars + no damage + 100% destruction + under par) | 5 perfect levels | 15 perfect levels | Silver: Diamond star border; Gold: "Flawless" character skin (crystalline) |
| 18 | Era Climber | Complete Era 1 | Complete Era 5 | Complete all 10 Eras | Silver: Era badge collection; Gold: "Ascendant" title + prismatic aura |
| 19 | Arsenal Master | Collect every weapon type at least once | Complete a level using all weapon types | All weapons mastered on Extreme | Silver: Weapon icon border set; Gold: "Armorer" character skin |
| 20 | Endurance | 25 levels played | 100 levels played | 250 levels played | Silver: Marathon badge; Gold: "Legend" title + legacy trail |

### 3.2 Leaderboard System

| Leaderboard | Scope | Sort By | Reset Cycle |
|-------------|-------|---------|-------------|
| Global All-Time | All players | Cumulative score | Never |
| Global Weekly | All players active this week | Weekly cumulative score | Monday 00:00 UTC |
| Friends | Player's Game Center friends | Cumulative score | Never |
| Per-Level | All players who completed that level | Level score | Never |
| Daily Challenge | All players (today's challenge) | Challenge score | Daily 00:00 UTC |
| Speedrun (per level) | All players | Completion time (ascending) | Never |
| Destruction (per level) | All players | Destruction percentage (descending) | Never |

**Leaderboard display**:
- Top 10 shown with rank, name, score, and profile cosmetics
- Player's own rank always shown even if outside top 10 ("You: #4,521")
- Friends highlighted with different color row
- Tap any entry to view their profile card (cosmetics, achievements, stats)

**Leaderboard anti-cheat**: Scores submitted with input replay hash. Top 100 scores are automatically verified by server-side replay simulation. Flagged scores are quarantined pending manual review.

### 3.3 Unlockable Cosmetics

All cosmetics are visual only -- no gameplay advantage.

#### Character Skins (15 total)

| # | Skin Name | Unlock Method | Visual Description |
|---|-----------|--------------|-------------------|
| 1 | Default | Start | Armored adventurer with weapon mount on shoulder |
| 2 | Stone Hunter | Era 1 3-star mastery | Bone armor with prehistoric weapon mounts |
| 3 | Bronze Commander | Era 2 3-star mastery | Bronze plate armor with ornate weapon cradle |
| 4 | Iron Warden | Era 3 3-star mastery | Dark iron armor with mechanical weapon rail |
| 5 | Medieval Knight | Era 4 3-star mastery | Full plate armor with heraldic weapon brace |
| 6 | The Transcendent | Complete all levels across all eras | Prismatic shifting colors with holographic weapon mount |
| 7 | Ghost | "Untouchable" Gold achievement | Semi-transparent sprite with blue glow |
| 8 | Wrecking Ball | "Demolisher" Gold achievement | Heavy armor with oversized weapon mount, rubble trail |
| 9 | Midas | "Coin Collector" Gold achievement | Golden sprite, coins fly toward player |
| 10 | Flash | "Speed Demon" Gold achievement | Streamlined sprite with speed-line cape |
| 11 | Seeker | "Secret Keeper" Gold achievement | Detective hat and magnifying glass |
| 12 | Lightning | "Blitz" Gold achievement | Electric blue with static particles |
| 13 | Flawless | "Perfect Run" Gold achievement | Crystal/diamond textured sprite |
| 14 | Champion | "Rival" Gold achievement | Crown and champion belt |
| 15 | Armorer | "Arsenal Master" Gold achievement | Multi-weapon holstered sprite, floating weapon ring |

#### Trail Effects (8 total)

| # | Trail | Unlock Method | Visual Description |
|---|-------|--------------|-------------------|
| 1 | None | Default | No trail |
| 2 | Dust | Start | Small dust particles (default) |
| 3 | Sparkle | 500 total coins collected | White sparkle particles |
| 4 | Fire | Defeat Era 8 boss (Digital Age) | Orange flame trail |
| 5 | Lightning | "Blitz" Silver achievement | Blue electric arcs |
| 6 | Rainbow | 30-day daily challenge streak | Full rainbow particle spectrum |
| 7 | Cosmic | "Daily Devotee" Gold achievement | Star and nebula particles |
| 8 | Golden | All 50 perfect runs complete | Golden pixel dust |

#### Profile Frames (5 total)

| # | Frame | Unlock Method | Visual Description |
|---|-------|--------------|-------------------|
| 1 | Basic | Start | Simple gray border |
| 2 | Bronze | 10 Bronze achievements | Bronze metallic border |
| 3 | Silver | 10 Silver achievements | Silver metallic border with shine |
| 4 | Gold | 10 Gold achievements | Gold border with animated shimmer |
| 5 | Prismatic | All 60 achievement milestones | Color-shifting animated border |

---

## 4. Reward Feedback Matrix

Complete mapping of every player action to its multi-sensory feedback response.

### 4.1 Core Actions

| Action | Visual | Audio | Haptic | Score Impact |
|--------|--------|-------|--------|-------------|
| Walk | Foot dust (2-3 particles) | Footstep loop (4/s) | None | None |
| Jump | Squash/stretch + dust burst | "Boing" (440Hz, 80ms) | Light impact | None |
| Land | Squash + dust cloud (6 particles) | "Thud" (low, 120ms) | Medium impact | None |
| Dash | 3-frame afterimage + speed lines | "Whoosh" (rising sweep, 200ms) | Rigid impact | None |
| Wall Jump | Wall dust + rotation | "Kick" (sharp snap, 60ms) | Light impact | None |
| Ground Slam | Shockwave ring + screen shake (4px) | Heavy boom (200ms) + shake | Heavy impact | None |
| Target Cycle | Highlight ring on new target | "Click" (sharp, 40ms) | Light impact | None |
| Auto-fire (hit wall) | Crack sprites + dust puff | "Thunk" (dull impact, 80ms) | Light impact | None (until destroyed) |
| Auto-fire (hit enemy) | Hit-stop (33ms) + white flash + sparks | "Clang" (metallic, 150ms) | Medium impact | +50 per enemy HP |

### 4.2 Reward Pickups

| Pickup | Visual | Audio | Haptic | Score Impact |
|--------|--------|-------|--------|-------------|
| Coin | Fly to counter + "+10" popup | "Ding" (C5, 80ms) | None | +10 per coin |
| Health Potion | Green spiral particles + HP fill | "Heal" (arpeggio, 200ms) | Success notification | Indirect (survival) |
| Weapon Attachment | Dramatic mount animation (0.3s) + weapon glow | "Power up" (fanfare, 400ms) | Rigid + success | Indirect (destruction capability) |
| Speed Boost | Speed lines (8s) + icon | "Speed up" (scale, 200ms) | Light impact | Indirect (time score) |
| Invincibility Star | Rainbow palette cycle (5s) | Invincibility jingle (5s loop) | Rhythmic pulse (4Hz) | Indirect (survival) |
| Magnet | Attraction lines + items fly to player | "Magnetize" (warble, 200ms) | Light per collection | Indirect (coin collection) |

### 4.3 Destruction Events

| Event | Visual | Audio | Haptic | Score Impact |
|-------|--------|-------|--------|-------------|
| Wall destroyed | 6-8 fragments + dust cloud + path revealed | "Crumble" (rocks falling, 300ms) | Heavy impact | +200 per wall (destruction score) |
| Chain reaction | Sequential cascade + escalating screen shake | "Rumble" (escalating cascade, 500-1000ms) | Building heavy pulses | +100 per chain link |
| Secret path revealed | Light sweep + "SECRET!" text + sparkles | "Discovery" (resolved chord, 500ms) | Success notification | +1000 (secret bonus) |
| 100% destruction | Gold flash + "TOTAL DESTRUCTION!" | "Achievement" (triumphant brass, 800ms) | Triple heavy impact | x1.5 destruction multiplier |
| Debris hazard (near miss) | Debris crashes near player, screen shake (2px) | "Crash" (impact, 200ms) | Medium impact | None |
| Enemy defeat | Flash 5x + pixel explosion + coin burst | "Poof" (explosion, 200ms) + coin jingle | Success notification | +50 base + drop value |
| Boss phase clear | Boss staggers + arena flash | Phase clear fanfare (500ms) | Success notification | +500 per phase |
| Boss defeat | Boss explodes into 20+ fragments, victory light pillar, cosmetic drop appears | Boss death theme (2s fanfare) | Triple heavy impact | +2000 |

### 4.4 Negative Events

| Event | Visual | Audio | Haptic | Score Impact |
|-------|--------|-------|--------|-------------|
| Take damage | Red flash (50ms) + invincibility blink (1.5s) | "Hurt" (buzzy, 150ms) | Error notification | -100 survival score |
| Fall into pit | Sprite falls off screen, camera holds | Falling whistle (descending, 500ms) | Error notification | -400 survival score (death) |
| Death (any cause) | Death animation + fade to 60% black | Death jingle (descending, 500ms) | Strong error notification | -400 survival score |
| Hit by debris | Debris-specific particles + red flash | "Crash" (impact) + "Hurt" | Error notification | -100 survival score |
| Touch lava/spikes | Hazard-specific particles + red flash | Hazard SFX (sizzle/crunch) | Error notification | -100 survival score |

### 4.5 Progress Events

| Event | Visual | Audio | Haptic | Score Impact |
|-------|--------|-------|--------|-------------|
| Checkpoint reached | Crystal lights up + beam + "CHECKPOINT" text | Warm chime (300ms) | Success notification | None (saves progress) |
| Level complete | Flag animation + "LEVEL COMPLETE!" + star fill | Completion fanfare (1s) | Success notification | Score calculated |
| New personal best | "NEW BEST!" banner + confetti | Best score jingle (celebratory, 800ms) | Triple success notification | None |
| Achievement unlocked | Full-screen toast (1.5s) + achievement icon | Achievement unlock SFX (triumphant, 1s) | Success notification | None (coins awarded) |
| Weapon unlocked | "NEW WEAPON" screen + demonstration | Weapon unlock fanfare (1.5s) | Heavy impact | None |
| Era unlocked | Era panorama reveal + title text | Era theme intro (2s) | Heavy impact | None |
| Era complete | Era completion ceremony + boss trophy | Era fanfare (3s, orchestral 8-bit) | Triple heavy impact | None |

---

## 5. Reward Pacing Verification

### 5.1 Reward Density per Minute

| Reward Type | Target Frequency | Minimum | Notes |
|-------------|-----------------|---------|-------|
| Coin pickup | 6-10 per minute | 4 per minute | Most common reward, maintains rhythm |
| Wall destruction | 4-8 per minute (Normal difficulty) | 2 per minute | Core destruction reward |
| Weapon attachment | 0.5-1 per minute | 0.25 per minute | Excitement spike, dramatic pickup |
| Enemy defeat | 3-5 per minute | 2 per minute | Core combat reward |
| Health pickup | 1-2 per minute | 0.5 per minute | Strategic placement |
| Score popup | 12-20 per minute | 8 per minute | Aggregate of all above |
| Checkpoint | 1 per 2-3 minutes | 1 per 3 minutes | Progress safety |

**Maximum time between any reward**: 8 seconds. If procedural generation places the player in a section with no rewards for > 8 seconds, the generator must inject at minimum a 3-coin cluster or a destructible wall.

### 5.2 Session Reward Arc

Each level follows a predictable reward arc:

```
Reward Density
    High |        _____              ___________
         |       /     \            /           \
    Med  |  ____/       \          /             \___
         | /             \        /                  \
    Low  |/               \______/                    \
         |________________________________________________
         Intro   Destroy    Rest    Climax+Boss    Score
```

- **Intro zone**: Moderate rewards (coins, easy walls to destroy)
- **Destroy zone**: High reward density (many walls = many destruction rewards, weapon pickups)
- **Rest zone**: Low rewards (exploration-based, secrets behind walls)
- **Climax + Boss**: Highest reward density (constant auto-fire, destruction chains, boss phase bonuses)
- **Score screen**: Meta-reward cascade (stars, bonuses, achievements)

---

## 6. Implementation Notes

### 6.1 Unity Implementation Priorities

| System | Priority | Unity Component |
|--------|----------|-----------------|
| Hit-stop | P0 | `Time.timeScale = 0` for 2 frames, use `WaitForSecondsRealtime` |
| Screen shake | P0 | Camera offset via `Cinemachine` impulse source |
| Particle effects | P0 | `ParticleSystem` with retro sprite sheet |
| Score popups | P0 | `TextMeshPro` world-space text with DOTween animation |
| Haptic feedback | P0 | `CoreHaptics` via native iOS plugin (`CHHapticEngine`) |
| Destruction debris | P0 | Physics-enabled 2D sprite fragments with gravity |
| Weapon mount animation | P0 | Custom tween from ground to mount point on character |
| Sprite squash/stretch | P1 | `Transform.localScale` animation via `AnimationCurve` |
| Audio stings | P0 | `AudioSource.PlayOneShot()` with pooled sources (8 pool) |
| Destruction counter | P1 | HUD element with `DOTween` scale/color punch |
| Achievement toast | P1 | Overlay Canvas with slide-in animation |

### 6.2 Performance Budget

| Feedback Type | Max CPU per Frame | Max Particles Active | Max Audio Sources |
|---------------|------------------|---------------------|------------------|
| Hit effects | 0.5ms | 20 particles | 2 concurrent |
| Wall destruction | 1.0ms | 30 particles (per wall) | 2 concurrent |
| Chain reaction | 2.0ms | 80 particles (cascade) | 3 concurrent |
| Coin collection | 0.3ms | 8 particles (per coin) | 1 |
| Death/respawn | 1.0ms | 40 particles | 2 concurrent |
| Boss defeat | 2.0ms | 100 particles (1s burst) | 3 concurrent |
| Total budget | 4.0ms per frame | 150 active | 8 pooled sources |

### 6.3 Haptic Engine Specification (iOS)

Using `CoreHaptics` framework for rich haptic patterns:

```swift
// Example: Wall destruction haptic pattern
let sharpness = CHHapticEventParameter(parameterID: .hapticSharpness, value: 0.9)
let intensity = CHHapticEventParameter(parameterID: .hapticIntensity, value: 0.8)
let event = CHHapticEvent(
    eventType: .hapticTransient,
    parameters: [sharpness, intensity],
    relativeTime: 0
)
```

| Haptic Pattern | Type | Intensity | Sharpness | Duration |
|---------------|------|-----------|-----------|----------|
| Light impact | Transient | 0.3 | 0.5 | Instant |
| Medium impact | Transient | 0.6 | 0.7 | Instant |
| Heavy impact | Transient | 0.9 | 0.9 | Instant |
| Rigid impact | Transient | 0.7 | 1.0 | Instant |
| Success notification | Continuous + Transient | 0.4 -> 0.6 | 0.3 -> 0.7 | 200ms |
| Error notification | Transient x2 | 0.8, 0.5 | 0.9, 0.6 | 100ms gap |
| Heartbeat loop | Transient x2 (repeating) | 0.5, 0.3 | 0.4, 0.2 | 1Hz cycle |
| Chain cascade | Transient (escalating) | 0.4 -> 0.9 | 0.5 -> 0.9 | 100ms per link |
| Weapon mount | Continuous + Transient | 0.3 -> 0.8 | 0.5 -> 1.0 | 300ms |

---

## References

- Swink, S. (2009). *Game Feel: A Game Designer's Guide to Virtual Sensation*. Chapters 3-5 (Feedback and Juiciness).
- Jonasson, M. & Purho, P. (2012). *Juice It or Lose It* (GDC Talk). Core principles of game feel.
- Apple Developer Documentation (2024). *Core Haptics Framework*. Haptic pattern design.
- Vlambeer (2013). *The Art of Screenshake* (GDC Talk). Screen shake, hit-stop, and particle feedback.
- Schell, J. (2008). *The Art of Game Design*. Lens #46: Reward.

---

**Version**: 1.1
**Last Updated**: 2026-02-04
**Status**: Active
**Assessment**: 3.4 - Reward System Architecture
