using System.Windows.Controls;

namespace CMGWpf.Layout
{
    /// <summary>
    /// The user interacts with a table of tracks, which is the main part of the UI. The Body class is the parent class for the Tracks and Track classes, which are the main components of the UI. The Body class is responsible for setting the DataContext for the Tracks and Track classes, which is the TracksViewModel instance. The TracksViewModel instance is responsible for managing the state of the tracks and providing data to the UI. The Body class also contains a reference to the CMGFile instance, which is the main data structure for the application. The CMGFile instance contains all of the tracks and their associated data. The Body class is responsible for updating the CMGFile instance when changes are made to the tracks in the UI.
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
