//using CMGWpf.Model;
//using CMGWpf.SoundFont_2;
//using CMGWpf.Types;
//using CMGWpf.Model.Database;
//using static CMGWpf.Types.PlayTypes;
//using System.Windows.Threading;
//using System.Runtime.CompilerServices;

//namespace CMGWpf.PlayFunctions.Utilities
//{
//    public static class Chord
//    {
//        public static void Play(ChordItem chord, SoundFont soundFont, Preset preset,  double BPM, string root, int octave)
//        {
//            if (soundFont == null || preset == null) return;

//            // get the midi notes for the chord based on the root 
//            double midiBase = Music.Notes[root] + 12 * (octave + 1) + 9; // A is first entry in Notes
//            string[] chordNotes = chord.Notes.Split(",");
//            double[] chordPitches = new double[chordNotes.Length];
//            double[] startTimes = new double[chordNotes.Length];
//            double interval = 60 / BPM * chord.Duration;
//            double duration = chord.Effort.Articulation switch
//            {
//                Music.ArticulationType.legato => interval,
//                Music.ArticulationType.staccato => interval * 0.2,
//                _ => interval
//            }; ;
//            double volumeDb = Music.WeightToVolumeDb[chord.Effort.Weight];
//            int velocity = chord.Effort.Articulation ==  Music.ArticulationType.accent ? 127 : 63;

//            for (int i = 0; i < chordNotes.Length; i++)
//            {
//                chordPitches[i] = midiBase + Music.Notes[chordNotes[i]];
//                startTimes[i] = chord.Effort.ChordBinding switch
//                {
//                    Music.ChordBindingType.Block => 0,
//                    Music.ChordBindingType.ArpeggioUp => i * interval / chordNotes.Length,
//                    Music.ChordBindingType.ArpeggioDown => (chordNotes.Length - i - 1) * interval / chordNotes.Length,
//                    Music.ChordBindingType.StrumUp => i * 0.05,
//                    Music.ChordBindingType.StrumDown => (chordNotes.Length - i - 1) * 0.05,
//                    // broken TBD
//                    _ => 0
//                };

//                // SpaceType TBD
//            }


//            // a chord is a set of notes that will be played based on its effort.
//            // Effort includes weight (volume), articulation, binding, and space
//            double[] chordSample = new double[(int)(interval * SampleRate)];
//            // loop through each note in the chord and build its sample 
//            for (int i = 0; i < chordNotes.Length; i++)
//            {
//                // get the instrument voices for this note in the chord and construct their samples
//                List<FinalVoice> voices = PresetUtilities.BuildVoicesForPresetAtKeyVel(preset, (int)chordPitches[i], velocity);
//                foreach (var voice in voices) { 
//                    // get the chord notes in terms of midi numbers. 
//                    (var sample, var source) = DSP.InstrumentSample.Get(new DSP.InstrumentSampleParameters()
//                    {
//                        Duration = duration,
//                        Interval = interval,
//                        StartPitch = chordPitches[i],
//                        EndPitch = chordPitches[i],
//                        VolumeDb = volumeDb,
//                        AttackEnabled = true,
//                        LoopEnabled = true,
//                        NoiseEnabled = false,
//                        NoiseAmplitude = 0,
//                        NoiseFrequency = 0,
//                        Tremolo = new Tremolo() { Depth = 0 },
//                        Vibrato = new Tremolo() { Depth = 0 },
//                        Voice = voice,
//                        SampleRate = SampleRate,
//                        SoundFont = soundFont
//                    });

//                    // add the instrument's sample to the chord sample and the location for this note
//                    chordSample = AudioBuffer.Add(sample, chordSample, (int)(startTimes[i] * SampleRate));
//                }
//            }
//            // make the chordSample stereo
//            double[] stereoSample = new double[chordSample.Length * 2];
//            for (int i = 0; i < chordSample.Length; i++)
//            {
//                stereoSample[i * 2] = chordSample[i];
//                stereoSample[i * 2 + 1] = chordSample[i];
//            }
//            // play the chord
//            float[] normalizedSample = PlayEngine.NormalizeBuffer(stereoSample);
//            var provider = new AudioBufferProvider(
//                normalizedSample,
//                SampleRate);

//            // Use WaveOut - AudioBufferProvider implements IWaveProvider directly
//            var AudioOutput = new NAudio.Wave.WaveOut();
//            AudioOutput.Init(provider);
//            AudioOutput.Volume = 1;
//            AudioOutput.Play();
//            // don't return until the chord has finished playing
//            TaskAwaiter taskAwaiter = Task.Run(() =>
//            {
//                while (provider.CurrentPosition < provider.Duration)
//                {
//                    Thread.Sleep(10);
//                }
//            }).GetAwaiter();
//        }
//    }
//}
