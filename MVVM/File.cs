using CMGWpf.Dialogs;
using CMGWpf.Model;
using CMGWpf.PlayFunctions;
using CMGWpf.Types;
using CMGWpf.PlayFunctions.Utilities;
using CMGWpf.Services;
using CMGWpf.Utilities;
using CMGWpf.View;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

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
                    vm.Status = "File is dirty. The current file is still active.";
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
            vm.Status = "New File created.";
        }
        public void Save()
        {
            if (vm.FileName != "")
            {
                // copy the UIModel to the model and the UIModel in the TimeLineView to the model before saving the file
                file.TimeLine = TimeLineViewModel.Instance.TimeLine;
                //TODO the same will be done for the tracks when that viewmodel is developed
                vm.Status = FileHandlers.Write(file, vm.FileName);
                vm.IsDirty = false;
                vm.Status = $"File {vm.FileName} saved.";
                vm.AddRecentFile(vm.FileName);
            }
            else vm.Status = "No file has been specified. Use Save As...";
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
                vm.Status = FileHandlers.Write(file, dlg.FileName);
                vm.IsDirty = false;
                vm.Status = $"New File {dlg.FileName} created.";
                vm.FileName = dlg.FileName;
                vm.AddRecentFile(vm.FileName);
            }
        }
        public void Open()
        {
            if (vm.IsDirty)
            {
                Debug.WriteLine("FileOpen: File is dirty.");
                vm.Status = "File is dirty.";
                // ask the user if they want to save the file
                MessageBoxResult result = MessageBox.Show("The current file has been modified and the changes will be lost. Do you want to proceed?", "File Dirty", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    vm.Status = "File is dirty. The current file is still active";
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
                vm.Status = FileHandlers.Read(out file, fileName);
                vm.IsDirty = false;
                if (vm.Status == string.Empty) vm.Status = $"File {fileName} opened.";
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
                vm.Status = "File is dirty.";
                // ask the user if they want to save the file
                MessageBoxResult result = MessageBox.Show("The current file has been modified and the changes will be lost. Do you want to proceed?", "File Dirty", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    vm.Status = "File is dirty. No changed has been made";
                    return;
                }
            }
            file = new();
            vm.Status = FileHandlers.Read(out file, fileName);
            if (vm.Status == string.Empty)
            {
                vm.IsDirty = false;
                vm.Status = $"File {fileName} opened.";
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
                vm.Status = "File is dirty.";
                MessageBoxResult result = MessageBox.Show("The current file has been modified and the changes will be lost. Do you want to exit?", "File Dirty", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                {
                    vm.Status = "File is dirty. CMG not Exited.";
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
                vm.Status = "Comment has changed.";
            }
            else
            {
                Debug.WriteLine("Comment has not changed.");
                vm.Status = "Comment has not changed.";
            }
            vm.ActiveDialog?.Close();
        }

        public void EditCommentCancel()
        {
            Debug.WriteLine("Cancel Comment command being executed.");
            vm.Status = "Comment not changed";
            // restore the comment in the UIModel to the original comment in the model
            vm.ActiveDialog?.Close();
        }
        public void AddTrack()
        {
            Debug.WriteLine("Add Track command being executed.");
            int uid = Uid.Get("Track", vm.File.Tracks);
            Track newTrack = new(uid);
            List<Track> newTracks = [.. vm.File.Tracks];
            newTracks.Add(newTrack);
            vm.NotifyTracksChanged(newTracks);
            vm.Status = $"new Track named T{uid} added.";
            vm.IsDirty = true;
        }
        public void Play()
        {
            Debug.WriteLine("Play command being executed.");
            // Check ReadyPlay and start the play dialog if the file is ready to play

            PlayTypes.ReadyPlayOutput ready = ReadyPlay.Check(null);
            if (ready.ErrorMessage != "")
            {
                vm.Status = ready.ErrorMessage;
                Debug.WriteLine($"Error: {ready.ErrorMessage}");
                return;
            }
            PlayDialog playDialog = new()
            {
                DataContext = FileViewModel.Instance,
                Owner = Application.Current.MainWindow
            };

            // Set the PlayDialog as the active dialog
            vm.ActiveDialog = playDialog;

            FileViewModel.Instance.PlayGenerators = ready.Generators;
            FileViewModel.Instance.PlayDuration = ready.Duration;

            // Generate the audio buffer
            float[] floatBuffer = PlayEngine.Go();
            FileViewModel.Instance.AudioBuffer = floatBuffer;

            // Initialize NAudio if we have audio data
            if (FileViewModel.Instance.AudioBuffer.Length > 0)
            {
                InitializePositionTimer();
                InitializeSignalLevelTimer();
                var provider = new AudioBufferProvider(
                    FileViewModel.Instance.AudioBuffer,
                    PlayTypes.SampleRate);
                FileViewModel.Instance.AudioProvider = provider;

                // Use WaveOut - AudioBufferProvider implements IWaveProvider directly
                FileViewModel.Instance.AudioOutput = new NAudio.Wave.WaveOut();
                FileViewModel.Instance.AudioOutput.Init(provider);

                // Set up playback stopped event
                FileViewModel.Instance.AudioOutput.PlaybackStopped += (s, e) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        FileViewModel.Instance.IsPlaying = false;
                    });
                };
            }

            playDialog.ShowDialog();

            // Clean up when dialog closes
            positionTimer?.Stop(); // kill the timer when the dialog closes
            FileViewModel.Instance.AudioOutput?.Stop();
            FileViewModel.Instance.AudioOutput?.Dispose();
            FileViewModel.Instance.AudioOutput = null;
        }
        #endregion
        #region Play Dialog Commands
        public void Rewind()
        {
            Debug.WriteLine("Rewind command being executed.");

            if (vm.AudioOutput != null && vm.AudioProvider is AudioBufferProvider provider)
            {
                vm.AudioOutput.Pause();
                provider.Reset();
                vm.CurrentPlayPosition = 0;
                vm.Status = "Rewound to start.";
            }
            else
            {
                vm.Status = "No audio loaded.";
            }
        }
        public void PlayPause()
        {
            Debug.WriteLine("Play/Pause command being executed.");

            if (vm.AudioOutput == null)
            {
                vm.Status = "No audio loaded.";
                return;
            }

            if (vm.IsPlaying)
            {
                vm.AudioOutput.Pause();
                vm.IsPlaying = false;
                vm.Status = "Playback paused.";
            }
            else
            {
                vm.AudioOutput.Play();
                vm.IsPlaying = true;
                vm.Status = "Playing...";
            }
        }
        public void ShowVoices()
        {
            Debug.WriteLine($"[ShowVoices] Called. Current ShowVoices state: {vm.ShowVoices}");
            vm.ShowVoices = !vm.ShowVoices;
            Debug.WriteLine($"[ShowVoices] Toggled ShowVoices state to: {vm.ShowVoices}");

            if (vm.ShowVoices)
            {
                Debug.WriteLine("[ShowVoices] Creating VoiceDialog...");
                FileViewModel.Instance.VoiceDialog = new VoiceDialog
                {
                    DataContext = FileViewModel.Instance,
                    Owner = vm.ActiveDialog,
                    Width = 300,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };

                Debug.WriteLine($"[ShowVoices] VoiceDialog created. Owner: {vm.ActiveDialog?.GetType().Name ?? "null"}");

                // Position it in the upper right corner of the screen containing the owner
                if (vm.ActiveDialog != null)
                {
                    var ownerHandle = new System.Windows.Interop.WindowInteropHelper(vm.ActiveDialog).Handle;
                    var ownerScreen = System.Windows.Forms.Screen.FromHandle(ownerHandle);

                    Debug.WriteLine($"[ShowVoices] Owner handle: {ownerHandle}");
                    Debug.WriteLine($"[ShowVoices] Owner screen working area: {ownerScreen.WorkingArea}");

                    double left = ownerScreen.WorkingArea.Right - 310;
                    double top = ownerScreen.WorkingArea.Top + 10;

                    Debug.WriteLine($"[ShowVoices] Calculated position - Left: {left}, Top: {top}");

                    FileViewModel.Instance.VoiceDialog.Left = left;
                    FileViewModel.Instance.VoiceDialog.Top = top;

                    Debug.WriteLine($"[ShowVoices] Set VoiceDialog position - Left: {FileViewModel.Instance.VoiceDialog.Left}, Top: {FileViewModel.Instance.VoiceDialog.Top}");
                }

                Debug.WriteLine("[ShowVoices] Calling Show()...");
                FileViewModel.Instance.VoiceDialog.Show();
                Debug.WriteLine("[ShowVoices] Show() completed");
            }
            else
            {
                Debug.WriteLine("[ShowVoices] Closing VoiceDialog...");
                FileViewModel.Instance.VoiceDialog?.Close();
            }
        }
        #endregion
        #region Play Timers
        public DispatcherTimer? positionTimer;
        public void InitializePositionTimer()
        {
            positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
            positionTimer.Tick += (s, e) =>
            {
                if (FileViewModel.Instance.AudioProvider is AudioBufferProvider provider)
                {
                    // Use UpdatePositionFromTimer to avoid triggering seek when timer updates position
                    FileViewModel.Instance.UpdatePositionFromTimer(provider.CurrentPosition);
                }
            };
            positionTimer.Start();
            Debug.WriteLine($"Position timer started at {DateTime.Now}");
        }
        public DispatcherTimer? signalTimer;
        public void InitializeSignalLevelTimer()
        {
            signalTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
            signalTimer.Tick += (s, e) =>
            {
                // Read that last second of audio data to calculate the signal level for each channel and update the UI
                // the max
                if (FileViewModel.Instance.AudioProvider is AudioBufferProvider provider)
                {
                    // update the signal levels in the UI
                    double volumeLevel = Math.Pow(10.0, (FileViewModel.Instance.AudioVolume - 10) / 20.0);
                    double[] signalLevels = provider.GetRecentSignalLevels(volumeLevel);
                    double[] peakLevels = provider.GetRecentPeakLevels(volumeLevel);
                    FileViewModel.Instance.SignalLevels = signalLevels;
                    FileViewModel.Instance.MaxSignalLevels = peakLevels;
                }
            };
            signalTimer.Start();
            Debug.WriteLine($"Signal Timer timer started at {DateTime.Now}");
        }
        #endregion
    }
}


