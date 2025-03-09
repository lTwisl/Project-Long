using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class WindSystem : MonoBehaviour
{
    public static WindSystem Instance { get; private set; }

    [SerializeField] private Vector2 _windGloabalDirection;
    [SerializeField] private float _currentSpeed;

    [Header("Параметры ветра:")]
    [SerializeField, Min(1)] private float _minSpeed = 2f;
    [SerializeField, Min(1)] private float _maxSpeed = 15f;
    [SerializeField, Tooltip("Скорость интерполяции изменения направления"), Range(0.01f, 5f)] private float _speedChange = 1f;
    [SerializeField, Tooltip("Размер шума перлина для изменения направления"), Range(0.001f, 2f)] private float _directionNoiseScale = 0.3f;

    [Header("Параметры локальной интенсивности:")]
    [SerializeField, Tooltip("На сколько сильно интенсивность зависит от шума"), Range(0f, 1f)] private float _intensityVariation = 0.3f;
    [SerializeField, Tooltip("Размер шума перлина для изменения интенсивности"), Range(0.001f, 0.5f)] private float _intensityNoiseScale = 0.5f;
    [SerializeField, Tooltip("Включить движения шума интенсивности")] private bool _enableIntensityMotion = true;
    [SerializeField, Tooltip("Множитель смещения шума от скорости глобального ветра"), Range(0.1f, 10f)] private float _noiseDriftSpeedMul = 0.5f;

    [Header("Параметры визуализации:")]
    [SerializeField] private bool _drawWindField = true;
    [SerializeField, Range(1, 100)] private int _vectorsCount = 4;
    [SerializeField, Range(0.1f, 10)] private float _vectorsSizeMul = 1f;
    [SerializeField] private Gradient _intensityColorGradient;
    private const float _maxSize = 33f; // Рабочий параметр шкалы Бофорта = 33 м/с

    [Header("Ветровые локальные зоны влияния:")]
    [SerializeField] private List<WindZone> _windZones = new List<WindZone>();

    private Vector2 _directionNoiseOffset;
    private Vector2 _intensityNoiseOffset;

    #region Инициализация системы
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeWindSystem();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeWindSystem()
    {
        // Инициализация стартового направления
        _directionNoiseOffset = Random.insideUnitCircle * 100f;
        _intensityNoiseOffset = Random.insideUnitCircle * 100f;

        // Инициализация стартовой скорости
        _currentSpeed = (_minSpeed + _maxSpeed) * 0.5f;
    }
    #endregion

    void Update()
    {
        UpdateWindDirection();
        UpdateWindSpeed();
        UpdateNoiseOffset();
    }

    #region Логика направления ветра
    private void UpdateWindDirection()
    {
        Vector2 noiseVector = new Vector2(
            Mathf.PerlinNoise(Time.time * _directionNoiseScale, _directionNoiseOffset.x),
            Mathf.PerlinNoise(_directionNoiseOffset.y, Time.time * _directionNoiseScale)
        ) * 2 - Vector2.one;

        _windGloabalDirection = Vector2.Lerp(
            _windGloabalDirection,
            noiseVector.normalized,
            Time.deltaTime * _speedChange
        ).normalized;
    }
    #endregion

    #region Логика скорости ветра
    private void UpdateWindSpeed()
    {
        float noise = Mathf.PerlinNoise(Time.time * _intensityNoiseScale, _intensityNoiseOffset.x);
        float targetSpeed = Mathf.Lerp(_minSpeed, _maxSpeed, noise);
        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * _speedChange);
    }
    #endregion

    #region Логика смещения шумов
    private void UpdateNoiseOffset()
    {
        if (_enableIntensityMotion)
        {
            // Смещение шума направления
            _directionNoiseOffset += _windGloabalDirection * (_currentSpeed * _noiseDriftSpeedMul * Time.deltaTime);

            // Смещение шума интенсивности
            _intensityNoiseOffset -= _windGloabalDirection * (_currentSpeed * _noiseDriftSpeedMul * Time.deltaTime);
        }
    }
    #endregion

    #region Работа с системой ветра

    /// <summary>
    /// Получить глобальное направление ветра нормализованное
    /// </summary>
    public Vector3 GetWindGlobalVectorNormalized()
    {
        return new Vector3(_windGloabalDirection.x, 0, _windGloabalDirection.y);
    }

    /// <summary>
    /// Получить глобальное направление ветра с учетом интенсивности
    /// </summary>
    public Vector3 GetWindGlobalVector()
    {
        return new Vector3(_windGloabalDirection.x, 0, _windGloabalDirection.y) * _currentSpeed;
    }

    /// <summary>
    /// Получить локальное направление ветра с учетом интенсивности и ветровых зон
    /// </summary>
    public Vector3 GetWindLocalVector(Vector3 worldPosition)
    {
        float baseIntensity = CalculateLocalIntensity(worldPosition);
        float zoneMultiplier = CalculateZoneMultiplier(worldPosition);
        return GetWindGlobalVectorNormalized() * (baseIntensity * zoneMultiplier);
    }

    private float CalculateLocalIntensity(Vector3 position)
    {
        Vector2 noisePos = new Vector2(
            position.x + _intensityNoiseOffset.x,
            position.z + _intensityNoiseOffset.y
        );

        float noise = Mathf.PerlinNoise(
            noisePos.x * _intensityNoiseScale,
            noisePos.y * _intensityNoiseScale
        );

        return Mathf.Clamp(
            Mathf.Lerp(
                _maxSpeed * (1 - _intensityVariation),
                _maxSpeed * (1 + _intensityVariation),
                noise
            ),
            _minSpeed,
            _maxSpeed
        );
    }

    private float CalculateZoneMultiplier(Vector3 position)
    {
        float totalMultiplier = 1f;
        foreach (var zone in _windZones)
        {
            if (zone.transform != null)
            {
                Vector2 flatPos = new Vector2(position.x, position.z);
                Vector2 zonePos = new Vector2(zone.transform.position.x, zone.transform.position.z);
                float distance = Vector2.Distance(flatPos, zonePos);

                if (distance <= zone.radius)
                {
                    float normalizedDistance = Mathf.Clamp01(distance / zone.radius);
                    float influence = zone.falloffCurve.Evaluate(normalizedDistance);
                    totalMultiplier += (zone.intensityMultiplier - 1) * influence;
                }
            }
        }
        return Mathf.Clamp(totalMultiplier, 0.1f, 10f);
    }

    /// <summary>
    /// Установить базовые параметры ветра
    /// </summary>
    public void InitializeSystemParameters(float minSpeed, float maxSpeed, float speedChange, float directionNoiseScale)
    {
        _minSpeed = Mathf.Clamp(minSpeed, 1f, _maxSize);
        _maxSpeed = Mathf.Clamp(maxSpeed, 1f, _maxSize);
        _minSpeed = Mathf.Min(_minSpeed, _maxSpeed);
        _maxSpeed = Mathf.Max(_minSpeed, _maxSpeed);
        _speedChange = Mathf.Clamp(speedChange, 0.01f, 5f);
        _directionNoiseScale = Mathf.Clamp(directionNoiseScale, 0.001f, 2f);
        _currentSpeed = Mathf.Clamp(_currentSpeed, _minSpeed, _maxSpeed);
    }

    /// <summary>
    /// Добавить новую ветровую зону с автоматическим именем
    /// </summary>
    public void AddWindZone(Transform zoneTransform, float radius, float multiplier)
    {
        string autoName = $"Wind Zone {_windZones.Count + 1}";
        _windZones.Add(new WindZone
        {
            name = autoName,
            transform = zoneTransform,
            radius = Mathf.Max(0, radius),
            intensityMultiplier = Mathf.Clamp(multiplier, 0.1f, 10f)
        });

        // Переименовать объект в сцене
        RenameZoneObject(_windZones.Count - 1);
    }

    /// <summary>
    /// Обновить все имена зон по шаблону "Wind Zone N"
    /// </summary>
    public void GenerateZoneNames()
    {
        for (int i = 0; i < _windZones.Count; i++)
        {
            if (string.IsNullOrEmpty(_windZones[i].name))
                _windZones[i].name = $"Wind Zone {i + 1}";

            // Переименовать объект в сцене
            RenameZoneObject(i);
        }
    }

    /// <summary>
    /// Переименовать объект в сцене в соответствии с именем зоны
    /// </summary>
    private void RenameZoneObject(int index)
    {
        if (index < 0 || index >= _windZones.Count) return;

        var zone = _windZones[index];
        if (zone.transform != null && zone.renameTransform)
        {
            zone.transform.name = zone.name;
            zone.renameTransform = false;
        }
    }

    /// <summary>
    /// Очистить все ветровые зоны
    /// </summary>
    public void ClearAllWindZones()
    {
        _windZones.Clear();
    }

    #endregion

    #region Отрисовка поля векторов ветра
    private void OnDrawGizmos()
    {
        DrawWindZones();
        if (!_drawWindField || !Application.isPlaying) return;

        DrawWindField();
    }

    private void DrawWindField()
    {
        Vector3 basePosition = transform.position;
        float gridSize = (_vectorsCount - 1) * _maxSize;
        Vector3 startPosition = basePosition - new Vector3(gridSize * 0.5f, 0, gridSize * 0.5f);
        
        // Отрисовка фона сетки
        Gizmos.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Темно-серый с прозрачностью
        for (int x = 0; x < _vectorsCount; x++)
        {
            for (int z = 0; z < _vectorsCount; z++)
            {
                Vector3 cellCenter = startPosition + new Vector3(x * _maxSize, 0, z * _maxSize);
                Gizmos.DrawWireCube(cellCenter, new Vector3(_maxSize, 0, _maxSize));
            }
        }

        // Отрисовка векторов
        for (int x = 0; x < _vectorsCount; x++)
        {
            for (int z = 0; z < _vectorsCount; z++)
            {
                Vector3 cellCenter = startPosition +
                    new Vector3(x * _maxSize, 0, z * _maxSize);

                // Расчет интенсивности с учетом зон
                float baseIntensity = CalculateLocalIntensity(cellCenter);
                float zoneMultiplier = CalculateZoneMultiplier(cellCenter);
                float finalIntensity = baseIntensity * zoneMultiplier;

                DrawWindArrow(cellCenter, finalIntensity);
            }
        }
    }

    private void DrawWindArrow(Vector3 position, float localSpeed)
    {
        Vector3 direction = new Vector3(_windGloabalDirection.x, 0, _windGloabalDirection.y);
        float arrowLength = localSpeed * _vectorsSizeMul;

        Gizmos.color = GetIntensityColor(localSpeed);
        Quaternion rotation = Quaternion.LookRotation(direction.normalized);

        Vector3 tip = position + rotation * Vector3.forward * arrowLength * 0.5f;
        Vector3 tail = position - rotation * Vector3.forward * arrowLength * 0.5f;

        Gizmos.DrawLine(tail, tip);

        float wingSize = arrowLength * 0.25f;
        Vector3 rightWing = tip + rotation * (-Vector3.forward + Vector3.right) * wingSize;
        Vector3 leftWing = tip + rotation * (-Vector3.forward - Vector3.right) * wingSize;

        Gizmos.DrawLine(tip, rightWing);
        Gizmos.DrawLine(tip, leftWing);
    }

    private Color GetIntensityColor(float intensity)
    {
        float t = Mathf.InverseLerp(0, _maxSize, intensity); // Интерпретация значений цвета ветра по шкале Бофорта
        return _intensityColorGradient.Evaluate(t);
    }

    private void DrawWindZones()
    {
        foreach (var zone in _windZones)
        {
            if (zone.transform != null && _drawWindField)
            {
                // Градиентная сфера
                int segments = 7;
                for (int i = 0; i < segments; i++)
                {
                    float t = i / (float)segments;
                    float radius = zone.radius * t;

                    // Получаем значение кривой затухания
                    float curveValue = zone.falloffCurve.Evaluate(t);

                    // Определяем цвет на основе значения кривой
                    Color zoneColor = GetZoneColor(curveValue);
                    zoneColor.a = Mathf.Lerp(1f, 0.5f, t); // Прозрачность уменьшается к краям

                    Gizmos.color = zoneColor;
                    Gizmos.DrawWireSphere(zone.transform.position, radius);
                }

                // Подпись с силой влияния
                Handles.Label(zone.transform.position + Vector3.up * 1.5f, $"Influence: {zone.intensityMultiplier:F2}");
            }
        }
    }

    // Метод для получения цвета на основе значения кривой
    private Color GetZoneColor(float value)
    {
        return Color.Lerp(Color.red, Color.green, Mathf.Clamp01(value));
    }

    // Дешевый и простой вариант отображения зон
    //private void DrawWindZones()
    //{
    //    foreach (var zone in _windZones)
    //    {
    //        if (zone.transform != null && _drawWindField)
    //        {
    //            // Цветовая индикация
    //            float intensity = Mathf.Clamp01(zone.intensityMultiplier);
    //            Gizmos.color = Color.Lerp(Color.green, Color.red, intensity);
    //            Gizmos.DrawWireSphere(zone.transform.position, zone.radius);

    //            // Подпись с силой влияния
    //            UnityEditor.Handles.Label(zone.transform.position + Vector3.up * 1.5f, $"Multiplier: {zone.intensityMultiplier:F2}");
    //        }
    //    }
    //}
    #endregion
}