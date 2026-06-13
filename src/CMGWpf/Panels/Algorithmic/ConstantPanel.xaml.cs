using CMGWpf.Model;
using CMGWpf.Utilities;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CMGWpf.Panels.Algorithmic
{
    public partial class ConstantPanel : UserControl
    {
        public static readonly DependencyProperty ValueUnitsProperty = DependencyProperty.Register(
            nameof(ValueUnits), 
            typeof(Func<double, string>), 
            typeof(ConstantPanel), 
            new PropertyMetadata(null, OnValueUnitsChanged));

        private static void OnValueUnitsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConstantPanel panel)
            {
                panel.UpdateFormattedValue();
            }
        }
        public Func<double, string>? ValueUnits { get => (Func<double, string>?)GetValue(ValueUnitsProperty); set => SetValue(ValueUnitsProperty, value); }

        public static readonly DependencyProperty AmplitudeUnitsProperty = DependencyProperty.Register(nameof(AmplitudeUnits), typeof(Func<double, string>), typeof(ConstantPanel), new PropertyMetadata(null));

        public Func<double, string>? AmplitudeUnits { get => (Func<double, string>?)GetValue(AmplitudeUnitsProperty); set => SetValue(AmplitudeUnitsProperty, value); }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(double?), typeof(ConstantPanel), new PropertyMetadata(null));
        public double? Minimum { get => (double?)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(double?), typeof(ConstantPanel), new PropertyMetadata(null));
        public double? Maximum { get => (double?)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(
            nameof(Increment), typeof(double?), typeof(ConstantPanel), new PropertyMetadata(null));
        public double? Increment { get => (double?)GetValue(IncrementProperty); set => SetValue(IncrementProperty, value); }

        public static readonly DependencyProperty ValueFormatProperty = DependencyProperty.Register(
            nameof(ValueFormat), typeof(string), typeof(ConstantPanel), new PropertyMetadata(null));
        public string ValueFormat { get => (string)GetValue(ValueFormatProperty); set => SetValue(ValueFormatProperty, value); }

        public static readonly DependencyProperty AmplitudeFormatProperty = DependencyProperty.Register(
            nameof(AmplitudeFormat), typeof(string), typeof(ConstantPanel), new PropertyMetadata(null));
        public string AmplitudeFormat { get => (string)GetValue(AmplitudeFormatProperty); set => SetValue(AmplitudeFormatProperty, value); }

        public static readonly DependencyProperty FormattedValueProperty = DependencyProperty.Register(
            nameof(FormattedValue),
            typeof(string),
            typeof(ConstantPanel),
            new PropertyMetadata(string.Empty));
        public string FormattedValue
        {
            get => (string)GetValue(FormattedValueProperty);
            private set => SetValue(FormattedValueProperty, value);
        }

        private void UpdateFormattedValue()
        {
            if (DataContext is Constant constant && ValueUnits != null)
            {
                FormattedValue = ValueUnits(constant.Value);
                DebugLog.Write($"ConstantPanel.UpdateFormattedValue: Value={constant.Value}, Formatted={FormattedValue}");
            }
            else
            {
                FormattedValue = string.Empty;
                DebugLog.Write($"ConstantPanel.UpdateFormattedValue: Returning empty - DataContext is {DataContext?.GetType().Name ?? "null"}, ValueUnits is {(ValueUnits == null ? "null" : "set")}");
            }
        }

        public ConstantPanel()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += (s, e) =>
            {
                // Force refresh after everything is loaded and bindings are applied
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateFormattedValue();
                    DebugLog.Write($"ConstantPanel.Loaded: Forcing refresh. DataContext={DataContext?.GetType().Name}, ValueUnits={(ValueUnits == null ? "null" : "set")}");
                }), System.Windows.Threading.DispatcherPriority.DataBind);
            };
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is Constant oldConstant)
                oldConstant.PropertyChanged -= OnAlgorithmPropertyChanged;

            if (e.NewValue is Constant newConstant)
                newConstant.PropertyChanged += OnAlgorithmPropertyChanged;

        }

        private void OnAlgorithmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Constant.Value))
                UpdateFormattedValue();
        }

    }
}
