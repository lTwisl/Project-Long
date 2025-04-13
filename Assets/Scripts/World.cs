using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class World : MonoBehaviour
{
    [field: Header("Температура")]
    [field: SerializeField] public float Temperature { get; private set; }
    public float WeatherTemperature => Weather.Temperature;
    [field: SerializeField, DisableEdit] public float ShelterTemperature { get; private set; }
    [field: SerializeField, DisableEdit] public float TotalTemperature { get; private set; }

    [field: Header("Влажность")]
    [field: SerializeField] public float Wetness { get; private set; }
    public float WeatherWetness => Weather.Wetness;
    [field: SerializeField, DisableEdit] public float ShelterWetness { get; private set; }

    [field: Header("Заражённость")]
    [field: SerializeField] public float Toxicity { get; private set; }
    public float WeatherToxicity => Weather.Toxicity;
    [field: SerializeField, DisableEdit] public float ShelterToxicity { get; private set; }
    [field: SerializeField, DisableEdit] public float ZoneToxicity { get; private set; }
    [field: SerializeField, DisableEdit] public float TotalToxicity { get; private set; }


    public WeatherSystem Weather { get; private set; }
    public WeatherWindSystem Wind => Weather.WindSystem;

    public event Action<Shelter> OnEnterShelter;
    public event Action<Shelter> OnExitShelter;

    public event Action<TemperatureZone> OnEnterTemperatureZone;
    public event Action<TemperatureZone> OnExitTemperatureZone;

    public event Action<ToxicityZone> OnEnterToxicityZone;
    public event Action<ToxicityZone> OnExitToxicityZone;

    private Player _player;

    private List<TemperatureZone> _externalHeats = new List<TemperatureZone>();
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

    public void InvokeOnEnterShelter(Shelter shelterSystem)
    {
        ShelterTemperature = shelterSystem.Temperature;
        ShelterWetness = shelterSystem.Wetness;
        ShelterToxicity = shelterSystem.Toxicity;

        CalculateTotalTemperature();
        CalculateTotalToxicity();

        OnEnterShelter?.Invoke(shelterSystem);
    }

    public void InvokeOnExitShelter(Shelter shelterSystem)
    {
        ShelterTemperature -= shelterSystem.Temperature;
        ShelterWetness -= shelterSystem.Wetness;
        ShelterToxicity -= shelterSystem.Toxicity;

        CalculateTotalTemperature();
        CalculateTotalToxicity();

        OnExitShelter?.Invoke(shelterSystem);
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
        return Wind.GetWindLocalIntensity(_player.transform.position);
    }

    public Vector2 GetWindLocalVector()
    {
        return Wind.GetWindLocalVector(_player.transform.position);
    }

    private void CalculateTotalTemperature()
        => TotalTemperature = Temperature + WeatherTemperature + ShelterTemperature + GetMaxExternalHeatsByPosiotion() + _player.ClothingSystem.TotalTemperatureBonus;


    public void CalculateTotalToxicity()
        => TotalToxicity = (Toxicity + WeatherToxicity + ShelterToxicity + ZoneToxicity) * (1 - _player.ClothingSystem.TotalToxicityProtection / 100);

    /// <summary>
    /// Добавить внешний источник тепла и обновить максимальную температуру
    /// </summary>
    private void AddExternalHeat(TemperatureZone externalHeat)
    {
        _externalHeats.Add(externalHeat);

        if (externalHeat.Temp > _currentMaxExternalTemp)
            _currentMaxExternalTemp = externalHeat.Temp;
    }

    /// <summary>
    /// Удалить внешний источник тепла и пересчитать максимальную температуру
    /// </summary>
    private void RemoveExternalHeat(TemperatureZone externalHeat)
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
