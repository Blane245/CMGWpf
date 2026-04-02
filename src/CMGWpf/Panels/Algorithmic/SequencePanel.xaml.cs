using CMGWpf.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CMGWpf.Panels.Algorithmic
{
    /// <summary>
    /// Interaction logic for Sequence.xaml
    /// </summary>
    public partial class SequencePanel : UserControl
    {

        public static readonly DependencyProperty ValueUnitsProperty = DependencyProperty.Register(
            nameof(ValueUnits),
            typeof(Func<double, string>),
            typeof(SequencePanel),
            new PropertyMetadata(null, OnValueUnitsChanged));

        private static void OnValueUnitsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SequencePanel panel)
            {
                panel.UpdateFormattedReflectPitch();
            }
        }

        public Func<double, string>? ValueUnits
        {
            get => (Func<double, string>?)GetValue(ValueUnitsProperty);
            set => SetValue(ValueUnitsProperty, value);
        }

        public static readonly DependencyProperty AmplitudeUnitsProperty = DependencyProperty.Register(
            nameof(AmplitudeUnits), typeof(Func<double, string>), typeof(SequencePanel), new PropertyMetadata(null));
        public Func<double, string>? AmplitudeUnits
        {
            get => (Func<double, string>?)GetValue(AmplitudeUnitsProperty);
            set => SetValue(AmplitudeUnitsProperty, value);
        }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(double?), typeof(SequencePanel), new PropertyMetadata(null));
        public double? Minimum { get => (double?)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(double?), typeof(SequencePanel), new PropertyMetadata(null));
        public double? Maximum { get => (double?)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(
            nameof(Increment), typeof(double?), typeof(SequencePanel), new PropertyMetadata(null));
        public double? Increment { get => (double?)GetValue(IncrementProperty); set => SetValue(IncrementProperty, value); }

        public static readonly DependencyProperty ValueFormatProperty = DependencyProperty.Register(
            nameof(ValueFormat), typeof(string), typeof(SequencePanel), new PropertyMetadata(null));
        public string ValueFormat { get => (string)GetValue(ValueFormatProperty); set => SetValue(ValueFormatProperty, value); }

        public static readonly DependencyProperty AmplitudeFormatProperty = DependencyProperty.Register(
            nameof(AmplitudeFormat), typeof(string), typeof(SequencePanel), new PropertyMetadata(null));
        public string AmplitudeFormat { get => (string)GetValue(AmplitudeFormatProperty); set => SetValue(AmplitudeFormatProperty, value); }

        // Dependency properties for formatted values
        public string FormattedReflectPitch
        {
            get => (string)GetValue(FormattedReflectPitchProperty);
            private set => SetValue(FormattedReflectPitchProperty, value);
        }

        public static readonly DependencyProperty FormattedReflectPitchProperty = DependencyProperty.Register(
            nameof(FormattedReflectPitch),
            typeof(string),
            typeof(SequencePanel),
            new PropertyMetadata(string.Empty));
        private void UpdateFormattedReflectPitch()
        {
            if (DataContext is Sequence sequence && ValueUnits != null)
                FormattedReflectPitch = ValueUnits(sequence.ReflectPitch);
            else
                FormattedReflectPitch = string.Empty;
        }

        public SequencePanel()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is Sequence oldSequence)
                oldSequence.PropertyChanged -= OnAlgorithmPropertyChanged;

            if (e.NewValue is Sequence newSequence)
                newSequence.PropertyChanged += OnAlgorithmPropertyChanged;

            UpdateFormattedReflectPitch();
        }

        private void OnAlgorithmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Sequence.ReflectPitch))
                UpdateFormattedReflectPitch();
        }
    }
}
