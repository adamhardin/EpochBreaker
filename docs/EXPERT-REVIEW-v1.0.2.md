# Expert Game Design Review — v1.0.2 build 027

**Date**: 2026-02-07
**Reviewer**: AI Expert Agent (competency framework from `docs/archive/Reviewer-Competency-Rubrics.md` — archived locally, not in repo)
**Scope**: Full codebase review of all gameplay, UI, audio, level generation, and progression systems
**Purpose**: Identify improvements to maximize fun, clarity, and player retention

---

## Review Methodology

This review evaluated the game against the project's 12-competency framework (4 Critical, 4 High Value, 4 Valuable). Each system was analyzed by reading the source code directly:

- **PlayerController.cs** — movement, jump physics, wall mechanics, stomp
- **GameManager.cs** — game modes, state machine, scoring, lives, sessions
- **WeaponSystem.cs / WeaponData.cs** — weapon types, auto-fire, power curve
- **HazardSystem.cs** — hazard types, damage, destruction triggers
- **TutorialManager.cs** — onboarding flow, hint system
- **TitleScreenUI.cs** — menu structure, social features, accessibility
- **LevelGenerator.cs** — zone pacing, material distribution, enemy placement
- **Boss.cs** — phases, era variants, attack patterns, DPS cap
- **EnemyBase.cs** — archetypes, behaviors, difficulty scaling
- **LevelCompleteUI.cs** — score breakdown, button layout
- **PlaceholderAudio.cs** — waveform synthesis, loop fading, normalization
- **AudioManager.cs** — playback, volume, low-pass filtering

---

## 1) Findings

### [Critical] C1 — Campaign lives pool is punitively small

**System**: GameManager.cs — `GlobalLives = 2` for Campaign mode

2 global lives across 10 epochs. One bad level in epoch 0 leaves 1 life for 9 remaining epochs. Meanwhile Streak mode (the "endgame") gives 10 lives. The introductory mode is harder than the veteran mode. Most players will Game Over before seeing epoch 3, killing retention before they reach interesting boss variants, Chainer/Slower unlocks, and the destruction-heavy middle game.

**Decision**: Remove global lives from Campaign entirely. Deaths cost time/score but never cause Game Over. Campaign becomes the "see everything" mode; Streak remains the challenge mode.

---

### [Critical] C2 — Weapon cycling is never taught

**System**: TutorialManager.cs — no hint for Attack/cycle button; WeaponSystem.cs — auto-select only covers Bolt/Piercer/Slower

3 of 6 weapons (Chainer, Spreader, Cannon) are only accessible via manual cycling, but the tutorial never mentions the Attack/cycle button. The Cannon — the only weapon that breaks all materials, placed specifically before reinforced-heavy zones — will go unused by anyone who doesn't discover the button.

**Recommendation**: Add a weapon-cycling hint in Tutorial Level 2 when the player picks up their first weapon upgrade. Include the Quick Draw glow as visual bait.

---

### [Critical] C3 — Hazards are never introduced or taught

**System**: TutorialManager.cs — no hint for gas/fire/spike hazards; HazardSystem.cs — hazard frequency scales from 5% (epoch 0) to 30% (epoch 9)

The player's first encounter with a green cloud doing ticking damage will feel like a bug. By epoch 7-9, nearly 1 in 3 destructible tiles has a hazard.

**Recommendation**: Add a context-sensitive hint on first hazard encounter: "Careful! Breaking some blocks releases hazards."

---

### [High] H1 — Era 0 boss is boring

**System**: Boss.cs — Primitive variant (era 0) is charge-only, no projectiles, 2.6s cooldown

The first boss fight defines player expectations. Long idle patrols between charges feel like waiting, not fighting.

**Recommendation**: Give the Era 0 boss a simple telegraphed projectile in Phase 2 (slow speed, long cooldown). Or shorten patrol distance so charges feel more frequent.

---

### [High] H2 — 5-second mandatory wait on Level Complete

**System**: GameManager.cs — `LevelCompleteMinDelay = 5f`

On a 2-minute level, 5 seconds is 4% of the session spent unable to act. Mobile players have short patience thresholds. Especially painful on replayed easy levels.

**Recommendation**: Reduce to 2.5–3 seconds.

---

### [High] H3 — Boss Phase 3 pillar shelter has no tutorialization

**System**: Boss.cs — shelter mechanic activates in Phase 3 (epoch 3+) with no visual cue pointing to the pillar

Auto-fire targets the boss (not the pillar), so the player may stand there wondering why shots aren't connecting.

**Recommendation**: When the boss shelters, flash the pillar with a pulsing color and/or show a brief "Destroy the pillar!" context hint on first encounter.

---

### [Medium] M1 — Teleport Dash (era 4 boss) has no telegraph

**System**: Boss.cs — Renaissance variant teleports with no warning animation

Every other boss attack has a telegraph (0.6s charge pull-back, 0.3s enemy shoot tint). The teleport breaks the visual language.

**Recommendation**: Add a 0.3–0.5 second shimmer/fade animation before the teleport.

---

### [Medium] M2 — Combo resets on any damage including environmental

**System**: GameManager.cs — combo resets on player damage from any source including gas/fire hazards

In destruction-heavy zones where hazards are unavoidable (tight corridors), maintaining combos is nearly impossible. This undermines combat mastery scoring in exactly the zones designed for combat.

**Recommendation**: Only reset combo on enemy-sourced damage, OR add a 1-second grace period for environmental damage.

---

### [Medium] M3 — Flying enemies have no satisfying counterplay

**System**: EnemyBase.cs — Flying behavior: immune to stomp, vertical bobbing makes auto-aim less reliable, no alternate weakness

At 10% of placements with HP scaling in late epochs, they become increasingly annoying without a clear counter-play option.

**Recommendation**: Let stomp create a downward shockwave that hits flying enemies within 2 tiles. Or make flying enemies briefly pause when hit, giving auto-aim a better window.

---

### [Medium] M4 — Weapon legend on title screen shows only 3 of 6 types

**System**: TitleScreenUI.cs — legend displays Sword/Crossbow/Cannon tiers, omits Piercer/Chainer/Spreader/Slower

Combined with the untaught weapon cycling, most of the weapon system is invisible.

**Recommendation**: Expand the legend to show all 6 weapon types, or add a "Weapons" section in Settings.

---

### [Low] L1 — No way to replay the tutorial from the title screen

**System**: TutorialManager.cs — `ResetTutorial()` exists but no UI button exposes it

Players who skipped hints or want to re-learn controls have no path back.

**Recommendation**: Add a "Tutorial" option in Settings.

---

### [Low] L2 — README status section is outdated

**System**: README.md — says "v0.5.0 build 011" and "Current phase: Playable prototype"

The game is at v1.0.2 build 027 with full campaign, 3 game modes, cosmetics, daily/weekly challenges, and WebGL deployment.

---

## 2) Competency Results

| # | Competency | Score | Threshold | Status |
|---|-----------|-------|-----------|--------|
| C1 | 2D Platformer Game Feel & Tuning | 90 | 85 | **Passed** |
| C2 | Destructible Environment Design | 85 | 85 | **Passed** |
| C3 | Weapon System & Power Fantasy | 72 | 85 | Needs Remediation |
| C4 | Mobile UX & Touch Controls | 82 | 90 | Needs Remediation |
| H5 | Procedural Level Design & Replayability | 92 | 85 | **Passed** |
| H6 | Difficulty Curve & Boss Encounter Design | 73 | 85 | Needs Remediation |
| H7 | Scoring, Incentive & Achievement Design | 78 | 85 | Needs Remediation |
| H8 | Ethical Retention & Session Design | 93 | 90 | **Passed** |
| V9 | Retro / Pixel Art Aesthetic Direction | 83 | 80 | **Passed** |
| V10 | Chiptune & Synthesized Audio | 82 | 80 | **Passed** |
| V11 | Social/Competitive Mechanics | 88 | 80 | **Passed** |
| V12 | Cross-Platform Design (Mobile + Browser) | 85 | 80 | **Passed** |

**Result: 8/12 passed (67%). Pass rule requires 90%+ (11/12). 4 competencies need remediation.**

### Scoring Rationale

**C1 (90/100) — Platformer Feel**: Coyote time (5 frames), jump buffer (6 frames), asymmetric gravity (28/42), wall-slide, wall-jump with coyote, variable jump height, stomp with air-stall, squash/stretch, dust particles. Complete and well-tuned. Gap: no formal tuning sheet or benchmark comparison documented.

**C3 (72/100) — Weapon System**: 6-weapon roster with clear roles. Auto-fire is a smart mobile accessibility choice. However: 3/6 weapons undiscoverable without untaught cycling, auto-select rarely triggers Piercer, Cannon heat system is restrictive, Quick Draw is never introduced. The power fantasy exists but most players won't experience it.

**H5 (92/100) — Procedural Levels**: Zone pacing (Intro→Traversal→Destruction→Combat→Boss), walkability guarantee, material-gated weapon drops, hidden content system, per-era material distribution. Reliably produces interesting, completable levels. Near best-in-class for procedural mobile platformers.

**H6 (73/100) — Difficulty & Bosses**: Boss phase structure is well-designed with era variants adding novelty. Within-level pacing is excellent (buffer zones, weapon drops before they're needed). However: Era 0 boss is dull (charge-only with long idle gaps), Phase 3 pillar shelter is untaught, Teleport Dash is untelegraphed, and Campaign's 2 global lives creates a steep early wall.

**H7 (78/100) — Scoring & Achievements**: The 5-component system (Time, Items+Enemies, Combat Mastery, Exploration, Preservation) rewards varied playstyles. Star thresholds are calibrated. Combo system adds moment-to-moment engagement. However: time score's 25pts/sec decay creates tension with exploration that forces playstyle choice rather than rewarding mastery across all pillars. Combo resetting on environmental damage undermines combat mastery in destruction zones.

**H8 (93/100) — Ethical Retention**: No dark patterns, no coercive timers, no IAP. Daily/weekly challenges are opt-in. Session save/restore respects player time. "NEW" badges create gentle urgency without FOMO gating. Streak rewards consistency without punishing absence. Exemplary ethical design.

---

## 3) Evidence Gaps

| Missing Artifact | Required For | Priority |
|-----------------|-------------|----------|
| Tuning sheet with benchmark comparisons (Celeste, Mega Man, etc.) | C1 full pass | Low |
| Playtest session notes (5+ sessions) | C2, H6, H7 evidence | Medium |
| Device matrix / thumb reach audit | C4 mobile pass | Deferred (mobile) |
| Difficulty curve graphs across all 10 epochs | H6 visual pacing analysis | Medium |
| Player choice data for preserve-vs-destroy decisions | C2 strategic depth evidence | Medium |

---

## 4) What's Working Well

These systems are strong and should be preserved:

1. **Movement feel** — The full movement toolkit (coyote, buffer, wall-slide, wall-jump, stomp, variable jump) is complete and polished. Squash/stretch and dust particles add juice without cluttering.

2. **Zone pacing** — The Intro→Traversal→Buffer→Destruction→Combat→Buffer→Boss structure within each level creates natural rhythm. Buffer zones provide literal breathing room.

3. **Walkability guarantee** — Every generated level is completable with the starting weapon. Hard/reinforced blocks in the walking corridor are converted to Soft. This is a critical fairness guarantee for procedural content.

4. **5-component scoring** — Supporting speedrunners, explorers, combat specialists, and preservationists with separate score pillars is unusually deep for a mobile platformer.

5. **Ethical design** — Zero monetization pressure, opt-in challenges, transparent progression. The game respects the player's time and agency.

6. **Session persistence** — Auto-save, crash recovery, and app-background auto-pause mean players never lose progress unexpectedly.

7. **Social features** — Level codes, daily/weekly challenges, friend challenges, ghost replay, and shareable results provide strong organic retention hooks.

8. **Hit-stop and screen shake** — 33ms on enemy kill, 66ms on boss phase, 133ms on boss death. Satisfying moment-to-moment combat feel.

9. **Procedural audio** — Runtime-generated music and SFX with era-specific character. Recent fixes (Triangle waves, soft-clip normalization, strengthened LPF) have significantly improved quality.

10. **Boss era variants** — 7 distinct boss behaviors across 10 epochs (Primitive, Standard, Medieval, Renaissance, Industrial, Complex, Architect) prevent boss fights from feeling repetitive.

---

## 5) Observations on Game Design Tensions

### Time vs. Exploration (Intentional Tension)

The time score formula (`5000 - elapsed * 25`, floor 500) bottoms out at 180 seconds. Exploration score requires finding hidden content behind destructibles. Preservation score requires NOT destroying relics. These three pillars are in deliberate tension.

**Design intent**: Players should choose a playstyle per run, not excel at everything simultaneously. A speedrunner optimizes Time + Combat Mastery. An explorer optimizes Exploration + Preservation. The 3-star threshold at 68% (~13,500 of ~20,000) requires engaging with multiple pillars but doesn't require maximizing all of them.

**Risk**: If players don't understand this is intentional, the tension may feel like a design flaw rather than a strategic choice. Consider communicating the tradeoff explicitly — e.g., a loading screen tip: "Speed or secrets? Every run is a choice."

### Destruction as Double-Edged Sword

Breaking terrain reveals paths and hidden content but triggers hazards and sacrifices preservation score. This is the game's central strategic tension and it works well. The hazard frequency scaling (5% at epoch 0, 30% at epoch 9) ensures early epochs teach the mechanic gently before it becomes dangerous.

### Auto-Fire and Player Agency

Auto-fire prioritizes accessibility over traditional shooter agency. Combat engagement comes from positioning and movement rather than aiming. The Quick Draw mechanic (weapon cycling reward) provides active combat choice for engaged players. This is a defensible design decision for mobile, but the untaught cycling means the agency layer is invisible to most players.
