using System.Windows;
using System.Windows.Threading;

namespace CMGWpf
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        private DispatcherTimer? _timer;

        public SplashScreen()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Shows the splash screen for the specified duration
        /// </summary>
        /// <param name="durationInSeconds">Duration to display splash screen</param>
        public void ShowSplash(double durationInSeconds = 2.0)
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(durationInSeconds)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
            
            Show();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _timer?.Stop();
            Close();
        }
    }
}
