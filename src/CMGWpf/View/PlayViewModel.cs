using CMGWpf.Model.Generators;
using CMGWpf.MVVM;
using CMGWpf.PlayFunctions;
using CMGWpf.PlayFunctions.Utilities;
using CMGWpf.Services;
using CMGWpf.Types;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using static CMGWpf.Types.PlayTypes;

namespace CMGWpf.View
{
    public class PlayViewModel : ViewModelBase
    {
        private static PlayViewModel? _instance;
        public static PlayViewModel Instance => _instance ??= new PlayViewModel();

        private PlayViewModel()
        {
        }
        #region Play/Record data
        private double playDuration = 0;
        public double PlayDuration { get { return playDuration; } set { playDuration = value; } }
        private ObservableCollection<Generator> playGenerators = [];
        public ObservableCollection<Generator> PlayGenerators { get => playGenerators; set { playGenerators = value; OnPropertyChanged(); } }
        private AudioBufferWrapper finalSignal = new AudioBufferWrapper([]);
        public AudioBufferWrapper FinalSignal { get { return finalSignal; } set { finalSignal = value; OnPropertyChanged(); } }

        // Use concurrent collections for lock-free parallel access during generator processing
        private ConcurrentBag<TimeMidiPreset> timeMidiPresets = [];
        public ConcurrentBag<TimeMidiPreset> TimeMidiPresets { get { return timeMidiPresets; } set { timeMidiPresets = value; OnPropertyChanged(); } }
        private ConcurrentBag<InstrumentSource> instrumentSources = [];
        public ConcurrentBag<InstrumentSource> InstrumentSources { get { return instrumentSources; } set { instrumentSources = value; OnPropertyChanged(); } }
        private ConcurrentBag<SF_Preset> sF_Presets = [];
        public ConcurrentBag<SF_Preset> SF_Presets { get { return sF_Presets; } set { sF_Presets = value; OnPropertyChanged(); } }
        #endregion
        private string progressMessage = string.Empty;
        public string ProgressMessage { get { return progressMessage; } set { progressMessage = value; OnPropertyChanged(); } }
        private bool userCancelled = false;
        public bool UserCancelled { get { return userCancelled; } set { userCancelled = value; OnPropertyChanged(); } }
        private int completedGenerators = 0;
        public int CompletedGenerators { get { return completedGenerators; } set { completedGenerators = value; OnPropertyChanged(); } }
        private int totalGenerators = 0;
        public int TotalGenerators { get { return totalGenerators; } set { totalGenerators = value; OnPropertyChanged(); } }

        #region Play Dialog Properties
        public string PlayTitle { get => GlobalService.Instance.PlayTitle; }
        private double scrollRollWidth = 0;
        public double SoundRollWidth { get { return scrollRollWidth; } set { scrollRollWidth = value; OnPropertyChanged(); } }
        private double currentPlayPosition = 0;
        private bool isUpdatingPosition = false;
        public double CurrentPlayPosition
        {
            get { return currentPlayPosition; }
            set
            {
                // Ignore tiny changes to reduce jitter
                if (Math.Abs(currentPlayPosition - value) < 0.01) return;

                currentPlayPosition = value;

                // Only seek audio if user manually changed slider (not from timer update)
                if (!isUpdatingPosition && AudioProvider is AudioBufferProvider provider)
                {
                    provider.SetPosition(TimeSpan.FromSeconds(value));
                }

                currentScrollPosition = -value / PlayDuration * SoundRollWidth; // Update scroll position based on play position
                OnPropertyChanged(nameof(CurrentScrollPosition));
                OnPropertyChanged(nameof(CurrentPlayTime));
                OnPropertyChanged();
            }
        }
        private double currentScrollPosition = 0;
        public double CurrentScrollPosition { get { return currentScrollPosition; } set { currentScrollPosition = value; OnPropertyChanged(); } }
        private double audioVolume = 0; // dB scale: -10 to +10
        public double AudioVolume
        {
            get { return audioVolume; }
            set
            {
                audioVolume = value;
                OnPropertyChanged();

                // Convert dB to linear scale and apply to audio output
                if (AudioOutput != null)
                {
                    // Formula: linear = 10^(dB/20)
                    // dB range: -10 to +10 maps to linear range: ~0.316 to ~3.162
                    float linearVolume = (float)Math.Pow(10, (audioVolume - 10) / 20.0);

                    // Clamp to NAudio's valid range (0.0 to 1.0 for normal volume, can go higher)
                    // Note: Values > 1.0 will amplify but may cause distortion
                    linearVolume = Math.Max(0.0f, Math.Min(linearVolume, 1.0f));

                    AudioOutput.Volume = linearVolume;
                    Debug.WriteLine($"Volume changed: {audioVolume:F1} dB -> {linearVolume:F3} linear");
                }
            }
        }
        private ObservableCollection<PresetColor> presetColors = [];
        public ObservableCollection<PresetColor> PresetColors { get { return presetColors; } set { presetColors = value; OnPropertyChanged(); } }
        private bool showVoices = false;
        public bool ShowVoices { get { return showVoices; } set { showVoices = value; OnPropertyChanged(); } }
        private VoiceDialog? voiceDialog;
        public VoiceDialog? VoiceDialog { get { return voiceDialog; } set { voiceDialog = value; OnPropertyChanged(); } }

        private double[] signalLevels = [0, 0];
        public double[] SignalLevels
        {
            get { return signalLevels; }
            set { if (value != signalLevels) { signalLevels = value; OnPropertyChanged(); DrawSignalDials(signalLevels, maxSignalLevels); OnPropertyChanged(nameof(SignalLevelPixels)); OnPropertyChanged(nameof(LeftDialCanvas)); OnPropertyChanged(nameof(RightDialCanvas)); } }
        }
        // given a signal level, determine the point on a dial where that signal level is
        // The dial is an elliptical shape of width and height. The signal level varies from 0 to 10
        // the return value is in canvas units of the dial
        private Point SignalToDial(double signalLevel, double width, double height)
        {
            // get the length of the line from the lower middle of the dial to the point where 
            // the signal line intersects the dialog ellipse

            double theta = Math.Clamp(signalLevel, 0, 1) * Math.PI;
            //Debug.WriteLine($"SignalDial signalLevel={signalLevel}, theta {theta}");
            double cTheta = Math.Cos(theta);
            double sTheta = Math.Sin(theta);
            double lTheta = Math.Sqrt(width * cTheta * width * cTheta / 4 + height * sTheta * height * sTheta);
            return new Point(width / 2 - lTheta * cTheta, height - lTheta * sTheta);
        }
        private Canvas? _leftDialCanvas;
        public Canvas? LeftDialCanvas { get => _leftDialCanvas; set { _leftDialCanvas = value; OnPropertyChanged(); } }
        private Canvas? _rightDialCanvas;
        public Canvas? RightDialCanvas { get => _rightDialCanvas; set { _rightDialCanvas = value; OnPropertyChanged(); } }
        public void RegisterSignalCanvases(Canvas left, Canvas right)
        {
            _leftDialCanvas = left;
            _rightDialCanvas = right;
        }

        public void UnregisterSignalCanvases()
        {
            _leftDialCanvas = null;
            _rightDialCanvas = null;
        }
        private void DrawSignalDials(double[] levels, double[] maxLevels)
        {
            if (_leftDialCanvas == null || _rightDialCanvas == null) return;

            // Check if Application.Current is available and dispatcher is accessible
            if (Application.Current == null) return;

            // Use Dispatcher to ensure UI updates happen on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Double-check canvases are still valid after dispatcher invoke
                if (_leftDialCanvas == null || _rightDialCanvas == null) return;

                Canvas leftDialCanvas = _leftDialCanvas;
                Canvas rightDialCanvas = _rightDialCanvas;

                // Check if canvases are still part of the visual tree
                if (!leftDialCanvas.IsLoaded || !rightDialCanvas.IsLoaded) return;

                leftDialCanvas.Children.Clear();
                rightDialCanvas.Children.Clear();

                double dialWidth = leftDialCanvas.Width;
                double dialHeight = leftDialCanvas.Height;
                leftDialCanvas.ClipToBounds = true;
                rightDialCanvas.ClipToBounds = true;

                Ellipse leftDial = new Ellipse()
                {
                    Width = dialWidth,
                    Height = dialHeight * 2,
                    Fill = System.Windows.Media.Brushes.White,
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = 1,
                };
                Canvas.SetLeft(leftDial, 0);
                Canvas.SetTop(leftDial, 0);
                leftDialCanvas.Children.Add(leftDial);
                Ellipse rightDial = new Ellipse()
                {
                    Width = dialWidth,
                    Height = dialHeight * 2,
                    Fill = System.Windows.Media.Brushes.White,
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = 1,
                };
                Canvas.SetLeft(rightDial, 0);
                Canvas.SetTop(rightDial, 0);
                rightDialCanvas.Children.Add(rightDial);
                var leftSignalPoint = SignalToDial(levels[0], dialWidth, dialHeight);
                var leftMaxPoint = SignalToDial(maxLevels[0], dialWidth, dialHeight);
                var rightSignalPoint = SignalToDial(levels[1], dialWidth, dialHeight);
                var rightMaxPoint = SignalToDial(maxLevels[1], dialWidth, dialHeight);
                Line leftSignalLine = new Line()
                {
                    X1 = dialWidth / 2,
                    Y1 = dialHeight,
                    X2 = leftSignalPoint.X,
                    Y2 = leftSignalPoint.Y,
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = 1,
                };
                Line leftMaxLine = new Line()
                {
                    X1 = dialWidth / 2,
                    Y1 = dialHeight,
                    X2 = leftMaxPoint.X,
                    Y2 = leftMaxPoint.Y,
                    Stroke = System.Windows.Media.Brushes.Red,
                    StrokeThickness = 1,
                };
                Canvas.SetLeft(leftSignalLine, 0);
                Canvas.SetTop(leftSignalLine, 0);
                Canvas.SetLeft(leftMaxLine, 0);
                Canvas.SetTop(leftMaxLine, 0);
                Line rightSignalLine = new Line()
                {
                    X1 = dialWidth / 2,
                    Y1 = dialHeight,
                    X2 = rightSignalPoint.X,
                    Y2 = rightSignalPoint.Y,
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = 1,
                };
                Line rightMaxLine = new Line()
                {
                    X1 = dialWidth / 2,
                    Y1 = dialHeight,
                    X2 = rightMaxPoint.X,
                    Y2 = rightMaxPoint.Y,
                    Stroke = System.Windows.Media.Brushes.Red,
                    StrokeThickness = 1,
                };

                Canvas.SetLeft(rightSignalLine, 0);
                Canvas.SetTop(rightSignalLine, 0);
                Canvas.SetLeft(rightMaxLine, 0);
                Canvas.SetTop(rightMaxLine, 0);
                leftDialCanvas.Children.Add(leftSignalLine);
                leftDialCanvas.Children.Add(leftMaxLine);
                rightDialCanvas.Children.Add(rightSignalLine);
                rightDialCanvas.Children.Add(rightMaxLine);

                // Force visual update
                leftDialCanvas.InvalidateVisual();
                rightDialCanvas.InvalidateVisual();
            });
        }

        private double[] maxSignalLevels = [0, 0];
        public double[] MaxSignalLevels
        {
            get { return maxSignalLevels; }
            set { maxSignalLevels = value; OnPropertyChanged(); DrawSignalDials(signalLevels, maxSignalLevels); OnPropertyChanged(nameof(MaxSignalLevelPixels)); }
        }
        public double[] SignalLevelPixels
        {
            get
            {
                // Convert max signal level to pixel height for visualization
                // Assuming max signal level of 1.0 corresponds to 100 pixels height
                return [SignalLevels[0] * 75, SignalLevels[1] * 75]; // Use left channel for visualization
            }
        }
        public double[] MaxSignalLevelPixels
        {
            get
            {
                // Convert max signal level to pixel height for visualization
                // Assuming max signal level of 1.0 corresponds to 100 pixels height
                return [MaxSignalLevels[0] * 75, MaxSignalLevels[1] * 75]; // Use left channel for visualization
            }
        }
        private ObservableCollection<Message> messages = [];
        public ObservableCollection<Message> Messages { get => messages; set { messages = value; OnPropertyChanged(); } }
        /// <summary>
        /// Call this from timer to update position without triggering seek
        /// </summary>
        public void UpdatePositionFromTimer(double position)
        {
            isUpdatingPosition = true;
            currentPlayPosition = position;
            OnPropertyChanged(nameof(CurrentPlayPosition));
            OnPropertyChanged(nameof(CurrentPlayTime));
            currentScrollPosition = -position / PlayDuration * SoundRollWidth; // Update scroll position based on play position
            OnPropertyChanged(nameof(CurrentScrollPosition));
            isUpdatingPosition = false;
        }

        public string CurrentPlayTime
        {
            get
            {
                var time = TimeSpan.FromSeconds(CurrentPlayPosition);
                return $"{(int)time.TotalMinutes:00}:{time.Seconds:00}.{time.Milliseconds / 100}";
            }
        }
        private bool isPlaying = false;
        public bool IsPlaying
        {
            get { return isPlaying; }
            set { isPlaying = value; OnPropertyChanged(); }
        }
        private float[] audioBuffer = [];
        public float[] AudioBuffer
        {
            get { return audioBuffer; }
            set { audioBuffer = value; OnPropertyChanged(); }
        }

        private NAudio.Wave.WaveOut? audioOutput;
        public NAudio.Wave.WaveOut? AudioOutput
        {
            get { return audioOutput; }
            set { audioOutput = value; OnPropertyChanged(); }
        }

        private AudioBufferProvider? audioProvider;
        public AudioBufferProvider? AudioProvider
        {
            get { return audioProvider; }
            set { audioProvider = value; OnPropertyChanged(); }
        }
        #endregion
        #region Play Dialog Commands
        private RelayCommand<object>? _rewindCommand;
        public RelayCommand<object> RewindCommand =>
            _rewindCommand ??= new RelayCommand<object>(execute => new PlayCommands(this).Rewind());
        private RelayCommand<object>? _playPauseCommand;
        public RelayCommand<object> PlayPauseCommand =>
            _playPauseCommand ??= new RelayCommand<object>(execute => new PlayCommands(this).PlayPause());
        private RelayCommand<PlayDialog>? _showVoicesCommand;
        public RelayCommand<PlayDialog> ShowVoicesCommand =>
            _showVoicesCommand ??= new RelayCommand<PlayDialog>(dialog => new PlayCommands(this).ShowVoicesToggle(dialog));
        private RelayCommand<Window>? _recordAudioCommand;
        public RelayCommand<Window> RecordAudioCommand =>
            _recordAudioCommand ??= new RelayCommand<Window>(window => new PlayCommands(this).RecordAudio(window), canExecute => !IsPlaying);
        private RelayCommand<Window>? _recordVideoCommand;
        public RelayCommand<Window> RecordVideoCommand =>
            _recordVideoCommand ??= new RelayCommand<Window>(window => new PlayCommands(this).RecordVideo(window), canExecute => !IsPlaying);
        #endregion

    }
}
