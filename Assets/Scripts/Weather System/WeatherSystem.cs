using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    public event Action<WeatherProfile> OnWeatherTransitionStarted;
    public event Action<WeatherProfile> OnWeatherTransitionCompleted;

    [SerializeField] private bool _useAutoChange = true;

    [field: Header("Список Weather Profiles:")]
    [field: SerializeField] public List<WeatherProfile> WeatherProfiles { get; private set; }


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


    [field: Header("Система ветра:")]
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
    [DisableEdit, SerializeField] public bool _isVFXValide = false;
    [field: SerializeField] public WeatherVFXSystem WeatherVFXSystem { get; private set; }

    public TimeSpan TimeStartCurrentWeather { get; private set; }
    public TimeSpan TimeEndCurrentWeather { get; private set; }
    public TimeSpan TransitionDuration { get; private set; } = new TimeSpan(0, 1, 0, 0);

    private Process waitNextTransition;
    private Coroutine _transitionCoroutine;

    private void Awake()
    {
        ValidateReferences();
    }

    /// <summary>
    /// Проверка модулей погоды и условий для смены
    /// </summary>
    public void ValidateReferences()
    {
        _isProfilesValide = CurrentWeatherProfile != null && NewWeatherProfile != null;
        _isLightingSystemsValide = SunLight != null && MoonLight != null;
        _isWindSystemValide = WindSystem != null;
        _isFogSystemValide = WeatherFogSystem != null;
        _isSkyboxSystemValide = WeatherSkyboxSystem != null;
        _isPostProcessSystemValide = WeatherPostProcessSystem != null;
        _isVFXValide = WeatherVFXSystem != null;

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
        SetNewWeatherImmediately(CurrentWeatherProfile); // Warning! Временное решение, ждем сохранения
    }

    /// <summary>
    /// Сменить погоду мгновенно
    /// </summary>
    public void SetNewWeatherImmediately(WeatherProfile weatherProfile)
    {
        if (weatherProfile == null)
        {
            Debug.LogError("Попытка установить null профиль погоды!");
            return;
        }

        if (CurrentWeatherProfile == null)
            CurrentWeatherProfile = weatherProfile;
        NewWeatherProfile = weatherProfile;

        StopWeatherTransition();
        OnWeatherTransitionStarted?.Invoke(NewWeatherProfile);
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
    public void SetNewWeatherWithTransition(WeatherProfile weatherProfile)
    {
        if (weatherProfile == null)
        {
            Debug.LogError("Попытка установить null профиль погоды!");
            return;
        }

        if (CurrentWeatherProfile == null)
            CurrentWeatherProfile = weatherProfile;
        NewWeatherProfile = weatherProfile;

        StopWeatherTransition();
        _transitionCoroutine = StartCoroutine(WeatherTransitionCoroutine());
    }

    /// <summary>
    /// Остановить текущий погодный переход, если он есть
    /// </summary>
    public void StopWeatherTransition()
    {
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }
    }

    /// <summary>
    /// Есть ли в текущий момент переход погодных условий?
    /// </summary>
    public bool CheckHasTransition()
    {
        if (_transitionCoroutine != null)
            return true;
        else
            return false;
    }

    private IEnumerator WeatherTransitionCoroutine()
    {
        OnWeatherTransitionStarted?.Invoke(NewWeatherProfile);
        TimeSpan startTime = GameTime.Time;
        //Debug.Log($"<color=yellow>Началась смена погоды! Меняем: {CurrentWeatherProfile.weatherIdentifier} на {NewWeatherProfile.weatherIdentifier}. Время начала: {WorldTime.Instance.GetFormattedTime(startTime)}</color>");

        // Процесс перехода
        float t = 0f;
        WeatherVFXSystem.SpawnVFX(NewWeatherProfile, FindAnyObjectByType<Player>().transform);
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
        OnWeatherTransitionCompleted?.Invoke(CurrentWeatherProfile);
    }

    /// <summary>
    /// Вычислить следующий профиль погоды
    /// </summary>
    private void CalculateWeatherProfiles()
    {
        CurrentWeatherProfile = NewWeatherProfile;
        //Debug.Log($"<color=green>Закончилась смена погоды! Сейчас на улице: {CurrentWeatherProfile.weatherIdentifier}. Текущее время: {WorldTime.Instance.GetFormattedTime(WorldTime.Instance.CurrentTime)}</color>");

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
        foreach (WeatherProfile profile in WeatherProfiles)
            if (CurrentWeatherProfile.weatherTransitions.HasFlag((WeatherTransitions)profile.weatherIdentifier))
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
        //Debug.Log($"<color=lightblue>Текущая погода закончится в: {WorldTime.Instance.GetFormattedTime(TimeEndCurrentWeather)}, после начнется переход на предсказанную погоду</color>");

        // Запускаем процесс ожидания перехода погодных условий
        if (_useAutoChange)
        {
            waitNextTransition = new Process(TimeEndCurrentWeather - TimeStartCurrentWeather,
                () => SetNewWeatherWithTransition(NewWeatherProfile),
                _ => Debug.Log("<color=red>Ожидание перехода погоды прервано</color>"));
            waitNextTransition.Play();
        }
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
        Temperature = Mathf.Lerp(currentProfile.temperature, newProfile.temperature, t);
        Wetness = Mathf.Lerp(currentProfile.wetness, newProfile.wetness, t);
        Toxicity = Mathf.Lerp(currentProfile.toxicity, newProfile.toxicity, t);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!_isFogSystemValide || !_isLightingSystemsValide || !_isPostProcessSystemValide || !_isSkyboxSystemValide || !_isWindSystemValide)
            FindReferences();
    }

    private void FindReferences()
    {
        UnityEditor.Undo.RecordObject(this, "Инициализированы ссылки на модули WeatherSystem");

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

    public void SetSceneWeatherInEditor()
    {
        UnityEditor.Undo.RecordObject(this, "Выставлена погода по пресету в редакторе");

        if (CurrentWeatherProfile == null)
        {
            Debug.LogWarning("<color=orange>Попытка установить null профиль погоды! Установи сменяемый профиль в переменной currentProfile</color>");
            return;
        }

        UpdateWeatherParameters(CurrentWeatherProfile, CurrentWeatherProfile, 1f);
    }
#endif
}