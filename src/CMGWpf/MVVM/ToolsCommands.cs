using CMGWpf.Helpers;
using CMGWpf.Model;
using CMGWpf.Model.Generators;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Windows;
using static CMGWpf.View.ToolsViewModel;

namespace CMGWpf.MVVM
{
    public class ToolsCommands(ToolsViewModel vm, CMGFile file)
    {
        private readonly ToolsViewModel vm = vm;
        private CMGFile file = file;
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
        private static bool CheckActiveGenerators()
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
        private static bool IsPrimaryinSecondaryList(TrackGeneratorName primaryName, ObservableCollection<TrackGeneratorName> secondaryNames)
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
            vm.Messages = [];
            // prevent execution when any generators are in edit mode
            if (CheckActiveGenerators())
            {
                Messages.Add(vm.Messages, "Generators cannot be aligned while any generator is being edited", true);
            }
            TrackGeneratorName primaryName = ExtractName(vm.PrimaryGeneratorName);
            if (vm.PrimaryGeneratorName == "")
            {
                Messages.Add(vm.Messages, $"No primary generator has been selected", true);
            }
            if (vm.SecondaryGeneratorNames.Count == 0)
            {
                Messages.Add(vm.Messages, $"No secondary generators have need selected", true);
            }
            ObservableCollection<TrackGeneratorName> secondaryNames = ExtractNames(vm.SecondaryGeneratorNames);
            if (IsPrimaryinSecondaryList(primaryName, secondaryNames)) {
                Messages.Add(vm.Messages, $"The primary generator cannot appear in the secondary list", true);
            }
            if (vm.Messages.Count != 0) return;
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
            Messages.Add(vm.Messages, $"Set {secondaryNames.Count} generators to have the generator {vm.PrimaryGeneratorName} duration {duration} leaving {option} fixed.", false);
            FileViewModel.Instance.IsDirty = true;
        }
        // given a primary generator and a list of secondary generators stagger there start time by the amount specified in stagger amount
        public void Stagger()
        {
            vm.Messages = [];
            // prevent execution when any generators are in edit mode
            if (CheckActiveGenerators())
            {
                Messages.Add(vm.Messages, "Generators cannot be staggered while any generator is being edited", true);
            }
            TrackGeneratorName primaryName = ExtractName(vm.PrimaryGeneratorName);
            if (vm.PrimaryGeneratorName == "")
            {
                Messages.Add(vm.Messages, $"No primary generator has been selected", true);
            }
            ObservableCollection<TrackGeneratorName> secondaryNames = ExtractStaggerNames(vm.StaggerGeneratorList);
            if (secondaryNames.Count == 0)
            {
                Messages.Add(vm.Messages, $"No secondary generators have need selected", true);
            }
            if (IsPrimaryinSecondaryList(primaryName, secondaryNames))
            {
                Messages.Add(vm.Messages, $"The primary generator cannot appear in the secondary list", true);
            }
            if (vm.Messages.Count != 0) return;
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
            Messages.Add(vm.Messages, $"Staggered {secondaryNames.Count} generators from generator {vm.PrimaryGeneratorName} by {staggerAmount} seconds.", false);
            FileViewModel.Instance.IsDirty = true;
        }
        // given a primary generator, list of secondary generators, and a align time option, align the secondary generators with the primary one
        public void Align()
        {
            vm.Messages = [];
            // prevent execution when any generators are in edit mode
            if (CheckActiveGenerators())
            {
                Messages.Add(vm.Messages, "Generators cannot be staggered while any generator is being edited", true);
            }
            TrackGeneratorName primaryName = ExtractName(vm.PrimaryGeneratorName);
            if (vm.PrimaryGeneratorName == "")
            {
                Messages.Add(vm.Messages, $"No primary generator has been selected", true);
            }
            if (vm.SecondaryGeneratorNames.Count == 0)
            {
                Messages.Add(vm.Messages, $"No secondary generators have need selected", true);
            }
            ObservableCollection<TrackGeneratorName> secondaryNames = ExtractNames(vm.SecondaryGeneratorNames);
            if (IsPrimaryinSecondaryList(primaryName, secondaryNames))
            {
                Messages.Add(vm.Messages, $"The primary generator cannot appear in the secondary list", true);
            }
            if (vm.Messages.Count != 0) return;

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
                        Messages.Add(vm.Messages, $"Alignment by Stop Time will cause one or more generators to have a Start Time less than zero.", true);
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
            Messages.Add(vm.Messages, $"Aligned {secondaryNames.Count} generators to {vm.PrimaryGeneratorName} by {option}.", false);
            FileViewModel.Instance.IsDirty = true;
        }
    }
}