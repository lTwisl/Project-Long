using System;
using UnityEngine;


[Serializable]
public class BaseStatusParameter : StatusParameter
{
    public bool IsZero { get; private set; }
    public TimeSpan TimeIsZero { get; private set; }
    public TimeSpan TimeGeaterZero { get; private set; }
    [field: SerializeField] public float DecreasedHealthRate { get; private set; }

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
        IsZero = false;
        TimeIsZero = TimeSpan.Zero;
        TimeGeaterZero = TimeSpan.Zero;
    }

    public override void UnsubscribeAll()
    {
        base.UnsubscribeAll();
        OnReachZero = null;
        OnRecoverFromZero = null;
    }
}

[Serializable]
public class StatusParameter : IStatusParameter
{
    [Tooltip("Текущее значение")]
    [SerializeField, DisableEdit] private float _current;
    public float Current
    {
        get => _current;
        set
        {
            if (Mathf.Approximately(_current, value)) 
                return;
            _current = Mathf.Clamp(value, 0f, Max + OffsetMax);
            OnValueChanged?.Invoke(_current);
        }
    }

    [field: Tooltip("Максимальное значение")]
    [field: SerializeField] public float Max { get; set; }

    [field: Tooltip("Смещение максимального значение")]
    [field: SerializeField, DisableEdit] public float OffsetMax { get; set; }

    [field: Tooltip("Скорость изменения [ед/м]"), DisableEdit]
    [field: SerializeField] public float ChangeRate { get; set; }

    public event Action<float> OnValueChanged;

    public virtual void Reset()
    {
        OffsetMax = 0;
        Current = Max;
        ChangeRate = 0.0f;
    }

    public virtual void UpdateParameter(float deltaSeconds)
    {
        ChangeParameter(deltaSeconds);
    }

    public virtual void ChangeParameter(float deltaSeconds)
    {
        Current = Mathf.Clamp(Current + ChangeRate * deltaSeconds, 0f, Max + OffsetMax);
    }

    public virtual void UnsubscribeAll()
    {
        OnValueChanged = null;
    }
}

public interface IStatusParameter
{
    public float Current { get; }
    public float Max { get; }
    public float OffsetMax { get; }
    public float ChangeRate { get; }

    public event Action<float> OnValueChanged;

    public void UpdateParameter(float deltaSeconds);

    public void Reset();
}
