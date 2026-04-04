using CMGWpf.Model.Generators;
using CMGWpf.MVVM;
using CMGWpf.PlayFunctions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
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
        #region Play Dialog Properties
        private ObservableCollection<Generator> playGenerators = [];
        public ObservableCollection<Generator> PlayGenerators
        {
            get => playGenerators;
            set { playGenerators = value; OnPropertyChanged(); }
        }
        private double scrollRollWidth = 0;
        public double ScrollRollWidth
        {
            get { return scrollRollWidth; }
            set { scrollRollWidth = value; OnPropertyChanged(); }
        }
        private double playDuration = 0;
        public double PlayDuration
        {
            get { return playDuration; }
            set { playDuration = value; }
        }
        private double currentPlayPosition = 0;
        private bool isUpdatingPosition = false; // Flag to prevent feedback loop with timer
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

                OnPropertyChanged();
                currentScrollPosition = -value / PlayDuration * ScrollRollWidth; // Update scroll position based on play position
                OnPropertyChanged(nameof(currentScrollPosition));
                OnPropertyChanged(nameof(CurrentPlayTime));
            }
        }
        private double currentScrollPosition = 0;
        public double CurrentScrollPosition
        {
            get { return currentScrollPosition; }
            set { currentScrollPosition = value; OnPropertyChanged(); }
        }
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

        public ObservableCollection<PresetColor> PresetColors
        {
            get { return presetColors; }
            set { presetColors = value; OnPropertyChanged(); }
        }

        private bool showVoices = false;
        public bool ShowVoices
        {
            get { return showVoices; }
            set { showVoices = value; OnPropertyChanged(); }
        }
        private VoiceDialog? voiceDialog;
        public VoiceDialog? VoiceDialog
        {
            get { return voiceDialog; }
            set { voiceDialog = value; OnPropertyChanged(); }
        }
        private List<SF_Preset> sF_Presets = [];
        public List<SF_Preset> SF_Presets
        {
            get { return sF_Presets; }
            set { sF_Presets = value; OnPropertyChanged(); }
        }
        private double[] signalLevels = [0, 0];
        public double[] SignalLevels
        {
            get { return signalLevels; }
            set { signalLevels = value; OnPropertyChanged(); OnPropertyChanged(nameof(SignalLevelPixels)); }
        }
        private double[] maxSignalLevels = [0, 0];
        public double[] MaxSignalLevels
        {
            get { return maxSignalLevels; }
            set { maxSignalLevels = value; OnPropertyChanged(); OnPropertyChanged(nameof(MaxSignalLevelPixels)); }
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
        /// <summary>
        /// Call this from timer to update position without triggering seek
        /// </summary>
        public void UpdatePositionFromTimer(double position)
        {
            isUpdatingPosition = true;
            currentPlayPosition = position;
            OnPropertyChanged(nameof(CurrentPlayPosition));
            OnPropertyChanged(nameof(CurrentPlayTime));
            currentScrollPosition = -position / PlayDuration * ScrollRollWidth; // Update scroll position based on play position
            OnPropertyChanged(nameof(currentScrollPosition));
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
        #endregion

    }
}
