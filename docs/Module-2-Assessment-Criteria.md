# Module 2: Mobile Game Development (iOS) - Assessment Criteria

## Module Overview
This module develops expertise in iOS-specific game development, including frameworks, touch controls, performance optimization, and App Store requirements. Learners must demonstrate ability to develop production-quality mobile games.

---

## Learning Objectives

By completion, learners will be able to:
1. Select and justify appropriate iOS framework choices
2. Design responsive touch control schemes for complex gameplay
3. Optimize game performance for varied iPhone hardware
4. Navigate App Store submission requirements and best practices
5. Implement iOS-specific features (haptics, saves, notifications)

---

## Assessment 2.1: iOS Framework Comparison & Selection

### Objective
Evaluate Unity vs. SpriteKit vs. Godot and justify engine selection for the project.

### Requirements
**Analysis Scope:**
- [ ] Performance characteristics per framework
- [ ] Learning curve and team skill requirements
- [ ] Asset pipeline maturity
- [ ] iOS-specific optimization tools
- [ ] Community support and resources
- [ ] Long-term maintenance and updates
- [ ] Deployment complexity and build times

**Deliverables:**

**Part 1: Comparison Matrix (2-3 pages)**
```
| Criterion | SpriteKit | Unity | Godot |
|-----------|-----------|-------|-------|
| Native Performance | 9/10 | 7/10 | 6/10 |
| 2D Capabilities | 8/10 | 8/10 | 8/10 |
| iOS Integration | 10/10 | 8/10 | 6/10 |
| Asset Pipeline | 7/10 | 9/10 | 8/10 |
| Community Resources | 6/10 | 10/10 | 7/10 |
| ... | ... | ... | ... |
```

**Part 2: Recommendation Report (3-5 pages)**
- Summary of analysis
- Recommended framework with justification
- Risk mitigation for chosen framework
- Contingency plan if primary choice proves unsuitable

### Success Criteria
- ✅ Comprehensive comparison of all three frameworks
- ✅ Evaluation includes performance, UX, and team factors
- ✅ Recommendation is well-justified and defensible
- ✅ Risk analysis is realistic and thorough
- ✅ Clear decision rubric showing how recommendation was reached

### Evaluation Rubric
| Criteria | Excellent (5) | Good (4) | Acceptable (3) | Needs Work (2) |
|----------|--------------|---------|----------------|----------------|
| Analysis Depth | Comprehensive | Very thorough | Adequate | Superficial |
| Technical Accuracy | All facts correct | 1-2 minor errors | Some inaccuracies | Multiple errors |
| Justification Quality | Compelling logic | Strong reasoning | Adequate reasoning | Weak reasoning |
| Risk Assessment | Thorough | Good coverage | Basic analysis | Minimal |
| Documentation | Excellent | Clear | Adequate | Poor |

**Pass Threshold**: Average >= 4.0/5.0

---

## Assessment 2.2: Touch Control Scheme Design

### Objective
Design three distinct touch control schemes for side-scroller gameplay and demonstrate usability.

### Challenge
**Scheme 1: Button-Based (Traditional)**
- Layout virtual buttons on-screen
- Optimize for different hand sizes and grip styles
- Support single-hand and two-hand play
- Minimize screen obstruction

**Scheme 2: Gesture-Based (Advanced)**
- Use swipe, tap, and hold gestures
- Minimize buttons (show only when necessary)
- Ensure gesture responsiveness
- Handle accidental inputs gracefully

**Scheme 3: Hybrid (Optimized)**
- Combine best aspects of Schemes 1 & 2
- Context-aware button visibility
- Adaptive layout based on situation
- Accessibility considerations

### Requirements per Scheme

**Technical Specifications:**
- [ ] Button placement documentation
- [ ] Hit detection areas (actual vs. visual)
- [ ] Response latency targets (< 50 ms)
- [ ] Accessibility compliance (VoiceOver support)
- [ ] Portrait and landscape support
- [ ] Safe area (notch) consideration

**User Experience:**
- [ ] Ergonomic button sizing (30x30pt minimum)
- [ ] Button opacity/visibility (readable with content)
- [ ] Haptic feedback integration
- [ ] Failure mode handling (missed taps, double-taps)

### Deliverables
```
For each control scheme:
  - control_scheme_[1,2,3].pdf     (visual layout with specs)
  - control_mapping.json            (button-to-action mapping)
  - usability_guide.md              (how to use, accessibility notes)
  
Additional:
  - comparison_analysis.md          (strengths/weaknesses of each)
  - recommendation.md               (which scheme for main game)
```

### Success Criteria
- ✅ All three schemes are fully designed and documented
- ✅ Schemes are technically feasible in Unity
- ✅ Each scheme addresses accessibility requirements
- ✅ Hit detection areas are clear and realistic
- ✅ Responsive latency targets are achievable
- ✅ Comparison clearly identifies best scheme

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Design Completeness | /25 | All schemes fully specified |
| Ergonomic Quality | /25 | User-centered, comfortable design |
| Technical Accuracy | /25 | Feasible and well-documented |
| Accessibility | /15 | Supports various player abilities |
| Documentation | /10 | Clear and comprehensive |

**Pass Threshold**: >= 85/100

---

## Assessment 2.3: iOS Performance Optimization Challenge

### Objective
Optimize a complex 2D game scene to meet performance targets across device tiers.

### Challenge Scenario
Given a game scene with:
- 256x32 tilemap (8x8 pixel tiles)
- 50 animated sprites (enemies, player, particles)
- Parallax background (3 layers)
- 10 simultaneous VFX
- HUD with dynamic text

**Performance Targets:**
| Device | Target FPS | Max Draw Calls | Max Memory |
|--------|-----------|-----------------|-----------|
| iPhone 14 Pro | 120 fps | 50 | 100 MB |
| iPhone 13 | 60 fps | 40 | 80 MB |
| iPhone 11 | 60 fps | 30 | 60 MB |
| iPhone XR | 60 fps | 25 | 40 MB |

### Optimization Tasks

**Part 1: Analysis (15 minutes)**
- [ ] Profile current performance
- [ ] Identify bottlenecks (CPU, GPU, memory)
- [ ] Document baseline metrics

**Part 2: Optimization (60 minutes)**
Implement optimizations:
- [ ] Texture atlas consolidation
- [ ] Draw call reduction
- [ ] Object pooling for VFX
- [ ] LOD (level of detail) systems
- [ ] Culling (off-screen entity removal)
- [ ] Memory optimization

**Part 3: Validation (15 minutes)**
- [ ] Measure post-optimization performance
- [ ] Verify targets met on all devices
- [ ] Identify remaining bottlenecks

### Success Criteria
- ✅ Achieves target FPS on all device tiers
- ✅ Draw calls below targets
- ✅ Memory usage within limits
- ✅ Visual quality maintained (no obvious degradation)
- ✅ Optimization process documented

### Deliverables
```
optimization_report.md           (analysis, optimization steps, results)
performance_metrics.json         (before/after measurements)
screenshot_comparison.png        (visual quality check)
code_changes.md                  (optimization implementation notes)
```

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Baseline Analysis | /15 | Identifies key bottlenecks |
| Optimization Effectiveness | /35 | Meets all targets |
| Visual Quality | /20 | Maintains quality during optimization |
| Technical Approach | /20 | Sound optimization techniques |
| Documentation | /10 | Clear explanation of changes |

**Pass Threshold**: >= 85/100

---

## Assessment 2.4: App Store Submission Readiness

### Objective
Prepare a complete App Store submission package including all required metadata, legal compliance, and technical requirements.

### Requirements Checklist

**App Store Requirements:**
- [ ] Privacy Policy (written and published)
- [ ] Terms of Service
- [ ] App description (250-1000 characters)
- [ ] Localization (minimum English)
- [ ] Keywords (5 relevant terms)
- [ ] Support URL and contact information
- [ ] Subscription information (if applicable)
- [ ] Age rating questionnaire completion

**Technical Requirements:**
- [ ] Build signed and provisioned correctly
- [ ] All required app icons (1024x1024, various sizes)
- [ ] App preview video (15-30 seconds)
- [ ] Screenshots for all screen sizes
- [ ] Release notes (current version)
- [ ] Privacy manifest (iOS 17+)
- [ ] Compliance with App Store Review Guidelines

**Legal/Compliance:**
- [ ] COPPA compliance (children's data protection)
- [ ] GDPR compliance (if serving EU users)
- [ ] Accessibility compliance (WCAG 2.1 Level AA)
- [ ] Content rating verification
- [ ] Third-party SDK compliance

### Deliverables
```
app_store_submission_package/
  ├── privacy_policy.md
  ├── terms_of_service.md
  ├── app_description.txt
  ├── keywords.txt
  ├── screenshots/
  │   ├── iphone_6.5_inch_1.png
  │   ├── iphone_5.5_inch_1.png
  │   └── ...
  ├── app_icons/
  │   ├── 1024x1024.png
  │   ├── app_icon_120x120.png
  │   └── ...
  ├── release_notes.txt
  ├── privacy_manifest.json
  ├── compliance_checklist.md
  └── app_store_review_notes.txt
```

### Success Criteria
- ✅ All legal documents complete and compliant
- ✅ All technical assets correct size and format
- ✅ Privacy and accessibility compliance verified
- ✅ App Store Review Guidelines checklist complete
- ✅ No red flags in compliance review

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Documentation Completeness | /30 | All legal docs present |
| Technical Asset Quality | /25 | Correct sizes, professional |
| Compliance Accuracy | /30 | Legal, privacy, accessibility |
| Review Readiness | /15 | Likely to pass review |

**Pass Threshold**: >= 85/100

---

## Assessment 2.5: iOS Feature Integration Challenge

### Objective
Implement and document three critical iOS-specific features.

### Challenge: Integrate Each Feature

**Feature 1: iCloud Cloud Save**
- [ ] Enable CloudKit support
- [ ] Implement level progress save/restore
- [ ] Handle save conflicts
- [ ] Test on multiple devices

**Feature 2: Haptic Feedback Integration**
- [ ] Map 10 game events to haptic patterns
- [ ] Use appropriate haptic types (tap, light, medium, heavy)
- [ ] Handle devices without haptic support
- [ ] Allow user to disable haptics

**Feature 3: Game Center Integration**
- [ ] Implement achievement system (minimum 10 achievements)
- [ ] Add leaderboard for high scores
- [ ] Enable multiplayer invites (if applicable)
- [ ] Test leaderboard submission

### Requirements per Feature

**Code Quality:**
- [ ] Clean, documented code
- [ ] Error handling and edge cases
- [ ] Memory management (no leaks)
- [ ] Thread safety (main thread usage)

**User Experience:**
- [ ] Features enhance gameplay
- [ ] Non-intrusive implementation
- [ ] Graceful degradation on unsupported devices
- [ ] Clear user communication

### Deliverables
```
For each feature:
  - implementation_guide.md         (step-by-step instructions)
  - source_code.swift              (complete, documented code)
  - testing_results.md             (verification on real devices)
  
Summary:
  - features_status_report.md      (overall integration status)
```

### Success Criteria
- ✅ All three features fully implemented
- ✅ Features tested on real iOS devices
- ✅ Code is production-quality
- ✅ User experience is seamless
- ✅ All edge cases handled

### Evaluation Rubric
| Criteria | Score | Notes |
|----------|-------|-------|
| Implementation Completeness | /30 | All features functional |
| Code Quality | /25 | Professional, documented code |
| User Experience | /25 | Seamless integration |
| Testing Thoroughness | /15 | Real device validation |
| Documentation | /5 | Clear implementation guide |

**Pass Threshold**: >= 85/100

---

## Module 2 Final Assessment: Capstone Project

### Objective
Build a production-ready mobile game prototype demonstrating mastery of iOS game development.

### Requirements

**Game Scope:**
- Minimum 30-60 seconds of engaging gameplay
- 2+ difficulty levels
- Responsive touch controls
- Optimized performance (target FPS met on iPhone 11+)
- Clean, professional UI
- Proper error handling and edge cases

**Technical Implementation:**
- [ ] Selected engine fully integrated
- [ ] Touch controls working smoothly
- [ ] Performance targets met
- [ ] Memory profiled and optimized
- [ ] iOS APIs integrated (haptics, cloud save, Game Center)

**App Store Readiness:**
- [ ] Complete submission package
- [ ] Privacy policy and legal compliance
- [ ] App Store Review Guidelines compliance
- [ ] Accessibility features implemented
- [ ] Ready for TestFlight distribution

**Documentation:**
- [ ] Design document
- [ ] Technical architecture overview
- [ ] Performance optimization report
- [ ] App Store submission checklist
- [ ] Post-launch support plan

### Success Criteria
- Performance targets met on minimum device (iPhone 11)
- Game is fun and engaging (internal playtest feedback)
- Code is professional and maintainable
- App Store submission package complete
- All iOS integration features working

### Evaluation Process
1. **Self-assessment** by learner
2. **Technical code review** by iOS engineer
3. **Playability test** by QA team
4. **App Store compliance review**
5. **Final approval** for Module completion

**Pass Threshold**: Approved by iOS engineer + QA team

---

## Module Completion Checklist

- [ ] Assessment 2.1: Engine Selection (PASS)
- [ ] Assessment 2.2: Touch Control Design (PASS)
- [ ] Assessment 2.3: Performance Optimization (PASS)
- [ ] Assessment 2.4: App Store Submission (PASS)
- [ ] Assessment 2.5: iOS Feature Integration (PASS)
- [ ] Capstone Project (APPROVED)
- [ ] Module 2 Knowledge Check (80%+ score)

**Module Status**: Ready to advance to Module 3

---

## Resources & References

### Essential Documentation
- [Apple iOS Development Guide](https://developer.apple.com/ios/)
- [App Store Connect Help](https://help.apple.com/app-store-connect/)
- [App Store Review Guidelines](https://developer.apple.com/app-store/review/guidelines/)
- [Privacy & Security - Apple Developer](https://developer.apple.com/privacy/)

### Framework Documentation
- [Unity iOS Build Settings](https://docs.unity3d.com/Manual/iphone-GettingStarted.html)
- [SpriteKit Programming Guide](https://developer.apple.com/library/archive/documentation/GraphicsAnimation/Conceptual/SpriteKit_Cookbook/Introduction/Introduction.html)
- [Godot iOS Export Guide](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_ios.html)

### Performance Optimization
- [Metal Programming Guide](https://developer.apple.com/metal/Metal-Shading-Language-Specification.pdf)
- [Instruments Tutorial](https://developer.apple.com/videos/all-videos/?name=instruments)
- [Game Performance Analysis Tools](https://developer.apple.com/videos/all-videos/?name=performance)

### Tools
- Xcode 14+
- iOS Simulator
- Instruments
- TestFlight
