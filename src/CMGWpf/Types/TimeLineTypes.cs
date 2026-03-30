using System.Collections.ObjectModel;

namespace CMGWpf.Types
{
    public class TimeLineTypes
    {
        public enum SNAPMODE
        {
            Time,
            Measures,
        }
        public enum TIMEFORMATTYPE
        {
            Number,
            Time,
        }

        public readonly struct TimeFormat(string value, TIMEFORMATTYPE type)
        {
            private readonly string value = value;
            private readonly TIMEFORMATTYPE type = type;

            public string Value { get { return value; } }
            public TIMEFORMATTYPE Type { get { return type; } }
        }

        public static readonly IList<TimeFormat> TimeFormats = new ReadOnlyCollection<TimeFormat>(new[]
                {
                new TimeFormat("0.000000",TIMEFORMATTYPE.Number),
                new TimeFormat("0.0000",TIMEFORMATTYPE.Number),
                new TimeFormat("0.000",TIMEFORMATTYPE.Number),
                new TimeFormat("0.00",TIMEFORMATTYPE.Number),
                new TimeFormat("0.0",TIMEFORMATTYPE.Number),
                new TimeFormat("00.0",TIMEFORMATTYPE.Number),
                new TimeFormat("0:00",TIMEFORMATTYPE.Time),
                new TimeFormat("00:00",TIMEFORMATTYPE.Time),
                new TimeFormat("0:00:00",TIMEFORMATTYPE.Time),
                new TimeFormat("0:00:00",TIMEFORMATTYPE.Time),
                new TimeFormat("000:00:00",TIMEFORMATTYPE.Time),
            }
        );

        public readonly struct TimeLineScale(double extent, int majorDivisions, int minorDivisions, int format)
        {
            private readonly double extent = extent;
            private readonly int majorDivisions = majorDivisions;
            private readonly int minorDivisions = minorDivisions;
            private readonly int format = format;
            public double Extent { get { return extent; } }
            public int MajorDivisions { get { return majorDivisions; } }
            public int MinorDivisions { get { return minorDivisions; } }
            public int Format { get { return format; } }
            public readonly bool Equals(TimeLineScale other)
            {
                return
                    extent == other.extent &&
                    majorDivisions == other.majorDivisions &&
                    minorDivisions == other.minorDivisions &&
                    format == other.format;
            }
        }

        public static readonly IList<TimeLineScale> TimeLineScales = new ReadOnlyCollection<TimeLineScale>(new[]
        {
            new TimeLineScale(0.00002, 10, 4, 0),
            new TimeLineScale(0.00004, 8, 5, 0),
            new TimeLineScale(0.00008, 8, 2, 0),
            new TimeLineScale(0.00016, 16, 2, 0),
            new TimeLineScale(0.003, 6, 5, 1),
            new TimeLineScale(0.006, 6, 2, 1),
            new TimeLineScale(0.013, 13, 2, 1),
            new TimeLineScale(0.025, 25, 2, 1),
            new TimeLineScale(0.05, 10, 5, 2),
            new TimeLineScale(0.1, 10, 2, 2),
            new TimeLineScale(0.21, 21, 2, 2),
            new TimeLineScale(0.4, 8, 5, 2),
            new TimeLineScale(0.8, 8, 2, 2),
            new TimeLineScale(1.7, 17, 2, 3),
            new TimeLineScale(3, 6, 5, 4),
            new TimeLineScale(6, 6, 2, 4),
            new TimeLineScale(13, 13, 2, 5),
            new TimeLineScale(27, 27, 2, 5),
            new TimeLineScale(50, 10, 2, 5),
            new TimeLineScale(105, 7, 3, 6),
            new TimeLineScale(210, 14, 3, 6),
            new TimeLineScale(420, 14, 3, 6),
            new TimeLineScale(840, 14, 3, 7),
            new TimeLineScale(1800, 6, 5, 7),
            new TimeLineScale(3600, 4, 3, 8),
            new TimeLineScale(7200, 8, 3, 8),
            new TimeLineScale(14400, 16, 3, 8),
            new TimeLineScale(28800, 16, 3, 8),
            new TimeLineScale(54000, 15, 2, 9),
            new TimeLineScale(108000, 5, 5, 9),
            new TimeLineScale(216000, 3, 4, 9),
            new TimeLineScale(413000, 5, 4, 10),
            new TimeLineScale(604000, 7, 4, 10),
            new TimeLineScale(1209600, 2, 7, 10),
        });
        public struct TimeTicks
        {
            public int majorTickCount = 0;
            public double scaleExtent = 0;
            public int tickCount = 0;
            public double tickHeight = 0;
            public double tickSpacing = 0;
            public double labelSize = 0;
            public double labelSpacing = 0;
            public string labelFormat = "";

            public TimeTicks()
            {
            }
            public readonly bool Equals(TimeTicks other)
            {
                return
                    majorTickCount == other.majorTickCount &&
                    scaleExtent == other.scaleExtent &&
                    tickCount == other.tickCount &&
                    tickHeight == other.tickHeight &&
                    tickSpacing == other.tickSpacing &&
                    labelSize == other.labelSize &&
                    labelSpacing == other.labelSpacing &&
                    labelFormat == other.labelFormat;
            }
        }

    }
}
