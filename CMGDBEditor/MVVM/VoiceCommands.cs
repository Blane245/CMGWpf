using CMGDBEditor.Dialogs;
using CMGDBEditor.Helpers;
using CMGDBEditor.Model;
using CMGDBEditor.Panels;
using CMGDBEditor.Types;
using CMGDBEditor.View;
using CMGWpf.Types;
using System.Collections.ObjectModel;
using System.Windows;

namespace CMGDBEditor.MVVM
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
            vm.Errors = new ObservableCollection<Message>();
        }
        public async void EditVoice(string name)
        {
            var response = await Helpers.VoiceHelpers.Get(name);
            if (response == null)
            {
                vm.Status = new Message() { Text = $"Voice '{name}' not found.", Error = true };
                return;
            }
            vm.UIVoice = response.Clone();
            vm.NewSoundFontFile = vm.UIVoice.SoundFontFile;
            vm.NewVoiceName = name;
            vm.EditorPanel = new VoiceEditorPanel(vm);
            vm.ModifyMode = "Modify";
            vm.NotifyPropertyChanged(nameof(vm.EditorPanel));
            vm.Errors = new ObservableCollection<Message>();
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
                    vm.Status = new Message() { Text = "Unknown error occurred while adding voice.", Error = true };
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
                    vm.Status = new Message() { Text = "Unknown error occurred while modifying voice.", Error = true };
                    vm.EditorPanel = new BlankPanel();
                    return;
                }
            }

            vm.Status = new Message() { Text = "Voice " + vm.UIVoice.Name + " has been " + (vm.ModifyMode == "Add" ? "added" : "modified") + " successfully. " + (vm.UIVoice.Name != vm.NewVoiceName && vm.ModifyMode == "Add" ? "" : "New Name is '" + vm.NewVoiceName + "'"), Error = false };
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
                    vm.Status = new Message() { Text = $"Voice '{name}' not found.", Error = true };
                    return;
                }
                vm.Status = new Message() { Text = $"Voice '{name}' has been deleted successfully.", Error = false };
                // refresh the voice list
                ListVoices();
            }
        }
        public async void ListVoiceEnsembles(string name)
        {
            var voice = await VoiceHelpers.Get(name);
            if (voice == null)
            {
                vm.Status = new Message() { Text = $"Voice '{name}' not found.", Error = true };
                return;
            }
            vm.UIVoice = voice;
            ObservableCollection<EnsembleView.VoiceEnsemblesListType> list = [];
            foreach (var ensemble in voice.Ensembles)
            {
                list.Add(new() { Name = ensemble.Name, Description = ensemble.Description });
            }
            vm.VoiceEnsemblesList = list;
            VoiceEnsemblesList dialog = new(vm, name, list);
            dialog.ShowDialog();
        }
        public async void ListVoices()
        {
            var response = await VoiceHelpers.List();
            if (response == null)
            {
                vm.Status = new Message() { Text = "Failed to load voices.", Error = true };
                return;
            }
            vm.VoiceList = response;
            vm.Status = new Message() { Text = $"{vm.VoiceList.Count} voices loaded.", Error = false };
        }
    }
}
