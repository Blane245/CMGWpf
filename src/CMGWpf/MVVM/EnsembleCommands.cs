using CMGWpf.Helpers;
using CMGWpf.Model.Database;
using CMGWpf.Panels;
using CMGWpf.View;
using CMGWpf.Types;
using System.Windows;
using CMGWpf.Panels.Database;

namespace CMGWpf.MVVM 
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
            vm.Errors = [];
        }
        public async void EditEnsemble(string name)
        {
            var response = await EnsembleHelpers.Get(name);
            if (response == null)
            {
                Messages.Add(vm.Errors, $"Error while loading ensemble '{name}'.", true );
                return;        
            }
            vm.UIEnsemble = response.Clone();
            vm.NewEnsembleName = name;
            vm.EditorPanel = new EnsembleEditorPanel(vm);
            vm.ModifyMode = "Modify";
            vm.Errors = [];
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
                    Messages.Add(vm.Errors, "Unknown error occurred while adding ensemble.", true );
                    vm.EditorPanel = new BlankPanel();
                    return;
                }
            }
            else
            {
                var response = await EnsembleHelpers.Modify(vm.UIEnsemble, vm.NewEnsembleName);
                if (!response)
                {
                    Messages.Add(vm.Errors, "Unknown error occurred while modifying ensemble.",  true );
                    vm.EditorPanel = new BlankPanel();
                }
            }

            vm.Errors = [new Message() { Text = "Ensemble '" + vm.UIEnsemble.Name + "' has been " + (vm.ModifyMode == "Add" ? "added" : "modified") + " successfully. " + (vm.UIEnsemble.Name != vm.NewEnsembleName && vm.ModifyMode == "Add" ? "" : "New Name is '" + vm.NewEnsembleName + "'"), Error = false }];
            vm.EditorPanel = new BlankPanel();
            ListEnsembles();
        }
        public async void DeleteEnsemble(string name)
        {
            vm.Errors = [];
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete ensemble '{name}'?", "Confirm Ensemble Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                // delete the ensemble from the DB
                var response = await EnsembleHelpers.Delete(name);
                if (!response)
                {
                    vm.Errors = [new Message() { Text = "Error while deleting ensemble.", Error = true }];
                    return;
                }
                vm.Errors = [new Message() { Text = $"Ensemble '{name}' has been deleted successfully.", Error = false }];
                ListEnsembles();
            }

        }
        public async void ListEnsembles()
        {
            var response = await EnsembleHelpers.List();
            if (response == null)
            {
                vm.Errors = [new Message() { Text = "Error while loading ensembles.", Error = true }];
                return;
            }
            vm.EnsembleList = response;
            Messages.Add(vm.Errors, $"{vm.EnsembleList.Count} ensembles loaded.", false);
            vm.EditorPanel = new BlankPanel();
            return;
        }
    }
}
