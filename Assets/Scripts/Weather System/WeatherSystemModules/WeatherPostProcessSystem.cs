using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WeatherPostProcessSystem : MonoBehaviour
{
    [DisableEdit, SerializeField] private bool _isPostProcessValide = false;

    [Header("Материал скайбокса:")]
    [DisableEdit, SerializeField] private Volume _volume;

    public void ValidateReferences()
    {
#if UNITY_EDITOR
        Undo.RecordObject(this, "Валидация скайбокса");
        EditorUtility.SetDirty(this);
#endif
        _volume = FindFirstObjectByType<Volume>();

        // Проверка ссылок на метериалы:
        if (_volume == null)
        {
            _isPostProcessValide = false;
            return;
        }

        // Проверка компонентов обьема пост процессинга:
        bool isValidePropertys;

        isValidePropertys = _volume.profile.TryGet<ColorAdjustments>(out var colorAdjustments);
        //isValidePropertys &= _volume.profile.TryGet<Bloom>(out var bloom);

        _isPostProcessValide = isValidePropertys;
    }

    public void UpdatePostProcessing(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        if (!_isPostProcessValide)
        {
            Debug.Log("<color=orange>Модуль пост процессинга в сцене неисправен, либо отстутствует. Погода не будет менять обьем пост процесса!</color>");
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
        ValidateReferences();
    }
}