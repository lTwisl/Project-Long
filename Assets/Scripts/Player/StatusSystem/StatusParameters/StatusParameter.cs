using System;
using UnityEngine;


[Serializable]
public class StatusParameter : BaseStatusParameter
{
    public bool IsZero { get; private set; }
    public TimeSpan TimeIsZero { get; private set; }
    public TimeSpan TimeGeaterZero { get; private set; }

    public event Action OnReachZero;
    public event Action OnRecoverFromZero;

    public override void UpdateParameter(float deltaSeconds)
    {
        float prevValue = Current;

        base.UpdateParameter(deltaSeconds);

        if (Current <= 0)
        {
            TimeIsZero += TimeSpan.FromSeconds(deltaSeconds);

            if (prevValue > 0)
            {
                TimeGeaterZero = TimeSpan.Zero;
                IsZero = true;
                OnReachZero?.Invoke();
            }
        }
        else
        {
            TimeGeaterZero += TimeSpan.FromSeconds(deltaSeconds);

            if (prevValue <= 0)
            {
                TimeIsZero = TimeSpan.Zero;
                IsZero = false;
                OnRecoverFromZero?.Invoke();
            }
        }
    }

    public override void Reset()
    {
        base.Reset();
        TimeIsZero = TimeSpan.Zero;
        TimeGeaterZero = TimeSpan.Zero;
    }
}

[Serializable]
public class BaseStatusParameter : IStatusParameter
{
    [SerializeField] private float _current;
    public float Current 
    {
        get => _current;
        set
        {
            if (Mathf.Approximately(_current, value)) 
                return;
            _current = value;
            OnValueChanged?.Invoke(_current);
        }
    }
    [field: SerializeField] public float Max { get; set; }
    [field: SerializeField] public float OffsetMax { get; set; }
    [field: SerializeField] public float ChangeRate { get; set; }

    public event Action<float> OnValueChanged;

    public virtual void Reset()
    {
        Current = Max + OffsetMax;
    }

    public virtual void UpdateParameter(float deltaSeconds)
    {
        Current = Mathf.Clamp(Current + ChangeRate * deltaSeconds, 0f, Max + OffsetMax);
    }
}

public interface IStatusParameter
{
    public float Current { get; set; }
    public float Max { get; set; }
    public float OffsetMax { get; set; }
    public float ChangeRate { get; set; }

    public event Action<float> OnValueChanged;

    public void UpdateParameter(float deltaSeconds);

    public void Reset();
}
