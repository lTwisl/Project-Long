#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class EditorPool : MonoBehaviour
{
    [Header("Weather System:")]
    [SerializeField] private bool _useWeatherSystemUpdate = false;
    [SerializeField] private WeatherSystem _weatherSystem;

    private void Awake()
    {
        FindReferences();
    }

    void Update()
    {
        if (_useWeatherSystemUpdate)
        {
            _weatherSystem?.SunLight?.UpdateLightingParameters();
            _weatherSystem?.MoonLight?.UpdateLightingParameters();
            _weatherSystem?.WeatherFogSystem?.UpdateSunDirection();
            _weatherSystem?.WeatherSkyboxSystem?.UpdateSunDirection();
        }
    }

    public void FindReferences()
    {
        Undo.RecordObject(this, "Поиск ссылок");
        EditorUtility.SetDirty(this);
        _weatherSystem = FindAnyObjectByType<WeatherSystem>();
    }
}
#endif