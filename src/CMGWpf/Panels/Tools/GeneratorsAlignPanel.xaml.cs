using CMGWpf.Utilities;
using CMGWpf.View;
using System.Windows.Controls;

namespace CMGWpf.Panels.Tools
{
    /// <summary>
    /// Interaction logic for GeneratorsAlignPanel.xaml
    /// </summary>
    public partial class GeneratorsAlignPanel : UserControl
    {
        public GeneratorsAlignPanel()
        {
            InitializeComponent();
            DataContext = ToolsViewModel.Instance;
            Loaded += GeneratorsAlignPanel_Loaded;
        }

        private void GeneratorsAlignPanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ToolsViewModel vm) {
                ListBoxHelper.BindSelectedItems(SecondaryGeneratorsListBox, vm.SecondaryGeneratorNames);
            }
        }
    }
}
