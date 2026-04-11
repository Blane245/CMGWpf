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
            this.Loaded += OscillatorFrequencyCalculatorDialog_Loaded;
        }

        private void OscillatorFrequencyCalculatorDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Services.GlobalService.Instance.StatusMessages.Clear();
        }
    }
}
