using System;
using System.Collections.Generic;
using EpochBreaker.Generative;

namespace EpochBreaker.Tests
{
    /// <summary>
    /// Test suite for the XORShift64 PRNG.
    /// Validates determinism, distribution quality, and edge cases.
    ///
    /// These tests can run without Unity -- they use plain assertions.
    /// In a Unity project, wrap with [Test] attributes from NUnit.
    /// </summary>
    public static class XORShift64Tests
    {
        /// <summary>
        /// Run all PRNG tests. Returns (passed, failed, messages).
        /// </summary>
        public static (int passed, int failed, List<string> messages) RunAll()
        {
            var messages = new List<string>();
            int passed = 0;
            int failed = 0;

            void Assert(bool condition, string testName)
            {
                if (condition)
                {
                    passed++;
                    messages.Add($"  PASS: {testName}");
                }
                else
                {
                    failed++;
                    messages.Add($"  FAIL: {testName}");
                }
            }

            messages.Add("=== XORShift64 Tests ===");

            // Test 1: Same seed produces same sequence
            {
                var rng1 = new XORShift64(12345UL);
                var rng2 = new XORShift64(12345UL);
                bool match = true;
                for (int i = 0; i < 1000; i++)
                {
                    if (rng1.Next() != rng2.Next())
                    {
                        match = false;
                        break;
                    }
                }
                Assert(match, "Same seed produces identical sequence (1000 values)");
            }

            // Test 2: Different seeds produce different sequences
            {
                var rng1 = new XORShift64(12345UL);
                var rng2 = new XORShift64(12346UL);
                bool allSame = true;
                for (int i = 0; i < 100; i++)
                {
                    if (rng1.Next() != rng2.Next())
                    {
                        allSame = false;
                        break;
                    }
                }
                Assert(!allSame, "Different seeds produce different sequences");
            }

            // Test 3: Zero seed falls back to default
            {
                var rng = new XORShift64(0UL);
                ulong first = rng.Next();
                Assert(first != 0, "Zero seed produces non-zero output (default seed used)");
            }

            // Test 4: No zero values in first 10000 outputs
            {
                var rng = new XORShift64(42UL);
                bool hasZero = false;
                for (int i = 0; i < 10000; i++)
                {
                    if (rng.Next() == 0)
                    {
                        hasZero = true;
                        break;
                    }
                }
                Assert(!hasZero, "No zero values in first 10000 outputs");
            }

            // Test 5: Range produces values within bounds
            {
                var rng = new XORShift64(99UL);
                bool inBounds = true;
                for (int i = 0; i < 10000; i++)
                {
                    int val = rng.Range(5, 15);
                    if (val < 5 || val >= 15)
                    {
                        inBounds = false;
                        break;
                    }
                }
                Assert(inBounds, "Range(5, 15) always produces values in [5, 15)");
            }

            // Test 6: RangeFloat produces values within bounds
            {
                var rng = new XORShift64(77UL);
                bool inBounds = true;
                for (int i = 0; i < 10000; i++)
                {
                    float val = rng.RangeFloat(0.0f, 1.0f);
                    if (val < 0.0f || val >= 1.0f)
                    {
                        inBounds = false;
                        break;
                    }
                }
                Assert(inBounds, "RangeFloat(0, 1) always produces values in [0, 1)");
            }

            // Test 7: Range distribution -- all values in range should appear
            {
                var rng = new XORShift64(1111UL);
                var counts = new int[10];
                for (int i = 0; i < 100000; i++)
                {
                    int val = rng.Range(0, 10);
                    counts[val]++;
                }
                bool allPresent = true;
                bool reasonablyUniform = true;
                for (int i = 0; i < 10; i++)
                {
                    if (counts[i] == 0) allPresent = false;
                    // Each bucket should have roughly 10000 +/- 2000
                    if (counts[i] < 7000 || counts[i] > 13000) reasonablyUniform = false;
                }
                Assert(allPresent, "Range(0, 10) covers all values in 100K samples");
                Assert(reasonablyUniform, "Range(0, 10) distribution is reasonably uniform");
            }

            // Test 8: NextBool with probability 0.5 is roughly 50/50
            {
                var rng = new XORShift64(2222UL);
                int trueCount = 0;
                int total = 100000;
                for (int i = 0; i < total; i++)
                {
                    if (rng.NextBool(0.5f)) trueCount++;
                }
                double ratio = (double)trueCount / total;
                Assert(ratio > 0.48 && ratio < 0.52, $"NextBool(0.5) ratio: {ratio:F4} (expected ~0.5)");
            }

            // Test 9: Shuffle produces valid permutation
            {
                var rng = new XORShift64(3333UL);
                int[] arr = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                rng.Shuffle(arr);

                var seen = new HashSet<int>();
                bool valid = true;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i] < 0 || arr[i] > 9 || !seen.Add(arr[i]))
                    {
                        valid = false;
                        break;
                    }
                }
                Assert(valid, "Shuffle produces valid permutation (all elements present, no duplicates)");
            }

            // Test 10: Fork produces independent sequence
            {
                var parent = new XORShift64(4444UL);
                ulong parentVal1 = parent.Next();
                var child = parent.Fork();
                ulong parentVal2 = parent.Next();
                ulong childVal1 = child.Next();

                // Child should not produce same values as parent's continued sequence
                Assert(childVal1 != parentVal2, "Fork produces independent sequence from parent");
            }

            // Test 11: WeightedChoice respects weights
            {
                var rng = new XORShift64(5555UL);
                float[] weights = { 0.1f, 0.9f };
                int[] counts = new int[2];
                for (int i = 0; i < 100000; i++)
                {
                    counts[rng.WeightedChoice(weights)]++;
                }
                double ratio = (double)counts[1] / (counts[0] + counts[1]);
                Assert(ratio > 0.85 && ratio < 0.95,
                    $"WeightedChoice(0.1, 0.9) ratio for index 1: {ratio:F4} (expected ~0.9)");
            }

            // Test 12: State can be saved and restored
            {
                var rng = new XORShift64(6666UL);
                for (int i = 0; i < 50; i++) rng.Next(); // advance
                ulong savedState = rng.State;

                // Continue generating
                ulong val1 = rng.Next();
                ulong val2 = rng.Next();

                // Create new RNG from saved state
                var rng2 = new XORShift64(savedState);
                ulong val1b = rng2.Next();
                ulong val2b = rng2.Next();

                Assert(val1 == val1b && val2 == val2b,
                    "State save/restore produces identical continuation");
            }

            // Test 13: Known output values for regression testing
            // These are the canonical expected values for seed=1
            {
                var rng = new XORShift64(1UL);
                ulong v1 = rng.Next();
                ulong v2 = rng.Next();
                ulong v3 = rng.Next();

                // Verify the xorshift64 algorithm:
                // seed=1: x=1, x^=(1<<13)=8193, x^=(8193>>7)=8257, x^=(8257<<17)=0x40822041=1082269761
                Assert(v1 == 1082269761UL,
                    $"Known value check: seed=1, first output = {v1} (expected 1082269761)");

                // Record v2 and v3 for future regression
                messages.Add($"  INFO: seed=1 values: v1={v1}, v2={v2}, v3={v3}");
            }

            messages.Add($"\nResults: {passed} passed, {failed} failed");
            return (passed, failed, messages);
        }
    }
}
