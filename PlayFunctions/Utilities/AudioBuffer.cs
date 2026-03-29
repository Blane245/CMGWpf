using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace CMGWpf.PlayFunctions.Utilities
{
    public static class AudioBuffer
    {
        public static void Add(double[] outputBuffer, double[] inputBuffer, int location)
        {
            int end = Math.Min(inputBuffer.Length + location, outputBuffer.Length);
            for (int i = location; i < end; i += 1)
            {
                outputBuffer[i] += inputBuffer[i - location];
            }
        }
    }
}
