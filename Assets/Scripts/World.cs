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


    public WeatherSystem Weather { get; private set; }
    public WeatherWindSystem Wind => Weather.WindSystem;

    public event Action<ShelterSystem> OnEnterShelter;
    public event Action<ShelterSystem> OnExitShelter;

    private Player _player;

    private List<IExternalHeat> _externalHeats = new List<IExternalHeat>();
    private float _currentMaxExternalTemp;

    private void Awake()
    {
        Weather = GetComponentInChildren<WeatherSystem>();
        _player = FindAnyObjectByType<Player>();

        
    }

    private void OnEnable()
    {
        WorldTime.Instance.OnMinuteChanged += CalculateTotalTemperature;
    }

    private void OnDisable()
    {
        WorldTime.Instance.OnMinuteChanged -= CalculateTotalTemperature;
    }

    public void InvokeOnEnterShelter(ShelterSystem shelterSystem)
    {
        ShelterTemperature = shelterSystem.Temperature;
        ShelterWetness = shelterSystem.Wetness;
        ShelterToxicity = shelterSystem.Toxicity;

        CalculateTotalTemperature(WorldTime.Instance.CurrentTime);

        OnEnterShelter?.Invoke(shelterSystem);
    }

    public void InvokeOnExitShelter(ShelterSystem shelterSystem)
    {
        ShelterTemperature -= shelterSystem.Temperature;
        ShelterWetness -= shelterSystem.Wetness;
        ShelterToxicity -= shelterSystem.Toxicity;

        CalculateTotalTemperature(WorldTime.Instance.CurrentTime);

        OnExitShelter?.Invoke(shelterSystem);
    }

    public float GetWindLocalIntensity()
    {
        return Wind.GetWindLocalIntensity(_player.transform.position);
    }

    public Vector2 GetWindLocalVector()
    {
        return Wind.GetWindLocalVector(_player.transform.position);
    }

    private void CalculateTotalTemperature(TimeSpan _)
    {
        TotalTemperature = AreaTemperature + WeatherTemperature + ShelterTemperature + GetMaxExternalHeatsByPosiotion() + _player.ClothingSystem.TotalTemperatureBonus;
    }

    public float GetTotalToxicity()
    {
        return AreaToxicity + WeatherToxicity + ShelterToxicity + _player.ClothingSystem.TotalToxicityProtection;
    }

    /// <summary>
    /// Добавить внешний источник тепла и обновить максимальную температуру
    /// </summary>
    public void AddExternalHeat(IExternalHeat externalHeat)
    {
        _externalHeats.Add(externalHeat);

        if (externalHeat.Temp > _currentMaxExternalTemp)
            _currentMaxExternalTemp = externalHeat.Temp;
    }

    /// <summary>
    /// Удалить внешний источник тепла и пересчитать максимальную температуру
    /// </summary>
    public void RemoveExternalHeat(IExternalHeat externalHeat)
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

        return _externalHeats.Max(p => Utility.MapRange((p.Position - _player.transform.position).sqrMagnitude,
            Mathf.Pow(p.MinRadius, 2), Mathf.Pow(p.MaxRadius, 2), p.Temp, 0, true));
    }
}
