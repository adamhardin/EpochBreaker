# ADR-003: Level ID Format

**Date:** 2026-02-04
**Status:** Accepted
**Revised:** 2026-02-07 (updated to reflect implemented format)

## Context

Players need to share procedurally generated levels with each other. The sharing mechanism must work across all common channels: text messages, social media posts, Discord, clipboard paste, and even verbal dictation. This means the level identifier must be:

1. **Compact.** Short enough to fit in a tweet, a text message, or a screenshot without truncation.
2. **Human-readable.** Visually parseable so players can identify the epoch at a glance.
3. **Self-contained.** The ID alone must carry enough information to fully reconstruct the level without a server lookup.
4. **Unambiguous.** No characters that are easily confused (0/O, 1/l/I) in the seed portion.

The level is fully determined by two parameters:

- **Epoch** -- which historical era (0-9) determines tileset, enemies, music, and aesthetics.
- **Seed** -- a 40-bit value that drives all procedural generation via xorshift64* PRNG.

Difficulty is handled separately via `DifficultyManager` (player setting, not encoded in the level ID).

## Decision

The level ID format is:

```
E-XXXXXXXX
```

| Field | Chars | Description |
|---|---|---|
| `E` | 1 | Epoch digit (0-9). Determines era, tileset, enemies, and music. |
| `-` | 1 | Separator. |
| `XXXXXXXX` | 8 | 40-bit seed encoded as 8 base32 characters (alphabet: `0-9, A-V` excluding ambiguous I/O). |
| **Total** | **10** | **10 characters** |

Example:

```
3-K7XM2P9A
```

This encodes: epoch 3 (Medieval), seed `K7XM2P9A`.

### Base32 Alphabet

```
0123456789ABCDEFGHJKLMNPQRSTUVWX
```

32 characters = 5 bits each. 8 characters = 40 bits = ~1.1 trillion unique levels per epoch.

### Parsing Rules

- The parser accepts both uppercase and lowercase input (normalised to uppercase).
- Epoch digit must be 0-9. Values outside this range are rejected.
- The `-` separator is mandatory. Strings without it are rejected immediately.
- Invalid base32 characters in the seed portion cause a parse failure.
- Whitespace is stripped before parsing.

### Implementation

The `LevelID` struct (`Scripts/Generative/LevelID.cs`) provides:
- `LevelID.GenerateRandom(epoch)` — creates a new random level
- `LevelID.TryParse(code, out result)` — parses a shareable code
- `LevelID.ToCode()` — encodes to shareable string
- `LevelID.Next()` — derives the next level in a deterministic sequence (increments epoch, mixes seed)

## Consequences

### Positive

- **Extremely compact.** 10 characters fit easily in any sharing medium, including verbal dictation.
- **Epoch visible at a glance.** The leading digit immediately tells players which era the level is in.
- **No server dependency.** Level sharing works entirely peer-to-peer via text.
- **Massive seed space.** 40 bits provide ~1.1 trillion unique levels per epoch, effectively inexhaustible.
- **No ambiguous characters.** The base32 alphabet excludes I, O, Y, and Z to prevent confusion.

### Negative

- **No embedded version field.** If the generation algorithm changes, old codes may produce different levels. This is mitigated by keeping the generator deterministic and stable.
- **No embedded difficulty.** Difficulty is a player setting, not part of the level identity. Two players with different difficulty settings will have different enemy counts but the same level layout from the same code.
- **No checksum.** A mistyped character will silently produce a different (but valid) level. This is acceptable since the primary sharing mechanism is copy-paste, not manual transcription.

### Superseded

This ADR replaces the original specification which proposed a 39-character `LVLID_VVV_DD_BB_SSSSSSSSSSSSSSSS` format. The implemented format is dramatically shorter (10 vs 39 characters) and better suited to casual sharing.
