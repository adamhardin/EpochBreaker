#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using EpochBreaker.Generative;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace EpochBreaker.Editor
{
    /// <summary>
    /// Sprint 9: Automated level validation tool.
    /// Generates 1000 deterministic seeds across all 10 epochs and validates each
    /// through the LevelValidator pipeline. Reports per-epoch pass rates, overall
    /// reachability, failure categories, and generation performance.
    ///
    /// Accessible via menu: Epoch Breaker > QA > Validate 1000 Seeds.
    /// </summary>
    public static class LevelValidator1000
    {
        private const int TOTAL_SEEDS = 1000;
        private const int EPOCHS = 10; // 0-9
        private const int SEEDS_PER_EPOCH = TOTAL_SEEDS / EPOCHS; // 100

        [MenuItem("Epoch Breaker/QA/Validate 1000 Seeds", priority = 32)]
        public static void RunValidation()
        {
            var sw = Stopwatch.StartNew();
            var generator = new LevelGenerator();
            var validator = new LevelValidator();

            // Per-epoch tracking
            int[] epochTotal = new int[EPOCHS];
            int[] epochPassed = new int[EPOCHS];
            int[] epochReachable = new int[EPOCHS];

            // Failure category tracking
            int failStartAccess = 0;
            int failGoalAccess = 0;
            int failReachable = 0;
            int failWeaponProg = 0;
            int failDestructPaths = 0;
            int failDifficulty = 0;
            int failGaps = 0;
            int failCheckpoints = 0;
            int failEntityBounds = 0;
            int failCascade = 0;

            // Performance tracking
            long totalGenMs = 0;
            long maxGenMs = 0;
            long totalValMs = 0;

            int totalPassed = 0;
            int totalReachable = 0;
            var failedSeeds = new List<string>();

            for (int epoch = 0; epoch < EPOCHS; epoch++)
            {
                for (int i = 0; i < SEEDS_PER_EPOCH; i++)
                {
                    // Deterministic seed generation: epoch * large prime + i * another prime
                    ulong seed = ((ulong)epoch * 1000003UL + (ulong)i * 7919UL + 42UL) & 0xFFFFFFFFFFUL;
                    if (seed == 0) seed = 1;
                    var id = LevelID.Create(epoch, seed);

                    epochTotal[epoch]++;

                    // Generate
                    var genSw = Stopwatch.StartNew();
                    LevelData level;
                    try
                    {
                        level = generator.Generate(id);
                    }
                    catch (Exception ex)
                    {
                        failedSeeds.Add($"  CRASH: {id.ToCode()} - {ex.Message}");
                        continue;
                    }
                    genSw.Stop();
                    long genMs = genSw.ElapsedMilliseconds;
                    totalGenMs += genMs;
                    if (genMs > maxGenMs) maxGenMs = genMs;

                    // Validate
                    var valSw = Stopwatch.StartNew();
                    ValidationResult result;
                    try
                    {
                        result = validator.Validate(level);
                    }
                    catch (Exception ex)
                    {
                        failedSeeds.Add($"  VAL-CRASH: {id.ToCode()} - {ex.Message}");
                        continue;
                    }
                    valSw.Stop();
                    totalValMs += valSw.ElapsedMilliseconds;

                    if (result.Reachable)
                    {
                        totalReachable++;
                        epochReachable[epoch]++;
                    }

                    if (result.Passed)
                    {
                        totalPassed++;
                        epochPassed[epoch]++;
                    }
                    else
                    {
                        // Categorize failures
                        if (!result.StartAccessible) failStartAccess++;
                        if (!result.ExitPortalAccessible) failGoalAccess++;
                        if (!result.Reachable) failReachable++;
                        if (!result.WeaponProgressionValid) failWeaponProg++;
                        if (!result.DestructionPathsValid) failDestructPaths++;
                        if (!result.DifficultyWithinTolerance) failDifficulty++;
                        if (!result.NoImpossibleGaps) failGaps++;
                        if (!result.HasMinCheckpoints) failCheckpoints++;
                        if (!result.EntitiesInBounds) failEntityBounds++;
                        if (!result.StructuralCascadeValid) failCascade++;

                        if (failedSeeds.Count < 20) // Cap logged failures
                            failedSeeds.Add($"  FAIL: {id.ToCode()} - {FailureReasons(result)}");
                    }

                    // Progress bar every 100 seeds
                    int progress = epoch * SEEDS_PER_EPOCH + i + 1;
                    if (progress % 100 == 0)
                    {
                        EditorUtility.DisplayProgressBar(
                            "Validating Levels",
                            $"Validated {progress}/{TOTAL_SEEDS} seeds...",
                            (float)progress / TOTAL_SEEDS);
                    }
                }
            }

            sw.Stop();
            EditorUtility.ClearProgressBar();

            // Build report
            var report = new List<string>();
            report.Add("========================================================");
            report.Add("  Epoch Breaker - 1000 Seed Validation Report (Sprint 9)");
            report.Add($"  {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            report.Add("========================================================");
            report.Add("");
            report.Add($"Total seeds tested:  {TOTAL_SEEDS}");
            report.Add($"Overall pass rate:   {totalPassed}/{TOTAL_SEEDS} ({100f * totalPassed / TOTAL_SEEDS:F1}%)");
            report.Add($"Reachability rate:   {totalReachable}/{TOTAL_SEEDS} ({100f * totalReachable / TOTAL_SEEDS:F1}%)");
            report.Add($"Total time:          {sw.ElapsedMilliseconds}ms");
            report.Add($"Avg gen time:        {totalGenMs / TOTAL_SEEDS}ms");
            report.Add($"Max gen time:        {maxGenMs}ms");
            report.Add($"Avg val time:        {totalValMs / TOTAL_SEEDS}ms");
            report.Add("");

            report.Add("--- Per-Epoch Results ---");
            for (int e = 0; e < EPOCHS; e++)
            {
                string epochName = LevelID.GetEpochName(e);
                float passRate = epochTotal[e] > 0 ? 100f * epochPassed[e] / epochTotal[e] : 0f;
                float reachRate = epochTotal[e] > 0 ? 100f * epochReachable[e] / epochTotal[e] : 0f;
                report.Add($"  Epoch {e} ({epochName,-14}): pass={epochPassed[e]}/{epochTotal[e]} ({passRate:F0}%), reachable={epochReachable[e]}/{epochTotal[e]} ({reachRate:F0}%)");
            }
            report.Add("");

            report.Add("--- Failure Categories ---");
            report.Add($"  Start inaccessible:     {failStartAccess}");
            report.Add($"  Goal inaccessible:      {failGoalAccess}");
            report.Add($"  Unreachable:            {failReachable}");
            report.Add($"  Weapon progression:     {failWeaponProg}");
            report.Add($"  Destruction paths:      {failDestructPaths}");
            report.Add($"  Difficulty tolerance:    {failDifficulty}");
            report.Add($"  Impossible gaps:        {failGaps}");
            report.Add($"  Insufficient checkpts:  {failCheckpoints}");
            report.Add($"  Entity out of bounds:   {failEntityBounds}");
            report.Add($"  Cascade depth:          {failCascade}");
            report.Add("");

            if (failedSeeds.Count > 0)
            {
                report.Add("--- Sample Failed Seeds (first 20) ---");
                foreach (var line in failedSeeds)
                    report.Add(line);
                report.Add("");
            }

            report.Add("========================================================");
            string verdict = totalReachable >= 950
                ? "REACHABILITY TARGET MET (>= 95%)"
                : $"REACHABILITY BELOW TARGET ({100f * totalReachable / TOTAL_SEEDS:F1}% < 95%)";
            report.Add(verdict);
            report.Add("========================================================");

            // Output
            foreach (var line in report)
            {
                if (line.Contains("FAIL") || line.Contains("CRASH") || line.Contains("BELOW TARGET"))
                    Debug.LogError(line);
                else if (line.Contains("WARN"))
                    Debug.LogWarning(line);
                else
                    Debug.Log(line);
            }

            // Save summary to EditorPrefs for QA window
            string summary = $"1000-Seed: Pass={totalPassed}, Reachable={totalReachable}, Time={sw.ElapsedMilliseconds}ms";
            EditorPrefs.SetString("EpochBreaker.QA.Last1000SeedSummary", summary);
            EditorPrefs.SetString("EpochBreaker.QA.Last1000SeedTimeUtc",
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"));
        }

        /// <summary>
        /// Get last run summary for display in the QA window.
        /// </summary>
        public static string GetLastRunSummary()
        {
            var summary = EditorPrefs.GetString("EpochBreaker.QA.Last1000SeedSummary", "Not run");
            var time = EditorPrefs.GetString("EpochBreaker.QA.Last1000SeedTimeUtc", "N/A");
            return $"1000-Seed Validation: {summary} at {time}";
        }

        private static string FailureReasons(ValidationResult r)
        {
            var reasons = new List<string>();
            if (!r.StartAccessible) reasons.Add("start");
            if (!r.ExitPortalAccessible) reasons.Add("goal");
            if (!r.Reachable) reasons.Add("reach");
            if (!r.WeaponProgressionValid) reasons.Add("weapon");
            if (!r.DestructionPathsValid) reasons.Add("destruct");
            if (!r.DifficultyWithinTolerance) reasons.Add("diff");
            if (!r.NoImpossibleGaps) reasons.Add("gaps");
            if (!r.HasMinCheckpoints) reasons.Add("checkpts");
            if (!r.EntitiesInBounds) reasons.Add("bounds");
            if (!r.StructuralCascadeValid) reasons.Add("cascade");
            return string.Join(", ", reasons);
        }
    }
}
#endif
