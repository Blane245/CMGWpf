using System.Windows;

namespace CMGWpf.Dialogs
{
    /// <summary>
    /// Interaction logic for ViewSequence.xaml
    /// </summary>
    public partial class ViewSequence : Window
    {
        public ViewSequence()
        {
            InitializeComponent();
        }
        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
