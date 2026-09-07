//using CMGWpf.SoundFont_2;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System.Runtime.CompilerServices;

namespace CMGWpf.Types
{
    public static class Music
    {
        public static Dictionary<string, int> Notes = new Dictionary<string, int>()
        {
        {"A", 0},
        {"A#", 1},
            {"Bb",1 },
        {"B", 2},
            {"Cb",2 },
        {"C", 3},
        {"C#", 4},
            {"Db",4 },
        {"D", 5},
        {"D#", 6},
            {"Eb",6 },
        {"E", 7},
            {"Fb",7 },
        {"F", 8},
        {"F#", 9},
            {"Gb",9 },
        {"G", 10},
        {"G#", 11},
            {"Ab",11 },
        };
        public static Dictionary<int, string> NotesReverse = new Dictionary<int, string>()
        {
            {0, "A"},
            {1, "A#/Bb"},
            {2, "B/Cb"},
            {3, "C"},
            {4, "C#/Db"},
            {5, "D"},
            {6, "D#/Eb"},
            {7, "E/Fb"},
            {8, "F"},
            {9, "F#/Gb"},
            {10, "G"},
            {11, "G#/Ab"}
        };
        public static Dictionary<string, int[]> Chords = new Dictionary<string, int[]>
        {
            {"I", new int[] { 0, 4, 7 } },
            {"i", new int[] { 0, 3, 7 } },
            {"ii", new int[] { 2, 5, 9 } },
            {"ii°", new int[] { 2, 5, 8 } },
            {"III", new int[] { 4, 8, 11 } },
            {"III+", new int[] { 4, 8, 12 } },
            {"iii", new int[] { 4, 7, 11 } },
            {"IV", new int[] { 5, 9, 12 } },
            {"iv", new int[] { 5, 8, 12 } },
            {"V", new int[] { 7, 11, 14 } },
            {"VI", new int[] { 9, 13, 16 } },
            {"vi", new int[] { 9, 12, 16 } },
            {"#vi°", new int[] {10, 14, 17 } },
            {"VII", new int[] { 11, 15, 18 } },
            {"vii°", new int[] { 11, 14, 19 } },
            {"rest", new int[] { } }
        };
        public static Dictionary<int[], string> ChordsReverse = new Dictionary<int[], string>
        {
            { new int[] { 0, 4, 7 }, "I" },
            { new int[] { 0, 3, 7 }, "i" },
            { new int[] { 2, 5, 9 }, "ii" },
            { new int[] { 2, 5, 8 }, "ii°" },
            { new int[] { 4, 8, 11 }, "III" },
            { new int [] { 4, 8, 12 }, "III+" },
            { new int[] { 4, 7, 11 }, "iii" },
            { new int[] {  5, 9, 12 }, "IV" },
            { new int[] { 5, 8, 12 }, "iv" },
            { new int[] { 7, 11, 14 }, "V" },
            { new int[] { 9, 13, 16 }, "VI" },
            { new int[] { 9, 12, 16 }, "vi" },
            { new int[] { 10, 14, 17 }, "#vi°" },
            { new int[] { 11, 15, 18 }, "VII" },
            { new int[] { 11, 14, 19}, "vii°"},
            { new int[]{}, "rest"},
        };
        //public static Dictionary<string, int[]> Chords = new Dictionary<string, int[]>
        //{
        //    {"M", new int[] { 0, 4, 7 } },
        //    {"m", new int[] { 0, 3, 7 } },
        //    {"7", new int[] { 0, 4, 7, 10 } },
        //    {"m7", new int[] { 0, 3, 7, 10 } },
        //    {"M7", new int[] { 0, 4, 7, 11 } },
        //    {"aug", new int [] { 0, 4, 8 } },
        //    {"dim", new int[] { 0, 3, 6 } },
        //    {"sus2", new int[] { 0, 2, 7 } },
        //    {"sus4", new int[] { 0, 5, 7 } },
        //    {"m7(b5)", new int[] { 0, 3, 6, 10 } },
        //    {"7(b9)", new int[] { 0, 4, 7, 10, 13 } },
        //    {"7(#9)", new int[] { 0, 4, 7, 10, 15 } },
        //    {"7(b5)", new int[] { 0, 4, 6, 10 } },
        //    {"7(#5)", new int[] { 0, 4, 8, 10 } },
        //    {"9", new int[] { 0, 4, 7, 10, 14 } },
        //    {"m9", new int[] { 0, 3, 7, 10, 14 } },
        //    {"M9", new int[] { 0, 4, 7, 11, 14 } },
        //    { "add9", new int[] { 0, 4, 7, 14 } },
        //    { "m(add9)", new int[] { 0, 3, 7, 14 } },
        //    { "M(add9)", new int[] { 0, 4, 7, 11, 14 } },
        //    { "6", new int[] { 0, 4, 7, 9 } },
        //    { "m6", new int[] { 0, 3, 7, 9 } },
        //    { "M6", new int[] { 0, 4, 7, 11, 9 } },
        //    { "11", new int[] { 0, 4, 7, 10, 14, 17 } },
        //    { "m11", new int[] { 0, 3, 7, 10, 14, 17 } },
        //    { "M11", new int[] { 0, 4, 7, 11, 14, 17 } }
        //};

        //public static Dictionary<int[], string> ChordsReverse = new Dictionary<int[], string>
        //{
        //    { new int[] { 0, 4, 7 }, "M" },
        //    { new int[] { 0, 3, 7 }, "m" },
        //    { new int[] { 0, 4, 7, 10 }, "7" },
        //    { new int[] { 0, 3, 7, 10 }, "m7" },
        //    { new int[] { 0, 4, 7, 11 }, "M7" },
        //    { new int [] { 0, 4, 8 }, "aug" },
        //    { new int[] { 0, 3, 6 }, "dim" },
        //    { new int[] { 0, 2, 7 }, "sus2" },
        //    { new int[] { 0, 5, 7 }, "sus4" },
        //    { new int[] { 0, 3, 6, 10 }, "m7(b5)" },
        //    { new int[] { 0, 4, 7, 10, 13 }, "7(b9)" },
        //    { new int[] { 0, 4, 7, 10, 15 }, "7(#9)" },
        //    { new int[] { 0, 4, 6, 10 }, "7(b5)" },
        //    { new int[] { 0, 4, 8, 10 }, "7(#5)" },
        //    { new int[] { 0, 4, 7, 10,14 }, "9" },
        //    { new int[] {0 ,3 ,7 ,10 ,14}, "m9"},
        //    {new int[]{0 ,4 ,7 ,11 ,14}, "M9"}
        //};

        // translate a chord value to the chord name given the given the root note.
        // _ is the place where the octave is inserted
        // __ is where octave+1 is inserted
        public static Dictionary<string, Dictionary<string, string>> TranslateChordValueToChordName = new Dictionary<string, Dictionary<string, string>>
        {
            { "I", new Dictionary<string, string>
                {
                    { "C", "C_" },
                    { "C#", "C#_" },
                {"Db", "Db_" },
                { "D", "D_" },
                { "D#", "D#_" },
                {"Eb", "Eb_" },
                {"E", "E_" },
                {"E#", "E#_" },
                {"Fb", "Fb_" },
                {"F", "F_" },
                {"F#", "E_" },
                {"Gb", "Gb_" },
                {"G", "G_" },
                {"G#", "G#_" },
                {"Ab", "Ab_" },
                {"A", "A_" },
                {"A#", "A#_" },
                {"Bb", "Bb_" },
                {"B", "B_" },
                {"B#", "B#_" },
                {"Cb", "Cb_" },
                }
            },
            {"i",  new Dictionary<string, string>
                {
                    { "C", "C_m" },
                    { "C#", "C#_m" },
                {"Db", "Db_m" },
                { "D", "D#_m" },
                { "D#", "D#_" },
                {"Eb", "Eb_m" },
                {"E", "E_m" },
                {"E#", "E#_m" },
                {"Fb", "Fb_m" },
                {"F", "F_m" },
                {"F#", "E_m" },
                {"Gb", "Gb_m" },
                {"G", "G_m" },
                {"G#", "G#_m" },
                {"Ab", "Ab_m" },
                {"A", "A_m" },
                {"A#", "A#_m" },
                {"Bb", "Bb_m" },
                {"B", "B_m" },
                {"B#", "B#_m" },
                {"Cb", "Cb_m" },
                }
            },
            {"ii",  new Dictionary<string, string>
                {
                    { "C", "D_m" },
                    { "C#", "D#_m" },
                {"Db", "Eb_m" },
                { "D", "E#_m" },
                { "D#", "E#_m" },
                {"Eb", "Fb_m" },
                {"E", "F_m" },
                {"E#", "F#_m" },
                {"Fb", "Gb_m" },
                {"F", "G_m" },
                {"F#", "G_m" },
                {"Gb", "Ab_m" },
                {"G", "A_m" },
                {"G#", "A#_m" },
                {"Ab", "Bb_m" },
                {"A", "B_m" },
                {"A#", "B#_m" },
                {"Bb", "Cb__m" },
                {"B", "C__m" },
                {"B#", "C#__m" },
                {"Cb", "Cb__m" },
                }
            },
            { "ii°", new Dictionary<string, string>
                {
                { "C", "D_m°" },
                { "C#", "D#_m°" },
                {"Db", "Eb_m°" },
                { "D", "E#_m°" },
                { "D#", "E#_m°" },
                {"Eb", "Fb_m°" },
                {"E", "F_m°" },
                {"E#", "F#_m°" },
                {"Fb", "Gb_m°" },
                {"F", "G_m°" },
                {"F#", "G_m°" },
                {"Gb", "Ab_m°" },
                {"G", "A_m°" },
                {"G#", "A#_m°" },
                {"Ab", "Bb_m°" },
                {"A", "B_m°" },
                {"A#", "B#_m°" },
                {"Bb", "Cb__m°" },
                {"B", "C__m°" },
                {"B#", "C#__m°" },
                {"Cb", "Db__m°" },
                }
            },
            { "III", new Dictionary<string, string>
                {
                {"C", "E_" },
                {"C#", "F_" },
                {"Db", "F_" },
                {"D", "F#_" },
                {"D#", "G_" },
                {"Eb", "G_" },
                {"E", "G#_" },
                {"E#", "A_" },
                {"Fb", "G#_" },
                {"F", "A_" },
                {"F#", "A#_" },
                {"Gb", "Bb_" },
                {"G", "B_" },
                {"G#", "C__" },
                {"Ab", "C__" },
                {"A", "C#__" },
                {"A#", "D__" },
                {"Bb", "D__" },
                {"B", "Eb__" },
                {"B#", "E__" },
                {"Cb", "Eb__" },
                }
            },
            { "iii", new Dictionary<string, string>
                {
                {"C", "E_m" },
                {"C#", "F_m" },
                {"Db", "F_m" },
                {"D", "F#_m" },
                {"D#", "G_m" },
                {"Eb", "G_m" },
                {"E", "G#_m" },
                {"E#", "A_m" },
                {"Fb", "G#_m" },
                {"F", "A_m" },
                {"F#", "A#_m" },
                {"Gb", "Bb_m" },
                {"G", "B_m" },
                {"G#", "C__m" },
                {"Ab", "C__m" },
                {"A", "C#__m" },
                {"A#", "D__m" },
                {"Bb", "D__m" },
                {"B", "Eb__m" },
                {"B#", "E__m" },
                {"Cb", "Eb__m" },
                }
            },
            { "III+", new Dictionary<string, string>
                {
                {"C", "E_+" },
                {"C#", "F_+" },
                {"Db", "F_+" },
                {"D", "F#_+" },
                {"D#", "G_+" },
                {"Eb", "G_+" },
                {"E", "G#_+" },
                {"E#", "A_+" },
                {"Fb", "G#_+" },
                {"F", "A_+" },
                {"F#", "A#_+" },
                {"Gb", "Bb_+" },
                {"G", "B_+" },
                {"G#", "C__+" },
                {"Ab", "C__+" },
                {"A", "C#__+" },
                {"A#", "D__+" },
                {"Bb", "D__+" },
                {"B", "Eb__+" },
                {"Cb", "Eb__+" },
                {"B#", "E__+" },
                }
            },
            { "IV", new Dictionary<string, string>
                {
                {"C", "F_" },
                {"C#", "F#_" },
                {"Db", "Gb_" },
                {"D", "G_" },
                {"D#", "G#_" },
                {"Eb", "Ab_" },
                {"E", "A_" },
                {"E#", "A#_" },
                {"Fb", "Bb_" },
                {"F", "Bb_" },
                {"F#", "B_" },
                {"Gb", "B_" },
                {"G", "C__" },
                {"G#", "C#__" },
                {"Ab", "Db__" },
                {"A", "D__" },
                {"A#", "D#__" },
                {"Bb", "Eb__" },
                {"B", "E__" },
                {"Cb", "E__" },
                {"B#", "F__" },
                }
            },
            { "iv", new Dictionary<string, string>
                {
                {"C", "F_m" },
                {"C#", "F#_m" },
                {"Db", "Gb_m" },
                {"D", "G_m" },
                {"D#", "G#_m" },
                {"Eb", "Ab_m" },
                {"E", "A_m" },
                {"E#", "A#_m" },
                {"Fb", "Bb_m" },
                {"F", "Bb_m" },
                {"F#", "B_m" },
                {"Gb", "B_m" },
                {"G", "C__m" },
                {"G#", "C#__m" },
                {"Ab", "Db__m" },
                {"A", "D__m" },
                {"A#", "D#__m" },
                {"Bb", "Eb__m" },
                {"B", "E__m" },
                {"Cb", "E__m" },
                {"B#", "F__m" },
                }
            },

            { "V", new Dictionary<string, string>
                {
                {"C", "G_" },
                {"C#", "G#_" },
                {"Db", "Ab_" },
                {"D", "A_" },
                {"D#", "A#_" },
                {"Eb", "Bb_" },
                {"E", "B_" },
                {"E#", "C__" },
                {"Fb", "B__" },
                {"F", "C__" },
                {"F#", "C#__" },
                {"Gb", "Db__" },
                {"G", "D__" },
                {"G#", "D#__" },
                {"Ab", "Eb__" },
                {"A", "E__" },
                {"A#", "F__" },
                {"Bb", "F__" },
                {"B", "Gb__" },
                {"Cb", "Gb__" },
                {"B#", "G__" },
                }
            },
            { "v", new Dictionary<string, string>
                {
                {"C", "G_m" },
                {"C#", "G#_m" },
                {"Db", "Ab_m" },
                {"D", "A_m" },
                {"D#", "A#_m" },
                {"Eb", "Bb_m" },
                {"E", "B_m" },
                {"E#", "C__m" },
                {"Fb", "B__m" },
                {"F", "C__m" },
                {"F#", "C#__m" },
                {"Gb", "Db__m" },
                {"G", "D__m" },
                {"G#", "D#__m" },
                {"Ab", "Eb__m" },
                {"A", "E__m" },
                {"A#", "F__m" },
                {"Bb", "F__m" },
                {"B", "Gb__m" },
                {"Cb", "Gb__m" },
                {"B#", "G__m" },
                }
            },
           { "vi", new Dictionary<string, string>
                {
                {"C", "A_m" },
                {"C#", "A#_m" },
                {"Db", "Bb_m" },
                {"D", "B_m" },
                {"D#", "C_m" },
                {"Eb", "C__m" },
                {"E", "C#__m" },
                {"E#", "D__m" },
                {"Fb", "D__m" },
                {"F", "D__m" },
                {"F#", "D#__m" },
                {"Gb", "Eb__m" },
                {"G", "E__m" },
                {"G#", "F__m" },
                {"Ab", "F__m" },
                {"A", "F#__m" },
                {"A#", "G__m" },
                {"Bb", "G__m" },
                {"B", "Ab__m" },
                {"Cb", "Ab__m" },
                {"B#", "A__m" },
                }
            },
            { "#vi°", new Dictionary<string, string>
                {
                {"C", "Bb_m" },
                {"C#", "B_m" },
                {"Db", "B_m" },
                {"D", "C__m" },
                {"D#", "C#__m" },
                {"Eb", "Db__m" },
                {"E", "D__m" },
                {"E#", "D#__m" },
                {"Fb", "D__m" },
                {"F", "Eb__m" },
                {"F#", "E__m" },
                {"Gb", "E__m" },
                {"G", "F__m" },
                {"G#", "F#__m" },
                {"Ab", "Gb__m" },
                {"A", "G__m" },
                {"A#", "G#__m" },
                {"Bb", "Ab__m" },
                {"B", "A__m" },
                {"Cb", "A__m" },
                {"B#", "Bb__m" },
                }
            },
            { "vii°", new Dictionary<string, string>
                {
                {"C", "B_m°" },
                {"C#", "C__m°" },
                {"Db", "C__m°" },
                {"D", "C#__m°" },
                {"D#", "D__m°" },
                {"Eb", "D__m°" },
                {"E", "Eb__m°" },
                {"E#", "E__m°" },
                {"Fb", "Eb__m°" },
                {"F", "E__m°" },
                {"F#", "F__m°" },
                {"Gb", "F__m°" },
                {"G", "F#__m°" },
                {"G#", "G__m°" },
                {"Ab", "G__m°" },
                {"A", "G#__m°" },
                {"A#", "A__m°" },
                {"Bb", "A__m°" },
                {"B", "Bb__m°" },
                {"Cb", "Bb__m°" },
                {"B#", "B__m°" },
                }
            },
            { "VII", new Dictionary<string, string>
                {
                {"C", "B_" },
                {"C#", "C__" },
                {"Db", "C__" },
                {"D", "C#__" },
                {"D#", "D__" },
                {"Eb", "D__" },
                {"E", "Eb__" },
                {"E#", "E__" },
                {"Fb", "Eb__" },
                {"F", "E__" },
                {"F#", "F__" },
                {"Gb", "F__" },
                {"G", "F#__" },
                {"G#", "G__" },
                {"Ab", "G__" },
                {"A", "G#__" },
                {"A#", "A__" },
                {"Bb", "A__" },
                {"B", "Bb__" },
                {"Cb", "Bb__" },
                {"B#", "B__" },
                }
            },
            { "rest", new Dictionary<string, string>
                {
                {"C", "" },
                {"C#", "" },
                {"Db", "" },
                {"D", "" },
                {"D#", "" },
                {"Eb", "" },
                {"E", "" },
                {"E#", "" },
                {"Fb", "" },
                {"F", "" },
                {"F#", "" },
                {"Gb", "" },
                {"G", "" },
                {"G#", "" },
                {"Ab", "" },
                {"A", "" },
                {"A#", "" },
                {"Bb", "" },
                {"B", "" },
                {"Cb", "" },
                {"B#", "" },
                }
            },
        };
        public static int[] NormalizeChord(int[] chord)
        {
            int min = int.MaxValue;
            foreach (int note in chord)
            {
                if (note < min)
                {
                    min = note;
                }
            }
            int[] normalizedChord = new int[chord.Length];
            for (int i = 0; i < chord.Length; i++)
            {
                normalizedChord[i] = (chord[i] - min + 12) % 12;
            }
            // sort normalizedChord in ascending order
            normalizedChord = normalizedChord.OrderBy(n => n).ToArray();
            return normalizedChord;
        }
        public static Dictionary<string, int[]> Scales = new Dictionary<string, int[]>
        {
            {"Pentatonic", new int[] { 0, 2, 4, 7, 9 } },
            {"Blues", new int[] { 0, 3, 5, 6, 7, 10 } },
            {"Major", new int[] { 0, 2, 4, 5, 7, 9, 11 } },
            {"Minor", new int[] { 0, 2, 3, 5, 7, 8, 10 } },
            {"Harmonic Minor", new int[] { 0, 2, 3, 5, 7, 8, 11 } },
            {"Melodic Minor", new int[] { 0, 2, 3, 5, 7, 9, 11 } },
            {"Diminished", new int[] { 0, 2, 3, 5, 6, 8, 9 } },
            {"Whole Tone", new int[] { 0, 2, 4, 6, 8,10 } },
            {"Chromatic", new int[] {0 ,1 ,2 ,3 ,4 ,5 ,6 ,7 ,8 ,9 ,10 ,11 } },
            {"Ionian", new int[] { 0, 2, 4, 5, 7, 9, 11 }  },
            {"Dorian", new int[] { 0, 2, 3, 5, 7, 9, 10 } },
            {"Phrygian", new int[] { 0, 1, 3, 5, 7, 8, 10 } },
            {"Lydian", new int[] { 0, 2, 4, 6, 7, 9, 11 } },
            {"Mixolydian", new int[] { 0, 2, 4, 5, 7, 9, 10 } },
            {"Aeolian", new int[] { 0, 2, 3, 5, 7, 8, 10 } },
            {"Locrian", new int[] { 0, 1, 3, 5, 6, 8, 10 } }
        };
        public static Dictionary<int[], string> ScalesReverse = new Dictionary<int[], string>
        {
            { new int[] { 0, 2, 4, 7, 9 }, "Pentatonic" },
            { new int[] { 0, 3, 5, 6, 7, 10 }, "Blues" },
            { new int[] { 0, 2, 4, 5, 7, 9, 11 }, "Major" },
            { new int[] { 0, 2, 3, 5, 7, 8, 10 }, "Minor" },
            { new int[] { 0, 2, 3, 5, 7, 8, 11 }, "Harmonic Minor" },
            { new int[] { 0, 2, 3, 5, 7, 9, 11 }, "Melodic Minor" },
            { new int[] { 0, 2, 3, 5, 6, 8, 9 }, "Diminished" },
            { new int[] { 0, 2, 4, 6, 8, 10 }, "Whole Tone" },
            { new int[] { 0 ,1 ,2 ,3 ,4 ,5 ,6 ,7 ,8 ,9 ,10 ,11 }, "Chromatic" },
            { new int[] { 0, 2, 4, 6, 7, 9, 11 }, "Lydian" },
            { new int[] { 0, 2, 4, 5, 7, 9, 10 }, "Mixolydian" },
            { new int[] { 0, 2, 3, 5, 7, 8, 10 }, "Aeolian" },
            { new int[] { 0, 1, 3, 5, 6, 8, 10 }, "Locrian" }
        };
        public static Dictionary<string, int> CircleOfFifths = new Dictionary<string, int>()
        {
            {"C", 0}, {"a", 0},
            {"G", 7}, {"e", 7},
            {"D", 14 % 12}, {"b", 14 % 12},
            {"A", 21 % 12}, {"f#", 21 % 12},
            {"E", 28 % 12}, {"c#", 28 % 12},
            {"B", 35 % 12}, {"g#", 35 % 12},
            {"F#", 42 % 12}, {"d#", 42 % 12},
            {"Gb", 42 % 12}, {"eb", 42 % 12},
            {"Db", 49 % 12}, {"bb", 49 % 12},
            {"Ab", 56 % 12}, {"f", 56 % 12},
            {"Eb", 63 % 12}, {"c", 63 % 12},
            {"Bb", 70 % 12}, {"g", 70 % 12},
            {"F", 77 % 12}, {"d", 77 % 12}
        };
        public static Dictionary<int, string> MajorCircleOfFifthsReverse = new Dictionary<int, string>()
        {
            {0, "C"},
            {7, "G"},
            {14 % 12, "D"},
            {21 % 12, "A"},
            {28 % 12, "E"},
            {35 % 12, "B"},
            {42 % 12, "F#/Gb"},
            {49 % 12, "Db"},
            {56 % 12, "Ab"},
            {63 % 12, "Eb"},
            {70 % 12, "Bb"},
            {77 % 12, "F"}
        };

        public static Dictionary<int, string> MinorCircleOfFifthsReverse = new Dictionary<int, string>()
        {
            {0, "a"},
            {7, "e"},
            {14 % 12, "b"},
            {21 % 12, "f#"},
            {28 % 12, "c#"},
            {35 % 12, "g#"},
            {42 % 12, "d#/eb"},
            {49 % 12, "bb"},
            {56 % 12, "f"},
            {63 % 12, "c"},
            {70 % 12, "g"},
            {77 % 12, "d"}
        };
        public static Dictionary<string, int[]> Sharps = new Dictionary<string, int[]>()
        {
            {"C", new int[] { 0 }  },
            {"G", new int[] { 7 } },
            {"D", new int[] { 2, 9 } },
            {"A", new int[] { 4, 11 } },
            {"E", new int[] { 6, 1 } },
            {"B", new int[] { 8, 3 } },
            {"F#", new int[] { 10, 5 } }
        };
        public static Dictionary<string, int[]> Flats = new Dictionary<string, int[]>()
        {
            {"C", new int[] { 0 } },
            {"F", new int[] { 5 } },
            {"Bb", new int[] { 10, 3 } },
            {"Eb", new int[] { 3, 8 } },
            {"Ab", new int[] { 8, 1 } },
            {"Db", new int[] { 1, 6 } },
            {"Gb", new int[] { 6, 11 } }
        };
        public static string? GetNoteNameFromRoot(string root, int note)
        {
            if (!Notes.TryGetValue(root, out int rootNote))
            {
                return null;
            }
            int transposedNote = (rootNote + note) % 12;
            return NotesReverse[transposedNote];
        }
        //public static string? GetChordNameFromRoot(string root, int[] chord)
        //{
        //    if (!Notes.TryGetValue(root, out int rootNote))
        //    {
        //        return null;
        //    }

        //    int[] transposedChord = new int[chord.Length];
        //    for (int i = 0; i < chord.Length; i++)
        //    {
        //        transposedChord[i] = (rootNote + chord[i]) % 12;
        //    }
        //    return ChordsReverse[transposedChord];
        //}

        public static int[]? GetChordNotesFromRoot(string root, string chordType)
        {
            if (!Notes.TryGetValue(root, out int rootNote) || !Chords.TryGetValue(chordType, out int[]? intervals))
            {
                return null;
            }

            int[] chordNotes = new int[intervals.Length];
            for (int i = 0; i < intervals.Length; i++)
            {
                chordNotes[i] = (rootNote + intervals[i]);
            }
            return chordNotes;
        }
        public static int[]? GetScaleNotesFromRoot(string root, string scaleType)
        {
            if (!Notes.TryGetValue(root, out int rootNote) || !Scales.TryGetValue(scaleType, out int[]? intervals))
            {
                return null;
            }
            int[] scaleNotes = new int[intervals.Length];
            for (int i = 0; i < intervals.Length; i++)
            {
                scaleNotes[i] = (rootNote + intervals[i]);
            }
            return scaleNotes;
        }

        public static string? GetScaleNameFromRoot(string root, int[] scale)
        {
            if (!Notes.TryGetValue(root, out int rootNote))
            {
                return null;
            }
            int[] transposedScale = new int[scale.Length];
            for (int i = 0; i < scale.Length; i++)
            {
                transposedScale[i] = (rootNote + scale[i]) % 12;
            }
            return ScalesReverse[transposedScale];
        }
        public static Dictionary<string, string[]> MajorProgressionStateTransitions = new Dictionary<string, string[]>()
        {
            {"I", new string[] { "I", "ii", "iii", "IV", "V", "vi", "vii°", "rest" } },
            {"ii", new string[] { "I", "ii", "V", "vii°" , "rest" } },
            {"iii", new string[] { "I", "ii", "iii","IV", "vi", "rest" } },
            {"IV", new string[] { "I", "ii", "iii", "IV", "V", "vii°", "rest" } },
            {"V", new string[] { "I", "V", "vi", "rest" } },
            {"vi", new string[] { "I", "ii", "iii", "IV", "V", "vi", "rest" } },
            {"vii°", new string[] { "I", "iii", "vii°", "rest" } },
            {"rest", new string[] {"I", "ii", "iii", "IV", "V", "vi", "vii°", "rest" } }
        };
        public static Dictionary<string, string[]> MinorProgressionStateTransitions = new Dictionary<string, string[]>()
        {
            {"i", new string[] { "i", "ii", "ii°", "III", "III+", "iv", "IV", "V", "v", "VI", "#vi°", "vii°", "VII", "rest" } },
            {"ii°", new string[] { "i", "ii°", "III", "V", "v", "vii°", "VII", "rest" } },
            {"ii", new string[] { "i", "ii", "III", "V", "v", "vii°", "VII", "rest" } },
            {"III", new string[] { "i", "III",  "iv","IV", "VI", "#vi°", "vii°", "VII", "rest" } },
            {"III+", new string[] { "i", "III+", "iv", "IV", "VI", "#vi°", "vii°", "VII", "rest" } },
            {"iv", new string[] { "i", "iv", "V", "v", "vii°", "VII", "rest" } },
            {"IV", new string[] { "i", "IV", "V", "v", "vii°", "VII", "rest" } },
            {"V", new string[] { "i", "V", "VI", "#vi°", "rest" } },
            {"v", new string[] { "i", "v", "VI", "#vi°", "rest" } },
            {"VI", new string[] { "i", "III", "III+", "iv", "IV", "V", "v", "VI", "vii°", "VII", "rest" } },
            {"#vi°", new string[] { "i", "III", "III+", "iv", "IV", "V", "v", "#vi°", "vii°", "VII", "rest" } },
            {"vii°", new string[] { "i", "vii°", "rest" } },
            {"VII", new string[] { "i", "VII", "rest" } },
            {"rest", new string[] {"i", "ii", "ii°", "III", "III+", "iv", "IV", "V", "v", "VI", "#vi°", "vii°", "VII", "rest" } }
        };

        public static Dictionary<string, int[]> NormalizedChordValues = new Dictionary<string, int[]>()
        {
            {"I", new int[] { 0, 4, 7 }},
            {"i", new int[] { 0, 3, 7 }},
            {"II", new int[] { 2, 6, 9 }},
            {"ii", new int[] { 2, 5, 9 }},
            {"ii°", new int[] { 2, 5, 8 }},
            {"III", new int[] { 4, 7, 11 }},
            {"III+", new int[] { 4, 8, 12 }},
            {"iii", new int[] { 4, 6, 11 }},
            {"IV", new int[] { 5, 9, 12 }},
            {"iv", new int[] { 5, 8, 12 }},
            {"V", new int[] { 7, 11, 14 }},
            {"v", new int[] { 7, 10, 14 }},
            {"VI", new int[] { 9, 12, 16 }},
            {"vi", new int[] { 9, 11, 16 }},
            {"#vi°", new int[] { 10, 13, 16 }},
            {"VII", new int[] { 11, 14, 17 }},
            {"vii°", new int[] { 11, 13, 16 }},
            {"rest", new int[] { } }
        };

        /// <summary>
        /// Inverts a chord by the specified inversion number. Only works on triad chords and first and second inversion
        /// </summary>
        /// <param name="chord">list of note numbers in chord</param>
        /// <param name="inversion">inversion number (0, 1, or 2)</param>
        /// <returns></returns>
        public static int[] InvertChord(int[] chord, int inversion)
        {
            if (inversion == 0) return chord;
            int[] invertedChord = new int[chord.Length];
            switch (inversion)
            {
                case 1:
                    invertedChord[0] = chord[1];
                    invertedChord[1] = chord[2];
                    invertedChord[2] = chord[0] + 12;
                    break;
                case 2:
                    invertedChord[0] = chord[2];
                    invertedChord[1] = chord[0] + 12;
                    invertedChord[2] = chord[1] + 12;
                    break;
                default:
                    return chord;
            }
            return invertedChord;
        }
        public enum WeightType
        {
            pppp, ppp, pp, p, mp, mf, f, ff, fff, ffff
        }
        public enum ArticulationType
        {
            staccato, legato, accent
        }
        public enum ChordBindingType
        {
            ArpeggioUp, ArpeggioDown, StrumUp, StrumDown, Block
        }
        public enum SpaceType
        {
            Direct, Indirect
        }
        public class EffortType
        {
            public WeightType Weight { get; set; } = WeightType.mf;
            public ArticulationType Articulation { get; set; } = ArticulationType.legato;
            public ChordBindingType ChordBinding { get; set; } = ChordBindingType.Block;
            public SpaceType Space { get; set; } = SpaceType.Direct;
            public EffortType(WeightType weight, ArticulationType articulation, ChordBindingType chordBinding, SpaceType space)
            {
                Weight = weight;
                Articulation = articulation;
                ChordBinding = chordBinding;
                Space = space;
            }
            public EffortType() { }
        }
        public static Dictionary<Music.WeightType, double> WeightToVolumeDb = new()
        {
            { Music.WeightType.pppp, -25 },
            { Music.WeightType.ppp, -20 },
            { Music.WeightType.pp, -15 },
            { Music.WeightType.p, -10 },
            { Music.WeightType.mp, -5 },
            { Music.WeightType.mf, 0 },
            { Music.WeightType.f, 5 },
            { Music.WeightType.ff, 10 },
            { Music.WeightType.fff, 15 },
            { Music.WeightType.ffff, 20 }
        };

    }

}



