using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace CMGWpf.Panels
{
    /// <summary>
    /// Interaction logic for VoiceEditorPanel.xaml
    /// </summary>
    public partial class VoiceEditorPanel : UserControl
    {
        private EnsembleView _vm;
        public VoiceEditorPanel(EnsembleView vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = vm;
            Loaded += VoiceEditorPanel_Loaded;
        }

        private void VoiceEditorPanel_Loaded(object sender, RoutedEventArgs e)
        {
            // trigger the loading of the preset list 
            if (_vm != null && _vm.UIVoice != null && _vm.UIVoice.SoundFontFile != string.Empty)
            {
                var soundfont = Utilities.SoundFontUtilities.GetSoundFont(_vm.UIVoice.SoundFontFile);
                if (soundfont != null)
                {
                    var list = soundfont.Presets.Select((p) => Utilities.SoundFontUtilities.BankPresetToName(p)).ToArray();
                    list.Sort();
                    _vm.PresetNames = new ObservableCollection<string>(list);
                }
            }
        }
    }
}
