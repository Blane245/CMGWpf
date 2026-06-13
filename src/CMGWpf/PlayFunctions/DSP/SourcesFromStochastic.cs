using CMGWpf.Model;
using CMGWpf.Model.Generators;
using CMGWpf.PlayFunctions.Utilities;
using CMGWpf.SoundFont_2;
using CMGWpf.Types;
using CMGWpf.Utilities;
using CMGWpf.View;
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
        /// <summary>
        /// Generate the samples for a stochastic generator by looping through each voice and then through each time cell for the voice, building clouds of sounds based on the generator's dynamic parameters and the voice's preset and timbre. The generated samples are added to the final signal at the end of processing all voices. Intensity, Pan, and Reverb are applied during this process. This is done in a separate thread for each generator so that multiple generators can be processed in parallel.
        /// </summary>
        /// <param name="generator"></param>
        public static void Get(Stochastic generator)
        {
            // gather the DSP, etc., information from this generator on a separate thread so that multiple generators can be processed in parallel. 
            DebugLog.Write($"SourcesFromStochastic: Starting stochastic generator processing for generator {generator.Name}...");
            // get all of the needed properties from the generator
            double generatorStartTime = generator.StartTime;
            double generatorStopTime = generator.StopTime;
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

            // initialize the dynamics portion of the generator. 
            generator.InitializeDynamics();
            FastRandom Rn = generator.DynamicsRn;

            if (Nt == 0 | Tc == 0) return;
            int voiceCount = (int)Math.Round(Tc * (1 + 1 / (double)Nt) * SampleRate); // the total number of samples in a voice (room added for cloud extensions)
            double[] compositionBuffer = new double[voiceCount * 2];

            // build the samples from the composition and its characteristics by looping through each voice and then through each time cell for the voice
            // This is done in the order as voices may extend across time cells bounds based on their calculated start and end times
            foreach ((var voice, int iVoice) in voices.Select((v, i) => (v, i)))
            {
                // skip muted voices
                if (voice.Muted) continue;
                if (voice.Preset == null) return;

                // add the voice's soundfont and preset to the global presets list 
                DebugLog.Write($"SourcesFromStochastic: Adding preset {voice.PresetName} from soundfont {voice.SoundFontFileName} to global presets list for voice {voice.Name}...");
                // ConcurrentBag is thread-safe, no lock needed
                PlayViewModel.Instance.GeneratorVoices.Add(new GeneratorVoice()
                {
                    GeneratorName = generator.Name,
                    VoiceName = voice.Name
                });

                // create a stereo buffer to hold the voice's signal
                double[] voiceBuffer = new double[voiceCount * 2];
                DebugLog.Write($"SourcesFromStochastic: Created stereo buffer for voice {voice.Name} with {voiceCount * 2} samples.");
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


                // loop through all of the time cells for this voice, stopping early if the generator has been defined to stop early
                for (int iTime = 0; iTime < Nt && iTime * cloudDuration < generatorStopTime - generatorStartTime; iTime++)
                {
                    // generate the audio for all clouds at this time, tracking the cloud state at the end of each cloud
                    int nClouds = composition[iTime][iVoice];
                    for (int iCloud = 0; iCloud < nClouds; iCloud++)
                    {
                        (var cloudBuffer, var cloudState) = BuildCloud(generator, cloudDuration, voice, cloudStates[iCloud], iTime * cloudDuration);
                        cloudStates[iCloud] = cloudState; // remember the update state of the cloud

                        // add the cloud to the voice at the proper time
                        int location = (int)Math.Floor(iTime * cloudDuration * SampleRate * 2);
                        voiceBuffer = AudioBuffer.Add(cloudBuffer, voiceBuffer, location);
                    }
                }

                // voice level intensity and pan
                if (generator.IntensityOption == INTENSITYOPTION.voice) Intensity.Apply(voiceBuffer, generator.IntensityTransitionOption, generator.IntensityParameters, generator.DynamicsRn);
                if (generator.PanOption == PANOPTION.voice) Pan.Apply(voiceBuffer, generator.PanAlgorithm, generator.PanParameters, generator.DynamicsRn);
                // add the voice to the composition buffer
                compositionBuffer = AudioBuffer.Add(voiceBuffer, compositionBuffer, 0);
            }

            // composition level intensity, pan, and reverb, if any
            if (generator.IntensityOption == INTENSITYOPTION.composition) Intensity.Apply(compositionBuffer, generator.IntensityTransitionOption, generator.IntensityParameters, generator.DynamicsRn);
            if (generator.PanOption == PANOPTION.composition) Pan.Apply(compositionBuffer, generator.PanAlgorithm, generator.PanParameters, generator.DynamicsRn);
            Reverb.Apply(compositionBuffer, reverbDelay, reverbDecay, sampleRate);

            // update final signal
            bool lockTaken = false;
            try
            {
                Monitor.Enter(PlayViewModel.Instance.PlayResultsLock, ref lockTaken);
                // now move the composition to the stereo buffer at the start time of the composition
                PlayViewModel.Instance.FinalSignal.Add(compositionBuffer, (int)(generatorStartTime * sampleRate * 2));
            }
            finally
            {
                if (lockTaken) Monitor.Exit(PlayViewModel.Instance.PlayResultsLock);
            }
        }

        /// <summary>
        /// Create a cloud of sounds based on the generator's dynamic parameters and the voice's preset and timbre. The cloud is built by placing elements based on the duration table for the cloud, where each element may be a sustained pitch or a glissando between two pitches based on the voice's timbre. The cloud state is updated at the end of the cloud so that clouds can extend across time cell boundaries.
        /// </summary>
        /// <param name="generator" type="Stochastic">The stochastic generator containing dynamic parameters.</param>
        /// <param name="cloudDuration" type="double">The duration of the cloud in seconds.</param, aka DeltaT>
        /// <param name="voice" type="Voice">The voice containing preset and timbre information.</param>
        /// <param name="cloudState" type="CloudState">The current state of the cloud.</param>
        /// <param name="cellTime" type="double">The time of the current cell. This is some multiple of cloudDuration.</param>
        /// <returns>(double[], CloudState) The generated cloud samples (stereo) and the updated cloud state.</returns>
        private static (double[], CloudState) BuildCloud(Stochastic generator, double cloudDuration, Voice voice, CloudState cloudState, double cellTime)
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
            FastRandom rN = generator.DynamicsRn;
            double lo = voice.RegisterLo;
            double hi = voice.RegisterHi;
            TIMBRE timbre = voice.Timbre;
            Preset? preset = voice.Preset;
            if (preset == null)
            {
                DebugLog.Write($"BuildCloud: preset for voice={voice.Name} is null.");
                return ([], new CloudState() { offset = -1, pitch = 0 });
            }

            // initialize the cloud state
            CloudState newCloudState = cloudState;

            // allocate single channel twice as large as the time interval to provide for elements running into the next time interval. This may get extended by long running release times, but that is not currently accounted for as we do not know yet how long the releases might be. See AudioBuffer.Add.
            int cloudCount = (int)Math.Ceiling(SampleRate * cloudDuration);
            double[] monoSamples = new double[cloudCount * 2];
            for (int i = 0; i < monoSamples.Length; i++) { monoSamples[i] = 0; }

            // create the duration table for the cloud elements
            (var Nd, var Pd) = Probability.Continuous((int)Math.Round(cloudDuration * delta), cloudDuration, cloudDuration / StochasticConstants.UNIT);

            // check that not all duration are 0
            if (Pd.Length <= 1)
            {
                DebugLog.Write($"BuildCloud: duration table for timber={timbre}, deltaT={delta}, duration={cloudDuration} has no or only zero values");
                return ([], new CloudState() { offset = -1, pitch = 0 });
            }

            // initialize the starting time and starting pitch based on the current cloud state and the microtone option
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
            if (interval == 0)
            {
                DebugLog.Write($"BuildCloud: could not find non-zero interval for voice {voice.Name}, time={cellTime}, t1={t1}, pitch1={pitch1}. Skipping this cloud.");
                return ([], new CloudState() { offset = -1, pitch = 0 });
            }
            while (!finished)
            {
                t2 = t1 + interval;

                // get a glissando using a Gaussian random speed and the current interval, or get a sustained pitch
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
                        VolumeDb = voice.Volume + generator.Parent.Volume,
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
                    double instrumentEndTime = t1 + instrumentSample.Length / (double)SampleRate;
                    source.Generator = generator;
                    source.StartTime = generator.StartTime + cellTime + t1;
                    source.StopTime = generator.StartTime + cellTime + instrumentEndTime;
                    source.SoundFontName = voice.SoundFontFileName;
                    source.PresetName = voice.PresetName;
                    source.Name = instrument.InstrumentName;

                    // Use concurrent collections - no lock needed
                    PlayViewModel.Instance.InstrumentSources.Add(source);
                    PlayViewModel.Instance.TimeMidiVoices.Add(new TimeMidiVoice()
                    {
                        Line = new TimeMidiLine
                        {
                            Start = new TimeMidiPoint { Time = source.StartTime, Midi = (int)pitch1 },
                            End = new TimeMidiPoint { Time = source.StopTime, Midi = (int)pitch2 }
                        },
                        GeneratorName = generator.Name,
                        VoiceName = voice.Name,
                    });

                    // calculate the start index for the instrument sample in the cloud sample based on the current element start time (t1) and the cloud start time (time)
                    int instrumentStartIndex = (int)(t1 * SampleRate);
                    // add the instrument sample to the single channel voice sample
                    monoSamples = AudioBuffer.Add(instrumentSample, monoSamples, instrumentStartIndex);
                }

                // move forward until we are finished with this cloud duration
                if (t2 >= cloudDuration) finished = true;
                else
                {
                    t1 = t2;
                    pitch1 = voice.Timbre == TIMBRE.glissando ? pitch2 : Probability.Interval(hi - lo, rN) + lo;
                    if (!microTones) pitch1 = Math.Round(pitch1);
                    interval = 0;
                    int iteration2 = 0;
                    while (interval == 0 && iteration2 < 100)
                    {
                        interval = Probability.Lookup(Pd, Nd, rN.NextDouble());
                        iteration2++;
                    }
                    if (interval == 0)
                    {
                        DebugLog.Write($"BuildCloud: could not find non-zero interval for voice {voice.Name}, time={cellTime}, t1={t1}, pitch1={pitch1}. Ending this cloud.");
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
            if (reverbDecay > 0 && reverbDelay > 0) Reverb.Apply(cloudSample, reverbDelay, reverbDecay, PlayTypes.SampleRate);
            return (cloudSample, newCloudState);
        }
    }
}
