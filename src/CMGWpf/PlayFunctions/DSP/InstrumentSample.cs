
using CMGWpf.Model;
using CMGWpf.PlayFunctions.Utilities;
using CMGWpf.SoundFont_2;
using CMGWpf.Utilities;
using static CMGWpf.Types.PlayTypes;
using static CMGWpf.Types.PresetTypes;

namespace CMGWpf.PlayFunctions.DSP
{
    public class InstrumentSampleParameters
    {
        // Timing
        public required double Duration { get; init; } = 0;
        public required double Interval { get; init; } = 0;

        // Pitch
        public required double StartPitch { get; init; } = 0;
        public required double EndPitch { get; init; } = 0;

        // Volume
        public required double VolumeDb { get; init; } = 0;

        // Envelope and Loop settings
        public bool AttackEnabled { get; init; } = true;
        public bool LoopEnabled { get; init; } = true;

        // Noise
        public bool NoiseEnabled { get; init; } = false;
        public double NoiseFrequency { get; init; } = 0;
        public double NoiseAmplitude { get; init; } = 0;

        // Tremolo
        public Tremolo Tremolo { get; init; } = new Tremolo();

        // Vibrato
        public Tremolo Vibrato { get; init; } = new Tremolo();

        // Voice and Sample Rate
        public required FinalVoice? Voice { get; init; } = null;
        public required int SampleRate { get; init; } = 0;

        // SoundFont sample data
        public required SoundFont? SoundFont { get; init; } = null;
    }

    public static class InstrumentSample
    {
        /// <summary>
        /// Given a note with its instrument characteristics, build the audio sample
        /// </summary>
        /// <param name="parameters" type="InstrumentSampleParameters">All parameters needed to generate the instrument sample</param>
        /// <returns>Mono audio sample and the instrument source data with real time information </returns>
        public static (double[], InstrumentSource) Get(InstrumentSampleParameters parameters)
        {
            // Deconstruct parameters into local variables for easier access
            var interval = parameters.Interval;
            var duration = parameters.Duration;
            var startPitch = parameters.StartPitch;
            var endPitch = parameters.EndPitch;
            var volumeDb = parameters.VolumeDb;
            var attackEnabled = parameters.AttackEnabled;
            var loopEnabled = parameters.LoopEnabled;
            var noiseEnabled = parameters.NoiseEnabled;
            var noiseFrequency = parameters.NoiseFrequency;
            var noiseAmplitude = parameters.NoiseAmplitude;
            var tremolo = parameters.Tremolo;
            var vibrato = parameters.Vibrato;
            var voice = parameters.Voice;
            var outputSampleRate = parameters.SampleRate;
            var soundFont = parameters.SoundFont;

            DebugLog.Write($"=== InstrumentSample Parameters ===");
            DebugLog.Write($"Duration: {duration:F3}s, Interval: {interval:F3}s");
            DebugLog.Write($"Pitch: {startPitch:F2} -> {endPitch:F2}, Volume: {volumeDb:F2}dB");
            DebugLog.Write($"AttackEnabled: {attackEnabled}, LoopEnabled: {loopEnabled}");
            DebugLog.Write($"Noise: Enabled={noiseEnabled}, Freq={noiseFrequency:F2}, Amp={noiseAmplitude:F2}");
            DebugLog.Write($"Tremolo: Speed={tremolo.Speed:F2}, Depth={tremolo.Depth:F2}, Wave={tremolo.WaveForm}");
            DebugLog.Write($"Vibrato: Speed={vibrato.Speed:F2}, Depth={vibrato.Depth:F2}, Wave={vibrato.WaveForm}");
            DebugLog.Write($"SampleRate: {outputSampleRate}Hz");

            // Deconstruct voice.SampleHeader properties
            var sampleName = voice!.SampleHeader!.Name;
            var sampleStart = voice.SampleHeader.Start;
            var sampleEnd = voice.SampleHeader.End;
            var sampleStartLoop = voice.SampleHeader.StartLoop;
            var sampleEndLoop = voice.SampleHeader.EndLoop;
            var inputSampleRate = voice.SampleHeader.SampleRate;
            var sampleOriginalPitch = voice.SampleHeader.OriginalPitch;
            var samplePitchCorrection = voice.SampleHeader.PitchCorrection;

            DebugLog.Write($"=== Sample Header: {sampleName} ===");
            DebugLog.Write($"Range: {sampleStart}-{sampleEnd} (length: {sampleEnd - sampleStart})");
            DebugLog.Write($"Loop: {sampleStartLoop}-{sampleEndLoop}");
            DebugLog.Write($"SampleRate: {inputSampleRate}Hz, OriginalPitch: {sampleOriginalPitch}, PitchCorrection: {samplePitchCorrection} cents");

            // start a random number generator for adding noise to the sample
            FastRandom random = MathUtilities.StartFastRandom(null);

            // Get the actual sample data from the SoundFont
            var waveData = soundFont!.WaveData;
            double[] instrumentSample = new double[sampleEnd - sampleStart];
            for (int i = sampleStart; i < sampleEnd; i++)
            {
                instrumentSample[i - sampleStart] = (double)waveData[i] / 32768.0;
            }

            // Get all of the generator values and convert to standard units
            short attackTc = voice.Generators.GetValueOrDefault(GenOp.attackVolEnv, Sf2Defaults.GetDefault(GenOp.attackVolEnv));
            double attackSeconds = Sf2Units.TimecentsToSeconds(attackTc);
            short delayTc = voice.Generators.GetValueOrDefault(GenOp.delayVolEnv, Sf2Defaults.GetDefault(GenOp.delayVolEnv));
            double delaySeconds = Sf2Units.TimecentsToSeconds(delayTc);
            short holdTc = voice.Generators.GetValueOrDefault(GenOp.holdVolEnv, Sf2Defaults.GetDefault(GenOp.holdVolEnv));
            double holdSeconds = Sf2Units.TimecentsToSeconds(holdTc);
            short decayTc = voice.Generators.GetValueOrDefault(GenOp.decayVolEnv, Sf2Defaults.GetDefault(GenOp.decayVolEnv));
            double decaySeconds = Sf2Units.TimecentsToSeconds(decayTc);
            short releaseTc = voice.Generators.GetValueOrDefault(GenOp.releaseVolEnv, Sf2Defaults.GetDefault(GenOp.releaseVolEnv));
            double releaseSeconds = Sf2Units.TimecentsToSeconds(releaseTc);
            short initialAttenuationCb = voice.Generators.GetValueOrDefault(GenOp.initialAttenuation, Sf2Defaults.GetDefault(GenOp.initialAttenuation));
            double initialAttenuationGain = Sf2Units.AttenuationCbToGain(-initialAttenuationCb);
            short sustainVolEnv = voice.Generators.GetValueOrDefault(GenOp.sustainVolEnv, Sf2Defaults.GetDefault(GenOp.sustainVolEnv));
            short overridingRootKey = voice.Generators.GetValueOrDefault(GenOp.overridingRootKey, Sf2Defaults.GetDefault(GenOp.overridingRootKey));
            short fineTune = voice.Generators.GetValueOrDefault(GenOp.fineTune, Sf2Defaults.GetDefault(GenOp.fineTune));
            short coarseTune = voice.Generators.GetValueOrDefault(GenOp.coarseTune, Sf2Defaults.GetDefault(GenOp.coarseTune));
            short endAddrsCoarseOffset = voice.Generators.GetValueOrDefault(GenOp.endAddrsCoarseOffset, Sf2Defaults.GetDefault(GenOp.endAddrsCoarseOffset));
            short startAddrsCoarseOffset = voice.Generators.GetValueOrDefault(GenOp.startAddrsCoarseOffset, Sf2Defaults.GetDefault(GenOp.startAddrsCoarseOffset));
            short startAddrOffset = voice.Generators.GetValueOrDefault(GenOp.startAddrOffset, Sf2Defaults.GetDefault(GenOp.startAddrOffset));
            short endAddrOffset = voice.Generators.GetValueOrDefault(GenOp.endAddrOffset, Sf2Defaults.GetDefault(GenOp.endAddrOffset));
            short sampleModes = voice.Generators.GetValueOrDefault(GenOp.sampleModes, Sf2Defaults.GetDefault(GenOp.sampleModes));

            DebugLog.Write($"=== Generator Values ===");
            DebugLog.Write($"Raw Generator Dictionary Contents:");
            foreach (var gen in voice.Generators)
            {
                DebugLog.Write($"  {gen.Key} = {gen.Value}");
            }
            DebugLog.Write($"Envelope (tc): Delay={delayTc}, Attack={attackTc}, Hold={holdTc}, Decay={decayTc}, Release={releaseTc}");
            DebugLog.Write($"Envelope (s): Delay={delaySeconds:F3}, Attack={attackSeconds:F3}, Hold={holdSeconds:F3}, Decay={decaySeconds:F3}, Release={releaseSeconds:F3}");
            DebugLog.Write($"Attenuation: {initialAttenuationCb}cB -> {initialAttenuationGain:F3} gain");
            DebugLog.Write($"SustainVolEnv: {sustainVolEnv}cB");
            DebugLog.Write($"Tuning: coarse={coarseTune}, fine={fineTune}, overridingRoot={overridingRootKey}");
            DebugLog.Write($"Address offsets: start={startAddrOffset}+{startAddrsCoarseOffset}*32k, end={endAddrOffset}+{endAddrsCoarseOffset}*32k");
            DebugLog.Write($"SampleModes: {sampleModes}");

            // get the starting and ending cents
            double rootKey = overridingRootKey != -1 ? overridingRootKey : sampleOriginalPitch;
            double baseDetune = 100 * rootKey + samplePitchCorrection + fineTune + 100 * coarseTune;

            // get that starting and ending cents for the interval
            double startCents = startPitch * 100 - baseDetune;
            double endCents = endPitch * 100 - baseDetune;

            // resolve the instrument looping behavior considering the generator may override the instrument looping mode
            int loopStart = 0;
            int loopEnd = 0;
            bool loop;
            int loopLength = 0;
            if (sampleModes == 1)
            {
                loopStart = sampleStartLoop + startAddrsCoarseOffset * 32768 + startAddrOffset - sampleStart;
                loopEnd = Math.Clamp(sampleEndLoop + endAddrsCoarseOffset * 32768 + endAddrOffset - sampleStart, loopStart, instrumentSample.Length);
                loop = loopEnabled;
                loopLength = loopEnd - loopStart;
            }
            else loop = false;

            // determine the base playback rate from the instrument's sample rate and the system's output sample rate. This seems upside down but the formula is correct. If the sample rate is higher than the system sample rate, we need to play it back slower to get the correct pitch, and if it's lower, we need to play it back faster. So the formula is system sample rate divided by sample rate.
            double baseRatio = (double)outputSampleRate / (double)inputSampleRate;
            // build the envelope for the note. This will be used to modulate the volume of the note over time. The envelope will be built based on the attack, decay, sustain, and release times, and the initial attenuation.
            double delayEnd = attackEnabled ? delaySeconds : 0;
            double attackEnd = delayEnd + (attackEnabled ? attackSeconds : 0);
            double holdEnd = attackEnd + holdSeconds;
            double decayEnd = holdEnd + decaySeconds;
            // these last two numbers may be less than the others depending on the duration of the note. These special cases are handled below.
            double noteInterval = Math.Min(duration, interval);
            if (!loop) noteInterval = Math.Min(noteInterval, (double)instrumentSample.Length / inputSampleRate);
            double noteEnd = noteInterval;
            // if the note is staccato, drop the release phase and end the note noteEnd seconds.
            // also, if the release is very short, cut it off
            double releaseEnd = (duration == interval) ? noteEnd + releaseSeconds : noteEnd;

            double volumeGain = Sf2Units.VolumeDbToGain(volumeDb);
            static double Attenuate(double gain, double dB)
            {
                if (dB <= 0) return gain;
                return gain * Math.Pow(10, -dB / 20.0);
            }
            ;
            // Calculate sustain gain with clamped attenuation to handle extreme SoundFont values
            double sustainGain = Attenuate(volumeGain * initialAttenuationGain, sustainVolEnv / 10.0);

            DebugLog.Write($"=== Calculated Gains ===");
            DebugLog.Write($"SustainVolEnv: {sustainVolEnv} cB ({sustainVolEnv / 10.0:F1} dB raw");
            DebugLog.Write($"Cents: Start: {startCents:F2} End: {endCents:F4}");
            DebugLog.Write($"Loop: {loop}, Start: {loopStart} End: {loopEnd}, Sample End: {sampleEnd - sampleStart}");
            DebugLog.Write($"VolumeDb: {volumeDb:F2}dB -> VolumeGain: {volumeGain:F4}");
            DebugLog.Write($"Initial Attenuation Gain: {initialAttenuationGain:F4}");
            DebugLog.Write($"Peak Gain (vol * initAtten): {volumeGain * initialAttenuationGain:F4}");
            DebugLog.Write($"Sustain Gain: {sustainGain:F4}");
            DebugLog.Write($"Envelope times - Delay end:{delayEnd:F3}, Attack end:{attackEnd:F3}, Hold end:{holdEnd:F3}, Decay end:{decayEnd:F3}, NoteEnd:{noteEnd:F3}, ReleaseEnd:{releaseEnd:F3}");

            // build the gain envelope
            GainEnvelope[] envelope = [];
            // there are several special case based on when the note ends relative to the other parts of the envelope
            double noteEndGain;
            // the envelope always starts at 0,0
            envelope = [
                new GainEnvelope { Gain = 0, Time = 0 },
                ];
            if (noteEnd < delayEnd) return ([], new InstrumentSource()); // the output will be all 0s, which is silence
            else if (noteEnd < attackEnd)  // there is a delay and a partial attack phase and maybe a release
            {
                noteEndGain = Interpolation.Linear(noteEnd, delayEnd, attackEnd, 0, 1) * volumeGain * initialAttenuationGain;
                envelope = [.. envelope, new GainEnvelope { Gain = 0, Time = delayEnd }];
                envelope = [.. envelope, new GainEnvelope { Gain = noteEndGain, Time = noteEnd }];
                if (noteEnd != releaseEnd) envelope = [.. envelope, new GainEnvelope { Gain = 0, Time = releaseEnd }];
            }
            else // attack is complete - add the delay and attach phases to the envelope and check for the other phases
            {
                envelope = [.. envelope, new GainEnvelope { Gain = 0, Time = delayEnd }];
                envelope = [.. envelope, new GainEnvelope { Gain = volumeGain * initialAttenuationGain, Time = attackEnd }];
                noteEndGain = volumeGain * initialAttenuationGain;
                if (noteEnd < holdEnd) // there is a partial hold phase and maybe a release. No decay or sustain phase since the note ends before the hold phase is complete
                {
                    envelope = [.. envelope, new GainEnvelope { Gain = noteEndGain, Time = noteEnd }];
                    if (noteEnd != releaseEnd) envelope = [.. envelope, new GainEnvelope { Gain = 0, Time = releaseEnd }];
                }
                else if (noteEnd < decayEnd) // there is a partial decay phase and maybe a release but no sustain phase
                {
                    envelope = [.. envelope, new GainEnvelope { Gain = noteEndGain, Time = holdEnd }];
                    noteEndGain = Interpolation.Linear(noteEnd, holdEnd, decayEnd, volumeGain * initialAttenuationGain, sustainGain);
                    envelope = [.. envelope, new GainEnvelope { Gain = noteEndGain, Time = noteEnd }];
                    if (noteEnd != releaseEnd) envelope = [.. envelope, new GainEnvelope { Gain = 0, Time = releaseEnd }];
                }
                else
                { // decay completes before the end of the note, so stop at decayEnd as gain has dropped to sustain gain
                    envelope = [.. envelope, new GainEnvelope { Gain = noteEndGain, Time = holdEnd }];
                    envelope = [.. envelope, new GainEnvelope { Gain = sustainGain, Time = decayEnd }];
                    envelope = [.. envelope, new GainEnvelope { Gain = sustainGain, Time = noteEnd }];
                    if (noteEnd != releaseEnd) envelope = [.. envelope, new GainEnvelope { Gain = 0, Time = releaseEnd }];
                }
            }

            // the sample will extend from 0 to releaseEnd
            int totalSamples = (int)Math.Ceiling(outputSampleRate * releaseEnd);
            double[] finalSamples = new double[totalSamples]; // Single channel output.
            for (int i = 0; i < finalSamples.Length; i++) finalSamples[i] = 0; // initialize to silence
            DebugLog.Write($"Total samples={totalSamples}, base resampling ratio={baseRatio}, startcents={startCents}, endcents={endCents}");

            DebugLog.Write($"=== Gain Envelope ({envelope.Length} points) ===");
            for (int idx = 0; idx < envelope.Length; idx++)
            {
                DebugLog.Write($"  [{idx}] Time: {envelope[idx].Time:F3}s, Gain: {envelope[idx].Gain:F4}");
            }

            // we should now have a complete gain envelope. Build the Instrument Source
            InstrumentSource source = new InstrumentSource();
            // NOTE: generator data must be set by the caller as this routine has no knowledge of it
            source.Generator = null; // deferred to caller
            source.StartTime = 0; // deferred to caller
            source.StopTime = 0; // deferred to caller
            source.SoundFontName = ""; // deferred to caller
            source.PresetName = ""; // deferred to caller
            source.Name = ""; // deferred to caller
            source.StartPitch = startPitch;
            source.EndPitch = endPitch;
            source.LoopEnabled = loop;
            source.LoopStart = loopStart;
            source.LoopEnd = loopEnd;
            source.RootKey = rootKey;
            source.StartCents = startCents;
            source.EndCents = endCents;
            source.SampleRate = inputSampleRate;
            source.SampleCount = totalSamples;
            source.AttackEnabled = attackEnabled;
            source.Envelope = envelope;

            // Now we are ready to starting processing the instrument sample. The main loop here is through the output samples. At each step, it is necessary to determine which instrument sample is being addressed. That is determined based on factors including the base sample ratio, tone tuning, application of glissando and vibrato, and looping.

            // first a few things to initialize.
            int iEnvelope = 0; // the current index in the gain envelope.
            int maxEnvelope = envelope.Length - 1; // the maximum index for the gain envelope.
            double instrumentSampleIndex = 0; // the current position in the instrument sample data that is being read from. This is a double because is is calculated from using the base sample ratio, start and end cents, and tremolo. The integer part of this number will be used to determine which sample to read from the instrument sample data, and the fractional part will be used for interpolation between samples if necessary.
            for (int i = 0; i < totalSamples; i++)
            {
                // the time of the sample point
                double t = (double)i / outputSampleRate;

                // calculate the current cents and vibrato cents and combine them with the base ratio.
                double currentCents = Interpolation.Linear(t, 0, interval, startCents, endCents);
                double currentVibrato = vibrato.GetCurrentValue(t);
                double pitchRatio = Math.Pow(2, (currentCents + currentVibrato) / 1200.0); // the time cents to seconds function in double precision
                double effectiveRatio = (pitchRatio / baseRatio);
                //double effectiveRatio = (baseRatio * pitchRatio);

                // now we can calculate the current position in the instrument sample data based on the effective ratio and the previous position. The effective ratio will determine how quickly we move through the instrument sample data, which will affect the pitch of the output. If the effective ratio is greater than 1, we will move through the instrument sample data faster, which will result in a higher pitch. If it is less than 1, we will move through the instrument sample data slower, which will result in a lower pitch.
                instrumentSampleIndex += effectiveRatio;
                double inputPosition = instrumentSampleIndex;
                if (loop && inputPosition >= loopEnd) // if we are looping and the current index is greater than the loop end, we need to wrap around to the loop start
                {
                    double excess = inputPosition - (double)loopEnd;
                    inputPosition = (double)loopStart + (excess % loopLength);
                    instrumentSampleIndex = inputPosition;
                }
                else if (!loop && inputPosition >= instrumentSample.Length) return (finalSamples, source); // if we are not looping and the current index is greater than the sample end, we need to stop processing samples because we have reached the end of the sample data. The rest of the output samples will remain 0, which is silence.

                int index = (int)Math.Floor(inputPosition);
                double frac = inputPosition - (double)index;
                // apply linear interpolation if necessary handling looping
                double sample1 = (loop && index >= loopEnd) ? instrumentSample[loopStart] : instrumentSample[index];
                double sample2 = (loop && index + 1 >= loopEnd) ? instrumentSample[loopStart] : (index + 1 < instrumentSample.Length - 1 ? instrumentSample[index + 1] : sample1); // if the next index is out of bounds, use sample1 as the sample value for interpolation. This can happen if we are at the end of the sample data and not looping, or if we are at the end of the loop and looping.
                double sample = sample1 * (1 - frac) + sample2 * frac; // apply linear interpolation between the two samples

                // now we have a very nice sample. We apply frequency noise, the gain envelope and tremolo
                if (noiseEnabled && noiseFrequency > 0 && noiseAmplitude > 0)
                {
                    double noise = Probability.GaussianRandom(0, noiseFrequency, random);
                    sample += noiseAmplitude * MathUtilities.Sin(2 * Math.PI * (noiseFrequency + noise) * t) / (1 + noiseAmplitude);
                }
                if (t >= envelope[iEnvelope].Time && iEnvelope < maxEnvelope)
                {
                    iEnvelope++;
                }
                double envelopeGain = (iEnvelope < maxEnvelope) ? Interpolation.Linear(t, envelope[iEnvelope - 1].Time, envelope[iEnvelope].Time, envelope[iEnvelope - 1].Gain, envelope[iEnvelope].Gain) : volumeGain * initialAttenuationGain;
                double tremoloGain = Sf2Units.VolumeDbToGain(tremolo.GetCurrentValue(t));
                finalSamples[i] = sample * envelopeGain * tremoloGain;
            }

            // oh my gosh, I think I got it all. 
            return (finalSamples, source);
        }
    }
}
