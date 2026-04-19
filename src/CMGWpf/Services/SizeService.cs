namespace CMGWpf.Services
{
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
        }
        public double ControlWidth { get; } = 200;
        public double ChromeHeight { get; } = 40; // Custom window chrome height
        public double TimeLineHeight { get; } = 40;
        public double FooterHeight { get; } = 100;
        public double TrackHeight { get; } = 100;

        private double displayWidth = 0;
        public double DisplayWidth { get => displayWidth; }
        private double bodyHeight = 0;
        public double BodyHeight { get => bodyHeight; }

        /// <summary>
        /// Gets the available height for dialog content (WindowHeight - ChromeHeight)
        /// </summary>
        public double DialogHeight => WindowHeight - ChromeHeight;
    }
}