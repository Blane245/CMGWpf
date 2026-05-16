using CMGWpf.Panels.Database;
using CMGWpf.View;
using System.Windows;

namespace CMGWpf.Dialogs.Tools
{
    /// <summary>
    /// Interaction logic for EnsembleDialog.xaml
    /// </summary>
    public partial class EnsembleDialog : Window
    {
        private bool isLoaded = false;
        public EnsembleDialog()
        {
            InitializeComponent();
            DataContext = EnsembleView.Instance;
            Closing += EnsembleDialog_Closing;
            Loaded += EnsembleDialog_Loaded;
        }

        private async void EnsembleDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (isLoaded) return;
            isLoaded = true;
            if (DataContext is not EnsembleView vm) return;

            var ensembles = await Helpers.EnsembleHelpers.List();
            var voices = await Helpers.VoiceHelpers.List();
            vm.EditorPanel = new BlankPanel(); // Initialize with a blank panel

            // Ensure UI updates happen on the UI thread
            await Dispatcher.InvokeAsync(() =>
            {
                vm.EnsembleList = ensembles;
                vm.VoiceList = voices;
            });

        }

        private void EnsembleDialog_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            ToolsViewModel.Instance.EnsembleDialog = null;
        }
    }
}
