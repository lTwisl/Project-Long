#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WeatherWindSystem))]
public class WeatherWindSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        WeatherWindSystem system = (WeatherWindSystem)target;

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