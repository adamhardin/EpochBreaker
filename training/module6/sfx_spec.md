# Assessment 6.4: Sound Effects Specification

## Overview

Complete sound effects library specification for a 16-bit side-scrolling mobile shooter (iOS). The game features weapon attachments that auto-fire continuously at enemies, destructible environments (walls, rocks, terrain), and weapons that evolve across 10 eras of human civilization. This spec covers 15 gameplay SFX and 5 UI SFX. Each sound is defined with waveform type, frequency range, duration, volume level, ADSR synthesis parameters, and descriptive character. The specification also covers the priority/layering system, haptic feedback pairing, volume mixing, cooldown timers, and Unity integration details.

**Key Design Principle:** Because weapons auto-fire continuously, the weapon_fire SFX must function as a pleasant rhythmic texture rather than a discrete event. Multiple simultaneous weapon attachments should layer into a pleasing polyrhythmic backdrop, not a wall of noise. Weapon-related sounds are designed to be quieter, shorter, and more tonal than typical shooter SFX.

---

## Technical Constraints

| Parameter | Value |
|-----------|-------|
| Waveform Types Allowed | Square (12.5%, 25%, 50% duty), Sawtooth, Triangle, Noise (white, pink) |
| Effects Allowed | Pitch bend (sweep up/down), volume envelope (ADSR), basic ring modulation |
| Effects Prohibited | Reverb, delay, chorus, flanger, compression, EQ, modern FX processing |
| Max Simultaneous SFX | 4 channels |
| Sample Rate | 44.1 kHz |
| Bit Depth | 16-bit |
| Working Format | .wav (uncompressed) |
| Game Format | .ogg (Vorbis, quality 6) or .wav (for very short clips <50ms) |
| Loudness Reference | All volumes specified relative to music level (music = 0 dB reference) |
| Master SFX Volume | -3 dB below music by default (user adjustable) |

---

## Gameplay Sound Effects (15)

### 1. SFX_JUMP

| Parameter | Value |
|-----------|-------|
| ID | sfx_jump |
| Duration | 120ms |
| Waveform | Square 50% duty |
| Frequency | 280 Hz -> 560 Hz (rising pitch sweep over duration) |
| Volume | -6 dB (relative to music) |
| ADSR | A: 0ms, D: 0ms, S: 80%, R: 40ms |
| Description | Short, snappy upward pitch sweep. Bright and springy. Starts at a mid-low pitch and quickly rises one octave. Communicates upward motion. A clean "bwip" sound that reads well over continuous weapon fire. Since the player jumps frequently to dodge enemies and navigate terrain, this must cut through the weapon texture without being fatiguing. |
| Synthesis Notes | Linear pitch sweep from 280 Hz to 560 Hz over 80ms, then 40ms release/decay to silence. Square wave gives it a clear, cutting quality that reads well through music and the constant weapon fire layer beneath it. |

---

### 2. SFX_LAND

| Parameter | Value |
|-----------|-------|
| ID | sfx_land |
| Duration | 80ms |
| Waveform | Noise (white) + Triangle |
| Frequency | Noise: broadband, filtered low-pass ~400 Hz; Triangle: 80 Hz |
| Volume | -8 dB |
| ADSR | A: 0ms, D: 80ms, S: 0%, R: 0ms |
| Description | Soft thud with a tiny bit of grit. The triangle provides the low "thump" body, the filtered noise adds texture like dust/debris being disturbed on landing. Very brief -- almost subliminal. Should feel grounding without being intrusive, since the player lands constantly during side-scrolling combat. Must blend naturally under the weapon fire texture. |
| Synthesis Notes | Layer: Triangle at 80 Hz (instant attack, 80ms decay to 0). Noise channel low-pass filtered to 400 Hz, 50% volume of triangle, same envelope. Combined creates a dull impact that sits beneath the weapon fire layer. |

---

### 3. SFX_WEAPON_FIRE

| Parameter | Value |
|-----------|-------|
| ID | sfx_weapon_fire |
| Duration | 60ms |
| Waveform | Triangle (primary) + Square 12.5% duty (accent, 30% mix) |
| Frequency | Triangle: 440 Hz (single pitch, no sweep); Square: 880 Hz (octave harmonic) |
| Volume | -12 dB (very quiet -- this is a constant rhythmic texture, not an event) |
| ADSR | A: 2ms, D: 15ms, S: 0%, R: 30ms |
| Description | Ultra-subtle rhythmic pulse. This is the most-heard SFX in the game -- it plays continuously as weapons auto-fire. Designed as a rhythmic texture, not an attention-grabbing event. The triangle wave provides a soft, warm "tick" while the quiet square harmonic adds just enough brightness to register. Must be pleasant on infinite repetition. When multiple weapon attachments fire simultaneously, their staggered timing should create a polyrhythmic pattern, not cacophony. Think of it as a gentle metronome underlying the action. Different eras can shift the base frequency slightly (see Era Variants below) to hint at the weapon's technological origin without requiring separate audio files. |
| Synthesis Notes | Triangle at 440 Hz provides the body -- chosen for its soft harmonic content (odd harmonics only, no harsh overtones). Square 12.5% at 880 Hz mixed at 30% adds a thin "click" transient for articulation. The extremely short duration (60ms) and quiet volume (-12 dB) ensure it functions as texture. The 2ms attack softens the onset to prevent clicks when rapidly repeating. No pitch sweep -- stability prevents ear fatigue on repetition. |

#### Era Variant Frequency Hints

These are subtle base frequency shifts applied to the triangle wave. The tonal character changes slightly per era without requiring separate audio assets. Implemented via a pitch multiplier parameter on the AudioSource.

| Era | Base Frequency | Pitch Multiplier | Tonal Character |
|-----|---------------|-------------------|-----------------|
| 1 - Stone Age | 330 Hz | 0.75x | Lower, duller -- rocks and slings |
| 2 - Bronze Age | 370 Hz | 0.84x | Slightly metallic resonance |
| 3 - Iron Age | 400 Hz | 0.91x | Harder, sharper tone |
| 4 - Classical | 415 Hz | 0.94x | Balanced, precise |
| 5 - Medieval | 440 Hz | 1.00x | Reference pitch -- crossbows and trebuchets |
| 6 - Renaissance | 466 Hz | 1.06x | Brighter -- early firearms |
| 7 - Industrial | 494 Hz | 1.12x | Mechanical, rhythmic |
| 8 - Modern | 523 Hz | 1.19x | Clean, efficient |
| 9 - Atomic | 554 Hz | 1.26x | Higher energy, slightly buzzy |
| 10 - Future | 587 Hz | 1.33x | Highest, ethereal shimmer |

---

### 4. SFX_ENEMY_HIT

| Parameter | Value |
|-----------|-------|
| ID | sfx_enemy_hit |
| Duration | 100ms |
| Waveform | Square 25% duty + Noise (pink) |
| Frequency | Square: 500 Hz -> 300 Hz (downward sweep); Noise: broadband, filtered bandpass 1-3 kHz |
| Volume | -4 dB |
| ADSR | A: 0ms, D: 40ms, S: 20%, R: 50ms |
| Description | Satisfying impact confirmation when a projectile hits an enemy. This is the primary reward sound in the gameplay loop -- the player should feel each hit connect. A tonal "crack" from the square wave pitch drop layered with a noise burst for physical impact texture. Punchy and bright. Designed to be clearly distinct from wall_hit (which is lower and crumblier). Since weapons auto-fire, this sound plays frequently but not constantly -- only when shots actually connect. Must feel good on the 100th hit as much as the 1st. |
| Synthesis Notes | Square 25% sweeps from 500 Hz to 300 Hz over 40ms (the tonal "crack" -- the narrow duty cycle gives it a nasal, cutting quality that reads through the mix). Pink noise burst at 60% volume for 15ms, then decays over 50ms. Pink noise chosen over white to avoid harshness on repetition. The downward pitch sweep communicates impact force. |

---

### 5. SFX_WALL_HIT

| Parameter | Value |
|-----------|-------|
| ID | sfx_wall_hit |
| Duration | 120ms |
| Waveform | Noise (white, low-pass filtered) + Triangle |
| Frequency | Noise: low-pass cutoff 800 Hz; Triangle: 120 Hz -> 60 Hz (downward sweep) |
| Volume | -6 dB |
| ADSR | A: 0ms, D: 30ms, S: 30%, R: 70ms |
| Description | Projectile impacts destructible wall/rock/terrain but does not destroy it. Distinctly different from enemy_hit -- this is lower, duller, and "crumbly" rather than punchy. The noise creates a gritty, granular texture like rock dust. The triangle adds a low thud of structural impact. Should communicate "you're damaging it, keep shooting" without the same reward as hitting an enemy. A dull "tch-thunk" sound. |
| Synthesis Notes | Low-pass filtered white noise at 800 Hz cutoff provides the gritty, rocky texture. Triangle sweeps from 120 Hz to 60 Hz over 80ms for a settling "thud." The noise is the primary element (70% of the mix), triangle is secondary (30%). The low frequency range keeps this sound distinct from the brighter, punchier enemy_hit. |

---

### 6. SFX_WALL_BREAK

| Parameter | Value |
|-----------|-------|
| ID | sfx_wall_break |
| Duration | 300ms |
| Waveform | Noise (white) + Square 50% duty + Triangle |
| Frequency | Noise: broadband burst; Square: 250 Hz -> 100 Hz (slow descending sweep); Triangle: 80 Hz -> 30 Hz |
| Volume | -2 dB |
| ADSR | A: 0ms, D: 50ms, S: 40%, R: 200ms |
| Description | A destructible tile is completely destroyed. This is a key reward sound -- satisfying crumble/shatter that makes destroying terrain feel powerful. Louder and more dramatic than wall_hit. Begins with a sharp noise burst (the initial fracture), followed by a descending tonal rumble (the crumbling), and a low triangle sub-bass (the weight of rubble falling). Should feel visceral and rewarding. Think of shattering blocks in Mega Man -- that satisfying destruction feedback. Encourages the player to keep blasting through the environment. |
| Synthesis Notes | Three layers: (1) White noise burst at full volume for 30ms, then rapid decay over 100ms -- this is the "crack/shatter" moment. (2) Square 50% sweeps from 250 Hz to 100 Hz over 250ms -- the tonal "crumbling" descent. (3) Triangle sweeps from 80 Hz to 30 Hz over 200ms at 60% volume -- sub-bass weight of falling debris. The combined effect is a sharp crack that dissolves into a satisfying low rumble. |

---

### 7. SFX_WEAPON_PICKUP

| Parameter | Value |
|-----------|-------|
| ID | sfx_weapon_pickup |
| Duration | 500ms |
| Waveform | Square 50% + Square 25% (two-voice harmony) |
| Frequency | Voice 1: C5->E5->G5->C6 (ascending major arpeggio); Voice 2: E5->G5->C6->E6 (parallel thirds above) |
| Volume | -2 dB |
| ADSR | A: 2ms, D: 20ms, S: 80%, R: 30ms per note |
| Description | Dramatic, empowering fanfare when the player picks up a new weapon attachment. This is a significant power moment -- the player's firepower just increased. Two square waves play in parallel thirds, creating a harmonized ascending fanfare. Brighter, bolder, and more elaborate than any other pickup sound. Should make the player feel powerful and excited. Four notes instead of three, and the two voices create harmonic richness. Think of the Mega Man weapon-get jingle compressed to 500ms. The ascending motion communicates upgrading and evolution -- fitting for a game about weapons evolving through civilization eras. |
| Synthesis Notes | Voice 1 (50% square): C5(100ms) E5(100ms) G5(100ms) C6(200ms, sustained with gentle vibrato 6 Hz +/- 10 cents). Voice 2 (25% square, 70% volume): E5(100ms) G5(100ms) C6(100ms) E6(200ms). Last note held longer for emphasis and triumph. The 25% duty on Voice 2 gives it a thinner, brighter quality that sits above Voice 1 without muddying. |

---

### 8. SFX_PLAYER_HURT

| Parameter | Value |
|-----------|-------|
| ID | sfx_player_hurt |
| Duration | 250ms |
| Waveform | Square 50% duty |
| Frequency | 350 Hz -> 150 Hz (downward sweep) |
| Volume | -1 dB |
| ADSR | A: 0ms, D: 30ms, S: 70%, R: 150ms |
| Description | Alarming damage feedback. A descending pitch that communicates "something bad happened." Lower and longer than enemy_hit. Should make the player wince slightly and instinctively want to dodge. Not painful to hear, but clearly negative. A descending "weh-weh" tone with a buzzy quality from the square wave that communicates distress. Must cut through the constant weapon fire texture to grab attention. |
| Synthesis Notes | Square 50% sweeps from 350 Hz to 150 Hz linearly over 250ms. Fast vibrato (12 Hz, +/- 20 cents) adds a "wobbling" distress quality. Volume peaks immediately then decays. The vibrato is crucial -- it distinguishes this from simple pitch sweeps used elsewhere and signals "danger" to the player. |

---

### 9. SFX_PLAYER_DEATH

| Parameter | Value |
|-----------|-------|
| ID | sfx_player_death |
| Duration | 800ms |
| Waveform | Square 50% duty + Triangle + Noise |
| Frequency | Square: 500 Hz -> 80 Hz (long descending sweep); Triangle: 200 Hz -> 40 Hz; Noise: broadband |
| Volume | 0 dB (loudest gameplay SFX -- full music-level volume) |
| ADSR | A: 0ms, D: 200ms, S: 50%, R: 500ms |
| Description | Dramatic, final, unmistakable death sound. A long descending sweep that drops into a low rumble before fading. The triangle adds weight, the noise adds a "crumbling/dissolving" quality. This sound must communicate finality. When this plays, the weapon fire texture stops (player is dead), so this sound fills the sudden silence with dramatic weight. Think classic Mega Man death: a descending cascade implying dissolution. Should not be annoying on repetition since players will hear it when they fail. |
| Synthesis Notes | Three layers: (1) Square sweeps 500->80 Hz over 600ms. (2) Triangle sweeps 200->40 Hz over 700ms, 50% volume. (3) Noise at 30% volume, 300ms burst at start then fades. All layers share a slow volume decay reaching silence at 800ms. Add slight ring modulation between square and triangle for a "breaking apart" quality. The sudden absence of weapon fire texture when this plays creates a powerful contrast. |

---

### 10. SFX_ENEMY_DEFEAT

| Parameter | Value |
|-----------|-------|
| ID | sfx_enemy_defeat |
| Duration | 200ms |
| Waveform | Square 25% duty + Noise |
| Frequency | Square: 600 Hz -> 1200 Hz (upward pop); Noise: broadband burst |
| Volume | -3 dB |
| ADSR | A: 0ms, D: 50ms, S: 0%, R: 50ms |
| Description | Satisfying pop/burst when an enemy is destroyed. An upward "pop" that feels rewarding. The square wave provides a bright tonal pop while the noise burst adds an explosive quality. Should feel celebratory and satisfying -- the player accomplished something. Brighter and more positive than enemy_hit. Quick and punchy. Distinctly different from wall_break (which is lower and crumblier). This is a "clean kill" sound -- sharp and precise. |
| Synthesis Notes | Square sweeps 600->1200 Hz over 50ms (instant upward pop). Noise burst at 60% volume, 30ms duration. Combined creates a "bwip-psh" sound that is clearly positive and rewarding. The upward pitch direction contrasts with the downward sweep of enemy_hit, creating a satisfying "damage -> kill" sonic arc. |

---

### 11. SFX_BOSS_ROAR

| Parameter | Value |
|-----------|-------|
| ID | sfx_boss_roar |
| Duration | 1200ms |
| Waveform | Sawtooth + Triangle + Noise |
| Frequency | Sawtooth: 150 Hz (sustained growl); Triangle: 60 Hz (sub-bass); Noise: broadband rumble |
| Volume | +2 dB (louder than music -- this is a major event) |
| ADSR | A: 100ms, D: 300ms, S: 70%, R: 600ms |
| Description | Powerful, intimidating boss entrance roar. Low, guttural, and resonant. The sawtooth provides a growling mid-frequency body. The triangle adds sub-bass weight that can be felt (especially with haptics). The noise adds texture like a roar reverberating through the level. Should stop the player in their tracks. When this plays, weapon fire continues but is momentarily eclipsed by the sheer volume and low-frequency dominance of the roar. This plays once when the boss first appears. Not repeatable -- one-shot only. |
| Synthesis Notes | Sawtooth at 150 Hz with slow vibrato (3 Hz, +/- 30 cents) for a growling quality. Triangle at 60 Hz, 80% volume, provides physical weight. Noise at 40% volume, low-pass filtered to 500 Hz. All layers ramp up over 100ms (attack), sustain for 500ms, then slowly decay over 600ms. Apply slight pitch drop on sawtooth (150->120 Hz) during decay for "settling" feel. |

---

### 12. SFX_BOSS_PHASE

| Parameter | Value |
|-----------|-------|
| ID | sfx_boss_phase |
| Duration | 1500ms |
| Waveform | Square 50% + Sawtooth + Noise |
| Frequency | Square: ascending chromatic scale (200 Hz -> 800 Hz in steps); Sawtooth: 100 Hz sustained; Noise: rising filtered sweep |
| Volume | +1 dB |
| ADSR | A: 50ms, D: 100ms, S: 80%, R: 400ms |
| Description | The boss is powering up / entering a new phase. Alarming, escalating tension. The square wave plays a rapid ascending chromatic scale (each note ~100ms), creating an "alarm siren" escalation. The sawtooth provides a sustained threatening drone underneath. Noise builds from low rumble to high hiss, suggesting energy accumulation. The player should think: "it's getting worse -- shoot faster." Plays at phase transition health thresholds (e.g., 66% HP, 33% HP). |
| Synthesis Notes | Square plays: 200, 225, 250, 280, 315, 350, 400, 450, 500, 565, 630, 710, 800 Hz -- each note held for ~90ms with 10ms gap. Sawtooth drones at 100 Hz throughout with increasing vibrato (3->8 Hz rate over duration). Noise sweep: low-pass filter cutoff rises from 200 Hz to 4000 Hz over 1200ms. Final 300ms: all sounds cut abruptly to silence for dramatic effect before new phase music/behavior begins. |

---

### 13. SFX_SPECIAL_ATTACK

| Parameter | Value |
|-----------|-------|
| ID | sfx_special_attack |
| Duration | 600ms |
| Waveform | Square 50% + Sawtooth + Noise (white) |
| Frequency | Square: 300 Hz -> 1200 Hz (aggressive upward sweep); Sawtooth: 150 Hz -> 600 Hz (parallel sweep); Noise: broadband, bandpass 2-6 kHz |
| Volume | 0 dB |
| ADSR | A: 0ms, D: 50ms, S: 80%, R: 200ms |
| Description | Dramatic, powerful blast triggered by holding jump when powered up. This is the player's ultimate ability -- it must sound devastating and cathartic. A rising dual-wave sweep communicates charging energy being unleashed. The noise layer adds an explosive, overwhelming quality. Much louder and more complex than regular weapon fire. The contrast between this sound and the quiet weapon_fire texture creates a powerful "unleashing power" moment. Should feel like all the pent-up energy from the constant auto-fire is exploding outward at once. |
| Synthesis Notes | Square 50% sweeps 300->1200 Hz over 400ms (the "charging beam" sweep). Sawtooth sweeps 150->600 Hz in parallel at 70% volume (adds harmonic thickness). Noise bandpass filtered at 2-6 kHz at 50% volume, 200ms burst at the start then fades (the "explosion" texture). At 400ms, both tonal waves hold their peak frequency for 200ms with rapid vibrato (15 Hz, +/- 30 cents) creating a "sizzling" sustain before the 200ms release fades everything to silence. |

---

### 14. SFX_TARGET_CYCLE

| Parameter | Value |
|-----------|-------|
| ID | sfx_target_cycle |
| Duration | 40ms |
| Waveform | Square 25% duty |
| Frequency | 1200 Hz (single pitch, no sweep) |
| Volume | -8 dB |
| ADSR | A: 0ms, D: 15ms, S: 0%, R: 15ms |
| Description | Quick, clean "click" confirming the player has cycled their weapon targeting to a new enemy or destructible. Ultra-short and precise. The 25% duty square wave gives it a thin, sharp quality that reads as a mechanical "selector click." Must not interfere with weapon fire texture or combat sounds. Think of it as a targeting reticle snapping to a new position. Subtle but confirmatory. Similar in spirit to menu_select but pitched higher and thinner to distinguish it as a gameplay element rather than a UI element. |
| Synthesis Notes | Single square pulse at 1200 Hz, 40ms total. Immediate onset, rapid decay. The high frequency and very short duration make it cut through the mix as a precise transient without occupying any sustained sonic space. No pitch sweep -- pitch stability communicates precision and mechanical reliability. |

---

### 15. SFX_CHECKPOINT

| Parameter | Value |
|-----------|-------|
| ID | sfx_checkpoint |
| Duration | 400ms |
| Waveform | Triangle + Square 25% |
| Frequency | Triangle: G4->C5 (two notes); Square: E5->G5 (harmony) |
| Volume | -5 dB |
| ADSR | A: 10ms, D: 50ms, S: 60%, R: 100ms |
| Description | Reassuring, safe. A gentle two-note chime that says "progress saved." Calmer and softer than the weapon_pickup sound. Triangle wave provides warmth. The square harmony adds a subtle sparkle without being aggressive. Should feel like a breath of relief amid constant shooting action. Not celebratory, just comforting. A warm "ding-dong" in a gentle register. |
| Synthesis Notes | Note 1 (0-180ms): Triangle at G4 (392 Hz) + Square at E5 (659 Hz, 40% volume). Note 2 (200-400ms): Triangle at C5 (523 Hz) + Square at G5 (784 Hz, 40% volume). The second note is higher, resolving upward -- positive, reassuring conclusion. |

---

## Additional Gameplay SFX (Jingles)

### SFX_LEVEL_COMPLETE

| Parameter | Value |
|-----------|-------|
| ID | sfx_level_complete |
| Duration | 2500ms |
| Waveform | Square 50% + Triangle + Noise (cymbal) |
| Frequency | Melody: C5-E5-G5-C6 then C6-E6-G6-C7 (ascending two-octave fanfare) |
| Volume | +1 dB |
| ADSR | A: 5ms, D: 30ms, S: 85%, R: 100ms per note |
| Description | Triumphant fanfare. The most elaborate positive SFX. An ascending major scale/arpeggio spanning two octaves with a cymbal crash at the climax. When this plays, weapon fire stops (level is over), allowing this fanfare to ring out in full glory. The silence of the auto-fire texture being gone makes this sound even more impactful. Should feel victorious and final. Think of the "stage clear" jingle from classic side-scrolling shooters. |
| Synthesis Notes | Notes 1-4 (0-800ms): C5(180ms) E5(180ms) G5(180ms) C6(260ms) on square 50%. Triangle doubles at lower octave (C4,E4,G4,C5) at 50% volume. Notes 5-8 (900-1700ms): C6(150ms) E6(150ms) G6(150ms) C7(350ms) on square 50% (faster tempo). At 1700ms: Noise cymbal crash, 800ms decay. Final note C7 sustains with gentle vibrato (4 Hz, +/- 10 cents) until 2500ms. |

---

### SFX_GAME_OVER

| Parameter | Value |
|-----------|-------|
| ID | sfx_game_over |
| Duration | 2000ms |
| Waveform | Square 50% + Triangle |
| Frequency | Descending minor phrase: D5-C5-Bb4-A4-F4-D4 |
| Volume | -2 dB |
| ADSR | A: 10ms, D: 80ms, S: 60%, R: 200ms |
| Description | Somber, final, but not punishing. A descending minor scale phrase that communicates failure without being irritating (players hear this after already dying, so do not pile on). Square wave plays the descending melody. Triangle provides a low sustained D minor pedal. Tempo is slow and deliberate -- each note lingers. The absence of weapon fire texture (already stopped at player death) leaves this playing in relative silence, giving it weight and finality. The feeling should be "that's a shame" not "you suck." Think Game Boy Tetris game-over -- dignified, brief sadness. |
| Synthesis Notes | Note sequence: D5(300ms) C5(300ms) Bb4(300ms) A4(250ms) F4(250ms) D4(600ms, held with slow decay). Triangle holds D3 (147 Hz) throughout as a pedal tone at 60% volume. Final D4 note has added vibrato (3 Hz, +/- 15 cents) and fades to silence over 600ms. |

---

## UI Sound Effects (5)

### 16. SFX_MENU_SELECT

| Parameter | Value |
|-----------|-------|
| ID | sfx_menu_select |
| Duration | 50ms |
| Waveform | Square 25% duty |
| Frequency | 800 Hz (single pitch, no sweep) |
| Volume | -10 dB |
| ADSR | A: 0ms, D: 20ms, S: 0%, R: 20ms |
| Description | Tiny, clean cursor-movement tick. Nearly inaudible -- just enough to confirm the input registered. Ultra-short. No pitch change. The 25% duty square wave gives it a thin, precise quality that does not compete with music. Like a soft typewriter key click. Must not be annoying when navigating quickly through long menus (weapon selection, era upgrades, settings). |
| Synthesis Notes | Single square pulse at 800 Hz, 50ms total. Immediate onset, rapid decay. Keep very quiet -- this is heard dozens of times in menus. |

---

### 17. SFX_MENU_CONFIRM

| Parameter | Value |
|-----------|-------|
| ID | sfx_menu_confirm |
| Duration | 100ms |
| Waveform | Square 50% duty |
| Frequency | 600 Hz -> 900 Hz (quick upward sweep) |
| Volume | -6 dB |
| ADSR | A: 0ms, D: 10ms, S: 80%, R: 50ms |
| Description | Positive confirmation "bleep." Slightly louder and more pronounced than menu_select. The upward pitch sweep communicates "yes, confirmed, proceeding." Clean, bright, affirmative. Used for starting a level, confirming weapon selection, accepting era upgrades. Definitively positive. |
| Synthesis Notes | Square sweeps 600->900 Hz over 50ms, holds 900 Hz for 10ms, then 40ms release decay. |

---

### 18. SFX_MENU_CANCEL

| Parameter | Value |
|-----------|-------|
| ID | sfx_menu_cancel |
| Duration | 100ms |
| Waveform | Square 50% duty |
| Frequency | 600 Hz -> 400 Hz (downward sweep) |
| Volume | -6 dB |
| ADSR | A: 0ms, D: 10ms, S: 80%, R: 50ms |
| Description | Negative/back "boop." The mirror of menu_confirm -- same waveform and volume, but descending pitch instead of ascending. Communicates "going back, canceling, not this." Not harsh or punishing, just clearly the opposite of confirm. Natural pair with menu_confirm. |
| Synthesis Notes | Square sweeps 600->400 Hz over 50ms, holds 400 Hz for 10ms, then 40ms release. |

---

### 19. SFX_SCREEN_TRANSITION

| Parameter | Value |
|-----------|-------|
| ID | sfx_screen_transition |
| Duration | 300ms |
| Waveform | Noise (white, filtered) |
| Frequency | Low-pass sweep: 200 Hz -> 4000 Hz -> 200 Hz (up then back down) |
| Volume | -8 dB |
| ADSR | A: 20ms, D: 0ms, S: 100%, R: 50ms |
| Description | A soft filtered noise "whoosh" accompanying screen wipes/fades. The rising-then-falling filter sweep creates a sense of movement/transition. Not musical -- purely textural. Accompanies visual transitions between menus, era selection, loading screens, or level starts. Should feel like a page turning or a curtain sweeping. Gentle and unobtrusive. |
| Synthesis Notes | White noise with an automated low-pass filter: starts at 200 Hz cutoff, sweeps to 4000 Hz over 150ms, then sweeps back to 200 Hz over 150ms. Volume envelope: 20ms fade in, sustain, 50ms fade out. |

---

### 20. SFX_ERROR

| Parameter | Value |
|-----------|-------|
| ID | sfx_error |
| Duration | 200ms |
| Waveform | Square 50% duty |
| Frequency | Two-note sequence: 300 Hz (100ms) -> 250 Hz (100ms) |
| Volume | -5 dB |
| ADSR | A: 0ms, D: 20ms, S: 70%, R: 30ms per note |
| Description | Subtle negative "nuh-uh" tone. Two descending notes indicating an invalid action (trying to select a locked era, attempting to equip an unavailable weapon, insufficient resources for upgrade, etc.). Lower register and more "buzzy" than menu sounds to clearly communicate "no." Not harsh -- the player should not feel scolded. Just informative: "that is not available." |
| Synthesis Notes | Note 1: 300 Hz square, 100ms. Note 2: 250 Hz square, 100ms. No gap between notes -- continuous. The minor second descent (close interval) creates a natural "wrong" feeling. |

---

## Priority & Layering System

### Maximum Simultaneous SFX: 4 Channels

When more than 4 SFX trigger simultaneously, the system uses priority-based voice stealing.

### Priority Tiers

| Priority | Tier Name | SFX in This Tier | Behavior |
|----------|-----------|-------------------|----------|
| 1 (Highest) | Critical Player Feedback | player_hurt, player_death, game_over | Always plays. Steals any lower-priority channel. Cannot be interrupted. |
| 2 | Boss Events | boss_roar, boss_phase, special_attack | Always plays. Steals priority 3-5 channels. |
| 3 | Reward Events | wall_break, weapon_pickup, enemy_defeat, level_complete | Plays if channel available. Will steal priority 4-5 channels. |
| 4 | Combat Feedback | jump, enemy_hit, wall_hit, target_cycle, checkpoint | Plays if channel available. Steals priority 5 only. If no channel available, dropped (not queued). |
| 5 (Lowest) | Texture / UI | weapon_fire, land, menu_select, menu_confirm, menu_cancel, screen_transition, error | Plays if channel available. If no channel available, dropped silently. Weapon fire at this tier ensures it never steals channels from meaningful combat feedback. |

### Design Rationale for Weapon Fire Priority

Weapon fire is placed at the **lowest gameplay priority** (Tier 5) because:
- It fires continuously and would otherwise dominate all 4 channels permanently.
- Dropping occasional weapon fire pulses is inaudible to the player because the constant rhythm masks gaps.
- Important combat feedback (enemy_hit, wall_break, enemy_defeat) must always take precedence.
- The weapon fire texture is perceptual -- the brain fills in missing pulses due to rhythmic expectation.

### Voice Stealing Rules

```
When SFX requests playback:
  1. Check if any of the 4 channels is idle -> use it.
  2. If all channels busy, find the channel with the LOWEST priority SFX.
  3. If requesting SFX priority > lowest playing priority:
     a. Fade out lowest-priority SFX over 10ms (avoid click).
     b. Start new SFX on that channel.
  4. If requesting SFX priority <= all playing priorities:
     a. Drop the new SFX (do not play).
     b. Log for debugging: "SFX dropped: [name], priority [X] could not steal any channel."
```

### Overlap Rules

| Scenario | Rule |
|----------|------|
| Same SFX triggered twice rapidly (e.g., rapid jumping) | Restart: stop the first instance, play from beginning. No stacking. |
| weapon_fire while enemy_hit plays | enemy_hit takes priority. Weapon fire pulse dropped. Inaudible due to rhythmic masking. |
| enemy_hit + wall_hit simultaneously | Only enemy_hit plays. Wall_hit dropped. Enemy feedback is more important. |
| wall_break + enemy_defeat simultaneously | Both play (different channels). Both are Tier 3 reward sounds and should layer. |
| player_hurt + enemy_defeat simultaneously | Both play. Player_hurt takes priority channel if needed. |
| Multiple enemies defeated at once | Only 1 enemy_defeat plays. Others dropped. Prevents SFX overload. |
| Multiple wall_break at once | Only 1 wall_break plays. Others dropped. Prevents rumble overload. |
| jump + land within 50ms | Land cancels any playing jump SFX (jump should have ended naturally anyway). |
| weapon_fire + weapon_fire (multiple attachments) | See "Multiple Weapon Layering" below. |
| UI SFX during gameplay | UI sounds play alongside gameplay SFX. They are quiet enough to not interfere. |
| level_complete during boss_phase | level_complete takes priority. Boss_phase fades out in 10ms. |
| special_attack during weapon_fire | Special_attack plays. Weapon_fire drops silently (dramatically appropriate -- the special replaces normal fire). |

### Multiple Weapon Layering

When the player has multiple weapon attachments, each fires on its own timer. To prevent cacophony:

```
Rules for multiple simultaneous weapon_fire instances:
  1. Maximum 1 weapon_fire AudioSource playing at any time.
  2. If a new weapon_fire triggers while one is already playing, the new one is DROPPED.
  3. Weapon attachments should have staggered fire rates (not synchronized) so their
     fire events naturally interleave, creating a polyrhythmic texture from a
     single SFX playing at varying intervals.
  4. The firing rate stagger is handled by gameplay code, not audio code.
  5. With 3+ attachments, the interleaved rhythm creates a pleasing "rapid pulse"
     effect using only 1 channel.
```

---

## Cooldown Timers

To prevent annoying rapid repetition:

| SFX | Minimum Interval Between Plays | Notes |
|-----|-------------------------------|-------|
| sfx_weapon_fire | 30ms | Very short -- allows rapid rhythmic pulsing but prevents overlapping. Critical for preventing cacophony with multiple attachments. |
| sfx_enemy_hit | 50ms | Frequent but needs minimum spacing for clarity. |
| sfx_wall_hit | 60ms | Slightly longer than enemy_hit to prevent rumble buildup. |
| sfx_wall_break | 100ms | Prevents multiple simultaneous destruction sounds from stacking. |
| sfx_jump | 100ms | Tied to jump input buffering. |
| sfx_land | 50ms | Brief -- landing is frequent. |
| sfx_player_hurt | 300ms | Tied to i-frame duration. |
| sfx_enemy_defeat | 100ms | Prevents multi-kill sound overload. |
| sfx_target_cycle | 80ms | Allows rapid cycling but prevents machine-gun clicking. |
| sfx_menu_select | 30ms | Fast menu navigation. |
| sfx_special_attack | 500ms | Long cooldown matches ability recharge. |
| All others | No cooldown | One-shot events (boss_roar, boss_phase, weapon_pickup, checkpoint, level_complete, game_over, menu_confirm, menu_cancel, screen_transition, error). |

---

## Haptic Feedback Pairing

Each SFX can trigger an iOS haptic event via `UIImpactFeedbackGenerator` or `UINotificationFeedbackGenerator`. Haptics are optional and can be disabled in accessibility settings.

| SFX | Haptic Type | Intensity | iOS API |
|-----|-------------|-----------|---------|
| sfx_jump | Light Impact | 0.4 | UIImpactFeedbackGenerator(.light) |
| sfx_land | Medium Impact | 0.6 | UIImpactFeedbackGenerator(.medium) |
| sfx_weapon_fire | None | -- | No haptic. Weapon fires constantly -- continuous haptic would drain battery and desensitize the player. |
| sfx_enemy_hit | Light Impact | 0.5 | UIImpactFeedbackGenerator(.light) |
| sfx_wall_hit | Light Impact | 0.3 | UIImpactFeedbackGenerator(.light) |
| sfx_wall_break | Heavy Impact | 0.8 | UIImpactFeedbackGenerator(.heavy) |
| sfx_weapon_pickup | Medium Impact | 0.6 | UIImpactFeedbackGenerator(.medium) |
| sfx_player_hurt | Notification (Error) | 1.0 | UINotificationFeedbackGenerator(.error) |
| sfx_player_death | Heavy Impact + Notification | 1.0 | UIImpactFeedbackGenerator(.heavy) -> 200ms delay -> UINotificationFeedbackGenerator(.error) |
| sfx_enemy_defeat | Light Impact | 0.3 | UIImpactFeedbackGenerator(.light) |
| sfx_boss_roar | Heavy Impact (sustained) | 1.0 | UIImpactFeedbackGenerator(.heavy) x3 at 200ms intervals |
| sfx_boss_phase | Rigid Impact | 0.9 | UIImpactFeedbackGenerator(.rigid) x5 at 150ms intervals |
| sfx_special_attack | Heavy Impact | 0.9 | UIImpactFeedbackGenerator(.heavy) -> 100ms delay -> UIImpactFeedbackGenerator(.rigid) |
| sfx_target_cycle | Selection | 0.2 | UISelectionFeedbackGenerator() |
| sfx_checkpoint | Notification (Success) | 0.5 | UINotificationFeedbackGenerator(.success) |
| sfx_level_complete | Notification (Success) | 0.8 | UINotificationFeedbackGenerator(.success) |
| sfx_game_over | Notification (Error) | 0.6 | UINotificationFeedbackGenerator(.error) |
| sfx_menu_select | Selection | 0.2 | UISelectionFeedbackGenerator() |
| sfx_menu_confirm | Light Impact | 0.4 | UIImpactFeedbackGenerator(.light) |
| sfx_menu_cancel | Light Impact | 0.3 | UIImpactFeedbackGenerator(.light) |
| sfx_screen_transition | None | -- | No haptic |
| sfx_error | Notification (Warning) | 0.4 | UINotificationFeedbackGenerator(.warning) |

### Haptic Timing

- Haptic fires at the SAME TIME as the SFX (frame 0).
- Exception: player_death uses a delayed second haptic for dramatic effect.
- Exception: boss_roar and boss_phase use repeated haptics to simulate sustained vibration.
- Exception: special_attack uses a two-stage haptic (blast then sizzle).
- **weapon_fire has NO haptic** -- continuous haptic feedback would drain battery, cause motor wear, and desensitize the player to meaningful haptic events.
- All haptic calls are dispatched on the main thread.
- Haptic preparation: call `prepare()` during scene load to eliminate first-trigger latency.

---

## Volume Mixing Reference

### Relative Volume Chart (All SFX Relative to Music at 0 dB)

```
+2 dB  |  boss_roar
+1 dB  |  boss_phase, level_complete
 0 dB  |  player_death, special_attack (MUSIC REFERENCE LEVEL)
-1 dB  |  player_hurt
-2 dB  |  wall_break, weapon_pickup, game_over
-3 dB  |  enemy_defeat
-4 dB  |  enemy_hit
-5 dB  |  checkpoint, error
-6 dB  |  jump, wall_hit, menu_confirm, menu_cancel
-8 dB  |  land, target_cycle, screen_transition
-10 dB |  menu_select
-12 dB |  weapon_fire (INTENTIONALLY VERY QUIET -- constant texture)
```

### Weapon Fire Volume Design Rationale

Weapon fire sits at -12 dB (the quietest SFX) because:
- It plays continuously and would otherwise dominate the mix.
- At -12 dB, it registers as a subtle rhythmic pulse beneath the action.
- Impact sounds (enemy_hit at -4 dB, wall_break at -2 dB) are 8-10 dB louder, ensuring they "pop" against the fire texture.
- This volume relationship creates the satisfying "shoot (quiet) -> hit (loud)" feedback loop.
- Players perceive the fire rhythm subconsciously rather than consciously -- it adds to the game's energy without demanding attention.

### Volume Groups (Unity Audio Mixer)

```
Master
+-- Music (default: -3 dB)
|   +-- LevelMusic
|   +-- BossMusic
+-- SFX (default: 0 dB)
|   +-- PlayerSFX (jump, land, weapon_fire, player_hurt, player_death, special_attack)
|   +-- CombatSFX (enemy_hit, wall_hit, wall_break, enemy_defeat, target_cycle)
|   +-- BossSFX (boss_roar, boss_phase)
|   +-- RewardSFX (weapon_pickup, checkpoint, level_complete, game_over)
|   +-- UISFX (menu_select, menu_confirm, menu_cancel, screen_transition, error)
+-- (reserved for future ambient/environmental)
```

### Weapon Fire Ducking (Optional Enhancement)

When a Tier 3 reward event plays (wall_break, weapon_pickup, enemy_defeat), the weapon_fire volume can be temporarily ducked by an additional -3 dB for 200ms. This creates a brief "clearing" effect that makes reward sounds even more prominent. Implemented via Unity Audio Mixer snapshot transitions:

```
Normal State:     weapon_fire at -12 dB
Reward Ducking:   weapon_fire at -15 dB (200ms duration, 20ms transition)
```

---

## SFX File Specifications

### Export Checklist

| SFX | Filename | Duration | Est. WAV Size | Est. OGG Size |
|-----|----------|----------|---------------|---------------|
| Jump | sfx_jump.wav | 120ms | 10 KB | 3 KB |
| Land | sfx_land.wav | 80ms | 7 KB | 2 KB |
| Weapon Fire | sfx_weapon_fire.wav | 60ms | 5 KB | 2 KB |
| Enemy Hit | sfx_enemy_hit.wav | 100ms | 9 KB | 3 KB |
| Wall Hit | sfx_wall_hit.wav | 120ms | 10 KB | 3 KB |
| Wall Break | sfx_wall_break.wav | 300ms | 26 KB | 7 KB |
| Weapon Pickup | sfx_weapon_pickup.wav | 500ms | 44 KB | 12 KB |
| Player Hurt | sfx_player_hurt.wav | 250ms | 22 KB | 6 KB |
| Player Death | sfx_player_death.wav | 800ms | 70 KB | 18 KB |
| Enemy Defeat | sfx_enemy_defeat.wav | 200ms | 17 KB | 5 KB |
| Boss Roar | sfx_boss_roar.wav | 1200ms | 105 KB | 28 KB |
| Boss Phase | sfx_boss_phase.wav | 1500ms | 132 KB | 35 KB |
| Special Attack | sfx_special_attack.wav | 600ms | 53 KB | 14 KB |
| Target Cycle | sfx_target_cycle.wav | 40ms | 3 KB | 1 KB |
| Checkpoint | sfx_checkpoint.wav | 400ms | 35 KB | 9 KB |
| Level Complete | sfx_level_complete.wav | 2500ms | 220 KB | 55 KB |
| Game Over | sfx_game_over.wav | 2000ms | 176 KB | 44 KB |
| Menu Select | sfx_menu_select.wav | 50ms | 4 KB | 1 KB |
| Menu Confirm | sfx_menu_confirm.wav | 100ms | 9 KB | 3 KB |
| Menu Cancel | sfx_menu_cancel.wav | 100ms | 9 KB | 3 KB |
| Screen Transition | sfx_screen_transition.wav | 300ms | 26 KB | 7 KB |
| Error | sfx_error.wav | 200ms | 17 KB | 5 KB |
| **TOTAL** | -- | -- | **~1,034 KB** | **~269 KB** |

All .wav files: 44.1 kHz, 16-bit, mono.
All .ogg files: Vorbis, quality 6, mono.

---

## Unity Integration Notes

### AudioSource Pool Setup

```csharp
// SFX uses a pool of 4 AudioSources for simultaneous playback
// Each AudioSource is configured:
//   - Spatial Blend: 0 (2D -- side-scroller, no 3D audio needed)
//   - Doppler Level: 0
//   - Priority: set per-SFX from priority table
//   - Volume: set per-SFX from volume table
//   - Loop: false (all SFX are one-shot)
//   - Play On Awake: false
//
// SPECIAL: weapon_fire AudioSource
//   - Dedicated 5th AudioSource (outside the pool of 4) reserved for weapon fire.
//   - This prevents weapon fire from ever competing with the 4 main channels.
//   - Volume: controlled by PlayerSFX mixer group.
//   - Can be independently muted without affecting other SFX.
//   - Pitch property used for era variant frequency shifts (see Era Variant table).
```

### Weapon Fire AudioSource (Dedicated Channel)

```csharp
// Because weapon_fire plays continuously, it gets its own dedicated AudioSource
// separate from the 4-channel SFX pool. This ensures:
//   1. Weapon fire never steals a channel from combat/reward SFX.
//   2. Weapon fire is never stolen/interrupted (it just plays or doesn't).
//   3. Era-based pitch shifting is applied to this single AudioSource.
//   4. Volume ducking can target this source specifically.
//
// Implementation:
//   - WeaponFireAudioSource.clip = sfx_weapon_fire
//   - WeaponFireAudioSource.pitch = eraMultiplier (from Era Variant table)
//   - On each weapon fire event: if cooldown elapsed, call PlayOneShot()
//   - Multiple attachments stagger their fire timing in gameplay code
```

### SFX Trigger Points (Game Events)

| Game Event | SFX Triggered | Trigger Condition |
|------------|---------------|-------------------|
| PlayerController.Jump() | sfx_jump | On jump input accepted (grounded check passed) |
| PlayerController.OnGroundContact() | sfx_land | On first ground collision after airborne state |
| WeaponSystem.OnFire() | sfx_weapon_fire | On each auto-fire tick (per attachment, subject to cooldown) |
| CombatSystem.OnEnemyHitConfirm() | sfx_enemy_hit | When projectile collider overlaps enemy hurtbox |
| CombatSystem.OnWallHitConfirm() | sfx_wall_hit | When projectile collider overlaps destructible tile (tile not destroyed) |
| DestructibleTile.OnDestroy() | sfx_wall_break | When destructible tile HP reaches 0 |
| WeaponSystem.OnWeaponPickup() | sfx_weapon_pickup | When player collider enters weapon attachment pickup trigger |
| PlayerHealth.TakeDamage() | sfx_player_hurt | When player HP decreases (and i-frames not active) |
| PlayerHealth.OnDeath() | sfx_player_death | When player HP reaches 0 |
| EnemyHealth.OnDeath() | sfx_enemy_defeat | When enemy HP reaches 0 |
| BossController.OnSpawn() | sfx_boss_roar | When boss entity first becomes active |
| BossController.OnPhaseChange() | sfx_boss_phase | When boss reaches HP threshold for next phase |
| PlayerController.OnSpecialAttack() | sfx_special_attack | When player holds jump while powered up (special meter full) |
| WeaponSystem.OnTargetCycle() | sfx_target_cycle | When player taps target-switch button |
| Checkpoint.OnActivate() | sfx_checkpoint | When player first touches an inactive checkpoint |
| LevelManager.OnLevelComplete() | sfx_level_complete | When level-end trigger is reached; weapon fire stops |
| GameManager.OnGameOver() | sfx_game_over | When game-over state is entered (after death animation) |
| UINavigator.OnSelectionChange() | sfx_menu_select | When menu cursor moves to new option |
| UINavigator.OnConfirm() | sfx_menu_confirm | When menu option is selected/confirmed |
| UINavigator.OnCancel() | sfx_menu_cancel | When back/cancel button pressed in menu |
| SceneTransition.OnTransition() | sfx_screen_transition | When screen transition animation begins |
| UINavigator.OnInvalidAction() | sfx_error | When an unavailable option is selected |

---

## Synthesis Tools (Recommended)

| Tool | Purpose |
|------|---------|
| Bfxr / jfxr | Quick chiptune SFX generation with waveform controls |
| ChipTone | Browser-based chip SFX designer |
| Audacity | Editing, trimming, volume normalization |
| sfxr | Original retro SFX generator (good starting point) |
| LMMS | DAW for more complex multi-layer SFX |

### Workflow

1. Prototype each SFX in Bfxr/sfxr using parameters from this spec
2. Fine-tune in Audacity (trim silence, normalize, verify duration)
3. **Weapon fire repetition test**: loop sfx_weapon_fire at 100ms intervals for 60 seconds. If it becomes annoying, reduce volume, shorten duration, or soften the attack further
4. **Multi-weapon layering test**: trigger sfx_weapon_fire at staggered intervals (3 sources at 80ms, 110ms, 150ms offsets) and verify the composite rhythm is pleasant
5. Export as .wav (44.1 kHz, 16-bit, mono)
6. Convert to .ogg: `ffmpeg -i sfx_name.wav -codec:a libvorbis -qscale:a 6 -ac 1 sfx_name.ogg`
7. Import into Unity, configure AudioSource settings per the tables above
8. Test all 22 SFX in-game with music playing to verify mix balance
9. Test priority/stealing system by triggering 5+ SFX simultaneously
10. Test weapon_fire ducking during reward events
11. Verify era pitch variants sound distinct but cohesive across all 10 eras

---

**Version**: 2.0
**Last Updated**: 2026-02-04
**Status**: Active
**Assessment**: 6.4 - Sound Effects Design
**Game**: 16-Bit Side-Scrolling Mobile Shooter (iOS)
