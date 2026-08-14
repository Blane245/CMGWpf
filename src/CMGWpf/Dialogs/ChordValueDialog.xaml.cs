using CMGWpf.View;
using System.Windows;

namespace CMGWpf.Dialogs
{
    /// <summary>
    /// Interaction logic for ChordValueDialog.xaml
    /// </summary>
    public partial class ChordValueDialog : Window
    {
        public ChordValueDialog()
        {
            InitializeComponent();
            DataContext = ChordSequenceView.Instance;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ChordSequenceView vm) vm.NewChordValue = null;
            Close();
        }
    }
}
