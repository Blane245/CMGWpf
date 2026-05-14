using CMGDBEditor.Model;
using CMGDBEditor.View;
using System.Collections.ObjectModel;
using System.Windows;

namespace CMGDBEditor.Dialogs
{
    /// <summary>
    /// Interaction logic for TagNotesequencesList.xaml
    /// </summary>
    public partial class TagNotesequencesList : Window
    {
        public ObservableCollection<NoteSequence> List { get; set; } = new ObservableCollection<NoteSequence>();
        public string ListTitle { get; set; } = string.Empty;
        public NoteSequencesView? CommandView { get; set; } = null;
        public TagNotesequencesList(NoteSequencesView vm, string name, ObservableCollection<NoteSequence> list)
        {
            InitializeComponent();
            DataContext = this;
            List = list;
            ListTitle = $"Note Sequence list for tag '{name}'";
            CommandView = vm;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
