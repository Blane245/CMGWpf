using CMGWpf.Properties;
using CMGWpf.Utilities;
using System.Collections.ObjectModel;
using static CMGWpf.Model.Generators.StochasticTypes;

namespace CMGWpf.Services
{
    public class GlobalService : ServiceBase
    {
        private static GlobalService? _instance;
        public static GlobalService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GlobalService();
                    _instance.LoadEnsembleNamesAsync();
                }
                return _instance;
            }
        }
        private async void LoadEnsembleNamesAsync()
        {
            try
            {
                ObservableCollection<Ensemble> ensembleList = await EnsembleUtilities.GetEnsembleListAsync();
                ObservableCollection<string> names = [.. ensembleList.Select(x => x.Name).ToArray()];
                EnsembleNames = names;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading ensemble names: {ex.Message}";
            }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _isDirty = false;
        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (value != _isDirty)
                {
                    _isDirty = value;
                    OnPropertyChanged();
                    OnPropertyChanged(WindowTitle);
                }
            }
        }

        private string _fileName = string.Empty;
        public string FileName
        {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged();
                    OnPropertyChanged(WindowTitle);
                }
            }
        }
        public string WindowTitle
        {
            get { return $"CMG {Settings.Default.Version} - ({FileName}){(IsDirty ? "*" : "")}"; }
        }
        public System.Windows.Window? ActiveDialog { get; set; }

        private ObservableCollection<string> soundFontFileNames = [];
        public ObservableCollection<string> SoundFontFileNames
        {
            get { return soundFontFileNames; }
            set { soundFontFileNames = value; OnPropertyChanged(); }
        }
        private ObservableCollection<string> ensembleNames = [];
        public ObservableCollection<string> EnsembleNames
        {
            get => ensembleNames;
            set { ensembleNames = value; OnPropertyChanged(); }
        }

        public string DbServer { get; set; } = "http://blane-latitude-7290";
        //public string DbServer { get; set; } = "http://localhost";
        //public string DbServer { get; set; } = "http://192.168.1.182"; // IPv4 address
        //public string DbServer { get; set; } = "http://10.17.1.23"; // Current network IP
        public string DbPort { get; set; } = "8081";

    }
}
