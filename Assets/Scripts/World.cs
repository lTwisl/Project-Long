using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class World : MonoBehaviour
{
    [field: Header("Температура")]
    [field: SerializeField] public float AreaTemperature { get; private set; }
    public float WeatherTemperature => Weather.Temperature;
    [field: SerializeField, DisableEdit] public float ShelterTemperature { get; private set; }
    [field: SerializeField, DisableEdit] public float TotalTemperature { get; private set; }

    [field: Header("Влажность")]
    [field: SerializeField] public float AreaWetness { get; private set; }
    public float WeatherWetness => Weather.Wetness;
    [field: SerializeField, DisableEdit] public float ShelterWetness { get; private set; }

    [field: Header("Заражённость")]
    [field: SerializeField] public float AreaToxicity { get; private set; }
    public float WeatherToxicity => Weather.Toxicity;
    [field: SerializeField, DisableEdit] public float ShelterToxicity { get; private set; }
    [field: SerializeField, DisableEdit] public float ZoneToxicity { get; private set; }
    [field: SerializeField, DisableEdit] public float TotalToxicity { get; private set; }


    public WeatherSystem Weather { get; private set; }
    public WeatherWindSystem Wind => Weather.WindSystem;

    public event Action<ShelterSystem> OnEnterShelter;
    public event Action<ShelterSystem> OnExitShelter;

    public event Action<HeatZone> OnEnterHeatZone;
    public event Action<HeatZone> OnExitHeatZone;

    public event Action<ToxicityZone> OnEnterToxicityZone;
    public event Action<ToxicityZone> OnExitToxicityZone;

    private Player _player;

    private List<HeatZone> _externalHeats = new List<HeatZone>();
    private float _currentMaxExternalTemp;

    private void Awake()
    {
        Weather = FindAnyObjectByType<WeatherSystem>();
        _player = FindAnyObjectByType<Player>();
    }

    private void OnEnable()
    {
        GameTime.OnMinuteChanged += HandleChangedMinute;
    }

    private void OnDisable()
    {
        GameTime.OnMinuteChanged -= HandleChangedMinute;
    }

    private void HandleChangedMinute()
    {
        CalculateTotalTemperature();
        CalculateTotalToxicity();
    }

    public void InvokeOnEnterShelter(ShelterSystem shelterSystem)
    {
        ShelterTemperature = shelterSystem.Temperature;
        ShelterWetness = shelterSystem.Wetness;
        ShelterToxicity = shelterSystem.Toxicity;

        CalculateTotalTemperature();
        CalculateTotalToxicity();

        OnEnterShelter?.Invoke(shelterSystem);
    }

    public void InvokeOnExitShelter(ShelterSystem shelterSystem)
    {
        ShelterTemperature -= shelterSystem.Temperature;
        ShelterWetness -= shelterSystem.Wetness;
        ShelterToxicity -= shelterSystem.Toxicity;

        CalculateTotalTemperature();
        CalculateTotalToxicity();

        OnExitShelter?.Invoke(shelterSystem);
    }

    public void InvokeOnEnterHeatZone(HeatZone heatZone)
    {
        AddExternalHeat(heatZone);
        OnEnterHeatZone?.Invoke(heatZone);
    }

    public void InvokeOnExitHeatZone(HeatZone heatZone)
    {
        RemoveExternalHeat(heatZone);
        OnExitHeatZone?.Invoke(heatZone);
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
        return Wind.GetWindLocalIntensity(_player.transform.position);
    }

    public Vector2 GetWindLocalVector()
    {
        return Wind.GetWindLocalVector(_player.transform.position);
    }

    private void CalculateTotalTemperature()
        => TotalTemperature = AreaTemperature + WeatherTemperature + ShelterTemperature + GetMaxExternalHeatsByPosiotion() + _player.ClothingSystem.TotalTemperatureBonus;


    public void CalculateTotalToxicity()
        => TotalToxicity = (AreaToxicity + WeatherToxicity + ShelterToxicity + ZoneToxicity) * (1 - _player.ClothingSystem.TotalToxicityProtection / 100);

    /// <summary>
    /// Добавить внешний источник тепла и обновить максимальную температуру
    /// </summary>
    private void AddExternalHeat(HeatZone externalHeat)
    {
        _externalHeats.Add(externalHeat);

        if (externalHeat.Temp > _currentMaxExternalTemp)
            _currentMaxExternalTemp = externalHeat.Temp;
    }

    /// <summary>
    /// Удалить внешний источник тепла и пересчитать максимальную температуру
    /// </summary>
    private void RemoveExternalHeat(HeatZone externalHeat)
    {
        _externalHeats.Remove(externalHeat);

        if (externalHeat.Temp == _currentMaxExternalTemp)
        {
            if (_externalHeats.Count == 0)
                _currentMaxExternalTemp = 0;
            else
                _currentMaxExternalTemp = _externalHeats.Max(p => p.Temp);
        }
    }

    /// <summary>
    /// Получить максимальное значение тепла с учетом расстояния до источников
    /// </summary>
    public float GetMaxExternalHeatsByPosiotion()
    {
        if (_externalHeats.Count == 0)
            return 0;

        return _externalHeats.Max(p => Utility.MapRange((p.transform.position - _player.transform.position).sqrMagnitude,
            Mathf.Pow(p.MinRadius, 2), Mathf.Pow(p.MaxRadius, 2), p.Temp, 0, true));
    }
}
