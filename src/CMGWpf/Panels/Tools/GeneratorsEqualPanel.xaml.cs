using CMGWpf.Utilities;
using CMGWpf.View;
using System.Windows.Controls;

namespace CMGWpf.Panels.Tools
{
    /// <summary>
    /// Interaction logic for GeneratorsEqualPanel.xaml
    /// </summary>
    public partial class GeneratorsEqualPanel : UserControl
    {
        public GeneratorsEqualPanel()
        {
            InitializeComponent();
            DataContext = ToolsViewModel.Instance;
            Loaded += GeneratorsEqualPanel_Loaded;
        }

        private void GeneratorsEqualPanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ToolsViewModel vm)
            {
                ListBoxHelper.BindSelectedItems(SecondaryGeneratorsListBox, vm.SecondaryGeneratorNames);
            }
        }
    }
}
