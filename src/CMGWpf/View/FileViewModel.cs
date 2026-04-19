
using CMGWpf.Model;
using CMGWpf.MVVM;
using CMGWpf.Properties;
using CMGWpf.Services;
using CMGWpf.Types;
using System.Collections.ObjectModel;
using Track = CMGWpf.Model.Track;

namespace CMGWpf.View
{
    public class FileViewModel : ViewModelBase
    {
        private static FileViewModel? _instance;
        public static FileViewModel Instance => _instance ??= new FileViewModel();

        private FileViewModel()
        {
            string recentFilesString = Settings.Default.CMGRecentFiles;
            if (!string.IsNullOrEmpty(recentFilesString))
            {
                string[] recentFilesArray = recentFilesString.Split('|');
                RecentFiles = new ObservableCollection<string>(recentFilesArray);
            }

            GlobalService.Instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(GlobalService.Instance.FileName) ||
                    e.PropertyName == nameof(GlobalService.Instance.IsDirty))
                {
                    OnPropertyChanged(nameof(WindowTitle));
                }
                if (e.PropertyName == nameof(GlobalService.Instance.StatusMessages))
                {
                    OnPropertyChanged(nameof(StatusMessages));
                }
            };
        }

        public ObservableCollection<Message> StatusMessages
        {
            get => GlobalService.Instance.StatusMessages;
            set { GlobalService.Instance.StatusMessages = value; OnPropertyChanged(); }
        }
        public bool IsDirty
        {
            get => GlobalService.Instance.IsDirty;
            set
            {
                GlobalService.Instance.IsDirty = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowTitle));
            }
        }
        private ObservableCollection<string> recentFiles = [];
        public ObservableCollection<string> RecentFiles
        {
            get { return recentFiles; }
            set { recentFiles = value; OnPropertyChanged(); }
        }
        public void AddRecentFile(string filePath)
        {
            recentFiles.Remove(filePath);
            RecentFiles.Insert(0, filePath);
            // limit the recent files list to 10 items
            while (recentFiles.Count > 10) recentFiles.RemoveAt(10);
            Settings.Default.CMGRecentFiles = String.Join("|", [.. RecentFiles]);
            Settings.Default.Save();

            // Add to Windows Jump List
            Services.JumpListService.Instance.AddToRecentFiles(filePath);
        }
        public void NotifyTracksChanged(List<Track> newTracks)
        {
            File.Tracks = newTracks;
            OnPropertyChanged(nameof(File));
            IsDirty = true;
            OnPropertyChanged(nameof(IsDirty));
        }

        public string FileName
        {
            get => GlobalService.Instance.FileName;
            set
            {
                GlobalService.Instance.FileName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private CMGFile file = new();
        public CMGFile File
        {
            get { return file; }
            set { file = value; OnPropertyChanged(); }
        }
        private string newComment = string.Empty;
        public string NewComment
        {
            get { return newComment; }
            set { newComment = value; }
        }
        public string WindowTitle
        {
            get => GlobalService.Instance.WindowTitle;
        }
        public ObservableCollection<string> SoundFontFileNames
        {
            get => GlobalService.Instance.SoundFontFileNames;
            set { GlobalService.Instance.SoundFontFileNames = value; OnPropertyChanged(); }
        }
        private ObservableCollection<Message> messages = [];
        public ObservableCollection<Message> Messages
        {
            get { return messages; }
            set { messages = value; OnPropertyChanged(); }
        }

        #region File Menu Commands

        private RelayCommand<object>? _notImplementedCommand;
        public RelayCommand<object> NotImplementedCommand =>
            _notImplementedCommand ??= new RelayCommand<object>(execute => StatusMessages = new ObservableCollection<Message> { new Message { Text = "Command not implemented", Error = true } });
        private RelayCommand<object>? _fileNewCommand;
        public RelayCommand<object> FileNewCommand =>
            _fileNewCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).New());

        private RelayCommand<object>? _fileOpenCommand;
        public RelayCommand<object> FileOpenCommand =>
            _fileOpenCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).Open());

        private RelayCommand<object>? _fileSaveCommand;
        public RelayCommand<object> FileSaveCommand =>
            _fileSaveCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).Save(),
            canExecute => { return IsDirty; });

        private RelayCommand<object>? _fileSaveAsCommand;
        public RelayCommand<object> FileSaveAsCommand =>
            _fileSaveAsCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).SaveAs());

        private RelayCommand<string>? _fileOpenRecentCommand;
        public RelayCommand<string> FileOpenRecentCommand =>
            _fileOpenRecentCommand ??= new RelayCommand<string>(filePath => new FileCommands(this, File).OpenRecent(filePath));

        private RelayCommand<object>? _openRecentFileCommand;
        public RelayCommand<object> OpenRecentFileCommand =>
            _openRecentFileCommand ??= new RelayCommand<object>(param =>
            {
                if (param is int index || (param is string str && int.TryParse(str, out index)))
                {
                    if (index >= 0 && index < RecentFiles.Count)
                    {
                        new FileCommands(this, File).OpenRecent(RecentFiles[index]);
                    }
                }
            }); private RelayCommand<object>? _exitFileCommand;
        public RelayCommand<object> ExitFileCommand =>
            _exitFileCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).Exit());

        #endregion
        #region Edit Menu Commands
        private RelayCommand<object>? _editCommentCommand;
        public RelayCommand<object> EditCommentCommand =>
            _editCommentCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).EditComment());

        private RelayCommand<object>? _editCommentOkCommand;
        public RelayCommand<object> EditCommentOkCommand =>
            _editCommentOkCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).EditCommentOk());

        private RelayCommand<object>? _addTrackCommand;
        public RelayCommand<object> AddTrackCommand =>
            _addTrackCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).AddTrack());
        private RelayCommand<object>? _playCommand;
        public RelayCommand<object> PlayCommand =>
            _playCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).Play(null));
        private RelayCommand<object>? _reportCommand;
        public RelayCommand<object> ReportCommand =>
            _reportCommand ??= new RelayCommand<object>(execute => new FileCommands(this, File).Report(null));
        #endregion
    }
}
