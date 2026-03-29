using CMGWpf.Model;
using CMGWpf.Model.Generators;
using CMGWpf.View;
using CMGWpf.PlayFunctions.Utilities;
using static CMGWpf.Types.PlayTypes;
using CMGWpf.PlayFunctions.DSP;
using CMGWpf.Types;

namespace CMGWpf.PlayFunctions
{
    //this will start the integration of the soundfont BuildVoicesForPresetAtKeyVel routine which is the first step in getting instrument smaples for DSP. It will be used in the PlayEngine and will be called when a note is played to determine which samples to use for that note. 
    public static class PlayEngine
    {
        public static float[] Go()
        {
            string error = "";
            int totalSamples = (int)Math.Ceiling(FileViewModel.Instance.PlayDuration) * PlayTypes.SampleRate;
            double[] stereoBuffer = new double[totalSamples * 2]; // the sample buffer for the entire composition with interlaced stereo
            List<SF_Preset> sF_Presets = FileViewModel.Instance.SF_Presets; // this will be populated with the sounfont/preset unique list for later assigning colors 
            foreach (Generator gen in FileViewModel.Instance.PlayGenerators)
            {
                switch (gen)
                {
                    case Silent:
                        break;
                    case Algorithmic:
                        error = SourcesFromAlgorithmic.Get(gen as Algorithmic, stereoBuffer, sF_Presets);
                        break;
                    case Stochastic:
                        error = SourcesFromStochastic.Get(gen as Stochastic, stereoBuffer, sF_Presets);
                        break;
                }

            }

            // build the color palette for the presets that are to be played based on the sF_Presets collection that was populated while processing the generators. This will be used to assign colors to the notes in the UI so that the user can see which notes correspond to which presets. For now it just shows debug output with the preset names and their assigned colors.
            FileViewModel.Instance.PresetColors = SoundRollBuilder.DefineVoicePalette(sF_Presets);
            // normalize the stereo buffer to prevent clipping
            float[] floatBuffer = NormalizeBuffer(stereoBuffer);
            return floatBuffer;
        }
        /// <summary>
        /// Normalize the output so that the rms value of the nonzero samples becomes 0.5, but clip anything outside of -1 and +1 to prevent distortion. This is a simple normalization approach that can be improved in the future with more advanced techniques like dynamic range compression or limiting. For now it just ensures that the output is not too quiet or too loud on average, while allowing for some variation in the sample values. The buffer is converted to single precision to be compatible with the audio output system, which typically uses 32-bit float samples. 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns>A single precision array that has been normalized</returns>
        /// <exception cref="Exception"></exception>
        private static float[] NormalizeBuffer(double[] buffer)
        {
            double max = 0;
            double rms = 0;
            double sum = 0;
            int count = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                // ignore zeroes so they overload the numbers
                if (buffer[i] != 0)
                {
                    if (double.IsNaN(buffer[i])) throw new Exception($"buffer is undefined at position i={i}");
                    sum += Math.Abs(buffer[i]);
                    max = Math.Max(max, Math.Abs(buffer[i]));
                    rms += buffer[i] * buffer[i];
                    count++;
                }
            }
            float[] floatBuffer = new float[buffer.Length];
            if (count == 0)
            {
                DebugLog.Write($"***** Entire sample buffer is zero. *****");
                return floatBuffer; // return silence
            }
            double average = sum / count;
            rms = (float)Math.Sqrt(rms / count);
            // normalize using rms * 2 so that the samples at the rms value become 0.5, but clip anything outside of -1 and +1
            for (int i = 0; i < buffer.Length; i++)
            {
                floatBuffer[i] /= (float)(rms * 2.0F);
                floatBuffer[i] = Math.Clamp((float)buffer[i], -1.0F, 1.0F);
            }
            DebugLog.Write($"Final audio buffer normalized, average={average}, max={max}, rms={rms}");
            return floatBuffer;
        }
    }


}