using CMGWpf.Model;
using CMGWpf.Model.Generators;
using CMGWpf.PlayFunctions.Utilities;
using CMGWpf.Services;
using CMGWpf.Types;
using CMGWpf.View;
using static CMGWpf.Types.DBTypes;
using static CMGWpf.Types.PlayTypes;

namespace CMGWpf.PlayFunctions.DSP
{
    public class SourcesFromAlgorithmic
    {
        public static void Get(Algorithmic algorithmic)
        {
            // gather the DSP, etc., information from this generator on a separate thread so that multiple generators can be processed in parallel. 
            var name = algorithmic.Name;
            var startTime = algorithmic.StartTime;
            var stopTime = algorithmic.StopTime;
            var soundFontName = algorithmic.SoundFontFileName;
            var soundFont = algorithmic.SoundFont;
            var preset = algorithmic.Preset;
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
                    double interval = Math.Min(60 / speed, stopTime - time);
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
                            source.StopTime = time + noteDuration;
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
                            double instrumentEndTime = time + (double)instrumentSample.Length / PlayTypes.SampleRate;

                            // Lock only for the audio buffer (needs synchronization for resize)
                            bool lockTaken = false;
                            try
                            {
                                Monitor.Enter(GlobalService.Instance.PlayResultsLock, ref lockTaken);
                                PlayViewModel.Instance.FinalSignal.Add(panSamples, instrumentStartIndex);
                            }
                            finally
                            {
                                if (lockTaken)
                                {
                                    Monitor.Exit(GlobalService.Instance.PlayResultsLock);
                                }
                            }

                            // These use ConcurrentBag - no lock needed!
                            PlayViewModel.Instance.TimeMidiPresets.Add(new TimeMidiPreset   
                            {
                                Line = new TimeMidiLine { Start = new TimeMidiPoint { Time = time, Midi = note }, End = new TimeMidiPoint { Time = instrumentEndTime, Midi = note } },
                                SoundFontName = soundFontName,
                                PresetName = preset.Name
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
                    double beat = item.beats;
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
                    if (item.value >= 0 && hitBeat) // not a rest or a skipped note
                    {
                        List<FinalVoice> voices = PresetUtilities.BuildVoicesForPresetAtKeyVel(preset, (int)note, (int)velocity);
                        foreach (var voice in voices)
                        {
                            DebugLog.Write($"Building sample {voice!.SampleHeader!.Name}, start {voice.SampleHeader.Start}, end {voice.SampleHeader.End}, length {voice.SampleHeader.End - voice.SampleHeader.Start}, loop start {voice.SampleHeader.StartLoop}, loop end {voice.SampleHeader.EndLoop}, sample rate {voice.SampleHeader.SampleRate}, original pitch {voice.SampleHeader.OriginalPitch}, pitch correction {voice.SampleHeader.PitchCorrection} for Instrument {voice.InstrumentName} with generators:");
                            DebugLog.Write($"");
                            //foreach (var SFgen in voice.Generators)
                            //{
                            //    DebugLog($"  {SFgen.Key}: {SFgen.Value}");
                            //}                        // not a rest
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
                            source.StopTime = time + duration;
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
                            double instrumentEndTime = time + (double)instrumentSample.Length / PlayTypes.SampleRate;

                            // Lock only for audio buffer
                            bool lockTaken = false;
                            try
                            {
                                Monitor.Enter(GlobalService.Instance.PlayResultsLock, ref lockTaken);
                                PlayViewModel.Instance.FinalSignal.Add(panInstrumentSample, instrumentStartIndex);
                            }
                            finally
                            {
                                if (lockTaken)
                                {
                                    Monitor.Exit(GlobalService.Instance.PlayResultsLock);
                                }
                            }

                            // Use concurrent collections - no lock needed
                            PlayViewModel.Instance.TimeMidiPresets.Add(new TimeMidiPreset
                            {
                                Line = new TimeMidiLine
                                {
                                    Start = new TimeMidiPoint { Time = time, Midi = note },
                                    End = new TimeMidiPoint { Time = instrumentEndTime, Midi = note }
                                },
                                SoundFontName = soundFontName,
                                PresetName = preset.Name
                            });
                            PlayViewModel.Instance.InstrumentSources.Add(source);
                        }
                    }
                    time += interval;
                    beats += 1;
                }
            }

            // add the preset to the collection of presets that have been played so that it can be displayed in the UI with its assigned color
            // ConcurrentBag is thread-safe, no lock needed
            SF_Preset currentPreset = new()
            {
                SoundFontName = soundFontName,
                PresetName = preset.Name
            };
            PlayViewModel.Instance.SF_Presets.Add(currentPreset);
        }
    }
}
