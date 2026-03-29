using CMGWpf.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CMGWpf.Panels.Algorithmic
{
    public partial class OscillatorPanel : UserControl
    {
        // Dependency property for ValueUnits with callback to update formatted center
        public static readonly DependencyProperty ValueUnitsProperty = DependencyProperty.Register(
            nameof(ValueUnits),
            typeof(Func<double, string>),
            typeof(OscillatorPanel),
            new PropertyMetadata(null, OnValueUnitsChanged));

        private static void OnValueUnitsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OscillatorPanel panel)
                panel.UpdateFormattedCenter();
        }

        public Func<double, string>? ValueUnits
        {
            get => (Func<double, string>?)GetValue(ValueUnitsProperty);
            set => SetValue(ValueUnitsProperty, value);
        }

        // Dependency property for AmplitudeUnits with callback to update formatted amplitude
        public static readonly DependencyProperty AmplitudeUnitsProperty = DependencyProperty.Register(
            nameof(AmplitudeUnits),
            typeof(Func<double, string>),
            typeof(OscillatorPanel),
            new PropertyMetadata(null, OnAmplitudeUnitsChanged));

        private static void OnAmplitudeUnitsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is OscillatorPanel panel)
                panel.UpdateFormattedAmplitude();
        }

        public Func<double, string>? AmplitudeUnits
        {
            get => (Func<double, string>?)GetValue(AmplitudeUnitsProperty);
            set => SetValue(AmplitudeUnitsProperty, value);
        }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(double?), typeof(OscillatorPanel), new PropertyMetadata(null));
        public double? Minimum { get => (double?)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(double?), typeof(OscillatorPanel), new PropertyMetadata(null));
        public double? Maximum { get => (double?)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(
            nameof(Increment), typeof(double?), typeof(OscillatorPanel), new PropertyMetadata(null));
        public double? Increment { get => (double?)GetValue(IncrementProperty); set => SetValue(IncrementProperty, value); }

        public static readonly DependencyProperty ValueFormatProperty = DependencyProperty.Register(
            nameof(ValueFormat), typeof(string), typeof(OscillatorPanel), new PropertyMetadata(null));
        public string ValueFormat { get => (string)GetValue(ValueFormatProperty); set => SetValue(ValueFormatProperty, value); }

        public static readonly DependencyProperty AmplitudeFormatProperty = DependencyProperty.Register(
            nameof(AmplitudeFormat), typeof(string), typeof(OscillatorPanel), new PropertyMetadata(null));
        public string AmplitudeFormat { get => (string)GetValue(AmplitudeFormatProperty); set => SetValue(AmplitudeFormatProperty, value); }

        // Dependency property for formatted center value (for binding in XAML)
        public static readonly DependencyProperty FormattedCenterProperty = DependencyProperty.Register(
            nameof(FormattedCenter),
            typeof(string),
            typeof(OscillatorPanel),
            new PropertyMetadata(string.Empty));

        public string FormattedCenter
        {
            get => (string)GetValue(FormattedCenterProperty);
            private set => SetValue(FormattedCenterProperty, value);
        }

        // Dependency property for formatted amplitude value (for binding in XAML)
        public static readonly DependencyProperty FormattedAmplitudeProperty = DependencyProperty.Register(
            nameof(FormattedAmplitude),
            typeof(string),
            typeof(OscillatorPanel),
            new PropertyMetadata(string.Empty));

        public string FormattedAmplitude
        {
            get => (string)GetValue(FormattedAmplitudeProperty);
            private set => SetValue(FormattedAmplitudeProperty, value);
        }

        private void UpdateFormattedCenter()
        {
            if (DataContext is Oscillator oscillator && ValueUnits != null)
                FormattedCenter = ValueUnits(oscillator.Center);
            else
                FormattedCenter = string.Empty;
        }

        private void UpdateFormattedAmplitude()
        {
            if (DataContext is Oscillator oscillator && AmplitudeUnits != null)
                FormattedAmplitude = AmplitudeUnits(oscillator.Amplitude);
            else
                FormattedAmplitude = string.Empty;
        }

        public OscillatorPanel()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is Oscillator oldOscillator)
                oldOscillator.PropertyChanged -= OnAlgorithmPropertyChanged;

            if (e.NewValue is Oscillator newOscillator)
                newOscillator.PropertyChanged += OnAlgorithmPropertyChanged;

            UpdateFormattedCenter();
            UpdateFormattedAmplitude();
        }

        private void OnAlgorithmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Oscillator.Center))
                UpdateFormattedCenter();
            else if (e.PropertyName == nameof(Oscillator.Amplitude))
                UpdateFormattedAmplitude();
        }
    }
}
