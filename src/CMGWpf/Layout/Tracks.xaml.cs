using System.Windows.Controls;

namespace CMGWpf.Layout
{
    /// <summary>
    /// Interaction logic for Tracks.xaml
    /// </summary>
    public partial class Tracks : UserControl
    {
        public Tracks()
        {
            InitializeComponent();
            DataContext = View.TracksViewModel.Instance;
        }

    }
}
