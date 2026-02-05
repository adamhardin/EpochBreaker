using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace SixteenBit.Tests
{
    /// <summary>
    /// Shared utility for parsing RunAll() output into NUnit TestCaseData.
    /// Each PASS/FAIL line from the custom test framework becomes its own
    /// NUnit test case visible in Unity Test Runner.
    /// </summary>
    public static class NUnitBridgeBase
    {
        private static readonly Regex ResultPattern =
            new Regex(@"^\s+(PASS|FAIL):\s+(.+)$", RegexOptions.Compiled);

        /// <summary>
        /// Parse messages from RunAll() into individual test case data objects.
        /// </summary>
        public static IEnumerable<TestCaseData> ParseResults(
            List<string> messages, string suiteName)
        {
            int index = 0;
            foreach (var msg in messages)
            {
                var match = ResultPattern.Match(msg);
                if (!match.Success) continue;

                string status = match.Groups[1].Value;
                string testName = match.Groups[2].Value;
                bool passed = status == "PASS";

                yield return new TestCaseData(passed, testName)
                    .SetName($"{suiteName}_{index:D2}_{SanitizeName(testName)}")
                    .SetCategory(suiteName);
                index++;
            }
        }

        private static string SanitizeName(string name)
        {
            if (name.Length > 80)
                name = name.Substring(0, 80);
            return name
                .Replace("(", "")
                .Replace(")", "")
                .Replace(",", "_")
                .Replace("'", "")
                .Replace("\"", "");
        }
    }
}
