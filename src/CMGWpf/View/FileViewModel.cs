
using CMGWpf.Model;
using CMGWpf.Model.Generators;
using CMGWpf.MVVM;
using CMGWpf.PlayFunctions;
using CMGWpf.Properties;
using CMGWpf.Services;
using CMGWpf.Types;
using CMGWpf.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using static CMGWpf.Types.PlayTypes;
using Track = CMGWpf.Model.Track;

namespace CMGWpf.View
{
    public class FileViewModel : ViewModelBase
    {
        private static FileViewModel? _instance;
        public static FileViewModel Instance => _instance ??= new FileViewModel();

        private FileViewModel()
        {
            string recentFilesString = Settings.Default.CMGRecentFiles;
            if (!string.IsNullOrEmpty(recentFilesString))
            {
                string[] recentFilesArray = recentFilesString.Split('|');
                RecentFiles = new ObservableCollection<string>(recentFilesArray);
            }
            string soundFontFileLocation = Settings.Default.CMGSoundFontLocation;
            if (!string.IsNullOrEmpty(soundFontFileLocation))
            {
                SoundFontFileNames = SoundFontUtilities.List(soundFontFileLocation);
                Debug.WriteLine($"{SoundFontFileNames.Count} read from {soundFontFileLocation}");
                StatusMessages.Add(new Message { Text = $"{SoundFontFileNames.Count} read from {soundFontFileLocation}", Error = false });
            }
            //TODO load other settings

            GlobalService.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(GlobalService.Instance.FileName) ||
                    e.PropertyName == nameof(GlobalService.Instance.IsDirty))
                {
                    OnPropertyChanged(nameof(WindowTitle));
                }
            };
        }

        public ObservableCollection<Message> StatusMessages
        {
            get => GlobalService.Instance.StatusMessages;
            set { GlobalService.Instance.StatusMessages = value; OnPropertyChanged(); }
        }
        public System.Windows.Window? ActiveDialog
        {
            get => GlobalService.Instance.ActiveDialog;
            set { GlobalService.Instance.ActiveDialog = value; OnPropertyChanged(); }
        }
        public bool IsDirty
        {
            get => GlobalService.Instance.IsDirty;
            set
            {
                GlobalService.Instance.IsDirty = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowTitle));
            }
        }
        private ObservableCollection<string> recentFiles = [];
        public ObservableCollection<string> RecentFiles
        {
            get { return recentFiles; }
            set { recentFiles = value; OnPropertyChanged(); }
        }
        public void AddRecentFile(string filePath)
        {
            recentFiles.Remove(filePath);
            RecentFiles.Insert(0, filePath);
            // limit the recent files list to 10 items
            while (recentFiles.Count > 10) recentFiles.RemoveAt(10);
            Settings.Default.CMGRecentFiles = String.Join("|", [.. RecentFiles]);
            Settings.Default.Save();
        }
        public void NotifyTracksChanged(List<Track> newTracks)
        {
            File.Tracks = newTracks;
            OnPropertyChanged(nameof(File));
        }

        public string FileName
        {
            get => GlobalService.Instance.FileName;
            set
            {
                GlobalService.Instance.FileName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private CMGFile file = new();
        public CMGFile File
        {
            get { return file; }
            set { file = value; OnPropertyChanged(); }
        }
        private string newComment = string.Empty;
        public string NewComment
        {
            get { return newComment; }
            set { newComment = value; }
        }
        public string WindowTitle
        {
            get => GlobalService.Instance.WindowTitle;
        }
        public ObservableCollection<string> SoundFontFileNames
        {
            get => GlobalService.Instance.SoundFontFileNames;
            set { GlobalService.Instance.SoundFontFileNames = value; OnPropertyChanged(); }
        }
        private ObservableCollection<Message> messages = [];
        public ObservableCollection<Message> Messages
        {
            get { return messages; }
            set { messages = value; OnPropertyChanged(); }
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
        //private double scrollRollHeight = 0;
        //public double ScrollRollHeight
        //{
        //    get { return scrollRollHeight; }
        //    set { scrollRollHeight = value; OnPropertyChanged(); }
        //}
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

        #region File Menu Commands

        private RelayCommand<object>? _notImplementedCommand;
        public RelayCommand<object> NotImplementedCommand =>
            _notImplementedCommand ??= new RelayCommand<object>(execute => StatusMessages = new ObservableCollection<Message> { new Message { Text = "Command not implemented", Error = true } });
        private RelayCommand<object>? _fileNewCommand;
        public RelayCommand<object> FileNewCommand =>
            _fileNewCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).New());

        private RelayCommand<object>? _fileOpenCommand;
        public RelayCommand<object> FileOpenCommand =>
            _fileOpenCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).Open());

        private RelayCommand<object>? _fileSaveCommand;
        public RelayCommand<object> FileSaveCommand =>
            _fileSaveCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).Save(),
            canExecute => { return IsDirty; });

        private RelayCommand<object>? _fileSaveAsCommand;
        public RelayCommand<object> FileSaveAsCommand =>
            _fileSaveAsCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).SaveAs());

        private RelayCommand<string>? _fileOpenRecentCommand;
        public RelayCommand<string> FileOpenRecentCommand =>
            _fileOpenRecentCommand ??= new RelayCommand<string>(filePath => new FileCommands(this, File).OpenRecent(filePath));

        private RelayCommand<object>? _exitFileCommand;
        public RelayCommand<object> ExitFileCommand =>
            _exitFileCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).Exit());

        #endregion
        #region Edit Menu Commands
        private RelayCommand<object>? _editCommentCommand;
        public RelayCommand<object> EditCommentCommand =>
            _editCommentCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).EditComment());

        private RelayCommand<object>? _editCommentOkCommand;
        public RelayCommand<object> EditCommentOkCommand =>
            _editCommentOkCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).EditCommentOk());

        private RelayCommand<object>? _editCommentCancelCommand;
        public RelayCommand<object> EditCommentCancelCommand =>
            _editCommentCancelCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).EditCommentCancel());
        private RelayCommand<object>? _editPreferencesCommand;
        public RelayCommand<object> EditPreferencesCommand =>
            _editPreferencesCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).EditPreferences());

        private RelayCommand<object>? _addTrackCommand;
        public RelayCommand<object> AddTrackCommand =>
            _addTrackCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).AddTrack());
        private RelayCommand<object>? _playCommand;
        public RelayCommand<object> PlayCommand =>
            _playCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).Play());
        #endregion
        #region Play Dialog Commands
        private RelayCommand<object>? _rewindCommand;
        public RelayCommand<object> RewindCommand =>
            _rewindCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).Rewind());
        private RelayCommand<object>? _playPauseCommand;
        public RelayCommand<object> PlayPauseCommand =>
            _playPauseCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).PlayPause());
        private RelayCommand<object>? _showVoicesCommand;
        public RelayCommand<object> ShowVoicesCommand =>
            _showVoicesCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).ShowVoices());
        #endregion
    }
}
