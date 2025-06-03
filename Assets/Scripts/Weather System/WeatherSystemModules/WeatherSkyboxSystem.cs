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

        _isSystemValid = isValidePropertys;

        if (!IsSystemValid) return;

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
            Debug.LogWarning($"<color=orange>Погодный профиль {nextProfile.name} не содержит ссылку на материал скайбокса. Погода не будет управлять скайбоксом!</color>");
            return;
        }

        // Colors
        _skyboxMaterial.SetColor("_Color_Zenith", Color.Lerp(currentProfile.SkyboxMat.GetColor("_Color_Zenith"), nextProfile.SkyboxMat.GetColor("_Color_Zenith"), t));
        _skyboxMaterial.SetColor("_Color_Horizon", Color.Lerp(currentProfile.SkyboxMat.GetColor("_Color_Horizon"), nextProfile.SkyboxMat.GetColor("_Color_Horizon"), t));
        _skyboxMaterial.SetFloat("_Softness_Zenith_Horizon", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Softness_Zenith_Horizon"), nextProfile.SkyboxMat.GetFloat("_Softness_Zenith_Horizon"), t));
        _skyboxMaterial.SetColor("_Color_Skyline", Color.Lerp(currentProfile.SkyboxMat.GetColor("_Color_Skyline"), nextProfile.SkyboxMat.GetColor("_Color_Skyline"), t));
        _skyboxMaterial.SetFloat("_Softness_Sky_Skyline", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Softness_Sky_Skyline"), nextProfile.SkyboxMat.GetFloat("_Softness_Sky_Skyline"), t));
        _skyboxMaterial.SetFloat("_Range_Skyline", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Range_Skyline"), nextProfile.SkyboxMat.GetFloat("_Range_Skyline"), t));
        _skyboxMaterial.SetColor("_Color_Ground", Color.Lerp(currentProfile.SkyboxMat.GetColor("_Color_Ground"), nextProfile.SkyboxMat.GetColor("_Color_Ground"), t));
        _skyboxMaterial.SetFloat("_Softness_Sky_Ground", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Softness_Sky_Ground"), nextProfile.SkyboxMat.GetFloat("_Softness_Sky_Ground"), t));
        // HeyneyGreenstein Scattering
        _skyboxMaterial.SetFloat("_Sun_Phase_In", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Sun_Phase_In"), nextProfile.SkyboxMat.GetFloat("_Sun_Phase_In"), t));
        _skyboxMaterial.SetFloat("_Sun_Phase_Out", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Sun_Phase_Out"), nextProfile.SkyboxMat.GetFloat("_Sun_Phase_Out"), t));
        _skyboxMaterial.SetFloat("_Moon_Phase", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Moon_Phase"), nextProfile.SkyboxMat.GetFloat("_Moon_Phase"), t));
        // Rayleight Scattering
        _skyboxMaterial.SetFloat("_Height_Atmosphere", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Height_Atmosphere"), nextProfile.SkyboxMat.GetFloat("_Height_Atmosphere"), t));
        // Stars
        _skyboxMaterial.SetColor("_Color_Stars", Color.Lerp(currentProfile.SkyboxMat.GetColor("_Color_Stars"), nextProfile.SkyboxMat.GetColor("_Color_Stars"), t));
        _skyboxMaterial.SetFloat("_Horizon_Offset", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Horizon_Offset"), nextProfile.SkyboxMat.GetFloat("_Horizon_Offset"), t));
        _skyboxMaterial.SetFloat("_Scale_Flick_Noise", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Scale_Flick_Noise"), nextProfile.SkyboxMat.GetFloat("_Scale_Flick_Noise"), t));
        _skyboxMaterial.SetFloat("_Speed_Flick_Noise", Mathf.Lerp(currentProfile.SkyboxMat.GetFloat("_Speed_Flick_Noise"), nextProfile.SkyboxMat.GetFloat("_Speed_Flick_Noise"), t));
        // Moon
        _skyboxMaterial.SetColor("_Moon_Color", Color.Lerp(currentProfile.SkyboxMat.GetColor("_Moon_Color"), nextProfile.SkyboxMat.GetColor("_Moon_Color"), t));
    }

    void Update()
    {
        UpdateMaterialsSunDirection();
    }

    public void UpdateMaterialsSunDirection()
    {
        if (!IsSystemValid) return;

        if (!_sunTransform || !_skyboxMaterial.HasProperty("_Sun_Direction"))
        {
            Debug.LogWarning("<color=orange>Не удалось взять направление солнца или для материала скайбокса неправильно указан параметр направления солнца, направление солнца не передается</color>");
            return;
        }

        _skyboxMaterial.SetVector("_Sun_Direction", _sunTransform.transform.forward);
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