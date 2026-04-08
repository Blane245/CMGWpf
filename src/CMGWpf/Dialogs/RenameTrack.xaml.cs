using System.ComponentModel;
using System.Windows;
using CMGWpf.Services;
using CMGWpf.View;

namespace CMGWpf.Dialogs
{
    /// <summary>
    /// Interaction logic for RenameTrack.xaml
    /// </summary>
    public partial class RenameTrack : Window
    {
        public RenameTrack()
        {
            InitializeComponent();
            this.Closing += RenameTrack_Closing;
        }

        private void RenameTrack_Closing(object? sender, CancelEventArgs e)
        {
            (DataContext as TrackViewModel)!.ActiveRenameDialog = null;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
            (DataContext as TrackViewModel)!.ActiveRenameDialog = null;
        }
    }
}
