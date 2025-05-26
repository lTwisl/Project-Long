using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class WeatherSystem : MonoBehaviour
{
    #region ПЕРЕМЕННЫЕ КЛАССА
    public event Action<WeatherProfile> OnWeatherTransitionStarted;
    public event Action<WeatherProfile> OnWeatherTransitionFinished;

    [field: SerializeField] public bool UseAutoTransition { get; private set; } = true;

    [field: Header("Все погодные профили:")]
    [field: SerializeField] public List<WeatherProfile> AvailableWeatherProfiles { get; private set; }


    [field: Header("Текущие погодные профили:")]
    [field: SerializeField] public WeatherProfile CurrentWeatherProfile { get; private set; }
    [field: SerializeField, DisableEdit] public WeatherProfile NewWeatherProfile { get; private set; }


    [field: Header("Параметры текущей погоды:")]
    [field: SerializeField, DisableEdit] public float Temperature { get; private set; }
    [field: SerializeField, DisableEdit] public float Wetness { get; private set; }
    [field: SerializeField, DisableEdit] public float Toxicity { get; private set; }


    [field: Header("Системы освещения сцены:")]
    [field: SerializeField] public WeatherLightingColor SunLight { get; private set; }
    [field: SerializeField] public WeatherLightingColor MoonLight { get; private set; }
    [field: SerializeField, DisableEdit] public bool IsLightingSystemsValid { get; private set; }


    [field: Header("Система ветра:")]
    [field: SerializeField] public WeatherWindSystem WeatherWindSystem { get; private set; }
    [field: SerializeField, DisableEdit] public bool IsWindSystemValid { get; private set; }


    [field: Header("Система объемного тумана:")]
    [field: SerializeField] public WeatherFogSystem WeatherFogSystem { get; private set; }
    [field: SerializeField, DisableEdit] public bool IsFogSystemValid { get; private set; }


    [field: Header("Система скайбокса:")]
    [field: SerializeField] public WeatherSkyboxSystem WeatherSkyboxSystem { get; private set; }
    [field: SerializeField, DisableEdit] public bool IsSkyboxSystemValid { get; private set; }


    [field: Header("Система пост процессинга:")]
    [field: SerializeField] public WeatherPostProcessSystem WeatherPostProcessSystem { get; private set; }
    [field: SerializeField, DisableEdit] public bool IsPostProcessSystemValid { get; private set; }


    [field: Header("Система визуальных эффектов:")]
    [field: SerializeField] public WeatherVFXSystem WeatherVFXSystem { get; private set; }
    [field: SerializeField, DisableEdit] public bool IsVfxSystemValid { get; private set; }

    public TimeSpan TimeStartCurrentWeather { get; private set; }
    public TimeSpan TimeEndCurrentWeather { get; private set; }
    public TimeSpan TransitionDuration { get; private set; } = new TimeSpan(0, 1, 0, 0); // Время перехода между сменяемыми погодными условиями

    private Process _waitNextTransition;
    private Coroutine _transitionCoroutine;
    #endregion

    /// <summary>
    /// Находится ли система погодных условий в переходном состоянии?
    /// </summary>
    public bool IsWeatherOnTransitionState => _transitionCoroutine != null;

    private void Awake()
    {
        ValidateReferences();
    }

    /// <summary>
    /// Проверка модулей системы погодных условий
    /// </summary>
    public void ValidateReferences()
    {
        IsLightingSystemsValid = SunLight != null && MoonLight != null;
        IsWindSystemValid = WeatherWindSystem != null;
        IsFogSystemValid = WeatherFogSystem != null;
        IsSkyboxSystemValid = WeatherSkyboxSystem != null;
        IsPostProcessSystemValid = WeatherPostProcessSystem != null;
        IsVfxSystemValid = WeatherVFXSystem != null;

        // Выводы для отладки:
        //if (!_isProfilesValide) Debug.LogWarning("<color=orange>В сцене не инициализированы профили погоды</color>", this);
        //if (!_isLightingSystemsValide) Debug.LogWarning("<color=orange>Потеряна ссылка на источник света</color>", this);
        //if (!_isWindSystemValide) Debug.LogWarning("<color=orange>Потеряна ссылка на WeatherWindSystem</color>", this);
        //if (!_isFogSystemValide) Debug.LogWarning("<color=orange>Потеряна ссылка на WeatherFogSystem</color>", this);
        //if (!_isSkyboxSystemValide) Debug.LogWarning("<color=orange>Потеряна ссылка на WeatherSkyboxSystem</color>", this);
        //if (!_isPostProcessSystemValide) Debug.LogWarning("<color=orange>Потеряна ссылка на WeatherPostProcessSystem</color>", this);
        //if (!_isPostProcessSystemValide) Debug.LogWarning("<color=orange>Потеряна ссылка на WeatherVFXSystem</color>", this);
    }

    private void Start()
    {
        // Warning!!! Временное решение, ждем сохранения. Стартовая конфигурация сцены при запуске
        SetNewWeatherImmediately(CurrentWeatherProfile);
    }

    /// <summary>
    /// Сменить погоду мгновенно
    /// </summary>
    public void SetNewWeatherImmediately(WeatherProfile weatherProfile)
    {
        if (weatherProfile == null)
        {
            Debug.LogError("<color=red>Попытка установить null профиль погоды!</color>");
            return;
        }

        if (CurrentWeatherProfile == null) CurrentWeatherProfile = weatherProfile;
        NewWeatherProfile = weatherProfile;

        StopWeatherTransition();
        OnWeatherTransitionStarted?.Invoke(NewWeatherProfile);
        SetupVFXSystem();
        UpdateWeatherParameters(CurrentWeatherProfile, NewWeatherProfile, 1f);

        // Обновляем профили
        CalculateWeatherProfiles();

        // Обновляем временные рамки
        CalculateTimeWeather();
        OnWeatherTransitionFinished?.Invoke(CurrentWeatherProfile);
    }

    /// <summary>
    /// Сменить погоду плавно по установленному профилю
    /// </summary>
    public void SetNewWeatherWithTransition(WeatherProfile weatherProfile)
    {
        if (weatherProfile == null)
        {
            Debug.LogError("Попытка установить null профиль погоды!");
            return;
        }

        if (CurrentWeatherProfile == null) CurrentWeatherProfile = weatherProfile;
        NewWeatherProfile = weatherProfile;

        StopWeatherTransition();
        _transitionCoroutine = StartCoroutine(WeatherTransitionCoroutine());
    }

    /// <summary>
    /// Остановить текущий погодный переход, если он есть
    /// </summary>
    public void StopWeatherTransition()
    {
        if (_transitionCoroutine == null) return;

        StopCoroutine(_transitionCoroutine);
        _transitionCoroutine = null;
    }

    private IEnumerator WeatherTransitionCoroutine()
    {
        OnWeatherTransitionStarted?.Invoke(NewWeatherProfile);
        TimeSpan startTime = GameTime.Time;
        //Debug.Log($"<color=yellow>Началась смена погоды! Меняем: {CurrentWeatherProfile.weatherIdentifier} на {NewWeatherProfile.weatherIdentifier}. Время начала: {WorldTime.Instance.GetFormattedTime(startTime)}</color>");

        // Процесс перехода
        SetupVFXSystem();
        float t = 0f;
        while (t < 1f)
        {
            TimeSpan passedTime = GameTime.GetPassedTime(startTime);
            t = Mathf.Clamp01((float)(passedTime.TotalSeconds / TransitionDuration.TotalSeconds));
            UpdateWeatherParameters(CurrentWeatherProfile, NewWeatherProfile, t);
            yield return new WaitForEndOfFrame();
        }

        // Обновляем профили
        CalculateWeatherProfiles();

        // Обновляем временные рамки
        CalculateTimeWeather();

        _transitionCoroutine = null;
        OnWeatherTransitionFinished?.Invoke(CurrentWeatherProfile);
    }

    /// <summary>
    /// Погодготовить систему визуальных эффектов
    /// </summary>
    private void SetupVFXSystem()
    {
        Transform VFXTargetTransform = FindAnyObjectByType<Player>()?.transform;
        if (VFXTargetTransform == null) VFXTargetTransform = WeatherVFXSystem.transform;

        WeatherVFXSystem.SpawnVFX(NewWeatherProfile, VFXTargetTransform);
    }

    /// <summary>
    /// Вычислить следующий профиль погоды
    /// </summary>
    private void CalculateWeatherProfiles()
    {
        CurrentWeatherProfile = NewWeatherProfile;
        //Debug.Log($"<color=green>Закончилась смена погоды! Сейчас на улице: {CurrentWeatherProfile.weatherIdentifier}. Текущее время: {GameTime.GetFormattedTime(GameTime.Time)}</color>");

        List<WeatherProfile> availableTransitions = GetAvailableTransitions();
        if (availableTransitions.Count > 0)
        {
            NewWeatherProfile = availableTransitions[UnityEngine.Random.Range(0, availableTransitions.Count)];
            //Debug.Log($"<color=lightblue>Следующей погодой будет: {NewWeatherProfile.weatherIdentifier}</color>");
        }
        else
        {
            NewWeatherProfile = CurrentWeatherProfile;
            //Debug.LogWarning("<color=lightblue>Нет доступных погодных переходов. Оставлен текущий погодный профиль.</color>");
        }
    }

    /// <summary>
    /// Найти все доступные для перехода погодные профили
    /// </summary>
    private List<WeatherProfile> GetAvailableTransitions()
    {
        List<WeatherProfile> availableProfiles = new();

        // Проходим по всем профилям погод сцены, ищем доступные для перехода
        foreach (WeatherProfile profile in AvailableWeatherProfiles)
            if (profile != null && CurrentWeatherProfile.weatherTransitions.HasFlag((WeatherTransitions)profile.weatherIdentifier))
                availableProfiles.Add(profile);

        return availableProfiles;
    }

    /// <summary>
    /// Рассчитать временные рамки погодных условий
    /// </summary>
    private void CalculateTimeWeather()
    {
        TimeStartCurrentWeather = GameTime.Time;
        int lifetimeHours = UnityEngine.Random.Range(CurrentWeatherProfile.minLifetimeHours, CurrentWeatherProfile.maxLifetimeHours + 1);
        TimeEndCurrentWeather = TimeStartCurrentWeather + TimeSpan.FromHours(lifetimeHours);
        //Debug.Log($"<color=lightblue>Текущая погода закончится в: {GameTime.GetFormattedTime(TimeEndCurrentWeather)}, после начнется переход на предсказанную погоду</color>");

        // Запускаем процесс ожидания наступления времени следующего погодного перехода
        if (UseAutoTransition)
        {
            _waitNextTransition = new Process(TimeEndCurrentWeather - TimeStartCurrentWeather,
                () => SetNewWeatherWithTransition(NewWeatherProfile),
                _ => Debug.Log("<color=red>Ожидание перехода погоды прервано</color>"));
            _waitNextTransition.Play();
        }
    }

    private void UpdateWeatherParameters(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        UpdateWeatherIndicators(currentProfile, newProfile, t);
        if (IsLightingSystemsValid)
        {
            SunLight.UpdateLighting(currentProfile, newProfile, t);
            MoonLight.UpdateLighting(currentProfile, newProfile, t);
        }
        if (IsWindSystemValid) WeatherWindSystem.UpdateSystem(currentProfile, newProfile, t);
        if (IsFogSystemValid) WeatherFogSystem.UpdateSystem(currentProfile, newProfile, t);
        if (IsSkyboxSystemValid) WeatherSkyboxSystem.UpdateSystem(currentProfile, newProfile, t);
        if (IsPostProcessSystemValid) WeatherPostProcessSystem.UpdateSystem(currentProfile, newProfile, t);
        if (IsVfxSystemValid) WeatherVFXSystem.UpdateSystem(currentProfile, newProfile, t);
    }

    private void UpdateWeatherIndicators(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        Temperature = Mathf.Lerp(currentProfile.temperature, newProfile.temperature, t);
        Wetness = Mathf.Lerp(currentProfile.wetness, newProfile.wetness, t);
        Toxicity = Mathf.Lerp(currentProfile.toxicity, newProfile.toxicity, t);
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        // Сбрасываем погодные условия для редактора
        SetNewWeatherImmediately(AvailableWeatherProfiles[0]);
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateReferences();
        if (!IsFogSystemValid || !IsLightingSystemsValid || !IsPostProcessSystemValid || !IsSkyboxSystemValid || !IsWindSystemValid || !IsVfxSystemValid)
            FindReferences();
    }

    private void FindReferences()
    {
        if (CurrentWeatherProfile != null)
        {
            Temperature = CurrentWeatherProfile.temperature;
            Wetness = CurrentWeatherProfile.wetness;
            Toxicity = CurrentWeatherProfile.toxicity;
        }

        // Поиск главных модулей в сцене
        WeatherLightingColor[] dynamicLightingColor = FindObjectsByType<WeatherLightingColor>(FindObjectsSortMode.None);
        foreach (var dyn in dynamicLightingColor)
            (SunLight, MoonLight) = dyn.isSun ? (dyn, MoonLight) : (SunLight, dyn);

        WeatherFogSystem = FindFirstObjectByType<WeatherFogSystem>();
        WeatherSkyboxSystem = FindFirstObjectByType<WeatherSkyboxSystem>();
        WeatherWindSystem = FindFirstObjectByType<WeatherWindSystem>();
        WeatherPostProcessSystem = FindFirstObjectByType<WeatherPostProcessSystem>();
        WeatherVFXSystem = FindFirstObjectByType<WeatherVFXSystem>();

        ValidateReferences();
        if (PrefabUtility.IsPartOfPrefabInstance(this))
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }

    public void SetSceneWeatherInEditor()
    {
        Undo.RecordObject(this, "Вручную выставлена погода в редакторе");

        if (CurrentWeatherProfile == null)
        {
            Debug.LogError("<color=red>Попытка установить null профиль погоды! Установи сменяемый профиль в переменной CurrentWeatherProfile</color>");
            return;
        }

        UpdateWeatherParameters(CurrentWeatherProfile, CurrentWeatherProfile, 1f);
    }
#endif
}