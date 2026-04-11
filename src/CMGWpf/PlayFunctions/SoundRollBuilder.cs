using CMGWpf.Types;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static CMGWpf.Types.PlayTypes;

namespace CMGWpf.PlayFunctions
{
    /// <summary>
    /// Builds the Sound Roll grid and visual elements
    /// </summary>
    public class TimeMidiPreset(TimeMidiLine line, string soundFontName, string presetName)
    {
        TimeMidiLine Line { get; set; } = line; string SoundFontName { get; set; } = soundFontName; string PresetName { get; set; } = presetName;
    };
    public static class SoundRollBuilder
    {
        private static double PixelsPerNote = 0;
        private static double PixelsPerSecond = 0;
        private const int MinNote = 0;   // C-1
        private const int MaxNote = 127; // G9
        private record struct TimeMidiPreset(TimeMidiLine Line, string SoundFontName, string PresetName);
        private static List<TimeMidiPreset> timeMidiPresets = [];
        public static void ClearInstruments() { timeMidiPresets = []; }

        /// <summary>
        /// Calculate canvas width based on total duration
        /// </summary>
        public static double CalculateCanvasWidth(double totalDurationSeconds, double displayWidth)
        {
            PixelsPerSecond = displayWidth / 60; // 60 seconds per display
            return ((int)(totalDurationSeconds / 60) + 1) * displayWidth;
        }

        /// <summary>
        /// Calculate canvas height for the full MIDI note range
        /// </summary>
        public static double CalculateCanvasHeight(double displayHeight)
        {
            double totalHeight = displayHeight;
            PixelsPerNote = totalHeight / (MaxNote - MinNote + 1);
            return totalHeight;
        }

        /// <summary>
        /// Build the complete sound roll grid
        /// </summary>
        public static void BuildGrid(Canvas canvas, double totalDurationSeconds)
        {
            canvas.Children.Clear();
            DrawOctaveLines(canvas);
            DrawTimeLines(canvas, totalDurationSeconds);
            //DrawOctaveLabels(canvas);
            DrawTimeLabels(canvas, totalDurationSeconds);
        }
        public static void BuildFixedGrid(Canvas canvas, double height)
        {
            canvas.Children.Clear();
            DrawOctaveLabels(canvas);
            var line = new Line
            {
                X1 = 0,
                X2 = 0,
                Y1 = 0,
                Y2 = height,
                Stroke = Brushes.Red,
                StrokeThickness = 1.0
            };
            canvas.Children.Add(line);
        }
        private static void DrawOctaveLines(Canvas canvas)
        {
            var octaveNotes = new[] { 0, 12, 24, 36, 48, 60, 72, 84, 96, 108, 120 };

            foreach (int note in octaveNotes)
            {
                double y = (MaxNote - note) * PixelsPerNote;
                var line = new Line
                {
                    X1 = 0,
                    X2 = canvas.Width,
                    Y1 = y,
                    Y2 = y,
                    Stroke = note == 60 ? Brushes.DarkGray : Brushes.LightGray,
                    StrokeThickness = note == 60 ? 1.5 : 0.5
                };
                canvas.Children.Add(line);
            }
        }

        private static void DrawTimeLines(Canvas canvas, double totalDurationSeconds)
        {
            for (int sec = 0; sec <= (int)totalDurationSeconds; sec++)
            {
                double x = sec * PixelsPerSecond;
                bool isMajorTick = sec % 5 == 0;
                var line = new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = 0,
                    Y2 = canvas.Height,
                    Stroke = isMajorTick ? Brushes.Gray : Brushes.LightGray,
                    StrokeThickness = isMajorTick ? 1.0 : 0.5
                };
                canvas.Children.Add(line);
            }
        }

        private static void DrawOctaveLabels(Canvas canvas)
        {
            var octaveNotes = new[] { 0, 12, 24, 36, 48, 60, 72, 84, 96, 108, 120 };
            var noteNames = new[] { "C-1", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9" };

            for (int i = 0; i < octaveNotes.Length; i++)
            {
                double y = (MaxNote - octaveNotes[i]) * PixelsPerNote;
                var label = new TextBlock
                {
                    Text = noteNames[i],
                    FontSize = 10,
                    Foreground = Brushes.Black,
                    Background = Brushes.LightGray
                };
                Canvas.SetLeft(label, 5);
                Canvas.SetTop(label, y - 7);
                canvas.Children.Add(label);
            }
        }
        private static void DrawTimeLabels(Canvas canvas, double totalDurationSeconds)
        {
            for (int sec = 0; sec <= (int)totalDurationSeconds; sec += 5)
            {
                double x = sec * PixelsPerSecond;
                var label = new TextBlock
                {
                    Text = $"{sec}s",
                    FontSize = 10,
                    Foreground = Brushes.Black,
                    Background = Brushes.LightGray
                };
                Canvas.SetLeft(label, x + 2);
                Canvas.SetTop(label, 2);
                canvas.Children.Add(label);
            }
        }

        public static double NoteToY(int note) => (MaxNote - note) * PixelsPerNote;
        public static double TimeToX(double seconds) => seconds * PixelsPerSecond;
        public static ObservableCollection<PresetColor> DefineVoicePalette(List<SF_Preset> sF_Presets)
        {
            int i = 0;
            int count = sF_Presets.Count;
            ObservableCollection<PresetColor> presetColors = [];
            foreach (SF_Preset preset in sF_Presets)
            {
                // check if the soundfont/preset exists and then skip it if it does, otherwise add it with a new color
                PresetColor? found = Array.Find(presetColors.ToArray(), ((p) => p.SoundFontName == preset.SoundFontName && p.PresetName == preset.PresetName));
                if (found != null) continue;
                double hue = (double)i / count * 360.0;
                Color color = HslToRgb(hue, 0.9, 0.5);
                PresetColor newOne = new()
                {
                    SoundFontName = preset.SoundFontName,
                    PresetName = preset.PresetName,
                    Color = color
                };
                presetColors.Add(newOne);
            }
            i++;
            return presetColors;
        }

        private static Color HslToRgb(double h, double s, double l)
        {
            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = l - c / 2;

            double r = 0, g = 0, b = 0;

            if (h < 60) { r = c; g = x; b = 0; }
            else if (h < 120) { r = x; g = c; b = 0; }
            else if (h < 180) { r = 0; g = c; b = x; }
            else if (h < 240) { r = 0; g = x; b = c; }
            else if (h < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return Color.FromRgb(
                (byte)Math.Round((r + m) * 255),
                (byte)Math.Round((g + m) * 255),
                (byte)Math.Round((b + m) * 255)
            );
        }
        public static void AddInstrument(TimeMidiLine line, string SoundFontName, string PresetName)
        {
            timeMidiPresets.Add(new TimeMidiPreset(line, SoundFontName, PresetName));
        }
        public static void AddInstrumentsToCanvas(Canvas canvas, ObservableCollection<PresetColor> presetColors)
        {
            foreach (var _timeMidiPreset in timeMidiPresets)
            {
                Color color = presetColors.FirstOrDefault((p) => p.SoundFontName == _timeMidiPreset.SoundFontName && p.PresetName == _timeMidiPreset.PresetName)!.Color;
     
                double x1 = TimeToX(_timeMidiPreset.Line.Start.Time);
                double y1 = NoteToY(_timeMidiPreset.Line.Start.Midi);
                double x2 = TimeToX(_timeMidiPreset.Line.End.Time);
                double y2 = NoteToY(_timeMidiPreset.Line.End.Midi);
                if (Math.Abs(x1 - x2) < 0.01) // if it's essentially the same time, draw a rectangle instead of a line
                {
                    double circleSize = PixelsPerNote * 1.5; // Adjust size as needed
                    var circle = new Ellipse
                    {
                        Width = circleSize,
                        Height = circleSize,
                        Fill = new SolidColorBrush (color) { Opacity = 0.7},
                        Stroke = Brushes.Black,
                        StrokeThickness = 0.5
                    };
                    Canvas.SetLeft(circle, x1 - circleSize / 2); // Center the circle horizontally
                    Canvas.SetTop(circle, y1 - circleSize / 2);  // Center the circle vertically
                    canvas.Children.Add(circle);
                }
                else
                {
                    var line = new Line
                    {
                        X1 = x1,
                        Y1 = y1,
                        X2 = x2,
                        Y2 = y2,
                        Stroke = new SolidColorBrush(color) { Opacity = 0.7 },
                        StrokeThickness = 3
                    };
                    canvas.Children.Add(line);
                }
            }
        }
    }
}