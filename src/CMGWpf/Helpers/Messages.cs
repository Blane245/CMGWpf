using CMGWpf.Types;
using System.Collections.ObjectModel;

namespace CMGWpf.Helpers
{
    public static class Messages
    {
        public static void Add(ObservableCollection<Message> messages,  string text, bool error = false)
        {
            messages.Add(new Message() { Text = text, Error = error, Brush = (error)?System.Windows.Media.Brushes.IndianRed: System.Windows.Media.Brushes.Black });
        }
    }
}
