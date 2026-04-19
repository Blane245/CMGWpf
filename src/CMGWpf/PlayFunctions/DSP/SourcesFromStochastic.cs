using CMGWpf.Model;
using CMGWpf.Model.Generators;
using CMGWpf.PlayFunctions.Utilities;
using CMGWpf.SoundFont_2;
using CMGWpf.Types;
using CMGWpf.Utilities;
using System.Collections.ObjectModel;
using static CMGWpf.Model.Generators.StochasticTypes;
using static CMGWpf.Types.PlayTypes;

namespace CMGWpf.PlayFunctions.DSP
{
    public static class StochasticConstants
    {
        public const int UNIT = 100;
        public const double RMSFACTOR = 2;
        public const double DELTAPAN = 0.3;
    }
    public static class SourcesFromStochastic
    {
        public class CloudState
        {
            public double offset;
            public double pitch;
        }
        public static string Get(Stochastic? generator, ref double[] stereoBuffer, List<SF_Preset> sF_Presets, ObservableCollection<InstrumentSource> sources)
        {
            if (generator == null)
            {
                return "Stochastic generator is null.";
            }

            // get all of the needed properties from the generator
            double generatorStartTime = generator.StartTime;
            int Nt = generator.NumberOfTimeCells;
            double Tc = generator.CompositionDuration;
            ObservableCollection<Voice> voices = generator.Voices;
            Composition composition = generator.Composition;
            PANOPTION panOption = generator.PanOption;
            PANALGORITHM panAlgorithm = generator.PanAlgorithm;
            PanParameters panParameters = generator.PanParameters;
            INTENSITYOPTION intensityOption = generator.IntensityOption;
            INTENSITYTRANSITIONOPTION intensityTransitionOption = generator.IntensityTransitionOption;
            IntensityParameters intensityParameters = generator.IntensityParameters;
            int sampleRate = PlayTypes.SampleRate;
            double reverbDecay = generator.ReverbParameters.Decay;
            double reverbDelay = generator.ReverbParameters.Delay;
            double cloudDuration = generator.GetDeltaT();

            // initialize the dynamcis portion of the genrator. 
            generator.InitializeDynamics();
            FastRandom Rn = generator.DynamicsRn;

            if (Nt == 0 | Tc == 0) return $"no composition available in generator {generator}";
            int voiceCount = (int)Math.Round(Tc * (1 + 1 / (double)Nt) * SampleRate); // the total number of samples in a voice (room added for cloud extensions)
            double[] compositionBuffer = new double[voiceCount * 2];

            // build the samples from the composition and its charateristics by looping through each voice and then throug each time cell for the voice
            // This is done in the order as voices may extend across time cells bounds based on thier calculated start and end times
            foreach ((var voice, int iVoice) in voices.Select((v, i) => (v, i)))
            {
                // skip muted voices
                if (voice.Muted) continue;
                if (voice.Preset == null) return $"Voice ${voice.Name} is missing its preset";
                // initialize the random number generator for this voice

                // create a stereo buffer to hold the voice's signal
                double[] voiceBuffer = new double[voiceCount * 2];
                // initialize the could states so that clouds can extend from one time cell to the next. We first need to know the maximum number of clouds in the voice for all times
                int maxCloud = 0;
                for (int iTime = 0; iTime < Nt; iTime++)
                {
                    maxCloud = Math.Max(maxCloud, composition[iTime][iVoice]);
                }
                CloudState[] cloudStates = new CloudState[maxCloud];
                for (int iCloud = 0; iCloud < maxCloud; iCloud++)
                {
                    cloudStates[iCloud] = new()
                    {
                        offset = -1,
                        pitch = 0,
                    };
                }
                if (maxCloud == 0) continue; // there are no clouds in this voice for any time

                // add the voice's soundfont and preset to the presets list
                sF_Presets.Add(new SF_Preset()
                {
                    SoundFontName = voice.SoundFontFileName,
                    PresetName = voice.PresetName
                });

                // loop through all of the time cells for this voice
                for (int iTime = 0; iTime < Nt; iTime++)
                {
                    // generate the audio for all clouds at this time, tracking the cloud state at the end of each cloud
                        int nClouds = composition[iTime][iVoice];
                        for (int iCloud = 0; iCloud < nClouds; iCloud++)
                        {
                            (var cloudBuffer, var cloudState) = BuildCloud(generator, cloudDuration, voice, cloudStates[iCloud], iTime * cloudDuration, sources);
                            cloudStates[iCloud] = cloudState; // remember the update state of the cloud

                            // add the cloud to the voice at the proper time
                            int location = (int)Math.Floor(iTime * cloudDuration * SampleRate * 2);
                            AudioBuffer.Add(cloudBuffer, ref voiceBuffer, location);
                    }
                }

                // voice level intensity and pan
                if (generator.IntensityOption == INTENSITYOPTION.voice) Intensity.Apply(voiceBuffer, generator.IntensityTransitionOption, generator.IntensityParameters, generator.DynamicsRn);
                if (generator.PanOption == PANOPTION.voice) Pan.Apply(voiceBuffer, generator.PanAlgorithm, generator.PanParameters, generator.DynamicsRn);
                // add the voice to the composition buffer
                AudioBuffer.Add(voiceBuffer, ref compositionBuffer, 0);
            }

            // composition level intensity, pan, and reverb, if any
            if (generator.IntensityOption == INTENSITYOPTION.composition) Intensity.Apply(compositionBuffer, generator.IntensityTransitionOption, generator.IntensityParameters, generator.DynamicsRn);
            if (generator.PanOption == PANOPTION.composition) Pan.Apply(compositionBuffer, generator.PanAlgorithm, generator.PanParameters, generator.DynamicsRn);
            Reverb.Apply(compositionBuffer, reverbDelay, reverbDecay, sampleRate);

            // now move the composition to the stereo buffer at the starttime of the composition
            AudioBuffer.Add(compositionBuffer, ref stereoBuffer, (int)(generatorStartTime * sampleRate * 2));
            return "";
        }

        /// <summary>
        /// Create a cloud of sounds based on the generator's dynamic parameters and the voice's preset and timbre. The cloud is built by placing elements based on the duration table for the cloud, where each element may be a sustained pitch or a glissando between two pitches based on the voice's timbre. The cloud state is updated at the end of the cloud so that clouds can extend across time cell boundaries.
        /// </summary>
        /// <param name="generator" type="Stochastic">The stochastic generator containing dynamic parameters.</param>
        /// <param name="cloudDuration" type="double">The duration of the cloud in seconds.</param, aka DeltaT>
        /// <param name="voice" type="Voice">The voice containing preset and timbre information.</param>
        /// <param name="cloudState" type="CloudState">The current state of the cloud.</param>
        /// <param name="cellTime" type="double">The time of the current cell. This is some multiple of cloudDuration.</param>
        /// <param name="sources" type="ObservableCollection<InstrumentSource>">The collection of instrument sources.</param>
        /// <returns>(double[], CloudState) The generated cloud samples (stereo) and the updated cloud state.</returns>
        private static (double[], CloudState) BuildCloud(Stochastic generator, double cloudDuration, Voice voice, CloudState cloudState, double cellTime, ObservableCollection<InstrumentSource> sources)
        {
            double delta = generator.Delta;
            bool microTones = generator.Microtones;
            PANOPTION panOption = generator.PanOption;
            PANALGORITHM panAlgorithm = generator.PanAlgorithm;
            PanParameters panParameters = generator.PanParameters;
            INTENSITYOPTION intensityOption = generator.IntensityOption;
            INTENSITYTRANSITIONOPTION intensityTransitionOption = generator.IntensityTransitionOption;
            IntensityParameters intensityParameters = generator.IntensityParameters;
            double reverbDecay = generator.ReverbParameters.Decay;
            double reverbDelay = generator.ReverbParameters.Delay;
            int sampleRate = PlayTypes.SampleRate;
            FastRandom rN = generator.DynamicsRn;
            double lo = voice.RegisterLo;
            double hi = voice.RegisterHi;
            TIMBRE timbre = voice.Timbre;
            Preset? preset = voice.Preset;
            if (preset == null) { DebugLog.Write($"buildcloud: preset for voice={voice.Name} is null."); return ([], new CloudState() { offset = -1, pitch = 0 }); }

            // initialize the cloud state
            CloudState newCloudState = cloudState;

            // allocate single channel twice as large as the time interval to provide for elements running into the next time interval. This may get extended by long running release times, but that is not currently accounted for as we do not know yet how long the releases might be. See AudioBuffer.Add.
            int cloudCount = (int)Math.Ceiling(SampleRate * cloudDuration);
            double[] monoSamples = new double[cloudCount * 2];
            for (int i = 0; i < monoSamples.Length; i++) { monoSamples[i] = 0; }

            // create the duration table for the cloud elements
            (var Nd, var Pd) = Probability.Continuous((int)Math.Round(cloudDuration * delta), cloudDuration, cloudDuration / StochasticConstants.UNIT);

            // check that not all duration are 0
            if (Pd.Length <= 1) { DebugLog.Write($"buildcloud: duration table for timber={timbre}, deltaT={delta}, duration={cloudDuration} has no or only zero values"); return ([], new CloudState() { offset = -1, pitch = 0 }); }

            // initialize the starting time and starting pitch based on the current cloud state and the microtones option
            double t1 = cloudState.offset < 0 ?
                Probability.Lookup(Pd, Nd, rN.NextDouble()) :
                cloudState.offset;
            double pitch1 = cloudState.offset < 0 ?
                Probability.Interval(hi - lo, rN) + lo :
                cloudState.pitch;
            if (!microTones) pitch1 = Math.Round(pitch1);

            // initialize the end time, pitch, and whether or not we are finished placing elements in this cloud
            double t2 = 0;
            double pitch2 = 0;
            bool finished = false;
            double interval = 0;
            int iteration = 0;
            while (interval == 0 && iteration < 100) { interval = Probability.Lookup(Pd, Nd, rN.NextDouble()); iteration++; }
            if (interval == 0) {
                DebugLog.Write($"buildcloud: could not find non-zero interval for voice {voice.Name}, time={cellTime}, t1={t1}, pitch1={pitch1}. Skipping this cloud."); 
                return ([], new CloudState() { offset = -1, pitch = 0 }); 
            }
            while (!finished)
            {
                t2 = t1 + interval;

                // get a glissando using a gaussian random speed and the current interval, or get a sustained pitch
                if (timbre == TIMBRE.glissando)
                {
                    double speed = Probability.GaussianRandom(0, delta * StochasticConstants.RMSFACTOR, rN);
                    pitch2 = Math.Clamp(pitch1 + speed * interval, lo, hi);
                    if (!microTones) pitch2 = Math.Round(pitch2);
                }
                else pitch2 = pitch1;

                // get all of the instruments for the voice (preset) and process each individually
                List<FinalVoice> instruments = PresetUtilities.BuildVoicesForPresetAtKeyVel(preset, (int)pitch1, (int)voice.Velocity);
                foreach (var instrument in instruments)
                {
                    // get the single channel sample and the source data from the preset instrument
                    double duration = voice.Duration == 0 ? interval : voice.Duration;
                    (double[] instrumentSample, InstrumentSource source) = InstrumentSample.Get(new InstrumentSampleParameters()
                    {
                        Duration = duration,
                        Interval = interval,
                        StartPitch = pitch1,
                        EndPitch = pitch2,
                        VolumeDb = voice.Volume,
                        Voice = instrument,
                        SampleRate = PlayTypes.SampleRate,
                        SoundFont = voice.SoundFont!,
                        // the following parameters are not currently used in the stochastic sample generation, but may be used in the future when we add more complex samples with noise, attack and release, and modulation
                        AttackEnabled = true,
                        LoopEnabled = true,
                        NoiseEnabled = false,
                        NoiseFrequency = 0,
                        NoiseAmplitude = 0,
                        Tremolo = new Tremolo() { Depth = 0, Speed = 0 },
                        Vibrato = new Tremolo() { Depth = 0, Speed = 0 },
                    });

                    // complete the source definition and add to the sources collection
                    source.Generator = generator;
                    source.StartTime = generator.StartTime + cellTime + t1;
                    source.StopTime = generator.StartTime + cellTime + t1 + duration;
                    source.SoundFontName = voice.SoundFontFileName;
                    source.PresetName = voice.PresetName;
                    source.Name = instrument.InstrumentName;
                    sources.Add(source);

                    // calculate the start index for the instrument sample in the cloud sample based on the current element start time (t1) and the cloud start time (time)
                    int instrumentStartIndex = (int)(t1 * SampleRate);

                    // add the instrument sample to the single channel voice sample
                    AudioBuffer.Add(instrumentSample, ref monoSamples, instrumentStartIndex);
                    SoundRollBuilder.AddInstrument(new TimeMidiLine
                    {
                        Start = new TimeMidiPoint { Time = source.StartTime, Midi = (int)pitch1 },
                        End = new TimeMidiPoint { Time = source.StopTime, Midi = (int)pitch2 }
                    }, voice.SoundFontFileName, voice.PresetName);
                }

                // move forward until we are finished with this cloud duration
                if (t2 >= cloudDuration) finished = true;
                else
                {
                    t1 = t2;
                    pitch1 = voice.Timbre == TIMBRE.glissando ? pitch2 : Probability.Interval(hi - lo, rN) + lo;
                    interval = 0;
                    int iteration2 = 0;
                    while (interval == 0 && iteration2 < 100) 
                    {
                        interval = Probability.Lookup(Pd, Nd, rN.NextDouble());
                        iteration2++;
                    }
                    if (interval == 0)
                    {
                        DebugLog.Write($"buildcloud: could not find non-zero interval for voice {voice.Name}, time={cellTime}, t1={t1}, pitch1={pitch1}. Ending this cloud.");
                        finished = true;
                    }
                }
            }

            // update the cloud state
            newCloudState.offset = t2 % cloudDuration;
            newCloudState.pitch = pitch2;

            // convert the cloud samples into a 2-channel interleaved sample
            double[] cloudSample = new double[monoSamples.Length * 2];
            for (int iSample = 0; iSample < monoSamples.Length; iSample++)
            {
                cloudSample[2 * iSample] = monoSamples[iSample];
                cloudSample[2 * iSample + 1] = monoSamples[iSample];
            }
            // apply cloud level intensity, pan and reverb to the cloud sample
            if (intensityOption == INTENSITYOPTION.cloud) Intensity.Apply(cloudSample, intensityTransitionOption, intensityParameters, rN);
            if (panOption == PANOPTION.cloud) Pan.Apply(cloudSample, panAlgorithm, panParameters, rN);
            return (cloudSample, newCloudState);
        }
    }
}
