#if UNITY_2020_1_OR_NEWER

using UnityEditor.Experimental.GraphView;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    public class NoteView : StickyNote
    {
        public Note Note { get; }

        public NoteView(Note note)
        {
            Note = note;
        }
    }
}

#endif