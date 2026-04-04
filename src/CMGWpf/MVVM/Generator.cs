using CMGWpf.Dialogs;
using CMGWpf.Model;
using CMGWpf.Model.Generators;
using CMGWpf.PlayFunctions;
using CMGWpf.Types;
using CMGWpf.Utilities;
using CMGWpf.View;
using System.Windows;

namespace CMGWpf.MVVM
{
    public class GeneratorCommands(GeneratorViewModel vm, Generator generator)
    {
        private readonly GeneratorViewModel vm = vm;
        private Generator generator = generator;
        #region Generator Control Commands
        public void Submit()
        {
            if (generator == null) return;
            // perform validation and update generator properties
            vm.Messages = generator.Validate();
            if (vm.Messages.Count == 0) // the UI generator is error free, so add it to the parent track
            {
                // either add the generator or modify an existing generator based on the mode
                if (vm.Mode == GeneratorEditMode.Modify)
                {
                    // find the generator in the parent track and update its properties based on the UI generator
                    int index = generator.Parent.Generators.FindIndex((Generator g) => g.Name == generator.Name);
                    if (index < 0) // this should not happen, but if it does, add the generator to the track
                    {
                        vm.Messages.Add(new Message { Text = $"SYSTEM ERROR: Generator '{generator.Name}' not found in track '{generator.Parent.Name}'.", Error = true });
                    }
                    else
                    {
                        generator.Parent.Generators[index] = vm.UIGenerator;
                        vm.Messages.Add(new Message { Text = $"Generator '{generator.Name}' on track '{generator.Parent.Name}' updated successfully.", Error = false });
                        vm.IsDirty = true;
                    }

                }
                else if (vm.Mode == GeneratorEditMode.Add)
                {
                    generator.Parent.Generators.Add(vm.UIGenerator);
                    vm.IsDirty = true;
                    vm.Messages.Add(new Message { Text = $"Generator '{generator.Name}' on track '{generator.Parent.Name}' updated added.", Error = false });
                }
                else // this should not happen, but if it does, display an error
                {
                    vm.Messages.Add(new Message { Text = $"SYSTEM ERROR: Invalid generator edit mode '{vm.Mode}'.", Error = true });
                    return;
                }
                vm.ActiveDialog?.Close();
                generator = vm.UIGenerator;
                vm.UpdateGenerator(vm.UIGenerator);
                vm.NotifyTrackChanged();
                vm.NotifyGeneratorChanged();
            }
            else
            {
                vm.NotifyGeneratorChanged(nameof(vm.Messages));
            }
        }
        public void Delete()
        {
            if (generator == null) return;
            System.Diagnostics.Debug.WriteLine($"Delete generator {generator.Name} command executed.");

            // need a message here to the user to confirm deletion of the generator, as this cannot be undone.
            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show($"Are you sure you want to delete the generator '{generator.Name}'? This action cannot be undone.", "Confirm Delete", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                // User confirmed deletion, proceed with deleting the generator from the parent track
                Track parentTrack = generator.Parent;
                List<Generator> generators = parentTrack.Generators;
                List<Generator> newGenerators = [];
                foreach (Generator theGenerator in generators)
                {
                    if (generator.Name != theGenerator.Name)
                        newGenerators.Add(theGenerator);
                }
                parentTrack.Generators = newGenerators;
                vm.IsDirty = true;
                vm.NotifyTrackChanged();
                vm.Status = [new Message { Text = $"Generator '{generator.Name}' deleted from track '{parentTrack.Name}'.", Error = false }];
            }
            else
            {
                // User canceled deletion, exit the method
                vm.Status = [new Message { Text = $"Deletion of generator '{generator.Name}' canceled.", Error = false }];
                return;
            }

        }
        public void Mute()
        {
            if (generator == null) return;
            // Toggle the mute state
            generator.Mute = !generator.Mute;

            // Find and update the generator in the parent track
            int index = generator.Parent.Generators.FindIndex((Generator g) => g.Name == generator.Name);
            if (index >= 0)
            {
                generator.Parent.Generators[index] = generator;
                // Update the ViewModel's generator reference
                vm.UpdateGenerator(generator);
            }

            vm.IsDirty = true;
            vm.Status = [new Message { Text = $"Generator '{generator.Name}' is now {((generator.Mute) ? "muted" : "unmuted")}.", Error = false }];
            vm.NotifyGeneratorChanged();
            vm.NotifyTrackChanged();
        }
        public void Edit()
        {
            if (generator == null) return;
            System.Diagnostics.Debug.WriteLine($"Edit generator on track {generator.Parent.Name} command executing.");
            vm.Mode = GeneratorEditMode.Modify;
            vm.NewGeneratorName = generator.Name;

            // Set the appropriate panel based on generator type
            string generatorType = generator.ToString();
            vm.GeneratorPanel = generatorType switch
            {
                "Algorithmic" => new Panels.Algorithmic.AlgorithmicPanel(),
                "Silent" => null, // Silent has no additional controls
                "Stochastic" => new Panels.Stochastic.StochasticPanel(),
                _ => null
            };

            vm.ActiveDialog = new GeneratorDialog
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            vm.ActiveDialog?.ShowDialog();
        }
        public void Move()
        {
            vm.MoveCopyMode = MoveCopyMode.Move;
            MoveCopy();
        }
        public void Copy()
        {
            vm.MoveCopyMode = MoveCopyMode.Copy;
            MoveCopy();
        }
        public void MoveCopy()
        {
            if (generator == null) return;
            System.Diagnostics.Debug.WriteLine($"Copy generator {generator.Name} command executed.");
            // present a dialog containing a list of track to copy the new generator to. This dialog will be used by both move and copy, so it need to accept a parameter to determine whether the operation is a copy or a move. The dialog will also need to accept the generator to be copied or moved, so it can be added to the selected track when the user clicks the OK button.
            vm.ActiveDialog = new MoveCopyGeneratorDialog
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SizeToContent = SizeToContent.WidthAndHeight,
            };
            vm.ActiveDialog.ShowDialog();
        }
        public void MoveCopyAction()
        {
            if (generator == null) return;
            Track? targetTrack = vm.SelectedTrack;
            if (targetTrack == null)
            {
                vm.Status = [new Message { Text = "No target track selected.", Error = true }];
                return;
            }

            Track sourceTrack = generator.Parent;
            string newGeneratorName = "G" + Uid.Get("generator", FileViewModel.Instance.File.Tracks).ToString();

            // Use TracksViewModel to handle the operation - it will handle status, dialog closing, and refresh
            TracksViewModel.Instance.MoveOrCopyGenerator(
                generator,
                sourceTrack,
                targetTrack,
                vm.MoveCopyMode,
                newGeneratorName
            );
        }
        // This command causes the PlayEngine to startup with the current generator
        public void Play()
        {
            PlayEngine.StartUp(generator);
        }
        #endregion
    }
}

    
