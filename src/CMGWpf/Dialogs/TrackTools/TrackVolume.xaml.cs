using CMGWpf.View;
using System;
using System.Windows;

namespace CMGWpf.Dialogs.TrackTools
{
    /// <summary>
    /// Interaction logic for TrackVolume.xaml
    /// </summary>
    public partial class TrackVolume : Window
    {
        public TrackVolume()
        {
            InitializeComponent();
            this.Closing += TrackVolume_Closing;
        }

        private void TrackVolume_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            (DataContext as TrackViewModel)!.ActiveVolumeDialog = null;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
            (DataContext as TrackViewModel)!.ActiveVolumeDialog = null;

        }
    }
}
