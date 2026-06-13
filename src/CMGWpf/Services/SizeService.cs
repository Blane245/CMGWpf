namespace CMGWpf.Services
{
    /// <summary>
    /// A singleton service that manages the current window size and calculates available space for different UI components based on predefined dimensions for controls, chrome, timeline, etc. It provides properties for the main window dimensions and calculated dimensions for display area, body area, play area, and dialog content. The service raises property change notifications when dimensions are updated to allow the UI to react accordingly.
    /// </summary>
    public class SizeService : ServiceBase
    {
        private static SizeService? _instance;
        public static SizeService Instance => _instance ??= new SizeService();

        private double windowWidth;
        public double WindowWidth
        {
            get { return windowWidth; }
            set
            {
                windowWidth = value; OnPropertyChanged();
                displayWidth = windowWidth - ControlWidth;
                OnPropertyChanged(nameof(DisplayWidth));
            }
        }
        private double windowHeight;
        public double WindowHeight
        {
            get { return windowHeight; }
            set
            {
                windowHeight = value; OnPropertyChanged();
                bodyHeight = WindowHeight - ChromeHeight - TimeLineHeight - FooterHeight;
                playHeight = windowHeight - ChromeHeight - PlayHeaderHeight;
                OnPropertyChanged(nameof(BodyHeight));
                OnPropertyChanged(nameof(DialogHeight));
            }
        }

        private SizeService()
        {
            // Initialize with reasonable default values to avoid negative dimensions during startup
            windowWidth = 800;
            windowHeight = 450;
            // Initialize calculated values
            displayWidth = windowWidth - ControlWidth;
            bodyHeight = windowHeight - ChromeHeight - TimeLineHeight - FooterHeight;
            playHeight = windowHeight - ChromeHeight - PlayHeaderHeight;
        }
        public double ControlWidth { get; } = 200;
        public double ChromeHeight { get; } = 40; // Custom window chrome height
        public double TimeLineHeight { get; } = 40;
        public double PlayHeaderHeight { get; } = 52;
        public double FooterHeight { get; } = 100;
        public double TrackHeight { get; } = 100;

        private double displayWidth = 0;
        public double DisplayWidth { get => displayWidth; }
        private double bodyHeight = 0;
        public double BodyHeight { get => bodyHeight; }
        private double playHeight = 0;

        /// <summary>
        /// Gets the available height for dialog content (WindowHeight - ChromeHeight)
        /// </summary>
        public double DialogHeight => WindowHeight - ChromeHeight;
    }
}