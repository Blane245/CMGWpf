using CMGWpf.MVVM;
using CMGWpf.Types;
using System.Collections.ObjectModel;

namespace CMGWpf.View
{
    public class ToolsViewModel : ViewModelBase
    {
        private static ToolsViewModel? _instance;
        public static ToolsViewModel Instance => _instance ??= new ToolsViewModel();

        private ToolsViewModel()
        {
        }
        private double _midi = 0;
        public double Midi
        {
            get => _midi;
            set { 
                _midi = value;
                _frequency = Math.Round(ToolUtilities.MidiToFrequency(_midi),2);
                OnPropertyChanged(nameof(Frequency));
            }
        }
        private double _frequency = 0;
        public double Frequency
        {
            get => _frequency;
            set { 
                _frequency = value;
                _midi = Math.Round(ToolUtilities.FrequencyToMidi(_frequency),2);
                OnPropertyChanged(nameof(Midi));
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
                _oscillatorFrequency = (_BPM * _amplitude) != 0 ? Math.Round(60_000 / (_BPM / _amplitude)) : 0;
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
                _oscillatorFrequency = (_BPM * _amplitude) != 0 ? Math.Round(60_000 / (_BPM / _amplitude)) : 0;
                OnPropertyChanged(nameof(OscillatorFrequency));
            }
        }
        private double _oscillatorFrequency = 0;
        public double OscillatorFrequency { get => _oscillatorFrequency; }
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
        public static ObservableCollection<string> MaintainAlignTimeOptions = ["Start Time", "Stop Time"];
        private string _maintainTimeOption = "Start Time";
        public string MaintainTimeOption { get => _maintainTimeOption; set { _maintainTimeOption = value; OnPropertyChanged(); } }
        private double _staggerAmount = 0;
        public double StaggerAmount { get => _staggerAmount; set { _staggerAmount = Math.Round(value,2); OnPropertyChanged(); } }
        private string _primaryGeneratorName = "";
        public string PrimaryGeneratorName { get => _primaryGeneratorName; set { _primaryGeneratorName = value; OnPropertyChanged(); } }
        private ObservableCollection<string> _secondaryGeneratorNames = [];
        public ObservableCollection<string> SecondaryGeneratorNames { get => _secondaryGeneratorNames; set { _secondaryGeneratorNames = value; OnPropertyChanged(); } }
        private string _alignTimeOption = "Start Time";
        public string AlignTimeOption { get => _alignTimeOption; set { _alignTimeOption = value; OnPropertyChanged(); } }
        #region Tools Commands
        private RelayCommand<object>? _midiFrequencyConverterCommand;
        public RelayCommand<object> MidiFrequencyConverterCommand =>
            _midiFrequencyConverterCommand ??= new RelayCommand<object>(execute => new ToolsCommands(this, FileViewModel.Instance.File).MidiFrequencyConverter());
        private RelayCommand<object>? _startCMGDBEditorCommand;
        public RelayCommand<object> StartCMGDBEditorCommand =>
            _startCMGDBEditorCommand ??= new RelayCommand<object>(execute => new ToolsCommands(this, FileViewModel.Instance.File).StartCMGDBEditor());
        private RelayCommand<object>? _measureDurationCalculatorCommand;
        public RelayCommand<object> MeasureDurationCalculatorCommand =>
            _measureDurationCalculatorCommand ??= new RelayCommand<object>(execute => new ToolsCommands(this, FileViewModel.Instance.File).MeasureDurationCalculator());
        private RelayCommand<object>? _oscillatorFrequencyCalculatorCommand;
        public RelayCommand<object> OscillatorFrequencyCalculatorCommand =>
            _oscillatorFrequencyCalculatorCommand ??= new RelayCommand<object>(execute => new ToolsCommands(this, FileViewModel.Instance.File).OscillatorFrequencyCalculator());
        private RelayCommand<object>? _setGeneratorsDurationEqualCommand;
        public RelayCommand<object> SetGeneratorsDurationEqualCommand =>
            _setGeneratorsDurationEqualCommand ??= new RelayCommand<object>(execute => new ToolsCommands(this, FileViewModel.Instance.File).SetGeneratorsDurationEqual());
        private RelayCommand<object>? _staggerGeneratorsStartTimeCommand;
        public RelayCommand<object> StaggerGeneratorsStartTimeCommand =>
            _staggerGeneratorsStartTimeCommand ??= new RelayCommand<object>(execute => new ToolsCommands(this, FileViewModel.Instance.File).StaggerGeneratorsStartTime());
        private RelayCommand<object>? _alignGeneratorsCommand;
        public RelayCommand<object> AlignGeneratorsCommand =>
            _alignGeneratorsCommand ??= new RelayCommand<object>(execute => new ToolsCommands(this, FileViewModel.Instance.File).AlignGenerators());
        #endregion
    }
}
