using CMGWpf.Model;
using CMGWpf.Model.Generators;
using CMGWpf.PlayFunctions.Utilities;
using CMGWpf.Types;
using CMGWpf.Utilities;
using CMGWpf.View;
using static CMGWpf.Types.DBTypes;
using static CMGWpf.Types.PlayTypes;

namespace CMGWpf.PlayFunctions.DSP
{
    public class SourcesFromAlgorithmic
    {
        /// <summary>
        /// This function generates the audio sources for an algorithmic generator by looping through the time range of the generator and getting the current values from the note, attack, speed, duration, pan, and volume algorithms at each point in time. Looping is done differently when the note algorithm is sequencer It then generates the instrument samples for each note using the preset and sound font information, applies pan and reverb, and merges the samples into the final audio buffer. It also creates InstrumentSource objects for each note and adds them to the collection for visualization. The function is designed to run on a separate thread for each generator to allow for parallel processing of multiple generators.
        /// </summary>
        /// <param name="algorithmic">The generator to be processed</param>
        public static void Get(Algorithmic algorithmic)
        {
            // gather the DSP, etc., information from this generator on a separate thread so that multiple generators can be processed in parallel. 
            var name = algorithmic.Name;
            var startTime = algorithmic.StartTime;
            var stopTime = algorithmic.StopTime;
            var soundFontName = algorithmic.SoundFontFileName;
            var soundFont = algorithmic.SoundFont;
            var preset = algorithmic.Preset;
            var microtones = algorithmic.Microtones;
            var noteAlgorithm = algorithmic.NoteAlgorithm;
            var attackAlgorithm = algorithmic.AttackAlgorithm;
            var speedAlgorithm = algorithmic.SpeedAlgorithm;
            var durationAlgorithm = algorithmic.DurationAlgorithm;
            var panAlgorithm = algorithmic.PanAlgorithm;
            var volumeAlgorithm = algorithmic.VolumeAlgorithm;
            var parent = algorithmic.Parent;

            double time = startTime;
            if (noteAlgorithm == null) return;
            if (attackAlgorithm == null) return;
            if (speedAlgorithm == null) return;
            if (durationAlgorithm == null) return;
            if (volumeAlgorithm == null) return;
            if (panAlgorithm == null) return;
            if (preset == null) return;

            algorithmic.InitialSequence();

            if (noteAlgorithm.GetType().Name != "Sequencer")
            {
                // loop through the time range and get the values from the algorithmic generator at each step, then generate the samples for each note and merge them into the audio buffer. The time step is determined by the speed value of the generator at each point in time, so that it can change over time according to the speed algorithm.
                while (time < stopTime - 0.001)
                {
                    CurrentValues currentValues = algorithmic.GetCurrentValues(time - startTime, 0);
                    DebugLog.Write($"At time {time}: Note={currentValues.Note}, Attack={currentValues.Attack}, Speed={currentValues.Speed}, Duration={currentValues.Duration}, Pan={currentValues.Pan}, Volume={currentValues.Volume}");
                    var hitBeat = currentValues.Beat;
                    var note = currentValues.Note;
                    if (!algorithmic.Microtones) note = Math.Round(note);
                    var velocity = currentValues.Attack;
                    var speed = currentValues.Speed;
                    var durationPercent = currentValues.Duration;
                    var volumedB = currentValues.Volume;
                    var pan = currentValues.Pan;
                    volumedB += Math.Clamp(parent.Volume, -10, 10);
                    double interval = 60 / speed;
                    double noteDuration = (interval * durationPercent) / 100;
                    if (hitBeat)
                    {
                        DebugLog.Write($"Hit beat at time {time}. Playing note {note} with velocity {velocity}, speed {speed}, interval {interval}, duration {noteDuration}, volume {volumedB}, pan {pan}");
                        List<FinalVoice> voices = PresetUtilities.BuildVoicesForPresetAtKeyVel(preset, (int)note, (int)velocity);
                        foreach (var voice in voices)
                        {
                            (double[] instrumentSample, InstrumentSource source) = InstrumentSample.Get(new InstrumentSampleParameters
                            {
                                Duration = noteDuration,
                                Interval = interval,
                                StartPitch = note,
                                EndPitch = note,
                                VolumeDb = volumedB,
                                AttackEnabled = algorithmic.AttackEnabled,
                                LoopEnabled = algorithmic.IsLooping,
                                NoiseEnabled = algorithmic.NoiseEnabled,
                                NoiseFrequency = algorithmic.NoiseFrequency,
                                NoiseAmplitude = algorithmic.NoiseAmplitude,
                                Tremolo = algorithmic.Tremolo,
                                Vibrato = algorithmic.Vibrato,
                                Voice = voice,
                                SampleRate = PlayTypes.SampleRate,
                                SoundFont = soundFont!
                            });

                            // complete the source definition and add to the sources collection
                            source.Generator = algorithmic;
                            source.StartTime = time;
                            source.StopTime = time + (double)instrumentSample.Length / SampleRate;
                            source.SoundFontName = soundFontName;
                            source.PresetName = preset.Name;
                            source.Name = voice.InstrumentName;
                            // apply pan and merge into audio buffer here
                            double left = (1 - currentValues.Pan) / 2;
                            double right = (1 + currentValues.Pan) / 2;
                            int instrumentStartIndex = (int)(time * PlayTypes.SampleRate) * 2; // in stereo buffer pointer space
                                                                                               // first create the stereo pan version of the instrument samples
                            double[] panSamples = new double[instrumentSample.Length * 2];
                            for (int i = 0; i < instrumentSample.Length; i++)
                            {
                                panSamples[i * 2] = instrumentSample[i] * left;
                                panSamples[i * 2 + 1] = instrumentSample[i] * right;
                            }
                            Reverb.Apply(panSamples, algorithmic.ReverbDelay, algorithmic.ReverbDecay, PlayTypes.SampleRate);

                            // Update global data - only lock for buffer modification, use concurrent collections for the rest
                            double instrumentEndTime = source.StopTime;

                            // Lock only for the audio buffer (needs synchronization for resize)
                            bool lockTaken = false;
                            try
                            {
                                Monitor.Enter(PlayViewModel.Instance.PlayResultsLock, ref lockTaken);
                                PlayViewModel.Instance.FinalSignal.Add(panSamples, instrumentStartIndex);
                            }
                            finally
                            {
                                if (lockTaken)
                                {
                                    Monitor.Exit(PlayViewModel.Instance.PlayResultsLock);
                                }
                            }

                            // These use ConcurrentBag - no lock needed!
                            PlayViewModel.Instance.TimeMidiVoices.Add(new TimeMidiVoice
                            {
                                Line = new TimeMidiLine { Start = new TimeMidiPoint { Time = time, Midi = note }, End = new TimeMidiPoint { Time = instrumentEndTime, Midi = note } },
                                GeneratorName = algorithmic.Name,
                                VoiceName = ""
                            });
                            PlayViewModel.Instance.InstrumentSources.Add(source);
                        }
                    }
                    time += interval;
                }

            }
            else
            {
                // note sequencing is driven by the note item sequence and the speed of each beat. The note item sequence is a list of note items, each with a time and a note value. The speed algorithm determines the speed of the generator at each point in time, which affects the interval between notes. The attack, duration, pan, and volume algorithms determine the corresponding values for each note at each point in time.
                DebugLog.Write($"Sequence algorithm voice generation");
                Sequencer sequencer = (noteAlgorithm as Sequencer)!;
                int beats = 0;
                foreach (SequenceItem item in sequencer.Items)
                {
                    double beat = item.Beats;
                    CurrentValues currentValues = algorithmic.GetCurrentValues(time - startTime, beats);
                    double note = currentValues.Note;
                    var hitBeat = currentValues.Beat;
                    var velocity = currentValues.Attack;
                    var speed = currentValues.Speed;
                    var durationPercent = currentValues.Duration;
                    double interval = Math.Min(beat * 60 / speed, stopTime - time);
                    double duration = (interval * durationPercent) / 100;
                    var volumedB = Math.Clamp(parent.Volume + currentValues.Volume, -10, 10);
                    var pan = currentValues.Pan;
                    if (item.Value >= 0 && hitBeat) // not a rest or a skipped note
                    {
                        DebugLog.Write($"Hit beat at time {time}. Playing note {note} with velocity {velocity}, speed {speed}, interval {interval}, duration {duration}, volume {volumedB}, pan {pan}");
                        List<FinalVoice> voices = PresetUtilities.BuildVoicesForPresetAtKeyVel(preset, (int)note, (int)velocity);
                        foreach (var voice in voices)
                        {
                            (double[] instrumentSample, InstrumentSource source) = InstrumentSample.Get(new InstrumentSampleParameters
                            {
                                Duration = duration,
                                Interval = interval,
                                StartPitch = note,
                                EndPitch = note,
                                VolumeDb = currentValues.Volume,
                                AttackEnabled = algorithmic.AttackEnabled,
                                LoopEnabled = algorithmic.IsLooping,
                                NoiseEnabled = algorithmic.NoiseEnabled,
                                NoiseFrequency = algorithmic.NoiseFrequency,
                                NoiseAmplitude = algorithmic.NoiseAmplitude,
                                Tremolo = algorithmic.Tremolo,
                                Vibrato = algorithmic.Vibrato,
                                Voice = voice,
                                SampleRate = PlayTypes.SampleRate,
                                SoundFont = soundFont!

                            });

                            // complete the source definition and add to the sources collection
                            source.Generator = algorithmic;
                            source.StartTime = time;
                            source.StopTime = time + (double)instrumentSample.Length / SampleRate;
                            source.SoundFontName = soundFontName;
                            source.PresetName = preset.Name;
                            source.Name = voice.InstrumentName;

                            // apply pan, reverb, and merge into audio buffer here
                            double left = (1 - currentValues.Pan) / 2;
                            double right = (1 + currentValues.Pan) / 2;
                            double[] panInstrumentSample = new double[instrumentSample.Length * 2];
                            for (int i = 0; i < instrumentSample.Length; i++)
                            {
                                panInstrumentSample[i * 2] += instrumentSample[i] * left;
                                panInstrumentSample[i * 2 + 1] += instrumentSample[i] * right;
                            }

                            Reverb.Apply(panInstrumentSample, algorithmic.ReverbDelay, algorithmic.ReverbDecay, PlayTypes.SampleRate);

                            // Update global data - only lock for buffer modification
                            int instrumentStartIndex = (int)(time * PlayTypes.SampleRate) * 2;
                            double instrumentEndTime = source.StopTime;

                            // Lock only for audio buffer
                            bool lockTaken = false;
                            try
                            {
                                Monitor.Enter(PlayViewModel.Instance.PlayResultsLock, ref lockTaken);
                                PlayViewModel.Instance.FinalSignal.Add(panInstrumentSample, instrumentStartIndex);
                            }
                            finally
                            {
                                if (lockTaken)
                                {
                                    Monitor.Exit(PlayViewModel.Instance.PlayResultsLock);
                                }
                            }

                            // Use concurrent collections - no lock needed
                            PlayViewModel.Instance.TimeMidiVoices.Add(new TimeMidiVoice
                            {
                                Line = new TimeMidiLine
                                {
                                    Start = new TimeMidiPoint { Time = time, Midi = note },
                                    End = new TimeMidiPoint { Time = instrumentEndTime, Midi = note }
                                },
                                GeneratorName = algorithmic.Name,
                                VoiceName = ""
                            });
                            PlayViewModel.Instance.InstrumentSources.Add(source);
                        }
                    }
                    time += interval;
                    beats += 1;
                }
            }

            // add the generator name to the list of generator voices for scroll roll display
            // ConcurrentBag is thread-safe, no lock needed
            GeneratorVoice currentGV = new()
            {
                GeneratorName = algorithmic.Name,
                VoiceName = ""
            };
            PlayViewModel.Instance.GeneratorVoices.Add(currentGV);
        }
    }
}
