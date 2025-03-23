using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ShelterSystem))]
public class ShelterSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var shelter = (ShelterSystem)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Обновить список входов/выходов", GUILayout.Height(30)))
        {
            Undo.RegisterCompleteObjectUndo(shelter, "Update All Entrances");
            shelter.FindAndConfigureEntrances();
        }
    }
}