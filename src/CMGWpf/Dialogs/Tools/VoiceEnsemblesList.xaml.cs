using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Windows;
namespace CMGWpf.Dialogs
{
    /// <summary>
    /// Interaction logic for VoiceEnsemblesList.xaml
    /// </summary>
    public partial class VoiceEnsemblesList : Window
    {
        public ObservableCollection<EnsembleView.VoiceEnsemblesListType> List { get; set; } = new ObservableCollection<EnsembleView.VoiceEnsemblesListType>();
        public string ListTitle { get; set; } = string.Empty;
        public VoiceEnsemblesList(string name, ObservableCollection<EnsembleView.VoiceEnsemblesListType> list)
        {
            InitializeComponent();
            DataContext = EnsembleView.Instance;
            List = list;
            ListTitle = $"Ensemble list for voice '{name}'";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
