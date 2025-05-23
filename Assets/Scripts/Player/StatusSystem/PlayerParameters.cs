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
    [field: SerializeField] public HealthStatusParameter Health { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public StaminaStatusParameter Stamina { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public CapacityStatusParameter Capacity { get; private set; }

    [field: Header("StatusParameters")]
    [field: SerializeField] public FoodBalanceStatusParameter FoodBalance { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public WaterBalanceStatusParameter WaterBalance { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public MovementStatusParameter Energy { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public BaseStatusParameter Heat { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public ToxicityStatusParameter Toxicity { get; private set; }





    private Dictionary<ParameterType, StatusParameter> _statusParameterCache;
    public IEnumerable<IStatusParameter> AllParameters => _statusParameterCache.Values.AsEnumerable();

    public void Init(Inventory playerInventory)
    {
        _statusParameterCache = new Dictionary<ParameterType, StatusParameter>()
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
    }

    public void ModifyParameter(ParameterType parameter, float value)
    {
        _statusParameterCache[parameter].Current += value;
    }


    [ContextMenu("AllReset")]
    public void AllReset()
    {
        foreach (var parameter in AllParameters)
        {
            parameter.Reset();
        }
    }
}
