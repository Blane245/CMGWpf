using CMGDBEditor.Helpers;
using CMGWpf.Types;
using System.Collections.ObjectModel;

namespace CMGDBEditor.Model
{
    public class NoteSequence
    {
        public string Name { get; set; } = string.Empty;
        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public string Items { get; set; } = string.Empty;
        public NoteSequence Clone() { return new NoteSequence { Name = Name, Tags = new List<Tag>(Tags), Items = Items }; }
        public static ObservableCollection<Message> Validate(NoteSequence noteSequence, string newName, ObservableCollection<NoteSequence> allNoteSequences, string[] newTagList, ObservableCollection<Tag> tagList, ObservableCollection<NoteItem> newNoteItems)
        {
            ObservableCollection<Message> errors = [];
            if (newName.Trim(' ', '\t', ',') == "")
            {
                Messages.Add(errors, "Note sequence name must not be blank or contain a comma.", true);
            }

            // Handle rename
            if (noteSequence.Name != newName)
            {
                int sequenceIndex = -1;
                for (int i = 0; i < allNoteSequences.Count; i++)
                {
                    if (allNoteSequences[i].Name == noteSequence.Name) { sequenceIndex = i; break; }
                }
                // see if there is another note sequence with the same name
                for (int i = 0; i < allNoteSequences.Count; i++)
                {
                    if (i == sequenceIndex) continue;
                    if (newName == allNoteSequences[i].Name)
                    {
                        Messages.Add(errors, $"Note sequence name '{newName}' must be unique", true);
                        break;
                    }
                }
            }

            // check that the tags entered exist in the list of tags in the database
            string[] tags = newTagList.Select(t => t.Trim(' ', '\t', ',')).Where(t => t != "").ToArray();
            foreach (var tag in tags)
            {
                if (!tagList.Any(t => t.Name == tag))
                {
                    Messages.Add(errors, $"Tag '{tag}' does not exist in the database.", true);
                }
            }

            // check that the note items are valid
            foreach (var noteItem in newNoteItems)
            {
                if (!NoteItem.CheckNote(noteItem.Note))
                {
                    Messages.Add(errors, $"Note item '{noteItem.Note}' is not a valid note.", true);
                }
                if (noteItem.Beats <= 0)
                {
                    {
                        Messages.Add(errors, $"Note item '{noteItem.Beats}' must have a positive number of beats.", true);
                    }
                }
            }
            return errors;
        }
    }
}
