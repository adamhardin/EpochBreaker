using System;

namespace SixteenBit.Generative
{
    /// <summary>
    /// Deterministic pseudo-random number generator using the xorshift64* algorithm.
    /// All level generation randomness MUST flow through this class to guarantee
    /// cross-platform determinism. Do NOT use UnityEngine.Random, System.Random,
    /// or Mathf.PerlinNoise anywhere in the generation pipeline.
    ///
    /// Reference: Marsaglia (2003), "Xorshift RNGs"
    /// Period: 2^64 - 1 (full period for any non-zero seed)
    /// </summary>
    public sealed class XORShift64
    {
        private ulong _state;

        /// <summary>
        /// Fallback seed used when seed=0 is provided (zero is the only invalid seed
        /// for xorshift, as it would produce an all-zero sequence).
        /// </summary>
        private const ulong DEFAULT_SEED = 13531446109741973463UL;

        /// <summary>
        /// Initialize the PRNG with a 64-bit seed.
        /// If seed is 0, a default non-zero seed is used instead.
        /// </summary>
        public XORShift64(ulong seed)
        {
            _state = seed != 0 ? seed : DEFAULT_SEED;
        }

        /// <summary>
        /// Returns the current internal state. Useful for save/restore of PRNG position.
        /// </summary>
        public ulong State => _state;

        /// <summary>
        /// Generate the next 64-bit unsigned integer in the sequence.
        /// This is the core operation -- all other methods derive from this.
        /// </summary>
        public ulong Next()
        {
            ulong x = _state;
            x ^= x << 13;
            x ^= x >> 7;
            x ^= x << 17;
            _state = x;
            return x;
        }

        /// <summary>
        /// Generate a random integer in [min, max) using rejection sampling
        /// to avoid modulo bias.
        /// </summary>
        public int Range(int min, int max)
        {
            if (min >= max)
                return min;

            uint range = (uint)(max - min);

            // For power-of-two ranges, simple masking works without bias
            if ((range & (range - 1)) == 0)
            {
                return min + (int)(Next() & (range - 1));
            }

            // Rejection sampling to eliminate modulo bias
            // The threshold is the largest multiple of range that fits in 64 bits
            ulong threshold = (ulong.MaxValue - range + 1) % range;

            ulong value;
            do
            {
                value = Next();
            } while (value < threshold);

            return min + (int)(value % range);
        }

        /// <summary>
        /// Generate a random float in [min, max).
        /// Uses integer arithmetic only -- the division is the single
        /// floating-point operation, which is deterministic on all IEEE 754 platforms.
        /// </summary>
        public float RangeFloat(float min, float max)
        {
            // Convert to [0, 1) range using top 24 bits for float precision
            // (float has 23-bit mantissa + 1 implicit bit = 24 bits of precision)
            float t = (Next() >> 40) / (float)(1UL << 24);
            return min + t * (max - min);
        }

        /// <summary>
        /// Generate a random double in [0, 1) with higher precision than RangeFloat.
        /// </summary>
        public double NextDouble()
        {
            // Use top 53 bits for double precision (52-bit mantissa + 1 implicit)
            return (Next() >> 11) / (double)(1UL << 53);
        }

        /// <summary>
        /// Generate a random boolean with the given probability of being true.
        /// Default: 50% chance.
        /// </summary>
        public bool NextBool(float probability = 0.5f)
        {
            return RangeFloat(0f, 1f) < probability;
        }

        /// <summary>
        /// Shuffle an array in-place using Fisher-Yates algorithm.
        /// Deterministic given the same PRNG state.
        /// </summary>
        public void Shuffle<T>(T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = Range(0, i + 1);
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }

        /// <summary>
        /// Select a random element from an array.
        /// </summary>
        public T Choose<T>(T[] array)
        {
            if (array == null || array.Length == 0)
                throw new ArgumentException("Cannot choose from empty array");
            return array[Range(0, array.Length)];
        }

        /// <summary>
        /// Select an index based on weighted probabilities.
        /// Weights do not need to sum to 1 -- they are normalized internally.
        /// </summary>
        public int WeightedChoice(float[] weights)
        {
            if (weights == null || weights.Length == 0)
                throw new ArgumentException("Cannot choose from empty weights");

            float totalWeight = 0f;
            for (int i = 0; i < weights.Length; i++)
                totalWeight += weights[i];

            float roll = RangeFloat(0f, totalWeight);
            float cumulative = 0f;

            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative)
                    return i;
            }

            return weights.Length - 1;
        }

        /// <summary>
        /// Create a fork of this PRNG with a derived seed.
        /// Useful for generating independent sub-sequences (e.g., one per zone)
        /// without affecting the parent sequence.
        /// </summary>
        public XORShift64 Fork()
        {
            // XOR with golden ratio constant to ensure the child's initial state
            // differs from the parent's current state. Without this, Fork would
            // leave parent and child in the same state, producing identical sequences.
            ulong childSeed = Next() ^ 0x9E3779B97F4A7C15UL;
            if (childSeed == 0) childSeed = DEFAULT_SEED;
            return new XORShift64(childSeed);
        }
    }
}
