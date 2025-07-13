using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


public class WeatherSystem : MonoBehaviour
{
    #region ПЕРЕМЕННЫЕ КЛАССА
    public event Action<WeatherProfile> OnWeatherTransitionStarted;
    public event Action<WeatherProfile> OnWeatherTransitionFinished;

    [field: SerializeField] public bool UseAutoTransition { get; private set; } = true;

    [field: Header("Все погодные профили:")]
    [field: SerializeField] public List<WeatherProfile> AvailableWeatherProfiles { get; private set; } = new();


    [field: Header("Текущие погодные профили:")]
    [field: SerializeField] public WeatherProfile CurrentWeatherProfile { get; private set; }
    [field: SerializeField, DisableEdit] public WeatherProfile NextWeatherProfile { get; private set; }


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
    private Coroutine _transitionCoroutine = null;
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
        IsLightingSystemsValid = SunLight && MoonLight;
        IsWindSystemValid = WeatherWindSystem;
        IsFogSystemValid = WeatherFogSystem;
        IsSkyboxSystemValid = WeatherSkyboxSystem;
        IsPostProcessSystemValid = WeatherPostProcessSystem;
        IsVfxSystemValid = WeatherVFXSystem;

        // Выводы для отладки:
        if (AvailableWeatherProfiles.Count == 0) Debug.LogWarning("<color=orange>В сцене не инициализированы профили погоды</color>", this);
        if (!IsLightingSystemsValid) Debug.LogWarning("<color=orange>Потеряны ссылки на источники света</color>", this);
        if (!IsWindSystemValid) Debug.LogWarning("<color=orange>Потеряна ссылка на WeatherWindSystem</color>", this);
        if (!IsFogSystemValid) Debug.LogWarning("<color=orange>Потеряна ссылка на WeatherFogSystem</color>", this);
        if (!IsSkyboxSystemValid) Debug.LogWarning("<color=orange>Потеряна ссылка на WeatherSkyboxSystem</color>", this);
        if (!IsPostProcessSystemValid) Debug.LogWarning("<color=orange>Потеряна ссылка на WeatherPostProcessSystem</color>", this);
        if (!IsVfxSystemValid) Debug.LogWarning("<color=orange>Потеряна ссылка на WeatherVFXSystem</color>", this);
    }

    private void Start()
    {
        ValidateSystems();
        SetWeatherConditionImmediately(CurrentWeatherProfile);
    }

    /// <summary>
    /// Проверка модулей системы погодных условий
    /// </summary>
    public void ValidateSystems()
    {
        SunLight.InitializeAndValidateSystem();
        MoonLight.InitializeAndValidateSystem();
        WeatherWindSystem.InitializeAndValidateSystem();
        WeatherFogSystem.InitializeAndValidateSystem();
        WeatherSkyboxSystem.InitializeAndValidateSystem();
        WeatherPostProcessSystem.InitializeAndValidateSystem();
        WeatherVFXSystem.InitializeAndValidateSystem();
    }

    /// <summary>
    /// Установить новое состояние погоды мгновенно
    /// </summary>
    public void SetWeatherConditionImmediately(WeatherProfile weatherProfile)
    {
        if (!weatherProfile)
        {
            Debug.LogError("<color=red>Попытка установить невалидный профиль погоды отклонена!</color>");
            return;
        }

        // 1. Сначала останавливаем текущий переход
        StopWeatherTransition();

        // 2. Устанавливаем профили погодных состояний
        if (!CurrentWeatherProfile) CurrentWeatherProfile = weatherProfile;
        NextWeatherProfile = weatherProfile;

        OnWeatherTransitionStarted?.Invoke(NextWeatherProfile);

        // 3. Обновляем параметры погодного состояния
        PrepareWeatherVFXSystem(NextWeatherProfile);
        UpdateWeatherParameters(CurrentWeatherProfile, NextWeatherProfile, 1f);

        // 4. Рассчитываем параметры следующего погодного состояния
        SetNextWeatherProfile();
        SetNextWeatherTimeBorders();

        OnWeatherTransitionFinished?.Invoke(CurrentWeatherProfile);
    }

    /// <summary>
    /// Сменить погоду плавно по установленному профилю
    /// </summary>
    public void SetWeatherConditionTransient(WeatherProfile weatherProfile)
    {
        if (!weatherProfile)
        {
            Debug.LogError("<color=red>Попытка установить невалидный профиль погоды отклонена!</color>");
            return;
        }

        // 1. Сначала останавливаем текущий переход
        StopWeatherTransition();

        // 2. Устанавливаем профили погодных состояний
        if (!CurrentWeatherProfile) CurrentWeatherProfile = weatherProfile;
        NextWeatherProfile = weatherProfile;

        // 3. Запускаем корутину перехода погодных состояний
        OnWeatherTransitionStarted?.Invoke(NextWeatherProfile);
        _transitionCoroutine = StartCoroutine(WeatherConditionTransitionCoroutine());
    }

    /// <summary>
    /// Остановить текущий переход погодных состояний, если он запущен
    /// </summary>
    public void StopWeatherTransition()
    {
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }
    }

    private IEnumerator WeatherConditionTransitionCoroutine()
    {
        TimeSpan startTime = GameTime.Time;
        //Debug.Log($"<color=green>Смена погодных условий началась в {GameTime.GetFormattedTime(startTime)}. Меняем: {CurrentWeatherProfile.weatherIdentifier} на {NewWeatherProfile.weatherIdentifier}.</color>");

        // 1. Обновляем параметры погодного состояния
        PrepareWeatherVFXSystem(NextWeatherProfile);
        float t = 0f;
        while (t < 1f)
        {
            TimeSpan passedTime = GameTime.GetPassedTime(startTime);
            t = Mathf.Clamp01((float)(passedTime.TotalSeconds / TransitionDuration.TotalSeconds));
            UpdateWeatherParameters(CurrentWeatherProfile, NextWeatherProfile, t);
            yield return null;
        }

        // 2. Рассчитываем параметры следующего погодного состояния
        SetNextWeatherProfile();
        SetNextWeatherTimeBorders();

        OnWeatherTransitionFinished?.Invoke(CurrentWeatherProfile);
        //Debug.Log($"<color=green>Смена погодных условий закончилась в {GameTime.GetFormattedTime(GameTime.Time)}! Сейчас состояние погоды: {CurrentWeatherProfile.weatherIdentifier}.</color>");
        _transitionCoroutine = null;
    }

    /// <summary>
    /// Погодготовить систему визуальных эффектов погоды
    /// </summary>
    private void PrepareWeatherVFXSystem(WeatherProfile nextProfile)
    {
        if (!IsVfxSystemValid) return;

        WeatherVFXSystem.PreSpawnVFXControllers(nextProfile);
    }

    /// <summary>
    /// Очистить систему визуальных эффектов погоды
    /// </summary>
    private void ClearWeatherVFXSystem()
    {
        if (IsVfxSystemValid) return;

        WeatherVFXSystem.ClearVFXControllers();
    }

    /// <summary>
    /// Вычислить профиль следующего погодного состояния
    /// </summary>
    private void SetNextWeatherProfile()
    {
        // 1. Устанавливаем новый текущий профиль погоды
        CurrentWeatherProfile = NextWeatherProfile;

        // 2. Рассчитываем следующее погодное состояние
        List<WeatherProfile> availableWeatherProfiles = GetAvailableWeatherProfiles();
        if (availableWeatherProfiles.Count > 0)
        {
            NextWeatherProfile = availableWeatherProfiles[UnityEngine.Random.Range(0, availableWeatherProfiles.Count)];
        }
        else
        {
            NextWeatherProfile = CurrentWeatherProfile;
            Debug.Log("<color=orange>У текущей погоды нет перехода в другие погодные состояния. Погода не будет менять состояния!</color>");
        }
    }

    /// <summary>
    /// Получить все профили, доступные для перехода из текущего состояния погоды
    /// </summary>
    private List<WeatherProfile> GetAvailableWeatherProfiles()
    {
        List<WeatherProfile> availableWeatherProfiles = new();

        foreach (WeatherProfile profile in AvailableWeatherProfiles)
            if (profile && CurrentWeatherProfile && CurrentWeatherProfile.Transitions.HasFlag((WeatherTransitions)profile.Identifier))
                availableWeatherProfiles.Add(profile);

        return availableWeatherProfiles;
    }

    /// <summary>
    /// Рассчитать временные рамки следующего погодного условия
    /// </summary>
    private void SetNextWeatherTimeBorders()
    {
        TimeStartCurrentWeather = GameTime.Time;

        // 1. Рассчитываем время существования текущего погодного состояния
        int lifetimeHours = UnityEngine.Random.Range(CurrentWeatherProfile.MinLifetime, CurrentWeatherProfile.MaxLifetime + 1);
        TimeEndCurrentWeather = TimeStartCurrentWeather + TimeSpan.FromHours(lifetimeHours);

        // 2. Если погода должна меняться автоматически - запускаем процесс ожидания времени следующего перехода
        if (UseAutoTransition)
        {
            _waitNextTransition = new Process(TimeEndCurrentWeather - TimeStartCurrentWeather,
                () => SetWeatherConditionTransient(NextWeatherProfile),
                _ => Debug.Log("<color=red>Процесс ожидания следующего перехода состояния погоды прерван!</color>"));
            _waitNextTransition.Play();
        }
    }

    private void UpdateWeatherParameters(WeatherProfile currentWeatherProfile, WeatherProfile nextWeatherProfile, float t)
    {
        UpdateWeatherIndicators(currentWeatherProfile, nextWeatherProfile, t);

        if (IsLightingSystemsValid)
        {
            SunLight.UpdateSystemParameters(currentWeatherProfile, nextWeatherProfile, t);
            MoonLight.UpdateSystemParameters(currentWeatherProfile, nextWeatherProfile, t);
        }
        if (IsWindSystemValid) WeatherWindSystem.UpdateSystemParameters(currentWeatherProfile, nextWeatherProfile, t);
        if (IsFogSystemValid) WeatherFogSystem.UpdateSystemParameters(currentWeatherProfile, nextWeatherProfile, t);
        if (IsSkyboxSystemValid) WeatherSkyboxSystem.UpdateSystemParameters(currentWeatherProfile, nextWeatherProfile, t);
        if (IsPostProcessSystemValid) WeatherPostProcessSystem.UpdateSystemParameters(currentWeatherProfile, nextWeatherProfile, t);
        if (IsVfxSystemValid) WeatherVFXSystem.UpdateSystemParameters(currentWeatherProfile, nextWeatherProfile, t);
    }

    private void UpdateWeatherIndicators(WeatherProfile currentProfile, WeatherProfile nextProfile, float t)
    {
        Temperature = Mathf.Lerp(currentProfile.Temperature, nextProfile.Temperature, t);
        Wetness = Mathf.Lerp(currentProfile.Wetness, nextProfile.Wetness, t);
        Toxicity = Mathf.Lerp(currentProfile.Toxicity, nextProfile.Toxicity, t);
    }

#if UNITY_EDITOR
    private void OnDestroy()
    {
        if (AvailableWeatherProfiles.Count > 0)
            SetWeatherConditionImmediately(AvailableWeatherProfiles[0]);
    }


    private void OnValidate()
    {
        if (!Application.isPlaying)
            FindReferences();
    }

    private void FindReferences()
    {
        if (EditorChangeTracker.IsPrefabInEditMode(this)) return;

        EditorChangeTracker.RegisterUndo(this, "Find Weather System References");

        if (CurrentWeatherProfile)
        {
            Temperature = CurrentWeatherProfile.Temperature;
            Wetness = CurrentWeatherProfile.Wetness;
            Toxicity = CurrentWeatherProfile.Toxicity;
        }

        if (!SunLight || !MoonLight)
        {
            WeatherLightingColor[] weatherLightings = FindObjectsByType<WeatherLightingColor>(FindObjectsSortMode.None);

            SunLight = weatherLightings.FirstOrDefault(wetLight => wetLight?.IsSun == true);
            MoonLight = weatherLightings.FirstOrDefault(wetLight => wetLight?.IsSun == false);
        }
        if (!WeatherFogSystem) WeatherFogSystem = FindFirstObjectByType<WeatherFogSystem>();
        if (!WeatherSkyboxSystem) WeatherSkyboxSystem = FindFirstObjectByType<WeatherSkyboxSystem>();
        if (!WeatherWindSystem) WeatherWindSystem = FindFirstObjectByType<WeatherWindSystem>();
        if (!WeatherPostProcessSystem) WeatherPostProcessSystem = FindFirstObjectByType<WeatherPostProcessSystem>();
        if (!WeatherVFXSystem) WeatherVFXSystem = FindFirstObjectByType<WeatherVFXSystem>();
        ValidateReferences();

        EditorChangeTracker.SetDirty(this);
    }

    public void SetWeatherConditionEditor()
    {
        if (EditorChangeTracker.IsPrefabInEditMode(this)) return;

        if (!CurrentWeatherProfile)
        {
            Debug.LogError("<color=red>Отклонена попытка установить null профиль погоды!</color>");
            return;
        }

        EditorChangeTracker.RegisterUndo(this, $"Set new Weather Profile Editor - {CurrentWeatherProfile.Identifier}");

        ClearWeatherVFXSystem();
        PrepareWeatherVFXSystem(CurrentWeatherProfile);
        UpdateWeatherParameters(CurrentWeatherProfile, CurrentWeatherProfile, 1f);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }
#endif
}