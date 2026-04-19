using System.Windows;

namespace CMGWpf.PlayFunctions
{
    public partial class VideoProgressDialog : Window
    {
        private int totalFrames;
        private int currentFrame;

        public bool IsCancelled { get; private set; }

        public VideoProgressDialog()
        {
            InitializeComponent();
            IsCancelled = false;
            Loaded += VideoProgressDialog_Loaded;
        }

        private void VideoProgressDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (Owner != null)
            {
                Left = Owner.Left;
                Top = Owner.Top;
                Width = Owner.ActualWidth;
                Height = Owner.ActualHeight;
            }
        }

        public void SetTotalFrames(int total)
        {
            totalFrames = total;
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Maximum = total;
                UpdateProgressText();
            });
        }

        public void UpdateProgress(int frame)
        {
            currentFrame = frame;
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = frame;
                UpdateProgressText();
            });
        }

        public void SetStatus(string status)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = status;
            });
        }

        private void UpdateProgressText()
        {
            double percentage = totalFrames > 0 ? (currentFrame * 100.0 / totalFrames) : 0;
            ProgressText.Text = $"{currentFrame} / {totalFrames} frames ({percentage:F1}%)";
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsCancelled = true;
            StatusText.Text = "Cancelling...";
            if (sender is System.Windows.Controls.Button button)
            {
                button.IsEnabled = false;
            }
        }
    }
}
