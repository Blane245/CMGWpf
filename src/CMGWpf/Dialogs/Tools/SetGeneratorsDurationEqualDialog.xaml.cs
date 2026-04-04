using CMGWpf.Types;
using CMGWpf.View;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
namespace CMGWpf.Dialogs.Tools
{
    /// <summary>
    /// Interaction logic for SetGeneratorsDurationEqualDialog.xaml
    /// </summary>
    public partial class SetGeneratorsDurationEqualDialog : Window
    {
        public SetGeneratorsDurationEqualDialog()
        {
            InitializeComponent();
            DataContext = ToolsViewModel.Instance;
        }
    }
}
