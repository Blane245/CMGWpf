using CMGWpf.View;
using System.Windows;

namespace CMGWpf.Dialogs
{
    public partial class CommentDialog : Window
    {
        FileViewModel vm = FileViewModel.Instance;
        public CommentDialog()
        {
            InitializeComponent();
            DataContext = vm;
            vm.StatusMessages = [];
            this.Loaded += CommentDialog_Loaded;
        }

        private void CommentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Services.GlobalService.Instance.StatusMessages.Clear();
        }
        private void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
