using CMGWpf.Data;
using CMGWpf.Dialogs;
using CMGWpf.Helpers;
using CMGWpf.Model.Database;
using CMGWpf.Panels.Tools;
using CMGWpf.Types;
using CMGWpf.Utilities;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Windows;

namespace CMGWpf.MVVM
{
    public class ChordSequenceCommands(ChordSequenceView vm)
    {
        private readonly ChordSequenceView vm = vm;
        private readonly CmgDbContext? dbContext = new CmgDbContext();

        public void AddChordSequence()
        {
            vm.UIChordSequence = new ChordSequence();
            vm.NewName = "";
            vm.EditorPanel = new ChordSequenceEditorPanel();
            vm.ModifyMode = "Add";
            vm.Errors.Clear();
        }
        public async void EditChordSequence(string name)
        {
            var response = await ChordSequenceHelpers.Get(name);
            if (response == null)
            {
                Messages.Add(vm.Errors, "Note Sequence not found", true);
                return;
            }
            vm.UIChordSequence = response.Clone();
            // update the PresetNames
            //vm.SoundFont = SoundFontUtilities.GetSoundFont(vm.ChordSequencerDialog.SoundFontFileName);
            //vm.PresetNames = (vm.SoundFont != null) ? vm.PresetNames = new ObservableCollection<string>(vm.SoundFont.Presets.Select(preset => SoundFontUtilities.BankPresetToName(preset)).OrderBy(name => name)) : vm.PresetNames = [];
            vm.NewName = name;
            //vm.SoundFontFileName = vm.UIChordSequence.SoundFontFileName;
            //vm.PresetName = vm.UIChordSequence.PresetName?? "";
            vm.EditorPanel = new ChordSequenceEditorPanel();
            vm.IsMajor = vm.UIChordSequence.IsMajor;
            vm.ModifyMode = "Modify";
            vm.AllowedChords = GetAllowedChords(vm.UIChordSequence, vm.IsMajor);
            vm.Errors.Clear();
        }
        private ObservableCollection<string> GetAllowedChords(ChordSequence chordSequence, bool isMajor)
        {
            int chordCount = chordSequence.Items.Count;
            if (chordCount == 0)
            {
                return isMajor ? new ObservableCollection<string>(Music.MajorProgressionStateTransitions["I"]) : new ObservableCollection<string>(Music.MinorProgressionStateTransitions["i"]);
            }
            else
            {
                string lastChordValue = chordSequence.Items[chordSequence.Items.Count - 1].ChordValue;
                if (isMajor)
                {
                    if (Music.MajorProgressionStateTransitions.ContainsKey(lastChordValue))
                    {
                        return new ObservableCollection<string>(Music.MajorProgressionStateTransitions[lastChordValue]);
                    }
                    else
                    {
                        return new ObservableCollection<string>(Music.MajorProgressionStateTransitions["I"]);
                    }
                }
                else
                {
                    if (Music.MinorProgressionStateTransitions.ContainsKey(lastChordValue))
                    {
                        return new ObservableCollection<string>(Music.MinorProgressionStateTransitions[lastChordValue]);
                    }
                    else
                    {
                        return new ObservableCollection<string>(Music.MinorProgressionStateTransitions["i"]);
                    }
                }
            }
        }
        public async void SubmitChordSequence()
        {
            if (vm.UIChordSequence == null || dbContext == null) return;
            vm.UIChordSequence.IsMajor = vm.IsMajor;
            vm.Errors = await ChordSequence.Validate(vm.UIChordSequence, vm.NewName, dbContext);
            if (vm.Errors.Count > 0) return;

            // update the chord sequence in the database by either adding it to the list or replacing the existing one
            if (vm.ModifyMode == "Add")
            {
                vm.UIChordSequence.Name = vm.NewName;
                var response = await ChordSequenceHelpers.Add(vm.UIChordSequence);
                if (!response)
                {
                    Messages.Add(vm.Errors, "Unknown error occurred while adding chord sequence.", true);
                    vm.EditorPanel = new BlankPanel();
                    ListChordSequences();
                    return;
                }
                vm.EditorPanel = new BlankPanel();
            }
            else
            {
                var response = await ChordSequenceHelpers.Modify(vm.UIChordSequence, vm.NewName);
                if (!response)
                {
                    Messages.Add(vm.Errors, "Unknown error occurred while modifying chord sequence.", true);
                    vm.EditorPanel = new BlankPanel();
                    ListChordSequences();
                    return;
                }
            }
            vm.Errors = [new Message()
            {
                Text = "Chord sequence '" + vm.UIChordSequence.Name + "' has been " + (vm.ModifyMode == "Add" ? "added" : "modified") + " successfully. " + (vm.UIChordSequence.Name != vm.NewName && vm.ModifyMode == "Add" ? "" : "New Name is '" + vm.NewName + "'"), Error = false }];
            vm.EditorPanel = new BlankPanel();
            // refresh the note sequence list
            ListChordSequences();
        }
        public async void DeleteChordSequence(string name)
        {
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete note sequence '{name}'?", "Confirm Note Sequence Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                // delete the note sequence from the DB
                var response = await ChordSequenceHelpers.Delete(name);
                if (!response)
                {
                    Messages.Add(vm.Errors, $"Note sequence '{name}' not found.", true);
                    ListChordSequences();
                    return;
                }
                vm.Errors = [new Message()
                {
                    Text = $"Note sequence '{name}' has been deleted successfully.",
                    Error = false
                }];
                // refresh the note sequence list
                ListChordSequences();
            }
        }
        public async void ListChordSequences()
        {
            var response = await ChordSequenceHelpers.List();
            if (response == null)
            {
                Messages.Add(vm.Errors, "Failed to load note sequences.", true);
                return;
            }
            vm.ChordSequenceList = response;
            Messages.Add(vm.Errors, $"{vm.ChordSequenceList.Count} note sequences loaded.", false);
            vm.EditorPanel = new BlankPanel();
        }
        public void AddChord()
        {
            vm.AllowedChords = GetAllowedChords(vm.UIChordSequence, vm.IsMajor);
            vm.NewChordValue = null;
            vm.NewChordValueDialog = new ChordValueDialog()
            {
                DataContext = vm,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            vm.NewChordValueDialog.ShowDialog();
            if (vm.NewChordValue == null) return;

            ChordItem newChord = new ChordItem()
            {
                ChordValue = vm.NewChordValue,
                Inversion = vm.DefaultInversion,
                Duration = vm.DefaultDuration,
                Effort = { Weight = vm.DefaultWeight, Articulation = vm.DefaultArticulation, ChordBinding = vm.DefaultBinding, Space = vm.DefaultSpace },
            };
            vm.UIChordSequence.Items.Add(newChord);
            vm.IsMajorEnabled = false;

        }
    }

}
