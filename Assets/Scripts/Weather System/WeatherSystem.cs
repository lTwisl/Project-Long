using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[DefaultExecutionOrder(-90)]
public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance { get; private set; }

    public event Action<WeatherProfile> OnWeatherTransitionStarted;
    public event Action<WeatherProfile> OnWeatherTransitionCompleted;

    [field: Header("������ Weather Profiles:")]
    [SerializeField] private List<WeatherProfile> weatherProfiles;

    [field: Header("������������ Weather Profiles:")]
    [DisableEdit, SerializeField] private bool _isProfilesValide = false;
    [field: SerializeField] public WeatherProfile CurrentWeatherProfile { get; private set; }
    [field: SerializeField] public WeatherProfile NewWeatherProfile { get; private set; }

    [field: Header("��������� ������� ������:")]
    [field: DisableEdit, SerializeField] public float Temperature { get; private set; }
    [field: DisableEdit, SerializeField] public float Wetness { get; private set; }
    [field: DisableEdit, SerializeField] public float Toxicity { get; private set; }

    [field: Header("��������� �����:")]
    [DisableEdit, SerializeField] private bool _isLightingSystemsValide = false;
    [field: SerializeField] public DynamicLightingColor SunLight { get; private set; }
    [field: SerializeField] public DynamicLightingColor MoonLight { get; private set; }

    [field: Header("������� �����:")]
    [DisableEdit, SerializeField] private bool _isWindSystemValide = false;
    [field: SerializeField] public WeatherWindSystem WindSystem { get; private set; }

    [field: Header("�������� �����:")]
    [DisableEdit, SerializeField] private bool _isFogSystemValide = false;
    [field: SerializeField] public WeatherFogSystem WeatherFogSystem { get; private set; }

    [field: Header("��������:")]
    [DisableEdit, SerializeField] private bool _isSkyboxSystemValide = false;
    [field: SerializeField] public WeatherSkyboxSystem WeatherSkyboxSystem { get; private set; }

    [field: Header("���� ����������:")]
    [DisableEdit, SerializeField] private bool _isPostProcessSystemValide = false;
    [field: SerializeField] public WeatherPostProcessSystem WeatherPostProcessSystem { get; private set; }

    [field: Header("���������� �������:")]
    [DisableEdit, SerializeField] private bool _isVFXValide = false;
    [field: SerializeField] public WeatherVFXSystem WeatherVFXSystem { get; private set; }


    private Process waitNextTransition;
    private Coroutine _transitionCoroutine;
    private readonly TimeSpan _transitionDuration = new(0, 1, 0, 0); // ����� �������� ����� ��������� ���������
    private TimeSpan _timeStartCurrentWeather = new(0, 0, 0, 0); // ����� ����� ������� ������ ��������
    private TimeSpan _timeEndCurrentWeather = new(0, 0, 0, 0); // ����� ����� ������� ������ ����������

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            ValidateReferences();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ContextMenu("[Script] ������������ �������")]
    /// <summary>
    /// �������� ������� � ������� ��� ����� �������� �������
    /// </summary>
    private void ValidateReferences()
    {
        _isProfilesValide = ValidateProfiles();
        _isLightingSystemsValide = SunLight != null && MoonLight != null && RenderSettings.sun == SunLight.GetComponent<Light>();
        _isWindSystemValide = WindSystem != null;
        _isFogSystemValide = WeatherFogSystem != null;
        _isSkyboxSystemValide = WeatherSkyboxSystem != null;
        _isPostProcessSystemValide = WeatherPostProcessSystem != null;
        _isVFXValide = WeatherVFXSystem != null;

        // ������ ��� �������:
        if (!_isProfilesValide) Debug.LogWarning("<color=orange>� ����� �� ���������������� ������� ������</color>", this);
        if (!_isLightingSystemsValide) Debug.LogWarning("<color=orange>�������� ������ �� WeatherLightingSystems</color>", this);
        if (!_isWindSystemValide) Debug.LogWarning("<color=orange>�������� ������ �� WeatherWindSystem</color>", this);
        if (!_isFogSystemValide) Debug.LogWarning("<color=orange>�������� ������ �� WeatherFogSystem</color>", this);
        if (!_isSkyboxSystemValide) Debug.LogWarning("<color=orange>�������� ������ �� WeatherSkyboxSystem</color>", this);
        if (!_isPostProcessSystemValide) Debug.LogWarning("<color=orange>�������� ������ �� WeatherPostProcessSystem</color>", this);
        if (!_isPostProcessSystemValide) Debug.LogWarning("<color=orange>�������� ������ �� WeatherVFXSystem</color>", this);
    }

    private bool ValidateProfiles()
    {
        return CurrentWeatherProfile != null && NewWeatherProfile != null;
    }

    private void Start()
    {
        SetNewWeatherImmediately(CurrentWeatherProfile);
    }

    /// <summary>
    /// ������� ������ ��������� �� �������������� �������
    /// </summary>
    /// <param name="weatherProfile"></param>
    public void SetNewWeatherImmediately(WeatherProfile weatherProfile)
    {
        if (weatherProfile == null)
        {
            Debug.LogError("������� ���������� null ������� ������!");
            return;
        }
        if (WorldTime.Instance == null)
        {
            Debug.LogError("World Time Instance ����������!");
            return;
        }

        if (CurrentWeatherProfile == null)
            CurrentWeatherProfile = weatherProfile;
        NewWeatherProfile = weatherProfile;

        OnWeatherTransitionStarted?.Invoke(NewWeatherProfile);
        StopWeatherTransition();
        WeatherVFXSystem.SpawnVFX(NewWeatherProfile, FindAnyObjectByType<Player>().transform);
        UpdateWeatherParameters(CurrentWeatherProfile, NewWeatherProfile, 1f);

        // ��������� �������
        CalculateWeatherProfiles();

        // ��������� ��������� �����
        CalculateTimeWeather();
        OnWeatherTransitionCompleted?.Invoke(CurrentWeatherProfile);
    }

    /// <summary>
    /// ������� ������ ������ �� �������������� �������
    /// </summary>
    /// <param name="weatherProfile"></param>
    public void SetNewWeatherWithTransition(WeatherProfile weatherProfile)
    {
        if (weatherProfile == null)
        {
            Debug.LogError("������� ���������� null ������� ������!");
            return;
        }
        if (WorldTime.Instance == null)
        {
            Debug.LogError("World Time Instance ����������!");
            return;
        }

        if (CurrentWeatherProfile == null)
            CurrentWeatherProfile = weatherProfile;
        NewWeatherProfile = weatherProfile;

        StopWeatherTransition();
        _transitionCoroutine = StartCoroutine(WeatherTransitionCoroutine());
    }

    public void StopWeatherTransition()
    {
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }
    }

    /// <summary>
    /// �������� ����������� ����� �������� �������
    /// </summary>
    /// <returns></returns>
    private IEnumerator WeatherTransitionCoroutine()
    {
        OnWeatherTransitionStarted?.Invoke(NewWeatherProfile);
        TimeSpan startTime = WorldTime.Instance.CurrentTime;
        //Debug.Log($"<color=yellow>�������� ����� ������! ������: {currentWeatherProfile.weatherIdentifier} �� {newWeatherProfile.weatherIdentifier}. ����� ������: {WorldTime.Instance.GetFormattedTime(startTime)}</color>");

        // ������� �������� ��������� �������� �������
        float t = 0f;
        WeatherVFXSystem.SpawnVFX(NewWeatherProfile, FindAnyObjectByType<Player>().transform);
        while (t < 1f)
        {
            TimeSpan passedTime = WorldTime.Instance.GetPassedTime(startTime);
            t = Mathf.Clamp01((float)(passedTime.TotalSeconds / _transitionDuration.TotalSeconds));
            UpdateWeatherParameters(CurrentWeatherProfile, NewWeatherProfile, t);
            yield return new WaitForEndOfFrame();
        }

        // ��������� �������
        CalculateWeatherProfiles();

        // ��������� ��������� �����
        CalculateTimeWeather();

        _transitionCoroutine = null;
        OnWeatherTransitionCompleted?.Invoke(CurrentWeatherProfile);
    }

    /// <summary>
    /// ��������� ��������� ������� ������
    /// </summary>
    private void CalculateWeatherProfiles()
    {
        CurrentWeatherProfile = NewWeatherProfile;
        //Debug.Log($"<color=green>����������� ����� ������! ������ �� �����: {currentWeatherProfile.weatherIdentifier}. ������� �����: {WorldTime.Instance.GetFormattedTime(WorldTime.Instance.CurrentTime)}</color>");

        List<WeatherProfile> availableTransitions = GetAvailableTransitions();
        if (availableTransitions.Count > 0)
        {
            // �������� ��������� ��������� ������ �� ���������
            NewWeatherProfile = availableTransitions[UnityEngine.Random.Range(0, availableTransitions.Count)];
            //Debug.Log($"<color=lightblue>��������� ������ �����: {newWeatherProfile.weatherIdentifier}</color>");
        }
        else
        {
            NewWeatherProfile = CurrentWeatherProfile;
            //Debug.LogWarning("<color=lightblue>��� ��������� �������� ���������. �������� ������� �������� �������.</color>");
        }
    }

    /// <summary>
    /// ����� ��� ��������� ��� �������� �������� �������
    /// </summary>
    /// <returns></returns>
    private List<WeatherProfile> GetAvailableTransitions()
    {
        List<WeatherProfile> availableProfiles = new List<WeatherProfile>();

        // �������� �� ���� �������� ����� �����, ���� ��������� ��� ��������
        foreach (WeatherProfile profile in weatherProfiles)
        {
            if (CurrentWeatherProfile.weatherTransitions.HasFlag((WeatherTransitions)profile.weatherIdentifier))
                availableProfiles.Add(profile);
        }
        return availableProfiles;
    }

    private void CalculateTimeWeather()
    {
        _timeStartCurrentWeather = WorldTime.Instance.CurrentTime;
        int lifetimeHours = UnityEngine.Random.Range(CurrentWeatherProfile.minLifetimeHours, CurrentWeatherProfile.maxLifetimeHours + 1);
        _timeEndCurrentWeather = _timeStartCurrentWeather + TimeSpan.FromHours(lifetimeHours);
        waitNextTransition = new Process(_timeEndCurrentWeather - _timeStartCurrentWeather,
            () => SetNewWeatherWithTransition(NewWeatherProfile),
            _ => Debug.Log($"<color=red>�������� �������� ������ ��������</color>"));
        waitNextTransition.Play();
        //Debug.Log($"<color=lightblue>������� ������ ���������� �: {WorldTime.Instance.GetFormattedTime(_timeEndCurrentWeather)}, ����� �������� ������� �� ������������� ������</color>");
    }

    private void UpdateWeatherParameters(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        ValidateReferences();
        UpdatePlayerInfluencePrameters(currentProfile, newProfile, t);
        if (_isLightingSystemsValide)
        {
            SunLight.UpdateLighting(currentProfile, newProfile, t);
            MoonLight.UpdateLighting(currentProfile, newProfile, t);
        }
        if (_isWindSystemValide) WindSystem.UpdateWind(currentProfile, newProfile, t);
        if (_isFogSystemValide) WeatherFogSystem.UpdateFog(currentProfile, newProfile, t);
        if (_isSkyboxSystemValide) WeatherSkyboxSystem.UpdateSkybox(currentProfile, newProfile, t);
        if (_isPostProcessSystemValide) WeatherPostProcessSystem.UpdatePostProcessing(currentProfile, newProfile, t);
        if (_isVFXValide) WeatherVFXSystem.UpdateVFX(t);
    }

    private void UpdatePlayerInfluencePrameters(WeatherProfile currentProfile, WeatherProfile newProfile, float t)
    {
        Temperature = Mathf.Lerp(currentProfile.temperature, NewWeatherProfile.temperature, t);
        Wetness = Mathf.Lerp(currentProfile.wetness, NewWeatherProfile.wetness, t);
        Toxicity = Mathf.Lerp(currentProfile.toxicity, NewWeatherProfile.toxicity, t);
    }

    private void OnDestroy()
    {
        // ������� ��� �� ����������� ������ ��������
        if (_transitionCoroutine != null) StopCoroutine(_transitionCoroutine);
    }

    #region ������� EDITOR
    // ������ ��� ������������� � ���������
    [ContextMenu("[Script] ���������������� ������ � �������� �� � ��������� �����")]
    private void FindReferences()
    {
#if UNITY_EDITOR
        Undo.RecordObject(this, "���������������� ������ WeatherSystem");
        EditorUtility.SetDirty(this);
#endif
        if (CurrentWeatherProfile != null)
        {
            Temperature = CurrentWeatherProfile.temperature;
            Wetness = CurrentWeatherProfile.wetness;
            Toxicity = CurrentWeatherProfile.toxicity;
        }

        // ����� ������� ������� � �����
        WeatherFogSystem = FindFirstObjectByType<WeatherFogSystem>();
        WeatherSkyboxSystem = FindFirstObjectByType<WeatherSkyboxSystem>();
        WindSystem = FindFirstObjectByType<WeatherWindSystem>();
        WeatherPostProcessSystem = FindFirstObjectByType<WeatherPostProcessSystem>();
        var dynamicLightingColor = FindObjectsByType<DynamicLightingColor>(FindObjectsSortMode.None);
        foreach (var dyn in dynamicLightingColor)
        {
            if (dyn.isSun)
                SunLight = dyn;
            else
                MoonLight = dyn;
        }

        ValidateReferences();
    }

    [ContextMenu("[Script] ���������������� � ����� currentWeather")]
    private void ConfigureWeather()
    {
#if UNITY_EDITOR
        Undo.RecordObject(this, "���������������� �����");
        EditorUtility.SetDirty(this);
#endif
        SetNewWeatherImmediatelyEditor(CurrentWeatherProfile);
    }


    /// <summary>
    /// ������� ������ ��������� �� �������������� �������
    /// </summary>
    /// <param name="weatherProfile"></param>
    private void SetNewWeatherImmediatelyEditor(WeatherProfile weatherProfile)
    {
        if (weatherProfile == null)
        {
            Debug.LogError("������� ���������� null ������� ������!");
            return;
        }

        if (CurrentWeatherProfile == null)
            CurrentWeatherProfile = weatherProfile;
        NewWeatherProfile = weatherProfile;

        UpdateWeatherParameters(CurrentWeatherProfile, NewWeatherProfile, 1f);
    }
    #endregion
}