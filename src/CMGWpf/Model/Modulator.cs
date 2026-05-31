using CMGWpf.Utilities;

namespace CMGWpf.Model
{
    public enum MODULATORTYPE
    {
        NOMODULATOR,
        SINE,
        SQUARE,
        TRIANGLE,
        ASCENDINGSAWTOOTH,
        DESCENDINGSAWTOOTH,
    }
    public static class ModulatorFunctions
    {
        private const double TWO_PI = 2 * Math.PI;
        public static double NoModulator(double time, double center, double frequency, double amplitude, double phase)
        {
            return center;
        }
        public static double Sine(double time, double center, double frequency, double amplitude, double phase)
        {
            if (frequency == 0) return center;
            if (double.IsNaN(frequency) || double.IsInfinity(frequency)) return double.NaN;
            return center + amplitude * (double)MathUtilities.Sin(TWO_PI * frequency * time + phase * TWO_PI / 360);
        }
        public static double Square(double time, double center, double frequency, double amplitude, double phase)
        {
            if (frequency == 0) return center;
            if (double.IsNaN(frequency) || double.IsInfinity(frequency)) return double.NaN;
            double period = 1 / frequency;
            double lowValue = center + amplitude / 2;
            double highValue = center - amplitude / 2;
            double tOffset = time + period * phase / 360;
            double t = tOffset - Math.Floor(time / period) * period;
            double result = (t < period / 2) ? lowValue:highValue;
            return result;
        }
        public static double Triangle(double time, double center, double frequency, double amplitude, double phase)
        {
            if (frequency == 0) return center;
            if (double.IsNaN(frequency) || double.IsInfinity(frequency)) return double.NaN;
            double period = 1 / frequency;
            double lowValue = center - amplitude / 2;
            double highValue = center + amplitude / 2;
            double tOffset = time + period * phase / 360;
            double t = tOffset - Math.Floor(time / period) * period;
            double result = (t < period / 2)? Interpolation.Linear(t, 0, period / 2, lowValue, highValue):
                Interpolation.Linear(t, period / 2, period, highValue, lowValue);
            return result;
        }
        public static double AscendingSawTooth(double time, double center, double frequency, double amplitude, double phase)
        {
            if (frequency == 0) return center;
            if (double.IsNaN(frequency) || double.IsInfinity(frequency)) return double.NaN;
            double period = 1 / frequency;
            double lowValue = center - amplitude / 2;
            double highValue = center + amplitude / 2;
            double tOffset = time + period * phase / 360; 
            double t = tOffset - Math.Floor(time / period) * period;
            double result = Interpolation.Linear(t, 0, period, lowValue, highValue);
            return result;
        }
        public static double DescendingSawTooth(double time, double center, double frequency, double amplitude, double phase)
        {
            if (frequency == 0) return center;
            if (double.IsNaN(frequency) || double.IsInfinity(frequency)) return double.NaN;
            double period = 1 / frequency;
            double lowValue = center + amplitude / 2;
            double highValue = center - amplitude / 2;
            double tOffset = time + period * phase / 360;
            double t = tOffset - Math.Floor(time / period) * period;
            double result = Interpolation.Linear(t, 0, period, lowValue, highValue);
            return result;
        }
    }
}