using StatsModifiers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum WeightRange
{
    Acceptable = 0,
    Critical = 1,
    Ultimate = 2,
    UltimateImmovable = 3,
}

public enum ParameterType
{
    Health,
    Stamina,
    FoodBalance,
    WaterBalance,
    Energy,
    Heat,
    Toxicity,
    Capacity,
}

[CreateAssetMenu(fileName = "Player Parameters", menuName = "Scriptable Objects/Player Parameters File")]
public class PlayerParameters : ScriptableObject
{
    [field: Header("<size=16>- - General Parameters:</size>")]
    [field: SerializeField] public HealthParameter Health { get; private set; }
    [field: Space(10)]
    [field: SerializeField] public StaminaParameter Stamina { get; private set; }
    [field: Space(10)]
    [field: SerializeField] public CapacityParameter Capacity { get; private set; }

    [field: Header("<size=16>- - Status Parameters:</size>")]
    [field: SerializeField] public FoodBalanceParameter FoodBalance { get; private set; }
    [field: Space(10)]
    [field: SerializeField] public WaterBalanceParameter WaterBalance { get; private set; }
    [field: Space(10)]
    [field: SerializeField] public MovementParameter Energy { get; private set; }
    [field: Space(10)]
    [field: SerializeField] public HeatParameter Heat { get; private set; }
    [field: Space(10)]
    [field: SerializeField] public ToxicityParameter Toxicity { get; private set; }

    private Dictionary<ParameterType, IPlayerParameter> _parameterMap;

    public IEnumerable<IPlayerParameter> AllParameters => _parameterMap.Values.AsEnumerable();

    public void Initialize()
    {
        InitializeParameterMap();
        foreach (var parameter in AllParameters)
            parameter.Initialize();
    }

    private void InitializeParameterMap()
    {
        _parameterMap = new Dictionary<ParameterType, IPlayerParameter>()
        {
            { ParameterType.Health, Health },
            { ParameterType.Stamina, Stamina },
            { ParameterType.FoodBalance, FoodBalance },
            { ParameterType.WaterBalance, WaterBalance },
            { ParameterType.Energy, Energy },
            { ParameterType.Heat, Heat },
            { ParameterType.Toxicity, Toxicity },
            { ParameterType.Capacity, Capacity },
        };
    }

    // ������� ������ ��� ������� �����
    public IPlayerParameter GetParameter(ParameterType parameter)
    {
        return _parameterMap[parameter];
    }

    public void AddModifier(ParameterType parameter, StatModifier<ValueType> modifier)
    {
        if (_parameterMap.TryGetValue(parameter, out var param))
        {
            param.Mediator.AddModifier(modifier);
        }
    }

    public void ModifyParameter(ParameterType parameter, float value)
    {
        if (_parameterMap.TryGetValue(parameter, out var param))
        {
            param.Current += value;
        }
    }
}
