namespace CMGWpf.Types
{
    /// <summary>
    /// Represents a container for database-related types and definitions.
    /// </summary>
    public class DBTypes
    {
        public record struct SequenceItem
        {
            public double Value { get; set; }
            public double Beats { get; set; }
        }
    }
}
