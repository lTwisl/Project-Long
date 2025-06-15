using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class World : MonoBehaviour
{
    [SerializeField] private float _degradationScaleOutside = 2f;
    [SerializeField] private float _scaleSpeedWetting = 0.1f; 

    [field: Header("�����������")]
    [field: SerializeField] public float Temperature { get; private set; }
    public float WeatherTemperature => Weather.Temperature;
    [field: SerializeField, DisableEdit] public float ShelterTemperature { get; private set; }
    [field: SerializeField, DisableEdit] public float TotalTemperature { get; private set; }
    public event Action<float> OnChangedTotalTemperature;

    [field: Header("���������")]
    [field: SerializeField, Range(0, 1)] public float Wetness { get; private set; }
    public float WeatherWetness => Weather.Wetness;
    [field: SerializeField, DisableEdit] public float ShelterWetness { get; private set; }
    [field: SerializeField, DisableEdit] public float TotalWetness { get; private set; }
    public event Action<float> OnChangedTotalWetness;

    [field: Header("�����������")]
    [field: SerializeField] public float Toxicity { get; private set; }
    public float WeatherToxicity => Weather.Toxicity;
    [field: SerializeField, DisableEdit] public float ShelterToxicity { get; private set; }
    [field: SerializeField, DisableEdit] public float ZoneToxicity { get; private set; }
    [field: SerializeField, DisableEdit] public float TotalToxicity { get; private set; }
    public event Action<float> OnChangedTotalToxicity;


    public WeatherSystem Weather { get; private set; }
    public WeatherWindSystem Wind => Weather?.WeatherWindSystem;


    public Shelter PlayerEnteredLastShelter { get; private set; }

    public event Action<Shelter> OnEnterShelter;
    public event Action<Shelter> OnExitShelter;

    public event Action<TemperatureZone> OnEnterTemperatureZone;
    public event Action<TemperatureZone> OnExitTemperatureZone;

    public event Action<ToxicityZone> OnEnterToxicityZone;
    public event Action<ToxicityZone> OnExitToxicityZone;

    private Player _player;

    private List<TemperatureZone> _externalHeats = new List<TemperatureZone>();
    private float _currentMaxExternalTemp;

    public float DegradationScale { get; private set; }

    private void Awake()
    {
        Weather = FindAnyObjectByType<WeatherSystem>();
        _player = FindAnyObjectByType<Player>();
    }

    private void OnEnable()
    {
        GameTime.OnTimeChanged += HandleChangedTime;
    }

    private void OnDisable()
    {
        GameTime.OnTimeChanged -= HandleChangedTime;
    }

    private void HandleChangedTime()
    {
        CalculateTotalTemperature();
        CalculateTotalToxicity();
        CalculateTotalWetness();
    }

    public void InvokeOnEnterShelter(Shelter shelterSystem)
    {
#if UNITY_EDITOR
        if (PlayerEnteredLastShelter != null)
            Debug.LogWarning($"����� ����� � ����� ������� \"{shelterSystem.gameObject.name}\", �� �� ����� �� ����������� \"{PlayerEnteredLastShelter.gameObject.name}\"");
#endif

        ShelterTemperature = shelterSystem.Temperature;
        ShelterWetness = shelterSystem.Wetness;
        ShelterToxicity = shelterSystem.Toxicity;

        DegradationScale = 1;

        //CalculateTotalTemperature();
        //CalculateTotalToxicity();

        PlayerEnteredLastShelter = shelterSystem;
        OnEnterShelter?.Invoke(shelterSystem);
    }

    public void InvokeOnExitShelter(Shelter shelterSystem)
    {
#if UNITY_EDITOR
        if (PlayerEnteredLastShelter != shelterSystem)
        {
            if (PlayerEnteredLastShelter != null)
                Debug.LogWarning($"����� �������� ����� �� ������� \"{shelterSystem.gameObject.name}\", �� ������ �� � ������� \"{PlayerEnteredLastShelter.gameObject.name}\"");
            else
                Debug.LogWarning($"����� �������� ����� �� ������� \"{shelterSystem.gameObject.name}\", �� �� �� ������ �� � ���� �������");
        }
#endif

        ShelterTemperature = 0;
        ShelterWetness = 0;
        ShelterToxicity = 0;

        DegradationScale = _degradationScaleOutside;

        //CalculateTotalTemperature();
        //CalculateTotalToxicity();

        OnExitShelter?.Invoke(shelterSystem);
        PlayerEnteredLastShelter = null;
    }

    public void InvokeOnEnterTemperatureZone(TemperatureZone heatZone)
    {
        AddExternalHeat(heatZone);
        OnEnterTemperatureZone?.Invoke(heatZone);
    }

    public void InvokeOnExitTemperatureZone(TemperatureZone heatZone)
    {
        RemoveExternalHeat(heatZone);
        OnExitTemperatureZone?.Invoke(heatZone);
    }

    public void InvokeOnEnterToxicityZone(ToxicityZone toxicityZone)
    {
        if (toxicityZone.CurrentType == ToxicityZone.ZoneType.Rate)
        {
            ZoneToxicity += toxicityZone.Toxicity;
            CalculateTotalToxicity();
        }

        OnEnterToxicityZone?.Invoke(toxicityZone);
    }

    public void InvokeOnExitToxicityZone(ToxicityZone toxicityZone)
    {
        if (toxicityZone.CurrentType == ToxicityZone.ZoneType.Rate)
        {
            ZoneToxicity -= toxicityZone.Toxicity;
            CalculateTotalToxicity();
        }

        OnExitToxicityZone?.Invoke(toxicityZone);
    }

    public float GetWindLocalIntensity()
    {
        if (PlayerEnteredLastShelter)
            return 0;
        else
            return Wind.GetWindLocalIntensity(_player.transform.position);

    }

    public Vector2 GetWindLocalVector()
    {
        if (PlayerEnteredLastShelter)
            return Vector2.zero;
        else
            return Wind.GetWindLocalVector(_player.transform.position);
    }

    private void CalculateTotalTemperature()
    {
        float WeatherOrShalter = PlayerEnteredLastShelter ? ShelterTemperature : WeatherTemperature;

        TotalTemperature = Temperature + WeatherOrShalter + GetMaxExternalHeatsByPosiotion();
        OnChangedTotalTemperature?.Invoke(TotalTemperature);
    }


    public void CalculateTotalToxicity()
    {
        float WeatherOrShalter = PlayerEnteredLastShelter ? ShelterToxicity : WeatherToxicity;

        TotalToxicity = Toxicity + WeatherOrShalter + ZoneToxicity;
        OnChangedTotalToxicity?.Invoke(TotalToxicity);
    }


    public void CalculateTotalWetness()
    {
        float WeatherOrShalter = PlayerEnteredLastShelter ? ShelterWetness : WeatherWetness;

        TotalWetness = Mathf.Clamp01(Wetness + WeatherOrShalter) * _scaleSpeedWetting;
        OnChangedTotalWetness?.Invoke(TotalWetness);
    }

    /// <summary>
    /// �������� ������� �������� ����� � �������� ������������ �����������
    /// </summary>
    private void AddExternalHeat(TemperatureZone externalHeat)
    {
        _externalHeats.Add(externalHeat);

        if (externalHeat.Temperature > _currentMaxExternalTemp)
            _currentMaxExternalTemp = externalHeat.Temperature;
    }

    /// <summary>
    /// ������� ������� �������� ����� � ����������� ������������ �����������
    /// </summary>
    private void RemoveExternalHeat(TemperatureZone externalHeat)
    {
        _externalHeats.Remove(externalHeat);

        if (externalHeat.Temperature == _currentMaxExternalTemp)
        {
            if (_externalHeats.Count == 0)
                _currentMaxExternalTemp = 0;
            else
                _currentMaxExternalTemp = _externalHeats.Max(p => p.Temperature);
        }
    }

    /// <summary>
    /// �������� ������������ �������� ����� � ������ ���������� �� ����������
    /// </summary>
    public float GetMaxExternalHeatsByPosiotion()
    {
        if (_externalHeats.Count == 0)
            return 0;

        return _externalHeats.Max(p => Utility.MapRange((p.transform.position - _player.transform.position).sqrMagnitude,
            Mathf.Pow(p.MinRadius, 2), Mathf.Pow(p.MaxRadius, 2), p.Temperature, 0, true));
    }
}
