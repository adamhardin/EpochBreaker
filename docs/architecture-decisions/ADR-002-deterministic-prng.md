# ADR-002: Deterministic PRNG

**Date:** 2026-02-04
**Status:** Accepted

## Context

The game features procedurally generated levels that players can share via compact level IDs (see ADR-003). When a recipient enters a shared level ID, the game must reconstruct the **exact same level** -- tile-for-tile, enemy-for-enemy -- regardless of the device, OS version, or Unity patch version the recipient is running.

This requirement demands **cross-platform, cross-version determinism** in every calculation that influences level layout. Specifically:

- **`System.Random`** does not guarantee identical sequences across .NET / Mono / IL2CPP runtimes or across .NET versions. Microsoft's documentation explicitly states the implementation may change.
- **`UnityEngine.Random`** is deterministic for a given Unity version but is not guaranteed stable across Unity patch releases. It also carries global state, making it unsafe in multithreaded or coroutine-heavy generation code.
- **`Mathf.PerlinNoise`** is similarly undocumented with respect to cross-version stability and has known precision issues at large coordinate values.

We need a PRNG that:

1. Produces identical output for identical input on every platform, forever.
2. Has a period large enough to avoid visible repetition across 2^64 possible seeds.
3. Is fast enough to generate a full level within our 150 ms budget on the baseline device (ADR-005).
4. Passes basic statistical quality tests (no visible banding or correlation in terrain).

## Decision

We will implement **xorshift64\*** (Vigna, 2016) as our sole source of randomness in the level generation pipeline.

```
// Reference implementation
public struct Xorshift64Star
{
    private ulong _state;

    public Xorshift64Star(ulong seed)
    {
        // Zero seed is a fixed point; avoid it.
        _state = seed != 0 ? seed : 1;
    }

    public ulong Next()
    {
        _state ^= _state >> 12;
        _state ^= _state << 25;
        _state ^= _state >> 27;
        return _state * 0x2545F4914F6CDD1DUL;
    }

    // Convenience: returns a double in [0, 1)
    public double NextDouble()
    {
        return (Next() >> 11) * (1.0 / (1UL << 53));
    }
}
```

The following rules apply to all code in the generation pipeline:

- **No Unity math functions.** `Mathf.PerlinNoise`, `UnityEngine.Random`, `Mathf.Lerp`, and any other function whose implementation is internal to the Unity engine must not be called during level generation. Pure C# math (`System.Math`, manual fixed-point, or our own implementations) must be used instead.
- **No `System.Random`.** All randomness derives from the `Xorshift64Star` struct.
- **No floating-point in layout decisions.** Tile placement, platform positions, and enemy spawn locations will use integer or fixed-point arithmetic to avoid IEEE 754 rounding differences across compilers. Floating-point is permitted only for visual-only parameters (e.g., parallax offsets) that do not affect gameplay or level identity.
- **Noise functions** (Perlin, simplex) required for organic terrain shapes will be implemented from scratch in pure C# with integer or fixed-point internals, seeded from the xorshift64* state.

## Consequences

### Positive

- **Guaranteed determinism.** The PRNG state is fully contained in a single `ulong`. Identical seeds produce identical sequences on any platform, any runtime, any Unity version.
- **Excellent performance.** xorshift64* produces a 64-bit result in ~3 CPU instructions. Benchmarks show >500 million values/second on A13 Bionic, far exceeding our generation budget.
- **Large period.** The period is 2^64 - 1, making repetition effectively impossible.
- **Statistical quality.** xorshift64* passes BigCrush and PractRand, ensuring terrain and object distributions look natural.
- **Minimal state.** The 8-byte state is trivially serializable for save/restore and replay systems.

### Negative

- **Custom noise implementation required.** We cannot use Unity's built-in Perlin noise or any third-party noise library that internally uses non-deterministic math. We must write and test our own Perlin/simplex noise from scratch. Estimated effort: 2-3 days.
- **Developer discipline burden.** Any engineer working on the generation pipeline must understand and follow the "no Unity math" rule. A generation-path linter or static analysis rule should be added to CI to catch violations.
- **Not cryptographically secure.** xorshift64* is trivially predictable. Players could reverse-engineer seeds from level data. This is acceptable -- level IDs are not secrets.
- **Fixed-point arithmetic complexity.** Avoiding floating-point in layout calculations adds implementation complexity and makes the code less readable. Thorough unit tests comparing outputs across platforms will be necessary.
