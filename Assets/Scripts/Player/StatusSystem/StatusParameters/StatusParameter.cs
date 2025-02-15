using System;
using UnityEngine;


[System.Serializable]
public class StatusParameter
{
    public float Current { get; protected set; }
    [field: SerializeField] public float Max { get; protected set; }
    [field: SerializeField] public float ChangeRate { get; protected set; }
    public bool IsZero { get; protected set; }

    public event Action<float> OnValueChanged;
    public event Action OnReachZero;
    public event Action OnRecoverFromZero;

    public virtual void UpdateParameter(float deltaTime)
    {
        float prevValue = Current;

        float newValue = Mathf.Clamp(Current + ChangeRate * deltaTime, 0f, Max);
        if (Mathf.Approximately(newValue, Current)) return;

        Current = newValue;
        OnValueChanged?.Invoke(Current);

        if (prevValue > 0 && Current <= 0)
        {
            IsZero = true;
            OnReachZero?.Invoke();
        }
        else if (prevValue <= 0 && Current > 0)
        {
            IsZero = false;
            OnRecoverFromZero?.Invoke();
        }
    }

    public void Reset()
    {
        Current = Max;
        IsZero = false;

        OnValueChanged?.Invoke(Current);
        OnRecoverFromZero?.Invoke();
    }
}






