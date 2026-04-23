using CMGWpf.Utilities;
using CMGWpf.View;
using System.Windows;

namespace CMGWpf.Dialogs.Tools
{
    /// <summary>
    /// Interaction logic for AlignGeneratorsDialog.xaml
    /// </summary>
    public partial class AlignGeneratorsDialog : Window
    {
        public AlignGeneratorsDialog()
        {
            InitializeComponent();
            this.Loaded += AlignGeneratorsDialog_Loaded;
            this.Closing += AlignGeneratorsDialog_Closing;
        }

        private void AlignGeneratorsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // Bind the ListBox multi-selection to the ViewModel's SecondaryGeneratorNames collection
            if (DataContext is ToolsViewModel vm)
            {
                vm.PrimaryGeneratorName = "";
                vm.AlignTimeOption = "Start Time";
                vm.SecondaryGeneratorNames.Clear();
                ListBoxHelper.BindSelectedItems(SecondaryGeneratorsListBox, vm.SecondaryGeneratorNames);
                Services.GlobalService.Instance.StatusMessages.Clear();
            }
        }

        private void AlignGeneratorsDialog_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is ToolsViewModel vm) vm.ActiveAlignGeneratorsDialog = null;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ToolsViewModel vm) vm.ActiveAlignGeneratorsDialog = null;
            Close();
        }
    }
}
