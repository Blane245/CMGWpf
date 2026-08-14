using CMGWpf.Helpers;
using CMGWpf.Types;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using CMGWpf.Data;

namespace CMGWpf.Model.Database
{
    public class ChordSequence
    {
        public string Name { get; set; } = string.Empty;
        //public string SoundFontFileName { get; set; } = "";
        //public string? PresetName { get; set; } = null;
        //public double BPM { get; set; }  = 60;
        //public string RootNote { get; set; }  = "C";
        //public int RootOctave { get; set; }  = 4;
        public bool IsMajor { get; set; }  = true;
        public ObservableCollection<ChordItem> Items { get; set; } = new ObservableCollection<ChordItem>();

        public ChordSequence Clone() {
            ChordSequence n = (ChordSequence)this.MemberwiseClone();
            n.Items = [.. this.Items];
            return n; 
        }
        public static async Task<ObservableCollection<Message>> Validate(ChordSequence chordSequence, string newName, CmgDbContext dbContext)
        {
            ObservableCollection<Message> errors = [];
            if (newName.Trim(' ', '\t', ',') == "")
            {
                Messages.Add(errors, "Chord sequence name must not be blank or contain a comma.", true);
            }

            // Handle rename
            if (chordSequence.Name != newName)
            {
                // check for uniqueness of the new name
                bool exists = await dbContext.ChordSequences
                    .AnyAsync(cs => cs.Name == newName);
                if (exists) {
                    Messages.Add(errors, $"Chord sequence name '{newName}' must be unique", true);
                    }
                }
            
            //// check presence of SoundFont and preset   
            //if (chordSequence.SoundFontFileName == "")
            //{
            //    Messages.Add(errors, $"Chord sequence must have a valid SoundFont.", true);
            //}
            //if (chordSequence.PresetName == "")
            //{
            //    Messages.Add(errors, $"Chord sequence must have a valid Preset.", true);
            //}

            return errors;
        }
    }
}
