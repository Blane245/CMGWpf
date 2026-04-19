namespace CMGWpf.PlayFunctions.Utilities
{
    /// <summary>
    /// Applies a feedback-delay reverb effect to a 2-channel interleafed channel audio
    /// </summary>
    public static class Reverb
    {
        /// <summary>
        /// Apply reverb to a 2-channel interleaved audio buffer. The reverb is a simple feedback-delay effect, where the delayed signal is added back to the original signal with a decay factor. The delay time and decay factor can be adjusted to create different reverb effects.
        /// </summary>
        /// <param name="audioBuffer">2-channel interleaved audio</param>
        /// <param name="reverbDelay">milliseconds of delay</param>
        /// <param name="reverbDecay">decay level (dB)</param>
        /// <param name="sampleRate">number of samples per second</param>
        public static void Apply(double[] audioBuffer, double reverbDelay, double reverbDecay, int sampleRate)
        {
            if (reverbDelay > 0 && reverbDecay > 0)
            {
                int delaySamples = (int)(sampleRate * reverbDelay / 1000) * 2;
                double decayFactor = Math.Pow(10, -reverbDecay / 20.0);
                for (int i = delaySamples; i < audioBuffer.Length - 1; i += 2)
                {
                    audioBuffer[i] += decayFactor * audioBuffer[i - delaySamples];
                    audioBuffer[i + 1] += decayFactor * audioBuffer[i + 1 - delaySamples];
                }
            }
        }
    }
}