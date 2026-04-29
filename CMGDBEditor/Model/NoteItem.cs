using CMGWpf.Types;
using System.Text.RegularExpressions;

namespace CMGDBEditor.Model
{
    public class NoteItem
    {
        public string Id { get; set; } = string.Empty;
        public float Value { get; set; } = 0;
        public float Beats { get; set; } = 0;
        public NoteItem(string? Id)
        {
            if (Id != null) this.Id = Id;
            else this.Id = Guid.NewGuid().ToString();
        }
        public static (DBTypes.DbErrorType[], float) Validate(string noteValue, float beats)
        {
            DBTypes.DbErrorType[] errors = [];
            // validate that note value string for a valid format 
            float note = CheckNote(noteValue);
            if (float.IsNaN(note) || note > 127)
            {
                errors = [.. errors, new DBTypes.DbErrorType()
                {
                    type = DBTypes.DBRESPONSETYPE.error,
                    message = $"Note '{noteValue}' is not in proper format or is out of range"
                }];
            }

            // check that the beats value is not negative
            if (beats < 0)
            {
                errors = [.. errors, new DBTypes.DbErrorType()
                {
                    type = DBTypes.DBRESPONSETYPE.error,
                    message = "Note value must not be negative"
                }];
            }
            return (errors, note);
        }
        private const string notePattern = "/([A-G, a-g])([#,b]?)(\\d)([+-]\\d\\d)?/";
        private static float CheckNote(string note)
        {
            if (note.Trim().ToUpper() == "REST") return -1;
            var match = Regex.Match(note, notePattern);
            if (match.Success && match.Value == note)
            {
                // parse the note value
                return NoteToMidi(match);
            }
            else
            {
                return float.NaN;
            }
        }
        private readonly static Dictionary<string, int> midi = new Dictionary<string, int> { { "C", 0 }, { "D", 2 }, { "E", 4 }, { "F", 5 }, { "G", 7 }, { "A", 9 }, { "B", 11 } };
        private static float NoteToMidi(Match match)
        {
            if (string.Equals(match.Value.Trim().ToUpper(), "REST")) return -1;
            var noteName = match.Groups[1].Value.ToUpper();
            var accidentalPart = match.Groups[2].Value;
            var octavePart = match.Groups[3].Value;
            var centsPart = match.Groups[4].Value;
            if (!midi.ContainsKey(noteName)) return -1;
            var midiValue = midi[noteName];
            if (accidentalPart == "#") midiValue++;
            else if (accidentalPart == "b") midiValue--;
            midiValue += (int.Parse(octavePart) + 1) * 12;
            if (!string.IsNullOrEmpty(centsPart)) midiValue += int.Parse(centsPart) / 100;
            return midiValue;
        }


}

    
}
