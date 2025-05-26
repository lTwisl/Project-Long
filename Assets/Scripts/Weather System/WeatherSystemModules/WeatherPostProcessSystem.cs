using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WeatherPostProcessSystem : MonoBehaviour, IWeatherSystem
{
    [field: SerializeField, DisableEdit] public bool IsSystemValid { get; set; }

    [Header("����� ����������������:")]
    [SerializeField, DisableEdit] private Volume _volume;

    public void ValidateSystem()
    {
        _volume = FindFirstObjectByType<Volume>();

        // �������� ������ �� ���������:
        if (_volume == null)
        {
            IsSystemValid = false;
            return;
        }

        // �������� ����������� ������ ���� �����������:
        bool isValidePropertys;

        isValidePropertys = _volume.profile.TryGet<ColorAdjustments>(out var colorAdjustments);
        //isValidePropertys &= _volume.profile.TryGet<Bloom>(out var bloom);

        IsSystemValid = isValidePropertys;

#if UNITY_EDITOR
        if (PrefabUtility.IsPartOfPrefabInstance(this))
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
    }

    public void UpdateSystem(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        if (!IsSystemValid)
        {
            Debug.Log("<color=orange>������ ���� ����������� � ����� ����������, ���� ������������. ������ �� ����� ������ ����� ���� ��������!</color>");
            return;
        }

        if (_volume.profile.TryGet<ColorAdjustments>(out var colorAdjustments))
        {
            colorAdjustments.postExposure.value = Mathf.Lerp(currentProfile.postExposure, newProfile.postExposure, t);
            colorAdjustments.contrast.value = Mathf.Lerp(currentProfile.constrast, newProfile.constrast, t);
            colorAdjustments.saturation.value = Mathf.Lerp(currentProfile.saturation, newProfile.saturation, t);
        }
    }

    private void Awake()
    {
        ValidateSystem();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateSystem();
    }
#endif
}