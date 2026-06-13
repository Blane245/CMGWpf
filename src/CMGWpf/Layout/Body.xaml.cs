using System.Windows.Controls;

namespace CMGWpf.Layout
{
    /// <summary>
    /// The user interacts with a table of tracks, which is the main part of the UI. The Body class is the parent class for the Tracks, which are the main components of the UI.
    /// </summary>
    public partial class Body : UserControl
    {
        public Body()
        {
            InitializeComponent();
            DataContext=View.TracksViewModel.Instance;
        }
    }
}
