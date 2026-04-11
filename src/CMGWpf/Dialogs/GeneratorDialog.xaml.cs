using CMGWpf.View;
using System.ComponentModel;
using System.Windows;

namespace CMGWpf.Dialogs
{
    /// <summary>
    /// Interaction logic for GeneratorDialog.xaml
    /// </summary>
    public partial class GeneratorDialog : Window
    {
        public GeneratorDialog()
        {
            InitializeComponent();
            this.Closing += Generator_Closing;
            this.Loaded += GeneratorDialog_Loaded;
        }

        private void GeneratorDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Services.GlobalService.Instance.StatusMessages.Clear();
            if (DataContext is GeneratorViewModel vm) vm.Messages.Clear();
        }

        private void Generator_Closing(object? sender, CancelEventArgs e)
        {
            if (DataContext is GeneratorViewModel vm) vm.ActiveGeneratorDialog = null;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
            if (DataContext is GeneratorViewModel vm) vm.ActiveGeneratorDialog = null;
        }


    }
}