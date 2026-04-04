using CMGWpf.MVVM;
using CMGWpf.Services;
using CMGWpf.Types;
using System.Collections.ObjectModel;
using System.Windows;
using Track = CMGWpf.Model.Track;

namespace CMGWpf.View
{
    public class TrackViewModel : ViewModelBase
    {
        private Track track;
        private ObservableCollection<GeneratorViewModel>? cachedGenerators;

        public TrackViewModel(Track track)
        {
            this.track = track;
            _newVolume = track.Volume;
        }

        #region View Properties
        public Track Track { get => track;
            set { track = value; OnPropertyChanged(nameof(Track)); }
        }

        // Expose generators as GeneratorViewModels for the TrackDisplay
        public ObservableCollection<GeneratorViewModel> Generators
        {
            get
            {
                if (cachedGenerators == null)
                {
                    cachedGenerators = new ObservableCollection<GeneratorViewModel>();
                    foreach (var generator in track.Generators)
                    {
                        var generatorVM = new GeneratorViewModel(generator, this);
                        generatorVM.InitializeSubscriptions();
                        cachedGenerators.Add(generatorVM);
                    }
                }
                return cachedGenerators;
            }
        }

        public Window? ActiveDialog
        {
            get => GlobalService.Instance.ActiveDialog;
            set
            {
                GlobalService.Instance.ActiveDialog = value;
                OnPropertyChanged();
            }
        }
        public bool IsDirty
        {
            get => GlobalService.Instance.IsDirty;
            set
            {
                GlobalService.Instance.IsDirty = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Message> Status
        {
            get => GlobalService.Instance.StatusMessages;
            set
            {
                GlobalService.Instance.StatusMessages = value;
                OnPropertyChanged();
            }
        }
        private string newTrackName = string.Empty;

        public string NewTrackName
        {
            get { return newTrackName; }
            set { newTrackName = value; }
        }
        private double _shiftAmount = 0;
        public double ShiftAmount { get => _shiftAmount; set { _shiftAmount = Math.Round(value,2); OnPropertyChanged(); } }
        private int _newVolume = 0;
        public int NewVolume { get => _newVolume; set { _newVolume = value; OnPropertyChanged(); } }
        public void NotifyTrackChanged(Track newTrack)
        {
            Track = newTrack;
            // Clear the cached generators so they get recreated with the updated track
            cachedGenerators = null;
            OnPropertyChanged(nameof(Generators));
        }


        #endregion
        #region Track Control Commands
        private RelayCommand<Track>? _deleteCommand;
        public RelayCommand<Track> DeleteCommand =>
            _deleteCommand ??= new RelayCommand<Track>(execute => new TrackCommands(this).Delete());
        private RelayCommand<Track>? _renameCommand;
        public RelayCommand<Track> RenameCommand =>
            _renameCommand ??= new RelayCommand<Track>(execute => new TrackCommands(this).Rename());
        private RelayCommand<Track>? _renameOKCommand;
        public RelayCommand<Track> RenameOKCommand =>
            _renameOKCommand ??= new RelayCommand<Track>(execute => new TrackCommands(this).RenameOK());
        private RelayCommand<Track>? _renameCancelCommand;
        public RelayCommand<Track> RenameCancelCommand =>
            _renameCancelCommand ??= new RelayCommand<Track>(execute => new TrackCommands(this).RenameCancel());
        private RelayCommand<Track>? _muteCommand;
        public RelayCommand<Track> MuteCommand =>
            _muteCommand ??= new RelayCommand<Track>(execute => new TrackCommands(this).Mute());
        private RelayCommand<Track>? _soloCommand;
        public RelayCommand<Track> SoloCommand =>
            _soloCommand ??= new RelayCommand<Track>(execute => new TrackCommands(this).Solo());
        private RelayCommand<Track>? _moveUpCommand;
        public RelayCommand<Track> MoveUpCommand =>
            _moveUpCommand ??= new RelayCommand<Track>(execute => new TrackCommands(this).MoveUp());
        private RelayCommand<Track>? _moveDownCommand;
        public RelayCommand<Track> MoveDownCommand =>
            _moveDownCommand ??= new RelayCommand<Track>(execute => new TrackCommands(this).MoveDown());

        private RelayCommand<Track>? _addSilentCommand;
        public RelayCommand<Track> AddSilentCommand =>
            _addSilentCommand ??= new RelayCommand<Track>(execute => new TrackCommands(this).AddGenerator(Model.Generators.GENERATORTYPE.Silent));
        private RelayCommand<Track>? _addAlgorithmicCommand;
        public RelayCommand<Track> AddAlgorithmicCommand =>
            _addAlgorithmicCommand ??= new RelayCommand<Track>(execute => new TrackCommands(this).AddGenerator(Model.Generators.GENERATORTYPE.Algorithmic));
        private RelayCommand<Track>? _addStochasticCommand;
        public RelayCommand<Track> AddStochasticCommand =>
            _addStochasticCommand ??= new RelayCommand<Track>(execute => new TrackCommands(this).AddGenerator(Model.Generators.GENERATORTYPE.Stochastic));
        private RelayCommand<Track>? _duplicateCommand;
        public RelayCommand<Track> DuplicateCommand =>
            _duplicateCommand ??= new RelayCommand<Track>(track => new TrackCommands(this).Duplicate());
        private RelayCommand<Track>? _shiftCommand;
        public RelayCommand<Track> ShiftCommand =>
            _shiftCommand ??= new RelayCommand<Track>(track => new TrackCommands(this).Shift());
        private RelayCommand<Track>? _shiftOKCommand;
        public RelayCommand<Track> ShiftOKCommand =>
            _shiftOKCommand ??= new RelayCommand<Track>(track => new TrackCommands(this).ShiftOK());
        private RelayCommand<Track>? _volumeCommand;
        public RelayCommand<Track> VolumeCommand =>
            _volumeCommand ??= new RelayCommand<Track>(track => new TrackCommands(this).Volume());
        private RelayCommand<Track>? _volumeOKCommand;
        public RelayCommand<Track> VolumeOKCommand =>
            _volumeOKCommand ??= new RelayCommand<Track>(track => new TrackCommands(this).VolumeOK());
        #endregion
    }
}
