using CMGWpf.SoundFont_2;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
        public TIMBRE Timbre { get; set; }
        public double RegisterLo { get; set; }
        public double RegisterHi { get; set; }
        public double Duration { get; set; }
        // a change to the mute property will cause a change in the UI for the composition
        private bool muted;
        public bool Muted
        { get => muted; 
            set { if (muted != value) { muted = value; OnPropertyChanged(); } } }
        public double Volume { get; set; }
        public double Velocity { get; set; }
        public Voice Clone()
        {
            return (Voice)MemberwiseClone();
        }

    }
}
