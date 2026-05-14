using CMGDBEditor.Dialogs;
using CMGDBEditor.Panels;
using CMGWpf.Properties;
using CMGWpf.Services;
using CMGWpf.Types;
using CMGWpf.Utilities;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Media;

namespace CMGDBEditor.View
{
    public class MainView : ViewModelBase
    {
        private static MainView? _instance;
        public static MainView Instance => _instance ??= new MainView();

        private MainView()
        {
            GlobalService.Instance.PropertyChanged += (s, e) =>
            {
            };
        }
        public static string WindowTitle
        {
            get => "CMG Database Editor Version " + Properties.Settings.Default.Version;
        }
        private ObservableCollection<string> soundFontFileNames = SoundFontUtilities.List(Settings.Default.CMGSoundFontLocation);
        public ObservableCollection<string> SoundFontFileNames
        {
            get { return soundFontFileNames; }
            set { soundFontFileNames = value; OnPropertyChanged(); }
        }
        private Message _status = new();
        public Message Status { get => _status; 
            set {  _status = value;
                _status.Brush = (_status.Error) ? new SolidColorBrush (Colors.Red)  : new SolidColorBrush(Colors.Black);
                OnPropertyChanged(); }  }
        // panel selection either ensemble or note sequences
        private object? elementPanel = new BlankPanel();
        public object? ElementPanel
        {
            get => elementPanel;
            set
            {
                elementPanel = value;
                OnPropertyChanged();
            }
        }
        // relay commands
        private RelayCommand<object>? _showEnsembleCommand;
        public RelayCommand<object> ShowEnsembleCommand =>
            _showEnsembleCommand ??= new RelayCommand<object>(async execute =>
            {
                ElementPanel = new EnsemblePanel();
            });
        private RelayCommand<object>? _showNoteSequencesCommand;
        public RelayCommand<object> ShowNoteSequencesCommand =>
            _showNoteSequencesCommand ??= new RelayCommand<object>(execute =>
            {
                ElementPanel = new NoteSequencesPanel();

            });
        private RelayCommand<object>? _showHelpCommand;
        public RelayCommand<object> ShowHelpCommand =>
            _showHelpCommand ??= new RelayCommand<object>(execute =>
            {
                HelpDialog dialog = new HelpDialog();
                dialog.ShowDialog();
            });
    }
}
