using CMGDBEditor.Helpers;
using CMGDBEditor.Model;
using CMGDBEditor.Panels;
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
            vm.Errors = new ObservableCollection<Message>();
        }
        public async void EditEnsemble(string name)
        {
            var response = await EnsembleHelpers.Get(name);
            if (response == null)
            {
                vm.Status = new Message() { Text = "Ensemble not found.", Error = true };
                return;        
            }
            vm.UIEnsemble = response.Clone();
            vm.NewEnsembleName = name;
            vm.EditorPanel = new EnsembleEditorPanel(vm);
            vm.ModifyMode = "Modify";
            vm.Errors = new ObservableCollection<Message>();
        }
        public async void SubmitEnsemble()
        {
            if (vm.UIEnsemble == null) return;
            vm.Errors = Ensemble.Validate(vm.UIEnsemble, vm.NewEnsembleName, vm.EnsembleList);
            if (vm.Errors.Count > 0) return;

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
                var response = await EnsembleHelpers.Add(vm.UIEnsemble);
                if (!response)
                {
                    vm.Status = new Message() { Text = "Unknown error occurred while adding ensemble.", Error = true };
                    vm.EditorPanel = new BlankPanel();

                    return;
                }
            }
            else
            {
                var response = await EnsembleHelpers.Modify(vm.UIEnsemble, vm.NewEnsembleName);
                if (!response)
                {
                    vm.Status = new Message() { Text = "Unknown error occurred while modifying ensemble.", Error = true };
                    vm.EditorPanel = new BlankPanel();
                }
            }

            vm.Status = new Message() { Text = "Ensemble '" + 
                vm.UIEnsemble.Name + 
                "' has been " + 
                (vm.ModifyMode == "Add" ? "added" : "modified") + 
                " successfully. " + 
                (vm.UIEnsemble.Name != vm.NewEnsembleName && vm.ModifyMode == "Add" ? "" : "New Name is '" + vm.NewEnsembleName + "'"), 
                Error = false };
            // refresh the ensemble list
            vm.EditorPanel = new BlankPanel();
            ListEnsembles();
        }
        public async void DeleteEnsemble(string name)
        {
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete ensemble '{name}'?", "Confirm Ensemble Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                // delete the ensemble from the DB
                var response = await EnsembleHelpers.Delete(name);
                if (!response)
                {
                    vm.Status = new Message() { Text = "Ensemble not found.", Error = true };
                    return;
                }
                vm.Status = new Message() { Text = $"Ensemble '{name}' has been deleted successfully.", Error = false };
                ListEnsembles();
            }

        }
        public async void ListEnsembles()
        {
            var response = await EnsembleHelpers.List();
            if (response == null)
            {
                vm.Status = new Message() { Text = "Failed to load ensembles.", Error = true };
                return;
            }
            vm.EnsembleList = response;
            vm.Status = new Message() { Text = $"{vm.EnsembleList.Count} ensembles loaded.", Error = false };
            return;
        }
    }
}
