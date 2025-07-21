using StatsModifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "BloodPoisoningEffect", menuName = "Scriptable Objects/StatusEffect/BloodPoisoning")]
public class BloodPoisoningEffect : StatusEffect
{
    [SerializeField, Min(1)] private int _requiredCountHealing = 1; // Требуемое количество использованных лекарств подряд
    [SerializeField, Min(0.001f)] private float _minHour = 1;
    [SerializeField, Min(0.001f)] private float _maxHour = 4;

    [SerializeField] private List<Receipt> _healingReceipts = new();

    private int _countHealing = 0;
    private readonly List<UsageItemInfo> _currentReceipt = new();
    private TimeSpan _lastHealing = TimeSpan.Zero;

    private StatModifier<ValueType> _statModifierHealth = null;
    private StatModifier<ValueType> _statModifierStamina = null;
    private StatModifier<ValueType> _statModifierHeat = null;


    public override void Init(Player player)
    {
        base.Init(player);
        _countHealing = 0;
        _lastHealing = TimeSpan.Zero;
    }

    public void CheckOnRemove(UsageItemInfo usageItemInfo)
    {
        if ((GameTime.Time - _lastHealing).TotalHours < _minHour)
            return;

        // Проверка учавствует ли предмет в крафте
        bool contains = false;
        foreach (var receipt in _healingReceipts)
        {
            if (receipt.items.Contains(usageItemInfo.item))
            {
                contains = true;
                break;
            }
        }

        if (!contains)
            return;

        // Удаляем все предметы которые были использованы больше чем 30 минут назад
        _currentReceipt.RemoveAll(v => (GameTime.Time - v.timeUsed).TotalMinutes > 30f);

        // Если предмет уже использовался то заменить старый на новый
        int idx = _currentReceipt.FindIndex(v => v.item == usageItemInfo.item);
        if (idx < 0)
            _currentReceipt.Add(usageItemInfo);
        else
            _currentReceipt[idx] = usageItemInfo;

        // Проверка: выполнены ли условия кравта
        contains = false;
        foreach (var receipt in _healingReceipts)
        {
            var set = new HashSet<InventoryItem>(_currentReceipt.Select(v => v.item));
            if (receipt.items.All(set.Contains))
            {
                contains = true;
                break;
            }
        }

        if (!contains)
            return;

        // Если крафт выполнен позже положенного, то сбрасываем счётчик
        if ((GameTime.Time - _lastHealing).TotalHours > _maxHour)
            _countHealing = 0;

        _countHealing++;
        _lastHealing = GameTime.Time;
        _currentReceipt.Clear();

        if (_countHealing >= _requiredCountHealing)
            OnRemove();
    }

    public override void OnApply()
    {
        IsActive = true;

        _statModifierHealth = new(0, ValueType.ChangeRate, value =>
        {
            return value - _player.Parameters.Health.BaseMax * 1;
        }, $"{GetType().Name}");

        _statModifierStamina = new(0, ValueType.ChangeRate, value =>
        {
            return value - _player.Parameters.Stamina.BaseMax * 0.1f;
        }, $"{GetType().Name}");

        _statModifierHeat = new(0, ValueType.ChangeRate, value =>
        {
            return value - _player.Parameters.Heat.BaseMax * 0.1f;
        }, $"{GetType().Name}");

        _player.Parameters.Health.Mediator.AddModifier(_statModifierHealth);
        _player.Parameters.Stamina.Mediator.AddModifier(_statModifierStamina);
        _player.Parameters.Heat.Mediator.AddModifier(_statModifierHeat);

        _player.StatusEffectsManeger.OnChangedLastUsedItem += CheckOnRemove;
    }

    public override void OnRemove()
    {
        if (!IsActive)
            return;

        IsActive = false;

        _countHealing = 0;

        _statModifierHealth.Dispose();
        _statModifierHealth = null;

        _statModifierStamina.Dispose();
        _statModifierStamina = null;

        _statModifierHeat.Dispose();
        _statModifierHeat = null;

        _player.StatusEffectsManeger.OnChangedLastUsedItem -= CheckOnRemove;
        _player = null;
    }

    public override void Dispose()
        => OnRemove();

    [Serializable]
    private class Receipt
    {
        public List<InventoryItem> items;
    }
}
