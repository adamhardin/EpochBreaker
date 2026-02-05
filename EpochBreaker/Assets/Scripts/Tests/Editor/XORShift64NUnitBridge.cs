using System.Collections.Generic;
using NUnit.Framework;

namespace EpochBreaker.Tests
{
    [TestFixture]
    [Category("PRNG")]
    public class XORShift64NUnitBridge
    {
        private static (int passed, int failed, List<string> messages)? _cachedResults;

        private static (int, int, List<string>) GetResults()
        {
            if (!_cachedResults.HasValue)
                _cachedResults = XORShift64Tests.RunAll();
            return _cachedResults.Value;
        }

        public static IEnumerable<TestCaseData> TestCases()
        {
            var (_, _, messages) = GetResults();
            return NUnitBridgeBase.ParseResults(messages, "XORShift64");
        }

        [Test, TestCaseSource(nameof(TestCases))]
        public void XORShift64Test(bool passed, string testName)
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
