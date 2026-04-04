using CMGWpf.Dialogs;
using CMGWpf.Dialogs.TrackTools;
using CMGWpf.Model;
using CMGWpf.Model.Generators;
using CMGWpf.Types;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Windows;

namespace CMGWpf.MVVM
{
    public class TrackCommands(TrackViewModel vm)
    {
        private readonly TrackViewModel vm = vm;
        #region Track Control Commands
        public void Delete()
        {
            if (vm.Track == null) return;
            System.Diagnostics.Debug.WriteLine($"Delete track {vm.Track.Name} command executed.");

            // need a message here to the user to confirm deletion of the track, as this cannot be undone.
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete the track '{vm.Track}'? This action cannot be undone.", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                // User confirmed deletion, proceed with deleting the track
                ObservableCollection<TrackViewModel> newTracks = [];
                foreach (TrackViewModel trackVM in TracksViewModel.Instance.Tracks)
                {
                    if (trackVM.Track != vm.Track)
                        newTracks.Add(trackVM);
                }
                vm.Status = [new Message { Text = $"Deleted track: '{vm.Track.Name}'", Error = false }];
                TracksViewModel.Instance.Tracks = newTracks;
                TracksViewModel.Instance.NotifyTracksChanged(newTracks);
                vm.IsDirty = true;
            }
            else
            {
                // User canceled deletion, exit the method
                vm.Status = new ObservableCollection<Message> { new Message { Text = $"Deletion of track '{vm.Track.Name}' canceled.", Error = false } };
                return;
            }

        }
        public void Rename()
        {
            if (vm.Track == null) return;
            System.Diagnostics.Debug.WriteLine($"Rename track {vm.Track.Name} command executed.");
            vm.NewTrackName = vm.Track.Name;
            vm.ActiveDialog = new RenameTrack
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            vm.ActiveDialog.ShowDialog();
        }
        public void RenameOK()
        {
            if (vm.Track == null) return; 
            string newName = vm.NewTrackName.Trim();
            string oldName = vm.Track.Name;
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Track name cannot be empty.", "Invalid Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            foreach (TrackViewModel trackVM in TracksViewModel.Instance.Tracks)
            {
                if (vm.Track != trackVM.Track && string.Equals(trackVM.Track.Name, newName, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"Track name '{newName}' already exists. Please choose a different name.", "Duplicate Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            //Track newTrack = vm.Track.Clone();
            //newTrack.Name = newName;
            vm.Track.Name = newName;
            vm.IsDirty = true;
            vm.Status = new ObservableCollection<Message> { new Message { Text = $"Renamed track: '{oldName}' to '{newName}'", Error = false } };
            vm.NotifyTrackChanged(vm.Track);
            vm.ActiveDialog?.Close();
        }
        public void RenameCancel()
        {
            System.Diagnostics.Debug.WriteLine("Cancel Track Rename command being executed.");
            vm.Status = new ObservableCollection<Message> { new Message { Text = "Track name not changed.", Error = false } };
            vm.ActiveDialog?.Close();
        }

        public void Mute()
        {
            if (vm.Track == null) return;
            System.Diagnostics.Debug.WriteLine($"Mute track {vm.Track.Name} command executed.");
            vm.Track.Mute = !vm.Track.Mute;
            vm.IsDirty = true;
            vm.Status = new ObservableCollection<Message> { new Message { Text = $"Track '{vm.Track.Name}' is now {(vm.Track.Mute ? "muted" : "unmuted")}.", Error = false } };
            vm.NotifyTrackChanged(vm.Track);
        }
        public void Solo()
        {
            if (vm.Track == null) return;
            System.Diagnostics.Debug.WriteLine($"Solo track {vm.Track.Name} command executed.");
            vm.Track.Solo = !vm.Track.Solo;
            vm.IsDirty = true;
            vm.Status = new ObservableCollection<Message> { new Message { Text = $"Track '{vm.Track.Name}' is now {(vm.Track.Solo ? "soloed" : "unsoloed")}.", Error = false } };
            vm.NotifyTrackChanged(vm.Track);
        }
        public void MoveUp() { 
            if (vm.Track == null) return;
            System.Diagnostics.Debug.WriteLine($"Move track {vm.Track.Name} up command executed.");
            int index = -1;
            var tracks = TracksViewModel.Instance.Tracks;
            for (int i = 0; i < tracks.Count; i++)
            {
                if (tracks[i].Track == vm.Track)
                {
                    index = i;
                    break;
                }
            }
            if (index > 0)
            {
                ObservableCollection<TrackViewModel> newTracks = new(tracks);
                (newTracks[index], newTracks[index - 1]) = (newTracks[index - 1], newTracks[index]);
                TracksViewModel.Instance.Tracks = newTracks;
                TracksViewModel.Instance.NotifyTracksChanged(newTracks);
                vm.IsDirty = true;
                vm.Status = new ObservableCollection<Message> { new Message { Text = $"Moved track '{vm.Track.Name}' up.", Error = false } };
            }
        }
        public void MoveDown() { 
            if (vm.Track == null) return;
            System.Diagnostics.Debug.WriteLine($"Move track {vm.Track.Name} down command executed.");
            int index = -1;
            var tracks = TracksViewModel.Instance.Tracks;
            for (int i = 0; i < tracks.Count; i++)
            {
                if (tracks[i].Track == vm.Track)
                {
                    index = i;
                    break;
                }
            }
            if (index < tracks.Count - 1 && index >= 0)
            {
                ObservableCollection<TrackViewModel> newTracks = new(tracks);
                (newTracks[index], newTracks[index + 1]) = (newTracks[index + 1], newTracks[index]);
                TracksViewModel.Instance.Tracks = newTracks;
                TracksViewModel.Instance.NotifyTracksChanged(newTracks);
                vm.IsDirty = true;
                vm.Status = new ObservableCollection<Message> { new Message { Text = $"Moved track '{vm.Track.Name}' down.", Error = false } };
            }
        }
        public void AddGenerator(GENERATORTYPE type)
        {
            if (vm.Track == null) return;
            System.Diagnostics.Debug.WriteLine($"Add ${type} generator to track {vm.Track.Name} command executing.");
            var generator = Generator.GeneratorFactory.Create(type, vm.Track);
            GeneratorViewModel generatorViewModel = new(generator, vm)
            {
                UIGenerator = generator.Clone(vm.Track),
                Mode = GeneratorEditMode.Add,
                NewGeneratorName = generator.Name
            };
            string generatorType = generator.ToString();
            generatorViewModel.GeneratorPanel = generatorType switch
            {
                "Algorithmic" => new Panels.Algorithmic.AlgorithmicPanel(),
                "Silent" => null, // Silent has no additional controls
                "Stochastic" => new Panels.Stochastic.StochasticPanel(),
                _ => null
            };
            Window activeDialog = new GeneratorDialog()
            {
                DataContext = generatorViewModel,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SizeToContent = SizeToContent.WidthAndHeight,
            };
            vm.ActiveDialog = activeDialog;
            vm.ActiveDialog.ShowDialog();
        }
        private static TrackShift? _trackShift = null;
        private static TrackVolume? _trackVolume = null;
        public void Shift()
        {
            if (vm.Track == null) return;
            _trackShift = new() { 
                Owner = Application.Current.MainWindow,
                DataContext = vm
            };
            _trackShift.Show();
        }
        public void ShiftOK()
        {
            if (vm.Track == null) return;
            // check that the shift amount will not cause any of the track's generator to have a start time less that 0
            Generator? g = vm.Track.Generators.Find((g) => g.StartTime + vm.ShiftAmount < 0);
            if (g != null)
            {
                _ = MessageBox.Show($"The shift amount will move the start time of at least one generator, '{g.Name}', before zero.", "Track Shift Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Track newTrack = vm.Track.Clone();
            foreach (var generator in newTrack.Generators)
            {
                generator.StartTime += vm.ShiftAmount;
                generator.StopTime += vm.ShiftAmount;
            }
            vm.NotifyTrackChanged(newTrack);
            _trackShift?.Close();
            _trackShift = null;
        }
        public void Duplicate()
        {
            if (vm.Track == null) return;
            MessageBoxResult response = MessageBox.Show($"Press OK to confirm duplication of track '{vm.Track.Name}' or Cancel to abort duplication.", "Track Duplication", MessageBoxButton.OKCancel);
            if (response == MessageBoxResult.Cancel) return;
            // duplicate the track by finding a new track uid, cloning the current one, and then adding the new track to the file
            int uid = Utilities.Uid.Get("Track", FileViewModel.Instance.File.Tracks);
            Track newTrack = vm.Track.Clone();
            newTrack.Name = "T"+uid.ToString();
            CMGFile newFile = FileViewModel.Instance.File.Clone();
            newFile.Tracks.Add(newTrack);
            FileViewModel.Instance.File = newFile; // should trigger a property change and update display
        }
        public void Volume()
        {
            if (vm.Track == null) return;
            _trackVolume = new() 
            { 
                Owner = Application.Current.MainWindow,
                DataContext = vm
            };
            _trackVolume.Show();
        }
        public void VolumeOK()
        {
            if (vm.Track == null) return;
            Track newTrack = vm.Track.Clone();
            newTrack.Volume = vm.NewVolume;
            vm.NotifyTrackChanged(newTrack);
            _trackVolume?.Close();
            _trackVolume = null;
        }
        #endregion
    }
}
