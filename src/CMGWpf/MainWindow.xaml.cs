using CMGWpf.Services;
using CMGWpf.View;
using System.ComponentModel;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace CMGWpf
{
    /// <summary>
    /// Intializes the main window, sets the data context to the FileViewModel, and handles window events for loading, closing, and resizing. On load, it moves the window to the secondary monitor if available and maximizes it. On closing, it saves settings and shuts down the application. On size change, it updates the SizeService with the new window dimensions.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = FileViewModel.Instance;
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            this.SizeChanged += MainWindow_SizeChanged;
        }

        private void MoveToSecondaryMonitor()
        {
            var screens = WinForms.Screen.AllScreens;
            if (screens.Length > 1)
            {
                var secondaryScreen = screens.FirstOrDefault(s => !s.Primary) ?? screens[1];
                var workingArea = secondaryScreen.WorkingArea;

                // Position the window on secondary monitor
                this.WindowState = WindowState.Normal;
                this.Left = workingArea.Left;
                this.Top = workingArea.Top;
                this.Width = workingArea.Width;
                this.Height = workingArea.Height;

                // Delay maximizing until AFTER the window has processed its new position
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.WindowState = WindowState.Maximized;
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            Application.Current.Shutdown();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MoveToSecondaryMonitor();
            SizeService.Instance.WindowWidth = this.ActualWidth;
            SizeService.Instance.WindowHeight = this.ActualHeight;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Capture the new window size
            SizeService.Instance.WindowWidth = e.NewSize.Width;
            SizeService.Instance.WindowHeight = e.NewSize.Height;
        }
    }
}