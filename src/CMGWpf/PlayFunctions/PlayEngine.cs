using CMGWpf.Model.Generators;
using CMGWpf.PlayFunctions.DSP;
using CMGWpf.PlayFunctions.Utilities;
using CMGWpf.Types;
using CMGWpf.Utilities;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using static CMGWpf.Types.PlayTypes;

namespace CMGWpf.PlayFunctions
{
    /// <summary>
    /// The driver for playing the composition or generating a report. This class handles the main workflow for processing the generators in the composition, including checking for active generator dialogs, preparing the data for playback or reporting, and managing the multi-threading of DSP processing for the generators. It also includes functions for normalizing the audio buffer and managing timers for updating the UI during playback. The PlayEngine class is designed to be responsive to user interactions, allowing for cancellation of processing and providing feedback on progress through the UI. Overall, it serves as the central hub for coordinating the various components involved in playing or reporting on the composition.
    /// </summary>
    public static class PlayEngine
    {
        /// <summary>
        /// check if any generator dialogs are currently open and return true if so. Play and Report are prevented from executing if generators are being added or edited.
        /// </summary>
        /// <returns>true if any generator dialogs are open, otherwise false.</returns>
        public static bool CheckActiveGenerators()
        {
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
        /// <summary>
        /// The main entry point for starting the play or report process. This function checks for active generator dialogs, prepares the data for playback or reporting, and initializes the multi-threading process for DSP processing of the generators. It also handles user interactions such as cancellation and provides feedback on progress through the UI. The function first checks if any generator dialogs are open and prompts the user to continue if so. Then it uses the ReadyPlay utility to check if the composition is ready for play or report, and if there are any errors it displays them in the UI. If everything is ready, it initializes the necessary data structures and calls the Go function to start the processing.
        /// </summary>
        /// <param name="generator">The generator to be processed, if any.</param>
        /// <param name="isPlay">Indicates whether the process is for playback (true) or report generation (false).</param>
        /// <param name="isBeingEdited">Indicates whether the generator is currently being edited.</param>
        public static void StartUp(Generator? generator, bool isPlay, bool isBeingEdited)
        {
            DebugLog.Write($"{(isPlay ? "Play" : "Report")} command being executed.");

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

            // initialize the play/record processing inputs and outputs and launch the generator conversion multi threading process.
            PlayViewModel.Instance.PlayGenerators = ready.Generators;
            PlayViewModel.Instance.FinalSignal = new AudioBufferWrapper(new double[(int)Math.Ceiling(ready.Duration) * SampleRate]);
            PlayViewModel.Instance.TimeMidiVoices = [];
            PlayViewModel.Instance.GeneratorVoices = [];
            PlayViewModel.Instance.InstrumentSources = [];
            PlayViewModel.Instance.CompletedGenerators = 0;
            Go(isPlay);
        }

        // 
        /// <summary>
        /// This function sets up for either playing the composition or generating a report of the composition. It uses multi-threading to handle DSP for the generators in the composition, allowing for a responsive UI and efficient processing. The function initializes the necessary dialogs and timers for monitoring progress and updating the UI, and it handles cancellation by the user. Once all processing is complete, it either starts playback or generates the report based on the user's choice.
        /// </summary>
        /// <param name="isPlay"></param>
        public static void Go(bool isPlay)
        {
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
                DialogResult openResult = saveFileDialog.ShowDialog();
                if (openResult == DialogResult.Cancel) return; // user cancelled
            }

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
                     }
                     catch (OperationCanceledException)
                     {
                         DebugLog.Write($"Operation Canceled Exception Thrown");
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

                progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
                progressTimer.Tick += (s, e) => PollProgress();
                progressTimer.Start();
            }

            // Monitor the progress of the tasks to be completed or cancelled every second, and update the progress message in the UI. If the user cancels, stop the tasks and close the progress dialog. If all tasks are completed, close the progress dialog and proceed to play or report generation.
            void PollProgress()
            {
                DebugLog.Write($"Polling progress at {DateTime.Now}, {PlayViewModel.Instance.CompletedGenerators} of {PlayViewModel.Instance.TotalGenerators} generators completed.");
                if (PlayViewModel.Instance.UserCancelled)
                {
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                    progressTimer.Stop();
                    FileViewModel.Instance.StatusMessages = new ObservableCollection<Types.Message>() { new Types.Message() { Text = $"{(isPlay ? "Play" : "Report")} processing cancelled by user." } };
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
                    PerformPlayReport();
                }

            }

            // once all data is available perform play or report
            void PerformPlayReport()
            {
                PlayViewModel.Instance.AudioBuffer = NormalizeBuffer(PlayViewModel.Instance.FinalSignal.Buffer);
                PlayViewModel.Instance.PlayDuration = PlayViewModel.Instance.AudioBuffer.Length / (SampleRate * 2);
                if (isPlay)
                {
                    // convert the final signal from the generators from double to float and normalize
                    // prepare the palette for the sound roll
                    SoundRollBuilder.TimeMidiVoices = PlayViewModel.Instance.TimeMidiVoices;
                    PlayViewModel.Instance.VoiceColors = SoundRollBuilder.DefineVoicePalette(PlayViewModel.Instance.GeneratorVoices);
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
                    FileViewModel.Instance.StatusMessages = [new() { Text = $"Play complete for {FileViewModel.Instance.FileName}." }];
                }
                else
                {
                    // Generate the report using the active generators and sources
                    ReportWriter.WriteReport(saveFileDialog!.FileName);
                    FileViewModel.Instance.StatusMessages = [new() { Text = $"HTML report written to {saveFileDialog.FileName}." }];
                }

                return;

            }
        }

        /// <summary>
        /// Normalize the output so that the rms value of the nonzero samples becomes 0.5, but clip anything outside of -1 and +1 to prevent distortion. This is a simple normalization approach that can be improved in the future with more advanced techniques like dynamic range compression or limiting. For now it just ensures that the output is not too quiet or too loud on average, while allowing for some variation in the sample values. The buffer is converted to single precision to be compatible with the audio output system, which typically uses 32-bit float samples. Also, all but a maximum of one second of silence is allowed at the end
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
            // find the last nonzero sample to allow for up to one second of silence at the end of the buffer, but not more. This prevents long tails of silence from being included in the output that can cause issues with some audio players and also ensures that the duration of the composition is accurate based on the actual audio content.
            int maxSilenceSamples = SampleRate; // allow for up to one second of silence at the end
            int lastNonZero = buffer.Length;
            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                if (buffer[i] != 0) break;
                lastNonZero--;
            }
            int allowedSilenceSamples = Math.Min(lastNonZero + maxSilenceSamples, buffer.Length);
            if (allowedSilenceSamples < buffer.Length) buffer = buffer.Take(allowedSilenceSamples).ToArray();

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
            DebugLog.Write($"Position timer started at {DateTime.Now}");
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
            DebugLog.Write($"Signal Timer started at {DateTime.Now}");
        }

        public static void StopTimers()
        {
            positionTimer?.Stop();
            signalTimer?.Stop();
            DebugLog.Write($"Timers stopped at {DateTime.Now}");
        }
        #endregion

    }
}