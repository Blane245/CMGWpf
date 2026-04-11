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
        }

        private void PlayDialog_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        private void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
        private void PlayDialog_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalService.Instance.StatusMessages.Clear();
            double displayWidth = SizeService.Instance.DisplayWidth;
            double displayHeight = SizeService.Instance.BodyHeight;
            double totalDuration = PlayViewModel.Instance.PlayDuration;

            // Calculate base canvas width, then add viewport width so we can scroll until content reaches left edge
            double baseWidth = SoundRollBuilder.CalculateCanvasWidth(totalDuration, displayWidth);
            double viewportWidth = SoundRollScrollViewer.ViewportWidth;
            SoundRollCanvas.Width = baseWidth + viewportWidth; // Add viewport width for extra scroll space

            SoundRollCanvas.Height = SoundRollBuilder.CalculateCanvasHeight(displayHeight);
            double canvasTime = 60 * baseWidth / displayWidth; // Use base width for time calculation
            SoundRollBuilder.BuildGrid(SoundRollCanvas, canvasTime);
            PlayViewModel.Instance.SoundRollWidth = baseWidth; // Store base width, not total width
            SoundRollBuilder.BuildFixedGrid(SoundRollFixedCanvas, SoundRollCanvas.Height);
            SoundRollBuilder.AddInstrumentsToCanvas(SoundRollCanvas, PlayViewModel.Instance.PresetColors);

            // Subscribe to scroll position changes
            PlayViewModel.Instance.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(PlayViewModel.Instance.CurrentPlayPosition))
                {
                    UpdateSoundRollPosition();
                }
            };

            // set the play/pause mode to not playing
            if (DataContext is PlayViewModel vm) vm.IsPlaying = false;
            
        }

        private void UpdateSoundRollPosition()
        {
            double playPosition = PlayViewModel.Instance.CurrentPlayPosition;

            // Calculate scroll offset using the same pixel-per-second ratio used to draw the content
            double scrollOffset = SoundRollBuilder.TimeToX(playPosition);

            // Clamp to valid range
            double maxScroll = Math.Max(0, SoundRollCanvas.Width - SoundRollScrollViewer.ViewportWidth);
            scrollOffset = Math.Clamp(scrollOffset, 0, maxScroll);

            //Debug.WriteLine($"Position: {playPosition:F2}s, Offset: {scrollOffset:F0}, MaxScroll: {maxScroll:F0}");
            SoundRollScrollViewer.ScrollToHorizontalOffset(scrollOffset);
        }

        private void SoundRollCanvas_Initialized(object sender, EventArgs e)
        {
        }
    }
}
