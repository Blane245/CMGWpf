using CMGWpf.View;

namespace CMGWpf.PlayFunctions.Utilities
{
    public static class AudioBuffer
    {
        /// <summary>
        /// Add the input buffer to the output buffer starting at the specified location. If the input buffer exceeds the output buffer, the output buffer will be extended to accommodate the additional samples. This is particularly relevant for long release envelopes that may extend beyond the original output buffer length. All buffers and the loaction should consider the number of channels (e.g., for stereo, the buffers should be interleaved and the location should account for both left and right channels). 
        /// </summary>
        /// <param name="inputBuffer" type="double[]">The buffer containing the audio samples to be added.</param>
        /// <param name="outputBuffer" type="double[]">The buffer to which the audio samples will be added.</param>
        /// <param name="location" type="int">The starting index in the output buffer where the input buffer will be added.</param>
        public static void Add(double[] inputBuffer, ref double[] outputBuffer, int location)
        {
            if (location < 0) return;
            int excessLength = (location + inputBuffer.Length) - outputBuffer.Length;
            if (excessLength > 0) {
                // extend the output buffer to accommodate the additional samples
                Array.Resize(ref outputBuffer, outputBuffer.Length + excessLength);
                for (int i = outputBuffer.Length - excessLength; i < outputBuffer.Length; i += 1)
                {
                    outputBuffer[i] = 0; // zero out the extended portion of the output buffer
                }
                DebugLog.Write($"Warning: Audio extended - {excessLength} samples added (input length={inputBuffer.Length}, location={location}, new output length={outputBuffer.Length}, time={(double)excessLength / 44100})");
            }
            for (int i = location; i < location + inputBuffer.Length; i += 1)
            {
                outputBuffer[i] += inputBuffer[i - location];
            }
        }
    }
}
