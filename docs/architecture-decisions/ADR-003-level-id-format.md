# ADR-003: Level ID Format

**Date:** 2026-02-04
**Status:** Accepted

## Context

Players need to share procedurally generated levels with each other. The sharing mechanism must work across all common channels: text messages, social media posts, Discord, clipboard paste, and even verbal dictation. This means the level identifier must be:

1. **Compact.** Short enough to fit in a tweet, a text message, or a screenshot without truncation.
2. **Human-readable.** Visually parseable so players can identify the difficulty and biome at a glance.
3. **Self-contained.** The ID alone must carry enough information to fully reconstruct the level without a server lookup.
4. **Versioned.** The format must support future changes to the generation algorithm without breaking old IDs.
5. **Unambiguous.** No characters that are easily confused (0/O, 1/l/I) in the seed portion.

The level is fully determined by three parameters plus the 64-bit PRNG seed (ADR-002):

- **Generator version** -- which generation algorithm to use.
- **Difficulty** -- affects enemy density, platform spacing, and hazard frequency.
- **Biome** -- determines tileset, background, and environmental hazards.

## Decision

The level ID format is:

```
LVLID_VVV_DD_BB_SSSSSSSSSSSSSSSS
```

| Field | Chars | Description |
|---|---|---|
| `LVLID` | 5 | Fixed prefix. Enables clipboard detection and deep-link routing. |
| `VVV` | 3 | Generator version, zero-padded decimal (001-999). |
| `DD` | 2 | Difficulty tier, zero-padded decimal (01-99). |
| `BB` | 2 | Biome code, zero-padded decimal (01-99). |
| `SSSSSSSSSSSSSSSS` | 16 | 64-bit seed encoded as uppercase hexadecimal. |
| Separators (`_`) | 4 | Underscores between each field. |
| **Total** | **32 + 4 + 3 (prefix)** | **39 characters** |

Example:

```
LVLID_001_03_07_A3F29C7B004E1D82
```

This encodes: generator version 1, difficulty tier 3, biome 7, seed `0xA3F29C7B004E1D82`.

### Parsing Rules

- The parser must accept both uppercase and lowercase hex digits in the seed field.
- Unknown version numbers cause a graceful error ("This level requires a newer version of the game").
- Difficulty and biome values outside the currently supported range produce a clear error rather than undefined behavior.
- The `LVLID_` prefix is mandatory. Strings without it are rejected immediately, enabling fast clipboard scanning.

### Deep Linking

The ID will also serve as a deep-link path:

```
https://ourgame.example.com/level/LVLID_001_03_07_A3F29C7B004E1D82
```

The app registers a Universal Link handler for this path pattern, extracting and parsing the ID from the URL.

## Consequences

### Positive

- **Version field enables forward compatibility.** When the generation algorithm changes (new tile types, rebalanced difficulty curves), we increment the version. Old IDs with older versions continue to use the original algorithm, preserving shared levels permanently.
- **Human-scannable metadata.** Players can see difficulty and biome at a glance without loading the level. Community sites and leaderboards can sort and filter by these fields trivially.
- **Massive seed space.** 64 bits of seed provide 1.8 x 10^19 unique levels per version/difficulty/biome combination. Combined with 999 versions, 99 difficulties, and 99 biomes, the total addressable space is effectively inexhaustible.
- **No server dependency.** Level sharing works entirely peer-to-peer via text. No backend infrastructure is required to store or resolve level IDs.
- **Deep-link ready.** The fixed prefix and structured format make URL-based sharing straightforward.

### Negative

- **39 characters is not trivially short.** It is longer than a typical short URL but remains within the practical limit for text sharing. QR code generation will be offered as an alternative for in-person sharing.
- **Hex seed is not human-friendly.** Players will not memorise seeds. This is acceptable since the primary sharing mechanism is copy-paste or deep link, not memorisation.
- **No checksum.** A mistyped character will silently produce a different (but valid) level rather than an error. Adding a checksum digit would increase length to 40-41 characters. We accept this trade-off; a wrong level is a minor annoyance, not a data-loss scenario. This decision may be revisited if user feedback indicates frequent transcription errors.
- **Fixed field widths.** The format supports up to 999 generator versions, 99 difficulty tiers, and 99 biomes. If any of these limits are exceeded, a new ID format version will be required. These limits are generous for the foreseeable product roadmap.
