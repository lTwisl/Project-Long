using System;
using UnityEngine;


[Serializable]
public class DecreasingStatusParameter : IStatusParameter
{
    public float Current { get; protected set; }
    public float Max { get; protected set; }
    public float DecreaseRate { get; protected set; }
    public event Action<float> OnValueChanged;

    public DecreasingStatusParameter(float max, float decreaseRate)
    {
        Max = max;
        Current = max;
        DecreaseRate = decreaseRate;
    }

    public virtual void UpdateParameter(float deltaTime)
    {
        float newValue = Mathf.Clamp(Current - DecreaseRate * deltaTime, 0f, Max);
        if (Mathf.Approximately(newValue, Current)) return;

        Current = newValue;
        OnValueChanged?.Invoke(Current);
    }

    public void Restore()
    {
        Current = Max;
        OnValueChanged?.Invoke(Current);
    }
}
