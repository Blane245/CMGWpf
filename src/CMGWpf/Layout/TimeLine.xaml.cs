using CMGWpf.Services;
using CMGWpf.View;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace CMGWpf.Layout
{
    /// <summary>
    /// Set the data context to the FileViewModel, which provides the necessary data and commands for the timeline functionality. This allows the timeline to display file-related information and respond to user interactions based on the underlying data model.
    /// </summary>
    public partial class TimeLine : UserControl
    {
        public TimeLine()
        {
            InitializeComponent();
            DataContext = TimeLineViewModel.Instance;
        }
        private void TimeLineDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            // When the TimeLineDisplay is loaded, it checks if the DataContext is of type TimeLineViewModel. If it is, it registers the TimeLineCanvas with the view model and calls the DrawTimeLine method to render the timeline based on the current UI model's properties such as StartTime, DisplayWidth, TimeLineHeight, CurrentZoomLevel, and TimeInterval.
            Debug.WriteLine($"TimeLineDisplay Loaded, DataContext {DataContext}");
            if (DataContext is TimeLineViewModel viewModel)
            {
                viewModel.RegisterCanvas(TimeLineCanvas);
                viewModel.DrawTimeLine(TimeLineCanvas, viewModel.TimeLine.StartTime, SizeService.Instance.DisplayWidth, SizeService.Instance.TimeLineHeight, viewModel.TimeLine.CurrentZoomLevel, viewModel.TimeLine.TimeInterval);
            }
        }
    }
}