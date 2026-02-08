# Documentation Index

**Last Updated**: 2026-02-07 | **Version**: v1.1.0 build 028

---

## Start Here

| Document | Purpose |
|----------|---------|
| [README.md](README.md) | Project overview, tech stack, gameplay pillars |
| [BUILD-LOG.md](BUILD-LOG.md) | WebGL deployment log with version, date, and change notes |
| [QC-CHECKLIST.md](QC-CHECKLIST.md) | Systematic QC checklist for all screens, buttons, systems, and features |
| [ROADMAP.md](ROADMAP.md) | Development roadmap with version history |

---

## Planning & Architecture

| Document | Purpose |
|----------|---------|
| [docs/PROJECT-STRUCTURE.md](docs/PROJECT-STRUCTURE.md) | Directory layout, naming conventions |
| [docs/architecture-decisions/](docs/architecture-decisions/) | Architecture Decision Records (ADR-001 through ADR-006) |

---

## Generative Level System

| Document | Purpose |
|----------|---------|
| [docs/Level-Generation-Technical-Spec.md](docs/Level-Generation-Technical-Spec.md) | Architecture, PRNG, pipeline, schema, performance targets |
| [docs/Level-Generation-Research-Guide.md](docs/Level-Generation-Research-Guide.md) | Algorithm comparison, code examples, biome config |
| [docs/Validation-QA-Suite.md](docs/Validation-QA-Suite.md) | 7 test categories, 22+ test cases, failure protocol |

---

## Game Design Review

| Document | Purpose |
|----------|---------|
| [docs/EXPERT-REVIEW-v1.0.2.md](docs/EXPERT-REVIEW-v1.0.2.md) | Expert design review: 12 findings, competency scores, improvement roadmap |

---

## Quality Assurance

| Document | Purpose |
|----------|---------|
| [QC-CHECKLIST.md](QC-CHECKLIST.md) | Full QC checklist: 20 sections, 200+ items |
| [docs/Validation-QA-Suite.md](docs/Validation-QA-Suite.md) | Level generation validation suite |
| Play Mode Smoke Tests | 12 automated smoke tests in `Assets/Scripts/Tests/PlayMode/SmokeTests.cs` |

---

## Browser Build & Deployment

The game is playable at **[https://adamhardin.github.io/EpochBreaker/](https://adamhardin.github.io/EpochBreaker/)**

| File | Purpose |
|------|---------|
| [.github/workflows/deploy-webgl.yml](.github/workflows/deploy-webgl.yml) | GitHub Actions workflow for gh-pages deployment |
| [EpochBreaker/Assets/Scripts/Editor/WebGLBuildScript.cs](EpochBreaker/Assets/Scripts/Editor/WebGLBuildScript.cs) | Custom build script for headless WebGL builds |
| [EpochBreaker/Assets/WebGLTemplates/EpochBreaker/index.html](EpochBreaker/Assets/WebGLTemplates/EpochBreaker/index.html) | Custom HTML template (landscape 16:9, dark theme) |
| [scripts/test-and-build.sh](scripts/test-and-build.sh) | Local test + build script |
| [deploy-webgl.sh](deploy-webgl.sh) | WebGL deployment helper |

---

## Local-Only Folders (not on GitHub)

The following folders are kept locally but excluded from the GitHub repository via `.gitignore`:

| Folder | Purpose |
|--------|---------|
| `agents/` | Python expert agent server for local development assistance |
| `training/` | Design spec reference docs (modules 3-6: progression, combat, UX, art/audio) |
| `docs/archive/` | Archived historical docs (expert reviews, old plans, assessment criteria) |
