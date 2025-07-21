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
    [NonSerialized] private Dictionary<IReadOnlyPlayerParameter, bool> _parameterFlags = new();

    float initCapasity = 0;

    // Начинает использование предмета на себе.
    public override void Execute(InventorySlot slot)
    {
        if (_usedSlot != null || slot?.Item is not IGiverPlayerParameters consumables)
            return;

        _usedSlot = slot;
        initCapasity = slot.Capacity;

        InitializeModifiers(consumables);
    }


    // Отменяет использование предмета.
    public override void Cancel()
    {
        if (_usedSlot == null || _usedSlot.Item.MeasuredAsInteger)
            return;

        SetLastUsedItem();

        Clear();
    }


    // Освобождает ресурсы и сбрасывает состояние.
    private void Clear()
    {
        foreach (var modifier in _statModifiers)
            modifier.Dispose();

        _statModifiers.Clear();
        _parameterFlags.Clear();
        _usedSlot = null;
    }


    // Создаёт модификаторы для параметров предмета.
    private void InitializeModifiers(IGiverPlayerParameters consumables)
    {
        foreach (var parameter in consumables.GivesParameters)
        {
            if (parameter.ParameterType == ParameterType.Capacity)
            {
                Debug.LogWarning($"Предмет {parameter.ParameterType} не поддерживает ParameterType.Capacity");
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


    // Обновляет значение параметра игрока во время использования предмета.
    private float UpdateParameterValue(IReadOnlyPlayerParameter parameter, GivesParameter replenishment, float baseValue)
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


    // Обрабатывает логику для целочисленных предметов.
    private float HandleIntegerConsumable(IReadOnlyPlayerParameter parameter, GivesParameter replenishment)
    {
        float currentCapacity = _usedSlot.Capacity;
        _usedSlot.Capacity -= 5 * GameTime.DeltaTime / 60f;

        if (Mathf.CeilToInt(currentCapacity) != Mathf.CeilToInt(_usedSlot.Capacity))
        {
            _usedSlot.Capacity = Mathf.Ceil(_usedSlot.Capacity);
            //Debug.Log($"Usage completed. Used capasity = {initCapasity - _usedSlot.Capacity}");
            SetLastUsedItem();
            Clear();
        }

        return replenishment.Value;
    }

    private void SetLastUsedItem()
    {
        _player.StatusEffectsManeger.LastUsedItem = new()
        {
            item = _usedSlot.Item,
            capacity = initCapasity - _usedSlot.Capacity,
            condition = (float)_usedSlot.Condition,
            timeUsed = GameTime.Time
        };
    }


    // Обрабатывает логику для непрерывных предметов.
    private float HandleContinuousConsumable(IReadOnlyPlayerParameter parameter, GivesParameter replenishment, float baseValue)
    {
        if (_parameterFlags[parameter] == false && parameter.Current >= parameter.Max)
        {
            _parameterFlags[parameter] = true;

            if (!_parameterFlags.Values.Contains(false))
            {
                //Debug.Log($"Usage completed. Used capasity = {initCapasity - _usedSlot.Capacity}");
                SetLastUsedItem();
                Clear();
                return replenishment.Value;
            }
        }

        _usedSlot.Capacity -= _usedSlot.Item.CostOfUse * GameTime.DeltaTime / 60f;
        return baseValue + replenishment.Value;
    }
}