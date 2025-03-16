using System;
using UnityEngine;

// Коррелированные Enums идентификатора погоды и переходов
public enum WeatherIdentifires
{
    Bright = 1,
    SnowStorm = 2
}

[Flags]
public enum WeatherTransitions
{
    Bright = 1,
    SnowStorm = 2
}

[CreateAssetMenu(fileName = "WeatherProfile_", menuName = "Scriptable Objects/Weather Profile")]
public class WeatherProfile : ScriptableObject
{
    [Header("Идентификатор пресета и основные параметры:")]
    [Tooltip("Идентификатор текущего профиля")] public WeatherIdentifires weatherIdentifier;
    [Tooltip("Доступные данной погоде переходы")] public WeatherTransitions weatherTransitions;
    [Range(1f, 23f)] public int minLifetimeHours;
    [Range(1f, 23f)] public int maxLifetimeHours;

    [Header("Параметры влияния на персонажа:")]
    [Range(-25f, 25f)] public float temperature;
    [Range(0f, 25f)] public float wetness;
    [Range(0f, 25f)] public float toxicity;

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
    public Material materialNearVolumFog;
    public Material materialFarVolumFog;

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