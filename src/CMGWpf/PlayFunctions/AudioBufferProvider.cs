using NAudio.Wave;

namespace CMGWpf.PlayFunctions
{
    /// <summary>
    /// NAudio wave provider that plays from a float audio buffer
    /// </summary>
    public class AudioBufferProvider : IWaveProvider
    {
        private readonly float[] buffer;
        private long position;
        private readonly object lockObject = new();

        public WaveFormat WaveFormat { get; }

        /// <summary>
        /// Current playback position in seconds
        /// </summary>
        public double CurrentPosition
        {
            get
            {
                lock (lockObject)
                {
                    return (double)position / (WaveFormat.SampleRate * WaveFormat.Channels);
                }
            }
        }

        /// <summary>
        /// Total duration in seconds
        /// </summary>
        public double Duration => (double)buffer.Length / (WaveFormat.SampleRate * WaveFormat.Channels);

        /// <summary>
        /// Create a new audio buffer provider
        /// </summary>
        /// <param name="stereoBuffer">Interleaved stereo buffer [L, R, L, R, ...]</param>
        /// <param name="sampleRate">Sample rate (e.g., 44100)</param>
        public AudioBufferProvider(float[] stereoBuffer, int sampleRate)
        {
            buffer = stereoBuffer;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2); // 2 channels (stereo)
            position = 0;
        }

        /// <summary>
        /// Read samples from the buffer, converting float to bytes
        /// </summary>
        public int Read(byte[] destBuffer, int offset, int count)
        {
            lock (lockObject)
            {
                int floatsAvailable = buffer.Length - (int)position;
                int floatsRequested = count / 4; // 4 bytes per float
                int floatsToCopy = Math.Min(floatsAvailable, floatsRequested);

                if (floatsToCopy > 0)
                {
                    // Convert float samples to bytes
                    Buffer.BlockCopy(buffer, (int)position * 4, destBuffer, offset, floatsToCopy * 4);
                    position += floatsToCopy;
                }

                int bytesRead = floatsToCopy * 4;

                // Fill remaining with silence (zeros)
                if (bytesRead < count)
                {
                    Array.Clear(destBuffer, offset + bytesRead, count - bytesRead);
                }

                return bytesRead;
            }
        }

        /// <summary>
        /// Set playback position by time
        /// </summary>
        public void SetPosition(TimeSpan time)
        {
            lock (lockObject)
            {
                long newPosition = (long)(time.TotalSeconds * WaveFormat.SampleRate * WaveFormat.Channels);
                position = Math.Clamp(newPosition, 0, buffer.Length);
            }
        }

        /// <summary>
        /// Set playback position by sample index
        /// </summary>
        public void SetPositionBySample(long sampleIndex)
        {
            lock (lockObject)
            {
                position = Math.Clamp(sampleIndex, 0, buffer.Length);
            }
        }

        /// <summary>
        /// Reset to beginning
        /// </summary>
        public void Reset()
        {
            lock (lockObject)
            {
                position = 0;
            }
        }

        /// <summary>
        /// Check if playback has finished
        /// </summary>
        public bool HasFinished => position >= buffer.Length;

        /// <summary>
        /// Get average signal level (RMS) for each channel over the last second
        /// </summary>
        /// <returns>Tuple of (leftLevel, rightLevel) in range 0.0 to 1.0</returns>
        public double[] GetRecentSignalLevels(double volume)
        {
            lock (lockObject)
            {
                // Calculate how many samples to look back (1 second worth)
                int samplesPerSecond = WaveFormat.SampleRate * WaveFormat.Channels; // Total samples for 1 second (both channels)
                long startPos = Math.Max(0, position - samplesPerSecond);
                int samplesToAnalyze = (int)(position - startPos);

                if (samplesToAnalyze < WaveFormat.Channels)
                {
                    // Not enough data yet
                    return [0.0, 0.0];
                }

                // Calculate RMS for each channel
                double leftSum = 0.0;
                double rightSum = 0.0;
                int leftCount = 0;
                int rightCount = 0;

                for (long i = startPos; i < position; i += WaveFormat.Channels)
                {
                    // Left channel (even indices in interleaved buffer)
                    if (i < buffer.Length)
                    {
                        float leftSample = buffer[i];
                        leftSum += leftSample * leftSample; // Square for RMS
                        leftCount++;
                    }

                    // Right channel (odd indices in interleaved buffer)
                    if (i + 1 < buffer.Length)
                    {
                        float rightSample = buffer[i + 1];
                        rightSum += rightSample * rightSample; // Square for RMS
                        rightCount++;
                    }
                }

                // Calculate RMS (Root Mean Square)
                double leftRMS = leftCount > 0 ? Math.Sqrt(leftSum / leftCount) : 0.0;
                double rightRMS = rightCount > 0 ? Math.Sqrt(rightSum / rightCount) : 0.0;

                return [leftRMS * volume, rightRMS * volume];
            }
        }

        /// <summary>
        /// Get peak signal level for each channel over the last second
        /// </summary>
        /// <returns>Tuple of (leftPeak, rightPeak) in range 0.0 to 1.0</returns>
        public double[] GetRecentPeakLevels(double volume)
        {
            lock (lockObject)
            {
                // Calculate how many samples to look back (1 second worth)
                int samplesPerSecond = WaveFormat.SampleRate * WaveFormat.Channels;
                long startPos = Math.Max(0, position - samplesPerSecond);

                if (position - startPos < WaveFormat.Channels)
                {
                    // Not enough data yet
                    return [0.0, 0.0];
                }

                double leftPeak = 0.0;
                double rightPeak = 0.0;

                for (long i = startPos; i < position; i += WaveFormat.Channels)
                {
                    // Left channel
                    if (i < buffer.Length)
                    {
                        leftPeak = Math.Max(leftPeak, Math.Abs(buffer[i]));
                    }

                    // Right channel
                    if (i + 1 < buffer.Length)
                    {
                        rightPeak = Math.Max(rightPeak, Math.Abs(buffer[i + 1]));
                    }
                }

                return [leftPeak * volume, rightPeak * volume];
            }
        }
    }
}
