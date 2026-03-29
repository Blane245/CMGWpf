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
            Debug.WriteLine("[VoiceDialog] Constructor called");
            InitializeComponent();

            // Position window after it's fully loaded
            Loaded += VoiceDialog_Loaded;
            Debug.WriteLine("[VoiceDialog] Constructor completed");
        }

        private void VoiceDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[VoiceDialog] Loaded event fired");

            // Reposition after window is loaded
            if (Owner != null)
            {
                Debug.WriteLine($"[VoiceDialog] Owner is not null: {Owner.GetType().Name}");

                var ownerHandle = new System.Windows.Interop.WindowInteropHelper(Owner).Handle;
                Debug.WriteLine($"[VoiceDialog] Owner handle: {ownerHandle}");

                var ownerScreen = System.Windows.Forms.Screen.FromHandle(ownerHandle);
                Debug.WriteLine($"[VoiceDialog] Owner screen bounds: {ownerScreen.Bounds}");
                Debug.WriteLine($"[VoiceDialog] Owner screen working area: {ownerScreen.WorkingArea}");
                Debug.WriteLine($"[VoiceDialog] Current position - Left: {this.Left}, Top: {this.Top}");
                Debug.WriteLine($"[VoiceDialog] This window Width: {this.ActualWidth}, Height: {this.ActualHeight}");

                double newLeft = ownerScreen.WorkingArea.Right - this.ActualWidth - 10;
                double newTop = ownerScreen.WorkingArea.Top + 90;

                Debug.WriteLine($"[VoiceDialog] Calculated position - Left: {newLeft}, Top: {newTop}");

                this.Left = newLeft;
                this.Top = newTop;

                Debug.WriteLine($"[VoiceDialog] Position set - Left: {this.Left}, Top: {this.Top}");
            }
            else
            {
                Debug.WriteLine("[VoiceDialog] Owner is null!");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Debug.WriteLine("[VoiceDialog] Window_Closed event fired");
            (DataContext as FileViewModel)!.ShowVoices = false;
        }
    }
}
