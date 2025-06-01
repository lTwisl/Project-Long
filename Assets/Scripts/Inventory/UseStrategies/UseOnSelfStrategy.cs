using StatsModifiers;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[CreateAssetMenu(fileName = "UseOnSelfStrategy", menuName = "Use Item Strategy/UseOnSelfStrategy")]
public class UseOnSelfStrategy : UseStrategy
{
    [NonSerialized] private InventorySlot _usedSlot;
    [NonSerialized] private List<StatModifier<ValueType>> _statModifiers = new();
    [NonSerialized] private Dictionary<IPlayerParameter, bool> _parameterFlags = new();


    // �������� ������������� �������� �� ����.
    public override void Execute(InventorySlot slot)
    {
        if (_usedSlot != null || slot?.Item is not ReplenishingPlayerParameters consumables)
            return;

        _usedSlot = slot;
        InitializeModifiers(consumables);
    }


    // �������� ������������� ��������.
    public override void Cancel()
    {
        if (_usedSlot == null || _usedSlot.Item.MeasuredAsInteger)
            return;

        Clear();
    }


    // ����������� ������� � ���������� ���������.
    private void Clear()
    {
        foreach (var modifier in _statModifiers)
            modifier.Dispose();

        _statModifiers.Clear();
        _parameterFlags.Clear();
        _usedSlot = null;
    }


    // ������ ������������ ��� ���������� ��������.
    private void InitializeModifiers(ReplenishingPlayerParameters consumables)
    {
        foreach (var parameter in consumables.ReplenishmentParameters)
        {
            if (parameter.ParameterType == ParameterType.Capacity)
            {
                Debug.LogWarning($"������� {parameter.ParameterType} �� ������������ ParameterType.Capacity");
                continue;
            }

            var playerParameter = PlayerParameters.GetParameter(parameter.ParameterType);
            _parameterFlags.Add(playerParameter, false);

            var modifier = new StatModifier<ValueType>(0, ValueType.ChangeRate,
                value => UpdateParameterValue(playerParameter, parameter, value));

            playerParameter.Mediator.AddModifier(modifier);
            _statModifiers.Add(modifier);
        }
    }


    // ��������� �������� ��������� ������ �� ����� ������������� ��������.
    private float UpdateParameterValue(IPlayerParameter parameter, ReplenishmentParameter replenishment, float baseValue)
    {
        if (_usedSlot.IsEmpty)
        {
            Clear();
            return baseValue;
        }

        if (_usedSlot.Item.MeasuredAsInteger)
        {
            return HandleIntegerConsumable(parameter, replenishment);
        }

        return HandleContinuousConsumable(parameter, replenishment, baseValue);
    }


    // ������������ ������ ��� ������������� ���������.
    private float HandleIntegerConsumable(IPlayerParameter parameter, ReplenishmentParameter replenishment)
    {
        float currentCapacity = _usedSlot.Capacity;
        _usedSlot.Capacity -= 5 * GameTime.DeltaTime / 60f;

        if (Mathf.CeilToInt(currentCapacity) != Mathf.CeilToInt(_usedSlot.Capacity))
        {
            _usedSlot.Capacity = Mathf.Ceil(_usedSlot.Capacity);
            Clear();
        }

        return replenishment.Value;
    }


    // ������������ ������ ��� ����������� ���������.
    private float HandleContinuousConsumable(IPlayerParameter parameter, ReplenishmentParameter replenishment, float baseValue)
    {
        if (_parameterFlags[parameter] == false && parameter.Current >= parameter.Max)
        {
            _parameterFlags[parameter] = true;

            if (!_parameterFlags.Values.Contains(false))
            {
                Clear();
                return replenishment.Value;
            }
        }

        _usedSlot.Capacity -= _usedSlot.Item.CostOfUse * GameTime.DeltaTime / 60f;
        return baseValue + replenishment.Value;
    }
}