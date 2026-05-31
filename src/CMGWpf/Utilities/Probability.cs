
namespace CMGWpf.Utilities
{
    public static class Probability
    {
        // the continuous probability first law. Stop build the arrays when the cumulative probability reaches 99.9%
        public static (double[], double[]) Continuous(int d, double length, double v)
        {
            static double Poisson(int i, double c, double v) { return Math.Exp(-i * c * v) * (1 - Math.Exp(-c * v)); }
            ; // the Poisson Distribution
            // create the probability for the interval in v units
            double c = d / length;
            int n = (int)Math.Round(length / v);
            double[] Pi = [];
            double[] Xi = [];
            double sum = 0;
            for (int i = 0; i < n; i++)
            {
                double p = Poisson(i, c, v);
                sum = (i == n) ? 1.0 : sum + p;
                Pi = [.. Pi, sum];
                Xi = [.. Xi, i * v];
                if (sum > 0.999)
                {
                    Xi = [.. Xi, (i + 1) * v];
                    Pi = [ ..Pi, 1];
                    break;
                }
            }
            return (Xi, Pi);
        }
        // the continuous probability second law
        public static double Interval(double length, FastRandom Rn)
        {
            return length * (1 - Math.Sqrt(1 - Rn.NextDouble()));
        }

        public static double Lookup(double[] p, double[] x, double r)
        {
            if (p.Length == 0) return 0;
            if (r < p[0]) return 0;
            if (x.Length == 0) return 0;
            if (r > 1) return x[^1];
            int index = Array.FindIndex(p, v => r <= v);
            if (index == -1) return 0;
            return x[index];
        }
        // Standard Normal variate using Box-Muller transform.
        public static double GaussianRandom(double mean, double stdev, FastRandom rN)
        {
            if (stdev == 0) return mean;
            double u = 1 - rN.NextDouble(); // Converting [0,1) to (0,1]
            double v = rN.NextDouble();
            double z = MathUtilities.Sqrt(-2.0 * MathUtilities.Log(u)) * MathUtilities.Cos(2.0 * MathUtilities.PI * v);
            // Transform to the desired mean and standard deviation:
            return z * stdev + mean;
        }

    }
}
