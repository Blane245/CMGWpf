using CMGWpf.Utilities;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace CMGWpf.Types
{
    public static class ToolUtilities
    {
        public static double MidiToFrequency(double midi)
        {
            return 440.0 * MathUtilities.Pow2((midi - 69) / 12);
        }
        public static double FrequencyToMidi(double frequency)
        {
            return 69 + 12 * MathUtilities.Log2(frequency / 440.0);
        }
    }
}
