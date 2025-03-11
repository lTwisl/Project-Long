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
}

[CreateAssetMenu(fileName = "PlayerParameters", menuName = "Scriptable Objects/PlayerParameters")]
public class PlayerParameters : ScriptableObject
{
    [field: SerializeField] public HealthStatusParameter Health { get; private set; }

    [field: Space(10)]
    [field: SerializeField] public StaminaStatusParameter Stamina { get; private set; }


    [Header("LoadCapacity")]
    public float OffsetMaxLoadCapacity = 0;
    public readonly float[] RangeLoadCapacity = { 30, 45, 60 };
    public float CurrentLoad => _playerInventory.Weight;


    [field: Header("StatusParameters")]
    [field: SerializeField] public MovementStatusParameter FoodBalance { get; private set; }
    [field: SerializeField] public MovementStatusParameter WaterBalance { get; private set; }
    [field: SerializeField] public MovementStatusParameter Energy { get; private set; }
    [field: SerializeField] public StatusParameter Heat { get; private set; }
    [field: SerializeField] public StatusParameter Toxicity { get; private set; }

    Dictionary<ParameterType, BaseStatusParameter> _statusParameterCache;
    public IEnumerable<IStatusParameter> AllParameters => _statusParameterCache.Values.AsEnumerable();

    private Inventory _playerInventory;

    public void Init(Inventory playerInventory)
    {
        _statusParameterCache = new Dictionary<ParameterType, BaseStatusParameter>()
        {
            { ParameterType.Health, Health},
            { ParameterType.Stamina, Stamina},
            { ParameterType.FoodBalance, FoodBalance},
            { ParameterType.WaterBalance, WaterBalance},
            { ParameterType.Energy, Energy},
            { ParameterType.Heat, Heat},
            { ParameterType.Toxicity, Toxicity},
        };

        _playerInventory = playerInventory;
    }

    [ContextMenu("AllReset")]
    public void AllReset()
    {
        foreach (var parameter in AllParameters)
        {
            parameter.Reset();
        }

        OffsetMaxLoadCapacity = 0.0f;
    }

    public WeightRange GetCurrentWeightRange()
    {
        if (CurrentLoad < RangeLoadCapacity[0])
            return WeightRange.Acceptable;

        if (CurrentLoad < RangeLoadCapacity[1])
            return WeightRange.Critical;

        if (CurrentLoad < RangeLoadCapacity[2])
            return WeightRange.Ultimate;

        return WeightRange.UltimateImmovable;
    }

    public void Add(ParameterType parameter, float value)
    {
        _statusParameterCache[parameter].Current += value;
    }
}
