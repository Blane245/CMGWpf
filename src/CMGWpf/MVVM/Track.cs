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
                ToolsViewModel.Instance?.NotifyGeneratorListChanged();
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
            if (vm.ActiveRenameDialog != null)
            {
                _ = MessageBox.Show($"Rename already in progress for track '{vm.Track.Name}'", "Duplicate Rename Dialog", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            vm.NewTrackName = vm.Track.Name;
            vm.ActiveRenameDialog = new RenameTrack
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            vm.StatusMessages.Clear();
            vm.ActiveRenameDialog.Show();
        }
        public void RenameOK()
        {
            if (vm.Track == null) return;
            string newName = vm.NewTrackName.Trim();
            string oldName = vm.Track.Name;
            if (string.IsNullOrEmpty(newName))
            {
                _ = MessageBox.Show("Track name cannot be empty.", "Invalid Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            foreach (TrackViewModel trackVM in TracksViewModel.Instance.Tracks)
            {
                if (vm.Track != trackVM.Track && string.Equals(trackVM.Track.Name, newName, StringComparison.OrdinalIgnoreCase))
                {
                    _ = MessageBox.Show($"Track name '{newName}' already exists. Please choose a different name.", "Duplicate Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            vm.Track.Name = newName;
            vm.IsDirty = true;
            vm.Status = [new Message { Text = $"Renamed track: '{oldName}' to '{newName}'", Error = false }];
            vm.NotifyTrackChanged(vm.Track);
            vm.ActiveRenameDialog?.Close();
            vm.ActiveRenameDialog = null;
            ToolsViewModel.Instance?.NotifyGeneratorListChanged();
        }
        public void Mute()
        {
            if (vm.Track == null) return;
            vm.Track.Mute = !vm.Track.Mute;
            vm.IsDirty = true;
            vm.Status = new ObservableCollection<Message> { new Message { Text = $"Track '{vm.Track.Name}' is now {(vm.Track.Mute ? "muted" : "unmuted")}.", Error = false } };
            vm.NotifyTrackChanged(vm.Track);
        }
        public void Solo()
        {
            if (vm.Track == null) return;
            vm.Track.Solo = !vm.Track.Solo;
            vm.IsDirty = true;
            vm.Status = new ObservableCollection<Message> { new Message { Text = $"Track '{vm.Track.Name}' is now {(vm.Track.Solo ? "soloed" : "unsoloed")}.", Error = false } };
            vm.NotifyTrackChanged(vm.Track);
        }
        public void MoveUp()
        {
            if (vm.Track == null) return;
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
        public void MoveDown()
        {
            if (vm.Track == null) return;
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
                "Stochastic" => new Panels.Stochastic.StochasticPanel(),
                _ => null
            };
            GeneratorDialog activeDialog = new()
            {
                DataContext = generatorViewModel,
                SizeToContent = SizeToContent.WidthAndHeight,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            generatorViewModel.ActiveGeneratorDialog = activeDialog;
            activeDialog.ShowDialog();
        }
        public void Shift()
        {
            if (vm.Track == null) return;
            // check if a generator on this track is being edited and block if so
            if (vm.CachedGenerators != null)
            {
                foreach (var gVm in vm.CachedGenerators)
                {
                    if (gVm.ActiveGeneratorDialog != null)
                    {
                        _ = MessageBox.Show($"One or more generators on track '{vm.Track.Name}' are being edited. Track cannot be duplicated.", "Track Duplication Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }

            // prevent multiple shift dialogs
            if (vm.ActiveShiftDialog != null)
            {
                _ = MessageBox.Show($"Track shift already in progress for track '{vm.Track.Name}'", "Duplicate Shift Dialog", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            vm.ActiveShiftDialog = new TrackShift
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            vm.StatusMessages.Clear();
            vm.ActiveShiftDialog.Show();
        }
        public void ShiftOK()
        {
            if (vm.Track == null) return;

            // check that the shift amount will not cause any of the track's generator to have a start time less that 0
            Generator? g = vm.Track.Generators.Find((g) => g.StartTime + vm.ShiftAmount < 0);
            if (g != null)
            {
                _ = MessageBox.Show($"The shift amount will move the start time of at least one generator, '{g.Name}', before zero.", "Track Shift Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Track newTrack = vm.Track.Clone();
            foreach (var generator in newTrack.Generators)
            {
                generator.StartTime += vm.ShiftAmount;
                generator.StopTime += vm.ShiftAmount;
            }
            vm.NotifyTrackChanged(newTrack);
            vm.IsDirty = true;
            vm.Status = [new Message { Text = $"Generators for track '{vm.Track.Name} shifted by {vm.ShiftAmount} seconds", Error = false }];
            vm.ActiveShiftDialog?.Close();
            vm.ActiveShiftDialog = null;
        }
        public void Duplicate()
        {
            if (vm.Track == null) return;
            // check if a generator on this track is being edited and block if so
            if (vm.CachedGenerators != null)
            {
                foreach (var gVm in vm.CachedGenerators)
                {
                    if (gVm.ActiveGeneratorDialog != null)
                    {
                        _ = MessageBox.Show($"One or more generators on track '{vm.Track.Name}' are being edited. Track cannot be duplicated.", "Track Duplication Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            MessageBoxResult response = MessageBox.Show($"Press OK to confirm duplication of track '{vm.Track.Name}' or Cancel to abort duplication.", "Track Duplication", MessageBoxButton.OKCancel);
            if (response == MessageBoxResult.Cancel) return;

            // Create new file first
            CMGFile newFile = FileViewModel.Instance.File.Clone();

            // Clone the track - this already clones all generators with the correct parent reference
            Track newTrack = vm.Track.Clone();
            int uid = Utilities.Uid.Get("Track", newFile.Tracks);
            newTrack.Name = "T" + uid.ToString();

            // ADD the track to the file BEFORE renaming generators
            // so Uid.Get can see all generators when generating unique names
            newFile.Tracks.Add(newTrack);

            // NOW rename generators to have unique names (Uid.Get can see them in newFile.Tracks)
            foreach (var g in newTrack.Generators)
            {
                int gUid = Utilities.Uid.Get("Generator", newFile.Tracks);
                g.Name = "G" + gUid.ToString();
            }
            vm.IsDirty = true;

            // Set the new file - this automatically triggers TracksViewModel to refresh
            FileViewModel.Instance.File = newFile;

            Services.GlobalService.Instance.StatusMessages = [new Message() { Text = $"Track '{vm.Track.Name}' duplicated to Track '{newTrack.Name}'.", Error = false }];
            ToolsViewModel.Instance?.NotifyGeneratorListChanged();
        }
        public void Volume()
        {
            if (vm.Track == null) return;
            if (vm.ActiveVolumeDialog != null)
            {
                _ = MessageBox.Show($"Volume adjustment already in progress for track '{vm.Track.Name}'", "Duplicate Volume Dialog", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            vm.ActiveVolumeDialog = new TrackVolume
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            vm.StatusMessages = [];
            vm.ActiveVolumeDialog.Show();
        }
        public void VolumeOK()
        {
            if (vm.Track == null) return;
            vm.Track.Volume = vm.NewVolume;
            vm.IsDirty = true;
            vm.Status = [new Message { Text = $"Volume for track '{vm.Track.Name} set to {vm.Track.Volume}dB", Error = false }];
            vm.NotifyTrackChanged(vm.Track);
            vm.ActiveVolumeDialog?.Close();
            vm.ActiveVolumeDialog = null;
        }
        #endregion
    }
}
