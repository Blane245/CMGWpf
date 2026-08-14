using CMGWpf.Model;
using CMGWpf.Model.Database;
using CMGWpf.Model.Generators;
using CMGWpf.PlayFunctions.Utilities;
using CMGWpf.Types;
using CMGWpf.Utilities;
using CMGWpf.View;
using static CMGWpf.Types.PlayTypes;

namespace CMGWpf.PlayFunctions.DSP
{
    public class SourcesFromChordSequencer
    {
        /// <summary>
        /// This function generates the audio sources for an algorithmic generator by looping through the time range of the generator and getting the current values from the note, attack, speed, duration, pan, and volume algorithms at each point in time. Looping is done differently when the note algorithm is sequencer It then generates the instrument samples for each note using the preset and sound font information, applies pan and reverb, and merges the samples into the final audio buffer. It also creates InstrumentSource objects for each note and adds them to the collection for visualization. The function is designed to run on a separate thread for each generator to allow for parallel processing of multiple generators.
        /// </summary>
        /// <param name="chordSequencer">The generator to be processed</param>
        public static void Get(ChordSequencer chordSequencer)
        {
            // gather the DSP, etc., information from this generator on a separate thread so that multiple generators can be processed in parallel. 
            var name = chordSequencer.Name;
            var startTime = chordSequencer.StartTime;
            var stopTime = chordSequencer.StopTime;
            var soundFontName = chordSequencer.SoundFontFileName;
            var soundFont = chordSequencer.SoundFont;
            var preset = chordSequencer.Preset;
            var speedAlgorithm = chordSequencer.SpeedAlgorithm;
            var panAlgorithm = chordSequencer.PanAlgorithm;
            var volumeAlgorithm = chordSequencer.VolumeAlgorithm;
            var parent = chordSequencer.Parent;
            var root = chordSequencer.Key ?? "C";
            var octave = chordSequencer.Octave;
            var repeat = chordSequencer.Repeat;
            double time = startTime;
            if (speedAlgorithm == null) return;
            if (volumeAlgorithm == null) return;
            if (panAlgorithm == null) return;
            if (preset == null) return;

            chordSequencer.InitialSequence();

            // note sequencing is driven by the note item sequence and the speed of each beat. The note item sequence is a list of note items, each with a time and a note value. The speed algorithm determines the speed of the generator at each point in time, which affects the interval between notes. The attack, duration, pan, and volume algorithms determine the corresponding values for each note at each point in time.
            DebugLog.Write($"Chord Sequence voice generation");
            int currentRepeat = 0;
            while ((time < stopTime && repeat == 0) || currentRepeat < repeat)
            {
                if (repeat != 0) currentRepeat++;
                foreach (ChordItem chord in chordSequencer.ChordSequence!.Items)
                {
                    if (time >= stopTime && repeat == 0) break;
                    if (chord.ChordValue == "rest") continue;
                    ChordSequencerValues currentValues = chordSequencer.GetChordSequencerValues(time - startTime);
                    var speed = currentValues.Speed;
                    var pan = currentValues.Pan;
                    // get the midi notes for the chord based on the root 
                    double midiBase = Music.Notes[root] + 12 * (octave + 1) - 3; // A is first entry in Notes
                    int[] chordNotes = Music.Chords[chord.ChordValue];
                    double[] chordPitches = new double[chordNotes.Length];
                    double[] startTimes = new double[chordNotes.Length];
                    double interval = (60 / speed) * chord.Duration;
                    int inversion = chord.Inversion;
                    double duration = chord.Effort.Articulation switch
                    {
                        Music.ArticulationType.legato  => interval,
                        Music.ArticulationType.accent => interval,
                        Music.ArticulationType.staccato => interval * 0.2,
                        _ => interval
                    };
                    double volumeDb = Music.WeightToVolumeDb[chord.Effort.Weight] + Math.Clamp(parent.Volume + currentValues.Volume, -100, 100);
                    int velocity = chord.Effort.Articulation == Music.ArticulationType.accent ? 127 : 63;

                    for (int i = 0; i < chordNotes.Length; i++)
                    {
                        chordPitches[i] = midiBase + chordNotes[i];
                        startTimes[i] = chord.Effort.ChordBinding switch
                        {
                            Music.ChordBindingType.Block => 0,
                            Music.ChordBindingType.ArpeggioUp => i * interval / chordNotes.Length,
                            Music.ChordBindingType.ArpeggioDown => (chordNotes.Length - i - 1) * interval / chordNotes.Length,
                            Music.ChordBindingType.StrumUp => i * 0.05,
                            Music.ChordBindingType.StrumDown => (chordNotes.Length - i - 1) * 0.05,
                            // broken TBD
                            _ => 0
                        };

                        // SpaceType TBD
                    }

                    // handle inversions of the chord
                    switch (inversion)
                    {
                        case 0:
                            break;
                        case 1:
                            double holdNote = chordPitches[0];
                            for (int i = 1; i < chordPitches.Length; i++)
                            {
                                chordPitches[i - 1] = chordPitches[i];
                            }
                            chordPitches[^1] = holdNote + 12;
                            break;
                        case 2:
                            double holdNote1 = chordPitches[0];
                            double holdNote2 = chordPitches[1];
                            for (int i = 2; i < chordPitches.Length; i++)
                            {
                                chordPitches[i - 2] = chordPitches[i];
                            }
                            chordPitches[^2] = holdNote1 + 12;
                            chordPitches[^1] = holdNote2 + 12;
                            break;
                        default:
                            break;
                    }


                    // a chord is a set of notes that will be played based on its effort.
                    // Effort includes weight (volume), articulation, binding, and space
                    double[] chordSample = new double[(int)(interval * SampleRate)];
                    // loop through each note in the chord and build its sample 
                    for (int i = 0; i < chordNotes.Length; i++)
                    {
                        // get the instrument voices for this note in the chord and construct their samples
                        List<FinalVoice> voices = PresetUtilities.BuildVoicesForPresetAtKeyVel(preset, (int)chordPitches[i], velocity);
                        foreach (var voice in voices)
                        {
                            // get the chord notes in terms of midi numbers. 
                            (var instrumentSample, var source) = DSP.InstrumentSample.Get(new DSP.InstrumentSampleParameters()
                            {
                                Duration = duration,
                                Interval = interval,
                                StartPitch = chordPitches[i],
                                EndPitch = chordPitches[i],
                                VolumeDb = volumeDb,
                                AttackEnabled = true,
                                LoopEnabled = true,
                                NoiseEnabled = chordSequencer.NoiseEnabled,
                                NoiseAmplitude = chordSequencer.NoiseAmplitude,
                                NoiseFrequency = chordSequencer.NoiseFrequency,
                                Tremolo = chordSequencer.Tremolo,
                                Vibrato = chordSequencer.Vibrato,
                                Voice = voice,
                                SampleRate = SampleRate,
                                SoundFont = soundFont
                            });
                            // complete the source definition and add to the sources collection
                            source.Generator = chordSequencer;
                            source.StartTime = time + startTimes[i];
                            source.StopTime = source.StartTime + (double)instrumentSample.Length / SampleRate;
                            source.SoundFontName = soundFontName;
                            source.PresetName = preset.Name;
                            source.Name = voice.InstrumentName;

                            // apply pan, reverb, and merge into audio buffer here
                            double left = (1 - pan) / 2;
                            double right = (1 + pan) / 2;
                            double[] panInstrumentSample = new double[instrumentSample.Length * 2];
                            for (int j = 0; j < instrumentSample.Length; j++)
                            {
                                panInstrumentSample[j * 2] += instrumentSample[j] * left;
                                panInstrumentSample[j * 2 + 1] += instrumentSample[j] * right;
                            }

                            Reverb.Apply(panInstrumentSample, chordSequencer.ReverbDelay, chordSequencer.ReverbDecay, PlayTypes.SampleRate);

                            // Update global data - only lock for buffer modification
                            int instrumentStartIndex = (int)((time + startTimes[i]) * PlayTypes.SampleRate) * 2;
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
                                    Start = new TimeMidiPoint { Time = time + startTimes[i], Midi = chordPitches[i] },
                                    End = new TimeMidiPoint { Time = instrumentEndTime, Midi = chordPitches[i] }
                                },
                                GeneratorName = chordSequencer.Name,
                                VoiceName = ""
                            });
                            PlayViewModel.Instance.InstrumentSources.Add(source);
                            // add the generator name to the list of generator voices for scroll roll display
                            // ConcurrentBag is thread-safe, no lock needed
                            PlayViewModel.Instance.GeneratorVoices.Add(new GeneratorVoice()
                            {
                                GeneratorName = chordSequencer.Name,
                                VoiceName = ""
                            });
                        }
                    }
                    time += interval;
                }
            }
        }
    }
}
