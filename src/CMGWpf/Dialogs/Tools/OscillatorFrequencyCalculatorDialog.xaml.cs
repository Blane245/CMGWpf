using CMGWpf.View;
using System.Windows;

namespace CMGWpf.Dialogs.Tools
{
    /// <summary>
    /// Interaction logic for OscillatorFrequencyCalculatorDialog.xaml
    /// </summary>
    public partial class OscillatorFrequencyCalculatorDialog : Window
    {
        public OscillatorFrequencyCalculatorDialog()
        {
            InitializeComponent();
            DataContext = ToolsViewModel.Instance;
        }
    }
}
