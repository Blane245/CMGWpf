using CMGWpf.Services;
using CMGWpf.MVVM;
using CMGWpf.View;
using FFMpegCore;
using System.IO;
using System.Windows;

namespace CMGWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Handle unhandled exceptions on UI thread
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Handle unhandled exceptions on non-UI threads
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Handle unhandled exceptions from async/await tasks
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // Handle application exit to ensure cleanup
            this.Exit += App_Exit;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Unhandled UI Thread Exception: {e.Exception}");

                // Release file lock before crashing
                FileLockService.Instance.ReleaseLock();

                // Show error message to user
                MessageBox.Show(
                    $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe application will now close.",
                    "Critical Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in exception handler: {ex}");
            }

            // Let the application crash - don't mark as handled
            e.Handled = false;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Unhandled Non-UI Thread Exception: {e.ExceptionObject}");

                // Release file lock before crashing
                FileLockService.Instance.ReleaseLock();

                if (e.ExceptionObject is Exception exception)
                {
                    MessageBox.Show(
                        $"A critical error occurred:\n\n{exception.Message}\n\nThe application will now close.",
                        "Critical Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in exception handler: {ex}");
            }
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Unhandled Task Exception: {e.Exception}");

                // Release file lock
                FileLockService.Instance.ReleaseLock();

                // Mark as observed so it doesn't crash the app
                e.SetObserved();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in exception handler: {ex}");
            }
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                // Ensure file lock is released on normal exit
                FileLockService.Instance.ReleaseLock();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error releasing lock on exit: {ex}");
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Configure FFmpeg path (check common installation locations)
            ConfigureFFmpeg();

            // Initialize Jump List for recent files
            JumpListService.Instance.Initialize();

            // Set shutdown mode to prevent app from closing when splash closes
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Show splash screen
            var splash = new SplashScreen();
            splash.ShowSplash(3.5); // Display for 3.5 seconds

            // Create and show main window after splash closes
            splash.Closed += (s, args) =>
            {
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                mainWindow.Show();

                // Now allow app to close when main window closes
                ShutdownMode = ShutdownMode.OnMainWindowClose;

                // Populate Jump List with existing recent files
                if (FileViewModel.Instance.RecentFiles.Count > 0)
                {
                    JumpListService.Instance.PopulateRecentFiles(FileViewModel.Instance.RecentFiles);
                }

                // Handle command-line arguments for opening a file
                if (e.Args.Length > 0 && !string.IsNullOrEmpty(e.Args[0]))
                {
                    string filePath = e.Args[0];

                    // Check if file exists
                    if (System.IO.File.Exists(filePath) && filePath.EndsWith(".cmg", StringComparison.OrdinalIgnoreCase))
                    {
                        // Try to open the file
                        mainWindow.Dispatcher.BeginInvoke(() =>
                        {
                            new FileCommands(FileViewModel.Instance, FileViewModel.Instance.File).OpenRecent(filePath);
                        }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    }
                }
            };
        }

        private void ConfigureFFmpeg()
        {
            // Check common FFmpeg installation locations
            string[] commonPaths = [
                @"C:\ffmpeg\bin",
                @"C:\Program Files\ffmpeg\bin",
                @"C:\ProgramData\chocolatey\bin",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\WinGet\Packages\Gyan.FFmpeg_Microsoft.Winget.Source_8wekyb3d8bbwe\ffmpeg-*\bin")
            ];

            foreach (var path in commonPaths)
            {
                // Handle wildcard in winget path
                if (path.Contains('*'))
                {
                    var directory = Path.GetDirectoryName(path);
                    if (directory != null && Directory.Exists(directory))
                    {
                        var pattern = Path.GetFileName(path);
                        var matchingDirs = Directory.GetDirectories(directory, pattern.Replace("\\bin", ""));
                        if (matchingDirs.Length > 0)
                        {
                            var ffmpegPath = Path.Combine(matchingDirs[0], "bin");
                            if (Directory.Exists(ffmpegPath) && File.Exists(Path.Combine(ffmpegPath, "ffmpeg.exe")))
                            {
                                GlobalFFOptions.Configure(new FFOptions { BinaryFolder = ffmpegPath });
                                System.Diagnostics.Debug.WriteLine($"FFmpeg found at: {ffmpegPath}");
                                return;
                            }
                        }
                    }
                }
                else if (Directory.Exists(path) && File.Exists(Path.Combine(path, "ffmpeg.exe")))
                {
                    GlobalFFOptions.Configure(new FFOptions { BinaryFolder = path });
                    System.Diagnostics.Debug.WriteLine($"FFmpeg found at: {path}");
                    return;
                }
            }

            // If not found in common locations, assume it's in PATH
            System.Diagnostics.Debug.WriteLine("FFmpeg not found in common locations, assuming it's in system PATH");
        }
    }

}
