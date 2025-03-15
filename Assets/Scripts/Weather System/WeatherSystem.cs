using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(-90)]
public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance { get; private set; }

    public event Action<WeatherProfile> OnWeatherTransitionStarted;
    public event Action<WeatherProfile> OnWeatherTransitionCompleted;

    [field: Header("Погодный пресет:")]
    [SerializeField] private bool autoChangeWeather = true;
    [field: SerializeField] public WeatherProfile currentWeatherProfile { get; private set; }
    [field: SerializeField] public WeatherProfile newWeatherProfile { get; private set; }
    [SerializeField] private bool _isProfilesValide = false;
    private Coroutine _transitionCoroutine;

    [field: Header("Параметры погоды:")]
    [field: DisableEdit, SerializeField] public float Temperature { get; private set; }
    [field: DisableEdit, SerializeField] public float Wetness { get; private set; }
    [field: DisableEdit, SerializeField] public float Toxicity { get; private set; }

    [field: Header("Освещение сцены:")]
    [field: SerializeField] public DynamicLightingColor sunLight { get; private set; }
    [field: SerializeField] public DynamicLightingColor moonLight { get; private set; }
    [SerializeField] private bool _isLightsValide = false;

    [field: Header("Системы ветра:")]
    [field: SerializeField] public WindSystem windSystem { get; private set; }
    [SerializeField] private bool _isWindSystemValide = false;

    [field: Header("Объемный туман:")]
    [field: SerializeField] public Material UserVolumFogMaterial { get; private set; }
    [field: SerializeField] public Material UserVolumFogMaterialFar { get; private set; }
    [SerializeField] private bool _isFogValide = false;

    [field: Header("Скайбокс:")]
    [field: SerializeField] public Material skyboxMaterial { get; private set; }
    [SerializeField] private bool _isSkyboxValide = false;

    [field: Header("Пост процессинг:")]
    [field: SerializeField] public Volume postProcessingVolume { get; private set; }
    [SerializeField] private bool _isPostProcessValide = false;

    [field: Header("VFX Graphs:")]
    [field: SerializeField] public List<VFXController> CurrentVFXControllers { get; private set; }
    [field: SerializeField] public List<VFXController> NewVFXControllers { get; private set; }
    [Space(16f)]
    [field: Header("All Scene Weather Profiles:")]
    [SerializeField] private List<WeatherProfile> weatherProfiles;

    private Process waitNextTransition;
    private readonly TimeSpan _transitionDuration = new TimeSpan(0, 1, 0, 0); // Время перехода между погодными условиями
    private TimeSpan _timeStartCurrentWeather = new TimeSpan(0, 0, 0, 0); // Время когда текущая погода началась
    private TimeSpan _timeEndCurrentWeather = new TimeSpan(0, 0, 0, 0); // Время когда текущая погода закончится

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

    /// <summary>
    /// Проверка модулей и условий для смены погодных условий
    /// </summary>
    private void ValidateReferences()
    {
        _isProfilesValide = ValidateProfiles();
        _isLightsValide = sunLight != null && moonLight != null && RenderSettings.sun == sunLight.GetComponent<Light>();
        _isWindSystemValide = windSystem != null;
        _isFogValide = UserVolumFogMaterial != null;
        _isSkyboxValide = skyboxMaterial != null && skyboxMaterial == RenderSettings.skybox;
        _isPostProcessValide = postProcessingVolume != null;

        // Выводы для отладки:
        if (!_isProfilesValide) Debug.LogWarning("<color=orange>В сцене не инициализированы профили погоды</color>", this);
        if (!_isLightsValide) Debug.LogWarning("<color=orange>В сцене не инициализированы солнце и луна</color>", this);
        if (!_isWindSystemValide) Debug.LogWarning("<color=orange>В сцене не инициализирована система ветра</color>", this);
        if (!_isFogValide) Debug.LogWarning("<color=orange>В сцене не инициализирован VolumetricFog</color>", this);
        if (!_isSkyboxValide) Debug.LogWarning("<color=orange>В сцене не инициализирован Skybox</color>", this);
        if (!_isPostProcessValide) Debug.LogWarning("<color=orange>В сцене не инициализирован Volume PostProcess</color>", this);
    }

    private bool ValidateProfiles()
    {
        return currentWeatherProfile != null && newWeatherProfile != null;
    }

    private void Start()
    {
        // For Test Only! Стартовое конфигурирование сцены
        SetNewWeatherImmediately(currentWeatherProfile);
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

        if (currentWeatherProfile == null)
            currentWeatherProfile = weatherProfile;
        newWeatherProfile = weatherProfile;

        OnWeatherTransitionStarted?.Invoke(newWeatherProfile);
        StopWeatherTransition();
        SpawnVFX(newWeatherProfile);
        UpdateWeatherParameters(currentWeatherProfile, newWeatherProfile, 1f);

        // Обновляем профили
        CalculateWeatherProfiles();

        // Обновляем временные рамки
        CalculateTimeWeather();
        OnWeatherTransitionCompleted?.Invoke(currentWeatherProfile);
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

        if (currentWeatherProfile == null)
            currentWeatherProfile = weatherProfile;
        newWeatherProfile = weatherProfile;

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
        OnWeatherTransitionStarted?.Invoke(newWeatherProfile);
        TimeSpan startTime = WorldTime.Instance.CurrentTime;
        //Debug.Log($"<color=yellow>Началась смена погоды! Меняем: {currentWeatherProfile.weatherIdentifier} на {newWeatherProfile.weatherIdentifier}. Время начала: {WorldTime.Instance.GetFormattedTime(startTime)}</color>");

        // Процесс перехода состояний погодных условий
        float t = 0f;
        SpawnVFX(newWeatherProfile);
        while (t < 1f)
        {
            TimeSpan passedTime = WorldTime.Instance.GetPassedTime(startTime);
            t = Mathf.Clamp01((float)(passedTime.TotalSeconds / _transitionDuration.TotalSeconds));
            UpdateWeatherParameters(currentWeatherProfile, newWeatherProfile, t);
            yield return new WaitForEndOfFrame();
        }

        // Обновляем профили
        CalculateWeatherProfiles();

        // Обновляем временные рамки
        CalculateTimeWeather();

        _transitionCoroutine = null;
        OnWeatherTransitionCompleted?.Invoke(currentWeatherProfile);
    }

    /// <summary>
    /// Вычислить следующий профиль погоды
    /// </summary>
    private void CalculateWeatherProfiles()
    {
        currentWeatherProfile = newWeatherProfile;
        Debug.Log($"<color=green>Закончилась смена погоды! Сейчас на улице: {currentWeatherProfile.weatherIdentifier}. Текущее время: {WorldTime.Instance.GetFormattedTime(WorldTime.Instance.CurrentTime)}</color>");

        List<WeatherProfile> availableTransitions = GetAvailableTransitions();
        if (availableTransitions.Count > 0)
        {
            // Выбираем рандомную следующую погоду из доступных
            newWeatherProfile = availableTransitions[UnityEngine.Random.Range(0, availableTransitions.Count)];
            //Debug.Log($"<color=lightblue>Следующая погода будет: {newWeatherProfile.weatherIdentifier}</color>");
        }
        else
        {
            newWeatherProfile = currentWeatherProfile;
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
            if (currentWeatherProfile.weatherTransitions.HasFlag((WeatherTransitions)profile.weatherIdentifier))
                availableProfiles.Add(profile);
        }
        return availableProfiles;
    }

    private void CalculateTimeWeather()
    {
        _timeStartCurrentWeather = WorldTime.Instance.CurrentTime;
        int lifetimeHours = UnityEngine.Random.Range(currentWeatherProfile.minLifetimeHours, currentWeatherProfile.maxLifetimeHours + 1);
        _timeEndCurrentWeather = _timeStartCurrentWeather + TimeSpan.FromHours(lifetimeHours);
        waitNextTransition = new Process(_timeEndCurrentWeather - _timeStartCurrentWeather, () => SetNewWeatherWithTransition(newWeatherProfile), _ => Debug.Log($"<color=red>Ожидание перехода погоды прервано</color>"));
        waitNextTransition.Play();
        //Debug.Log($"<color=lightblue>Текущая погода закончится в: {WorldTime.Instance.GetFormattedTime(_timeEndCurrentWeather)}, после начнется переход на предсказанную погоду</color>");
    }

    private void UpdateWeatherParameters(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        ValidateReferences();
        if (_isLightsValide) UpdateLighting(currentProfile, newProfile, t);
        if (_isWindSystemValide) UpdateWind(currentProfile, newProfile, t);
        if (_isFogValide) UpdateFog(currentProfile, newProfile, t);
        if (_isSkyboxValide) UpdateSkybox(currentProfile, newProfile, t);
        if (_isPostProcessValide) UpdatePostProcessing(currentProfile, newProfile, t);
        UpdatePlayerInfluencePrameters(currentProfile, newProfile, t);
        UpdateVFX(t);
    }

    #region ФУНКЦИИ СМЕНЫ ПАРАМЕТРОВ ОБЬЕКТОВ
    /// <summary>
    /// Обновить параметры системы освещения
    /// </summary>
    /// <param name="currentProfile"></param>
    /// <param name="newProfile"></param>
    /// <param name="t"></param>
    private void UpdateLighting(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        // Солнце
        sunLight.MaxIntensity = Mathf.Lerp(currentProfile.maxIntensitySun, newProfile.maxIntensitySun, t);
        sunLight.Temperature = Mathf.Lerp(currentProfile.temperatureSun, newProfile.temperatureSun, t);
        sunLight.colorAtZenith = Color.Lerp(currentProfile.colorZenithSun, newProfile.colorZenithSun, t);
        sunLight.colorAtSunset = Color.Lerp(currentProfile.colorSunsetSun, newProfile.colorSunsetSun, t);

        // Луна
        moonLight.MaxIntensity = Mathf.Lerp(currentProfile.maxIntensityMoon, newProfile.maxIntensityMoon, t);
        moonLight.colorAtZenith = Color.Lerp(currentProfile.colorZenithMoon, newProfile.colorZenithMoon, t);
        moonLight.colorAtSunset = Color.Lerp(currentProfile.colorSunsetMoon, newProfile.colorSunsetMoon, t);
    }

    /// <summary>
    /// Обновить параметры системы ветра
    /// </summary>
    /// <param name="currentProfile"></param>
    /// <param name="newProfile"></param>
    /// <param name="t"></param>
    private void UpdateWind(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        float minSpeed = Mathf.Lerp(currentProfile.minSpeedWind, newProfile.minSpeedWind, t);
        float maxSpeed = Mathf.Lerp(currentProfile.maxSpeedWind, newProfile.maxSpeedWind, t);
        float speedChange = Mathf.Lerp(currentProfile.speedChangeWind, newProfile.speedChangeWind, t);
        float directionNoiseScale = Mathf.Lerp(currentProfile.directionNoiseScaleWind, newProfile.directionNoiseScaleWind, t);
        windSystem.InitializeSystemParameters(minSpeed, maxSpeed, speedChange, directionNoiseScale, directionNoiseScale);
    }

    /// <summary>
    /// Обновить параметры обьемного тумана
    /// </summary>
    /// <param name="currentProfile"></param>
    /// <param name="newProfile"></param>
    /// <param name="t"></param>
    private void UpdateFog(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        try
        {
            //// Classic Fog
            // Color
            UserVolumFogMaterial.SetColor("_Color_Fog", Color.Lerp(currentProfile.materialNearVolumFog.GetColor("_Color_Fog"), newProfile.materialNearVolumFog.GetColor("_Color_Fog"), t));
            UserVolumFogMaterial.SetFloat("_Impact_Light", Mathf.Lerp(currentProfile.materialNearVolumFog.GetFloat("_Impact_Light"), newProfile.materialNearVolumFog.GetFloat("_Impact_Light"), t));
            UserVolumFogMaterial.SetFloat("_Min_Intensity_Fog", Mathf.Lerp(currentProfile.materialNearVolumFog.GetFloat("_Min_Intensity_Fog"), newProfile.materialNearVolumFog.GetFloat("_Min_Intensity_Fog"), t));
            // Scattering
            UserVolumFogMaterial.SetFloat("_Sun_Phase", Mathf.Lerp(currentProfile.materialNearVolumFog.GetFloat("_Sun_Phase"), newProfile.materialNearVolumFog.GetFloat("_Sun_Phase"), t));
            UserVolumFogMaterial.SetFloat("_Moon_Phase", Mathf.Lerp(currentProfile.materialNearVolumFog.GetFloat("_Moon_Phase"), newProfile.materialNearVolumFog.GetFloat("_Moon_Phase"), t));
            // Fog
            UserVolumFogMaterial.SetFloat("_Distance", Mathf.Lerp(currentProfile.materialNearVolumFog.GetFloat("_Distance"), newProfile.materialNearVolumFog.GetFloat("_Distance"), t));
            UserVolumFogMaterial.SetFloat("_Height", Mathf.Lerp(currentProfile.materialNearVolumFog.GetFloat("_Height"), newProfile.materialNearVolumFog.GetFloat("_Height"), t));
            UserVolumFogMaterial.SetFloat("_Softness_Height", Mathf.Lerp(currentProfile.materialNearVolumFog.GetFloat("_Softness_Height"), newProfile.materialNearVolumFog.GetFloat("_Softness_Height"), t));
            UserVolumFogMaterial.SetFloat("_Transparency", Mathf.Lerp(currentProfile.materialNearVolumFog.GetFloat("_Transparency"), newProfile.materialNearVolumFog.GetFloat("_Transparency"), t));

            //// Far Fog
            // Color
            UserVolumFogMaterialFar.SetColor("_Color_Fog", Color.Lerp(currentProfile.materialFarVolumFog.GetColor("_Color_Fog"), newProfile.materialFarVolumFog.GetColor("_Color_Fog"), t));
            UserVolumFogMaterialFar.SetFloat("_Min_Intensity_Fog", Mathf.Lerp(currentProfile.materialFarVolumFog.GetFloat("_Min_Intensity_Fog"), newProfile.materialFarVolumFog.GetFloat("_Min_Intensity_Fog"), t));
            UserVolumFogMaterialFar.SetFloat("_Min_Intensity_Fog", Mathf.Lerp(currentProfile.materialFarVolumFog.GetFloat("_Min_Intensity_Fog"), newProfile.materialFarVolumFog.GetFloat("_Min_Intensity_Fog"), t));
            // Scattering
            UserVolumFogMaterialFar.SetFloat("_Sun_Phase", Mathf.Lerp(currentProfile.materialFarVolumFog.GetFloat("_Sun_Phase"), newProfile.materialFarVolumFog.GetFloat("_Sun_Phase"), t));
            UserVolumFogMaterialFar.SetFloat("_Moon_Phase", Mathf.Lerp(currentProfile.materialFarVolumFog.GetFloat("_Moon_Phase"), newProfile.materialFarVolumFog.GetFloat("_Moon_Phase"), t));
            // Fog
            UserVolumFogMaterialFar.SetFloat("_Distance", Mathf.Lerp(currentProfile.materialFarVolumFog.GetFloat("_Distance"), newProfile.materialFarVolumFog.GetFloat("_Distance"), t));
            UserVolumFogMaterialFar.SetFloat("_Height", Mathf.Lerp(currentProfile.materialFarVolumFog.GetFloat("_Height"), newProfile.materialFarVolumFog.GetFloat("_Height"), t));
            UserVolumFogMaterialFar.SetFloat("_Softness_Height", Mathf.Lerp(currentProfile.materialFarVolumFog.GetFloat("_Softness_Height"), newProfile.materialFarVolumFog.GetFloat("_Softness_Height"), t));
            UserVolumFogMaterialFar.SetFloat("_Transparency", Mathf.Lerp(currentProfile.materialFarVolumFog.GetFloat("_Transparency"), newProfile.materialFarVolumFog.GetFloat("_Transparency"), t));
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    /// <summary>
    /// Обновить параметры скайбокса
    /// </summary>
    /// <param name="currentProfile"></param>
    /// <param name="newProfile"></param>
    /// <param name="t"></param>
    private void UpdateSkybox(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        try
        {
            // Colors
            skyboxMaterial.SetColor("_Color_On_Zenith", Color.Lerp(currentProfile.materialSkybox.GetColor("_Color_On_Zenith"), newProfile.materialSkybox.GetColor("_Color_On_Zenith"), t));
            skyboxMaterial.SetColor("_Color_On_Horizon", Color.Lerp(currentProfile.materialSkybox.GetColor("_Color_On_Horizon"), newProfile.materialSkybox.GetColor("_Color_On_Horizon"), t));
            skyboxMaterial.SetFloat("_Softness_Gradient_Mask", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Softness_Gradient_Mask"), newProfile.materialSkybox.GetFloat("_Softness_Gradient_Mask"), t));
            skyboxMaterial.SetColor("_Color_On_Skyline", Color.Lerp(currentProfile.materialSkybox.GetColor("_Color_On_Skyline"), newProfile.materialSkybox.GetColor("_Color_On_Skyline"), t));
            skyboxMaterial.SetFloat("_Softness_Skyline_Mask", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Softness_Skyline_Mask"), newProfile.materialSkybox.GetFloat("_Softness_Skyline_Mask"), t));
            skyboxMaterial.SetFloat("_Range_Skyline_Mask", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Range_Skyline_Mask"), newProfile.materialSkybox.GetFloat("_Range_Skyline_Mask"), t));
            skyboxMaterial.SetColor("_Color_On_Ground", Color.Lerp(currentProfile.materialSkybox.GetColor("_Color_On_Ground"), newProfile.materialSkybox.GetColor("_Color_On_Ground"), t));
            skyboxMaterial.SetFloat("_Softness_Ground_Mask", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Softness_Ground_Mask"), newProfile.materialSkybox.GetFloat("_Softness_Ground_Mask"), t));
            // HeyneyGreenstein Scattering
            skyboxMaterial.SetFloat("_Sun_Phase_In", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Sun_Phase_In"), newProfile.materialSkybox.GetFloat("_Sun_Phase_In"), t));
            skyboxMaterial.SetFloat("_Sun_Phase_Out", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Sun_Phase_Out"), newProfile.materialSkybox.GetFloat("_Sun_Phase_Out"), t));
            skyboxMaterial.SetFloat("_Moon_Phase", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Moon_Phase"), newProfile.materialSkybox.GetFloat("_Moon_Phase"), t));
            // Rayleight Scattering
            skyboxMaterial.SetFloat("_Height_Atmosphere", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Height_Atmosphere"), newProfile.materialSkybox.GetFloat("_Height_Atmosphere"), t));
            // Stars
            skyboxMaterial.SetColor("_Color_Stars", Color.Lerp(currentProfile.materialSkybox.GetColor("_Color_Stars"), newProfile.materialSkybox.GetColor("_Color_Stars"), t));
            skyboxMaterial.SetFloat("_Horizon_Offset", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Horizon_Offset"), newProfile.materialSkybox.GetFloat("_Horizon_Offset"), t));
            skyboxMaterial.SetFloat("_Scale_Flick_Noise", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Scale_Flick_Noise"), newProfile.materialSkybox.GetFloat("_Scale_Flick_Noise"), t));
            skyboxMaterial.SetFloat("_Speed_Flick_Noise", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Speed_Flick_Noise"), newProfile.materialSkybox.GetFloat("_Speed_Flick_Noise"), t));
            // Moon
            skyboxMaterial.SetColor("_Moon_Color", Color.Lerp(currentProfile.materialSkybox.GetColor("_Moon_Color"), newProfile.materialSkybox.GetColor("_Moon_Color"), t));
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    private void UpdatePostProcessing(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        if (postProcessingVolume.profile.TryGet<ColorAdjustments>(out var colorAdjustments))
        {
            colorAdjustments.postExposure.value = Mathf.Lerp(currentProfile.postExposure, newProfile.postExposure, t);
            colorAdjustments.contrast.value = Mathf.Lerp(currentProfile.constrast, newProfile.constrast, t);
            colorAdjustments.saturation.value = Mathf.Lerp(currentProfile.saturation, newProfile.saturation, t);
        }
    }

    private void UpdatePlayerInfluencePrameters(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        Temperature = Mathf.Lerp(currentProfile.temperature, newWeatherProfile.temperature, t);
        Wetness = Mathf.Lerp(currentProfile.wetness, newWeatherProfile.wetness, t);
        Toxicity = Mathf.Lerp(currentProfile.toxicity, newWeatherProfile.toxicity, t);
    }


    private void SpawnVFX(WeatherProfile newProfile)
    {
        foreach (GameObject vfx in newProfile.VFX)
        {
            if (vfx == null) continue;
            NewVFXControllers.Add(Instantiate(vfx, FindAnyObjectByType<Player>().gameObject.transform.position, Quaternion.identity, FindAnyObjectByType<Player>().gameObject.transform).GetComponent<VFXController>());
        }
    }

    private void UpdateVFX(float t)
    {
        // Заставляем исчезнуть старые эффекты
        foreach (VFXController vfx in CurrentVFXControllers)
        {
            vfx.SetVFXParameter("t", 1f - t);
        }
        // Заставляем появиться новые эффекты
        foreach (VFXController vfx in NewVFXControllers)
        {
            vfx.SetVFXParameter("t", t);
        }

        if (t >= 1)
        {
            // Заставляем исчезнуть старые эффекты
            foreach (VFXController vfx in CurrentVFXControllers)
            {
                vfx.DestroyVFX();
            }
            CurrentVFXControllers.Clear();
            // Заставляем появиться новые эффекты
            foreach (VFXController vfx in NewVFXControllers)
            {
                CurrentVFXControllers.Add(vfx);
            }
            NewVFXControllers.Clear();
        }
    }
    #endregion

    private void OnDestroy()
    {
        // Убиваем все не завершившие работу корутины
        if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
    }

    #region EDITOR
    // Только для использования в редакторе
    [ContextMenu("[Script] Инициализировать ссылки и передать их в параметры сцены")]
    private void FindReferences()
    {
#if UNITY_EDITOR
        Undo.RecordObject(this, "Инициализировали ссылки WeatherSystem");
        EditorUtility.SetDirty(this);
#endif
        var dynamicLightingColor = FindObjectsByType<DynamicLightingColor>(FindObjectsSortMode.InstanceID);
        foreach (var dyn in dynamicLightingColor)
        {
            if (dyn.isSun)
                sunLight = dyn;
            else
                moonLight = dyn;
        }
        if (sunLight != null)
            RenderSettings.sun = sunLight.GetComponent<Light>();

        if (currentWeatherProfile != null)
        {
            Temperature = currentWeatherProfile.temperature;
            Wetness = currentWeatherProfile.wetness;
            Toxicity = currentWeatherProfile.toxicity;
        }

        windSystem = FindFirstObjectByType<WindSystem>();
        postProcessingVolume = FindFirstObjectByType<Volume>();
        skyboxMaterial = RenderSettings.skybox;

        ValidateReferences();
    }

    [ContextMenu("[Script] Сконфигурировать в сцене currentWeather")]
    private void ConfigureWeather()
    {
        SetNewWeatherImmediatelyEditor(currentWeatherProfile);
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

        if (currentWeatherProfile == null)
            currentWeatherProfile = weatherProfile;
        newWeatherProfile = weatherProfile;

        UpdateWeatherParameters(currentWeatherProfile, newWeatherProfile, 1f);
    }
    #endregion
}