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
            TimeLine newModel = timeLine.Clone();
            newModel.ZoomIn();
            vm.TimeToOffset(newModel.TimeInterval);
            vm.NotifyTimeLineChanged(newModel);
            vm.IsDirty = true;
        }
        public void ZoomOutCommand()
        {
            TimeLine newModel = timeLine.Clone();
            newModel.ZoomOut();
            vm.TimeToOffset(newModel.TimeInterval);
            vm.NotifyTimeLineChanged(newModel);
            vm.IsDirty = true;
        }
        public void PanLeftCommand()
        {
            TimeLine newModel = timeLine.Clone();
            newModel.ShiftLeft();
            vm.TimeToOffset(newModel.TimeInterval);
            vm.NotifyTimeLineChanged(newModel);
            vm.IsDirty = true;
        }
        public void PanRightCommand()
        {
            TimeLine newModel = timeLine.Clone();
            newModel.ShiftRight();
            vm.TimeToOffset(newModel.TimeInterval);
            vm.NotifyTimeLineChanged(newModel);
            vm.IsDirty = true;
        }
        #endregion
    }
}
