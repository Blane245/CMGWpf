namespace CMGWpf.Utilities
{
    public static class GaussianNoise
    {
        public static double Get(FastRandom random, double mean, double standardDeviation)
        {
            double u = 1 - random.NextDouble();
            double v = random.NextDouble();
            double z = MathUtilities.Sqrt(-2 * MathUtilities.Log(u)) * MathUtilities.Cos(2 * MathUtilities.PI * v);
            return z * standardDeviation + mean;
        }
    }
}
