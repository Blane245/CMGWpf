using CMGWpf.Dialogs.Tools;
using CMGWpf.Model;
using CMGWpf.Model.Generators;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using static CMGWpf.View.ToolsViewModel;

namespace CMGWpf.MVVM
{
    public class ToolsCommands(ToolsViewModel vm, CMGFile file)
    {
        private readonly ToolsViewModel vm = vm;
        private CMGFile file = file;
        public void MidiFrequencyConverter()
        {
            MidiFrequencyConverterDialog dialog = new()
            {
                Owner = Application.Current.MainWindow,
            };
            dialog.Show();
        }
        public void StartCMGDBEditor()
        {
            string url = "http://localhost/cmgdbeditor";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open browser: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void MeasureDurationCalculator()
        {
            MeasureDurationCalculatorDialog dialog = new()
            {
                Owner = Application.Current.MainWindow,
            };
            dialog.Show();

        }
        public void OscillatorFrequencyCalculator()
        {
            OscillatorFrequencyCalculatorDialog dialog = new()
            {
                Owner = Application.Current.MainWindow,
            };
            dialog.Show();

        }
        // the generator names in the following functions composites of the track name and genrator name separated by a colon. This utility separates the names. One version handles a single name . The other version handles a list of names
        private class TrackGeneratorName
        {
            public string TrackName = "";
            public string GeneratorName = "";
        }
        private static ObservableCollection<TrackGeneratorName> ExtractNames(ObservableCollection<string> list)
        {
            ObservableCollection<TrackGeneratorName> result = [];
            foreach (var item in list)
            {
                string[] split = item.Split(":");
                if (split.Length == 2)
                {
                    result.Add(new TrackGeneratorName()
                    {
                        TrackName = split[0],
                        GeneratorName = split[1]
                    });
                }
            }
            return result;
        }
        private static ObservableCollection<TrackGeneratorName> ExtractStaggerNames(ObservableCollection<StaggerGeneratorsSelection> list)
        {
            ObservableCollection<TrackGeneratorName> result = [];
            foreach (var item in list)
            {
                if (item.IsSelected)
                    result.Add(new TrackGeneratorName()
                    {
                        TrackName = item.TrackName,
                        GeneratorName = item.GeneratorName
                    });
            }
            return result;
        }
        private static TrackGeneratorName ExtractName(string name)
        {
            string[] split = name.Split(":");
            if (split.Length == 2)
            {
                return new TrackGeneratorName()
                {
                    TrackName = split[0],
                    GeneratorName = split[1],
                };
            }
            else return new TrackGeneratorName()
            {
                TrackName = "",
                GeneratorName = ""
            };
        }
        // given a track and generator name combination, locate the track and generator objects
        private (Track?, Generator?) Locate (TrackGeneratorName names)
        {
            Track? targetTrack = file.Tracks.Find((t) => t.Name == names.TrackName);
            if (targetTrack == null) return (null, null);
            Generator? targetGenerator = targetTrack.Generators.Find((g) => g.Name == names.GeneratorName);
            if (targetGenerator == null) return (null, null);
            return (targetTrack, targetGenerator);
        }
        private static void PopError(TrackGeneratorName name)
        {
            _ = MessageBox.Show($"An error occurred while locating track '{name.TrackName}', generator '{name.GeneratorName}'. No changes have been made.", "***SYSTEM ERROR***", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // these three functions must check if there are any generators during being edited or added. If so, they are prevented from executing
        private bool CheckActiveGenerators()
        {
            ObservableCollection<TrackViewModel>? trackViewModels = TracksViewModel.Instance.CachedTracks;
            if (trackViewModels == null) return false;
            foreach (var trackVm in trackViewModels)
            {
                var genVms = trackVm.CachedGenerators;
                if (genVms == null) continue;
                foreach(var genVm in genVms)
                {
                    if (genVm.ActiveGeneratorDialog != null) return true;
                }
            }
            return false;
        }
        private bool isPrimaryinSecondaryList(TrackGeneratorName primaryName, ObservableCollection<TrackGeneratorName> secondaryNames)
        {
            try
            {
                _ = secondaryNames.First((s) => (s.TrackName == primaryName.TrackName && s.GeneratorName == primaryName.GeneratorName));
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void AlignGenerators()
        {
            if (CheckActiveGenerators())
            {
                _ = MessageBox.Show("Tracks cannot be aligned while any generators are being added or edited.", "Align Generators Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            vm.ActiveAlignGeneratorsDialog = new AlignGeneratorsDialog
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            vm.ActiveAlignGeneratorsDialog.ShowDialog();
        }
        public void StaggerGeneratorsStartTime()
        {
            if (CheckActiveGenerators())
            {
                _ = MessageBox.Show("Tracks cannot be stagger while any generators are being added or edited.", "Stagger Generators Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            vm.ActiveStaggerGeneratorsStartTimeDialog = new StaggerGeneratorsStartTimeDialog
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            vm.ActiveStaggerGeneratorsStartTimeDialog.ShowDialog();

        }
        public void SetGeneratorsDurationEqual()
        {
            if (CheckActiveGenerators())
            {
                _ = MessageBox.Show("Tracks cannot be set equal while any generators are being added or edited.", "Set Generators Duration Equal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            vm.ActiveSetGeneratorsDurationEqualDialog = new SetGeneratorsDurationEqualDialog
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            vm.ActiveSetGeneratorsDurationEqualDialog.ShowDialog();
        }
        public void MoveStaggerUp(StaggerGeneratorsSelection item)
        {
            // find the selected item in the stagger generator list and move it up one position
            int selected = vm.StaggerGeneratorList.IndexOf(item);
            if (selected >= 1)
            {
                ObservableCollection<StaggerGeneratorsSelection> tempList = vm.StaggerGeneratorList;
                tempList.Move(selected, selected - 1);
                vm.NotifyStaggerListChanged(tempList);

            }
        }
        public void MoveStaggerDown(StaggerGeneratorsSelection item)
        {
            // find the selected item in the stagger generator list and move it down one position
            int selected = vm.StaggerGeneratorList.IndexOf(item);
            if (selected < vm.StaggerGeneratorList.Count - 1 && selected >= 0)
            {
                ObservableCollection<StaggerGeneratorsSelection> tempList = vm.StaggerGeneratorList;
                tempList.Move(selected, selected + 1);
                vm.NotifyStaggerListChanged(tempList);
            }
        }
        public void SetEqual()
        {
            TrackGeneratorName primaryName = ExtractName(vm.PrimaryGeneratorName);
            if (vm.PrimaryGeneratorName == "")
            {
                _ = MessageBox.Show($"No primary generator has been selected", "Generator Selection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (vm.SecondaryGeneratorNames.Count == 0)
            {
                _ = MessageBox.Show($"No secondary generators have need selected", "Generator Selection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ObservableCollection<TrackGeneratorName> secondaryNames = ExtractNames(vm.SecondaryGeneratorNames);
            if (isPrimaryinSecondaryList(primaryName, secondaryNames)) {
                _ = MessageBox.Show($"The primary generator cannot appear in the secondary list", "Generator Selection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string option = vm.AlignTimeOption;
            (var primaryTrack, var primaryGenerator) = Locate(primaryName);
            if (primaryTrack == null || primaryGenerator == null) { PopError(primaryName); return; }
            double duration = primaryGenerator.StopTime - primaryGenerator.StartTime;
            foreach (var secondaryName in secondaryNames)
            {
                (var secondaryTrack, var secondaryGenerator) = Locate(secondaryName);
                if (secondaryTrack == null || secondaryGenerator == null) { PopError(secondaryName); return; }
                if (option == "Start Time")
                {
                    secondaryGenerator.StopTime = secondaryGenerator.StartTime + duration;
                } else 
                {
                    secondaryGenerator.StartTime = secondaryGenerator.StopTime - duration;
                }
            }
            // signal that the tracks have changed
            FileViewModel.Instance.NotifyTracksChanged(file.Tracks);
            FileViewModel.Instance.StatusMessages = [new Types.Message() { Text = $"Set {secondaryNames.Count} generators to have the generator {vm.PrimaryGeneratorName} duration {duration} leaving {option} fixed.", Error = false }];
            FileViewModel.Instance.IsDirty = true;
            vm.ActiveSetGeneratorsDurationEqualDialog?.Close();
            vm.ActiveSetGeneratorsDurationEqualDialog = null;
        }
        // given a primary generator and a list of secondary generators stagger there start time by the amount specified in stagger amount
        public void Stagger()
        {
            TrackGeneratorName primaryName = ExtractName(vm.PrimaryGeneratorName);
            if (vm.PrimaryGeneratorName == "")
            {
                _ = MessageBox.Show($"No primary generator has been selected", "Generator Selection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ObservableCollection<TrackGeneratorName> secondaryNames = ExtractStaggerNames(vm.StaggerGeneratorList);
            if (secondaryNames.Count == 0)
            {
                _ = MessageBox.Show($"No secondary generators have need selected", "Generator Selection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (isPrimaryinSecondaryList(primaryName, secondaryNames))
            {
                _ = MessageBox.Show($"The primary generator cannot appear in the secondary list", "Generator Selection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            double staggerAmount = vm.StaggerAmount;
            (var primaryTrack, var primaryGenerator) = Locate(primaryName);
            if (primaryTrack == null || primaryGenerator == null) { PopError(primaryName); return; }
            double staggerSum = 0;

            // stagger the selected generators based on the primary generators start time maintain the secondary generator duration
            foreach (var secondaryName in secondaryNames)
            {
                (var secondaryTrack, var secondaryGenerator) = Locate(secondaryName);
                if (secondaryTrack == null || secondaryGenerator == null) { PopError(secondaryName); return; }
                staggerSum += staggerAmount;
                double duration = secondaryGenerator.StopTime - secondaryGenerator.StartTime;
                secondaryGenerator.StartTime = primaryGenerator.StartTime + staggerSum;
                secondaryGenerator.StopTime = secondaryGenerator.StartTime + duration;
            }
            // signal that the secondary tracks have changed
            FileViewModel.Instance.NotifyTracksChanged(file.Tracks);
            FileViewModel.Instance.StatusMessages = [new Types.Message() { Text = $"Staggered {secondaryNames.Count} generators from generator {vm.PrimaryGeneratorName} by {staggerAmount} seconds.", Error = false }];
            FileViewModel.Instance.IsDirty = true;
            vm.ActiveStaggerGeneratorsStartTimeDialog?.Close();
            vm.ActiveStaggerGeneratorsStartTimeDialog = null;
        }
        // given a primary generator, list of secondary generators, and a align time option, align the secondary generators with the primary one
        public void Align()
        {
            TrackGeneratorName primaryName = ExtractName(vm.PrimaryGeneratorName);
            if (vm.PrimaryGeneratorName == "")
            {
                _ = MessageBox.Show($"No primary generator has been selected", "Generator Selection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (vm.SecondaryGeneratorNames.Count == 0)
            {
                _ = MessageBox.Show($"No secondary generators have need selected", "Generator Selection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ObservableCollection<TrackGeneratorName> secondaryNames = ExtractNames(vm.SecondaryGeneratorNames);
            if (isPrimaryinSecondaryList(primaryName, secondaryNames))
            {
                _ = MessageBox.Show($"The primary generator cannot appear in the secondary list", "Generator Selection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string option = vm.AlignTimeOption;
            (var primaryTrack, var primaryGenerator) = Locate(primaryName);
            if (primaryTrack == null || primaryGenerator == null) { PopError(primaryName); return; }

            // prevent an alignment of stop times from causing a start time to be less than zero
            if (option == "Stop Time") {
                foreach (var s in secondaryNames)
                {
                    (_, var sG) = Locate(s);
                    double? duration = sG?.StopTime - sG?.StartTime;
                    if (duration != null && primaryGenerator.StopTime - duration <0)
                    {
                        _ = MessageBox.Show($"Alignment by Stop Time will cause one or more generators to have a Start Time less than zero.", "Generator Alignment Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            foreach (var secondaryName in secondaryNames)
            {
                (var secondaryTrack, var secondaryGenerator) = Locate(secondaryName);
                if (secondaryTrack == null || secondaryGenerator == null) { PopError(secondaryName); return; }
                double duration = secondaryGenerator.StopTime - secondaryGenerator.StartTime;
                if (option == "Start Time")
                {
                    secondaryGenerator.StartTime = primaryGenerator.StartTime;
                    secondaryGenerator.StopTime = secondaryGenerator.StartTime + duration;

                } else
                {
                    secondaryGenerator.StopTime = primaryGenerator.StopTime;
                    secondaryGenerator.StartTime = secondaryGenerator.StopTime - duration;
                }
            }
            // signal that the secondary tracks have changed
            FileViewModel.Instance.NotifyTracksChanged(file.Tracks);
            FileViewModel.Instance.StatusMessages = [new Types.Message() { Text = $"Aligned {secondaryNames.Count} generators to {vm.PrimaryGeneratorName} by {option}.", Error = false }];
            FileViewModel.Instance.IsDirty = true;
            vm.ActiveAlignGeneratorsDialog?.Close();
            vm.ActiveAlignGeneratorsDialog = null;
        }
    }
}