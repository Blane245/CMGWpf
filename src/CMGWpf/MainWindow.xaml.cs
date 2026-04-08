using CMGWpf.Services;
using CMGWpf.View;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using WinForms = System.Windows.Forms;

namespace CMGWpf
{
    /// <summary>
    /// Intializes the main window, sets the data context to the FileViewModel, and handles window events for loading, closing, and resizing. On load, it restores the window position and size from saved settings or moves to secondary monitor if first run. On closing, it saves window state and settings. On size change, it updates the SizeService with the new window dimensions.
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isFirstRun = false;

        /// <summary>
        /// Gets the MainWindow instance from Application.Current.MainWindow
        /// </summary>
        /// <returns>The MainWindow instance</returns>
        public static MainWindow? GetInstance()
        {
            return Application.Current.MainWindow as MainWindow;
        }

        #region Windows API for Taskbar-Aware Maximize

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        private const uint MONITOR_DEFAULTTONEAREST = 2;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public uint Size;
            public RECT Monitor;
            public RECT WorkArea;
            public uint Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            DataContext = FileViewModel.Instance;
            this.SourceInitialized += MainWindow_SourceInitialized;
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            this.SizeChanged += MainWindow_SizeChanged;
            this.StateChanged += MainWindow_StateChanged;
            this.LocationChanged += MainWindow_LocationChanged;

            // Check if this is the first run (no saved position)
            _isFirstRun = Properties.Settings.Default.WindowLeft == 0 && 
                         Properties.Settings.Default.WindowTop == 0;
        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            // Hook into Windows message handling to fix maximize behavior
            IntPtr handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(handle)?.AddHook(WindowProc);
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_GETMINMAXINFO = 0x0024;

            if (msg == WM_GETMINMAXINFO)
            {
                // Get the monitor info for the monitor containing this window
                IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
                if (monitor != IntPtr.Zero)
                {
                    MONITORINFO monitorInfo = new MONITORINFO();
                    monitorInfo.Size = (uint)Marshal.SizeOf(typeof(MONITORINFO));
                    GetMonitorInfo(monitor, ref monitorInfo);

                    MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO))!;

                    // Set max size to the working area (excludes taskbar)
                    mmi.ptMaxPosition.X = monitorInfo.WorkArea.Left - monitorInfo.Monitor.Left;
                    mmi.ptMaxPosition.Y = monitorInfo.WorkArea.Top - monitorInfo.Monitor.Top;
                    mmi.ptMaxSize.X = monitorInfo.WorkArea.Right - monitorInfo.WorkArea.Left;
                    mmi.ptMaxSize.Y = monitorInfo.WorkArea.Bottom - monitorInfo.WorkArea.Top;

                    Marshal.StructureToPtr(mmi, lParam, true);
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        private void RestoreWindowState()
        {
            if (_isFirstRun)
            {
                // First run - move to secondary monitor if available
                MoveToSecondaryMonitor();
            }
            else
            {
                // Restore saved position and size
                var settings = Properties.Settings.Default;

                // Restore window position
                this.Left = settings.WindowLeft;
                this.Top = settings.WindowTop;
                this.Width = settings.WindowWidth;
                this.Height = settings.WindowHeight;

                // Ensure window is visible on screen
                EnsureWindowIsVisible();

                // Restore window state
                if (Enum.TryParse<WindowState>(settings.WindowState, out var windowState))
                {
                    this.WindowState = windowState;
                }
            }
        }

        private void EnsureWindowIsVisible()
        {
            // Get the bounds of all screens
            var screens = WinForms.Screen.AllScreens;
            var isVisible = false;

            foreach (var screen in screens)
            {
                var workingArea = screen.WorkingArea;
                if (this.Left >= workingArea.Left && 
                    this.Left < workingArea.Right &&
                    this.Top >= workingArea.Top && 
                    this.Top < workingArea.Bottom)
                {
                    isVisible = true;
                    break;
                }
            }

            // If window is not visible on any screen, reset to primary screen
            if (!isVisible)
            {
                var primaryScreen = WinForms.Screen.PrimaryScreen;
                if (primaryScreen != null)
                {
                    var workingArea = primaryScreen.WorkingArea;
                    this.Left = workingArea.Left;
                    this.Top = workingArea.Top;
                    this.Width = Math.Min(this.Width, workingArea.Width);
                    this.Height = Math.Min(this.Height, workingArea.Height);
                }
            }
        }

        private void SaveWindowState()
        {
            var settings = Properties.Settings.Default;

            // Save window state
            settings.WindowState = this.WindowState.ToString();

            // Save position and size only if not maximized or minimized
            if (this.WindowState == WindowState.Normal)
            {
                settings.WindowLeft = this.Left;
                settings.WindowTop = this.Top;
                settings.WindowWidth = this.Width;
                settings.WindowHeight = this.Height;
            }
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
            else
            {
                // Single monitor - just maximize on primary
                this.WindowState = WindowState.Maximized;
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            // Check for unsaved changes
            if (FileViewModel.Instance.IsDirty)
            {
                MessageBoxResult result = MessageBox.Show(
                    "The current file has been modified and the changes will be lost. Do you want to exit?", 
                    "File Dirty", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    // Cancel the closing event
                    e.Cancel = true;
                    return;
                }
            }

            SaveWindowState();
            Properties.Settings.Default.Save();

            // Release file lock before closing
            FileLockService.Instance.ReleaseLock();

            Application.Current.Shutdown();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RestoreWindowState();
            SizeService.Instance.WindowWidth = this.ActualWidth;
            SizeService.Instance.WindowHeight = this.ActualHeight;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Capture the new window size
            SizeService.Instance.WindowWidth = e.NewSize.Width;
            SizeService.Instance.WindowHeight = e.NewSize.Height;

            // Force layout update to ensure content is rendered
            this.UpdateLayout();
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            // When state changes, let SizeChanged handle the size update
            // Just force a layout update here
            this.Dispatcher.BeginInvoke(() =>
            {
                this.UpdateLayout();
                // Ensure SizeService is updated with actual dimensions after layout
                SizeService.Instance.WindowWidth = this.ActualWidth;
                SizeService.Instance.WindowHeight = this.ActualHeight;
            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void MainWindow_LocationChanged(object? sender, EventArgs e)
        {
            // We track location changes so they can be saved
            // The actual saving happens on close
        }
    }
}