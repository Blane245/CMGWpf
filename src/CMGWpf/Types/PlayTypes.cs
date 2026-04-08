using CMGWpf.SoundFont_2;
using System.Collections.ObjectModel;
using System.Windows.Media;
using static CMGWpf.Types.PresetTypes;

namespace CMGWpf.Types
{
    public static class PlayTypes
    {
        public class ReadyPlayOutput
        {
            public ObservableCollection<Model.Generators.Generator> Generators { get; set; } = [];
            public double Duration { get; set; } = 0;
            public string ErrorMessage { get; set; } = "";
        }

        public record class TimeMidiPoint
        {
            public double Time { get; init; } = 0;
            public int Midi { get; init; } = 0;
        }
        public class TimeMidiLine
        {
            public TimeMidiPoint Start { get; init; } = new TimeMidiPoint();
            public TimeMidiPoint End { get; init; } = new TimeMidiPoint();
        }
        public class FinalVoice
        {
            public string InstrumentName { get; set; } = "";
            public SampleHeader? SampleHeader { get; set; } = null;
            public Dictionary<GenOp, short> Generators = [];
        }
        public class SF_Preset
        {
            public string SoundFontName { get; set; } = "";
            public string PresetName { get; set; } = "";
        }
        public class PresetColor
        {
            public string SoundFontName { get; set; } = "";
            public string PresetName { get; set; } = "";
            public Color Color { get; set; } = new Color();
            public Brush ColorBrush => new SolidColorBrush(Color);
        }
        public const int SampleRate = 44100;
    }

}
