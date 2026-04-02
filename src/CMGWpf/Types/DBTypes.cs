using CMGWpf.Model;

namespace CMGWpf.Types
{
    /// <summary>
    /// Represents a container for database-related types and definitions.
    /// </summary>
    public class DBTypes
    {
        public enum DBRESPONSETYPE
        {
            error,
            info,
            notesequencevalidnamelist,
            notesequencevalue,
            ensemblelist,
            ensemble,
            voicelist,
            voice,
        }
        public record struct SequenceName
        {
            public string name { get; set; }
        }
        public record struct DbErrorType
        {
            public DBRESPONSETYPE type { get; set; }
            public string message { get; set; }
        }
        public record struct DbInfoType
        {
            public DBRESPONSETYPE type { get; set; }
            public string message { get; set; }
        }
        public record struct DbNoteSequenceValidNamesType
        {
            public DBRESPONSETYPE type { get; set; }
            public SequenceName[] value { get; set; }
        }
        public record struct SequenceItem
        {
            public string id { get; set; }
            public double value { get; set; }
            public double beats { get; set; }
        }
        public record struct SequenceType
        {
            public string name { get; set; }
            public string tags { get; set; }
            public SequenceItem[] items { get; set; }
        }
        public record struct DbNoteSequenceValueType
        {
            public DBRESPONSETYPE type { get; set; }
            public SequenceType value { get; set; }
        }
        public record struct EnsembleType
        {
            public string name { get; set; }
            public string description { get; set; }
            public string voices { get; set; }
        }
        public record struct DbEnsembleListType
        {
            public DBRESPONSETYPE type { get; set; }
            public EnsembleType[] value { get; set; }
        }
        public record struct DbEnsembleType
        {
            public DBRESPONSETYPE type { get; set; }
            public EnsembleType value { get; set; }
        }
        public record struct DbVoiceListType
        {
            public DBRESPONSETYPE type { get; set; }
            public VoiceType[] value { get; set; }
        }
        public record struct VoiceType
        {
            public string name { get; set; }
            public string description { get; set; }
            public string soundFontFile { get; set; }
            public string presetName { get; set; }
            public string timbre { get; set; }
            public double registerLo { get; set; }
            public double registerHi { get; set; }
            public double duration { get; set; }
        }
        public record struct DbVoiceType
        {
            public DBRESPONSETYPE type { get; set; }
            public VoiceType value { get; set; }
        }
    }
}
