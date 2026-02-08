# Audio Squeal Issue Report

**Project:** Epoch Breaker (Unity WebGL, procedural 16-bit audio)
**Date:** 2026-02-08
**Current Build:** 034 (v1.4.2)
**Status:** Unresolved — squeal mitigated but not eliminated

This document is intended for external review. Reviewers are encouraged to examine the codebase alongside this report and propose root causes or fixes.

---

## Table of Contents

1. [Symptom Description](#1-symptom-description)
2. [Audio Architecture Overview](#2-audio-architecture-overview)
3. [Signal Chain](#3-signal-chain)
4. [Complete Fix History (Builds 023–034)](#4-complete-fix-history-builds-023034)
5. [Current Parameter State (Build 034)](#5-current-parameter-state-build-034)
6. [Key Findings and Constraints](#6-key-findings-and-constraints)
7. [Boss Combat Audio Analysis](#7-boss-combat-audio-analysis)
8. [Hypotheses for External Reviewers](#8-hypotheses-for-external-reviewers)
9. [Relevant Source Files](#9-relevant-source-files)
10. [Appendix: Waveform & Frequency Inventory](#10-appendix-waveform--frequency-inventory)

---

## 1. Symptom Description

**What the player hears:** A high-pitched squeal or whine during gameplay. The squeal is:

- **Intermittent**, not constant — it correlates with on-screen activity
- **Most pronounced during boss encounters**, particularly in Phase 3 (rapid-fire spread attacks) and Phase 4 (The Architect, Era 9)
- **Varies in intensity** with the number of simultaneous audio events
- **Not present on the title screen** (music-only playback, no SFX)
- **Present across browsers** (tested in Chrome and Firefox on macOS)

**When it was first reported:** Build 023 timeframe (2026-02-07). Has persisted through every subsequent build despite multiple mitigation attempts.

**Critical failed fix (Build 034):** An attempt to switch all combat SFX from Square/Sawtooth to Triangle waveforms caused the squeal to appear **at level start** — significantly worse than the original boss-only squeal. This was immediately reverted. The revert restored the squeal to its pre-034 behavior (boss encounters only, intermittent).

---

## 2. Audio Architecture Overview

All audio in Epoch Breaker is **procedurally synthesized at runtime** — there are no imported audio files. This is by design (placeholder audio system mirroring the procedural sprite system).

### Synthesis Engine: `PlaceholderAudio.cs`

- **Sample rate:** 44,100 Hz (mono)
- **Waveform types:** Square, Triangle, Sawtooth, Noise
- **Anti-aliasing:** PolyBLEP (Polynomial Band-Limited Step) applied to Square and Sawtooth waveforms
- **Tone generator:** `AddTone()` — additive synthesis with frequency sweep and attack/release envelope
- **Clip finalization:** `MakeClip()` — applies 3-pass IIR low-pass filter, then peak normalization
- **Caching:** All generated clips are stored in a static `Dictionary<string, AudioClip>` — each clip is synthesized exactly once per session

### Playback Engine: `AudioManager.cs`

- **Singleton** on the GameManager GameObject (DontDestroyOnLoad)
- **Audio sources:**
  - 1 looping `_musicSource` (BASE_MUSIC_VOLUME = 0.15)
  - 1 looping `_ambientSource` (BASE_AMBIENT_VOLUME = 0.08)
  - 8 pooled `_sfxSources` for environment/combat SFX (BASE_SFX_VOLUME = 0.18)
  - 3 pooled `_weaponSources` for weapon fire (BASE_SFX_VOLUME = 0.18)
- **Concurrency limit:** MAX_CONCURRENT_SFX = 3 (SFX pool only; weapon pool has no limit)
- **Repeat prevention:** MIN_REPEAT_INTERVAL = 0.05s per clip instance ID
- **Volume cap:** All PlayOneShot calls clamp at 0.8f
- **Runtime filter:** Unity `AudioLowPassFilter` component at 3000 Hz cutoff (WebGL try-catch guard)

### Level Start Audio Sequence

When a level begins (GameState.Playing transition):
1. `PlayGameplayMusicWithVariant(epoch)` is called
2. Music starts at volume 0, fades in over 200ms
3. After music fade completes, ambient starts at volume 0, fades in over 200ms
4. No SFX play at level start — SFX only trigger from gameplay events (shooting, hits, hazards)

---

## 3. Signal Chain

```
AddTone() [per-tone synthesis with PolyBLEP]
    |
    v
float[] buffer [multiple tones summed additively]
    |
    v
MakeClip() [3-pass IIR LP filter, alpha=0.20, ~2.5kHz cutoff, -18dB/oct]
    |
    v
MakeClip() [peak normalization to 0.40 target, hard clamp at ±1.0]
    |
    v
AudioClip [cached in Dictionary]
    |
    v
PlayOneShot() [volume = min(requestedVol * userVolumeSetting, 0.8)]
    |
    v
AudioSource [one of 8 SFX / 3 weapon / 1 music / 1 ambient sources]
    |
    v
AudioLowPassFilter [3000 Hz cutoff, applied to entire GameObject]
    |
    v
AudioListener [on GameManager, persistent across scenes]
    |
    v
WebGL AudioContext → Browser → Speakers
```

**Important note about the runtime LPF:** The `AudioLowPassFilter` is added to the GameManager GameObject. In Unity, `AudioLowPassFilter` affects the **listener**, not individual sources. This means ALL audio (music, ambient, SFX, weapons) passes through the same 3000 Hz filter.

---

## 4. Complete Fix History (Builds 023–034)

### Build 023 (v0.9.9) — First Audio Fix Attempt
**Changes:**
- Introduced 2-pass IIR low-pass filter in `MakeClip()` with alpha = 0.35 (~4kHz cutoff, -12dB/oct)
- Peak normalization target lowered to 0.28
- Runtime `AudioLowPassFilter` added at 2500 Hz cutoff

**Result:** Reduced harshness noticeably. Squeal still present during intense combat.

---

### Build 025 (v1.0.1) — WebGL Guard
**Changes:**
- Added try-catch around `AudioLowPassFilter` instantiation (WebGL compatibility — some browsers throw on audio filter components)

**Result:** Stability fix only. No change to squeal behavior.

---

### Build 026 (v1.0.2) — Frequency Caps + Weapon Pool
**Changes:**
- Upgraded to 3-pass LP filter in `MakeClip()` (alpha unchanged at 0.35)
- Capped SFX frequencies (Piercer: 2000→1500 Hz, Chainer: 1800→1200/1400 Hz)
- Created dedicated 3-source weapon audio pool (`_weaponSources[]`) to prevent boss/environment SFX from starving weapon fire sounds

**Result:** Improved perceived weapon responsiveness. Squeal persisted during boss encounters.

---

### Build 027 (v1.0.3) — Waveform Migration + Normalization Overhaul
**Changes:**
- Doubled loop crossfade (50ms → 100ms)
- Switched ambient and music lead waveforms from Sawtooth → Triangle
- Introduced soft-clip normalization (tanh-based saturation curve)
- "Strengthened" LP filter (details: tighter alpha, exact value not recorded)

**Result:** Music and ambient sounded cleaner. Combat SFX squeal unchanged — combat still used Square/Sawtooth.

---

### Build 029 (v1.1.1) — Aggressive Frequency Capping
**Changes:**
- All music lead instruments switched to Triangle waveform
- High notes capped at E5 (659 Hz) — removed A5/F5/G5 from lead patterns
- Hi-hat frequency lowered: 1000 → 700 Hz
- Shimmer frequency lowered: 1320 → 990 Hz
- Music fade-in on level start: 200ms
- Loop crossfade: 100ms
- Soft-clip normalization target: 0.65

**Result:** Music quality improved significantly. **Known issue documented in BUILD-LOG: "audio squeal persists."** The squeal was confirmed to be a SFX-layer problem, not music.

---

### Build 030 (v1.2.0) — Filter + Normalization Retuning
**Changes:**
- 3-pass LP filter alpha: 0.35 → 0.25 (tighter cutoff, ~3kHz)
- Normalization: switched from soft-clip saturation to linear gain (0.50 target)
- Runtime `AudioLowPassFilter`: re-added at 4000 Hz (had been removed at some point)
- `BASE_SFX_VOLUME`: 0.25 → 0.18

**Result:** Overall audio was quieter and less harsh. Squeal reduced but not eliminated during boss Phase 3.

---

### Build 033 (v1.4.1) — Targeted Squeal Mitigation
**Changes:**
- **Removed weapon pitch randomization** — `src.pitch = 1f` fixed for all weapon fire. Pitch randomization (0.95–1.05) was causing beating/interference between overlapping shots
- LP filter alpha: 0.25 → 0.20 (~2.5kHz cutoff)
- Normalization target: 0.50 → 0.40
- Runtime LPF cutoff: 4000 → 3000 Hz
- MAX_CONCURRENT_SFX: 4 → 3
- AudioListener moved from per-level camera to persistent GameManager (fixed "no audio listeners" warning)

**Result:** Best result to date. Squeal mostly gone during normal gameplay. **Still returns during boss encounters**, particularly Phase 3 rapid-fire attacks. This is the current stable state.

---

### Build 034 (v1.4.2) — REVERTED: Triangle Waveform Migration
**Changes attempted:**
- All combat SFX waveforms switched from Square/Sawtooth to Triangle:
  - `GetShootSFX`: Square 1200→300 Hz → Triangle 900→250 Hz
  - `GetEnemyHitSFX`: Square 300 Hz + Noise 500 Hz → Triangle 300 Hz + Noise 400 Hz
  - `GetStompSFX`: Square 80→40 Hz → Triangle 80→40 Hz
  - `GetBossRoarSFX`: Sawtooth 80→50 + Square 120→60 + Noise 300→100 → all Triangle, lower volumes
  - `GetChargeWindupSFX`: Sawtooth 60→200 + Noise 100→300 + Square 80→250 → all Triangle, lower freqs
- Weapon pool concurrency limit added: max 2 simultaneous weapon sounds
- Runtime LPF: 3000 → 2500 Hz

**Result: MADE SQUEAL SIGNIFICANTLY WORSE.** The squeal appeared **at level start** (not just during boss combat), which had never happened before. User reported: *"The squeal is back like it was before. The sound now is present when a level starts."*

**All changes were immediately reverted** to Build 033 audio state.

**Analysis of why this failed:** The audio clips are globally cached. Changing `GetShootSFX` from Square to Triangle changed the clip used by ALL weapon fire (player + enemies), not just boss-related sounds. The combination of:
1. Triangle waveforms at lower frequencies passing through the LP filter differently
2. The lowered runtime LPF (2500 Hz) creating a resonant peak at a different frequency
3. Multiple Triangle tones at similar frequencies causing new constructive interference patterns

...likely produced a new resonance that was audible from the very first gameplay audio event.

---

## 5. Current Parameter State (Build 034, post-revert)

### PlaceholderAudio.cs — MakeClip()
| Parameter | Value | Description |
|-----------|-------|-------------|
| LP filter passes | 3 | IIR low-pass, applied sequentially |
| LP alpha | 0.20 | ~2.5 kHz effective cutoff, -18dB/oct rolloff |
| Normalization target | 0.40 | Linear gain, hard clamp at ±1.0 |

### AudioManager.cs — Playback
| Parameter | Value | Description |
|-----------|-------|-------------|
| SFX pool size | 8 | General environment/combat SFX |
| Weapon pool size | 3 | Dedicated weapon fire |
| MAX_CONCURRENT_SFX | 3 | SFX pool concurrency limit |
| Weapon concurrency limit | None | No limit on weapon pool |
| MIN_REPEAT_INTERVAL | 0.05s | Per-clip dedup window |
| BASE_MUSIC_VOLUME | 0.15 | Music source volume |
| BASE_SFX_VOLUME | 0.18 | SFX/weapon source volume |
| BASE_AMBIENT_VOLUME | 0.08 | Ambient source volume |
| Runtime LPF cutoff | 3000 Hz | Unity AudioLowPassFilter |
| Weapon pitch | Fixed 1.0 | No randomization |
| Volume cap | 0.8 | PlayOneShot max volume |
| Music fade-in | 200ms | Linear ramp from 0 |
| Ambient stagger | 200ms | Starts after music fade |
| Loop crossfade | 100ms | ApplyLoopFade on looping clips |

### Combat SFX Waveform Summary
| SFX | Primary Waveform | Frequency Range | Duration |
|-----|-----------------|-----------------|----------|
| Shoot (Bolt) | Square | 1200→300 Hz | 80ms |
| EnemyHit | Square + Noise | 300 Hz / 500 Hz | 80ms |
| Stomp | Triangle + Noise + Square | 100→30 / 500 / 80→40 Hz | 180ms |
| BossRoar | Sawtooth + Square + Noise | 80→50 / 120→60 / 300→100 Hz | 600ms |
| ChargeWindup | Sawtooth + Noise + Square | 60→200 / 100→300 / 80→250 Hz | 500ms |
| BossPhaseChange | Sawtooth + Square + Noise + Triangle | 50→35 / 70→40 / 400→100 / 60→30 Hz | 900ms |
| BossDeath | Triangle + Noise + Square + Sawtooth | Multiple layers | 1500ms |
| Piercer fire | Sawtooth + Square | 1500→500 / 1400→800 Hz | 100ms |
| Chainer fire | Square + Noise + Triangle | 1200→600 / 1400→800 / 600→400 Hz | 120ms |
| Spreader fire | Noise + Square | 800→200 / 600→300 Hz | 100ms |
| Cannon fire | Triangle + Noise + Square | 120→40 / 500→200 / 100→50 Hz | 250ms |

---

## 6. Key Findings and Constraints

### What We Know

1. **The squeal is in the SFX layer, not music.** Music-only playback (title screen) has no squeal. The squeal correlates with SFX-heavy gameplay events.

2. **The squeal is worst during boss Phase 3.** Phase 3 has 50% attack cooldown (~1 attack/second) firing 3-projectile spreads = ~6 projectiles/second. Each projectile generates SFX on impact.

3. **Removing weapon pitch randomization helped (Build 033).** Slight pitch differences between overlapping weapon shots created audible beating/interference patterns. Fixing pitch to 1.0 reduced this.

4. **Lowering the concurrent SFX limit helped (Build 033).** Going from 4→3 max concurrent SFX reduced the number of overlapping waveforms.

5. **Switching all combat waveforms to Triangle made it WORSE (Build 034, reverted).** This is counterintuitive — Triangle waves have fewer harmonics than Square. Possible explanations:
   - Triangle waves at similar frequencies produce stronger constructive interference (smoother waveforms sum more coherently)
   - The LP filter's frequency response interacts differently with Triangle harmonic content
   - The frequency adjustments (not just waveform changes) shifted energy into a resonant band

6. **The runtime LPF at 3000 Hz may create a resonant peak.** Unity's `AudioLowPassFilter` is a single-pole IIR filter. These have a gentle rolloff with a slight gain bump near the cutoff frequency. With multiple sources passing through the same filter, this bump could amplify content near 3000 Hz.

7. **PolyBLEP anti-aliasing is correctly implemented** for Square and Sawtooth waves. The squeal is not from aliasing artifacts.

### What We Don't Know

1. **Whether the squeal is from synthesis (MakeClip) or playback (multiple AudioSources).** The clips sound fine individually. The problem may only manifest when multiple clips play simultaneously through the same audio pipeline.

2. **Whether WebGL's AudioContext introduces additional artifacts.** Unity's WebGL audio goes through the browser's Web Audio API, which has its own resampling and mixing behavior.

3. **Whether the runtime AudioLowPassFilter is actually active on WebGL.** The try-catch guard silently skips if it throws. We don't know if it's working or silently failing on any given browser.

4. **The exact frequency content of the squeal.** No spectral analysis has been performed on the actual output. The squeal's center frequency is unknown.

---

## 7. Boss Combat Audio Analysis

### Boss Attack Patterns by Phase

| Phase | Cooldown | Attacks/sec | Projectiles/attack | Audio Events/sec (est.) |
|-------|----------|-------------|--------------------|-----------------------|
| 1 | 2.0s | 0.5 | 1 | ~1 |
| 2 | 1.4s | 0.7 | 1 | ~2 |
| 3 | 1.0s | 1.0 | 3 (spread) | ~4-6 |
| 4 (Era 9) | 0.7s | 1.4 | 1-3 (mixed) | ~3-6 |

### Boss Audio Trigger Points

| Event | SFX Called | File:Line |
|-------|-----------|-----------|
| Boss intro | GetBossRoarSFX | BossArenaTrigger.cs:60 |
| Phase change | GetBossRoarSFX | Boss.cs:341 |
| Charge start | GetChargeWindupSFX | Boss.cs:718 |
| Teleport dash | GetChargeWindupSFX | Boss.cs:852 |
| Ground slam | GetStompSFX | Boss.cs:801 |
| Minion summon | GetBossRoarSFX | Boss.cs:906 |
| Boss hit | GetEnemyHitSFX | Boss.cs:1045 |
| Boss death | GetBossDeathSFX | Boss.cs:1087 |

### Worst-Case Simultaneous Audio (Phase 3)

During Phase 3 rapid-fire, the following can overlap within a 100ms window:
1. Player weapon fire (weapon pool) — every ~0.3s
2. Boss spread attack — 3 projectiles, each triggering SFX on spawn/impact
3. Enemy hit feedback — when player shots connect
4. Environmental hazards — if present in arena
5. Music loop (continuous)
6. Ambient loop (continuous)

With MAX_CONCURRENT_SFX = 3, at most 3 of the SFX events play simultaneously, plus up to 3 weapon sounds (no limit), plus music + ambient = **up to 8 simultaneous audio sources**.

### Note: GetBossPhaseChangeSFX is Unused

`PlaceholderAudio.GetBossPhaseChangeSFX()` is defined (0.9s clip with heavy Sawtooth + Square content) but **never called** anywhere in the codebase. Phase transitions use `GetBossRoarSFX()` instead. This is not related to the squeal but is worth noting for cleanup.

---

## 8. Hypotheses for External Reviewers

The following are untested hypotheses that may warrant investigation:

### H1: Constructive Interference Between Overlapping Square Waves
Multiple Square-wave SFX playing simultaneously through `PlayOneShot` on the same `AudioSource` could produce constructive interference at harmonic frequencies. When two 300 Hz square waves overlap with slight timing offsets, their 3rd harmonics (900 Hz), 5th harmonics (1500 Hz), and 7th harmonics (2100 Hz) can reinforce each other, producing energy spikes near the LP filter's cutoff.

**How to test:** Log exact timing of all `PlayOneShot` calls during a boss fight. Check for sub-10ms overlaps between SFX sharing harmonic relationships.

### H2: Unity AudioLowPassFilter Resonance
Unity's built-in `AudioLowPassFilter` has a `lowpassResonanceQ` property (default 1.0). At Q = 1.0, there may be a slight gain bump near the cutoff frequency. With 3000 Hz cutoff and multiple sources, this could amplify content in the 2500–3500 Hz range — exactly where the human ear is most sensitive.

**How to test:** Try lowering `lowpassResonanceQ` to 0.5 or removing the runtime filter entirely to see if the squeal character changes.

### H3: PlayOneShot Summing on Shared AudioSource
`PlayOneShot` allows multiple clips to play simultaneously on the same `AudioSource`. Unity sums these in the audio thread. If many short clips overlap on a single source, the summed amplitude can exceed 1.0 before Unity's internal clipping, producing distortion artifacts.

**How to test:** Instead of `PlayOneShot`, try assigning clips to individual sources with `source.clip = clip; source.Play()`. This ensures each clip has its own output stage.

### H4: WebGL AudioContext Resampling
Unity WebGL pipes audio through the browser's Web Audio API. The browser may resample from 44100 Hz to its preferred sample rate (often 48000 Hz). If the resampling algorithm introduces spectral content, it could interact with the LP-filtered audio.

**How to test:** Try generating clips at 48000 Hz sample rate. Test in a desktop Unity build (bypasses WebGL audio pipeline) to isolate whether the squeal is WebGL-specific.

### H5: 3-Pass IIR Filter Phase Response
The 3-pass LP filter in `MakeClip()` is a cascaded single-pole IIR (`prev += alpha * (sample - prev)`). Each pass adds phase shift. With 3 passes, the phase response near the cutoff frequency (~2.5 kHz) could cause the filter's step response to ring. If the input contains sharp transients (Square wave edges, even after PolyBLEP), this ringing could produce a brief tonal artifact in the 2–3 kHz range.

**How to test:** Replace the 3-pass single-pole filter with a proper biquad low-pass filter (e.g., Butterworth 2nd-order) which has a well-characterized, non-ringing response.

### H6: Normalization Amplifying Filter Ringing
`MakeClip()` normalizes to a 0.40 peak target AFTER the LP filter. If the LP filter produces a small ringing artifact at 2.5 kHz (see H5), and the rest of the clip has been attenuated by the filter, the normalization step will boost the entire clip — including the ringing — back up to 0.40 peak. This could make a subtle filter artifact clearly audible.

**How to test:** Inspect the raw float[] buffer after LP filtering and before normalization. Check if there are high-frequency ringing artifacts that are disproportionately amplified by normalization.

### H7: Weapon Pool Has No Concurrency Limit
The weapon audio pool (3 sources) has no MAX_CONCURRENT_WEAPON limit, unlike the SFX pool (MAX_CONCURRENT_SFX = 3). During rapid fire, all 3 weapon sources can play simultaneously. With the player firing continuously at a boss, 3 overlapping weapon shots + 3 overlapping SFX + music + ambient = 8 sources summed in Unity's audio mixer.

**How to test:** Add a concurrency limit (max 2) to the weapon pool. (Note: this was attempted in Build 034 alongside other changes — it should be tested in isolation.)

### H8: Per-Source AudioLowPassFilter Instead of Listener-Level
The current `AudioLowPassFilter` is attached to the GameManager GameObject and affects the **AudioListener**, meaning all audio (music, ambient, SFX, weapons) passes through a single 3000 Hz cutoff. An alternative approach: attach individual `AudioLowPassFilter` components to each `AudioSource` with per-source cutoff tuning. This would allow weapon sources to have a tighter cutoff (e.g., 2000 Hz) while leaving music and ambient with a higher cutoff (e.g., 5000 Hz) or no filter at all. Per-source filtering would prevent the single-filter resonance accumulation described in H2 and give finer control over which audio layers contribute high-frequency energy.

**How to test:** Remove the listener-level `AudioLowPassFilter`. Add an `AudioLowPassFilter` to each of the 3 weapon sources (cutoff 2000 Hz) and each of the 8 SFX sources (cutoff 2500 Hz). Leave music and ambient unfiltered. Compare squeal behavior during boss Phase 3.

### H9: The Squeal May Be a Combination Effect
The most likely explanation is that no single factor causes the squeal. It may require:
- Multiple Square-wave SFX overlapping (H1)
- The runtime LPF adding a resonant bump (H2) — or per-source LPF approach (H8)
- Normalization boosting filter artifacts (H6)
- WebGL audio pipeline adding its own artifacts (H4)

All of these interact. Fixing any single one may not be sufficient, which would explain why incremental changes have reduced but not eliminated the problem.

---

## 9. Relevant Source Files

All paths relative to `EpochBreaker/Assets/Scripts/`:

| File | Purpose | Key Lines |
|------|---------|-----------|
| `Gameplay/PlaceholderAudio.cs` | All audio synthesis | `AddTone` (L82), `MakeClip` (L114), `SampleWave` (L54), all SFX methods |
| `Gameplay/AudioManager.cs` | Audio playback, pooling, volume | `PlaySFX` (L139), `PlayWeaponSFX` (L165), `PlaySFXPitched` (L187), `PlayGameplayMusicWithVariant` (L234), `FadeInGameplayAudio` (L250) |
| `Gameplay/Enemies/Boss.cs` | Boss AI, attack patterns | Phase transitions (L341), charge (L718), teleport (L852), stomp (L801), summon (L906), hit (L1045), death (L1087) |
| `Gameplay/Enemies/BossArenaTrigger.cs` | Boss intro sequence | Intro roar (L60) |
| `Gameplay/Weapons/WeaponSystem.cs` | Weapon fire | `PlayWeaponSFX` calls during fire |
| `Gameplay/GameManager.cs` | Game state transitions | Audio triggers at state changes (L504-512) |
| `Gameplay/AbilitySystem.cs` | Player abilities | Shares `GetChargeWindupSFX` (L315) |

---

## 10. Appendix: Waveform & Frequency Inventory

### Harmonic Content by Waveform Type

| Waveform | Harmonics Present | Rolloff | Notes |
|----------|------------------|---------|-------|
| Square | Odd (1st, 3rd, 5th, 7th...) | 1/n amplitude | Rich harmonic content, PolyBLEP applied |
| Sawtooth | All (1st, 2nd, 3rd, 4th...) | 1/n amplitude | Richest harmonic content, PolyBLEP applied |
| Triangle | Odd (1st, 3rd, 5th, 7th...) | 1/n^2 amplitude | Much faster harmonic rolloff, no PolyBLEP needed |
| Noise | Broadband (all frequencies) | Flat | Deterministic pseudo-random, no anti-aliasing |

### Frequency Danger Zones

- **2–4 kHz:** Human ear is maximally sensitive (ear canal resonance). The LP filter cutoff (2.5 kHz synthesis + 3000 Hz runtime) sits right in this range.
- **3 kHz:** Runtime LPF cutoff. Any resonance peak (Q > 0.7) will amplify content here.
- **1.5–3 kHz:** 3rd/5th harmonics of SFX fundamentals in the 300–600 Hz range (EnemyHit at 300 Hz → 3rd harmonic at 900 Hz, 5th at 1500 Hz; Piercer at 1500 Hz → fundamental already in danger zone)

### All SFX with Frequencies Above 500 Hz (Fundamental)

These are the clips most likely to contribute harmonic content in the 2–4 kHz danger zone:

| Clip | Max Frequency | Waveform | Harmonics Above 2 kHz |
|------|--------------|----------|----------------------|
| Shoot (Bolt) | 1200 Hz | Square | 3rd: 3600, 5th: 6000 |
| Piercer | 1500 Hz | Sawtooth | 2nd: 3000, 3rd: 4500 |
| Chainer | 1400 Hz | Noise | Broadband above 2k |
| ChainArc | 1400 Hz | Noise | Broadband above 2k |
| Spike | 1200 Hz | Sawtooth | 2nd: 2400, 3rd: 3600 |
| MenuSelect | 880 Hz | Square | 3rd: 2640, 5th: 4400 |
| RewardPickup | 880 Hz (A5) | Square | 3rd: 2640, 5th: 4400 |
| Snare (percussion) | 500 Hz | Noise | Broadband above 2k |
| Hi-hat (percussion) | 700 Hz | Noise | Broadband above 2k |
| BossRoar Noise | 300 Hz | Noise | Broadband above 2k |
| ChargeWindup Noise | 300 Hz | Noise | Broadband above 2k |
| EnemyHit Noise | 500 Hz | Noise | Broadband above 2k |

**Key observation:** Many combat SFX include Noise waveforms that contribute broadband energy above 2 kHz. The 3-pass LP filter attenuates this but does not eliminate it. When multiple Noise-containing clips play simultaneously, their broadband content sums constructively.

---

## Revision History

| Date | Change |
|------|--------|
| 2026-02-08 | Initial document created for external review |
