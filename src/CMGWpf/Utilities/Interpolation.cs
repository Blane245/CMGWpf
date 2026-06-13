namespace CMGWpf.Utilities
{
    public static class Interpolation
    {
        /// <summary>
        /// Performs linear interpolation to find the y value corresponding to a given x value, based on two known points (x0, y0) and (x1, y1). If x1 equals x0, it returns y0 to avoid division by zero.
        /// </summary>
        /// <param name="x">The x value for which to interpolate the y value.</param>
        /// <param name="x0">The x value of the first known point.</param>
        /// <param name="x1">The x value of the second known point.</param>
        /// <param name="y0">The y value of the first known point.</param>
        /// <param name="y1">The y value of the second known point.</param>
        /// <returns>The interpolated y value corresponding to the given x value.</returns>
        public static double Linear(double x, double x0, double x1, double y0, double y1)
        {
            if (x1 == x0) return y0;
            return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
        }

    }
}
