using CMGWpf.Data;
using CMGWpf.Dialogs;
using CMGWpf.Model.Database;
using CMGWpf.MVVM;
using CMGWpf.Panels.Tools;
using CMGWpf.Services;
using CMGWpf.Utilities;
using CMGWpf.Types;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CMGWpf.SoundFont_2;
using System.Runtime.Intrinsics.X86;

namespace CMGWpf.View
{
    public class ChordSequenceView : ViewModelBase
    {
        private static ChordSequenceView? _instance;
        public static ChordSequenceView Instance => _instance ??= new ChordSequenceView();

        private ChordSequenceView()
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
        private ObservableCollection<Message> _errors = [];
        public ObservableCollection<Message> Errors
        {
            get { return _errors; }
            set { _errors = value; OnPropertyChanged(); }
        }
        private ObservableCollection<ChordSequence> _chordSequenceList = [];
        public ObservableCollection<ChordSequence> ChordSequenceList
        {
            get { return _chordSequenceList; }
            set { _chordSequenceList = [.. value.OrderBy(sequence => sequence.Name)]; OnPropertyChanged(); }
        }
        private string _newName = "";
        public string NewName { get => _newName; set { _newName = value; OnPropertyChanged(); } }

        public string ChordSequenceEditorTitle => ModifyMode == "Add" ? "Add Chord Sequence" : $"Modify Chord Sequence: {NewName}";
        private ChordSequence _UIChordSequence = new ChordSequence();
        public ChordSequence UIChordSequence { get=>_UIChordSequence; 
            set { _UIChordSequence = value;  OnPropertyChanged(); } }
        private ObservableCollection<ChordItem> _newChordItems = new ObservableCollection<ChordItem>();
        public ObservableCollection<ChordItem> NewChordItems
        {
            get => _newChordItems;
            set { _newChordItems = value; OnPropertyChanged(); }
        }
        private UserControl? _editorPanel;
        public UserControl? EditorPanel
        {
            get { return _editorPanel; }
            set { _editorPanel = value; OnPropertyChanged(); }
        }

        private bool isMajorEnabled = true;
        public bool IsMajorEnabled { 
            get => isMajorEnabled; 
            set { isMajorEnabled = value; OnPropertyChanged(); } }
        private bool isMajor = true;
        public bool IsMajor { get {
                return isMajor;
            } 
            set
            {
                isMajor = value;
            } }
        private ObservableCollection<string> allowedChordValues = new ObservableCollection<string>(Music.MajorProgressionStateTransitions["I"]);
        public ObservableCollection<string> AllowedChords { 
            get => allowedChordValues; 
            set { allowedChordValues = value; OnPropertyChanged(); } }
        public ObservableCollection<string> NoteNames { get; } = new ObservableCollection<string>(Music.Notes.Keys.ToList());
        private double defaultDuration = 1.0;
        public double DefaultDuration
        {
            get { return defaultDuration; }
            set { defaultDuration = value; OnPropertyChanged(); }
        }
        private int defaultInversion = 0;
        public int DefaultInversion
        {
            get { return defaultInversion; }
            set { defaultInversion = value; OnPropertyChanged(); }
        }
        private static readonly ObservableCollection<Music.WeightType> weightTypes = new(Enum.GetValues<Music.WeightType>());
        public static ObservableCollection<Music.WeightType> WeightTypes { get => weightTypes; }
        private Music.WeightType defaultWeight = Music.WeightType.mf;
        public Music.WeightType DefaultWeight
        {
            get { return defaultWeight; }
            set { defaultWeight = value; OnPropertyChanged(); }
        }
        private static readonly ObservableCollection<Music.ArticulationType> articulationTypes = new(Enum.GetValues<Music.ArticulationType>());
        public static ObservableCollection<Music.ArticulationType> ArticulationTypes { get => articulationTypes; }
        private Music.ArticulationType defaultArticulation = Music.ArticulationType.legato;
        public Music.ArticulationType DefaultArticulation
        {
            get { return defaultArticulation; }
            set { defaultArticulation = value; OnPropertyChanged(); }
        }
        private static readonly ObservableCollection<Music.ChordBindingType> chordBindingTypes = new(Enum.GetValues<Music.ChordBindingType>());
        public static ObservableCollection<Music.ChordBindingType> ChordBindingTypes { get => chordBindingTypes; }
        private Music.ChordBindingType defaultBinding = Music.ChordBindingType.Block;
        public Music.ChordBindingType DefaultBinding
        {
            get { return defaultBinding; }
            set { defaultBinding = value; OnPropertyChanged(); }
        }

        private static readonly ObservableCollection<Music.SpaceType> spaceTypes = new(Enum.GetValues<Music.SpaceType>());
        public static ObservableCollection<Music.SpaceType> SpaceTypes { get => spaceTypes; }
        private Music.SpaceType defaultSpace = Music.SpaceType.Direct;
        public Music.SpaceType DefaultSpace
        {
            get { return defaultSpace; }
            set { defaultSpace = value; OnPropertyChanged(); }
        }
        private string? newChordValue = "";
        public string? NewChordValue { get => newChordValue; set { newChordValue = value; OnPropertyChanged(); } }

        private RelayCommand<ObservableCollection<ChordItem>>? _PlaySequence;
        public RelayCommand<ObservableCollection<ChordItem>> PlaySequence =>
            _PlaySequence ??= new RelayCommand<ObservableCollection<ChordItem>>(sequence =>
            {
                if (sequence is ObservableCollection<ChordItem> thisSequence)
                {
                }
            });
        private RelayCommand<object>? _newChordValueCommand;
        public ChordValueDialog? NewChordValueDialog { get; set; } = null;
        public RelayCommand<object> NewChordValueCommand =>
            _newChordValueCommand ??= new RelayCommand<object>(execute =>
            {
                if (newChordValue == "")
                {
                    MessageBox.Show("A new chord value must be provided.");
                    return;
                }

                NewChordValueDialog?.DialogResult = true;
                NewChordValueDialog?.Close();
                NewChordValueDialog = null;

            });
        private RelayCommand<object>? _AddChordCommand;
        public RelayCommand<object> AddChordCommand =>
            _AddChordCommand ??= new RelayCommand<object>(execute =>
             new ChordSequenceCommands(this).AddChord());

        private RelayCommand<object?>? _addChordSequenceCommand;
        public RelayCommand<object?> AddChordSequenceCommand =>
            _addChordSequenceCommand ??= new RelayCommand<object?>(execute => new ChordSequenceCommands(this).AddChordSequence());
        private RelayCommand<ChordSequence>? _editChordSequenceCommand;
        public RelayCommand<ChordSequence> EditChordSequenceCommand =>
            _editChordSequenceCommand ??= new RelayCommand<ChordSequence>(chordSequence => new ChordSequenceCommands(this).EditChordSequence(chordSequence.Name));
        private RelayCommand<object?>? _submitChordSequenceCommand;
        public RelayCommand<object?> SubmitChordSequenceCommand =>
            _submitChordSequenceCommand ??= new RelayCommand<object?>(execute => new ChordSequenceCommands(this).SubmitChordSequence());
        private RelayCommand<object?>? _cancelEditorCommand;
        public RelayCommand<object?> CancelEditorCommand =>
            _cancelEditorCommand ??= new RelayCommand<object?>(execute =>
            {
                EditorPanel = new BlankPanel();
            });
        private RelayCommand<ChordSequence>? _deleteChordSequenceCommand;
        public RelayCommand<ChordSequence> DeleteChordSequenceCommand =>
            _deleteChordSequenceCommand ??= new RelayCommand<ChordSequence>(chordSequence => new ChordSequenceCommands(this).DeleteChordSequence(chordSequence.Name));
        private RelayCommand<object?>? _listChordSequencesCommand;
        public RelayCommand<object?> ListChordSequencesCommand =>
            _listChordSequencesCommand ??= new RelayCommand<object?>(execute => new ChordSequenceCommands(this).ListChordSequences());


    }
}
