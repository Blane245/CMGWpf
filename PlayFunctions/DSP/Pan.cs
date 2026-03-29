
using CMGWpf.Types;
using CMGWpf.Utilities;
using static CMGWpf.Model.Generators.StochasticTypes;

namespace CMGWpf.PlayFunctions.DSP
{
    public static class Pan
    {
        public static void Apply (double[] sample, PANALGORITHM algorithm, PanParameters parameters, Random Rn)
        {
            switch (algorithm)
            {
                case PANALGORITHM.walk:
                    {
                        PanWalk(sample, parameters, Rn);
                        return;
                    }
                case PANALGORITHM.glide:
                    {
                        PanGlide(sample, parameters, Rn);
                        return;
                    }
            }

        }
        private static double Bounce(double last, double delta, double low, double high )
        {
            double test = last + delta;
            if (test > high || test < low) return Math.Max(low, Math.Min(high, last - delta));
            return test;
        }
        private static (double, double) PanToLeftRight (double pan)
        {
            return ((1 - pan) / 2, (1 + pan) / 2);
        }
        // pan is a random walk where each step is of DELTAPN size (tuned by experiment). A change is made at each CycleTime
        private static void PanWalk(double[] sample, PanParameters parameters, Random Rn)
        {
            double deltaT = 1.0 / PlayTypes.SampleRate;
            double interval = parameters.CycleTime;
            double walk = Math.Sign(Rn.NextDouble() - 0.5) * StochasticConstants.DELTAPAN;
            double currentInterval = 0;
            double pan1 = 2 * (Rn.NextDouble() - 0.5);
            double pan2 = Bounce(pan1, walk, -1, 1);
            // loop through all of the samples applying the walking pan
            for (int i = 0; i < sample.Length / 2; i++) {
                if (currentInterval >= interval)
                {
                    currentInterval = 0;
                    pan1 = pan2;
                    walk = Math.Sign(Rn.NextDouble() - 0.5) * StochasticConstants.DELTAPAN;
                    pan2 = Bounce(pan1, walk, -1, 1);
                }
                double pan = Interpolation.Linear(currentInterval, 0, interval, pan1, pan2);
                (var left, var right) = PanToLeftRight(pan);
                sample[2 * i] *= left;
                sample[2 * i + 1] *= right;
                currentInterval += deltaT;
            }
        }
        // apply the pan glaide algorithm to the smaple. Pan seqment durations are defined by the parameter cyctime. The number of point on this time line is 10. Pan glides from one point to the next
        private static void PanGlide(double[] sample, PanParameters parameters, Random rN)
        {
            double deltaT = 1.0 / PlayTypes.SampleRate;
            double interval = parameters.CycleTime;
            double walk = Math.Sign(rN.NextDouble() - 0.5) * StochasticConstants.DELTAPAN;
            (var Nd, var Pd) = Probability.Continuous(10, parameters.CycleTime, 0.01); 
            double currentInterval = 0;
            // random first pan
            double pan1 = Probability.Interval(2, rN) - 1; // between -1 and 1;
            double duration = 0;
            while (duration == 0) duration = Probability.Lookup(Pd, Nd, rN.NextDouble()); // length (sec) of the pan glissando
            double speed = Probability.GaussianRandom(0, StochasticConstants.RMSFACTOR * duration, rN);
            double pan2 = Bounce(pan1, duration * speed, -1, 1);
            for (int i = 0; i < sample.Length / 2; i++)
            {
                if (currentInterval >= duration)
                {
                    currentInterval = 0;
                    pan1 = pan2;
                    duration = 0;
                    while (duration == 0) { duration = Probability.Lookup(Pd, Nd, rN.NextDouble()); };
                    speed = Probability.GaussianRandom(0, StochasticConstants.RMSFACTOR * duration, rN);
                    pan2 = Bounce(pan1, duration * speed, -1, 1);
                }
                double pan = Interpolation.Linear(currentInterval, 0, duration, pan1, pan2);
                (var left, var right) = PanToLeftRight(pan);
                sample[2 * i] *= left;
                sample[2 * i + 1] *= right;
                currentInterval += deltaT;
            }
        }
    }
}
