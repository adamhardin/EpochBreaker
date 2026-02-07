namespace EpochBreaker.Generative
{
    /// <summary>
    /// Epoch-based difficulty scaling table.
    /// All values are multipliers applied to base enemy/boss stats.
    /// Smooth interpolation between defined control points.
    /// </summary>
    public static class DifficultyProfile
    {
        // Control points: (epoch, enemyCountMult, enemyHpMult, enemySpeedMult, shootPct, bossHp)
        private static readonly float[,] PROFILES =
        {
            // Epoch  EnemyCount  EnemyHP  EnemySpeed  ShootPct  BossHP
            { 0f,     0.50f,      0.70f,   0.80f,      0.40f,    100f },
            { 3f,     0.80f,      1.00f,   0.95f,      0.60f,    160f },
            { 6f,     0.95f,      1.20f,   1.05f,      0.70f,    220f },
            { 9f,     1.00f,      1.50f,   1.20f,      0.85f,    280f },
        };

        /// <summary>
        /// Get difficulty parameters for a given epoch (0-9).
        /// Linearly interpolates between control points.
        /// </summary>
        public static DifficultyParams GetParams(int epoch)
        {
            float e = epoch;
            if (e < 0f) e = 0f;
            if (e > 9f) e = 9f;

            // Find surrounding control points
            int rows = PROFILES.GetLength(0);
            int lower = 0;
            for (int i = 1; i < rows; i++)
            {
                if (PROFILES[i, 0] <= e)
                    lower = i;
            }
            int upper = lower < rows - 1 ? lower + 1 : lower;

            float lowerEpoch = PROFILES[lower, 0];
            float upperEpoch = PROFILES[upper, 0];
            float t = (upperEpoch > lowerEpoch) ? (e - lowerEpoch) / (upperEpoch - lowerEpoch) : 0f;

            return new DifficultyParams
            {
                EnemyCountMultiplier = Lerp(PROFILES[lower, 1], PROFILES[upper, 1], t),
                EnemyHpMultiplier = Lerp(PROFILES[lower, 2], PROFILES[upper, 2], t),
                EnemySpeedMultiplier = Lerp(PROFILES[lower, 3], PROFILES[upper, 3], t),
                ShootPercentage = Lerp(PROFILES[lower, 4], PROFILES[upper, 4], t),
                BossHp = (int)Lerp(PROFILES[lower, 5], PROFILES[upper, 5], t),
            };
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }

    public struct DifficultyParams
    {
        public float EnemyCountMultiplier;
        public float EnemyHpMultiplier;
        public float EnemySpeedMultiplier;
        public float ShootPercentage;
        public int BossHp;
    }
}
