#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WindSystem))]
public class WindSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        WindSystem system = (WindSystem)target;

        GUILayout.Space(10);

        //  нопка дл€ обновлени€ имен зон
        if (GUILayout.Button("—генерировать имена пустых зон и gameObject"))
        {
            system.GenerateZoneNames();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif