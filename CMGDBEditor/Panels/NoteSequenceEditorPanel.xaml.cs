using CMGDBEditor.Model;
using CMGDBEditor.View;
using CMGWpf.Utilities;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace CMGDBEditor.Panels
{
    /// <summary>
    /// Interaction logic for NoteSequenceEditorPanel.xaml
    /// </summary>
    public partial class NoteSequenceEditorPanel : UserControl
    {
        private NoteSequencesView? _vm;
        public NoteSequenceEditorPanel(NoteSequencesView vm)
        {
            InitializeComponent();
            DataContext = vm;
            _vm = vm;
            Loaded += NoteSequenceEditorPanel_Loaded;
        }

        private void NoteSequenceEditorPanel_Loaded(object sender, RoutedEventArgs e)
        {
            // build the tag list from the note sequence tags
            if (DataContext is not NoteSequencesView vm || vm.UINoteSequence == null) return;
            vm.NewTagListString = string.Join(",", vm.UINoteSequence.Tags.Select(t => t.Name).ToList());
            // extract the note items from the note sequence and build the note item list
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var noteItems = (vm.UINoteSequence.Items != string.Empty) ? JsonSerializer.Deserialize<ObservableCollection<NoteItem>>(vm.UINoteSequence.Items, options) : new ObservableCollection<NoteItem>();
            // derive the note names from the midi values
            if (noteItems != null)
                foreach (var noteItem in noteItems)
                {
                    noteItem.Note = SoundFontUtilities.MidiToNote(noteItem.Value);
                }
            vm.NewNoteItems = noteItems ?? new ObservableCollection<NoteItem>();
        }
    }
}
