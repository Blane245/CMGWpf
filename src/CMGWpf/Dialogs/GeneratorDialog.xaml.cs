using CMGWpf.Model.Generators;
using CMGWpf.View;
using System.ComponentModel;
using System.Windows;

namespace CMGWpf.Dialogs
{
    /// <summary>
    /// Interaction logic for GeneratorDialog.xaml
    /// </summary>
    public partial class GeneratorDialog : Window
    {
        // we assume that this window is close by a cancel action. When it is closed for other reasons, this must be made tru to avoid restoring the UIGenerator
        public bool userCancel = true;
        public GeneratorDialog()
        {
            InitializeComponent();
            this.Closing += GeneratorDialog_Closing;
            this.Loaded += GeneratorDialog_Loaded;
        }

        private void GeneratorDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Services.GlobalService.Instance.StatusMessages.Clear();
            if (DataContext is GeneratorViewModel vm)
            {
                vm.InitializeNewTimes(vm.UIGenerator.StartTime, vm.UIGenerator.StopTime);
            }
        }

        private void GeneratorDialog_Closing(object? sender, CancelEventArgs e)
        {
            // the user has either submitted the changes, or selected the "X" or Cancel button to close the dialog. We so we restore the UIGenerator to the original generator, which is what is currently in the track.
            if (userCancel) RestoreUIGenerator();
            if (DataContext is GeneratorViewModel vm) vm.ActiveGeneratorDialog = null;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            userCancel = true;
            Close();
        }
        private void RestoreUIGenerator()
        {
            if (DataContext is GeneratorViewModel vm)
            {
                if (vm.Generator != null)
                {
                    Generator g = vm.Generator;
                    //vm.NewStartTime = g.StartTime; // this will change the UIGenerator.StopTime, which will have to be restored
                    // the cloning occuring here will restore the UI generator stop time values
                    switch (vm.Generator.ToString())
                    {
                        case "Algorithmic":
                            {
                                vm.UIGenerator = (g as Algorithmic)!.Clone(g.Parent);
                                break;
                            }
                        case "Stochastic":
                            {
                                vm.UIGenerator = (g as Stochastic)!.Clone(g.Parent);
                                break;
                            }
                        default:
                            break;
                    }
                    vm.Status = [new Types.Message() { Text = "Changes canceled.", Error = false }];
                    vm.NotifyGeneratorChanged(nameof(vm.UIGenerator));
                }
            }
        }

    }
}