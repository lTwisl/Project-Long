using UnityEngine;

public class WeatherWindSystem : MonoBehaviour, IWeatherSystem
{
    #region ПЕРЕМЕННЫЕ КЛАССА
    [field: SerializeField, DisableEdit] public bool IsSystemValid { get; set; }

    [Header("- - Текущие параметры ветра:")]
    [SerializeField, DisableEdit] private Vector2 _windGlobalDirection;
    [field: SerializeField, DisableEdit] public float CurrentSpeed { get; private set; }

    [Header("- - Параметры глобального ветра:")]
    [SerializeField, DisableEdit, Range(0, 33)] private float _minWindSpeed = 2f;
    [SerializeField, DisableEdit, Range(0, 33)] private float _maxWindSpeed = 15f;

    [Space(10)]
    [Tooltip("Интенсивность изменения скорости ветра. (0.01 - очень медленная смена текущей скорости ветра; 10 - порывистая, почти мгновенная смена скорости ветра)")]
    [SerializeField, DisableEdit, Range(0.01f, 10f)] private float _windSpeedInterpolationSpeed = 1f;

    [Tooltip("Резкость изменения направления ветра (0.001 - штиль; 2 - порывистый ветер)")]
    [SerializeField, DisableEdit, Range(0.001f, 2f)] private float _windDirectionNoiseSharpness = 0.3f;

    [Tooltip("Интенсивность изменения направления ветра. (0.01 - очень медленная смена текущего направления ветра; 3 - порывистая, почти мгновенная смена направления ветра)")]
    [SerializeField, DisableEdit, Range(0.01f, 3f)] private float _windDirectionInterpolationSpeed = 1f;

    [Header("- - Параметры локального ветра:")]
    [Tooltip("Размер шума Перлина, по которому изменяется интенсивность")]
    [SerializeField, Range(0.001f, 0.5f)] private float _windIntensityNoiseTiling = 0.005f;

    [Tooltip("Влияние шума Перлина на локальную скорость ветра")]
    [SerializeField, Range(0f, 1f)] private float _windIntensityNoiseInfluence = 1f;

    [Tooltip("Множитель скорости смещения шума Перлина от текущей глобальной скорости ветра")]
    [SerializeField, Range(0.1f, 10f)] private float _noiseWindSpeedSpeedMul = 1f;

    [Header("- - Параметры визуализации:")]
    [SerializeField] private bool _drawWindField = true;
    [SerializeField, HideIf(nameof(_drawWindField), false), Range(1, 20)] private int _windVectorsGridSize = 4;
    [SerializeField, HideIf(nameof(_drawWindField), false), Range(0.1f, 2)] private float _windArrowsSizeMultiplier = 1f;
    [SerializeField, HideIf(nameof(_drawWindField), false)] private Gradient _gradientIntensityToColor;

    public const float MaxWindTemperature = -10; // Максимальное влияние ветра на ощущаемую температуру
    public const float MaxWindIntensity = 33f; // Рабочий параметр шкалы Бофорта = 33 м/с

    private Vector2 _directionNoiseOffset;
    private Vector2 _intensityNoiseOffset;
    #endregion


    private void Awake()
    {
        InitializeAndValidateSystem();
    }

    public void InitializeAndValidateSystem()
    {
        // 1. Инициализация стартового направления
        _directionNoiseOffset = Random.insideUnitCircle * 100f;
        _intensityNoiseOffset = Random.insideUnitCircle * 100f;

        // 2. Инициализация стартовой скорости
        CurrentSpeed = (_minWindSpeed + _maxWindSpeed) * 0.5f;

        IsSystemValid = true;
    }

    void Update()
    {
        UpdateWindDirection();
        UpdateWindSpeed();
        UpdateNoiseOffset();
    }

    private void UpdateWindDirection()
    {
        Vector2 noiseVector = new Vector2(Mathf.PerlinNoise(Time.time * _windDirectionNoiseSharpness, _directionNoiseOffset.x),
            Mathf.PerlinNoise(_directionNoiseOffset.y, Time.time * _windDirectionNoiseSharpness)) * 2 - Vector2.one;

        _windGlobalDirection = Vector2.Lerp(
            _windGlobalDirection,
            noiseVector.normalized,
            Time.deltaTime * _windDirectionInterpolationSpeed
        ).normalized;
    }

    private void UpdateWindSpeed()
    {
        float noise = Mathf.PerlinNoise(Time.time * _windIntensityNoiseTiling, _intensityNoiseOffset.x);
        float targetSpeed = Mathf.Lerp(_minWindSpeed, _maxWindSpeed, noise);
        CurrentSpeed = Mathf.Lerp(CurrentSpeed, targetSpeed, Time.deltaTime * _windSpeedInterpolationSpeed);
    }

    private void UpdateNoiseOffset()
    {
        Vector2 windDir = _windGlobalDirection * CurrentSpeed * _noiseWindSpeedSpeedMul * Time.deltaTime;
        _directionNoiseOffset += windDir;
        _intensityNoiseOffset -= windDir;
    }

    #region ФУНКЦИИ РАБОТЫ С СИСТЕМОЙ
    /// <summary>
    /// Получить глобальное нормализованное направление ветра
    /// </summary>
    public Vector3 GetWindGlobalVectorNormalized()
    {
        return new(_windGlobalDirection.x, 0, _windGlobalDirection.y);
    }

    /// <summary>
    /// Получить глобальное направление ветра с учетом интенсивности
    /// </summary>
    public Vector3 GetWindGlobalVector()
    {
        return GetWindGlobalVectorNormalized() * CurrentSpeed;
    }

    /// <summary>
    /// Получить локальное направление ветра с учетом интенсивности
    /// </summary>
    public Vector3 GetWindLocalVector(Vector3 worldPosition)
    {
        return GetWindGlobalVectorNormalized() * GetWindLocalIntensity(worldPosition);
    }

    /// <summary>
    /// Получить локальную интенсивность ветра
    /// </summary>
    public float GetWindLocalIntensity(Vector3 worldPosition)
    {
        return CalculateLocalIntensity(worldPosition);
    }

    private float CalculateLocalIntensity(Vector3 position)
    {
        Vector2 noisePos = new(position.x + _intensityNoiseOffset.x, position.z + _intensityNoiseOffset.y);
        float noise = Mathf.PerlinNoise(noisePos.x * _windIntensityNoiseTiling, noisePos.y * _windIntensityNoiseTiling);

        float lerpMin = _maxWindSpeed * (1 - _windIntensityNoiseInfluence);
        float lerpMax = _maxWindSpeed * (1 + _windIntensityNoiseInfluence);

        return Mathf.Clamp(Mathf.Lerp(lerpMin, lerpMax, noise), _minWindSpeed, _maxWindSpeed);
    }

    public void UpdateSystemParameters(WeatherProfile currentProfile, WeatherProfile nextProfile, float t)
    {
        float minWindSpeed = Mathf.Lerp(currentProfile.MinWindSpeed, nextProfile.MinWindSpeed, t);
        float maxWindSpeed = Mathf.Lerp(currentProfile.MaxWindSpeed, nextProfile.MaxWindSpeed, t);
        float windSpeedInterpolationSpeed = Mathf.Lerp(currentProfile.IntensityChangeSpeed, nextProfile.IntensityChangeSpeed, t);
        float windDirectionNoiseSharpness = Mathf.Lerp(currentProfile.DirectionChangeSharpness, nextProfile.DirectionChangeSharpness, t);
        float windDirectionInterpolationSpeed = Mathf.Lerp(currentProfile.IntensityChangeDirection, nextProfile.IntensityChangeDirection, t);

        _minWindSpeed = Mathf.Clamp(minWindSpeed, 0, MaxWindIntensity);
        _maxWindSpeed = Mathf.Clamp(maxWindSpeed, 0, MaxWindIntensity);

        _minWindSpeed = Mathf.Min(_minWindSpeed, _maxWindSpeed);
        _maxWindSpeed = Mathf.Max(_minWindSpeed, _maxWindSpeed);

        _windSpeedInterpolationSpeed = Mathf.Clamp(windSpeedInterpolationSpeed, 0.01f, 5f);
        _windDirectionNoiseSharpness = Mathf.Clamp(windDirectionNoiseSharpness, 0.001f, 2f);
        _windDirectionInterpolationSpeed = Mathf.Clamp(windDirectionInterpolationSpeed, 0.01f, 5f);
    }
    #endregion


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_drawWindField || !Application.isPlaying) return;

        Vector3 startPosition = transform.position - new Vector3(MaxWindIntensity * (_windVectorsGridSize - 1) / 2, 0, MaxWindIntensity * (_windVectorsGridSize - 1) / 2);

        GeometryShapesDrawer.DrawGrid(transform.position, MaxWindIntensity, _windVectorsGridSize - 1, new Color(0.1f, 0.1f, 0.1f, 1f));

        for (int x = 0; x < _windVectorsGridSize; x++)
        {
            for (int z = 0; z < _windVectorsGridSize; z++)
            {
                Vector3 cellCenter = startPosition + new Vector3(x * MaxWindIntensity, 0, z * MaxWindIntensity);
                Vector3 direction = new Vector3(_windGlobalDirection.x, 0f, _windGlobalDirection.y).normalized;
                Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                float intensity = GetWindLocalIntensity(cellCenter);
                GeometryShapesDrawer.DrawArrow(cellCenter, rotation, intensity * _windArrowsSizeMultiplier, 0.25f, _gradientIntensityToColor.Evaluate(Mathf.InverseLerp(0, MaxWindIntensity, intensity)));
            }
        }
    }
#endif
}