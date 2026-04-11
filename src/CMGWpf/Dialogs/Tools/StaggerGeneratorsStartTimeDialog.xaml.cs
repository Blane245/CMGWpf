using CMGWpf.Utilities;
using CMGWpf.View;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            // Bind the ListBox multi-selection to the ViewModel's SecondaryGeneratorNames collection
            if (DataContext is ToolsViewModel vm)
            {
                ListBoxHelper.BindSelectedItems(SecondaryGeneratorsListBox, vm.SecondaryGeneratorNames);
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
