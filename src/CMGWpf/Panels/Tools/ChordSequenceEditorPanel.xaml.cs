using CMGWpf.View;
using System.Windows.Controls;

namespace CMGWpf.Panels.Tools
{
    /// <summary>
    /// Interaction logic for ChordSequenceEditorPanel.xaml
    /// </summary>
    public partial class ChordSequenceEditorPanel : UserControl
    {
        public ChordSequenceEditorPanel()
        {
            InitializeComponent();
            DataContext = ChordSequenceView.Instance;
            Loaded += ChordSequenceEditorPanel_Loaded;
        }

        private void ChordSequenceEditorPanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ChordSequenceView.Instance.IsMajorEnabled = (ChordSequenceView.Instance.UIChordSequence.Items.Count == 0);
        }
    }
}
