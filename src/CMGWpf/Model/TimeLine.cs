
using CMGWpf.Types;
using CMGWpf.Utilities;
using System.Globalization;
using System.Xml;

namespace CMGWpf.Model
{
    public class TimeInterval(double start, double end)
    {
        public double StartOffset { get; set; } = start;
        public double EndOffset { get; set; } = end;
        public double StartTime { get; set; } = 0;
        public double EndTime { get; set; } = 0;
        public TimeInterval Clone()
        {
            return new TimeInterval(StartOffset, EndOffset)
            {
                StartTime = StartTime,
                EndTime = EndTime,
            };
        }
    }
    public class TimeLine(double width, double height)
    {
        public double StartTime { get; set; } = 0;
        public int CurrentZoomLevel { get; set; } = TimeLineTypes.TimeLineScales.ToList().FindIndex(scale => scale.Extent == 50);
        public double Width { get; set; } = width;
        public double Height { get; set; } = height;
        public TimeLineTypes.SNAPMODE SnapMode { get; set; } = TimeLineTypes.SNAPMODE.Time;
        public bool Snap { get; set; } = false;
        public double MeasureSize { get; set; } = 0;
        public int BeatsPerMeasure { get; set; } = 0;
        public double SnapIncrement { get; set; } = 1;
        public TimeInterval TimeInterval { get; set; } = new(0, 0);
        public TimeLine Clone()
        {
            TimeLine n = new(Width, Height)
            {
                StartTime = StartTime,
                CurrentZoomLevel = CurrentZoomLevel,
                SnapMode = SnapMode,
                Snap = Snap,
                SnapIncrement = SnapIncrement,
                MeasureSize = MeasureSize,
                BeatsPerMeasure = BeatsPerMeasure,
                TimeInterval = TimeInterval.Clone()
            };

            return n;
        }
        public void ZoomIn()
        {
            if (CurrentZoomLevel > 0) CurrentZoomLevel--;
        }
        public void ZoomOut()
        {
            if (CurrentZoomLevel < TimeLineTypes.TimeLineScales.Count - 1) CurrentZoomLevel++;
        }
        public void ShiftLeft()
        {
            StartTime = Math.Max(0, StartTime - TimeLineTypes.TimeLineScales[CurrentZoomLevel].Extent / 2);
        }
        public void ShiftRight()
        {
            StartTime += TimeLineTypes.TimeLineScales[CurrentZoomLevel].Extent / 2;
        }
        public void AppendXml(XmlDocument doc, XmlElement elem)
        {
            XmlElement timeLineElem = doc.CreateElement("timeLine");
            elem.AppendChild(timeLineElem);
            timeLineElem.SetAttribute("startTime", StartTime.ToString(CultureInfo.InvariantCulture));
            timeLineElem.SetAttribute("currentZoomLevel", CurrentZoomLevel.ToString());
            timeLineElem.SetAttribute("mode", SnapMode.ToString());
            timeLineElem.SetAttribute("snap", Snap.ToString());
            timeLineElem.SetAttribute("snapIncrement", SnapIncrement.ToString(CultureInfo.InvariantCulture));
            timeLineElem.SetAttribute("beatsPerMeasure", BeatsPerMeasure.ToString());
            timeLineElem.SetAttribute("measureSize", MeasureSize.ToString(CultureInfo.InvariantCulture));
            XmlElement timeIntervalElem = doc.CreateElement("timeInterval");
            timeLineElem.AppendChild(timeIntervalElem);
            timeIntervalElem.SetAttribute("startTime", TimeInterval.StartTime.ToString(CultureInfo.InvariantCulture));
            timeIntervalElem.SetAttribute("endTime", TimeInterval.EndTime.ToString(CultureInfo.InvariantCulture));
        }
        public void LoadXml(XmlElement elem)
        {
            StartTime = XMLFunctions.GetAttributeDouble(elem, "startTime", 0);
            CurrentZoomLevel = XMLFunctions.GetAttributeInt(elem, "currentZoomLevel", TimeLineTypes.TimeLineScales.ToList().FindIndex(scale => scale.Extent == 50));
            SnapMode = Enum.Parse<TimeLineTypes.SNAPMODE>(XMLFunctions.GetAttributeString(elem, "mode", TimeLineTypes.SNAPMODE.Time.ToString()));
            Snap = XMLFunctions.GetAttributeBool(elem, "snap", false);
            SnapIncrement = XMLFunctions.GetAttributeInt(elem, "snapIncrement", 0);
            BeatsPerMeasure = XMLFunctions.GetAttributeInt(elem, "beatsPerMeasure", 0);
            MeasureSize = XMLFunctions.GetAttributeDouble(elem, "measureSize", 0);
            XmlElement? timeIntervalElem = elem.GetElementsByTagName("timeInterval").Cast<XmlElement?>().FirstOrDefault();
            if (timeIntervalElem == null) TimeInterval = new(0, 0);
            else
            {
                double startTime = XMLFunctions.GetAttributeDouble(timeIntervalElem, "startTime", 0);
                double endTime = XMLFunctions.GetAttributeDouble(timeIntervalElem, "endTime", 0);
                TimeInterval = new(0, 0) { StartTime = startTime, EndTime = endTime};
                
            }
        }
    }
}
