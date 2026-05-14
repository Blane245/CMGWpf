using CMGWpf.View;
using System.Windows.Controls;

namespace CMGWpf.Panels.Tools
{
    /// <summary>
    /// Interaction logic for GeneratorsStaggerPanel.xaml
    /// </summary>
    public partial class GeneratorsStaggerPanel : UserControl
    {
        public GeneratorsStaggerPanel()
        {
            InitializeComponent();
            DataContext = ToolsViewModel.Instance;
        }
    }
}
