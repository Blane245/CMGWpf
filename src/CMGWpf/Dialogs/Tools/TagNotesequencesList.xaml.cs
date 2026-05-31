using CMGWpf.Model.Database;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CMGWpf.Dialogs
{
    /// <summary>
    /// Interaction logic for TagNotesequencesList.xaml
    /// </summary>
    public partial class TagNotesequencesList : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private ObservableCollection<NoteSequence> _list = new ObservableCollection<NoteSequence>();
        public ObservableCollection<NoteSequence> List
        {
            get { return _list; }
            set { _list = value; OnPropertyChanged(); }
        }
        private string _listTitle = string.Empty;
        public string ListTitle
        {
            get { return _listTitle; }
            set { _listTitle = value; OnPropertyChanged(); }
        }
        public TagNotesequencesList(string tagName, ObservableCollection<NoteSequence> list)
        {
            InitializeComponent();
            DataContext = this;
            List = list;
            ListTitle = $"Note Sequence list for tag '{tagName}'";
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
