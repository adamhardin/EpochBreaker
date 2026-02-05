# Project Directory Structure & File Guide

## Overview

This document describes the complete directory structure for the retro Mobile Game project and provides guidance on file organization throughout development.

---

## Current Structure

```
Retro Mobile Game/
├── README.md                          ← START HERE (project overview)
├── .gitignore
├── .github/
│   └── workflows/
│       └── ci.yml                    (GitHub Actions CI/CD pipeline)
│
└── docs/                             ← ALL DOCUMENTATION
    ├── Training-Plan.md              (Expert training curriculum)
    ├── Development-Roadmap.md        (16-week timeline, milestones)
    ├── Engine-Selection-Rubric.md    (Engine evaluation & selection)
    │
    ├── Level-Generation-Technical-Spec.md      ⭐ CRITICAL
    │                                  (Complete level gen architecture)
    ├── Level-Generation-Research-Guide.md      ⭐ CRITICAL
    │                                  (Research, algorithms, code examples)
    ├── Validation-QA-Suite.md        ⭐ CRITICAL
    │                                  (Testing framework for gen system)
    │
    ├── Module-1-Assessment-Criteria.md
    │   (retro Design Fundamentals)
    ├── Module-2-Assessment-Criteria.md
    │   (Mobile Game Development)
    ├── Module-3-Assessment-Criteria.md (COMING SOON)
    │   (Game Design & Gamification)
    ├── Module-4-Assessment-Criteria.md (COMING SOON)
    │   (Side-Scrolling Mechanics)
    ├── Module-5-Assessment-Criteria.md (COMING SOON)
    │   (Mobile UX & Accessibility)
    ├── Module-6-Assessment-Criteria.md (COMING SOON)
    │   (retro Audio & Aesthetics)
    │
    ├── LIVING DOCUMENTS (Updated weekly)
    ├── Progress-Log.md               (Weekly status updates)
    ├── Known-Issues-Tracker.md       (Bug database)
    ├── Performance-Benchmarks.md     (Performance metrics)
    ├── Architecture-Decisions.md     (ADR log)
    ├── Design-Changes-Log.md         (Design iterations)
    │
    ├── IMPLEMENTATION GUIDES (Created during development)
    ├── Architecture-Overview.md      (System design)
    ├── Implementation-Guide.md       (Step-by-step coding)
    ├── Performance-Optimization-Guide.md
    ├── App-Store-Launch-Checklist.md
    ├── Testing-Protocol.md           (QA procedures)
    │
    ├── REFERENCE & TEMPLATES
    ├── design_template.md            (Level design document template)
    ├── code_review_template.md       (Code review checklist)
    ├── test_report_template.md       (QA test report template)
    │
    ├── archive/                      (Obsolete documents)
    │   └── [old versions, previous iterations]
    │
    └── images/                       (Screenshots, diagrams, reference)
        ├── ui-mockups/
        ├── architecture-diagrams/
        └── 16bit-references/


🎮 SOURCE CODE (Created Week 5+)
├── unity_project/
│   ├── Assets/
│   │   ├── Scripts/
│   │   │   ├── Generative/          (Level generation system)
│   │   │   │   ├── LevelGenerator.cs
│   │   │   │   ├── XORShift64.cs    (PRNG)
│   │   │   │   ├── LevelValidator.cs
│   │   │   │   └── LevelSerializer.cs
│   │   │   │
│   │   │   ├── Gameplay/            (Core game mechanics)
│   │   │   │   ├── PlayerController.cs
│   │   │   │   ├── EnemyAI.cs
│   │   │   │   ├── PhysicsEngine.cs
│   │   │   │   └── CollisionDetection.cs
│   │   │   │
│   │   │   ├── UI/                  (User interface)
│   │   │   │   ├── MenuManager.cs
│   │   │   │   ├── HUD.cs
│   │   │   │   └── TouchInput.cs
│   │   │   │
│   │   │   ├── Audio/               (Sound design)
│   │   │   │   ├── AudioManager.cs
│   │   │   │   └── SoundEffects.cs
│   │   │   │
│   │   │   ├── iOS/                 (iOS-specific)
│   │   │   │   ├── GameKitManager.cs
│   │   │   │   ├── CloudSaveManager.cs
│   │   │   │   └── HapticManager.cs
│   │   │   │
│   │   │   └── Tests/               (Unit & integration tests)
│   │   │       ├── GenerationTests.cs
│   │   │       ├── DeterminismTests.cs
│   │   │       ├── PerformanceTests.cs
│   │   │       └── ValidationTests.cs
│   │   │
│   │   ├── Prefabs/                 (Game object templates)
│   │   │   ├── Player.prefab
│   │   │   ├── Enemy.prefab
│   │   │   ├── Level.prefab
│   │   │   └── UI/
│   │   │
│   │   ├── Scenes/                  (Game scenes)
│   │   │   ├── MainMenu.unity
│   │   │   ├── GameLevel.unity
│   │   │   ├── GameOver.unity
│   │   │   └── Settings.unity
│   │   │
│   │   ├── Sprites/                 (Pixel art)
│   │   │   ├── Player/
│   │   │   │   ├── player_idle.png
│   │   │   │   ├── player_walk.png
│   │   │   │   ├── player_jump.png
│   │   │   │   └── ...
│   │   │   │
│   │   │   ├── Enemies/
│   │   │   │   ├── slime.png
│   │   │   │   ├── goblin.png
│   │   │   │   ├── skeleton.png
│   │   │   │   └── boss.png
│   │   │   │
│   │   │   ├── Tilesets/            (Biome-specific)
│   │   │   │   ├── forest.png
│   │   │   │   ├── cavern.png
│   │   │   │   ├── sky.png
│   │   │   │   └── volcanic.png
│   │   │   │
│   │   │   └── UI/
│   │   │       ├── buttons/
│   │   │       ├── icons/
│   │   │       └── backgrounds/
│   │   │
│   │   ├── Audio/                   (Sound files)
│   │   │   ├── Music/
│   │   │   │   ├── level_intro.ogg
│   │   │   │   ├── level_loop.ogg
│   │   │   │   ├── boss_theme.ogg
│   │   │   │   └── ...
│   │   │   │
│   │   │   └── SFX/
│   │   │       ├── jump.wav
│   │   │       ├── land.wav
│   │   │       ├── enemy_hit.wav
│   │   │       ├── health_pickup.wav
│   │   │       └── ...
│   │   │
│   │   ├── Resources/               (Unity resources)
│   │   │   ├── LevelConfigs/
│   │   │   │   ├── difficulty_1.json
│   │   │   │   ├── difficulty_2.json
│   │   │   │   └── biomes.json
│   │   │   │
│   │   │   └── Palettes/
│   │   │       ├── forest.palette
│   │   │       ├── cavern.palette
│   │   │       └── ...
│   │   │
│   │   └── Materials/               (Shaders, materials)
│   │       ├── PixelArt.shader
│   │       └── ParallaxScroll.shader
│   │
│   ├── ProjectSettings/
│   ├── Packages/
│   ├── Library/
│   └── Build/                       (iOS builds)
│       ├── Development/
│       └── Release/
│
├── xcode_project/
│   └── (Auto-generated by Unity iOS build)
│
├── tests/                           (QA test data & procedures)
│   ├── determinism_tests.md
│   ├── performance_tests.md
│   ├── gameplay_tests.md
│   ├── test_devices.md              (Device matrix)
│   └── test_results/                (Ongoing results)
│
├── marketing/                       (App Store materials)
│   ├── screenshots/                 (Promotional images)
│   ├── video/                       (Preview video)
│   ├── descriptions.txt             (App store copy)
│   └── privacy_policy.md
│
└── archive/                         (Previous versions, prototypes)
    └── v0.1/
        └── [early prototypes]
```

---

## File Guidelines

### Documentation Files (.md)

**Naming Convention**: `[Title]-[Descriptor].md`

**Examples**:
- `Training-Plan.md`
- `Level-Generation-Technical-Spec.md`
- `Module-1-Assessment-Criteria.md`

**Structure**:
```markdown
# Title

## Overview (1 paragraph)

## Section 1
...

## Section 2
...

### Subsection 2.1
...

## References
- Link 1
- Link 2

---
Last Updated: YYYY-MM-DD
Status: [Draft|Active|Complete|Archived]
Version: X.Y
```

**Status Indicators**:
- `Draft`: Work in progress, not for public use
- `Active`: Current, maintained, reference material
- `Complete`: Finished, no major changes expected
- `Archived`: Superseded by newer version

### Code Files (.cs)

**Naming Convention**: `PascalCase.cs` matching class name

**Structure**:
```csharp
using UnityEngine;
using System;

/// <summary>
/// Brief description of what this class does.
/// </summary>
public class MyClassName : MonoBehaviour
{
    [SerializeField] private int exampleField;
    
    public void PublicMethod()
    {
        // Implementation
    }
    
    private void PrivateMethod()
    {
        // Implementation
    }
}
```

**Documentation**:
- XML comments for public methods
- Inline comments for complex logic
- No commented-out code (use Git history)

### Test Files (.cs)

**Naming Convention**: `[TargetClass]Tests.cs`

**Examples**:
- `LevelGeneratorTests.cs`
- `DeterminismTests.cs`
- `PerformanceTests.cs`

**Structure**:
```csharp
using UnityEngine.TestTools;
using NUnit.Framework;

public class LevelGeneratorTests
{
    [Test]
    public void TestSpecificBehavior()
    {
        // Arrange
        var generator = new LevelGenerator();
        
        // Act
        var result = generator.GenerateLevel(seed: 12345);
        
        // Assert
        Assert.NotNull(result);
    }
}
```

### Asset Files

**Sprites**: 
- Name format: `[character]_[action]_[frame].png`
- Example: `player_walk_01.png`, `slime_idle_01.png`
- Keep palette-optimized (retro constraints)

**Audio**:
- Format: .ogg (compressed) or .wav (for editing)
- Name format: `[type]_[action].ogg`
- Example: `sfx_jump.ogg`, `music_boss_theme.ogg`

**Tilesets**:
- Name format: `[biome]_tileset.png`
- Size: 256x256 or 512x512
- Document palette in JSON

### Configuration Files

**JSON Configs**:
```json
{
  "version": "1.0",
  "biome_id": 0,
  "biome_name": "Forest",
  "enemy_types": ["slime", "goblin"],
  "difficulty_range": [1, 5]
}
```

**Always include**:
- Version number
- Descriptive comments
- Validation schema

---

## When to Create New Files

### DO Create New Files For:
- ✅ Major system component (Gameplay, UI, Audio)
- ✅ New assessment module
- ✅ Implementation guide
- ✅ Test suite (per system)
- ✅ Major design change log

### DON'T Create New Files For:
- ❌ Minor updates to existing docs (edit in place)
- ❌ Temporary notes (use Slack/GitHub discussions)
- ❌ Code that should be in existing module
- ❌ Duplicate information (link instead)

---

## File Lifecycle

### Stage 1: Creation
- Create in feature branch: `docs/[title]` or `feature/[name]`
- Use template if available
- Add header with status: `Draft`

### Stage 2: Development
- Update frequently
- Keep status as `Active` once shared with team
- Note changes in header

### Stage 3: Completion
- Status → `Complete`
- Final review by 2+ team members
- Merge to main branch

### Stage 4: Maintenance
- Update with project changes
- Keep last-updated date current
- Link from related documents

### Stage 5: Archival
- Move to `/docs/archive/`
- Keep version number in filename
- Document reason for archival
- Link from successor document

---

## Quick Reference: Important Files

| File | Purpose | Owner | Update Frequency |
|------|---------|-------|------------------|
| README.md | Project overview | PM | Weekly |
| Training-Plan.md | Curriculum | Design Lead | As needed |
| Development-Roadmap.md | Timeline & milestones | PM | Weekly |
| Level-Generation-Technical-Spec.md | Gen system specs | Tech Lead | As needed |
| Validation-QA-Suite.md | Testing framework | QA Lead | As needed |
| Progress-Log.md | Status updates | PM | Weekly |
| Known-Issues-Tracker.md | Bug database | QA Lead | Daily |
| Architecture-Decisions.md | Technical decisions | Tech Lead | As needed |

---

## Collaboration Best Practices

### Before Creating a New Document
1. Check if it already exists (search in `/docs/`)
2. Consider if it should be a section in existing document
3. Link from README if it's important

### Writing Documentation
1. Use clear, concise language
2. Include examples and code snippets
3. Link to related documents
4. Keep it DRY (Don't Repeat Yourself)
5. Add version control info

### Updating Documentation
1. Note the change in the document
2. Update "Last Updated" date
3. Increment version if major change
4. Link from changelog if applicable
5. Mention in team standup if significant

### Managing Old Versions
1. Don't delete old docs (Git history tracks them)
2. Archive superseded docs in `/archive/`
3. Add note: "This document has been superseded by [new-doc].md"
4. Keep for reference (learning from past decisions)

---

## Tools & Integration

### GitHub Integration
- Documents tracked in Git
- Pull requests for significant changes
- Branch protection on main
- Auto-generated docs index (optional)

### Search & Navigation
- Use `README.md` as hub
- Breadcrumb navigation in headers
- Cross-linking between related docs
- Consistent naming conventions

### Automation (Future)
- GitHub Actions to validate markdown
- Auto-generate table of contents
- PDF export for distribution
- Version tagging for releases

---

## Examples

### Creating a New Assessment Module

**File**: `Module-X-Assessment-Criteria.md`

**Template**:
```markdown
# Module X: [Title] - Assessment Criteria

## Module Overview
[1-2 paragraphs]

## Learning Objectives
- Objective 1
- Objective 2
- ...

## Assessment X.1: [Name]
### Objective
[Clear statement of what learner must demonstrate]

### Requirements
[Technical specs and design specs]

### Success Criteria
- ✅ Criterion 1
- ✅ Criterion 2

### Deliverables
[List of files/artifacts]

### Evaluation Rubric
[Scoring table]

**Pass Threshold**: [Score]

---

## Module Completion Checklist
- [ ] Assessment X.1 (PASS)
- [ ] Assessment X.2 (PASS)
- ...
```

### Creating a Living Document

**File**: `Known-Issues-Tracker.md`

**Template**:
```markdown
# Known Issues Tracker

**Last Updated**: 2026-02-04
**Total Open Issues**: 0
**Critical**: 0 | **High**: 0 | **Medium**: 0 | **Low**: 0

## Open Issues

### [Priority] [Component]: [Issue Title]
- **ID**: #123
- **Status**: Open / In Progress / Resolved
- **Reporter**: [Name]
- **Date**: YYYY-MM-DD
- **Description**: [Details]
- **Impact**: [User/system impact]
- **Assigned To**: [Name]

## Resolved Issues (Archive)
[Previous issues...]
```

---

## File Size Guidelines

| File Type | Max Size | Notes |
|-----------|----------|-------|
| Markdown docs | 50 KB | Break into sections if larger |
| Code files | 500 lines | Consider splitting if larger |
| Test files | 300 lines | One test class per file |
| Images | 500 KB | Compress sprites & diagrams |
| JSON configs | 100 KB | Validate syntax |

---

## Deprecation & Cleanup

### When to Deprecate
1. Document has been superseded
2. Information is no longer accurate
3. Better version exists
4. No longer relevant to project

### Deprecation Process
1. Add notice at top of document
2. Link to replacement/successor
3. Move to `/archive/` folder
4. Keep in Git history
5. Note deprecation date

**Example Notice**:
```
⚠️ **DEPRECATED**: This document has been superseded by 
[New-Document.md](New-Document.md). 
See that document for current information.

*Last used: 2026-02-04 | Archived: 2026-03-15*
```

---

## Version Control Conventions

### Commit Messages for Documentation
```
docs: [type] - [brief description]

Examples:
  docs: training - add Module 3 assessment criteria
  docs: spec - update level generation algorithm
  docs: roadmap - adjust timeline based on prototyping
  docs: archive - move deprecated architecture doc
```

### Pull Request Template
```markdown
## Changes
[What changed and why]

## Related Issues
Closes #[issue number]

## Validation
- [ ] All links working
- [ ] No spelling/grammar errors
- [ ] Consistent with style guide
- [ ] Code examples tested (if applicable)
```

---

## Support & Questions

**Question about file organization?** Ask in #documentation Slack channel.

**Found outdated information?** Create an issue with tag `documentation`.

**Have a better structure idea?** Propose in team meeting.

---

**Version**: 1.0  
**Last Updated**: 2026-02-04  
**Status**: Active
