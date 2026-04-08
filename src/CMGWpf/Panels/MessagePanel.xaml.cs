using CMGWpf.Types;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace CMGWpf.Panels
{
    /// <summary>
    /// Interaction logic for MessagePanel.xaml
    /// </summary>
    public partial class MessagePanel : UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(ObservableCollection<Message>), typeof(MessagePanel), new PropertyMetadata(null));
        public ObservableCollection<Message>? Source { get => (ObservableCollection<Message>?)GetValue(SourceProperty); set => SetValue(SourceProperty, value); }
        public MessagePanel()
        {
            InitializeComponent();
        }
    }
}
