using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DisableEditAttribute))]
public class DisableEditDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool prev = GUI.enabled;

        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = prev;
    }
}
