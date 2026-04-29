using CMGDBEditor.Model;
using CMGDBEditor.Panels;
using CMGDBEditor.Types;
using CMGDBEditor.View;
using CMGWpf.Types;
using System.Collections.ObjectModel;
using System.Windows;

namespace CMGDBEditor.MVVM
{
    public class EnsembleCommands(EnsembleView vm)
    {
        private readonly EnsembleView vm = vm;

        public void AddEnsemble()
        {
            vm.UIEnsemble = new Ensemble();
            vm.NewEnsembleName = "";
            vm.EditorPanel = new EnsembleEditorPanel(vm);
            vm.ModifyMode = "Add";
            vm.NotifyPropertyChanged(nameof(vm.EditorPanel));
        }
        public async void EditEnsemble(string name)
        {
            var response = await Helpers.EnsembleHelpers.Get(name);
            if (response == null)
            {
                MainView.Instance.Messages = [.. MainView.Instance.Messages, new Message() { Error = true, Text = "Ensemble not found." }];
                return;
            }
            vm.UIEnsemble = response.Clone();
            vm.NewEnsembleName = name;
            vm.EditorPanel = new EnsembleEditorPanel(vm);
            vm.ModifyMode = "Modify";
            vm.NotifyPropertyChanged(nameof(vm.EditorPanel));
        }
        public async void SubmitEnsemble()
        {
            if (vm.UIEnsemble == null) return;
            ObservableCollection<Error> errors = Ensemble.Validate(vm.UIEnsemble, vm.NewEnsembleName, vm.EnsembleList);
            if (errors.Count > 0)
            {
                vm.Errors = errors;
                return;
            }

            // retrieve the selectableVoiceList and update the UIEnsemble voices
            vm.UIEnsemble.Voices.Clear();
            foreach (var voice in vm.SelectableVoiceList)
            {
                if (voice.IsVoiceSelected) vm.UIEnsemble.Voices.Add(voice.Voice!);
            }
            // update the ensemble in the database by either adding it to the list or replacing the existing one
            if (vm.ModifyMode == "Add")
            {
                vm.UIEnsemble.Name = vm.NewEnsembleName;
                var response = await Helpers.EnsembleHelpers.Add(vm.UIEnsemble);
                if (!response)
                {
                    MainView.Instance.Messages = [.. MainView.Instance.Messages, new Message() { Error = true, Text = "Unknown error occurred while adding ensemble." }];
                    vm.EditorPanel = new BlankPanel();
                    vm.NotifyPropertyChanged(nameof(vm.EditorPanel));
                    return;
                }
                MainView.Instance.Messages = [.. MainView.Instance.Messages, new Message() { Error = false, Text = "Ensemble '" + vm.UIEnsemble.Name + "' has been added successfully." }];
            }
            else
            {
                var response = await Helpers.EnsembleHelpers.Modify(vm.UIEnsemble, vm.NewEnsembleName);
                if (!response)
                {
                    MainView.Instance.Messages = [.. MainView.Instance.Messages, new Message() { Error = true, Text = "Unknown error occurred while modifying ensemble." }];
                    vm.EditorPanel = new BlankPanel();
                    vm.NotifyPropertyChanged(nameof(vm.EditorPanel));
                    return;
                }
            }

            MainView.Instance.Messages = [.. MainView.Instance.Messages, new Message() { Error = false, Text =
                "Ensemble '" + vm.UIEnsemble.Name + "' has been " + (vm.ModifyMode == "Add" ? "added" : "modified") + " successfully. " + (vm.UIEnsemble.Name != vm.NewEnsembleName && vm.ModifyMode == "Add"? "": "New Name is '" + vm.NewEnsembleName + "'") }];
            // refresh the ensemble list
            ListEnsembles();
        }
        public async void DeleteEnsemble(string name)
        {
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete ensemble '{name}'?", "Confirm Ensemble Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                // delete the ensemble from the DB
                var response = await Helpers.EnsembleHelpers.Delete(name);
                if (!response)
                {
                    MainView.Instance.Messages = [.. MainView.Instance.Messages, new Message() { Error = true, Text = $"Ensemble '{name}' not found." }];
                    return;
                }
                MainView.Instance.Messages = [.. MainView.Instance.Messages, new Message() { Error = false, Text = $"Ensemble '{name}' has been deleted successfully." }];
                // refresh the ensemble list
                ListEnsembles();
            }

        }
        public async void ListEnsembles()
        {
            var response = await Helpers.EnsembleHelpers.List();
            if (response == null)
            {
                MainView.Instance.Messages = [.. MainView.Instance.Messages, new Message() { Error = true, Text = "Failed to load ensembles." }];
                return;
            }
            vm.EnsembleList = response;
            MainView.Instance.Messages = [.. MainView.Instance.Messages, new Message() { Error = false, Text = $"{vm.EnsembleList.Count} ensembles loaded." }];
            vm.EditorPanel = new BlankPanel();
            vm.NotifyPropertyChanged(nameof(vm.EnsembleList));
            vm.NotifyPropertyChanged(nameof(vm.EditorPanel));
        }
    }
}
