using CMGDBEditor.View;
using System.Windows;
using System.Windows.Controls;

namespace CMGDBEditor.Panels
{
    /// <summary>
    /// Interaction logic for EnsemblePanel.xaml
    /// </summary>
    public partial class EnsemblePanel : UserControl
    {
        private EnsembleView? vm;
        private bool isLoaded = false;

        public EnsemblePanel()
        {
            InitializeComponent();
            vm = new EnsembleView();
            DataContext = vm;

            Loaded += EnsemblePanel_Loaded;
            SizeChanged += EnsemblePanel_SizeChanged;
        }

        private void EnsemblePanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[EnsemblePanel] Size changed - Width: {ActualWidth}, Height: {ActualHeight}");
            System.Diagnostics.Debug.WriteLine($"[EnsemblePanel] DesiredSize - Width: {DesiredSize.Width}, Height: {DesiredSize.Height}");
        }

        private async void EnsemblePanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (isLoaded) return;
            isLoaded = true;
            if (vm == null) return;

            var ensembles = await Helpers.EnsembleHelpers.List();
            var voices = await Helpers.VoiceHelpers.List();
            vm.EditorPanel = new BlankPanel(); // Initialize with a blank panel

            // Ensure UI updates happen on the UI thread
            await Dispatcher.InvokeAsync(() =>
            {

                // Clear and repopulate instead of replacing the collection
                vm.EnsembleList.Clear();
                foreach (var ensemble in ensembles)
                {
                    vm.EnsembleList.Add(ensemble);
                }

                vm.VoiceList.Clear();
                foreach (var voice in voices)
                {
                    vm.VoiceList.Add(voice);
                }

            });
        }
    }
}
