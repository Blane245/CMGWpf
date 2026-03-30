namespace CMGWpf.Utilities
{
    public static class GaussianNoise
    {
        public static double Get(Random random, double mean, double standardDeviation)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (standardDeviation < 0) throw new ArgumentOutOfRangeException(nameof(standardDeviation), "Standard deviation must be greater than zero.");
            return GenerateGaussian(random, mean, standardDeviation);
        }
        private static double GenerateGaussian(Random random, double mean, double standardDeviation)
        {
            double u = 1 - random.NextDouble();
            double v = random.NextDouble();
            double z = Math.Sqrt(-2 * Math.Log(u)) * Math.Cos(2 * Math.PI * v);
            return z * standardDeviation + mean;
        }
    }
}
