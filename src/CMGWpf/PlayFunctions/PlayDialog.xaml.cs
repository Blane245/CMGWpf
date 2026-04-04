using CMGWpf.Services;
using CMGWpf.View;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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
            DataContext = FileViewModel.Instance;
            Loaded += PlayDialog_Loaded;
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.WindowState = WindowState.Minimized;

        }
        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.WindowState = window.WindowState == WindowState.Maximized? WindowState.Normal: WindowState.Maximized;
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void PlayDialog_Loaded(object sender, RoutedEventArgs e)
        {
            double displayWidth = SizeService.Instance.DisplayWidth.Value;
            double displayHeight = SizeService.Instance.BodyHeight.Value;
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
            FileViewModel.Instance.PropertyChanged += (s, args) =>
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
            scrollOffset = Math.Max(0, Math.Min(scrollOffset, maxScroll));

            //Debug.WriteLine($"Position: {playPosition:F2}s, Offset: {scrollOffset:F0}, MaxScroll: {maxScroll:F0}");
            SoundRollScrollViewer.ScrollToHorizontalOffset(scrollOffset);
        }

        private void ScrollRollCanvas_Initialized(object sender, EventArgs e)
        {
            //Debug.WriteLine($"ScrollRollCanvas_Initialized. Sender is {sender}.");
            //double displayWidth = SizeService.Instance.DisplayWidth.Value;
            //if (sender is not Canvas canvas) return;
            //double canvasTime = 60 * FileViewModel.Instance.ScrollRollWidth / displayWidth;
            //Debug.WriteLine($"ScrollRollCanvas_Initialized. Calculated canvasTime: {canvasTime} seconds for ScrollRollWidth: {FileViewModel.Instance.ScrollRollWidth} and displayWidth: {displayWidth}");
            //SoundRollBuilder.BuildGrid(canvas, canvasTime);
        }
    }
}
