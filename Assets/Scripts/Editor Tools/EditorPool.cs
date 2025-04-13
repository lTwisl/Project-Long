#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using EditorAttributes;

[ExecuteInEditMode]
public class EditorPool : MonoBehaviour
{
    [Header("Weather System:")]
    [SerializeField] private bool _useWeatherSystemUpdate = false;
    [SerializeField] private WeatherSystem _weatherSystem;

    void Update()
    {
        // Обновление параметров погодных систем в Editor
        if (_useWeatherSystemUpdate)
        {
            _weatherSystem?.SunLight?.UpdateLightingParameters();
            _weatherSystem?.MoonLight?.UpdateLightingParameters();
            _weatherSystem?.WeatherFogSystem?.UpdateSunDirection();
            _weatherSystem?.WeatherSkyboxSystem?.UpdateSunDirection();
        }
    }

    [Button("Найти ссылки в сцене", buttonHeight: 30)]
    public void FindReferences()
    {
        //Undo.RecordObject(this, "Find References");
        EditorUtility.SetDirty(this);

        _weatherSystem = FindAnyObjectByType<WeatherSystem>();
    }
}
#endif