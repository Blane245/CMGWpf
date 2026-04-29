using CMGDBEditor.Model;
using CMGDBEditor.Panels;
using CMGWpf.Properties;
using CMGWpf.Services;
using CMGWpf.Types;
using CMGWpf.Utilities;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Windows.Controls;

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
                OnPropertyChanged(nameof(ElementPanel));
                //var eResponse = await Ensemble.List();
                //var ensembles = new ObservableCollection<Ensemble>();
                //foreach (var item in eResponse)
                //{
                //    string name = item["name"]?.ToString() ?? string.Empty;
                //    string description = item["description"]?.ToString() ?? string.Empty;
                //    ensembles.Add(new Ensemble() { Name = name, Description = description });
                //}
                //EnsembleList = ensembles;
                //var vResponse = await Voice.List();
                //var voices = new ObservableCollection<Voice>();
                //foreach (var item in vResponse)
                //{
                //    string name = item["name"]?.ToString() ?? string.Empty;
                //    string description = item["description"]?.ToString() ?? string.Empty;
                //    voices.Add(new Voice() { Name = name, Description = description });
                //}
                //VoiceList = voices;
                //ElementPanel = EnsemblePanel;
                //OnPropertyChanged(nameof(EnsembleList));
                //OnPropertyChanged(nameof(VoiceList));
                //OnPropertyChanged(nameof(ElementPanel));

            });
        private RelayCommand<object>? _showNoteSequencesCommand;
        public RelayCommand<object> ShowNoteSequencesCommand =>
            _showNoteSequencesCommand ??= new RelayCommand<object>(execute =>
            {
                ElementPanel = new NoteSequencesPanel();
                OnPropertyChanged(nameof(ElementPanel));

            });
        //private RelayCommand<Ensemble>? _showEnsembleEditorCommand;
        //public RelayCommand<Ensemble> EnsembleEditorCommand =>
        //    _showEnsembleEditorCommand ??= new RelayCommand<Ensemble>(execute =>
        //    {
        //        EditorPanel = EnsembleEditorPanel;
        //        OnPropertyChanged(nameof(EditorPanel));
        //    });
        //private RelayCommand<Voice>? _showVoiceEditorCommand;
        //public RelayCommand<Voice> VoiceEditorCommand =>
        //    _showVoiceEditorCommand ??= new RelayCommand<Voice>(execute =>
        //    {
        //        EditorPanel = VoiceEditorPanel;
        //        OnPropertyChanged(nameof(EditorPanel));
        //    });
        //private RelayCommand<NoteSequence>? _showNoteSequenceEditorCommand;
        //public RelayCommand<NoteSequence> NoteSequenceEditorCommand =>
        //    _showNoteSequenceEditorCommand ??= new RelayCommand<NoteSequence>(execute =>
        //    {
        //        EditorPanel = NoteSequenceEditorPanel;
        //        OnPropertyChanged(nameof(EditorPanel));
        //    });
        private ObservableCollection<Message> _messages = new ObservableCollection<Message>();
        public ObservableCollection<Message> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                OnPropertyChanged();
            }
        }
    }
}
