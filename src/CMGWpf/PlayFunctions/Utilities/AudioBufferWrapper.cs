namespace CMGWpf.PlayFunctions.Utilities
{
    /// <summary>
    /// Wrapper class for audio buffers to allow safe modification in multithreaded scenarios
    /// without requiring ref parameters
    /// </summary>
    public class AudioBufferWrapper
    {
        public double[] Buffer { get; set; }

        public AudioBufferWrapper(double[] buffer)
        {
            Buffer = buffer;
        }

        /// <summary>
        /// Add input buffer to the wrapped buffer at the specified location, automatically resizing if needed
        /// </summary>
        public void Add(double[] inputBuffer, int location)
        {
            Buffer = AudioBuffer.Add(inputBuffer, Buffer, location);
        }
    }
}
