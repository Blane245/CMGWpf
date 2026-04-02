using Microsoft.VisualBasic;
using System.Windows.Media;

namespace CMGWpf.Types
{
    public enum GeneratorEditMode
    {
        Add,
        Modify
    }

    public enum MoveCopyMode
    {
        Move,
        Copy
    }
    public class Message 
    {
        public string Text { get; init; } = "";
        private bool _error = false;
        public bool Error { get => _error; set { _error = value; if (_error) _brush = new SolidColorBrush(Colors.Red); else _brush = new SolidColorBrush(Colors.Black); }  }
        private Brush _brush = new SolidColorBrush(Colors.Black);
        public Brush Brush { get => _brush; set => _brush = value; }
    }
}
