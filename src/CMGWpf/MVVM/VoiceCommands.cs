using CMGWpf.Dialogs;
using CMGWpf.Helpers;
using CMGWpf.Model.Database;
using CMGWpf.Panels;
using CMGWpf.Panels.Database;
using CMGWpf.Types;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Windows;

namespace CMGWpf.MVVM
{
    public class VoiceCommands(EnsembleView vm)
    {
        private readonly EnsembleView vm = vm;

        public void AddVoice()
        {
            vm.UIVoice = new Voice();
            vm.NewVoiceName = "";
            vm.EditorPanel = new VoiceEditorPanel(vm);
            vm.ModifyMode = "Add";
            vm.NotifyPropertyChanged(nameof(vm.EditorPanel));
            vm.Errors = [];
        }
        public async void EditVoice(string name)
        {
            vm.Errors.Clear();
            var response = await VoiceHelpers.Get(name);
            if (response == null)
            {
                vm.Errors = [new Message() { Text = $"Voice '{name}' not found.", Error = true }];
                return;
            }
            vm.UIVoice = response.Clone();
            vm.NewSoundFontFile = vm.UIVoice.SoundFontFile;
            vm.NewVoiceName = name;
            vm.EditorPanel = new VoiceEditorPanel(vm);
            vm.ModifyMode = "Modify";
            vm.NotifyPropertyChanged(nameof(vm.EditorPanel));
            vm.Errors.Clear ();
        }
        public async void SubmitVoice()
        {
            if (vm.UIVoice == null) return;
            vm.UIVoice.SoundFontFile = vm.NewSoundFontFile;
            vm.Errors = Voice.Validate(vm.UIVoice, vm.NewVoiceName, vm.VoiceList);
            if (vm.Errors.Count > 0) return;

            // update the voice in the database by either adding it to the list or replacing the existing one
            if (vm.ModifyMode == "Add")
            {
                vm.UIVoice.Name = vm.NewVoiceName;
                var response = await VoiceHelpers.Add(vm.UIVoice);
                if (!response)
                {
                    vm.Errors = [new Message() { Text = "Unknown error occurred while adding voice.", Error = true }];
                    return;
                }
                vm.EditorPanel = new BlankPanel();
                vm.NotifyPropertyChanged(nameof(vm.EditorPanel));
            }
            else
            {
                var response = await VoiceHelpers.Modify(vm.UIVoice, vm.NewVoiceName);
                if (!response)
                {
                    vm.Errors = [new Message() { Text = "Unknown error occurred while modifying voice.", Error = true }];
                    vm.EditorPanel = new BlankPanel();
                    return;
                }
            }

            vm.Errors = [new Message() { Text = "Voice " + vm.UIVoice.Name + " has been " + (vm.ModifyMode == "Add" ? "added" : "modified") + " successfully. " + (vm.UIVoice.Name != vm.NewVoiceName && vm.ModifyMode == "Add" ? "" : "New Name is '" + vm.NewVoiceName + "'"), Error = false }];
            // refresh the voice list
            vm.EditorPanel = new BlankPanel();
            ListVoices();
        }
        public async void DeleteVoice(string name)
        {
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete voice {name}?", "Confirm Voice Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                // delete the voice from the DB
                var response = await VoiceHelpers.Delete(name);
                if (!response)
                {
                    vm.Errors = [new Message() { Text = $"Voice '{name}' not found.", Error = true }];
                    return;
                }
                vm.Errors = [new Message() { Text = $"Voice '{name}' has been deleted successfully.", Error = false }];
                // refresh the voice list
                ListVoices();
            }
        }
        public async void ListVoiceEnsembles(string name)
        {
            var voice = await VoiceHelpers.Get(name);
            if (voice == null)
            {
                vm.Errors = [new Message() { Text = $"Voice '{name}' not found.", Error = true }];
                return;
            }
            vm.UIVoice = voice;
            ObservableCollection<EnsembleView.VoiceEnsemblesListType> list = [];
            foreach (var ensemble in voice.Ensembles)
            {
                list.Add(new() { Name = ensemble.Name, Description = ensemble.Description });
            }
            vm.VoiceEnsemblesList = list;
            VoiceEnsemblesList dialog = new(name, list);
            dialog.ShowDialog();
            vm.Errors = [];
        }
        public async void ListVoices()
        {
            var response = await VoiceHelpers.List();
            if (response == null)
            {
                Messages.Add(vm.Errors, "Failed to load voices.", true );
                return;
            }
            vm.VoiceList = response;
            Messages.Add(vm.Errors, $"{vm.VoiceList.Count} voices loaded.", false );
            vm.EditorPanel = new BlankPanel();
        }
    }
}
