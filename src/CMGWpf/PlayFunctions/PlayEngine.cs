using CMGWpf.Model.Generators;
using CMGWpf.PlayFunctions.DSP;
using CMGWpf.PlayFunctions.Utilities;
using CMGWpf.Types;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using static CMGWpf.Types.PlayTypes;

namespace CMGWpf.PlayFunctions
{

    public static class PlayEngine
    {
        public static bool CheckActiveGenerators()
        {
            // check if any generator dialogs are currently open and return true if so. Play and Report are prevented from executing if generators are being added or edited.
            ObservableCollection<TrackViewModel>? trackViewModels = TracksViewModel.Instance.CachedTracks;
            if (trackViewModels == null) return false;
            foreach (var trackVm in trackViewModels)
            {
                var genVms = trackVm.CachedGenerators;
                if (genVms == null) continue;
                foreach (var genVm in genVms)
                {
                    if (genVm.ActiveGeneratorDialog != null) return true;
                }
            }
            return false;
        }
        public static void StartUp(Generator? generator, bool isPlay, bool isBeingEdited)
        {
            Debug.WriteLine($"{(isPlay ? "Play" : "Report")} command being executed.");

            // check if any generators or being edited and give the user the option to continue or not. Special handling is needed when play is invoked from the generator edit dialog
            if (!isBeingEdited && CheckActiveGenerators())
            {
                MessageBoxResult response = System.Windows.MessageBox.Show($"One or more generators are currently being edited. Any changes to them will not be reflected in the composition audio or sound roll. Do you wish to continue?", "Generators Being Edited", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (response == MessageBoxResult.No) return;
            }

            // Check ReadyPlay and start the play dialog if the file is ready to play
            PlayTypes.ReadyPlayOutput ready = ReadyPlay.Check(generator);
            if (ready.ErrorMessages.Count > 0)
            {
                FileViewModel.Instance.StatusMessages = ready.ErrorMessages;
                return;
            }

            // this function is used by both Play and Report. In Play it will generate the audio buffer and the sources for the report writer, and then show the PlayDialog with the sound roll visualization. In Report it will just generate the audio buffer and the sources for the report writer, show an file save dialog, and the build the htm that describes all of the active generators and their sources. The PlayDialog and ReportWriter are separate windows that can be shown independently, but they both use the same PlayViewModel to share data between them. The PlayViewModel is a singleton that holds the state of the play session, including the generators being played, the audio buffer, the current play position, and other relevant information. This allows for a clean separation of concerns between the data processing in the PlayEngine and the UI presentation in the dialogs.
            // initialize the play/record processing inputs and outputs and launch the generator conversion multi threading process.
            PlayViewModel.Instance.PlayGenerators = ready.Generators;
            PlayViewModel.Instance.PlayDuration = ready.Duration;
            PlayViewModel.Instance.FinalSignal = new AudioBufferWrapper(new double[(int)Math.Ceiling(ready.Duration) * SampleRate]);
            PlayViewModel.Instance.TimeMidiPresets = [];
            PlayViewModel.Instance.SF_Presets = [];
            PlayViewModel.Instance.InstrumentSources = [];
            Go(isPlay);
        }
        public static void Go(bool isPlay)
        {
            // this function is used by both Play and Report. In Play it will generate the audio buffer and the sources for the report writer, and then show the PlayDialog with the sound roll visualization. In Report it will just generate the audio buffer and the sources for the report writer, show an file save dialog, and the build the htm that describes all of the active generators and their sources. The PlayDialog and ReportWriter are separate windows that can be shown independently, but they both use the same PlayViewModel to share data between them. The PlayViewModel is a singleton that holds the state of the play session, including the generators being played, the audio buffer, the current play position, and other relevant information. This allows for a clean separation of concerns between the data processing in the PlayEngine and the UI presentation in the dialogs.
            PlayDialog? playDialog = null;
            SaveFileDialog? saveFileDialog = null;
            if (isPlay)
            {
                playDialog = new()
                {
                    DataContext = PlayViewModel.Instance,
                    Owner = System.Windows.Application.Current.MainWindow
                };

            }
            else
            {
                string fileNameRoot = string.IsNullOrWhiteSpace(FileViewModel.Instance.FileName) ? "Composition" : Path.GetFileNameWithoutExtension(FileViewModel.Instance.FileName);

                saveFileDialog = new()
                {
                    Filter = "HTML files (*.htm)|*.htm|All files (*.*)|*.*",
                    DefaultExt = "htm",
                    AddExtension = true,
                    Title = "Save Report",
                    OverwritePrompt = true,
                    FileName = $"{fileNameRoot}.htm"
                };
                saveFileDialog.ShowDialog();
                if (string.IsNullOrEmpty(saveFileDialog.FileName)) return; // user cancelled
            }


            // kick off the tasks for each generator and have them update the global state as they go. The generators will be responsible for updating the audio buffer, the sources collection, the presets collection, and the sound roll instruments as they are being processed. The tasks will be monitored for completion and the progress will be reported to the UI. Once all of the tasks are completed, the audio buffer will be normalized and assigned to the PlayViewModel for playback or report generation. The use of multiple threads allows for a responsive UI and efficient processing of the generators, especially when there are multiple generators that can be processed in parallel.
            List<Task> tasks = new List<Task>();
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;
            foreach (var gen in PlayViewModel.Instance.PlayGenerators)
            {
                if (gen is Algorithmic || gen is Stochastic)
                {
                    // Use LongRunning hint to avoid thread pool starvation for CPU-intensive DSP work
                    Task aTask = Task.Factory.StartNew(() =>
                 {
                     switch (gen)
                     {
                         case Algorithmic:
                             SourcesFromAlgorithmic.Get((gen as Algorithmic)!);
                             break;
                         case Stochastic:
                             SourcesFromStochastic.Get((gen as Stochastic)!);
                             break;
                     }
                     try
                     {
                         if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
                     } catch (OperationCanceledException)
                     {
                         Debug.WriteLine($"Operation Canceled Exception Thrown");
                     }
                 }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    tasks.Add(aTask);
                }
            }

            // monitor the progress of the tasks to completion or cancellation once every second
            DispatcherTimer? progressTimer;
            Window? progressDialog;
            InitializeProgress();
            void InitializeProgress()
            {
                // display the progress dialog
                progressDialog = new ProgressWindow()
                {
                    DataContext = PlayViewModel.Instance,
                    Owner = System.Windows.Application.Current.MainWindow
                };
                PlayViewModel.Instance.UserCancelled = false;
                PlayViewModel.Instance.CompletedGenerators = 0;
                PlayViewModel.Instance.TotalGenerators = tasks.Count;
                PlayViewModel.Instance.ProgressMessage = $"Dispatching tasks to process {PlayViewModel.Instance.TotalGenerators} generators...";
                progressDialog.Show();

                progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                progressTimer.Tick += (s, e) => PollProgress();
                progressTimer.Start();
                // At this point we relinquish control to the progress timer to monitor the completion of the tasks and to update the UI accordingly. The user can cancel the operation at any time, which will signal the tasks to stop and will update the status messages in the UI to indicate that the operation was cancelled.
            }
            // setup a timer to monitor the progress of the tasks and update the UI accordingly. This will allow the user to see the progress of the processing and to cancel it if they wish. The timer will check the status of the tasks at regular intervals and update the progress in the PlayViewModel, which will be reflected in the UI. The user can cancel the operation at any time, which will signal the tasks to stop and will update the status messages in the UI to indicate that the operation was cancelled.
            void PollProgress()
            {
                Debug.WriteLine($"Polling progress at {DateTime.Now}, {PlayViewModel.Instance.CompletedGenerators} of {PlayViewModel.Instance.TotalGenerators} generators completed.");
                if (PlayViewModel.Instance.UserCancelled)
                {
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                    progressTimer.Stop();
                    FileViewModel.Instance.StatusMessages = new ObservableCollection<Types.Message>() { new Types.Message() { Text = $"{(isPlay ? "Play" : "Record")} processing cancelled by user." } };
                    return;
                }
                int completedTasks = tasks.Count(t => t.IsCompleted);
                int totalTasks = tasks.Count;
                PlayViewModel.Instance.ProgressMessage = $"Completed {completedTasks} of {totalTasks} generators.";
                PlayViewModel.Instance.TotalGenerators = totalTasks;
                PlayViewModel.Instance.CompletedGenerators = completedTasks;
                if (completedTasks == totalTasks)
                {
                    progressTimer.Stop();
                    progressDialog.Close();
                    // adjust the composition duration based on the length of the float buffer. divide by 2 for stereo.
                    PlayViewModel.Instance.PlayDuration = (PlayViewModel.Instance.FinalSignal != null) ? (PlayViewModel.Instance.FinalSignal.Buffer.Length / (float)(SampleRate * 2)) : 0;
                    PerformPlayReport();
                }

            }

            // once all data is available perform play or report
            void PerformPlayReport()
            {
                if (isPlay)
                {
                    // convert the final signal from the generators from double to float and normalize
                    PlayViewModel.Instance.AudioBuffer = NormalizeBuffer(PlayViewModel.Instance.FinalSignal.Buffer);
                    // prepare the palette for the sound roll
                    SoundRollBuilder.TimeMidiPresets = PlayViewModel.Instance.TimeMidiPresets;
                    PlayViewModel.Instance.PresetColors = SoundRollBuilder.DefineVoicePalette(PlayViewModel.Instance.SF_Presets);
                    // Initialize NAudio if we have audio data
                    if (PlayViewModel.Instance.AudioBuffer!.Length > 0)
                    {
                        InitializePositionTimer();
                        InitializeSignalLevelTimer();
                        var provider = new AudioBufferProvider(
                            PlayViewModel.Instance.AudioBuffer,
                            SampleRate);
                        PlayViewModel.Instance.AudioProvider = provider;

                        // Use WaveOut - AudioBufferProvider implements IWaveProvider directly
                        PlayViewModel.Instance.AudioOutput = new NAudio.Wave.WaveOut();
                        PlayViewModel.Instance.AudioOutput.Init(provider);

                        // Set up playback stopped event
                        PlayViewModel.Instance.AudioOutput.PlaybackStopped += (s, e) =>
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                PlayViewModel.Instance.IsPlaying = false;
                            });
                        };
                    }
                    playDialog!.ShowDialog();

                    // Clean up when dialog closes
                    StopTimers();
                    PlayViewModel.Instance.AudioOutput?.Stop();
                    PlayViewModel.Instance.AudioOutput?.Dispose();
                    PlayViewModel.Instance.AudioOutput = null;
                }
                else
                {
                    // Generate the report using the active generators and sources
                    ReportWriter.WriteReport(saveFileDialog!.FileName);
                    FileViewModel.Instance.StatusMessages = new ObservableCollection<Types.Message>() { new Types.Message() { Text = $"HTML report written to {saveFileDialog.FileName}." } };
                }

                return;

            }
        }

        /// <summary>
        /// Normalize the output so that the rms value of the nonzero samples becomes 0.5, but clip anything outside of -1 and +1 to prevent distortion. This is a simple normalization approach that can be improved in the future with more advanced techniques like dynamic range compression or limiting. For now it just ensures that the output is not too quiet or too loud on average, while allowing for some variation in the sample values. The buffer is converted to single precision to be compatible with the audio output system, which typically uses 32-bit float samples. 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns>A single precision array that has been normalized</returns>
        /// <exception cref="Exception"></exception>
        private static float[] NormalizeBuffer(double[] buffer)
        {
            double max = 0;
            double rms = 0;
            double sum = 0;
            int count = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                // ignore zeroes so they overload the numbers
                if (buffer[i] != 0)
                {
                    if (double.IsNaN(buffer[i])) throw new Exception($"buffer is undefined at position i={i}");
                    sum += Math.Abs(buffer[i]);
                    max = Math.Max(max, Math.Abs(buffer[i]));
                    rms += buffer[i] * buffer[i];
                    count++;
                }
            }
            float[] floatBuffer = new float[buffer.Length];
            if (count == 0)
            {
                DebugLog.Write($"***** Entire sample buffer is zero. *****");
                return floatBuffer; // return silence
            }
            double average = sum / count;
            rms = (float)Math.Sqrt(rms / count);
            // normalize using rms * 2 so that the samples at the rms value become 0.5, but clip anything outside of -1 and +1
            for (int i = 0; i < buffer.Length; i++)
            {
                floatBuffer[i] = (float)buffer[i] / (float)(rms * 2.0F);
                floatBuffer[i] = Math.Clamp(floatBuffer[i], -1.0F, 1.0F);
            }
            DebugLog.Write($"Final audio buffer normalized, average={average}, max={max}, rms={rms}");
            return floatBuffer;
        }
        #region Play Timers
        private static DispatcherTimer? positionTimer;
        public static void InitializePositionTimer()
        {
            positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
            positionTimer.Tick += (s, e) =>
            {
                if (PlayViewModel.Instance.AudioProvider is AudioBufferProvider provider)
                {
                    // Use UpdatePositionFromTimer to avoid triggering seek when timer updates position
                    PlayViewModel.Instance.UpdatePositionFromTimer(provider.CurrentPosition);
                }
            };
            positionTimer.Start();
            Debug.WriteLine($"Position timer started at {DateTime.Now}");
        }
        private static DispatcherTimer? signalTimer;
        public static void InitializeSignalLevelTimer()
        {
            signalTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
            signalTimer.Tick += (s, e) =>
            {
                // Read that last second of audio data to calculate the signal level for each channel and update the UI
                // the max
                if (PlayViewModel.Instance.AudioProvider is AudioBufferProvider provider)
                {
                    // update the signal levels in the UI
                    double volumeLevel = Math.Pow(10.0, (PlayViewModel.Instance.AudioVolume - 10) / 20.0);
                    double[] signalLevels = provider.GetRecentSignalLevels(volumeLevel);
                    double[] peakLevels = provider.GetRecentPeakLevels(volumeLevel);
                    PlayViewModel.Instance.SignalLevels = signalLevels;
                    PlayViewModel.Instance.MaxSignalLevels = peakLevels;
                }
            };
            signalTimer.Start();
            Debug.WriteLine($"Signal Timer started at {DateTime.Now}");
        }

        public static void StopTimers()
        {
            positionTimer?.Stop();
            signalTimer?.Stop();
            Debug.WriteLine($"Timers stopped at {DateTime.Now}");
        }
        #endregion

    }
}