using CMGWpf.Model;
using CMGWpf.Model.Generators;
using CMGWpf.PlayFunctions.Utilities;
using CMGWpf.Types;
using System.Collections.ObjectModel;
using static CMGWpf.Types.DBTypes;
using static CMGWpf.Types.PlayTypes;

namespace CMGWpf.PlayFunctions.DSP
{
    public class SourcesFromAlgorithmic
    {
        public static string Get(Algorithmic? algorithmic, ref double[] stereoBuffer, List<SF_Preset> sF_Presets, ObservableCollection<InstrumentSource> sources)
        {
            if (algorithmic == null)
            {
                return "Algorithmic generator is null.";
            }
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
            if (noteAlgorithm == null) return $"Note attribute has not been specified for generator {name}";
            if (attackAlgorithm == null) return $"Attack attribute has not been specified for generator {name}";
            if (speedAlgorithm == null) return $"Speed attribute has not been specified for generator {name}";
            if (durationAlgorithm == null) return $"Duration attribute has not been specified for generator {name}";
            if (volumeAlgorithm == null) return $"Volume attribute has not been specified for generator {name}";
            if (panAlgorithm == null) return $"Pan attribute has not been specified for generator {name}";
            if (preset == null) return $"Preset has not been specified for generator {name}";

            RandomAlgorithm.Initialize(noteAlgorithm);
            RandomAlgorithm.Initialize(attackAlgorithm);
            RandomAlgorithm.Initialize(speedAlgorithm);
            RandomAlgorithm.Initialize(durationAlgorithm);
            RandomAlgorithm.Initialize(volumeAlgorithm);
            RandomAlgorithm.Initialize(panAlgorithm);
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
                            sources.Add(source);
                            // apply pan and merge into audio buffer here
                            double left = (1 - currentValues.Pan) / 2;
                            double right = (1 + currentValues.Pan) / 2;
                            int instrumentStartIndex = (int)(time * PlayTypes.SampleRate) * 2; // in stereobuffer pointer space
                            // first create the stereo pan version of the instrument samples
                            double[] panSamples = new double[instrumentSample.Length * 2];
                            for (int i = 0; i < instrumentSample.Length; i++)
                            {
                                panSamples[i * 2] = instrumentSample[i] * left;
                                panSamples[i * 2 + 1] = instrumentSample[i] * right;
                            }
                            Reverb.Apply(panSamples, algorithmic.ReverbDelay, algorithmic.ReverbDecay, PlayTypes.SampleRate);
                            // now add the pan samples into the stereo buffer
                            AudioBuffer.Add(panSamples, ref stereoBuffer, instrumentStartIndex);
                            SoundRollBuilder.AddInstrument(new TimeMidiLine
                            {
                                Start = new TimeMidiPoint { Time = time, Midi = (int)note },
                                End = new TimeMidiPoint { Time = time + noteDuration, Midi = (int)note }
                            }, soundFontName, preset.Name);
                        }
                    }
                    time += interval;
                }

            } else
            {
                // note sequencing is driven by the note item sequence and the speed of each beat. The note item sequence is a list of note items, each with a time and a note value. The speed algorithm determines the speed of the generator at each point in time, which affects the interval between notes. The attack, duration, pan, and volume algorithms determine the corresponding values for each note at each point in time.
                DebugLog.Write($"Sequence algorithm voice generation");
                Sequencer sequencer = (noteAlgorithm as Sequencer)!;
                double beats = 1;
                double transpose = sequencer.Transpose;
                sequencer.SetReflect();
                sequencer.SetReverse();
                foreach (SequenceItem item in sequencer.Items)
                {
                    double note = item.value + transpose;
                    double beat = item.beats;
                    CurrentValues currentValues = algorithmic.GetCurrentValues(time - startTime, beats);
                    var hitBeat = currentValues.Beat;
                    var velocity = currentValues.Attack;
                    var speed = currentValues.Speed;
                    var durationPercent = currentValues.Duration;
                    var volumedB = currentValues.Volume;
                    var pan = currentValues.Pan;
                    volumedB += parent.Volume;
                    double interval = Math.Min(60 / speed, stopTime - time);
                    double duration = (interval * currentValues.Duration) / 100;
                    if (item.value >= 0 && hitBeat) // not a rest or a skipped note
                    {
                        List<FinalVoice> voices = PresetUtilities.BuildVoicesForPresetAtKeyVel(preset, (int)note, (int)velocity);
                        foreach (var voice in voices)
                        {
                            DebugLog.Write($"Building sample {voice!.SampleHeader!.Name}, start {voice.SampleHeader.Start}, end {voice.SampleHeader.End}, length {voice.SampleHeader.End - voice.SampleHeader.Start}, loop start {voice.SampleHeader.StartLoop}, loop end {voice.SampleHeader.EndLoop}, sample rate {voice.SampleHeader.SampleRate}, original pitch {voice.SampleHeader.OriginalPitch}, pitch correction {voice.SampleHeader.PitchCorrection} for Intrument {voice.InstrumentName} with generators:");
                            DebugLog.Write($"");
                            //foreach (var SFgen in voice.Generators)
                            //{
                            //    DebugLog($"  {SFgen.Key}: {SFgen.Value}");
                            //}                        // not a rest
                            (double[] instrumentSample, InstrumentSource source)  = InstrumentSample.Get(new InstrumentSampleParameters
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
                            sources.Add(source);

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

                            int instrumentStartIndex = (int)(time * PlayTypes.SampleRate) * 2; // in stereobuffer pointer space
                            DebugLog.Write($"Generated sample for note {note} with {instrumentSample.Length} samples at {PlayTypes.SampleRate}Hz. Pan is (left, right)=({left},{right}). Merging at t={time}(sec), index={instrumentStartIndex}");
                            AudioBuffer.Add(panInstrumentSample, ref stereoBuffer, instrumentStartIndex);
                            double instrumentEndTime = time + (double)instrumentSample.Length / PlayTypes.SampleRate;
                            SoundRollBuilder.AddInstrument(new TimeMidiLine
                            {
                                Start = new TimeMidiPoint { Time = time, Midi = (int)note },
                                End = new TimeMidiPoint { Time = instrumentEndTime, Midi = (int)note }
                            }, soundFontName, preset.Name);
                        }
                    }
                    time += interval;
                    beats += beat;
                }
            }

            // add the preset to the collection of presets that have been played so that it can be displayed in the UI with its assigned color
            SF_Preset currentPreset = new()
            {
                SoundFontName = soundFontName,
                PresetName = preset.Name
            };
            sF_Presets.Add(currentPreset);

            return "";
        }

    }
}
