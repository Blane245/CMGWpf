using System.Diagnostics;
using System.IO;

namespace CMGWpf.Services
{
    /// <summary>
    /// Manages file locking across multiple application instances to prevent the same CMG file 
    /// from being opened simultaneously by different instances.
    /// </summary>
    public class FileLockService
    {
        private static FileLockService? _instance;
        public static FileLockService Instance => _instance ??= new FileLockService();

        private string? _currentLockedFile;
        private FileStream? _lockFileStream;

        private FileLockService()
        {
        }

        /// <summary>
        /// Gets the path to the lock file for a given CMG file.
        /// </summary>
        private static string GetLockFilePath(string cmgFilePath)
        {
            return cmgFilePath + ".lock";
        }

        /// <summary>
        /// Attempts to acquire a lock on the specified file.
        /// If a different file is currently locked, this will atomically replace it.
        /// If the same file is already locked, returns true immediately.
        /// If lock acquisition fails, the previous lock (if any) is maintained.
        /// </summary>
        /// <param name="filePath">The full path to the CMG file to lock</param>
        /// <returns>True if lock was acquired, false if file is already locked by another process</returns>
        public bool TryAcquireLock(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            // If we're trying to lock the same file that's already locked, it's a no-op
            if (_currentLockedFile == filePath && _lockFileStream != null)
            {
                Debug.WriteLine($"FileLockService: File {filePath} is already locked by this instance");
                return true;
            }

            string lockFilePath = GetLockFilePath(filePath);

            // Save current lock state in case we need to restore it
            var previousLockStream = _lockFileStream;
            var previousLockedFile = _currentLockedFile;

            try
            {
                // Clean up any stale lock files
                CleanupStaleLock(lockFilePath);

                // Try to create and hold the lock file exclusively
                _lockFileStream = new FileStream(
                    lockFilePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 1,
                    FileOptions.DeleteOnClose);

                // Write process information to the lock file
                using (StreamWriter writer = new StreamWriter(_lockFileStream, leaveOpen: true))
                {
                    writer.WriteLine($"Process ID: {Environment.ProcessId}");
                    writer.WriteLine($"Process Name: {Process.GetCurrentProcess().ProcessName}");
                    writer.WriteLine($"Locked at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine($"File: {filePath}");
                    writer.Flush();
                }

                _currentLockedFile = filePath;

                // Success! Now release the previous lock if there was one
                if (previousLockStream != null && previousLockedFile != filePath)
                {
                    Debug.WriteLine($"FileLockService: Releasing previous lock on {previousLockedFile}");
                    try
                    {
                        previousLockStream.Close();
                        previousLockStream.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"FileLockService: Error releasing previous lock: {ex.Message}");
                    }
                }

                Debug.WriteLine($"FileLockService: Acquired lock on {filePath}");
                return true;
            }
            catch (IOException)
            {
                // File is already locked by another process
                Debug.WriteLine($"FileLockService: File {filePath} is already locked by another process");

                // Restore previous lock state
                _lockFileStream = previousLockStream;
                _currentLockedFile = previousLockedFile;

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FileLockService: Error acquiring lock: {ex.Message}");

                // Restore previous lock state
                _lockFileStream = previousLockStream;
                _currentLockedFile = previousLockedFile;

                return false;
            }
        }

        /// <summary>
        /// Releases the lock on the currently locked file.
        /// </summary>
        public void ReleaseLock()
        {
            if (_lockFileStream != null)
            {
                Debug.WriteLine($"FileLockService: Releasing lock on {_currentLockedFile}");
                
                try
                {
                    _lockFileStream.Close();
                    _lockFileStream.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"FileLockService: Error releasing lock: {ex.Message}");
                }
                finally
                {
                    _lockFileStream = null;
                    _currentLockedFile = null;
                }
            }
        }

        /// <summary>
        /// Checks if a file is currently locked and cleans up stale locks from dead processes.
        /// </summary>
        private static void CleanupStaleLock(string lockFilePath)
        {
            if (!File.Exists(lockFilePath))
                return;

            try
            {
                // Try to read the lock file to get process ID
                string[] lines = File.ReadAllLines(lockFilePath);
                if (lines.Length > 0)
                {
                    string firstLine = lines[0];
                    if (firstLine.StartsWith("Process ID: "))
                    {
                        if (int.TryParse(firstLine.Substring("Process ID: ".Length), out int processId))
                        {
                            // Check if the process is still running
                            try
                            {
                                Process process = Process.GetProcessById(processId);
                                // Process exists, lock is still valid
                                Debug.WriteLine($"FileLockService: Lock file is valid, process {processId} is running");
                                return;
                            }
                            catch (ArgumentException)
                            {
                                // Process doesn't exist anymore, lock is stale
                                Debug.WriteLine($"FileLockService: Cleaning up stale lock, process {processId} no longer exists");
                            }
                        }
                    }
                }

                // If we get here, the lock is stale - try to delete it
                File.Delete(lockFilePath);
                Debug.WriteLine($"FileLockService: Deleted stale lock file {lockFilePath}");
            }
            catch (IOException)
            {
                // File is locked by another process, which means it's valid
                Debug.WriteLine($"FileLockService: Lock file is currently in use");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FileLockService: Error cleaning up stale lock: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets information about who has locked a file.
        /// </summary>
        /// <param name="filePath">The full path to the CMG file</param>
        /// <returns>Information about the lock, or null if not locked</returns>
        public static string? GetLockInfo(string filePath)
        {
            string lockFilePath = GetLockFilePath(filePath);
            
            if (!File.Exists(lockFilePath))
                return null;

            try
            {
                string[] lines = File.ReadAllLines(lockFilePath);
                if (lines.Length > 0)
                {
                    return string.Join(Environment.NewLine, lines);
                }
            }
            catch (IOException)
            {
                // File is locked, but we can't read it
                return "File is locked by another instance (details unavailable)";
            }
            catch (Exception)
            {
                // Ignore other errors
            }

            return null;
        }

        /// <summary>
        /// Gets the currently locked file path.
        /// </summary>
        public string? CurrentLockedFile => _currentLockedFile;
    }
}
