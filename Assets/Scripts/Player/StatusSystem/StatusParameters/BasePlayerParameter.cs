using StatsModifiers;
using System;
using UnityEngine;


[Serializable]
public class BasePlayerParameter : PlayerParameter
{
    public bool IsZero { get; private set; }
    public TimeSpan TimeIsZero { get; private set; }
    public TimeSpan TimeGeaterZero { get; private set; }

    public StatModifier<ModifiableValue> DecreasedHealthModifier { get; private set; }
    [field: SerializeField, Space(5), Tooltip("доли % в минуту")] public float DecreasedHealthRate { get; private set; }

    public event Action OnReachZero;
    public event Action OnRecoverFromZero;

    public override void Update(float deltaTime)
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
                TimeGeaterZero += TimeSpan.FromMinutes(deltaTime);
            }
        }
        else
        {
            if (IsZero)
            {
                TimeIsZero += TimeSpan.FromMinutes(deltaTime);
            }
            else
            {
                TimeGeaterZero = TimeSpan.Zero;
                IsZero = true;
                OnReachZero?.Invoke();
            }
        }

        base.Update(deltaTime);
    }

    public override void Initialize()
    {
        base.Initialize();

        DecreasedHealthModifier = new(0, ModifiableValue.ChangeRate, value => value += DecreasedHealthRate, $"DecreasedHealthModifier by {GetType().Name}");

        IsZero = false;
        TimeIsZero = TimeSpan.Zero;
        TimeGeaterZero = TimeSpan.Zero;
    }

    public override void Dispose()
    {
        base.Dispose();

        OnRecoverFromZero = null;
        OnReachZero = null;
    }
}



[Serializable]
public class PlayerParameter : IReadOnlyPlayerParameter
{
    public event Action<float> OnCurrentChanged;

    [Tooltip("Текущее значение")]
    [SerializeField, DisableEdit] private float _current;

    public float Current
    {
        get => _current;
        set
        {
            if (Mathf.Approximately(_current, value))
                return;

            if (ClampCurrentValue)
                _current = Mathf.Clamp(value, 0f, Mathf.Max(0, Max));
            else
                _current = value;

            OnCurrentChanged?.Invoke(_current);
        }
    }

    protected virtual bool ClampCurrentValue => true;

    [field: Tooltip("Максимальное значение"), Space(5)]
    [field: SerializeField] public float BaseMax { get; protected set; }
    [field: SerializeField, DisableEdit] public virtual float Max { get; private set; }

    [field: Tooltip("Скорость изменения [ед/м]"), DisableEdit, Space(5)]
    [field: SerializeField] public float BaseChangeRate { get; protected set; }
    [field: SerializeField, DisableEdit] public virtual float ChangeRate { get; private set; }

    public StatsMediator<ModifiableValue> Mediator { get; } = new();


    private float Request(ModifiableValue valueType, float value)
    {
        var q = new Query<ModifiableValue>(valueType, value);
        Mediator.PerformQuery(this, q);
        return q.Value;
    }

    public virtual void Initialize()
    {
        Current = Max;
        BaseChangeRate = 0.0f;
    }

    public virtual void Update(float deltaTime)
    {
        ChangeCurrent(deltaTime);

        Max = Request(ModifiableValue.Max, BaseMax);
        Max = Mathf.Max(0, Max);
        ChangeRate = Request(ModifiableValue.ChangeRate, BaseChangeRate);
    }

    public virtual void ChangeCurrent(float deltaTime)
    {
        Current = Current + ChangeRate * deltaTime;
    }

    public virtual void Dispose()
    {
        OnCurrentChanged = null;
    }
}

public interface IReadOnlyPlayerParameter : IDisposable
{
    public float Current { get; }
    public float BaseMax { get; }
    public float Max { get; }
    public float BaseChangeRate { get; }
    public float ChangeRate { get; }

    public event Action<float> OnCurrentChanged;
}
