using System;
using System.Text;

namespace EpochBreaker.Generative
{
    /// <summary>
    /// Encodes and decodes Level IDs as shareable codes.
    /// Format: E-XXXXXXXX where E is epoch (0-9) and XXXXXXXX is 8 base32 characters.
    /// Example: 3-K7XM2P9A (Medieval epoch, seed K7XM2P9A)
    ///
    /// Base32 alphabet: 0-9, A-V (32 characters = 5 bits each)
    /// 8 characters = 40 bits = ~1 trillion unique levels per epoch
    ///
    /// The same code always produces the exact same level.
    /// </summary>
    public readonly struct LevelID : IEquatable<LevelID>
    {
        public const int MAX_EPOCH = 9;
        private const string BASE32_CHARS = "0123456789ABCDEFGHJKLMNPQRSTUVWX";

        public readonly int Epoch;
        public readonly ulong Seed;

        public LevelID(int epoch, ulong seed)
        {
            if (epoch < 0 || epoch > MAX_EPOCH)
                throw new ArgumentOutOfRangeException(nameof(epoch), $"Epoch must be 0-{MAX_EPOCH}");

            Epoch = epoch;
            Seed = seed;
        }

        private static long _seedCounter = 0;

        /// <summary>
        /// Generate a new random level code for the given epoch.
        /// </summary>
        public static LevelID GenerateRandom(int epoch)
        {
            if (epoch < 0 || epoch > MAX_EPOCH)
                epoch = 0;

            ulong timePart = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ulong counterPart = (ulong)System.Threading.Interlocked.Increment(ref _seedCounter);
            ulong seed = timePart ^ (counterPart << 32) ^ 0xDEADBEEFCAFEBABEUL;

            // Mix the bits
            seed ^= seed << 13;
            seed ^= seed >> 7;
            seed ^= seed << 17;
            if (seed == 0) seed = 1;

            // Mask to 40 bits (8 base32 chars)
            seed &= 0xFFFFFFFFFFUL;

            return new LevelID(epoch, seed);
        }

        /// <summary>
        /// Generate a completely random level (random epoch + random seed).
        /// </summary>
        public static LevelID GenerateRandom()
        {
            ulong timePart = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            int epoch = (int)(timePart % 10);
            return GenerateRandom(epoch);
        }

        /// <summary>
        /// Generate the next level code derived from this one.
        /// Creates a deterministic sequence of levels.
        /// </summary>
        public LevelID Next()
        {
            // Mix current seed to get next
            ulong next = Seed;
            next ^= next << 13;
            next ^= next >> 7;
            next ^= next << 17;
            if (next == 0) next = 1;

            // Mask to 40 bits
            next &= 0xFFFFFFFFFFUL;

            // Progress through epochs (wrap around)
            int nextEpoch = (Epoch + 1) % (MAX_EPOCH + 1);

            return new LevelID(nextEpoch, next);
        }

        /// <summary>
        /// Encode to shareable code format: E-XXXXXXXX
        /// </summary>
        public string ToCode()
        {
            var sb = new StringBuilder(11);
            sb.Append(Epoch);
            sb.Append('-');

            // Encode 40-bit seed as 8 base32 characters
            ulong val = Seed;
            char[] chars = new char[8];
            for (int i = 7; i >= 0; i--)
            {
                chars[i] = BASE32_CHARS[(int)(val & 0x1F)];
                val >>= 5;
            }
            sb.Append(chars);

            return sb.ToString();
        }

        /// <summary>
        /// Try to parse a code string into a LevelID.
        /// </summary>
        public static bool TryParse(string code, out LevelID result)
        {
            result = default;

            if (string.IsNullOrWhiteSpace(code))
                return false;

            code = code.Trim().ToUpperInvariant();

            // Remove any spaces or extra hyphens
            code = code.Replace(" ", "").Replace("--", "-");

            // Expected format: E-XXXXXXXX (length 10)
            if (code.Length != 10)
                return false;

            if (code[1] != '-')
                return false;

            // Parse epoch
            if (!int.TryParse(code.Substring(0, 1), out int epoch) || epoch < 0 || epoch > MAX_EPOCH)
                return false;

            // Parse seed from base32
            string seedStr = code.Substring(2, 8);
            ulong seed = 0;
            for (int i = 0; i < 8; i++)
            {
                int idx = BASE32_CHARS.IndexOf(seedStr[i]);
                if (idx < 0)
                    return false;
                seed = (seed << 5) | (uint)idx;
            }

            result = new LevelID(epoch, seed);
            return true;
        }

        /// <summary>
        /// Parse a code string, throwing on invalid format.
        /// </summary>
        public static LevelID Parse(string code)
        {
            if (!TryParse(code, out LevelID result))
                throw new FormatException($"Invalid level code: '{code}'. Expected format: E-XXXXXXXX (e.g., 3-K7XM2P9A)");
            return result;
        }

        /// <summary>
        /// Create a LevelID from a specific epoch and seed.
        /// </summary>
        public static LevelID Create(int epoch, ulong seed)
        {
            return new LevelID(epoch, seed & 0xFFFFFFFFFFUL);
        }

        /// <summary>
        /// Get the display name for the current epoch.
        /// </summary>
        public string EpochName => Epoch switch
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

        /// <summary>
        /// Get the short epoch prefix (single character).
        /// </summary>
        public string EpochPrefix => Epoch switch
        {
            0 => "S",
            1 => "B",
            2 => "C",
            3 => "M",
            4 => "R",
            5 => "I",
            6 => "O",
            7 => "D",
            8 => "P",
            9 => "T",
            _ => "?"
        };

        /// <summary>
        /// Derive a difficulty-like value (0.0-1.0) from the seed.
        /// Higher values indicate more challenging generation parameters.
        /// </summary>
        public float DerivedIntensity
        {
            get
            {
                // Use lower bits of seed to derive intensity
                uint intensityBits = (uint)(Seed & 0xFFFF);
                return intensityBits / 65535f;
            }
        }

        /// <summary>
        /// Derive level length modifier from seed (0.8 to 1.2).
        /// </summary>
        public float DerivedLengthMod
        {
            get
            {
                uint lengthBits = (uint)((Seed >> 16) & 0xFF);
                return 0.8f + (lengthBits / 255f) * 0.4f;
            }
        }

        public override string ToString() => ToCode();
        public override int GetHashCode() => HashCode.Combine(Epoch, Seed);
        public bool Equals(LevelID other) => Epoch == other.Epoch && Seed == other.Seed;
        public override bool Equals(object obj) => obj is LevelID other && Equals(other);
        public static bool operator ==(LevelID a, LevelID b) => a.Equals(b);
        public static bool operator !=(LevelID a, LevelID b) => !a.Equals(b);
    }
}
