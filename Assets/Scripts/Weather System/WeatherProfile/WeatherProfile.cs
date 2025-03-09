using System;
using UnityEngine;

// --> Параметр интерполируется
// --X Параметр не интерполируется

[CreateAssetMenu(fileName = "WeatherProfile_X", menuName = "Scriptable Objects/WeatherProfile")]
public class WeatherProfile : ScriptableObject
{
    [Header("Параметры пресета:")]
    [Tooltip("Идентификатор текущего профиля")] public Weathers weatherIdentifier;
    [Tooltip("Доступные данной погоде переходы")] public Weathers weatherTransitions;
    [Range(1f, 23f)] public int minLifetimeHours;
    [Range(1f, 23f)] public int maxLifetimeHours;

    [Header("Параметры влияния:")]
    public float tempRatio;
    public float toxicityRatio;

    [Header("Параметры освещения:")]
    [Range(0f, 5f)] public float maxIntensitySun;
    [Range(0f, 15000f)] public float temperatureSun;
    public Color colorZenithSun;
    public Color colorSunsetSun;
    [Space(8)]
    [Range(0f, 2f)] public float maxIntensityMoon;
    public Color colorZenithMoon;
    public Color colorSunsetMoon;

    [Header("Параметры ветра:")]
    [Range(1f, 33f)] public float minSpeedWind;
    [Range(1f, 33f)] public float maxSpeedWind;
    [Range(0.01f, 5f)] public float speedChangeWind;
    [Range(0.001f, 2f)] public float directionNoiseScaleWind;

    [Header("Параметры VolumetricFog:")]
    public Material materialVolumFog;
    public Material materialVolumFogFar;

    [Header("Параметры Skybox:")]
    public Material materialSkybox;

    [Header("Параметры PostProcessing:")]
    // Color Adjustments
    [Range(0f, 1f)] public float postExposure;
    [Range(0f, 1f)] public float constrast;
    [Range(0f, 1f)] public float saturation;

    [Header("Параметры визуальных эффектов")]
    public GameObject[] VFX;
}