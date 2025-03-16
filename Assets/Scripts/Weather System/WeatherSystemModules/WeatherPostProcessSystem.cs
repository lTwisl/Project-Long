using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WeatherPostProcessSystem : MonoBehaviour
{
    [DisableEdit, SerializeField] private bool _isPostProcessValide = false;

    [Header("�������� ���������:")]
    [DisableEdit, SerializeField] private Volume _volume;

    public void ValidateReferences()
    {
#if UNITY_EDITOR
        Undo.RecordObject(this, "��������� ���������");
        EditorUtility.SetDirty(this);
#endif
        _volume = FindFirstObjectByType<Volume>();

        // �������� ������ �� ���������:
        if (_volume == null)
        {
            _isPostProcessValide = false;
            return;
        }

        // �������� ����������� ������ ���� �����������:
        bool isValidePropertys;

        isValidePropertys = _volume.profile.TryGet<ColorAdjustments>(out var colorAdjustments);
        //isValidePropertys &= _volume.profile.TryGet<Bloom>(out var bloom);

        _isPostProcessValide = isValidePropertys;
    }

    public void UpdatePostProcessing(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        if (!_isPostProcessValide)
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
        ValidateReferences();
    }
}