using CMGDBEditor.View;
using System.Windows;

namespace CMGDBEditor.Dialogs
{
    /// <summary>
    /// Interaction logic for TagDialog.xaml
    /// </summary>
    public partial class TagDialog : Window
    {
        private string Mode = "";
        public TagDialog(NoteSequencesView vm, string mode)
        {
            Mode = mode;
            InitializeComponent();
            DataContext = vm;
            Loaded += TagDialog_Loaded;
        }

        private void TagDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Title = (Mode == "Add") ? "Add Tag" : "Rename Tag";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
