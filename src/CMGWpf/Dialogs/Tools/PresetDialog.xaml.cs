using CMGWpf.Model;
using CMGWpf.PlayFunctions;
using CMGWpf.PlayFunctions.DSP;
using CMGWpf.PlayFunctions.Utilities;
using CMGWpf.Utilities;
using CMGWpf.View;
using NAudio.Wave;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static CMGWpf.Types.PlayTypes;

namespace CMGWpf.Dialogs
{
    /// <summary>
    /// Interaction logic for PresetDialog.xaml
    /// </summary>
    public partial class PresetDialog : Window, System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        public PresetDialog()
        {
            InitializeComponent();
            DataContext = this;
            this.Closing += PresetDialog_Closing; ;
        }

        private void PresetDialog_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            ToolsViewModel.Instance.PresetDialog = null;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private ObservableCollection<string> soundFontFileNames = new ObservableCollection<string>();
        // load the list of soundfont files from the CMGSoundFontLocation setting for use wen adding/editing voices
        public void LoadSoundFontFileNames()
        {
            soundFontFileNames.Clear();
            string soundFontFileLocation = Properties.Settings.Default.CMGSoundFontLocation;
            if (Directory.Exists(soundFontFileLocation))
            {
                string[] files = Directory.GetFiles(soundFontFileLocation, "*.sf2");
                foreach (string file in files)
                {
                    soundFontFileNames.Add(System.IO.Path.GetFileName(file));
                }
                var sorted = new ObservableCollection<string>(soundFontFileNames.OrderBy(name => name));
                soundFontFileNames = sorted;
            }
        }
        public ObservableCollection<string> SoundFontFileNames
        {
            get
            {
                if (soundFontFileNames.Count == 0)
                    LoadSoundFontFileNames();
                return soundFontFileNames;
            }
        }
        private ObservableCollection<string> _presetNames = new ObservableCollection<string>();
        public ObservableCollection<string> PresetNames
        {
            get => _presetNames;
            set { _presetNames = new ObservableCollection<string>(value.OrderBy(name => name)); OnPropertyChanged(); }
        }
        private SoundFont_2.SoundFont? SoundFont = null;
        private string soundFontFileName = "";
        public string SoundFontFileName
        {
            get { return soundFontFileName; }
            // when the soundfont file changes, load it into the SF buffer and get the list of presets to populate the preset drop down
            set
            {
                soundFontFileName = value;
                SoundFont = SoundFontUtilities.GetSoundFont(soundFontFileName);
                LoadPresets ();
                OnPropertyChanged();
            }
        }
        private void LoadPresets()
        {
            if (SoundFont == null) return;
            PresetNames.Clear();
            foreach (var preset in SoundFont.Presets)
            {
                PresetNames.Add(SoundFontUtilities.BankPresetToName(preset));
            }
            var sorted = new ObservableCollection<string>(PresetNames.OrderBy(name => name));
            PresetNames = sorted;
            Preset = null;
        }
        private void ReloadSoundFont_Click(object sender, RoutedEventArgs e)
        {
            SoundFont = SoundFontUtilities.ReloadSoundFont(soundFontFileName);
            LoadPresets();
        }
        private SoundFont_2.Preset? Preset = null;
        private string presetName = "";
        public string PresetName
        {
            get { return presetName; }
            set
            {
                presetName = value;
                Preset = SoundFontUtilities.GetPreset(soundFontFileName, presetName);
                OnPropertyChanged();
            }
        }

        private double startPitch = 60;
        public double StartPitch { get { return startPitch; } set { startPitch = value; OnPropertyChanged(); } }
        private double endPitch = 60;
        public double EndPitch { get { return endPitch; } set { endPitch = value; OnPropertyChanged(); } }
        private double velocity = 63;
        public double Velocity { get { return velocity; } set { velocity = value; OnPropertyChanged(); } }
        private double duration = 0;
        public double Duration { get { return duration; } set { duration = value; OnPropertyChanged(); } }
        private double interval = 1;
        public double Interval { get { return interval; } set { interval = value; OnPropertyChanged(); } }
        private bool isLooping = true;
        public bool IsLooping { get { return isLooping; } set { isLooping = value; OnPropertyChanged(); } }
        private bool isAttack = true;
        public bool IsAttack { get { return isAttack; } set { isAttack = value; OnPropertyChanged(); } }
        private double reverbDelay = 0;
        public double ReverbDelay { get { return reverbDelay; } set { reverbDelay = value; OnPropertyChanged(); } }
        private double reverbDecay = 0;
        public double ReverbDecay { get { return reverbDecay; } set { reverbDecay = value; OnPropertyChanged(); } }
        private double noiseFrequency = 0;
        public double NoiseFrequency { get { return noiseFrequency; } set { noiseFrequency = value; OnPropertyChanged(); } }
        private double noiseAmplitude = 0;
        public double NoiseAmplitude { get { return noiseAmplitude; } set { noiseAmplitude = value; OnPropertyChanged(); } }
        private double tremoloSpeed = 0;
        public double TremoloSpeed { get { return tremoloSpeed; } set { tremoloSpeed = value; OnPropertyChanged(); } }
        private double tremoloDepth = 0;
        public double TremoloDepth { get { return tremoloDepth; } set { tremoloDepth = value; OnPropertyChanged(); } }
        private MODULATORTYPE tremoloWaveform = MODULATORTYPE.SINE;
        public MODULATORTYPE TremoloWaveform { get { return tremoloWaveform; } set { tremoloWaveform = value; OnPropertyChanged(); } }
        private double vibratoSpeed = 0;
        public double VibratoSpeed { get { return vibratoSpeed; } set { vibratoSpeed = value; OnPropertyChanged(); } }
        private double vibratoDepth = 0;
        public double VibratoDepth { get { return vibratoDepth; } set { vibratoDepth = value; OnPropertyChanged(); } }
        private MODULATORTYPE vibratoWaveform = MODULATORTYPE.SINE;
        public MODULATORTYPE VibratoWaveform { get { return vibratoWaveform; } set { vibratoWaveform = value; OnPropertyChanged(); } }
        public static ObservableCollection<MODULATORTYPE> ModulatorTypes => new(Enum.GetValues<MODULATORTYPE>());
        public struct PresetTableRow
        {
            public string Title;
            public string Value;
        }

        public void Play_Click(object? sender, RoutedEventArgs e)
        {
            // validate the values
            if (string.IsNullOrEmpty(soundFontFileName))
            {
                _ = MessageBox.Show("Please select a SoundFont file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Preset == null)
            {
                _ = MessageBox.Show("Please select a preset.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Interval <= 0)
            {
                _ = MessageBox.Show("Please enter a valid interval.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Duration > Interval)
            {
                _ = MessageBox.Show("Duration must be less than or equal to Interval.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }

            // setup the vibrato and tremolo modulators
            Tremolo Tremolo = new()
            {
                Speed = TremoloSpeed,
                Depth = TremoloDepth,
                WaveForm = TremoloWaveform
            };
            Tremolo Vibrato = new()
            {
                Speed = VibratoSpeed,
                Depth = VibratoDepth,
                WaveForm = VibratoWaveform
            };

            // construct the voices for the preset instruments
            List<FinalVoice> voices = PresetUtilities.BuildVoicesForPresetAtKeyVel(Preset, (int)StartPitch, (int)Velocity);

            // set up the preset sample
            double[] presetSamples = new double[(int)(SampleRate * 2 * Interval)];
            InstrumentSource[] Sources = [];

            // loop through each instrument in the preset and build the sample data
            foreach (var voice in voices)
            {
                // get the sample data for the instrument
                (double[] instrumentSamples, InstrumentSource source) = InstrumentSample.Get(new InstrumentSampleParameters
                {
                    Duration = Duration == 0 ? Interval : Duration,
                    Interval = Interval,
                    StartPitch = StartPitch,
                    EndPitch = EndPitch,
                    VolumeDb = 0,
                    AttackEnabled = IsAttack,
                    LoopEnabled = IsLooping,
                    NoiseEnabled = NoiseAmplitude > 0 && NoiseFrequency > 0,
                    NoiseFrequency = NoiseFrequency,
                    NoiseAmplitude = NoiseAmplitude,
                    Tremolo = Tremolo,
                    Vibrato = Vibrato,
                    Voice = voice,
                    SampleRate = SampleRate,
                    SoundFont = SoundFont!
                });
                source.Name = voice.InstrumentName;

                // create two channels from the mono instrument samples
                double[] stereoSamples = new double[instrumentSamples.Length * 2];
                for (int i = 0; i < instrumentSamples.Length; i++)
                {
                    stereoSamples[i * 2] = instrumentSamples[i];
                    stereoSamples[i * 2 + 1] = instrumentSamples[i];
                }

                // add the stereo samples to the preset samples and collect the new source
                presetSamples = AudioBuffer.Add(stereoSamples, presetSamples, 0);
                Sources = [.. Sources, source];
            }

            // apply the reverb effect to the preset samples and normalize 
            Reverb.Apply(presetSamples, reverbDelay, reverbDecay, SampleRate);
            float[] floatBuffer = PlayEngine.NormalizeBuffer(presetSamples);

            // build the preset instrument data table
            BuildInstrumentTable(PresetTable, Sources, floatBuffer);

            // build the sample data canvas
            BuildSampleCanvas(PresetSignal, floatBuffer, SampleRate);

            // play the preset sample
            PlaySample(floatBuffer, SampleRate);
        }

        // Simple UI helpers
        private static void BuildInstrumentTable(Border container, InstrumentSource[] sources, float[] samples)
        {
            int ENVELOPEWIDTH = 100;
            int ENVELOPEHEIGHT = 20;
            string[] rowNames = [
                "Name",
                "Start Pitch",
                "End Pitch",
                "Looping?",
                "Loop Start",
                "Loop End",
                "Root Key",
                "Start Cents",
                "End Cents",
                "Sample Rate",
                "Sample Count",
                "Attack?",
                "Length",
                "Envelope",
                ];
            var rows = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(0, 2, 0, 2) };
            container.Child = rows;
            foreach (var title in rowNames)
            {
                var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
                rows.Children.Add(panel);
                var label = new TextBlock { Text = title, Margin = new Thickness(0, 2, 0, 2), Width = 100 };
                panel.Children.Add(label);
                foreach (var source in sources)
                {
                    double maxTime = source.Envelope.Length > 0 ? source.Envelope[^1].Time : 1;
                    double xScale = ENVELOPEWIDTH / maxTime;
                    double yScale = ENVELOPEHEIGHT / 1;
                    string value = title switch
                    {
                        "Name" => source.Name,
                        "Start Pitch" => source.StartPitch.ToString(),
                        "End Pitch" => source.EndPitch.ToString(),
                        "Looping?" => source.LoopEnabled.ToString(),
                        "Loop Start" => source.LoopStart.ToString(),
                        "Loop End" => source.LoopEnd.ToString(),
                        "Root Key" => source.RootKey.ToString(),
                        "Start Cents" => source.StartCents.ToString(),
                        "End Cents" => source.EndCents.ToString(),
                        "Sample Rate" => source.SampleRate.ToString(),
                        "Sample Count" => source.SampleCount.ToString(),
                        "Attack?" => source.AttackEnabled.ToString(),
                        "Length" => ((double)source.Envelope[^1].Time).ToString("F3"),
                        "Envelope" => string.Join(", ", source.Envelope.Select(e => $"({e.Time}, {e.Gain})")),
                        _ => ""
                    };
                    if (title == "Envelope" && source.Envelope.Length > 0)
                    {
                        var envelopeCanvas = new Canvas { Width = ENVELOPEWIDTH, Height = ENVELOPEHEIGHT, Margin= new Thickness(0,0,10,0) };
                        PointCollection points = [];
                        for (int i = 0; i < source.Envelope.Length; i++)
                        {
                            var e = source.Envelope[i];
                            double x = e.Time * xScale;
                            double y = (1 - e.Gain) * yScale;
                            var point = new Point(x, y);
                            points.Add(point);
                        }
                        var polyline = new Polyline
                        {
                            Stroke = Brushes.Black,
                            StrokeThickness = 1,
                            Points = points,
                            Fill = Brushes.Black,
                        };
                        points.Add(new Point(source.Envelope[^1].Time * xScale, (1 - source.Envelope[0].Gain) * yScale));
                        points.Add(new Point(source.Envelope[0].Time * xScale, (1 - source.Envelope[0].Gain) * yScale));
                        envelopeCanvas.Children.Add(polyline);
                        panel.Children.Add(envelopeCanvas);
                    }
                    else
                    {
                        var valueLabel = new TextBlock { Text = value, Width = ENVELOPEWIDTH + 10 };
                        panel.Children.Add(valueLabel);
                    }
                }
            }
        }
        private static void BuildSampleCanvas(Border container, float[] samples, int sampleRate)
        {
            // Create a simple poly-line visualization
            int width = 800;
            int height = 120;
            var canvas = new Canvas { Width = width, Height = height };
            var poly = new Polyline { Stroke = System.Windows.Media.Brushes.Black, StrokeThickness = 1 };
            int step = (int)Math.Max(1.0, (float)samples.Length / (float)width);
            for (int i = 0; i < samples.Length; i += step)
            {
                double x = (double)i / samples.Length * width;
                double y = height / 2 - (samples[i] * (height / 2));
                poly.Points.Add(new Point(x, y));
            }
            canvas.Children.Add(poly);
            container.Child = canvas;
        }

        static WaveOut audioOutput = new WaveOut();
        private static void PlaySample(float[] samples, int sampleRate)
        {
            var provider = new AudioBufferProvider(samples, sampleRate);
            audioOutput.Init(provider);
            audioOutput.Play();
        }

        private void ViewPresets_Click(object sender, RoutedEventArgs e)
        {
            if (SoundFont == null) return;
            var presetViewDialog = new PresetViewDialog();
            presetViewDialog.SoundFontFileName = SoundFontFileName;
            presetViewDialog.WindowTitle = "Preset for SoundFont: " + SoundFontFileName;
            presetViewDialog.Show();
        }

    }
}
