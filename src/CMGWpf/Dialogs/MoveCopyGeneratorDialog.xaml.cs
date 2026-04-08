using CMGWpf.View;
using System.Windows;

namespace CMGWpf.Dialogs
{
    /// <summary>
    /// Interaction logic for MoveCopyGeneratorDialog.xaml
    /// </summary>
    public partial class MoveCopyGeneratorDialog : Window
    {
        public MoveCopyGeneratorDialog()
        {
            InitializeComponent();
            this.Closing += MoveCopyGeneratorDialog_Closing;
        }

        private void MoveCopyGeneratorDialog_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is GeneratorViewModel vm) vm.ActiveGeneratorDialog = null;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is GeneratorViewModel vm) vm.ActiveGeneratorDialog = null;
            Close();
        }
    }
}
