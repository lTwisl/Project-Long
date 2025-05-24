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

[CreateAssetMenu(fileName = "PlayerParameters", menuName = "Scriptable Objects/PlayerParameters")]
public class PlayerParameters : ScriptableObject
{
    [field: SerializeField] public HealthParameter Health { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public StaminaParameter Stamina { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public CapacityParameter Capacity { get; private set; }

    [field: Header("<size=16>StatusParameters</size>")]
    [field: SerializeField] public FoodBalanceParameter FoodBalance { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public WaterBalanceParameter WaterBalance { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public MovementParameter Energy { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public BasePlayerParameter Heat { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public ToxicityParameter Toxicity { get; private set; }


    private Dictionary<ParameterType, PlayerParameter> _statusParameterCache;
    public IEnumerable<IPlayerParameter> AllParameters => _statusParameterCache.Values.AsEnumerable();

    public void Initialize(Inventory playerInventory)
    {
        _statusParameterCache = new Dictionary<ParameterType, PlayerParameter>()
        {
            { ParameterType.Health, Health},
            { ParameterType.Stamina, Stamina},
            { ParameterType.FoodBalance, FoodBalance},
            { ParameterType.WaterBalance, WaterBalance},
            { ParameterType.Energy, Energy},
            { ParameterType.Heat, Heat},
            { ParameterType.Toxicity, Toxicity},
            { ParameterType.Capacity, Capacity},
        };

        foreach (var parameter in AllParameters)
            parameter.Initialize();
    }

    public void ModifyParameter(ParameterType parameter, float value)
    {
        _statusParameterCache[parameter].Current += value;
    }
}
