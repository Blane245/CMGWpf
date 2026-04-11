using CMGWpf.Types;
using CMGWpf.Utilities;
using CMGWpf.View;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace CMGWpf.Dialogs.Tools
{
    /// <summary>
    /// Interaction logic for SetGeneratorsDurationEqualDialog.xaml
    /// </summary>
    public partial class SetGeneratorsDurationEqualDialog : Window
    {
        public SetGeneratorsDurationEqualDialog()
        {
            InitializeComponent();
            this.Loaded += SetGeneratorsDurationEqualDialog_Loaded;
            this.Closing += SetGeneratorsDurationEqualDialog_Closing;
        }

        private void SetGeneratorsDurationEqualDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Services.GlobalService.Instance.StatusMessages.Clear();
            // Bind the ListBox multi-selection to the ViewModel's SecondaryGeneratorNames collection
            if (DataContext is ToolsViewModel vm)
            {
                ListBoxHelper.BindSelectedItems(SecondaryGeneratorsListBox, vm.SecondaryGeneratorNames);
            }
        }

        private void SetGeneratorsDurationEqualDialog_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is ToolsViewModel vm) vm.ActiveSetGeneratorsDurationEqualDialog = null;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ToolsViewModel vm) vm.ActiveSetGeneratorsDurationEqualDialog = null;
            Close();
        }
    }
}
