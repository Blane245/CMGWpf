using CMGWpf.View;
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
            this.Loaded += GeneratorDialog_Loaded;
        }

        private void GeneratorDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is GeneratorViewModel vm)
            {
                vm.Messages = [];
            }
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = (this.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}