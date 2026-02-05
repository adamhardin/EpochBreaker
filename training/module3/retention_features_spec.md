# Assessment 3.3: Retention Features Specification

## Overview

Retention feature specifications for the retro side-scrolling shooter mobile game (iOS). This document covers daily challenge systems, social sharing via Level IDs, achievement design, mastery goals, and a KPI dashboard with specific thresholds. All features leverage the procedural generation system and deterministic Level ID format (`LVLID_[VERSION]_[DIFFICULTY]_[ERA]_[SEED64]`).

---

## 1. Retention Research Summary

### Industry Benchmarks for Mobile Platformers

| Metric | Industry Median (Mobile) | Top 25% Platformers | Our Targets |
|--------|-------------------------|---------------------|-------------|
| D1 Retention | 25-30% | 35-45% | >= 40% |
| D7 Retention | 8-12% | 15-22% | >= 20% |
| D30 Retention | 3-5% | 8-12% | >= 10% |
| Session Length | 4-6 min | 8-12 min | 6-10 min |
| Sessions/Day | 1.5-2.0 | 2.5-3.5 | >= 2.0 |

Sources: GameAnalytics 2024 Mobile Gaming Benchmarks; Sensor Tower Q4 2024 Mobile Game Performance Report; data.ai State of Mobile Gaming 2024.

### Top 3 Reasons Players Churn in First Session

1. **Confusion / No clear goal** (38% of first-session churn): Player doesn't understand what to do or why. Mitigation: Clear tutorial with zero-friction onboarding, no account creation, immediate gameplay within 30 seconds.

2. **Difficulty spike / Frustration** (28%): Player hits an impassable section too early. Mitigation: Smooth difficulty curve (Levels 1-5 are gentle, no death possible in Level 1), instant respawn, generous checkpoints.

3. **Lack of perceived depth** (22%): Player feels the game is shallow and not worth returning to. Mitigation: Tease the meta-loop early (show Tier progression, daily challenges, achievement progress by minute 10).

### Top 3 Reasons Players Return After Day 1

1. **Unfinished progress** (42%): Player was mid-tier or had an unsatisfied 3-star attempt. The Zeigarnik effect drives return to complete unfinished goals.

2. **Social pressure** (31%): A friend shared a Level ID, or the player wants to beat their own shared score. Social comparison creates return motivation.

3. **New content anticipation** (27%): Player saw an era teaser, achievement preview, or daily challenge notification. Curiosity about what comes next drives return.

### Ethical Design Principles Applied

- All engagement mechanics are opt-in, never coercive
- No artificial wait timers, energy systems, or session limits
- No FOMO-based limited-time content for gameplay items
- Daily challenges are bonus content, not required for progression
- Notifications are limited to 1/day max, player controls notification preferences
- No dark patterns: no artificial scarcity, no deceptive UI, no manipulative loss framing

---

## 2. Daily Challenge System

### 2.1 System Architecture

**Concept**: One unique procedurally generated level per day, shared globally. Every player plays the exact same level. Global leaderboard resets daily. Streak tracking rewards consistency.

**Seed Generation**:
```
daily_seed = SHA256(date_string + "DAILYCHALLENGE" + version_salt)
daily_level_id = LVLID_1_[rotating_difficulty]_[rotating_era]_[daily_seed_64bit]
```

| Component | Specification |
|-----------|--------------|
| Seed source | UTC date string (YYYY-MM-DD) + constant salt |
| Reset time | 00:00 UTC daily |
| Difficulty rotation | Cycles through 0-3 on a 4-day pattern (Easy Mon, Normal Tue, Hard Wed, Extreme Thu, repeat) |
| Era rotation | Cycles through 0-9 on a 10-day pattern offset by 1 day from difficulty |
| Level length | Fixed: 256 tiles wide (standardized for fair comparison) |
| Attempt limit | Unlimited attempts, only best score counts |
| Availability | Unlocks after completing Level 5 (Tier 1 skill gate) |

### 2.2 Daily Challenge Leaderboard

| Leaderboard Tier | Scope | Reward |
|-----------------|-------|--------|
| Global Top 100 | All players worldwide | "Daily Champion" badge (24h), 500 bonus coins |
| Global Top 1000 | All players worldwide | "Daily Elite" badge (24h), 200 bonus coins |
| Friends | Players on friends list | Bragging rights + notification sent to beaten friends |
| Personal Best | Self-comparison | "New Personal Best!" celebration animation |

**Score tiebreaker**: If scores are equal, faster completion time wins. If times are also equal, higher destruction percentage wins.

**Anti-cheat**: Daily challenge score submission includes:
- Completion time (validated against minimum theoretical time based on level layout)
- Input replay hash (can be reconstructed from Level ID + inputs for verification)
- Device attestation via Apple App Attest API

### 2.3 Streak System

Streaks reward consistency without punishing missed days harshly.

| Streak Length | Reward |
|--------------|--------|
| 3 days | 100 bonus coins + "Getting Started" streak badge |
| 7 days | 300 bonus coins + "Dedicated" streak badge + random cosmetic unlock |
| 14 days | 500 bonus coins + "Committed" streak badge |
| 30 days | 1000 bonus coins + "Relentless" streak badge + exclusive trail effect |
| 60 days | 2000 bonus coins + "Legendary" streak badge + exclusive character skin |

**Streak protection**: Missing one day does not break the streak immediately. Instead:
- 1 missed day: streak is "frozen" (gold turns silver in UI), next completion resumes it
- 2 consecutive missed days: streak resets to 0
- This prevents punishing players for a single busy day while still requiring consistent engagement

**Notification**: One push notification at player-chosen time: "Your daily challenge is ready! Current streak: [X] days." Player can disable entirely in settings.

### 2.4 Daily Challenge UI Flow

1. Main menu shows "DAILY CHALLENGE" button with today's era icon and difficulty star rating
2. Tap to see preview: era, difficulty, current #1 score, friend scores, personal best
3. "PLAY" button starts the level
4. On completion: score breakdown (including destruction % and weapons collected), rank position, comparison to friends, streak update
5. "RETRY" to improve score, "SHARE" to send Level ID, "DONE" to return to menu

---

## 3. Level ID Sharing & Friend Comparison

### 3.1 Sharing Flow

The Level ID system (`LVLID_1_2_0_9876543210ABCDEF`) enables organic social acquisition:

**Share triggers** (moments when sharing is surfaced):
1. After completing any level (score screen has "SHARE" button)
2. After achieving a 3-star rating ("Share your perfect run!")
3. After setting a personal best time ("Challenge a friend to beat your time!")
4. After achieving 100% destruction ("Share your demolition run!")
5. Main menu "Share a Level" button (share any previously played level)

**Share format** (iOS Share Sheet):
```
I scored [SCORE] on this level! ([DESTRUCTION]% destroyed, [WEAPONS] weapons collected) Can you beat me?
[GAME_NAME] Level: LVLID_1_2_0_9876543210ABCDEF
Tap to play: [deep_link_url]
```

**Deep link handling**:
- URL format: `https://[domain]/play/LVLID_1_2_0_9876543210ABCDEF`
- If app installed: opens directly to level with challenger's score shown as ghost/target
- If app not installed: redirects to App Store listing with Level ID preserved in referral parameter
- Post-install: first launch detects pending Level ID and offers to play it after tutorial completion

### 3.2 Friend Comparison System

**Score comparison UI**: When playing a shared level, the HUD shows:
- Challenger's score as a target: "BEAT: 12,450"
- Real-time progress indicator: green (on track to beat) / red (behind pace)
- Challenger's star rating displayed as ghost stars

**Comparison result screen**:
```
YOUR SCORE: 13,200       vs.      FRIEND'S SCORE: 12,450
    Time: 1:42                         Time: 1:58
    Destruction: 94%                   Destruction: 78%
    Weapons: 3/4                       Weapons: 2/4
    Damage: 1                          Damage: 3
    Secrets: 2/2                       Secrets: 1/2

    VERDICT: YOU WIN! (+750 points)

    [SHARE RESULT]  [CHALLENGE BACK]  [NEXT LEVEL]
```

**"Challenge Back"**: Generates a new level with similar difficulty parameters and sends it to the friend. This creates a volley-style engagement loop.

### 3.3 Social Features Integration

| Feature | Implementation | Platform |
|---------|---------------|----------|
| Level ID sharing | iOS Share Sheet (Messages, WhatsApp, etc.) | Native iOS |
| Friend leaderboards | Apple Game Center friends list | GameKit |
| Score comparison | In-game comparison screen | Custom UI |
| Challenge notifications | Local push notification when friend beats your shared level | UNUserNotificationCenter |
| Replay viewing | Not in v1 (future feature: share input replay as video) | Future |

---

## 4. Achievement System

### 4.1 Design Philosophy

Achievements reward **skill**, **destruction**, **exploration**, and **persistence** -- never just time played. Each achievement has 3 tiers (Bronze, Silver, Gold) that represent increasing mastery. Achievements are organized into 5 categories.

### 4.2 Achievement Categories & Full List

#### Category 1: Destruction Mastery (Skill-Based)

| # | Achievement | Bronze | Silver | Gold |
|---|------------|--------|--------|------|
| 1 | Demolisher | Destroy 100 wall tiles | Destroy 1,000 wall tiles | Destroy 10,000 wall tiles |
| 2 | Untouchable | Complete 1 level without taking damage | 5 levels no damage | 20 levels no damage |
| 3 | Boss Slayer | Defeat 1 boss | Defeat 5 bosses | Defeat all 10 era bosses |
| 4 | Path Finder | Discover 1 hidden destruction path | 5 destruction paths | 20 destruction paths |
| 5 | Total Annihilation | Achieve 100% destruction on 1 level | 5 levels 100% destruction | 20 levels 100% destruction |

#### Category 2: Arsenal (Weapon-Based)

| # | Achievement | Bronze | Silver | Gold |
|---|------------|--------|--------|------|
| 6 | Collector | Pick up 10 weapon attachments | Pick up 100 weapon attachments | Pick up 500 weapon attachments |
| 7 | Arsenal Master | Use 3 different weapon types in one level | Use 5 different types | Use all weapon types in one level |
| 8 | Era Explorer | Complete 1 level in each era | 3 levels in each era | 5 levels in each era |
| 9 | Special Ops | Trigger 5 special attacks (hold jump when powered up) | 25 special attacks | 100 special attacks |
| 10 | Completionist | 100% a level (all secrets, 100% destruction, 3 stars) | 100% 10 levels | 100% 25 levels |

#### Category 3: Speedrunner (Skill/Persistence)

| # | Achievement | Bronze | Silver | Gold |
|---|------------|--------|--------|------|
| 11 | Speed Demon | Complete any level under par time | 10 levels under par | 25 levels under par |
| 12 | Sub-Minute | Complete any level in under 60 seconds | 5 levels in under 60s | 10 levels in under 60s |
| 13 | Blitz | Complete a level in under 30 seconds | 3 levels under 30s | 5 levels under 30s |

#### Category 4: Social (Engagement)

| # | Achievement | Bronze | Silver | Gold |
|---|------------|--------|--------|------|
| 14 | Challenger | Share 1 Level ID | Share 10 Level IDs | Share 25 Level IDs |
| 15 | Rival | Beat a friend's shared level score | Beat 10 friend scores | Beat 50 friend scores |
| 16 | Daily Devotee | Complete 7 daily challenges | 30 daily challenges | 100 daily challenges |

#### Category 5: Mastery (Persistence/Skill)

| # | Achievement | Bronze | Silver | Gold |
|---|------------|--------|--------|------|
| 17 | Perfect Run | Complete 1 level with 3 stars + no damage + under par time | 5 perfect runs | 15 perfect runs |
| 18 | Era Climber | Complete Era 1 (Stone Age) | Complete Era 5 (Renaissance) | Complete all 10 Eras |
| 19 | Weapon Specialist | Collect every weapon type at least once | Complete a level with maximum weapon synergy score | All weapons mastered on Extreme difficulty |
| 20 | Endurance | Play 25 total levels | Play 100 total levels | Play 250 total levels |

### 4.3 Achievement Reward Structure

| Tier | Reward | XP Value |
|------|--------|----------|
| Bronze | 50 coins + achievement toast notification | 100 XP |
| Silver | 150 coins + unique cosmetic element (per achievement) | 300 XP |
| Gold | 500 coins + exclusive cosmetic set piece + achievement banner | 1000 XP |

**Achievement banner**: Gold achievements unlock a banner frame for the player's profile card (visible on leaderboards and shared levels).

**Total achievements**: 20 achievements x 3 tiers = 60 achievement milestones
**Total coins from all achievements**: (20 x 50) + (20 x 150) + (20 x 500) = 1,000 + 3,000 + 10,000 = 14,000 coins

### 4.4 Achievement Display

- Main menu shows achievement progress bar: "[X]/60 achievements unlocked"
- Achievement screen organized by category with visual progress rings
- Each achievement shows progress toward next tier (e.g., "Walls destroyed: 872/1,000")
- New achievement unlocks trigger a full-screen celebration animation (1.5s, skippable)
- Gold achievements have animated borders and particle effects in the achievement screen

---

## 5. Long-Term Mastery Goals

### 5.1 Perfect Run System

A "perfect run" is the highest-skill accomplishment per level:

**Perfect Run criteria**:
- 3-star score rating
- Zero damage taken
- All secrets found
- 100% destruction achieved
- Completion under par time

**Perfect run reward per level**: "P" badge on level select, level frame turns gold, visible on shared Level IDs

**Perfect run tracking**:
- Level select shows: "Perfect: 12/50 levels"
- Per-era breakdown: "Stone Age: 3/5, Bronze Age: 2/5, Iron Age: 2/5, Medieval: 1/5, Renaissance: 1/5, Industrial: 1/5, Modern: 1/5, Digital: 1/5, Space Age: 0/5, Transcendent: 0/5"
- Completing all 50 perfect runs unlocks "True Mastery" title and exclusive golden character skin

### 5.2 Speedrun Targets

Every level has three time targets, derived from the level's geometry and destructible wall count:

**Par time formula**:
```
par_time = base_traversal_time + (destruction_time) + buffer
where:
  base_traversal_time = level_width_tiles / player_run_speed * traversal_complexity_multiplier
  destruction_time = destructible_wall_count * avg_destroy_time_per_wall
  buffer = par_time * 0.20 (20% grace period)
  traversal_complexity_multiplier = 1.0 + (0.1 * vertical_layers) + (0.05 * moving_platform_count)
```

| Target | Threshold | Indicator | Reward |
|--------|-----------|-----------|--------|
| Par Time | Formula above | Bronze clock icon | Score multiplier x1.2 |
| Speed Target | Par time x 0.75 | Silver clock icon | Score multiplier x1.5 + "Fast!" badge |
| Blitz Target | Par time x 0.50 | Gold clock icon | Score multiplier x2.0 + "Blitz!" badge + trail effect |

**Speedrun leaderboard**: Separate from score leaderboard, sorted by completion time only. Accessible per level from level select.

### 5.3 Difficulty Mastery

**Era mastery badges**: Completing all levels in an era with 3 stars earns an era mastery badge:
- Era 1 Mastery: "Stone Hunter" title + bone aura cosmetic
- Era 2 Mastery: "Bronze Commander" title + bronze shimmer cosmetic
- Era 3 Mastery: "Iron Warden" title + iron aura cosmetic
- Era 4 Mastery: "Medieval Knight" title + heraldic shield cosmetic
- Era 5 Mastery: "Renaissance Master" title + inventor aura cosmetic
- Era 6 Mastery: "Industrial Titan" title + steam aura cosmetic
- Era 7 Mastery: "Modern Operative" title + tactical aura cosmetic
- Era 8 Mastery: "Digital Architect" title + glitch aura cosmetic
- Era 9 Mastery: "Space Pioneer" title + nebula aura cosmetic
- Era 10 Mastery: "The Transcendent" title + prismatic aura cosmetic (combines all)

**Extreme difficulty mode**: After completing all 50 levels on normal difficulty, an "Extreme" modifier unlocks. Extreme mode applies globally:
- Enemy speed +25%
- Wall material hardness +50%
- Timing windows -30%
- Weapon attachment spawns -40%
- Separate leaderboard for Extreme completions

### 5.4 Statistics Tracking

Long-term statistics provide a sense of cumulative accomplishment:

| Statistic | Display |
|-----------|---------|
| Total walls destroyed | Lifetime counter + per-material breakdown |
| Total weapon attachments collected | Lifetime counter + per-type breakdown |
| Total levels completed | Counter + per-era breakdown |
| Total deaths | Counter + per-cause breakdown (falls, enemies, hazards) |
| Total play time | Hours:minutes |
| Favorite era | Most-played era |
| Best daily challenge rank | Historical best rank + date |
| Longest streak | Highest daily challenge streak achieved |
| Levels shared | Total Level IDs shared |
| Friends beaten | Total times player outscored a friend's shared level |
| Perfect runs completed | Counter + per-era breakdown |
| Fastest level completion | Record time + Level ID |
| Highest destruction percentage | Best % + Level ID |
| Weapons collected in single level | Record count + Level ID |

---

## 6. First-Session Return Hooks

Specific mechanisms designed to make the player think about the game after closing it:

### 6.1 Unfinished Business (Zeigarnik Effect)

| Hook | Trigger | Psychology |
|------|---------|-----------|
| Boss preview | After Level 9, boss silhouette shown: "The Stone Colossus awaits..." | Anticipation of unseen challenge |
| Near-miss 3-star | Score screen shows: "87 points from 3 stars!" | Desire to complete the near-goal |
| Achievement almost-done | Progress bar at 80%+: "2 more walls for Demolisher Bronze!" | Completion drive |
| Era teaser | Level 10 score screen shows Bronze Age environment preview | Curiosity about new content |
| Streak start | After first daily challenge: "1-day streak! Come back tomorrow for day 2" | Consistency motivation |
| Weapon preview | After finding 2/4 weapon types: "2 more weapon types to discover in this era!" | Collection drive |

### 6.2 Anticipated Return Moment

| Hook | Implementation |
|------|---------------|
| Daily challenge refresh | "New challenge in 8h 23m" countdown on main menu |
| Friend challenge pending | "Alex scored 12,450 on your shared level. Can you beat it?" notification |
| Achievement progress | "Destroy 3 more walls to unlock Demolisher Bronze" |
| Next unlock preview | "2 levels until you unlock the Scatter Shot attachment!" |

### 6.3 Notification Strategy

| Notification Type | Timing | Frequency | Opt-Out |
|------------------|--------|-----------|---------|
| Daily challenge available | Player-chosen time (default: 10 AM local) | 1/day max | Yes |
| Friend beat your score | Within 5 min of event | Max 3/day | Yes |
| Streak at risk | 20:00 local time if challenge not completed | 1/day max | Yes |
| Achievement nearly complete | On app background if 90%+ progress | Max 1/week | Yes |

**Rules**:
- Never more than 2 notifications per day total
- No notifications in first 24 hours after install (let the game speak for itself)
- All notification types individually toggleable in Settings
- No "come back, we miss you!" manipulative notifications
- Notifications stop entirely after 14 days of inactivity (respect player's decision)

---

## 7. Anti-Dark-Pattern Audit

### Patterns Evaluated and Rejected

| Dark Pattern | Status | Rationale |
|-------------|--------|-----------|
| Energy/Lives system | REJECTED | No artificial play limits. Players can play as much as they want. |
| FOMO limited-time offers | REJECTED | No time-limited gameplay content. Cosmetics may rotate seasonally but are never removed permanently. |
| Loot boxes / gacha | REJECTED | All purchases are deterministic. Player knows exactly what they are buying. |
| Pay-to-skip | REJECTED | No level skips, no instant unlocks. Progression is earned. |
| Artificial difficulty walls | REJECTED | Difficulty curve is smooth. No intentional frustration spikes designed to sell solutions. |
| Misleading UI | REJECTED | No "X" buttons that look like close but are purchase buttons. All UI is honest. |
| Social obligation | REJECTED | Sharing is always optional. No "send lives to friends" mechanics. |
| Loss aversion | REJECTED | No "your progress will be lost" warnings. Game auto-saves constantly. |
| Streak punishment | MITIGATED | 1-day grace period prevents harsh punishment. Streak is a bonus, not a requirement. |
| Notification manipulation | MITIGATED | Strict limits, full opt-out, no guilt-tripping language. |

### Ethical Design Principles

1. **Respect player time**: Every session should feel worthwhile regardless of length
2. **Respect player money**: Every purchase is a known-value exchange, no gambling
3. **Respect player attention**: Notifications are useful, not manipulative
4. **Respect player choice**: All engagement features are opt-in bonuses
5. **Respect player absence**: No punishment for not playing, no "decay" mechanics

---

## 8. KPI Dashboard Specification

### 8.1 Primary KPIs

| KPI | Formula | Target | Warning | Critical |
|-----|---------|--------|---------|----------|
| D1 Retention | Users active Day 1 / Users installed Day 0 | >= 40% | 35-40% | < 35% |
| D7 Retention | Users active Day 7 / Users installed Day 0 | >= 20% | 15-20% | < 15% |
| D30 Retention | Users active Day 30 / Users installed Day 0 | >= 10% | 7-10% | < 7% |
| Avg Session Length | Total play time / Total sessions | 6-10 min | 4-6 min | < 4 min |
| Sessions per Day | Total sessions / DAU | >= 2.0 | 1.5-2.0 | < 1.5 |
| Tutorial Completion | Users completing Level 5 / Users starting Level 1 | >= 75% | 65-75% | < 65% |
| D1 Payer Conversion | Users who purchase Day 0-1 / Users installed Day 0 | >= 2% | 1-2% | < 1% |

### 8.2 Feature-Specific KPIs

| KPI | Formula | Target | Warning |
|-----|---------|--------|---------|
| Daily Challenge Participation | Users completing daily challenge / DAU | >= 30% | < 20% |
| Level Share Rate | Level IDs shared / Levels completed | >= 5% | < 2% |
| Share-to-Install Rate | Installs from shared links / Total shares | >= 10% | < 5% |
| Achievement Engagement | Users with 5+ achievements / Users past Level 10 | >= 60% | < 40% |
| Streak 7-Day Rate | Users with 7+ day streak / Daily challenge participants | >= 25% | < 15% |
| Retry Rate | Level retries / Level completions | 0.3-0.8 | < 0.2 or > 1.5 |
| Boss Defeat Rate | Boss defeats / Boss attempts (per boss) | 30-60% | < 20% or > 80% |
| Avg Destruction % | Mean destruction percentage per completed level | 60-85% | < 40% |

### 8.3 Churn Indicators (Early Warning)

| Indicator | Signal | Action |
|-----------|--------|--------|
| Session length declining | 3 consecutive sessions shorter than previous | Check difficulty curve at player's current level |
| Retry count spiking | > 5 retries on same level | Player may be stuck; trigger optional hint system |
| Zero daily challenges for 3 days | Active player stops doing dailies | Check if daily difficulty is appropriate |
| No shares after 10 levels | Player never uses share feature | Surface sharing prompts more naturally |
| Achievement progress stalled | No achievement progress in 7 days | Check if achievable goals are visible |
| Destruction % very low | Player consistently below 30% destruction | Tutorial on destruction mechanics may be needed |

### 8.4 Dashboard Implementation

**Technology**: Unity Analytics + custom Firebase event logging

**Event schema**:
```json
{
  "event": "level_complete",
  "user_id": "[hashed]",
  "level_id": "LVLID_1_2_0_9876543210ABCDEF",
  "level_number": 15,
  "score": 12450,
  "stars": 2,
  "time_seconds": 142,
  "deaths": 3,
  "damage_taken": 5,
  "destruction_percentage": 82,
  "weapons_collected": 3,
  "walls_destroyed": 47,
  "secrets_found": 1,
  "retries": 1,
  "session_number": 7,
  "day_since_install": 3,
  "daily_challenge": false,
  "shared_level": false,
  "weapons_used": ["scatter_shot", "piercing_beam", "homing_missile"],
  "device_model": "iPhone14,2",
  "os_version": "17.2",
  "app_version": "1.0.0"
}
```

**Dashboard refresh**: Real-time for DAU/session metrics, hourly for retention calculations, daily for trend analysis.

**Alerts**: Slack integration for critical KPI breaches. PagerDuty for crash rate > 1%.

### 8.5 A/B Testing Framework

| Test | Variants | Success Metric | Sample Size |
|------|----------|---------------|-------------|
| Tutorial length | 3 levels vs 5 levels vs 7 levels | Tutorial completion rate + D1 retention | 5,000/variant |
| Daily challenge difficulty | Fixed rotation vs adaptive vs random | Daily challenge participation rate | 3,000/variant |
| Share prompt timing | After every level vs after 3-star vs after 100% destruction only | Share rate + share-to-install rate | 5,000/variant |
| Notification cadence | 1/day vs 2/day vs smart (ML-based) | D7 retention | 5,000/variant |
| Streak grace period | 0 days vs 1 day vs 2 days | Streak 7-day rate + D30 retention | 3,000/variant |

---

## References

- GameAnalytics (2024). *Mobile Gaming Benchmarks Report*. Retention and engagement benchmarks by genre.
- Sensor Tower (2024). *State of Mobile Gaming Q4 2024*. Revenue and download metrics for indie platformers.
- data.ai (2024). *State of Mobile 2024*. User acquisition and engagement trends.
- Deterding, S. (2012). *Gamification: Designing for Motivation*. Ethical gamification principles.
- Eyal, N. (2014). *Hooked: How to Build Habit-Forming Products*. Hook model (with ethical application).
- Apple Developer Documentation (2024). *App Attest API*. Anti-cheat device attestation.
- Apple Developer Documentation (2024). *GameKit Framework*. Leaderboards and achievements.

---

**Version**: 1.1
**Last Updated**: 2026-02-04
**Status**: Active
**Assessment**: 3.3 - Retention Features Specification
