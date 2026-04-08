using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Shell;

namespace CMGWpf.Services
{
    /// <summary>
    /// Manages the Windows 7+ Jump List (recent files shown in taskbar)
    /// </summary>
    public class JumpListService
    {
        private static JumpListService? _instance;
        public static JumpListService Instance => _instance ??= new JumpListService();

        private JumpList? _jumpList;

        // Windows Shell API for adding to recent documents
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern void SHAddToRecentDocs(uint uFlags, [MarshalAs(UnmanagedType.LPWStr)] string pv);

        private const uint SHARD_PATHW = 0x00000003;

        private JumpListService()
        {
        }

        /// <summary>
        /// Initializes the Jump List for the application
        /// </summary>
        public void Initialize()
        {
            try
            {
                _jumpList = new JumpList();
                _jumpList.ShowRecentCategory = true;
                _jumpList.ShowFrequentCategory = false;

                JumpList.SetJumpList(System.Windows.Application.Current, _jumpList);

                System.Diagnostics.Debug.WriteLine("JumpListService: Initialized");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JumpListService: Error initializing: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a file to the recent files jump list
        /// </summary>
        /// <param name="filePath">Full path to the CMG file</param>
        public void AddToRecentFiles(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return;

            try
            {
                // Add to Windows recent documents - this automatically updates the jump list
                SHAddToRecentDocs(SHARD_PATHW, filePath);

                System.Diagnostics.Debug.WriteLine($"JumpListService: Added {filePath} to recent files");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JumpListService: Error adding to recent files: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a custom task to the Jump List (optional - for future use)
        /// </summary>
        public void AddCustomTask(string title, string description, string applicationPath, string arguments, string iconPath)
        {
            if (_jumpList == null)
                return;

            try
            {
                var task = new JumpTask
                {
                    Title = title,
                    Description = description,
                    ApplicationPath = applicationPath,
                    Arguments = arguments,
                    IconResourcePath = iconPath
                };

                _jumpList.JumpItems.Add(task);
                _jumpList.Apply();

                System.Diagnostics.Debug.WriteLine($"JumpListService: Added custom task '{title}'");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JumpListService: Error adding custom task: {ex.Message}");
            }
        }

        /// <summary>
        /// Refreshes the jump list
        /// </summary>
        public void Refresh()
        {
            if (_jumpList == null)
                return;

            try
            {
                _jumpList.Apply();
                System.Diagnostics.Debug.WriteLine("JumpListService: Jump list refreshed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JumpListService: Error refreshing: {ex.Message}");
            }
        }

        /// <summary>
        /// Populates the jump list with a collection of recent files
        /// </summary>
        /// <param name="recentFiles">Collection of file paths to add</param>
        public void PopulateRecentFiles(IEnumerable<string> recentFiles)
        {
            foreach (var file in recentFiles)
            {
                AddToRecentFiles(file);
            }
        }
    }
}
