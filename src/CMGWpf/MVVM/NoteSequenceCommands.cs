using CMGWpf.Helpers;
using CMGWpf.Model.Database;
using CMGWpf.Panels;
using CMGWpf.Panels.Database;
using CMGWpf.Types;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;

namespace CMGWpf.MVVM
{
    public class NoteSequenceCommands(NoteSequencesView vm)
    {
        private readonly NoteSequencesView vm = vm;

        public void AddNoteSequence()
        {
            vm.UINoteSequence = new NoteSequence();
            vm.NewNoteSequenceName = "";
            vm.EditorPanel = new NoteSequenceEditorPanel();
            vm.ModifyMode = "Add";
            vm.Errors.Clear();
        }
        public async void EditNoteSequence(string name)
        {
            var response = await NoteSequenceHelpers.Get(name);
            if (response == null)
            {
                Messages.Add(vm.Errors, "Note Sequence not found", true);
                return;
            }
            vm.UINoteSequence = response.Clone();
            vm.NewNoteSequenceName = name;
            vm.EditorPanel = new NoteSequenceEditorPanel();
            vm.ModifyMode = "Modify";
            vm.Errors.Clear();
        }
        public async void SubmitNoteSequence()
        {
            if (vm.UINoteSequence == null) return;
            // fill in the UINoteSequence with the current values for tags and validate the note sequence
            string[] newTagList = vm.NewTagListString.Split(',');
            // get the current list of tags from the database to validate against
            var tagList = await TagHelpers.List();
            var noteSequenceList = await NoteSequenceHelpers.List();
            vm.Errors = NoteSequence.Validate(vm.UINoteSequence, vm.NewNoteSequenceName, noteSequenceList, newTagList, tagList, vm.NewNoteItems);
            if (vm.Errors.Count > 0) return;

            // convert the newTagList into a list of tag objects and assign it to the UINoteSequence
            vm.UINoteSequence.Tags.Clear();
            foreach (var tagName in newTagList)
            {
                var tag = tagList.FirstOrDefault(t => t.Name == tagName.Trim());
                if (tag != null) vm.UINoteSequence.Tags.Add(tag);
            }

            // Convert the noteItems values note to midi values and then serialize the new note items list into a string and assign it to the UINoteSequence
            foreach (var noteItem in vm.NewNoteItems)
            {
                noteItem.NoteToMidi();
            }
            vm.UINoteSequence.Items = JsonSerializer.Serialize(vm.NewNoteItems);

            // update the note sequence in the database by either adding it to the list or replacing the existing one
            if (vm.ModifyMode == "Add")
            {
                vm.UINoteSequence.Name = vm.NewNoteSequenceName;
                var response = await NoteSequenceHelpers.Add(vm.UINoteSequence);
                if (!response)
                {
                    Messages.Add(vm.Errors, "Unknown error occurred while adding note sequence.", true);
                    vm.EditorPanel = new BlankPanel();
                    return;
                }
                vm.EditorPanel = new BlankPanel();
            }
            else
            {
                var response = await NoteSequenceHelpers.Modify(vm.UINoteSequence, vm.NewNoteSequenceName);
                if (!response)
                {
                    Messages.Add(vm.Errors, "Unknown error occurred while modifying note sequence.", true);
                    vm.EditorPanel = new BlankPanel();
                    return;
                }
            }
            vm.Errors = [new Message()
            {
                Text = "Note sequence '" + vm.UINoteSequence.Name + "' has been " + (vm.ModifyMode == "Add" ? "added" : "modified") + " successfully. " + (vm.UINoteSequence.Name != vm.NewNoteSequenceName && vm.ModifyMode == "Add" ? "" : "New Name is '" + vm.NewNoteSequenceName + "'"), Error = false }];
            vm.EditorPanel = new BlankPanel();
            // refresh the note sequence list
            ListNoteSequences();
        }
        public async void DeleteNoteSequence(string name)
        {
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete note sequence '{name}'?", "Confirm Note Sequence Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                // delete the note sequence from the DB
                var response = await NoteSequenceHelpers.Delete(name);
                if (!response)
                {
                    Messages.Add(vm.Errors, $"Note sequence '{name}' not found.", true);
                    return;
                }
                vm.Errors = [new Message()
                {
                    Text = $"Note sequence '{name}' has been deleted successfully.",
                    Error = false
                }];
                // refresh the note sequence list
                ListNoteSequences();
            }
        }
        public async void ListNoteSequences()
        {
            var response = await NoteSequenceHelpers.List();
            if (response == null)
            {
                Messages.Add(vm.Errors,"Failed to load note sequences.", true );
                return;
            }
            vm.NoteSequenceList = response;
            Messages.Add(vm.Errors, $"{vm.NoteSequenceList.Count} note sequences loaded.", false );
            vm.EditorPanel = new BlankPanel();
        }
    }
}
