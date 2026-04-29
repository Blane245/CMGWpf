using CMGDBEditor.View;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CMGDBEditor.Dialogs
{
    /// <summary>
    /// Interaction logic for VoiceEnsemblesList.xaml
    /// </summary>
    public partial class VoiceEnsemblesList : Window
    {
        public VoiceEnsemblesList(EnsembleView vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
