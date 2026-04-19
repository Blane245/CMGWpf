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
                        Generator newGenerator = vm.UIGenerator.Clone(generator.Parent);
                        newGenerator.Name = vm.NewGeneratorName;
                        //newGenerator.StartTime = vm.NewStartTime;
                        generator.Parent.Generators[index] = newGenerator;
                        vm.Messages.Add(new Message { Text = $"Generator '{generator.Name}' on track '{generator.Parent.Name}' updated successfully.", Error = false });
                        vm.IsDirty = true;
                    }

                }
                else if (vm.Mode == GeneratorEditMode.Add)
                {
                    Generator newGenerator = vm.UIGenerator.Clone(generator.Parent);
                    newGenerator.Name = vm.NewGeneratorName;
                    //newGenerator.StartTime = vm.NewStartTime;
                    generator.Parent.Generators.Add(newGenerator);
                    vm.IsDirty = true;
                    vm.Messages.Add(new Message { Text = $"Generator '{generator.Name}' on track '{generator.Parent.Name}' updated added.", Error = false });
                }
                else // this should not happen, but if it does, display an error
                {
                    vm.Messages.Add(new Message { Text = $"SYSTEM ERROR: Invalid generator edit mode '{vm.Mode}'.", Error = true });
                    return;
                }
                if (vm.ActiveGeneratorDialog is GeneratorDialog gd) gd.userCancel = false;
                vm.ActiveGeneratorDialog?.Close();
                vm.ActiveGeneratorDialog = null;
                vm.UIGenerator.Name = vm.NewGeneratorName;
                //vm.UIGenerator.StartTime = vm.NewStartTime;
                vm.UpdateGenerator(vm.UIGenerator);
                //vm.NotifyGeneratorChanged(nameof(vm.Generator));
                vm.NotifyTrackChanged();
            }

        }
        public void Delete()
        {
            if (generator == null) return;
            vm.Status = [];
            if (vm.ActiveGeneratorDialog != null)
            {
                _ = MessageBox.Show($"Generator '{generator.Name}' cannot be deleted while being edited.", "Generator Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete the generator '{generator.Name}'? This action cannot be undone.", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
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
        }
        public void Mute()
        {
            if (generator == null) return;
            vm.Status = [];
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
            vm.Status = [];
            if (vm.ActiveGeneratorDialog != null)
            {
                _ = MessageBox.Show($"Generator {vm.Generator.Name} is already being edited.'", "Duplicate Generator Edit", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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

            vm.ActiveGeneratorDialog = new GeneratorDialog()
            {
                DataContext = vm,
                SizeToContent = SizeToContent.WidthAndHeight,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            vm.ActiveGeneratorDialog.Show();
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
            vm.Messages = [];
            if (vm.ActiveGeneratorDialog != null)
            {
                _ = MessageBox.Show($"Cannot move or copy '{vm.Generator.Name}' while generator is being edited.", "Generator Move/Copy error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            vm.ActiveGeneratorDialog = new MoveCopyGeneratorDialog()
            {
                DataContext = vm,
                SizeToContent = SizeToContent.WidthAndHeight,
                Owner = CMGWpf.MainWindow.GetInstance(),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            vm.ActiveGeneratorDialog.ShowDialog();
        }
        public void MoveCopyAction()
        {
            if (generator == null) return;
            Track? targetTrack = vm.SelectedTrack;
            if (targetTrack == null)
            {
                _ = MessageBox.Show("No target track selected.", "Move/copy Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            vm.ActiveGeneratorDialog?.Close();
            vm.ActiveGeneratorDialog = null;
        }
        // This command causes the PlayEngine to startup with the current generator
        public void Play()
        {
            PlayEngine.StartUp(generator, true, false);
        }
        #endregion
    }
}

    
