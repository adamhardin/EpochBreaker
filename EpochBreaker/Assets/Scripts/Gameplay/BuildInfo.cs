namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Build identification. Update BUILD_NUMBER before each deployment.
    /// Tracked in BUILD-LOG.md at the project root.
    /// </summary>
    public static class BuildInfo
    {
        public const int BUILD_NUMBER = 12;
        public const string BUILD_DATE = "2026-02-06";
        public const string VERSION = "0.6.0";

        public static string FullBuildID => $"v{VERSION} build {BUILD_NUMBER:D3} | {BUILD_DATE}";
    }
}
