using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class CapacityParameter : BasePlayerParameter
{
    protected override bool ClampCurrentValue => false;

    private readonly Dictionary<WeightRange, float> _rangeLoadCapacity = new Dictionary<WeightRange, float>()
    {
        { WeightRange.Acceptable, 0},
        { WeightRange.Critical, 30},
        { WeightRange.Ultimate, 45},
        { WeightRange.UltimateImmovable, 60},
    };

    public float GetRangeLoadCapacity(WeightRange weightRange) => _rangeLoadCapacity[weightRange] + (Max - BaseMax);
    public bool IsCanWalk() => Current < GetRangeLoadCapacity(WeightRange.UltimateImmovable);
    public bool IsCanSprint() => Current < GetRangeLoadCapacity(WeightRange.Ultimate);

    public WeightRange GetCurrentWeightRange()
    {
        if (Current < GetRangeLoadCapacity(WeightRange.Critical))
            return WeightRange.Acceptable;

        if (Current < GetRangeLoadCapacity(WeightRange.Ultimate))
            return WeightRange.Critical;

        if (Current < GetRangeLoadCapacity(WeightRange.UltimateImmovable))
            return WeightRange.Ultimate;

        return WeightRange.UltimateImmovable;
    }

    public override void Initialize()
    {
        Current = 0;
        BaseMax = _rangeLoadCapacity.Values.Max();
        OffsetMax = 0;
    }
}
