using System;
using System.Collections.Generic;

namespace EpochBreaker.Tests
{
    /// <summary>
    /// Standalone test runner that executes all test suites.
    /// Can run outside Unity as a plain C# console application.
    ///
    /// Usage (from command line):
    ///   csc -out:TestRunner.exe TestRunner.cs XORShift64Tests.cs LevelIDTests.cs
    ///       LevelGeneratorTests.cs ../Generative/*.cs
    ///   mono TestRunner.exe
    ///
    /// Or within Unity as an Editor test via NUnit.
    ///
    /// Test suites:
    ///   1. XORShift64Tests  - PRNG determinism, distribution, edge cases
    ///   2. LevelIDTests     - Era-based ID encoding, parsing, validation (10 eras, 4 difficulties)
    ///   3. LevelGeneratorTests - Generation pipeline (TC-001 through TC-013)
    /// </summary>
    public static class TestRunner
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("================================================");
            Console.WriteLine("  Epoch Breaker - Test Suite");
            Console.WriteLine($"  Run at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine("  System: 10-era, destructible environments");
            Console.WriteLine("================================================\n");

            int totalPassed = 0;
            int totalFailed = 0;

            // Suite 1: XORShift64 tests (PRNG - no era/biome dependency)
            {
                var (p, f, m) = XORShift64Tests.RunAll();
                totalPassed += p;
                totalFailed += f;
                foreach (var msg in m) Console.WriteLine(msg);
                Console.WriteLine();
            }

            // Suite 2: LevelID tests (era-based encoding/decoding)
            {
                var (p, f, m) = LevelIDTests.RunAll();
                totalPassed += p;
                totalFailed += f;
                foreach (var msg in m) Console.WriteLine(msg);
                Console.WriteLine();
            }

            // Suite 3: LevelGenerator tests (TC-001 through TC-013)
            {
                var (p, f, m) = LevelGeneratorTests.RunAll();
                totalPassed += p;
                totalFailed += f;
                foreach (var msg in m) Console.WriteLine(msg);
                Console.WriteLine();
            }

            // Summary
            Console.WriteLine("================================================");
            Console.WriteLine($"  TOTAL: {totalPassed} passed, {totalFailed} failed");
            Console.WriteLine($"  {(totalFailed == 0 ? "ALL TESTS PASSED" : "SOME TESTS FAILED")}");
            Console.WriteLine("================================================");

            return totalFailed > 0 ? 1 : 0;
        }
    }
}
