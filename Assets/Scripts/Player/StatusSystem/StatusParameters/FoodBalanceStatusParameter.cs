using UnityEngine;

[System.Serializable]
public class FoodBalanceStatusParameter : MovementStatusParameter
{
    private float _changeRateRatioByCapacity = 1;
    public float ChangeRateRatioByCapacity
    {
        get => _changeRateRatioByCapacity;
        set => _changeRateRatioByCapacity = Mathf.Clamp01(value);
    }

    public override void ChangeParameter(float deltaSeconds)
    {
        float newChangeRate = ChangeRate * (ChangeRate > 0 ? ChangeRateRatioByCapacity : (2 - ChangeRateRatioByCapacity));
        Current = Mathf.Clamp(Current + newChangeRate * deltaSeconds, 0f, Max + OffsetMax);
    }

    public override void Reset()
    {
        base.Reset();
        ChangeRateRatioByCapacity = 1;
    }
}