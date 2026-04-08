using CMGWpf.Dialogs.Tools;
using CMGWpf.Model;
using CMGWpf.Model.Generators;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

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
        private static ObservableCollection<TrackGeneratorName> ExtractNames (ObservableCollection<string> list)
        {
            ObservableCollection<TrackGeneratorName> result = [];
            foreach (var item in list)
            {
                string[] split = item.Split(":");
                if (split.Length == 2) {
                    result.Add(new TrackGeneratorName()
                    {
                        TrackName = split[0],
                        GeneratorName = split[1]
                    });
                }
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
                _ = MessageBox.Show("Tracks cannot be stagger while any generators are being added or edited.", "Stagger Generators Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            vm.ActiveSetGeneratorsDurationEqualDialog = new SetGeneratorsDurationEqualDialog
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            vm.ActiveSetGeneratorsDurationEqualDialog.ShowDialog();
        }
        private bool isPrimaryinSecondaryList (TrackGeneratorName primaryName, ObservableCollection<TrackGeneratorName> secondaryNames)
        {
            try
            {
                _ = secondaryNames.First((s) => (s.TrackName == primaryName.TrackName && s.GeneratorName == primaryName.GeneratorName));
                return true;
            } catch
            {
                return false;
            }
        }
        public void SetEqual()
        {
            TrackGeneratorName primaryName = ExtractName(vm.PrimaryGeneratorName);
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
            vm.ActiveSetGeneratorsDurationEqualDialog?.Close();
            vm.ActiveSetGeneratorsDurationEqualDialog = null;
        }
        // given a primary generator and a list of secondary generators stagger there start time by the amount specified in stagger amount
        public void Stagger()
        {
            TrackGeneratorName primaryName = ExtractName(vm.PrimaryGeneratorName);
            ObservableCollection<TrackGeneratorName> secondaryNames = ExtractNames(vm.SecondaryGeneratorNames);
            if (isPrimaryinSecondaryList(primaryName, secondaryNames))
            {
                _ = MessageBox.Show($"The primary generator cannot appear in the secondary list", "Generator Selection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            double staggerAmount = vm.StaggerAmount;
            (var primaryTrack, var primaryGenerator) = Locate(primaryName);
            if (primaryTrack == null || primaryGenerator == null) { PopError(primaryName); return; }
            double staggerSum = 0;
            foreach (var secondaryName in secondaryNames)
            {
                (var secondaryTrack, var secondaryGenerator) = Locate(secondaryName);
                if (secondaryTrack == null || secondaryGenerator == null) { PopError(secondaryName); return; }
                staggerSum += staggerAmount;
                secondaryGenerator.StartTime = primaryGenerator.StartTime + staggerSum;
                secondaryGenerator.StopTime = primaryGenerator.StopTime + staggerSum;
            }
            // signal that the secondary tracks have changed
            FileViewModel.Instance.NotifyTracksChanged(file.Tracks);
            vm.ActiveStaggerGeneratorsStartTimeDialog?.Close();
            vm.ActiveStaggerGeneratorsStartTimeDialog = null;
        }
        // given a primary generator, list of secondary generators, and a align time option, align the secondary generators with the primary one
        public void Align()
        {
            TrackGeneratorName primaryName = ExtractName(vm.PrimaryGeneratorName);
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
            vm.ActiveAlignGeneratorsDialog?.Close();
            vm.ActiveAlignGeneratorsDialog = null;
        }
    }
}