using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RhythmGame
{
    [CustomPropertyDrawer(typeof(AssetReferenceScene))]
    public class AssetReferenceSceneDrawer : PropertyDrawer
    {
        AssetReferenceScene sceneReference;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sceneRefProperty = property.FindPropertyRelative("sceneReference");
            EditorGUI.BeginProperty(position, label, property);

            var referenceRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(referenceRect, sceneRefProperty, label);

            if(sceneReference == null)
            {
                var target = property.serializedObject.targetObject;
                FieldInfo fieldInfo = target.GetType().GetField(property.propertyPath, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if(fieldInfo != null)
                    sceneReference = (AssetReferenceScene)fieldInfo.GetValue(target);
            }

            if (sceneReference?.CheckReferenceValueChanged() == true)
                sceneReference.UpdateReferenceValue();
        }
    }
}
