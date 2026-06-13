
using CMGWpf.Model;
using CMGWpf.Model.Generators;
using CMGWpf.Services;
using CMGWpf.Types;
using System.Collections.ObjectModel;

namespace CMGWpf.View
{
    // This singlet view model manages the state and behavior of the tracks view in the body user control, which displays all of the tracks in the composition. Each track is shown in the body user control with a set of track controls and a canvas containing the graphical representation of the track's generators. Note that the Edit menu handles the adding of a new track. This view model will handle:
    // 1. renaming and deleting tracks
    // 2. Setting mute and solo states for tracks
    // 3. Adding generators
    // 4. Moving tracks up and down in the track list
    // 5. Duplicating tracks
    // 6. Adjusting track volume
    // 7. Shifting track generators in time
    // 8. Display of each of the track's generators in the track display canvas
    public class TracksViewModel : ViewModelBase
    {
        private static TracksViewModel? _instance;
        public static TracksViewModel Instance => _instance ??= new TracksViewModel();
        public ObservableCollection<TrackViewModel>? CachedTracks;

        private TracksViewModel()
        {
            // Subscribe to FileViewModel property changes to propagate Tracks updates
            FileViewModel.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FileViewModel.Instance.File))
                {
                    CachedTracks = null;
                    OnPropertyChanged(nameof(Tracks));
                }
            };
        }
        #region View Properties
        public ObservableCollection<TrackViewModel> Tracks
        {
            get
            {
                if (CachedTracks == null)
                {
                    List<Track> tracks = FileViewModel.Instance.File.Tracks;
                    CachedTracks = tracks == null ? [] : new ObservableCollection<TrackViewModel>(tracks.Select(t => new TrackViewModel(t)));
                }
                return CachedTracks;
            }
            set
            {
                FileViewModel.Instance.File.Tracks = [.. value.Select(vm => vm.Track)];
                CachedTracks = null;
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

        private TrackViewModel? selectedTrack = null;
        public TrackViewModel? SelectedTrack
        {
            get => selectedTrack;
            set
            {
                selectedTrack = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #region Tracks Commands
        // when the mouse enters a track select the track and update the selected track field, which will be used for some of the commands. When the mouse leaves a track, deselect the track and set the selected track field to null. Note that this behavior may need to be modified based on UI needs, but this is a starting point.
        private RelayCommand<TrackViewModel>? _mouseEnterCommand;
        public RelayCommand<TrackViewModel> MouseEnterCommand =>
            _mouseEnterCommand ??= new RelayCommand<TrackViewModel>(trackVM => { SelectedTrack = trackVM; });

        private RelayCommand<TrackViewModel>? _mouseLeaveCommand;
        public RelayCommand<TrackViewModel> MouseLeaveCommand =>
            _mouseLeaveCommand ??= new RelayCommand<TrackViewModel>(trackVM => { SelectedTrack = null; });

        public void NotifyTracksChanged(ObservableCollection<TrackViewModel> newTracks)
        {
            Tracks = newTracks;
        }

        /// <summary>
        /// Refreshes all cached track view models to reflect changes in the underlying track data
        /// </summary>
        public void RefreshAllTracks()
        {
            CachedTracks = null;
            OnPropertyChanged(nameof(Tracks));
        }

        /// <summary>
        /// Moves or copies a generator from one track to another
        /// </summary>
        public void MoveOrCopyGenerator(Generator generator, Track sourceTrack, Track targetTrack, MoveCopyMode mode, string newGeneratorName)
        {
            if (mode == MoveCopyMode.Copy)
            {
                // Create a copy of the generator and add it to the target track
                Generator copiedGenerator = generator.Clone(targetTrack);
                copiedGenerator.Name = newGeneratorName;
                targetTrack.Generators.Add(copiedGenerator);
                Status = [new Message { Text = $"Generator '{generator.Name}' copied to track '{targetTrack.Name}' as '{newGeneratorName}'.", Error = false }];
            }
            else if (mode == MoveCopyMode.Move)
            {
                // Remove the generator from the source track
                sourceTrack.Generators.RemoveAll((Generator g) => g.Name == generator.Name);
                // Update the parent reference and add to target track
                generator.Parent = targetTrack;
                targetTrack.Generators.Add(generator);
                Status = [new Message { Text = $"Generator '{generator.Name}' moved from track '{sourceTrack.Name}' to track '{targetTrack.Name}'.", Error = false }];
            }
            else
            {
                Status = [new Message { Text = $"SYSTEM ERROR: Invalid move/copy mode '{mode}'.", Error = true }];
                return;
            }

            // Mark as dirty
            IsDirty = true;
            // Refresh all track view models to reflect the changes
            RefreshAllTracks();
            ToolsViewModel.Instance?.NotifyGeneratorListChanged();
        }
        #endregion
    }
}
