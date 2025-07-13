using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WeatherFogSystem : MonoBehaviour, IWeatherSystem
{
    [field: SerializeField, DisableEdit] public bool IsSystemValid { get; set; }

    [Header("- - Fog Render Features names:")]
    [SerializeField] private string _nameAtmosphericFogFeature = NAME_ATMOSPHERIC_FOG_FEATURE;
    [SerializeField] private string _nameVolumetricFogFeature = NAME_VOLUMETRIC_FOG_FEATURE;

    [Header("- - Fog Materials:")]
    [SerializeField, DisableEdit] private Material _atmosphericFogMaterial;
    [SerializeField, DisableEdit] private Material _volumetricFogMaterial;

    private const string NAME_ATMOSPHERIC_FOG_FEATURE = "FullScreenAtmosphericFog";
    private const string NAME_VOLUMETRIC_FOG_FEATURE = "FullScreenVolumetricFog";
    private Material _currMaterial;
    private Material _nextMaterial;

    public void InitializeAndValidateSystem()
    {
        if (!_atmosphericFogMaterial || !_volumetricFogMaterial)
        {
            // 1. Ищем ссылку на UniversalRenderPipelineAsset:
            UniversalRenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (!pipeline)
            {
                IsSystemValid = false;
                Debug.LogWarning("<color=orange>Система WeatherFog невалидна (не найден UniversalRenderPipelineAsset)!</color>");
                return;
            }

            // 2. Ищем ссылку на UniversalRendererData:
            UniversalRendererData renderData = pipeline.rendererDataList[0] as UniversalRendererData;
            if (!renderData)
            {
                IsSystemValid = false;
                Debug.LogWarning("<color=orange>Система WeatherFog невалидна (не найден UniversalRendererData)!</color>");
                return;
            }

            // 3. Ищем ссылки на Fog Render Features:
            FullScreenPassRendererFeature atmosphericFogFeature = renderData.rendererFeatures.Where((f) => f.name == _nameAtmosphericFogFeature).FirstOrDefault() as FullScreenPassRendererFeature;
            FullScreenPassRendererFeature volumetricFogFeature = renderData.rendererFeatures.Where((f) => f.name == _nameVolumetricFogFeature).FirstOrDefault() as FullScreenPassRendererFeature;

            // 4. Кешируем Fog Materials:
            if (volumetricFogFeature) _atmosphericFogMaterial = atmosphericFogFeature.passMaterial;
            if (atmosphericFogFeature) _volumetricFogMaterial = volumetricFogFeature.passMaterial;
        }

        // 5. Проверяем кешированные ссылки Fog Materials:
        if (!_atmosphericFogMaterial || !_volumetricFogMaterial)
        {
            IsSystemValid = false;
            Debug.LogWarning("<color=orange>Система WeatherFog невалидна (не найдены Fog Materials)!</color>");
            return;
        }

        // 5. Проверяем кешированные ссылки Fog Materials:
        IsSystemValid = ShaderFogIDs.IsMaterialHasShaderProperties(_atmosphericFogMaterial);
        IsSystemValid &= ShaderFogIDs.IsMaterialHasShaderProperties(_volumetricFogMaterial);

        if (!IsSystemValid) Debug.LogWarning("<color=orange>Система WeatherFog невалидна (не найдены все необходимые поля Fog Materials)!</color>");
    }

    public void UpdateSystemParameters(WeatherProfile currentProfile, WeatherProfile nextProfile, float t)
    {
        if (!IsSystemValid) return;

        if (!currentProfile.AtmosphericFogMat || !currentProfile.VolumetricFogMat || !nextProfile.AtmosphericFogMat || !nextProfile.VolumetricFogMat)
        {
            IsSystemValid = false;
            Debug.LogWarning($"<color=orange>Система WeatherFog невалидна ({currentProfile.name} или {nextProfile.name} не содержит Fog Materials)!</color>");
            return;
        }

        // Atmospheric Fog
        _currMaterial = currentProfile.AtmosphericFogMat;
        _nextMaterial = nextProfile.AtmosphericFogMat;
        SetFogMaterialPropertys(_atmosphericFogMaterial, _currMaterial, _nextMaterial, t);

        // Volumetric Fog
        _currMaterial = currentProfile.VolumetricFogMat;
        _nextMaterial = nextProfile.VolumetricFogMat;
        SetFogMaterialPropertys(_volumetricFogMaterial, _currMaterial, _nextMaterial, t);
    }

    private void SetFogMaterialPropertys(Material fogMaterial, Material currentMaterial, Material nextMaterial, float t)
    {
        // Colors
        fogMaterial.SetColor(ShaderFogIDs.FogColor, Color.Lerp(currentMaterial.GetColor(ShaderFogIDs.FogColor), nextMaterial.GetColor(ShaderFogIDs.FogColor), t));
        fogMaterial.SetFloat(ShaderFogIDs.LightInfluence, Mathf.Lerp(currentMaterial.GetFloat(ShaderFogIDs.LightInfluence), nextMaterial.GetFloat(ShaderFogIDs.LightInfluence), t));
        fogMaterial.SetFloat(ShaderFogIDs.FogMinIntensity, Mathf.Lerp(currentMaterial.GetFloat(ShaderFogIDs.FogMinIntensity), nextMaterial.GetFloat(ShaderFogIDs.FogMinIntensity), t));
        fogMaterial.SetFloat(ShaderFogIDs.FogTransparency, Mathf.Lerp(currentMaterial.GetFloat(ShaderFogIDs.FogTransparency), nextMaterial.GetFloat(ShaderFogIDs.FogTransparency), t));

        // Distance Mask
        fogMaterial.SetFloat(ShaderFogIDs.FogDistance, Mathf.Lerp(currentMaterial.GetFloat(ShaderFogIDs.FogDistance), nextMaterial.GetFloat(ShaderFogIDs.FogDistance), t));
        fogMaterial.SetFloat(ShaderFogIDs.FogDistanceSoftness, Mathf.Lerp(currentMaterial.GetFloat(ShaderFogIDs.FogDistanceSoftness), nextMaterial.GetFloat(ShaderFogIDs.FogDistanceSoftness), t));
        fogMaterial.SetFloat(ShaderFogIDs.FogOffset, Mathf.Lerp(currentMaterial.GetFloat(ShaderFogIDs.FogOffset), nextMaterial.GetFloat(ShaderFogIDs.FogOffset), t));

        // Height Mask
        fogMaterial.SetFloat(ShaderFogIDs.FogLowerHeight, Mathf.Lerp(currentMaterial.GetFloat(ShaderFogIDs.FogLowerHeight), nextMaterial.GetFloat(ShaderFogIDs.FogLowerHeight), t));
        fogMaterial.SetFloat(ShaderFogIDs.FogUpperHeight, Mathf.Lerp(currentMaterial.GetFloat(ShaderFogIDs.FogUpperHeight), nextMaterial.GetFloat(ShaderFogIDs.FogUpperHeight), t));
        fogMaterial.SetFloat(ShaderFogIDs.FogSoftnessHeight, Mathf.Lerp(currentMaterial.GetFloat(ShaderFogIDs.FogSoftnessHeight), nextMaterial.GetFloat(ShaderFogIDs.FogSoftnessHeight), t));
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_nameAtmosphericFogFeature)) _nameAtmosphericFogFeature = NAME_ATMOSPHERIC_FOG_FEATURE;
        if (string.IsNullOrEmpty(_nameVolumetricFogFeature)) _nameVolumetricFogFeature = NAME_VOLUMETRIC_FOG_FEATURE;

        if (EditorChangeTracker.IsPrefabInstance(this))
        {
            EditorChangeTracker.RegisterUndo(this, "Initialize and Validate Weather Fog System");
            InitializeAndValidateSystem();
            EditorChangeTracker.SetDirty(this);
        }
    }
#endif
}