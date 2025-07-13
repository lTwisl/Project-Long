using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WeatherPostProcessSystem : MonoBehaviour, IWeatherSystem
{
    [field: SerializeField, DisableEdit] public bool IsSystemValid { get; set; }

    [Header("- - Post Process Volume:")]
    [SerializeField, DisableEdit] private Volume _volume;

    [Header("- - Post Process Volume Components:")]
    [SerializeField, DisableEdit] private ColorAdjustments _colorAdj;
    [SerializeField, DisableEdit] private Bloom _bloom;

    public void InitializeAndValidateSystem()
    {
        // 1. Поиск Post Process Volume в сцене:
        if (!_volume)
        {
            Volume[] volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);

            // 1.1. Кешируем ссылку Post Process Volume:
            foreach (Volume vol in volumes)
            {
                if (vol && vol.isGlobal)
                {
                    _volume = vol;
                    IsSystemValid = true;
                    break;
                }
            }
        }

        // 2. Проверяем кешированную ссылку Post Process Volume:
        if (!_volume)
        {
            IsSystemValid = false;
            Debug.LogWarning("<color=orange>Система WeatherPostProcess невалидна (не найден Post Process Volume)!</color>");
            return;
        }

        // 3. Кешируем требуемые компоненты Post Process Volume
        GetPostProcessVolumeComponents();
    }

    public void UpdateSystemParameters(WeatherProfile currentProfile, WeatherProfile nextProfile, float t)
    {
        if (!IsSystemValid) return;

        // ColorAdjustments:
        if (_colorAdj)
        {
            _colorAdj.postExposure.value = Mathf.Lerp(currentProfile.ColAdjPostExposure, nextProfile.ColAdjPostExposure, t);
            _colorAdj.contrast.value = Mathf.Lerp(currentProfile.ColAdjContrast, nextProfile.ColAdjContrast, t);
            _colorAdj.colorFilter.value = Color.Lerp(currentProfile.ColAdjColorFilter, nextProfile.ColAdjColorFilter, t);
            _colorAdj.saturation.value = Mathf.Lerp(currentProfile.ColAdjSaturation, nextProfile.ColAdjSaturation, t);
        }

        // Bloom:
        if (_bloom)
        {
            _bloom.threshold.value = Mathf.Lerp(currentProfile.BloomThreshold, nextProfile.BloomThreshold, t);
            _bloom.intensity.value = Mathf.Lerp(currentProfile.BloomIntensity, nextProfile.BloomIntensity, t);
            _bloom.scatter.value = Mathf.Lerp(currentProfile.BloomScatter, nextProfile.BloomScatter, t);
            _bloom.tint.value = Color.Lerp(currentProfile.BloomTint, nextProfile.BloomTint, t);
        }
    }

    public void SetWeatherPostProcessVolumeProfile(VolumeProfile volumeProfile)
    {
        if (!IsSystemValid) return;

        if (!volumeProfile) return;

        _volume.sharedProfile = volumeProfile; // Обновляем ссылку на профиль
        _volume.profile = volumeProfile; // Обновляем данные профиля

        // Заново кешируем ссылки на компоненты Post Process Volume
        GetPostProcessVolumeComponents();
    }

    private void GetPostProcessVolumeComponents()
    {
        _volume.profile.TryGet<ColorAdjustments>(out _colorAdj);
        _volume.profile.TryGet<Bloom>(out _bloom);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (EditorChangeTracker.IsPrefabInstance(this))
        {
            EditorChangeTracker.RegisterUndo(this, "Initialize and Validate Weather PP System");
            InitializeAndValidateSystem();
            EditorChangeTracker.SetDirty(this);
        }
    }
#endif
}