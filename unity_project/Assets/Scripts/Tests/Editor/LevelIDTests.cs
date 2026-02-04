using System;
using System.Collections.Generic;
using SixteenBit.Generative;

namespace SixteenBit.Tests
{
    /// <summary>
    /// Test suite for the LevelID encoder/decoder.
    /// Updated for the 10-era system (era 0-9) and difficulty 0-3.
    /// Validates round-trip encoding, parsing edge cases, format compliance,
    /// era/difficulty names, equality, and constructor validation.
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

            // Test 1: Round-trip encode/decode across all 10 eras
            {
                bool allPassed = true;
                for (int era = 0; era <= LevelID.MAX_ERA; era++)
                {
                    int difficulty = era % (LevelID.MAX_DIFFICULTY + 1);
                    ulong seed = 0xA1B2C3D4E5F6A7B8UL + (ulong)era;
                    var id = LevelID.Create(difficulty, era, seed);
                    string encoded = id.Encode();
                    bool ok = LevelID.TryParse(encoded, out LevelID decoded);

                    if (!ok ||
                        decoded.Version != id.Version ||
                        decoded.Difficulty != id.Difficulty ||
                        decoded.Era != id.Era ||
                        decoded.Seed != id.Seed)
                    {
                        allPassed = false;
                        messages.Add($"    DETAIL: Round-trip failed for era={era}, difficulty={difficulty}");
                    }
                }
                Assert(allPassed, "Round-trip: encode/decode succeeds for all 10 eras (0-9)");
            }

            // Test 2: Encode format compliance
            {
                var id = new LevelID(1, 3, 5, 0x0000000000000001UL);
                string encoded = id.Encode();
                Assert(encoded == "LVLID_1_3_5_0000000000000001",
                    $"Format: '{encoded}' (expected 'LVLID_1_3_5_0000000000000001')");
            }

            // Test 3: Encode preserves leading zeros in seed
            {
                var id = LevelID.Create(0, 0, 0x000000000000ABCDUL);
                string encoded = id.Encode();
                Assert(encoded.EndsWith("000000000000ABCD"),
                    $"Leading zeros preserved: '{encoded}'");
            }

            // Test 4: Parse valid ID with era values
            {
                bool ok = LevelID.TryParse("LVLID_1_3_7_FFFFFFFFFFFFFFFF", out LevelID result);
                Assert(ok, "Parse valid ID with max seed and era=7");
                Assert(result.Difficulty == 3, "Parsed difficulty = 3 (Extreme)");
                Assert(result.Era == 7, "Parsed era = 7 (Digital)");
                Assert(result.Seed == ulong.MaxValue, "Parsed seed = max ulong");
            }

            // Test 5: Parse valid IDs at era boundaries
            {
                bool okEra0 = LevelID.TryParse("LVLID_1_0_0_0000000000000001", out LevelID r0);
                bool okEra9 = LevelID.TryParse("LVLID_1_0_9_0000000000000001", out LevelID r9);
                Assert(okEra0 && r0.Era == 0, "Parse valid ID with era=0 (minimum)");
                Assert(okEra9 && r9.Era == 9, "Parse valid ID with era=9 (maximum)");
            }

            // Test 6: Reject invalid prefix
            {
                bool ok = LevelID.TryParse("WRONG_1_2_0_A1B2C3D4E5F6A7B8", out _);
                Assert(!ok, "Rejects invalid prefix");
            }

            // Test 7: Reject missing components
            {
                bool ok1 = LevelID.TryParse("LVLID_1_2", out _);
                bool ok2 = LevelID.TryParse("LVLID_1_2_0", out _);
                bool ok3 = LevelID.TryParse("", out _);
                bool ok4 = LevelID.TryParse(null, out _);
                Assert(!ok1 && !ok2 && !ok3 && !ok4, "Rejects incomplete/empty/null input");
            }

            // Test 8: Reject out-of-range difficulty (> 3)
            {
                bool ok = LevelID.TryParse("LVLID_1_4_0_A1B2C3D4E5F6A7B8", out _);
                Assert(!ok, "Rejects difficulty 4 (max is 3)");
            }

            // Test 9: Reject out-of-range era (> 9)
            {
                bool ok10 = LevelID.TryParse("LVLID_1_2_10_A1B2C3D4E5F6A7B8", out _);
                Assert(!ok10, "Rejects era > 9");
            }

            // Test 10: Reject invalid hex seed
            {
                bool ok = LevelID.TryParse("LVLID_1_2_0_ZZZZZZZZZZZZZZZZ", out _);
                Assert(!ok, "Rejects non-hex seed");
            }

            // Test 11: EraName returns correct names for all 10 eras
            {
                string[] expectedEraNames = {
                    "Stone Age", "Bronze Age", "Classical", "Medieval", "Renaissance",
                    "Industrial", "Modern", "Digital", "Spacefaring", "Transcendent"
                };

                bool allCorrect = true;
                for (int era = 0; era < expectedEraNames.Length; era++)
                {
                    string actual = LevelID.Create(0, era, 1).EraName;
                    if (actual != expectedEraNames[era])
                    {
                        allCorrect = false;
                        messages.Add($"    DETAIL: EraName mismatch for era={era}: got '{actual}', expected '{expectedEraNames[era]}'");
                    }
                }
                Assert(allCorrect, "EraName: all 10 eras return correct names");
            }

            // Test 12: DifficultyName returns correct names
            {
                Assert(LevelID.Create(0, 0, 1).DifficultyName == "Easy", "DifficultyName: Easy");
                Assert(LevelID.Create(1, 0, 1).DifficultyName == "Normal", "DifficultyName: Normal");
                Assert(LevelID.Create(2, 0, 1).DifficultyName == "Hard", "DifficultyName: Hard");
                Assert(LevelID.Create(3, 0, 1).DifficultyName == "Extreme", "DifficultyName: Extreme");
            }

            // Test 13: Equality comparison
            {
                var id1 = LevelID.Create(2, 5, 12345UL);
                var id2 = LevelID.Create(2, 5, 12345UL);
                var id3 = LevelID.Create(2, 5, 12346UL);
                var id4 = LevelID.Create(2, 6, 12345UL);

                Assert(id1 == id2, "Equal IDs compare as equal");
                Assert(id1 != id3, "Different seeds compare as not equal");
                Assert(id1 != id4, "Different eras compare as not equal");
                Assert(id1.GetHashCode() == id2.GetHashCode(), "Equal IDs have equal hash codes");
            }

            // Test 14: GenerateNew produces unique IDs
            {
                var seen = new HashSet<ulong>();
                bool allUnique = true;
                for (int i = 0; i < 100; i++)
                {
                    int era = i % (LevelID.MAX_ERA + 1);
                    int difficulty = i % (LevelID.MAX_DIFFICULTY + 1);
                    var id = LevelID.GenerateNew(difficulty, era);
                    if (!seen.Add(id.Seed))
                    {
                        allUnique = false;
                        break;
                    }
                }
                Assert(allUnique, "GenerateNew produces 100 unique seeds across eras");
            }

            // Test 15: Constructor rejects invalid values
            {
                bool threw = false;
                try { new LevelID(1, -1, 0, 1); } catch (ArgumentOutOfRangeException) { threw = true; }
                Assert(threw, "Constructor rejects negative difficulty");

                threw = false;
                try { new LevelID(1, 0, -1, 1); } catch (ArgumentOutOfRangeException) { threw = true; }
                Assert(threw, "Constructor rejects negative era");

                threw = false;
                try { new LevelID(-1, 0, 0, 1); } catch (ArgumentOutOfRangeException) { threw = true; }
                Assert(threw, "Constructor rejects negative version");

                threw = false;
                try { new LevelID(1, 4, 0, 1); } catch (ArgumentOutOfRangeException) { threw = true; }
                Assert(threw, "Constructor rejects difficulty > MAX_DIFFICULTY (3)");

                threw = false;
                try { new LevelID(1, 0, 10, 1); } catch (ArgumentOutOfRangeException) { threw = true; }
                Assert(threw, "Constructor rejects era > MAX_ERA (9)");
            }

            messages.Add($"\nResults: {passed} passed, {failed} failed");
            return (passed, failed, messages);
        }
    }
}
