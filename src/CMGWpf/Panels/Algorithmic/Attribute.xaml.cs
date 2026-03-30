using System;
using System.Windows;
using System.Windows.Controls;

namespace CMGWpf.Panels.Algorithmic
{
    /// <summary>
    /// Interaction logic for Attribute.xaml
    /// </summary>
    public partial class Attribute : UserControl
    {
        public static readonly DependencyProperty ValueUnitsProperty = DependencyProperty.Register(nameof(ValueUnits), typeof(Func<double, string>), typeof(Attribute), new PropertyMetadata(null));
        public Func<double, string>? ValueUnits { get => (Func<double, string>?)GetValue(ValueUnitsProperty); set => SetValue(ValueUnitsProperty, value); }

        public static readonly DependencyProperty AmplitudeUnitsProperty = DependencyProperty.Register(nameof(AmplitudeUnits), typeof(Func<double, string>), typeof(Attribute), new PropertyMetadata(null));

        public Func<double, string>? AmplitudeUnits { get => (Func<double, string>?)GetValue(AmplitudeUnitsProperty); set => SetValue(AmplitudeUnitsProperty, value); }

        public static readonly DependencyProperty ValueFormatProperty = DependencyProperty.Register(
            nameof(ValueFormat), typeof(string), typeof(Attribute), new PropertyMetadata(null));
        public string ValueFormat { get => (string)GetValue(ValueFormatProperty); set => SetValue(ValueFormatProperty, value); }

        public static readonly DependencyProperty AmplitudeFormatProperty = DependencyProperty.Register(
            nameof(AmplitudeFormat), typeof(string), typeof(Attribute), new PropertyMetadata(null));
        public string AmplitudeFormat { get => (string)GetValue(AmplitudeFormatProperty); set => SetValue(AmplitudeFormatProperty, value); }

        public Attribute()
        {
            InitializeComponent();
        }
    }
}
