# iOS Game Engine Selection Rubric

## Executive Summary
This document evaluates three primary engines for developing a retro side-scrolling mobile game targeting iOS. Selection criteria balance native performance, retro aesthetics fidelity, development velocity, and deployment complexity.

---

## Candidate Engines

1. **SpriteKit** (Apple's native framework)
2. **Unity** (industry standard cross-platform)
3. **Godot** (open-source, emerging iOS support)

---

## Evaluation Criteria

### 1. Native Performance (Weight: 25%)

#### SpriteKit
- **Score: 9/10**
- Native Metal rendering on iOS
- Optimized for Apple hardware
- Direct access to iOS APIs
- Minimal overhead on newer devices
- **Cons**: Limited on older devices (iPhone XR and prior may struggle with 120 fps)

#### Unity
- **Score: 7/10**
- Well-optimized IL2CPP backend
- Proven in thousands of iOS titles
- Some overhead compared to native
- Good profiling tools
- **Cons**: Larger build size (50-100 MB baseline), slightly higher startup time

#### Godot
- **Score: 6/10**
- Improving iOS support (v4.0+)
- GDScript is lightweight
- Less mature profiling on iOS
- **Cons**: Smaller ecosystem for mobile optimization, fewer device-specific optimizations

**Winner: SpriteKit**

---

### 2. retro Aesthetic Fidelity (Weight: 20%)

#### SpriteKit
- **Score: 8/10**
- Pixel-perfect rendering with proper scaling
- Full control over sprite rendering
- Easy to maintain clean pixels without anti-aliasing
- **Cons**: Manual texture atlas management required

#### Unity
- **Score: 8/10**
- Sprite mode with excellent control
- Pixel Perfect Camera component
- Built-in 2D physics
- **Cons**: Requires careful setup to avoid upscaling artifacts

#### Godot
- **Score: 8/10**
- Canvas item rendering is lightweight
- Good pixel-perfect support
- 2D focus makes this natural
- **Cons**: Texture atlas tools less mature

**Winner: Tie (all handle retro well with proper setup)**

---

### 3. Development Velocity (Weight: 20%)

#### SpriteKit
- **Score: 7/10**
- Steep learning curve for non-Apple developers
- Excellent Xcode integration
- Smaller community resources
- **Cons**: Limited third-party tools and asset stores

#### Unity
- **Score: 9/10**
- Massive asset store and community
- Well-documented pipeline
- Visual scene editor
- Extensive tutorials and plugins
- **Cons**: Overkill features for simple 2D game

#### Godot
- **Score: 8/10**
- Clean, intuitive editor
- Strong 2D workflow
- Easier to learn than Unity
- **Cons**: Smaller community means fewer pre-made solutions

**Winner: Unity**

---

### 4. iOS Deployment & App Store (Weight: 15%)

#### SpriteKit
- **Score: 10/10**
- Native Xcode integration
- Direct StoreKit support
- No intermediary build process
- Fastest TestFlight turnaround
- **Cons**: Requires macOS for development

#### Unity
- **Score: 8/10**
- Mature iOS build pipeline
- Unity Cloud Build available
- StoreKit integration via plugins
- **Cons**: Xcode project generation adds complexity

#### Godot
- **Score: 6/10**
- iOS export less mature
- Requires manual Xcode configuration
- **Cons**: Smaller track record on App Store submissions

**Winner: SpriteKit**

---

### 5. Generative Level System Support (Weight: 10%)

#### SpriteKit
- **Score: 7/10**
- Direct control over scene generation
- No abstraction layers
- Deterministic rendering guaranteed
- **Cons**: Manual implementation of all systems

#### Unity
- **Score: 8/10**
- ECS (Entity Component System) ideal for procedural generation
- Burst compiler supports deterministic math
- Extensive procedural generation examples
- **Cons**: More overhead

#### Godot
- **Score: 8/10**
- Node system perfect for level instantiation
- GDScript supports deterministic algorithms
- Scene serialization is straightforward
- **Cons**: Less mature procedural generation ecosystem

**Winner: Tie (all capable, different approaches)**

---

### 6. Documentation & Community (Weight: 10%)

#### SpriteKit
- **Score: 6/10**
- Official Apple documentation
- Smaller community
- Fewer third-party tutorials
- **Cons**: Harder to find solutions for edge cases

#### Unity
- **Score**: 10/10
- Massive documentation and tutorials
- Thousands of open-source projects
- Active forums and Discord communities
- **Cons**: Noise signal-to-noise ratio

#### Godot
- **Score: 7/10**
- Growing documentation
- Active community
- More approachable than Unity
- **Cons**: Less mobile-specific content

**Winner: Unity**

---

## Scoring Summary

| Engine | Performance | Aesthetics | Velocity | Deployment | Generative | Community | **Total** |
|--------|-------------|-----------|----------|-----------|-----------|-----------|---------|
| SpriteKit | 9 | 8 | 7 | 10 | 7 | 6 | **7.9/10** |
| Unity | 7 | 8 | 9 | 8 | 8 | 10 | **8.4/10** |
| Godot | 6 | 8 | 8 | 6 | 8 | 7 | **7.2/10** |

---

## Recommendation

### **Primary Choice: Unity**
- Highest overall score (8.4/10)
- Best for team onboarding and velocity
- Proven track record in mobile gaming
- Excellent tooling for 2D/sprite-based games
- Strong community support for procedural generation

### **Alternative: SpriteKit**
- If maximum native performance is critical
- If team is already experienced with Swift/Xcode
- If minimizing binary size is essential

---

## Implementation Plan

### Phase 1: Unity Setup (Week 1-2)
- Install Unity Editor (2022 LTS recommended)
- Configure iOS build settings
- Setup pixel-perfect camera
- Create sprite pipeline
- Implement basic physics

### Phase 2: Prototype (Week 3-4)
- Core movement mechanics (walk, jump, fall)
- Basic enemy AI
- Simple level with tiles and sprites
- Touch controls
- Performance baseline testing

### Phase 3: Generative System Integration (Week 5-6)
- Implement deterministic PRNG
- Build level generation pipeline
- Create validation system
- Test deterministic reconstruction

---

## Decision Lock
**Selected Engine: Unity 2022 LTS**
**Reasoning**: Optimal balance of performance, development velocity, community resources, and team scalability.

## Accepted Tradeoffs

Choosing Unity over SpriteKit means accepting these costs:

1. **Larger binary size.** Unity baseline is 50-100 MB before game assets. A SpriteKit app would be 5-15 MB. This affects download conversion on cellular networks. Mitigation: aggressive asset compression, on-demand resources for non-critical content.

2. **Xcode project generation step.** Unity exports an intermediate Xcode project rather than building directly. This adds build pipeline complexity and can cause issues with signing, entitlements, or native plugin integration. Mitigation: document and automate the export-to-Xcode workflow early; test the full pipeline in Week 1.

3. **Non-native performance overhead.** Unity's IL2CPP backend adds a small overhead vs. native Metal/SpriteKit rendering. For a 2D pixel art game this is negligible in practice, but it means the 120 fps target on iPhone 14+ requires more careful draw call management than it would in SpriteKit.

4. **Dependency on Unity's release cycle.** Unity LTS versions receive patches but no new features. If an iOS update introduces a breaking change, the fix depends on Unity's patch timeline. Mitigation: use Unity 2022 LTS (long-term support through 2025+), monitor Unity's iOS compatibility notes each Xcode release.

These tradeoffs are acceptable given Unity's advantages in development velocity, community resources, and the team's ability to iterate faster with Unity's editor and asset pipeline.
