# ADR-005: Minimum Device Target

**Date:** 2026-02-04
**Status:** Accepted

## Context

We need to establish a minimum supported device that balances two competing goals:

1. **Install base coverage.** Supporting older devices increases the addressable market.
2. **Performance headroom.** The game has real-time performance requirements that constrain how far back we can reach.

Key performance requirements:

| Requirement | Target |
|---|---|
| Frame rate | Stable 60 fps during gameplay |
| Level generation time | < 150 ms (ADR-004) |
| Memory budget (game total) | < 500 MB |
| Thermal sustainability | No thermal throttling during 30-minute play sessions |

The game's rendering workload is moderate (2D URP, tilemap, sprite particles, 2D lighting), but the procedural generation pipeline (ADR-004) is CPU-intensive and must complete within a single frame's worth of perceived latency.

### Device Benchmarks

We benchmarked the generation pipeline prototype on several devices:

| Device | SoC | Gen Time (256x16) | 60fps Headroom | Notes |
|---|---|---|---|---|
| iPhone SE 2 (2020) | A13 Bionic | ~120 ms | Adequate | Smallest A13 screen (4.7") |
| iPhone 11 | A13 Bionic | ~110 ms | Good | 6.1" display, adequate RAM (4 GB) |
| iPhone XR | A12 Bionic | ~190 ms | Marginal | Exceeds 150ms budget |
| iPhone X | A11 Bionic | ~260 ms | Poor | Significantly over budget |
| iPhone 13 | A15 Bionic | ~45 ms | Excellent | |
| iPhone 15 | A16 Bionic | ~35 ms | Excellent | |

The A12 Bionic (iPhone XR, XS) marginally exceeds our generation time budget and has less thermal headroom for sustained gameplay. The A13 Bionic (iPhone 11, SE 2) meets all targets with comfortable margin.

### iOS Version Considerations

- **iOS 15** is the oldest version that supports all Unity 6 features we require and provides the Metal API level we target. (Note: project migrated from Unity 2022 LTS to Unity 6000.3.6f1 before first build.)
- As of early 2026, iOS 15+ covers approximately 97% of active iPhone users (Apple does not publish exact figures, but adoption data from analytics services supports this estimate).
- iOS 15 is the minimum version supported on iPhone 6s and later, but our hardware floor (A13) is more restrictive than the OS floor.

## Decision

The minimum supported configuration is:

- **Hardware:** iPhone 11 (A13 Bionic) and all later iPhones, including iPhone SE 2nd generation (2020) and iPhone SE 3rd generation (2022).
- **Operating System:** iOS 15.0 and later.
- **Metal API:** Metal 2.2 (supported by A13 and later).

The Xcode project will set:

- `IPHONEOS_DEPLOYMENT_TARGET = 15.0`
- Unity Player Settings: Target minimum iOS Version = 15.0

### Level Size Constraints

To guarantee the 150 ms generation budget is met on the baseline A13 device:

- Maximum level grid size: **256 x 16 tiles** (width x height).
- On devices with A15 or later, an optional "extended level" mode may increase this to 512 x 16 tiles, detected at runtime via `SystemInfo.processorType` or generation-time benchmarking on first launch.

## Consequences

### Positive

- **Reliable performance.** The A13 Bionic provides a comfortable margin for 60 fps rendering and sub-150ms generation. We are not riding the edge of feasibility.
- **Strong install base.** iPhone 11 launched in September 2019. Combined with SE 2 (2020) and SE 3 (2022), the A13+ installed base represents the vast majority of active iPhones as of 2026.
- **Metal 2.2 features.** A13's Metal support enables tile-based deferred rendering optimisations, indirect command buffers, and GPU-driven rendering paths if needed for future visual enhancements.
- **4 GB RAM baseline.** iPhone 11 ships with 4 GB of RAM, providing ample headroom for our < 500 MB memory budget.
- **Simplified QA matrix.** A clear hardware floor reduces the number of devices that must be tested. The QA matrix focuses on A13 (baseline), A15 (mid-range), and A17 Pro / M-series (high-end).

### Negative

- **Excludes iPhone XR, XS, X, 8, and older.** This removes a portion of the potential market. However, these devices are 6-8+ years old as of 2026, and their user base is declining. The performance trade-off is not worth the engineering cost of optimising for A12 and below.
- **Level size cap on baseline.** The 256 x 16 tile limit constrains level width on A13 devices. The optional extended mode mitigates this for newer devices, but the core experience must be designed around the 256-tile width.
- **No iPad-specific optimisation in v1.** iPads with A13 or later are supported but the game will run in iPhone compatibility mode or with simple letterboxing. A dedicated iPad layout is deferred to a future release.
