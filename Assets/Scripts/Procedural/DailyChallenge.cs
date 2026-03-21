using System;
using PathOfPower.Core;
using PathOfPower.Data;

namespace PathOfPower.Procedural
{
    public static class DailyChallenge
    {
        /// <summary>
        /// Returns a deterministic seed based on today's UTC date.
        /// Same seed for all players worldwide every day.
        /// </summary>
        public static int GetTodaySeed()
        {
            string dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
            // Simple but consistent hash
            int hash = 17;
            foreach (char c in dateStr)
                hash = hash * 31 + c;
            return Math.Abs(hash);
        }

        /// <summary>
        /// Returns today's daily challenge level. Difficulty scales with day of year (1-10).
        /// </summary>
        public static LevelData GetDailyLevel()
        {
            int seed = GetTodaySeed();
            int dayOfYear = DateTime.UtcNow.DayOfYear;
            int difficulty = Math.Max(1, Math.Min(10, (dayOfYear % 10) + 1));

            LevelData level = LevelGenerator.GenerateLevel(seed, difficulty);
            level.id   = $"daily_{DateTime.UtcNow:yyyyMMdd}";
            level.name = $"Daily Challenge — {DateTime.UtcNow:MMM dd}";
            level.biome = "Daily";
            return level;
        }

        public static bool IsNewDay(PlayerData data)
        {
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            return data.lastDailyDate != today;
        }

        public static void MarkDailyPlayed(PlayerData data, int score)
        {
            string today = DateTime.UtcNow.ToString("yyyyMMdd");
            data.lastDailyDate = today;
            if (score > data.dailyBestScore) data.dailyBestScore = score;
        }
    }
}
