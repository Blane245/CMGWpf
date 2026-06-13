using CMGWpf.Model;
using CMGWpf.MVVM;
using CMGWpf.Services;
using CMGWpf.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static CMGWpf.Types.TimeLineTypes;

namespace CMGWpf.View
{
    /// <summary>
    /// Singleton ViewModel for the TimeLine display and related interactions. This includes managing the current timeline properties such as start time, zoom level, and time interval, as well as handling user interactions with the timeline such as zooming, panning, and manipulating the time interval through mouse events. The TimeLineViewModel is responsible for updating the timeline display on the canvas and ensuring that it stays in sync with the underlying data model.
    /// </summary>
    public class TimeLineViewModel : ViewModelBase
    {
        private static TimeLineViewModel? _instance;
        public static TimeLineViewModel Instance => _instance ??= new TimeLineViewModel();

        private TimeLineViewModel()
        {
            // Subscribe to SizeService property changes to redraw timeline when display width changes
            SizeService.Instance.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(SizeService.DisplayWidth))
                {
                    RedrawTimeLine();
                    UpdateGeneratorColors();
                }
            };
        }
        #region View Properties
        public TimeLine TimeLine
        {
            get
            {
                return FileViewModel.Instance.File.TimeLine;
            }
            set
            {
                FileViewModel.Instance.File.TimeLine = value;
                RedrawTimeLine();
                UpdateGeneratorColors();
                OnPropertyChanged();
            }
        }
        public bool IsDirty
        {
            get => GlobalService.Instance.IsDirty;
            set
            {
                GlobalService.Instance.IsDirty = value;
                OnPropertyChanged();
            }
        }
        // register the canvas from the TimeLine view, which is used to draw the timeline. This allows the TimeLineViewModel to access the canvas and update the timeline display as needed. This registration is performed from the timeline xaml code behind when the timeline display is loaded.
        private Canvas? _timeLineCanvas;
        public void RegisterCanvas(Canvas canvas)
        {
            _timeLineCanvas = canvas;
        }
        private Rectangle? _timeIntervalRectangle;
        // time interval change mode and edge, which are used to track the current state of user interactions with the time interval on the timeline. The TimeIntervalChangeMode enum indicates whether the user is currently manipulating the edges of the time interval (Edge) or moving the entire body of the time interval (Body), while the TimeIntervalEdge enum specifies which edge (Start or End) is being manipulated when in Edge mode.
        private enum TimeIntervalChangeMode
        {
            None,
            Edge,
            Body,
        }
        private enum TimeIntervalEdge
        {
            Start,
            End,
        }
        private TimeIntervalChangeMode _timeIntervalChangeMode = TimeIntervalChangeMode.None;
        private TimeIntervalEdge _timeIntervalEdge = TimeIntervalEdge.Start;

        // update the timeLine when the UIModel changes, which happens when the timeline is manipulated by the user. This ensures that the UIModel is updated. This notification is received from the TimeLineCommands when the user interacts with the timeline through zooming or panning.
        public void NotifyTimeLineChanged(TimeLine newTimeLine)
        {
            TimeLine = newTimeLine;
        }
        // draw the timeline on the canvas based on the current UIModel properties, which include the start time, zoom level, and time interval. This method is responsible for rendering the visual representation of the timeline on the canvas, including the time ticks, labels, and the time interval box. It is called whenever there are changes to the UIModel that affect the timeline display, ensuring that the timeline is always up-to-date with the underlying data model.
        public void DrawTimeLine()
        {
            if (_timeLineCanvas == null) return;
            Canvas timeLineCanvas = _timeLineCanvas;
            timeLineCanvas.ClipToBounds = true;
            double startTime = TimeLine.StartTime;
            double displayWidth = Services.SizeService.Instance.DisplayWidth;
            double timeLineHeight = SizeService.Instance.TimeLineHeight;
            int currentZoomLevel = TimeLine.CurrentZoomLevel;
            TimeInterval timeInterval = TimeLine.TimeInterval;
            timeLineCanvas.Children.Clear();
            // draw the timeline box
            Rectangle box = new()
            {
                Width = displayWidth,
                Height = timeLineHeight,
                Fill = Brushes.LightBlue,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Canvas.SetLeft(box, 0);
            Canvas.SetTop(box, 0);
            timeLineCanvas.Children.Add(box);
            // draw the timeline
            Line line = new()
            {
                Stroke = Brushes.Black,
                X1 = 0,
                Y1 = timeLineHeight,
                X2 = displayWidth,
                Y2 = timeLineHeight,
                StrokeThickness = 2,
            };
            timeLineCanvas.Children.Add(line);
            // draw time ticks
            TimeLineScale scale = TimeLineScales[currentZoomLevel];
            TimeTicks ticks = new()
            {
                majorTickCount = scale.MajorDivisions,
                scaleExtent = scale.Extent,
                tickCount = scale.MajorDivisions * scale.MinorDivisions,
                tickHeight = timeLineHeight / 3,
                tickSpacing = displayWidth / (scale.MajorDivisions * scale.MinorDivisions),
                labelSize = displayWidth / scale.MajorDivisions,
                labelFormat = TimeFormats[scale.Format].Value,
                labelSpacing = displayWidth / scale.MajorDivisions,
            };
            for (int i = 0; i <= ticks.tickCount; i++)
            {
                Line tick = new()
                {
                    Stroke = Brushes.Black,
                    X1 = i * ticks.tickSpacing,
                    Y1 = timeLineHeight,
                    X2 = i * ticks.tickSpacing,
                    Y2 = timeLineHeight - ticks.tickHeight,
                    StrokeThickness = 2,

                };
                timeLineCanvas.Children.Add(tick);
            }

            // draw the major tick labels
            TimeFormat timeFormat = TimeFormats[scale.Format];
            for (int i = 0; i <= ticks.majorTickCount; i++)
            {
                double timeValue = startTime + i * ticks.scaleExtent / ticks.majorTickCount;
                string labelText;

                if (timeFormat.Type == TIMEFORMATTYPE.Number)
                {
                    // Format as a number (seconds with decimal places)
                    labelText = timeValue.ToString(timeFormat.Value);
                }
                else
                {
                    // Format as time (convert seconds to TimeSpan and format)
                    TimeSpan ts = TimeSpan.FromSeconds(timeValue);
                    labelText = FormatTimeSpan(ts, timeFormat.Value);
                }

                TextBlock label = new()
                {
                    Text = labelText,
                    FontSize = 10,
                    Foreground = Brushes.Black,
                    Width = ticks.labelSize,
                    Height = timeLineHeight / 3,
                };
                Canvas.SetLeft(label, i * ticks.labelSpacing);
                Canvas.SetTop(label, 10);
                timeLineCanvas.Children.Add(label);
            }

            // adjust the time interval offsets based on the current time line, draw the visible part of the time interval, and add its mouse events for the body and edges of its rectangle. This ensures that the time interval is accurately represented on the timeline according to the current zoom level and start time, and allows the user to interact with the time interval directly on the timeline by dragging its edges or moving its body, providing a dynamic and responsive user experience.
            TimeToOffset(timeInterval);
            double startInterval = timeInterval.StartOffset;
            double endInterval = timeInterval.EndOffset;

            DebugLog.Write($"Time Interval Rectangle created at ({startInterval}, {endInterval}");
            Rectangle intervalBox = new()
            {
                Width = endInterval - startInterval,
                Height = timeLineHeight,
                Fill = Brushes.LightGray,
                Stroke = Brushes.Gray,
                StrokeThickness = 1,
                Opacity = 0.8
            };
            _timeIntervalRectangle = intervalBox;
            AddTimeIntervalMouseEvents();
            Canvas.SetLeft(intervalBox, startInterval);
            Canvas.SetTop(intervalBox, 0);
            timeLineCanvas.Children.Add(intervalBox);
        }
        private void RedrawTimeLine()
        {
            if (_timeLineCanvas == null) return;
            DrawTimeLine();
            TracksViewModel.Instance.RefreshAllTracks();
        }

        #endregion
        #region TimeLine Button Commands
        private RelayCommand<object>? _zoomInCommand;
        public RelayCommand<object> ZoomInCommand =>
            _zoomInCommand ??= new RelayCommand<object>(execute => new TimeLineCommands(this, TimeLine).ZoomInCommand());
        private RelayCommand<object>? _zoomOutCommand;
        public RelayCommand<object> ZoomOutCommand =>
            _zoomOutCommand ??= new RelayCommand<object>(execute => new TimeLineCommands(this, TimeLine).ZoomOutCommand());
        private RelayCommand<object>? _panLeftCommand;
        public RelayCommand<object> PanLeftCommand =>
            _panLeftCommand ??= new RelayCommand<object>(execute => new TimeLineCommands(this, TimeLine).PanLeftCommand());
        private RelayCommand<object>? _panRightCommand;
        public RelayCommand<object> PanRightCommand =>
            _panRightCommand ??= new RelayCommand<object>(execute => new TimeLineCommands(this, TimeLine).PanRightCommand());
        #endregion
        #region TimeLine Canvas Mouse Commands

        // when the user enters the timeline canvas, the mouse changes to a cross cursor, indicating that the user can interact with the timeline.
        private RelayCommand<object>? _mouseEnterCommand;
        public RelayCommand<object> MouseEnterCommand =>
            _mouseEnterCommand ??= new RelayCommand<object>(args =>
            {
                if (_timeLineCanvas == null) return;
                _timeLineCanvas?.Cursor = System.Windows.Input.Cursors.Cross;
            });
        // when the left mouse button is pushed in the timeline canvas, the mouse is captured, the cursor is changed to an WE, and the timeinterval mode is set to Edge, indicating that the timeinterval size is being changed until the mouse button is released.
        private RelayCommand<object>? _mouseLeftButtonDownCommand;
        public RelayCommand<object> MouseLeftButtonDownCommand =>
            _mouseLeftButtonDownCommand ??= new RelayCommand<object>(args =>
            {
                if (args is not System.Windows.Input.MouseButtonEventArgs e) return;
                if (_timeLineCanvas == null) return;
                DebugLog.Write($"Mouse down on time line at {e.GetPosition(_timeLineCanvas)}");
                Point position = e.GetPosition(_timeLineCanvas);
                _timeLineCanvas.Cursor = System.Windows.Input.Cursors.ScrollWE;
                _timeLineCanvas.CaptureMouse();
                _timeIntervalChangeMode = TimeIntervalChangeMode.Edge;
                StartDragInterval(position.X);
            });
        // when the mouse is moved with the left button pressed continue timeinterval edge movements until the left mouse button is released, which releases the mouse capture and sets the timeinterval mode to none, indicating that the timeinterval manipulation is complete.
        private RelayCommand<object>? _mouseMoveCommand;
        public RelayCommand<object> MouseMoveCommand =>
            _mouseMoveCommand ??= new RelayCommand<object>(args =>
            {
                if (_timeLineCanvas == null) return;
                if (args is not System.Windows.Input.MouseEventArgs e || !_timeLineCanvas.IsMouseCaptured) return;
                DebugLog.Write($"mouse move on time line to {e.GetPosition(_timeLineCanvas)}");
                Point position = e.GetPosition(_timeLineCanvas);
                DragInterval(position.X);
                // signal the tracks to redraw thus highlighting generators that are selected by the timeInterval
                UpdateGeneratorColors();
            });

        // when the mouse leaves the timeline canvas and the mouse not is captured, the cursor is reset to the default arrow cursor, indicating that the user is no longer interacting with the timeline. If the mouse is captured, it means the user is dragging the timeinterval edge, so the cursor remains in its current state until the mouse button is released.
        private RelayCommand<object>? _mouseLeaveCommand;
        public RelayCommand<object> MouseLeaveCommand =>
            _mouseLeaveCommand ??= new RelayCommand<object>(args =>
            {
                if (_timeLineCanvas == null) return;
                if (args is not System.Windows.Input.MouseEventArgs e) return;
                if (!_timeLineCanvas.IsMouseCaptured)
                    _timeLineCanvas.Cursor = System.Windows.Input.Cursors.Arrow;
                else
                {
                    Point position = e.GetPosition(_timeLineCanvas);
                    DragInterval(position.X);
                }
            });

        // when the left mouse button is released, the mouse capture is released, the timeinterval mode is set to none, the cursor is set arrow indicating no more interaction with the timeline.
        private RelayCommand<object>? _mouseLeftButtonUpCommand;
        public RelayCommand<object> MouseLeftButtonUpCommand =>
            _mouseLeftButtonUpCommand ??= new RelayCommand<object>(args =>
            {
                if (_timeLineCanvas == null) return;
                if (args is not System.Windows.Input.MouseEventArgs e) return;
                _timeLineCanvas.ReleaseMouseCapture();
                _timeIntervalChangeMode = TimeIntervalChangeMode.None;
                _timeLineCanvas.Cursor = System.Windows.Input.Cursors.Arrow;
                TracksViewModel.Instance.RefreshAllTracks();
            });
        private void OffsetToTime(TimeInterval interval)
        {
            double timePerPixel = TimeLineScales[TimeLine.CurrentZoomLevel].Extent / SizeService.Instance.DisplayWidth;
            interval.StartTime = TimeLine.StartTime + interval.StartOffset * timePerPixel;
            interval.EndTime = TimeLine.StartTime + interval.EndOffset * timePerPixel;
        }

        #endregion
        #region TimeInterval Mouse Commands
        // The time interval mouse events include the following:
        // 1. MouseEnter: Change the cursor to a hand if the mouse is not near an edge. Change it to a EW if close to an edge. Set the mode accordingly.
        // 2. MouseLeftButtonDown: Capture the mouse. If the mode is body, perform the body movement. If edge, perform the edge drag funtion.
        private void AddTimeIntervalMouseEvents()
        {
            if (_timeIntervalRectangle == null || _timeLineCanvas == null) return;
            _timeIntervalRectangle.MouseEnter += (s, e) =>
            {
                // depending on how close the mouse is to the edges of the interval box, change the cursor to indicate that the user can drag the edges or the body of the box. This provides visual feedback to the user about how they can interact with the time interval.
                double x = e.GetPosition(_timeIntervalRectangle).X;
                if (x < 5 || x > _timeIntervalRectangle.Width - 5)
                {
                    _timeIntervalChangeMode = TimeIntervalChangeMode.Edge;
                    if (x < 5)
                        _timeIntervalEdge = TimeIntervalEdge.Start;
                    else
                        _timeIntervalEdge = TimeIntervalEdge.End;
                    _timeIntervalRectangle.Cursor = System.Windows.Input.Cursors.SizeWE;
                }
                else
                {
                    _timeIntervalRectangle.Cursor = System.Windows.Input.Cursors.ScrollWE;
                    _timeIntervalChangeMode = TimeIntervalChangeMode.Body;
                }
            };
            _timeIntervalRectangle.MouseLeftButtonDown += (s, e) =>
            {
                // we are in either edge or body mode.
                // If in body mode, we will move the entire interval box, which means we need to track the offset of the mouse position from the start and end offsets of the interval box to make sure the movement is smooth.
                // If in edge mode, we will only move the edge that is closest to the mouse position, which means we need to track which edge is being moved and update the interval box accordingly.
                if (_timeIntervalChangeMode == TimeIntervalChangeMode.None)
                    return;
                if (_timeIntervalChangeMode == TimeIntervalChangeMode.Edge)
                {
                    DebugLog.Write($"Dragging edge to {e.GetPosition(_timeLineCanvas).X}.");
                    DragInterval(e.GetPosition(_timeLineCanvas).X);
                    _timeIntervalRectangle.CaptureMouse();
                    e.Handled = true; // Prevent event from bubbling to Canvas
                    return;
                }
                else if (_timeIntervalChangeMode == TimeIntervalChangeMode.Body)
                {
                    DebugLog.Write($"start dragging body at {e.GetPosition(_timeLineCanvas).X}");
                    StartDragBody(e.GetPosition(_timeLineCanvas).X);
                    _timeIntervalRectangle.CaptureMouse();
                    e.Handled = true; // Prevent event from bubbling to Canvas
                    return;
                }
            };

            _timeIntervalRectangle.MouseMove += (s, e) =>
            {
                // In move, we need to check if the mouse is captured, and if so, whether we are in edge or body mode, and call the appropriate functions to update the interval box position. 
                if (_timeIntervalRectangle.IsMouseCaptured)
                {
                    if (_timeIntervalChangeMode == TimeIntervalChangeMode.Edge)
                    {
                        DebugLog.Write($"mouse move dragging edge to {e.GetPosition(_timeLineCanvas).X}");
                        DragInterval(e.GetPosition(_timeLineCanvas).X);
                    }
                    else
                    {
                        DebugLog.Write($"mouse move body to {e.GetPosition(_timeLineCanvas)}");
                        DragBody(e.GetPosition(_timeLineCanvas).X);
                    }
                    // signal the tracks to redraw thus highlighting generators that are selected by the timeInterval
                    UpdateGeneratorColors();
                    e.Handled = true; // Prevent event from bubbling to Canvas
                }
                else
                {
                    // mouse is not captured - update the cursor and mode 
                    double x = e.GetPosition(_timeIntervalRectangle).X;
                    if (x < 5 || x > _timeIntervalRectangle.Width - 5)
                    {
                        _timeIntervalChangeMode = TimeIntervalChangeMode.Edge;
                        if (x < 5)
                            _timeIntervalEdge = TimeIntervalEdge.Start;
                        else
                            _timeIntervalEdge = TimeIntervalEdge.End;
                        _timeIntervalRectangle.Cursor = System.Windows.Input.Cursors.SizeWE;
                    }
                    else
                    {
                        _timeIntervalRectangle.Cursor = System.Windows.Input.Cursors.ScrollWE;
                        _timeIntervalChangeMode = TimeIntervalChangeMode.Body;
                    }

                }
            };
            _timeIntervalRectangle.MouseLeave += (s, e) =>
            {
                // In mouse leave, if the mouse is not captured, we need to reset the cursor to the default arrow cursor.
                if (!_timeIntervalRectangle.IsMouseCaptured)
                    _timeIntervalRectangle.Cursor = System.Windows.Input.Cursors.Arrow;
            };
            _timeIntervalRectangle.MouseLeftButtonUp += (s, e) =>
            {
                // when the time interval left mouse goes up, we release the cursor, set the mode to none, and reset the cursor to the default arrow cursor, indicating that the user has finished interacting with the time interval.
                _timeIntervalRectangle.ReleaseMouseCapture();
                _timeIntervalRectangle.Cursor = System.Windows.Input.Cursors.Arrow;
                _timeIntervalChangeMode = TimeIntervalChangeMode.None;
                TracksViewModel.Instance.RefreshAllTracks();
                e.Handled = true; // Prevent event from bubbling to Canvas
            };

        }
        private void StartDragInterval(double x)
        {
            TimeInterval interval = TimeLine.TimeInterval.Clone();
            interval.StartOffset = x;
            interval.EndOffset = x;
            OffsetToTime(interval);
            _timeIntervalEdge = TimeIntervalEdge.Start;
            TimeLine.TimeInterval = interval;
            IsDirty = true;
            OnPropertyChanged(nameof(TimeLine));
            MoveTimeIntervalRectangle();
        }
        private void DragInterval(double x)
        {
            if (_timeIntervalChangeMode != TimeIntervalChangeMode.Edge) return;
            var interval = TimeLine.TimeInterval.Clone();

            // a couple of modes to consider:
            // 1. The user is dragging the start edge. If the position goes past the end edge, switch to dragging the end edge and exchange the start end offsets; otherwise move the start edge
            // 2. The user is dragging the end edge. If the position goes past the start edge, switch to dragging the start edge and exchange the start and end offsets; otherwise move the end edge
            if (_timeIntervalEdge == TimeIntervalEdge.Start)
            {
                if (x > interval.EndOffset)
                {
                    interval.StartOffset = interval.EndOffset;
                    interval.EndOffset = Math.Min(x, SizeService.Instance.DisplayWidth);
                    _timeIntervalEdge = TimeIntervalEdge.End;
                }
                else
                {
                    interval.StartOffset = Math.Max(x, 0D);
                }
            }
            else if (_timeIntervalEdge == TimeIntervalEdge.End)
            {
                if (x < interval.StartOffset)
                {
                    interval.EndOffset = interval.StartOffset;
                    interval.StartOffset = Math.Max(x, 0D);
                    _timeIntervalEdge = TimeIntervalEdge.Start;
                }
                else
                    interval.EndOffset = Math.Min(x, SizeService.Instance.DisplayWidth);
            }
            OffsetToTime(interval);
            DebugLog.Write($"drag {_timeIntervalEdge}, time interval {interval}");
            TimeLine.TimeInterval = interval;
            IsDirty = true;
            OnPropertyChanged(nameof(TimeLine));
            MoveTimeIntervalRectangle();
            return;
        }
        // Modify the time interval rectangle based on its new definition, which is determined by the mouse events that are added to the time interval rectangle. These mouse events allow the user to interact with the time interval on the timeline, enabling them to adjust its start and end times by dragging its edges or moving its body. The mouse events provide a way for the user to manipulate the time interval directly on the timeline, making it easier to use and more intuitive.
        private void MoveTimeIntervalRectangle()
        {
            if (_timeIntervalRectangle == null || _timeLineCanvas == null) return;
            TimeInterval interval = TimeLine.TimeInterval;
            DebugLog.Write($"move interval to ({interval.StartOffset}, {interval.EndOffset})");
            // make sure that the time interval is at least partially displayed on the time line 
            if (interval.EndOffset <= 0 || interval.StartOffset >= SizeService.Instance.DisplayWidth) return;
            // redefine the time interval rectangle position and size based on the new start and end offsets, which allows the time interval to be visually updated on the timeline to reflect any changes made by the user through mouse interactions. This ensures that the time interval display is always in sync with its underlying data model, providing a consistent and accurate representation of the time interval on the timeline.
            double left = Math.Max(0, interval.StartOffset);
            double width = Math.Min(SizeService.Instance.DisplayWidth, interval.EndOffset) - left;
            _timeIntervalRectangle.Width = width;
            Canvas.SetLeft(_timeIntervalRectangle, left);
            Canvas.SetTop(_timeIntervalRectangle, 0);
        }
        // when starting to drag the body of the time interval, we need to calculate the offset of the mouse position from the start offset of the interval box. This allows the user to move the time interval along the timeline without changing its duration.
        double _bodyDragOffset = 0;
        private void StartDragBody(double x)
        {
            _bodyDragOffset = x;
        }

        // when dragging the body of the time interval, we need to move the start and end offsets of the interval box by the same amount, which is the difference between the current mouse position and the initial mouse position when the drag started. This allows the entire interval box to move along the timeline while maintaining its size, providing a smooth dragging experience for the user.
        private void DragBody(double x)
        {
            double offsetChange = x - _bodyDragOffset;
            double newStartOffset = TimeLine.TimeInterval.StartOffset + offsetChange;
            double newEndOffset = TimeLine.TimeInterval.EndOffset + offsetChange;
            // we need to prevent the move if either the start or end offsets go beyond the timeline boundaries, which ensures that the time interval remains visible within the timeline, and prevents it from being dragged off-screen where it cannot be accessed by the user.
            if (newStartOffset < 0 || newEndOffset > SizeService.Instance.DisplayWidth)
                return;
            TimeInterval interval = TimeLine.TimeInterval.Clone();
            interval.StartOffset = newStartOffset;
            interval.EndOffset = newEndOffset;
            OffsetToTime(interval);
            TimeLine.TimeInterval = interval;
            _bodyDragOffset = x;
            DebugLog.Write($"drag body by {offsetChange} new interval {interval}");
            MoveTimeIntervalRectangle();
            IsDirty = true;
            OnPropertyChanged(nameof(TimeLine));
        }
        public void TimeToOffset(TimeInterval interval)
        {
            // convert the time interval start and end times to pixel offsets on the timeline, which allows the time interval to be displayed correctly on the timeline. This conversion is based on the current zoom level and the display width of the timeline, ensuring that the time interval is accurately represented in the UI.
            interval.StartOffset = (interval.StartTime - TimeLine.StartTime) / TimeLineScales[TimeLine.CurrentZoomLevel].Extent * SizeService.Instance.DisplayWidth;
            interval.EndOffset = (interval.EndTime - TimeLine.StartTime) / TimeLineScales[TimeLine.CurrentZoomLevel].Extent * SizeService.Instance.DisplayWidth;
            interval.StartOffset = Math.Clamp(interval.StartOffset, 0, SizeService.Instance.DisplayWidth);
            interval.EndOffset = Math.Clamp(interval.EndOffset, interval.StartOffset, SizeService.Instance.DisplayWidth);
        }
        // go through all of the generators and update their colors based on a new time interval.
        private void UpdateGeneratorColors()
        {
            var trackviewmodels = TracksViewModel.Instance.Tracks;
            foreach (var trackviewmodel in trackviewmodels)
            {
                var generatorViewModels = trackviewmodel.Generators;
                foreach (var generatorViewModel in generatorViewModels)
                {
                    generatorViewModel.UpdateColor();
                }
            }
        }

        /// <summary>
        /// Formats a TimeSpan according to custom time format strings
        /// Supports formats: "0:00" (m:ss), "00:00" (mm:ss), "0:00:00" (h:mm:ss), "000:00:00" (hhh:mm:ss)
        /// </summary>
        private static string FormatTimeSpan(TimeSpan ts, string format)
        {
            int totalHours = (int)ts.TotalHours;
            int minutes = ts.Minutes;
            int seconds = ts.Seconds;

            return format switch
            {
                "0:00" => $"{(int)ts.TotalMinutes}:{seconds:D2}",           // m:ss
                "00:00" => $"{(int)ts.TotalMinutes:D2}:{seconds:D2}",       // mm:ss
                "0:00:00" => $"{totalHours}:{minutes:D2}:{seconds:D2}",     // h:mm:ss
                "000:00:00" => $"{totalHours:D3}:{minutes:D2}:{seconds:D2}", // hhh:mm:ss
                _ => ts.ToString(@"hh\:mm\:ss")                              // fallback
            };
        }
        #endregion   
    }
}
