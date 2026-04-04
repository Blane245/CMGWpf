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
            vm.File = new()
            {
                Comment = "",
                TimeLine = new(SizeService.Instance.DisplayWidth.Value, SizeService.Instance.TimeLineHeight.Value),
                Tracks = []
            };
            TimeLineViewModel.Instance.TimeLine = vm.File.TimeLine.Clone();
            vm.IsDirty = false;
            vm.FileName = string.Empty;
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
                // clone the 
                file.TimeLine = TimeLineViewModel.Instance.TimeLine;
                //TODO the same will be done for the tracks when that viewmodel is developed
                _ = FileHandlers.Write(file, dlg.FileName);
                vm.IsDirty = false;
                vm.StatusMessages = [new Message { Text = $"New File {dlg.FileName} created.", Error = false }];
                vm.FileName = dlg.FileName;
                vm.AddRecentFile(vm.FileName);
            }
        }
        public void Open()
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
                file = new();
                string fileName = dlg.FileName;
                string status = FileHandlers.Read(out file, fileName);
                vm.IsDirty = false;
                if (status == string.Empty) vm.StatusMessages = [new Message { Text = $"File {fileName} opened.", Error = false }];
                else vm.StatusMessages = [new Message { Text = status, Error = true }];
                vm.FileName = fileName;
                vm.AddRecentFile(fileName);
                vm.File = file;
                TimeLineViewModel.Instance.TimeLine = vm.File.TimeLine.Clone();
            }
        }
        public void OpenRecent(string fileName)
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
            file = new();
            string status = FileHandlers.Read(out file, fileName);
            if (status == string.Empty)
            {
                vm.IsDirty = false;
                vm.StatusMessages = [new Message { Text = $"File {fileName} opened.", Error = false }];
                vm.FileName = fileName;
                vm.File = file;
                TimeLineViewModel.Instance.TimeLine = vm.File.TimeLine.Clone();
                vm.AddRecentFile(fileName);
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
            vm.ActiveDialog = new CommentDialog
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };
            vm.ActiveDialog.ShowDialog();
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
            vm.ActiveDialog?.Close();
        }

        public void EditCommentCancel()
        {
            Debug.WriteLine("Cancel Comment command being executed.");
            vm.StatusMessages = [new Message { Text = "Comment not changed.", Error = false }];
            // restore the comment in the UIModel to the original comment in the model
            vm.ActiveDialog?.Close();
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
        public void Play(Generator? generator)
        {
            PlayFunctions.PlayEngine.StartUp(generator);
        }
        public void Report()
        {
            // TODO
        }
        #endregion
    }
}


