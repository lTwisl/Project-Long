using System;
using UnityEngine;

[ExecuteAlways]
public class DynamicLightingColor : MonoBehaviour
{
    [Header("Настройки света:")]
    public bool isSun = true;
    [SerializeField, Tooltip("Максимальная интенсивность источника освещения"), Min(0)] private float _maxIntensity = 1.5f;
    [SerializeField, Tooltip("Температура источника освещения"), Min(0)] private float _temperature = 8000f;
    [Tooltip("Цвет света в полдень")] public Color colorAtZenith = new Color(1f, 0.95f, 0.9f);
    [Tooltip("Цвет света на закате")] public Color colorAtSunset = new Color(1f, 0.65f, 0.3f);

    [SerializeField, Tooltip("Смещение позиции горизонта изменения интенсивности"), Range(-1, 1)] private float _horizontOffset = 0.1f;
    [SerializeField, Tooltip("Диапазон углов для верхнего перехода (от X'верх' до Y'низ')")] private Vector2 _colorTransitionRangeAngles = new(90, 0);
    [SerializeField, Tooltip("Порог обновления параметров света"), Min(0)] private float _updateThreshold = 0.001f;

    [Header("Настройки окружающего света:")]
    [SerializeField, Tooltip("Максимальная интенсивность рассеянного освещения"), Range(0f, 2f)] private float _maxAmbientIntensity = 1f;
    [SerializeField, Tooltip("Минимальная интенсивность рассеянного освещения"), Range(0f, 1f)] private float _minAmbientIntensity = 0.3f;


    [Header("Кривые анимации:")]
    [SerializeField] private AnimationCurve _intensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)); // Кривая интенсивности света
    [SerializeField] private AnimationCurve _colorBlendCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)); // Кривая смешения цветов
    [SerializeField] private AnimationCurve _ambientIntensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)); // Кривая интенсивности окружающего света

    private Light _light;
    private float _lastDotProduct = float.MaxValue;
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
        if (ShouldUpdateLighting())
        {
            UpdateLightingParameters();
            CacheCurrentState();
            try
            {
                FindFirstObjectByType<WeatherSystem>().UserVolumFogMaterial.SetVector("_Sun_Direction", RenderSettings.sun.transform.forward);
                RenderSettings.skybox.SetVector("_Sun_Direction", RenderSettings.sun.transform.forward);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
    }

    /// <summary>
    /// Проверка необходимости обновления параметров света
    /// </summary>
    /// <returns>True, если требуется обновление</returns>
    private bool ShouldUpdateLighting()
    {
        return transform.hasChanged || Mathf.Abs(CalculateDotProduct() - _lastDotProduct) > _updateThreshold;
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
            ChangeAmbientIntensity(normalizedAngle);
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
    /// Кеширование текущего состояния для оптимизации
    /// </summary>
    private void CacheCurrentState()
    {
        _lastDotProduct = CalculateDotProduct();
        transform.hasChanged = false;
    }
}