using CMGWpf.Dialogs.Tools;
using CMGWpf.Model;
using CMGWpf.Model.Generators;
using CMGWpf.View;
using System;
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
        private ObservableCollection<TrackGeneratorName> ExtractNames (ObservableCollection<string> list)
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
        private TrackGeneratorName ExtractName(string name)
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
        private void PopError(TrackGeneratorName name)
        {
            _ = MessageBox.Show($"An error occurred while locating track '{name.TrackName}', generator '{name.GeneratorName}'. No changes have been made.", "***SYSTEM ERROR***", MessageBoxButton.OK);
        }
        // given a primary generator, a list of secondary generators, and the option to maintain the position of the start or stop time of the generators set the duration of the secondary generators to that of the primary generator. 
        public void SetGeneratorsDurationEqual()
        {
            TrackGeneratorName primaryName = ExtractName(vm.PrimaryGeneratorName);
            ObservableCollection<TrackGeneratorName> secondaryNames = ExtractNames(vm.SecondaryGeneratorNames);
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
        }
        // given a primary generator and a list of secondary generators stagger there start time by the amount specified in stagger amount
        public void StaggerGeneratorsStartTime()
        {
            TrackGeneratorName primaryName = ExtractName(vm.PrimaryGeneratorName);
            ObservableCollection<TrackGeneratorName> secondaryNames = ExtractNames(vm.SecondaryGeneratorNames);
            double staggerAmount = vm.StaggerAmount;
            (var primaryTrack, var primaryGenerator) = Locate(primaryName);
            if (primaryTrack == null || primaryGenerator == null) { PopError(primaryName); return; }
            double staggerSum = 0;
            foreach (var secondaryName in secondaryNames)
            {
                (var secondaryTrack, var secondaryGenerator) = Locate(secondaryName);
                if (secondaryTrack == null || secondaryGenerator == null) { PopError(secondaryName); return; }
                staggerSum += staggerAmount;
                secondaryGenerator.startTime = primaryGenerator.startTime + staggerAmount;
                secondaryGenerator.StopTime = primaryGenerator.StopTime + staggerAmount;
            }
            // signal that the secondary tracks have changed
            FileViewModel.Instance.NotifyTracksChanged(file.Tracks);
        }
        // given a primary generator, list of secondary generators, and a align time option, align the secondary generators with the primary one
        public void AlignGenerators()
        {
            TrackGeneratorName primaryName = ExtractName(vm.PrimaryGeneratorName);
            ObservableCollection<TrackGeneratorName> secondaryNames = ExtractNames(vm.SecondaryGeneratorNames);
            string option = vm.AlignTimeOption;
            (var primaryTrack, var primaryGenerator) = Locate(primaryName);
            if (primaryTrack == null || primaryGenerator == null) { PopError(primaryName); return; }
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
        }
    }
}