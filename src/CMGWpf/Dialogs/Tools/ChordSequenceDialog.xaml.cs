using System.Windows;
using CMGWpf.View;
using CMGWpf.Panels.Tools;

namespace CMGWpf.Dialogs.Tools
{
    /// <summary>
    /// Interaction logic for ChordSequenceDialog.xaml
    /// </summary>
    public partial class ChordSequenceDialog : Window
    {
        private bool isLoaded = false;

        public ChordSequenceDialog()
        {
            InitializeComponent();
            DataContext = ChordSequenceView.Instance;
            Closing += ChordSequenceDialog_Closing;
            Loaded += ChordSequenceDialog_Loaded;
        }
        private async void ChordSequenceDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (isLoaded) return;
            isLoaded = true;
            if (DataContext is not ChordSequenceView vm) return;

            var chordSequences = await Helpers.ChordSequenceHelpers.List();
            vm.EditorPanel = new BlankPanel(); // Initialize with a blank panel

            // Ensure UI updates happen on the UI thread
            await Dispatcher.InvokeAsync(() =>
            {
                vm.ChordSequenceList = [.. chordSequences.OrderBy(sequence => sequence.Name)];
            });

        }

        private void ChordSequenceDialog_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            ToolsViewModel.Instance.ChordSequenceDialog = null;
        }

    }
}
