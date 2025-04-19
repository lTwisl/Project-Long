using UnityEngine;

[System.Serializable]
public class StaminaStatusParameter : MovementStatusParameter
{
    [Tooltip("Штраф за достижения нуля [м]")]
    [SerializeField] private float _reload;

    private float _changeRateRatioByCapacity = 1;
    public float ChangeRateRatioByCapacity 
    {
        get => _changeRateRatioByCapacity;
        set => _changeRateRatioByCapacity = Mathf.Clamp(value, _minChangeRateRatioByCapacity, _maxChangeRateRatioByCapacity);
    }

    private const float _minChangeRateRatioByCapacity = 1;
    private const float _maxChangeRateRatioByCapacity = 3;

    private float _timer = 0f;

    public override void UpdateParameter(float deltaTime)
    {
        if (_timer >= 0)
        {
            _timer -= deltaTime;
            return;
        }

        base.UpdateParameter(deltaTime);

        if (Current <= 0)
            _timer = _reload;
    }

    public override void ChangeParameter(float deltaSeconds)
    {
        float newChangeRate = ChangeRate;
        if (ChangeRate > 0)
            newChangeRate *= Utility.MapRange(ChangeRateRatioByCapacity, _minChangeRateRatioByCapacity, _maxChangeRateRatioByCapacity, 1, 0, true);
        else
            newChangeRate *= ChangeRateRatioByCapacity;

        Current = Mathf.Clamp(Current + newChangeRate * deltaSeconds, 0f, Max + OffsetMax);
    }

    public override void Reset()
    {
        base.Reset();
        ChangeRateRatioByCapacity = 1;
    }
}

