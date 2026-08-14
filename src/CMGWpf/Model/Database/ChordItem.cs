using CMGWpf.Types;

namespace CMGWpf.Model.Database
{
    /// <summary>
    /// Represents a single chord. A chord is in standard chord format (i, I, etc., rest, with other attributes).
    /// </summary>
    public class ChordItem
    {
        public ChordItem()
        {
        }
        private string chordValue = ""; // I, i, etc.
        public string ChordValue { get => chordValue; set { if (chordValue != value) { chordValue = value; } } }
        private int inversion = 0;
        public int Inversion { get => inversion; set { if (inversion != value) { inversion = value; } } }
        private double duration = 1.0; // beats
        public double Duration { get => duration; set { if (duration != value) { duration = value; } } }
        private Music.EffortType effort = new Music.EffortType();
        public Music.EffortType Effort { get => effort; set { if (effort != value) { effort = value; } } }
    }


}
