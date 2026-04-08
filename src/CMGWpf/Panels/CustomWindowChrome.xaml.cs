using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;

namespace CMGWpf.Panels
{
    /// <summary>
    /// Custom window chrome with logo, title, menu, and window controls
    /// </summary>
    public partial class CustomWindowChrome : UserControl
    {
        public static readonly DependencyProperty WindowTitleProperty =
            DependencyProperty.Register(nameof(WindowTitle), typeof(string), typeof(CustomWindowChrome), 
                new PropertyMetadata("CMG"));

        public static readonly DependencyProperty ChromeMenuContentProperty =
            DependencyProperty.Register(nameof(ChromeMenuContent), typeof(object), typeof(CustomWindowChrome), 
                new PropertyMetadata(null));

        public static readonly DependencyProperty ChromeBackgroundProperty =
            DependencyProperty.Register(nameof(ChromeBackground), typeof(System.Windows.Media.Brush), typeof(CustomWindowChrome), 
                new PropertyMetadata(new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 139))));

        public string WindowTitle
        {
            get => (string)GetValue(WindowTitleProperty);
            set => SetValue(WindowTitleProperty, value);
        }

        public object ChromeMenuContent
        {
            get => GetValue(ChromeMenuContentProperty);
            set => SetValue(ChromeMenuContentProperty, value);
        }

        public System.Windows.Media.Brush ChromeBackground
        {
            get => (System.Windows.Media.Brush)GetValue(ChromeBackgroundProperty);
            set => SetValue(ChromeBackgroundProperty, value);
        }

        private Window? ParentWindow => Window.GetWindow(this);

        public CustomWindowChrome()
        {
            InitializeComponent();

            MinimizeButton.Click += MinimizeButton_Click;
            MaximizeButton.Click += MaximizeButton_Click;
            CloseButton.Click += CloseButton_Click;

            // Make window control buttons clickable through WindowChrome
            WindowChrome.SetIsHitTestVisibleInChrome(MinimizeButton, true);
            WindowChrome.SetIsHitTestVisibleInChrome(MaximizeButton, true);
            WindowChrome.SetIsHitTestVisibleInChrome(CloseButton, true);

            Loaded += CustomWindowChrome_Loaded;
        }

        private void CustomWindowChrome_Loaded(object sender, RoutedEventArgs e)
        {
            if (ParentWindow != null)
            {
                ParentWindow.StateChanged += ParentWindow_StateChanged;
                UpdateMaximizeRestoreButton();
            }

            // Ensure menu is also clickable if it exists
            if (ChromeMenuContent is UIElement menuElement)
            {
                WindowChrome.SetIsHitTestVisibleInChrome(menuElement, true);
            }
        }

        private void ParentWindow_StateChanged(object? sender, EventArgs e)
        {
            UpdateMaximizeRestoreButton();
        }

        private void UpdateMaximizeRestoreButton()
        {
            if (ParentWindow != null)
            {
                if (ParentWindow.WindowState == WindowState.Maximized)
                {
                    MaximizeButton.Content = "❐"; // Restore icon
                    MaximizeButton.ToolTip = "Restore Down";
                }
                else
                {
                    MaximizeButton.Content = "□"; // Maximize icon
                    MaximizeButton.ToolTip = "Maximize";
                }
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ParentWindow != null)
            {
                ParentWindow.WindowState = WindowState.Minimized;
            }
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ParentWindow != null)
            {
                ParentWindow.WindowState = ParentWindow.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow?.Close();
        }

        private Point? _dragStartPoint;

        private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Double-click to maximize/restore
                MaximizeButton_Click(sender, e);
            }
            else
            {
                // Single click to start drag
                _dragStartPoint = e.GetPosition(this);
            }
        }

        private void DragArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _dragStartPoint.HasValue && ParentWindow != null)
            {
                // If window is maximized, restore it first
                if (ParentWindow.WindowState == WindowState.Maximized)
                {
                    var mousePos = PointToScreen(e.GetPosition(this));
                    
                    // Restore to normal size
                    ParentWindow.WindowState = WindowState.Normal;
                    
                    // Position window so cursor is in same relative position on title bar
                    var titleBarWidth = ParentWindow.ActualWidth;
                    var newLeft = mousePos.X - (titleBarWidth * (_dragStartPoint.Value.X / ActualWidth));
                    
                    ParentWindow.Left = newLeft;
                    ParentWindow.Top = mousePos.Y - _dragStartPoint.Value.Y;
                }
                
                // Drag the window
                try
                {
                    ParentWindow.DragMove();
                }
                catch
                {
                    // DragMove can throw if mouse is released during the operation
                }
            }
        }
    }
}
