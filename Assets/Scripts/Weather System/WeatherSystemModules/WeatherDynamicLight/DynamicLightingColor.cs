using System;
using UnityEngine;

/// <summary>
/// Класс предназначенный для управления поведением глобального освещения сцены
/// </summary>
[ExecuteAlways]
public class DynamicLightingColor : MonoBehaviour
{
    [Header("Настройки света:")]
    public bool isSun = true;
    [SerializeField, Tooltip("Максимальная интенсивность источника освещения"), Min(0)] private float _maxIntensity = 1.5f;
    [HideIf(nameof(isSun), false), SerializeField, Tooltip("Температура источника освещения"), Min(0)] private float _temperature = 8000f;
    [Tooltip("Цвет света в полдень")] public Color colorAtZenith = new Color(1f, 0.95f, 0.9f);
    [Tooltip("Цвет света на закате")] public Color colorAtSunset = new Color(1f, 0.65f, 0.3f);

    [SerializeField, Tooltip("Смещение позиции горизонта изменения интенсивности"), Range(-1, 1)] private float _horizontOffset = 0.1f;
    [SerializeField, Tooltip("Диапазон углов для верхнего перехода (от X'верх' до Y'низ')")] private Vector2 _colorTransitionRangeAngles = new(90, 0);

    [HideIf(nameof(isSun), false), SerializeField, Tooltip("Максимальная интенсивность рассеянного освещения"), Range(0f, 2f)] private float _maxAmbientIntensity = 1f;
    [HideIf(nameof(isSun), false), SerializeField, Tooltip("Минимальная интенсивность рассеянного освещения"), Range(0f, 1f)] private float _minAmbientIntensity = 0.3f;

    [Header("Кривые анимации:")]
    [SerializeField] private AnimationCurve _intensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField] private AnimationCurve _colorBlendCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [HideIf(nameof(isSun), false), SerializeField] private AnimationCurve _ambientIntensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    private Light _light;
    private Vector2 _colorTransitionRange;

    public float MaxIntensity
    {
        get => _maxIntensity;
        set => _maxIntensity = Mathf.Clamp(value, 0, float.MaxValue);
    }

    public float Temperature
    {
        get => _temperature;
        set => _temperature = Mathf.Clamp(value, 0, 20000);
    }

    private void Awake()
    {
        InitializeLightComponent();
    }

    /// <summary>
    /// Инициализация компонента света
    /// </summary>
    private void InitializeLightComponent()
    {
        if (_light == null)
        {
            _light = GetComponent<Light>();
            if (_light == null)
            {
                Debug.LogWarning("<color=orange>Источник света не найден!</color>", this);
                enabled = false;
                return;
            }
        }

        // Переводим углы в значения dotProduct
        _colorTransitionRange = new Vector2(
            Mathf.Sin(_colorTransitionRangeAngles.x * Mathf.Deg2Rad),
            Mathf.Sin(_colorTransitionRangeAngles.y * Mathf.Deg2Rad)
        );
    }

    private void OnValidate()
    {
        InitializeLightComponent();
        UpdateLightingParameters();
    }

    private void Update()
    {
        if (transform.hasChanged)
            UpdateLightingParameters();
    }

    /// <summary>
    /// Обновление параметров света
    /// </summary>
    private void UpdateLightingParameters()
    {
        float dotProduct = CalculateDotProduct();
        float normalizedAngle = Mathf.Clamp((dotProduct + _horizontOffset), 0, 1);

        ChangeLightIntensity(normalizedAngle);
        ChangeLightColor(dotProduct);
        if (isSun)
        {
            ChangeAmbientIntensity(normalizedAngle);
        }  
    }

    /// <summary>
    /// Вычисление скалярного произведения для определения положения солнца
    /// </summary>
    /// <returns>Значение скалярного произведения</returns>
    private float CalculateDotProduct()
    {
        return Vector3.Dot(-transform.forward, Vector3.up);
    }

    /// <summary>
    /// Изменение интенсивности света
    /// </summary>
    /// <param name="normalizedAngle">Нормализованный угол</param>
    private void ChangeLightIntensity(float normalizedAngle)
    {
        float curveValue = _intensityCurve.Evaluate(normalizedAngle);
        _light.intensity = Mathf.Lerp(0, _maxIntensity, curveValue);
    }

    /// <summary>
    /// Изменение цвета света
    /// </summary>
    /// <param name="dotProduct">Скалярное произведение</param>
    private void ChangeLightColor(float dotProduct)
    {
        // Нормализуем dotProduct в диапазоне перехода
        float normalizedDot = Mathf.InverseLerp(_colorTransitionRange.x, _colorTransitionRange.y, dotProduct);

        // Используем кривую для плавного перехода
        float colorBlend = _colorBlendCurve.Evaluate(normalizedDot);

        // Интерполируем цвет между зенитом и горизонтом
        _light.color = Color.Lerp(colorAtZenith, colorAtSunset, colorBlend);
    }

    /// <summary>
    /// Изменение интенсивности окружающего света
    /// </summary>
    /// <param name="normalizedAngle">Нормализованный угол</param>
    private void ChangeAmbientIntensity(float normalizedAngle)
    {
        _light.colorTemperature = _temperature;
        float ambientCurveValue = _ambientIntensityCurve.Evaluate(normalizedAngle);
        RenderSettings.ambientIntensity = Mathf.Lerp(_minAmbientIntensity, _maxAmbientIntensity, ambientCurveValue);
    }

    /// <summary>
    /// Обновить параметры системы освещения
    /// </summary>
    /// <param name="currentProfile"></param>
    /// <param name="newProfile"></param>
    /// <param name="t"></param>
    public void UpdateLighting(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        MaxIntensity = Mathf.Lerp(currentProfile.maxIntensitySun, newProfile.maxIntensitySun, t);
        colorAtZenith = Color.Lerp(currentProfile.colorZenithSun, newProfile.colorZenithSun, t);
        colorAtSunset = Color.Lerp(currentProfile.colorSunsetSun, newProfile.colorSunsetSun, t);

        if (isSun)
        {
            Temperature = Mathf.Lerp(currentProfile.temperatureSun, newProfile.temperatureSun, t);
        }
    }
}