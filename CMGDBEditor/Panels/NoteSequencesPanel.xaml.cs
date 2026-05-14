using CMGDBEditor.View;
using System.Windows.Controls;

namespace CMGDBEditor.Panels
{
    /// <summary>
    /// Interaction logic for NoteSequencesPanel.xaml
    /// </summary>
    public partial class NoteSequencesPanel : UserControl
    {
        private NoteSequencesView? vm;
        private bool isLoaded = false;
        public NoteSequencesPanel()
        {
            InitializeComponent();
            vm = new NoteSequencesView();
            DataContext = vm;
            Loaded += NoteSequencesPanel_Loaded;
            SizeChanged += NoteSequencesPanel_SizeChanged;
        }

        private async void NoteSequencesPanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (isLoaded) return;
            isLoaded = true;
            if (vm == null) return;

            var noteSequences = await Helpers.NoteSequenceHelpers.List();
            var tags = await Helpers.TagHelpers.List();
            vm.EditorPanel = new BlankPanel(); // Initialize with a blank panel

            // Ensure UI updates happen on the UI thread
            await Dispatcher.InvokeAsync(() =>
            {
                // Clear and repopulate instead of replacing the collection
                vm.NoteSequenceList.Clear();
                foreach (var noteSequence in noteSequences)
                {
                    vm.NoteSequenceList.Add(noteSequence);
                }
                vm.TagList.Clear();
                foreach (var tag in tags)
                {
                    vm.TagList.Add(tag);
                }
            });
        }

        private void NoteSequencesPanel_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[NoteSequencesPanel] Size changed - Width: {ActualWidth}, Height: {ActualHeight}");
            System.Diagnostics.Debug.WriteLine($"[NoteSequencesPanel] DesiredSize - Width: {DesiredSize.Width}, Height: {DesiredSize.Height}");
        }
    }
}
