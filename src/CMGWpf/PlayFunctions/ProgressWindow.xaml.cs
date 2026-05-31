using CMGWpf.View;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CMGWpf.PlayFunctions
{
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
            DataContext = PlayViewModel.Instance;
            Closing += ProgressWindow_Closing;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ProgressWindow_Closing(object? sender, CancelEventArgs e)
        {
            PlayViewModel.Instance.UserCancelled = true;
            PlayViewModel.Instance.ProgressMessage = "Cancelling...";
        }
    }
}
