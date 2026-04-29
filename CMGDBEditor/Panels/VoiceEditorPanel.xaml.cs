using CMGDBEditor.Model;
using CMGDBEditor.View;
using System.Windows;
using System.Windows.Controls;

namespace CMGDBEditor.Panels
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
        }
    }
}
