using StatsModifiers;
using System;
using UnityEngine;


[CreateAssetMenu(fileName = "SatiationEffect", menuName = "Scriptable Objects/StatusEffect/Satiation")]
public class SatiationEffect : StatusEffect
{
    [Space(20)]
    [SerializeField] private int _activationDuration = 1; // Количесво дне для приобритения данного эффекта
    [SerializeField] private float _deactivationThreshold = 0.0f; // Значени ниже которого эффект снимается

    [SerializeField, Min(0)] private float _addMaxCapacity = 0.1f; // Процент от базового максимального значения 
    [SerializeField, Min(0)] private float _addMaxStamina = 0.1f; // процент от базового максимального значения 

    [NonSerialized] private StatModifier<ValueType> _statModifierCapacity = null;
    [NonSerialized] private StatModifier<ValueType> _statModifierStamina = null;

    public override void Init(Player player)
    {
        base.Init(player);

        GameTime.OnHourChanged += CheckOnApply;
    }

    public void CheckOnApply()
    {
        if (!IsActive && _player.Parameters.FoodBalance.TimeGeaterZero.Days >= _activationDuration &&
                _player.Parameters.WaterBalance.TimeGeaterZero.Days >= _activationDuration)
        {
            OnApply();
            GameTime.OnHourChanged -= CheckOnApply;
            GameTime.OnMinuteChanged += CheckOnRemove;
        }
    }

    public void CheckOnRemove()
    {
        if (IsActive && _player.Parameters.FoodBalance.Current <= _deactivationThreshold ||
            _player.Parameters.WaterBalance.Current <= _deactivationThreshold)
        {
            OnRemove();
            GameTime.OnMinuteChanged -= CheckOnRemove;
            GameTime.OnHourChanged += CheckOnApply;
        }
    }

    public override void OnApply()
    {
        IsActive = true;

        _statModifierCapacity = new(0, ValueType.Max, value =>
        {
            return value + _player.Parameters.Capacity.BaseMax * _addMaxCapacity;
        }, $"{GetType().Name}");

        _statModifierStamina = new(0, ValueType.Max, value =>
        {
            return value + _player.Parameters.Stamina.BaseMax * _addMaxStamina;
        }, $"{GetType().Name}");

        _player.Parameters.Capacity.Mediator.AddModifier(_statModifierCapacity);
        _player.Parameters.Stamina.Mediator.AddModifier(_statModifierStamina);
    }

    public override void OnRemove()
    {
        IsActive = false;
        _player = null;

        _statModifierCapacity.Dispose();
        _statModifierStamina.Dispose();
    }

    public override void Dispose()
    {
        OnRemove();

        GameTime.OnMinuteChanged -= OnRemove;
        GameTime.OnHourChanged -= CheckOnApply;
    }
}
