using CMGWpf.Model.Database;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace CMGWpf.Panels
{
    /// <summary>
    /// Interaction logic for EnsembleEditorPanel.xaml
    /// </summary>
    public partial class EnsembleEditorPanel : UserControl
    {
        private readonly Ensemble? _vm;
        public EnsembleEditorPanel(EnsembleView vm)
        {
            InitializeComponent();
            DataContext = vm;
            _vm = vm.UIEnsemble;
            Loaded += EnsembleEditorPanel_Loaded;
        }

        private async void EnsembleEditorPanel_Loaded(object sender, RoutedEventArgs e)
        {
            // build the selectable voice list by checking which voices in the VoiceList are in the UIEnsemble's Voices collection and setting isVoiceSelected accordingly
            if (DataContext is not EnsembleView vm || vm.UIEnsemble == null) return;
            var list = new ObservableCollection<EnsembleView.SelectableVoiceType>();
            foreach (var voice in vm.VoiceList)
            {
                bool isSelected = false;
                foreach (var eVoice in vm.UIEnsemble.Voices)
                {
                    if (voice.Name == eVoice.Name)
                    {
                        isSelected = true;
                        break;
                    }
                }
                var item = new EnsembleView.SelectableVoiceType() { Voice = voice, IsVoiceSelected = isSelected };
                list.Add(item);
            }
            vm.SelectableVoiceList = list;
        }
    }
}
