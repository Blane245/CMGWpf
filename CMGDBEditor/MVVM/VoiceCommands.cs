using CMGDBEditor.Dialogs;
using CMGDBEditor.Model;
using CMGDBEditor.Panels;
using CMGDBEditor.Types;
using CMGDBEditor.View;
using CMGWpf.Model.Generators;
using CMGWpf.Types;
using System.Collections.ObjectModel;
using System.Windows;
using System.Xml.Linq;

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
        }
        public async void EditVoice(string name)
        {
            var response = await Helpers.VoiceHelpers.Get(name);
            if (response == null)
            {
                MainView.Instance.Messages = [.. MainView.Instance.Messages, new Message() { Error = true, Text = "Voice not found." }];
                return;
            }
            vm.UIVoice = response.Clone();
            vm.NewSoundFontFile = vm.UIVoice.SoundFontFile;
            vm.NewVoiceName = name;
            vm.EditorPanel = new VoiceEditorPanel(vm);
            vm.ModifyMode = "Modify";
            vm.NotifyPropertyChanged(nameof(vm.EditorPanel));
        }
        public async void SubmitVoice()
        {
            if (vm.UIVoice == null) return;
            vm.UIVoice.SoundFontFile = vm.NewSoundFontFile;
            ObservableCollection<Error> errors = Voice.Validate(vm.UIVoice, vm.NewVoiceName, vm.VoiceList);
            if (errors.Count > 0)
            {
                vm.Errors = errors;
                return;
            }

            // update the voice in the database by either adding it to the list or replacing the existing one
            if (vm.ModifyMode == "Add")
            {
                vm.UIVoice.Name = vm.NewVoiceName;
                var response = await Helpers.VoiceHelpers.Add(vm.UIVoice);
                if (!response)
                {
                    MainView.Instance.Messages = [new Message() { Error = true, Text = "Unknown error occurred while adding voice." }];
                    return;
                }
                vm.EditorPanel = new BlankPanel();
                vm.NotifyPropertyChanged(nameof(vm.EditorPanel));
            }
            else
            {
                var response = await Helpers.VoiceHelpers.Modify(vm.UIVoice, vm.NewVoiceName);
                if (!response)
                {
                    MainView.Instance.Messages = [.. MainView.Instance.Messages, new Message() { Error = true, Text = "Unknown error occurred while modifying voice." }];
                    vm.EditorPanel = new BlankPanel();
                    vm.NotifyPropertyChanged(nameof(vm.EditorPanel));
                    return;
                }
            }

            MainView.Instance.Messages = [.. MainView.Instance.Messages, new Message() { Error = false, Text = "Voice " + vm.UIVoice.Name + " has been " + (vm.ModifyMode == "Add" ? "added" : "modified") + " successfully. " + (vm.UIVoice.Name != vm.NewVoiceName && vm.ModifyMode == "Add" ? "" : "New Name is '" + vm.NewVoiceName + "'") }];
            // refresh the voice list
            ListVoices();
        }
        public async void DeleteVoice(string name)
        {
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete voice {name}?", "Confirm Voice Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                // delete the voice from the DB
                var response = await Helpers.VoiceHelpers.Delete(name);
                if (!response)
                {
                    MainView.Instance.Messages = [.. MainView.Instance.Messages, new Message() { Error = true, Text = $"Voice '{name}' not found." }];
                    return;
                }
                MainView.Instance.Messages = [.. MainView.Instance.Messages, new Message() { Error = false, Text = $"Voice '{name}' has been deleted successfully." }];
                // refresh the voice list
                ListVoices();
            }
        }
        public async void ListVoiceEnsembles(string name)
        {
            var voice = await Helpers.VoiceHelpers.Get(name);
            if (voice == null)
            {
                MainView.Instance.Messages = [.. MainView.Instance.Messages, new Message() { Error = true, Text = $"Voice '{name}' not found." }];
                return;
            }
            vm.UIVoice = voice;
            ObservableCollection<EnsembleView.VoiceEnsemblesListType> list = [];
            foreach (var ensemble in voice.Ensembles)
            {
                list.Add(new() { Name = ensemble.Name, Description = ensemble.Description });
            }
            vm.VoiceEnsemblesList = list;
            VoiceEnsemblesList dialog = new(vm);
            dialog.ShowDialog();
        }
        public async void ListVoices()
        {
            var response = await Helpers.VoiceHelpers.List();
            if (response == null)
            {
                MainView.Instance.Messages = [.. MainView.Instance.Messages, new Message() { Error = true, Text = "Failed to load voices." }];
                return;
            }
            vm.VoiceList = response;
            MainView.Instance.Messages = [.. MainView.Instance.Messages, new Message() { Error = false, Text = $"{vm.VoiceList.Count} voices loaded." }];
            vm.EditorPanel = new BlankPanel();
            vm.NotifyPropertyChanged(nameof(vm.VoiceList));
            vm.NotifyPropertyChanged(nameof(vm.EditorPanel));
        }
    }
}
