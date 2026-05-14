using CMGWpf.Types;
using System.Collections.ObjectModel;

namespace CMGDBEditor.Helpers
{
    public static class Messages
    {
        public static void Add(ObservableCollection<Message> messages,  string text, bool error = false)
        {
            messages.Add(new Message() { Text = text, Error = error });
        }
    }
}
