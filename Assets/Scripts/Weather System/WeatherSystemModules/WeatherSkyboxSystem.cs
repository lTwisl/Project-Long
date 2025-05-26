using UnityEditor;
using UnityEngine;

public class WeatherSkyboxSystem : MonoBehaviour, IWeatherSystem
{
    [field: SerializeField, DisableEdit] public bool IsSystemValid { get; set; }

    [Header("�������� ���������:")]
    [SerializeField, DisableEdit] private Material _skyboxMaterial;
    [Space(10)]
    [SerializeField, DisableEdit] private Transform _sunTransform;

    public void ValidateSystem()
    {
        _skyboxMaterial = RenderSettings.skybox;

        // ������������� ���������� ������ � �����:
        WeatherLightingColor[] dynamicLightingColors = FindObjectsByType<WeatherLightingColor>(FindObjectsSortMode.None);
        foreach (WeatherLightingColor dyn in dynamicLightingColors)
            if (dyn.isSun)
                _sunTransform = dyn.transform;

        // �������� ������ �� ���������:
        if (_skyboxMaterial == null)
        {
            IsSystemValid = false;
            return;
        }

        // �������� ��������� ����� ���������:
        bool isValidePropertys;

        // Colors
        isValidePropertys = _skyboxMaterial.HasProperty("_Color_On_Zenith");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Color_On_Horizon");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Softness_Gradient_Mask");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Color_On_Skyline");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Softness_Skyline_Mask");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Range_Skyline_Mask");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Color_On_Ground");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Softness_Ground_Mask");
        // HeyneyGreenstein Scattering
        isValidePropertys &= _skyboxMaterial.HasProperty("_Sun_Phase_In");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Sun_Phase_Out");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Moon_Phase");
        // Rayleight Scattering
        isValidePropertys &= _skyboxMaterial.HasProperty("_Height_Atmosphere");
        // Stars
        isValidePropertys &= _skyboxMaterial.HasProperty("_Color_Stars");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Horizon_Offset");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Scale_Flick_Noise");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Speed_Flick_Noise");
        // Moon
        isValidePropertys &= _skyboxMaterial.HasProperty("_Moon_Color");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Sun_Direction");

        IsSystemValid = isValidePropertys;

#if UNITY_EDITOR
        if (PrefabUtility.IsPartOfPrefabInstance(this))
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
    }

    /// <summary>
    /// �������� ��������� ���������
    /// </summary>
    public void UpdateSystem(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        if (!IsSystemValid)
        {
            Debug.Log("<color=orange>������ ��������� � ����� ����������, ���� ������������. ������ �� ����� ������ ��������!</color>");
            return;
        }

        // Colors
        _skyboxMaterial.SetColor("_Color_On_Zenith", Color.Lerp(currentProfile.materialSkybox.GetColor("_Color_On_Zenith"), newProfile.materialSkybox.GetColor("_Color_On_Zenith"), t));
        _skyboxMaterial.SetColor("_Color_On_Horizon", Color.Lerp(currentProfile.materialSkybox.GetColor("_Color_On_Horizon"), newProfile.materialSkybox.GetColor("_Color_On_Horizon"), t));
        _skyboxMaterial.SetFloat("_Softness_Gradient_Mask", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Softness_Gradient_Mask"), newProfile.materialSkybox.GetFloat("_Softness_Gradient_Mask"), t));
        _skyboxMaterial.SetColor("_Color_On_Skyline", Color.Lerp(currentProfile.materialSkybox.GetColor("_Color_On_Skyline"), newProfile.materialSkybox.GetColor("_Color_On_Skyline"), t));
        _skyboxMaterial.SetFloat("_Softness_Skyline_Mask", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Softness_Skyline_Mask"), newProfile.materialSkybox.GetFloat("_Softness_Skyline_Mask"), t));
        _skyboxMaterial.SetFloat("_Range_Skyline_Mask", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Range_Skyline_Mask"), newProfile.materialSkybox.GetFloat("_Range_Skyline_Mask"), t));
        _skyboxMaterial.SetColor("_Color_On_Ground", Color.Lerp(currentProfile.materialSkybox.GetColor("_Color_On_Ground"), newProfile.materialSkybox.GetColor("_Color_On_Ground"), t));
        _skyboxMaterial.SetFloat("_Softness_Ground_Mask", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Softness_Ground_Mask"), newProfile.materialSkybox.GetFloat("_Softness_Ground_Mask"), t));
        // HeyneyGreenstein Scattering
        _skyboxMaterial.SetFloat("_Sun_Phase_In", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Sun_Phase_In"), newProfile.materialSkybox.GetFloat("_Sun_Phase_In"), t));
        _skyboxMaterial.SetFloat("_Sun_Phase_Out", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Sun_Phase_Out"), newProfile.materialSkybox.GetFloat("_Sun_Phase_Out"), t));
        _skyboxMaterial.SetFloat("_Moon_Phase", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Moon_Phase"), newProfile.materialSkybox.GetFloat("_Moon_Phase"), t));
        // Rayleight Scattering
        _skyboxMaterial.SetFloat("_Height_Atmosphere", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Height_Atmosphere"), newProfile.materialSkybox.GetFloat("_Height_Atmosphere"), t));
        // Stars
        _skyboxMaterial.SetColor("_Color_Stars", Color.Lerp(currentProfile.materialSkybox.GetColor("_Color_Stars"), newProfile.materialSkybox.GetColor("_Color_Stars"), t));
        _skyboxMaterial.SetFloat("_Horizon_Offset", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Horizon_Offset"), newProfile.materialSkybox.GetFloat("_Horizon_Offset"), t));
        _skyboxMaterial.SetFloat("_Scale_Flick_Noise", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Scale_Flick_Noise"), newProfile.materialSkybox.GetFloat("_Scale_Flick_Noise"), t));
        _skyboxMaterial.SetFloat("_Speed_Flick_Noise", Mathf.Lerp(currentProfile.materialSkybox.GetFloat("_Speed_Flick_Noise"), newProfile.materialSkybox.GetFloat("_Speed_Flick_Noise"), t));
        // Moon
        _skyboxMaterial.SetColor("_Moon_Color", Color.Lerp(currentProfile.materialSkybox.GetColor("_Moon_Color"), newProfile.materialSkybox.GetColor("_Moon_Color"), t));
    }

    private void Awake()
    {
        ValidateSystem();
    }

    void Update()
    {
        UpdateSunDirection();
    }

    public void UpdateSunDirection()
    {
        if (_sunTransform == null || !IsSystemValid) return;

        _skyboxMaterial.SetVector("_Sun_Direction", _sunTransform.transform.forward);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateSystem();
    }
#endif
}