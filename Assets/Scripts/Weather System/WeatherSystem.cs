using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[DefaultExecutionOrder(-90)]
public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance { get; private set; }

    public event Action<WeatherProfile> OnWeatherTransitionStarted;
    public event Action<WeatherProfile> OnWeatherTransitionCompleted;

    [field: Header("Список Weather Profiles:")]
    [SerializeField] private List<WeatherProfile> weatherProfiles;

    [field: Header("Используемые Weather Profiles:")]
    [DisableEdit, SerializeField] private bool _isProfilesValide = false;
    [field: SerializeField] public WeatherProfile CurrentWeatherProfile { get; private set; }
    [field: SerializeField] public WeatherProfile NewWeatherProfile { get; private set; }

    [field: Header("Параметры текущей погоды:")]
    [field: DisableEdit, SerializeField] public float Temperature { get; private set; }
    [field: DisableEdit, SerializeField] public float Wetness { get; private set; }
    [field: DisableEdit, SerializeField] public float Toxicity { get; private set; }

    [field: Header("Освещение сцены:")]
    [DisableEdit, SerializeField] private bool _isLightingSystemsValide = false;
    [field: SerializeField] public DynamicLightingColor SunLight { get; private set; }
    [field: SerializeField] public DynamicLightingColor MoonLight { get; private set; }

    [field: Header("Системы ветра:")]
    [DisableEdit, SerializeField] private bool _isWindSystemValide = false;
    [field: SerializeField] public WeatherWindSystem WindSystem { get; private set; }

    [field: Header("Объемный туман:")]
    [DisableEdit, SerializeField] private bool _isFogSystemValide = false;
    [field: SerializeField] public WeatherFogSystem WeatherFogSystem { get; private set; }

    [field: Header("Скайбокс:")]
    [DisableEdit, SerializeField] private bool _isSkyboxSystemValide = false;
    [field: SerializeField] public WeatherSkyboxSystem WeatherSkyboxSystem { get; private set; }

    [field: Header("Пост процессинг:")]
    [DisableEdit, SerializeField] private bool _isPostProcessSystemValide = false;
    [field: SerializeField] public WeatherPostProcessSystem WeatherPostProcessSystem { get; private set; }

    [field: Header("Визуальные эффекты:")]
    [DisableEdit, SerializeField] private bool _isVFXValide = false;
    [field: SerializeField] public WeatherVFXSystem WeatherVFXSystem { get; private set; }


    private Process waitNextTransition;
    private Coroutine _transitionCoroutine;
    private readonly TimeSpan _transitionDuration = new(0, 1, 0, 0); // Время перехода между погодными условиями
    private TimeSpan _timeStartCurrentWeather = new(0, 0, 0, 0); // Время когда текущая погода началась
    private TimeSpan _timeEndCurrentWeather = new(0, 0, 0, 0); // Время когда текущая погода закончится

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            ValidateReferences();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ContextMenu("[Script] Валидировать системы")]
    /// <summary>
    /// Проверка модулей и условий для смены погодных условий
    /// </summary>
    private void ValidateReferences()
    {
        _isProfilesValide = ValidateProfiles();
        _isLightingSystemsValide = SunLight != null && MoonLight != null && RenderSettings.sun == SunLight.GetComponent<Light>();
        _isWindSystemValide = WindSystem != null;
        _isFogSystemValide = WeatherFogSystem != null;
        _isSkyboxSystemValide = WeatherSkyboxSystem != null;
        _isPostProcessSystemValide = WeatherPostProcessSystem != null;
        _isVFXValide = WeatherVFXSystem != null;

        // Выводы для отладки:
        if (!_isProfilesValide) Debug.LogWarning("<color=orange>В сцене не инициализированы профили погоды</color>", this);
        if (!_isLightingSystemsValide) Debug.LogWarning("<color=orange>Потеряна ссылка на WeatherLightingSystems</color>", this);
        if (!_isWindSystemValide) Debug.LogWarning("<color=orange>Потеряна ссылка на WeatherWindSystem</color>", this);
        if (!_isFogSystemValide) Debug.LogWarning("<color=orange>Потеряна ссылка на WeatherFogSystem</color>", this);
        if (!_isSkyboxSystemValide) Debug.LogWarning("<color=orange>Потеряна ссылка на WeatherSkyboxSystem</color>", this);
        if (!_isPostProcessSystemValide) Debug.LogWarning("<color=orange>Потеряна ссылка на WeatherPostProcessSystem</color>", this);
        if (!_isPostProcessSystemValide) Debug.LogWarning("<color=orange>Потеряна ссылка на WeatherVFXSystem</color>", this);
    }

    private bool ValidateProfiles()
    {
        return CurrentWeatherProfile != null && NewWeatherProfile != null;
    }

    private void Start()
    {
        SetNewWeatherImmediately(CurrentWeatherProfile);
    }

    /// <summary>
    /// Сменить погоду мгновенно по установленному профилю
    /// </summary>
    /// <param name="weatherProfile"></param>
    public void SetNewWeatherImmediately(WeatherProfile weatherProfile)
    {
        if (weatherProfile == null)
        {
            Debug.LogError("Попытка установить null профиль погоды!");
            return;
        }
        if (WorldTime.Instance == null)
        {
            Debug.LogError("World Time Instance отсутсвует!");
            return;
        }

        if (CurrentWeatherProfile == null)
            CurrentWeatherProfile = weatherProfile;
        NewWeatherProfile = weatherProfile;

        OnWeatherTransitionStarted?.Invoke(NewWeatherProfile);
        StopWeatherTransition();
        WeatherVFXSystem.SpawnVFX(NewWeatherProfile, FindAnyObjectByType<Player>().transform);
        UpdateWeatherParameters(CurrentWeatherProfile, NewWeatherProfile, 1f);

        // Обновляем профили
        CalculateWeatherProfiles();

        // Обновляем временные рамки
        CalculateTimeWeather();
        OnWeatherTransitionCompleted?.Invoke(CurrentWeatherProfile);
    }

    /// <summary>
    /// Сменить погоду плавно по установленному профилю
    /// </summary>
    /// <param name="weatherProfile"></param>
    public void SetNewWeatherWithTransition(WeatherProfile weatherProfile)
    {
        if (weatherProfile == null)
        {
            Debug.LogError("Попытка установить null профиль погоды!");
            return;
        }
        if (WorldTime.Instance == null)
        {
            Debug.LogError("World Time Instance отсутсвует!");
            return;
        }

        if (CurrentWeatherProfile == null)
            CurrentWeatherProfile = weatherProfile;
        NewWeatherProfile = weatherProfile;

        StopWeatherTransition();
        _transitionCoroutine = StartCoroutine(WeatherTransitionCoroutine());
    }

    public void StopWeatherTransition()
    {
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }
    }

    /// <summary>
    /// Корутина постепенной смены погодных условий
    /// </summary>
    /// <returns></returns>
    private IEnumerator WeatherTransitionCoroutine()
    {
        OnWeatherTransitionStarted?.Invoke(NewWeatherProfile);
        TimeSpan startTime = WorldTime.Instance.CurrentTime;
        //Debug.Log($"<color=yellow>Началась смена погоды! Меняем: {currentWeatherProfile.weatherIdentifier} на {newWeatherProfile.weatherIdentifier}. Время начала: {WorldTime.Instance.GetFormattedTime(startTime)}</color>");

        // Процесс перехода состояний погодных условий
        float t = 0f;
        WeatherVFXSystem.SpawnVFX(NewWeatherProfile, FindAnyObjectByType<Player>().transform);
        while (t < 1f)
        {
            TimeSpan passedTime = WorldTime.Instance.GetPassedTime(startTime);
            t = Mathf.Clamp01((float)(passedTime.TotalSeconds / _transitionDuration.TotalSeconds));
            UpdateWeatherParameters(CurrentWeatherProfile, NewWeatherProfile, t);
            yield return new WaitForEndOfFrame();
        }

        // Обновляем профили
        CalculateWeatherProfiles();

        // Обновляем временные рамки
        CalculateTimeWeather();

        _transitionCoroutine = null;
        OnWeatherTransitionCompleted?.Invoke(CurrentWeatherProfile);
    }

    /// <summary>
    /// Вычислить следующий профиль погоды
    /// </summary>
    private void CalculateWeatherProfiles()
    {
        CurrentWeatherProfile = NewWeatherProfile;
        //Debug.Log($"<color=green>Закончилась смена погоды! Сейчас на улице: {currentWeatherProfile.weatherIdentifier}. Текущее время: {WorldTime.Instance.GetFormattedTime(WorldTime.Instance.CurrentTime)}</color>");

        List<WeatherProfile> availableTransitions = GetAvailableTransitions();
        if (availableTransitions.Count > 0)
        {
            // Выбираем рандомную следующую погоду из доступных
            NewWeatherProfile = availableTransitions[UnityEngine.Random.Range(0, availableTransitions.Count)];
            //Debug.Log($"<color=lightblue>Следующая погода будет: {newWeatherProfile.weatherIdentifier}</color>");
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
    /// <returns></returns>
    private List<WeatherProfile> GetAvailableTransitions()
    {
        List<WeatherProfile> availableProfiles = new List<WeatherProfile>();

        // Проходим по всем профилям погод сцены, ищем доступные для перехода
        foreach (WeatherProfile profile in weatherProfiles)
        {
            if (CurrentWeatherProfile.weatherTransitions.HasFlag((WeatherTransitions)profile.weatherIdentifier))
                availableProfiles.Add(profile);
        }
        return availableProfiles;
    }

    private void CalculateTimeWeather()
    {
        _timeStartCurrentWeather = WorldTime.Instance.CurrentTime;
        int lifetimeHours = UnityEngine.Random.Range(CurrentWeatherProfile.minLifetimeHours, CurrentWeatherProfile.maxLifetimeHours + 1);
        _timeEndCurrentWeather = _timeStartCurrentWeather + TimeSpan.FromHours(lifetimeHours);
        waitNextTransition = new Process(_timeEndCurrentWeather - _timeStartCurrentWeather,
            () => SetNewWeatherWithTransition(NewWeatherProfile),
            _ => Debug.Log($"<color=red>Ожидание перехода погоды прервано</color>"));
        waitNextTransition.Play();
        //Debug.Log($"<color=lightblue>Текущая погода закончится в: {WorldTime.Instance.GetFormattedTime(_timeEndCurrentWeather)}, после начнется переход на предсказанную погоду</color>");
    }

    private void UpdateWeatherParameters(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        ValidateReferences();
        UpdatePlayerInfluencePrameters(currentProfile, newProfile, t);
        if (_isLightingSystemsValide)
        {
            SunLight.UpdateLighting(currentProfile, newProfile, t);
            MoonLight.UpdateLighting(currentProfile, newProfile, t);
        }
        if (_isWindSystemValide) WindSystem.UpdateWind(currentProfile, newProfile, t);
        if (_isFogSystemValide) WeatherFogSystem.UpdateFog(currentProfile, newProfile, t);
        if (_isSkyboxSystemValide) WeatherSkyboxSystem.UpdateSkybox(currentProfile, newProfile, t);
        if (_isPostProcessSystemValide) WeatherPostProcessSystem.UpdatePostProcessing(currentProfile, newProfile, t);
        if (_isVFXValide) WeatherVFXSystem.UpdateVFX(t);
    }

    private void UpdatePlayerInfluencePrameters(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        Temperature = Mathf.Lerp(currentProfile.temperature, NewWeatherProfile.temperature, t);
        Wetness = Mathf.Lerp(currentProfile.wetness, NewWeatherProfile.wetness, t);
        Toxicity = Mathf.Lerp(currentProfile.toxicity, NewWeatherProfile.toxicity, t);
    }

    private void OnDestroy()
    {
        // Убиваем все не завершившие работу корутины
        if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
    }

    #region Функции EDITOR
    // Только для использования в редакторе
    [ContextMenu("[Script] Инициализировать ссылки и передать их в параметры сцены")]
    private void FindReferences()
    {
#if UNITY_EDITOR
        Undo.RecordObject(this, "Инициализировали ссылки WeatherSystem");
        EditorUtility.SetDirty(this);
#endif
        if (CurrentWeatherProfile != null)
        {
            Temperature = CurrentWeatherProfile.temperature;
            Wetness = CurrentWeatherProfile.wetness;
            Toxicity = CurrentWeatherProfile.toxicity;
        }

        // Поиск главных модулей в сцене
        WeatherFogSystem = FindFirstObjectByType<WeatherFogSystem>();
        WeatherSkyboxSystem = FindFirstObjectByType<WeatherSkyboxSystem>();
        WindSystem = FindFirstObjectByType<WeatherWindSystem>();
        WeatherPostProcessSystem = FindFirstObjectByType<WeatherPostProcessSystem>();
        var dynamicLightingColor = FindObjectsByType<DynamicLightingColor>(FindObjectsSortMode.None);
        foreach (var dyn in dynamicLightingColor)
        {
            if (dyn.isSun)
                SunLight = dyn;
            else
                MoonLight = dyn;
        }

        ValidateReferences();
    }

    [ContextMenu("[Script] Сконфигурировать в сцене currentWeather")]
    private void ConfigureWeather()
    {
#if UNITY_EDITOR
        Undo.RecordObject(this, "Инициализировали сцены");
        EditorUtility.SetDirty(this);
#endif
        SetNewWeatherImmediatelyEditor(CurrentWeatherProfile);
    }


    /// <summary>
    /// Сменить погоду мгновенно по установленному профилю
    /// </summary>
    /// <param name="weatherProfile"></param>
    private void SetNewWeatherImmediatelyEditor(WeatherProfile weatherProfile)
    {
        if (weatherProfile == null)
        {
            Debug.LogError("Попытка установить null профиль погоды!");
            return;
        }

        if (CurrentWeatherProfile == null)
            CurrentWeatherProfile = weatherProfile;
        NewWeatherProfile = weatherProfile;

        UpdateWeatherParameters(CurrentWeatherProfile, NewWeatherProfile, 1f);
    }
    #endregion
}