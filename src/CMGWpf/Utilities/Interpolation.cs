namespace CMGWpf.Utilities
{
    public static class Interpolation
    {
        public static double Linear(double x, double x0, double x1, double y0, double y1)
        {
            if (x1 == x0) return y0;
            return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
        }

    }
}
