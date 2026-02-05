# ADR-006: Audio Middleware

**Date:** 2026-02-04
**Status:** Accepted

## Context

The game's retro aesthetic demands a chiptune-inspired soundtrack and retro sound effects, but with modern production quality and dynamic mixing. The audio system must support:

1. **Real-time adaptive mixing.** Music intensity should shift in response to gameplay (combat, exploration, boss encounters) without audible seams or pops.
2. **Chiptune playback fidelity.** The soundtrack uses synthesised waveforms (square, triangle, sawtooth, noise) that must play back without artefacts, aliasing, or unwanted filtering.
3. **Haptic synchronisation.** iOS Core Haptics integration requires precise timing data from the audio engine to trigger haptic feedback on beat hits, impacts, and collectible pickups.
4. **Low-latency SFX.** Sound effects must play within one audio buffer frame (~5 ms) of the triggering event to feel responsive.
5. **Procedural variation.** Sound effects should support randomised pitch, volume, and sample selection to avoid repetitive audio in procedurally generated levels.
6. **Memory efficiency.** The total audio memory footprint should stay under 40 MB to fit within our overall 500 MB budget (ADR-005).

### Options Evaluated

| Criteria | Unity Native Audio | FMOD Studio | Wwise |
|---|---|---|---|
| Adaptive music | Limited (scripted crossfades) | Excellent (parameter-driven transitions, stingers) | Excellent |
| Authoring tools | Unity mixer (basic) | FMOD Studio (professional DAW-like) | Wwise Authoring (professional) |
| iOS Core Haptics integration | Manual implementation | Plugin available, timing API accessible | Manual implementation |
| Latency | ~10-20 ms (AudioSource) | ~3-5 ms (FMOD low-level) | ~3-5 ms |
| Memory management | Limited control | Fine-grained bank loading/unloading | Fine-grained |
| Unity integration | Native | Official Unity plugin, well-maintained | Official Unity plugin |
| Licensing | Included with Unity | Free under $200K revenue, then tiered | Free under $150K revenue, then tiered |
| Team familiarity | Medium | High | Low |

Unity's native audio system (`AudioSource`, `AudioMixer`) can handle basic requirements but lacks the authoring workflow and runtime flexibility needed for adaptive music. Building equivalent functionality from scratch on top of Unity audio would require significant engineering effort with inferior results.

Wwise is a strong professional option but the team has no prior experience with it, and its authoring tool has a steeper learning curve. FMOD Studio offers comparable runtime capabilities with a more intuitive authoring environment and existing team expertise.

## Decision

We will use **FMOD Studio** as the audio middleware, integrated via the official **FMOD for Unity** plugin.

### Architecture

- **FMOD Studio project** (.fspro) lives in the repository under `Audio/FMODProject/`. The sound designer authors all events, buses, snapshots, and parameter automations in FMOD Studio.
- **FMOD Banks** are built from the Studio project and placed in `StreamingAssets/FMODbanks/`. Banks are loaded and unloaded per biome to control memory usage.
- **Adaptive music** is implemented using FMOD's parameter system. A `GameIntensity` parameter (0.0 - 1.0) drives transitions between exploration, action, and boss music layers. The game code sets this parameter; FMOD handles all crossfading and transition logic.
- **SFX events** use FMOD's multi-instrument and scatterer features for procedural variation (random pitch, random sample selection).
- **Haptic bridge.** A lightweight C# wrapper reads FMOD's playback timeline markers and beat callbacks to dispatch `CHHapticEvent` calls via a native iOS plugin. This provides beat-synchronised haptics with sub-frame accuracy.
- **Chiptune playback.** The soundtrack is authored as tracker-style sequences in FMOD Studio. Instrument DSP chains use FMOD's built-in oscillator and bitcrusher effects to produce authentic 8-bit/retro waveforms while maintaining modern mixing (EQ, compression, reverb sends for spatial depth).

### FMOD Configuration

- Output format: Stereo, 48 kHz, 256-sample buffer (5.3 ms latency on iOS).
- Virtual voice limit: 32 active, 64 virtual.
- Sample loading: Compressed in-memory for SFX (< 1 second), streaming for music tracks.
- Bank loading strategy: Master bank always loaded; biome-specific banks loaded during level generation, unloaded on biome change.

## Consequences

### Positive

- **Professional adaptive music.** FMOD Studio's parameter-driven transition system lets the sound designer author complex adaptive music behaviours without writing game code. Changes to music behaviour do not require code changes or builds.
- **Low-latency playback.** FMOD's low-level API provides sub-5ms latency on iOS, meeting our responsiveness requirement for sound effects.
- **Haptic synchronisation.** FMOD's timeline marker and beat callback APIs provide the precise timing data needed for Core Haptics integration without building a custom timing system.
- **Memory control.** Per-biome bank loading/unloading keeps audio memory in the 20-35 MB range, well under our 40 MB budget.
- **Procedural SFX variation.** Built-in randomisation features (multi-instruments, pitch/volume ranges) eliminate repetitive audio in procedurally generated levels without custom code.
- **Team velocity.** The sound designer can iterate on audio independently of the engineering team, using FMOD Studio's live-update feature to audition changes in real time on a connected device.

### Negative

- **Additional dependency.** FMOD introduces a third-party SDK into the build pipeline. FMOD Studio versions must be kept in sync with the Unity plugin version. Build documentation must include FMOD bank build steps.
- **Licensing cost.** FMOD is free for projects earning under $200K annual revenue. Above that threshold, licensing fees apply on a tiered scale. This is an expected and acceptable cost at the revenue levels where it triggers.
- **Plugin binary size.** The FMOD Unity plugin adds approximately 3-5 MB to the iOS binary. This is minor relative to the overall Unity binary size (ADR-001).
- **Debugging complexity.** Audio issues may require debugging in both Unity and FMOD Studio. FMOD's profiler mitigates this but adds another tool to the workflow.
- **Native plugin maintenance.** The haptic bridge requires a small native iOS plugin (Objective-C / Swift) that must be maintained alongside Unity and FMOD updates. This is a narrow surface area but introduces a build dependency on Xcode-compiled native code.
