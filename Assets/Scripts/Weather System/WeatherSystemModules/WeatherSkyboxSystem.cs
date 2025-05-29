using UnityEditor;
using UnityEngine;

public class WeatherSkyboxSystem : MonoBehaviour, IWeatherSystem
{
    [field: SerializeField, DisableEdit] public bool IsSystemValid { get; set; }

    [Header("- - Материал скайбокса:")]
    [SerializeField, DisableEdit] private Material _skyboxMaterial;
    [Space(10)]
    [SerializeField, DisableEdit] private Transform _sunTransform;

    public void ValidateSystem()
    {
        // 1. Инициализируем ссылку на материал скайбокса:
        _skyboxMaterial = RenderSettings.skybox;

        // 2. Проверяем найденную ссылку на метериал:
        if (!_skyboxMaterial)
        {
            IsSystemValid = false;
            return;
        }

        // 3. Проверяем наличие необходимых полей материала:
        bool isValidePropertys;

        // Colors
        isValidePropertys = _skyboxMaterial.HasProperty("_Color_Zenith");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Color_Horizon");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Softness_Zenith_Horizon");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Color_Skyline");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Softness_Sky_Skyline");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Range_Skyline");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Color_Ground");
        isValidePropertys &= _skyboxMaterial.HasProperty("_Softness_Sky_Ground");
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

        if (!IsSystemValid) return;

        // 4. Инициализация трансформа солнца в сцене:
        WeatherLightingColor[] WeatherLightingColors = FindObjectsByType<WeatherLightingColor>(FindObjectsSortMode.None);
        foreach (WeatherLightingColor lightColor in WeatherLightingColors)
            if (lightColor.isSun)
                _sunTransform = lightColor.transform;

#if UNITY_EDITOR
        if (PrefabUtility.IsPartOfPrefabInstance(this))
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
    }

    public void UpdateSystem(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        if (!IsSystemValid)
        {
            Debug.LogWarning("<color=orange>Модуль скайбокса неисправен. Система погоды не будет управлять скайбоксом!</color>");
            return;
        }

        // Colors
        _skyboxMaterial.SetColor("_Color_Zenith", Color.Lerp(currentProfile.SkyboxMat.GetColor("_Color_Zenith"), newProfile.SkyboxMat.GetColor("_Color_Zenith"), t));
        _skyboxMaterial.SetColor("_Color_Horizon", Color.Lerp(currentProfile.SkyboxMat.GetColor("_Color_Horizon"), newProfile.SkyboxMat.GetColor("_Color_Horizon"), t));
        _skyboxMaterial.SetFloat("_Softness_Zenith_Horizon", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Softness_Zenith_Horizon"), newProfile.SkyboxMat.GetFloat("_Softness_Zenith_Horizon"), t));
        _skyboxMaterial.SetColor("_Color_Skyline", Color.Lerp(currentProfile.SkyboxMat.GetColor("_Color_Skyline"), newProfile.SkyboxMat.GetColor("_Color_Skyline"), t));
        _skyboxMaterial.SetFloat("_Softness_Sky_Skyline", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Softness_Sky_Skyline"), newProfile.SkyboxMat.GetFloat("_Softness_Sky_Skyline"), t));
        _skyboxMaterial.SetFloat("_Range_Skyline", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Range_Skyline"), newProfile.SkyboxMat.GetFloat("_Range_Skyline"), t));
        _skyboxMaterial.SetColor("_Color_Ground", Color.Lerp(currentProfile.SkyboxMat.GetColor("_Color_Ground"), newProfile.SkyboxMat.GetColor("_Color_Ground"), t));
        _skyboxMaterial.SetFloat("_Softness_Sky_Ground", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Softness_Sky_Ground"), newProfile.SkyboxMat.GetFloat("_Softness_Sky_Ground"), t));
        // HeyneyGreenstein Scattering
        _skyboxMaterial.SetFloat("_Sun_Phase_In", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Sun_Phase_In"), newProfile.SkyboxMat.GetFloat("_Sun_Phase_In"), t));
        _skyboxMaterial.SetFloat("_Sun_Phase_Out", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Sun_Phase_Out"), newProfile.SkyboxMat.GetFloat("_Sun_Phase_Out"), t));
        _skyboxMaterial.SetFloat("_Moon_Phase", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Moon_Phase"), newProfile.SkyboxMat.GetFloat("_Moon_Phase"), t));
        // Rayleight Scattering
        _skyboxMaterial.SetFloat("_Height_Atmosphere", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Height_Atmosphere"), newProfile.SkyboxMat.GetFloat("_Height_Atmosphere"), t));
        // Stars
        _skyboxMaterial.SetColor("_Color_Stars", Color.Lerp(currentProfile.SkyboxMat.GetColor("_Color_Stars"), newProfile.SkyboxMat.GetColor("_Color_Stars"), t));
        _skyboxMaterial.SetFloat("_Horizon_Offset", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Horizon_Offset"), newProfile.SkyboxMat.GetFloat("_Horizon_Offset"), t));
        _skyboxMaterial.SetFloat("_Scale_Flick_Noise", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Scale_Flick_Noise"), newProfile.SkyboxMat.GetFloat("_Scale_Flick_Noise"), t));
        _skyboxMaterial.SetFloat("_Speed_Flick_Noise", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Speed_Flick_Noise"), newProfile.SkyboxMat.GetFloat("_Speed_Flick_Noise"), t));
        // Moon
        _skyboxMaterial.SetColor("_Moon_Color", Color.Lerp(currentProfile.SkyboxMat.GetColor("_Moon_Color"), newProfile.SkyboxMat.GetColor("_Moon_Color"), t));
    }

    private void Awake()
    {
        ValidateSystem();
    }

    void Update()
    {
        UpdateSunDirection();
    }

    /// <summary>
    /// Передать актуальное направление солнца в указанные материалы
    /// </summary>
    public void UpdateSunDirection()
    {
        if (!IsSystemValid) return;

        if (_sunTransform == null || !_skyboxMaterial.HasProperty("_Sun_Direction"))
        {
            Debug.LogWarning("<color=orange>Отсутсвует ссылка на источник света(солнце) или у материала нет требуемой переменной. Направление источника света не будет передаваться в систему скайбокса!</color>");
            return;
        }

        _skyboxMaterial.SetVector("_Sun_Direction", _sunTransform.transform.forward);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateSystem();
    }
#endif
}