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
            double displayWidth = SizeService.Instance.DisplayWidth;
            double displayHeight = SizeService.Instance.BodyHeight;
            double totalDuration = PlayViewModel.Instance.PlayDuration;

            // Calculate base canvas width, then add viewport width so we can scroll until content reaches left edge
            double baseWidth = SoundRollBuilder.CalculateCanvasWidth(totalDuration, displayWidth);
            double viewportWidth = SoundRollScrollViewer.ViewportWidth;
            ScrollRollCanvas.Width = baseWidth + viewportWidth; // Add viewport width for extra scroll space

            ScrollRollCanvas.Height = SoundRollBuilder.CalculateCanvasHeight(displayHeight);
            double canvasTime = 60 * baseWidth / displayWidth; // Use base width for time calculation
            SoundRollBuilder.BuildGrid(ScrollRollCanvas, canvasTime);
            PlayViewModel.Instance.ScrollRollWidth = baseWidth; // Store base width, not total width
            //Debug.WriteLine($"PlayDialog_Loaded. Base Width: {baseWidth}, Canvas Width: {ScrollRollCanvas.Width}, ScrollRollHeight: {ScrollRollCanvas.Height}");
            SoundRollBuilder.BuildFixedGrid(SoundRollFixedCanvas, ScrollRollCanvas.Height);
            SoundRollBuilder.AddInstrumentsToCanvas(ScrollRollCanvas, PlayViewModel.Instance.PresetColors);

            // Subscribe to scroll position changes
            PlayViewModel.Instance.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(PlayViewModel.Instance.CurrentPlayPosition))
                {
                    UpdateScrollPosition();
                }
            };
        }

        private void UpdateScrollPosition()
        {
            double playPosition = PlayViewModel.Instance.CurrentPlayPosition;

            // Calculate scroll offset using the same pixel-per-second ratio used to draw the content
            double scrollOffset = SoundRollBuilder.TimeToX(playPosition);

            // Clamp to valid range
            double maxScroll = Math.Max(0, ScrollRollCanvas.Width - SoundRollScrollViewer.ViewportWidth);
            scrollOffset = Math.Clamp(scrollOffset, 0, maxScroll);

            //Debug.WriteLine($"Position: {playPosition:F2}s, Offset: {scrollOffset:F0}, MaxScroll: {maxScroll:F0}");
            SoundRollScrollViewer.ScrollToHorizontalOffset(scrollOffset);
        }

        private void ScrollRollCanvas_Initialized(object sender, EventArgs e)
        {
        }
    }
}
