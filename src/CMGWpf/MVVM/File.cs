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
        //private TimeLine timeLine = timeLine;

        #region File Menu Commands
        public void New()
        {
            Debug.WriteLine("FileNew command executed.");
            if (vm.IsDirty)
            {
                Debug.WriteLine("FileNew: File is dirty.");
                // ask the user if they want to save the file
                MessageBoxResult result = MessageBox.Show("The current file has been modified and the changes will be lost. Do you want to proceed?", "File Dirty", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    vm.StatusMessages = [new Message { Text = "File is dirty. The current file is still active.", Error = true }];
                    return;
                }
            }
            Debug.WriteLine("FileNew: File is not dirty or old file saved first.");

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
                // copy the UIModel to the model and the UIModel in the TimeLineView to the model before saving the file
                file.TimeLine = TimeLineViewModel.Instance.TimeLine;
                //TODO the same will be done for the tracks when that viewmodel is developed
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

                // clone the
                file.TimeLine = TimeLineViewModel.Instance.TimeLine;
                //TODO the same will be done for the tracks when that viewmodel is developed
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
                Debug.WriteLine("FileOpen: File is dirty.");
                vm.StatusMessages = [new Message { Text = "File is dirty.", Error = true }] ;
                // ask the user if they want to save the file
                MessageBoxResult result = MessageBox.Show("The current file has been modified and the changes will be lost. Do you want to proceed?", "File Dirty", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    vm.StatusMessages = [new Message { Text = "File is dirty. The current file is still active.", Error = true }];
                    return;
                }
            }
            Debug.WriteLine("FileOpen command being executed.");
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
            Debug.WriteLine($"OpenRecentFile: {fileName}");
            if (vm.IsDirty)
            {
                Debug.WriteLine("FileOpen: File is dirty.");
                vm.StatusMessages = [new Message { Text = "File is dirty.", Error = true }];
                // ask the user if they want to save the file
                MessageBoxResult result = MessageBox.Show("The current file has been modified and the changes will be lost. Do you want to proceed?", "File Dirty", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    vm.StatusMessages = [new Message { Text = "File is dirty. No changed has been made", Error = true }];
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
                Debug.WriteLine("Exit: File is dirty.");
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
        public void EditComment()
        {
            Debug.WriteLine("EditComment command being executed.");

            // display the comment dialog
            vm.NewComment = vm.File.Comment;
            CommentDialog activeDialog = new()
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            activeDialog.ShowDialog();
        }
        public void EditCommentOk()
        {
            if (vm.NewComment != vm.File.Comment)
            {
                Debug.WriteLine("Comment has changed.");
                vm.File.Comment = vm.NewComment;
                vm.IsDirty = true;
                vm.StatusMessages = [new Message { Text = "Comment has changed.", Error = false }];
            }
            else
            {
                Debug.WriteLine("Comment has not changed.");
                vm.StatusMessages = [new Message { Text = "Comment has not changed.", Error = false }];
            }
        }

        public void EditPreferences()
        {
            Debug.WriteLine("EditPreferences command being executed.");

            // display the preferences dialog
            //vm.NewComment = vm.File.Comment;
            PreferencesDialog dialog = new PreferencesDialog
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            dialog.ShowDialog();
        }
        public void AddTrack()
        {
            Debug.WriteLine("Add Track command being executed.");
            int uid = Uid.Get("Track", vm.File.Tracks);
            Track newTrack = new(uid);
            List<Track> newTracks = [.. vm.File.Tracks];
            newTracks.Add(newTrack);
            vm.NotifyTracksChanged(newTracks);
            vm.StatusMessages = [new Message { Text = $"new Track named T{uid} added.", Error = false }];
            vm.IsDirty = true;
        }
        /// <summary>
        /// Run the PlayEngine for playing the file. If a generator is provided, the PlayEngine will start up with that generator selected and the UI will be in play mode. If no generator is provided, the PlayEngine will start up with active generators and the UI will not be in play mode.
        /// </summary>
        /// <param name="generator"></param>
        public void Play(Generator? generator)
        {
            PlayFunctions.PlayEngine.StartUp(generator, true, false);
        }
        /// <summary>
        /// Run the PlayEngine for reporting the file. If a generator is provided, the PlayEngine will start up with that generator selected and the UI will be in report mode. If no generator is provided, the PlayEngine will start up with active generators and the UI will not be in report mode.
        /// </summary>
        /// <param name="generator"></param>
        public void Report(Generator? generator)
        {
            PlayFunctions.PlayEngine.StartUp(generator, false, false);
        }
        public void About(object? param)
        {
            Debug.WriteLine("About command being executed.");
            // display the about dialog
            AboutDialog dialog = new()
            {
                Owner = Application.Current.MainWindow
            };
            dialog.ShowDialog();
        }
        public void UG(object? param)
        {
            Debug.WriteLine("User's Guide command being executed.");

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


