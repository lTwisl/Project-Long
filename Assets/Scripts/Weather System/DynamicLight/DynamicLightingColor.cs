using System;
using UnityEngine;

[ExecuteAlways]
public class DynamicLightingColor : MonoBehaviour
{
    [Header("��������� �����:")]
    public bool isSun = true;
    [SerializeField, Tooltip("������������ ������������� ��������� ���������"), Min(0)] private float _maxIntensity = 1.5f;
    [SerializeField, Tooltip("����������� ��������� ���������"), Min(0)] private float _temperature = 8000f;
    [Tooltip("���� ����� � �������")] public Color colorAtZenith = new Color(1f, 0.95f, 0.9f);
    [Tooltip("���� ����� �� ������")] public Color colorAtSunset = new Color(1f, 0.65f, 0.3f);

    [SerializeField, Tooltip("�������� ������� ��������� ��������� �������������"), Range(-1, 1)] private float _horizontOffset = 0.1f;
    [SerializeField, Tooltip("�������� ����� ��� �������� �������� (�� X'����' �� Y'���')")] private Vector2 _colorTransitionRangeAngles = new(90, 0);
    [SerializeField, Tooltip("����� ���������� ���������� �����"), Min(0)] private float _updateThreshold = 0.001f;

    [Header("��������� ����������� �����:")]
    [SerializeField, Tooltip("������������ ������������� ����������� ���������"), Range(0f, 2f)] private float _maxAmbientIntensity = 1f;
    [SerializeField, Tooltip("����������� ������������� ����������� ���������"), Range(0f, 1f)] private float _minAmbientIntensity = 0.3f;


    [Header("������ ��������:")]
    [SerializeField] private AnimationCurve _intensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)); // ������ ������������� �����
    [SerializeField] private AnimationCurve _colorBlendCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)); // ������ �������� ������
    [SerializeField] private AnimationCurve _ambientIntensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)); // ������ ������������� ����������� �����

    private Light _light;
    private float _lastDotProduct = float.MaxValue;
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
        InitializeLightComponent();
    }

    /// <summary>
    /// ������������� ���������� �����
    /// </summary>
    private void InitializeLightComponent()
    {
        if (_light == null)
        {
            _light = GetComponent<Light>();
            if (_light == null)
            {
                Debug.LogWarning("<color=orange>�������� ����� �� ������!</color>", this);
                enabled = false;
                return;
            }
        }

        // ��������� ���� � �������� dotProduct
        _colorTransitionRange = new Vector2(
            Mathf.Sin(_colorTransitionRangeAngles.x * Mathf.Deg2Rad),
            Mathf.Sin(_colorTransitionRangeAngles.y * Mathf.Deg2Rad)
        );
    }

    private void OnValidate()
    {
        InitializeLightComponent();
        UpdateLightingParameters();
    }

    private void Update()
    {
        if (ShouldUpdateLighting())
        {
            UpdateLightingParameters();
            CacheCurrentState();
            try
            {
                FindFirstObjectByType<WeatherSystem>().UserVolumFogMaterial.SetVector("_Sun_Direction", RenderSettings.sun.transform.forward);
                RenderSettings.skybox.SetVector("_Sun_Direction", RenderSettings.sun.transform.forward);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
        }
    }

    /// <summary>
    /// �������� ������������� ���������� ���������� �����
    /// </summary>
    /// <returns>True, ���� ��������� ����������</returns>
    private bool ShouldUpdateLighting()
    {
        return transform.hasChanged || Mathf.Abs(CalculateDotProduct() - _lastDotProduct) > _updateThreshold;
    }

    /// <summary>
    /// ���������� ���������� �����
    /// </summary>
    private void UpdateLightingParameters()
    {
        float dotProduct = CalculateDotProduct();
        float normalizedAngle = Mathf.Clamp((dotProduct + _horizontOffset), 0, 1);

        ChangeLightIntensity(normalizedAngle);
        ChangeLightColor(dotProduct);
        if (isSun)
            ChangeAmbientIntensity(normalizedAngle);
    }

    /// <summary>
    /// ���������� ���������� ������������ ��� ����������� ��������� ������
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
    private void ChangeLightIntensity(float normalizedAngle)
    {
        float curveValue = _intensityCurve.Evaluate(normalizedAngle);
        _light.intensity = Mathf.Lerp(0, _maxIntensity, curveValue);
    }

    /// <summary>
    /// ��������� ����� �����
    /// </summary>
    /// <param name="dotProduct">��������� ������������</param>
    private void ChangeLightColor(float dotProduct)
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
    private void ChangeAmbientIntensity(float normalizedAngle)
    {
        _light.colorTemperature = _temperature;
        float ambientCurveValue = _ambientIntensityCurve.Evaluate(normalizedAngle);
        RenderSettings.ambientIntensity = Mathf.Lerp(_minAmbientIntensity, _maxAmbientIntensity, ambientCurveValue);
    }

    /// <summary>
    /// ����������� �������� ��������� ��� �����������
    /// </summary>
    private void CacheCurrentState()
    {
        _lastDotProduct = CalculateDotProduct();
        transform.hasChanged = false;
    }
}