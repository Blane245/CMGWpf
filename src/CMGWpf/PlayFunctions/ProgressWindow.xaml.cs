using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CMGWpf.PlayFunctions
{
    public partial class ProgressWindow : Window, INotifyPropertyChanged
    {
        private string _statusMessage = "Processing generators...";
        private int _completedGenerators;
        private int _totalGenerators;
        private bool _isIndeterminate;
        private bool _isCancelled;

        public ProgressWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public int CompletedGenerators
        {
            get => _completedGenerators;
            set
            {
                _completedGenerators = value;
                OnPropertyChanged();
            }
        }

        public int TotalGenerators
        {
            get => _totalGenerators;
            set
            {
                _totalGenerators = value;
                OnPropertyChanged();
            }
        }

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set
            {
                _isIndeterminate = value;
                OnPropertyChanged();
            }
        }

        public bool IsCancelled => _isCancelled;

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _isCancelled = true;
            StatusMessage = "Cancelling...";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
