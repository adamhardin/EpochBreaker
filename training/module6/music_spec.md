# Assessment 6.3: Music Composition Specification

## Overview

Complete composition specifications for a retro side-scrolling mobile shooter spanning 10 eras of human civilization. The soundtrack must evolve authentically across eras -- from primitive percussion in the Stone Age to transcendent harmonics in the final era -- while strictly adhering to retro audio constraints (8 channels maximum, chip waveforms only). This document provides two fully detailed track specs (Era 1 level theme and Era 7 boss theme), era-by-era musical guidelines for all 10 eras, a dynamic music system for in-level transitions, and all technical requirements a composer needs to produce authentic, era-evolving chiptune tracks for iOS delivery.

---

## Technical Constraints (All Tracks)

| Parameter | Value |
|-----------|-------|
| Maximum Channels | 8 simultaneous |
| Waveform Types Allowed | Square (12.5%, 25%, 50% duty), Sawtooth, Triangle, Noise |
| Sample Rate Reference | 32 kHz (SNES BRR standard) |
| Bit Depth Reference | retro (SNES SPC700) |
| Effects Allowed | Vibrato (pitch wobble), Pitch bend/slide, Volume envelope (ADSR), Echo/delay (SNES-style, max 3 taps) |
| Effects Prohibited | Reverb, chorus, flanger, phaser, compression, EQ, modern synth pads, FM synthesis |
| Final Delivery Format | .ogg (Vorbis, quality 6, ~128kbps) |
| Max File Size | 500 KB per track (compressed .ogg) |
| Loudness Target | -14 LUFS (integrated), -1 dBTP (true peak) |
| Loop Transition | Seamless -- final sample connects to loop point with zero-crossing alignment and no click/pop |

### Channel Architecture (SNES-Inspired)

The 8 available channels are assigned by role. Each track allocates channels from this pool. Earlier eras intentionally leave channels silent or sparse to reflect primitive music; later eras use all 8 at full density.

| Channel | Role Category | Typical Assignment |
|---------|--------------|-------------------|
| CH1 | Lead Melody | Primary melody line |
| CH2 | Harmony / Counter-melody | Supporting melodic line, harmonies |
| CH3 | Bass | Bass line, root movement |
| CH4 | Chord Pad / Arpeggio | Harmonic fill, chord stabs |
| CH5 | Rhythm / Percussion 1 | Kick drum, bass drum equivalent |
| CH6 | Rhythm / Percussion 2 | Snare, clap equivalent |
| CH7 | Rhythm / Percussion 3 | Hi-hat, cymbal, shaker equivalent |
| CH8 | FX / Accent | Fills, stingers, transition effects, SFX layer |

### Era-Based Channel Density

Not all eras use all 8 channels. Sparse arrangements reflect primitive eras; dense arrangements reflect advanced eras.

| Era | Active Channels | Rationale |
|-----|----------------|-----------|
| 1 - Stone Age | 4-5 (CH1, CH3, CH5, CH6, CH7) | Primitive -- percussion-heavy, minimal melody |
| 2 - Bronze Age | 5-6 | Simple melody and harmony emerge |
| 3 - Iron Age | 6 | Martial arrangements, fuller percussion |
| 4 - Medieval | 6-7 | Counterpoint requires more melodic voices |
| 5 - Renaissance | 7-8 | Richer voicing, fuller harmony |
| 6 - Industrial | 7-8 | Mechanical density, layered percussion |
| 7 - Modern | 8 (full) | Full harmonic and rhythmic palette |
| 8 - Digital | 8 (full) | Dense textures, rapid arpeggios |
| 9 - Space Age | 6-8 | Wide, atmospheric -- some channels sparse |
| 10 - Transcendent | 7-8 | Dense but ethereal, otherworldly layering |

---

## Musical Evolution Across Eras

The soundtrack tells the story of human civilization through music. Each era introduces new harmonic, rhythmic, and timbral elements while retiring others. The evolution must feel natural and progressive -- a player who plays from Era 1 through Era 10 should sense the music "growing up" alongside civilization.

### Evolution Principles

1. **Harmonic Complexity Grows**: Stone Age uses only pentatonic scales (5 notes). Each era adds harmonic vocabulary until the Transcendent era, which ventures beyond conventional tonality.
2. **Rhythmic Sophistication Increases**: Stone Age uses simple, repetitive patterns. Later eras introduce syncopation, polyrhythm, odd time signatures, and subdivision density.
3. **Melodic Range Expands**: Early eras use narrow intervals (seconds, thirds). Later eras use wide leaps (sixths, octaves, ninths).
4. **Channel Density Grows**: Early eras leave channels silent. Later eras use all 8 at high activity.
5. **Timbral Color Shifts**: Early eras favor triangle and noise (warm, primitive). Later eras incorporate all waveform types at varying duty cycles for timbral variety.

### Era-by-Era Harmonic Vocabulary

| Era | Scale/Mode | Chords Available | New Element Introduced |
|-----|-----------|-----------------|----------------------|
| 1 - Stone Age | A minor pentatonic (A-C-D-E-G) | Power fifths only (no triads) | Rhythm, pentatonic melody |
| 2 - Bronze Age | D Dorian (D-E-F-G-A-B-C) | Simple triads (Dm, C, Am) | Basic triadic harmony |
| 3 - Iron Age | E natural minor | Full diatonic triads, dominant chord | Functional harmony (V-i) |
| 4 - Medieval | D Mixolydian / G Dorian | Suspended chords, open fifths, modal interchange | Counterpoint, modal color |
| 5 - Renaissance | C Major / A minor (relative) | Seventh chords, secondary dominants | Extended harmony, modulation |
| 6 - Industrial | F minor / Bb minor | Diminished chords, tritone intervals | Dissonance as expression |
| 7 - Modern | E minor / various | All diatonic and chromatic chords, Neapolitan, augmented | Full chromatic vocabulary |
| 8 - Digital | Whole tone / Lydian | Quartal harmony, add9 chords, cluster voicings | Non-tertian harmony |
| 9 - Space Age | Open fifths / Lydian / Mixolydian | Suspended chords, polychords | Ambiguous tonality |
| 10 - Transcendent | Microtonal hints, synthetic scales | Stacked fourths, tone clusters, unresolved | Beyond Western harmony |

---

## Era Theme Guidelines (All 10 Eras)

### Era 1: Stone Age

| Parameter | Value |
|-----------|-------|
| Tempo Range | 90-110 BPM |
| Key Preferences | A minor pentatonic, E minor pentatonic |
| Mood | Primal, mysterious, earthy, raw |
| Waveform Emphasis | Noise (percussion dominant), Triangle (bass/drone), Square 50% (sparse melody) |
| Rhythmic Style | Heavy percussion, steady pulse, tribal patterns. Kick on beats 1 and 3, hand-clap sounds on 2 and 4. Minimal subdivision -- quarter and eighth notes only. Repetitive, hypnotic loops. |
| Melodic Character | Short melodic fragments (2-4 notes), call-and-response. Narrow intervals (minor thirds, whole steps). Melody appears sparingly, almost incidentally, as if hummed by a caveman. |
| Channel Usage | 4-5 channels. CH5/CH6/CH7 carry the track. CH1 plays sparse melody. CH3 plays a low drone or pedal. CH2, CH4, CH8 silent or near-silent. |
| Reference Tracks | *Ecco the Dolphin* (Genesis) -- "The Undercaves" (primal atmosphere); *ActRaiser* (SNES) -- "Filmore" (ancient world feel); *Far Cry Primal* OST (modern reference for Stone Age mood, translate to chip) |

### Era 2: Bronze Age

| Parameter | Value |
|-----------|-------|
| Tempo Range | 100-120 BPM |
| Key Preferences | D Dorian, A Aeolian |
| Mood | Ancient, ceremonial, emerging order, warmth |
| Waveform Emphasis | Triangle (bell-like tones with short decay), Square 25% (thin, reed-like melody), Noise (hand drums) |
| Rhythmic Style | Steady pulse with slight syncopation. Introduction of accented downbeats suggesting ceremony. Shaker patterns (hi-hat channel with very short decay). Rhythms suggest procession or ritual. |
| Melodic Character | Simple modal melodies, 4-8 note phrases. Introduction of stepwise motion alongside pentatonic leaps. Bronze bell accents (triangle wave with fast decay ADSR) on beat 1 of every other bar. |
| Channel Usage | 5-6 channels. CH1 melody, CH3 bass (root-fifth movement), CH5/CH6/CH7 percussion, CH8 bell accents. CH2 and CH4 begin to emerge with simple sustained tones. |
| Reference Tracks | *ActRaiser* (SNES) -- "Birth of the People" (civilization building); *Civilization IV* -- "Baba Yetu" intro feel (translate to chip); *Age of Empires* (DE) -- "Main Theme" opening measures |

### Era 3: Iron Age

| Parameter | Value |
|-----------|-------|
| Tempo Range | 115-130 BPM |
| Key Preferences | E natural minor, A minor |
| Mood | Martial, determined, brooding, expansionist |
| Waveform Emphasis | Square 50% (strong lead), Sawtooth (aggressive counter-melody), Triangle (marching bass), Noise (military percussion) |
| Rhythmic Style | Martial rhythms: snare rolls, steady march tempo. Kick on every beat (four-on-the-floor march). Snare patterns suggesting military drums. Introduction of sixteenth-note hi-hat for drive. |
| Melodic Character | Minor key melodies with wider intervals (fourths, fifths). Melody is more assertive and deliberate. Introduction of harmonic minor raised 7th for dramatic leading tones. Call-and-response between CH1 and CH2. |
| Channel Usage | 6 channels. All percussion channels active with march patterns. CH1 lead, CH2 counter-melody, CH3 walking bass. CH4 begins chord stabs. |
| Reference Tracks | *Fire Emblem: Genealogy of the Holy War* (SNES) -- battle themes; *Castlevania III* (NES) -- "Beginning" (driving minor key energy); *Rome: Total War* OST -- march themes (translate to chip) |

### Era 4: Medieval

| Parameter | Value |
|-----------|-------|
| Tempo Range | 120-140 BPM |
| Key Preferences | D Mixolydian, G Dorian, C Ionian |
| Mood | Chivalric, adventurous, noble, questing |
| Waveform Emphasis | Square 25% (lute-like thin timbre), Square 50% (trumpet-like fanfares), Triangle (bass lute/drone), Sawtooth (reserved for fanfare accents) |
| Rhythmic Style | Compound meter feel within 4/4 (triplet subdivisions suggesting 12/8). Lighter percussion -- hi-hat and snare suggest tambourine. Kick is less prominent. Rhythms suggest dance (galliard, saltarello). |
| Melodic Character | Modal counterpoint -- two independent melodic lines (CH1 and CH2) moving in contrary motion. Ornamentation: quick grace notes (pitch bends) and trills. Fanfare motifs at section transitions. Open fifth power chords suggesting lute strumming. |
| Channel Usage | 6-7 channels. CH1 and CH2 carry melodic weight equally (counterpoint). CH4 plays arpeggiated chords suggesting strummed lute. CH8 handles fanfare stabs and ornamental fills. |
| Reference Tracks | *Final Fantasy IV* (SNES) -- "Theme of Love" (medieval elegance); *Shovel Knight* -- "Main Theme" (chivalric chiptune); *The Legend of Zelda: A Link to the Past* (SNES) -- "Hyrule Castle" (noble, questing) |

### Era 5: Renaissance

| Parameter | Value |
|-----------|-------|
| Tempo Range | 110-135 BPM |
| Key Preferences | C Major, F Major, A minor, D minor |
| Mood | Elegant, refined, flourishing, harmonically rich |
| Waveform Emphasis | Square 50% (harpsichord-like lead with fast decay), Square 25% (recorder-like second voice), Sawtooth (viola/string-like sustain), Triangle (bass viol) |
| Rhythmic Style | Lighter, dance-like rhythms. Introduction of dotted rhythms (dotted quarter + eighth). Percussion pulls back further -- suggestion of gentle tapping. Emphasis shifts from rhythm to harmony and melody. |
| Melodic Character | Longer, more elaborate melodic phrases (8-16 notes). Introduction of sequences (melodic patterns repeated at different pitch levels). Imitative counterpoint: CH2 echoes CH1's melody one bar later at a different pitch. Harmonically, seventh chords and secondary dominants create richer color. |
| Channel Usage | 7-8 channels. Dense harmonic texture. CH4 plays full arpeggiated chords. CH2 carries independent counter-melody. CH8 plays ornamental runs and trills. Every channel contributes to a "chamber ensemble" feel. |
| Reference Tracks | *Final Fantasy VI* (SNES) -- "Terra's Theme" (sweeping, refined melody); *Castlevania: Symphony of the Night* (PS1) -- "Wood Carving Partita" (baroque/Renaissance elegance); *Octopath Traveler* -- town themes (warm, elegant) |

### Era 6: Industrial

| Parameter | Value |
|-----------|-------|
| Tempo Range | 130-150 BPM |
| Key Preferences | F minor, Bb minor, C minor |
| Mood | Grinding, mechanical, smoky, relentless, dark |
| Waveform Emphasis | Sawtooth (harsh, grinding lead), Square 12.5% (thin, cutting secondary), Noise (mechanical clatter, factory rhythms), Triangle (heavy, pumping bass) |
| Rhythmic Style | Driving, machine-like rhythms. Straight sixteenth-note patterns suggesting factory machinery. Kick drum doubles to eighth notes. Noise channel plays mechanical "clanking" patterns (irregular noise bursts). Syncopated accents on off-beats create a lurching, gear-grinding feel. |
| Melodic Character | Darker, minor-key melodies with chromatic inflections. Diminished intervals (tritones) appear. Melodies are shorter and more aggressive. Bass becomes a driving force -- constant eighth-note movement. Introduction of ostinato patterns (repeating melodic/rhythmic cells). |
| Channel Usage | 7-8 channels. Heavy percussion presence. CH3 bass is as prominent as CH1 lead. CH4 plays repetitive mechanical arpeggios (machine-like). CH8 plays industrial noise accents. Dense, relentless. |
| Reference Tracks | *Mega Man 2* (NES) -- "Dr. Wily Stage 1" (driving, relentless energy); *Donkey Kong Country* (SNES) -- "Fear Factory" (industrial atmosphere); *NieR: Automata* -- factory themes (translate mechanical feel to chip) |

### Era 7: Modern

| Parameter | Value |
|-----------|-------|
| Tempo Range | 140-180 BPM |
| Key Preferences | E minor, A minor, D minor, G minor |
| Mood | Intense, militaristic, urgent, aggressive, full-spectrum |
| Waveform Emphasis | All waveforms at full use. Square 50% (aggressive lead), Sawtooth (powerful counter-melody), Triangle (deep, punchy bass), Noise (full drum kit), Square 25% (chord stabs) |
| Rhythmic Style | Full rhythmic complexity. Driving kick patterns (eighth notes and dotted eighths). Complex snare patterns with ghost notes (lower volume hits). Hi-hat plays rapid sixteenths with open/closed variation. Syncopation throughout. Occasional 7/8 or 5/4 bars for tension. |
| Melodic Character | Full chromatic palette. Wide-interval melodies (octave jumps, sixths). Aggressive, staccato phrasing. Neapolitan chords (bII) for dramatic color. Augmented chords for instability. Bass plays power-fifth patterns. Heavy use of all 8 channels at maximum density. |
| Channel Usage | 8 channels, all fully active. Maximum density. Every channel contributes essential material. No resting channels. |
| Reference Tracks | *Contra III* (SNES) -- "Spirit of the Tomahawk" (military aggression); *Metal Slug* (Neo Geo) -- stage themes (intense action); *Mega Man X* (SNES) -- "Sigma Stage 1" (relentless modern urgency) |

### Era 8: Digital

| Parameter | Value |
|-----------|-------|
| Tempo Range | 140-160 BPM |
| Key Preferences | Lydian mode (raised 4th), Whole tone passages, F# minor |
| Mood | Cybernetic, glitchy, frenetic, neon-bright, electric |
| Waveform Emphasis | Square 12.5% (thin, digital-sounding lead), Square 25% (rapid arpeggios), Sawtooth (supersaw-like chords via layering), Noise (glitch textures, bit-crush percussion) |
| Rhythmic Style | Electronic dance music patterns translated to chip. Four-on-the-floor kick with syncopated snare. Rapid arpeggio patterns (sixteenth-note or thirty-second-note arpeggios on CH4). Hi-hat plays complex patterns with rests creating "stuttering" effects. Occasional beat drops (1-2 beats of silence for impact). |
| Melodic Character | Soaring synth-like melodies using wide Lydian intervals (augmented fourths). Arpeggiated chord sequences dominate. Quartal harmony (stacked fourths) replaces traditional triads in some passages. Glitch effects simulated by rapid pitch bends and staccato note cuts. Add9 chords for that shimmering digital quality. |
| Channel Usage | 8 channels at high density. CH4 runs constant arpeggios. CH8 handles glitch accents and beat-drop effects. CH2 doubles as arpeggio support. Texture is bright and busy. |
| Reference Tracks | *Mega Man 9/10* -- Wily stages (aggressive digital chiptune); *VVVVVV* OST -- "Potential for Anything" (soaring electronic chip); *Shovel Knight: Cyber Shadow* OST (modern digital chiptune) |

### Era 9: Space Age

| Parameter | Value |
|-----------|-------|
| Tempo Range | 100-130 BPM |
| Key Preferences | Open fifths, Lydian, Mixolydian, Bb Major |
| Mood | Vast, cosmic, weightless, awe-inspiring, lonely |
| Waveform Emphasis | Triangle (deep space bass, warm pads via slow ADSR), Square 50% (distant melodic calls), Sawtooth (sustained "string" tones), Noise (white noise "static" and cosmic wind) |
| Rhythmic Style | Slower, more spacious rhythms. Emphasis on half notes and whole notes in melody. Percussion is minimal and subdued -- kick is soft, hi-hat simulates quiet radar blips. Syncopation is gentle. Lots of rests and breathing room between phrases. Echo/delay (SNES-style) used prominently to simulate cosmic vastness. |
| Melodic Character | Wide intervals -- octave leaps, sevenths, ninths (simulated as octave + second). Long, sustaining melodic notes that drift. Melodies feel like radio transmissions across space. Harmony is ambiguous -- suspended chords, open fifths, polychords (two different triads layered). Lydian raised 4th gives a sense of wonder. |
| Channel Usage | 6-8 channels, but with spacious arrangement. Some channels sustain long notes rather than playing rapid patterns. Echo/delay on CH1 and CH2 is essential. CH8 plays ambient noise textures (static, cosmic wind). |
| Reference Tracks | *Super Metroid* (SNES) -- "Brinstar Depths" (atmospheric, isolated); *Outer Wilds* OST -- "Travelers" (cosmic wonder, translate to chip); *FTL: Faster Than Light* OST -- "Space Cruise" (space ambience in chip style) |

### Era 10: Transcendent

| Parameter | Value |
|-----------|-------|
| Tempo Range | 80-120 BPM (variable -- tempo shifts within tracks) |
| Key Preferences | Synthetic scales, stacked fourths, no fixed tonal center |
| Mood | Otherworldly, transcendent, unknowable, euphoric, sublime |
| Waveform Emphasis | All waveforms used in unusual combinations. Square duty-cycle sweeps (12.5% morphing to 50% within a note). Pitch bends simulating microtonal intervals. Noise used as textural wash. Triangle plays in extreme registers (very high and very low simultaneously). |
| Rhythmic Style | Fluid, shifting meter. Alternating 4/4, 5/4, and 7/8 bars. Percussion is abstract -- noise bursts at irregular intervals suggesting non-human time-keeping. Some passages have no clear beat (rubato feel achieved through very long note durations). Other passages burst into rapid, crystalline patterns. |
| Melodic Character | Melodies use intervals outside traditional Western music -- pitch bends between semitones suggest quarter-tones. Tone clusters (adjacent notes played simultaneously on CH1 and CH2). Melodies ascend continuously, never fully resolving, creating a sense of eternal rising (Shepard tone principle). Harmony is stacked fourths and unresolved suspensions. |
| Channel Usage | 7-8 channels used for texture rather than traditional roles. CH1 and CH2 may play near-unison lines a quarter-tone apart (simulated via pitch bend). CH4 plays slow, sweeping arpeggios across dissonant intervals. CH8 provides otherworldly textures. |
| Reference Tracks | *Chrono Trigger* (SNES) -- "Corridors of Time" (otherworldly, mysterious); *Undertale* -- "Hopes and Dreams" (transcendent climax); *Journey* OST -- "Apotheosis" (transcendence, translate to chip) |

---

## Track 1: Era 1 Stone Age Level Theme -- "Primal Pulse"

### Musical Parameters

| Parameter | Value |
|-----------|-------|
| Title | "Primal Pulse" |
| Tempo | 100 BPM |
| Time Signature | 4/4 |
| Key | A minor pentatonic (A-C-D-E-G) |
| Mood | Primal, mysterious, earthy, hypnotic |
| Energy Level | Medium-low (4/10) -- steady and grounded, simmering tension |
| Duration Before Loop | 60 seconds |
| Loop Point | Bar 5, beat 1 (skips intro on loop -- intro plays only on first play) |
| Total Bars | ~25 bars |

### Structure

| Section | Bars | Duration | Description |
|---------|------|----------|-------------|
| Intro ("Awakening") | 1-4 | 0:00-0:10 | 4 bars. Sparse and primal. CH5 (kick) plays a slow heartbeat pattern: two hits on beat 1 and the "and" of 2, then silence. CH7 (noise, very short decay) plays a quiet shaker rattle on every beat -- like pebbles in a gourd. CH3 (triangle) enters on bar 3 with a low A1 drone, sustained whole notes. The world is waking up. No melody yet. First-play only. |
| Verse A ("The Hunt Begins") | 5-12 | 0:10-0:29 | 8 bars. The main groove establishes. Percussion locks into a tribal loop: kick on 1 and 3, clap-snare on 2 and 4, shaker on eighth notes. CH3 bass plays a simple two-note pattern (A1 to E2, alternating bars). CH1 enters with the first melodic fragment -- a three-note call (A4-C5-D5) played in quarter notes, then silence for two beats. Call-and-response: the melody "calls," then percussion fills the space. Sparse, breathing, ancient. |
| Verse B ("Firelight") | 13-18 | 0:29-0:43 | 6 bars. Melody expands. CH1 now plays longer 5-note phrases using the full pentatonic (A4-C5-D5-E5-G5). CH3 bass becomes slightly more active -- quarter notes walking A1-C2-D2-E2. CH6 adds a deeper drum pattern (accented hits on beat 3, creating a lopsided feel). CH8 enters for the first time with a single sustained high note (A5, square 12.5%) that rings out on bar 15 like a distant animal call, then fades. Mood shifts from mysterious to subtly warm -- the tribe has gathered around a fire. |
| Bridge ("Night Watch") | 19-22 | 0:43-0:53 | 4 bars. Tension. Percussion drops to kick only (half notes). CH1 melody plays descending fragments: E5-D5-C5-A4, slow whole notes, each one lower. CH3 bass drops to A1 pedal (sustained whole notes). CH7 hi-hat plays very sparse, irregular hits (beats 1, then the "and" of 3 -- asymmetric, unsettling). Something is out there in the dark. The track thins to just 3 active channels. |
| Loop Pickup ("Dawn Returns") | 23-25 | 0:53-1:00 | 3 bars. Energy rebuilds. Percussion gradually re-enters: shaker on bar 23, snare on bar 24, full kit on bar 25. CH1 plays the three-note call from Verse A (A4-C5-D5) as a pickup into the loop. CH3 bass walks from E2 back to A1, arriving on the downbeat of the loop restart. Bar 25 ends with a CH8 noise fill (descending noise burst, 300ms) to signal the loop. |

### Channel Assignments

| Channel | Waveform | Role | Register | Volume |
|---------|----------|------|----------|--------|
| CH1 | Square 50% duty | Lead Melody Fragments | A3-G5 (narrow, primal) | 75% |
| CH2 | -- | SILENT (unused in Stone Age) | -- | 0% |
| CH3 | Triangle | Bass Drone / Pedal | A1-E2 (very low, earthy) | 80% |
| CH4 | -- | SILENT (unused in Stone Age) | -- | 0% |
| CH5 | Noise (short, punchy) | Kick Drum (heartbeat) | Low-freq noise burst, 25ms decay | 85% |
| CH6 | Noise (mid, snappy) | Hand Clap / Tribal Snare | Mid-freq noise, 50ms decay | 70% |
| CH7 | Noise (very short) | Shaker / Gourd Rattle | High-freq noise, 10ms decay | 35% |
| CH8 | Square 12.5% duty | Rare Accents / Animal Calls | A5-E6 (high, distant) | 40% |

### Melody Notation (Lead -- CH1, Simplified)

**Verse A (Bars 5-8) -- Square 50%, sparse call-and-response:**
```
Bar 5:  A4(q) C5(q) D5(q) rest(q)
Bar 6:  rest(w)
Bar 7:  A4(q) C5(q) D5(h)
Bar 8:  rest(h)             E5(q) D5(q)
```
(q = quarter note, h = half note, w = whole note, e = eighth note)

**Verse A (Bars 9-12) -- Melody repeats with slight variation:**
```
Bar 9:  A4(q) C5(q) D5(q) E5(q)
Bar 10: D5(h)             rest(h)
Bar 11: A4(q) C5(q) D5(q) rest(q)
Bar 12: rest(q) G4(q)     A4(h)
```

**Verse B (Bars 13-16) -- Expanded pentatonic phrases:**
```
Bar 13: A4(e) C5(e) D5(e) E5(e) | G5(q)        E5(q)
Bar 14: D5(q)        C5(q)      | A4(h)
Bar 15: E5(q) D5(q) C5(q) A4(q)
Bar 16: rest(h)                   G4(q) A4(q)
```

**Bridge (Bars 19-22) -- Descending, sparse:**
```
Bar 19: E5(w)
Bar 20: D5(w)
Bar 21: C5(w)
Bar 22: A4(w)
```

### Bass Line Notation (CH3, Verse A, Bars 5-12)

```
Bar 5:  A1(w)                                     [root drone]
Bar 6:  A1(w)                                     [root drone]
Bar 7:  E2(w)                                     [fifth above root]
Bar 8:  E2(w)                                     [fifth above root]
Bar 9:  A1(q) A1(q) A1(q) A1(q)                  [root pulse]
Bar 10: A1(q) A1(q) A1(q) A1(q)                  [root pulse]
Bar 11: E2(q) E2(q) E2(q) E2(q)                  [fifth pulse]
Bar 12: E2(q) E2(q) D2(q) A1(q)                  [walk down to root]
```

### Percussion Pattern (Bars 5-8, Full Tribal Groove)

```
CH5 (Kick):   |X . . . | X . . . | X . . . | X . . .|
CH6 (Clap):   |. . X . | . . X . | . . X . | . . X .|
CH7 (Shaker): |x x x x | x x x x | x x x x | x x x x|

X = accented hit, x = soft hit, . = rest
Grid: each position = eighth note (8 per bar at 100 BPM in 4/4)
```

**Expanded (with ghost notes, Bars 9-12):**
```
CH5 (Kick):   |X . . . X . . . | X . . . X . . . | X . . x X . . . | X . . . X . . .|
CH6 (Clap):   |. . . . X . . . | . . . . X . . . | . . . . X . . . | . . . . X . . .|
CH7 (Shaker): |x x x x x x x x | x x x x x x x x | x x x x x x x x | x x x x x x x x|
```

### Chord Progression Summary

| Section | Bars | Progression |
|---------|------|-------------|
| Intro | 1-4 | A pedal (open fifth A-E implied, no triad) |
| Verse A | 5-12 | Am(no3) - Am(no3) - E5 - E5 (power chords only) |
| Verse B | 13-18 | Am(no3) - D5 - Am(no3) - E5 - D5 - Am(no3) |
| Bridge | 19-22 | Am drone (bass pedal only, no chords) |
| Pickup | 23-25 | E5 - D5 -> Am(no3) (leads to loop) |

Note: "no3" and "5" designations indicate power chords (root + fifth only). The Stone Age has no triadic harmony -- only open fifths and unisons. This is a deliberate limitation reflecting the era.

### ADSR Envelope Specifications

| Channel | Attack (ms) | Decay (ms) | Sustain (%) | Release (ms) |
|---------|-------------|------------|-------------|--------------|
| CH1 (Lead) | 10 | 80 | 65 | 200 |
| CH3 (Bass) | 5 | 40 | 90 | 100 |
| CH5 (Kick) | 0 | 25 | 0 | 10 |
| CH6 (Clap) | 0 | 50 | 0 | 15 |
| CH7 (Shaker) | 0 | 10 | 0 | 5 |
| CH8 (Accent) | 15 | 120 | 40 | 300 |

Note: CH1 has a slower attack (10ms) and longer release (200ms) than later eras. This softens note onsets, giving the melody a breathy, primitive quality -- as if played on a bone flute rather than a precise instrument. Later eras will sharpen the attack for more articulate playing.

### Vibrato Settings (Lead Melody)

| Parameter | Value |
|-----------|-------|
| Rate | 3.5 Hz (slow, organic wobble) |
| Depth | +/- 20 cents (wide, raw -- imprecise pitch, like a primitive instrument) |
| Delay | 400ms (vibrato only on long-held notes -- most notes are too short) |
| Applied to | CH1 only |

Note: The wide vibrato depth (+/- 20 cents) is intentional. Stone Age music should sound slightly "out of tune" -- raw and human. Later eras tighten the vibrato to +/- 10 cents as "instruments" become more precise.

---

## Track 2: Era 7 Modern Boss Theme -- "Steel Thunder"

### Musical Parameters

| Parameter | Value |
|-----------|-------|
| Title | "Steel Thunder" |
| Tempo | 170 BPM |
| Time Signature | 4/4 (with one 7/8 bar per 8-bar phrase for rhythmic destabilization) |
| Key | E minor (natural minor base, with Neapolitan F chord and augmented chords for drama) |
| Mood | Intense, militaristic, urgent, overwhelming, industrial warfare |
| Energy Level | Very high (10/10) -- peak aggression, maximum channel density |
| Duration Before Loop | 68 seconds |
| Loop Point | Bar 5, beat 1 (skips intro on loop -- intro plays only on first play) |
| Total Bars | ~48 bars |

### Structure

| Section | Bars | Duration | Description |
|---------|------|----------|-------------|
| Intro ("Incoming") | 1-4 | 0:00-0:06 | 4 bars. Alarm sequence. CH1 plays a rapid repeating E5 staccato (sixteenth notes, square 50%) -- like a warning siren. CH3 drops a massive E1 sub-bass hit on beat 1 of bar 1 (triangle, max volume, long sustain). CH5 plays a militant snare roll crescendo across all 4 bars. CH8 plays a descending chromatic sweep E6-E5 (pitch bend, 2 bars). Bar 4: all channels hit E unison across octaves on beat 1, then silence for beats 2-4. Dread before the storm. First-play only. |
| Verse A ("Full Assault") | 5-16 | 0:06-0:22 | 12 bars. Maximum aggression from the start. Bass (CH3): relentless eighth-note octave pumps (E1-E2). Lead (CH1): angular, chromatic descending riff -- E5-Eb5-D5-C#5-C5-B4 in rapid staccato bursts. Percussion at full tilt: kick on every eighth note, snare accents on 2 and 4 with ghost notes on the "e" and "a," hi-hat in sixteenths with open hat on the "and" of 4. CH2 (sawtooth): aggressive counter-melody playing ascending power fifths (E3-B3, F3-C4, G3-D4). CH4: chord stabs -- Em, C, Am, B7 as staccato hits on beat 1 of each bar. This is the tank rolling forward. Bar 12 is 7/8 (one beat stolen -- the stumble effect). |
| Build ("Escalation") | 17-24 | 0:22-0:34 | 8 bars. Rising intensity. Lead melody climbs chromatically through octaves: the riff from Verse A repeats but starting on F5 (bar 17), then F#5 (bar 19), then G5 (bar 21). Each repetition is higher, more frantic. Bass switches to driving sixteenth notes on bars 21-24. CH2 arpeggio intensifies: cycling Em triad (E-G-B-E-B-G) in sixteenths on sawtooth. CH4 adds sustained power chords (no longer just stabs). CH8 plays offbeat accent hits -- sharp noise bursts on the "and" of every beat. Bar 20 is 7/8. Bar 24 is 7/8. Two stolen beats in 8 bars -- the escalation is literally tripping over itself with urgency. |
| Chorus ("Steel Thunder") | 25-36 | 0:34-0:50 | 12 bars. Absolute maximum intensity. All 8 channels at peak volume and activity. Lead (CH1) hits the highest register: E6, F6 (Neapolitan!), G6. The melody is triumphant-aggressive -- not heroic, but overwhelmingly powerful. Chord progression: Em - F - C - B / Em - Am - B - Em. The Neapolitan F major chord (bII) hits like a sledgehammer every time it appears. Bass plays power octaves with chromatic passing tones. Counter-melody plays in rhythmic unison with lead (power octaves). Percussion doubles: kick moves to sixteenth notes on bars 29-32. CH8 plays crash accents every 2 bars. Wall of 8-channel fury. |
| Breakdown ("Ceasefire") | 37-42 | 0:50-0:57 | 6 bars. Sudden, dramatic drop. Only CH3 (bass, whole notes, E1), CH1 (lead, sustained high E5), and CH7 (sparse hi-hat clicks) remain active. The silence of the other 5 channels is deafening after the assault. Creates the illusion the boss is staggering. Melody plays long, descending tones: E5-D5-C5-B4 (one per bar, whole notes). But on bar 40, CH3 begins a chromatic bass ascent: E1-F1-F#1-G1-G#1-A1-Bb1-B1 in quarter notes. The boss is powering back up. CH6 re-enters with a quiet, ominous military snare roll starting on bar 41. |
| Re-Attack ("No Mercy") | 43-48 | 0:57-1:08 | 6 bars. Full intensity returns with a vengeance. Abbreviated, condensed reprise of Verse A material at even higher intensity. All 8 channels re-engage by bar 44. Bars 43-44 rebuild: percussion first, then bass, then chords, then lead. Bars 45-48 are at Chorus-level intensity. Bar 48 ends on a sustained B chord (dominant of E minor) -- unresolved, demanding the loop. The final beat of bar 48 has all channels except CH3 cut to silence, leaving only the B1 bass note ringing into the loop restart. |

### Channel Assignments

| Channel | Waveform | Role | Register | Volume |
|---------|----------|------|----------|--------|
| CH1 | Square 50% duty | Lead Melody | E4-G6 (aggressive, extreme range) | 90% |
| CH2 | Sawtooth | Counter-melody / Power Arpeggio | E3-E5 (driving midrange) | 70% |
| CH3 | Triangle | Bass | E1-E3 (maximum depth, chest-punch) | 90% |
| CH4 | Square 25% duty | Chord Stabs / Power Chords | E3-E5 | 55% |
| CH5 | Noise (very punchy) | Kick Drum | Very low noise, 15ms decay, instant attack | 95% |
| CH6 | Noise (snappy, tight) | Snare / Military Roll | Mid-high noise, 40ms decay | 85% |
| CH7 | Noise (tight) | Hi-hat / Ride | High noise, 8ms closed / 35ms open | 50% |
| CH8 | Square 12.5% duty | Crash Accents / Fills / Siren FX | Variable | 65% |

### Melody Notation (Lead -- CH1, Verse A, Bars 5-12)

```
Bar 5:  E5(e) rest(e) E5(e) Eb5(e) | D5(e) rest(e) C#5(e) C5(e)
Bar 6:  B4(q)         rest(q)       | E5(e) E5(e)   rest(q)
Bar 7:  G5(e) F#5(e)  E5(e) D5(e)  | C5(q)          B4(q)
Bar 8:  E5(q)         rest(e) B4(e) | E5(e) G5(e)    rest(q)
Bar 9:  E5(e) rest(e) E5(e) Eb5(e) | D5(e) C#5(e)   C5(e) B4(e)
Bar 10: A4(q)         rest(q)       | E5(e) rest(e)  E5(e) E5(e)
Bar 11: G5(e) E5(e)   D5(e) C5(e)  | B4(e) A4(e)    G4(e) rest(e)
Bar 12: E5(q) D5(q)   C5(e) B4(e) rest(e)  [7/8 bar -- 7 eighth notes]
```

**Chorus (Bars 25-28) -- Maximum intensity:**
```
Bar 25: E6(q) E6(e) D6(e) | C6(q) B5(q)
Bar 26: F5(q) G5(q)       | A5(h)               [F natural = Neapolitan color]
Bar 27: E6(q) E6(e) F6(e) | G6(q) F6(q)         [F6 = Neapolitan peak!]
Bar 28: E6(h)              | rest(h)
```

### Bass Line Notation (CH3, Verse A, Bars 5-8)

```
Bar 5:  E1(e) E2(e) E1(e) E2(e) | E1(e) E2(e) E1(e) E2(e)   [octave pump, relentless]
Bar 6:  C1(e) C2(e) C1(e) C2(e) | C1(e) C2(e) C1(e) C2(e)
Bar 7:  A1(e) A2(e) A1(e) A2(e) | A1(e) A2(e) A1(e) A2(e)
Bar 8:  B1(e) B2(e) B1(e) B2(e) | B1(e) B2(e) B1(q)           [hold for accent]
```

**Bass Line (CH3, Chorus, Bars 25-28):**
```
Bar 25: E1(e) B1(e) E2(e) B1(e) | E1(e) B1(e) E2(e) B1(e)   [power fifth pattern]
Bar 26: F1(e) C2(e) F1(e) C2(e) | F1(e) C2(e) F1(e) C2(e)   [Neapolitan bass!]
Bar 27: C1(e) G1(e) C2(e) G1(e) | C1(e) G1(e) C2(e) G1(e)
Bar 28: B1(e) F#1(e) B1(e) F#1(e) | B1(e) F#1(e) B1(q)       [dominant hold]
```

### Counter-Melody Notation (CH2, Verse A, Bars 5-8)

```
Bar 5:  E3(e) B3(e) E3(e) B3(e) | E3(e) B3(e) E3(e) B3(e)   [power fifth oscillation]
Bar 6:  F3(e) C4(e) F3(e) C4(e) | F3(e) C4(e) F3(e) C4(e)   [shifts with harmony]
Bar 7:  G3(e) D4(e) G3(e) D4(e) | G3(e) D4(e) G3(e) D4(e)
Bar 8:  A3(e) E4(e) A3(e) E4(e) | B3(e) F#4(e) B3(q)         [accent into dominant]
```

### Percussion Pattern (Bars 5-8, Full Military Assault)

```
CH5 (Kick):   |X x X x X x X x | X x X x X x X x | X x X x X x X x | X x X x X x X x|
CH6 (Snare):  |. . . . X . . . | . . . . X . . . | . . . . X . . . | . . . . X . . .|
CH7 (Hi-hat): |x x x x x x x O | x x x x x x x O | x x x x x x x O | x x x x x x x O|

X = accented hit, x = soft hit, O = open hi-hat, . = rest
Grid: each position = sixteenth note (16 per bar at 170 BPM in 4/4)
```

Note: 16 sixteenth notes per bar at 170 BPM = each sixteenth is ~88ms. This is extremely fast. The kick plays eighth notes (every other sixteenth), the snare hits beats 2 and 4, and the hi-hat plays constant sixteenths with an open hat on the last sixteenth of each bar for drive.

**Chorus Percussion (Bars 29-32, Peak Intensity):**
```
CH5 (Kick):   |X x x x X x x x X x x x X x x x | [sixteenth note kicks!]
CH6 (Snare):  |. . . . X . . x . . . . X . . x | [ghost notes on "a" of 2 and 4]
CH7 (Hi-hat): |x x x x x x x x x x x x x x x O |
CH8 (Crash):  |X . . . . . . . . . . . . . . . | [crash on beat 1 every 2 bars]
```

### Chord Progression Summary

| Section | Bars | Progression |
|---------|------|-------------|
| Intro | 1-4 | E pedal (unison E across octaves) |
| Verse A | 5-16 | Em - C - Am - B7 (x3, bar 12 truncated in 7/8) |
| Build | 17-24 | Em - D - C - B / Em - D - C - B7 (rising chromatic bass under static chords) |
| Chorus | 25-36 | Em - F - C - B / Em - Am - B - Em (x2, Neapolitan F is key color) |
| Breakdown | 37-42 | Em(w) - Dm(w) - C(w) - [chromatic bass rise] - B(w) |
| Re-Attack | 43-48 | Em - C - Am - B / Em - Am - B - B(sustained, unresolved dominant) |

### ADSR Envelopes (Boss Theme -- Maximum Punch)

| Channel | Attack (ms) | Decay (ms) | Sustain (%) | Release (ms) |
|---------|-------------|------------|-------------|--------------|
| CH1 (Lead) | 1 | 25 | 85 | 40 |
| CH2 (Counter) | 0 | 15 | 75 | 25 |
| CH3 (Bass) | 0 | 10 | 95 | 15 |
| CH4 (Stabs) | 0 | 8 | 0 | 8 |
| CH5 (Kick) | 0 | 15 | 0 | 5 |
| CH6 (Snare) | 0 | 40 | 0 | 8 |
| CH7 (Hi-hat) | 0 | 8 | 0 | 3 |
| CH8 (Crash) | 0 | 25 | 30 | 40 |

Note: Attack times are minimal (0-1ms) for maximum punch. The 1ms attack on CH1 lead creates a near-instant onset that cuts through the dense mix. Compare with "Primal Pulse" where CH1 has 10ms attack -- the Modern era demands precision and aggression, not organic warmth.

### Vibrato Settings (Boss Lead)

| Parameter | Value |
|-----------|-------|
| Rate | 7 Hz (fast, agitated, almost tremolo) |
| Depth | +/- 12 cents (tight, precise -- military precision) |
| Delay | 80ms (vibrato kicks in almost immediately -- no leisurely build) |
| Applied to | CH1, CH2 |

Note: Compare with "Primal Pulse" vibrato: 3.5 Hz rate, +/- 20 cents depth, 400ms delay. The evolution is clear -- Stone Age is slow, wide, and imprecise. Modern is fast, tight, and controlled.

---

## Dynamic Music System

### Zone Transitions (Within Levels)

Each era's levels contain multiple gameplay zones that shift the music dynamically. The base level theme has variants (layers that can be added/removed) to match zone intensity.

#### Transition Types

| Transition | Duration | Method |
|------------|----------|--------|
| Exploration-to-Combat | 2 bars | Cross-fade: current base track fades out over 2 bars while combat variant fades in, synchronized to nearest bar line. Combat variant adds more active percussion and shifts lead to a more aggressive pattern. |
| Combat-to-Exploration | 4 bars | Longer fade to avoid jarring drop after intensity. Current phrase completes before transition begins. Percussion layers strip away first, then harmonic density reduces. |
| Level-to-Boss | Immediate (sting) | Boss intro sting plays (see below), then boss theme begins from bar 1 (intro). |
| Boss-to-Victory | Immediate (sting) | Boss music cuts immediately on final hit. Victory sting plays. Then era-appropriate level-complete fanfare plays. |
| Era Transition (level select) | 3 seconds | Music fades to silence over 1.5 seconds, then new era theme fades in over 1.5 seconds. The harmonic vocabulary, tempo, and timbre shift are too dramatic for a cross-fade to sound good -- silence acts as a palette cleanser between eras. |

#### Quantized Transitions

All in-level music transitions are **bar-quantized**: when a transition is triggered, the system waits until the next bar boundary before executing. This prevents transitions mid-phrase, which sounds jarring.

```
// Pseudocode for bar-quantized transition
void OnZoneChange(MusicZone newZone) {
    targetTrack = GetTrackForZone(newZone);
    float timeToNextBar = GetTimeUntilNextBarBoundary();
    ScheduleTransition(targetTrack, timeToNextBar, crossfadeDuration);
}
```

#### Era-Aware Transition Timing

Because eras have different tempos, transition durations vary in real time:

| Era | Tempo (typical) | 2-Bar Transition Duration | 4-Bar Transition Duration |
|-----|-----------------|---------------------------|---------------------------|
| 1 - Stone Age | 100 BPM | 4800ms | 9600ms |
| 4 - Medieval | 130 BPM | 3692ms | 7385ms |
| 7 - Modern | 170 BPM | 2824ms | 5647ms |
| 10 - Transcendent | 100 BPM | 4800ms | 9600ms |

The system must calculate transition times dynamically based on current track BPM.

### Boss Intro Sting (Era-Adaptive)

The boss intro sting adapts its tonality and character to the current era, but always follows the same structural template: a dramatic 3-second musical announcement that the boss has arrived.

#### Universal Boss Sting Template

| Parameter | Value |
|-----------|-------|
| Duration | 3 seconds |
| Channels Used | 4 (CH1, CH3, CH5, CH8) |
| Structure | Hit - Call - Silence - Ascent - Alarm - Decay |

#### Era 1 (Stone Age) Boss Sting -- "Beast Approaches"

```
Beat 1 (0.0s):   CH5 - Massive kick (longest decay, 60ms). CH3 - A1 drone (triangle, sustained).
Beat 2 (0.4s):   CH1 - A4 staccato (square, raw). CH8 - Low noise rumble (long decay, 300ms).
Beat 3 (0.8s):   Silence (dramatic -- the world holds its breath).
Beat 4 (1.2s):   CH1 - Ascending: A4-C5-E5 slow triplet. CH3 - A1 tremolo (volume wobble).
Beat 5 (1.6s):   CH5 - Three rapid kicks (triplet). CH1 - A5 sustained (alarm wail).
Beat 6 (2.0s):   CH8 - Descending noise sweep (mid to low, 500ms).
                  CH3 - A1 sub-bass final hit.
Beats 7-8 (2.4-3.0s): All channels sustain and decay into silence.
                       Boss theme begins on next downbeat.
```

#### Era 7 (Modern) Boss Sting -- "Weapons Lock"

```
Beat 1 (0.0s):   CH5 - Punching kick hit (15ms, max volume). CH3 - E1 power hit (triangle, instant attack, sustained).
Beat 2 (0.3s):   CH1 - E5 rapid staccato (sixteenth-note pulse, 4 hits). CH8 - Noise burst crash (sharp, metallic).
Beat 3 (0.6s):   Silence (0.2s only -- shorter than Stone Age, more urgent).
Beat 4 (0.8s):   CH1 - Chromatic ascending run: E5-F5-F#5-G5-G#5-A5 (sixteenths). CH3 - E1-E2 octave slam.
Beat 5 (1.2s):   CH5 - Double kick (two hits, 100ms apart). CH1 - E6 sustained (maximum alarm, piercing).
Beat 6 (1.6s):   CH8 - Descending noise sweep (high to sub, 600ms, aggressive).
                  CH3 - E1 tremolo (rapid volume oscillation).
Beat 7 (2.0s):   CH1 - B5 sustained (dominant, unresolved). CH5 - Snare roll crescendo.
Beat 8 (2.4-3.0s): All channels converge on E unison. Hard cut to silence on 2.8s.
                    Boss theme intro begins immediately (no gap).
```

### Victory Sting (Era-Adaptive)

The victory sting celebrates the boss defeat. Its character shifts across eras -- from a primal war cry in the Stone Age to a transcendent resolution in Era 10.

#### Universal Victory Sting Template

| Parameter | Value |
|-----------|-------|
| Duration | 2.5 seconds |
| Channels Used | 5 (CH1, CH2, CH3, CH4, CH8) |
| Structure | Triumphant Ascent - Harmonic Peak - Resolution |

#### Era 1 (Stone Age) Victory Sting -- "Beast Felled"

```
Beat 1 (0.0s):   CH1 - A4 (square). CH3 - A2 (triangle). Sparse, primal.
Beat 2 (0.4s):   CH1 - C5. CH5 - Kick hit (tribal). Rising.
Beat 3 (0.8s):   CH1 - E5. CH6 - Clap hit. Pentatonic climb.
Beat 4 (1.2s):   CH1 - A5 (held, highest note). CH3 - A1 (deep root, sustained).
                  CH8 - Noise burst (celebratory, 200ms).
Beats 5-7 (1.6-2.5s): CH1 sustains A5 with wide vibrato (primal war cry).
                       All other channels decay. Silence by 2.5s.
```

#### Era 7 (Modern) Victory Sting -- "Target Eliminated"

```
Beat 1 (0.0s):   CH1 - E5 (square). CH3 - E2 (triangle). CH4 - E minor power chord arpeggio up.
Beat 2 (0.3s):   CH1 - G5. CH2 - B4 (harmony third). CH4 - Faster arpeggio. Rising energy.
Beat 3 (0.6s):   CH1 - B5. CH2 - G5. CH4 - Rapid Em arpeggio (sixteenths).
Beat 4 (0.9s):   CH1 - E6 (held, triumphant peak). CH2 - B5 (held). CH3 - E1 (maximum bass root).
                  CH8 - Noise crash (300ms decay, sharp attack).
Beat 5 (1.2s):   CH4 - Em -> G -> B7 rapid chord ascent (staccato stabs, resolving upward).
Beats 6-7 (1.5-2.5s): CH1 E6 sustains with tight vibrato. CH3 E1 sustains.
                       All other channels cut sharply at 2.0s. CH1 and CH3 fade by 2.5s.
                       Military precision -- the celebration is brief and controlled.
```

---

## Reference Tracks (Era-Specific)

### Stone Age (Era 1) References

| Game | Track | What to Study |
|------|-------|---------------|
| Ecco the Dolphin (Genesis) | "The Undercaves" | Primal atmosphere within chip constraints. Use of sparse arrangement and echo to create vastness. How silence and space create mystery. |
| ActRaiser (SNES) | "Filmore (Act 1)" | Ancient-world mood in retro. Simple melodic fragments that suggest early civilization. Warm triangle bass tones. |
| Secret of Mana (SNES) | "Into the Thick of It" | Organic, earthy soundscape. Percussion-forward arrangement. How limited channels create an immersive natural environment. |
| Shovel Knight | "The Apparition" (Lich Yard) | Sparse, atmospheric chiptune. Long rests between melodic phrases. Restraint as a compositional tool. |

### Modern (Era 7) References

| Game | Track | What to Study |
|------|-------|---------------|
| Contra III (SNES) | "Spirit of the Tomahawk" | Maximum military aggression within SNES channels. Driving bass and percussion. How to create a sense of overwhelming force. |
| Mega Man X (SNES) | "Sigma Stage 1" | Relentless urgency. Rapid chord changes maintaining tension. The feeling of escalating threat. How chromatic movement creates dread. |
| Metal Slug 3 (Neo Geo) | "Final Attack" | Modern warfare intensity in chip style. Dense percussion layering. How to sustain high energy across a full loop without listener fatigue. |
| Undertale | "Megalovania" | Modern reference for driving boss music with chiptune influence. Strong melodic hook in a battle context. Rhythmic intensity that stays engaging through repetition. How odd-meter interruptions (7/8 bars) create urgency. |

### Cross-Era References (General Guidance)

| Game | Track | Applicable Era(s) | What to Study |
|------|-------|-------------------|---------------|
| Chrono Trigger (SNES) | "Corridors of Time" | Era 9, 10 | Otherworldly atmosphere within SNES hardware. How unusual scales create a sense of the unknown. |
| Final Fantasy VI (SNES) | "Terra's Theme" | Era 5, 6 | Sweeping melodic development across sections. How a theme can feel "civilized" and refined. |
| Castlevania: Rondo of Blood (PCE) | "Opposing Bloodlines" | Era 3, 4 | Dark, dramatic intensity. Minor key with chromatic passages. Medieval/gothic atmosphere. |
| Donkey Kong Country 2 (SNES) | "Stickerbrush Symphony" | Era 9 | Atmospheric layering within chip constraints. How limited channels can feel vast and spacious. |
| Mega Man 9 (Wii/NES-style) | "We're the Robots" | Era 8 | Digital/electronic energy in pure chiptune. Rapid arpeggios and bright, cutting timbres. |
| Final Fantasy VI (SNES) | "Decisive Battle" | Era 6, 7 | Epic boss battle energy. How the track builds intensity across its loop. Effective use of all 8 SPC700 channels. |

---

## Audio File Specifications

### Export Settings

| Parameter | Level Themes (All Eras) | Boss Themes (All Eras) | Stings |
|-----------|------------------------|----------------------|--------|
| Duration | 48-72 seconds (varies by era) | 60-80 seconds (varies by era) | 2-3 seconds |
| Sample Rate | 44.1 kHz (export) | 44.1 kHz (export) | 44.1 kHz |
| Channels | Stereo | Stereo | Stereo |
| Format (working) | .wav retro | .wav retro | .wav retro |
| Format (game) | .ogg Vorbis q6 | .ogg Vorbis q6 | .ogg Vorbis q6 |
| Est. File Size | 250-450 KB | 350-500 KB | 15-30 KB |
| Loop Start Sample | Varies (see per-track spec) | Varies (see per-track spec) | N/A (no loop) |
| Loop End Sample | Last sample | Last sample | Last sample |

### Per-Track Export Details

| Track | Duration | Loop Start (sample offset at 44.1kHz) | Est. Size |
|-------|----------|----------------------------------------|-----------|
| "Primal Pulse" (Era 1 Level) | 60 seconds | 44,100 * (10/1) = 441,000 (bar 5 at 100 BPM) | ~300 KB |
| "Steel Thunder" (Era 7 Boss) | 68 seconds | 44,100 * (6/1) = 264,600 (bar 5 at 170 BPM, approx.) | ~450 KB |
| Boss Intro Sting (any era) | 3 seconds | N/A | ~25 KB |
| Victory Sting (any era) | 2.5 seconds | N/A | ~20 KB |

Note: Loop start sample offsets are approximate. Exact values must be determined during mastering by identifying the precise zero-crossing at the target bar boundary.

### Stereo Panning

All channels are center-panned by default. Subtle panning is allowed for width:

| Channel | Pan |
|---------|-----|
| CH1 (Lead) | Center (0) |
| CH2 (Counter) | Slight left (-15%) |
| CH3 (Bass) | Center (0) |
| CH4 (Pad/Chords) | Slight right (+15%) |
| CH5 (Kick) | Center (0) |
| CH6 (Snare) | Center (0) |
| CH7 (Hi-hat) | Slight right (+10%) |
| CH8 (FX) | Variable per phrase |

Note: Panning is consistent across all eras. The spatial image should remain stable so the player's ears adapt to consistent stereo placement regardless of era.

### File Naming Convention

All music files follow this naming convention for organization across 10 eras:

```
era{XX}_{type}_{name}.ogg

Examples:
  era01_level_primal_pulse.ogg
  era01_boss_mammoth_rage.ogg
  era01_sting_boss_intro.ogg
  era01_sting_victory.ogg
  era07_level_frontline.ogg
  era07_boss_steel_thunder.ogg
  era07_sting_boss_intro.ogg
  era07_sting_victory.ogg
  era10_level_ascension.ogg
  era10_boss_singularity.ogg
```

---

## Composition Tools (Recommended)

| Tool | Purpose | Notes |
|------|---------|-------|
| FamiTracker | Composition & prototyping | Use for NES-authentic prototyping, then translate to SNES channel count |
| DefleMask | Multi-platform tracker | SNES mode supports 8 channels with BRR samples |
| OpenMPT | Module tracker | Modern tracker with retro sound capability |
| LMMS | DAW with chip synths | TripleOscillator and BitInvader plugins for chip waveforms |
| Audacity | Loop point editing | Verify seamless loops by crossfade testing |

### Workflow

1. Compose in tracker (FamiTracker or DefleMask) to enforce channel limits
2. Export as .wav
3. Verify loop points in Audacity (zoom to sample level, check zero-crossings)
4. Convert to .ogg using FFmpeg: `ffmpeg -i track.wav -codec:a libvorbis -qscale:a 6 track.ogg`
5. Test in Unity AudioSource with `loop = true` and verify seamless playback
6. Validate file size is under 500 KB per track

### Era Composition Order (Recommended)

Compose tracks in this order to ensure the evolutionary arc is coherent:

1. Era 1 (Stone Age) -- establish the baseline
2. Era 7 (Modern) -- establish the peak complexity
3. Era 4 (Medieval) -- establish the midpoint
4. Eras 2, 3 -- fill in early progression
5. Eras 5, 6 -- fill in middle progression
6. Eras 8, 9, 10 -- complete the arc beyond Modern

This "bookend first, then fill" approach ensures the evolutionary arc has clear start and end points before intermediate eras are composed.

---

## Unity Integration Notes

### AudioSource Configuration

```
Level Theme AudioSource (any era):
  - Clip: era{XX}_level_{name}.ogg
  - Loop: true
  - Volume: 0.7
  - Priority: 0 (highest)
  - Spatial Blend: 0 (2D)
  - Doppler Level: 0

Boss Theme AudioSource (any era):
  - Clip: era{XX}_boss_{name}.ogg
  - Loop: true
  - Volume: 0.75
  - Priority: 0 (highest)
  - Spatial Blend: 0 (2D)
  - Doppler Level: 0

Sting AudioSource (shared, one-shot):
  - Clip: (loaded dynamically)
  - Loop: false
  - Volume: 0.85
  - Priority: 0 (highest)
  - Spatial Blend: 0 (2D)
  - Doppler Level: 0
```

### Music Manager State Machine

```
States:
  SILENT -> PLAYING_LEVEL -> TRANSITION -> PLAYING_BOSS -> TRANSITION -> PLAYING_VICTORY -> PLAYING_LEVEL

Events:
  LEVEL_START      -> Fade in era-appropriate level theme over 1 second
  ZONE_COMBAT      -> Cross-fade to combat variant (bar-quantized, 2 bars)
  ZONE_EXPLORE     -> Cross-fade to exploration variant (bar-quantized, 4 bars)
  BOSS_TRIGGER     -> Play era-appropriate boss sting, then start boss theme
  BOSS_DEFEATED    -> Play era-appropriate victory sting, then play level-complete fanfare
  LEVEL_COMPLETE   -> Play era-complete fanfare, then fade to SILENT over 2 seconds
  ERA_TRANSITION   -> Fade out over 1.5 seconds, silence 0.5 seconds, fade in new era over 1.5 seconds
  PAUSE            -> Reduce volume to 30%, maintain playback position
  UNPAUSE          -> Restore volume over 0.5 seconds
```

### Era-Based Music Loading

```csharp
// Pseudocode for era-based music management
public class EraMusicManager : MonoBehaviour {
    [System.Serializable]
    public struct EraMusic {
        public int eraNumber;           // 1-10
        public AudioClip levelTheme;
        public AudioClip bossTheme;
        public AudioClip bossIntroSting;
        public AudioClip victorySting;
        public float levelLoopStartSample;
        public float bossLoopStartSample;
    }

    public EraMusic[] eraTracks;  // 10 entries, one per era

    public void LoadEra(int eraNumber) {
        var era = eraTracks[eraNumber - 1];
        levelSource.clip = era.levelTheme;
        bossSource.clip = era.bossTheme;
        // Set loop points based on era-specific sample offsets
    }
}
```

---

**Version**: 2.0
**Last Updated**: 2026-02-04
**Status**: Active
**Assessment**: 6.3 - Chiptune Music Composition (Era-Evolving Soundtrack)
