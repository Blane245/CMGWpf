using CMGWpf.Model.Generators;
using CMGWpf.SoundFont_2;
using System.Collections.ObjectModel;
using System.Windows.Media;
using static CMGWpf.Types.PresetTypes;

namespace CMGWpf.Types
{
    public static class PlayTypes
    {
        /// <summary>
        /// Represents the output data produced when a play operation is ready, including generated items, duration, and
        /// any error messages.
        /// </summary>
        public class ReadyPlayOutput
        {
            public ObservableCollection<Model.Generators.Generator> Generators { get; set; } = [];
            public double Duration { get; set; } = 0;
            public ObservableCollection<Message> ErrorMessages { get; set; } = [];
        }

        /// <summary>
        /// Represents a point in time associated with a MIDI value.
        /// </summary>
        public record class TimeMidiPoint
        {
            public double Time { get; init; } = 0;
            public double Midi { get; init; } = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        public class TimeMidiLine
        {
            public TimeMidiPoint Start { get; init; } = new TimeMidiPoint();
            public TimeMidiPoint End { get; init; } = new TimeMidiPoint();
        }
        public class TimeMidiPreset
        {
            public TimeMidiLine Line { get; set; } = new TimeMidiLine();
            public string SoundFontName { get; set; } = "";
            public string PresetName { get; set; } = "";
        }
        /// <summary>
        /// Contains the instrument name, its sample header, and the merged SoundFont generators that apply to the instrument
        /// </summary>
        public class FinalVoice
        {
            public string InstrumentName { get; set; } = "";
            public SampleHeader? SampleHeader { get; set; } = null;
            public Dictionary<GenOp, short> Generators = [];
        }
        public class GainEnvelope
        {
            public double Gain { get; init; }
            public double Time { get; init; }
        }
        /// <summary>
        /// This contains all of the information about an instrument source as it is used to build audio samples. It is used principally for report writing
        /// </summary>
        public class InstrumentSource
        {
            public Model.Generators.Generator? Generator { get; set; } = null;
            public double StartTime { get; set; } = 0;
            public double StopTime { get; set; } = 0;
            public string SoundFontName { get; set; } = "";
            public string PresetName { get; set; } = "";
            public double StartPitch { get; set; } = 0;
            public double EndPitch { get; set; } = 0;
            public string Name { get; set; } = "";
            public bool LoopEnabled { get; set; } = false;
            public double LoopStart { get; set; } = 0;
            public double LoopEnd { get; set; } = 0;
            public double RootKey { get; set; } = 0;
            public double StartCents { get; set; } = 0;
            public double EndCents { get; set; } = 0;
            public int SampleRate { get; set; } = 0;
            public int SampleCount { get; set; } = 0;
            public bool AttackEnabled { get; set; } = false;
            public GainEnvelope[] Envelope { get; set; } = [];
        }
        /// <summary>
        /// Represents a preset within a SoundFont, including its associated SoundFont name and preset name.
        /// </summary>
        public class SF_Preset
        {
            public string SoundFontName { get; set; } = "";
            public string PresetName { get; set; } = "";
        }
        /// <summary>
        /// 
        /// </summary>
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
