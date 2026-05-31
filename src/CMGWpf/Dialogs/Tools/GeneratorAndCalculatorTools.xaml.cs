using CMGWpf.View;
using System.Windows;

namespace CMGWpf.Dialogs.Tools
{
    /// <summary>
    /// Interaction logic for GeneratorAndCalculatorTools.xaml
    /// </summary>
    public partial class GeneratorAndCalculatorTools : Window
    {
        public GeneratorAndCalculatorTools()
        {
            InitializeComponent();
            DataContext = ToolsViewModel.Instance;
            Closing += GeneratorAndCalculatorTools_Closing;
            Loaded += GeneratorAndCalculatorTools_Loaded;
        }

        private void GeneratorAndCalculatorTools_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ToolsViewModel vm)
            {
                vm.LoadStaggerGeneratorList();
                vm.StaggerAmount = 0;
            }

        }

        private void GeneratorAndCalculatorTools_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is ToolsViewModel vm) vm.ToolDialog = null;
        }
    }
}
