using CMGDBEditor.View;
using System.Windows.Controls;

namespace CMGDBEditor.Layout
{
    /// <summary>
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu : UserControl
    {
        public Menu()
        {
            InitializeComponent();
            DataContext = MainView.Instance;
        }
    }
}
