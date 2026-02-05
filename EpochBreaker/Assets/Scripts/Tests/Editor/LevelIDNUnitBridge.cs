using System.Collections.Generic;
using NUnit.Framework;

namespace SixteenBit.Tests
{
    [TestFixture]
    [Category("LevelID")]
    public class LevelIDNUnitBridge
    {
        private static (int passed, int failed, List<string> messages)? _cachedResults;

        private static (int, int, List<string>) GetResults()
        {
            if (!_cachedResults.HasValue)
                _cachedResults = LevelIDTests.RunAll();
            return _cachedResults.Value;
        }

        public static IEnumerable<TestCaseData> TestCases()
        {
            var (_, _, messages) = GetResults();
            return NUnitBridgeBase.ParseResults(messages, "LevelID");
        }

        [Test, TestCaseSource(nameof(TestCases))]
        public void LevelIDTest(bool passed, string testName)
        {
            Assert.IsTrue(passed, $"Test failed: {testName}");
        }

        [OneTimeTearDown]
        public void ClearCache()
        {
            _cachedResults = null;
        }
    }
}
