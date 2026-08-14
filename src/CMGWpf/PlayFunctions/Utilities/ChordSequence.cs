//using CMGWpf.Model;
//using CMGWpf.SoundFont_2;
//using CMGWpf.Types;
//using static CMGWpf.Types.PlayTypes;

//namespace CMGWpf.PlayFunctions.Utilities
//{
//    public static class ChordSequence
//    {
//        public static void Play(CMGWpf.Model.Database.ChordSequence chordSequence, SoundFont soundFont, Preset preset)
//        {
//            if (soundFont == null || preset == null) return;

//            for (int i = 0; i < chordSequence.Items.Count; i++)
//            {
//                var chord = chordSequence.Items[i];
//                Chord.Play(chord, soundFont, preset, chordSequence.BPM, chordSequence.RootNote, chordSequence.RootOctave);
//            }
//        }
//    }
//}
