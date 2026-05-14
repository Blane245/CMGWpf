using CMGDBEditor.Model;
using CMGDBEditor.MVVM;
using CMGDBEditor.Panels;
using CMGWpf.SoundFont_2;
using CMGWpf.Types;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace CMGDBEditor.View
{
    public class EnsembleView : ViewModelBase
    {

        public EnsembleView()
        {
        }
        public void NotifyPropertyChanged(string name)
        {
            OnPropertyChanged(name);
        }
        private string modifyMode = "Add";
        public string ModifyMode
        {
            get { return modifyMode; }
            set { modifyMode = value; OnPropertyChanged(); }
        }
        // serves as the new for either a voice or an ensemble when adding or editing. When the user clicks submit, the properties of this object will be validated and then either added to the database or used to modify an existing entry in the database.
        private string newEnsembleName = "";
        public string NewEnsembleName
        {
            get { return newEnsembleName; }
            set { newEnsembleName = value; OnPropertyChanged(); }
        }
        private string newVoiceName = "";
        public string NewVoiceName
        {
            get { return newVoiceName; }
            set { newVoiceName = value; OnPropertyChanged(); }
        }
        public static ObservableCollection<CMGWpf.Model.Generators.StochasticTypes.TIMBRE> TimbreOptions => new(Enum.GetValues<CMGWpf.Model.Generators.StochasticTypes.TIMBRE>());
        private ObservableCollection<string> soundFontFileNames = new ObservableCollection<string>();
        // load the list of soundfont files from the CMGSoundFontLocation setting 
        public void LoadSoundFontFileNames()
        {
            soundFontFileNames.Clear();
            string soundFontFileLocation = CMGWpf.Properties.Settings.Default.CMGSoundFontLocation;
            if (Directory.Exists(soundFontFileLocation))
            {
                string[] files = Directory.GetFiles(soundFontFileLocation, "*.sf2");
                foreach (string file in files)
                {
                    soundFontFileNames.Add(Path.GetFileName(file));
                }
            }
        }
        public ObservableCollection<string> SoundFontFileNames
        {
            get { if (soundFontFileNames.Count == 0)
                    LoadSoundFontFileNames();
                return soundFontFileNames; }
        }
        private ObservableCollection<string> _presetNames = new ObservableCollection<string>();
        public ObservableCollection<string> PresetNames
        {
            get => _presetNames;
            set { _presetNames = value; OnPropertyChanged(); }
        }
        private string newSoundFontFile = "";
        public string NewSoundFontFile
        {
            get { return newSoundFontFile; }
            // when the soundfont file changes, load it into the SF buffer and get the list of presets to populate the preset drop down
            set {
                newSoundFontFile = value;
                SoundFont? sf = CMGWpf.Utilities.SoundFontUtilities.GetSoundFont(newSoundFontFile);
                if (sf != null)
                {
                    ObservableCollection<string> newPresets = [];
                    foreach (var preset in sf.Presets)
                    {
                        newPresets.Add(CMGWpf.Utilities.SoundFontUtilities.BankPresetToName(preset));
                    }
                    PresetNames = newPresets;
                }

                OnPropertyChanged();
            }
        }
        private ObservableCollection<Ensemble> _ensembleList = new ObservableCollection<Ensemble>();
        public ObservableCollection<Ensemble> EnsembleList
        {
            get => _ensembleList;
            set { _ensembleList = value; OnPropertyChanged(); }
        }
        private ObservableCollection<Voice> _voiceList = new ObservableCollection<Voice>();
        public ObservableCollection<Voice> VoiceList
        {
            get => _voiceList;
            set { _voiceList = value; OnPropertyChanged(); }
        }
        public class SelectableVoiceType() : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private bool _isVoiceSelected = false;
            public bool IsVoiceSelected
            {
                get => _isVoiceSelected;
                set { _isVoiceSelected = value; OnPropertyChanged(); }
            }
            public Voice? Voice { get; set; } = null;
        }
        public class VoiceEnsemblesListType() : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            private string _name = "";
            public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
            private string _description = "";
            public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }
        }
        private ObservableCollection<VoiceEnsemblesListType> _voiceEnsemblesList = [];
        public ObservableCollection<VoiceEnsemblesListType> VoiceEnsemblesList
        { get => _voiceEnsemblesList;
            set { _voiceEnsemblesList = value; OnPropertyChanged(); } }

        private ObservableCollection<SelectableVoiceType> _selectableVoiceList = new ObservableCollection<SelectableVoiceType>();
        public ObservableCollection<SelectableVoiceType> SelectableVoiceList
        {
            get => _selectableVoiceList;
            set { _selectableVoiceList = value; OnPropertyChanged(); }
        }

        public string EnsembleEditorTitle
        {
            get
            {
                if (ModifyMode == "Add") return "Add Ensemble";
                else if (ModifyMode == "Edit") return "Edit Ensemble '" + UIEnsemble?.Name + "'";
                else return "";
            }
        }
        public string VoiceEditorTitle
        {
            get
            {
                if (ModifyMode == "Add") return "Add Voice";
                else if (ModifyMode == "Edit") return "Edit Voice '" + UIVoice?.Name + "'";
                else return "";
            }
        }
        public string EnsembleListTitle { get { return (UIVoice != null) ? $"Voice : {UIVoice.Name}" : "Unknown Voice"; } }
        private Ensemble? _UIEnsemble;
        public Ensemble? UIEnsemble
        {
            get { return _UIEnsemble; }
            set { _UIEnsemble = value; OnPropertyChanged(); }
        }
        private Voice? _UIVoice;
        public Voice? UIVoice
        {
            get { return _UIVoice; }
            set { _UIVoice = value; OnPropertyChanged(); }
        }
        private ObservableCollection<Message> _errors = new ObservableCollection<Message>();
        public ObservableCollection<Message> Errors { get { return _errors; } set { _errors = value; OnPropertyChanged(); } }
        public Message Status
        { get => MainView.Instance.Status; 
         set { MainView.Instance.Status = value; OnPropertyChanged(); } }

        private UserControl? _editorPanel;
        public UserControl? EditorPanel
        {
            get { return _editorPanel; }
            set { _editorPanel = value; OnPropertyChanged(); }
        }
        // relay commands
        private RelayCommand<object?>? _addEnsembleCommand;
        public RelayCommand<object?> AddEnsembleCommand =>
            _addEnsembleCommand ??= new RelayCommand<object?>(execute => new EnsembleCommands(this).AddEnsemble());
        private RelayCommand<Ensemble>? _editEnsembleCommand;
        public RelayCommand<Ensemble> EditEnsembleCommand =>
            _editEnsembleCommand ??= new RelayCommand<Ensemble>(ensemble => new EnsembleCommands(this).EditEnsemble(ensemble.Name));
        private RelayCommand<object?>? _submitEnsembleCommand;
        public RelayCommand<object?> SubmitEnsembleCommand =>
            _submitEnsembleCommand ??= new RelayCommand<object?>(execute => new EnsembleCommands(this).SubmitEnsemble());
        private RelayCommand<object?>? _cancelEditorCommand;
        public RelayCommand<object?> CancelEditorCommand =>
            _cancelEditorCommand ??= new RelayCommand<object?>(execute =>
            {
                EditorPanel = new BlankPanel();
            });
        private RelayCommand<Ensemble>? _deleteEnsembleCommand;
        public RelayCommand<Ensemble> DeleteEnsembleCommand =>
            _deleteEnsembleCommand ??= new RelayCommand<Ensemble>(ensemble => new EnsembleCommands(this).DeleteEnsemble(ensemble.Name));
        private RelayCommand<object?>? _listEnsemblesCommand;
        public RelayCommand<object?> ListEnsemblesCommand =>
            _listEnsemblesCommand ??= new RelayCommand<object?>(execute => new EnsembleCommands(this).ListEnsembles());
        private RelayCommand<object?>? _addVoiceCommand;
        public RelayCommand<object?> AddVoiceCommand =>
            _addVoiceCommand ??= new RelayCommand<object?>(execute => new VoiceCommands(this).AddVoice());
        private RelayCommand<object?>? _submitVoiceCommand;
        public RelayCommand<object?> SubmitVoiceCommand =>
            _submitVoiceCommand ??= new RelayCommand<object?>(execute => new VoiceCommands(this).SubmitVoice());
        private RelayCommand<Voice>? _editVoiceCommand;
        public RelayCommand<Voice> EditVoiceCommand =>
            _editVoiceCommand ??= new RelayCommand<Voice>(voice => new VoiceCommands(this).EditVoice(voice.Name));
        private RelayCommand<Voice>? _listVoiceEnsemblesCommand;
        public RelayCommand<Voice> ListVoiceEnsemblesCommand =>
            _listVoiceEnsemblesCommand ??= new RelayCommand<Voice>(voice => new VoiceCommands(this).ListVoiceEnsembles(voice.Name));
        private RelayCommand<Voice>? _deleteVoiceCommand;
        public RelayCommand<Voice> DeleteVoiceCommand =>
            _deleteVoiceCommand ??= new RelayCommand<Voice>(voice => new VoiceCommands(this).DeleteVoice(voice.Name));
        private RelayCommand<object?>? _listVoicesCommand;
        public RelayCommand<object?> ListVoicesCommand =>
            _listVoicesCommand ??= new RelayCommand<object?>(execute => new VoiceCommands(this).ListVoices());
    }
}
