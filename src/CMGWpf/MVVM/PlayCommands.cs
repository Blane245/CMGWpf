using CMGWpf.PlayFunctions;
using CMGWpf.Services;
using CMGWpf.Types;
using CMGWpf.View;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Win32;
using NAudio.Lame;
using NAudio.Wave;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CMGWpf.MVVM
{
    public class PlayCommands(PlayViewModel vm)
    {
        private readonly PlayViewModel vm = vm;
        #region Play Dialog Commands
        public void Rewind()
        {
            Debug.WriteLine("Rewind command being executed.");

            if (vm.AudioOutput != null && vm.AudioProvider is AudioBufferProvider provider)
            {
                vm.AudioOutput.Pause();
                provider.Reset();
                vm.CurrentPlayPosition = 0;
            }
        }
        public void PlayPause()
        {
            Debug.WriteLine("Play/Pause command being executed.");

            if (vm.AudioOutput == null) return;

            if (vm.IsPlaying)
            {
                vm.AudioOutput.Pause();
                vm.IsPlaying = false;
            }
            else
            {
                vm.AudioOutput.Play();
                vm.IsPlaying = true;
            }
        }
        public void ShowVoicesToggle(PlayDialog dialog)
        {
            vm.ShowVoices = !vm.ShowVoices;
            if (vm.ShowVoices)
            {
                vm.VoiceDialog = new VoiceDialog
                {
                    DataContext = PlayViewModel.Instance,
                    Owner = dialog,
                    Width = 300,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };

                // Position it in the upper right corner of the screen containing the owner
                var ownerHandle = new System.Windows.Interop.WindowInteropHelper(dialog).Handle;
                var ownerScreen = System.Windows.Forms.Screen.FromHandle(ownerHandle);
                double left = ownerScreen.WorkingArea.Right - 310;
                double top = ownerScreen.WorkingArea.Top + 10;
                vm.VoiceDialog.Left = left;
                vm.VoiceDialog.Top = top;
                vm.VoiceDialog.Show();
            }
            else vm.VoiceDialog?.Close();
        }

        public void RecordAudio(Window owner)
        {
            Debug.WriteLine("Record Audio command being executed.");

            if (vm.AudioBuffer == null || vm.AudioBuffer.Length == 0)
            {
                MessageBox.Show("No audio to record. Please play a composition first.", "No Audio", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get the record format from GlobalService
            string format = GlobalService.Instance.RecordFormat.ToLower();

            string fileNameRoot = string.IsNullOrWhiteSpace(FileViewModel.Instance.FileName)
                ? "Composition"
                : Path.GetFileNameWithoutExtension(FileViewModel.Instance.FileName);

            // Create SaveFileDialog
            SaveFileDialog saveDialog = new()
            {
                Title = "Save Audio Recording",
                Filter = format switch
                {
                    "wav" => "WAV Files (*.wav)|*.wav|All Files (*.*)|*.*",
                    "mp3" => "MP3 Files (*.mp3)|*.mp3|All Files (*.*)|*.*",
                    _ => "All Files (*.*)|*.*"
                },
                DefaultExt = format,
                FileName = $"{fileNameRoot}.{format}"
            };

            if (saveDialog.ShowDialog(owner) != true)
                return;

            string filePath = saveDialog.FileName;

            // Check if file exists and ask for confirmation
            //if (File.Exists(filePath))
            //{
            //    var result = MessageBox.Show(
            //        $"The file '{Path.GetFileName(filePath)}' already exists. Do you want to overwrite it?",
            //        "Confirm Overwrite",
            //        MessageBoxButton.YesNo,
            //        MessageBoxImage.Warning);

            //    if (result != MessageBoxResult.Yes)
            //        return;
            //}

            try
            {
                // Save the audio based on format
                switch (format)
                {
                    case "wav":
                        SaveAsWav(filePath);
                        break;
                    case "mp3":
                        SaveAsMp3(filePath);
                        break;
                    default:
                        vm.Messages = [new Message() { Text = $"Unsupported format: {format}", Error = true }];
                        return;
                }

                vm.Messages = [new Message() { Text = $"Audio saved successfully to:\n{filePath}", Error = false }];
            }
            catch (Exception ex)
            {
                vm.Messages = [new Message() { Text = $"Error saving audio:\n{ex.Message}", Error = true }];
                Debug.WriteLine($"Error saving audio: {ex}");
            }
        }

        public async void RecordVideo(Window owner)
        {
            Debug.WriteLine("Record Video command being executed.");

            if (vm.AudioBuffer == null || vm.AudioBuffer.Length == 0)
            {
                MessageBox.Show("No video to record. Please play a composition first.", "No Video", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if owner is PlayDialog to access Sound Roll
            if (owner is not PlayDialog playDialog)
            {
                MessageBox.Show("Video recording can only be initiated from the Play dialog.", "Invalid Context", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string fileNameRoot = string.IsNullOrWhiteSpace(FileViewModel.Instance.FileName)
                ? "Composition"
                : Path.GetFileNameWithoutExtension(FileViewModel.Instance.FileName);

            // Create SaveFileDialog
            SaveFileDialog saveDialog = new()
            {
                Title = "Save Video Recording",
                Filter = "MP4 Video (*.mp4)|*.mp4|All Files (*.*)|*.*",
                DefaultExt = "mp4",
                FileName = $"{fileNameRoot}.mp4"
            };

            if (saveDialog.ShowDialog(owner) != true)
                return;

            string filePath = saveDialog.FileName;

            // Create progress dialog with cancel button
            var progressDialog = new VideoProgressDialog
            {
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            bool wasPlaying = vm.IsPlaying;
            if (wasPlaying)
            {
                vm.AudioOutput?.Pause();
                vm.IsPlaying = false;
            }

            try
            {
                progressDialog.Show();

                // Generate video asynchronously with progress and cancellation support
                bool success = await GenerateVideoAsync(playDialog, filePath, progressDialog);

                if (success)
                {
                    vm.Messages = [new Message() { Text = $"Video saved successfully to:\n{filePath}", Error = false }];
                }
                else
                {
                    vm.Messages = [new Message() { Text = "Video generation was cancelled.", Error = false }];
                }
            }
            catch (Exception ex)
            {
                vm.Messages = [new Message() { Text = $"Error saving video:\n{ex.Message}", Error = true }];
                Debug.WriteLine($"Error saving video: {ex}");
            }
            finally
            {
                progressDialog?.Close();
            }
        }

        private void SaveAsWav(string filePath)
        {
            const int sampleRate = PlayTypes.SampleRate;
            const int channels = 2; // Stereo

            using var writer = new WaveFileWriter(filePath, WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels));
            writer.WriteSamples(vm.AudioBuffer, 0, vm.AudioBuffer.Length);
        }

        private void SaveAsMp3(string filePath)
        {
            const int sampleRate = PlayTypes.SampleRate;
            const int channels = 2; // Stereo

            byte[] wavBytes;

            // Create a temporary WAV in memory with 32-bit IEEE float format
            using (var memoryStream = new MemoryStream())
            {
                // Write the float buffer as a WAV to memory using IEEE float format
                using (var waveWriter = new WaveFileWriter(memoryStream, WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels)))
                {
                    waveWriter.WriteSamples(vm.AudioBuffer, 0, vm.AudioBuffer.Length);
                }

                // Get the bytes before the stream is disposed
                wavBytes = memoryStream.ToArray();
            }

            // Create a new stream from the bytes and convert to MP3
            using var wavStream = new MemoryStream(wavBytes);
            using var reader = new WaveFileReader(wavStream);
            using var mp3Writer = new LameMP3FileWriter(filePath, reader.WaveFormat, LAMEPreset.STANDARD);
            reader.CopyTo(mp3Writer);
        }

        private async Task<bool> GenerateVideoAsync(PlayDialog playDialog, string outputPath, VideoProgressDialog progressDialog)
        {
            const int frameRate = 30;
            const int sampleRate = PlayTypes.SampleRate;

            double duration = vm.PlayDuration;
            int totalFrames = (int)(duration * frameRate);

            progressDialog.SetTotalFrames(totalFrames);

            // Create temp directory for audio only (frames stay in memory)
            string tempDir = Path.Combine(Path.GetTempPath(), $"CMGVideo_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Save audio to temporary WAV file (PCM 16-bit for maximum compatibility)
                string tempAudioPath = Path.Combine(tempDir, "audio.wav");
                await Task.Run(() => SaveAsWavPcm16(tempAudioPath));

                // Early return if cancelled - finally block still executes for cleanup
                if (progressDialog.IsCancelled)
                    return false;

                // PRE-RENDER the entire sound roll canvas once to avoid repeated WPF layout costs
                progressDialog.SetStatus("Pre-rendering sound roll...");

                RenderTargetBitmap? fullCanvasBitmap = null;
                int viewportWidth = 0;
                int viewportHeight = 0;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (playDialog.FindName("SoundRollScrollViewer") is System.Windows.Controls.ScrollViewer scrollViewer &&
                        scrollViewer.Content is System.Windows.Controls.Canvas canvas)
                    {
                        viewportWidth = (int)scrollViewer.ViewportWidth;
                        viewportHeight = (int)scrollViewer.ViewportHeight;

                        // Render the ENTIRE canvas once (this is expensive but only happens once)
                        int fullWidth = (int)Math.Ceiling(canvas.Width);
                        int fullHeight = (int)Math.Ceiling(canvas.Height);

                        fullCanvasBitmap = new RenderTargetBitmap(
                            fullWidth,
                            fullHeight,
                            96, 96,
                            PixelFormats.Pbgra32);

                        fullCanvasBitmap.Render(canvas);
                        fullCanvasBitmap.Freeze(); // Make the bitmap thread-safe for background processing
                    }
                }, System.Windows.Threading.DispatcherPriority.Normal);

                if (fullCanvasBitmap == null || progressDialog.IsCancelled)
                    return false;

                // Now extract frames from the pre-rendered bitmap (FAST - no WPF layout!)
                progressDialog.SetStatus("Generating frames...");
                List<byte[]> frameData = new List<byte[]>(totalFrames);

                await Task.Run(() =>
                {
                    for (int frameIndex = 0; frameIndex < totalFrames; frameIndex++)
                    {
                        if (progressDialog.IsCancelled)
                            return;

                        double timePosition = frameIndex / (double)frameRate;
                        double scrollOffset = SoundRollBuilder.TimeToX(timePosition);

                        // Extract a viewport-sized rectangle from the pre-rendered bitmap
                        byte[] framePng = ExtractFrameFromBitmap(
                            fullCanvasBitmap, 
                            (int)scrollOffset, 
                            0, 
                            viewportWidth, 
                            viewportHeight);

                        frameData.Add(framePng);

                        // Update progress on UI thread periodically
                        if (frameIndex % 10 == 0)
                        {
                            int current = frameIndex;
                            Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                vm.CurrentPlayPosition = current / (double)frameRate;
                                progressDialog.UpdateProgress(current + 1);
                            }, System.Windows.Threading.DispatcherPriority.Background);
                        }
                    }
                });

                // Final progress update
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    progressDialog.UpdateProgress(totalFrames);
                });

                if (progressDialog.IsCancelled)
                    return false;

                // Write frames to disk just before encoding
                progressDialog.SetStatus("Writing frames to disk...");
                string frameFilePattern = Path.Combine(tempDir, "frame_{0:D4}.png");

                await Task.Run(() =>
                {
                    for (int i = 0; i < frameData.Count; i++)
                    {
                        if (progressDialog.IsCancelled)
                            return;

                        File.WriteAllBytes(string.Format(frameFilePattern, i), frameData[i]);
                    }
                });

                // Clear frame data from memory
                frameData.Clear();

                if (progressDialog.IsCancelled)
                    return false;

                // Encode video with FFmpeg
                progressDialog.SetStatus("Encoding video...");

                await Task.Run(() =>
                {
                    string frameInputPattern = Path.Combine(tempDir, "frame_%04d.png");

                    FFMpegArguments
                        .FromFileInput(frameInputPattern, false, options => options
                            .WithFramerate(frameRate))
                        .AddFileInput(tempAudioPath)
                        .OutputToFile(outputPath, true, options => options
                            .WithVideoCodec(VideoCodec.LibX264)
                            .WithConstantRateFactor(23)
                            .WithAudioCodec(AudioCodec.Aac)
                            .WithAudioBitrate(192)
                            .WithAudioSamplingRate(sampleRate)
                            .WithCustomArgument("-profile:v baseline")
                            .WithCustomArgument("-pix_fmt yuv420p")
                            .WithFastStart())
                        .ProcessSynchronously();
                });

                // If cancelled during encoding, FFmpeg still completed - delete the output file
                if (progressDialog.IsCancelled && File.Exists(outputPath))
                {
                    try
                    {
                        File.Delete(outputPath);
                        Debug.WriteLine($"Deleted output file after cancellation: {outputPath}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to delete output file: {ex.Message}");
                    }
                }

                // Return true only if we weren't cancelled
                return !progressDialog.IsCancelled;
            }
            finally
            {
                // Clean up temp directory with retry logic
                await CleanupTempDirectoryAsync(tempDir);
            }
        }

        private async Task CleanupTempDirectoryAsync(string tempDir)
        {
            if (!Directory.Exists(tempDir))
                return;

            try
            {
                // Give file system time to release locks
                await Task.Delay(100);

                // Try deleting the directory (with retry logic)
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                        Debug.WriteLine($"Temp directory cleaned up: {tempDir}");
                        return;
                    }
                    catch (IOException) when (attempt < 2)
                    {
                        // Files might still be locked, wait and retry
                        await Task.Delay(500);
                    }
                }

                // If directory deletion failed, try deleting individual files
                Debug.WriteLine($"Attempting to delete individual files in: {tempDir}");
                foreach (var file in Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to delete file {file}: {ex.Message}");
                    }
                }

                // Try one more time to delete the directory
                await Task.Delay(500);
                Directory.Delete(tempDir, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to delete temp directory {tempDir}: {ex.Message}");
                Debug.WriteLine("Temp files may need manual cleanup.");
            }
        }

        private byte[] RenderFrameToPngBytes(System.Windows.Controls.ScrollViewer scrollViewer)
        {
            // Get the viewport dimensions (visible area only)
            int width = (int)scrollViewer.ViewportWidth;
            int height = (int)scrollViewer.ViewportHeight;

            if (width <= 0 || height <= 0)
                return Array.Empty<byte>();

            // Get the canvas inside the scroll viewer
            if (scrollViewer.Content is not System.Windows.Controls.Canvas canvas)
                return Array.Empty<byte>();

            // Create a visual brush that shows only the visible portion
            VisualBrush visualBrush = new VisualBrush(canvas)
            {
                // Calculate which portion of the canvas is currently visible
                Viewbox = new Rect(
                    scrollViewer.HorizontalOffset,  // X offset in canvas coordinates
                    scrollViewer.VerticalOffset,    // Y offset in canvas coordinates
                    width,                          // Width of visible area
                    height                          // Height of visible area
                ),
                ViewboxUnits = BrushMappingMode.Absolute,
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top
            };

            // Create a drawing visual with the viewport dimensions
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                // Draw only the visible portion at the actual size
                drawingContext.DrawRectangle(visualBrush, null, new Rect(0, 0, width, height));
            }

            // Render to bitmap at viewport size
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                width,
                height,
                96, // DPI
                96,
                PixelFormats.Pbgra32);

            renderBitmap.Render(drawingVisual);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using var memoryStream = new MemoryStream();
            encoder.Save(memoryStream);
            return memoryStream.ToArray();
        }

        private byte[] ExtractFrameFromBitmap(RenderTargetBitmap sourceBitmap, int x, int y, int width, int height)
        {
            // Clamp to source bitmap bounds
            int sourceWidth = sourceBitmap.PixelWidth;
            int sourceHeight = sourceBitmap.PixelHeight;

            x = Math.Clamp(x, 0, sourceWidth - width);
            y = Math.Clamp(y, 0, sourceHeight - height);
            width = Math.Min(width, sourceWidth - x);
            height = Math.Min(height, sourceHeight - y);

            if (width <= 0 || height <= 0)
                return Array.Empty<byte>();

            // Create a cropped bitmap from the source
            CroppedBitmap croppedBitmap = new CroppedBitmap(sourceBitmap, new Int32Rect(x, y, width, height));

            // Encode to PNG
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));

            using var memoryStream = new MemoryStream();
            encoder.Save(memoryStream);
            return memoryStream.ToArray();
        }

        private void SaveAsWavPcm16(string filePath)
        {
            const int sampleRate = PlayTypes.SampleRate;
            const int channels = 2; // Stereo

            // Convert float samples to 16-bit PCM for maximum compatibility
            short[] pcmSamples = new short[vm.AudioBuffer.Length];
            for (int i = 0; i < vm.AudioBuffer.Length; i++)
            {
                // Clamp to [-1, 1] and convert to 16-bit signed integer
                float sample = Math.Clamp((float)vm.AudioBuffer[i], -1.0f, 1.0f);
                pcmSamples[i] = (short)(sample * 32767f);
            }

            // Write as 16-bit PCM WAV
            using var writer = new WaveFileWriter(filePath, new WaveFormat(sampleRate, 16, channels));
            writer.WriteSamples(pcmSamples, 0, pcmSamples.Length);
        }
        #endregion

    }
}
