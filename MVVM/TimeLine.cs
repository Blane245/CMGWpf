using CMGWpf.Model;
using CMGWpf.View;

namespace CMGWpf.MVVM
{
    public class TimeLineCommands(TimeLineViewModel vm, TimeLine timeLine )
    {
        private readonly TimeLineViewModel vm = vm;
        private readonly TimeLine timeLine = timeLine;

        #region TimeLine Commands
        public void ZoomInCommand()
        {
            System.Diagnostics.Debug.WriteLine("TimeLine zoom in");
            TimeLine newModel = timeLine.Clone();
            newModel.ZoomIn();
            vm.TimeToOffset(newModel.TimeInterval);
            vm.NotifyTimeLineChanged(newModel);
            vm.IsDirty = true;
        }
        public void ZoomOutCommand()
        {
            System.Diagnostics.Debug.WriteLine("TimeLine zoom out");
            TimeLine newModel = timeLine.Clone();
            newModel.ZoomOut();
            vm.TimeToOffset(newModel.TimeInterval);
            vm.NotifyTimeLineChanged(newModel);
            vm.IsDirty = true;
        }
        public void PanLeftCommand()
        {
            System.Diagnostics.Debug.WriteLine("TimeLine pan left");
            TimeLine newModel = timeLine.Clone();
            newModel.ShiftLeft();
            vm.TimeToOffset(newModel.TimeInterval);
            vm.NotifyTimeLineChanged(newModel);
            vm.IsDirty = true;
        }
        public void PanRightCommand()
        {
            System.Diagnostics.Debug.WriteLine("TimeLine pan right");
            TimeLine newModel = timeLine.Clone();
            newModel.ShiftRight();
            vm.TimeToOffset(newModel.TimeInterval);
            vm.NotifyTimeLineChanged(newModel);
            vm.IsDirty = true;
        }
        #endregion
    }
}
