using CMGWpf.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CMGWpf.Panels.Algorithmic
{
    /// <summary>
    /// Interaction logic for PoissonPanel.xaml
    /// </summary>
    public partial class PoissonPanel : UserControl
    {
        // Dependency property for ValueUnits with callback to update formatted values
        public static readonly DependencyProperty ValueUnitsProperty = DependencyProperty.Register(
            nameof(ValueUnits),
            typeof(Func<double, string>),
            typeof(PoissonPanel),
            new PropertyMetadata(null, OnValueUnitsChanged));

        private static void OnValueUnitsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PoissonPanel panel)
            {
                panel.UpdateFormattedLo();
                panel.UpdateFormattedHi();
            }
        }

        public Func<double, string>? ValueUnits
        {
            get => (Func<double, string>?)GetValue(ValueUnitsProperty);
            set => SetValue(ValueUnitsProperty, value);
        }

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(
            nameof(Increment), typeof(double?), typeof(PoissonPanel), new PropertyMetadata(null));
        public double? Increment { get => (double?)GetValue(IncrementProperty); set => SetValue(IncrementProperty, value); }
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(double?), typeof(PoissonPanel), new PropertyMetadata(null));
        public double? Minimum { get => (double?)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(double?), typeof(PoissonPanel), new PropertyMetadata(null));
        public double? Maximum { get => (double?)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }


        public static readonly DependencyProperty ValueFormatProperty = DependencyProperty.Register(
            nameof(ValueFormat), typeof(string), typeof(PoissonPanel), new PropertyMetadata(null));
        public string ValueFormat { get => (string)GetValue(ValueFormatProperty); set => SetValue(ValueFormatProperty, value); }

        public static readonly DependencyProperty FormattedLoProperty = DependencyProperty.Register(
            nameof(FormattedLo),
            typeof(string),
            typeof(PoissonPanel),
            new PropertyMetadata(string.Empty));

        public string FormattedLo
        {
            get => (string)GetValue(FormattedLoProperty);
            private set => SetValue(FormattedLoProperty, value);
        }

        public static readonly DependencyProperty FormattedHiProperty = DependencyProperty.Register(
            nameof(FormattedHi),
            typeof(string),
            typeof(PoissonPanel),
            new PropertyMetadata(string.Empty));

        public string FormattedHi
        {
            get => (string)GetValue(FormattedHiProperty);
            private set => SetValue(FormattedHiProperty, value);
        }

        private void UpdateFormattedLo()
        {
            if (DataContext is Poisson poisson && ValueUnits != null)
                FormattedLo = ValueUnits(poisson.Lo);
            else
                FormattedLo = string.Empty;
        }

        private void UpdateFormattedHi()
        {
            if (DataContext is Poisson poisson && ValueUnits != null)
                FormattedHi = ValueUnits(poisson.Hi);
            else
                FormattedHi = string.Empty;
        }

        public PoissonPanel()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is Poisson oldPoisson)
                oldPoisson.PropertyChanged -= OnAlgorithmPropertyChanged;

            if (e.NewValue is Poisson newPoisson)
                newPoisson.PropertyChanged += OnAlgorithmPropertyChanged;
            UpdateFormattedLo();
            UpdateFormattedHi();
        }
        private void OnAlgorithmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Poisson.Lo))
                UpdateFormattedLo();
            else if (e.PropertyName == nameof(Poisson.Hi))
                UpdateFormattedHi();
        }
    }
}
