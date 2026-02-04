# ADR-001: Engine Selection

**Date:** 2026-02-04
**Status:** Accepted

## Context

We are building a 16-bit aesthetic side-scrolling game targeting iOS. The engine must provide:

- A mature 2D rendering pipeline with tilemap support, sprite atlasing, and pixel-perfect camera control.
- Reliable iOS build tooling (code signing, App Store submission, TestFlight).
- A large ecosystem of documentation, assets, and community knowledge to maximise team velocity.
- Support for C# or a similarly productive language for gameplay and procedural generation logic.

Three engines were evaluated:

| Criteria | Unity 2022 LTS | SpriteKit | Godot 4.x |
|---|---|---|---|
| 2D pipeline maturity | Excellent (Tilemap, URP 2D Renderer, Sprite Atlas v2) | Good (native Apple framework) | Good (TileMap, CanvasItem renderer) |
| iOS tooling | Proven (IL2CPP, Xcode project export) | Native (no export step) | Functional but less battle-tested |
| Team familiarity | High | Low | Medium |
| Asset store / ecosystem | Extensive | Limited | Growing |
| Cross-platform option | Yes (Android, PC) | Apple-only | Yes |
| Long-term support | LTS commitment through 2025+ | Tied to Apple OS releases | Community-driven release cadence |

SpriteKit was ruled out because it locks the project to the Apple ecosystem and the team has minimal Swift/Objective-C experience. Godot 4.x is a strong contender but lacks the depth of iOS-specific battle-testing and the breadth of production references for mobile shipping titles that Unity offers.

## Decision

We will use **Unity 2022 LTS** (currently 2022.3.x) as the game engine.

All gameplay code will be written in C#. We will use the **Universal Render Pipeline (URP)** configured for 2D rendering with the 2D Renderer asset. Tilemaps will use Unity's built-in Tilemap package. The project will target the **IL2CPP** scripting backend for iOS builds.

## Consequences

### Positive

- The team can leverage existing Unity expertise, reducing ramp-up time to near zero.
- URP's 2D Renderer provides built-in 2D lighting, shadow casters, and sprite sorting that accelerate the 16-bit visual style.
- Unity's iOS build pipeline is well-documented and widely used in production mobile titles.
- The Asset Store provides fallback options for audio, UI, and tooling if custom solutions hit roadblocks.
- LTS guarantees critical bug fixes and security patches without feature churn.
- Preserves the option to ship on Android or other platforms later without an engine rewrite.

### Negative

- **Binary size overhead.** A minimal Unity IL2CPP build for iOS ships at roughly 50-100 MB, significantly larger than an equivalent SpriteKit app. We will mitigate this with aggressive stripping settings (`Strip Engine Code`, managed code stripping set to `High`).
- **Xcode project generation step.** Every iOS build requires Unity to generate an Xcode project, adding 1-3 minutes to the build cycle. CI will cache Library and build artifacts to reduce iteration time.
- **IL2CPP compilation overhead.** Full IL2CPP rebuilds are slow (5-10 minutes). Incremental builds and build caching on the CI server will be essential.
- **Runtime memory baseline.** Unity's runtime consumes approximately 40-60 MB of memory before any game content is loaded. This is acceptable given our minimum device target (ADR-005) but must be monitored.
- **License cost.** Unity Personal is free below the revenue threshold; Unity Pro will be required if annual revenue exceeds $200K. This is an acceptable cost at that stage.
