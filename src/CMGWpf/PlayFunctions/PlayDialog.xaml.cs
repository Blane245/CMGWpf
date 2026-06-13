using CMGWpf.Services;
using CMGWpf.View;
using System.Windows;

namespace CMGWpf.PlayFunctions
{
    /// <summary>
    /// Interaction logic for PlayDialog.xaml
    /// </summary>
    public partial class PlayDialog : Window
    {
        public PlayDialog()
        {
            InitializeComponent();
            Loaded += PlayDialog_Loaded;
            Closing += PlayDialog_Closing;
            SizeChanged += PlayDialog_SizeChanged;
        }

        private void PlayDialog_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            PlayEngine.StopTimers();
            PlayViewModel.Instance.UnregisterSignalCanvases();
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            FileViewModel.Instance.StatusMessages = [new Types.Message() { Text = "Play complete.", Error = false }];
            Close();
        }
        private void PlayDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PlayViewModel vm)
            {
                double height = SoundRollGrid.ActualHeight;
                double width = SizeService.Instance.DisplayWidth;
                DrawSoundRoll(vm, width, height);
                vm.IsPlaying = false;
            }
            
        }
        private void DrawSoundRoll(PlayViewModel vm, double width, double height)
        {
                vm.Messages = [];
                vm.RegisterSignalCanvases(LeftSignalLevel, RightSignalLevel);
                GlobalService.Instance.StatusMessages.Clear();
                double displayWidth = SizeService.Instance.DisplayWidth;

                // Get the actual rendered height of row 2 (the SoundRoll grid)
                // This accounts for the actual layout after star-sizing is applied
                double displayHeight = SoundRollGrid.ActualHeight;
                double totalDuration = PlayViewModel.Instance.PlayDuration;

                // Calculate base canvas width, then add view port width so we can scroll until content reaches left edge
                double baseWidth = SoundRollBuilder.CalculateCanvasWidth(totalDuration, displayWidth);
                double viewportWidth = SoundRollScrollViewer.ViewportWidth;
                SoundRollCanvas.Width = baseWidth + viewportWidth; // Add view port width for extra scroll space

                SoundRollCanvas.Height = SoundRollBuilder.CalculateCanvasHeight(displayHeight);
                double canvasTime = 60 * baseWidth / displayWidth; // Use base width for time calculation
                SoundRollBuilder.BuildGrid(SoundRollCanvas, canvasTime);
                PlayViewModel.Instance.SoundRollWidth = baseWidth; // Store base width, not total width
                SoundRollBuilder.BuildFixedGrid(SoundRollFixedCanvas, SoundRollCanvas.Height);
                SoundRollBuilder.AddInstrumentsToCanvas(SoundRollCanvas, PlayViewModel.Instance.VoiceColors);

                // Subscribe to scroll position changes
                PlayViewModel.Instance.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(PlayViewModel.Instance.CurrentPlayPosition))
                    {
                        UpdateSoundRollPosition();
                    }
                };

        }
        private void PlayDialog_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is PlayViewModel vm)
            {
                double width = e.NewSize.Width;
                double height = SoundRollGrid.ActualHeight;
                DrawSoundRoll(vm, width, height);
                vm.IsPlaying = false;
            }
        }

        private void UpdateSoundRollPosition()
        {
            double playPosition = PlayViewModel.Instance.CurrentPlayPosition;

            // Calculate scroll offset using the same pixel-per-second ratio used to draw the content
            double scrollOffset = SoundRollBuilder.TimeToX(playPosition);

            // Clamp to valid range
            double maxScroll = Math.Max(0, SoundRollCanvas.Width - SoundRollScrollViewer.ViewportWidth);
            scrollOffset = Math.Clamp(scrollOffset, 0, maxScroll);

            //DebugLog.Write($"Position: {playPosition:F2}s, Offset: {scrollOffset:F0}, MaxScroll: {maxScroll:F0}");
            SoundRollScrollViewer.ScrollToHorizontalOffset(scrollOffset);
        }

    }
}
