using CMGWpf.Dialogs;
using CMGWpf.Helpers;
using CMGWpf.Properties;
using CMGWpf.Types;
using CMGWpf.Utilities;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Windows;

namespace CMGWpf.Services
{
    /// <summary>
    /// Singleton service that manages global application state, including user preferences, status messages, and lists of ensembles and note sequences. Provides commands for editing preferences and loading data on startup.
    /// </summary>
    public class GlobalService : ServiceBase
    {
        private static GlobalService? _instance;
        public static GlobalService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GlobalService();
                    _instance.LoadEnsembleNamesAsync();
                    _instance.LoadNoteSequenceNamesAsync();
                    _instance.SoundFontFileLocation = Settings.Default.CMGSoundFontLocation;
                }
                return _instance;
            }
        }
        private ObservableCollection<Message> statusMessages = new ObservableCollection<Message>();
        public ObservableCollection<Message> StatusMessages { get => statusMessages; set { statusMessages = value; OnPropertyChanged(); } }
        #region Preferences
        private string soundFontFileLocation = Settings.Default.CMGSoundFontLocation;
        public string SoundFontFileLocation
        {
            get { return soundFontFileLocation; }
            set
            {
                ObservableCollection<string> list = SoundFontUtilities.List(soundFontFileLocation);
                if (list.Count > 0)
                {
                    soundFontFileLocation = value;
                    StatusMessages.Add(new Message { Text = $"{list.Count} read from {soundFontFileLocation}", Error = false });
                    soundFontFileNames = list;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SoundFontFileNames));
                }
                else
                {
                    _ = MessageBox.Show($"There Are No Soundfont Files at Location '{value}'", "Soundfont Load Error", MessageBoxButton.OK);
                }
            }
        }
        public ObservableCollection<string> RecordFormats { get; } = ["mp3", "wav"];
        private string recordFormat = Settings.Default.CMGRecordFormat;
        public string RecordFormat
        {
            get => recordFormat;
            set
            {
                if (RecordFormats.Contains(value))
                {
                    recordFormat = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool isSnap = Settings.Default.CMGIsSnap == "true";
        public bool IsSnap
        {
            get => isSnap;
            set
            {
                isSnap = value;
                OnPropertyChanged();
            }
        }
        private double snapIncrement = double.Parse(Settings.Default.CMGSnapIncrement);
        public double SnapIncrement
        {
            get => snapIncrement;
            set
            {
                if (value <= 0)
                {
                    _ = MessageBox.Show("Snap Increment Must Be Positive", "Snap Increment Error", MessageBoxButton.OK);
                }
                else
                {
                    snapIncrement = value;
                    OnPropertyChanged();
                }
            }
        }
        private RelayCommand<object>? _editPreferencesCommand;
        public RelayCommand<object> EditPreferencesCommand =>
            _editPreferencesCommand ??= new RelayCommand<object>(execute =>
            {
                DebugLog.Write("EditPreferences command being executed.");

                // display the preferences dialog
                PreferencesDialog activeDialog = new()
                {
                    DataContext = this,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };
                StatusMessages.Clear();
                activeDialog.ShowDialog();
            });
        // user has completed preference edits. All fields have been validated. Update the settings with the new preferences and close the dialog
        private RelayCommand<Window>? _editPreferencesOkCommand;
        public RelayCommand<Window> EditPreferencesOkCommand =>
            _editPreferencesOkCommand ??= new RelayCommand<Window>(window =>
            {
                DebugLog.Write("EditPreferencesOK command being executed.");
                // move all of the preferences properties to the settings
                Settings.Default.CMGIsSnap = IsSnap ? "true" : "false";
                Settings.Default.CMGRecordFormat = RecordFormat;
                Settings.Default.CMGSnapIncrement = SnapIncrement.ToString();
                Settings.Default.CMGSoundFontLocation = SoundFontFileLocation;
                StatusMessages = [new Message() { Text = "Preferences Updated", Error = false }];
                window.Close();
            });
        #endregion
        public async void LoadEnsembleNamesAsync()
        {
            try
            {
                var ensembleList = await EnsembleHelpers.List();
                ObservableCollection<string> names = new ObservableCollection<string>(ensembleList.Select(x => x.Name).OrderBy(name => name));
                EnsembleNames = names;
                StatusMessages.Add(new Message { Text = $"{names.Count} Ensembles loaded.", Error = names.Count == 0 });
            }
            catch (Exception ex)
            {
                StatusMessages = [new Message { Text = $"Error loading ensemble names: {ex.Message}", Error = true }];
            }
        }
        private async void LoadNoteSequenceNamesAsync()
        {
            try
            {
                var sequenceNames = await NoteSequenceHelpers.List();
                NoteSequenceNames = new ObservableCollection<string>(sequenceNames.Select(x => x.Name).OrderBy(name => name));
                StatusMessages.Add(new Message { Text = $"{NoteSequenceNames.Count} Note Sequences loaded.", Error = NoteSequenceNames.Count == 0 });
            }
            catch (Exception ex)
            {
                StatusMessages = [new Message { Text = $"Error loading note sequence names: {ex.Message}", Error = true }];
            }
        }

        private bool _isDirty = false;
        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (value != _isDirty)
                {
                    _isDirty = value;
                    OnPropertyChanged();
                    OnPropertyChanged(WindowTitle);
                }
            }
        }

        private string _fileName = string.Empty;
        public string FileName
        {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(WindowTitle);
                }
            }
        }
        public string WindowTitle
        {
            get { return $"CMG {Settings.Default.Version} - ({FileName}){(IsDirty ? "*" : "")}"; }
        }
        public string PlayTitle
        {
            get { return $"CMG {Settings.Default.Version} Play/Sound Roll - ({FileName}){(IsDirty ? "*" : "")}"; }
        }

        private ObservableCollection<string> soundFontFileNames = SoundFontUtilities.List(Settings.Default.CMGSoundFontLocation);
        public ObservableCollection<string> SoundFontFileNames
        {
            get { return soundFontFileNames; }
            set { soundFontFileNames = value; OnPropertyChanged(); }
        }
        private ObservableCollection<string> ensembleNames = [];
        public ObservableCollection<string> EnsembleNames
        {
            get => ensembleNames;
            set { ensembleNames = value; OnPropertyChanged(); }
        }
        private ObservableCollection<string> noteSequenceNames = [];
        public ObservableCollection<string> NoteSequenceNames { 
            get => noteSequenceNames; 
            set { noteSequenceNames = value; OnPropertyChanged(); } }
    }
}
