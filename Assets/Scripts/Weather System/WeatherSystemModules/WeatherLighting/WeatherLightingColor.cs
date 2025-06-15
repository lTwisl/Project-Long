using System;
using UnityEditor;
using UnityEngine;

public class WeatherLightingColor : MonoBehaviour, IWeatherSystem
{
    [SerializeField, DisableEdit] private bool _isSystemValid;
    public bool IsSystemValid => _isSystemValid;

    [field: Header("- - Настройки источника света:")]
    [field: SerializeField] public bool IsSun { get; private set; } = true;

    [SerializeField, Tooltip("Цвет света в полдень")] private Color _colorAtZenith = new(1f, 0.95f, 0.9f);
    [SerializeField, Tooltip("Цвет света на закате")] private Color _colorAtSunset = new(1f, 0.5f, 0f);

    [SerializeField, Min(0)] private float _maxIntensity = 3f;
    [HideIf(nameof(IsSun), false)]
    [SerializeField, Min(0)] private float _temperature = 8000f;

    [Tooltip("Максимальная интенсивность рассеянного освещения"), HideIf(nameof(IsSun), false)]
    [SerializeField, Range(0f, 2f)] private float _maxAmbientIntensity = 1f;
    [Tooltip("Минимальная интенсивность рассеянного освещения"), HideIf(nameof(IsSun), false)]
    [SerializeField, Range(0f, 1f)] private float _minAmbientIntensity = 0.5f;

    [Tooltip("Смещение позиции горизонта изменения интенсивности")]
    [SerializeField, Range(-1, 1)] private float _horizontOffset = 0.1f;
    [Tooltip("Диапазон углов для верхнего перехода (от X'верх' до Y'низ')")]
    [SerializeField] private Vector2 _colorTransitionRangeAngles = new(90, 0);


    [Header("- - Анимационные кривые перехода:")]
    [SerializeField] private AnimationCurve _intensityCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField] private AnimationCurve _colorBlendCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
    [HideIf(nameof(IsSun), false)]
    [SerializeField] private AnimationCurve _ambientIntensityCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));

    [SerializeField, DisableEdit] private Light _light;
    public Light GetLight => _light;
    private Vector2 _colorTransitionRange;

    public float MaxIntensity
    {
        get => _maxIntensity;
        set => _maxIntensity = Mathf.Clamp(value, 0, 5);
    }

    public float Temperature
    {
        get => _temperature;
        set => _temperature = Mathf.Clamp(value, 0, 20000);
    }

    private void Awake()
    {
        GameTime.OnTimeChanged += UpdateLightingParameters;
    }

    public void InitializeAndValidateSystem()
    {
        _light ??= GetComponent<Light>();
        if (!_light)
        {
            _isSystemValid = false;
            return;
        }

        // Переводим углы в значения dotProduct
        _colorTransitionRange = new Vector2(
            Mathf.Sin(_colorTransitionRangeAngles.x * Mathf.Deg2Rad),
            Mathf.Sin(_colorTransitionRangeAngles.y * Mathf.Deg2Rad)
        );

        _isSystemValid = true;
    }

    private void Update()
    {
        UpdateLightingParameters();
    }

    /// <summary>
    /// Рассчет и изменение параметров света из параметров скрипта
    /// </summary>
    public void UpdateLightingParameters()
    {
        if (!IsSystemValid) return;

        float dotProduct = CalculateDotProduct();
        float normalizedAngle = Mathf.Clamp((dotProduct + _horizontOffset), 0, 1);

        SetLightIntensity(normalizedAngle);
        SetLightColor(dotProduct);

        if (IsSun) SetAmbientIntensity(normalizedAngle);
    }

    /// <summary>
    /// Вычисление скалярного произведения для определения направления света
    /// </summary>
    private float CalculateDotProduct()
    {
        return Vector3.Dot(-transform.forward, Vector3.up);
    }

    /// <summary>
    /// Изменение интенсивности света
    /// </summary>
    /// <param name="normalizedAngle">Нормализованный угол направления источника света</param>
    private void SetLightIntensity(float normalizedAngle)
    {
        float curveValue = _intensityCurve.Evaluate(normalizedAngle);
        _light.intensity = Mathf.Lerp(0, _maxIntensity, curveValue);
    }

    /// <summary>
    /// Изменение цвета света
    /// </summary>
    /// <param name="dotProduct">Скалярное произведение направления света</param>
    private void SetLightColor(float dotProduct)
    {
        // Нормализуем dotProduct в диапазоне перехода
        float normalizedDot = Mathf.InverseLerp(_colorTransitionRange.x, _colorTransitionRange.y, dotProduct);

        // Используем кривую для плавного перехода
        float colorBlend = _colorBlendCurve.Evaluate(normalizedDot);

        // Интерполируем цвет между зенитом и горизонтом
        _light.color = Color.Lerp(_colorAtZenith, _colorAtSunset, colorBlend);
    }

    /// <summary>
    /// Изменение интенсивности рассеянного освещения
    /// </summary>
    /// <param name="normalizedAngle">Нормализованный угол направления источника света</param>
    private void SetAmbientIntensity(float normalizedAngle)
    {
        _light.colorTemperature = _temperature;
        float ambientCurveValue = _ambientIntensityCurve.Evaluate(normalizedAngle);
        RenderSettings.ambientIntensity = Mathf.Lerp(_minAmbientIntensity, _maxAmbientIntensity, ambientCurveValue);
    }

    /// <summary>
    /// Обновить параметры системы освещения
    /// </summary>
    public void UpdateSystemParameters(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        if (IsSun)
        {
            MaxIntensity = Mathf.Lerp(currentProfile.SunMaxIntensity, newProfile.SunMaxIntensity, t);
            _colorAtZenith = Color.Lerp(currentProfile.SunZenithColor, newProfile.SunZenithColor, t);
            _colorAtSunset = Color.Lerp(currentProfile.SunSunsetColor, newProfile.SunSunsetColor, t);
            Temperature = Mathf.Lerp(currentProfile.SunTemperature, newProfile.SunTemperature, t);
        }
        else
        {
            MaxIntensity = Mathf.Lerp(currentProfile.MoonMaxIntensity, newProfile.MoonMaxIntensity, t);
            _colorAtZenith = Color.Lerp(currentProfile.MoonZenithColor, newProfile.MoonZenithColor, t);
            _colorAtSunset = Color.Lerp(currentProfile.MoonSunsetColor, newProfile.MoonSunsetColor, t);
        }
    }

    private void OnDestroy()
    {
        GameTime.OnTimeChanged -= UpdateLightingParameters;
    }

#if UNITY_EDITOR
    private Quaternion _oldRotation;

    private void OnValidate()
    {
        // 0. Не валидируем, если это префаб-ассет (не экземпляр)
        if (PrefabUtility.IsPartOfPrefabAsset(this)) return;

        // 1. Автоматически инициализируем и валидируем систему в редакторе
        InitializeAndValidateSystem();

        // 2. Сохраняем значения для префаба
        if (PrefabUtility.IsPartOfPrefabInstance(this))
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }

    public void UpdateEnviromentLight()
    {
        if (transform.rotation == _oldRotation) return;

        _oldRotation = transform.rotation;
        DynamicGI.UpdateEnvironment();
        SceneView.RepaintAll();
    }
#endif
}