using CMGWpf.View;
using System.Windows.Controls;

namespace CMGWpf.Panels.Tools
{
    /// <summary>
    /// Interaction logic for TagEditorPanel.xaml
    /// </summary>
    public partial class TagEditorPanel : UserControl
    {
        public TagEditorPanel()
        {
            InitializeComponent();
            DataContext = NoteSequencesView.Instance;
        }
    }
}
