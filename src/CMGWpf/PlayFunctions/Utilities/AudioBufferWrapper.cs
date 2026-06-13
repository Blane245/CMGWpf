namespace CMGWpf.PlayFunctions.Utilities
{
    /// <summary>
    /// Wrapper class for audio buffers to allow safe modification in multi-threaded scenarios
    /// without requiring ref parameters
    /// </summary>
    public class AudioBufferWrapper(double[] buffer)
    {
        public double[] Buffer { get; set; } = buffer;

        /// <summary>
        /// Add input buffer to the wrapped buffer at the specified location, automatically resizing if needed
        /// </summary>
        /// <param name="inputBuffer">The buffer containing the audio samples to be added.</param>
        /// <param name="location">The starting index in the wrapped buffer where the input buffer will be added.</param>
        public void Add(double[] inputBuffer, int location)
        {
            Buffer = AudioBuffer.Add(inputBuffer, Buffer, location);
        }
    }
}
