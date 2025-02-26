using System.Collections.Generic;
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

    [field: Header("LoadCapacity")]
    public readonly float[] RangeLoadCapacity = { 30, 45, 60 };
    [field: SerializeField] public float CurrentLoad { get; set; } = 3;
    public float OffsetMaxLoadCapacity = 0;


    [field: Header("StatusParameters")]
    [field: SerializeField] public MovementStatusParameter FoodBalance { get; private set; }
    [field: SerializeField] public MovementStatusParameter WaterBalance { get; private set; }
    [field: SerializeField] public MovementStatusParameter Energy { get; private set; }
    [field: SerializeField] public StatusParameter Heat { get; private set; }
    [field: SerializeField] public StatusParameter Toxicity { get; private set; }


    private List<IStatusParameter> _allParameters = new();
    public IReadOnlyList<IStatusParameter> AllParameters => _allParameters;

    public void Init()
    {
        _allParameters.Clear();

        _allParameters.AddRange(new IStatusParameter[] {
            Health,
            Stamina,
            FoodBalance,
            WaterBalance,
            Energy,
            Heat,
            Toxicity
        });
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
}
