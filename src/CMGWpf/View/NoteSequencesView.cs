using CMGWpf.Model.Database;
using CMGWpf.MVVM;
using CMGWpf.Panels.Database;
using CMGWpf.Types;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using static CMGWpf.View.EnsembleView;

namespace CMGWpf.View
{
    /// <summary>
    /// Singleton ViewModel for the Note Sequences management panel. This includes managing the list of note sequences, the current note sequence being edited, and related tags. It also handles commands for adding, editing, deleting, and listing note sequences and tags.
    /// </summary>
    public class NoteSequencesView : ViewModelBase
    {
        private static NoteSequencesView? _instance;
        public static NoteSequencesView Instance => _instance ??= new NoteSequencesView();

        private NoteSequencesView()
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
        private ObservableCollection<NoteSequence> _noteSequenceList = [];
        public ObservableCollection<NoteSequence> NoteSequenceList
        {
            get { return _noteSequenceList; }
            set { _noteSequenceList = [..value.OrderBy(sequence=>sequence.Name)]; OnPropertyChanged(); }
        }
        private ObservableCollection<Tag> _tagList = [];
        public ObservableCollection<Tag> TagList
        {
            get { return _tagList; }
            set { _tagList = [..value.OrderBy(tag => tag.Name)]; OnPropertyChanged(); }
        }
        private Tag? _UITag;
        public Tag? UITag
        {
            get { return _UITag; }
            set { _UITag = value; OnPropertyChanged(); }
        }
        private string _newTagName = "";
        public string NewTagName { get => _newTagName; set { _newTagName = value; OnPropertyChanged(); } }
        public string NoteSequenceEditorTitle => ModifyMode == "Add" ? "Add Note Sequence" : $"Modify Note Sequence: {NewNoteSequenceName}";
        private NoteSequence? _UINoteSequence;
        public NoteSequence? UINoteSequence
        {
            get { return _UINoteSequence; }
            set { _UINoteSequence = value; OnPropertyChanged(); }
        }
        private string _newNoteSequenceName = "";
        public string NewNoteSequenceName { get => _newNoteSequenceName; set { _newNoteSequenceName = value; OnPropertyChanged(); } }
        private string _newTagListString = "";
        public string NewTagListString { get => _newTagListString; set { _newTagListString = value; OnPropertyChanged(); } }
        private ObservableCollection<NoteItem> _newNoteItems = new ObservableCollection<NoteItem>();
        public ObservableCollection<NoteItem> NewNoteItems
        {
            get => _newNoteItems;
            set { _newNoteItems = value; OnPropertyChanged(); }
        }
        //public TagDialog? TagDialog { get; set; }
        private UserControl? _editorPanel;
        public UserControl? EditorPanel
        {
            get { return _editorPanel; }
            set { _editorPanel = value; OnPropertyChanged(); }
        }

        private RelayCommand<object?>? _addNoteSequenceCommand;
        public RelayCommand<object?> AddNoteSequenceCommand =>
            _addNoteSequenceCommand ??= new RelayCommand<object?>(execute => new NoteSequenceCommands(this).AddNoteSequence());
        private RelayCommand<NoteSequence>? _editNoteSequenceCommand;
        public RelayCommand<NoteSequence> EditNoteSequenceCommand =>
            _editNoteSequenceCommand ??= new RelayCommand<NoteSequence>(noteSequence => new NoteSequenceCommands(this).EditNoteSequence(noteSequence.Name));
        private RelayCommand<VoiceEnsemblesListType>? _editNoteSequenceByNameCommand;
        public RelayCommand<VoiceEnsemblesListType> EditNoteSequenceByNameCommand =>
            _editNoteSequenceByNameCommand ??= new RelayCommand<VoiceEnsemblesListType>(item => new NoteSequenceCommands(this).EditNoteSequence(item.Name));
        private RelayCommand<object?>? _submitNoteSequenceCommand;
        public RelayCommand<object?> SubmitNoteSequenceCommand =>
            _submitNoteSequenceCommand ??= new RelayCommand<object?>(execute => new NoteSequenceCommands(this).SubmitNoteSequence());
        private RelayCommand<object?>? _cancelEditorCommand;
        public RelayCommand<object?> CancelEditorCommand =>
            _cancelEditorCommand ??= new RelayCommand<object?>(execute =>
            {
                EditorPanel = new BlankPanel();
            });
        private RelayCommand<NoteSequence>? _deleteNoteSequenceCommand;
        public RelayCommand<NoteSequence> DeleteNoteSequenceCommand =>
            _deleteNoteSequenceCommand ??= new RelayCommand<NoteSequence>(noteSequence => new NoteSequenceCommands(this).DeleteNoteSequence(noteSequence.Name));
        private RelayCommand<object?>? _listNoteSequencesCommand;
        public RelayCommand<object?> ListNoteSequencesCommand =>
            _listNoteSequencesCommand ??= new RelayCommand<object?>(execute => new NoteSequenceCommands(this).ListNoteSequences());

        private RelayCommand<object?>? _addTagCommand;
        public RelayCommand<object?> AddTagCommand =>
            _addTagCommand ??= new RelayCommand<object?>(execute => new TagCommands(this).AddModifyTag("Add", null));
        private RelayCommand<Tag>? _editTagCommand;
        public RelayCommand<Tag> EditTagCommand =>
            _editTagCommand ??= new RelayCommand<Tag>(tag => new TagCommands(this).AddModifyTag("Modify", tag));
        private RelayCommand<Tag>? _submitTagCommand;
        public RelayCommand<Tag> SubmitTagCommand =>
            _submitTagCommand ??= new RelayCommand<Tag>(tag => new TagCommands(this).SubmitTag());
        private RelayCommand<Tag>? _deleteTagCommand;
        public RelayCommand<Tag> DeleteTagCommand =>
            _deleteTagCommand ??= new RelayCommand<Tag>(tag => new TagCommands(this).DeleteTag(tag.Name));
        private RelayCommand<Tag>? _listTagNoteSequencesCommand;
        public RelayCommand<Tag> ListTagNoteSequencesCommand =>
            _listTagNoteSequencesCommand ??= new RelayCommand<Tag>(tag => new TagCommands(this).ListTagNoteSequences(tag.Name));
    }
}
