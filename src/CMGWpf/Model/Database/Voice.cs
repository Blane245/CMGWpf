
using CMGWpf.Helpers;
using CMGWpf.Model.Generators;
using CMGWpf.Types;
using System.Collections.ObjectModel;
namespace CMGWpf.Model.Database
{
    public class Voice
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public StochasticTypes.TIMBRE Timbre { get; set; } = StochasticTypes.TIMBRE.sustained;
        public float RegisterLo { get; set; } = 0;
        public float RegisterHi { get; set; } = 0;
        public float Duration { get; set; } = 0;
        public string SoundFontFile { get; set; } = "";
        public string PresetName { get; set; } = "";

        // EF Core navigation property for many-to-many relationship
        public virtual ICollection<Ensemble> Ensembles { get; set; } = new List<Ensemble>();

        public Voice Clone()
        {
            Voice n = (Voice)this.MemberwiseClone();
            return n;
        }
        public static ObservableCollection<Message> Validate(Voice voice, string newName, ObservableCollection<Voice> allVoices)
        {
            ObservableCollection<Message> errors = new ObservableCollection<Message>();
            if (newName.Trim(' ', '\t') == "")
            {
                Messages.Add(errors, "Voice name must not be blank", true);
            }
            
            // find the voice object so it can be skipped
            int voiceIndex = -1;
            for (int i = 0; i < allVoices.Count; i++)
            {
                if (allVoices[i].Name == voice.Name) { voiceIndex = i; break; }
            }
            // see if there is another voice with the same name
            for (int i = 0; i < allVoices.Count; i++)
            {
                if (i == voiceIndex) continue;
                if (newName == allVoices[i].Name)
                {
                    Messages.Add(errors, $"Voice name '{newName}' must be unique", true);
                    break;
                }
            }

            // check register range
            if (voice.RegisterLo < 0 || voice.RegisterHi > 127 || voice.RegisterLo > voice.RegisterHi)
            {
                Messages.Add(errors, "Voice register lo must be less than or equal to register hi and register hi must be within the range lo-127", true);
            }

            // validate soundfont file and preset
            if (voice.SoundFontFile.Trim(' ', '\t') == "")
            {
                Messages.Add(errors, "No soundfont file specified for voice", true);
            }
            if (voice.PresetName.Trim(' ', '\t') == "")
            {
                Messages.Add(errors, "No preset name specified for voice", true);
            }
            return errors;
        }
    }
}
