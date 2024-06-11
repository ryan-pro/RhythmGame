using UnityEditor;
using UnityEngine;

namespace RhythmGame.SongModels
{
    [CustomEditor(typeof(NotesMap))]
    public class NotesMapEditor : Editor
    {
        const string sortNotesLabel = "Sort Notes";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button(sortNotesLabel, GUILayout.Height(40f)))
            {
                var notesMap = (NotesMap)target;
                Undo.RecordObject(notesMap, sortNotesLabel);

                notesMap.SortListByPosition();
                EditorUtility.SetDirty(notesMap);
            }
        }
    }
}
