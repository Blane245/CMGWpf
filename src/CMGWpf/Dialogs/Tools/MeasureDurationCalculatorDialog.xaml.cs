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
        }
    }
}
