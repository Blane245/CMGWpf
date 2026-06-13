using CMGWpf.View;
using System.Windows.Controls;

namespace CMGWpf.Layout
{
    /// <summary>
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu : UserControl
    {
        public Menu()
        {
            InitializeComponent();
            DataContext = FileViewModel.Instance;
        }
    }
}
