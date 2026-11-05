using CMGWpf.Model.Database;
using CMGWpf.Model.Generators;
using CMGWpf.SoundFont_2;
using CMGWpf.Utilities;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using CMGWpf.Types; 
using System.Xml;
using static CMGWpf.Model.Generators.StochasticTypes;

namespace CMGWpf.Model
{
    public class Voice
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SoundFontFileName { get; set; } = string.Empty;
        public SoundFont? SoundFont { get; set; } = null;
        public string PresetName { get; set; } = string.Empty;
        public Preset? Preset { get; set; } = null;
        //private TIMBRE timbre = TIMBRE.sustained;
        public TIMBRE Timbre { get; set; } = TIMBRE.sustained;
        //public TIMBRE Timbre
        //{
        //    get => timbre;
        //    set
        //    {
        //        if (timbre != value)
        //        {
        //            timbre = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}
        public double RegisterLo { get; set; } = 0;
        public double RegisterHi { get; set; } = 0;
        private double _duration = 0;
        public double Duration { get => Math.Round(_duration, 3); set { _duration = value; } }
        // a change to the mute property will cause a change in the UI for the composition
        //private bool muted;
        //public bool Muted
        //{
        //    get => muted;
        //    set { if (muted != value) { muted = value; OnPropertyChanged(); } }
        //}
        public bool Muted { get; set; } = false;
        public double Volume { get; set; } = 0;
        public double Velocity { get; set; } = 63;
        public double Delta { get; set; } = 0;
        public Voice Clone()
        {
            return (Voice)MemberwiseClone();
        }
        public void AppendXML(XmlDocument _doc, XmlElement voiceElem)
        {

            voiceElem.SetAttribute("name", Name);
            voiceElem.SetAttribute("description", Description);
            voiceElem.SetAttribute("soundFontFile", SoundFontFileName);
            voiceElem.SetAttribute("presetName", PresetName);
            voiceElem.SetAttribute("timbre", Timbre.ToString());
            voiceElem.SetAttribute("registerLo", RegisterLo.ToString());
            voiceElem.SetAttribute("registerHi", RegisterHi.ToString());
            voiceElem.SetAttribute("duration", Duration.ToString());
            voiceElem.SetAttribute("muted", Muted.ToString());
            voiceElem.SetAttribute("volume", Volume.ToString());
            voiceElem.SetAttribute("velocity", Velocity.ToString());
            voiceElem.SetAttribute("delta", Delta.ToString());
        }
        public static Voice LoadXML(XmlElement voiceElem)
        {
            Voice voice = new()
            {
                Name = XMLFunctions.GetAttributeString(voiceElem, "name", ""),
                Description = XMLFunctions.GetAttributeString(voiceElem, "description", ""),
                SoundFontFileName = XMLFunctions.GetAttributeString(voiceElem, "soundFontFile", ""),
                PresetName = XMLFunctions.GetAttributeString(voiceElem, "presetName", ""),
                Timbre = Enum.Parse<TIMBRE>(XMLFunctions.GetAttributeString(voiceElem, "timbre", "Sustained")),
                RegisterLo = XMLFunctions.GetAttributeDouble(voiceElem, "registerLo", 0),
                RegisterHi = XMLFunctions.GetAttributeDouble(voiceElem, "registerHi", 127),
                Duration = XMLFunctions.GetAttributeDouble(voiceElem, "duration", 1),
                Muted = XMLFunctions.GetAttributeBool(voiceElem, "muted", false),
                Volume = XMLFunctions.GetAttributeDouble(voiceElem, "volume", 0),
                Velocity = XMLFunctions.GetAttributeDouble(voiceElem, "velocity", 64),
                Delta = XMLFunctions.GetAttributeDouble(voiceElem, "delta", 2), // use old format default is missing
            };
            // Read the soundfont from the identified file, and find the preset in the soundfont. If the soundfont or preset cannot be found, then these fields should be cleared. This is because the soundfont and preset are used to generate the sound for the voice, and if they cannot be found, then the voice cannot be generated, so these fields should be cleared to reflect that.
            voice.SoundFont = SoundFontUtilities.GetSoundFont(voice.SoundFontFileName);
            if (voice.PresetName != "" && voice.SoundFont != null)
            {
                voice.Preset = voice.SoundFont.Presets.FirstOrDefault(p => SoundFontUtilities.BankPresetToName(p) == voice.PresetName);
            }
            else
            {
                voice.Preset = null;
                voice.PresetName = "";
            }
            return voice;
        }

        public ObservableCollection<Message> Validate()
        {
            ObservableCollection<Message> errors = new();
            if (string.IsNullOrEmpty(Name))
                errors.Add(new Message() { Text = $"Voice name cannot be empty.", Error = true });
            if (string.IsNullOrEmpty(SoundFontFileName))
                errors.Add(new Message() { Text = $"Voice {Name}: SoundFont file cannot be empty.", Error = true });
            if (string.IsNullOrEmpty(PresetName))
                errors.Add(new Message() { Text = $"Voice {Name}: preset name cannot be empty.", Error = true });
            if (Preset == null)
                errors.Add(new Message() { Text = $"Voice {Name}: Voice preset has not been successfully loaded from the SoundFont file" });
            if (RegisterLo < 0 || RegisterLo > 127)
                errors.Add(new Message() { Text = $"Voice {Name}: Voice register low must be between 0 and 127.", Error = true });
            if (RegisterHi < 0 || RegisterHi > 127)
                errors.Add(new Message() { Text = $"Voice {Name}: register high must be between 0 and 127.", Error = true });
            if (RegisterLo > RegisterHi)
                errors.Add(new Message() { Text = $"Voice {Name}: register low must be less than or equal to register high.", Error = true });
            if (Duration < 0)
                errors.Add(new Message() { Text = $"Voice {Name}: duration must be greater than 0.", Error = true });
            if (Volume < -10 || Volume > 10)
                errors.Add(new Message() { Text = $"Voice {Name}: volume must be between -10 and 10 dB.", Error = true });
            if (Velocity < 0 || Velocity > 127)
                errors.Add(new Message() { Text = $"Voice {Name}: velocity must be between 0 and 127.", Error = true });
            if (Delta < 0 || Delta > 10)
                errors.Add(new Message() { Text = $"Voice {Name}: delta must be between 0 and 10 seconds.", Error = true });
            return errors;
        }
    }
}
