using System;
using System.Globalization;

namespace SixteenBit.Generative
{
    /// <summary>
    /// Encodes and decodes Level IDs in the format: LVLID_[VERSION]_[DIFFICULTY]_[ERA]_[SEED64]
    /// Example: LVLID_1_2_5_A1B2C3D4E5F6A7B8
    ///
    /// Components:
    ///   VERSION:    Format version for backwards compatibility (0-255)
    ///   DIFFICULTY: 0=Easy, 1=Normal, 2=Hard, 3=Extreme (0-3)
    ///   ERA:        Historical era 0-9 determining tileset, materials, enemies
    ///   SEED64:     64-bit hex seed for deterministic generation
    /// </summary>
    public readonly struct LevelID : IEquatable<LevelID>
    {
        public const string PREFIX = "LVLID";
        public const int CURRENT_VERSION = 1;
        public const int MAX_DIFFICULTY = 3;
        public const int MAX_ERA = 9;

        public readonly int Version;
        public readonly int Difficulty;
        public readonly int Era;
        public readonly ulong Seed;

        public LevelID(int version, int difficulty, int era, ulong seed)
        {
            if (version < 0 || version > 255)
                throw new ArgumentOutOfRangeException(nameof(version), "Version must be 0-255");
            if (difficulty < 0 || difficulty > MAX_DIFFICULTY)
                throw new ArgumentOutOfRangeException(nameof(difficulty), $"Difficulty must be 0-{MAX_DIFFICULTY}");
            if (era < 0 || era > MAX_ERA)
                throw new ArgumentOutOfRangeException(nameof(era), $"Era must be 0-{MAX_ERA}");

            Version = version;
            Difficulty = difficulty;
            Era = era;
            Seed = seed;
        }

        public static LevelID Create(int difficulty, int era, ulong seed)
        {
            return new LevelID(CURRENT_VERSION, difficulty, era, seed);
        }

        private static long _seedCounter = 0;
        public static LevelID GenerateNew(int difficulty, int era)
        {
            ulong timePart = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ulong counterPart = (ulong)System.Threading.Interlocked.Increment(ref _seedCounter);
            ulong seed = timePart ^ (counterPart << 32) ^ 0xDEADBEEFCAFEBABEUL;

            seed ^= seed << 13;
            seed ^= seed >> 7;
            seed ^= seed << 17;
            if (seed == 0) seed = 1;

            return Create(difficulty, era, seed);
        }

        public string Encode()
        {
            return string.Format("{0}_{1}_{2}_{3}_{4:X16}",
                PREFIX, Version, Difficulty, Era, Seed);
        }

        public static bool TryParse(string input, out LevelID result)
        {
            result = default;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            string[] parts = input.Trim().Split('_');
            if (parts.Length != 5)
                return false;

            if (parts[0] != PREFIX)
                return false;

            if (!int.TryParse(parts[1], out int version) || version < 0 || version > 255)
                return false;

            if (!int.TryParse(parts[2], out int difficulty) || difficulty < 0 || difficulty > MAX_DIFFICULTY)
                return false;

            if (!int.TryParse(parts[3], out int era) || era < 0 || era > MAX_ERA)
                return false;

            if (!ulong.TryParse(parts[4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong seed))
                return false;

            result = new LevelID(version, difficulty, era, seed);
            return true;
        }

        public static LevelID Parse(string input)
        {
            if (!TryParse(input, out LevelID result))
                throw new FormatException($"Invalid Level ID format: '{input}'. Expected: {PREFIX}_VERSION_DIFFICULTY_ERA_SEED64HEX");
            return result;
        }

        public string EraName => Era switch
        {
            0 => "Stone Age",
            1 => "Bronze Age",
            2 => "Classical",
            3 => "Medieval",
            4 => "Renaissance",
            5 => "Industrial",
            6 => "Modern",
            7 => "Digital",
            8 => "Spacefaring",
            9 => "Transcendent",
            _ => "Unknown"
        };

        public string DifficultyName => Difficulty switch
        {
            0 => "Easy",
            1 => "Normal",
            2 => "Hard",
            3 => "Extreme",
            _ => "Unknown"
        };

        public override string ToString() => Encode();
        public override int GetHashCode() => HashCode.Combine(Version, Difficulty, Era, Seed);
        public bool Equals(LevelID other) =>
            Version == other.Version && Difficulty == other.Difficulty &&
            Era == other.Era && Seed == other.Seed;
        public override bool Equals(object obj) => obj is LevelID other && Equals(other);
        public static bool operator ==(LevelID a, LevelID b) => a.Equals(b);
        public static bool operator !=(LevelID a, LevelID b) => !a.Equals(b);
    }
}
