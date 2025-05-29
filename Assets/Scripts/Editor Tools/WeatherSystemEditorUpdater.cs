using UnityEngine;

/// <summary>
/// Класс отвечает за автоматическое обновление параметров погоды в редакторе, чтобы увеличить удобство настройки и редактирования
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
        // Актуализация Editor-параметров систем погоды
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