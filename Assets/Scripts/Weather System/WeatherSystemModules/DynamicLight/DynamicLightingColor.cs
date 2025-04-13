using System;
using UnityEngine;

public class DynamicLightingColor : MonoBehaviour
{
    [Header("��������� �����:")] public bool isSun = true;
    [DisableEdit, SerializeField] private bool _isLightValide;
    [SerializeField, Tooltip("������������ ������������� ��������� ���������"), Min(0)]
    private float _maxIntensity = 1.5f;
    [HideIf(nameof(isSun), false), SerializeField, Tooltip("����������� ��������� ���������"), Min(0)]
    private float _temperature = 8000f;
    [Tooltip("���� ����� � �������")] public Color colorAtZenith = new Color(1f, 0.95f, 0.9f);
    [Tooltip("���� ����� �� ������")] public Color colorAtSunset = new Color(1f, 0.65f, 0.3f);

    [SerializeField, Tooltip("�������� ������� ��������� ��������� �������������"), Range(-1, 1)]
    private float _horizontOffset = 0.1f;
    [SerializeField, Tooltip("�������� ����� ��� �������� �������� (�� X'����' �� Y'���')")]
    private Vector2 _colorTransitionRangeAngles = new(90, 0);

    [HideIf(nameof(isSun), false), SerializeField, Tooltip("������������ ������������� ����������� ���������"), Range(0f, 2f)]
    private float _maxAmbientIntensity = 1f;
    [HideIf(nameof(isSun), false), SerializeField, Tooltip("����������� ������������� ����������� ���������"), Range(0f, 1f)]
    private float _minAmbientIntensity = 0.3f;

    [Header("������ ��������:")]
    [SerializeField] private AnimationCurve _intensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField] private AnimationCurve _colorBlendCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    [HideIf(nameof(isSun), false), SerializeField] private AnimationCurve _ambientIntensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    private Light _light;
    private Vector2 _colorTransitionRange;

    public float MaxIntensity
    {
        get => _maxIntensity;
        set => _maxIntensity = Mathf.Clamp(value, 0, float.MaxValue);
    }
    public float Temperature
    {
        get => _temperature;
        set => _temperature = Mathf.Clamp(value, 0, 20000);
    }

    private void Awake()
    {
        ValidateReferences();
        GameTime.OnTimeChanged += UpdateLightingParameters;
    }

    public void ValidateReferences()
    {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "��������� ��������� ���������");
#endif

        _light = GetComponent<Light>();
        if (_light == null)
        {
            _isLightValide = false;
            return;
        }

        // ��������� ���� � �������� dotProduct
        _colorTransitionRange = new Vector2(
            Mathf.Sin(_colorTransitionRangeAngles.x * Mathf.Deg2Rad),
            Mathf.Sin(_colorTransitionRangeAngles.y * Mathf.Deg2Rad)
        );

        _isLightValide = true;
    }

    /// <summary>
    /// ���������� ���������� �����
    /// </summary>
    public void UpdateLightingParameters()
    {
        if (!_isLightValide) return;

        float dotProduct = CalculateDotProduct();
        float normalizedAngle = Mathf.Clamp((dotProduct + _horizontOffset), 0, 1);

        SetLightIntensity(normalizedAngle);
        SetLightColor(dotProduct);
        if (isSun)
            SetAmbientIntensity(normalizedAngle);
    }

    /// <summary>
    /// ���������� ���������� ������������ ��� ����������� ����������� �����
    /// </summary>
    /// <returns>�������� ���������� ������������</returns>
    private float CalculateDotProduct()
    {
        return Vector3.Dot(-transform.forward, Vector3.up);
    }

    /// <summary>
    /// ��������� ������������� �����
    /// </summary>
    /// <param name="normalizedAngle">��������������� ����</param>
    private void SetLightIntensity(float normalizedAngle)
    {
        float curveValue = _intensityCurve.Evaluate(normalizedAngle);
        _light.intensity = Mathf.Lerp(0, _maxIntensity, curveValue);
    }

    /// <summary>
    /// ��������� ����� �����
    /// </summary>
    /// <param name="dotProduct">��������� ������������</param>
    private void SetLightColor(float dotProduct)
    {
        // ����������� dotProduct � ��������� ��������
        float normalizedDot = Mathf.InverseLerp(_colorTransitionRange.x, _colorTransitionRange.y, dotProduct);

        // ���������� ������ ��� �������� ��������
        float colorBlend = _colorBlendCurve.Evaluate(normalizedDot);

        // ������������� ���� ����� ������� � ����������
        _light.color = Color.Lerp(colorAtZenith, colorAtSunset, colorBlend);
    }

    /// <summary>
    /// ��������� ������������� ����������� �����
    /// </summary>
    /// <param name="normalizedAngle">��������������� ����</param>
    private void SetAmbientIntensity(float normalizedAngle)
    {
        _light.colorTemperature = _temperature;
        float ambientCurveValue = _ambientIntensityCurve.Evaluate(normalizedAngle);
        RenderSettings.ambientIntensity = Mathf.Lerp(_minAmbientIntensity, _maxAmbientIntensity, ambientCurveValue);
    }

    /// <summary>
    /// �������� ��������� ������� ���������
    /// </summary>
    public void UpdateLighting(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        MaxIntensity = Mathf.Lerp(currentProfile.maxIntensitySun, newProfile.maxIntensitySun, t);
        colorAtZenith = Color.Lerp(currentProfile.colorZenithSun, newProfile.colorZenithSun, t);
        colorAtSunset = Color.Lerp(currentProfile.colorSunsetSun, newProfile.colorSunsetSun, t);

        if (isSun)
        {
            Temperature = Mathf.Lerp(currentProfile.temperatureSun, newProfile.temperatureSun, t);
        }
    }

    private void OnDestroy()
    {
        GameTime.OnTimeChanged -= UpdateLightingParameters;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateReferences();
    }
#endif
}