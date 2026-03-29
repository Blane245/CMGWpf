using CMGWpf.Model;
using CMGWpf.MVVM;
using CMGWpf.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static CMGWpf.Types.TimeLineTypes;

namespace CMGWpf.View
{
    public class TimeLineViewModel : ViewModelBase
    {
        private static TimeLineViewModel? _instance;
        public static TimeLineViewModel Instance => _instance ??= new TimeLineViewModel();

        private TimeLineViewModel()
        {
        }
        #region View Properties
        public TimeLine TimeLine
        {
            get => FileViewModel.Instance.File.TimeLine;
            set {
                ////the only times that we want the timeline redrawn is if the start time or zoom level, which are the properties that affect the timeline display. This optimization prevents unnecessary redraws of the timeline when other properties of the UIModel change that do not impact the visual representation of the timeline, improving performance and responsiveness of the application.
                //if (TimeLine.StartTime != value.StartTime || TimeLine.CurrentZoomLevel != value.CurrentZoomLevel)
                //{
                //    _timeIntervalRectangle = null;
                DrawTimeLine(_timeLineCanvas, value.StartTime, SizeService.Instance.DisplayWidth.Value, SizeService.Instance.TimeLineHeight.Value, value.CurrentZoomLevel, value.TimeInterval);
                //}
                FileViewModel.Instance.File.TimeLine = value;
                TracksViewModel.Instance.RefreshAllTracks();
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
        public void DrawTimeLine(Canvas? timeLineCanvas, double startTime, double displayWidth, double timeLineHeight, int currentZoomLevel, TimeInterval timeInterval)
        {
            if (timeLineCanvas == null) return;
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
            for (int i = 0; i <= ticks.majorTickCount; i++)
            {
                TextBlock label = new()
                {
                    Text = (startTime + i * ticks.scaleExtent / ticks.majorTickCount).ToString(ticks.labelFormat),
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
            //TimeLine.TimeInterval = timeInterval;
            if (timeInterval.StartOffset >= displayWidth || timeInterval.EndOffset <= 0) return;

            double startInterval = timeInterval.StartOffset;
            double endInterval = timeInterval.EndOffset;
            if (startInterval == endInterval) return;
            _timeIntervalRectangle = CreateTimeIntervalRectangle(timeLineCanvas, startInterval, endInterval, timeLineHeight);
            timeLineCanvas.Children.Add(_timeIntervalRectangle);
        }
        Rectangle CreateTimeIntervalRectangle(Canvas timeLineCanvas, double startInterval, double endInterval, double timeLineHeight)
        {
            Debug.WriteLine($"Time Interval Rectangle created at ({startInterval}, {endInterval}");
            Rectangle intervalBox = new()
            {
                Width = endInterval - startInterval,
                Height = timeLineHeight,
                Fill = Brushes.LightGray,
                Stroke = Brushes.Gray,
                StrokeThickness = 1,
                Opacity = 0.5
            };
            _timeIntervalRectangle = intervalBox;
            AddTimeIntervalMouseEvents();
            Canvas.SetLeft(intervalBox, startInterval);
            Canvas.SetTop(intervalBox, 0);
            timeLineCanvas.Children.Add(intervalBox);
            return intervalBox;
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
                Debug.WriteLine($"Mouse down on time line at {e.GetPosition(_timeLineCanvas)}");
                Point position = e.GetPosition(_timeLineCanvas);
                _timeLineCanvas.Cursor = System.Windows.Input.Cursors.ScrollWE;
                _timeLineCanvas.CaptureMouse();
                _timeIntervalChangeMode = TimeIntervalChangeMode.Edge;
                // if there is no timeinterval rectangle on the canvas
                // one needs to be created and its mouse events added
                if (_timeIntervalRectangle == null) _timeIntervalRectangle = CreateTimeIntervalRectangle(_timeLineCanvas, position.X, position.X, SizeService.Instance.TimeLineHeight.Value);
                StartDragInterval(position.X);
            });
        // when the mouse is moved with the left button pressed continue timeinterval edge movements until the left mouse button is released, which releases the mouse capture and sets the timeinterval mode to none, indicating that the timeinterval manipulation is complete.
        private RelayCommand<object>? _mouseMoveCommand;
        public RelayCommand<object> MouseMoveCommand =>
            _mouseMoveCommand ??= new RelayCommand<object>(args =>
            {
                if (_timeLineCanvas == null) return;
                if (args is not System.Windows.Input.MouseEventArgs e || !_timeLineCanvas.IsMouseCaptured) return;
                Debug.WriteLine($"mouse move on time line to {e.GetPosition(_timeLineCanvas)}");
                Point position = e.GetPosition(_timeLineCanvas);
                DragInterval(position.X);
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
            });
        private void OffsetToTime(TimeInterval interval)
        {
            double timePerPixel = TimeLineScales[TimeLine.CurrentZoomLevel].Extent / SizeService.Instance.DisplayWidth.Value;
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
                    _timeIntervalRectangle.Cursor = System.Windows.Input.Cursors.Hand;
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
                    Debug.WriteLine($"Dragging edge to {e.GetPosition(_timeLineCanvas).X}.");
                    DragInterval(e.GetPosition(_timeLineCanvas).X);
                    _timeIntervalRectangle.CaptureMouse();
                    return;
                }
                else if (_timeIntervalChangeMode == TimeIntervalChangeMode.Body)
                {
                    Debug.WriteLine($"start dragging body at {e.GetPosition(_timeIntervalRectangle).X}");
                    StartDragBody(e.GetPosition(_timeIntervalRectangle).X);
                    _timeIntervalRectangle.CaptureMouse();
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
                        Debug.WriteLine($"mouse move dragging edge to {e.GetPosition(_timeLineCanvas).X}");
                        DragInterval(e.GetPosition(_timeLineCanvas).X);
                    }
                    else
                    {
                        Debug.WriteLine($"mouse move body to {e.GetPosition(_timeIntervalRectangle)}");
                        DragBody(e.GetPosition(_timeIntervalRectangle).X);
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
            };

        }
        private void StartDragInterval(double x)
        {
            TimeInterval interval = TimeLine.TimeInterval;
            interval.StartOffset = x;
            interval.EndOffset = x;
            OffsetToTime(interval);
            _timeIntervalEdge = TimeIntervalEdge.Start;
            TimeLine.TimeInterval = interval;
            OnPropertyChanged(nameof(TimeLine));
        }
        private void DragInterval(double x)
        {
            if (_timeIntervalChangeMode != TimeIntervalChangeMode.Edge) return;
            var interval = TimeLine.TimeInterval;

            // a couple of modes to consider:
            // 1. The user is dragging the start edge. If the position goes past the end edge, switch to dragging the end edge and exchange the start end end offsets; otherwise move the start edge
            // 2. The user is dragging the end edge. If the position goes past the start edge, switch to dragging the start edge and exchange the start and end offsets; otherwise move the end edge
            if (_timeIntervalEdge == TimeIntervalEdge.Start)
            {
                if (x > interval.EndOffset)
                {
                    interval.StartOffset = interval.EndOffset;
                    interval.EndOffset = Math.Min(x, SizeService.Instance.DisplayWidth.Value);
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
                    interval.EndOffset = Math.Min(x, SizeService.Instance.DisplayWidth.Value);
            }
            OffsetToTime(interval);
            Debug.WriteLine($"drag {_timeIntervalEdge}, time interval {interval}");
            TimeLine.TimeInterval = interval;
            OnPropertyChanged(nameof(TimeLine));
            MoveTimeIntervalRectangle();
            return;
        }
        // Modify the time interval rectangle based on its new definition, which is determined by the mouse events that are added to the time interval rectangle. These mouse events allow the user to interact with the time interval on the timeline, enabling them to adjust its start and end times by dragging its edges or moving its body. The mouse events provide a way for the user to manipulate the time interval directly on the timeline, making it easier to use and more intuitive.
        private void MoveTimeIntervalRectangle()
        {
            //TODO this isn't working. There are not changes made to the TimeLine.TimeInterval
            if (_timeIntervalRectangle == null || _timeLineCanvas == null) return;
            TimeInterval interval = TimeLine.TimeInterval;
            Debug.WriteLine($"move interval to ({interval.StartOffset}, {interval.EndOffset})");
            // make sure that the time interval is at least partially dipslayed on the time line 
            if (interval.EndOffset <= 0 || interval.StartOffset >= SizeService.Instance.DisplayWidth.Value) return;
            // redefine the time interval rectangle position and size based on the new start and end offsets, which allows the time interval to be visually updated on the timeline to reflect any changes made by the user through mouse interactions. This ensures that the time interval display is always in sync with its underlying data model, providing a consistent and accurate representation of the time interval on the timeline.
            double left = Math.Max(0, interval.StartOffset);
            double width = Math.Min(SizeService.Instance.DisplayWidth.Value, interval.EndOffset) - left;
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
            // we need to prevent the move if either the start or end offsets go beyond the timeline boundaries, which ensures that the time interval remains visible and interactable within the timeline, and prevents it from being dragged off-screen where it cannot be accessed by the user.
            if (newStartOffset < 0 || newEndOffset > SizeService.Instance.DisplayWidth.Value)
                return;
            TimeInterval interval = TimeLine.TimeInterval;
            interval.StartOffset = newStartOffset;
            interval.EndOffset = newEndOffset;
            OffsetToTime(interval);
            TimeLine.TimeInterval = interval;
            _bodyDragOffset = x;
            Debug.WriteLine($"drag body by {offsetChange} new interval {interval}");
            MoveTimeIntervalRectangle();
            OnPropertyChanged(nameof(TimeLine));
        }
        public void TimeToOffset(TimeInterval interval)
        {
            // convert the timeinterval start and end times to pixel offsets on the timeline, which allows the time interval to be displayed correctly on the timeline. This conversion is based on the current zoom level and the display width of the timeline, ensuring that the time interval is accurately represented in the UI.
            interval.StartOffset = (interval.StartTime - TimeLine.StartTime) / TimeLineScales[TimeLine.CurrentZoomLevel].Extent * SizeService.Instance.DisplayWidth.Value;
            interval.EndOffset = (interval.EndTime - TimeLine.StartTime) / TimeLineScales[TimeLine.CurrentZoomLevel].Extent * SizeService.Instance.DisplayWidth.Value;
            interval.StartOffset = Math.Min(Math.Max(0D, interval.StartOffset), SizeService.Instance.DisplayWidth.Value);
            interval.EndOffset = Math.Min(Math.Max(0D, interval.EndOffset), SizeService.Instance.DisplayWidth.Value);
        }
        #endregion   
    }
}
