using CMGWpf.SoundFont_2;
using CMGWpf.Utilities;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CMGWpf.Dialogs
{
    /// <summary>
    /// Interaction logic for PresetViewDialog.xaml
    /// </summary>
    public partial class PresetViewDialog : Window
    {
        private class PresetMap
        {
            public required string Name;
            public required Preset Preset;
        }
        private class InstrumentMap
        {
            public required string Name;
            public required PresetRegion Region;
        }
        private class SampleMap
        {
            public required string Name;
            public required InstrumentRegion Region;
        }
        public static readonly DependencyProperty SoundFontFileNameProperty = DependencyProperty.Register(
                    nameof(SoundFontFileName), typeof(string), typeof(PresetViewDialog), new PropertyMetadata(null));
        public string SoundFontFileName { get => (string)GetValue(SoundFontFileNameProperty); set { SetValue(SoundFontFileNameProperty, value); } }
        public static readonly DependencyProperty WindowTitleProperty = DependencyProperty.Register(
                    nameof(WindowTitle), typeof(string), typeof(PresetViewDialog), new PropertyMetadata(null));
        public string WindowTitle { get => (string)GetValue(WindowTitleProperty); set { SetValue(WindowTitleProperty, value); } }
        public PresetViewDialog()
        {
            InitializeComponent();

            this.Loaded += PresetViewDialog_Loaded;
            // ensure window title reflects the current SoundFont value
        }

        private void PresetViewDialog_Loaded(object sender, RoutedEventArgs e)
        {
            SoundFont? SoundFont = SoundFontUtilities.GetSoundFont(SoundFontFileName);
            if (SoundFont == null) return;
            // build the preset/instrument/sample table showing the key and velocity ranges
            double columnWidth = 100.0;
            double nameWidth = 2 * columnWidth;
            var viewer = PresetList;
            var outerStack = new StackPanel { Orientation = Orientation.Vertical };
            viewer.Content = outerStack;
            var header = new StackPanel { Orientation = Orientation.Horizontal };
            outerStack.Children.Add(header);
            var header1 = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Width = nameWidth,
                Child = new TextBlock { Text = "Preset" }
            };
            var header2 = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Width = nameWidth,
                Child = new TextBlock { Text = "Instrument" }
            };
            var header3 = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Width = columnWidth,
                Child = new TextBlock { Text = "Key" }
            };
            var header4 = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Width = columnWidth,
                Child = new TextBlock { Text = "Velocity" }
            };
            var header5 = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Width = nameWidth,
                Child = new TextBlock { Text = "Sample" }
            };
            var header6 = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Width = columnWidth,
                Child = new TextBlock { Text = "Key" }
            };
            var header7 = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Width = columnWidth,
                Child = new TextBlock { Text = "Velocity" }
            };
            header.Children.Add(header1);
            header.Children.Add(header2);
            header.Children.Add(header3);
            header.Children.Add(header4);
            header.Children.Add(header5);
            header.Children.Add(header6);
            header.Children.Add(header7);
            // sort the preset names 
            ObservableCollection<PresetMap> presetMap = [];
            foreach (var preset in SoundFont.Presets)
            {
                presetMap.Add(new PresetMap { Name = SoundFontUtilities.BankPresetToName(preset), Preset = preset });
            }
            presetMap = [..presetMap.OrderBy(m => m.Name).ToArray<PresetMap>()];
            // loop through the presets
            foreach (var presetItem in presetMap)
            {
                var preset = presetItem.Preset;
                var presetRow = new StackPanel { Orientation = Orientation.Horizontal };
                outerStack.Children.Add(presetRow);
                var presetName = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Width = nameWidth,
                    Child = new TextBlock { Text = presetItem.Name }
                };
                presetRow.Children.Add(presetName);
                // sort the instruments by KeyRangeStart
                ObservableCollection<InstrumentMap> instrumentMap = [];
                foreach (var presetRegion in preset.Regions)
                {
                    instrumentMap.Add(new InstrumentMap { Name = presetRegion.Instrument.Name, Region = presetRegion });
                }
                instrumentMap = [.. instrumentMap.OrderBy(m => m.Region.KeyRangeStart * 128 + m.Region.VelocityRangeStart).ToArray()];
                // loop through the instruments in the preset
                bool firstInstrument = true;
                foreach (var instrumentItem in instrumentMap)
                {
                    var presetRegion = instrumentItem.Region;
                    if (!firstInstrument)
                    {
                        // need to add one blank columns for the preset name and key/velocity ranges
                        presetRow = new StackPanel { Orientation = Orientation.Horizontal };
                        outerStack.Children.Add(presetRow);
                        for (int i = 0; i < 1; i++)
                        {
                            var blankBorder = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0), Width = nameWidth };
                            presetRow.Children.Add(blankBorder);
                        }
                    }
                    firstInstrument = false;
                    var instrumentName = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        Width = nameWidth,
                        Child = new TextBlock { Text = instrumentItem.Name }
                    };
                    presetRow.Children.Add(instrumentName);
                    var instrumentKeyBorder = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        Width = columnWidth,
                        Child = new TextBlock { Text = $"({presetRegion.KeyRangeStart}-{presetRegion.KeyRangeEnd})" }
                    };
                    presetRow.Children.Add(instrumentKeyBorder);
                    var instrumentVelocityBorder = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        Width = columnWidth,
                        Child = new TextBlock { Text = $"({presetRegion.VelocityRangeStart}-{presetRegion.VelocityRangeEnd})" }
                    };
                    presetRow.Children.Add(instrumentVelocityBorder);
                    // sort the samples by KeyRangeStart
                    ObservableCollection<SampleMap> sampleMap = [];
                    foreach (var region in presetRegion.Instrument.RegionArray)
                    {
                        sampleMap.Add(new SampleMap { Name = region.Sample.Name, Region = region });
                    }
                    sampleMap = [.. sampleMap.OrderBy(m => m.Region.KeyRangeStart* 128 + m.Region.VelocityRangeStart).ToArray()];

                    // loop through the samples in the instrument
                    bool firstSample = true;
                    foreach (var sampleItem in sampleMap)
                    {
                        var region = sampleItem.Region;
                        if (!firstSample)
                        {
                            // need to add four blank columns for the preset and instrument names and key/velocity ranges
                            presetRow = new StackPanel { Orientation = Orientation.Horizontal };
                            outerStack.Children.Add(presetRow);
                            for (int i = 0; i < 4; i++)
                            {
                                var blankBorder = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0), Width = (i < 2)?nameWidth:  columnWidth };
                                presetRow.Children.Add(blankBorder);
                            }
                        }
                        firstSample = false;
                        var sampleName = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1),
                            Width = nameWidth,
                            Child = new TextBlock { Text = region.Sample.Name }
                        };
                        presetRow.Children.Add(sampleName);
                        var sampleKeyBorder = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1),
                            Width = columnWidth,
                            Child = new TextBlock { Text = $"({region.KeyRangeStart}-{region.KeyRangeEnd})" }
                        };
                        presetRow.Children.Add(sampleKeyBorder);
                        var sampleVelocityBorder = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1),
                            Width = columnWidth,
                            Child = new TextBlock { Text = $"({region.VelocityRangeStart}-{region.VelocityRangeEnd})" }
                        };
                        presetRow.Children.Add(sampleVelocityBorder);
                    }
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
