using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class WeatherSystemEditorUpdater : MonoBehaviour
{
    [SerializeField] private bool _useModulesUpdate = false;
    [SerializeField] private WeatherSystem _weatherSystem;

    void Update()
    {
        // ���������� ���������� �������� ������ � Editor
        if (_useModulesUpdate && _weatherSystem)
        {
            // 1. ��������� ��������� ����� �� ����������� ������:
            _weatherSystem.SunLight?.UpdateLightingParameters();
            _weatherSystem.MoonLight?.UpdateLightingParameters();

            // 2. ��������� ���������� ��������� ����� �� ����������� ������:
            //_weatherSystem.SunLight?.UpdateEnviromentLight();

            // 3. ��������� ����������� ������ ��� ����������:
            _weatherSystem.WeatherFogSystem?.UpdateMaterialsSunDirection();
            _weatherSystem.WeatherSkyboxSystem?.UpdateMaterialsSunDirection();
        }
    }

#if UNITY_EDITOR
    public void FindReferences()
    {
        if (_weatherSystem) return;

        _weatherSystem = FindAnyObjectByType<WeatherSystem>();
    }

    private void OnValidate()
    {
        // 0. �� ����������, ���� ��� ������-����� (�� ���������)
        if (PrefabUtility.IsPartOfPrefabAsset(this)) return;

        // 1. ������������� �������������� ������� � ���������
        FindReferences();

        // 2. ��������� �������� ��� �������
        if (PrefabUtility.IsPartOfPrefabInstance(this))
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }
#endif
}