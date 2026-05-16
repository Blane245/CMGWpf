using CMGWpf.Panels.Database;
using CMGWpf.View;
using System.Windows;

namespace CMGWpf.Dialogs.Tools
{
    /// <summary>
    /// Interaction logic for NoteSeqeunceDialog.xaml
    /// </summary>
    public partial class NoteSequenceDialog : Window
    {
        private bool isLoaded = false;
        public NoteSequenceDialog()
        {
            InitializeComponent();
            DataContext = NoteSequencesView.Instance;
            Closing += NoteSequenceDialog_Closing;
            Loaded += NoteSequenceDialog_Loaded;
        }

        private async void NoteSequenceDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (isLoaded) return;
            isLoaded = true;
            if (DataContext is not NoteSequencesView vm) return;

            var notesequences = await Helpers.NoteSequenceHelpers.List();
            var tags = await Helpers.TagHelpers.List();
            vm.EditorPanel = new BlankPanel(); // Initialize with a blank panel

            // Ensure UI updates happen on the UI thread
            await Dispatcher.InvokeAsync(() =>
            {
                vm.NoteSequenceList = notesequences;
                vm.TagList = tags;
            });

        }

        private void NoteSequenceDialog_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            ToolsViewModel.Instance.NoteSequenceDialog = null;
        }
    }
}
