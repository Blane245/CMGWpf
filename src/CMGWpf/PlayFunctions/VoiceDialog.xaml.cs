using CMGWpf.View;
using System.Diagnostics;
using System.Windows;

namespace CMGWpf.PlayFunctions
{
    /// <summary>
    /// Interaction logic for VoiceDialog.xaml
    /// </summary>
    public partial class VoiceDialog : Window
    {
        public VoiceDialog()
        {
            InitializeComponent();

            // Position window after it's fully loaded
            Loaded += VoiceDialog_Loaded;
        }

        private void VoiceDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // Reposition after window is loaded
            if (Owner != null)
            {
                var ownerHandle = new System.Windows.Interop.WindowInteropHelper(Owner).Handle;
                var ownerScreen = System.Windows.Forms.Screen.FromHandle(ownerHandle);
                double newLeft = ownerScreen.WorkingArea.Right - this.ActualWidth - 10;
                double newTop = ownerScreen.WorkingArea.Top + 90;
                this.Left = newLeft;
                this.Top = newTop;
            }
            else
            {
                Debug.WriteLine("[VoiceDialog] Owner is null!");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (DataContext is PlayViewModel ctx) ctx.ShowVoices = false;
        }
    }
}
