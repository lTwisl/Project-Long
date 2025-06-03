using System;
using UnityEditor;
using UnityEngine;

public class WeatherLightingColor : MonoBehaviour, IWeatherSystem
{
    [SerializeField, DisableEdit] private bool _isSystemValid;
    public bool IsSystemValid => _isSystemValid;

    [field: Header("- - ��������� ��������� �����:")]
    [field: SerializeField] public bool IsSun { get; private set; } = true;

    [SerializeField, Tooltip("���� ����� � �������")] private Color _colorAtZenith = new(1f, 0.95f, 0.9f);
    [SerializeField, Tooltip("���� ����� �� ������")] private Color _colorAtSunset = new(1f, 0.5f, 0f);

    [SerializeField, Min(0)] private float _maxIntensity = 3f;
    [HideIf(nameof(IsSun), false)]
    [SerializeField, Min(0)] private float _temperature = 8000f;

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
    [SerializeField] private AnimationCurve _colorBlendCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
    [HideIf(nameof(IsSun), false)]
    [SerializeField] private AnimationCurve _ambientIntensityCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));

    [SerializeField, DisableEdit] private Light _light;
    public Light GetLight => _light;
    private Vector2 _colorTransitionRange;

    public float MaxIntensity
    {
        get => _maxIntensity;
        set => _maxIntensity = Mathf.Clamp(value, 0, 5);
    }

    public float Temperature
    {
        get => _temperature;
        set => _temperature = Mathf.Clamp(value, 0, 20000);
    }

    private void Awake()
    {
        GameTime.OnTimeChanged += UpdateLightingParameters;
    }

    public void InitializeAndValidateSystem()
    {
        _light ??= GetComponent<Light>();
        if (!_light)
        {
            _isSystemValid = false;
            return;
        }

        // ��������� ���� � �������� dotProduct
        _colorTransitionRange = new Vector2(
            Mathf.Sin(_colorTransitionRangeAngles.x * Mathf.Deg2Rad),
            Mathf.Sin(_colorTransitionRangeAngles.y * Mathf.Deg2Rad)
        );

        _isSystemValid = true;
    }

    private void Update()
    {
        UpdateLightingParameters();
    }

    /// <summary>
    /// ������� � ��������� ���������� ����� �� ���������� �������
    /// </summary>
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
        _light.intensity = Mathf.Lerp(0, _maxIntensity, curveValue);
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
        float colorBlend = _colorBlendCurve.Evaluate(normalizedDot);

        // ������������� ���� ����� ������� � ����������
        _light.color = Color.Lerp(_colorAtZenith, _colorAtSunset, colorBlend);
    }

    /// <summary>
    /// ��������� ������������� ����������� ���������
    /// </summary>
    /// <param name="normalizedAngle">��������������� ���� ����������� ��������� �����</param>
    private void SetAmbientIntensity(float normalizedAngle)
    {
        _light.colorTemperature = _temperature;
        float ambientCurveValue = _ambientIntensityCurve.Evaluate(normalizedAngle);
        RenderSettings.ambientIntensity = Mathf.Lerp(_minAmbientIntensity, _maxAmbientIntensity, ambientCurveValue);
    }

    /// <summary>
    /// �������� ��������� ������� ���������
    /// </summary>
    public void UpdateSystemParameters(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        if (IsSun)
        {
            MaxIntensity = Mathf.Lerp(currentProfile.SunMaxIntensity, newProfile.SunMaxIntensity, t);
            _colorAtZenith = Color.Lerp(currentProfile.SunZenithColor, newProfile.SunZenithColor, t);
            _colorAtSunset = Color.Lerp(currentProfile.SunSunsetColor, newProfile.SunSunsetColor, t);
            Temperature = Mathf.Lerp(currentProfile.SunTemperature, newProfile.SunTemperature, t);
        }
        else
        {
            MaxIntensity = Mathf.Lerp(currentProfile.MoonMaxIntensity, newProfile.MoonMaxIntensity, t);
            _colorAtZenith = Color.Lerp(currentProfile.MoonZenithColor, newProfile.MoonZenithColor, t);
            _colorAtSunset = Color.Lerp(currentProfile.MoonSunsetColor, newProfile.MoonSunsetColor, t);
        }
    }

    private void OnDestroy()
    {
        GameTime.OnTimeChanged -= UpdateLightingParameters;
    }

#if UNITY_EDITOR
    private Quaternion _oldRotation;

    private void OnValidate()
    {
        // 0. �� ����������, ���� ��� ������-����� (�� ���������)
        if (PrefabUtility.IsPartOfPrefabAsset(this)) return;

        // 1. ������������� �������������� � ���������� ������� � ���������
        InitializeAndValidateSystem();

        // 2. ��������� �������� ��� �������
        if (PrefabUtility.IsPartOfPrefabInstance(this))
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }

    public void UpdateEnviromentLight()
    {
        if (transform.rotation == _oldRotation) return;

        _oldRotation = transform.rotation;
        DynamicGI.UpdateEnvironment();
        SceneView.RepaintAll();
    }
#endif
}