using EditorAttributes;
using StatsModifiers;
using System;
using UnityEngine;


[Serializable]
public class BasePlayerParameter : PlayerParameter
{
    public bool IsZero { get; private set; }
    public TimeSpan TimeIsZero { get; private set; }
    public TimeSpan TimeGeaterZero { get; private set; }

    public StatModifier DecreasedHealthModifier { get; private set; }
    [field: SerializeField, Space(5)] public float DecreasedHealthRate { get; private set; }

    public event Action OnReachZero;
    public event Action OnRecoverFromZero;

    public override void UpdateParameter(float deltaSeconds)
    {

        if (Current > 0)
        {
            if (IsZero)
            {
                TimeIsZero = TimeSpan.Zero;
                IsZero = false;
                OnRecoverFromZero?.Invoke();
            }
            else
            {
                TimeGeaterZero += TimeSpan.FromSeconds(deltaSeconds);
            }
        }
        else
        {
            if (IsZero)
            {
                TimeIsZero += TimeSpan.FromSeconds(deltaSeconds);
            }
            else
            {
                TimeGeaterZero = TimeSpan.Zero;
                IsZero = true;
                OnReachZero?.Invoke();
            }
        }

        base.UpdateParameter(deltaSeconds);
    }

    public override void Initialize()
    {
        base.Initialize();

        DecreasedHealthModifier = new(0, new ParameterTypeCondition(ValueType.ChangeRate), value => value += DecreasedHealthRate);

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
public class PlayerParameter : IPlayerParameter
{
    public event Action<float> OnValueChanged;

    [Tooltip("Текущее значение")]
    [SerializeField, DisableField] private float _current;
    public float Current
    {
        get => _current;
        set
        {
            if (Mathf.Approximately(_current, value))
                return;

            if (ClampCurrentValue)
                _current = Mathf.Clamp(value, 0f, Max);
            else
                _current = value;

            OnValueChanged?.Invoke(_current);
        }
    }

    protected virtual bool ClampCurrentValue => true;

    [field: Tooltip("Максимальное значение"), Space(5)]
    [field: SerializeField] public float BaseMax { get; protected set; }
    [field: SerializeField, DisableField] public virtual float Max { get; private set; }

    [field: Tooltip("Скорость изменения [ед/м]"), DisableField, Space(5)]
    [field: SerializeField] public float BaseChangeRate { get; set; }
    [field: SerializeField, DisableField] public virtual float ChangeRate { get; private set; }

    public float OffsetMax { get; set; }

    public StatsMediator Mediator { get; } = new();


    private float Request(ValueType valueType, float value)
    {
        var q = new Query(new ParameterTypeCondition(valueType), value);
        Mediator.PerformQuery(this, q);
        return q.Value;
    }

    public virtual void Initialize()
    {
        OffsetMax = 0;
        Current = Max;
        BaseChangeRate = 0.0f;
    }

    public virtual void UpdateParameter(float deltaSeconds)
    {
        ChangeParameter(deltaSeconds);

        Max = Request(ValueType.Max, BaseMax);
        ChangeRate = Request(ValueType.ChangeRate, BaseChangeRate);
    }

    public virtual void ChangeParameter(float deltaSeconds)
    {
        Current = Current + ChangeRate * deltaSeconds;
    }

    public virtual void UnsubscribeAll()
    {
        OnValueChanged = null;
    }
}

public interface IPlayerParameter
{
    public float Current { get; }
    public float Max { get; }
    public float OffsetMax { get; }
    public float BaseChangeRate { get; }

    public event Action<float> OnValueChanged;

    public StatsMediator Mediator { get; }

    public void UpdateParameter(float deltaSeconds);

    public void Initialize();
}
