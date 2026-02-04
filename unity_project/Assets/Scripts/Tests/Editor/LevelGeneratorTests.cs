using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SixteenBit.Generative;

namespace SixteenBit.Tests
{
    /// <summary>
    /// Test suite for the level generation pipeline.
    /// Updated for the 10-era system with destructible environments and weapon drops.
    ///
    /// Validates determinism, reachability, difficulty, performance, and new features
    /// including weapon progression, era-specific enemies, and destructible tiles.
    ///
    /// Mapped to test cases from Validation-QA-Suite.md:
    ///   TC-001: Deterministic reconstruction (100 seeds, difficulty 0-3, era 0-9)
    ///   TC-002: Cross-run determinism (different seeds produce different levels)
    ///   TC-003: Reachability (target 95%+)
    ///   TC-004: Difficulty within tolerance
    ///   TC-005: Performance (avg < 200ms, max < 500ms)
    ///   TC-006: Entity bounds
    ///   TC-007: No impossible gaps
    ///   TC-008: Minimum checkpoints
    ///   TC-009: Metadata consistency (including WeaponDrops and DestructibleTiles)
    ///   TC-010: Full validation pass rate (75%+)
    ///   TC-011: Weapon drops exist in generated levels
    ///   TC-012: Era-specific enemy types
    ///   TC-013: Destructible tiles present in generated levels
    /// </summary>
    public static class LevelGeneratorTests
    {
        private static readonly LevelGenerator Generator = new LevelGenerator();
        private static readonly LevelValidator Validator = new LevelValidator();

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

            messages.Add("=== Level Generator Tests ===");

            // ------------------------------------------------------------------
            // TC-001: Deterministic reconstruction
            // Same LevelID must produce byte-identical levels every time.
            // Tests across all 4 difficulty levels (0-3) and all 10 eras (0-9).
            // ------------------------------------------------------------------
            {
                int deterministicCount = 0;
                int testCount = 100;

                for (int i = 0; i < testCount; i++)
                {
                    ulong seed = (ulong)(i + 1) * 7919UL; // distinct seeds
                    int difficulty = i % (LevelID.MAX_DIFFICULTY + 1); // 0-3
                    int era = i % (LevelID.MAX_ERA + 1);              // 0-9
                    var id = LevelID.Create(difficulty, era, seed);

                    var level1 = Generator.Generate(id);
                    var level2 = Generator.Generate(id);

                    ulong hash1 = level1.ComputeHash();
                    ulong hash2 = level2.ComputeHash();

                    if (hash1 == hash2)
                        deterministicCount++;
                }

                Assert(deterministicCount == testCount,
                    $"TC-001: Determinism - {deterministicCount}/{testCount} seeds produced identical levels");
            }

            // ------------------------------------------------------------------
            // TC-002: Different seeds produce different levels
            // ------------------------------------------------------------------
            {
                var id1 = LevelID.Create(2, 3, 12345UL);
                var id2 = LevelID.Create(2, 3, 12346UL);
                var level1 = Generator.Generate(id1);
                var level2 = Generator.Generate(id2);

                ulong hash1 = level1.ComputeHash();
                ulong hash2 = level2.ComputeHash();

                Assert(hash1 != hash2, "TC-002: Different seeds produce different levels");
            }

            // ------------------------------------------------------------------
            // TC-003: Reachability validation
            // Target: 95%+ of generated levels must be completable.
            // ------------------------------------------------------------------
            {
                int reachableCount = 0;
                int testCount = 50;

                for (int i = 0; i < testCount; i++)
                {
                    ulong seed = (ulong)(i + 1) * 31337UL;
                    int difficulty = i % (LevelID.MAX_DIFFICULTY + 1);
                    int era = i % (LevelID.MAX_ERA + 1);
                    var id = LevelID.Create(difficulty, era, seed);
                    var level = Generator.Generate(id);
                    var result = Validator.Validate(level);

                    if (result.Reachable)
                        reachableCount++;
                    else
                        messages.Add($"    WARN: Unreachable level seed={seed}, diff={difficulty}, era={era}");
                }

                float reachableRatio = (float)reachableCount / testCount;
                Assert(reachableRatio >= 0.95f,
                    $"TC-003: Reachability - {reachableCount}/{testCount} levels reachable ({reachableRatio:P0})");
            }

            // ------------------------------------------------------------------
            // TC-004: Difficulty within tolerance
            // ------------------------------------------------------------------
            {
                int withinToleranceCount = 0;
                int testCount = 50;

                for (int i = 0; i < testCount; i++)
                {
                    ulong seed = (ulong)(i + 1) * 65537UL;
                    int difficulty = i % (LevelID.MAX_DIFFICULTY + 1);
                    int era = i % (LevelID.MAX_ERA + 1);
                    var id = LevelID.Create(difficulty, era, seed);
                    var level = Generator.Generate(id);
                    var result = Validator.Validate(level);

                    if (result.DifficultyWithinTolerance)
                        withinToleranceCount++;
                }

                float ratio = (float)withinToleranceCount / testCount;
                Assert(ratio >= 0.75f,
                    $"TC-004: Difficulty balance - {withinToleranceCount}/{testCount} within tolerance ({ratio:P0})");
            }

            // ------------------------------------------------------------------
            // TC-005: Performance (generation time)
            // Target: avg < 200ms, max < 500ms on development machine.
            // ------------------------------------------------------------------
            {
                int testCount = 20;
                long totalMs = 0;
                long maxMs = 0;

                for (int i = 0; i < testCount; i++)
                {
                    ulong seed = (ulong)(i + 1) * 104729UL;
                    int difficulty = i % (LevelID.MAX_DIFFICULTY + 1);
                    int era = i % (LevelID.MAX_ERA + 1);
                    var id = LevelID.Create(difficulty, era, seed);

                    var sw = Stopwatch.StartNew();
                    Generator.Generate(id);
                    sw.Stop();

                    long elapsed = sw.ElapsedMilliseconds;
                    totalMs += elapsed;
                    if (elapsed > maxMs) maxMs = elapsed;
                }

                long avgMs = totalMs / testCount;
                messages.Add($"  INFO: Generation avg={avgMs}ms, max={maxMs}ms over {testCount} levels");

                Assert(avgMs < 200,
                    $"TC-005: Performance - avg generation time {avgMs}ms (target < 200ms)");
                Assert(maxMs < 500,
                    $"TC-005: Performance - max generation time {maxMs}ms (target < 500ms)");
            }

            // ------------------------------------------------------------------
            // TC-006: Entity bounds
            // All enemies, rewards, checkpoints must be within level bounds.
            // ------------------------------------------------------------------
            {
                int inBoundsCount = 0;
                int testCount = 50;

                for (int i = 0; i < testCount; i++)
                {
                    ulong seed = (ulong)(i + 1) * 48271UL;
                    int difficulty = i % (LevelID.MAX_DIFFICULTY + 1);
                    int era = i % (LevelID.MAX_ERA + 1);
                    var id = LevelID.Create(difficulty, era, seed);
                    var level = Generator.Generate(id);
                    var result = Validator.Validate(level);

                    if (result.EntitiesInBounds)
                        inBoundsCount++;
                }

                Assert(inBoundsCount == testCount,
                    $"TC-006: Entity bounds - {inBoundsCount}/{testCount} levels have all entities in bounds");
            }

            // ------------------------------------------------------------------
            // TC-007: No impossible gaps
            // ------------------------------------------------------------------
            {
                int noImpossibleGapsCount = 0;
                int testCount = 50;

                for (int i = 0; i < testCount; i++)
                {
                    ulong seed = (ulong)(i + 1) * 16127UL;
                    int difficulty = i % (LevelID.MAX_DIFFICULTY + 1);
                    int era = i % (LevelID.MAX_ERA + 1);
                    var id = LevelID.Create(difficulty, era, seed);
                    var level = Generator.Generate(id);
                    var result = Validator.Validate(level);

                    if (result.NoImpossibleGaps)
                        noImpossibleGapsCount++;
                }

                float ratio = (float)noImpossibleGapsCount / testCount;
                Assert(ratio >= 0.95f,
                    $"TC-007: No impossible gaps - {noImpossibleGapsCount}/{testCount} levels ({ratio:P0})");
            }

            // ------------------------------------------------------------------
            // TC-008: Minimum checkpoints
            // ------------------------------------------------------------------
            {
                int hasMinCheckpointsCount = 0;
                int testCount = 50;

                for (int i = 0; i < testCount; i++)
                {
                    ulong seed = (ulong)(i + 1) * 99991UL;
                    int difficulty = i % (LevelID.MAX_DIFFICULTY + 1);
                    int era = i % (LevelID.MAX_ERA + 1);
                    var id = LevelID.Create(difficulty, era, seed);
                    var level = Generator.Generate(id);
                    var result = Validator.Validate(level);

                    if (result.HasMinCheckpoints)
                        hasMinCheckpointsCount++;
                }

                Assert(hasMinCheckpointsCount == testCount,
                    $"TC-008: Minimum checkpoints - {hasMinCheckpointsCount}/{testCount} levels have >= {LevelValidator.MIN_CHECKPOINTS}");
            }

            // ------------------------------------------------------------------
            // TC-009: Level metadata consistency
            // Includes WeaponDrops count and DestructibleTiles count.
            // ------------------------------------------------------------------
            {
                var id = LevelID.Create(2, 4, 55555UL);
                var level = Generator.Generate(id);

                Assert(level.Metadata.TotalEnemies == level.Enemies.Count,
                    "TC-009a: Metadata enemy count matches list count");
                Assert(level.Metadata.TotalRewards == level.Rewards.Count,
                    "TC-009b: Metadata reward count matches list count");
                Assert(level.Metadata.TotalCheckpoints == level.Checkpoints.Count,
                    "TC-009c: Metadata checkpoint count matches list count");
                Assert(level.Layout.WidthTiles > 0 && level.Layout.HeightTiles > 0,
                    "TC-009d: Level has positive dimensions");
                Assert(level.Layout.Tiles.Length == level.Layout.WidthTiles * level.Layout.HeightTiles,
                    "TC-009e: Tile array size matches dimensions");

                // Weapon drops metadata consistency
                int actualWeaponDrops = level.WeaponDrops != null ? level.WeaponDrops.Count : 0;
                Assert(level.Metadata.TotalWeaponDrops == actualWeaponDrops,
                    $"TC-009f: Metadata weapon drop count ({level.Metadata.TotalWeaponDrops}) matches list count ({actualWeaponDrops})");

                // Destructible tiles metadata consistency
                int actualDestructibles = 0;
                if (level.Layout.Destructibles != null)
                {
                    for (int i = 0; i < level.Layout.Destructibles.Length; i++)
                    {
                        if (level.Layout.Destructibles[i].MaterialClass > 0)
                            actualDestructibles++;
                    }
                }
                Assert(level.Metadata.TotalDestructibleTiles == actualDestructibles,
                    $"TC-009g: Metadata destructible tile count ({level.Metadata.TotalDestructibleTiles}) matches actual count ({actualDestructibles})");
            }

            // ------------------------------------------------------------------
            // TC-010: Full validation pass rate
            // Target: >= 75% pass rate.
            // ------------------------------------------------------------------
            {
                int passCount = 0;
                int testCount = 100;

                for (int i = 0; i < testCount; i++)
                {
                    ulong seed = (ulong)(i + 1) * 257UL;
                    int difficulty = i % (LevelID.MAX_DIFFICULTY + 1);
                    int era = i % (LevelID.MAX_ERA + 1);
                    var id = LevelID.Create(difficulty, era, seed);
                    var level = Generator.Generate(id);
                    var result = Validator.Validate(level);

                    if (result.Passed)
                        passCount++;
                }

                float ratio = (float)passCount / testCount;
                messages.Add($"  INFO: Full validation pass rate: {passCount}/{testCount} ({ratio:P0})");

                Assert(ratio >= 0.75f,
                    $"TC-010: Full validation - {passCount}/{testCount} levels pass all checks ({ratio:P0})");
            }

            // ------------------------------------------------------------------
            // TC-011: Weapon drops exist in generated levels
            // Levels should contain weapon drops for the weapon progression system.
            // ------------------------------------------------------------------
            {
                int levelsWithWeapons = 0;
                int testCount = 30;

                for (int i = 0; i < testCount; i++)
                {
                    ulong seed = (ulong)(i + 1) * 54321UL;
                    int difficulty = i % (LevelID.MAX_DIFFICULTY + 1);
                    int era = i % (LevelID.MAX_ERA + 1);
                    var id = LevelID.Create(difficulty, era, seed);
                    var level = Generator.Generate(id);

                    if (level.WeaponDrops != null && level.WeaponDrops.Count > 0)
                        levelsWithWeapons++;
                }

                float ratio = (float)levelsWithWeapons / testCount;
                messages.Add($"  INFO: Weapon drops present in {levelsWithWeapons}/{testCount} levels ({ratio:P0})");

                Assert(levelsWithWeapons > 0,
                    $"TC-011: Weapon drops - {levelsWithWeapons}/{testCount} levels contain weapon drops");
            }

            // ------------------------------------------------------------------
            // TC-012: Era-specific enemy types
            // Each era has 3 enemy types encoded as era*3 to era*3+2.
            // Enemies in a level for a given era should fall within that range.
            // ------------------------------------------------------------------
            {
                int eraCorrectCount = 0;
                int testCount = 30;

                for (int i = 0; i < testCount; i++)
                {
                    ulong seed = (ulong)(i + 1) * 77777UL;
                    int era = i % (LevelID.MAX_ERA + 1);
                    int difficulty = i % (LevelID.MAX_DIFFICULTY + 1);
                    var id = LevelID.Create(difficulty, era, seed);
                    var level = Generator.Generate(id);

                    int expectedMin = era * 3;
                    int expectedMax = era * 3 + 2;
                    bool allCorrect = true;

                    for (int e = 0; e < level.Enemies.Count; e++)
                    {
                        int enemyTypeValue = (int)level.Enemies[e].Type;
                        if (enemyTypeValue < expectedMin || enemyTypeValue > expectedMax)
                        {
                            allCorrect = false;
                            messages.Add($"    WARN: Era {era} enemy type {enemyTypeValue} outside range [{expectedMin}, {expectedMax}]");
                            break;
                        }
                    }

                    if (allCorrect)
                        eraCorrectCount++;
                }

                float ratio = (float)eraCorrectCount / testCount;
                Assert(ratio >= 0.90f,
                    $"TC-012: Era-specific enemies - {eraCorrectCount}/{testCount} levels have correct enemy types ({ratio:P0})");
            }

            // ------------------------------------------------------------------
            // TC-013: Destructible tiles present in generated levels
            // Levels should contain destructible tiles as part of the
            // destructible environment system.
            // ------------------------------------------------------------------
            {
                int levelsWithDestructibles = 0;
                int testCount = 30;

                for (int i = 0; i < testCount; i++)
                {
                    ulong seed = (ulong)(i + 1) * 13579UL;
                    int difficulty = i % (LevelID.MAX_DIFFICULTY + 1);
                    int era = i % (LevelID.MAX_ERA + 1);
                    var id = LevelID.Create(difficulty, era, seed);
                    var level = Generator.Generate(id);

                    bool hasDestructibles = false;
                    if (level.Layout.Destructibles != null)
                    {
                        for (int d = 0; d < level.Layout.Destructibles.Length; d++)
                        {
                            if (level.Layout.Destructibles[d].MaterialClass > 0)
                            {
                                hasDestructibles = true;
                                break;
                            }
                        }
                    }

                    if (hasDestructibles)
                        levelsWithDestructibles++;
                }

                float ratio = (float)levelsWithDestructibles / testCount;
                messages.Add($"  INFO: Destructible tiles present in {levelsWithDestructibles}/{testCount} levels ({ratio:P0})");

                Assert(levelsWithDestructibles > 0,
                    $"TC-013: Destructible tiles - {levelsWithDestructibles}/{testCount} levels contain destructible tiles");
            }

            messages.Add($"\nResults: {passed} passed, {failed} failed");
            return (passed, failed, messages);
        }
    }
}
