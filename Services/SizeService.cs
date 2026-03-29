using System.Windows;

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
            set { windowWidth = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayWidth)); }
        }
        private double windowHeight;
        public double WindowHeight
        {
            get { return windowHeight; }
            set { windowHeight = value; OnPropertyChanged(); OnPropertyChanged(nameof(BodyHeight)); }
        }

        private SizeService()
        {
            // Initialize with reasonable default values to avoid negative dimensions during startup
            windowWidth = 800;
            windowHeight = 450;
        }
        public GridLength ControlWidth { get; } = new(200);
        public GridLength DisplayWidth
        {
            get { return new GridLength(WindowWidth - ControlWidth.Value); }
        }
        public GridLength MenuHeight { get; } = new(45);
        public GridLength TimeLineHeight { get; } = new(45);
        public GridLength FooterHeight { get; } = new(45);
        public GridLength TrackHeight { get; } = new(100);
        public GridLength BodyHeight
        {
            get { return new GridLength(WindowHeight - MenuHeight.Value - TimeLineHeight.Value - FooterHeight.Value); }
        }
        public GridLength TableColumnWidth { get { return new GridLength(DisplayWidth.Value / 12); } }
    }
}
