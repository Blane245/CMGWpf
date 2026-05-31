using CMGWpf.Dialogs;
using CMGWpf.Helpers;
using CMGWpf.Model;
using CMGWpf.Model.Database;
using CMGWpf.Panels;
using CMGWpf.Panels.Database;
using CMGWpf.Panels.Tools;
using CMGWpf.Types;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Windows;

namespace CMGWpf.MVVM
{
    public class TagCommands(NoteSequencesView vm)
    {
        private readonly NoteSequencesView vm = vm;
        public void AddModifyTag(string mode, Tag? tag = null)
        {
            vm.Errors.Clear();
            if (mode == "Modify" && tag == null)
            {
                Messages.Add(vm.Errors, "No tag selected to modify.", true);
                return;
            }
            if (mode == "Modify" && tag != null)
            {
                vm.NewTagName = tag.Name;
                vm.UITag = tag;
            }
            else
            {
                vm.UITag = new();
                vm.NewTagName = "";
            }
            vm.EditorPanel = new TagEditorPanel();
            vm.Errors.Clear();
        }
        public async void DeleteTag(string name)
        {
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete tag {name}?", "Confirm Tag Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                // delete the tag from the DB
                var response = await TagHelpers.Delete(name);
                if (!response)
                {
                    vm.Errors = [new Message() { Text = $"Tag '{name}' not found.", Error = true }];
                    return;
                }
                vm.Errors = [new Message() { Text = $"Tag '{name}' has been deleted successfully.", Error = false }];
                // refresh the tag list
                ListTags();
            }
        }
        public async void SubmitTag()
        {
            if (vm.UITag == null)
            {
                vm.Errors = [new Message() { Text = $"No tag selected to {vm.ModifyMode}.", Error = true }];
                return;
            }
            ObservableCollection<Message> errors = Tag.Validate(vm.UITag, vm.NewTagName, vm.TagList);
            if (errors.Count > 0) return;
            if (vm.ModifyMode == "Add")
            {
                vm.UITag.Name = vm.NewTagName;
                var response = await TagHelpers.Add(vm.UITag);
                if (!response)
                {
                    vm.Errors = [new Message() { Text = $"Failed to add tag '{vm.NewTagName}'.", Error = true }];
                    return;
                }
                vm.Errors = [new Message() { Text = $"Tag '{vm.NewTagName}' has been added successfully.", Error = false }];
                ListTags();
            }
            else if (vm.ModifyMode == "Modify")
            {
                if (vm.UITag.Name == vm.NewTagName) {
                    vm.Errors = [new Message() { Text = $"Tag '{vm.UITag.Name}' has not changed.", Error = false }];
                    return;
                }
                var response = await TagHelpers.Modify(vm.UITag, vm.NewTagName);
                if (!response)
                {
                    vm.Errors = [new Message() { Text = $"Failed to update tag '{vm.UITag.Name}'.", Error = true }];
                    return;
                }
                vm.Errors = [new Message() { Text = $"Tag '{vm.UITag.Name}' has been updated to '{vm.NewTagName}' successfully.", Error = false }];
                ListTags();
            }
        }
        public async void ListTags()
        {
            var response = await TagHelpers.List();
            if (response == null)
            {
                vm.Errors = [new Message() { Text = "Failed to load tags.", Error = true }];
                return;
            }
            vm.TagList = new ObservableCollection<Tag>(response);
            vm.EditorPanel = new BlankPanel();
        }
        public async void ListTagNoteSequences(string name)
        {
            var tag = await TagHelpers.Get(name);
            if (tag == null)
            {
                vm.Errors = [new Message() { Text = $"Tag '{name}' not found.", Error = true }];
                return;
            }
            vm.UITag = tag;
            ObservableCollection<NoteSequence> list = new ObservableCollection<NoteSequence>(tag.NoteSequences);
            TagNotesequencesList dialog = new(tag.Name, list);
            dialog.ShowDialog();
        }
    }
}
