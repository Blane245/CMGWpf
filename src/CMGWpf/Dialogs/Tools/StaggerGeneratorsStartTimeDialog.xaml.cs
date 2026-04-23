using CMGWpf.Utilities;
using CMGWpf.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static CMGWpf.View.ToolsViewModel;

namespace CMGWpf.Dialogs.Tools
{
    /// <summary>
    /// Interaction logic for StaggerGeneratorsStartTimeDialog.xaml
    /// </summary>
    public partial class StaggerGeneratorsStartTimeDialog : Window
    {
        public StaggerGeneratorsStartTimeDialog()
        {
            InitializeComponent();
            this.Loaded += StaggerGeneratorsStartTimeDialog_Loaded;
            this.Closing += StaggerGeneratorsStartTimeDialog_Closing;
        }

        private void StaggerGeneratorsStartTimeDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Services.GlobalService.Instance.StatusMessages.Clear();
            // Initialize the source of the listview 
            if (DataContext is ToolsViewModel vm)
            {
                ObservableCollection<StaggerGeneratorsSelection> tempList = [];
                tempList.Clear();
                vm.PrimaryGeneratorName = "";
                vm.StaggerAmount = 0;
                vm.StaggerGeneratorList.Clear();
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

        private void StaggerGeneratorsStartTimeDialog_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is ToolsViewModel vm) vm.ActiveStaggerGeneratorsStartTimeDialog = null;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ToolsViewModel vm) vm.ActiveStaggerGeneratorsStartTimeDialog = null;
            Close();
        }
    }
}
