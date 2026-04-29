using CMGWpf.Services;

namespace CMGDBEditor.View
{
    public class NoteSequencesView
    {
        private static NoteSequencesView? _instance;
        public static NoteSequencesView Instance => _instance ??= new NoteSequencesView();

        private NoteSequencesView()
        {
            GlobalService.Instance.PropertyChanged += (s, e) =>
            {
            };
        }
    }
}
