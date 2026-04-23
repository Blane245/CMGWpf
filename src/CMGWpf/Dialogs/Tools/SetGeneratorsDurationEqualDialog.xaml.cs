using CMGWpf.Utilities;
using CMGWpf.View;
using System.Windows;
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
                vm.PrimaryGeneratorName = "";
                vm.AlignTimeOption = "Start Time";
                vm.SecondaryGeneratorNames.Clear();
                ListBoxHelper.BindSelectedItems(SecondaryGeneratorsListBox, vm.SecondaryGeneratorNames);
                Services.GlobalService.Instance.StatusMessages.Clear();
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
