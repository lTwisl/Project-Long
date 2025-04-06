using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EditorPool))]
public class EditorPoolEditor : Editor
{
    private EditorPool _editorPool;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        _editorPool = (EditorPool)target;

        if (GUILayout.Button("Найти ссылки"))
        {
            _editorPool.FindReferences();
        }
    }
}