using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WeatherPostProcessSystem : MonoBehaviour, IWeatherSystem
{
    [SerializeField, DisableEdit] private bool _isSystemValid;
    public bool IsSystemValid => _isSystemValid;

    [Header("- - Обьем простпроцессинга:")]
    [SerializeField, DisableEdit] private Volume _volume;

    [Header("- - Компоненты простпроцессинга:")]
    [SerializeField, DisableEdit] private ColorAdjustments _colorAdj;
    [SerializeField, DisableEdit] private Bloom _bloom;

    public void InitializeAndValidateSystem()
    {
        // 1. Поиск обьема пост процессинга в сцене:
        _volume = FindFirstObjectByType<Volume>();

        // 2. Проверяем найденную ссылку на обьем:
        if (!_volume)
        {
            _isSystemValid = false;
            return;
        }

        // 3. Проверка наличия требуемых компонентов обьема пост процессинга с кешированием сслыки на них
        bool isValideComponents;
        isValideComponents = _volume.profile.TryGet<ColorAdjustments>(out _colorAdj);
        isValideComponents &= _volume.profile.TryGet<Bloom>(out _bloom);

        _isSystemValid = isValideComponents;
    }

    public void UpdateSystemParameters(WeatherProfile currentProfile, WeatherProfile nextProfile, float t)
    {
        if (!IsSystemValid)
        {
            Debug.LogWarning("<color=orange>Модуль пост процессинга невалиден, либо отстутствует. Погода не будет управлять обьемом пост процессинга!</color>");
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