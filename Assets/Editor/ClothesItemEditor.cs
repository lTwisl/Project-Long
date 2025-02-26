/*
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ClothesItem), true)]
public class ClothesItemEditor : InventoryItemEditor
{
    protected override void DrawStackingProp()
    {
        GUI.enabled = false;

        base.DrawStackingProp();

        GUI.enabled = true;
    }

    protected override void DrawDegradeProp()
    {
        EditorGUILayout.Space(10);
        
        GUI.enabled = false;
        EditorGUILayout.PropertyField(_degradeTypeProp);
        GUI.enabled = true;
        
        EditorGUI.indentLevel++;
        if (_degradeTypeProp.enumValueIndex == 1)
            EditorGUILayout.PropertyField(_degradationValueProp, new GUIContent("Degradation Used"));
        else if (_degradeTypeProp.enumValueIndex == 2)
            EditorGUILayout.PropertyField(_degradationValueProp, new GUIContent("Degradation Rate"));
        EditorGUI.indentLevel--;
    }
}*/

