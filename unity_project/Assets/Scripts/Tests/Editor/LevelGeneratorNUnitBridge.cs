using System.Collections.Generic;
using NUnit.Framework;

namespace EpochBreaker.Tests
{
    [TestFixture]
    [Category("LevelGenerator")]
    public class LevelGeneratorNUnitBridge
    {
        private static (int passed, int failed, List<string> messages)? _cachedResults;

        private static (int, int, List<string>) GetResults()
        {
            if (!_cachedResults.HasValue)
                _cachedResults = LevelGeneratorTests.RunAll();
            return _cachedResults.Value;
        }

        public static IEnumerable<TestCaseData> TestCases()
        {
            var (_, _, messages) = GetResults();
            return NUnitBridgeBase.ParseResults(messages, "LevelGenerator");
        }

        [Test, TestCaseSource(nameof(TestCases))]
        public void LevelGeneratorTest(bool passed, string testName)
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
