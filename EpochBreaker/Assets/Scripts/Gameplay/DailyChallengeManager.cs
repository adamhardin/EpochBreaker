using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using EpochBreaker.Generative;

namespace EpochBreaker.Gameplay
{
    [Serializable]
    public class DailyScore
    {
        public int Score;
        public int Stars;
        public float Time;
    }

    [Serializable]
    public class DailyData
    {
        public string DateKey;
        public List<DailyScore> Scores = new List<DailyScore>();
    }

    [Serializable]
    public class DailyStreakData
    {
        public string LastPlayedDate;
        public int CurrentStreak;
    }

    [Serializable]
    public class WeeklyData
    {
        public string WeekKey;
        public List<DailyScore> Scores = new List<DailyScore>();
    }

    /// <summary>
    /// Manages daily and weekly challenge systems. Generates deterministic levels from
    /// date-based seeds. Tracks local leaderboards, streaks, and provides share text.
    /// </summary>
    public static class DailyChallengeManager
    {
        private const string DAILY_PREFS_KEY = "EpochBreaker_Daily";
        private const string STREAK_PREFS_KEY = "EpochBreaker_DailyStreak";
        private const string WEEKLY_PREFS_KEY = "EpochBreaker_Weekly";
        private const int MAX_DAILY_SCORES = 5;
        private const int MAX_WEEKLY_SCORES = 5;

        /// <summary>
        /// Get today's date key (YYYYMMDD format).
        /// </summary>
        public static string GetTodayKey()
        {
            return DateTime.Now.ToString("yyyyMMdd");
        }

        /// <summary>
        /// Get today's epoch based on difficulty rotation.
        /// Mon=Easy(0-2), Tue=Medium(3-5), Wed=Hard(6-8), Thu=Expert(9), Fri-Sun=Random
        /// </summary>
        public static int GetTodayEpoch()
        {
            var today = DateTime.Now;
            int dayOfWeek = (int)today.DayOfWeek; // 0=Sun, 1=Mon...

            // Use day as seed for deterministic "random" within range
            int daySeed = today.Year * 10000 + today.Month * 100 + today.Day;

            switch (dayOfWeek)
            {
                case 1: // Monday - Easy (0-2)
                    return daySeed % 3;
                case 2: // Tuesday - Medium (3-5)
                    return 3 + (daySeed % 3);
                case 3: // Wednesday - Hard (6-8)
                    return 6 + (daySeed % 3);
                case 4: // Thursday - Expert (9)
                    return 9;
                default: // Fri, Sat, Sun - Random
                    return daySeed % 10;
            }
        }

        /// <summary>
        /// Get today's difficulty label for UI display.
        /// </summary>
        public static string GetTodayDifficulty()
        {
            int dayOfWeek = (int)DateTime.Now.DayOfWeek;
            return dayOfWeek switch
            {
                1 => "EASY",
                2 => "MEDIUM",
                3 => "HARD",
                4 => "EXPERT",
                _ => "RANDOM"
            };
        }

        /// <summary>
        /// Generate a LevelID for today's daily challenge.
        /// </summary>
        public static LevelID GetTodayLevelID()
        {
            string dateStr = GetTodayKey();
            ulong seed = 0;
            for (int i = 0; i < dateStr.Length; i++)
                seed = seed * 31 + dateStr[i];
            // Ensure non-zero seed
            if (seed == 0) seed = 1;

            int epoch = GetTodayEpoch();
            return new LevelID(epoch, seed);
        }

        /// <summary>
        /// Check if today's daily has been attempted.
        /// </summary>
        public static bool HasPlayedToday()
        {
            var data = LoadDailyData();
            return data != null && data.DateKey == GetTodayKey() && data.Scores.Count > 0;
        }

        /// <summary>
        /// Record a daily challenge score. Keeps top 5 per day.
        /// </summary>
        public static void RecordScore(int score, int stars, float time)
        {
            var data = LoadDailyData();
            string today = GetTodayKey();

            if (data == null || data.DateKey != today)
            {
                data = new DailyData { DateKey = today };
            }

            data.Scores.Add(new DailyScore { Score = score, Stars = stars, Time = time });
            data.Scores.Sort((a, b) => b.Score.CompareTo(a.Score));

            while (data.Scores.Count > MAX_DAILY_SCORES)
                data.Scores.RemoveAt(data.Scores.Count - 1);

            SaveDailyData(data);
            UpdateStreak();
        }

        /// <summary>
        /// Get the current daily streak bonus multiplier.
        /// 0-2 days = 1.0x, 3-6 days = 1.1x, 7+ days = 1.5x
        /// </summary>
        public static float GetStreakMultiplier()
        {
            var streak = LoadStreakData();
            if (streak.CurrentStreak >= 7) return 1.5f;
            if (streak.CurrentStreak >= 3) return 1.1f + (streak.CurrentStreak - 3) * 0.025f;
            return 1.0f;
        }

        /// <summary>
        /// Get the current streak count.
        /// </summary>
        public static int GetStreakCount()
        {
            return LoadStreakData().CurrentStreak;
        }

        /// <summary>
        /// Generate share text for today's daily result.
        /// </summary>
        public static string GetShareText(int score, int stars)
        {
            string starStr = new string('*', stars) + new string('-', 3 - stars);
            int epoch = GetTodayEpoch();
            string epochName = LevelID.GetEpochName(epoch);
            return $"Epoch Breaker Daily [{starStr}] {score} pts\n" +
                   $"{epochName} | {GetTodayDifficulty()}\n" +
                   $"Streak: {GetStreakCount()} days";
        }

        /// <summary>
        /// Get today's top scores for leaderboard display.
        /// </summary>
        public static List<DailyScore> GetTodayScores()
        {
            var data = LoadDailyData();
            if (data == null || data.DateKey != GetTodayKey())
                return new List<DailyScore>();
            return data.Scores;
        }

        // =====================================================================
        // Weekly Challenge System
        // =====================================================================

        /// <summary>
        /// Get the current week key (YYYY-Wnn format based on ISO week number).
        /// Resets every Monday.
        /// </summary>
        public static string GetWeeklyKey()
        {
            var now = DateTime.Now;
            // ISO 8601 week number
            var cal = CultureInfo.InvariantCulture.Calendar;
            int weekNum = cal.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return $"{now.Year}-W{weekNum:D2}";
        }

        /// <summary>
        /// Get the weekly seed based on year + ISO week number.
        /// </summary>
        public static ulong GetWeeklySeed()
        {
            string weekStr = GetWeeklyKey();
            ulong seed = 0;
            for (int i = 0; i < weekStr.Length; i++)
                seed = seed * 37 + weekStr[i];
            if (seed == 0) seed = 1;
            return seed;
        }

        /// <summary>
        /// Get this week's epoch based on the week seed.
        /// Random epoch, determined by week.
        /// </summary>
        public static int GetWeeklyEpoch()
        {
            ulong seed = GetWeeklySeed();
            return (int)(seed % 10);
        }

        /// <summary>
        /// Get the weekly difficulty label for UI display.
        /// Weekly challenges are always "WEEKLY" difficulty.
        /// </summary>
        public static string GetWeeklyDifficulty()
        {
            return "WEEKLY";
        }

        /// <summary>
        /// Generate a LevelID for this week's challenge.
        /// </summary>
        public static LevelID GetWeeklyLevelID()
        {
            ulong seed = GetWeeklySeed();
            int epoch = GetWeeklyEpoch();
            return new LevelID(epoch, seed);
        }

        /// <summary>
        /// Check if this week's challenge has been attempted.
        /// </summary>
        public static bool HasPlayedThisWeek()
        {
            var data = LoadWeeklyData();
            return data != null && data.WeekKey == GetWeeklyKey() && data.Scores.Count > 0;
        }

        /// <summary>
        /// Record a weekly challenge score. Keeps top 5 per week.
        /// </summary>
        public static void RecordWeeklyScore(int score, int stars, float time)
        {
            var data = LoadWeeklyData();
            string thisWeek = GetWeeklyKey();

            if (data == null || data.WeekKey != thisWeek)
            {
                data = new WeeklyData { WeekKey = thisWeek };
            }

            data.Scores.Add(new DailyScore { Score = score, Stars = stars, Time = time });
            data.Scores.Sort((a, b) => b.Score.CompareTo(a.Score));

            while (data.Scores.Count > MAX_WEEKLY_SCORES)
                data.Scores.RemoveAt(data.Scores.Count - 1);

            SaveWeeklyData(data);
        }

        /// <summary>
        /// Get this week's top scores for leaderboard display.
        /// </summary>
        public static List<DailyScore> GetWeeklyScores()
        {
            var data = LoadWeeklyData();
            if (data == null || data.WeekKey != GetWeeklyKey())
                return new List<DailyScore>();
            return data.Scores;
        }

        /// <summary>
        /// Generate share text for this week's result.
        /// </summary>
        public static string GetWeeklyShareText(int score, int stars)
        {
            string starStr = new string('*', stars) + new string('-', 3 - stars);
            int epoch = GetWeeklyEpoch();
            string epochName = LevelID.GetEpochName(epoch);
            return $"Epoch Breaker Weekly [{starStr}] {score} pts\n" +
                   $"{epochName} | {GetWeeklyKey()}\n" +
                   $"More time to optimize!";
        }

        // =====================================================================
        // Private Helpers
        // =====================================================================

        private static void UpdateStreak()
        {
            var streak = LoadStreakData();
            string today = GetTodayKey();

            if (streak.LastPlayedDate == today)
                return; // Already counted today

            // Check if yesterday was played
            string yesterday = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            if (streak.LastPlayedDate == yesterday)
                streak.CurrentStreak++;
            else
                streak.CurrentStreak = 1;

            streak.LastPlayedDate = today;
            SaveStreakData(streak);
        }

        private static DailyData LoadDailyData()
        {
            string json = PlayerPrefs.GetString(DAILY_PREFS_KEY, "");
            if (string.IsNullOrEmpty(json)) return null;
            try { return JsonUtility.FromJson<DailyData>(json); }
            catch { return null; }
        }

        private static void SaveDailyData(DailyData data)
        {
            PlayerPrefs.SetString(DAILY_PREFS_KEY, JsonUtility.ToJson(data));
            SafePrefs.Save();
        }

        private static DailyStreakData LoadStreakData()
        {
            string json = PlayerPrefs.GetString(STREAK_PREFS_KEY, "");
            if (string.IsNullOrEmpty(json)) return new DailyStreakData();
            try { return JsonUtility.FromJson<DailyStreakData>(json) ?? new DailyStreakData(); }
            catch { return new DailyStreakData(); }
        }

        private static void SaveStreakData(DailyStreakData data)
        {
            PlayerPrefs.SetString(STREAK_PREFS_KEY, JsonUtility.ToJson(data));
            SafePrefs.Save();
        }

        private static WeeklyData LoadWeeklyData()
        {
            string json = PlayerPrefs.GetString(WEEKLY_PREFS_KEY, "");
            if (string.IsNullOrEmpty(json)) return null;
            try { return JsonUtility.FromJson<WeeklyData>(json); }
            catch { return null; }
        }

        private static void SaveWeeklyData(WeeklyData data)
        {
            PlayerPrefs.SetString(WEEKLY_PREFS_KEY, JsonUtility.ToJson(data));
            SafePrefs.Save();
        }
    }
}
