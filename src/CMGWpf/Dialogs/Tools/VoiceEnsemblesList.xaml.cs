using CMGWpf.View;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
namespace CMGWpf.Dialogs
{
    /// <summary>
    /// Interaction logic for VoiceEnsemblesList.xaml
    /// </summary>
    public partial class VoiceEnsemblesList : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<EnsembleView.VoiceEnsemblesListType> _list = new();
        public ObservableCollection<EnsembleView.VoiceEnsemblesListType> List 
        { 
            get => _list; 
            set { _list = value; OnPropertyChanged(); } 
        }

        private string _listTitle = string.Empty;
        public string ListTitle 
        { 
            get => _listTitle; 
            set { _listTitle = value; OnPropertyChanged(); } 
        }

        public VoiceEnsemblesList(string name, ObservableCollection<EnsembleView.VoiceEnsemblesListType> list)
        {
            InitializeComponent();
            List = list;
            ListTitle = $"Ensemble list for voice '{name}'";
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
