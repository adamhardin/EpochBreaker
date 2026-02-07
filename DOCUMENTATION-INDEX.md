# Documentation Index

**Last Updated**: 2026-02-07 | **Version**: v0.9.9 build 023

---

## Start Here

| Document | Purpose |
|----------|---------|
| [README.md](README.md) | Project overview, tech stack, targets, deployment guide |
| [GAME-DESIGN-REVIEW.md](GAME-DESIGN-REVIEW.md) | Comprehensive game design document for expert review (v0.5.0) |
| [BUILD-LOG.md](BUILD-LOG.md) | WebGL deployment log with version, date, and change notes |
| [QC-CHECKLIST.md](QC-CHECKLIST.md) | Systematic QC checklist for all screens, buttons, systems, and features |
| [ROADMAP.md](ROADMAP.md) | Development roadmap with version targets and sprint plans |

---

## Planning & Architecture

| Document | Purpose | Status |
|----------|---------|--------|
| [docs/PROJECT-STRUCTURE.md](docs/PROJECT-STRUCTURE.md) | Directory layout, naming conventions, dual-project sync | Active |
| [docs/Training-Plan.md](docs/Training-Plan.md) | 10-module expert training curriculum (6 phases) | Active |
| [docs/Training-Plan-Addendum.md](docs/Training-Plan-Addendum.md) | Creativity, fun, and expert feedback addendum | Active |
| [docs/Implementation-Plan-v0.4.0.md](docs/Implementation-Plan-v0.4.0.md) | Final implementation plan for v0.4.0 (8 sprints, approved) | Implemented |
| [docs/Implementation-Plan-v0.6.0.md](docs/Implementation-Plan-v0.6.0.md) | Implementation plan for v0.6.0 "Make It Fun" (9 sprints) | Awaiting Approval |

---

## Expert Review Process

| Document | Purpose | Status |
|----------|---------|--------|
| [docs/Expert-Review-Process.md](docs/Expert-Review-Process.md) | Consolidated workflow, roles, scope, quality standards, scheduling | Active |
| [docs/Expert-Review-Templates.md](docs/Expert-Review-Templates.md) | Intake form, sprint review, change proposal, decision log templates | Active |
| [docs/Expert-Review-Report-v0.3.0.md](docs/Expert-Review-Report-v0.3.0.md) | Expert review findings and recommendations (cycle 2) | Active |
| [docs/Expert-Review-Response-v0.3.0.md](docs/Expert-Review-Response-v0.3.0.md) | Implementation response with proposed approaches | Active |
| [docs/Expert-Review-Response-v0.3.0-Expert-Reply.md](docs/Expert-Review-Response-v0.3.0-Expert-Reply.md) | Expert reply approving/refining approaches | Active |
| [docs/Expert-Review-Report-v0.3.0-Memory-Addendum.md](docs/Expert-Review-Report-v0.3.0-Memory-Addendum.md) | MEMORY.md consistency audit | Active |
| [docs/Expert-Review-Change-Log.md](docs/Expert-Review-Change-Log.md) | Log of all review-driven changes | Active |
| [docs/Expert-Feedback-Log.md](docs/Expert-Feedback-Log.md) | Structured log of review entries with findings and owners | Active |
| [docs/Expert-Review-Report-v0.5.0.md](docs/Expert-Review-Report-v0.5.0.md) | Comprehensive deep review (code + design + fun + creativity) — cycle 3 | Active |

---

## Quality Assurance

| Document | Purpose | Status |
|----------|---------|--------|
| [QC-CHECKLIST.md](QC-CHECKLIST.md) | Full QC checklist: 20 sections, 200+ items covering every screen, button, system, and feature | Active |
| [docs/Validation-QA-Suite.md](docs/Validation-QA-Suite.md) | Level generation validation: 7 test categories, 22+ test cases | Active |

---

## Generative Level System

| Document | Purpose | Status |
|----------|---------|--------|
| [docs/Level-Generation-Technical-Spec.md](docs/Level-Generation-Technical-Spec.md) | Architecture, PRNG, pipeline, schema, performance targets | Active |
| [docs/Level-Generation-Research-Guide.md](docs/Level-Generation-Research-Guide.md) | Algorithm comparison, code examples, biome config | Active |
| [docs/Validation-QA-Suite.md](docs/Validation-QA-Suite.md) | 7 test categories, 22+ test cases, failure protocol | Active |

---

## Training Modules (Assessment Criteria)

| Document | Topic | Status |
|----------|-------|--------|
| [docs/Module-1-Assessment-Criteria.md](docs/Module-1-Assessment-Criteria.md) | Retro Design Fundamentals | Complete |
| [docs/Module-2-Assessment-Criteria.md](docs/Module-2-Assessment-Criteria.md) | Mobile Game Development (iOS) | Complete |
| [docs/Module-3-Assessment-Criteria.md](docs/Module-3-Assessment-Criteria.md) | Game Design & Gamification | Complete |
| [docs/Module-4-Assessment-Criteria.md](docs/Module-4-Assessment-Criteria.md) | Side-Scrolling Mechanics | Complete |
| [docs/Module-5-Assessment-Criteria.md](docs/Module-5-Assessment-Criteria.md) | Mobile UX & Accessibility | Complete |
| [docs/Module-6-Assessment-Criteria.md](docs/Module-6-Assessment-Criteria.md) | Retro Audio & Aesthetics | Complete |

---

## Reviewer Competency

| Document | Purpose | Status |
|----------|---------|--------|
| [docs/Reviewer-Competency-Track.md](docs/Reviewer-Competency-Track.md) | Competency tracking across 12 domains | Active |
| [docs/Reviewer-Competency-Rubrics.md](docs/Reviewer-Competency-Rubrics.md) | Scoring rubrics for competency evaluation | Active |
| [docs/Reviewer-Competency-Templates.md](docs/Reviewer-Competency-Templates.md) | Evidence artifact templates | Active |
| [docs/Reviewer-Competency-Results.md](docs/Reviewer-Competency-Results.md) | Assessment results record | Active |

---

## Browser Build & Deployment

The game is playable at **[https://adamhardin.github.io/EpochBreaker/](https://adamhardin.github.io/EpochBreaker/)**

| File | Purpose |
|------|---------|
| [.github/workflows/deploy-webgl.yml](.github/workflows/deploy-webgl.yml) | GitHub Actions workflow for gh-pages deployment |
| [EpochBreaker/Assets/Scripts/Editor/WebGLBuildScript.cs](EpochBreaker/Assets/Scripts/Editor/WebGLBuildScript.cs) | Custom build script for headless WebGL builds |
| [EpochBreaker/Assets/WebGLTemplates/EpochBreaker/index.html](EpochBreaker/Assets/WebGLTemplates/EpochBreaker/index.html) | Custom HTML template (landscape 16:9, dark theme) |

---

## Archived Documents

Historical documents moved to `docs/archive/`. Kept for reference only.

| Document | Reason |
|----------|--------|
| docs/archive/Expert-Review-Report-v0.2.0.md | Superseded by v0.3.0 review |
| docs/archive/Development-Roadmap-Original.md | Stale timeline; development tracked via implementation plans |
| docs/archive/Engine-Selection-Rubric.md | Decision locked (Unity 6000.3.6f1) |
| docs/archive/PROJECT-LAUNCH-CHECKLIST.md | Stale checklist; work tracked via sprint plans |
