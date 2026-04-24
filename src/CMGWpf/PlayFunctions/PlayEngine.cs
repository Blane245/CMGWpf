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

    //this will start the integration of the soundfont BuildVoicesForPresetAtKeyVel routine which is the first step in getting instrument smaples for DSP. It will be used in the PlayEngine and will be called when a note is played to determine which samples to use for that note. 
    public static class PlayEngine
    {
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
            PlayDialog? playDialog = null;
            SaveFileDialog? saveFileDialog = null;
            if (isPlay)
            {
                playDialog = new()
                {
                    DataContext = PlayViewModel.Instance,
                    Owner = System.Windows.Application.Current.MainWindow
                };

                // Set the PlayDialog as the active dialog
                //FileViewModel.Instance.ActiveDialog = playDialog;
            }
            else
            {
                string fileNameRoot = string.IsNullOrWhiteSpace(FileViewModel.Instance.FileName)
    ? "Composition"
    : Path.GetFileNameWithoutExtension(FileViewModel.Instance.FileName);

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

            PlayViewModel.Instance.PlayGenerators = ready.Generators;
            PlayViewModel.Instance.PlayDuration = ready.Duration;

            // Generate the audio buffer and the sources for the report writer with progress reporting
            // Show progress window instead of wait cursor
            ProgressWindow progressWindow = new()
            {
                Owner = System.Windows.Application.Current.MainWindow,
                TotalGenerators = ready.Generators.Count,
                CompletedGenerators = 0
            };

            // Create progress reporter
            Progress<GeneratorProgress> progress = new(p =>
            {
                progressWindow.CompletedGenerators = p.CompletedCount;
                progressWindow.StatusMessage = p.Message;
            });

            bool processingCompleted = false;
            float[]? floatBuffer = null;
            ObservableCollection<InstrumentSource>? sources = null;

            // Process generators on background thread to keep UI responsive
            Task.Run(() =>
            {
                try
                {
                    (floatBuffer, sources) = Go(progress, () => progressWindow.IsCancelled);
                    processingCompleted = true;
                }
                catch (OperationCanceledException)
                {
                    // User cancelled
                    processingCompleted = false;
                }
                finally
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        progressWindow.Close();
                    });
                }
            });

            // Show progress dialog modally
            progressWindow.ShowDialog();

            // Check if cancelled
            if (!processingCompleted || floatBuffer == null || sources == null)
            {
                FileViewModel.Instance.StatusMessages = [new Types.Message() { Text = "Processing cancelled by user." }];
                return;
            }

            PlayViewModel.Instance.InstrumentSources = sources;
            PlayViewModel.Instance.AudioBuffer = floatBuffer;

            if (isPlay)
            {
                // Initialize NAudio if we have audio data
                if (PlayViewModel.Instance.AudioBuffer.Length > 0)
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
                // adjust the composition duration based on the length of the float buffer.
                PlayViewModel.Instance.PlayDuration = (floatBuffer != null) ? (floatBuffer.Length / (float)(SampleRate * 2)) : 0; // divide by 2 for stereo
                playDialog!.ShowDialog();

                // Clean up when dialog closes
                positionTimer?.Stop();
                PlayViewModel.Instance.AudioOutput?.Stop();
                PlayViewModel.Instance.AudioOutput?.Dispose();
                PlayViewModel.Instance.AudioOutput = null;
            }
            else
            {
                // adjust the composition duration based on the length of the float buffer.
                PlayViewModel.Instance.PlayDuration = (floatBuffer != null) ? (floatBuffer.Length / (float)(SampleRate * 2)) : 0; // divide by 2 for stereo
                // Generate the report using the active generators and sources
                ReportWriter.WriteReport(saveFileDialog!.FileName);
                FileViewModel.Instance.StatusMessages = [new Types.Message() { Text = $"HTML report written to {saveFileDialog.FileName}." }];
            }

        }
        public static (float[], ObservableCollection<InstrumentSource>) Go(IProgress<GeneratorProgress>? progress = null, Func<bool>? isCancelled = null)
        {
            int totalSamples = (int)Math.Ceiling(PlayViewModel.Instance.PlayDuration) * PlayTypes.SampleRate;
            double[] stereoBuffer = new double[totalSamples * 2]; // the sample buffer for the entire composition with interlaced stereo
            List<SF_Preset> sF_Presets = []; // this will be populated with the sounfont/preset unique list for later assigning colors
            ObservableCollection<InstrumentSource> sources = new();
            PlayFunctions.SoundRollBuilder.ClearInstruments();

            int totalGenerators = PlayViewModel.Instance.PlayGenerators.Count;
            int completedGenerators = 0;

            foreach (Generator gen in PlayViewModel.Instance.PlayGenerators)
            {
                // Check for cancellation
                if (isCancelled?.Invoke() == true)
                {
                    throw new OperationCanceledException("Processing cancelled by user");
                }

                // Report progress
                progress?.Report(new GeneratorProgress
                {
                    CompletedCount = completedGenerators,
                    TotalCount = totalGenerators,
                    CurrentGeneratorName = gen.Name,
                    Message = $"Processing generator {completedGenerators + 1} of {totalGenerators}: {gen.Name}"
                });

                switch (gen)
                {
                    case Algorithmic:
                        _ = SourcesFromAlgorithmic.Get(gen as Algorithmic, ref stereoBuffer, sF_Presets, sources);
                        break;
                    case Stochastic:
                        _ = SourcesFromStochastic.Get(gen as Stochastic, ref stereoBuffer, sF_Presets, sources);
                        break;
                }

                completedGenerators++;
            }

            // Report completion before normalization
            progress?.Report(new GeneratorProgress
            {
                CompletedCount = completedGenerators,
                TotalCount = totalGenerators,
                Message = "Normalizing audio buffer..."
            });

            // build the color palette for the presets that are to be played based on the sF_Presets collection that was populated while processing the generators. This will be used to assign colors to the notes in the UI so that the user can see which notes correspond to which presets. For now it just shows debug output with the preset names and their assigned colors.
            PlayViewModel.Instance.PresetColors = SoundRollBuilder.DefineVoicePalette(sF_Presets);
            // normalize the stereo buffer to prevent clipping
            float[] floatBuffer = NormalizeBuffer(stereoBuffer);
            return (floatBuffer, sources);
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
        public static DispatcherTimer? positionTimer;
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
        public static DispatcherTimer? signalTimer;
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
            Debug.WriteLine($"Signal Timer timer started at {DateTime.Now}");
        }
        #endregion

    }
}