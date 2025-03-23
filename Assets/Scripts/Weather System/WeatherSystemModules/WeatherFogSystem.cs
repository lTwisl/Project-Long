using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WeatherFogSystem : MonoBehaviour
{
    [DisableEdit, SerializeField] private bool _isFogValide = false;

    [Header("Названия Render Feature тумана:")]
    [SerializeField] private string _nearFogFeatureName = "FullScreenVolumetricFogNear";
    [SerializeField] private string _farFogFeatureName = "FullScreenVolumetricFogFar";

    [Header("Материалы тумана:")]
    [DisableEdit, SerializeField] private Material _nearFogMaterial;
    [DisableEdit, SerializeField] private Material _farFogMaterial;
    [Space(10)]
    [DisableEdit, SerializeField] private Transform _sunTransform;

    public void ValidateReferences()
    {
#if UNITY_EDITOR
        Undo.RecordObject(this, "Валидация тумана");
        EditorUtility.SetDirty(this);
#endif
        // Ищем ссылки на материалы тумана
        var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

        if (pipeline == null)
        {
            _isFogValide = false;
            return;
        }
        var renderData = pipeline.rendererDataList[0] as UniversalRendererData;

        var fogRendererFeatureNear = renderData.rendererFeatures.Where((f) => f.name == _nearFogFeatureName).FirstOrDefault() as FullScreenPassRendererFeature;
        var fogRendererFeatureFar = renderData.rendererFeatures.Where((f) => f.name == _farFogFeatureName).FirstOrDefault() as FullScreenPassRendererFeature;

        if (fogRendererFeatureNear != null)
            _nearFogMaterial = fogRendererFeatureNear.passMaterial;
        if (fogRendererFeatureFar != null)
            _farFogMaterial = fogRendererFeatureFar.passMaterial;

        // Инициализация трансформа солнца в сцене:
        DynamicLightingColor[] dynamicLightingColors = FindObjectsByType<DynamicLightingColor>(FindObjectsSortMode.None);
        foreach (DynamicLightingColor dyn in dynamicLightingColors)
        {
            if (dyn.isSun)
                _sunTransform = dyn.transform;
        }

        // Проверка ссылок на метериалы:
        if (_nearFogMaterial == null || _farFogMaterial == null)
        {
            _isFogValide = false;
            return;
        }

        // Проверка доступных полей материала:
        bool isValidePropertys;

        // Color
        isValidePropertys = _nearFogMaterial.HasProperty("_Color_Fog");
        isValidePropertys &= _nearFogMaterial.HasProperty("_Impact_Light");
        isValidePropertys &= _nearFogMaterial.HasProperty("_Min_Intensity_Fog");
        // Scattering
        isValidePropertys &= _nearFogMaterial.HasProperty("_Sun_Phase");
        isValidePropertys &= _nearFogMaterial.HasProperty("_Moon_Phase");
        // Fog
        isValidePropertys &= _nearFogMaterial.HasProperty("_Distance");
        isValidePropertys &= _nearFogMaterial.HasProperty("_Height");
        isValidePropertys &= _nearFogMaterial.HasProperty("_Softness_Height");
        isValidePropertys &= _nearFogMaterial.HasProperty("_Transparency");
        isValidePropertys &= _nearFogMaterial.HasProperty("_Sun_Direction");

        // Color
        isValidePropertys &= _farFogMaterial.HasProperty("_Color_Fog");
        isValidePropertys &= _farFogMaterial.HasProperty("_Impact_Light");
        isValidePropertys &= _farFogMaterial.HasProperty("_Min_Intensity_Fog");
        // Scattering
        isValidePropertys &= _farFogMaterial.HasProperty("_Sun_Phase");
        isValidePropertys &= _farFogMaterial.HasProperty("_Moon_Phase");
        // Fog
        isValidePropertys &= _farFogMaterial.HasProperty("_Distance");
        isValidePropertys &= _farFogMaterial.HasProperty("_Height");
        isValidePropertys &= _farFogMaterial.HasProperty("_Softness_Height");
        isValidePropertys &= _farFogMaterial.HasProperty("_Transparency");
        isValidePropertys &= _farFogMaterial.HasProperty("_Sun_Direction");

        _isFogValide = isValidePropertys;
    }

    /// <summary>
    /// Обновить параметры обьемного тумана
    /// </summary>
    /// <param name="currentProfile"></param>
    /// <param name="newProfile"></param>
    /// <param name="t"></param>
    public void UpdateFog(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        if (!_isFogValide)
        {
            Debug.Log("<color=orange>Модуль тумана в сцене неисправен, либо отстутствует. Погода не будет менять туман!</color>");
            return;
        }
        //// Classic Fog
        // Color
        _nearFogMaterial.SetColor("_Color_Fog", Color.Lerp(currentProfile.nearVolumFogMat.GetColor("_Color_Fog"), newProfile.nearVolumFogMat.GetColor("_Color_Fog"), t));
        _nearFogMaterial.SetFloat("_Impact_Light", Mathf.Lerp(currentProfile.nearVolumFogMat.GetFloat("_Impact_Light"), newProfile.nearVolumFogMat.GetFloat("_Impact_Light"), t));
        _nearFogMaterial.SetFloat("_Min_Intensity_Fog", Mathf.Lerp(currentProfile.nearVolumFogMat.GetFloat("_Min_Intensity_Fog"), newProfile.nearVolumFogMat.GetFloat("_Min_Intensity_Fog"), t));
        // Scattering
        _nearFogMaterial.SetFloat("_Sun_Phase", Mathf.Lerp(currentProfile.nearVolumFogMat.GetFloat("_Sun_Phase"), newProfile.nearVolumFogMat.GetFloat("_Sun_Phase"), t));
        _nearFogMaterial.SetFloat("_Moon_Phase", Mathf.Lerp(currentProfile.nearVolumFogMat.GetFloat("_Moon_Phase"), newProfile.nearVolumFogMat.GetFloat("_Moon_Phase"), t));
        // Fog
        _nearFogMaterial.SetFloat("_Distance", Mathf.Lerp(currentProfile.nearVolumFogMat.GetFloat("_Distance"), newProfile.nearVolumFogMat.GetFloat("_Distance"), t));
        _nearFogMaterial.SetFloat("_Height", Mathf.Lerp(currentProfile.nearVolumFogMat.GetFloat("_Height"), newProfile.nearVolumFogMat.GetFloat("_Height"), t));
        _nearFogMaterial.SetFloat("_Softness_Height", Mathf.Lerp(currentProfile.nearVolumFogMat.GetFloat("_Softness_Height"), newProfile.nearVolumFogMat.GetFloat("_Softness_Height"), t));
        _nearFogMaterial.SetFloat("_Transparency", Mathf.Lerp(currentProfile.nearVolumFogMat.GetFloat("_Transparency"), newProfile.nearVolumFogMat.GetFloat("_Transparency"), t));

        //// Far Fog
        // Color
        _farFogMaterial.SetColor("_Color_Fog", Color.Lerp(currentProfile.farVolumFogMat.GetColor("_Color_Fog"), newProfile.farVolumFogMat.GetColor("_Color_Fog"), t));
        _farFogMaterial.SetFloat("_Impact_Light", Mathf.Lerp(currentProfile.farVolumFogMat.GetFloat("_Impact_Light"), newProfile.farVolumFogMat.GetFloat("_Impact_Light"), t));
        _farFogMaterial.SetFloat("_Min_Intensity_Fog", Mathf.Lerp(currentProfile.farVolumFogMat.GetFloat("_Min_Intensity_Fog"), newProfile.farVolumFogMat.GetFloat("_Min_Intensity_Fog"), t));
        // Scattering
        _farFogMaterial.SetFloat("_Sun_Phase", Mathf.Lerp(currentProfile.farVolumFogMat.GetFloat("_Sun_Phase"), newProfile.farVolumFogMat.GetFloat("_Sun_Phase"), t));
        _farFogMaterial.SetFloat("_Moon_Phase", Mathf.Lerp(currentProfile.farVolumFogMat.GetFloat("_Moon_Phase"), newProfile.farVolumFogMat.GetFloat("_Moon_Phase"), t));
        // Fog
        _farFogMaterial.SetFloat("_Distance", Mathf.Lerp(currentProfile.farVolumFogMat.GetFloat("_Distance"), newProfile.farVolumFogMat.GetFloat("_Distance"), t));
        _farFogMaterial.SetFloat("_Height", Mathf.Lerp(currentProfile.farVolumFogMat.GetFloat("_Height"), newProfile.farVolumFogMat.GetFloat("_Height"), t));
        _farFogMaterial.SetFloat("_Softness_Height", Mathf.Lerp(currentProfile.farVolumFogMat.GetFloat("_Softness_Height"), newProfile.farVolumFogMat.GetFloat("_Softness_Height"), t));
        _farFogMaterial.SetFloat("_Transparency", Mathf.Lerp(currentProfile.farVolumFogMat.GetFloat("_Transparency"), newProfile.farVolumFogMat.GetFloat("_Transparency"), t));
    }

    private void Awake()
    {
        ValidateReferences();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    void Update()
    {
        UpdateSunDirection();
    }

    public void UpdateSunDirection()
    {
        if (_sunTransform == null || !_isFogValide) return;

        _nearFogMaterial.SetVector("_Sun_Direction", _sunTransform.transform.forward);
        _farFogMaterial.SetVector("_Sun_Direction", _sunTransform.transform.forward);
    }
}

