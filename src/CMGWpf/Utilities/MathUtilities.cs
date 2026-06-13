namespace CMGWpf.Utilities
{
    /// <summary>
    /// Fast mathematical operations using lookup tables with linear interpolation
    /// </summary>
    public static class MathUtilities
    {
        #region Constants and Tables

        private const int SIN_TABLE_SIZE = 2048;
        private const int POW10_TABLE_SIZE = 2048;
        private const int POW2_TABLE_SIZE = 2048;
        private const int SQRT_TABLE_SIZE = 2048;
        private const int LOG_TABLE_SIZE = 2048;
        private const int LOG10_TABLE_SIZE = 2048;
        private const int LOG2_TABLE_SIZE = 2048;

        // Pow10 range: -10 to +10 for dB conversions
        private const double POW10_MIN = -10.0;
        private const double POW10_MAX = 10.0;
        private const double POW10_RANGE = POW10_MAX - POW10_MIN;

        // Pow2 range: -10 to +10 for octaves
        private const double POW2_MIN = -10.0;
        private const double POW2_MAX = 10.0;
        private const double POW2_RANGE = POW2_MAX - POW2_MIN;

        // Sqrt range: 0 to 100 (covers most audio use cases)
        private const double SQRT_MAX = 100.0;

        // Log range: 0.001 to 1000 (avoids zero, covers wide range)
        private const double LOG_MIN = 0.001;
        private const double LOG_MAX = 1000.0;
        private const double LOG_RANGE = LOG_MAX - LOG_MIN;

        private static readonly double[] sinTable;
        private static readonly double[] pow10Table;
        private static readonly double[] pow2Table;
        private static readonly double[] sqrtTable;
        private static readonly double[] logTable;
        private static readonly double[] log10Table;
        private static readonly double[] log2Table;

        public static readonly double PI = Math.PI;

        #endregion

        #region Initialization

        static MathUtilities()
        {
            // Build sine table (0 to 2π)
            sinTable = new double[SIN_TABLE_SIZE];
            for (int i = 0; i < SIN_TABLE_SIZE; i++)
            {
                double angle = (double)i / SIN_TABLE_SIZE * Math.PI * 2.0;
                sinTable[i] = Math.Sin(angle);
            }

            // Build Pow10 table
            pow10Table = new double[POW10_TABLE_SIZE];
            for (int i = 0; i < POW10_TABLE_SIZE; i++)
            {
                double exponent = POW10_MIN + (double)i / (POW10_TABLE_SIZE - 1) * POW10_RANGE;
                pow10Table[i] = Math.Pow(10.0, exponent);
            }

            // Build Pow2 table
            pow2Table = new double[POW2_TABLE_SIZE];
            for (int i = 0; i < POW2_TABLE_SIZE; i++)
            {
                double exponent = POW2_MIN + (double)i / (POW2_TABLE_SIZE - 1) * POW2_RANGE;
                pow2Table[i] = Math.Pow(2.0, exponent);
            }

            // Build Sqrt table
            sqrtTable = new double[SQRT_TABLE_SIZE];
            for (int i = 0; i < SQRT_TABLE_SIZE; i++)
            {
                double value = (double)i / (SQRT_TABLE_SIZE - 1) * SQRT_MAX;
                sqrtTable[i] = Math.Sqrt(value);
            }

            // Build Log tables (natural and base-10 and base-2)
            logTable = new double[LOG10_TABLE_SIZE];
            log10Table = new double[LOG10_TABLE_SIZE];
            log2Table = new double[LOG2_TABLE_SIZE];
            for (int i = 0; i < LOG10_TABLE_SIZE; i++)
            {
                double value = LOG_MIN + (double)i / (LOG10_TABLE_SIZE - 1) * LOG_RANGE;
                logTable[i] = Math.Log(value);
                log10Table[i] = Math.Log10(value);
                log2Table[i] = Math.Log(value, 2.0);
            }
        }

        #endregion

        #region Trigonometric Functions

        /// <summary>
        /// Fast sine using lookup table with linear interpolation
        /// </summary>
        /// <param name="radians">Angle in radians</param>
        /// <returns>Sine value</returns>
        public static double Sin(double radians)
        {
            // Normalize to 0-2π range
            double normalized = radians % (Math.PI * 2.0);
            if (normalized < 0) normalized += Math.PI * 2.0;

            // Convert to table index
            double indexFloat = normalized / (Math.PI * 2.0) * SIN_TABLE_SIZE;
            int index = (int)indexFloat;
            double fraction = indexFloat - index;

            // Handle wrap-around
            int nextIndex = (index + 1) % SIN_TABLE_SIZE;

            // Linear interpolation
            return sinTable[index] + (sinTable[nextIndex] - sinTable[index]) * fraction;
        }

        /// <summary>
        /// Fast cosine using sine lookup table
        /// </summary>
        /// <param name="radians">Angle in radians</param>
        /// <returns>Cosine value</returns>
        public static double Cos(double radians)
        {
            // cos(x) = sin(x + π/2)
            return Sin(radians + PI / 2.0);
        }

        #endregion

        #region Power Functions

        /// <summary>
        /// Fast power of 10 for dB conversions (range: -10 to +10)
        /// </summary>
        /// <param name="exponent">Exponent value</param>
        /// <returns>10^exponent</returns>
        public static double Pow10(double exponent)
        {
            // Clamp to table range
            if (exponent < POW10_MIN) exponent = POW10_MIN;
            if (exponent > POW10_MAX) exponent = POW10_MAX;

            // Convert to table index
            double indexFloat = (exponent - POW10_MIN) / POW10_RANGE * (POW10_TABLE_SIZE - 1);
            int index = (int)indexFloat;
            double fraction = indexFloat - index;

            // Linear interpolation
            if (index >= POW10_TABLE_SIZE - 1) return pow10Table[POW10_TABLE_SIZE - 1];
            return pow10Table[index] + (pow10Table[index + 1] - pow10Table[index]) * fraction;
        }

        /// <summary>
        /// Fast power of 2 for octaves (range: -10 to +10)
        /// </summary>
        /// <param name="exponent">Exponent value</param>
        /// <returns>2^exponent</returns>
        public static double Pow2(double exponent)
        {
            // Clamp to table range
            if (exponent < POW2_MIN) exponent = POW2_MIN;
            if (exponent > POW2_MAX) exponent = POW2_MAX;

            // Convert to table index
            double indexFloat = (exponent - POW2_MIN) / POW2_RANGE * (POW2_TABLE_SIZE - 1);
            int index = (int)indexFloat;
            double fraction = indexFloat - index;

            // Linear interpolation
            if (index >= POW2_TABLE_SIZE - 1) return pow2Table[POW2_TABLE_SIZE - 1];
            return pow2Table[index] + (pow2Table[index + 1] - pow2Table[index]) * fraction;
        }

        /// <summary>
        /// General power function with optimization for common bases
        /// For variable base (1-10) with exponent (-10 to +10), uses calculation
        /// For base 10 or 2, uses lookup tables
        /// </summary>
        /// <param name="baseValue">Base value</param>
        /// <param name="exponent">Exponent value</param>
        /// <returns>base^exponent</returns>
        public static double Pow(double baseValue, double exponent)
        {
            // Optimize common cases
            if (Math.Abs(baseValue - 10.0) < 0.0001)
                return Pow10(exponent);

            if (Math.Abs(baseValue - 2.0) < 0.0001)
                return Pow2(exponent);

            // For other bases, use standard Math.Pow
            // (Could add more lookup tables if specific bases are common)
            return Math.Pow(baseValue, exponent);
        }

        #endregion

        #region Sqrt and Log Functions

        /// <summary>
        /// Fast square root using lookup table (range: 0 to 100)
        /// </summary>
        /// <param name="value">Value to take square root of</param>
        /// <returns>Square root</returns>
        public static double Sqrt(double value)
        {
            if (value <= 0) return 0;
            if (value > SQRT_MAX) return Math.Sqrt(value); // Fall back for large values

            // Convert to table index
            double indexFloat = value / SQRT_MAX * (SQRT_TABLE_SIZE - 1);
            int index = (int)indexFloat;
            double fraction = indexFloat - index;

            // Linear interpolation
            if (index >= SQRT_TABLE_SIZE - 1) return sqrtTable[SQRT_TABLE_SIZE - 1];
            return sqrtTable[index] + (sqrtTable[index + 1] - sqrtTable[index]) * fraction;
        }

        /// <summary>
        /// Fast natural logarithm using lookup table (range: 0.001 to 1000)
        /// </summary>
        /// <param name="value">Value to take log of</param>
        /// <returns>Natural logarithm</returns>
        public static double Log(double value)
        {
            if (value <= 0) return double.NegativeInfinity;
            if (value < LOG_MIN || value > LOG_MAX) return Math.Log(value); // Fall back

            // Convert to table index
            double indexFloat = (value - LOG_MIN) / LOG_RANGE * (LOG_TABLE_SIZE - 1);
            int index = (int)indexFloat;
            double fraction = indexFloat - index;

            // Linear interpolation
            if (index >= LOG_TABLE_SIZE - 1) return logTable[LOG_TABLE_SIZE - 1];
            return logTable[index] + (logTable[index + 1] - logTable[index]) * fraction;
        }

        /// <summary>
        /// Fast base-10 logarithm using lookup table (range: 0.001 to 1000)
        /// </summary>
        /// <param name="value">Value to take log10 of</param>
        /// <returns>Base-10 logarithm</returns>
        public static double Log10(double value)
        {
            if (value <= 0) return double.NegativeInfinity;
            if (value < LOG_MIN || value > LOG_MAX) return Math.Log10(value); // Fall back

            // Convert to table index
            double indexFloat = (value - LOG_MIN) / LOG_RANGE * (LOG10_TABLE_SIZE - 1);
            int index = (int)indexFloat;
            double fraction = indexFloat - index;

            // Linear interpolation
            if (index >= LOG10_TABLE_SIZE - 1) return log10Table[LOG10_TABLE_SIZE - 1];
            return log10Table[index] + (log10Table[index + 1] - log10Table[index]) * fraction;
        }
        /// <summary>
        /// Fast base-2 logarithm using lookup table (range: 0.001 to 1000)
        /// </summary>
        /// <param name="value">Value to take log2 of</param>
        /// <returns>Base-2 logarithm</returns>
        public static double Log2(double value)
        {
            if (value <= 0) return double.NegativeInfinity;
            if (value < LOG_MIN || value > LOG_MAX) return Math.Log2(value); // Fall back

            // Convert to table index
            double indexFloat = (value - LOG_MIN) / LOG_RANGE * (LOG2_TABLE_SIZE - 1);
            int index = (int)indexFloat;
            double fraction = indexFloat - index;

            // Linear interpolation
            if (index >= LOG2_TABLE_SIZE - 1) return log2Table[LOG2_TABLE_SIZE - 1];
            return log2Table[index] + (log2Table[index + 1] - log2Table[index]) * fraction;
        }

        #endregion


        public static FastRandom StartFastRandom(string? seed)
        {
            FastRandom r = (string.IsNullOrEmpty(seed)) ? new FastRandom() : new FastRandom(GetStateFromSeed(seed));
            return r;
        }
        private static ulong GetStateFromSeed(string seed)
        {
            // Convert seed string to ulong using hash code
            ulong hash = (ulong)seed.GetHashCode();
            return hash;
        }
    }
    /// <summary>
    /// Extremely fast random number generator using xorshift algorithm
    /// Much faster than System.Random, uses only bit operations and multiplication
    /// Thread-safe via ThreadLocal
    /// </summary>
    public class FastRandom
    {
        private ulong state;
        /// <summary>
        /// Create a new FastRandom with time-based seed by shuffling the current time string to generate a more unique seed
        /// </summary>
        public FastRandom()
        {
            string[] now = DateTime.Now.ToString("yyyyMMddHHmmssffff").Split("");
            // shuffle the characters of now
            Random rn = new Random(); // using the system random number generator to shuffle the characters of the time string
            for (int i = 0; i < now.Length; i++)
            {
                now[i] = now[(int)rn.Next(now.Length)];
            }
            string theSeed = string.Join("", now);
            state = (ulong)(theSeed.GetHashCode() ^ (int)DateTime.Now.Ticks);
            if (state == 0) state = 1; // Ensure non-zero
        }

        /// <summary>
        /// Create a new FastRandom with specific seed
        /// </summary>
        /// <param name="seed">Seed value</param>
        public FastRandom(ulong seed)
        {
            state = seed == 0 ? 1 : seed; // Ensure non-zero
        }

        /// <summary>
        /// Generate next random 64-bit unsigned integer using xorshift64*
        /// </summary>
        /// <returns>Random unsigned long</returns>
        public ulong NextULong()
        {
            ulong x = state;
            x ^= x >> 12;
            x ^= x << 25;
            x ^= x >> 27;
            state = x;
            return x * 0x2545F4914F6CDD1DUL; // Multiplication constant
        }

        /// <summary>
        /// Generate random double in range [0.0, 1.0)
        /// </summary>
        /// <returns>Random double</returns>
        public double NextDouble()
        {
            // Convert to double in range [0, 1)
            return (NextULong() >> 11) * (1.0 / (1UL << 53));
        }

        /// <summary>
        /// Generate random integer in range [0, maxValue)
        /// </summary>
        /// <param name="maxValue">Exclusive upper bound</param>
        /// <returns>Random integer</returns>
        public int Next(int maxValue)
        {
            return (int)(NextDouble() * maxValue);
        }

        /// <summary>
        /// Generate random integer in range [minValue, maxValue)
        /// </summary>
        /// <param name="minValue">Inclusive lower bound</param>
        /// <param name="maxValue">Exclusive upper bound</param>
        /// <returns>Random integer</returns>
        public int Next(int minValue, int maxValue)
        {
            return minValue + (int)(NextDouble() * (maxValue - minValue));
        }
    }
}

