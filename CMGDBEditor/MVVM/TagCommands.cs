using CMGDBEditor.Dialogs;
using CMGDBEditor.Helpers;
using CMGDBEditor.Model;
using CMGDBEditor.View;
using CMGWpf.Types;
using System.Collections.ObjectModel;
using System.Windows;

namespace CMGDBEditor.MVVM
{
    public class TagCommands(NoteSequencesView vm)
    {
        private readonly NoteSequencesView vm = vm;
        public void AddModifyTag(string mode, Tag? tag)
        {
            if (mode == "Modify" && tag == null)
            {
                vm.Status = new Message() { Text = "No tag selected to modify.", Error = true };
                return;
            }
            if (mode == "Modify" && tag != null)
            {
                vm.NewTagName = tag.Name;
                vm.UITag = tag;
            } else
            {
                vm.UITag = new();
                vm.NewTagName = "";
            }
            vm.Errors = new ObservableCollection<Message>();
            vm.NotifyPropertyChanged(nameof(vm.Errors));
            vm.TagDialog = new TagDialog(vm, mode);
            vm.TagDialog.ShowDialog();
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
                    vm.Status = new Message() { Text = $"Tag '{name}' not found.", Error = true };
                    return;
                }
                vm.Status = new Message() { Text = $"Tag '{name}' has been deleted successfully.", Error = false };
                // refresh the tag list
                ListTags();
            }
        }
        public async void SubmitTag()
        {
            if (vm.UITag == null)
            {
                vm.Status = new Message() { Text = $"No tag selected to {vm.ModifyMode}.", Error = true };
                vm.TagDialog?.Close();
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
                    vm.Status = new Message() { Text = $"Failed to add tag '{vm.NewTagName}'.", Error = true };
                    vm.TagDialog?.Close();
                    return;
                }
                vm.Status = new Message() { Text = $"Tag '{vm.NewTagName}' has been added successfully.", Error = false };
                ListTags();
                vm.TagDialog?.Close();
            }
            else if (vm.ModifyMode == "Modify")
            {
                if (vm.UITag.Name == vm.NewTagName) {
                    vm.Status = new Message() { Text = $"Tag '{vm.UITag.Name}' has not changed.", Error = false };
                    vm.TagDialog?.Close();
                    return;
                }
                var response = await TagHelpers.Modify(vm.UITag, vm.NewTagName);
                if (!response)
                {
                    vm.Status = new Message() { Text = $"Failed to update tag '{vm.UITag.Name}'.", Error = true };
                    vm.TagDialog?.Close();
                    return;
                }
                vm.Status = new Message() { Text = $"Tag '{vm.UITag.Name}' has been updated to '{vm.NewTagName}' successfully.", Error = false };
                ListTags();
                vm.TagDialog?.Close();
            }
        }
        public async void ListTags()
        {
            var response = await TagHelpers.List();
            if (response == null)
            {
                vm.Status = new Message() { Text = "Failed to load tags.", Error = true };
                return;
            }
            vm.TagList = new ObservableCollection<Tag>(response);
            vm.NotifyPropertyChanged(nameof(vm.TagList));
        }
        public async void ListTagNoteSequences(string name)
        {
            var tag = await TagHelpers.Get(name);
            if (tag == null)
            {
                vm.Status = new Message() { Text = $"Tag '{name}' not found.", Error = true };
                return;
            }
            vm.UITag = tag;
            ObservableCollection<NoteSequence> list = new ObservableCollection<NoteSequence>(tag.NoteSequences);
            TagNotesequencesList dialog = new(vm, name, list);
            dialog.ShowDialog();
        }

    }
}
