using UnityEngine;

/// <summary>
/// ����� �������� �� �������������� ���������� ���������� ������ � ���������, ����� ��������� �������� ��������� � ��������������
/// </summary>
[ExecuteInEditMode]
public class WeatherSystemEditorUpdater : MonoBehaviour
{
#if UNITY_EDITOR

    [Header("Weather System:")]
    [SerializeField, DisableEdit] private WeatherSystem _weatherSystem;
    [SerializeField] private bool _useWeatherSystemUpdate = false;

    void Update()
    {
        // ������������ Editor-���������� ������ ������
        if (_useWeatherSystemUpdate && _weatherSystem)
        {
            _weatherSystem.SunLight?.UpdateLightingParameters();
            _weatherSystem.MoonLight?.UpdateLightingParameters();
            _weatherSystem.WeatherFogSystem?.UpdateSunDirection();
            _weatherSystem.WeatherSkyboxSystem?.UpdateSunDirection();
        }
    }

    private void OnValidate()
    {
        _weatherSystem = FindAnyObjectByType<WeatherSystem>();
    }

#endif
}