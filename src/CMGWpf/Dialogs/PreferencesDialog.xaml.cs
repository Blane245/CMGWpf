using CMGWpf.Services;
using System.Windows;

namespace CMGWpf.Dialogs
{
    /// <summary>
    /// Interaction logic for PreferencesDialog.xaml
    /// </summary>
    public partial class PreferencesDialog : Window
    {
        GlobalService vm = GlobalService.Instance;
        public PreferencesDialog()
        {
            InitializeComponent();
            DataContext = vm;
            vm.StatusMessages.Clear();
            this.Closing += PreferencesDialog_Closing;
        }

        private void PreferencesDialog_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();

        }
    }
}
