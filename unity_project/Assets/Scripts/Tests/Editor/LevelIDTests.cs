using System;
using System.Collections.Generic;
using EpochBreaker.Generative;

namespace EpochBreaker.Tests
{
    /// <summary>
    /// Test suite for the LevelID encoder/decoder.
    /// Updated for epoch-based system (epoch 0-9) with seed-derived parameters.
    /// Format: E-XXXXXXXX where E is epoch (0-9) and XXXXXXXX is 8 base32 characters.
    /// </summary>
    public static class LevelIDTests
    {
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

            messages.Add("=== LevelID Tests ===");

            // Test 1: Round-trip encode/decode across all 10 epochs
            {
                bool allPassed = true;
                for (int epoch = 0; epoch <= LevelID.MAX_EPOCH; epoch++)
                {
                    ulong seed = 0xA1B2C3D4E5UL + (ulong)epoch;
                    var id = LevelID.Create(epoch, seed);
                    string encoded = id.ToCode();
                    bool ok = LevelID.TryParse(encoded, out LevelID decoded);

                    if (!ok ||
                        decoded.Epoch != id.Epoch ||
                        decoded.Seed != id.Seed)
                    {
                        allPassed = false;
                        messages.Add($"    DETAIL: Round-trip failed for epoch={epoch}");
                    }
                }
                Assert(allPassed, "Round-trip: encode/decode succeeds for all 10 epochs (0-9)");
            }

            // Test 2: Code format compliance (E-XXXXXXXX)
            {
                var id = LevelID.Create(3, 0x0000000001UL);
                string code = id.ToCode();
                Assert(code.Length == 10 && code[1] == '-' && code[0] == '3',
                    $"Format: '{code}' starts with epoch and hyphen");
            }

            // Test 3: Seed masked to 40 bits
            {
                var id = LevelID.Create(0, 0xFFFFFFFFFFFFFFFFUL);
                Assert(id.Seed == 0xFFFFFFFFFFUL, "Seed masked to 40 bits (max value)");
            }

            // Test 4: Parse valid code
            {
                // Create a known ID and parse its code
                var original = LevelID.Create(7, 0x123456789AUL);
                string code = original.ToCode();
                bool ok = LevelID.TryParse(code, out LevelID result);
                Assert(ok, $"Parse valid code: {code}");
                Assert(result.Epoch == 7, "Parsed epoch = 7 (Digital)");
                Assert(result.Seed == original.Seed, "Parsed seed matches original");
            }

            // Test 5: Parse valid codes at epoch boundaries
            {
                var id0 = LevelID.Create(0, 1);
                var id9 = LevelID.Create(9, 1);
                bool okEpoch0 = LevelID.TryParse(id0.ToCode(), out LevelID r0);
                bool okEpoch9 = LevelID.TryParse(id9.ToCode(), out LevelID r9);
                Assert(okEpoch0 && r0.Epoch == 0, "Parse valid code with epoch=0 (minimum)");
                Assert(okEpoch9 && r9.Epoch == 9, "Parse valid code with epoch=9 (maximum)");
            }

            // Test 6: Reject invalid format
            {
                bool ok1 = LevelID.TryParse("INVALID", out _);
                bool ok2 = LevelID.TryParse("A-12345678", out _); // Invalid epoch character
                Assert(!ok1 && !ok2, "Rejects invalid format");
            }

            // Test 7: Reject incomplete/empty/null input
            {
                bool ok1 = LevelID.TryParse("3-ABC", out _); // Too short
                bool ok2 = LevelID.TryParse("", out _);
                bool ok3 = LevelID.TryParse(null, out _);
                Assert(!ok1 && !ok2 && !ok3, "Rejects incomplete/empty/null input");
            }

            // Test 8: Reject invalid base32 characters
            {
                bool ok = LevelID.TryParse("3-IIIIIIII", out _); // 'I' is not in base32 alphabet
                Assert(!ok, "Rejects invalid base32 characters (I, O, Y, Z not allowed)");
            }

            // Test 9: EpochName returns correct names for all 10 epochs
            {
                string[] expectedEpochNames = {
                    "Stone Age", "Bronze Age", "Classical", "Medieval", "Renaissance",
                    "Industrial", "Modern", "Digital", "Spacefaring", "Transcendent"
                };

                bool allCorrect = true;
                for (int epoch = 0; epoch < expectedEpochNames.Length; epoch++)
                {
                    string actual = LevelID.Create(epoch, 1).EpochName;
                    if (actual != expectedEpochNames[epoch])
                    {
                        allCorrect = false;
                        messages.Add($"    DETAIL: EpochName mismatch for epoch={epoch}: got '{actual}', expected '{expectedEpochNames[epoch]}'");
                    }
                }
                Assert(allCorrect, "EpochName: all 10 epochs return correct names");
            }

            // Test 10: DerivedIntensity is in valid range (0.0-1.0)
            {
                bool allValid = true;
                for (int i = 0; i < 100; i++)
                {
                    var id = LevelID.GenerateRandom(i % 10);
                    if (id.DerivedIntensity < 0f || id.DerivedIntensity > 1f)
                    {
                        allValid = false;
                        messages.Add($"    DETAIL: DerivedIntensity out of range: {id.DerivedIntensity}");
                        break;
                    }
                }
                Assert(allValid, "DerivedIntensity: always in range [0.0, 1.0]");
            }

            // Test 11: DerivedLengthMod is in valid range (0.8-1.2)
            {
                bool allValid = true;
                for (int i = 0; i < 100; i++)
                {
                    var id = LevelID.GenerateRandom(i % 10);
                    if (id.DerivedLengthMod < 0.8f || id.DerivedLengthMod > 1.2f)
                    {
                        allValid = false;
                        messages.Add($"    DETAIL: DerivedLengthMod out of range: {id.DerivedLengthMod}");
                        break;
                    }
                }
                Assert(allValid, "DerivedLengthMod: always in range [0.8, 1.2]");
            }

            // Test 12: Equality comparison
            {
                var id1 = LevelID.Create(5, 12345UL);
                var id2 = LevelID.Create(5, 12345UL);
                var id3 = LevelID.Create(5, 12346UL);
                var id4 = LevelID.Create(6, 12345UL);

                Assert(id1 == id2, "Equal IDs compare as equal");
                Assert(id1 != id3, "Different seeds compare as not equal");
                Assert(id1 != id4, "Different epochs compare as not equal");
                Assert(id1.GetHashCode() == id2.GetHashCode(), "Equal IDs have equal hash codes");
            }

            // Test 13: GenerateRandom produces unique IDs
            {
                var seen = new HashSet<ulong>();
                bool allUnique = true;
                for (int i = 0; i < 100; i++)
                {
                    int epoch = i % (LevelID.MAX_EPOCH + 1);
                    var id = LevelID.GenerateRandom(epoch);
                    if (!seen.Add(id.Seed))
                    {
                        allUnique = false;
                        break;
                    }
                }
                Assert(allUnique, "GenerateRandom produces 100 unique seeds across epochs");
            }

            // Test 14: Next() produces deterministic sequence
            {
                var id1 = LevelID.Create(0, 12345UL);
                var next1 = id1.Next();
                var next2 = id1.Next();
                Assert(next1 == next2, "Next() is deterministic (same input = same output)");
                Assert(next1.Epoch == 1, "Next() advances epoch by 1");
                Assert(next1.Seed != id1.Seed, "Next() changes seed");
            }

            // Test 15: Next() wraps epoch at MAX_EPOCH
            {
                var id = LevelID.Create(9, 12345UL);
                var next = id.Next();
                Assert(next.Epoch == 0, "Next() wraps epoch from 9 to 0");
            }

            // Test 16: Constructor rejects invalid epoch
            {
                bool threw = false;
                try { new LevelID(-1, 1); } catch (ArgumentOutOfRangeException) { threw = true; }
                Assert(threw, "Constructor rejects negative epoch");

                threw = false;
                try { new LevelID(10, 1); } catch (ArgumentOutOfRangeException) { threw = true; }
                Assert(threw, "Constructor rejects epoch > MAX_EPOCH (9)");
            }

            // Test 17: Case insensitive parsing
            {
                var id = LevelID.Create(3, 0xABCDEF1234UL);
                string upperCode = id.ToCode();
                string lowerCode = upperCode.ToLowerInvariant();
                bool okUpper = LevelID.TryParse(upperCode, out LevelID r1);
                bool okLower = LevelID.TryParse(lowerCode, out LevelID r2);
                Assert(okUpper && okLower && r1 == r2, "Parsing is case insensitive");
            }

            messages.Add($"\nResults: {passed} passed, {failed} failed");
            return (passed, failed, messages);
        }
    }
}
