using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CMGWpf.Panels
{
    public partial class CustomWindowChrome : UserControl
    {
        public static readonly DependencyProperty ChromeBackgroundProperty =
            DependencyProperty.Register(nameof(ChromeBackground), typeof(Brush), typeof(CustomWindowChrome), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(33, 47, 61))));

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(CustomWindowChrome), new PropertyMetadata(null));

        public static readonly DependencyProperty ChromeMenuContentProperty =
            DependencyProperty.Register(nameof(ChromeMenuContent), typeof(object), typeof(CustomWindowChrome), new PropertyMetadata(null));

        public static readonly DependencyProperty WindowTitleProperty =
            DependencyProperty.Register(nameof(WindowTitle), typeof(string), typeof(CustomWindowChrome), new PropertyMetadata(string.Empty));

        public Brush ChromeBackground
        {
            get => (Brush)GetValue(ChromeBackgroundProperty);
            set => SetValue(ChromeBackgroundProperty, value);
        }

        public ImageSource IconSource
        {
            get => (ImageSource)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public object ChromeMenuContent
        {
            get => GetValue(ChromeMenuContentProperty);
            set => SetValue(ChromeMenuContentProperty, value);
        }

        public string WindowTitle
        {
            get => (string)GetValue(WindowTitleProperty);
            set => SetValue(WindowTitleProperty, value);
        }

        public CustomWindowChrome()
        {
            InitializeComponent();

            MinimizeButton.Click += MinimizeButton_Click;
            MaximizeButton.Click += MaximizeButton_Click;
            CloseButton.Click += CloseButton_Click;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.ToggleMaximizeRestore();
                UpdateMaximizeButtonContent();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window?.Close();
        }

        private void UpdateMaximizeButtonContent()
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                MaximizeButton.Content = window.WindowState == WindowState.Maximized ? "❐" : "□";
                MaximizeButton.ToolTip = window.WindowState == WindowState.Maximized ? "Restore" : "Maximize";
            }
        }

        private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Window.GetWindow(this)?.ToggleMaximizeRestore();
                UpdateMaximizeButtonContent();
            }
            else
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    try
                    {
                        window.DragMove();
                    }
                    catch
                    {
                        // DragMove can throw if the mouse button is not pressed
                    }
                }
            }
        }

        private void DragArea_MouseMove(object sender, MouseEventArgs e)
        {
            // Placeholder for mouse move logic
        }
    }

    public static class WindowExtensions
    {
        public static void ToggleMaximizeRestore(this Window window)
        {
            window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
    }
}