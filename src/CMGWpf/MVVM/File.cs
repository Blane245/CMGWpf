using CMGWpf.Dialogs;
using CMGWpf.Model;
using CMGWpf.Types;
using CMGWpf.Services;
using CMGWpf.Utilities;
using CMGWpf.View;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using CMGWpf.Model.Generators;

namespace CMGWpf.MVVM
{
    public class FileCommands(FileViewModel vm, CMGFile file)
    {
        private readonly FileViewModel vm = vm;
        private CMGFile file = file;

        #region File Menu Commands
        public void New()
        {
            DebugLog.Write("FileNew command executed.");
            if (vm.IsDirty)
            {
                DebugLog.Write("FileNew: File is dirty.");
                // ask the user if they want to save the file
                MessageBoxResult result = MessageBox.Show("The current file has been modified and the changes will be lost. Do you want to proceed?", "File Dirty", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    vm.StatusMessages = [new Message { Text = "File is dirty. The current file is still active.", Error = true }];
                    return;
                }
            }
            DebugLog.Write("FileNew: File is not dirty or old file saved first.");

            // Release lock on current file
            FileLockService.Instance.ReleaseLock();

            vm.File = new()
            {
                Comment = "",
                TimeLine = new(SizeService.Instance.DisplayWidth, SizeService.Instance.TimeLineHeight),
                Tracks = []
            };
            TimeLineViewModel.Instance.TimeLine = vm.File.TimeLine.Clone();
            vm.IsDirty = false;
            vm.FileName = string.Empty;
            ToolsViewModel.Instance?.NotifyGeneratorListChanged();
            vm.StatusMessages = [new Message { Text = "New File created.", Error = false }];
        }
        public void Save()
        {
            if (vm.FileName != "")
            {
                // pick up the timeline changes before saving
                file.TimeLine = TimeLineViewModel.Instance.TimeLine;
                _ = FileHandlers.Write(file, vm.FileName);
                vm.IsDirty = false;
                vm.StatusMessages = [new Message { Text = $"File {vm.FileName} saved.", Error = false }];
                vm.AddRecentFile(vm.FileName);
            }
            else vm.StatusMessages = [new Message { Text = "No file has been specified. Use Save As...", Error = true }];
        }
        public void SaveAs()
        {
            // open a file dialog and get the filename to save to
            SaveFileDialog dlg = new()
            {
                DefaultExt = ".cmg",
                Filter = "CMG Files (*.cmg)|*.cmg",
                OverwritePrompt = true
            };
            bool? openResult = dlg.ShowDialog();
            if (openResult == true)
            {
                string newFileName = dlg.FileName;

                // Try to acquire lock on the new file (will atomically replace current lock if successful)
                if (!FileLockService.Instance.TryAcquireLock(newFileName))
                {
                    string lockInfo = FileLockService.GetLockInfo(newFileName) ?? "File is already open in another instance";
                    MessageBox.Show(
                        $"Cannot save to file:\n\n{newFileName}\n\n{lockInfo}", 
                        "File Already Open", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    vm.StatusMessages = [new Message { Text = $"File {newFileName} is already open in another instance.", Error = true }];
                    return;
                }

                file.TimeLine = TimeLineViewModel.Instance.TimeLine;
                string writeStatus = FileHandlers.Write(file, newFileName);

                if (writeStatus == string.Empty)
                {
                    vm.IsDirty = false;
                    vm.StatusMessages = [new Message { Text = $"New File {newFileName} created.", Error = false }];
                    vm.FileName = newFileName;
                    vm.AddRecentFile(newFileName);
                }
                else
                {
                    vm.StatusMessages = [new Message { Text = writeStatus, Error = true }];
                    // Note: We've already acquired the lock on newFileName at this point.
                    // If save failed but it was a different file, we're now locked to the new file
                    // even though the save didn't complete. Release the lock to clean up.
                    if (newFileName != vm.FileName)
                    {
                        FileLockService.Instance.ReleaseLock();
                    }
                }
            }
        }
        public async void Open()
        {
            if (vm.IsDirty)
            {
                DebugLog.Write("FileOpen: File is dirty.");
                vm.StatusMessages = [new Message { Text = "File is dirty.", Error = true }] ;
                // ask the user if they want to save the file
                MessageBoxResult result = MessageBox.Show("The current file has been modified and the changes will be lost. Do you want to proceed?", "File Dirty", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    vm.StatusMessages = [new Message { Text = "File is dirty. The current file is still active.", Error = true }];
                    return;
                }
            }
            DebugLog.Write("FileOpen command being executed.");
            // open a file dialog and load the file
            OpenFileDialog dlg = new()
            {
                DefaultExt = ".cmg",
                Filter = "CMG Files (*.cmg)|*.cmg"
            };
            bool? openResult = dlg.ShowDialog();
            if (openResult == true)
            {
                string fileName = dlg.FileName;

                // Try to acquire lock on the new file (will atomically replace current lock if successful)
                if (!FileLockService.Instance.TryAcquireLock(fileName))
                {
                    string lockInfo = FileLockService.GetLockInfo(fileName) ?? "File is already open in another instance";
                    MessageBox.Show(
                        $"Cannot open file:\n\n{fileName}\n\n{lockInfo}", 
                        "File Already Open", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    vm.StatusMessages = [new Message { Text = $"File {fileName} is already open in another instance.", Error = true }];
                    return;
                }

                file = new();
                var (status, loadedFile) = await FileHandlers.Read(fileName);
                file = loadedFile;
                vm.IsDirty = false;
                if (status == string.Empty) vm.StatusMessages = [new Message { Text = $"File {fileName} opened.", Error = false }];
                else 
                {
                    vm.StatusMessages = [new Message { Text = status, Error = true }];
                    FileLockService.Instance.ReleaseLock();
                    return;
                }
                vm.FileName = fileName;
                vm.AddRecentFile(fileName);
                vm.File = file;
                TimeLineViewModel.Instance.TimeLine = vm.File.TimeLine.Clone();
                ToolsViewModel.Instance?.NotifyGeneratorListChanged();
            }
        }
        public async void OpenRecent(string fileName)
        {
            DebugLog.Write($"OpenRecentFile: {fileName}");
            if (vm.IsDirty)
            {
                DebugLog.Write("FileOpen: File is dirty.");
                vm.StatusMessages = [new Message { Text = "File is dirty.", Error = true }];
                // ask the user if they want to save the file
                MessageBoxResult result = MessageBox.Show("The current file has been modified and the changes will be lost. Do you want to proceed?", "File Dirty", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    vm.StatusMessages = [new Message { Text = "File is dirty. No changes have been made.", Error = true }];
                    return;
                }
            }

            // Try to acquire lock on the new file (will atomically replace current lock if successful)
            if (!FileLockService.Instance.TryAcquireLock(fileName))
            {
                string lockInfo = FileLockService.GetLockInfo(fileName) ?? "File is already open in another instance";
                MessageBox.Show(
                    $"Cannot open file:\n\n{fileName}\n\n{lockInfo}", 
                    "File Already Open", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                vm.StatusMessages = [new Message { Text = $"File {fileName} is already open in another instance.", Error = true }];
                return;
            }

            file = new();
            var (status, loadedFile) = await FileHandlers.Read(fileName);
            file = loadedFile;
            if (status == string.Empty)
            {
                vm.IsDirty = false;
                vm.StatusMessages = [new Message { Text = $"File {fileName} opened.", Error = false }];
                vm.FileName = fileName;
                vm.File = file;
                TimeLineViewModel.Instance.TimeLine = vm.File.TimeLine.Clone();
                vm.AddRecentFile(fileName);
                ToolsViewModel.Instance?.NotifyGeneratorListChanged();
            }
            else
            {
                vm.StatusMessages = [new Message { Text = status, Error = true }];
                FileLockService.Instance.ReleaseLock();
            }
        }
        public void Exit()
        {
            if (vm.IsDirty)
            {
                DebugLog.Write("Exit: File is dirty.");
                vm.StatusMessages = [new Message { Text = "File is dirty.", Error = true }];
                MessageBoxResult result = MessageBox.Show("The current file has been modified and the changes will be lost. Do you want to exit?", "File Dirty", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    vm.StatusMessages = [new Message { Text = "File is dirty. CMG not Exited.", Error = false }];
                    return;
                }

            }
            Application.Current.Shutdown();
        }
        #endregion
        #region Edit Menu Commands
        static CommentDialog? commentDialog;
        public void EditComment()
        {
            // display the comment dialog
            vm.NewComment = vm.File.Comment;
            commentDialog = new()
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            commentDialog.ShowDialog();
        }
        public void EditCommentOk()
        {
            if (vm.NewComment != vm.File.Comment)
            {
                vm.File.Comment = vm.NewComment;
                vm.IsDirty = true;
                vm.StatusMessages = [new Message { Text = "Comment has changed.", Error = false }];
            }
            else
            {
                vm.StatusMessages = [new Message { Text = "Comment has not changed.", Error = false }];
            }
            commentDialog?.Close();
        }

        public void EditPreferences()
        {
            DebugLog.Write("EditPreferences command being executed.");

            PreferencesDialog dialog = new PreferencesDialog
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            dialog.ShowDialog();
        }
        public void AddTrack()
        {
            DebugLog.Write("Add Track command being executed.");
            int uid = Uid.Get("Track", vm.File.Tracks);
            Track newTrack = new(uid);
            List<Track> newTracks = [.. vm.File.Tracks];
            newTracks.Add(newTrack);
            vm.NotifyTracksChanged(newTracks);
            vm.StatusMessages = [new Message { Text = $"new Track named T{uid} added.", Error = false }];
            vm.IsDirty = true;
        }
        public void Play(Generator? generator)
        {
            PlayFunctions.PlayEngine.StartUp(generator, true, false);
        }
        public void Report(Generator? generator)
        {
            PlayFunctions.PlayEngine.StartUp(generator, false, false);
        }
        public void About(object? param)
        {
            DebugLog.Write("About command being executed.");
            AboutDialog dialog = new()
            {
                Owner = Application.Current.MainWindow
            };
            dialog.ShowDialog();
        }
        public void UG(object? param)
        {
            // Try multiple locations for the User's Guide
            string? ugPath = null;

            // Location 1: Installed location (same directory as executable)
            string installedPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "docs", "UsersGuide.html");
            if (System.IO.File.Exists(installedPath))
            {
                ugPath = installedPath;
            }
            else
            {
                // Location 2: Development location (solution root)
                string devPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "docs", "UsersGuide.html");
                string fullDevPath = System.IO.Path.GetFullPath(devPath);
                if (System.IO.File.Exists(fullDevPath))
                {
                    ugPath = fullDevPath;
                }
            }

            if (ugPath == null)
            {
                MessageBox.Show("User's Guide (UsersGuide.html) not found.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                vm.StatusMessages = [new Message { Text = "User's Guide not found.", Error = true }];
                return;
            }

            string url = new Uri(ugPath).AbsoluteUri;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open browser: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}


