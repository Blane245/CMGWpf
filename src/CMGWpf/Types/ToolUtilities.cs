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
            return 440.0 * Math.Pow(2, (midi - 69) / 12);
        }
        public static double FrequencyToMidi(double frequency)
        {
            return 69 + 12 * Math.Log2(frequency / 440.0);
        }
    }
}
