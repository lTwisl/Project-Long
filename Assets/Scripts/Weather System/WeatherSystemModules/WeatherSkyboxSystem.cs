using UnityEditor;
using UnityEngine;

public class WeatherSkyboxSystem : MonoBehaviour, IWeatherSystem
{
    [SerializeField, DisableEdit] private bool _isSystemValid;
    public bool IsSystemValid => _isSystemValid;

    [Header("- - Материал скайбокса:")]
    [SerializeField, DisableEdit] private Material _skyboxMaterial;
    [Space(10)]
    [SerializeField, DisableEdit] private Transform _sunTransform;

    public void InitializeAndValidateSystem()
    {
        // 1. Инициализируем ссылку на материал скайбокса:
        _skyboxMaterial = RenderSettings.skybox;

        // 2. Проверяем найденную ссылку на метериал:
        if (!_skyboxMaterial)
        {
            _isSystemValid = false;
            return;
        }

        // 3. Проверяем наличие необходимых полей материала:
        _isSystemValid = ShaderSkyboxIDs.IsPropertiesValid(_skyboxMaterial);

        if (!IsSystemValid)
        {
            Debug.LogWarning("<color=orange>Не удалось найти все необходимые параметры шейдера скайбокса!</color>");
            return;
        }

        // 4. Кешируем ссылку на источник света(солнце) в сцене
        WeatherLightingColor[] weatherLightColors = FindObjectsByType<WeatherLightingColor>(FindObjectsSortMode.None);
        foreach (WeatherLightingColor weatherLightColor in weatherLightColors)
        {
            if (weatherLightColor.IsSun)
            {
                _sunTransform = weatherLightColor.transform;
                break;
            }
        }
    }

    public void UpdateSystemParameters(WeatherProfile currentProfile, WeatherProfile nextProfile, float t)
    {
        if (!IsSystemValid)
        {
            Debug.LogWarning("<color=orange>Модуль скайбокса неисправен. Система погоды не будет управлять скайбоксом!</color>");
            return;
        }

        if (!currentProfile.SkyboxMat || !nextProfile.SkyboxMat)
        {
            _isSystemValid = false;
            Debug.LogWarning($"<color=orange>Погодный профиль {currentProfile.name} или {nextProfile.name} не содержит ссылку на материал скайбокса. Погода не будет управлять скайбоксом!</color>");
            return;
        }

        // Colors
        _skyboxMaterial.SetColor(ShaderSkyboxIDs.ColorZenith, Color.Lerp(
            currentProfile.SkyboxMat.GetColor(ShaderSkyboxIDs.ColorZenith),
            nextProfile.SkyboxMat.GetColor(ShaderSkyboxIDs.ColorZenith), t));

        _skyboxMaterial.SetColor(ShaderSkyboxIDs.ColorHorizon, Color.Lerp(
            currentProfile.SkyboxMat.GetColor(ShaderSkyboxIDs.ColorHorizon),
            nextProfile.SkyboxMat.GetColor(ShaderSkyboxIDs.ColorHorizon), t));

        _skyboxMaterial.SetColor(ShaderSkyboxIDs.ColorHaze, Color.Lerp(
            currentProfile.SkyboxMat.GetColor(ShaderSkyboxIDs.ColorHaze),
            nextProfile.SkyboxMat.GetColor(ShaderSkyboxIDs.ColorHaze), t));

        _skyboxMaterial.SetColor(ShaderSkyboxIDs.ColorGround, Color.Lerp(
            currentProfile.SkyboxMat.GetColor(ShaderSkyboxIDs.ColorGround),
            nextProfile.SkyboxMat.GetColor(ShaderSkyboxIDs.ColorGround), t));

        _skyboxMaterial.SetColor(ShaderSkyboxIDs.ColorNight, Color.Lerp(
            currentProfile.SkyboxMat.GetColor(ShaderSkyboxIDs.ColorNight),
            nextProfile.SkyboxMat.GetColor(ShaderSkyboxIDs.ColorNight), t));

        // Colors Blending
        _skyboxMaterial.SetFloat(ShaderSkyboxIDs.SoftnessZenithHorizon, Mathf.Lerp(
            currentProfile.SkyboxMat.GetFloat(ShaderSkyboxIDs.SoftnessZenithHorizon),
            nextProfile.SkyboxMat.GetFloat(ShaderSkyboxIDs.SoftnessZenithHorizon), t));

        _skyboxMaterial.SetFloat(ShaderSkyboxIDs.HeightHaze, Mathf.Lerp(
            currentProfile.SkyboxMat.GetFloat(ShaderSkyboxIDs.HeightHaze),
            nextProfile.SkyboxMat.GetFloat(ShaderSkyboxIDs.HeightHaze), t));

        _skyboxMaterial.SetFloat(ShaderSkyboxIDs.SoftnessHaze, Mathf.Lerp(
            currentProfile.SkyboxMat.GetFloat(ShaderSkyboxIDs.SoftnessHaze),
            nextProfile.SkyboxMat.GetFloat(ShaderSkyboxIDs.SoftnessHaze), t));

        _skyboxMaterial.SetFloat(ShaderSkyboxIDs.SoftnessGround, Mathf.Lerp(
            currentProfile.SkyboxMat.GetFloat(ShaderSkyboxIDs.SoftnessGround),
            nextProfile.SkyboxMat.GetFloat(ShaderSkyboxIDs.SoftnessGround), t));

        // Time of Day Correction
        _skyboxMaterial.SetFloat(ShaderSkyboxIDs.SunsetRange, Mathf.Lerp(
            currentProfile.SkyboxMat.GetFloat(ShaderSkyboxIDs.SunsetRange),
            nextProfile.SkyboxMat.GetFloat(ShaderSkyboxIDs.SunsetRange), t));

        // Heyney-Greenstein Scattering
        _skyboxMaterial.SetFloat(ShaderSkyboxIDs.MoonOuterPhase, Mathf.Lerp(
            currentProfile.SkyboxMat.GetFloat(ShaderSkyboxIDs.MoonOuterPhase),
            nextProfile.SkyboxMat.GetFloat(ShaderSkyboxIDs.MoonOuterPhase), t));

        // Stars
        _skyboxMaterial.SetColor(ShaderSkyboxIDs.ColorStars, Color.Lerp(
            currentProfile.SkyboxMat.GetColor(ShaderSkyboxIDs.ColorStars),
            nextProfile.SkyboxMat.GetColor(ShaderSkyboxIDs.ColorStars), t));

        _skyboxMaterial.SetFloat(ShaderSkyboxIDs.SpeedFlickingStars, Mathf.Lerp(
            currentProfile.SkyboxMat.GetFloat(ShaderSkyboxIDs.SpeedFlickingStars),
            nextProfile.SkyboxMat.GetFloat(ShaderSkyboxIDs.SpeedFlickingStars), t));

        _skyboxMaterial.SetFloat(ShaderSkyboxIDs.HorizonOffsetStars, Mathf.Lerp(
            currentProfile.SkyboxMat.GetFloat(ShaderSkyboxIDs.HorizonOffsetStars),
            nextProfile.SkyboxMat.GetFloat(ShaderSkyboxIDs.HorizonOffsetStars), t));
    }

    private void Update()
    {
        UpdateMaterialsSunDirection();
    }

    public void UpdateMaterialsSunDirection()
    {
        if (!IsSystemValid) return;

        if (!_sunTransform)
        {
            Debug.LogWarning("<color=orange>Не удалось взять направление солнца или для материала скайбокса, направление солнца не передается</color>");
            return;
        }

        _skyboxMaterial.SetVector(ShaderSkyboxIDs.SunDirection, _sunTransform.transform.forward);
    }

#if UNITY_EDITOR
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
#endif
}