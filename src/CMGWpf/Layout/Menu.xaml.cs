using CMGWpf.Services;
using CMGWpf.Types;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CMGWpf.Layout
{
    /// <summary>
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu : UserControl
    {
        public Menu()
        {
            InitializeComponent();
            DataContext = FileViewModel.Instance;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Window window = Window.GetWindow(this);
                window?.DragMove();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (FileViewModel.Instance.IsDirty)
            {
                FileViewModel.Instance.StatusMessages = [new Message { Text = "File is dirty. CMG not Exited.", Error = false }];
                MessageBoxResult result = MessageBox.Show("The current file has been modified and the changes will be lost. Do you want to exit?", "File Dirty", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) { return; }
            }
                Application.Current.Shutdown();
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Normal;
                SizeService.Instance.WindowHeight = window.Height;
                SizeService.Instance.WindowHeight = window.Width;

            }
            else
            {
                window.WindowState = WindowState.Maximized;
                SizeService.Instance.WindowHeight = window.Height;
                SizeService.Instance.WindowHeight = window.Width;
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.WindowState = WindowState.Minimized;
        }

    }
}
