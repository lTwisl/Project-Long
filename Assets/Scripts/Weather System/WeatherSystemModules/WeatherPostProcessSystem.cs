using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WeatherPostProcessSystem : MonoBehaviour, IWeatherSystem
{
    [field: SerializeField, DisableEdit] public bool IsSystemValid { get; set; }

    [Header("����� ����������������:")]
    [SerializeField, DisableEdit] private Volume _volume;

    [Header("���������� ����������������:")]
    [SerializeField, DisableEdit] private ColorAdjustments _colorAdj;
    [SerializeField, DisableEdit] private Bloom _bloom;

    public void ValidateSystem()
    {
        // 1. �������������� ������ �� ����� ���� �����������:
        _volume = FindFirstObjectByType<Volume>();
        if (!_volume)
        {
            IsSystemValid = false;
            return;
        }

        // 2. �������� ������� ����������� ������ ���� �����������:
        bool isValidePropertys;

        isValidePropertys = _volume.profile.TryGet<ColorAdjustments>(out _colorAdj);
        isValidePropertys &= _volume.profile.TryGet<Bloom>(out _bloom);

        IsSystemValid = isValidePropertys;

#if UNITY_EDITOR
        if (PrefabUtility.IsPartOfPrefabInstance(this))
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
    }

    public void UpdateSystem(WeatherProfile currentProfile, WeatherProfile nextProfile, float t)
    {
        if (!IsSystemValid)
        {
            Debug.LogWarning("<color=orange>������ ���� ����������� ���������, ���� ������������. ������ �� ����� ��������� ������� ���� �����������!</color>");
            return;
        }

        // ColorAdjustments:
        _colorAdj.postExposure.value = Mathf.Lerp(currentProfile.ColAdjPostExposure, nextProfile.ColAdjPostExposure, t);
        _colorAdj.contrast.value = Mathf.Lerp(currentProfile.ColAdjContrast, nextProfile.ColAdjContrast, t);
        _colorAdj.colorFilter.value = Color.Lerp(currentProfile.ColAdjColorFilter, nextProfile.ColAdjColorFilter, t);
        _colorAdj.saturation.value = Mathf.Lerp(currentProfile.ColAdjSaturation, nextProfile.ColAdjSaturation, t);

        // Bloom:
        _bloom.threshold.value = Mathf.Lerp(currentProfile.BloomThreshold, nextProfile.BloomThreshold, t);
        _bloom.intensity.value = Mathf.Lerp(currentProfile.BloomIntensity, nextProfile.BloomIntensity, t);
        _bloom.scatter.value = Mathf.Lerp(currentProfile.BloomScatter, nextProfile.BloomScatter, t);
        _bloom.tint.value = Color.Lerp(currentProfile.BloomTint, nextProfile.BloomTint, t);
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