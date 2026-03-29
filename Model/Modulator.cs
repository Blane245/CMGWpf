namespace CMGWpf.Model
{
    public enum MODULATORTYPE
    {
        NoModulator,
        Sine,
        Square,
        Triangle,
        AscendingSawTooth,
        DescendingSawTooth,
    }
    public static class ModulatorFunctions
    {
        private const double TWO_PI = 2 * Math.PI;
        public static double NoModulator(double time, double center, double frequency, double amplitude, double phase)
        {
            if (frequency == 0 || center == 0) return center;
            return center + amplitude * (double)Math.Sin(TWO_PI * frequency * time + phase);
        }
        public static double Sine(double time, double center, double frequency, double amplitude, double phase)
        {
            if (frequency == 0 || center == 0) return center;
            return center + amplitude * (double)Math.Sin(TWO_PI * frequency * time + phase);
        }
        public static double Square(double time, double center, double frequency, double amplitude, double phase)
        {
            if (frequency == 0 || center == 0) return center;
            double currentPhase = (time * frequency * 360 + phase) % 360;
            return currentPhase < 180 ? center + amplitude / 2 : center - amplitude / 2;
        }
        public static double Triangle(double time, double center, double frequency, double amplitude, double phase)
        {
            if (frequency == 0 || center == 0) return center;
            double currentPhase = (time * frequency * 360 + phase) % 360;
            return currentPhase < 180 ? center + (amplitude * (currentPhase - 90)) / 180 : center - (amplitude * (currentPhase - 270)) / 180;
        }
        public static double AscendingSawTooth(double time, double center, double frequency, double amplitude, double phase)
        {
            if (frequency == 0 || center == 0) return center;
            double period = 1 / frequency;
            double tPhase = (period * phase) / 360;
            double t0 = (time + tPhase) % period;
            double tOffset = t0 < period / 2 ? t0 : t0 - period / 2;
            double result = center - amplitude / 2 + (2 * amplitude * tOffset) / period;
            return result;
        }
        public static double DescendingSawTooth(double time, double center, double frequency, double amplitude, double phase)
        {
            if (frequency == 0 || center == 0) return center;
            double period = 1 / frequency;
            double tPhase = (period * phase) / 360;
            double t0 = (time + tPhase) % period;
            double tOffset = t0 < period / 2 ? t0 : t0 - period / 2;
            double result = center + amplitude / 2 - (2 * amplitude * tOffset) / period;
            return result;
        }
    }
}