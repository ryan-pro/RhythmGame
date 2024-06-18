using RhythmGame;
using UnityEditor;
using UnityEngine;

namespace RhythmGameEditor
{
    [CustomPropertyDrawer(typeof(SerializedKeyValue<,>))]
    public class SerializedKeyValuePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var keyRect = new Rect(position.x, position.y, (position.width * 0.5f) - (10f / 2f), position.height);
            EditorGUI.PropertyField(keyRect, property.FindPropertyRelative("Key"), GUIContent.none);

            var valueRect = new Rect(position.x + (position.width * 0.5f) + (10f / 2f), position.y, (position.width * 0.5f) - (10f / 2f), position.height);
            EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("Value"), GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}
