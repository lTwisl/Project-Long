using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(OffsetFieldAttribute))]
public class OffsetFieldDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        int prev = EditorGUI.indentLevel;

        EditorGUI.indentLevel = ((OffsetFieldAttribute)attribute).Value;
        EditorGUI.PropertyField(position, property, label);
        EditorGUI.indentLevel = prev;
    }
}
