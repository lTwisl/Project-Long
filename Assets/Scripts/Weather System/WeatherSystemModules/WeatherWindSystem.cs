using UnityEngine;

public class WeatherWindSystem : MonoBehaviour, IWeatherSystem
{
    [field: SerializeField, DisableEdit] public bool IsSystemValid { get; set; } = true;

    [Header("- - Текущие параметры ветра:")]
    [SerializeField, DisableEdit] private Vector2 _windGloabalDirection;
    [field: SerializeField, DisableEdit] public float CurrentSpeed { get; private set; }
    [field: SerializeField, Range(-20, 20)] public float MaxWindTemperature { get; private set; } = -10;


    [Header("Параметры глобального ветра:")]
    [SerializeField, DisableEdit, Tooltip("Минимальная скорость ветра"), Min(1)]
    private float _minWindSpeed = 2f;
    [SerializeField, DisableEdit, Tooltip("Максимальная скорость ветра"), Min(1)]
    private float _maxWindSpeed = 15f;
    [SerializeField, DisableEdit, Tooltip("Интенсивность изменений скорости ветра"), Range(0.01f, 5f)]
    private float _intensityChangeSpeed = 1f;
    [Space(8)]
    [SerializeField, DisableEdit, Tooltip("Резкость изменения направления ветра по шуму Перлина (0.01 - штиль; 0.3 - порывистый ветер)"), Range(0.001f, 2f)]
    private float _directionChangeSharpness = 0.3f;
    [SerializeField, DisableEdit, Tooltip("Интенсивность изменения направления ветра"), Range(0.01f, 5f)]
    private float _intensityChangeDirection = 1f;

    [Header("Параметры локального ветра:")]
    [SerializeField, Tooltip("Размер шума перлина для изменения интенсивности"), Range(0.001f, 0.5f)]
    private float _tilingNoiseWindSpeed = 0.005f;
    [SerializeField, Tooltip("На сколько сильно интенсивность зависит от шума"), Range(0f, 1f)]
    private float _influenceNoiseWindSpeed = 1f;
    [SerializeField, Tooltip("Включить движения шума интенсивности")]
    private bool _useNoiseWindSpeedMotion = true;
    [HideIf(nameof(_useNoiseWindSpeedMotion), false), SerializeField, Tooltip("Множитель смещения шума от скорости глобального ветра"), Range(0.1f, 10f)]
    private float _noiseWindSpeedSpeedMul = 1f;

    [Header("Параметры визуализации:")]
    [SerializeField] private bool _drawWindField = true;
    [HideIf(nameof(_drawWindField), false), SerializeField, Range(1, 100)]
    private int _vectorsGrid = 4;
    [HideIf(nameof(_drawWindField), false), SerializeField, Range(0.1f, 10)]
    private float _vectorsSizeMul = 1f;
    [HideIf(nameof(_drawWindField), false), SerializeField]
    private Gradient _intensityColorGradient;
    public const float MaxWindIntensity = 33f; // Рабочий параметр шкалы Бофорта = 33 м/с

    private Vector2 _directionNoiseOffset;
    private Vector2 _intensityNoiseOffset;


    private void Awake()
    {
        InitializeWindSystem();
    }

    private void InitializeWindSystem()
    {
        // Инициализация стартового направления
        _directionNoiseOffset = Random.insideUnitCircle * 100f;
        _intensityNoiseOffset = Random.insideUnitCircle * 100f;

        // Инициализация стартовой скорости
        CurrentSpeed = (_minWindSpeed + _maxWindSpeed) * 0.5f;
    }

    void Update()
    {
        UpdateWindDirection();
        UpdateWindSpeed();
        UpdateNoiseOffset();
    }

    private void UpdateWindDirection()
    {
        Vector2 noiseVector = new Vector2(
            Mathf.PerlinNoise(Time.time * _directionChangeSharpness, _directionNoiseOffset.x),
            Mathf.PerlinNoise(_directionNoiseOffset.y, Time.time * _directionChangeSharpness)
        ) * 2 - Vector2.one;

        _windGloabalDirection = Vector2.Lerp(
            _windGloabalDirection,
            noiseVector.normalized,
            Time.deltaTime * _intensityChangeDirection
        ).normalized;
    }

    private void UpdateWindSpeed()
    {
        float noise = Mathf.PerlinNoise(Time.time * _tilingNoiseWindSpeed, _intensityNoiseOffset.x);
        float targetSpeed = Mathf.Lerp(_minWindSpeed, _maxWindSpeed, noise);
        CurrentSpeed = Mathf.Lerp(CurrentSpeed, targetSpeed, Time.deltaTime * _intensityChangeSpeed);
    }

    private void UpdateNoiseOffset()
    {
        if (_useNoiseWindSpeedMotion)
        {
            // Смещение шума направления
            _directionNoiseOffset += _windGloabalDirection * (CurrentSpeed * _noiseWindSpeedSpeedMul * Time.deltaTime);

            // Смещение шума интенсивности
            _intensityNoiseOffset -= _windGloabalDirection * (CurrentSpeed * _noiseWindSpeedSpeedMul * Time.deltaTime);
        }
    }


    #region Работа с системой ветра

    /// <summary>
    /// Получить глобальное нормализованное направление ветра
    /// </summary>
    public Vector2 GetWindGlobalVectorNormalized()
    {
        return new Vector3(_windGloabalDirection.x, _windGloabalDirection.y);
    }

    /// <summary>
    /// Получить глобальное направление ветра с учетом интенсивности
    /// </summary>
    public Vector2 GetWindGlobalVector()
    {
        return new Vector3(_windGloabalDirection.x, _windGloabalDirection.y) * CurrentSpeed;
    }

    /// <summary>
    /// Получить локальное направление ветра с учетом интенсивности и ветровых зон
    /// </summary>
    public Vector2 GetWindLocalVector(Vector3 worldPosition)
    {
        return GetWindGlobalVectorNormalized() * GetWindLocalIntensity(worldPosition);
    }

    /// <summary>
    /// Получить локальную интенсивность ветра с учетом ветровых зон
    /// </summary>
    public float GetWindLocalIntensity(Vector3 worldPosition)
    {
        return CalculateLocalIntensity(worldPosition);
    }

    private float CalculateLocalIntensity(Vector3 position)
    {
        Vector2 noisePos = new Vector2(
            position.x + _intensityNoiseOffset.x,
            position.z + _intensityNoiseOffset.y
        );

        float noise = Mathf.PerlinNoise(
            noisePos.x * _tilingNoiseWindSpeed,
            noisePos.y * _tilingNoiseWindSpeed
        );

        return Mathf.Clamp(
            Mathf.Lerp(
                _maxWindSpeed * (1 - _influenceNoiseWindSpeed),
                _maxWindSpeed * (1 + _influenceNoiseWindSpeed),
                noise
            ),
            _minWindSpeed,
            _maxWindSpeed
        );
    }

    public void UpdateSystem(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        float minWindSpeed = Mathf.Lerp(currentProfile.MinWindSpeed, newProfile.MinWindSpeed, t);
        float maxWindSpeed = Mathf.Lerp(currentProfile.MaxWindSpeed, newProfile.MaxWindSpeed, t);
        float intensityChangeSpeed = Mathf.Lerp(currentProfile.IntensityChangeSpeed, newProfile.IntensityChangeSpeed, t);
        float directionChangeSharpness = Mathf.Lerp(currentProfile.DirectionChangeSharpness, newProfile.DirectionChangeSharpness, t);
        float intensityChangeDirection = Mathf.Lerp(currentProfile.IntensityChangeDirection, newProfile.IntensityChangeDirection, t);
        InitializeSystemParameters(minWindSpeed, maxWindSpeed, intensityChangeSpeed, directionChangeSharpness, intensityChangeDirection);
    }

    public void ValidateSystem() { }

    /// <summary>
    /// Установить базовые параметры ветра
    /// </summary>
    public void InitializeSystemParameters(float minWindSpeed, float maxWindSpeed, float intensityChangeSpeed, float directionChangeSharpness, float intensityChangeDirection)
    {
        // Приводим к допустимым границам скорости ветра
        _minWindSpeed = Mathf.Clamp(minWindSpeed, 1f, MaxWindIntensity);
        _maxWindSpeed = Mathf.Clamp(maxWindSpeed, 1f, MaxWindIntensity);

        // Ограничиваем параметры чтобы они соответствовали минимуму и максмуму
        _minWindSpeed = Mathf.Min(_minWindSpeed, _maxWindSpeed);
        _maxWindSpeed = Mathf.Max(_minWindSpeed, _maxWindSpeed);
        _intensityChangeSpeed = Mathf.Clamp(intensityChangeSpeed, 0.01f, 5f);
        _directionChangeSharpness = Mathf.Clamp(directionChangeSharpness, 0.001f, 2f);
        _intensityChangeSpeed = Mathf.Clamp(intensityChangeDirection, 0.01f, 5f);
    }
    #endregion

    #region ВИЗУАЛИЗАЦИЯ

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_drawWindField || !Application.isPlaying) return;

        DrawWindField();
    }

    private void DrawWindField()
    {
        Vector3 basePosition = transform.position;
        float gridSize = (_vectorsGrid - 1) * MaxWindIntensity;
        Vector3 startPosition = basePosition - new Vector3(gridSize * 0.5f, 0, gridSize * 0.5f);

        // Отрисовка фона сетки
        Gizmos.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Темно-серый с прозрачностью
        for (int x = 0; x < _vectorsGrid; x++)
        {
            for (int z = 0; z < _vectorsGrid; z++)
            {
                Vector3 cellCenter = startPosition + new Vector3(x * MaxWindIntensity, 0, z * MaxWindIntensity);
                Gizmos.DrawWireCube(cellCenter, new Vector3(MaxWindIntensity, 0, MaxWindIntensity));
            }
        }

        // Отрисовка векторов
        for (int x = 0; x < _vectorsGrid; x++)
        {
            for (int z = 0; z < _vectorsGrid; z++)
            {
                Vector3 cellCenter = startPosition + new Vector3(x * MaxWindIntensity, 0, z * MaxWindIntensity);

                //GeometryShapesDrawer.DrawWindArrow(cellCenter, CalculateLocalIntensity(cellCenter));
            }
        }
    }

    //private void DrawWindArrow(Vector3 position, float localSpeed)
    //{
    //    Vector3 direction = new Vector3(_windGloabalDirection.x, 0, _windGloabalDirection.y);
    //    float arrowLength = localSpeed * _vectorsSizeMul;

    //    Gizmos.color = GetColorOfIntensity(localSpeed);
    //    Quaternion rotation = Quaternion.LookRotation(direction.normalized);

    //    Vector3 tip = position + rotation * Vector3.forward * arrowLength * 0.5f;
    //    Vector3 tail = position - rotation * Vector3.forward * arrowLength * 0.5f;

    //    Gizmos.DrawLine(tail, tip);

    //    float wingSize = arrowLength * 0.25f;
    //    Vector3 rightWing = tip + rotation * (-Vector3.forward + Vector3.right) * wingSize;
    //    Vector3 leftWing = tip + rotation * (-Vector3.forward - Vector3.right) * wingSize;

    //    Gizmos.DrawLine(tip, rightWing);
    //    Gizmos.DrawLine(tip, leftWing);
    //}

    private Color GetColorOfIntensity(float intensity)
    {
        // Интерпретация значений цвета ветра по шкале Бофорта
        return _intensityColorGradient.Evaluate(Mathf.InverseLerp(0, MaxWindIntensity, intensity));
    }
#endif

    #endregion
}