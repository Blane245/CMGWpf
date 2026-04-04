using CMGWpf.Types;
using CMGWpf.View;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace CMGWpf.Dialogs.Tools
{
    /// <summary>
    /// Interaction logic for MidiFrequencyConverterDialog.xaml
    /// </summary>
    public partial class MidiFrequencyConverterDialog : Window
    {
        public MidiFrequencyConverterDialog()
        {
            InitializeComponent();
            DataContext = ToolsViewModel.Instance;
        }
    }
}
