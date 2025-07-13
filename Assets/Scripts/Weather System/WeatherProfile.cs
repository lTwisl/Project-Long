using System;
using System.Collections.Generic;
using UnityEngine;

// Чем больше значков, тем серьезней эффект от погоды:
// ☀️ - комфортная погода
// ❆ - некомфортная погода
// ☣ - опасная погода

public enum WeatherIdentifires
{
    Sunny = 1,                  // ☀️☀️☀️ - Солнечно
    PartlyCloudy = 2,           // ☀️☀️ - Частичная облачность
    Overcast = 4,               // ☀️ - Сильно пасмурно
    Foggy = 8,                  // ☀️☀️ - Туманно
    Rainу = 16,                 // ☀️ - Дождливо
    Sleet = 32,                 // ❆ - Слякоть = снег + дождь
    Shower = 64,                // ❆ - Ливень
    SunnySnow = 128,            // ❆ - Легкий снег с солнцем
    OvercastSnow = 256,         // ❆ - Легкий снег пасмурно
    Snowfall = 512,             // ❆❆ - Снегопад
    SnowStorm = 1024,           // ❆❆❆ - Снежная буря с молниями
    AluminumRain = 2048,        // ☣ - Аэрозольный смог (добавляет зараженность)
    MetallWind = 4096,          // ☣ - Металлический ветер (добавляет зараженность + снимает здоровье)
    AerosolSmog = 8192,         // ☣☣ - Аллюминевый дождь (добавляет зараженность)
    ToxicIceStorm = 16384       // ☣☣☣ - Ледяной шторм Хлад-9 (добавляет зараженность + снимает здоровье)
}

[Flags]
public enum WeatherTransitions
{
    Sunny = 1,
    PartlyCloudy = 2,
    Overcast = 4,
    Foggy = 8,
    Rainу = 16,
    Sleet = 32,
    Shower = 64,
    SunnySnow = 128,
    OvercastSnow = 256,
    Snowfall = 512,
    SnowStorm = 1024,
    AluminumRain = 2048,
    MetallWind = 4096,
    AerosolSmog = 8192,
    ToxicIceStorm = 16384
}


[CreateAssetMenu(fileName = "WeatherProfile_", menuName = "Scriptable Objects/Weather Profile File")]
public class WeatherProfile : ScriptableObject
{
    [field: Header("- - Base Info:")]
    [field: SerializeField] public WeatherIdentifires Identifier { get; private set; } = WeatherIdentifires.Sunny;
    [field: SerializeField] public WeatherTransitions Transitions { get; private set; }
    [field: SerializeField, Range(1f, 23f)] public int MinLifetime { get; private set; } = 1;
    [field: SerializeField, Range(1f, 23f)] public int MaxLifetime { get; private set; } = 23;
    [field: SerializeField] public List<GameObject> VFX { get; private set; } = new ();


    [field: Header("- - Weather Params:")]
    [field: SerializeField, Range(-25f, 25f)] public float Temperature { get; private set; } = 0;
    [field: SerializeField, Range(0f, 1f)] public float Wetness { get; private set; } = 0;
    [field: SerializeField, Range(0f, 250f)] public float Toxicity { get; private set; } = 0;


    [field: Header("- - Light Params:")]
    [field: SerializeField, Range(0f, 5f)] public float SunMaxIntensity { get; private set; } = 2f;
    [field: SerializeField, Range(3000f, 15000f)] public float SunTemperature { get; private set; } = 8000f;
    [field: SerializeField] public Color SunZenithColor { get; private set; } = new Color(1f, 1f, 1f, 1f);
    [field: SerializeField] public Color SunSunsetColor { get; private set; } = new Color(1f, 0.5f, 0f, 1f);
    [field: Space(8)]
    [field: SerializeField, Range(0f, 2f)] public float MoonMaxIntensity { get; private set; } = 0.1f;
    [field: SerializeField] public Color MoonZenithColor { get; private set; } = new Color(0.84f, 0.85f, 0.86f);
    [field: SerializeField] public Color MoonSunsetColor { get; private set; } = new Color(0.7f, 0.76f, 0.8f, 1f);


    [field: Header("- - Wind Params:")]
    [field: SerializeField, Range(0f, 33f)] public float MinWindSpeed { get; private set; } = 0f;
    [field: SerializeField, Range(0f, 33f)] public float MaxWindSpeed { get; private set; } = 33f;
    [field: SerializeField, Range(0.01f, 10f)] public float IntensityChangeSpeed { get; private set; } = 1f;
    [field: Space(8)]
    [field: SerializeField, Range(0.001f, 2f)] public float DirectionChangeSharpness { get; private set; } = 0.05f;
    [field: SerializeField, Range(0.01f, 5f)] public float IntensityChangeDirection { get; private set; } = 1f;


    [field: Header("- - Volumetric Fog:")]
    [field: SerializeField] public Material AtmosphericFogMat { get; private set; }
    [field: SerializeField] public Material VolumetricFogMat { get; private set; }


    [field: Header("- - Skybox:")]
    [field: SerializeField] public Material SkyboxMat { get; private set; }


    [field: Header("- - Post Processing (Color Adjustments):")]
    [field: SerializeField, Range(-2f, 2f)] public float ColAdjPostExposure { get; private set; } = 0f;
    [field: SerializeField, Range(-100f, 100f)] public float ColAdjContrast { get; private set; } = 0f;
    [field: SerializeField] public Color ColAdjColorFilter { get; private set; } = Color.white;
    [field: SerializeField, Range(-100f, 100f)] public float ColAdjSaturation { get; private set; } = 0f;


    [field: Header("- - Post Processing (Bloom):")]
    [field: SerializeField, Range(0f, 1.5f)] public float BloomThreshold { get; private set; } = 1.2f;
    [field: SerializeField, Range(0f, 15f)] public float BloomIntensity { get; private set; } = 1f;
    [field: SerializeField, Range(0f, 1f)] public float BloomScatter { get; private set; } = 0.7f;
    [field: SerializeField] public Color BloomTint { get; private set; } = Color.white;
}