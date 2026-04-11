using CMGWpf.View;
using System.Windows;

namespace CMGWpf.Dialogs.Tools
{
    /// <summary>
    /// Interaction logic for MeasureDurationCalculatorDialog.xaml
    /// </summary>
    public partial class MeasureDurationCalculatorDialog : Window
    {
        public MeasureDurationCalculatorDialog()
        {
            InitializeComponent();
            DataContext = ToolsViewModel.Instance;
            this.Loaded += MeasureDurationCalculatorDialog_Loaded;
        }

        private void MeasureDurationCalculatorDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Services.GlobalService.Instance.StatusMessages.Clear();
        }
    }
}
