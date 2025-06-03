using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WeatherFogSystem : MonoBehaviour, IWeatherSystem
{
    [SerializeField, DisableEdit] private bool _isSystemValid;
    public bool IsSystemValid => _isSystemValid;

    [Header("- - �������� Render Features ������:")]
    [SerializeField] private string _nameNearFogFeature = "FullScreenVolumetricFogNear";
    [SerializeField] private string _nameFarFogFeature = "FullScreenVolumetricFogFar";

    [Header("- - ��������� ������:")]
    [SerializeField, DisableEdit] private Material _nearFogMaterial;
    [SerializeField, DisableEdit] private Material _farFogMaterial;
    [Space(10)]
    [SerializeField, DisableEdit] private Transform _sunTransform;

    public void InitializeAndValidateSystem()
    {
        // 1. �������������� ������ �� ��������� ������ �� �� RenderFeature:
        var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (pipeline == null)
        {
            _isSystemValid = false;
            return;
        }

        var renderData = pipeline.rendererDataList[0] as UniversalRendererData;

        var fogRendererFeatureNear = renderData.rendererFeatures.Where((f) => f.name == _nameNearFogFeature).FirstOrDefault() as FullScreenPassRendererFeature;
        var fogRendererFeatureFar = renderData.rendererFeatures.Where((f) => f.name == _nameFarFogFeature).FirstOrDefault() as FullScreenPassRendererFeature;

        if (fogRendererFeatureNear != null) _nearFogMaterial = fogRendererFeatureNear.passMaterial;
        if (fogRendererFeatureFar != null) _farFogMaterial = fogRendererFeatureFar.passMaterial;

        // 2. ��������� ��������� ������ �� ���������:
        if (_nearFogMaterial == null || _farFogMaterial == null)
        {
            _isSystemValid = false;
            return;
        }

        // 3. ��������� ������� ����������� ����� ����������:
        bool isPropertysValid;

        // - - Near Volumetric Fog
        // Color
        isPropertysValid = _nearFogMaterial.HasProperty("_Color_Fog");
        isPropertysValid &= _nearFogMaterial.HasProperty("_Impact_Light");
        isPropertysValid &= _nearFogMaterial.HasProperty("_Min_Intensity_Fog");
        // Scattering
        isPropertysValid &= _nearFogMaterial.HasProperty("_Sun_Phase");
        isPropertysValid &= _nearFogMaterial.HasProperty("_Moon_Phase");
        // Fog
        isPropertysValid &= _nearFogMaterial.HasProperty("_Distance");
        isPropertysValid &= _nearFogMaterial.HasProperty("_Height");
        isPropertysValid &= _nearFogMaterial.HasProperty("_Softness_Height");
        isPropertysValid &= _nearFogMaterial.HasProperty("_Transparency");
        isPropertysValid &= _nearFogMaterial.HasProperty("_Sun_Direction");

        // - - Far Volumetric Fog
        // Color
        isPropertysValid &= _farFogMaterial.HasProperty("_Color_Fog");
        isPropertysValid &= _farFogMaterial.HasProperty("_Impact_Light");
        isPropertysValid &= _farFogMaterial.HasProperty("_Min_Intensity_Fog");
        // Scattering
        isPropertysValid &= _farFogMaterial.HasProperty("_Sun_Phase");
        isPropertysValid &= _farFogMaterial.HasProperty("_Moon_Phase");
        // Fog
        isPropertysValid &= _farFogMaterial.HasProperty("_Distance");
        isPropertysValid &= _farFogMaterial.HasProperty("_Height");
        isPropertysValid &= _farFogMaterial.HasProperty("_Softness_Height");
        isPropertysValid &= _farFogMaterial.HasProperty("_Transparency");
        isPropertysValid &= _farFogMaterial.HasProperty("_Sun_Direction");

        _isSystemValid = isPropertysValid;

        if (!IsSystemValid) return;

        // 4. �������� ������ �� �������� �����(������) � �����
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

    public void UpdateSystemParameters(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        if (!IsSystemValid)
        {
            Debug.LogWarning("<color=orange>������ ������ � ����� ����������. ������ �� ����� ��������� �������!</color>");
            return;
        }

        if (!newProfile.NearVolumFogMat || !newProfile.FarVolumFogMat)
        {
            _isSystemValid = false;
            Debug.LogWarning($"<color=orange>�������� ������� {newProfile.name} �� �������� ������ �� ��������� ������. ������ �� ����� ��������� �������!</color>");
            return;
        }

        // - - Near Volumetric Fog
        // Color
        _nearFogMaterial.SetColor("_Color_Fog", Color.Lerp(currentProfile.NearVolumFogMat.GetColor("_Color_Fog"), newProfile.NearVolumFogMat.GetColor("_Color_Fog"), t));
        _nearFogMaterial.SetFloat("_Impact_Light", Mathf.Lerp(currentProfile.NearVolumFogMat.GetFloat("_Impact_Light"), newProfile.NearVolumFogMat.GetFloat("_Impact_Light"), t));
        _nearFogMaterial.SetFloat("_Min_Intensity_Fog", Mathf.Lerp(currentProfile.NearVolumFogMat.GetFloat("_Min_Intensity_Fog"), newProfile.NearVolumFogMat.GetFloat("_Min_Intensity_Fog"), t));
        // Scattering
        _nearFogMaterial.SetFloat("_Sun_Phase", Mathf.Lerp(currentProfile.NearVolumFogMat.GetFloat("_Sun_Phase"), newProfile.NearVolumFogMat.GetFloat("_Sun_Phase"), t));
        _nearFogMaterial.SetFloat("_Moon_Phase", Mathf.Lerp(currentProfile.NearVolumFogMat.GetFloat("_Moon_Phase"), newProfile.NearVolumFogMat.GetFloat("_Moon_Phase"), t));
        // Fog
        _nearFogMaterial.SetFloat("_Distance", Mathf.Lerp(currentProfile.NearVolumFogMat.GetFloat("_Distance"), newProfile.NearVolumFogMat.GetFloat("_Distance"), t));
        _nearFogMaterial.SetFloat("_Height", Mathf.Lerp(currentProfile.NearVolumFogMat.GetFloat("_Height"), newProfile.NearVolumFogMat.GetFloat("_Height"), t));
        _nearFogMaterial.SetFloat("_Softness_Height", Mathf.Lerp(currentProfile.NearVolumFogMat.GetFloat("_Softness_Height"), newProfile.NearVolumFogMat.GetFloat("_Softness_Height"), t));
        _nearFogMaterial.SetFloat("_Transparency", Mathf.Lerp(currentProfile.NearVolumFogMat.GetFloat("_Transparency"), newProfile.NearVolumFogMat.GetFloat("_Transparency"), t));

        // - - Far Volumetric Fog
        // Color
        _farFogMaterial.SetColor("_Color_Fog", Color.Lerp(currentProfile.FarVolumFogMat.GetColor("_Color_Fog"), newProfile.FarVolumFogMat.GetColor("_Color_Fog"), t));
        _farFogMaterial.SetFloat("_Impact_Light", Mathf.Lerp(currentProfile.FarVolumFogMat.GetFloat("_Impact_Light"), newProfile.FarVolumFogMat.GetFloat("_Impact_Light"), t));
        _farFogMaterial.SetFloat("_Min_Intensity_Fog", Mathf.Lerp(currentProfile.FarVolumFogMat.GetFloat("_Min_Intensity_Fog"), newProfile.FarVolumFogMat.GetFloat("_Min_Intensity_Fog"), t));
        // Scattering
        _farFogMaterial.SetFloat("_Sun_Phase", Mathf.Lerp(currentProfile.FarVolumFogMat.GetFloat("_Sun_Phase"), newProfile.FarVolumFogMat.GetFloat("_Sun_Phase"), t));
        _farFogMaterial.SetFloat("_Moon_Phase", Mathf.Lerp(currentProfile.FarVolumFogMat.GetFloat("_Moon_Phase"), newProfile.FarVolumFogMat.GetFloat("_Moon_Phase"), t));
        // Fog
        _farFogMaterial.SetFloat("_Distance", Mathf.Lerp(currentProfile.FarVolumFogMat.GetFloat("_Distance"), newProfile.FarVolumFogMat.GetFloat("_Distance"), t));
        _farFogMaterial.SetFloat("_Height", Mathf.Lerp(currentProfile.FarVolumFogMat.GetFloat("_Height"), newProfile.FarVolumFogMat.GetFloat("_Height"), t));
        _farFogMaterial.SetFloat("_Softness_Height", Mathf.Lerp(currentProfile.FarVolumFogMat.GetFloat("_Softness_Height"), newProfile.FarVolumFogMat.GetFloat("_Softness_Height"), t));
        _farFogMaterial.SetFloat("_Transparency", Mathf.Lerp(currentProfile.FarVolumFogMat.GetFloat("_Transparency"), newProfile.FarVolumFogMat.GetFloat("_Transparency"), t));
    }
    
    private void Update()
    {
        UpdateMaterialsSunDirection();
    }

    public void UpdateMaterialsSunDirection()
    {
        if (!IsSystemValid) return;

        if (!_sunTransform || !_nearFogMaterial.HasProperty("_Sun_Direction") || !_farFogMaterial.HasProperty("_Sun_Direction"))
        {
            Debug.LogWarning("<color=orange>�� ������� ����� ����������� ������ ��� ��� ���������� ������ ����������� ������ �������� ����������� ������, ����������� ������ �� ����������!</color>");
            return;
        }

        _nearFogMaterial.SetVector("_Sun_Direction", _sunTransform.transform.forward);
        _farFogMaterial.SetVector("_Sun_Direction", _sunTransform.transform.forward);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 0. �� ����������, ���� ��� ������-����� (�� ���������)
        if (PrefabUtility.IsPartOfPrefabAsset(this)) return;

        // 1. ������������� �������������� � ���������� ������� � ���������
        InitializeAndValidateSystem();

        // 2. ������������� ���������� �������� �������� VolumetricFogRenderFeatures
        if (string.IsNullOrEmpty(_nameNearFogFeature)) _nameNearFogFeature = "FullScreenVolumetricFogNear";
        if (string.IsNullOrEmpty(_nameFarFogFeature)) _nameFarFogFeature = "FullScreenVolumetricFogFar";

        // 3. ��������� �������� ��� �������
        if (PrefabUtility.IsPartOfPrefabInstance(this))
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }
#endif
}

