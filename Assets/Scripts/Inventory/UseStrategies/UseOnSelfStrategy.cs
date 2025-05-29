using StatsModifiers;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[CreateAssetMenu(fileName = "UseOnSelfStrategy", menuName = "Use Item Strategy/UseOnSelfStrategy")]
public class UseOnSelfStrategy : UseStrategy
{
    [NonSerialized] private InventorySlot _usedSlot;
    [NonSerialized] private List<StatModifier> _statModifiers = new();
    [NonSerialized] private Dictionary<IPlayerParameter, bool> _keyValuePairs = new();

    public override void Execute(InventorySlot slot)
    {
        if (_usedSlot != null)
            return;

        if (slot?.Item is not ReplenishingPlayerParameters consumables)
            return;

        _usedSlot = slot;

        if (_playerParameters == null)
            _playerParameters = _player.GetComponent<PlayerStatusManager>().PlayerParameters;


        foreach (var parameter in consumables.ReplenishmentParameters)
        {
            if (parameter.ParameterType == ParameterType.Capacity)
            {
                Debug.LogWarning($"Предмет {slot.Item.Name} пытается начислить ParameterType.Capacity");
                continue;
            }

            IPlayerParameter p = _playerParameters.GetParameter(parameter.ParameterType);
            _keyValuePairs.Add(p, false);

            _statModifiers.Add(new(0, new ParameterTypeCondition(ValueType.ChangeRate), value =>
            {
                if (slot.IsEmpty)
                {
                    Clear();
                    return value;
                }

                if (slot.Item.MeasuredAsInteger)
                {
                    float current = slot.Capacity;
                    slot.Capacity -= 5 * GameTime.DeltaTime / 60f;

                    if (Mathf.CeilToInt(current) != Mathf.CeilToInt(slot.Capacity))
                    {
                        slot.Capacity = Mathf.Ceil(slot.Capacity);
                        Clear();
                        return value;
                    }
                }
                else
                {
                    if (_keyValuePairs[p] == false && p.Current >= p.Max)
                    {
                        _keyValuePairs[p] = true;

                        if (!_keyValuePairs.Values.Contains(false))
                        {
                            Clear();
                            return value;
                        }
                    }

                    slot.Capacity -= slot.Item.CostOfUse * GameTime.DeltaTime / 60f;
                }

                return value + parameter.Value;
            }));

            p.Mediator.AddModifier(_statModifiers[^1]);
        }
    }

    public override void Cancel()
    {
        if (_usedSlot == null)
            return;

        if (_usedSlot.Item.MeasuredAsInteger)
            return;

        Clear();
    }

    private void Clear()
    {
        foreach (var m in _statModifiers)
            m.Dispose();

        _statModifiers.Clear();
        _keyValuePairs.Clear();
        _usedSlot = null;
    }
}
