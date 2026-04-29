using CMGDBEditor.View;
using System.Windows.Controls;

namespace CMGDBEditor.Layout
{
    /// <summary>
    /// Interaction logic for Body.xaml
    /// </summary>
    public partial class Body : UserControl
    {
        public Body()
        {
            InitializeComponent();
            DataContext = MainView.Instance;
        }
    }
}
