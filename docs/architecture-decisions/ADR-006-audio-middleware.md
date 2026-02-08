# ADR-006: Audio System

**Date:** 2026-02-04
**Status:** Accepted
**Revised:** 2026-02-07 (updated to reflect implemented approach)

## Context

The game's retro aesthetic demands a chiptune-inspired soundtrack and retro sound effects. The audio system must support:

1. **Era-adaptive music.** Each of the 10 epochs needs distinct music that matches its historical theme.
2. **Chiptune synthesis.** The soundtrack uses synthesised waveforms (square, triangle, sawtooth, noise) for authentic retro feel.
3. **Low-latency SFX.** Sound effects must feel responsive to gameplay events.
4. **Procedural variation.** Pitch randomisation and multiple variants prevent repetitive audio.
5. **Zero imported assets.** Consistent with the project's fully-procedural philosophy — all sprites and audio generated at runtime.

### Options Evaluated

| Criteria | Unity Native Audio | FMOD Studio | Runtime Synthesis |
|---|---|---|---|
| Adaptive music | Limited (scripted crossfades) | Excellent (parameter-driven) | Full control (code-driven) |
| Asset size | Requires imported audio files | Requires FMOD banks | Zero — all generated at runtime |
| Chiptune quality | Depends on imported samples | Good with DSP chains | Excellent — direct waveform control |
| Complexity | Low | Medium (external tool) | High (custom synthesis code) |
| Dependencies | None | FMOD SDK + Studio | None |

The original plan (ADR-006 v1) specified FMOD Studio as the audio middleware. During implementation, the team chose runtime synthesis instead, for two reasons:

1. **Zero-asset philosophy.** The project generates all visual assets at runtime via `PlaceholderAssets.cs`. Extending this to audio eliminates all imported media files, keeping the project fully self-contained.
2. **Direct waveform control.** Runtime synthesis provides precise control over chiptune aesthetics — PolyBLEP anti-aliased oscillators, procedural melody generation, era-specific instrument palettes — without the overhead of an external authoring tool.

## Decision

All audio is **synthesised at runtime** using Unity's `AudioClip.Create()` API. No external audio middleware (FMOD, Wwise) is used. No audio files are imported.

### Architecture

- **`PlaceholderAudio.cs`** — Central synthesis engine. Generates all music, SFX, and ambient audio clips at startup using procedural waveforms.
  - Waveform types: Triangle, Square, Sawtooth, Noise (with PolyBLEP anti-aliasing for Square/Sawtooth)
  - Music: 3 variants per era group (30 total), era-appropriate instrument palettes and tempos
  - SFX: Per-weapon-type fire sounds, hit sounds, pickup sounds, UI sounds
  - Ambient: Era-specific environmental loops (wind, machinery, digital hum)

- **`AudioManager.cs`** — Playback manager (singleton on GameManager GameObject, persists across scenes).
  - 1 looping `AudioSource` for music
  - 1 looping `AudioSource` for ambient
  - 8-channel SFX pool (round-robin `PlayOneShot`)
  - 3-channel dedicated weapon SFX pool (prevents weapon fire being starved by environment SFX)
  - Runtime `AudioLowPassFilter` (4000Hz cutoff) to attenuate harsh high-frequency content
  - Per-clip rapid-fire prevention (50ms minimum repeat interval)
  - Concurrent SFX cap (max 4) to prevent audio overload
  - Volume settings persisted to PlayerPrefs (Music, SFX, Weapon)

- **Synthesis pipeline** (in `MakeClip()`):
  1. Waveform generation via `AddTone()` at 44100Hz sample rate
  2. 3-pass low-pass filter (alpha 0.25) before normalization
  3. Peak normalization to 0.50 target with hard clamp at ±1.0
  4. `AudioClip.Create()` with PCM float data

### Audio Filtering

- **Compile-time:** 3-pass single-pole IIR low-pass in `MakeClip()` attenuates broadband noise content above ~3kHz
- **Runtime:** Unity `AudioLowPassFilter` component (4000Hz cutoff) on the AudioManager GameObject, with WebGL try-catch guard for browser compatibility

## Consequences

### Positive

- **Zero imported assets.** The entire game — code, sprites, audio — ships with no media files. Build size is minimal.
- **Perfect chiptune aesthetics.** Direct waveform synthesis produces authentic retro audio with precise control over timbre, pitch, and envelope.
- **Era-adaptive music.** Each epoch group has 3 music variants with era-appropriate instruments and tempos, selected randomly per level.
- **No external dependencies.** No FMOD SDK, no audio banks, no authoring tool versioning issues. The audio system is pure C#.
- **Procedural variation.** Weapon SFX use ±10% pitch randomisation. Music variants rotate per level. No two play sessions sound identical.

### Negative

- **Synthesis complexity.** The `PlaceholderAudio.cs` file is large (~1200 lines) and requires audio DSP knowledge to maintain.
- **Limited dynamic mixing.** No parameter-driven crossfades or real-time intensity scaling (FMOD excels at this). Music transitions are handled by stopping/starting clips with a 200ms fade-in.
- **High-frequency artifacts.** Procedural synthesis (especially noise waveforms and harmonic interference between simultaneous sources) can produce harsh high-frequency content. Mitigated by multi-stage LP filtering but requires ongoing tuning.
- **No haptic synchronisation.** Without FMOD's timeline marker API, beat-synchronised haptics would require a custom timing system (not yet implemented).

### Superseded

This ADR supersedes the original v1 which specified FMOD Studio as the audio middleware. FMOD was never integrated; runtime synthesis was implemented from the start.
