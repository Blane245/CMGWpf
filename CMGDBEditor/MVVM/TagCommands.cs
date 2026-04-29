using CMGDBEditor.Model;
using CMGDBEditor.View;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMGDBEditor.MVVM
{
    public class TagCommands(NoteSequencesView vm, Tag tag)
    {
        private readonly NoteSequencesView vm = vm;
        private Tag tag = tag;
    }
}
