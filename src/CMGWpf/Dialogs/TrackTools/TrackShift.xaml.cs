using CMGWpf.View;
using System.Windows;

namespace CMGWpf.Dialogs.TrackTools
{
    /// <summary>
    /// Interaction logic for TrackShift.xaml
    /// </summary>
    public partial class TrackShift : Window
    {
        public TrackShift()
        {
            InitializeComponent();
            this.Closing += TrackShift_Closing;
            this.Loaded += TrackShift_Loaded;
        }

        private void TrackShift_Loaded(object sender, RoutedEventArgs e)
        {
            Services.GlobalService.Instance.StatusMessages.Clear();
        }

        private void TrackShift_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            (DataContext as TrackViewModel)!.ActiveShiftDialog = null;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
            (DataContext as TrackViewModel)!.ActiveRenameDialog = null;
        }
    }
}
