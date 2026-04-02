using CMGWpf.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CMGWpf.Panels.Algorithmic
{
    /// <summary>
    /// Interaction logic for AutoregressivePanel.xaml
    /// </summary>
    public partial class AutoregressivePanel : UserControl
    {
        // Dependency property for ValueUnits with callback to update formatted values
        public static readonly DependencyProperty ValueUnitsProperty = DependencyProperty.Register(
            nameof(ValueUnits),
            typeof(Func<double, string>),
            typeof(AutoregressivePanel),
            new PropertyMetadata(null, OnValueUnitsChanged));

        private static void OnValueUnitsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AutoregressivePanel panel)
            {
                panel.UpdateFormattedInitial();
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
            nameof(AmplitudeUnits), typeof(Func<double, string>), typeof(AutoregressivePanel), new PropertyMetadata(null));
        public Func<double, string>? AmplitudeUnits
        {
            get => (Func<double, string>?)GetValue(AmplitudeUnitsProperty);
            set => SetValue(AmplitudeUnitsProperty, value);
        }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(double?), typeof(AutoregressivePanel), new PropertyMetadata(null));
        public double? Minimum { get => (double?)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(double?), typeof(AutoregressivePanel), new PropertyMetadata(null));
        public double? Maximum { get => (double?)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(
            nameof(Increment), typeof(double?), typeof(AutoregressivePanel), new PropertyMetadata(null));
        public double? Increment { get => (double?)GetValue(IncrementProperty); set => SetValue(IncrementProperty, value); }

        public static readonly DependencyProperty ValueFormatProperty = DependencyProperty.Register(
            nameof(ValueFormat), typeof(string), typeof(AutoregressivePanel), new PropertyMetadata(null));
        public string ValueFormat { get => (string)GetValue(ValueFormatProperty); set => SetValue(ValueFormatProperty, value); }

        public static readonly DependencyProperty AmplitudeFormatProperty = DependencyProperty.Register(
            nameof(AmplitudeFormat), typeof(string), typeof(AutoregressivePanel), new PropertyMetadata(null));
        public string AmplitudeFormat { get => (string)GetValue(AmplitudeFormatProperty); set => SetValue(AmplitudeFormatProperty, value); }

        // Dependency properties for formatted values
        public static readonly DependencyProperty FormattedInitialProperty = DependencyProperty.Register(
            nameof(FormattedInitial),
            typeof(string),
            typeof(AutoregressivePanel),
            new PropertyMetadata(string.Empty));

        public string FormattedInitial
        {
            get => (string)GetValue(FormattedInitialProperty);
            private set => SetValue(FormattedInitialProperty, value);
        }

        public static readonly DependencyProperty FormattedAlphaProperty = DependencyProperty.Register(
            nameof(FormattedAlpha),
            typeof(string),
            typeof(AutoregressivePanel),
            new PropertyMetadata(string.Empty));

        public string FormattedAlpha
        {
            get => (string)GetValue(FormattedAlphaProperty);
            private set => SetValue(FormattedAlphaProperty, value);
        }

        public static readonly DependencyProperty FormattedSigmaProperty = DependencyProperty.Register(
            nameof(FormattedSigma),
            typeof(string),
            typeof(AutoregressivePanel),
            new PropertyMetadata(string.Empty));

        public string FormattedSigma
        {
            get => (string)GetValue(FormattedSigmaProperty);
            private set => SetValue(FormattedSigmaProperty, value);
        }

        public static readonly DependencyProperty FormattedLoProperty = DependencyProperty.Register(
            nameof(FormattedLo),
            typeof(string),
            typeof(AutoregressivePanel),
            new PropertyMetadata(string.Empty));

        public string FormattedLo
        {
            get => (string)GetValue(FormattedLoProperty);
            private set => SetValue(FormattedLoProperty, value);
        }

        public static readonly DependencyProperty FormattedHiProperty = DependencyProperty.Register(
            nameof(FormattedHi),
            typeof(string),
            typeof(AutoregressivePanel),
            new PropertyMetadata(string.Empty));

        public string FormattedHi
        {
            get => (string)GetValue(FormattedHiProperty);
            private set => SetValue(FormattedHiProperty, value);
        }

        private void UpdateFormattedInitial()
        {
            if (DataContext is Autoregressive autoRegressive && ValueUnits != null)
                FormattedInitial = ValueUnits(autoRegressive.Initial);
            else
                FormattedInitial = string.Empty;
        }

        private void UpdateFormattedAlpha()
        {
            if (DataContext is Autoregressive autoRegressive && ValueUnits != null)
                FormattedAlpha = ValueUnits(autoRegressive.Alpha);
            else
                FormattedAlpha = string.Empty;
        }
        private void UpdateFormattedSigma()
        {
            if (DataContext is Autoregressive autoRegressive && ValueUnits != null)
                FormattedSigma = ValueUnits(autoRegressive.Sigma);
            else
                FormattedSigma = string.Empty;
        }

        private void UpdateFormattedLo()
        {
            if (DataContext is Autoregressive autoRegressive && ValueUnits != null)
                FormattedLo = ValueUnits(autoRegressive.Lo);
            else
                FormattedLo = string.Empty;
        }

        private void UpdateFormattedHi()
        {
            if (DataContext is Autoregressive autoRegressive && ValueUnits != null)
                FormattedHi = ValueUnits(autoRegressive.Hi);
            else
                FormattedHi = string.Empty;
        }

        public AutoregressivePanel()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is Autoregressive oldAutoregressive)
                oldAutoregressive.PropertyChanged -= OnAlgorithmPropertyChanged;

            if (e.NewValue is Autoregressive newAutoregressive)
                newAutoregressive.PropertyChanged += OnAlgorithmPropertyChanged;
            UpdateFormattedInitial();
            UpdateFormattedLo();
            UpdateFormattedHi();
        }

        private void OnAlgorithmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Autoregressive.Initial))
                UpdateFormattedInitial();
            else if (e.PropertyName == nameof(Autoregressive.Alpha))
                UpdateFormattedAlpha();
            else if (e.PropertyName == nameof(Autoregressive.Sigma))
                UpdateFormattedSigma();
            else if (e.PropertyName == nameof(Autoregressive.Lo))
                UpdateFormattedLo();
            else if (e.PropertyName == nameof(Autoregressive.Hi))
                UpdateFormattedHi();
        }
    }
}
