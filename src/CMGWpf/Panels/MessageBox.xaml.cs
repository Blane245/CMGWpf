using CMGWpf.Types;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace CMGWpf.Panels
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(ObservableCollection<Message>), typeof(MessageBox), new PropertyMetadata(null));
        public ObservableCollection<Message>? Source { get => (ObservableCollection<Message>?)GetValue(SourceProperty); set => SetValue(SourceProperty, value); }
        public MessageBox()
        {
            InitializeComponent();
        }
    }
}
