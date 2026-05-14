
using CMGWpf.Helpers;
using CMGWpf.Types;
using System.Collections.ObjectModel;

namespace CMGWpf.Model.Database
{
    public class Tag
    {
        public string Name { get; set; } = "";
        public virtual ICollection<NoteSequence> NoteSequences { get; set; } = new List<NoteSequence>();
        public Tag Clone() { return new Tag { Name = Name, NoteSequences = new List<NoteSequence>(NoteSequences) }; }
        public static ObservableCollection<Message> Validate(Tag tag, string newName, ObservableCollection<Tag> allTags)
        {
            ObservableCollection<Message> errors = [];
            if (newName.Trim(' ', '\t') == "" || newName.Contains(','))
            {
                Messages.Add(errors, "Tag name must not be blank or contain commas", true);
            }
            // check that the tag name is unique
            int tagIndex = 0;
            for (int i = 0; i < allTags.Count; i++)
            {
                if (tag.Name == allTags[i].Name)
                {
                    tagIndex = i;
                    break;
                }
            }
            // see if there is another tag with the same name
            for (int i = 0; i < allTags.Count; i++)
            {
                if (i != tagIndex && allTags[i].Name == newName)
                {
                    Messages.Add(errors, "Tag name must be unique", true);
                    break;
                }
            }
            return errors;
        }
    }
}
