using System;
using UnityEditor;
using UnityEngine;

public class WeatherLightingColor : MonoBehaviour, IWeatherSystem
{
    [field: SerializeField, DisableEdit] public bool IsSystemValid { get; set; }

    [field: Header("- - ��������� ��������� �����:")]
    [field: SerializeField] public bool IsSun { get; private set; } = true;

    [SerializeField, Tooltip("���� ��������� ����� � �������")] private Color _colorAtZenith = new(1f, 0.95f, 0.9f);
    [SerializeField, Tooltip("���� ��������� ����� �� ������")] private Color _colorAtSunset = new(1f, 0.5f, 0f);

    [SerializeField, Range(0f, 5f)] private float _maxIntensity = 3f;
    [HideIf(nameof(IsSun), false)]
    [SerializeField, Range(3000f, 20000f)] private float _temperature = 8000f;

    [Tooltip("������������ ������������� ����������� ���������"), HideIf(nameof(IsSun), false)]
    [SerializeField, Range(0f, 2f)] private float _maxAmbientIntensity = 1f;
    [Tooltip("����������� ������������� ����������� ���������"), HideIf(nameof(IsSun), false)]
    [SerializeField, Range(0f, 1f)] private float _minAmbientIntensity = 0.5f;

    [Tooltip("�������� ������� ��������� ��������� �������������")]
    [SerializeField, Range(-1, 1)] private float _horizontOffset = 0.1f;
    [Tooltip("�������� ����� ��� �������� �������� (�� X'����' �� Y'���')")]
    [SerializeField] private Vector2 _colorTransitionRangeAngles = new(90, 0);

    [Header("- - ������������ ������ ��������:")]
    [SerializeField] private AnimationCurve _intensityCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
    [SerializeField] private AnimationCurve _colorCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
    [HideIf(nameof(IsSun), false)]
    [SerializeField] private AnimationCurve _ambientIntensityCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));

    private Vector2 _colorTransitionRange;

    [field: SerializeField, DisableEdit] public Light Light { get; private set; }
    public float MaxIntensity
    {
        get => _maxIntensity;
        set => _maxIntensity = Mathf.Clamp(value, 0, 5);
    }
    public float Temperature
    {
        get => _temperature;
        set => _temperature = Mathf.Clamp(value, 3000, 20000);
    }


    private void OnEnable()
    {
        GameTime.OnTimeChanged += UpdateLightingParameters;
    }

    private void OnDisable()
    {
        GameTime.OnTimeChanged -= UpdateLightingParameters;
    }

    public void UpdateLightingParameters()
    {
        if (!IsSystemValid) return;

        float dotProduct = CalculateDotProduct();
        float normalizedAngle = Mathf.Clamp((dotProduct + _horizontOffset), 0, 1);

        SetLightIntensity(normalizedAngle);
        SetLightColor(dotProduct);

        if (IsSun) SetAmbientIntensity(normalizedAngle);
    }

    /// <summary>
    /// ���������� ���������� ������������ ��� ����������� ����������� �����
    /// </summary>
    private float CalculateDotProduct()
    {
        return Vector3.Dot(-transform.forward, Vector3.up);
    }

    /// <summary>
    /// ��������� ������������� �����
    /// </summary>
    /// <param name="normalizedAngle">��������������� ���� ����������� ��������� �����</param>
    private void SetLightIntensity(float normalizedAngle)
    {
        float curveValue = _intensityCurve.Evaluate(normalizedAngle);
        Light.intensity = Mathf.Lerp(0, _maxIntensity, curveValue);
    }

    /// <summary>
    /// ��������� ����� �����
    /// </summary>
    /// <param name="dotProduct">��������� ������������ ����������� �����</param>
    private void SetLightColor(float dotProduct)
    {
        // ����������� dotProduct � ��������� ��������
        float normalizedDot = Mathf.InverseLerp(_colorTransitionRange.x, _colorTransitionRange.y, dotProduct);

        // ���������� ������ ��� �������� ��������
        float colorBlend = _colorCurve.Evaluate(normalizedDot);

        // ������������� ���� ����� ������� � ����������
        Light.color = Color.Lerp(_colorAtZenith, _colorAtSunset, colorBlend);
    }

    /// <summary>
    /// ��������� ������������� ����������� ���������
    /// </summary>
    /// <param name="normalizedAngle">��������������� ���� ����������� ��������� �����</param>
    private void SetAmbientIntensity(float normalizedAngle)
    {
        Light.colorTemperature = _temperature;
        float ambientCurveValue = _ambientIntensityCurve.Evaluate(normalizedAngle);
        RenderSettings.ambientIntensity = Mathf.Lerp(_minAmbientIntensity, _maxAmbientIntensity, ambientCurveValue);
    }

    public void InitializeAndValidateSystem()
    {
        if (!Light) Light = GetComponent<Light>();

        if (!Light)
        {
            IsSystemValid = false;
            Debug.LogWarning($"<color=orange>������� SunLight ��������� (�� ������� ������ �� Light Component)!</color>");
            return;
        }

        // ��������� ���� � �������� dotProduct
        _colorTransitionRange = new Vector2(Mathf.Sin(_colorTransitionRangeAngles.x * Mathf.Deg2Rad), Mathf.Sin(_colorTransitionRangeAngles.y * Mathf.Deg2Rad));

        IsSystemValid = true;
    }

    /// <summary>
    /// �������� ��������� ������� ���������
    /// </summary>
    public void UpdateSystemParameters(WeatherProfile currentProfile, WeatherProfile nextProfile, float t)
    {
        if (IsSun)
        {
            MaxIntensity = Mathf.Lerp(currentProfile.SunMaxIntensity, nextProfile.SunMaxIntensity, t);
            _colorAtZenith = Color.Lerp(currentProfile.SunZenithColor, nextProfile.SunZenithColor, t);
            _colorAtSunset = Color.Lerp(currentProfile.SunSunsetColor, nextProfile.SunSunsetColor, t);
            Temperature = Mathf.Lerp(currentProfile.SunTemperature, nextProfile.SunTemperature, t);
        }
        else
        {
            MaxIntensity = Mathf.Lerp(currentProfile.MoonMaxIntensity, nextProfile.MoonMaxIntensity, t);
            _colorAtZenith = Color.Lerp(currentProfile.MoonZenithColor, nextProfile.MoonZenithColor, t);
            _colorAtSunset = Color.Lerp(currentProfile.MoonSunsetColor, nextProfile.MoonSunsetColor, t);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (EditorChangeTracker.IsPrefabInstance(this))
        {
            EditorChangeTracker.RegisterUndo(this, "Initialize and Validate Sun Light");
            InitializeAndValidateSystem();
            EditorChangeTracker.SetDirty(this);
        }
    }
#endif
}