using CMGWpf.Dialogs;
using CMGWpf.Properties;
using CMGWpf.Types;
using CMGWpf.Utilities;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using static CMGWpf.Model.Generators.StochasticTypes;

namespace CMGWpf.Services
{
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
                }
                return _instance;
            }
        }
        private ObservableCollection<Message> statusMessges = [];
        public ObservableCollection<Message> StatusMessages { get => statusMessges; set { statusMessges = value; OnPropertyChanged(); } }
        #region Preferences
        private string soundFontFileLocation = Settings.Default.CMGSoundFontLocation;
        public string SoundFontFileLocation { get { return soundFontFileLocation; }
            set {
                if (SoundFontFileLocation != value)
                {
                    ObservableCollection<string> list = SoundFontUtilities.List(soundFontFileLocation);
                    if (list.Count > 0)
                    {
                        soundFontFileLocation = value;
                        StatusMessages.Add(new Message { Text = $"{list.Count} read from {soundFontFileLocation}", Error = false });
                        soundFontFileNames = list;
                        OnPropertyChanged();
                        OnPropertyChanged(nameof(SoundFontFileNames));
                    } else {
                        _ = MessageBox.Show($"There Are No Soundfont Files at Location '{value}'", "Soundfont Load Error", MessageBoxButton.OK);
                    }
                }
            }
        }
        public readonly static ObservableCollection<string> RecordFormats = ["mp3", "wav"];
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
        public readonly static ObservableCollection<string> TimeLineModes = ["Time", "Measure"];
        private string timeLineMode = Settings.Default.CMGTimeLineMode;
        public string TimeLineMode
        {
            get => timeLineMode;
            set
            {
                if (TimeLineModes.Contains(value))
                {
                    timeLineMode = value;
                    OnPropertyChanged();
                }
            }
        }
        private double measureLength = Double.Parse(Settings.Default.CMGMeasureLength);
        public double MeasureLength
        {
            get => measureLength;
            set
            {
                if (value <= 0)
                {
                    _ = MessageBox.Show("Measure Length Must Be Positive", "Measure Length Error", MessageBoxButton.OK);
                }
                else
                {
                    measureLength = value;
                    OnPropertyChanged();
                }
            }
        }
        private int beatsPerMeasure = int.Parse(Settings.Default.CMGBeatsPerMeasure);
        public int BeatsPerMeasure
        {
            get => beatsPerMeasure;
            set
            {
                if (value <= 0)
                {
                    _ = MessageBox.Show("Beats Per Measure Must Be Positive", "Beats Per Message Error", MessageBoxButton.OK);
                }
                else
                {
                    beatsPerMeasure = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool isSnap = Settings.Default.CMGBeatsPerMeasure == "true";
        public bool IsSnap
        {
            get => isSnap;
            set
            {
                isSnap = value;
                OnPropertyChanged();
            }
        }
        private int snapIncrement = int.Parse(Settings.Default.CMGSnapIncrement);
        public int SnapIncrement
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
        public readonly string SnapIncrementUnits = Settings.Default.CMGTimeLineMode == "time" ? "(sec)" : "(beats)";
        private RelayCommand<object>? _editPreferencesCommand;
        public RelayCommand<object> EditPreferencesCommand =>
            _editPreferencesCommand ??= new RelayCommand<object>(execute =>
            {
                Debug.WriteLine("EditPreferences command being executed.");

                // display the preferences dialog
                PreferencesDialog dialog = new PreferencesDialog
                {
                    DataContext = this,
                    Owner = Application.Current.MainWindow
                };
                dialog.ShowDialog();
            });
        // user has completed preference edits. All fields have been validated. Update the settings with the new preferences and close the dialog
        private RelayCommand<Window>? _editPreferencesOkCommand;
        public RelayCommand<Window> EditPreferencesOkCommand =>
            _editPreferencesOkCommand ??= new RelayCommand<Window>(window =>
            {
                Debug.WriteLine("EditPreferencesOK command being executed.");
                // move all of the preferences properties to the settings
                Settings.Default.CMGBeatsPerMeasure = BeatsPerMeasure.ToString();
                Settings.Default.CMGIsSnap = IsSnap ? "true" : "false";
                Settings.Default.CMGMeasureLength = MeasureLength.ToString();
                Settings.Default.CMGRecordFormat = RecordFormat;
                Settings.Default.CMGSnapIncrement = SnapIncrement.ToString();
                Settings.Default.CMGSoundFontLocation = SoundFontFileLocation;
                Settings.Default.CMGTimeLineMode = TimeLineMode;
                window.Close();
            });
        // user has canceled preference edits. Close the dialog
        private RelayCommand<Window>? _editPreferencesCancelCommand;
        public RelayCommand<Window> EditPreferencesCancelCommand =>
            _editPreferencesCancelCommand ??= new RelayCommand<Window>(window =>
            {
                window.Close();
            });
        #endregion
        private async void LoadEnsembleNamesAsync()
        {
            try
            {
                ObservableCollection<Ensemble> ensembleList = await EnsembleUtilities.GetEnsembleListAsync();
                ObservableCollection<string> names = [.. ensembleList.Select(x => x.Name).ToArray()];
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
                NoteSequenceNames = await NoteSequenceUtilities.GetNoteSequenceNamesAsync();
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
        public Window? ActiveDialog { get; set; }

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
        public ObservableCollection<string> NoteSequenceNames { get => noteSequenceNames; set { noteSequenceNames = value; OnPropertyChanged(); } }

        //public string DbServer { get; set; } = "http://blane-latitude-7290";
        public readonly string DbServer = "http://localhost";
        //public string DbServer { get; set; } = "http://192.168.1.182"; // IPv4 address
        //public string DbServer { get; set; } = "http://10.17.1.23"; // Current network IP
        public readonly string DbPort = "8081";

    }
}
