using CMGWpf.Utilities;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Windows;
using static CMGWpf.View.ToolsViewModel;

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
                ObservableCollection<StaggerGeneratorsSelection> tempList = [];
                tempList.Clear();
                vm.StaggerAmount = 0;
                foreach (var track in FileViewModel.Instance.File.Tracks)
                {
                    foreach (var generator in track.Generators)
                    {
                        tempList.Add(new StaggerGeneratorsSelection() { TrackName = track.Name, GeneratorName = generator.Name, IsSelected = false });
                    }
                }
                vm.StaggerGeneratorList = tempList;
            }

        }

        private void GeneratorAndCalculatorTools_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is ToolsViewModel vm) vm.ThisDialog = null;
        }
    }
}
