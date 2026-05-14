using CMGDBEditor.View;
using System.Collections.ObjectModel;
using System.Windows;
namespace CMGDBEditor.Dialogs
{
    /// <summary>
    /// Interaction logic for VoiceEnsemblesList.xaml
    /// </summary>
    public partial class VoiceEnsemblesList : Window
    {
        public ObservableCollection<EnsembleView.VoiceEnsemblesListType> List { get; set; } = new ObservableCollection<EnsembleView.VoiceEnsemblesListType>();
        public string ListTitle { get; set; } = string.Empty;
        public EnsembleView? CommandView { get; set; } = null;
        public VoiceEnsemblesList(EnsembleView vm, string name, ObservableCollection<EnsembleView.VoiceEnsemblesListType> list)
        {
            InitializeComponent();
            DataContext = this;
            List = list;
            ListTitle = $"Ensemble list for voice '{name}'";
            CommandView = vm;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
