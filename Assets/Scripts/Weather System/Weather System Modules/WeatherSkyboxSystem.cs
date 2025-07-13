using UnityEngine;

public class WeatherSkyboxSystem : MonoBehaviour, IWeatherSystem
{
    [field: SerializeField, DisableEdit] public bool IsSystemValid { get; set; }

    [Header("- - Skybox Material:")]
    [SerializeField, DisableEdit] private Material _skyboxMaterial;

    private Material _currMaterial;
    private Material _nextMaterial;

    public void InitializeAndValidateSystem()
    {
        // 1. Кешируем ссылку Skybox Material:
        if (!_skyboxMaterial) _skyboxMaterial = RenderSettings.skybox;

        // 2. Проверяем кешированную ссылку Skybox Material:
        if (!_skyboxMaterial)
        {
            IsSystemValid = false;
            Debug.LogWarning("<color=orange>Система WeatherSkybox невалидна (не найдена ссылка Skybox Material)!</color>");
            return;
        }

        // 3. Проверяем наличие необходимых полей Skybox Material:
        IsSystemValid = ShaderSkyboxIDs.IsMaterialHasShaderProperties(_skyboxMaterial);

        if (!IsSystemValid) Debug.LogWarning("<color=orange>Система WeatherSkybox невалидна (не найдены все необходимые поля Skybox Material)!</color>");
    }

    public void UpdateSystemParameters(WeatherProfile currentProfile, WeatherProfile nextProfile, float t)
    {
        if (!IsSystemValid) return;

        if (!currentProfile.SkyboxMat || !nextProfile.SkyboxMat)
        {
            IsSystemValid = false;
            Debug.LogWarning($"<color=orange>Система WeatherSkybox невалидна ({currentProfile.name} или {nextProfile.name} не содержит Skybox Material)!</color>");
            return;
        }

        _currMaterial = currentProfile.SkyboxMat;
        _nextMaterial = nextProfile.SkyboxMat;
        SetSkyboxMaterialPropertys(_skyboxMaterial, _currMaterial, _nextMaterial, t);
    }

    private void SetSkyboxMaterialPropertys(Material skyboxMaterial, Material currentMaterial, Material nextMaterial, float t)
    {
        // Colors
        skyboxMaterial.SetColor(ShaderSkyboxIDs.ColorZenith, Color.Lerp(currentMaterial.GetColor(ShaderSkyboxIDs.ColorZenith),nextMaterial.GetColor(ShaderSkyboxIDs.ColorZenith), t));
        skyboxMaterial.SetColor(ShaderSkyboxIDs.ColorHorizon, Color.Lerp( currentMaterial.GetColor(ShaderSkyboxIDs.ColorHorizon),nextMaterial.GetColor(ShaderSkyboxIDs.ColorHorizon), t));
        skyboxMaterial.SetColor(ShaderSkyboxIDs.ColorHaze, Color.Lerp(currentMaterial.GetColor(ShaderSkyboxIDs.ColorHaze),nextMaterial.GetColor(ShaderSkyboxIDs.ColorHaze), t));
        skyboxMaterial.SetColor(ShaderSkyboxIDs.ColorGround, Color.Lerp(currentMaterial.GetColor(ShaderSkyboxIDs.ColorGround), nextMaterial.GetColor(ShaderSkyboxIDs.ColorGround), t));
        skyboxMaterial.SetColor(ShaderSkyboxIDs.ColorNight, Color.Lerp(currentMaterial.GetColor(ShaderSkyboxIDs.ColorNight),nextMaterial.GetColor(ShaderSkyboxIDs.ColorNight), t));

        // Colors Blending
        skyboxMaterial.SetFloat(ShaderSkyboxIDs.SoftnessZenithHorizon, Mathf.Lerp(  currentMaterial.GetFloat(ShaderSkyboxIDs.SoftnessZenithHorizon), nextMaterial.GetFloat(ShaderSkyboxIDs.SoftnessZenithHorizon), t));
        skyboxMaterial.SetFloat(ShaderSkyboxIDs.HeightHaze, Mathf.Lerp(currentMaterial.GetFloat(ShaderSkyboxIDs.HeightHaze), nextMaterial.GetFloat(ShaderSkyboxIDs.HeightHaze), t));
        skyboxMaterial.SetFloat(ShaderSkyboxIDs.SoftnessHaze, Mathf.Lerp( currentMaterial.GetFloat(ShaderSkyboxIDs.SoftnessHaze),nextMaterial.GetFloat(ShaderSkyboxIDs.SoftnessHaze), t));
        skyboxMaterial.SetFloat(ShaderSkyboxIDs.SoftnessGround, Mathf.Lerp( currentMaterial.GetFloat(ShaderSkyboxIDs.SoftnessGround),nextMaterial.GetFloat(ShaderSkyboxIDs.SoftnessGround), t));

        // Time of Day Correction
        skyboxMaterial.SetFloat(ShaderSkyboxIDs.SunsetRange, Mathf.Lerp( currentMaterial.GetFloat(ShaderSkyboxIDs.SunsetRange),nextMaterial.GetFloat(ShaderSkyboxIDs.SunsetRange), t));

        // Stars
        skyboxMaterial.SetColor(ShaderSkyboxIDs.ColorStars, Color.Lerp(currentMaterial.GetColor(ShaderSkyboxIDs.ColorStars),nextMaterial.GetColor(ShaderSkyboxIDs.ColorStars), t));
        skyboxMaterial.SetColor(ShaderSkyboxIDs.ColorMoon, Color.Lerp(currentMaterial.GetColor(ShaderSkyboxIDs.ColorMoon),nextMaterial.GetColor(ShaderSkyboxIDs.ColorMoon), t));
        skyboxMaterial.SetFloat(ShaderSkyboxIDs.SpeedFlickingStars, Mathf.Lerp( currentMaterial.GetFloat(ShaderSkyboxIDs.SpeedFlickingStars),nextMaterial.GetFloat(ShaderSkyboxIDs.SpeedFlickingStars), t));
        skyboxMaterial.SetFloat(ShaderSkyboxIDs.HorizonOffsetStars, Mathf.Lerp(currentMaterial.GetFloat(ShaderSkyboxIDs.HorizonOffsetStars), nextMaterial.GetFloat(ShaderSkyboxIDs.HorizonOffsetStars), t));
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (EditorChangeTracker.IsPrefabInstance(this))
        {
            EditorChangeTracker.RegisterUndo(this, "Initialize and Validate Weather Skybox System");
            InitializeAndValidateSystem();
            EditorChangeTracker.SetDirty(this);
        }
    }
#endif
}