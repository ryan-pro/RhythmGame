using UnityEngine;
using UnityEditor;

namespace RhythmGame.Songs
{
    [CustomPropertyDrawer(typeof(NoteData))]
    public class NotePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), GUIContent.none);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate label width dynamically based on the content. Adjust the multiplier as necessary for aesthetics.
            float quarterWidth = position.width * 0.25f;

            // Define rects for label and field separately
            var trackIndexLabelRect = new Rect(position.x, position.y, quarterWidth, position.height);
            var trackIndexFieldRect = new Rect(position.x + quarterWidth, position.y, quarterWidth * 0.95f, position.height);

            var beatPositionLabelRect = new Rect(position.x + quarterWidth * 2 * 1.05f, position.y, quarterWidth, position.height);
            var beatPositionFieldRect = new Rect(position.x + (quarterWidth * 3), position.y, quarterWidth, position.height);

            // Draw the labels
            EditorGUI.LabelField(trackIndexLabelRect, "Track");
            EditorGUI.LabelField(beatPositionLabelRect, "Beat Pos");

            // Draw the fields without labels, as we've manually drawn them
            EditorGUI.PropertyField(trackIndexFieldRect, property.FindPropertyRelative("trackIndex"), GUIContent.none);
            EditorGUI.PropertyField(beatPositionFieldRect, property.FindPropertyRelative("beatPosition"), GUIContent.none);

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}
