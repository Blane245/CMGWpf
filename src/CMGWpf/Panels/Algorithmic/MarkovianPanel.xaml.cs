using CMGWpf.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CMGWpf.Panels.Algorithmic
{
    public partial class MarkovianPanel : UserControl
    {
        // Dependency property for ValueUnits with callback to update formatted values
        public static readonly DependencyProperty ValueUnitsProperty = DependencyProperty.Register(
            nameof(ValueUnits),
            typeof(Func<double, string>),
            typeof(MarkovianPanel),
            new PropertyMetadata(null, OnValueUnitsChanged));

        private static void OnValueUnitsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarkovianPanel panel)
            {
                panel.UpdateFormattedStart();
                panel.UpdateFormattedLo();
                panel.UpdateFormattedHi();
            }
        }

        public Func<double, string>? ValueUnits
        {
            get => (Func<double, string>?)GetValue(ValueUnitsProperty);
            set => SetValue(ValueUnitsProperty, value);
        }

        public static readonly DependencyProperty AmplitudeUnitsProperty = DependencyProperty.Register(
            nameof(AmplitudeUnits), typeof(Func<double, string>), typeof(MarkovianPanel), new PropertyMetadata(null));
        public Func<double, string>? AmplitudeUnits
        {
            get => (Func<double, string>?)GetValue(AmplitudeUnitsProperty);
            set => SetValue(AmplitudeUnitsProperty, value);
        }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(double?), typeof(MarkovianPanel), new PropertyMetadata(null));
        public double? Minimum { get => (double?)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(double?), typeof(MarkovianPanel), new PropertyMetadata(null));
        public double? Maximum { get => (double?)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(
            nameof(Increment), typeof(double?), typeof(MarkovianPanel), new PropertyMetadata(null));
        public double? Increment { get => (double?)GetValue(IncrementProperty); set => SetValue(IncrementProperty, value); }

        public static readonly DependencyProperty ValueFormatProperty = DependencyProperty.Register(
            nameof(ValueFormat), typeof(string), typeof(MarkovianPanel), new PropertyMetadata(null));
        public string ValueFormat { get => (string)GetValue(ValueFormatProperty); set => SetValue(ValueFormatProperty, value); }

        public static readonly DependencyProperty AmplitudeFormatProperty = DependencyProperty.Register(
            nameof(AmplitudeFormat), typeof(string), typeof(MarkovianPanel), new PropertyMetadata(null));
        public string AmplitudeFormat { get => (string)GetValue(AmplitudeFormatProperty); set => SetValue(AmplitudeFormatProperty, value); }

        // Dependency properties for formatted values
        public static readonly DependencyProperty FormattedStartProperty = DependencyProperty.Register(
            nameof(FormattedStart),
            typeof(string),
            typeof(MarkovianPanel),
            new PropertyMetadata(string.Empty));

        public string FormattedStart
        {
            get => (string)GetValue(FormattedStartProperty);
            private set => SetValue(FormattedStartProperty, value);
        }

        public static readonly DependencyProperty FormattedLoProperty = DependencyProperty.Register(
            nameof(FormattedLo),
            typeof(string),
            typeof(MarkovianPanel),
            new PropertyMetadata(string.Empty));

        public string FormattedLo
        {
            get => (string)GetValue(FormattedLoProperty);
            private set => SetValue(FormattedLoProperty, value);
        }

        public static readonly DependencyProperty FormattedHiProperty = DependencyProperty.Register(
            nameof(FormattedHi),
            typeof(string),
            typeof(MarkovianPanel),
            new PropertyMetadata(string.Empty));

        public string FormattedHi
        {
            get => (string)GetValue(FormattedHiProperty);
            private set => SetValue(FormattedHiProperty, value);
        }

        private void UpdateFormattedStart()
        {
            if (DataContext is Markovian markovian && ValueUnits != null)
                FormattedStart = ValueUnits(markovian.Start);
            else
                FormattedStart = string.Empty;
        }

        private void UpdateFormattedLo()
        {
            if (DataContext is Markovian markovian && ValueUnits != null)
                FormattedLo = ValueUnits(markovian.Lo);
            else
                FormattedLo = string.Empty;
        }

        private void UpdateFormattedHi()
        {
            if (DataContext is Markovian markovian && ValueUnits != null)
                FormattedHi = ValueUnits(markovian.Hi);
            else
                FormattedHi = string.Empty;
        }

        public MarkovianPanel()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is Markovian oldMarkovian)
                oldMarkovian.PropertyChanged -= OnAlgorithmPropertyChanged;

            if (e.NewValue is Markovian newMarkovian)
                newMarkovian.PropertyChanged += OnAlgorithmPropertyChanged;

            UpdateFormattedStart();
            UpdateFormattedLo();
            UpdateFormattedHi();
        }

        private void OnAlgorithmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Markovian.Start))
                UpdateFormattedStart();
            else if (e.PropertyName == nameof(Markovian.Lo))
                UpdateFormattedLo();
            else if (e.PropertyName == nameof(Markovian.Hi))
                UpdateFormattedHi();
        }
    }
}
