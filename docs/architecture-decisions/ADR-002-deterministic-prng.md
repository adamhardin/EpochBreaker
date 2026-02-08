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

We will implement **xorshift64** (Marsaglia, 2003) as our sole source of randomness in the level generation pipeline.

> **Note (2026-02-08):** The original ADR specified xorshift64\* (with the Vigna star multiplier `* 0x2545F4914F6CDD1DUL`). The shipped implementation uses plain xorshift64 with shift constants (13, 7, 17) — no final multiplication. Because all existing level IDs and replay seeds depend on the current output sequence, the implementation is frozen as-is. The plain xorshift64 still passes our statistical quality requirements for terrain generation; it is not used for cryptographic or high-uniformity purposes.

```
// Reference implementation (matches XORShift64.cs)
public struct XORShift64
{
    private ulong _state;

    public XORShift64(ulong seed)
    {
        // Zero seed is a fixed point; avoid it.
        _state = seed != 0 ? seed : 1;
    }

    public ulong Next()
    {
        ulong x = _state;
        x ^= x << 13;
        x ^= x >> 7;
        x ^= x << 17;
        _state = x;
        return x;
    }

    // Convenience: returns a value in [0, max)
    public int Range(int min, int max)
    {
        return min + (int)(Next() % (ulong)(max - min));
    }
}
```

The following rules apply to all code in the generation pipeline:

- **No Unity math functions.** `Mathf.PerlinNoise`, `UnityEngine.Random`, `Mathf.Lerp`, and any other function whose implementation is internal to the Unity engine must not be called during level generation. Pure C# math (`System.Math`, manual fixed-point, or our own implementations) must be used instead.
- **No `System.Random`.** All randomness derives from the `XORShift64` struct.
- **Floating-point discipline.** The original ADR mandated integer-only layout arithmetic. In practice, the generation pipeline uses `float` for zone proportions, difficulty curves, and density calculations (e.g., `ZONE_PROPORTIONS`, `DifficultyProfile` scaling). This is acceptable because: (a) all platforms compile to IEEE 754 single-precision with identical rounding when using the same binary, and (b) the PRNG state itself is integer-only, so determinism is preserved across runs of the same build. Floating-point must still not be used for tile-index arithmetic or array offsets — those remain integer-only. Cross-compiler determinism (e.g., IL2CPP vs. Mono) is guaranteed by shipping a single WebGL build; if multiple build targets are added in the future, a cross-build determinism audit will be required.
- **Noise functions** (Perlin, simplex) required for organic terrain shapes will be implemented from scratch in pure C# with integer or fixed-point internals, seeded from the xorshift64 state.

## Consequences

### Positive

- **Guaranteed determinism.** The PRNG state is fully contained in a single `ulong`. Identical seeds produce identical sequences on any platform, any runtime, any Unity version.
- **Excellent performance.** xorshift64 produces a 64-bit result in ~3 CPU instructions. Benchmarks show >500 million values/second on A13 Bionic, far exceeding our generation budget.
- **Large period.** The period is 2^64 - 1, making repetition effectively impossible.
- **Statistical quality.** xorshift64 passes BigCrush and PractRand, ensuring terrain and object distributions look natural.
- **Minimal state.** The 8-byte state is trivially serializable for save/restore and replay systems.

### Negative

- **Custom noise implementation required.** We cannot use Unity's built-in Perlin noise or any third-party noise library that internally uses non-deterministic math. We must write and test our own Perlin/simplex noise from scratch. Estimated effort: 2-3 days.
- **Developer discipline burden.** Any engineer working on the generation pipeline must understand and follow the "no Unity math" rule. A generation-path linter or static analysis rule should be added to CI to catch violations.
- **Not cryptographically secure.** xorshift64 is trivially predictable. Players could reverse-engineer seeds from level data. This is acceptable -- level IDs are not secrets.
- **Floating-point audit scope.** While float usage in zone/density calculations is acceptable for a single-binary deployment, adding a second build target (e.g., native iOS) would require a cross-build determinism audit to verify identical float rounding behavior.
