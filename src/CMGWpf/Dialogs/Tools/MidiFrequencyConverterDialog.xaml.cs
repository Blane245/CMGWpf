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
            this.Closing += MidiFrequencyConverterDialog_Closing;
            this.Loaded += MidiFrequencyConverterDialog_Loaded;
        }

        private void MidiFrequencyConverterDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Services.GlobalService.Instance.StatusMessages.Clear();
        }

        private void MidiFrequencyConverterDialog_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is ToolsViewModel vm) vm.ActiveMidiFrequencyConverterDialog = null;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
            if (DataContext is ToolsViewModel vm) vm.ActiveMidiFrequencyConverterDialog = null;
        }

    }
}
