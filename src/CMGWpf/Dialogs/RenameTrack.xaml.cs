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
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Set status message when window closes via X button
            // (OK and Cancel buttons will have already set their own messages)
            if (DataContext is TrackViewModel vm)
            {
                if (string.IsNullOrEmpty(GlobalService.Instance.StatusMessage) || 
                    !GlobalService.Instance.StatusMessage.Contains("Renamed") &&
                    !GlobalService.Instance.StatusMessage.Contains("not changed"))
                {
                    GlobalService.Instance.StatusMessage = $"Track {vm.Track.Name} name not changed.";
                }
            }
        }
    }
}
