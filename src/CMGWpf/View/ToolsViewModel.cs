using CMGWpf.Dialogs.Tools;
using CMGWpf.MVVM;
using CMGWpf.Types;
using System.Collections.ObjectModel;
using System.Windows;

namespace CMGWpf.View
{
    public class ToolsViewModel : ViewModelBase
    {
        private static ToolsViewModel? _instance;
        public static ToolsViewModel Instance => _instance ??= new ToolsViewModel();

        private ToolsViewModel()
        {
        }

        // property and relay command to assure that only a single dialog is activated
        // dialog close action will permit new dialog to be created.
        private Window? _thisDialog = null;
        public Window? ThisDialog {get=>_thisDialog; set { _thisDialog = value; OnPropertyChanged(); } }
        private RelayCommand<object?>? _generatorandCalculatorToolsCommand;
        public RelayCommand<object?> GeneratorandCalculatorToolsCommand =>
            _generatorandCalculatorToolsCommand ??= new RelayCommand<Object?>(execute =>
            {
                if (ThisDialog == null)
                {
                    ThisDialog = new GeneratorAndCalculatorTools();
                    ThisDialog.Show();
                }
            });

        private double _midi1 = 0;
        public double Midi1
        {
            get => _midi1;
            set
            {
                _midi1 = value;
                OnPropertyChanged();
                _frequency1 = Math.Round(ToolUtilities.MidiToFrequency(_midi1), 2);
                OnPropertyChanged(nameof(Frequency1));
            }
        }
        private double _frequency1 = 0;
        public double Frequency1 { get => _frequency1; }
        private double _midi2 = 0;
        public double Midi2 { get => _midi2; }
        private double _frequency2 = 0;
        public double Frequency2
        {
            get => _frequency2;
            set
            {
                _frequency2 = value;
                OnPropertyChanged();
                _midi2 = Math.Round(ToolUtilities.FrequencyToMidi(_frequency2), 2);
                OnPropertyChanged(nameof(Midi2));
            }
        }
        private double _beatCount = 0;
        public double BeatCount
        {
            get => _beatCount;
            set
            {
                _beatCount = value; OnPropertyChanged();
                _measureDuration = _measureBPM != 0 ? Math.Round(_beatCount * 60 / _measureBPM, 3) : 0;
                OnPropertyChanged(nameof(MeasureDuration));
            }
        }
        private double _measureBPM = 0;
        public double MeasureBPM
        {
            get => _measureBPM;
            set
            {
                _measureBPM = value; OnPropertyChanged();
                _measureDuration = _measureBPM != 0 ? Math.Round(_beatCount * 60 / _measureBPM, 3) : 0;
                OnPropertyChanged(nameof(MeasureDuration));
            }
        }
        private double _measureDuration = 0;
        public double MeasureDuration { get => _measureDuration; }
        private double _amplitude = 0;
        public double Amplitude
        {
            get => _amplitude;
            set
            {
                _amplitude = value; OnPropertyChanged();
                _oscillatorFrequency = (_BPM * _amplitude) != 0 ? Math.Round(60_000 / (_BPM * _amplitude)) : 0;
                OnPropertyChanged(nameof(OscillatorFrequency));
            }
        }
        private double _BPM = 0;
        public double BPM
        {
            get => _BPM;
            set
            {
                _BPM = value; OnPropertyChanged();
                _oscillatorFrequency = (_BPM * _amplitude) != 0 ? Math.Round(60_000 / (_BPM * _amplitude)) : 0;
                OnPropertyChanged(nameof(OscillatorFrequency));
            }
        }
        private double _oscillatorFrequency = 0;
        public double OscillatorFrequency { get => _oscillatorFrequency; }
        private ObservableCollection<Message> _messages = [];
        public ObservableCollection<Message> Messages
        {
            get => _messages;
            set { _messages = value; OnPropertyChanged(); }
        }
        public ObservableCollection<string> GeneratorList
        {
            get
            {
                ObservableCollection<string> list = [];
                foreach (var track in FileViewModel.Instance.File.Tracks)
                {
                    foreach (var generator in track.Generators)
                    {
                        list.Add($"{track.Name}:{generator.Name}");
                    }
                }
                return list;
            }
        }
        public class StaggerGeneratorsSelection
        {
            public bool MoveUp { get; set; }
            public bool MoveDown { get; set; }
            public string TrackName { get; set; } = "";
            public string GeneratorName { get; set; } = "";
            public bool IsSelected { get; set; } = false;
        }
        private ObservableCollection<StaggerGeneratorsSelection> staggerGeneratorList = [];
        public ObservableCollection<StaggerGeneratorsSelection> StaggerGeneratorList
        {
            get => staggerGeneratorList;
            set { staggerGeneratorList = value; OnPropertyChanged(); }
        }
        public void NotifyStaggerListChanged(ObservableCollection<StaggerGeneratorsSelection> newList)
        {
            StaggerGeneratorList = newList;
        }

        public bool StaggerSelectAll { get; set; } = false;

        // generator calculator properties
        public static ObservableCollection<string> MaintainAlignTimeOptions { get => ["Start Time", "Stop Time"]; }
        private string _maintainTimeOption = "Start Time";
        public string MaintainTimeOption { get => _maintainTimeOption; set { _maintainTimeOption = value; OnPropertyChanged(); } }
        private double _staggerAmount = 0;
        public double StaggerAmount { get => _staggerAmount; set { _staggerAmount = Math.Round(value, 2); OnPropertyChanged(); } }
        private string _primaryGeneratorName = "";
        public string PrimaryGeneratorName { get => _primaryGeneratorName; set { _primaryGeneratorName = value; OnPropertyChanged(); } }
        private ObservableCollection<string> _secondaryGeneratorNames = [];
        public ObservableCollection<string> SecondaryGeneratorNames { get => _secondaryGeneratorNames; set { _secondaryGeneratorNames = value; OnPropertyChanged(); } }
        private string _alignTimeOption = "Start Time";
        public string AlignTimeOption { get => _alignTimeOption; set { _alignTimeOption = value; OnPropertyChanged(); } }
        #region Tools Commands

        private RelayCommand<StaggerGeneratorsSelection>? _moveStaggerUp;
        public RelayCommand<StaggerGeneratorsSelection> MoveStaggerUp =>
            _moveStaggerUp ??= new RelayCommand<StaggerGeneratorsSelection>(selection => new ToolsCommands(this, FileViewModel.Instance.File).MoveStaggerUp(selection));
        private RelayCommand<StaggerGeneratorsSelection>? _moveStaggerDown;
        public RelayCommand<StaggerGeneratorsSelection> MoveStaggerDown =>
            _moveStaggerDown ??= new RelayCommand<StaggerGeneratorsSelection>(selection => new ToolsCommands(this, FileViewModel.Instance.File).MoveStaggerDown(selection));
        private RelayCommand<object>? _align;
        public RelayCommand<object> Align =>
            _align ??= new RelayCommand<object>(execute => new ToolsCommands(this, FileViewModel.Instance.File).Align());

        private RelayCommand<object>? _setEqual;
        public RelayCommand<object> SetEqual =>
            _setEqual ??= new RelayCommand<object>(execute => new ToolsCommands(this, FileViewModel.Instance.File).SetEqual());
        private RelayCommand<object>? _stagger;
        public RelayCommand<object> Stagger =>
            _stagger ??= new RelayCommand<object>(execute => new ToolsCommands(this, FileViewModel.Instance.File).Stagger());
        #endregion
    }
}
