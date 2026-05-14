using CMGDBEditor.Helpers;
using CMGWpf.Types;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace CMGDBEditor.Model
{
    /// <summary>
    /// Represents a single note and its number of beats to be played. A note is in standard note format (e.g. C4, D#5, Gb3, REST) and beats is a non-negative float representing the number of beats to play the note for. Note value is converted to MIDI format for storage in the database. Note value of REST is stored as -1 in the database.
    /// </summary>
    public class NoteItem
    {
        public string Note { get; set; } = "";
        public float Value { get; set; } = float.NaN;
        public float Beats { get; set; } = 0;
        public NoteItem()
        {
        }
        private const string notePattern = @"([A-Ga-g])([#,b]?)(\d)([+-]\d\d)?";
        public static bool CheckNote(string note)
        {
            if (note.Trim().Equals("REST", StringComparison.CurrentCultureIgnoreCase)) return true;
            var match = Regex.Match(note, notePattern);
            return (match.Success && match.Value == note);
        }
        private readonly static Dictionary<string, int> midi = new Dictionary<string, int> { { "C", 0 }, { "D", 2 }, { "E", 4 }, { "F", 5 }, { "G", 7 }, { "A", 9 }, { "B", 11 } };
        public void NoteToMidi()
        {
            if (string.Equals(Note.Trim(), "REST", StringComparison.CurrentCultureIgnoreCase)) { Value = -1; return; }
            var match = Regex.Match(Note, notePattern);
            if (!(match.Success && match.Value == Note)) { Value = float.NaN; return; }
            var noteName = match.Groups[1].Value.ToUpper();
            var accidentalPart = match.Groups[2].Value;
            var octavePart = match.Groups[3].Value;
            var centsPart = match.Groups[4].Value;
            if (!midi.TryGetValue(noteName, out int midiValue)) { Value = float.NaN; return; }
            if (accidentalPart == "#") midiValue++;
            else if (accidentalPart == "b") midiValue--;
            midiValue += (int.Parse(octavePart) + 1) * 12;
            if (!string.IsNullOrEmpty(centsPart)) midiValue += int.Parse(centsPart) / 100;
            Value = midiValue;
        }
    }


}
