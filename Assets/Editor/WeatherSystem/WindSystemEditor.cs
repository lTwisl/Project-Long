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

        // ������ ��� ���������� ���� ���
        if (GUILayout.Button("������������� ����� ������ ��� � gameObject"))
        {
            system.GenerateZoneNames();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif