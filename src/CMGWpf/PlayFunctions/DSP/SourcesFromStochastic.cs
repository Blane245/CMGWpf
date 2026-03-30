using CMGWpf.Model;
using CMGWpf.Model.Generators;
using CMGWpf.PlayFunctions.Utilities;
using CMGWpf.SoundFont_2;
using CMGWpf.Types;
using CMGWpf.Utilities;
using System.Collections.ObjectModel;
using System.Configuration;
using static CMGWpf.Model.Generators.StochasticTypes;
using static CMGWpf.Types.PlayTypes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

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
        public static string Get(Stochastic? generator, double[] stereoBuffer, List<SF_Preset> sF_Presets)
        {
            if (generator == null)
            {
                return "Algorithmic generator is null.";
            }

            // get all of the needed properties from the generator
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
            double cloudDuration = generator.GetDeltaT();

            // initialize the dynamcis portion of the genrator. 
            generator.InitializeDynamics();
            Random Rn = generator.DynamicsRn;

            if (Nt == 0 | Tc == 0) return $"no composition available in generator {generator}";
            int voiceCount = (int)Math.Round( Tc*(1 + 1/Nt)  * SampleRate); // the total number of samples in a voice (room added for cloud extensions)

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
                    DebugLog.Write($"sourcesfromstochastic: build {nClouds} clouds from voice {voice.Name} @ time cell {iTime}");
                    for (int iCloud = 0; iCloud < nClouds; iCloud++)
                    {
                        // TODO complete
                        (var cloudBuffer, var cloudState) = BuildCloud(generator, cloudDuration, voice, cloudStates[iCloud], iTime * cloudDuration);
                        cloudStates[iCloud] = cloudState; // remember the update state of the cloud

                        // add the cloud to the voice at the proper time
                        int location = (int)Math.Floor(iTime * cloudDuration * SampleRate * 2);
                        AudioBuffer.Add(voiceBuffer, cloudBuffer, location);
                        DebugLog.Write($"sourcesfromstochastic: cloud {iCloud} in time cell {iTime} added to voice {voice.Name}, at voice buffer location {location}");
                    }
                }

                // voice level intensity and pan
                if (generator.IntensityOption == INTENSITYOPTION.voice)
                    Intensity.Apply(voiceBuffer, generator.IntensityTransitionOption, generator.IntensityParameters, generator.DynamicsRn);
                if (generator.PanOption == PANOPTION.voice)
                    Pan.Apply(voiceBuffer, generator.PanAlgorithm, generator.PanParameters, generator.DynamicsRn);

                // add the voice to the audiobuffer
                AudioBuffer.Add(stereoBuffer, voiceBuffer, 0);
            }

            // composition level intensity and pan
            if (generator.IntensityOption == INTENSITYOPTION.composition)
                Intensity.Apply(stereoBuffer, generator.IntensityTransitionOption, generator.IntensityParameters, generator.DynamicsRn);
            if (generator.PanOption == PANOPTION.composition)
                Pan.Apply(stereoBuffer, generator.PanAlgorithm, generator.PanParameters, generator.DynamicsRn);
            return "";
        }

        private static (double[], CloudState) BuildCloud(Stochastic generator, double cloudDuration, Voice voice, CloudState cloudState, double time)
        {
            double delta = generator.Delta;
            bool microTones = generator.Microtones;
            PANOPTION panOption = generator.PanOption;
            PANALGORITHM panAlgorithm = generator.PanAlgorithm;
            PanParameters panParameters = generator.PanParameters;
            INTENSITYOPTION intensityOption = generator.IntensityOption;
            INTENSITYTRANSITIONOPTION intensityTransitionOption = generator.IntensityTransitionOption;
            IntensityParameters intensityParameters = generator.IntensityParameters;
            Random rN = generator.DynamicsRn;
            double lo = voice.RegisterLo;
            double hi = voice.RegisterHi;
            TIMBRE timbre = voice.Timbre;
            Preset? preset = voice.Preset;
            if (preset == null) { DebugLog.Write($"buildcloud: preset for voice={voice.Name} is null."); return ([], new CloudState() { offset = -1, pitch = 0 }); }

            // initialize the cloud state and buffer
            CloudState newCloudState = cloudState;
            int cloudCount = (int)Math.Ceiling(SampleRate * cloudDuration);
            double[] monoSamples = new double[cloudCount * 2]; // allocate single channel twice as large as the time interval to provide for elements running into the next time interval
            for (int i = 0; i < monoSamples.Length; i++) { monoSamples[i] = 0; }

            // create the duration table for the cloud elements
            (var Nd, var Pd) = Probability.Continuous((int)Math.Round(cloudDuration * delta), cloudDuration, cloudDuration / StochasticConstants.UNIT);
            DebugLog.Write($"buildcloud: contunuous distribution for cloud with duration={cloudDuration}, delta={delta}:");
            for (int i = 0; (i < Nd.Length); i++)
            {
                DebugLog.Write($"(Nd,Pd), i={i}, ({Nd[i]}, {Pd[i]})");
            }

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
            while (interval == 0) { interval = Probability.Lookup(Pd, Nd, rN.NextDouble()); }
            DebugLog.Write($"buildcloud: initial conditions for voice {voice.Name}, t1={t1}, pitch1={pitch1}, interval={interval}");

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
                    // get the single channel sample from the preset instrument
                    double duration = voice.Duration == 0 ? interval : voice.Duration;
                    var instrumentSample = InstrumentSample.Get(new InstrumentSampleParameters()
                    {
                        Duration = duration,
                        Interval = interval,
                        StartPitch = pitch1,
                        EndPitch = pitch2,
                        VolumeDb = voice.Volume,
                        Voice = instrument,
                        SampleRate = PlayTypes.SampleRate,
                        SoundFont = voice.SoundFont!
                    });
                    int instrumentStartIndex = (int)(t1 * SampleRate);

                    // add the instrument sample to the single channel voice sample (hopefully will not overflow buffer)
                    AudioBuffer.Add(monoSamples, instrumentSample, instrumentStartIndex);
                    SoundRollBuilder.AddInstrument(new TimeMidiLine
                    {
                        Start = new TimeMidiPoint { Time = t1 + time, Midi = (int)pitch1 },
                        End = new TimeMidiPoint { Time = t1 + time + duration, Midi = (int)pitch2 }
                    }, voice.SoundFontFileName, voice.PresetName);
                }

                // move forward until we are finished with this cloud duration
                if (t1 >= cloudDuration) finished = true;
                else
                {
                    t1 = t2;
                    pitch1 = voice.Timbre == TIMBRE.glissando ? pitch2 : Probability.Interval(hi - lo, rN) + lo;
                    interval = 0;
                    while (interval == 0) interval = Probability.Lookup(Pd, Nd, rN.NextDouble());
                }
            }

            // update the cloud state
            newCloudState.offset = t2 - cloudDuration;
            newCloudState.pitch = pitch2;

            // convert the cloud samples into a 2-channel interleaved sample
            double[] cloudSample = new double[monoSamples.Length * 2];
            for (int iSample = 0; iSample < monoSamples.Length; iSample++)
            {
                cloudSample[2 * iSample] = monoSamples[iSample];
                cloudSample[2 * iSample + 1] = monoSamples[iSample];
            }
            // apply cloud level intensity and pan
            if (intensityOption == INTENSITYOPTION.cloud) Intensity.Apply(cloudSample, intensityTransitionOption, intensityParameters, rN);
            if (panOption == PANOPTION.cloud) Pan.Apply(cloudSample, panAlgorithm, panParameters, rN);
            return (cloudSample, newCloudState);
        }
    }
}
