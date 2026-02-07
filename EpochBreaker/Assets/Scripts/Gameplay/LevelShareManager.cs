using UnityEngine;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    /// <summary>
    /// Static utility class for generating shareable text and challenge codes.
    /// Handles level sharing, achievement sharing, and friend challenge encoding.
    /// </summary>
    public static class LevelShareManager
    {
        /// <summary>
        /// Generate formatted share text for a completed level.
        /// Example: "I scored 12,450 on Epoch Breaker level 3-K7XM2P9A! Can you beat it?"
        /// </summary>
        public static string GenerateShareText(int score, int stars, string levelCode)
        {
            string starStr = new string('*', stars) + new string('-', 3 - stars);
            return $"I scored {score:N0} on Epoch Breaker level {levelCode}! [{starStr}]\n" +
                   $"Can you beat it?";
        }

        /// <summary>
        /// Generate share text for an unlocked achievement.
        /// Example: "I earned the 'Speed Demon' achievement in Epoch Breaker! Complete a level in under 60s"
        /// </summary>
        public static string GenerateAchievementShareText(string achievementName, string description)
        {
            return $"I earned the '{achievementName}' achievement in Epoch Breaker!\n" +
                   $"{description}";
        }

        /// <summary>
        /// Copy text to the system clipboard using the cross-platform method.
        /// </summary>
        public static void CopyToClipboard(string text)
        {
            GameManager.CopyToClipboard(text);
        }

        /// <summary>
        /// Generate a challenge code that encodes both the level code and the challenger's score.
        /// Format: "levelCode:score" encoded in a simple format.
        /// Example: "3-K7XM2P9A:12450"
        /// </summary>
        public static string GenerateChallengeCode(string levelCode, int score)
        {
            return $"{levelCode}:{score}";
        }

        /// <summary>
        /// Parse a challenge code to extract the level code and friend's score.
        /// Returns true if the code is valid.
        /// </summary>
        public static bool TryParseChallengeCode(string challengeCode, out string levelCode, out int friendScore)
        {
            levelCode = "";
            friendScore = 0;

            if (string.IsNullOrEmpty(challengeCode))
                return false;

            challengeCode = challengeCode.Trim().ToUpperInvariant();

            // Try challenge code format: "levelCode:score"
            int colonIdx = challengeCode.IndexOf(':');
            if (colonIdx > 0 && colonIdx < challengeCode.Length - 1)
            {
                string codePart = challengeCode.Substring(0, colonIdx);
                string scorePart = challengeCode.Substring(colonIdx + 1);

                if (LevelID.TryParse(codePart, out _) && int.TryParse(scorePart, out friendScore))
                {
                    levelCode = codePart;
                    return true;
                }
            }

            // Fallback: try as plain level code (no score)
            if (LevelID.TryParse(challengeCode, out _))
            {
                levelCode = challengeCode;
                friendScore = 0;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Generate the full challenge share text including the challenge code.
        /// Example: "I scored 12,450 on Epoch Breaker! Challenge me: 3-K7XM2P9A:12450"
        /// </summary>
        public static string GenerateChallengeShareText(int score, int stars, string levelCode)
        {
            string starStr = new string('*', stars) + new string('-', 3 - stars);
            string challengeCode = GenerateChallengeCode(levelCode, score);
            return $"I scored {score:N0} on Epoch Breaker! [{starStr}]\n" +
                   $"Challenge me: {challengeCode}";
        }

        /// <summary>
        /// Generate weekly challenge share text.
        /// </summary>
        public static string GenerateWeeklyShareText(int score, int stars, string weekKey)
        {
            string starStr = new string('*', stars) + new string('-', 3 - stars);
            return $"Epoch Breaker Weekly [{starStr}] {score:N0} pts\n" +
                   $"Week: {weekKey}";
        }
    }
}
