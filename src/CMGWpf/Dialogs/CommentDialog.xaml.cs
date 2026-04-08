using CMGWpf.Types;
using CMGWpf.View;
using System.ComponentModel;
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
            this.Closing += CommentDialog_Closing;
        }

        private void CommentDialog_Closing(object? sender, CancelEventArgs e)
        {
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
